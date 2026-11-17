using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// Runs an area: knows its enemies, where the player comes back to, and what the storm does. Resting or falling puts
    /// every enemy back. While a fight is on, the storm now and then picks one of the enemies in it, marks the ground
    /// around it, and strikes: the enemy is charged by the bolt and the player is hurt if standing too close, so a
    /// marked enemy is one to step away from, or to kill before the bolt arrives.
    /// </summary>
    public class AreaDirector : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private Enemy[] enemies = new Enemy[0];
        [SerializeField] private Transform startPoint;
        [SerializeField] private MeshFilter strikeOutline;
        [SerializeField] private MeshFilter strikeFill;

        public float StormStrikeMin = 8f;
        public float StormStrikeMax = 15f;
        /// <summary>Seconds the ground is marked before the bolt lands.</summary>
        public float StrikeWarning = 1.3f;
        public float StrikeRadius = 2.8f;
        public float StrikeDamage = 18f;
        public Color StrikeColour = new Color(0.55f, 0.75f, 1f, 1f);

        /// <summary>Set by a land's <see cref="LandReturn"/>: an elite once beaten is not brought back by a rest or a fall. Off, as in the castle, it returns.</summary>
        [System.NonSerialized] public bool ElitesStayFallen;
        /// <summary>Where a fall returns the player to now: the last fire rested at, or the start.</summary>
        public Transform ReturnPoint => respawn;

        public int StormStrikes { get; private set; }
        public int EnemiesAlive
        {
            get
            {
                int alive = 0;
                foreach (var enemy in enemies)
                    if (enemy != null && !enemy.IsDead) alive++;
                return alive;
            }
        }
        public Enemy[] Enemies => enemies;

        private static readonly int MarkerColour = Shader.PropertyToID("_Color");

        private PlayerDefence defence;
        private Transform respawn;
        private Renderer outlineRenderer;
        private Renderer fillRenderer;
        private MaterialPropertyBlock block;
        private Mesh disc;
        private Enemy marked;
        private float untilStrike;
        private float warningLeft;
        private PlayerUpgrades upgrades;
        private bool[] paid;

        public void Configure(PlayerController playerToServe, Enemy[] enemiesHere, Transform start, MeshFilter outline, MeshFilter fill)
        {
            player = playerToServe;
            enemies = enemiesHere;
            startPoint = start;
            strikeOutline = outline;
            strikeFill = fill;
        }

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            respawn = startPoint;
            disc = MarkerShapes.Sector(StrikeRadius, 360f);
            if (strikeOutline != null)
            {
                strikeOutline.sharedMesh = disc;
                outlineRenderer = strikeOutline.GetComponent<Renderer>();
            }
            if (strikeFill != null)
            {
                strikeFill.sharedMesh = disc;
                fillRenderer = strikeFill.GetComponent<Renderer>();
            }
            ShowMark(false);
            untilStrike = Random.Range(StormStrikeMin, StormStrikeMax);
        }

        private void Start()
        {
            if (player == null) return;
            defence = player.GetComponent<PlayerDefence>();
            upgrades = player.GetComponent<PlayerUpgrades>();
            if (defence != null) defence.Revived += OnRevived;
            paid = new bool[enemies.Length];
            foreach (var enemy in enemies)
                if (enemy != null) enemy.Died += OnEnemyDied;
        }

        // Each of the fallen gives up its ember once; killing it again after a rest gives nothing more.
        private void OnEnemyDied(Enemy who)
        {
            int index = System.Array.IndexOf(enemies, who);
            if (index < 0 || paid[index] || upgrades == null) return;
            paid[index] = true;
            int embers = who.Kind.Embers;
            upgrades.Earn(embers);
            // Paid now; the body will show them leaving when it burns away.
            who.CarryEmbers(upgrades.transform, embers);
            Vfx.Play(VfxKind.Embers, who.transform.position + Vector3.up, Vector3.up, 1.2f);
        }

        private void OnDestroy()
        {
            if (defence != null) defence.Revived -= OnRevived;
            foreach (var enemy in enemies)
                if (enemy != null) enemy.Died -= OnEnemyDied;
            if (disc != null) Destroy(disc);
        }

        /// <summary>The player rests here: this becomes where they come back to, they are made whole, and the area's enemies return.</summary>
        public void Rest(Transform at)
        {
            respawn = at;
            if (defence != null) defence.Restore();
            RestoreEnemies();
        }

        private void OnRevived()
        {
            if (respawn != null) player.Teleport(respawn.position, respawn.rotation);
            RestoreEnemies();
        }

        private void RestoreEnemies()
        {
            foreach (var enemy in enemies)
                // A god stays fallen; an elite too, in a land that says so.
                if (enemy != null && ReturnRule.ComesBack(enemy.Kind.IsBoss, enemy.Kind.IsElite, enemy.IsDead, ElitesStayFallen)) enemy.Restore();
            marked = null;
            ShowMark(false);
            untilStrike = Random.Range(StormStrikeMin, StormStrikeMax);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (marked != null)
            {
                // The mark follows the enemy: the bolt is drawn to it, not to a spot.
                warningLeft -= deltaTime;
                if (marked.IsDead || !marked.gameObject.activeInHierarchy)
                {
                    marked = null;
                    ShowMark(false);
                    return;
                }
                Vector3 centre = marked.transform.position;
                float shown = 1f - Mathf.Clamp01(warningLeft / StrikeWarning);
                if (strikeOutline != null) strikeOutline.transform.position = centre + Vector3.up * 0.05f;
                if (strikeFill != null)
                {
                    strikeFill.transform.position = centre + Vector3.up * 0.07f;
                    strikeFill.transform.localScale = new Vector3(shown, 1f, shown);
                }
                if (warningLeft <= 0f) Land(centre);
                return;
            }

            untilStrike -= deltaTime;
            if (untilStrike > 0f) return;
            untilStrike = Random.Range(StormStrikeMin, StormStrikeMax);
            marked = PickAware();
            if (marked == null) return;
            warningLeft = StrikeWarning;
            ShowMark(true);
        }

        private void Land(Vector3 centre)
        {
            StormStrikes++;
            LightningBolts.Strike(centre, 1f);
            marked.Charge();
            marked = null;
            ShowMark(false);
            if (player == null || defence == null) return;
            Vector3 offset = player.transform.position - centre;
            offset.y = 0f;
            if (offset.magnitude > StrikeRadius) return;
            // Lightning from the sky cannot be blocked; only being elsewhere, or a roll through it, saves the player.
            defence.TakeHit(new Hit(StrikeDamage, 0f, offset.sqrMagnitude > 0.01f ? offset.normalized : Vector3.forward, false, true, centre));
        }

        // One of the enemies that is in the fight, chosen at random without building a list.
        private Enemy PickAware()
        {
            int aware = 0;
            foreach (var enemy in enemies)
                if (MayBeMarked(enemy)) aware++;
            if (aware == 0) return null;
            int pick = Random.Range(0, aware);
            foreach (var enemy in enemies)
            {
                if (!MayBeMarked(enemy)) continue;
                if (pick == 0) return enemy;
                pick--;
            }
            return null;
        }

        // In the fight, not charged already, and of a kind the storm is let at.
        private static bool MayBeMarked(Enemy enemy) =>
            enemy != null && enemy.IsAware && !enemy.IsCharged && enemy.Kind.DrawsStorm;

        private void ShowMark(bool shown)
        {
            Paint(outlineRenderer, shown, StrikeColour, MarkerShapes.OutlineAlpha);
            Paint(fillRenderer, shown, MarkerShapes.Growing(StrikeColour), MarkerShapes.GrowingAlpha);
        }

        private void Paint(Renderer markRenderer, bool shown, Color colour, float alpha)
        {
            if (markRenderer == null) return;
            markRenderer.enabled = shown;
            if (!shown) return;
            colour.a = alpha;
            block.SetColor(MarkerColour, colour);
            markRenderer.SetPropertyBlock(block);
        }
    }
}

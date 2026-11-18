using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// The rule of a land where a fall returns the player to the last rest fire lit (the scene has no RunEnder, so
    /// <see cref="AreaDirector"/> already puts the player back and the ordinary enemies on their feet). This adds the
    /// land's own terms: its elite stays dead once beaten, and the unspent embers carried are lost. A scene without this
    /// component behaves as it always did. It also lets enemies far from the player stop animating, since a land is
    /// several hundred metres long and its people are all in one scene.
    /// </summary>
    public class LandReturn : MonoBehaviour
    {
        [SerializeField] private AreaDirector director;
        [SerializeField] private PlayerDefence defence;

        /// <summary>The cost of a fall proposed in WORLD.md: the embers carried and not yet spent are lost. Switch off to keep them.</summary>
        public bool LoseCarriedEmbers = true;
        /// <summary>The land's elite, once beaten, is not brought back by a rest or a fall.</summary>
        public bool ElitesStayFallen = true;
        /// <summary>Enemies farther than this that have not noticed anyone stop animating until the player comes near. Nothing or less: none sleep.</summary>
        public float SleepBeyond = 55f;
        /// <summary>
        /// The owner's rule for this land (fire_rework.md, section 3): the embers lost on a fall go to the enemies killed
        /// since the last rest, who carry them back glowing and give them up on their next death; a second fall loses
        /// what is still waiting. Off, a fall's embers are simply gone.
        /// </summary>
        public bool EmbersReturnWithTheDead = true;

        /// <summary>Who carries what after a fall.</summary>
        public EmberDebt Debt { get; private set; } = new EmberDebt(0);
        public int Reclaimed { get; private set; }
        public int Forfeited => Debt.Forfeited;

        public int Falls { get; private set; }
        public int EmbersLost { get; private set; }
        public int Asleep { get; private set; }

        private Animator[] animators;
        private float untilLook;
        private float untilMote;
        private PlayerUpgrades upgrades;
        [SerializeField] private Clover.UI.LandHud hud;

        public void Configure(AreaDirector areaDirector, PlayerDefence playerDefence)
        {
            director = areaDirector;
            defence = playerDefence;
        }

        /// <summary>The HUD line that says how many embers are waiting to be reclaimed.</summary>
        public void ConfigureHud(Clover.UI.LandHud landHud) => hud = landHud;

        private void Awake()
        {
            if (director != null) director.ElitesStayFallen = ElitesStayFallen;
        }

        private void Start()
        {
            if (defence != null)
            {
                defence.Revived += OnRevived;
                upgrades = defence.GetComponent<PlayerUpgrades>();
            }
            if (director == null) return;
            director.Rested += OnRested;
            Debt = new EmberDebt(director.Enemies.Length);
            foreach (var enemy in director.Enemies)
                if (enemy != null) enemy.Died += OnEnemyDied;
            animators = new Animator[director.Enemies.Length];
            for (int i = 0; i < animators.Length; i++)
                if (director.Enemies[i] != null) animators[i] = director.Enemies[i].GetComponentInChildren<Animator>();
        }

        private void OnDestroy()
        {
            if (defence != null) defence.Revived -= OnRevived;
            if (director == null) return;
            director.Rested -= OnRested;
            foreach (var enemy in director.Enemies)
                if (enemy != null) enemy.Died -= OnEnemyDied;
        }

        private void OnRested() => Debt.Rested();

        private void OnEnemyDied(Enemy who)
        {
            int index = System.Array.IndexOf(director.Enemies, who);
            if (index < 0) return;
            if (ReturnRule.ComesBack(who.Kind.IsBoss, who.Kind.IsElite, true, ElitesStayFallen)) Debt.Killed(index);
            int share = Debt.Reclaim(index);
            if (share <= 0 || upgrades == null) return;
            // Its share comes back as embers always do: paid now, seen flying to the player as the body burns away.
            Reclaimed += share;
            upgrades.Earn(share);
            who.CarryEmbers(upgrades.transform, share);
            Vfx.Play(VfxKind.Burst, who.transform.position + Vector3.up * 1.2f, Vector3.up, 0.8f);
            if (defence != null) defence.Callout(share == 1 ? "1 Shard reclaimed" : share + " Shards reclaimed");
        }

        private void OnRevived()
        {
            Falls++;
            int kept = ReturnRule.EmbersAfterFall(RunState.Points, LoseCarriedEmbers, PlaytestMode.On);
            // The playtest's rule: a fall costs nothing, so no Shards are lost or left with the dead.
            if (kept == RunState.Points && PlaytestMode.On) return;
            int lost = RunState.Points - kept;
            int waiting = Debt.Waiting;
            if (EmbersReturnWithTheDead)
            {
                int gone = Debt.Fall(lost);
                if (lost > 0)
                {
                    EmbersLost += lost;
                    RunState.Points = kept;
                }
                if (defence != null && (lost > 0 || waiting > 0))
                    defence.Callout(Debt.Waiting > 0 ? (gone > 0 ? gone + " Shards gone. The fallen carry " + Debt.Waiting : "The fallen carry your Shards")
                        : gone == 1 ? "1 Shard lost" : gone + " Shards lost");
                return;
            }
            if (lost <= 0) return;
            EmbersLost += lost;
            RunState.Points = kept;
            if (defence != null) defence.Callout(lost == 1 ? "1 Shard lost" : lost + " Shards lost");
        }

        private void Update()
        {
            if (hud != null) hud.ShowReclaim(Debt.Waiting);
            if (Debt.Waiting > 0 && director != null)
            {
                untilMote -= Time.deltaTime;
                if (untilMote <= 0f)
                {
                    // Each carrier shines with an ember mote over its heart.
                    untilMote = 0.12f;
                    var carriers = director.Enemies;
                    for (int i = 0; i < carriers.Length; i++)
                    {
                        var enemy = carriers[i];
                        if (enemy == null || enemy.IsDead || !enemy.gameObject.activeInHierarchy || !Debt.Carries(i)) continue;
                        Vector3 heart = enemy.transform.position + Vector3.up * 1.35f;
                        Vfx.Play(VfxKind.TintPuff, heart + Random.insideUnitSphere * 0.2f, Vector3.up, 0.35f, new Color(1f, 0.55f, 0.15f, 1f));
                        if (Random.value < 0.3f) Vfx.Play(VfxKind.Embers, heart, Vector3.up, 0.25f);
                    }
                }
            }
            if (animators == null || defence == null) return;
            untilLook -= Time.deltaTime;
            if (untilLook > 0f) return;
            untilLook = 0.5f;
            Vector3 centre = defence.transform.position;
            float limit = SleepBeyond * SleepBeyond;
            int asleep = 0;
            for (int i = 0; i < animators.Length; i++)
            {
                var enemy = director.Enemies[i];
                if (enemy == null || animators[i] == null) continue;
                bool far = SleepBeyond > 0f && !enemy.IsAware && (enemy.transform.position - centre).sqrMagnitude > limit;
                if (far) asleep++;
                // Far off it holds the pose it had; near or in a fight it animates whether it is seen or not, as elsewhere.
                animators[i].cullingMode = far ? AnimatorCullingMode.CullCompletely : AnimatorCullingMode.AlwaysAnimate;
            }
            Asleep = asleep;
        }
    }
}

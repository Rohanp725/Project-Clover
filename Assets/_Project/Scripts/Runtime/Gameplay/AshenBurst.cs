using Clover.Core;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// What makes the ashen what it is (fire_rework.md, section 1). Its white-hot core shows through the ash shell and
    /// brightens as it swells; the swell's red ring is the attack's own mark. When the swell's blow falls it bursts:
    /// a ring of burning ash that hurts everything standing in it, the player and its own side alike, wakes and
    /// lights any wick it reaches, and leaves the ashen gone. Killed before that, it bursts where it fell after a
    /// short delay, its ring drawn on the ground meanwhile, so a player can kill it beside other enemies on purpose.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class AshenBurst : MonoBehaviour
    {
        [SerializeField] private Transform core;
        [SerializeField] private Renderer coreRenderer;
        [SerializeField] private Light coreLight;
        [SerializeField] private Transform model;
        [SerializeField] private MeshFilter ringOutline;
        [SerializeField] private MeshFilter ringFill;

        [ColorUsage(true, true)] public Color CoreCold = new Color(2.2f, 1.1f, 0.35f, 0.75f);
        [ColorUsage(true, true)] public Color CoreHot = new Color(9f, 7.5f, 5f, 1f);
        public Color AshColour = new Color(0.62f, 0.6f, 0.57f, 1f);
        public Color RingColour = new Color(1f, 0.08f, 0.05f, 1f);
        public float CoreSize = 0.55f;
        public float SwollenSize = 1.5f;

        public int Bursts { get; private set; }
        public int EnemiesHurt { get; private set; }
        public int WicksLit { get; private set; }
        public bool PlayerHurt { get; private set; }
        /// <summary>Where it last burst.</summary>
        public Vector3 BurstAt { get; private set; }
        /// <summary>True while it lies dead with its burst still to come.</summary>
        public bool Pending => pendingLeft > 0f;
        public float Swell { get; private set; }
        /// <summary>The names of the enemies its last burst struck, for a probe to read.</summary>
        public string LastStruck { get; private set; } = "";

        private static readonly Collider[] caught = new Collider[32];
        private static readonly IDamageable[] struck = new IDamageable[32];
        private static readonly int ColourId = Shader.PropertyToID("_Color");

        private Enemy enemy;
        private Renderer[] body;
        private MaterialPropertyBlock block;
        private GroundMark ring;
        private Mesh ringShape;
        private bool burst;
        private float pendingLeft;
        private float untilFlake;
        private Vector3 modelHome;
        private Vector3 fellAt;
        private bool fromFall;
        private Quaternion coreFacing;
        private float coreHeight;

        public void Configure(Transform coreToShow, Renderer coreGlow, Light coreLamp, Transform bodyModel, MeshFilter outline, MeshFilter fill)
        {
            core = coreToShow;
            coreRenderer = coreGlow;
            coreLight = coreLamp;
            model = bodyModel;
            ringOutline = outline;
            ringFill = fill;
        }

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            block = new MaterialPropertyBlock();
            body = model != null ? model.GetComponentsInChildren<Renderer>(true) : new Renderer[0];
            if (model != null) modelHome = model.localPosition;
            if (core != null)
            {
                coreFacing = core.rotation;
                coreHeight = core.localPosition.y;
            }
            if (ringOutline != null && ringFill != null)
            {
                ringShape = MarkerShapes.Sector(enemy.Kind != null ? enemy.Kind.BurstRadius : 3.2f, 360f);
                ring = new GroundMark(ringOutline, ringFill, ringShape, block);
                ring.Hide();
            }
        }

        private void OnEnable()
        {
            enemy.AttackStruck += OnAttackStruck;
            enemy.Died += OnDied;
        }

        private void OnDisable()
        {
            enemy.AttackStruck -= OnAttackStruck;
            enemy.Died -= OnDied;
        }

        private void OnDestroy()
        {
            if (ringShape != null) Destroy(ringShape);
        }

        private void OnAttackStruck(Enemy who, EnemyAttack attack)
        {
            if (!attack.Bursts || burst) return;
            Burst();
            // The burst is its death: it is gone with it, and the body is not left standing.
            enemy.Kill();
        }

        private void OnDied(Enemy who)
        {
            if (burst || enemy.Kind.BurstsOnDeath <= 0f) return;
            pendingLeft = enemy.Kind.BurstsOnDeath;
            fellAt = transform.position;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (!enemy.IsDead && burst)
            {
                // Stood up again by a rest or a fall: whole, and its body seen again.
                burst = false;
                pendingLeft = 0f;
                ShowBody(true);
            }
            if (pendingLeft > 0f)
            {
                pendingLeft -= deltaTime;
                float near = 1f - Mathf.Clamp01(pendingLeft / Mathf.Max(0.01f, enemy.Kind.BurstsOnDeath));
                if (ring != null) ring.Show(fellAt, Quaternion.identity, near, RingColour, false);
                // Fallen, the core flickers faster and faster as the shell gives.
                Glow(Mathf.Repeat(Time.time * (6f + 18f * near), 1f) < 0.5f ? 1f : 0.55f, near);
                Spit(near, deltaTime);
                if (pendingLeft <= 0f)
                {
                    pendingLeft = 0f;
                    if (ring != null) ring.Hide();
                    fromFall = true;
                    Burst();
                    fromFall = false;
                }
                return;
            }
            if (enemy.IsDead)
            {
                if (core != null && core.gameObject.activeSelf && !burst) core.gameObject.SetActive(false);
                return;
            }
            var attack = enemy.Brain.Current;
            bool swelling = attack != null && attack.Bursts && enemy.Brain.State == EnemyState.Telegraph;
            Swell = swelling ? enemy.Brain.TelegraphProgress : 0f;
            Glow(swelling ? 0.35f + 0.65f * Swell : 0.3f + 0.08f * Mathf.Sin(Time.time * 3.1f), Swell);
            if (swelling) Spit(Swell, deltaTime);
            // It hisses and shakes as it swells.
            if (model != null)
                model.localPosition = swelling ? modelHome + new Vector3(Mathf.Sin(Time.time * 61f), 0f, Mathf.Cos(Time.time * 47f)) * (0.03f * Swell) : modelHome;
        }

        private void Glow(float heat, float swell)
        {
            if (core == null) return;
            if (!core.gameObject.activeSelf) core.gameObject.SetActive(true);
            // It faces the camera whichever way the body turns, and sits on the camera side of the chest so the shell does not hide it.
            core.SetPositionAndRotation(transform.position + Vector3.up * coreHeight - coreFacing * Vector3.forward * 0.42f, coreFacing);
            core.localScale = Vector3.one * Mathf.Lerp(CoreSize, CoreSize * SwollenSize, swell);
            if (coreRenderer != null)
            {
                block.Clear();
                block.SetColor(ColourId, Color.Lerp(CoreCold, CoreHot, heat));
                coreRenderer.SetPropertyBlock(block);
            }
            if (coreLight != null)
            {
                coreLight.intensity = Mathf.Lerp(0.6f, 9f, heat * heat);
                coreLight.range = Mathf.Lerp(2.5f, 5f, swell);
            }
        }

        // Grey flakes of the shell and white sparks of the core thrown off as the cracks open.
        private void Spit(float swell, float deltaTime)
        {
            untilFlake -= deltaTime;
            if (untilFlake > 0f) return;
            untilFlake = Mathf.Lerp(0.16f, 0.04f, swell);
            Vector3 chest = core != null ? core.position : transform.position + Vector3.up * 1.2f;
            Vfx.Play(VfxKind.Ash, chest + Random.insideUnitSphere * 0.3f, Vector3.up, 0.6f + swell, AshColour);
            if (swell > 0.3f) Vfx.Play(VfxKind.TintSpark, chest, Vector3.up, 0.15f + 0.3f * swell, new Color(1f, 0.85f, 0.55f, 1f));
        }

        private void ShowBody(bool shown)
        {
            foreach (var bodyRenderer in body)
                if (bodyRenderer != null) bodyRenderer.enabled = shown;
            if (core != null) core.gameObject.SetActive(shown);
        }

        /// <summary>Bursts now, wherever it is: a ring of burning ash that hurts everything in it and lights any wick it reaches.</summary>
        public void Burst()
        {
            if (burst) return;
            burst = true;
            Bursts++;
            PlayerHurt = false;
            // Killed early, it bursts where it fell, whatever has moved the body since.
            Vector3 at = fromFall ? fellAt : transform.position;
            BurstAt = at;
            float radius = enemy.Kind.BurstRadius;
            if (model != null) model.localPosition = modelHome;
            ShowBody(false);

            // What it looks like: a white flash at the core, a ring of grey ash rolling out along the ground with fire
            // in it, flakes of the shell thrown up all round its edge, and embers left hanging.
            Vfx.Play(VfxKind.Burst, at + Vector3.up * 1.1f, Vector3.up, 2.2f);
            Vfx.Play(VfxKind.TintSpark, at + Vector3.up * 1.1f, Vector3.up, 2.5f, new Color(1f, 0.8f, 0.45f, 1f));
            Vfx.Play(VfxKind.TintRing, at + Vector3.up * 0.25f, Vector3.up, radius / 2.4f, AshColour);
            Vfx.Play(VfxKind.Ring, at + Vector3.up * 0.3f, Vector3.up, radius / 3f);
            Vfx.Play(VfxKind.Fire, at + Vector3.up * 0.5f, Vector3.up, 2.4f);
            Vfx.Play(VfxKind.Embers, at + Vector3.up * 0.9f, Vector3.up, 2.5f);
            for (int i = 0; i < 16; i++)
            {
                float angle = i * Mathf.PI * 2f / 16f;
                var edge = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                Vfx.Play(VfxKind.Ash, at + edge * (radius * 0.8f) + Vector3.up * 0.3f, Vector3.up, 1.4f, AshColour);
                if (i % 2 == 0) Vfx.Play(VfxKind.Fire, at + edge * (radius * 0.65f) + Vector3.up * 0.2f, Vector3.up, 0.6f);
                if (i % 4 == 0) Vfx.Play(VfxKind.Dust, at + edge * radius, edge);
            }
            if (coreLight != null)
            {
                coreLight.intensity = 0f;
            }

            // What it does: everything with a body in the ring is struck, each once, and every wick in it is lit.
            int count = Physics.OverlapSphereNonAlloc(at + Vector3.up * 0.8f, radius, caught, ~0, QueryTriggerInteraction.Ignore);
            int found = 0;
            LastStruck = "";
            for (int i = 0; i < count; i++)
            {
                var target = caught[i].GetComponentInParent<IDamageable>();
                if (target == null || ReferenceEquals(target, enemy)) continue;
                bool seen = false;
                for (int j = 0; j < found; j++)
                    if (ReferenceEquals(struck[j], target)) seen = true;
                if (seen || found >= struck.Length) continue;
                struck[found++] = target;
                var where = (target as Component) != null ? ((Component)target).transform.position : at;
                Vector3 away = where - at;
                away.y = 0f;
                away = away.sqrMagnitude > 0.01f ? away.normalized : Vector3.forward;
                var other = target as Enemy;
                if (other != null)
                {
                    if (other.IsDead) continue;
                    EnemiesHurt++;
                    LastStruck = LastStruck.Length == 0 ? other.name : LastStruck + ", " + other.name;
                    // A wick is not consumed by fire: it catches, and is only scorched.
                    var flame = other.GetComponent<WickFlame>();
                    if (flame != null && flame.Light()) WicksLit++;
                    other.TakeHit(new Hit(enemy.Kind.BurstEnemyDamage * (flame != null ? 0.25f : 1f), 30f, away, false, true, at, flame != null ? 0f : 3f, flame != null ? 0 : 1));
                    continue;
                }
                var outcome = target.TakeHit(new Hit(enemy.Kind.BurstDamage, 0f, away, false, true, at));
                if (outcome == HitOutcome.Landed && target is Clover.Player.PlayerDefence) PlayerHurt = true;
            }
            WicksLit += WickFlame.LightWithin(at, radius);
            for (int i = 0; i < found; i++) struck[i] = null;
        }
    }
}

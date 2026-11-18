using Clover.Core;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// What the blows of the two spearmen of the Fire land look like (fire_boss_visuals.md, section 2.1 and 2.2), all of it
    /// fire since the owner's review (2026-11-17), not light: a
    /// thrust leaves a trail of flame down its lane, drawn tip-first and cooling to smoke tail-first; a haft skimming the
    /// floor raises sand along the rim it swept; a circle of fire leaves a ring of fire and sand thrown out at its edge. It
    /// listens to the enemy's own blow, so the enemy's code does not grow; the streaks are a pool the builder made.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class SpearLook : MonoBehaviour
    {
        [SerializeField] private Renderer[] streaks = new Renderer[0];
        /// <summary>Shafts of noon: crossed pairs of tall quads, the even one and the odd one after it.</summary>
        [SerializeField] private Renderer[] shafts = new Renderer[0];
        [ColorUsage(true, true)] public Color ShaftColour = new Color(1f, 1f, 1f, 1f);
        public float ShaftHeight = 16f, ShaftThin = 0.3f, ShaftFoot = 1.2f, ShaftSlam = 0.1f, ShaftCollapse = 0.25f;
        /// <summary>The planted spear's ring of fire: tongues of flame standing round the edge that bites.</summary>
        [SerializeField] private Renderer[] tongues = new Renderer[0];
        [ColorUsage(true, true)] public Color TongueColour = new Color(1f, 1f, 1f, 1f);
        public float TongueRise = 0.15f, TongueSink = 0.4f, TongueBeat = 0.5f;
        /// <summary>
        /// The planted spear's fire as fire (the owner's review, 2026-11-18): a field of low, dense flames over charred,
        /// cracking floor inside the circle and a tall wall of fierce flame round its edge with smoke rising off it. Without
        /// it (a scene built before) the ring is the old tongues.
        /// </summary>
        [SerializeField] private ScorchedGround plantFire;
        /// <summary>How low the flames inside the ring stand beside the wall's, how long the wall takes to rise, and how long the char lies after.</summary>
        public float FieldFlames = 0.55f, WallRise = 0.3f, CharAfter = 1.6f;
        private float untilWallSmoke;
        /// <summary>A multiplier on the streak's own fire (its material is flame, not light): white leaves it as it is.</summary>
        [ColorUsage(true, true)] public Color StreakColour = new Color(1f, 1f, 1f, 1f);
        /// <summary>True for the god: his streaks are wider and his rings sun white at the heart.</summary>
        public bool Godly;
        // Wide enough to read as a trail of flame at the court's camera, not a line of light.
        public float StreakHeight = 1.1f, StreakWidth = 0.6f, HeavyWidth = 0.85f, GrowSeconds = 0.06f, FadeSeconds = 0.26f;

        private static readonly int ColourId = Shader.PropertyToID("_Color");
        private static readonly Color Gold = new Color(1f, 0.82f, 0.62f, 1f);
        private static readonly Color Smoke = new Color(0.25f, 0.22f, 0.2f, 0.85f);
        private static readonly Color Sand = new Color(1f, 0.88f, 0.64f, 1f);
        private static readonly Color SunWhite = new Color(1f, 0.92f, 0.78f, 1f);

        private Enemy enemy;
        private MaterialPropertyBlock block;
        // Shafts: where each stands, how long since it slammed down (-1 when not slamming), how wide it slams, and
        // the pair over the circle being warned now (-1 when none) with the attack it warns of.
        private Vector3[] shaftAt;
        private float[] shaftAge, shaftWide;
        private bool[] shaftSlammed;
        private int warning = -1, nextShaft;
        private EnemyAttack warned;
        // The ring of tongues: its middle, its radius, how many stand, and how long it has stood of how long it burns.
        private Vector3 ringAt;
        private float ringRadius, ringAge = -1f, ringLife;
        private int ringCount;
        private Transform eye;
        private Vector3[] tail, tip;
        private float[] age, width;
        private int next;

        public void Configure(Renderer[] streakPool, Color colour, bool godly)
        {
            streaks = streakPool;
            StreakColour = colour;
            Godly = godly;
        }

        public void ConfigureShafts(Renderer[] crossedPairs) => shafts = crossedPairs;

        public void ConfigureTongues(Renderer[] flames) => tongues = flames;

        public void ConfigurePlantFire(ScorchedGround fire) => plantFire = fire;

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            block = new MaterialPropertyBlock();
            tail = new Vector3[streaks.Length];
            tip = new Vector3[streaks.Length];
            age = new float[streaks.Length];
            width = new float[streaks.Length];
            for (int i = 0; i < streaks.Length; i++)
            {
                age[i] = 999f;
                if (streaks[i] != null) streaks[i].enabled = false;
            }
            int pairs = shafts.Length / 2;
            shaftAt = new Vector3[pairs];
            shaftAge = new float[pairs];
            shaftWide = new float[pairs];
            shaftSlammed = new bool[pairs];
            for (int i = 0; i < pairs; i++) shaftAge[i] = -1f;
            foreach (var shaft in shafts)
                if (shaft != null) shaft.enabled = false;
            foreach (var tongue in tongues)
                if (tongue != null) tongue.enabled = false;
            // The ring's own fire is drawn here; the circle's random puffs of flame are not needed.
            if (plantFire != null) enemy.BurnDrawn = true;
            var main = Camera.main;
            eye = main != null ? main.transform : null;
        }

        private void OnEnable()
        {
            if (enemy != null) enemy.AttackStruck += OnStruck;
        }

        private void OnDisable()
        {
            if (enemy != null) enemy.AttackStruck -= OnStruck;
            ringAge = -1f;
            if (plantFire != null) plantFire.HideAll();
            foreach (var tongue in tongues)
                if (tongue != null) tongue.enabled = false;
            for (int i = 0; i < streaks.Length; i++)
            {
                age[i] = 999f;
                if (streaks[i] != null) streaks[i].enabled = false;
            }
        }

        private void OnStruck(Enemy who, EnemyAttack attack)
        {
            if (attack == null || who.IsDead) return;
            Vector3 forward = Flat(transform.forward);
            if (attack.IsCircle)
            {
                Vector3 centre = attack.Ranged || attack.Leap ? who.StrikePoint : transform.position;
                centre.y = transform.position.y;
                // The shaft over the circle slams down to its full width, then collapses to a line and is gone.
                if (attack.Ranged && warning >= 0)
                {
                    shaftAge[warning] = 0f;
                    shaftWide[warning] = attack.Radius * 2f;
                    warning = -1;
                    warned = null;
                }
                // Sand thrown out at the edge that bites, and for the god's noon a splash of fire where it fell.
                int puffs = Mathf.Clamp(Mathf.RoundToInt(attack.Radius * 2.4f), 6, 14);
                for (int i = 0; i < puffs; i++)
                {
                    float turn = i * Mathf.PI * 2f / puffs;
                    var way = new Vector3(Mathf.Sin(turn), 0f, Mathf.Cos(turn));
                    Vfx.Play(VfxKind.Dust, centre + way * attack.Radius + Vector3.up * 0.1f, way, 1f, Sand);
                }
                if (attack.Ranged && Godly)
                {
                    // The fire that fell from the sky splashes and burns where it struck, and smokes.
                    Vfx.Play(VfxKind.Impact, centre + Vector3.up * 0.8f, Vector3.up, 1.2f, SunWhite);
                    Vfx.Play(VfxKind.Fire, centre + Vector3.up * 0.3f, Vector3.up, 2.5f);
                    Vfx.Play(VfxKind.Smoke, centre + Vector3.up * 1.2f, Vector3.up, 1.6f, Smoke);
                }
                // The planted spear: fire stands up round the edge of the circle for as long as it burns.
                if (!attack.Ranged && attack.BurnSeconds > 0f && (tongues.Length > 0 || plantFire != null))
                {
                    ringAt = centre;
                    ringRadius = attack.Radius;
                    ringLife = attack.BurnSeconds;
                    ringAge = 0f;
                    ringCount = Mathf.Min(tongues.Length, Mathf.RoundToInt(attack.Radius * 4f));
                }
                if (!attack.Fiery) Vfx.Play(VfxKind.Ring, centre + Vector3.up * 0.3f, Vector3.up, 1f, Gold);
                return;
            }
            if (attack.IsLine)
            {
                // The ray is drawn as a beam of its own; every other lane is a thrust or a throw down it.
                if (attack.Beams || attack.Name.EndsWith("_ray")) return;
                Vector3 start = attack.PassThrough ? who.LungeFrom : transform.position;
                Vector3 along = attack.PassThrough ? Flat(who.LungeForward) : forward;
                bool heavy = attack.Fiery || attack.Reach > 6f;
                Streak(start + along * 0.6f, start + along * attack.Reach, heavy ? HeavyWidth : StreakWidth);
                Vector3 end = start + along * attack.Reach + Vector3.up * StreakHeight;
                Vfx.Play(VfxKind.Embers, end, along, 0.6f, Gold);
                Vfx.Play(VfxKind.Smoke, end, Vector3.up, 0.35f, Smoke);
                if (Godly && heavy) Vfx.Play(VfxKind.Glint, end, along, 1f, SunWhite);
                if (attack.Lunge >= 1.2f) Vfx.Play(VfxKind.Dust, transform.position + Vector3.up * 0.1f, -along, 1.2f, Sand);
                return;
            }
            // A sweep: the haft skims the floor, and sand rises along the rim it swept.
            float half = attack.ArcDegrees * 0.5f;
            for (int i = 0; i < 5; i++)
            {
                float degrees = Mathf.Lerp(-half, half, i / 4f);
                Vector3 way = Quaternion.Euler(0f, degrees, 0f) * forward;
                Vfx.Play(VfxKind.Dust, transform.position + way * (attack.Reach - 0.25f) + Vector3.up * 0.1f, way, 0.8f, Sand);
            }
        }

        private void Streak(Vector3 from, Vector3 to, float wide)
        {
            if (streaks.Length == 0) return;
            int slot = next;
            next = (next + 1) % streaks.Length;
            tail[slot] = new Vector3(from.x, transform.position.y + StreakHeight, from.z);
            tip[slot] = new Vector3(to.x, transform.position.y + StreakHeight, to.z);
            age[slot] = 0f;
            width[slot] = wide;
            if (streaks[slot] != null) streaks[slot].enabled = true;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            DrawShafts(deltaTime);
            DrawTongues(deltaTime);
            for (int i = 0; i < streaks.Length; i++)
            {
                if (age[i] > GrowSeconds + FadeSeconds) continue;
                age[i] += deltaTime;
                var shown = streaks[i];
                if (shown == null) continue;
                if (age[i] > GrowSeconds + FadeSeconds)
                {
                    shown.enabled = false;
                    continue;
                }
                // Drawn tip-first, then eaten from the tail.
                float grown = Mathf.Clamp01(age[i] / GrowSeconds);
                float eaten = Mathf.Clamp01((age[i] - GrowSeconds) / FadeSeconds);
                Vector3 from = Vector3.Lerp(tail[i], tip[i], eaten * 0.85f);
                Vector3 to = Vector3.Lerp(tail[i], tip[i], grown);
                Vector3 line = to - from;
                float length = line.magnitude;
                if (length < 0.05f)
                {
                    shown.enabled = false;
                    continue;
                }
                Vector3 along = line / length;
                shown.transform.SetPositionAndRotation((from + to) * 0.5f, Quaternion.LookRotation(Vector3.up, Vector3.Cross(Vector3.up, along)));
                shown.transform.localScale = new Vector3(length, width[i] * (1f - eaten * 0.5f), 1f);
                block.SetColor(ColourId, StreakColour * (1f - eaten * eaten));
                shown.SetPropertyBlock(block);
            }
        }

        // Shafts of noon (fire_boss_visuals.md 2.2): from the moment a sun-shaft's circle is drawn a thin column of light
        // stands over it from the sky, widening as the circle fills, never wider at its foot than ShaftFoot so the red
        // fill is not washed orange; at the strike it slams to the circle's width, then collapses and is gone.
        private void DrawShafts(float deltaTime)
        {
            if (shaftAt == null || shaftAt.Length == 0) return;
            var brain = enemy.Brain;
            var current = brain.Current;
            bool telling = !enemy.IsDead && current != null && current.Ranged && current.IsCircle && brain.State == EnemyState.Telegraph;
            if (telling && warned != current)
            {
                warned = current;
                warning = nextShaft;
                nextShaft = (nextShaft + 1) % shaftAt.Length;
                shaftAge[warning] = -1f;
                shaftAt[warning] = enemy.StrikePoint;
            }
            if (!telling && warning >= 0 && shaftAge[warning] < 0f)
            {
                Place(warning, 0f, 0f);
                warning = -1;
                warned = null;
            }
            for (int i = 0; i < shaftAt.Length; i++)
            {
                if (i == warning && telling)
                {
                    float progress = brain.TelegraphProgress;
                    Place(i, Mathf.Lerp(ShaftThin, ShaftFoot, progress), Mathf.Lerp(0.35f, 0.7f, progress));
                    continue;
                }
                if (shaftAge[i] < 0f) continue;
                shaftAge[i] += deltaTime;
                float age = shaftAge[i];
                if (age < ShaftSlam) Place(i, shaftWide[i], 1.15f);
                else if (age < ShaftSlam + ShaftCollapse)
                {
                    float t = (age - ShaftSlam) / ShaftCollapse;
                    Place(i, Mathf.Lerp(shaftWide[i] * 0.5f, 0.2f, t), 1f - t);
                }
                else
                {
                    shaftAge[i] = -1f;
                    Place(i, 0f, 0f);
                }
            }
        }

        // Fire_boss_visuals.md 2.1 and 2.2, the plant: the tongues rise out of the floor round the rim in 0.15 s, stand
        // pulsing on the burn's beat, and as the ring dies sink one after another round it.
        private void DrawTongues(float deltaTime)
        {
            if (plantFire != null)
            {
                DrawPlantFire(deltaTime);
                return;
            }
            if (ringAge < 0f) return;
            ringAge += deltaTime;
            if (ringAge > ringLife + TongueSink)
            {
                ringAge = -1f;
                foreach (var tongue in tongues)
                    if (tongue != null) tongue.enabled = false;
                return;
            }
            Quaternion facing = eye != null ? eye.rotation : Quaternion.identity;
            float beat = 1f + 0.15f * Mathf.Sin(ringAge * Mathf.PI * 2f / TongueBeat);
            for (int i = 0; i < tongues.Length; i++)
            {
                var tongue = tongues[i];
                if (tongue == null) continue;
                if (i >= ringCount)
                {
                    if (tongue.enabled) tongue.enabled = false;
                    continue;
                }
                float turn = i * Mathf.PI * 2f / ringCount;
                // Each sinks in its turn round the ring over the last of the burn.
                float sinkFrom = ringLife - TongueSink * 2f + TongueSink * i / ringCount;
                float up = Mathf.Clamp01(ringAge / TongueRise) * (1f - Mathf.Clamp01((ringAge - sinkFrom) / TongueSink));
                bool on = up > 0.01f;
                if (tongue.enabled != on) tongue.enabled = on;
                if (!on) continue;
                float tall = (1.4f + 0.4f * Mathf.Sin(i * 2.3f)) * beat * up;
                Vector3 foot = ringAt + new Vector3(Mathf.Sin(turn), 0f, Mathf.Cos(turn)) * ringRadius;
                tongue.transform.SetPositionAndRotation(foot + facing * Vector3.up * (tall * 0.5f), facing);
                tongue.transform.localScale = new Vector3(0.8f + 0.2f * Mathf.Cos(i * 1.7f), tall, 1f);
                block.SetColor(ColourId, TongueColour);
                tongue.SetPropertyBlock(block);
            }
        }

        // The planted spear's fire: the wall rises out of the floor round the edge in a moment, one tongue after
        // another; inside it the floor chars and cracks and low flames stand thick on it; as the burn ends the wall sinks
        // round the ring and the field dies from the edge in, leaving char that fades.
        private void DrawPlantFire(float deltaTime)
        {
            if (ringAge < 0f) return;
            ringAge += deltaTime;
            if (ringAge > ringLife + CharAfter)
            {
                ringAge = -1f;
                plantFire.HideAll();
                return;
            }
            float rise = Mathf.Clamp01(ringAge / WallRise);
            float heat = Mathf.Clamp01((ringLife - ringAge) / 1.2f);
            float fade = ringAge <= ringLife ? 1f : 1f - (ringAge - ringLife) / CharAfter;
            plantFire.Ring(0, ringAt, ringRadius, heat, rise);
            plantFire.Disc(0, ringAt, ringRadius, heat, fade, FieldFlames);
            if (heat <= 0.05f || !Every(ref untilWallSmoke, 0.07f, deltaTime)) return;
            // Smoke rolls up off the wall.
            float turn = Random.value * Mathf.PI * 2f;
            Vector3 on = ringAt + new Vector3(Mathf.Sin(turn), 0f, Mathf.Cos(turn)) * ringRadius;
            Vfx.Play(VfxKind.Smoke, on + Vector3.up * 2.4f, Vector3.up, 1.2f, Smoke);
            if (Random.value < 0.3f) Vfx.Play(VfxKind.Embers, on + Vector3.up * 1.5f, Vector3.up, 0.8f);
        }

        private static bool Every(ref float clock, float interval, float passed)
        {
            clock -= passed;
            if (clock > 0f) return false;
            clock = clock < -interval ? interval : clock + interval;
            return true;
        }

        private void Place(int pair, float width, float strength)
        {
            for (int k = 0; k < 2; k++)
            {
                var shaft = shafts[pair * 2 + k];
                if (shaft == null) continue;
                bool on = width > 0.01f && strength > 0.01f;
                if (shaft.enabled != on) shaft.enabled = on;
                if (!on) continue;
                Vector3 foot = shaftAt[pair];
                // Its length runs down from the sky to the floor along the quad's width axis; the two face across each other.
                Quaternion turn = k == 0 ? Quaternion.LookRotation(Vector3.forward, Vector3.right) : Quaternion.LookRotation(Vector3.right, Vector3.back);
                shaft.transform.SetPositionAndRotation(new Vector3(foot.x, foot.y + ShaftHeight * 0.5f, foot.z), turn);
                shaft.transform.localScale = new Vector3(ShaftHeight, width, 1f);
                block.SetColor(ColourId, ShaftColour * strength);
                shaft.SetPropertyBlock(block);
            }
        }

        private static Vector3 Flat(Vector3 v)
        {
            v.y = 0f;
            return v.sqrMagnitude > 0.0001f ? v.normalized : Vector3.forward;
        }
    }
}

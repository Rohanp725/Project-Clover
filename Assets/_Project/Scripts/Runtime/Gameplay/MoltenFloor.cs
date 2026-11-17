using Clover.Core;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// What fire leaves on the floor of the Fire land, from a fixed pool so a long fight neither allocates nor floods
    /// the ground: molten gold splashed from a struck gilded (it burns while hot, then cools into a bright puddle that
    /// slows whoever wades it), the burning footprints of a lit wick, and the warden's pour, a low arc of molten gold
    /// that stands as a wall for a few seconds. When the pool is full the oldest mark goes first. One per scene, on
    /// its own object; a scene without one simply has no marks (the calls do nothing).
    /// </summary>
    public class MoltenFloor : MonoBehaviour
    {
        [SerializeField] private Transform[] pieces = new Transform[0];
        [SerializeField] private Clover.Player.PlayerDefence defence;
        [SerializeField] private Clover.Player.PlayerController player;
        [SerializeField] private Enemy[] enemies = new Enemy[0];

        public float HotSeconds = 2.6f;
        public float CoolSeconds = 5f;
        public float FadeSeconds = 0.8f;
        public float BurnDamage = 4f;
        public float BurnEvery = 0.5f;
        /// <summary>What cooling gold does to the pace of whoever wades it.</summary>
        public float CoolingSlow = 0.55f;
        public float FootprintSeconds = 2f;
        public float FootprintDamage = 3f;
        public float WallSeconds = 4.5f;
        [ColorUsage(true, true)] public Color HotEmission = new Color(3.4f, 1.3f, 0.15f);
        [ColorUsage(true, true)] public Color CoolEmission = new Color(0.5f, 0.32f, 0.05f);
        public Color HotBase = new Color(1f, 0.62f, 0.16f);
        public Color CoolBase = new Color(0.95f, 0.72f, 0.22f);
        [ColorUsage(true, true)] public Color FootEmission = new Color(3f, 0.7f, 0.08f);

        private const int Gold = 1, Foot = 2, Wall = 3;
        private int[] kind;
        private float[] age, life, radius;
        private Renderer[] renderers;
        private Collider[] walls;
        private int next;
        private float untilBite, untilPace, untilShow;
        private MaterialPropertyBlock block;

        private static MoltenFloor instance;
        private static readonly int BaseColour = Shader.PropertyToID("_BaseColor");
        private static readonly int Emission = Shader.PropertyToID("_EmissionColor");

        // Domain reload is off: a floor from the last play must not take this one's marks.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        /// <summary>The marks lying on the floor now, of every kind.</summary>
        public static int Count { get; private set; }
        public static int GoldCount { get; private set; }
        public static int WallCount { get; private set; }
        /// <summary>The most marks that can lie at once.</summary>
        public static int Capacity => instance != null ? instance.pieces.Length : 0;
        public static int Splashes { get; private set; }

        public void Configure(Transform[] pooled, Clover.Player.PlayerDefence playerDefence, Clover.Player.PlayerController playerToSlow, Enemy[] enemiesToSlow)
        {
            pieces = pooled;
            defence = playerDefence;
            player = playerToSlow;
            enemies = enemiesToSlow ?? new Enemy[0];
        }

        private void Awake()
        {
            instance = this;
            block = new MaterialPropertyBlock();
            int n = pieces.Length;
            kind = new int[n];
            age = new float[n];
            life = new float[n];
            radius = new float[n];
            renderers = new Renderer[n];
            walls = new Collider[n];
            for (int i = 0; i < n; i++)
            {
                renderers[i] = pieces[i].GetComponent<Renderer>();
                walls[i] = pieces[i].GetComponent<Collider>();
                if (walls[i] != null) walls[i].enabled = false;
                pieces[i].gameObject.SetActive(false);
            }
            Count = GoldCount = WallCount = 0;
            Splashes = 0;
        }

        private void OnDestroy()
        {
            if (instance == this) instance = null;
        }

        /// <summary>Molten gold splashed onto the floor at a point; it cools into a puddle.</summary>
        public static void Splash(Vector3 at, float size = 0.6f)
        {
            if (instance == null) return;
            Splashes++;
            instance.Lay(Gold, at, size, instance.HotSeconds + instance.CoolSeconds + instance.FadeSeconds);
            Vfx.Play(VfxKind.TintSpark, at + Vector3.up * 0.4f, Vector3.up, 0.6f, new Color(1f, 0.72f, 0.2f, 1f));
        }

        /// <summary>A burning footprint, as a lit wick leaves.</summary>
        public static void Footprint(Vector3 at)
        {
            if (instance == null) return;
            instance.Lay(Foot, at, 0.2f, instance.FootprintSeconds);
        }

        /// <summary>
        /// The warden's pour: molten gold laid in an arc round a point, a low wall that blocks the way for some seconds
        /// and then runs flat and cools as any spilled gold does.
        /// </summary>
        public static void PourArc(Vector3 centre, Vector3 forward, float arcRadius, float arcDegrees)
        {
            if (instance == null) return;
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.001f) forward = Vector3.forward;
            forward.Normalize();
            // A piece every 0.8 m along the arc, close enough that nobody slips between.
            int segments = Mathf.Clamp(Mathf.CeilToInt(arcRadius * arcDegrees * Mathf.Deg2Rad / 0.8f) + 1, 3, 12);
            for (int i = 0; i < segments; i++)
            {
                float angle = Mathf.Lerp(-arcDegrees * 0.5f, arcDegrees * 0.5f, i / (float)(segments - 1));
                Vector3 at = centre + Quaternion.AngleAxis(angle, Vector3.up) * forward * arcRadius;
                instance.Lay(Wall, at, 0.55f, instance.WallSeconds);
                Vfx.Play(VfxKind.Fire, at + Vector3.up * 0.3f, Vector3.up, 0.5f);
                Vfx.Play(VfxKind.TintSpark, at + Vector3.up * 0.5f, Vector3.up, 0.4f, new Color(1f, 0.72f, 0.2f, 1f));
            }
        }

        /// <summary>What the floor does to the pace of whoever stands at a point: below one on cooling gold.</summary>
        public static float PaceAt(Vector3 at) => instance != null ? instance.Pace(at) : 1f;

        /// <summary>True for a point where the floor burns now (hot gold, a burning footprint, the foot of the wall).</summary>
        public static bool BurnsAt(Vector3 at) => instance != null && instance.Burns(at);

        private void Lay(int what, Vector3 at, float size, float seconds)
        {
            int n = pieces.Length;
            if (n == 0) return;
            // A free piece if there is one; otherwise the oldest goes.
            int chosen = -1;
            for (int i = 0; i < n && chosen < 0; i++)
            {
                int index = (next + i) % n;
                if (kind[index] == 0) chosen = index;
            }
            if (chosen < 0)
            {
                chosen = 0;
                for (int i = 1; i < n; i++)
                    if (age[i] / life[i] > age[chosen] / life[chosen]) chosen = i;
            }
            next = (chosen + 1) % n;
            if (kind[chosen] == 0) Count++;
            else Forget(chosen);
            kind[chosen] = what;
            if (what == Gold) GoldCount++;
            if (what == Wall) WallCount++;
            age[chosen] = 0f;
            life[chosen] = seconds;
            radius[chosen] = size;
            // On the ground under the point, or at the point where there is none.
            if (Physics.Raycast(at + Vector3.up * 1.5f, Vector3.down, out RaycastHit ground, 4f, ~0, QueryTriggerInteraction.Ignore)
                && (walls[chosen] == null || ground.collider != walls[chosen]))
                at.y = ground.point.y;
            var piece = pieces[chosen];
            piece.position = at + Vector3.up * 0.02f;
            piece.rotation = Quaternion.Euler(0f, Random.Range(0f, 360f), 0f);
            piece.gameObject.SetActive(true);
            if (walls[chosen] != null) walls[chosen].enabled = what == Wall;
            Shape(chosen);
        }

        private void Forget(int index)
        {
            if (kind[index] == Gold) GoldCount--;
            if (kind[index] == Wall) WallCount--;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            bool show = (untilShow -= deltaTime) <= 0f;
            if (show) untilShow = 0.12f;
            for (int i = 0; i < pieces.Length; i++)
            {
                if (kind[i] == 0) continue;
                age[i] += deltaTime;
                if (age[i] >= life[i])
                {
                    Forget(i);
                    kind[i] = 0;
                    Count--;
                    if (walls[i] != null) walls[i].enabled = false;
                    pieces[i].gameObject.SetActive(false);
                    continue;
                }
                if (kind[i] == Wall && age[i] >= WallSeconds - FadeSeconds && walls[i] != null && walls[i].enabled) walls[i].enabled = false;
                Shape(i);
                // Hot gold smokes and spits now and then; the wall more.
                if (show && Hot(i) && Random.value < (kind[i] == Wall ? 0.5f : 0.18f))
                    Vfx.Play(kind[i] == Foot ? VfxKind.Fire : VfxKind.Embers, pieces[i].position + Vector3.up * 0.15f, Vector3.up, kind[i] == Foot ? 0.12f : 0.2f);
            }
            if (player != null)
            {
                player.GroundFactor = Pace(player.transform.position);
                untilBite -= deltaTime;
                if (untilBite <= 0f)
                {
                    untilBite = BurnEvery;
                    float bite = Bite(player.transform.position);
                    if (bite > 0f && defence != null && !defence.IsDead && defence.Scorch(bite) > 0f)
                        Vfx.Play(VfxKind.Fire, player.transform.position + Vector3.up * 0.3f, Vector3.up, 0.4f);
                }
            }
            untilPace -= deltaTime;
            if (untilPace <= 0f)
            {
                untilPace = 0.25f;
                // The gilded wade their own gold; everyone else is slowed by it.
                foreach (var enemy in enemies)
                    if (enemy != null && enemy.Kind != null && enemy.Kind.Prefix != "gilded" && enemy.Kind.Prefix != "champion")
                        enemy.GroundPace = Count > 0 ? Pace(enemy.transform.position) : 1f;
            }
        }

        private bool Hot(int i) => kind[i] == Foot || kind[i] == Wall ? age[i] < life[i] - FadeSeconds : age[i] < HotSeconds;

        private float Pace(Vector3 at)
        {
            if (Count == 0) return 1f;
            for (int i = 0; i < pieces.Length; i++)
            {
                if (kind[i] != Gold || age[i] < HotSeconds || age[i] > life[i] - FadeSeconds) continue;
                if (Within(i, at)) return CoolingSlow;
            }
            return 1f;
        }

        private bool Burns(Vector3 at) => Bite(at) > 0f;

        private float Bite(Vector3 at)
        {
            if (Count == 0) return 0f;
            float most = 0f;
            for (int i = 0; i < pieces.Length; i++)
            {
                if (kind[i] == 0 || !Hot(i) || !Within(i, at)) continue;
                most = Mathf.Max(most, kind[i] == Foot ? FootprintDamage : BurnDamage);
            }
            return most;
        }

        private bool Within(int i, Vector3 at)
        {
            Vector3 offset = at - pieces[i].position;
            float reach = radius[i] + (kind[i] == Wall ? 0.5f : 0.25f);
            return offset.x * offset.x + offset.z * offset.z <= reach * reach && Mathf.Abs(offset.y) < 1.2f;
        }

        // Each mark's size and colour for its age: gold splashes out, glows, cools to bright gold and shrinks away;
        // the wall rises, stands and runs flat; a footprint glows and goes out.
        private void Shape(int i)
        {
            float t = age[i];
            float r = radius[i];
            float fade = Mathf.Clamp01((life[i] - t) / FadeSeconds);
            Vector3 size;
            Color baseColour, glow;
            if (kind[i] == Wall)
            {
                float rise = Mathf.Clamp01(t / 0.18f);
                float height = Mathf.Lerp(0.05f, 1.1f, rise) * Mathf.Lerp(0.12f, 1f, fade);
                size = new Vector3(r * 2.2f, height, r * 2.2f);
                baseColour = HotBase;
                glow = HotEmission * (0.6f + 0.4f * fade);
            }
            else if (kind[i] == Foot)
            {
                size = new Vector3(r * 1.3f, 0.04f, r * 2.2f) * Mathf.Lerp(0.3f, 1f, fade);
                baseColour = HotBase;
                glow = FootEmission * fade;
            }
            else
            {
                float spread = Mathf.Clamp01(t / 0.15f);
                float cool = Mathf.Clamp01((t - HotSeconds) / 1.2f);
                size = new Vector3(r * 2f, Mathf.Lerp(0.14f, 0.06f, spread), r * 2f) * (Mathf.Lerp(0.3f, 1f, spread) * Mathf.Lerp(0.2f, 1f, fade));
                baseColour = Color.Lerp(HotBase, CoolBase, cool);
                glow = Color.Lerp(HotEmission, CoolEmission, cool);
            }
            pieces[i].localScale = size;
            if (renderers[i] == null) return;
            block.Clear();
            block.SetColor(BaseColour, baseColour);
            block.SetColor(Emission, glow);
            renderers[i].SetPropertyBlock(block);
        }
    }
}

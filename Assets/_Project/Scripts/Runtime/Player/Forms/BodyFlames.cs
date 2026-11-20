using Clover.Gameplay;
using UnityEngine;
using UnityEngine.Rendering;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Burn on a body, drawn as real flame (the owner's review of 2026-11-20: the Burn was "a overlayed staircase of
    /// flames upward"; the flames should flicker "and look like real flame", coloured as fire). The same technique as the
    /// burning ground's real fire (clover_flame_field.shader): many thin tongues, each its own height, width, lean and
    /// time, standing up toward the camera, their edges licked by noise and churned from below, flickering in height and
    /// swaying at the tip, banded from a smoky fringe through deep red and orange to a yellow heart. Here they are rooted
    /// all over the body, shins to shoulders, and follow it.
    ///
    /// Whoever draws a burning body calls <see cref="Keep"/> each frame it burns; the fire catches over a quarter of a
    /// second, grows with the Burn's depth, and dies down by itself once it is no longer kept, so nothing pops in or out.
    /// A fixed pool made when the scene starts; nothing is made or allocated while it burns.
    /// </summary>
    public class BodyFlames : MonoBehaviour
    {
        /// <summary>How many bodies can burn at once; a further one takes the dimmest.</summary>
        public const int Most = 16;
        /// <summary>Tongues on one body.</summary>
        public const int Tongues = 30;
        /// <summary>Seconds the fire takes to catch, and to die down once it is no longer kept.</summary>
        public const float Catch = 0.25f, DieDown = 0.4f;
        /// <summary>Seconds without being kept before it starts to die down.</summary>
        public const float Linger = 0.2f;
        /// <summary>The flames' height and width in metres before each tongue's own share, and how far round the body they stand.</summary>
        public float FlameHeight = 0.92f, FlameWidth = 0.42f, Girth = 0.3f, Height = 1.45f;

        /// <summary>Bodies burning now, for probes.</summary>
        public static int Burning => instance != null ? instance.burning : 0;

        /// <summary>True when burning bodies are drawn this way: the new effects on and the flame material in the art.</summary>
        public static bool Ready => instance != null && instance.material != null && ElementArt.Ready;

        /// <summary>How hot a Burn of this depth burns (more tongues alight, taller): 1 to 5 stacks.</summary>
        public static float HeatOf(int depth) => Mathf.Clamp(0.62f + 0.12f * depth, 0.7f, 1.05f);

        private static BodyFlames instance;

        private static readonly int HeatId = Shader.PropertyToID("_Heat");
        private static readonly int GrownId = Shader.PropertyToID("_Grown");
        private static readonly int FadeId = Shader.PropertyToID("_Fade");
        private static readonly int HeightId = Shader.PropertyToID("_FlameHeight");
        private static readonly int WidthId = Shader.PropertyToID("_FlameWidth");
        private static readonly int WallId = Shader.PropertyToID("_Wall");
        private static readonly int RiseId = Shader.PropertyToID("_Rise");
        private static readonly int BlackId = Shader.PropertyToID("_Black");
        /// <summary>Seconds a body's fire takes to turn black when the curse takes it (Smoulder), and back.</summary>
        public const float Blackens = 0.5f;

        private Material material;
        private Mesh[] meshes;
        private MeshRenderer[] shown;
        private Transform[] bodies;
        private float[] heat, wanted, keptAt;
        private float[] blackness;
        private bool[] blackWanted;
        private int burning;
        private MaterialPropertyBlock block;

        // Domain reload is off: last play's pool must not be thought here.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        /// <summary>The element art hands over the flame material and the pool is made (once).</summary>
        public void Configure(Material flame)
        {
            if (material != null || flame == null) return;
            material = flame;
            block = new MaterialPropertyBlock();
            // A few scatters, so two burning bodies side by side do not burn alike.
            meshes = new Mesh[4];
            for (int i = 0; i < meshes.Length; i++) meshes[i] = FlameField.Body(Tongues, 31 + i * 17);
            shown = new MeshRenderer[Most];
            bodies = new Transform[Most];
            heat = new float[Most];
            wanted = new float[Most];
            keptAt = new float[Most];
            blackWanted = new bool[Most];
            blackness = new float[Most];
            for (int i = 0; i < Most; i++)
            {
                var part = new GameObject("Body Flames");
                part.transform.SetParent(transform, false);
                part.AddComponent<MeshFilter>().sharedMesh = meshes[i % meshes.Length];
                shown[i] = part.AddComponent<MeshRenderer>();
                shown[i].sharedMaterial = material;
                shown[i].shadowCastingMode = ShadowCastingMode.Off;
                shown[i].receiveShadows = false;
                shown[i].lightProbeUsage = LightProbeUsage.Off;
                shown[i].reflectionProbeUsage = ReflectionProbeUsage.Off;
                shown[i].enabled = false;
            }
        }

        private void OnEnable() => instance = this;

        private void OnDisable()
        {
            if (instance == this) instance = null;
            if (shown == null) return;
            for (int i = 0; i < Most; i++)
            {
                bodies[i] = null;
                heat[i] = 0f;
                if (shown[i] != null) shown[i].enabled = false;
            }
            burning = 0;
        }

        private void OnDestroy()
        {
            if (meshes == null) return;
            foreach (var mesh in meshes)
                if (mesh != null) Destroy(mesh);
        }

        /// <summary>
        /// Keeps a body burning this frame with a Burn <paramref name="depth"/> deep. False when burning bodies are not
        /// drawn this way (the caller draws its old fire).
        /// </summary>
        public static bool Keep(Transform body, int depth) => Keep(body, depth, false);

        /// <summary>
        /// The same, burning black when <paramref name="black"/> (Smoulder, the owner's review of 2026-11-19 and 2026-11-20:
        /// "the flames turning black but still burning"): the same flame, taken by the curse, as long as it is kept so.
        /// </summary>
        public static bool Keep(Transform body, int depth, bool black)
        {
            if (!Ready || body == null) return false;
            // The Blender worker's flame tongues (fire_burn), when they are the Burn chosen (AfflictionArt.BurnStyle).
            if (AfflictionArt.BurnStyle == AfflictionArt.KitLook) return ElementLooks.Keep(ElementLooks.Find(AfflictionArt.BurnKitLook), body);
            return instance.KeepBody(body, depth, black);
        }

        private bool KeepBody(Transform body, int depth, bool black)
        {
            int slot = -1, free = -1, dimmest = -1;
            for (int i = 0; i < Most; i++)
            {
                if (ReferenceEquals(bodies[i], body)) { slot = i; break; }
                // Empty by reference only: a destroyed body's slot is let go in LateUpdate, which counts it.
                if (ReferenceEquals(bodies[i], null)) { if (free < 0) free = i; }
                else if (dimmest < 0 || heat[i] < heat[dimmest]) dimmest = i;
            }
            if (slot < 0)
            {
                slot = free >= 0 ? free : dimmest;
                if (slot < 0) return false;
                if (ReferenceEquals(bodies[slot], null)) burning++;
                bodies[slot] = body;
                heat[slot] = 0f;
                blackness[slot] = black ? 1f : 0f;
            }
            wanted[slot] = HeatOf(depth);
            blackWanted[slot] = black;
            keptAt[slot] = Time.time;
            return true;
        }

        private void LateUpdate()
        {
            if (shown == null) return;
            float deltaTime = Time.deltaTime;
            float now = Time.time;
            for (int i = 0; i < Most; i++)
            {
                var body = bodies[i];
                if (ReferenceEquals(body, null)) continue;
                // A body destroyed while it burned takes its flame with it.
                if (body == null)
                {
                    bodies[i] = null;
                    shown[i].enabled = false;
                    burning--;
                    continue;
                }
                bool kept = now - keptAt[i] <= Linger && body.gameObject.activeInHierarchy;
                float target = kept ? wanted[i] : 0f;
                float rate = (kept ? wanted[i] / Catch : wanted[i] / DieDown) * deltaTime;
                heat[i] = Mathf.MoveTowards(heat[i], target, Mathf.Max(rate, 0.0001f));
                if (!kept && heat[i] <= 0.001f)
                {
                    bodies[i] = null;
                    shown[i].enabled = false;
                    burning--;
                    continue;
                }
                var at = shown[i].transform;
                at.SetPositionAndRotation(body.position, Quaternion.identity);
                at.localScale = new Vector3(Girth, Height, Girth);
                block.Clear();
                block.SetFloat(HeatId, heat[i]);
                block.SetFloat(GrownId, 1f);
                block.SetFloat(FadeId, 1f);
                // Taller as it catches and as it burns deeper; a yellow heart kept only in a deep burn.
                block.SetFloat(HeightId, FlameHeight * Mathf.Lerp(0.5f, 1f, Mathf.Clamp01(heat[i])));
                block.SetFloat(WidthId, FlameWidth);
                block.SetFloat(WallId, Mathf.Clamp01((heat[i] - 0.8f) * 2.5f) * 0.6f);
                block.SetFloat(RiseId, 1f);
                blackness[i] = Mathf.MoveTowards(blackness[i], blackWanted[i] ? 1f : 0f, deltaTime / Blackens);
                block.SetFloat(BlackId, blackness[i]);
                shown[i].SetPropertyBlock(block);
                if (!shown[i].enabled) shown[i].enabled = true;
            }
        }
    }
}

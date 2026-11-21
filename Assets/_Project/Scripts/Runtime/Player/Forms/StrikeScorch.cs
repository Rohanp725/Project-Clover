using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// The char under the flames Fire's infused strike leaves along its cut (the owner's review, 2026-11-21: the flames
    /// burnt on bare stone): the burning ground's own look (clover_scorch.shader, as under the Sun-Bearer's fires), a
    /// near-black wash with ash and ember cracks that glow while it is hot and cool from the edges in, then the char
    /// fades. A few discs kept ready under one holder made the first time one is laid, so a scene needs nothing placed.
    /// </summary>
    public class StrikeScorch : MonoBehaviour
    {
        private const int Count = 16;
        private static readonly int HeatId = Shader.PropertyToID("_Heat");
        private static readonly int GrownId = Shader.PropertyToID("_Grown");
        private static readonly int SizeId = Shader.PropertyToID("_Size");
        private static readonly int SeedId = Shader.PropertyToID("_Seed");
        private static readonly int FadeId = Shader.PropertyToID("_Fade");
        private static readonly int DiscId = Shader.PropertyToID("_Disc");

        private static StrikeScorch instance;
        private static bool missing;

        private MeshRenderer[] shown;
        private float[] age, hot, lasts, radius;
        private int next;
        private Mesh disc;
        private Material material;
        private MaterialPropertyBlock block;

        /// <summary>How many patches of char are showing, for probes.</summary>
        public static int Showing
        {
            get
            {
                if (instance == null) return 0;
                int count = 0;
                foreach (var one in instance.shown) if (one.enabled) count++;
                return count;
            }
        }

        // Domain reload is off: the holder from the last play is gone with its scene.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            missing = false;
        }

        /// <summary>
        /// Lays a disc of char <paramref name="metres"/> round a point: its embers glow for <paramref name="hotSeconds"/>
        /// (as long as the flames on it), then the char fades over <paramref name="coolSeconds"/>.
        /// </summary>
        public static void Lay(Vector3 at, float metres, float hotSeconds, float coolSeconds)
        {
            if (instance == null && !Make()) return;
            instance.Place(at, metres, hotSeconds, coolSeconds);
        }

        private static bool Make()
        {
            if (missing) return false;
            // The shader is in every build through the burning floors' material (m_scorch_ground).
            var shader = Shader.Find("Clover/Scorch");
            if (shader == null)
            {
                missing = true;
                return false;
            }
            var holder = new GameObject("Strike Scorch");
            instance = holder.AddComponent<StrikeScorch>();
            instance.Build(shader);
            return true;
        }

        private void Build(Shader shader)
        {
            material = new Material(shader) { name = "Strike Scorch", enableInstancing = true };
            disc = FlameField.DiscGround();
            block = new MaterialPropertyBlock();
            shown = new MeshRenderer[Count];
            age = new float[Count];
            hot = new float[Count];
            lasts = new float[Count];
            radius = new float[Count];
            for (int i = 0; i < Count; i++)
            {
                var piece = new GameObject("Char " + (i + 1));
                piece.transform.SetParent(transform, false);
                piece.AddComponent<MeshFilter>().sharedMesh = disc;
                var one = piece.AddComponent<MeshRenderer>();
                one.sharedMaterial = material;
                one.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                one.receiveShadows = false;
                one.enabled = false;
                shown[i] = one;
            }
        }

        private void Place(Vector3 at, float metres, float hotSeconds, float coolSeconds)
        {
            int i = next;
            next = (next + 1) % Count;
            age[i] = 0f;
            hot[i] = Mathf.Max(0.05f, hotSeconds);
            lasts[i] = hot[i] + Mathf.Max(0.05f, coolSeconds);
            radius[i] = metres;
            var one = shown[i];
            one.transform.SetPositionAndRotation(at + Vector3.up * 0.06f, Quaternion.Euler(0f, Random.value * 360f, 0f));
            one.transform.localScale = new Vector3(metres, 1f, metres);
            one.enabled = true;
            Show(i);
        }

        private void Update()
        {
            float step = Time.deltaTime;
            for (int i = 0; i < Count; i++)
            {
                if (!shown[i].enabled) continue;
                age[i] += step;
                if (age[i] >= lasts[i]) shown[i].enabled = false;
                else Show(i);
            }
        }

        private void Show(int i)
        {
            // The embers stay hot while the flames stand on them and die at the end, as the flames do; once they are cold
            // the char fades away.
            float through = Mathf.Clamp01(age[i] / hot[i]);
            float heat = 1f - through * through * through;
            float fade = 1f - Mathf.Clamp01((age[i] - hot[i]) / (lasts[i] - hot[i]));
            block.Clear();
            block.SetFloat(HeatId, heat);
            block.SetFloat(GrownId, 1f);
            block.SetVector(SizeId, new Vector4(radius[i], radius[i], 0f, 0f));
            block.SetFloat(SeedId, i * 5.3f + 2f);
            block.SetFloat(FadeId, fade);
            block.SetFloat(DiscId, 1f);
            shown[i].SetPropertyBlock(block);
        }

        private void OnDestroy()
        {
            if (disc != null) Destroy(disc);
            if (material != null) Destroy(material);
            if (instance == this) instance = null;
        }
    }
}

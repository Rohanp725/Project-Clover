using Clover.Gameplay;
using Clover.Player.Forms;
using UnityEngine;
using UnityEngine.Rendering;

namespace Clover.World
{
    /// <summary>
    /// What an element's brazier holds (the owner, 2026-11-19: "The element braziers must contain their element, not show
    /// flames of its colour"): the Blender worker's idle loop for that element (staging/blender/2026-11-20_braziers, built
    /// by Clover > Build > Brazier Looks into an <see cref="ElementLookSet"/>), played round and round on the brazier's
    /// Shown group, so it is there when the element is open and gone when it is not. Fire may burn in the flame field
    /// instead of the kit's tongues (the same real flame a burning body wears). Every piece is made when the brazier wakes;
    /// nothing is made or allocated while it plays, and a brazier far from the camera is not posed.
    /// </summary>
    public class BrazierLook : MonoBehaviour
    {
        [SerializeField] private ElementLookSet set;
        [SerializeField] private string key = "";
        [Tooltip("When set, the bowl burns in this flame-field material in place of the look's own flame tongues.")]
        [SerializeField] private Material flameField;
        [SerializeField] private float bed = 1.27f, flameRadius = 0.46f, flameHeight = 1.15f, flameWidth = 0.42f;
        [Tooltip("Metres from the camera past which the loop is not posed (it holds where it was).")]
        [SerializeField] private float farDistance = 45f;

        private static readonly int HeadId = Shader.PropertyToID("_DrawHead");
        private static readonly int TailId = Shader.PropertyToID("_DrawTail");
        private static readonly int InkId = Shader.PropertyToID("_OutlineWidth");
        private static readonly int InkColourId = Shader.PropertyToID("_OutlineColor");
        private static readonly int ThinId = Shader.PropertyToID("_Thin");
        private static readonly int HeatId = Shader.PropertyToID("_Heat");
        private static readonly int GrownId = Shader.PropertyToID("_Grown");
        private static readonly int FadeId = Shader.PropertyToID("_Fade");
        private static readonly int HeightId = Shader.PropertyToID("_FlameHeight");
        private static readonly int WidthId = Shader.PropertyToID("_FlameWidth");
        private static readonly int WallId = Shader.PropertyToID("_Wall");
        private static readonly int RiseId = Shader.PropertyToID("_Rise");

        private int look = -1;
        private Transform[] frames;
        private MeshRenderer[] shown;
        // The look's own flame tongues, cores and licks, which the flame field takes the place of.
        private bool[] tongues;
        private MaterialPropertyBlock block;
        private GameObject flames;
        private Mesh flameMesh;
        private float phase;
        private bool flameChosen;

        /// <summary>The look this brazier plays (its recipe slug), and the parts it has.</summary>
        public string Key => key;
        public int Parts => frames != null ? frames.Length : 0;

        /// <summary>True while the bowl burns in the flame field rather than the kit's tongues.</summary>
        public bool UsesFlameField => flames != null && flames.activeSelf;

        /// <summary>Set up by the scene builder.</summary>
        public void Configure(ElementLookSet looks, string lookKey, Material flameFieldMaterial)
        {
            set = looks;
            key = lookKey;
            flameField = flameFieldMaterial;
        }

        private void Awake()
        {
            if (set == null) return;
            look = set.Find(key);
            if (look < 0) return;
            block = new MaterialPropertyBlock();
            // Each brazier its own place in the loop, so a row of them does not move as one.
            phase = Mathf.Repeat(transform.position.x * 0.37f + transform.position.z * 0.61f, 10f);
            ref var data = ref set.Looks[look];
            flameChosen = flameField != null;
            frames = new Transform[data.PartCount];
            shown = new MeshRenderer[data.PartCount];
            tongues = new bool[data.PartCount];
            for (int k = 0; k < data.PartCount; k++)
            {
                ref var part = ref set.Parts[data.FirstPart + k];
                var piece = new GameObject("Brazier Piece");
                piece.transform.SetParent(transform, false);
                piece.AddComponent<MeshFilter>().sharedMesh = part.Mesh;
                var renderer = piece.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = part.Material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.receiveShadows = false;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                renderer.enabled = false;
                frames[k] = piece.transform;
                tongues[k] = part.Mesh != null && part.Mesh.name.Contains("vfxe2_flame");
                shown[k] = renderer;
            }
            if (flameField != null) MakeFlames();
        }

        // The flame field in the bowl: tongues spread over the coal bed, each its own height and time.
        private void MakeFlames()
        {
            flameMesh = FlameField.Disc(26, 211);
            flames = new GameObject("Brazier Flames");
            flames.transform.SetParent(transform, false);
            flames.transform.localPosition = new Vector3(0f, bed, 0f);
            flames.transform.localScale = new Vector3(flameRadius, 1f, flameRadius);
            flames.AddComponent<MeshFilter>().sharedMesh = flameMesh;
            var renderer = flames.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = flameField;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            renderer.lightProbeUsage = LightProbeUsage.Off;
            renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
            block.Clear();
            block.SetFloat(HeatId, 1f);
            block.SetFloat(GrownId, 1f);
            block.SetFloat(FadeId, 1f);
            block.SetFloat(HeightId, flameHeight);
            block.SetFloat(WidthId, flameWidth);
            block.SetFloat(WallId, 0f);
            block.SetFloat(RiseId, 1f);
            renderer.SetPropertyBlock(block);
        }

        /// <summary>
        /// Burn the bowl in the flame field (true) or the look's own flame tongues (false), for comparing the two; only a
        /// brazier given a flame-field material can.
        /// </summary>
        public void UseFlameField(bool on)
        {
            if (flames == null) return;
            flameChosen = on;
            flames.SetActive(on);
        }

        private void OnDestroy()
        {
            if (flameMesh != null) Destroy(flameMesh);
        }

        private void LateUpdate()
        {
            if (frames == null) return;
            var view = Camera.main;
            if (view != null && (view.transform.position - transform.position).sqrMagnitude > farDistance * farDistance) return;
            ref var data = ref set.Looks[look];
            float seconds = Mathf.Max(0.1f, data.Seconds);
            float t = Mathf.Repeat(Time.time + phase, seconds);
            for (int k = 0; k < frames.Length; k++)
            {
                ref var part = ref set.Parts[data.FirstPart + k];
                // With the flame field burning, the look's own tongues, cores and licks are left out (its embers and bed stay).
                bool tongue = flameChosen && tongues[k];
                bool visible = !tongue && t >= part.Show && (part.Hide >= 9f || t < part.Hide);
                if (!visible)
                {
                    if (shown[k].enabled) shown[k].enabled = false;
                    continue;
                }
                set.LocalPose(part, t, t, k, out Vector3 position, out Quaternion rotation, out Vector3 scale);
                frames[k].SetLocalPositionAndRotation(position, rotation);
                frames[k].localScale = scale;
                if (part.Kind == ElementLookSet.Ribbon)
                {
                    block.Clear();
                    block.SetFloat(HeadId, part.DrawCount > 0 ? Mathf.Clamp01(set.Sample(part.DrawFirst, part.DrawCount, t, Vector3.one).x) : 1f);
                    block.SetFloat(TailId, part.EraseCount > 0 ? Mathf.Clamp01(set.Sample(part.EraseFirst, part.EraseCount, t, Vector3.zero).x) : 0f);
                    block.SetFloat(InkId, part.InkWidth);
                    block.SetColor(InkColourId, part.InkColour);
                    block.SetFloat(ThinId, part.Thin);
                    shown[k].SetPropertyBlock(block);
                }
                if (!shown[k].enabled) shown[k].enabled = true;
            }
        }
    }
}

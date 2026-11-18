using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// Burning ground drawn as fire, not as a highlighted strip (the owner's review, 2026-11-18): each lane or disc is a
    /// char wash with ember cracks (clover_scorch.shader) and a field of real flames standing on it
    /// (clover_flame_field.shader); a ring is a wall of tall flame. Whoever owns the burning says where each patch lies,
    /// how hot it still is and how far a lane's fire has reached; this places, tints and hides the pieces the builder
    /// made, through one property block, and makes the shared meshes once when it wakes. The flames die from the edges
    /// in as the heat falls, and what is left is char with fading cracks.
    /// </summary>
    public class ScorchedGround : MonoBehaviour
    {
        [SerializeField] private MeshRenderer[] laneGround = new MeshRenderer[0];
        [SerializeField] private MeshRenderer[] laneFlames = new MeshRenderer[0];
        [SerializeField] private MeshRenderer[] discGround = new MeshRenderer[0];
        [SerializeField] private MeshRenderer[] discFlames = new MeshRenderer[0];
        [SerializeField] private MeshRenderer[] ringFlames = new MeshRenderer[0];
        /// <summary>How many flames stand on one lane, one disc and one ring; set by the builder.</summary>
        public int LaneCount = 260, DiscCount = 80, RingCount = 90;
        /// <summary>How tall the flames over a lane and a disc stand, and how wide, in metres before their own shares.</summary>
        public float FlameHeight = 1.35f, FlameWidth = 0.95f;
        public float RingHeight = 2.8f, RingWidth = 1.05f;

        private static readonly int HeatId = Shader.PropertyToID("_Heat");
        private static readonly int GrownId = Shader.PropertyToID("_Grown");
        private static readonly int SizeId = Shader.PropertyToID("_Size");
        private static readonly int SeedId = Shader.PropertyToID("_Seed");
        private static readonly int FadeId = Shader.PropertyToID("_Fade");
        private static readonly int DiscId = Shader.PropertyToID("_Disc");
        private static readonly int HeightId = Shader.PropertyToID("_FlameHeight");
        private static readonly int WidthId = Shader.PropertyToID("_FlameWidth");
        private static readonly int WallId = Shader.PropertyToID("_Wall");
        private static readonly int RiseId = Shader.PropertyToID("_Rise");
        private static readonly int ShadeId = Shader.PropertyToID("_ScorchShade");
        private static readonly int ShadeSizeId = Shader.PropertyToID("_ScorchShadeSize");

        private MaterialPropertyBlock block;
        private Mesh laneMesh, discMesh;
        private Mesh[] laneFlameMeshes = new Mesh[0], discFlameMeshes = new Mesh[0], ringMeshes = new Mesh[0];

        public int Lanes => Mathf.Min(laneGround.Length, laneFlames.Length);
        public int Discs => Mathf.Min(discGround.Length, discFlames.Length);
        public int Rings => ringFlames.Length;

        public void Configure(MeshRenderer[] lanesGround, MeshRenderer[] lanesFlames, MeshRenderer[] discsGround, MeshRenderer[] discsFlames, MeshRenderer[] rings)
        {
            laneGround = lanesGround;
            laneFlames = lanesFlames;
            discGround = discsGround;
            discFlames = discsFlames;
            ringFlames = rings;
        }

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            laneMesh = FlameField.LaneGround();
            discMesh = FlameField.DiscGround();
            // Each patch its own scatter, so two lanes side by side do not burn alike.
            laneFlameMeshes = new Mesh[laneFlames.Length];
            for (int i = 0; i < laneFlames.Length; i++) laneFlameMeshes[i] = FlameField.Lane(LaneCount, 11 + i * 7);
            discFlameMeshes = new Mesh[discFlames.Length];
            for (int i = 0; i < discFlames.Length; i++) discFlameMeshes[i] = FlameField.Disc(DiscCount, 5 + i * 13);
            ringMeshes = new Mesh[ringFlames.Length];
            for (int i = 0; i < ringFlames.Length; i++) ringMeshes[i] = FlameField.Ring(RingCount, 3 + i * 17);
            for (int i = 0; i < laneGround.Length; i++) Fill(laneGround[i], laneMesh);
            for (int i = 0; i < laneFlames.Length; i++) Fill(laneFlames[i], laneFlameMeshes[i]);
            for (int i = 0; i < discGround.Length; i++) Fill(discGround[i], discMesh);
            for (int i = 0; i < discFlames.Length; i++) Fill(discFlames[i], discFlameMeshes[i]);
            for (int i = 0; i < ringFlames.Length; i++) Fill(ringFlames[i], ringMeshes[i]);
        }

        private static void Fill(MeshRenderer shown, Mesh mesh)
        {
            if (shown == null) return;
            var filter = shown.GetComponent<MeshFilter>();
            if (filter != null) filter.sharedMesh = mesh;
            shown.enabled = false;
        }

        private void OnDestroy()
        {
            if (laneMesh != null) Destroy(laneMesh);
            if (discMesh != null) Destroy(discMesh);
            foreach (var mesh in laneFlameMeshes) if (mesh != null) Destroy(mesh);
            foreach (var mesh in discFlameMeshes) if (mesh != null) Destroy(mesh);
            foreach (var mesh in ringMeshes) if (mesh != null) Destroy(mesh);
        }

        private void OnDisable() => HideAll();

        public void HideAll()
        {
            Hide(laneGround);
            Hide(laneFlames);
            Hide(discGround);
            Hide(discFlames);
            Hide(ringFlames);
        }

        private static void Hide(MeshRenderer[] shown)
        {
            foreach (var one in shown)
                if (one != null && one.enabled) one.enabled = false;
        }

        /// <summary>
        /// A burning lane from <paramref name="from"/> along <paramref name="along"/>, <paramref name="length"/> long when
        /// it has all burnt, of which <paramref name="grown"/> (0 to 1) has caught so far. <paramref name="heat"/> is 1
        /// lit and 0 cold (the flames die from the edges in as it falls); <paramref name="fade"/> takes the char away at the end.
        /// </summary>
        public void Lane(int index, Vector3 from, Vector3 along, float width, float length, float grown, float heat, float fade, float flameScale = 1f, bool charred = true)
        {
            if (index < 0 || index >= Lanes || length < 0.05f) return;
            along.y = 0f;
            if (along.sqrMagnitude < 0.0001f) return;
            var turn = Quaternion.LookRotation(along.normalized);
            var scale = new Vector3(width, 1f, length);
            Place(laneGround[index], from + Vector3.up * 0.06f, turn, scale, charred);
            block.Clear();
            block.SetFloat(HeatId, heat);
            block.SetFloat(GrownId, grown);
            block.SetVector(SizeId, new Vector4(width, length, 0f, 0f));
            block.SetFloat(SeedId, index * 3.7f + 1f);
            block.SetFloat(FadeId, fade);
            block.SetFloat(DiscId, 0f);
            laneGround[index].SetPropertyBlock(block);
            bool flames = heat > 0.01f && fade > 0.01f;
            Place(laneFlames[index], flames ? from : Vector3.zero, turn, scale, flames);
            if (!flames) return;
            block.Clear();
            block.SetFloat(HeatId, heat);
            block.SetFloat(GrownId, grown);
            block.SetFloat(FadeId, fade);
            block.SetFloat(HeightId, FlameHeight * flameScale);
            block.SetFloat(WidthId, FlameWidth * Mathf.Lerp(0.8f, 1f, flameScale));
            block.SetFloat(WallId, 0f);
            block.SetFloat(RiseId, 1f);
            laneFlames[index].SetPropertyBlock(block);
        }

        /// <summary>A burning disc of <paramref name="radius"/>: char and cracks, and a field of flames over it; lower flames with a smaller <paramref name="flameScale"/>.</summary>
        public void Disc(int index, Vector3 centre, float radius, float heat, float fade, float flameScale = 1f, bool charred = true)
        {
            if (index < 0 || index >= Discs) return;
            var scale = new Vector3(radius, 1f, radius);
            Place(discGround[index], centre + Vector3.up * 0.06f, Quaternion.identity, scale, charred);
            block.Clear();
            block.SetFloat(HeatId, heat);
            block.SetFloat(GrownId, 1f);
            block.SetVector(SizeId, new Vector4(radius, radius, 0f, 0f));
            block.SetFloat(SeedId, index * 5.3f + 2f);
            block.SetFloat(FadeId, fade);
            block.SetFloat(DiscId, 1f);
            discGround[index].SetPropertyBlock(block);
            bool flames = heat > 0.01f && fade > 0.01f && flameScale > 0.01f;
            Place(discFlames[index], centre, Quaternion.identity, scale, flames);
            if (!flames) return;
            block.Clear();
            block.SetFloat(HeatId, heat);
            block.SetFloat(GrownId, 1f);
            block.SetFloat(FadeId, fade);
            block.SetFloat(HeightId, FlameHeight * flameScale);
            block.SetFloat(WidthId, FlameWidth * Mathf.Lerp(0.75f, 1f, flameScale));
            block.SetFloat(WallId, 0f);
            block.SetFloat(RiseId, 1f);
            discFlames[index].SetPropertyBlock(block);
        }

        /// <summary>
        /// A wall of fierce flame round a circle: <paramref name="rise"/> from 0 to 1 brings it up out of the floor one
        /// tongue after another; as <paramref name="heat"/> falls below a half it sinks again round the ring.
        /// </summary>
        public void Ring(int index, Vector3 centre, float radius, float heat, float rise, float fade = 1f)
        {
            if (index < 0 || index >= Rings) return;
            bool on = heat > 0.01f && rise > 0.01f && fade > 0.01f;
            Place(ringFlames[index], centre, Quaternion.identity, new Vector3(radius, 1f, radius), on);
            if (!on) return;
            block.Clear();
            block.SetFloat(HeatId, heat);
            block.SetFloat(GrownId, 1f);
            block.SetFloat(FadeId, fade);
            block.SetFloat(HeightId, RingHeight);
            block.SetFloat(WidthId, RingWidth);
            block.SetFloat(WallId, 1f);
            block.SetFloat(RiseId, rise);
            ringFlames[index].SetPropertyBlock(block);
        }

        public void HideLane(int index)
        {
            if (index < 0 || index >= Lanes) return;
            if (laneGround[index] != null && laneGround[index].enabled) laneGround[index].enabled = false;
            if (laneFlames[index] != null && laneFlames[index].enabled) laneFlames[index].enabled = false;
        }

        public void HideDisc(int index)
        {
            if (index < 0 || index >= Discs) return;
            if (discGround[index] != null && discGround[index].enabled) discGround[index].enabled = false;
            if (discFlames[index] != null && discFlames[index].enabled) discFlames[index].enabled = false;
        }

        public void HideRing(int index)
        {
            if (index < 0 || index >= Rings) return;
            if (ringFlames[index] != null && ringFlames[index].enabled) ringFlames[index].enabled = false;
        }

        private static void Place(MeshRenderer shown, Vector3 at, Quaternion turn, Vector3 scale, bool on = true)
        {
            if (shown == null) return;
            if (shown.enabled != on) shown.enabled = on;
            if (!on) return;
            shown.transform.SetPositionAndRotation(at, turn);
            // In metres whatever the holder is scaled by (a spearman's body may be drawn larger than life).
            var holder = shown.transform.parent;
            if (holder != null)
            {
                Vector3 by = holder.lossyScale;
                scale = new Vector3(scale.x / Mathf.Max(0.0001f, by.x), scale.y / Mathf.Max(0.0001f, by.y), scale.z / Mathf.Max(0.0001f, by.z));
            }
            shown.transform.localScale = scale;
        }

        /// <summary>
        /// Where the obelisk's shadow lies, for every burning patch at once: under it the embers are cold and the flames
        /// all but out (fire_land.md 6.3, rule 2). Null takes the rule away.
        /// </summary>
        public static void Shade(ShadowHand hand)
        {
            if (hand == null)
            {
                Shader.SetGlobalVector(ShadeSizeId, Vector4.zero);
                return;
            }
            Vector3 centre = hand.Centre, along = hand.Direction;
            Shader.SetGlobalVector(ShadeId, new Vector4(centre.x, centre.z, along.x, along.z));
            Shader.SetGlobalVector(ShadeSizeId, new Vector4(hand.From, hand.To, hand.HalfWidth, 1f));
        }
    }
}

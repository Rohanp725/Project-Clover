using Clover.Player.Forms;
using UnityEngine;
using UnityEngine.Rendering;

namespace Clover.Player
{
    /// <summary>
    /// Bedrock, Earth's surge, as earthen armour growing over the player (the owner, 2026-11-20: "a idea for a rework that
    /// might look much better is to have earthen armor grow over my skin", chosen over the old Bedrock the same day). The
    /// body's toon material closes over in stone from the feet up in five bands (Clover/Toon's _Ice shell drawn as
    /// _IceEarth: warm brown plates, dark seams that glow ochre now and then), and thick faceted stone plates (elements 4's
    /// chunky stone) grow out over the shins, hips, chest and shoulders, rooted on the skeleton's bones so they move with
    /// the body and never float off it. When Bedrock ends the plates crack off and fall and the stone goes. The plates are
    /// made the first time Bedrock is worn and kept; nothing is allocated a frame.
    /// </summary>
    public sealed class StoneArmour
    {
        /// <summary>The stone skin's bands, the seconds between them, when the first plate grows and how far apart the rest are, and how long each takes.</summary>
        public const int Bands = 5;
        public const float BandEvery = 0.1f, FirstPlate = 0.15f, PlateStep = 0.04f, PlateGrow = 0.18f;
        /// <summary>Seconds the plates take to crack off and fall when Bedrock ends, and the stone to go.</summary>
        public const float Shed = 0.35f;

        /// <summary>Where a plate grows: the bone, an offset from it and the way it faces out (the player's own axes: x right, y up, z ahead), and its width, metres.</summary>
        private struct Spot
        {
            public string Bone;
            public Vector3 Offset, Out;
            public float Size;

            public Spot(string bone, Vector3 offset, Vector3 outward, float size)
            {
                Bone = bone;
                Offset = offset;
                Out = outward;
                Size = size;
            }
        }

        // Round 4's sixteen slabs (staging/blender/2026-11-22_round4, Bedrock round 2: "thicker, chunkier plates that visibly
        // change the silhouette at the game camera"): the shins, the thighs, the hip and the back, the chest in three, both
        // upper arms, a big layered pauldron on each shoulder (two slabs, about 0.7 m across together) and a crest at the
        // nape; in the order they grow, from the feet up as the stone climbs. They stand a little off the body.
        private static readonly Spot[] Spots =
        {
            new Spot("shin.L", new Vector3(0f, 0.18f, 0.07f), new Vector3(-0.2f, 0f, 1f), 0.2f),
            new Spot("shin.R", new Vector3(0f, 0.18f, 0.07f), new Vector3(0.2f, 0f, 1f), 0.2f),
            new Spot("thigh.L", new Vector3(-0.09f, 0.06f, 0f), new Vector3(-1f, 0f, 0.25f), 0.24f),
            new Spot("thigh.R", new Vector3(0.09f, 0.06f, 0f), new Vector3(1f, 0f, 0.25f), 0.24f),
            new Spot("spine", new Vector3(0f, 0.02f, 0.13f), new Vector3(0f, -0.1f, 1f), 0.26f),
            new Spot("spine", new Vector3(0f, 0.12f, -0.14f), new Vector3(0f, 0.15f, -1f), 0.3f),
            new Spot("chest", new Vector3(0f, 0.1f, 0.14f), new Vector3(0f, 0.2f, 1f), 0.3f),
            new Spot("chest", new Vector3(-0.13f, 0.04f, 0.12f), new Vector3(-0.55f, 0.2f, 1f), 0.22f),
            new Spot("chest", new Vector3(0.13f, 0.04f, 0.12f), new Vector3(0.55f, 0.2f, 1f), 0.22f),
            new Spot("upperarm.L", new Vector3(-0.05f, 0.1f, 0f), new Vector3(-1f, 0.1f, 0f), 0.2f),
            new Spot("upperarm.R", new Vector3(0.05f, 0.1f, 0f), new Vector3(1f, 0.1f, 0f), 0.2f),
            new Spot("shoulder.L", new Vector3(-0.08f, 0.08f, 0f), new Vector3(-0.6f, 1f, 0f), 0.42f),
            new Spot("shoulder.R", new Vector3(0.08f, 0.08f, 0f), new Vector3(0.6f, 1f, 0f), 0.42f),
            new Spot("shoulder.L", new Vector3(-0.16f, 0.15f, 0f), new Vector3(-0.85f, 1f, 0f), 0.3f),
            new Spot("shoulder.R", new Vector3(0.16f, 0.15f, 0f), new Vector3(0.85f, 1f, 0f), 0.3f),
            new Spot("head", new Vector3(0f, -0.1f, -0.1f), new Vector3(0f, 0.35f, -1f), 0.22f),
        };

        /// <summary>How thick a plate is against the slab's own: round 4's slab carries its 15 cm in its mesh.</summary>
        public const float Thickness = 1f;

        private static readonly int IceId = Shader.PropertyToID("_Ice");
        private static readonly int IceRevealId = Shader.PropertyToID("_IceReveal");
        private static readonly int EarthId = Shader.PropertyToID("_IceEarth");
        private static readonly int BaseId = Shader.PropertyToID("_RevealBase");
        private static readonly int HeightId = Shader.PropertyToID("_RevealHeight");

        private readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        private readonly Transform[] bones = new Transform[Spots.Length];
        private Transform[] plates;
        private Vector3[] full, falling;
        private Transform holder;
        private SkinnedMeshRenderer body;
        private float age, shedAge = Shed, stone, writtenStone = -1f, writtenReveal = -1f;
        private bool on;

        /// <summary>How far the player is armoured now (0 to 1), and how many plates stand on them, for probes.</summary>
        public float Level => stone;
        public int Standing
        {
            get
            {
                if (plates == null || !on) return 0;
                int count = 0;
                for (int i = 0; i < plates.Length; i++)
                    if (plates[i] != null && plates[i].gameObject.activeSelf && plates[i].parent == bones[i]) count++;
                return count;
            }
        }

        /// <summary>The body the armour grows on (the outfit's skinned body) and the player it belongs to.</summary>
        public void Watch(SkinnedMeshRenderer bodyRenderer, Transform player)
        {
            holder = player;
            if (body == bodyRenderer) return;
            if (body != null) Write(0f, 1f, 0f, 0f);
            body = bodyRenderer;
            writtenStone = -1f;
            for (int i = 0; i < bones.Length; i++) bones[i] = null;
            if (body == null) return;
            var all = body.bones;
            for (int s = 0; s < Spots.Length; s++)
                for (int b = 0; b < all.Length; b++)
                    if (all[b] != null && all[b].name == Spots[s].Bone)
                    {
                        bones[s] = all[b];
                        break;
                    }
        }

        /// <summary>Grows the armour while Bedrock is worn and sheds it after, for a body standing at <paramref name="feet"/>.</summary>
        public void Tick(bool wearing, float deltaTime, float feet)
        {
            if (wearing && !on) Begin();
            else if (!wearing && on) End();
            if (on)
            {
                age += deltaTime;
                // Band by band from the feet: each band closes quickly, then the next begins.
                float bands = age / BandEvery;
                float band = Mathf.Floor(bands), share = bands - band;
                float reveal = Mathf.Clamp01((band + 1f - (1f - share) * (1f - share)) / Bands);
                stone = Mathf.Clamp01(age / (BandEvery * Bands));
                GrowPlates();
                WriteIfMoved(1f, reveal, feet);
            }
            else if (shedAge < Shed)
            {
                shedAge += deltaTime;
                stone = Mathf.Clamp01(1f - shedAge / Shed);
                Fall(deltaTime);
                WriteIfMoved(stone, 1f, feet);
            }
            else if (writtenStone != 0f)
            {
                stone = 0f;
                WriteIfMoved(0f, 1f, feet);
            }
        }

        private void Begin()
        {
            on = true;
            age = 0f;
            if (holder == null || !MakePlates()) return;
            var turn = holder.rotation;
            for (int i = 0; i < plates.Length; i++)
            {
                var plate = plates[i];
                if (bones[i] == null)
                {
                    plate.gameObject.SetActive(false);
                    continue;
                }
                // Set where it grows and which way it faces in the player's frame, then rooted on its bone so it moves
                // with the body; its full size is kept as the bone sees it.
                plate.SetParent(holder, false);
                plate.SetPositionAndRotation(bones[i].position + turn * Spots[i].Offset, Quaternion.FromToRotation(Vector3.up, turn * Spots[i].Out.normalized));
                plate.localScale = new Vector3(Spots[i].Size, Spots[i].Size * Thickness, Spots[i].Size) * plateScale;
                plate.SetParent(bones[i], true);
                full[i] = plate.localScale;
                plate.localScale = Vector3.zero;
                plate.gameObject.SetActive(true);
            }
        }

        private void End()
        {
            on = false;
            shedAge = 0f;
            if (plates == null) return;
            // They crack off where they stand and fall, each knocked a little out along the way it faced.
            for (int i = 0; i < plates.Length; i++)
            {
                var plate = plates[i];
                if (!plate.gameObject.activeSelf) continue;
                Vector3 scale = plate.lossyScale;
                plate.SetParent(holder != null ? holder.parent : null, true);
                full[i] = scale;
                falling[i] = plate.up * 0.5f + Vector3.up * 0.3f;
            }
        }

        private void GrowPlates()
        {
            if (plates == null) return;
            for (int i = 0; i < plates.Length; i++)
            {
                if (!plates[i].gameObject.activeSelf) continue;
                float grown = Mathf.Clamp01((age - FirstPlate - i * PlateStep) / PlateGrow);
                // Pushed out with a little overshoot, then held still.
                float y = grown - 1f;
                float push = grown >= 1f ? 1f : 1f + 2.70158f * y * y * y + 1.70158f * y * y;
                plates[i].localScale = full[i] * Mathf.Max(0f, push);
            }
        }

        private void Fall(float deltaTime)
        {
            if (plates == null) return;
            float left = Mathf.Clamp01(1f - shedAge / Shed);
            for (int i = 0; i < plates.Length; i++)
            {
                var plate = plates[i];
                if (!plate.gameObject.activeSelf) continue;
                falling[i].y -= 9.8f * deltaTime;
                plate.position += falling[i] * deltaTime;
                // They crumble as they fall: gone before they reach the floor.
                plate.localScale = full[i] * (left * left);
                if (left <= 0f) plate.gameObject.SetActive(false);
            }
        }

        // The plate mesh's own width, so a spot's size is metres across.
        private float plateScale = 1f;

        // The plates, made once: the built looks' stone plate in the earth solid, inked like the rest of the stone.
        private bool MakePlates()
        {
            if (plates != null) return true;
            if (!ElementLooks.TryArmourPlate(out var mesh, out var material)) return false;
            plateScale = 1f / Mathf.Max(0.01f, Mathf.Max(mesh.bounds.size.x, mesh.bounds.size.z));
            plates = new Transform[Spots.Length];
            full = new Vector3[Spots.Length];
            falling = new Vector3[Spots.Length];
            for (int i = 0; i < Spots.Length; i++)
            {
                var plate = new GameObject("Bedrock Plate");
                plate.transform.SetParent(holder, false);
                plate.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = plate.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = material;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                plate.SetActive(false);
                plates[i] = plate.transform;
            }
            return true;
        }

        /// <summary>Takes the armour off at once (the element put away).</summary>
        public void Clear()
        {
            on = false;
            shedAge = Shed;
            stone = 0f;
            if (plates != null)
                for (int i = 0; i < plates.Length; i++)
                    if (plates[i] != null) plates[i].gameObject.SetActive(false);
            Write(0f, 1f, 0f, 0f);
            writtenStone = 0f;
        }

        private void WriteIfMoved(float amount, float reveal, float feet)
        {
            if (Mathf.Abs(amount - writtenStone) < 0.004f && Mathf.Abs(reveal - writtenReveal) < 0.004f) return;
            float height = body != null ? Mathf.Max(1f, body.bounds.max.y - feet) : 1.85f;
            Write(amount, reveal, feet, height);
            writtenStone = amount;
            writtenReveal = reveal;
        }

        private void Write(float amount, float reveal, float feet, float height)
        {
            if (body == null) return;
            // Read back first, so whatever else the body's block carries (Temper's steel) is kept.
            body.GetPropertyBlock(block);
            block.SetFloat(IceId, amount);
            block.SetFloat(IceRevealId, reveal);
            block.SetFloat(EarthId, amount > 0f ? 1f : 0f);
            if (height > 0f)
            {
                block.SetFloat(BaseId, feet);
                block.SetFloat(HeightId, height);
            }
            body.SetPropertyBlock(block);
        }
    }
}

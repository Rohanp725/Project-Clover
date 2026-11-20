using Clover.Player.Forms;
using UnityEngine;
using UnityEngine.Rendering;

namespace Clover.Player
{
    /// <summary>
    /// Glaze, Ice's surge, as ice grown out of the player (the owner, 2026-11-19: "should grow out of you instead of
    /// floating around you cause its ice"). The body's toon material is frozen over from the feet up (Clover/Toon's _Ice,
    /// the shell Chill puts on an enemy), and hexagonal crystals (the elements2 kit's) snap out of the shoulders, the arms
    /// and the back one after another, rooted on the skeleton's bones so they move with the body. They hold while Glaze
    /// lasts; when it ends the crystals break off and fall, shrinking, and the frost goes. Nothing floats round the player
    /// and nothing lies on the floor. The crystals are made the first time Glaze is worn and kept; nothing is allocated
    /// a frame.
    /// </summary>
    public sealed class GlazeIce
    {
        /// <summary>Seconds the frost takes to climb the body, when the first crystal snaps out and how far apart the rest are, and how long each takes.</summary>
        public const float Climb = 0.3f, FirstCrystal = 0.12f, CrystalStep = 0.035f, CrystalGrow = 0.14f;
        /// <summary>Seconds the crystals take to break off and fall when Glaze ends, and the frost to go.</summary>
        public const float Shed = 0.3f;

        /// <summary>Where a crystal grows: the bone, an offset from it and the way it points (the player's own axes: x right, y up, z ahead), and its length.</summary>
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

        // Shoulders, arms and back, both sides: up and out from the shoulders, out from the arms, back and up from the
        // back, so they stand off the silhouette the camera sees.
        private static readonly Spot[] Spots =
        {
            new Spot("shoulder.L", new Vector3(-0.04f, 0.07f, -0.02f), new Vector3(-0.45f, 1f, -0.25f), 1.7f),
            new Spot("shoulder.L", new Vector3(-0.08f, 0.02f, 0.02f), new Vector3(-1f, 0.55f, 0.1f), 1.2f),
            new Spot("shoulder.R", new Vector3(0.04f, 0.07f, -0.02f), new Vector3(0.45f, 1f, -0.25f), 1.7f),
            new Spot("shoulder.R", new Vector3(0.08f, 0.02f, 0.02f), new Vector3(1f, 0.55f, 0.1f), 1.2f),
            new Spot("upperarm.L", new Vector3(-0.05f, -0.08f, -0.03f), new Vector3(-1f, 0.25f, -0.35f), 1.05f),
            new Spot("upperarm.R", new Vector3(0.05f, -0.08f, -0.03f), new Vector3(1f, 0.25f, -0.35f), 1.05f),
            new Spot("forearm.L", new Vector3(-0.04f, -0.06f, -0.02f), new Vector3(-1f, 0.1f, -0.5f), 0.8f),
            new Spot("forearm.R", new Vector3(0.04f, -0.06f, -0.02f), new Vector3(1f, 0.1f, -0.5f), 0.8f),
            new Spot("chest", new Vector3(0f, 0.12f, -0.14f), new Vector3(0f, 0.65f, -1f), 1.8f),
            new Spot("chest", new Vector3(-0.12f, 0.04f, -0.13f), new Vector3(-0.55f, 0.45f, -1f), 1.3f),
            new Spot("chest", new Vector3(0.12f, 0.04f, -0.13f), new Vector3(0.55f, 0.45f, -1f), 1.3f),
        };

        private static readonly int IceId = Shader.PropertyToID("_Ice");
        private static readonly int IceRevealId = Shader.PropertyToID("_IceReveal");
        private static readonly int BaseId = Shader.PropertyToID("_RevealBase");
        private static readonly int HeightId = Shader.PropertyToID("_RevealHeight");

        private readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        private readonly Transform[] bones = new Transform[Spots.Length];
        private Transform[] crystals;
        private Vector3[] full, falling;
        private Transform holder;
        private SkinnedMeshRenderer body;
        private float age, shedAge = Shed, frost, writtenFrost = -1f, writtenReveal = -1f;
        private bool on;

        /// <summary>How far the player is iced over now (0 to 1), and how many crystals stand out of them, for probes.</summary>
        public float Frost => frost;
        public int Standing
        {
            get
            {
                if (crystals == null || !on) return 0;
                int count = 0;
                for (int i = 0; i < crystals.Length; i++)
                    if (crystals[i] != null && crystals[i].gameObject.activeSelf && crystals[i].parent == bones[i]) count++;
                return count;
            }
        }

        /// <summary>The body the ice grows on (the outfit's skinned body) and the player it belongs to.</summary>
        public void Watch(SkinnedMeshRenderer bodyRenderer, Transform player)
        {
            holder = player;
            if (body == bodyRenderer) return;
            if (body != null) Write(0f, 1f, 0f, 0f);
            body = bodyRenderer;
            writtenFrost = -1f;
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

        /// <summary>Grows the ice while Glaze is worn and sheds it after, for a body standing at <paramref name="feet"/>.</summary>
        public void Tick(bool wearing, float deltaTime, float feet)
        {
            if (wearing && !on) Begin();
            else if (!wearing && on) End();
            if (on)
            {
                age += deltaTime;
                frost = Mathf.Clamp01(age / Climb);
                float reveal = 1f - (1f - frost) * (1f - frost);
                GrowCrystals();
                WriteIfMoved(1f, reveal, feet);
            }
            else if (shedAge < Shed)
            {
                shedAge += deltaTime;
                frost = Mathf.Clamp01(1f - shedAge / Shed);
                Fall(deltaTime);
                WriteIfMoved(frost, 1f, feet);
            }
            else if (writtenFrost != 0f)
            {
                frost = 0f;
                WriteIfMoved(0f, 1f, feet);
            }
        }

        private void Begin()
        {
            on = true;
            age = 0f;
            if (holder == null || !MakeCrystals()) return;
            var turn = holder.rotation;
            for (int i = 0; i < crystals.Length; i++)
            {
                var crystal = crystals[i];
                if (bones[i] == null)
                {
                    crystal.gameObject.SetActive(false);
                    continue;
                }
                // Set where it grows and which way it points in the player's frame, then rooted on its bone so it moves
                // with the body; its full size is kept as the bone sees it.
                crystal.SetParent(holder, false);
                crystal.SetPositionAndRotation(bones[i].position + turn * Spots[i].Offset, Quaternion.FromToRotation(Vector3.up, turn * Spots[i].Out.normalized));
                crystal.localScale = Vector3.one * Spots[i].Size;
                crystal.SetParent(bones[i], true);
                full[i] = crystal.localScale;
                crystal.localScale = Vector3.zero;
                crystal.gameObject.SetActive(true);
            }
        }

        private void End()
        {
            on = false;
            shedAge = 0f;
            if (crystals == null) return;
            // They break off where they stand and fall, each thrown a little out along the way it pointed.
            for (int i = 0; i < crystals.Length; i++)
            {
                var crystal = crystals[i];
                if (!crystal.gameObject.activeSelf) continue;
                Vector3 scale = crystal.lossyScale;
                crystal.SetParent(holder != null ? holder.parent : null, true);
                full[i] = scale;
                falling[i] = crystal.up * 0.6f + Vector3.up * 0.4f;
            }
        }

        private void GrowCrystals()
        {
            if (crystals == null) return;
            for (int i = 0; i < crystals.Length; i++)
            {
                if (!crystals[i].gameObject.activeSelf) continue;
                float grown = Mathf.Clamp01((age - FirstCrystal - i * CrystalStep) / CrystalGrow);
                // Snapped out with a little overshoot, then held still.
                float y = grown - 1f;
                float snap = grown >= 1f ? 1f : 1f + 2.70158f * y * y * y + 1.70158f * y * y;
                crystals[i].localScale = full[i] * Mathf.Max(0f, snap);
            }
        }

        private void Fall(float deltaTime)
        {
            if (crystals == null) return;
            float left = Mathf.Clamp01(1f - shedAge / Shed);
            for (int i = 0; i < crystals.Length; i++)
            {
                var crystal = crystals[i];
                if (!crystal.gameObject.activeSelf) continue;
                falling[i].y -= 9.8f * deltaTime;
                crystal.position += falling[i] * deltaTime;
                // They shatter as they fall: gone well before they reach the floor.
                crystal.localScale = full[i] * (left * left);
                if (left <= 0f) crystal.gameObject.SetActive(false);
            }
        }

        // The crystals, made once: the built looks' crystal mesh in the ice solid, inked like the rest of the ice.
        private bool MakeCrystals()
        {
            if (crystals != null) return true;
            if (!ElementLooks.TryCrystal(out var mesh, out var ice)) return false;
            crystals = new Transform[Spots.Length];
            full = new Vector3[Spots.Length];
            falling = new Vector3[Spots.Length];
            for (int i = 0; i < Spots.Length; i++)
            {
                var crystal = new GameObject("Glaze Crystal");
                crystal.transform.SetParent(holder, false);
                crystal.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = crystal.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = ice;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                renderer.lightProbeUsage = LightProbeUsage.Off;
                renderer.reflectionProbeUsage = ReflectionProbeUsage.Off;
                crystal.SetActive(false);
                crystals[i] = crystal.transform;
            }
            return true;
        }

        /// <summary>Takes the ice off at once (the element put away).</summary>
        public void Clear()
        {
            on = false;
            shedAge = Shed;
            frost = 0f;
            if (crystals != null)
                for (int i = 0; i < crystals.Length; i++)
                    if (crystals[i] != null) crystals[i].gameObject.SetActive(false);
            Write(0f, 1f, 0f, 0f);
            writtenFrost = 0f;
        }

        private void WriteIfMoved(float amount, float reveal, float feet)
        {
            if (Mathf.Abs(amount - writtenFrost) < 0.004f && Mathf.Abs(reveal - writtenReveal) < 0.004f) return;
            float height = body != null ? Mathf.Max(1f, body.bounds.max.y - feet) : 1.85f;
            Write(amount, reveal, feet, height);
            writtenFrost = amount;
            writtenReveal = reveal;
        }

        private void Write(float amount, float reveal, float feet, float height)
        {
            if (body == null) return;
            // Read back first, so whatever else the body's block carries (Temper's steel) is kept.
            body.GetPropertyBlock(block);
            block.SetFloat(IceId, amount);
            block.SetFloat(IceRevealId, reveal);
            if (height > 0f)
            {
                block.SetFloat(BaseId, feet);
                block.SetFloat(HeightId, height);
            }
            body.SetPropertyBlock(block);
        }
    }
}

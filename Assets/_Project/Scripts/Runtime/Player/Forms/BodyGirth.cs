using System.Collections.Generic;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// How far out from its own upright the drawn body of an enemy (or the player) reaches at each height, measured once
    /// from its skinned mesh, so the element looks drawn round the recipe's clay stand-in sit on the real body. The
    /// stand-in was a chunky figure (about 0.42 m out at the chest); the castle's soldiers are drawn far slimmer (about
    /// 0.2 to 0.3 m), and pieces laid on the stand-in's surface stood off them in the air (the owner, 2026-11-20: "the ice
    /// that grow off the person, and most of the reaction stuff is floating off the enemy, and is not on the enemy").
    /// A fixed table of bodies; measuring a body allocates once, nothing is allocated after.
    /// </summary>
    public static class BodyGirth
    {
        /// <summary>Height bands a body is measured in, and the bodies kept measured at once.</summary>
        public const int Bands = 10, Most = 32;

        /// <summary>The recipe's clay stand-in's reach out from its upright: legs, chest, head (by share of its height).</summary>
        public const float StandInLegs = 0.32f, StandInChest = 0.42f, StandInHead = 0.27f;

        /// <summary>The least and most a piece's reach out from the body is scaled by.</summary>
        public const float LeastRatio = 0.4f, MostRatio = 1.25f;

        private static readonly Transform[] bodies = new Transform[Most];
        private static readonly float[] heights = new float[Most];
        private static readonly float[] radii = new float[Most * Bands];
        private static int next;
        private static Mesh baked;
        private static readonly List<Vector3> vertices = new List<Vector3>();
        private static readonly List<float>[] byBand = new List<float>[Bands];

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            System.Array.Clear(bodies, 0, Most);
            System.Array.Clear(heights, 0, Most);
            System.Array.Clear(radii, 0, radii.Length);
            next = 0;
            baked = null;
            vertices.Clear();
        }

        /// <summary>The stand-in's reach out at a share (0 feet, 1 crown) of its height.</summary>
        public static float StandIn(float share) => share < 0.45f ? StandInLegs : share < 0.82f ? StandInChest : StandInHead;

        /// <summary>The body's slot in the table, measuring it the first time; -1 for one with no skinned body to measure.</summary>
        public static int Slot(Transform body)
        {
            if (body == null) return -1;
            for (int i = 0; i < Most; i++)
                if (ReferenceEquals(bodies[i], body)) return heights[i] > 0f ? i : -1;
            int slot = next;
            next = (next + 1) % Most;
            bodies[slot] = body;
            heights[slot] = Measure(body, slot) ? heights[slot] : 0f;
            return heights[slot] > 0f ? slot : -1;
        }

        /// <summary>How far out the body reaches at <paramref name="height"/> metres over its feet.</summary>
        public static float RadiusAt(int slot, float height)
        {
            if (slot < 0 || heights[slot] <= 0f) return StandInChest;
            float band = Mathf.Clamp(height / heights[slot] * Bands - 0.5f, 0f, Bands - 1f);
            int low = Mathf.FloorToInt(band);
            int high = Mathf.Min(Bands - 1, low + 1);
            return Mathf.Lerp(radii[slot * Bands + low], radii[slot * Bands + high], band - low);
        }

        /// <summary>
        /// How much a piece laid on the stand-in at <paramref name="standInHeight"/> metres (its share of the stand-in's
        /// height carried to the real body's) is drawn in toward the real body's upright: its reach over the stand-in's.
        /// </summary>
        public static float Ratio(int slot, float standInHeight, float standInTall)
        {
            if (slot < 0) return 1f;
            float share = Mathf.Clamp01(standInHeight / standInTall);
            return Mathf.Clamp(RadiusAt(slot, share * heights[slot]) / StandIn(share), LeastRatio, MostRatio);
        }

        // The median reach out from the upright through the feet of the body's drawn vertices in each height band (the
        // weapon held and the cloth trailing are fewer than the body's own, so the median is the body).
        private static bool Measure(Transform body, int slot)
        {
            var skin = body.GetComponentInChildren<SkinnedMeshRenderer>();
            if (skin == null || skin.sharedMesh == null) return false;
            if (baked == null) baked = new Mesh { name = "Body Girth" };
            skin.BakeMesh(baked, true);
            baked.GetVertices(vertices);
            if (vertices.Count == 0) return false;
            var from = skin.transform;
            Vector3 feet = body.position;
            float top = 0f;
            for (int i = 0; i < vertices.Count; i++) top = Mathf.Max(top, from.TransformPoint(vertices[i]).y - feet.y);
            if (top < 0.5f) return false;
            for (int b = 0; b < Bands; b++)
            {
                if (byBand[b] == null) byBand[b] = new List<float>(256);
                byBand[b].Clear();
            }
            for (int i = 0; i < vertices.Count; i++)
            {
                Vector3 at = from.TransformPoint(vertices[i]) - feet;
                int b = Mathf.Clamp((int)(at.y / top * Bands), 0, Bands - 1);
                byBand[b].Add(new Vector2(at.x, at.z).magnitude);
            }
            float last = StandInChest;
            for (int b = 0; b < Bands; b++)
            {
                var band = byBand[b];
                if (band.Count > 0)
                {
                    band.Sort();
                    last = band[band.Count / 2];
                }
                radii[slot * Bands + b] = last;
            }
            heights[slot] = top;
            return true;
        }
    }
}

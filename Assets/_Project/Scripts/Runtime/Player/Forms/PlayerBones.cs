using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// The player's bones that a surge's pieces are bound to (round 4: Kindle's flames up the arms, Overdrive's arcs over the
    /// limbs, Fever's sweat and veins), and where each bone lay in the Blender worker's stand-in pose (read from round4.blend,
    /// scene RD, the rdp_ rig: player-local, Unity axes, x right, y up, z ahead, metres from the feet). A piece drawn on the
    /// stand-in is kept at its share along its bone and its offset off it; on the real body the bone's own head and tail
    /// carry it, so it follows the arms, shoulders and body in every pose (ElementLooks, Part.Bone). Bedrock's plates on
    /// the bones (StoneArmour) are the precedent.
    /// </summary>
    public static class PlayerBones
    {
        /// <summary>The bones by index (a part's Bone is the index plus one; 0 is unbound), with the stand-in's head and tail.</summary>
        public static readonly (string name, Vector3 head, Vector3 tail)[] StandIn =
        {
            ("hips", new Vector3(0f, 0.98f, 0f), new Vector3(0f, 1.09f, 0f)),
            ("spine", new Vector3(0f, 1.1f, 0f), new Vector3(0f, 1.28f, 0f)),
            ("chest", new Vector3(0f, 1.28f, 0f), new Vector3(0f, 1.51f, 0f)),
            ("neck", new Vector3(0f, 1.5f, 0f), new Vector3(0f, 1.62f, 0f)),
            ("head", new Vector3(0f, 1.62f, 0f), new Vector3(0f, 1.841f, 0f)),
            ("shoulder.L", new Vector3(-0.045f, 1.47f, 0.021f), new Vector3(-0.181f, 1.48f, 0.085f)),
            ("upperarm.L", new Vector3(-0.181f, 1.48f, 0.085f), new Vector3(-0.238f, 1.172f, 0.169f)),
            ("forearm.L", new Vector3(-0.238f, 1.172f, 0.169f), new Vector3(-0.274f, 0.896f, 0.239f)),
            ("hand.L", new Vector3(-0.274f, 0.896f, 0.239f), new Vector3(-0.314f, 0.621f, 0.31f)),
            ("shoulder.R", new Vector3(0.045f, 1.47f, -0.021f), new Vector3(0.181f, 1.48f, -0.085f)),
            ("upperarm.R", new Vector3(0.181f, 1.48f, -0.085f), new Vector3(0.323f, 1.4f, 0.196f)),
            ("forearm.R", new Vector3(0.323f, 1.4f, 0.196f), new Vector3(0.439f, 1.316f, 0.444f)),
            ("hand.R", new Vector3(0.439f, 1.316f, 0.444f), new Vector3(0.465f, 1.299f, 0.497f)),
            ("thigh.L", new Vector3(-0.105f, 0.98f, 0f), new Vector3(-0.167f, 0.539f, 0.18f)),
            ("shin.L", new Vector3(-0.167f, 0.539f, 0.18f), new Vector3(-0.216f, 0.153f, 0.337f)),
            ("foot.L", new Vector3(-0.216f, 0.153f, 0.337f), new Vector3(-0.212f, 0.185f, 0.756f)),
            ("thigh.R", new Vector3(0.105f, 0.98f, 0f), new Vector3(0.105f, 0.529f, -0.164f)),
            ("shin.R", new Vector3(0.105f, 0.529f, -0.164f), new Vector3(0.1f, 0.207f, -0.434f)),
            ("foot.R", new Vector3(0.1f, 0.207f, -0.434f), new Vector3(0.1f, -0.147f, -0.209f)),
        };

        // The bone whose head is each bone's tail on the real rig (-1: none; the tail is then carried out along the bone's
        // own way at the stand-in's length).
        private static readonly int[] TailBone = { 1, 2, 3, 4, -1, 6, 7, 8, -1, 10, 11, 12, -1, 14, 15, -1, 17, 18, -1 };

        private static readonly Transform[] found = new Transform[19];
        private static Transform holder;
        private static SkinnedMeshRenderer body;

        // Domain reload is off: nothing found in the last play may be thought still there.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            for (int i = 0; i < found.Length; i++) found[i] = null;
            holder = null;
            body = null;
        }

        /// <summary>The nearest stand-in bone to a player-local point: its index, the share along it and the offset off it.</summary>
        public static int Nearest(Vector3 local, out float share, out Vector3 offset)
        {
            int best = 0;
            float bestDistance = float.MaxValue;
            share = 0f;
            offset = Vector3.zero;
            for (int i = 0; i < StandIn.Length; i++)
            {
                Vector3 a = StandIn[i].head, b = StandIn[i].tail;
                Vector3 along = b - a;
                float t = Mathf.Clamp01(Vector3.Dot(local - a, along) / Mathf.Max(0.0001f, along.sqrMagnitude));
                Vector3 on = a + along * t;
                float distance = (local - on).sqrMagnitude;
                if (distance >= bestDistance) continue;
                bestDistance = distance;
                best = i;
                share = t;
                offset = local - on;
            }
            return best;
        }

        /// <summary>Finds the player's bones on its worn body (again only when the body changes, an outfit put on).</summary>
        public static bool Resolve(Transform player, SkinnedMeshRenderer worn)
        {
            if (player == null || worn == null) return false;
            if (ReferenceEquals(holder, player) && ReferenceEquals(body, worn)) return found[0] != null;
            holder = player;
            body = worn;
            for (int i = 0; i < found.Length; i++) found[i] = null;
            var bones = worn.bones;
            for (int i = 0; i < StandIn.Length; i++)
                for (int b = 0; b < bones.Length; b++)
                    if (bones[b] != null && bones[b].name == StandIn[i].name)
                    {
                        found[i] = bones[b];
                        break;
                    }
            return found[0] != null;
        }

        /// <summary>
        /// Where a bone lies on the real body now (world), from its head to its tail, for the player last resolved. False
        /// when that bone is not on the rig.
        /// </summary>
        public static bool Segment(int index, out Vector3 head, out Vector3 tail)
        {
            head = tail = Vector3.zero;
            if (index < 0 || index >= found.Length || found[index] == null) return false;
            head = found[index].position;
            int next = TailBone[index];
            if (next >= 0 && found[next] != null) tail = found[next].position;
            else
            {
                // Carried on along the way it comes in from its parent bone, at the stand-in's length.
                float length = (StandIn[index].tail - StandIn[index].head).magnitude;
                Vector3 way = found[index].parent != null ? head - found[index].parent.position : Vector3.up;
                if (index == 4) way = holder != null ? holder.up : Vector3.up;
                tail = head + (way.sqrMagnitude > 0.0001f ? way.normalized : Vector3.up) * length;
            }
            return true;
        }
    }
}

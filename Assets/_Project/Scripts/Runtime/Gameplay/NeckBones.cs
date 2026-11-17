using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// Bends a neck's chain of bones so that its end meets the back of a skull wherever the head has gone. The neck
    /// keeps the curve it was made with and gives it up only as the head goes further than that curve reaches: each
    /// joint is first moved a share of the way the end has to go, more toward the end, and then the lengths between
    /// joints are put right from both ends. Its root never moves. Nothing is allocated once it has woken.
    /// </summary>
    public class NeckBones : MonoBehaviour
    {
        [SerializeField] private Transform[] bones = new Transform[0];
        [SerializeField] private Transform skull;
        [SerializeField] private Vector3 skullPoint;
        [SerializeField] private float endLength = 1f;

        /// <summary>How far the neck fell short of the skull in the last pose: the metres its last stretch had to be drawn out by.</summary>
        public float Gap { get; private set; }
        /// <summary>The sharpest bend between two neighbouring stretches of the neck in the last pose, in degrees.</summary>
        public float SharpestBend { get; private set; }

        /// <summary>How long the whole neck is as it was made, from its root to the point that meets the skull; nothing until it has woken.</summary>
        public float Length { get; private set; }
        /// <summary>Where the neck's root is now. It does not move within the piece it belongs to.</summary>
        public Vector3 Root => restAt != null ? transform.TransformPoint(restAt[0]) : transform.position;
        /// <summary>Where on the skull the neck's end has to be now.</summary>
        public Vector3 SkullPoint => skull != null ? skull.TransformPoint(skullPoint) : transform.position;

        private Vector3[] restAt;
        private Quaternion[] restTurn;
        private float[] lengths;
        private Vector3[] rest;
        private Vector3[] points;

        /// <summary>The chain root first; the head whose skull it follows; the point on that head, in the head's own space, the neck's end meets; and how long the last bone is.</summary>
        public void Configure(Transform[] chain, Transform head, Vector3 pointOnSkull, float lastBoneLength)
        {
            bones = chain;
            skull = head;
            skullPoint = pointOnSkull;
            endLength = lastBoneLength;
        }

        private void Awake()
        {
            int count = bones.Length;
            if (count < 2) return;
            restAt = new Vector3[count + 1];
            restTurn = new Quaternion[count];
            lengths = new float[count];
            rest = new Vector3[count + 1];
            points = new Vector3[count + 1];
            var back = Quaternion.Inverse(transform.rotation);
            for (int i = 0; i < count; i++)
            {
                restAt[i] = transform.InverseTransformPoint(bones[i].position);
                restTurn[i] = back * bones[i].rotation;
            }
            // The end of the last bone: straight on from it.
            Vector3 last = (bones[count - 1].position - bones[count - 2].position).normalized;
            restAt[count] = transform.InverseTransformPoint(bones[count - 1].position + last * endLength);
            for (int i = 0; i < count; i++) lengths[i] = (transform.TransformPoint(restAt[i + 1]) - transform.TransformPoint(restAt[i])).magnitude;
            Length = 0f;
            for (int i = 0; i < count; i++) Length += lengths[i];
        }

        private void LateUpdate()
        {
            if (points == null || skull == null) return;
            int count = bones.Length;
            for (int i = 0; i <= count; i++) rest[i] = transform.TransformPoint(restAt[i]);
            Vector3 want = skull.TransformPoint(skullPoint);
            Vector3 offset = want - rest[count];
            for (int i = 0; i <= count; i++)
            {
                float share = i / (float)count;
                points[i] = rest[i] + offset * (share * share * (3f - 2f * share));
            }
            for (int pass = 0; pass < 4; pass++)
            {
                points[count] = want;
                for (int i = count - 1; i >= 1; i--) points[i] = points[i + 1] + Toward(points[i + 1], points[i], rest[i] - rest[i + 1]) * lengths[i];
                points[0] = rest[0];
                for (int i = 1; i < count; i++) points[i] = points[i - 1] + Toward(points[i - 1], points[i], rest[i] - rest[i - 1]) * lengths[i - 1];
            }
            points[count] = want;
            float sharpest = 0f;
            for (int i = 0; i < count; i++)
            {
                Vector3 was = rest[i + 1] - rest[i];
                Vector3 now = points[i + 1] - points[i];
                bones[i].SetPositionAndRotation(points[i], Quaternion.FromToRotation(was, now) * (transform.rotation * restTurn[i]));
                if (i == 0) continue;
                // How much more this joint is bent than it was made.
                float bend = Mathf.Abs(Vector3.Angle(points[i] - points[i - 1], now) - Vector3.Angle(rest[i] - rest[i - 1], was));
                if (bend > sharpest) sharpest = bend;
            }
            SharpestBend = sharpest;
            // The end is always put on the skull; what is left over shows as the last stretch drawn longer than it was made.
            Gap = Mathf.Max(0f, (points[count] - points[count - 1]).magnitude - lengths[count - 1]);
        }

        private static Vector3 Toward(Vector3 from, Vector3 to, Vector3 otherwise)
        {
            Vector3 way = to - from;
            return way.sqrMagnitude > 0.000001f ? way.normalized : otherwise.normalized;
        }
    }
}

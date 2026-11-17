using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// Writes a <see cref="SerpentSpine"/> to a chain of bones each frame, so a skinned tail or neck bends where the
    /// spine bends, and carries the shapes a blow can land on along with it. Whoever owns the spine moves it; this
    /// only poses the bones.
    /// </summary>
    public class SpineBones : MonoBehaviour
    {
        [SerializeField] private Transform[] bones = new Transform[0];
        [SerializeField] private CapsuleCollider[] reaches = new CapsuleCollider[0];

        /// <summary>The spine to follow; none leaves the bones as they are.</summary>
        public SerpentSpine Spine { get; set; }

        public int Count => bones.Length;
        /// <summary>The distance from one bone to the next as the model was made.</summary>
        public float Spacing { get; private set; } = 1f;
        /// <summary>The distance from each bone to the next as the model was made, root end first.</summary>
        public float[] Lengths { get; private set; } = new float[0];
        public CapsuleCollider[] Reaches => reaches;

        private Quaternion[] fix;
        // How far above the ground each joint sits when the piece lies on it: the spine runs along the ground, the bones along the middle of the body.
        private float[] lift;

        public void Configure(Transform[] chain, CapsuleCollider[] hitShapes)
        {
            bones = chain;
            reaches = hitShapes;
        }

        private void Awake()
        {
            // However the model's bones are turned, what each one's own turn is when it points along the chain is
            // read from the pose it was made in, and kept.
            fix = new Quaternion[bones.Length];
            lift = new float[bones.Length];
            Lengths = new float[Mathf.Max(0, bones.Length - 1)];
            float total = 0f;
            for (int i = 0; i < bones.Length; i++)
            {
                Vector3 along = i < bones.Length - 1 ? bones[i + 1].position - bones[i].position : bones[i].position - bones[i - 1].position;
                if (i < bones.Length - 1)
                {
                    Lengths[i] = along.magnitude;
                    total += along.magnitude;
                }
                lift[i] = bones[i].position.y - transform.position.y;
                fix[i] = along.sqrMagnitude > 0.000001f ? Quaternion.Inverse(Quaternion.LookRotation(along, Vector3.up)) * bones[i].rotation : Quaternion.identity;
            }
            if (bones.Length > 1) Spacing = Mathf.Max(0.01f, total / (bones.Length - 1));
        }

        private void LateUpdate()
        {
            if (Spine == null || fix == null) return;
            var points = Spine.Points;
            int count = Mathf.Min(bones.Length, points.Length);
            // Root first: each bone is the child of the one before it.
            for (int i = 0; i < count; i++)
            {
                Vector3 at = points[i] + Vector3.up * lift[i];
                Vector3 along = i < count - 1 ? points[i + 1] + Vector3.up * lift[i + 1] - at : at - (points[i - 1] + Vector3.up * lift[i - 1]);
                var turned = along.sqrMagnitude > 0.000001f ? Quaternion.LookRotation(along, Vector3.up) * fix[i] : bones[i].rotation;
                bones[i].SetPositionAndRotation(at, turned);
            }
            // The hit shapes lie along the chain, shared out evenly, each on its stretch of it.
            for (int i = 0; i < reaches.Length; i++)
            {
                int from = Mathf.Min(count - 2, i * (count - 1) / reaches.Length);
                int to = Mathf.Min(count - 1, (i + 1) * (count - 1) / reaches.Length);
                Vector3 along = points[to] - points[from];
                if (along.sqrMagnitude < 0.000001f) continue;
                reaches[i].transform.SetPositionAndRotation((points[from] + points[to]) * 0.5f, Quaternion.LookRotation(along, Vector3.up));
            }
        }
    }
}

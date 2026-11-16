using UnityEngine;

namespace Clover.World
{
    /// <summary>Keeps this object over another, without turning with it: the rain follows the player this way.</summary>
    public class Follow : MonoBehaviour
    {
        [SerializeField] private Transform target;

        public Vector3 Offset;

        public void Configure(Transform targetToFollow, Vector3 offset)
        {
            target = targetToFollow;
            Offset = offset;
        }

        private void LateUpdate()
        {
            if (target != null) transform.position = target.position + Offset;
        }
    }
}

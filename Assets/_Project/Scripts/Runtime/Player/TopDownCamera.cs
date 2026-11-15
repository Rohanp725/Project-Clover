using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The fixed-angle camera above the player. It never rotates during play, so the player always knows which way is up
    /// on the stick; it only follows, with a little smoothing so rolls do not jerk the view.
    /// </summary>
    public class TopDownCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;

        public float PitchDegrees = 55f;
        public float YawDegrees = 0f;
        public float Distance = 12f;
        /// <summary>Seconds the camera takes to close most of the gap to the player; 0 locks it on.</summary>
        public float FollowTime = 0.12f;

        private Vector3 focus;
        private Vector3 focusVelocity;

        public void Configure(Transform targetToFollow) => target = targetToFollow;

        /// <summary>Puts the camera where it belongs at once; the builder uses it so the saved scene already looks right.</summary>
        public void SnapToTarget()
        {
            if (target != null) focus = target.position;
            focusVelocity = Vector3.zero;
            Place();
        }

        private void Start() => SnapToTarget();

        private void LateUpdate()
        {
            if (target == null) return;
            focus = Vector3.SmoothDamp(focus, target.position, ref focusVelocity, FollowTime);
            Place();
        }

        private void Place()
        {
            var rotation = Quaternion.Euler(PitchDegrees, YawDegrees, 0f);
            transform.SetPositionAndRotation(focus - rotation * Vector3.forward * Distance, rotation);
        }
    }
}

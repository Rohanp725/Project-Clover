using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The fixed-angle camera above the player. It never rotates during play, so the player always knows which way is up
    /// on the stick; it only follows, with a little smoothing so rolls do not jerk the view, and shakes on heavy blows.
    /// </summary>
    public class TopDownCamera : MonoBehaviour
    {
        [SerializeField] private Transform target;

        public float PitchDegrees = 55f;
        public float YawDegrees = 0f;
        public float Distance = 12f;
        /// <summary>Seconds the camera takes to close most of the gap to the player; 0 locks it on.</summary>
        public float FollowTime = 0.12f;
        /// <summary>How many times a second the shake picks a new offset.</summary>
        public float ShakeRate = 45f;

        private Vector3 focus;
        private Vector3 focusVelocity;
        private float shakeAmount;
        private float shakeSeconds;
        private float shakeLeft;
        private float shakeStepLeft;
        private Vector3 shakeOffset;

        public void Configure(Transform targetToFollow) => target = targetToFollow;

        /// <summary>Puts the camera where it belongs at once; the builder uses it so the saved scene already looks right.</summary>
        public void SnapToTarget()
        {
            if (target != null) focus = target.position;
            focusVelocity = Vector3.zero;
            Place();
        }

        /// <summary>Shakes the view by up to this many metres, dying away over the given real seconds. A weaker shake never replaces a stronger one.</summary>
        public void Shake(float metres, float seconds)
        {
            if (metres <= 0f || seconds <= 0f) return;
            float left = shakeSeconds > 0f ? shakeAmount * (shakeLeft / shakeSeconds) : 0f;
            if (metres < left) return;
            shakeAmount = metres;
            shakeSeconds = seconds;
            shakeLeft = seconds;
            shakeStepLeft = 0f;
        }

        private void Start() => SnapToTarget();

        private void LateUpdate()
        {
            if (target == null) return;
            focus = Vector3.SmoothDamp(focus, target.position, ref focusVelocity, FollowTime);

            // The shake runs on real time: it is at its strongest exactly while a hit has time nearly stopped.
            if (shakeLeft > 0f)
            {
                shakeLeft -= Time.unscaledDeltaTime;
                shakeStepLeft -= Time.unscaledDeltaTime;
                if (shakeLeft <= 0f)
                {
                    shakeOffset = Vector3.zero;
                }
                else if (shakeStepLeft <= 0f)
                {
                    shakeStepLeft = ShakeRate > 0f ? 1f / ShakeRate : 0f;
                    Vector2 point = Random.insideUnitCircle * (shakeAmount * (shakeLeft / shakeSeconds));
                    shakeOffset = new Vector3(point.x, 0f, point.y);
                }
            }
            Place();
        }

        private void Place()
        {
            var rotation = Quaternion.Euler(PitchDegrees, YawDegrees, 0f);
            transform.SetPositionAndRotation(focus + shakeOffset - rotation * Vector3.forward * Distance, rotation);
        }
    }
}

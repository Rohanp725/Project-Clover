using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The fixed-angle camera above the player. It never rotates during play, so the player always knows which way is up
    /// on the stick; it only follows, with a little smoothing so rolls do not jerk the view, and shakes on heavy blows.
    /// A fight with something too big for the usual view may ask for a wider framing for as long as it lasts.
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
        /// <summary>Seconds the camera takes to settle into a framing that was asked for, or back out of it.</summary>
        public float FramingTime = 0.9f;

        /// <summary>True while something has asked for a framing other than the usual one.</summary>
        public bool HasFraming { get; private set; }

        private Vector3 focus;
        private Vector3 focusVelocity;
        private float shakeAmount;
        private float shakeSeconds;
        private float shakeLeft;
        private float shakeStepLeft;
        private Vector3 shakeOffset;
        private float wantedDistance = 1f, shownDistance = 1f, distanceVelocity;
        private float wantedPitch, shownPitch, pitchVelocity;
        private Vector3 wantedLookAhead, shownLookAhead, lookAheadVelocity;

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

        /// <summary>
        /// Asks for a temporary framing, eased into: the camera stands this many times further off, looks at a point this
        /// far (in metres, on the ground) from the player, and tips its pitch by this many degrees. It may be called every
        /// frame with a new look-ahead. The heading never changes, so the stick still means what it meant.
        /// </summary>
        public void SetFraming(float distanceMultiplier, Vector3 lookAhead, float pitchChangeDegrees = 0f)
        {
            HasFraming = true;
            wantedDistance = Mathf.Max(0.1f, distanceMultiplier);
            wantedLookAhead = lookAhead;
            wantedPitch = pitchChangeDegrees;
        }

        /// <summary>Lets the camera ease back to its usual framing.</summary>
        public void ClearFraming()
        {
            HasFraming = false;
            wantedDistance = 1f;
            wantedLookAhead = Vector3.zero;
            wantedPitch = 0f;
        }

        private void Start() => SnapToTarget();

        private void LateUpdate()
        {
            if (target == null) return;
            focus = Vector3.SmoothDamp(focus, target.position, ref focusVelocity, FollowTime);
            shownDistance = Mathf.SmoothDamp(shownDistance, wantedDistance, ref distanceVelocity, FramingTime);
            shownPitch = Mathf.SmoothDamp(shownPitch, wantedPitch, ref pitchVelocity, FramingTime);
            shownLookAhead = Vector3.SmoothDamp(shownLookAhead, wantedLookAhead, ref lookAheadVelocity, FramingTime);

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
            var rotation = Quaternion.Euler(PitchDegrees + shownPitch, YawDegrees, 0f);
            transform.SetPositionAndRotation(focus + shownLookAhead + shakeOffset - rotation * Vector3.forward * (Distance * shownDistance), rotation);
        }
    }
}

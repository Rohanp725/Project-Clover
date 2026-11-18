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
        /// <summary>Real seconds the camera takes to lean in for a finishing blow, and to draw back after it.</summary>
        public float CloseInTime = 0.1f;
        public float CloseOutTime = 0.22f;
        /// <summary>The widest and narrowest screen shapes the view is made for. On a narrower screen the field is opened upward so as much is seen to either side as at 16:9; on a wider one the picture stays 21:9 in the middle of the screen, since the edges of the world were dressed no further.</summary>
        public float NarrowestAspect = 16f / 9f;
        public float WidestAspect = 21f / 9f;

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
        // The lean-in for a finishing blow is its own layer on top of any framing, so it can never leave a framing changed.
        private float closeDistance = 1f, closeLeft, shownClose, closeVelocity;
        private Vector3 closeOffset;

        private Camera lens;
        private Camera bars;
        private float madeField;
        private float fittedAspect;

        private void Awake()
        {
            lens = GetComponent<Camera>();
            if (lens != null) madeField = lens.fieldOfView;
        }

        // The field of view is kept up and down by the engine, so a 4:3 screen would lose the sides of a fight and a
        // 32:9 one would see far past what was built. Narrower than 16:9, the field is opened so the sides are kept;
        // wider than 21:9, the picture keeps 21:9 in the middle of the screen with black to either side, since closing
        // the field instead cut the god's head off the top. Only when the screen's shape changes.
        private void FitScreen()
        {
            if (lens == null || Screen.height <= 0) return;
            float screen = Screen.width / (float)Screen.height;
            if (Mathf.Approximately(screen, fittedAspect)) return;
            fittedAspect = screen;
            float shown = Mathf.Min(screen, WidestAspect);
            float width = shown / screen;
            bool barred = width < 0.999f;
            lens.rect = barred ? new Rect((1f - width) * 0.5f, 0f, width, 1f) : new Rect(0f, 0f, 1f, 1f);
            if (barred && bars == null)
            {
                // Something has to paint the strips the picture no longer covers: a camera that sees nothing, drawn first.
                var barsObject = new GameObject("Screen Bars");
                bars = barsObject.AddComponent<Camera>();
                bars.clearFlags = CameraClearFlags.SolidColor;
                bars.backgroundColor = Color.black;
                bars.cullingMask = 0;
                bars.depth = lens.depth - 10f;
                bars.allowHDR = false;
                bars.allowMSAA = false;
            }
            if (bars != null) bars.enabled = barred;
            float held = Mathf.Max(shown, NarrowestAspect);
            float half = Mathf.Tan(madeField * 0.5f * Mathf.Deg2Rad) * held / Mathf.Max(0.1f, shown);
            lens.fieldOfView = Mathf.Clamp(2f * Mathf.Atan(half) * Mathf.Rad2Deg, 10f, 80f);
        }

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

        /// <summary>
        /// A framing that keeps the player and one other point on the ground both in the picture (and whatever stands
        /// <paramref name="otherHeight"/> metres tall there), with a margin round each, standing back no less than <paramref name="least"/> times the usual distance and no more than
        /// <paramref name="most"/>. For a moment that must show who it happens to and who watches it.
        /// </summary>
        public void FrameBoth(Vector3 other, float least, float pitchChangeDegrees = 0f, float most = 3f, float otherHeight = 2f)
        {
            if (target == null) return;
            float field = lens != null ? lens.fieldOfView : 40f;
            float aspect = lens != null && Screen.height > 0 ? Mathf.Min(Screen.width / (float)Screen.height, WidestAspect) : 16f / 9f;
            Vector3 look = Hold(target.position, other, PitchDegrees + pitchChangeDegrees, field, aspect, Distance, least, most, out float multiplier, otherHeight);
            SetFraming(multiplier, look - new Vector3(target.position.x, 0f, target.position.z), pitchChangeDegrees);
        }

        /// <summary>Metres kept clear round each of the two points held in the picture.</summary>
        public const float HoldMargin = 1.6f;

        /// <summary>
        /// Where on the ground a camera of this pitch, field and distance must look, and how many times the distance it
        /// must stand back, to hold two points on the ground in the picture with <see cref="HoldMargin"/> round each.
        /// The camera looks north and down, so it sees less of the ground nearer to it than beyond what it looks at.
        /// </summary>
        public static Vector3 Hold(Vector3 first, Vector3 second, float pitchDegrees, float fieldDegrees, float aspect, float distance,
            float least, float most, out float multiplier, float secondHeight = 0f)
        {
            float pitch = Mathf.Clamp(pitchDegrees, 15f, 89f) * Mathf.Deg2Rad;
            float half = Mathf.Clamp(fieldDegrees, 5f, 120f) * 0.5f * Mathf.Deg2Rad;
            float height = Mathf.Sin(pitch);
            float middle = height / Mathf.Tan(pitch);
            // Metres of ground per metre of distance: toward the camera, beyond the point looked at, and to either side.
            float near = middle - height / Mathf.Tan(Mathf.Min(pitch + half, 89.9f * Mathf.Deg2Rad));
            float far = height / Mathf.Tan(Mathf.Max(pitch - half, 1f * Mathf.Deg2Rad)) - middle;
            float side = Mathf.Tan(half) * Mathf.Max(0.5f, aspect);
            // Something tall at the second point reaches up the picture as far as ground this much further north would,
            // seen along the steepest-lying ray of the picture, its top edge.
            float top = second.z + Mathf.Max(0f, secondHeight) / Mathf.Tan(Mathf.Max(pitch - half, 5f * Mathf.Deg2Rad));
            float south = Mathf.Min(first.z, second.z) - HoldMargin, north = Mathf.Max(first.z, top) + HoldMargin;
            float west = Mathf.Min(first.x, second.x) - HoldMargin, east = Mathf.Max(first.x, second.x) + HoldMargin;
            float need = Mathf.Max((north - south) / ((near + far) * distance), (east - west) / (2f * side * distance));
            multiplier = Mathf.Clamp(need, least, most);
            float x = (west + east) * 0.5f, halfAcross = (east - west) * 0.5f;
            float z = 0f;
            // Nearer the camera less ground is seen to either side: stand back a little at a time until the side of each
            // point is held too. Twenty steps at most, so a framing asked for every frame costs next to nothing.
            for (int step = 0; step < 20; step++)
            {
                float depth = multiplier * distance;
                // Anywhere between these the two are held top to bottom; the middle leaves the most room either way.
                float lookFrom = north - far * depth, lookTo = south + near * depth;
                // When they only just fit (or cannot), the halfway point between the two limits is still the fairest.
                z = (lookFrom + lookTo) * 0.5f;
                float nearest = Mathf.Min(first.z, second.z) - HoldMargin;
                float seenAt = (nearest - z) * Mathf.Cos(pitch) + depth;
                if (halfAcross <= seenAt * side || multiplier >= most) break;
                multiplier = Mathf.Min(most, multiplier * 1.06f);
            }
            return new Vector3(x, 0f, z);
        }

        /// <summary>Lets the camera ease back to its usual framing.</summary>
        public void ClearFraming()
        {
            HasFraming = false;
            wantedDistance = 1f;
            wantedLookAhead = Vector3.zero;
            wantedPitch = 0f;
        }

        /// <summary>
        /// Leans the view in for a moment: this many times the distance, looking this far (metres, on the ground) from
        /// the player, for this many real seconds, eased in and out again. It is ignored while a framing is in force,
        /// because whoever asked for that framing needs the whole of it seen.
        /// </summary>
        public void CloseIn(float distanceMultiplier, Vector3 offset, float seconds)
        {
            if (HasFraming || seconds <= 0f) return;
            closeDistance = Mathf.Clamp(distanceMultiplier, 0.1f, 1f);
            closeOffset = offset;
            closeLeft = seconds;
        }

        private void Start() => SnapToTarget();

        private void LateUpdate()
        {
            FitScreen();
            if (target == null) return;
            // A menu has stopped the world: the view holds exactly where it was, lean-in and shake included, and goes
            // on from there when time starts again. A blow's hit-stop never reaches zero, so it does not come here.
            if (Time.timeScale == 0f) return;
            focus =Vector3.SmoothDamp(focus, target.position, ref focusVelocity, FollowTime);
            shownDistance = Mathf.SmoothDamp(shownDistance, wantedDistance, ref distanceVelocity, FramingTime);
            shownPitch = Mathf.SmoothDamp(shownPitch, wantedPitch, ref pitchVelocity, FramingTime);
            shownLookAhead = Vector3.SmoothDamp(shownLookAhead, wantedLookAhead, ref lookAheadVelocity, FramingTime);
            // On real time, like the shake: the lean-in happens while the blow has time stopped and slowed.
            if (HasFraming) closeLeft = 0f;
            bool closing = closeLeft > 0f;
            if (closing) closeLeft -= Time.unscaledDeltaTime;
            if (closing || shownClose > 0.0005f)
                shownClose = Mathf.SmoothDamp(shownClose, closing ? 1f : 0f, ref closeVelocity, closing ? CloseInTime : CloseOutTime, Mathf.Infinity, Time.unscaledDeltaTime);
            else
                shownClose = 0f;

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
            float distance = Distance * shownDistance * Mathf.Lerp(1f, closeDistance, shownClose);
            transform.SetPositionAndRotation(focus + shownLookAhead + closeOffset * shownClose + shakeOffset - rotation * Vector3.forward * distance, rotation);
        }
    }
}

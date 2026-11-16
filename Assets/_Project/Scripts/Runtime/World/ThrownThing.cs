using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// Something small thrown through the air in an arc, such as a flask: it is only for the eye. Whatever happens
    /// where it lands is decided elsewhere and timed to match. One is kept in the scene and used again each throw.
    /// </summary>
    public class ThrownThing : MonoBehaviour
    {
        [SerializeField] private Transform body;
        [SerializeField] private TrailRenderer trail;

        /// <summary>Turns per second, end over end, as it flies.</summary>
        public float Tumble = 2.5f;

        private static ThrownThing instance;

        private Vector3 from, to;
        private float seconds, height, elapsed;
        private bool flying;

        // Domain reload is off: the thing of the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        public void Configure(Transform bodyToMove, TrailRenderer trailBehind)
        {
            body = bodyToMove;
            trail = trailBehind;
        }

        private void OnEnable()
        {
            instance = this;
            if (body != null) body.gameObject.SetActive(false);
        }

        private void OnDisable()
        {
            if (instance == this) instance = null;
        }

        /// <summary>Throws it from one place to another over the given seconds, rising to the given height on the way.</summary>
        public static void Throw(Vector3 start, Vector3 end, float flightSeconds, float arcHeight)
        {
            if (instance == null || instance.body == null) return;
            instance.from = start;
            instance.to = end;
            instance.seconds = Mathf.Max(0.05f, flightSeconds);
            instance.height = arcHeight;
            instance.elapsed = 0f;
            instance.flying = true;
            instance.body.position = start;
            instance.body.gameObject.SetActive(true);
            if (instance.trail != null) instance.trail.Clear();
        }

        private void Update()
        {
            if (!flying) return;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / seconds);
            Vector3 at = Vector3.Lerp(from, to, t);
            at.y += height * 4f * t * (1f - t);
            body.position = at;
            body.Rotate(Vector3.right, Tumble * 360f * Time.deltaTime, Space.Self);
            if (t < 1f) return;
            flying = false;
            body.gameObject.SetActive(false);
        }
    }
}

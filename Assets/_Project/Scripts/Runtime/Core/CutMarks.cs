using UnityEngine;

namespace Clover.Core
{
    /// <summary>
    /// The cut line a blow leaves across what it struck: a thin white-hot stroke along the way the blow went, there at
    /// full size the instant it lands and gone a few frames later, like the line a comic draws through a hit. A handful
    /// are kept ready and reused. A finisher crosses two. With none in the scene the calls do nothing.
    /// </summary>
    public class CutMarks : MonoBehaviour
    {
        [SerializeField] private Transform[] marks = new Transform[0];
        [SerializeField] private Transform viewer;

        /// <summary>Real seconds a mark lasts.</summary>
        public float Seconds = 0.12f;
        /// <summary>Length and thickness of each kind of stroke, in metres.</summary>
        public Vector2 LightSize = new Vector2(2.6f, 0.22f);
        public Vector2 HeavySize = new Vector2(3.8f, 0.4f);
        public Vector2 FinisherSize = new Vector2(4.8f, 0.46f);
        /// <summary>Half the angle between the two strokes of a finisher.</summary>
        public float CrossDegrees = 34f;

        /// <summary>Counts every stroke drawn, so probes can see them without watching the screen.</summary>
        public static int Drawn { get; private set; }

        private static CutMarks instance;

        private float[] ages;
        private Vector2[] sizes;
        private int next;
        private int live;

        // Domain reload is off: the marks of the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            Drawn = 0;
        }

        /// <summary>The builder hands over the strokes, each one unit long and one thick, and the camera they are turned to.</summary>
        public void Configure(Transform[] strokes, Transform cameraToFace)
        {
            marks = strokes;
            viewer = cameraToFace;
        }

        private void Awake()
        {
            ages = new float[marks.Length];
            sizes = new Vector2[marks.Length];
            for (int i = 0; i < marks.Length; i++)
            {
                ages[i] = float.MaxValue;
                if (marks[i] != null) marks[i].gameObject.SetActive(false);
            }
        }

        private void OnEnable() => instance = this;

        private void OnDisable()
        {
            if (instance == this) instance = null;
        }

        /// <summary>The line of a light blow through a point, along the way the blow travelled.</summary>
        public static void Light(Vector3 at, Vector3 along)
        {
            if (instance != null) instance.Draw(at, along, 0f, instance.LightSize);
        }

        /// <summary>The line of a heavy blow: longer and thicker.</summary>
        public static void Heavy(Vector3 at, Vector3 along)
        {
            if (instance != null) instance.Draw(at, along, 0f, instance.HeavySize);
        }

        /// <summary>The mark of a finisher: two long strokes crossed over the point.</summary>
        public static void Finisher(Vector3 at, Vector3 along)
        {
            if (instance == null) return;
            instance.Draw(at, along, instance.CrossDegrees, instance.FinisherSize);
            instance.Draw(at, along, -instance.CrossDegrees, instance.FinisherSize);
        }

        private void Draw(Vector3 at, Vector3 along, float turnDegrees, Vector2 size)
        {
            if (marks.Length == 0 || viewer == null) return;
            int i = next;
            next = (next + 1) % marks.Length;
            var mark = marks[i];
            if (mark == null) return;
            // The stroke lies flat to the camera, so it is never seen edge-on, and runs the way the blow ran as the camera sees it.
            Vector3 toward = viewer.forward;
            Vector3 across = Vector3.ProjectOnPlane(along, toward);
            if (across.sqrMagnitude < 0.0001f) across = viewer.right;
            across = Quaternion.AngleAxis(turnDegrees, toward) * across.normalized;
            mark.SetPositionAndRotation(at, Quaternion.LookRotation(toward, Vector3.Cross(toward, across)));
            mark.localScale = new Vector3(size.x, size.y, 1f);
            if (ages[i] > Seconds) live++;
            ages[i] = 0f;
            sizes[i] = size;
            mark.gameObject.SetActive(true);
            Drawn++;
        }

        private void Update()
        {
            if (live == 0) return;
            for (int i = 0; i < marks.Length; i++)
            {
                if (ages[i] > Seconds) continue;
                // Real time: the line is seen through the stop that comes with the blow.
                ages[i] += Time.unscaledDeltaTime;
                var mark = marks[i];
                if (ages[i] > Seconds)
                {
                    live--;
                    if (mark != null) mark.gameObject.SetActive(false);
                    continue;
                }
                if (mark == null) continue;
                // Whole for the first part, then it runs on a little and thins to nothing: snapped on, not faded in.
                float shown = ages[i] / Seconds;
                float thin = shown < 0.35f ? 1f : 1f - (shown - 0.35f) / 0.65f;
                mark.localScale = new Vector3(sizes[i].x * (1f + 0.3f * shown), sizes[i].y * thin, 1f);
            }
        }
    }
}

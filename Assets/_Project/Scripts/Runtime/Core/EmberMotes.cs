using UnityEngine;

namespace Clover.Core
{
    /// <summary>
    /// The embers seen flying from a burnt-out body to the one who earned them. They are for the eye only: the ember
    /// itself is credited when the enemy dies, by whoever keeps that account. So that the number on screen does not run
    /// ahead of the mote, an ember can be promised here when it is credited; the HUD holds it back until its mote lands.
    /// A few motes are kept ready and reused. With none in the scene nothing is promised and nothing flies.
    /// </summary>
    public class EmberMotes : MonoBehaviour
    {
        [SerializeField] private Transform[] motes = new Transform[0];
        [SerializeField] private TrailRenderer[] trails = new TrailRenderer[0];

        /// <summary>Seconds a mote takes to reach the one it flies to.</summary>
        public float Seconds = 0.62f;
        /// <summary>Seconds between one mote and the next when a body gives up several.</summary>
        public float Apart = 0.08f;
        /// <summary>How high above the body, and how far to one side, the arc swings out before it turns in.</summary>
        public float Rise = 2.4f;
        public float Swing = 1.7f;
        /// <summary>How high on the one it flies to the mote is taken in.</summary>
        public float TakenAt = 1.1f;
        public float Size = 0.3f;

        /// <summary>Embers credited whose motes have not yet landed.</summary>
        public static int Pending { get; private set; }
        /// <summary>Counts every mote that has landed, so the HUD can answer each one.</summary>
        public static int Arrived { get; private set; }

        private static EmberMotes instance;

        private float[] ages;
        private Vector3[] starts;
        private Vector3[] swings;
        private Transform[] takers;
        private int flying;

        // Domain reload is off: the counts and the pool of the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            Pending = 0;
            Arrived = 0;
        }

        public void Configure(Transform[] moteShapes, TrailRenderer[] moteTrails)
        {
            motes = moteShapes;
            trails = moteTrails;
        }

        private void Awake()
        {
            ages = new float[motes.Length];
            starts = new Vector3[motes.Length];
            swings = new Vector3[motes.Length];
            takers = new Transform[motes.Length];
            for (int i = 0; i < motes.Length; i++)
            {
                ages[i] = float.MaxValue;
                if (motes[i] != null) motes[i].gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            instance = this;
            // A new scene starts with nothing in the air, whatever the last one left unpaid to the eye.
            Pending = 0;
        }

        private void OnDisable()
        {
            if (instance == this) instance = null;
            Pending = 0;
        }

        /// <summary>Says that this many embers have been credited and their motes will fly later.</summary>
        public static void Promise(int count)
        {
            if (instance != null && count > 0) Pending += count;
        }

        /// <summary>Takes back a promise whose mote will never fly, as when the body is put back before it burns.</summary>
        public static void Withdraw(int count)
        {
            if (count > 0) Pending = Mathf.Max(0, Pending - count);
        }

        /// <summary>Sends one promised mote from a point to someone. The order is its place among those sent together.</summary>
        public static void Fly(Vector3 from, Transform to, int order)
        {
            if (instance == null) return;
            // With no mote free, the ember is simply there: the count must never be left held back.
            if (!instance.Launch(from, to, order)) Land();
        }

        private static void Land()
        {
            Pending = Mathf.Max(0, Pending - 1);
            Arrived++;
        }

        private bool Launch(Vector3 from, Transform to, int order)
        {
            if (to == null) return false;
            for (int i = 0; i < motes.Length; i++)
            {
                if (ages[i] <= Seconds || motes[i] == null) continue;
                // It waits its turn unseen, then leaves; each goes out on the other side from the last.
                ages[i] = -Apart * order - 0.0001f;
                starts[i] = from;
                Vector3 gap = to.position - from;
                gap.y = 0f;
                Vector3 side = gap.sqrMagnitude > 0.01f ? Vector3.Cross(Vector3.up, gap.normalized) : Vector3.right;
                swings[i] = Vector3.up * (Rise * (1f + 0.15f * order)) + side * (Swing * (order % 2 == 0 ? 1f : -1f));
                takers[i] = to;
                flying++;
                return true;
            }
            return false;
        }

        private void Update()
        {
            if (flying == 0) return;
            for (int i = 0; i < motes.Length; i++)
            {
                if (ages[i] > Seconds) continue;
                bool waiting = ages[i] < 0f;
                ages[i] += Time.deltaTime;
                if (ages[i] < 0f) continue;
                var mote = motes[i];
                Vector3 end = takers[i] != null ? takers[i].position + Vector3.up * TakenAt : starts[i];
                if (ages[i] > Seconds)
                {
                    flying--;
                    mote.gameObject.SetActive(false);
                    Vfx.Play(VfxKind.Embers, end, Vector3.up, 0.6f);
                    Land();
                    continue;
                }
                // Slow off the body and fast into the hand: it hangs, then is drawn in.
                float shown = ages[i] / Seconds;
                float along = shown * shown * (3f - 2f * shown);
                along *= along;
                Vector3 bend = starts[i] + swings[i];
                mote.position = Vector3.Lerp(Vector3.Lerp(starts[i], bend, along), Vector3.Lerp(bend, end, along), along);
                float size = Size * (shown < 0.15f ? shown / 0.15f : 1f - 0.45f * along);
                mote.localScale = new Vector3(size, size, size);
                if (waiting)
                {
                    mote.gameObject.SetActive(true);
                    // The trail must not draw a line from wherever this mote last was.
                    if (i < trails.Length && trails[i] != null) trails[i].Clear();
                }
            }
        }
    }
}

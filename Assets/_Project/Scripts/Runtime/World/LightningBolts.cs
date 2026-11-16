using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// Draws lightning striking the ground: a jagged line from the sky that redraws itself a few times and is gone, a
    /// crackle at its foot, and a flash across the whole scene. A few lines are kept ready; the scene has one of these
    /// and anything may call <see cref="Strike"/>. With none in the scene the call does nothing.
    /// </summary>
    public class LightningBolts : MonoBehaviour
    {
        [SerializeField] private LineRenderer[] lines = new LineRenderer[0];
        [SerializeField] private StormLight storm;
        [SerializeField] private Light groundFlash;

        public float Seconds = 0.22f;
        public float Height = 26f;
        /// <summary>How far, in metres, the bolt's kinks stray from the straight line.</summary>
        public float Jitter = 1.1f;
        /// <summary>Seconds between redraws of a bolt's shape while it lasts.</summary>
        public float RedrawEvery = 0.045f;
        public float FlashIntensity = 9f;

        private const int Points = 12;

        private static LightningBolts instance;

        private float[] ages;
        private float[] untilRedraw;
        private Vector3[] feet;
        private readonly Vector3[] shape = new Vector3[Points];
        private int next;
        private float flashLeft;

        // Domain reload is off: the bolts of the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        public void Configure(LineRenderer[] boltLines, StormLight stormToFlash, Light flashOnGround)
        {
            lines = boltLines;
            storm = stormToFlash;
            groundFlash = flashOnGround;
        }

        private void Awake()
        {
            ages = new float[lines.Length];
            untilRedraw = new float[lines.Length];
            feet = new Vector3[lines.Length];
            for (int i = 0; i < lines.Length; i++)
            {
                ages[i] = float.MaxValue;
                if (lines[i] == null) continue;
                lines[i].positionCount = Points;
                lines[i].enabled = false;
            }
            if (groundFlash != null) groundFlash.enabled = false;
        }

        private void OnEnable() => instance = this;

        private void OnDisable()
        {
            if (instance == this) instance = null;
        }

        /// <summary>A bolt comes down on this point now. <paramref name="skyFlash"/> is how much of the whole sky's flash goes with it.</summary>
        public static void Strike(Vector3 point, float skyFlash = 0.6f)
        {
            if (instance == null || instance.lines.Length == 0) return;
            instance.Begin(point, skyFlash);
        }

        private void Begin(Vector3 point, float skyFlash)
        {
            int i = next;
            next = (next + 1) % lines.Length;
            if (lines[i] == null) return;
            feet[i] = point;
            ages[i] = 0f;
            untilRedraw[i] = 0f;
            lines[i].enabled = true;
            Clover.Core.Vfx.Play(Clover.Core.VfxKind.Shock, point + Vector3.up * 0.3f, Vector3.up, 1.6f);
            Clover.Core.Vfx.Play(Clover.Core.VfxKind.Ring, point + Vector3.up * 0.15f, Vector3.up);
            if (storm != null && skyFlash > 0f) storm.Flash(skyFlash);
            if (groundFlash != null)
            {
                groundFlash.transform.position = point + Vector3.up * 2.5f;
                groundFlash.enabled = true;
                flashLeft = Seconds * 1.6f;
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < lines.Length; i++)
            {
                if (ages[i] > Seconds || lines[i] == null) continue;
                ages[i] += deltaTime;
                if (ages[i] > Seconds)
                {
                    lines[i].enabled = false;
                    continue;
                }
                untilRedraw[i] -= deltaTime;
                if (untilRedraw[i] > 0f) continue;
                untilRedraw[i] = RedrawEvery;
                Draw(lines[i], feet[i]);
                lines[i].widthMultiplier = Mathf.Lerp(0.5f, 0.12f, ages[i] / Seconds);
            }

            if (groundFlash != null && flashLeft > 0f)
            {
                flashLeft -= deltaTime;
                groundFlash.intensity = FlashIntensity * Mathf.Clamp01(flashLeft / (Seconds * 1.6f));
                if (flashLeft <= 0f) groundFlash.enabled = false;
            }
        }

        // From a point high above, a little off to one side, down to the foot, kinked more in the middle than at the ends.
        private void Draw(LineRenderer line, Vector3 foot)
        {
            Vector3 top = foot + new Vector3(Random.Range(-4f, 4f), Height, Random.Range(-4f, 4f));
            for (int p = 0; p < Points; p++)
            {
                float t = p / (float)(Points - 1);
                Vector3 point = Vector3.Lerp(top, foot, t);
                float stray = Jitter * Mathf.Sin(t * Mathf.PI);
                point += new Vector3(Random.Range(-stray, stray), 0f, Random.Range(-stray, stray));
                shape[p] = point;
            }
            line.SetPositions(shape);
        }
    }
}

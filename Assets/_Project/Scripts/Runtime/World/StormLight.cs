using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// The storm overhead: every so often the sky flashes, lighting the whole scene hard and cold for an instant, with a
    /// quick stutter after it the way real lightning has. It drives one directional light that is otherwise dark.
    /// Anything else that wants a flash (a bolt striking the ground) calls <see cref="Flash"/>.
    /// </summary>
    public class StormLight : MonoBehaviour
    {
        [SerializeField] private Light flashLight;

        public float MinSecondsBetween = 4f;
        public float MaxSecondsBetween = 11f;
        public float PeakIntensity = 1.7f;
        /// <summary>Seconds a flash takes to die away.</summary>
        public float FadeSeconds = 0.35f;
        /// <summary>How many times a flash may stutter again before it dies.</summary>
        public int MaxStutters = 2;

        private float untilNext;
        private float level;
        private int stuttersLeft;
        private float untilStutter;

        public void Configure(Light lightToFlash) => flashLight = lightToFlash;

        private void OnEnable()
        {
            untilNext = Random.Range(MinSecondsBetween, MaxSecondsBetween) * 0.5f;
            level = 0f;
            Apply();
        }

        /// <summary>Flashes the sky now, at a share of the full strength.</summary>
        public void Flash(float strength = 1f)
        {
            level = Mathf.Max(level, Mathf.Clamp01(strength));
            stuttersLeft = Random.Range(0, MaxStutters + 1);
            untilStutter = Random.Range(0.05f, 0.12f);
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            untilNext -= deltaTime;
            if (untilNext <= 0f)
            {
                untilNext = Random.Range(MinSecondsBetween, MaxSecondsBetween);
                Flash(Random.Range(0.6f, 1f));
            }

            if (level > 0f)
            {
                level = Mathf.Max(0f, level - deltaTime / Mathf.Max(0.01f, FadeSeconds));
                if (stuttersLeft > 0)
                {
                    untilStutter -= deltaTime;
                    if (untilStutter <= 0f)
                    {
                        stuttersLeft--;
                        untilStutter = Random.Range(0.05f, 0.12f);
                        level = Mathf.Max(level, Random.Range(0.5f, 0.9f));
                    }
                }
            }
            Apply();
        }

        private void Apply()
        {
            if (flashLight == null) return;
            flashLight.intensity = PeakIntensity * level * level;
            flashLight.enabled = level > 0f;
        }
    }
}

using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// The light of an open fire: its strength wanders unevenly, several times a second, as flame does, never pulsing in
    /// step with the fire beside it (each starts from its own place in the noise). Only what is drawn changes; nothing
    /// is allocated.
    /// </summary>
    [RequireComponent(typeof(Light))]
    public class FireFlicker : MonoBehaviour
    {
        /// <summary>The light's strength at the middle of its flicker.</summary>
        public float Intensity = 2f;
        /// <summary>How quickly it wanders, in steps a second (an open fire flickers at about 6 to 13).</summary>
        public float Rate = 9f;

        private Light shown;
        private float seed;

        private void Awake()
        {
            shown = GetComponent<Light>();
            Vector3 at = transform.position;
            seed = Mathf.Repeat(at.x * 12.9898f + at.z * 78.233f, 97f);
        }

        private void Update()
        {
            float t = Time.time * Rate + seed;
            float wander = Mathf.PerlinNoise(t, seed) * 0.7f + Mathf.PerlinNoise(t * 2.3f, seed + 31f) * 0.3f;
            shown.intensity = Intensity * (0.7f + 0.5f * wander);
        }
    }
}

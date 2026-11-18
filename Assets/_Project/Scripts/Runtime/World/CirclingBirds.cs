using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// Kites wheeling high over the player: too high for this camera to see, but their shadows cross the ground under
    /// it, slow and banking, now and then. Each bird is a child drawn only as its shadow; the ring they wheel round
    /// drifts after the player. Nothing is allocated.
    /// </summary>
    public class CirclingBirds : MonoBehaviour
    {
        [SerializeField] private Transform player;
        [SerializeField] private Transform[] birds = new Transform[0];

        public float Height = 22f;
        public float Radius = 14f;
        public float Speed = 0.16f;
        /// <summary>How far ahead of the player the ring keeps, so the shadows fall where the camera looks (north).</summary>
        public Vector3 Ahead = new Vector3(0f, 0f, 4f);

        private Vector3 centre;
        private float[] phase, reach, lift;

        public void Configure(Transform who, Transform[] wheeling)
        {
            player = who;
            birds = wheeling;
        }

        private void Start()
        {
            centre = player != null ? player.position + Ahead : transform.position;
            phase = new float[birds.Length];
            reach = new float[birds.Length];
            lift = new float[birds.Length];
            for (int i = 0; i < birds.Length; i++)
            {
                phase[i] = i * 2.4f;
                reach[i] = Radius * (0.6f + 0.25f * i);
                lift[i] = i * 3.5f;
            }
        }

        private void LateUpdate()
        {
            if (player != null) centre = Vector3.Lerp(centre, player.position + Ahead, 1f - Mathf.Exp(-Time.deltaTime * 0.25f));
            float t = Time.time;
            for (int i = 0; i < birds.Length; i++)
            {
                if (birds[i] == null) continue;
                float angle = phase[i] + t * Speed * (i % 2 == 0 ? 1f : -0.8f);
                float wander = reach[i] * (1f + 0.3f * Mathf.Sin(t * 0.07f + i));
                var at = centre + new Vector3(Mathf.Cos(angle) * wander, Height + lift[i] + Mathf.Sin(t * 0.3f + i) * 1.5f, Mathf.Sin(angle) * wander * 0.7f);
                var ahead = new Vector3(-Mathf.Sin(angle), 0f, Mathf.Cos(angle) * 0.7f) * (i % 2 == 0 ? 1f : -1f);
                birds[i].SetPositionAndRotation(at, Quaternion.LookRotation(ahead, Vector3.up) * Quaternion.Euler(0f, 0f, i % 2 == 0 ? -14f : 14f));
                // A few slow beats of the wings now and then; mostly a glide.
                float beat = Mathf.Max(0f, Mathf.Sin(t * 0.4f + i * 1.9f) - 0.75f) * 4f;
                birds[i].localScale = new Vector3(1f - 0.35f * beat * Mathf.Abs(Mathf.Sin(t * 9f)), 1f, 1f);
            }
        }
    }
}

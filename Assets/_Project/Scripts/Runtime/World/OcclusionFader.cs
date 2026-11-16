using System.Collections.Generic;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// Keeps the player in sight. Each frame it looks along the line from the camera to the player; any
    /// <see cref="Fadeable"/> piece of the world in the way is thinned out, and pieces no longer in the way fill back in.
    /// The camera never turns, so without this every tower and wall on its side of the fight would hide it.
    /// </summary>
    public class OcclusionFader : MonoBehaviour
    {
        [SerializeField] private Transform target;

        /// <summary>How solid a piece in the way is left, from 0 to 1.</summary>
        public float FadedTo = 0.18f;
        public float FadeSeconds = 0.18f;
        /// <summary>The width of the line of sight that is kept clear, in metres.</summary>
        public float Radius = 0.9f;
        /// <summary>The line is checked to this height on the player.</summary>
        public float TargetHeight = 1f;
        /// <summary>A piece is left solid when its top is at least this far, in metres, under the line of sight.</summary>
        public float LowClearance = 0.25f;
        public LayerMask Layers = ~0;

        private readonly RaycastHit[] hits = new RaycastHit[64];
        private readonly Dictionary<Collider, Fadeable> known = new Dictionary<Collider, Fadeable>();
        private readonly List<Fadeable> faded = new List<Fadeable>();
        private readonly HashSet<Fadeable> inTheWay = new HashSet<Fadeable>();

        public void Configure(Transform targetToKeepInSight) => target = targetToKeepInSight;

        // Switched off with pieces still thinned, they would stay that way: put them all back.
        private void OnDisable()
        {
            foreach (var piece in faded)
                if (piece != null) piece.Step(1f, 1f);
            faded.Clear();
        }

        private void LateUpdate()
        {
            inTheWay.Clear();
            Vector3 from = transform.position;
            Vector3 to = target != null ? target.position + Vector3.up * TargetHeight : from;
            Vector3 line = to - from;
            float length = line.magnitude;
            if (length > 0.5f)
            {
                Vector3 direction = line / length;
                // How much of each metre along the line is travelled over the ground: the camera looks steeply down.
                float flatShare = Mathf.Max(0.05f, Mathf.Sqrt(direction.x * direction.x + direction.z * direction.z));
                // Stop short of the player, so the floor under their feet and what stands behind them are left alone.
                int count = Physics.SphereCastNonAlloc(from, Radius, direction, hits, length - Radius - 0.6f, Layers, QueryTriggerInteraction.Ignore);
                for (int i = 0; i < count; i++)
                {
                    var hit = hits[i].collider;
                    // Something whose top is below the line of sight even where the line is lowest over it (a planter, a
                    // step, a low wall some way off) hides nothing of the player, and thinning it only puts a screen of
                    // dots beside the fighters. The line falls toward the player, so it is lowest at the piece's far side.
                    Bounds bounds = hit.bounds;
                    float reach = Mathf.Sqrt(bounds.extents.x * bounds.extents.x + bounds.extents.z * bounds.extents.z);
                    float farSide = Mathf.Min(Vector3.Dot(bounds.center - from, direction) + reach / flatShare, length);
                    if (bounds.max.y < from.y + direction.y * farSide - LowClearance) continue;
                    if (!known.TryGetValue(hit, out var piece))
                    {
                        // Looked up once per collider, then remembered; null is remembered too.
                        piece = hit.GetComponent<Fadeable>();
                        known[hit] = piece;
                    }
                    if (piece == null) continue;
                    inTheWay.Add(piece);
                    if (!faded.Contains(piece)) faded.Add(piece);
                }
            }

            float amount = FadeSeconds > 0f ? Time.unscaledDeltaTime / FadeSeconds : 1f;
            for (int i = faded.Count - 1; i >= 0; i--)
            {
                var piece = faded[i];
                if (piece == null)
                {
                    faded.RemoveAt(i);
                    continue;
                }
                bool blocking = inTheWay.Contains(piece);
                bool arrived = piece.Step(blocking ? FadedTo : 1f, amount);
                if (!blocking && arrived) faded.RemoveAt(i);
            }
        }
    }
}

using Clover.Gameplay;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// The body of a standing thing (<see cref="StandingThings"/>): the model the editor step imported, its collider and its
    /// NavMesh carve (sizes in <see cref="StandingShape.For"/>), and the pieces it breaks into. It takes the blows that land
    /// on it, as an enemy would, and passes them to its rules. The hook for each model: a prefab with this component, its
    /// <see cref="Whole"/> object (and <see cref="Cracked"/> for a wall), its chunk and shard objects in <see cref="Pieces"/>,
    /// registered by kind in the scene's <see cref="StandingLibrary"/>.
    /// </summary>
    public class StandingThingBody : MonoBehaviour, IDamageable
    {
        public StandingKind Kind;
        /// <summary>The model shown while it stands whole.</summary>
        public GameObject Whole;
        /// <summary>The model swapped in when it is first struck (the cracked wall); none for the others.</summary>
        public GameObject Cracked;
        /// <summary>The chunks and shards, hidden until it breaks, then thrown out from where they sit.</summary>
        public Transform[] Pieces;
        /// <summary>Seconds the pieces fly and fade when it breaks.</summary>
        public float BreakSeconds = 0.6f;

        private int slot = -1;
        private float fallingFor = -1f;
        private Vector3[] from;
        private Vector3[] outward;

        /// <summary>Joins this body to the rules' slot it stands for.</summary>
        public void Bind(int standingSlot) => slot = standingSlot;

        private void Awake()
        {
            if (Cracked != null) Cracked.SetActive(false);
            if (Pieces != null)
                foreach (var piece in Pieces)
                    if (piece != null) piece.gameObject.SetActive(false);
        }

        /// <summary>A blow lands on it.</summary>
        public HitOutcome TakeHit(in Hit hit)
        {
            if (slot < 0 || fallingFor >= 0f) return HitOutcome.Blocked;
            // The player's blows come with a point they were struck from; an enemy's blows on it are dealt by the enemy (Enemy.Strike).
            var struck = StandingThings.Strike(slot, hit.Damage, Time.time, hit.Heavy, true);
            return struck == StandingStruck.None ? HitOutcome.Blocked : HitOutcome.Landed;
        }

        /// <summary>Shows the cracked model in place of the whole one (Cracked Wall, struck and about to burst).</summary>
        public void ShowCracked()
        {
            if (Cracked == null) return;
            Cracked.SetActive(true);
            if (Whole != null) Whole.SetActive(false);
        }

        /// <summary>It goes: broken apart, its pieces thrown out and fading, or simply gone at its time's end.</summary>
        public void Fall(bool broken)
        {
            slot = -1;
            foreach (var shape in GetComponentsInChildren<Collider>()) shape.enabled = false;
            if (!broken || Pieces == null || Pieces.Length == 0)
            {
                Destroy(gameObject);
                return;
            }
            if (Whole != null) Whole.SetActive(false);
            if (Cracked != null) Cracked.SetActive(false);
            from = new Vector3[Pieces.Length];
            outward = new Vector3[Pieces.Length];
            for (int i = 0; i < Pieces.Length; i++)
            {
                if (Pieces[i] == null) continue;
                Pieces[i].gameObject.SetActive(true);
                from[i] = Pieces[i].position;
                outward[i] = Thrown(Pieces[i].position - transform.position, i);
            }
            fallingFor = 0f;
        }

        /// <summary>The way a piece sitting at <paramref name="offset"/> from the root is thrown when the thing breaks: out and up.</summary>
        public static Vector3 Thrown(Vector3 offset, int index)
        {
            offset.y = 0f;
            Vector3 out_ = offset.sqrMagnitude > 0.0001f ? offset.normalized : Quaternion.Euler(0f, index * 137.5f, 0f) * Vector3.forward;
            return out_ * 2.2f + Vector3.up * 2.6f;
        }

        private void Update()
        {
            if (fallingFor < 0f) return;
            fallingFor += Time.deltaTime;
            float t = Mathf.Clamp01(fallingFor / Mathf.Max(0.1f, BreakSeconds));
            for (int i = 0; i < Pieces.Length; i++)
            {
                if (Pieces[i] == null) continue;
                // Thrown out and falling back under their weight, shrinking away as they land.
                Vector3 flown = outward[i] * (t * BreakSeconds) + Vector3.down * (4.9f * t * t * BreakSeconds * BreakSeconds);
                Pieces[i].position = from[i] + flown;
                Pieces[i].localScale = Vector3.one * (1f - t * t);
            }
            if (t >= 1f) Destroy(gameObject);
        }
    }
}

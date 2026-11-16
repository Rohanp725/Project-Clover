using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// One piece of a colossus that a blow can land on: a hand lying on the ground, or the head when it has fallen.
    /// It can be struck only while it is within reach, and it passes the blow on to the colossus it belongs to.
    /// </summary>
    public class ColossusPart : MonoBehaviour, IDamageable, IStaggerable
    {
        [SerializeField] private Colossus owner;
        [SerializeField] private Collider body;

        /// <summary>True for the head: only the head can be finished.</summary>
        public bool IsHead;
        /// <summary>How much of a blow's damage this piece lets through.</summary>
        public float DamageMultiplier = 1f;

        /// <summary>True while the piece lies where a blow can reach it.</summary>
        public bool Vulnerable { get; private set; }
        public bool IsStaggered => IsHead && Vulnerable && owner != null && owner.IsStaggered;
        public int BlowsTaken { get; private set; }

        public void Configure(Colossus colossus, Collider collider, bool head, float multiplier)
        {
            owner = colossus;
            body = collider;
            IsHead = head;
            DamageMultiplier = multiplier;
        }

        private void Awake() => SetVulnerable(false);

        public void SetVulnerable(bool vulnerable)
        {
            Vulnerable = vulnerable;
            if (body != null) body.enabled = vulnerable;
        }

        public HitOutcome TakeHit(in Hit hit)
        {
            if (!Vulnerable || owner == null) return HitOutcome.Missed;
            BlowsTaken++;
            return owner.TakePartHit(this, hit);
        }
    }
}

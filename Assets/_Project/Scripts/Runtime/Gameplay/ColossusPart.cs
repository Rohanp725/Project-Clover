using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// One piece of a godly form that a blow can land on: a colossus's hand lying on the ground or its fallen head, a
    /// dragon's head, tail or forefoot. It can be struck only while it is within reach, and it passes the blow on to
    /// the form it belongs to.
    /// </summary>
    public class ColossusPart : MonoBehaviour, IDamageable, IStaggerable, IAfflictable, IPaysNothing
    {
        [SerializeField] private GodlyForm owner;
        [SerializeField] private Collider body;
        [SerializeField] private Collider[] more = new Collider[0];

        /// <summary>True for the head: only the head can be finished.</summary>
        public bool IsHead;
        /// <summary>How much of a blow's damage this piece lets through.</summary>
        public float DamageMultiplier = 1f;

        /// <summary>True for a piece that is only a thing to strike (the Sun-Bearer's loose sun): blows on it pay the striker nothing back.</summary>
        [System.NonSerialized] public bool Inert;
        public bool PaysNothing => Inert;

        /// <summary>True while the piece lies where a blow can reach it.</summary>
        public bool Vulnerable { get; private set; }
        public bool IsStaggered => IsHead && Vulnerable && owner != null && owner.IsStaggered;
        public int BlowsTaken { get; private set; }
        /// <summary>What is laid on any piece is laid on the whole of it.</summary>
        public AfflictionSet Afflictions => owner != null ? owner.Afflictions : orphan;

        private readonly AfflictionSet orphan = new AfflictionSet();

        public void Configure(GodlyForm colossus, Collider collider, bool head, float multiplier)
        {
            owner = colossus;
            body = collider;
            IsHead = head;
            DamageMultiplier = multiplier;
        }

        /// <summary>Further shapes a blow can land on, for a piece that bends: they open and close with the first.</summary>
        public void ConfigureMore(Collider[] others) => more = others;

        private void Awake() => SetVulnerable(false);

        public void SetVulnerable(bool vulnerable)
        {
            Vulnerable = vulnerable;
            if (body != null) body.enabled = vulnerable;
            foreach (var other in more)
                if (other != null) other.enabled = vulnerable;
        }

        public HitOutcome TakeHit(in Hit hit)
        {
            if (!Vulnerable || owner == null) return HitOutcome.Missed;
            BlowsTaken++;
            return owner.TakePartHit(this, hit);
        }
    }
}

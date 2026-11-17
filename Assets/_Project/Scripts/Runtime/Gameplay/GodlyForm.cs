using System;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// What every god's godly form has in common, whatever its body: health, a balance that can be broken, what the
    /// elements have laid on it, pieces that pass blows on to it, and the three moments a scene's director listens
    /// for. The colossus (held for the Metal god) and the storm dragon are both one of these, so the same director,
    /// bar and probes serve either.
    /// </summary>
    public abstract class GodlyForm : MonoBehaviour, IBoss
    {
        public Health Health { get; } = new Health();
        public StaggerMeter Stagger { get; } = new StaggerMeter();
        /// <summary>What the elements have laid on it. It is too great to be frozen, slowed or shoved: afflictions only wear at it and let more through.</summary>
        public AfflictionSet Afflictions { get; } = new AfflictionSet { CanFreeze = false };

        public abstract bool IsStaggered { get; }
        public abstract bool IsDead { get; }
        public abstract bool HasRisen { get; }
        /// <summary>The fight is on from the end of its entrance to the blow that kills it: the bar shows for as long.</summary>
        public abstract bool InFight { get; }
        /// <summary>What it is doing now, by name, and for how many of its own seconds: for probes and tests.</summary>
        public abstract string Doing { get; }
        public abstract float DoingFor { get; }
        /// <summary>The piece that falls when its balance breaks, and can be finished.</summary>
        public abstract ColossusPart HeadPart { get; }
        public float HealthFraction => Health.Fraction;
        public float BalanceFraction => IsStaggered ? 1f : Stagger.Fraction;

        /// <summary>Raised once, when it has fallen and gone.</summary>
        public event Action<GodlyForm> Died;
        /// <summary>Raised once in its entrance, at the roar.</summary>
        public event Action<GodlyForm> Roared;
        /// <summary>Raised once, when its fury begins.</summary>
        public event Action<GodlyForm> Enraged;

        /// <summary>Brings it up out of the cloud. The fight with it begins when its entrance is over.</summary>
        public abstract void Rise();
        /// <summary>A blow has landed on one of its pieces.</summary>
        public abstract HitOutcome TakePartHit(ColossusPart part, in Hit hit);
        /// <summary>Wears at its balance from outside a blow; enough of it breaks the balance at once.</summary>
        public abstract void Unbalance(float amount);

        protected void RaiseDied() => Died?.Invoke(this);
        protected void RaiseRoared() => Roared?.Invoke(this);
        protected void RaiseEnraged() => Enraged?.Invoke(this);
    }
}

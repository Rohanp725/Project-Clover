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
        /// <summary>
        /// What the elements have laid on it. It is too great to be lifted, rooted, slowed or shoved, and ranks as a god;
        /// ice and holds stop it only for a hitch (<see cref="GodHolds"/>), during which its own clock stands still.
        /// </summary>
        public AfflictionSet Afflictions { get; } = new AfflictionSet { CanFreeze = false, IsGod = true, InGodlyForm = true, HoldsRefused = true };
        /// <summary>What its body glows while ice has it, as a frozen enemy's does.</summary>
        public Color FrozenGlow = new Color(0.22f, 0.5f, 0.85f);
        /// <summary>True on the steps ice or a hold has it: its own clock stands still (<see cref="OwnSeconds"/>).</summary>
        public bool IsStopped { get; private set; }
        /// <summary>Its own seconds, which stand still while it is stopped: what sways, flaps or pulses on its body reads this, not the world's time, so it stops too and carries on where it was.</summary>
        protected float FormTime { get; private set; }
        /// <summary>The piece a blow last landed on, where the ice is shown; the head until one has.</summary>
        public ColossusPart LastStruck { get; private set; }
        private int iceBeat;
        private const int IcePieces = 3;

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
        /// <summary>False while a condition on it says it begins no attack (Breath Taken and the like): its chooser waits. What is asked from outside still goes.</summary>
        protected bool MayBeginAttack => !Afflictions.Conditions.Has(ConditionFlags.BeginsNoAttack);
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
        /// <summary>Takes it out of the fight as if it had never risen, so the fight can begin again from the man: a land where a fall returns the player to a rest fire asks this. A form that cannot does nothing.</summary>
        public virtual void Retire() { }

        /// <summary>Notes the piece a blow is landing on, before the blow is passed on (<see cref="ColossusPart.TakeHit"/>).</summary>
        public void NoteStruck(ColossusPart part) => LastStruck = part;

        /// <summary>
        /// False while it must not be stopped: its entrance, its broken balance and its death, the times its clock
        /// already runs plain. Ice and holds do not take on it then (<see cref="AfflictionSet.HoldsRefused"/>).
        /// </summary>
        protected virtual bool MayBeStopped => InFight && !IsStaggered;

        /// <summary>
        /// Its own seconds for this step, from the world's: none while ice or a hold has it, so everything its body does
        /// that runs on them (its attacks' timers and telegraphs, its turning and moving, its sway) stops for that moment
        /// and carries on where it was, nothing cancelled or skipped. The one place a form's clock is stopped; each form
        /// calls it once at the top of its step, ticks its afflictions on the world's seconds, and keeps on the world's
        /// seconds only what it has already set loose. Also says whether ice or holds may take on it now.
        /// </summary>
        protected float OwnSeconds(float worldSeconds)
        {
            Afflictions.HoldsRefused = !MayBeStopped;
            bool stopped = !Afflictions.HoldsRefused && (Afflictions.IsFrozen || Afflictions.IsHeld);
            if (stopped && !IsStopped && Afflictions.IsFrozen) ShowIce();
            IsStopped = stopped;
            float own = stopped ? 0f : worldSeconds;
            FormTime += own;
            return own;
        }

        /// <summary>True on the steps ice has it: its body is painted <see cref="FrozenGlow"/>.</summary>
        protected bool Iced => IsStopped && Afflictions.IsFrozen;

        // The ice taking it, drawn as an enemy's freezing is drawn (the frozen Chill's crystals), at the piece last struck.
        private void ShowIce()
        {
            var part = LastStruck != null ? LastStruck : HeadPart;
            Vector3 at = part != null ? part.transform.position : transform.position;
            int depth = Afflictions.Stacks(AfflictionKind.Chill);
            for (int piece = 0; piece < IcePieces; piece++)
                Clover.Player.Forms.AfflictionLook.Show(AfflictionKind.Chill, depth, true, at, iceBeat++);
        }

        protected void RaiseDied() => Died?.Invoke(this);
        protected void RaiseRoared() => Roared?.Invoke(this);
        protected void RaiseEnraged() => Enraged?.Invoke(this);
    }
}

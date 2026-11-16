using System;
using System.Collections.Generic;

namespace Clover.Player
{
    /// <summary>
    /// A weapon's attacks. Light presses run the chain: a press is remembered for a short time, so pressing a little early
    /// still attacks, and a press during an attack fires the next one as soon as the swing is over. Stopping for longer
    /// than the grace time starts the chain again. The heavy button ends a chain with a finishing blow; pressed alone it
    /// is a heavy attack when tapped and, when held long enough with the mana to pay, the weapon's skill on release.
    /// Plain C#: the caller feeds it time and acts on what it reports.
    /// </summary>
    [Serializable]
    public class WeaponCombo
    {
        /// <summary>The light chain, in order.</summary>
        public List<AttackStep> Steps = new List<AttackStep>();
        /// <summary>The heavy attack when the button is tapped outside a chain. Null: the weapon has no heavy.</summary>
        public AttackStep Heavy;
        /// <summary>The heavy that ends a light chain. Null: <see cref="Heavy"/> is used.</summary>
        public AttackStep ChainHeavy;
        /// <summary>The weapon's skill, released from a full charge. Null: holding only delays the heavy.</summary>
        public AttackStep Skill;
        public float SkillManaCost = 30f;
        /// <summary>The move the heavy button gives when a staggered enemy is in reach. Null: the plain heavy is used.</summary>
        public AttackStep Finisher;
        /// <summary>Seconds the heavy button must be held before releasing it gives the skill.</summary>
        /// <summary>What the player's numbers make of every blow's stamina.</summary>
        [NonSerialized] public float StaminaScale = 1f;
        public float ChargeTime = 0.4f;
        /// <summary>Seconds a press is remembered.</summary>
        public float BufferTime = 0.3f;
        /// <summary>Seconds after an attack ends in which a press still continues the chain.</summary>
        public float ChainGrace = 0.35f;
        /// <summary>Seconds of recovery that must pass before the next attack of the chain can start.</summary>
        public float ChainDelay = 0.03f;

        public AttackPhase Phase { get; private set; }
        /// <summary>The light attack being performed, or the last one performed; -1 before the first and during a heavy or the skill.</summary>
        public int StepIndex { get; private set; } = -1;
        public bool IsAttacking => Phase != AttackPhase.None;
        public bool IsCharging => Phase == AttackPhase.Charge;
        public bool IsFinishing => Current != null && Current == Finisher;
        /// <summary>True once a charge has been held long enough, with the mana to pay, to release the skill.</summary>
        public bool IsCharged => IsCharging && chargeAnnounced;
        /// <summary>A dodge or a guard may cut short a recovery or a charge, never a wind-up or the swing itself.</summary>
        public bool CanBeCancelled => Phase == AttackPhase.Recovery || Phase == AttackPhase.Charge;
        /// <summary>The attack being performed; null when idle or charging.</summary>
        public AttackStep Current { get; private set; }
        /// <summary>How far through the current attack's whole duration the player is, from 0 to 1.</summary>
        public float StepProgress => Current != null && Current.Total > 0f ? Math.Min(1f, elapsed / Current.Total) : 0f;
        /// <summary>How far the charge has come toward <see cref="ChargeTime"/>, from 0 to 1.</summary>
        public float ChargeProgress => IsCharging ? (ChargeTime > 0f ? Math.Min(1f, elapsed / ChargeTime) : 1f) : 0f;

        private float elapsed;
        private float sinceEnd = float.MaxValue;
        private float buffered;
        private float heavyBuffered;
        private bool heavyHeld;
        private bool finisherWanted;
        private bool chargeAnnounced;
        private bool currentIsLight;
        private int nextIndex;

        public void Reset()
        {
            Phase = AttackPhase.None;
            Current = null;
            StepIndex = -1;
            elapsed = 0f;
            sinceEnd = float.MaxValue;
            buffered = 0f;
            heavyBuffered = 0f;
            heavyHeld = false;
            finisherWanted = false;
            chargeAnnounced = false;
            currentIsLight = false;
            nextIndex = 0;
        }

        public void Press() => buffered = BufferTime;

        /// <summary>
        /// The heavy button went down. With <paramref name="finisher"/> true (a staggered enemy is in reach) the press
        /// asks for the finisher instead: it starts at once, with no charge, from idle or from any recovery.
        /// </summary>
        public void PressHeavy(bool finisher = false)
        {
            if (Heavy == null) return;
            heavyBuffered = BufferTime;
            heavyHeld = true;
            finisherWanted = finisher && Finisher != null;
        }

        public void ReleaseHeavy() => heavyHeld = false;

        /// <summary>Stops the current attack or charge where it is. A light chain may still be continued.</summary>
        public void Cancel()
        {
            if (!IsAttacking) return;
            // A press made before the cancel was meant for the attack being abandoned, not for after the dodge.
            buffered = 0f;
            heavyBuffered = 0f;
            EndAttack();
        }

        /// <summary>
        /// Lets time pass while the weapon cannot be used (during a roll, a flinch, a drink): remembered presses still
        /// fade and the chain's grace still runs out, so nothing stale fires afterwards.
        /// </summary>
        public void Idle(float deltaTime)
        {
            if (!IsAttacking && sinceEnd < float.MaxValue) sinceEnd += deltaTime;
            if (buffered > 0f) buffered -= deltaTime;
            if (heavyBuffered > 0f) heavyBuffered -= deltaTime;
        }

        public ComboEvents Tick(float deltaTime, StaminaPool stamina, ManaPool mana = null)
        {
            var events = ComboEvents.None;
            if (Steps.Count == 0) return events;

            if (Phase == AttackPhase.None)
            {
                if (sinceEnd < float.MaxValue) sinceEnd += deltaTime;
                if (sinceEnd > ChainGrace) nextIndex = 0;
                if (heavyBuffered > 0f)
                {
                    if (finisherWanted)
                    {
                        if (StartHeavy(Finisher, stamina)) events |= ComboEvents.Started;
                    }
                    else if (nextIndex > 0)
                    {
                        if (StartHeavy(ChainHeavy ?? Heavy, stamina)) events |= ComboEvents.Started;
                    }
                    else if (!heavyHeld)
                    {
                        // Pressed and let go while something else was happening: there is nothing left to charge.
                        if (StartHeavy(Heavy, stamina)) events |= ComboEvents.Started;
                    }
                    else if (stamina.Current >= Heavy.StaminaCost * StaminaScale)
                    {
                        heavyBuffered = 0f;
                        buffered = 0f;
                        Phase = AttackPhase.Charge;
                        Current = null;
                        StepIndex = -1;
                        elapsed = 0f;
                        chargeAnnounced = false;
                    }
                    else
                    {
                        heavyBuffered = 0f;
                    }
                }
                else if (buffered > 0f && StartLight(stamina))
                {
                    events |= ComboEvents.Started;
                }
            }
            else if (Phase == AttackPhase.Charge)
            {
                elapsed += deltaTime;
                bool ready = elapsed >= ChargeTime;
                bool canPay = Skill != null && mana != null && mana.Current >= SkillManaCost && stamina.Current >= Skill.StaminaCost * StaminaScale;
                if (!heavyHeld)
                {
                    bool started;
                    if (ready && canPay)
                    {
                        mana.TrySpend(SkillManaCost);
                        started = StartHeavy(Skill, stamina);
                    }
                    else
                    {
                        started = StartHeavy(Heavy, stamina);
                    }
                    if (started) events |= ComboEvents.Started;
                    else EndAttack();
                }
                else if (ready && !canPay)
                {
                    // Nothing to charge toward: do not make the player stand and hold for no reason.
                    if (StartHeavy(Heavy, stamina)) events |= ComboEvents.Started;
                    else EndAttack();
                }
                else if (ready && !chargeAnnounced)
                {
                    chargeAnnounced = true;
                    events |= ComboEvents.Charged;
                }
            }
            else
            {
                elapsed += deltaTime;
                var step = Current;
                if (Phase == AttackPhase.Windup && elapsed >= step.Windup)
                {
                    Phase = AttackPhase.Active;
                    events |= ComboEvents.Hit;
                }
                if (Phase == AttackPhase.Active && elapsed >= step.Windup + step.Active) Phase = AttackPhase.Recovery;
                if (Phase == AttackPhase.Recovery)
                {
                    bool open = elapsed >= step.Windup + step.Active + ChainDelay;
                    bool chainOpen = currentIsLight && open;
                    if (heavyBuffered > 0f && finisherWanted && open && StartHeavy(Finisher, stamina)) events |= ComboEvents.Started;
                    else if (heavyBuffered > 0f && !finisherWanted && chainOpen && StartHeavy(ChainHeavy ?? Heavy, stamina)) events |= ComboEvents.Started;
                    else if (buffered > 0f && chainOpen && nextIndex < Steps.Count && StartLight(stamina)) events |= ComboEvents.Started;
                    else if (elapsed >= step.Total) EndAttack();
                }
            }

            if (buffered > 0f) buffered -= deltaTime;
            if (heavyBuffered > 0f) heavyBuffered -= deltaTime;
            return events;
        }

        private bool StartLight(StaminaPool stamina)
        {
            if (nextIndex >= Steps.Count) nextIndex = 0;
            if (!Begin(Steps[nextIndex], stamina)) return false;
            StepIndex = nextIndex;
            nextIndex++;
            currentIsLight = true;
            return true;
        }

        private bool StartHeavy(AttackStep step, StaminaPool stamina)
        {
            if (!Begin(step, stamina)) return false;
            StepIndex = -1;
            // A heavy of any kind is the end of a chain.
            nextIndex = 0;
            currentIsLight = false;
            return true;
        }

        // A press is used up by the attempt, whether or not there was the stamina for it.
        private bool Begin(AttackStep step, StaminaPool stamina)
        {
            buffered = 0f;
            heavyBuffered = 0f;
            finisherWanted = false;
            if (step == null || !stamina.TrySpend(step.StaminaCost * StaminaScale)) return false;
            Current = step;
            Phase = AttackPhase.Windup;
            elapsed = 0f;
            return true;
        }

        private void EndAttack()
        {
            Phase = AttackPhase.None;
            Current = null;
            sinceEnd = 0f;
            // After the last attack the chain is over, whatever is pressed next.
            if (nextIndex >= Steps.Count) nextIndex = 0;
        }
    }
}

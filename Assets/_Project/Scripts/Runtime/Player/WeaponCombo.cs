using System;
using System.Collections.Generic;

namespace Clover.Player
{
    /// <summary>
    /// A weapon's chain of attacks. A press is remembered for a short time, so pressing a little early still attacks; a
    /// press during an attack fires the next one as soon as the swing is over. Stopping for longer than the grace time
    /// starts the chain again from the first attack. Plain C#: the caller feeds it time and acts on what it reports.
    /// </summary>
    [Serializable]
    public class WeaponCombo
    {
        public List<AttackStep> Steps = new List<AttackStep>();
        /// <summary>Seconds a press is remembered.</summary>
        public float BufferTime = 0.3f;
        /// <summary>Seconds after an attack ends in which a press still continues the chain.</summary>
        public float ChainGrace = 0.35f;
        /// <summary>Seconds of recovery that must pass before the next attack of the chain can start.</summary>
        public float ChainDelay = 0.03f;

        public AttackPhase Phase { get; private set; }
        /// <summary>The attack being performed, or the last one performed; -1 before the first.</summary>
        public int StepIndex { get; private set; } = -1;
        public bool IsAttacking => Phase != AttackPhase.None;
        public bool CanBeCancelled => Phase == AttackPhase.Recovery;
        public AttackStep Current => StepIndex >= 0 && StepIndex < Steps.Count ? Steps[StepIndex] : null;
        /// <summary>How far through the current attack's whole duration the player is, from 0 to 1.</summary>
        public float StepProgress => IsAttacking && Current.Total > 0f ? Math.Min(1f, elapsed / Current.Total) : 0f;

        private float elapsed;
        private float sinceEnd = float.MaxValue;
        private float buffered;
        private int nextIndex;

        public void Reset()
        {
            Phase = AttackPhase.None;
            StepIndex = -1;
            elapsed = 0f;
            sinceEnd = float.MaxValue;
            buffered = 0f;
            nextIndex = 0;
        }

        public void Press() => buffered = BufferTime;

        /// <summary>Stops the current attack where it is (a dodge out of recovery). The chain may still be continued.</summary>
        public void Cancel()
        {
            if (!IsAttacking) return;
            // A press made before the cancel was meant for the attack being abandoned, not for after the dodge.
            buffered = 0f;
            EndAttack();
        }

        /// <summary>
        /// Lets time pass while the weapon cannot be used (during a roll): a remembered press still fades and the chain's
        /// grace still runs out, so nothing stale fires when the roll ends.
        /// </summary>
        public void Idle(float deltaTime)
        {
            if (!IsAttacking && sinceEnd < float.MaxValue) sinceEnd += deltaTime;
            if (buffered > 0f) buffered -= deltaTime;
        }

        public ComboEvents Tick(float deltaTime, StaminaPool stamina)
        {
            var events = ComboEvents.None;
            if (Steps.Count == 0) return events;

            if (Phase == AttackPhase.None)
            {
                if (sinceEnd < float.MaxValue) sinceEnd += deltaTime;
                if (sinceEnd > ChainGrace) nextIndex = 0;
                if (buffered > 0f && TryStart(stamina)) events |= ComboEvents.Started;
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
                    bool chainOpen = nextIndex < Steps.Count && elapsed >= step.Windup + step.Active + ChainDelay;
                    if (buffered > 0f && chainOpen && TryStart(stamina)) events |= ComboEvents.Started;
                    else if (elapsed >= step.Total) EndAttack();
                }
            }

            if (buffered > 0f) buffered -= deltaTime;
            return events;
        }

        private bool TryStart(StaminaPool stamina)
        {
            buffered = 0f;
            if (nextIndex >= Steps.Count) nextIndex = 0;
            if (!stamina.TrySpend(Steps[nextIndex].StaminaCost)) return false;
            StepIndex = nextIndex;
            nextIndex++;
            Phase = AttackPhase.Windup;
            elapsed = 0f;
            return true;
        }

        private void EndAttack()
        {
            Phase = AttackPhase.None;
            sinceEnd = 0f;
            // After the last attack the chain is over, whatever is pressed next.
            if (nextIndex >= Steps.Count) nextIndex = 0;
        }
    }
}

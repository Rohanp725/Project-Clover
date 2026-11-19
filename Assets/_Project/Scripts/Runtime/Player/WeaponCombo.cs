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
        /// <summary>What the skill's mana cost is multiplied by now (0: free), and whether it comes at once with no time to charge. The player's upgrades keep both.</summary>
        public float SkillCostScale = 1f;
        public bool SkillAtOnce;
        public float SkillPrice => SkillManaCost * SkillCostScale;
        /// <summary>The move the heavy button gives when a staggered enemy is in reach. Null: the plain heavy is used.</summary>
        public AttackStep Finisher;
        /// <summary>Seconds the heavy button must be held before releasing it gives the skill.</summary>
        /// <summary>What the player's numbers make of every blow's stamina.</summary>
        [NonSerialized] public float StaminaScale = 1f;
        public float ChargeTime = 0.4f;
        /// <summary>A second heavy, free of mana, released from a charge held to <see cref="ChargeTime"/>. Null: the weapon has one tier, the skill.</summary>
        public AttackStep ChargedHeavy;
        /// <summary>With a <see cref="ChargedHeavy"/>: seconds the button must be held for the skill. Zero means <see cref="ChargeTime"/>.</summary>
        public float SkillChargeTime;
        /// <summary>The share of walking speed kept while charging; above zero the stick also turns the player.</summary>
        public float ChargeMoveFactor;
        /// <summary>The skill is a counter: it is not released, it answers a blow taken while the charge is held (see <see cref="Counter"/>).</summary>
        public bool SkillIsCounter;
        /// <summary>Seconds a counter's stance can be held past <see cref="ChargeTime"/> before it lets go by itself as the <see cref="ChargedHeavy"/>.</summary>
        public float CounterHoldMax = 1.2f;
        /// <summary>The heavy that ends the chain after this many lights (index 1 is after one). A null entry, or none, falls to <see cref="ChainHeavy"/>.</summary>
        public List<AttackStep> ChainHeavies;
        /// <summary>A step started by a light press made in the second half of a dodge: it begins as the dodge ends. It is no part of the chain. Null: the press is an ordinary light.</summary>
        public AttackStep DashAttack;
        /// <summary>A step started by a light press behind a raised guard, which stays up through it. It is no part of the chain. Null: the press is an ordinary light.</summary>
        public AttackStep GuardAttack;
        /// <summary>True while the guard's own attack is being made: the one attack the guard does not come down for.</summary>
        public bool IsGuardAttack => Current != null && Current == GuardAttack;
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
        /// <summary>How far the held charge has come: 0 not yet, 1 the charged heavy (or the skill, on a weapon with one tier), 2 the skill.</summary>
        public int ChargeTier => !IsCharging ? 0 : skillAnnounced ? 2 : chargeAnnounced ? 1 : 0;
        /// <summary>How many repeats of the current step have been struck.</summary>
        public int RepeatsDone { get; private set; }
        /// <summary>Seconds into the current attack or charge.</summary>
        public float Elapsed => elapsed;
        private float SkillAt => ChargedHeavy != null && SkillChargeTime > ChargeTime ? SkillChargeTime : ChargeTime;
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
        private bool guardWanted;
        private bool dashWanted;
        private bool chargeAnnounced;
        private bool skillAnnounced;
        private bool currentIsLight;
        private int nextIndex;

        /// <summary>While true the heavy button gives the chain's heavy from a standing start too: the Chain Sickle with an enemy bound.</summary>
        [NonSerialized] public bool EnderAlways;

        /// <summary>
        /// The skill has its own button (Triangle, Y, Q: STEEL_AND_ELEMENT.md 3.5), pressed through <see cref="PressSkill"/>.
        /// A held heavy then charges only toward the charged heavy and never releases the skill; a weapon with no charged
        /// heavy swings its heavy at once. A counter's stance is raised by holding either button. The player sets it;
        /// left false, the combo keeps the old rule (the skill from a full charge).
        /// </summary>
        [NonSerialized] public bool SkillOnButton;

        /// <summary>True when the skill could be started now with this mana and stamina.</summary>
        public bool CanPaySkill(StaminaPool stamina, ManaPool mana) =>
            Skill != null && mana != null && mana.Current >= SkillPrice && stamina != null && stamina.Current >= Skill.StaminaCost * StaminaScale;

        /// <summary>The skill button went down: the skill starts from idle or from a recovery, if it can be paid for.</summary>
        public void PressSkill()
        {
            if (Skill == null) return;
            skillBuffered = BufferTime;
            skillHeld = true;
        }

        public void ReleaseSkill() => skillHeld = false;

        private bool Held => heavyHeld || skillHeld;
        private float skillBuffered;
        private bool skillHeld;
        // The heavy button alone is worth holding: there is a charged heavy, a counter's stance, or (the old rule) the skill.
        private bool ChargeWorthHolding => !SkillOnButton || ChargedHeavy != null || SkillIsCounter;

        public void Reset()
        {
            EnderAlways = false;
            Phase = AttackPhase.None;
            Current = null;
            StepIndex = -1;
            elapsed = 0f;
            sinceEnd = float.MaxValue;
            buffered = 0f;
            heavyBuffered = 0f;
            heavyHeld = false;
            skillBuffered = 0f;
            skillHeld = false;
            finisherWanted = false;
            guardWanted = false;
            dashWanted = false;
            chargeAnnounced = false;
            skillAnnounced = false;
            currentIsLight = false;
            nextIndex = 0;
            RepeatsDone = 0;
        }

        // The heavy that ends a chain of this many lights.
        private AttackStep Ender(int lights)
        {
            if (ChainHeavies != null && lights < ChainHeavies.Count && ChainHeavies[lights] != null) return ChainHeavies[lights];
            return ChainHeavy ?? Heavy;
        }

        /// <summary>
        /// A blow has reached the player while a counter's stance is held and ready. With the mana to pay, the mana is
        /// spent and the skill starts at once; true means the blow was taken on the stance.
        /// </summary>
        public bool Counter(StaminaPool stamina, ManaPool mana)
        {
            if (!SkillIsCounter || Skill == null || !IsCharged || mana == null || mana.Current < SkillPrice) return false;
            mana.TrySpend(SkillPrice);
            if (!StartHeavy(Skill, stamina))
            {
                EndAttack();
                return false;
            }
            return true;
        }

        /// <summary>The light button went down; <paramref name="guarded"/> says the guard was up, which a weapon with a <see cref="GuardAttack"/> answers with it.</summary>
        public void Press(bool guarded = false, bool dodging = false)
        {
            buffered = BufferTime;
            guardWanted = guarded && GuardAttack != null;
            dashWanted = dodging && DashAttack != null;
        }

        /// <summary>
        /// Starts a step that follows from something else than a press (the cut that ends a run to a thrown spear).
        /// Not while a wind-up or a swing is in progress. True when it started.
        /// </summary>
        public bool Follow(AttackStep step, StaminaPool stamina)
        {
            if (step == null || Phase == AttackPhase.Windup || Phase == AttackPhase.Active) return false;
            return StartHeavy(step, stamina);
        }

        /// <summary>
        /// Puts another step in place of a light step still in its wind-up (the technique chord, IMPLEMENTATION.md 3.5):
        /// the light is dropped where it is and the new step starts at once, so the chord never waits on the light.
        /// Refused outside a light's wind-up. True when it started.
        /// </summary>
        public bool Replace(AttackStep step, StaminaPool stamina)
        {
            if (step == null || Phase != AttackPhase.Windup || !currentIsLight) return false;
            int place = nextIndex;
            if (!StartHeavy(step, stamina)) return false;
            // The light's place in the chain is given back: the chord is not a link of it.
            nextIndex = place > 0 ? place - 1 : 0;
            return true;
        }

        /// <summary>True while the step in progress is a light of the chain, winding up: the chord may still take it.</summary>
        public bool LightWindingUp => Phase == AttackPhase.Windup && currentIsLight;

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
            skillBuffered = 0f;
            dashWanted = false;
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
            if (skillBuffered > 0f) skillBuffered -= deltaTime;
        }

        public ComboEvents Tick(float deltaTime, StaminaPool stamina, ManaPool mana = null)
        {
            var events = ComboEvents.None;
            if (Steps.Count == 0) return events;

            if (Phase == AttackPhase.None)
            {
                if (sinceEnd < float.MaxValue) sinceEnd += deltaTime;
                if (sinceEnd > ChainGrace) nextIndex = 0;
                if (skillBuffered > 0f && SkillOnButton && Skill != null)
                {
                    if (SkillIsCounter)
                    {
                        // A counter is a stance: the skill button raises it as a held heavy does.
                        skillBuffered = 0f;
                        if (stamina.Current >= Heavy.StaminaCost * StaminaScale) BeginCharge();
                    }
                    else if (CanPaySkill(stamina, mana))
                    {
                        mana.TrySpend(SkillPrice);
                        if (StartHeavy(Skill, stamina)) events |= ComboEvents.Started;
                    }
                    else skillBuffered = 0f;
                }
                else if (heavyBuffered > 0f)
                {
                    if (finisherWanted)
                    {
                        if (StartHeavy(Finisher, stamina)) events |= ComboEvents.Started;
                    }
                    else if (nextIndex > 0 || (EnderAlways && ChainHeavy != null))
                    {
                        if (StartHeavy(nextIndex > 0 ? Ender(nextIndex) : ChainHeavy, stamina)) events |= ComboEvents.Started;
                    }
                    else if (!heavyHeld || !ChargeWorthHolding)
                    {
                        // Pressed and let go while something else was happening, or nothing to charge toward: the heavy at once.
                        if (StartHeavy(Heavy, stamina)) events |= ComboEvents.Started;
                    }
                    else if (stamina.Current >= Heavy.StaminaCost * StaminaScale)
                    {
                        BeginCharge();
                    }
                    else
                    {
                        heavyBuffered = 0f;
                    }
                }
                else if (buffered > 0f && guardWanted)
                {
                    // From behind the guard: its own thrust, which neither starts nor continues a chain.
                    if (StartHeavy(GuardAttack, stamina)) events |= ComboEvents.Started;
                }
                else if (buffered > 0f && dashWanted)
                {
                    // Out of the dodge: its own cut, which neither starts nor continues a chain.
                    if (StartHeavy(DashAttack, stamina)) events |= ComboEvents.Started;
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
                // With the skill on its own button a charge never releases it (a counter's stance still answers a blow).
                bool canPay = (!SkillOnButton || SkillIsCounter) && CanPaySkill(stamina, mana);
                if (ChargedHeavy != null)
                {
                    // Two tiers: the charged heavy for nothing, then the skill for mana. A counter's skill is never
                    // released: it waits for a blow, and lets go by itself when it has been held too long.
                    bool skillReady = !SkillIsCounter && (elapsed >= SkillAt || SkillAtOnce) && canPay;
                    bool spent = SkillIsCounter && elapsed >= ChargeTime + CounterHoldMax;
                    if (!Held || spent || (SkillAtOnce && skillReady))
                    {
                        bool started;
                        if (skillReady)
                        {
                            mana.TrySpend(SkillPrice);
                            started = StartHeavy(Skill, stamina);
                        }
                        else started = StartHeavy(ready ? ChargedHeavy : Heavy, stamina);
                        if (started) events |= ComboEvents.Started;
                        else EndAttack();
                    }
                    else
                    {
                        if (ready && !chargeAnnounced)
                        {
                            chargeAnnounced = true;
                            events |= ComboEvents.Charged;
                        }
                        if (skillReady && !skillAnnounced)
                        {
                            skillAnnounced = true;
                            events |= ComboEvents.SkillReady;
                        }
                    }
                }
                else if (!Held || (SkillAtOnce && canPay))
                {
                    bool started;
                    if ((ready || SkillAtOnce) && canPay)
                    {
                        mana.TrySpend(SkillPrice);
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
                    skillAnnounced = true;
                    events |= ComboEvents.Charged | ComboEvents.SkillReady;
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
                // A step of several blows strikes again at each of its moments, while the swing is still out.
                if (Phase == AttackPhase.Active && step.Repeats != null && RepeatsDone < step.Repeats.Length
                    && elapsed >= step.Windup + step.Repeats[RepeatsDone])
                {
                    RepeatsDone++;
                    events |= ComboEvents.Repeat;
                }
                if (Phase == AttackPhase.Active && elapsed >= step.Windup + step.Active) Phase = AttackPhase.Recovery;
                if (Phase == AttackPhase.Recovery)
                {
                    bool open = elapsed >= step.Windup + step.Active + ChainDelay;
                    bool chainOpen = currentIsLight && open;
                    if (heavyBuffered > 0f && finisherWanted && open && StartHeavy(Finisher, stamina)) events |= ComboEvents.Started;
                    else if (skillBuffered > 0f && SkillOnButton && !SkillIsCounter && open && CanPaySkill(stamina, mana) && mana.TrySpend(SkillPrice) && StartHeavy(Skill, stamina)) events |= ComboEvents.Started;
                    else if (heavyBuffered > 0f && !finisherWanted && chainOpen && StartHeavy(Ender(nextIndex), stamina)) events |= ComboEvents.Started;
                    else if (buffered > 0f && guardWanted && open && step == GuardAttack && StartHeavy(GuardAttack, stamina)) events |= ComboEvents.Started;
                    else if (buffered > 0f && chainOpen && nextIndex < Steps.Count && StartLight(stamina)) events |= ComboEvents.Started;
                    else if (elapsed >= step.Total) EndAttack();
                }
            }

            if (buffered > 0f) buffered -= deltaTime;
            if (heavyBuffered > 0f) heavyBuffered -= deltaTime;
            if (skillBuffered > 0f) skillBuffered -= deltaTime;
            return events;
        }

        private void BeginCharge()
        {
            heavyBuffered = 0f;
            skillBuffered = 0f;
            buffered = 0f;
            Phase = AttackPhase.Charge;
            Current = null;
            StepIndex = -1;
            elapsed = 0f;
            chargeAnnounced = false;
            skillAnnounced = false;
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
            skillBuffered = 0f;
            finisherWanted = false;
            guardWanted = false;
            dashWanted = false;
            if (step == null || !stamina.TrySpend(step.StaminaCost * StaminaScale)) return false;
            Current = step;
            Phase = AttackPhase.Windup;
            elapsed = 0f;
            RepeatsDone = 0;
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

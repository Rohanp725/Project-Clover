using System;
using System.Collections.Generic;

namespace Clover.Gameplay
{
    /// <summary>
    /// What an ordinary enemy decides: notice the target, keep its preferred distance, pick an attack that suits the
    /// range, show it, strike, recover, rest a moment, and again. Blows can make it flinch out of anything but a strike
    /// already shown, and a full stagger meter leaves it helpless. Plain C#: the enemy feeds it time and the distance to
    /// the target, moves as it asks, and deals the blow when it says so.
    /// </summary>
    [Serializable]
    public class EnemyBrain
    {
        public List<EnemyAttack> Attacks = new List<EnemyAttack>();
        /// <summary>The target is noticed inside this distance and forgotten beyond the leash.</summary>
        public float AggroRange = 11f;
        public float LeashRange = 26f;
        /// <summary>The enemy closes in until the target is this near, and backs away if it comes nearer than the retreat range.</summary>
        public float PreferredRange = 2.2f;
        public float RetreatRange;
        /// <summary>Seconds of rest between attacks, picked between these two.</summary>
        public float RestMin = 0.5f;
        public float RestMax = 1.3f;
        public float FlinchSeconds = 0.3f;
        /// <summary>While another enemy has the attack, this one keeps this far back and circles.</summary>
        public float WaitRange = 5.5f;
        /// <summary>The attack it answers with when it has turned enough blows on its guard and the target is in its reach; empty for whatever suits the range.</summary>
        public string CounterAttack = "";
        /// <summary>Above zero: with the target farther off than this, its rest between attacks runs out this many times faster, so hanging back is no rest from it.</summary>
        public float ImpatientBeyond;
        public float ImpatientRate = 3f;
        /// <summary>For an enemy that keeps to its own ground: seconds the target may be off that ground before it gives the chase up.</summary>
        public float GiveUpSeconds = 3f;
        /// <summary>Struck from off its ground, it forgets the ground for this long and comes after whoever struck it.</summary>
        public float ProvokedSeconds = 8f;

        /// <summary>The phase of the fight, for enemies that change as they are worn down; attacks may wait for a later one.</summary>
        public int Phase;
        /// <summary>Set by the enemy each frame: whether the target is found standing in the sun, and how many flasks it carries.</summary>
        [NonSerialized] public bool TargetLit;
        [NonSerialized] public int TargetFlasks;
        /// <summary>The conditions on the body this brain drives (its afflictions' own), read once at the top of each step; none for a brain with no body.</summary>
        [NonSerialized] public EnemyConditions Conditions;

        public EnemyState State { get; private set; }
        public EnemyAttack Current { get; private set; }
        public bool IsAttacking => State == EnemyState.Telegraph || State == EnemyState.Strike || State == EnemyState.Recover;
        /// <summary>How far through the whole of the current attack the enemy is, from 0 to 1.</summary>
        public float AttackProgress => IsAttacking && Current.Total > 0f ? Math.Min(1f, attackElapsed / Current.Total) : 0f;
        public float TelegraphProgress => State == EnemyState.Telegraph && Current.Telegraph > 0f ? Math.Min(1f, attackElapsed / Current.Telegraph) : 0f;
        /// <summary>How far through a flinch, from 0 to 1.</summary>
        public float FlinchProgress => State == EnemyState.Flinch && flinchFor > 0f ? Math.Min(1f, stateElapsed / flinchFor) : 0f;
        /// <summary>Seconds spent in the current state.</summary>
        public float StateSeconds => stateElapsed;
        /// <summary>Seconds until the blow it shows lands, at its own pace; float.MaxValue when it shows none.</summary>
        public float StrikeIn => State == EnemyState.Telegraph && Current != null ? Math.Max(0f, Current.Telegraph - attackElapsed) : float.MaxValue;
        /// <summary>+1 to walk toward the target, -1 to back away, 0 to stand.</summary>
        public int MoveWish { get; private set; }
        /// <summary>True while the enemy is holding back because it is not its turn to attack.</summary>
        public bool IsWaiting { get; private set; }
        /// <summary>True while the enemy should keep turning to face the target.</summary>
        public bool Tracks => State == EnemyState.Stalk || (State == EnemyState.Telegraph && TelegraphProgress < Current.AimLockAt);
        /// <summary>True while a blow from off its ground has it chasing wherever the target goes.</summary>
        public bool IsProvoked => provokedLeft > 0f;

        private float stateElapsed;
        private float attackElapsed;
        private float rest;
        private bool counter;
        private float offGround;
        private float provokedLeft;
        private float busyFor;
        private float flinchFor;
        private float[] cooldowns = new float[0];
        // Its feet caught this step (ConditionFlags.Mired): no leap or blink is chosen.
        private bool mired;

        /// <summary>How far through whatever it is busy with, from 0 to 1; 0 when the time was left open.</summary>
        public float BusyProgress => State == EnemyState.Busy && busyFor > 0f && busyFor < float.MaxValue ? Math.Min(1f, stateElapsed / busyFor) : 0f;

        /// <summary>
        /// Seconds the fall onto its back takes, one breath while it lies there, and getting up again (task 3.7). The
        /// Forge makes the <c>_knockdown</c>, <c>_down_loop</c> and <c>_getup</c> clips to these lengths.
        /// </summary>
        public const float FallSeconds = 0.45f;
        public const float DownBreathSeconds = 2.4f;
        public const float GetUpSeconds = 0.6f;
        /// <summary>True while it is knocked flat: falling, lying, or getting up.</summary>
        public bool IsDown => State == EnemyState.Down;
        /// <summary>True once its Down condition is over and it is getting to its feet.</summary>
        public bool IsGettingUp => State == EnemyState.Down && gettingUp >= 0f;
        /// <summary>How far through getting up, from 0 to 1.</summary>
        public float GetUpProgress => IsGettingUp ? Math.Min(1f, gettingUp / GetUpSeconds) : 0f;

        // Seconds into getting up, or below nought while it still lies (or is not down at all).
        private float gettingUp = -1f;

        /// <summary>
        /// Takes it up with something that is not an attack for this many seconds, or until <see cref="EndBusy"/>.
        /// Blows do not make it flinch out of it; a broken balance and death do end it. False when it is dead or staggered.
        /// </summary>
        public bool Occupy(float seconds = float.MaxValue)
        {
            if (State == EnemyState.Dead || State == EnemyState.Staggered || State == EnemyState.Down) return false;
            Current = null;
            Enter(EnemyState.Busy);
            busyFor = seconds;
            return true;
        }

        /// <summary>Whatever it was busy with is over: back to the fight, after this much rest.</summary>
        public void EndBusy(float restAfter = 0.3f)
        {
            if (State != EnemyState.Busy) return;
            Enter(EnemyState.Stalk);
            rest = restAfter;
        }

        public void Reset()
        {
            State = EnemyState.Idle;
            Current = null;
            MoveWish = 0;
            stateElapsed = 0f;
            attackElapsed = 0f;
            rest = 0f;
            counter = false;
            offGround = 0f;
            provokedLeft = 0f;
            IsWaiting = false;
            Phase = 0;
            gettingUp = -1f;
            cooldowns = new float[Attacks.Count];
        }

        /// <summary>
        /// Advances the brain. <paramref name="roll"/> is a random number from 0 to 1, used when a choice is made.
        /// Returns true on the tick the current attack's blow lands. <paramref name="targetOnGround"/> is false when
        /// the enemy keeps to a ground of its own and the target is standing off it.
        /// </summary>
        public bool Tick(float deltaTime, float distance, float roll, bool mayAttack = true, bool targetOnGround = true)
        {
            // The conditions on it, read once (IMPLEMENTATION.md 3.7): rooted it walks nowhere, breathless it begins no
            // attack, its wind-ups may drag, and with its sight cut short whoever stands farther off is lost to it.
            var held = Conditions != null ? Conditions.Flags : ConditionFlags.None;
            bool rooted = (held & ConditionFlags.Rooted) != 0;
            bool noAttack = (held & ConditionFlags.BeginsNoAttack) != 0;
            float windup = (held & ConditionFlags.WindupSlowed) != 0 ? Conditions.WindupPace : 1f;
            bool unseen = (held & ConditionFlags.SightShort) != 0 && distance > Conditions.Sight;
            // Knocked flat it goes down out of whatever it was doing, the blow it had begun included; one getting up
            // goes down again. A broken balance is left to run (it is as helpless), and it goes down after if there is time.
            bool down = (held & ConditionFlags.Down) != 0;
            if (down && !(State == EnemyState.Down && gettingUp < 0f) && State != EnemyState.Dead && State != EnemyState.Staggered)
            {
                Current = null;
                counter = false;
                Enter(EnemyState.Down);
            }
            mired = (held & ConditionFlags.Mired) != 0;
            bool struck = Step(deltaTime, distance, roll, mayAttack, targetOnGround, windup, unseen, noAttack || unseen, down);
            if (rooted || unseen) MoveWish = 0;
            // Its feet caught, it does not step back: it holds where it is and lets the target come.
            if (mired && MoveWish < 0) MoveWish = 0;
            return struck;
        }

        private bool Step(float deltaTime, float distance, float roll, bool mayAttack, bool targetOnGround, float windup, bool unseen, bool holdsOff, bool down)
        {
            IsWaiting = false;
            stateElapsed += deltaTime;
            if (provokedLeft > 0f) provokedLeft -= deltaTime;
            bool mayChase = targetOnGround || provokedLeft > 0f;
            // Counted whatever it is doing, so an attack begun as the target left does not buy the chase more time.
            if (mayChase) offGround = 0f;
            else offGround += deltaTime;
            for (int i = 0; i < cooldowns.Length; i++)
                if (cooldowns[i] > 0f) cooldowns[i] -= deltaTime;
            MoveWish = 0;

            switch (State)
            {
                case EnemyState.Idle:
                    if (distance <= AggroRange && mayChase && !unseen) Enter(EnemyState.Stalk);
                    return false;

                case EnemyState.Stalk:
                    if (distance > LeashRange)
                    {
                        Enter(EnemyState.Idle);
                        return false;
                    }
                    // The target has left its ground: it watches from the edge a while, then lets them go.
                    if (offGround >= GiveUpSeconds)
                    {
                        counter = false;
                        Enter(EnemyState.Idle);
                        return false;
                    }
                    // One that will not be stood off from is done resting sooner when the target hangs back.
                    rest -= ImpatientBeyond > 0f && distance > ImpatientBeyond ? deltaTime * ImpatientRate : deltaTime;
                    if (!mayAttack && !counter)
                    {
                        // Not its turn: hold a ring around the fight until it is.
                        IsWaiting = true;
                        float ring = Math.Max(WaitRange, PreferredRange);
                        if (distance > ring + 0.6f) MoveWish = 1;
                        else if (distance < ring - 0.6f) MoveWish = -1;
                        return false;
                    }
                    if ((rest <= 0f || counter) && !holdsOff)
                    {
                        // Its answer to blows turned is the one it keeps for that, when the target is close enough for it.
                        var answer = counter ? Named(CounterAttack) : null;
                        counter = false;
                        var attack = answer != null && distance <= answer.MaxRange ? answer : Choose(distance, roll);
                        if (attack != null)
                        {
                            Begin(attack);
                            return false;
                        }
                    }
                    if (RetreatRange > 0f && distance < RetreatRange) MoveWish = -1;
                    else if (distance > PreferredRange) MoveWish = 1;
                    return false;

                case EnemyState.Telegraph:
                    attackElapsed += deltaTime * windup;
                    if (attackElapsed < Current.Telegraph) return false;
                    State = EnemyState.Strike;
                    stateElapsed = 0f;
                    return true;

                case EnemyState.Strike:
                    attackElapsed += deltaTime;
                    if (attackElapsed >= Current.Telegraph + Current.Strike)
                    {
                        // A chained attack begins the moment the first one's blow is through.
                        var next = Named(Current.ChainInto);
                        if (next != null) Begin(next);
                        else
                        {
                            State = EnemyState.Recover;
                            stateElapsed = 0f;
                        }
                    }
                    return false;

                case EnemyState.Recover:
                    attackElapsed += deltaTime;
                    if (attackElapsed >= Current.Total) EndAttack(roll);
                    return false;

                case EnemyState.Busy:
                    if (stateElapsed >= busyFor) EndBusy();
                    return false;

                case EnemyState.Down:
                    // It lies for as long as the condition lasts, and never less than its fall; then it gets up and is
                    // back in the fight, with a moment's rest before its first attack.
                    if (down || stateElapsed < FallSeconds) return false;
                    gettingUp = (gettingUp < 0f ? 0f : gettingUp) + deltaTime;
                    if (gettingUp >= GetUpSeconds)
                    {
                        Enter(EnemyState.Stalk);
                        rest = Math.Max(rest, 0.4f);
                    }
                    return false;

                case EnemyState.Flinch:
                    if (stateElapsed >= flinchFor)
                    {
                        Enter(EnemyState.Stalk);
                        rest = Math.Max(rest, 0.15f);
                    }
                    return false;

                default:
                    return false;
            }
        }

        /// <summary>
        /// A blow landed on the enemy. It is knocked out of walking, resting and recovering by anything, and out of an
        /// attack it is still showing only by a heavy blow. A blow already struck is never taken back.
        /// </summary>
        public bool Flinch(bool heavy, float seconds = 0f)
        {
            if (State == EnemyState.Dead || State == EnemyState.Staggered || State == EnemyState.Strike || State == EnemyState.Busy || State == EnemyState.Down) return false;
            if (State == EnemyState.Telegraph && !heavy) return false;
            if (State == EnemyState.Idle) rest = 0f;
            Current = null;
            Enter(EnemyState.Flinch);
            // A flinch of an effect's own length (a reaction's); a blow's is the brain's usual.
            flinchFor = seconds > 0f ? seconds : FlinchSeconds;
            return true;
        }

        /// <summary>
        /// The enemy has turned enough blows on its guard: it answers at once with whatever attack suits the range,
        /// whoever's turn it is.
        /// </summary>
        public void Counter()
        {
            if (State == EnemyState.Stalk || State == EnemyState.Idle) counter = true;
            Alert();
        }

        /// <summary>Begins the named attack at once, whatever the enemy was doing, unless it is dead, staggered or knocked flat.</summary>
        public bool Force(string attackName)
        {
            if (State == EnemyState.Dead || State == EnemyState.Staggered || State == EnemyState.Down) return false;
            var attack = Named(attackName);
            if (attack == null) return false;
            Begin(attack);
            return true;
        }

        /// <summary>The stagger meter filled: the enemy drops whatever it was doing.</summary>
        public void Stagger()
        {
            if (State == EnemyState.Dead) return;
            Current = null;
            Enter(EnemyState.Staggered);
        }

        /// <summary>The stagger is over, by time or by the finisher.</summary>
        public void EndStagger()
        {
            if (State != EnemyState.Staggered) return;
            Enter(EnemyState.Stalk);
            rest = 0.4f;
        }

        public void Die()
        {
            Current = null;
            Enter(EnemyState.Dead);
        }

        /// <summary>Makes the enemy notice the target now, as when it is struck from out of sight.</summary>
        public void Alert()
        {
            offGround = 0f;
            if (State == EnemyState.Idle) Enter(EnemyState.Stalk);
        }

        /// <summary>
        /// Struck by a target standing off its ground: for a while the ground no longer holds it, so the edge of an
        /// enemy's ground is not a place to hit it from in safety.
        /// </summary>
        public void Provoke()
        {
            provokedLeft = ProvokedSeconds;
            Alert();
        }

        private void Enter(EnemyState state)
        {
            State = state;
            stateElapsed = 0f;
            MoveWish = 0;
            gettingUp = -1f;
        }

        private void Begin(EnemyAttack attack)
        {
            Current = attack;
            State = EnemyState.Telegraph;
            stateElapsed = 0f;
            attackElapsed = 0f;
            int index = Attacks.IndexOf(attack);
            if (index >= 0 && index < cooldowns.Length) cooldowns[index] = attack.Cooldown + attack.Total;
        }

        private void EndAttack(float roll)
        {
            Current = null;
            Enter(EnemyState.Stalk);
            rest = RestMin + (RestMax - RestMin) * roll;
        }

        private EnemyAttack Named(string name)
        {
            if (string.IsNullOrEmpty(name)) return null;
            for (int i = 0; i < Attacks.Count; i++)
                if (Attacks[i].Name == name) return Attacks[i];
            return null;
        }

        // Among the attacks that suit the distance and are off cooldown, the roll picks one.
        private EnemyAttack Choose(float distance, float roll)
        {
            int fitting = 0;
            for (int i = 0; i < Attacks.Count; i++)
                if (Fits(i, distance)) fitting++;
            if (fitting == 0) return null;
            int pick = Math.Min(fitting - 1, (int)(roll * fitting));
            for (int i = 0; i < Attacks.Count; i++)
            {
                if (!Fits(i, distance)) continue;
                if (pick == 0) return Attacks[i];
                pick--;
            }
            return null;
        }

        private bool Fits(int index, float distance)
        {
            var attack = Attacks[index];
            if (index < cooldowns.Length && cooldowns[index] > 0f) return false;
            if (attack.MinPhase > Phase) return false;
            if (attack.OnlyLit && !TargetLit) return false;
            if (attack.NeedsFlasks > TargetFlasks) return false;
            if (mired && (attack.Leap || attack.BlinkTo > 0f)) return false;
            return distance >= attack.MinRange && distance <= attack.MaxRange;
        }
    }
}

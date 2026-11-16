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

        public EnemyState State { get; private set; }
        public EnemyAttack Current { get; private set; }
        public bool IsAttacking => State == EnemyState.Telegraph || State == EnemyState.Strike || State == EnemyState.Recover;
        /// <summary>How far through the whole of the current attack the enemy is, from 0 to 1.</summary>
        public float AttackProgress => IsAttacking && Current.Total > 0f ? Math.Min(1f, attackElapsed / Current.Total) : 0f;
        public float TelegraphProgress => State == EnemyState.Telegraph && Current.Telegraph > 0f ? Math.Min(1f, attackElapsed / Current.Telegraph) : 0f;
        /// <summary>How far through a flinch, from 0 to 1.</summary>
        public float FlinchProgress => State == EnemyState.Flinch && FlinchSeconds > 0f ? Math.Min(1f, stateElapsed / FlinchSeconds) : 0f;
        /// <summary>Seconds spent in the current state.</summary>
        public float StateSeconds => stateElapsed;
        /// <summary>+1 to walk toward the target, -1 to back away, 0 to stand.</summary>
        public int MoveWish { get; private set; }
        /// <summary>True while the enemy is holding back because it is not its turn to attack.</summary>
        public bool IsWaiting { get; private set; }
        /// <summary>True while the enemy should keep turning to face the target.</summary>
        public bool Tracks => State == EnemyState.Stalk || (State == EnemyState.Telegraph && TelegraphProgress < Current.AimLockAt);

        private float stateElapsed;
        private float attackElapsed;
        private float rest;
        private bool counter;
        private float[] cooldowns = new float[0];

        public void Reset()
        {
            State = EnemyState.Idle;
            Current = null;
            MoveWish = 0;
            stateElapsed = 0f;
            attackElapsed = 0f;
            rest = 0f;
            counter = false;
            IsWaiting = false;
            cooldowns = new float[Attacks.Count];
        }

        /// <summary>
        /// Advances the brain. <paramref name="roll"/> is a random number from 0 to 1, used when a choice is made.
        /// Returns true on the tick the current attack's blow lands.
        /// </summary>
        public bool Tick(float deltaTime, float distance, float roll, bool mayAttack = true)
        {
            IsWaiting = false;
            stateElapsed += deltaTime;
            for (int i = 0; i < cooldowns.Length; i++)
                if (cooldowns[i] > 0f) cooldowns[i] -= deltaTime;
            MoveWish = 0;

            switch (State)
            {
                case EnemyState.Idle:
                    if (distance <= AggroRange) Enter(EnemyState.Stalk);
                    return false;

                case EnemyState.Stalk:
                    if (distance > LeashRange)
                    {
                        Enter(EnemyState.Idle);
                        return false;
                    }
                    rest -= deltaTime;
                    if (!mayAttack && !counter)
                    {
                        // Not its turn: hold a ring around the fight until it is.
                        IsWaiting = true;
                        float ring = Math.Max(WaitRange, PreferredRange);
                        if (distance > ring + 0.6f) MoveWish = 1;
                        else if (distance < ring - 0.6f) MoveWish = -1;
                        return false;
                    }
                    if (rest <= 0f || counter)
                    {
                        counter = false;
                        var attack = Choose(distance, roll);
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
                    attackElapsed += deltaTime;
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

                case EnemyState.Flinch:
                    if (stateElapsed >= FlinchSeconds)
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
        public bool Flinch(bool heavy)
        {
            if (State == EnemyState.Dead || State == EnemyState.Staggered || State == EnemyState.Strike) return false;
            if (State == EnemyState.Telegraph && !heavy) return false;
            if (State == EnemyState.Idle) rest = 0f;
            Current = null;
            Enter(EnemyState.Flinch);
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
            if (State == EnemyState.Idle) Enter(EnemyState.Stalk);
        }

        private void Enter(EnemyState state)
        {
            State = state;
            stateElapsed = 0f;
            MoveWish = 0;
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
            return distance >= attack.MinRange && distance <= attack.MaxRange;
        }
    }
}

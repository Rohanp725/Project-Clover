using System;

namespace Clover.Gameplay
{
    /// <summary>
    /// Every affliction lying on one enemy: how deep each is, how long it has left, and what all of them together do
    /// to it. Plain rules with no scene in them. Burning keeps its own engine (<see cref="BurnStatus"/>), which this
    /// holds and counts with the rest.
    /// </summary>
    public class AfflictionSet
    {
        /// <summary>Bosses cannot be frozen for long, and some things not at all.</summary>
        public bool CanFreeze = true;
        public float FreezeScale = 1f;

        public BurnStatus Burn { get; } = new BurnStatus();
        public bool IsFrozen { get; private set; }
        public float FrozenLeft { get; private set; }
        /// <summary>True on the step a poison's burst landed, so the body can show it.</summary>
        public bool BitThisTick { get; private set; }
        /// <summary>Seconds it is still held fast by something other than ice: it cannot move, turn or begin an attack.</summary>
        public float HeldLeft { get; private set; }
        public bool IsHeld => HeldLeft > 0f;
        /// <summary>How high something thrown into the air rises, in metres.</summary>
        public const float AirRise = 2f;
        /// <summary>Seconds a thing that cannot be lifted reels instead, and the share of a lift's time an elite or a god hangs.</summary>
        public const float ReelSeconds = 1f;
        public const float SturdyAirShare = 0.6f;
        /// <summary>Seconds it will still be in the air (or, for what cannot be lifted, still reeling).</summary>
        public float AirLeft { get; private set; }
        /// <summary>True while it is in the air or reeling: every bonus against "an enemy in the air" counts.</summary>
        public bool IsAirborne => AirLeft > 0f;
        /// <summary>True while it is truly off the ground: helpless, hanging, to come down.</summary>
        public bool IsLifted => AirLeft > 0f && !reeling;
        /// <summary>True on the one tick in which something lifted came back to the ground.</summary>
        public bool LandedThisTick { get; private set; }
        /// <summary>How many blows have held it up in this lift.</summary>
        public int AirBlows { get; private set; }
        /// <summary>True when anything here runs on time, so its owner must tick it.</summary>
        public bool Runs => Any || HeldLeft > 0f || AirLeft > 0f;
        /// <summary>How far off the ground it is now: up quickly, hanging, down quickly.</summary>
        public float AirHeight => !IsLifted ? 0f : AirRise * Math.Min(1f, Math.Min(airFor / 0.18f, AirLeft / 0.18f));

        private bool reeling;
        private float airFor;

        private readonly int[] stacks = new int[Afflictions.Count];
        private readonly float[] left = new float[Afflictions.Count];
        private readonly float[] untilTick = new float[Afflictions.Count];
        private readonly float[] strength = new float[Afflictions.Count];
        private readonly int[] blowsLeft = new int[Afflictions.Count];

        public AfflictionSet() => Clear();

        public void Clear()
        {
            for (int i = 0; i < stacks.Length; i++)
            {
                stacks[i] = 0;
                left[i] = 0f;
                untilTick[i] = 0f;
                strength[i] = 1f;
            }
            Burn.Reset();
            IsFrozen = false;
            FrozenLeft = 0f;
            BitThisTick = false;
            HeldLeft = 0f;
            AirLeft = 0f;
            airFor = 0f;
            AirBlows = 0;
            reeling = false;
            LandedThisTick = false;
        }

        public bool Has(AfflictionKind kind) => kind == AfflictionKind.Burn ? Burn.IsBurning : kind != AfflictionKind.None && stacks[(int)kind] > 0;

        /// <summary>How deep it lies. A burn with no depth still counts as one.</summary>
        public int Stacks(AfflictionKind kind) =>
            kind == AfflictionKind.Burn ? (Burn.IsBurning ? Math.Max(1, Burn.Stacks) : 0) : kind == AfflictionKind.None ? 0 : stacks[(int)kind];

        public float SecondsLeft(AfflictionKind kind) => kind == AfflictionKind.Burn ? Burn.SecondsLeft : left[(int)kind];

        public bool Any
        {
            get
            {
                if (Burn.IsBurning) return true;
                for (int i = 2; i < stacks.Length; i++)
                    if (stacks[i] > 0) return true;
                return false;
            }
        }

        /// <summary>
        /// Lays an affliction: deepens it by the stacks given, up to its maximum, and starts its time over.
        /// <paramref name="seconds"/> of zero means its usual length; <paramref name="power"/> scales what it does.
        /// </summary>
        public AfflictionResult Apply(AfflictionKind kind, int count, float seconds = 0f, float power = 1f)
        {
            if (kind == AfflictionKind.None || count <= 0) return default;
            var spec = Afflictions.Spec(kind);
            int index = (int)kind;
            strength[index] = Math.Max(strength[index], power);
            bool carried = Carries(kind);
            if (kind == AfflictionKind.Burn)
            {
                int before = Burn.Stacks;
                Burn.Ignite(seconds > 0f ? seconds : spec.Seconds, count);
                return new AfflictionResult(true, before < Burn.MaximumStacks && Burn.Stacks >= Burn.MaximumStacks, carried);
            }
            // What is frozen cannot be chilled further; the ice must break or melt first.
            if (IsFrozen && spec.FreezesAtFull) return default;
            int had = stacks[index];
            int deepest = Afflictions.Depth(kind);
            stacks[index] = Math.Min(deepest, had + count);
            left[index] = seconds > 0f ? seconds : spec.Seconds;
            blowsLeft[index] = spec.Blows > 0 ? spec.Blows + Afflictions.MoreBlows : 0;
            if (had == 0) untilTick[index] = spec.TickSeconds;
            bool full = had < deepest && stacks[index] >= deepest;
            if (full && spec.FreezesAtFull && CanFreeze)
            {
                IsFrozen = true;
                FrozenLeft = spec.FrozenSeconds * FreezeScale;
                left[index] = Math.Max(left[index], FrozenLeft);
            }
            return new AfflictionResult(true, full, carried);
        }

        /// <summary>
        /// Holds it still for a time, as ice does but without the ice: what resists being frozen resists this the same
        /// way (a shorter time for elites and gods, none at all for what cannot be frozen). False when it did not take.
        /// </summary>
        public bool Hold(float seconds)
        {
            if (!CanFreeze || seconds <= 0f) return false;
            HeldLeft = Math.Max(HeldLeft, seconds * FreezeScale);
            return HeldLeft > 0f;
        }

        /// <summary>How many more weapon blows an affliction that is spent by blows will last.</summary>
        public int BlowsLeft(AfflictionKind kind) => kind == AfflictionKind.None || stacks[(int)kind] <= 0 ? 0 : blowsLeft[(int)kind];

        /// <summary>A blow of the weapon has landed: what is spent by blows (a mark of light) loses one, and ends with its last.</summary>
        public void BlowLanded()
        {
            for (int i = 2; i < stacks.Length; i++)
            {
                if (stacks[i] <= 0 || blowsLeft[i] <= 0) continue;
                if (--blowsLeft[i] <= 0) End(i);
            }
        }

        /// <summary>
        /// Throws it into the air for a time: it hangs helpless and comes down. Elites and gods hang 0.6 of the time.
        /// What cannot be frozen cannot be lifted: it reels for a second instead (which counts as in the air for
        /// every bonus) and false is returned, so the caller can shake its balance.
        /// </summary>
        public bool Lift(float seconds)
        {
            if (seconds <= 0f) return false;
            if (!CanFreeze)
            {
                reeling = true;
                AirLeft = Math.Max(AirLeft, ReelSeconds);
                return false;
            }
            if (!IsLifted)
            {
                airFor = 0f;
                AirBlows = 0;
            }
            reeling = false;
            AirLeft = Math.Max(AirLeft, seconds * (FreezeScale < 1f ? SturdyAirShare : 1f));
            return true;
        }

        /// <summary>A blow has reached it in the air: it hangs a little longer, for the first few blows. False when it did not.</summary>
        public bool AirBlow(float extraSeconds, int most, float fade = 1f)
        {
            if (!IsLifted || AirBlows >= most) return false;
            float extra = extraSeconds;
            for (int i = 0; i < AirBlows && fade < 1f; i++) extra *= fade;
            AirBlows++;
            AirLeft += extra;
            return true;
        }

        /// <summary>Ends a lift at once: it is driven into the ground.</summary>
        public void Drop()
        {
            if (IsLifted) AirLeft = 0.0001f;
        }

        // True when something already lying here passes this kind on to those standing near.
        private bool Carries(AfflictionKind kind)
        {
            for (int i = 2; i < stacks.Length; i++)
            {
                if (stacks[i] <= 0) continue;
                var spec = Afflictions.Spec((AfflictionKind)i);
                if (spec.Carries == kind || spec.AlsoCarries == kind) return true;
            }
            return false;
        }

        /// <summary>Advances everything and returns the damage dealt over this step.</summary>
        public float Tick(float deltaTime)
        {
            BitThisTick = false;
            LandedThisTick = false;
            if (AirLeft > 0f)
            {
                airFor += deltaTime;
                AirLeft -= deltaTime;
                if (AirLeft <= 0f)
                {
                    AirLeft = 0f;
                    LandedThisTick = !reeling;
                    reeling = false;
                }
            }
            float damage = Burn.Tick(deltaTime) * strength[(int)AfflictionKind.Burn];
            if (!Burn.IsBurning) strength[(int)AfflictionKind.Burn] = 1f;
            if (HeldLeft > 0f) HeldLeft = Math.Max(0f, HeldLeft - deltaTime);
            if (IsFrozen)
            {
                FrozenLeft -= deltaTime;
                // Thawed: the cold that froze it is spent.
                if (FrozenLeft <= 0f) End((int)AfflictionKind.Chill);
            }
            for (int i = 2; i < stacks.Length; i++)
            {
                if (stacks[i] <= 0) continue;
                var spec = Afflictions.Spec((AfflictionKind)i);
                if (spec.TickSeconds > 0f && spec.DamagePerTick > 0f)
                {
                    untilTick[i] -= deltaTime;
                    if (untilTick[i] <= 0f)
                    {
                        untilTick[i] += spec.TickSeconds;
                        damage += spec.DamagePerTick * stacks[i] * strength[i];
                        BitThisTick = true;
                    }
                }
                left[i] -= deltaTime;
                if (left[i] <= 0f) End(i);
            }
            return damage;
        }

        private void End(int index)
        {
            stacks[index] = 0;
            left[index] = 0f;
            strength[index] = 1f;
            if (index == (int)AfflictionKind.Chill)
            {
                IsFrozen = false;
                FrozenLeft = 0f;
            }
        }

        /// <summary>How fast it moves and acts: 1 unhindered, 0 frozen.</summary>
        public float SpeedMultiplier
        {
            get
            {
                if (IsFrozen || HeldLeft > 0f || IsLifted) return 0f;
                float slow = 0f;
                for (int i = 2; i < stacks.Length; i++)
                    if (stacks[i] > 0) slow += Afflictions.Spec((AfflictionKind)i).SlowPerStack * stacks[i] * strength[i];
                return Math.Max(0.3f, 1f - slow);
            }
        }

        /// <summary>What a blow's damage is multiplied by: one number for spells, another for weapons.</summary>
        public float DamageTakenMultiplier(bool spell)
        {
            float more = 0f;
            for (int i = 2; i < stacks.Length; i++)
            {
                if (stacks[i] <= 0) continue;
                var spec = Afflictions.Spec((AfflictionKind)i);
                more += (spell ? spec.SpellDamageTakenPerStack : spec.WeaponDamageTakenPerStack) * stacks[i] * strength[i];
            }
            return 1f + more;
        }

        public float StaggerTakenMultiplier => 1f + Sum(spec => spec.StaggerTakenPerStack);
        public float ShoveMultiplier => 1f + Sum(spec => spec.ShovePerStack);
        public float TurnMultiplier => HeldLeft > 0f || IsLifted ? 0f : Math.Max(0.3f, 1f - Sum(spec => spec.TurnSlowPerStack));
        public float DamageDealtMultiplier => Math.Max(0.3f, 1f - Sum(spec => spec.DealsLessPerStack));
        public float HealOnBlow => Sum(spec => spec.HealOnBlowPerStack);
        public float ManaOnBlow => Sum(spec => spec.ManaPerStack);
        public float HealOnKill => Sum(spec => spec.HealOnKillPerStack);

        public bool BalanceRecovers
        {
            get
            {
                for (int i = 2; i < stacks.Length; i++)
                    if (stacks[i] > 0 && Afflictions.Spec((AfflictionKind)i).StopsBalanceRecovery) return false;
                return true;
            }
        }

        public bool MakesBlowsCritical
        {
            get
            {
                for (int i = 2; i < stacks.Length; i++)
                    if (stacks[i] > 0 && Afflictions.Spec((AfflictionKind)i).MakesBlowsCritical) return true;
                return false;
            }
        }

        // The selectors capture nothing, so the compiler makes each once: summing allocates nothing.
        private float Sum(Func<AfflictionSpec, float> perStack)
        {
            float total = 0f;
            for (int i = 2; i < stacks.Length; i++)
                if (stacks[i] > 0) total += perStack(Afflictions.Spec((AfflictionKind)i)) * stacks[i] * strength[i];
            return total;
        }

        /// <summary>A heavy blow on something frozen breaks the ice: the freeze and the chill end, and the damage is returned.</summary>
        public float Shatter()
        {
            if (!IsFrozen) return 0f;
            float damage = Afflictions.Spec(AfflictionKind.Chill).ShatterDamage * strength[(int)AfflictionKind.Chill];
            End((int)AfflictionKind.Chill);
            return damage;
        }

        /// <summary>Takes an affliction away and says how deep it was.</summary>
        public int Consume(AfflictionKind kind)
        {
            int had = Stacks(kind);
            if (had <= 0) return 0;
            if (kind == AfflictionKind.Burn) Burn.Reset();
            else End((int)kind);
            return had;
        }

        /// <summary>Sets an affliction off: it ends, and the damage it gives all at once is returned.</summary>
        public float SetOff(AfflictionKind kind)
        {
            if (!Has(kind)) return 0f;
            float power = strength[(int)kind];
            if (kind == AfflictionKind.Burn) return Burn.Detonate() * power;
            float extra = kind == AfflictionKind.Chill && IsFrozen ? Shatter() : 0f;
            return extra + Consume(kind) * Afflictions.Spec(kind).SetOffPerStack * power;
        }

        /// <summary>When it dies: the damage its afflictions throw at everyone near, and how far.</summary>
        public float DeathBurst(out float radius)
        {
            radius = 0f;
            float damage = 0f;
            for (int i = 2; i < stacks.Length; i++)
            {
                if (stacks[i] <= 0) continue;
                var spec = Afflictions.Spec((AfflictionKind)i);
                if (spec.DeathBurstPerStack <= 0f) continue;
                damage += spec.DeathBurstPerStack * stacks[i] * strength[i];
                radius = Math.Max(radius, spec.DeathBurstRadius);
            }
            return damage;
        }
    }
}

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
        /// <summary>Seconds it stays brittle after a freeze ended (see <see cref="Afflictions.BrittleSeconds"/>).</summary>
        public float BrittleLeft { get; private set; }
        public bool IsBrittle => BrittleLeft > 0f;
        /// <summary>True on the step a poison's burst landed, so the body can show it.</summary>
        public bool BitThisTick { get; private set; }
        /// <summary>How deep the poison was, and what it dealt, at its last bite.</summary>
        public int BitDepth { get; private set; }
        public float BitDamage { get; private set; }
        /// <summary>How many bites the poison has made with no fresh dose since.</summary>
        public int RipeBites => ripeBites;
        private int ripeBites;
        private bool biteOwed;
        private float sinceBite = float.MaxValue;

        /// <summary>Makes the poison here bite on the next step, whatever its beat. False when there is none.</summary>
        public bool BiteSoon()
        {
            if (stacks[(int)AfflictionKind.Poison] <= 0) return false;
            biteOwed = true;
            return true;
        }

        /// <summary>True when the poison here bit, or will bite, within the given seconds of now.</summary>
        public bool NearBeat(float within)
        {
            int index = (int)AfflictionKind.Poison;
            if (stacks[index] <= 0 || Afflictions.PoisonWaits) return false;
            if (sinceBite <= within) return true;
            if (Afflictions.PoisonBeatOff) return false;
            return (Afflictions.SharedBeat ? Afflictions.BeatLeft : untilTick[index]) <= within;
        }
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
        /// <summary>True when anything here runs on time, so its owner must tick it. A resting reaction counts: its rest must run out even after the afflictions have gone.</summary>
        public bool Runs => Any || HeldLeft > 0f || AirLeft > 0f || restLongest > 0f;
        /// <summary>How far off the ground it is now: up quickly, hanging, down quickly.</summary>
        public float AirHeight => !IsLifted ? 0f : AirRise * Math.Min(1f, Math.Min(airFor / 0.18f, AirLeft / 0.18f));

        private bool reeling;
        private float airFor;

        private readonly int[] stacks = new int[Afflictions.Count];
        private readonly float[] left = new float[Afflictions.Count];
        private readonly float[] untilTick = new float[Afflictions.Count];
        private readonly float[] strength = new float[Afflictions.Count];
        private readonly int[] blowsLeft = new int[Afflictions.Count];

        // One bit for each kind whose stacks are above nought, kept as they rise and fall. Burn keeps its own engine,
        // which can be lit or put out from outside, so its bit is read from it live (see PresentMask).
        private int present;
        // Seconds until each of the 55 pairs may react on this one again, and the longest of them, so a step with
        // nothing resting costs one comparison.
        private readonly float[] reactionRest = new float[AfflictionPairs.Count];
        private float restLongest;

        /// <summary>The reaction engine this set reports to; none given means <see cref="ReactionEngine.Shared"/>. Tests give their own.</summary>
        public ReactionEngine Reactions { get; set; }

        public AfflictionSet() => Clear();

        /// <summary>
        /// One bit for each kind it carries, at <c>1 &lt;&lt; (int)kind</c>, so whether it carries a partner is one AND.
        /// </summary>
        public ushort PresentMask => (ushort)(present | (Burn.IsBurning ? 1 << (int)AfflictionKind.Burn : 0));

        /// <summary>Seconds until the pair (numbered as in <see cref="AfflictionPairs"/>) may react on this one again.</summary>
        public float ReactionRest(int pair) => reactionRest[pair];

        /// <summary>Rests a pair here for the seconds given, or for what it has left if that is longer.</summary>
        public void RestReaction(int pair, float seconds)
        {
            if (seconds <= reactionRest[pair]) return;
            reactionRest[pair] = seconds;
            if (seconds > restLongest) restLongest = seconds;
        }

        /// <summary>
        /// Lays what a blow carries, both its afflictions, and reports each to <see cref="CombatEvents"/> as laid on
        /// <paramref name="who"/>: the one helper every <c>TakeHit</c> calls (IMPLEMENTATION.md 1, 3.7). The burn given
        /// in seconds (<see cref="Hit.BurnSeconds"/>) is still lit by the caller where its order matters, so it does
        /// not pass through <see cref="Lay"/>: it can be a partner but does not itself set a reaction off.
        /// </summary>
        public void TakeFrom(in Hit hit, UnityEngine.Component who)
        {
            var source = hit.Spell ? LaySource.Spell : LaySource.Weapon;
            if (hit.Affliction != AfflictionKind.None && hit.AfflictionStacks > 0)
                CombatEvents.Report(who, hit.Affliction, hit.AfflictionStacks, Lay(hit.Affliction, hit.AfflictionStacks, hit.AfflictionSeconds, hit.AfflictionPower, source, who));
            if (hit.Affliction2 != AfflictionKind.None && hit.Affliction2Stacks > 0)
                CombatEvents.Report(who, hit.Affliction2, hit.Affliction2Stacks, Lay(hit.Affliction2, hit.Affliction2Stacks, hit.Affliction2Seconds, hit.AfflictionPower, source, who));
        }

        /// <summary>
        /// The one entry every laying goes through (IMPLEMENTATION.md 3.7): lays as <see cref="Apply"/> does, and when it
        /// took on something that already carried another kind, tells the reaction engine, which reports each pair
        /// that reacts as happening to <paramref name="who"/>. What a reaction laid (<see cref="LaySource.Reaction"/>)
        /// starts nothing. It does not report to <see cref="CombatEvents.Report"/>; the caller does, as before.
        /// </summary>
        public AfflictionResult Lay(AfflictionKind kind, int count, float seconds, float power, LaySource source, UnityEngine.Component who = null)
        {
            // Read before the laying: the partner must already be there, and the laying itself may spend it (Flash Freeze).
            int carried = PresentMask;
            var result = Apply(kind, count, seconds, power);
            if (result.Applied && source != LaySource.Reaction && (carried & ~(1 << (int)kind)) != 0)
                (Reactions ?? ReactionEngine.Shared).Laid(this, who, kind, carried);
            return result;
        }

        public void Clear()
        {
            owedSeconds = 0f;
            present = 0;
            Array.Clear(reactionRest, 0, reactionRest.Length);
            restLongest = 0f;
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
            BrittleLeft = 0f;
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
            // A fresh dose: the poison is no longer left alone to ripen.
            if (kind == AfflictionKind.Poison) ripeBites = 0;
            int deepest = Afflictions.Depth(kind);
            stacks[index] = Math.Min(deepest, had + count);
            present |= 1 << index;
            left[index] = seconds > 0f ? seconds : spec.Seconds;
            blowsLeft[index] = spec.Blows > 0 ? spec.Blows + Afflictions.MoreBlows : 0;
            if (had == 0) untilTick[index] = spec.TickSeconds;
            bool full = had < deepest && stacks[index] >= deepest;
            // Flash Freeze: what is Soaked freezes sooner, and the water goes into the ice.
            bool flash = kind == AfflictionKind.Chill && Afflictions.FlashFreezeAt > 0 && stacks[(int)AfflictionKind.Soaked] > 0
                && stacks[index] >= Afflictions.FlashFreezeAt && stacks[index] < deepest;
            if ((full || flash) && spec.FreezesAtFull && CanFreeze)
            {
                IsFrozen = true;
                FrozenLeft = spec.FrozenSeconds * FreezeScale * Afflictions.FreezeSeconds;
                left[index] = Math.Max(left[index], FrozenLeft);
                if (flash)
                {
                    End((int)AfflictionKind.Soaked);
                    full = true;
                }
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
            if (sinceBite < float.MaxValue) sinceBite += deltaTime;
            if (restLongest > 0f)
            {
                restLongest -= deltaTime;
                if (restLongest <= 0f)
                {
                    restLongest = 0f;
                    Array.Clear(reactionRest, 0, reactionRest.Length);
                }
                else
                {
                    for (int pair = 0; pair < reactionRest.Length; pair++)
                        if (reactionRest[pair] > 0f) reactionRest[pair] = Math.Max(0f, reactionRest[pair] - deltaTime);
                }
            }
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
            if (BrittleLeft > 0f) BrittleLeft = Math.Max(0f, BrittleLeft - deltaTime);
            for (int i = 2; i < stacks.Length; i++)
            {
                if (stacks[i] <= 0) continue;
                if (i == (int)AfflictionKind.Poison && Afflictions.PoisonWaits)
                {
                    // Held: it neither bites nor fades, and every beat it would have bitten is owed.
                    owedSeconds += deltaTime;
                    continue;
                }
                var spec = Afflictions.Spec((AfflictionKind)i);
                bool poison = i == (int)AfflictionKind.Poison;
                bool cursed = poison && Afflictions.PoisonOnCursed > 0f && stacks[(int)AfflictionKind.Cursed] > 0;
                if (spec.TickSeconds > 0f && spec.DamagePerTick > 0f)
                {
                    bool bites;
                    if (poison && Afflictions.PoisonBeatOff) bites = false;
                    else if (poison && Afflictions.SharedBeat) bites = Afflictions.BeatNow;
                    else
                    {
                        untilTick[i] -= deltaTime;
                        bites = untilTick[i] <= 0f;
                        if (bites) untilTick[i] += spec.TickSeconds;
                    }
                    bool shared = bites && poison && Afflictions.SharedBeat;
                    if (poison && biteOwed)
                    {
                        // Made to bite out of turn: its own beat goes on as it was.
                        bites = true;
                        biteOwed = false;
                    }
                    if (bites)
                    {
                        float bite = spec.DamagePerTick * stacks[i] * strength[i];
                        if (poison)
                        {
                            bite *= 1f + Afflictions.PoisonRipens * ripeBites;
                            if (cursed) bite *= 1f + Afflictions.PoisonOnCursed;
                            if (shared)
                            {
                                bite *= Afflictions.BeatStrength;
                                Afflictions.BeatCount++;
                            }
                            ripeBites++;
                            sinceBite = 0f;
                            BitDepth = stacks[i];
                            BitDamage = bite;
                        }
                        damage += bite;
                        BitThisTick = true;
                    }
                }
                if (poison && (Afflictions.PoisonKeeps || cursed)) continue;
                left[i] -= deltaTime;
                if (left[i] <= 0f) End(i);
            }
            return damage;
        }

        private void End(int index)
        {
            stacks[index] = 0;
            present &= ~(1 << index);
            left[index] = 0f;
            strength[index] = 1f;
            if (index == (int)AfflictionKind.Poison)
            {
                ripeBites = 0;
                biteOwed = false;
                sinceBite = float.MaxValue;
            }
            if (index == (int)AfflictionKind.Chill)
            {
                // Brittle: the ice is gone, by thaw or by breaking, and what it held is weaker for a while.
                if (IsFrozen && Afflictions.BrittleSeconds > 0f) BrittleLeft = Afflictions.BrittleSeconds;
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
                {
                    if (stacks[i] <= 0 || (Afflictions.ChillSlowOff && i == (int)AfflictionKind.Chill)) continue;
                    slow += Afflictions.Spec((AfflictionKind)i).SlowPerStack * stacks[i] * strength[i];
                }
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
            return (1f + more) * (!spell && BrittleLeft > 0f ? 1f + Afflictions.BrittleShare : 1f);
        }

        public float StaggerTakenMultiplier => (1f + Sum(spec => spec.StaggerTakenPerStack))
            * (BrittleLeft > 0f ? 1f + Afflictions.BrittleShare : 1f)
            * (Afflictions.BuckledShare > 0f && stacks[(int)AfflictionKind.Tremor] > 0 && stacks[(int)AfflictionKind.Sundered] > 0 ? 1f + Afflictions.BuckledShare : 1f);
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
            float damage = Afflictions.Spec(AfflictionKind.Chill).ShatterDamage * strength[(int)AfflictionKind.Chill] * Afflictions.ShatterDamage;
            End((int)AfflictionKind.Chill);
            return damage;
        }

        private float owedSeconds;
        /// <summary>Seconds the poison here has been made to wait (<see cref="Afflictions.PoisonWaits"/>) and not yet paid for.</summary>
        public float OwedSeconds => owedSeconds;

        /// <summary>The bites the poison owes for the time it waited, all at once: one for each whole beat, at its depth now. What it owed is forgotten.</summary>
        public float TakeOwed()
        {
            var spec = Afflictions.Spec(AfflictionKind.Poison);
            int index = (int)AfflictionKind.Poison;
            float seconds = owedSeconds;
            owedSeconds = 0f;
            if (stacks[index] <= 0 || spec.TickSeconds <= 0f) return 0f;
            return (float)Math.Floor(seconds / spec.TickSeconds) * spec.DamagePerTick * stacks[index] * strength[index];
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

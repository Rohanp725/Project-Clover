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
        /// <summary>
        /// Bosses cannot be frozen for long, and some things not at all. False also means it cannot be lifted, rooted or
        /// slowed; a godly form is false here and yet takes ice and holds for a moment (<see cref="GodHolds"/>).
        /// </summary>
        public bool CanFreeze = true;
        public float FreezeScale = 1f;
        /// <summary>
        /// True for every god, in either body: the man (Narukami, Khepharu as a man), which can be frozen for a breath and
        /// so would read as an elite, and the godly form. Every god ranks as a god (the owner's decision of 2026-11-19),
        /// and ice and holds stop a god only by <see cref="GodHolds"/>. Set by its body.
        /// </summary>
        public bool IsGod;
        /// <summary>True for a god's godly form, its second phase, which ice and holds stop for no more than a hitch.</summary>
        public bool InGodlyForm;
        /// <summary>Which of a god's bodies this is, for <see cref="GodHolds"/>; none for what is no god.</summary>
        public GodPhase GodPhase => !IsGod ? GodPhase.None : InGodlyForm ? GodPhase.Godly : GodPhase.Man;
        /// <summary>
        /// True while its body will not be stopped at all (a godly form in its entrance, its broken balance or its
        /// death): no freeze or hold takes, and Chill only builds. Set by the body each step.
        /// </summary>
        public bool HoldsRefused;
        /// <summary>Seconds before a god whose freeze or hold has ended can be frozen or held again (<see cref="GodHolds.Guard"/>).</summary>
        public float GuardLeft { get; private set; }

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
        public bool Runs => Any || HeldLeft > 0f || AirLeft > 0f || restLongest > 0f || GuardLeft > 0f || Conditions.Any;

        /// <summary>The timed states reactions and marks set on it (six slots), ticked and cleared with the rest.</summary>
        public EnemyConditions Conditions { get; } = new EnemyConditions();
        /// <summary>The counts and clocks the player's marks keep on this enemy, cleared with the rest.</summary>
        public EnemyTally Tally { get; } = new EnemyTally();
        /// <summary>How far off the ground it is now: up quickly, hanging, down quickly.</summary>
        public float AirHeight => !IsLifted ? 0f : AirRise * Math.Min(1f, Math.Min(airFor / 0.18f, AirLeft / 0.18f));

        private bool reeling;
        private float airFor;

        private readonly int[] stacks = new int[Afflictions.Count];
        private readonly float[] left = new float[Afflictions.Count];
        private readonly float[] untilTick = new float[Afflictions.Count];
        private readonly float[] strength = new float[Afflictions.Count];
        private readonly int[] blowsLeft = new int[Afflictions.Count];
        // For the player's affliction rules (Afflictions.Rules): seconds since each kind was last laid or deepened, and
        // seconds each kind still rests after fading before it can be laid again, with one bit a resting kind.
        private readonly float[] sinceLaid = new float[Afflictions.Count];
        // For a kind that ripens (In Its Season's seeds): seconds it has lain with nothing new laid on it, and one bit for
        // each kind ripe here now.
        private readonly float[] alone = new float[Afflictions.Count];
        private int ripe;
        private readonly float[] restLeft = new float[Afflictions.Count];
        private int resting;

        // One bit for each kind whose stacks are above nought, kept as they rise and fall. Burn keeps its own engine,
        // which can be lit or put out from outside, so its bit is read from it live (see PresentMask).
        private int present;
        private int keptForGood;
        // Poison bites held back by a condition (Burning Sap), and how hard they land when it lets go.
        private float heldBite;
        private float heldScale = 1f;
        // How much harder this freeze shatters (Hard Water's 30%), until it thaws or breaks.
        private float shatterScale = 1f;

        /// <summary>The poison damage held back now, waiting to land at once (Burning Sap).</summary>
        public float HeldBite => heldBite;
        // Seconds until each of the 55 pairs may react on this one again, and the longest of them, so a step with
        // nothing resting costs one comparison.
        private readonly float[] reactionRest = new float[AfflictionPairs.Count];
        private float restLongest;

        /// <summary>The reaction engine this set reports to; none given means <see cref="ReactionEngine.Shared"/>. Tests give their own.</summary>
        public ReactionEngine Reactions { get; set; }

        /// <summary>How hard it is to hold: a god is a god, however its body holds; otherwise read from what decides holding: what cannot be frozen is a god, what is frozen for less an elite.</summary>
        public EnemyRank Rank => RankOf(IsGod, CanFreeze, FreezeScale);

        /// <summary>The rank from its parts, for <see cref="Rank"/> and for tests.</summary>
        public static EnemyRank RankOf(bool isGod, bool canFreeze, float freezeScale) =>
            isGod || !canFreeze ? EnemyRank.God : freezeScale < 1f ? EnemyRank.Elite : EnemyRank.Plain;

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

        /// <summary>What laid what a blow carries: its own flag first, then spell or weapon.</summary>
        public static LaySource SourceOf(in Hit hit) => hit.FromReaction ? LaySource.Reaction : hit.Spell ? LaySource.Spell : LaySource.Weapon;

        /// <summary>
        /// Lays what a blow carries, both its afflictions, and reports each to <see cref="CombatEvents"/> as laid on
        /// <paramref name="who"/>: the one helper every <c>TakeHit</c> calls (IMPLEMENTATION.md 1, 3.7). Then the
        /// conditions that answer a blow are told of it, unless a reaction dealt it. The burn given in seconds
        /// (<see cref="Hit.BurnSeconds"/>) is lit by the caller where its order matters, through <see cref="IgniteFrom"/>.
        /// </summary>
        public void TakeFrom(in Hit hit, UnityEngine.Component who, bool shattered = false)
        {
            var source = SourceOf(hit);
            if (hit.Affliction != AfflictionKind.None && hit.AfflictionStacks > 0)
                CombatEvents.Report(who, hit.Affliction, hit.AfflictionStacks, Lay(hit.Affliction, hit.AfflictionStacks, hit.AfflictionSeconds, hit.AfflictionPower, source, who), source);
            if (hit.Affliction2 != AfflictionKind.None && hit.Affliction2Stacks > 0)
                CombatEvents.Report(who, hit.Affliction2, hit.Affliction2Stacks, Lay(hit.Affliction2, hit.Affliction2Stacks, hit.Affliction2Seconds, hit.AfflictionPower, source, who), source);
            if (!hit.FromReaction && hit.Damage > 0f) Conditions.Struck(hit.Damage, hit.Heavy, hit.Spell, shattered, hit.Critical);
        }

        /// <summary>Lights the burn a blow gives in seconds (<see cref="Hit.BurnSeconds"/>), through the laying rules, so a weapon's fire reacts like any affliction.</summary>
        public void IgniteFrom(in Hit hit, UnityEngine.Component who) => Ignite(hit.BurnSeconds, hit.BurnStacks, SourceOf(hit), who);

        /// <summary>
        /// Sets it burning for the seconds given (topped up, never shortened) and deepens the burn by the stacks given,
        /// which may be none: the burn's own entry, beside <see cref="Lay"/>, for fire given in seconds. It tells the
        /// reaction engine as <see cref="Lay"/> does.
        /// </summary>
        public void Ignite(float seconds, int stacks, LaySource source, UnityEngine.Component who = null)
        {
            if (seconds <= 0f) return;
            int carried = PresentMask;
            Burn.Ignite(seconds, stacks);
            if (source != LaySource.Reaction && (carried & ~(1 << (int)AfflictionKind.Burn)) != 0)
                (Reactions ?? ReactionEngine.Shared).Laid(this, who, AfflictionKind.Burn, carried);
        }

        /// <summary>
        /// Sets a pair's reaction off now, through its rest (Lit Fuse, Plasma Lance and the like: the action
        /// ForceReaction), when it carries both of the pair. It rests again from now. False when a partner is missing.
        /// </summary>
        public bool ForceReaction(int pair, UnityEngine.Component who)
        {
            if (pair < 0 || pair >= AfflictionPairs.Count) return false;
            if (!Has(AfflictionPairs.First(pair)) || !Has(AfflictionPairs.Second(pair))) return false;
            return (Reactions ?? ReactionEngine.Shared).Force(this, who, pair, AfflictionPairs.Second(pair));
        }

        /// <summary>Takes up to this many stacks of an affliction away, with no burst, and says how many went. A burn with no depth counts as one.</summary>
        public int Spend(AfflictionKind kind, int count)
        {
            int had = Stacks(kind);
            if (had <= 0 || count <= 0) return 0;
            int spent = Math.Min(had, count);
            if (spent >= had)
            {
                Consume(kind);
                return spent;
            }
            if (kind == AfflictionKind.Burn) Burn.Lessen(spent);
            else stacks[(int)kind] -= spent;
            return spent;
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
            keptForGood = 0;
            heldBite = 0f;
            heldScale = 1f;
            shatterScale = 1f;
            Conditions.Clear();
            Tally.Clear();
            Array.Clear(reactionRest, 0, reactionRest.Length);
            restLongest = 0f;
            resting = 0;
            for (int i = 0; i < stacks.Length; i++)
            {
                stacks[i] = 0;
                left[i] = 0f;
                untilTick[i] = 0f;
                strength[i] = 1f;
                sinceLaid[i] = 0f;
                alone[i] = 0f;
                restLeft[i] = 0f;
            }
            ripe = 0;
            Burn.Reset();
            IsFrozen = false;
            FrozenLeft = 0f;
            BrittleLeft = 0f;
            BitThisTick = false;
            HeldLeft = 0f;
            GuardLeft = 0f;
            AirLeft = 0f;
            airFor = 0f;
            AirBlows = 0;
            reeling = false;
            LandedThisTick = false;
        }

        /// <summary>
        /// The kind never fades from this enemy again, only by being spent, set off or its death (Stone Does Not Forgive:
        /// once turned to stone it keeps its Tremor and its Curse). Lasts until <see cref="Clear"/>.
        /// </summary>
        public void KeepForGood(AfflictionKind kind)
        {
            if (kind == AfflictionKind.None || (int)kind >= Afflictions.Count) return;
            keptForGood |= 1 << (int)kind;
        }

        /// <summary>True when the kind is kept on this enemy until it dies (<see cref="KeepForGood"/>).</summary>
        public bool KeptForGood(AfflictionKind kind) => kind != AfflictionKind.None && (keptForGood & (1 << (int)kind)) != 0;

        /// <summary>How deep the kind can lie on this enemy now: its usual depth, deeper while a condition allows (Rust).</summary>
        public int DepthOf(AfflictionKind kind) => Afflictions.Depth(kind) + Conditions.DeeperBy(kind);

        public bool Has(AfflictionKind kind) => kind == AfflictionKind.Burn ? Burn.IsBurning : kind != AfflictionKind.None && stacks[(int)kind] > 0;

        /// <summary>How deep it lies. A burn with no depth still counts as one.</summary>
        public int Stacks(AfflictionKind kind) =>
            kind == AfflictionKind.Burn ? (Burn.IsBurning ? Math.Max(1, Burn.Stacks) : 0) : kind == AfflictionKind.None ? 0 : stacks[(int)kind];

        public float SecondsLeft(AfflictionKind kind) => kind == AfflictionKind.Burn ? Burn.SecondsLeft : left[(int)kind];

        /// <summary>True when the kind lies here ripe: left alone long enough under a ripening rule (In Its Season's seeds).</summary>
        public bool IsRipe(AfflictionKind kind) => kind != AfflictionKind.None && (ripe & (1 << (int)kind)) != 0 && Has(kind);

        /// <summary>
        /// What a burst of the kind here is multiplied by: the rules' ripe share when it is ripe, their unripe share when it
        /// is not, and 1 when the kind does not ripen. Read before the burst spends it.
        /// </summary>
        public float RipeShare(AfflictionKind kind)
        {
            var rules = Afflictions.Rules;
            if (!rules.Any || rules.RipensAfter(kind) <= 0f || !Has(kind)) return 1f;
            return IsRipe(kind) ? rules.RipeShare(kind) : rules.UnripeShare(kind);
        }

        /// <summary>Seconds the kind has lain here with nothing new laid on it, for a kind that ripens.</summary>
        public float AloneFor(AfflictionKind kind) => kind == AfflictionKind.None ? 0f : alone[(int)kind];

        private void Unripen(int index)
        {
            alone[index] = 0f;
            ripe &= ~(1 << index);
        }

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
            // Resting after it faded (Full Daylight's mark): it cannot be laid here again yet.
            if ((resting & (1 << index)) != 0) return default;
            strength[index] = Math.Max(strength[index], power);
            bool carried = Carries(kind);
            if (kind == AfflictionKind.Burn)
            {
                sinceLaid[index] = 0f;
                Unripen(index);
                int before = Burn.Stacks;
                Burn.Ignite(seconds > 0f ? seconds : spec.Seconds, count);
                return new AfflictionResult(true, before < Burn.MaximumStacks && Burn.Stacks >= Burn.MaximumStacks, carried);
            }
            // What is frozen cannot be chilled further; the ice must break or melt first.
            if (IsFrozen && spec.FreezesAtFull) return default;
            sinceLaid[index] = 0f;
            // A fresh seed laid on one already there: it is no longer left alone, and must ripen again.
            Unripen(index);
            int had = stacks[index];
            // A fresh dose: the poison is no longer left alone to ripen.
            if (kind == AfflictionKind.Poison) ripeBites = 0;
            int deepest = DepthOf(kind);
            stacks[index] = Math.Min(deepest, had + count);
            present |= 1 << index;
            left[index] = seconds > 0f ? seconds : spec.Seconds;
            blowsLeft[index] = spec.Blows > 0 ? spec.Blows + Afflictions.MoreBlows : 0;
            if (had == 0) untilTick[index] = spec.TickSeconds;
            bool full = had < deepest && stacks[index] >= deepest;
            // Flash Freeze: what is Soaked freezes sooner, and the water goes into the ice.
            bool flash = kind == AfflictionKind.Chill && Afflictions.FlashFreezeAt > 0 && stacks[(int)AfflictionKind.Soaked] > 0
                && stacks[index] >= Afflictions.FlashFreezeAt && stacks[index] < deepest;
            var phase = GodPhase;
            // A god kept from freezing by its guard lies at full Chill: the next cold that reaches it once the guard is
            // over freezes it, though the Chill was full already.
            bool freezes = phase == GodPhase.None ? (full || flash) && CanFreeze && !HoldsRefused
                : (full || flash || stacks[index] >= deepest) && GodMayStop;
            if (freezes && spec.FreezesAtFull)
            {
                IsFrozen = true;
                FrozenLeft = GodHolds.Seconds(spec.FrozenSeconds * Afflictions.FreezeSeconds * RuleFreezeScale, phase, FreezeScale);
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
        /// way (a shorter time for elites, none at all for what cannot be frozen). A god takes it by
        /// <see cref="GodHolds"/>: a moment, only when nothing already stops it and its guard is over, and never made
        /// longer while it lasts. False when it did not take.
        /// </summary>
        public bool Hold(float seconds)
        {
            if (seconds <= 0f) return false;
            var phase = GodPhase;
            if (phase == GodPhase.None)
            {
                if (!CanFreeze || HoldsRefused) return false;
                HeldLeft = Math.Max(HeldLeft, seconds * FreezeScale);
                return HeldLeft > 0f;
            }
            if (!GodMayStop) return false;
            HeldLeft = GodHolds.Seconds(seconds, phase);
            return HeldLeft > 0f;
        }

        // A god may be frozen or held only when nothing stops it now, its body allows it and its guard is over.
        private bool GodMayStop => !HoldsRefused && GuardLeft <= 0f && !IsFrozen && HeldLeft <= 0f;

        // A god's freeze or hold has ended: it cannot be stopped again for a while.
        private void GuardGod()
        {
            var phase = GodPhase;
            if (phase != GodPhase.None) GuardLeft = Math.Max(GuardLeft, GodHolds.Guard(phase));
        }

        /// <summary>How many more weapon blows an affliction that is spent by blows will last.</summary>
        public int BlowsLeft(AfflictionKind kind) => kind == AfflictionKind.None || stacks[(int)kind] <= 0 ? 0 : blowsLeft[(int)kind];

        /// <summary>A blow of the weapon has landed: what is spent by blows (a mark of light) loses one, and ends with its last.</summary>
        public void BlowLanded()
        {
            for (int i = 2; i < stacks.Length; i++)
            {
                if (stacks[i] <= 0 || blowsLeft[i] <= 0) continue;
                // A condition that keeps it (Diamond Ring's mark) keeps it from being spent too, and so does a rule (Full Daylight).
                if (Conditions.KeepsKind((AfflictionKind)i) || Afflictions.Rules.Unspent((AfflictionKind)i)) continue;
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
            Conditions.Tick(deltaTime);
            // A condition that lasts only while afflictions are carried ends once one is gone.
            if (Conditions.Any) Conditions.Keep(PresentMask);
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
            var rules = Afflictions.Rules;
            // A rest runs out even after the rule that began it has left force.
            if (rules.Any || resting != 0) FollowRules(rules, deltaTime);
            // Kept from fading (Smoulder, or Fire Without End's rule): the burn's time is given back before it is spent, so it
            // bites on and never runs down.
            if (Burn.IsBurning && (Conditions.KeepsKind(AfflictionKind.Burn) || rules.Keeps(AfflictionKind.Burn) || KeptForGood(AfflictionKind.Burn)
                || (rules.Any && rules.KeepsWith(AfflictionKind.Burn, PresentMask)))) Burn.Ignite(Burn.SecondsLeft + deltaTime);
            float burnt = Burn.Tick(deltaTime) * strength[(int)AfflictionKind.Burn] * Conditions.BiteScale(AfflictionKind.Burn, IsFrozen);
            // The Blade Drinks Fire: a burn left alone does nothing; it is fuel for the blade.
            float damage = Afflictions.BurnBiteOff ? 0f : burnt;
            if (!Burn.IsBurning) strength[(int)AfflictionKind.Burn] = 1f;
            // The guard is spent before a new one can begin on this step, so a guard always starts whole.
            if (GuardLeft > 0f) GuardLeft = Math.Max(0f, GuardLeft - deltaTime);
            if (HeldLeft > 0f)
            {
                HeldLeft = Math.Max(0f, HeldLeft - deltaTime);
                if (HeldLeft <= 0f) GuardGod();
            }
            if (IsFrozen)
            {
                FrozenLeft -= deltaTime;
                // Thawed: the cold that froze it is spent.
                if (FrozenLeft <= 0f)
                {
                    End((int)AfflictionKind.Chill);
                    shatterScale = 1f;
                }
            }
            if (BrittleLeft > 0f) BrittleLeft = Math.Max(0f, BrittleLeft - deltaTime);
            // Held bites land at once, at their hold's strength, once no condition holds them (Burning Sap).
            if (heldBite > 0f && !Conditions.HoldsBite(AfflictionKind.Poison, out _))
            {
                float landed = heldBite * heldScale * (1f + (Afflictions.Rules.Any ? Afflictions.Rules.HeldStronger : 0f));
                heldBite = 0f;
                heldScale = 1f;
                damage += landed;
                BitDamage = landed;
                BitThisTick = true;
            }
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
                // Fast Water: in a Soaked body the poison's own clock runs at twice the pace, its beat and its fading both.
                float pace = poison && Afflictions.PoisonQuickWhenSoaked && stacks[(int)AfflictionKind.Soaked] > 0 ? 2f : 1f;
                if (spec.TickSeconds > 0f && spec.DamagePerTick > 0f)
                {
                    bool bites;
                    if (poison && Afflictions.PoisonBeatOff) bites = false;
                    else if (poison && Afflictions.SharedBeat) bites = Afflictions.BeatNow;
                    else
                    {
                        // A condition may quicken the beat alone (Ichor), not the fading.
                        untilTick[i] -= deltaTime * pace * Conditions.Quickening((AfflictionKind)i);
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
                    if (bites && poison && Conditions.HoldsBite(AfflictionKind.Poison, out float holdScale))
                    {
                        // Held back: counted, not dealt, and no bite is told until the hold lets go.
                        heldBite += spec.DamagePerTick * stacks[i] * strength[i] * Conditions.BiteScale((AfflictionKind)i, IsFrozen);
                        heldScale = Math.Max(heldScale, holdScale);
                        bites = false;
                    }
                    if (bites)
                    {
                        float bite = spec.DamagePerTick * stacks[i] * strength[i] * Conditions.BiteScale((AfflictionKind)i, IsFrozen);
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
                            // The conditions that beat on the bite (Nerve Fire, Venom Rime) come to their beat.
                            Conditions.Bitten();
                        }
                        damage += bite;
                        BitThisTick = true;
                    }
                }
                if (poison && (Afflictions.PoisonKeeps || cursed)) continue;
                if (Conditions.KeepsKind((AfflictionKind)i) || rules.Keeps((AfflictionKind)i) || (keptForGood & (1 << i)) != 0) continue;
                // Kept, or slowed, while the enemy carries another kind a rule pairs it with (Permafrost, Neither Wins, Brine).
                float rulePace = 1f;
                if (rules.Any)
                {
                    int carried = PresentMask;
                    if (rules.KeepsWith((AfflictionKind)i, carried)) continue;
                    rulePace = rules.FadePace((AfflictionKind)i, carried);
                }
                // A ripe seed stops fading (In Its Season); once the rule has left force it ripens no more and fades as written.
                if ((ripe & (1 << i)) != 0)
                {
                    if (rules.RipensAfter((AfflictionKind)i) > 0f) continue;
                    Unripen(i);
                }
                left[i] -= deltaTime * pace * rulePace;
                if (left[i] > 0f) continue;
                End(i);
                // Faded by itself: under a rule it rests before it can be laid here again.
                float rest = rules.Any ? rules.RestsAfter((AfflictionKind)i) : 0f;
                if (rest > 0f)
                {
                    restLeft[i] = rest;
                    resting |= 1 << i;
                }
            }
            return damage;
        }

        /// <summary>Seconds before the kind can be laid here again after fading under a rule; zero when it can be laid now.</summary>
        public float RestLeft(AfflictionKind kind) => kind == AfflictionKind.None || (resting & (1 << (int)kind)) == 0 ? 0f : restLeft[(int)kind];

        // The player's affliction rules, once a step: what rests counts down, and what is left alone deepens by itself
        // (Fire Without End: a burn with nothing new laid on it gains a stack every few seconds, up to its depth).
        private void FollowRules(AfflictionRules rules, float deltaTime)
        {
            for (int i = 1; i < Afflictions.Count; i++)
            {
                var kind = (AfflictionKind)i;
                if ((resting & (1 << i)) != 0)
                {
                    restLeft[i] -= deltaTime;
                    if (restLeft[i] <= 0f)
                    {
                        restLeft[i] = 0f;
                        resting &= ~(1 << i);
                    }
                }
                float ripens = rules.RipensAfter(kind);
                if (ripens > 0f && (ripe & (1 << i)) == 0)
                {
                    // Left alone long enough, it ripens (In Its Season): it stops fading until it bursts or a fresh one is laid.
                    if (Has(kind))
                    {
                        alone[i] += deltaTime;
                        if (alone[i] >= ripens) ripe |= 1 << i;
                    }
                    else alone[i] = 0f;
                }
                float every = rules.DeepensEvery(kind);
                if (every <= 0f) continue;
                bool burn = i == (int)AfflictionKind.Burn;
                if (burn ? !Burn.IsBurning : stacks[i] <= 0)
                {
                    sinceLaid[i] = 0f;
                    continue;
                }
                sinceLaid[i] += deltaTime;
                if (sinceLaid[i] < every) continue;
                sinceLaid[i] -= every;
                int depth = rules.DeepensTo(kind);
                if (burn)
                {
                    if (Burn.Stacks < depth) Burn.Ignite(Burn.SecondsLeft, 1);
                }
                else if (stacks[i] < Math.Min(depth, Afflictions.Depth(kind))) stacks[i]++;
            }
        }

        private void End(int index)
        {
            stacks[index] = 0;
            present &= ~(1 << index);
            left[index] = 0f;
            strength[index] = 1f;
            Unripen(index);
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
                if (IsFrozen) GuardGod();
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
                return Math.Max(0.3f, 1f - slow) * Conditions.Pace;
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
            return (1f + more) * (!spell && BrittleLeft > 0f ? 1f + Afflictions.BrittleShare : 1f) * Conditions.TakenScale
                * (spell ? Conditions.SpellTakenScale : Conditions.WeaponTakenScale);
        }

        public float StaggerTakenMultiplier => (1f + Sum(spec => spec.StaggerTakenPerStack))
            * (BrittleLeft > 0f ? 1f + Afflictions.BrittleShare : 1f)
            * (Afflictions.BuckledShare > 0f && stacks[(int)AfflictionKind.Tremor] > 0 && stacks[(int)AfflictionKind.Sundered] > 0 ? 1f + Afflictions.BuckledShare : 1f)
            * Conditions.StaggerScale;
        public float ShoveMultiplier => 1f + Sum(spec => spec.ShovePerStack);
        public float TurnMultiplier => HeldLeft > 0f || IsLifted ? 0f : Math.Max(0.3f, 1f - Sum(spec => spec.TurnSlowPerStack));
        public float DamageDealtMultiplier => Math.Max(0.3f, 1f - Sum(spec => spec.DealsLessPerStack)) * Conditions.DealtScale;
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
                if (Conditions.Has(ConditionFlags.TakesCritical)) return true;
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

        // A freeze held longer by the player's rules on an enemy carrying their partner (Winter in the Bones).
        private float RuleFreezeScale => Afflictions.Rules.Any ? Afflictions.Rules.FreezeScale(PresentMask) : 1f;

        /// <summary>
        /// Frozen at once for <paramref name="seconds"/>, whatever its Chill (Glass Spear): the Chill is filled and the
        /// freeze runs its own time. What cannot be frozen is not, an elite for its share, and a god as its hold allows.
        /// False when it did not take.
        /// </summary>
        public bool FreezeFor(float seconds) => FreezeFor(seconds, 1f);

        /// <summary>The same, the freeze shattering <paramref name="shatter"/> times as hard (Hard Water: 1.3).</summary>
        public bool FreezeFor(float seconds, float shatter)
        {
            if (seconds <= 0f) return false;
            var phase = GodPhase;
            if (phase == GodPhase.None ? !CanFreeze || HoldsRefused : !GodMayStop) return false;
            int index = (int)AfflictionKind.Chill;
            stacks[index] = Math.Max(stacks[index], Afflictions.Depth(AfflictionKind.Chill));
            present |= 1 << index;
            float time = GodHolds.Seconds(seconds * RuleFreezeScale, phase, FreezeScale);
            FrozenLeft = IsFrozen ? Math.Max(FrozenLeft, time) : time;
            IsFrozen = true;
            shatterScale = Math.Max(shatterScale, Math.Max(1f, shatter));
            left[index] = Math.Max(left[index], FrozenLeft);
            return true;
        }

        /// <summary>
        /// Keeps a frozen enemy frozen for longer (Six Levels Down): a Chill laid on it does nothing, since the freeze is
        /// already full. True when it was frozen.
        /// </summary>
        public bool KeepFrozen(float seconds)
        {
            if (!IsFrozen || seconds <= 0f) return false;
            FrozenLeft += seconds;
            int index = (int)AfflictionKind.Chill;
            left[index] = Math.Max(left[index], FrozenLeft);
            return true;
        }

        /// <summary>A heavy blow on something frozen breaks the ice: the freeze and the chill end, and the damage is returned.</summary>
        public float Shatter()
        {
            if (!IsFrozen) return 0f;
            float damage = Afflictions.Spec(AfflictionKind.Chill).ShatterDamage * strength[(int)AfflictionKind.Chill] * Afflictions.ShatterDamage * shatterScale;
            shatterScale = 1f;
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
        public float DeathBurst(out float radius) => DeathBurst(out radius, AfflictionKind.None);

        /// <summary>The same, with one kind kept from bursting (What the Yew Keeps passes the seed on whole instead).</summary>
        public float DeathBurst(out float radius, AfflictionKind withheld)
        {
            radius = 0f;
            float damage = 0f;
            for (int i = 2; i < stacks.Length; i++)
            {
                if (stacks[i] <= 0 || i == (int)withheld) continue;
                var spec = Afflictions.Spec((AfflictionKind)i);
                if (spec.DeathBurstPerStack <= 0f) continue;
                // A kind that ripens bursts harder ripe, softer unripe (In Its Season).
                damage += spec.DeathBurstPerStack * stacks[i] * strength[i] * RipeShare((AfflictionKind)i);
                radius = Math.Max(radius, spec.DeathBurstRadius);
            }
            return damage;
        }
    }
}

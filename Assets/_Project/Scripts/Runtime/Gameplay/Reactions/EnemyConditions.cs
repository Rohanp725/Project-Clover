using System;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The timed states on one enemy that reactions and marks set (IMPLEMENTATION.md 3.7, README's most-needed engine
    /// piece): six fixed slots, each with a name, its time, a beat, the flags the enemy reads, and the mark that set it.
    /// It says when one begins, beats, is struck and ends, through <see cref="CombatEvents.Condition"/>, and the
    /// listener runs the setting mark's answers. A condition may also keep an affliction from fading, last only while
    /// afflictions are carried, beat on the poison's bite, be spent by a blow, or end after so many blows (task 3.4).
    /// Held by each <see cref="AfflictionSet"/>, which ticks and clears it. Plain rules with nothing allocated after it
    /// is made.
    /// </summary>
    public sealed class EnemyConditions
    {
        public const int Slots = 6;

        /// <summary>Told of each moment; none given means <see cref="CombatEvents.RaiseCondition"/>. Tests give their own.</summary>
        public Action<Component, ConditionNews> Report { get; set; }

        /// <summary>The enemy these are on, as the last setter named it; what each moment is said to happen to.</summary>
        public Component Who { get; private set; }

        private readonly ConditionRun[] runs = new ConditionRun[Slots];
        // Those that ended this step, told once the rest are counted again.
        private readonly ConditionRun[] endedRuns = new ConditionRun[Slots];

        /// <summary>How many slots are in use.</summary>
        public int Count { get; private set; }
        public bool Any => Count > 0;
        /// <summary>Every flag any running condition sets.</summary>
        public ConditionFlags Flags { get; private set; }
        /// <summary>The share of its speed it keeps, all conditions together.</summary>
        public float Pace { get; private set; } = 1f;
        /// <summary>How fast its wind-ups run: 1 as usual.</summary>
        public float WindupPace { get; private set; } = 1f;
        /// <summary>What the damage it takes, and deals, is multiplied by.</summary>
        public float TakenScale { get; private set; } = 1f;
        public float DealtScale { get; private set; } = 1f;
        /// <summary>What its turning pace is multiplied by: 0 when it cannot turn, the Scale of each turn-slowing run otherwise.</summary>
        public float TurnScale { get; private set; } = 1f;
        /// <summary>What the damage it takes from weapon blows, and from spells, is multiplied by beyond <see cref="TakenScale"/>.</summary>
        public float WeaponTakenScale { get; private set; } = 1f;
        public float SpellTakenScale { get; private set; } = 1f;
        /// <summary>What the stagger it takes is multiplied by.</summary>
        public float StaggerScale { get; private set; } = 1f;
        /// <summary>What the damage it takes from heavy weapon blows is multiplied by, beyond the weapon's own share.</summary>
        public float HeavyTakenScale { get; private set; } = 1f;
        /// <summary>What the stagger it takes from weapon blows is multiplied by, beyond <see cref="StaggerScale"/>.</summary>
        public float WeaponStaggerScale { get; private set; } = 1f;
        /// <summary>Seconds a freeze lasts longer while a condition runs (the most any asks), and how much harder a shatter strikes.</summary>
        public float FreezeLonger { get; private set; }
        public float ShatterScale { get; private set; } = 1f;
        /// <summary>How far it sees while its sight is cut short; 0 when it is not.</summary>
        public float Sight { get; private set; }
        /// <summary>Metres its body is drawn down into the floor: the deepest any running condition asks (<see cref="ConditionSpec.Sink"/>).</summary>
        public float SinkDepth { get; private set; }
        /// <summary>The afflictions kept from fading and from being spent by blows, as bits at <c>1 &lt;&lt; (int)kind</c>.</summary>
        public int Keeps { get; private set; }

        public bool Has(ConditionFlags flag) => (Flags & flag) != 0;

        /// <summary>True when a running condition keeps this affliction from fading.</summary>
        public bool KeepsKind(AfflictionKind kind) => (Keeps & (1 << (int)kind)) != 0;

        /// <summary>True when a side in degrees lies outside a run's flank (<see cref="ConditionSpec.FlankLeast"/>); an unknown side (-1) or no flank never does.</summary>
        public static bool OffFlank(float least, float most, float side) => (least > 0f || most > 0f) && side >= 0f && (side < least || side > most);

        /// <summary>
        /// True when a running condition keeps this affliction from being spent by a weapon blow from <paramref name="side"/>
        /// (degrees from its face; -1 unknown): one with a flank keeps it only for blows from that flank (Mock Suns).
        /// </summary>
        public bool KeepsFrom(AfflictionKind kind, float side)
        {
            if (!KeepsKind(kind)) return false;
            int bit = 1 << (int)kind;
            for (int i = 0; i < Slots; i++)
                if (runs[i].Active && (runs[i].KeepsMask & bit) != 0 && !OffFlank(runs[i].FlankLeast, runs[i].FlankMost, side)) return true;
            return false;
        }

        /// <summary>True when a running condition with a flank bars the afflictions' critical blows from <paramref name="side"/> (Mock Suns: only from its flanks).</summary>
        public bool BarsCritical(float side)
        {
            if (Count == 0) return false;
            for (int i = 0; i < Slots; i++)
                if (runs[i].Active && OffFlank(runs[i].FlankLeast, runs[i].FlankMost, side)) return true;
            return false;
        }

        /// <summary>How hard the kind bites under the conditions running: the product of each one's scale (1 as written).</summary>
        public float BiteScale(AfflictionKind kind, bool frozen)
        {
            int bit = Bit(kind);
            float scale = 1f;
            if (bit == 0) return scale;
            for (int i = 0; i < Slots; i++)
                if (runs[i].Active && (runs[i].BiteMask & bit) != 0 && (!runs[i].BiteFrozen || frozen)) scale *= runs[i].BiteScale;
            return scale;
        }

        /// <summary>True while a condition running holds the kind's bites back; <paramref name="scale"/> is how hard they land when let go (the most any asks).</summary>
        public bool HoldsBite(AfflictionKind kind, out float scale)
        {
            int bit = Bit(kind);
            scale = 1f;
            bool held = false;
            if (bit == 0) return false;
            for (int i = 0; i < Slots; i++)
                if (runs[i].Active && (runs[i].HoldMask & bit) != 0)
                {
                    held = true;
                    scale = Math.Max(scale, runs[i].HeldScale);
                }
            return held;
        }

        /// <summary>How fast the kind beats under the conditions running: the product of each one's quickening (1 as written).</summary>
        public float Quickening(AfflictionKind kind)
        {
            int bit = Bit(kind);
            float by = 1f;
            if (bit == 0) return by;
            for (int i = 0; i < Slots; i++)
                if (runs[i].Active && (runs[i].QuickMask & bit) != 0) by *= runs[i].QuickBy;
            return by;
        }

        /// <summary>How much deeper than its usual depth the kind may lie while the conditions running allow it (the most any one allows).</summary>
        public int DeeperBy(AfflictionKind kind)
        {
            int bit = Bit(kind), most = 0;
            if (bit == 0) return 0;
            for (int i = 0; i < Slots; i++)
                if (runs[i].Active && (runs[i].DeepMask & bit) != 0 && runs[i].DeepBy > most) most = runs[i].DeepBy;
            return most;
        }

        public ConditionRun Run(int slot) => runs[slot];

        /// <summary>
        /// The news a running condition named <paramref name="id"/> (or that is it, by <see cref="ConditionSpec.Is"/>) would carry
        /// at <paramref name="moment"/> now, its count as it stands, without that moment coming: a payout made early (Reckoning's
        /// Master pays the seal's count on a finisher, and the seal pays again as it breaks). False when none runs.
        /// </summary>
        public bool NewsOf(string id, ConditionMoment moment, out ConditionNews news)
        {
            news = default;
            if (string.IsNullOrEmpty(id)) return false;
            for (int i = 0; i < Slots; i++)
            {
                ref var run = ref runs[i];
                if (!run.Active || (run.Id != id && run.Is != id)) continue;
                news = new ConditionNews
                {
                    Moment = moment, Id = run.Id, Mark = run.Mark, Count = run.Count, Blows = run.Blows, Flags = run.Flags, Scale = run.Scale, Link = run.Link,
                    Is = run.Is, Side = -1f,
                };
                return true;
            }
            return false;
        }

        /// <summary>True while a condition runs under this name, or is this by what it is (<see cref="ConditionSpec.Is"/>): Hot Shards' crack is a Cracked one.</summary>
        public bool Holds(string id)
        {
            if (string.IsNullOrEmpty(id)) return false;
            for (int i = 0; i < Slots; i++)
                if (runs[i].Active && (runs[i].Id == id || runs[i].Is == id)) return true;
            return false;
        }

        /// <summary>
        /// True while it <see cref="Holds"/> the condition, or a blow spent it since the last step: the heavy blow that
        /// cracks a brittle enemy still landed on a brittle one, for whoever asks after the blow (Glass Bell).
        /// </summary>
        public bool Held(string id)
        {
            if (Holds(id)) return true;
            for (int i = 0; i < spentCount; i++)
                if (spentNames[i] == id) return true;
            return false;
        }

        /// <summary>
        /// A weapon blow told to the conditions under this name (or that are this) alone, as if it landed on them now
        /// (Hot Stone, Cold Water: a perfect guard splits the crack): each counts it and answers it, and one such a blow
        /// spends ends. The enemy itself takes nothing. Returns how many it reached.
        /// </summary>
        public int StrikeWhere(string id, bool heavy)
        {
            if (Count == 0 || string.IsNullOrEmpty(id)) return 0;
            int reached = 0;
            bool spentAny = false;
            for (int i = 0; i < Slots; i++)
            {
                if (!runs[i].Active || (runs[i].Id != id && runs[i].Is != id)) continue;
                ref var run = ref runs[i];
                reached++;
                run.Blows++;
                if (run.AnswersStruck) Tell(ConditionMoment.Struck, in run, 0f, heavy, false);
                if (!SpentBy(in run, heavy, false)) continue;
                NoteSpent(in run);
                runs[i] = default;
                spentAny = true;
            }
            if (spentAny) Recount();
            return reached;
        }

        // The side of the blow being told, in degrees (see SideDegrees), or -1 when it is not known.
        private float blowSide = -1f;

        /// <summary>
        /// The side a blow comes from, in degrees: 0 when it comes at the enemy's face, 90 at a flank, 180 at its back.
        /// <paramref name="facing"/> is the way the enemy faces, <paramref name="travel"/> the way the blow travels. -1 when
        /// either is not known (a test's set with no body, a blow with no way).
        /// </summary>
        public static float SideDegrees(Vector3 facing, Vector3 travel)
        {
            facing.y = 0f;
            travel.y = 0f;
            if (facing.sqrMagnitude < 0.0001f || travel.sqrMagnitude < 0.0001f) return -1f;
            return Vector3.Angle(facing, -travel);
        }

        // The names of what a blow spent since the last step (each run's name and what it is).
        private readonly string[] spentNames = new string[Slots * 2];
        private int spentCount;

        private void NoteSpent(in ConditionRun run)
        {
            if (spentCount < spentNames.Length) spentNames[spentCount++] = run.Id;
            if (!string.IsNullOrEmpty(run.Is) && spentCount < spentNames.Length) spentNames[spentCount++] = run.Is;
        }

        private void ForgetSpent()
        {
            for (int i = 0; i < spentCount; i++) spentNames[i] = null;
            spentCount = 0;
        }

        /// <summary>Seconds the named condition has left (the longest, when it runs more than once); 0 when it does not run.</summary>
        public float Left(string id)
        {
            float most = 0f;
            for (int i = 0; i < Slots; i++)
                if (runs[i].Active && runs[i].Id == id && runs[i].Left > most) most = runs[i].Left;
            return most;
        }

        /// <summary>
        /// Sets a condition. Set again from the same mark, with the same name and link, it is refreshed: its time starts
        /// over (never shortened), its numbers are the new ones, and its beats and blows are kept. Otherwise it takes a
        /// free slot, or the place of the one nearest its end. <paramref name="slowShare"/> is how much of the slowing
        /// takes (an elite's is half), and of a root's time likewise. Returns the slot, or -1 when nothing was asked.
        /// </summary>
        /// <summary>Keeps a number on a run in place of its spec's <see cref="ConditionSpec.Scale"/>: what the trigger that set it carried (Collected Twice's damage).</summary>
        /// <summary>Where its loose shadow lies (<see cref="ConditionFlags.ShadowLoose"/>, the run's point); false while its shadow is its own.</summary>
        public bool LooseShadow(out Vector3 spot)
        {
            spot = Vector3.zero;
            if (!Has(ConditionFlags.ShadowLoose)) return false;
            for (int i = 0; i < Slots; i++)
            {
                if (!runs[i].Active || (runs[i].Flags & ConditionFlags.ShadowLoose) == 0) continue;
                spot = runs[i].FacePoint;
                return true;
            }
            return false;
        }

        /// <summary>True when a chain it carries joins it to <paramref name="host"/>, with the share of blows it passes and whether spells pass.</summary>
        public bool ChainedTo(Component host, out float share, out bool spells)
        {
            share = 0f;
            spells = false;
            if (host == null || !Has(ConditionFlags.Chained)) return false;
            for (int i = 0; i < Slots; i++)
            {
                if (!runs[i].Active || (runs[i].Flags & ConditionFlags.Chained) == 0 || runs[i].Link != host) continue;
                share = runs[i].Scale;
                spells = runs[i].SharesSpells;
                return true;
            }
            return false;
        }

        /// <summary>The other end of the chain it carries (the newest), or null, with the share of blows it passes and whether spells pass.</summary>
        public Component ChainMate(out float share, out bool spells)
        {
            share = 0f;
            spells = false;
            if (!Has(ConditionFlags.Chained)) return null;
            Component mate = null;
            float most = -1f;
            for (int i = 0; i < Slots; i++)
            {
                if (!runs[i].Active || (runs[i].Flags & ConditionFlags.Chained) == 0 || runs[i].Link == null || runs[i].Left <= most) continue;
                most = runs[i].Left;
                mate = runs[i].Link;
                share = runs[i].Scale;
                spells = runs[i].SharesSpells;
            }
            return mate;
        }

        /// <summary>
        /// True while a run holds it on a leash: its length, and what holds it (the run's link, the player, or the spot the
        /// run was set on when it has neither). The shortest leash wins.
        /// </summary>
        public bool Leashed(out Component link, out Vector3 spot, out float leash, out bool toPlayer)
        {
            link = null;
            spot = Vector3.zero;
            leash = 0f;
            toPlayer = false;
            for (int i = 0; i < Slots; i++)
            {
                if (!runs[i].Active || runs[i].Leash <= 0f || (leash > 0f && runs[i].Leash >= leash)) continue;
                leash = runs[i].Leash;
                link = runs[i].Link;
                spot = runs[i].Anchor;
                toPlayer = runs[i].LeashToPlayer;
            }
            return leash > 0f;
        }

        /// <summary>Sets the point a run that turns its carrier (<see cref="ConditionFlags.FacesPoint"/>) turns it to face.</summary>
        public void FaceToward(int slot, Vector3 point)
        {
            if (slot < 0 || slot >= Slots || !runs[slot].Active) return;
            runs[slot].FacePoint = point;
        }

        /// <summary>The point it is turned to face: the newest running condition's that turns it; false when none does.</summary>
        public bool TurnedToward(out Vector3 point)
        {
            point = Vector3.zero;
            if (!Has(ConditionFlags.FacesPoint)) return false;
            float most = -1f;
            for (int i = 0; i < Slots; i++)
            {
                if (!runs[i].Active || (runs[i].Flags & ConditionFlags.FacesPoint) == 0 || runs[i].Left <= most) continue;
                most = runs[i].Left;
                point = runs[i].FacePoint;
            }
            return most >= 0f;
        }

        /// <summary>A count of blows begun this many blows along (Held Down: the fig wound at two coils). Never counts back.</summary>
        public void StartAt(int slot, int blows)
        {
            if (slot < 0 || slot >= Slots || !runs[slot].Active || blows <= runs[slot].Blows) return;
            runs[slot].Blows = blows;
        }

        public void Keep(int slot, float scale)
        {
            if (slot < 0 || slot >= Slots || !runs[slot].Active) return;
            runs[slot].Scale = scale;
            // The scale may be what it does (a turning slowed by the notch: Seized Joints), so what is summed is summed again.
            Recount();
        }

        public int Set(ConditionSpec spec, Component who, int mark = -1, Component link = null, bool answersStruck = false, bool answersEnd = false, float slowShare = 1f,
            bool answersDeath = false, bool answersAttack = false, float pace = -1f)
        {
            if (spec == null || string.IsNullOrEmpty(spec.Id) || spec.Seconds <= 0f) return -1;
            if (who != null) Who = who;
            int slot = Find(spec.Id, link, mark);
            bool fresh = slot < 0;
            if (fresh)
            {
                for (int i = 0; i < Slots && slot < 0; i++)
                    if (!runs[i].Active) slot = i;
                if (slot < 0)
                {
                    slot = 0;
                    for (int i = 1; i < Slots; i++)
                        if (runs[i].Left < runs[slot].Left) slot = i;
                }
                runs[slot] = new ConditionRun { Active = true, Id = spec.Id, Mark = mark, Link = link, BeatClock = spec.Beat };
            }
            ref var run = ref runs[slot];
            // What resists a slowing resists a root, and being knocked flat, the same way: an elite is held for half the time.
            float seconds = (spec.Flags & (ConditionFlags.Rooted | ConditionFlags.Down)) != 0 ? spec.Seconds * Mathf.Clamp01(slowShare) : spec.Seconds;
            // A time of its own on what resists (an elite: slowShare below 1).
            if (spec.EliteSeconds > 0f && slowShare < 1f) seconds = Math.Min(spec.Seconds, spec.EliteSeconds);
            run.Left = Math.Max(fresh ? 0f : run.Left, seconds);
            run.Beat = spec.OnBite ? 0f : spec.Beat;
            run.Flags = spec.Flags;
            // A pace given with the call (a slow by the stacks counted) stands in place of the spec's own.
            run.Pace = 1f - (1f - Mathf.Clamp01(pace >= 0f ? pace : spec.Pace)) * Mathf.Clamp01(slowShare);
            run.Scale = spec.Scale;
            run.Sight = spec.Sight;
            run.AnswersStruck = answersStruck;
            run.AnswersEnd = answersEnd;
            run.AnswersDeath = answersDeath;
            run.AnswersAttack = answersAttack;
            run.KeepsMask = Bit(spec.Keeps);
            run.DeepMask = Bit(spec.Deepens);
            run.BiteMask = spec.BiteScale > 0f && spec.BiteScale != 1f ? Bit(spec.BitesHarder) : 0;
            run.BiteScale = spec.BiteScale;
            run.BiteFrozen = spec.BitesHarderFrozen;
            run.HoldMask = Bit(spec.HoldsBites);
            run.HeldScale = Math.Max(1f, spec.HeldScale);
            run.QuickMask = spec.QuickenBy > 0f && spec.QuickenBy != 1f ? Bit(spec.Quickens) : 0;
            run.QuickBy = spec.QuickenBy;
            run.DeepBy = Math.Max(0, spec.DeepensBy);
            run.WhileMask = Bit(spec.While) | Bit(spec.While2);
            run.OnBite = spec.OnBite;
            run.EndsWith = spec.EndsWith;
            run.BlowLimit = Math.Max(0, spec.Blows);
            run.Is = spec.Is;
            run.WhileIn = spec.WhileIn;
            run.CriticalOnly = spec.CriticalOnly;
            run.EndsOnFreeze = spec.EndsOnFreeze;
            run.DriftsTo = Math.Max(0f, spec.DriftsTo);
            run.DeathsWithin = Math.Max(0f, spec.DeathsWithin);
            run.DischargesWithin = Math.Max(0f, spec.DischargesWithin);
            run.Sink = Math.Max(0f, spec.Sink);
            run.Leash = Math.Max(0f, spec.Leash);
            run.LeashToPlayer = spec.LeashToPlayer;
            run.SharesSpells = spec.SharesSpells;
            run.FlankLeast = Math.Max(0f, spec.FlankLeast);
            run.FlankMost = Math.Max(0f, spec.FlankMost);
            run.TheirBlowPoints = Math.Max(0, spec.TheirBlowPoints);
            run.TurnedPoints = Math.Max(0, spec.TurnedPoints);
            if (fresh) run.Anchor = who != null ? who.transform.position : Vector3.zero;
            // A chain to another enemy is kept where its other end can find it.
            if ((spec.Flags & ConditionFlags.Chained) != 0 && link != null && who != null) EnemyChains.Note(who);
            run.FreezeLonger = Math.Max(0f, spec.FreezeLonger);
            run.ShatterHarder = Math.Max(1f, spec.ShatterHarder);
            // The way it faced as it was set, for a condition whose side is the side it showed then (Rime's crust).
            if (fresh)
            {
                Vector3 facing = spec.KeepsFacing && who != null ? who.transform.forward : Vector3.zero;
                facing.y = 0f;
                run.Facing = facing;
            }
            Recount();
            if (fresh) Tell(ConditionMoment.Began, in run, 0f, false, false);
            return slot;
        }

        /// <summary>Advances every running condition: beats come, and those whose time is up end.</summary>
        public void Tick(float deltaTime)
        {
            if (spentCount > 0) ForgetSpent();
            if (Count == 0) return;
            int ended = 0;
            for (int i = 0; i < Slots; i++)
            {
                if (!runs[i].Active) continue;
                ref var run = ref runs[i];
                run.Left -= deltaTime;
                // One that lasts only while another holds ends with it (New Face's opening, while the enemy lies felled).
                if (run.Left <= 0f || (!string.IsNullOrEmpty(run.WhileIn) && !Holds(run.WhileIn)))
                {
                    EndInto(i, ref ended);
                    continue;
                }
                if (run.Beat <= 0f) continue;
                run.BeatClock -= deltaTime;
                while (run.BeatClock <= 0f)
                {
                    run.BeatClock += run.Beat;
                    run.Count++;
                    Tell(ConditionMoment.Beat, in run, 0f, false, false);
                }
            }
            TellEnded(ended);
        }

        /// <summary>
        /// Ends, and tells the end of, each condition that lasts only while afflictions are carried and has lost one.
        /// <paramref name="present"/> is the enemy's <see cref="AfflictionSet.PresentMask"/>.
        /// </summary>
        public void Keep(int present)
        {
            if (Count == 0) return;
            int ended = 0;
            for (int i = 0; i < Slots; i++)
                if (runs[i].Active && runs[i].WhileMask != 0 && (present & runs[i].WhileMask) != runs[i].WhileMask) EndInto(i, ref ended);
            TellEnded(ended);
        }

        /// <summary>The poison on the enemy bit: each condition that beats on the bite comes to its beat.</summary>
        public void Bitten()
        {
            if (Count == 0) return;
            for (int i = 0; i < Slots; i++)
            {
                if (!runs[i].Active || !runs[i].OnBite) continue;
                runs[i].Count++;
                Tell(ConditionMoment.Beat, in runs[i], 0f, false, false);
            }
        }

        /// <summary>
        /// A blow landed on the enemy: each condition counts it (weapon blows only), each that answers blows is told, and
        /// each the blow spends, or that has had its last blow, ends without its end being told. A reaction's own blow is
        /// not one (the caller keeps those out).
        /// </summary>
        public void Struck(float damage, bool heavy, bool spell, bool shattered = false, bool critical = false, bool finisher = false,
            Vector3 travel = default, Vector3 facing = default)
        {
            if (Count == 0) return;
            bool spentAny = false;
            for (int i = 0; i < Slots; i++)
            {
                if (!runs[i].Active) continue;
                ref var run = ref runs[i];
                // One that hears only critical blows lets every other blow pass as if it were not there (Kindling Point's shapes).
                if (run.CriticalOnly && (spell || !critical)) continue;
                // One that counts its carrier's blows does not count yours (Witnessed).
                if (!spell && run.TheirBlowPoints == 0) run.Blows++;
                blowSide = SideDegrees(run.Facing.sqrMagnitude > 0.0001f ? run.Facing : facing, travel);
                if (run.AnswersStruck) Tell(ConditionMoment.Struck, in run, damage, heavy, spell, shattered, critical, finisher && !spell);
                blowSide = -1f;
                if (!SpentBy(in run, heavy, spell, finisher)) continue;
                NoteSpent(in run);
                runs[i] = default;
                spentAny = true;
            }
            if (spentAny) Recount();
        }

        /// <summary>
        /// The enemy struck at the player and the blow landed or was turned: each condition that answers it is told, and
        /// each that such a blow spends (<see cref="ConditionBlow.TheirBlow"/>) ends without its end being told.
        /// </summary>
        public void Attacked(bool turned = false)
        {
            if (Count == 0) return;
            bool spentAny = false;
            for (int i = 0; i < Slots; i++)
            {
                if (!runs[i].Active) continue;
                ref var run = ref runs[i];
                run.Blows += TheirBlowCount(run.TheirBlowPoints, run.TurnedPoints, turned);
                if (run.AnswersAttack) Tell(ConditionMoment.Attacked, in run, 0f, false, false, turned: turned);
                if (run.EndsWith != ConditionBlow.TheirBlow) continue;
                runs[i] = default;
                spentAny = true;
            }
            if (spentAny) Recount();
        }

        /// <summary>
        /// The points one of its carrier's blows on the player adds to a condition that counts them: <paramref name="each"/>, and
        /// <paramref name="turned"/> more when a perfect guard turned it; nothing for one that does not count them.
        /// </summary>
        public static int TheirBlowCount(int each, int turnedPoints, bool turned) => each <= 0 ? 0 : each + (turned ? Math.Max(0, turnedPoints) : 0);

        /// <summary>True when a linked condition joins it to <paramref name="host"/>, with the share the link passes on.</summary>
        public bool LinkedTo(Component host, out float share)
        {
            share = 0f;
            if (host == null || !Has(ConditionFlags.Linked)) return false;
            for (int i = 0; i < Slots; i++)
            {
                if (!runs[i].Active || (runs[i].Flags & ConditionFlags.Linked) == 0 || runs[i].Link != host) continue;
                share = runs[i].Scale;
                return true;
            }
            return false;
        }

        /// <summary>Everything ends at once and nothing is told: the enemy died or was set back.</summary>
        public void Clear()
        {
            for (int i = 0; i < Slots; i++) runs[i] = default;
            ForgetSpent();
            Recount();
        }

        private static int Bit(AfflictionKind kind) => kind == AfflictionKind.None ? 0 : 1 << (int)kind;

        private static bool SpentBy(in ConditionRun run, bool heavy, bool spell, bool finisher = false)
        {
            if (run.BlowLimit > 0 && run.Blows >= run.BlowLimit) return true;
            switch (run.EndsWith)
            {
                case ConditionBlow.Any: return true;
                case ConditionBlow.Weapon: return !spell;
                case ConditionBlow.Heavy: return heavy && !spell;
                case ConditionBlow.Finisher: return finisher && !spell;
                case ConditionBlow.Shove: return heavy && !spell;
                default: return false;
            }
        }

        // Taken out of its slot now; its end is told by TellEnded, once the rest are counted again.
        private void EndInto(int slot, ref int ended)
        {
            runs[slot].Active = false;
            endedRuns[ended++] = runs[slot];
            runs[slot] = default;
        }

        private void TellEnded(int ended, bool frozen = false)
        {
            if (ended == 0) return;
            // Counted again first, so whoever hears of an end finds the enemy as it now is.
            Recount();
            for (int i = 0; i < ended; i++)
            {
                Tell(ConditionMoment.Ended, in endedRuns[i], 0f, false, false, frozen: frozen);
                endedRuns[i] = default;
            }
        }

        /// <summary>
        /// Takes a condition that drifted from another enemy, whole: its time left, beats, blows and answers as it ran there
        /// (<see cref="ConditionSpec.DriftsTo"/>). It takes a free slot, or the place of the one nearest its end. Returns the
        /// slot, or -1 when nothing was given.
        /// </summary>
        public int Adopt(in ConditionRun from, Component who)
        {
            if (!from.Active || string.IsNullOrEmpty(from.Id) || from.Left <= 0f) return -1;
            if (who != null) Who = who;
            int slot = -1;
            for (int i = 0; i < Slots && slot < 0; i++)
                if (!runs[i].Active) slot = i;
            if (slot < 0)
            {
                slot = 0;
                for (int i = 1; i < Slots; i++)
                    if (runs[i].Left < runs[slot].Left) slot = i;
            }
            runs[slot] = from;
            Recount();
            return slot;
        }

        /// <summary>
        /// The enemy was shoved or thrown, not struck (the ledger's shove-moment): each condition that answers a blow is told
        /// of it as a shove, and one a shove ends (<see cref="ConditionBlow.Shove"/>) ends. Returns how many were told.
        /// </summary>
        public int Shoved()
        {
            if (Count == 0) return 0;
            int told = 0;
            bool spentAny = false;
            for (int i = 0; i < Slots; i++)
            {
                if (!runs[i].Active) continue;
                ref var run = ref runs[i];
                if (run.AnswersStruck)
                {
                    Tell(ConditionMoment.Struck, in run, 0f, false, false, shoved: true);
                    told++;
                }
                if (run.EndsWith != ConditionBlow.Shove) continue;
                NoteSpent(in run);
                runs[i] = default;
                spentAny = true;
            }
            if (spentAny) Recount();
            return told;
        }

        /// <summary>
        /// The enemy has just frozen: each condition that ends on a freeze (<see cref="ConditionSpec.EndsOnFreeze"/>) ends
        /// now, and its end is told as the freeze's. Returns how many ended.
        /// </summary>
        public int Froze()
        {
            if (Count == 0) return 0;
            int ended = 0;
            for (int i = 0; i < Slots; i++)
                if (runs[i].Active && runs[i].EndsOnFreeze) EndInto(i, ref ended);
            TellEnded(ended, true);
            return ended;
        }

        /// <summary>
        /// Ends every condition of this name (or what it is) now, its end told as when its time runs out (the ledger's
        /// condition-ends-now: Cold Snap makes a running Sleet Charge strike at once). Returns how many ended.
        /// </summary>
        public int EndNow(string id)
        {
            if (Count == 0 || string.IsNullOrEmpty(id)) return 0;
            int ended = 0;
            for (int i = 0; i < Slots; i++)
                if (runs[i].Active && (runs[i].Id == id || runs[i].Is == id)) EndInto(i, ref ended);
            TellEnded(ended);
            return ended;
        }

        // A mark of -2 asks only for the name, from whichever mark and joined to whatever.
        private int Find(string id, Component link, int mark)
        {
            for (int i = 0; i < Slots; i++)
            {
                if (!runs[i].Active || runs[i].Id != id) continue;
                if (mark == -2 || (runs[i].Mark == mark && runs[i].Link == link)) return i;
            }
            return -1;
        }

        private void Recount()
        {
            int count = 0, keeps = 0;
            var flags = ConditionFlags.None;
            float pace = 1f, windup = 1f, taken = 1f, dealt = 1f, weapon = 1f, spellTaken = 1f, stagger = 1f, sight = 0f, heavyTaken = 1f, weaponStagger = 1f, longer = 0f, shatter = 1f, turn = 1f, sink = 0f;
            for (int i = 0; i < Slots; i++)
            {
                if (!runs[i].Active) continue;
                ref var run = ref runs[i];
                count++;
                flags |= run.Flags;
                keeps |= run.KeepsMask;
                pace *= run.Pace;
                if ((run.Flags & ConditionFlags.WindupSlowed) != 0) windup *= run.Scale;
                if ((run.Flags & ConditionFlags.DamageTakenShare) != 0) taken *= run.Scale;
                if ((run.Flags & ConditionFlags.DamageDealtShare) != 0) dealt *= run.Scale;
                if ((run.Flags & ConditionFlags.WeaponTakenShare) != 0) weapon *= run.Scale;
                if ((run.Flags & ConditionFlags.SpellTakenShare) != 0) spellTaken *= run.Scale;
                if ((run.Flags & ConditionFlags.StaggerTakenShare) != 0) stagger *= run.Scale;
                if ((run.Flags & ConditionFlags.HeavyTakenShare) != 0) heavyTaken *= run.Scale;
                if ((run.Flags & ConditionFlags.WeaponStaggerShare) != 0) weaponStagger *= run.Scale;
                if ((run.Flags & ConditionFlags.TurnSlowed) != 0) turn *= run.Scale;
                if ((run.Flags & ConditionFlags.CannotTurn) != 0) turn = 0f;
                longer = Math.Max(longer, run.FreezeLonger);
                sink = Math.Max(sink, run.Sink);
                shatter = Math.Max(shatter, run.ShatterHarder);
                if ((run.Flags & ConditionFlags.SightShort) != 0) sight = sight > 0f ? Math.Min(sight, run.Sight) : run.Sight;
            }
            Count = count;
            Flags = flags;
            Keeps = keeps;
            Pace = pace;
            WindupPace = Math.Max(0.05f, windup);
            TakenScale = taken;
            DealtScale = dealt;
            TurnScale = Math.Max(0f, turn);
            WeaponTakenScale = weapon;
            SpellTakenScale = spellTaken;
            StaggerScale = stagger;
            HeavyTakenScale = heavyTaken;
            WeaponStaggerScale = weaponStagger;
            FreezeLonger = longer;
            ShatterScale = shatter;
            Sight = sight;
            SinkDepth = sink;
        }

        private void Tell(ConditionMoment moment, in ConditionRun run, float damage, bool heavy, bool spell, bool shattered = false, bool critical = false, bool finisher = false,
            bool turned = false, bool frozen = false, bool shoved = false)
        {
            var news = new ConditionNews
            {
                Moment = moment, Id = run.Id, Mark = run.Mark, Count = run.Count, Blows = run.Blows, Flags = run.Flags, Scale = run.Scale, Link = run.Link,
                Damage = damage, Heavy = heavy, Spell = spell, Shattered = shattered, Critical = critical, Is = run.Is, Finisher = finisher, Side = blowSide,
                Turned = turned, Frozen = frozen, Shoved = shoved,
            };
            if (Report != null) Report(Who, news);
            else CombatEvents.RaiseCondition(Who, news);
        }
    }
}

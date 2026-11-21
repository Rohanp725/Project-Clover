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
        /// <summary>What the damage it takes from weapon blows, and from spells, is multiplied by beyond <see cref="TakenScale"/>.</summary>
        public float WeaponTakenScale { get; private set; } = 1f;
        public float SpellTakenScale { get; private set; } = 1f;
        /// <summary>What the stagger it takes is multiplied by.</summary>
        public float StaggerScale { get; private set; } = 1f;
        /// <summary>How far it sees while its sight is cut short; 0 when it is not.</summary>
        public float Sight { get; private set; }
        /// <summary>The afflictions kept from fading and from being spent by blows, as bits at <c>1 &lt;&lt; (int)kind</c>.</summary>
        public int Keeps { get; private set; }

        public bool Has(ConditionFlags flag) => (Flags & flag) != 0;

        /// <summary>True when a running condition keeps this affliction from fading.</summary>
        public bool KeepsKind(AfflictionKind kind) => (Keeps & (1 << (int)kind)) != 0;

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

        public bool Holds(string id) => Find(id, null, -2) >= 0;

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
        public int Set(ConditionSpec spec, Component who, int mark = -1, Component link = null, bool answersStruck = false, bool answersEnd = false, float slowShare = 1f,
            bool answersDeath = false, bool answersAttack = false)
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
            run.Left = Math.Max(fresh ? 0f : run.Left, seconds);
            run.Beat = spec.OnBite ? 0f : spec.Beat;
            run.Flags = spec.Flags;
            run.Pace = 1f - (1f - Mathf.Clamp01(spec.Pace)) * Mathf.Clamp01(slowShare);
            run.Scale = spec.Scale;
            run.Sight = spec.Sight;
            run.AnswersStruck = answersStruck;
            run.AnswersEnd = answersEnd;
            run.AnswersDeath = answersDeath;
            run.AnswersAttack = answersAttack;
            run.KeepsMask = Bit(spec.Keeps);
            run.DeepMask = Bit(spec.Deepens);
            run.DeepBy = Math.Max(0, spec.DeepensBy);
            run.WhileMask = Bit(spec.While) | Bit(spec.While2);
            run.OnBite = spec.OnBite;
            run.EndsWith = spec.EndsWith;
            run.BlowLimit = Math.Max(0, spec.Blows);
            Recount();
            if (fresh) Tell(ConditionMoment.Began, in run, 0f, false, false);
            return slot;
        }

        /// <summary>Advances every running condition: beats come, and those whose time is up end.</summary>
        public void Tick(float deltaTime)
        {
            if (Count == 0) return;
            int ended = 0;
            for (int i = 0; i < Slots; i++)
            {
                if (!runs[i].Active) continue;
                ref var run = ref runs[i];
                run.Left -= deltaTime;
                if (run.Left <= 0f)
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
        public void Struck(float damage, bool heavy, bool spell, bool shattered = false)
        {
            if (Count == 0) return;
            bool spentAny = false;
            for (int i = 0; i < Slots; i++)
            {
                if (!runs[i].Active) continue;
                ref var run = ref runs[i];
                if (!spell) run.Blows++;
                if (run.AnswersStruck) Tell(ConditionMoment.Struck, in run, damage, heavy, spell, shattered);
                if (!SpentBy(in run, heavy, spell)) continue;
                runs[i] = default;
                spentAny = true;
            }
            if (spentAny) Recount();
        }

        /// <summary>
        /// The enemy struck at the player and the blow landed or was turned: each condition that answers it is told, and
        /// each that such a blow spends (<see cref="ConditionBlow.TheirBlow"/>) ends without its end being told.
        /// </summary>
        public void Attacked()
        {
            if (Count == 0) return;
            bool spentAny = false;
            for (int i = 0; i < Slots; i++)
            {
                if (!runs[i].Active) continue;
                ref var run = ref runs[i];
                if (run.AnswersAttack) Tell(ConditionMoment.Attacked, in run, 0f, false, false);
                if (run.EndsWith != ConditionBlow.TheirBlow) continue;
                runs[i] = default;
                spentAny = true;
            }
            if (spentAny) Recount();
        }

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
            Recount();
        }

        private static int Bit(AfflictionKind kind) => kind == AfflictionKind.None ? 0 : 1 << (int)kind;

        private static bool SpentBy(in ConditionRun run, bool heavy, bool spell)
        {
            if (run.BlowLimit > 0 && run.Blows >= run.BlowLimit) return true;
            switch (run.EndsWith)
            {
                case ConditionBlow.Any: return true;
                case ConditionBlow.Weapon: return !spell;
                case ConditionBlow.Heavy: return heavy && !spell;
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

        private void TellEnded(int ended)
        {
            if (ended == 0) return;
            // Counted again first, so whoever hears of an end finds the enemy as it now is.
            Recount();
            for (int i = 0; i < ended; i++)
            {
                Tell(ConditionMoment.Ended, in endedRuns[i], 0f, false, false);
                endedRuns[i] = default;
            }
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
            float pace = 1f, windup = 1f, taken = 1f, dealt = 1f, weapon = 1f, spellTaken = 1f, stagger = 1f, sight = 0f;
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
                if ((run.Flags & ConditionFlags.SightShort) != 0) sight = sight > 0f ? Math.Min(sight, run.Sight) : run.Sight;
            }
            Count = count;
            Flags = flags;
            Keeps = keeps;
            Pace = pace;
            WindupPace = Math.Max(0.05f, windup);
            TakenScale = taken;
            DealtScale = dealt;
            WeaponTakenScale = weapon;
            SpellTakenScale = spellTaken;
            StaggerScale = stagger;
            Sight = sight;
        }

        private void Tell(ConditionMoment moment, in ConditionRun run, float damage, bool heavy, bool spell, bool shattered = false)
        {
            var news = new ConditionNews
            {
                Moment = moment, Id = run.Id, Mark = run.Mark, Count = run.Count, Blows = run.Blows, Flags = run.Flags, Scale = run.Scale, Link = run.Link,
                Damage = damage, Heavy = heavy, Spell = spell, Shattered = shattered,
            };
            if (Report != null) Report(Who, news);
            else CombatEvents.RaiseCondition(Who, news);
        }
    }
}

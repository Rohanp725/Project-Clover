using System;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The timed states on one enemy that reactions and marks set (IMPLEMENTATION.md 3.7, README's most-needed engine
    /// piece): six fixed slots, each with a name, its time, a beat, the flags the enemy reads, and the mark that set it.
    /// It says when one begins, beats, is struck and ends, through <see cref="CombatEvents.Condition"/>, and the
    /// listener runs the setting mark's answers. Held by each <see cref="AfflictionSet"/>, which ticks and clears it.
    /// Plain rules with nothing allocated after it is made.
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
        /// <summary>How far it sees while its sight is cut short; 0 when it is not.</summary>
        public float Sight { get; private set; }

        public bool Has(ConditionFlags flag) => (Flags & flag) != 0;

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
        /// over (never shortened), its numbers are the new ones, and its beats are kept. Otherwise it takes a free slot,
        /// or the place of the one nearest its end. <paramref name="slowShare"/> is how much of the slowing takes (an
        /// elite's is half). Returns the slot, or -1 when nothing was asked.
        /// </summary>
        public int Set(ConditionSpec spec, Component who, int mark = -1, Component link = null, bool answersStruck = false, bool answersEnd = false, float slowShare = 1f)
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
            run.Left = Math.Max(fresh ? 0f : run.Left, spec.Seconds);
            run.Beat = spec.Beat;
            run.Flags = spec.Flags;
            run.Pace = 1f - (1f - Mathf.Clamp01(spec.Pace)) * Mathf.Clamp01(slowShare);
            run.Scale = spec.Scale;
            run.Sight = spec.Sight;
            run.AnswersStruck = answersStruck;
            run.AnswersEnd = answersEnd;
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
                    run.Active = false;
                    endedRuns[ended++] = run;
                    run.Link = null;
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
            if (ended == 0) return;
            // Counted again first, so whoever hears of an end finds the enemy as it now is.
            Recount();
            for (int i = 0; i < ended; i++)
            {
                Tell(ConditionMoment.Ended, in endedRuns[i], 0f, false, false);
                endedRuns[i] = default;
            }
        }

        /// <summary>A blow landed on the enemy: each condition that answers blows is told. A reaction's own blow is not one (the caller keeps those out).</summary>
        public void Struck(float damage, bool heavy, bool spell)
        {
            if (Count == 0) return;
            for (int i = 0; i < Slots; i++)
                if (runs[i].Active && runs[i].AnswersStruck) Tell(ConditionMoment.Struck, in runs[i], damage, heavy, spell);
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
            int count = 0;
            var flags = ConditionFlags.None;
            float pace = 1f, windup = 1f, taken = 1f, dealt = 1f, sight = 0f;
            for (int i = 0; i < Slots; i++)
            {
                if (!runs[i].Active) continue;
                ref var run = ref runs[i];
                count++;
                flags |= run.Flags;
                pace *= run.Pace;
                if ((run.Flags & ConditionFlags.WindupSlowed) != 0) windup *= run.Scale;
                if ((run.Flags & ConditionFlags.DamageTakenShare) != 0) taken *= run.Scale;
                if ((run.Flags & ConditionFlags.DamageDealtShare) != 0) dealt *= run.Scale;
                if ((run.Flags & ConditionFlags.SightShort) != 0) sight = sight > 0f ? Math.Min(sight, run.Sight) : run.Sight;
            }
            Count = count;
            Flags = flags;
            Pace = pace;
            WindupPace = Math.Max(0.05f, windup);
            TakenScale = taken;
            DealtScale = dealt;
            Sight = sight;
        }

        private void Tell(ConditionMoment moment, in ConditionRun run, float damage, bool heavy, bool spell)
        {
            var news = new ConditionNews
            {
                Moment = moment, Id = run.Id, Mark = run.Mark, Count = run.Count, Flags = run.Flags, Scale = run.Scale, Link = run.Link,
                Damage = damage, Heavy = heavy, Spell = spell,
            };
            if (Report != null) Report(Who, news);
            else CombatEvents.RaiseCondition(Who, news);
        }
    }
}

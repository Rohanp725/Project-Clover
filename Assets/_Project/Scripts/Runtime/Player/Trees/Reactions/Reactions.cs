using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// The plain reactions (IMPLEMENTATION.md 2.4; docs/design/steel/pairs_*.md as amended by review_pairs.md): one hidden
    /// mark per pair, never drawn or sold and always in force, whose effects ride <see cref="UpgradeTrigger.Reacted"/>
    /// with the enemy that reacted as their target. What they lay starts no reaction of its own. Phase 3 task 3.2 builds
    /// the design's first six, each at its simplest version that keeps its intent; "NEEDS" names the engine piece a
    /// fuller version waits for (README.md, "What the engine needs"). Task 3.4 adds the other 49 in pair order.
    /// The look is a placeholder (<see cref="ReactionLook"/>) until the reactions' own effects are approved.
    /// </summary>
    public static class Reactions
    {
        // Conditions the six set, by name: their answers name them too.
        public const string LiveFlame = "flashover.live";
        public const string SunkInSand = "groundgives.sunk";
        public const string IronLive = "liveiron.live";
        public const string Roots = "rotroots.roots";

        public static void Add(List<Upgrade> to)
        {
            // Fire and Lightning (1). Flame carries current: the Static on it discharges through the fire at once, and
            // none of the Burn is spent. The discharge is Static's own set-off (7 a stack in 3 m) with 4 stagger a stack
            // (20 at full depth), and it flinches.
            // NEEDS: discharges that bend toward one enemy: for 4 s every discharge or arc within 6 m bends into the live
            // flame and bursts again (5 a Burn stack in 2.5 m, once every 0.8 s). The live flame is set as a condition so
            // that piece has something to find.
            to.Add(Reaction(AfflictionKind.Burn, AfflictionKind.Static,
                "Static meets Burn: the flame on it goes live and the Static discharges through it at once, 7 a stack in 3 m. The Burn is not spent.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 3f, Stagger = 4f },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Flinch, Seconds = 0.4f },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = LiveFlame, Seconds = 4f } }));

            // Fire and Wind (12). The wind strips three embers off it: they fly 5 m downwind (away from the player, along
            // the line to it) and land 1 m apart as small fires (1 m, 3 s, 3 a bite, 1 Burn); whatever stands within
            // 1.5 m of a landing is set alight as they land. The player aims the fire by where they stand.
            to.Add(Reaction(AfflictionKind.Burn, AfflictionKind.Unsteady,
                "Burn meets Unsteady: the wind strips three embers off it and carries them 5 m on, away from you. Where they land small fires burn, and lights what stands near.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Shape = HazardShape.Downwind, Count = 3, Reach = 5f, Amount = 1f,
                    Radius = 1f, Seconds = 3f, Interval = 0.5f, Damage = 3f, Affliction = AfflictionKind.Burn, Stacks = 1, Delay = 0.35f },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Shape = HazardShape.Downwind, Count = 3, Reach = 5f, Amount = 1f,
                    Radius = 1.5f, Damage = 0f, Affliction = AfflictionKind.Burn, Stacks = 1, Delay = 0.35f }));

            // Water and Earth (16). Shaken wet ground turns to sand: it sinks to the knees for 4 s, slowed by half (an
            // elite by a quarter; a god takes only the Tremor), and a heavy blow on it while sunk holds it 0.5 s.
            // NEEDS: sinking (no dodge, leap or step back, the body drawn lower) and the pool itself (2 m across, slowing
            // other enemies that wade in by 30%).
            to.Add(Reaction(AfflictionKind.Soaked, AfflictionKind.Tremor,
                "Tremor meets Soaked: the wet floor under it turns to sand and it sinks to the knees for 4 s, slowed by half. A heavy blow holds it fast.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = SunkInSand, Seconds = 4f, Pace = 0.5f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = SunkInSand, By = StruckBy.Heavy, Do = UpgradeAction.Hold, Seconds = 0.5f }));

            // Fire and Water (34). The water flashes off the burning body: Soaked is spent and half the Burn (rounded up)
            // goes with it, in a scalding puff of 2.5 m: 8 damage and 4 more for each Burn spent, 15 stagger, to every
            // enemy in it. Once every 3 s on one enemy.
            to.Add(Reaction(AfflictionKind.Burn, AfflictionKind.Soaked,
                "Burn meets Soaked: the water flashes off in a scalding puff, 8 and 4 for each Burn it takes, to all within 2.5 m. The water is gone and half the Burn with it.",
                3f,
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Soaked },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Burn, Share = 0.5f },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Affliction = AfflictionKind.Soaked, Radius = 2.5f, Damage = 8f, DamagePerSpent = 4f, Stagger = 15f }));

            // Life and Poison (50). Pale roots run from its feet to up to three other enemies within 5 m and hold 6 s; each
            // is Seeded once as they arrive, and every bite of poison on it bites each of them for half.
            // NEEDS: the roots seen on the floor (grey-white threads, a drip running along them with each bite).
            to.Add(Reaction(AfflictionKind.Seeded, AfflictionKind.Poison,
                "Seeded meets Poisoned: roots run from it to three enemies near and hold 6 s. Each is Seeded, and every bite of poison on it bites them too, for half.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, Radius = 5f, Count = 3, Affliction = AfflictionKind.Seeded, Stacks = 1,
                    State = new ConditionSpec { Id = Roots, Seconds = 6f, Flags = ConditionFlags.Linked, Scale = 0.5f, ToOthers = true } }));

            // Metal and Lightning (52). For 6 s every weapon blow on it arcs only from iron to iron: to the nearest other
            // Sundered enemy within 8 m, never a clean one, for 35% of the blow, laying a notch where it lands; it leaps
            // once more from there (two at most; one arc every 0.3 s).
            to.Add(Reaction(AfflictionKind.Sundered, AfflictionKind.Static,
                "Sundered meets Static: for 6 s every blow of yours on it arcs to the nearest other notched enemy within 8 m, for a third of the blow, and notches it.",
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = IronLive, Seconds = 6f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = IronLive, By = StruckBy.Weapon, Cooldown = 0.3f, Do = UpgradeAction.Arc,
                    LeapsTo = AfflictionKind.Sundered, Count = 2, Reach = 8f, Amount = 0.35f, Affliction = AfflictionKind.Sundered, Stacks = 1 }));
        }

        private static Upgrade Reaction(AfflictionKind one, AfflictionKind other, string words, params UpgradeEffect[] effects) =>
            Reaction(one, other, words, 0f, effects);

        // A pair's plain reaction, named and keyed as the pair is, on the two elements whose afflictions they are.
        private static Upgrade Reaction(AfflictionKind one, AfflictionKind other, string words, float rest, params UpgradeEffect[] effects)
        {
            int pair = AfflictionPairs.Index(one, other);
            return new Upgrade
            {
                Id = AfflictionPairs.Id(pair), Name = AfflictionPairs.Name(pair), Kind = UpgradeKind.Reaction, Tree = MarkTree.Hidden,
                Element = ElementOf(AfflictionPairs.First(pair)), Element2 = ElementOf(AfflictionPairs.Second(pair)),
                Description = words, Reaction = new ReactionDef { Rest = rest }, Effects = effects,
            };
        }

        private static string ElementOf(AfflictionKind kind)
        {
            foreach (var info in Elements.All)
                if (info.Affliction == kind) return info.Name;
            return "";
        }
    }
}

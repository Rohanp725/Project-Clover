using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Life and Light (pairs_d3.md, "Life and Light", distance 3; task 4.16), named by its reaction,
    /// Shining Herb. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark
    /// opens Bitter Leaf and Seed of Light; Sixth Moon and Open to the Sun (majors), Long Day (a minor), the keystone The
    /// Whole Mountain, and the duo spell Turned to the Sun. Life is written first.
    /// </summary>
    public static class ShiningHerbDuo
    {
        public const string Shape = "shiningherb.shape";
        public const string SixthMoon = "shiningherb.sixthmoon";
        public const string OpenToTheSun = "shiningherb.opentosun";
        public const string LongDay = "shiningherb.longday";
        public const string TheWholeMountain = "shiningherb.wholemountain";
        public const string TurnedToTheSun = "shiningherb.turnedtosun";
        /// <summary>The conditions the two shapes set: the herb on the enemy, and the seed of light waiting for a blow.</summary>
        public const string BitterLeaf = "shiningherb.bitter";
        public const string SeedOfLight = "shiningherb.seedoflight";
        /// <summary>Bitter Leaf's opening after the picking: the next weapon blow is critical.</summary>
        public const string Picked = "shiningherb.picked";
        private const string Reaction = "react.shiningherb";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Life, B = Elements.Light;

            // The leaf is picked and the seed burst only by a critical blow (ConditionSpec.CriticalOnly); after the picking
            // the next weapon blow on it is critical (ConditionFlags.TakesCritical).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Shining Herb: Bitter Leaf or Seed of Light", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Shining Herb's two shapes, chosen at a rest fire. Bitter Leaf: the herb grows on the enemy, and your next blow on it picks it. Seed of Light: no herb; the seed bursts at your next blow as a flare. Either runs instead of the plain Shining Herb.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Bitter Leaf",
                        Description = "The herb grows on the enemy, not on the floor, for 6 s: your next critical blow on it picks it: you heal 8, and your next blow on it is critical.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = BitterLeaf, Seconds = 6f, EndsWith = ConditionBlow.Weapon, CriticalOnly = true } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = BitterLeaf, By = StruckBy.Critical, Do = UpgradeAction.Heal, Amount = 8f },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = BitterLeaf, By = StruckBy.Critical, Do = UpgradeAction.Condition,
                                State = new ConditionSpec { Id = Picked, Seconds = 4f, Flags = ConditionFlags.TakesCritical, EndsWith = ConditionBlow.Weapon } },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Seed of Light",
                        Description = "No herb: for 6 s the seed waits, and your next critical blow on it bursts it as a flare that Marks and Seeds every enemy within 4 m.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = SeedOfLight, Seconds = 6f, EndsWith = ConditionBlow.Weapon, CriticalOnly = true } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = SeedOfLight, By = StruckBy.Critical, Do = UpgradeAction.Afflict, Radius = 4f, Affliction = AfflictionKind.Marked, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = SeedOfLight, By = StruckBy.Critical, Do = UpgradeAction.Afflict, Radius = 4f, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                        },
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = SixthMoon, Name = "Sixth Moon", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Moon,
                Description = "The blow that ends a chain on a Seeded enemy Marks it.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.ChainEnd, Needs = AfflictionKind.Seeded, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Marked, Stacks = 1 } },
            }, HeartPattern.LeftLow));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = OpenToTheSun, Name = "Open to the Sun", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Sun,
                Description = "A perfect guard against a Marked enemy seeds it three deep.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Marked, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Seeded, Stacks = 3 } },
            }, HeartPattern.RightLow));

            // Changes its reaction's own numbers while it is in force (reaction-numbers, ReactionNumbers):
            // herbs standing 9 s before they flower, not 6.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = LongDay, Name = "Long Day", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { SixthMoon }, Icon = UpgradeIcons.Hourglass,
                Description = "Herbs stand 3 s longer before they flower.",
                ReactionChanges = new[]
                {
                    new ReactionChange { Shape = -1, Index = 0, ConditionSeconds = 9f },
                    new ReactionChange { Shape = 0, Index = 0, ConditionSeconds = 9f },
                    new ReactionChange { Shape = 1, Index = 0, ConditionSeconds = 9f },
                },
            }, HeartPattern.LeftHigh));

            // NEEDS: pickups (herbs drawn to you when one is picked; each after the first healing twice); now each herb that
            // springs heals you 4.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheWholeMountain, Name = "The Whole Mountain", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { SixthMoon, OpenToTheSun }, Icon = UpgradeIcons.Rock,
                Description = "Picking a herb draws every other standing herb to you across the floor, and each one picked after the first heals twice.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, Do = UpgradeAction.Heal, Amount = 4f } },
            }, HeartPattern.Keystone));

            // The duo spell, bought with Shards once both are carried (no god gives it): an opener on an 11 s cooldown (SpellTimings). The field form lays a lit patch at the aim
            // point that flares every 2 s, seeding (the branch's first element) and Marking what stands in it.
            // NEEDS: enemies turned to face a point (now none turn), and blows on their backs made critical.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TurnedToTheSun, Name = "Turned to the Sun", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Opener, Requires = new[] { SixthMoon, OpenToTheSun }, Icon = UpgradeIcons.Leaf,
                Description = "You plant a lit herb at the aim point for 6 s. Every 2 s it flares, and every Seeded or Marked enemy near turns to face it, so their backs are yours.",
                Form = SpellForms.Field,
                Spell = new SpellSpec { Reach = 7f, Radius = 3f, Seconds = 6f, Interval = 2f, LingerDamage = 2f, Stacks = 1 },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Marked, Stacks = 1 } },
            }, HeartPattern.Spell));
        }
    }
}

using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Ice and Life (pairs_d1_d2.md, "Ice and Life", distance 2; task 4.15), named by its reaction, Winter
    /// Seed. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark, Thaw,
    /// opens Hard Spring and Heart Under Ice; Seed Under Snow and Late Frost, the keystone Hidden in the Tree, and the duo
    /// spell Green Under the Ice. Ice is written first: the duo spell is cast in Ice.
    /// </summary>
    public static class WinterSeedDuo
    {
        public const string Shape = "winterseed.shape";
        public const string SeedUnderSnow = "winterseed.undersnow";
        public const string LateFrost = "winterseed.latefrost";
        public const string HiddenInTheTree = "winterseed.hidden";
        public const string GreenUnderTheIce = "winterseed.green";
        /// <summary>The conditions the two shapes set, each a seed asleep under the frost as the plain reaction's is.</summary>
        public const string HardSpring = "winterseed.hardspring";
        public const string HeartUnderIce = "winterseed.heart";

        // Laid as stacks, more than any affliction holds: "to full depth", whatever the depth is.
        private const int ToFull = 99;

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Ice, B = Elements.Life;

            // Hard Spring wakes only on a shatter (StruckBy.Shatter), twice as hard; a thaw wakes nothing.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Winter Seed: Hard Spring or Heart Under Ice", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Winter Seed's two shapes, chosen at a rest fire. Hard Spring: the seed wakes twice as hard and seeds all it reaches to full. Heart Under Ice: the waking mends rather than wounds. Either runs instead of the plain Winter Seed.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Hard Spring",
                        Description = "The seed sleeps under the frost and stops fading. Only when the ice is shattered does it wake, twice as hard: thorns of 24 a seed in 2.5 m, seeding all they reach to full, and you heal 3 a seed.",
                        Effects = Asleep(HardSpring, new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = HardSpring, By = StruckBy.Shatter, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Seeded, CountOnly = true },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = HardSpring, By = StruckBy.Shatter, Do = UpgradeAction.Burst, Radius = 2.5f, DamagePerSpent = 24f, Affliction = AfflictionKind.Seeded, Stacks = ToFull },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = HardSpring, By = StruckBy.Shatter, Do = UpgradeAction.Heal, Amount = 3f, PerStack = true },
                        }),
                    },
                    new ShapeDef
                    {
                        Name = "Heart Under Ice",
                        Description = "The seed sleeps under the frost and stops fading. When the cold ends the waking mends rather than wounds: thorns of 6 a seed in 2.5 m, you heal 5 a seed, and you gain a ward that turns aside one blow.",
                        Effects = Asleep(HeartUnderIce, new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = HeartUnderIce, Do = UpgradeAction.Burst, Radius = 2.5f, DamagePerSpent = 6f, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = HeartUnderIce, Do = UpgradeAction.Heal, Amount = 5f, PerStack = true },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = HeartUnderIce, Do = UpgradeAction.Ward, Count = 1 },
                        }),
                    },
                },
            }, HeartPattern.Foot));

            // NEEDS: a pickup left on the floor, cracked by stepping on it (pickups); now its burst comes as it dies.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = SeedUnderSnow, Name = "Seed Under Snow", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Snowflake,
                Description = "An enemy that dies chilled and Seeded leaves a frozen seed that cracks: you heal 8, and everything within 2.5 m is chilled and seeded.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Chill, Needs2 = AfflictionKind.Seeded, Do = UpgradeAction.Heal, Amount = 8f, Cooldown = 0.5f },
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Chill, Needs2 = AfflictionKind.Seeded, Do = UpgradeAction.Afflict, Radius = 2.5f, Affliction = AfflictionKind.Chill, Stacks = 1, Cooldown = 0.5f },
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Chill, Needs2 = AfflictionKind.Seeded, Do = UpgradeAction.Afflict, Radius = 2.5f, Affliction = AfflictionKind.Seeded, Stacks = 1, Cooldown = 0.5f },
                },
            }, HeartPattern.LeftLow));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = LateFrost, Name = "Late Frost", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard against a Seeded enemy roots it in frost: 2 Chill, held 0.8 s.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Seeded, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 2 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Seeded, Do = UpgradeAction.Hold, Seconds = 0.8f },
                },
            }, HeartPattern.RightLow));

            // Every grown thing lays a Chill when it bites (Hazards.GrownBitesMask), and the bark turning a blow is a ward given
            // every 6 s while you stand near one of them.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = HiddenInTheTree, Name = "Hidden in the Tree", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { SeedUnderSnow, LateFrost }, Icon = UpgradeIcons.Tower,
                Description = "Things you grow are rimed: while you stand within 4 m of one, the first blow that would land on you every 6 s is turned by its frozen bark.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.EverySeconds, Every = 6f, While = StatWhen.NearGrown, Do = UpgradeAction.Ward, Count = 1 } },
            }, HeartPattern.Keystone));

            // The duo spell, bought with Shards once both are carried (no god gives it): a detonator on a 14 s cooldown (SpellTimings). Lines run only to the
            // enemies within 9 m carrying both, eight at most (SpellSpec.OnlyCarrying, OnlyCarrying2); on each the Chill is spent, so its sleeping
            // seed wakes at once (Winter Seed's or a shape's), and a Frozen one is shattered first; with none reached half the wait comes
            // back (SpellSpec.MissedWait). NEEDS: one struck by your weapon in the last 2 s waking at one and a half times (count-rules).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = GreenUnderTheIce, Name = "Green Under the Ice", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Detonator, Requires = new[] { SeedUnderSnow, LateFrost }, Icon = UpgradeIcons.Leaf,
                Description = "You strike the floor and lines of green run through the frost to every enemy within 9 m: on each that carries both Chill and Seeded, the frost breaks and its sleeping seed wakes at once.",
                Form = SpellForms.Nova,
                Spell = new SpellSpec { Radius = 9f, Delay = 0.2f, Damage = 4f, Stagger = 4f, Stacks = 0, Count = 8, MissedWait = 0.5f,
                    OnlyCarrying = AfflictionKind.Chill, OnlyCarrying2 = AfflictionKind.Seeded },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Shatter },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Needs = AfflictionKind.Chill, Needs2 = AfflictionKind.Seeded, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Chill },
                },
            }, HeartPattern.Spell));
        }

        // A seed asleep under the frost as the plain Winter Seed's: its Seeded kept while the Chill lasts, the depth counted
        // as the cold ends, and the shape's own waking after.
        private static UpgradeEffect[] Asleep(string id, UpgradeEffect[] waking)
        {
            var effects = new List<UpgradeEffect>
            {
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = id, Seconds = 30f, While = AfflictionKind.Chill, Keeps = AfflictionKind.Seeded } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = id, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Seeded, CountOnly = true },
            };
            effects.AddRange(waking);
            return effects.ToArray();
        }
    }
}

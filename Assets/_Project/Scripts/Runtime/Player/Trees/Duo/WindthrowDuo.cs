using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Wind and Life (pairs_d3.md, "Wind and Life", distance 3; task 4.16), named by its reaction,
    /// Windthrow. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark
    /// opens Blown Seed and Deep Roots; Seed on the Wind and Torn Up (majors), Green Wood (a minor), the keystone The Fairy
    /// Wind, and the duo spell Thistledown, the crown. Wind is written first.
    /// </summary>
    public static class WindthrowDuo
    {
        public const string Shape = "windthrow.shape";
        public const string SeedOnTheWind = "windthrow.seedonwind";
        public const string TornUp = "windthrow.tornup";
        public const string GreenWood = "windthrow.greenwood";
        public const string TheFairyWind = "windthrow.fairywind";
        public const string Thistledown = "windthrow.thistledown";
        /// <summary>The conditions the two shapes set, each a rooting as the plain reaction's.</summary>
        public const string BlownSeed = "windthrow.blown";
        public const string DeepRoots = "windthrow.deep";
        private const string Reaction = "react.windthrow";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Wind, B = Elements.Life;

            // NEEDS: a shove moment (any shove, not only a heavy blow, as the plain reaction); blows on it healing twice the
            // seed's usual (Deep Roots; now 2 a blow).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Windthrow: Blown Seed or Deep Roots", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Windthrow's two shapes, chosen at a rest fire. Blown Seed: the shove blows its seeds off downwind instead of tearing it out. Deep Roots: it holds longer and tears out harder. Either runs instead of the plain Windthrow.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Blown Seed",
                        Description = "Rooted for 2 s. The heavy blow that would tear it out blows its seeds off downwind instead: everything up to 6 m beyond it, away from you, is Seeded two deep, and on purpose may root in turn.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = BlownSeed, Seconds = 2f, Flags = ConditionFlags.Rooted, EndsWith = ConditionBlow.Heavy } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = BlownSeed, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Shape = HazardShape.Downwind, Count = 3, Reach = 3.5f, Amount = 1.6f, Radius = 1.5f,
                                Affliction = AfflictionKind.Seeded, Stacks = 2, Cascades = true },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Deep Roots",
                        Description = "Rooted for 4 s, and each of your blows on it heals you 2. The heavy blow that tears it out deals 14 and 60 stagger, throws it far and leaves a torn pit that seeds what walks in.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = DeepRoots, Seconds = 4f, Flags = ConditionFlags.Rooted, EndsWith = ConditionBlow.Heavy } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = DeepRoots, By = StruckBy.Weapon, Do = UpgradeAction.Heal, Amount = 2f, Cooldown = 0.2f },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = DeepRoots, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 14f, Stagger = 60f, Affliction = AfflictionKind.Seeded },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = DeepRoots, By = StruckBy.Heavy, Do = UpgradeAction.Hazard, Radius = 0.75f, Seconds = 5f, Interval = 0.5f, Grown = true, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = DeepRoots, By = StruckBy.Heavy, Do = UpgradeAction.Push, Shape = HazardShape.Downwind, Amount = 6f },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // NEEDS: the dodge's own path (dodge-path); now all within 2 m of where it ends.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = SeedOnTheWind, Name = "Seed on the Wind", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Leaf,
                Description = "A dodge that passes within 2 m of an enemy leaves it Seeded and Unsteady.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, At = EffectAt.Self, Do = UpgradeAction.Afflict, Radius = 2f, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, At = EffectAt.Self, Do = UpgradeAction.Afflict, Radius = 2f, Affliction = AfflictionKind.Unsteady, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            // As built the plain reaction already tears out on a heavy blow (shove-moment); this throws a rooted enemy on.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TornUp, Name = "Torn Up", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Claw,
                Description = "A heavy blow on a rooted enemy counts as a shove: it tears it out, whatever the weapon.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Seeded, Needs2 = AfflictionKind.Unsteady, Do = UpgradeAction.Push, Shape = HazardShape.Downwind, Amount = 3f, Cooldown = 0.5f } },
            }, HeartPattern.RightLow));

            // NEEDS: a mark that changes the reaction's own numbers (reaction-numbers); until then it does nothing.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = GreenWood, Name = "Green Wood", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { SeedOnTheWind }, Icon = UpgradeIcons.Tower,
                Description = "Torn pits stand longer and seed one deeper.",
            }, HeartPattern.LeftHigh));

            // NEEDS: a small whirlwind that wanders where an enemy is torn out (now a still patch where it reacts that
            // unsteadies and seeds what stands in it, on purpose able to react).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheFairyWind, Name = "The Fairy Wind", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { SeedOnTheWind, TornUp }, Icon = UpgradeIcons.Gust,
                Description = "Where an enemy is torn out, a small whirlwind rises and wanders for 4 s, making what it passes Unsteady and Seeded, so tearing one out roots the next.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, Do = UpgradeAction.Hazard, Radius = 1.2f, Seconds = 4f, Interval = 0.5f, Affliction = AfflictionKind.Unsteady, Stacks = 1, Cascades = true },
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, Do = UpgradeAction.Hazard, Radius = 1.2f, Seconds = 4f, Interval = 1f, Affliction = AfflictionKind.Seeded, Stacks = 1, Cascades = true },
                },
            }, HeartPattern.Keystone));

            // The duo spell, the crown: a movement on a 9 s cooldown (SpellTimings). The drill form carries you through
            // enemies; each is unsteadied (the branch's first element) and seeded, so it roots as you go by.
            // NEEDS: untouchable for the drift; a ring of flowers where you form again, counting as grown and healing you; a
            // light blow within 1 s counting as a shove.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Thistledown, Name = "Thistledown", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Movement, Requires = new[] { SeedOnTheWind, TornUp }, Icon = UpgradeIcons.Wing,
                Description = "You come apart into a drift of seed-down and blow 7 m along the stick, passing through enemies. Each one passed is Seeded and Unsteady two deep, so it roots as you go by.",
                Form = SpellForms.Drill,
                Spell = new SpellSpec { Reach = 7f, Radius = 1.5f, Count = 1, Damage = 4f, Stagger = 4f, Stacks = 2, Speed = 17f },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Seeded, Stacks = 2 } },
            }, HeartPattern.Spell));
        }
    }
}

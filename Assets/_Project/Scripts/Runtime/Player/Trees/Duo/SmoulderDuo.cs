using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Dark and Fire (pairs_d1_d2.md, "Dark and Fire", distance 2; task 4.15), named by its reaction,
    /// Smoulder. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark,
    /// Banked Coals, opens Night Sun and Coal Seam; Third Eye and Deep Coal, the keystone The Sun Goes Under, and the duo
    /// spell The Burning Lake. Dark is written first, as the ring names the pair: the duo spell is cast in Dark.
    /// </summary>
    public static class SmoulderDuo
    {
        public const string Shape = "smoulder.shape";
        public const string ThirdEye = "smoulder.thirdeye";
        public const string DeepCoal = "smoulder.deepcoal";
        public const string TheSunGoesUnder = "smoulder.sungoesunder";
        public const string TheBurningLake = "smoulder.lake";
        /// <summary>The condition Night Sun sets: the fire sunk in, as the plain reaction's.</summary>
        public const string NightSun = "smoulder.nightsun";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Dark, B = Elements.Fire;

            // NEEDS: a heal by the depth of an affliction on the one struck (Night Sun, 1 a Burn stack; now 3 a blow); the coal
            // seam flaring at its end and setting off every smoulder on it (Coal Seam; now a patch that curses what stands on it,
            // on purpose able to react).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Smoulder: Night Sun or Coal Seam", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Smoulder's two shapes, chosen at a rest fire. Night Sun: while it smoulders, your blows on it heal you. Coal Seam: the fire goes down into the floor, and what stands there smoulders. Either runs instead of the plain Smoulder.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Night Sun",
                        Description = "The fire sinks in as before, deepening while the curse lasts, and flares when it ends; while it smoulders, each of your blows on it heals you 3.",
                        Effects = Sunk(NightSun, new[] { new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = NightSun, By = StruckBy.Any, Do = UpgradeAction.Heal, Amount = 3f, Cooldown = 0.2f } }),
                    },
                    new ShapeDef
                    {
                        Name = "Coal Seam",
                        Description = "The fire goes down into the floor where it reacted: a patch of smouldering ground 2.5 m across for 8 s, almost dark. Any burning enemy that stands on it is Cursed, and so smoulders.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 1.25f, Seconds = 8f, Interval = 2f, Damage = 0f, Affliction = AfflictionKind.Cursed, Stacks = 1, Cascades = true },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // A perfect guard names its attacker; the line runs from the player through it.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = ThirdEye, Name = "Third Eye", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Eye,
                Description = "A perfect guard against a burning, cursed enemy opens a line of black-edged fire from you through it, 6 m: 20, 2 Burn and a Curse to all on it.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Burn, Needs2 = AfflictionKind.Cursed, At = EffectAt.Self, Do = UpgradeAction.Lane, Reach = 6f, Radius = 0.6f, Damage = 20f, Affliction = AfflictionKind.Burn, Stacks = 2 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Burn, Needs2 = AfflictionKind.Cursed, At = EffectAt.Self, Do = UpgradeAction.Lane, Reach = 6f, Radius = 0.6f, Affliction = AfflictionKind.Cursed, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            // NEEDS: an affliction's depth raised on one enemy while it is in a condition; until then it does nothing.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = DeepCoal, Name = "Deep Coal", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Flame,
                Description = "Burn on a smouldering enemy can lie two deeper.",
            }, HeartPattern.RightLow));

            // NEEDS: every Burn laid smouldering, curse or no curse (now each blow that lands on a burning enemy while you are
            // below half health Curses it, once a second, so it smoulders).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheSunGoesUnder, Name = "The Sun Goes Under", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { ThirdEye, DeepCoal }, Icon = UpgradeIcons.Moon,
                Description = "While your health is below half, every Burn you lay smoulders, curse or no curse.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AnyHit, While = StatWhen.HalfHealth, Needs = AfflictionKind.Burn, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Cursed, Stacks = 1, Cooldown = 1f } },
            }, HeartPattern.Keystone));

            // The duo spell, bought with Shards once both are carried (no god gives it): a zone on a 16 s cooldown (SpellTimings). The field form lays a lake at the aim point;
            // it Curses what stands in it (the branch's first element) and each bite lays a Burn, so all of them smoulder.
            // NEEDS: slowing what stands in it by a fifth; an enemy killed on it drawn under, the lake growing 0.5 m and
            // lasting 1 s longer; a finisher on it flaring every smoulder on the lake.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheBurningLake, Name = "The Burning Lake", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Opener, Requires = new[] { ThirdEye, DeepCoal }, Icon = UpgradeIcons.Wave,
                Description = "A lake 4 m across opens at the aim point for 7 s: a still black surface, flame licking at its rim. Every enemy on it is Cursed and takes a Burn each second, so all of them smoulder.",
                Form = SpellForms.Field,
                Spell = new SpellSpec { Reach = 8f, Radius = 2f, Seconds = 7f, Interval = 1f, LingerDamage = 2f, Stacks = 1 },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1 } },
            }, HeartPattern.Spell));
        }

        // The fire sunk in as the plain Smoulder's: deepening every 2 s (a god's every 3 s) while the curse lasts, and flaring
        // when it ends or the enemy dies; then the shape's own answers.
        private static UpgradeEffect[] Sunk(string id, UpgradeEffect[] more)
        {
            var effects = new List<UpgradeEffect>
            {
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Ranks = EnemyRank.Plain | EnemyRank.Elite, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = id, Seconds = 30f, Beat = 2f, While = AfflictionKind.Cursed, Keeps = AfflictionKind.Burn } },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Ranks = EnemyRank.God, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = id, Seconds = 30f, Beat = 3f, While = AfflictionKind.Cursed, Keeps = AfflictionKind.Burn } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = id, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = id, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Burn },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = id, Needs = AfflictionKind.Burn, Do = UpgradeAction.Burst, Radius = 3f, Damage = 10f, PerStack = true, Affliction = AfflictionKind.Burn },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = id, Needs = AfflictionKind.Burn, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Burn, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionDied, Condition = id, Needs = AfflictionKind.Burn, Do = UpgradeAction.Burst, Radius = 3f, Damage = 10f, PerStack = true, Affliction = AfflictionKind.Burn },
                new UpgradeEffect { On = UpgradeTrigger.ConditionDied, Condition = id, Needs = AfflictionKind.Burn, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Burn, Stacks = 1 },
            };
            effects.AddRange(more);
            return effects.ToArray();
        }
    }
}

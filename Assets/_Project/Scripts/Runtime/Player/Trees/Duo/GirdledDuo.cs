using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Life and Dark (pairs_d4.md, "Life and Dark", distance 4; task 4.17), named by its reaction,
    /// Girdled. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark,
    /// Flowers or Fig, opens Corpse Flowers and Fig in the Crown; Bitter Seed and Held Down (majors), the keystone What the
    /// Yew Keeps, and the duo spell Death Turned Back, bought with Shards once both are carried. Life is written first, as
    /// the ring names the pair.
    /// </summary>
    public static class GirdledDuo
    {
        public const string Shape = "girdled.shape";
        public const string BitterSeed = "girdled.bitterseed";
        public const string HeldDown = "girdled.helddown";
        public const string WhatTheYewKeeps = "girdled.yew";
        public const string DeathTurnedBack = "girdled.deathturnedback";
        /// <summary>The prefixes of the conditions the two shapes set, each the coils as the plain reaction's; and the hold
        /// Death Turned Back's roots keep on the attacker.</summary>
        public const string CorpseFlowers = "girdled.flowers";
        public const string FigInTheCrown = "girdled.fig";
        public const string Held = "girdled.held";
        private const string Reaction = "react.girdled";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Life, B = Elements.Dark;

            // NEEDS: blows near a grown thing (at-grown): Corpse Flowers' 3 grey health made real for each weapon blow landed
            // within 3 m of a flower; now the flowers only stand. As the plain reaction, the hollow trunk (standing-things).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Girdled: Corpse Flowers or Fig in the Crown", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Girdled's two shapes, chosen at a rest fire. Corpse Flowers: when the roots let go, pale flowers stand where it stood. Fig in the Crown: an enemy that dies in the roots drops their seed on the nearest, which starts to coil. Either runs instead of the plain Girdled.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Corpse Flowers",
                        Description = "The roots coil and crush as before. When they let go, three pale flowers stand where it stood for 8 s, grown things of yours; fight beside them and your blows win back grey health.",
                        Effects = Coil(CorpseFlowers, new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = CorpseFlowers + ".tight", Do = UpgradeAction.Hazard, Shape = HazardShape.Ring, Count = 3, Reach = 1f, Radius = 0.6f,
                                Seconds = 8f, Interval = 1f, Grown = true, Limit = 3, Affliction = AfflictionKind.Seeded },
                        }),
                    },
                    new ShapeDef
                    {
                        Name = "Fig in the Crown",
                        Description = "The roots coil and crush as before. An enemy that dies in them drops their seed on the nearest enemy within 5 m: it is Seeded and Cursed, and the fig starts on it in turn.",
                        Effects = Coil(FigInTheCrown, new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.ConditionDied, Condition = FigInTheCrown + ".tight", Do = UpgradeAction.Arc, FromTarget = true, Count = 1, Reach = 5f,
                                Affliction = AfflictionKind.Cursed, Stacks = 1, Cascades = true },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionDied, Condition = FigInTheCrown + ".tight", Do = UpgradeAction.Arc, FromTarget = true, Count = 1, Reach = 5f,
                                Affliction = AfflictionKind.Seeded, Stacks = 1, Cascades = true },
                        }),
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = BitterSeed, Name = "Bitter Seed", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Leaf,
                Description = "Your finishers lay a Seed and a Curse on every enemy within 3 m.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Cursed, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            // NEEDS: a reaction started at a count of its own (reaction-numbers): the fig wound at two coils; now a Seed laid
            // and Girdled forced through its rest, at one coil.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = HeldDown, Name = "Held Down", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard against a Cursed enemy winds the fig round it at two coils, whatever else it carries. Once every 6 s.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Cursed, Cooldown = 6f, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Cursed, Cooldown = 6f, Do = UpgradeAction.ForceReaction, Reaction = Reaction },
                },
            }, HeartPattern.RightLow));

            // NEEDS: every stack passed on as it dies, the thorns withheld (depth-passed); now its seed and curse pass whole to
            // the nearest enemy within 6 m, which may set the fig going there at one coil, and the thorns still burst.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = WhatTheYewKeeps, Name = "What the Yew Keeps", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { BitterSeed, HeldDown }, Icon = UpgradeIcons.Tower,
                Description = "A Seeded, Cursed enemy that dies does not burst into thorns: its seed and its curse pass whole to the nearest enemy within 6 m, and the fig starts on it at two coils.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Seeded, Needs2 = AfflictionKind.Cursed, Cooldown = 0.3f, Do = UpgradeAction.Pass, Affliction = AfflictionKind.Seeded, CarriesDepth = true, Reach = 6f },
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Cursed, Needs2 = AfflictionKind.Seeded, Cooldown = 0.3f, Do = UpgradeAction.Pass, Affliction = AfflictionKind.Cursed, CarriesDepth = true, Reach = 6f },
                },
            }, HeartPattern.Keystone));

            // The duo spell, bought with 5 Shards once both are carried (no god gives it): a counter on a 12 s cooldown
            // (SpellTimings). The parry form spreads the roots round you for 1.2 s, cast in Life (three Seeds on what it
            // answers); the answer Curses the attacker and holds it 2 s (an elite 0.8 s), and you mend 15. Each of your blows on
            // the held enemy mends you 3.
            // NEEDS: the mending as what the blow would have dealt, 30 at most (damage-tally); half the cooldown back when
            // nothing came (missed-counter).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = DeathTurnedBack, Name = "Death Turned Back", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Counter, Requires = new[] { BitterSeed, HeldDown }, Icon = UpgradeIcons.Heart,
                Description = "You go down on one knee and roots spread round you for 1.2 s. The first blow that would land is taken by the roots: they climb the attacker and hold it 2 s, Seed it three deep and Curse it, and you mend. Each of your blows on the held enemy mends you 3.",
                Form = SpellForms.Parry,
                Spell = new SpellSpec { Seconds = 1.2f, Reach = 3f, Arc = 120f, Damage = 8f, Stagger = 20f, Stacks = 3 },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Cursed, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Hold, Seconds = 2f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Cooldown = 1f, Do = UpgradeAction.Heal, Amount = 15f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Held, Seconds = 2f } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Held, By = StruckBy.Weapon, Do = UpgradeAction.Heal, Amount = 3f },
                },
            }, HeartPattern.Spell));
        }

        // The coils as the plain Girdled's, under the shape's own names (prefix.coils, .bound, .tight): one coil at once,
        // slowed a quarter and 6% more from the weapon; each weapon blow mends 1 and draws a coil tighter, 6% more each; at
        // four coils (the third blow) held 1.5 s while the roots crush, 20 and 40 stagger, and 4 to you. Then the shape's own.
        private static UpgradeEffect[] Coil(string prefix, UpgradeEffect[] more)
        {
            string coils = prefix + ".coils", tight = prefix + ".tight";
            var effects = new List<UpgradeEffect>
            {
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = coils, Seconds = 8f, Blows = 3 } },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = prefix + ".bound", Seconds = 8f, Pace = 0.75f } },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = tight, Seconds = 8f, Flags = ConditionFlags.WeaponTakenShare, Scale = 1.06f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = coils, By = StruckBy.Weapon, Do = UpgradeAction.Heal, Amount = 1f },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = coils, By = StruckBy.Weapon, AtCount = 1, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = tight, Seconds = 8f, Flags = ConditionFlags.WeaponTakenShare, Scale = 1.12f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = coils, By = StruckBy.Weapon, AtCount = 2, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = tight, Seconds = 8f, Flags = ConditionFlags.WeaponTakenShare, Scale = 1.18f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = coils, By = StruckBy.Weapon, AtCount = 3, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = tight, Seconds = 8f, Flags = ConditionFlags.WeaponTakenShare, Scale = 1.24f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = coils, By = StruckBy.Weapon, AtCount = 3, Do = UpgradeAction.Hold, Seconds = 1.5f },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = coils, By = StruckBy.Weapon, AtCount = 3, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 20f, Stagger = 40f,
                    Affliction = AfflictionKind.Cursed },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = coils, By = StruckBy.Weapon, AtCount = 3, Do = UpgradeAction.Heal, Amount = 4f },
            };
            effects.AddRange(more);
            return effects.ToArray();
        }
    }
}

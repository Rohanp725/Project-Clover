using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Poison and Ice (pairs_d4.md, "Poison and Ice", distance 4; task 4.17), named by its reaction, Venom
    /// Rime. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark, Floor or
    /// Bone, opens Frost Over Frost and Cold Damage; Bitter Frost and Six Levels Down (majors), the keystone The Drops
    /// Quicken, and the duo spell Rime Over Rime, bought with Shards once both are carried. Poison is written first, as the
    /// ring names the pair.
    /// </summary>
    public static class VenomRimeDuo
    {
        public const string Shape = "venomrime.shape";
        public const string BitterFrost = "venomrime.bitterfrost";
        public const string SixLevelsDown = "venomrime.sixlevels";
        public const string TheDropsQuicken = "venomrime.drops";
        public const string RimeOverRime = "venomrime.rimeoverrime";
        /// <summary>The conditions the two shapes set, each the crust as the plain reaction's.</summary>
        public const string FrostOverFrost = "venomrime.floor";
        public const string ColdDamage = "venomrime.bone";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Poison, B = Elements.Ice;

            // NEEDS: an enemy freezing as a moment (freeze-moment): Frost Over Frost's rime spread as it freezes, now the first
            // bite on it while frozen (at most once in 5 s). Cold Damage's doses go off on the blow that shatters it
            // (StruckBy.Shatter: the shatter has ended the Chill before the answer, so Needs cannot ask for it). Cold Damage's
            // poison bites twice as hard while it is frozen (ConditionSpec.BitesHarderFrozen).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Venom Rime: Frost Over Frost or Cold Damage", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Venom Rime's two shapes, chosen at a rest fire. Frost Over Frost: a rimed enemy that freezes spreads its rime over the floor. Cold Damage: the cold drives the poison inward, harder while frozen and all at once when it shatters. Either runs instead of the plain Venom Rime.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Frost Over Frost",
                        Description = "Its poison does not fade and each bite lays a Chill, as before. When it freezes, its rime spreads over the floor 3 m round it for 5 s: what stands there takes a Chill and a dose each second.",
                        Effects = Crust(FrostOverFrost, new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = FrostOverFrost, Needs = AfflictionKind.Chill, NeedsFull = true, Cooldown = 5f, Do = UpgradeAction.Hazard,
                                Radius = 3f, Seconds = 5f, Interval = 1f, Affliction = AfflictionKind.Chill, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = FrostOverFrost, Needs = AfflictionKind.Chill, NeedsFull = true, Cooldown = 5f, Do = UpgradeAction.Hazard,
                                Radius = 3f, Seconds = 5f, Interval = 1f, Affliction = AfflictionKind.Poison, Stacks = 1 },
                        }),
                    },
                    new ShapeDef
                    {
                        Name = "Cold Damage",
                        Description = "Its poison does not fade and each bite lays a Chill, as before. While it is frozen its poison bites twice as hard, and when your heavy blow shatters it every dose it carries goes off at once, 6 a dose, on top of the shatter.",
                        Effects = Crust(ColdDamage, new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = ColdDamage, By = StruckBy.Shatter, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Poison },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = ColdDamage, By = StruckBy.Shatter, Do = UpgradeAction.Burst, Radius = 1f, DamagePerSpent = 6f,
                                Affliction = AfflictionKind.Poison },
                        }, 2f),
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = BitterFrost, Name = "Bitter Frost", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Fang,
                Description = "Your heavy blows lay a Chill and a dose together.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Poison, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            // NEEDS: a freeze held a set time longer (frozen-for): each bite adding 0.3 s to the freeze; now a fresh Chill laid
            // on the frozen enemy at each bite, at most every 0.3 s.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = SixLevelsDown, Name = "Six Levels Down", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Snowflake,
                Description = "Each bite on a frozen enemy keeps it frozen 0.3 s longer.",
                Effects = new[]
                {
                    // A Chill laid on a frozen enemy does nothing (the freeze is full), so the bite keeps it frozen itself.
                    new UpgradeEffect { On = UpgradeTrigger.PoisonBite, Needs = AfflictionKind.Chill, NeedsFull = true, Cooldown = 0.3f, Do = UpgradeAction.KeepFrozen, Seconds = 0.3f },
                },
            }, HeartPattern.RightLow));

            // NEEDS: a helper rising where an enemy fell and going to the nearest, laying two afflictions (helpers-answer); now a
            // helper from the drop strikes three times, 6 and a dose each, and the rime it rises from chills all within 3 m.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheDropsQuicken, Name = "The Drops Quicken", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { BitterFrost, SixLevelsDown }, Icon = UpgradeIcons.Claw,
                Description = "A rimed enemy that dies leaves a drop of venom rime that rises as a small rime creature and strikes the nearest enemy three times, 6 each, a dose and a Chill, then melts.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Poison, Needs2 = AfflictionKind.Chill, Cooldown = 0.5f, Do = UpgradeAction.Helper, Count = 3, Interval = 0.6f, Radius = 2f,
                        Damage = 6f, Stagger = 6f, Affliction = AfflictionKind.Poison, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Poison, Needs2 = AfflictionKind.Chill, Cooldown = 0.5f, Do = UpgradeAction.Afflict, Radius = 3f,
                        Affliction = AfflictionKind.Chill, Stacks = 1 },
                },
            }, HeartPattern.Keystone));

            // The duo spell, bought with 5 Shards once both are carried (no god gives it): an opener on a 10 s cooldown
            // (SpellTimings). The field form lays the rime at the aim point, 4.5 m round, cast in Poison (a dose each bite); it
            // bites three times, 0.6 s apart, and each bite chills, so one that stands in it takes three of each.
            // NEEDS: rings growing outward 0.6 s apart to 1.5 m, 3 m and 4.5 m, each reaching further, the ground standing 4 s
            // after, and a heavy blow struck on it shattering every ring at once, 14, a Chill and two doses to each on it
            // (form-pieces); now one patch biting three times.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = RimeOverRime, Name = "Rime Over Rime", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Opener, Requires = new[] { BitterFrost, SixLevelsDown }, Icon = UpgradeIcons.Ring,
                Description = "A drop of venom falls on the ground where you aim, within 8 m, and rime grows from it frost over frost, out to 4.5 m. Each time it grows it gives every enemy it reaches a dose and a Chill, three times over.",
                Form = SpellForms.Field,
                Spell = new SpellSpec { Reach = 8f, Radius = 4.5f, Seconds = 1.8f, Interval = 0.6f, LingerDamage = 3f, Stacks = 1 },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 1 },
                },
            }, HeartPattern.Spell));
        }

        // The crust as the plain Venom Rime's, under the shape's own name: while both lie on it its poison does not fade and
        // each bite lays a Chill. Then the shape's own.
        // frozenBite: how hard the poison bites while the enemy is frozen (Cold Damage's twice); 1 as written.
        private static UpgradeEffect[] Crust(string id, UpgradeEffect[] more, float frozenBite = 1f)
        {
            var effects = new List<UpgradeEffect>
            {
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = id, Seconds = 30f, OnBite = true, While = AfflictionKind.Poison, While2 = AfflictionKind.Chill, Keeps = AfflictionKind.Poison,
                        BitesHarder = AfflictionKind.Poison, BiteScale = frozenBite, BitesHarderFrozen = true } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = id, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 1 },
            };
            effects.AddRange(more);
            return effects.ToArray();
        }
    }
}

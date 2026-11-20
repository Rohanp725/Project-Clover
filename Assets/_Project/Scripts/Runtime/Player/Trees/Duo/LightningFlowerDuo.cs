using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Lightning and Life (pairs_d4.md, "Lightning and Life", distance 4; task 4.17), named by its
    /// reaction, Lightning Flower. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried.
    /// Its Shape mark, Field or Oak, opens The Rice Wakes and Struck Oak; Storm Season and The Favoured Oak (majors), the
    /// keystone Rain on the Field, and the duo spell Sowing Thunder, bought with Shards once both are carried. Lightning is
    /// written first, as the ring names the pair.
    /// </summary>
    public static class LightningFlowerDuo
    {
        public const string Shape = "lightningflower.shape";
        public const string StormSeason = "lightningflower.stormseason";
        public const string TheFavouredOak = "lightningflower.favouredoak";
        public const string RainOnTheField = "lightningflower.rain";
        public const string SowingThunder = "lightningflower.sowing";
        /// <summary>The oak leaf The Favoured Oak hangs on the attacker, and the seeds Sowing Thunder leaves in what it strikes.</summary>
        public const string Leaf = "lightningflower.leaf";
        public const string Sown = "lightningflower.sown";
        private const string Reaction = "react.lightningflower";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Lightning, B = Elements.Life;

            // NEEDS: ground that mends the player standing on it (mending-ground): The Rice Wakes' shoots mending 2 a second;
            // now the field only charges what crosses it. And grown things as sources (grown-sources), as the plain reaction.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Lightning Flower: The Rice Wakes or Struck Oak", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Lightning Flower's two shapes, chosen at a rest fire. The Rice Wakes: the fern throws its bolt and a field of shoots springs round it. Struck Oak: no fern; the seed in the body bursts like a struck tree. Either runs instead of the plain Lightning Flower.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "The Rice Wakes",
                        Description = "The fern flowers and throws its bolt as before, and the ground 3 m round it sprouts green shoots for 6 s, a grown thing of yours. An enemy that stands in them takes a Static each second.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Arc, LeapsTo = AfflictionKind.Seeded, Count = 1, Reach = 8f, Damage = 8f, Stagger = 10f,
                                Affliction = AfflictionKind.Static, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Radius = 3f, Seconds = 6f, Interval = 1f, Grown = true, Limit = 2,
                                Affliction = AfflictionKind.Static, Stacks = 1 },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Struck Oak",
                        Description = "No fern. The current boils the seed inside the body and it bursts like a struck oak, bark and splinters thrown 3 m: 16, and 5 more for each Static and each Seed it carried. Its Static and its Seeds are spent.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Static },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Affliction = AfflictionKind.Static, Radius = 3f, Damage = 16f, DamagePerSpent = 5f, Stagger = 20f },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Seeded },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Affliction = AfflictionKind.Seeded, Radius = 3f, DamagePerSpent = 5f, Unseen = true },
                        },
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = StormSeason, Name = "Storm Season", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Leaf,
                Description = "Your heavy blows lay Static and a Seed together.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            // NEEDS: something hung on the blade for the next blow of a kind on any enemy (blade-carries): the leaf rides your
            // blade to your next heavy blow wherever it lands; now it hangs on the attacker guarded, and your next heavy blow on
            // that one spends all its Static in a discharge of twice the strength, 14 a stack in 4 m.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheFavouredOak, Name = "The Favoured Oak", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard against a Seeded enemy hangs a scorched oak leaf on your blade. Your next heavy blow discharges at twice the strength.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Seeded, Cooldown = 1f, Do = UpgradeAction.Condition,
                        State = new ConditionSpec { Id = Leaf, Seconds = 8f, EndsWith = ConditionBlow.Heavy } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Leaf, By = StruckBy.Heavy, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Static },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Leaf, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Affliction = AfflictionKind.Static, Radius = 4f, Damage = 14f, Stagger = 10f, PerStack = true },
                },
            }, HeartPattern.RightLow));

            // NEEDS: grown things as sources (grown-sources): 3 health for every bolt a grown thing throws into the flower, and
            // each that throws one standing 3 s longer; now the fern's own bolt is the one bolt, so each Lightning Flower heals 3.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = RainOnTheField, Name = "Rain on the Field", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { StormSeason, TheFavouredOak }, Icon = UpgradeIcons.Drop,
                Description = "Each Lightning Flower mends you 3 for every bolt thrown into it, and every grown thing that throws one stands 3 s longer.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, Do = UpgradeAction.Heal, Amount = 3f } },
            }, HeartPattern.Keystone));

            // The duo spell, bought with 5 Shards once both are carried (no god gives it): a detonator on a 10 s cooldown
            // (SpellTimings). The wave form flings the seeds, cast in Lightning (Static); each lays a Seed, so each enemy struck
            // flowers, and holds the seed for 6 s: your next heavy blow on it calls lightning down on it, 12 and 20 stagger in
            // 1.5 m.
            // NEEDS: seven seeds flung in a fan, the ones that miss lying in the floor, and one heavy blow or discharge anywhere
            // calling lightning onto every seed at once (form-pieces; now each seeded enemy answers your heavy blow on it); a
            // finisher striking every bolt twice (finisher-only).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = SowingThunder, Name = "Sowing Thunder", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Detonator, Requires = new[] { StormSeason, TheFavouredOak }, Icon = UpgradeIcons.Bolt,
                Description = "You fling seven seeds in a fan 6 m ahead, like a farmer sowing; each that strikes an enemy Seeds it. For 6 s your heavy blow on a seeded one calls lightning down onto its seed, 12 and 20 stagger round it.",
                Form = SpellForms.Wave,
                Spell = new SpellSpec { Reach = 6f, Arc = 70f, Damage = 4f, Stagger = 6f, Stacks = 1 },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Sown, Seconds = 6f, EndsWith = ConditionBlow.Heavy } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Sown, By = StruckBy.Heavy, Do = UpgradeAction.Falling, Delay = 0.1f, Radius = 1.5f, Damage = 12f, Stagger = 20f,
                        Affliction = AfflictionKind.Static, Stacks = 1 },
                },
            }, HeartPattern.Spell));
        }
    }
}

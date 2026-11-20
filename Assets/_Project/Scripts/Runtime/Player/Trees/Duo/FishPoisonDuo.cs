using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Poison and Water (pairs_d5.md, "Poison and Water", distance 5; task 4.17), named by its reaction,
    /// Fish Poison. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark,
    /// Bad Water, opens Rock Pool and The Ebb Takes It; The Tide Brings It (a major), Wide Slick (a minor), the keystone Fast
    /// Water, and the duo spell Poison the Lagoon. Poison is written first, as the ring names the pair.
    /// </summary>
    public static class FishPoisonDuo
    {
        public const string Shape = "fishpoison.shape";
        public const string TheTideBringsIt = "fishpoison.tide";
        public const string WideSlick = "fishpoison.wideslick";
        public const string FastWater = "fishpoison.fastwater";
        public const string PoisonTheLagoon = "fishpoison.lagoon";
        /// <summary>The conditions the two shapes set, each the stunned float as the plain reaction's.</summary>
        public const string RockPool = "fishpoison.rockpool";
        public const string Ebb = "fishpoison.ebb";
        private const string Reaction = "react.fishpoison";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Poison, B = Elements.Water;

            // NEEDS: ground-states (any enemy in the slick, or along The Ebb Takes It's stream, that comes to the doses floating
            // up too; now only the reacting enemy is tested as it reacts).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Fish Poison: Rock Pool or The Ebb Takes It", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Fish Poison's two shapes, chosen at a rest fire. Rock Pool: a small slick that floats one enemy at only two doses. The Ebb Takes It: the slick runs away from you as a stream, floating what it reaches. Either runs instead of the plain Fish Poison.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Rock Pool",
                        Description = "The fouled slick is only 1.5 m across, and it stuns at two doses: dosed two deep, it floats up stunned, held 1.5 s, and your next weapon blow on it is critical and spends two doses for 6 each. One enemy, often.",
                        Effects = Float(RockPool, 2, new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Radius = 0.75f, Seconds = 5f, Interval = 0.5f,
                            Affliction = AfflictionKind.Soaked, Stacks = 1 }),
                    },
                    new ShapeDef
                    {
                        Name = "The Ebb Takes It",
                        Description = "The slick runs 6 m away from you as a stream 1.5 m wide over 1 s, soaking all along it, and stuns by the same rule along its length: what carries four doses floats up for your blade.",
                        Effects = Float(Ebb, 4, new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Shape = HazardShape.Downwind, Count = 4, Reach = 3.5f, Amount = 1.5f,
                            Radius = 0.75f, Seconds = 5f, Interval = 0.5f, Affliction = AfflictionKind.Soaked, Stacks = 1 }),
                    },
                },
            }, HeartPattern.Foot));

            // NEEDS: Riptide's wave carrying a slick it crosses (ground-states); now any spell of yours striking an enemy both
            // Soaked and poisoned doses it two more, once in half a second.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheTideBringsIt, Name = "The Tide Brings It", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Wave,
                Description = "Riptide's wave, crossing a fouled slick, carries it: everything the wave carries takes two doses.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, Needs = AfflictionKind.Soaked, Needs2 = AfflictionKind.Poison, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Poison, Stacks = 2, Cooldown = 0.5f },
                },
            }, HeartPattern.LeftLow));

            // NEEDS: a mark that changes the reaction's own numbers (reaction-numbers); until then it does nothing.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = WideSlick, Name = "Wide Slick", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { Shape }, Icon = UpgradeIcons.Hourglass,
                Description = "Fish Poison's slick is 4 m across and lasts 2 s longer.",
            }, HeartPattern.RightLow));

            // A second bite half a second after each bite on a Soaked enemy. The bite it makes is itself a bite, so the effect's
            // cooldown is what keeps it from feeding itself.
            // NEEDS: one enemy's poison beat quickened while it is Soaked, and its doses fading twice as fast (quickened-beat);
            // now each bite on a Soaked enemy bites again half a second later, once in half a second for all.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = FastWater, Name = "Fast Water", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { TheTideBringsIt, WideSlick }, Icon = UpgradeIcons.Drop,
                Description = "Poison on a Soaked enemy bites twice as often and fades twice as fast.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PoisonBite, Needs = AfflictionKind.Soaked, Do = UpgradeAction.Bite, Delay = 0.5f, Cooldown = 0.5f } },
            }, HeartPattern.Keystone));

            // The duo spell, an opener on a 12 s cooldown (SpellTimings), bought with 5 Shards once both are carried. The field
            // form spreads the slick at the aim point, dosing (the branch's first element) and soaking what stands in it; any
            // enemy in it that reaches four doses floats up as Fish Poison does, past its rest.
            // NEEDS: each weapon blow on an enemy in the slick stirring it, a dose to every other enemy in it
            // (while-spell-stands); and each enemy floated once only (now at most once a second for all).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = PoisonTheLagoon, Name = "Poison the Lagoon", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Opener, Requires = new[] { TheTideBringsIt, WideSlick }, Icon = UpgradeIcons.Drop,
                Description = "You crush the fish-poison seed on the floor ahead: a slick spreads in rings to 6 m across and stands 6 s, soaking all in it. Each weapon blow on an enemy in it stirs the water, a dose to every other in it, and any that reaches four doses floats up as Fish Poison does.",
                Form = SpellForms.Field,
                Spell = new SpellSpec { Reach = 7f, Radius = 3f, Seconds = 6f, Interval = 1f, LingerDamage = 1f, Stacks = 1 },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Needs = AfflictionKind.Poison, NeedsAtLeast = 4, Do = UpgradeAction.ForceReaction, Reaction = Reaction, Cooldown = 1f },
                },
            }, HeartPattern.Spell));
        }

        // The slick (the shape's own), then the stunned float as the plain Fish Poison's under the shape's own name: dosed at
        // least so deep it is lifted and held 1.5 s (an elite 0.5 s; a god takes 30 stagger instead), and the next weapon
        // blow on it is critical and spends two doses for 6 each.
        private static UpgradeEffect[] Float(string id, int doses, UpgradeEffect slick) => new[]
        {
            slick,
            new UpgradeEffect { On = UpgradeTrigger.Reacted, Needs = AfflictionKind.Poison, NeedsAtLeast = doses, Ranks = EnemyRank.Plain, Do = UpgradeAction.Lift, Seconds = 1.5f },
            // An elite hangs 0.6 of a lift's time: 0.83 s asked is the entry's 0.5 s.
            new UpgradeEffect { On = UpgradeTrigger.Reacted, Needs = AfflictionKind.Poison, NeedsAtLeast = doses, Ranks = EnemyRank.Elite, Do = UpgradeAction.Lift, Seconds = 0.83f },
            new UpgradeEffect { On = UpgradeTrigger.Reacted, Needs = AfflictionKind.Poison, NeedsAtLeast = doses, Ranks = EnemyRank.God, Do = UpgradeAction.Burst, Radius = 0.5f, Stagger = 30f,
                Affliction = AfflictionKind.Poison },
            new UpgradeEffect { On = UpgradeTrigger.Reacted, Needs = AfflictionKind.Poison, NeedsAtLeast = doses, Ranks = EnemyRank.Plain | EnemyRank.Elite, Do = UpgradeAction.Condition,
                State = new ConditionSpec { Id = id, Seconds = 1.5f, Flags = ConditionFlags.TakesCritical, EndsWith = ConditionBlow.Weapon } },
            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = id, By = StruckBy.Weapon, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Poison, Count = 2 },
            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = id, By = StruckBy.Weapon, Do = UpgradeAction.Burst, Radius = 0.5f, DamagePerSpent = 6f, Affliction = AfflictionKind.Poison },
        };
    }
}

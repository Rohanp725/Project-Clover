using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Bloom: the duo branch Water and Life share, reshaped by pairs_d1_d2.md ("Water and Life", task 4.15) and drawn in the
    /// chart's heart (<see cref="HeartPattern"/>). Its reaction is Quickening; the Shape mark opens its two shapes, Floating
    /// Flower and Rootlets. Wet Seed is cut (the Shape mark takes its place), Flowering is now the reaction without its wait,
    /// and Lotus Step, offered to Water's tree, is cut because that roster is full; all three are in
    /// <see cref="RetiredMarks"/>. Kept: Drowned Roots, Morning Dew, Strangling Vine (the duo spell, re-aimed at the flower)
    /// and Great Lotus, begun by a flask in the surge, the branch's duo surge and top mark. Open while both are carried.
    /// </summary>
    public static class BloomTree
    {
        public const string Shape = "bloom.shape";
        public const string DrownedRoots = "bloom.roots";
        public const string MorningDew = "bloom.dew";
        public const string StranglingVine = "bloom.vine";
        public const string Gift = "bloom.great";
        /// <summary>The condition Rootlets keeps from the plain reaction: the flower a heavy blow bursts.</summary>
        public const string Rooted = "quickening.rootlets";

        // Laid as stacks, more than any affliction holds: "to full depth", whatever the depth is.
        private const int ToFull = 99;

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Water, B = Elements.Life;

            // NEEDS: ground that mends the player who stands on it (Floating Flower's lily, 3 a second; now 8 at once as it
            // opens, and the lily soaks what stands on it); a laying that reaches only the Soaked (Rootlets; now every enemy
            // within 5 m is seeded to full).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Quickening: Floating Flower or Rootlets", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Quickening's two shapes, chosen at a rest fire. Floating Flower: the flower drops to the ground as a lily that mends you. Rootlets: as it opens, rootlets run through the wet and seed every enemy near to full. Either runs instead of the plain Quickening.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Floating Flower",
                        Description = "The seed flowers to full and the flower drops to the ground under it as a lily 2 m across for 5 s, mending you as it opens and soaking whatever stands on it.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Seeded, Stacks = ToFull },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 1f, Seconds = 5f, Interval = 1f, Damage = 0f, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Heal, Amount = 8f },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Rootlets",
                        Description = "As the flower opens, white rootlets run through the wet to the enemies within 5 m and seed each to full. A heavy blow within 4 s still bursts the flower: you heal 10.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Seeded, Stacks = ToFull },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Afflict, Radius = 5f, Affliction = AfflictionKind.Seeded, Stacks = ToFull },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Rooted, Seconds = 4f, EndsWith = ConditionBlow.Heavy } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Rooted, By = StruckBy.Heavy, Do = UpgradeAction.Heal, Amount = 10f },
                        },
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = DrownedRoots, Name = "Drowned Roots", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard against an enemy both Soaked and Seeded binds it in roots for a moment.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Soaked, Needs2 = AfflictionKind.Seeded, Do = UpgradeAction.Hold, Seconds = 1.2f } },
            }, HeartPattern.LeftLow));

            // BESPOKE (in the health bar): mending past full is kept as Dew, which takes wounds first, and blows throw a
            // blade of water while any is held. Until then: a flask gives a ward, and blows throw the blade now and then.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = MorningDew, Name = "Morning Dew", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Plus,
                Description = "Drinking a flask gives you a ward that turns aside one blow, up to three held. Your blows throw a blade of water that soaks what it passes, once every moment or so.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.FlaskDrunk, Do = UpgradeAction.Ward, Count = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Projectile, Flight = Flight.Straight, Damage = 4f, Radius = 0.8f, Speed = 14f, Seconds = 0.25f, Affliction = AfflictionKind.Soaked, Stacks = 1, Cooldown = 0.8f },
                },
            }, HeartPattern.RightLow));

            // The duo spell, re-aimed at the flower (pairs_d1_d2.md): the vine drinks the water and Quickening opens on it at
            // once, past the rest; then the water is spent. The pillars form lifts what it strikes for a moment.
            // NEEDS: a form aimed at the nearest Soaked enemy (now the nearest enemy), and the flower burst on a lifted enemy
            // for twice the heal.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = StranglingVine, Name = "Strangling Vine", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Breaker, Requires = new[] { DrownedRoots, MorningDew }, Icon = UpgradeIcons.Tower,
                Description = "A great vine bursts up under the nearest enemy and throws it into the air for a moment. If it was soaked and seeded the vine drinks the water and the seed flowers at once, whatever its rest. 45 mana.",
                Form = "pillars",
                Spell = new SpellSpec { Cost = 45f, Count = 1, Reach = 5f, Radius = 3f, Damage = 20f, Stagger = 50f, Stacks = 1, Delay = 0.5f },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Needs = AfflictionKind.Soaked, Do = UpgradeAction.ForceReaction, Reaction = "react.quickening" },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Needs = AfflictionKind.Soaked, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Soaked },
                },
            }, HeartPattern.Spell));

            // BESPOKE (built): LotusForm. The branch's duo surge, begun by a flask drunk in the surge (pairs_d1_d2.md), and its top mark.
            // NEEDS: a duo surge chosen in the Hand's surge slot (now it answers any surge while both are carried).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Gift, Name = "Great Lotus", Element = A, Element2 = B, Kind = UpgradeKind.Signature, Icon = UpgradeIcons.Crown,
                Description = "A flask drunk while your surge lasts opens a great lotus under you. Inside it you mend and enemies are Soaked and Seeded. Each perfect guard inside closes a petal on the attacker and holds it, up to five. When the last petal closes, or its ten seconds are done, the lotus bursts.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.FlaskDrunk, Do = UpgradeAction.Form, Form = SpellForms.Lotus, WhileSurge = true, Affliction = AfflictionKind.Seeded, Spell = new SpellSpec { Cost = 0f, Radius = 3f, Seconds = 10f, Damage = 30f, Stagger = 40f, Count = 5, LingerDamage = 2f, Stacks = 1 } } },
            }, HeartPattern.Crown));
        }
    }
}

using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Bloom: the branch Water and Life share. Watered seeds flower: mending is turned outward, and the ground the
    /// player stands on becomes a place enemies should not have followed them into. Its crown is given by consuming
    /// the neighbour's god.
    /// </summary>
    public static class BloomTree
    {
        public const string WetSeed = "bloom.wetseed";
        public const string Flowering = "bloom.flower";
        public const string DrownedRoots = "bloom.roots";
        public const string MorningDew = "bloom.dew";
        public const string LotusStep = "bloom.lotus";
        public const string StranglingVine = "bloom.vine";
        public const string Gift = "bloom.great";

        public static void Add(List<Upgrade> to)
        {
            // The first element is the one whose mix this is: the earlier of the two round the ring.
            const string A = Elements.Water, B = Elements.Life;

            // Both afflictions on every blow, the second element's too (the owner's of 2026-11-19); with the first element's root learned, the root's stack is that element's share and is not laid twice.
            to.Add(new Upgrade { Id = WetSeed, Name = "Wet Seed", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, X = 0f, Y = 1f, Icon = UpgradeIcons.Drop,
                Description = "Your blows lay Soaked and Seeded together.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                } });
            // NEAREST: no flower is shown and only one affliction can be asked for, so a heavy blow on a Seeded enemy
            // bursts it, with seven seconds between (the four the flower takes and the three it stands).
            to.Add(new Upgrade { Id = Flowering, Name = "Flowering", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { WetSeed }, X = -0.5f, Y = 2.36f, Icon = UpgradeIcons.Leaf,
                Description = "A heavy blow on a Seeded enemy bursts its seed: you are mended and everyone near is Seeded and Soaked. It can happen once every seven seconds.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Heal, Needs = AfflictionKind.Seeded, Amount = 12f, Cooldown = 7f },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Burst, Needs = AfflictionKind.Seeded, Damage = 12f, Radius = 3.5f, Affliction = AfflictionKind.Seeded, Stacks = 1, Cooldown = 7f },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Needs = AfflictionKind.Seeded, Radius = 3.5f, Affliction = AfflictionKind.Soaked, Stacks = 1, Cooldown = 7f },
                } });
            to.Add(new Upgrade { Id = DrownedRoots, Name = "Drowned Roots", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { WetSeed }, X = 0.5f, Y = 2.36f, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard against an enemy both Soaked and Seeded binds it in roots for a moment.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Soaked, Needs2 = AfflictionKind.Seeded, Do = UpgradeAction.Hold, Seconds = 1.2f } } });
            // BESPOKE (in the health bar): mending past full is kept as Dew, which takes wounds first, and blows throw a
            // blade of water while any is held. Until then: a flask gives a ward, and blows throw the blade now and then.
            to.Add(new Upgrade { Id = MorningDew, Name = "Morning Dew", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { Flowering }, X = 0f, Y = 3.48f, Icon = UpgradeIcons.Plus,
                Description = "Drinking a flask gives you a ward that turns aside one blow, up to three held. Your blows throw a blade of water that soaks what it passes, once every moment or so.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.FlaskDrunk, Do = UpgradeAction.Ward, Count = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Projectile, Flight = Flight.Straight, Damage = 4f, Radius = 0.8f, Speed = 14f, Seconds = 0.25f, Affliction = AfflictionKind.Soaked, Stacks = 1, Cooldown = 0.8f },
                } });
            // NEEDS FORM: for a while each dodge leaves a lotus that mends the player who steps on it and cuts an enemy who does. The field is the nearest: one patch that cuts.
            to.Add(new Upgrade { Id = LotusStep, Name = "Lotus Step", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { DrownedRoots }, X = 0.5f, Y = 4.6f, Icon = UpgradeIcons.Boot,
                Description = "Hold the spell button to open a lotus on the ground just ahead. For twelve seconds it cuts every enemy standing on it, once a second. 30 mana.",
                Form = "field",
                Spell = new SpellSpec { Cost = 30f, Reach = 2f, Radius = 1.5f, Seconds = 12f, Interval = 1f, LingerDamage = 10f, Stacks = 1 } });
            // The pillars form lifts what it strikes for a moment; the three-second hold and the tripled mending are not in it yet.
            to.Add(new Upgrade { Id = StranglingVine, Name = "Strangling Vine", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { Flowering }, X = -0.5f, Y = 4.6f, Icon = UpgradeIcons.Tower,
                Description = "Hold the spell button to raise a great vine under the nearest enemy. It strikes hard and throws the enemy into the air for a moment. 45 mana.",
                Form = "pillars",
                Spell = new SpellSpec { Cost = 45f, Count = 1, Reach = 5f, Radius = 3f, Damage = 20f, Stagger = 50f, Stacks = 1, Delay = 0.5f } });

            // BESPOKE (built): LotusForm.
            to.Add(new Upgrade { Id = Gift, Name = "Great Lotus", Element = A, Element2 = B, Kind = UpgradeKind.Signature, X = 0f, Y = 5.8f, Icon = UpgradeIcons.Crown,
                Description = "A flask drunk while your surge lasts opens a great lotus under you. Inside it you mend and enemies are Soaked and Seeded. Each perfect guard inside closes a petal on the attacker and holds it, up to five. When the last petal closes, or its ten seconds are done, the lotus bursts.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.FlaskDrunk, Do = UpgradeAction.Form, Form = SpellForms.Lotus, WhileSurge = true, Affliction = AfflictionKind.Seeded, Spell = new SpellSpec { Cost = 0f, Radius = 3f, Seconds = 10f, Damage = 30f, Stagger = 40f, Count = 5, LingerDamage = 2f, Stacks = 1 } } } });
        }
    }
}

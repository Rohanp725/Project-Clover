using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Fire and Life (pairs_d5.md, "Fire and Life", distance 5; task 4.17), named by its reaction,
    /// Firebloom. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark,
    /// Seeds That Wait, opens Between Two Fires and Ash Bed; Kindled Guard (a major), Dry Pods (a minor), the keystone The
    /// Burning Tree, and the duo spell Need-Fire, bought with Shards once both are carried. Fire is written first, as the
    /// ring names the pair.
    /// </summary>
    public static class FirebloomDuo
    {
        public const string Shape = "firebloom.shape";
        public const string KindledGuard = "firebloom.kindledguard";
        public const string DryPods = "firebloom.drypods";
        public const string TheBurningTree = "firebloom.burningtree";
        public const string NeedFire = "firebloom.needfire";
        /// <summary>The condition Ash Bed sets: the burn and the seed one thing while it is Seeded.</summary>
        public const string AshBed = "firebloom.ashbed";
        private const string Reaction = "react.firebloom";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Fire, B = Elements.Life;

            // Between Two Fires: the gap between each pair is a pickup at the enemy's spot (World.Pickups), passing through it heals
            // 8 and charges the blade with two blows of Burn. NEEDS: mending-ground (Ash Bed's shoots mending 2 a second; now they only
            // seed what steps in).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Firebloom: Between Two Fires or Ash Bed", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Firebloom's two shapes, chosen at a rest fire. Between Two Fires: the lilies grow in pairs across the enemy, and passing between them mends you and charges your blade. Ash Bed: no lilies; its Burn holds while it is Seeded, and its death sows a bed of shoots. Either runs instead of the plain Firebloom.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Between Two Fires",
                        Description = "The Seeded is spent and the lilies grow in pairs, 3 m apart, facing each other across the enemy: a pair for each seed. Passing between the two of a pair heals you 8 and charges your blade with two blows of Burn.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Seeded },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Shape = HazardShape.Ring, Count = 2, PerStack = true, Reach = 1.5f, Radius = 1f, Seconds = 5f,
                                Interval = 0.5f, Damage = 3f, Grown = true, Affliction = AfflictionKind.Burn, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.DropPickup, Drops = Clover.World.PickupKind.LilyFire, RaiseAt = Clover.World.StandingPlace.At, Count = 1, PerStack = true, Radius = 1.2f, Seconds = 5f },
                            new UpgradeEffect { On = UpgradeTrigger.PickedUp, Do = UpgradeAction.Heal, Amount = 8f },
                            new UpgradeEffect { On = UpgradeTrigger.PickedUp, Do = UpgradeAction.CoatBlade, Affliction = AfflictionKind.Burn, Stacks = 1, Count = 2 },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Ash Bed",
                        Description = "No lilies, and the seed is not spent: the burn and the seed are one thing, and while it is Seeded its Burn does not fade. When it dies the ash is sown, a bed of green shoots 3 m across for 6 s that mends you and Seeds every enemy that steps in.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = AshBed, Seconds = 30f, While = AfflictionKind.Seeded, Keeps = AfflictionKind.Burn } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionDied, Condition = AshBed, Do = UpgradeAction.Hazard, Radius = 1.5f, Seconds = 6f, Interval = 1f, Grown = true,
                                Affliction = AfflictionKind.Seeded, Stacks = 1 },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // The lily (whatever you grew) nearest you within 3 m flares where it stands and stands 3 s longer (UpgradeAction.Tend).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = KindledGuard, Name = "Kindled Guard", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard made within 3 m of a lily makes it flare: 12 damage and two Burn in 2 m, and it stands 3 s longer.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, At = EffectAt.Self, Do = UpgradeAction.Tend, Radius = 3f, Seconds = 3f, Reach = 2f, Damage = 12f, Stagger = 10f,
                        Affliction = AfflictionKind.Burn, Stacks = 2, Cooldown = 1f },
                },
            }, HeartPattern.LeftLow));

            // Changes its reaction's own numbers while it is in force (reaction-numbers, ReactionNumbers):
            // one more seed (four at most) and lilies standing 7 s, in the plain reaction and Between Two Fires.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = DryPods, Name = "Dry Pods", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { Shape }, Icon = UpgradeIcons.Hourglass,
                Description = "One more seed is thrown, four at most, and lilies stand 2 s longer.",
                ReactionChanges = new[]
                {
                    new ReactionChange { Shape = -1, Index = 1, PerMore = 1, PerMost = 4, Seconds = 7f },
                    new ReactionChange { Shape = 0, Index = 1, PerMore = 1, PerMost = 4, Seconds = 7f },
                },
            }, HeartPattern.RightLow));

            // The burning tree: a helper that strikes the enemy that bloomed every 2 s for 10 s, a Burn each time.
            // NEEDS: standing-things (a tree that stands 10 s and throws a burning seed, Seeded and Burn, at the nearest
            // enemy every 2 s; and only the first lily of each fight, once-per-fight); now the helper strikes the enemy that
            // bloomed, Burn only, at most once in 30 s.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheBurningTree, Name = "The Burning Tree", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { KindledGuard, DryPods }, Icon = UpgradeIcons.Tower,
                Description = "The first lily of each fight grows into a burning tree that stands 10 s and throws a burning seed at the nearest enemy every 2 s, Seeded and Burn.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, Do = UpgradeAction.Helper, Count = 5, Interval = 2f, Radius = 6f, Damage = 4f, Stagger = 4f,
                        Affliction = AfflictionKind.Burn, Stacks = 1, Cooldown = 30f },
                },
            }, HeartPattern.Keystone));

            // The duo spell, an opener on a 12 s cooldown (SpellTimings), bought with 5 Shards once both are carried. The field
            // form lays the fire on the enemy faced, burning (the branch's first element) and seeding each second, so the
            // pair blooms on what stands in it.
            // NEEDS: two fires 4 m apart on either side of the enemy, joined by a line of sparks (form-pieces; now one patch 5 m
            // across on it); each weapon blow heals 2 and takes twice its time off the clocks while you stand between them
            // (while-spell-stands); a shove across the line setting its Burn to full (shove-moment).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = NeedFire, Name = "Need-Fire", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Opener, Requires = new[] { KindledGuard, DryPods }, Icon = UpgradeIcons.Flame,
                Description = "Two fires of green branches spring up on either side of the enemy you face and burn 6 s, joined by a line of sparks. Every enemy between them takes a Burn and a Seed each second.",
                Form = SpellForms.Field,
                Spell = new SpellSpec { Reach = 7f, Radius = 2.5f, Seconds = 6f, Interval = 1f, LingerDamage = 2f, Stacks = 1 },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Seeded, Stacks = 1 } },
            }, HeartPattern.Spell));
        }
    }
}

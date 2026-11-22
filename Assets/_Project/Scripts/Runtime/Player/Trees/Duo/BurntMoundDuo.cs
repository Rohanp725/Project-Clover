using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Earth and Fire (pairs_d5.md, "Earth and Fire", distance 5; task 4.17), named by its reaction,
    /// Burnt Mound. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark,
    /// Fired Hard, opens Heaped Stones and Fire-Cracked; Timber-Laced (a major), Hot Stones (a minor), the keystone Brick and
    /// Fire, and the duo spell Vitrified Wall. Earth is written first, as the ring names the pair.
    /// </summary>
    public static class BurntMoundDuo
    {
        public const string Shape = "burntmound.shape";
        public const string TimberLaced = "burntmound.timberlaced";
        public const string HotStones = "burntmound.hotstones";
        public const string BrickAndFire = "burntmound.brickfire";
        public const string VitrifiedWall = "burntmound.wall";
        /// <summary>The condition Heaped Stones sets: the boulder waiting for a heavy blow to roll it.</summary>
        public const string Boulder = "burntmound.boulder";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Earth, B = Elements.Fire;

            // NEEDS: standing-things (Heaped Stones' boulder lying where it was thrown up, rolled 8 m through a line by a heavy
            // blow beside it; now a heavy blow on the enemy sends a lane of it 8 m on from the enemy, away from you).
            // Fire-Cracked's stones each crack once as an enemy steps within 1 m, 12 and a Burn in 2 m (UpgradeEffect.CracksAt).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Burnt Mound: Heaped Stones or Fire-Cracked", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Burnt Mound's two shapes, chosen at a rest fire. Heaped Stones: the three stones fuse into one boulder that your heavy blow rolls through a line. Fire-Cracked: no batting; each stone cracks by itself as an enemy steps near. Either runs instead of the plain Burnt Mound.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Heaped Stones",
                        Description = "The three stones fuse into one red-hot boulder beside it for 6 s, burning what touches it. Your heavy blow rolls it 8 m through a line: 20, two Burn and two Tremor to each it crosses.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Shape = HazardShape.Ring, Count = 1, Reach = 1.5f, Radius = 0.8f, Seconds = 6f, Interval = 0.5f,
                                Damage = 3f, Affliction = AfflictionKind.Burn, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Boulder, Seconds = 6f, EndsWith = ConditionBlow.Heavy } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Boulder, By = StruckBy.Heavy, Do = UpgradeAction.Lane, Reach = 8f, Radius = 0.6f, Damage = 20f, Stagger = 20f,
                                Affliction = AfflictionKind.Burn, Stacks = 2 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Boulder, By = StruckBy.Heavy, Do = UpgradeAction.Lane, Reach = 8f, Radius = 0.6f,
                                Affliction = AfflictionKind.Tremor, Stacks = 2 },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Fire-Cracked",
                        Description = "No batting. The three stones lie 6 s, and each cracks by itself when an enemy steps within 1 m of it: 12 and a Burn in 2 m. A field to drive them across.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Shape = HazardShape.Ring, Count = 3, Reach = 1.5f, Radius = 1f, Seconds = 6f, Interval = 0.2f,
                                Damage = 12f, Affliction = AfflictionKind.Burn, Stacks = 1, CracksAt = 2f },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // NEEDS: the player's raised rock as a thing that catches fire (standing-things): it burns what touches it and
            // bursts in glass sherds when it ends; now any spell of yours striking a burning enemy lays burning ground under it
            // for 3 s and a burst of sherds after, once a second.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TimberLaced, Name = "Timber-Laced", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Rock,
                Description = "Rock you raise catches fire from a burning enemy within 2 m: it burns what touches it, 4 and a Burn every half second, and when it ends it bursts in glass sherds, 12 in 2.5 m.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, Needs = AfflictionKind.Burn, Do = UpgradeAction.Hazard, Radius = 1f, Seconds = 3f, Interval = 0.5f, Damage = 4f,
                        Affliction = AfflictionKind.Burn, Stacks = 1, Cooldown = 1f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, Needs = AfflictionKind.Burn, Do = UpgradeAction.Burst, Radius = 2.5f, Delay = 3f, Damage = 12f, Stagger = 10f,
                        Affliction = AfflictionKind.Tremor, Cooldown = 1f },
                },
            }, HeartPattern.LeftLow));

            // Changes its reaction's own numbers while it is in force (reaction-numbers, ReactionNumbers):
            // the stones lying 8 s, not 6, and Heaped Stones' boulder rolling 10 m, not 8.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = HotStones, Name = "Hot Stones", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { Shape }, Icon = UpgradeIcons.Hourglass,
                Description = "Burnt Mound's stones lie 2 s longer and skip 2 m further.",
                ReactionChanges = new[]
                {
                    new ReactionChange { Shape = -1, Index = 0, Seconds = 8f },
                    new ReactionChange { Shape = 0, Index = 0, Seconds = 8f },
                    new ReactionChange { Shape = 0, Index = 1, ConditionSeconds = 8f },
                    new ReactionChange { Shape = 0, Index = 2, Reach = 10f },
                    new ReactionChange { Shape = 0, Index = 3, Reach = 10f },
                    new ReactionChange { Shape = 1, Index = 0, Seconds = 8f },
                },
            }, HeartPattern.RightLow));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = BrickAndFire, Name = "Brick and Fire", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { TimberLaced, HotStones }, Icon = UpgradeIcons.Hammer,
                Description = "Heavy blows that land on a burning enemy lay Tremor; light blows that land on a shaken enemy lay Burn. The weave of light and heavy feeds both.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Burn, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Tremor, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.LightHit, Needs = AfflictionKind.Tremor, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1 },
                },
            }, HeartPattern.Keystone));

            // The duo spell, a breaker on a 12 s cooldown (SpellTimings), bought with 5 Shards once both are carried. The wall
            // form raises the rampart 4 m ahead after 0.4 s, 6 m wide, and it blocks for 5 s; its end fuses it and bursts it
            // outward, shaking (the branch's first element) and burning two deep.
            // NEEDS: what stands where it rises thrown aside (12 and a Tremor), the wall burning what touches it, and a heavy
            // blow on it fusing it early (form-pieces).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = VitrifiedWall, Name = "Vitrified Wall", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Breaker, Requires = new[] { TimberLaced, HotStones }, Icon = UpgradeIcons.Wall,
                Description = "A rampart of stone laced with burning timber rises 4 m ahead, 6 m wide, and blocks for 5 s. A heavy blow on it, or its end, fuses it: the stone runs to black glass and bursts outward, 30 and 60 stagger, two Burn and two Tremor within 3 m.",
                Form = SpellForms.Wall,
                Spell = new SpellSpec { Reach = 4f, Arc = 6f, Delay = 0.4f, Seconds = 5f, Radius = 3f, Damage = 30f, Stagger = 60f, Stacks = 2 },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 2 } },
            }, HeartPattern.Spell));
        }
    }
}

using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Blizzard: the duo branch Wind and Ice share, reshaped by pairs_d1_d2.md ("Wind and Ice", task 4.15) and drawn in the
    /// chart's heart (<see cref="HeartPattern"/>). Its reaction is Rime; the Shape mark opens its two shapes, Rime Tree and
    /// Spindrift. Biting Wind is cut (the Shape mark takes its place; <see cref="RetiredMarks"/>) and Whiteout, offered to
    /// Ice's or Wind's tree, whose rosters are full, is kept hidden for the Evolutions (<see cref="SpareSpells"/>). Kept: Windchill, Cold Front, Sliding Ice,
    /// Avalanche (the duo spell) and Diamond Dust, begun by a shatter, the branch's top mark. Open while both are carried.
    /// </summary>
    public static class BlizzardTree
    {
        public const string Shape = "blizzard.shape";
        public const string Windchill = "blizzard.windchill";
        public const string ColdFront = "blizzard.front";
        public const string SlidingIce = "blizzard.sliding";
        public const string Avalanche = "blizzard.avalanche";
        public const string Gift = "blizzard.dust";
        /// <summary>The condition Rime Tree sets: the crust that never breaks and gathers cold with each blow.</summary>
        public const string RimeTree = "rime.tree";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Wind, B = Elements.Ice;

            // NEEDS: a freeze held longer and a shatter struck harder by a condition (Rime Tree: frozen 1 s longer, its shatter
            // 25% harder; now only the Chill each blow adds); the side of a blow, as for the plain Rime.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Rime: Rime Tree or Spindrift", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Rime's two shapes, chosen at a rest fire. Rime Tree: the crust never breaks, and every blow on it adds Chill. Spindrift: the wind tears the crust away at once, and the ice blows out behind it with no heavy blow needed. Either runs instead of the plain Rime.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Rime Tree",
                        Description = "The crust never breaks: for 5 s every blow on it, weapon or spell, adds 1 Chill, so it freezes standing in its rime.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = RimeTree, Seconds = 5f } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = RimeTree, By = StruckBy.Any, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 1 },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Spindrift",
                        Description = "The wind tears the crust away at once by itself: the ice blows out of its far side the moment Rime fires, at half damage, 5, 2 Chill and 1 Unsteady to all behind it.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Shape = HazardShape.Downwind, Count = 2, Reach = 2.6f, Amount = 1.6f, Radius = 1.1f, Damage = 5f, Affliction = AfflictionKind.Chill, Stacks = 2 },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Shape = HazardShape.Downwind, Count = 2, Reach = 2.6f, Amount = 1.6f, Radius = 1.1f, Affliction = AfflictionKind.Unsteady, Stacks = 1 },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // NEAREST: only the enemy faced grows colder, once every two seconds.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Windchill, Name = "Windchill", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Snowflake,
                Description = "The enemy you face, if it is both Unsteady and chilled, grows colder by itself.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.EverySeconds, Every = 2f, Needs = AfflictionKind.Unsteady, Needs2 = AfflictionKind.Chill, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 1 } },
            }, HeartPattern.LeftLow));

            // A perfect dodge names no attacker: the cone goes at the enemy faced, and the push and the Unsteady go out round the player.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = ColdFront, Name = "Cold Front", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Boot,
                Description = "A perfect dodge leaves a gust of ice behind you. It throws the attacker back, two deeper in Chill and Unsteady.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Cone, Damage = 10f, Stagger = 20f, Reach = 4f, Arc = 90f, Affliction = AfflictionKind.Chill, Stacks = 2 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Afflict, At = EffectAt.Self, Radius = 4f, Affliction = AfflictionKind.Unsteady, Stacks = 2 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Push, At = EffectAt.Self, Radius = 4f, Amount = 3f },
                },
            }, HeartPattern.RightLow));

            // BESPOKE (small): a light blow sends a Frozen enemy sliding until it meets a wall or another enemy, and it
            // shatters there. Until then: a light blow on a chilled enemy throws back those near and a burst follows.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = SlidingIce, Name = "Sliding Ice", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Windchill }, Icon = UpgradeIcons.Arrow,
                Description = "A light blow on a chilled enemy throws everything close to you back, and half a second later ice bursts round the one struck.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.LightHit, Do = UpgradeAction.Push, Needs = AfflictionKind.Chill, At = EffectAt.Self, Radius = 3.5f, Amount = 5f, Cooldown = 1.5f },
                    new UpgradeEffect { On = UpgradeTrigger.LightHit, Do = UpgradeAction.Burst, Needs = AfflictionKind.Chill, Damage = 20f, Stagger = 20f, Radius = 2.5f, Delay = 0.5f, Cooldown = 1.5f },
                },
            }, HeartPattern.LeftHigh));

            // The duo spell. The dragon form carries what it meets along its line, as the rolling snow does; it does not bury
            // them Frozen where it stops.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Avalanche, Name = "Avalanche", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Breaker, Requires = new[] { Windchill, ColdFront }, Icon = UpgradeIcons.Wave,
                Description = "You send a great head of snow down a lane nine metres long. It strikes what it passes and carries it along, then bites hard where it stops. 50 mana.",
                Form = "dragon",
                Spell = new SpellSpec { Cost = 50f, Reach = 9f, Radius = 2f, Speed = 4.5f, Damage = 25f, Stagger = 30f, Stacks = 4 },
            }, HeartPattern.Spell));

            // BESPOKE (built): DustForm. The branch's top mark, begun by a shatter.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Gift, Name = "Diamond Dust", Element = A, Element2 = B, Kind = UpgradeKind.Signature, Icon = UpgradeIcons.Crown,
                Description = "Shards from a shattered enemy do not fall. They hang round you, four from each, up to twelve, and your next dodge throws them down its path.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Shatter, Do = UpgradeAction.Form, Form = SpellForms.Dust, Affliction = AfflictionKind.Chill, Spell = new SpellSpec { Cost = 0f, Count = 4, Damage = 6f, Stagger = 4f, Reach = 9f, Stacks = 1 } } },
            }, HeartPattern.Crown));
        }
    }
}

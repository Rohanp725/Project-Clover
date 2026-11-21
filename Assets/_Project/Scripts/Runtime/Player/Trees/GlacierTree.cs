using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Glacier: the duo branch Ice and Water share, reshaped by pairs_d1_d2.md ("Ice and Water", task 4.15) and drawn in the
    /// chart's heart (<see cref="HeartPattern"/>). Its reaction is Supercooled; the Shape mark opens its two shapes, Sea Ice
    /// and Hard Water. Cold Water is cut (the Shape mark takes its place), Flash Freeze is now the reaction, both in
    /// <see cref="RetiredMarks"/>, and Creeping Ice, offered to Ice's tree, whose roster is full, is kept hidden for the Evolutions (<see cref="SpareSpells"/>). Kept:
    /// Icebreaker, Cold in the Bones, The Wave Stands Still (the duo spell) and Weight of Ice, begun by the guard, the
    /// branch's top mark. Open while both are carried.
    /// </summary>
    public static class GlacierTree
    {
        public const string Shape = "glacier.shape";
        public const string Icebreaker = "glacier.breaker";
        public const string ColdInTheBones = "glacier.bones";
        public const string TheWaveStandsStill = "glacier.wave";
        public const string Gift = "glacier.weight";
        /// <summary>The conditions the two shapes set, each in place of the plain reaction's still water.</summary>
        public const string SeaIce = "supercooled.sea";
        public const string HardWater = "supercooled.hard";

        // Laid as stacks, more than any affliction holds: "to full depth", whatever the depth is.
        private const int ToFull = 99;

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Ice, B = Elements.Water;

            // Sea Ice's web reaches only the Soaked within 4 m (UpgradeEffect.OnlyCarrying).
            // Hard Water's freeze is held half again as long and shatters 30% harder (UpgradeAction.Freeze).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Supercooled: Sea Ice or Hard Water", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Supercooled's two shapes, chosen at a rest fire. Sea Ice: when it freezes, the cold runs out through the floor and chills the wet round it. Hard Water: only a heavy blow freezes it, and harder. Either runs instead of the plain Supercooled.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Sea Ice",
                        Description = "The next weapon blow freezes it outright and spends the water, and the cold runs down into the floor and out in a web of frost: everything within 4 m takes 2 Chill, and a Soaked one reached becomes Supercooled itself.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = SeaIce, Seconds = 6f, EndsWith = ConditionBlow.Weapon } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = SeaIce, By = StruckBy.Weapon, Ranks = EnemyRank.Plain, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = ToFull },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = SeaIce, By = StruckBy.Weapon, Ranks = EnemyRank.Elite, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 3 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = SeaIce, By = StruckBy.Weapon, Ranks = EnemyRank.God, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 2 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = SeaIce, By = StruckBy.Weapon, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Soaked },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = SeaIce, By = StruckBy.Weapon, Do = UpgradeAction.Afflict, Radius = 4f, Affliction = AfflictionKind.Chill, Stacks = 2, OnlyCarrying = AfflictionKind.Soaked, Cascades = true },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Hard Water",
                        Description = "Only a heavy blow freezes it, but that freeze is the harder for it: the water stays still for 6 s, and the first heavy blow freezes it outright for half again as long and spends the water; that ice shatters 30% harder.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = HardWater, Seconds = 6f, EndsWith = ConditionBlow.Heavy } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = HardWater, By = StruckBy.Heavy, Ranks = EnemyRank.Plain, Do = UpgradeAction.Freeze, Seconds = 3.75f, Amount = 1.3f },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = HardWater, By = StruckBy.Heavy, Ranks = EnemyRank.Elite, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 3 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = HardWater, By = StruckBy.Heavy, Ranks = EnemyRank.God, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 2 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = HardWater, By = StruckBy.Heavy, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Soaked },
                        },
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Icebreaker, Name = "Icebreaker", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Hammer,
                Description = "Shattering an enemy gives mana and fills a little of an empty gourd.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Shatter, Do = UpgradeAction.Mana, Amount = 12f }, new UpgradeEffect { On = UpgradeTrigger.Shatter, Do = UpgradeAction.FlaskFill, Amount = 0.33f } },
            }, HeartPattern.LeftLow));

            // NEAREST: an enemy's wind-up cannot be changed, so a blow on a Soaked enemy lengthens the player's own moment to guard for a while.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = ColdInTheBones, Name = "Cold in the Bones", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Hourglass,
                Description = "For four seconds after you strike a Soaked enemy, the moment for a perfect guard is a quarter longer.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictedHit, Do = UpgradeAction.Boon, Needs = AfflictionKind.Soaked, Stat = PlayerStat.PerfectGuardWindow, Amount = 0.25f, Seconds = 4f, Cooldown = 0.5f } },
            }, HeartPattern.RightLow));

            // BESPOKE (built): FrozenWaveForm. The duo spell.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheWaveStandsStill, Name = "The Wave Stands Still", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Breaker, Requires = new[] { Icebreaker, ColdInTheBones }, Icon = UpgradeIcons.Wave,
                Description = "You throw a great wave that rolls on while you keep the spell's button held. Let go, and it freezes where it stands with everything in it. One heavy blow shatters the wall and all it holds. 50 mana.",
                Form = SpellForms.FrozenWave,
                Spell = new SpellSpec { Cost = 50f, Reach = 9f, Speed = 6f, Arc = 6f, LingerDamage = 10f, Seconds = 4f, Damage = 45f, Stagger = 60f, Stacks = 5 },
            }, HeartPattern.Spell));

            // BESPOKE (built): PlatesForm. The branch's top mark, begun by the guard.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Gift, Name = "Weight of Ice", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { TheWaveStandsStill }, Icon = UpgradeIcons.Crown,
                Description = "Each blow you guard lays a plate of ice on you, up to five. A perfect guard lays two. Plates turn a little of every wound. Your next heavy blow spends them all in one great edge of ice.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Guard, Do = UpgradeAction.Form, Form = SpellForms.Plates, Affliction = AfflictionKind.Chill, Spell = new SpellSpec { Cost = 0f, Count = 1, Damage = 12f, Stagger = 12f, Reach = 1.2f, Radius = 1.3f, Stacks = 1 } }, new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Form, Form = SpellForms.Plates, Affliction = AfflictionKind.Chill, Spell = new SpellSpec { Cost = 0f, Count = 2, Damage = 12f, Stagger = 12f, Reach = 1.2f, Radius = 1.3f, Stacks = 1 } } },
            }, HeartPattern.Crown));
        }
    }
}

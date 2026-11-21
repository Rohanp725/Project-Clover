using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Ice and Light (pairs_d5.md, "Ice and Light", distance 5; task 4.17), named by its reaction, Mock
    /// Suns. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark, Through
    /// the Ice, opens Prism and Lens of Ice; Bent Light (a major), Wide Fan (a minor), the keystone White Night, and the duo
    /// spell Sunstone. Ice is written first, as the ring names the pair.
    /// </summary>
    public static class MockSunsDuo
    {
        public const string Shape = "mocksuns.shape";
        public const string BentLight = "mocksuns.bentlight";
        public const string WideFan = "mocksuns.widefan";
        public const string WhiteNight = "mocksuns.whitenight";
        public const string Sunstone = "mocksuns.sunstone";
        /// <summary>The conditions the two shapes set: Prism's clear ice, waiting for the next weapon blow, and Lens of
        /// Ice's gathered light.</summary>
        public const string Prism = "mocksuns.prism";
        public const string Lens = "mocksuns.lens";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Ice, B = Elements.Light;

            // NEEDS: cone-from-enemy (Prism's fan of three rays out of its far side, 6 m and 30 degrees; now three bursts on
            // your line beyond it); freeze-moment (Lens of Ice's Mark kept only while it is Frozen and its shatter 60% harder;
            // now kept while it is chilled and Marked, and a heavy blow on it at full Chill strikes 18 more and throws its Mark).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Mock Suns: Prism or Lens of Ice", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Mock Suns' two shapes, chosen at a rest fire. Prism: no suns; the ice goes clear and your next blow's light fans out behind it as rays. Lens of Ice: the light is gathered into the body, its Mark kept while frozen and thrown out when it shatters. Either runs instead of the plain Mock Suns.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Prism",
                        Description = "No mock suns. The ice in it goes clear: your next weapon blow on it is critical and does not spend the Mark, and its light passes through the body and out the far side as a fan of three rays, 6 m long: 8 each, a Mark and a Chill on what they cross.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Prism, Seconds = 4f, Keeps = AfflictionKind.Marked, EndsWith = ConditionBlow.Weapon } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Prism, By = StruckBy.Weapon, Do = UpgradeAction.Burst, Shape = HazardShape.Downwind, Count = 3, Reach = 3f,
                                Amount = 1.5f, Radius = 0.8f, Damage = 8f, Affliction = AfflictionKind.Marked, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Prism, By = StruckBy.Weapon, Do = UpgradeAction.Burst, Shape = HazardShape.Downwind, Count = 3, Reach = 3f,
                                Amount = 1.5f, Radius = 0.8f, Affliction = AfflictionKind.Chill, Stacks = 1 },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Lens of Ice",
                        Description = "No rays: the light is gathered into the one body. While it is Frozen its Mark is not spent by blows, and a shatter of it, Marked and frozen, deals 60% more and throws its Mark onto every enemy within 4 m.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Lens, Seconds = 30f, While = AfflictionKind.Chill, While2 = AfflictionKind.Marked, Keeps = AfflictionKind.Marked } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Lens, By = StruckBy.Heavy, Needs = AfflictionKind.Chill, NeedsFull = true, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 18f,
                                Affliction = AfflictionKind.Marked },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Lens, By = StruckBy.Heavy, Needs = AfflictionKind.Chill, NeedsFull = true, Do = UpgradeAction.Afflict, Radius = 4f,
                                Affliction = AfflictionKind.Marked, Stacks = 1 },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // NEEDS: rays bending off the ice the player made, Hoarfrost's crystals and walls of ice as well as the frozen
            // (standing-things); now any spell striking a frozen enemy runs on 5 m to the nearest other, once in half a second.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = BentLight, Name = "Bent Light", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Beam,
                Description = "A ray that meets ice you made (Hoarfrost's crystals, a wall of ice, a frozen enemy) bends toward the nearest enemy and runs on 5 m: 8, a Mark.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, Needs = AfflictionKind.Chill, NeedsFull = true, Do = UpgradeAction.Arc, FromTarget = true, Count = 1, Reach = 5f, Damage = 8f, Stagger = 5f,
                        Affliction = AfflictionKind.Marked, Stacks = 1, Cooldown = 0.5f },
                },
            }, HeartPattern.LeftLow));

            // Changes its reaction's own numbers while it is in force (reaction-numbers, ReactionNumbers):
            // the suns hanging 6 s, not 4, and Prism's fan five rays reaching 7 m beyond the enemy.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = WideFan, Name = "Wide Fan", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { Shape }, Icon = UpgradeIcons.Hourglass,
                Description = "The mock suns hang 2 s longer; with Prism, the fan reaches 8 m and holds five rays.",
                ReactionChanges = new[]
                {
                    new ReactionChange { Shape = -1, Index = 0, ConditionSeconds = 6f },
                    new ReactionChange { Shape = 0, Index = 0, ConditionSeconds = 6f },
                    new ReactionChange { Shape = 0, Index = 1, Count = 5, Reach = 4f },
                    new ReactionChange { Shape = 0, Index = 2, Count = 5, Reach = 4f },
                },
            }, HeartPattern.RightLow));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = WhiteNight, Name = "White Night", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { BentLight, WideFan }, Icon = UpgradeIcons.Snowflake,
                Description = "A perfect guard against a Marked enemy that carries three or more Chill freezes it at once.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Chill, NeedsAtLeast = 3, Needs2 = AfflictionKind.Marked, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill,
                        Stacks = 99, Cooldown = 0.5f },
                },
            }, HeartPattern.Keystone));

            // The duo spell, a counter on a 10 s cooldown (SpellTimings), bought with 5 Shards once both are carried. The parry
            // form holds up the block of spar: the first blow on it is caught and split into a fan toward the attacker, chilling
            // (the branch's first element) two deep, and each ray Marks.
            // NEEDS: a perfect guard made through it in its last 0.3 s freezing the attacker outright (spell-windows); half the
            // cooldown back when nothing strikes it (missed-counter); five rays, not one cone (form-pieces).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Sunstone, Name = "Sunstone", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Counter, Requires = new[] { BentLight, WideFan }, Icon = UpgradeIcons.Gem,
                Description = "You hold up a block of clear ice spar for 1.2 s. The first blow that lands on it is caught for nothing and split: five rays fan 8 m toward the attacker, 12 and 20 stagger each, a Mark and two Chill.",
                Form = SpellForms.Parry,
                Spell = new SpellSpec { Seconds = 1.2f, Reach = 8f, Arc = 80f, Damage = 12f, Stagger = 20f, Stacks = 2 },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Marked, Stacks = 1 } },
            }, HeartPattern.Spell));
        }
    }
}

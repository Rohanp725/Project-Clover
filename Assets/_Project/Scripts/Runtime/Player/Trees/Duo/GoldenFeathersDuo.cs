using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Light and Wind (pairs_d5.md, "Light and Wind", distance 5; task 4.17), named by its reaction,
    /// Golden Feathers. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape
    /// mark, Feathered, opens Eagle's Sight and Wingbeat; Bane of Serpents (a major), Riding High (a minor), the keystone
    /// Wings Behind, and the duo spell Sweep of Wings. Light is written first, as the ring names the pair.
    /// </summary>
    public static class GoldenFeathersDuo
    {
        public const string Shape = "goldenfeathers.shape";
        public const string BaneOfSerpents = "goldenfeathers.bane";
        public const string RidingHigh = "goldenfeathers.ridinghigh";
        public const string WingsBehind = "goldenfeathers.wingsbehind";
        public const string SweepOfWings = "goldenfeathers.sweep";
        /// <summary>The condition Eagle's Sight sets (the sight held on it until your next weapon blow), and the one Sweep of
        /// Wings leaves on what it threw.</summary>
        public const string Sight = "goldenfeathers.sight";
        public const string Swept = "goldenfeathers.swept";
        private const string Reaction = "react.goldenfeathers";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Light, B = Elements.Wind;

            // NEEDS: a dodge toward the enemy carrying you all the way to it, up to 6 m, and ending in a cut (dodge-path); now
            // your next weapon blow on it within 4 s is critical and cuts 10 more.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Golden Feathers: Eagle's Sight or Wingbeat", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Golden Feathers' two shapes, chosen at a rest fire. Eagle's Sight: no feathers fly; your next dodge toward it carries you to it and ends in a critical cut. Wingbeat: the feathers beat once, throwing every other enemy near away and marking it. Either runs instead of the plain Golden Feathers.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Eagle's Sight",
                        Description = "No feathers fly. The eagle's sight stays on the enemy: your next dodge toward it carries you all the way to it, up to 6 m, and ends in a cut, 10 and critical.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Sight, Seconds = 4f, Flags = ConditionFlags.TakesCritical, EndsWith = ConditionBlow.Weapon } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Sight, By = StruckBy.Weapon, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 10f, Affliction = AfflictionKind.Marked },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Wingbeat",
                        Description = "The feathers gather and beat once: a gust from the enemy outward to 3 m throws every other enemy away from it and Marks it.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Push, Radius = 3f, Amount = 3f },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Marked, Stacks = 1 },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // NEEDS: feathers that stop at the first enemy they touch, striking only one already Marked (thrown-from-enemy); now
            // each Golden Feathers strikes everything on the feathers' line, 12 and 20 stagger, whatever shape it wears, once a
            // second.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = BaneOfSerpents, Name = "Bane of Serpents", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Claw,
                Description = "A feather that reaches an enemy already Marked strikes it for 12 and 20 stagger.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, Do = UpgradeAction.Burst, Shape = HazardShape.Downwind, Count = 3, Reach = 3.5f, Amount = 1.4f, Radius = 0.7f,
                        Delay = 0.3f, Damage = 12f, Stagger = 20f, Affliction = AfflictionKind.Marked, Cooldown = 1f },
                },
            }, HeartPattern.LeftLow));

            // Changes its reaction's own numbers while it is in force (reaction-numbers, ReactionNumbers):
            // four feathers, the farthest 9 m beyond the enemy.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = RidingHigh, Name = "Riding High", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { Shape }, Icon = UpgradeIcons.Hourglass,
                Description = "Golden Feathers' feathers fly 9 m, four of them.",
                ReactionChanges = new[]
                {
                    // The fan of feathers thrown from it (UpgradeAction.Pieces): four, riding 9 m.
                    new ReactionChange { Shape = -1, Index = 0, Count = 4, Reach = 9f },
                },
            }, HeartPattern.RightLow));

            // A ring of feathers round you on each perfect dodge; laying both at once, by a mark, it may set Golden Feathers off.
            // A half ring raised behind you and flying out 4 m (UpgradeEffect.Behind on a cone of 180 degrees), once in half a
            // second; the feathers' own look is the looks row's.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = WingsBehind, Name = "Wings Behind", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { BaneOfSerpents, RidingHigh }, Icon = UpgradeIcons.Wing,
                Description = "Every perfect dodge raises a half ring of golden feathers behind you that flies out 4 m, Marking and laying Unsteady on what it reaches.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, At = EffectAt.Self, Do = UpgradeAction.Cone, Behind = true, Reach = 4f, Arc = 180f, Damage = 0f, Affliction = AfflictionKind.Marked, Stacks = 1, Cooldown = 0.5f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, At = EffectAt.Self, Do = UpgradeAction.Cone, Behind = true, Reach = 4f, Arc = 180f, Damage = 0f, Affliction = AfflictionKind.Unsteady, Stacks = 1, Cooldown = 0.5f },
                },
            }, HeartPattern.Keystone));

            // The duo spell, an opener on a 9 s cooldown (SpellTimings), bought with 5 Shards once both are carried. The wave
            // form sweeps the wing through 160 degrees, 6 m out, Marking (the branch's first element) and unsteadying two deep
            // what it strikes and throwing it 3 m; for 3 s each weapon blow on one it threw lays another Unsteady.
            // The throw goes along the sweep, sideways across your front, not away from you (UpgradeEffect.Sideways).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = SweepOfWings, Name = "Sweep of Wings", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Opener, Requires = new[] { BaneOfSerpents, RidingHigh }, Icon = UpgradeIcons.Wing,
                Description = "A great wing of layered gold feathers sweeps from your side through 160 degrees, 6 m out: 10, 25 stagger, a Mark and two Unsteady, and everything it strikes is thrown 3 m along the sweep. For 3 s each weapon blow on an enemy it threw lays another Unsteady.",
                Form = SpellForms.Wave,
                Spell = new SpellSpec { Reach = 6f, Arc = 160f, Damage = 10f, Stagger = 25f, Stacks = 1 },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Unsteady, Stacks = 2 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Push, Shape = HazardShape.Downwind, Sideways = true, Amount = 3f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Swept, Seconds = 3f } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Swept, By = StruckBy.Weapon, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Unsteady, Stacks = 1 },
                },
            }, HeartPattern.Spell));
        }
    }
}

using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Water and Light (pairs_d4.md, "Water and Light", distance 4; task 4.17), named by its reaction,
    /// Caught Light. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark,
    /// Net or Eye, opens Thrown Wide and The Fish's Eye; Bright Water and Sun on the Spray (majors), the keystone Seen in the
    /// Water, and the duo spell The Gods' Road, bought with Shards once both are carried. Water is written first, as the ring
    /// names the pair.
    /// </summary>
    public static class CaughtLightDuo
    {
        public const string Shape = "caughtlight.shape";
        public const string BrightWater = "caughtlight.brightwater";
        public const string SunOnTheSpray = "caughtlight.spray";
        public const string SeenInTheWater = "caughtlight.seen";
        public const string TheGodsRoad = "caughtlight.road";
        /// <summary>The conditions the two shapes set (Thrown Wide's caught and its bearer's haul; The Fish's Eye), and Seen
        /// in the Water's open net.</summary>
        public const string Hauled = "caughtlight.hauled";
        public const string Bearer = "caughtlight.bearer";
        public const string Eye = "caughtlight.eye";
        public const string Open = "caughtlight.open";
        private const string Reaction = "react.caughtlight";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Water, B = Elements.Light;

            // Thrown Wide's net catches whoever walks onto it later too, as the plain net (UpgradeEffect.Inside), and is told as the
            // net for the marks that answer it. A blow passed through its enemy to strike one behind it (cone-from-enemy): The Fish's
            // Eye's blow; now a burst of 12 on the line beyond it.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Caught Light: Thrown Wide or The Fish's Eye", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Caught Light's two shapes, chosen at a rest fire. Thrown Wide: the net is cast wider and hauled in when its time ends. The Fish's Eye: no net; the light gathers into one white eye, and your next blow on it is a triple critical. Either runs instead of the plain Caught Light.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Thrown Wide",
                        Description = "The net of light is cast 5 m wide for 4 s: all in it are Marked and caught, a quarter slower. When its time ends it is hauled: everything within 2.5 m of the enemy that bore it is dragged 2 m toward it.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Radius = 2.5f, Seconds = 4f, Interval = 0.5f, Affliction = AfflictionKind.Marked, Stacks = 1,
                                Inside = new ConditionSpec { Id = Hauled, Seconds = 0.5f, Pace = 0.75f, Is = Reactions.LightNet }, Ground = Reactions.LightNet },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, Radius = 2.5f, State = new ConditionSpec { Id = Hauled, Seconds = 4f, Pace = 0.75f, Is = Reactions.LightNet } },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Bearer, Seconds = 4f } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Bearer, Do = UpgradeAction.Pull, Radius = 2.5f, Amount = 2f },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "The Fish's Eye",
                        Description = "No net. The water on the body gathers the light into one white eye: your next weapon blow on it within 6 s is critical at three times the damage, spends its Mark, and passes through to strike behind it, 12.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition,
                                State = new ConditionSpec { Id = Eye, Seconds = 6f, Flags = ConditionFlags.TakesCritical | ConditionFlags.WeaponTakenShare, Scale = 2f, EndsWith = ConditionBlow.Weapon } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Eye, By = StruckBy.Weapon, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Marked },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Eye, By = StruckBy.Weapon, Do = UpgradeAction.Burst, Shape = HazardShape.Downwind, Count = 1, Reach = 2f, Amount = 1f,
                                Radius = 1f, Damage = 12f, Stagger = 10f, Affliction = AfflictionKind.Marked },
                        },
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = BrightWater, Name = "Bright Water", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Drop,
                Description = "The blow that ends a chain lays Soaked and a Mark together.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.ChainEnd, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.ChainEnd, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Marked, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            // NEEDS: the place a dodge ends (dodge-path); now the spray is round you at the perfect dodge itself.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = SunOnTheSpray, Name = "Sun on the Spray", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Boot,
                Description = "A perfect dodge Soaks and Marks every enemy within 3 m of where it ends.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, At = EffectAt.Self, Cooldown = 0.5f, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, At = EffectAt.Self, Cooldown = 0.5f, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Marked, Stacks = 1 },
                },
            }, HeartPattern.RightLow));

            // While the net lies, every blow on those in it is critical, as a mark of light makes it, and no mark is spent.
            // The net read from each enemy, in whichever shape: the opening lasts only while it is caught in the net
            // (ConditionSpec.WhileIn). Those who walk onto the net later are opened too, at each of its bites (UpgradeTrigger.InGround).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = SeenInTheWater, Name = "Seen in the Water", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { BrightWater, SunOnTheSpray }, Icon = UpgradeIcons.Eye,
                Description = "While an enemy stands on the net, your blows on any other enemy on it are critical as if it were Marked, without spending a mark.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, Do = UpgradeAction.Condition, Radius = 3f,
                        State = new ConditionSpec { Id = Open, Seconds = 4f, Flags = ConditionFlags.TakesCritical, WhileIn = Reactions.LightNet } },
                    new UpgradeEffect { On = UpgradeTrigger.InGround, Condition = Reactions.LightNet, Do = UpgradeAction.Condition,
                        State = new ConditionSpec { Id = Open, Seconds = 1f, Flags = ConditionFlags.TakesCritical, WhileIn = Reactions.LightNet } },
                },
            }, HeartPattern.Keystone));

            // The duo spell, bought with 5 Shards once both are carried (no god gives it): a movement on a 9 s cooldown
            // (SpellTimings). The step form runs the rainbow to the enemy faced, cast in Water (Soaked), and the falling cut
            // Marks it, so Caught Light opens under it; the road of spray stands 3 s on the line you ran, Soaking and Marking.
            // The road runs to the nearest Soaked or Marked enemy within 10 m before the one faced (SpellSpec.AimAt).
            // NEEDS: light pressed as you land making the cut critical (spell-windows); the arch of falling, lit spray (form-pieces).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheGodsRoad, Name = "The Gods' Road", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Movement, Requires = new[] { BrightWater, SunOnTheSpray }, Icon = UpgradeIcons.Arrow,
                Description = "A rainbow of falling spray arches from you to the nearest Soaked or Marked enemy within 10 m, or else the enemy you face. You run it, untouchable, and come down on it with a falling cut, 18 and 30 stagger, Soaked and Marked. The road stands 3 s and Soaks and Marks what stands in it.",
                Form = SpellForms.Step,
                Spell = new SpellSpec { Reach = 10f, Damage = 18f, Stagger = 30f, Stacks = 1, Seconds = 1.5f, AimAt = AfflictionKind.Soaked, AimAt2 = AfflictionKind.Marked },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Marked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnSpell = true, Do = UpgradeAction.Hazard, Shape = HazardShape.Line, Count = 5, Reach = 8f, Radius = 0.7f, Seconds = 3f, Interval = 1f,
                        Affliction = AfflictionKind.Soaked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnSpell = true, Do = UpgradeAction.Hazard, Shape = HazardShape.Line, Count = 5, Reach = 8f, Radius = 0.7f, Seconds = 3f, Interval = 1f,
                        Affliction = AfflictionKind.Marked, Stacks = 1 },
                },
            }, HeartPattern.Spell));
        }
    }
}

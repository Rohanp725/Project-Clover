using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Ice and Earth (pairs_d3.md, "Ice and Earth", distance 3; task 4.16), named by its reaction, Frost
    /// Heave. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark opens
    /// Sorted Stones and Pingo; Hard Frost and Frozen Footing (majors), Permafrost (a minor), the keystone Winter in the
    /// Bones, and the duo spell The Hag's Staff. Ice is written first.
    /// </summary>
    public static class FrostHeaveDuo
    {
        public const string Shape = "frostheave.shape";
        public const string HardFrost = "frostheave.hardfrost";
        public const string FrozenFooting = "frostheave.footing";
        public const string Permafrost = "frostheave.permafrost";
        public const string WinterInTheBones = "frostheave.bones";
        public const string TheHagsStaff = "frostheave.hagsstaff";
        /// <summary>The Hag's Staff's disc of frozen ground, your own while it lasts.</summary>
        public const string HagsGround = "frostheave.hagsground";
        /// <summary>The conditions the two shapes set: each heave, and Pingo's mound with its core of ice.</summary>
        public const string Sorted = "frostheave.sorted";
        public const string PingoHeave = "frostheave.pingoheave";
        public const string Pingo = "frostheave.pingo";
        private const string Reaction = "react.frostheave";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Ice, B = Elements.Earth;

            // Both shapes leave the rimed mound as the plain reaction does (UpgradeAction.Raise); Pingo's bursts with its core.
            // NEEDS: Sorted Stones' ring of stones that nothing crosses (a model of its own, standing-things); now the heave
            // shoves the others 3 m out.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Frost Heave: Sorted Stones or Pingo", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Frost Heave's two shapes, chosen at a rest fire. Sorted Stones: the heave pushes everything else out, leaving it alone with you. Pingo: the mound has a core of ice that bursts. Either runs instead of the plain Frost Heave.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Sorted Stones",
                        Description = "The floor heaves up under it as before, 12, 25 stagger and thrown up 1 s, and everything else within 3 m is pushed out to a ring, leaving it alone with you.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Sorted, Seconds = 0.4f } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Sorted, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 12f, Stagger = 25f, Affliction = AfflictionKind.Tremor },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Sorted, Do = UpgradeAction.Lift, Seconds = 1f, Stagger = 1f },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Sorted, Do = UpgradeAction.Push, Radius = 3f, Amount = 3f },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Sorted, Do = UpgradeAction.Raise, Raises = Clover.World.StandingKind.RimedMound, RaiseAt = Clover.World.StandingPlace.Behind, Reach = 1.1f, Seconds = 6f, Amount = 30f },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Pingo",
                        Description = "The floor heaves up under it as before, and the mound has a core of ice: 2 s later it bursts, 24 and two Chill to everything within 2.5 m.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = PingoHeave, Seconds = 0.4f } },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Pingo, Seconds = 2.4f } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = PingoHeave, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 12f, Stagger = 25f, Affliction = AfflictionKind.Tremor },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = PingoHeave, Do = UpgradeAction.Lift, Seconds = 1f, Stagger = 1f },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Pingo, Do = UpgradeAction.Burst, Radius = 2.5f, Damage = 24f, Affliction = AfflictionKind.Chill, Stacks = 2 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = PingoHeave, Do = UpgradeAction.Raise, Raises = Clover.World.StandingKind.RimedMound, RaiseAt = Clover.World.StandingPlace.Behind, Reach = 1.1f, Seconds = 6f, Amount = 30f },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Pingo, Do = UpgradeAction.BreakStanding, Radius = 2.5f },
                        },
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = HardFrost, Name = "Hard Frost", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Hammer,
                Description = "Heavy blows on a chilled enemy shake it; heavy blows on a shaken enemy chill it.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Chill, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Tremor, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Tremor, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            // Only while you stand within 2 m of a mound (UpgradeEffect.NearStanding).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = FrozenFooting, Name = "Frozen Footing", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard while you stand within 2 m of a mound heaves the ground under the attacker.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, NearStanding = 2f, Raises = Clover.World.StandingKind.RimedMound, Do = UpgradeAction.ForceReaction, Reaction = Reaction } },
            }, HeartPattern.RightLow));

            // Tremor kept from fading while the enemy carries Chill (AfflictionRules.KeepWhile).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Permafrost, Name = "Permafrost", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { HardFrost }, Icon = UpgradeIcons.Snowflake,
                Description = "Tremor does not fade from a chilled enemy.",
                Rules = book => book.Afflictions.KeepWhile(AfflictionKind.Tremor, AfflictionKind.Chill),
            }, HeartPattern.LeftHigh));

            // A shaken enemy's freeze held twice as long (AfflictionRules.FreezeLongerWith); the shatter's heave is built.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = WinterInTheBones, Name = "Winter in the Bones", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { HardFrost, FrozenFooting }, Icon = UpgradeIcons.Rock,
                Description = "A Frozen enemy that is shaken stays frozen twice as long, and shattering it heaves the ground under every enemy within 4 m.",
                Rules = book => book.Afflictions.FreezeLongerWith(AfflictionKind.Tremor, 2f),
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Shatter, Needs = AfflictionKind.Tremor, Do = UpgradeAction.Lift, Radius = 4f, Seconds = 1f, Cooldown = 0.3f },
                    new UpgradeEffect { On = UpgradeTrigger.Shatter, Needs = AfflictionKind.Tremor, Do = UpgradeAction.Burst, Radius = 4f, Damage = 12f, Stagger = 25f, Affliction = AfflictionKind.Tremor, Cooldown = 0.3f },
                },
            }, HeartPattern.Keystone));

            // The duo spell, bought with Shards once both are carried (no god gives it): a breaker on a 12 s cooldown (SpellTimings). The field form hardens the ground round
            // you; each second it chills (the branch's first element) and shakes what stands on it, so it heaves. The disc is your own
            // ground (SpellSpec.Ground): a heavy blow on an enemy standing on it drives frost under it (UpgradeEffect.TargetOnGround),
            // 25 more stagger and a heave of half a second.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheHagsStaff, Name = "The Hag's Staff", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Breaker, Requires = new[] { HardFrost, FrozenFooting }, Icon = UpgradeIcons.Snowflake,
                Description = "You strike the ground and hammer the frost into it. A disc 6 m round you goes hard and white for 6 s; each second every enemy on it is chilled and shaken, so it heaves when the two meet.",
                Form = SpellForms.Field,
                Spell = new SpellSpec { Reach = 0f, Radius = 6f, Seconds = 6f, Interval = 1f, LingerDamage = 2f, Stacks = 1, Ground = HagsGround },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Tremor, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, TargetOnGround = HagsGround, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 0f, Stagger = 25f, Stacks = 0, Cooldown = 0.2f },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, TargetOnGround = HagsGround, Do = UpgradeAction.Lift, Seconds = 0.5f, Stagger = 1f, Cooldown = 0.2f },
                },
            }, HeartPattern.Spell));
        }
    }
}

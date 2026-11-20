using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Tempest: the duo branch Lightning and Wind share, reshaped by pairs_d1_d2.md ("Lightning and Wind", task 4.15) and
    /// drawn in the chart's heart (<see cref="HeartPattern"/>). Its reaction is Thunder Gust; the Shape mark opens its two
    /// shapes, Thunderhead and Bolt from the Blue. Charged Air is cut (the Shape mark takes its place), Thunder Lift is now
    /// Thunderhead, both in <see cref="RetiredMarks"/>, and Storm Overhead, offered to Lightning's tree, whose roster is
    /// full, is kept hidden for the Evolutions (<see cref="SpareSpells"/>). Kept: The Sky Finds Them (it pays off Thunderhead), Slip Current, Eye of the Storm (the
    /// duo spell) and Falling Thunder, the branch's top mark. Open while both are carried.
    /// </summary>
    public static class TempestTree
    {
        public const string Shape = "tempest.shape";
        public const string TheSkyFindsThem = "tempest.sky";
        public const string SlipCurrent = "tempest.slip";
        public const string EyeOfTheStorm = "tempest.eye";
        public const string FallingThunder = "tempest.falling";
        /// <summary>The condition Thunderhead sets: the enemy rising, the bolt meeting it at the top.</summary>
        public const string Rising = "thundergust.rising";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Lightning, B = Elements.Wind;

            // NEEDS: thrown into something (Bolt from the Blue's arc runs along the line downwind of it, each enemy taking
            // the whole discharge it would have had; now an arc through three from it, 14 each).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Thunder Gust: Thunderhead or Bolt from the Blue", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Thunder Gust's two shapes, chosen at a rest fire. Thunderhead: it is thrown up, not away, and a bolt discharges it at the top of its rise. Bolt from the Blue: nothing is thrown, and the discharge runs on as an arc through the enemies behind it. Either runs instead of the plain Thunder Gust.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Thunderhead",
                        Description = "It is thrown up, not away, for 1 s, and a bolt meets it at the top of its rise and discharges its Static there, 7 and 5 stagger a stack to all within 4 m, over the heads of the rest.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Lift, Seconds = 1f },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Rising, Seconds = 0.5f } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Rising, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 4f, Stagger = 5f },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Bolt from the Blue",
                        Description = "Nothing is thrown: its Static goes off where it stands, and the discharge runs on as an arc through up to three enemies behind it, 14, 10 stagger and a Static each.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 1.5f, Stagger = 5f },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Arc, FromTarget = true, Count = 3, Reach = 5f, Damage = 14f, Stagger = 10f, Affliction = AfflictionKind.Static, Stacks = 1 },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // The enemies a dodge passed cannot be counted: whatever stands close where the dodge ends is left one deeper in
            // both. The stamina paid back is still to be written by hand.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = SlipCurrent, Name = "Slip Current", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Boot,
                Description = "Where a dodge ends, every enemy close to you is left one deeper in Static and Unsteady.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, Do = UpgradeAction.Afflict, At = EffectAt.Self, Radius = 2.2f, Affliction = AfflictionKind.Static, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, Do = UpgradeAction.Afflict, At = EffectAt.Self, Radius = 2.2f, Affliction = AfflictionKind.Unsteady, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            // Answers every throw into the air, Thunderhead's among them.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheSkyFindsThem, Name = "The Sky Finds Them", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Chain,
                Description = "An enemy thrown into the air is struck by a bolt, which leaps on to two more and leaves Static on each.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Lifted, Do = UpgradeAction.Arc, Damage = 14f, Count = 2, Reach = 4f, Affliction = AfflictionKind.Static, Stacks = 2 } },
            }, HeartPattern.RightLow));

            // BESPOKE (built): EyeForm. Each bite lays the affliction of the branch's first element, Static. The duo spell.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = EyeOfTheStorm, Name = "Eye of the Storm", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Breaker, Requires = new[] { SlipCurrent, TheSkyFindsThem }, Icon = UpgradeIcons.Spiral,
                Description = "While you keep the spell's button held you are the storm: a turning column of wind and lightning that drags enemies in and carries the light ones round. Let go and it throws them out, and a bolt meets each where it lands. 50 mana.",
                Form = SpellForms.Eye,
                Spell = new SpellSpec { Cost = 50f, Reach = 5f, Radius = 3.5f, Seconds = 2.5f, Interval = 0.4f, Damage = 6f, Stagger = 20f, LingerDamage = 12f, Stacks = 1 },
            }, HeartPattern.Spell));

            // BESPOKE (built): ThunderForm. The branch's top mark, begun by a finisher.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = FallingThunder, Name = "Falling Thunder", Element = A, Element2 = B, Kind = UpgradeKind.Signature, Icon = UpgradeIcons.Crown,
                Description = "After your finisher lands you throw the enemy skyward, vanish, and come down on it as a bolt. Where you land, everything near is shocked and thrown back.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Form, Form = SpellForms.Thunder, Affliction = AfflictionKind.Static, Spell = new SpellSpec { Cost = 0f, Damage = 22f, Stagger = 40f, Radius = 4.5f, Stacks = 3 } } },
            }, HeartPattern.Crown));
        }
    }
}

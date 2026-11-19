using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Tempest: the branch Lightning and Wind share. Static in an enemy that has lost its footing: discharges throw
    /// enemies into the sky and the sky answers. It is open to one who holds either and has held or consumed the
    /// other in this run. The god's gift at its crown is given by consuming one of the two into the other.
    /// </summary>
    public static class TempestTree
    {
        public const string ChargedAir = "tempest.charged";
        public const string ThunderLift = "tempest.lift";
        public const string TheSkyFindsThem = "tempest.sky";
        public const string SlipCurrent = "tempest.slip";
        public const string StormOverhead = "tempest.cloud";
        public const string EyeOfTheStorm = "tempest.eye";
        public const string FallingThunder = "tempest.falling";

        public static void Add(List<Upgrade> to)
        {
            // The first element is the one whose mix this is: the earlier of the two round the ring.
            const string A = Elements.Lightning, B = Elements.Wind;

            // The foot of the branch. Both afflictions are named; the runner lays only what the held element's root has not laid with the same blow (schema 15.8). Static left by a Wind holder is set off by a heavy blow here, as Lightning's root would.
            to.Add(new Upgrade { Id = ChargedAir, Name = "Charged Air", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, X = 0f, Y = 1f, Icon = UpgradeIcons.Bolt,
                Description = "Your blows lay Static and Unsteady together.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Unsteady, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.LightHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Static, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 4f, Cooldown = 0.3f },
                } });
            to.Add(new Upgrade { Id = ThunderLift, Name = "Thunder Lift", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { ChargedAir }, X = -0.5f, Y = 2.36f, Icon = UpgradeIcons.Arrow,
                Description = "A discharge of five Static throws its enemy into the air.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SetOff, Spent = AfflictionKind.Static, SpentAtLeast = 5, Do = UpgradeAction.Lift, Seconds = 1f } } });
            // The enemies a dodge passed cannot be counted: whatever stands close where the dodge ends is left one deeper in
            // both. The stamina paid back is still to be written by hand.
            to.Add(new Upgrade { Id = SlipCurrent, Name = "Slip Current", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { ChargedAir }, X = 0.5f, Y = 2.36f, Icon = UpgradeIcons.Boot,
                Description = "Where a dodge ends, every enemy close to you is left one deeper in Static and Unsteady.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, Do = UpgradeAction.Afflict, At = EffectAt.Self, Radius = 2.2f, Affliction = AfflictionKind.Static, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, Do = UpgradeAction.Afflict, At = EffectAt.Self, Radius = 2.2f, Affliction = AfflictionKind.Unsteady, Stacks = 1 },
                } });
            to.Add(new Upgrade { Id = TheSkyFindsThem, Name = "The Sky Finds Them", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { ThunderLift }, X = 0f, Y = 3.48f, Icon = UpgradeIcons.Chain,
                Description = "An enemy in the air is struck by a bolt at the top of its rise.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Lifted, Do = UpgradeAction.Arc, Damage = 14f, Count = 2, Reach = 4f, Affliction = AfflictionKind.Static, Stacks = 2 } } });
            // BESPOKE: a cloud that follows the player and answers each dodge beneath it with a bolt where they stood, three for a
            // perfect dodge. Until then the rain form: bolts fall for a while round the place it was cast.
            to.Add(new Upgrade { Id = StormOverhead, Name = "Storm Overhead", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { SlipCurrent }, X = 0.5f, Y = 4.6f, Icon = UpgradeIcons.Comet,
                Description = "You call a storm over the ground before you. For ten seconds bolts fall inside its circle, one every 0.8 seconds.",
                Form = "rain",
                Spell = new SpellSpec { Cost = 40f, Reach = 1f, Radius = 3f, Seconds = 10f, Interval = 0.8f, Damage = 14f, Stagger = 20f, Stacks = 1 } });
            // BESPOKE (built): EyeForm. Each bite lays the affliction of the element held, not one of each.
            to.Add(new Upgrade { Id = EyeOfTheStorm, Name = "Eye of the Storm", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { ThunderLift }, X = -0.5f, Y = 4.6f, Icon = UpgradeIcons.Spiral,
                Description = "While you keep the spell's button held you are the storm: a turning column of wind and lightning that drags enemies in and carries the light ones round. Let go and it throws them out, and a bolt meets each where it lands. 50 mana.",
                Form = SpellForms.Eye,
                Spell = new SpellSpec { Cost = 50f, Reach = 5f, Radius = 3.5f, Seconds = 2.5f, Interval = 0.4f, Damage = 6f, Stagger = 20f, LingerDamage = 12f, Stacks = 1 } });

            // BESPOKE (built): ThunderForm.
            to.Add(new Upgrade { Id = FallingThunder, Name = "Falling Thunder", Element = A, Element2 = B, Kind = UpgradeKind.Signature, X = 0f, Y = 5.8f, Icon = UpgradeIcons.Crown,
                Description = "After your finisher lands you throw the enemy skyward, vanish, and come down on it as a bolt. Where you land, everything near is shocked and thrown back.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Form, Form = SpellForms.Thunder, Affliction = AfflictionKind.Static, Spell = new SpellSpec { Cost = 0f, Damage = 22f, Stagger = 40f, Radius = 4.5f, Stacks = 3 } } } });
        }
    }
}

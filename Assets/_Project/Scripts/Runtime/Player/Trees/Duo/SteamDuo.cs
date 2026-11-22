using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Fire and Water (pairs_d4.md, "Fire and Water", distance 4; task 4.17), named by its reaction,
    /// Steam. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark, Two
    /// Sisters, opens Blind Steam and Scald; Hissing Blade and Strike Through Steam (majors), the keystone Kettle, and the
    /// duo spell The Sisters' Quarrel, bought with Shards once both are carried. Fire is written first, as the ring names
    /// the pair.
    /// </summary>
    public static class SteamDuo
    {
        public const string Shape = "steam.shape";
        public const string HissingBlade = "steam.hissingblade";
        public const string StrikeThroughSteam = "steam.strikethrough";
        public const string Kettle = "steam.kettle";
        public const string TheSistersQuarrel = "steam.quarrel";
        /// <summary>The condition Blind Steam sets on those in its cloud, Strike Through Steam's second after a steam, and the
        /// raw scald The Sisters' Quarrel leaves.</summary>
        public const string Blind = "steam.blind";
        public const string Scalded = "steam.scalded";
        public const string Raw = "steam.raw";
        private const string Reaction = "react.steam";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Fire, B = Elements.Water;

            // Blind Steam's cloud blinds whoever stands in it, those in it as it forms and those who step in later, while they
            // stay (UpgradeEffect.Inside); one that walks out sees again.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Steam: Blind Steam or Scald", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Steam's two shapes, chosen at a rest fire. Blind Steam: the plume lies low, a cloud the enemies in it cannot see out of. Scald: no cloud, a harder burst that throws them and leaves the Burn boiling. Either runs instead of the plain Steam.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Blind Steam",
                        Description = "The water flashes off as before, taking half the Burn, but the plume does not lift: it lies 3 m round it for 4 s, low and white. Enemies in it see only 2 m and swing a fifth slower, and it stings, 3 a second.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Soaked },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Burn, Share = 0.5f },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Radius = 3f, Seconds = 4f, Interval = 1f, Damage = 3f, Affliction = AfflictionKind.Soaked,
                                Inside = new ConditionSpec { Id = Blind, Seconds = 0.5f, Flags = ConditionFlags.SightShort | ConditionFlags.WindupSlowed, Sight = 2f, Scale = 0.8f } },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Scald",
                        Description = "No cloud, and the Burn is not spent: it boils on. The water flashes off in a harder burst, 14 and 6 more for each Burn it carries, 30 stagger, and everything within 2.5 m is thrown 1.5 m.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Soaked },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Burn, CountOnly = true },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Affliction = AfflictionKind.Soaked, Radius = 2.5f, Damage = 14f, DamagePerSpent = 6f, Stagger = 30f },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Push, Radius = 2.5f, Amount = 1.5f },
                        },
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = HissingBlade, Name = "Hissing Blade", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Blade,
                Description = "Your heavy blows lay Burn and Soaked together.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            // For a second after its steam the enemy is scalded: its next weapon blow lands half again as hard. The share is on
            // the stagger of every blow in that second, not only the weapon's (one Scale for both flags).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = StrikeThroughSteam, Name = "Strike Through Steam", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Burst,
                Description = "A weapon blow on an enemy within a second of its steam is a scald: half again the damage and stagger.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, Do = UpgradeAction.Condition,
                        State = new ConditionSpec { Id = Scalded, Seconds = 1f, Flags = ConditionFlags.WeaponTakenShare | ConditionFlags.StaggerTakenShare, Scale = 1.5f, EndsWith = ConditionBlow.Weapon } },
                },
            }, HeartPattern.RightLow));

            // A Soaked enemy that dies burning goes up as Steam where it fell, in the shape worn, past its rest, at full strength
            // (UpgradeAction.EchoReaction with the reaction named: its striking effects, counting all the Burn it carried).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Kettle, Name = "Kettle", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { HissingBlade, StrikeThroughSteam }, Icon = UpgradeIcons.Flask,
                Description = "A Soaked enemy that dies burning goes up as steam at full strength, with no rest.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Burn, Needs2 = AfflictionKind.Soaked, Do = UpgradeAction.EchoReaction, Reaction = Reaction, Amount = 1f },
                },
            }, HeartPattern.Keystone));

            // The duo spell, bought with 5 Shards once both are carried (no god gives it): a breaker on a 12 s cooldown
            // (SpellTimings). The bullets form closes two strikes on the enemy faced, cast in Fire (Burn); each lays Soaked,
            // and where they meet the steam blast goes off and forces Steam at full in its shape. For 3 s after, those caught
            // are scalded raw: weapon blows on them strike a quarter harder and each lays a Burn.
            // NEEDS: two curved lanes, flame from the left hand and a wave from the right, striking what they cross as they close
            // in 0.45 s (form-pieces); now two strikes of a ring closing on the enemy.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheSistersQuarrel, Name = "The Sisters' Quarrel", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Breaker, Requires = new[] { HissingBlade, StrikeThroughSteam }, Icon = UpgradeIcons.Wave,
                Description = "Flame from one hand and a wave from the other close like pincers on the enemy you face, 7 m off at most. Where they meet steam bursts, 30 and 55 stagger in 3.5 m, and Steam goes off at full. For 3 s your weapon blows on those caught strike harder and burn.",
                Form = SpellForms.Bullets,
                Spell = new SpellSpec { Count = 2, Radius = 3f, Delay = 0.45f, Reach = 7f, Damage = 8f, Stagger = 10f, Stacks = 1 },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Cooldown = 0.4f, Do = UpgradeAction.Burst, Affliction = AfflictionKind.Soaked, Radius = 3.5f, Damage = 30f, Stagger = 55f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Cooldown = 0.4f, Do = UpgradeAction.ForceReaction, Reaction = Reaction },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Condition,
                        State = new ConditionSpec { Id = Raw, Seconds = 3f, Flags = ConditionFlags.WeaponTakenShare, Scale = 1.25f } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Raw, By = StruckBy.Weapon, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1 },
                },
            }, HeartPattern.Spell));
        }
    }
}

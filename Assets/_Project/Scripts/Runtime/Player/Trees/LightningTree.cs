using System.Collections.Generic;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Lightning: speed and precision. Three boughs from Static: on the left the guard and the single exact blow, in
    /// the middle the storm called down, on the right the dodge. The seven first marks and the god's Storm Step are
    /// done by hand in <see cref="PlayerUpgrades"/> under their ids; the rest are data.
    /// </summary>
    public static class LightningTree
    {
        public const string KeenEdge = "lightning.keen";
        public const string QuickHands = "lightning.quick";
        public const string Capacitor = "lightning.capacitor";
        public const string Grounding = "lightning.grounding";
        public const string Slipstream = "lightning.slipstream";
        public const string ChainLance = "lightning.lance";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Lightning;

            to.Add(new Upgrade { Id = Upgrade.Static, Name = "Static", Element = E, Kind = UpgradeKind.Root, X = 0f, Y = 1.2f, Icon = UpgradeIcons.Bolt,
                Description = "Blows build Static on an enemy, up to five. A heavy blow or a finisher discharges it, and it arcs to everyone near." });

            // Left bough: the guard and the single exact blow.
            to.Add(new Upgrade { Id = Upgrade.StoredCharge, Name = "Stored Charge", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Static }, X = -1.21f, Y = 2.4f, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard banks a charge. The next blow spends it for double damage and stagger." });
            to.Add(new Upgrade { Id = KeenEdge, Name = "Keen Edge", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.StoredCharge }, X = -1.98f, Y = 3.42f, Icon = UpgradeIcons.Eye,
                Description = "Now and then a blow of the weapon is critical.",
                Stats = new[] { new StatMod(PlayerStat.CritChance, 0.06f) } });
            to.Add(new Upgrade { Id = Upgrade.DelayedCuts, Name = "Delayed Cuts", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.StoredCharge }, X = -1.61f, Y = 4.2f, Icon = UpgradeIcons.Blade, Weapon = Weapons.KatanaName,
                Description = "The Iaido Slash passes through. Its cuts land a moment later, all at once, when the blade is home." });
            to.Add(new Upgrade { Id = Grounding, Name = "Grounding", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.DelayedCuts, Upgrade.StoredCharge }, X = -2.0f, Y = 6.0f, Icon = UpgradeIcons.Chain,
                Description = "A perfect guard sends an arc through the one who struck and on to two more.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Arc, Damage = 12f, Stagger = 20f, Count = 2, Reach = 5f } } });

            // Middle bough: the storm called down.
            to.Add(new Upgrade { Id = Upgrade.LightningRod, Name = "Lightning Rod", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Upgrade.Static }, X = 0f, Y = 2.9f, Icon = UpgradeIcons.Tower,
                Description = "Hold the spell button to plant a rod. Bolts fall around it for a while. 40 mana.",
                Form = SpellForms.Rod,
                Spell = new SpellSpec { Cost = 40f, Reach = 2.5f, Radius = 3.5f, Seconds = 6f, Interval = 0.9f, Damage = 12f, Stagger = 12f, Delay = 0.2f, Stacks = 0 } });
            to.Add(new Upgrade { Id = Capacitor, Name = "Capacitor", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.LightningRod }, X = 0.62f, Y = 3.8f, Icon = UpgradeIcons.Gem,
                Description = "A perfect guard gives more mana.",
                Stats = new[] { new StatMod(PlayerStat.ManaFromGuards, 0.3f) } });
            to.Add(new Upgrade { Id = Upgrade.Overclock, Name = "Overclock", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { Upgrade.LightningRod }, X = 0f, Y = 4.7f, Icon = UpgradeIcons.Hourglass,
                Description = "Overdrive opens with a breath in which everything but you is slowed." });
            to.Add(new Upgrade { Id = ChainLance, Name = "Chain Lance", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Upgrade.Overclock, Capacitor }, X = 0f, Y = 6.5f, Icon = UpgradeIcons.Beam,
                Description = "Hold the spell button to throw a bolt that leaps through four more and leaves Static on each. 35 mana.",
                Form = SpellForms.Bolt,
                Spell = new SpellSpec { Cost = 35f, Reach = 12f, Damage = 26f, Stagger = 30f, Count = 4, Radius = 5f, Stacks = 2 } });

            // Right bough: the dodge.
            to.Add(new Upgrade { Id = Upgrade.Blink, Name = "Blink", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Static }, X = 1.21f, Y = 2.4f, Icon = UpgradeIcons.Boot,
                Description = "The dodge becomes a short step through lightning that shocks whatever it passes through." });
            to.Add(new Upgrade { Id = QuickHands, Name = "Quick Hands", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.Blink }, X = 1.98f, Y = 3.42f, Icon = UpgradeIcons.Fist,
                Description = "The weapon is swung a little faster.",
                Stats = new[] { new StatMod(PlayerStat.SwingSpeed, 0.05f) } });
            to.Add(new Upgrade { Id = Upgrade.Afterimage, Name = "Afterimage", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Blink }, X = 1.61f, Y = 4.2f, Icon = UpgradeIcons.Eye,
                Description = "A dodge at the last moment leaves a copy of you that strikes where you stood." });
            to.Add(new Upgrade { Id = Slipstream, Name = "Slipstream", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Afterimage }, X = 2.0f, Y = 6.0f, Icon = UpgradeIcons.Wing,
                Description = "A dodge at the last moment quickens your feet and your weapon for a while.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Boon, Stat = PlayerStat.MoveSpeed, Amount = 0.25f, Seconds = 6f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Boon, Stat = PlayerStat.SwingSpeed, Amount = 0.15f, Seconds = 6f },
                } });

            to.Add(new Upgrade { Id = Upgrade.StormStep, Name = "Storm Step", Element = E, Kind = UpgradeKind.Signature, X = 0f, Y = 12f, Icon = UpgradeIcons.Crown,
                Description = "While Overdrive lasts, every dodge costs nothing and shocks what it passes through." });
        }
    }
}

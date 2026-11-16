using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Fire: pressure and momentum. Three boughs from Heat: on the left what a guard gives back, in the middle the
    /// sword and the leap, on the right fire left on the ground. The seven first marks are done by hand in
    /// <see cref="PlayerUpgrades"/> under their ids; the rest are data.
    /// </summary>
    public static class FireTree
    {
        public const string Kindling = "fire.kindling";
        public const string Stoked = "fire.stoked";
        public const string DryTinder = "fire.tinder";
        public const string SearingGuard = "fire.searing";
        public const string Wildfire = "fire.wildfire";
        public const string CinderFan = "fire.fan";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Fire;

            to.Add(new Upgrade { Id = Upgrade.Heat, Name = "Heat", Element = E, Kind = UpgradeKind.Root, X = 0f, Y = 1.2f, Icon = UpgradeIcons.Flame,
                Description = "Blows landed without being struck build Heat, up to five. Each level adds damage. A blow taken puts it out. Kindle fills it at once." });

            // Left bough: what a guard gives back.
            to.Add(new Upgrade { Id = Upgrade.Backdraft, Name = "Backdraft", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Heat }, X = -1.21f, Y = 2.4f, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard throws a cone of fire back at the one who struck." });
            to.Add(new Upgrade { Id = Kindling, Name = "Kindling", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.Backdraft }, X = -1.98f, Y = 3.42f, Icon = UpgradeIcons.Hourglass,
                Description = "The moment in which a guard is perfect lasts a little longer.",
                Stats = new[] { new StatMod(PlayerStat.PerfectGuardWindow, 0.15f) } });
            to.Add(new Upgrade { Id = Upgrade.BurnAndDetonate, Name = "Burn and Detonate", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Backdraft }, X = -1.61f, Y = 4.2f, Icon = UpgradeIcons.Burst,
                Description = "Every blow of the weapon sets a burn, up to five deep. A perfect guard against a burning enemy sets all of it off at once." });
            to.Add(new Upgrade { Id = SearingGuard, Name = "Searing Guard", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.BurnAndDetonate }, X = -2.0f, Y = 6.0f, Icon = UpgradeIcons.Wall,
                Description = "A blow caught on the guard burns the one who struck it.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Guard, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 2 } } });

            // Middle bough: the sword and the leap.
            to.Add(new Upgrade { Id = Upgrade.BurningCrescent, Name = "Burning Crescent", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Heat }, X = 0f, Y = 2.9f, Icon = UpgradeIcons.Blade, Weapon = Weapons.KatanaName,
                Description = "The Iaido Slash throws a slow arc of flame that travels on after the cut." });
            to.Add(new Upgrade { Id = Stoked, Name = "Stoked", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.BurningCrescent }, X = 0.62f, Y = 3.8f, Icon = UpgradeIcons.Fist,
                Description = "Every blow of the weapon hits a little harder.",
                Stats = new[] { new StatMod(PlayerStat.WeaponDamage, 0.05f) } });
            to.Add(new Upgrade { Id = Upgrade.MeteorLeap, Name = "Meteor Leap", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Upgrade.BurningCrescent }, X = 0f, Y = 4.7f, Icon = UpgradeIcons.Comet,
                Description = "Hold the spell button to leap out of the fight and come down on the enemy faced, leaving a burning crater. 50 mana.",
                Form = SpellForms.Leap,
                Spell = new SpellSpec { Cost = 50f, Reach = 9f, Seconds = 0.62f, Height = 3.4f, Damage = 46f, Stagger = 55f, Radius = 3.2f, LingerDamage = 5f, Stacks = 0 } });
            to.Add(new Upgrade { Id = CinderFan, Name = "Cinder Fan", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Upgrade.MeteorLeap }, X = 0f, Y = 6.5f, Icon = UpgradeIcons.Wave,
                Description = "Hold the spell button to throw fire in a wide fan, close in. What it touches burns two deep. 30 mana.",
                Form = SpellForms.Wave,
                Spell = new SpellSpec { Cost = 30f, Reach = 4.5f, Arc = 140f, Damage = 22f, Stagger = 20f, Stacks = 2 } });

            // Right bough: fire left on the ground.
            to.Add(new Upgrade { Id = Upgrade.FlameTrail, Name = "Flame Trail", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Heat }, X = 1.21f, Y = 2.4f, Icon = UpgradeIcons.Boot,
                Description = "The dodge leaves a line of fire behind it that burns what follows." });
            to.Add(new Upgrade { Id = DryTinder, Name = "Dry Tinder", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.FlameTrail }, X = 1.98f, Y = 3.42f, Icon = UpgradeIcons.Leaf,
                Description = "What you set burning burns for longer.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionSeconds, 0.25f) } });
            to.Add(new Upgrade { Id = Upgrade.FirebombFlask, Name = "Firebomb Flask", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.FlameTrail }, X = 1.61f, Y = 4.2f, Icon = UpgradeIcons.Flask,
                Description = "Hold the flask button to throw it. No healing: a great burst of fire where it lands." });
            to.Add(new Upgrade { Id = Wildfire, Name = "Wildfire", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.FirebombFlask }, X = 2.0f, Y = 6.0f, Icon = UpgradeIcons.Skull,
                Description = "Where an enemy dies the ground burns for a while.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 2.2f, Seconds = 4f, Damage = 5f, Interval = 0.5f, Affliction = AfflictionKind.Burn, Stacks = 1 } } });
        }
    }
}

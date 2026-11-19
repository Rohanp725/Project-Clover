using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Fire: never letting up. Every blow deepens a burn, and Heat pays the one who keeps hitting and is not hit.
    /// Three boughs from Cinder: on the left Answer (the perfect guard and the dodge turned into fire), in the middle
    /// Pressure (Heat and momentum), on the right Ruin (big spells, burning ground and the flask). The seven first
    /// marks are done by hand in <see cref="PlayerUpgrades"/> under their ids; the rest are data.
    /// </summary>
    public static class FireTree
    {
        public const string Cinder = "fire.cinder";
        // Pressure.
        public const string Stoked = "fire.stoked";
        public const string BankedCoals = "fire.banked";
        public const string CatchingFire = "fire.catch";
        public const string SearingEdge = "fire.searing";
        public const string SlowFuse = "fire.fuse";
        public const string DryTinder = "fire.tinder";
        public const string WildfireRush = "fire.rush";
        public const string WhiteHeat = "fire.whiteheat";
        // Answer.
        public const string StruckFlint = "fire.flint";
        public const string FurnaceBreath = "fire.breath";
        public const string WideHearth = "fire.hearth";
        public const string LongEmbers = "fire.embers";
        public const string HotFooting = "fire.footing";
        public const string Flashpoint = "fire.flash";
        public const string KilnStance = "fire.kiln";
        public const string OpenFurnace = "fire.furnace";
        // Ruin.
        public const string BroadWave = "fire.broadwave";
        public const string ShortBreath = "fire.shortbreath";
        public const string HotCoals = "fire.coals";
        public const string LongKindle = "fire.longkindle";
        public const string Sunspot = "fire.sunspot";
        public const string ScorchedEarth = "fire.scorched";
        // The god's gift.
        public const string FallingSun = "fire.sun";
        // Kept from the first slice; not in the design.
        public const string Kindling = "fire.kindling";
        public const string Wildfire = "fire.wildfire";
        public const string CinderFan = "fire.fan";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Fire;

            to.Add(new Upgrade { Id = Cinder, Name = "Cinder", Element = E, Kind = UpgradeKind.Root, X = 0f, Y = 1.2f, Icon = UpgradeIcons.Flame,
                Description = "Every blow of the weapon sets a burn, and each blow deepens it, up to five.",
                WeaponStacks = 1 });

            // Middle bough: Pressure. Heat and momentum.
            // BESPOKE (as built): Heat, done by hand in PlayerUpgrades. It was the root; it is now the foot of this bough.
            to.Add(new Upgrade { Id = Upgrade.Heat, Name = "Heat", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Cinder }, X = -0.02f, Y = 3.84f, Icon = UpgradeIcons.Fist,
                Description = "Blows landed without being struck build Heat, up to five. Each level adds damage. A blow taken puts it out. Kindle fills it at once." });
            to.Add(new Upgrade { Id = Stoked, Name = "Stoked", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.Heat }, X = -0.34f, Y = 5.1f, Icon = UpgradeIcons.Fist,
                Description = "Heat rises on fewer blows.",
                Stats = new[] { new StatMod(PlayerStat.HeatBlows, 1f) } });
            to.Add(new Upgrade { Id = BankedCoals, Name = "Banked Coals", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.Heat }, X = 0.76f, Y = 4.34f, Icon = UpgradeIcons.Hourglass,
                Description = "Heat cools more slowly between blows.",
                Stats = new[] { new StatMod(PlayerStat.HeatCooling, 0.5f) } });
            to.Add(new Upgrade { Id = CatchingFire, Name = "Catching Fire", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Heat }, X = 0.26f, Y = 6.99f, Icon = UpgradeIcons.Skull,
                Description = "A burning enemy that dies throws its burn onto everyone near it.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Burn, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1, CarriesDepth = true, Radius = 4f } } });
            // BESPOKE (as built): the Iaido Slash throws its arc of flame. Still to add: each enemy it burns counts as a blow for Heat.
            to.Add(new Upgrade { Id = Upgrade.BurningCrescent, Name = "Burning Crescent", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Heat }, X = 0.56f, Y = 5.43f, Icon = UpgradeIcons.Blade, Weapon = Weapons.KatanaName,
                Description = "The Iaido Slash throws a slow arc of flame that travels on after the cut." });
            to.Add(new Upgrade { Id = SearingEdge, Name = "Searing Edge", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { CatchingFire }, X = 1.26f, Y = 7.58f, Icon = UpgradeIcons.Flame,
                Description = "Burns bite harder.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionPower, 0.2f) } });
            // BESPOKE (built): DrillForm leaves the line burning and raises Heat a level for each enemy passed.
            to.Add(new Upgrade { Id = WildfireRush, Name = "Wildfire Rush", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { CatchingFire }, X = 1.34f, Y = 8.65f, Icon = UpgradeIcons.Arrow,
                Description = "You drive through the enemies ahead in a line of fire. Each one passed is set burning and raises your Heat.",
                Form = "drill",
                Spell = new SpellSpec { Cost = 35f, Reach = 7f, Radius = 1f, Count = 1, Damage = 14f, Stagger = 20f, Stacks = 2, Speed = 20f, Seconds = 2.6f, LingerDamage = 4f } });
            to.Add(new Upgrade { Id = SlowFuse, Name = "Slow Fuse", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { CatchingFire }, X = 0.24f, Y = 8.21f, Icon = UpgradeIcons.Hourglass,
                Description = "Burns last longer.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionSeconds, 0.5f) } });
            to.Add(new Upgrade { Id = DryTinder, Name = "Dry Tinder", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { CatchingFire }, X = -0.92f, Y = 8.24f, Icon = UpgradeIcons.Leaf,
                Description = "Your weapon cuts deeper into a burning enemy.",
                Stats = new[] { new StatMod(PlayerStat.WeaponDamage, 0.08f, AfflictionKind.Burn) } });
            to.Add(new Upgrade { Id = WhiteHeat, Name = "White Heat", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { WildfireRush }, X = 1.24f, Y = 10.26f, Icon = UpgradeIcons.Star,
                Description = "Heat rises to eight, and above five your blows come faster. Struck above five, you burn.",
                Stats = new[] { new StatMod(PlayerStat.HeatCap, 3f) } });

            // Left bough: Answer. The perfect guard and the dodge turned into fire.
            // BESPOKE (as built): the cone of fire, done by hand. The two stacks of Burn it now lays are data.
            to.Add(new Upgrade { Id = Upgrade.Backdraft, Name = "Backdraft", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Cinder }, X = -1.28f, Y = 3.47f, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard throws a cone of fire back at the one who struck.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 2 } } });
            to.Add(new Upgrade { Id = StruckFlint, Name = "Struck Flint", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.Backdraft }, X = -2.15f, Y = 3.99f, Icon = UpgradeIcons.Gem,
                Description = "A perfect guard gives more mana.",
                Stats = new[] { new StatMod(PlayerStat.ManaFromGuards, 0.4f) } });
            // BESPOKE (as built): the detonation, done by hand. It no longer lays the burn itself: Cinder does.
            to.Add(new Upgrade { Id = Upgrade.BurnAndDetonate, Name = "Burn and Detonate", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Backdraft }, X = -2.51f, Y = 5.3f, Icon = UpgradeIcons.Burst,
                Description = "A perfect guard against a burning enemy sets all of its burn off at once." });
            // BESPOKE (as built): the line of fire behind the dodge. It used to grow from Heat.
            to.Add(new Upgrade { Id = Upgrade.FlameTrail, Name = "Flame Trail", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Backdraft }, X = -1.27f, Y = 5.7f, Icon = UpgradeIcons.Boot,
                Description = "The dodge leaves a line of fire behind it that burns what follows." });
            to.Add(new Upgrade { Id = FurnaceBreath, Name = "Furnace Breath", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.Backdraft }, X = -1.69f, Y = 4.85f, Icon = UpgradeIcons.Wave,
                Description = "Backdraft breaks balance harder.",
                Stats = new[] { new StatMod(PlayerStat.BackdraftStagger, 0.55f) } });
            // BESPOKE (built): ParryForm answers Fire's caught blow with a pillar under the attacker and counts it as a perfect guard.
            to.Add(new Upgrade { Id = KilnStance, Name = "Kiln Stance", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Upgrade.BurnAndDetonate }, X = -1.96f, Y = 6.49f, Icon = UpgradeIcons.Tower,
                Description = "You stand ready in flame. The first blow that reaches you, red or not, is turned and answered with a pillar of fire.",
                Form = "parry",
                Spell = new SpellSpec { Cost = 30f, Seconds = 1.2f, Reach = 4f, Arc = 90f, Damage = 30f, Stagger = 50f, Stacks = 3 } });
            // UntilBlow: the first swing that lands within three seconds has it, and no other.
            to.Add(new Upgrade { Id = Flashpoint, Name = "Flashpoint", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.FlameTrail }, X = -0.88f, Y = 7.18f, Icon = UpgradeIcons.Eye,
                Description = "After a perfect dodge, your next blow within three seconds sets a burn three deep at once.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Boon, Stat = PlayerStat.AfflictionStacks, Amount = 2f, Seconds = 3f, UntilBlow = true } } });
            // Kept from the first slice; not in the design.
            to.Add(new Upgrade { Id = Kindling, Name = "Kindling", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.Backdraft }, X = -0.83f, Y = 4.31f, Icon = UpgradeIcons.Hourglass,
                Description = "The moment in which a guard is perfect lasts a little longer.",
                Stats = new[] { new StatMod(PlayerStat.PerfectGuardWindow, 0.15f) } });
            // Built by hand (PlayerUpgrades.OpenFurnace): every burning enemy within eight metres is set off where it stands and
            // keeps two stacks; a plain guard lets 60% through as grey where it let 35%.
            to.Add(new Upgrade { Id = OpenFurnace, Name = "Open Furnace", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { KilnStance }, X = -2.89f, Y = 7.88f, Icon = UpgradeIcons.Burst,
                Description = "A perfect guard sets off every burning enemy near you, not only the one who struck, and leaves each still alight. A plain guard lets more of the blow through.",
                Stats = new[] { new StatMod(PlayerStat.GuardGrey, 0.71f) } });
            to.Add(new Upgrade { Id = LongEmbers, Name = "Long Embers", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.FlameTrail }, X = -0.26f, Y = 6.22f, Icon = UpgradeIcons.Hourglass,
                Description = "Flame Trail burns longer on the ground.",
                Stats = new[] { new StatMod(PlayerStat.TrailSeconds, 0.54f) } });
            to.Add(new Upgrade { Id = WideHearth, Name = "Wide Hearth", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.BurnAndDetonate }, X = -2.89f, Y = 6.65f, Icon = UpgradeIcons.Ring,
                Description = "Backdraft and the detonation reach further.",
                Stats = new[] { new StatMod(PlayerStat.BackdraftReach, 0.25f) } });
            to.Add(new Upgrade { Id = HotFooting, Name = "Hot Footing", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.FlameTrail }, X = -1.8f, Y = 7.42f, Icon = UpgradeIcons.Boot,
                Description = "Flame Trail bites harder.",
                Stats = new[] { new StatMod(PlayerStat.TrailDamage, 0.5f) } });

            // Right bough: Ruin. Big spells, burning ground and the flask.
            // BESPOKE (as built): the thrown flask and its burst. The three stacks of Burn it now lays are data. It used to grow from Flame Trail.
            to.Add(new Upgrade { Id = Upgrade.FirebombFlask, Name = "Firebomb Flask", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Cinder }, X = 1.1f, Y = 3.46f, Icon = UpgradeIcons.Flask,
                Description = "Hold the flask button to throw it. No healing: a great burst of fire where it lands, flames left on the ground there, and three stacks of Burn on everything near.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.FlaskThrown, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 3, Radius = 2.6f } } });
            // The reach is the stat for every spell, the base one and whatever is worn.
            to.Add(new Upgrade { Id = BroadWave, Name = "Broad Wave", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.FirebombFlask }, X = 1.45f, Y = 5.02f, Icon = UpgradeIcons.Wave,
                Description = "Flame Wave reaches further.",
                Stats = new[] { new StatMod(PlayerStat.BaseSpellReach, 0.27f) } });
            to.Add(new Upgrade { Id = Upgrade.MeteorLeap, Name = "Meteor Leap", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Upgrade.FirebombFlask }, X = 2.37f, Y = 5.74f, Icon = UpgradeIcons.Comet,
                Description = "You leap out of the fight and come down on the enemy faced, leaving a burning crater. 45 mana.",
                Form = SpellForms.Leap,
                Spell = new SpellSpec { Cost = 50f, Reach = 9f, Seconds = 0.62f, Height = 3.4f, Damage = 46f, Stagger = 55f, Radius = 3.2f, LingerDamage = 5f, Stacks = 0 } });
            // The wait of this element's own base spell alone (SpellTimings.BaseCooldownShare), carried first or second.
            to.Add(new Upgrade { Id = ShortBreath, Name = "Short Breath", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.FirebombFlask }, X = 2.34f, Y = 4.73f, Icon = UpgradeIcons.Gem,
                Description = "Flame Wave waits a fifth less between casts.",
                Stats = new[] { new StatMod(PlayerStat.BaseSpellCooldown, -0.2f) } });
            // BESPOKE (built): SunspotForm. Light blows feed it, a heavy blow sets it off; left alone it bursts at half.
            to.Add(new Upgrade { Id = Sunspot, Name = "Sunspot", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Upgrade.MeteorLeap }, X = 2.46f, Y = 7.87f, Icon = UpgradeIcons.Target,
                Description = "You brand the enemy faced. Each blow you land feeds the brand, up to six. A heavy blow sets it off.",
                Form = SpellForms.Sunspot,
                Spell = new SpellSpec { Cost = 35f, Reach = 6f, Seconds = 7f, Count = 6, Radius = 3f, Damage = 12f, Stagger = 10f, Stacks = 1 } });
            // There is no number for spells on burning enemies alone: half the share, on every enemy.
            to.Add(new Upgrade { Id = HotCoals, Name = "Hot Coals", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.MeteorLeap }, X = 1.93f, Y = 6.84f, Icon = UpgradeIcons.Orb,
                Description = "Spells strike burning enemies harder.",
                Stats = new[] { new StatMod(PlayerStat.SpellDamage, 0.2f, AfflictionKind.Burn) } });
            to.Add(new Upgrade { Id = LongKindle, Name = "Long Kindle", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.MeteorLeap }, X = 2.93f, Y = 6.8f, Icon = UpgradeIcons.Hourglass,
                Description = "Kindle lasts longer.",
                Stats = new[] { new StatMod(PlayerStat.SurgeSeconds, 0.3f) } });
            // BESPOKE (built): Hazards.FireSecondsScale doubles fire left on the ground; StatWhen.InOwnFire carries the mana; PlayerUpgrades.SurgeHeld stops Kindle's clock.
            to.Add(new Upgrade { Id = ScorchedEarth, Name = "Scorched Earth", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { Sunspot }, X = 2.9f, Y = 9.29f, Icon = UpgradeIcons.Spike,
                Description = "Fire you leave on the ground burns twice as long. While you stand in it Kindle does not run down and your blows give more mana. Your spells cost more.",
                Stats = new[] { new StatMod(PlayerStat.ManaFromBlows, 0.5f, StatWhen.InOwnFire), new StatMod(PlayerStat.SpellCost, 0.2f) } });
            // Kept from the first slice; not in the design.
            to.Add(new Upgrade { Id = Wildfire, Name = "Wildfire", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.FirebombFlask }, X = 2.09f, Y = 3.81f, Icon = UpgradeIcons.Skull,
                Description = "Where an enemy dies the ground burns for a while.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 2.2f, Seconds = 4f, Damage = 5f, Interval = 0.5f, Affliction = AfflictionKind.Burn, Stacks = 1 } } });
            // Kept from the first slice; not in the design.
            to.Add(new Upgrade { Id = CinderFan, Name = "Cinder Fan", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Upgrade.MeteorLeap }, X = 1.14f, Y = 6.32f, Icon = UpgradeIcons.Wave,
                Description = "You throw fire in a wide fan, close in. What it touches burns two deep.",
                Form = SpellForms.Wave,
                Spell = new SpellSpec { Cost = 30f, Reach = 4.5f, Arc = 140f, Damage = 22f, Stagger = 20f, Stacks = 2 } });

            // BESPOKE (built): SunForm.
            to.Add(new Upgrade { Id = FallingSun, Name = "Falling Sun", Element = E, Kind = UpgradeKind.Signature, X = 0f, Y = 12f, Icon = UpgradeIcons.Crown,
                Description = "Light Kindle and the sun begins its road. At dawn it is rolled along the ground to the enemy you face, and the ground it crosses burns. At noon it is carried up, and every burn near you deepens by itself. When Kindle ends, or when you press the surge button again, it sets on the enemy below it, heavier for every burn it finds. A blow taken sends it under, to rise again.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.Sun, Spell = new SpellSpec { Cost = 0f, Damage = 60f, Stagger = 80f, Radius = 4f, Stacks = 2, LingerDamage = 5f } } } });
        }
    }
}

using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Ice: patience. Every blow slows the enemy until it stops, and one heavy blow breaks it. Three boughs from Chill:
    /// on the left Stillness (the guard and the wait), in the middle Shatter (freeze and break), on the right Winter
    /// (ice on the ground and the shell). Form ids are written as plain strings: see docs/design/FORMS.md.
    /// </summary>
    public static class IceTree
    {
        public const string Root = "ice.chill";
        public const string ColdRead = "ice.coldread";
        public const string PatientGuard = "ice.patient";
        public const string Braced = "ice.braced";
        public const string GatheringFrost = "ice.gather";
        public const string HeavySnow = "ice.snow";
        public const string SlowPulse = "ice.slowpulse";
        public const string HollowStatue = "ice.statue";
        public const string Unmoved = "ice.unmoved";
        public const string FlyingShards = "ice.shards";
        public const string DeepCold = "ice.deepcold";
        public const string LongWinter = "ice.longwinter";
        public const string ColdIron = "ice.coldiron";
        public const string WinterDraw = "ice.winterdraw";
        public const string Brittle = "ice.brittle";
        public const string HeldFast = "ice.heldfast";
        public const string SharpBreak = "ice.sharpbreak";
        public const string CleanBreak = "ice.cleanbreak";
        public const string HangingIce = "ice.hanging";
        public const string EightPoints = "ice.eight";
        public const string AllAtOnce = "ice.allatonce";
        public const string BlackIce = "ice.blackice";
        public const string ThinIce = "ice.thinice";
        public const string LongFrost = "ice.longfrost";
        public const string SplinterWard = "ice.splinter";
        public const string ThickGlaze = "ice.thick";
        public const string ClearIce = "ice.clearice";
        public const string BitterCold = "ice.bitter";
        public const string WinterWall = "ice.wall";
        public const string SecondSkin = "ice.skin";
        public const string Gift = "ice.stillworld";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Ice;

            to.Add(new Upgrade { Id = Root, Name = "Chill", Element = E, Kind = UpgradeKind.Root, X = 0f, Y = 1.2f, Icon = UpgradeIcons.Snowflake,
                Description = "Blows chill an enemy, up to five. Each slows it. At five it is Frozen, and a heavy blow shatters it.",
                WeaponStacks = 1 });

            // Left bough: Stillness, the guard and the wait.
            to.Add(new Upgrade { Id = ColdRead, Name = "Cold Read", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = -1.19f, Y = 2.66f, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard chills the one who struck, two deep.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 2 } } });
            to.Add(new Upgrade { Id = PatientGuard, Name = "Patient Guard", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { ColdRead }, X = -2.01f, Y = 3.54f, Icon = UpgradeIcons.Hourglass,
                Description = "The moment for a perfect guard is a little longer.",
                Stats = new[] { new StatMod(PlayerStat.PerfectGuardWindow, 0.17f) } });
            to.Add(new Upgrade { Id = Braced, Name = "Braced", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { ColdRead }, X = -0.91f, Y = 3.62f, Icon = UpgradeIcons.Shield,
                Description = "Guarding costs less stamina.",
                Stats = new[] { new StatMod(PlayerStat.GuardStamina, -0.167f) } });
            // BESPOKE: rings gather while the player neither strikes nor dodges (three at most, one every 0.8 s); the next
            // blow spends them for more damage and one more Chill each. Until then: a guard makes the next blows harder and colder.
            to.Add(new Upgrade { Id = GatheringFrost, Name = "Gathering Frost", Element = E, Kind = UpgradeKind.Major, Requires = new[] { ColdRead }, X = -2.03f, Y = 4.77f, Icon = UpgradeIcons.Ring,
                Description = "Hold your ground without striking and frost gathers at your feet, up to three rings. Your next blow spends them, harder and colder for each.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Guard, Do = UpgradeAction.Boon, Stat = PlayerStat.WeaponDamage, Amount = 0.2f, Seconds = 4f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Boon, Stat = PlayerStat.WeaponDamage, Amount = 0.4f, Seconds = 4f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Boon, Stat = PlayerStat.AfflictionStacks, Amount = 1f, Seconds = 4f },
                } });
            to.Add(new Upgrade { Id = HeavySnow, Name = "Heavy Snow", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { GatheringFrost }, X = -1.61f, Y = 5.88f, Icon = UpgradeIcons.Hammer,
                Description = "Heavy blows break balance harder.",
                Stats = new[] { new StatMod(PlayerStat.StaggerDealt, 0.15f, StatWhen.HeavyBlow) } });
            // NEAREST: less harm of every kind while the guard is up; the design lowers the grey share of a guarded blow and the harm of a guard break.
            to.Add(new Upgrade { Id = SlowPulse, Name = "Slow Pulse", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { GatheringFrost }, X = -2.62f, Y = 5.68f, Icon = UpgradeIcons.Heart,
                Description = "You take less harm while your guard is up.",
                Stats = new[] { new StatMod(PlayerStat.DamageTaken, -0.08f, StatWhen.Guarding) } });
            // NEEDS FORM: a statue that draws enemies, bursts when they strike it and bursts forward for the player's heavy blow. The wall is the nearest.
            to.Add(new Upgrade { Id = HollowStatue, Name = "Hollow Statue", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { GatheringFrost }, X = -2.37f, Y = 6.79f, Icon = UpgradeIcons.Tower,
                Description = "Hold the spell button to step back and leave a shape of ice where you stood. Struck, it bursts and chills. Strike it yourself with a heavy blow and it bursts forward, freezing. 30 mana.",
                Form = "wall",
                Spell = new SpellSpec { Cost = 30f, Reach = 1.5f, Arc = 1.5f, Delay = 0.1f, Seconds = 6f, Radius = 3.5f, Damage = 20f, Stagger = 30f, Stacks = 3 } });
            // BESPOKE: no flinch through a heavy blow and half damage taken in it. Until then: plain numbers, and StaminaCost raises the guard's price as well as the dodge's.
            to.Add(new Upgrade { Id = Unmoved, Name = "Unmoved", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { HollowStatue }, X = -3f, Y = 8.08f, Icon = UpgradeIcons.Wall,
                Description = "Nothing breaks your heavy blows, and a blow that lands on you while you swing does half. Your dodge is short and costs more.",
                Stats = new[]
                {
                    new StatMod(PlayerStat.DamageTaken, -0.15f),
                    new StatMod(PlayerStat.StaggerDealt, 0.2f),
                    new StatMod(PlayerStat.DodgeDistance, -0.3f),
                    new StatMod(PlayerStat.StaminaCost, 0.25f),
                } });

            // Middle bough: Shatter, freeze and break.
            to.Add(new Upgrade { Id = FlyingShards, Name = "Flying Shards", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = -0.21f, Y = 4.45f, Icon = UpgradeIcons.Burst,
                Description = "A shattered enemy throws its ice wide. Everyone it reaches is chilled two deep.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Shatter, Do = UpgradeAction.Burst, Damage = 14f, Stagger = 10f, Radius = 4.5f, Affliction = AfflictionKind.Chill, Stacks = 2 } } });
            to.Add(new Upgrade { Id = DeepCold, Name = "Deep Cold", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { FlyingShards }, X = -0.54f, Y = 5.66f, Icon = UpgradeIcons.Snowflake,
                Description = "Chill slows more.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionPower, 0.15f) } });
            to.Add(new Upgrade { Id = LongWinter, Name = "Long Winter", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { FlyingShards }, X = 0.41f, Y = 5.43f, Icon = UpgradeIcons.Hourglass,
                Description = "Chill lasts longer.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionSeconds, 0.5f) } });
            to.Add(new Upgrade { Id = ColdIron, Name = "Cold Iron", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { FlyingShards }, X = -1.09f, Y = 4.83f, Icon = UpgradeIcons.Hammer,
                Description = "Heavy blows chill two deep.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 1 } } });
            // NEAREST: the lane is thrown when the slash lands on any chilled enemy; the design asks for a Frozen one, shattered.
            to.Add(new Upgrade { Id = WinterDraw, Name = "Winter Draw", Element = E, Kind = UpgradeKind.Major, Requires = new[] { FlyingShards }, X = 1.4f, Y = 5.57f, Icon = UpgradeIcons.Blade, Weapon = Weapons.KatanaName,
                Description = "The Iaido Slash shatters a Frozen enemy and carries on through everyone behind it.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SkillHit, Do = UpgradeAction.Projectile, Needs = AfflictionKind.Chill, Flight = Flight.Straight, Damage = 18f, Stagger = 20f, Radius = 1f, Speed = 16f, Seconds = 0.5f, Affliction = AfflictionKind.Chill, Stacks = 2, Cooldown = 1f } } });
            // NEAREST: an enemy cannot be made brittle, so the player strikes harder for five seconds after a shatter.
            to.Add(new Upgrade { Id = Brittle, Name = "Brittle", Element = E, Kind = UpgradeKind.Major, Requires = new[] { FlyingShards }, X = 0.12f, Y = 6.6f, Icon = UpgradeIcons.Spike,
                Description = "An enemy that thaws or is shattered stays brittle for a while. Blows on it cut deeper and break its balance sooner.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Shatter, Do = UpgradeAction.Boon, Stat = PlayerStat.WeaponDamage, Amount = 0.2f, Seconds = 5f }, new UpgradeEffect { On = UpgradeTrigger.Shatter, Do = UpgradeAction.Boon, Stat = PlayerStat.StaggerDealt, Amount = 0.2f, Seconds = 5f } } });
            // NEAREST: no stat lengthens the freeze itself; this lengthens the Chill.
            to.Add(new Upgrade { Id = HeldFast, Name = "Held Fast", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Brittle }, X = 0.81f, Y = 7.7f, Icon = UpgradeIcons.Hourglass,
                Description = "Frozen enemies stay Frozen longer.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionSeconds, 0.15f) } });
            // AfflictionPower scales the shatter (AfflictionSet.Shatter); it deepens the slow a little too.
            to.Add(new Upgrade { Id = SharpBreak, Name = "Sharp Break", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Brittle }, X = -0.34f, Y = 7.83f, Icon = UpgradeIcons.Hammer,
                Description = "Shattering strikes harder.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionPower, 0.2f) } });
            // NEAREST: no kill outright and no reading of the enemy's health; a heavy blow on a chilled enemy bites again.
            to.Add(new Upgrade { Id = CleanBreak, Name = "Clean Break", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Brittle }, X = 1.66f, Y = 6.9f, Icon = UpgradeIcons.Skull,
                Description = "Shatter an enemy with little life left and nothing is left of it.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Burst, Needs = AfflictionKind.Chill, Damage = 18f, Stagger = 10f, Radius = 1.2f, Cooldown = 1.5f } } });
            // The sword form carries the blade; the triple shatter of a Frozen target is not in it yet.
            to.Add(new Upgrade { Id = HangingIce, Name = "Hanging Ice", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Brittle }, X = -1.13f, Y = 6.95f, Icon = UpgradeIcons.Comet,
                Description = "Hold the spell button to hang a great blade of ice over the enemy faced. It falls in a few moments. If the enemy is Frozen when it lands, it is shattered three times over. 40 mana.",
                Form = "sword",
                Spell = new SpellSpec { Cost = 40f, Reach = 7f, Delay = 3f, Radius = 2f, Damage = 30f, Stagger = 40f, Stacks = 3, Seconds = 2f, LingerDamage = 2f } });
            to.Add(new Upgrade { Id = EightPoints, Name = "Eight Points", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { HangingIce }, X = -1.58f, Y = 8.3f, Icon = UpgradeIcons.Target,
                Description = "Hold the spell button to set eight shards in a ring round the enemy faced. They close on it together. 45 mana.",
                Form = "bullets",
                Spell = new SpellSpec { Cost = 45f, Reach = 6f, Count = 8, Radius = 3.5f, Delay = 1.2f, Damage = 5f, Stagger = 4f, Stacks = 1 } });
            // BESPOKE: the shatter itself tripled and Chill's slowing removed. Until then: a heavy blow on a chilled
            // enemy bursts hard, and the price is a Chill that fades sooner.
            to.Add(new Upgrade { Id = AllAtOnce, Name = "All at Once", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { EightPoints }, X = -0.55f, Y = 9.23f, Icon = UpgradeIcons.Star,
                Description = "Shattering strikes three times as hard. Chill no longer slows anyone.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionSeconds, -0.3f) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Burst, Needs = AfflictionKind.Chill, Damage = 60f, Stagger = 40f, Radius = 3f, Cooldown = 4f } } });

            // Right bough: Winter, ice on the ground and the shell.
            // NEAREST: the sheet is laid for any spell cast, worn spells included, and does not lengthen pushes.
            to.Add(new Upgrade { Id = BlackIce, Name = "Black Ice", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = 1.27f, Y = 1.55f, Icon = UpgradeIcons.Wave,
                Description = "Hoarfrost leaves a sheet of ice on the ground. Enemies standing on it grow colder and are pushed further.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, Do = UpgradeAction.Hazard, Shape = HazardShape.Line, Count = 4, Reach = 4.5f, Radius = 1.1f, Seconds = 4f, Interval = 1f, Damage = 1f, Affliction = AfflictionKind.Chill, Stacks = 1, Cooldown = 1f } } });
            to.Add(new Upgrade { Id = ThinIce, Name = "Thin Ice", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { BlackIce }, X = 0.39f, Y = 2.36f, Icon = UpgradeIcons.Gem,
                Description = "Hoarfrost costs less mana.",
                Stats = new[] { new StatMod(PlayerStat.BaseSpellCost, -0.2f) } });
            to.Add(new Upgrade { Id = LongFrost, Name = "Long Frost", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { BlackIce }, X = 1.71f, Y = 2.46f, Icon = UpgradeIcons.Arrow,
                Description = "Hoarfrost reaches further.",
                Stats = new[] { new StatMod(PlayerStat.BaseSpellReach, 0.33f) } });
            // NEAREST: the burst and a perfect guard's mana come when the ward is spent; the blow does not yet count as a perfect guard for other marks.
            to.Add(new Upgrade { Id = SplinterWard, Name = "Splinter Ward", Element = E, Kind = UpgradeKind.Major, Requires = new[] { BlackIce }, X = 1.16f, Y = 3.27f, Icon = UpgradeIcons.Shield,
                Description = "When Glaze breaks, it bursts on the one who broke it. The blow counts as a perfect guard.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.WardSpent, Do = UpgradeAction.Burst, At = EffectAt.Self, Damage = 12f, Stagger = 30f, Radius = 3.5f, Affliction = AfflictionKind.Chill, Stacks = 3 }, new UpgradeEffect { On = UpgradeTrigger.WardSpent, Do = UpgradeAction.Mana, Amount = 15f } } });
            to.Add(new Upgrade { Id = ThickGlaze, Name = "Thick Glaze", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { SplinterWard }, X = 2.04f, Y = 3.64f, Icon = UpgradeIcons.Hourglass,
                Description = "Glaze lasts longer.",
                Stats = new[] { new StatMod(PlayerStat.SurgeSeconds, 0.5f) } });
            to.Add(new Upgrade { Id = ClearIce, Name = "Clear Ice", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { SplinterWard }, X = 1.08f, Y = 4.46f, Icon = UpgradeIcons.Gem,
                Description = "Glaze costs less mana.",
                Stats = new[] { new StatMod(PlayerStat.SurgeCost, -0.2f) } });
            to.Add(new Upgrade { Id = BitterCold, Name = "Bitter Cold", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { SplinterWard }, X = 0.27f, Y = 3.61f, Icon = UpgradeIcons.Orb,
                Description = "Spells strike chilled enemies harder.",
                Stats = new[] { new StatMod(PlayerStat.SpellDamage, 0.2f, AfflictionKind.Chill) } });
            to.Add(new Upgrade { Id = WinterWall, Name = "Winter Wall", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { SplinterWard }, X = 2.32f, Y = 4.66f, Icon = UpgradeIcons.Wall,
                Description = "Hold the spell button to raise a wall of ice before you. It stops blows and chills what touches it. Your heavy blow bursts it forward. 35 mana.",
                Form = "wall",
                Spell = new SpellSpec { Cost = 35f, Reach = 2f, Arc = 5f, Delay = 0.2f, Seconds = 6f, Radius = 4f, Damage = 20f, Stagger = 30f, Stacks = 3 } });
            // BESPOKE: the shell regrows by itself and the surge button breaks it by choice. Until then: a ward every
            // twelve seconds of a fight (up to the three that can be held), for less health.
            to.Add(new Upgrade { Id = SecondSkin, Name = "Second Skin", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { WinterWall }, X = 2.69f, Y = 6f, Icon = UpgradeIcons.Orb,
                Description = "Glaze returns by itself a while after it breaks, at no cost. The surge button breaks it by choice. You have less health.",
                Stats = new[] { new StatMod(PlayerStat.Health, -25f) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.EverySeconds, Every = 12f, Do = UpgradeAction.Ward, Count = 1 } } });

            // BESPOKE: pressed again with the shell standing, it breaks outward as a ring that freezes all it crosses,
            // and each shatter runs on to the next Frozen enemy. Until then: lighting the surge freezes everything near.
            to.Add(new Upgrade { Id = Gift, Name = "The World Holds Still", Element = E, Kind = UpgradeKind.Signature, X = 0f, Y = 12f, Icon = UpgradeIcons.Crown,
                Description = "With Glaze standing, press the surge button again to break it outward. Everything it crosses is Frozen where it stands. Shatter one and its ice shatters the next.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Afflict, At = EffectAt.Self, Radius = 8f, Affliction = AfflictionKind.Chill, Stacks = 5 },
                    new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.SlowTime, Seconds = 1.5f, Amount = 0.2f },
                } });
        }
    }
}

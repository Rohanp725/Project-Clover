using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Water: it does not stop. Guarding runs into striking and back again, and the flask is more than healing. Three
    /// boughs from Soaked: on the left Tide (pulling and placing), in the middle Current (the rhythm of guard and
    /// blow), on the right Vessel (the flask). Form ids are written as plain strings: see docs/design/FORMS.md.
    /// </summary>
    public static class WaterTree
    {
        public const string Root = "water.soak";
        public const string EbbAndFlood = "water.ebbflood";
        public const string EasyWater = "water.easy";
        public const string Wading = "water.wading";
        public const string FlowingGuard = "water.flowing";
        public const string GreyTide = "water.greytide";
        public const string LongFlood = "water.longflood";
        public const string UnbrokenChain = "water.unbroken";
        public const string MoonOnWater = "water.moon";
        public const string RiverWithoutEnd = "water.river";
        public const string Undertow = "water.undertow";
        public const string LongRiptide = "water.longpull";
        public const string LowWater = "water.lowwater";
        public const string RiverSerpent = "water.serpent";
        public const string HeavyWater = "water.heavy";
        public const string SoakedThrough = "water.soakedthrough";
        public const string WeightOfWater = "water.weight";
        public const string Whirlpool = "water.whirlpool";
        public const string StandingRain = "water.rain";
        public const string SpringTide = "water.spring";
        public const string SpilledWater = "water.spilled";
        public const string ClearWater = "water.clear";
        public const string EarnedWater = "water.earned";
        public const string BrokenGourd = "water.gourd";
        public const string StillPool = "water.stillpool";
        public const string CatchTheRain = "water.catch";
        public const string SecondGourd = "water.second";
        public const string LongDraught = "water.longdraught";
        public const string PouredBlade = "water.poured";
        public const string DeepWell = "water.well";
        public const string Gift = "water.sea";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Water;

            to.Add(new Upgrade { Id = Root, Name = "Soaked", Element = E, Kind = UpgradeKind.Root, X = 0f, Y = 1.2f, Icon = UpgradeIcons.Drop,
                Description = "Blows leave an enemy Soaked. Spells strike it harder, and Static or Chill laid on it spreads to others who are Soaked.",
                WeaponStacks = 1 });

            // Left bough: Tide, pulling and placing.
            // NEAREST: the pull takes everyone near, Soaked or not.
            to.Add(new Upgrade { Id = Undertow, Name = "Undertow", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = -1.03f, Y = 3.13f, Icon = UpgradeIcons.Arrow,
                Description = "A heavy blow drags every enemy near a step toward you.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Pull, At = EffectAt.Self, Radius = 6f, Amount = 1.5f, Cooldown = 0.5f } } });
            to.Add(new Upgrade { Id = LongRiptide, Name = "Long Riptide", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Undertow }, X = -2.03f, Y = 4.47f, Icon = UpgradeIcons.Arrow,
                Description = "Riptide reaches further.",
                Stats = new[] { new StatMod(PlayerStat.BaseSpellReach, 0.3f) } });
            to.Add(new Upgrade { Id = LowWater, Name = "Low Water", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Undertow }, X = -0.88f, Y = 4.24f, Icon = UpgradeIcons.Gem,
                Description = "Riptide costs less mana.",
                Stats = new[] { new StatMod(PlayerStat.BaseSpellCost, -0.25f) } });
            // The dragon form carries what it meets away along its line; the design's serpent comes back and brings the first enemy to the player's feet.
            to.Add(new Upgrade { Id = RiverSerpent, Name = "River Serpent", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Undertow }, X = -2f, Y = 3.54f, Icon = UpgradeIcons.Claw,
                Description = "Hold the spell button to send a serpent of water down a lane ahead. It carries what it meets along a little, and its jaws close at the end and leave all it struck Soaked. 30 mana.",
                Form = "dragon",
                Spell = new SpellSpec { Cost = 30f, Reach = 8f, Radius = 1.2f, Damage = 12f, Stagger = 30f, Stacks = 1, Speed = 16f } });
            // NEAREST: an enemy's recovery cannot be changed, so blows on a Soaked enemy break its balance sooner for a while.
            to.Add(new Upgrade { Id = HeavyWater, Name = "Heavy Water", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Undertow }, X = -1.59f, Y = 5.29f, Icon = UpgradeIcons.Hammer,
                Description = "Striking a Soaked enemy makes your weapon break balance a fifth sooner for three seconds.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictedHit, Do = UpgradeAction.Boon, Needs = AfflictionKind.Soaked, Stat = PlayerStat.StaggerDealt, Amount = 0.2f, Seconds = 3f, Cooldown = 0.5f } } });
            to.Add(new Upgrade { Id = SoakedThrough, Name = "Soaked Through", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { HeavyWater }, X = -1.69f, Y = 6.59f, Icon = UpgradeIcons.Hourglass,
                Description = "Soaked lasts longer.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionSeconds, 0.5f) } });
            to.Add(new Upgrade { Id = WeightOfWater, Name = "Weight of Water", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { HeavyWater }, X = -2.63f, Y = 5.79f, Icon = UpgradeIcons.Drop,
                Description = "Spells strike Soaked enemies harder still.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionPower, 0.3f) } });
            // The field form bites and soaks; it does not yet draw enemies to its middle.
            to.Add(new Upgrade { Id = Whirlpool, Name = "Whirlpool", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { HeavyWater }, X = -2.85f, Y = 6.95f, Icon = UpgradeIcons.Spiral,
                Description = "Hold the spell button to open a whirlpool where the enemy faced stands. For five seconds it bites everything standing in it and keeps it Soaked. 35 mana.",
                Form = "field",
                Spell = new SpellSpec { Cost = 35f, Reach = 6f, Radius = 4f, Seconds = 5f, Interval = 0.5f, LingerDamage = 3f, Stacks = 1 } });
            // The rain form soaks what its drops strike; the extra mana from blows inside it is not in it yet.
            to.Add(new Upgrade { Id = StandingRain, Name = "Standing Rain", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Whirlpool }, X = -1.44f, Y = 7.77f, Icon = UpgradeIcons.Drop,
                Description = "Hold the spell button to call rain over the ground you stand on for ten seconds. Its drops strike enemies in it and leave them Soaked. 40 mana.",
                Form = "rain",
                Spell = new SpellSpec { Cost = 40f, Reach = 0.5f, Radius = 7f, Seconds = 10f, Interval = 0.6f, Damage = 3f, Stagger = 0f, Stacks = 1 } });
            // Built by hand (PlayerUpgrades.TideRises): every spell needs its usual cost and takes all the mana held; its damage grows by
            // mana held over cost, to two and a half times, and its reach by the square root of that.
            to.Add(new Upgrade { Id = SpringTide, Name = "Spring Tide", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { StandingRain }, X = -2.72f, Y = 8.45f, Icon = UpgradeIcons.Gem,
                Description = "Every spell spends all the mana you hold, and is as great as what it spends.",
                Stats = new StatMod[0] });

            // Middle bough: Current, the rhythm of guard and blow.
            // NEAREST: the Flood is more damage and one splash on the attacker (no longer reach, no splash on each blow); the Ebb is as designed.
            to.Add(new Upgrade { Id = EbbAndFlood, Name = "Ebb and Flood", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = 0.15f, Y = 3.77f, Icon = UpgradeIcons.Wave,
                Description = "A perfect guard brings the Flood: a splash soaks the attacker and those beside it, and for five seconds your blows strike 15% harder. A heavy blow that lands brings the Ebb: for four seconds your guard and dodge cost half the stamina and the moment of your perfect guard is longer.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Boon, Stat = PlayerStat.WeaponDamage, Amount = 0.15f, Seconds = 5f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Burst, Damage = 6f, Stagger = 8f, Radius = 2.5f, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Boon, Stat = PlayerStat.StaminaCost, Amount = -0.5f, Seconds = 4f },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Boon, Stat = PlayerStat.PerfectGuardWindow, Amount = 0.22f, Seconds = 4f },
                } });
            to.Add(new Upgrade { Id = EasyWater, Name = "Easy Water", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { EbbAndFlood }, X = -0.24f, Y = 4.92f, Icon = UpgradeIcons.Wing,
                Description = "Your blows cost less stamina.",
                Stats = new[] { new StatMod(PlayerStat.AttackStamina, -0.1f) } });
            to.Add(new Upgrade { Id = Wading, Name = "Wading", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { EbbAndFlood }, X = 0.92f, Y = 4.73f, Icon = UpgradeIcons.Boot,
                Description = "You move faster with your guard up.",
                Stats = new[] { new StatMod(PlayerStat.MoveSpeed, 0.33f, StatWhen.Guarding) } });
            // NEAREST: the weapon's moves cannot be changed, so a perfect guard makes the next blows quick and heavy on balance.
            to.Add(new Upgrade { Id = FlowingGuard, Name = "Flowing Guard", Element = E, Kind = UpgradeKind.Major, Requires = new[] { EbbAndFlood }, X = 0.26f, Y = 5.85f, Icon = UpgradeIcons.Blade,
                Description = "After a perfect guard, for two seconds you swing 30% faster and your blows break balance 30% sooner.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Boon, Stat = PlayerStat.SwingSpeed, Amount = 0.3f, Seconds = 2f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Boon, Stat = PlayerStat.StaggerDealt, Amount = 0.3f, Seconds = 2f },
                } });
            // NEAREST: grey health fades more slowly, which leaves longer to win it back.
            to.Add(new Upgrade { Id = GreyTide, Name = "Grey Tide", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { FlowingGuard }, X = -0.12f, Y = 7.11f, Icon = UpgradeIcons.Heart,
                Description = "Grey health fades more slowly.",
                Stats = new[] { new StatMod(PlayerStat.GreyKept, 0.3f) } });
            // NEAREST: no stat lengthens a boon; a longer perfect guard keeps the rhythm going instead.
            to.Add(new Upgrade { Id = LongFlood, Name = "Long Flood", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { FlowingGuard }, X = -0.79f, Y = 6.17f, Icon = UpgradeIcons.Hourglass,
                Description = "The moment of your perfect guard is longer.",
                Stats = new[] { new StatMod(PlayerStat.PerfectGuardWindow, 0.1f) } });
            // BESPOKE: the chain's place is remembered for 1.5 s through a dodge, a guard or a perfect guard. Until then: each of them quickens the next blows.
            to.Add(new Upgrade { Id = UnbrokenChain, Name = "Unbroken Chain", Element = E, Kind = UpgradeKind.Major, Requires = new[] { FlowingGuard }, X = 1.03f, Y = 7.37f, Icon = UpgradeIcons.Chain,
                Description = "A dodge, a guard or a perfect guard quickens your swings by a fifth for a moment.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, Do = UpgradeAction.Boon, Stat = PlayerStat.SwingSpeed, Amount = 0.2f, Seconds = 1.5f },
                    new UpgradeEffect { On = UpgradeTrigger.Guard, Do = UpgradeAction.Boon, Stat = PlayerStat.SwingSpeed, Amount = 0.2f, Seconds = 1.5f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Boon, Stat = PlayerStat.SwingSpeed, Amount = 0.2f, Seconds = 1.5f },
                } });
            // The parry form turns one blow and answers; the design's stance turns every blow for 10 mana each and stores up to five for the answer.
            to.Add(new Upgrade { Id = MoonOnWater, Name = "Moon on Water", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { UnbrokenChain }, X = 1.49f, Y = 8.76f, Icon = UpgradeIcons.Ring,
                Description = "Hold the spell button to stand open for up to three seconds. The first blow that comes is turned and answered with one wide cut that leaves those it strikes Soaked. If no blow comes, the mana is lost. 10 mana.",
                Form = "parry",
                Spell = new SpellSpec { Cost = 10f, Seconds = 3f, Reach = 4f, Arc = 200f, Damage = 30f, Stagger = 45f, Stacks = 1 } });
            // BESPOKE: the light chain's last blow runs into its first, quicker each time round. Until then: a quicker, softer weapon.
            to.Add(new Upgrade { Id = RiverWithoutEnd, Name = "River Without End", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { MoonOnWater }, X = 1.51f, Y = 10.18f, Icon = UpgradeIcons.Wave,
                Description = "You swing 15% faster. Each blow strikes 12% softer.",
                Stats = new[]
                {
                    new StatMod(PlayerStat.SwingSpeed, 0.15f),
                    new StatMod(PlayerStat.WeaponDamage, -0.12f),
                } });

            // Right bough: Vessel, the flask.
            to.Add(new Upgrade { Id = SpilledWater, Name = "Spilled Water", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = 1.45f, Y = 1.85f, Icon = UpgradeIcons.Ring,
                Description = "Drinking a flask throws a ring of water that pushes back and soaks everyone near.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.FlaskDrunk, Do = UpgradeAction.Burst, At = EffectAt.Self, Damage = 1f, Stagger = 15f, Radius = 3.5f, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.FlaskDrunk, Do = UpgradeAction.Push, At = EffectAt.Self, Radius = 3.5f, Amount = 2.5f },
                } });
            to.Add(new Upgrade { Id = ClearWater, Name = "Clear Water", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { SpilledWater }, X = 1.81f, Y = 2.89f, Icon = UpgradeIcons.Plus,
                Description = "Flasks mend more.",
                Stats = new[] { new StatMod(PlayerStat.FlaskHeal, 0.2f) } });
            to.Add(new Upgrade { Id = EarnedWater, Name = "Earned Water", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { SpilledWater }, X = 0.55f, Y = 2.55f, Icon = UpgradeIcons.Flask,
                Description = "An empty gourd is won back with fewer blows.",
                Stats = new[] { new StatMod(PlayerStat.FlaskEarn, 0.26f) } });
            // NEAREST: the pool soaks; it cannot mend the player who stands in it, so the throw mends a part at once.
            to.Add(new Upgrade { Id = BrokenGourd, Name = "Broken Gourd", Element = E, Kind = UpgradeKind.Major, Requires = new[] { SpilledWater }, X = 1.35f, Y = 3.79f, Icon = UpgradeIcons.Flask,
                Description = "Hold the flask button to throw it. Where it breaks it strikes and soaks everyone near and leaves a pool that keeps what stands in it Soaked for six seconds. The throw mends you 20 at once.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.FlaskThrown, Do = UpgradeAction.Burst, Damage = 18f, Stagger = 20f, Radius = 2.6f, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.FlaskThrown, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 1.5f, Seconds = 6f, Interval = 1f, Damage = 1f, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.FlaskThrown, Do = UpgradeAction.Heal, Amount = 20f },
                } });
            // GroundSeconds: ground laid by effects and left by spells, the gourd's pool among them.
            to.Add(new Upgrade { Id = StillPool, Name = "Still Pool", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { BrokenGourd }, X = 2.25f, Y = 4.41f, Icon = UpgradeIcons.Hourglass,
                Description = "Pools and other ground you leave last longer.",
                Stats = new[] { new StatMod(PlayerStat.GroundSeconds, 0.4f) } });
            to.Add(new Upgrade { Id = CatchTheRain, Name = "Catch the Rain", Element = E, Kind = UpgradeKind.Major, Requires = new[] { BrokenGourd }, X = 2.34f, Y = 5.57f, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard fills a little of an empty gourd.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.FlaskFill, Amount = 0.29f } } });
            to.Add(new Upgrade { Id = SecondGourd, Name = "Second Gourd", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { CatchTheRain }, X = 2.02f, Y = 6.71f, Icon = UpgradeIcons.Flask,
                Description = "You carry one more flask.",
                Stats = new[] { new StatMod(PlayerStat.FlaskCount, 1f) } });
            to.Add(new Upgrade { Id = LongDraught, Name = "Long Draught", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { CatchTheRain }, X = 1.22f, Y = 5.88f, Icon = UpgradeIcons.Hourglass,
                Description = "Wellspring lasts longer.",
                Stats = new[] { new StatMod(PlayerStat.SurgeSeconds, 0.33f) } });
            // NEEDS FORM: a timed boon paid with a flask (longer reach, each blow mends, the chain's last blow throws a wave). The clone is the nearest: it adds to every blow for as long.
            to.Add(new Upgrade { Id = PouredBlade, Name = "Poured Blade", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { CatchTheRain }, X = 3.03f, Y = 7.17f, Icon = UpgradeIcons.Blade,
                Description = "Hold the spell button to call a copy of yourself in water to your side for twelve seconds. It lands each of your blows again a beat late, at 30% of its strength. 20 mana.",
                Form = "clone",
                Spell = new SpellSpec { Cost = 20f, Seconds = 12f, Damage = 30f, Stacks = 1 } });
            // Built by hand (PlayerUpgrades.OnFlaskDrunk and Update): a flask adds 70 to a well of 140 at most, kept as the mark's count;
            // the well pays 6 health a second while a blow has landed in the last three seconds and health is not full.
            to.Add(new Upgrade { Id = DeepWell, Name = "Deep Well", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { PouredBlade }, X = 3.05f, Y = 8.58f, Icon = UpgradeIcons.Heart,
                Description = "You carry one more flask. A flask no longer mends you at once: it fills a well that mends you only while you are landing blows.",
                Tally = new TallySpec { Name = "Well", Cap = 140 },
                Stats = new[] { new StatMod(PlayerStat.FlaskCount, 1f) } });

            // BESPOKE (built): SeaForm. The surge button is pressed again, not held.
            to.Add(new Upgrade { Id = Gift, Name = "The Sea Rises", Element = E, Kind = UpgradeKind.Signature, X = 0f, Y = 12f, Icon = UpgradeIcons.Crown,
                Description = "While Wellspring lasts, press the surge button again to flood the ground. One flask. For six seconds everything in the flood is Soaked and your blows run out across the water as waves. Then the sea draws back to you and breaks where you face.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.Sea, Spell = new SpellSpec { Cost = 0f, Radius = 9f, Seconds = 6f, Damage = 55f, Stagger = 90f, Reach = 9f, Arc = 120f, Count = 40, Stacks = 1 } } } });
        }
    }
}

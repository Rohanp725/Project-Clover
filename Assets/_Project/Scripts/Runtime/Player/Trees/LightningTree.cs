using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Lightning: speed and the single right moment. Quick cuts load an enemy with Static and one heavy blow spends it.
    /// Three boughs from Static: on the left Charge (perfect guards banked and spent), in the middle Thunder (bolts,
    /// the rod and Overdrive), on the right Flicker (the dodge). The seven first marks and the god's Storm Step are
    /// done by hand in <see cref="PlayerUpgrades"/> under their ids; the rest are data.
    /// </summary>
    public static class LightningTree
    {
        // Flicker.
        public const string BitingStep = "lightning.bitingstep";
        public const string ForkedStep = "lightning.forked";
        public const string QuickReturn = "lightning.return";
        public const string BrightEcho = "lightning.echo";
        public const string ChargedStep = "lightning.chargedstep";
        public const string SevenFlashes = "lightning.flashes";
        public const string NeverThere = "lightning.neverthere";
        // Charge.
        public const string HeavyAir = "lightning.heavyair";
        public const string LongArc = "lightning.longarc";
        public const string DeepReserve = "lightning.reserve";
        public const string HardCharge = "lightning.hardcharge";
        public const string KeenEdge = "lightning.keen";
        public const string OpenSeam = "lightning.seam";
        public const string FullDischarge = "lightning.fulldischarge";
        public const string WaitingThunder = "lightning.waiting";
        public const string SingleStroke = "lightning.stroke";
        // Thunder.
        public const string Conductor = "lightning.conductor";
        public const string QuickClap = "lightning.quickclap";
        public const string ThirdLeap = "lightning.thirdleap";
        public const string LiveWire = "lightning.wire";
        public const string LongOverdrive = "lightning.longdrive";
        public const string RunningCurrent = "lightning.current";
        public const string Lodestone = "lightning.lodestone";
        // Kept from the first slice; not in the design.
        public const string QuickHands = "lightning.quick";
        public const string Capacitor = "lightning.capacitor";
        public const string Grounding = "lightning.grounding";
        public const string Slipstream = "lightning.slipstream";
        public const string ChainLance = "lightning.lance";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Lightning;

            // BESPOKE (as built): light blows build Static and a heavy blow discharges it, done by hand. No WeaponStacks: that would lay it twice.
            to.Add(new Upgrade { Id = Upgrade.Static, Name = "Static", Element = E, Kind = UpgradeKind.Root, X = 0f, Y = 1.2f, Icon = UpgradeIcons.Bolt,
                Description = "Blows build Static on an enemy, up to five. A heavy blow or a finisher discharges it, and it arcs to everyone near." });

            // Left bough: Charge. Bank it and spend it.
            // BESPOKE (as built). Still to add: the charged blow is critical.
            to.Add(new Upgrade { Id = Upgrade.StoredCharge, Name = "Stored Charge", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Static }, X = -0.98f, Y = 2.91f, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard banks a charge. The next blow spends it for double damage and stagger." });
            to.Add(new Upgrade { Id = HeavyAir, Name = "Heavy Air", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.StoredCharge }, X = -1.95f, Y = 3.34f, Icon = UpgradeIcons.Hourglass,
                Description = "Static stays longer on an enemy.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionSeconds, 0.5f) } });
            to.Add(new Upgrade { Id = DeepReserve, Name = "Deep Reserve", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.StoredCharge }, X = -1.39f, Y = 4.31f, Icon = UpgradeIcons.Gem,
                // BESPOKE: three charges banked, shown as pips, each blow spending one. Until then a perfect guard also leaves
                // every blow harder for a few seconds, so the blows after the charged one still carry something.
                Description = "Perfect guards bank up to three charges. Each blow spends one.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Boon, Stat = PlayerStat.WeaponDamage, Amount = 0.3f, Seconds = 4f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Boon, Stat = PlayerStat.StaggerDealt, Amount = 0.3f, Seconds = 4f },
                } });
            // BESPOKE (as built): the cuts that land when the blade is home. It keeps its foot on Stored Charge and moves aside for Deep Reserve.
            to.Add(new Upgrade { Id = Upgrade.DelayedCuts, Name = "Delayed Cuts", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.StoredCharge }, X = -0.77f, Y = 5.19f, Icon = UpgradeIcons.Blade, Weapon = Weapons.KatanaName,
                Description = "The Iaido Slash passes through. Its cuts land a moment later, all at once, when the blade is home." });
            to.Add(new Upgrade { Id = LongArc, Name = "Long Arc", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.StoredCharge }, X = 0.04f, Y = 3.24f, Icon = UpgradeIcons.Chain,
                Description = "A discharge arcs further.",
                Stats = new[] { new StatMod(PlayerStat.DischargeReach, 0.375f) } });
            // There is no trigger for "an affliction at full depth is set off" and nothing holds an enemy still: this answers
            // Static reaching five, with mana and a jolt to its balance.
            to.Add(new Upgrade { Id = FullDischarge, Name = "Full Discharge", Element = E, Kind = UpgradeKind.Major, Requires = new[] { DeepReserve }, X = -1.21f, Y = 6.41f, Icon = UpgradeIcons.Burst,
                Description = "A discharge of five Static holds its enemy still for a moment and gives mana.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SetOff, Spent = AfflictionKind.Static, SpentAtLeast = 5, Do = UpgradeAction.Hold, Seconds = 0.8f }, new UpgradeEffect { On = UpgradeTrigger.SetOff, Spent = AfflictionKind.Static, SpentAtLeast = 5, Do = UpgradeAction.Mana, Amount = 10f } } });
            // Kept from the first slice; not in the design.
            to.Add(new Upgrade { Id = Grounding, Name = "Grounding", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.DelayedCuts, Upgrade.StoredCharge }, X = -0.03f, Y = 5.95f, Icon = UpgradeIcons.Chain,
                Description = "A perfect guard sends an arc through the one who struck and on to two more.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Arc, Damage = 12f, Stagger = 20f, Count = 2, Reach = 5f } } });
            to.Add(new Upgrade { Id = HardCharge, Name = "Hard Charge", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { DeepReserve }, X = -2.62f, Y = 5.68f, Icon = UpgradeIcons.Hammer,
                Description = "A stored charge breaks balance harder.",
                Stats = new[] { new StatMod(PlayerStat.ChargeStagger, 0.25f) } });
            to.Add(new Upgrade { Id = WaitingThunder, Name = "Waiting Thunder", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { FullDischarge }, X = -2.69f, Y = 6.93f, Icon = UpgradeIcons.Spike,
                Description = "Hold the spell button to hang five bolts at your shoulder. Each blow you land sends one down with it. A heavy blow sends all that are left. 35 mana.",
                Form = "phalanx",
                Spell = new SpellSpec { Cost = 35f, Count = 5, Seconds = 10f, Reach = 8f, Damage = 8f, Stagger = 6f, Stacks = 1, Speed = 18f } });
            // It took the id and name the first slice gave a mark of critical chance; it is critical damage now, as designed.
            to.Add(new Upgrade { Id = KeenEdge, Name = "Keen Edge", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { DeepReserve }, X = -1.71f, Y = 5.46f, Icon = UpgradeIcons.Eye,
                Description = "Critical blows cut deeper.",
                Stats = new[] { new StatMod(PlayerStat.CritDamage, 0.2f) } });
            // There is no chance against enemies with Static alone: half the chance, on every enemy.
            to.Add(new Upgrade { Id = OpenSeam, Name = "Open Seam", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { DeepReserve }, X = -2.32f, Y = 4.61f, Icon = UpgradeIcons.Target,
                Description = "Blows on an enemy carrying Static are critical more often.",
                Stats = new[] { new StatMod(PlayerStat.CritChance, 0.08f, AfflictionKind.Static) } });
            // BESPOKE: Static to ten deep, and a discharge that does not arc and lands twice as hard on the one struck.
            // Until then a heavy blow on an enemy carrying Static bursts on that enemy alone, on top of the root's discharge.
            to.Add(new Upgrade { Id = SingleStroke, Name = "Single Stroke", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { WaitingThunder }, X = -3.21f, Y = 8.27f, Icon = UpgradeIcons.Blade,
                Description = "Static builds to ten. A discharge no longer arcs: all of it lands on the one struck, twice as hard.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Static, Do = UpgradeAction.Burst, Damage = 24f, Stagger = 20f, Radius = 1f, Cooldown = 0.5f } } });

            // Middle bough: Thunder. The sky and the surge.
            // A bolt that sets off an enemy already at five is still to be written by hand.
            to.Add(new Upgrade { Id = Conductor, Name = "Conductor", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Static }, X = 0.74f, Y = 3.87f, Icon = UpgradeIcons.Bolt,
                Description = "Thunderclap leaves Static two deep on all it strikes, and sets off an enemy already at five.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 2 }, new UpgradeEffect { On = UpgradeTrigger.SpellHit, Needs = AfflictionKind.Static, NeedsFull = true, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 4f, Cooldown = 0.2f } } });
            // The cost is the stat for every spell, the base one and whatever is worn.
            to.Add(new Upgrade { Id = QuickClap, Name = "Quick Clap", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Conductor }, X = 0.21f, Y = 5.04f, Icon = UpgradeIcons.Gem,
                Description = "Thunderclap costs less mana.",
                Stats = new[] { new StatMod(PlayerStat.BaseSpellCost, -0.2f) } });
            to.Add(new Upgrade { Id = ThirdLeap, Name = "Third Leap", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Conductor }, X = -0.14f, Y = 4.18f, Icon = UpgradeIcons.Chain,
                Description = "Thunderclap leaps to one more enemy.",
                Stats = new[] { new StatMod(PlayerStat.BaseSpellCount, 1f) } });
            // BESPOKE: holding the spell button again while the rod stands should step the player to it with a shock, and spend it.
            // It used to grow straight from Static.
            to.Add(new Upgrade { Id = Upgrade.LightningRod, Name = "Lightning Rod", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Conductor }, X = 0.58f, Y = 6.85f, Icon = UpgradeIcons.Tower,
                Description = "Hold the spell button to plant a rod. Bolts fall around it for a while. Hold again to step to it in a flash. 40 mana.",
                Form = SpellForms.Rod,
                Spell = new SpellSpec { Cost = 40f, Reach = 2f, Radius = 3.5f, Seconds = 8f, Interval = 0.9f, Damage = 12f, Stagger = 12f, Delay = 0.2f, Stacks = 1 } });
            // Kept from the first slice; not in the design.
            to.Add(new Upgrade { Id = Capacitor, Name = "Capacitor", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.LightningRod }, X = 0.36f, Y = 8.13f, Icon = UpgradeIcons.Gem,
                Description = "A perfect guard gives more mana.",
                Stats = new[] { new StatMod(PlayerStat.ManaFromGuards, 0.3f) } });
            // BESPOKE: a line of ground whose far end follows the player, from the rod, while it stands. Until then any spell cast
            // lays a line of lightning ahead of the player for a while.
            to.Add(new Upgrade { Id = LiveWire, Name = "Live Wire", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.LightningRod }, X = -0.94f, Y = 7.67f, Icon = UpgradeIcons.Beam,
                Description = "While a rod stands, a wire of lightning runs from it to you. Enemies it crosses are shocked.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, Do = UpgradeAction.Hazard, Shape = HazardShape.Line, Count = 5, Reach = 8f, Radius = 0.9f, Seconds = 4f, Interval = 0.5f, Damage = 5f, Affliction = AfflictionKind.Static, Stacks = 1, Cooldown = 6f } } });
            to.Add(new Upgrade { Id = LongOverdrive, Name = "Long Overdrive", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { LiveWire }, X = -1.37f, Y = 8.8f, Icon = UpgradeIcons.Hourglass,
                Description = "Overdrive lasts longer.",
                Stats = new[] { new StatMod(PlayerStat.SurgeSeconds, 0.3f) } });
            to.Add(new Upgrade { Id = RunningCurrent, Name = "Running Current", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { LiveWire }, X = -2.09f, Y = 7.98f, Icon = UpgradeIcons.Gem,
                Description = "Your blows give more mana.",
                Stats = new[] { new StatMod(PlayerStat.ManaFromBlows, 0.15f) } });
            // The pull that grows with each stack of Static, and the discharge of all it gathers, are still to be written by hand.
            to.Add(new Upgrade { Id = Lodestone, Name = "Lodestone", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { LiveWire }, X = -0.22f, Y = 9.14f, Icon = UpgradeIcons.Spiral,
                Description = "Hold the spell button to drag every enemy near toward you, further for each Static on it, then set all of it off at once. 45 mana.",
                Form = "reverse",
                Spell = new SpellSpec { Cost = 45f, Reach = 1.5f, Radius = 5f, Delay = 0.3f, Damage = 14f, Stagger = 30f, Stacks = 1 } });
            // BESPOKE (as built): the breath that opens Overdrive. The breath each perfect guard or dodge adds, and the dearer surge, are data.
            // It used to grow from Lightning Rod.
            to.Add(new Upgrade { Id = Upgrade.Overclock, Name = "Overclock", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { Lodestone }, X = -1.2f, Y = 10f, Icon = UpgradeIcons.Hourglass,
                Description = "Overdrive opens with a breath in which everything but you is slowed. Each perfect guard or perfect dodge while it lasts opens another. Overdrive costs more mana.",
                Stats = new[] { new StatMod(PlayerStat.SurgeCost, 0.375f) },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, WhileSurge = true, Do = UpgradeAction.SlowTime, Seconds = 1f, Amount = 0.35f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, WhileSurge = true, Do = UpgradeAction.SlowTime, Seconds = 1f, Amount = 0.35f },
                } });
            // Kept from the first slice; not in the design. It sits beyond Overclock because a mark may not lie inward of what it grows from.
            to.Add(new Upgrade { Id = ChainLance, Name = "Chain Lance", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Upgrade.Overclock, Capacitor }, X = 0.38f, Y = 10.3f, Icon = UpgradeIcons.Beam,
                Description = "Hold the spell button to throw a bolt that leaps through four more and leaves Static on each. 35 mana.",
                Form = SpellForms.Bolt,
                Spell = new SpellSpec { Cost = 35f, Reach = 12f, Damage = 26f, Stagger = 30f, Count = 4, Radius = 5f, Stacks = 2 } });

            // Right bough: Flicker. The dodge.
            // BESPOKE (as built). Still to add: the player is gone for the first tenth of a second and passes through enemies.
            to.Add(new Upgrade { Id = Upgrade.Blink, Name = "Blink", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Static }, X = 1.58f, Y = 4.51f, Icon = UpgradeIcons.Boot,
                Description = "The dodge becomes a short step through lightning that shocks whatever it passes through." });
            to.Add(new Upgrade { Id = BitingStep, Name = "Biting Step", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.Blink }, X = 1.08f, Y = 5.39f, Icon = UpgradeIcons.Fang,
                Description = "Blink shocks harder.",
                Stats = new[] { new StatMod(PlayerStat.BlinkPower, 0.4f) } });
            // BESPOKE (as built). Still to add: the copy repeats the player's last blow and lays its Static.
            to.Add(new Upgrade { Id = Upgrade.Afterimage, Name = "Afterimage", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Blink }, X = 2.07f, Y = 7.01f, Icon = UpgradeIcons.Eye,
                Description = "A dodge at the last moment leaves a copy of you that strikes where you stood." });
            // There is no stamina cost for a dodge out of a recovery alone: a quarter of the saving, on every dodge.
            to.Add(new Upgrade { Id = QuickReturn, Name = "Quick Return", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.Blink }, X = 2.52f, Y = 5.92f, Icon = UpgradeIcons.Arrow,
                Description = "A dodge that breaks off an attack costs less stamina.",
                Stats = new[] { new StatMod(PlayerStat.StaminaCost, -0.1f) } });
            to.Add(new Upgrade { Id = ForkedStep, Name = "Forked Step", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.Blink }, X = 2.41f, Y = 4.98f, Icon = UpgradeIcons.Boot,
                Description = "Blink shocks a wider path.",
                Stats = new[] { new StatMod(PlayerStat.BlinkWidth, 0.8f) } });
            // The flash form cuts the enemy faced; lines from enemy to enemy and the return to the starting spot are still to be written by hand.
            to.Add(new Upgrade { Id = SevenFlashes, Name = "Seven Flashes", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Upgrade.Afterimage }, X = 3.33f, Y = 8.5f, Icon = UpgradeIcons.Star,
                Description = "Hold the spell button to cut through up to seven enemies in one breath and return to where you stood. The cuts land together when you are still. 45 mana.",
                Form = "flash",
                Spell = new SpellSpec { Cost = 45f, Count = 7, Interval = 0.1f, Delay = 0.5f, Damage = 9f, Stagger = 8f, Stacks = 1, Reach = 8f } });
            // Kept from the first slice; not in the design.
            to.Add(new Upgrade { Id = QuickHands, Name = "Quick Hands", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.Blink }, X = 1.43f, Y = 6.25f, Icon = UpgradeIcons.Fist,
                Description = "The weapon is swung a little faster.",
                Stats = new[] { new StatMod(PlayerStat.SwingSpeed, 0.05f) } });
            to.Add(new Upgrade { Id = ChargedStep, Name = "Charged Step", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.Afterimage }, X = 2.18f, Y = 8.17f, Icon = UpgradeIcons.Gem,
                Description = "A perfect dodge gives more mana.",
                Stats = new[] { new StatMod(PlayerStat.ManaFromDodges, 0.5f) } });
            // BESPOKE: a guard that costs twice the stamina for each point caught. Until then the price is a shorter perfect guard.
            // The stamina given back is the dodge's usual 25.
            to.Add(new Upgrade { Id = NeverThere, Name = "Never There", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { SevenFlashes }, X = 2.96f, Y = 9.94f, Icon = UpgradeIcons.Wing,
                Description = "Every dodge carries further and a perfect dodge costs nothing. Your guard holds half as well.",
                Stats = new[] { new StatMod(PlayerStat.DodgeDistance, 0.3f), new StatMod(PlayerStat.PerfectGuardWindow, -0.25f) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Stamina, Amount = 25f } } });
            to.Add(new Upgrade { Id = BrightEcho, Name = "Bright Echo", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Upgrade.Afterimage }, X = 3.08f, Y = 7.37f, Icon = UpgradeIcons.Eye,
                Description = "The copy left by Afterimage strikes harder.",
                Stats = new[] { new StatMod(PlayerStat.AfterimagePower, 0.3f) } });
            // Kept from the first slice; not in the design.
            to.Add(new Upgrade { Id = Slipstream, Name = "Slipstream", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Upgrade.Afterimage }, X = 1.26f, Y = 8.87f, Icon = UpgradeIcons.Wing,
                Description = "A dodge at the last moment quickens your feet and your weapon for a while.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Boon, Stat = PlayerStat.MoveSpeed, Amount = 0.25f, Seconds = 6f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Boon, Stat = PlayerStat.SwingSpeed, Amount = 0.15f, Seconds = 6f },
                } });

            // BESPOKE (as built): Storm Step, done by hand.
            to.Add(new Upgrade { Id = Upgrade.StormStep, Name = "Storm Step", Element = E, Kind = UpgradeKind.Signature, X = 0f, Y = 12f, Icon = UpgradeIcons.Crown,
                Description = "While Overdrive lasts, every dodge costs nothing and shocks what it passes through." });
        }
    }
}

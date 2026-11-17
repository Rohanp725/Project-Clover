using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Dark: everything is bought with health and taken back out of the enemy. Three boughs from Curse: on the left
    /// Debt (power from health that is owed), in the middle Hunger (feeding on the Cursed), on the right Grave (the
    /// edge of death and past it). The Rise does not exist yet, so the marks that speak of being Risen work while
    /// Hunger lasts until it does: see staging/trees/notes_g4.md.
    /// </summary>
    public static class DarkTree
    {
        public const string Curse = "dark.curse";
        // Debt.
        public const string BloodPrice = "dark.bloodprice";
        public const string SlowCollector = "dark.slowcollector";
        public const string SmallCuts = "dark.smallcuts";
        public const string BorrowedStrength = "dark.borrowed";
        public const string MoreToSpend = "dark.deepwell";
        public const string RecklessArm = "dark.recklessarm";
        public const string DebtsCalled = "dark.debtscalled";
        public const string BitterHarvest = "dark.bitterharvest";
        public const string PawnedHeart = "dark.pawnedheart";
        public const string Unpaid = "dark.unpaid";
        // Hunger.
        public const string Feast = "dark.feast";
        public const string LongCurse = "dark.longcurse";
        public const string HeavyCurse = "dark.heavycurse";
        public const string Glut = "dark.glut";
        public const string FatOfTheKill = "dark.fatofthekill";
        public const string Dread = "dark.dread";
        public const string EasyPrey = "dark.easyprey";
        public const string DevouringMaw = "dark.maw";
        public const string LongHunger = "dark.longhunger";
        public const string NeverSated = "dark.neversated";
        // Grave.
        public const string NearTheGrave = "dark.nearthegrave";
        public const string HardToBury = "dark.hardtobury";
        public const string HollowStrength = "dark.hollowstrength";
        public const string SlowRot = "dark.slowrot";
        public const string GraveAppetite = "dark.graveappetite";
        public const string SecondShadow = "dark.secondshadow";
        public const string LongShadows = "dark.longshadows";
        public const string TheDeadStand = "dark.deadstand";
        public const string DeadAlready = "dark.deadalready";
        // The god's gift.
        public const string LifeForALife = "dark.lifeforalife";

        // Spell forms by the ids fixed in docs/design/FORMS.md; they are being built while this is written.
        private const string FormReverse = "reverse";
        private const string FormClone = "clone";
        private const string FormDragon = "dragon";
        private const string FormMinions = "minions";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Dark;

            to.Add(new Upgrade { Id = Curse, Name = "Curse", Element = E, Kind = UpgradeKind.Root, X = 0f, Y = 1.2f, Icon = UpgradeIcons.Moon,
                Description = "Blows of the weapon curse an enemy. A Cursed enemy deals less, and killing it heals you.",
                WeaponStacks = 1 });

            // Left bough: Debt. Power bought with health.

            to.Add(new Upgrade { Id = BloodPrice, Name = "Blood Price", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Curse }, X = -1.21f, Y = 2.4f, Icon = UpgradeIcons.Drop,
                Description = "With too little mana for a spell, the rest is paid in health. Health paid this way is only owed: blows win it back.",
                Stats = new[] { new StatMod(PlayerStat.BloodPrice, 0.5f) }, });
            to.Add(new Upgrade { Id = SlowCollector, Name = "Slow Collector", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { BloodPrice }, X = -0.72f, Y = 3.51f, Icon = UpgradeIcons.Hourglass,
                Description = "Health that is owed lingers longer before it is lost.",
                Stats = new[] { new StatMod(PlayerStat.GreyKept, 0.6f) } });
            // Stands in: there are no prices in health to make smaller yet, so blows that land on you take a little less.
            to.Add(new Upgrade { Id = SmallCuts, Name = "Small Cuts", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { BloodPrice }, X = -1.92f, Y = 3.18f, Icon = UpgradeIcons.Shield,
                Description = "Prices paid in health are smaller.",
                Stats = new[] { new StatMod(PlayerStat.HealthPrices, -0.2f) } });
            to.Add(new Upgrade { Id = BorrowedStrength, Name = "Borrowed Strength", Element = E, Kind = UpgradeKind.Major, Requires = new[] { BloodPrice }, X = -1.61f, Y = 4.2f, Icon = UpgradeIcons.Fist,
                Description = "The more of your health is owed, the harder you strike.", Stats = new[] { new StatMod(PlayerStat.GreyStrength, 0.0075f) } });
            to.Add(new Upgrade { Id = MoreToSpend, Name = "More to Spend", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { BorrowedStrength }, X = -1.03f, Y = 5.92f, Icon = UpgradeIcons.Heart,
                Description = "You have more health to spend.",
                Stats = new[] { new StatMod(PlayerStat.Health, 10f) } });
            // Stands in: swings cost no stamina today, so this lightens the dodge and the guard, and always.
            to.Add(new Upgrade { Id = RecklessArm, Name = "Reckless Arm", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { BorrowedStrength }, X = -2.38f, Y = 4.96f, Icon = UpgradeIcons.Wing,
                Description = "While any health is owed, swings cost less stamina.",
                Stats = new[] { new StatMod(PlayerStat.AttackStamina, -0.15f, StatWhen.GreyHeld) } });
            // The burst does not yet grow as health falls.
            to.Add(new Upgrade { Id = DebtsCalled, Name = "Debts Called", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { BorrowedStrength }, X = -2.0f, Y = 6.0f, Icon = UpgradeIcons.Spiral,
                Description = "Hold the spell button. Everything near is dragged to a point before you, and then thrown back by a burst. 40 mana.",
                Form = FormReverse,
                Spell = new SpellSpec { Cost = 40f, Reach = 1f, Radius = 4.5f, Delay = 0.5f, Damage = 18f, Stagger = 35f, Stacks = 1 } });
            to.Add(new Upgrade { Id = BitterHarvest, Name = "Bitter Harvest", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { DebtsCalled }, X = -2.88f, Y = 6.74f, Icon = UpgradeIcons.Gem,
                Description = "Below half your health, blows give more mana.",
                Stats = new[] { new StatMod(PlayerStat.ManaFromBlows, 0.25f, StatWhen.HalfHealth) } });
            // BESPOKE: the heart set down, health held at one, the ring that closes. Until then it costs thirty health (left grey) and no mana, and a shade of the player doubles most of every blow for ten seconds.
            to.Add(new Upgrade { Id = PawnedHeart, Name = "Pawned Heart", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { DebtsCalled }, X = -2.4f, Y = 7.8f, Icon = UpgradeIcons.Heart,
                Description = "Hold the spell button to tear out your heart and set it down. While it beats there you stand one blow from death, and strike as you never have. Come back for it in time and it returns with what your blows have earned. No mana.",
                // BESPOKE (built): HeartForm. The form takes the health itself; the price of one point only says that one who has nothing to pawn cannot cast it.
                Form = SpellForms.Heart,
                Spell = new SpellSpec { Cost = 0f, HealthCost = 1f, Seconds = 10f, Damage = 60f, Count = 30, Radius = 3f, Stacks = 0 } });
            // BESPOKE: every wound left whole as grey health. Until then grey health lingers twice as long and every blow landed wins a little back.
            to.Add(new Upgrade { Id = Unpaid, Name = "Unpaid", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { PawnedHeart }, X = -3.43f, Y = 8.8f, Icon = UpgradeIcons.Chain,
                Description = "Grey health lingers twice as long, and every blow you land wins a little health back. Flasks heal half as much.",
                Stats = new[] { new StatMod(PlayerStat.GreyKept, 1f), new StatMod(PlayerStat.FlaskHeal, -0.5f) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Heal, Amount = 2f, Cooldown = 0.5f } } });

            // Middle bough: Hunger. Taking it back.

            // The surge is not yet lengthened by a finisher.
            to.Add(new Upgrade { Id = Feast, Name = "Feast", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Curse }, X = 0f, Y = 2.9f, Icon = UpgradeIcons.Claw,
                Description = "A finisher on a Cursed enemy feeds you. It heals, and heals more while Hunger lasts.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Heal, Needs = AfflictionKind.Cursed, Amount = 12f },
                    new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Heal, Needs = AfflictionKind.Cursed, WhileSurge = true, Amount = 6f },
                } });
            to.Add(new Upgrade { Id = LongCurse, Name = "Long Curse", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Feast }, X = 0.8f, Y = 3.56f, Icon = UpgradeIcons.Hourglass,
                Description = "Curse lasts longer.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionSeconds, 0.5f) } });
            to.Add(new Upgrade { Id = HeavyCurse, Name = "Heavy Curse", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Feast }, X = 0.92f, Y = 4.86f, Icon = UpgradeIcons.Moon,
                Description = "Cursed enemies deal less still.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionPower, 0.47f) } });
            // BESPOKE: healing beyond a full bar kept as a second measure of health. Until then a flask or a kill wraps the player in a shade that takes one blow.
            to.Add(new Upgrade { Id = Glut, Name = "Glut", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Feast }, X = 0f, Y = 4.7f, Icon = UpgradeIcons.Plus,
                Description = "A flask drunk wraps you in a shade that turns aside one blow. So does a kill, once in ten seconds. Three shades at most.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.FlaskDrunk, Do = UpgradeAction.Ward, Count = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Do = UpgradeAction.Ward, Count = 1, Cooldown = 10f },
                } });
            // A number, but there is no stat for it: three more health for every kill (a kill has no target to ask whether it was Cursed).
            to.Add(new Upgrade { Id = FatOfTheKill, Name = "Fat of the Kill", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Glut }, X = -0.88f, Y = 5.01f, Icon = UpgradeIcons.Skull,
                Description = "Killing a Cursed enemy heals more.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Cursed, Do = UpgradeAction.Heal, Amount = 3f } } });
            // NEAREST: every enemy within eight metres buckles when a Cursed one dies, not only the Cursed, and no guard is dropped.
            to.Add(new Upgrade { Id = Dread, Name = "Dread", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Glut }, X = 0f, Y = 6.5f, Icon = UpgradeIcons.Eye,
                Description = "When a Cursed enemy dies, every enemy near it loses heart and its balance buckles.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Cursed, Do = UpgradeAction.Burst, Damage = 0f, Stagger = 30f, Radius = 8f, Cooldown = 1f } } });
            to.Add(new Upgrade { Id = EasyPrey, Name = "Easy Prey", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Dread }, X = -0.79f, Y = 7.37f, Icon = UpgradeIcons.Target,
                Description = "Cursed enemies take more from your weapon.",
                Stats = new[] { new StatMod(PlayerStat.DamageToAfflicted, 0.08f) } });
            // The feeding is the Maw's own (OwnSpell on SpellHit). The swallow of a kill or a staggered enemy is not built, and the words do not promise it.
            to.Add(new Upgrade { Id = DevouringMaw, Name = "Devouring Maw", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Dread }, X = 0f, Y = 8.3f, Icon = UpgradeIcons.Fang,
                Description = "Hold the spell button and a great jaw rises out of your shadow and bites what stands before you. Each Cursed enemy it bites feeds you. 40 mana.",
                Form = FormDragon,
                Spell = new SpellSpec { Cost = 40f, Reach = 5f, Radius = 1.6f, Damage = 30f, Stagger = 30f, Stacks = 1, Speed = 14f },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Heal, Needs = AfflictionKind.Cursed, Amount = 5f } } });
            to.Add(new Upgrade { Id = LongHunger, Name = "Long Hunger", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { DevouringMaw }, X = 1.02f, Y = 9.01f, Icon = UpgradeIcons.Hourglass,
                Description = "Hunger lasts longer.",
                Stats = new[] { new StatMod(PlayerStat.SurgeSeconds, 0.3f) } });
            // The game never lets the flasks fall below one, so one is left.
            to.Add(new Upgrade { Id = NeverSated, Name = "Never Sated", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { DevouringMaw }, X = -0.83f, Y = 9.56f, Icon = UpgradeIcons.Flask,
                Description = "You carry no flasks. Every blow you land feeds you a little, and every kill and finisher a great deal.",
                Stats = new[] { new StatMod(PlayerStat.FlaskCount, -3f), new StatMod(PlayerStat.Leech, 0.06f) },
                Effects = new[]
                {
                    
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Cursed, Do = UpgradeAction.Heal, Amount = 12f },
                    new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Heal, Amount = 20f },
                } });

            // Right bough: Grave. The edge of death and past it.

            // The strength is a boon given when health falls low, not held for as long as it stays low; the shade comes then too, not with each kill.
            to.Add(new Upgrade { Id = NearTheGrave, Name = "Near the Grave", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Curse }, X = 1.21f, Y = 2.4f, Icon = UpgradeIcons.Skull,
                Description = "Below a third of your health you strike harder, and each kill wraps you in a shade that takes the next blow.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, While = StatWhen.LowHealth, Do = UpgradeAction.Ward, Count = 1, Cooldown = 1f } }, Stats = new[] { new StatMod(PlayerStat.WeaponDamage, 0.2f, StatWhen.LowHealth) } });
            to.Add(new Upgrade { Id = HardToBury, Name = "Hard to Bury", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { NearTheGrave }, X = 1.89f, Y = 3.09f, Icon = UpgradeIcons.Shield,
                Description = "Below a third of your health you take less from every blow.",
                Stats = new[] { new StatMod(PlayerStat.DamageTaken, -0.12f, StatWhen.LowHealth) } });
            // The Rise is built (PlayerUpgrades.Rise, RiseForm), and "Risen" means it: StatWhen.Risen. Not flinching is not
            // built, and a kill gives health back in place of stilling the drain for a few breaths.
            to.Add(new Upgrade { Id = HollowStrength, Name = "Hollow Strength", Element = E, Kind = UpgradeKind.Major, Requires = new[] { NearTheGrave }, X = 1.61f, Y = 4.2f, Icon = UpgradeIcons.Fist,
                Description = "Risen, you strike harder and no blow makes you flinch. Each kill stills the draining for a few breaths.",
                Stats = new[] { new StatMod(PlayerStat.WeaponDamage, 0.25f, StatWhen.Risen) } });
            // The drain of the Risen runs at seven tenths: PlayerUpgrades.RiseDrain reads this mark.
            to.Add(new Upgrade { Id = SlowRot, Name = "Slow Rot", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { HollowStrength }, X = 1.02f, Y = 5.83f, Icon = UpgradeIcons.Hourglass,
                Description = "Risen, your health drains more slowly.",
                Effects = System.Array.Empty<UpgradeEffect>() });
            // A number, but there is no stat for it: while Risen each blow feeds a point more.
            to.Add(new Upgrade { Id = GraveAppetite, Name = "Grave Appetite", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { HollowStrength }, X = 2.38f, Y = 4.9f, Icon = UpgradeIcons.Plus,
                Description = "Risen, your blows feed you more.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AnyHit, While = StatWhen.Risen, Do = UpgradeAction.Heal, Amount = 1f, Cooldown = 0.2f } } });
            // BESPOKE: the second hold that changes places with the shadow. The shadow itself is the clone form.
            to.Add(new Upgrade { Id = SecondShadow, Name = "Second Shadow", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { HollowStrength }, X = 2.0f, Y = 6.0f, Icon = UpgradeIcons.Moon,
                Description = "Hold the spell button and your shadow stands at your shoulder for a while. It repeats every blow you land a beat late, for half its damage. 45 mana.",
                Form = FormClone,
                Spell = new SpellSpec { Cost = 45f, Seconds = 8f, Damage = 50f, Stacks = 1 } });
            to.Add(new Upgrade { Id = LongShadows, Name = "Long Shadows", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { SecondShadow }, X = 2.9f, Y = 6.72f, Icon = UpgradeIcons.Orb,
                Description = "Your spells strike 15% harder.",
                Stats = new[] { new StatMod(PlayerStat.SpellDamage, 0.15f) } });
            // The shades rise beside the player, not where the dead fell, and need no dead to rise.
            to.Add(new Upgrade { Id = TheDeadStand, Name = "The Dead Stand", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { SecondShadow }, X = 2.4f, Y = 7.8f, Icon = UpgradeIcons.Tower,
                Description = "Hold the spell button and three shades rise beside you. Each runs at an enemy, strikes a few times for you and is gone. 40 mana.",
                Form = FormMinions,
                Spell = new SpellSpec { Cost = 40f, Count = 3, Seconds = 5f, Interval = 1.6f, Damage = 12f, Stagger = 12f, Stacks = 1 } });
            // Built in PlayerUpgrades: Risen for good (StatWhen.Risen always holds), two points of health a second while an enemy is within twelve metres, never the last point, and Rise() refuses.
            to.Add(new Upgrade { Id = DeadAlready, Name = "Dead Already", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { TheDeadStand }, X = 2.25f, Y = 9.28f, Icon = UpgradeIcons.Skull,
                Description = "You do not wait to die. You are Risen from now on: in every fight your health drains to its last point, and dealing damage feeds you. You strike far harder. There is no second rising.",
                Stats = new[] { new StatMod(PlayerStat.WeaponDamage, 0.25f), new StatMod(PlayerStat.Leech, 0.3f) }, });

            // BESPOKE (built): the Rise, in PlayerUpgrades.Rise and RiseForm. This mark gives it.
            to.Add(new Upgrade { Id = LifeForALife, Name = "A Life for a Life", Element = E, Kind = UpgradeKind.Signature, X = 0f, Y = 12f, Icon = UpgradeIcons.Crown,
                Description = "The blow that would kill you does not, once. You rise with half your health, and it drains away. A sigil marks the one that killed you: land a finisher on it and you live again, and may rise once more.",
                Effects = System.Array.Empty<UpgradeEffect>() });
        }
    }
}

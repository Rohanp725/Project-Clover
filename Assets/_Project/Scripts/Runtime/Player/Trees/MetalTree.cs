using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Metal: the weapon and the guard and nothing else. Three boughs from Sunder: on the left Guard (the perfect
    /// guard forges something), in the middle Edge (the weapon itself), on the right Arsenal (blades conjured,
    /// thrown and called back). The marks named BESPOKE do the nearest thing data can do until their code is written.
    /// </summary>
    public static class MetalTree
    {
        public const string Root = "metal.sunder";
        // Guard.
        public const string ForgedAnswer = "metal.forgedanswer";
        public const string PractisedWrist = "metal.practisedwrist";
        public const string BrightSparks = "metal.brightsparks";
        public const string OpenLine = "metal.openline";
        public const string HardReturn = "metal.hardreturn";
        public const string BracedFrame = "metal.bracedframe";
        public const string CircleOfSteel = "metal.circle";
        public const string LongTemper = "metal.longtemper";
        public const string EdgeGuard = "metal.edgeguard";
        // Edge.
        public const string CounterCut = "metal.countercut";
        public const string DeepBite = "metal.deepbite";
        public const string FinishingEdge = "metal.finishingedge";
        public const string Splinter = "metal.splinter";
        public const string CleanEdge = "metal.cleanedge";
        public const string ShatterPoint = "metal.shatterpoint";
        public const string SlowMending = "metal.slowmending";
        public const string Hone = "metal.hone";
        public const string BalancedBlade = "metal.balancedblade";
        public const string MastersAnswer = "metal.mastersanswer";
        // Arsenal.
        public const string BuriedBlades = "metal.buriedblades";
        public const string TrueFlight = "metal.trueflight";
        public const string ReadySteel = "metal.readysteel";
        public const string Ricochet = "metal.ricochet";
        public const string LongThrow = "metal.longthrow";
        public const string ReturningEdge = "metal.returningedge";
        public const string PaidInBlows = "metal.paidinblows";
        public const string GraveOfSwords = "metal.graveofswords";
        public const string AllBladesHome = "metal.allbladeshome";
        // The god's gift.
        public const string HangingSwords = "metal.hangingswords";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Metal;
            const AfflictionKind S = AfflictionKind.Sundered;

            // The design notches with heavy blows only; a root lays its affliction with every blow, as the schema asks.
            to.Add(new Upgrade { Id = Root, Name = "Sunder", Element = E, Kind = UpgradeKind.Root, X = 0f, Y = 1.2f, Icon = UpgradeIcons.Anvil,
                Description = "Heavy blows and perfect guards notch an enemy's armour, up to three, and a finisher notches it three deep at once. Each notch lets your weapon bite deeper.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = S, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, Affliction = S, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Afflict, Affliction = S, Stacks = 3 },
                } });

            // Left bough: Guard.
            to.Add(new Upgrade { Id = ForgedAnswer, Name = "Forged Answer", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = -1.21f, Y = 2.4f, Icon = UpgradeIcons.Blade,
                Description = "A perfect guard forges a blade from the blow it turned. Up to three hang at your shoulder. Your next heavy blow sends them all.",
                Tally = new TallySpec { Name = "Blades", Cap = 3 }, Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Bank, Count = 1 }, new UpgradeEffect { On = UpgradeTrigger.HeavyHit, SpendsAll = true, PerStack = true, Do = UpgradeAction.Projectile, Flight = Flight.Homing, Count = 1, Damage = 10f, Stagger = 12f, Speed = 16f, Affliction = AfflictionKind.Sundered, Stacks = 0 } } });
            to.Add(new Upgrade { Id = PractisedWrist, Name = "Practised Wrist", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { ForgedAnswer }, X = -0.77f, Y = 3.49f, Icon = UpgradeIcons.Hourglass,
                Description = "The moment for a perfect guard is a little longer.",
                Stats = new[] { new StatMod(PlayerStat.PerfectGuardWindow, 0.17f) } });
            to.Add(new Upgrade { Id = BrightSparks, Name = "Bright Sparks", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { ForgedAnswer }, X = -1.94f, Y = 3.26f, Icon = UpgradeIcons.Gem,
                Description = "Perfect guards give more mana.",
                Stats = new[] { new StatMod(PlayerStat.ManaFromGuards, 0.33f) } });
            // NEAREST: the attacker is held for a moment (it cannot turn, nor move or strike) and blows on it are a little harder. Blows do not count as from behind.
            to.Add(new Upgrade { Id = OpenLine, Name = "Open Line", Element = E, Kind = UpgradeKind.Major, Requires = new[] { ForgedAnswer }, X = -1.61f, Y = 4.2f, Icon = UpgradeIcons.Arrow,
                Description = "A perfect guard throws the attacker's weapon wide. It is held still for a moment, and for that moment your weapon strikes 15% harder.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Hold, Seconds = 1.2f }, new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Boon, Stat = PlayerStat.WeaponDamage, Amount = 0.15f, Seconds = 1.5f } } });
            // NEEDS: a stat for the stagger a perfect guard throws back. A share of the weapon's stagger stands in.
            to.Add(new Upgrade { Id = HardReturn, Name = "Hard Return", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { OpenLine }, X = -1.03f, Y = 5.78f, Icon = UpgradeIcons.Shield,
                Description = "Every blow of your weapon breaks a little more balance.",
                Stats = new[] { new StatMod(PlayerStat.StaggerDealt, 0.06f) } });
            // GuardGrey: the grey share of a guarded blow.
            to.Add(new Upgrade { Id = BracedFrame, Name = "Braced Frame", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { OpenLine }, X = -2.39f, Y = 5f, Icon = UpgradeIcons.Heart,
                Description = "A blow held on your guard lets less through.",
                Stats = new[] { new StatMod(PlayerStat.GuardGrey, -0.25f) } });
            // The parry form turns the first blow only, not four, and its answer is a cone all round; it does not count as a perfect guard for other marks.
            to.Add(new Upgrade { Id = CircleOfSteel, Name = "Circle of Steel", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { OpenLine }, X = -2.0f, Y = 6.0f, Icon = UpgradeIcons.Ring,
                Description = "Hold the spell button and stand with your blade raised for a breath. The first blow from any side is turned for nothing and answered with a cut all round you that notches. If nothing strikes, the mana is lost. 40 mana.",
                Form = "parry",
                Spell = new SpellSpec { Cost = 40f, Seconds = 1.2f, Reach = 3.5f, Arc = 360f, Damage = 18f, Stagger = 40f, Stacks = 1 } });
            to.Add(new Upgrade { Id = LongTemper, Name = "Long Temper", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { CircleOfSteel }, X = -2.48f, Y = 7.11f, Icon = UpgradeIcons.Hourglass,
                Description = "Temper lasts longer.",
                Stats = new[] { new StatMod(PlayerStat.SurgeSeconds, 0.3f) } });
            // BESPOKE: a held guard blocks nothing; the perfect window is 0.28 s and may be pressed again after 0.2 s; each perfect guard deals a light blow to the attacker
            // and counts as a blow landed. Until then: the longer window, a dearer plain guard, and a cut on the attacker with each perfect guard.
            to.Add(new Upgrade { Id = EdgeGuard, Name = "Edge Guard", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { CircleOfSteel }, X = -1.22f, Y = 7.29f, Icon = UpgradeIcons.Shield,
                Description = "The perfect guard's moment is half again as long, and each one cuts the attacker and notches its armour. The dodge and a blow held on a plain guard cost half again as much stamina.",
                Stats = new[]
                {
                    new StatMod(PlayerStat.PerfectGuardWindow, 0.55f),
                    new StatMod(PlayerStat.StaminaCost, 0.5f),
                },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Burst, Radius = 1.2f, Damage = 12f, Stagger = 10f, Affliction = S, Stacks = 1 } } });

            // Middle bough: Edge.
            // Built (PlayerCombat.CounterCut): a blow that lands on an enemy winding up or striking is critical. The design's 25% more stagger is not in it.
            to.Add(new Upgrade { Id = CounterCut, Name = "Counter Cut", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = 0f, Y = 2.9f, Icon = UpgradeIcons.Eye,
                Description = "A blow that lands while an enemy is showing its own attack is critical.",
                Stats = new StatMod[0] });
            to.Add(new Upgrade { Id = DeepBite, Name = "Deep Bite", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { CounterCut }, X = 0.92f, Y = 4.86f, Icon = UpgradeIcons.Fang,
                Description = "Each notch lets your weapon bite deeper still.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionPower, 0.25f) } });
            to.Add(new Upgrade { Id = FinishingEdge, Name = "Finishing Edge", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { CounterCut }, X = -0.92f, Y = 4.85f, Icon = UpgradeIcons.Fist,
                Description = "The last blow of a chain cuts deeper.",
                Stats = new[] { new StatMod(PlayerStat.WeaponDamage, 0.12f, StatWhen.ChainEndBlow) } });
            // NEAREST: the cone is thrown from the player through the target, not from the target.
            to.Add(new Upgrade { Id = Splinter, Name = "Splinter", Element = E, Kind = UpgradeKind.Major, Requires = new[] { CounterCut }, X = 0f, Y = 4.7f, Icon = UpgradeIcons.Spike,
                Description = "A blow on armour notched three deep throws splinters from you through it, and they cut and notch whoever stands behind.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AnyHit, Needs = AfflictionKind.Sundered, NeedsFull = true, Do = UpgradeAction.Cone, Reach = 5f, Arc = 60f, Damage = 8f, Stagger = 8f, Affliction = AfflictionKind.Sundered, Stacks = 1, Cooldown = 0.6f } } });
            to.Add(new Upgrade { Id = CleanEdge, Name = "Clean Edge", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Splinter }, X = 0.75f, Y = 5.82f, Icon = UpgradeIcons.Eye,
                Description = "Critical blows cut deeper.",
                Stats = new[] { new StatMod(PlayerStat.CritDamage, 0.15f) } });
            // NEEDS: an affliction that can be made to last for an enemy's life, and damage by the depth it carries. Here the finisher strikes again and notches to full.
            to.Add(new Upgrade { Id = ShatterPoint, Name = "Shatter Point", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Splinter }, X = 0f, Y = 6.5f, Icon = UpgradeIcons.Burst,
                Description = "A finisher on notched armour strikes a second time and notches it three deep.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Needs = S, Do = UpgradeAction.Burst, Radius = 1.2f, Damage = 20f, Stagger = 0f },
                    new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Needs = S, Do = UpgradeAction.Afflict, Affliction = S, Stacks = 3 },
                } });
            to.Add(new Upgrade { Id = SlowMending, Name = "Slow Mending", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { ShatterPoint }, X = 0.98f, Y = 7.2f, Icon = UpgradeIcons.Hourglass,
                Description = "Notched armour takes longer to mend.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionSeconds, 0.5f) } });
            // NEEDS: a spell that is a boon on the player counted in blows, and blows that pass a guard. Here the brand form sets the five blows on the enemy faced.
            to.Add(new Upgrade { Id = Hone, Name = "Hone", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { ShatterPoint }, X = 0f, Y = 8.3f, Icon = UpgradeIcons.Blade,
                Description = "Hold the spell button to set a sign on the enemy you face. Each of your next five blows on it sets off a small burst that notches. 30 mana.",
                Form = "brand",
                Spell = new SpellSpec { Cost = 30f, Reach = 8f, Seconds = 12f, Count = 5, Radius = 1.5f, Damage = 6f, Stagger = 6f, Stacks = 1 } });
            to.Add(new Upgrade { Id = BalancedBlade, Name = "Balanced Blade", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Hone }, X = 0.88f, Y = 9.08f, Icon = UpgradeIcons.Wing,
                Description = "Swings cost less stamina.",
                Stats = new[] { new StatMod(PlayerStat.AttackStamina, -0.1f) } });
            // Built by hand (WeaponCombo.SkillCostScale and SkillAtOnce, kept by PlayerUpgrades): for two seconds after a perfect guard the
            // skill costs no mana and a press of heavy releases it at once; at any other time it costs half again as much.
            to.Add(new Upgrade { Id = MastersAnswer, Name = "Master's Answer", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { Hone }, X = -1.21f, Y = 8.99f, Icon = UpgradeIcons.Star,
                Description = "After a perfect guard, your weapon's skill is free and comes at once, with no time to charge. At any other time it costs more.",
                Stats = new StatMod[0] });

            // Right bough: Arsenal.
            // NEEDS: thrown blades that stand in an enemy and a trigger that drives them. Here a heavy blow on notched armour strikes a second time.
            to.Add(new Upgrade { Id = BuriedBlades, Name = "Buried Blades", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = 1.21f, Y = 2.4f, Icon = UpgradeIcons.Target,
                Description = "A heavy blow on notched armour strikes a second time, harder, and notches it again. Once a second at most.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = S, Do = UpgradeAction.Burst, Radius = 1.2f, Damage = 24f, Stagger = 30f, Affliction = S, Stacks = 1, Cooldown = 1f } } });
            to.Add(new Upgrade { Id = TrueFlight, Name = "True Flight", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { BuriedBlades }, X = 0.78f, Y = 3.52f, Icon = UpgradeIcons.Arrow,
                Description = "Conjured blades strike harder.",
                Stats = new[] { new StatMod(PlayerStat.SpellDamage, 0.15f) } });
            // The stat makes every spell cheaper, not the base spell alone.
            to.Add(new Upgrade { Id = ReadySteel, Name = "Ready Steel", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { BuriedBlades }, X = 1.93f, Y = 3.22f, Icon = UpgradeIcons.Gem,
                Description = "Flung Blade costs less mana.",
                Stats = new[] { new StatMod(PlayerStat.BaseSpellCost, -0.2f) } });
            // The arc strikes its first target again and does not weaken as it leaps.
            to.Add(new Upgrade { Id = Ricochet, Name = "Ricochet", Element = E, Kind = UpgradeKind.Major, Requires = new[] { BuriedBlades }, X = 1.61f, Y = 4.2f, Icon = UpgradeIcons.Chain,
                Description = "A spell that strikes an enemy sends a blade through it that springs on to the next enemy near, and then the next, notching each.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, Do = UpgradeAction.Arc, Damage = 9f, Stagger = 6f, Count = 2, Reach = 6f, Affliction = S, Stacks = 1, Cooldown = 0.3f } } });
            to.Add(new Upgrade { Id = LongThrow, Name = "Long Throw", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Ricochet }, X = 2.42f, Y = 5.01f, Icon = UpgradeIcons.Arrow,
                Description = "Blades fly further and spring further.",
                Stats = new[] { new StatMod(PlayerStat.SpellReach, 0.25f) } });
            // BESPOKE: a great blade thrown 8 m out and back (12 damage, 10 stagger each way, a notch on the first pass); a guard in the perfect window as it returns sends it
            // out again faster and half again as hard, three times at most. Until then: the lunging form down the same lane, struck once for both passes.
            to.Add(new Upgrade { Id = ReturningEdge, Name = "Returning Edge", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Ricochet }, X = 2.0f, Y = 6.0f, Icon = UpgradeIcons.Spiral,
                Description = "Hold the spell button and a great head of steel lunges down a line before you. It carries along what it passes and bites hard where it ends. 35 mana.",
                Form = "dragon",
                Spell = new SpellSpec { Cost = 35f, Reach = 8f, Radius = 0.75f, Damage = 24f, Stagger = 20f, Stacks = 1, Speed = 14f } });
            to.Add(new Upgrade { Id = PaidInBlows, Name = "Paid in Blows", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { ReturningEdge }, X = 2.92f, Y = 6.75f, Icon = UpgradeIcons.Gem,
                Description = "Blows on notched armour give more mana.",
                Stats = new[] { new StatMod(PlayerStat.ManaFromBlows, 0.25f, AfflictionKind.Sundered) } });
            // The phalanx form hangs the twelve at the shoulders, not in a ring in the ground, and they do not fall first.
            to.Add(new Upgrade { Id = GraveOfSwords, Name = "Grave of Swords", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { ReturningEdge }, X = 2.4f, Y = 7.8f, Icon = UpgradeIcons.Blade,
                Description = "Hold the spell button and twelve blades hang behind your shoulders. One after another they fly at the nearest enemy, and a heavy blow that lands sends all that remain at the one it struck. 50 mana.",
                Form = "phalanx",
                Spell = new SpellSpec { Cost = 50f, Count = 12, Seconds = 12f, Reach = 6f, Damage = 9f, Stagger = 6f, Stacks = 1, Speed = 16f } });
            // BESPOKE: conjured blades never fade (sixteen at most); lighting the surge calls every one home in a straight line through whatever stands between: 12 damage,
            // 8 stagger and a notch each, and each that arrives adds 0.4 s to the surge. Until then: lighting the surge throws a fan of blades out and back, and the surge costs more and lasts a little longer.
            to.Add(new Upgrade { Id = AllBladesHome, Name = "All Blades Home", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { GraveOfSwords }, X = 3.48f, Y = 8.78f, Icon = UpgradeIcons.Spiral,
                Description = "Beginning Temper throws five blades out in a fan and calls them back, and they cut and notch what they pass. Temper costs more and lasts a little longer.",
                Stats = new[]
                {
                    new StatMod(PlayerStat.SurgeCost, 0.375f),
                    new StatMod(PlayerStat.SurgeSeconds, 0.2f),
                },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Projectile, Flight = Flight.Returning, Count = 5, Damage = 12f, Stagger = 8f, Speed = 14f, Seconds = 1.2f, Affliction = S, Stacks = 1 } } });

            // BESPOKE (built): HangingSwordsForm; the held guard is perfect through the window made as long as the surge.
            to.Add(new Upgrade { Id = HangingSwords, Name = "Hanging Swords", Element = E, Kind = UpgradeKind.Signature, X = 0f, Y = 12f, Icon = UpgradeIcons.Crown,
                Description = "While Temper lasts, every blow you hold on your guard is a perfect guard, and a sword is forged above the one who struck, up to eight. When Temper ends, they fall.",
                Stats = new[] { new StatMod(PlayerStat.PerfectGuardWindow, 200f, StatWhen.Surge) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Guard, While = StatWhen.Surge, Do = UpgradeAction.Mana, Amount = 5f, Cooldown = 0.3f }, new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, While = StatWhen.Surge, Do = UpgradeAction.Mana, Amount = 5f, Cooldown = 0.3f }, new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.HangingSwords, Spell = new SpellSpec { Cost = 0f, Count = 8, Damage = 16f, Stagger = 20f, Radius = 1.5f, Stacks = 1 } } } });
        }
    }
}

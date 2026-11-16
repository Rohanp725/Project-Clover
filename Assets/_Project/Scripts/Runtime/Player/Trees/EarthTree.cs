using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Earth: it does not get out of the way. Three boughs from Tremor: on the left Standing (hold your ground and be
    /// paid for it), in the middle Weight (slow, certain blows), on the right Ground (stone raised and broken over
    /// the enemy). The marks named BESPOKE do the nearest thing data can do until their own code is written.
    /// </summary>
    public static class EarthTree
    {
        public const string Root = "earth.tremor";
        // Standing.
        public const string Rooted = "earth.rooted";
        public const string DeepFooting = "earth.deepfooting";
        public const string ThickHide = "earth.thickhide";
        public const string StoneSkin = "earth.stoneskin";
        public const string SteadyArm = "earth.steadyarm";
        public const string OldGranite = "earth.oldgranite";
        public const string Aftershock = "earth.aftershock";
        public const string DeepBedrock = "earth.deepbedrock";
        public const string MountainStance = "earth.stance";
        public const string Immovable = "earth.immovable";
        // Weight.
        public const string HeavyHand = "earth.heavyhand";
        public const string BreakingWeight = "earth.breakingweight";
        public const string FullSwing = "earth.fullswing";
        public const string Landslide = "earth.landslide";
        public const string ShakenGround = "earth.loosefooting";
        public const string LongTremor = "earth.longtremor";
        public const string QuakeStep = "earth.quakestep";
        public const string EasyBurden = "earth.easyburden";
        public const string MountainCleaver = "earth.cleaver";
        public const string TheSlowHand = "earth.slowhand";
        // Ground.
        public const string StandingStones = "earth.standingstones";
        public const string HardStone = "earth.hardstone";
        public const string NearToHand = "earth.nearathand";
        public const string Toppling = "earth.toppling";
        public const string FarRoots = "earth.farroots";
        public const string Fissure = "earth.fissure";
        public const string OldStones = "earth.oldstones";
        public const string Rampart = "earth.rampart";
        public const string TheClosedRing = "earth.closedring";
        // The god's gift.
        public const string HandOfTheMountain = "earth.hand";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Earth;
            const AfflictionKind T = AfflictionKind.Tremor;

            // The design lays Tremor with heavy blows only; a root lays its affliction with every blow, as the schema asks.
            to.Add(new Upgrade { Id = Root, Name = "Tremor", Element = E, Kind = UpgradeKind.Root, X = 0f, Y = 1.2f, Icon = UpgradeIcons.Rock,
                Description = "Heavy blows and finishers shake an enemy's footing, up to three deep. A shaken enemy is slow, and its balance does not come back.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = T, Stacks = 1 }, new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Afflict, Affliction = T, Stacks = 3 } } });

            // Left bough: Standing.
            // NEAREST: Rooted is the planted state (no dodge begun for three seconds): less damage and a cheaper guard while it holds. Not built: nothing pushing you back.
            to.Add(new Upgrade { Id = Rooted, Name = "Rooted", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = -1.21f, Y = 2.4f, Icon = UpgradeIcons.Tower,
                Description = "Fight without rolling for a few breaths and you are Rooted. Rooted, nothing pushes you back and your guard costs less. A roll ends it.", Stats = new[] { new StatMod(PlayerStat.DamageTaken, -0.08f, StatWhen.Planted), new StatMod(PlayerStat.GuardStamina, -0.3f, StatWhen.Planted) } });
            to.Add(new Upgrade { Id = DeepFooting, Name = "Deep Footing", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Rooted }, X = -0.78f, Y = 3.5f, Icon = UpgradeIcons.Hourglass,
                Description = "You become Rooted sooner.",
                Stats = new[] { new StatMod(PlayerStat.PlantedSooner, 1f) } });
            to.Add(new Upgrade { Id = ThickHide, Name = "Thick Hide", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Rooted }, X = -1.93f, Y = 3.24f, Icon = UpgradeIcons.Shield,
                Description = "Rooted, you take less from every blow.",
                Stats = new[] { new StatMod(PlayerStat.DamageTaken, -0.08f, StatWhen.Planted) } });
            // NEAREST: a ward (the whole of one blow, three held) stands for a plate that takes most of a blow.
            to.Add(new Upgrade { Id = StoneSkin, Name = "Stone Skin", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Rooted }, X = -1.61f, Y = 4.2f, Icon = UpgradeIcons.Wall,
                Description = "A perfect guard grows a plate of stone on you, up to three. A plate takes most of one blow, and breaks.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Ward, Count = 1 } } });
            to.Add(new Upgrade { Id = SteadyArm, Name = "Steady Arm", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { StoneSkin }, X = -1.13f, Y = 6.34f, Icon = UpgradeIcons.Shield,
                Description = "Holding your guard costs less stamina.",
                Stats = new[] { new StatMod(PlayerStat.GuardStamina, -0.167f) } });
            to.Add(new Upgrade { Id = OldGranite, Name = "Old Granite", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { StoneSkin }, X = -2.43f, Y = 5.03f, Icon = UpgradeIcons.Heart,
                Description = "Grey health lingers longer before it fades.",
                Stats = new[] { new StatMod(PlayerStat.GreyKept, 0.6f) } });
            to.Add(new Upgrade { Id = Aftershock, Name = "Aftershock", Element = E, Kind = UpgradeKind.Major, Requires = new[] { StoneSkin }, X = -2.0f, Y = 6.0f, Icon = UpgradeIcons.Burst,
                Description = "A plate that breaks bursts outward and shakes everyone near.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.WardSpent, Do = UpgradeAction.Burst, At = EffectAt.Self, Radius = 3f, Damage = 14f, Stagger = 24f, Affliction = T, Stacks = 1 } } });
            to.Add(new Upgrade { Id = DeepBedrock, Name = "Deep Bedrock", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Aftershock }, X = -2.89f, Y = 6.73f, Icon = UpgradeIcons.Hourglass,
                Description = "Bedrock lasts longer.",
                Stats = new[] { new StatMod(PlayerStat.SurgeSeconds, 0.3f) } });
            // The parry form answers with a cone, not a pillar under the attacker, and gives no mana back when nothing strikes.
            to.Add(new Upgrade { Id = MountainStance, Name = "Mountain Stance", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Aftershock }, X = -2.4f, Y = 7.8f, Icon = UpgradeIcons.Fist,
                Description = "Hold the spell button to stand as stone. The first blow to land on you is turned, and a fist of rock rises under the one who struck. 35 mana.",
                Form = "parry",
                Spell = new SpellSpec { Cost = 35f, Seconds = 1.5f, Reach = 4f, Arc = 90f, Damage = 30f, Stagger = 60f, Stacks = 2 } });
            // BESPOKE: the dodge button becomes a brace that moves nowhere; its first 0.2 s is a perfect guard against any blow, red ones too; Rooted never ends.
            // Until then: the roll hardly moves, the guard is cheap and you take less, as one who is always Rooted.
            to.Add(new Upgrade { Id = Immovable, Name = "Immovable", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { MountainStance }, X = -2.82f, Y = 9.2f, Icon = UpgradeIcons.Tower,
                Description = "You no longer roll. The dodge button braces you where you stand, and a brace timed well turns any blow, the red ones too. You are always Rooted.",
                Stats = new[]
                {
                    new StatMod(PlayerStat.DodgeDistance, -0.9f),
                    new StatMod(PlayerStat.DamageTaken, -0.16f),
                    new StatMod(PlayerStat.StaminaCost, -0.3f),
                    new StatMod(PlayerStat.PerfectGuardWindow, 0.3f),
                } });

            // Middle bough: Weight.
            // NEEDS: a trigger "a heavy blow begins" and no flinch. Here the blow that lands leaves you hard to hurt for a moment after.
            to.Add(new Upgrade { Id = HeavyHand, Name = "Heavy Hand", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = 0f, Y = 2.9f, Icon = UpgradeIcons.Hammer,
                Description = "Your heavy blows are not stopped by being struck. You take less from that blow, and yours still lands.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Boon, Stat = PlayerStat.DamageTaken, Amount = -0.35f, Seconds = 2f } } });
            to.Add(new Upgrade { Id = BreakingWeight, Name = "Breaking Weight", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { HeavyHand }, X = 0.92f, Y = 4.83f, Icon = UpgradeIcons.Hammer,
                Description = "Heavy blows break balance sooner.",
                Stats = new[] { new StatMod(PlayerStat.StaggerDealt, 0.15f, StatWhen.HeavyBlow) } });
            to.Add(new Upgrade { Id = FullSwing, Name = "Full Swing", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { HeavyHand }, X = -0.92f, Y = 4.84f, Icon = UpgradeIcons.Fist,
                Description = "Heavy blows and finishers cut deeper.",
                Stats = new[] { new StatMod(PlayerStat.WeaponDamage, 0.1f, StatWhen.HeavyBlow) } });
            // NEAREST: the growing strength within a chain is a two-second boon from each light blow; the burst comes with the blow that ends a chain, as designed.
            to.Add(new Upgrade { Id = Landslide, Name = "Landslide", Element = E, Kind = UpgradeKind.Major, Requires = new[] { HeavyHand }, X = 0f, Y = 4.7f, Icon = UpgradeIcons.Comet,
                Description = "Each blow in a chain lands harder than the one before. The heavy blow that ends a chain shakes the ground round its target.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.LightHit, Do = UpgradeAction.Boon, Stat = PlayerStat.WeaponDamage, Amount = 0.16f, Seconds = 2f },
                    new UpgradeEffect { On = UpgradeTrigger.LightHit, Do = UpgradeAction.Boon, Stat = PlayerStat.StaggerDealt, Amount = 0.16f, Seconds = 2f },
                    new UpgradeEffect { On = UpgradeTrigger.ChainEnd, Do = UpgradeAction.Burst, Radius = 2.5f, Damage = 10f, Stagger = 15f, Affliction = T, Stacks = 1, Cooldown = 1f },
                } });
            to.Add(new Upgrade { Id = ShakenGround, Name = "Shaken Ground", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Landslide }, X = 0.75f, Y = 5.77f, Icon = UpgradeIcons.Target,
                Description = "Shaken enemies take more from your weapon.",
                Stats = new[] { new StatMod(PlayerStat.DamageToAfflicted, 0.1f) } });
            to.Add(new Upgrade { Id = LongTremor, Name = "Long Tremor", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Landslide }, X = -0.44f, Y = 5.68f, Icon = UpgradeIcons.Hourglass,
                Description = "Tremor lasts longer.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionSeconds, 0.5f) } });
            // The nova does not yet give double stagger to an enemy showing an attack, nor break that attack.
            to.Add(new Upgrade { Id = QuakeStep, Name = "Quake Step", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Landslide }, X = 0f, Y = 6.5f, Icon = UpgradeIcons.Boot,
                Description = "Hold the spell button to raise a foot and bring it down. The ground heaves all round you, and an enemy caught in the middle of its own attack loses its footing. 35 mana.",
                Form = "nova",
                Spell = new SpellSpec { Cost = 35f, Radius = 5f, Delay = 0.45f, Damage = 16f, Stagger = 30f, Stacks = 2 } });
            // NEAREST: the stamina of every blow, not of heavy blows alone, so the share is smaller.
            to.Add(new Upgrade { Id = EasyBurden, Name = "Easy Burden", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { QuakeStep }, X = 1.12f, Y = 6.94f, Icon = UpgradeIcons.Wing,
                Description = "Heavy blows cost less stamina.",
                Stats = new[] { new StatMod(PlayerStat.AttackStamina, -0.08f) } });
            // BESPOKE: a held charge in three stages (0.5, 1.2, 2.0 s) released as one overhead heavy blow down a drawn lane: 30 / 55 / 90 damage, 40 / 70 / 120 stagger,
            // Tremor 1 / 2 / 3; no flinch and half damage while holding. Until then: the great falling sword at its middle stage, with the crack it leaves.
            to.Add(new Upgrade { Id = MountainCleaver, Name = "Mountain Cleaver", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { QuakeStep }, X = 0f, Y = 8.3f, Icon = UpgradeIcons.Blade,
                Description = "Hold the spell button and stone gathers on your weapon, in three stages. Let go to bring it down. While you hold you can be hurt but not stopped. 45 mana.",
                Form = "sword",
                Spell = new SpellSpec { Cost = 45f, Reach = 5f, Delay = 1.2f, Radius = 3f, Damage = 55f, Stagger = 70f, Stacks = 2, Seconds = 3f, LingerDamage = 4f } });
            // BESPOKE in part: every blow is slower and dearer and light blows far stronger, as designed. Light blows do not yet count as heavy: no guard is passed, no flinch forced, and heavy-blow triggers do not fire.
            to.Add(new Upgrade { Id = TheSlowHand, Name = "The Slow Hand", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { MountainCleaver }, X = 0.19f, Y = 9.76f, Icon = UpgradeIcons.Hammer,
                Description = "Every swing is slower and costs more stamina. Every swing lands as a heavy blow.",
                WeaponStacks = 1,
                Stats = new[] { new StatMod(PlayerStat.SwingSpeed, -0.22f), new StatMod(PlayerStat.AttackStamina, 0.2f), new StatMod(PlayerStat.WeaponDamage, 0.35f, StatWhen.LightBlow), new StatMod(PlayerStat.StaggerDealt, 0.6f, StatWhen.LightBlow) } });

            // Right bough: Ground.
            // NEEDS: pillars that stand, and a trigger "an enemy is driven into one". Here a second pillar rises under whatever a spell of yours strikes.
            to.Add(new Upgrade { Id = StandingStones, Name = "Standing Stones", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = 1.21f, Y = 2.4f, Icon = UpgradeIcons.Tower,
                Description = "Two pillars may stand at once. An enemy driven into a pillar breaks it and is hurt.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, Do = UpgradeAction.Falling, Radius = 1.5f, Delay = 0.5f, Damage = 20f, Stagger = 35f, Affliction = T, Stacks = 1, Cooldown = 1.5f } } });
            to.Add(new Upgrade { Id = HardStone, Name = "Hard Stone", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { StandingStones }, X = 0.78f, Y = 3.5f, Icon = UpgradeIcons.Rock,
                Description = "Pillars and their shards strike harder.",
                Stats = new[] { new StatMod(PlayerStat.SpellDamage, 0.15f) } });
            // The stat makes every spell cheaper, not the base spell alone.
            to.Add(new Upgrade { Id = NearToHand, Name = "Near to Hand", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { StandingStones }, X = 1.92f, Y = 3.21f, Icon = UpgradeIcons.Gem,
                Description = "Stone Rise costs less mana.",
                Stats = new[] { new StatMod(PlayerStat.BaseSpellCost, -0.2f) } });
            // NEEDS: pillars that stand, and a condition "one is within 4 m". Here every perfect guard brings stone down on the attacker.
            to.Add(new Upgrade { Id = Toppling, Name = "Toppling", Element = E, Kind = UpgradeKind.Major, Requires = new[] { StandingStones }, X = 1.61f, Y = 4.2f, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard beside one of your pillars topples it onto the one who struck.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Falling, Radius = 1.5f, Delay = 0.3f, Damage = 26f, Stagger = 45f, Affliction = T, Stacks = 2 } } });
            to.Add(new Upgrade { Id = FarRoots, Name = "Far Roots", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Toppling }, X = 2.42f, Y = 4.98f, Icon = UpgradeIcons.Arrow,
                Description = "Stone rises further from you, and its shards fly further.",
                Stats = new[] { new StatMod(PlayerStat.SpellReach, 0.25f) } });
            // The spikes do not yet hold what they catch, nor stand as a wall after.
            to.Add(new Upgrade { Id = Fissure, Name = "Fissure", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Toppling }, X = 2.0f, Y = 6.0f, Icon = UpgradeIcons.Spike,
                Description = "Hold the spell button to drive your weapon into the ground. A crack runs ahead, waits a breath, and breaks open in a row of spikes that hold what they catch. 40 mana.",
                Form = "spikes",
                Spell = new SpellSpec { Cost = 40f, Reach = 8f, Speed = 20f, Count = 1, Radius = 1f, Damage = 28f, Stagger = 35f, Stacks = 1 } });
            // NEEDS: a stat for how long what a spell leaves lasts. A little spell damage stands in.
            to.Add(new Upgrade { Id = OldStones, Name = "Old Stones", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Fissure }, X = 2.9f, Y = 6.73f, Icon = UpgradeIcons.Hourglass,
                Description = "Pillars, spikes and walls stand longer.",
                Stats = new[] { new StatMod(PlayerStat.SpellDamage, 0.06f) } });
            // The wall form bursts when its time ends as well as when struck; the design has it crumble for nothing.
            to.Add(new Upgrade { Id = Rampart, Name = "Rampart", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Fissure }, X = 2.4f, Y = 7.8f, Icon = UpgradeIcons.Wall,
                Description = "Hold the spell button to raise a wall before you. Strike it with a heavy blow and it falls forward on whatever stands behind it. 40 mana.",
                Form = "wall",
                Spell = new SpellSpec { Cost = 40f, Reach = 2.5f, Arc = 5f, Delay = 0.2f, Seconds = 8f, Radius = 4f, Damage = 34f, Stagger = 50f, Stacks = 2 } });
            // BESPOKE: the surge raises eight stones in a circle 12 m across round the player and the enemy faced; nothing walks, rolls or is thrown across it while the surge lasts;
            // an enemy driven into a stone takes 20 damage and 35 stagger; inside it the player is Rooted. Until then: a ring of stone that bites what touches it, and the dearer surge.
            to.Add(new Upgrade { Id = TheClosedRing, Name = "The Closed Ring", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { Rampart }, X = 2.02f, Y = 9.18f, Icon = UpgradeIcons.Ring,
                Description = "Bedrock raises a ring of stone round you and the enemy you face. Nothing enters and nothing leaves while it stands, you least of all.",
                Stats = new[] { new StatMod(PlayerStat.SurgeCost, 0.25f) },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Hazard, At = EffectAt.Self, Shape = HazardShape.Ring, Reach = 6f, Count = 8, Radius = 1f, Seconds = 10f, Interval = 1f, Damage = 8f, Stagger = 20f, Affliction = T, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Boon, Stat = PlayerStat.DamageTaken, Amount = -0.08f, Seconds = 10f },
                } });

            // BESPOKE (built): HandForm.
            to.Add(new Upgrade { Id = HandOfTheMountain, Name = "Hand of the Mountain", Element = E, Kind = UpgradeKind.Signature, X = 0f, Y = 12f, Icon = UpgradeIcons.Crown,
                Description = "While Bedrock lasts, a hand of stone as tall as a tower follows your arm. Each heavy blow you land, it lands again a moment after.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.Hand, Spell = new SpellSpec { Cost = 0f, Damage = 70f, Stagger = 40f, Radius = 3f, Delay = 0.5f, Stacks = 1 } } } });
        }
    }
}

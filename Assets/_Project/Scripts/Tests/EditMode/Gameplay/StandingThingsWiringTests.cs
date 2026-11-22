using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.Player.Trees;
using Clover.World;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>The standing things wired into the marks and the enemies (standin_clearance_plan.md pass 11).</summary>
    public class StandingThingsWiringTests
    {
        [SetUp]
        public void Begin()
        {
            StandingThings.Clear();
            Pickups.Clear();
            OwnGrounds.Clear();
        }

        [TearDown]
        public void End()
        {
            StandingThings.Clear();
            Pickups.Clear();
            OwnGrounds.Clear();
        }

        [Test]
        public void OwnGroundsLieByNameUntilTheirTimeAndSlickOnesThrowFarther()
        {
            OwnGrounds.Lay(OwnGrounds.Sheet, Vector3.zero, 2.3f, 5f, 1.5f);
            Assert.IsTrue(OwnGrounds.Under(OwnGrounds.Sheet, new Vector3(1f, 0f, 1f), 1f));
            Assert.IsFalse(OwnGrounds.Under(OwnGrounds.Sheet, new Vector3(3f, 0f, 0f), 1f), "off the sheet");
            Assert.IsFalse(OwnGrounds.Under(OwnGrounds.WetSand, Vector3.zero, 1f), "another ground's name");
            Assert.IsFalse(OwnGrounds.Under(OwnGrounds.Sheet, Vector3.zero, 5.5f), "melted");
            Assert.AreEqual(1.5f, OwnGrounds.ThrownAt(new Vector3(0f, 0f, 2f), 1f), 1e-5f, "thrown half again as far on it");
            Assert.AreEqual(1f, OwnGrounds.ThrownAt(new Vector3(0f, 0f, 4f), 1f), 1e-5f);
            Assert.AreEqual(1, OwnGrounds.Count(OwnGrounds.Sheet, 1f));
            for (int i = 0; i < OwnGrounds.Most + 3; i++) OwnGrounds.Lay(OwnGrounds.Glow, new Vector3(50f, 0f, 0f), 1f, 10f + i);
            Assert.AreEqual(OwnGrounds.Most, OwnGrounds.Count(OwnGrounds.Glow, 1f), "the one nearest its end gives way");
        }

        [Test]
        public void ALineIsCrossedOnlyWithinItsLengthAndARingsRimBothWays()
        {
            // The Drawn Line: six metres across your front, 1.5 m ahead.
            OwnGrounds.LayLane(OwnGrounds.SpearLine, new Vector3(-3f, 0f, 1.5f), Vector3.right, 6f, 0.5f, 5f);
            Assert.IsTrue(OwnGrounds.Across(OwnGrounds.SpearLine, Vector3.zero, new Vector3(0f, 0f, 3f), 1f), "a blow across it");
            Assert.IsTrue(OwnGrounds.Across(OwnGrounds.SpearLine, new Vector3(2f, 0f, 3f), new Vector3(-1f, 0f, 0f), 1f), "slanting, either way");
            Assert.IsFalse(OwnGrounds.Across(OwnGrounds.SpearLine, Vector3.zero, new Vector3(0f, 0f, 1f), 1f), "short of it");
            Assert.IsFalse(OwnGrounds.Across(OwnGrounds.SpearLine, new Vector3(5f, 0f, 0f), new Vector3(5f, 0f, 3f), 1f), "past its end");
            Assert.IsTrue(OwnGrounds.Under(OwnGrounds.SpearLine, new Vector3(2f, 0f, 1.7f), 1f), "standing on the line");
            // The Closed Ring: twelve metres across.
            OwnGrounds.Lay(OwnGrounds.StoneRing, Vector3.zero, 6f, 10f);
            Assert.IsTrue(OwnGrounds.AcrossRim(OwnGrounds.StoneRing, new Vector3(0f, 0f, 5.9f), new Vector3(0f, 0f, 6.1f), 1f), "out");
            Assert.IsTrue(OwnGrounds.AcrossRim(OwnGrounds.StoneRing, new Vector3(7f, 0f, 0f), new Vector3(5.8f, 0f, 0f), 1f), "in");
            Assert.IsFalse(OwnGrounds.AcrossRim(OwnGrounds.StoneRing, new Vector3(1f, 0f, 0f), new Vector3(2f, 0f, 0f), 1f), "inside it");
            Assert.IsFalse(OwnGrounds.AcrossRim(OwnGrounds.StoneRing, new Vector3(8f, 0f, 0f), new Vector3(9f, 0f, 0f), 1f), "outside it");
            Assert.IsFalse(OwnGrounds.AcrossRim(OwnGrounds.StoneRing, new Vector3(0f, 0f, 5.9f), new Vector3(0f, 0f, 6.1f), 11f), "fallen");
            OwnGrounds.End(OwnGrounds.StoneRing);
            Assert.AreEqual(0, OwnGrounds.Count(OwnGrounds.StoneRing, 1f));
        }

        [Test]
        public void StandingInYourOwnGroundIsAStateTheStatsAndMarksRead()
        {
            Assert.Greater(PlayerStats.WhenCount, (int)StatWhen.InOwnBlood);
            var states = PlayerUpgrades.OwnGroundStates;
            foreach (var when in new[] { StatWhen.OnOwnIce, StatWhen.OnWetSand, StatWhen.InOwnMist, StatWhen.InOwnGlow, StatWhen.InOwnBlood })
                Assert.AreEqual(1, states.Count(each => each.When == when), when + " has its ground");
            Assert.AreEqual(states.Length, states.Select(each => each.Ground).Distinct().Count());
            Assert.IsFalse(PlayerStats.IsBlow(StatWhen.OnOwnIce), "a state, not a kind of blow");
            Assert.IsFalse(Hazards.NamedUnder(Vector3.zero, OwnGrounds.WetSand), "no ground with no hazards in the scene");
        }

        [Test]
        public void BlackIceIsASheetThatThrowsFartherAndSlidesYourDodge()
        {
            var shape = Upgrade.ById(SpellBranch.ShapeId(IceTree.Hoarfrost)).Shapes.Single(each => each.Name == "Black Ice");
            Assert.AreEqual(OwnGrounds.Sheet, shape.Spell.Ground);
            Assert.AreEqual(1.5f, shape.Spell.Thrown, 1e-5f, "half again as far");
            var slide = shape.Stats.Single();
            Assert.AreEqual(PlayerStat.DodgeDistance, slide.Stat);
            Assert.AreEqual(StatWhen.OnOwnIce, slide.When);
            var stats = new PlayerStats();
            stats.AddLearned(slide);
            stats.SetWhen(StatWhen.OnOwnIce, true);
            Assert.AreEqual(2f, stats.Multiplier(PlayerStat.DodgeDistance), 1e-5f, "twice as far on the sheet");
            var copy = new SpellSpec();
            SpellChange.Copy(shape.Spell, copy);
            Assert.AreEqual(OwnGrounds.Sheet, copy.Ground, "the sheet goes with the spell's numbers");
            Assert.AreEqual(1.5f, copy.Thrown, 1e-5f);
        }

        [Test]
        public void TheClosedRingIsAWallThatStopsAndStrikesWhatIsDrivenIntoIt()
        {
            var ring = Upgrade.ById(EarthTree.TheClosedRing);
            var wall = ring.Effects.Single(each => each.Do == UpgradeAction.OwnGround);
            Assert.AreEqual(OwnGrounds.StoneRing, wall.Ground);
            Assert.AreEqual(6f, wall.Radius, 1e-5f, "twelve metres across");
            Assert.AreEqual(EffectAt.Self, wall.At);
            Assert.AreEqual(11, ring.Effects.Single(each => each.Do == UpgradeAction.Hazard).Count, "the eleven stones drawn");
            Assert.AreEqual(0f, ring.Effects.Single(each => each.Do == UpgradeAction.Hazard).Damage, "the stones strike nothing standing by them");
            var driven = ring.Effects.Single(each => each.On == UpgradeTrigger.StoppedAtStones);
            Assert.AreEqual(20f, driven.Damage, 1e-5f);
            Assert.AreEqual(35f, driven.Stagger, 1e-5f);
            Assert.AreNotEqual("its moment comes", TriggerWords.Of(UpgradeTrigger.StoppedAtStones));
            Assert.Greater(OwnGrounds.DrivenAtLeast, 2f, "a walk up to the stones is not a drive into them");
        }

        [Test]
        public void WetSandIsGroundOfYourOwnAndADodgeFromItSlidesFree()
        {
            var land = Upgrade.ById(Clover.Player.Trees.Duo.TheGroundGivesDuo.TheLandWasSea).Effects;
            Assert.IsFalse(land.Any(each => each.On == UpgradeTrigger.DodgeEnd), "not every dodge");
            Assert.IsTrue(land.All(each => each.On == UpgradeTrigger.DodgeStart && each.While == StatWhen.OnWetSand));
            Assert.AreEqual(1, land.Count(each => each.Do == UpgradeAction.FreeDodge));
            Assert.AreEqual(OwnGrounds.WetSand, land.Single(each => each.Do == UpgradeAction.Hazard).Ground, "its trail is wet sand too");
            Assert.AreEqual(OwnGrounds.WetSand, ReactionOf(AfflictionKind.Soaked, AfflictionKind.Tremor).Single(each => each.Do == UpgradeAction.Hazard).Ground, "the plain sand");
            Assert.AreEqual(OwnGrounds.WetSand, ShapeOf(Clover.Player.Trees.Duo.TheGroundGivesDuo.Shape, "Quicksand").Single(each => each.Do == UpgradeAction.Hazard).Ground);
            var breath = Upgrade.ById(Clover.Player.Trees.Duo.TheGroundGivesDuo.TheIslandBreathes).Effects.Single(each => each.Do == UpgradeAction.Hazard);
            Assert.AreEqual(OwnGrounds.WetSand, breath.Ground, "the out-breath");
            Assert.AreEqual(4f, breath.Seconds, 1e-5f);
            Assert.Greater(breath.Cooldown, 0f, "one place a cast");
        }

        [Test]
        public void BlackPitchCoatsSpreadsByBlowsAndGoesUpAllAtOnce()
        {
            var learn = Upgrade.ById(FireTree.BlackPitch);
            Assert.IsTrue(learn.Spell.Quiet, "the jar splashes no fire of its own");
            Assert.AreEqual(FireTree.PitchCoat, learn.Effects[0].State.Id, "the enemy faced is coated");
            var spread = learn.Effects[1];
            Assert.AreEqual(UpgradeTrigger.ConditionStruck, spread.On);
            Assert.AreEqual(StruckBy.Weapon, spread.By);
            Assert.AreEqual(2f, spread.EachWithin, 1e-5f);
            Assert.IsTrue(spread.EachOther, "to its neighbours");
            Assert.AreEqual(UpgradeAction.Bank, learn.Effects[2].Do, "each spreading blow counted");
            var lit = learn.Effects.Single(each => each.Do == UpgradeAction.LightCoat);
            Assert.AreEqual(AfflictionKind.Burn, lit.Spent, "any Burn lights it");
            Assert.AreEqual(FireTree.PitchCoat, lit.InState);
            Assert.AreEqual(FireTree.PitchCoat, lit.Condition);
            Assert.AreEqual(16f, PlayerUpgrades.CoatBurst(lit.Damage, lit.DamagePerSpent, 0, lit.Amount), 1e-5f);
            Assert.AreEqual(40f, PlayerUpgrades.CoatBurst(lit.Damage, lit.DamagePerSpent, 3, lit.Amount), 1e-5f, "8 more a spreading blow");
            Assert.AreEqual(70f, PlayerUpgrades.CoatBurst(lit.Damage, lit.DamagePerSpent, 20, lit.Amount), 1e-5f, "70 at most");
            Assert.AreEqual(3f, lit.Seconds, 1e-5f, "the floor burns 3 s");
            var shapes = Upgrade.ById(SpellBranch.ShapeId(FireTree.BlackPitch)).Shapes;
            var pool = shapes.Single(each => each.Name == "Pitch Pool");
            Assert.AreEqual(FireTree.PitchCoat, pool.Effects.Single().Inside.Id, "the pool coats who walks in it");
            var torch = shapes.Single(each => each.Name == "The Torch");
            Assert.IsTrue(ShapeDef.SetAside(torch, 0) && ShapeDef.SetAside(torch, 1), "nothing splashes");
            Assert.IsFalse(torch.Effects.Any(each => each.On == UpgradeTrigger.SpellCast && each.State != null && each.State.Id == FireTree.TorchLit), "not lit as it is thrown");
            Assert.IsTrue(torch.Effects.Any(each => each.On == UpgradeTrigger.AfflictionLaid && each.Spent == AfflictionKind.Burn && each.State != null && each.State.Id == FireTree.TorchLit), "lit when fire touches it");
        }

        [Test]
        public void LowMistIsYourGroundAndADodgeInItDosesAllInIt()
        {
            var shape = Upgrade.ById(SpellBranch.ShapeId(PoisonTree.Miasma)).Shapes.Single(each => each.Name == "Low Mist");
            Assert.AreEqual(OwnGrounds.LowMist, shape.Effects.Single(each => each.Do == UpgradeAction.Hazard).Ground);
            var dodge = shape.Effects.Single(each => each.On == UpgradeTrigger.DodgeStart);
            Assert.AreEqual(StatWhen.InOwnMist, dodge.While, "only a dodge begun in it");
            Assert.AreEqual(OwnGrounds.LowMist, dodge.ThroughGround, "every enemy in the sheet");
            Assert.AreEqual(AfflictionKind.Poison, dodge.Affliction);
        }

        [Test]
        public void RunningWaterIsABandThatRunsOnAndSeizesWhatItCrosses()
        {
            var band = ShapeOf(Clover.Player.Trees.Duo.SeizedDuo.Shape, "Running Water").Single();
            Assert.AreEqual(UpgradeAction.Hazard, band.Do);
            Assert.AreEqual(HazardShape.Band, band.Shape, "a band, not a ring");
            Assert.AreEqual(7f, band.Speed * band.Seconds, 1e-4f, "7 m on");
            Assert.AreEqual(Reactions.LiveWater, band.Ground, "live water: Grounded runs through it");
            Assert.AreEqual(0.5f, band.InsideDo.Single(each => each.Do == UpgradeAction.Hold).Seconds, 1e-5f);
            Assert.IsTrue(band.InsideDo.All(each => each.CooldownPerEnemy), "each once as it passes");
        }

        [Test]
        public void ADodgesPathIsWhereItsGroundAndDosesLie()
        {
            Assert.AreEqual(4.5f, DodgeRules.CarryTo(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 5.5f)), 1e-4f, "to arm's length");
            Assert.AreEqual(0f, DodgeRules.CarryTo(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 7f)), "beyond 6 m");
            Assert.AreEqual(0f, DodgeRules.CarryTo(Vector3.zero, Vector3.forward, new Vector3(4f, 0f, 0.5f)), "not toward it");
            Assert.AreEqual(0.5f, DodgeRules.CarryTo(Vector3.zero, Vector3.forward, new Vector3(0f, 0f, 1.2f)), 1e-4f, "already close");
            Assert.AreEqual(3, PlayerUpgrades.DodgePathSteps(5.5f, 2f));
            Assert.AreEqual(1, PlayerUpgrades.DodgePathSteps(0f, 2f));
            var seed = Upgrade.ById(Clover.Player.Trees.Duo.WindthrowDuo.SeedOnTheWind).Effects;
            Assert.IsTrue(seed.All(each => each.AlongDodge && each.On == UpgradeTrigger.DodgeEnd));
            var wake = Upgrade.ById(Clover.Player.Trees.Duo.CapsizeDuo.Wake).Effects;
            Assert.IsTrue(wake.All(each => each.AlongDodge && each.On == UpgradeTrigger.PerfectDodgeEnd && each.Shape == HazardShape.Line));
            Assert.IsTrue(Upgrade.ById(Clover.Player.Trees.Duo.CaughtLightDuo.SunOnTheSpray).Effects.All(each => each.On == UpgradeTrigger.PerfectDodgeEnd), "where it ends");
            var sight = ShapeOf(Clover.Player.Trees.Duo.GoldenFeathersDuo.Shape, "Eagle's Sight");
            Assert.IsTrue((sight[0].State.Flags & ConditionFlags.DrawsDodge) != 0, "the sight draws your dodge");
            Assert.AreEqual(10f, sight.Single(each => each.On == UpgradeTrigger.CarriedTo).Damage, 1e-5f, "the cut");
            var terms = DodgeTerms.Plain;
            Assert.AreEqual(0f, terms.Reach, "a plain roll goes its own way");
        }

        [Test]
        public void TastedEveryHerbTakesNoPoisonAndFeedsOnlyInACloud()
        {
            var herb = Upgrade.ById(PoisonTree.TastedEveryHerb);
            Assert.AreEqual(StatWhen.InCloud, herb.Stats.Single().When, "blows dose twice only in a cloud");
            Assert.AreEqual(StatWhen.InCloud, herb.Effects.Single().While, "mending only in a cloud");
            Assert.IsTrue(PlayerUpgrades.OwnGroundStates.Any(each => each.When == StatWhen.InCloud && each.Ground == OwnGrounds.Cloud));
            var on = new PlayerAfflictions();
            on.ImmuneTo = AfflictionKind.Poison;
            Assert.IsFalse(on.Lay(AfflictionKind.Poison, 2), "no poison takes");
            Assert.AreEqual(0, on.Stacks(AfflictionKind.Poison));
            Assert.IsTrue(on.Lay(AfflictionKind.Chill, 1), "the rest still do");
        }

        [Test]
        public void YourOwnGlowAndRingOfBloodHoldWhatOnlyWorksInsideThem()
        {
            var lamp = Upgrade.ById(LightTree.StandingLight);
            Assert.AreEqual(OwnGrounds.Glow, lamp.Spell.Ground, "the lamp's glow is yours");
            Assert.AreEqual(StatWhen.InOwnGlow, lamp.Effects.Single(each => each.On == UpgradeTrigger.PerfectGuard).While, "its burst only inside");
            var empower = Upgrade.ById(SpellBranch.EmpowerId(LightTree.StandingLight));
            Assert.AreEqual(2, empower.Stats.Count(each => each.When == StatWhen.InOwnGlow && each.Amount == 0.5f), "both moments half again as long, inside");
            Assert.IsTrue(Upgrade.ById(SpellBranch.MasterId(LightTree.StandingLight)).Effects.All(each => each.While == StatWhen.InOwnGlow));
            var row = Upgrade.ById(SpellBranch.ShapeId(LightTree.StandingLight)).Shapes.Single(each => each.Name == "Row of Lamps").Effects;
            Assert.AreEqual(OwnGrounds.Glow, row.Single(each => each.Do == UpgradeAction.Hazard).Ground);
            Assert.AreEqual(StatWhen.InOwnGlow, row.Single(each => each.On == UpgradeTrigger.PerfectGuard).While, "the guard made in the row");
            var furrow = Upgrade.ById(SpellBranch.ShapeId(LifeTree.DrawnFromTheWood)).Shapes.Single(each => each.Name == "Blood in the Furrow");
            Assert.IsTrue(ShapeDef.SetAside(furrow, 0) && ShapeDef.SetAside(furrow, 1), "the window does not go with you");
            Assert.IsTrue(furrow.Stats.All(each => each.When == StatWhen.InOwnBlood));
            Assert.AreEqual(OwnGrounds.Blood, furrow.Effects.Single().Ground);
        }

        [Test]
        public void RiptidesWaveCarriesTheFouledSlicksItCrosses()
        {
            var tide = Upgrade.ById(Clover.Player.Trees.Duo.FishPoisonDuo.TheTideBringsIt).Effects;
            var carry = tide.Single(each => each.Do == UpgradeAction.CarryGround);
            Assert.AreEqual(WaterTree.Riptide, carry.CastOf, "only Riptide's wave");
            Assert.AreEqual(Reactions.FouledSlick, carry.ThroughGround);
            Assert.AreEqual(Reactions.FouledSlick, ReactionOf(AfflictionKind.Poison, AfflictionKind.Soaked).Single(each => each.Do == UpgradeAction.Hazard).Ground, "the slick is named");
            Assert.IsTrue(tide.All(each => each.CastOf == WaterTree.Riptide), "not any spell of yours");
            Assert.IsTrue(SpellBranches.CastOf(WaterTree.Riptide, null, Elements.Water), "the kit's own cast is Riptide");
            Assert.IsFalse(SpellBranches.CastOf(WaterTree.Riptide, null, Elements.Fire), "Fire's kit is not");
            Assert.IsFalse(SpellBranches.CastOf("", null, Elements.Water));
            Assert.AreEqual(0, Hazards.Drift(Reactions.FouledSlick, Vector3.zero, Vector3.forward, 7f, 1.5f, Vector3.forward, 0.8f), "nothing to carry with no hazards in the scene");
        }

        [Test]
        public void ACounterThatMeetsNothingAndAStepThroughTwoAreMoments()
        {
            var blown = Upgrade.ById(Clover.Player.Trees.Duo.BreathTakenDuo.BlownThrough).Effects.Single(each => each.On == UpgradeTrigger.CounterMissed);
            Assert.IsTrue(blown.OwnSpell, "only its own counter");
            Assert.AreEqual(UpgradeAction.Push, blown.Do, "a small push round you");
            Assert.AreEqual(EffectAt.Self, blown.At);
            Assert.AreNotEqual("its moment comes", TriggerWords.Of(UpgradeTrigger.CounterMissed));
            var master = Upgrade.ById(SpellBranch.MasterId(LightTree.StepIntoLight)).Effects;
            Assert.IsTrue(master.All(each => each.StruckAtLeast == 2), "two in one line");
            Assert.IsTrue(PlayerUpgrades.CastStruckAtLeast(7, 7, 2, 2));
            Assert.IsFalse(PlayerUpgrades.CastStruckAtLeast(7, 7, 1, 2), "only one cut yet");
            Assert.IsFalse(PlayerUpgrades.CastStruckAtLeast(8, 7, 3, 2), "another cast's count");
            Assert.IsFalse(PlayerUpgrades.CastStruckAtLeast(0, 0, 3, 2), "no cast");
        }

        [Test]
        public void TheDrawnLineStandsAcrossYourFrontHoldingWhoCrosses()
        {
            var shape = Upgrade.ById(SpellBranch.ShapeId(LightTree.RingOfSpears)).Shapes.Single(each => each.Name == "The Drawn Line");
            Assert.IsTrue(shape.Spell.Quiet, "the spears do not drive into the enemy");
            var line = shape.Effects.Single(each => each.Do == UpgradeAction.OwnGround);
            Assert.AreEqual(OwnGrounds.SpearLine, line.Ground);
            Assert.IsTrue(line.AcrossFront);
            Assert.AreEqual(6f, line.Reach, 1e-5f);
            var spears = shape.Effects.Single(each => each.Do == UpgradeAction.Hazard);
            Assert.IsTrue(spears.AcrossFront);
            Assert.AreEqual(1f, spears.InsideDo.Single(each => each.Do == UpgradeAction.Hold).Seconds, 1e-5f, "held a second");
            Assert.IsTrue(spears.InsideDo.Any(each => each.Do == UpgradeAction.Afflict && each.Affliction == AfflictionKind.Marked));
        }

        [Test]
        public void ThingsAreRaisedBehindBesideOrAtTheEnemy()
        {
            Vector3 player = Vector3.zero, enemy = new Vector3(0f, 0f, 4f);
            Assert.AreEqual(new Vector3(0f, 0f, 5.1f), StandingRules.PlaceFor(StandingPlace.Behind, player, Vector3.forward, enemy, 1.1f), "at its back as you see it");
            Assert.AreEqual(1.1f, Mathf.Abs(StandingRules.PlaceFor(StandingPlace.Beside, player, Vector3.forward, enemy, 1.1f).x), 1e-4f, "to one side of your line");
            Assert.AreEqual(enemy, StandingRules.PlaceFor(StandingPlace.At, player, Vector3.forward, enemy, 1.1f));
            Assert.AreEqual(new Vector3(0f, 0f, 2f), StandingRules.PlaceFor(StandingPlace.Ahead, player, Vector3.forward, enemy, 2f));
        }

        [Test]
        public void AThingBlocksALineAndEnemiesWalkRoundIt()
        {
            int wall = StandingThings.Raise(StandingKind.IceWall, new Vector3(0f, 0f, 3f), Vector3.back, 6f, 60f, 0f);
            Assert.AreEqual(wall, StandingThings.FirstOnLine(new Vector3(0f, 0f, 6f), Vector3.zero), "a blow from its far side meets the wall");
            Assert.AreEqual(-1, StandingThings.FirstOnLine(new Vector3(4f, 0f, 6f), new Vector3(4f, 0f, 0f)), "past its end it does not");
            Assert.IsTrue(StandingRules.Touches(StandingThings.Get(wall), new Vector3(2.4f, 0f, 3.3f), 0f), "five metres wide along its line");
            Assert.IsFalse(StandingRules.Touches(StandingThings.Get(wall), new Vector3(0f, 0f, 4f), 0.3f), "and thin across it");
            Vector3 step = new Vector3(0f, 0f, -0.5f);
            Vector3 turned = StandingThings.SteerAround(new Vector3(0.2f, 0f, 3.9f), step, 0.4f);
            Assert.AreEqual(0f, turned.z, 0.05f, "it does not walk into the wall");
            Assert.Greater(Mathf.Abs(turned.x), 0.3f, "it goes along it instead");
            Assert.AreEqual(step, StandingThings.SteerAround(new Vector3(8f, 0f, 3.9f), step, 0.4f), "far from it, its step is its own");
        }

        [Test]
        public void ADrawingThingPullsEnemiesAndAnOwnersOldestCrumble()
        {
            int tree = StandingThings.Raise(StandingKind.HollowTrunk, Vector3.zero, Vector3.forward, 10f, 40f, 0f, 7, 0f, 5f);
            Assert.AreEqual(tree, StandingThings.DrawnTo(new Vector3(4f, 0f, 0f)), "within its pull");
            Assert.AreEqual(-1, StandingThings.DrawnTo(new Vector3(6f, 0f, 0f)), "beyond it");
            StandingThings.Raise(StandingKind.StonePillar, Vector3.right * 10f, Vector3.forward, 6f, 40f, 1f, 3);
            StandingThings.Raise(StandingKind.StonePillar, Vector3.right * 12f, Vector3.forward, 6f, 40f, 2f, 3);
            StandingThings.KeepAtMost(3, 1);
            Assert.AreEqual(1, StandingThings.Count(3));
            Assert.AreEqual(12f, StandingThings.Get(StandingThings.Nearest(Vector3.right * 11f, 5f, 3)).Position.x, 1e-4f, "the newest stays");
            Assert.AreEqual(1, StandingThings.Count(7), "another owner's are left alone");
        }

        [Test]
        public void BlowsOnAThingAreToldToTheMarkThatRaisedIt()
        {
            int stone = StandingThings.Raise(StandingKind.StonePillar, Vector3.zero, Vector3.forward, 6f, 40f, 0f, 11);
            StandingThings.Strike(stone, 10f, 1f, true, true);
            Assert.IsTrue(StandingThings.TakeNews(out var news));
            Assert.IsTrue(news.Heavy && news.ByPlayer);
            Assert.AreEqual(11, news.Thing.Owner);
            Assert.IsFalse(StandingThings.TakeNews(out _), "each blow told once");
            var shape = Upgrade.ById(SpellBranch.ShapeId(EarthTree.StoneRise)).Shapes.Single(each => each.Name == "Raised Stone");
            var raise = shape.Effects.Single(each => each.Do == UpgradeAction.Raise);
            Assert.AreEqual(StandingKind.StonePillar, raise.Raises);
            Assert.AreEqual(StandingPlace.Beside, raise.RaiseAt);
            Assert.AreEqual(6f, raise.Seconds, 1e-5f);
            var cone = shape.Effects.Single(each => each.Do == UpgradeAction.Cone);
            Assert.AreEqual(UpgradeTrigger.StandingStruck, cone.On);
            Assert.IsTrue(PlayerUpgrades.AnswersStanding(cone, cone.Owner.Index, true), "your heavy blow on your own pillar");
            Assert.IsFalse(PlayerUpgrades.AnswersStanding(cone, cone.Owner.Index, false), "not a light one");
            Assert.IsFalse(PlayerUpgrades.AnswersStanding(cone, cone.Owner.Index + 1, true), "not another mark's thing");
            Assert.IsTrue(shape.Effects.Any(each => each.Do == UpgradeAction.BreakStanding), "and it breaks");
        }

        [Test]
        public void TheReactionsRaiseTheirStonesMoundsAndTrunks()
        {
            var anvil = Upgrade.ById(AfflictionPairs.Id(AfflictionPairs.Index(AfflictionKind.Tremor, AfflictionKind.Sundered))).Effects;
            Assert.IsTrue(anvil.Any(each => each.Do == UpgradeAction.Raise && each.Raises == StandingKind.AnvilStone && each.RaiseAt == StandingPlace.Behind && each.Seconds == 5f), "the anvil at its back for 5 s");
            var heave = Upgrade.ById(AfflictionPairs.Id(AfflictionPairs.Index(AfflictionKind.Chill, AfflictionKind.Tremor))).Effects;
            Assert.IsTrue(heave.Any(each => each.Do == UpgradeAction.Raise && each.Raises == StandingKind.RimedMound && each.Seconds == 6f), "the rimed mound for 6 s");
            var girdled = Upgrade.ById(AfflictionPairs.Id(AfflictionPairs.Index(AfflictionKind.Seeded, AfflictionKind.Cursed))).Effects;
            Assert.IsTrue(girdled.Any(each => each.On == UpgradeTrigger.ConditionDied && each.Do == UpgradeAction.Raise && each.Raises == StandingKind.HollowTrunk && each.Seconds == 8f), "the trunk where it dies girdled");
            Assert.IsTrue(girdled.Any(each => each.On == UpgradeTrigger.ConditionDied && each.Do == UpgradeAction.Hazard && each.BitesLay == AfflictionKind.Cursed && each.Affliction == AfflictionKind.Seeded), "Seeding and Cursing what touches it");
            var hammer = Upgrade.ById(ForgeTree.HammerAndAnvil).Effects.Single(each => each.Do == UpgradeAction.Echo);
            Assert.AreEqual(1.5f, hammer.StoneAtBack, 1e-5f, "stone at its back, not an enemy already shaken");
            Assert.AreEqual(AfflictionKind.None, hammer.Needs);
            Assert.IsTrue(StandingRules.AtItsBack(new Standing { Active = true, Kind = StandingKind.AnvilStone, Position = new Vector3(0f, 0f, 5.1f), Forward = Vector3.back },
                Vector3.zero, new Vector3(0f, 0f, 4f), 1.5f + StandingRules.Reach(StandingKind.AnvilStone)));
        }

        [Test]
        public void WinterWallStandsWithHealthAndTheCrackedWallBurstsOnceStruck()
        {
            var learn = Upgrade.ById(IceTree.WinterWall);
            Assert.AreEqual(60f, learn.Spell.WallHealth, 1e-5f);
            Assert.AreEqual(1, learn.Spell.TouchStacks, "its touch chills");
            var cracked = Upgrade.ById(SpellBranch.ShapeId(IceTree.WinterWall)).Shapes.Single(each => each.Name == "Cracked Wall");
            Assert.AreEqual(1.5f, cracked.Change.BurstAfterStruck, 1e-5f);
            Assert.AreEqual(0f, cracked.Change.Seconds, 1e-5f, "no stand-in three-second burst");
            Assert.AreEqual(StandingKind.IceWall, WallRules.KindFor(5f));
            Assert.AreEqual(StandingKind.ShieldWall, WallRules.KindFor(3f));
            int wall = StandingThings.Raise(StandingKind.IceWall, Vector3.zero, Vector3.forward, 6f, 60f, 0f, -1, 1.5f);
            Assert.IsTrue(WallRules.Stands(StandingThings.Get(wall), Vector3.zero));
            StandingThings.Strike(wall, 5f, 2f);
            StandingThings.Tick(3f, out int early);
            Assert.AreEqual(-1, early, "not yet");
            StandingThings.Tick(3.6f, out int burst);
            Assert.AreEqual(wall, burst, "a second and a half after the first enemy blow");
            Assert.IsFalse(WallRules.Stands(StandingThings.Get(wall), Vector3.zero), "and the wall bursts with it");
        }

        [Test]
        public void FrostHeavesShapesLeaveTheMoundAndFrozenFootingNeedsOne()
        {
            var shapes = Upgrade.ById(Clover.Player.Trees.Duo.FrostHeaveDuo.Shape).Shapes;
            Assert.IsTrue(shapes.All(shape => shape.Effects.Any(each => each.Do == UpgradeAction.Raise && each.Raises == StandingKind.RimedMound)), "both shapes heave the mound up");
            Assert.IsTrue(shapes.Single(each => each.Name == "Pingo").Effects.Any(each => each.Do == UpgradeAction.BreakStanding), "Pingo's mound bursts with its core");
            var footing = Upgrade.ById(Clover.Player.Trees.Duo.FrostHeaveDuo.FrozenFooting).Effects.Single();
            Assert.AreEqual(2f, footing.NearStanding, 1e-5f);
            Assert.AreEqual(StandingKind.RimedMound, footing.Raises);
            Assert.AreEqual(AfflictionKind.None, footing.Needs, "no stand-in: the attacker need carry nothing");
            StandingThings.Raise(StandingKind.RimedMound, Vector3.zero, Vector3.forward, 6f, 30f, 0f);
            Assert.IsTrue(StandingThings.NearKind(new Vector3(2.5f, 0f, 0f), 2f, StandingKind.RimedMound), "within two metres of its edge");
            Assert.IsFalse(StandingThings.NearKind(new Vector3(3f, 0f, 0f), 2f, StandingKind.RimedMound));
            Assert.IsFalse(StandingThings.NearKind(Vector3.zero, 2f, StandingKind.AnvilStone), "a mound, not any stone");
        }

        [Test]
        public void PiecesLeaveAnEnemyInAFanARingOrAtTheNearest()
        {
            var ways = new Vector3[12];
            Assert.AreEqual(3, PieceRules.Ways(PieceAim.Fan, Vector3.forward, 3, 40f, ways));
            Assert.AreEqual(20f, Vector3.Angle(ways[0], Vector3.forward), 0.01f, "a fan forty degrees across");
            Assert.AreEqual(0f, Vector3.Angle(ways[1], Vector3.forward), 0.01f);
            Assert.AreEqual(5, PieceRules.Ways(PieceAim.Ring, Vector3.forward, 5, 0f, ways));
            Assert.AreEqual(72f, Vector3.Angle(ways[0], ways[1]), 0.01f, "five blades all round");
            Assert.AreEqual(Vector3.right, PieceRules.Downwind(Vector3.forward, Vector3.right, 9f, 10f), "the way it was last shoved");
            Assert.AreEqual(Vector3.forward, PieceRules.Downwind(Vector3.forward, Vector3.right, 1f, 10f), "a shove long past: away from you");
            var feathers = Upgrade.ById(AfflictionPairs.Id(AfflictionPairs.Index(AfflictionKind.Marked, AfflictionKind.Unsteady))).Effects.Single();
            Assert.AreEqual(UpgradeAction.Pieces, feathers.Do);
            Assert.AreEqual(PieceAim.Fan, feathers.Thrown);
            Assert.AreEqual(3, feathers.Count);
            Assert.AreEqual(40f, feathers.Arc, 1e-5f);
            Assert.AreEqual(AfflictionKind.Unsteady, feathers.BitesLay, "a Mark and an Unsteady on the first each touches");
            var scale = Upgrade.ById(ForgeTree.Shape).Shapes.Single(each => each.Name == "Hammer Scale").Effects.Single();
            Assert.AreEqual(PieceAim.AtNearest, scale.Thrown);
            Assert.AreEqual(4, scale.Count, "four flakes, each its own");
            var harvest = Upgrade.ById(Clover.Player.Trees.Duo.IronThornsDuo.Shape).Shapes.Single(each => each.Name == "Iron Harvest").Effects.Where(each => each.Do == UpgradeAction.Pieces).ToArray();
            Assert.AreEqual(2, harvest.Length, "as it dies or as the 6 s end");
            Assert.IsTrue(harvest.All(each => each.Thrown == PieceAim.Ring && each.Count == 5));
            var hail = Upgrade.ById(Clover.Player.Trees.Duo.SleetChargeDuo.ChargedHail).Effects.Single();
            Assert.AreEqual(UpgradeAction.Pieces, hail.Do, "the shatter's own shards");
            Assert.AreEqual(AfflictionKind.Static, hail.Affliction);
            var prism = Upgrade.ById(Clover.Player.Trees.Duo.MockSunsDuo.Shape).Shapes.Single(each => each.Name == "Prism").Effects.Single(each => each.Do == UpgradeAction.Pieces);
            Assert.AreEqual(PieceAim.FarCone, prism.Thrown);
            Assert.AreEqual(6f, prism.Reach, 1e-5f);
            Assert.AreEqual(30f, prism.Arc, 1e-5f);
            var sand = Upgrade.ById(Clover.Player.Trees.Duo.LoganStoneDuo.Shape).Shapes.Single(each => each.Name == "Sandblast").Effects.Single(each => each.Do == UpgradeAction.Pieces);
            Assert.AreEqual(PieceAim.FarCone, sand.Thrown);
            Assert.AreEqual(60f, sand.Arc, 1e-5f);
        }

        [Test]
        public void PickupsAreTakenByWalkingOverThemOrLeftToLie()
        {
            int herb = Pickups.Drop(PickupKind.Herb, Vector3.zero, 6f, 5, 0f, 3);
            Assert.AreEqual(8f, Pickups.Get(Pickups.Drop(PickupKind.FrozenSeed, Vector3.right * 9f, 0f, 6, 0f)).Until, 1e-5f, "the plain eight seconds");
            Assert.IsFalse(Pickups.TakeAt(new Vector3(1.2f, 0f, 0f), out _), "a metre and a bit away: not taken");
            Assert.IsTrue(Pickups.TakeAt(new Vector3(0.8f, 0f, 0f), out var taken), "walked over");
            Assert.AreEqual(PickupKind.Herb, taken.Kind);
            Assert.AreEqual(5, taken.Owner);
            Assert.IsFalse(Pickups.Get(herb).Active, "taken once");
            for (int i = 0; i < 4; i++) Pickups.Drop(PickupKind.Herb, Vector3.forward * (3f + i), 6f + i, 5, 0f, 3);
            Assert.AreEqual(3, Pickups.Count(5), "three herbs at most");
            Assert.IsFalse(Pickups.TakeLeft(5f, out _), "not yet");
            Assert.IsTrue(Pickups.TakeLeft(7.5f, out var left), "left to lie until its time ran out");
            Assert.AreEqual(PickupKind.Herb, left.Kind);
            Assert.AreEqual(2, Pickups.DrawAll(PickupKind.Herb), "the whole mountain comes to you");
            Pickups.StepDrawn(Vector3.zero, 10f);
            Assert.IsTrue(Pickups.TakeAt(Vector3.zero, out var drawn) && drawn.Drawn, "drawn across the floor and taken");
        }

        [Test]
        public void TheMarksDropTheirHerbsLiliesAndSeeds()
        {
            var herb = Upgrade.ById(AfflictionPairs.Id(AfflictionPairs.Index(AfflictionKind.Seeded, AfflictionKind.Marked)));
            var drop = herb.Effects.Single(each => each.Do == UpgradeAction.DropPickup);
            Assert.AreEqual(PickupKind.Herb, drop.Drops);
            Assert.AreEqual(StandingPlace.Before, drop.RaiseAt, "at its feet on your side");
            Assert.AreEqual(3, drop.Limit);
            var picked = new System.Collections.Generic.List<UpgradeEffect>();
            Assert.AreEqual(2, PlayerUpgrades.AnswersOf(drop, UpgradeTrigger.PickedUp, picked), "heal 8 and the next blow critical");
            Assert.IsTrue(picked.Any(each => each.Do == UpgradeAction.Heal && each.Amount == 8f));
            Assert.IsTrue(picked.Any(each => each.Do == UpgradeAction.CriticalNext));
            Assert.AreEqual(2, PlayerUpgrades.AnswersOf(drop, UpgradeTrigger.PickupLeft, picked), "left, it flowers: Marked and Seeded");
            var longDay = Upgrade.ById(Clover.Player.Trees.Duo.ShiningHerbDuo.LongDay).ReactionChanges.Single(each => each.Shape == -1);
            Assert.AreEqual(9f, longDay.Seconds, 1e-5f, "Long Day: the herb lies 9 s");
            var lilies = Upgrade.ById(AfflictionPairs.Id(AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Seeded))).Effects;
            var lilyFire = lilies.Single(each => each.Do == UpgradeAction.DropPickup);
            Assert.AreEqual(PickupKind.LilyFire, lilyFire.Drops);
            Assert.AreEqual(2, PlayerUpgrades.AnswersOf(lilyFire, UpgradeTrigger.PickedUp, picked));
            Assert.IsTrue(picked.Any(each => each.Do == UpgradeAction.CoatBlade && each.Affliction == AfflictionKind.Burn && each.Count == 1), "one charged blow of Burn");
            var pair = Upgrade.ById(Clover.Player.Trees.Duo.FirebloomDuo.Shape).Shapes.Single(each => each.Name == "Between Two Fires");
            var gap = pair.Effects.Single(each => each.Do == UpgradeAction.DropPickup);
            Assert.AreEqual(2, PlayerUpgrades.AnswersOf(gap, UpgradeTrigger.PickedUp, picked), "the shape's own answers, found beside its drop");
            Assert.IsTrue(picked.Any(each => each.Do == UpgradeAction.CoatBlade && each.Count == 2), "two blows of Burn");
            var seed = Upgrade.ById(Clover.Player.Trees.Duo.WinterSeedDuo.SeedUnderSnow).Effects;
            Assert.AreEqual(PickupKind.FrozenSeed, seed.Single(each => each.Do == UpgradeAction.DropPickup).Drops);
            Assert.IsFalse(seed.Any(each => each.On == UpgradeTrigger.Kill && each.Do == UpgradeAction.Heal), "no stand-in burst as it dies");
            var mountain = Upgrade.ById(Clover.Player.Trees.Duo.ShiningHerbDuo.TheWholeMountain).Effects;
            Assert.IsTrue(mountain.All(each => each.On == UpgradeTrigger.PickedUp && each.AnyDrops && each.Drops == PickupKind.Herb), "any herb picked");
            Assert.IsTrue(mountain.Any(each => each.Do == UpgradeAction.DrawPickups));
            Assert.IsTrue(mountain.Any(each => each.Do == UpgradeAction.Heal && each.SpentAtLeast == 1), "each drawn one heals twice");
        }

        [Test]
        public void GroundKeepsActingOnWhoeverStandsInIt()
        {
            Assert.AreEqual(1.5f * 0.25f * 1.5f, GroundRules.PullStep(1.5f, 0.25f), 1e-5f, "a pull of 1.5 m a second, a bite every quarter second");
            Assert.AreEqual(2f, GroundRules.MendStep(2f, 1f), 1e-5f, "2 health a second, a bite a second");
            Assert.AreEqual(1.25f, GroundRules.StateSeconds(1f), 1e-5f, "held a little past the next bite, so it lasts while they stay");
            Assert.IsTrue(GroundRules.Within(new Vector3(1.4f, 0f, 0f), Vector3.zero, 1.5f));
            Assert.IsFalse(GroundRules.Within(new Vector3(1.6f, 0f, 0f), Vector3.zero, 1.5f));
            var ash = Upgrade.ById(SpellBranch.ShapeId(Upgrade.MeteorLeap)).Shapes.Single(each => each.Name == "Ash Fall").Effects.Single(each => each.Do == UpgradeAction.Hazard);
            Assert.IsNotNull(ash.Inside, "an enemy that walks into the ash later is held too");
            Assert.AreNotEqual(0, (int)(ash.Inside.Flags & ConditionFlags.BalanceHeld));
            var pit = Upgrade.ById(SpellBranch.ShapeId(DarkTree.Grasp)).Shapes.Single(each => each.Name == "The Pit");
            Assert.AreEqual(1f, pit.Spell.Pull, 1e-5f, "drawn slowly to its middle");
            var under = Upgrade.ById(Clover.Player.Trees.Duo.DrownedWeightDuo.Shape).Shapes.Single(each => each.Name == "Drawn Under").Effects;
            Assert.IsTrue(under.Any(each => each.Do == UpgradeAction.Hazard && each.Pull == 1.5f), "the pool draws the others in at 1.5 m a second");
            Assert.IsFalse(under.Any(each => each.Do == UpgradeAction.Pull), "not a pull of 2 m at once");
            var rice = Upgrade.ById(Clover.Player.Trees.Duo.LightningFlowerDuo.Shape).Shapes.Single(each => each.Name == "The Rice Wakes").Effects.Single(each => each.Do == UpgradeAction.Hazard);
            Assert.AreEqual(2f, rice.Mends, 1e-5f);
            var bed = Upgrade.ById(Clover.Player.Trees.Duo.FirebloomDuo.Shape).Shapes.Single(each => each.Name == "Ash Bed").Effects.Single(each => each.Do == UpgradeAction.Hazard);
            Assert.AreEqual(2f, bed.Mends, 1e-5f);
        }

        private static UpgradeEffect[] ReactionOf(AfflictionKind one, AfflictionKind other) =>
            Upgrade.ById(AfflictionPairs.Id(AfflictionPairs.Index(one, other))).Effects;

        private static UpgradeEffect[] ShapeOf(string mark, string shape) => Upgrade.ById(mark).Shapes.Single(each => each.Name == shape).Effects;

        [Test]
        public void GroundActsOnWhoeverWadesInLater()
        {
            var answer = new UpgradeEffect { On = UpgradeTrigger.InGround, Condition = Reactions.LightNet };
            Assert.IsTrue(PlayerUpgrades.AnswersGround(answer, Reactions.LightNet));
            Assert.IsFalse(PlayerUpgrades.AnswersGround(answer, Reactions.Damp), "only the ground it names");
            Assert.IsFalse(PlayerUpgrades.AnswersGround(answer, ""));
            Assert.IsFalse(new UpgradeEffect { Do = UpgradeAction.Hazard }.ActsAsGround, "plain ground bites and is done");
            Assert.IsTrue(new UpgradeEffect { Do = UpgradeAction.Hazard, Ground = "x" }.ActsAsGround);

            // The sand pool slows the others that wade in, never the sunk one twice over.
            var sand = ReactionOf(AfflictionKind.Soaked, AfflictionKind.Tremor).Single(each => each.Do == UpgradeAction.Hazard);
            Assert.IsTrue(sand.InsideSpares);
            Assert.AreEqual(0.7f, sand.InsideDo.Single().State.Pace, 1e-5f, "30% slower in the sand");
            Assert.AreEqual(2f, sand.Radius * 2f, 1e-5f, "2 m across");
            // The net catches whoever walks onto it later, and is told as the net.
            var net = ReactionOf(AfflictionKind.Soaked, AfflictionKind.Marked).Single(each => each.Do == UpgradeAction.Hazard);
            Assert.AreEqual(0.75f, net.Inside.Pace, 1e-5f);
            Assert.AreEqual(Reactions.LightNet, net.Ground);
            // The damp lies in pools under it and under three shaken ones near; they dose and slow what stands in them.
            var damp = ReactionOf(AfflictionKind.Tremor, AfflictionKind.Poison);
            Assert.AreEqual(2, damp.Count(each => each.Do == UpgradeAction.Hazard));
            var others = damp.Single(each => each.EachWithin > 0f);
            Assert.AreEqual(7f, others.EachWithin, 1e-5f);
            Assert.AreEqual(3, others.Count);
            Assert.AreEqual(AfflictionKind.Tremor, others.OnlyCarrying);
            Assert.IsTrue(damp.All(each => each.Inside != null && each.Inside.Is == Reactions.Damp && each.Inside.Pace == 0.8f && each.Ground == Reactions.Damp));
            Assert.IsFalse(damp.Any(each => each.Do == UpgradeAction.Condition), "the damp no longer goes with the enemies it reached");
            // Whatever in the slick is dosed four deep floats up, once a slick.
            var slick = ReactionOf(AfflictionKind.Poison, AfflictionKind.Soaked).Single(each => each.Do == UpgradeAction.Hazard);
            Assert.IsTrue(slick.InsideSpares, "the reacting one floated as it reacted");
            Assert.IsTrue(slick.InsideDo.Any(each => each.Do == UpgradeAction.Lift && each.NeedsAtLeast == 4 && each.CooldownPerEnemy && each.Cooldown == 5f));
            Assert.IsTrue(slick.InsideDo.Any(each => each.Do == UpgradeAction.Condition && each.State.Id == Reactions.Floating));
            Assert.IsTrue(slick.InsideDo.All(each => each.Slot >= 0 && each.Owner == Upgrade.ById(AfflictionPairs.Id(AfflictionPairs.Index(AfflictionKind.Poison, AfflictionKind.Soaked)))));

            // The marks: walk-ins are caught, opened, slowed, thrown flat, blinded and floated as those there at first are.
            Assert.AreEqual(Reactions.LightNet, ShapeOf(Clover.Player.Trees.Duo.CaughtLightDuo.Shape, "Thrown Wide").Single(each => each.Do == UpgradeAction.Hazard).Inside.Is);
            var seen = Upgrade.ById(Clover.Player.Trees.Duo.CaughtLightDuo.SeenInTheWater).Effects;
            Assert.IsTrue(seen.Any(each => each.On == UpgradeTrigger.InGround && each.Condition == Reactions.LightNet && (each.State.Flags & ConditionFlags.TakesCritical) != 0));
            var black = ShapeOf(Clover.Player.Trees.Duo.ChokedampDuo.Shape, "Blackdamp").Single(each => each.Do == UpgradeAction.Hazard);
            Assert.AreEqual(0.8f, black.Inside.Pace, 1e-5f);
            Assert.IsTrue(Upgrade.ById(Clover.Player.Trees.Duo.ChokedampDuo.TheLampGoesOut).Effects.Single().On == UpgradeTrigger.InGround, "the lamp goes out for those standing in the damp");
            var plate = ShapeOf(Clover.Player.Trees.Duo.ColdShortDuo.Shape, "Shed Plate").Single(each => each.Do == UpgradeAction.Hazard).InsideDo.Single();
            Assert.AreEqual(ConditionFlags.Down, plate.State.Flags);
            Assert.IsTrue(plate.CooldownPerEnemy);
            var drift = ShapeOf(Clover.Player.Trees.Duo.LoganStoneDuo.Shape, "Drifted Over").Single(each => each.Do == UpgradeAction.Hazard);
            Assert.AreEqual(0.6f, drift.Inside.Pace, 1e-5f);
            var steam = ShapeOf(Clover.Player.Trees.Duo.SteamDuo.Shape, "Blind Steam");
            Assert.IsTrue((steam.Single(each => each.Do == UpgradeAction.Hazard).Inside.Flags & ConditionFlags.SightShort) != 0);
            Assert.IsFalse(steam.Any(each => each.Do == UpgradeAction.Condition), "blind only while in the cloud");
            Assert.AreEqual(2, ShapeOf(Clover.Player.Trees.Duo.FishPoisonDuo.Shape, "Rock Pool").Single(each => each.Do == UpgradeAction.Hazard).InsideDo[0].NeedsAtLeast);
            Assert.AreEqual(4, ShapeOf(Clover.Player.Trees.Duo.FishPoisonDuo.Shape, "The Ebb Takes It").Single(each => each.Do == UpgradeAction.Hazard).InsideDo[0].NeedsAtLeast);
            var quick = ShapeOf(Clover.Player.Trees.Duo.TheGroundGivesDuo.Shape, "Quicksand");
            Assert.AreEqual(1f, quick.Single(each => each.Do == UpgradeAction.Hazard).Pull, 1e-5f, "the pool draws in a metre a second");
            Assert.AreEqual(3f, quick.Single(each => each.Do == UpgradeAction.Hazard).Radius * 2f, 1e-5f, "3 m across");
            Assert.IsFalse(quick.Any(each => each.Do == UpgradeAction.Pull), "not a tug once a second from the sunk one");
        }

        [Test]
        public void LanesCarryAndFieldsDrawInAndTheIceGivesUnderFollowers()
        {
            Vector3 from = Vector3.zero, way = Vector3.forward;
            Assert.IsTrue(GroundRules.OnLane(new Vector3(0.9f, 0f, 5f), from, way, 8f, 1f), "within a metre of its line");
            Assert.IsFalse(GroundRules.OnLane(new Vector3(1.1f, 0f, 5f), from, way, 8f, 1f), "beside it");
            Assert.IsFalse(GroundRules.OnLane(new Vector3(0f, 0f, 9.5f), from, way, 8f, 1f), "past its end");
            Assert.IsTrue(GroundRules.OnLane(new Vector3(0f, 0f, -0.5f), from, way, 8f, 1f), "its ends are rounded by its width");

            var spec = new SpellSpec { Pull = 1.2f };
            new SpellChange { Pull = 3f }.ApplyTo(spec);
            Assert.AreEqual(3f, spec.Pull, 1e-5f);
            new SpellChange { NoPull = true }.ApplyTo(spec);
            Assert.AreEqual(0f, spec.Pull, 1e-5f);
            Assert.IsFalse(new SpellChange { NoPull = true }.IsEmpty);

            // Whirlpool draws in at 1.2 m a second, Maelstrom at 3; Rip Current draws nothing in and its foam carries down the lane.
            Assert.AreEqual(1.2f, Upgrade.ById(WaterTree.Whirlpool).Spell.Pull, 1e-5f);
            var whirl = Upgrade.ById(SpellBranch.ShapeId(WaterTree.Whirlpool)).Shapes;
            Assert.AreEqual(3f, whirl.Single(each => each.Name == "Maelstrom").Change.Pull, 1e-5f);
            var rip = whirl.Single(each => each.Name == "Rip Current");
            Assert.IsTrue(rip.Change.NoPull);
            var foam = rip.Effects.Single(each => each.Do == UpgradeAction.Hazard);
            Assert.AreEqual(2.5f, foam.Carry, 1e-5f, "2.5 m a second");
            Assert.AreEqual(6f, foam.Seconds, 1e-5f);
            Assert.AreEqual(2f, foam.Radius * 2f, 1e-5f, "2 m wide");
            Assert.AreEqual(1.3f, foam.Inside.Scale, 1e-5f, "blows on the carried break balance 30% harder");
            Assert.IsFalse(rip.Effects.Any(each => each.Do == UpgradeAction.Push));

            // Thin Ice: every one on the lane cracks through once, a god slowed rather than held.
            var thin = Upgrade.ById(SpellBranch.ShapeId(IceTree.SkiRun)).Shapes.Single(each => each.Name == "Thin Ice").Effects.Single(each => each.Do == UpgradeAction.Hazard);
            Assert.AreEqual(5f, thin.Seconds, 1e-5f);
            Assert.IsTrue(thin.InsideDo.Any(each => each.Do == UpgradeAction.Hold && (each.Ranks & EnemyRank.God) == 0 && each.CooldownPerEnemy));
            Assert.AreEqual(0.7f, thin.InsideDo.Single(each => each.Ranks == EnemyRank.God).State.Pace, 1e-5f);
            Assert.IsTrue(thin.InsideDo.Any(each => each.Do == UpgradeAction.Condition && each.State.Id == IceTree.ThroughTheIce));
            // Ink blinds only while they stand in it.
            var ink = Upgrade.ById(SpellBranch.ShapeId(WaterTree.DeepCrack)).Shapes.Single(each => each.Name == "Ink").Effects;
            Assert.AreEqual(3f, ink.Single(each => each.Do == UpgradeAction.Hazard).Inside.Sight, 1e-5f);
            Assert.IsFalse(ink.Any(each => each.Do == UpgradeAction.Condition));
            // To the Blade's chain trips each crosser flat.
            var chain = Upgrade.ById(SpellBranch.ShapeId(MetalTree.ToTheBlade)).Shapes.Single(each => each.Name == "Chain").Effects.Single();
            Assert.AreEqual(ConditionFlags.Down, chain.InsideDo.Single().State.Flags);
            // Hot Wind's gust carries everything on its lane 2 m.
            var hot = Upgrade.ById(Clover.Player.Trees.Duo.CarriedEmbersDuo.Shape).Shapes.Single(each => each.Name == "Hot Wind").Effects;
            Assert.AreEqual(2f, hot.Single(each => each.Do == UpgradeAction.Lane).Carry, 1e-5f);
            Assert.IsFalse(hot.Any(each => each.Do == UpgradeAction.Push), "not the enemy alone");
        }

        [Test]
        public void EarthlightBreaksUpOnlyAlongItsSeams()
        {
            Vector3 first = Vector3.forward;
            Assert.IsTrue(GroundRules.OnStar(new Vector3(0f, 0f, 2.5f), Vector3.zero, 6, 3f, GroundRules.SeamHalf, first), "on the first seam");
            Assert.IsTrue(GroundRules.OnStar(GroundRules.SeamWay(first, 1, 6) * 2f, Vector3.zero, 6, 3f, GroundRules.SeamHalf, first), "on the second");
            Assert.IsFalse(GroundRules.OnStar(new Vector3(Mathf.Sin(Mathf.PI / 6f), 0f, Mathf.Cos(Mathf.PI / 6f)) * 2.5f, Vector3.zero, 6, 3f, GroundRules.SeamHalf, first), "between two seams, inside the old disc");
            Assert.IsFalse(GroundRules.OnStar(new Vector3(0f, 0f, 3.7f), Vector3.zero, 6, 3f, GroundRules.SeamHalf, first), "past a seam's end");
            Assert.AreEqual(-1f, GroundRules.SeamWay(first, 3, 6).z, 1e-4f, "the fourth of six runs the other way");
            Assert.AreEqual(Mathf.Sin(Mathf.PI / 3f), GroundRules.SeamWay(first, 1, 6).x, 1e-4f, "the second turned 60 degrees toward +x, as a yaw turns");

            var plain = ReactionOf(AfflictionKind.Tremor, AfflictionKind.Marked).Single(each => each.Do == UpgradeAction.Burst);
            Assert.AreEqual(6, plain.Seams);
            Assert.AreEqual(3f, plain.Radius, 1e-5f, "each seam 3 m long");
            var lit = ShapeOf(Clover.Player.Trees.Duo.EarthlightDuo.Shape, "Lit Seam").Where(each => each.Do == UpgradeAction.Burst).ToList();
            Assert.AreEqual(2, lit.Count);
            Assert.IsTrue(lit.All(each => each.Seams == 6), "the light and its flash run along the seams");
        }

        [Test]
        public void CoalSeamFlaresEveryBurningEnemyOnItAsItEnds()
        {
            var seam = ShapeOf(Clover.Player.Trees.Duo.SmoulderDuo.Shape, "Coal Seam").Single(each => each.Do == UpgradeAction.Hazard);
            Assert.IsTrue(seam.ActsAsGround, "it is told as it runs out");
            Assert.AreEqual(3, seam.InsideAtEnd.Length);
            Assert.IsTrue(seam.InsideAtEnd.All(each => each.Needs == AfflictionKind.Burn), "only the burning flare");
            Assert.AreEqual(UpgradeAction.Spend, seam.InsideAtEnd[0].Do, "the Burn spent first, so the burst counts it");
            var burst = seam.InsideAtEnd[1];
            Assert.IsTrue(burst.Do == UpgradeAction.Burst && burst.PerStack && burst.Damage == 10f && burst.Radius == 3f);
            Assert.IsTrue(seam.InsideAtEnd.All(each => each.Slot >= 0 && each.Owner == Upgrade.ById(Clover.Player.Trees.Duo.SmoulderDuo.Shape)));
            Assert.IsTrue(new UpgradeEffect { InsideAtEnd = new UpgradeEffect[0] }.ActsAsGround);
        }

        [Test]
        public void YourHeavyBlowBurstsEveryPuddleTheSoreWept()
        {
            var sore = Upgrade.ById(SpellBranch.ShapeId(PoisonTree.Blister)).Shapes.Single(each => each.Name == "Weeping Sore").Effects;
            var puddle = sore.Single(each => each.Do == UpgradeAction.Hazard);
            var burst = sore.Single(each => each.Do == UpgradeAction.BurstGround);
            Assert.AreSame(puddle, burst.BurstsGround, "it bursts the puddles the sore weeps");
            Assert.AreEqual(UpgradeTrigger.HeavyHit, burst.On, "any heavy blow of yours, not only one on the sore");
            Assert.AreEqual(12f, burst.Damage, 1e-5f);
            Assert.AreEqual(2, burst.Stacks, "two doses to all in each");
            Assert.IsFalse(sore.Any(each => each.Do == UpgradeAction.Burst), "not a burst round the sore");
            Assert.AreEqual(0, Hazards.TakeAllOf(puddle, new Vector3[4], new float[4]), "nothing to burst with no hazards in the scene");
        }

        [Test]
        public void GroundFollowsBitesTheMovingAndCracksOnce()
        {
            Assert.IsFalse(GroundRules.Moved(Vector3.zero, new Vector3(0.1f, 3f, 0f)), "a hair, or up and down, is standing still");
            Assert.IsTrue(GroundRules.Moved(Vector3.zero, new Vector3(0.2f, 0f, 0f)));
            var silver = ShapeOf(Clover.Player.Trees.Duo.QuicksilverDuo.Shape, "Heavy Silver");
            Assert.IsTrue(silver.Where(each => each.Do == UpgradeAction.Hazard).All(each => each.Follows), "the mercury goes with it");
            Assert.AreEqual(0.7f, silver.Single(each => each.Inside != null).Inside.Pace, 1e-5f, "30% slower only in the mercury");
            Assert.IsTrue(silver.Where(each => each.Do == UpgradeAction.Condition).All(each => each.State.Pace == 1f), "not slowed everywhere");
            var step = ShapeOf(Clover.Player.Trees.Duo.EarthedDuo.Shape, "Step Current").Single(each => each.Do == UpgradeAction.Hazard);
            Assert.IsTrue(step.InsideMovingOnly && step.InsideSpares);
            Assert.AreEqual(0f, step.Damage, 1e-5f, "the figure itself bites no one standing still");
            Assert.AreEqual(4f, step.InsideDo.Single().Damage, 1e-5f);
            var stones = ShapeOf(Clover.Player.Trees.Duo.BurntMoundDuo.Shape, "Fire-Cracked").Single(each => each.Do == UpgradeAction.Hazard);
            Assert.AreEqual(2f, stones.CracksAt, 1e-5f, "12 and a Burn in 2 m");
            Assert.AreEqual(12f, stones.Damage, 1e-5f);
            Assert.AreEqual(1f, stones.Radius, 1e-5f, "as an enemy steps within 1 m");
        }

        [Test]
        public void GroundDoesThingsForYouOnlyWhileYouStandInIt()
        {
            var wreck = Upgrade.ById(Clover.Player.Trees.Duo.RustDuo.TheWreck).Effects.First(each => each.YouInside != null);
            Assert.IsTrue(wreck.ActsAsGround);
            var unbroken = wreck.YouInside.Single();
            Assert.AreEqual(UpgradeAction.HeavyUnbroken, unbroken.Do);
            Assert.Greater(unbroken.Seconds, wreck.Interval, "kept up between its bites while you stay");
            Assert.AreSame(wreck.Owner, unbroken.Owner);
            var grass = ShapeOf(Clover.Player.Trees.Duo.RotRootsDuo.Shape, "Summer Grass");
            Assert.IsFalse(grass.Any(each => each.Do == UpgradeAction.Heal), "not wherever you stand");
            var spores = grass.Single(each => each.YouInside != null);
            Assert.AreEqual(6f, spores.YouInside.Single().Amount, 1e-5f);
            Assert.AreEqual(1.5f, spores.Delay, 1e-5f, "as the spores burst");
        }

        [Test]
        public void GroundedDischargesThroughTheLiveWater()
        {
            var film = ReactionOf(AfflictionKind.Static, AfflictionKind.Soaked).Single(each => each.Do == UpgradeAction.Hazard);
            Assert.AreEqual(Reactions.LiveWater, film.Ground, "the film is live water by name");
            var grounded = Upgrade.ById(Clover.Player.Trees.Duo.SeizedDuo.Grounded).Effects.Single();
            Assert.AreEqual(UpgradeAction.SetOff, grounded.Do);
            Assert.AreEqual(Reactions.LiveWater, grounded.ThroughGround, "all standing in live water");
            Assert.AreEqual(0f, grounded.Radius, 1e-5f, "not a circle round its enemy");
            Assert.AreEqual(0, Hazards.AllNamed(Reactions.LiveWater, new Vector3[4], new float[4]), "no water with no hazards in the scene");
        }

        [Test]
        public void TheBubblesStandInARowAndCrackInTurn()
        {
            Assert.AreEqual(new Vector3(0f, 0f, 2f), FormRules.InRow(Vector3.zero, Vector3.forward, 2f, 0));
            Assert.AreEqual(new Vector3(6f, 0f, 0f), FormRules.InRow(Vector3.zero, new Vector3(3f, 1f, 0f), 2f, 2), "flat, along the way faced");
            var bubbles = Upgrade.ById(SpellBranch.ShapeId(WaterTree.DeepCrack)).Shapes.Single(each => each.Name == "String of Bubbles");
            Assert.AreEqual(SpellForms.Pillars, bubbles.Form);
            Assert.AreEqual(3, bubbles.Spell.Count);
            Assert.AreEqual(2f, bubbles.Spell.InLine, 1e-5f, "2 m apart");
            Assert.AreEqual(0.15f, bubbles.Spell.Between, 1e-5f, "each 0.15 s after the last");
            var copy = new SpellSpec();
            SpellChange.Copy(bubbles.Spell, copy);
            Assert.AreEqual(2f, copy.InLine, 1e-5f);
        }

        [Test]
        public void TheRimeGrowsOutRingByRing()
        {
            Assert.AreEqual(1.5f, FormRules.GrownRadius(4.5f, 3, 0), 1e-5f);
            Assert.AreEqual(3f, FormRules.GrownRadius(4.5f, 3, 1), 1e-5f);
            Assert.AreEqual(4.5f, FormRules.GrownRadius(4.5f, 3, 2), 1e-5f);
            Assert.AreEqual(4.5f, FormRules.GrownRadius(4.5f, 3, 7), 1e-5f, "no further than its radius");
            Assert.AreEqual(4.5f, FormRules.GrownRadius(4.5f, 0, 0), 1e-5f, "a plain field is whole at once");
            var rime = Upgrade.ById(Clover.Player.Trees.Duo.VenomRimeDuo.RimeOverRime).Spell;
            Assert.AreEqual(3, rime.GrowsOut);
            Assert.AreEqual(0.6f, rime.Interval, 1e-5f, "0.6 s apart");
        }

        [Test]
        public void TheWingsRiseBehindAndTheSweepThrowsSideways()
        {
            Assert.AreEqual(-1f, PushRules.Sideways(Vector3.forward).x, 1e-5f, "facing ahead, to the left");
            Assert.AreEqual(1f, PushRules.Sideways(new Vector3(2f, 0f, 0f)).z, 1e-5f, "facing right, still to its left");
            var wings = Upgrade.ById(Clover.Player.Trees.Duo.GoldenFeathersDuo.WingsBehind).Effects;
            Assert.AreEqual(2, wings.Length);
            Assert.IsTrue(wings.All(each => each.Do == UpgradeAction.Cone && each.Behind && each.Arc == 180f && each.Reach == 4f), "a half ring behind you, 4 m");
            var sweep = Upgrade.ById(Clover.Player.Trees.Duo.GoldenFeathersDuo.SweepOfWings).Effects.Single(each => each.Do == UpgradeAction.Push);
            Assert.IsTrue(sweep.Sideways, "along the sweep, not away from you");
        }

        [Test]
        public void TheConesSplitIntoThinRays()
        {
            Assert.AreEqual(0f, FormRules.RayWay(Vector3.forward, 3, 7, 70f).x, 1e-5f, "the middle of seven runs straight ahead");
            Assert.AreEqual(-Mathf.Sin(35f * Mathf.Deg2Rad), FormRules.RayWay(Vector3.forward, 0, 7, 70f).x, 1e-5f, "the first at the left edge of the fan");
            Assert.AreEqual(Mathf.Sin(35f * Mathf.Deg2Rad), FormRules.RayWay(Vector3.forward, 6, 7, 70f).x, 1e-5f, "the last at the right edge");
            var horses = Upgrade.ById(SpellBranch.ShapeId(LightTree.Glare)).Shapes.Single(each => each.Name == "Seven Horses");
            Assert.AreEqual(7, horses.Spell.Rays);
            Assert.AreEqual(5, Upgrade.ById(Clover.Player.Trees.Duo.MockSunsDuo.Sunstone).Spell.Rays);
            var copy = new SpellSpec();
            SpellChange.Copy(horses.Spell, copy);
            Assert.AreEqual(7, copy.Rays);
        }

        [Test]
        public void ShedTailRushesBackward()
        {
            var tail = Upgrade.ById(SpellBranch.ShapeId(PoisonTree.ShareAlike)).Shapes.Single(each => each.Name == "Shed Tail");
            Assert.IsTrue(tail.Change.Backward);
            Assert.AreEqual(6f, tail.Change.Reach, 1e-5f, "six metres");
            var spec = new SpellSpec { Reach = 7f };
            tail.Change.ApplyTo(spec);
            Assert.IsTrue(spec.Backward);
            Assert.AreEqual(6f, spec.Reach, 1e-5f);
            var copy = new SpellSpec();
            SpellChange.Copy(spec, copy);
            Assert.IsTrue(copy.Backward);
            Assert.IsFalse(new SpellChange { Backward = true }.IsEmpty);
        }

        [Test]
        public void BlownThroughLeavesYouUntouchableAsYouGo()
        {
            var blown = Upgrade.ById(Clover.Player.Trees.Duo.BreathTakenDuo.BlownThrough).Effects.Where(each => each.On == UpgradeTrigger.SpellHit).ToList();
            int untouchable = blown.FindIndex(each => each.Do == UpgradeAction.Untouchable);
            int step = blown.FindIndex(each => each.Do == UpgradeAction.Step);
            Assert.GreaterOrEqual(untouchable, 0);
            Assert.Less(untouchable, step, "untouchable before the step is taken");
            Assert.AreEqual(0.4f, blown[untouchable].Seconds, 1e-5f);
        }

        [Test]
        public void GoneToAshScattersAndStandsAgain()
        {
            var learn = Upgrade.ById(DarkTree.GoneToAsh).Effects;
            Assert.AreEqual(UpgradeAction.Step, learn[0].Do, "the step behind stays first, for the shapes that set it aside");
            var scatter = learn.Single(each => each.Do == UpgradeAction.Scatter);
            Assert.AreEqual(0.4f, scatter.Seconds, 1e-5f, "standing again 0.4 s later");
            Assert.AreEqual(learn.Length - 1, System.Array.IndexOf(learn, scatter), "last, so the shapes keep it");
            foreach (var shape in Upgrade.ById(SpellBranch.ShapeId(DarkTree.GoneToAsh)).Shapes)
                if (shape.SetsAside != null) CollectionAssert.DoesNotContain(shape.SetsAside, learn.Length - 1, shape.Name + " still scatters");
        }

        [Test]
        public void TheLilyMendsYouWhileYouStandOnIt()
        {
            var lily = Upgrade.ById(BloomTree.Shape).Shapes.Single(each => each.Name == "Floating Flower").Effects;
            Assert.AreEqual(3f, lily.Single(each => each.Do == UpgradeAction.Hazard).Mends, 1e-5f, "3 a second");
            Assert.IsFalse(lily.Any(each => each.Do == UpgradeAction.Heal), "not 8 at once as it opens");
        }

        [Test]
        public void DeepCoalLetsTheBurnLieDeeperWhileItSmoulders()
        {
            Assert.AreEqual("react.smoulder", AfflictionPairs.Id(AfflictionPairs.Index(AfflictionKind.Cursed, AfflictionKind.Burn)));
            var deep = Upgrade.ById(Clover.Player.Trees.Duo.SmoulderDuo.DeepCoal).Effects.Single();
            Assert.AreEqual(UpgradeTrigger.Reacted, deep.On);
            Assert.AreEqual("react.smoulder", deep.Reaction, "on Smoulder alone");
            Assert.AreEqual(AfflictionKind.Burn, deep.State.Deepens);
            Assert.AreEqual(2, deep.State.DeepensBy, "two deeper");
            Assert.AreEqual(AfflictionKind.Cursed, deep.State.While, "while it smoulders: its curse lasts");
        }

        [Test]
        public void TheBurningTreeGrowsOnceEachFight()
        {
            var clock = new Clover.Player.Magic.FightClock();
            Assert.AreEqual(0, clock.Fights);
            clock.Tick(0.1f, true);
            Assert.AreEqual(1, clock.Fights, "a fight began");
            clock.Tick(0.1f, true);
            Assert.AreEqual(1, clock.Fights, "the same fight");
            clock.Tick(clock.CalmSeconds + 1f, false);
            clock.Tick(0.1f, true);
            Assert.AreEqual(2, clock.Fights, "calm, then a new fight");
            Assert.IsTrue(MarkRules.FirstInFight(-1, 1));
            Assert.IsFalse(MarkRules.FirstInFight(1, 1), "not twice in one fight");
            Assert.IsTrue(MarkRules.FirstInFight(1, 2));
            var tree = Upgrade.ById(Clover.Player.Trees.Duo.FirebloomDuo.TheBurningTree).Effects.Single();
            Assert.IsTrue(tree.OncePerFight);
            Assert.AreEqual(0f, tree.Cooldown, 1e-5f, "not once in 30 s");
            Assert.AreEqual(AfflictionKind.Seeded, tree.BitesLay, "Seeded and Burn");
        }

        [Test]
        public void ClearSkyKeepsAMarkABlowLongerOnABurningEnemy()
        {
            Assert.IsTrue(AfflictionRules.BurningBlowMore(1, 2, false, false));
            Assert.IsFalse(AfflictionRules.BurningBlowMore(1, 0, false, false), "not burning");
            Assert.IsFalse(AfflictionRules.BurningBlowMore(1, 2, false, true), "once a laying");
            Assert.IsFalse(AfflictionRules.BurningBlowMore(0, 2, false, false), "without Clear Sky");
            Assert.IsFalse(AfflictionRules.BurningBlowMore(1, 2, true, false), "not the Burn itself");
        }

        [Test]
        public void TheSeaComesInQuickensYouOnlyInYourOwnWater()
        {
            var sea = Upgrade.ById(WaterTree.TheSeaComesIn);
            Assert.IsTrue(sea.Stats.All(each => each.When == StatWhen.InOwnWater), "the dodge and the clocks only in your water");
            var pool = sea.Effects.Single(each => each.Do == UpgradeAction.Hazard);
            Assert.AreEqual(4, pool.Limit, "four pools at most");
            Assert.AreEqual(0.85f, pool.Inside.Pace, 1e-5f, "enemies in them 15% slower");
            Assert.AreEqual(AfflictionKind.Soaked, pool.Affliction, "it is water: it Soaks");
            Assert.Greater(PlayerStats.WhenCount, (int)StatWhen.InOwnWater);
            var stats = new PlayerStats();
            stats.AddLearned(new StatMod(PlayerStat.DodgeDistance, 0.5f, StatWhen.InOwnWater));
            Assert.AreEqual(1f, stats.Multiplier(PlayerStat.DodgeDistance), 1e-5f, "dry ground");
            stats.SetWhen(StatWhen.InOwnWater, true);
            Assert.AreEqual(1.5f, stats.Multiplier(PlayerStat.DodgeDistance), 1e-5f, "in your own water");
            Assert.IsFalse(Hazards.OwnGroundUnder(Vector3.zero, AfflictionKind.Soaked), "no ground with no hazards in the scene");
        }
    }
}

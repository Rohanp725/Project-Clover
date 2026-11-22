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
            Assert.IsTrue(ShapeDef.SetAside(furrow, 1) && ShapeDef.SetAside(furrow, 2) && !ShapeDef.SetAside(furrow, 0), "the window does not go with you; the withering still comes");
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
        public void EdgeGuardsCutCountsAsABlowLanded()
        {
            var edge = Upgrade.ById(MetalTree.EdgeGuard).Effects;
            Assert.AreEqual(1, edge.Count(each => each.On == UpgradeTrigger.PerfectGuard && each.Do == UpgradeAction.CountAsBlow), "its mana and its cut to the waits");
            Assert.IsTrue(edge.Any(each => each.On == UpgradeTrigger.PerfectGuard && each.Do == UpgradeAction.Burst), "the cut itself");
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

        [Test]
        public void TheSunGoesUnderSmouldersEveryBurnCurseOrNoCurse()
        {
            int reported = 0;
            var enemy = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => reported++, () => 0) };
            int smoulder = AfflictionPairs.Index(AfflictionKind.Cursed, AfflictionKind.Burn);
            Assert.IsFalse(enemy.ReactAsLaid(smoulder, null), "nothing of the pair: nothing goes off");
            enemy.Lay(AfflictionKind.Burn, 1, 3f, 1f, LaySource.Weapon);
            Assert.IsTrue(enemy.ReactAsLaid(smoulder, null), "a Burn alone smoulders");
            Assert.AreEqual(1, reported);
            Assert.IsTrue(enemy.Unbound(smoulder), "what it sets now lasts without the Curse");
            Assert.IsFalse(enemy.ReactAsLaid(smoulder, null), "not again while Smoulder rests on it");
            Assert.AreEqual(1, reported);

            var sunk = new ConditionSpec { Id = Reactions.Sunk, Seconds = 30f, Beat = 2f, While = AfflictionKind.Cursed, Keeps = AfflictionKind.Burn };
            var loose = ConditionCaster.Unbound(sunk);
            Assert.AreEqual(AfflictionKind.None, loose.While, "no Curse needed");
            Assert.AreEqual(AfflictionKind.Cursed, sunk.While, "the reaction's own spec is left as written");
            Assert.AreSame(loose, ConditionCaster.Unbound(loose), "a spec bound to nothing is kept");
            enemy.Conditions.Set(loose, null, 1);
            enemy.Tick(0.5f);
            Assert.IsTrue(enemy.Conditions.Holds(Reactions.Sunk), "it smoulders with no Curse on it");
            Assert.IsTrue(enemy.Burn.IsBurning, "and its fire does not fade");

            var bound = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
            bound.Lay(AfflictionKind.Burn, 1, 3f, 1f, LaySource.Weapon);
            bound.Conditions.Set(sunk, null, 1);
            bound.Tick(0.1f);
            Assert.IsFalse(bound.Conditions.Holds(Reactions.Sunk), "the plain smoulder ends with no Curse");
            enemy.Tick(1.1f);
            Assert.IsFalse(enemy.Unbound(smoulder), "only the second after it went off");

            var under = Upgrade.ById(Clover.Player.Trees.Duo.SmoulderDuo.TheSunGoesUnder).Effects.Single();
            Assert.AreEqual(UpgradeTrigger.AfflictionLaid, under.On);
            Assert.AreEqual(AfflictionKind.Burn, under.Spent, "every Burn you lay");
            Assert.AreEqual(StatWhen.HalfHealth, under.While);
            Assert.AreEqual(UpgradeAction.ForceReaction, under.Do);
            Assert.AreEqual("react.smoulder", under.Reaction);
            Assert.IsTrue(under.AsLaid, "curse or no curse");
        }

        [Test]
        public void ColdShortsCrackedPlatesKeepTheNotchesForTheRestOfTheFight()
        {
            var plain = Upgrade.ById(ReactionTable.MarkFor(AfflictionPairs.Index(AfflictionKind.Chill, AfflictionKind.Sundered)).Id).Effects;
            var shed = Upgrade.ById(Clover.Player.Trees.Duo.ColdShortDuo.Shape).Shapes.Single(each => each.Name == "Shed Plate").Effects;
            foreach (var effects in new[] { plain, shed })
            {
                var keep = effects.Single(each => each.Do == UpgradeAction.KeepForGood);
                Assert.AreEqual(UpgradeTrigger.ConditionStruck, keep.On);
                Assert.AreEqual(StruckBy.Heavy, keep.By, "the heavy blow that cracks the plates");
                Assert.AreEqual(AfflictionKind.Sundered, keep.Affliction, "its notches");
                Assert.IsFalse(effects.Any(each => each.State != null && each.State.Seconds >= 120f), "no two minutes standing in");
            }
            var enemy = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
            enemy.Lay(AfflictionKind.Sundered, 2, 0f, 1f, LaySource.Weapon);
            enemy.KeepForGood(AfflictionKind.Sundered);
            for (int i = 0; i < 3000; i++) enemy.Tick(0.1f);
            Assert.AreEqual(2, enemy.Stacks(AfflictionKind.Sundered), "five minutes on, its notches are still there");
        }

        [Test]
        public void AshHangsAndSeizedJointsKeepEnemiesFromTurning()
        {
            var ash = Upgrade.ById(SpellBranch.ShapeId(DarkTree.GoneToAsh)).Shapes.Single(each => each.Name == "Ash Hangs").Effects.Single(each => each.Do == UpgradeAction.Hazard);
            Assert.AreEqual(ConditionFlags.CannotTurn, ash.Inside.Flags & ConditionFlags.CannotTurn, "an attack begun inside goes where it already faced");
            Assert.IsTrue(ash.ActsAsGround);

            var spec = new ConditionSpec { Id = "test.joints", Seconds = 2f, SlowPerStack = 0.06f, TurnPerStack = 0.1f, Flags = ConditionFlags.TurnSlowed };
            Assert.AreEqual(0.7f, ConditionCaster.TurnFor(spec, 3), 1e-4f, "10% a notch");
            Assert.AreEqual(0.82f, ConditionCaster.PaceFor(spec, 3), 1e-4f, "6% a notch");
            Assert.AreEqual(0.1f, ConditionCaster.TurnFor(spec, 20), 1e-4f, "never below a tenth");
            var enemy = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
            int slot = enemy.Conditions.Set(spec, null, 1, null, false, false, 1f, false, false, ConditionCaster.PaceFor(spec, 5));
            enemy.Conditions.Keep(slot, ConditionCaster.TurnFor(spec, 5));
            enemy.Tick(0.1f);
            Assert.AreEqual(0.5f, enemy.Conditions.TurnScale, 1e-4f, "five notches: half the turning");

            var joints = Upgrade.ById(Clover.Player.Trees.Duo.RustDuo.Shape).Shapes.Single(each => each.Name == "Seized Joints").Effects;
            var seize = joints.Single(each => each.State != null && each.State.Id == Clover.Player.Trees.Duo.RustDuo.JointsSeized);
            Assert.IsTrue(seize.CountsNeeds && seize.PerStack, "by the notches it carries");
            Assert.AreEqual(0.06f, seize.State.SlowPerStack, 1e-5f);
            Assert.AreEqual(0.1f, seize.State.TurnPerStack, 1e-5f);
            Assert.AreEqual(1f, joints[0].State.Pace, 1e-5f, "no flat fifth slower");
        }

        [Test]
        public void TheBlisterSwellsAtEachBiteAndBreaksOnlyOnAHeavyBlow()
        {
            Assert.AreEqual(20f, Clover.Player.Forms.BrandForm.BlisterDamage(40f, 0), 1e-4f);
            Assert.AreEqual(40f, Clover.Player.Forms.BrandForm.BlisterDamage(40f, 1), 1e-4f);
            Assert.AreEqual(70f, Clover.Player.Forms.BrandForm.BlisterDamage(40f, 2), 1e-4f);
            Assert.AreEqual(85f, Clover.Player.Forms.BrandForm.BlisterDamage(50f, 2), 1e-4f, "the Empower's 25, 50, 85");
            Assert.AreEqual(2f, Clover.Player.Forms.BrandForm.BlisterRadius(3f, 0), 1e-4f);
            Assert.AreEqual(4f, Clover.Player.Forms.BrandForm.BlisterRadius(3f, 2), 1e-4f);
            Assert.AreEqual(2, Clover.Player.Forms.BrandForm.BlisterDoses(4, 0));
            Assert.AreEqual(4, Clover.Player.Forms.BrandForm.BlisterDoses(4, 1));
            Assert.AreEqual(6, Clover.Player.Forms.BrandForm.BlisterDoses(4, 2));
            Assert.AreEqual(1, Clover.Player.Forms.BrandForm.Swell(0));
            Assert.AreEqual(2, Clover.Player.Forms.BrandForm.Swell(2), "three sizes at most");
            var blister = Upgrade.ById(PoisonTree.Blister).Spell;
            Assert.IsTrue(blister.Swells);
            var copy = new SpellSpec();
            SpellChange.Copy(blister, copy);
            Assert.IsTrue(copy.Swells, "kept through the Learn's copy");
            var master = Upgrade.ById(SpellBranch.MasterId(PoisonTree.Blister)).Effects;
            Assert.AreEqual(Clover.Player.Forms.BrandForm.DoublesOnBeat, master.Single().State.Is, "the form doubles its own burst on the beat");
            Assert.IsFalse(master.Any(each => each.Do == UpgradeAction.Burst), "no second burst of a set size");
        }

        [Test]
        public void TheDelayedTouchHoldsPoisonsReactionsAndLandsThemDoubled()
        {
            var touch = Upgrade.ById(PoisonTree.TheDelayedTouch).Effects;
            int jar = AfflictionPairs.Index(AfflictionKind.Cursed, AfflictionKind.Poison);
            int smoulder = AfflictionPairs.Index(AfflictionKind.Cursed, AfflictionKind.Burn);
            Assert.AreEqual(2.5f, ReactionRules.Hold(touch, jar, out float strength), 1e-5f, "held two and a half seconds");
            Assert.AreEqual(2f, strength, 1e-5f, "landing at double strength");
            Assert.AreEqual(0f, ReactionRules.Hold(touch, smoulder, out float none), 1e-5f, "no poison in it: not held");
            Assert.AreEqual(1f, none, 1e-5f);
            var dose = touch.Single(each => each.Do == UpgradeAction.Afflict);
            Assert.AreEqual(UpgradeTrigger.Reacted, dose.On, "as it lands");
            Assert.AreEqual(2, dose.Stacks, "two deep on all it reaches");
            Assert.IsFalse(touch.Any(each => each.Do == UpgradeAction.Burst), "no burst of its own: the reaction itself lands");

            int sap = AfflictionPairs.Index(AfflictionKind.Marked, AfflictionKind.Poison);
            Assert.IsTrue(ReactionRules.WaitsAlready(ReactionTable.MarkFor(sap).Effects), "Burning Sap holds back its own bites already");
            Assert.IsFalse(ReactionRules.WaitsAlready(ReactionTable.MarkFor(jar).Effects));
            Assert.IsTrue(PlayerReactions.StaysHeld(1f, 2.5f, true));
            Assert.IsFalse(PlayerReactions.StaysHeld(1f, 2.5f, false), "a heavy blow ended the hold");
            Assert.IsFalse(PlayerReactions.StaysHeld(2.6f, 2.5f, true), "its time is out");
        }

        [Test]
        public void MockSunsCriticalAndSparingOnlyFromTheFlanks()
        {
            Assert.IsFalse(EnemyConditions.OffFlank(0f, 0f, 10f), "no flank: every side");
            Assert.IsFalse(EnemyConditions.OffFlank(45f, 135f, 90f), "a flank");
            Assert.IsTrue(EnemyConditions.OffFlank(45f, 135f, 10f), "its face");
            Assert.IsTrue(EnemyConditions.OffFlank(45f, 135f, 170f), "its back");
            Assert.IsFalse(EnemyConditions.OffFlank(45f, 135f, -1f), "a side not known");
            var suns = ReactionTable.MarkFor(AfflictionPairs.Index(AfflictionKind.Chill, AfflictionKind.Marked)).Effects[0].State;
            Assert.AreEqual(45f, suns.FlankLeast, 1e-5f);
            Assert.AreEqual(135f, suns.FlankMost, 1e-5f);

            var enemy = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
            enemy.Lay(AfflictionKind.Marked, 1, 0f, 1f, LaySource.Weapon);
            enemy.Conditions.Set(suns, null, 1);
            Assert.IsTrue(enemy.MakesBlowsCriticalFrom(90f), "a blow on its flank is critical");
            Assert.IsFalse(enemy.MakesBlowsCriticalFrom(0f), "a blow at its face is not, by the Mark");
            int left = enemy.BlowsLeft(AfflictionKind.Marked);
            enemy.BlowLanded(90f);
            Assert.AreEqual(left, enemy.BlowsLeft(AfflictionKind.Marked), "the flank blow spares the Mark");
            enemy.BlowLanded(0f);
            Assert.AreEqual(left - 1, enemy.BlowsLeft(AfflictionKind.Marked), "the face blow spends it");
        }

        [Test]
        public void TheYawningGapsBlowsTakeTurnsAndThermalShockRestsTwoSeconds()
        {
            Assert.IsTrue(MarkRules.OnTurn(5, 0, 0), "no turns: every swing");
            Assert.IsTrue(MarkRules.OnTurn(4, 2, 0));
            Assert.IsFalse(MarkRules.OnTurn(5, 2, 0));
            Assert.IsTrue(MarkRules.OnTurn(5, 2, 1));
            Assert.IsTrue(MarkRules.OnTurn(-1, 2, 1), "never a bad turn");
            var gap = Upgrade.ById(Clover.Player.Trees.Duo.ThermalShockDuo.TheYawningGap).Effects;
            var burn = gap.Single(each => each.On == UpgradeTrigger.AnyHit && each.Affliction == AfflictionKind.Burn);
            var chill = gap.Single(each => each.On == UpgradeTrigger.AnyHit && each.Affliction == AfflictionKind.Chill);
            Assert.AreEqual(2, burn.TakesTurns);
            Assert.AreEqual(2, chill.TakesTurns);
            Assert.AreNotEqual(burn.Turn, chill.Turn, "burning, then chilling");
            var rest = gap.Single(each => each.Do == UpgradeAction.ShortenRest);
            Assert.AreEqual(2f, rest.Seconds, 1e-5f);
            Assert.IsTrue(rest.WhileSurge);

            int shock = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Chill);
            var enemy = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
            enemy.RestReaction(shock, 4f);
            enemy.ShortenRest(shock, 2f);
            Assert.AreEqual(2f, enemy.ReactionRest(shock), 1e-5f, "once every 2 s on each enemy");
            enemy.ShortenRest(shock, 3f);
            Assert.AreEqual(2f, enemy.ReactionRest(shock), 1e-5f, "never made longer");
        }

        [Test]
        public void WastingGripDosesAllInsideTheJarsRingEachSecond()
        {
            var grip = Upgrade.ById(BlightTree.WastingGrip).Effects.Single(each => each.InState == Reactions.Jar);
            Assert.AreEqual(UpgradeTrigger.SpellHit, grip.On, "as the stream bites the jarred one");
            Assert.AreEqual(UpgradeAction.Afflict, grip.Do);
            Assert.AreEqual(AfflictionKind.Poison, grip.Affliction);
            Assert.AreEqual(1, grip.Stacks);
            Assert.AreEqual(1f, grip.Cooldown, 1e-5f, "a dose a second");
            var jar = ReactionTable.MarkFor(AfflictionPairs.Index(AfflictionKind.Cursed, AfflictionKind.Poison)).Effects.First(each => each.State != null && each.State.Id == Reactions.Jar);
            Assert.AreEqual(jar.State.DeathsWithin, grip.Radius, 1e-5f, "the jar's own ring");
        }

        [Test]
        public void StoneUnderfootTurnsItToStoneAtOnce()
        {
            var under = Upgrade.ById(Clover.Player.Trees.Duo.TurnedToStoneDuo.StoneUnderfoot).Effects;
            var stone = under.Single(each => each.State != null && each.State.Id == Reactions.Stone);
            Assert.AreEqual(UpgradeTrigger.SpellHit, stone.On, "as the foot comes down, no climb first");
            Assert.AreEqual(ConditionFlags.Rooted | ConditionFlags.BeginsNoAttack | ConditionFlags.StaggerTakenShare, stone.State.Flags);
            Assert.AreEqual(2f, stone.State.Scale, 1e-5f, "double stagger");
            Assert.AreEqual(1.5f, stone.State.Seconds, 1e-5f);
            Assert.IsFalse((stone.Ranks & EnemyRank.God) != 0);
            Assert.IsTrue(under.Any(each => each.State != null && each.State.Id == Reactions.StoneSpellproof), "half from spells");
            var god = under.Single(each => each.Ranks == EnemyRank.God);
            Assert.AreEqual(0.4f, god.State.Pace, 1e-5f, "a god only slowed 60%");
            var breaks = under.Single(each => each.On == UpgradeTrigger.ConditionStruck);
            Assert.AreEqual(Reactions.Stone, breaks.Condition);
            Assert.AreEqual(StruckBy.Heavy, breaks.By, "the heavy blow that breaks it out");
            Assert.AreEqual(50f, breaks.Stagger, 1e-5f);
            Assert.IsFalse(under.Any(each => each.Do == UpgradeAction.ForceReaction), "no climb");
            var rest = under[0];
            Assert.AreEqual(UpgradeAction.RestReaction, rest.Do, "rested first, so the Curse starts no climb");
            Assert.AreEqual(AfflictionPairs.Index(AfflictionKind.Tremor, AfflictionKind.Cursed), ReactionTable.PairOf(rest.Reaction));
        }

        [Test]
        public void EatenInTurnPassesToTheNextOnTheLineByItsOrder()
        {
            Assert.IsTrue(PlayerUpgrades.NextInLine(2f, 3f, 0f), "the one after it");
            Assert.IsTrue(PlayerUpgrades.NextInLine(2f, 3f, 5f), "nearer in the line than the best so far");
            Assert.IsFalse(PlayerUpgrades.NextInLine(2f, 5f, 3f), "farther down the line than one found");
            Assert.IsFalse(PlayerUpgrades.NextInLine(2f, 1f, 0f), "before it on the line");
            Assert.IsFalse(PlayerUpgrades.NextInLine(2f, 2f, 0f), "itself");
            var enemy = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
            int slot = enemy.Conditions.Set(new ConditionSpec { Id = PoisonTree.EatenInTurn, Seconds = 8f, Numbered = true }, null, 2);
            enemy.Conditions.Keep(slot, 4f);
            Assert.IsTrue(enemy.Conditions.NewsOf(PoisonTree.EatenInTurn, ConditionMoment.Began, out var news));
            Assert.AreEqual(4f, news.Scale, 1e-5f, "its place on the line is kept on it");
            var eaten = Upgrade.ById(SpellBranch.ShapeId(PoisonTree.OneByOne)).Shapes.Single(each => each.Name == "Eaten in Turn").Effects;
            Assert.IsTrue(eaten.Single(each => each.On == UpgradeTrigger.SpellHit).State.Numbered, "the line is numbered as it is drawn");
            var pass = eaten.Single(each => each.On == UpgradeTrigger.ConditionDied);
            Assert.IsTrue(pass.InOrder, "the next by the line's order");
            Assert.AreEqual(PoisonTree.EatenInTurn, pass.ToState);
        }

        [Test]
        public void NothingResistsRunsOnEveryArcThatLeavesAChilledEnemy()
        {
            var resists = Upgrade.ById(Clover.Player.Trees.Duo.SleetChargeDuo.NothingResists);
            var onArc = resists.Effects.Single(each => each.On == UpgradeTrigger.ArcLeft);
            Assert.AreEqual(AfflictionKind.Chill, onArc.Needs, "only from a chilled enemy");
            Assert.AreEqual(UpgradeAction.Arc, onArc.Do);
            Assert.AreEqual(1f, onArc.Amount, 1e-5f, "at the arc's full strength");
            Assert.AreEqual(AfflictionKind.Chill, onArc.LeapsTo, "to the chilled");
            Assert.IsTrue(resists.Effects.Any(each => each.On == UpgradeTrigger.SetOff && each.Spent == AfflictionKind.Static), "a discharge still counts");
            Assert.IsTrue(PlayerUpgrades.AnswersArcs(resists), "its own arcs are not answered again");
            Assert.IsFalse(PlayerUpgrades.AnswersArcs(Upgrade.ById(LightTree.Reckoning)));
            Assert.AreNotEqual("its moment comes", TriggerWords.Of(UpgradeTrigger.ArcLeft));
        }

        [Test]
        public void TheFeastAnswersOnlyAKillByKindlingPointsOwnBurst()
        {
            string kindling = Clover.Player.Trees.Duo.KindlingPointDuo.KindlingReaction;
            Assert.AreEqual("Kindling Point", AfflictionPairs.Name(AfflictionPairs.Index(AfflictionKind.Marked, AfflictionKind.Burn)));
            Assert.IsTrue(PlayerUpgrades.KillAnswers("", ""), "a plain kill answer hears every death");
            Assert.IsTrue(PlayerUpgrades.KillAnswers("", kindling));
            Assert.IsTrue(PlayerUpgrades.KillAnswers(kindling, kindling), "the burst's own kill");
            Assert.IsFalse(PlayerUpgrades.KillAnswers(kindling, ""), "a burning enemy cut down by your blade");
            Assert.IsFalse(PlayerUpgrades.KillAnswers(kindling, "react.smoulder"), "another reaction's kill");
            foreach (var feast in Upgrade.ById(Clover.Player.Trees.Duo.KindlingPointDuo.TheFeast).Effects)
            {
                Assert.AreEqual(UpgradeTrigger.Kill, feast.On);
                Assert.AreEqual(kindling, feast.KilledBy);
            }
        }

        [Test]
        public void DeathTurnedBackMendsWhatTheBlowWouldHaveDealt()
        {
            Assert.AreEqual(22f, PlayerDefence.TurnedMend(22f, 30f), 1e-5f);
            Assert.AreEqual(30f, PlayerDefence.TurnedMend(55f, 30f), 1e-5f, "30 at most");
            Assert.AreEqual(0f, PlayerDefence.TurnedMend(-3f, 30f), 1e-5f);
            var heal = Upgrade.ById(Clover.Player.Trees.Duo.GirdledDuo.DeathTurnedBack).Effects.Single(each => each.On == UpgradeTrigger.SpellHit && each.Do == UpgradeAction.Heal);
            Assert.IsTrue(heal.ByTurnedWound, "by the blow, not a set 15");
            Assert.AreEqual(30f, heal.Amount, 1e-5f);
        }

        [Test]
        public void ReckoningsMasterPaysTheSealsCountOnAFinisher()
        {
            var enemy = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
            Assert.IsFalse(enemy.Conditions.NewsOf(LightTree.Sealed, ConditionMoment.Ended, out _), "no seal, nothing to pay");
            enemy.Conditions.Set(new ConditionSpec { Id = LightTree.Sealed, Seconds = 6f }, null, 3, null, false, true);
            enemy.Conditions.Struck(10f, false, false);
            enemy.Conditions.Struck(10f, true, false, finisher: true);
            Assert.IsTrue(enemy.Conditions.NewsOf(LightTree.Sealed, ConditionMoment.Ended, out var news));
            Assert.AreEqual(ConditionMoment.Ended, news.Moment);
            Assert.AreEqual(2, news.Blows, "the count as it stands, the finisher among it");
            Assert.AreEqual(3, news.Mark, "answered by the mark that set it, with its Empower");
            Assert.IsTrue(enemy.Conditions.Holds(LightTree.Sealed), "it runs on, to pay again as it breaks");

            var witnessed = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
            witnessed.Conditions.Set(new ConditionSpec { Id = LightTree.Witnessed, Seconds = 6f, Is = LightTree.Sealed, TheirBlowPoints = 2 }, null, 4, null, false, true);
            witnessed.Conditions.Attacked(false);
            Assert.IsTrue(witnessed.Conditions.NewsOf(LightTree.Sealed, ConditionMoment.Ended, out var seen), "Witnessed is a seal too");
            Assert.AreEqual(LightTree.Witnessed, seen.Id);
            Assert.AreEqual(2, seen.Blows);

            var master = Upgrade.ById(SpellBranch.MasterId(LightTree.Reckoning)).Effects.Single(each => each.On == UpgradeTrigger.ConditionStruck);
            Assert.AreEqual(StruckBy.Finisher, master.By);
            Assert.AreEqual(UpgradeAction.PayNow, master.Do, "the count, not a flat 40");
            Assert.AreEqual(LightTree.Sealed, master.ToState);
            Assert.AreEqual(LightTree.Sealed, Upgrade.ById(SpellBranch.ShapeId(LightTree.Reckoning)).Shapes.Single(each => each.Name == "Witnessed").Effects[0].State.Is);
        }

        [Test]
        public void WitnessedCountsTheBearersBlowsOnYou()
        {
            Assert.AreEqual(0, EnemyConditions.TheirBlowCount(0, 1, true), "a condition that does not count them");
            Assert.AreEqual(2, EnemyConditions.TheirBlowCount(2, 1, false));
            Assert.AreEqual(3, EnemyConditions.TheirBlowCount(2, 1, true), "15 more for a perfect guard");
            var enemy = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
            var spec = new ConditionSpec { Id = LightTree.Witnessed, Seconds = 6f, TheirBlowPoints = 2, TurnedPoints = 1 };
            int slot = enemy.Conditions.Set(spec, null, 1, null, false, true);
            enemy.Conditions.Struck(10f, false, false);
            Assert.AreEqual(0, enemy.Conditions.Run(slot).Blows, "your own blows are not written");
            enemy.Conditions.Attacked(false);
            enemy.Conditions.Attacked(true);
            Assert.AreEqual(5, enemy.Conditions.Run(slot).Blows, "one blow landed or guarded (2) and one turned (3)");

            var witnessed = Upgrade.ById(SpellBranch.ShapeId(LightTree.Reckoning)).Shapes.Single(each => each.Name == "Witnessed").Effects;
            Assert.AreEqual(2, witnessed[0].State.TheirBlowPoints);
            Assert.AreEqual(1, witnessed[0].State.TurnedPoints);
            var pay = witnessed.Single(each => each.On == UpgradeTrigger.ConditionEnded && each.PerStack);
            Assert.AreEqual(15f, pay.Damage, 1e-5f, "15 a point: 30 a blow written, 45 one turned");
            Assert.AreEqual(10, pay.PerMost, "150 at most");
            Assert.AreEqual(1, pay.SpentAtLeast, "nothing written, nothing paid");
            Assert.IsTrue(witnessed.Any(each => each.On == UpgradeTrigger.ConditionEnded && each.Do == UpgradeAction.Falling && each.Stagger == 40f), "the column and its stagger");
        }

        [Test]
        public void CircleOfSteelTurnsMoreThanOneBlowAndTheRingsGrow()
        {
            Assert.IsFalse(Clover.Player.Forms.ParryForm.TurnsAnother(1, 0, 0.2f, 1.2f), "the plain circle: the first blow only");
            Assert.IsFalse(Clover.Player.Forms.ParryForm.TurnsAnother(1, 1, 0.2f, 1.2f));
            Assert.IsTrue(Clover.Player.Forms.ParryForm.TurnsAnother(1, 2, 0.2f, 1.2f), "the Empower: a second");
            Assert.IsFalse(Clover.Player.Forms.ParryForm.TurnsAnother(2, 2, 0.4f, 1.2f), "two turned: done");
            Assert.IsFalse(Clover.Player.Forms.ParryForm.TurnsAnother(1, 4, 1.5f, 1.5f), "its time is out");
            Assert.AreEqual(4f, Clover.Player.Forms.ParryForm.AnswerReach(4f, 1f, 1), 1e-5f);
            Assert.AreEqual(7f, Clover.Player.Forms.ParryForm.AnswerReach(4f, 1f, 4), 1e-5f, "the fourth ring three metres wider");

            var branch = Upgrade.ById(MetalTree.CircleOfSteel);
            var empower = Upgrade.ById(SpellBranch.EmpowerId(MetalTree.CircleOfSteel)).Empower;
            Assert.AreEqual(2, empower.Turns, "two blows turned");
            var spec = new SpellSpec();
            SpellChange.Copy(branch.Spell, spec);
            empower.ApplyTo(spec);
            Assert.AreEqual(2, spec.Turns);
            Assert.AreEqual(16f, spec.Damage, 1e-5f);
            var plate = Upgrade.ById(SpellBranch.ShapeId(MetalTree.CircleOfSteel)).Shapes.Single(each => each.Name == "Ringing Plate").Spell;
            Assert.AreEqual(4, plate.Turns, "four at most");
            Assert.AreEqual(1f, plate.TurnGrows, 1e-5f, "each ring wider");
            var worn = new SpellSpec();
            SpellChange.Copy(plate, worn);
            empower.ApplyTo(worn);
            Assert.AreEqual(4, worn.Turns, "the Empower never turns fewer than the shape");
        }

        [Test]
        public void StoneSkinsPlatesTakeMostOfOneBlowEach()
        {
            Assert.AreEqual(40f, PlayerDefence.PlatedWound(100f, 0.6f), 1e-4f, "a plate takes 60% of the blow");
            Assert.AreEqual(0f, PlayerDefence.PlatedWound(100f, 1f), 1e-4f);
            Assert.AreEqual(100f, PlayerDefence.PlatedWound(100f, 0f), 1e-4f);
            var skin = Upgrade.ById(SpellBranch.ShapeId(EarthTree.MountainStance)).Shapes.Single(each => each.Name == "Stone Skin").Effects;
            var grow = skin.Single(each => each.On == UpgradeTrigger.SpellHit);
            Assert.AreEqual(UpgradeAction.Plate, grow.Do, "plates, not wards that turn the whole blow");
            Assert.AreEqual(2, grow.Count);
            Assert.AreEqual(0.6f, grow.Amount, 1e-5f);
            Assert.IsFalse(skin.Any(each => each.Do == UpgradeAction.Ward || each.On == UpgradeTrigger.WardSpent), "nothing answers wards");
            var breaks = skin.Single(each => each.On == UpgradeTrigger.PlateBroken);
            Assert.AreEqual(UpgradeAction.Burst, breaks.Do, "each plate breaks in its burst");
            Assert.AreEqual(14f, breaks.Damage, 1e-5f);
            Assert.AreNotEqual("its moment comes", TriggerWords.Of(UpgradeTrigger.PlateBroken));
        }

        [Test]
        public void TheSecondWaveThrowsDownOnlyWhatBothRingsCatch()
        {
            var wave = Upgrade.ById(SpellBranch.ShapeId(EarthTree.QuakeStep)).Shapes.Single(each => each.Name == "The Second Wave").Effects;
            var first = wave.Single(each => each.On == UpgradeTrigger.SpellHit);
            Assert.AreEqual(EarthTree.FirstRing, first.State.Id, "the first ring only marks what it catches");
            Assert.AreEqual(ConditionFlags.None, first.State.Flags, "not thrown down by the first ring");
            Assert.GreaterOrEqual(first.State.Seconds, 1.15f, "kept until the heavy ring lands");
            var heavy = wave.Single(each => each.Do == UpgradeAction.Burst);
            Assert.AreEqual(1.15f, heavy.Delay, 1e-5f);
            Assert.IsTrue(heavy.ActsAsGround, "it acts on each one standing in it as it lands");
            var down = heavy.InsideDo.Single();
            Assert.AreEqual(EarthTree.FirstRing, down.InState, "only what the first ring caught");
            Assert.AreEqual(ConditionFlags.Down, down.State.Flags & ConditionFlags.Down);
            Assert.AreEqual(1f, down.State.Seconds, 1e-5f, "thrown off its feet for a second");
            Assert.GreaterOrEqual(down.Slot, 0, "slotted, so it can be done");
        }

        [Test]
        public void HeldDownAndTheYewStartTheFigAtTwoCoils()
        {
            int girdled = AfflictionPairs.Index(AfflictionKind.Seeded, AfflictionKind.Cursed);
            var enemy = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
            Assert.AreEqual(0, enemy.CountStartsAt(girdled));
            enemy.StartCountAt(girdled, 1);
            Assert.AreEqual(1, enemy.CountStartsAt(girdled), "one blow already drawn");
            Assert.AreEqual(0, enemy.CountStartsAt(AfflictionPairs.Index(AfflictionKind.Cursed, AfflictionKind.Burn)), "only for the fig");
            Assert.IsFalse(enemy.Unbound(girdled), "a count, not a reaction gone off alone");

            var coils = new ConditionSpec { Id = Reactions.Coils, Seconds = 8f, Blows = 3 };
            int slot = enemy.Conditions.Set(coils, null, 1, null, true);
            enemy.Conditions.StartAt(slot, enemy.CountStartsAt(girdled));
            enemy.Conditions.Struck(5f, false, false);
            Assert.IsTrue(enemy.Conditions.Holds(Reactions.Coils), "three coils after one blow");
            enemy.Conditions.Struck(5f, false, false);
            Assert.IsFalse(enemy.Conditions.Holds(Reactions.Coils), "crushed at four coils on the second blow, not the third");
            enemy.Tick(1.1f);
            Assert.AreEqual(0, enemy.CountStartsAt(girdled), "the note lasts a second");

            var held = Upgrade.ById(Clover.Player.Trees.Duo.GirdledDuo.HeldDown).Effects.Single(each => each.Do == UpgradeAction.ForceReaction);
            Assert.AreEqual(1, held.CountedBlows, "Held Down: two coils");
            foreach (var pass in Upgrade.ById(Clover.Player.Trees.Duo.GirdledDuo.WhatTheYewKeeps).Effects.Where(each => each.Do == UpgradeAction.Pass))
            {
                Assert.AreEqual("react.girdled", pass.Reaction);
                Assert.AreEqual(1, pass.CountedBlows, "the fig on the one it passes to: two coils");
            }
        }

        [Test]
        public void AshOnTheBrowKeepsTheHeavyBlowAndTheWardForTheAttacker()
        {
            var smeared = new object();
            var other = new object();
            Assert.IsTrue(PlayerCombat.MadeHeavyOn(false, 6f, 2f, smeared, smeared), "the blow on the smeared attacker is heavy");
            Assert.IsFalse(PlayerCombat.MadeHeavyOn(false, 6f, 2f, smeared, other), "a blow on anyone else stays light");
            Assert.IsTrue(PlayerCombat.MadeHeavyOn(false, 6f, 2f, null, other), "a window kept for no one makes any blow heavy");
            Assert.IsFalse(PlayerCombat.MadeHeavyOn(false, 6f, 6.5f, smeared, smeared), "six seconds, then gone");
            Assert.IsFalse(PlayerCombat.MadeHeavyOn(true, 6f, 2f, smeared, smeared), "a heavy blow is heavy already");

            Assert.IsTrue(PlayerDefence.KeptWardTurns(smeared, 6f, 2f, smeared), "its blow is turned");
            Assert.IsFalse(PlayerDefence.KeptWardTurns(smeared, 6f, 2f, other), "another's blow is not");
            Assert.IsFalse(PlayerDefence.KeptWardTurns(smeared, 6f, 2f, null), "a blow from no one found is not");
            Assert.IsFalse(PlayerDefence.KeptWardTurns(smeared, 6f, 7f, smeared), "kept six seconds");

            var shape = Upgrade.ById(SpellBranch.ShapeId(DarkTree.GoneToAsh)).Shapes.Single(each => each.Name == "Ash on the Brow");
            var ward = shape.Effects.Single(each => each.Do == UpgradeAction.Ward);
            Assert.IsTrue(ward.OnTarget, "the ward is the attacker's alone");
            Assert.AreEqual(6f, ward.Seconds, 1e-5f);
            var heavy = shape.Effects.Single(each => each.Do == UpgradeAction.NextHeavy);
            Assert.IsTrue(heavy.OnTarget, "only the first blow on it");
            Assert.AreEqual(6f, heavy.Seconds, 1e-5f, "waiting while the smear lasts");
            var learn = Upgrade.ById(DarkTree.GoneToAsh).Effects;
            int learnHeavy = System.Array.FindIndex(learn, each => each.Do == UpgradeAction.NextHeavy);
            Assert.IsTrue(ShapeDef.SetAside(shape, learnHeavy), "the Learn's blow on anyone is set aside");
            Assert.IsTrue(ShapeDef.SetAside(shape, 0), "you stand again where you were");
        }

        [Test]
        public void TheHagsStaffDrivesFrostUnderWhatAHeavyBlowStrikesOnItsDisc()
        {
            var staff = Upgrade.ById(Clover.Player.Trees.Duo.FrostHeaveDuo.TheHagsStaff);
            Assert.AreEqual(Clover.Player.Trees.Duo.FrostHeaveDuo.HagsGround, staff.Spell.Ground, "the disc is your own ground");
            var driven = staff.Effects.Where(each => each.On == UpgradeTrigger.HeavyHit).ToArray();
            Assert.AreEqual(2, driven.Length);
            Assert.IsTrue(driven.All(each => each.TargetOnGround == Clover.Player.Trees.Duo.FrostHeaveDuo.HagsGround), "only on the disc");
            Assert.AreEqual(25f, driven.Single(each => each.Do == UpgradeAction.Burst).Stagger, 1e-5f, "25 more stagger");
            Assert.AreEqual(0.5f, driven.Single(each => each.Do == UpgradeAction.Lift).Seconds, 1e-5f, "thrown up half a second");

            OwnGrounds.Lay(Clover.Player.Trees.Duo.FrostHeaveDuo.HagsGround, Vector3.zero, 6f, 6f);
            Assert.IsTrue(OwnGrounds.Under(Clover.Player.Trees.Duo.FrostHeaveDuo.HagsGround, new Vector3(4f, 0f, 3f), 1f), "five metres out is on it");
            Assert.IsFalse(OwnGrounds.Under(Clover.Player.Trees.Duo.FrostHeaveDuo.HagsGround, new Vector3(5f, 0f, 5f), 1f), "seven metres out is not");
            Assert.IsFalse(OwnGrounds.Under(Clover.Player.Trees.Duo.FrostHeaveDuo.HagsGround, Vector3.zero, 6.5f), "gone after six seconds");
        }

        [Test]
        public void UnderTheMoundsMasterMakesTheNextBlowHeavy()
        {
            var master = Upgrade.ById(SpellBranch.MasterId(EarthTree.UnderTheMound)).Effects;
            var heavy = master.Single(each => each.Do == UpgradeAction.NextHeavy);
            Assert.AreEqual(0.3f, heavy.BlowLanding, 1e-5f, "only when gone under as a blow would land");
            Assert.IsFalse(heavy.OnTarget);
        }

        [Test]
        public void BellowsAndWindSickleFeedSickleWindsWhirlAShard()
        {
            Assert.IsTrue(Hazards.FeedsWithin(new Vector3(4.4f, 0f, 0f), Vector3.zero, 1.5f, 3f), "a dodge ending 3 m from the whirl's edge feeds it");
            Assert.IsFalse(Hazards.FeedsWithin(new Vector3(4.6f, 0f, 0f), Vector3.zero, 1.5f, 3f), "farther does not");
            Assert.IsTrue(Hazards.FeedsWithin(new Vector3(0f, 3f, 1f), Vector3.zero, 1.5f, 3f), "height does not count");

            var reaction = Upgrade.ById(AfflictionPairs.Id(AfflictionPairs.Index(AfflictionKind.Unsteady, AfflictionKind.Sundered))).Effects.Single(each => each.Do == UpgradeAction.Helper);
            Assert.AreEqual(Reactions.SickleWhirl, reaction.Named, "the whirl can be found by name");
            Assert.IsFalse(reaction.ActsAsGround, "a name only: it does not act as ground");
            Assert.AreEqual(4f, reaction.StaggerPerSpent, 1e-5f, "4 stagger a shard, as 3 damage a shard");
            Assert.AreEqual(3f, reaction.DamagePerSpent, 1e-5f);

            var bellows = Upgrade.ById(Clover.Player.Trees.Duo.SickleWindDuo.Bellows).Effects.Single();
            Assert.AreEqual(UpgradeTrigger.DodgeEnd, bellows.On);
            Assert.AreEqual(UpgradeAction.Feed, bellows.Do);
            Assert.AreEqual(Reactions.SickleWhirl, bellows.Named);
            Assert.AreEqual(3f, bellows.Reach, 1e-5f);
            Assert.AreEqual(3f, bellows.Damage, 1e-5f, "one shard's cut");
            Assert.AreEqual(4f, bellows.Stagger, 1e-5f);
            Assert.AreEqual(AfflictionKind.Unsteady, bellows.Affliction, "its next cut lays Unsteady");
            Assert.AreEqual(0f, bellows.Radius, 1e-5f, "no Unsteady on everything near any more");

            var sickle = Upgrade.ById(Clover.Player.Trees.Duo.SickleWindDuo.WindSickle).Effects;
            var tear = sickle.Single(each => each.Do == UpgradeAction.Feed);
            Assert.AreEqual(UpgradeTrigger.ChainEnd, tear.On);
            Assert.IsTrue(tear.TearsNeeds);
            Assert.AreEqual(AfflictionKind.Sundered, tear.Needs, "a notch torn from a Sundered enemy");
            Assert.AreEqual(AfflictionKind.None, tear.Affliction, "a shard, nothing laid");
            Assert.IsTrue(sickle.Any(each => each.Do == UpgradeAction.Cone), "the sickle still cuts");

            var downwind = Upgrade.ById(Clover.Player.Trees.Duo.SickleWindDuo.Shape).Shapes.Single(each => each.Name == "Sent Downwind").Effects.Single(each => each.Do == UpgradeAction.Hazard);
            Assert.AreEqual(Reactions.SickleWhirl, downwind.Named, "the wandering whirl is fed too");
        }

        [Test]
        public void IronThornsGrowAThornANotchThreeAtMost()
        {
            var thorns = new ConditionSpec { Id = Reactions.Thorns, Seconds = 6f, Blows = 3, BlowsByStacks = true };
            Assert.AreEqual(2, ConditionCaster.BlowsDrawn(thorns, 1), "one notch: one thorn");
            Assert.AreEqual(1, ConditionCaster.BlowsDrawn(thorns, 2));
            Assert.AreEqual(0, ConditionCaster.BlowsDrawn(thorns, 5), "three at most");
            Assert.AreEqual(0, ConditionCaster.BlowsDrawn(new ConditionSpec { Id = "x", Seconds = 1f, Blows = 3 }, 1), "a plain count starts at none");

            var enemy = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
            int slot = enemy.Conditions.Set(thorns, null, 1, null, true);
            enemy.Conditions.StartAt(slot, ConditionCaster.BlowsDrawn(thorns, 1));
            enemy.Conditions.Struck(5f, false, false);
            Assert.IsFalse(enemy.Conditions.Holds(Reactions.Thorns), "one notch, one thorn: gone after the first blow");

            var reaction = Upgrade.ById(AfflictionPairs.Id(AfflictionPairs.Index(AfflictionKind.Seeded, AfflictionKind.Sundered))).Effects;
            var count = reaction.First();
            Assert.AreEqual(UpgradeAction.Spend, count.Do);
            Assert.IsTrue(count.CountOnly, "the notches are counted, not spent");
            var grown = reaction.Single(each => each.State != null && each.State.Id == Reactions.Thorns);
            Assert.IsTrue(grown.PerStack && grown.State.BlowsByStacks);
        }

        [Test]
        public void DrawnFromTheWoodGivesGreenBloodForEachThingWithered()
        {
            var learn = Upgrade.ById(LifeTree.DrawnFromTheWood).Effects;
            Assert.AreEqual(UpgradeAction.BurstGrown, learn[0].Do, "the withering is counted first");
            var boons = learn.Where(each => each.Do == UpgradeAction.Boon).ToArray();
            Assert.AreEqual(2, boons.Length);
            foreach (var boon in boons)
            {
                Assert.AreEqual(1.5f, boon.SecondsEach, 1e-5f, "a second and a half a withered thing");
                Assert.AreEqual(10f, boon.Seconds, 1e-5f, "ten seconds at most");
                Assert.AreEqual(StatWhen.Always, boon.While, "not by standing near the garden any more");
            }
            Assert.AreEqual(16f, Upgrade.ById(SpellBranch.EmpowerId(LifeTree.DrawnFromTheWood)).Empower.Effects.Single().Damage, 1e-5f);
        }

        [Test]
        public void CairnsMasterDropsEveryStoneAgainAndPouredBronzeHoldsAGodAQuarter()
        {
            var again = Upgrade.ById(SpellBranch.MasterId(EarthTree.Cairn)).Effects.Single(each => each.Do == UpgradeAction.Falling);
            Assert.IsTrue(again.CountsNeeds && again.PerStack, "one stone again for each depth");
            Assert.AreEqual(12f, again.Damage, 1e-5f, "the stone's own fall");
            Assert.AreEqual(15f, again.Stagger, 1e-5f);

            var holds = Upgrade.ById(MetalTree.PouredBronze).Effects.Where(each => each.Do == UpgradeAction.Hold).ToArray();
            var plain = holds.Single(each => each.Ranks == EnemyRank.Plain);
            var great = holds.Single(each => each.Ranks == (EnemyRank.Elite | EnemyRank.God));
            Assert.AreEqual(plain.Seconds / 4f, great.Seconds, 1e-5f, "a quarter on a god or elite");
            Assert.IsTrue(great.Exact);

            var elite = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0), FreezeScale = 0.4f };
            Assert.IsTrue(elite.Hold(0.3f, true));
            Assert.AreEqual(0.3f, elite.HeldLeft, 1e-5f, "held as asked, not shortened again");
            var other = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0), FreezeScale = 0.4f };
            Assert.IsTrue(other.Hold(0.3f));
            Assert.AreEqual(0.12f, other.HeldLeft, 1e-5f, "a plain hold is its share");
        }

        [Test]
        public void WeighedMakesCriticalBlowsHarderForEachMark()
        {
            var weighed = Upgrade.ById(LightTree.Weighed);
            var mod = weighed.Stats.Single(each => each.Stat == PlayerStat.CritDamage);
            Assert.AreEqual(AfflictionKind.Marked, mod.Against);
            Assert.IsTrue(mod.PerDepth);
            Assert.IsEmpty(weighed.Effects ?? new UpgradeEffect[0], "no boon when a mark fills any more");

            var stats = new PlayerStats();
            stats.AddLearned(mod);
            var target = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
            Assert.AreEqual(0f, stats.Against(PlayerStat.CritDamage, target), 1e-5f, "unmarked: nothing");
            target.Lay(AfflictionKind.Marked, 1, 0f, 1f, LaySource.Weapon, null);
            Assert.AreEqual(0.1f, stats.Against(PlayerStat.CritDamage, target), 1e-5f, "one Mark");
            stats.AddLearned(new StatMod(PlayerStat.CritDamage, 0.2f, AfflictionKind.Marked));
            Assert.AreEqual(0.3f, stats.Against(PlayerStat.CritDamage, target), 1e-5f, "a plain one counts once");
        }

        [Test]
        public void StoneUnderfootSlowsThoseItsStridePasses()
        {
            var spell = Upgrade.ById(Clover.Player.Trees.Duo.TurnedToStoneDuo.StoneUnderfoot).Effects;
            var passed = spell.Single(each => each.AlongCast);
            Assert.AreEqual(UpgradeAction.Condition, passed.Do);
            Assert.AreEqual(1.5f, passed.Radius, 1e-5f, "within a metre and a half of the stride");
            Assert.AreEqual(2f, passed.State.Seconds, 1e-5f);
            Assert.AreEqual(0.7f, passed.State.Pace, 1e-5f, "30% slower");
            Assert.AreEqual(Clover.Player.Trees.Duo.TurnedToStoneDuo.UnderfootPassed, passed.State.Id);
            Assert.AreEqual(Clover.Player.Trees.Duo.TurnedToStoneDuo.UnderfootPassed, Upgrade.ById(Clover.Player.Trees.Duo.TurnedToStoneDuo.StoneUnderfoot).Effects.Last().State.Id, "last, after the stone is set");
            Assert.AreEqual(6, PlayerUpgrades.DodgePathSteps(8f, 1.5f), "an 8 m stride looked along at every point 1.5 m apart");
        }

        [Test]
        public void RedBeforeTheSunGathersTheBlowsItStopsIntoTheBeam()
        {
            var shape = Upgrade.ById(SpellBranch.ShapeId(LightTree.Daybreak)).Shapes.Single(each => each.Name == "Red Before the Sun");
            Assert.AreEqual(SpellForms.Wall, shape.Form, "a shield stands first");
            Assert.AreEqual(RingSource.Stopped, shape.Spell.RingsBy, "each blow it stops adds a ring");
            Assert.AreEqual(8, shape.Spell.Rings);
            Assert.AreEqual(0f, shape.Spell.Damage, 1e-5f, "the shield's fall strikes nothing: the beam is the blow");

            var learn = Upgrade.ById(LightTree.Daybreak);
            var learned = new System.Collections.Generic.HashSet<string> { LightTree.Daybreak, SpellBranch.EmpowerId(LightTree.Daybreak) };
            var plain = new SpellBranches().PlainSpecOf(learn, learned, new SpellSpec());
            Assert.AreEqual(15f, plain.Damage, 1e-5f, "the Learn's beam with its Empower");
            Assert.AreEqual(12f, plain.Reach, 1e-5f);

            var gather = new RingGather();
            plain.Rings = 8;
            gather.Begin(learn, plain, 7, 4.15f, learn.Form);
            Assert.AreEqual(SpellForms.Beam, gather.Form, "loosed through the Learn's beam, not the wall");
            Assert.IsTrue(gather.Add(1f) && gather.Add(2f) && gather.Add(3f));
            var loosed = new SpellSpec();
            Assert.IsTrue(gather.Loose(loosed));
            Assert.AreEqual(15f * 3f / 6f, loosed.Damage, 1e-4f, "three blows stopped: half the beam written for six");
            Assert.IsNull(gather.Form, "forgotten once loosed");
            Assert.IsTrue(Upgrade.ById(LightTree.Daybreak).Presses[0].Any(each => each.Do == UpgradeAction.Loose), "the second press looses it");
        }

        [Test]
        public void TheBurningLakeSlowsGrowsAndFlaresOnAFinisher()
        {
            var lake = Upgrade.ById(Clover.Player.Trees.Duo.SmoulderDuo.TheBurningLake);
            string ground = Clover.Player.Trees.Duo.SmoulderDuo.LakeGround;
            Assert.AreEqual(ground, lake.Spell.Ground, "the lake is your own ground");
            var slow = lake.Effects.Single(each => each.State != null && each.State.Id == Clover.Player.Trees.Duo.SmoulderDuo.LakeSlowed);
            Assert.AreEqual(0.8f, slow.State.Pace, 1e-5f, "a fifth slower");
            Assert.Greater(slow.State.Seconds, lake.Spell.Interval, "lasting past the next bite");
            var grow = lake.Effects.Single(each => each.Do == UpgradeAction.GrowGround);
            Assert.AreEqual(UpgradeTrigger.Kill, grow.On);
            Assert.AreEqual(ground, grow.TargetOnGround, "only a kill on the lake");
            Assert.AreEqual(0.5f, grow.Amount, 1e-5f);
            Assert.AreEqual(1f, grow.Seconds, 1e-5f);
            var flare = lake.Effects.Single(each => each.Do == UpgradeAction.EndCondition);
            Assert.AreEqual(UpgradeTrigger.FinisherHit, flare.On);
            Assert.AreEqual(Reactions.Sunk, flare.Condition, "every smoulder");
            Assert.AreEqual(ground, flare.TargetOnGround, "on the lake, and only those on it");
            Assert.Greater(flare.EachWithin, 0f);

            OwnGrounds.Lay(ground, Vector3.zero, 2f, 7f);
            Assert.IsFalse(OwnGrounds.Under(ground, new Vector3(2.3f, 0f, 0f), 1f));
            Assert.IsTrue(OwnGrounds.Grow(ground, new Vector3(1f, 0f, 0f), 0.5f, 1f, 1f), "a kill on it grows it");
            Assert.IsTrue(OwnGrounds.Under(ground, new Vector3(2.3f, 0f, 0f), 1f), "half a metre wider");
            Assert.IsTrue(OwnGrounds.Extent(ground, Vector3.zero, 1f, out float radius, out float until));
            Assert.AreEqual(2.5f, radius, 1e-5f);
            Assert.AreEqual(8f, until, 1e-5f, "a second longer");
            Assert.IsFalse(OwnGrounds.Grow(ground, new Vector3(9f, 0f, 0f), 0.5f, 1f, 1f), "a kill off it does nothing");
        }

        [Test]
        public void BlackPitchsMasterLightsTheCoatAndTheBurningGoUpAtHalf()
        {
            var master = Upgrade.ById(SpellBranch.MasterId(FireTree.BlackPitch)).Effects.Single();
            Assert.AreEqual(UpgradeTrigger.FinisherHit, master.On);
            Assert.AreEqual(FireTree.PitchCoat, master.InState, "a finisher on a coated enemy");
            Assert.AreEqual(UpgradeAction.LightCoat, master.Do, "the coat lit as a Burn would light it");
            Assert.AreEqual(0.5f, master.BurningShare, 1e-5f, "the others at half the coat's burst");
            Assert.AreEqual(10f, master.BurningWithin, 1e-5f);
            Assert.IsTrue(master.PaysLearn, "the spreads counted on the Learn's tally");
            var learnLight = Upgrade.ById(FireTree.BlackPitch).Effects.Single(each => each.Do == UpgradeAction.LightCoat);
            Assert.AreEqual(learnLight.Damage, master.Damage, 1e-5f);
            Assert.AreEqual(learnLight.Amount, master.Amount, 1e-5f);
            Assert.AreEqual(16f, PlayerUpgrades.CoatBurst(16f, 8f, 2, 70f) * master.BurningShare, 1e-5f, "two spreads: the coat 32, the others 16");
        }

        [Test]
        public void TheLampsGlowAndHighNoonMakeCleanGround()
        {
            Purified.Clear();
            Assert.IsFalse(Purified.Covers(Vector3.zero, 1f), "nothing clean yet");
            OwnGrounds.Lay(OwnGrounds.Glow, new Vector3(10f, 0f, 0f), 5f, 10f);
            Assert.IsTrue(Purified.Covers(new Vector3(13f, 0f, 0f), 1f), "your own glow is clean ground");
            Assert.IsFalse(Purified.Covers(new Vector3(13f, 0f, 0f), 11f), "while it lasts");
            Purified.Add(Vector3.zero, 2.5f, 2f);
            Assert.IsTrue(Purified.Covers(new Vector3(2f, 0f, 0f), 1f));
            Assert.IsFalse(Purified.Covers(new Vector3(3f, 0f, 0f), 1f), "only inside");
            Assert.IsFalse(Purified.Covers(Vector3.zero, 2.5f), "for its time");
            Purified.Clear();

            var circled = Upgrade.ById(SpellBranch.ShapeId(LightTree.StandingLight)).Shapes.Single(each => each.Name == "Circled Lamp").Effects.Single();
            Assert.AreEqual(UpgradeAction.Purify, circled.Do);
            Assert.IsTrue(circled.Follows, "it goes where you go");
            Assert.AreEqual(8f, circled.Seconds, 1e-5f);
            var noon = Upgrade.ById(SpellBranch.ShapeId(LightTree.Daybreak)).Shapes.Single(each => each.Name == "High Noon").Effects;
            var burnt = noon.Single(each => each.Do == UpgradeAction.Purify);
            Assert.AreEqual(UpgradeTrigger.SpellLoosed, burnt.On);
            Assert.AreEqual(2.5f, burnt.Radius, 1e-5f, "the column five metres across");
        }

        [Test]
        public void KeepingTheHourFillsInAFightAndMakesTheHeavyBlowOnItCritical()
        {
            var clock = new Clover.Player.Magic.HourClock();
            Assert.IsFalse(clock.Tick(5f, 6f, true));
            Assert.AreEqual(5f / 6f, clock.Share(6f), 1e-4f, "the dial five sixths full");
            Assert.IsTrue(clock.Tick(1.5f, 6f, true), "on the hour");
            Assert.IsTrue(clock.Ready);
            Assert.IsFalse(clock.Tick(10f, 6f, true), "it waits, full, for its heavy blow");
            Assert.IsTrue(clock.Ready);
            clock.Spend();
            Assert.IsFalse(clock.Ready);
            Assert.AreEqual(0f, clock.Share(6f), 1e-5f, "it fills again from the blow");
            clock.Tick(4f, 6f, true);
            clock.Tick(0.1f, 6f, false);
            Assert.AreEqual(0f, clock.Share(6f), 1e-5f, "out of a fight it empties");
            Assert.IsFalse(clock.Tick(10f, 6f, false), "and does not fill");

            var hour = Upgrade.ById(Clover.Player.Trees.Duo.EarthlightDuo.KeepingTheHour).Effects.Single();
            Assert.AreEqual(UpgradeTrigger.HeavyHit, hour.On);
            Assert.AreEqual(6f, hour.Hour, 1e-5f);
            Assert.AreEqual(AfflictionKind.Tremor, hour.Affliction);
            Assert.AreEqual(2, hour.Stacks);
            Assert.AreEqual(0f, hour.Cooldown, 1e-5f, "the hour, not a cooldown");
        }

        [Test]
        public void TheStoneKeepsTheHoursLineTurnsRoundItAndMakesTheMomentCritical()
        {
            Vector3 way = Hazards.SweepWay(Vector3.forward, 3f, 0.75f, out int turn);
            Assert.AreEqual(0, turn);
            Assert.AreEqual(1f, way.x, 1e-4f, "a quarter turn in three quarters of a second");
            Hazards.SweepWay(Vector3.forward, 3f, 7f, out turn);
            Assert.AreEqual(2, turn, "the third turn at seven seconds");

            var stone = Upgrade.ById(Clover.Player.Trees.Duo.EarthlightDuo.TheStoneKeepsTheHour);
            Assert.AreEqual(3f, stone.Spell.SweepTurn, 1e-5f, "one turn every three seconds");
            Assert.AreEqual(6f, stone.Spell.Radius, 1e-5f, "a line six metres long");
            var spec = new SpellSpec();
            SpellChange.Copy(stone.Spell, spec);
            Assert.AreEqual(3f, spec.SweepTurn, 1e-5f, "copied with the rest");
            var moment = stone.Effects.Single(each => each.State != null && each.State.Id == Clover.Player.Trees.Duo.EarthlightDuo.OnTheLine).State;
            Assert.AreEqual(ConditionFlags.TakesCritical, moment.Flags & ConditionFlags.TakesCritical, "a blow as the line crosses is critical");
            Assert.AreEqual(ConditionFlags.StaggerTakenShare, moment.Flags & ConditionFlags.StaggerTakenShare);
            Assert.AreEqual(2f, moment.Scale, 1e-5f, "and staggers double");
            Assert.Less(moment.Seconds, 0.5f, "only the moment");
        }

        [Test]
        public void NeedFiresTwoFiresHealBetweenThemAndBurnWhatIsShovedAcross()
        {
            var fire = Upgrade.ById(Clover.Player.Trees.Duo.FirebloomDuo.NeedFire);
            Assert.IsTrue(fire.Spell.Across, "a line across, a fire at each end");
            Assert.AreEqual(2f, fire.Spell.Radius, 1e-5f, "the fires four metres apart");
            Assert.AreEqual(OwnGrounds.NeedFireLine, fire.Spell.Ground);
            Assert.IsTrue(PlayerUpgrades.OwnGroundStates.Any(each => each.When == StatWhen.BetweenFires && each.Ground == OwnGrounds.NeedFireLine));
            var cut = fire.Stats.Single();
            Assert.AreEqual(PlayerStat.BlowCut, cut.Stat);
            Assert.AreEqual(StatWhen.BetweenFires, cut.When);
            Assert.AreEqual(1f, cut.Amount, 1e-5f, "twice the time off");
            var heal = fire.Effects.Single(each => each.Do == UpgradeAction.Heal);
            Assert.AreEqual(StatWhen.BetweenFires, heal.While);
            Assert.AreEqual(2f, heal.Amount, 1e-5f);
            var shoved = fire.Effects.Single(each => each.On == UpgradeTrigger.DrivenAcross);
            Assert.AreEqual(AfflictionKind.Burn, shoved.Affliction);
            Assert.GreaterOrEqual(shoved.Stacks, 9, "to full");

            OwnGrounds.LayLane(OwnGrounds.NeedFireLine, new Vector3(-2f, 0f, 3f), Vector3.right, 4f, 0.8f, 6f);
            Assert.IsTrue(OwnGrounds.Under(OwnGrounds.NeedFireLine, new Vector3(1f, 0f, 3.5f), 1f), "standing between the fires");
            Assert.IsFalse(OwnGrounds.Under(OwnGrounds.NeedFireLine, new Vector3(1f, 0f, 5f), 1f));
            Assert.IsTrue(OwnGrounds.Across(OwnGrounds.NeedFireLine, new Vector3(0f, 0f, 2f), new Vector3(0f, 0f, 4f), 1f), "shoved over the line");
            Assert.IsFalse(OwnGrounds.Across(OwnGrounds.NeedFireLine, new Vector3(3f, 0f, 2f), new Vector3(3f, 0f, 4f), 1f), "past the fire's end is not across");

            var stats = new PlayerStats();
            stats.AddLearned(cut);
            stats.SetWhen(StatWhen.BetweenFires, true);
            Assert.AreEqual(2f, stats.Multiplier(PlayerStat.BlowCut), 1e-5f);
            stats.SetWhen(StatWhen.BetweenFires, false);
            Assert.AreEqual(1f, stats.Multiplier(PlayerStat.BlowCut), 1e-5f);
        }

        [Test]
        public void FireSettingBanksAFireAndTheSecondPressQuenchesIt()
        {
            var fire = Upgrade.ById(Clover.Player.Trees.Duo.ThermalShockDuo.FireSetting);
            Assert.IsTrue(fire.Spell.Quiet, "no brand waiting for a blow: the fire is laid at once");
            string banked = Clover.Player.Trees.Duo.ThermalShockDuo.Banked;
            var lay = fire.Effects.Single(each => each.On == UpgradeTrigger.SpellCast);
            Assert.AreEqual(banked, lay.State.Id);
            Assert.AreEqual(4f, lay.State.Seconds, 1e-5f, "up to 4 s");
            Assert.AreEqual(1f, lay.State.Beat, 1e-5f);
            var beat = fire.Effects.Single(each => each.On == UpgradeTrigger.ConditionBeat);
            Assert.AreEqual(4f, beat.Damage, 1e-5f, "4 a second");
            Assert.AreEqual(AfflictionKind.Burn, beat.Affliction, "the Burn deepens each second");
            var press = fire.Presses[0].Single();
            Assert.AreEqual(UpgradeAction.EndCondition, press.Do, "the second press quenches it");
            Assert.AreEqual(banked, press.Condition);
            var quench = fire.Effects.Where(each => each.On == UpgradeTrigger.ConditionEnded && each.Do == UpgradeAction.Burst).ToArray();
            var full = quench.Single(each => !each.SparesTarget);
            Assert.AreEqual(20f, full.Damage, 1e-5f);
            Assert.AreEqual(8f, full.DamagePerSpent, 1e-5f, "8 a Burn");
            Assert.AreEqual(60f, full.Stagger, 1e-5f);
            Assert.AreEqual(3, full.Stacks, "Chill three deep");
            var half = quench.Single(each => each.SparesTarget);
            Assert.AreEqual(full.Damage / 2f, half.Damage, 1e-5f, "neighbours half");
            Assert.AreEqual(2.5f, half.Radius, 1e-5f);
            Assert.AreEqual(1.5f, PlayerUpgrades.HarderBy(0.1f, 5, 5), 1e-5f, "five blows: half again");
            Assert.AreEqual(1.5f, PlayerUpgrades.HarderBy(0.1f, 9, 5), 1e-5f, "to 50% at most");
            Assert.AreEqual(1.2f, PlayerUpgrades.HarderBy(0.1f, 2, 5), 1e-5f);
            Assert.AreEqual(UpgradeAction.Spend, fire.Effects.Last().Do, "all its Burn spent last");
        }

        [Test]
        public void TheExilesGrudgePaysEachAttackerTwiceWhatItDealt()
        {
            var tally = new Clover.Player.Magic.HarmTally();
            var first = new object();
            var second = new object();
            {
                tally.Add(first, 10f);
                Assert.AreEqual(0, tally.Count, "nothing counted while closed");
                tally.Begin(3f);
                tally.Add(first, 10f);
                tally.Add(second, 4f * 2f);
                tally.Add(first, 6f);
                Assert.AreEqual(2, tally.Count, "each attacker tied once");
                Assert.AreEqual(16f, tally.Dealt(0), 1e-5f);
                Assert.AreEqual(8f, tally.Dealt(1), 1e-5f, "a guarded blow of 4 counts as 8");
                Assert.IsFalse(tally.Due(2.9f));
                Assert.IsTrue(tally.Due(3f), "the cloud breaks at 3 s");
                tally.Close();
                Assert.AreEqual(0, tally.Count);
                Assert.IsFalse(tally.Open);
            }
            Assert.AreEqual(32f, PlayerUpgrades.PaidBack(16f, 2f, 60f), 1e-5f, "twice what it dealt");
            Assert.AreEqual(60f, PlayerUpgrades.PaidBack(45f, 2f, 60f), 1e-5f, "60 at most");

            var grudge = Upgrade.ById(Clover.Player.Trees.Duo.TheGrudgeDuo.TheExilesGrudge);
            Assert.IsTrue(grudge.Spell.Quiet, "no parry: the blows still land");
            Assert.AreEqual(3f, grudge.Effects.Single(each => each.Do == UpgradeAction.TallyHarm).Seconds, 1e-5f);
            var bolt = grudge.Effects.Single(each => each.On == UpgradeTrigger.HarmPaid && each.Do == UpgradeAction.Burst);
            Assert.AreEqual(12f, bolt.Damage, 1e-5f);
            Assert.AreEqual(2f, bolt.PowerShare, 1e-5f);
            Assert.AreEqual(60f, bolt.PowerMost, 1e-5f);
            Assert.AreEqual(AfflictionKind.Static, bolt.Affliction);
            Assert.AreEqual(3, bolt.Stacks);
            Assert.IsTrue(grudge.Effects.Any(each => each.On == UpgradeTrigger.HarmPaid && each.Affliction == AfflictionKind.Cursed));
        }

        [Test]
        public void OneNooseForTwoSharesHalfTheFinishersStagger()
        {
            var noose = Upgrade.ById(Clover.Player.Trees.Duo.BlackChainDuo.OneNooseForTwo).Effects.Single();
            Assert.IsTrue(noose.ToChainMate && noose.ByPower);
            Assert.AreEqual(0.5f, noose.Share, 1e-5f, "half its damage");
            Assert.AreEqual(0.5f, noose.StaggerOfBlow, 1e-5f, "half its stagger");
            Assert.AreEqual(0f, noose.Stagger, 1e-5f, "no set 30 any more");
        }

        [Test]
        public void EarthlightsSeamsAreGroundAGuardAndAFlashCanFind()
        {
            var plain = Upgrade.ById(AfflictionPairs.Id(AfflictionPairs.Index(AfflictionKind.Tremor, AfflictionKind.Marked))).Effects.Single(each => each.Seams > 0);
            Assert.AreEqual(OwnGrounds.Seams, plain.Named, "the seams glow as your own ground");
            Assert.Greater(plain.GroundSeconds, 0f);
            var lit = Upgrade.ById(Clover.Player.Trees.Duo.EarthlightDuo.Shape).Shapes.Single(each => each.Name == "Lit Seam").Effects;
            Assert.AreEqual(5f, lit.Single(each => each.On == UpgradeTrigger.Reacted && each.Seams > 0).GroundSeconds, 1e-5f, "lit 5 s");
            Assert.AreEqual(OwnGrounds.Seams, lit.Single(each => each.On == UpgradeTrigger.ConditionStruck).AtOwnGround, "the flash where the seams were laid");
            var dawn = Upgrade.ById(Clover.Player.Trees.Duo.EarthlightDuo.DawnThroughTheDoor).Effects;
            Assert.IsTrue(dawn.All(each => each.While == StatWhen.OnSeams && each.Needs == AfflictionKind.None), "only a guard made on the seams");
            Assert.IsTrue(PlayerUpgrades.OwnGroundStates.Any(each => each.When == StatWhen.OnSeams && each.Ground == OwnGrounds.Seams));

            for (int seam = 0; seam < 6; seam++)
                OwnGrounds.LayLane(OwnGrounds.Seams, new Vector3(5f, 0f, 5f), GroundRules.SeamWay(Vector3.forward, seam, 6), 3f, GroundRules.SeamHalf, 5f);
            Assert.IsTrue(OwnGrounds.Under(OwnGrounds.Seams, new Vector3(5f, 0f, 7f), 1f), "on a seam");
            Assert.IsFalse(OwnGrounds.Under(OwnGrounds.Seams, new Vector3(6.6f, 0f, 7.4f), 1f), "between two seams");
            Assert.IsTrue(OwnGrounds.NearestLane(OwnGrounds.Seams, new Vector3(7f, 0f, 5f), 1f, 6f, out Vector3 at, out Vector3 way));
            Assert.AreEqual(5f, at.x, 1e-4f, "the star's middle, where it was laid");
            Assert.AreEqual(5f, at.z, 1e-4f);
            Assert.IsFalse(OwnGrounds.NearestLane(OwnGrounds.Seams, new Vector3(20f, 0f, 5f), 1f, 6f, out _, out _), "too far");
        }

        [Test]
        public void GlassStormsShardsAreFlungByWeaponBlows()
        {
            var storm = Upgrade.ById(Clover.Player.Trees.Duo.SleetChargeDuo.GlassStorm);
            Assert.AreEqual(8f, storm.Spell.FlingDamage, 1e-5f, "a flung shard strikes for 8");
            Assert.AreEqual(1, DiscsForm.FlingsFor(false), "a blow flings one");
            Assert.GreaterOrEqual(DiscsForm.FlingsFor(true), storm.Spell.Count, "a heavy blow flings all that are left");
            var at = new[] { new Vector3(2f, 0f, 0f), new Vector3(0f, 0f, 2f), new Vector3(-2f, 0f, 0f) };
            Assert.AreEqual(1, DiscsForm.Nearest(0b111, at, 3, new Vector3(0f, 0f, 5f)), "the nearest shard");
            Assert.AreEqual(0, DiscsForm.Nearest(0b101, at, 3, new Vector3(0f, 0f, 5f)), "of those still turning");
            Assert.AreEqual(-1, DiscsForm.Nearest(0, at, 3, Vector3.zero), "none left");
            Assert.IsTrue(storm.Effects.Single().EveryOnEnemy == 2, "a Static every other pass, counted on each enemy");
        }

        [Test]
        public void TsukaAteIsTheRollsHeavyThroughAGuard()
        {
            var plain = Clover.Player.Way.Armoury.Resolve(Weapons.KatanaName, Clover.Player.Trees.Ways.KatanaWay.Stream, new Clover.Player.Way.MarkSet(Clover.Player.Trees.Ways.KatanaWay.Stream));
            Assert.IsNull(plain.Combo.DashHeavy, "no hilt out of the roll without the mark");
            var hilt = Clover.Player.Way.Armoury.Resolve(Weapons.KatanaName, Clover.Player.Trees.Ways.KatanaWay.Stream,
                new Clover.Player.Way.MarkSet(Clover.Player.Trees.Ways.KatanaWay.Stream, Clover.Player.Trees.Ways.KatanaWay.TsukaAte)).Combo.DashHeavy;
            Assert.IsNotNull(hilt);
            Assert.AreEqual(6f, hilt.Damage, 1e-5f);
            Assert.AreEqual(45f, hilt.Stagger, 1e-5f);
            Assert.AreEqual(1f, hilt.OpensGuard, 1e-5f, "a raised guard broken for a second");
            Assert.IsTrue(hilt.Heavy);

            var combo = new WeaponCombo { Heavy = new AttackStep { Name = "heavy", Heavy = true, Windup = 0.1f, Active = 0.1f, Recovery = 0.1f }, DashHeavy = hilt };
            combo.Steps.Add(new AttackStep { Name = "light", Windup = 0.1f, Active = 0.1f, Recovery = 0.1f });
            var stamina = new StaminaPool();
            stamina.Fill();
            combo.PressHeavy(false, true);
            combo.Tick(0.01f, stamina, null);
            Assert.AreSame(hilt, combo.Current, "a heavy press as the roll ends is the hilt");
        }

        [Test]
        public void TheSwarmSealsANewJarAndTheRotHoldsTheBalance()
        {
            var shapes = Upgrade.ById(BlightTree.Shape).Shapes;
            var swarm = shapes.Single(each => each.Name == "Swarm").Effects;
            var seal = swarm.Single(each => each.On == UpgradeTrigger.ConditionEnded && each.Do == UpgradeAction.Condition);
            Assert.AreEqual(BlightTree.SwarmJar, seal.State.Id, "a new jar");
            Assert.AreEqual(8f, seal.EachWithin, 1e-5f, "on the nearest enemy within 8 m");
            Assert.IsTrue(seal.EachOther, "not the one whose jar broke");
            Assert.AreEqual(1, seal.Count, "one");
            Assert.AreEqual(AfflictionKind.Poison, seal.State.Keeps);

            var rot = shapes.Single(each => each.Name == "Rot to the Bone").Effects;
            var sealedRot = rot.Single(each => each.On == UpgradeTrigger.Reacted && each.State != null);
            Assert.AreEqual(ConditionFlags.BalanceHeld, sealedRot.State.Flags & ConditionFlags.BalanceHeld, "the meter does not recover while sealed");
            Assert.IsTrue(sealedRot.State.OnBite, "its beat is the poison's bite");
            var bite = rot.Single(each => each.On == UpgradeTrigger.ConditionBeat);
            Assert.AreEqual(BlightTree.RotJar, bite.Condition);
            Assert.IsTrue(bite.CountsNeeds, "by the doses it carries");
            Assert.AreEqual(3f, bite.Stagger, 1e-5f, "3 a dose");
            Assert.IsTrue(bite.PerStack);
            Assert.AreEqual(0f, sealedRot.State.Beat, 1e-5f, "no clock of a second any more");

            var jar = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
            jar.Conditions.Set(sealedRot.State, null);
            Assert.IsFalse(jar.BalanceRecovers, "held while sealed");
        }

        [Test]
        public void FlashoversLiveFlameDrawsDischargesIntoIt()
        {
            var flashover = Upgrade.ById(AfflictionPairs.Id(AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Static))).Effects;
            var live = flashover.Single(each => each.State != null && each.State.Id == Reactions.LiveFlame).State;
            Assert.AreEqual(6f, live.DischargesWithin, 1e-5f, "every discharge within 6 m");
            var bent = flashover.Single(each => each.On == UpgradeTrigger.ConditionStruck);
            Assert.AreEqual(StruckBy.Discharge, bent.By);
            Assert.AreEqual(5f, bent.Damage, 1e-5f, "5 a Burn stack");
            Assert.IsTrue(bent.PerStack && bent.CountsNeeds && bent.Needs == AfflictionKind.Burn);
            Assert.AreEqual(2.5f, bent.Radius, 1e-5f);
            Assert.AreEqual(0.8f, bent.Cooldown, 1e-5f, "once every 0.8 s");
            Assert.IsTrue(bent.CooldownPerEnemy, "on each flame");

            var discharge = new ConditionNews { DischargeNear = true };
            Assert.IsTrue(PlayerReactions.Answers(StruckBy.Discharge, discharge));
            Assert.IsFalse(PlayerReactions.Answers(StruckBy.Weapon, discharge), "a discharge is no blow");
            Assert.IsFalse(PlayerReactions.Answers(StruckBy.Discharge, new ConditionNews()), "a blow is no discharge");

            var set = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
            int slot = set.Conditions.Set(live, null);
            Assert.AreEqual(6f, set.Conditions.Run(slot).DischargesWithin, 1e-5f, "the run keeps its reach");
        }

        [Test]
        public void TheFreezeMomentCountsForHangingIceLensOfIceAndNoSummer()
        {
            var cold = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
            Assert.IsFalse(cold.FrozeWithin(0.3f), "not frozen");
            Assert.AreEqual(float.MaxValue, cold.SinceShatter);

            var master = Upgrade.ById(SpellBranch.MasterId(IceTree.HangingIce)).Effects;
            Assert.IsTrue(master.All(each => each.FrozeWithin == 0.3f), "only landing within 0.3 s of the freeze");
            Assert.AreEqual(3f, master.Single(each => each.Do == UpgradeAction.Shatter).Amount, 1e-5f, "four times over with the Learn's twice");
            Assert.IsTrue(master.Any(each => each.Do == UpgradeAction.ResetCooldown));
            Assert.AreEqual(2f, Upgrade.ById(IceTree.HangingIce).Effects.Single(each => each.Do == UpgradeAction.Shatter).Amount, 1e-5f, "90 on a Frozen enemy");

            var lens = Upgrade.ById(Clover.Player.Trees.Duo.MockSunsDuo.Shape).Shapes.Single(each => each.Name == "Lens of Ice").Effects;
            var held = lens.Single(each => each.On == UpgradeTrigger.Reacted).State;
            Assert.IsTrue(held.KeepsWhileFrozen, "the Mark kept only while Frozen");
            Assert.AreEqual(1.6f, held.ShatterHarder, 1e-5f, "60% harder");
            var thrown = lens.Single(each => each.On == UpgradeTrigger.ConditionStruck);
            Assert.AreEqual(StruckBy.Shatter, thrown.By);
            Assert.AreEqual(4f, thrown.Radius, 1e-5f);

            var set = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
            set.Conditions.Set(held, null);
            set.Conditions.CarrierFrozen = false;
            Assert.IsFalse(set.Conditions.KeepsKind(AfflictionKind.Marked), "not frozen: the Mark is spent as usual");
            set.Conditions.CarrierFrozen = true;
            Assert.IsTrue(set.Conditions.KeepsKind(AfflictionKind.Marked), "frozen: kept");
            Assert.IsTrue(set.Conditions.KeepsFrom(AfflictionKind.Marked, -1f));

            var noSummer = Upgrade.ById(Clover.Player.Trees.Duo.StolenWarmthDuo.NoSummer);
            var free = noSummer.Stats.Single();
            Assert.AreEqual(PlayerStat.SurgeDrain, free.Stat, "Hunger's drain, not every health price");
            Assert.AreEqual(StatWhen.FrozenNear, free.When);
            Assert.AreEqual(8f, PlayerUpgrades.FrozenNearWithin, 1e-5f);
        }
    }
}

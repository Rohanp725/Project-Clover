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
        }

        [TearDown]
        public void End()
        {
            StandingThings.Clear();
            Pickups.Clear();
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
    }
}

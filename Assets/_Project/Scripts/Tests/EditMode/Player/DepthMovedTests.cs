using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using Clover.Player.Trees;
using Clover.Player.Trees.Duo;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// Depth moved between enemies (standin_clearance_plan.md pass 2, the ledger's depth-moved) and Paper Seal's thunder
    /// (seal-thunder): the sums, the edge's coats, and the marks that use them.
    /// </summary>
    public class DepthMovedTests
    {
        private static AfflictionSet Poisoned(int doses)
        {
            var set = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
            if (doses > 0) set.Lay(AfflictionKind.Poison, doses, 0f, 1f, LaySource.Spell);
            return set;
        }

        [Test]
        public void EveryOnePassedIsRaisedToTheSickestOnesDepth()
        {
            var light = Poisoned(1);
            var sick = Poisoned(6);
            var clean = Poisoned(0);
            var sets = new[] { light, sick, clean };
            Assert.AreEqual(6, DepthMoves.Sickest(sets, AfflictionKind.Poison, 2));
            Assert.AreEqual(2, DepthMoves.Sickest(new[] { clean }, AfflictionKind.Poison, 2), "two at least");
            Assert.AreEqual(5, DepthMoves.ToRaise(light, AfflictionKind.Poison, 6));
            Assert.AreEqual(0, DepthMoves.ToRaise(sick, AfflictionKind.Poison, 6), "the sickest is there already");
            Assert.AreEqual(light.DepthOf(AfflictionKind.Poison) - 1, DepthMoves.ToRaise(light, AfflictionKind.Poison, 99), "no deeper than it can lie");
        }

        [Test]
        public void DosesMoveOnlyAsDeepAsTheTakerCanHold()
        {
            var joined = Poisoned(3);
            int depth = Poisoned(0).DepthOf(AfflictionKind.Poison);
            var host = Poisoned(depth - 1);
            Assert.AreEqual(1, DepthMoves.Movable(joined, host, AfflictionKind.Poison, 3), "the host has room for one");
            host = Poisoned(depth);
            Assert.AreEqual(0, DepthMoves.Movable(joined, host, AfflictionKind.Poison, 1), "a full host draws nothing out");
            Assert.AreEqual(2, DepthMoves.Fits(Poisoned(depth - 2), AfflictionKind.Poison, 5, out int over));
            Assert.AreEqual(3, over, "what is pushed past its depth");
        }

        [Test]
        public void ALineOfBurstsCarriesItsDosesOnSoTheLastIsTheLargest()
        {
            int carried = 0;
            Assert.AreEqual(3, DepthMoves.LineBurst(3, ref carried));
            Assert.AreEqual(5, DepthMoves.LineBurst(2, ref carried));
            Assert.AreEqual(5, DepthMoves.LineBurst(0, ref carried), "one with none of its own still bursts with what came to it");
            Assert.AreEqual(9, DepthMoves.LineBurst(4, ref carried));
            Assert.AreEqual(9, carried);
        }

        [Test]
        public void TheEdgeCarriesItsCoatForItsBlowsOnAnyEnemy()
        {
            var coat = new BladeCoat();
            var kinds = new AfflictionKind[BladeCoat.Most];
            var stacks = new int[BladeCoat.Most];
            coat.Load(AfflictionKind.Poison, 6, true, false, 5, 0f);
            Assert.IsTrue(coat.Any);
            var first = Poisoned(1);
            Assert.AreEqual(1, coat.Blow(first, false, 0f, kinds, stacks));
            Assert.AreEqual(5, stacks[0], "set to the sickest one's depth");
            coat.Swung();
            Assert.AreEqual(0, coat.Blow(Poisoned(6), true, 0f, kinds, stacks), "one already that deep takes nothing");
            coat.Swung();
            Assert.AreEqual(3, coat.BlowsLeft(AfflictionKind.Poison), "and still the blow is counted");
            for (int i = 0; i < 3; i++)
            {
                coat.Blow(Poisoned(0), false, 0f, kinds, stacks);
                coat.Swung();
            }
            Assert.IsFalse(coat.Any, "five blows and it is spent");

            coat.Load(AfflictionKind.Unsteady, 1, false, true, 3, 0f);
            Assert.AreEqual(0, coat.Blow(Poisoned(0), true, 0f, kinds, stacks), "light blows only");
            coat.Swung();
            Assert.AreEqual(1, coat.Blow(Poisoned(0), false, 0f, kinds, stacks));
            Assert.AreEqual(AfflictionKind.Unsteady, kinds[0]);
            Assert.AreEqual(1, stacks[0], "a dose each light blow");
            coat.Swung();
            Assert.AreEqual(2, coat.BlowsLeft(AfflictionKind.Unsteady), "the heavy blow was not counted");
            coat.Swung();
            Assert.AreEqual(2, coat.BlowsLeft(AfflictionKind.Unsteady), "a swing that struck nothing is not counted");
            coat.Load(AfflictionKind.Unsteady, 1, false, true, 3, 5f);
            Assert.AreEqual(0, coat.Blow(Poisoned(0), false, 6f, kinds, stacks), "a timed coat is gone once its time is up");
        }

        [Test]
        public void ShareAlikeAndItsShapesMoveTheDepth()
        {
            var learn = Upgrade.ById(PoisonTree.ShareAlike).Effects;
            Assert.IsTrue(learn.Any(effect => effect.On == UpgradeTrigger.SpellHit && effect.Do == UpgradeAction.EvenDepth && effect.Affliction == AfflictionKind.Poison && effect.Stacks == 2),
                "each passed raised to the sickest's depth, two at least");
            var shapes = Upgrade.ById(SpellBranch.ShapeId(PoisonTree.ShareAlike)).Shapes;
            var blade = shapes.First(shape => shape.Name == "Carried on the Blade").Effects.Single();
            Assert.AreEqual(UpgradeAction.CoatBlade, blade.Do);
            Assert.IsTrue(blade.SetsDepth);
            Assert.AreEqual(5, blade.Count, "five blows");
            Assert.IsFalse(shapes.SelectMany(shape => shape.Effects ?? new UpgradeEffect[0]).Any(effect => effect.Do == UpgradeAction.Boon), "no more two doses a blow for 5 s");
        }

        [Test]
        public void OneByOneDrawsItsLineUnderThePoisonedAndCarriesTheDosesOn()
        {
            var spell = Upgrade.ById(PoisonTree.OneByOne);
            Assert.AreEqual(AfflictionKind.Poison, spell.Spell.OnlyCarrying, "under the poisoned alone");
            var burst = spell.Effects.Single(effect => effect.Do == UpgradeAction.SetOff);
            Assert.IsTrue(burst.PassesOn, "each passing its doses to the next");
            var shapes = Upgrade.ById(SpellBranch.ShapeId(PoisonTree.OneByOne)).Shapes;
            var gather = shapes.First(shape => shape.Name == "All Into One").Effects.Single();
            Assert.AreEqual(UpgradeAction.Gather, gather.Do);
            Assert.AreEqual(UpgradeTrigger.SpellCast, gather.On, "gathered before the pillar bursts");
            Assert.AreEqual(10f, gather.Radius, 1e-5f);
            var eaten = shapes.First(shape => shape.Name == "Eaten in Turn").Effects.Single(effect => effect.Do == UpgradeAction.Pass);
            Assert.AreEqual(PoisonTree.EatenInTurn, eaten.ToState, "to the next on the line");
            Assert.IsTrue(eaten.Overflow, "one pushed past eight bursts and passes on");
        }

        [Test]
        public void TheGritCoatsTheEdgeAndTheRootsDrinkIntoTheHost()
        {
            var dust = Upgrade.ById(IllWindDuo.Shape).Shapes.First(shape => shape.Name == "Dust on the Blade").Effects;
            var coats = dust.Where(effect => effect.Do == UpgradeAction.CoatBlade).ToArray();
            Assert.AreEqual(2, coats.Length, "a dose and an Unsteady");
            Assert.IsTrue(coats.All(effect => effect.LightOnly && effect.Count == 3 && !effect.SetsDepth), "three light blows on any enemy");
            Assert.IsFalse(dust.Any(effect => effect.Condition == IllWindDuo.Coat), "no longer only on the one shed");
            var gathered = Upgrade.ById(RotRootsDuo.Shape).Shapes.First(shape => shape.Name == "Gathered In").Effects;
            var drink = gathered.Single(effect => effect.Do == UpgradeAction.Pass);
            Assert.IsTrue(drink.ToLink, "into the host itself");
            Assert.IsFalse(gathered.Any(effect => effect.Do == UpgradeAction.Spend), "a dose leaves only when the host can take it");
        }

        [Test]
        public void PaperSealsThunderFallsAtTheFifthOrAtHalf()
        {
            Assert.AreEqual(30f + 4f * 5 + 7f * 2, SealRules.Thunder(30f, 4f, 5, 2, 7f, false), 1e-4f);
            Assert.AreEqual(0.5f * (30f + 4f * 5), SealRules.Thunder(30f, 4f, 5, 0, 7f, true), 1e-4f, "the time ran out first");
            Assert.AreEqual(0.5f, SealRules.Share(true), 1e-5f);
            var seal = Upgrade.ById(NerveFireDuo.PaperSeal);
            Assert.IsTrue(seal.Spell.Seal);
            Assert.AreEqual(5, seal.Spell.Count, "the fifth blow");
            Assert.AreEqual(4f, seal.Spell.PerDose, 1e-5f);
            Assert.AreEqual(3f, seal.Spell.PassReach, 1e-5f, "each dose to all within 3 m");
            Assert.AreEqual(3f, seal.Spell.StepReach, 1e-5f, "the step to the enemy");
            Assert.AreEqual(6f, seal.Spell.Seconds, 1e-5f);
            var holds = seal.Effects.Where(effect => effect.Do == UpgradeAction.Hold).ToArray();
            Assert.IsTrue(holds.Any(effect => effect.Ranks == EnemyRank.Plain && effect.Seconds == 1.2f));
            Assert.IsTrue(holds.Any(effect => effect.Ranks == EnemyRank.Elite && effect.Seconds * 0.4f > 0.49f && effect.Seconds * 0.4f < 0.51f), "an elite 0.5 s");
            Assert.IsFalse(holds.Any(effect => (effect.Ranks & EnemyRank.God) != 0), "a god not at all");
        }

        [Test]
        public void ASpellsCopyKeepsWhatItSeeksAndHowItEnds()
        {
            var from = new SpellSpec { AimAt = AfflictionKind.Soaked, AimAt2 = AfflictionKind.Marked, AimFarthest = true, AimMissShare = 0.5f, OnlyCarrying = AfflictionKind.Poison,
                Seal = true, PerDose = 4f, PassReach = 3f, StepReach = 3f };
            var into = new SpellSpec();
            SpellChange.Copy(from, into);
            Assert.AreEqual(AfflictionKind.Soaked, into.AimAt);
            Assert.AreEqual(AfflictionKind.Marked, into.AimAt2);
            Assert.IsTrue(into.AimFarthest);
            Assert.AreEqual(0.5f, into.AimMissShare, 1e-5f);
            Assert.AreEqual(AfflictionKind.Poison, into.OnlyCarrying);
            Assert.IsTrue(into.Seal);
            Assert.AreEqual(4f, into.PerDose, 1e-5f);
            Assert.AreEqual(3f, into.PassReach, 1e-5f);
            Assert.AreEqual(3f, into.StepReach, 1e-5f);
        }
    }
}

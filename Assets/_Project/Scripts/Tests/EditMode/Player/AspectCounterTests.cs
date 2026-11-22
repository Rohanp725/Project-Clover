using Clover.Player;
using Clover.Player.Trees.Ways;
using Clover.Player.Way;
using Clover.Player.Way.Aspects;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The aspects' counts kept in the player's <see cref="AspectRun"/> (way_build_plan.md 2.7, "aspect counters"; 3.2 and
    /// 3.5; questions 15, 18 and 22): the Storm Katana's seven drums, beaten by cuts that land (and by perfect guards and
    /// finishers under Drumbeat), spent by any draw for +40 stagger, fading out of a fight unless Rolling Drums keeps
    /// them; and A Hundred Days' draws in a row, every fourth leaving its cut hanging, a blow taken setting it back.
    /// </summary>
    public class AspectCounterTests
    {
        private static AspectContext Step(int landed = 0, bool guard = false, bool finished = false, bool inFight = true, bool struck = false, float seconds = 0.016f, params string[] marks) =>
            new AspectContext { DeltaTime = seconds, Landed = landed, PerfectGuard = guard, Finished = finished, InFight = inFight, Struck = struck, Marks = new MarkSet(marks) };

        [Test]
        public void EachCutThatLandsBeatsADrumAndTheSeventhsDrawSpendsThemForFortyStagger()
        {
            var storm = new StormKatanaBough();
            var run = new AspectRun();
            for (int i = 0; i < 6; i++) storm.Tick(ref run, Step(1, marks: KatanaWay.SevenDrums));
            Assert.AreEqual(6, run.Count);
            Assert.AreEqual(0f, storm.Blow(ref run, null, true, new MarkSet(KatanaWay.SevenDrums)).Stagger, "six are not enough");
            storm.Tick(ref run, Step(3, marks: KatanaWay.SevenDrums));
            Assert.AreEqual(7, run.Count, "never more than seven");
            Assert.AreEqual(0f, storm.Blow(ref run, null, false, new MarkSet(KatanaWay.SevenDrums)).Stagger, "only a draw spends them");
            Assert.AreEqual(40f, storm.Blow(ref run, null, true, new MarkSet(KatanaWay.SevenDrums)).Stagger);
            Assert.AreEqual(0, run.Count, "spent");
        }

        [Test]
        public void WithoutSevenDrumsNothingIsCountedAndDrumbeatAddsTwoForGuardsAndFinishers()
        {
            var storm = new StormKatanaBough();
            var run = new AspectRun();
            storm.Tick(ref run, Step(5, guard: true));
            Assert.AreEqual(0, run.Count, "no Seven Drums, no drums");
            storm.Tick(ref run, Step(0, guard: true, finished: true, marks: new[] { KatanaWay.SevenDrums }));
            Assert.AreEqual(0, run.Count, "guards and finishers beat only under Drumbeat");
            storm.Tick(ref run, Step(1, guard: true, finished: true, marks: new[] { KatanaWay.SevenDrums, KatanaWay.Drumbeat }));
            Assert.AreEqual(5, run.Count, "a cut, two for the guard, two for the finisher");
        }

        [Test]
        public void DrumsFadeOutOfAFightUnlessRollingDrumsKeepsThem()
        {
            var storm = new StormKatanaBough();
            var run = new AspectRun { Count = 7 };
            storm.Tick(ref run, Step(inFight: false, seconds: Drums.FadeSeconds - 0.1f, marks: KatanaWay.SevenDrums));
            Assert.AreEqual(7, run.Count, "not yet");
            storm.Tick(ref run, Step(inFight: false, seconds: 0.2f, marks: KatanaWay.SevenDrums));
            Assert.AreEqual(0, run.Count, "gone quiet");
            run.Count = 7;
            storm.Tick(ref run, Step(inFight: false, seconds: 60f, marks: new[] { KatanaWay.SevenDrums, KatanaWay.RollingDrums }));
            Assert.AreEqual(7, run.Count, "seven beaten wait until spent");
        }

        [Test]
        public void EveryFourthDrawInARowLeavesItsCutHangingAndABlowTakenStartsOver()
        {
            var sheath = new SheathAspect();
            var run = new AspectRun();
            var marks = new MarkSet(KatanaWay.Sheath, KatanaWay.AHundredDays);
            Assert.IsFalse(sheath.Blow(ref run, null, true, marks).HangingCut);
            Assert.IsFalse(sheath.Blow(ref run, null, true, marks).HangingCut);
            Assert.IsFalse(sheath.Blow(ref run, null, false, marks).HangingCut, "not a draw: not counted");
            Assert.IsFalse(sheath.Blow(ref run, null, true, marks).HangingCut);
            Assert.IsTrue(sheath.Blow(ref run, null, true, marks).HangingCut, "the fourth draw");
            sheath.Tick(ref run, Step(struck: true));
            Assert.AreEqual(0, run.Count, "a blow taken sets the count to nothing");
            for (int i = 0; i < 4; i++) Assert.IsFalse(sheath.Blow(ref run, null, true, new MarkSet(KatanaWay.Sheath)).HangingCut, "without A Hundred Days");
            Assert.AreEqual(0.6f, SheathAspect.HangSeconds, 1e-6f);
            Assert.AreEqual(0.4f, SheathAspect.HangShare, 1e-6f);
        }

        [Test]
        public void TheArmouryNamesTheAspectItBuiltIn()
        {
            Assert.IsNull(Armoury.Resolve(Weapons.KatanaName, "", MarkSet.None).Aspect);
            Assert.IsInstanceOf<SheathAspect>(Armoury.Resolve(Weapons.KatanaName, KatanaWay.Sheath, new MarkSet(KatanaWay.Sheath)).Aspect);
            Assert.IsInstanceOf<StormKatanaBough>(Armoury.Resolve(Weapons.StormKatanaName, "", MarkSet.None).Aspect);
            Assert.IsNull(Armoury.Resolve(Weapons.KatanaName, KatanaWay.Sheath, MarkSet.None).Aspect, "not in force: plain");
        }
    }
}

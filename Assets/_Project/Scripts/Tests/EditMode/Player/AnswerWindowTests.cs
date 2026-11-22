using Clover.Player;
using Clover.Player.Trees.Ways;
using Clover.Player.Way;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The Answer's moment to answer (way_build_plan.md 2.7 and 3.4): a perfect guard opens it for 0.6 s (0.8 with Open
    /// Window, 1.0 after a red attack with Catch the Red); light is the Riposte (24, critical, 20 stagger), heavy Through
    /// the Gap (30, 40, through the attacker); one answer a perfect guard, a second in the same moment only with Second
    /// Answer and +30; Patient Blade's +10 a half second of guard held unstruck, to +40; Kiri-otoshi answers by itself.
    /// </summary>
    public class AnswerWindowTests
    {
        private static Weapon Answer(params string[] marks) => Armoury.Resolve(Weapons.KatanaName, KatanaWay.Answer, new MarkSet(marks));

        private static AspectContext Step(MarkSet marks, bool guard = false, bool red = false, float seconds = 0.016f, bool guarding = false, bool guardHit = false, bool struck = false) =>
            new AspectContext { DeltaTime = seconds, PerfectGuard = guard, RedTurned = red, Marks = marks, Guarding = guarding, GuardHit = guardHit, Struck = struck };

        [Test]
        public void APerfectGuardOpensTheMomentForItsLength()
        {
            var plain = new MarkSet(KatanaWay.Answer);
            var run = new AspectRun();
            Assert.IsFalse(AnswerWindow.Open(run));
            AnswerWindow.Tick(ref run, Step(plain, guard: true));
            Assert.AreEqual(0.6f, run.Window, 1e-5f);
            AnswerWindow.Tick(ref run, Step(plain, seconds: 0.61f));
            Assert.IsFalse(AnswerWindow.Open(run), "closed after 0.6 s");
            Assert.AreEqual(0.8f, AnswerWindow.LengthFor(new MarkSet(KatanaWay.OpenWindow), false), 1e-5f);
            Assert.AreEqual(1f, AnswerWindow.LengthFor(new MarkSet(KatanaWay.CatchTheRed), true), 1e-5f);
            Assert.AreEqual(0.6f, AnswerWindow.LengthFor(new MarkSet(KatanaWay.CatchTheRed), false), 1e-5f, "only after a red attack");
            Assert.AreEqual(0.6f, AnswerWindow.LengthFor(new MarkSet(), true), 1e-5f);
        }

        [Test]
        public void LightIsTheRiposteAndHeavyIsThroughTheGap()
        {
            var marks = new MarkSet(KatanaWay.Answer);
            var weapon = Answer(KatanaWay.Answer);
            var run = new AspectRun();
            AnswerWindow.Tick(ref run, Step(marks, guard: true));
            var riposte = AnswerWindow.Answer(ref run, weapon, false, marks);
            Assert.AreEqual(24f, riposte.Damage, 1e-5f);
            Assert.AreEqual(20f, riposte.Stagger, 1e-5f);
            Assert.IsTrue(riposte.Critical, "critical (36)");
            Assert.AreEqual(0.06f, riposte.Windup, 1e-5f);
            Assert.AreEqual(0.3f, riposte.Recovery, 1e-5f);
            Assert.IsFalse(AnswerWindow.Open(run), "spent");
            Assert.IsNull(AnswerWindow.Answer(ref run, weapon, false, marks), "nothing to answer");
            AnswerWindow.Tick(ref run, Step(marks, guard: true));
            var gap = AnswerWindow.Answer(ref run, weapon, true, marks);
            Assert.AreEqual(30f, gap.Damage, 1e-5f);
            Assert.AreEqual(40f, gap.Stagger, 1e-5f);
            Assert.IsTrue(gap.PassThrough);
            Assert.AreEqual(3f, gap.Travel, 1e-5f);
            Assert.IsFalse(gap.Critical);
            foreach (var step in weapon.AllSteps()) Assert.IsFalse(step.Critical, "the chain is not changed: " + step.Name);
        }

        [Test]
        public void SecondAnswerKeepsTheMomentForAnotherPerfectGuardAndAddsThirty()
        {
            var marks = new MarkSet(KatanaWay.Answer, KatanaWay.SecondAnswer);
            var weapon = Answer(KatanaWay.Answer, KatanaWay.SecondAnswer);
            var run = new AspectRun();
            AnswerWindow.Tick(ref run, Step(marks, guard: true));
            Assert.AreEqual(20f, AnswerWindow.Answer(ref run, weapon, false, marks).Stagger, 1e-5f);
            Assert.IsTrue(AnswerWindow.Open(run), "kept open");
            Assert.IsNull(AnswerWindow.Answer(ref run, weapon, false, marks), "a second answer needs a second perfect guard");
            AnswerWindow.Tick(ref run, Step(marks, guard: true));
            Assert.AreEqual(50f, AnswerWindow.Answer(ref run, weapon, false, marks).Stagger, 1e-5f, "+30 on the second");
            Assert.IsFalse(AnswerWindow.Open(run), "two at most");
        }

        [Test]
        public void PatientBladeAddsTenAHalfSecondOfGuardHeldUnstruckToForty()
        {
            Assert.AreEqual(0f, AnswerWindow.PatientBonus(0.49f));
            Assert.AreEqual(10f, AnswerWindow.PatientBonus(0.5f), 1e-5f);
            Assert.AreEqual(30f, AnswerWindow.PatientBonus(1.6f), 1e-5f);
            Assert.AreEqual(40f, AnswerWindow.PatientBonus(9f), 1e-5f);
            var marks = new MarkSet(KatanaWay.Answer, KatanaWay.PatientBlade);
            var run = new AspectRun();
            AnswerWindow.Tick(ref run, Step(marks, guarding: true, seconds: 1.2f));
            Assert.AreEqual(1.2f, run.SinceCount, 1e-5f);
            AnswerWindow.Tick(ref run, Step(marks, guarding: true, guardHit: true));
            Assert.AreEqual(0f, run.SinceCount, "struck on the guard: starts over");
            AnswerWindow.Tick(ref run, Step(marks, guarding: true, seconds: 1.1f));
            AnswerWindow.Tick(ref run, Step(marks, guard: true, guarding: true));
            var riposte = AnswerWindow.Answer(ref run, Answer(KatanaWay.Answer, KatanaWay.PatientBlade), false, marks);
            Assert.AreEqual(20f + 20f, riposte.Stagger, 1e-4f, "1.1 s held: +20");
            AnswerWindow.Tick(ref run, Step(new MarkSet(KatanaWay.Answer), guarding: true, seconds: 3f));
            Assert.AreEqual(0f, run.SinceCount, "without Patient Blade nothing is counted");
        }

        [Test]
        public void ReadTheRedWidensThePerfectWindowAgainstRedAttacksOnly()
        {
            var stamina = new StaminaPool();
            stamina.Fill();
            var guard = new Guard();
            guard.SetRaised(true);
            guard.Press();
            guard.Tick(guard.PerfectWindow + 0.02f);
            Assert.AreEqual(GuardResult.NotGuarded, guard.Resolve(10f, true, stamina, out _), "late, without Read the Red: the red blow lands");
            guard.RedWindowBonus = AnswerWindow.ReadTheRedSeconds;
            Assert.AreEqual(GuardResult.Perfect, guard.Resolve(10f, true, stamina, out _), "0.04 s wider against red");
            Assert.AreEqual(GuardResult.Blocked, guard.Resolve(10f, false, stamina, out _), "other blows as before");
            guard.Tick(0.03f);
            Assert.AreEqual(GuardResult.NotGuarded, guard.Resolve(10f, true, stamina, out _), "no wider than that");
        }

        [Test]
        public void KiriOtoshiAnswersByItself()
        {
            Assert.IsTrue(AnswerWindow.Answers(new MarkSet(KatanaWay.KiriOtoshi)));
            Assert.IsFalse(AnswerWindow.Answers(new MarkSet(KatanaWay.Answer)));
        }
    }
}

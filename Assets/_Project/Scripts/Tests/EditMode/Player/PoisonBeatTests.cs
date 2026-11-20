using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Trees;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>Poison's beat as marks may change it, the stats added with it, and the marks that now stand on them.</summary>
    public class PoisonBeatTests
    {
        private const AfflictionKind P = AfflictionKind.Poison;

        [SetUp]
        [TearDown]
        public void LeaveTheBeatAsItWas()
        {
            Afflictions.PoisonWaits = false;
            Afflictions.PoisonBeatOff = false;
            Afflictions.PoisonKeeps = false;
            Afflictions.PoisonRipens = 0f;
            Afflictions.PoisonOnCursed = 0f;
            Afflictions.SharedBeat = false;
            Afflictions.SharedBeatShare = 0f;
            Afflictions.BeatCount = 0;
            Afflictions.FreezeSeconds = 1f;
            Afflictions.ShatterDamage = 1f;
            // Two steps with the shared beat off put its clock and strength back.
            Afflictions.TickBeat(0f);
            Afflictions.TickBeat(0f);
        }

        private static float Run(AfflictionSet set, float seconds)
        {
            float dealt = 0f;
            for (float t = 0f; t < seconds - 0.0001f; t += 0.1f) dealt += set.Tick(0.1f);
            return dealt;
        }

        [Test]
        public void ABeatThatIsStilledNeverBites()
        {
            var set = new AfflictionSet();
            set.Apply(P, 4);
            Afflictions.PoisonBeatOff = true;
            Assert.AreEqual(0f, Run(set, 5f), 0.001f);
            Assert.AreEqual(4, set.Stacks(P), "Stilled poison should still be there to set off.");
        }

        [Test]
        public void PoisonThatKeepsDoesNotFade()
        {
            var spec = Afflictions.Spec(P);
            var set = new AfflictionSet();
            set.Apply(P, 3);
            Afflictions.PoisonKeeps = true;
            float dealt = Run(set, spec.Seconds * 3f);
            Assert.AreEqual(3, set.Stacks(P));
            Assert.Greater(dealt, spec.DamagePerTick * 3f * 5f, "It should have gone on biting all that time.");
        }

        [Test]
        public void PoisonLeftAloneRipensAndAFreshDoseStartsItOver()
        {
            var spec = Afflictions.Spec(P);
            var set = new AfflictionSet();
            Afflictions.PoisonKeeps = true;
            Afflictions.PoisonRipens = 0.25f;
            set.Apply(P, 2);
            float first = Run(set, spec.TickSeconds + 0.05f);
            float second = Run(set, spec.TickSeconds);
            float third = Run(set, spec.TickSeconds);
            Assert.AreEqual(spec.DamagePerTick * 2f, first, 0.01f);
            Assert.AreEqual(first * 1.25f, second, 0.01f);
            Assert.AreEqual(first * 1.5f, third, 0.01f);
            Assert.AreEqual(3, set.RipeBites);
            set.Apply(P, 1);
            Assert.AreEqual(0, set.RipeBites, "A fresh dose did not start the ripening over.");
            float after = Run(set, spec.TickSeconds);
            Assert.AreEqual(spec.DamagePerTick * 3f, after, 0.01f);
        }

        [Test]
        public void PoisonBitesHarderOnTheCursedAndStaysWhileTheCurseDoes()
        {
            var spec = Afflictions.Spec(P);
            Afflictions.PoisonOnCursed = 0.25f;
            var plain = new AfflictionSet();
            var cursed = new AfflictionSet();
            plain.Apply(P, 4);
            cursed.Apply(P, 4);
            cursed.Apply(AfflictionKind.Cursed, 1);
            float onPlain = Run(plain, spec.TickSeconds + 0.05f);
            float onCursed = Run(cursed, spec.TickSeconds + 0.05f);
            Assert.AreEqual(onPlain * 1.25f, onCursed, 0.01f);
            // Past the poison's own time but inside the curse's: it fades from the one and not the other.
            float more = spec.Seconds - spec.TickSeconds + 0.3f;
            Assert.Less(more + spec.TickSeconds, Afflictions.Spec(AfflictionKind.Cursed).Seconds, "The test needs the curse to outlast the poison.");
            Run(plain, more);
            Run(cursed, more);
            Assert.AreEqual(0, plain.Stacks(P));
            Assert.AreEqual(4, cursed.Stacks(P));
        }

        [Test]
        public void OnTheSharedBeatEverythingBitesTogetherAndCompanyMakesItStronger()
        {
            var spec = Afflictions.Spec(P);
            Afflictions.SharedBeat = true;
            Afflictions.SharedBeatShare = 0.15f;
            Afflictions.PoisonKeeps = true;
            var sets = new[] { new AfflictionSet(), new AfflictionSet(), new AfflictionSet() };
            sets[0].Apply(P, 1);
            Run(sets[0], 0.7f);
            sets[1].Apply(P, 1);
            Run(sets[1], 0.3f);
            sets[2].Apply(P, 1);
            int beats = 0;
            float firstBeat = 0f, secondBeat = 0f;
            for (int step = 0; step < 60 && beats < 2; step++)
            {
                Afflictions.TickBeat(0.1f);
                float dealt = 0f;
                int bit = 0;
                foreach (var set in sets)
                {
                    dealt += set.Tick(0.1f);
                    if (set.BitThisTick) bit++;
                }
                if (bit == 0) continue;
                Assert.AreEqual(3, bit, "They did not all bite on the one beat.");
                beats++;
                if (beats == 1) firstBeat = dealt; else secondBeat = dealt;
            }
            Assert.AreEqual(2, beats);
            Assert.AreEqual(spec.DamagePerTick * 3f, firstBeat, 0.01f);
            // Two beyond the first: three tenths stronger.
            Assert.AreEqual(firstBeat * 1.3f, secondBeat, 0.01f);
        }

        [Test]
        public void ABiteCanBeCalledOutOfTurnAndABlowCanBeTimedToTheBeat()
        {
            var spec = Afflictions.Spec(P);
            var set = new AfflictionSet();
            Assert.IsFalse(set.BiteSoon(), "Nothing to bite with.");
            set.Apply(P, 5);
            Assert.IsFalse(set.NearBeat(0.25f), "Fresh poison has not bitten and is not about to.");
            Assert.IsTrue(set.BiteSoon());
            float dealt = set.Tick(0.02f);
            Assert.AreEqual(spec.DamagePerTick * 5f, dealt, 0.01f);
            Assert.IsTrue(set.BitThisTick);
            Assert.AreEqual(5, set.BitDepth);
            Assert.IsTrue(set.NearBeat(0.25f), "Just bitten.");
            Run(set, 0.5f);
            Assert.IsFalse(set.NearBeat(0.25f));
            Run(set, spec.TickSeconds - 0.5f - 0.02f - 0.2f);
            Assert.IsTrue(set.NearBeat(0.25f), "About to bite.");
        }

        [Test]
        public void AFreezeCanLastLongerAndAShatterLandHarder()
        {
            var chill = Afflictions.Spec(AfflictionKind.Chill);
            var usual = new AfflictionSet();
            usual.Apply(AfflictionKind.Chill, Afflictions.Depth(AfflictionKind.Chill));
            Afflictions.FreezeSeconds = 1.3f;
            Afflictions.ShatterDamage = 1.25f;
            var longer = new AfflictionSet();
            longer.Apply(AfflictionKind.Chill, Afflictions.Depth(AfflictionKind.Chill));
            Assert.IsTrue(longer.IsFrozen);
            Assert.AreEqual(usual.FrozenLeft * 1.3f, longer.FrozenLeft, 0.001f);
            Assert.AreEqual(chill.ShatterDamage * 1.25f, longer.Shatter(), 0.001f);
        }

        [Test]
        public void ABoonForOneBlowCanBeEndedEarly()
        {
            var stats = new PlayerStats();
            stats.AddBoon(PlayerStat.WeaponDamage, 0.3f, 2f);
            Assert.AreEqual(1.3f, stats.Multiplier(PlayerStat.WeaponDamage), 0.001f);
            Assert.IsTrue(stats.EndBoon(PlayerStat.WeaponDamage, 0.3f));
            Assert.AreEqual(1f, stats.Multiplier(PlayerStat.WeaponDamage), 0.001f);
            Assert.IsFalse(stats.EndBoon(PlayerStat.WeaponDamage, 0.3f), "It was ended twice.");
            stats.Tick(3f);
            Assert.AreEqual(1f, stats.Multiplier(PlayerStat.WeaponDamage), 0.001f, "Its time running out took it away a second time.");
            // Flashpoint and Rushing Wake, the first boons for one blow, were retired with Fire's and Wind's reshaping (Phase 4);
            // Closing Circle's Half Circle carries one now: the first blow after the run is critical, and only that one.
            var halfCircle = Upgrade.ById(SpellBranch.ShapeId(WindTree.ClosingCircle)).Shapes[0];
            Assert.AreEqual("Half Circle", halfCircle.Name);
            Assert.IsTrue(halfCircle.Effects.All(e => e.Do == UpgradeAction.Boon && e.UntilBlow));
        }

        [Test]
        public void TheMarksOfTheBeatStandOnIt()
        {
            Assert.IsTrue(Upgrade.ById(PoisonTree.Catching).Effects.Any(e => e.On == UpgradeTrigger.PoisonBite && e.Do == UpgradeAction.Pass));
            Assert.IsTrue(Upgrade.ById(PoisonTree.LanceTheWound).Effects.Any(e => e.On == UpgradeTrigger.PerfectGuard && e.Do == UpgradeAction.Bite && e.Radius > 0f));
            Assert.IsTrue(Upgrade.ById(PoisonTree.InTime).Effects.All(e => e.OnBeat > 0f));
            Assert.IsTrue(Upgrade.ById(BlightTree.FeedingRot).Effects.Any(e => e.On == UpgradeTrigger.PoisonBite && e.Needs == AfflictionKind.Cursed && e.PerStack));
            Assert.IsTrue(Upgrade.ById(BlightTree.OneDeathFeeds).Effects.Any(e => e.On == UpgradeTrigger.Kill && e.Do == UpgradeAction.Bite));
            // Devouring Maw is a spell branch now (Phase 4): its feeding answers its own branch's bites.
            Assert.IsTrue(Upgrade.ById(DarkTree.DevouringMaw).Effects.All(e => e.OwnBranch));
            // Black Ice, the one mark that answered the base spell alone, is Hoarfrost's shape now (Phase 4): a sheet cast in
            // the crystals' place.
            Assert.IsTrue(RetiredMarks.IsRetired(IceTree.BlackIce));
            var blackIce = Upgrade.ById(SpellBranch.ShapeId(IceTree.Hoarfrost)).Shapes[0];
            Assert.AreEqual("Black Ice", blackIce.Name);
            Assert.AreEqual(SpellForms.Field, blackIce.Form);
        }

        [Test]
        public void EveryTriggerHasWordsAndEveryNewStatIsReadByAMark()
        {
            var probe = new Upgrade { Id = "test.words", Name = "Words", Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PoisonBite, Do = UpgradeAction.Heal, Amount = 1f } } };
            StringAssert.Contains("poison bites", TriggerWords.HowTo(probe));
            // Strong Arm, Following Draft and Held Fast, which gave PushDistance, BoonSeconds and FreezeSeconds, were retired with
            // Wind's and Ice's reshaping (Phase 4), and Braced Frame, which gave GuardGrey, with Metal's; those stats wait for
            // the Empowers and Bonds that will give them again.
            var named = new[] { PlayerStat.ShatterDamage, PlayerStat.BurstStagger, PlayerStat.GroundSeconds, PlayerStat.LeapReach };
            foreach (var stat in named)
                Assert.IsTrue(Upgrade.All.Any(mark => mark.Stats.Any(mod => mod.Stat == stat)), $"No mark gives {stat}.");
            var stats = new PlayerStats();
            stats.AddLearned(new StatMod(PlayerStat.LeapReach, 0.5f));
            Assert.AreEqual(1.5f, stats.Multiplier(PlayerStat.LeapReach), 0.001f);
            Assert.IsFalse(PlayerStats.IsFlat(PlayerStat.GuardGrey));
        }
    }
}

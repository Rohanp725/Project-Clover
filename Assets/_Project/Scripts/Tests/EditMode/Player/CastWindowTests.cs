using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Trees;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// A cast within a moment's window as a Master's condition (standin_clearance_plan.md pass 6, the ledger's spell-windows).
    /// </summary>
    public class CastWindowTests
    {
        [Test]
        public void ACastCountsOnlyInsideItsMomentsWindow()
        {
            var moments = new CastMoments();
            Assert.IsFalse(moments.Within(CastMoment.Finisher, 0.5f, 3f), "no finisher yet");
            moments.Note(CastMoment.Finisher, 10f);
            Assert.IsTrue(moments.Within(CastMoment.Finisher, 0.5f, 10.4f), "thrown within half a second after it");
            Assert.IsFalse(moments.Within(CastMoment.Finisher, 0.5f, 10.6f), "too late");
            Assert.IsFalse(moments.Within(CastMoment.Finisher, 0.5f, 9.9f), "a cast before the moment is not after it");
            Assert.IsTrue(moments.Within(CastMoment.None, 0f, 0f), "no moment asked: always");
            moments.NoteDeathCarrying(AfflictionKind.Seeded, 20f);
            Assert.IsTrue(moments.Within(CastMoment.Death, 0.5f, 20.3f, AfflictionKind.Seeded), "a Seeded enemy died");
            Assert.IsFalse(moments.Within(CastMoment.Death, 0.5f, 20.3f, AfflictionKind.Burn), "no burning one did");
            Assert.IsTrue(moments.Within(CastMoment.Death, 0.5f, 20.3f), "any death");
            moments.Clear();
            Assert.IsFalse(moments.Within(CastMoment.Death, 0.5f, 20.3f));
        }

        private static UpgradeEffect MasterAsking(string branch, CastMoment moment) =>
            Upgrade.ById(SpellBranch.MasterId(branch)).Effects.Single(each => each.CastAfter == moment);

        [Test]
        public void TheMastersAskForTheirMoments()
        {
            var lance = MasterAsking(LightningTree.ChainLance, CastMoment.Finisher);
            Assert.AreEqual(0.5f, lance.CastWindow, 1e-5f);
            Assert.AreEqual(UpgradeAction.Arc, lance.Do);
            Assert.AreEqual(12f, lance.Reach, 1e-5f, "every enemy within twelve metres");
            Assert.IsFalse(Upgrade.ById(SpellBranch.MasterId(LightningTree.ChainLance)).Effects.Any(each => each.On == UpgradeTrigger.FinisherHit), "a finisher alone sends nothing");

            var reap = MasterAsking(LifeTree.ReapWhatIsSown, CastMoment.Death);
            Assert.AreEqual(AfflictionKind.Seeded, reap.CastAfterCarrying);
            Assert.AreEqual(0.5f, reap.CastWindow, 1e-5f);
            Assert.IsTrue(reap.ThisCast && reap.Stat == PlayerStat.BurstDamage && reap.Amount == 1f, "its own bursts count twice");

            var pour = MasterAsking(MetalTree.PouredBronze, CastMoment.PerfectGuard);
            Assert.AreEqual(1f, pour.CastWindow, 1e-5f);
            Assert.IsTrue(pour.NeedsAttacker && pour.CountsNeeds && pour.PerStack, "the attacker's pour, a notch at a time");
            Assert.AreEqual(12f, pour.Damage, 1e-5f);

            var frost = MasterAsking(IceTree.Hoarfrost, CastMoment.Shatter);
            Assert.AreEqual(0.5f, frost.CastWindow, 1e-5f);
            Assert.AreEqual(2, frost.Stacks, "two more Chill: four deep");
        }
    }
}

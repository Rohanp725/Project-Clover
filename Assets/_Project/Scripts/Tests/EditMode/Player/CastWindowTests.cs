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

        [Test]
        public void AMarkGivesBackHalfItsOwnSpellsWaitAlone()
        {
            var clocks = new Clover.Player.Magic.SpellCooldowns();
            var slots = new Clover.Player.Magic.SpellSlots();
            slots.Set(0, "t.whirl");
            slots.Set(1, "t.other");
            clocks.Start(0, 10f);
            clocks.Start(1, 10f);
            var half = new UpgradeEffect { Do = UpgradeAction.KeepWaitShare, Amount = 0.5f };
            Assert.IsTrue(Clover.Player.Magic.BranchCooldowns.Handles(UpgradeAction.KeepWaitShare));
            Assert.AreEqual(0, Clover.Player.Magic.BranchCooldowns.Answer(half, "t.whirl", clocks, slots));
            Assert.AreEqual(5f, clocks.Remaining(0), 1e-4f, "its own wait halved");
            Assert.AreEqual(10f, clocks.Remaining(1), 1e-4f, "every other clock left alone");
            var whirl = Upgrade.ById(SpellBranch.MasterId(WaterTree.Whirlpool)).Effects.Single(each => each.Do == UpgradeAction.KeepWaitShare);
            Assert.AreEqual(0.3f, whirl.NeedsWindup, 1e-5f, "only in the instant its enemy's attack mark shows");
            var blown = Upgrade.ById(Clover.Player.Trees.Duo.BreathTakenDuo.BlownThrough);
            Assert.IsTrue(blown.Effects.Any(each => each.Do == UpgradeAction.KeepWaitShare && each.Amount == 0.5f), "half its own wait back");
            Assert.IsFalse(blown.Effects.Any(each => each.Do == UpgradeAction.CutCooldowns), "not seconds off every clock");
            Assert.AreEqual(blown.Id, Clover.Player.Magic.BranchCooldowns.BranchOf(blown.Effects.First(each => each.Do == UpgradeAction.KeepWaitShare)), "a duo spell's own slot");
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

        [Test]
        public void GraspAndGlareAnswerTheOneWhoseBlowItWas()
        {
            Assert.IsTrue(PlayerUpgrades.RecentAttacker(new object(), 10f, 1f, 10.8f), "struck 0.8 s ago");
            Assert.IsFalse(PlayerUpgrades.RecentAttacker(new object(), 10f, 1f, 11.2f), "too long ago");
            Assert.IsFalse(PlayerUpgrades.RecentAttacker(null, 10f, 1f, 10.1f), "no one");
            var grasp = Upgrade.ById(SpellBranch.MasterId(DarkTree.Grasp)).Effects;
            Assert.AreEqual(2, grasp.Length);
            Assert.IsTrue(grasp.All(each => each.On == UpgradeTrigger.SpellCast && each.CastAfter == CastMoment.Struck && each.CastWindow == 1f && each.ToStruckBy),
                "Grasp cast within a second of being struck answers the one that struck you");
            Assert.IsTrue(grasp.Any(each => each.Do == UpgradeAction.Pull) && grasp.Any(each => each.Do == UpgradeAction.Afflict && each.Stacks == 2));
            var glare = Upgrade.ById(SpellBranch.MasterId(LightTree.Glare)).Effects.Single();
            Assert.AreEqual(CastMoment.PerfectDodge, glare.CastAfter);
            Assert.AreEqual(0.3f, glare.CastWindow, 1e-5f, "in the perfect dodge's moment");
            Assert.IsTrue(glare.ToTurnedBy, "on the attacker it dodged");
            Assert.AreEqual(ConditionFlags.CannotTurn, glare.State.Flags, "a blinding that stops only its turning");
            Assert.AreEqual(2f, glare.State.Seconds, 1e-5f);
            Assert.AreEqual(1f, glare.State.EliteSeconds, 1e-5f, "one second for a god or an elite");
            var lift = Upgrade.ById(SpellBranch.MasterId(WindTree.LiftingWind)).Effects.Single();
            Assert.IsTrue(lift.NeedsAttacker && lift.CastAfter == CastMoment.PerfectDodge, "only on the enemy the dodge took you past");
            Assert.AreEqual(2f, lift.Seconds, 1e-5f, "twice its second in the air");
        }

        [Test]
        public void ThunderclapsInstantCastHasNoWindUp()
        {
            var mana = new ManaPool { Maximum = 100f };
            mana.Gain(100f);
            var kit = new ElementKit { SpellCost = 0f, SpellWindup = 0.3f, SpellRecovery = 0.2f };
            Assert.IsTrue(kit.TryCast(mana, 1f, true));
            Assert.IsTrue(kit.TickCast(0f), "loosed on the first tick, with no time passed");
            Assert.IsTrue(kit.IsCasting, "the recovery after it is kept");
            var plain = new ElementKit { SpellCost = 0f, SpellWindup = 0.3f, SpellRecovery = 0.2f };
            Assert.IsTrue(plain.TryCast(mana, 1f, false));
            Assert.IsFalse(plain.TickCast(0.1f), "a plain cast still winds up");
            var master = Upgrade.ById(SpellBranch.MasterId(LightningTree.Thunderclap)).Effects;
            Assert.IsTrue(master.Any(each => each.On == UpgradeTrigger.PerfectGuard && each.Do == UpgradeAction.InstantNext));
            var held = Upgrade.ById(SpellBranch.ShapeId(LightningTree.Thunderclap)).Shapes.Single(each => each.Name == "Held Thunder").Spell;
            Assert.AreEqual(1f, held.HoldUpTo, 1e-5f, "held up to a second");
            var spec = new SpellSpec();
            SpellChange.Copy(held, spec);
            SpellSpec.ApplyHold(spec, 1f);
            Assert.AreEqual(90f, spec.Stagger, 1e-4f, "90 stagger at the full hold");
            SpellChange.Copy(held, spec);
            SpellSpec.ApplyHold(spec, 0f);
            Assert.AreEqual(45f, spec.Stagger, 1e-4f, "a tap is Thunderclap's own clap");
        }

        [Test]
        public void WaitingThundersSecondRoundComesOnlyAfterTheLastBoltFell()
        {
            var times = new[] { -1f, 0.2f, -2f };
            Assert.IsFalse(Clover.Player.Forms.PhalanxForm.LastSent(times, 3), "one still hovers");
            times[0] = 0f;
            Assert.IsTrue(Clover.Player.Forms.PhalanxForm.LastSent(times, 3), "none left at the shoulder");
            var moments = new CastMoments();
            moments.Note(CastMoment.LastBolt, 5f);
            Assert.IsTrue(moments.Within(CastMoment.LastBolt, 1f, 5.8f));
            Assert.IsFalse(moments.Within(CastMoment.LastBolt, 1f, 6.2f));
            var round = Upgrade.ById(SpellBranch.MasterId(LightningTree.WaitingThunder)).Effects.Single();
            Assert.AreEqual(UpgradeTrigger.HeavyHit, round.On);
            Assert.AreEqual(CastMoment.LastBolt, round.After);
            Assert.AreEqual(1f, round.AfterWindow, 1e-5f);
            Assert.AreEqual(UpgradeAction.Form, round.Do);
            Assert.AreEqual(SpellForms.Waiting, round.Form, "the five hang again for a second round");
            Assert.AreEqual(5, round.Spell.Count);
        }

        [Test]
        public void SevenFlashesCastInADodgesMomentStandsNotStill()
        {
            var flashes = Upgrade.ById(LightningTree.SevenFlashes);
            var master = Upgrade.ById(SpellBranch.MasterId(LightningTree.SevenFlashes));
            var quick = master.Effects.Single();
            Assert.AreEqual(UpgradeAction.Quicken, quick.Do);
            var inForce = new System.Collections.Generic.List<UpgradeEffect> { quick };
            var moments = new CastMoments();
            Assert.IsFalse(PlayerUpgrades.Quickens(inForce, flashes, moments, 3f), "no dodge yet");
            moments.Note(CastMoment.PerfectDodge, 10f);
            Assert.IsTrue(PlayerUpgrades.Quickens(inForce, flashes, moments, 10.25f), "within a third of a second");
            Assert.IsFalse(PlayerUpgrades.Quickens(inForce, flashes, moments, 10.4f), "too late");
            Assert.IsFalse(PlayerUpgrades.Quickens(inForce, Upgrade.ById(LightningTree.ChainLance), moments, 10.25f), "another branch's cast");
            Assert.AreEqual(0f, Clover.Player.Forms.FlashesForm.StillFor(0f), "quickened: no stillness");
            Assert.AreEqual(0.5f, Clover.Player.Forms.FlashesForm.StillFor(0.5f), 1e-5f);
            Assert.AreEqual(0.15f, Clover.Player.Forms.FlashesForm.StillFor(0.1f), 1e-5f);
        }
    }
}

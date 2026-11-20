using System.Collections.Generic;
using Clover.Player;
using Clover.Player.Build;
using Clover.Player.Magic;
using Clover.Player.Trees;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// What a spell branch makes of its spell (IMPLEMENTATION.md 2.2, 3.1; task 4.1), on Fire's and Lightning's own
    /// branches: the Empower's shorter wait and lower price, the shape worn by the loadout's choice, a shape's whole spec
    /// keeping the Learn's wait, a base spell cast through its shape, a Master answering only its own branch's spell, and
    /// the three actions on the spell clocks.
    /// </summary>
    public class SpellBranchTests
    {
        private static Loadout Fire(string slotted = "", int shape = -1)
        {
            var loadout = new Loadout();
            loadout.Attune(Elements.Fire, "");
            if (slotted != "") loadout.Slots[0] = slotted;
            if (shape >= 0) loadout.Shapes.Add(new IdChoice(slotted, shape));
            return loadout;
        }

        [Test]
        public void ASpellChangeSetsOnlyWhatItNamesAndCopyTakesEveryNumber()
        {
            var spec = new SpellSpec { Cooldown = 8f, Cost = 0f, Reach = 7f, Count = 1, Seconds = 2.6f };
            Assert.IsTrue(new SpellChange().IsEmpty);
            new SpellChange { Cooldown = 6f, Seconds = 4f }.ApplyTo(spec);
            Assert.AreEqual(6f, spec.Cooldown);
            Assert.AreEqual(4f, spec.Seconds);
            Assert.AreEqual(7f, spec.Reach, "a number it leaves at zero is left alone");
            Assert.AreEqual(1, spec.Count);
            var copy = new SpellSpec();
            SpellChange.Copy(spec, copy);
            Assert.AreEqual(spec.Cooldown, copy.Cooldown);
            Assert.AreEqual(spec.Reach, copy.Reach);
            Assert.AreEqual(spec.Count, copy.Count);
            Assert.AreEqual(spec.Height, copy.Height);
        }

        [Test]
        public void TheEmpowerShortensTheWaitAndLowersThePriceOnlyOnceBought()
        {
            var branches = new SpellBranches();
            var rush = Upgrade.ById(FireTree.WildfireRush);
            var learned = new HashSet<string> { FireTree.WildfireRush };
            Assert.AreEqual(8f, branches.CooldownOf(rush, learned, Fire(FireTree.WildfireRush)), 1e-4f, "the table's wait");
            learned.Add(SpellBranch.EmpowerId(FireTree.WildfireRush));
            Assert.AreEqual(6f, branches.CooldownOf(rush, learned, Fire(FireTree.WildfireRush)), 1e-4f, "the Empower's wait");
            Assert.AreEqual(4f, branches.SpecOf(rush, learned, Fire(FireTree.WildfireRush)).Seconds, 1e-4f, "its line burns four seconds");
            Assert.AreEqual(8f, rush.Spell.Cooldown, 1e-4f, "the mark's own numbers are never changed");

            var meteor = Upgrade.ById(Upgrade.MeteorLeap);
            var bought = new HashSet<string> { Upgrade.MeteorLeap };
            Assert.AreEqual(45f, branches.SpecOf(meteor, bought, Fire(Upgrade.MeteorLeap)).Cost, 1e-4f);
            bought.Add(SpellBranch.EmpowerId(Upgrade.MeteorLeap));
            Assert.AreEqual(35f, branches.SpecOf(meteor, bought, Fire(Upgrade.MeteorLeap)).Cost, 1e-4f);
        }

        [Test]
        public void TheShapeWornIsTheLoadoutsChoiceOnceTheShapeIsBought()
        {
            string id = Upgrade.MeteorLeap;
            var shapeMark = Upgrade.ById(SpellBranch.ShapeId(id));
            var learned = new HashSet<string> { id };
            Assert.IsNull(SpellBranches.WornShape(id, learned, Fire(id, 1)), "before the Shape is bought the base spell is cast");
            Assert.AreEqual(SpellForms.Leap, SpellBranches.FormOf(Upgrade.ById(id), learned, Fire(id, 1)));
            learned.Add(SpellBranch.ShapeId(id));
            Assert.AreSame(shapeMark.Shapes[0], SpellBranches.WornShape(id, learned, Fire(id)), "the first unless the second is chosen");
            Assert.AreSame(shapeMark.Shapes[1], SpellBranches.WornShape(id, learned, Fire(id, 1)));
            Assert.AreSame(shapeMark.Shapes[0], SpellBranches.Worn(shapeMark, Fire(id, 7)), "a choice out of range is the first");
            // Falling Star casts its own form and numbers, and keeps the Learn's price.
            var branches = new SpellBranches();
            Assert.AreEqual(SpellForms.Bomb, SpellBranches.FormOf(Upgrade.ById(id), learned, Fire(id, 0)));
            var spec = branches.SpecOf(Upgrade.ById(id), learned, Fire(id, 0));
            Assert.AreEqual(60f, spec.Stagger, 1e-4f);
            Assert.AreEqual(45f, spec.Cost, 1e-4f, "a shape's whole spec keeps the Learn's price");
            Assert.AreEqual(0f, spec.Cooldown, 1e-4f, "and its wait (a larger spell has none)");
            // Fire Front changes the drive's numbers and keeps its form.
            var rush = Upgrade.ById(FireTree.WildfireRush);
            var rushLearned = new HashSet<string> { FireTree.WildfireRush, SpellBranch.ShapeId(FireTree.WildfireRush) };
            Assert.AreEqual(SpellForms.Drill, SpellBranches.FormOf(rush, rushLearned, Fire(FireTree.WildfireRush, 0)));
            Assert.AreEqual(5f, branches.SpecOf(rush, rushLearned, Fire(FireTree.WildfireRush, 0)).Reach, 1e-4f);
            Assert.AreEqual(8f, branches.CooldownOf(rush, rushLearned, Fire(FireTree.WildfireRush, 0)), 1e-4f);
        }

        [Test]
        public void AChoiceOfShapeIsWrittenOnceAndCopiesShareNothing()
        {
            var shapes = new List<IdChoice>();
            SpellBranches.Choose(shapes, FireTree.WildfireRush, 1);
            SpellBranches.Choose(shapes, FireTree.WildfireRush, 0);
            SpellBranches.Choose(shapes, FireTree.KilnStance, 5);
            Assert.AreEqual(2, shapes.Count, "a spell has one choice");
            var loadout = new Loadout();
            SpellBranches.CopyChoices(shapes, loadout.Shapes);
            Assert.AreEqual(0, loadout.ShapeOf(FireTree.WildfireRush));
            Assert.AreEqual(0, loadout.ShapeOf(FireTree.KilnStance), "a choice out of range is the first");
            shapes.Clear();
            Assert.AreEqual(2, loadout.Shapes.Count, "the copy shares nothing");
        }

        [Test]
        public void ABaseSpellIsCastByItsKitUntilItWearsAShapeOfItsOwn()
        {
            var wave = Upgrade.ById(FireTree.FlameWave);
            Assert.IsTrue(wave.IsBaseSpell);
            var learned = new HashSet<string>();
            var branches = new SpellBranches();
            Assert.IsFalse(SpellBranches.CastsThroughMark(wave, learned, Fire(FireTree.FlameWave)));
            Assert.IsNull(branches.SpecOf(wave, learned, Fire(FireTree.FlameWave)), "the kit casts it with its own numbers");
            learned.Add(SpellBranch.ShapeId(FireTree.FlameWave));
            Assert.IsTrue(SpellBranches.CastsThroughMark(wave, learned, Fire(FireTree.FlameWave, 0)), "Cinder Fan is cast through the mark");
            Assert.AreEqual(SpellForms.Wave, SpellBranches.FormOf(wave, learned, Fire(FireTree.FlameWave, 0)));
            var fan = branches.SpecOf(wave, learned, Fire(FireTree.FlameWave, 0));
            Assert.AreEqual(150f, fan.Arc, 1e-4f);
            Assert.AreEqual(7f, fan.Cooldown, 1e-4f, "with the base spell's wait");
            Assert.AreEqual(0f, fan.Cost, 1e-4f, "and no price");
            // The Empower's wait for a base spell its kit casts, keeping the marks' share as a ratio.
            Assert.AreEqual(7f, SpellBranches.BaseCooldown(FireTree.FlameWave, 7f, learned), 1e-4f);
            learned.Add(SpellBranch.EmpowerId(FireTree.FlameWave));
            Assert.AreEqual(5.5f, SpellBranches.BaseCooldown(FireTree.FlameWave, 7f, learned), 1e-4f);
            Assert.AreEqual(5.5f * 0.8f, SpellBranches.BaseCooldown(FireTree.FlameWave, 7f * 0.8f, learned), 1e-4f);
        }

        [Test]
        public void AMasterAnswersOnlyItsOwnBranchsSpell()
        {
            var master = Upgrade.ById(SpellBranch.MasterId(Upgrade.MeteorLeap));
            Assert.IsTrue(SpellBranches.OwnBranch(master, Upgrade.ById(Upgrade.MeteorLeap)));
            Assert.IsFalse(SpellBranches.OwnBranch(master, Upgrade.ById(FireTree.WildfireRush)));
            Assert.IsFalse(SpellBranches.OwnBranch(master, null), "the kit's base spell is not Meteor Leap");
            Assert.IsFalse(SpellBranches.OwnBranch(Upgrade.ById(Upgrade.Heat), Upgrade.ById(Upgrade.MeteorLeap)), "a passive is on no branch");
            // A base spell's branch answers the kit's cast and its own Learn cast through a shape.
            var waveShape = Upgrade.ById(SpellBranch.ShapeId(FireTree.FlameWave));
            Assert.IsTrue(SpellBranches.OwnBranch(waveShape, null));
            Assert.IsTrue(SpellBranches.OwnBranch(waveShape, Upgrade.ById(FireTree.FlameWave)));
            Assert.AreSame(Upgrade.ById(Upgrade.MeteorLeap), SpellBranches.LearnOf(master));
            Assert.IsNull(SpellBranches.LearnOf(Upgrade.ById(Upgrade.Heat)));
        }

        [Test]
        public void TheClockActionsCutEveryClockAndResetOrPromiseOnlyTheirBranchsSlot()
        {
            var clocks = new SpellCooldowns();
            var slots = new SpellSlots();
            slots.Set(1, "t.lance");
            clocks.Start(0, 10f);
            clocks.Start(1, 10f);

            Assert.AreEqual(-1, BranchCooldowns.Answer(new UpgradeEffect { Do = UpgradeAction.CutCooldowns, Amount = 3f }, "t.lance", clocks, slots));
            Assert.AreEqual(7f, clocks.Remaining(0), 1e-4f);
            Assert.AreEqual(7f, clocks.Remaining(1), 1e-4f);

            Assert.AreEqual(1, BranchCooldowns.Answer(new UpgradeEffect { Do = UpgradeAction.ResetCooldown }, "t.lance", clocks, slots));
            Assert.IsTrue(clocks.Ready(1));
            Assert.AreEqual(7f, clocks.Remaining(0), 1e-4f, "another slot's clock is left alone");
            Assert.AreEqual(-1, BranchCooldowns.Answer(new UpgradeEffect { Do = UpgradeAction.ResetCooldown }, "t.unslotted", clocks, slots), "a spell in no slot is left alone");

            Assert.AreEqual(1, BranchCooldowns.Answer(new UpgradeEffect { Do = UpgradeAction.InstantNext }, "t.lance", clocks, slots));
            clocks.Start(1, 10f);
            Assert.IsTrue(clocks.Ready(1), "the promised cast starts no clock");
            clocks.Start(1, 10f);
            Assert.IsFalse(clocks.Ready(1), "and only that one");
            // A named branch wins over the effect's own.
            Assert.AreEqual("t.named", BranchCooldowns.BranchOf(new UpgradeEffect { Branch = "t.named" }));
            Assert.IsTrue(BranchCooldowns.Handles(UpgradeAction.InstantNext));
            Assert.IsFalse(BranchCooldowns.Handles(UpgradeAction.Burst));
        }

        [Test]
        public void APromiseWithAWindowLapsesAndLeavesARunningClockRunning()
        {
            var clocks = new SpellCooldowns();
            clocks.Start(2, 7f);
            clocks.InstantNext(2, 0.5f);
            Assert.IsFalse(clocks.Ready(2), "a window does not end the clock: the spell had to be ready");
            Assert.IsTrue(clocks.IsInstant(2));
            clocks.Tick(0.6f);
            Assert.IsFalse(clocks.IsInstant(2), "the window has passed");
            clocks.ResetAll();
            clocks.InstantNext(2, 0.5f);
            clocks.Tick(0.2f);
            clocks.Start(2, 7f);
            Assert.IsTrue(clocks.Ready(2), "cast within the window, its cooldown does not start");
            Assert.IsFalse(clocks.IsInstant(2), "and the promise is spent");
            // Without a window the promise keeps until the cast and ends the clock at once, as before.
            clocks.Start(3, 9f);
            clocks.InstantNext(3);
            Assert.IsTrue(clocks.Ready(3));
            clocks.Tick(30f);
            Assert.IsTrue(clocks.IsInstant(3));
        }

        [Test]
        public void TheFireMastersRideTheTriggersTheirTimingsName()
        {
            // Flame Wave's Master: the third charged blow promises the next wave no cooldown, for a moment.
            var wave = Upgrade.ById(SpellBranch.MasterId(FireTree.FlameWave));
            Assert.AreEqual(UpgradeTrigger.ChargedBlowThird, wave.Effects[0].On);
            Assert.AreEqual(UpgradeAction.InstantNext, wave.Effects[0].Do);
            Assert.Greater(wave.Effects[0].Seconds, 0f);
            Assert.AreEqual(FireTree.FlameWave, BranchCooldowns.BranchOf(wave.Effects[0]));
            // Thunderclap's: a perfect guard ends its wait and makes the next one instant.
            var clap = Upgrade.ById(SpellBranch.MasterId(LightningTree.Thunderclap));
            CollectionAssert.AreEquivalent(new[] { UpgradeAction.ResetCooldown, UpgradeAction.InstantNext }, new[] { clap.Effects[0].Do, clap.Effects[1].Do });
            Assert.IsTrue(System.Array.TrueForAll(clap.Effects, effect => effect.On == UpgradeTrigger.PerfectGuard));
            // Meteor Leap's: on an enemy at five Burn, its burn goes off with the blow.
            var meteor = Upgrade.ById(SpellBranch.MasterId(Upgrade.MeteorLeap));
            Assert.IsTrue(System.Array.TrueForAll(meteor.Effects, effect => effect.OwnBranch && effect.NeedsFull));
        }
    }
}

using System.Linq;
using Clover.Player;
using Clover.Player.Magic;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>Infused strikes (STEEL_AND_ELEMENT.md 3.4; IMPLEMENTATION.md 3.4): their clocks, the deep strike's price, the carriers and their footprints, the chord's cancel, and Hunger's wait.</summary>
    public class InfusedStrikeTests
    {
        [Test]
        public void EachStrikeButtonHasItsOwnEightSecondClock()
        {
            var rules = new StrikeRules();
            var clocks = new SpellCooldowns();
            Assert.AreEqual(StrikeRefusal.None, rules.Check(clocks, false, false, 0f, true));
            rules.Begin(clocks, false);
            Assert.AreEqual(8f, clocks.Length(SpellCooldowns.StrikeFirst), 1e-5f);
            Assert.AreEqual(StrikeRefusal.NotReady, rules.Check(clocks, false, false, 100f, true), "R1 waits");
            Assert.AreEqual(StrikeRefusal.None, rules.Check(clocks, true, false, 100f, true), "R2 has its own clock");
            Assert.IsTrue(clocks.Ready(0) && clocks.Ready(1) && clocks.Ready(2) && clocks.Ready(3), "a strike starts no spell's clock");
            clocks.Tick(7.9f);
            Assert.IsFalse(clocks.Ready(SpellCooldowns.StrikeFirst));
            clocks.Tick(0.2f);
            Assert.IsTrue(clocks.Ready(SpellCooldowns.StrikeFirst));
        }

        [Test]
        public void WeaponPlayCutsAStrikesClockButItsOwnBlowDoesNot()
        {
            var rules = new StrikeRules();
            var clocks = new SpellCooldowns();
            clocks.Start(0, 8f);
            rules.Begin(clocks, false);
            rules.Begin(clocks, true);
            // The strike's own blow lands: every clock but its own is cut.
            clocks.Cut(0.3f, StrikeRules.Clock(false));
            Assert.AreEqual(8f, clocks.Remaining(SpellCooldowns.StrikeFirst), 1e-5f, "not its own");
            Assert.AreEqual(7.7f, clocks.Remaining(SpellCooldowns.StrikeSecond), 1e-5f, "the other strike's");
            Assert.AreEqual(7.7f, clocks.Remaining(0), 1e-5f, "a spell's");
            // A perfect guard cuts them all.
            clocks.Cut(2f);
            Assert.AreEqual(6f, clocks.Remaining(SpellCooldowns.StrikeFirst), 1e-5f);
        }

        [Test]
        public void TheDeepStrikeCostsTwentyManaAndTheStrikeNothing()
        {
            var rules = new StrikeRules();
            var clocks = new SpellCooldowns();
            Assert.AreEqual(0f, rules.Price(false));
            Assert.AreEqual(20f, rules.Price(true));
            Assert.AreEqual(StrikeRefusal.NoMana, rules.Check(clocks, true, true, 19.9f, true));
            Assert.AreEqual(StrikeRefusal.None, rules.Check(clocks, true, true, 20f, true));
            Assert.AreEqual(StrikeRefusal.None, rules.Check(clocks, false, false, 0f, true), "the plain strike asks no mana");
            Assert.AreEqual(StrikeRefusal.NoElement, rules.Check(clocks, false, false, 100f, false), "nothing to infuse without an element");
            // A faster clock (the SpellCooldown stat) shortens the wait.
            rules.Begin(clocks, true, 1.25f);
            Assert.AreEqual(6.4f, clocks.Length(SpellCooldowns.StrikeSecond), 1e-4f);
        }

        [Test]
        public void EveryWeaponHasACarrierOfItsOwnThatHasAClip()
        {
            foreach (var weapon in Weapons.All)
            {
                var carrier = weapon.CarrierStep;
                Assert.IsNotNull(carrier, weapon.Name);
                Assert.IsNotNull(weapon.Carrier, weapon.Name + " names its carrier");
                var others = weapon.Combo.Steps.Concat(new[] { weapon.Combo.Heavy, weapon.Combo.ChainHeavy, weapon.Combo.Skill, weapon.Combo.Finisher }).Where(step => step != null).ToList();
                Assert.IsFalse(others.Contains(weapon.Carrier), weapon.Name + ": a copy, so the strike never changes the chain");
                Assert.IsTrue(others.Any(step => step.Name == carrier.Name), weapon.Name + ": plays a clip the weapon has (" + carrier.Name + ")");
            }
        }

        [Test]
        public void TheCarriersCoverTheShapesTheDesignNames()
        {
            Assert.AreEqual(FootprintShape.Arc, Shape(Weapons.Katana()), "the Katana's drawing cut");
            Assert.AreEqual(170f, Weapons.Katana().Carrier.ArcDegrees, 1e-4f);
            Assert.AreEqual(22f, Weapons.Katana().Carrier.Damage, 1e-4f);
            Assert.AreEqual(FootprintShape.Arc, Shape(Weapons.MassiveSword()), "the rising cut");
            Assert.AreEqual(FootprintShape.Ring, Shape(Weapons.Scythe()), "the full turn");
            Assert.AreEqual(FootprintShape.Lane, Shape(Weapons.GreatSpear()), "the first thrust");
            Assert.AreEqual(FootprintShape.Disc, Shape(Weapons.GreatHammer()), "the drop");
            Assert.AreEqual(FootprintShape.Path, Shape(Weapons.TwinKnives()), "the crossing");
            Assert.AreEqual(FootprintShape.Path, Shape(Weapons.GreatBow()), "the shot");
            Assert.AreEqual(FootprintShape.Lane, Shape(Weapons.ChainSickle()), "the weight's snap");
            Assert.AreEqual(FootprintShape.Arc, Shape(Weapons.Fists()), "the kick");
            var stormKatana = Weapons.StormKatana();
            Assert.AreEqual(22f * 1.15f, stormKatana.Carrier.Damage, 1e-3f, "a god's weapon scales its carrier once, as every other move");
        }

        [Test]
        public void AFootprintKnowsWhereItLies()
        {
            var lane = Footprint.Of(new AttackStep { Reach = 4f, LineWidth = 0.9f }, Vector3.zero, Vector3.forward);
            Assert.AreEqual(new Vector3(0f, 0f, 2f), lane.Centre);
            Assert.AreEqual(new Vector3(0f, 0f, 4f), lane.Far);
            var disc = Footprint.Of(new AttackStep { Reach = 2.5f, Radius = 1.5f }, Vector3.zero, Vector3.right);
            Assert.AreEqual(new Vector3(2.5f, 0f, 0f), disc.Centre);
            Assert.AreEqual(new Vector3(4f, 0f, 0f), disc.Far);
            var path = Footprint.Of(new AttackStep { Reach = 2f, Travel = 5f }, Vector3.zero, Vector3.forward);
            Assert.AreEqual(FootprintShape.Path, path.Shape);
            Assert.AreEqual(7f, path.Reach, 1e-5f, "the lunge's ground and its reach at the end");
        }

        [Test]
        public void LightningsFlashCutIsALungeOfFiveMetresAtOneAndAHalfTimesTheBlow()
        {
            var step = Weapons.Katana().Carrier.Clone();
            float damage = step.Damage;
            ElementSteel.Bend(step, Elements.Lightning, false);
            Assert.AreEqual(5f, step.Travel, 1e-5f);
            Assert.IsTrue(step.PassThrough);
            Assert.AreEqual(damage * 1.5f, step.Damage, 1e-4f);
            var fire = Weapons.Katana().Carrier.Clone();
            ElementSteel.Bend(fire, Elements.Fire, false);
            Assert.AreEqual(0f, fire.Travel, "Fire keeps the carrier's shape");
        }

        [Test]
        public void LightningsFlashCutIsUntouchableThroughTheLungeAndNoOtherCarrierIs()
        {
            var step = Weapons.Katana().Carrier.Clone();
            ElementSteel.Bend(step, Elements.Lightning, false);
            Assert.AreEqual(step.Windup + step.Active, ElementSteel.UntouchableSeconds(step, Elements.Lightning), 1e-5f, "the wind-up and the 5 m it covers");
            Assert.GreaterOrEqual(ElementSteel.UntouchableSeconds(step, Elements.Lightning), 0.16f, "at least the lunge itself");
            var fire = Weapons.Katana().Carrier.Clone();
            ElementSteel.Bend(fire, Elements.Fire, false);
            Assert.AreEqual(0f, ElementSteel.UntouchableSeconds(fire, Elements.Fire), "Fire's carrier is open as any blow");
        }

        [Test]
        public void TheChordCancelsALightInItsFirstMomentAndNothingLater()
        {
            var stamina = new StaminaPool();
            stamina.Fill();
            var weapon = Weapons.Katana();
            var combo = weapon.Combo;
            var technique = weapon.Combo.Heavy.Clone();
            Assert.IsFalse(combo.Replace(technique, stamina), "nothing to cancel at rest");
            combo.Press();
            combo.Tick(0.016f, stamina);
            Assert.IsTrue(combo.LightWindingUp, "the light starts at once, no wait for a chord");
            Assert.IsTrue(combo.Replace(technique, stamina));
            Assert.AreSame(technique, combo.Current, "cancelled into the technique");
            Assert.IsFalse(combo.LightWindingUp);

            var chord = new ChordRules();
            chord.Light();
            chord.Tick(0.05f);
            Assert.IsTrue(chord.Heavy(0.05f), "heavy 0.05 s after the light, in its first 0.08 s");
            chord.Light();
            chord.Tick(0.12f);
            Assert.IsFalse(chord.Heavy(0.12f), "too late: the light is its own blow now");
        }

        [Test]
        public void HungerWaitsTwentyFiveSecondsFromWhenItEnds()
        {
            var kit = ElementKit.ByName(Elements.Dark, false);
            var mana = new ManaPool();
            Assert.AreEqual(25f, kit.SurgeCooldown, 1e-5f);
            Assert.IsTrue(kit.TrySurge(mana));
            Assert.IsFalse(kit.SurgeWaiting, "no wait while it burns");
            Assert.IsTrue(kit.TickSurge(kit.SurgeSeconds + 0.01f), "it ends");
            Assert.IsTrue(kit.SurgeWaiting);
            Assert.AreEqual(1f, kit.SurgeWaitFraction, 1e-4f);
            Assert.IsFalse(kit.TrySurge(mana), "not lit again while it waits");
            kit.TickSurge(24f);
            Assert.IsFalse(kit.TrySurge(mana));
            kit.TickSurge(1.1f);
            Assert.IsTrue(kit.TrySurge(mana), "lit again after 25 s");
            kit.TickSurge(kit.SurgeSeconds + 0.01f);
            kit.EndSurgeWait();
            Assert.IsTrue(kit.TrySurge(mana), "a rest ends the wait");
            var fire = ElementKit.ByName(Elements.Fire, false);
            Assert.AreEqual(0f, fire.SurgeCooldown, "a surge paid in mana needs no wait");
        }

        [Test]
        public void TheDiamondNamesASpellByItsInitials()
        {
            Assert.AreEqual("FW", Clover.UI.SpellDiamondHud.Initials("Flame Wave"));
            Assert.AreEqual("SB", Clover.UI.SpellDiamondHud.Initials("The Sun on the Blade"));
            Assert.AreEqual("GI", Clover.UI.SpellDiamondHud.Initials("Grown Into It"));
            Assert.AreEqual("L", Clover.UI.SpellDiamondHud.Initials("Lash"));
            Assert.AreEqual("", Clover.UI.SpellDiamondHud.Initials(""));
        }

        private static FootprintShape Shape(Weapon weapon) => Footprint.Of(weapon.Carrier, Vector3.zero, Vector3.forward).Shape;
    }
}

using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Magic;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>The charged blade (STEEL_AND_ELEMENT.md 3.3; IMPLEMENTATION.md 3.3) and the second affliction a blow carries.</summary>
    public class ChargedBladeTests
    {
        [Test]
        public void ALandedSpellGivesThreeBlowsEachTwentyPercentHarderAndTheThirdIsSaid()
        {
            var blade = new ChargedBlade();
            Assert.IsFalse(blade.Charged);
            blade.Charge(Elements.Fire);
            Assert.IsTrue(blade.Charged);
            Assert.AreEqual(3, blade.Left);
            for (int i = 1; i <= 3; i++)
            {
                var blow = blade.OnBlowLanded();
                Assert.IsTrue(blow.Carried, $"blow {i} carries the element");
                Assert.AreEqual(Elements.Fire, blow.Element);
                Assert.AreEqual(1.2f, blow.DamageShare, 1e-5f);
                Assert.AreEqual(i == 3, blow.Third, $"only blow 3 is the third (blow {i})");
                Assert.AreEqual(i, blow.Number);
            }
            Assert.IsFalse(blade.Charged, "spent after three");
            Assert.AreEqual(0, blade.Left);
            Assert.AreEqual("", blade.Element);
            Assert.IsFalse(blade.OnBlowLanded().Carried, "a fourth blow is plain");
            Assert.AreEqual(3, blade.Spent);
        }

        [Test]
        public void AFreshChargeReplacesTheOldOneInFull()
        {
            var blade = new ChargedBlade();
            blade.Charge(Elements.Fire);
            blade.OnBlowLanded();
            blade.OnBlowLanded();
            Assert.AreEqual(1, blade.Left);
            blade.Charge(Elements.Ice);
            Assert.AreEqual(Elements.Ice, blade.Element, "the new element takes the blade");
            Assert.AreEqual(3, blade.Left, "three blows again, not one more");
            Assert.AreEqual(2, blade.Charges);
        }

        [Test]
        public void NothingChargesWithoutAnElementAndAClearedBladeIsPlain()
        {
            var blade = new ChargedBlade();
            blade.Charge("");
            blade.Charge(null);
            Assert.IsFalse(blade.Charged);
            int version = blade.Version;
            blade.Charge(Elements.Lightning);
            Assert.Greater(blade.Version, version, "a charge is a change the sign must redraw");
            blade.Clear();
            Assert.IsFalse(blade.Charged);
            Assert.IsFalse(blade.OnBlowLanded().Carried);
        }

        [Test]
        public void TheEdgeDrinksKeepsItAndTheNumbersCanBeTuned()
        {
            var blade = new ChargedBlade { Mode = BladeMode.NeverFades, DamageShare = 1.5f };
            blade.Charge(Elements.Wind);
            for (int i = 0; i < 10; i++) Assert.AreEqual(1.5f, blade.OnBlowLanded().DamageShare, 1e-5f);
            Assert.IsTrue(blade.Charged, "never fades");
            var longer = new ChargedBlade();
            longer.Charge(Elements.Earth, 5);
            Assert.AreEqual(5, longer.Left);
            for (int i = 1; i <= 5; i++)
            {
                var blow = longer.OnBlowLanded();
                Assert.AreEqual(i, blow.Number, "a longer charge numbers its blows from one");
                Assert.AreEqual(i == 5, blow.Third, "its extra comes on the last blow");
            }
            var drinks = new ChargedBlade { Mode = BladeMode.NeverFades };
            drinks.Charge(Elements.Fire);
            for (int i = 1; i <= 6; i++) Assert.AreEqual(i % 3 == 0, drinks.OnBlowLanded().Third, "a blade that never fades still finds every third");
        }

        [Test]
        public void ABlowLaysBothItsAfflictionsAndOnlyThose()
        {
            var set = new AfflictionSet();
            var hit = new Hit(10f, 5f, Vector3.forward, affliction: AfflictionKind.Static, afflictionStacks: 1,
                affliction2: AfflictionKind.Chill, affliction2Stacks: 2);
            set.TakeFrom(hit, null);
            Assert.AreEqual(1, set.Stacks(AfflictionKind.Static), "the blow's own affliction");
            Assert.AreEqual(2, set.Stacks(AfflictionKind.Chill), "the charged element's, beside it");
            Assert.AreEqual(0, set.Stacks(AfflictionKind.Burn));

            var plain = new AfflictionSet();
            plain.TakeFrom(new Hit(10f, 5f, Vector3.forward), null);
            Assert.IsFalse(plain.Any, "a plain blow lays nothing");
        }

        [Test]
        public void AChargedBladeOfFireLaysBurnAsTheSecondAffliction()
        {
            var set = new AfflictionSet();
            set.TakeFrom(new Hit(10f, 5f, Vector3.forward, affliction2: AfflictionKind.Burn, affliction2Stacks: 1), null);
            Assert.IsTrue(set.Burn.IsBurning, "Burn rides as a second affliction like any other");
        }

        [Test]
        public void EachElementHasItsStrikeAndDeepStrikeNamed()
        {
            foreach (var element in Elements.All)
            {
                string strike = ElementSteel.StrikeName(element.Name, false);
                string deep = ElementSteel.StrikeName(element.Name, true);
                Assert.AreNotEqual("Infused strike", strike, element.Name);
                Assert.AreNotEqual("Deep strike", deep, element.Name);
                Assert.AreNotEqual(strike, deep, element.Name);
            }
            Assert.AreEqual("Burning Edge", ElementSteel.StrikeName(Elements.Fire, false));
            Assert.AreEqual("Ice Through", ElementSteel.StrikeName(Elements.Ice, true));
            Assert.AreEqual(2, ElementSteel.Stacks(Elements.Poison), "Poison's charged blows dose twice");
            Assert.AreEqual(1, ElementSteel.Stacks(Elements.Fire));
            Assert.IsTrue(ElementSteel.PassesGuard(Elements.Metal));
            Assert.IsFalse(ElementSteel.PassesGuard(Elements.Fire));
        }
    }
}

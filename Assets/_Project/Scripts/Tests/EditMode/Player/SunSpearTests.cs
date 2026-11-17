using System.Linq;
using Clover.Player;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>The Sun-Spear (fire_rework.md section 5): Fire taken gives it, it fights as the Great Spear, and its own move stands and comes home as a ray.</summary>
    public class SunSpearTests
    {
        [Test]
        public void TakingAGodGivesItsOwnWeapon()
        {
            Assert.AreEqual(Weapons.SunSpearName, Weapons.OfGod(Elements.Fire));
            Assert.AreEqual(Weapons.StormKatanaName, Weapons.OfGod(Elements.Lightning));
            Assert.IsNull(Weapons.OfGod(Elements.Water), "a god whose weapon is not made gives none, and the one in the hand is kept");
            Assert.AreEqual(Weapons.SunSpearName, Weapons.ByName(Weapons.SunSpearName).Name, "a run carried into the next scene finds it again by its name");
            Assert.IsFalse(Weapons.Offered.Contains(Weapons.SunSpearName), "the god's spear is given, not set out on a stand");
        }

        [Test]
        public void ItFightsAsTheGreatSpearSaveForItsOwnThrow()
        {
            var sun = Weapons.SunSpear();
            var great = Weapons.GreatSpear();
            Assert.AreEqual(great.Prefix, sun.Prefix, "the same poses");
            Assert.AreNotEqual(great.Carry, sun.Carry, "drawn as the king's spear, not the iron one");
            var sunMoves = sun.Combo.Steps.Concat(new[] { sun.Combo.GuardAttack, sun.Combo.Heavy, sun.Combo.ChainHeavy, sun.Combo.ChargedHeavy, sun.Combo.Finisher }).ToArray();
            var greatMoves = great.Combo.Steps.Concat(new[] { great.Combo.GuardAttack, great.Combo.Heavy, great.Combo.ChainHeavy, great.Combo.ChargedHeavy, great.Combo.Finisher }).ToArray();
            Assert.AreEqual(greatMoves.Length, sunMoves.Length);
            for (int i = 0; i < greatMoves.Length; i++)
            {
                Assert.AreEqual(greatMoves[i].Name, sunMoves[i].Name);
                Assert.AreEqual(greatMoves[i].Damage, sunMoves[i].Damage, 1e-4f, greatMoves[i].Name);
            }
        }

        [Test]
        public void ItsThrowStandsAndComesHomeAsARayThatStrikes()
        {
            var skill = Weapons.SunSpear().Combo.Skill;
            Assert.IsNotNull(skill.Shot);
            Assert.IsTrue(skill.Shot.Sticks, "it stands in the floor where it lands");
            Assert.IsTrue(skill.Shot.RaysBack, "it is called home as a ray");
            Assert.IsNull(skill.FollowStep, "nobody runs to it");
            Assert.Greater(skill.Shot.ReturnScale * skill.Shot.Damage, 0f, "the ray hurts what it crosses");
            Assert.GreaterOrEqual(skill.Shot.RayRadius * 2f, 2f, "as wide as the king's own ray, about two metres");
            Assert.AreEqual("sunspear", skill.Shot.Prop, "the spear itself flies");
            // The Great Spear's Flying Spear is only fetched: its return harms nothing.
            Assert.IsFalse(Weapons.GreatSpear().Combo.Skill.Shot.RaysBack);
        }
    }
}

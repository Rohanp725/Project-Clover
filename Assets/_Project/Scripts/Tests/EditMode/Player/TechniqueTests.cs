using System.Collections.Generic;
using Clover.Player;
using Clover.Player.Build;
using Clover.Player.Growth;
using Clover.Player.Trees.Ways;
using Clover.Player.Way;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The technique on the chord (steel/weapons.md 0.5; way_build_plan.md 2.3 and 3.1, questions 7 and 39; task 5.3):
    /// sealed or unbought, the weapon has none and the chord stays a heavy; bought, the armoury puts it on the weapon in
    /// every aspect. Before the Thunder lands in 0.06 s for 22 damage and 15 stamina, no mana, behind the Last Retainer
    /// outside the playtest, for 2 Memories. The Fists, taught twice, play the one bought first.
    /// </summary>
    public class TechniqueTests
    {
        [Test]
        public void SealedOrUnboughtTheWeaponHasNoTechniqueAndTheChordStaysAHeavy()
        {
            Assert.IsNull(Armoury.Resolve(Weapons.KatanaName, "", MarkSet.None).Technique);
            Assert.IsNull(Weapons.Katana().Technique, "a weapon built without the armoury has none");
            Assert.IsNull(Armoury.Resolve(Weapons.ScytheName, "", new MarkSet(KatanaWay.BeforeTheThunder)).Technique, "the Katana's on another weapon");
            Assert.IsNull(Techniques.StepFor(Weapons.GreatSpearName, new MarkSet("spear.holdthegate")), "Hold the Gate waits on the Great Spear's Way");
        }

        [Test]
        public void BoughtItIsOnTheWeaponInEveryAspectAFreshStepEachTime()
        {
            var marks = new[] { "", KatanaWay.Sheath, KatanaWay.Stream, KatanaWay.Answer, KatanaWay.Storm };
            foreach (string aspect in marks)
            {
                var weapon = Armoury.Resolve(Weapons.KatanaName, aspect, new MarkSet(KatanaWay.BeforeTheThunder, aspect));
                Assert.IsNotNull(weapon.Technique, "in force in " + (aspect == "" ? "the plain Katana" : aspect));
            }
            var a = Armoury.Resolve(Weapons.KatanaName, "", new MarkSet(KatanaWay.BeforeTheThunder)).Technique;
            var b = Armoury.Resolve(Weapons.KatanaName, "", new MarkSet(KatanaWay.BeforeTheThunder)).Technique;
            Assert.AreNotSame(a, b);
        }

        [Test]
        public void BeforeTheThunderHasTheOwnersNumbers()
        {
            var step = Techniques.BeforeTheThunder();
            Assert.AreEqual(0.06f, step.Windup, 1e-5f, "lands in 0.06 s, the quickest of any move (question 7)");
            Assert.Less(step.Windup, Weapons.Katana().Combo.Skill.Windup, "quicker than the Iaido Slash");
            Assert.AreEqual(22f, step.Damage, 1e-5f);
            Assert.AreEqual(15f, step.StaminaCost, 1e-5f);
            Assert.AreEqual(2.8f, step.Reach, 1e-5f);
            Assert.AreEqual(120f, step.ArcDegrees, 1e-5f);
            Assert.AreEqual(0.8f, step.Lunge, 1e-5f);
            Assert.AreEqual(0.6f, step.Recovery, 1e-5f, "struck nothing in a wind-up: the long recovery");
            Assert.IsFalse(step.Heavy);
            StringAssert.StartsWith("attack_", step.Name, "an attack clip, or the animator has no state for it");
        }

        [Test]
        public void ItWaitsBehindTheLastRetainerOutsideThePlaytestAndCostsTwo()
        {
            var mark = Upgrade.ById(KatanaWay.BeforeTheThunder);
            Assert.AreEqual(UpgradeKind.Technique, mark.Kind);
            Assert.AreEqual(KatanaWay.LastRetainerFlag, mark.Gate);
            Assert.AreEqual(2, mark.Cost);
            var learned = new HashSet<string>();
            var wallet = new Wallet { Memories = 9 };
            Assert.IsFalse(MarkRules.CanBuy(mark, learned, wallet, true, false, flag => false, 0), "sealed");
            Assert.IsTrue(MarkRules.CanBuy(mark, learned, wallet, true, false, flag => flag == KatanaWay.LastRetainerFlag, 0), "his fall opens it");
            Assert.IsTrue(MarkRules.CanBuy(mark, learned, wallet, true, true, flag => false, 0), "the playtest opens every gate");
            StringAssert.StartsWith("Press light and heavy attack together.", mark.Description);
        }

        [Test]
        public void TheFistsPlayTheTechniqueBoughtFirst()
        {
            var first = new Upgrade { Id = "fists.technique.a", Kind = UpgradeKind.Technique, Tree = MarkTree.Way, Weapon = Weapons.FistsName, Currency = Currency.Memories };
            var second = new Upgrade { Id = "fists.technique.b", Kind = UpgradeKind.Technique, Tree = MarkTree.Way, Weapon = Weapons.FistsName, Currency = Currency.Memories };
            var loadout = new Loadout { Weapon = Weapons.FistsName };
            Assert.IsTrue(WayRules.SettleBought(first, loadout));
            Assert.AreEqual(first.Id, loadout.Technique);
            Assert.IsFalse(WayRules.SettleBought(second, loadout), "the second waits to be chosen at a fire");
            Assert.AreEqual(first.Id, loadout.Technique);
            Assert.IsTrue(WayRules.InForce(first, loadout));
            Assert.IsFalse(WayRules.InForce(second, loadout));
            var katana = new Loadout { Weapon = Weapons.KatanaName };
            Assert.IsFalse(WayRules.SettleBought(Upgrade.ById(KatanaWay.BeforeTheThunder), katana), "one technique a weapon: nothing to choose");
            Assert.AreEqual("", katana.Technique);
        }
    }
}

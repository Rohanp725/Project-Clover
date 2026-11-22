using System.Linq;
using Clover.EditorTools;
using Clover.Player;
using Clover.Player.Forms;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// The Great Hammer's infused strike as a slam (staging/blender/2026-11-21_strikes/slam, the owner's verdicts in
    /// docs/design/feedback/2026-11-21_swing_comments.md): all eleven built from the slam recipe about the impact point,
    /// Lightning's begun before the head lands, the pieces made once per enemy struck only on the struck, the cracks
    /// above the patch, nothing painted white; and the hammer's Lightning a slam in place, not Flash Cut's lunge.
    /// </summary>
    public class SlamStrikeTests
    {
        private static readonly string[] Ring = { "fire", "lightning", "wind", "ice", "water", "life", "earth", "metal", "light", "dark", "poison" };
        private const int LightningIndex = 1, DarkIndex = 9, PoisonIndex = 10;

        private static ElementLookSet Built()
        {
            var set = ElementLookBuilder.Set();
            if (set == null || UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(ElementLookBuilder.SlamRecipePath) == null)
                Assert.Ignore("The slam recipe is not in the project, or the looks are not built; run Clover > Build > Element Looks.");
            return set;
        }

        [Test]
        public void EachElementHasItsSlamKeyOnTheRingsOrder()
        {
            for (int i = 0; i < Ring.Length; i++)
            {
                Assert.AreEqual("e3_slam_" + Ring[i], ElementMoves.SlamKey(i, false));
                Assert.AreEqual("e3_slam_" + Ring[i] + "@b", ElementMoves.SlamKey(i, true));
            }
            Assert.IsNull(ElementMoves.SlamKey(Ring.Length, false));
        }

        [Test]
        public void AllElevenAreBuiltAndOnlyLightningBeginsBeforeTheHeadLands()
        {
            var set = Built();
            for (int i = 0; i < Ring.Length; i++)
            {
                int look = set.Find(ElementMoves.SlamKey(i, false));
                Assert.GreaterOrEqual(look, 0, Ring[i]);
                Assert.Greater(set.Looks[look].PartCount, 0, Ring[i]);
                if (i == LightningIndex) Assert.AreEqual(0.13f, set.Looks[look].Lead, 0.02f, "the bolt strikes the raised head 0.13 s before it lands");
                else Assert.AreEqual(0f, set.Looks[look].Lead, 1e-4f, Ring[i]);
            }
        }

        [Test]
        public void ThePiecesMadeForEachEnemyStruckAreOnlyOnTheStruck()
        {
            var set = Built();
            // Dark's smoke burst, Poison's bruise and veins and Lightning's jumps go on each enemy struck; the rest have none.
            foreach (int i in new[] { LightningIndex, DarkIndex, PoisonIndex })
                Assert.GreaterOrEqual(set.Find(ElementMoves.SlamKey(i, true)), 0, Ring[i]);
            foreach (int i in Enumerable.Range(0, Ring.Length).Except(new[] { LightningIndex, DarkIndex, PoisonIndex }))
                Assert.Less(set.Find(ElementMoves.SlamKey(i, true)), 0, Ring[i]);
        }

        [Test]
        public void TheCracksLieAboveThePatchAndNothingIsWhite()
        {
            var set = Built();
            for (int i = 0; i < Ring.Length; i++)
            {
                var look = set.Looks[set.Find(ElementMoves.SlamKey(i, false))];
                var parts = set.Parts.Skip(look.FirstPart).Take(look.PartCount).ToArray();
                foreach (var part in parts)
                {
                    if (part.Mesh == null) continue;
                    foreach (var colour in part.Mesh.colors.Take(1))
                        Assert.IsFalse(colour.r > 0.97f && colour.g > 0.97f && colour.b > 0.97f, $"{Ring[i]}: {part.Mesh.name} is white");
                    // A ribbon on the ground is raised over the patch the game lifts 3.5 cm.
                    if (part.Kind == ElementLookSet.Ribbon && part.Mesh.name.Contains("crack") && part.Mesh.bounds.max.y < 0.15f)
                        Assert.Greater(part.Mesh.bounds.min.y, 0.035f, $"{Ring[i]}: {part.Mesh.name} lies under the patch");
                }
            }
        }

        [Test]
        public void EveryShapeIsBuiltForAllElevenAndPlayedByItsWeapon()
        {
            var set = Built();
            for (int shape = 0; shape < ElementMoves.Shapes.Length; shape++)
                for (int i = 0; i < Ring.Length; i++)
                    Assert.GreaterOrEqual(set.Find(ElementMoves.ShapeKey(shape, i, false)), 0, $"{ElementMoves.Shapes[shape]} {Ring[i]}");
            Assert.AreEqual(ElementMoves.Slam, PlayerCombat.ShapeOf(Weapons.GreatHammer()));
            Assert.AreEqual(ElementMoves.Spin, PlayerCombat.ShapeOf(Weapons.Scythe()));
            Assert.AreEqual(ElementMoves.Thrust, PlayerCombat.ShapeOf(Weapons.GreatSpear()));
            Assert.AreEqual(ElementMoves.Dash, PlayerCombat.ShapeOf(Weapons.TwinKnives()));
            Assert.AreEqual(ElementMoves.Lash, PlayerCombat.ShapeOf(Weapons.ChainSickle()));
            Assert.AreEqual(-1, PlayerCombat.ShapeOf(Weapons.Katana()), "an arc weapon keeps the arc strikes");
            // Dash and lash draw Dark's open hands beside each one struck.
            Assert.GreaterOrEqual(set.Find(ElementMoves.ShapeKey(ElementMoves.Dash, DarkIndex, true)), 0);
            Assert.GreaterOrEqual(set.Find(ElementMoves.ShapeKey(ElementMoves.Lash, DarkIndex, true)), 0);
        }

        [Test]
        public void TheHammersLightningIsASlamInPlaceNotFlashCutsLunge()
        {
            var carrier = Weapons.GreatHammer().CarrierStep;
            Assert.AreEqual(StepEffect.GroundSlam, carrier.Effect, "the hammer's carrier is its slam");
            var lunged = carrier.Clone();
            ElementSteel.Bend(lunged, Elements.Lightning, false);
            Assert.AreEqual(5f, lunged.Travel, 1e-5f, "elsewhere Lightning's strike is Flash Cut's 5 m");
            var inPlace = carrier.Clone();
            ElementSteel.Bend(inPlace, Elements.Lightning, false, true);
            Assert.AreEqual(carrier.Travel, inPlace.Travel, 1e-5f, "no lunge");
            Assert.IsFalse(inPlace.PassThrough);
            Assert.AreEqual(carrier.Damage * 1.5f, inPlace.Damage, 1e-4f, "still one and a half times the blow");
        }

        [Test]
        public void TheShapeWeaponsStrikeInPlaceWithLightningAndTheArcWeaponsLunge()
        {
            // The owner's answer, 2026-11-22: Scythe, Great Spear, Twin Knives and Chain Sickle strike where they stand, like the hammer.
            foreach (var weapon in new[] { Weapons.Scythe(), Weapons.GreatSpear(), Weapons.TwinKnives(), Weapons.ChainSickle() })
                Assert.IsTrue(PlayerCombat.StrikesInPlace(weapon, weapon.CarrierStep, LightningIndex), weapon.Name + " strikes in place");
            foreach (var weapon in new[] { Weapons.Katana(), Weapons.MassiveSword() })
                Assert.IsFalse(PlayerCombat.StrikesInPlace(weapon, weapon.CarrierStep, LightningIndex), weapon.Name + " keeps Flash Cut's lunge");
        }
    }
}

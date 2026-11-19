using System.IO;
using System.Linq;
using Clover.EditorTools;
using Clover.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// The approved weapon models as the builder uses them: each bow strung on its own model's grip and nocks (the
    /// plain bow on today's points), and the multi-object files (the Katana with its scabbard, the chain sickle with its
    /// weight and link) giving each prefab only its own object.
    /// </summary>
    public class WeaponModelTests
    {
        private static void Near(Vector3 expected, Vector3 actual, string what) =>
            Assert.That(Vector3.Distance(expected, actual), Is.LessThan(0.001f), $"{what}: expected {expected:F3}, got {actual:F3}");

        [Test]
        public void ThePlainBowKeepsTodaysGripAndNocks()
        {
            var shape = WeaponBuilder.BowShapeOf(WeaponBuilder.SpecFor("bow"));
            Near(Vector3.zero, shape.Grip, "plain grip");
            Near(new Vector3(0f, 0.312f, 1.178f), shape.NockUpper, "plain upper nock");
            Near(new Vector3(0f, 0.312f, -0.628f), shape.NockLower, "plain lower nock");
        }

        [Test]
        public void TheTrueLongbowCarriesItsOwnPoints()
        {
            Assert.That(WeaponBuilder.BowShapes.TryGetValue("bow_ice_true.fbx", out var shape), "bow_ice_true.fbx has no shape");
            Near(Vector3.zero, shape.Grip, "true longbow grip");
            Near(new Vector3(0f, 0.19f, 0.90f), shape.NockUpper, "true longbow upper nock");
            Near(new Vector3(0f, 0.19f, -0.90f), shape.NockLower, "true longbow lower nock");
            // A bow whose file is not listed is strung as the plain bow, not left unstrung.
            var stranger = new WeaponBuilder.Spec { Carry = "test_bow", ModelFile = "no_such_bow.fbx" };
            Near(WeaponBuilder.PlainBow.NockUpper, WeaponBuilder.BowShapeOf(stranger).NockUpper, "unlisted bow");
        }

        [Test]
        public void EveryBowModelHasAShape()
        {
            foreach (string file in Directory.GetFiles(WeaponBuilder.ModelFolder, "bow*.fbx").Select(Path.GetFileName))
                Assert.That(WeaponBuilder.BowShapes.ContainsKey(file), $"{file} has no grip and nock points in WeaponBuilder.BowShapes");
        }

        [Test]
        public void TheBuiltBowIsStrungAtItsShapesNocks()
        {
            var prefab = WeaponBuilder.Prefab("bow");
            Assert.IsNotNull(prefab, "no bow prefab");
            var shape = WeaponBuilder.BowShapeOf(WeaponBuilder.SpecFor("bow"));
            var upper = prefab.transform.Find("tip_upper");
            var lower = prefab.transform.Find("tip_lower");
            Assert.IsNotNull(upper, "the bow prefab has no tip_upper");
            Assert.IsNotNull(lower, "the bow prefab has no tip_lower");
            Near(shape.NockUpper - shape.Grip, upper.localPosition, "built upper nock");
            Near(shape.NockLower - shape.Grip, lower.localPosition, "built lower nock");
        }

        [Test]
        public void EachPrefabTakesOnlyItsOwnObjectsFromASharedFile()
        {
            foreach (var spec in WeaponBuilder.Specs.Where(spec => spec.ModelObjects != null))
            {
                Assert.IsNotNull(AssetDatabase.LoadAssetAtPath<GameObject>($"{WeaponBuilder.ModelFolder}/{spec.ModelFile}"), $"{spec.Carry}: no model at {spec.ModelFile}");
                var prefab = WeaponBuilder.Prefab(spec.Carry);
                var meshes = prefab.GetComponentsInChildren<MeshFilter>(true).Select(filter => filter.name).ToArray();
                CollectionAssert.AreEquivalent(spec.ModelObjects, meshes, $"{spec.Carry} is made of {string.Join(", ", meshes)}");
            }
        }

        [Test]
        public void TheKatanaIsAModelShownByTheAnimatorNotTheRack()
        {
            var katana = WeaponBuilder.SpecFor(Weapon.KatanaCarry);
            Assert.IsNotNull(katana, "no Katana spec");
            Assert.That(katana.ShownByAnimator, "the Katana must be moved between sheath and hand by PlayerAnimator");
            Assert.AreEqual("katana.fbx", katana.ModelFile);
            CollectionAssert.DoesNotContain(katana.ModelObjects, "katana_scabbard", "the scabbard is not shown until Phase 5");
        }
    }
}

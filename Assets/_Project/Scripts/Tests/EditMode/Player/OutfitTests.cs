using System.Linq;
using Clover.EditorTools;
using Clover.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// The outfits as the owner decided on 2026-11-18: each land dresses the player in its own clothes, a scene outside
    /// every land keeps what was worn last, and an outfit the player chose overrides the land. The rules are tested as
    /// plain functions, so no saved choice is read or written.
    /// </summary>
    public class OutfitTests
    {
        [TestCase("SunCity", Outfits.Fire)]
        [TestCase("SunCourt", Outfits.Fire)]
        [TestCase("Castle", Outfits.Ronin)]
        [TestCase("Summit", Outfits.Ronin)]
        public void EachLandSceneWearsItsLandsOutfit(string scene, string outfit)
        {
            Assert.AreEqual(outfit, Outfits.ForScene(scene));
            Assert.AreEqual(outfit, Outfits.Choose(scene, "", Outfits.Water));
        }

        [Test]
        public void LandsWithoutScenesHaveTheirOutfitsRegistered()
        {
            Assert.AreEqual(Outfits.Ice, Outfits.ForLand(Elements.Ice));
            Assert.AreEqual(Outfits.Water, Outfits.ForLand(Elements.Water));
            Assert.AreEqual(Outfits.Ronin, Outfits.ForLand(Elements.Wind));
            Assert.AreEqual(Outfits.Metal, Outfits.ForLand(Elements.Metal));
            Assert.AreEqual(Outfits.Poison, Outfits.ForLand(Elements.Poison));
            Assert.AreEqual(Outfits.Celtic, Outfits.ForLand(Elements.Earth));
            Assert.AreEqual(Outfits.Celtic, Outfits.ForLand(Elements.Life));
            Assert.AreEqual(Outfits.Champion, Outfits.ForLand(Elements.Light));
            Assert.AreEqual(Outfits.Champion, Outfits.ForLand(Elements.Dark));
            CollectionAssert.AreEquivalent(new[] { Outfits.Ronin, Outfits.Fire, Outfits.Ice, Outfits.Water, Outfits.Metal,
                Outfits.Poison, Outfits.Celtic, Outfits.Champion }, Outfits.All);
            // Every element's land has an outfit.
            foreach (var element in Elements.All)
                Assert.IsNotNull(Outfits.ForLand(element.Name), element.Name);
        }

        [TestCase("Hub")]
        [TestCase("CombatTest")]
        public void ASceneOutsideEveryLandKeepsTheLastOutfit(string scene)
        {
            Assert.IsNull(Outfits.ForScene(scene));
            Assert.AreEqual(Outfits.Fire, Outfits.Choose(scene, "", Outfits.Fire));
            Assert.AreEqual(Outfits.Ice, Outfits.Choose(scene, null, Outfits.Ice));
            // A first waking, with nothing worn before, wears the ronin.
            Assert.AreEqual(Outfits.Ronin, Outfits.Choose(scene, "", ""));
        }

        [Test]
        public void AChosenOutfitOverridesTheLandEverywhere()
        {
            Assert.AreEqual(Outfits.Water, Outfits.Choose("SunCity", Outfits.Water, Outfits.Fire));
            Assert.AreEqual(Outfits.Ice, Outfits.Choose("Castle", Outfits.Ice, ""));
            Assert.AreEqual(Outfits.Fire, Outfits.Choose("Hub", Outfits.Fire, Outfits.Ice));
        }

        [Test]
        public void AnOutfitThatCannotBeWornIsPassedOver()
        {
            bool known(string id) => id != Outfits.Water;
            // A chosen outfit the game no longer has falls back to the land, and a last one to the ronin.
            Assert.AreEqual(Outfits.Fire, Outfits.Choose("SunCity", Outfits.Water, "", known));
            Assert.AreEqual(Outfits.Ronin, Outfits.Choose("Hub", "", Outfits.Water, known));
        }

        [Test]
        public void TheSandboxedAndRealKeysDiffer()
        {
            Assert.AreEqual("clover.outfit.chosen", Outfits.ChosenPrefsKey);
            Assert.AreNotEqual(Outfits.ChosenPrefsKey, Outfits.ChosenSandboxKey);
            Assert.AreNotEqual(Outfits.LastPrefsKey, Outfits.LastSandboxKey);
        }

        [Test]
        public void EveryOutfitIsSkinnedToThePlayersRigInTheSameBoneOrder()
        {
            var ronin = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerBuilder.PlayerModelPath)
                .GetComponentsInChildren<SkinnedMeshRenderer>(true).Single();
            foreach (string id in Outfits.All)
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerBuilder.OutfitModelPath(id));
                Assert.IsNotNull(model, $"no model for the outfit '{id}'");
                var body = model.GetComponentsInChildren<SkinnedMeshRenderer>(true).Single();
                CollectionAssert.AreEqual(ronin.bones.Select(bone => bone.name), body.bones.Select(bone => bone.name), id);
                Assert.AreEqual(ronin.sharedMesh.bindposes.Length, body.sharedMesh.bindposes.Length, id);
            }
        }
    }
}

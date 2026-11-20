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
                Outfits.Poison, Outfits.Celtic, Outfits.Champion, Outfits.RoninGen }, Outfits.All);
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
        public void TheTrialRoninIsWornOnlyWhenChosen()
        {
            Assert.IsTrue(Outfits.IsTrial(Outfits.RoninGen));
            Assert.IsFalse(Outfits.IsTrial(Outfits.Ronin));
            // No land dresses the player in it, so the default ronin and every land's outfit stay as they were.
            foreach (var element in Elements.All)
                Assert.AreNotEqual(Outfits.RoninGen, Outfits.ForLand(element.Name), element.Name);
            Assert.AreEqual(Outfits.Ronin, Outfits.Choose("Castle", "", ""));
            Assert.AreEqual(Outfits.Fire, Outfits.Choose("SunCity", "", ""));
            // Chosen, it is worn everywhere, in a land or out of one.
            foreach (string scene in new[] { "SunCity", "Castle", "Hub", "ProvingGround" })
                Assert.AreEqual(Outfits.RoninGen, Outfits.Choose(scene, Outfits.RoninGen, Outfits.Fire), scene);
            // The playtest's switch puts it on over any choice and takes it off to no choice at all.
            Assert.AreEqual(Outfits.RoninGen, Outfits.ToggleTrial(""));
            Assert.AreEqual(Outfits.RoninGen, Outfits.ToggleTrial(Outfits.Water));
            Assert.AreEqual("", Outfits.ToggleTrial(Outfits.RoninGen));
            Assert.AreEqual("Trial ronin: on", Clover.UI.PauseChoices.TrialOutfitLabel(Outfits.RoninGen));
            Assert.AreEqual("Trial ronin: off", Clover.UI.PauseChoices.TrialOutfitLabel(""));
        }

        [Test]
        public void ThePlaytestPauseMenuOffersTheTrialOutfitAndThePlainGameDoesNot()
        {
            CollectionAssert.Contains(Entries(new Clover.UI.PauseChoices("ProvingGround", true)), Clover.UI.PauseEntry.TrialOutfit);
            CollectionAssert.Contains(Entries(new Clover.UI.PauseChoices("Castle", true)), Clover.UI.PauseEntry.TrialOutfit);
            CollectionAssert.DoesNotContain(Entries(new Clover.UI.PauseChoices("Castle", false)), Clover.UI.PauseEntry.TrialOutfit);
            CollectionAssert.DoesNotContain(Entries(new Clover.UI.PauseChoices("Hub", false)), Clover.UI.PauseEntry.TrialOutfit);
            Assert.IsFalse(Clover.UI.PauseChoices.AsksFirst(Clover.UI.PauseEntry.TrialOutfit), "a switch that can be pressed back is not asked twice");
            Assert.IsNotEmpty(Clover.UI.PauseChoices.Note(Clover.UI.PauseEntry.TrialOutfit));
        }

        private static Clover.UI.PauseEntry[] Entries(Clover.UI.PauseChoices choices) =>
            Enumerable.Range(0, choices.Count).Select(choices.At).ToArray();

        // The generated ronin as the other outfits are held to: one skinned body, the rig's bones by name and bind pose,
        // and only materials the game's palette has a toon material for.
        [Test]
        public void TheTrialRoninIsOneSkinnedBodyOnTheRigInThePalettesMaterials()
        {
            var ronin = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerBuilder.PlayerModelPath)
                .GetComponentsInChildren<SkinnedMeshRenderer>(true).Single();
            string path = PlayerBuilder.OutfitModelPath(Outfits.RoninGen);
            Assert.AreEqual("Assets/_Project/Art/Models/Characters/player_outfit_ronin_gen.fbx", path);
            var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            Assert.IsNotNull(model, "the trial ronin's model is not in the project");
            var skinned = model.GetComponentsInChildren<SkinnedMeshRenderer>(true);
            Assert.AreEqual(1, skinned.Length, "exactly one skinned mesh");
            Assert.AreEqual(1, model.GetComponentsInChildren<Renderer>(true).Length, "no weapons or other meshes ride in the file");
            var body = skinned[0];
            CollectionAssert.AreEqual(ronin.bones.Select(bone => bone.name), body.bones.Select(bone => bone.name));
            var poses = body.sharedMesh.bindposes;
            var roninPoses = ronin.sharedMesh.bindposes;
            Assert.AreEqual(roninPoses.Length, poses.Length);
            for (int i = 0; i < poses.Length; i++)
                for (int k = 0; k < 16; k++)
                    Assert.AreEqual(roninPoses[i][k], poses[i][k], 0.002f, $"bind pose of {body.bones[i].name}");

            var names = body.sharedMaterials.Select(material => material.name).ToArray();
            CollectionAssert.IsSubsetOf(names, ronin.sharedMaterials.Select(material => material.name).ToArray(), "only the ronin's own slots");
            // The built set dresses it in the toon materials of the same names, as PlayerBuilder.Dress does the ronin.
            var set = AssetDatabase.LoadAssetAtPath<OutfitSet>(PlayerBuilder.OutfitsPath);
            Assert.IsNotNull(set, "no built outfit set");
            var outfit = set.Find(Outfits.RoninGen);
            Assert.IsNotNull(outfit, "the built outfit set has no trial ronin: run Clover > Build > Player Outfits");
            Assert.AreSame(body.sharedMesh, outfit.Mesh);
            CollectionAssert.AreEqual(names, outfit.Materials.Select(material => material.name));
            var toon = set.Find(Outfits.Ronin).Materials[0].shader;
            foreach (var material in outfit.Materials)
            {
                Assert.AreEqual(toon, material.shader, material.name);
                StringAssert.StartsWith(BuildKit.GeneratedRoot, AssetDatabase.GetAssetPath(material), material.name);
            }
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

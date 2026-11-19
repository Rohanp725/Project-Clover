using Clover.EditorTools;
using Clover.Player;
using NUnit.Framework;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Clover.Tests
{
    /// <summary>The player looks its actions up by name, so the names in the asset are part of the code's contract.</summary>
    public class ControlsAssetTests
    {
        [Test]
        public void ControlsHaveThePlayerMapWithTheActionsThePlayerReads()
        {
            var controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(PlayerBuilder.ControlsPath);
            Assert.IsNotNull(controls, $"no input actions at {PlayerBuilder.ControlsPath}");
            var map = controls.FindActionMap("Player");
            Assert.IsNotNull(map, "no Player action map");
            foreach (string action in new[] { "Move", "Dodge", "LightAttack", "HeavyAttack", "Guard", "Flask", "WeaponSkill", "Surge", "Interact", "Pause" })
            {
                var found = map.FindAction(action);
                Assert.IsNotNull(found, $"no {action} action");
                Assert.Greater(found.bindings.Count, 0, $"{action} has no bindings");
            }
            Assert.IsNull(map.FindAction("Spell"), "the spell button became the weapon skill (renamed, so saved bindings keep its id)");
            Assert.AreEqual("5a1f0b52-6c1d-4f0e-9a51-1c0f7d2a1008", map.FindAction("WeaponSkill").id.ToString(), "WeaponSkill keeps the old Spell action's id");
        }

        [Test]
        public void ControlsHaveTheSpellsMapWithTheLayerAndFourSlots()
        {
            var controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(PlayerBuilder.ControlsPath);
            var map = controls.FindActionMap("Spells");
            Assert.IsNotNull(map, "no Spells action map");
            foreach (string action in new[] { "SpellLayer", "SpellWest", "SpellNorth", "SpellEast", "SpellSouth" })
            {
                var found = map.FindAction(action);
                Assert.IsNotNull(found, $"no {action} action");
                Assert.AreEqual(2, found.bindings.Count, $"{action} has a pad and a keys binding");
            }
            foreach (string action in LayeredButtons.SlotActions) Assert.IsNotNull(map.FindAction(action), action);
        }

        [Test]
        public void TheSavedArenaGivesThePlayerItsControls()
        {
            if (!System.IO.File.Exists(CombatTestArenaBuilder.ScenePath))
                Assert.Ignore("The combat test arena has not been built yet; run Clover > Build > Combat Test Arena.");

            var scene = EditorSceneManager.OpenPreviewScene(CombatTestArenaBuilder.ScenePath);
            try
            {
                PlayerController player = null;
                foreach (var root in scene.GetRootGameObjects())
                    player = player != null ? player : root.GetComponentInChildren<PlayerController>(true);
                Assert.IsNotNull(player, "no PlayerController in the arena");
                var controls = new SerializedObject(player).FindProperty("controls").objectReferenceValue;
                Assert.IsNotNull(controls, "the player's controls reference is empty");
            }
            finally
            {
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }
    }
}

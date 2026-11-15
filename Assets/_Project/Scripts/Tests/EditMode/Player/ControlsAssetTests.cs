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
            var controls = AssetDatabase.LoadAssetAtPath<InputActionAsset>(CombatTestArenaBuilder.ControlsPath);
            Assert.IsNotNull(controls, $"no input actions at {CombatTestArenaBuilder.ControlsPath}");
            var map = controls.FindActionMap("Player");
            Assert.IsNotNull(map, "no Player action map");
            foreach (string action in new[] { "Move", "Dodge" })
            {
                var found = map.FindAction(action);
                Assert.IsNotNull(found, $"no {action} action");
                Assert.Greater(found.bindings.Count, 0, $"{action} has no bindings");
            }
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

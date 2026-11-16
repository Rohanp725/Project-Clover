using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>The start and end every scene builder shares: refuse to throw away unsaved work, then save and reopen.</summary>
    public static class SceneBuild
    {
        /// <summary>False, with an error in the console, when the open scene has unsaved changes that building would lose.</summary>
        public static bool Begin(string what)
        {
            for (int i = 0; i < EditorSceneManager.sceneCount; i++)
            {
                if (!EditorSceneManager.GetSceneAt(i).isDirty) continue;
                Debug.LogError($"[Clover.Build] The open scene has unsaved changes; save or discard them, then build {what} again.");
                return false;
            }
            return true;
        }

        public static void Finish(UnityEngine.SceneManagement.Scene scene, string scenePath, bool firstInBuild)
        {
            if (!EditorSceneManager.SaveScene(scene, scenePath))
            {
                Debug.LogError($"[Clover.Build] The scene could not be saved to {scenePath}.");
                return;
            }
            var scenes = EditorBuildSettings.scenes.Where(existing => existing.path != scenePath).ToList();
            var entry = new EditorBuildSettingsScene(scenePath, true);
            if (firstInBuild) scenes.Insert(0, entry);
            else scenes.Add(entry);
            EditorBuildSettings.scenes = scenes.ToArray();
            AssetDatabase.SaveAssets();
            // Open the saved scene afresh. Entering Play Mode straight from the scene as it was built in memory lost the
            // player's reference to the controls asset every time; the same scene loaded from disk never did.
            EditorSceneManager.OpenScene(scenePath, OpenSceneMode.Single);
            Debug.Log($"[Clover.Build] Built {scenePath}.");
        }
    }
}

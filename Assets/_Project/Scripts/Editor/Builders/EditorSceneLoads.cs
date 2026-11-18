using System.IO;
using Clover.World;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace Clover.EditorTools
{
    /// <summary>
    /// Lets a door lead, in the editor's play mode, to a scene that is kept out of the list the game is built from
    /// (the Fire land's scenes, while the demo is the hub, the castle and the summit). The build list is not touched.
    /// </summary>
    [InitializeOnLoad]
    public static class EditorSceneLoads
    {
        static EditorSceneLoads()
        {
            EditorApplication.playModeStateChanged -= OnPlayModeChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
        }

        private static void OnPlayModeChanged(PlayModeStateChange change)
        {
            if (change != PlayModeStateChange.EnteredPlayMode) return;
            GateOfNoon.LoadOutsideBuild = Load;
            Clover.Core.SceneRoutes.LoadOutsideBuild = Load;
        }

        private static bool Load(string sceneName)
        {
            string path = $"Assets/_Project/Scenes/{sceneName}.unity";
            if (!File.Exists(path)) return false;
            EditorSceneManager.LoadSceneInPlayMode(path, new LoadSceneParameters(LoadSceneMode.Single));
            return true;
        }
    }
}

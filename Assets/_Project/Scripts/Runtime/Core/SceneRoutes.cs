using UnityEngine;
using UnityEngine.SceneManagement;

namespace Clover.Core
{
    /// <summary>
    /// Every way from one scene to another goes through here. Outside the playtest a scene is loaded by its name as it
    /// always was. In the playtest the hub is not part of the build: whatever would lead home to the hub leads to the
    /// Proving Ground instead. In the editor a scene that is not in the build list (the Fire land, the Proving Ground)
    /// is still loaded, through the loader the editor's own code puts in <see cref="LoadOutsideBuild"/>.
    /// </summary>
    public static class SceneRoutes
    {
        public const string HubScene = "Hub";

        /// <summary>
        /// In the editor, a loader for scenes kept out of the build list; given a scene's name it says whether it loaded
        /// it. Never set in a built game, where only the scenes built with are loaded.
        /// </summary>
        public static System.Func<string, bool> LoadOutsideBuild;

        /// <summary>The name of the last scene this asked to load, for tests and probes.</summary>
        public static string LastAsked { get; private set; } = "";

        // Domain reload is off: the last play's loader and its record must not be this one's.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            LoadOutsideBuild = null;
            LastAsked = "";
        }

        /// <summary>Where a scene's name leads now: the hub is the Proving Ground while the playtest is on.</summary>
        public static string Resolve(string sceneName) =>
            PlaytestMode.On && sceneName == HubScene ? PlaytestMode.ProvingGroundScene : sceneName;

        /// <summary>The hub, or in the playtest the Proving Ground.</summary>
        public static string Home => Resolve(HubScene);

        /// <summary>Loads the scene the name leads to. Returns false when it is neither in the build nor loadable from the editor.</summary>
        public static bool Load(string sceneName)
        {
            if (string.IsNullOrEmpty(sceneName)) return false;
            string scene = Resolve(sceneName);
            LastAsked = scene;
            if (Application.CanStreamedLevelBeLoaded(scene))
            {
                SceneManager.LoadScene(scene);
                return true;
            }
            // Outside the playtest a scene is loaded by its name alone, as it always was.
            if (PlaytestMode.On && LoadOutsideBuild != null && LoadOutsideBuild(scene)) return true;
            // Not loadable here: asking anyway gives Unity's own error, which says which scene is missing.
            SceneManager.LoadScene(scene);
            return false;
        }
    }
}

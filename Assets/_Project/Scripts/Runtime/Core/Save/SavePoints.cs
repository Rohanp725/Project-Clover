using UnityEngine;

namespace Clover.Core.Save
{
    /// <summary>
    /// The moments the save is written (IMPLEMENTATION.md 5.1): resting at a fire, changing scene, taking or consuming a
    /// god, quitting. One method for each, named for where it is called, so what is gathered into the save at each moment
    /// (the wallet, the Hand, the last rest) is added here as the tasks that own it move it out of <see cref="RunState"/>.
    /// Today the save holds only <see cref="Progress"/>'s flags, which are already written as they are set, so these
    /// write again only what a first look into PlayerPrefs added. Nothing is written while sandboxed.
    /// </summary>
    public static class SavePoints
    {
        // Domain reload is off: the quitting handler is taken off and put back once each play, never twice.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad()
        {
            Application.quitting -= OnQuitting;
            Application.quitting += OnQuitting;
        }

        /// <summary>Called by a rest fire when the player rests at it.</summary>
        public static bool Rested() => Progress.Save();

        /// <summary>Called by <see cref="SceneRoutes.Load"/> just before another scene is loaded.</summary>
        public static bool SceneChanging(string sceneName) => Progress.Save();

        /// <summary>Called once a god's element has been taken or consumed.</summary>
        public static bool GodRewarded() => Progress.Save();

        /// <summary>Called as the game quits (hooked to <c>Application.quitting</c> here).</summary>
        public static bool Quitting() => Progress.Save();

        private static void OnQuitting() => Quitting();
    }
}

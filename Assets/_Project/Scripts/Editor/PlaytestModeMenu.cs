using Clover.Core;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// The editor's switch for the combat playtest's rules (<see cref="PlaytestMode"/>): unlimited Shards at rest, the
    /// free chart, no loss on a fall, the guide and the marks' details. Ticked, every play in the editor has them;
    /// unticked, the game is as it always was. The Proving Ground turns them on for itself whatever this says, and the
    /// Combat Playtest build has them always. Kept in the editor's own settings, not in the project.
    /// </summary>
    [InitializeOnLoad]
    public static class PlaytestModeMenu
    {
        private const string Path = "Clover/Play/Playtest Mode";
        private const string Key = "Clover.PlaytestMode";

        static PlaytestModeMenu()
        {
            PlaytestMode.EditorSwitch = EditorPrefs.GetBool(Key, false);
            EditorApplication.playModeStateChanged -= PlayChanged;
            EditorApplication.playModeStateChanged += PlayChanged;
        }

        // Domain reload is off, so what a play set (the Proving Ground turns the mode on) would outlive it into edit mode.
        private static void PlayChanged(PlayModeStateChange change)
        {
            if (change == PlayModeStateChange.EnteredEditMode) PlaytestMode.ResetToSetting();
        }

        [MenuItem(Path)]
        public static void Toggle()
        {
            bool on = !EditorPrefs.GetBool(Key, false);
            EditorPrefs.SetBool(Key, on);
            PlaytestMode.EditorSwitch = on;
            Debug.Log(on
                ? "[Clover.Play] Plays in the editor now follow the playtest's rules (unlimited Shards at rest, the free chart, no loss on a fall)."
                : "[Clover.Play] Plays in the editor follow the game's own rules again.");
        }

        [MenuItem(Path, true)]
        private static bool Shown()
        {
            Menu.SetChecked(Path, EditorPrefs.GetBool(Key, false));
            return !Application.isPlaying;
        }
    }
}

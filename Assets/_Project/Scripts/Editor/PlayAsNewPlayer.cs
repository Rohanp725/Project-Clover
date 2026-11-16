using Progress = Clover.Core.Progress;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// A switch for playing the game from its very beginning without losing what has really been unlocked: while it
    /// is on, every play in the editor starts as a new player (bare hands, the Katana still in its stone, no Lightning)
    /// and writes nothing down. Turn it off and the saved progress is as it was. It is the same flag the scripted
    /// probes set for themselves, so a probe run leaves it off.
    /// </summary>
    public static class PlayAsNewPlayer
    {
        private const string Path = "Clover/Play/As A New Player (saves nothing)";

        [MenuItem(Path)]
        public static void Toggle()
        {
            bool on = PlayerPrefs.GetInt(Progress.SandboxFlag, 0) != 1;
            if (on) PlayerPrefs.SetInt(Progress.SandboxFlag, 1);
            else PlayerPrefs.DeleteKey(Progress.SandboxFlag);
            PlayerPrefs.Save();
            Debug.Log(on
                ? "[Clover.Play] Plays in the editor now start as a new player and save nothing."
                : "[Clover.Play] Plays in the editor use the saved progress again.");
        }

        [MenuItem(Path, true)]
        private static bool Shown()
        {
            Menu.SetChecked(Path, PlayerPrefs.GetInt(Progress.SandboxFlag, 0) == 1);
            return !Application.isPlaying;
        }
    }
}

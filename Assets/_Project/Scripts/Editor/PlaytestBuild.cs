using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// The Windows playtest build: a development build of the scenes enabled in Build Settings into
    /// Builds/[Product]_Playtest_v[version]_[date]/, with a READ ME FIRST beside the exe. Zipping is left to the
    /// playtest-build skill (Compress-Archive) so this file has no dependency on compression assemblies.
    /// </summary>
    public static class PlaytestBuild
    {
        private static readonly string[] DemoScenes =
        {
            "Assets/_Project/Scenes/Hub.unity",
            "Assets/_Project/Scenes/Castle.unity",
            "Assets/_Project/Scenes/Summit.unity",
        };

        private static string ReadMe(string product) =>
$@"{PlayerSettings.productName}: playtest demo {PlayerSettings.bundleVersion}, made {DateTime.Now:yyyy-MM-dd}

A top-down action game of hard fights. You wake on a stopped clock, cross a castle
under a storm, and climb to the god of lightning.

THIS IS AN EARLY DEMO. There is NO SOUND yet. Nothing is wrong with your speakers.

TO START
  Run {product}.exe. It opens on the whole screen at your display's own size.
  Alt+Enter switches between fullscreen and a window you can resize; so does
  ""Fullscreen or window"" in the pause menu. Any screen shape should work.
  A voice speaks first: press Cross (pad) or E (keys) to read on.
  Then walk north through the gate.

CONTROLS            PlayStation pad        Keyboard
  Move              Left stick             W A S D
  Light blow        R1                     J
  Heavy blow        R2                     K      (hold: the weapon's skill)
  Guard             L1                     L      (at the last instant: perfect guard)
  Dodge             Circle                 Space
  Flask (heal)      Square                 R
  Element spell     Triangle               U
  Element surge     L2                     I
  Use / read on     Cross                  E
  Pause             Start                  Esc
  An Xbox pad works too: the same buttons by position (RB, RT, LB, B, X, Y, LT, A, Menu).

PAUSE, QUIT
  Start or Esc pauses. The pause menu has the controls, the fullscreen switch,
  ""Give up the run"" (back to the clock) and ""Quit"". Alt+F4 also quits.

WHAT TO TRY
  - The story: bare hands and Fire, through the gate to the castle. Draw the Katana
    from its stone, beat the three kinds of soldier and the Gate Captain, climb to
    the Summit and fight Narukami: a swordsman first, and then what he really is.
    When he falls, choose to take his power or consume it, and you wake on the clock again.
  - Red attacks cannot be guarded: roll. Pale ones can. Break an enemy's balance
    and finish it with a heavy blow.
  - Embers from the fallen are spent at rest fires, on the star chart.
  - The stands and braziers round the rim of the clock are not part of the story:
    every weapon and element is open there to try. Signs beside them say so.
  - Falling is expected. You wake on the clock and go again.

IF SOMETHING GOES WRONG
  Please send this file with a note of what you were doing:
  %USERPROFILE%\AppData\LocalLow\{PlayerSettings.companyName}\{PlayerSettings.productName}\Player.log
";

        [MenuItem("Clover/Build/Windows Playtest")]
        public static void BuildWindows()
        {
            // The demo and nothing else, the hub first: the practice yard has no way back out of it and stays in the editor.
            var scenes = DemoScenes.Where(File.Exists).ToArray();
            if (scenes.Length != DemoScenes.Length)
            {
                Debug.LogError("[Clover.Build] A scene of the demo is missing: " + string.Join(", ", DemoScenes.Where(path => !File.Exists(path))));
                return;
            }
            EditorBuildSettings.scenes = scenes.Select(path => new EditorBuildSettingsScene(path, true)).ToArray();
            // Other people's screens: the whole screen at the display's own size, without a change of mode, in a window
            // that can be resized after Alt+Enter or the pause menu's entry; the picture is never stretched.
            PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            PlayerSettings.defaultIsNativeResolution = true;
            PlayerSettings.resizableWindow = true;
            PlayerSettings.allowFullscreenSwitch = true;
            PlayerSettings.runInBackground = true;
            string product = Regex.Replace(PlayerSettings.productName, @"[^A-Za-z0-9]+", "");
            string dir = $"Builds/{product}_Playtest_v{PlayerSettings.bundleVersion}_{DateTime.Now:yyyyMMdd}";
            Directory.CreateDirectory(dir);

            var options = new BuildPlayerOptions
            {
                scenes = scenes,
                locationPathName = $"{dir}/{product}.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development,
            };
            var report = BuildPipeline.BuildPlayer(options);
            var summary = report.summary;
            if (summary.result == BuildResult.Succeeded)
            {
                File.WriteAllText(Path.Combine(dir, "READ ME FIRST.txt"), ReadMe(product).Replace("\r\n", "\n").Replace("\n", "\r\n"));
                Debug.Log($"[Clover.Build] Succeeded: {dir} ({summary.totalSize / (1024 * 1024)} MB, {summary.totalTime.TotalSeconds:F0} s).");
            }
            else
            {
                Debug.LogError($"[Clover.Build] {summary.result}: {summary.totalErrors} error(s). See the console.");
            }
        }
    }
}

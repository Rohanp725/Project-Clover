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
    /// The Windows build of the evolving combat playtest, beside the old demo's (Windows Playtest), which stays buildable
    /// for the players who have it. It starts in the Proving Ground, holds the levels the Proving Ground's doors lead to,
    /// and is compiled with CLOVER_PLAYTEST, so the playtest's rules are always on in it. It goes under a product name of
    /// its own, so its saved bindings and window never mix with the demo's on a machine that has both. The editor's own
    /// build list is not touched. The READ ME beside the exe is docs/design/playtest/readme.md with its blanks filled.
    /// </summary>
    public static class CombatPlaytestBuild
    {
        public static readonly string[] Scenes =
        {
            ProvingGroundBuilder.ScenePath,
            "Assets/_Project/Scenes/Castle.unity",
            "Assets/_Project/Scenes/Summit.unity",
            "Assets/_Project/Scenes/SunCity.unity",
            "Assets/_Project/Scenes/SunCourt.unity",
        };

        public const string ProductSuffix = " Combat Playtest";
        public const string Define = "CLOVER_PLAYTEST";

        /// <summary>The version the title shows, from whats_new.md, as "0.1" for folder names.</summary>
        public static string ShortVersion()
        {
            string path = Path.Combine(PlaytestTextsBuilder.SourceFolder, "whats_new.md");
            string first = File.Exists(path) ? File.ReadLines(path).FirstOrDefault(line => line.StartsWith("# ")) ?? "" : "";
            var match = Regex.Match(first, @"\d+(\.\d+)+");
            return match.Success ? match.Value : PlayerSettings.bundleVersion;
        }

        [MenuItem("Clover/Build/Combat Playtest")]
        public static void BuildWindows()
        {
            var missing = Scenes.Where(path => !File.Exists(path)).ToArray();
            if (missing.Length > 0)
            {
                Debug.LogError("[Clover.Build] A scene of the combat playtest is missing: " + string.Join(", ", missing) + ". Build the Proving Ground first (Clover > Build > Proving Ground).");
                return;
            }
            // The words the guide, the chart and the title show, as the docs have them now.
            PlaytestTextsBuilder.Build();

            string ownProduct = PlayerSettings.productName;
            string product = ownProduct + ProductSuffix;
            string exe = Regex.Replace(product, @"[^A-Za-z0-9]+", "");
            string version = ShortVersion();
            string dir = $"Builds/{exe}_v{version}_{DateTime.Now:yyyyMMdd}";
            Directory.CreateDirectory(dir);
            var options = new BuildPlayerOptions
            {
                scenes = Scenes,
                locationPathName = $"{dir}/{exe}.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.Development,
                extraScriptingDefines = new[] { Define },
            };
            BuildReport report;
            // Other people's screens, as the demo's build has them; the product name only for the length of the build.
            var mode = PlayerSettings.fullScreenMode;
            bool native = PlayerSettings.defaultIsNativeResolution, resizable = PlayerSettings.resizableWindow;
            bool switchable = PlayerSettings.allowFullscreenSwitch, background = PlayerSettings.runInBackground;
            try
            {
                PlayerSettings.productName = product;
                PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
                PlayerSettings.defaultIsNativeResolution = true;
                PlayerSettings.resizableWindow = true;
                PlayerSettings.allowFullscreenSwitch = true;
                PlayerSettings.runInBackground = true;
                report = BuildPipeline.BuildPlayer(options);
            }
            finally
            {
                PlayerSettings.productName = ownProduct;
                PlayerSettings.fullScreenMode = mode;
                PlayerSettings.defaultIsNativeResolution = native;
                PlayerSettings.resizableWindow = resizable;
                PlayerSettings.allowFullscreenSwitch = switchable;
                PlayerSettings.runInBackground = background;
                // The build writes the settings to disk with its own values in them: write them again as they were.
                EditorUtility.SetDirty(Unsupported.GetSerializedAssetInterfaceSingleton("PlayerSettings"));
                AssetDatabase.SaveAssets();
            }
            var summary = report.summary;
            if (summary.result != BuildResult.Succeeded)
            {
                Debug.LogError($"[Clover.Build] Combat playtest {summary.result}: {summary.totalErrors} error(s). See the console.");
                return;
            }
            File.WriteAllText(Path.Combine(dir, "READ ME FIRST.txt"), ReadMe(product, exe, version).Replace("\r\n", "\n").Replace("\n", "\r\n"));
            Debug.Log($"[Clover.Build] Combat playtest succeeded: {dir} ({summary.totalSize / (1024 * 1024)} MB, {summary.totalTime.TotalSeconds:F0} s).");
        }

        private static string ReadMe(string product, string exe, string version)
        {
            string path = Path.Combine(PlaytestTextsBuilder.SourceFolder, "readme.md");
            string text = File.Exists(path) ? File.ReadAllText(path) : "{product}: Combat Playtest {version}\n";
            string news = Path.Combine(PlaytestTextsBuilder.SourceFolder, "whats_new.md");
            string heading = File.Exists(news) ? File.ReadLines(news).FirstOrDefault(line => line.StartsWith("# ")) ?? "" : "";
            string shown = heading.Length > 2 ? heading.Substring(2).Replace("Combat Playtest", "").Trim() : version;
            return text.Replace("{product}", PlayerSettings.productName).Replace("{version}", shown)
                .Replace("{date}", DateTime.Now.ToString("yyyy-MM-dd")).Replace("{exe}", exe + ".exe")
                .Replace("{log}", $"%USERPROFILE%\\AppData\\LocalLow\\{PlayerSettings.companyName}\\{product}\\Player.log");
        }
    }
}

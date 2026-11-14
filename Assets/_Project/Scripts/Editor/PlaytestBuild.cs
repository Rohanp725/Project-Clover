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
        [MenuItem("Clover/Build/Windows Playtest")]
        public static void BuildWindows()
        {
            var scenes = EditorBuildSettings.scenes.Where(s => s.enabled).Select(s => s.path).ToArray();
            if (scenes.Length == 0)
            {
                Debug.LogError("[Clover.Build] No scenes are enabled in Build Settings; add one with 'unity command scene add_to_build'.");
                return;
            }
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
                File.WriteAllText(Path.Combine(dir, "READ ME FIRST.txt"),
                    $"{PlayerSettings.productName} playtest build {PlayerSettings.bundleVersion}, made {DateTime.Now:yyyy-MM-dd}.\r\n" +
                    $"Run {product}.exe. If something goes wrong, send the log at\r\n" +
                    $"%USERPROFILE%\\AppData\\LocalLow\\{PlayerSettings.companyName}\\{PlayerSettings.productName}\\Player.log\r\n");
                Debug.Log($"[Clover.Build] Succeeded: {dir} ({summary.totalSize / (1024 * 1024)} MB, {summary.totalTime.TotalSeconds:F0} s).");
            }
            else
            {
                Debug.LogError($"[Clover.Build] {summary.result}: {summary.totalErrors} error(s). See the console.");
            }
        }
    }
}

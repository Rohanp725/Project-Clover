using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Clover.Core;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Clover.EditorTools
{
    /// <summary>
    /// Golden-image review: renders every ShotStation in the open scenes to review/shots/current and compares each frame with
    /// review/shots/golden. The report names the stations that changed; a person decides whether the change is wanted and
    /// accepts the current frames as the new goldens.
    /// </summary>
    public static class ShotStations
    {
        public const string CurrentDir = "review/shots/current";
        public const string GoldenDir = "review/shots/golden";
        public const string ReportPath = "review/shots/diff.md";

        /// <summary>Mean per-channel difference (0-255) above which one pixel counts as changed.</summary>
        public const float PixelThreshold = 12f;
        /// <summary>Share of changed pixels above which a station is reported as different.</summary>
        public const float ChangedShareThreshold = 0.005f;

        [MenuItem("Clover/Review/Capture Stations")]
        public static void CaptureAll()
        {
            var stations = FindStations();
            Directory.CreateDirectory(CurrentDir);
            Directory.CreateDirectory(GoldenDir);
            var rows = new List<string>();
            int differing = 0, missing = 0;
            foreach (var s in stations)
            {
                string current = Path.Combine(CurrentDir, s.Name + ".png");
                string golden = Path.Combine(GoldenDir, s.Name + ".png");
                Render(s, current);
                if (!File.Exists(golden)) { rows.Add($"| {s.Name} | no golden image yet | - | - |"); missing++; continue; }
                var d = Compare(current, golden);
                string verdict = d.sizeMismatch ? "size differs" : d.changedShare > ChangedShareThreshold ? "DIFFERENT" : "same";
                if (verdict != "same") differing++;
                rows.Add($"| {s.Name} | {verdict} | {d.changedShare * 100f:F2}% | {d.meanDelta:F1} |");
            }

            var sb = new StringBuilder();
            sb.AppendLine("# Review shots");
            sb.AppendLine();
            sb.AppendLine($"Written {DateTime.Now:yyyy-MM-dd HH:mm} by Clover > Review > Capture Stations. {stations.Count} stations in the open scenes; {differing} differ from their golden image, {missing} have none yet. A pixel counts as changed when its mean channel difference exceeds {PixelThreshold:F0}; a station differs when more than {ChangedShareThreshold * 100f:F1}% of its pixels changed.");
            sb.AppendLine();
            sb.AppendLine("| station | verdict | changed pixels | mean delta |");
            sb.AppendLine("|---|---|---|---|");
            foreach (var r in rows) sb.AppendLine(r);
            sb.AppendLine();
            sb.AppendLine($"Current frames: `{CurrentDir}/`. Golden frames: `{GoldenDir}/`. Accept the current look with Clover > Review > Accept Current As Golden.");
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, sb.ToString());
            Debug.Log($"[Clover.Shots] {stations.Count} stations captured, {differing} differ, {missing} without a golden image. Report: {ReportPath}");
        }

        [MenuItem("Clover/Review/Accept Current As Golden")]
        public static void AcceptCurrent()
        {
            if (!Directory.Exists(CurrentDir)) { Debug.LogWarning("[Clover.Shots] Nothing captured yet; run Capture Stations first."); return; }
            Directory.CreateDirectory(GoldenDir);
            int n = 0;
            foreach (var png in Directory.GetFiles(CurrentDir, "*.png"))
            {
                File.Copy(png, Path.Combine(GoldenDir, Path.GetFileName(png)), true);
                n++;
            }
            Debug.Log($"[Clover.Shots] {n} frames accepted as golden.");
        }

        public static List<ShotStation> FindStations()
        {
            var list = new List<ShotStation>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                    list.AddRange(root.GetComponentsInChildren<ShotStation>(true));
            }
            return list.OrderBy(s => s.Name, StringComparer.Ordinal).ToList();
        }

        /// <summary>Renders the station through a temporary camera into a PNG. Works in edit mode and in play mode.</summary>
        public static void Render(ShotStation station, string path)
        {
            var go = new GameObject("ShotStationCamera") { hideFlags = HideFlags.HideAndDontSave };
            try
            {
                var cam = go.AddComponent<Camera>();
                cam.transform.SetPositionAndRotation(station.transform.position, station.transform.rotation);
                cam.fieldOfView = station.fieldOfView;
                cam.nearClipPlane = 0.05f;
                int w = Mathf.Max(16, station.width), h = Mathf.Max(16, station.height);
                var rt = RenderTexture.GetTemporary(w, h, 24, RenderTextureFormat.ARGB32);
                var previous = RenderTexture.active;
                try
                {
                    cam.targetTexture = rt;
                    cam.Render();
                    RenderTexture.active = rt;
                    var tex = new Texture2D(w, h, TextureFormat.RGB24, false);
                    tex.ReadPixels(new Rect(0, 0, w, h), 0, 0);
                    tex.Apply();
                    File.WriteAllBytes(path, tex.EncodeToPNG());
                    UnityEngine.Object.DestroyImmediate(tex);
                }
                finally
                {
                    cam.targetTexture = null;
                    RenderTexture.active = previous;
                    RenderTexture.ReleaseTemporary(rt);
                }
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(go);
            }
        }

        public struct Diff { public bool sizeMismatch; public float changedShare, meanDelta; }

        public static Diff Compare(string aPath, string bPath)
        {
            var a = Load(aPath);
            var b = Load(bPath);
            try
            {
                if (a.width != b.width || a.height != b.height) return new Diff { sizeMismatch = true, changedShare = 1f };
                var pa = a.GetPixels32();
                var pb = b.GetPixels32();
                long changed = 0;
                double sum = 0;
                for (int i = 0; i < pa.Length; i++)
                {
                    float d = (Mathf.Abs(pa[i].r - pb[i].r) + Mathf.Abs(pa[i].g - pb[i].g) + Mathf.Abs(pa[i].b - pb[i].b)) / 3f;
                    sum += d;
                    if (d > PixelThreshold) changed++;
                }
                return new Diff { changedShare = (float)changed / pa.Length, meanDelta = (float)(sum / pa.Length) };
            }
            finally
            {
                UnityEngine.Object.DestroyImmediate(a);
                UnityEngine.Object.DestroyImmediate(b);
            }
        }

        private static Texture2D Load(string path)
        {
            var tex = new Texture2D(2, 2, TextureFormat.RGB24, false);
            tex.LoadImage(File.ReadAllBytes(path));
            return tex;
        }
    }
}

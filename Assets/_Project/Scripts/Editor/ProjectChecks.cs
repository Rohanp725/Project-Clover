using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Clover.EditorTools
{
    /// <summary>
    /// The project's conventions as scans, so they are checked by a tool and not remembered by a person: missing scripts,
    /// broken shaders, art and audio without a manifest row, runtime code reaching for editor or legacy input APIs.
    /// The EditMode tests call the same scans, so a failing check fails the test run too.
    /// </summary>
    public static class ProjectChecks
    {
        public const string ReportPath = "review/checks/checks.md";
        public const string ManifestPath = "Assets/_Project/ASSETS.csv";
        public const string ProjectRoot = "Assets/_Project";
        public const string RuntimeDir = "Assets/_Project/Scripts/Runtime";
        public static readonly string[] ManifestedDirs = { "Assets/_Project/Art", "Assets/_Project/Audio" };

        private static readonly Regex LegacyInput = new Regex(@"\bInput\.(GetKey|GetKeyDown|GetKeyUp|GetAxis|GetAxisRaw|GetButton|GetButtonDown|GetButtonUp|GetMouseButton|GetMouseButtonDown|GetMouseButtonUp|mousePosition|anyKey|touches|GetTouch)\b");
        private static readonly Regex EditorUsing = new Regex(@"^\s*using\s+UnityEditor(\.|;)", RegexOptions.Multiline);
        private static readonly Regex BuiltInRp = new Regex(@"\b(OnRenderImage|OnPreRender|OnPostRender|OnPreCull|RenderWithShader|SetReplacementShader|Graphics\.Blit)\b");

        [MenuItem("Clover/Checks/Run All")]
        public static void RunAll()
        {
            var sections = new List<(string title, List<string> items)>
            {
                ("Missing scripts in open scenes", MissingScriptsInOpenScenes()),
                ("Missing scripts in prefabs", MissingScriptsInPrefabs()),
                ("Materials with a broken shader", BrokenMaterials()),
                ("Art and audio files without a manifest row", UnlistedAssets()),
                ("Manifest rows whose file is gone", StaleManifestRows()),
                ("Runtime code using editor, legacy input or Built-in RP APIs", RuntimeCodeProblems()),
            };
            int total = sections.Sum(s => s.items.Count);

            var sb = new StringBuilder();
            sb.AppendLine("# Project checks");
            sb.AppendLine();
            sb.AppendLine($"Written {DateTime.Now:yyyy-MM-dd HH:mm} by Clover > Checks > Run All. {total} problem(s).");
            sb.AppendLine();
            foreach (var (title, items) in sections)
            {
                sb.AppendLine($"## {title} ({items.Count})");
                sb.AppendLine();
                if (items.Count == 0) sb.AppendLine("none");
                foreach (var i in items) sb.AppendLine($"- {i}");
                sb.AppendLine();
            }
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, sb.ToString());
            Debug.Log($"[Clover.Checks] {total} problem(s). Report: {ReportPath}");
        }

        public static List<string> MissingScriptsInOpenScenes()
        {
            var list = new List<string>();
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isLoaded) continue;
                foreach (var root in scene.GetRootGameObjects())
                    foreach (var t in root.GetComponentsInChildren<Transform>(true))
                        if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) > 0)
                            list.Add($"{scene.name}: {HierarchyPath(t)}");
            }
            return list;
        }

        public static List<string> MissingScriptsInPrefabs()
        {
            var list = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:Prefab", new[] { ProjectRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var go = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                if (go == null) continue;
                foreach (var t in go.GetComponentsInChildren<Transform>(true))
                    if (GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(t.gameObject) > 0)
                        list.Add($"{path}: {HierarchyPath(t)}");
            }
            return list;
        }

        public static List<string> BrokenMaterials()
        {
            var list = new List<string>();
            foreach (var guid in AssetDatabase.FindAssets("t:Material", new[] { ProjectRoot }))
            {
                string path = AssetDatabase.GUIDToAssetPath(guid);
                var m = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (m == null) continue;
                if (m.shader == null || m.shader.name == "Hidden/InternalErrorShader" || !m.shader.isSupported)
                    list.Add($"{path} ({(m.shader == null ? "no shader" : m.shader.name)})");
            }
            return list;
        }

        /// <summary>Files under Art/ and Audio/ (not .meta, code or docs) with no row in ASSETS.csv.</summary>
        public static List<string> UnlistedAssets()
        {
            var listed = ManifestPaths();
            return ManifestedFiles().Where(f => !listed.Contains(f)).ToList();
        }

        public static List<string> StaleManifestRows()
        {
            // A row may list a file the unlisted-assets scan skips (a recipe .json beside its kits), so ask the disk.
            return ManifestPaths().Where(p => !File.Exists(p)).ToList();
        }

        /// <summary>Runtime .cs files that use UnityEditor, legacy Input or Built-in RP camera hooks, as "file:line: what".</summary>
        public static List<string> RuntimeCodeProblems()
        {
            var list = new List<string>();
            if (!Directory.Exists(RuntimeDir)) return list;
            foreach (var file in Directory.GetFiles(RuntimeDir, "*.cs", SearchOption.AllDirectories))
            {
                string rel = file.Replace('\\', '/');
                var lines = File.ReadAllLines(file);
                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    if (EditorUsing.IsMatch(line)) list.Add($"{rel}:{i + 1}: using UnityEditor in the runtime assembly");
                    if (LegacyInput.IsMatch(line)) list.Add($"{rel}:{i + 1}: legacy Input API ({LegacyInput.Match(line).Value})");
                    if (BuiltInRp.IsMatch(line)) list.Add($"{rel}:{i + 1}: Built-in RP API ({BuiltInRp.Match(line).Value})");
                }
            }
            return list;
        }

        // ------------------------------------------------------------------------------------------ helpers

        private static readonly string[] NotAssets = { ".meta", ".cs", ".asmdef", ".md", ".txt", ".csv", ".json", ".shader", ".hlsl", ".cginc", ".shadergraph", ".shadersubgraph", ".mat", ".asset", ".prefab", ".anim", ".controller", ".mixer", ".vfx", ".vfxoperator", ".vfxblock" };

        private static IEnumerable<string> ManifestedFiles()
        {
            foreach (var dir in ManifestedDirs)
            {
                if (!Directory.Exists(dir)) continue;
                foreach (var f in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                {
                    string ext = Path.GetExtension(f).ToLowerInvariant();
                    if (NotAssets.Contains(ext) || Path.GetFileName(f).StartsWith(".")) continue;
                    yield return f.Replace('\\', '/');
                }
            }
        }

        public static HashSet<string> ManifestPaths()
        {
            var set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            if (!File.Exists(ManifestPath)) return set;
            foreach (var line in File.ReadAllLines(ManifestPath).Skip(1))
            {
                if (string.IsNullOrWhiteSpace(line)) continue;
                string first = line.StartsWith("\"") ? line.Substring(1, Math.Max(0, line.IndexOf('"', 1) - 1)) : line.Split(',')[0];
                first = first.Trim().Replace('\\', '/');
                if (first.Length > 0) set.Add(first);
            }
            return set;
        }

        private static string HierarchyPath(Transform t)
        {
            var sb = new StringBuilder(t.name);
            while (t.parent != null) { t = t.parent; sb.Insert(0, t.name + "/"); }
            return sb.ToString();
        }
    }
}

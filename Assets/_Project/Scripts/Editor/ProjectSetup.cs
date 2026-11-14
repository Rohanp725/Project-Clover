using System.IO;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// Makes sure the folder layout CLAUDE.md describes exists, with the asset manifest's header row. Safe to run any time:
    /// it only adds what is missing. Folders are created through AssetDatabase so Unity writes their .meta files at once.
    /// </summary>
    public static class ProjectSetup
    {
        public static readonly string[] Folders =
        {
            "Assets/_Project/Art/Models/Props",
            "Assets/_Project/Art/Models/Characters",
            "Assets/_Project/Art/Models/Environment",
            "Assets/_Project/Art/Textures/Props",
            "Assets/_Project/Art/Textures/Characters",
            "Assets/_Project/Art/Textures/Environment",
            "Assets/_Project/Art/Textures/UI",
            "Assets/_Project/Art/Materials",
            "Assets/_Project/Art/Shaders",
            "Assets/_Project/Art/VFX",
            "Assets/_Project/Art/Animation",
            "Assets/_Project/Art/Source",
            "Assets/_Project/Audio/Music",
            "Assets/_Project/Audio/Ambience",
            "Assets/_Project/Audio/SFX",
            "Assets/_Project/Audio/Voice",
            "Assets/_Project/Audio/UI",
            "Assets/_Project/Audio/Mixers",
            "Assets/_Project/Prefabs",
            "Assets/_Project/Scenes",
            "Assets/_Project/Settings",
            "Assets/_Project/Generated",
            "Assets/_Project/Scripts/Runtime/Core",
            "Assets/_Project/Scripts/Runtime/Player",
            "Assets/_Project/Scripts/Runtime/World",
            "Assets/_Project/Scripts/Runtime/Gameplay",
            "Assets/_Project/Scripts/Runtime/UI",
            "Assets/_Project/Scripts/Runtime/Audio",
            "Assets/_Project/Scripts/Editor/Builders",
            "Assets/_Project/Scripts/Tests/EditMode",
            "Assets/_Project/Scripts/Tests/PlayMode",
        };

        public const string ManifestHeader = "path,kind,source,licence,credit,prompt_or_notes,seed,date,approved_by";

        [MenuItem("Clover/Setup/Ensure Project Layout")]
        public static void EnsureLayout()
        {
            int made = 0;
            foreach (var folder in Folders)
                made += EnsureFolder(folder);
            if (!File.Exists(ProjectChecks.ManifestPath))
            {
                File.WriteAllText(ProjectChecks.ManifestPath, ManifestHeader + "\n");
                made++;
            }
            foreach (var dir in new[] { "review/shots/golden", "review/shots/current", "review/audio", "review/checks", "review/audit", "staging", "AgentScripts" })
                Directory.CreateDirectory(dir);
            AssetDatabase.Refresh();
            Debug.Log($"[Clover.Setup] Layout ensured; {made} folder(s) or file(s) created.");
        }

        private static int EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return 0;
            int made = 0;
            string parent = Path.GetDirectoryName(path).Replace('\\', '/');
            if (!AssetDatabase.IsValidFolder(parent)) made += EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
            return made + 1;
        }
    }
}

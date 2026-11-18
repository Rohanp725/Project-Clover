using System.Collections.Generic;
using System.IO;
using System.Text;
using Clover.Player;
using Clover.UI;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// Reads the playtest's written texts from docs/design/playtest/ into the generated asset the game loads
    /// (<see cref="PlaytestTexts"/>), so the owner changes the words in plain files and runs this, with no code:
    /// effects.md (the guide to effects: "## id", then "Title:" and "Group:" lines, then the words), marks/*.md (the
    /// details of high-level marks: "## mark id", then the words) and whats_new.md (the first "# " heading is the
    /// version, the rest is what is new). In the words, **bold** stays bold and "- " lines become bullets; paragraphs
    /// are kept. What was read, and any mark id the game does not know, is written to review/checks/playtest_texts.md.
    /// </summary>
    public static class PlaytestTextsBuilder
    {
        public const string SourceFolder = "docs/design/playtest";
        public const string AssetFolder = BuildKit.GeneratedRoot + "/Playtest/Resources";
        public const string AssetPath = AssetFolder + "/" + PlaytestTexts.ResourceName + ".asset";
        private const string ReportPath = "review/checks/playtest_texts.md";

        [MenuItem("Clover/Build/Playtest Texts")]
        public static void Build()
        {
            BuildKit.EnsureFolder(AssetFolder);
            var texts = AssetDatabase.LoadAssetAtPath<PlaytestTexts>(AssetPath);
            bool made = texts == null;
            if (made) texts = ScriptableObject.CreateInstance<PlaytestTexts>();
            var report = new StringBuilder("# Playtest texts\n\nRead by Clover > Build > Playtest Texts from `" + SourceFolder + "/`.\n\n");

            texts.Effects = new List<PlaytestText>();
            string effects = Path.Combine(SourceFolder, "effects.md");
            if (File.Exists(effects))
                foreach (var section in Sections(File.ReadAllText(effects)))
                {
                    var entry = new PlaytestText { Id = section.Id };
                    var body = new List<string>();
                    foreach (string line in section.Lines)
                    {
                        if (line.StartsWith("Title:") && string.IsNullOrEmpty(entry.Title)) entry.Title = line.Substring(6).Trim();
                        else if (line.StartsWith("Group:") && string.IsNullOrEmpty(entry.Group)) entry.Group = line.Substring(6).Trim();
                        else body.Add(line);
                    }
                    if (string.IsNullOrEmpty(entry.Title)) entry.Title = section.Id;
                    entry.Body = Words(body);
                    texts.Effects.Add(entry);
                }
            report.Append($"- effects.md: {texts.Effects.Count} entries\n");

            texts.Marks = new List<PlaytestText>();
            var unknown = new List<string>();
            string marks = Path.Combine(SourceFolder, "marks");
            if (Directory.Exists(marks))
                foreach (string file in Directory.GetFiles(marks, "*.md"))
                {
                    int count = 0;
                    foreach (var section in Sections(File.ReadAllText(file)))
                    {
                        var mark = Upgrade.ById(section.Id);
                        if (mark == null) unknown.Add(Path.GetFileName(file) + ": " + section.Id);
                        texts.Marks.Add(new PlaytestText { Id = section.Id, Title = mark != null ? mark.Name : section.Id, Body = Words(section.Lines) });
                        count++;
                    }
                    report.Append($"- marks/{Path.GetFileName(file)}: {count} marks\n");
                }

            string whatsNew = Path.Combine(SourceFolder, "whats_new.md");
            texts.Version = "";
            texts.WhatsNew = "";
            if (File.Exists(whatsNew))
            {
                var rest = new List<string>();
                foreach (string raw in File.ReadAllLines(whatsNew))
                {
                    string line = raw.TrimEnd();
                    if (string.IsNullOrEmpty(texts.Version) && line.StartsWith("# ")) texts.Version = line.Substring(2).Trim();
                    else if (!string.IsNullOrEmpty(texts.Version)) rest.Add(line);
                }
                texts.WhatsNew = Words(rest);
            }
            report.Append($"- whats_new.md: version \"{texts.Version}\"\n");
            if (unknown.Count > 0) report.Append("\n## Mark ids the game does not know\n\n").Append(string.Join("\n", unknown.ConvertAll(id => "- " + id))).Append('\n');

            texts.Forget();
            if (made) AssetDatabase.CreateAsset(texts, AssetPath);
            EditorUtility.SetDirty(texts);
            AssetDatabase.SaveAssets();
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, report.ToString());
            if (unknown.Count > 0) Debug.LogWarning($"[Clover.Build] Playtest texts: {unknown.Count} mark id(s) the game does not know. See {ReportPath}.");
            Debug.Log($"[Clover.Build] Playtest texts: {texts.Effects.Count} effects, {texts.Marks.Count} marks' details, version \"{texts.Version}\". {AssetPath}; {ReportPath}.");
        }

        private struct Section
        {
            public string Id;
            public List<string> Lines;
        }

        // The "## id" sections of a file; what comes before the first is not read.
        private static List<Section> Sections(string text)
        {
            var sections = new List<Section>();
            Section current = default;
            bool open = false;
            foreach (string raw in text.Replace("\r", "").Split('\n'))
            {
                string line = raw.TrimEnd();
                if (line.StartsWith("## "))
                {
                    if (open) sections.Add(current);
                    current = new Section { Id = line.Substring(3).Trim(), Lines = new List<string>() };
                    open = true;
                    continue;
                }
                if (open) current.Lines.Add(line);
            }
            if (open) sections.Add(current);
            return sections;
        }

        /// <summary>Markdown lines as the reading page's rich text: paragraphs kept, lines of one paragraph joined, bullets and bold kept.</summary>
        public static string Words(List<string> lines)
        {
            var paragraphs = new List<string>();
            var paragraph = new StringBuilder();
            void End()
            {
                if (paragraph.Length > 0) paragraphs.Add(paragraph.ToString());
                paragraph.Clear();
            }
            foreach (string raw in lines)
            {
                string line = raw.Trim();
                if (line.Length == 0)
                {
                    End();
                    continue;
                }
                if (line.StartsWith("### ") || line.StartsWith("## ") || line.StartsWith("# "))
                {
                    End();
                    paragraphs.Add("<color=#E8C77A>" + Inline(line.TrimStart('#').Trim()) + "</color>");
                    continue;
                }
                if (line.StartsWith("- ") || line.StartsWith("* "))
                {
                    if (paragraph.Length > 0) paragraph.Append('\n');
                    paragraph.Append("•  ").Append(Inline(line.Substring(2).Trim()));
                    continue;
                }
                if (paragraph.Length > 0) paragraph.Append(paragraph[paragraph.Length - 1] == '\n' ? "" : " ");
                paragraph.Append(Inline(line));
            }
            End();
            return string.Join("\n\n", paragraphs);
        }

        // **bold** to the rich text's bold; `code` loses its marks.
        private static string Inline(string text)
        {
            var built = new StringBuilder(text.Length + 16);
            bool bold = false;
            for (int i = 0; i < text.Length; i++)
            {
                if (text[i] == '*' && i + 1 < text.Length && text[i + 1] == '*')
                {
                    built.Append(bold ? "</b>" : "<b>");
                    bold = !bold;
                    i++;
                    continue;
                }
                if (text[i] == '`') continue;
                built.Append(text[i]);
            }
            if (bold) built.Append("</b>");
            return built.ToString();
        }
    }
}

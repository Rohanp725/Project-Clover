using System.Collections.Generic;
using System.Linq;
using System.Text;
using Clover.Player;

// Writes every mark of the great chart as markdown tables (by element in ring order, then by mixed branch) to
// docs/marks.generated.md, for splicing into docs/GUIDE.md. Edit mode:
//   unity command script run -- --file AgentScripts/DumpMarks.cs --entry DumpMarks.Run
public static class DumpMarks
{
    public static string Run()
    {
        var text = new StringBuilder();
        var all = Upgrade.All;
        int count = 0;
        void Table(string title, IEnumerable<Upgrade> marks)
        {
            var list = marks.OrderBy(m => (int)m.Kind == (int)UpgradeKind.Root ? 0 : 1).ThenBy(m => m.Y).ThenBy(m => m.X).ToList();
            text.Append("#### ").Append(title).Append(" (").Append(list.Count).Append(")\n\n");
            text.Append("| Mark | Kind | Embers | What it does | How it happens |\n|---|---|---|---|---|\n");
            foreach (var m in list)
            {
                count++;
                string cost = m.Signature ? "gift" : (m.Cost > 0 ? m.Cost : Upgrade.UsualCost(m.Kind)).ToString();
                text.Append("| **").Append(m.Name).Append("** | ").Append(m.Kind).Append(" | ").Append(cost).Append(" | ")
                    .Append(m.Description.Replace("|", "/").Replace("\n", " ")).Append(" | ").Append(TriggerWords.HowTo(m)).Append(" |\n");
            }
            text.Append('\n');
        }
        foreach (var element in Elements.All)
            Table(element.Name, all.Where(m => !m.IsMix && m.Element == element.Name));
        foreach (var element in Elements.All)
            Table($"{element.MixName} ({element.Name} + {element.MixWith})", all.Where(m => m.IsMix && m.BelongsTo(element.Name) && m.BelongsTo(element.MixWith)));
        System.IO.File.WriteAllText("docs/marks.generated.md", text.ToString());
        return $"wrote {count} of {all.Count} marks";
    }

    // The truthful account: every mark that does not yet do all its words say, from docs/design/marks_open_notes.tsv
    // (one line each: id, a tab, what is missing; a line is deleted when its mark is finished), written out by element
    // to docs/design/marks_open.md with the count of marks that are fully true. Edit mode:
    //   unity command script run -- --file AgentScripts/DumpMarks.cs --entry DumpMarks.Open
    public static string Open()
    {
        var notes = new Dictionary<string, string>();
        var strangers = new List<string>();
        foreach (string line in System.IO.File.ReadAllLines("docs/design/marks_open_notes.tsv"))
        {
            if (line.Length == 0 || line[0] == '#') continue;
            int tab = line.IndexOf('\t');
            if (tab <= 0) continue;
            string id = line.Substring(0, tab);
            if (Upgrade.ById(id) == null) strangers.Add(id);
            else notes[id] = line.Substring(tab + 1);
        }
        var all = Upgrade.All;
        var text = new StringBuilder();
        text.Append("# Marks that do not yet do all their words say\n\n");
        text.Append("Written by `DumpMarks.Open` from `marks_open_notes.tsv`; do not edit this page, edit the notes. ");
        text.Append(all.Count - notes.Count).Append(" of ").Append(all.Count).Append(" marks are counted fully true; the ").Append(notes.Count).Append(" below are not. ");
        text.Append("\"Today\" in a note is what the mark did when the note was written.\n\n");
        foreach (var group in all.Where(m => notes.ContainsKey(m.Id)).GroupBy(m => m.IsMix ? m.Element + " + " + m.Element2 : m.Element))
        {
            text.Append("## ").Append(group.Key).Append(" (").Append(group.Count()).Append(")\n\n");
            foreach (var m in group)
                text.Append("- **").Append(m.Name).Append("** (`").Append(m.Id).Append("`, ").Append(m.Kind).Append("): ").Append(notes[m.Id]).Append('\n');
            text.Append('\n');
        }
        System.IO.File.WriteAllText("docs/design/marks_open.md", text.ToString());
        return $"{all.Count - notes.Count} of {all.Count} fully true; {notes.Count} open" + (strangers.Count > 0 ? "; unknown ids in the notes: " + string.Join(", ", strangers) : "");
    }
}

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
}

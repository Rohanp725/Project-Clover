using System.Text;
using Clover.Player.Forms;
using UnityEditor;
using UnityEngine;

// Lists the parts of the built element looks named in AgentScripts/look_dump_only.txt (slugs, comma separated, a prefix
// matches "slug@n" too): each part's mesh, window, flags and its keys, to read how it moves at its end.
// unity command script run -- --file AgentScripts/LookPartDump.cs --entry LookPartDump.Run
public static class LookPartDump
{
    public static string Run()
    {
        var set = AssetDatabase.LoadAssetAtPath<ElementLookSet>("Assets/_Project/Generated/Vfx/Element/element_looks.asset");
        if (set == null) return "no set";
        string[] only = System.IO.File.ReadAllText("AgentScripts/look_dump_only.txt").Split(',');
        var text = new StringBuilder();
        foreach (var look in set.Looks)
        {
            bool wanted = false;
            foreach (var one in only) { var s = one.Trim(); if (s.Length > 0 && (look.Key == s || look.Key.StartsWith(s + "@"))) wanted = true; }
            if (!wanted) continue;
            text.AppendLine($"== {look.Key} seconds {look.Seconds:0.00} hold {look.Hold:0.00} loopFrom {look.LoopFrom:0.00} parts {look.PartCount} bodyFlame {look.BodyFlame} facing {look.FacingYaw:0.0} fromMain {look.FromMain}");
            for (int k = 0; k < look.PartCount; k++)
            {
                var p = set.Parts[look.FirstPart + k];
                text.Append($"  [{k}] {(p.Mesh != null ? p.Mesh.name : "-")} kind {p.Kind} show {p.Show:0.00} hide {p.Hide:0.00} cyc {p.Cycles} rep {p.Repeats} runs {p.RunsOn} fol {p.Follows} pos {p.Position} rot {p.Rotation.eulerAngles} scale {p.Scale}");
                Keys(text, set, " path", p.PathFirst, p.PathCount);
                Keys(text, set, " scale", p.ScaleFirst, p.ScaleCount);
                Keys(text, set, " spin" + p.SpinAxis, p.SpinFirst, p.SpinCount);
                Keys(text, set, " orbit", p.OrbitFirst, p.OrbitCount);
                text.AppendLine();
            }
        }
        System.IO.File.WriteAllText("review/checks/look_part_dump.txt", text.ToString());
        return "written review/checks/look_part_dump.txt";
    }

    private static void Keys(StringBuilder text, ElementLookSet set, string name, int first, int count)
    {
        if (count <= 0) return;
        text.Append(name).Append(':');
        for (int i = first; i < first + count; i++) text.Append($" {set.Keys[i].Time:0.00}={Short(set.Keys[i].Value)}/{set.Keys[i].Ease}");
    }

    private static string Short(Vector3 v) => $"({v.x:0.##},{v.y:0.##},{v.z:0.##})";
}

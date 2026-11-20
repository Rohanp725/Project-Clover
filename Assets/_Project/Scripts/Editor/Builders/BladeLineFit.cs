using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Clover.Player.Swings;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// The weapon builder's step that reads each weapon's blade line off its own model and writes it onto the prefab as
    /// a <see cref="BladeLine"/> (the swing looks follow it through the animation). The line is measured from the mesh:
    /// the vertices of the slot its <see cref="WeaponBuilder.Spec.BladeSlots"/> names (the edge, the hammer's head, the
    /// bow's limbs), their two furthest-apart points the base and the tip and a bend between them when the edge curves; a
    /// head's runs along the haft through its bounds; an opened fan's from the leaf's root out to the crown of its rim. A measurement that strays far from the model notes' line
    /// (<see cref="BladeLines.Defaults"/>) is not trusted: the notes' line is written instead, and the report says so.
    /// Clover > Weapons > Report Blade Lines writes review/checks/blade_lines.md from the built prefabs.
    /// </summary>
    public static class BladeLineFit
    {
        public const string ReportPath = "review/checks/blade_lines.md";
        private const int MostCloud = 1500;

        /// <summary>The lines of a weapon built under <paramref name="root"/> (its prefab's root, before saving), with a note for each.</summary>
        public static List<BladeLineSpec> Measure(GameObject root, WeaponBuilder.Spec spec, List<string> notes = null)
        {
            var lines = new List<BladeLineSpec>();
            if (root == null || spec == null || spec.BladeSlots == null || spec.Worn != null) return lines;
            var cloud = Cloud(root, spec.BladeSlots);
            if (spec.BladeKind == BladeKind.Head && cloud.Count > 0)
            {
                // The head is the slot's mass at the far end: a butt cap in the same slot at the other end is no part of it.
                float far = cloud.Max(point => point.z);
                cloud = cloud.Where(point => point.z >= far - 0.6f).ToList();
                // Nor is the collar where it meets the haft: the head is what stands out wide of the axis (the hammer's
                // block is 0.18 m out, its socket 0.03 m), so only the points out past half the widest are kept.
                float widest = cloud.Max(point => Mathf.Max(Mathf.Abs(point.x), Mathf.Abs(point.y)));
                var wide = cloud.Where(point => Mathf.Max(Mathf.Abs(point.x), Mathf.Abs(point.y)) >= widest * 0.5f).ToList();
                if (wide.Count >= 2) cloud = wide;
            }
            if (spec.BladeKind == BladeKind.Stave)
            {
                // A bow: each limb by itself, from the grip out to its nock.
                Add(lines, spec, 0, "upper limb", cloud.Where(point => point.z > 0.05f).ToList(), notes);
                Add(lines, spec, 1, "lower limb", cloud.Where(point => point.z < -0.05f).ToList(), notes);
            }
            else Add(lines, spec, 0, spec.BladeKind == BladeKind.Head ? "head" : spec.BladeKind == BladeKind.Point ? "point" : spec.BladeKind == BladeKind.Rim ? "rim" : "edge", cloud, notes);
            return lines;
        }

        /// <summary>Measures the lines and puts them on the prefab's root in place of any it had.</summary>
        public static int Write(GameObject root, WeaponBuilder.Spec spec, List<string> notes = null)
        {
            if (root == null) return 0;
            foreach (var old in root.GetComponents<BladeLine>()) Object.DestroyImmediate(old);
            var lines = Measure(root, spec, notes);
            foreach (var line in lines) root.AddComponent<BladeLine>().Configure(line);
            return lines.Count;
        }

        private static void Add(List<BladeLineSpec> lines, WeaponBuilder.Spec spec, int nth, string part, List<Vector3> cloud, List<string> notes)
        {
            bool hasNotes = BladeLines.TryGet(spec.Carry, nth, out var noted);
            var points = new Vector3[3];
            int count = 0;
            // A head and a point run along the haft through their middle: corner to corner would lie across a hammer's
            // block, a spear's flat leaf or the sunspear's disc, not along the thrust.
            if (spec.BladeKind == BladeKind.Head || spec.BladeKind == BladeKind.Point)
            {
                if (cloud.Count > 0)
                {
                    var bounds = new Bounds(cloud[0], Vector3.zero);
                    foreach (var point in cloud) bounds.Encapsulate(point);
                    count = BladeFit.FitHead(bounds, points);
                }
            }
            else if (spec.BladeKind == BladeKind.Rim)
            {
                // An opened fan sweeps as a whole: its line runs out through the middle of the leaf, from where the leaf
                // starts by the rivet to the crown of the rim. Rim end to rim end (the furthest pair) would lie across
                // the fan, and a swing along it would leave a thread, not the fan-shaped band.
                if (cloud.Count > 0)
                {
                    Vector3 crown = cloud.OrderByDescending(point => point.z).First();
                    var middle = cloud.Where(point => new Vector2(point.x - crown.x, point.y - crown.y).magnitude < 0.06f).ToList();
                    float root = middle.Count > 0 ? middle.Min(point => point.z) : 0f;
                    points[0] = new Vector3(crown.x, crown.y, root);
                    points[1] = crown;
                    count = crown.z - root > 0.05f ? 2 : 0;
                }
            }
            else count = BladeFit.Fit(cloud, Vector3.zero, points);
            if (count >= 2 && spec.BladeFrom > 0f) points[0] = Vector3.Lerp(points[0], points[count - 1], spec.BladeFrom);

            var measured = new BladeLineSpec(spec.Carry, hasNotes ? noted.Part : part, spec.BladeKind, hasNotes ? noted.EdgeSide : Vector3.down,
                hasNotes ? noted.Thickness : 0.03f, points.Take(count).ToArray());
            string why = Trust(measured, hasNotes ? noted : default, hasNotes, cloud.Count);
            if (why != null)
            {
                if (hasNotes)
                {
                    lines.Add(noted);
                    notes?.Add($"{spec.Carry} {noted.Part}: kept the notes' line ({why})");
                }
                else notes?.Add($"{spec.Carry} {part}: no line ({why})");
                return;
            }
            lines.Add(measured);
            notes?.Add($"{spec.Carry} {measured.Part}: measured from {cloud.Count} vertices, {count} points, {measured.Length:0.00} m");
        }

        // Null when the measured line can be used; otherwise why not.
        private static string Trust(BladeLineSpec measured, BladeLineSpec noted, bool hasNotes, int vertices)
        {
            if (vertices < 2 || !measured.Valid) return $"the slot gave {vertices} vertices";
            if (!hasNotes) return null;
            float length = measured.Length, expected = noted.Length;
            if (length < expected * 0.5f || length > expected * 1.8f) return $"{length:0.00} m long against the notes' {expected:0.00} m";
            float slack = Mathf.Max(0.3f, expected * 0.35f);
            var a = measured.Points;
            var b = noted.Points;
            float baseOff = (a[0] - b[0]).magnitude, tipOff = (a[a.Length - 1] - b[b.Length - 1]).magnitude;
            if (baseOff > slack || tipOff > slack) return $"base {baseOff:0.00} m and tip {tipOff:0.00} m from the notes' line";
            return null;
        }

        // The vertices of the named slots in the root's own space. A slot is the last word of a submesh's material name
        // (m_weapon_katana_edge), or the word in brackets of a stand-in's name (Edge (edge)).
        private static List<Vector3> Cloud(GameObject root, string[] slots)
        {
            var cloud = new List<Vector3>();
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                // A line renderer (the bow's drawn string) has no mesh to measure; Unity's stand-in null defeats ?. on GetComponent.
                Mesh mesh = renderer is SkinnedMeshRenderer skinned ? skinned.sharedMesh
                    : renderer.TryGetComponent<MeshFilter>(out var filter) ? filter.sharedMesh : null;
                if (mesh == null) continue;
                var materials = renderer.sharedMaterials;
                var vertices = mesh.vertices;
                for (int sub = 0; sub < mesh.subMeshCount; sub++)
                {
                    var material = sub < materials.Length ? materials[sub] : null;
                    if (!Wanted(slots, material != null ? material.name : "", renderer.name)) continue;
                    var seen = new HashSet<int>();
                    foreach (int index in mesh.GetTriangles(sub))
                        if (seen.Add(index) && index < vertices.Length)
                            cloud.Add(root.transform.InverseTransformPoint(renderer.transform.TransformPoint(vertices[index])));
                }
            }
            if (cloud.Count <= MostCloud) return cloud;
            // A dense mesh is thinned evenly: the furthest pair is found among what is left all the same.
            int every = Mathf.CeilToInt(cloud.Count / (float)MostCloud);
            return cloud.Where((point, i) => i % every == 0).ToList();
        }

        private static bool Wanted(string[] slots, string materialName, string rendererName)
        {
            string fromMaterial = LastWord(materialName);
            string fromName = Bracketed(rendererName);
            foreach (string slot in slots)
                if (slot == fromMaterial || slot == fromName) return true;
            return false;
        }

        private static string LastWord(string name)
        {
            string lower = name.ToLowerInvariant().Replace(" (instance)", "");
            int cut = lower.LastIndexOfAny(new[] { '_', ' ' });
            string last = cut >= 0 ? lower.Substring(cut + 1) : lower;
            int dot = last.IndexOf('.');
            return dot >= 0 ? last.Substring(0, dot) : last;
        }

        private static string Bracketed(string name)
        {
            int open = name.LastIndexOf('('), close = name.LastIndexOf(')');
            return open >= 0 && close > open ? name.Substring(open + 1, close - open - 1).Trim().ToLowerInvariant() : "";
        }

        /// <summary>Writes what each built weapon prefab carries as its blade lines, against the notes' lines, to review/checks/blade_lines.md.</summary>
        [MenuItem("Clover/Weapons/Report Blade Lines")]
        public static void Report()
        {
            var text = new StringBuilder();
            text.AppendLine("# Blade lines on the weapon prefabs");
            text.AppendLine();
            text.AppendLine("What the swing looks follow on each weapon: the line the weapon builder measured off the model (`BladeLineFit`), against the model notes' line (`BladeLines.Defaults`). Points are in the weapon's own space (grip at the origin, business end along +Z).");
            text.AppendLine();
            text.AppendLine("| Carry | Part | Kind | Points on the prefab | Notes' points | Base off (m) | Tip off (m) |");
            text.AppendLine("|---|---|---|---|---|---|---|");
            int prefabs = 0, lines = 0, missing = 0;
            foreach (var spec in WeaponBuilder.Specs)
            {
                if (spec.Worn != null || spec.BladeSlots == null) continue;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{WeaponBuilder.Folder}/{spec.Carry}/{spec.Carry}.prefab");
                if (prefab == null)
                {
                    text.AppendLine($"| {spec.Carry} | - | - | NO PREFAB | | | |");
                    missing++;
                    continue;
                }
                prefabs++;
                var found = prefab.GetComponents<BladeLine>();
                if (found.Length == 0)
                {
                    text.AppendLine($"| {spec.Carry} | - | - | NO LINE: rebuild the weapon prefabs | | | |");
                    missing++;
                    continue;
                }
                for (int n = 0; n < found.Length; n++)
                {
                    var line = found[n].Spec;
                    lines++;
                    bool noted = BladeLines.TryGet(spec.Carry, n, out var notes);
                    string baseOff = noted ? (line.Points[0] - notes.Points[0]).magnitude.ToString("0.00") : "-";
                    string tipOff = noted ? (line.Points[line.Points.Length - 1] - notes.Points[notes.Points.Length - 1]).magnitude.ToString("0.00") : "-";
                    text.AppendLine($"| {spec.Carry} | {line.Part} | {line.Kind} | {Points(line.Points)} | {(noted ? Points(notes.Points) : "-")} | {baseOff} | {tipOff} |");
                }
            }
            text.AppendLine();
            text.AppendLine("Bare hands and the Armament sweep with the body: " + string.Join(", ", BladeLines.Limbs.Select(limb => $"{limb.Part} ({limb.Bone} to {limb.TipBone} and {limb.Beyond:0.00} m on)")) + ".");
            Directory.CreateDirectory(Path.GetDirectoryName(ReportPath));
            File.WriteAllText(ReportPath, text.ToString());
            Debug.Log($"[Clover.Build] Blade lines: {lines} on {prefabs} weapon prefabs, {missing} missing; {ReportPath}");
        }

        private static string Points(Vector3[] points) =>
            points == null ? "-" : string.Join(" ", points.Select(point => $"({point.x:0.00}, {point.y:0.00}, {point.z:0.00})"));
    }
}

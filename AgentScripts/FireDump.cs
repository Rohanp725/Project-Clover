using System.Text;
using UnityEditor;
using UnityEngine;

public static class FireDump
{
    public static string Run()
    {
        var sb = new StringBuilder();
        string[] paths =
        {
            "Assets/_Project/Art/Models/Characters/enemy_thirst.fbx", "Assets/_Project/Art/Models/Characters/enemy_wick.fbx",
            "Assets/_Project/Art/Models/Characters/enemy_gilded.fbx", "Assets/_Project/Art/Models/Characters/enemy_sunblind.fbx",
            "Assets/_Project/Art/Models/Characters/elite_gilded.fbx", "Assets/_Project/Art/Models/Characters/boss_sunking.fbx",
            "Assets/_Project/Art/Models/Characters/enemy_soldier.fbx",
            "Assets/_Project/Art/Models/Characters/boss_sunform.fbx", "Assets/_Project/Art/Models/Environment/fire_kit.fbx",
            "Assets/_Project/Art/Models/Environment/tomb_panels.fbx",
        };
        foreach (var path in paths)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (asset == null) { sb.AppendLine("MISSING " + path); continue; }
            var go = Object.Instantiate(asset);
            sb.AppendLine("== " + path + " rootScale " + go.transform.localScale + " rot " + go.transform.localEulerAngles);
            foreach (Transform child in go.transform)
            {
                var r = child.GetComponentInChildren<Renderer>();
                string b = "";
                if (r != null)
                {
                    if (r is SkinnedMeshRenderer s) { var m = new Mesh(); s.BakeMesh(m); m.RecalculateBounds(); b = " meshBounds c" + m.bounds.center.ToString("F2") + " s" + m.bounds.size.ToString("F2"); Object.DestroyImmediate(m); }
                    b += " world c" + r.bounds.center.ToString("F2") + " s" + r.bounds.size.ToString("F2") + " mats " + r.sharedMaterials.Length;
                }
                sb.AppendLine("  " + child.name + " lp" + child.localPosition.ToString("F2") + " lr" + child.localEulerAngles.ToString("F0") + " ls" + child.localScale.ToString("F2") + b);
            }
            var hand = FindDeep(go.transform, "weapon.R");
            var head = FindDeep(go.transform, "head");
            var toe = FindDeep(go.transform, "foot.L");
            if (hand != null) sb.AppendLine("  weapon.R at " + hand.position.ToString("F2") + " head " + (head != null ? head.position.ToString("F2") : "-") + " foot.L " + (toe != null ? toe.position.ToString("F2") : "-"));
            Object.DestroyImmediate(go);
        }
        System.IO.File.WriteAllText("review/checks/fire_dump.txt", sb.ToString());
        return "written";
    }

    private static Transform FindDeep(Transform t, string name)
    {
        if (t.name == name) return t;
        foreach (Transform c in t) { var f = FindDeep(c, name); if (f != null) return f; }
        return null;
    }
}

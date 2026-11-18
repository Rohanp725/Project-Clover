using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

// Edit mode, the scene open: counts what the open scene draws, by group and by kind, to review/checks/world2/count.txt.
public static class CityCount
{
    public static string Run()
    {
        var sb = new StringBuilder();
        var renderers = Object.FindObjectsByType<MeshRenderer>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        long tris = 0, verts = 0, subs = 0, casting = 0;
        var byGroup = new Dictionary<string, (int count, long tris, long subs)>();
        var materials = new HashSet<Material>();
        int kit = 0, prims = 0, enabledCount = 0;
        foreach (var r in renderers)
        {
            if (!r.enabled) continue;
            var f = r.GetComponent<MeshFilter>();
            if (f == null || f.sharedMesh == null) continue;
            enabledCount++;
            var m = f.sharedMesh;
            long t = 0;
            for (int i = 0; i < m.subMeshCount; i++) t += m.GetIndexCount(i) / 3;
            tris += t;
            verts += m.vertexCount;
            subs += m.subMeshCount;
            if (r.shadowCastingMode != UnityEngine.Rendering.ShadowCastingMode.Off) casting += t;
            foreach (var mat in r.sharedMaterials) if (mat != null) materials.Add(mat);
            if (m.name.StartsWith("fire_")) kit++; else prims++;
            var t0 = r.transform;
            string g = "";
            var chain = new List<string>();
            while (t0 != null) { chain.Insert(0, t0.name); t0 = t0.parent; }
            g = string.Join("/", chain.Take(Mathf.Min(2, chain.Count)));
            byGroup.TryGetValue(g, out var v);
            byGroup[g] = (v.count + 1, v.tris + t, v.subs + m.subMeshCount);
        }
        int objects = Object.FindObjectsByType<Transform>(FindObjectsInactive.Include, FindObjectsSortMode.None).Length;
        sb.AppendLine($"objects {objects} renderers enabled {enabledCount} kit {kit} other {prims} tris {tris} casting tris {casting} verts {verts} submeshes {subs} materials {materials.Count}");
        foreach (var kv in byGroup.OrderByDescending(k => k.Value.tris).Take(40))
            sb.AppendLine($"{kv.Key}: renderers {kv.Value.count} tris {kv.Value.tris} submeshes {kv.Value.subs}");
        System.IO.Directory.CreateDirectory("review/checks/world2");
        System.IO.File.WriteAllText("review/checks/world2/count.txt", sb.ToString());
        return sb.ToString().Split('\n')[0];
    }
}

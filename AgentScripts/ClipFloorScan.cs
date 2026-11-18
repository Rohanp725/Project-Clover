// Run with: unity command script run --file AgentScripts/ClipFloorScan.cs --entry ClipFloorScan.Run
// Plays every baked player clip on a copy of the player model and finds, for each, the lowest point of the skinned
// body and of the hips over the clip. A clip whose body goes well under the floor (y = 0) is listed with when.
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class ClipFloorScan
{
    public static string Run()
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>("Assets/_Project/Art/Models/Characters/player.fbx");
        var clips = AssetDatabase.FindAssets("t:AnimationClip", new[] { "Assets/_Project/Generated/Animation/Player" })
            .Select(guid => AssetDatabase.LoadAssetAtPath<AnimationClip>(AssetDatabase.GUIDToAssetPath(guid)))
            .Where(clip => clip != null).OrderBy(clip => clip.name).ToArray();
        var copy = Object.Instantiate(asset);
        var body = copy.GetComponentInChildren<SkinnedMeshRenderer>();
        var hips = copy.GetComponentsInChildren<Transform>().First(t => t.name == "hips");
        var baked = new Mesh();
        // A foot tipped onto its toe or heel dips its sole a few centimetres below the floor; that is not the fault
        // looked for, so points carried mostly by a foot or a shin are left out.
        var weights = body.sharedMesh.boneWeights;
        var boneNames = body.bones.Select(bone => bone.name).ToArray();
        var counted = weights.Select(w => !boneNames[w.boneIndex0].StartsWith("foot") && !boneNames[w.boneIndex0].StartsWith("shin")).ToArray();
        var text = new StringBuilder();
        var report = new StringBuilder();
        int under = 0;
        foreach (var clip in clips)
        {
            float lowest = float.MaxValue, lowestAt = 0f, lowestHips = float.MaxValue, hipsAt = 0f; string part = "";
            int samples = Mathf.Max(20, Mathf.CeilToInt(clip.length * 60f));
            for (int i = 0; i <= samples; i++)
            {
                float time = clip.length * i / samples;
                clip.SampleAnimation(copy, time);
                body.BakeMesh(baked, true);
                var toWorld = body.transform.localToWorldMatrix;
                var vertices = baked.vertices;
                for (int v = 0; v < vertices.Length; v++)
                {
                    if (!counted[v]) continue;
                    float y = toWorld.MultiplyPoint3x4(vertices[v]).y;
                    if (y < lowest) { lowest = y; lowestAt = time; part = boneNames[weights[v].boneIndex0]; }
                }
                if (hips.position.y < lowestHips) { lowestHips = hips.position.y; hipsAt = time; }
            }
            bool sinks = lowest < -0.05f;
            if (sinks) under++;
            report.AppendLine($"{clip.name}\t{clip.length:0.00}\tlowest {lowest:0.00} at {lowestAt:0.00}\thips {lowestHips:0.00} at {hipsAt:0.00}{(sinks ? "\tUNDER" : "")}");
            if (sinks) text.AppendLine($"{clip.name} ({clip.length:0.00} s): body to {lowest:0.00} m at {lowestAt:0.00} s ({part}), hips to {lowestHips:0.00} m at {hipsAt:0.00} s");
        }
        Object.DestroyImmediate(copy);
        Object.DestroyImmediate(baked);
        System.IO.File.WriteAllText(System.IO.Path.Combine(System.IO.Path.GetTempPath(), "clip_floor_scan.txt"), report.ToString());
        return $"{clips.Length} clips, {under} take the body above the shins more than 5 cm under the floor\n" + text;
    }
}

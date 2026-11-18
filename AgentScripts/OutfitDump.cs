// Run with: unity command script run --file AgentScripts/OutfitDump.cs --entry OutfitDump.Run --args '["Assets/.../x.fbx"]'
// Describes a character model: its objects, skinned meshes, bone order and bind poses, so two can be compared.
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEngine;

public static class OutfitDump
{
    public static string Run(string path)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
        if (asset == null) return "no model at " + path;
        var text = new StringBuilder();
        text.AppendLine($"{path} guid {AssetDatabase.AssetPathToGUID(path)}");
        var animator = asset.GetComponent<Animator>();
        text.AppendLine($"animator {(animator != null)} avatar {(animator != null && animator.avatar != null ? animator.avatar.name : "none")}");
        var importer = (ModelImporter)AssetImporter.GetAtPath(path);
        text.AppendLine($"animationType {importer.animationType} avatarSetup {importer.avatarSetup} optimizeBones {importer.optimizeBones} clips {importer.clipAnimations.Length}");
        text.AppendLine("objects: " + string.Join(",", asset.GetComponentsInChildren<Transform>(true).Select(t => t.name)));
        foreach (var filter in asset.GetComponentsInChildren<MeshFilter>(true))
            text.AppendLine($"meshfilter {filter.name} parent {filter.transform.parent?.name} mesh {filter.sharedMesh?.name} mats {string.Join("/", filter.GetComponent<MeshRenderer>().sharedMaterials.Select(m => m?.name))}");
        foreach (var skinned in asset.GetComponentsInChildren<SkinnedMeshRenderer>(true))
        {
            var mesh = skinned.sharedMesh;
            text.AppendLine($"skinned {skinned.name} mesh {mesh.name} verts {mesh.vertexCount} tris {mesh.triangles.Length / 3} submeshes {mesh.subMeshCount} root {skinned.rootBone?.name} bounds {skinned.localBounds}");
            text.AppendLine("  mats " + string.Join(",", skinned.sharedMaterials.Select(m => m?.name)));
            text.AppendLine($"  bones {skinned.bones.Length}: " + string.Join(",", skinned.bones.Select(b => b?.name)));
            var poses = mesh.bindposes;
            text.AppendLine($"  bindposes {poses.Length}");
            for (int i = 0; i < poses.Length; i++)
                text.AppendLine($"   {skinned.bones[i]?.name} {poses[i].GetColumn(3)} {poses[i].rotation.eulerAngles}");
            text.AppendLine($"  transform {skinned.transform.localPosition} {skinned.transform.localRotation.eulerAngles} {skinned.transform.localScale}");
        }
        foreach (var t in asset.GetComponentsInChildren<Transform>(true).Take(40))
            text.AppendLine($" t {t.name} {t.localPosition} {t.localRotation.eulerAngles} {t.localScale}");
        return text.ToString();
    }
}

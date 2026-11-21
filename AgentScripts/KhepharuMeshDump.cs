using System.IO;
using UnityEditor;
using UnityEngine;

// Edit mode only: writes the Sun-Bearer model's meshes (vertices, each submesh's indices, bone weights and bind poses)
// to review/khepharu/audit/meshes.bin, since in Play Mode a mesh imported without Read/Write cannot be read, and
// KhepharuAudit needs the triangles to measure how deep the sun goes into the body and below the floor.
//   unity command script run -- --file AgentScripts/KhepharuMeshDump.cs --entry KhepharuMeshDump.Run
public static class KhepharuMeshDump
{
    public static string Run()
    {
        if (Application.isPlaying) return "edit mode only";
        Directory.CreateDirectory("review/khepharu/audit");
        int count = 0;
        using (var writer = new BinaryWriter(File.Create("review/khepharu/audit/meshes.bin")))
        {
            var all = AssetDatabase.LoadAllAssetsAtPath("Assets/_Project/Art/Models/Characters/boss_sunform.fbx");
            foreach (var asset in all)
                if (asset is Mesh) count++;
            writer.Write(count);
            foreach (var asset in all)
            {
                var mesh = asset as Mesh;
                if (mesh == null) continue;
                writer.Write(mesh.name);
                var vertices = mesh.vertices;
                writer.Write(vertices.Length);
                foreach (var v in vertices) { writer.Write(v.x); writer.Write(v.y); writer.Write(v.z); }
                writer.Write(mesh.subMeshCount);
                for (int s = 0; s < mesh.subMeshCount; s++)
                {
                    var indices = mesh.GetIndices(s);
                    writer.Write(mesh.GetTopology(s) == MeshTopology.Triangles);
                    writer.Write(indices.Length);
                    foreach (int i in indices) writer.Write(i);
                }
                var weights = mesh.boneWeights;
                writer.Write(weights.Length);
                foreach (var w in weights)
                {
                    writer.Write(w.boneIndex0); writer.Write(w.weight0);
                    writer.Write(w.boneIndex1); writer.Write(w.weight1);
                    writer.Write(w.boneIndex2); writer.Write(w.weight2);
                    writer.Write(w.boneIndex3); writer.Write(w.weight3);
                }
                var poses = mesh.bindposes;
                writer.Write(poses.Length);
                foreach (var pose in poses)
                    for (int k = 0; k < 16; k++) writer.Write(pose[k]);
            }
        }
        return $"{count} meshes written to review/khepharu/audit/meshes.bin";
    }
}

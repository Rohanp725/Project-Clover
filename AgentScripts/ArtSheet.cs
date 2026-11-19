using System.Collections.Generic;
using System.IO;
using System.Text;
using Clover.Player.Forms;
using UnityEditor;
using UnityEngine;

// Contact sheets of the built element art, one per element: every piece at its own origin in a cell 12 m square, seen
// straight down (+Z is up the picture) or from the game's 55 degrees. A grey cube marks each origin, a red one 2 m along
// +Z from it. Nothing is saved: everything is made hidden and thrown away. Args: [element or "all", "top" or "game"].
// Writes review/shots/current/artsheet_<element>_<view>.png and lists the cells in %TEMP%/artsheet.txt.
public static class ArtSheet
{
    public static string Run(string only, string view)
    {
        var set = AssetDatabase.LoadAssetAtPath<ElementArtSet>("Assets/_Project/Generated/Vfx/Element/element_art.asset");
        var square = MakeSquare();
        var text = new StringBuilder();
        var made = new List<Object>();
        var cubeMaterial = new Material(Shader.Find("Clover/Flipbook"));
        cubeMaterial.SetFloat("_Cutoff", 0f);
        made.Add(cubeMaterial);
        var red = new Material(cubeMaterial);
        red.SetColor("_Color", new Color(1f, 0.1f, 0.1f, 1f));
        var grey = new Material(cubeMaterial);
        grey.SetColor("_Color", new Color(0.6f, 0.6f, 0.6f, 1f));
        var floorMaterial = new Material(cubeMaterial);
        floorMaterial.SetColor("_Color", new Color(0.3f, 0.3f, 0.32f, 1f));
        floorMaterial.renderQueue = 1000;
        made.Add(red); made.Add(grey); made.Add(floorMaterial);
        var cube = Resources.GetBuiltinResource<Mesh>("Cube.fbx");
        foreach (string element in Clover.EditorTools.ElementArtBuilder.Elements)
        {
            if (only != "all" && only != element) continue;
            var list = new List<ElementArtSet.Piece>();
            foreach (var piece in set.Pieces) if (piece.Name.StartsWith(element + "_")) list.Add(piece);
            int columns = Mathf.CeilToInt(Mathf.Sqrt(list.Count));
            int rows = Mathf.CeilToInt(list.Count / (float)columns);
            const float cell = 12f;
            var root = new GameObject("Art Sheet") { hideFlags = HideFlags.HideAndDontSave };
            root.transform.position = new Vector3(20000f, 0f, 20000f);
            Thing(root.transform, "floor", square, floorMaterial, new Vector3((columns - 1) * cell * 0.5f, -0.02f, -(rows - 1) * cell * 0.5f), Quaternion.identity, new Vector3(columns * cell, 1f, rows * cell));
            text.AppendLine("## " + element + $" ({columns} columns)");
            for (int i = 0; i < list.Count; i++)
            {
                var piece = list[i];
                var at = new Vector3((i % columns) * cell, 0f, -(i / columns) * cell);
                text.AppendLine($"{i}: {piece.Name} kind {piece.Kind} centre {piece.Centre:F2} size {piece.Size:F2}");
                Thing(root.transform, "o", cube, grey, at, Quaternion.identity, Vector3.one * 0.2f);
                Thing(root.transform, "z", cube, red, at + Vector3.forward * 2f, Quaternion.identity, Vector3.one * 0.25f);
                if (piece.Kind == ElementArtSet.Decal)
                    Thing(root.transform, piece.Name, square, piece.Material, at + new Vector3(piece.Centre.x, 0.01f, piece.Centre.z), Quaternion.identity, new Vector3(piece.Size.x, 1f, piece.Size.z));
                else
                    Thing(root.transform, piece.Name, piece.Mesh, piece.Material, at, Quaternion.identity, Vector3.one);
            }
            var holder = new GameObject("Art Sheet Camera") { hideFlags = HideFlags.HideAndDontSave };
            var camera = holder.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.25f, 0.25f, 0.27f);
            camera.cullingMask = ~0;
            Vector3 middle = root.transform.position + new Vector3((columns - 1) * cell * 0.5f, 0f, -(rows - 1) * cell * 0.5f);
            if (view == "top")
            {
                camera.orthographic = true;
                camera.orthographicSize = rows * cell * 0.5f;
                holder.transform.SetPositionAndRotation(middle + Vector3.up * 60f, Quaternion.Euler(90f, 0f, 0f));
            }
            else
            {
                camera.fieldOfView = 40f;
                float back = rows * cell * 1.35f;
                var turn = Quaternion.Euler(55f, 0f, 0f);
                holder.transform.SetPositionAndRotation(middle + Vector3.up * 0.9f - turn * Vector3.forward * back, turn);
            }
            camera.nearClipPlane = 0.3f;
            camera.farClipPlane = 400f;
            int width = 1600, height = Mathf.RoundToInt(1600f * rows / columns);
            if (view != "top") height = 1000;
            camera.aspect = width / (float)height;
            var target = new RenderTexture(width, height, 24);
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            var picture = new Texture2D(width, height, TextureFormat.RGB24, false);
            picture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            picture.Apply();
            RenderTexture.active = null;
            Directory.CreateDirectory("review/shots/current");
            File.WriteAllBytes($"review/shots/current/artsheet_{element}_{view}.png", picture.EncodeToPNG());
            camera.targetTexture = null;
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(picture);
            Object.DestroyImmediate(holder);
            Object.DestroyImmediate(root);
        }
        foreach (var thing in made) Object.DestroyImmediate(thing);
        Object.DestroyImmediate(square);
        string outPath = Path.Combine(Path.GetTempPath(), "artsheet.txt");
        File.WriteAllText(outPath, text.ToString());
        return outPath;
    }

    private static void Thing(Transform parent, string name, Mesh mesh, Material material, Vector3 at, Quaternion rotation, Vector3 scale)
    {
        var thing = new GameObject(name) { hideFlags = HideFlags.HideAndDontSave };
        thing.transform.SetParent(parent, false);
        thing.transform.localPosition = at;
        thing.transform.localRotation = rotation;
        thing.transform.localScale = scale;
        thing.AddComponent<MeshFilter>().sharedMesh = mesh;
        thing.AddComponent<MeshRenderer>().sharedMaterial = material;
    }

    private static Mesh MakeSquare()
    {
        var mesh = new Mesh { hideFlags = HideFlags.HideAndDontSave };
        mesh.vertices = new[] { new Vector3(-0.5f, 0f, -0.5f), new Vector3(-0.5f, 0f, 0.5f), new Vector3(0.5f, 0f, 0.5f), new Vector3(0.5f, 0f, -0.5f) };
        mesh.uv = new[] { new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(1f, 1f), new Vector2(1f, 0f) };
        mesh.colors = new[] { Color.white, Color.white, Color.white, Color.white };
        mesh.triangles = new[] { 0, 1, 2, 0, 2, 3 };
        mesh.RecalculateBounds();
        return mesh;
    }
}

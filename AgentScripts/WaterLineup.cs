using System.Collections.Generic;
using System.IO;
using Clover.EditorTools;
using UnityEditor;
using UnityEngine;

// The Water land's imported enemies dressed in their toon materials, stood in a row 600 m under the open scene on a dark
// floor (the bait's jaws buried to the lip, as in the game), with the Fire land's thirsting at the end for scale. Shots:
// front, back, the game's 55-degree camera, and close-ups of the lure's face, the drowned's and the hooked's.
// Nothing is saved: everything is made hidden and thrown away. Writes review/shots/current/water_lineup_*.png.
// unity command script run -- --file AgentScripts/WaterLineup.cs --entry WaterLineup.Run
public static class WaterLineup
{
    private static readonly Vector3 Origin = new Vector3(0f, -600f, 0f);

    public static string Run()
    {
        var made = new List<Object>();
        var root = new GameObject("Water Lineup") { hideFlags = HideFlags.HideAndDontSave };
        made.Add(root);
        root.transform.position = Origin;
        var names = new List<string>(WaterEnemyKit.Models) { "enemy_thirst" };
        var placed = new Dictionary<string, Transform>();
        for (int i = 0; i < names.Count; i++)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>($"Assets/_Project/Art/Models/Characters/{names[i]}.fbx");
            var model = Object.Instantiate(asset, root.transform);
            model.hideFlags = HideFlags.HideAndDontSave;
            model.name = names[i];
            model.transform.localPosition = new Vector3((i - (names.Count - 1) * 0.5f) * 2.3f, 0f, 0f);
            model.transform.localRotation = Quaternion.Euler(0f, 180f, 0f);
            if (names[i].StartsWith("enemy_reefgrown")) model.transform.localScale = Vector3.one * WaterEnemyKit.ReefGrownScale;
            if (names[i] == "enemy_thirst") SunCityKit.Dress(model);
            else WaterEnemyKit.Dress(model);
            placed[names[i]] = model.transform;
        }
        // A dark floor, so what is buried stays buried.
        var floor = GameObject.CreatePrimitive(PrimitiveType.Quad);
        floor.hideFlags = HideFlags.HideAndDontSave;
        made.Add(floor);
        floor.transform.SetParent(root.transform, false);
        floor.transform.localRotation = Quaternion.Euler(90f, 0f, 0f);
        floor.transform.localScale = new Vector3(40f, 40f, 1f);
        var floorMaterial = new Material(Shader.Find("Universal Render Pipeline/Unlit"));
        floorMaterial.SetColor("_BaseColor", new Color(0.13f, 0.17f, 0.2f));
        made.Add(floorMaterial);
        floor.GetComponent<Renderer>().sharedMaterial = floorMaterial;
        var light = new GameObject("Lineup Sun") { hideFlags = HideFlags.HideAndDontSave };
        made.Add(light);

        var shots = new List<string>();
        // Models face -Z after the 180 turn (toward a camera on -Z).
        Shot("water_lineup_front", Origin + new Vector3(0f, 1.3f, -19f), Origin + new Vector3(0f, 0.9f, 0f), 30f, 1800, 700, shots);
        Shot("water_lineup_back", Origin + new Vector3(0f, 1.3f, 19f), Origin + new Vector3(0f, 0.9f, 0f), 30f, 1800, 700, shots);
        Vector3 game = Quaternion.Euler(55f, 0f, 0f) * Vector3.back * 17f;
        Shot("water_lineup_game", Origin + game, Origin, 38f, 1800, 900, shots);
        Face("water_lineup_face_lure", placed["enemy_bait_lure"], 0.95f, 1.1f, shots);
        // The same face with the lure's eye and dark slots on copies that draw no ink hull, to see what the hull adds
        // round the face shapes (Blender's outline left them out). The copies are thrown away.
        var lureRenderer = placed["enemy_bait_lure"].GetComponentInChildren<SkinnedMeshRenderer>();
        var inked = lureRenderer.sharedMaterials;
        var bare = (Material[])inked.Clone();
        for (int i = 0; i < bare.Length; i++)
            if (bare[i] != null && (bare[i].name == "m_water_eye" || bare[i].name == "m_water_dark"))
            {
                bare[i] = new Material(bare[i]) { name = bare[i].name + " (no ink)" };
                bare[i].SetFloat("_OutlineWidth", 0f);
                made.Add(bare[i]);
            }
        lureRenderer.sharedMaterials = bare;
        Face("water_lineup_face_lure_noink", placed["enemy_bait_lure"], 0.95f, 1.1f, shots);
        lureRenderer.sharedMaterials = inked;
        Face("water_lineup_face_drowned", placed["enemy_drowned"], 1.62f, 1.3f, shots);
        Face("water_lineup_face_hooked", placed["enemy_hooked"], 1.62f, 1.3f, shots);
        // The lure as the game sees it, close: the 55-degree camera at 5 m.
        Vector3 lure = placed["enemy_bait_lure"].position + Vector3.up * 0.7f;
        Shot("water_lineup_lure_game_close", lure + Quaternion.Euler(55f, 0f, 0f) * Vector3.back * 5f, lure, 30f, 1200, 900, shots);

        foreach (var thing in made) if (thing != null) Object.DestroyImmediate(thing);
        return "wrote " + string.Join(", ", shots);
    }

    private static void Face(string name, Transform model, float height, float distance, List<string> shots)
    {
        Vector3 head = model.position + Vector3.up * height;
        Vector3 from = head + Quaternion.Euler(8f, -25f, 0f) * Vector3.back * distance;
        Shot(name, from, head + Vector3.down * 0.05f, 30f, 1000, 1000, shots);
    }

    private static void Shot(string name, Vector3 from, Vector3 at, float fov, int width, int height, List<string> shots)
    {
        var holder = new GameObject("Lineup Camera") { hideFlags = HideFlags.HideAndDontSave };
        try
        {
            var camera = holder.AddComponent<Camera>();
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = new Color(0.22f, 0.27f, 0.32f);
            camera.fieldOfView = fov;
            camera.nearClipPlane = 0.05f;
            camera.farClipPlane = 200f;
            camera.transform.SetPositionAndRotation(from, Quaternion.LookRotation(at - from, Vector3.up));
            camera.aspect = width / (float)height;
            var target = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            var texture = new Texture2D(width, height, TextureFormat.RGB24, false);
            texture.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            texture.Apply();
            string path = $"review/shots/current/{name}.png";
            File.WriteAllBytes(path, texture.EncodeToPNG());
            shots.Add(path);
            Object.DestroyImmediate(texture);
            camera.targetTexture = null;
            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(target);
        }
        finally
        {
            Object.DestroyImmediate(holder);
        }
    }
}

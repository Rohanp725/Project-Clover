using UnityEngine;

// Edit mode, the SunCity scene open: one picture of each district from a raised camera of its own (thrown away after),
// looking north and down at 62 degrees so the faces that rise toward the game's camera show as bands:
// review/shots/current/suncity2_overview_<district>.png. Run with
//   unity command script run --file AgentScripts/SunCityOverview.cs --entry SunCityOverview.Run
public static class SunCityOverview
{
    public static string Run()
    {
        var places = new (string name, Vector3 centre, float size)[]
        {
            ("river", new Vector3(-6f, 1.5f, 60f), 76f),
            ("bank_well", new Vector3(-40f, 2f, 60f), 64f),
            ("basin_quay", new Vector3(-4f, 3f, 120f), 34f),
            ("city", new Vector3(-8f, 6f, 178f), 52f),
            ("market_yard", new Vector3(-12f, 8f, 236f), 60f),
            ("golden", new Vector3(-4f, 11f, 316f), 60f),
            ("terraces", new Vector3(-4f, 14f, 384f), 50f),
            ("hall_bowl", new Vector3(-4f, 16f, 470f), 64f),
        };
        var holder = new GameObject("Overview Camera");
        var camera = holder.AddComponent<Camera>();
        camera.orthographic = true;
        camera.nearClipPlane = 0.5f;
        camera.farClipPlane = 600f;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = new Color(0.2f, 0.2f, 0.22f);
        int count = 0;
        foreach (var (name, centre, size) in places)
        {
            var turn = Quaternion.Euler(62f, 0f, 0f);
            holder.transform.SetPositionAndRotation(centre - turn * Vector3.forward * 200f, turn);
            camera.orthographicSize = size * 0.5f;
            var target = new RenderTexture(1600, 1600, 24);
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            var picture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
            picture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            picture.Apply();
            RenderTexture.active = null;
            System.IO.File.WriteAllBytes("review/shots/current/suncity2_overview_" + name + ".png", picture.EncodeToPNG());
            camera.targetTexture = null;
            Object.DestroyImmediate(target);
            Object.DestroyImmediate(picture);
            count++;
        }
        Object.DestroyImmediate(holder);
        return "wrote " + count + " overviews";
    }
}

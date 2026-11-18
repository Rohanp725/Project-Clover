using UnityEngine;

// Edit mode, any scene open: one picture from a camera of its own (thrown away after), from a point toward a point:
//   script run --file AgentScripts/CityLook.cs --entry CityLook.Run --args '["name", fromX, fromY, fromZ, atX, atY, atZ, fov]'
// writes review/shots/current/look_<name>.png (1280x720).
public static class CityLook
{
    public static string Run(string name, float fx, float fy, float fz, float ax, float ay, float az, float fov)
    {
        var holder = new GameObject("Look Camera");
        var camera = holder.AddComponent<Camera>();
        camera.nearClipPlane = 0.2f;
        camera.farClipPlane = 800f;
        camera.fieldOfView = fov;
        var from = new Vector3(fx, fy, fz);
        holder.transform.SetPositionAndRotation(from, Quaternion.LookRotation(new Vector3(ax, ay, az) - from, Vector3.up));
        var target = new RenderTexture(1280, 720, 24);
        camera.targetTexture = target;
        camera.Render();
        RenderTexture.active = target;
        var picture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        picture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
        picture.Apply();
        RenderTexture.active = null;
        string path = "review/shots/current/look_" + name + ".png";
        System.IO.File.WriteAllBytes(path, picture.EncodeToPNG());
        camera.targetTexture = null;
        Object.DestroyImmediate(target);
        Object.DestroyImmediate(picture);
        Object.DestroyImmediate(holder);
        return path;
    }
}

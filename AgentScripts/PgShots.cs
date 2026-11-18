using UnityEngine;

// Edit-mode pictures of the Proving Ground (the scene open): the whole ground from high over its south side, and the
// rack, the pens, the dummies and the gates from the game camera's angle, a little higher. Each from a camera of its
// own, thrown away after.   unity command script run -- --file AgentScripts/PgShots.cs --entry PgShots.Run
public static class PgShots
{
    public static string Run()
    {
        Shoot("pg_ground", new Vector3(0f, 0f, 19f), 108f, 78f, 1920, 1080);
        Shoot("pg_rack", new Vector3(-27f, 0f, 1f), 26f, 52f, 1920, 1080);
        Shoot("pg_pens", new Vector3(0f, 0f, 28.5f), 46f, 55f, 1920, 1080);
        Shoot("pg_dummies", new Vector3(27f, 0f, 0f), 26f, 52f, 1920, 1080);
        Shoot("pg_gates", new Vector3(0f, 0f, 50f), 40f, 50f, 1920, 1080);
        Shoot("pg_plaza", new Vector3(0f, 0f, 0f), 26f, 55f, 1920, 1080);
        return "pg_ground, pg_rack, pg_pens, pg_dummies, pg_gates, pg_plaza to review/shots/current/";
    }

    // Looks at the spot from the south at the given pitch, far enough back that the given width fills the frame.
    private static void Shoot(string name, Vector3 at, float width, float pitch, int pixelsWide, int pixelsHigh)
    {
        var holder = new GameObject("PG Shot Camera") { hideFlags = HideFlags.HideAndDontSave };
        var camera = holder.AddComponent<Camera>();
        camera.fieldOfView = 40f;
        camera.farClipPlane = 400f;
        float aspect = pixelsWide / (float)pixelsHigh;
        float horizontal = 2f * Mathf.Atan(Mathf.Tan(camera.fieldOfView * 0.5f * Mathf.Deg2Rad) * aspect);
        float distance = width * 0.5f / Mathf.Tan(horizontal * 0.5f);
        var turn = Quaternion.Euler(pitch, 0f, 0f);
        holder.transform.SetPositionAndRotation(at - turn * Vector3.forward * distance, turn);
        var main = Camera.main;
        if (main != null)
        {
            camera.clearFlags = main.clearFlags;
            camera.backgroundColor = main.backgroundColor;
        }
        var target = new RenderTexture(pixelsWide, pixelsHigh, 24);
        camera.targetTexture = target;
        camera.Render();
        RenderTexture.active = target;
        var picture = new Texture2D(pixelsWide, pixelsHigh, TextureFormat.RGB24, false);
        picture.ReadPixels(new Rect(0, 0, pixelsWide, pixelsHigh), 0, 0);
        picture.Apply();
        RenderTexture.active = null;
        System.IO.Directory.CreateDirectory("review/shots/current");
        System.IO.File.WriteAllBytes($"review/shots/current/{name}.png", picture.EncodeToPNG());
        camera.targetTexture = null;
        Object.DestroyImmediate(target);
        Object.DestroyImmediate(picture);
        Object.DestroyImmediate(holder);
    }
}

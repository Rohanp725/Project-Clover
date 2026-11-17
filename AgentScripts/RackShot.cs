using System.Linq;
using Clover.World;
using UnityEngine;

// Edit-mode picture of every weapon stand in the open scene: a camera of its own is set up south of them, looking
// down the way the game's does, rendered once to review/shots/current/<name>.png and thrown away. Nothing in the
// scene is changed.   unity command script run -- --file AgentScripts/RackShot.cs --entry RackShot.Run
public static class RackShot
{
    public static string Run()
    {
        var stands = Object.FindObjectsByType<LoadoutChoice>(FindObjectsInactive.Exclude)
            .Where(stand => stand.name.EndsWith("Stand") && !stand.name.Contains("Brazier")).ToArray();
        if (stands.Length == 0) return "no stands";
        var bounds = new Bounds(stands[0].transform.position, Vector3.zero);
        foreach (var stand in stands) bounds.Encapsulate(stand.transform.position);
        float span = Mathf.Max(6f, Mathf.Max(bounds.size.x, bounds.size.z));
        var holder = new GameObject("Rack Shot Camera") { hideFlags = HideFlags.HideAndDontSave };
        var camera = holder.AddComponent<Camera>();
        camera.fieldOfView = 40f;
        Vector3 centre = bounds.center + Vector3.up * 1f;
        // Whichever way the stands face, they are looked at from in front.
        Vector3 front = -stands[0].transform.forward;
        if (bounds.size.z > bounds.size.x * 1.5f) front = stands[0].transform.right;
        holder.transform.position = centre - front * (span * 0.95f) + Vector3.up * (span * 0.55f);
        // Stands set round a ring are looked at from high over the ring's south side instead.
        if (bounds.size.z > bounds.size.x * 0.4f) holder.transform.position = centre + new Vector3(0f, span * 1.25f, -span * 0.55f);
        holder.transform.LookAt(centre);
        var target = new RenderTexture(1920, 820, 24);
        camera.targetTexture = target;
        camera.Render();
        RenderTexture.active = target;
        var picture = new Texture2D(target.width, target.height, TextureFormat.RGB24, false);
        picture.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
        picture.Apply();
        RenderTexture.active = null;
        string scene = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name.ToLowerInvariant();
        string path = $"review/shots/current/weapon_stands_{scene}.png";
        System.IO.File.WriteAllBytes(path, picture.EncodeToPNG());
        camera.targetTexture = null;
        Object.DestroyImmediate(target);
        Object.DestroyImmediate(picture);
        Object.DestroyImmediate(holder);
        return $"{stands.Length} stands ({string.Join(", ", stands.Select(stand => stand.name + " x " + stand.transform.position.x.ToString("0.0")))}) to {path}";
    }
}

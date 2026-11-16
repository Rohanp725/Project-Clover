using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>The small things that float over or lie under a fighter in the world: its health and stagger bars, and the mark on the ground for its attack.</summary>
    public static class WorldBars
    {
        /// <summary>Health and stagger bars above a fighter, turned to face the camera, which never rotates.</summary>
        public static (Transform health, Transform stagger, Transform root) Build(Transform parent, float height, Quaternion towardCamera,
            Material back, Material healthMaterial, Material staggerMaterial)
        {
            var bars = BuildKit.Group("Bars", parent);
            bars.localPosition = new Vector3(0f, height, 0f);
            bars.rotation = towardCamera;
            Plain(BuildKit.Shape(PrimitiveType.Quad, "Back", bars, new Vector3(0f, 0.05f, 0.01f), new Vector3(1.26f, 0.3f, 1f), back));
            var health = Fill("Health", bars, 0.11f, 0.12f, healthMaterial);
            var stagger = Fill("Stagger", bars, -0.03f, 0.07f, staggerMaterial);
            return (health, stagger, bars);
        }

        // A bar that shrinks toward its left end: the pivot sits at the left and the block hangs to its right.
        private static Transform Fill(string name, Transform parent, float height, float thickness, Material material)
        {
            var pivot = BuildKit.Group(name, parent);
            pivot.localPosition = new Vector3(-0.6f, height, 0f);
            Plain(BuildKit.Shape(PrimitiveType.Quad, name + " Block", pivot, new Vector3(0.6f, 0f, 0f), new Vector3(1.2f, thickness, 1f), material));
            return pivot;
        }

        private static void Plain(Renderer renderer)
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        /// <summary>An empty mesh holder lying on the ground; the fighter gives it the shape of its attack when the game starts.</summary>
        public static MeshFilter Marker(string name, Transform parent, float height, Material material)
        {
            var marker = new GameObject(name);
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = new Vector3(0f, height, 0f);
            var filter = marker.AddComponent<MeshFilter>();
            var renderer = marker.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            Plain(renderer);
            renderer.enabled = false;
            return filter;
        }
    }
}

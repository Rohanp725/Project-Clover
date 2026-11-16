using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

namespace Clover.EditorTools
{
    /// <summary>
    /// The small pieces every builder needs: folders, materials in the game's two shaders, and shapes. Generated assets
    /// keep stable names, so rebuilding changes them in place and nothing that points at them breaks.
    /// </summary>
    public static class BuildKit
    {
        public const string GeneratedRoot = "Assets/_Project/Generated";
        public const string ToonShaderName = "Clover/Toon";
        public const string GlowShaderName = "Clover/Glow";

        public static void EnsureFolder(string folder)
        {
            if (AssetDatabase.IsValidFolder(folder)) return;
            string parent = System.IO.Path.GetDirectoryName(folder).Replace('\\', '/');
            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, System.IO.Path.GetFileName(folder));
        }

        /// <summary>Loads the material at the path or creates it there, and makes sure it uses the named shader.</summary>
        public static Material MaterialAt(string folder, string name, string shaderName)
        {
            EnsureFolder(folder);
            string path = $"{folder}/{name}.mat";
            var shader = Shader.Find(shaderName);
            if (shader == null) Debug.LogError($"[Clover.Build] Shader {shaderName} was not found; {name} will be pink.");
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(shader);
                AssetDatabase.CreateAsset(material, path);
            }
            material.shader = shader;
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>
        /// A cel-shaded material. <paramref name="colour"/> is as the eye sees it. Shapes made in Unity have ordinary
        /// tangents, so their outline follows the normals; character meshes pass true for the smoothed ones.
        /// </summary>
        public static Material Toon(string folder, string name, Color colour, bool smoothNormalsInTangents = false,
            Texture2D texture = null, float tilesPerMetre = 0f, float outline = 0.012f)
        {
            var material = MaterialAt(folder, name, ToonShaderName);
            material.SetColor("_BaseColor", colour);
            material.SetFloat("_SmoothNormalsInTangents", smoothNormalsInTangents ? 1f : 0f);
            material.SetTexture("_BaseMap", texture);
            material.SetFloat("_WorldMapping", tilesPerMetre > 0f ? 1f : 0f);
            if (tilesPerMetre > 0f) material.SetFloat("_WorldScale", tilesPerMetre);
            // Characters carry the heavy ink; the world's line is thinner and lighter so it sits behind them.
            material.SetFloat("_OutlineWidth", smoothNormalsInTangents ? 0.026f : outline * 0.7f);
            material.SetColor("_OutlineColor", smoothNormalsInTangents ? CharacterInk : WorldInk);
            material.SetColor("_ShadeColor", new Color(0.3f, 0.28f, 0.58f));
            material.SetColor("_RimColor", Color.black);
            material.SetColor("_EmissionColor", Color.black);
            material.enableInstancing = true;
            return material;
        }

        /// <summary>Sets the ink line: thick and near black on characters, thinner and lighter on the world behind them.</summary>
        public static Material Ink(this Material material, float width, Color colour)
        {
            material.SetFloat("_OutlineWidth", width);
            material.SetColor("_OutlineColor", colour);
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>Makes an effect material graphic: a hard printed edge and, optionally, a white-hot core.</summary>
        public static Material Graphic(this Material material, float hard, float core = 0f)
        {
            material.SetFloat("_Hard", hard);
            material.SetFloat("_Core", core);
            EditorUtility.SetDirty(material);
            return material;
        }

        public static readonly Color CharacterInk = new Color(0.035f, 0.02f, 0.07f);
        public static readonly Color WorldInk = new Color(0.085f, 0.08f, 0.17f);

        /// <summary>Gives a toon material a rim and, optionally, a glow of its own. Colours here are light values and may pass 1.</summary>
        public static Material Lit(this Material material, Color rim, float rimWidth = 0.35f, Color? emission = null)
        {
            material.SetColor("_RimColor", rim);
            material.SetFloat("_RimWidth", rimWidth);
            material.SetColor("_EmissionColor", emission ?? Color.black);
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>
        /// A material made of light, for effects. Shape: 0 soft disc, 1 streak, 2 slash, 3 flat, 4 ring. Additive ones
        /// brighten what is behind them; the others cover it, as smoke and ground markers do.
        /// </summary>
        public static Material Glow(string folder, string name, Color colour, int shape, bool additive = true,
            float sharpness = 2f, float noise = 0f, bool throughWalls = false)
        {
            var material = MaterialAt(folder, name, GlowShaderName);
            material.SetColor("_Color", colour);
            material.SetFloat("_Shape", shape);
            material.SetFloat("_Sharpness", sharpness);
            material.SetFloat("_Noise", noise);
            material.SetFloat("_Hard", 0f);
            material.SetFloat("_Core", 0f);
            material.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
            material.SetFloat("_DstBlend", (float)(additive ? BlendMode.One : BlendMode.OneMinusSrcAlpha));
            material.SetFloat("_ZTest", (float)(throughWalls ? CompareFunction.Always : CompareFunction.LessEqual));
            material.renderQueue = (int)RenderQueue.Transparent;
            return material;
        }

        /// <summary>A material for tongues of flame: a hard-edged lick of the given colour with a hotter heart. Its particles must stand upright.</summary>
        public static Material Flame(string folder, string name, Color colour)
        {
            // Laid over what is behind it, not added to it: where tongues overlap they stay flat shapes and do not burn out to white.
            var material = Glow(folder, name, colour, 6, false, 1f);
            material.SetFloat("_Hard", 0.1f);
            material.SetFloat("_Core", 0f);
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>A primitive with no collider, as a child, at a local position and size.</summary>
        public static Renderer Shape(PrimitiveType type, string name, Transform parent, Vector3 localPosition, Vector3 size,
            Material material, Vector3? localEuler = null)
        {
            var shape = GameObject.CreatePrimitive(type);
            shape.name = name;
            Object.DestroyImmediate(shape.GetComponent<Collider>());
            shape.transform.SetParent(parent, false);
            shape.transform.localPosition = localPosition;
            shape.transform.localScale = size;
            if (localEuler.HasValue) shape.transform.localRotation = Quaternion.Euler(localEuler.Value);
            var renderer = shape.GetComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            return renderer;
        }

        /// <summary>A solid, static box in the world: floors, walls, steps. It keeps its collider.</summary>
        public static GameObject Block(string name, Transform parent, Vector3 position, Vector3 size, Material material,
            float yawDegrees = 0f)
        {
            var box = GameObject.CreatePrimitive(PrimitiveType.Cube);
            box.name = name;
            if (parent != null) box.transform.SetParent(parent, false);
            box.transform.position = position;
            box.transform.rotation = Quaternion.Euler(0f, yawDegrees, 0f);
            box.transform.localScale = size;
            box.GetComponent<MeshRenderer>().sharedMaterial = material;
            box.isStatic = true;
            // Whatever stands in the camera's way thins out; builders take this off the ground itself.
            box.AddComponent<Clover.World.Fadeable>();
            return box;
        }

        public static Transform Group(string name, Transform parent = null)
        {
            var group = new GameObject(name).transform;
            if (parent != null) group.SetParent(parent, false);
            return group;
        }

        public static Transform FindDeep(Transform parent, string name)
        {
            foreach (var child in parent.GetComponentsInChildren<Transform>(true))
                if (child.name == name) return child;
            return null;
        }
    }
}

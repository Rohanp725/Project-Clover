using System;
using System.IO;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Clover.EditorTools.Forge
{
    /// <summary>
    /// Renders an animation as a contact sheet: a strip of frames across the clip, seen from the game's own camera angle
    /// on the top row and from the side on the bottom. It is how an animation written as code gets looked at: every
    /// clip's sheet goes to review/anim, and a pose that is wrong is plain in it.
    /// </summary>
    public static class ForgeSheet
    {
        public const string Folder = "review/anim";
        private const int TileWidth = 250, TileHeight = 300;

        /// <summary>
        /// Draws the sheet. <paramref name="pose"/> puts the character in its pose for a moment from 0 to 1; the model is
        /// whatever was dressed for looking at (materials on, weapon shown or hidden).
        /// </summary>
        public static string Render(string name, GameObject model, Action<float> pose, int frames = 8, float height = 0.95f, float[] moments = null, float distance = 5.2f)
        {
            if (moments != null) frames = moments.Length;
            Directory.CreateDirectory(Folder);
            var scene = EditorSceneManager.NewPreviewScene();
            var sheet = new Texture2D(TileWidth * frames, TileHeight * 2, TextureFormat.RGB24, false);
            var cameraObject = new GameObject("Sheet Camera");
            var lightObject = new GameObject("Sheet Light");
            var floor = GameObject.CreatePrimitive(PrimitiveType.Plane);
            var previousParent = model.transform.parent;
            var rt = RenderTexture.GetTemporary(TileWidth, TileHeight, 24, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            try
            {
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject, scene);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(lightObject, scene);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(floor, scene);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(model, scene);
                floor.transform.localScale = Vector3.one * 3f;
                // Several pictures are taken within one editor frame; without this the skin is worked out once and every
                // picture shows the same body.
                foreach (var skinned in model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    skinned.forceMatrixRecalculationPerRender = true;
                    skinned.updateWhenOffscreen = true;
                }
                floor.GetComponent<Renderer>().sharedMaterial = BuildKit.Toon(BuildKit.GeneratedRoot + "/Forge", "m_forge_floor", new Color(0.42f, 0.42f, 0.5f), false, null, 0f, 0f);

                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.25f;
                light.color = new Color(0.95f, 0.95f, 1f);
                light.shadows = LightShadows.Soft;
                lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);

                var camera = cameraObject.AddComponent<Camera>();
                camera.scene = scene;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.16f, 0.16f, 0.2f);
                camera.fieldOfView = 32f;
                camera.nearClipPlane = 0.1f;
                camera.targetTexture = rt;

                for (int f = 0; f < frames; f++)
                {
                    float t = moments != null ? moments[f] : frames > 1 ? f / (float)(frames - 1) : 0f;
                    pose(t);
                    for (int row = 0; row < 2; row++)
                    {
                        // Top row: the game's camera, above and behind. Bottom row: from the character's right side.
                        Quaternion view = row == 0 ? Quaternion.Euler(55f, 0f, 0f) : Quaternion.Euler(4f, -90f, 0f);
                        Vector3 focus = new Vector3(0f, height, 0f);
                        cameraObject.transform.SetPositionAndRotation(focus - view * Vector3.forward * distance, view);
                        camera.Render();
                        RenderTexture.active = rt;
                        sheet.ReadPixels(new Rect(0, 0, TileWidth, TileHeight), f * TileWidth, row == 0 ? TileHeight : 0);
                    }
                }
                sheet.Apply();
                string path = $"{Folder}/{name}.png";
                File.WriteAllBytes(path, sheet.EncodeToPNG());
                return path;
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
                UnityEngine.Object.DestroyImmediate(sheet);
                // The model belongs to the caller: hand it back before the preview scene is thrown away.
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(model, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                if (previousParent != null) model.transform.SetParent(previousParent, true);
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using Clover.EditorTools;
using Clover.EditorTools.Forge;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Edit-mode close-ups of both hands through every Twin Knives clip, posed by the Forge exactly as baked. One sheet per
// clip: a row per moment, each row the right hand from outside, the right hand from the game camera's height, the
// left hand from outside and the left from above. Written to review/anim/knife_grip/<tag>_<clip>.png.
//   unity command script run -- --file AgentScripts/KnifeGripShots.cs --entry KnifeGripShots.Run --args '["after"]'
public static class KnifeGripShots
{
    private const int Tile = 300;

    public static string Run(string tag)
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerBuilder.PlayerModelPath);
        string folder = "review/anim/knife_grip";
        Directory.CreateDirectory(folder);
        var written = new List<string>();
        using (var rig = new ForgeRig(asset))
        {
            PlayerBuilder.Dress(rig.Model);
            var rack = WeaponBuilder.Hang(rig.Model, asset);
            rack.Tint(false, Color.white, 0f);
            rack.Show("knife", true);
            foreach (var t in rig.Model.GetComponentsInChildren<Transform>(true))
                if (t.name == "katana_drawn" || t.name == "katana_hilt" || t.name == "katana_sheath") t.gameObject.SetActive(false);
            var scene = EditorSceneManager.NewPreviewScene();
            var cameraObject = new GameObject("Grip Camera");
            var lightObject = new GameObject("Grip Light");
            var fill = new GameObject("Grip Fill");
            var rt = RenderTexture.GetTemporary(Tile, Tile, 24, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            try
            {
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(cameraObject, scene);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(lightObject, scene);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(fill, scene);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(rig.Model, scene);
                foreach (var skinned in rig.Model.GetComponentsInChildren<SkinnedMeshRenderer>(true))
                {
                    skinned.forceMatrixRecalculationPerRender = true;
                    skinned.updateWhenOffscreen = true;
                }
                var light = lightObject.AddComponent<Light>();
                light.type = LightType.Directional;
                light.intensity = 1.2f;
                lightObject.transform.rotation = Quaternion.Euler(50f, -35f, 0f);
                var fillLight = fill.AddComponent<Light>();
                fillLight.type = LightType.Directional;
                fillLight.intensity = 0.5f;
                fill.transform.rotation = Quaternion.Euler(20f, 150f, 0f);
                var camera = cameraObject.AddComponent<Camera>();
                camera.scene = scene;
                camera.clearFlags = CameraClearFlags.SolidColor;
                camera.backgroundColor = new Color(0.3f, 0.32f, 0.38f);
                camera.fieldOfView = 26f;
                camera.nearClipPlane = 0.05f;
                camera.targetTexture = rt;
                var handR = rig["hand.R"].Transform;
                var handL = rig["hand.L"].Transform;
                var foreR = rig["forearm.R"].Transform;
                var foreL = rig["forearm.L"].Transform;
                var knifeR = Clover.Player.WeaponRack.FindSocket(rig.Model.transform, "knife_hand");
                var knifeL = Clover.Player.WeaponRack.FindSocket(rig.Model.transform, "knife_hand_L");

                foreach (var clip in PlayerAnimations.All().Where(c => c.Name.Contains("knife")))
                {
                    float[] moments = clip.SheetMoments != null && clip.SheetMoments.Length > 0
                        ? new[] { clip.SheetMoments[1], clip.SheetMoments[clip.SheetMoments.Length / 2], clip.SheetMoments[clip.SheetMoments.Length - 2] }
                        : new[] { 0.1f, 0.45f, 0.8f };
                    var sheet = new Texture2D(Tile * 4, Tile * moments.Length, TextureFormat.RGB24, false);
                    for (int m = 0; m < moments.Length; m++)
                    {
                        clip.Pose(rig, moments[m]);
                        var chest = rig["chest"].Transform.position;
                        for (int view = 0; view < 4; view++)
                        {
                            var hand = view < 2 ? handR : handL;
                            var knife = view < 2 ? knifeR : knifeL;
                            var forearm = view < 2 ? foreR : foreL;
                            Vector3 grip = hand.TransformPoint(new Vector3(0f, 0.061f, 0f));
                            Vector3 along = (hand.position - forearm.position).normalized;
                            Vector3 blade = knife != null ? knife.forward : along;
                            // Square on to the plane of forearm and blade, from the side away from the chest; or the game's camera.
                            Vector3 side = Vector3.Cross(along, blade);
                            if (side.sqrMagnitude < 1e-3f) side = Vector3.Cross(along, Vector3.up);
                            side.Normalize();
                            Vector3 outward = grip - chest;
                            if (Vector3.Dot(side, outward + Vector3.up * 0.3f) < 0f) side = -side;
                            Vector3 from = view % 2 == 0 ? side : (Quaternion.Euler(55f, 0f, 0f) * Vector3.back);
                            Vector3 focus = grip + blade * 0.08f;
                            cameraObject.transform.position = focus + from * (view % 2 == 0 ? 1.15f : 1.5f);
                            cameraObject.transform.LookAt(focus);
                            camera.Render();
                            RenderTexture.active = rt;
                            sheet.ReadPixels(new Rect(0, 0, Tile, Tile), view * Tile, (moments.Length - 1 - m) * Tile);
                        }
                    }
                    sheet.Apply();
                    string path = $"{folder}/{tag}_{clip.Name}.png";
                    File.WriteAllBytes(path, sheet.EncodeToPNG());
                    Object.DestroyImmediate(sheet);
                    written.Add(path);
                }
            }
            finally
            {
                RenderTexture.active = previous;
                RenderTexture.ReleaseTemporary(rt);
                UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(rig.Model, UnityEngine.SceneManagement.SceneManager.GetActiveScene());
                EditorSceneManager.ClosePreviewScene(scene);
            }
        }
        return $"{written.Count} sheets: " + string.Join(", ", written);
    }
}

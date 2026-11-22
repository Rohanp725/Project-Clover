using System.Collections.Generic;
using System.IO;
using System.Linq;
using Clover.EditorTools;
using Clover.EditorTools.Forge;
using Clover.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Edit-mode films of the new hands (fingers, 2026-11-22) holding every weapon through one of its clips, posed by the
// Forge as baked and gripped as the game grips them (HandGrip.Apply): 40 frames a second of the clip (half speed at 20), each frame the holding hand
// from outside, from the front and above, and the whole body from 3.2 m, side by side (960 x 320).
// Frames to review/gifs/hands/frames/<weapon>/<nnn>.png; assemble with Pillow into review/gifs/hands/hands_<weapon>.gif.
//   unity command script run -- --file AgentScripts/HandsFilm.cs --entry HandsFilm.Run
public static class HandsFilm
{
    private const int Tile = 320;

    private static readonly (string weapon, string carry, string[] clips)[] Weapons =
    {
        ("katana", "katana", new[] { "attack_light_1" }),
        ("fists", "", new[] { "attack_fist_1" }),
        ("gsword", "gsword", new[] { "attack_gsword_1" }),
        ("scythe", "scythe", new[] { "attack_scythe_1" }),
        ("spear", "spear", new[] { "attack_spear_1" }),
        ("hammer", "hammer", new[] { "attack_hammer_1" }),
        ("knife", "knife", new[] { "attack_knife_1" }),
        ("bow", "bow", new[] { "attack_bow_1" }),
        ("sickle", "sickle", new[] { "attack_sickle_1" }),
        ("arm", "arm", new[] { "attack_arm_1" }),
    };

    public static string Run()
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerBuilder.PlayerModelPath);
        string folder = "review/gifs/hands/frames";
        Directory.CreateDirectory(folder);
        var written = new List<string>();
        var clips = PlayerAnimations.All().ToDictionary(c => c.Name);
        using (var rig = new ForgeRig(asset))
        {
            PlayerBuilder.Dress(rig.Model);
            var rack = WeaponBuilder.Hang(rig.Model, asset);
            rack.Tint(false, Color.white, 0f);
            var grip = rig.Model.GetComponent<HandGrip>();
            var scene = EditorSceneManager.NewPreviewScene();
            var cameraObject = new GameObject("Hands Camera");
            var lightObject = new GameObject("Hands Light");
            var fill = new GameObject("Hands Fill");
            var rt = RenderTexture.GetTemporary(Tile, Tile, 24, RenderTextureFormat.ARGB32);
            var previous = RenderTexture.active;
            try
            {
                foreach (var thing in new[] { cameraObject, lightObject, fill, rig.Model })
                    UnityEngine.SceneManagement.SceneManager.MoveGameObjectToScene(thing, scene);
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
                camera.nearClipPlane = 0.02f;
                camera.targetTexture = rt;
                var handR = rig["hand.R"].Transform;
                var handL = rig["hand.L"].Transform;

                foreach (var (weapon, carry, names) in Weapons)
                {
                    rack.Show(carry == "katana" || carry == "" ? null : carry, true);
                    foreach (var t in rig.Model.GetComponentsInChildren<Transform>(true))
                    {
                        if (t.name == "katana_drawn") t.gameObject.SetActive(carry == "katana");
                        else if (t.name == "katana_hilt" || t.name == "katana_sheath") t.gameObject.SetActive(false);
                    }
                    grip.CarryWithoutPlayer = carry;
                    var clip = clips[names[0]];
                    int frames = Mathf.Max(8, Mathf.RoundToInt(clip.Seconds * 40f));
                    string into = $"{folder}/{weapon}";
                    Directory.CreateDirectory(into);
                    foreach (var old in Directory.GetFiles(into, "*.png")) File.Delete(old);
                    bool leftHolds = carry == "bow";
                    var sheet = new Texture2D(Tile * 3, Tile, TextureFormat.RGB24, false);
                    for (int f = 0; f <= frames; f++)
                    {
                        clip.Pose(rig, f / (float)frames);
                        grip.Apply(1f);
                        var chest = rig["chest"].Transform.position;
                        for (int view = 0; view < 3; view++)
                        {
                            Vector3 focus, from;
                            float distance;
                            if (view == 2)
                            {
                                focus = chest + Vector3.down * 0.2f;
                                from = Quaternion.Euler(30f, 200f, 0f) * Vector3.back;
                                distance = 3.2f;
                                camera.fieldOfView = 40f;
                            }
                            else
                            {
                                var hand = leftHolds ? handL : handR;
                                focus = hand.TransformPoint(new Vector3(leftHolds ? 0.028f : -0.028f, 0.096f, -0.02f));
                                Vector3 outward = focus - chest;
                                outward.y = 0f;
                                from = view == 1 ? (Quaternion.Euler(35f, 180f, 0f) * Vector3.back) : (outward.normalized + Vector3.up * 0.35f).normalized;
                                distance = 0.95f;
                                camera.fieldOfView = 30f;
                            }
                            cameraObject.transform.position = focus + from * distance;
                            cameraObject.transform.LookAt(focus);
                            camera.Render();
                            RenderTexture.active = rt;
                            sheet.ReadPixels(new Rect(0, 0, Tile, Tile), view * Tile, 0);
                        }
                        sheet.Apply();
                        File.WriteAllBytes($"{into}/{f:000}.png", sheet.EncodeToPNG());
                    }
                    Object.DestroyImmediate(sheet);
                    written.Add($"{weapon}: {names[0]} {frames + 1} frames R={grip.PoseOf(0)} L={grip.PoseOf(1)}");
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
        return string.Join("\n", written);
    }
}

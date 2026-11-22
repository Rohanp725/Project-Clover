using System.Collections.Generic;
using System.IO;
using System.Linq;
using Clover.EditorTools;
using Clover.EditorTools.Forge;
using Clover.Player;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

// Edit-mode close-ups of the new hands (fingers, 2026-11-22) holding every weapon, posed by the Forge as baked and then
// gripped as the game grips them (HandGrip.Apply). One sheet per weapon: a row per clip moment; columns: the right hand
// from outside, the right hand from the front and above, the left hand from outside, and the whole body from 3.2 m.
// Written to review/shots/current/hands/held_<weapon>.png.
//   unity command script run -- --file AgentScripts/HandsShots.cs --entry HandsShots.Run
public static class HandsShots
{
    private const int Tile = 320;

    private static readonly (string weapon, string carry, string[] clips)[] Weapons =
    {
        ("katana", "katana", new[] { "attack_light_1", "attack_heavy", "guard" }),
        ("fists", "", new[] { "idle_fist", "attack_fist_1" }),
        ("gsword", "gsword", new[] { "idle_gsword", "attack_gsword_1" }),
        ("scythe", "scythe", new[] { "idle_scythe", "attack_scythe_1" }),
        ("spear", "spear", new[] { "idle_spear", "attack_spear_1" }),
        ("hammer", "hammer", new[] { "idle_hammer", "attack_hammer_1" }),
        ("knife", "knife", new[] { "idle_knife", "attack_knife_1", "attack_knife_finisher" }),
        ("bow", "bow", new[] { "idle_bow", "attack_bow_1" }),
        ("sickle", "sickle", new[] { "idle_sickle", "attack_sickle_1" }),
        ("arm", "arm", new[] { "idle_arm", "attack_arm_1" }),
    };

    public static string Run()
    {
        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(PlayerBuilder.PlayerModelPath);
        string folder = "review/shots/current/hands";
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
                    var moments = new List<(string clip, float at)>();
                    foreach (string name in names)
                    {
                        if (!clips.TryGetValue(name, out var clip)) continue;
                        moments.Add((name, name.StartsWith("idle") || name == "guard" ? 0.3f : 0.35f));
                        if (!name.StartsWith("idle") && name != "guard") moments.Add((name, 0.6f));
                    }
                    var sheet = new Texture2D(Tile * 4, Tile * moments.Count, TextureFormat.RGB24, false);
                    for (int m = 0; m < moments.Count; m++)
                    {
                        clips[moments[m].clip].Pose(rig, moments[m].at);
                        grip.Apply(1f);
                        var chest = rig["chest"].Transform.position;
                        for (int view = 0; view < 4; view++)
                        {
                            Vector3 focus, from;
                            float distance;
                            if (view == 3)
                            {
                                focus = chest + Vector3.down * 0.2f;
                                from = (Quaternion.Euler(30f, 200f, 0f) * Vector3.back);
                                distance = 3.2f;
                                camera.fieldOfView = 40f;
                            }
                            else
                            {
                                var hand = view < 2 ? handR : handL;
                                focus = hand.TransformPoint(new Vector3(view < 2 ? -0.028f : 0.028f, 0.096f, -0.02f));
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
                            sheet.ReadPixels(new Rect(0, 0, Tile, Tile), view * Tile, (moments.Count - 1 - m) * Tile);
                        }
                    }
                    sheet.Apply();
                    string path = $"{folder}/held_{weapon}.png";
                    File.WriteAllBytes(path, sheet.EncodeToPNG());
                    Object.DestroyImmediate(sheet);
                    written.Add($"{weapon}: {string.Join(" ", moments.Select(x => x.clip + "@" + x.at))} R={grip.PoseOf(0)} L={grip.PoseOf(1)}");
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

using System.Collections.Generic;
using System.Linq;
using Clover.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// Builds a god's godly form into the open scene: the three modelled pieces (head, body, and a hand used twice,
    /// once mirrored), what can be struck on each, the marks its attacks draw on the floor, and the ropes of lightning
    /// that hold its hands to it. It is left under the cloud, unseen, until the scene's director raises it.
    /// </summary>
    public static class ColossusBuilder
    {
        private const string Folder = BuildKit.GeneratedRoot + "/Enemies";
        private const string ModelPath = "Assets/_Project/Art/Models/Characters/boss_storm.fbx";

        // The storm itself: the swordsman's storm-grey skin gone darker and bluer, his gold and red, and light in the eyes and the scar.
        private static readonly (string name, Color colour)[] Palette =
        {
            ("skin", new Color(0.4f, 0.46f, 0.66f)),
            ("skin_dark", new Color(0.24f, 0.27f, 0.46f)),
            ("eye", new Color(0.8f, 0.95f, 1f)),
            ("mouth", new Color(0.05f, 0.04f, 0.1f)),
            ("tooth", new Color(0.92f, 0.88f, 0.76f)),
            ("gold", new Color(0.98f, 0.72f, 0.16f)),
            ("hair", new Color(0.93f, 0.94f, 1f)),
            ("mark", new Color(0.7f, 0.9f, 1f)),
            ("red", new Color(0.82f, 0.12f, 0.16f)),
        };

        public static Colossus Build(Transform target, float floorRadius, Material marker)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (asset == null)
            {
                Debug.LogError($"[Clover.Build] No model at {ModelPath}; the godly form was not built.");
                return null;
            }
            var root = new GameObject("Colossus");
            var model = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            var head = BuildKit.FindDeep(model.transform, "storm_head");
            var body = BuildKit.FindDeep(model.transform, "storm_body");
            var rightHand = BuildKit.FindDeep(model.transform, "storm_hand");
            if (head == null || body == null || rightHand == null)
            {
                Debug.LogError("[Clover.Build] The godly form's model lacks storm_head, storm_body or storm_hand.");
                Object.DestroyImmediate(model);
                return null;
            }
            head.SetParent(root.transform, true);
            body.SetParent(root.transform, true);
            rightHand.SetParent(root.transform, true);
            rightHand.name = "Right Hand";
            head.name = "Head";
            body.name = "Body";
            var leftHand = Object.Instantiate(rightHand.gameObject, root.transform).transform;
            leftHand.name = "Left Hand";
            Object.DestroyImmediate(model);

            var materials = Materials();
            var renderers = new List<Renderer>();
            foreach (var piece in new[] { head, body, leftHand, rightHand })
            {
                foreach (var renderer in piece.GetComponentsInChildren<Renderer>(true))
                {
                    renderer.sharedMaterials = renderer.sharedMaterials.Select(imported => ToonFor(imported, materials)).ToArray();
                    renderers.Add(renderer);
                }
            }
            // The body is scenery: nothing strikes it, and its shadow would lie across half the floor.
            foreach (var renderer in body.GetComponentsInChildren<Renderer>(true))
                renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            var colossus = root.AddComponent<Colossus>();
            colossus.FloorRadius = floorRadius;

            var skull = head.gameObject.AddComponent<SphereCollider>();
            skull.center = new Vector3(0f, 2.5f, 0f);
            skull.radius = 2.5f;
            var headPart = head.gameObject.AddComponent<ColossusPart>();
            headPart.Configure(colossus, skull, true, colossus.HeadDamage);
            var leftPart = Hand(leftHand, colossus, true);
            var rightPart = Hand(rightHand, colossus, false);

            var marks = BuildKit.Group("Marks", root.transform);
            var slamOutline = WorldBars.Marker("Slam Outline", marks, 0f, marker);
            var slamFill = WorldBars.Marker("Slam Fill", marks, 0f, marker);
            var sweepOutline = WorldBars.Marker("Sweep Outline", marks, 0f, marker);
            var sweepFill = WorldBars.Marker("Sweep Fill", marks, 0f, marker);
            const int Bolts = 3;
            var boltOutlines = new MeshFilter[Bolts];
            var boltFills = new MeshFilter[Bolts];
            for (int i = 0; i < Bolts; i++)
            {
                boltOutlines[i] = WorldBars.Marker("Bolt Outline " + (i + 1), marks, 0f, marker);
                boltFills[i] = WorldBars.Marker("Bolt Fill " + (i + 1), marks, 0f, marker);
            }

            var rope = BuildKit.Glow(Folder, "m_storm_tether", new Color(2.2f, 3.4f, 7f, 1f), 3, true, 1f);
            var tethers = new LineRenderer[2];
            for (int i = 0; i < 2; i++)
            {
                var lineObject = new GameObject(i == 0 ? "Left Tether" : "Right Tether");
                lineObject.transform.SetParent(root.transform, false);
                var line = lineObject.AddComponent<LineRenderer>();
                line.useWorldSpace = true;
                line.sharedMaterial = rope;
                line.widthMultiplier = 0.42f;
                line.numCornerVertices = 2;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                line.receiveShadows = false;
                tethers[i] = line;
            }

            colossus.Configure(target, head, body, leftHand, rightHand, headPart, leftPart, rightPart, renderers.ToArray(),
                slamOutline, slamFill, sweepOutline, sweepFill, boltOutlines, boltFills, tethers);
            return colossus;
        }

        private static ColossusPart Hand(Transform hand, Colossus colossus, bool mirrored)
        {
            // The left hand is the right one turned inside out, and a box cannot be: its box sits on a child turned back again.
            var reach = BuildKit.Group("Reach", hand);
            reach.localScale = new Vector3(mirrored ? -1f : 1f, 1f, 1f);
            var palm = reach.gameObject.AddComponent<BoxCollider>();
            palm.center = new Vector3(0f, 0.8f, 0f);
            palm.size = new Vector3(3.6f, 1.6f, 6.4f);
            var part = hand.gameObject.AddComponent<ColossusPart>();
            part.Configure(colossus, palm, false, colossus.HandDamage);
            return part;
        }

        private static Material ToonFor(Material imported, Dictionary<string, Material> materials)
        {
            string name = imported != null ? imported.name.Replace("m_storm_", "") : "";
            if (materials.TryGetValue(name, out var toon)) return toon;
            Debug.LogWarning($"[Clover.Build] The godly form uses a material '{name}' that its palette does not know.");
            return imported;
        }

        private static Dictionary<string, Material> Materials()
        {
            var materials = new Dictionary<string, Material>();
            foreach (var (name, colour) in Palette)
            {
                var material = BuildKit.Toon(Folder, $"m_storm_{name}", colour, true);
                // At this size the character's line would be a hair; it is drawn far thicker.
                material.Ink(0.05f, BuildKit.CharacterInk);
                if (name == "eye") material.Lit(new Color(0.8f, 1f, 1.6f), 0.5f, new Color(2.2f, 4f, 7f));
                else if (name == "mark") material.Lit(new Color(0.8f, 1f, 1.6f), 0.5f, new Color(1.5f, 3f, 6f));
                else if (name == "gold") material.Lit(new Color(1.2f, 0.9f, 0.4f), 0.4f, new Color(0.25f, 0.14f, 0.02f));
                else material.Lit(new Color(0.55f, 0.7f, 1.2f), 0.3f);
                materials[name] = material;
            }
            return materials;
        }
    }
}

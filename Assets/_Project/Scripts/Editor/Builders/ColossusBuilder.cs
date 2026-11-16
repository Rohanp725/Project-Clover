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
            // The middle of the head itself, which in the model's own axes stands along +Z from the neck.
            skull.center = new Vector3(0f, 0.3f, 3f);
            skull.radius = 2.6f;
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

            var breathOutline = WorldBars.Marker("Breath Outline", marks, 0f, marker);
            var breathFill = WorldBars.Marker("Breath Fill", marks, 0f, marker);
            var biteOutline = WorldBars.Marker("Bite Outline", marks, 0f, marker);
            var biteFill = WorldBars.Marker("Bite Fill", marks, 0f, marker);
            var clapOutlines = new MeshFilter[2];
            var clapFills = new MeshFilter[2];
            for (int i = 0; i < 2; i++)
            {
                clapOutlines[i] = WorldBars.Marker(i == 0 ? "Clap Outline West" : "Clap Outline East", marks, 0f, marker);
                clapFills[i] = WorldBars.Marker(i == 0 ? "Clap Fill West" : "Clap Fill East", marks, 0f, marker);
            }
            var roarRing = WorldBars.Marker("Roar Ring", marks, 0f, marker);
            // As many pools as bolts can be burning at once: six bolts half a second apart, each pool two seconds.
            const int Pools = 6;
            var pools = new MeshFilter[Pools];
            for (int i = 0; i < Pools; i++) pools[i] = WorldBars.Marker("Pool " + (i + 1), marks, 0f, marker);

            // The ropes are the same stuff as the sky's bolts, and as thin: three strands to a hand, left hand first.
            var rope = BuildKit.Glow(Folder, "m_storm_tether", new Color(1.7f, 2.4f, 3.9f, 1f), 3, true, 1f);
            var tethers = new LineRenderer[6];
            for (int i = 0; i < tethers.Length; i++)
                tethers[i] = Bolt((i < 3 ? "Left Tether " : "Right Tether ") + (i % 3 + 1), root.transform, rope, 0.075f);
            // The breath: one thick bolt down the lane and two thin ones wandering beside it.
            var breath = BuildKit.Glow(Folder, "m_storm_breath", new Color(2.2f, 3f, 4.6f, 1f), 3, true, 1f);
            var beam = new LineRenderer[3];
            for (int i = 0; i < beam.Length; i++)
                beam[i] = Bolt("Breath " + (i + 1), root.transform, breath, 0.5f);

            colossus.Configure(target, head, body, leftHand, rightHand, headPart, leftPart, rightPart, renderers.ToArray(),
                slamOutline, slamFill, sweepOutline, sweepFill, boltOutlines, boltFills, tethers);
            colossus.ConfigureSetPiece(breathOutline, breathFill, biteOutline, biteFill, clapOutlines, clapFills, roarRing, pools, beam);

            // What is made of light on him, so the fight can turn it up and down: the eyes, and the scar and marks.
            var glowing = new List<Renderer>();
            var glowIndex = new List<int>();
            var glowIsEye = new List<bool>();
            foreach (var renderer in renderers)
            {
                var shared = renderer.sharedMaterials;
                for (int i = 0; i < shared.Length; i++)
                {
                    bool eye = shared[i] == materials["eye"];
                    if (!eye && shared[i] != materials["mark"]) continue;
                    glowing.Add(renderer);
                    glowIndex.Add(i);
                    glowIsEye.Add(eye);
                }
            }
            colossus.ConfigureGlow(glowing.ToArray(), glowIndex.ToArray(), glowIsEye.ToArray());
            // Over where the head comes to lie when its balance breaks; the camera here looks the same way as everywhere.
            var towardCamera = Camera.main != null ? Camera.main.transform.rotation : Quaternion.Euler(55f, 0f, 0f);
            colossus.ConfigureFeedback(WorldBars.FinishMark(root.transform, new Vector3(0f, 1.7f, floorRadius - 10f), 5f, towardCamera));
            return colossus;
        }

        private static LineRenderer Bolt(string name, Transform parent, Material material, float width)
        {
            var lineObject = new GameObject(name);
            lineObject.transform.SetParent(parent, false);
            var line = lineObject.AddComponent<LineRenderer>();
            line.useWorldSpace = true;
            line.sharedMaterial = material;
            line.widthMultiplier = width;
            // Hard corners: a bolt is kinked, not curved.
            line.numCornerVertices = 0;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            return line;
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

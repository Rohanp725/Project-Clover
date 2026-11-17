using System.Collections.Generic;
using System.Linq;
using Clover.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// Builds the storm dragon into the open scene: the modelled pieces (head with its hinged jaw, the neck in its
    /// bank of cloud, a forefoot used twice, an arch of the body used many times round the summit, the short tail far
    /// off and the long tail that comes onto the floor), what can be struck on each, the marks its attacks draw, its
    /// orbs, and the lines its beam is drawn with. It is left under the cloud, unseen, until the director raises it.
    /// </summary>
    public static class DragonBuilder
    {
        private const string Folder = BuildKit.GeneratedRoot + "/Enemies";
        private const string ModelPath = "Assets/_Project/Art/Models/Characters/boss_dragon.fbx";

        // Storm blue-grey hide and a pale belly, gold horn and talon, white mane, cloud the colour of the summit's own,
        // and light in the eyes, on the brow and down every flank.
        private static readonly (string name, Color colour)[] Palette =
        {
            ("scale", new Color(0.17f, 0.44f, 0.66f)),
            ("belly", new Color(0.82f, 0.86f, 0.9f)),
            ("horn", new Color(0.98f, 0.72f, 0.16f)),
            ("mane", new Color(0.93f, 0.94f, 1f)),
            ("eye", new Color(0.85f, 0.97f, 1f)),
            ("tooth", new Color(0.92f, 0.88f, 0.76f)),
            ("mark", new Color(0.7f, 0.9f, 1f)),
            ("cloud", new Color(0.52f, 0.53f, 0.76f)),
            ("mouth", new Color(0.5f, 0.1f, 0.16f)),
        };

        // The arches round the summit: degrees clockwise from north, metres out from the middle, size, and degrees turned
        // off the round. None stands due north, where the neck is, and those the camera has behind it are smaller.
        private static readonly (float angle, float radius, float scale, float turn)[] Coils =
        {
            (27f, 28f, 1.5f, 10f),
            (42f, 21.2f, 1.2f, 6f),
            (70f, 20.8f, 1.15f, -8f),
            (99f, 20.8f, 1.1f, 6f),
            (130f, 21.5f, 0.85f, -12f),
            (230f, 21.5f, 0.85f, 12f),
            (261f, 20.8f, 1.1f, -6f),
            (290f, 20.8f, 1.15f, 8f),
            (318f, 21.2f, 1.2f, -6f),
            (333f, 28f, 1.5f, -10f),
        };

        public static StormDragon Build(Transform target, float floorRadius, Material marker)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (asset == null)
            {
                Debug.LogError($"[Clover.Build] No model at {ModelPath}; the storm dragon was not built.");
                return null;
            }
            var root = new GameObject("Storm Dragon");
            var model = (GameObject)PrefabUtility.InstantiatePrefab(asset);
            PrefabUtility.UnpackPrefabInstance(model, PrefabUnpackMode.Completely, InteractionMode.AutomatedAction);
            var materials = Materials();
            var renderers = new List<Renderer>();

            // The modelled pieces, or boxes where the model lacks one, so the fight can still be built and seen to be wrong.
            var head = Piece(model, "dragon_head", "Head", root.transform, materials, renderers, new Vector3(5f, 3f, 7f));
            var jaw = Piece(model, "dragon_jaw", "Jaw", head, materials, renderers, new Vector3(3.6f, 1f, 4.5f));
            // The hinge, which the model's notes give in the head's own space.
            jaw.localPosition = new Vector3(0f, 1.45f, -2f);
            var body = Piece(model, "dragon_body", "Neck", root.transform, materials, renderers, new Vector3(16f, 12f, 8f));
            var rightClaw = Piece(model, "dragon_claw", "Right Forefoot", root.transform, materials, renderers, new Vector3(2.2f, 1.4f, 4.4f));
            var leftClaw = Copy(rightClaw, "Left Forefoot", root.transform, renderers);
            var shortTail = Piece(model, "dragon_tail", "Tail In The Cloud", root.transform, materials, renderers, new Vector3(2f, 5.5f, 2f));
            var whip = Piece(model, "dragon_tail_whip", "Tail", root.transform, materials, renderers, new Vector3(2.4f, 2f, 26f));
            var firstCoil = Piece(model, "dragon_coil", "Arch 1", root.transform, materials, renderers, new Vector3(12f, 6f, 2.4f));
            var coils = new Transform[Coils.Length];
            coils[0] = firstCoil;
            for (int i = 1; i < coils.Length; i++) coils[i] = Copy(firstCoil, "Arch " + (i + 1), root.transform, renderers);
            Object.DestroyImmediate(model);

            // The neck, the arches and the far tail are scenery: nothing strikes them, and their shadows would lie across the floor.
            foreach (var scenery in coils.Append(body).Append(shortTail))
                foreach (var renderer in scenery.GetComponentsInChildren<Renderer>(true))
                    renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;

            var dragon = root.AddComponent<StormDragon>();
            dragon.FloorRadius = floorRadius;

            var skull = head.gameObject.AddComponent<SphereCollider>();
            skull.center = new Vector3(0f, 1.7f, 0f);
            skull.radius = 2.7f;
            var headPart = head.gameObject.AddComponent<ColossusPart>();
            headPart.Configure(dragon, skull, true, dragon.HeadDamage);
            // Half as large again as it was modelled: it has to be a big thing on the floor, and long enough to cross it.
            whip.localScale = Vector3.one * 1.5f;
            // The tail bends: its bones are driven by a spine, and what a blow lands on is capsules laid along them.
            var tailChain = new List<Transform>();
            for (int i = 0; i < 64; i++)
            {
                var bone = BuildKit.FindDeep(whip, $"tail_{i:00}");
                if (bone == null) break;
                tailChain.Add(bone);
            }
            if (tailChain.Count < 2) Debug.LogError("[Clover.Build] The dragon's tail has no chain of bones tail_00, tail_01, ...; it will not move.");
            const int TailReaches = 8;
            var tailReaches = new CapsuleCollider[TailReaches];
            for (int i = 0; i < TailReaches; i++)
            {
                var reach = BuildKit.Group("Reach " + (i + 1), whip).gameObject.AddComponent<CapsuleCollider>();
                // Along its own +Z, which the bones' writer lays along the chain.
                reach.direction = 2;
                reach.radius = Mathf.Lerp(1.1f, 0.7f, i / (float)(TailReaches - 1));
                reach.height = 4.4f;
                reach.center = new Vector3(0f, 0.9f, 0f);
                tailReaches[i] = reach;
            }
            whip.gameObject.AddComponent<SpineBones>().Configure(tailChain.ToArray(), tailReaches);
            var tailPart = whip.gameObject.AddComponent<ColossusPart>();
            tailPart.Configure(dragon, tailReaches[0], false, dragon.TailDamage);
            tailPart.ConfigureMore(tailReaches.Skip(1).Cast<Collider>().ToArray());
            var leftPart = Forefoot(leftClaw, dragon, true);
            var rightPart = Forefoot(rightClaw, dragon, false);

            var marks = BuildKit.Group("Marks", root.transform);
            var orbGlow = BuildKit.Toon(Folder, "m_dragon_orb", new Color(0.75f, 0.9f, 1f), false, null, 0f, 0.02f).Lit(new Color(0.8f, 1f, 1.6f), 0.6f, new Color(2.4f, 4.2f, 7f));
            var orbs = new Transform[3];
            for (int i = 0; i < orbs.Length; i++)
            {
                var orb = BuildKit.Shape(PrimitiveType.Sphere, "Orb " + (i + 1), root.transform, Vector3.down * 30f, Vector3.one * 1.2f, orbGlow);
                orb.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                orb.enabled = false;
                orbs[i] = orb.transform;
            }
            // The beam: one thick straight bolt and two thin ones kinking round it; the breath draws its three forks with the same lines.
            var light = BuildKit.Glow(Folder, "m_dragon_beam", new Color(2.4f, 3.2f, 4.8f, 1f), 3, true, 1f);
            var beam = new LineRenderer[3];
            for (int i = 0; i < beam.Length; i++) beam[i] = Bolt("Beam " + (i + 1), root.transform, light, 0.5f);

            dragon.Configure(target, head, jaw, body, leftClaw, rightClaw, shortTail, whip, headPart, tailPart, leftPart, rightPart, renderers.ToArray());
            dragon.ConfigureCoils(coils, Coils.Select(coil => coil.angle).ToArray(), Coils.Select(coil => coil.radius).ToArray(),
                Coils.Select(coil => coil.scale).ToArray(), Coils.Select(coil => coil.turn).ToArray());
            // Far round to the north-west, where the camera sees it over the rim.
            dragon.ConfigureTail(new Vector3(-15.5f, 0f, 17.5f), 150f);
            shortTail.localScale = Vector3.one * 1.3f;
            // Sixteen circles is the most that are ever drawn at once: the outer ring of sigils and the one inside it.
            dragon.ConfigureMarks(Pairs("Beam", 2, marks, marker), Pairs("Breath", 3, marks, marker), Pairs("Bite", 1, marks, marker),
                Pairs("Fall", 16, marks, marker), Pairs("Orb", 3, marks, marker), Pairs("Storm", 2, marks, marker),
                Pairs("Slither", 12, marks, marker), Pairs("Tail Slam", 1, marks, marker), WorldBars.Marker("Roar Ring", marks, 0f, marker), orbs, beam);

            // What is made of light on him, so the fight can turn it up and down: the eyes, and the marks on brow and flank.
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
            dragon.ConfigureGlow(glowing.ToArray(), glowIndex.ToArray(), glowIsEye.ToArray());
            // Over where the head comes to lie when its balance breaks; the camera here looks the same way as everywhere.
            var towardCamera = Camera.main != null ? Camera.main.transform.rotation : Quaternion.Euler(55f, 0f, 0f);
            dragon.ConfigureFeedback(WorldBars.FinishMark(root.transform, new Vector3(0f, 4.2f, floorRadius - 9.5f), 5f, towardCamera));
            return dragon;
        }

        // One piece of the model under a holder of its own. The holder is what the fight moves: it faces along +Z with
        // +Y up, and the mesh keeps under it the turn the import gave it, which is what stands it that way.
        private static Transform Piece(GameObject model, string modelName, string name, Transform parent, Dictionary<string, Material> materials, List<Renderer> renderers, Vector3 standIn)
        {
            var holder = BuildKit.Group(name, parent);
            var mesh = BuildKit.FindDeep(model.transform, modelName);
            // A piece that bends comes as two things side by side, its skinned mesh and the rig of bones that mesh hangs on: both are moved in.
            var rig = BuildKit.FindDeep(model.transform, modelName + "_rig");
            if (rig != null)
            {
                var rigTurn = rig.localRotation;
                rig.SetParent(holder, false);
                rig.localPosition = Vector3.zero;
                rig.localRotation = rigTurn;
                rig.localScale = Vector3.one;
                rig.name = "Rig";
            }
            if (mesh == null)
            {
                Debug.LogError($"[Clover.Build] The dragon's model lacks {modelName}; a box stands in for it.");
                renderers.Add(BuildKit.Shape(PrimitiveType.Cube, "Stand In", holder, new Vector3(0f, standIn.y * 0.5f, 0f), standIn, materials["scale"]));
                return holder;
            }
            var turn = mesh.localRotation;
            mesh.SetParent(holder, false);
            mesh.localPosition = Vector3.zero;
            mesh.localRotation = turn;
            mesh.localScale = Vector3.one;
            mesh.name = "Model";
            foreach (var renderer in mesh.GetComponentsInChildren<Renderer>(true))
            {
                renderer.sharedMaterials = renderer.sharedMaterials.Select(imported => ToonFor(imported, materials)).ToArray();
                // A bent tail is nowhere near where its mesh was made: its bounds follow its bones.
                if (renderer is SkinnedMeshRenderer skinned) skinned.updateWhenOffscreen = true;
                renderers.Add(renderer);
            }
            return holder;
        }

        private static Transform Copy(Transform piece, string name, Transform parent, List<Renderer> renderers)
        {
            var copy = Object.Instantiate(piece.gameObject, parent).transform;
            copy.name = name;
            renderers.AddRange(copy.GetComponentsInChildren<Renderer>(true));
            return copy;
        }

        private static MeshFilter[] Pairs(string name, int count, Transform marks, Material marker)
        {
            var pairs = new MeshFilter[count * 2];
            for (int i = 0; i < count; i++)
            {
                pairs[i * 2] = WorldBars.Marker($"{name} Outline {i + 1}", marks, 0f, marker);
                pairs[i * 2 + 1] = WorldBars.Marker($"{name} Fill {i + 1}", marks, 0f, marker);
            }
            return pairs;
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

        private static ColossusPart Forefoot(Transform claw, StormDragon dragon, bool mirrored)
        {
            // The left foot is the right one turned inside out, and a box cannot be: its box sits on a child that is not.
            var reach = BuildKit.Group("Reach", claw);
            reach.localScale = new Vector3(mirrored ? -1f : 1f, 1f, 1f);
            var foot = reach.gameObject.AddComponent<BoxCollider>();
            foot.center = new Vector3(0f, 0.75f, 0f);
            foot.size = new Vector3(2.2f, 1.5f, 4.5f);
            var part = claw.gameObject.AddComponent<ColossusPart>();
            part.Configure(dragon, foot, false, dragon.ClawDamage);
            return part;
        }

        private static Material ToonFor(Material imported, Dictionary<string, Material> materials)
        {
            string name = imported != null ? imported.name.Replace("m_dragon_", "") : "";
            if (materials.TryGetValue(name, out var toon)) return toon;
            Debug.LogWarning($"[Clover.Build] The dragon uses a material '{name}' that its palette does not know.");
            return imported;
        }

        private static Dictionary<string, Material> Materials()
        {
            var materials = new Dictionary<string, Material>();
            foreach (var (name, colour) in Palette)
            {
                var material = BuildKit.Toon(Folder, $"m_dragon_{name}", colour, true);
                // At this size the line of a character would be a hair; it is drawn far thicker. Cloud keeps a thin one.
                material.Ink(name == "cloud" ? 0.02f : 0.05f, BuildKit.CharacterInk);
                if (name == "eye") material.Lit(new Color(0.8f, 1f, 1.6f), 0.5f, new Color(2.2f, 4f, 7f));
                else if (name == "mark") material.Lit(new Color(0.8f, 1f, 1.6f), 0.5f, new Color(1.5f, 3f, 6f));
                else if (name == "horn") material.Lit(new Color(1.2f, 0.9f, 0.4f), 0.4f, new Color(0.25f, 0.14f, 0.02f));
                else if (name == "cloud") material.Lit(new Color(0.3f, 0.34f, 0.6f), 0.5f);
                else material.Lit(new Color(0.55f, 0.7f, 1.2f), 0.3f);
                materials[name] = material;
            }
            return materials;
        }
    }
}

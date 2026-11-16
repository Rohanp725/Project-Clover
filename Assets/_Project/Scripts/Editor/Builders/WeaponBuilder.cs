using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Clover.EditorTools.Forge;
using Clover.Player;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// The weapons as things to be seen: one small prefab each under Generated/Weapons, hung on sockets the character's
    /// skeleton is given (right hand, left hand, back, hip), with the toon materials named after the model's slots.
    /// The slots "accent" and "inlay" are the ones the held element re-colours in play (<see cref="WeaponRack"/>).
    ///
    /// A weapon's model is the approved file under Art/Models/Weapons when there is one. Until the user has approved
    /// the Blender models waiting in staging, each weapon is a stand-in made here from boxes to the same size, the same
    /// axes (grip at the origin, business end along +Z, edge toward -Y) and the same slot names, so the approved
    /// file takes its place with no other change. Clover > Weapons > Intake Staged Models brings the staged files in
    /// and writes their manifest rows; it is for the user's word, and has not been run.
    ///
    /// To add a weapon: one <see cref="Spec"/> in <see cref="Specs"/>. Nothing else here changes.
    /// </summary>
    public static class WeaponBuilder
    {
        public const string Folder = BuildKit.GeneratedRoot + "/Weapons";
        public const string ModelFolder = "Assets/_Project/Art/Models/Weapons";
        public const string StagingFolder = "staging/blender/2026-11-16_weapons";

        public class Spec
        {
            /// <summary>The name <see cref="Weapon.Carry"/> uses, and the prefab's.</summary>
            public string Carry;
            /// <summary>The staged model file, or null for a weapon with none (the Armament).</summary>
            public string ModelFile;
            public string Notes;
            /// <summary>Builds the stand-in under a root, in the model's own axes.</summary>
            public Action<Transform> StandIn;
            /// <summary>Slots whose colour is this weapon's own and not the shared one: slot name to colour as the eye sees it.</summary>
            public Dictionary<string, Color> OwnColours = new Dictionary<string, Color>();
            /// <summary>Where it rests on the body: which socket, and where in that socket (character axes at rest: x right, y up, z ahead).</summary>
            public string RestSocket = WeaponRack.Back;
            public Vector3 RestPosition;
            public Vector3 RestAlong = Vector3.down, RestUp = Vector3.right;
            /// <summary>Pieces worn on bones in place of one thing in the hand: bone name, then local position and size along the bone.</summary>
            public (string bone, Vector3 position, Vector3 size)[] Worn;
            /// <summary>How it stands on a stand: lifted this far, turned by these degrees.</summary>
            public Vector3 DisplayPosition = new Vector3(0f, 0.9f, 0f), DisplayEuler = new Vector3(-90f, 0f, 0f);
        }

        // Slot colours as the eye sees them, from the Blender notes (staging/blender/2026-11-16_weapons/notes.md).
        private static readonly Dictionary<string, Color> SlotColours = new Dictionary<string, Color>
        {
            { "blade", new Color(0.81f, 0.85f, 0.91f) }, { "edge", new Color(0.81f, 0.85f, 0.91f) },
            { "guard", new Color(0.45f, 0.48f, 0.56f) }, { "head", new Color(0.45f, 0.48f, 0.56f) },
            { "grip", new Color(0.35f, 0.32f, 0.31f) }, { "wood", new Color(0.4f, 0.33f, 0.27f) },
            { "wrap", new Color(0.81f, 0.75f, 0.61f) }, { "shaft", new Color(0.81f, 0.75f, 0.61f) },
            { "string", new Color(0.91f, 0.9f, 0.85f) },
            { "accent", new Color(0.93f, 0.29f, 0.25f) }, { "inlay", new Color(0.29f, 0.31f, 0.38f) },
        };

        public static readonly Spec[] Specs =
        {
            new Spec
            {
                Carry = "gsword", ModelFile = "gsword.fbx", StandIn = SwordStandIn,
                Notes = "Massive Sword: 518 triangles, slots blade edge grip accent inlay; origin on the grip, point along +Z, edge toward -Y",
                OwnColours = { { "blade", new Color(0.45f, 0.48f, 0.56f) } },
                // Slanted from the right shoulder to behind the left heel, flat to the back.
                RestPosition = new Vector3(0.3f, 0.3f, -0.17f), RestAlong = new Vector3(-0.42f, -0.9f, 0f), RestUp = new Vector3(0.9f, -0.42f, 0f),
                // Point down in its stand, the flat toward whoever walks up to it.
                DisplayPosition = new Vector3(0f, 2.05f, 0f), DisplayEuler = new Vector3(90f, 90f, 0f),
            },
            new Spec
            {
                Carry = "scythe", ModelFile = "scythe.fbx", StandIn = ScytheStandIn,
                Notes = "Scythe: 510 triangles, slots blade guard wood accent inlay; origin on the haft 1.15 m from the butt, blade end along +Z, crescent toward -Y",
                // Haft across the back from the left hip up past the right shoulder, the crescent behind the head pointing left.
                RestPosition = new Vector3(0.12f, 0.1f, -0.19f), RestAlong = new Vector3(0.42f, 0.9f, 0f), RestUp = new Vector3(0.9f, -0.42f, 0f),
                // Stood on its butt, the crescent out to one side and seen whole from the front.
                DisplayPosition = new Vector3(0f, 1.45f, 0f), DisplayEuler = new Vector3(-90f, 90f, 0f),
            },
            new Spec
            {
                Carry = "arm", StandIn = _ => { },
                Notes = "Full Body Armament: no model; shells on the forearms, hands and shins",
                Worn = new[]
                {
                    ("forearm.L", new Vector3(0f, 0.15f, 0f), new Vector3(0.14f, 0.27f, 0.14f)),
                    ("forearm.R", new Vector3(0f, 0.15f, 0f), new Vector3(0.14f, 0.27f, 0.14f)),
                    ("hand.L", new Vector3(0f, 0.07f, 0f), new Vector3(0.19f, 0.2f, 0.19f)),
                    ("hand.R", new Vector3(0f, 0.07f, 0f), new Vector3(0.19f, 0.2f, 0.19f)),
                    ("shin.L", new Vector3(0f, 0.25f, 0f), new Vector3(0.15f, 0.3f, 0.15f)),
                    ("shin.R", new Vector3(0f, 0.25f, 0f), new Vector3(0.15f, 0.3f, 0.15f)),
                    ("upperarm.L", new Vector3(0f, 0.03f, 0f), new Vector3(0.2f, 0.1f, 0.2f)),
                    ("upperarm.R", new Vector3(0f, 0.03f, 0f), new Vector3(0.2f, 0.1f, 0.2f)),
                },
            },
        };

        public static Spec SpecFor(string carry) => Specs.FirstOrDefault(spec => spec.Carry == carry);

        [MenuItem("Clover/Weapons/Build Weapon Prefabs")]
        public static void BuildAll()
        {
            int built = 0;
            foreach (var spec in Specs)
                if (spec.Worn == null && Prefab(spec.Carry, true) != null) built++;
            AssetDatabase.SaveAssets();
            Debug.Log($"[Clover.Build] Built {built} weapon prefabs under {Folder}.");
        }

        /// <summary>The weapon's prefab, built if it is not there (or again, when asked).</summary>
        public static GameObject Prefab(string carry, bool rebuild = false)
        {
            var spec = SpecFor(carry);
            if (spec == null || spec.Worn != null) return null;
            string folder = $"{Folder}/{carry}";
            string path = $"{folder}/{carry}.prefab";
            var existing = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (existing != null && !rebuild) return existing;

            BuildKit.EnsureFolder(folder);
            var root = new GameObject(carry);
            var approved = spec.ModelFile != null ? AssetDatabase.LoadAssetAtPath<GameObject>($"{ModelFolder}/{carry}.fbx") : null;
            if (approved != null)
            {
                // The approved model: each of its materials only carries a slot name, and is replaced by the toon one.
                var model = (GameObject)PrefabUtility.InstantiatePrefab(approved);
                model.name = "Model";
                model.transform.SetParent(root.transform, false);
                foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
                    renderer.sharedMaterials = renderer.sharedMaterials.Select(imported => SlotMaterial(spec, SlotOf(imported != null ? imported.name : ""))).ToArray();
            }
            else
            {
                spec.StandIn(root.transform);
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                    renderer.sharedMaterial = SlotMaterial(spec, SlotOf(renderer.name));
            }
            var prefab = PrefabUtility.SaveAsPrefabAsset(root, path);
            UnityEngine.Object.DestroyImmediate(root);
            return prefab;
        }

        // "blade.001", "m_weapon_blade", "Blade Tip (blade)": the slot is the known name the text ends with or carries.
        private static string SlotOf(string name)
        {
            string lower = name.ToLowerInvariant();
            foreach (string slot in SlotColours.Keys)
                if (lower.Contains(slot)) return slot;
            return "guard";
        }

        /// <summary>The toon material of a slot, the weapon's own when it has its own colour for it.</summary>
        public static Material SlotMaterial(Spec spec, string slot)
        {
            bool own = spec != null && spec.OwnColours.ContainsKey(slot);
            Color colour = own ? spec.OwnColours[slot] : SlotColours[slot];
            var material = BuildKit.Toon(Folder, own ? $"m_weapon_{spec.Carry}_{slot}" : $"m_weapon_{slot}", colour).Ink(0.028f, BuildKit.CharacterInk);
            // Steel catches the light; everything else is as muted as the world.
            if (slot == "blade" || slot == "edge") material.Lit(own ? new Color(0.5f, 0.56f, 0.72f) : new Color(0.9f, 1.05f, 1.35f), own ? 0.28f : 0.4f);
            return material;
        }

        private static Material CoatMaterial() =>
            BuildKit.Toon(Folder, "m_armament_inlay", new Color(0.72f, 0.72f, 0.78f)).Ink(0.03f, BuildKit.CharacterInk);

        /// <summary>
        /// Gives a copy of the character model its sockets and hangs every weapon on them, in the hand and at rest.
        /// Used for the player in a scene and for the model the Forge poses, so both show the same thing.
        /// </summary>
        public static WeaponRack Hang(GameObject model, GameObject modelAsset)
        {
            Quaternion handRest, handLeftRest, chestRest, hipsRest;
            Vector3 blade;
            using (var rig = new ForgeRig(modelAsset))
            {
                handRest = rig["hand.R"].RestRotation;
                handLeftRest = rig["hand.L"].RestRotation;
                chestRest = rig["chest"].RestRotation;
                hipsRest = rig["hips"].RestRotation;
                blade = rig.RestBlade;
            }
            // The weapon's +Z lies along the rest blade. Its +Y (the back of a blade; the edge is -Y) is set so that
            // a pose which points the blade straight ahead with no roll holds it edge down.
            Vector3 restUp = Quaternion.Inverse(Quaternion.FromToRotation(blade, Vector3.forward)) * Vector3.up;
            var handR = Socket(model, "hand.R", WeaponRack.HandRight, new Vector3(0f, 0.061f, 0f), Quaternion.Inverse(handRest) * Quaternion.LookRotation(blade, restUp));
            Vector3 bladeLeft = new Vector3(-blade.x, blade.y, blade.z), upLeft = new Vector3(-restUp.x, restUp.y, restUp.z);
            Socket(model, "hand.L", WeaponRack.HandLeft, new Vector3(0f, 0.061f, 0f), Quaternion.Inverse(handLeftRest) * Quaternion.LookRotation(bladeLeft, upLeft));
            var back = Socket(model, "chest", WeaponRack.Back, Vector3.zero, Quaternion.Inverse(chestRest));
            var hip = Socket(model, "hips", WeaponRack.Hip, Vector3.zero, Quaternion.Inverse(hipsRest));

            var sets = new List<WeaponRack.Set>();
            foreach (var spec in Specs)
            {
                var inHand = new List<GameObject>();
                var atRest = new List<GameObject>();
                if (spec.Worn != null)
                {
                    var coat = CoatMaterial();
                    foreach (var (bone, position, size) in spec.Worn)
                    {
                        var parent = BuildKit.FindDeep(model.transform, bone);
                        if (parent == null) continue;
                        inHand.Add(BuildKit.Shape(PrimitiveType.Cube, "armament_" + bone, parent, position, size, coat, new Vector3(0f, 45f, 0f)).gameObject);
                    }
                }
                else
                {
                    var prefab = Prefab(spec.Carry);
                    if (prefab == null || handR == null) continue;
                    inHand.Add(Place(prefab, handR, spec.Carry + "_hand", Vector3.zero, Quaternion.identity));
                    var rest = spec.RestSocket == WeaponRack.Hip ? hip : back;
                    if (rest != null) atRest.Add(Place(prefab, rest, spec.Carry + "_rest", spec.RestPosition, Quaternion.LookRotation(spec.RestAlong.normalized, spec.RestUp.normalized)));
                }
                var tinted = new List<Renderer>();
                var indices = new List<int>();
                var glow = new List<float>();
                var plain = new List<Color>();
                foreach (var part in inHand.Concat(atRest))
                {
                    foreach (var renderer in part.GetComponentsInChildren<Renderer>(true))
                    {
                        var materials = renderer.sharedMaterials;
                        for (int i = 0; i < materials.Length; i++)
                        {
                            if (materials[i] == null) continue;
                            bool inlay = materials[i].name.EndsWith("_inlay"), accent = materials[i].name.EndsWith("_accent");
                            if (!inlay && !accent) continue;
                            tinted.Add(renderer);
                            indices.Add(i);
                            glow.Add(inlay ? (spec.Worn != null ? 1.5f : 1f) : 0f);
                            plain.Add(materials[i].GetColor("_BaseColor"));
                        }
                    }
                    part.SetActive(false);
                }
                sets.Add(new WeaponRack.Set
                {
                    Carry = spec.Carry, InHand = inHand.ToArray(), AtRest = atRest.ToArray(),
                    Tinted = tinted.ToArray(), Index = indices.ToArray(), Glow = glow.ToArray(), Plain = plain.ToArray(),
                });
            }
            var rack = model.GetComponent<WeaponRack>();
            if (rack == null) rack = model.AddComponent<WeaponRack>();
            rack.Configure(sets.ToArray());
            return rack;
        }

        private static Transform Socket(GameObject model, string bone, string name, Vector3 localPosition, Quaternion localRotation)
        {
            var parent = BuildKit.FindDeep(model.transform, bone);
            if (parent == null)
            {
                Debug.LogWarning($"[Clover.Build] The character has no '{bone}' bone; weapons that hang from {name} will not be shown.");
                return null;
            }
            var existing = parent.Find(name);
            if (existing != null) UnityEngine.Object.DestroyImmediate(existing.gameObject);
            var socket = new GameObject(name).transform;
            socket.SetParent(parent, false);
            socket.localPosition = localPosition;
            socket.localRotation = localRotation;
            return socket;
        }

        private static GameObject Place(GameObject prefab, Transform socket, string name, Vector3 localPosition, Quaternion localRotation)
        {
            // A plain copy, not a prefab link: the Forge hangs these on a model that is never saved.
            var instance = UnityEngine.Object.Instantiate(prefab, socket);
            instance.name = name;
            instance.transform.localPosition = localPosition;
            instance.transform.localRotation = localRotation;
            return instance;
        }

        /// <summary>The weapon set out to be looked at, on a stand or a rack: upright, at a height that reads.</summary>
        public static GameObject Display(string carry, Transform parent)
        {
            var spec = SpecFor(carry);
            if (spec == null) return null;
            if (spec.Worn != null)
            {
                // The Armament has no object: a pair of shells on a short post stands for it.
                var pair = BuildKit.Group(carry + " display", parent).gameObject;
                var coat = CoatMaterial();
                var post = SlotMaterial(null, "wood");
                BuildKit.Shape(PrimitiveType.Cube, "Post", pair.transform, new Vector3(0f, 0.75f, 0f), new Vector3(0.12f, 1f, 0.12f), post);
                BuildKit.Shape(PrimitiveType.Cube, "Arm", pair.transform, new Vector3(0f, 1.25f, 0f), new Vector3(0.9f, 0.1f, 0.1f), post);
                foreach (float side in new[] { -1f, 1f })
                {
                    BuildKit.Shape(PrimitiveType.Cube, "Sleeve", pair.transform, new Vector3(side * 0.4f, 1.05f, 0f), new Vector3(0.2f, 0.42f, 0.2f), coat, new Vector3(0f, 45f, 0f));
                    BuildKit.Shape(PrimitiveType.Cube, "Gauntlet", pair.transform, new Vector3(side * 0.4f, 0.72f, 0f), new Vector3(0.27f, 0.27f, 0.27f), coat, new Vector3(0f, 45f, 0f));
                }
                return pair;
            }
            var prefab = Prefab(carry);
            if (prefab == null) return null;
            var shown = (GameObject)PrefabUtility.InstantiatePrefab(prefab, parent);
            shown.name = carry + " display";
            shown.transform.localPosition = spec.DisplayPosition;
            shown.transform.localRotation = Quaternion.Euler(spec.DisplayEuler);
            return shown;
        }

        /// <summary>
        /// The asset-intake procedure for the staged weapon models: copies each from staging to Art/Models/Weapons,
        /// writes its manifest row with approved_by "pending", and rebuilds the prefabs from them. For the user's
        /// word only: nothing enters Art before they have looked at the staging renders.
        /// </summary>
        [MenuItem("Clover/Weapons/Intake Staged Models")]
        public static void IntakeStaged()
        {
            BuildKit.EnsureFolder(ModelFolder);
            const string manifest = "Assets/_Project/ASSETS.csv";
            string rows = File.ReadAllText(manifest);
            string ending = rows.Contains("\r\n") ? "\r\n" : "\n";
            int taken = 0;
            foreach (var spec in Specs)
            {
                if (spec.ModelFile == null) continue;
                string source = $"{StagingFolder}/{spec.ModelFile}", target = $"{ModelFolder}/{spec.Carry}.fbx";
                if (!File.Exists(source))
                {
                    Debug.LogWarning($"[Clover.Build] No staged model at {source}.");
                    continue;
                }
                File.Copy(source, target, true);
                AssetDatabase.ImportAsset(target, ImportAssetOptions.ForceUpdate);
                if (AssetImporter.GetAtPath(target) is ModelImporter importer && !importer.bakeAxisConversion)
                {
                    // The files were exported without Apply Transform: bake the axes so +Z is the business end here too.
                    importer.bakeAxisConversion = true;
                    importer.SaveAndReimport();
                }
                if (!rows.Contains(target + ","))
                {
                    if (!rows.EndsWith("\n")) rows += ending;
                    rows += $"{target},model,blender,own,Floopbob Studios,\"{spec.Notes}; made in Blender 5.3 through the Blender MCP connection on 2026-11-16; source file {StagingFolder}/{Path.ChangeExtension(spec.ModelFile, ".blend")}\",-,{DateTime.Now:yyyy-MM-dd},pending{ending}";
                }
                taken++;
            }
            File.WriteAllText(manifest, rows);
            AssetDatabase.Refresh();
            BuildAll();
            Debug.Log($"[Clover.Build] Took {taken} staged weapon models into {ModelFolder}; rebuild the scenes to see them.");
        }

        // ------------------------------------------------------------------ stand-ins

        private static void Box(Transform root, string slot, string name, Vector3 position, Vector3 size, Vector3? euler = null) =>
            BuildKit.Shape(PrimitiveType.Cube, $"{name} ({slot})", root, position, size, null, euler);

        // The Massive Sword: 2.07 m in all, the blade 1.45 x 0.38 x 0.055, single-edged, the point clipped toward the back.
        private static void SwordStandIn(Transform root)
        {
            Box(root, "grip", "Grip", new Vector3(0f, 0f, -0.155f), new Vector3(0.05f, 0.055f, 0.41f));
            Box(root, "blade", "Pommel", new Vector3(0f, 0f, -0.42f), new Vector3(0.045f, 0.11f, 0.11f), new Vector3(45f, 0f, 0f));
            Box(root, "blade", "Guard", new Vector3(0f, 0f, 0.08f), new Vector3(0.08f, 0.5f, 0.07f));
            Box(root, "blade", "Body", new Vector3(0f, 0.03f, 0.74f), new Vector3(0.055f, 0.32f, 1.26f));
            Box(root, "blade", "Point", new Vector3(0f, 0.035f, 1.37f), new Vector3(0.054f, 0.22f, 0.22f), new Vector3(45f, 0f, 0f));
            Box(root, "edge", "Edge", new Vector3(0f, -0.15f, 0.74f), new Vector3(0.03f, 0.08f, 1.26f));
            Box(root, "edge", "Point Edge", new Vector3(0f, -0.1f, 1.4f), new Vector3(0.03f, 0.06f, 0.26f), new Vector3(-38f, 0f, 0f));
            Box(root, "inlay", "Fuller", new Vector3(0f, 0.04f, 0.6f), new Vector3(0.066f, 0.11f, 0.84f));
            Box(root, "accent", "Ribbon A", new Vector3(0.035f, -0.11f, -0.02f), new Vector3(0.012f, 0.25f, 0.05f), new Vector3(20f, 0f, 0f));
            Box(root, "accent", "Ribbon B", new Vector3(-0.035f, -0.1f, -0.06f), new Vector3(0.012f, 0.2f, 0.05f), new Vector3(34f, 0f, 0f));
        }

        // The Scythe: haft from -1.15 to +0.75, the crescent leaving the top toward -Y and curving back to a point
        // near (0, -0.9, 0): six boxes along that curve, tapering, with the cutting edge on the inside.
        private static void ScytheStandIn(Transform root)
        {
            BuildKit.Shape(PrimitiveType.Cylinder, "Haft (wood)", root, new Vector3(0f, 0f, -0.2f), new Vector3(0.055f, 0.95f, 0.055f), null, new Vector3(90f, 0f, 0f));
            Box(root, "guard", "Butt Spike", new Vector3(0f, 0f, -1.21f), new Vector3(0.035f, 0.035f, 0.14f));
            Box(root, "guard", "Collar", new Vector3(0f, 0f, 0.76f), new Vector3(0.085f, 0.085f, 0.16f));
            Box(root, "guard", "Peg", new Vector3(0.07f, 0.01f, -0.4f), new Vector3(0.13f, 0.035f, 0.035f));
            Box(root, "accent", "Cloth A", new Vector3(0.03f, 0.1f, 0.66f), new Vector3(0.012f, 0.3f, 0.07f), new Vector3(-18f, 0f, 0f));
            Box(root, "accent", "Cloth B", new Vector3(-0.03f, 0.08f, 0.62f), new Vector3(0.012f, 0.22f, 0.06f), new Vector3(-34f, 0f, 0f));
            Vector2 start = new Vector2(0.02f, 0.8f), bend = new Vector2(-0.7f, 1.02f), end = new Vector2(-0.92f, -0.02f);
            const int Pieces = 6;
            for (int i = 0; i < Pieces; i++)
            {
                float a = i / (float)Pieces, b = (i + 1) / (float)Pieces;
                Vector2 from = Curve(start, bend, end, a), to = Curve(start, bend, end, b);
                Vector2 middle = (from + to) * 0.5f, along = to - from;
                // Turned about X so the box's length lies along the curve in the weapon's Y-Z plane.
                float angle = Mathf.Atan2(-along.x, along.y) * Mathf.Rad2Deg;
                float width = Mathf.Lerp(0.25f, 0.07f, (a + b) * 0.5f);
                var euler = new Vector3(angle, 0f, 0f);
                // The inside of the curve is toward the grip: the edge runs along it, the iron spine along the outside.
                Vector2 inward = (new Vector2(-0.2f, 0.1f) - middle).normalized;
                Vector2 normal = new Vector2(-along.y, along.x).normalized;
                if (Vector2.Dot(normal, inward) < 0f) normal = -normal;
                Box(root, "blade", "Blade " + i, new Vector3(0f, middle.x, middle.y), new Vector3(0.03f, width, along.magnitude * 1.08f), euler);
                Box(root, "inlay", "Edge " + i, new Vector3(0f, middle.x + normal.x * width * 0.5f, middle.y + normal.y * width * 0.5f), new Vector3(0.034f, 0.06f, along.magnitude * 1.08f), euler);
                Box(root, "guard", "Spine " + i, new Vector3(0f, middle.x - normal.x * width * 0.5f, middle.y - normal.y * width * 0.5f), new Vector3(0.05f, 0.04f, along.magnitude * 1.08f), euler);
            }
        }

        // A point on a curve through three points; x is the weapon's Y and y its Z.
        private static Vector2 Curve(Vector2 a, Vector2 b, Vector2 c, float t) =>
            Vector2.Lerp(Vector2.Lerp(a, b, t), Vector2.Lerp(b, c, t), t);
    }
}

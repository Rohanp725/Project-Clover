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
    /// and writes their manifest rows; it is for the user's word. The sword and scythe came in that way on 2026-11-16, pending approval.
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
            /// <summary>
            /// For a weapon that is one model carried twice (the knives): how the second copy is turned in the left
            /// hand's socket. Null: one copy, in the right hand. The intake brings the one file in once; the second
            /// copy is hung from the same prefab, and at rest it lies mirrored across the body from the first.
            /// </summary>
            public Vector3? OffHandEuler;
            /// <summary>True for a weapon held in the left hand (the bow): its one copy hangs on the left hand's socket.</summary>
            public bool LeftHanded;
            /// <summary>The carry name of something else held in the right hand with it (the bow's arrow), and where in that hand.</summary>
            public string AlsoInRightHand;
            public Vector3 AlsoPosition;
            /// <summary>True for a thing that is never a weapon by itself (an arrow, the chain's weight): a prefab and nothing on the rack.</summary>
            public bool PropOnly;
            /// <summary>What is added to the prefab once its model or stand-in is in place: lines, markers, parts that code moves.</summary>
            public Action<GameObject> Finish;
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
                Carry = "spear", ModelFile = "spear.fbx", StandIn = SpearStandIn,
                Notes = "Great Spear: 420 triangles, slots blade guard wood accent inlay; origin on the shaft 1.0 m from the butt, point along +Z, the leaf's broad faces toward +Y and -Y",
                // Slanted across the back, butt by the right hip, head above the left shoulder.
                RestPosition = new Vector3(0.2f, -0.3f, -0.19f), RestAlong = new Vector3(-0.42f, 0.9f, 0f), RestUp = new Vector3(0f, 0f, -1f),
                // Stood on its butt, the leaf's face toward whoever walks up.
                DisplayPosition = new Vector3(0f, 1.2f, 0f), DisplayEuler = new Vector3(-90f, 0f, 0f),
            },
            new Spec
            {
                Carry = "hammer", ModelFile = "hammer.fbx", StandIn = HammerStandIn,
                Notes = "Great Hammer: 496 triangles, slots head wood wrap accent inlay; origin at the swinging grip 0.9 m under the head, head along +Z, striking face toward -Y",
                // Head up behind the right shoulder, the head lying across the back.
                RestPosition = new Vector3(0.08f, -0.2f, -0.24f), RestAlong = new Vector3(0.3f, 0.95f, 0f), RestUp = new Vector3(0.95f, -0.3f, 0f),
                // Stood on its butt, the head seen from the side.
                DisplayPosition = new Vector3(0f, 0.62f, 0f), DisplayEuler = new Vector3(-90f, 90f, 0f),
            },
            new Spec
            {
                Carry = "knife", ModelFile = "knife.fbx", StandIn = KnifeStandIn,
                Notes = "Twin Knives: one knife, 370 triangles, carried twice; slots blade guard grip accent inlay; origin at the middle of the grip, point along +Z, edge toward -Y",
                // Crossed at the small of the back under the belt, hilts out to either side (the second lies mirrored).
                RestSocket = WeaponRack.Hip,
                RestPosition = new Vector3(0.12f, 0.04f, -0.15f), RestAlong = new Vector3(-0.85f, -0.5f, 0f), RestUp = new Vector3(0f, 0f, -1f),
                // Point up on its stand, the flat toward whoever walks up.
                DisplayPosition = new Vector3(0f, 1.0f, 0f), DisplayEuler = new Vector3(-90f, 90f, 0f),
                // The same way round in the left hand: the reverse grip is a matter of where the pose points it.
                OffHandEuler = Vector3.zero,
            },
            new Spec
            {
                Carry = "bow", ModelFile = "bow.fbx", StandIn = BowStandIn, Finish = BowFinish, LeftHanded = true,
                AlsoInRightHand = "arrow", AlsoPosition = new Vector3(0f, 0f, -0.2f),
                Notes = "Great Bow: 360 triangles and a 12 triangle string, slots wood wrap grip accent inlay string; origin at the middle of the grip, upper limb along +Z, the back toward -Y (the target) and the string on +Y",
                OwnColours = { { "wood", new Color(0.21f, 0.21f, 0.25f) } },
                // Slung on the back, upper limb up past the left shoulder, the string across the chest.
                RestPosition = new Vector3(0.02f, -0.12f, -0.2f), RestAlong = new Vector3(-0.3f, 0.95f, 0f), RestUp = new Vector3(0f, 0f, 1f),
                // Stood on its lower tip, seen from the side: the whole curve and the string.
                DisplayPosition = new Vector3(0f, 0.95f, 0f), DisplayEuler = new Vector3(-90f, 90f, 0f),
            },
            new Spec
            {
                Carry = "arrow", ModelFile = "arrow.fbx", StandIn = ArrowStandIn, PropOnly = true,
                Notes = "Great Bow's arrow: 84 triangles, slots shaft blade accent; origin at the nock, point along +Z",
            },
            new Spec
            {
                Carry = "sickle", ModelFile = "chain_sickle.fbx", StandIn = SickleStandIn,
                Notes = "Chain Sickle: three objects (sickle 382 triangles, weight 200, one link 80), slots blade guard wood accent inlay; origin on the haft under the blade's collar, haft along Z (top +0.17, butt ring -0.43), the blade leaving the top toward -Y",
                // Thrust through the back of the belt, haft upright, blade to the right.
                RestSocket = WeaponRack.Hip,
                RestPosition = new Vector3(0.06f, 0.08f, -0.16f), RestAlong = new Vector3(0.1f, 1f, 0f), RestUp = new Vector3(-1f, 0.1f, 0f),
                // Hung blade up, the crescent seen whole from the front.
                DisplayPosition = new Vector3(0f, 1.1f, 0f), DisplayEuler = new Vector3(-90f, 90f, 0f),
            },
            new Spec
            {
                Carry = "weight", StandIn = WeightStandIn, PropOnly = true,
                Notes = "Chain Sickle's weight: a stand-in for the chain_weight object of chain_sickle.fbx; origin where the chain joins it, body toward +Z",
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
                // As imported the file's business end lies along -Z (the exporter's forward is Unity's back): turned
                // about so the point is along +Z, as the stand-ins and every pose take it to be. The edge stays toward -Y.
                model.transform.localRotation = Quaternion.Euler(0f, 180f, 0f) * model.transform.localRotation;
                foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
                    renderer.sharedMaterials = renderer.sharedMaterials.Select(imported => SlotMaterial(spec, SlotOf(imported != null ? imported.name : ""))).ToArray();
            }
            else
            {
                spec.StandIn(root.transform);
                foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                    renderer.sharedMaterial = SlotMaterial(spec, SlotOf(renderer.name));
            }
            spec.Finish?.Invoke(root);
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
            var handL = Socket(model, "hand.L", WeaponRack.HandLeft, new Vector3(0f, 0.061f, 0f), Quaternion.Inverse(handLeftRest) * Quaternion.LookRotation(bladeLeft, upLeft));
            var back = Socket(model, "chest", WeaponRack.Back, Vector3.zero, Quaternion.Inverse(chestRest));
            var hip = Socket(model, "hips", WeaponRack.Hip, Vector3.zero, Quaternion.Inverse(hipsRest));

            var sets = new List<WeaponRack.Set>();
            foreach (var spec in Specs)
            {
                if (spec.PropOnly) continue;
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
                    if (prefab == null || handR == null || (spec.LeftHanded && handL == null)) continue;
                    inHand.Add(Place(prefab, spec.LeftHanded ? handL : handR, spec.Carry + "_hand", Vector3.zero, Quaternion.identity));
                    var also = spec.AlsoInRightHand != null ? Prefab(spec.AlsoInRightHand) : null;
                    if (also != null) inHand.Add(Place(also, handR, spec.AlsoInRightHand + "_hand", spec.AlsoPosition, Quaternion.identity));
                    var rest = spec.RestSocket == WeaponRack.Hip ? hip : back;
                    if (rest != null) atRest.Add(Place(prefab, rest, spec.Carry + "_rest", spec.RestPosition, Quaternion.LookRotation(spec.RestAlong.normalized, spec.RestUp.normalized)));
                    if (spec.OffHandEuler.HasValue && handL != null)
                    {
                        // The same model a second time: in the left hand, and at rest mirrored across the body.
                        inHand.Add(Place(prefab, handL, spec.Carry + "_hand_L", Vector3.zero, Quaternion.Euler(spec.OffHandEuler.Value)));
                        Vector3 along = spec.RestAlong.normalized, up = spec.RestUp.normalized;
                        if (rest != null) atRest.Add(Place(prefab, rest, spec.Carry + "_rest_L", new Vector3(-spec.RestPosition.x, spec.RestPosition.y, spec.RestPosition.z),
                            Quaternion.LookRotation(new Vector3(-along.x, along.y, along.z), new Vector3(-up.x, up.y, up.z))));
                    }
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
            if (spec == null)
            {
                // The Katana lives inside the character's own model and bare hands are no object at all, so neither
                // has a prefab: each is set out from a few boxes so its stand is not left empty.
                var made = BuildKit.Group((carry == Weapon.KatanaCarry ? "katana" : "fists") + " display", parent);
                var wood = SlotMaterial(null, "wood");
                if (carry == Weapon.KatanaCarry)
                {
                    // Point down in its stand like the Massive Sword, the flat toward whoever walks up.
                    BuildKit.Shape(PrimitiveType.Cube, "Blade (blade)", made, new Vector3(0f, 0.72f, 0f), new Vector3(0.07f, 1.05f, 0.018f), SlotMaterial(null, "blade"), new Vector3(0f, 0f, 2f));
                    BuildKit.Shape(PrimitiveType.Cube, "Guard (guard)", made, new Vector3(0.02f, 1.27f, 0f), new Vector3(0.16f, 0.03f, 0.1f), SlotMaterial(null, "guard"));
                    BuildKit.Shape(PrimitiveType.Cube, "Grip (grip)", made, new Vector3(0.025f, 1.46f, 0f), new Vector3(0.05f, 0.36f, 0.045f), SlotMaterial(null, "grip"), new Vector3(0f, 0f, 2f));
                    BuildKit.Shape(PrimitiveType.Cube, "Tassel (accent)", made, new Vector3(0.08f, 1.52f, 0f), new Vector3(0.03f, 0.22f, 0.03f), SlotMaterial(null, "accent"), new Vector3(0f, 0f, -24f));
                }
                else
                {
                    // A striking post with two wrapped fists hung from its arm.
                    var wrap = SlotMaterial(null, "wrap");
                    BuildKit.Shape(PrimitiveType.Cube, "Post", made, new Vector3(0f, 0.75f, 0f), new Vector3(0.12f, 1f, 0.12f), wood);
                    BuildKit.Shape(PrimitiveType.Cube, "Arm", made, new Vector3(0f, 1.25f, 0f), new Vector3(0.9f, 0.1f, 0.1f), wood);
                    foreach (float side in new[] { -1f, 1f })
                    {
                        BuildKit.Shape(PrimitiveType.Cube, "Wrist (wrap)", made, new Vector3(side * 0.4f, 1.08f, 0f), new Vector3(0.11f, 0.26f, 0.11f), wrap);
                        BuildKit.Shape(PrimitiveType.Cube, "Fist (wrap)", made, new Vector3(side * 0.4f, 0.86f, 0f), new Vector3(0.2f, 0.2f, 0.2f), wrap, new Vector3(0f, 45f, 0f));
                        BuildKit.Shape(PrimitiveType.Cube, "Band (accent)", made, new Vector3(side * 0.4f, 0.97f, 0f), new Vector3(0.13f, 0.04f, 0.13f), SlotMaterial(null, "accent"));
                    }
                }
                return made.gameObject;
            }
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

        // The Great Spear: 3.06 m, the grip 1.0 m from the butt; a leaf 0.5 x 0.23 lying flat (its faces up and down)
        // over a crossbar 0.39 wide, so a level spear shows a cross to the high camera.
        private static void SpearStandIn(Transform root)
        {
            BuildKit.Shape(PrimitiveType.Cylinder, "Pole (wood)", root, new Vector3(0f, 0f, 0.26f), new Vector3(0.055f, 1.18f, 0.055f), null, new Vector3(90f, 0f, 0f));
            Box(root, "guard", "Butt Spike", new Vector3(0f, 0f, -0.96f), new Vector3(0.05f, 0.05f, 0.12f));
            Box(root, "guard", "Sleeve A", new Vector3(0f, 0f, 0f), new Vector3(0.07f, 0.07f, 0.16f));
            Box(root, "guard", "Sleeve B", new Vector3(0f, 0f, 0.6f), new Vector3(0.07f, 0.07f, 0.16f));
            for (int i = 0; i < 3; i++) Box(root, "guard", "Ring " + i, new Vector3(0f, 0f, 1.2f + 0.08f * i), new Vector3(0.075f, 0.075f, 0.03f));
            Box(root, "guard", "Socket", new Vector3(0f, 0f, 1.5f), new Vector3(0.065f, 0.065f, 0.14f));
            Box(root, "guard", "Crossbar", new Vector3(0f, 0f, 1.45f), new Vector3(0.39f, 0.045f, 0.045f));
            Box(root, "blade", "Leaf", new Vector3(0f, 0f, 1.72f), new Vector3(0.2f, 0.024f, 0.32f));
            Box(root, "blade", "Leaf Point", new Vector3(0f, 0f, 1.9f), new Vector3(0.142f, 0.022f, 0.142f), new Vector3(0f, 45f, 0f));
            Box(root, "inlay", "Midrib", new Vector3(0f, 0f, 1.76f), new Vector3(0.035f, 0.04f, 0.42f));
            Box(root, "accent", "Tuft", new Vector3(0f, -0.11f, 1.38f), new Vector3(0.035f, 0.2f, 0.045f));
        }

        // One of the Twin Knives, to the staged model's size: 0.53 m, the point 0.39 ahead of the grip's middle and the
        // ring 0.14 behind it, the blade 0.11 deep with its edge (the inlay) toward -Y and an iron back.
        private static void KnifeStandIn(Transform root)
        {
            Box(root, "grip", "Grip", new Vector3(0f, 0f, 0f), new Vector3(0.032f, 0.036f, 0.11f));
            Box(root, "accent", "Cord", new Vector3(0f, 0f, 0.045f), new Vector3(0.042f, 0.046f, 0.02f));
            Box(root, "guard", "Guard", new Vector3(0f, 0f, 0.066f), new Vector3(0.05f, 0.14f, 0.02f));
            Box(root, "blade", "Blade", new Vector3(0f, -0.004f, 0.2f), new Vector3(0.014f, 0.08f, 0.25f));
            Box(root, "blade", "Point", new Vector3(0f, 0.004f, 0.325f), new Vector3(0.013f, 0.09f, 0.09f), new Vector3(45f, 0f, 0f));
            Box(root, "guard", "Back", new Vector3(0f, 0.044f, 0.19f), new Vector3(0.018f, 0.022f, 0.23f));
            Box(root, "inlay", "Edge", new Vector3(0f, -0.05f, 0.2f), new Vector3(0.016f, 0.03f, 0.25f));
            Box(root, "guard", "Ring", new Vector3(0f, 0f, -0.105f), new Vector3(0.02f, 0.06f, 0.06f));
        }

        // The Great Hammer: 1.76 m, the swinging grip 0.9 m under the head; the head 0.36 square and 0.69 from the
        // boss on its striking face (toward -Y) to the blunt wedge behind.
        private static void HammerStandIn(Transform root)
        {
            BuildKit.Shape(PrimitiveType.Cylinder, "Haft (wood)", root, new Vector3(0f, 0f, 0.44f), new Vector3(0.06f, 0.88f, 0.06f), null, new Vector3(90f, 0f, 0f));
            Box(root, "head", "Butt Cap", new Vector3(0f, 0f, -0.44f), new Vector3(0.075f, 0.075f, 0.06f));
            Box(root, "wrap", "Binding", new Vector3(0f, 0f, -0.1f), new Vector3(0.07f, 0.07f, 0.5f));
            Box(root, "head", "Block", new Vector3(0f, -0.08f, 1.08f), new Vector3(0.36f, 0.42f, 0.36f));
            Box(root, "head", "Wedge", new Vector3(0f, 0.22f, 1.08f), new Vector3(0.36f, 0.24f, 0.2f));
            Box(root, "inlay", "Boss", new Vector3(0f, -0.315f, 1.08f), new Vector3(0.24f, 0.06f, 0.24f));
            Box(root, "inlay", "Belt", new Vector3(0f, -0.05f, 1.08f), new Vector3(0.385f, 0.06f, 0.385f));
            Box(root, "head", "Collar", new Vector3(0f, 0f, 0.86f), new Vector3(0.1f, 0.1f, 0.08f));
            Box(root, "wrap", "Rope", new Vector3(0f, 0f, 0.76f), new Vector3(0.085f, 0.085f, 0.12f));
            Box(root, "accent", "Knot", new Vector3(0.05f, -0.09f, 0.74f), new Vector3(0.05f, 0.16f, 0.05f));
        }

        // A box laid from one point to another in the weapon's Y-Z plane (x of each point is the weapon's Y, y its Z).
        private static void Span(Transform root, string slot, string name, Vector2 from, Vector2 to, float thick, float width)
        {
            Vector2 middle = (from + to) * 0.5f, along = to - from;
            Box(root, slot, name, new Vector3(0f, middle.x, middle.y), new Vector3(thick, width, along.magnitude * 1.06f), new Vector3(Mathf.Atan2(-along.x, along.y) * Mathf.Rad2Deg, 0f, 0f));
        }

        // The Great Bow to the staged model's size: 1.99 m, gripped a third of the way up (upper tip at +1.28, lower at
        // -0.71), both limbs sweeping from the grip back toward the archer (+Y) so the string runs straight at Y 0.312
        // from Z -0.628 to +1.178, and each tip recurving away again under its horn cap.
        private static void BowStandIn(Transform root)
        {
            Box(root, "grip", "Grip", Vector3.zero, new Vector3(0.062f, 0.058f, 0.15f));
            Box(root, "accent", "Tie Above", new Vector3(0f, 0f, 0.095f), new Vector3(0.07f, 0.066f, 0.04f));
            Box(root, "accent", "Tie Below", new Vector3(0f, 0f, -0.095f), new Vector3(0.07f, 0.066f, 0.04f));
            var upper = new[] { new Vector2(0f, 0.07f), new Vector2(0.03f, 0.42f), new Vector2(0.13f, 0.76f), new Vector2(0.26f, 1.03f), new Vector2(0.312f, 1.178f) };
            var lower = new[] { new Vector2(0f, -0.07f), new Vector2(0.05f, -0.3f), new Vector2(0.19f, -0.5f), new Vector2(0.312f, -0.628f) };
            for (int i = 0; i + 1 < upper.Length; i++)
            {
                Span(root, "wood", "Upper Limb " + i, upper[i], upper[i + 1], Mathf.Lerp(0.07f, 0.045f, i / 3f), Mathf.Lerp(0.052f, 0.034f, i / 3f));
                Span(root, "inlay", "Upper Strip " + i, upper[i], upper[i + 1], 0.024f, Mathf.Lerp(0.062f, 0.044f, i / 3f));
            }
            for (int i = 0; i + 1 < lower.Length; i++)
            {
                Span(root, "wood", "Lower Limb " + i, lower[i], lower[i + 1], Mathf.Lerp(0.07f, 0.045f, i / 2f), Mathf.Lerp(0.052f, 0.034f, i / 2f));
                Span(root, "inlay", "Lower Strip " + i, lower[i], lower[i + 1], 0.024f, Mathf.Lerp(0.062f, 0.044f, i / 2f));
            }
            Span(root, "wrap", "Horn Cap Upper", upper[upper.Length - 1], new Vector2(0.275f, 1.28f), 0.05f, 0.045f);
            Span(root, "wrap", "Horn Cap Lower", lower[lower.Length - 1], new Vector2(0.285f, -0.71f), 0.05f, 0.045f);
            foreach (var at in new[] { upper[1], upper[2], upper[3], lower[1], lower[2] })
                Box(root, "wrap", "Binding", new Vector3(0f, at.x, at.y), new Vector3(0.075f, 0.062f, 0.05f));
            Box(root, "string", "String", new Vector3(0f, 0.312f, 0.275f), new Vector3(0.012f, 0.012f, 1.806f));
        }

        // What the bow needs besides its shape: the two points its string runs between, and the line drawn through
        // the string hand while it is drawn (the bar is the string at rest).
        private static void BowFinish(GameObject root)
        {
            Renderer bar = null;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
                if (renderer.name.ToLowerInvariant().Contains("string")) bar = renderer;
            var upper = new GameObject("tip_upper").transform;
            upper.SetParent(root.transform, false);
            upper.localPosition = new Vector3(0f, 0.312f, 1.178f);
            var lower = new GameObject("tip_lower").transform;
            lower.SetParent(root.transform, false);
            lower.localPosition = new Vector3(0f, 0.312f, -0.628f);
            var drawn = new GameObject("Drawn String");
            drawn.transform.SetParent(root.transform, false);
            var line = drawn.AddComponent<LineRenderer>();
            line.sharedMaterial = BuildKit.Toon(Folder, "m_weapon_string_line", SlotColours["string"], false, null, 0f, 0.02f);
            line.positionCount = 3;
            line.useWorldSpace = true;
            line.startWidth = line.endWidth = 0.02f;
            line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            line.receiveShadows = false;
            line.enabled = false;
            root.AddComponent<BowString>().Configure(line, upper, lower, bar);
        }

        // An arrow: 1.1 m from the nock at the origin to the point, a leaf head and three vanes.
        private static void ArrowStandIn(Transform root)
        {
            Box(root, "shaft", "Shaft", new Vector3(0f, 0f, 0.5f), new Vector3(0.022f, 0.022f, 1.0f));
            Box(root, "blade", "Leaf", new Vector3(0f, 0f, 1.02f), new Vector3(0.014f, 0.085f, 0.085f), new Vector3(45f, 0f, 0f));
            Box(root, "blade", "Leaf Point", new Vector3(0f, 0f, 1.07f), new Vector3(0.012f, 0.04f, 0.06f));
            for (int i = 0; i < 3; i++)
                Box(root, "accent", "Vane " + i, new Vector3(0f, 0f, 0.13f), new Vector3(0.008f, 0.1f, 0.2f), new Vector3(0f, 0f, 120f * i));
        }

        // The Chain Sickle's blade hand, to the staged model's size: a haft 0.6 m long gripped just under the collar,
        // and a crescent leaving its top toward -Y and curving back down to a point near (0, -0.34, -0.16).
        private static void SickleStandIn(Transform root)
        {
            Box(root, "wood", "Haft", new Vector3(0f, 0f, -0.13f), new Vector3(0.036f, 0.036f, 0.6f));
            Box(root, "guard", "Butt Ring", new Vector3(0f, 0f, -0.455f), new Vector3(0.016f, 0.06f, 0.06f), new Vector3(45f, 0f, 0f));
            Box(root, "guard", "Collar", new Vector3(0f, 0f, 0.15f), new Vector3(0.052f, 0.052f, 0.06f));
            Box(root, "accent", "Cord", new Vector3(0f, 0f, 0.09f), new Vector3(0.046f, 0.046f, 0.03f));
            Vector2 start = new Vector2(0.02f, 0.17f), bend = new Vector2(-0.34f, 0.3f), end = new Vector2(-0.34f, -0.16f);
            const int Pieces = 5;
            for (int i = 0; i < Pieces; i++)
            {
                float a = i / (float)Pieces, b = (i + 1) / (float)Pieces;
                Vector2 from = Curve(start, bend, end, a), to = Curve(start, bend, end, b);
                Vector2 middle = (from + to) * 0.5f, along = to - from;
                float width = Mathf.Lerp(0.125f, 0.04f, (a + b) * 0.5f);
                Vector2 inward = (new Vector2(-0.12f, -0.05f) - middle).normalized;
                Vector2 normal = new Vector2(-along.y, along.x).normalized;
                if (Vector2.Dot(normal, inward) < 0f) normal = -normal;
                Span(root, "blade", "Blade " + i, from, to, 0.022f, width);
                Span(root, "inlay", "Inner " + i, from + normal * width * 0.5f, to + normal * width * 0.5f, 0.026f, 0.03f);
                Span(root, "guard", "Spine " + i, from - normal * width * 0.5f, to - normal * width * 0.5f, 0.036f, 0.024f);
            }
        }

        // The weight on the end of the chain: an iron teardrop with a band round its belly, the chain joining at the origin.
        private static void WeightStandIn(Transform root)
        {
            Box(root, "guard", "Eye", new Vector3(0f, 0f, 0f), new Vector3(0.02f, 0.05f, 0.05f), new Vector3(45f, 0f, 0f));
            Box(root, "guard", "Neck", new Vector3(0f, 0f, 0.05f), new Vector3(0.07f, 0.07f, 0.07f), new Vector3(0f, 0f, 45f));
            Box(root, "guard", "Body", new Vector3(0f, 0f, 0.14f), new Vector3(0.13f, 0.13f, 0.15f), new Vector3(0f, 0f, 45f));
            Box(root, "inlay", "Belly", new Vector3(0f, 0f, 0.14f), new Vector3(0.15f, 0.15f, 0.045f), new Vector3(0f, 0f, 45f));
        }

        // A point on a curve through three points; x is the weapon's Y and y its Z.
        private static Vector2 Curve(Vector2 a, Vector2 b, Vector2 c, float t) =>
            Vector2.Lerp(Vector2.Lerp(a, b, t), Vector2.Lerp(b, c, t), t);
    }
}

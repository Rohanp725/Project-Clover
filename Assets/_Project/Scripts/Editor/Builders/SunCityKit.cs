using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// The Fire land's materials and scenery pieces (docs/design/fire_land.md, section 2). The models carry no colour
    /// of their own: each material slot is named for what it is, and the toon material is found here by that name.
    /// The world is pale and muted; gold shines, and the ember, molten and sun slots glow. Those are the only strong
    /// colours. People carry the heavy ink, scenery the thin one.
    /// </summary>
    public static class SunCityKit
    {
        public const string Folder = BuildKit.GeneratedRoot + "/SunCity";
        public const string KitPath = "Assets/_Project/Art/Models/Environment/fire_kit.fbx";
        public const string TombPath = "Assets/_Project/Art/Models/Environment/tomb_panels.fbx";
        public const string SunFormPath = "Assets/_Project/Art/Models/Characters/boss_sunform.fbx";

        public static readonly Color Gold = new Color(0.95f, 0.7f, 0.18f);
        public static readonly Color Ember = new Color(1f, 0.42f, 0.08f);

        // What each slot is painted, as the eye sees it. The enemies' people: m_fire_*.
        private static readonly Dictionary<string, Color> fire = new Dictionary<string, Color>
        {
            { "skin", new Color(0.53f, 0.44f, 0.37f) },
            { "wrap", new Color(0.7f, 0.67f, 0.6f) },
            { "char", new Color(0.09f, 0.08f, 0.09f) },
            { "linen", new Color(0.76f, 0.73f, 0.66f) },
            { "dark", new Color(0.11f, 0.09f, 0.1f) },
            { "wood", new Color(0.33f, 0.25f, 0.19f) },
            { "bronze", new Color(0.47f, 0.38f, 0.27f) },
            { "sand", new Color(0.66f, 0.58f, 0.43f) },
            { "clay", new Color(0.52f, 0.31f, 0.24f) },
            { "gold", Gold },
            { "ember", Ember },
            { "molten", new Color(1f, 0.62f, 0.16f) },
            { "sun", new Color(1f, 0.92f, 0.62f) },
        };

        // The god's form: m_sunform_*.
        private static readonly Dictionary<string, Color> sunform = new Dictionary<string, Color>
        {
            { "shell", new Color(0.17f, 0.14f, 0.12f) },
            { "wing", new Color(0.74f, 0.7f, 0.6f) },
            { "feather", new Color(0.42f, 0.33f, 0.25f) },
            { "gold", Gold },
            { "face", new Color(0.6f, 0.49f, 0.4f) },
            { "dark", new Color(0.1f, 0.08f, 0.09f) },
            { "eye", new Color(1f, 0.92f, 0.62f) },
            { "sun", new Color(1f, 0.8f, 0.35f) },
            { "molten", new Color(1f, 0.55f, 0.12f) },
        };

        // The scenery: m_kit_*.
        private static readonly Dictionary<string, Color> kit = new Dictionary<string, Color>
        {
            { "stone", new Color(0.66f, 0.62f, 0.55f) },
            { "stone_dark", new Color(0.42f, 0.38f, 0.35f) },
            // What the king had made or recut: a shade too pale, so the eye catches it.
            { "stone_new", new Color(0.82f, 0.79f, 0.72f) },
            { "paint", new Color(0.55f, 0.36f, 0.27f) },
            { "gold", Gold },
            { "wood", new Color(0.36f, 0.28f, 0.21f) },
            { "reed", new Color(0.5f, 0.46f, 0.3f) },
            { "cloth", new Color(0.7f, 0.67f, 0.6f) },
            // The land's one blue: the lid on the sealed well.
            { "water", new Color(0.2f, 0.33f, 0.5f) },
            { "ember", Ember },
        };

        // The painted tombs: m_tomb_*.
        private static readonly Dictionary<string, Color> tomb = new Dictionary<string, Color>
        {
            { "plaster", new Color(0.8f, 0.75f, 0.63f) },
            { "ochre", new Color(0.7f, 0.5f, 0.22f) },
            { "red", new Color(0.52f, 0.2f, 0.14f) },
            { "blue", new Color(0.2f, 0.33f, 0.5f) },
            { "black", new Color(0.08f, 0.07f, 0.07f) },
            { "gold", Gold },
            { "flaked", new Color(0.5f, 0.45f, 0.38f) },
            { "ember", Ember },
        };

        /// <summary>True for a slot name this land knows how to paint.</summary>
        public static bool Knows(string slot) =>
            slot != null && (slot.StartsWith("m_fire_") || slot.StartsWith("m_sunform_") || slot.StartsWith("m_kit_") || slot.StartsWith("m_tomb_"));

        /// <summary>
        /// The toon material for an imported slot name. <paramref name="rank"/> is the gilded soldier's (1 to 3): a slot
        /// ending _r2 or _r3 is gold from that rank up; below it, skin stays skin, bronze stays bronze, and the gold
        /// runs and drips that belong to the higher rank are not drawn at all.
        /// </summary>
        public static Material For(string slot, int rank = 3)
        {
            if (slot.StartsWith("m_fire_"))
            {
                string name = slot.Substring("m_fire_".Length);
                int from = name.EndsWith("_r2") ? 2 : name.EndsWith("_r3") ? 3 : 0;
                if (from > 0)
                {
                    name = name.Substring(0, name.Length - 3);
                    if (rank >= from) name = "gold";
                    else if (name == "gold") return Hidden();
                }
                return Person("m_fire_" + name, fire.TryGetValue(name, out var colour) ? colour : Unknown(slot), name);
            }
            if (slot.StartsWith("m_sunform_"))
            {
                string name = slot.Substring("m_sunform_".Length);
                return Person(slot, sunform.TryGetValue(name, out var colour) ? colour : Unknown(slot), name == "eye" ? "sun" : name);
            }
            if (slot.StartsWith("m_tomb_"))
            {
                string name = slot.Substring("m_tomb_".Length);
                return Scenery(slot, tomb.TryGetValue(name, out var colour) ? colour : Unknown(slot), name, 0.006f);
            }
            {
                string name = slot.Substring("m_kit_".Length);
                return Scenery(slot, kit.TryGetValue(name, out var colour) ? colour : Unknown(slot), name, 0.014f);
            }
        }

        private static Color Unknown(string slot)
        {
            Debug.LogWarning($"[Clover.Build] The Fire land has no colour for the slot '{slot}'; it is drawn grey.");
            return Color.grey;
        }

        // Heavy ink, and the light that makes gold shine and the three glowing slots the brightest things on the body.
        private static Material Person(string materialName, Color colour, string what)
        {
            var material = BuildKit.Toon(Folder, materialName, colour, true);
            return Light(material, what, 1f);
        }

        private static Material Scenery(string materialName, Color colour, string what, float outline)
        {
            var material = BuildKit.Toon(Folder, materialName, colour, false, null, 0f, outline);
            return Light(material, what, 0.8f);
        }

        /// <summary>What every surface of the land turns to out of the sun: a cool grey-violet, never black.</summary>
        public static readonly Color ShadeColour = new Color(0.56f, 0.53f, 0.66f);

        private static Material Light(Material material, string what, float strength)
        {
            material.SetColor("_ShadeColor", ShadeColour);
            switch (what)
            {
                case "ember": return material.Lit(new Color(1.6f, 0.7f, 0.2f), 0.5f, new Color(2.7f, 0.62f, 0.07f) * strength);
                case "molten": return material.Lit(new Color(1.6f, 0.9f, 0.3f), 0.5f, new Color(3.1f, 1.15f, 0.12f) * strength);
                // The hottest slot is a coal's yellow-orange, not white light (the owner's review, 2026-11-17).
                case "sun": return material.Lit(new Color(1.6f, 1.1f, 0.5f), 0.5f, new Color(3.0f, 1.5f, 0.3f) * strength);
                // Gold is lit, not glowing: a broad warm rim and a little of its own light so it holds its colour in shade.
                case "gold": return material.Lit(new Color(2.2f, 1.6f, 0.6f), 0.6f, new Color(0.42f, 0.27f, 0.04f));
                case "char":
                case "dark":
                case "black":
                case "shell": return material.Lit(new Color(0.5f, 0.42f, 0.36f), 0.3f);
                default: return material.Lit(new Color(0.75f, 0.7f, 0.6f), 0.3f);
            }
        }

        /// <summary>
        /// The ashen's colours on the wick's slots: the wrappings burned to a shell of grey ash, the char gone darker, and
        /// every glowing slot turned to the white-hot core that shows through the cracks.
        /// </summary>
        public static Material Ashen(string slot)
        {
            string name = slot.StartsWith("m_fire_") ? slot.Substring("m_fire_".Length) : slot;
            if (name.EndsWith("_r2") || name.EndsWith("_r3")) return Hidden();
            if (name == "ember" || name == "molten" || name == "sun")
            {
                var core = BuildKit.Toon(Folder, "m_ashen_core", new Color(1f, 0.93f, 0.8f), true);
                core.SetColor("_ShadeColor", ShadeColour);
                return core.Lit(new Color(2f, 1.8f, 1.4f), 0.6f, new Color(6f, 4.6f, 2.6f));
            }
            bool dark = name == "char" || name == "dark" || name == "wood";
            var ash = BuildKit.Toon(Folder, dark ? "m_ashen_char" : "m_ashen_ash", dark ? new Color(0.17f, 0.15f, 0.15f) : new Color(0.6f, 0.58f, 0.55f), true);
            ash.SetColor("_ShadeColor", ShadeColour);
            // A faint ember rim: the heat inside is never quite hidden.
            return ash.Lit(dark ? new Color(0.9f, 0.4f, 0.15f) : new Color(1f, 0.75f, 0.5f), 0.35f, dark ? (Color?)new Color(0.25f, 0.06f, 0.01f) : null);
        }

        /// <summary>A material that draws nothing: for the gold a lower rank has not been given yet.</summary>
        public static Material Hidden()
        {
            var material = BuildKit.Toon(Folder, "m_fire_hidden", Color.black, true);
            material.SetFloat("_Fade", 0f);
            material.SetFloat("_OutlineWidth", 0f);
            EditorUtility.SetDirty(material);
            return material;
        }

        /// <summary>Paints every slot of a model that this land knows, and leaves the others as they are.</summary>
        public static void Dress(GameObject model, int rank = 3)
        {
            foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
                renderer.sharedMaterials = renderer.sharedMaterials.Select(imported => imported != null && Knows(imported.name) ? For(imported.name, rank) : imported).ToArray();
        }

        /// <summary>
        /// The wholly gilded champion's model has one gold slot from mask to soles, and so read as one flat colour.
        /// This gives the cast its tones by where each part of the body hangs: the mask and neck a paler, newer gold;
        /// the kilt (hips and thighs), the hands and the feet a darker, redder gold, as a casting is where it is
        /// thick; the spear's shaft dark bronze. The rest keeps the gold it had. The mesh is copied to a generated
        /// asset with a submesh for each tone; the model's own file is not touched.
        /// </summary>
        public static void CastTones(SkinnedMeshRenderer renderer)
        {
            var mesh = renderer.sharedMesh;
            var materials = renderer.sharedMaterials;
            int goldSlot = System.Array.FindIndex(materials, material => material != null && material.name == "m_fire_gold");
            if (mesh == null || goldSlot < 0 || goldSlot >= mesh.subMeshCount) return;
            var weights = mesh.boneWeights;
            var heights = mesh.vertices;
            var tone = new int[renderer.bones.Length];
            for (int i = 0; i < tone.Length; i++)
            {
                string bone = renderer.bones[i] != null ? renderer.bones[i].name : "";
                tone[i] = bone == "head" || bone == "neck" ? 1
                    : bone == "hips" || bone.StartsWith("thigh") || bone.StartsWith("hand") || bone.StartsWith("foot") ? 2
                    : bone.StartsWith("weapon") ? 3 : 0;
            }
            var triangles = mesh.GetTriangles(goldSlot);
            var split = new[] { new List<int>(), new List<int>(), new List<int>(), new List<int>() };
            // The spear's head stays bright: only the shaft is the dark bronze. The spear's line is found from its own
            // points (the two farthest apart), and the head is the end that is the wider.
            Vector3 middle = Vector3.zero;
            int count = 0;
            for (int i = 0; i < weights.Length; i++)
                if (tone[weights[i].boneIndex0] == 3) { middle += heights[i]; count++; }
            middle /= Mathf.Max(1, count);
            Vector3 far = middle;
            for (int i = 0; i < weights.Length; i++)
                if (tone[weights[i].boneIndex0] == 3 && (heights[i] - middle).sqrMagnitude > (far - middle).sqrMagnitude) far = heights[i];
            Vector3 line = (far - middle).normalized;
            float least = 0f, most = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                if (tone[weights[i].boneIndex0] != 3) continue;
                float along = Vector3.Dot(heights[i] - middle, line);
                least = Mathf.Min(least, along);
                most = Mathf.Max(most, along);
            }
            float widthLow = 0f, widthHigh = 0f;
            for (int i = 0; i < weights.Length; i++)
            {
                if (tone[weights[i].boneIndex0] != 3) continue;
                float along = Vector3.Dot(heights[i] - middle, line);
                float off = (heights[i] - middle - line * along).magnitude;
                if (along < least + 0.6f) widthLow = Mathf.Max(widthLow, off);
                if (along > most - 0.6f) widthHigh = Mathf.Max(widthHigh, off);
            }
            bool headIsHigh = widthHigh >= widthLow;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                int which = tone[weights[triangles[i]].boneIndex0];
                if (which == 3)
                {
                    float along = Vector3.Dot(heights[triangles[i]] - middle, line);
                    if (headIsHigh ? along > most - 0.85f : along < least + 0.85f) which = 0;
                }
                split[which].Add(triangles[i]);
                split[which].Add(triangles[i + 1]);
                split[which].Add(triangles[i + 2]);
            }
            var toned = Object.Instantiate(mesh);
            toned.name = mesh.name + "_cast";
            toned.subMeshCount = mesh.subMeshCount + 3;
            toned.SetTriangles(split[0], goldSlot);
            for (int extra = 1; extra <= 3; extra++) toned.SetTriangles(split[extra], mesh.subMeshCount + extra - 1);
            string path = $"{Folder}/{toned.name}.asset";
            AssetDatabase.DeleteAsset(path);
            AssetDatabase.CreateAsset(toned, path);
            renderer.sharedMesh = toned;
            var pale = BuildKit.Toon(Folder, "m_fire_gold_pale", new Color(1f, 0.93f, 0.68f), true).Lit(new Color(2.4f, 2.1f, 1.2f), 0.6f, new Color(0.6f, 0.5f, 0.22f));
            var deep = BuildKit.Toon(Folder, "m_fire_gold_deep", new Color(0.52f, 0.3f, 0.09f), true).Lit(new Color(1.5f, 0.9f, 0.3f), 0.4f, new Color(0.08f, 0.04f, 0.01f));
            var shaft = BuildKit.Toon(Folder, "m_fire_cast_bronze", new Color(0.22f, 0.15f, 0.09f), true).Lit(new Color(1.2f, 0.8f, 0.4f), 0.4f);
            foreach (var material in new[] { pale, deep, shaft }) material.SetColor("_ShadeColor", ShadeColour);
            renderer.sharedMaterials = materials.Concat(new[] { pale, deep, shaft }).ToArray();
        }

        // ------------------------------------------------------------------ the ground and plain walls

        // Plain colours: the stone textures made for the castle carry its blues and reds, and this land has neither.
        public static Material Mud => Plain("m_sun_mud", new Color(0.52f, 0.47f, 0.4f), 0f);
        public static Material Brick => Plain("m_sun_brick", new Color(0.58f, 0.52f, 0.44f), 0.02f);
        public static Material BrickDark => Plain("m_sun_brick_dark", new Color(0.4f, 0.36f, 0.33f), 0.02f);
        public static Material Stone => Plain("m_sun_stone", new Color(0.66f, 0.62f, 0.55f), 0.02f);
        public static Material Shade => Plain("m_sun_shade", new Color(0.3f, 0.29f, 0.36f), 0f);

        /// <summary>
        /// Gold laid into a floor: worn, a deep bronze-gold, with no light of its own and only a narrow warm rim, so the
        /// court's rings and hour bars read as metal in the stone and not as lines of light (the owner's review, 2026-11-17).
        /// </summary>
        public static Material FloorGold
        {
            get
            {
                var material = BuildKit.Toon(Folder, "m_sun_floor_gold", new Color(0.66f, 0.47f, 0.2f), false, null, 0f, 0.012f);
                material.SetColor("_ShadeColor", ShadeColour);
                return material.Lit(new Color(0.85f, 0.55f, 0.25f), 0.18f);
            }
        }

        /// <summary>
        /// A brazier's fire. The model's flame is a hard faceted shape that read as a gold crystal (the owner's review,
        /// 2026-11-17), so it is not drawn; instead real fire burns in the bowl: tongues of flame that lick up and cool
        /// through red to smoke, and a thread of smoke over them. It burns from the start; a rest fire stops it at once
        /// and lights it when rested at (<see cref="Clover.World.RestPoint"/> plays it).
        /// </summary>
        public static ParticleSystem BrazierFire(Transform brazier, GameObject model)
        {
            foreach (var renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                var slots = renderer.sharedMaterials;
                for (int i = 0; i < slots.Length; i++)
                    if (slots[i] != null && slots[i].name == "m_kit_ember") slots[i] = Hidden();
                renderer.sharedMaterials = slots;
            }
            var fireObject = new GameObject("Brazier Fire");
            fireObject.transform.SetParent(brazier, false);
            fireObject.transform.localPosition = new Vector3(0f, 1.8f, 0f);
            fireObject.transform.localRotation = Quaternion.Euler(-90f, 0f, 0f);
            var flames = Burning(fireObject, BuildKit.Fire(Folder, "m_brazier_flame", 6, new Vector2(0f, -1.8f), 3f), 16f, 0.45f, 0.85f, 0.55f, 0.95f, 0.5f, 1.3f, 0.28f, -0.4f);
            flames.GetComponent<ParticleSystemRenderer>().sortingFudge = -1f;
            var main = flames.main;
            // Each tongue licks to its own time: the flame shape reads the red of its colour as its place in the pattern.
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.75f, 1f, 1f, 1f), Color.white);
            main.startRotation = new ParticleSystem.MinMaxCurve(-0.2f, 0.2f);
            var smokeObject = new GameObject("Brazier Smoke");
            smokeObject.transform.SetParent(fireObject.transform, false);
            smokeObject.transform.localPosition = new Vector3(0f, 0f, 0.9f);
            var smoke = Burning(smokeObject, BuildKit.Smoke(VfxBuilder.Folder, "m_vfx_smoke", new Color(0.85f, 0.8f, 0.76f, 0.75f)),
                3f, 1.6f, 2.6f, 0.45f, 0.8f, 0.3f, 0.8f, 0.2f, -0.12f);
            var smokeMain = smoke.main;
            smokeMain.startColor = new Color(0.32f, 0.29f, 0.27f, 0.7f);
            var grow = smoke.sizeOverLifetime;
            grow.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 2.6f));
            return flames;
        }

        /// <summary>A rest fire's flames, out until it is rested at: the brazier's fire and its smoke do not start by themselves.</summary>
        public static ParticleSystem RestFlames(GameObject brazier)
        {
            var flames = brazier.GetComponentInChildren<ParticleSystem>(true);
            if (flames == null) return null;
            foreach (var system in flames.GetComponentsInChildren<ParticleSystem>(true))
            {
                var main = system.main;
                main.playOnAwake = false;
            }
            return flames;
        }

        // A looping emitter that throws upward along its forward from a small disc: fire and smoke that keep burning.
        private static ParticleSystem Burning(GameObject host, Material material, float rate, float lifeMin, float lifeMax,
            float sizeMin, float sizeMax, float speedMin, float speedMax, float radius, float gravity)
        {
            var system = host.AddComponent<ParticleSystem>();
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main;
            main.loop = true;
            main.playOnAwake = true;
            main.duration = 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = 48;
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
            main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
            main.gravityModifier = gravity;
            var emission = system.emission;
            emission.enabled = true;
            emission.rateOverTime = rate;
            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Cone;
            shape.angle = 8f;
            shape.radius = radius;
            var fade = system.colorOverLifetime;
            fade.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.8f, 0.5f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;
            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 0.55f));
            var renderer = host.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return system;
        }

        /// <summary>A plain surface of the land in a colour of the builder's own (packed earth, sand, silt), shaded as the rest is.</summary>
        public static Material Flat(string name, Color colour, float outline = 0f) => Plain(name, colour, outline);

        private static Material Plain(string name, Color colour, float outline)
        {
            var material = BuildKit.Toon(Folder, name, colour, false, null, 0f, outline);
            return Light(material, "", 1f);
        }

        // ------------------------------------------------------------------ pieces

        /// <summary>The names of the pieces in the scenery kit.</summary>
        public static readonly string[] Pieces =
        {
            "statue_king", "statue_head_new", "statue_head_old", "column_lotus", "column_base", "pylon_gate", "barque", "obelisk", "wall_section",
            "wall_broken", "house_block", "house_doorway", "burial_niche", "brazier", "well_shrine", "sarcophagus", "reed_boat", "mud_tile", "market_awning",
        };

        public static readonly string[] Panels = { "tomb_1_hearth", "tomb_2_star", "tomb_3_bearer", "tomb_4_king", "tomb_5_recut", "tomb_6_flaked" };

        /// <summary>
        /// Stands one piece of the scenery kit in the scene, painted. Pieces were modelled with their fronts toward +Z
        /// as Unity has it; the game's camera looks north, so a yaw of 180 turns a front to face it. With
        /// <paramref name="solid"/> the piece gets a box round its whole bounds; anything with a way through it
        /// (the pylon, a doorway) wants its own colliders instead.
        /// </summary>
        public static GameObject Place(string piece, Transform parent, Vector3 position, float yaw = 180f, float scale = 1f, bool solid = true) =>
            PlaceFrom(KitPath, piece, parent, position, yaw, scale, solid);

        /// <summary>Hangs one of the six painted tomb panels: 4 m wide, 3 m high, picture toward +Z before the yaw.</summary>
        public static GameObject Panel(string panel, Transform parent, Vector3 position, float yaw = 180f) =>
            PlaceFrom(TombPath, panel, parent, position, yaw, 1f, true);

        /// <summary>One rigid piece of the god's form (boss_sunform.fbx), painted, at its own origin under the parent.</summary>
        public static GameObject FormPiece(string piece, Transform parent, Vector3 position, float yaw = 0f, float scale = 1f) =>
            PlaceFrom(SunFormPath, piece, parent, position, yaw, scale, false);

        private static GameObject PlaceFrom(string path, string piece, Transform parent, Vector3 position, float yaw, float scale, bool solid)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            var source = asset != null ? asset.transform.Find(piece) : null;
            if (source == null)
            {
                Debug.LogError($"[Clover.Build] No piece named '{piece}' in {path}.");
                return null;
            }
            var holder = new GameObject(piece);
            if (parent != null) holder.transform.SetParent(parent, false);
            holder.transform.position = position;
            holder.transform.rotation = Quaternion.Euler(0f, yaw, 0f);
            holder.transform.localScale = Vector3.one * scale;
            // The piece keeps the turn the file gave it, under a holder that is upright.
            var shown = Object.Instantiate(source.gameObject, holder.transform, false);
            shown.name = "Model";
            Dress(shown);
            if (piece == "brazier") BrazierFire(holder.transform, shown);
            foreach (var renderer in shown.GetComponentsInChildren<Renderer>()) renderer.gameObject.isStatic = true;
            if (solid)
            {
                var filter = shown.GetComponentInChildren<MeshFilter>();
                if (filter != null && filter.sharedMesh != null)
                {
                    var box = shown.AddComponent<BoxCollider>();
                    box.center = filter.sharedMesh.bounds.center;
                    box.size = filter.sharedMesh.bounds.size;
                    // Whatever stands between the camera and the player thins out, as the castle's walls do.
                    shown.AddComponent<Clover.World.Fadeable>();
                }
            }
            return holder;
        }
    }
}

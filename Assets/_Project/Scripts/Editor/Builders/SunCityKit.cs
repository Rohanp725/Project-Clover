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

        // The city kit (fire_city_kit.fbx): m_city_*. Mud brick and its plaster, dressed stone, palm timber, reed, clay
        // and linen, near the builders' own colours so a modelled house sits with what is still built from boxes.
        private static readonly Dictionary<string, Color> city = new Dictionary<string, Color>
        {
            { "mudbrick", new Color(0.52f, 0.44f, 0.36f) },
            { "plaster", new Color(0.72f, 0.66f, 0.56f) },
            { "stone", new Color(0.66f, 0.62f, 0.55f) },
            { "timber", new Color(0.36f, 0.28f, 0.21f) },
            { "reed", new Color(0.56f, 0.5f, 0.33f) },
            { "palm", new Color(0.42f, 0.45f, 0.29f) },
            { "clay", new Color(0.54f, 0.38f, 0.29f) },
            { "linen", new Color(0.8f, 0.77f, 0.7f) },
            { "gold", Gold },
            { "lapis", new Color(0.2f, 0.33f, 0.5f) },
            { "sand", new Color(0.76f, 0.68f, 0.52f) },
            { "char", new Color(0.17f, 0.15f, 0.14f) },
        };

        /// <summary>True for a slot name this land knows how to paint.</summary>
        public static bool Knows(string slot) =>
            slot != null && (slot.StartsWith("m_fire_") || slot.StartsWith("m_sunform_") || slot.StartsWith("m_kit_") || slot.StartsWith("m_tomb_") || slot.StartsWith("m_city_"));

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
            if (slot.StartsWith("m_city_"))
            {
                // The kit is finely made (brick courses, lashings, fronds a few centimetres thick): a hull as thick as
                // the castle's would ink every small part solid, so its line is thinner, and none on fronds and cloth.
                string name = slot.Substring("m_city_".Length);
                float outline = name == "palm" || name == "linen" ? 0f : name == "reed" ? 0.004f : 0.008f;
                return Scenery(slot, city.TryGetValue(name, out var colour) ? colour : Unknown(slot), name, outline);
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
            return Worked(Light(material, what, 0.8f));
        }

        /// <summary>What every surface of the land turns to out of the sun: a cool grey-violet, never black.</summary>
        public static readonly Color ShadeColour = new Color(0.56f, 0.53f, 0.66f);

        /// <summary>
        /// Shade with form for the land's world (the visual overview, step 4). The noon sun stands at 78 degrees, so a
        /// south face, the one the camera sees, catches it at a share of 0.21: just under the toon's shade line, and every
        /// face toward the camera was one flat violet slab. With the line lowered under that share the faces toward the
        /// camera take the middle tone and show their courses, doors and beams, and the faces turned from the sun stay in
        /// shade; those take a warm bounce from the bright floor, so they read as walls
        /// standing on sunlit ground and not as holes. Characters keep the shared line.
        /// </summary>
        public static Material Worked(Material material)
        {
            material.SetFloat("_ShadeThreshold", 0.12f);
            material.SetColor("_BounceColor", new Color(0.34f, 0.24f, 0.13f));
            // With no line, the outline pass would still be drawn for nothing: it is not asked for (a draw saved on
            // every floor, roof and drift of the land).
            material.SetShaderPassEnabled("SRPDefaultUnlit", material.GetFloat("_OutlineWidth") > 0.00001f);
            EditorUtility.SetDirty(material);
            return material;
        }

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

        // Plain colours for what the god's fight uses (the ridge it heaves up, its shadow): left as they were. The castle's
        // stone textures carry its blues and reds, which this land has neither of; the land's own warm-grey patterns
        // (ProceduralTextures.Sun*) go on the city's walls, floors and faces, by material name, in Flat.
        public static Material Mud => Plain("m_sun_mud", new Color(0.52f, 0.47f, 0.4f), 0f);
        public static Material Brick => Flat("m_sun_brick", new Color(0.58f, 0.52f, 0.44f), 0.02f);
        public static Material BrickDark => Flat("m_sun_brick_dark", new Color(0.4f, 0.36f, 0.33f), 0.02f);
        public static Material Stone => Plain("m_sun_stone", new Color(0.66f, 0.62f, 0.55f), 0.02f);
        public static Material Shade => Plain("m_sun_shade", new Color(0.3f, 0.29f, 0.36f), 0f);
        /// <summary>The city's own dressed stone and dried mud: the same colours as Stone and Mud, with surface.</summary>
        public static Material StoneWorked => Flat("m_sun_stone_worked", new Color(0.66f, 0.62f, 0.55f), 0.02f);
        public static Material MudWorked => Flat("m_sun_mud_worked", new Color(0.52f, 0.47f, 0.4f), 0f);

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
            // The city burns in two hundred places: a fire off screen holds still rather than simulating for nobody.
            main.cullingMode = ParticleSystemCullingMode.Pause;
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

        /// <summary>
        /// A surface of the land in a colour of the builder's own (packed earth, sand, silt, brick, ashlar), with the
        /// land's pattern for what it is laid on from world space when its name says what it is, and shaded with form.
        /// </summary>
        public static Material Flat(string name, Color colour, float outline = 0f)
        {
            var (texture, tiles) = Surface(name);
            var material = BuildKit.Toon(Folder, name, colour, false, texture, tiles, outline);
            return Worked(Light(material, "", 1f));
        }

        private static Material Plain(string name, Color colour, float outline)
        {
            var material = BuildKit.Toon(Folder, name, colour, false, null, 0f, outline);
            return Light(material, "", 1f);
        }

        // Which of the land's patterns a material's name asks for, and how many repeats a metre (each pattern is drawn
        // for a known size of ground: a brick course is 0.25 m, an ashlar block about 1.2 m, a mud plate about half a metre).
        private static (Texture2D, float) Surface(string name)
        {
            switch (name)
            {
                case "m_sun_brick":
                case "m_sun_brick_dark":
                    return (ProceduralTextures.SunBrick(), 0.25f);
                case "m_sun_stone_worked":
                case "m_sun_pave_new_a":
                case "m_sun_pave_new_b":
                case "m_sun_ashlar":
                case "m_sun_ashlar_band":
                case "m_sun_hall_floor":
                    return (ProceduralTextures.SunAshlar(), 0.125f);
                case "m_sun_earth":
                case "m_sun_earth_dark":
                    return (ProceduralTextures.SunEarth(), 0.125f);
                case "m_sun_sand":
                    return (ProceduralTextures.SunSand(), 0.1f);
                case "m_sun_plaster":
                    return (ProceduralTextures.SunEarth(), 0.18f);
                case "m_sun_bank_face":
                case "m_sun_bank_band":
                    return (ProceduralTextures.SunEarth(), 0.18f);
                case "m_sun_mud_worked":
                case "m_sun_silt":
                    return (ProceduralTextures.SunMud(), 0.125f);
                default:
                    return (null, 0f);
            }
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

        public const string CityKitPath = "Assets/_Project/Art/Models/Environment/fire_city_kit.fbx";

        /// <summary>
        /// Stands one piece of the city kit (fire_city_kit.fbx; <paramref name="piece"/> is its name without "fire_" and
        /// "_lod0", as "house_wall"), painted, for the eye only: no collider, static. Fronts face +Z before the yaw, so the
        /// default 180 turns them to the camera. <paramref name="scale"/> may differ per axis and be negative in x to
        /// mirror a piece. <paramref name="paint"/> repaints named slots (m_city_stone to a district's own tone, say).
        /// </summary>
        public static GameObject City(string piece, Transform parent, Vector3 position, float yaw = 180f, Vector3? scale = null,
            IDictionary<string, Material> paint = null, bool shadows = true)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(CityKitPath);
            var source = asset != null ? asset.transform.Find("fire_" + piece + "_lod0") : null;
            if (source == null)
            {
                Debug.LogError($"[Clover.Build] No piece named 'fire_{piece}_lod0' in {CityKitPath}.");
                return null;
            }
            var holder = new GameObject(piece);
            if (parent != null) holder.transform.SetParent(parent, false);
            holder.transform.SetPositionAndRotation(position, Quaternion.Euler(0f, yaw, 0f));
            holder.transform.localScale = scale ?? Vector3.one;
            var shown = Object.Instantiate(source.gameObject, holder.transform, false);
            shown.name = "Model";
            Dress(shown);
            foreach (var renderer in shown.GetComponentsInChildren<Renderer>())
            {
                if (paint != null)
                {
                    var slots = renderer.sharedMaterials;
                    for (int i = 0; i < slots.Length; i++)
                        if (slots[i] != null && paint.TryGetValue(slots[i].name, out var instead)) slots[i] = instead;
                    renderer.sharedMaterials = slots;
                }
                renderer.shadowCastingMode = shadows ? UnityEngine.Rendering.ShadowCastingMode.On : UnityEngine.Rendering.ShadowCastingMode.Off;
                renderer.gameObject.isStatic = true;
            }
            holder.isStatic = true;
            return holder;
        }

        /// <summary>A scenery material of the city kit's kind in a colour of a builder's own, for repainting a slot.</summary>
        public static Material CityTone(string name, Color colour, float outline = 0.008f) => Scenery(name, colour, "", outline);

        /// <summary>
        /// The city kit's slots for pieces that lie on the ground (paving, cracked mud, roofs, drifts, litter): no ink, so a
        /// floor reads as one surface worn into stones and not as a board of separately outlined tiles; the sand in the
        /// joints a shade darker than the sand that lies loose, so a missing slab reads as a gap. With a colour, the stone
        /// is that colour and the recut slabs a paler one (a builder's paving tones).
        /// </summary>
        public static Dictionary<string, Material> Ground(string name = "ground", Color? stoneColour = null, bool roof = false)
        {
            var paint = new Dictionary<string, Material>();
            var sandDark = new Color(0.64f, 0.56f, 0.43f);
            foreach (var slot in city)
            {
                Color colour = slot.Key == "sand" ? sandDark : slot.Value;
                if (stoneColour.HasValue && slot.Key == "stone") colour = stoneColour.Value;
                if (stoneColour.HasValue && slot.Key == "plaster") colour = Color.Lerp(stoneColour.Value, Color.white, 0.18f);
                // In paving the joints and the filled gaps sit close to the stone's own tone: a strong one makes a board of tiles.
                if (stoneColour.HasValue && slot.Key == "sand") colour = Color.Lerp(stoneColour.Value * 0.9f, sandDark, 0.25f);
                // A roof's repairs and its blown sand are near the plaster's tone: strong ones read as camouflage blots.
                if (roof && slot.Key == "mudbrick") colour = Color.Lerp(city["plaster"], slot.Value, 0.45f);
                if (roof && slot.Key == "sand") colour = Color.Lerp(city["plaster"], sandDark, 0.4f);
                var material = slot.Key == "gold" ? FloorGold : Scenery($"m_city_{name}_{slot.Key}", colour, slot.Key, 0f);
                // Sand lies in soft mounds whose slopes would drop into the middle band and read as dark spots: it stays
                // in the full light unless it is turned well away from the sun.
                if (slot.Key == "sand") material.SetFloat("_MidThreshold", 0.3f);
                paint["m_city_" + slot.Key] = material;
            }
            return paint;
        }

        /// <summary>
        /// The city kit's slots as materials that sway in the wind (clover_toon's _Sway: bend at a metre's height), for
        /// palms, reeds and hanging cloth: m_city_<paramref name="tag"/>_<slot>. Only pieces painted with these move.
        /// </summary>
        public static Dictionary<string, Material> Swaying(string tag, float sway)
        {
            var paint = new Dictionary<string, Material>();
            foreach (var slot in city)
            {
                float outline = slot.Key == "palm" || slot.Key == "linen" ? 0f : slot.Key == "reed" ? 0.004f : 0.008f;
                var material = Scenery($"m_city_{tag}_{slot.Key}", slot.Value, slot.Key, outline);
                material.SetFloat("_Sway", sway);
                EditorUtility.SetDirty(material);
                paint["m_city_" + slot.Key] = material;
            }
            return paint;
        }

        /// <summary>
        /// The four materials of a stretch of paving (<see cref="Clover.World.PavingLayout"/>) in a stone colour: the
        /// joints' bed (a shade under the stone, warmed by the sand in it, so joints read as thin lines), the stone, a
        /// shade darker, and the paler recut slabs. All carry the land's large wash of tone, laid from world space forty
        /// metres to a repeat, so a plaza is never one flat colour and its courses never show a repeat.
        /// </summary>
        public static Material[] PaveSet(string name, Color stone)
        {
            var mottle = ProceduralTextures.SunMottle();
            Material Make(string part, Color colour)
            {
                var material = BuildKit.Toon(Folder, $"m_city_pave_{name}_{part}", colour, false, mottle, 1f / 40f, 0f);
                return Worked(Light(material, "", 0.8f));
            }
            var sand = new Color(0.64f, 0.56f, 0.43f);
            return new[]
            {
                Make("bed", Color.Lerp(stone * 0.86f, sand, 0.18f)),
                Make("a", stone),
                Make("b", stone * 0.95f),
                Make("recut", Color.Lerp(stone, new Color(0.86f, 0.83f, 0.77f), 0.35f)),
            };
        }

        // ------------------------------------------------------------------ fire in the city

        /// <summary>
        /// A brazier of the city kit burning in the open (the overview, step 5): tongues of flame licking up out of the
        /// bowl over a bed of coals that glow through their cracks, embers spat up, a thread of smoke, and a warm light on
        /// the floor round it that flickers. Solid. With <paramref name="rest"/> the tall flames and the light are left to
        /// the rest point (out until rested at), but the coals glow and smoke from the start, so it reads as a fire banked
        /// for whoever comes, not as a dead prop.
        /// </summary>
        public static GameObject Brazier(Transform parent, Vector3 foot, float yaw = 180f, bool rest = false)
        {
            var holder = City("brazier", parent, foot, yaw);
            if (holder == null) return null;
            var model = holder.transform.Find("Model").gameObject;
            var box = model.AddComponent<BoxCollider>();
            var filter = model.GetComponent<MeshFilter>();
            box.center = filter.sharedMesh.bounds.center;
            box.size = Vector3.Scale(filter.sharedMesh.bounds.size, new Vector3(0.7f, 0.7f, 1f));
            // The particle tongues come first: a rest point finds its flames as the first particle system under it.
            BrazierFire(holder.transform, model);
            var bowl = holder.transform.position + Vector3.up * 1.76f;
            Coals(holder.transform, bowl, 0.5f);
            if (!rest)
            {
                Flames(holder.transform, bowl, 0.42f, 12, 0.85f, 0.5f, "brazier");
                Embers(holder.transform, bowl + Vector3.up * 0.2f, 0.35f, 7f);
                FireLight(holder.transform, bowl + Vector3.up * 0.6f, 7f, 2.6f);
            }
            else
            {
                Embers(holder.transform, bowl + Vector3.up * 0.1f, 0.3f, 1.5f);
            }
            return holder;
        }

        /// <summary>
        /// Something left burning low (the overview, step 5): a scorch on the ground with cracks still glowing, a carpet
        /// of small tongues on it, embers spat up, a thread of smoke leaning off, and, if asked for, a light that
        /// flickers on the floor and walls round it. <paramref name="radius"/> in metres, 0.4 to 2.5.
        /// </summary>
        public static Transform Smoulder(Transform parent, Vector3 at, float radius, bool light = true, float smoke = 1f, bool embers = true)
        {
            var group = new GameObject("Smoulder").transform;
            group.SetParent(parent, false);
            group.position = at;
            Scorch(group, at, radius * 1.35f, 0.75f);
            int count = Mathf.Clamp(Mathf.RoundToInt(radius * radius * 14f), 5, 48);
            Flames(group, at, radius, count, Mathf.Lerp(0.45f, 0.8f, Mathf.InverseLerp(0.4f, 2f, radius)), 0.55f, "smoulder");
            // Sparks are worth their cost only near the eye: far fires and fires on roofs go without (a particle system
            // is some fifty kilobytes of scene, and a draw).
            if (embers) Embers(group, at + Vector3.up * 0.2f, radius * 0.8f, 3f + radius * 5f);
            if (smoke > 0f) Smoke(group, at + Vector3.up * 0.5f, radius * 0.6f, smoke * (1.2f + radius));
            if (light) FireLight(group, at + Vector3.up * 1.1f, Mathf.Max(3.2f, radius * 2.8f), 1.6f + radius * 0.5f);
            return group;
        }

        /// <summary>
        /// A tall column of smoke going up from a fire seen from far (a beacon, a hearth in the lower town): dark puffs
        /// rising, leaning east with the wind and spreading. It keeps going off the screen, as something seen from afar.
        /// </summary>
        public static ParticleSystem SmokeColumn(Transform parent, Vector3 at, float scale)
        {
            var host = new GameObject("Smoke Column");
            host.transform.SetParent(parent, false);
            host.transform.SetPositionAndRotation(at, Quaternion.Euler(-90f, 0f, 0f));
            var system = Burning(host, BuildKit.Smoke(VfxBuilder.Folder, "m_vfx_smoke", new Color(0.85f, 0.8f, 0.76f, 0.75f)),
                2.2f * scale, 7f, 10f, 1.2f * scale, 2.2f * scale, 1.4f, 2.2f, 0.3f * scale, -0.02f);
            var main = system.main;
            main.startColor = new Color(0.26f, 0.23f, 0.22f, 0.45f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
            main.cullingMode = ParticleSystemCullingMode.AlwaysSimulate;
            var drift = system.velocityOverLifetime;
            drift.enabled = true;
            drift.space = ParticleSystemSimulationSpace.World;
            drift.x = new ParticleSystem.MinMaxCurve(0.6f, 1.2f);
            drift.y = new ParticleSystem.MinMaxCurve(0f, 0.2f);
            drift.z = new ParticleSystem.MinMaxCurve(0.1f, 0.4f);
            var grow = system.sizeOverLifetime;
            grow.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 0.7f, 1f, 3.2f));
            return system;
        }

        /// <summary>A small flame burning in a niche or a lamp: a few tongues, a glint of light on the wall round it.</summary>
        public static Transform Lamp(Transform parent, Vector3 at, bool light = true)
        {
            var group = new GameObject("Lamp Flame").transform;
            group.SetParent(parent, false);
            group.position = at;
            Flames(group, at, 0.08f, 3, 0.36f, 0.28f, "lamp");
            if (light) FireLight(group, at + Vector3.up * 0.25f, 3.4f, 1.3f);
            return group;
        }

        /// <summary>Char on the ground with its cracks glowing as much as <paramref name="heat"/> (0 cold char, 1 just burnt).</summary>
        public static Renderer Scorch(Transform parent, Vector3 centre, float radius, float heat)
        {
            int size = radius < 1.4f ? 1 : radius < 2.6f ? 2 : 4;
            string name = $"m_city_scorch_{size}_{Mathf.RoundToInt(heat * 10f)}";
            var material = BuildKit.Scorch(Folder, name);
            material.SetFloat("_Heat", heat);
            material.SetFloat("_Grown", 1f);
            material.SetVector("_Size", new Vector4(size, size, 0f, 0f));
            material.SetFloat("_Seed", size * 3.1f + heat * 7f);
            material.SetFloat("_Fade", 1f);
            material.SetFloat("_Disc", 1f);
            var scorch = new GameObject("Scorch");
            scorch.transform.SetParent(parent, false);
            scorch.transform.SetPositionAndRotation(centre + Vector3.up * 0.03f, Quaternion.Euler(0f, (centre.x * 37f + centre.z * 11f) % 360f, 0f));
            scorch.transform.localScale = new Vector3(radius, 1f, radius);
            scorch.AddComponent<MeshFilter>().sharedMesh = SavedMesh("scorch_disc", Clover.Gameplay.FlameField.DiscGround);
            var shown = scorch.AddComponent<MeshRenderer>();
            shown.sharedMaterial = material;
            shown.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            shown.receiveShadows = false;
            scorch.isStatic = true;
            return shown;
        }

        // Coals in a bowl: char whose cracks glow, laid over the model's dark coals.
        private static void Coals(Transform parent, Vector3 at, float radius)
        {
            var coals = Scorch(parent, at - Vector3.up * 0.02f, radius, 1f);
            coals.name = "Coals";
        }

        // A carpet of tongues of flame standing on a disc (clover_flame_field.shader): each tongue licks to its own time
        // in the shader, so nothing has to run for it to burn.
        private static void Flames(Transform parent, Vector3 at, float radius, int count, float height, float width, string kind)
        {
            string name = $"m_city_flames_{kind}";
            var material = BuildKit.FlameField(Folder, name);
            material.SetFloat("_Heat", 1f);
            material.SetFloat("_Grown", 1f);
            material.SetFloat("_Fade", 1f);
            material.SetFloat("_FlameHeight", height);
            material.SetFloat("_FlameWidth", width);
            material.SetFloat("_Wall", 0f);
            material.SetFloat("_Rise", 1f);
            int seed = 7 + count * 13;
            var flames = new GameObject("Flames");
            flames.transform.SetParent(parent, false);
            flames.transform.position = at;
            flames.transform.localScale = new Vector3(radius, 1f, radius) * (1f / Mathf.Max(0.0001f, parent.lossyScale.x));
            flames.AddComponent<MeshFilter>().sharedMesh = SavedMesh($"flames_disc_{count}", () => Clover.Gameplay.FlameField.Disc(count, seed));
            var shown = flames.AddComponent<MeshRenderer>();
            shown.sharedMaterial = material;
            shown.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            shown.receiveShadows = false;
        }

        // Sparks spat up from a fire: small, stretched by their speed, curling as they rise, cooling from yellow through
        // red to dark before they go out. Their brightness is the material's; the colour over their life only darkens it.
        private static void Embers(Transform parent, Vector3 at, float radius, float rate)
        {
            var host = new GameObject("Embers");
            host.transform.SetParent(parent, false);
            host.transform.SetPositionAndRotation(at, Quaternion.Euler(-90f, 0f, 0f));
            var material = BuildKit.Glow(Folder, "m_city_ember", new Color(3.2f, 1.7f, 0.5f, 1f), 0, false, 2.6f);
            var system = Burning(host, material, rate, 0.9f, 2.2f, 0.05f, 0.11f, 0.7f, 1.9f, radius, -0.06f);
            var main = system.main;
            main.maxParticles = 40;
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(1f, 0.92f, 0.6f, 1f), new Color(1f, 0.75f, 0.4f, 1f));
            var shape = system.shape;
            shape.angle = 24f;
            var noise = system.noise;
            noise.enabled = true;
            noise.strength = 0.55f;
            noise.frequency = 0.8f;
            noise.scrollSpeed = 0.4f;
            var fade = system.colorOverLifetime;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(new Color(1f, 0.95f, 0.7f), 0f), new GradientColorKey(new Color(1f, 0.38f, 0.1f), 0.45f), new GradientColorKey(new Color(0.25f, 0.06f, 0.03f), 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.6f), new GradientAlphaKey(0f, 1f) });
            fade.color = gradient;
            var renderer = host.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.07f;
            renderer.lengthScale = 1.6f;
        }

        // A thread of dark smoke rising and spreading, leaning a little with the air.
        private static void Smoke(Transform parent, Vector3 at, float radius, float rate)
        {
            var host = new GameObject("Smoke");
            host.transform.SetParent(parent, false);
            host.transform.SetPositionAndRotation(at, Quaternion.Euler(-80f, 20f, 0f));
            var system = Burning(host, BuildKit.Smoke(VfxBuilder.Folder, "m_vfx_smoke", new Color(0.85f, 0.8f, 0.76f, 0.75f)),
                rate, 2.2f, 3.6f, 0.5f, 1.0f, 0.5f, 1.0f, Mathf.Max(0.1f, radius), -0.1f);
            var main = system.main;
            // Dark and thin: a pale wide plume over the floor reads as a veil of fog, not as smoke.
            main.startColor = new Color(0.22f, 0.19f, 0.18f, 0.42f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, 6.28f);
            var grow = system.sizeOverLifetime;
            grow.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, 2.3f));
        }

        // A warm unshadowed light that flickers: the floor and the walls near a fire take its banded rings.
        private static void FireLight(Transform parent, Vector3 at, float range, float intensity)
        {
            var host = new GameObject("Fire Light");
            host.transform.SetParent(parent, false);
            host.transform.position = at;
            var light = host.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = new Color(1f, 0.46f, 0.1f);
            light.range = range;
            light.intensity = intensity;
            light.shadows = LightShadows.None;
            var flicker = host.AddComponent<Clover.World.FireFlicker>();
            flicker.Intensity = intensity;
        }

        // A mesh made by code, kept as an asset under a stable name so scenes can point at it.
        public static Mesh SavedMesh(string name, System.Func<Mesh> make)
        {
            string path = $"{Folder}/{name}.asset";
            var mesh = AssetDatabase.LoadAssetAtPath<Mesh>(path);
            if (mesh != null && madeThisBuild.Contains(name)) return mesh;
            var made = make();
            made.name = name;
            if (mesh == null)
            {
                AssetDatabase.CreateAsset(made, path);
                mesh = made;
            }
            else
            {
                // Rewritten in place, so what points at it keeps pointing.
                mesh.Clear();
                mesh.vertices = made.vertices;
                mesh.uv = made.uv;
                mesh.uv2 = made.uv2;
                mesh.colors = made.colors;
                mesh.triangles = made.triangles;
                mesh.bounds = made.bounds;
                EditorUtility.SetDirty(mesh);
                Object.DestroyImmediate(made);
            }
            madeThisBuild.Add(name);
            return mesh;
        }

        private static readonly HashSet<string> madeThisBuild = new HashSet<string>();

        /// <summary>Forgets which code-made meshes were written, so a new build writes them afresh.</summary>
        public static void BeginBuild() => madeThisBuild.Clear();

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

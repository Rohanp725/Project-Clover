using System;
using Clover.Core;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// Builds the scene's effects object: one particle emitter per <see cref="VfxKind"/>, each set up here in code, and
    /// the materials they and the slash arcs use. Colours are light values and go past white on purpose: the bloom turns
    /// the excess into glow, which is what makes the elements the only vivid thing in a muted world.
    /// </summary>
    public static class VfxBuilder
    {
        public const string Folder = BuildKit.GeneratedRoot + "/Vfx";

        public static Material SlashMaterial() =>
            BuildKit.Glow(Folder, "m_vfx_slash", Color.white, 2, true, 1.6f).Graphic(0.16f, 0.8f);

        /// <summary>A flat see-through material for marks on the ground, such as where an enemy's blow will land.</summary>
        /// <summary>The band a raised guard draws on the ground: brightest dead ahead, tapering to both ends, hard-edged.</summary>
        public static Material GuardArcMaterial() =>
            BuildKit.Glow(Folder, "m_vfx_guard_arc", Color.white, 1, true, 1f).Graphic(0.1f, 0.62f);

        public static Material MarkerMaterial() =>
            BuildKit.Glow(Folder, "m_vfx_marker", Color.white, 3, false, 1f);

        /// <summary>
        /// What spell forms are drawn in: the material of their solid shapes, and one material of light for each
        /// <see cref="Clover.Player.Forms.FormMark"/>, in the order of that list. The colour of each comes from the
        /// cast, so these are white.
        /// </summary>
        public static (Material solid, Material[] marks) FormMaterials()
        {
            var solid = BuildKit.MaterialAt(Folder, "m_spell_solid", "Clover/Spell");
            solid.SetColor("_Color", Color.white);
            solid.SetFloat("_Hot", 0f);
            solid.SetFloat("_OutlineWidth", 0.035f);
            solid.SetColor("_OutlineColor", BuildKit.CharacterInk);
            solid.enableInstancing = true;
            EditorUtility.SetDirty(solid);

            var lane = BuildKit.Glow(Folder, "m_spell_lane", Color.white, 14, true, 1f);
            // Six arrowheads along it, running toward its far end.
            lane.SetFloat("_NoiseScale", 6f);
            lane.SetVector("_NoiseSpeed", new Vector4(1.6f, 0f, 0f, 0f));
            EditorUtility.SetDirty(lane);
            var scorch = BuildKit.Glow(Folder, "m_spell_scorch", Color.white, 0, false, 1.3f, 0.45f);
            scorch.SetFloat("_NoiseScale", 5f);
            scorch.SetVector("_NoiseSpeed", Vector4.zero);
            EditorUtility.SetDirty(scorch);
            var marks = new[]
            {
                BuildKit.Glow(Folder, "m_spell_ring", Color.white, 4, true, 1f).Graphic(0.3f),
                BuildKit.Glow(Folder, "m_spell_disc", Color.white, 13, true, 1f),
                BuildKit.Glow(Folder, "m_spell_star", Color.white, 5, true, 1f).Graphic(0.12f, 0.45f),
                BuildKit.Glow(Folder, "m_spell_cross", Color.white, 9, true, 1f).Graphic(0.1f, 0.5f),
                BuildKit.Glow(Folder, "m_spell_crack", Color.white, 10, true, 1f).Graphic(0.2f, 0.7f),
                BuildKit.Glow(Folder, "m_spell_sigil", Color.white, 11, true, 1f).Graphic(0.3f),
                BuildKit.Glow(Folder, "m_spell_beam", Color.white, 12, true, 1f).Graphic(0.1f, 0.5f),
                lane,
                BuildKit.Glow(Folder, "m_spell_streak", Color.white, 1, true, 1f).Graphic(0.12f, 0.5f),
                scorch,
                BuildKit.Glow(Folder, "m_spell_brackets", Color.white, 8, true, 1f).Graphic(0.3f),
            };
            return (solid, marks);
        }

        public static Vfx Build()
        {
            var root = new GameObject("Effects");
            int kinds = Enum.GetValues(typeof(VfxKind)).Length;
            var emitters = new ParticleSystem[kinds];
            var counts = new int[kinds];

            void Add(VfxKind kind, int count, ParticleSystem emitter)
            {
                emitters[(int)kind] = emitter;
                counts[(int)kind] = count;
            }

            var warm = new Color(4.5f, 3.2f, 1.6f, 1f);
            var cold = new Color(2.4f, 3.4f, 5f, 1f);
            Add(VfxKind.HitSpark, 10, Sparks(root, "Hit Spark", Streak("m_vfx_spark_warm", warm), 26f, 6f, 15f, 0.1f, 0.26f, 0.05f, 0.11f, 2f));
            Add(VfxKind.HeavySpark, 24, Sparks(root, "Heavy Spark", Streak("m_vfx_spark_warm", warm), 38f, 8f, 22f, 0.14f, 0.4f, 0.06f, 0.15f, 2.5f));
            Add(VfxKind.GuardSpark, 9, Sparks(root, "Guard Spark", Streak("m_vfx_spark_cold", cold), 75f, 3f, 9f, 0.08f, 0.2f, 0.04f, 0.09f, 1f));
            Add(VfxKind.PerfectGuard, 30, Sparks(root, "Perfect Guard", Streak("m_vfx_spark_perfect", new Color(5f, 6.5f, 8f, 1f)), 180f, 8f, 20f, 0.18f, 0.45f, 0.06f, 0.14f, 0.5f));
            Add(VfxKind.Wound, 14, Sparks(root, "Wound", Streak("m_vfx_spark_wound", new Color(4f, 0.5f, 0.35f, 1f)), 34f, 4f, 11f, 0.12f, 0.3f, 0.05f, 0.12f, 3f));
            Add(VfxKind.Shock, 16, Sparks(root, "Shock", Streak("m_vfx_spark_shock", new Color(2.2f, 3.6f, 8f, 1f)), 180f, 6f, 18f, 0.06f, 0.18f, 0.04f, 0.1f, 0f));
            Add(VfxKind.Burst, 36, Sparks(root, "Burst", Streak("m_vfx_spark_burst", new Color(6f, 5.4f, 4f, 1f)), 180f, 7f, 24f, 0.2f, 0.55f, 0.07f, 0.18f, 1f));

            Add(VfxKind.Dust, 9, Puffs(root, "Dust", BuildKit.Glow(Folder, "m_vfx_dust", new Color(0.55f, 0.55f, 0.6f, 0.5f), 0, false, 1.2f, 0.5f),
                0.8f, 2.6f, 0.3f, 0.6f, 0.35f, 0.8f, 0f, 1.8f, flat: true));
            Add(VfxKind.Fire, 26, Puffs(root, "Fire", BuildKit.Flame(Folder, "m_vfx_flame", new Color(1.7f, 0.5f, 0.1f, 1f)),
                0.6f, 3.2f, 0.28f, 0.6f, 0.55f, 1.3f, -1.4f, 0.5f, flat: false, upright: true));
            Add(VfxKind.Embers, 12, Puffs(root, "Embers", BuildKit.Glow(Folder, "m_vfx_embers", new Color(6f, 2f, 0.3f, 1f), 0, true, 3f),
                0.3f, 1.6f, 0.6f, 1.4f, 0.04f, 0.1f, -0.6f, 1f, flat: false));
            Add(VfxKind.Heal, 16, Puffs(root, "Heal", BuildKit.Glow(Folder, "m_vfx_heal", new Color(1.6f, 4f, 1.1f, 1f), 0, true, 3f),
                0.2f, 0.9f, 0.6f, 1.1f, 0.06f, 0.14f, -1.6f, 1f, flat: false));
            Add(VfxKind.Ring, 1, Ring(root, "Ring", BuildKit.Glow(Folder, "m_vfx_ring", new Color(4f, 5.5f, 7f, 1f), 4, true, 1.5f).Graphic(0.3f)));
            Add(VfxKind.Impact, 1, Star(root, "Impact", BuildKit.Glow(Folder, "m_vfx_impact", new Color(6f, 5.6f, 4.2f, 1f), 5, true, 1f).Graphic(0.12f, 0.45f), 2.6f, 0.1f));

            Add(VfxKind.Glint, 1, Star(root, "Glint", BuildKit.Glow(Folder, "m_vfx_glint", new Color(5f, 5.5f, 6f, 1f), 5, true, 1f).Graphic(0.1f, 0.4f), 1.3f, 0.16f));
            Add(VfxKind.Danger, 1, Star(root, "Danger", BuildKit.Glow(Folder, "m_vfx_danger", new Color(7f, 0.5f, 0.3f, 1f), 5, true, 1f, 0f, true).Graphic(0.1f, 0.55f), 2.6f, 0.28f));
            Add(VfxKind.Clang, 1, Star(root, "Clang", BuildKit.Glow(Folder, "m_vfx_clang", new Color(3f, 4.5f, 7f, 1f), 5, true, 1f).Graphic(0.12f, 0.4f), 1.9f, 0.11f));

            // A body burning away. The flakes are thrown in the body's own colours, so their material is only brightness;
            // they are laid over what is behind them, as paint, so each keeps its colour.
            Add(VfxKind.Ash, 4, Puffs(root, "Ash", BuildKit.Glow(Folder, "m_vfx_ash", new Color(2.6f, 2.6f, 2.6f, 1f), 3, false, 1f),
                0.5f, 2.1f, 0.5f, 1.1f, 0.08f, 0.2f, -1.3f, 0.25f, flat: false));
            Add(VfxKind.Wisp, 12, Sparks(root, "Wisp", Streak("m_vfx_spark_wisp", new Color(1.6f, 3.2f, 8f, 1f)), 8f, 6f, 15f, 0.25f, 0.6f, 0.06f, 0.14f, -1f));
            Add(VfxKind.Bolt, 2, Bolt(root, "Bolt", BuildKit.Glow(Folder, "m_vfx_bolt", new Color(2.2f, 3.6f, 8f, 1f), 7, true, 1f).Graphic(0.42f, 0.78f), 1.5f, 4.4f, 0.17f));

            // The effects of the nine elements that have none of their own, and of anything else that wants a colour:
            // white light, thrown in whatever colour the caller names. The brightness is in the material; the colour is the particle's.
            Add(VfxKind.TintPuff, 14, Puffs(root, "Tint Puff", BuildKit.Glow(Folder, "m_vfx_tint_puff", new Color(2.6f, 2.6f, 2.6f, 0.9f), 0, true, 2.4f),
                0.5f, 2.6f, 0.3f, 0.7f, 0.3f, 0.75f, -1.2f, 0.35f, flat: false));
            Add(VfxKind.TintSpark, 14, Sparks(root, "Tint Spark", Streak("m_vfx_spark_tint", new Color(3.6f, 3.6f, 3.6f, 1f)), 180f, 6f, 18f, 0.1f, 0.3f, 0.05f, 0.12f, 0.5f));
            Add(VfxKind.TintRing, 1, Ring(root, "Tint Ring", BuildKit.Glow(Folder, "m_vfx_ring_tint", new Color(4.5f, 4.5f, 4.5f, 1f), 4, true, 1.5f).Graphic(0.3f)));
            // A critical blow: a gold star larger and longer than a heavy blow's.
            Add(VfxKind.Critical, 1, Star(root, "Critical", BuildKit.Glow(Folder, "m_vfx_critical", new Color(7f, 5f, 1.4f, 1f), 5, true, 1f).Graphic(0.1f, 0.5f), 3.4f, 0.17f));

            var vfx = root.AddComponent<Vfx>();
            vfx.Configure(emitters, counts);
            Shimmer(root);
            return vfx;
        }

        // Heat shimmer as drawn squiggles: a pool of thin wavy lines that rise off hot ground (HeatShimmer).
        private static void Shimmer(GameObject root)
        {
            var holder = new GameObject("Heat Shimmer");
            holder.transform.SetParent(root.transform, false);
            // Laid over the ground, not added to it: on the pale floors of the Fire land an added pale line would vanish.
            // Ember-tinted, so it shows on white-gold ground at the height of the blaze as well as on stone.
            var material = BuildKit.Glow(Folder, "m_vfx_shimmer", new Color(1.5f, 0.75f, 0.28f, 1f), 1, false, 0.7f);
            var lines = new LineRenderer[32];
            for (int i = 0; i < lines.Length; i++)
            {
                var lineObject = new GameObject("Squiggle " + (i + 1));
                lineObject.transform.SetParent(holder.transform, false);
                var line = lineObject.AddComponent<LineRenderer>();
                line.useWorldSpace = true;
                line.sharedMaterial = material;
                line.numCornerVertices = 2;
                line.alignment = LineAlignment.View;
                line.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                line.receiveShadows = false;
                line.enabled = false;
                lines[i] = line;
            }
            var shimmer = holder.AddComponent<HeatShimmer>();
            shimmer.Width = 0.18f;
            shimmer.Configure(lines);
        }

        private static Material Streak(string name, Color colour) => BuildKit.Glow(Folder, name, colour, 1, true, 1.3f).Graphic(0.14f, 0.6f);

        private static ParticleSystem New(GameObject root, string name, Material material, int max)
        {
            var emitterObject = new GameObject(name);
            emitterObject.transform.SetParent(root.transform, false);
            var system = emitterObject.AddComponent<ParticleSystem>();
            // A new particle system starts playing by default; these only ever throw bursts on request.
            system.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            var main = system.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 1f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.maxParticles = max;
            main.startColor = Color.white;
            var emission = system.emission;
            emission.enabled = false;
            var renderer = emitterObject.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = material;
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
            return system;
        }

        private static void FadeOut(ParticleSystem system, float shrinkTo)
        {
            var colour = system.colorOverLifetime;
            colour.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0.9f, 0.5f), new GradientAlphaKey(0f, 1f) });
            colour.color = gradient;
            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.Linear(0f, 1f, 1f, shrinkTo));
        }

        // Fast streaks thrown in a cone along the emitter's forward: sparks of every kind.
        private static ParticleSystem Sparks(GameObject root, string name, Material material, float coneDegrees,
            float speedMin, float speedMax, float lifeMin, float lifeMax, float sizeMin, float sizeMax, float gravity)
        {
            var system = New(root, name, material, 256);
            var main = system.main;
            main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
            main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            main.gravityModifier = gravity;
            var shape = system.shape;
            shape.enabled = true;
            if (coneDegrees >= 179f)
            {
                shape.shapeType = ParticleSystemShapeType.Sphere;
                shape.radius = 0.05f;
            }
            else
            {
                shape.shapeType = ParticleSystemShapeType.Cone;
                shape.angle = coneDegrees;
                shape.radius = 0.05f;
            }
            var drag = system.limitVelocityOverLifetime;
            drag.enabled = true;
            drag.drag = 3f;
            drag.limit = 100f;
            FadeOut(system, 0.2f);
            var renderer = system.GetComponent<ParticleSystemRenderer>();
            renderer.renderMode = ParticleSystemRenderMode.Stretch;
            renderer.velocityScale = 0.045f;
            renderer.lengthScale = 2.5f;
            return system;
        }

        // Soft blobs that drift: dust, flame, embers, healing motes. Negative gravity makes them rise.
        private static ParticleSystem Puffs(GameObject root, string name, Material material, float speedMin, float speedMax,
            float lifeMin, float lifeMax, float sizeMin, float sizeMax, float gravity, float growTo, bool flat, bool upright = false)
        {
            var system = New(root, name, material, 256);
            var main = system.main;
            main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
            main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            // Flames stand up, leaning a little; everything else may lie any way round.
            main.startRotation = upright ? new ParticleSystem.MinMaxCurve(-0.3f, 0.3f) : new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            // Each flame licks to its own time: the shader reads the red of its colour as its place in the pattern.
            if (upright) main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.75f, 1f, 1f, 1f), Color.white);
            main.gravityModifier = gravity;
            var shape = system.shape;
            shape.enabled = true;
            shape.shapeType = flat ? ParticleSystemShapeType.Hemisphere : ParticleSystemShapeType.Sphere;
            shape.radius = flat ? 0.3f : 0.35f;
            // The dust hemisphere opens upward whatever way the emitter is turned.
            if (flat) shape.rotation = new Vector3(-90f, 0f, 0f);
            var drag = system.limitVelocityOverLifetime;
            drag.enabled = true;
            drag.drag = 2f;
            drag.limit = 100f;
            FadeOut(system, growTo);
            return system;
        }

        // One spiked burst, there for an instant and gone: it does not fade so much as blink.
        private static ParticleSystem Star(GameObject root, string name, Material material, float starSize, float seconds)
        {
            var system = New(root, name, material, 16);
            var main = system.main;
            main.startSpeed = 0f;
            main.startLifetime = seconds;
            main.startSize = starSize;
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            // On real time, so it is seen through the stop that comes with the blow.
            main.useUnscaledTime = true;
            var shape = system.shape;
            shape.enabled = false;
            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.5f), new Keyframe(0.3f, 1f), new Keyframe(1f, 0.85f)));
            return system;
        }

        // One tall jagged stroke standing on the point, drawn upward, there for an instant: the lightning leaving.
        private static ParticleSystem Bolt(GameObject root, string name, Material material, float width, float height, float seconds)
        {
            var system = New(root, name, material, 16);
            var main = system.main;
            main.startSpeed = 0f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(seconds * 0.6f, seconds);
            main.startSize3D = true;
            main.startSizeX = width;
            main.startSizeY = height;
            main.startSizeZ = 1f;
            main.startRotation = 0f;
            // The shader reads the red of the colour as which kinks this bolt has, so two thrown together fork.
            main.startColor = new ParticleSystem.MinMaxGradient(new Color(0.2f, 1f, 1f, 1f), Color.white);
            var shape = system.shape;
            shape.enabled = false;
            // It stands on the point it is thrown at and does not straddle it.
            system.GetComponent<ParticleSystemRenderer>().pivot = new Vector3(0f, 0.5f, 0f);
            return system;
        }

        // One flat ring that widens and fades.
        private static ParticleSystem Ring(GameObject root, string name, Material material)
        {
            var system = New(root, name, material, 16);
            var main = system.main;
            main.startSpeed = 0f;
            main.startLifetime = 0.3f;
            main.startSize = 1f;
            var shape = system.shape;
            shape.enabled = false;
            var colour = system.colorOverLifetime;
            colour.enabled = true;
            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) });
            colour.color = gradient;
            var size = system.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(new Keyframe(0f, 0.6f), new Keyframe(0.35f, 2.6f), new Keyframe(1f, 3.6f)));
            return system;
        }
    }
}

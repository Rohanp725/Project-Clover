using System;
using Clover.Core;
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
        public static Material MarkerMaterial() =>
            BuildKit.Glow(Folder, "m_vfx_marker", Color.white, 3, false, 1f);

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
            Add(VfxKind.Fire, 26, Puffs(root, "Fire", BuildKit.Glow(Folder, "m_vfx_fire", new Color(5f, 1.7f, 0.3f, 1f), 0, true, 1.4f, 0.75f).Graphic(0.22f, 0.7f),
                0.8f, 4.5f, 0.25f, 0.6f, 0.35f, 0.9f, -1.2f, 0.4f, flat: false));
            Add(VfxKind.Embers, 12, Puffs(root, "Embers", BuildKit.Glow(Folder, "m_vfx_embers", new Color(6f, 2f, 0.3f, 1f), 0, true, 3f),
                0.3f, 1.6f, 0.6f, 1.4f, 0.04f, 0.1f, -0.6f, 1f, flat: false));
            Add(VfxKind.Heal, 16, Puffs(root, "Heal", BuildKit.Glow(Folder, "m_vfx_heal", new Color(1.6f, 4f, 1.1f, 1f), 0, true, 3f),
                0.2f, 0.9f, 0.6f, 1.1f, 0.06f, 0.14f, -1.6f, 1f, flat: false));
            Add(VfxKind.Ring, 1, Ring(root, "Ring", BuildKit.Glow(Folder, "m_vfx_ring", new Color(4f, 5.5f, 7f, 1f), 4, true, 1.5f).Graphic(0.3f)));
            Add(VfxKind.Impact, 1, Star(root, "Impact", BuildKit.Glow(Folder, "m_vfx_impact", new Color(6f, 5.6f, 4.2f, 1f), 5, true, 1f).Graphic(0.12f, 0.45f)));

            var vfx = root.AddComponent<Vfx>();
            vfx.Configure(emitters, counts);
            return vfx;
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
            float lifeMin, float lifeMax, float sizeMin, float sizeMax, float gravity, float growTo, bool flat)
        {
            var system = New(root, name, material, 256);
            var main = system.main;
            main.startSpeed = new ParticleSystem.MinMaxCurve(speedMin, speedMax);
            main.startLifetime = new ParticleSystem.MinMaxCurve(lifeMin, lifeMax);
            main.startSize = new ParticleSystem.MinMaxCurve(sizeMin, sizeMax);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
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
        private static ParticleSystem Star(GameObject root, string name, Material material)
        {
            var system = New(root, name, material, 16);
            var main = system.main;
            main.startSpeed = 0f;
            main.startLifetime = 0.1f;
            main.startSize = 2.6f;
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

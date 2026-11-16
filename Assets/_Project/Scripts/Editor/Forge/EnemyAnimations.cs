using System;
using System.Collections.Generic;
using Clover.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools.Forge
{
    /// <summary>
    /// Every animation of the castle's three enemies, written as poses like the player's. Clover > Forge > Bake Enemy
    /// Animations bakes them to Generated/Animation/Enemies and draws their contact sheets. Each attack reads its timing
    /// from the enemy's own numbers, and each is shaped to be read from above: a wind-up that holds long enough to be
    /// seen for what it is, and a blow that goes exactly where its mark on the ground said it would.
    /// </summary>
    public static class EnemyAnimations
    {
        public const string ClipFolder = BuildKit.GeneratedRoot + "/Animation/Enemies";
        private const string ModelFolder = "Assets/_Project/Art/Models/Characters";

        [MenuItem("Clover/Forge/Bake Enemy Animations")]
        public static void BakeAll() => Bake(null);

        public static void Bake(string filter)
        {
            int baked = 0;
            foreach (var kind in EnemyKind.All)
            {
                var asset = AssetDatabase.LoadAssetAtPath<GameObject>($"{ModelFolder}/{kind.ModelName}.fbx");
                if (asset == null)
                {
                    Debug.LogError($"[Clover.Forge] No model for {kind.Name}.");
                    continue;
                }
                using (var rig = new ForgeRig(asset))
                {
                    EnemyBuilder.Dress(rig.Model);
                    var tails = new ForgeTails()
                        .Add(rig, 0.1f, "scarf_a1", "scarf_a2", "scarf_a3")
                        .Add(rig, 0.1f, "scarf_b1", "scarf_b2")
                        .Add(rig, 0.14f, "band_a1", "band_a2")
                        .Add(rig, 0.14f, "band_b1", "band_b2")
                        .Add(rig, 0.2f, "pony1", "pony2", "pony3");
                    foreach (var clip in For(kind))
                    {
                        if (!string.IsNullOrEmpty(filter) && !clip.Name.Contains(filter)) continue;
                        clip.Bake(rig, ClipFolder, tails);
                        var local = clip;
                        tails.Reset(rig, local, 0f);
                        float last = 0f;
                        ForgeSheet.Render(clip.Name, rig.Model, t =>
                        {
                            int steps = Mathf.Max(1, Mathf.RoundToInt((t - last) * local.Seconds * 60f));
                            for (int i = 1; i <= steps; i++)
                            {
                                float at = Mathf.Lerp(last, t, i / (float)steps);
                                local.Pose(rig, at);
                                tails.Step(rig, 1f / 60f, local.Wind != null ? local.Wind(at) : Vector3.zero);
                            }
                            last = t;
                        }, 8, 1.05f, clip.SheetMoments);
                        baked++;
                    }
                }
            }
            AssetDatabase.SaveAssets();
            Debug.Log($"[Clover.Forge] Baked {baked} enemy clips to {ClipFolder}; sheets in {ForgeSheet.Folder}.");
        }

        public static IEnumerable<ForgeClip> For(EnemyKind kind)
        {
            switch (kind.Prefix)
            {
                case "soldier": return Soldier(kind);
                case "acolyte": return Acolyte(kind);
                case "boss": return BossAnimations.For(kind);
                default: return Caster(kind);
            }
        }

        private static EnemyAttack Named(EnemyKind kind, string name) => kind.Brain.Attacks.Find(a => a.Name == name);

        private static ForgeClip Attack(EnemyAttack attack, out float hit, out float done)
        {
            hit = attack.Telegraph / attack.Total;
            done = (attack.Telegraph + attack.Strike) / attack.Total;
            float h = hit, d = done;
            return new ForgeClip(attack.Name, attack.Total)
            {
                Shape = ForgeClip.Impact(h, attack.Total, 1.2f),
                Wind = t => new Vector3(0f, 0f, t > h * 0.8f && t < d ? -7f : 0f),
                SheetMoments = new[] { 0f, h * 0.4f, h * 0.9f, h, (h + d) * 0.5f, d, Mathf.Lerp(d, 1f, 0.4f), 1f },
            };
        }

        private static ForgePose Looking(ForgePose pose, float keep = 0.8f)
        {
            pose.HeadYaw = -(pose.HipYaw + pose.WaistYaw) * keep;
            return pose;
        }

        // The clips every kind has, made from its own ready stance: breathing, walking, flinching, kneeling, falling.
        private static IEnumerable<ForgeClip> Common(string prefix, Func<ForgePose> ready, float stride, float walkSeconds, ForgePose kneel) =>
            CommonClips(prefix, ready, stride, walkSeconds, kneel);

        public static IEnumerable<ForgeClip> CommonClips(string prefix, Func<ForgePose> ready, float stride, float walkSeconds, ForgePose kneel)
        {
            yield return new ForgeClip(prefix + "_idle_loop", 2.4f, true).From(t =>
            {
                float breath = Mathf.Sin(t * Mathf.PI * 2f), slow = Mathf.Sin(t * Mathf.PI * 2f + 1f);
                var p = ready();
                // Breathing that can be seen from above: the whole body rises and sinks, the weight drifts from foot to foot, the head turns.
                p.Hips += new Vector3(0.022f * slow, 0.028f * breath, 0.01f * slow);
                p.HipRoll += 2f * slow;
                p.WaistLean += 4f * breath;
                p.WaistYaw += 3f * slow;
                p.ShoulderLUp += 3f + 4f * breath;
                p.ShoulderRUp += 3f + 4f * breath;
                p.HeadYaw += 9f * slow;
                p.HeadLean -= 3f * breath;
                p.HandR += new Vector3(0.01f * slow, 0.025f * breath, 0.02f * slow);
                return p;
            });

            yield return new ForgeClip(prefix + "_walk_loop", walkSeconds, true) { Wind = _ => new Vector3(0f, 0.5f, -4f) }.From(t =>
            {
                float phase = t * Mathf.PI * 2f;
                float left = Mathf.Sin(phase), right = -left;
                float leftLift = Mathf.Max(0f, Mathf.Cos(phase)), rightLift = Mathf.Max(0f, -Mathf.Cos(phase));
                var p = ready();
                // The stance keeps its shape and the feet travel through it.
                Vector3 middle = (p.FootL + p.FootR) * 0.5f;
                float spread = Mathf.Abs(p.FootL.x - p.FootR.x) * 0.5f;
                p.FootL = new Vector3(-spread, 0.13f * leftLift, middle.z + stride * left);
                p.FootR = new Vector3(spread, 0.13f * rightLift, middle.z + stride * right);
                p.FootLTip = 20f * Mathf.Max(0f, -left);
                p.FootRTip = 20f * Mathf.Max(0f, -right);
                // A stalking walk told by the body, since from above the legs hardly show: it sways over each foot,
                // dips as the weight comes down, and the shoulders turn against the hips.
                p.Hips += new Vector3(0.045f * left, 0.04f * Mathf.Cos(phase * 2f) - 0.02f, 0.02f);
                p.HipLean += 5f;
                p.HipYaw += 11f * left;
                p.WaistYaw -= 20f * left;
                p.HipRoll += 5f * left;
                p.WaistRoll -= 3f * left;
                p.HeadYaw += 8f * left;
                p.HandR += new Vector3(0f, 0.03f * Mathf.Cos(phase * 2f), 0.07f * left);
                return p;
            });

            var struck = ready();
            struck.Hips += new Vector3(0.02f, -0.06f, -0.16f);
            struck.HipLean -= 14f;
            struck.WaistLean -= 14f;
            struck.WaistYaw += 24f;
            struck.WaistRoll = 10f;
            struck.HeadLean = -24f;
            struck.FootR = new Vector3(struck.FootR.x + 0.04f, 0f, struck.FootR.z - 0.2f);
            struck.ShoulderRUp = 14f;
            struck.ShoulderLUp = 10f;
            yield return new ForgeClip(prefix + "_hurt", 0.3f) { Wind = t => new Vector3(0f, 0f, t < 0.3f ? 9f : 0f), Shape = ForgeClip.Shudder(0.2f, 0.3f, 1.3f) }
                .Key(0f, ready()).Key(0.25f, struck, Ease.Snap).Key(0.55f, struck, Ease.Linear).Key(1f, ready(), Ease.InOut);

            // Staggered: the legs give and it drops to one knee, head down, wide open.
            var reel = struck.Copy();
            reel.Hips += new Vector3(0f, -0.06f, -0.06f);
            var sway = kneel.Copy();
            sway.WaistRoll = 6f;
            sway.HeadLean += 6f;
            yield return new ForgeClip(prefix + "_stagger", 0.55f) { SheetMoments = new[] { 0f, 0.15f, 0.3f, 0.45f, 0.6f, 0.75f, 0.9f, 1f } }
                .Key(0f, ready()).Key(0.2f, reel, Ease.Snap).Key(0.65f, kneel, Ease.In).Key(0.82f, sway, Ease.Out).Key(1f, kneel, Ease.InOut);

            var knees = kneel.Copy();
            knees.FootR = new Vector3(0.16f, 0f, -0.3f);
            knees.FootRTip = 70f;
            knees.FootL = new Vector3(-0.16f, 0f, -0.32f);
            knees.FootLTip = 70f;
            knees.Hips = new Vector3(0f, -0.54f, 0.02f);
            knees.HeadLean = 30f;
            var down = new ForgePose
            {
                Hips = new Vector3(0f, -0.84f, 0.2f),
                HipLean = 78f,
                WaistLean = 12f,
                HeadYaw = -50f,
                FootL = new Vector3(-0.18f, 0.02f, -0.62f),
                FootR = new Vector3(0.16f, 0.02f, -0.66f),
                FootLTip = 80f,
                FootRTip = 80f,
                KneeL = new Vector3(-0.4f, -1f, 0.2f),
                KneeR = new Vector3(0.4f, -1f, 0.2f),
                HandL = new Vector3(-0.42f, 0.07f, 0.7f),
                HandR = new Vector3(0.4f, 0.07f, 0.56f),
                ElbowL = new Vector3(-1f, 0.2f, 0f),
                ElbowR = new Vector3(1f, 0.2f, 0f),
                AimBlade = true,
                Blade = new Vector3(0.5f, 0.02f, 0.86f),
            };
            yield return new ForgeClip(prefix + "_death", 0.8f) { SheetMoments = new[] { 0f, 0.15f, 0.3f, 0.45f, 0.6f, 0.75f, 0.9f, 1f } }
                .Key(0f, ready()).Key(0.18f, reel, Ease.Snap).Key(0.5f, knees, Ease.In).Key(0.62f, knees, Ease.Linear).Key(0.92f, down, Ease.In).Key(1f, down, Ease.Linear);
        }

        // ------------------------------------------------------------------ temple soldier

        // Spear levelled in both hands, left side and left foot leading, as a line soldier is drilled to stand.
        private static ForgePose SoldierReady() => Looking(new ForgePose
        {
            Hips = new Vector3(0f, -0.14f, 0f),
            HipYaw = 34f,
            WaistYaw = 10f,
            WaistLean = 8f,
            FootL = new Vector3(-0.14f, 0f, 0.3f),
            FootR = new Vector3(0.22f, 0f, -0.24f),
            FootLTurn = -6f,
            FootRTurn = 48f,
            HandR = new Vector3(0.26f, 0.98f, -0.12f),
            ElbowR = new Vector3(0.7f, 0f, -1f),
            ElbowL = new Vector3(-0.8f, -0.6f, 0f),
            AimBlade = true,
            TwoHanded = true,
            GripGap = -0.5f,
            Blade = new Vector3(-0.16f, 0.14f, 0.98f),
        });

        private static IEnumerable<ForgeClip> Soldier(EnemyKind kind)
        {
            var kneel = new ForgePose
            {
                // Down on the right knee, held up by the spear planted in front of it.
                Hips = new Vector3(0f, -0.5f, 0f),
                HipLean = 10f,
                WaistLean = 26f,
                HeadLean = 30f,
                FootL = new Vector3(-0.16f, 0f, 0.26f),
                FootR = new Vector3(0.18f, 0f, -0.3f),
                FootRTip = 70f,
                KneeR = new Vector3(0.3f, -0.4f, 1f),
                HandR = new Vector3(0.24f, 1.0f, 0.34f),
                ElbowR = new Vector3(1f, -0.4f, 0f),
                ElbowL = new Vector3(-1f, -0.4f, 0f),
                AimBlade = true,
                TwoHanded = true,
                GripGap = 0.3f,
                Blade = new Vector3(-0.06f, 1f, 0.1f),
            };
            foreach (var clip in Common("soldier", SoldierReady, 0.2f, 0.9f, kneel)) yield return clip;

            // The thrust: the spear drawn back along its own line, held, and driven straight out with a step.
            {
                var clip = Attack(Named(kind, "soldier_thrust"), out float hit, out float done);
                var draw = SoldierReady();
                draw.Hips = new Vector3(0.02f, -0.2f, -0.14f);
                draw.HipYaw = 46f;
                draw.WaistYaw = 22f;
                draw.HandR = new Vector3(0.34f, 1.04f, -0.5f);
                draw.Blade = new Vector3(-0.2f, 0.04f, 0.98f);
                Looking(draw);
                var held = draw.Copy();
                held.Hips = new Vector3(0.02f, -0.23f, -0.17f);
                held.HandR = new Vector3(0.35f, 1.04f, -0.54f);
                var drive = SoldierReady();
                drive.Hips = new Vector3(-0.02f, -0.3f, 0.3f);
                drive.HipYaw = 4f;
                drive.HipLean = 14f;
                drive.WaistYaw = -18f;
                drive.WaistLean = 16f;
                drive.FootL = new Vector3(-0.14f, 0f, 0.7f);
                drive.FootR = new Vector3(0.22f, 0f, -0.36f);
                drive.FootRTip = 30f;
                drive.HandR = new Vector3(0.14f, 1.06f, 0.5f);
                drive.ShoulderRForward = 24f;
                drive.Blade = new Vector3(-0.04f, 0f, 1f);
                Looking(drive);
                var stuck = drive.Copy();
                stuck.Hips = new Vector3(-0.02f, -0.32f, 0.33f);
                stuck.HandR = new Vector3(0.14f, 1.05f, 0.54f);
                yield return clip.Key(0f, SoldierReady()).Key(hit * 0.45f, draw, Ease.Out).Key(hit * 0.88f, held, Ease.InOut).Key(hit, drive, Ease.In)
                    .Key(done, stuck, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.4f), stuck, Ease.Linear).Key(1f, SoldierReady(), Ease.InOut);
            }

            // The sweep: the spear carried round behind the left shoulder and scythed level from left to right.
            {
                var clip = Attack(Named(kind, "soldier_sweep"), out float hit, out float done);
                var wound = SoldierReady();
                wound.Hips = new Vector3(-0.04f, -0.2f, -0.06f);
                wound.HipYaw = -18f;
                wound.WaistYaw = -52f;
                wound.HandR = new Vector3(-0.1f, 1.16f, 0.12f);
                wound.ElbowR = new Vector3(0.8f, -0.3f, 0.4f);
                wound.GripGap = -0.45f;
                wound.Blade = new Vector3(-0.86f, 0.14f, -0.48f);
                Looking(wound, 0.7f);
                var held = wound.Copy();
                held.WaistYaw = -58f;
                held.Blade = new Vector3(-0.8f, 0.14f, -0.58f);
                var across = SoldierReady();
                across.Hips = new Vector3(0f, -0.24f, 0.1f);
                across.HipYaw = 18f;
                across.WaistYaw = 4f;
                across.HandR = new Vector3(0.2f, 1.08f, 0.3f);
                across.GripGap = -0.45f;
                across.Blade = new Vector3(-0.1f, 0f, 1f);
                Looking(across);
                var through = SoldierReady();
                through.Hips = new Vector3(0.04f, -0.24f, 0.1f);
                through.HipYaw = 44f;
                through.WaistYaw = 58f;
                through.HandR = new Vector3(0.52f, 1.06f, -0.1f);
                through.GripGap = -0.45f;
                through.Blade = new Vector3(0.9f, 0f, 0.06f);
                Looking(through, 0.6f);
                var settle = through.Copy();
                settle.WaistYaw = 50f;
                settle.Blade = new Vector3(0.94f, -0.06f, -0.1f);
                yield return clip.Key(0f, SoldierReady()).Key(hit * 0.5f, wound, Ease.Out).Key(hit * 0.86f, held, Ease.InOut).Key(hit, across, Ease.In)
                    .Key(done, through, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.35f), settle, Ease.Out).Key(1f, SoldierReady(), Ease.InOut);
            }

            // The red lunge: it sinks behind the spear, couches it under the arm, and charges the whole length of it.
            {
                var clip = Attack(Named(kind, "soldier_lunge"), out float hit, out float done);
                var couch = SoldierReady();
                couch.Hips = new Vector3(0.02f, -0.36f, -0.2f);
                couch.HipYaw = 40f;
                couch.HipLean = 10f;
                couch.WaistYaw = 20f;
                couch.WaistLean = 20f;
                couch.FootL = new Vector3(-0.16f, 0f, 0.36f);
                couch.FootR = new Vector3(0.24f, 0f, -0.42f);
                couch.HandR = new Vector3(0.32f, 0.9f, -0.5f);
                couch.Blade = new Vector3(-0.16f, 0.06f, 0.98f);
                Looking(couch);
                var shake = couch.Copy();
                shake.Hips = new Vector3(0.02f, -0.4f, -0.24f);
                shake.HandR = new Vector3(0.33f, 0.88f, -0.55f);
                var charge = SoldierReady();
                charge.Hips = new Vector3(-0.02f, -0.38f, 0.42f);
                charge.HipYaw = -4f;
                charge.HipLean = 22f;
                charge.WaistYaw = -24f;
                charge.WaistLean = 18f;
                charge.FootL = new Vector3(-0.15f, 0f, 0.86f);
                charge.FootR = new Vector3(0.22f, 0f, -0.46f);
                charge.FootRTip = 40f;
                charge.HandR = new Vector3(0.12f, 1.0f, 0.62f);
                charge.ShoulderRForward = 28f;
                charge.Blade = new Vector3(-0.02f, 0.02f, 1f);
                Looking(charge);
                var stuck = charge.Copy();
                stuck.Hips = new Vector3(-0.02f, -0.4f, 0.46f);
                yield return clip.Key(0f, SoldierReady()).Key(hit * 0.4f, couch, Ease.Out).Key(hit * 0.9f, shake, Ease.InOut).Key(hit, charge, Ease.In)
                    .Key(done, stuck, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), stuck, Ease.Linear).Key(1f, SoldierReady(), Ease.InOut);
            }
        }

        // ------------------------------------------------------------------ knife acolyte

        // Crouched low and forward, knives wide: all of it ready to spring.
        private static ForgePose AcolyteReady() => new ForgePose
        {
            Hips = new Vector3(0f, -0.3f, 0.02f),
            HipLean = 18f,
            WaistLean = 16f,
            HeadLean = -30f,
            FootL = new Vector3(-0.22f, 0f, 0.14f),
            FootR = new Vector3(0.22f, 0f, -0.14f),
            FootLTurn = 20f,
            FootRTurn = 24f,
            FootRTip = 16f,
            HandL = new Vector3(-0.44f, 0.82f, 0.3f),
            HandR = new Vector3(0.44f, 0.82f, 0.26f),
            ElbowL = new Vector3(-1f, 0.2f, -0.6f),
            ElbowR = new Vector3(1f, 0.2f, -0.6f),
            AimBlade = true,
            Blade = new Vector3(0.3f, -0.1f, 0.95f),
            HandLBend = -20f,
        };

        private static IEnumerable<ForgeClip> Acolyte(EnemyKind kind)
        {
            var kneel = new ForgePose
            {
                Hips = new Vector3(0f, -0.52f, 0f),
                HipLean = 14f,
                WaistLean = 30f,
                HeadLean = 30f,
                FootL = new Vector3(-0.18f, 0f, 0.22f),
                FootR = new Vector3(0.18f, 0f, -0.3f),
                FootRTip = 70f,
                KneeR = new Vector3(0.3f, -0.4f, 1f),
                HandL = new Vector3(-0.32f, 0.12f, 0.42f),
                HandR = new Vector3(0.36f, 0.3f, 0.3f),
                AimBlade = true,
                Blade = new Vector3(0.4f, -0.6f, 0.7f),
            };
            foreach (var clip in Common("acolyte", AcolyteReady, 0.34f, 0.5f, kneel)) yield return clip;

            // First cut: the right knife from wide on the right, across to the left.
            {
                var clip = Attack(Named(kind, "acolyte_slash_1"), out float hit, out float done);
                var wound = AcolyteReady();
                wound.HipYaw = 30f;
                wound.WaistYaw = 40f;
                wound.HandR = new Vector3(0.62f, 1.1f, -0.1f);
                wound.Blade = new Vector3(0.9f, 0.1f, -0.4f);
                Looking(wound, 0.7f);
                var cut = AcolyteReady();
                cut.Hips = new Vector3(0f, -0.32f, 0.16f);
                cut.HipYaw = -6f;
                cut.WaistYaw = -10f;
                cut.HandR = new Vector3(0.12f, 1.06f, 0.7f);
                cut.Blade = new Vector3(0.2f, 0f, 0.98f);
                Looking(cut);
                var through = cut.Copy();
                through.HipYaw = -30f;
                through.WaistYaw = -54f;
                through.HandR = new Vector3(-0.46f, 1.0f, 0.3f);
                through.Blade = new Vector3(-0.92f, 0f, 0.2f);
                through.HandL = new Vector3(-0.56f, 1.0f, -0.16f);
                Looking(through, 0.6f);
                yield return clip.Key(0f, AcolyteReady()).Key(hit * 0.6f, wound, Ease.Out).Key(hit * 0.9f, wound, Ease.Linear).Key(hit, cut, Ease.In)
                    .Key(done, through, Ease.Out).Key(1f, through, Ease.Linear);
            }

            // Second cut, straight out of the first: the left knife back the other way.
            {
                var clip = Attack(Named(kind, "acolyte_slash_2"), out float hit, out float done);
                var start = AcolyteReady();
                start.Hips = new Vector3(0f, -0.32f, 0.16f);
                start.HipYaw = -30f;
                start.WaistYaw = -54f;
                start.HandR = new Vector3(-0.46f, 1.0f, 0.3f);
                start.Blade = new Vector3(-0.92f, 0f, 0.2f);
                start.HandL = new Vector3(-0.56f, 1.0f, -0.16f);
                Looking(start, 0.6f);
                var cut = start.Copy();
                cut.Hips = new Vector3(0f, -0.34f, 0.24f);
                cut.HipYaw = 6f;
                cut.WaistYaw = 10f;
                cut.HandL = new Vector3(-0.1f, 1.04f, 0.72f);
                cut.HandLBend = -60f;
                cut.HandR = new Vector3(0.36f, 0.9f, 0.1f);
                cut.Blade = new Vector3(0.5f, -0.3f, 0.8f);
                Looking(cut);
                var through = cut.Copy();
                through.HipYaw = 32f;
                through.WaistYaw = 56f;
                through.HandL = new Vector3(0.46f, 1.0f, 0.26f);
                through.HandR = new Vector3(0.56f, 0.9f, -0.2f);
                Looking(through, 0.6f);
                yield return clip.Key(0f, start).Key(hit, cut, Ease.In).Key(done, through, Ease.Out)
                    .Key(Mathf.Lerp(done, 1f, 0.5f), through, Ease.Linear).Key(1f, AcolyteReady(), Ease.InOut);
            }

            // The red leap: it folds down like a spring, goes up and over, and comes down with both knives.
            {
                var clip = Attack(Named(kind, "acolyte_leap"), out float hit, out float done);
                var crouch = AcolyteReady();
                crouch.Hips = new Vector3(0f, -0.52f, -0.12f);
                crouch.HipLean = 30f;
                crouch.WaistLean = 24f;
                crouch.HandL = new Vector3(-0.4f, 0.5f, -0.3f);
                crouch.HandR = new Vector3(0.4f, 0.5f, -0.3f);
                crouch.Blade = new Vector3(0.2f, -0.3f, -0.93f);
                var air = new ForgePose
                {
                    Hips = new Vector3(0f, 0.36f, 0.2f),
                    HipLean = 34f,
                    WaistLean = -20f,
                    HeadLean = -20f,
                    FootL = new Vector3(-0.18f, 0.62f, -0.36f),
                    FootR = new Vector3(0.18f, 0.5f, -0.5f),
                    FootLTip = 50f,
                    FootRTip = 60f,
                    HandL = new Vector3(-0.3f, 2.0f, 0.2f),
                    HandR = new Vector3(0.3f, 2.02f, 0.2f),
                    ElbowL = new Vector3(-1f, 0.4f, 0.3f),
                    ElbowR = new Vector3(1f, 0.4f, 0.3f),
                    ShoulderLUp = 26f,
                    ShoulderRUp = 26f,
                    AimBlade = true,
                    Blade = new Vector3(0f, 0.7f, -0.7f),
                    HandLBend = 40f,
                };
                var stab = new ForgePose
                {
                    Hips = new Vector3(0f, -0.44f, 0.3f),
                    HipLean = 36f,
                    WaistLean = 26f,
                    HeadLean = -34f,
                    FootL = new Vector3(-0.22f, 0f, 0.5f),
                    FootR = new Vector3(0.22f, 0f, -0.3f),
                    FootRTip = 40f,
                    HandL = new Vector3(-0.14f, 0.5f, 0.86f),
                    HandR = new Vector3(0.14f, 0.5f, 0.86f),
                    AimBlade = true,
                    Blade = new Vector3(0f, -0.8f, 0.6f),
                    HandLBend = -70f,
                };
                var held = stab.Copy();
                held.Hips = new Vector3(0f, -0.48f, 0.32f);
                clip.Wind = t => new Vector3(0f, t > hit * 0.7f && t < hit ? -5f : 0f, t > hit * 0.6f && t < done ? -8f : 0f);
                yield return clip.Key(0f, AcolyteReady()).Key(hit * 0.4f, crouch, Ease.Out).Key(hit * 0.7f, crouch, Ease.Linear).Key(hit * 0.9f, air, Ease.Snap)
                    .Key(hit, stab, Ease.In).Key(done, held, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), held, Ease.Linear).Key(1f, AcolyteReady(), Ease.InOut);
            }
        }

        // ------------------------------------------------------------------ storm caller

        // Upright and still, the staff planted beside it with its head high, the free hand half raised.
        private static ForgePose CasterReady() => new ForgePose
        {
            Hips = new Vector3(0f, -0.03f, 0f),
            WaistLean = -2f,
            FootL = new Vector3(-0.12f, 0f, 0f),
            FootR = new Vector3(0.12f, 0f, 0.02f),
            HandL = new Vector3(-0.34f, 1.05f, 0.2f),
            ElbowL = new Vector3(-0.8f, -0.6f, -0.3f),
            HandLBend = -30f,
            HandR = new Vector3(0.36f, 1.12f, 0.22f),
            ElbowR = new Vector3(0.8f, -0.6f, -0.3f),
            AimBlade = true,
            Blade = new Vector3(0.04f, 1f, 0.06f),
        };

        private static IEnumerable<ForgeClip> Caster(EnemyKind kind)
        {
            var kneel = new ForgePose
            {
                Hips = new Vector3(0f, -0.48f, 0f),
                HipLean = 8f,
                WaistLean = 28f,
                HeadLean = 30f,
                FootL = new Vector3(-0.14f, 0f, 0.2f),
                FootR = new Vector3(0.16f, 0f, -0.28f),
                FootRTip = 70f,
                KneeR = new Vector3(0.3f, -0.4f, 1f),
                HandL = new Vector3(-0.3f, 0.2f, 0.4f),
                HandR = new Vector3(0.3f, 0.95f, 0.36f),
                AimBlade = true,
                Blade = new Vector3(0.1f, 1f, -0.1f),
            };
            foreach (var clip in Common("caster", CasterReady, 0.12f, 1.1f, kneel)) yield return clip;

            // The bolt: staff and free hand lifted to the sky, held there shaking, then the staff brought down at the target.
            {
                var clip = Attack(Named(kind, "caster_bolt"), out float hit, out float done);
                var call = CasterReady();
                call.Hips = new Vector3(0f, 0.01f, -0.04f);
                call.WaistLean = -14f;
                call.HeadLean = -26f;
                call.HandR = new Vector3(0.3f, 1.9f, 0.1f);
                call.HandL = new Vector3(-0.34f, 1.86f, 0.14f);
                call.ElbowL = new Vector3(-1f, 0.3f, 0.2f);
                call.ElbowR = new Vector3(1f, 0.3f, 0.2f);
                call.ShoulderRUp = 28f;
                call.ShoulderLUp = 28f;
                call.Blade = new Vector3(0f, 1f, -0.12f);
                var strain = call.Copy();
                strain.Hips = new Vector3(0f, 0.02f, -0.06f);
                strain.HandR = new Vector3(0.28f, 1.96f, 0.06f);
                strain.HandL = new Vector3(-0.38f, 1.9f, 0.1f);
                strain.WaistLean = -18f;
                var point = CasterReady();
                point.Hips = new Vector3(0f, -0.14f, 0.1f);
                point.HipLean = 8f;
                point.WaistLean = 20f;
                point.HeadLean = -16f;
                point.FootL = new Vector3(-0.14f, 0f, 0.3f);
                point.HandR = new Vector3(0.18f, 1.16f, 0.66f);
                point.Blade = new Vector3(0f, 0.16f, 0.98f);
                point.HandL = new Vector3(-0.4f, 0.96f, -0.2f);
                var held = point.Copy();
                held.Hips = new Vector3(0f, -0.16f, 0.12f);
                yield return clip.Key(0f, CasterReady()).Key(hit * 0.35f, call, Ease.Out).Key(hit * 0.92f, strain, Ease.InOut).Key(hit, point, Ease.In)
                    .Key(done, held, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.45f), held, Ease.Linear).Key(1f, CasterReady(), Ease.InOut);
            }

            // The burst: the staff swung up level over the head in both hands, and its foot slammed into the ground.
            {
                var clip = Attack(Named(kind, "caster_nova"), out float hit, out float done);
                var lift = CasterReady();
                lift.Hips = new Vector3(0f, 0.02f, -0.04f);
                lift.WaistLean = -12f;
                lift.HandR = new Vector3(0.12f, 2.0f, 0.1f);
                lift.TwoHanded = true;
                lift.GripGap = 0.4f;
                lift.ElbowL = new Vector3(-1f, 0.3f, 0.2f);
                lift.ElbowR = new Vector3(1f, 0.3f, 0.2f);
                lift.ShoulderRUp = 28f;
                lift.ShoulderLUp = 28f;
                lift.Blade = new Vector3(0f, 1f, 0f);
                var top = lift.Copy();
                top.HandR = new Vector3(0.12f, 2.08f, 0.06f);
                var slam = CasterReady();
                slam.Hips = new Vector3(0f, -0.3f, 0.06f);
                slam.HipLean = 12f;
                slam.WaistLean = 26f;
                slam.HeadLean = -10f;
                slam.FootL = new Vector3(-0.2f, 0f, 0.1f);
                slam.FootR = new Vector3(0.2f, 0f, -0.06f);
                slam.HandR = new Vector3(0.1f, 1.02f, 0.4f);
                slam.TwoHanded = true;
                slam.GripGap = 0.3f;
                slam.Blade = new Vector3(0f, 1f, -0.06f);
                var held = slam.Copy();
                held.Hips = new Vector3(0f, -0.33f, 0.06f);
                yield return clip.Key(0f, CasterReady()).Key(hit * 0.5f, lift, Ease.Out).Key(hit * 0.9f, top, Ease.InOut).Key(hit, slam, Ease.In)
                    .Key(done, held, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), held, Ease.Linear).Key(1f, CasterReady(), Ease.InOut);
            }
        }
    }
}

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
                    EnemyBuilder.Dress(rig.Model, kind.Palette);
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
                case "captain": return CaptainAnimations.For(kind);
                case "thirst":
                case "wick":
                case "gilded":
                case "sunblind": return FireAnimations.For(kind);
                default: return Caster(kind);
            }
        }

        private static EnemyAttack Named(EnemyKind kind, string name) => kind.Brain.Attacks.Find(a => a.Name == name);

        public static ForgeClip AttackClip(EnemyAttack attack, out float hit, out float done) => Attack(attack, out hit, out done);

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

            // The fall, staged so that no part of it stands still: thrown back by the blow, lurching forward again as
            // the legs go, down onto the knees, slumping there, and then over to lie full length, slantwise to the
            // camera so that from above it is a body laid out.
            var lurch = reel.Copy();
            lurch.Hips += new Vector3(-0.05f, -0.1f, 0.12f);
            lurch.HipLean += 16f;
            lurch.WaistLean += 30f;
            lurch.WaistYaw -= 34f;
            lurch.WaistRoll = -8f;
            lurch.HeadLean = 14f;
            var knees = kneel.Copy();
            knees.FootR = new Vector3(0.16f, 0f, -0.3f);
            knees.FootRTip = 70f;
            knees.FootL = new Vector3(-0.16f, 0f, -0.32f);
            knees.FootLTip = 70f;
            knees.Hips = new Vector3(0f, -0.54f, 0.02f);
            knees.HipYaw = -10f;
            knees.HeadLean = 30f;
            var slump = knees.Copy();
            slump.Hips = new Vector3(-0.03f, -0.57f, -0.04f);
            slump.HipLean -= 8f;
            slump.HipYaw = -20f;
            slump.WaistLean += 10f;
            slump.WaistRoll = 9f;
            slump.HeadLean = 46f;
            var down = new ForgePose
            {
                Hips = new Vector3(-0.06f, -0.84f, 0.16f),
                HipLean = 80f,
                HipYaw = -35f,
                WaistLean = 8f,
                HeadYaw = 40f,
                FootL = new Vector3(0.32f, 0.02f, -0.61f),
                FootR = new Vector3(0.52f, 0.02f, -0.47f),
                FootLTurn = 35f,
                FootRTurn = -35f,
                FootLTip = 80f,
                FootRTip = 80f,
                KneeL = new Vector3(-0.2f, -1f, 0.1f),
                KneeR = new Vector3(0.3f, -1f, 0.2f),
                HandL = new Vector3(-0.74f, 0.07f, 0.41f),
                HandR = new Vector3(-0.27f, 0.07f, 1.17f),
                ElbowL = new Vector3(-0.82f, 0.2f, -0.57f),
                ElbowR = new Vector3(0.82f, 0.2f, 0.57f),
                AimBlade = true,
                Blade = new Vector3(-0.3f, 0.02f, 0.95f),
            };
            var bounce = down.Copy();
            bounce.Hips = new Vector3(-0.06f, -0.8f, 0.17f);
            bounce.HipLean = 76f;
            bounce.HeadLean = -12f;
            yield return new ForgeClip(prefix + "_death", 0.8f) { SheetMoments = new[] { 0f, 0.14f, 0.3f, 0.5f, 0.62f, 0.76f, 0.9f, 1f } }
                .Key(0f, ready()).Key(0.14f, reel, Ease.Snap).Key(0.3f, lurch, Ease.InOut).Key(0.5f, knees, Ease.In).Key(0.62f, slump, Ease.Out)
                .Key(0.9f, down, Ease.In).Key(0.95f, bounce, Ease.Out).Key(1f, down, Ease.In);
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

            // The thrust: the rear hand pulled to the hip with the shoulders wound back, then the spear shot out through
            // the leading hand with the head behind it, and hauled back hand over hand.
            {
                var clip = Attack(Named(kind, "soldier_thrust"), out float hit, out float done);
                var draw = SoldierReady();
                draw.Hips = new Vector3(0.05f, -0.2f, -0.16f);
                draw.HipYaw = 52f;
                draw.WaistYaw = 34f;
                draw.WaistLean = 2f;
                draw.WaistRoll = -6f;
                draw.HandR = new Vector3(0.4f, 0.92f, -0.46f);
                draw.ElbowR = new Vector3(0.8f, 0.3f, -1f);
                draw.ShoulderRUp = 10f;
                draw.GripGap = -0.62f;
                draw.Blade = new Vector3(-0.3f, 0.1f, 0.95f);
                Looking(draw);
                var held = draw.Copy();
                held.Hips = new Vector3(0.07f, -0.28f, -0.26f);
                held.WaistYaw = 46f;
                held.HandR = new Vector3(0.44f, 0.88f, -0.6f);
                Looking(held);
                var drive = SoldierReady();
                drive.Hips = new Vector3(-0.04f, -0.28f, 0.32f);
                drive.HipYaw = 2f;
                drive.HipLean = 16f;
                drive.WaistYaw = -34f;
                drive.WaistLean = 22f;
                drive.WaistRoll = 6f;
                drive.FootL = new Vector3(-0.14f, 0f, 0.72f);
                drive.FootR = new Vector3(0.22f, 0f, -0.3f);
                drive.FootRTip = 34f;
                drive.HandR = new Vector3(-0.04f, 1.12f, 0.78f);
                drive.ElbowR = new Vector3(1f, -0.3f, 0f);
                drive.ShoulderRForward = 30f;
                drive.GripGap = -0.24f;
                drive.Blade = new Vector3(-0.02f, -0.02f, 1f);
                Looking(drive);
                drive.HeadLean = 12f;
                var stuck = drive.Copy();
                stuck.Hips = new Vector3(-0.05f, -0.37f, 0.43f);
                stuck.WaistYaw = -48f;
                stuck.WaistLean = 28f;
                stuck.HandR = new Vector3(-0.08f, 1.04f, 0.94f);
                Looking(stuck);
                stuck.HeadLean = 6f;
                // The pull: the body comes up and back off the front foot and the shaft runs back through the left hand.
                var pull = SoldierReady();
                pull.Hips = new Vector3(0.03f, -0.1f, -0.06f);
                pull.HipYaw = 44f;
                pull.WaistYaw = 24f;
                pull.WaistLean = 0f;
                pull.FootL = new Vector3(-0.14f, 0f, 0.42f);
                pull.HandR = new Vector3(0.38f, 1.06f, -0.4f);
                pull.GripGap = -0.6f;
                pull.Blade = new Vector3(-0.24f, 0.2f, 0.95f);
                Looking(pull);
                yield return clip.Key(0f, SoldierReady()).Key(hit * 0.45f, draw, Ease.Out).Key(hit * 0.88f, held, Ease.InOut).Key(hit, drive, Ease.In)
                    .Key(done, stuck, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), pull, Ease.InOut).Key(1f, SoldierReady(), Ease.InOut);
            }

            // The sweep: the spear carried round behind the left shoulder with the whole back turned to wind it, and
            // scythed level from left to right, the shoulders unwinding through the blow and on past it.
            {
                var clip = Attack(Named(kind, "soldier_sweep"), out float hit, out float done);
                var wound = SoldierReady();
                wound.Hips = new Vector3(-0.06f, -0.08f, -0.1f);
                wound.HipYaw = -30f;
                wound.WaistYaw = -58f;
                wound.WaistLean = 0f;
                wound.WaistRoll = 8f;
                wound.HandR = new Vector3(-0.18f, 1.3f, 0.1f);
                wound.ElbowR = new Vector3(0.8f, -0.3f, 0.4f);
                wound.ShoulderRUp = 12f;
                wound.GripGap = -0.45f;
                wound.Blade = new Vector3(-0.8f, 0.25f, -0.55f);
                Looking(wound, 0.7f);
                var held = wound.Copy();
                held.Hips = new Vector3(-0.08f, -0.16f, -0.14f);
                held.WaistYaw = -68f;
                held.Blade = new Vector3(-0.66f, 0.22f, -0.72f);
                Looking(held, 0.7f);
                var across = SoldierReady();
                across.Hips = new Vector3(0f, -0.32f, 0.14f);
                across.HipYaw = 20f;
                across.WaistYaw = 10f;
                across.WaistLean = 16f;
                across.HandR = new Vector3(0.16f, 1.0f, 0.36f);
                across.GripGap = -0.45f;
                across.Blade = new Vector3(-0.15f, -0.02f, 1f);
                Looking(across);
                var through = SoldierReady();
                through.Hips = new Vector3(0.08f, -0.3f, 0.1f);
                through.HipYaw = 48f;
                through.WaistYaw = 50f;
                through.WaistRoll = -8f;
                through.HandR = new Vector3(0.56f, 1.0f, -0.06f);
                through.GripGap = -0.45f;
                through.Blade = new Vector3(0.95f, -0.05f, 0.1f);
                Looking(through, 0.55f);
                var past = through.Copy();
                past.Hips = new Vector3(0.1f, -0.2f, 0.04f);
                past.HipYaw = 56f;
                past.WaistYaw = 62f;
                past.WaistRoll = -4f;
                past.HandR = new Vector3(0.6f, 0.94f, -0.3f);
                past.Blade = new Vector3(0.7f, -0.16f, -0.66f);
                Looking(past, 0.5f);
                yield return clip.Key(0f, SoldierReady()).Key(hit * 0.5f, wound, Ease.Out).Key(hit * 0.86f, held, Ease.InOut).Key(hit, across, Ease.In)
                    .Key(done, through, Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.4f), past, Ease.Out).Key(1f, SoldierReady(), Ease.InOut);
            }

            // The red lunge: it sinks behind the spear, couches it under the arm, and charges the whole length of it,
            // laid out long and low, then skids to a stop and climbs back up.
            {
                var clip = Attack(Named(kind, "soldier_lunge"), out float hit, out float done);
                var couch = SoldierReady();
                couch.Hips = new Vector3(0.04f, -0.42f, -0.22f);
                couch.HipYaw = 30f;
                couch.HipLean = 16f;
                couch.WaistYaw = 4f;
                couch.WaistLean = 30f;
                couch.FootL = new Vector3(-0.18f, 0f, 0.34f);
                couch.FootR = new Vector3(0.26f, 0f, -0.44f);
                couch.HandR = new Vector3(0.3f, 0.72f, -0.2f);
                couch.ElbowR = new Vector3(0.5f, 0.2f, -1f);
                couch.GripGap = -0.4f;
                couch.Blade = new Vector3(-0.1f, -0.06f, 0.99f);
                Looking(couch);
                couch.HeadLean = -30f;
                var shake = couch.Copy();
                shake.Hips = new Vector3(0.04f, -0.47f, -0.27f);
                shake.WaistLean = 34f;
                shake.HandR = new Vector3(0.31f, 0.68f, -0.24f);
                var charge = SoldierReady();
                charge.Hips = new Vector3(0f, -0.46f, 0.52f);
                charge.HipYaw = 16f;
                charge.HipLean = 36f;
                charge.WaistYaw = -4f;
                charge.WaistLean = 30f;
                charge.FootL = new Vector3(-0.16f, 0f, 1.0f);
                charge.FootR = new Vector3(0.22f, 0f, -0.28f);
                charge.FootRTip = 60f;
                charge.FootRTurn = 20f;
                charge.HandR = new Vector3(0.26f, 0.74f, 0.74f);
                charge.ElbowR = new Vector3(0.5f, 0.2f, -1f);
                charge.GripGap = -0.4f;
                charge.Blade = new Vector3(-0.14f, 0.02f, 1f);
                Looking(charge);
                charge.HeadLean = -46f;
                // The skid: the front foot digs in and the body swings round it, spear still out.
                var skid = charge.Copy();
                skid.Hips = new Vector3(0.06f, -0.52f, 0.62f);
                skid.HipYaw = 40f;
                skid.HipLean = 26f;
                skid.WaistYaw = 14f;
                skid.WaistLean = 20f;
                skid.FootR = new Vector3(0.36f, 0f, -0.12f);
                skid.FootRTip = 30f;
                skid.FootRTurn = 60f;
                skid.HandR = new Vector3(0.36f, 0.72f, 0.66f);
                skid.Blade = new Vector3(-0.22f, 0.08f, 0.97f);
                Looking(skid);
                skid.HeadLean = -30f;
                var spent = skid.Copy();
                spent.Hips = new Vector3(0.1f, -0.44f, 0.5f);
                spent.HipYaw = 54f;
                spent.WaistYaw = 22f;
                spent.WaistLean = 34f;
                spent.HandR = new Vector3(0.4f, 0.6f, 0.5f);
                spent.Blade = new Vector3(-0.4f, -0.14f, 0.9f);
                Looking(spent);
                spent.HeadLean = 10f;
                var rise = SoldierReady();
                rise.Hips = new Vector3(0.02f, -0.06f, 0.24f);
                rise.WaistLean = -4f;
                rise.FootL = new Vector3(-0.14f, 0f, 0.56f);
                rise.FootR = new Vector3(0.24f, 0f, -0.06f);
                rise.HandR = new Vector3(0.3f, 1.14f, 0.1f);
                rise.Blade = new Vector3(-0.2f, 0.4f, 0.9f);
                Looking(rise);
                yield return clip.Key(0f, SoldierReady()).Key(hit * 0.4f, couch, Ease.Out).Key(hit * 0.9f, shake, Ease.InOut).Key(hit, charge, Ease.In)
                    .Key(done, skid, Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.3f), spent, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.68f), rise, Ease.InOut).Key(1f, SoldierReady(), Ease.InOut);
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
                // The knife carries on round past the end of the cut and swings back to where the second cut starts from.
                var past = through.Copy();
                past.Hips = new Vector3(-0.04f, -0.38f, 0.2f);
                past.HipYaw = -38f;
                past.WaistYaw = -70f;
                past.WaistRoll = 8f;
                past.HandR = new Vector3(-0.62f, 0.92f, 0.08f);
                past.Blade = new Vector3(-0.8f, -0.12f, -0.5f);
                past.HandL = new Vector3(-0.6f, 1.1f, -0.3f);
                Looking(past, 0.6f);
                var tighter = wound.Copy();
                tighter.Hips += new Vector3(0.03f, -0.05f, -0.05f);
                tighter.WaistYaw = 50f;
                tighter.HandR = new Vector3(0.66f, 1.16f, -0.2f);
                Looking(tighter, 0.7f);
                yield return clip.Key(0f, AcolyteReady()).Key(hit * 0.6f, wound, Ease.Out).Key(hit * 0.9f, tighter, Ease.InOut).Key(hit, cut, Ease.In)
                    .Key(done, through, Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.45f), past, Ease.Out).Key(1f, through, Ease.InOut);
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

            // The red leap: it folds down like a spring, goes up stretched to its full length with both knives over its
            // head, and comes down into a deep crouch with the knives driven into what is under it.
            {
                var clip = Attack(Named(kind, "acolyte_leap"), out float hit, out float done);
                var crouch = AcolyteReady();
                crouch.Hips = new Vector3(0f, -0.5f, -0.12f);
                crouch.HipLean = 30f;
                crouch.WaistLean = 24f;
                crouch.WaistYaw = 14f;
                crouch.HandL = new Vector3(-0.42f, 0.56f, -0.2f);
                crouch.HandR = new Vector3(0.44f, 0.5f, -0.34f);
                crouch.Blade = new Vector3(0.2f, -0.3f, -0.93f);
                var coiled = crouch.Copy();
                coiled.Hips = new Vector3(0f, -0.6f, -0.18f);
                coiled.HipLean = 36f;
                coiled.WaistLean = 30f;
                coiled.HandL = new Vector3(-0.44f, 0.5f, -0.36f);
                coiled.HandR = new Vector3(0.46f, 0.44f, -0.46f);
                var air = new ForgePose
                {
                    Hips = new Vector3(0f, 0.56f, 0.22f),
                    HipLean = -6f,
                    HipYaw = 12f,
                    WaistLean = -22f,
                    WaistYaw = 10f,
                    HeadLean = 16f,
                    FootL = new Vector3(-0.14f, 0.5f, -0.34f),
                    FootR = new Vector3(0.16f, 0.72f, 0.02f),
                    FootLTip = 70f,
                    FootRTip = 50f,
                    HandL = new Vector3(-0.34f, 2.5f, 0.2f),
                    HandR = new Vector3(0.26f, 2.6f, 0.04f),
                    ElbowL = new Vector3(-1f, 0.2f, 0.4f),
                    ElbowR = new Vector3(1f, 0.2f, 0.4f),
                    ShoulderLUp = 30f,
                    ShoulderRUp = 30f,
                    AimBlade = true,
                    Blade = new Vector3(0.1f, 0.8f, -0.6f),
                    HandLBend = 40f,
                };
                var stab = new ForgePose
                {
                    Hips = new Vector3(0f, -0.46f, 0.3f),
                    HipLean = 40f,
                    HipYaw = -10f,
                    WaistLean = 30f,
                    WaistYaw = -16f,
                    HeadLean = -40f,
                    HeadYaw = 20f,
                    FootL = new Vector3(-0.24f, 0f, 0.52f),
                    FootR = new Vector3(0.24f, 0f, -0.16f),
                    FootRTip = 50f,
                    HandL = new Vector3(-0.36f, 0.36f, 0.78f),
                    HandR = new Vector3(0.3f, 0.32f, 0.9f),
                    ElbowL = new Vector3(-1f, 0.4f, -0.2f),
                    ElbowR = new Vector3(1f, 0.4f, -0.2f),
                    AimBlade = true,
                    Blade = new Vector3(0f, -0.9f, 0.4f),
                    HandLBend = -70f,
                };
                var sunk = stab.Copy();
                sunk.Hips = new Vector3(0f, -0.54f, 0.36f);
                sunk.WaistLean = 38f;
                sunk.HeadLean = -30f;
                sunk.HandL = new Vector3(-0.4f, 0.3f, 0.8f);
                sunk.HandR = new Vector3(0.32f, 0.28f, 0.94f);
                // The knives wrenched back out as it comes up off them.
                var wrench = AcolyteReady();
                wrench.Hips = new Vector3(0f, -0.4f, 0.16f);
                wrench.HipYaw = 14f;
                wrench.WaistYaw = 20f;
                wrench.WaistLean = 6f;
                wrench.FootL = new Vector3(-0.24f, 0f, 0.4f);
                wrench.FootR = new Vector3(0.24f, 0f, -0.24f);
                wrench.HandL = new Vector3(-0.5f, 0.9f, 0.2f);
                wrench.HandR = new Vector3(0.52f, 1.0f, 0.0f);
                wrench.Blade = new Vector3(0.5f, 0.3f, 0.8f);
                Looking(wrench);
                clip.Wind = t => new Vector3(0f, t > hit * 0.62f && t < hit * 0.9f ? -7f : t >= hit * 0.9f && t < hit ? 7f : 0f, t > hit * 0.6f && t < done ? -8f : 0f);
                yield return clip.Key(0f, AcolyteReady()).Key(hit * 0.4f, crouch, Ease.Out).Key(hit * 0.62f, coiled, Ease.InOut).Key(hit * 0.9f, air, Ease.Out)
                    .Key(hit, stab, Ease.In).Key(done, sunk, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.55f), wrench, Ease.InOut).Key(1f, AcolyteReady(), Ease.InOut);
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

            // The bolt: the free hand drawn back to the shoulder with the body wound behind it, thrown open at the
            // target as the shoulders come round, kicked back by what it let go, and lowered a step at a time.
            {
                var clip = Attack(Named(kind, "caster_bolt"), out float hit, out float done);
                var draw = CasterReady();
                draw.Hips = new Vector3(-0.03f, -0.1f, -0.1f);
                draw.HipYaw = -18f;
                draw.WaistYaw = -34f;
                draw.WaistLean = -6f;
                draw.WaistRoll = 6f;
                draw.FootL = new Vector3(-0.16f, 0f, -0.2f);
                draw.FootLTurn = 36f;
                draw.FootR = new Vector3(0.12f, 0f, 0.14f);
                draw.HandL = new Vector3(-0.46f, 1.5f, -0.22f);
                draw.ElbowL = new Vector3(-0.8f, 0.1f, -1f);
                draw.ShoulderLUp = 16f;
                draw.HandLBend = -40f;
                draw.HandR = new Vector3(0.3f, 1.16f, 0.4f);
                draw.Blade = new Vector3(0.1f, 1f, 0.16f);
                Looking(draw);
                var strain = draw.Copy();
                strain.Hips = new Vector3(-0.04f, -0.15f, -0.14f);
                strain.WaistYaw = -44f;
                strain.HandL = new Vector3(-0.5f, 1.56f, -0.3f);
                strain.HandR = new Vector3(0.28f, 1.2f, 0.46f);
                Looking(strain);
                var thrown = CasterReady();
                thrown.Hips = new Vector3(0.02f, -0.18f, 0.16f);
                thrown.HipYaw = 22f;
                thrown.HipLean = 8f;
                thrown.WaistYaw = 40f;
                thrown.WaistLean = 16f;
                thrown.FootL = new Vector3(-0.14f, 0f, 0.36f);
                thrown.FootR = new Vector3(0.16f, 0f, -0.16f);
                thrown.FootRTurn = 36f;
                thrown.FootRTip = 20f;
                thrown.HandL = new Vector3(0.0f, 1.36f, 0.9f);
                thrown.ElbowL = new Vector3(-1f, -0.4f, 0f);
                thrown.ShoulderLForward = 24f;
                thrown.HandLBend = -60f;
                thrown.HandR = new Vector3(0.56f, 1.1f, -0.14f);
                thrown.Blade = new Vector3(0.3f, 0.92f, -0.24f);
                Looking(thrown, 0.7f);
                var recoil = thrown.Copy();
                recoil.Hips = new Vector3(0f, -0.06f, -0.04f);
                recoil.HipLean = -4f;
                recoil.WaistYaw = 20f;
                recoil.WaistLean = -12f;
                recoil.HandL = new Vector3(-0.16f, 1.72f, 0.5f);
                recoil.ElbowL = new Vector3(-1f, -0.2f, 0.2f);
                recoil.ShoulderLForward = 0f;
                recoil.ShoulderLUp = 20f;
                recoil.HandR = new Vector3(0.5f, 1.14f, 0.02f);
                recoil.Blade = new Vector3(0.2f, 0.96f, -0.1f);
                Looking(recoil, 0.7f);
                recoil.HeadLean = -14f;
                var first = CasterReady();
                first.Hips = new Vector3(0f, -0.08f, 0.04f);
                first.HipYaw = 10f;
                first.WaistYaw = 10f;
                first.FootL = thrown.FootL;
                first.FootR = thrown.FootR;
                first.HandL = new Vector3(-0.24f, 1.42f, 0.5f);
                first.HandR = new Vector3(0.42f, 1.12f, 0.12f);
                var second = CasterReady();
                second.Hips = new Vector3(0f, -0.05f, 0.02f);
                second.FootL = new Vector3(-0.13f, 0f, 0.2f);
                second.HandL = new Vector3(-0.3f, 1.22f, 0.36f);
                yield return clip.Key(0f, CasterReady()).Key(hit * 0.4f, draw, Ease.Out).Key(hit * 0.92f, strain, Ease.InOut).Key(hit, thrown, Ease.In)
                    .Key(done, recoil, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.4f), first, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.75f), second, Ease.Out).Key(1f, CasterReady(), Ease.InOut);
            }

            // The burst: the staff heaved up level over the head in both hands with the back arched under it, slammed
            // foot first into the ground, and the arms thrown wide as it goes off.
            {
                var clip = Attack(Named(kind, "caster_nova"), out float hit, out float done);
                var lift = CasterReady();
                lift.Hips = new Vector3(0f, 0.0f, -0.06f);
                lift.HipLean = -6f;
                lift.WaistLean = -16f;
                lift.HeadLean = -14f;
                lift.HandR = new Vector3(0.36f, 1.96f, -0.06f);
                lift.TwoHanded = true;
                lift.GripGap = 0.62f;
                lift.ElbowL = new Vector3(-1f, 0.2f, 0.4f);
                lift.ElbowR = new Vector3(1f, 0.2f, 0.4f);
                lift.ShoulderRUp = 28f;
                lift.ShoulderLUp = 28f;
                lift.Blade = new Vector3(1f, 0.16f, 0f);
                var top = lift.Copy();
                top.Hips = new Vector3(0f, 0.03f, -0.1f);
                top.HipLean = -10f;
                top.WaistLean = -24f;
                top.HeadLean = -20f;
                top.HandR = new Vector3(0.36f, 1.98f, -0.2f);
                top.Blade = new Vector3(1f, 0.3f, -0.1f);
                var slam = CasterReady();
                slam.Hips = new Vector3(0f, -0.36f, 0.1f);
                slam.HipLean = 16f;
                slam.WaistLean = 30f;
                slam.HeadLean = -16f;
                slam.FootL = new Vector3(-0.24f, 0f, 0.16f);
                slam.FootR = new Vector3(0.22f, 0f, -0.1f);
                slam.HandR = new Vector3(0.06f, 1.12f, 0.6f);
                slam.TwoHanded = true;
                slam.GripGap = 0.3f;
                slam.ElbowL = new Vector3(-1f, 0f, 0f);
                slam.ElbowR = new Vector3(1f, 0f, 0f);
                slam.Blade = new Vector3(0f, 1f, -0.06f);
                var wide = CasterReady();
                wide.Hips = new Vector3(0f, -0.2f, 0f);
                wide.HipLean = -4f;
                wide.WaistLean = -14f;
                wide.WaistYaw = -10f;
                wide.HeadLean = -18f;
                wide.FootL = slam.FootL;
                wide.FootR = slam.FootR;
                wide.HandL = new Vector3(-0.8f, 1.5f, 0.0f);
                wide.ElbowL = new Vector3(-0.3f, -1f, -0.4f);
                wide.HandLBend = -40f;
                wide.HandR = new Vector3(0.74f, 1.3f, 0.06f);
                wide.ElbowR = new Vector3(0.3f, -1f, -0.4f);
                wide.ShoulderLUp = 14f;
                wide.ShoulderRUp = 10f;
                wide.Blade = new Vector3(0.34f, 0.94f, 0f);
                var spent = wide.Copy();
                spent.Hips = new Vector3(0f, -0.12f, 0.02f);
                spent.WaistLean = 2f;
                spent.WaistYaw = 0f;
                spent.HeadLean = 4f;
                spent.HandL = new Vector3(-0.6f, 1.12f, 0.16f);
                spent.HandR = new Vector3(0.56f, 1.14f, 0.18f);
                spent.Blade = new Vector3(0.16f, 0.98f, 0.04f);
                yield return clip.Key(0f, CasterReady()).Key(hit * 0.5f, lift, Ease.Out).Key(hit * 0.9f, top, Ease.InOut).Key(hit, slam, Ease.In)
                    .Key(done, wide, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), spent, Ease.InOut).Key(1f, CasterReady(), Ease.InOut);
            }
        }
    }
}

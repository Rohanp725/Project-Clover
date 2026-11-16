using System.Collections.Generic;
using Clover.Player;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools.Forge
{
    /// <summary>
    /// Every animation of the player, written as poses. Clover > Forge > Bake Player Animations plays each one through
    /// on the character's skeleton, saves it as a clip under Generated/Animation/Player, and draws its contact sheet to
    /// review/anim. Attacks read their timing from the weapon's own numbers, so the blow in the animation always falls
    /// on the moment the game deals it.
    ///
    /// The rules every action follows: a wind-up that goes the opposite way first, a beat held at its end, a blow that
    /// arrives fast, a follow-through that carries past the target, and a settle before the recovery. The hips and
    /// chest lead and the arms arrive after; the head stays on the enemy while the body turns under it.
    /// </summary>
    public static class PlayerAnimations
    {
        public const string ClipFolder = BuildKit.GeneratedRoot + "/Animation/Player";
        private const string ModelPath = PlayerBuilder.PlayerModelPath;

        [MenuItem("Clover/Forge/Bake Player Animations")]
        public static void BakeAll() => Bake(null);

        /// <summary>Bakes the clips whose names contain the filter (all of them when it is empty) and draws their sheets.</summary>
        public static void Bake(string filter)
        {
            var asset = AssetDatabase.LoadAssetAtPath<GameObject>(ModelPath);
            if (asset == null)
            {
                Debug.LogError($"[Clover.Forge] No character model at {ModelPath}.");
                return;
            }
            using (var rig = new ForgeRig(asset))
            {
                PlayerBuilder.Dress(rig.Model);
                var tails = new ForgeTails()
                    .Add(rig, 0.1f, "scarf_a1", "scarf_a2", "scarf_a3")
                    .Add(rig, 0.1f, "scarf_b1", "scarf_b2")
                    .Add(rig, 0.16f, "band_a1", "band_a2")
                    .Add(rig, 0.16f, "band_b1", "band_b2")
                    .Add(rig, 0.3f, "pony1", "pony2", "pony3");
                int baked = 0;
                foreach (var clip in All())
                {
                    if (!string.IsNullOrEmpty(filter) && !clip.Name.Contains(filter)) continue;
                    clip.Bake(rig, ClipFolder, tails);
                    ShowSword(rig, SwordIn(clip.Name));
                    var local = clip;
                    tails.Reset(rig, local, 0f);
                    float last = 0f;
                    ForgeSheet.Render("player_" + clip.Name, rig.Model, t =>
                    {
                        // The sheet walks through the clip in order, so the trailing parts are simulated between its frames.
                        int steps = Mathf.Max(1, Mathf.RoundToInt((t - last) * local.Seconds * 60f));
                        for (int i = 1; i <= steps; i++)
                        {
                            float at = Mathf.Lerp(last, t, i / (float)steps);
                            local.Pose(rig, at);
                            tails.Step(rig, 1f / 60f, local.Wind != null ? local.Wind(at) : Vector3.zero);
                        }
                        last = t;
                    }, 8, 0.95f, clip.SheetMoments);
                    baked++;
                }
                AssetDatabase.SaveAssets();
                Debug.Log($"[Clover.Forge] Baked {baked} player clips to {ClipFolder}; sheets in {ForgeSheet.Folder}.");
            }
        }

        // Which sword object a clip is looked at with: 1 in the hand, 0 in the sheath, -1 no sword at all.
        private static int SwordIn(string clip)
        {
            if (clip.Contains("fist")) return -1;
            if (clip == "charge" || clip == "idle" || clip == "run" || clip == "roll" || clip == "hurt" || clip == "drink" || clip == "cast" || clip == "death") return 0;
            return 1;
        }

        private static void ShowSword(ForgeRig rig, int where)
        {
            foreach (var t in rig.Model.GetComponentsInChildren<Transform>(true))
            {
                if (t.name == "katana_drawn") t.gameObject.SetActive(where == 1);
                else if (t.name == "katana_hilt") t.gameObject.SetActive(where == 0);
                else if (t.name == "katana_sheath") t.gameObject.SetActive(where >= 0);
            }
        }

        public static IEnumerable<ForgeClip> All()
        {
            yield return Idle();
            yield return Run();
            yield return IdleFist();
            yield return RunFist();
            yield return Roll();
            yield return Hurt();
            yield return Death();
            yield return Drink();
            yield return Cast();

            var katana = Weapons.Katana().Combo;
            yield return DrawCut(katana.Steps[0]);
            yield return ReturnCut(katana.Steps[1]);
            yield return StepCut(katana.Steps[2]);
            yield return Overhead(katana.Heavy);
            yield return IaidoStance();
            yield return Iaido(katana.Skill);
            yield return RunThrough(katana.Finisher);
            yield return SwordGuard();
            yield return Leap();

            var fists = Weapons.Fists().Combo;
            yield return Jab(fists.Steps[0]);
            yield return Cross(fists.Steps[1]);
            yield return Hook(fists.Steps[2]);
            yield return FrontKick(fists.Steps[3]);
            yield return Overhand(fists.Heavy);
            yield return Uppercut(fists.ChainHeavy);
            yield return Coil();
            yield return Breaker(fists.Skill);
            yield return PalmStrike(fists.Finisher);
            yield return FistGuard();
        }

        // The moments of an attack as fractions of its whole length: when the blow lands, and when the swing is over.
        private static (float hit, float done) Moments(AttackStep step) =>
            (step.Windup / step.Total, (step.Windup + step.Active) / step.Total);

        // The moments of an attack worth seeing: the start, the wind-up, just before and at the blow, the swing's end, the recovery.
        private static float[] Sheet(float hit, float done) =>
            new[] { 0f, hit * 0.6f, hit * 0.92f, hit, (hit + done) * 0.5f, done, Mathf.Lerp(done, 1f, 0.4f), 1f };

        // The head stays on the enemy ahead while the body turns under it.
        private static ForgePose Looking(ForgePose pose, float keep = 0.8f)
        {
            pose.HeadYaw = -(pose.HipYaw + pose.WaistYaw) * keep;
            return pose;
        }

        private static ForgeClip Attack(AttackStep step, float lungeWind = -5f)
        {
            var (hit, done) = Moments(step);
            return new ForgeClip(step.Name, step.Total)
            {
                // The body drives forward into the blow, so what trails from it streams back until it lands.
                Wind = t => new Vector3(0f, 0f, t < hit ? lungeWind : 0f),
                SheetMoments = Sheet(hit, done),
            };
        }

        // ------------------------------------------------------------------ stances

        /// <summary>Standing easy with the sword sheathed: weight a little on the left leg, left hand resting on the sheath.</summary>
        private static ForgePose Stand() => new ForgePose
        {
            Hips = new Vector3(-0.015f, -0.035f, 0f),
            HipRoll = 2f,
            WaistLean = 2f,
            FootL = new Vector3(-0.14f, 0f, -0.04f),
            FootR = new Vector3(0.16f, 0f, 0.07f),
            FootLTurn = 12f,
            FootRTurn = 22f,
            HandL = new Vector3(-0.23f, 0.97f, 0.1f),
            ElbowL = new Vector3(-0.8f, -0.1f, -0.6f),
            HandR = new Vector3(0.31f, 0.85f, 0.05f),
        };

        /// <summary>The sword out and held in both hands at the middle guard: where a cut ends up and the next begins.</summary>
        private static ForgePose SwordReady() => Looking(new ForgePose
        {
            Hips = new Vector3(0f, -0.11f, 0.02f),
            HipYaw = 14f,
            WaistYaw = 8f,
            WaistLean = 6f,
            FootL = new Vector3(-0.17f, 0f, -0.22f),
            FootR = new Vector3(0.15f, 0f, 0.26f),
            FootLTurn = 40f,
            FootRTurn = 4f,
            HandR = new Vector3(0.14f, 1.02f, 0.4f),
            ElbowR = new Vector3(0.8f, -0.5f, -0.3f),
            ElbowL = new Vector3(-0.8f, -0.5f, -0.3f),
            AimBlade = true,
            TwoHanded = true,
            Blade = new Vector3(-0.1f, 0.5f, 0.86f),
        });

        /// <summary>Bare hands: side on, left foot and left hand leading, weight on the balls of the feet.</summary>
        private static ForgePose FistReady() => Looking(new ForgePose
        {
            Hips = new Vector3(0f, -0.1f, 0f),
            HipYaw = 28f,
            WaistYaw = 8f,
            WaistLean = 8f,
            FootL = new Vector3(-0.12f, 0f, 0.24f),
            FootR = new Vector3(0.2f, 0f, -0.2f),
            FootLTurn = -10f,
            FootRTurn = 42f,
            FootRTip = 12f,
            HandL = new Vector3(-0.2f, 1.36f, 0.4f),
            HandR = new Vector3(0.2f, 1.3f, 0.2f),
            ElbowL = new Vector3(-0.5f, -1f, 0.1f),
            ElbowR = new Vector3(0.6f, -1f, -0.2f),
            ShoulderLUp = 8f,
            ShoulderRUp = 8f,
            HandLBend = -10f,
        });

        // ------------------------------------------------------------------ moving

        private static ForgeClip Idle() => new ForgeClip("idle", 2.6f, true).From(t =>
        {
            float breath = Mathf.Sin(t * Mathf.PI * 2f);
            float slow = Mathf.Sin(t * Mathf.PI * 2f + 1.1f);
            var p = Stand();
            p.Hips += new Vector3(0.004f * slow, 0.01f * breath, 0f);
            p.WaistLean += 1.6f * breath;
            p.ShoulderLUp = p.ShoulderRUp = 2.5f + 2.5f * breath;
            p.HeadLean = -1.5f * breath;
            p.HeadYaw = 3f * slow;
            p.HandR += new Vector3(0.005f * slow, 0.008f * breath, 0.012f * slow);
            return p;
        });

        private static ForgeClip IdleFist() => new ForgeClip("idle_fist", 1.3f, true).From(t =>
        {
            // Never quite still: a light bounce on the balls of the feet, hands circling a little.
            float bounce = Mathf.Sin(t * Mathf.PI * 4f);
            float sway = Mathf.Sin(t * Mathf.PI * 2f);
            var p = FistReady();
            p.Hips += new Vector3(0.012f * sway, 0.014f * bounce, 0.01f * sway);
            p.FootLTip = 6f + 4f * bounce;
            p.FootRTip = 14f + 5f * bounce;
            p.HandL += new Vector3(0.012f * sway, 0.014f * bounce, 0.02f * sway);
            p.HandR += new Vector3(-0.01f * sway, -0.012f * bounce, 0.012f * sway);
            p.WaistLean += 1.5f * bounce;
            return p;
        });

        private static ForgeClip Run()
        {
            var clip = new ForgeClip("run", 0.56f, true) { Wind = _ => new Vector3(0f, 1.5f, -11f) };
            return clip.From(t =>
            {
                float phase = t * Mathf.PI * 2f;
                float left = Mathf.Sin(phase), right = -left;
                float leftLift = Mathf.Max(0f, Mathf.Cos(phase)), rightLift = Mathf.Max(0f, -Mathf.Cos(phase));
                return new ForgePose
                {
                    // Low and thrown forward, the way a swordsman covers ground: sheath hand at the hip, sword arm
                    // swinging behind. The game is seen from above, where legs hardly show, so the run is told by
                    // the lean, the turn of the shoulders against the hips, and the arm.
                    Hips = new Vector3(0.012f * left, -0.14f + 0.05f * Mathf.Cos(phase * 2f), 0.09f),
                    HipLean = 24f,
                    HipYaw = 15f * left,
                    HipRoll = 6f * left,
                    WaistLean = 15f,
                    WaistYaw = -34f * left,
                    WaistRoll = -4f * left,
                    HeadLean = -30f,
                    HeadYaw = 14f * left,
                    FootL = new Vector3(-0.11f, 0.3f * leftLift, 0.52f * left - 0.08f),
                    FootR = new Vector3(0.11f, 0.3f * rightLift, 0.52f * right - 0.08f),
                    FootLTurn = 2f,
                    FootRTurn = 2f,
                    FootLTip = 34f * Mathf.Max(0f, -left) - 14f * leftLift,
                    FootRTip = 34f * Mathf.Max(0f, -right) - 14f * rightLift,
                    HandL = new Vector3(-0.25f, 0.84f, 0.2f + 0.05f * right),
                    ElbowL = new Vector3(-0.8f, 0f, -0.6f),
                    HandR = new Vector3(0.4f - 0.06f * left, 0.86f + 0.1f * left, -0.3f + 0.3f * left),
                    ElbowR = new Vector3(0.5f, 0.3f, -1f),
                    ShoulderRForward = 10f * left,
                };
            });
        }

        private static ForgeClip RunFist()
        {
            var clip = new ForgeClip("run_fist", 0.52f, true) { Wind = _ => new Vector3(0f, 1.5f, -11f) };
            return clip.From(t =>
            {
                // A sprinter's run: arms pumping against the legs, fists loose.
                float phase = t * Mathf.PI * 2f;
                float left = Mathf.Sin(phase), right = -left;
                float leftLift = Mathf.Max(0f, Mathf.Cos(phase)), rightLift = Mathf.Max(0f, -Mathf.Cos(phase));
                return new ForgePose
                {
                    Hips = new Vector3(0.012f * left, -0.12f + 0.05f * Mathf.Cos(phase * 2f), 0.08f),
                    HipLean = 20f,
                    HipYaw = 15f * left,
                    HipRoll = 6f * left,
                    WaistLean = 13f,
                    WaistYaw = -36f * left,
                    WaistRoll = -4f * left,
                    HeadLean = -26f,
                    HeadYaw = 16f * left,
                    FootL = new Vector3(-0.11f, 0.32f * leftLift, 0.54f * left - 0.08f),
                    FootR = new Vector3(0.11f, 0.32f * rightLift, 0.54f * right - 0.08f),
                    FootLTurn = 2f,
                    FootRTurn = 2f,
                    FootLTip = 34f * Mathf.Max(0f, -left) - 14f * leftLift,
                    FootRTip = 34f * Mathf.Max(0f, -right) - 14f * rightLift,
                    HandL = new Vector3(-0.3f, 1.04f - 0.16f * right, 0.14f + 0.42f * right),
                    HandR = new Vector3(0.3f, 1.04f - 0.16f * left, 0.14f + 0.42f * left),
                    ElbowL = new Vector3(-0.4f, -0.3f, -1f),
                    ElbowR = new Vector3(0.4f, -0.3f, -1f),
                };
            });
        }

        private static ForgeClip Roll()
        {
            var tuck = new ForgePose
            {
                Hips = new Vector3(0f, -0.5f, 0f),
                HipLean = 35f,
                WaistLean = 60f,
                HeadLean = 35f,
                FootL = new Vector3(-0.12f, 0.16f, 0.22f),
                FootR = new Vector3(0.12f, 0.16f, 0.22f),
                HandL = new Vector3(-0.2f, 0.42f, 0.34f),
                HandR = new Vector3(0.2f, 0.42f, 0.34f),
            };
            var dive = new ForgePose
            {
                Hips = new Vector3(0f, -0.3f, 0.1f),
                HipLean = 40f,
                WaistLean = 30f,
                FootL = new Vector3(-0.13f, 0f, -0.25f),
                FootR = new Vector3(0.13f, 0f, -0.05f),
                HandL = new Vector3(-0.22f, 0.5f, 0.55f),
                HandR = new Vector3(0.22f, 0.5f, 0.55f),
            };
            var land = new ForgePose
            {
                Hips = new Vector3(0f, -0.4f, 0f),
                HipLean = 18f,
                WaistLean = 16f,
                FootL = new Vector3(-0.16f, 0f, -0.2f),
                FootR = new Vector3(0.16f, 0f, 0.25f),
                FootLTip = 30f,
                HandL = new Vector3(-0.3f, 0.5f, 0.3f),
                HandR = new Vector3(0.34f, 0.62f, 0.1f),
            };
            var clip = new ForgeClip("roll", 0.4f) { Wind = t => new Vector3(0f, 0f, t < 0.8f ? -9f : 0f) };
            clip.Key(0f, Stand()).Key(0.14f, dive, Ease.Out).Key(0.3f, tuck, Ease.InOut).Key(0.72f, tuck, Ease.Linear).Key(1f, land, Ease.Out);
            // The body turns once, head over heels, about a point at its middle.
            clip.After = (rig, t) =>
            {
                float turn = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.14f, 0.82f, t)) * 360f;
                var root = rig["root"].Transform;
                var pivot = new Vector3(0f, 0.42f, 0f);
                var spin = Quaternion.AngleAxis(turn, Vector3.right);
                root.rotation = spin * rig["root"].RestRotation;
                root.position = pivot - spin * pivot + spin * rig["root"].RestPosition;
            };
            return clip;
        }

        private static ForgeClip Hurt()
        {
            // Thrown back off the front foot, twisted by the blow, then gathered low.
            var struck = new ForgePose
            {
                Hips = new Vector3(0.03f, -0.1f, -0.16f),
                HipLean = -14f,
                HipYaw = 16f,
                WaistLean = -12f,
                WaistYaw = 26f,
                WaistRoll = 10f,
                HeadLean = -26f,
                HeadYaw = -22f,
                FootL = new Vector3(-0.16f, 0f, 0.12f),
                FootR = new Vector3(0.2f, 0f, -0.3f),
                FootRTurn = 30f,
                HandL = new Vector3(-0.48f, 1.02f, -0.02f),
                HandR = new Vector3(0.52f, 1.12f, -0.18f),
                ShoulderRUp = 14f,
                ShoulderLUp = 10f,
            };
            var gather = new ForgePose
            {
                Hips = new Vector3(0f, -0.2f, -0.08f),
                HipLean = 10f,
                WaistLean = 14f,
                FootL = new Vector3(-0.17f, 0f, 0.08f),
                FootR = new Vector3(0.2f, 0f, -0.24f),
                FootRTurn = 30f,
                HandL = new Vector3(-0.3f, 0.95f, 0.22f),
                HandR = new Vector3(0.34f, 0.9f, 0.14f),
            };
            return new ForgeClip("hurt", 0.35f) { Wind = t => new Vector3(0f, 0f, t < 0.3f ? 9f : 0f) }
                .Key(0f, Stand()).Key(0.22f, struck, Ease.Snap).Key(0.5f, struck, Ease.Linear).Key(1f, gather, Ease.InOut);
        }

        private static ForgeClip Death()
        {
            // Staggers back, the knees go, and the body pitches forward onto the ground.
            var reel = new ForgePose
            {
                Hips = new Vector3(0.02f, -0.08f, -0.14f),
                HipLean = -12f,
                WaistLean = -16f,
                WaistYaw = 18f,
                HeadLean = -30f,
                FootL = new Vector3(-0.15f, 0f, 0.1f),
                FootR = new Vector3(0.2f, 0f, -0.26f),
                HandL = new Vector3(-0.45f, 1.0f, -0.05f),
                HandR = new Vector3(0.5f, 1.05f, -0.15f),
            };
            var knees = new ForgePose
            {
                Hips = new Vector3(0f, -0.52f, 0.02f),
                HipLean = 8f,
                WaistLean = 24f,
                HeadLean = 34f,
                FootL = new Vector3(-0.16f, 0f, -0.32f),
                FootR = new Vector3(0.16f, 0f, -0.3f),
                FootLTip = 70f,
                FootRTip = 70f,
                KneeL = new Vector3(-0.3f, -0.3f, 1f),
                KneeR = new Vector3(0.3f, -0.3f, 1f),
                HandL = new Vector3(-0.28f, 0.38f, 0.14f),
                HandR = new Vector3(0.3f, 0.36f, 0.16f),
            };
            var down = new ForgePose
            {
                Hips = new Vector3(0f, -0.84f, 0.2f),
                HipLean = 78f,
                WaistLean = 12f,
                HeadLean = 0f,
                HeadYaw = 50f,
                FootL = new Vector3(-0.18f, 0.02f, -0.62f),
                FootR = new Vector3(0.16f, 0.02f, -0.66f),
                FootLTip = 80f,
                FootRTip = 80f,
                KneeL = new Vector3(-0.4f, -1f, 0.2f),
                KneeR = new Vector3(0.4f, -1f, 0.2f),
                HandL = new Vector3(-0.4f, 0.06f, 0.72f),
                HandR = new Vector3(0.34f, 0.06f, 0.5f),
                ElbowL = new Vector3(-1f, 0.2f, 0f),
                ElbowR = new Vector3(1f, 0.2f, 0f),
            };
            return new ForgeClip("death", 1.1f) { SheetMoments = new[] { 0f, 0.12f, 0.25f, 0.4f, 0.55f, 0.7f, 0.85f, 1f } }
                .Key(0f, Stand()).Key(0.16f, reel, Ease.Snap).Key(0.3f, reel, Ease.Linear)
                .Key(0.55f, knees, Ease.In).Key(0.66f, knees, Ease.Linear).Key(0.9f, down, Ease.In).Key(1f, down, Ease.Linear);
        }

        private static ForgeClip Drink()
        {
            // The left hand brings the flask up, the head goes back, a breath, and down again.
            var lift = Stand();
            lift.HandL = new Vector3(-0.06f, 1.5f, 0.22f);
            lift.ElbowL = new Vector3(-1f, -0.3f, 0.3f);
            lift.HeadLean = -24f;
            lift.WaistLean = -6f;
            lift.ShoulderLUp = 12f;
            lift.HandLBend = -30f;
            var hold = lift.Copy();
            hold.HeadLean = -30f;
            hold.HandL = new Vector3(-0.05f, 1.54f, 0.2f);
            return new ForgeClip("drink", 0.8f).Key(0f, Stand()).Key(0.3f, lift, Ease.Out).Key(0.62f, hold, Ease.InOut).Key(1f, Stand(), Ease.InOut);
        }

        private static ForgeClip Cast()
        {
            var kit = new ElementKit();
            float release = kit.SpellWindup / (kit.SpellWindup + kit.SpellRecovery);
            // The hand is drawn back past the ear, then thrown open at the enemy with the whole body behind it.
            var gather = Looking(new ForgePose
            {
                Hips = new Vector3(0f, -0.14f, -0.08f),
                HipYaw = -30f,
                WaistYaw = -34f,
                WaistLean = 4f,
                FootL = new Vector3(-0.14f, 0f, 0.26f),
                FootR = new Vector3(0.2f, 0f, -0.24f),
                FootRTurn = 40f,
                HandL = new Vector3(-0.42f, 1.42f, -0.12f),
                ElbowL = new Vector3(-0.8f, -0.3f, -0.6f),
                HandR = new Vector3(0.3f, 0.95f, 0.2f),
                ShoulderLUp = 14f,
            });
            var thrown = Looking(new ForgePose
            {
                Hips = new Vector3(0f, -0.2f, 0.14f),
                HipYaw = 22f,
                WaistYaw = 34f,
                WaistLean = 16f,
                FootL = new Vector3(-0.14f, 0f, 0.42f),
                FootR = new Vector3(0.2f, 0f, -0.28f),
                FootRTurn = 40f,
                FootRTip = 24f,
                HandL = new Vector3(-0.1f, 1.28f, 0.9f),
                ElbowL = new Vector3(-1f, -0.4f, 0f),
                HandR = new Vector3(0.42f, 0.98f, -0.2f),
                HandLBend = -50f,
                ShoulderLForward = 20f,
            });
            var held = thrown.Copy();
            held.Hips += new Vector3(0f, 0f, 0.02f);
            return new ForgeClip("cast", kit.SpellWindup + kit.SpellRecovery)
            {
                SheetMoments = Sheet(release, Mathf.Lerp(release, 1f, 0.3f)),
            }.Key(0f, Stand()).Key(release * 0.62f, gather, Ease.Out).Key(release, thrown, Ease.In)
                .Key(Mathf.Lerp(release, 1f, 0.55f), held, Ease.Out).Key(1f, Stand(), Ease.InOut);
        }

        // ------------------------------------------------------------------ katana

        /// <summary>The first cut: drawn from the left hip and carried level across to the right.</summary>
        private static ForgeClip DrawCut(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var coil = Looking(new ForgePose
            {
                Hips = new Vector3(-0.03f, -0.16f, -0.02f),
                HipYaw = -26f,
                HipLean = 8f,
                WaistYaw = -34f,
                WaistLean = 10f,
                FootL = new Vector3(-0.2f, 0f, -0.28f),
                FootR = new Vector3(0.14f, 0f, 0.3f),
                FootLTurn = 40f,
                FootRTurn = 4f,
                HandL = new Vector3(-0.25f, 0.92f, 0.08f),
                HandR = new Vector3(-0.2f, 0.98f, 0.26f),
                ElbowR = new Vector3(0.9f, -0.2f, 0.3f),
                AimBlade = true,
                Blade = new Vector3(-0.5f, -0.2f, -0.85f),
            });
            var cut = Looking(new ForgePose
            {
                Hips = new Vector3(0.02f, -0.2f, 0.12f),
                HipYaw = 6f,
                HipLean = 12f,
                WaistYaw = 14f,
                WaistLean = 10f,
                FootL = new Vector3(-0.22f, 0f, -0.34f),
                FootR = new Vector3(0.16f, 0f, 0.42f),
                FootLTurn = 46f,
                FootLTip = 18f,
                FootRTurn = 0f,
                HandL = new Vector3(-0.28f, 0.9f, -0.02f),
                HandR = new Vector3(0.2f, 1.14f, 0.66f),
                AimBlade = true,
                Blade = new Vector3(-0.25f, 0.05f, 1f),
            });
            var through = cut.Copy();
            through.HipYaw = 24f;
            through.WaistYaw = 52f;
            through.Hips = new Vector3(0.05f, -0.2f, 0.12f);
            through.HandR = new Vector3(0.7f, 1.2f, 0.12f);
            through.Blade = new Vector3(1f, 0.12f, -0.3f);
            Looking(through, 0.6f);
            var settle = through.Copy();
            settle.WaistYaw = 44f;
            settle.HandR = new Vector3(0.66f, 1.12f, 0.08f);
            settle.Blade = new Vector3(0.95f, 0f, -0.42f);
            return Attack(step).Key(0f, coil).Key(hit * 0.55f, coil, Ease.Hold).Key(hit, cut, Ease.In)
                .Key(done, through, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.35f), settle, Ease.Out).Key(1f, SwordReady(), Ease.InOut);
        }

        /// <summary>The second cut: back from the right, rising across to the upper left, both hands on the grip.</summary>
        private static ForgeClip ReturnCut(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var load = Looking(new ForgePose
            {
                Hips = new Vector3(0.05f, -0.2f, 0.02f),
                HipYaw = 30f,
                WaistYaw = 48f,
                WaistLean = 10f,
                FootL = new Vector3(-0.2f, 0f, -0.3f),
                FootR = new Vector3(0.16f, 0f, 0.34f),
                FootLTurn = 44f,
                HandR = new Vector3(0.58f, 0.92f, -0.08f),
                AimBlade = true,
                TwoHanded = true,
                Blade = new Vector3(0.85f, -0.35f, -0.4f),
            }, 0.6f);
            var cut = Looking(new ForgePose
            {
                Hips = new Vector3(0f, -0.2f, 0.14f),
                HipYaw = 0f,
                WaistYaw = -6f,
                WaistLean = 12f,
                FootL = new Vector3(-0.2f, 0f, -0.32f),
                FootR = new Vector3(0.16f, 0f, 0.44f),
                FootLTurn = 44f,
                FootLTip = 16f,
                HandR = new Vector3(0.08f, 1.22f, 0.66f),
                AimBlade = true,
                TwoHanded = true,
                Blade = new Vector3(0.3f, 0.2f, 0.93f),
            });
            var through = Looking(new ForgePose
            {
                Hips = new Vector3(-0.04f, -0.16f, 0.14f),
                HipYaw = -22f,
                WaistYaw = -46f,
                WaistLean = 4f,
                FootL = new Vector3(-0.2f, 0f, -0.32f),
                FootR = new Vector3(0.16f, 0f, 0.44f),
                FootLTurn = 44f,
                FootLTip = 16f,
                HandR = new Vector3(-0.42f, 1.5f, 0.22f),
                AimBlade = true,
                TwoHanded = true,
                Blade = new Vector3(-0.8f, 0.58f, -0.1f),
            }, 0.6f);
            var settle = through.Copy();
            settle.WaistYaw = -38f;
            settle.HandR = new Vector3(-0.38f, 1.42f, 0.2f);
            settle.Blade = new Vector3(-0.76f, 0.5f, -0.3f);
            return Attack(step).Key(0f, load).Key(hit * 0.5f, load, Ease.Hold).Key(hit, cut, Ease.In)
                .Key(done, through, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.35f), settle, Ease.Out).Key(1f, SwordReady(), Ease.InOut);
        }

        /// <summary>The third cut: a step in and the sword brought straight down from over the head.</summary>
        private static ForgeClip StepCut(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var raise = new ForgePose
            {
                Hips = new Vector3(0f, -0.04f, -0.06f),
                HipLean = -6f,
                WaistLean = -12f,
                HeadLean = 10f,
                FootL = new Vector3(-0.16f, 0f, -0.12f),
                FootR = new Vector3(0.14f, 0.1f, 0.12f),
                FootLTurn = 30f,
                HandR = new Vector3(0.06f, 1.92f, -0.02f),
                ElbowR = new Vector3(1f, 0.2f, 0.4f),
                ElbowL = new Vector3(-1f, 0.2f, 0.4f),
                ShoulderRUp = 20f,
                ShoulderLUp = 20f,
                AimBlade = true,
                TwoHanded = true,
                Blade = new Vector3(0f, 0.55f, -0.84f),
            };
            var cut = new ForgePose
            {
                Hips = new Vector3(0f, -0.2f, 0.2f),
                HipLean = 16f,
                WaistLean = 16f,
                HeadLean = -14f,
                FootL = new Vector3(-0.18f, 0f, -0.34f),
                FootR = new Vector3(0.14f, 0f, 0.52f),
                FootLTurn = 40f,
                FootLTip = 20f,
                HandR = new Vector3(0.05f, 1.22f, 0.7f),
                AimBlade = true,
                TwoHanded = true,
                Blade = new Vector3(0f, 0.3f, 0.95f),
            };
            var low = cut.Copy();
            low.Hips = new Vector3(0f, -0.3f, 0.24f);
            low.HipLean = 24f;
            low.WaistLean = 24f;
            low.HeadLean = -26f;
            low.HandR = new Vector3(0.05f, 0.72f, 0.62f);
            low.Blade = new Vector3(0f, -0.5f, 0.87f);
            var settle = low.Copy();
            settle.Hips = new Vector3(0f, -0.28f, 0.22f);
            settle.HandR = new Vector3(0.05f, 0.76f, 0.6f);
            settle.Blade = new Vector3(0f, -0.42f, 0.9f);
            return Attack(step, -8f).Key(0f, SwordReady()).Key(hit * 0.55f, raise, Ease.Out).Key(hit * 0.8f, raise, Ease.Linear)
                .Key(hit, cut, Ease.In).Key(done, low, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.4f), settle, Ease.Out).Key(1f, SwordReady(), Ease.InOut);
        }

        /// <summary>The heavy cut: the sword taken right back over the head with the body arched, held, and brought down with everything.</summary>
        private static ForgeClip Overhead(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var back = new ForgePose
            {
                Hips = new Vector3(0f, -0.06f, -0.14f),
                HipLean = -10f,
                WaistLean = -20f,
                HeadLean = 18f,
                FootL = new Vector3(-0.17f, 0f, -0.26f),
                FootR = new Vector3(0.15f, 0f, 0.16f),
                FootLTurn = 36f,
                HandR = new Vector3(0.06f, 1.9f, -0.2f),
                ElbowR = new Vector3(1f, 0.3f, 0.3f),
                ElbowL = new Vector3(-1f, 0.3f, 0.3f),
                ShoulderRUp = 24f,
                ShoulderLUp = 24f,
                AimBlade = true,
                TwoHanded = true,
                Blade = new Vector3(0f, -0.35f, -0.94f),
            };
            var coiled = back.Copy();
            coiled.Hips = new Vector3(0f, -0.1f, -0.17f);
            coiled.WaistLean = -24f;
            coiled.Blade = new Vector3(0f, -0.5f, -0.87f);
            var cut = new ForgePose
            {
                Hips = new Vector3(0f, -0.26f, 0.22f),
                HipLean = 20f,
                WaistLean = 22f,
                HeadLean = -20f,
                FootL = new Vector3(-0.19f, 0f, -0.36f),
                FootR = new Vector3(0.15f, 0f, 0.54f),
                FootLTurn = 40f,
                FootLTip = 24f,
                HandR = new Vector3(0.05f, 1.0f, 0.74f),
                AimBlade = true,
                TwoHanded = true,
                Blade = new Vector3(0f, 0.02f, 1f),
            };
            var buried = cut.Copy();
            buried.Hips = new Vector3(0f, -0.4f, 0.26f);
            buried.HipLean = 30f;
            buried.WaistLean = 30f;
            buried.HeadLean = -32f;
            buried.HandR = new Vector3(0.05f, 0.7f, 0.64f);
            buried.Blade = new Vector3(0f, -0.4f, 0.92f);
            var settle = buried.Copy();
            settle.Hips = new Vector3(0f, -0.36f, 0.24f);
            settle.HandR = new Vector3(0.05f, 0.74f, 0.62f);
            return Attack(step, -7f).Key(0f, SwordReady()).Key(hit * 0.5f, back, Ease.Out).Key(hit * 0.86f, coiled, Ease.InOut)
                .Key(hit, cut, Ease.In).Key(done, buried, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.45f), settle, Ease.Out).Key(1f, SwordReady(), Ease.InOut);
        }

        /// <summary>The leap: a deep crouch, up with the sword taken back over the head and the knees drawn in, and down blade first.</summary>
        private static ForgeClip Leap()
        {
            var crouch = new ForgePose
            {
                Hips = new Vector3(0f, -0.42f, -0.04f),
                HipLean = 26f,
                WaistLean = 18f,
                HeadLean = -30f,
                FootL = new Vector3(-0.2f, 0f, -0.14f),
                FootR = new Vector3(0.18f, 0f, 0.16f),
                HandR = new Vector3(0.3f, 0.6f, -0.2f),
                HandL = new Vector3(-0.34f, 0.55f, -0.16f),
                AimBlade = true,
                Blade = new Vector3(0.3f, -0.3f, -0.9f),
            };
            var risen = new ForgePose
            {
                Hips = new Vector3(0f, 0.04f, 0.04f),
                HipLean = -12f,
                WaistLean = -22f,
                HeadLean = 22f,
                FootL = new Vector3(-0.16f, 0.34f, -0.3f),
                FootR = new Vector3(0.15f, 0.5f, 0.18f),
                FootLTip = 40f,
                FootRTip = 30f,
                HandR = new Vector3(0.06f, 1.95f, -0.22f),
                ElbowR = new Vector3(1f, 0.3f, 0.3f),
                ElbowL = new Vector3(-1f, 0.3f, 0.3f),
                ShoulderRUp = 26f,
                ShoulderLUp = 26f,
                AimBlade = true,
                TwoHanded = true,
                Blade = new Vector3(0f, -0.45f, -0.9f),
            };
            var hang = risen.Copy();
            hang.WaistLean = -28f;
            hang.FootL = new Vector3(-0.16f, 0.42f, -0.36f);
            hang.Blade = new Vector3(0f, -0.6f, -0.8f);
            var falling = new ForgePose
            {
                Hips = new Vector3(0f, -0.1f, 0.14f),
                HipLean = 24f,
                WaistLean = 20f,
                HeadLean = -26f,
                FootL = new Vector3(-0.18f, 0.3f, -0.4f),
                FootR = new Vector3(0.15f, 0.12f, 0.4f),
                FootLTip = 40f,
                HandR = new Vector3(0.05f, 1.15f, 0.7f),
                AimBlade = true,
                TwoHanded = true,
                Blade = new Vector3(0f, -0.3f, 0.95f),
            };
            var landed = new ForgePose
            {
                Hips = new Vector3(0f, -0.5f, 0.2f),
                HipLean = 34f,
                WaistLean = 30f,
                HeadLean = -36f,
                FootL = new Vector3(-0.22f, 0f, -0.4f),
                FootR = new Vector3(0.17f, 0f, 0.52f),
                FootLTurn = 40f,
                FootLTip = 28f,
                HandR = new Vector3(0.05f, 0.52f, 0.66f),
                AimBlade = true,
                TwoHanded = true,
                Blade = new Vector3(0f, -0.62f, 0.78f),
            };
            return new ForgeClip("leap", 0.62f) { Wind = t => new Vector3(0f, t < 0.5f ? -6f : 8f, -8f), SheetMoments = new[] { 0f, 0.1f, 0.25f, 0.45f, 0.62f, 0.8f, 0.92f, 1f } }
                .Key(0f, SwordReady()).Key(0.1f, crouch, Ease.Out).Key(0.3f, risen, Ease.Out).Key(0.6f, hang, Ease.InOut)
                .Key(0.9f, falling, Ease.In).Key(1f, landed, Ease.Out);
        }

        // The drawing stance: sunk low and turned away, right hand on the grip, eyes on the enemy over the shoulder.
        private static ForgePose Drawing() => Looking(new ForgePose
        {
            Hips = new Vector3(-0.04f, -0.32f, -0.04f),
            HipYaw = -36f,
            HipLean = 14f,
            WaistYaw = -30f,
            WaistLean = 16f,
            FootL = new Vector3(-0.24f, 0f, -0.36f),
            FootR = new Vector3(0.16f, 0f, 0.44f),
            FootLTurn = 50f,
            FootLTip = 14f,
            FootRTurn = 0f,
            HandL = new Vector3(-0.3f, 0.78f, 0.0f),
            ElbowL = new Vector3(-0.9f, 0.1f, -0.5f),
            HandR = new Vector3(-0.22f, 0.8f, 0.24f),
            ElbowR = new Vector3(0.9f, -0.1f, 0.4f),
            ShoulderRForward = 16f,
            AimBlade = true,
            Blade = new Vector3(-0.5f, -0.2f, -0.85f),
        }, 0.85f);

        private static ForgeClip IaidoStance() =>
            new ForgeClip("charge", 0.25f).Key(0f, Stand()).Key(0.7f, Drawing(), Ease.Out).Key(1f, Drawing(), Ease.Linear);

        /// <summary>The Iaido Slash: out of the sheath and through in one line, held at full reach, then flicked clean and sheathed.</summary>
        private static ForgeClip Iaido(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var cut = Looking(new ForgePose
            {
                Hips = new Vector3(0.02f, -0.34f, 0.2f),
                HipYaw = 0f,
                HipLean = 16f,
                WaistYaw = 10f,
                WaistLean = 14f,
                FootL = new Vector3(-0.24f, 0f, -0.46f),
                FootR = new Vector3(0.16f, 0f, 0.56f),
                FootLTurn = 50f,
                FootLTip = 26f,
                HandL = new Vector3(-0.34f, 0.76f, -0.1f),
                ElbowL = new Vector3(-0.9f, 0.1f, -0.5f),
                HandR = new Vector3(0.18f, 1.0f, 0.74f),
                AimBlade = true,
                Blade = new Vector3(-0.2f, 0.02f, 1f),
            });
            var through = Looking(new ForgePose
            {
                Hips = new Vector3(0.06f, -0.36f, 0.22f),
                HipYaw = 34f,
                HipLean = 14f,
                WaistYaw = 62f,
                WaistLean = 8f,
                FootL = new Vector3(-0.24f, 0f, -0.46f),
                FootR = new Vector3(0.16f, 0f, 0.56f),
                FootLTurn = 50f,
                FootLTip = 26f,
                HandL = new Vector3(-0.36f, 0.76f, -0.14f),
                ElbowL = new Vector3(-0.9f, 0.1f, -0.5f),
                HandR = new Vector3(0.76f, 1.06f, -0.16f),
                ShoulderRUp = 10f,
                AimBlade = true,
                Blade = new Vector3(0.82f, 0.08f, -0.56f),
            }, 0.55f);
            var held = through.Copy();
            held.WaistYaw = 58f;
            held.HandR = new Vector3(0.74f, 1.02f, -0.18f);
            // The flick that clears the blade, then home to the sheath's mouth.
            var flick = through.Copy();
            flick.Hips = new Vector3(0.02f, -0.24f, 0.14f);
            flick.HipYaw = 16f;
            flick.WaistYaw = 26f;
            flick.HandR = new Vector3(0.62f, 0.82f, 0.2f);
            flick.Blade = new Vector3(0.6f, -0.62f, 0.5f);
            Looking(flick);
            var sheathing = Drawing();
            sheathing.Hips = new Vector3(-0.02f, -0.18f, 0f);
            sheathing.HipYaw = -18f;
            sheathing.WaistYaw = -16f;
            sheathing.HandR = new Vector3(-0.1f, 0.9f, 0.34f);
            Looking(sheathing);
            var clip = Attack(step, -12f);
            clip.SheetMoments = new[] { 0f, hit, (hit + done) * 0.5f, done, 0.55f, 0.75f, 0.9f, 1f };
            return clip.Key(0f, Drawing()).Key(hit, cut, Ease.In).Key(done, through, Ease.Out).Key(0.56f, held, Ease.Linear)
                .Key(0.76f, flick, Ease.InOut).Key(1f, sheathing, Ease.InOut);
        }

        /// <summary>The finisher: the point drawn back to the hip, driven through with a long step, held, and torn out to the side.</summary>
        private static ForgeClip RunThrough(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var chamber = Looking(new ForgePose
            {
                Hips = new Vector3(0.04f, -0.24f, -0.14f),
                HipYaw = 34f,
                WaistYaw = 30f,
                WaistLean = 8f,
                FootL = new Vector3(-0.18f, 0f, 0.2f),
                FootR = new Vector3(0.2f, 0f, -0.28f),
                FootRTurn = 40f,
                HandR = new Vector3(0.34f, 1.0f, -0.16f),
                ElbowR = new Vector3(0.6f, 0f, -1f),
                AimBlade = true,
                TwoHanded = true,
                GripGap = 0.14f,
                Blade = new Vector3(-0.12f, 0.06f, 1f),
            });
            var thrust = Looking(new ForgePose
            {
                Hips = new Vector3(0f, -0.34f, 0.34f),
                HipYaw = -14f,
                HipLean = 18f,
                WaistYaw = -22f,
                WaistLean = 14f,
                FootL = new Vector3(-0.18f, 0f, -0.42f),
                FootR = new Vector3(0.16f, 0f, 0.66f),
                FootLTurn = 44f,
                FootLTip = 30f,
                HandR = new Vector3(0.1f, 1.12f, 0.92f),
                ShoulderRForward = 22f,
                AimBlade = true,
                TwoHanded = true,
                GripGap = 0.14f,
                Blade = new Vector3(0f, 0.02f, 1f),
            });
            var driven = thrust.Copy();
            driven.Hips = new Vector3(0f, -0.36f, 0.38f);
            driven.HandR = new Vector3(0.1f, 1.1f, 0.96f);
            var tear = Looking(new ForgePose
            {
                Hips = new Vector3(0.06f, -0.3f, 0.26f),
                HipYaw = 26f,
                WaistYaw = 58f,
                WaistLean = 4f,
                FootL = new Vector3(-0.18f, 0f, -0.42f),
                FootR = new Vector3(0.16f, 0f, 0.66f),
                FootLTurn = 44f,
                FootLTip = 30f,
                HandL = new Vector3(-0.34f, 1.0f, 0.2f),
                HandR = new Vector3(0.74f, 1.4f, 0.02f),
                ShoulderRUp = 18f,
                AimBlade = true,
                Blade = new Vector3(0.86f, 0.46f, -0.2f),
            }, 0.5f);
            var settle = tear.Copy();
            settle.HandR = new Vector3(0.7f, 1.3f, 0f);
            settle.Blade = new Vector3(0.9f, 0.3f, -0.3f);
            var clip = Attack(step, -9f);
            clip.SheetMoments = new[] { 0f, hit * 0.7f, hit, done, 0.62f, 0.72f, 0.85f, 1f };
            return clip.Key(0f, SwordReady()).Key(hit * 0.6f, chamber, Ease.Out).Key(hit * 0.82f, chamber, Ease.Linear).Key(hit, thrust, Ease.In)
                .Key(done, driven, Ease.Out).Key(0.6f, driven, Ease.Linear).Key(0.72f, tear, Ease.Snap).Key(0.86f, settle, Ease.Out).Key(1f, SwordReady(), Ease.InOut);
        }

        private static ForgeClip SwordGuard()
        {
            // The blade brought up across the body, point high to the left, both hands behind it.
            var guard = SwordReady();
            guard.Hips = new Vector3(0f, -0.16f, -0.02f);
            guard.WaistYaw = -6f;
            guard.WaistLean = 10f;
            guard.HandR = new Vector3(0.2f, 1.2f, 0.4f);
            guard.Blade = new Vector3(-0.62f, 0.72f, 0.3f);
            guard.ShoulderRUp = 8f;
            guard.ShoulderLUp = 8f;
            Looking(guard);
            return new ForgeClip("guard", 0.14f).Key(0f, SwordReady()).Key(0.8f, guard, Ease.Out).Key(1f, guard, Ease.Linear);
        }

        // ------------------------------------------------------------------ fists

        private static ForgeClip Jab(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var load = FistReady();
            load.HandL = new Vector3(-0.2f, 1.36f, 0.3f);
            load.WaistYaw = 14f;
            Looking(load);
            var reach = FistReady();
            reach.Hips = new Vector3(-0.02f, -0.12f, 0.1f);
            reach.HipYaw = 16f;
            reach.WaistYaw = -12f;
            reach.FootL = new Vector3(-0.12f, 0f, 0.36f);
            reach.HandL = new Vector3(-0.06f, 1.4f, 0.94f);
            reach.ElbowL = new Vector3(-1f, -0.4f, 0f);
            reach.ShoulderLForward = 24f;
            reach.ShoulderLUp = 12f;
            Looking(reach);
            var back = FistReady();
            back.HandL = new Vector3(-0.18f, 1.36f, 0.5f);
            return Attack(step, -4f).Key(0f, FistReady()).Key(hit * 0.5f, load, Ease.Out).Key(hit, reach, Ease.In)
                .Key(done, reach, Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.5f), back, Ease.Out).Key(1f, FistReady(), Ease.InOut);
        }

        private static ForgeClip Cross(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var load = FistReady();
            load.HipYaw = 40f;
            load.WaistYaw = 22f;
            load.HandR = new Vector3(0.3f, 1.3f, 0.02f);
            Looking(load);
            // The rear hand goes straight down the middle, the rear heel comes up and the whole side turns in behind it.
            var reach = FistReady();
            reach.Hips = new Vector3(-0.02f, -0.14f, 0.12f);
            reach.HipYaw = -16f;
            reach.WaistYaw = -48f;
            reach.WaistLean = 12f;
            reach.FootRTip = 40f;
            reach.FootRTurn = 10f;
            reach.HandR = new Vector3(0.04f, 1.4f, 0.98f);
            reach.ElbowR = new Vector3(1f, -0.4f, 0f);
            reach.ShoulderRForward = 28f;
            reach.ShoulderRUp = 12f;
            reach.HandL = new Vector3(-0.2f, 1.34f, 0.12f);
            Looking(reach, 0.7f);
            var back = FistReady();
            back.HandR = new Vector3(0.18f, 1.32f, 0.4f);
            back.WaistYaw = -10f;
            Looking(back);
            return Attack(step, -4f).Key(0f, FistReady()).Key(hit * 0.5f, load, Ease.Out).Key(hit, reach, Ease.In)
                .Key(done, reach, Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.5f), back, Ease.Out).Key(1f, FistReady(), Ease.InOut);
        }

        private static ForgeClip Hook(AttackStep step)
        {
            var (hit, done) = Moments(step);
            // A wide right hook: the elbow up, the arm a bar, the body whipping it round.
            var load = FistReady();
            load.Hips = new Vector3(0.03f, -0.16f, 0f);
            load.HipYaw = 48f;
            load.WaistYaw = 38f;
            load.HandR = new Vector3(0.58f, 1.3f, -0.02f);
            load.ElbowR = new Vector3(1f, 0.4f, -0.6f);
            load.ShoulderRUp = 16f;
            Looking(load, 0.7f);
            var land = FistReady();
            land.Hips = new Vector3(-0.02f, -0.16f, 0.1f);
            land.HipYaw = -10f;
            land.WaistYaw = -30f;
            land.WaistLean = 10f;
            land.FootRTip = 36f;
            land.HandR = new Vector3(0.02f, 1.4f, 0.62f);
            land.ElbowR = new Vector3(1f, 0.6f, 0.2f);
            land.ShoulderRUp = 18f;
            land.ShoulderRForward = 18f;
            land.HandL = new Vector3(-0.22f, 1.32f, 0.16f);
            Looking(land, 0.7f);
            var through = land.Copy();
            through.HipYaw = -30f;
            through.WaistYaw = -62f;
            through.HandR = new Vector3(-0.44f, 1.36f, 0.3f);
            Looking(through, 0.6f);
            return Attack(step, -4f).Key(0f, FistReady()).Key(hit * 0.55f, load, Ease.Out).Key(hit, land, Ease.In)
                .Key(done, through, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), through, Ease.Linear).Key(1f, FistReady(), Ease.InOut);
        }

        private static ForgeClip FrontKick(AttackStep step)
        {
            var (hit, done) = Moments(step);
            // The rear leg comes through: knee up to the chest, then the foot driven straight out with the hips behind it.
            var chamber = FistReady();
            chamber.Hips = new Vector3(-0.04f, -0.04f, 0.02f);
            chamber.HipYaw = 6f;
            chamber.WaistYaw = 0f;
            chamber.WaistLean = 6f;
            chamber.FootL = new Vector3(-0.12f, 0f, 0.1f);
            chamber.FootLTurn = 30f;
            chamber.FootR = new Vector3(0.1f, 0.62f, 0.3f);
            chamber.FootRTip = 30f;
            chamber.FootRTurn = 0f;
            chamber.KneeR = new Vector3(0.15f, 0.6f, 1f);
            chamber.HandL = new Vector3(-0.24f, 1.34f, 0.3f);
            chamber.HandR = new Vector3(0.26f, 1.24f, 0.16f);
            var kick = chamber.Copy();
            kick.Hips = new Vector3(-0.04f, -0.06f, 0.1f);
            kick.HipLean = -16f;
            kick.WaistLean = 2f;
            kick.FootR = new Vector3(0.08f, 0.98f, 1.0f);
            kick.FootRTip = -10f;
            kick.HandL = new Vector3(-0.34f, 1.24f, 0.1f);
            kick.HandR = new Vector3(0.28f, 1.2f, 0.04f);
            var held = kick.Copy();
            held.FootR = new Vector3(0.08f, 0.96f, 0.98f);
            var down = FistReady();
            down.FootR = new Vector3(0.14f, 0f, 0.4f);
            down.FootL = new Vector3(-0.14f, 0f, -0.12f);
            down.HipYaw = -14f;
            Looking(down);
            return Attack(step, -5f).Key(0f, FistReady()).Key(hit * 0.62f, chamber, Ease.Out).Key(hit, kick, Ease.In)
                .Key(done, held, Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.35f), chamber, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.75f), down, Ease.In).Key(1f, FistReady(), Ease.InOut);
        }

        private static ForgeClip Overhand(AttackStep step)
        {
            var (hit, done) = Moments(step);
            // Loaded low and wide, then thrown over the top with a full step through.
            var load = FistReady();
            load.Hips = new Vector3(0.06f, -0.3f, -0.12f);
            load.HipYaw = 52f;
            load.WaistYaw = 44f;
            load.WaistLean = 14f;
            load.HandR = new Vector3(0.6f, 1.2f, -0.34f);
            load.ElbowR = new Vector3(1f, 0.3f, -0.6f);
            load.HandL = new Vector3(-0.16f, 1.32f, 0.5f);
            Looking(load, 0.7f);
            var land = new ForgePose
            {
                Hips = new Vector3(-0.02f, -0.3f, 0.34f),
                HipYaw = -30f,
                HipLean = 14f,
                WaistYaw = -44f,
                WaistLean = 18f,
                WaistRoll = -8f,
                FootL = new Vector3(-0.16f, 0f, -0.34f),
                FootR = new Vector3(0.14f, 0f, 0.62f),
                FootLTurn = 40f,
                FootLTip = 30f,
                HandR = new Vector3(0.0f, 1.26f, 1.0f),
                ElbowR = new Vector3(1f, 0.6f, 0f),
                HandL = new Vector3(-0.3f, 1.2f, -0.04f),
                ShoulderRForward = 28f,
                ShoulderRUp = 16f,
            };
            Looking(land, 0.6f);
            var sunk = land.Copy();
            sunk.Hips = new Vector3(-0.02f, -0.36f, 0.38f);
            sunk.HandR = new Vector3(-0.04f, 1.14f, 0.98f);
            return Attack(step, -9f).Key(0f, FistReady()).Key(hit * 0.55f, load, Ease.Out).Key(hit * 0.82f, load, Ease.Linear).Key(hit, land, Ease.In)
                .Key(done, sunk, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), sunk, Ease.Linear).Key(1f, FistReady(), Ease.InOut);
        }

        private static ForgeClip Uppercut(AttackStep step)
        {
            var (hit, done) = Moments(step);
            // Down into the knees, then everything upward at once: the fist ends above the head and the heels leave the ground.
            var dip = FistReady();
            dip.Hips = new Vector3(0.04f, -0.42f, 0.04f);
            dip.HipYaw = 40f;
            dip.WaistYaw = 30f;
            dip.WaistLean = 26f;
            dip.HandR = new Vector3(0.3f, 0.66f, 0.2f);
            dip.ElbowR = new Vector3(1f, -0.2f, -0.6f);
            dip.HandL = new Vector3(-0.2f, 1.2f, 0.36f);
            Looking(dip);
            var rise = FistReady();
            rise.Hips = new Vector3(-0.02f, 0.04f, 0.14f);
            rise.HipYaw = -18f;
            rise.WaistYaw = -34f;
            rise.WaistLean = -12f;
            rise.FootLTip = 36f;
            rise.FootRTip = 44f;
            rise.FootL = new Vector3(-0.12f, 0.06f, 0.3f);
            rise.FootR = new Vector3(0.18f, 0.08f, -0.08f);
            rise.HandR = new Vector3(0.06f, 1.86f, 0.5f);
            rise.ElbowR = new Vector3(0.6f, -0.3f, 1f);
            rise.ShoulderRUp = 28f;
            rise.HandL = new Vector3(-0.3f, 1.1f, 0.06f);
            rise.HeadLean = -14f;
            Looking(rise, 0.6f);
            var hang = rise.Copy();
            hang.Hips = new Vector3(-0.02f, 0.07f, 0.16f);
            var land = FistReady();
            land.Hips = new Vector3(0f, -0.22f, 0.08f);
            land.HandR = new Vector3(0.2f, 1.4f, 0.3f);
            return Attack(step, -4f).Key(0f, FistReady()).Key(hit * 0.7f, dip, Ease.Out).Key(hit, rise, Ease.In)
                .Key(done, hang, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.45f), land, Ease.In).Key(1f, FistReady(), Ease.InOut);
        }

        // The coil before the Breaker: fist drawn back to the hip, the open left hand sighting the target.
        private static ForgePose Coiled() => Looking(new ForgePose
        {
            Hips = new Vector3(0.04f, -0.3f, -0.1f),
            HipYaw = 56f,
            HipLean = 6f,
            WaistYaw = 34f,
            WaistLean = 10f,
            FootL = new Vector3(-0.14f, 0f, 0.34f),
            FootR = new Vector3(0.22f, 0f, -0.32f),
            FootLTurn = -6f,
            FootRTurn = 56f,
            HandL = new Vector3(-0.12f, 1.3f, 0.66f),
            ElbowL = new Vector3(-1f, -0.5f, 0f),
            HandLBend = -50f,
            HandR = new Vector3(0.32f, 0.96f, -0.3f),
            ElbowR = new Vector3(0.6f, 0.1f, -1f),
            ShoulderRUp = 6f,
        }, 0.85f);

        private static ForgeClip Coil() =>
            new ForgeClip("charge_fist", 0.25f).Key(0f, FistReady()).Key(0.7f, Coiled(), Ease.Out).Key(1f, Coiled(), Ease.Linear);

        private static ForgeClip Breaker(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var strike = Looking(new ForgePose
            {
                Hips = new Vector3(-0.02f, -0.34f, 0.4f),
                HipYaw = -34f,
                HipLean = 16f,
                WaistYaw = -52f,
                WaistLean = 16f,
                FootL = new Vector3(-0.14f, 0f, 0.62f),
                FootR = new Vector3(0.22f, 0f, -0.44f),
                FootLTurn = -6f,
                FootRTurn = 30f,
                FootRTip = 40f,
                HandL = new Vector3(-0.32f, 1.0f, -0.12f),
                ElbowL = new Vector3(-0.6f, 0.1f, -1f),
                HandR = new Vector3(0.02f, 1.24f, 1.04f),
                ElbowR = new Vector3(1f, -0.3f, 0f),
                ShoulderRForward = 30f,
                ShoulderRUp = 10f,
            }, 0.6f);
            var held = strike.Copy();
            held.Hips = new Vector3(-0.02f, -0.36f, 0.43f);
            held.HandR = new Vector3(0.02f, 1.22f, 1.06f);
            var clip = Attack(step, -12f);
            clip.SheetMoments = new[] { 0f, hit * 0.6f, hit, (hit + done) * 0.5f, done, 0.6f, 0.8f, 1f };
            return clip.Key(0f, Coiled()).Key(hit, strike, Ease.In).Key(done, held, Ease.Out).Key(0.62f, held, Ease.Linear).Key(1f, FistReady(), Ease.InOut);
        }

        private static ForgeClip PalmStrike(AttackStep step)
        {
            var (hit, done) = Moments(step);
            // Both hands drawn to the ribs, then driven out together with a long step: the blow that puts them down.
            var gather = new ForgePose
            {
                Hips = new Vector3(0f, -0.3f, -0.16f),
                HipLean = 4f,
                WaistLean = 16f,
                FootL = new Vector3(-0.18f, 0f, 0.18f),
                FootR = new Vector3(0.2f, 0f, -0.3f),
                FootRTurn = 40f,
                HandL = new Vector3(-0.24f, 1.06f, 0.02f),
                HandR = new Vector3(0.24f, 1.06f, 0.02f),
                ElbowL = new Vector3(-0.6f, 0f, -1f),
                ElbowR = new Vector3(0.6f, 0f, -1f),
                HandLBend = -40f,
                HandRBend = 40f,
            };
            var strike = new ForgePose
            {
                Hips = new Vector3(0f, -0.34f, 0.4f),
                HipLean = 18f,
                WaistLean = 16f,
                HeadLean = -20f,
                FootL = new Vector3(-0.18f, 0f, 0.64f),
                FootR = new Vector3(0.2f, 0f, -0.42f),
                FootRTurn = 30f,
                FootRTip = 40f,
                HandL = new Vector3(-0.13f, 1.24f, 1.02f),
                HandR = new Vector3(0.13f, 1.24f, 1.02f),
                ElbowL = new Vector3(-1f, -0.3f, 0f),
                ElbowR = new Vector3(1f, -0.3f, 0f),
                ShoulderLForward = 26f,
                ShoulderRForward = 26f,
                HandLBend = -60f,
                HandRBend = 60f,
            };
            var held = strike.Copy();
            held.Hips = new Vector3(0f, -0.36f, 0.44f);
            var clip = Attack(step, -9f);
            clip.SheetMoments = new[] { 0f, hit * 0.7f, hit, done, 0.6f, 0.75f, 0.9f, 1f };
            return clip.Key(0f, FistReady()).Key(hit * 0.6f, gather, Ease.Out).Key(hit * 0.84f, gather, Ease.Linear).Key(hit, strike, Ease.In)
                .Key(done, held, Ease.Out).Key(0.68f, held, Ease.Linear).Key(1f, FistReady(), Ease.InOut);
        }

        private static ForgeClip FistGuard()
        {
            // Forearms crossed high in front of the face, chin tucked behind them.
            var guard = FistReady();
            guard.Hips = new Vector3(0f, -0.18f, -0.03f);
            guard.HipYaw = 12f;
            guard.WaistYaw = 0f;
            guard.WaistLean = 14f;
            guard.HeadLean = 10f;
            guard.HandL = new Vector3(0.1f, 1.48f, 0.38f);
            guard.HandR = new Vector3(-0.1f, 1.54f, 0.34f);
            guard.ElbowL = new Vector3(-0.6f, -1f, 0.3f);
            guard.ElbowR = new Vector3(0.6f, -1f, 0.3f);
            guard.ShoulderLUp = 16f;
            guard.ShoulderRUp = 16f;
            Looking(guard);
            return new ForgeClip("guard_fist", 0.14f).Key(0f, FistReady()).Key(0.8f, guard, Ease.Out).Key(1f, guard, Ease.Linear);
        }
    }
}

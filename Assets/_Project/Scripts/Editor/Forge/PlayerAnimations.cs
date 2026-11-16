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
    public static partial class PlayerAnimations
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
                // The other weapons hang on the same sockets as in the game, so each sheet is drawn with the right one in hand.
                var rack = WeaponBuilder.Hang(rig.Model, asset);
                rack.Tint(true, Clover.Player.Elements.Colour(Clover.Player.Elements.Fire), 1f);
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
                    string carry = CarryOf(clip.Name);
                    ShowSword(rig, carry != null ? -1 : SwordIn(clip.Name));
                    rack.Show(carry, true);
                    // A great weapon's tip is far from the body: its sheet is drawn from further off.
                    float distance = carry == "spear" ? 11f : carry == "gsword" || carry == "scythe" || carry == "hammer" ? 8.2f : 5.2f;
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
                    }, 8, 0.95f, clip.SheetMoments, distance);
                    baked++;
                }
                AssetDatabase.SaveAssets();
                Debug.Log($"[Clover.Forge] Baked {baked} player clips to {ClipFolder}; sheets in {ForgeSheet.Folder}.");
            }
        }

        // Which weapon on the rack a clip belongs to, read from its name; null for the Katana's and the fists' own.
        private static string CarryOf(string clip)
        {
            foreach (var spec in WeaponBuilder.Specs)
                if (clip.EndsWith("_" + spec.Carry) || clip.Contains("_" + spec.Carry + "_")) return spec.Carry;
            return null;
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
            yield return GuardHitFist("fist");
            yield return GuardPerfectFist("fist");

            foreach (var clip in GreatswordClips()) yield return clip;
            foreach (var clip in ScytheClips()) yield return clip;
            foreach (var clip in ArmamentClips()) yield return clip;
            foreach (var clip in SpearClips()) yield return clip;
            foreach (var clip in HammerClips()) yield return clip;
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
                Shape = ForgeClip.Impact(hit, step.Total),
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
            // Breathing that can be seen from above: the chest rises, the weight drifts from foot to foot, the head turns to look about.
            p.Hips += new Vector3(0.02f * slow, 0.024f * breath, 0.008f * slow);
            p.HipRoll += 2f * slow;
            p.WaistLean += 3.6f * breath;
            p.WaistYaw = 3f * slow;
            p.ShoulderLUp = p.ShoulderRUp = 3f + 4.5f * breath;
            p.HeadLean = -3f * breath;
            p.HeadYaw = 10f * slow;
            p.HandR += new Vector3(0.012f * slow, 0.022f * breath, 0.03f * slow);
            return p;
        });

        private static ForgeClip IdleFist() => new ForgeClip("idle_fist", 1.3f, true).From(t =>
        {
            // Never quite still: a light bounce on the balls of the feet, hands circling a little.
            float bounce = Mathf.Sin(t * Mathf.PI * 4f);
            float sway = Mathf.Sin(t * Mathf.PI * 2f);
            var p = FistReady();
            p.Hips += new Vector3(0.03f * sway, 0.03f * bounce, 0.02f * sway);
            p.HipYaw += 4f * sway;
            p.FootLTip = 8f + 7f * bounce;
            p.FootRTip = 16f + 8f * bounce;
            p.HandL += new Vector3(0.03f * sway, 0.03f * bounce, 0.05f * sway);
            p.HandR += new Vector3(-0.025f * sway, -0.025f * bounce, 0.03f * sway);
            p.WaistLean += 3f * bounce;
            p.WaistYaw += 5f * sway;
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
            return new ForgeClip("hurt", 0.35f) { Wind = t => new Vector3(0f, 0f, t < 0.3f ? 9f : 0f), Shape = ForgeClip.Shudder(0.2f, 0.35f, 1.2f) }
                .Key(0f, Stand()).Key(0.22f, struck, Ease.Snap).Key(0.5f, struck, Ease.Linear).Key(1f, gather, Ease.InOut);
        }

        private static ForgeClip Death()
        {
            // Struck and arched over backward, a reel the other way, the knees give, down onto them with the head
            // dropping, a moment swaying there, and then pitching forward and round to lie full length, slantwise to
            // the camera so that from above it is a body laid out and not a short figure.
            var struck = new ForgePose
            {
                Hips = new Vector3(0.02f, -0.04f, -0.12f),
                HipLean = -14f,
                WaistLean = -22f,
                WaistYaw = 20f,
                HeadLean = -34f,
                FootL = new Vector3(-0.15f, 0f, 0.06f),
                FootR = new Vector3(0.2f, 0f, -0.2f),
                HandL = new Vector3(-0.52f, 1.2f, -0.1f),
                HandR = new Vector3(0.56f, 1.32f, -0.2f),
                ShoulderLUp = 12f,
                ShoulderRUp = 16f,
            };
            var reel = new ForgePose
            {
                Hips = new Vector3(-0.06f, -0.14f, -0.26f),
                HipLean = -2f,
                HipYaw = -8f,
                WaistLean = 8f,
                WaistYaw = -18f,
                WaistRoll = -8f,
                HeadLean = 12f,
                HeadRoll = 10f,
                FootL = new Vector3(-0.2f, 0f, -0.22f),
                FootR = new Vector3(0.2f, 0f, -0.36f),
                FootRTurn = 30f,
                HandL = new Vector3(-0.04f, 1.14f, 0.0f),
                ElbowL = new Vector3(-1f, -0.4f, 0f),
                HandR = new Vector3(0.46f, 0.92f, -0.14f),
            };
            var sag = new ForgePose
            {
                Hips = new Vector3(-0.02f, -0.34f, -0.2f),
                HipLean = 10f,
                HipYaw = 4f,
                WaistLean = 22f,
                WaistYaw = -6f,
                WaistRoll = 6f,
                HeadLean = 22f,
                FootL = new Vector3(-0.2f, 0f, -0.22f),
                FootR = new Vector3(0.2f, 0f, -0.36f),
                FootRTurn = 30f,
                FootRTip = 30f,
                HandL = new Vector3(-0.08f, 0.9f, 0.1f),
                ElbowL = new Vector3(-1f, -0.4f, 0f),
                HandR = new Vector3(0.36f, 0.58f, 0.0f),
            };
            var knees = new ForgePose
            {
                Hips = new Vector3(0f, -0.54f, -0.04f),
                HipLean = 4f,
                HipYaw = 14f,
                WaistLean = 14f,
                HeadLean = 40f,
                FootL = new Vector3(-0.16f, 0f, -0.4f),
                FootR = new Vector3(0.16f, 0f, -0.42f),
                FootLTip = 70f,
                FootRTip = 70f,
                KneeL = new Vector3(-0.3f, -0.3f, 1f),
                KneeR = new Vector3(0.3f, -0.3f, 1f),
                HandL = new Vector3(-0.28f, 0.34f, 0.1f),
                HandR = new Vector3(0.3f, 0.3f, 0.08f),
            };
            var teeter = knees.Copy();
            teeter.Hips = new Vector3(0.03f, -0.56f, -0.08f);
            teeter.HipLean = -6f;
            teeter.HipYaw = 24f;
            teeter.WaistLean = 28f;
            teeter.WaistRoll = -8f;
            teeter.HeadLean = 48f;
            teeter.HandL = new Vector3(-0.24f, 0.26f, 0.06f);
            teeter.HandR = new Vector3(0.32f, 0.24f, 0.02f);
            var down = new ForgePose
            {
                Hips = new Vector3(0.08f, -0.84f, 0.16f),
                HipLean = 82f,
                HipYaw = 40f,
                WaistLean = 6f,
                HeadLean = -4f,
                HeadYaw = 50f,
                FootL = new Vector3(-0.55f, 0.03f, -0.41f),
                FootR = new Vector3(-0.37f, 0.03f, -0.57f),
                FootLTurn = -40f,
                FootRTurn = 40f,
                FootLTip = 80f,
                FootRTip = 80f,
                KneeL = new Vector3(-0.3f, -1f, 0.2f),
                KneeR = new Vector3(0.2f, -1f, -0.1f),
                HandL = new Vector3(0.14f, 0.06f, 0.89f),
                HandR = new Vector3(0.99f, 0.06f, 0.91f),
                ElbowL = new Vector3(-0.77f, 0.2f, 0.64f),
                ElbowR = new Vector3(0.77f, 0.2f, -0.64f),
            };
            var bounce = down.Copy();
            bounce.Hips = new Vector3(0.08f, -0.8f, 0.17f);
            bounce.HipLean = 78f;
            bounce.HeadLean = -14f;
            return new ForgeClip("death", 1.1f) { SheetMoments = new[] { 0f, 0.1f, 0.26f, 0.42f, 0.56f, 0.68f, 0.86f, 1f } }
                .Key(0f, Stand()).Key(0.1f, struck, Ease.Snap).Key(0.26f, reel, Ease.InOut).Key(0.42f, sag, Ease.In)
                .Key(0.56f, knees, Ease.In).Key(0.68f, teeter, Ease.InOut).Key(0.86f, down, Ease.In).Key(0.92f, bounce, Ease.Out).Key(1f, down, Ease.In);
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
            // The throw carries the hand on across and down, and what it let go kicks the arm back up before it drops.
            var follow = thrown.Copy();
            follow.Hips = new Vector3(0.02f, -0.26f, 0.2f);
            follow.WaistYaw = 46f;
            follow.WaistLean = 22f;
            follow.HandL = new Vector3(0.16f, 1.1f, 0.86f);
            follow.HandR = new Vector3(0.48f, 1.0f, -0.3f);
            Looking(follow);
            var recoil = thrown.Copy();
            recoil.Hips = new Vector3(0f, -0.1f, 0.02f);
            recoil.HipYaw = 10f;
            recoil.WaistYaw = 10f;
            recoil.WaistLean = -4f;
            recoil.FootRTip = 0f;
            recoil.HandL = new Vector3(-0.26f, 1.5f, 0.46f);
            recoil.ElbowL = new Vector3(-1f, -0.6f, 0f);
            recoil.ShoulderLForward = 0f;
            recoil.ShoulderLUp = 14f;
            recoil.HandR = new Vector3(0.36f, 0.92f, -0.04f);
            Looking(recoil);
            return new ForgeClip("cast", kit.SpellWindup + kit.SpellRecovery)
            {
                SheetMoments = Sheet(release, Mathf.Lerp(release, 1f, 0.3f)),
            }.Key(0f, Stand()).Key(release * 0.62f, gather, Ease.Out).Key(release, thrown, Ease.In)
                .Key(Mathf.Lerp(release, 1f, 0.3f), follow, Ease.Out).Key(Mathf.Lerp(release, 1f, 0.68f), recoil, Ease.InOut).Key(1f, Stand(), Ease.InOut);
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

        /// <summary>
        /// The third cut: a step in and a rising cut from the left hip up through to the right shoulder. Seen from above
        /// the blade crosses the whole body from one side to the other, where a cut straight down would only be a line.
        /// </summary>
        private static ForgeClip StepCut(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var coil = Looking(new ForgePose
            {
                Hips = new Vector3(-0.05f, -0.2f, -0.06f),
                HipYaw = -26f,
                HipLean = 8f,
                WaistYaw = -34f,
                WaistLean = 10f,
                FootL = new Vector3(-0.2f, 0f, -0.14f),
                FootR = new Vector3(0.14f, 0.08f, 0.16f),
                FootLTurn = 34f,
                HandR = new Vector3(-0.36f, 0.72f, 0.06f),
                ElbowR = new Vector3(0.6f, -0.6f, -0.4f),
                ElbowL = new Vector3(-0.8f, -0.4f, -0.5f),
                AimBlade = true,
                TwoHanded = true,
                Blade = new Vector3(-0.6f, -0.42f, -0.68f),
            }, 0.9f);
            var cut = Looking(new ForgePose
            {
                Hips = new Vector3(0.02f, -0.16f, 0.2f),
                HipYaw = 12f,
                HipLean = 10f,
                WaistYaw = 26f,
                WaistLean = 4f,
                FootL = new Vector3(-0.19f, 0f, -0.36f),
                FootR = new Vector3(0.15f, 0f, 0.54f),
                FootLTurn = 40f,
                FootLTip = 20f,
                HandR = new Vector3(0.36f, 1.42f, 0.62f),
                ElbowR = new Vector3(0.9f, -0.2f, -0.2f),
                ElbowL = new Vector3(-0.5f, -0.6f, -0.4f),
                ShoulderRUp = 14f,
                AimBlade = true,
                TwoHanded = true,
                Blade = new Vector3(0.42f, 0.66f, 0.62f),
            }, 0.7f);
            var high = cut.Copy();
            high.Hips = new Vector3(0.05f, -0.1f, 0.24f);
            high.WaistYaw = 46f;
            high.WaistLean = -6f;
            high.HeadYaw = -40f;
            high.HandR = new Vector3(0.62f, 1.78f, 0.22f);
            high.ShoulderRUp = 26f;
            high.Blade = new Vector3(0.74f, 0.62f, -0.26f);
            var settle = high.Copy();
            settle.Hips = new Vector3(0.04f, -0.13f, 0.22f);
            settle.WaistYaw = 38f;
            settle.HandR = new Vector3(0.56f, 1.66f, 0.28f);
            settle.Blade = new Vector3(0.68f, 0.7f, -0.2f);
            return Attack(step, -8f).Key(0f, SwordReady()).Key(hit * 0.5f, coil, Ease.Out).Key(hit * 0.82f, coil, Ease.Linear)
                .Key(hit, cut, Ease.In).Key(done, high, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.4f), settle, Ease.Out).Key(1f, SwordReady(), Ease.InOut);
        }

        /// <summary>
        /// The heavy cut: the sword taken back over the right shoulder with the whole body wound round after it, held,
        /// and brought down across to the left with everything. Seen from above the blade starts behind one shoulder
        /// and ends beyond the other hip.
        /// </summary>
        private static ForgeClip Overhead(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var back = Looking(new ForgePose
            {
                Hips = new Vector3(0.04f, -0.08f, -0.14f),
                HipYaw = 22f,
                HipLean = -8f,
                WaistYaw = 40f,
                WaistLean = -14f,
                FootL = new Vector3(-0.17f, 0f, -0.26f),
                FootR = new Vector3(0.16f, 0f, 0.16f),
                FootLTurn = 36f,
                HandR = new Vector3(0.44f, 1.82f, -0.2f),
                ElbowR = new Vector3(1f, 0.2f, -0.3f),
                ElbowL = new Vector3(-0.6f, 0.4f, 0.6f),
                ShoulderRUp = 26f,
                ShoulderLUp = 18f,
                AimBlade = true,
                TwoHanded = true,
                Blade = new Vector3(0.5f, -0.28f, -0.82f),
            }, 0.85f);
            var coiled = back.Copy();
            coiled.Hips = new Vector3(0.05f, -0.13f, -0.18f);
            coiled.WaistYaw = 48f;
            coiled.WaistLean = -18f;
            coiled.HandR = new Vector3(0.5f, 1.86f, -0.28f);
            coiled.Blade = new Vector3(0.56f, -0.42f, -0.72f);
            var cut = Looking(new ForgePose
            {
                Hips = new Vector3(-0.02f, -0.26f, 0.22f),
                HipYaw = -8f,
                HipLean = 20f,
                WaistYaw = -22f,
                WaistLean = 20f,
                FootL = new Vector3(-0.19f, 0f, -0.36f),
                FootR = new Vector3(0.16f, 0f, 0.56f),
                FootLTurn = 40f,
                FootLTip = 24f,
                HandR = new Vector3(-0.12f, 1.02f, 0.72f),
                ElbowR = new Vector3(0.7f, -0.5f, -0.2f),
                AimBlade = true,
                TwoHanded = true,
                Blade = new Vector3(-0.36f, 0.04f, 0.93f),
            }, 0.7f);
            var buried = cut.Copy();
            buried.Hips = new Vector3(-0.04f, -0.42f, 0.27f);
            buried.HipLean = 30f;
            buried.WaistYaw = -40f;
            buried.WaistLean = 28f;
            buried.HeadYaw = 34f;
            buried.HeadLean = -30f;
            buried.HandR = new Vector3(-0.38f, 0.56f, 0.5f);
            buried.Blade = new Vector3(-0.72f, -0.46f, 0.52f);
            var settle = buried.Copy();
            settle.Hips = new Vector3(-0.04f, -0.37f, 0.25f);
            settle.WaistYaw = -34f;
            settle.HandR = new Vector3(-0.34f, 0.62f, 0.5f);
            settle.Blade = new Vector3(-0.68f, -0.4f, 0.6f);
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
                BladeRoll = 90f,
            }, 0.55f);
            // The cut carries on round behind while the body skids on low: nothing stops until the flick.
            var held = through.Copy();
            held.Hips = new Vector3(0.1f, -0.42f, 0.3f);
            held.HipYaw = 44f;
            held.WaistYaw = 70f;
            held.HandR = new Vector3(0.66f, 1.16f, -0.36f);
            held.ShoulderRUp = 18f;
            held.Blade = new Vector3(0.5f, 0.3f, -0.82f);
            Looking(held, 0.55f);
            // The flick that clears the blade: quick, out to the right and level enough to stay off the floor.
            var flick = through.Copy();
            flick.Hips = new Vector3(0.02f, -0.24f, 0.14f);
            flick.HipYaw = 16f;
            flick.WaistYaw = 26f;
            flick.ShoulderRUp = 0f;
            flick.HandR = new Vector3(0.62f, 0.96f, 0.24f);
            flick.Blade = new Vector3(0.66f, -0.34f, 0.66f);
            flick.BladeRoll = 0f;
            Looking(flick);
            // Round the front to the sheath's mouth, so the point never swings down through the ground on its way home.
            var across = flick.Copy();
            across.Hips = new Vector3(0f, -0.2f, 0.06f);
            across.HipYaw = -4f;
            across.WaistYaw = -4f;
            across.HandR = new Vector3(0.1f, 1.0f, 0.42f);
            across.ElbowR = new Vector3(0.9f, -0.2f, 0.2f);
            across.Blade = new Vector3(-0.95f, 0.04f, 0.3f);
            Looking(across);
            var sheathing = Drawing();
            sheathing.Hips = new Vector3(-0.02f, -0.18f, 0f);
            sheathing.HipYaw = -18f;
            sheathing.WaistYaw = -16f;
            sheathing.HandR = new Vector3(-0.1f, 0.9f, 0.34f);
            Looking(sheathing);
            var clip = Attack(step, -12f);
            clip.SheetMoments = new[] { 0f, hit, (hit + done) * 0.5f, done, 0.5f, 0.68f, 0.84f, 1f };
            return clip.Key(0f, Drawing()).Key(hit, cut, Ease.In).Key(done, through, Ease.Out).Key(0.52f, held, Ease.Out)
                .Key(0.68f, flick, Ease.InOut).Key(0.84f, across, Ease.InOut).Key(1f, sheathing, Ease.InOut);
        }

        /// <summary>
        /// The finisher: a draw-cut that ends it. Wound deep away over the sheathed sword, the blade torn out level
        /// across the enemy, carried on round to stand out wide at the side with the body sunk low and still, and then,
        /// slowly, the flick that clears the blade and the sword walked home to the sheath.
        /// </summary>
        private static ForgeClip RunThrough(AttackStep step)
        {
            var (hit, done) = Moments(step);
            // Wound away to the left over the hilt, the right shoulder pointed at the enemy, eyes on them over it.
            var coil = Looking(new ForgePose
            {
                Hips = new Vector3(-0.06f, -0.4f, -0.1f),
                HipYaw = -48f,
                HipLean = 16f,
                WaistYaw = -40f,
                WaistLean = 20f,
                FootL = new Vector3(-0.26f, 0f, -0.34f),
                FootR = new Vector3(0.16f, 0f, 0.4f),
                FootLTurn = 56f,
                FootLTip = 12f,
                FootRTurn = -6f,
                HandL = new Vector3(-0.34f, 0.72f, -0.06f),
                ElbowL = new Vector3(-0.9f, 0.1f, -0.5f),
                HandR = new Vector3(-0.3f, 0.74f, 0.2f),
                ElbowR = new Vector3(0.9f, -0.1f, 0.5f),
                ShoulderRForward = 22f,
                AimBlade = true,
                Blade = new Vector3(-0.55f, -0.2f, -0.82f),
            }, 0.85f);
            var sunk = coil.Copy();
            sunk.Hips = new Vector3(-0.08f, -0.47f, -0.14f);
            sunk.HipYaw = -56f;
            sunk.WaistYaw = -46f;
            sunk.WaistLean = 24f;
            sunk.HandR = new Vector3(-0.34f, 0.7f, 0.16f);
            Looking(sunk, 0.85f);
            // The blade is through them: a long step, the arm thrown out, the sheath hand torn back the other way.
            var cut = Looking(new ForgePose
            {
                Hips = new Vector3(0.03f, -0.4f, 0.34f),
                HipYaw = 8f,
                HipLean = 18f,
                WaistYaw = 22f,
                WaistLean = 16f,
                FootL = new Vector3(-0.26f, 0f, -0.52f),
                FootR = new Vector3(0.18f, 0f, 0.7f),
                FootLTurn = 56f,
                FootLTip = 30f,
                FootRTurn = 0f,
                HandL = new Vector3(-0.42f, 0.74f, -0.2f),
                ElbowL = new Vector3(-0.9f, 0.2f, -0.6f),
                HandR = new Vector3(0.2f, 1.04f, 0.9f),
                ElbowR = new Vector3(1f, -0.2f, 0f),
                ShoulderRForward = 18f,
                AimBlade = true,
                Blade = new Vector3(-0.32f, 0.03f, 0.95f),
                BladeRoll = 45f,
            });
            // The pose time slows on: sunk on the front knee, chest turned right round, the sword standing out level
            // to the side at arm's length and the sheath hand far behind, the head still on the enemy.
            var through = Looking(new ForgePose
            {
                Hips = new Vector3(0.08f, -0.5f, 0.42f),
                HipYaw = 40f,
                HipLean = 14f,
                HipRoll = -4f,
                WaistYaw = 62f,
                WaistLean = 12f,
                FootL = new Vector3(-0.26f, 0f, -0.52f),
                FootR = new Vector3(0.18f, 0f, 0.7f),
                FootLTurn = 56f,
                FootLTip = 34f,
                FootRTurn = 0f,
                HandL = new Vector3(-0.5f, 0.7f, 0.1f),
                ElbowL = new Vector3(-0.9f, 0.2f, -0.6f),
                HandR = new Vector3(0.92f, 1.08f, 0.0f),
                ElbowR = new Vector3(0.6f, -0.3f, -1f),
                ShoulderRUp = 10f,
                AimBlade = true,
                Blade = new Vector3(0.94f, 0.1f, -0.34f),
                BladeRoll = 90f,
            }, 0.5f);
            through.HeadLean = -14f;
            // Still, but not frozen: the weight goes on settling and the point drifts on behind.
            var held = through.Copy();
            held.Hips = new Vector3(0.09f, -0.54f, 0.44f);
            held.WaistYaw = 68f;
            held.HandR = new Vector3(0.9f, 1.02f, -0.12f);
            held.Blade = new Vector3(0.82f, 0.04f, -0.58f);
            Looking(held, 0.5f);
            held.HeadLean = -8f;
            // The flick: the body rises a little and the blade is snapped down and forward at the right side.
            var flick = Looking(new ForgePose
            {
                Hips = new Vector3(0.05f, -0.34f, 0.3f),
                HipYaw = 22f,
                HipLean = 8f,
                WaistYaw = 26f,
                WaistLean = 6f,
                FootL = new Vector3(-0.24f, 0f, -0.44f),
                FootR = new Vector3(0.18f, 0f, 0.62f),
                FootLTurn = 50f,
                FootLTip = 20f,
                HandL = new Vector3(-0.36f, 0.8f, 0.04f),
                ElbowL = new Vector3(-0.9f, 0.1f, -0.5f),
                HandR = new Vector3(0.66f, 0.9f, 0.34f),
                AimBlade = true,
                Blade = new Vector3(0.62f, -0.42f, 0.66f),
            });
            // The back of the blade laid across the front toward the sheath's mouth.
            var across = Looking(new ForgePose
            {
                Hips = new Vector3(0f, -0.24f, 0.16f),
                HipYaw = -6f,
                HipLean = 6f,
                WaistYaw = -8f,
                WaistLean = 8f,
                FootL = new Vector3(-0.22f, 0f, -0.36f),
                FootR = new Vector3(0.17f, 0f, 0.5f),
                FootLTurn = 46f,
                FootLTip = 12f,
                HandL = new Vector3(-0.28f, 0.86f, 0.1f),
                ElbowL = new Vector3(-0.9f, 0.1f, -0.5f),
                HandR = new Vector3(0.12f, 1.0f, 0.44f),
                ElbowR = new Vector3(0.9f, -0.2f, 0.2f),
                AimBlade = true,
                Blade = new Vector3(-0.95f, 0.02f, 0.3f),
            });
            across.HeadLean = 10f;
            var home = Drawing();
            home.Hips = new Vector3(-0.02f, -0.16f, 0.02f);
            home.HipYaw = -16f;
            home.WaistYaw = -14f;
            home.WaistLean = 10f;
            home.HandR = new Vector3(-0.12f, 0.9f, 0.32f);
            Looking(home);
            home.HeadLean = 14f;
            var clip = Attack(step, -12f);
            clip.SheetMoments = new[] { hit * 0.5f, hit * 0.86f, hit, hit + 0.04f, 0.6f, 0.73f, 0.87f, 1f };
            return clip.Key(0f, Drawing()).Key(hit * 0.5f, coil, Ease.Out).Key(hit * 0.86f, sunk, Ease.InOut).Key(hit, cut, Ease.In)
                .Key(hit + 0.07f, through, Ease.Out).Key(0.6f, held, Ease.Out).Key(0.73f, flick, Ease.InOut)
                .Key(0.87f, across, Ease.InOut).Key(1f, home, Ease.InOut);
        }

        private static ForgeClip SwordGuard()
        {
            // Braced behind the blade: sunk onto the back leg with the lead shoulder turned in, the sword held level
            // across the front of the body, the left hand flat against its back to take the blow.
            var guard = SwordReady();
            guard.Hips = new Vector3(-0.02f, -0.26f, -0.08f);
            guard.HipYaw = -8f;
            guard.WaistYaw = -20f;
            guard.WaistLean = 14f;
            guard.FootL = new Vector3(-0.2f, 0f, -0.3f);
            guard.FootR = new Vector3(0.15f, 0f, 0.3f);
            guard.TwoHanded = false;
            guard.HandR = new Vector3(0.36f, 1.22f, 0.5f);
            guard.ElbowR = new Vector3(1f, -0.3f, -0.2f);
            guard.Blade = new Vector3(-0.95f, 0.24f, 0.14f);
            guard.HandL = new Vector3(-0.18f, 1.36f, 0.54f);
            guard.ElbowL = new Vector3(-1f, -0.4f, -0.2f);
            guard.HandLBend = -50f;
            guard.ShoulderRUp = 10f;
            guard.ShoulderLUp = 10f;
            Looking(guard);
            guard.HeadLean = 8f;
            var brace = guard.Copy();
            brace.Hips = new Vector3(-0.02f, -0.31f, -0.12f);
            brace.WaistLean = 18f;
            return new ForgeClip("guard", 0.14f).Key(0f, SwordReady()).Key(0.7f, brace, Ease.Out).Key(1f, guard, Ease.InOut);
        }

        // ------------------------------------------------------------------ fists

        // Seen from above a punch thrown straight ahead is only an arm getting shorter. So every blow here starts out
        // to one side and is carried across the body by the hips and shoulders turning: the line of the shoulders
        // swings like a door, and the fist travels sideways as well as out.

        private static ForgeClip Jab(AttackStep step)
        {
            var (hit, done) = Moments(step);
            // The lead hand cocked out by the left ear with that shoulder drawn back, then whipped in across the middle
            // as the body turns side on behind it.
            var load = FistReady();
            load.Hips = new Vector3(0.02f, -0.14f, -0.06f);
            load.HipYaw = 16f;
            load.WaistYaw = -14f;
            load.HandL = new Vector3(-0.44f, 1.34f, 0.06f);
            load.ElbowL = new Vector3(-1f, -0.3f, -0.5f);
            load.HandR = new Vector3(0.14f, 1.36f, 0.36f);
            Looking(load);
            var reach = FistReady();
            reach.Hips = new Vector3(-0.02f, -0.14f, 0.14f);
            reach.HipYaw = 44f;
            reach.WaistYaw = 36f;
            reach.WaistLean = 10f;
            reach.FootL = new Vector3(-0.12f, 0f, 0.4f);
            reach.FootRTip = 30f;
            reach.HandL = new Vector3(0.1f, 1.4f, 0.96f);
            reach.ElbowL = new Vector3(-1f, -0.2f, 0f);
            reach.ShoulderLForward = 26f;
            reach.ShoulderLUp = 12f;
            reach.HandR = new Vector3(0.36f, 1.2f, -0.1f);
            reach.ElbowR = new Vector3(0.6f, -0.6f, -1f);
            Looking(reach);
            var snap = reach.Copy();
            snap.Hips = new Vector3(-0.02f, -0.16f, 0.18f);
            snap.WaistYaw = 42f;
            snap.HandL = new Vector3(0.14f, 1.38f, 1.0f);
            Looking(snap);
            var back = FistReady();
            back.Hips = new Vector3(0f, -0.07f, -0.03f);
            back.WaistYaw = -2f;
            back.HandL = new Vector3(-0.28f, 1.4f, 0.28f);
            Looking(back);
            return Attack(step, -4f).Key(0f, FistReady()).Key(hit * 0.55f, load, Ease.Out).Key(hit, reach, Ease.In)
                .Key(done, snap, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), back, Ease.Out).Key(1f, FistReady(), Ease.InOut);
        }

        private static ForgeClip Cross(AttackStep step)
        {
            var (hit, done) = Moments(step);
            // The rear hand from out wide by the right shoulder: the hips and chest swing all the way round to the
            // left and carry it across to land left of the middle, the rear heel coming up behind it.
            var load = FistReady();
            load.Hips = new Vector3(0.04f, -0.16f, -0.08f);
            load.HipYaw = 48f;
            load.WaistYaw = 36f;
            load.HandR = new Vector3(0.52f, 1.3f, -0.22f);
            load.ElbowR = new Vector3(1f, -0.2f, -0.6f);
            load.HandL = new Vector3(-0.1f, 1.38f, 0.46f);
            Looking(load, 0.7f);
            var reach = FistReady();
            reach.Hips = new Vector3(-0.04f, -0.18f, 0.16f);
            reach.HipYaw = -22f;
            reach.WaistYaw = -54f;
            reach.WaistLean = 14f;
            reach.WaistRoll = -6f;
            reach.FootR = new Vector3(0.2f, 0f, -0.24f);
            reach.FootRTip = 44f;
            reach.FootRTurn = 6f;
            reach.HandR = new Vector3(-0.14f, 1.38f, 0.98f);
            reach.ElbowR = new Vector3(1f, -0.2f, 0f);
            reach.ShoulderRForward = 30f;
            reach.ShoulderRUp = 12f;
            reach.HandL = new Vector3(-0.42f, 1.2f, -0.1f);
            reach.ElbowL = new Vector3(-0.6f, -0.5f, -1f);
            Looking(reach, 0.7f);
            var through = reach.Copy();
            through.Hips = new Vector3(-0.06f, -0.22f, 0.2f);
            through.WaistYaw = -64f;
            through.HandR = new Vector3(-0.28f, 1.32f, 0.94f);
            Looking(through, 0.7f);
            var back = FistReady();
            back.Hips = new Vector3(0.02f, -0.08f, -0.02f);
            back.WaistYaw = 16f;
            back.HandR = new Vector3(0.26f, 1.34f, 0.28f);
            Looking(back);
            return Attack(step, -4f).Key(0f, FistReady()).Key(hit * 0.5f, load, Ease.Out).Key(hit, reach, Ease.In)
                .Key(done, through, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), back, Ease.Out).Key(1f, FistReady(), Ease.InOut);
        }

        private static ForgeClip Hook(AttackStep step)
        {
            var (hit, done) = Moments(step);
            // A wide right hook: the elbow up, the arm a bar, the body whipping it round from far out on the right to
            // far out on the left.
            var load = FistReady();
            load.Hips = new Vector3(0.06f, -0.2f, -0.04f);
            load.HipYaw = 52f;
            load.WaistYaw = 44f;
            load.WaistRoll = -8f;
            load.HandR = new Vector3(0.76f, 1.26f, -0.08f);
            load.ElbowR = new Vector3(1f, 0.4f, -0.6f);
            load.ShoulderRUp = 18f;
            load.HandL = new Vector3(-0.14f, 1.4f, 0.4f);
            Looking(load, 0.7f);
            var land = FistReady();
            land.Hips = new Vector3(-0.02f, -0.16f, 0.1f);
            land.HipYaw = -6f;
            land.WaistYaw = -24f;
            land.WaistLean = 10f;
            land.WaistRoll = 8f;
            land.FootRTip = 36f;
            land.HandR = new Vector3(0.04f, 1.42f, 0.68f);
            land.ElbowR = new Vector3(1f, 0.6f, 0.2f);
            land.ShoulderRUp = 18f;
            land.ShoulderRForward = 18f;
            land.HandL = new Vector3(-0.26f, 1.3f, 0.1f);
            Looking(land, 0.7f);
            var through = land.Copy();
            through.Hips = new Vector3(-0.06f, -0.14f, 0.1f);
            through.HipYaw = -34f;
            through.WaistYaw = -66f;
            through.WaistRoll = 12f;
            through.HandR = new Vector3(-0.54f, 1.34f, 0.26f);
            through.HandL = new Vector3(-0.38f, 1.2f, -0.16f);
            Looking(through, 0.6f);
            var settle = through.Copy();
            settle.Hips = new Vector3(-0.05f, -0.2f, 0.08f);
            settle.WaistYaw = -54f;
            settle.WaistRoll = 6f;
            settle.HandR = new Vector3(-0.44f, 1.24f, 0.32f);
            Looking(settle, 0.6f);
            return Attack(step, -4f).Key(0f, FistReady()).Key(hit * 0.55f, load, Ease.Out).Key(hit, land, Ease.In)
                .Key(done, through, Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.45f), settle, Ease.Out).Key(1f, FistReady(), Ease.InOut);
        }

        private static ForgeClip FrontKick(AttackStep step)
        {
            var (hit, done) = Moments(step);
            // The fourth blow sweeps from left to right, so it is the lead leg swung round level: the knee lifted out
            // to the left, the shin whipped across in front, the body laid back and away from it over the rear foot.
            var chamber = FistReady();
            chamber.Hips = new Vector3(0.08f, -0.04f, -0.04f);
            chamber.HipYaw = 4f;
            chamber.HipRoll = -8f;
            chamber.WaistYaw = -16f;
            chamber.WaistLean = 4f;
            chamber.FootR = new Vector3(0.16f, 0f, -0.1f);
            chamber.FootRTurn = 50f;
            chamber.FootRTip = 0f;
            chamber.FootL = new Vector3(-0.5f, 0.62f, 0.1f);
            chamber.FootLTip = 30f;
            chamber.KneeL = new Vector3(-1f, 0.6f, 0.4f);
            chamber.HandL = new Vector3(-0.36f, 1.3f, 0.2f);
            chamber.HandR = new Vector3(0.24f, 1.36f, 0.3f);
            Looking(chamber);
            var kick = chamber.Copy();
            kick.Hips = new Vector3(0.1f, -0.04f, 0.04f);
            kick.HipYaw = 50f;
            kick.HipRoll = -16f;
            kick.HipLean = -10f;
            kick.WaistYaw = -20f;
            kick.WaistRoll = -8f;
            kick.FootRTurn = 70f;
            kick.FootL = new Vector3(0.0f, 1.0f, 1.0f);
            kick.FootLTip = -10f;
            kick.FootLTurn = -60f;
            kick.KneeL = new Vector3(-1f, 0.3f, 0.6f);
            kick.HandL = new Vector3(-0.52f, 1.1f, -0.3f);
            kick.ElbowL = new Vector3(-0.6f, -0.3f, -1f);
            kick.HandR = new Vector3(0.3f, 1.36f, 0.26f);
            Looking(kick);
            var through = kick.Copy();
            through.Hips = new Vector3(0.12f, -0.06f, 0.04f);
            through.HipYaw = 78f;
            through.WaistYaw = -30f;
            through.FootL = new Vector3(0.46f, 0.8f, 0.72f);
            through.KneeL = new Vector3(-0.4f, 0.3f, 1f);
            through.HandL = new Vector3(-0.6f, 1.04f, -0.36f);
            Looking(through, 0.9f);
            var recoil = FistReady();
            recoil.Hips = new Vector3(0.06f, -0.06f, 0f);
            recoil.HipYaw = 44f;
            recoil.WaistYaw = -8f;
            recoil.FootR = chamber.FootR;
            recoil.FootRTurn = 50f;
            recoil.FootRTip = 0f;
            recoil.FootL = new Vector3(-0.06f, 0.5f, 0.42f);
            recoil.FootLTip = 30f;
            recoil.KneeL = new Vector3(-0.3f, 0.6f, 1f);
            Looking(recoil);
            var down = FistReady();
            down.Hips = new Vector3(0f, -0.16f, 0.06f);
            down.FootL = new Vector3(-0.12f, 0f, 0.36f);
            down.FootR = new Vector3(0.18f, 0f, -0.12f);
            return Attack(step, -5f).Key(0f, FistReady()).Key(hit * 0.62f, chamber, Ease.Out).Key(hit, kick, Ease.In)
                .Key(done, through, Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.35f), recoil, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.75f), down, Ease.In).Key(1f, FistReady(), Ease.InOut);
        }

        private static ForgeClip Overhand(AttackStep step)
        {
            var (hit, done) = Moments(step);
            // Loaded low with the fist far behind the right shoulder, then thrown over the top with a full step
            // through, landing across the body and driving on down toward the left knee.
            var load = FistReady();
            load.Hips = new Vector3(0.08f, -0.3f, -0.14f);
            load.HipYaw = 54f;
            load.WaistYaw = 48f;
            load.WaistLean = 10f;
            load.WaistRoll = -10f;
            load.HandR = new Vector3(0.66f, 1.5f, -0.4f);
            load.ElbowR = new Vector3(1f, 0.3f, -0.6f);
            load.ShoulderRUp = 20f;
            load.HandL = new Vector3(-0.12f, 1.3f, 0.52f);
            Looking(load, 0.7f);
            var wound = load.Copy();
            wound.Hips = new Vector3(0.09f, -0.35f, -0.18f);
            wound.WaistYaw = 56f;
            wound.HandR = new Vector3(0.7f, 1.62f, -0.46f);
            Looking(wound, 0.7f);
            var land = new ForgePose
            {
                Hips = new Vector3(-0.04f, -0.32f, 0.36f),
                HipYaw = -32f,
                HipLean = 16f,
                WaistYaw = -50f,
                WaistLean = 24f,
                WaistRoll = -8f,
                FootL = new Vector3(-0.16f, 0f, -0.34f),
                FootR = new Vector3(0.14f, 0f, 0.62f),
                FootLTurn = 40f,
                FootLTip = 30f,
                HandR = new Vector3(-0.12f, 1.1f, 0.98f),
                ElbowR = new Vector3(1f, 0.8f, 0f),
                HandL = new Vector3(-0.46f, 1.1f, -0.2f),
                ElbowL = new Vector3(-0.6f, 0.2f, -1f),
                ShoulderRForward = 28f,
                ShoulderRUp = 16f,
            };
            Looking(land, 0.6f);
            var sunk = land.Copy();
            sunk.Hips = new Vector3(-0.06f, -0.44f, 0.4f);
            sunk.WaistYaw = -62f;
            sunk.WaistLean = 34f;
            sunk.HandR = new Vector3(-0.34f, 0.74f, 0.84f);
            sunk.HandL = new Vector3(-0.5f, 1.2f, -0.3f);
            Looking(sunk, 0.6f);
            sunk.HeadLean = -24f;
            var settle = sunk.Copy();
            settle.Hips = new Vector3(-0.05f, -0.34f, 0.34f);
            settle.WaistYaw = -48f;
            settle.WaistLean = 22f;
            settle.HandR = new Vector3(-0.24f, 0.9f, 0.7f);
            Looking(settle, 0.6f);
            return Attack(step, -9f).Key(0f, FistReady()).Key(hit * 0.55f, load, Ease.Out).Key(hit * 0.82f, wound, Ease.InOut).Key(hit, land, Ease.In)
                .Key(done, sunk, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), settle, Ease.InOut).Key(1f, FistReady(), Ease.InOut);
        }

        private static ForgeClip Uppercut(AttackStep step)
        {
            var (hit, done) = Moments(step);
            // Sunk right down with the fist at the hip, then everything upward at once: the body stretched to its
            // full length and off the ground, the fist above the head, the head thrown back after it.
            var dip = FistReady();
            dip.Hips = new Vector3(0.06f, -0.46f, 0.02f);
            dip.HipYaw = 46f;
            dip.WaistYaw = 34f;
            dip.WaistLean = 28f;
            dip.WaistRoll = -10f;
            dip.HandR = new Vector3(0.38f, 0.58f, -0.06f);
            dip.ElbowR = new Vector3(1f, -0.2f, -0.6f);
            dip.HandL = new Vector3(-0.16f, 1.16f, 0.4f);
            Looking(dip);
            dip.HeadLean = -24f;
            var rise = FistReady();
            rise.Hips = new Vector3(-0.02f, 0.16f, 0.14f);
            rise.HipYaw = -22f;
            rise.HipLean = -6f;
            rise.WaistYaw = -36f;
            rise.WaistLean = -16f;
            rise.FootL = new Vector3(-0.12f, 0.1f, 0.26f);
            rise.FootR = new Vector3(0.16f, 0.18f, -0.04f);
            rise.FootLTip = 50f;
            rise.FootRTip = 60f;
            rise.HandR = new Vector3(0.02f, 2.2f, 0.34f);
            rise.ElbowR = new Vector3(0.6f, -0.3f, 1f);
            rise.ShoulderRUp = 30f;
            rise.HandL = new Vector3(-0.44f, 0.9f, -0.16f);
            rise.ElbowL = new Vector3(-0.6f, -0.2f, -1f);
            Looking(rise, 0.6f);
            rise.HeadLean = -30f;
            var hang = rise.Copy();
            hang.Hips = new Vector3(-0.02f, 0.23f, 0.17f);
            hang.WaistYaw = -44f;
            hang.FootL = new Vector3(-0.12f, 0.18f, 0.24f);
            hang.FootR = new Vector3(0.16f, 0.28f, -0.08f);
            hang.HandR = new Vector3(-0.02f, 2.27f, 0.3f);
            var land = FistReady();
            land.Hips = new Vector3(0f, -0.3f, 0.1f);
            land.WaistLean = 18f;
            land.HandR = new Vector3(0.22f, 1.46f, 0.3f);
            land.HandL = new Vector3(-0.3f, 1.1f, 0.3f);
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
            // Out of the coil in one line: the whole right side comes round and through, the left arm torn back the
            // other way, and it drives on into the target before it gives any of it back.
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
                HandL = new Vector3(-0.36f, 1.0f, -0.16f),
                ElbowL = new Vector3(-0.6f, 0.1f, -1f),
                HandR = new Vector3(-0.1f, 1.24f, 1.04f),
                ElbowR = new Vector3(1f, -0.3f, 0f),
                ShoulderRForward = 30f,
                ShoulderRUp = 10f,
            }, 0.6f);
            var driven = strike.Copy();
            driven.Hips = new Vector3(-0.04f, -0.42f, 0.5f);
            driven.WaistYaw = -64f;
            driven.WaistLean = 22f;
            driven.HandR = new Vector3(-0.2f, 1.16f, 1.08f);
            driven.HandL = new Vector3(-0.46f, 1.04f, -0.3f);
            Looking(driven, 0.6f);
            var settled = strike.Copy();
            settled.Hips = new Vector3(-0.02f, -0.26f, 0.34f);
            settled.HipYaw = -20f;
            settled.WaistYaw = -34f;
            settled.WaistLean = 8f;
            settled.HandR = new Vector3(0f, 1.3f, 0.76f);
            settled.HandL = new Vector3(-0.3f, 1.16f, 0.04f);
            Looking(settled, 0.6f);
            var clip = Attack(step, -12f);
            clip.SheetMoments = new[] { 0f, hit * 0.6f, hit, (hit + done) * 0.5f, done, 0.6f, 0.8f, 1f };
            return clip.Key(0f, Coiled()).Key(hit, strike, Ease.In).Key(done, driven, Ease.Out).Key(0.62f, settled, Ease.InOut).Key(1f, FistReady(), Ease.InOut);
        }

        private static ForgeClip PalmStrike(AttackStep step)
        {
            var (hit, done) = Moments(step);
            // The blow that puts them down, and the biggest shape of the set: both hands gathered at the right hip
            // with the whole body wound round over them, driven out together on a long step as it unwinds, and then
            // the arms flung wide open as the force leaves them.
            var gather = new ForgePose
            {
                Hips = new Vector3(0.08f, -0.34f, -0.18f),
                HipYaw = 50f,
                HipLean = 4f,
                WaistYaw = 40f,
                WaistLean = 14f,
                WaistRoll = -8f,
                FootL = new Vector3(-0.16f, 0f, 0.2f),
                FootR = new Vector3(0.24f, 0f, -0.32f),
                FootRTurn = 50f,
                HandL = new Vector3(0.26f, 1.08f, -0.04f),
                HandR = new Vector3(0.44f, 0.94f, -0.2f),
                ElbowL = new Vector3(-0.4f, -0.6f, 0.6f),
                ElbowR = new Vector3(0.6f, 0f, -1f),
                HandLBend = -40f,
                HandRBend = 40f,
            };
            Looking(gather, 0.7f);
            var wound = gather.Copy();
            wound.Hips = new Vector3(0.1f, -0.42f, -0.24f);
            wound.WaistYaw = 50f;
            wound.WaistLean = 18f;
            wound.HandL = new Vector3(0.3f, 1.02f, -0.12f);
            wound.HandR = new Vector3(0.48f, 0.88f, -0.28f);
            Looking(wound, 0.7f);
            var strike = new ForgePose
            {
                Hips = new Vector3(-0.02f, -0.36f, 0.44f),
                HipYaw = -20f,
                HipLean = 18f,
                WaistYaw = -30f,
                WaistLean = 18f,
                FootL = new Vector3(-0.18f, 0f, 0.7f),
                FootR = new Vector3(0.2f, 0f, -0.42f),
                FootRTurn = 30f,
                FootRTip = 44f,
                HandL = new Vector3(-0.16f, 1.06f, 1.0f),
                HandR = new Vector3(0.06f, 1.36f, 1.06f),
                ElbowL = new Vector3(-1f, -0.3f, 0f),
                ElbowR = new Vector3(1f, -0.3f, 0f),
                ShoulderLForward = 26f,
                ShoulderRForward = 26f,
                HandLBend = -60f,
                HandRBend = 60f,
            };
            Looking(strike, 0.6f);
            strike.HeadLean = -20f;
            var driven = strike.Copy();
            driven.Hips = new Vector3(-0.03f, -0.4f, 0.52f);
            driven.WaistYaw = -38f;
            driven.WaistLean = 22f;
            driven.HandL = new Vector3(-0.2f, 1.02f, 1.06f);
            driven.HandR = new Vector3(0.02f, 1.34f, 1.12f);
            Looking(driven, 0.6f);
            driven.HeadLean = -24f;
            // What time slows on: the force gone out of the hands and the arms flung right open, one high and
            // forward and one low and behind, on a body sunk over the front knee and turned to show its whole width.
            var burst = new ForgePose
            {
                Hips = new Vector3(-0.03f, -0.48f, 0.5f),
                HipYaw = -30f,
                HipLean = 12f,
                HipRoll = 4f,
                WaistYaw = -44f,
                WaistLean = 4f,
                WaistRoll = 8f,
                FootL = strike.FootL,
                FootR = strike.FootR,
                FootRTurn = 30f,
                FootRTip = 50f,
                HandL = new Vector3(-0.8f, 0.86f, -0.1f),
                HandR = new Vector3(0.9f, 1.5f, 0.56f),
                ElbowL = new Vector3(-0.3f, -1f, -0.3f),
                ElbowR = new Vector3(0.6f, -1f, -0.2f),
                ShoulderLUp = 4f,
                ShoulderRUp = 22f,
                ShoulderRForward = 10f,
                HandLBend = -40f,
                HandRBend = 50f,
            };
            Looking(burst, 0.6f);
            burst.HeadLean = -16f;
            // Held, and still opening: the chest goes on turning and the low hand on reaching back.
            var open = burst.Copy();
            open.Hips = new Vector3(-0.03f, -0.52f, 0.52f);
            open.WaistYaw = -52f;
            open.HandL = new Vector3(-0.84f, 0.8f, -0.24f);
            open.HandR = new Vector3(0.96f, 1.54f, 0.44f);
            Looking(open, 0.6f);
            open.HeadLean = -10f;
            var ebb = new ForgePose
            {
                Hips = new Vector3(-0.01f, -0.24f, 0.32f),
                HipYaw = -6f,
                HipLean = 6f,
                WaistYaw = -10f,
                WaistLean = 6f,
                FootL = strike.FootL,
                FootR = strike.FootR,
                FootRTurn = 30f,
                FootRTip = 36f,
                HandL = new Vector3(-0.52f, 0.98f, 0.26f),
                HandR = new Vector3(0.54f, 1.16f, 0.36f),
                ElbowL = new Vector3(-0.3f, -1f, -0.3f),
                ElbowR = new Vector3(0.3f, -1f, -0.3f),
                HandLBend = -20f,
                HandRBend = 20f,
            };
            var clip = Attack(step, -9f);
            clip.SheetMoments = new[] { 0f, hit * 0.7f, hit, hit + 0.04f, done, 0.62f, 0.8f, 1f };
            return clip.Key(0f, FistReady()).Key(hit * 0.6f, gather, Ease.Out).Key(hit * 0.84f, wound, Ease.InOut).Key(hit, strike, Ease.In)
                .Key(hit + 0.03f, driven, Ease.Out).Key(hit + 0.1f, burst, Ease.Out).Key(0.62f, open, Ease.Out).Key(0.82f, ebb, Ease.InOut).Key(1f, FistReady(), Ease.InOut);
        }

        private static ForgeClip FistGuard()
        {
            // Braced: sunk onto the back leg with the lead shoulder turned in, the lead forearm a bar across the front
            // of the face and the other fist behind it holding it up.
            var guard = FistReady();
            guard.Hips = new Vector3(0.02f, -0.26f, -0.08f);
            guard.HipYaw = 34f;
            guard.WaistYaw = 6f;
            guard.WaistLean = 16f;
            guard.FootL = new Vector3(-0.14f, 0f, 0.3f);
            guard.FootR = new Vector3(0.24f, 0f, -0.26f);
            guard.HandL = new Vector3(0.06f, 1.46f, 0.48f);
            guard.ElbowL = new Vector3(-1f, 0f, 0.2f);
            guard.HandLBend = 0f;
            guard.HandR = new Vector3(0.04f, 1.3f, 0.36f);
            guard.ElbowR = new Vector3(1f, -0.1f, 0.2f);
            guard.ShoulderRForward = 16f;
            guard.ShoulderLUp = 18f;
            guard.ShoulderLForward = 16f;
            guard.ShoulderRUp = 10f;
            Looking(guard);
            guard.HeadLean = 10f;
            var brace = guard.Copy();
            brace.Hips = new Vector3(0.02f, -0.31f, -0.12f);
            brace.WaistLean = 20f;
            return new ForgeClip("guard_fist", 0.14f).Key(0f, FistReady()).Key(0.7f, brace, Ease.Out).Key(1f, guard, Ease.InOut);
        }
    }
}

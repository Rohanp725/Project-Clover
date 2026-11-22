using System;
using System.Collections.Generic;
using Clover.Player;
using UnityEngine;

namespace Clover.EditorTools.Forge
{
    /// <summary>
    /// The Twin Knives' clips. The fists turn the shoulders like a door; the knives turn the whole body like a top.
    /// Both hands are aimed (the right as every weapon's, the left through <see cref="ForgePose.AimBladeL"/>), and
    /// since a knife is a few pixels from the game's camera every key pose holds the blades level and well out from
    /// the body, where the hat does not cover them: the read is two bright ticks on the ends of the arms.
    /// The passing cuts turn the skeleton's root; the game turns the player the rest of the way when the swing ends
    /// (<see cref="AttackStep.PassThrough"/>), so those clips come back to a root at rest from that moment.
    /// </summary>
    public static partial class PlayerAnimations
    {
        private static IEnumerable<ForgeClip> KnifeClips()
        {
            var combo = Weapons.TwinKnives().Combo;
            yield return KnifeIdle();
            yield return KnifeRun();
            yield return KnifeRoll();
            yield return KnifeGuard();
            yield return KnifeGuardHit();
            yield return KnifeGuardPerfect();
            yield return KnifeCharge(combo.ChargeTime);
            yield return KnifeFlick(combo.Steps[0]);
            yield return KnifeBackCut(combo.Steps[1]);
            yield return KnifeCross(combo.Steps[2]);
            yield return KnifeRip(combo.Steps[3]);
            yield return KnifePassingSpin(combo.Steps[4]);
            yield return KnifeLunge(combo.Heavy);
            yield return KnifeCrossingStep(combo.ChainHeavy);
            yield return KnifeDashCut(combo.DashAttack);
            yield return KnifeFan(combo.Skill);
            yield return KnifeThreeCrossings(combo.Finisher);
        }

        // A pose with a knife in each hand: where each hand is and where each blade points.
        private static ForgePose Knifer(Vector3 hips, float hipYaw, float waistYaw, float lean, Vector3 footL, Vector3 footR,
            Vector3 handL, Vector3 bladeL, Vector3 handR, Vector3 bladeR, float keep = 0.85f)
        {
            return Looking(new ForgePose
            {
                Hips = hips,
                HipYaw = hipYaw,
                HipLean = lean * 0.4f,
                WaistYaw = waistYaw,
                WaistLean = lean * 0.6f,
                FootL = footL,
                FootR = footR,
                FootLTurn = 12f,
                FootRTurn = 24f,
                HandL = handL,
                HandR = handR,
                // Elbows out and a little back: the arms are wings, not pistons.
                ElbowL = new Vector3(-1f, -0.3f, -0.5f),
                ElbowR = new Vector3(1f, -0.3f, -0.5f),
                ShoulderLUp = 8f,
                ShoulderRUp = 8f,
                AimBlade = true,
                Blade = bladeR,
                AimBladeL = true,
                BladeL = bladeL,
                // Held across the palm, as the rack hangs them: the hands stay in line with the forearms.
                GripAngle = WeaponBuilder.KnifeGripAngle,
            }, keep);
        }

        private static readonly Vector3 KnifeFootL = new Vector3(-0.14f, 0f, 0.28f), KnifeFootR = new Vector3(0.22f, 0f, -0.24f);

        /// <summary>Low and narrow, side on with the left shoulder leading: the left knife reversed out in front, the right at the hip pointing at the enemy.</summary>
        private static ForgePose KnifeReady() => With(Knifer(V(0f, -0.22f, 0f), 30f, 8f, 14f, KnifeFootL, KnifeFootR,
            V(-0.36f, 1.26f, 0.46f), V(-0.8f, -0.1f, 0.6f), V(0.46f, 0.98f, 0.16f), V(0.25f, 0.05f, 1f)), p =>
        {
            p.FootRTip = 16f;
            p.FootRTurn = 44f;
        });

        private static ForgeClip KnifeIdle() => new ForgeClip("idle_knife", 1.1f, true).From(t =>
        {
            // Never still: a bounce on the toes, and once a loop the right wrist rolls the knife over and back.
            float bounce = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 2f));
            var p = KnifeReady();
            p.Hips += V(0f, 0.03f * bounce, 0f);
            p.HandL += V(0f, 0.02f * bounce, 0f);
            p.FootLTip = 8f * bounce;
            float spin = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.55f, 0.85f, t));
            p.WristRollR = -150f * Mathf.Sin(spin * Mathf.PI);
            return p;
        });

        /// <summary>The fastest run: bent far forward, both arms trailing straight back and low, the blades two lines behind the body.</summary>
        private static ForgeClip KnifeRun()
        {
            var clip = Renamed(RunFist(), "run_knife");
            clip.Seconds = 0.46f;
            Func<float, ForgePose, ForgePose> trailing = (t, p) =>
            {
                float sway = Mathf.Sin(t * Mathf.PI * 2f);
                p.Hips += V(0f, -0.1f, 0.06f);
                p.HipLean += 8f;
                p.WaistLean += 26f;
                p.HeadLean -= 26f;
                p.HandL = V(-0.52f, 0.86f + 0.03f * sway, -0.5f);
                p.HandR = V(0.52f, 0.86f - 0.03f * sway, -0.5f);
                p.ElbowL = V(-1f, 0.3f, -0.2f);
                p.ElbowR = V(1f, 0.3f, -0.2f);
                p.AimBlade = true;
                p.Blade = V(0.45f, 0f, -0.9f);
                p.AimBladeL = true;
                p.BladeL = V(-0.45f, 0f, -0.9f);
                p.GripAngle = WeaponBuilder.KnifeGripAngle;
                p.TwoHanded = false;
                return p;
            };
            clip.Shape = clip.Shape != null ? Both(clip.Shape, trailing) : trailing;
            return clip;
        }

        /// <summary>A stepping pirouette, not a roll: one full turn on the lead foot, low, arms folded in and then flung out.</summary>
        private static ForgeClip KnifeRoll()
        {
            var folded = Knifer(V(0f, -0.42f, 0f), 0f, 0f, 22f, V(-0.1f, 0f, 0.1f), V(0.12f, 0.14f, -0.12f),
                V(-0.22f, 1.0f, 0.24f), V(-0.7f, 0f, 0.7f), V(0.22f, 1.0f, 0.24f), V(0.7f, 0f, 0.7f), 0f);
            var flung = With(Knifer(V(0f, -0.36f, 0f), 20f, 6f, 16f, V(-0.16f, 0f, 0.3f), V(0.24f, 0f, -0.3f),
                V(-0.84f, 1.08f, 0.1f), V(-1f, 0f, 0.1f), V(0.84f, 1.08f, 0f), V(1f, 0f, -0.1f)), p => p.FootRTip = 30f);
            var clip = new ForgeClip("roll_knife", 0.26f) { Wind = t => new Vector3(t < 0.8f ? 6f : 0f, 0f, t < 0.8f ? -8f : 0f) };
            clip.Key(0f, KnifeReady()).Key(0.25f, folded, Ease.Out).Key(0.62f, folded, Ease.Linear).Key(0.9f, flung, Ease.Snap).Key(1f, flung, Ease.Linear);
            clip.MovesRoot = true;
            clip.After = TurnAbout(t => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.04f, 0.9f, t)) * 360f, Vector3.up, Vector3.zero);
            return clip;
        }

        // The cross: both knives in front of the face, tipped forward so the X is seen from above.
        private static ForgePose KnifeCrossed() => With(Knifer(V(0f, -0.14f, -0.02f), 12f, 0f, 8f, V(-0.16f, 0f, 0.18f), V(0.2f, 0f, -0.18f),
            V(-0.14f, 1.4f, 0.44f), V(0.7f, 0.5f, 0.5f), V(0.14f, 1.36f, 0.5f), V(-0.7f, 0.5f, 0.5f)), p =>
        {
            p.ElbowL = V(-0.6f, -1f, 0f);
            p.ElbowR = V(0.6f, -1f, 0f);
            p.HeadLean = 10f;
        });

        private static ForgeClip KnifeGuard() => new ForgeClip("guard_knife", 0.1f).Key(0f, KnifeReady()).Key(1f, KnifeCrossed(), Ease.Out);

        private static ForgeClip KnifeGuardHit()
        {
            var driven = With(KnifeCrossed(), p =>
            {
                p.Hips = V(0f, -0.2f, -0.2f);
                p.HandL = V(-0.12f, 1.54f, 0.26f);
                p.HandR = V(0.12f, 1.5f, 0.3f);
                p.FootL += V(0f, 0f, -0.14f);
                p.FootR += V(0f, 0f, -0.14f);
                p.WaistLean = -4f;
                p.HeadLean = 18f;
            });
            return new ForgeClip("guard_hit_knife", 0.22f).Key(0f, KnifeCrossed()).Key(0.25f, driven, Ease.Snap).Key(1f, KnifeCrossed(), Ease.InOut);
        }

        /// <summary>The cross opens like scissors, throwing the blow out to the left, and the body slips right past it with the right knife already drawn back.</summary>
        private static ForgeClip KnifeGuardPerfect()
        {
            var open = With(Knifer(V(0.2f, -0.26f, 0.1f), -30f, -20f, 10f, V(-0.1f, 0f, 0.3f), V(0.34f, 0f, -0.1f),
                V(-0.82f, 1.3f, 0.36f), V(-1f, 0.1f, 0.3f), V(0.56f, 1.16f, -0.3f), V(0.2f, 0f, 1f)), p => p.ElbowR = V(0.6f, 0f, -1f));
            return new ForgeClip("guard_perfect_knife", 0.3f).Key(0f, KnifeCrossed()).Key(0.25f, open, Ease.Snap).Key(0.62f, open, Ease.Linear).Key(1f, KnifeReady(), Ease.InOut);
        }

        // A set spring: crouched on the back leg, both knives together at the left hip.
        private static ForgePose KnifeCoiled() => With(Knifer(V(0f, -0.44f, -0.14f), 44f, 20f, 22f, V(-0.12f, 0f, 0.3f), V(0.24f, 0f, -0.3f),
            V(-0.44f, 0.86f, -0.04f), V(-0.5f, 0f, -0.86f), V(-0.2f, 0.9f, 0.16f), V(-0.86f, 0f, -0.5f)), p =>
        {
            p.ElbowR = V(0.4f, -0.4f, 1f);
            p.FootRTip = 34f;
            p.ShoulderRForward = 24f;
        });

        private static ForgeClip KnifeCharge(float seconds) =>
            new ForgeClip("charge_knife", seconds).Key(0f, KnifeReady()).Key(0.7f, KnifeCoiled(), Ease.Out).Key(1f, With(KnifeCoiled(), p => p.Hips += V(0f, -0.03f, 0f)), Ease.InOut);

        /// <summary>Cut 1: a flat backhand flick of the right knife, left to right at throat height, the hips turning with it.</summary>
        private static ForgeClip KnifeFlick(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var drawn = With(Knifer(V(0f, -0.26f, -0.04f), 44f, 24f, 14f, KnifeFootL, KnifeFootR,
                V(-0.4f, 1.2f, 0.36f), V(-0.8f, -0.1f, 0.6f), V(-0.34f, 1.42f, 0.34f), V(-0.9f, 0f, 0.4f)), p =>
            {
                p.ElbowR = V(0.3f, -0.3f, 1f);
                p.ShoulderRForward = 26f;
            });
            var cut = With(Knifer(V(0f, -0.26f, 0.12f), -6f, -10f, 14f, V(-0.14f, 0f, 0.36f), KnifeFootR,
                V(-0.46f, 1.14f, 0.1f), V(-0.8f, -0.1f, 0.5f), V(0.56f, 1.42f, 0.78f), V(0.75f, 0f, 0.66f)), p => p.FootRTip = 28f);
            var carried = With(cut, p =>
            {
                p.HandR = V(0.84f, 1.4f, 0.34f);
                p.Blade = V(1f, 0f, 0f);
                p.HipYaw = -16f;
                p.WaistYaw = -18f;
                // The left is already coming.
                p.HandL = V(0.1f, 1.38f, 0.3f);
                p.BladeL = V(0.8f, 0f, 0.5f);
                Looking(p);
            });
            return Attack(step, -4f).Key(0f, KnifeReady()).Key(hit * 0.6f, drawn, Ease.Out).Key(hit, cut, Ease.In).Key(done, carried, Ease.Out)
                .Key(1f, With(carried, p => p.Hips += V(0f, 0.02f, -0.02f)), Ease.InOut);
        }

        /// <summary>Cut 2: the left, reversed, cutting right to left as the left hip comes through; the right recoils to the ribs.</summary>
        private static ForgeClip KnifeBackCut(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var drawn = With(Knifer(V(0f, -0.26f, 0.08f), -16f, -18f, 14f, V(-0.14f, 0f, 0.36f), KnifeFootR,
                V(0.3f, 1.4f, 0.34f), V(0.9f, 0f, 0.4f), V(0.8f, 1.34f, 0.28f), V(1f, 0f, 0f)), p =>
            {
                p.ElbowL = V(-0.3f, -0.3f, 1f);
                p.ShoulderLForward = 26f;
            });
            var cut = With(Knifer(V(0f, -0.26f, 0.16f), 40f, 30f, 14f, V(-0.14f, 0f, 0.4f), KnifeFootR,
                V(-0.56f, 1.42f, 0.78f), V(-0.75f, 0f, 0.66f), V(0.4f, 1.12f, 0f), V(0.3f, 0f, 1f)), p => p.FootRTip = 34f);
            var carried = With(cut, p =>
            {
                p.HandL = V(-0.84f, 1.38f, 0.32f);
                p.BladeL = V(-1f, 0f, 0f);
                p.HipYaw = 50f;
                Looking(p);
            });
            return Attack(step, -4f).Key(0f, drawn).Key(hit * 0.5f, drawn, Ease.Linear).Key(hit, cut, Ease.In).Key(done, carried, Ease.Out)
                .Key(1f, With(carried, p => p.Hips += V(0f, 0.02f, -0.02f)), Ease.InOut);
        }

        /// <summary>Cut 3, the cross, two blows: both hands thrown wide with a hop, both knives cut inward and cross, then cut outward again as the arms are flung open.</summary>
        private static ForgeClip KnifeCross(AttackStep step)
        {
            var (hit, done) = Moments(step);
            float second = (step.Windup + step.Repeats[0]) / step.Total;
            var wide = With(Knifer(V(0f, -0.1f, -0.04f), 0f, 0f, -6f, V(-0.2f, 0.06f, 0.1f), V(0.2f, 0.06f, -0.06f),
                V(-0.86f, 1.36f, 0.06f), V(-0.9f, 0f, 0.44f), V(0.86f, 1.36f, 0.06f), V(0.9f, 0f, 0.44f)), p =>
            {
                p.ShoulderLUp = p.ShoulderRUp = 16f;
                p.FootLTip = p.FootRTip = 20f;
            });
            var crossed = With(Knifer(V(0f, -0.3f, 0.16f), 0f, 0f, 22f, V(-0.2f, 0f, 0.34f), V(0.22f, 0f, -0.26f),
                V(0.3f, 1.34f, 0.74f), V(0.8f, 0f, 0.6f), V(-0.3f, 1.26f, 0.8f), V(-0.8f, 0f, 0.6f)), p =>
            {
                p.ElbowL = V(-0.8f, -0.4f, 0.6f);
                p.ElbowR = V(0.8f, -0.4f, 0.6f);
                p.ShoulderLForward = p.ShoulderRForward = 26f;
                p.FootRTip = 30f;
            });
            var open = With(Knifer(V(0f, -0.26f, 0.22f), 0f, 0f, 10f, V(-0.2f, 0f, 0.36f), V(0.22f, 0f, -0.26f),
                V(-0.9f, 1.3f, 0.4f), V(-0.85f, 0f, 0.5f), V(0.9f, 1.3f, 0.4f), V(0.85f, 0f, 0.5f)), p =>
            {
                p.FootRTip = 30f;
                p.HeadLean = -8f;
            });
            var clip = Attack(step, -5f);
            clip.SheetMoments = new[] { 0f, hit * 0.6f, hit * 0.92f, hit, (hit + second) * 0.5f, second, done, 1f };
            return clip.Key(0f, KnifeReady()).Key(hit * 0.62f, wide, Ease.Out).Key(hit, crossed, Ease.In).Key(second, open, Ease.In)
                .Key(Mathf.Lerp(done, 1f, 0.5f), open, Ease.Linear).Key(1f, With(open, p => p.Hips = V(0f, -0.3f, 0.16f)), Ease.InOut);
        }

        /// <summary>Cut 4: the body drops under its own arms, and the right knife rips upward from the left hip to over the right shoulder as the legs drive up.</summary>
        private static ForgeClip KnifeRip(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var dropped = With(Knifer(V(0f, -0.5f, 0.06f), 36f, 26f, 30f, V(-0.2f, 0f, 0.34f), V(0.22f, 0f, -0.3f),
                V(-0.7f, 1.0f, 0.1f), V(-0.9f, -0.2f, 0.3f), V(-0.36f, 0.6f, 0.4f), V(-0.6f, -0.5f, 0.6f)), p =>
            {
                p.ElbowR = V(0.4f, -0.4f, 1f);
                p.ShoulderRForward = 24f;
                p.FootRTip = 30f;
            });
            var ripped = With(Knifer(V(0f, -0.04f, 0.2f), -24f, -22f, -8f, V(-0.2f, 0f, 0.36f), V(0.2f, 0.04f, -0.16f),
                V(-0.6f, 0.96f, -0.1f), V(-0.8f, -0.3f, -0.4f), V(0.5f, 1.96f, 0.56f), V(0.5f, 0.6f, 0.6f)), p =>
            {
                p.ShoulderRUp = 30f;
                p.FootRTip = 40f;
                p.HeadLean = -10f;
            });
            var high = With(ripped, p =>
            {
                p.HandR = V(0.7f, 1.98f, 0.14f);
                p.Blade = V(0.8f, 0.5f, -0.3f);
                p.HipYaw = -40f;
                p.WaistYaw = -30f;
                Looking(p);
            });
            return Attack(step, -4f).Key(0f, KnifeReady()).Key(hit * 0.7f, dropped, Ease.Out).Key(hit, ripped, Ease.In).Key(done, high, Ease.Out)
                .Key(1f, high, Ease.Linear);
        }

        // Both knives at arm's length, one high and one low: what the passing spin and the dodge's cut whirl.
        private static ForgePose KnifeWhirl(float hipsDown) => With(Knifer(V(0f, hipsDown, 0f), 0f, 0f, 14f, V(-0.12f, 0f, 0.14f), V(0.2f, 0.1f, -0.2f),
            V(-0.88f, 1.5f, 0f), V(-1f, 0.12f, 0f), V(0.9f, 1.06f, 0f), V(1f, -0.1f, 0f), 0f), p =>
        {
            p.ElbowL = V(-1f, 0.2f, -0.4f);
            p.ElbowR = V(1f, 0.2f, -0.4f);
            p.FootRTip = 30f;
        });

        // Low, wide, both knives trailing: the skid a passing cut ends in.
        private static ForgePose KnifeSkid() => With(Knifer(V(0f, -0.46f, -0.06f), 24f, 10f, 26f, V(-0.2f, 0f, 0.44f), V(0.26f, 0f, -0.5f),
            V(-0.62f, 0.8f, -0.36f), V(-0.6f, -0.1f, -0.8f), V(0.66f, 0.84f, -0.4f), V(0.6f, -0.1f, -0.8f)), p =>
        {
            p.FootRTip = 50f;
            p.FootLTurn = -20f;
        });

        /// <summary>
        /// Cut 5, the passing spin: the high right hand pulls the body into a turn; a turn and a half with both knives
        /// out, carried through the target; a skid facing its back. The root turns 540 degrees by the end of the
        /// swing, which is the half turn the game then makes for real.
        /// </summary>
        private static ForgeClip KnifePassingSpin(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var pulled = With(Knifer(V(0f, -0.2f, 0.04f), -50f, -30f, 10f, V(0.06f, 0f, 0.3f), V(0.2f, 0f, -0.2f),
                V(-0.5f, 1.2f, 0.4f), V(-0.8f, 0f, 0.6f), V(0.6f, 1.9f, -0.1f), V(0.7f, 0.4f, -0.6f)), p => p.ShoulderRUp = 26f);
            var clip = Attack(step, -6f);
            clip.SheetMoments = new[] { 0f, hit * 0.4f, hit * 0.75f, hit, (hit + done) * 0.5f, done, Mathf.Lerp(done, 1f, 0.3f), 1f };
            clip.Key(0f, pulled).Key(hit * 0.35f, pulled, Ease.Linear).Key(hit * 0.8f, KnifeWhirl(-0.3f), Ease.Out).Key(done, KnifeWhirl(-0.34f), Ease.Linear)
                .Key(Mathf.Lerp(done, 1f, 0.3f), KnifeSkid(), Ease.Out).Key(1f, KnifeSkid(), Ease.Linear);
            clip.MovesRoot = true;
            clip.After = TurnAbout(t => t >= done ? 0f : Mathf.Lerp(0f, 540f, Mathf.InverseLerp(hit * 0.3f, done, t)), Vector3.up, Vector3.zero);
            return clip;
        }

        /// <summary>The lunge: both knives drawn back to the right hip, coiled low and held; a flat leap, both points driven in together; the knives torn out sideways.</summary>
        private static ForgeClip KnifeLunge(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var coiled = With(Knifer(V(0f, -0.46f, -0.2f), -40f, -24f, 24f, V(-0.14f, 0f, 0.26f), V(0.24f, 0f, -0.34f),
                V(0.2f, 0.9f, -0.06f), V(0.3f, 0f, 1f), V(0.5f, 0.86f, -0.26f), V(0.3f, 0f, 1f)), p =>
            {
                p.ElbowL = V(-0.4f, -0.4f, 1f);
                p.ShoulderLForward = 24f;
                p.FootRTip = 40f;
            });
            // The arms reach past either side of the head and the blades splay a little outward, a swallow's tail turned
            // forward: with both points together on the centre line the hat covers the knives from the game's camera.
            var dart = With(Knifer(V(0f, -0.38f, 0.44f), 0f, 0f, 46f, V(-0.12f, 0.1f, 0.2f), V(0.14f, 0.34f, -0.7f),
                V(-0.34f, 1.06f, 1.1f), V(-0.36f, 0f, 0.93f), V(0.34f, 1.02f, 1.12f), V(0.36f, 0f, 0.93f)), p =>
            {
                p.ElbowL = V(-1f, 0f, 0f);
                p.ElbowR = V(1f, 0f, 0f);
                p.ShoulderLForward = p.ShoulderRForward = 30f;
                p.HeadLean = -40f;
                p.FootRTip = 60f;
                p.FootLTip = 30f;
            });
            var torn = With(Knifer(V(0f, -0.36f, 0.3f), 0f, 0f, 20f, V(-0.3f, 0f, 0.4f), V(0.3f, 0f, -0.2f),
                V(-0.9f, 1.1f, 0.4f), V(-1f, 0f, 0.1f), V(0.9f, 1.1f, 0.4f), V(1f, 0f, 0.1f)), p => p.HeadLean = -10f);
            var clip = Attack(step, -12f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 1.4f);
            return clip.Key(0f, KnifeReady()).Key(hit * 0.5f, coiled, Ease.Out).Key(hit * 0.8f, With(coiled, p => p.Hips += V(0f, -0.04f, -0.03f)), Ease.Linear)
                .Key(hit, dart, Ease.In).Key(done, dart, Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.3f), torn, Ease.Snap)
                .Key(Mathf.Lerp(done, 1f, 0.7f), torn, Ease.Linear).Key(1f, KnifeReady(), Ease.InOut);
        }

        // The crossing step's dash: low, the lead shoulder dropped, the left knife dragged out to the side and the right folded back.
        private static ForgePose KnifeDashing() => With(Knifer(V(0f, -0.5f, 0.3f), 20f, 10f, 46f, V(-0.14f, 0f, 0.6f), V(0.2f, 0.2f, -0.6f),
            V(-0.9f, 0.92f, 0.3f), V(-1f, 0f, -0.1f), V(0.6f, 0.9f, -0.5f), V(0.5f, 0.1f, -0.85f)), p =>
        {
            p.ElbowL = V(-1f, 0.2f, 0f);
            p.HeadLean = -30f;
            p.FootRTip = 60f;
            p.ShoulderLForward = 20f;
        });

        /// <summary>The crossing step: a drop of the lead shoulder, a low dash past with the left knife dragged across, and a half turn on arrival with the right knife up.</summary>
        private static ForgeClip KnifeCrossingStep(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var dropped = With(Knifer(V(0f, -0.4f, 0f), 30f, 14f, 30f, KnifeFootL, KnifeFootR,
                V(-0.5f, 0.96f, 0.5f), V(-0.9f, 0f, 0.4f), V(0.5f, 0.96f, -0.2f), V(0.5f, 0.1f, -0.85f)), p => p.ShoulderLForward = 20f);
            var arrived = With(Knifer(V(0f, -0.34f, 0f), 30f, 10f, 14f, V(-0.16f, 0f, 0.34f), V(0.26f, 0f, -0.4f),
                V(-0.7f, 0.9f, -0.1f), V(-0.8f, -0.2f, -0.5f), V(0.56f, 1.66f, 0.3f), V(0.4f, 0.6f, 0.7f)), p =>
            {
                p.ShoulderRUp = 22f;
                p.FootRTip = 40f;
            });
            var clip = Attack(step, -12f);
            clip.SheetMoments = new[] { 0f, hit * 0.7f, hit, Mathf.Lerp(hit, done, 0.4f), Mathf.Lerp(hit, done, 0.8f), done, Mathf.Lerp(done, 1f, 0.35f), 1f };
            clip.Key(0f, KnifeReady()).Key(hit * 0.8f, dropped, Ease.Out).Key(hit, KnifeDashing(), Ease.In).Key(Mathf.Lerp(hit, done, 0.6f), KnifeDashing(), Ease.Linear)
                .Key(Mathf.Lerp(done, 1f, 0.25f), arrived, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.7f), arrived, Ease.Linear).Key(1f, KnifeReady(), Ease.InOut);
            clip.MovesRoot = true;
            // The half turn begins as the body clears the target; the game finishes it.
            clip.After = TurnAbout(t => t >= done ? 0f : Mathf.Lerp(0f, 180f, Mathf.InverseLerp(Mathf.Lerp(hit, done, 0.5f), done, t)), Vector3.up, Vector3.zero);
            return clip;
        }

        /// <summary>Out of the sidestep: the turn is simply allowed to finish, with the right arm flung out; one flat cut.</summary>
        private static ForgeClip KnifeDashCut(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var flung = With(KnifeWhirl(-0.34f), p =>
            {
                p.HandR = V(0.92f, 1.24f, 0.2f);
                p.Blade = V(1f, 0f, 0.2f);
                p.HandL = V(-0.4f, 1.1f, -0.2f);
                p.BladeL = V(-0.7f, 0f, -0.7f);
            });
            var held = With(Knifer(V(0f, -0.32f, 0.1f), 40f, 30f, 14f, V(-0.14f, 0f, 0.36f), V(0.24f, 0f, -0.3f),
                V(-0.5f, 1.1f, -0.1f), V(-0.8f, 0f, -0.5f), V(-0.5f, 1.3f, 0.7f), V(-0.85f, 0f, 0.5f)), p =>
            {
                p.ElbowR = V(0.4f, -0.3f, 1f);
                p.ShoulderRForward = 30f;
                p.FootRTip = 30f;
            });
            var clip = Attack(step, -6f);
            clip.Key(0f, flung).Key(hit, flung, Ease.Linear).Key(done, held, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), held, Ease.Linear).Key(1f, KnifeReady(), Ease.InOut);
            clip.MovesRoot = true;
            // The right arm leads a turn to the left (the cut runs right to left), finishing as the swing does.
            clip.After = TurnAbout(t => Mathf.Lerp(200f, 0f, Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0f, done, t))), Vector3.up, Vector3.zero);
            return clip;
        }

        /// <summary>
        /// Fan of Knives: both arms cross over the chest as the legs load; a back handspring, and at the top of it both
        /// arms whip open and the fan leaves the hands; a low three-point crouch, one hand on the ground.
        /// </summary>
        private static ForgeClip KnifeFan(AttackStep step)
        {
            var (hit, done) = Moments(step);
            float landed = Mathf.Lerp(done, 1f, 0.42f);
            var loaded = With(Knifer(V(0f, -0.44f, 0f), 0f, 0f, 18f, V(-0.16f, 0f, 0.1f), V(0.16f, 0f, -0.06f),
                V(0.2f, 1.26f, 0.4f), V(0.72f, 0.2f, 0.66f), V(-0.2f, 1.18f, 0.46f), V(-0.72f, 0.2f, 0.66f)), p =>
            {
                p.ElbowL = V(-0.8f, -0.6f, 0.4f);
                p.ElbowR = V(0.8f, -0.6f, 0.4f);
            });
            var whipped = With(Knifer(V(0f, 0.1f, 0f), 0f, 0f, -16f, V(-0.2f, 0.3f, 0.2f), V(0.2f, 0.4f, -0.1f),
                V(-0.9f, 1.5f, 0.36f), V(-0.8f, 0.2f, 0.6f), V(0.9f, 1.5f, 0.36f), V(0.8f, 0.2f, 0.6f)), p =>
            {
                p.ShoulderLUp = p.ShoulderRUp = 20f;
                p.FootLTip = p.FootRTip = 40f;
                p.HeadLean = -20f;
            });
            var tucked = With(Knifer(V(0f, 0.04f, 0f), 0f, 0f, 30f, V(-0.16f, 0.5f, 0.2f), V(0.16f, 0.5f, 0.1f),
                V(-0.84f, 1.2f, 0.1f), V(-1f, 0f, 0.2f), V(0.84f, 1.2f, 0.1f), V(1f, 0f, 0.2f)), p => p.HeadLean = 10f);
            var crouch = With(Knifer(V(0f, -0.6f, -0.04f), 20f, 6f, 30f, V(-0.2f, 0f, 0.36f), V(0.26f, 0f, -0.4f),
                V(-0.3f, 0.08f, 0.5f), V(-0.4f, 0f, 0.9f), V(0.84f, 1.0f, -0.3f), V(0.9f, 0.2f, -0.4f)), p =>
            {
                p.ElbowL = V(-1f, 0.3f, 0f);
                p.FootRTip = 60f;
                p.HeadLean = -30f;
            });
            var clip = Attack(step, 10f);
            clip.Shape = null;
            clip.SheetMoments = new[] { 0f, hit * 0.6f, hit, done, Mathf.Lerp(done, landed, 0.5f), landed, Mathf.Lerp(landed, 1f, 0.5f), 1f };
            clip.Key(0f, KnifeReady()).Key(hit * 0.55f, loaded, Ease.Out).Key(hit, whipped, Ease.Snap).Key(done, whipped, Ease.Linear)
                .Key(Mathf.Lerp(done, landed, 0.6f), tucked, Ease.InOut).Key(landed, crouch, Ease.In).Key(Mathf.Lerp(landed, 1f, 0.6f), crouch, Ease.Linear).Key(1f, KnifeReady(), Ease.InOut);
            clip.MovesRoot = true;
            // Heels over head backwards, about the middle of the body, lifted while it is upside down.
            clip.After = (rig, t) =>
            {
                float through = Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(hit * 0.5f, landed, t));
                var root = rig["root"];
                var pivot = new Vector3(0f, 0.95f, 0f);
                var spin = Quaternion.AngleAxis(-360f * through, Vector3.right);
                root.Transform.rotation = spin * root.RestRotation;
                root.Transform.position = pivot - spin * pivot + spin * root.RestPosition + Vector3.up * (0.5f * Mathf.Sin(through * Mathf.PI));
            };
            return clip;
        }

        /// <summary>
        /// Three Crossings. Three passes through the kneeling enemy (the game carries the body; the clip is the dash and
        /// the plant between), then, back turned, the slow rise out of the crouch and one turn of both knives on their
        /// rings; the knives stop dead, points down and out, and the lines burst behind.
        /// </summary>
        private static ForgeClip KnifeThreeCrossings(AttackStep step)
        {
            var (hit, done) = Moments(step);
            float total = step.Total;
            // Planted at a corner between two passes: low, turned back in, knives wide.
            var planted = With(KnifeSkid(), p =>
            {
                p.HandL = V(-0.8f, 0.9f, 0.1f);
                p.BladeL = V(-1f, 0f, 0.2f);
                p.HandR = V(0.8f, 0.9f, 0.1f);
                p.Blade = V(1f, 0f, 0.2f);
            });
            // Back to the enemy, low, the knives hanging wide.
            var arrived = With(Knifer(V(0f, -0.5f, 0f), 0f, 0f, 20f, V(-0.24f, 0f, 0.1f), V(0.24f, 0f, -0.1f),
                V(-0.7f, 0.8f, 0.1f), V(-0.9f, -0.3f, 0.2f), V(0.7f, 0.8f, 0.1f), V(0.9f, -0.3f, 0.2f), 0f), p => p.HeadLean = 20f);
            // Standing, arms out level: both wrists roll the knives over and back, the blades flashing as they turn.
            // (A held knife cannot go round a ring on a finger: the hand's mesh has no fingers.)
            ForgePose Spun(float turn, float rise) => With(Knifer(V(0f, Mathf.Lerp(-0.5f, -0.04f, rise), 0f), 0f, 0f, Mathf.Lerp(20f, 0f, rise), V(-0.22f, 0f, 0.06f), V(0.22f, 0f, -0.06f),
                V(-0.74f, Mathf.Lerp(0.8f, 1.2f, rise), 0.14f), V(-0.7f, 0f, 0.7f),
                V(0.74f, Mathf.Lerp(0.8f, 1.2f, rise), 0.14f), V(0.7f, 0f, 0.7f), 0f), p =>
            {
                p.HeadLean = Mathf.Lerp(20f, 4f, rise);
                p.WristRollL = 160f * Mathf.Sin(turn * Mathf.Deg2Rad * 0.5f);
                p.WristRollR = -p.WristRollL;
            });
            // The knives stop dead: points down and a little out, arms straight, head bowed.
            var stopped = With(Knifer(V(0f, -0.02f, 0f), 0f, 0f, 0f, V(-0.22f, 0f, 0.06f), V(0.22f, 0f, -0.06f),
                V(-0.72f, 1.08f, 0.12f), V(-0.72f, -0.68f, 0.14f), V(0.72f, 1.08f, 0.12f), V(0.72f, -0.68f, 0.14f), 0f), p =>
            {
                p.HeadLean = 16f;
                p.ElbowL = V(-1f, 0f, -0.3f);
                p.ElbowR = V(1f, 0f, -0.3f);
            });
            var clip = Attack(step, -10f);
            clip.Shape = null;
            float hold = step.HoldAt / total;
            clip.SheetMoments = new[] { 0.12f / total, 0.24f / total, 0.33f / total, 0.5f / total, Mathf.Lerp(hold, hit, 0.5f), hit * 0.97f, hit, Mathf.Lerp(done, 1f, 0.3f) };
            clip.Key(0f, KnifeReady());
            foreach (var beat in step.Beats)
            {
                clip.Key((beat.Time + 0.02f) / total, KnifeDashing(), Ease.Snap);
                clip.Key((beat.Time + beat.DashSeconds) / total, KnifeDashing(), Ease.Linear);
                clip.Key((beat.Time + beat.DashSeconds + 0.05f) / total, planted, Ease.Out);
            }
            float from = (step.Beats[step.Beats.Length - 1].Time + step.Beats[step.Beats.Length - 1].DashSeconds + 0.08f) / total;
            clip.Key(from, arrived, Ease.Out);
            for (int quarter = 1; quarter <= 4; quarter++)
                clip.Key(Mathf.Lerp(from, hit * 0.94f, quarter / 4f), Spun(90f * quarter, quarter / 4f), Ease.Linear);
            return clip.Key(hit, stopped, Ease.Snap).Key(Mathf.Lerp(done, 1f, 0.6f), stopped, Ease.Linear).Key(1f, KnifeReady(), Ease.InOut);
        }
    }
}

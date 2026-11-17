using System;
using System.Collections.Generic;
using Clover.Player;
using UnityEngine;

namespace Clover.EditorTools.Forge
{
    /// <summary>
    /// The Chain Sickle's clips. The right hand has the sickle; the left has the chain, whose line and weight are
    /// drawn by the game (<see cref="ChainLine"/>) from wherever these clips put the left hand, so the left hand's
    /// job here is to be where a hand that throws, whirls or hauls a chain would be. A sickle held point-up is a
    /// speck from the game's camera: every pose lays the haft out from the fist with the crescent turned level, so
    /// it reads as a hook on the end of the arm.
    /// </summary>
    public static partial class PlayerAnimations
    {
        private static IEnumerable<ForgeClip> SickleClips()
        {
            var combo = Weapons.ChainSickle().Combo;
            yield return SickleIdle();
            yield return SickleRun();
            yield return SickleGuard();
            yield return SickleGuardHit();
            yield return SickleGuardPerfect();
            yield return SickleCharge(combo.ChargeTime);
            yield return SickleChop(combo.Steps[0]);
            yield return SickleWeightSnap(combo.Steps[1]);
            yield return SickleHook(combo.Steps[2]);
            yield return SickleWhirl(combo.Steps[3]);
            yield return SickleThrown(combo.Heavy);
            yield return SickleYank(combo.ChainHeavy);
            yield return SickleEntangle(combo.Skill);
            yield return SickleUnwinding(combo.Finisher);
        }

        // A pose with the sickle in the right hand and the chain in the left: where each hand is, where the haft
        // points, and which way the crescent hooks (the blade leaves the haft's top that way).
        private static ForgePose Reaper(Vector3 hips, float hipYaw, float waistYaw, float lean, Vector3 footL, Vector3 footR,
            Vector3 chainHand, Vector3 sickleHand, Vector3 haft, Vector3 hook, float keep = 0.85f)
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
                FootLTurn = 40f,
                FootRTurn = 6f,
                HandL = chainHand,
                HandR = sickleHand,
                ElbowL = new Vector3(-1f, -0.3f, -0.5f),
                ElbowR = new Vector3(1f, -0.3f, -0.4f),
                AimBlade = true,
                Blade = haft,
                BladeBack = -hook,
            }, keep);
        }

        private static readonly Vector3 SickleFootL = new Vector3(-0.2f, 0f, -0.26f), SickleFootR = new Vector3(0.12f, 0f, 0.3f);

        /// <summary>Side on, right foot forward: the sickle low and ahead with its hook turned in, the chain hand back by the left hip.</summary>
        private static ForgePose SickleReady() => Reaper(V(0f, -0.2f, 0f), -36f, -8f, 8f, SickleFootL, SickleFootR,
            V(-0.44f, 0.94f, -0.14f), V(0.42f, 1.04f, 0.52f), V(0.3f, 0.24f, 0.92f), V(-1f, 0.1f, 0.2f));

        private static ForgeClip SickleIdle() => new ForgeClip("idle_sickle", 1.6f, true).From(t =>
        {
            // The left wrist keeps the weight turning: a small circle of the hand, once a loop, and the body rides it.
            float a = t * Mathf.PI * 2f;
            var p = SickleReady();
            p.HandL += V(0f, 0.04f * Mathf.Cos(a), 0.05f * Mathf.Sin(a));
            p.Hips += V(0f, 0.012f * Mathf.Sin(a), 0f);
            p.HandR += V(0f, 0.012f * Mathf.Sin(a + 1f), 0f);
            return p;
        });

        /// <summary>The sickle carried reversed along the right forearm; the left hand holds the gathered chain at the chest.</summary>
        private static ForgeClip SickleRun()
        {
            var clip = Renamed(RunFist(), "run_sickle");
            clip.Seconds = 0.54f;
            Func<float, ForgePose, ForgePose> carried = (t, p) =>
            {
                float sway = Mathf.Sin(t * Mathf.PI * 2f);
                p.WaistLean += 8f;
                p.HandR = V(0.46f, 1.0f + 0.04f * sway, 0.1f - 0.16f * sway);
                p.AimBlade = true;
                p.Blade = V(0.3f, 0.1f, -0.95f);
                p.BladeBack = V(-1f, 0f, 0f);
                p.HandL = V(-0.22f, 1.26f, 0.3f);
                p.ElbowL = V(-1f, -0.6f, 0f);
                p.TwoHanded = false;
                return p;
            };
            clip.Shape = clip.Shape != null ? Both(clip.Shape, carried) : carried;
            return clip;
        }

        // Both hands thrown up and apart, the chain taut between them slantwise across the face and chest.
        private static ForgePose SickleBarred() => With(Reaper(V(0f, -0.26f, -0.04f), -10f, 0f, 6f, V(-0.22f, 0f, -0.16f), V(0.2f, 0f, 0.22f),
            V(-0.5f, 1.1f, 0.5f), V(0.5f, 1.66f, 0.46f), V(-0.5f, 0.7f, 0.5f), V(-0.6f, -0.6f, 0.5f), 0.5f), p =>
        {
            p.ElbowL = V(-1f, -0.4f, 0.2f);
            p.ElbowR = V(1f, 0.2f, 0f);
            p.ShoulderRUp = 20f;
        });

        private static ForgeClip SickleGuard() => new ForgeClip("guard_sickle", 0.12f).Key(0f, SickleReady()).Key(1f, SickleBarred(), Ease.Out);

        private static ForgeClip SickleGuardHit()
        {
            // The taut line bows in toward the chest and the hands are driven together.
            var driven = With(SickleBarred(), p =>
            {
                p.Hips = V(0f, -0.3f, -0.2f);
                p.HandL = V(-0.38f, 1.16f, 0.32f);
                p.HandR = V(0.38f, 1.56f, 0.3f);
                p.FootL += V(0f, 0f, -0.14f);
                p.FootR += V(0f, 0f, -0.14f);
                p.WaistLean = -6f;
            });
            return new ForgeClip("guard_hit_sickle", 0.22f).Key(0f, SickleBarred()).Key(0.25f, driven, Ease.Snap).Key(1f, SickleBarred(), Ease.InOut);
        }

        /// <summary>The left hand throws a loop over the blow and both hands haul back and down past the left hip, the body turning side on with the pull.</summary>
        private static ForgeClip SickleGuardPerfect()
        {
            var looped = With(Reaper(V(0f, -0.2f, 0.1f), -20f, -10f, 12f, V(-0.22f, 0f, -0.1f), V(0.2f, 0f, 0.3f),
                V(-0.2f, 1.6f, 0.9f), V(0.5f, 1.3f, 0.5f), V(-0.2f, 0.6f, 0.8f), V(-0.8f, -0.3f, 0.5f)), p => p.ElbowL = V(-1f, 0.3f, 0f));
            var hauled = With(Reaper(V(-0.06f, -0.4f, -0.16f), 46f, 24f, 4f, V(-0.26f, 0f, -0.3f), V(0.2f, 0f, 0.3f),
                V(-0.56f, 0.8f, -0.3f), V(-0.1f, 0.96f, 0.1f), V(0.6f, 0.3f, 0.74f), V(-0.4f, -0.2f, 0.9f)), p =>
            {
                p.ElbowR = V(0.6f, -0.4f, 1f);
                p.FootRTip = 20f;
            });
            return new ForgeClip("guard_perfect_sickle", 0.3f).Key(0f, SickleBarred()).Key(0.22f, looped, Ease.Snap).Key(0.6f, hauled, Ease.In).Key(1f, hauled, Ease.Linear);
        }

        // The chain hand straight up, the weight circling above it; the sickle held back and low. Upright, so the ring sits level over the head.
        private static ForgePose SickleOverhead(float sink = 0f) => With(Reaper(V(0f, -0.1f - sink, 0f), -20f, 0f, 0f, V(-0.2f, 0f, -0.14f), V(0.16f, 0f, 0.2f),
            V(-0.1f, 2.06f, 0.06f), V(0.56f, 0.96f, -0.2f), V(0.5f, 0.1f, -0.86f), V(-0.9f, 0f, -0.4f), 0.4f), p =>
        {
            p.ShoulderLUp = 36f;
            p.ElbowL = V(-1f, 0f, 0f);
            p.ElbowR = V(1f, 0f, -1f);
        });

        private static ForgeClip SickleCharge(float seconds) => new ForgeClip("charge_sickle", seconds).Key(0f, SickleReady()).Key(0.6f, SickleOverhead(), Ease.Out).From(t =>
        {
            // The wrist goes round under the ring, faster as the hold fills.
            var p = ForgePose.Blend(SickleReady(), SickleOverhead(), ForgePose.Apply(Ease.Out, Mathf.Clamp01(t / 0.6f)));
            float a = t * t * Mathf.PI * 6f;
            p.HandL += V(0.07f * Mathf.Cos(a), 0f, 0.07f * Mathf.Sin(a)) * Mathf.Clamp01(t / 0.6f);
            return p;
        });

        /// <summary>The sickle cut: drawn back over the right shoulder, a short hooking chop right to left and down with the right hip driving in, finishing by the left hip.</summary>
        private static ForgeClip SickleChop(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var raised = With(Reaper(V(0f, -0.16f, -0.04f), -50f, -24f, -4f, SickleFootL, SickleFootR,
                V(-0.44f, 0.96f, -0.1f), V(0.56f, 1.8f, -0.06f), V(0.5f, 0.5f, -0.7f), V(0.6f, 0.5f, 0.6f)), p =>
            {
                p.ShoulderRUp = 26f;
                p.ElbowR = V(1f, 0.2f, -0.3f);
            });
            var chopped = With(Reaper(V(0f, -0.3f, 0.16f), 10f, 16f, 20f, SickleFootL, V(0.12f, 0f, 0.4f),
                V(-0.5f, 0.9f, -0.26f), V(0.12f, 1.32f, 1.04f), V(-0.4f, -0.1f, 0.9f), V(-0.8f, -0.4f, -0.4f)), p =>
            {
                p.ShoulderRForward = 30f;
                p.FootLTip = 30f;
            });
            var carried = With(chopped, p =>
            {
                p.HandR = V(-0.42f, 0.98f, 0.36f);
                p.Blade = V(-0.86f, -0.1f, 0.5f);
                p.BladeBack = V(0.3f, 0.3f, 0.9f);
                p.HipYaw = 26f;
                p.WaistYaw = 30f;
                Looking(p);
            });
            return Attack(step, -5f).Key(0f, SickleReady()).Key(hit * 0.6f, raised, Ease.Out).Key(hit, chopped, Ease.In).Key(done, carried, Ease.Out)
                .Key(1f, With(carried, p => p.Hips += V(0f, 0.02f, -0.02f)), Ease.InOut);
        }

        // The left arm whipped forward underhand, at full stretch: what sends the weight down the lane.
        private static ForgePose SickleCast(float lean, float height) => With(Reaper(V(0f, -0.28f, 0.16f), 34f, 20f, lean, V(-0.14f, 0f, 0.36f), V(0.2f, 0f, -0.2f),
            V(-0.2f, height, 1.0f), V(0.44f, 1.0f, -0.16f), V(0.5f, 0.2f, 0.84f), V(-0.9f, 0f, 0.3f)), p =>
        {
            p.ElbowL = V(-1f, -0.3f, 0f);
            p.ShoulderLForward = 34f;
            p.FootRTip = 30f;
            p.FootLTurn = 8f;
            p.FootRTurn = 40f;
        });

        // The left arm fully back and the chest opened left: the weight let go on the upswing behind.
        private static ForgePose SickleLoaded() => With(Reaper(V(0f, -0.3f, -0.08f), -60f, -30f, 6f, SickleFootL, SickleFootR,
            V(-0.5f, 0.86f, -0.66f), V(-0.06f, 0.98f, 0.3f), V(-0.5f, 0.2f, 0.84f), V(-0.4f, -0.4f, -0.8f)), p =>
        {
            p.ElbowL = V(-1f, 0.3f, -0.4f);
            p.ElbowR = V(0.6f, -0.4f, 1f);
        });

        /// <summary>The weight snap: let go on the upswing behind, whipped forward underhand down the lane, and flicked back to the hand.</summary>
        private static ForgeClip SickleWeightSnap(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var snapped = With(SickleCast(14f, 1.2f), p =>
            {
                p.HandL = V(-0.4f, 1.34f, 0.3f);
                p.ElbowL = V(-1f, -0.6f, -0.3f);
                p.WaistLean = 0f;
                p.Hips = V(0f, -0.2f, 0.04f);
            });
            var clip = Attack(step, -3f);
            return clip.Key(0f, SickleReady()).Key(hit * 0.55f, SickleLoaded(), Ease.Out).Key(hit, SickleCast(18f, 1.2f), Ease.In).Key(done, SickleCast(20f, 1.16f), Ease.Linear)
                .Key(Mathf.Lerp(done, 1f, 0.35f), snapped, Ease.Snap).Key(1f, snapped, Ease.Linear);
        }

        /// <summary>The hook: the sickle goes out wide to the left with its blade turned inward, is hooked behind the target and drawn back toward the bearer's own right hip. A pull, not a push.</summary>
        private static ForgeClip SickleHook(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var wide = With(Reaper(V(0f, -0.26f, 0.1f), 30f, 24f, 14f, V(-0.2f, 0f, -0.1f), V(0.12f, 0f, 0.4f),
                V(-0.44f, 1.0f, -0.2f), V(-0.56f, 1.24f, 0.8f), V(-0.3f, 0.1f, 0.95f), V(1f, 0f, 0f)), p =>
            {
                p.ElbowR = V(0.4f, -0.3f, 1f);
                p.ShoulderRForward = 30f;
            });
            var hooked = Reaper(V(0f, -0.26f, 0.06f), -20f, -16f, 4f, V(-0.2f, 0f, -0.1f), V(0.12f, 0f, 0.4f),
                V(-0.46f, 0.96f, -0.2f), V(0.4f, 1.16f, 0.74f), V(0.3f, 0.06f, 0.95f), V(-1f, 0f, -0.3f));
            var drawn = With(Reaper(V(0f, -0.22f, -0.12f), -50f, -30f, -12f, V(-0.2f, 0f, -0.2f), V(0.12f, 0f, 0.36f),
                V(-0.4f, 1.0f, 0.1f), V(0.56f, 1.02f, -0.1f), V(0.7f, 0.1f, 0.7f), V(-0.6f, 0f, -0.8f)), p => p.ElbowR = V(0.6f, 0f, -1f));
            return Attack(step, 3f).Key(0f, wide).Key(hit * 0.5f, wide, Ease.Linear).Key(hit, hooked, Ease.In).Key(done, drawn, Ease.Out)
                .Key(1f, With(drawn, p => p.Hips += V(0f, 0.02f, 0f)), Ease.InOut);
        }

        /// <summary>The whirl, two blows: the chain is let run and swung up overhead with the knees bent; two flat turns at full length, the body turning under it with the sickle held in; the weight is brought down round the waist.</summary>
        private static ForgeClip SickleWhirl(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var turning = With(SickleOverhead(0.2f), p =>
            {
                p.HandR = V(0.24f, 1.2f, 0.3f);
                p.Blade = V(-0.5f, 0.4f, 0.76f);
                p.BladeBack = V(0.6f, 0.6f, 0.5f);
                p.ElbowR = V(1f, -0.5f, 0f);
                p.FootL = V(-0.22f, 0f, -0.06f);
                p.FootR = V(0.22f, 0f, 0.06f);
            });
            var wrapped = With(Reaper(V(0f, -0.24f, 0f), -30f, -40f, 6f, SickleFootL, SickleFootR,
                V(0.2f, 1.0f, 0.3f), V(0.44f, 1.08f, 0.4f), V(0.3f, 0.24f, 0.92f), V(-1f, 0.1f, 0.2f)), p => p.ElbowL = V(-0.6f, -0.4f, 1f));
            var clip = Attack(step, 0f);
            clip.Shape = null;
            float stop = Mathf.Lerp(done, 1f, 0.45f);
            clip.SheetMoments = new[] { 0f, hit * 0.6f, hit, Mathf.Lerp(hit, done, 0.5f), done, Mathf.Lerp(done, stop, 0.5f), stop, 1f };
            clip.Key(0f, SickleReady()).Key(hit * 0.6f, SickleOverhead(0.24f), Ease.Out).Key(hit, turning, Ease.InOut).Key(stop, turning, Ease.Linear)
                .Key(Mathf.Lerp(stop, 1f, 0.6f), wrapped, Ease.Out).Key(1f, SickleReady(), Ease.InOut);
            clip.MovesRoot = true;
            // Two full turns under the chain, the same way it goes, home again by the time the weight comes down.
            clip.After = TurnAbout(t => -720f * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(hit * 0.6f, stop, t)), Vector3.up, Vector3.zero);
            return clip;
        }

        /// <summary>The thrown sickle: spun once low at the right side, cast out right-handed and swept flat across the front on the chain, hauled home by the left hand and caught overhand.</summary>
        private static ForgeClip SickleThrown(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var low = With(Reaper(V(0f, -0.34f, -0.06f), -56f, -30f, 10f, SickleFootL, SickleFootR,
                V(-0.3f, 1.1f, 0.3f), V(0.66f, 0.76f, -0.36f), V(0.4f, -0.5f, -0.76f), V(0.8f, 0.5f, 0f)), p => p.ElbowR = V(1f, 0f, -0.6f));
            var cast = With(Reaper(V(0f, -0.3f, 0.12f), -10f, -6f, 16f, V(-0.22f, 0f, -0.2f), V(0.14f, 0f, 0.4f),
                V(-0.36f, 1.1f, 0.2f), V(0.86f, 1.3f, 0.5f), V(0.8f, 0f, 0.6f), V(-0.5f, 0f, 0.86f)), p =>
            {
                p.ElbowR = V(1f, 0.2f, 0f);
                p.ShoulderRForward = 20f;
            });
            var swept = With(Reaper(V(0f, -0.3f, 0.14f), 44f, 34f, 18f, V(-0.22f, 0f, -0.2f), V(0.14f, 0f, 0.4f),
                V(-0.5f, 1.0f, -0.1f), V(-0.4f, 1.3f, 0.8f), V(-0.8f, 0f, 0.6f), V(-0.5f, 0f, -0.86f)), p =>
            {
                p.ShoulderRForward = 34f;
                p.ElbowR = V(0.3f, -0.4f, 1f);
                p.FootLTip = 30f;
            });
            var hauled = With(Reaper(V(0f, -0.22f, -0.08f), -20f, -10f, -8f, SickleFootL, SickleFootR,
                V(-0.54f, 0.9f, -0.4f), V(0.4f, 1.8f, 0.36f), V(0.2f, 0.7f, 0.68f), V(-0.4f, -0.5f, 0.76f)), p =>
            {
                p.ShoulderRUp = 26f;
                p.ElbowL = V(-1f, 0.2f, -0.6f);
            });
            var clip = Attack(step, -4f);
            return clip.Key(0f, SickleReady()).Key(hit * 0.5f, low, Ease.Out).Key(hit * 0.8f, cast, Ease.In).Key(done, swept, Ease.Linear)
                .Key(Mathf.Lerp(done, 1f, 0.6f), hauled, Ease.InOut).Key(1f, SickleReady(), Ease.InOut);
        }

        /// <summary>The yank: a short underhand toss, then both hands on the chain and the whole body thrown back onto the heels; it ends with the sickle raised, for what will not come and must be flown to.</summary>
        private static ForgeClip SickleYank(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var heaved = With(Reaper(V(0f, -0.36f, -0.3f), 20f, 10f, -30f, V(-0.2f, 0f, 0.2f), V(0.2f, 0f, -0.36f),
                V(-0.2f, 1.06f, 0.36f), V(0.14f, 1.16f, 0.46f), V(0.3f, 0.5f, 0.8f), V(-0.9f, 0f, 0.4f)), p =>
            {
                p.FootLTip = -24f;
                p.FootRTip = -16f;
                p.FootLTurn = 8f;
                p.FootRTurn = 30f;
                p.ElbowL = V(-1f, -0.6f, 0f);
                p.ElbowR = V(1f, -0.6f, 0f);
            });
            var raised = With(Reaper(V(0f, -0.2f, 0.16f), -20f, -10f, 14f, V(-0.2f, 0f, -0.2f), V(0.14f, 0f, 0.36f),
                V(-0.3f, 1.1f, 0.5f), V(0.5f, 1.9f, 0.1f), V(0.2f, 0.8f, -0.56f), V(0f, 0.5f, 0.86f)), p => p.ShoulderRUp = 30f);
            var chopped = With(raised, p =>
            {
                p.HandR = V(0.2f, 1.1f, 0.84f);
                p.Blade = V(-0.2f, -0.3f, 0.93f);
                p.BladeBack = V(0f, 0.9f, 0.3f);
                p.Hips = V(0f, -0.34f, 0.2f);
                p.WaistLean = 20f;
            });
            var clip = Attack(step, 4f);
            clip.Shape = null;
            return clip.Key(0f, SickleLoaded()).Key(hit * 0.4f, SickleLoaded(), Ease.Linear).Key(hit, SickleCast(16f, 1.16f), Ease.In).Key(done, SickleCast(16f, 1.16f), Ease.Linear)
                .Key(Mathf.Lerp(done, 1f, 0.3f), heaved, Ease.Snap).Key(Mathf.Lerp(done, 1f, 0.5f), heaved, Ease.Linear)
                .Key(Mathf.Lerp(done, 1f, 0.72f), raised, Ease.Out).Key(1f, chopped, Ease.In);
        }

        /// <summary>Entangle: the turns overhead carried over from the hold, released overhand; then a turn of chain taken round the wrist and the stance dropped low and wide, holding the line.</summary>
        private static ForgeClip SickleEntangle(AttackStep step)
        {
            var (hit, done) = Moments(step);
            // Second pass: let go higher and thrown further over the front foot: an overhand cast, not the light chain's underhand one.
            var released = With(SickleCast(30f, 1.64f), p =>
            {
                p.ShoulderLUp = 26f;
                p.Hips += V(0f, 0f, 0.1f);
            });
            var set = With(Reaper(V(0f, -0.46f, -0.1f), 30f, 10f, 4f, V(-0.2f, 0f, 0.4f), V(0.26f, 0f, -0.4f),
                V(-0.36f, 1.06f, 0.5f), V(0.56f, 0.96f, 0.0f), V(0.6f, 0.2f, 0.76f), V(-0.8f, 0f, 0.6f)), p =>
            {
                p.FootLTurn = 10f;
                p.FootRTurn = 50f;
                p.ElbowL = V(-1f, -0.5f, 0f);
            });
            var clip = Attack(step, -3f);
            return clip.Key(0f, SickleOverhead()).Key(hit * 0.6f, SickleOverhead(0.1f), Ease.Linear).Key(hit, released, Ease.In).Key(done, released, Ease.Linear)
                .Key(Mathf.Lerp(done, 1f, 0.4f), set, Ease.Out).Key(1f, set, Ease.Linear);
        }

        /// <summary>
        /// "Unwinding". The weight is thrown and wraps; the bearer turns the back (the game turns the body and carries
        /// it away: from then on "ahead" in this clip is away from the enemy), puts the chain over the shoulder and
        /// walks it, three heavy hauling steps; stops, back still turned, the sickle raised level at the right
        /// shoulder with its blade pointing behind; and without looking makes one backward hooking cut.
        /// </summary>
        private static ForgeClip SickleUnwinding(AttackStep step)
        {
            var (hit, done) = Moments(step);
            float total = step.Total;
            float wrap = step.Beats[0].Time / total, hold = step.HoldAt / total;
            // Hauling: bent far forward against the chain, both fists at the right shoulder, one long stride after another.
            ForgePose Hauling(bool leftLeads) => With(Reaper(V(0f, -0.34f, 0.1f), leftLeads ? 14f : -14f, 0f, 40f,
                leftLeads ? V(-0.16f, 0f, 0.46f) : V(-0.16f, 0f, -0.44f), leftLeads ? V(0.16f, 0f, -0.44f) : V(0.16f, 0f, 0.46f),
                V(0.2f, 1.36f, 0.34f), V(0.4f, 1.2f, 0.4f), V(0.5f, -0.2f, 0.84f), V(-0.7f, -0.6f, 0.3f), 0f), p =>
            {
                p.ElbowL = V(-0.6f, -0.6f, 1f);
                p.ElbowR = V(1f, -0.4f, 0.3f);
                p.FootLTurn = p.FootRTurn = 6f;
                p.FootLTip = leftLeads ? 0f : 50f;
                p.FootRTip = leftLeads ? 50f : 0f;
                p.HeadLean = -20f;
            });
            // Stopped: upright, back turned, the sickle level at the right shoulder pointing behind, its hook out to the side.
            var poised = With(Reaper(V(0f, -0.12f, 0f), 0f, 0f, 0f, V(-0.2f, 0f, 0.1f), V(0.2f, 0f, -0.1f),
                V(-0.36f, 0.94f, 0.16f), V(0.66f, 1.62f, 0.02f), V(0.3f, 0.08f, -0.95f), V(1f, 0f, 0f), 0f), p =>
            {
                p.ElbowR = V(1f, 0.3f, 0.6f);
                p.ShoulderRUp = 20f;
                p.HeadLean = 14f;
                p.FootLTurn = p.FootRTurn = 14f;
            });
            // The blind cut: the arm thrown straight back past the hip and the hook dragged across behind.
            var cut = With(Reaper(V(0f, -0.26f, -0.1f), 0f, 44f, -6f, V(-0.2f, 0f, 0.14f), V(0.22f, 0f, -0.2f),
                V(-0.4f, 1.0f, 0.3f), V(0.14f, 1.26f, -0.98f), V(-0.7f, -0.1f, -0.7f), V(-0.7f, 0f, 0.7f), 0f), p =>
            {
                p.ElbowR = V(1f, 0.4f, -0.3f);
                p.HeadLean = 18f;
                p.HeadYaw = 0f;
                p.FootLTurn = p.FootRTurn = 14f;
            });
            var through = With(cut, p =>
            {
                p.HandR = V(-0.4f, 1.0f, -0.5f);
                p.Blade = V(-0.9f, -0.2f, 0.2f);
                p.WaistYaw = -30f;
                p.HeadYaw = 0f;
            });
            var clip = Attack(step, -6f);
            clip.Shape = null;
            float walkFrom = wrap + 0.03f / total, walkTo = (step.Beats[0].Time + step.Beats[0].DashSeconds) / total;
            clip.SheetMoments = new[] { wrap * 0.9f, Mathf.Lerp(walkFrom, walkTo, 0.3f), Mathf.Lerp(walkFrom, walkTo, 0.75f), hold, hit * 0.97f, hit, done, Mathf.Lerp(done, 1f, 0.5f) };
            return clip.Key(0f, SickleLoaded()).Key(wrap * 0.9f, SickleCast(22f, 1.4f), Ease.In)
                .Key(walkFrom, Hauling(true), Ease.Snap).Key(Mathf.Lerp(walkFrom, walkTo, 0.4f), Hauling(false), Ease.InOut).Key(Mathf.Lerp(walkFrom, walkTo, 0.8f), Hauling(true), Ease.InOut)
                .Key(walkTo, Hauling(true), Ease.Linear).Key(hold, poised, Ease.Out).Key(hit * 0.96f, With(poised, p => p.HandR += V(0.02f, 0.04f, 0.04f)), Ease.Linear)
                .Key(hit, cut, Ease.Snap).Key(done, through, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.6f), through, Ease.Linear).Key(1f, poised, Ease.InOut);
        }
    }
}

using System.Collections.Generic;
using Clover.Player;
using UnityEngine;

namespace Clover.EditorTools.Forge
{
    /// <summary>
    /// The Great Spear's clips. Everything is the line: the body stands side-on behind it, left shoulder leading, the
    /// right hand at the hip driving the shaft through the left, and from above the weapon simply gets longer and
    /// shorter. The right hand holds the shaft a metre from the butt and the left 0.6 m ahead of it (a negative grip
    /// gap); the leaf lies flat, so a level spear shows its cross to the high camera.
    /// </summary>
    public static partial class PlayerAnimations
    {
        private const float SpearGrip = -0.55f;
        private static readonly Vector3 LeafUp = new Vector3(0f, 1f, 0f);

        private static IEnumerable<ForgeClip> SpearClips()
        {
            var combo = Weapons.GreatSpear().Combo;
            yield return SpearIdle();
            yield return SpearRun();
            yield return SpearGuard();
            yield return SpearGuardHit();
            yield return SpearGuardPerfect();
            yield return SpearCharge(combo.ChargeTime);
            yield return SpearThrust(combo.Steps[0]);
            yield return SpearHighThrust(combo.Steps[1]);
            yield return SpearSweep(combo.Steps[2]);
            yield return SpearGuardThrust(combo.GuardAttack);
            yield return SpearChop(combo.Heavy);
            yield return SpearVault(combo.ChainHeavy);
            yield return SpearCouchedCharge(combo.ChargedHeavy);
            yield return SpearThrow(combo.Skill);
            yield return SpearDash(combo.Skill.FollowSeconds);
            yield return SpearRecall(combo.Skill.FollowStep);
            yield return SpearAway();
            yield return SpearCatch();
            yield return SpearStandard(combo.Finisher);
        }

        private static ForgePose Spearman(Vector3 hips, float hipYaw, float waistYaw, float lean, Vector3 footL, Vector3 footR,
            Vector3 hand, Vector3 shaft, float gap = SpearGrip, float keep = 0.9f) =>
            With(Two(hips, hipYaw, waistYaw, lean, footL, footR, hand, shaft, LeafUp, gap, keep), p =>
            {
                p.ElbowR = V(0.7f, -0.3f, -0.8f);
                p.ElbowL = V(-0.7f, -0.6f, 0.1f);
            });

        /// <summary>At ease: side-on, the spear upright in the right hand with its butt on the ground, the left hand on the belt.</summary>
        private static ForgePose SpearUpright() => With(Two(V(0f, -0.03f, 0f), 24f, 6f, 2f, V(-0.13f, 0f, 0.14f), V(0.16f, 0f, -0.1f),
            V(0.4f, 1.04f, 0.12f), V(0f, 1f, 0.03f), V(0f, 0f, -1f)), p =>
        {
            p.TwoHanded = false;
            p.HandL = V(-0.2f, 1.0f, 0.14f);
            p.ElbowL = V(-0.9f, -0.2f, -0.4f);
            p.ElbowR = V(0.9f, -0.5f, -0.2f);
            p.FootRTurn = 40f;
        });

        /// <summary>The line: sunk a little, left shoulder leading, the spear levelled at the hip and pointing dead ahead.</summary>
        private static ForgePose SpearLine() => With(Spearman(V(0f, -0.16f, 0f), 42f, 20f, 6f, V(-0.12f, 0f, 0.3f), V(0.2f, 0f, -0.28f),
            V(0.3f, 1.0f, -0.12f), V(-0.14f, 0.04f, 1f)), p => p.FootRTurn = 50f);

        // Sunk low behind the levelled spear, the shaft slanting up from the hip so the point is at a throat.
        private static ForgePose SpearBehind() => With(Spearman(V(0f, -0.3f, -0.06f), 50f, 22f, 12f, V(-0.1f, 0f, 0.36f), V(0.22f, 0f, -0.34f),
            V(0.3f, 0.92f, -0.16f), V(-0.12f, 0.16f, 1f), -0.6f), p =>
        {
            p.FootRTurn = 56f;
            p.ShoulderLForward = 14f;
            p.HeadLean = 8f;
        });

        // Couched: locked under the right arm against the ribs, the left hand out along the shaft, a sprinter's lean with the back heel up.
        private static ForgePose SpearCouched() => With(Spearman(V(0f, -0.3f, 0.06f), 30f, 14f, 26f, V(-0.14f, 0f, 0.4f), V(0.2f, 0f, -0.5f),
            V(0.26f, 1.1f, -0.04f), V(-0.08f, -0.02f, 1f), -0.5f), p =>
        {
            p.FootRTip = 42f;
            p.ElbowR = V(0.5f, -0.1f, -1f);
            p.HeadLean = -18f;
        });

        private static ForgeClip SpearIdle() => new ForgeClip("idle_spear", 2.8f, true).From(t =>
        {
            // Still and patient: only the breath, and the head that looks along the lane and back.
            float breath = Mathf.Sin(t * Mathf.PI * 2f);
            var p = SpearUpright();
            p.Hips += V(0f, 0.012f * breath, 0f);
            p.WaistLean += 2f * breath;
            p.ShoulderLUp = p.ShoulderRUp = 2f + 2.5f * breath;
            p.Blade = V(0.012f * breath, 1f, 0.03f);
            p.HeadYaw += 7f * Mathf.Sin(t * Mathf.PI * 2f + 1.1f);
            return p;
        });

        private static ForgeClip SpearRun()
        {
            var clip = new ForgeClip("run_spear", 0.56f, true) { Wind = _ => new Vector3(0f, 1.4f, -10f) };
            return clip.From(t =>
            {
                float phase = t * Mathf.PI * 2f;
                float left = Mathf.Sin(phase), right = -left;
                float leftLift = Mathf.Max(0f, Mathf.Cos(phase)), rightLift = Mathf.Max(0f, -Mathf.Cos(phase));
                float bounce = Mathf.Cos(phase * 2f);
                // Trailed level in the right hand, head forward and low; the left arm pumps.
                return new ForgePose
                {
                    Hips = V(0.012f * left, -0.13f + 0.05f * bounce, 0.09f),
                    HipLean = 22f,
                    HipYaw = 12f * left,
                    HipRoll = 6f * left,
                    WaistLean = 12f,
                    WaistYaw = 10f - 12f * left,
                    HeadLean = -26f,
                    FootL = V(-0.11f, 0.3f * leftLift, 0.54f * left - 0.08f),
                    FootR = V(0.11f, 0.3f * rightLift, 0.54f * right - 0.08f),
                    FootLTurn = 2f,
                    FootRTurn = 2f,
                    FootLTip = 34f * Mathf.Max(0f, -left) - 14f * leftLift,
                    FootRTip = 34f * Mathf.Max(0f, -right) - 14f * rightLift,
                    HandR = V(0.38f, 0.92f + 0.02f * bounce, 0.02f),
                    ElbowR = V(0.6f, 0.1f, -1f),
                    HandL = V(-0.3f, 1.04f + 0.06f * right, 0.3f * right + 0.08f),
                    ElbowL = V(-0.6f, -0.5f, -0.6f),
                    AimBlade = true,
                    Blade = V(0.04f, -0.14f + 0.03f * bounce, 1f),
                    BladeBack = LeafUp,
                };
            });
        }

        private static ForgeClip SpearGuard() =>
            new ForgeClip("guard_spear", 0.14f).Key(0f, SpearUpright()).Key(1f, SpearBehind(), Ease.Out);

        private static ForgeClip SpearGuardHit()
        {
            // The shaft is knocked off the line to one side and brought straight back.
            var knocked = With(SpearBehind(), p =>
            {
                p.Blade = V(0.36f, 0.2f, 1f);
                p.HandR = V(0.32f, 0.9f, -0.22f);
                p.WaistLean -= 6f;
                p.ShoulderLUp = p.ShoulderRUp = 10f;
            });
            return new ForgeClip("guard_hit_spear", 0.22f).Key(0f, SpearBehind()).Key(0.25f, knocked, Ease.Snap).Key(1f, SpearBehind(), Ease.InOut);
        }

        private static ForgeClip SpearGuardPerfect()
        {
            // A tight circle of the point round the incoming weapon, flung out to the right, and back on the line a hand further forward.
            var under = With(SpearBehind(), p => p.Blade = V(-0.36f, 0.02f, 1f));
            var over = With(SpearBehind(), p => { p.Blade = V(-0.1f, 0.44f, 1f); p.HandR = V(0.3f, 0.9f, -0.1f); });
            var flung = With(SpearBehind(), p => { p.Blade = V(0.6f, 0.12f, 1f); p.HandR = V(0.34f, 0.96f, 0f); p.WaistYaw = 4f; });
            var forward = With(SpearBehind(), p => p.HandR = V(0.3f, 0.92f, -0.04f));
            return new ForgeClip("guard_perfect_spear", 0.3f).Key(0f, SpearBehind()).Key(0.18f, under, Ease.Out).Key(0.36f, over, Ease.Linear)
                .Key(0.56f, flung, Ease.Snap).Key(0.75f, flung, Ease.Linear).Key(1f, forward, Ease.InOut);
        }

        private static ForgeClip SpearCharge(float seconds) =>
            new ForgeClip("charge_spear", seconds).Key(0f, SpearUpright()).Key(0.45f, SpearLine(), Ease.InOut).Key(0.85f, SpearCouched(), Ease.Out)
                .Key(1f, With(SpearCouched(), p => p.Hips = V(0f, -0.33f, 0.07f)), Ease.InOut);

        // The right hand drawn back through the left until the fist is behind the hip, the weight on the back foot.
        private static ForgePose SpearDrawn() => With(Spearman(V(0f, -0.22f, -0.14f), 58f, 30f, -4f, V(-0.12f, 0f, 0.3f), V(0.2f, 0f, -0.32f),
            V(0.38f, 1.02f, -0.46f), V(-0.17f, 0.05f, 1f), -0.86f), p => p.FootRTurn = 56f);

        // The thrust at full stretch: the hips square, the chest over the front knee, the right fist driven up to meet the left.
        private static ForgePose SpearThrustOut() => With(Spearman(V(0f, -0.3f, 0.32f), 8f, 2f, 36f, V(-0.12f, 0f, 0.68f), V(0.2f, 0f, -0.42f),
            V(0.12f, 1.1f, 0.74f), V(-0.02f, 0f, 1f), -0.2f), p =>
        {
            p.FootRTip = 34f;
            p.HeadLean = -22f;
            p.ElbowR = V(0.6f, -0.6f, 0f);
        });

        /// <summary>The thrust: drawn back, driven out flat at chest height with the body leaning into it, and pulled straight back along the same line.</summary>
        private static ForgeClip SpearThrust(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var dipped = With(SpearThrustOut(), p => { p.Blade = V(-0.02f, -0.07f, 1f); p.Hips = V(0f, -0.32f, 0.34f); });
            // Pulled back as fast as it was pushed: the fist behind the hip again before the stance settles.
            var withdrawn = With(SpearDrawn(), p => { p.HandR = V(0.36f, 1.0f, -0.34f); p.GripGap = -0.76f; });
            var clip = Attack(step, -6f);
            return clip.Key(0f, SpearUpright()).Key(hit * 0.62f, SpearDrawn(), Ease.Out).Key(hit, SpearThrustOut(), Ease.In)
                .Key(done, dipped, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.34f), withdrawn, Ease.In).Key(Mathf.Lerp(done, 1f, 0.75f), SpearLine(), Ease.Out).Key(1f, SpearLine(), Ease.Linear);
        }

        /// <summary>The second thrust, high: the back foot passes the front and the point goes in at head height, the right hand finishing above the shoulder.</summary>
        private static ForgeClip SpearHighThrust(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var cocked = With(Spearman(V(0f, -0.12f, -0.04f), 46f, 24f, 0f, V(-0.12f, 0f, 0.3f), V(0.2f, 0f, -0.28f),
                V(0.34f, 1.52f, -0.26f), V(-0.12f, -0.1f, 1f), -0.72f), p =>
            {
                p.ElbowR = V(0.9f, 0.5f, -0.4f);
                p.ShoulderRUp = 18f;
            });
            var high = With(Spearman(V(0f, -0.2f, 0.3f), -14f, -14f, 24f, V(-0.16f, 0f, -0.06f), V(0.16f, 0f, 0.66f),
                V(0.2f, 1.64f, 0.56f), V(-0.06f, -0.12f, 1f), -0.3f), p =>
            {
                p.ElbowR = V(0.9f, 0.5f, -0.2f);
                p.ShoulderRUp = 24f;
                p.FootLTip = 34f;
                p.HeadLean = -16f;
            });
            var clip = Attack(step, -6f);
            return clip.Key(0f, SpearLine()).Key(hit * 0.6f, cocked, Ease.Out).Key(hit, high, Ease.In)
                .Key(done, With(high, p => p.Blade = V(-0.06f, -0.18f, 1f)), Ease.Out).Key(Mathf.Lerp(done, 1f, 0.6f), SpearLine(), Ease.InOut).Key(1f, SpearLine(), Ease.Linear);
        }

        /// <summary>The sweep: the point taken out wide and low on the right with both hands near the butt, a flat cut right to left, and the shaft carried on round over the head back to the line.</summary>
        private static ForgeClip SpearSweep(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var wide = With(Spearman(V(0.04f, -0.24f, -0.04f), 50f, 38f, 4f, V(-0.14f, 0f, 0.28f), V(0.22f, 0f, -0.26f),
                V(0.5f, 0.96f, -0.06f), V(0.92f, -0.12f, 0.36f), -0.3f, 0.6f), p => p.FootRTurn = 50f);
            var across = With(Spearman(V(0f, -0.22f, -0.06f), 4f, -8f, -10f, V(-0.16f, 0f, 0.3f), V(0.22f, 0f, -0.24f),
                V(0.1f, 1.02f, 0.52f), V(0.04f, -0.05f, 1f), -0.3f), p =>
            {
                p.ElbowR = V(0.6f, -0.7f, 0f);
                p.ElbowL = V(-0.6f, -0.7f, 0f);
            });
            var left = Spearman(V(-0.02f, -0.2f, -0.04f), -30f, -40f, -6f, V(-0.16f, 0f, 0.3f), V(0.22f, 0f, -0.24f),
                V(-0.4f, 1.1f, 0.3f), V(-0.96f, 0.04f, 0.26f), -0.3f, 0.6f);
            // Round over the head, level, like a rotor: behind, then out to the right, and down onto the line.
            var behind = With(Spearman(V(0f, -0.1f, 0f), -10f, -10f, 0f, V(-0.16f, 0f, 0.3f), V(0.22f, 0f, -0.24f),
                V(0.02f, 1.94f, 0.02f), V(-0.3f, 0.06f, -0.95f), -0.3f, 0.6f), p =>
            {
                p.ShoulderLUp = p.ShoulderRUp = 24f;
                p.ElbowR = V(0.8f, 0.3f, 0.4f);
                p.ElbowL = V(-0.8f, 0.3f, 0.4f);
            });
            var right = With(behind, p =>
            {
                p.HandR = V(0.16f, 1.8f, 0.1f);
                p.Blade = V(0.95f, 0.04f, 0.3f);
                p.HipYaw = 20f;
                p.WaistYaw = 14f;
            });
            var clip = Attack(step, 0f);
            clip.SheetMoments = new[] { 0f, hit * 0.7f, hit, done, Mathf.Lerp(done, 1f, 0.3f), Mathf.Lerp(done, 1f, 0.55f), Mathf.Lerp(done, 1f, 0.8f), 1f };
            return clip.Key(0f, SpearLine()).Key(hit * 0.65f, wide, Ease.Out).Key(hit, across, Ease.In).Key(done, left, Ease.Linear)
                .Key(Mathf.Lerp(done, 1f, 0.35f), behind, Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.62f), right, Ease.Linear).Key(1f, SpearLine(), Ease.Out);
        }

        /// <summary>From behind the guard: only the arms move. The right hand shoots the shaft through the left and brings it back.</summary>
        private static ForgeClip SpearGuardThrust(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var back = With(SpearBehind(), p => { p.HandR = V(0.34f, 0.9f, -0.36f); p.GripGap = -0.8f; });
            var shot = With(SpearBehind(), p =>
            {
                p.HandR = V(0.2f, 1.0f, 0.34f);
                p.GripGap = -0.2f;
                p.Blade = V(-0.07f, 0.1f, 1f);
                p.ShoulderRForward = 20f;
            });
            var clip = Attack(step, 0f);
            clip.Shape = null;
            return clip.Key(0f, SpearBehind()).Key(hit * 0.7f, back, Ease.Out).Key(hit, shot, Ease.In).Key(done, shot, Ease.Linear)
                .Key(Mathf.Lerp(done, 1f, 0.5f), SpearBehind(), Ease.Out).Key(1f, SpearBehind(), Ease.Linear);
        }

        // Brought over and down like a flail: the head on the ground far ahead, the bearer bent double behind it.
        private static ForgePose SpearSlapped() => With(Spearman(V(-0.06f, -0.44f, 0.2f), 6f, 0f, 40f, V(-0.2f, 0f, 0.5f), V(0.2f, 0f, -0.4f),
            V(0.14f, 0.7f, 0.62f), V(0f, -0.3f, 0.95f), 0.3f), p =>
        {
            p.FootRTip = 30f;
            p.HeadLean = -24f;
            p.ElbowR = V(0.8f, -0.4f, 0f);
            p.ElbowL = V(-0.8f, -0.4f, 0f);
        });

        // Let fall backward over the right shoulder, the chest open and the weight on the back foot.
        private static ForgePose SpearTippedBack() => With(Spearman(V(0f, -0.1f, -0.16f), 16f, 10f, -14f, V(-0.14f, 0f, 0.26f), V(0.2f, 0f, -0.3f),
            V(0.24f, 1.8f, -0.06f), V(0.12f, 0.3f, -0.95f), 0.3f), p =>
        {
            p.ShoulderLUp = p.ShoulderRUp = 24f;
            p.ElbowR = V(0.7f, 0.4f, 0.5f);
            p.ElbowL = V(-0.7f, 0.4f, 0.5f);
        });

        // Coming over the top: between fallen back and slapped down the point passes straight overhead, never round the side.
        private static ForgePose SpearOverTop() => With(Spearman(V(0f, -0.06f, 0.02f), 8f, 4f, 4f, V(-0.16f, 0f, 0.36f), V(0.2f, 0f, -0.34f),
            V(0.2f, 1.94f, 0.2f), V(0f, 0.92f, 0.4f), 0.3f), p =>
        {
            p.BladeBack = V(0f, 0.4f, -0.92f);
            p.ShoulderLUp = p.ShoulderRUp = 26f;
            p.ElbowR = V(0.7f, 0.3f, 0.5f);
            p.ElbowL = V(-0.7f, 0.3f, 0.5f);
            p.FootRTip = 20f;
        });

        /// <summary>The long chop: raised upright, let fall back over the shoulder, and slapped down the whole length of the lane.</summary>
        private static ForgeClip SpearChop(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var raised = With(Spearman(V(0f, -0.04f, -0.04f), 12f, 6f, -4f, V(-0.14f, 0f, 0.2f), V(0.2f, 0f, -0.2f),
                V(0.2f, 1.74f, 0.1f), V(0f, 1f, -0.08f), 0.3f), p =>
            {
                p.BladeBack = V(0f, 0f, -1f);
                p.ShoulderLUp = p.ShoulderRUp = 22f;
                p.ElbowR = V(0.8f, 0.3f, 0.5f);
                p.ElbowL = V(-0.8f, 0.3f, 0.5f);
            });
            var bounce = With(SpearSlapped(), p => { p.Blade = V(0f, -0.2f, 0.98f); p.Hips = V(-0.06f, -0.48f, 0.24f); });
            var clip = Attack(step, -7f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 1.6f);
            return clip.Key(0f, SpearUpright()).Key(hit * 0.4f, raised, Ease.Out).Key(hit * 0.8f, SpearTippedBack(), Ease.InOut)
                .Key(hit * 0.93f, SpearOverTop(), Ease.In).Key(hit, SpearSlapped(), Ease.Linear).Key(done, bounce, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.3f), SpearSlapped(), Ease.In)
                .Key(Mathf.Lerp(done, 1f, 0.55f), SpearSlapped(), Ease.Linear).Key(1f, SpearLine(), Ease.InOut);
        }

        /// <summary>The vault: the head stabbed into the ground a stride ahead, the bearer swung up on the shaft with the body level, and both feet driven into the target.</summary>
        private static ForgeClip SpearVault(AttackStep step)
        {
            var (hit, done) = Moments(step);
            // The spear is planted to the bearer's right and the body goes up on its left, so the shaft is seen beside the body from above.
            var planted = With(Spearman(V(-0.08f, -0.2f, 0.1f), 10f, 6f, 16f, V(-0.16f, 0f, -0.2f), V(0.14f, 0f, 0.36f),
                V(0.38f, 1.06f, 0.34f), V(0.05f, -0.42f, 0.9f), -0.4f), p => p.FootLTip = 30f);
            var kick = With(Spearman(V(-0.22f, 0.22f, 0.4f), -20f, -14f, -42f, V(-0.3f, 0.84f, 1.16f), V(-0.1f, 0.9f, 1.2f),
                V(0.4f, 1.22f, 0.4f), V(0.1f, -0.5f, 0.86f), -0.34f), p =>
            {
                p.HipRoll = 14f;
                p.FootLTip = -30f;
                p.FootRTip = -30f;
                p.HeadLean = 30f;
                p.KneeL = V(-0.2f, 1f, 0.3f);
                p.KneeR = V(0.2f, 1f, 0.3f);
            });
            var landed = With(Spearman(V(0f, -0.36f, 0.5f), 20f, 10f, 20f, V(-0.14f, 0f, 0.86f), V(0.2f, 0f, 0.3f),
                V(0.3f, 0.96f, 0.2f), V(0.1f, -0.36f, -0.93f), 0.3f), p =>
            {
                p.TwoHanded = false;
                p.HandL = V(-0.36f, 1.0f, 0.7f);
                p.ElbowR = V(0.6f, 0f, -1f);
            });
            var clip = Attack(step, -9f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 1.5f);
            clip.SheetMoments = new[] { 0f, hit * 0.5f, hit * 0.8f, hit, done, Mathf.Lerp(done, 1f, 0.3f), Mathf.Lerp(done, 1f, 0.7f), 1f };
            return clip.Key(0f, SpearLine()).Key(hit * 0.5f, planted, Ease.Out).Key(hit, kick, Ease.In).Key(done, kick, Ease.Linear)
                .Key(Mathf.Lerp(done, 1f, 0.3f), landed, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.55f), landed, Ease.Linear)
                .Key(Mathf.Lerp(done, 1f, 0.8f), With(SpearLine(), p => { p.HandR = V(0.42f, 0.98f, 0.1f); p.Blade = V(0.95f, -0.12f, 0.24f); p.GripGap = -0.3f; }), Ease.In)
                .Key(1f, SpearLine(), Ease.Out);
        }

        /// <summary>The couched charge: a low run with the spear locked under the arm, and a long skid on both feet, turning side-on to stop.</summary>
        private static ForgeClip SpearCouchedCharge(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var strideA = With(SpearCouched(), p =>
            {
                p.Hips = V(0f, -0.3f, 0.2f);
                p.FootL = V(-0.12f, 0.24f, -0.5f);
                p.FootR = V(0.14f, 0f, 0.56f);
                p.FootLTip = 50f;
                p.FootRTip = 0f;
                p.WaistLean = 24f;
            });
            var strideB = With(SpearCouched(), p =>
            {
                p.Hips = V(0f, -0.28f, 0.22f);
                p.FootL = V(-0.12f, 0f, 0.6f);
                p.FootR = V(0.14f, 0.26f, -0.5f);
                p.FootRTip = 50f;
                p.WaistLean = 24f;
            });
            var skid = With(Spearman(V(0f, -0.4f, -0.08f), 66f, 16f, -6f, V(-0.1f, 0f, 0.56f), V(0.2f, 0f, -0.5f),
                V(0.32f, 0.98f, -0.14f), V(-0.2f, 0.06f, 1f)), p =>
            {
                p.FootRTurn = 70f;
                p.FootLTurn = 40f;
            });
            var clip = Attack(step, -12f);
            clip.SheetMoments = new[] { 0f, hit, Mathf.Lerp(hit, done, 0.33f), Mathf.Lerp(hit, done, 0.66f), done, Mathf.Lerp(done, 1f, 0.3f), Mathf.Lerp(done, 1f, 0.7f), 1f };
            return clip.Key(0f, SpearCouched()).Key(hit, strideA, Ease.Out).Key(Mathf.Lerp(hit, done, 0.33f), strideB, Ease.Linear)
                .Key(Mathf.Lerp(hit, done, 0.66f), strideA, Ease.Linear).Key(done, strideB, Ease.Linear)
                .Key(Mathf.Lerp(done, 1f, 0.3f), skid, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.7f), With(skid, p => p.Hips = V(0f, -0.36f, -0.1f)), Ease.Linear)
                .Key(1f, SpearLine(), Ease.InOut);
        }

        // The throw's end: the body gone over the front leg after the arm, the right hand almost on the ground.
        private static ForgePose SpearThrown() => With(Spearman(V(0f, -0.46f, 0.36f), -34f, -30f, 56f, V(-0.14f, 0f, 0.66f), V(0.2f, 0.1f, -0.5f),
            V(0.12f, 0.36f, 0.8f), V(0f, -0.1f, 1f)), p =>
        {
            p.TwoHanded = false;
            p.HandL = V(-0.46f, 1.1f, -0.3f);
            p.ElbowL = V(-0.8f, 0.4f, -0.6f);
            p.FootRTip = 60f;
            p.HeadLean = -40f;
        });

        /// <summary>The Flying Spear: a crow-hop with the spear arm drawn far back and the left hand pointing, the throw, and the body following the arm over the front leg.</summary>
        private static ForgeClip SpearThrow(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var cocked = With(Spearman(V(0f, -0.14f, -0.14f), 72f, 30f, -12f, V(-0.1f, 0f, 0.42f), V(0.24f, 0.06f, -0.4f),
                V(0.26f, 1.7f, -0.6f), V(-0.1f, 0.1f, 1f)), p =>
            {
                p.TwoHanded = false;
                p.HandL = V(-0.1f, 1.46f, 0.72f);
                p.ElbowL = V(-0.6f, -0.4f, 0f);
                p.ElbowR = V(0.9f, 0.3f, -0.6f);
                p.ShoulderRUp = 20f;
                p.FootRTip = 30f;
            });
            var release = With(Spearman(V(0f, -0.3f, 0.26f), -24f, -26f, 34f, V(-0.14f, 0f, 0.62f), V(0.2f, 0f, -0.44f),
                V(0.2f, 1.44f, 0.8f), V(0f, -0.06f, 1f)), p =>
            {
                p.TwoHanded = false;
                p.HandL = V(-0.4f, 1.1f, -0.1f);
                p.FootRTip = 46f;
                p.ShoulderRForward = 26f;
                p.HeadLean = -26f;
            });
            // The crow-hop: both feet leave the ground as the back foot crosses behind and the spear goes up.
            var hop = With(cocked, p =>
            {
                p.Hips = V(0f, -0.02f, -0.06f);
                p.FootL = V(-0.1f, 0.1f, 0.26f);
                p.FootR = V(0.02f, 0.14f, -0.12f);
                p.HandR = V(0.3f, 1.5f, -0.3f);
                p.HandL = V(-0.2f, 1.3f, 0.5f);
                p.HipYaw = 56f;
            });
            var clip = Attack(step, -6f);
            clip.SheetMoments = new[] { 0f, hit * 0.4f, hit * 0.75f, hit * 0.95f, hit, done + 0.06f, done + 0.12f, 1f };
            return clip.Key(0f, SpearCouched()).Key(hit * 0.4f, hop, Ease.Out).Key(hit * 0.75f, cocked, Ease.InOut).Key(hit, release, Ease.In).Key(done + 0.12f, SpearThrown(), Ease.Out)
                .Key(1f, With(SpearThrown(), p => p.Hips = V(0f, -0.44f, 0.4f)), Ease.Linear);
        }

        /// <summary>The run to the thrown spear: pushed off the hand, low and long, arriving with both hands reaching for the shaft.</summary>
        private static ForgeClip SpearDash(float seconds)
        {
            var push = With(SpearThrown(), p =>
            {
                p.FootL = V(-0.12f, 0.1f, -0.6f);
                p.FootR = V(0.14f, 0.3f, 0.6f);
                p.Hips = V(0f, -0.4f, 0.3f);
                p.HandR = V(0.3f, 0.8f, -0.3f);
                p.HandL = V(-0.3f, 1.0f, 0.5f);
                p.FootLTip = 50f;
            });
            var stride = With(push, p =>
            {
                p.FootL = V(-0.12f, 0.3f, 0.7f);
                p.FootR = V(0.14f, 0.1f, -0.7f);
                p.HandR = V(0.3f, 1.0f, 0.5f);
                p.HandL = V(-0.3f, 0.8f, -0.3f);
                p.FootRTip = 50f;
                p.FootLTip = 0f;
            });
            var reaching = With(push, p =>
            {
                p.Hips = V(0f, -0.34f, 0.2f);
                p.WaistLean = 20f;
                p.HipLean = 14f;
                p.HandR = V(0.16f, 1.06f, 0.62f);
                p.HandL = V(-0.1f, 1.0f, 0.7f);
            });
            var clip = new ForgeClip("dash_spear", seconds) { Wind = _ => new Vector3(0f, 0f, -14f) };
            return clip.Key(0f, SpearThrown()).Key(0.3f, push, Ease.Out).Key(0.65f, stride, Ease.Linear).Key(1f, reaching, Ease.Linear);
        }

        /// <summary>Arriving: both hands take the shaft and the body's speed is turned into one full spin that rips the spear out sideways and strikes all round.</summary>
        private static ForgeClip SpearRecall(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var taken = With(Spearman(V(0f, -0.34f, 0.2f), 0f, 0f, 22f, V(-0.14f, 0f, 0.5f), V(0.2f, 0f, -0.36f),
                V(0.16f, 1.06f, 0.6f), V(0f, -0.36f, 0.93f), -0.4f), p => p.FootRTip = 30f);
            var ripping = With(Spearman(V(0f, -0.26f, -0.1f), 0f, 0f, -14f, V(-0.06f, 0f, 0f), V(0.4f, 0.12f, -0.28f),
                V(0.2f, 1.02f, 0.54f), V(0.3f, -0.06f, 0.95f), -0.3f, 0f), p =>
            {
                p.ElbowR = V(0.6f, -0.7f, 0f);
                p.ElbowL = V(-0.6f, -0.7f, 0f);
                p.HeadYaw = -30f;
            });
            var clip = Attack(step, 0f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 1.4f);
            clip.SheetMoments = new[] { 0f, hit * 0.5f, hit * 0.8f, hit, (hit + done) * 0.5f, done, Mathf.Lerp(done, 1f, 0.4f), 1f };
            clip.Key(0f, taken).Key(hit * 0.45f, With(taken, p => p.Hips = V(0f, -0.4f, 0.1f)), Ease.Out).Key(hit * 0.8f, ripping, Ease.In)
                .Key(done + 0.06f, With(ripping, p => p.Blade = V(0.3f, 0.1f, 0.95f)), Ease.Linear).Key(1f, SpearLine(), Ease.InOut);
            float from = hit * 0.5f, to = done + 0.1f;
            clip.MovesRoot = true;
            clip.After = TurnAbout(t => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(from, to, t)) * -360f, Vector3.up, Vector3.zero);
            return clip;
        }

        // The arms of one whose spear is somewhere else: low and open, ready to run or to catch.
        private static ForgePose SpearEmpty() => With(SpearLine(), p =>
        {
            p.TwoHanded = false;
            p.HandR = V(0.34f, 0.96f, 0.26f);
            p.HandL = V(-0.3f, 1.0f, 0.34f);
            p.Blade = V(0f, 0.3f, 1f);
            p.WaistLean = 16f;
        });

        private static ForgeClip SpearAway() => new ForgeClip("away_spear", 0.2f).Key(0f, SpearEmpty()).Key(1f, SpearEmpty(), Ease.Linear);

        private static ForgeClip SpearCatch()
        {
            // The hand goes out to meet it; the shaft slaps into the palm and is swung down onto the line.
            var met = With(SpearEmpty(), p =>
            {
                p.HandR = V(0.36f, 1.5f, 0.5f);
                p.Blade = V(0f, 0.2f, -1f);
                p.ShoulderRUp = 14f;
            });
            var taken = With(met, p =>
            {
                p.HandR = V(0.36f, 1.4f, 0.2f);
                p.Blade = V(0.3f, 0.9f, -0.2f);
                p.WaistYaw = 30f;
            });
            return new ForgeClip("catch_spear", 0.28f).Key(0f, met).Key(0.35f, taken, Ease.Snap).Key(1f, SpearUpright(), Ease.InOut);
        }

        /// <summary>
        /// The Standard. A low thrust under the ribs; the butt dropped to the ground and the body levered up on the
        /// point, hand over hand, until the shaft stands upright; the hold, head bowed beneath it; then the whole
        /// length brought over and down, and the spear drawn out of the ground with one hand.
        /// </summary>
        private static ForgeClip SpearStandard(AttackStep step)
        {
            var (hit, done) = Moments(step);
            float total = step.Total;
            float thrust = 0.14f / total, lever = 0.26f / total, hold = step.HoldAt / total;
            var low = With(Spearman(V(0f, -0.4f, 0.24f), 16f, 6f, 30f, V(-0.12f, 0f, 0.56f), V(0.2f, 0f, -0.4f),
                V(0.16f, 0.78f, 0.56f), V(0f, 0.04f, 1f), -0.3f), p => p.FootRTip = 34f);
            var levering = With(Spearman(V(0f, -0.34f, 0.06f), 10f, 4f, -8f, V(-0.16f, 0f, 0.36f), V(0.2f, 0f, -0.3f),
                V(0.14f, 1.2f, 0.44f), V(0f, 0.62f, 0.78f), 0.36f), p =>
            {
                p.ElbowR = V(0.8f, -0.4f, 0.2f);
                p.ElbowL = V(-0.8f, -0.4f, 0.2f);
            });
            // Beneath it: both hands on the upright shaft, feet wide, the head bowed.
            var standard = With(Spearman(V(0f, -0.26f, 0.02f), 14f, 0f, -2f, V(-0.2f, 0f, 0.3f), V(0.22f, 0f, -0.34f),
                V(0.05f, 1.5f, 0.46f), V(0f, 1f, 0.03f), 0.4f, 0f), p =>
            {
                p.BladeBack = V(0f, 0f, -1f);
                p.HeadLean = 38f;
                p.ShoulderLUp = p.ShoulderRUp = 14f;
                p.FootRTip = 24f;
                p.ElbowR = V(0.9f, -0.3f, 0.1f);
                p.ElbowL = V(-0.9f, -0.3f, 0.1f);
            });
            var still = With(standard, p => { p.Hips = V(0f, -0.31f, 0.02f); p.HandR = V(0.05f, 1.47f, 0.46f); });
            var back = With(SpearTippedBack(), p =>
            {
                p.Hips = V(0f, -0.1f, -0.26f);
                p.FootL = V(-0.2f, 0f, 0f);
                p.FootR = V(0.22f, 0f, -0.46f);
            });
            var slam = With(SpearSlapped(), p => p.Hips = V(-0.06f, -0.48f, 0.1f));
            var leaning = With(slam, p => { p.Hips = V(-0.06f, -0.52f, 0.14f); p.HeadLean = 0f; });
            var drawing = With(Spearman(V(0f, -0.1f, 0.1f), 20f, 6f, 6f, V(-0.14f, 0f, 0.3f), V(0.18f, 0f, -0.1f),
                V(0.34f, 1.1f, 0.4f), V(0f, 0.5f, 0.86f)), p =>
            {
                p.TwoHanded = false;
                p.HandL = V(-0.24f, 0.98f, 0.1f);
            });
            var clip = Attack(step, -4f);
            clip.Shape = ForgeClip.Impact(hit, total, 2.2f);
            clip.SheetMoments = new[] { thrust, lever, hold + 0.02f, hit * 0.9f, hit * 0.96f, hit * 0.985f, hit, 0.88f };
            return clip.Key(0f, SpearLine()).Key(thrust, low, Ease.In).Key(lever, levering, Ease.InOut).Key(hold, standard, Ease.Out)
                .Key(hit * 0.9f, still, Ease.Linear).Key(hit * 0.96f, back, Ease.Out).Key(hit * 0.985f, SpearOverTop(), Ease.In).Key(hit, slam, Ease.Linear).Key(done, leaning, Ease.Out)
                .Key(Mathf.Lerp(done, 1f, 0.4f), leaning, Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.8f), drawing, Ease.InOut).Key(1f, SpearUpright(), Ease.InOut);
        }
    }
}

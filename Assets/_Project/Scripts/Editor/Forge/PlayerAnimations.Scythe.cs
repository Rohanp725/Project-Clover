using System.Collections.Generic;
using Clover.Player;
using UnityEngine;

namespace Clover.EditorTools.Forge
{
    /// <summary>
    /// The Scythe's clips. Its whole character is the pendulum: no cut stops, each is caught and turned back, the
    /// crescent always leading and the body leaning away from its pull. The right hand holds the haft near the blade
    /// end and the left the peg behind it; the pose's blade is the haft, and the crescent hangs off its far end on the
    /// side the cut is travelling to.
    /// </summary>
    public static partial class PlayerAnimations
    {
        private const float ScytheGrip = 0.42f;

        private static IEnumerable<ForgeClip> ScytheClips()
        {
            var combo = Weapons.Scythe().Combo;
            yield return ScytheIdle();
            yield return ScytheRun();
            yield return ScytheGuard();
            yield return ScytheGuardHit();
            yield return ScytheGuardPerfect();
            yield return ScytheCharge(combo.ChargeTime);
            yield return ScytheOut(combo.Steps[0]);
            yield return ScytheBack(combo.Steps[1]);
            yield return ScytheFullMoon(combo.Steps[2]);
            yield return ScytheBackReap(combo.Heavy);
            yield return ScytheRisingMoon(combo.ChainHeavy);
            yield return ScytheCrescent(combo.Skill);
            yield return ScytheWheel(combo.Finisher);
        }

        private static ForgePose Reaper(Vector3 hips, float hipYaw, float waistYaw, float lean, Vector3 footL, Vector3 footR,
            Vector3 hand, Vector3 haft, Vector3 back, float keep = 0.8f) =>
            Two(hips, hipYaw, waistYaw, lean, footL, footR, hand, haft, back, ScytheGrip, keep);

        /// <summary>Upright, almost at ease: the haft over the right shoulder, the crescent behind the head pointing left like a dark halo.</summary>
        private static ForgePose ScytheCarry() => With(Reaper(V(0f, -0.04f, 0f), 10f, 4f, 2f, V(-0.15f, 0f, 0.08f), V(0.17f, 0f, -0.08f),
            V(0.24f, 1.34f, 0.2f), V(0.16f, 0.5f, -0.85f), V(1f, 0.1f, 0f)), p =>
        {
            p.ElbowR = V(0.8f, -0.6f, 0.2f);
            p.ElbowL = V(-0.4f, -0.9f, 0.2f);
        });

        // Carried round to the far left and a little up: where the first cut ends and the return begins.
        private static ForgePose ScytheFarLeft() => Reaper(V(0.06f, -0.14f, -0.1f), -26f, -42f, -10f, V(-0.22f, 0f, 0.06f), V(0.2f, 0f, -0.04f),
            V(-0.46f, 1.12f, 0.2f), V(-0.93f, 0.16f, 0.33f), BackLeftward(V(-0.93f, 0f, 0.33f)), 0.6f);

        // Swung up behind the right shoulder: where the return ends and the full moon begins.
        private static ForgePose ScytheBehindRight() => Reaper(V(-0.05f, -0.12f, -0.06f), 30f, 46f, -6f, V(-0.2f, 0f, 0.06f), V(0.22f, 0f, -0.08f),
            V(0.42f, 1.5f, 0f), V(0.62f, 0.46f, -0.63f), BackRightward(V(0.62f, 0f, -0.63f)), 0.6f);

        private static ForgeClip ScytheIdle() => new ForgeClip("idle_scythe", 3.2f, true).From(t =>
        {
            // The slowest idle: one long breath a loop.
            float breath = Mathf.Sin(t * Mathf.PI * 2f);
            var p = ScytheCarry();
            p.Hips += V(0.01f * breath, 0.014f * breath, 0f);
            p.WaistLean += 2f * breath;
            p.ShoulderLUp = p.ShoulderRUp = 2f + 2.5f * breath;
            p.Blade = V(0.16f, 0.5f + 0.03f * breath, -0.85f);
            p.HeadYaw += 6f * Mathf.Sin(t * Mathf.PI * 2f + 0.9f);
            return p;
        });

        private static ForgeClip ScytheRun()
        {
            var clip = new ForgeClip("run_scythe", 0.58f, true) { Wind = _ => new Vector3(0f, 1.4f, -10f) };
            return clip.From(t =>
            {
                float phase = t * Mathf.PI * 2f;
                float left = Mathf.Sin(phase), right = -left;
                float leftLift = Mathf.Max(0f, Mathf.Cos(phase)), rightLift = Mathf.Max(0f, -Mathf.Cos(phase));
                float bounce = Mathf.Cos(phase * 2f);
                // The haft level at the right hip in both hands, the blade trailing low behind with its edge up.
                return new ForgePose
                {
                    Hips = V(0.012f * left, -0.14f + 0.05f * bounce, 0.09f),
                    HipLean = 22f,
                    HipYaw = 14f * left,
                    HipRoll = 6f * left,
                    WaistLean = 13f,
                    WaistYaw = 16f - 14f * left,
                    HeadLean = -28f,
                    HeadYaw = -12f,
                    FootL = V(-0.11f, 0.3f * leftLift, 0.52f * left - 0.08f),
                    FootR = V(0.11f, 0.3f * rightLift, 0.52f * right - 0.08f),
                    FootLTurn = 2f,
                    FootRTurn = 2f,
                    FootLTip = 34f * Mathf.Max(0f, -left) - 14f * leftLift,
                    FootRTip = 34f * Mathf.Max(0f, -right) - 14f * rightLift,
                    HandR = V(0.34f, 0.96f + 0.02f * bounce, -0.04f),
                    ElbowR = V(0.6f, 0.2f, -1f),
                    ElbowL = V(-0.6f, -0.6f, 0.2f),
                    AimBlade = true,
                    TwoHanded = true,
                    GripGap = ScytheGrip,
                    Blade = V(0.16f, -0.3f + 0.04f * bounce, -0.94f),
                    // Laid flat with the hook out to the right: from above, a long line behind the player ending in a hook.
                    BladeBack = V(-1f, 0.1f, 0f),
                };
            });
        }

        // The haft slantwise across the body, hands wide, the body side-on behind it. (The blade end is up to the
        // right here, not the left as first drawn: the right hand is the upper hand, and the other way crosses the arms.)
        private static ForgePose ScytheBar() => With(Reaper(V(0f, -0.2f, -0.06f), -24f, -12f, 8f, V(-0.18f, 0f, -0.2f), V(0.18f, 0f, 0.22f),
            V(0.3f, 1.46f, 0.42f), V(0.62f, 0.76f, 0.16f), V(0.2f, 0f, -1f)), p =>
        {
            p.GripGap = 0.6f;
            p.ElbowR = V(1f, -0.2f, -0.2f);
            p.ElbowL = V(-1f, -0.3f, -0.2f);
        });

        private static ForgeClip ScytheGuard() =>
            new ForgeClip("guard_scythe", 0.14f).Key(0f, ScytheCarry()).Key(1f, ScytheBar(), Ease.Out);

        private static ForgeClip ScytheGuardHit()
        {
            // The haft bows in toward the body and the blade end nods.
            var bowed = With(ScytheBar(), p =>
            {
                p.Hips = V(0f, -0.24f, -0.16f);
                p.HandR = V(0.28f, 1.42f, 0.3f);
                p.Blade = V(0.62f, 0.7f, 0.34f);
                p.WaistLean -= 8f;
                p.ShoulderLUp = p.ShoulderRUp = 10f;
            });
            return new ForgeClip("guard_hit_scythe", 0.22f).Key(0f, ScytheBar()).Key(0.25f, bowed, Ease.Snap).Key(1f, ScytheBar(), Ease.InOut);
        }

        private static ForgeClip ScytheGuardPerfect()
        {
            // Rolled about its middle like a staff: the butt knocks the blow aside and the blade end comes over the top.
            var knock = With(ScytheBar(), p =>
            {
                p.HandR = V(-0.1f, 1.3f, 0.5f);
                p.Blade = V(-0.7f, 0.2f, 0.68f);
                p.BladeBack = V(0f, 1f, 0f);
                p.WaistYaw = 20f;
            });
            var over = With(ScytheBar(), p =>
            {
                p.HandR = V(0.2f, 1.7f, 0.3f);
                p.Blade = V(0.1f, 0.9f, 0.42f);
                p.BladeBack = V(1f, 0f, 0f);
            });
            return new ForgeClip("guard_perfect_scythe", 0.3f).Key(0f, ScytheBar()).Key(0.3f, knock, Ease.Snap).Key(0.65f, over, Ease.InOut)
                .Key(1f, ScytheBehindRight(), Ease.Out);
        }

        // Drawn far back to the right, level, at arm's length: the body wound right and sunk on the right leg.
        private static ForgePose ScytheDrawn() => With(Reaper(V(0.1f, -0.26f, -0.08f), 50f, 34f, 6f, V(-0.2f, 0f, 0.2f), V(0.26f, 0f, -0.24f),
            V(0.56f, 1.06f, -0.2f), V(0.62f, -0.14f, -0.77f), BackLeftward(V(0.62f, 0f, -0.77f)), 0.7f), p => p.FootRTurn = 50f);

        private static ForgeClip ScytheCharge(float seconds) =>
            new ForgeClip("charge_scythe", seconds).Key(0f, ScytheCarry()).Key(0.8f, ScytheDrawn(), Ease.Out)
                .Key(1f, With(ScytheDrawn(), p => p.Hips = V(0.11f, -0.3f, -0.1f)), Ease.InOut);

        /// <summary>Out, right to left: a level mowing cut at the knee, arms long, the body leaning back from the blade so the feet stay where they are.</summary>
        private static ForgeClip ScytheOut(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var wound = With(Reaper(V(0.06f, -0.2f, -0.04f), 40f, 30f, 6f, V(-0.2f, 0f, 0.1f), V(0.22f, 0f, -0.1f),
                V(0.52f, 1.0f, -0.12f), V(0.76f, -0.12f, -0.64f), BackLeftward(V(0.76f, 0f, -0.64f)), 0.7f), p => p.GripGap = 0.55f);
            var coming = With(Reaper(V(0.02f, -0.2f, -0.08f), 16f, 20f, -4f, V(-0.2f, 0f, 0.1f), V(0.22f, 0f, -0.1f),
                V(0.5f, 0.92f, 0.24f), V(0.97f, -0.2f, 0.14f), BackLeftward(V(0.97f, 0f, 0.14f))), p => p.GripGap = 0.55f);
            var contact = With(Reaper(V(0f, -0.18f, -0.14f), 0f, -10f, -14f, V(-0.2f, 0f, 0.1f), V(0.22f, 0f, -0.1f),
                V(0.08f, 0.9f, 0.56f), V(0.12f, -0.24f, 0.96f), BackLeftward(V(0.12f, 0f, 0.96f))), p =>
            {
                p.GripGap = 0.55f;
                p.ElbowR = V(0.6f, -0.7f, 0f);
                p.ElbowL = V(-0.6f, -0.7f, 0f);
            });
            var clip = Attack(step, 2f);
            return clip.Key(0f, ScytheCarry()).Key(hit * 0.6f, wound, Ease.Out).Key(hit * 0.9f, coming, Ease.In).Key(hit, contact, Ease.Linear)
                .Key(done, ScytheFarLeft(), Ease.Out).Key(1f, With(ScytheFarLeft(), p => p.HandR = V(-0.48f, 1.16f, 0.16f)), Ease.InOut);
        }

        /// <summary>Back, left to right, at the chest: no wind-up of its own, only the wrists rolling the edge over at the top of the first cut.</summary>
        private static ForgeClip ScytheBack(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var start = With(ScytheFarLeft(), p => p.HandR = V(-0.48f, 1.16f, 0.16f));
            var rolled = With(start, p =>
            {
                p.BladeBack = BackRightward(V(-0.93f, 0f, 0.33f));
                p.Hips = V(0.07f, -0.18f, -0.1f);
            });
            var contact = With(Reaper(V(0f, -0.16f, -0.12f), 4f, 8f, -12f, V(-0.22f, 0f, 0.06f), V(0.2f, 0f, -0.04f),
                V(0.04f, 1.26f, 0.56f), V(-0.1f, 0f, 1f), BackRightward(V(-0.1f, 0f, 1f))), p =>
            {
                p.ElbowR = V(0.6f, -0.7f, 0f);
                p.ElbowL = V(-0.6f, -0.7f, 0f);
            });
            var clip = Attack(step, 2f);
            return clip.Key(0f, start).Key(hit * 0.7f, rolled, Ease.InOut).Key(hit, contact, Ease.In)
                .Key(done, ScytheBehindRight(), Ease.Out).Key(1f, With(ScytheBehindRight(), p => p.HandR = V(0.44f, 1.54f, -0.04f)), Ease.InOut);
        }

        /// <summary>The full moon: the back turned, a long beat, then one whole turn at full stretch with the blade climbing from the knee to the shoulder; the butt planted to end it.</summary>
        private static ForgeClip ScytheFullMoon(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var start = With(ScytheBehindRight(), p => p.HandR = V(0.44f, 1.54f, -0.04f));
            // Wound right until the back is to the enemy, the blade trailing low, the head over the left shoulder.
            var wound = With(Reaper(V(0.04f, -0.3f, -0.04f), 64f, 50f, 10f, V(-0.2f, 0f, 0.14f), V(0.24f, 0f, -0.16f),
                V(0.5f, 0.86f, -0.2f), V(0.5f, -0.34f, -0.8f), BackLeftward(V(0.5f, 0f, -0.8f)), 0f), p =>
            {
                p.HeadYaw = -80f;
                p.GripGap = 0.55f;
            });
            var low = With(Reaper(V(0f, -0.26f, 0f), 0f, 0f, -8f, V(0f, 0f, 0f), V(0.4f, 0.14f, -0.3f),
                V(0.2f, 0.96f, 0.56f), V(0.36f, -0.3f, 0.88f), BackLeftward(V(0.36f, 0f, 0.88f)), 0f), p =>
            {
                p.GripGap = 0.55f;
                p.ElbowR = V(0.6f, -0.7f, 0f);
                p.ElbowL = V(-0.6f, -0.7f, 0f);
                p.HeadYaw = -30f;
            });
            var high = With(low, p =>
            {
                p.Hips = V(0f, -0.1f, 0f);
                p.HandR = V(0.2f, 1.46f, 0.52f);
                p.Blade = V(0.36f, 0.3f, 0.88f);
                p.ShoulderRUp = 16f;
            });
            // Let slide through the hands and caught at the collar; the butt swung down and planted, standing straight beside it.
            var planted = With(Reaper(V(0f, 0f, 0f), 0f, 0f, 0f, V(-0.13f, 0f, 0f), V(0.14f, 0f, 0.02f),
                V(0.34f, 1.3f, 0.26f), V(0f, 1f, 0f), V(1f, 0f, 0f)), p =>
            {
                p.TwoHanded = false;
                p.HandL = V(-0.3f, 0.86f, 0.06f);
            });
            var clip = Attack(step, 0f);
            clip.SheetMoments = new[] { 0f, hit * 0.7f, hit, (hit + done) * 0.5f, done, done + 0.08f, Mathf.Lerp(done, 1f, 0.6f), 1f };
            clip.Key(0f, start).Key(hit * 0.5f, wound, Ease.Out).Key(hit * 0.8f, wound, Ease.Linear).Key(hit, low, Ease.In).Key(done + 0.04f, high, Ease.Linear)
                .Key(Mathf.Lerp(done, 1f, 0.5f), planted, Ease.InOut).Key(Mathf.Lerp(done, 1f, 0.8f), planted, Ease.Linear).Key(1f, ScytheCarry(), Ease.InOut);
            float from = hit * 0.78f, to = done + 0.08f;
            clip.MovesRoot = true;
            clip.After = TurnAbout(t => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(from, to, t)) * -360f, Vector3.up, Vector3.zero);
            return clip;
        }

        /// <summary>The back reap: the blade reached out past the enemy, then both feet leave the ground in a flat hop backward and the arms haul it home across. The cut is made by the retreat.</summary>
        private static ForgeClip ScytheBackReap(AttackStep step)
        {
            var (hit, done) = Moments(step);
            // Reached long, forward and to the left, the crescent turned back toward the bearer.
            var reached = With(Reaper(V(0f, -0.16f, 0.22f), -14f, -20f, 24f, V(-0.18f, 0f, 0.3f), V(0.2f, 0f, -0.28f),
                V(-0.14f, 1.1f, 0.74f), V(-0.4f, -0.12f, 0.91f), BackRightward(V(-0.4f, 0f, 0.91f))), p =>
            {
                p.GripGap = 0.55f;
                p.FootRTip = 30f;
                p.ElbowR = V(0.6f, -0.7f, 0f);
            });
            var held = With(reached, p =>
            {
                p.Hips = V(0f, -0.22f, 0.26f);
                p.HandR = V(-0.16f, 1.06f, 0.78f);
            });
            // In the air, flat and low, the blade coming back and across.
            var hop = With(Reaper(V(0f, 0.06f, -0.06f), 10f, 14f, 10f, V(-0.2f, 0.2f, 0.26f), V(0.22f, 0.26f, -0.2f),
                V(0.22f, 1.0f, 0.4f), V(0.48f, -0.16f, 0.86f), BackRightward(V(0.48f, 0f, 0.86f))), p =>
            {
                p.GripGap = 0.55f;
                p.FootLTip = 30f;
                p.FootRTip = 40f;
            });
            // Landed low, the haft across the knees and the blade out to the right.
            var landed = With(Reaper(V(0.02f, -0.44f, -0.1f), 26f, 30f, 16f, V(-0.24f, 0f, 0.26f), V(0.26f, 0f, -0.3f),
                V(0.44f, 0.74f, 0.2f), V(0.95f, -0.04f, 0.3f), BackRightward(V(0.95f, 0f, 0.3f)), 0.6f), p =>
            {
                p.GripGap = 0.55f;
                p.FootRTip = 30f;
            });
            var clip = Attack(step, 8f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 1.2f);
            return clip.Key(0f, ScytheCarry()).Key(hit * 0.6f, reached, Ease.Out).Key(hit * 0.88f, held, Ease.InOut).Key(hit, hop, Ease.In)
                .Key(done, landed, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), With(landed, p => p.Hips = V(0.02f, -0.38f, -0.1f)), Ease.InOut)
                .Key(1f, ScytheCarry(), Ease.InOut);
        }

        /// <summary>The rising moon: the point dropped to the ground at the left, a skip forward, an upward diagonal to above the right shoulder, and the haft turned once over the head.</summary>
        private static ForgeClip ScytheRisingMoon(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var dropped = With(Reaper(V(0.04f, -0.32f, -0.04f), -30f, -30f, 16f, V(-0.22f, 0f, -0.14f), V(0.2f, 0f, 0.16f),
                V(-0.3f, 0.9f, 0.3f), V(-0.62f, -0.5f, 0.6f), V(0.5f, -0.3f, -0.8f), 0.7f), p => p.GripGap = 0.55f);
            var cut = With(Reaper(V(0f, -0.04f, 0.22f), 6f, 14f, -4f, V(-0.2f, 0.08f, -0.2f), V(0.2f, 0.06f, 0.44f),
                V(0.14f, 1.4f, 0.54f), V(0.2f, 0.5f, 0.84f), V(0.9f, -0.2f, -0.1f)), p =>
            {
                p.GripGap = 0.55f;
                p.FootLTip = 30f;
            });
            var high = With(Reaper(V(0f, 0.02f, 0.24f), 24f, 36f, -10f, V(-0.2f, 0f, -0.2f), V(0.2f, 0f, 0.44f),
                V(0.36f, 1.9f, 0.14f), V(0.5f, 0.74f, -0.45f), V(-0.8f, 0.5f, 0.2f), 0.6f), p =>
            {
                p.ShoulderRUp = 24f;
                p.FootLTip = 26f;
            });
            // Over the head, hand over hand: the haft goes round level above before it settles.
            var over = With(high, p =>
            {
                p.Hips = V(0f, -0.06f, 0.2f);
                p.HandR = V(0.06f, 1.96f, 0.06f);
                p.Blade = V(-0.78f, 0.26f, -0.56f);
                p.BladeBack = V(0f, 1f, 0f);
                p.WaistYaw = 10f;
            });
            var round = With(over, p =>
            {
                p.Blade = V(-0.3f, 0.2f, 0.93f);
                p.HandR = V(0.1f, 1.94f, 0.1f);
            });
            var clip = Attack(step, -8f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 1.2f);
            return clip.Key(0f, ScytheFarLeft()).Key(hit * 0.6f, dropped, Ease.Out).Key(hit, cut, Ease.In).Key(done, high, Ease.Out)
                .Key(Mathf.Lerp(done, 1f, 0.3f), over, Ease.InOut).Key(Mathf.Lerp(done, 1f, 0.55f), round, Ease.Linear)
                .Key(1f, ScytheCarry(), Ease.InOut);
        }

        /// <summary>The Crescent Slash: from the draw, a level cut at the chest snapped to a dead stop when the blade points straight ahead. The light goes on without it.</summary>
        private static ForgeClip ScytheCrescent(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var coming = With(Reaper(V(0.04f, -0.24f, 0.02f), 20f, 16f, 6f, V(-0.2f, 0f, 0.22f), V(0.26f, 0f, -0.24f),
                V(0.52f, 1.26f, 0.2f), V(0.96f, 0f, 0.26f), BackLeftward(V(0.96f, 0f, 0.26f))), p => p.GripGap = 0.55f);
            var stopped = With(Reaper(V(0f, -0.2f, 0.1f), -10f, -16f, 8f, V(-0.2f, 0f, 0.26f), V(0.26f, 0f, -0.26f),
                V(0.04f, 1.3f, 0.6f), V(0f, 0f, 1f), V(1f, 0f, 0f)), p =>
            {
                p.GripGap = 0.55f;
                p.ElbowR = V(0.6f, -0.7f, 0f);
                p.ElbowL = V(-0.6f, -0.7f, 0f);
            });
            var clip = Attack(step, 0f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 1.4f);
            return clip.Key(0f, ScytheDrawn()).Key(hit * 0.8f, coming, Ease.In).Key(hit, stopped, Ease.Linear)
                .Key(Mathf.Lerp(done, 1f, 0.33f), With(stopped, p => p.Hips = V(0f, -0.22f, 0.12f)), Ease.Out).Key(1f, ScytheCarry(), Ease.InOut);
        }

        /// <summary>
        /// The Wheel. The blade hooked under the kneeling enemy; one slow turn on the spot, hauling, that ends in a
        /// heave upward; then the hold, sunk and still with the scythe drawn far back and the eyes up, while the body
        /// hangs; and one rising cut that meets it coming down.
        /// </summary>
        private static ForgeClip ScytheWheel(AttackStep step)
        {
            var (hit, done) = Moments(step);
            float total = step.Total;
            float hook = 0.16f / total, heave = 0.46f / total, hold = step.HoldAt / total;
            var hooked = With(Reaper(V(0f, -0.3f, 0.16f), 10f, 10f, 16f, V(-0.2f, 0f, -0.2f), V(0.2f, 0f, 0.34f),
                V(0.26f, 0.9f, 0.5f), V(0.34f, -0.36f, 0.87f), V(0.2f, -0.9f, -0.3f)), p => p.GripGap = 0.55f);
            var hauling = With(Reaper(V(0f, -0.28f, -0.08f), 0f, 0f, -14f, V(-0.1f, 0f, 0f), V(0.34f, 0.08f, -0.2f),
                V(0.2f, 1.0f, 0.56f), V(0.3f, -0.1f, 0.95f), BackLeftward(V(0.3f, 0f, 0.95f)), 0f), p =>
            {
                p.GripGap = 0.55f;
                p.HeadYaw = -30f;
                p.ElbowR = V(0.6f, -0.7f, 0f);
            });
            var heaved = With(Reaper(V(0f, 0f, 0.04f), 0f, 6f, -16f, V(-0.2f, 0f, -0.1f), V(0.2f, 0f, 0.2f),
                V(0.16f, 1.86f, 0.4f), V(0.1f, 0.84f, 0.53f), V(0f, 0.5f, -0.86f), 0f), p =>
            {
                p.ShoulderLUp = p.ShoulderRUp = 24f;
                p.HeadLean = -24f;
                p.FootLTip = 24f;
                p.FootRTip = 24f;
            });
            // The hold: sunk, the scythe far back and low on the right, perfectly still, looking up.
            var waiting = With(Reaper(V(0.08f, -0.44f, -0.12f), 46f, 30f, 4f, V(-0.22f, 0f, 0.24f), V(0.28f, 0f, -0.3f),
                V(0.54f, 0.8f, -0.26f), V(0.56f, -0.3f, -0.77f), V(0.3f, 0.9f, -0.3f), 0.8f), p =>
            {
                p.GripGap = 0.55f;
                p.HeadLean = -34f;
                p.FootRTurn = 50f;
            });
            var still = With(waiting, p => p.Hips = V(0.08f, -0.47f, -0.13f));
            var rising = With(Reaper(V(0f, -0.06f, 0.14f), 0f, -6f, -10f, V(-0.2f, 0f, -0.2f), V(0.22f, 0f, 0.36f),
                V(0.1f, 1.56f, 0.5f), V(0.2f, 0.62f, 0.76f), V(0.6f, -0.6f, -0.5f)), p =>
            {
                p.GripGap = 0.55f;
                p.HeadLean = -26f;
            });
            var top = With(Reaper(V(0f, 0.06f, 0.18f), -20f, -36f, -16f, V(-0.2f, 0.05f, -0.2f), V(0.22f, 0f, 0.36f),
                V(-0.3f, 2.0f, 0.1f), V(-0.5f, 0.84f, -0.2f), V(0.8f, 0.5f, 0.3f), 0.5f), p =>
            {
                p.ShoulderLUp = p.ShoulderRUp = 26f;
                p.FootLTip = 40f;
                p.HeadLean = -20f;
            });
            var clip = Attack(step, -4f);
            clip.Shape = ForgeClip.Impact(hit, total, 2f);
            clip.SheetMoments = new[] { hook, (hook + heave) * 0.5f, heave, hold + 0.03f, hit * 0.97f, hit, done + 0.04f, 0.9f };
            clip.Key(0f, ScytheCarry()).Key(hook, hooked, Ease.Out).Key(Mathf.Lerp(hook, heave, 0.25f), hauling, Ease.InOut)
                .Key(Mathf.Lerp(hook, heave, 0.8f), hauling, Ease.Linear).Key(heave, heaved, Ease.In).Key(hold + 0.05f, waiting, Ease.Out)
                .Key(hit * 0.96f, still, Ease.Linear).Key(hit, rising, Ease.In).Key(done, top, Ease.Out)
                .Key(Mathf.Lerp(done, 1f, 0.55f), With(top, p => p.Hips = V(0f, 0f, 0.18f)), Ease.InOut).Key(1f, ScytheCarry(), Ease.InOut);
            // One turn on the spot between the hook and the heave.
            clip.MovesRoot = true;
            clip.After = TurnAbout(t => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(hook, heave - 0.02f, t)) * -360f, Vector3.up, Vector3.zero);
            return clip;
        }
    }
}

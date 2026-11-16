using System.Collections.Generic;
using Clover.Player;
using UnityEngine;

namespace Clover.EditorTools.Forge
{
    /// <summary>
    /// The Great Hammer's clips. Not the Massive Sword's: the sword is swung from the hips with long arms, the hammer
    /// is lifted with the legs and dropped, and after each blow it stays where it fell for a moment while the body
    /// recovers round it. The right hand holds the haft 0.9 m under the head and the left the butt; the pose's blade
    /// is the haft, and the striking face looks the way the back of the weapon does not.
    /// </summary>
    public static partial class PlayerAnimations
    {
        private const float HammerGrip = 0.33f;

        private static IEnumerable<ForgeClip> HammerClips()
        {
            var combo = Weapons.GreatHammer().Combo;
            yield return HammerIdle();
            yield return HammerRun();
            yield return HammerRoll();
            yield return HammerGuard();
            yield return HammerGuardHit();
            yield return HammerGuardPerfect();
            yield return HammerCharge(combo.ChargeTime);
            yield return HammerSide(combo.Steps[0]);
            yield return HammerDrop(combo.Steps[1]);
            yield return HammerQuake(combo.Heavy);
            yield return HammerLift(combo.ChainHeavy);
            yield return HammerEarthshaker(combo.ChargedHeavy);
            yield return HammerToss(combo.Skill);
            yield return HammerAway();
            yield return HammerCatch();
            yield return HammerBell(combo.Finisher);
        }

        private static ForgePose Smith(Vector3 hips, float hipYaw, float waistYaw, float lean, Vector3 footL, Vector3 footR,
            Vector3 hand, Vector3 haft, Vector3 back, float keep = 0.8f) =>
            Two(hips, hipYaw, waistYaw, lean, footL, footR, hand, haft, back, HammerGrip, keep);

        /// <summary>A workman leaning on his tool: the head on the ground ahead of the right foot, the haft slanting up to both hands at the belt, feet wide.</summary>
        private static ForgePose HammerRest() => With(Smith(V(0f, -0.06f, -0.04f), 10f, 4f, 6f, V(-0.24f, 0f, 0.04f), V(0.24f, 0f, -0.06f),
            V(0.2f, 1.06f, 0.16f), V(0.06f, -0.84f, 0.54f), V(0f, 0.54f, 0.84f)), p =>
        {
            p.ElbowR = V(0.9f, -0.4f, -0.3f);
            p.ElbowL = V(-0.9f, -0.4f, -0.3f);
            p.FootRTurn = 30f;
        });

        // Where the side swing ends: the head dropped to the ground out on the left, the body pulled a quarter turn past it.
        private static ForgePose HammerDownLeft() => With(Smith(V(-0.04f, -0.26f, 0.06f), -40f, -30f, 16f, V(-0.22f, 0f, 0.4f), V(0.22f, 0f, -0.2f),
            V(-0.24f, 0.96f, 0.46f), V(-0.68f, -0.6f, 0.42f), V(0.5f, 0.3f, 0.8f), 0.6f), p => p.FootRTip = 24f);

        // Hung behind the back, haft down the spine, the body arched and up on the toes.
        private static ForgePose HammerHung() => With(Smith(V(0f, -0.04f, -0.12f), 6f, 0f, -18f, V(-0.2f, 0f, 0.14f), V(0.2f, 0f, -0.2f),
            V(0.04f, 1.9f, -0.12f), V(0.02f, -0.84f, -0.54f), V(0f, -0.54f, 0.84f)), p =>
        {
            p.ElbowR = V(0.5f, 0.7f, 0.5f);
            p.ElbowL = V(-0.5f, 0.7f, 0.5f);
            p.ShoulderLUp = p.ShoulderRUp = 26f;
            p.FootLTip = 22f;
            p.FootRTip = 22f;
        });

        // Heaved straight up: one line from heels to hammer.
        private static ForgePose HammerRaised() => With(Smith(V(0f, 0.04f, 0f), 0f, 0f, -4f, V(-0.2f, 0f, 0.1f), V(0.2f, 0f, -0.1f),
            V(0.05f, 2.0f, 0.04f), V(0f, 1f, -0.06f), V(0f, 0f, -1f)), p =>
        {
            p.ShoulderLUp = p.ShoulderRUp = 26f;
            p.ElbowR = V(0.8f, 0.3f, 0.5f);
            p.ElbowL = V(-0.8f, 0.3f, 0.5f);
            p.FootLTip = 30f;
            p.FootRTip = 30f;
        });

        // Dropped: a deep squat with the arms straight down the haft and the head in the ground a stride ahead.
        // The body sits back and a little to the left of the haft, so the head and the haft are seen from above.
        private static ForgePose HammerDropped() => With(Smith(V(-0.1f, -0.56f, 0.06f), -8f, -6f, 22f, V(-0.3f, 0f, 0.2f), V(0.24f, 0f, 0.1f),
            V(0.12f, 0.56f, 0.7f), V(0f, -0.36f, 0.93f), V(0f, 0.93f, 0.36f)), p =>
        {
            p.ElbowR = V(0.8f, -0.4f, 0f);
            p.ElbowL = V(-0.8f, -0.4f, 0f);
            p.HeadLean = -20f;
        });

        private static ForgeClip HammerIdle() => new ForgeClip("idle_hammer", 3f, true).From(t =>
        {
            float breath = Mathf.Sin(t * Mathf.PI * 2f);
            var p = HammerRest();
            p.Hips += V(0f, 0.014f * breath, 0f);
            p.WaistLean += 2.5f * breath;
            p.ShoulderLUp = p.ShoulderRUp = 2f + 3f * breath;
            p.HandR += V(0f, 0.008f * breath, 0f);
            p.HeadYaw += 6f * Mathf.Sin(t * Mathf.PI * 2f + 1.2f);
            return p;
        });

        private static ForgeClip HammerRun()
        {
            var clip = new ForgeClip("run_hammer", 0.64f, true) { Wind = _ => new Vector3(0f, 1.2f, -8f) };
            return clip.From(t =>
            {
                float phase = t * Mathf.PI * 2f;
                float left = Mathf.Sin(phase), right = -left;
                float leftLift = Mathf.Max(0f, Mathf.Cos(phase)), rightLift = Mathf.Max(0f, -Mathf.Cos(phase));
                float bounce = Mathf.Cos(phase * 2f);
                // On the right shoulder, head behind; the body leans left to carry it and the head rocks with each stride.
                return new ForgePose
                {
                    Hips = V(-0.03f + 0.01f * left, -0.18f + 0.04f * bounce, 0.07f),
                    HipLean = 18f,
                    HipYaw = 9f * left,
                    HipRoll = 4f + 5f * left,
                    WaistLean = 10f,
                    WaistRoll = 6f,
                    WaistYaw = -6f * left,
                    HeadLean = -22f,
                    FootL = V(-0.13f, 0.24f * leftLift, 0.42f * left - 0.06f),
                    FootR = V(0.13f, 0.24f * rightLift, 0.42f * right - 0.06f),
                    FootLTurn = 4f,
                    FootRTurn = 4f,
                    FootLTip = 28f * Mathf.Max(0f, -left) - 12f * leftLift,
                    FootRTip = 28f * Mathf.Max(0f, -right) - 12f * rightLift,
                    HandR = V(0.24f, 1.24f + 0.02f * bounce, 0.26f),
                    ElbowR = V(0.8f, -0.6f, 0f),
                    ElbowL = V(-0.8f, -0.6f, 0f),
                    AimBlade = true,
                    TwoHanded = true,
                    GripGap = HammerGrip,
                    Blade = V(0.22f, 0.36f + 0.08f * bounce, -0.9f),
                    BladeBack = V(0f, 0.9f, 0.4f),
                };
            });
        }

        /// <summary>The worst roll there is: the hammer goes ahead head first and the bearer dives after it over the left shoulder, coming up with it already in hand.</summary>
        private static ForgeClip HammerRoll()
        {
            var thrown = With(Smith(V(0f, -0.4f, 0.14f), -16f, -16f, 54f, V(-0.14f, 0f, -0.3f), V(0.14f, 0.05f, -0.1f),
                V(0.2f, 0.5f, 0.56f), V(0.04f, -0.2f, 0.98f), V(0f, 1f, 0.2f)), p => p.HeadLean = 16f);
            var tuck = With(Smith(V(0f, -0.6f, 0f), 0f, 0f, 78f, V(-0.13f, 0.2f, -0.2f), V(0.13f, 0.2f, -0.24f),
                V(0.18f, 0.46f, 0.4f), V(0.06f, -0.06f, 1f), V(0f, 1f, 0f)), p => p.HeadLean = 30f);
            var knee = With(Smith(V(0f, -0.46f, 0f), 12f, 6f, 16f, V(-0.2f, 0f, 0.26f), V(0.22f, 0f, -0.3f),
                V(0.2f, 0.8f, 0.2f), V(0.06f, -0.6f, 0.8f), V(0f, 0.8f, 0.6f)), p => p.FootRTip = 50f);
            var clip = new ForgeClip("roll_hammer", 0.42f) { Wind = t => new Vector3(0f, 0f, t < 0.8f ? -7f : 0f) };
            clip.Key(0f, HammerRest()).Key(0.2f, thrown, Ease.Out).Key(0.42f, tuck, Ease.InOut).Key(0.76f, tuck, Ease.Linear).Key(1f, knee, Ease.Out);
            clip.MovesRoot = true;
            clip.After = TurnAbout(t => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.18f, 0.82f, t)) * -360f, Vector3.forward, V(0f, 0.36f, 0f));
            return clip;
        }

        // The haft stood up before the body, the head uppermost with its flat face to the enemy, both forearms braced behind it.
        // Tipped a little forward of upright: straight up, the head is hidden behind the bearer's own from the game's camera.
        private static ForgePose HammerWall() => With(Smith(V(0f, -0.3f, -0.08f), 16f, 8f, 10f, V(-0.2f, 0f, 0.2f), V(0.22f, 0f, -0.3f),
            V(0.1f, 0.84f, 0.44f), V(0f, 0.92f, 0.4f), V(0f, 0.4f, -0.92f)), p =>
        {
            p.GripGap = -0.4f;
            p.ElbowR = V(1f, -0.3f, -0.2f);
            p.ElbowL = V(-1f, -0.2f, -0.2f);
            p.HeadLean = 8f;
        });

        private static ForgeClip HammerGuard() =>
            new ForgeClip("guard_hammer", 0.2f).Key(0f, HammerRest()).Key(0.7f, With(HammerWall(), p => p.HandR = V(0.1f, 0.74f, 0.4f)), Ease.In).Key(1f, HammerWall(), Ease.Out);

        private static ForgeClip HammerGuardHit()
        {
            // Almost nothing: the head rings and the bearer does not move.
            var rung = With(HammerWall(), p => { p.HandR = V(0.1f, 0.84f, 0.41f); p.Blade = V(0.03f, 0.93f, 0.37f); });
            return new ForgeClip("guard_hit_hammer", 0.22f).Key(0f, HammerWall()).Key(0.15f, rung, Ease.Snap).Key(0.4f, With(HammerWall(), p => p.Blade = V(-0.02f, 0.92f, 0.41f)), Ease.InOut)
                .Key(1f, HammerWall(), Ease.InOut);
        }

        private static ForgeClip HammerGuardPerfect()
        {
            // The head shoved forward into the blow, a short flat punch with the whole haft; then drawn back low to the right hip, ready for the side swing.
            var punch = With(HammerWall(), p =>
            {
                p.Hips = V(0f, -0.26f, 0.08f);
                p.HandR = V(0.1f, 1.0f, 0.7f);
                p.Blade = V(0f, 0.74f, 0.67f);
                p.WaistLean += 10f;
            });
            var drawn = Smith(V(0.04f, -0.22f, -0.04f), 40f, 30f, 6f, V(-0.2f, 0f, 0.2f), V(0.24f, 0f, -0.24f),
                V(0.46f, 0.94f, -0.1f), V(0.7f, -0.3f, -0.65f), BackLeftward(V(0.7f, 0f, -0.65f)), 0.6f);
            return new ForgeClip("guard_perfect_hammer", 0.3f).Key(0f, HammerWall()).Key(0.22f, punch, Ease.Snap).Key(0.45f, punch, Ease.Linear)
                .Key(1f, drawn, Ease.InOut);
        }

        private static ForgeClip HammerCharge(float seconds)
        {
            // Dragged back along the ground behind the right heel, then heaved up and over until it hangs behind the back.
            var dragged = Smith(V(0.02f, -0.3f, -0.06f), 34f, 30f, 12f, V(-0.2f, 0f, 0.22f), V(0.24f, 0f, -0.24f),
                V(0.44f, 0.9f, -0.16f), V(0.24f, -0.62f, -0.75f), V(0f, 0.75f, -0.62f), 0.7f);
            var clip = new ForgeClip("charge_hammer", seconds);
            clip.Shape = (t, p) =>
            {
                if (t < 0.85f) return p;
                float tremble = Mathf.Sin(t * 240f) * 0.006f;
                p.HandR += V(tremble, 0f, tremble);
                return p;
            };
            clip.SheetMoments = new[] { 0f, 0.2f, 0.4f, 0.55f, 0.7f, 0.85f, 0.95f, 1f };
            return clip.Key(0f, HammerRest()).Key(0.4f, dragged, Ease.Out).Key(0.85f, HammerHung(), Ease.InOut)
                .Key(1f, With(HammerHung(), p => p.Hips = V(0f, -0.02f, -0.14f)), Ease.InOut);
        }

        // Swung back to the right at knee height, the body turned right and the left foot off the ground.
        private static ForgePose HammerWoundRight() => With(Smith(V(0.06f, -0.24f, -0.06f), 50f, 40f, 4f, V(-0.12f, 0.16f, 0.14f), V(0.24f, 0f, -0.2f),
            V(0.5f, 0.96f, -0.14f), V(0.74f, -0.3f, -0.6f), BackLeftward(V(0.74f, 0f, -0.6f)), 0.7f), p => p.FootRTurn = 50f);

        // Level across the front with the arms long: where the head stops against what it hits.
        private static ForgePose HammerAcross() => With(Smith(V(0f, -0.26f, 0.1f), -12f, -20f, 10f, V(-0.22f, 0f, 0.42f), V(0.24f, 0f, -0.2f),
            V(0.04f, 1.0f, 0.62f), V(-0.12f, -0.08f, 1f), BackLeftward(V(-0.12f, 0f, 1f))), p =>
        {
            p.ElbowR = V(0.6f, -0.7f, 0f);
            p.ElbowL = V(-0.6f, -0.7f, 0f);
        });

        /// <summary>The side swing: the head drawn back to the right at the knee with the left foot lifted; the foot stamps and the hammer comes round level, stops against what it hits, and drops to the ground on the left.</summary>
        private static ForgeClip HammerSide(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var coming = With(Smith(V(0.03f, -0.26f, 0.02f), 24f, 18f, 6f, V(-0.2f, 0.04f, 0.36f), V(0.24f, 0f, -0.2f),
                V(0.46f, 0.98f, 0.26f), V(0.93f, -0.14f, 0.34f), BackLeftward(V(0.93f, 0f, 0.34f))), p => { });
            var rebound = With(HammerAcross(), p =>
            {
                p.Blade = V(-0.3f, -0.02f, 0.95f);
                p.HandR = V(0f, 1.0f, 0.6f);
                p.HipYaw = -22f;
                p.WaistYaw = -26f;
            });
            var clip = Attack(step, -5f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 1.5f);
            return clip.Key(0f, HammerRest()).Key(hit * 0.55f, HammerWoundRight(), Ease.Out).Key(hit * 0.82f, With(HammerWoundRight(), p => p.Hips = V(0.07f, -0.27f, -0.07f)), Ease.Linear)
                .Key(hit * 0.94f, coming, Ease.In).Key(hit, HammerAcross(), Ease.Linear).Key(done, rebound, Ease.Out)
                .Key(Mathf.Lerp(done, 1f, 0.35f), HammerDownLeft(), Ease.In).Key(1f, With(HammerDownLeft(), p => p.Hips = V(-0.04f, -0.22f, 0.06f)), Ease.InOut);
        }

        /// <summary>The drop: heaved straight up from where it lies, a long beat on the toes, and then the knees are simply let go.</summary>
        private static ForgeClip HammerDrop(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var squat = With(HammerDropped(), p => { p.Hips = V(-0.1f, -0.62f, 0.08f); p.HandR = V(0.12f, 0.5f, 0.7f); });
            var clip = Attack(step, -3f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 2f);
            return clip.Key(0f, With(HammerDownLeft(), p => p.Hips = V(-0.04f, -0.22f, 0.06f))).Key(hit * 0.5f, HammerRaised(), Ease.Out)
                .Key(hit * 0.9f, With(HammerRaised(), p => p.Hips = V(0f, 0.07f, 0f)), Ease.Linear).Key(hit, HammerDropped(), Ease.In)
                .Key(done, squat, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), squat, Ease.Linear).Key(1f, HammerRest(), Ease.InOut);
        }

        /// <summary>Quake: a step back with the hammer dragged, a stride as it is swung up and over behind, a small jump, and the landing and the blow in the same instant.</summary>
        private static ForgeClip HammerQuake(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var back = Smith(V(0f, -0.24f, -0.22f), 30f, 24f, 8f, V(-0.2f, 0f, 0.1f), V(0.24f, 0f, -0.44f),
                V(0.42f, 0.9f, -0.2f), V(0.26f, -0.62f, -0.74f), V(0f, 0.74f, -0.62f), 0.7f);
            var over = With(HammerHung(), p =>
            {
                p.Hips = V(0f, -0.06f, 0.06f);
                p.FootL = V(-0.2f, 0f, 0.36f);
                p.FootR = V(0.2f, 0f, -0.3f);
            });
            var jump = With(Smith(V(0f, 0.34f, 0.2f), 0f, 0f, -12f, V(-0.16f, 0.46f, 0f), V(0.16f, 0.52f, 0.2f),
                V(0.05f, 2.3f, 0.14f), V(0f, 0.86f, -0.5f), V(0f, -0.5f, -0.86f)), p =>
            {
                p.ShoulderLUp = p.ShoulderRUp = 28f;
                p.ElbowR = V(0.7f, 0.3f, 0.5f);
                p.ElbowL = V(-0.7f, 0.3f, 0.5f);
                p.FootLTip = 30f;
                p.FootRTip = 30f;
            });
            var landed = With(HammerDropped(), p => { p.Hips = V(-0.1f, -0.6f, 0.12f); p.FootL = V(-0.32f, 0f, 0.26f); });
            // Thrown forward over the haft by the stop, hanging on it.
            var hanging = With(landed, p =>
            {
                p.Hips = V(-0.1f, -0.5f, 0.26f);
                p.WaistLean = 34f;
                p.HipLean = 16f;
                p.HandR = V(0.12f, 0.6f, 0.74f);
            });
            var clip = Attack(step, -8f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 2.2f);
            clip.SheetMoments = new[] { 0f, hit * 0.3f, hit * 0.66f, hit * 0.9f, hit, done, Mathf.Lerp(done, 1f, 0.4f), 1f };
            return clip.Key(0f, HammerRest()).Key(hit * 0.3f, back, Ease.Out).Key(hit * 0.68f, over, Ease.InOut).Key(hit * 0.9f, jump, Ease.Out)
                .Key(hit, landed, Ease.In).Key(done, hanging, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), hanging, Ease.Linear).Key(1f, HammerRest(), Ease.InOut);
        }

        // The golfer's address: the head slid back low on the right, the body coiled over it.
        private static ForgePose HammerAddressed() => Smith(V(0.04f, -0.34f, -0.04f), 36f, 34f, 18f, V(-0.2f, 0f, 0.24f), V(0.24f, 0f, -0.22f),
            V(0.4f, 0.86f, -0.04f), V(0.44f, -0.62f, -0.65f), V(-0.3f, 0.5f, -0.8f), 0.7f);

        // The upward swing at the moment it meets the chin: arms long, the head rising ahead.
        private static ForgePose HammerRising() => With(Smith(V(0f, -0.14f, 0.2f), -4f, -10f, -6f, V(-0.2f, 0f, 0.44f), V(0.22f, 0f, -0.24f),
            V(0.06f, 1.2f, 0.62f), V(-0.04f, 0.3f, 0.95f), V(0f, -0.95f, 0.3f)), p =>
        {
            p.ElbowR = V(0.6f, -0.7f, 0f);
            p.ElbowL = V(-0.6f, -0.7f, 0f);
            p.FootRTip = 30f;
        });

        // Carried on over the left shoulder until its weight turns the bearer half round.
        private static ForgePose HammerOverLeft() => With(Smith(V(-0.02f, 0f, 0.2f), -50f, -46f, -10f, V(-0.2f, 0f, 0.44f), V(0.22f, 0f, -0.2f),
            V(-0.28f, 1.86f, 0.1f), V(-0.56f, 0.62f, -0.55f), V(0.5f, 0.78f, 0.36f), 0.5f), p =>
        {
            p.ShoulderLUp = p.ShoulderRUp = 24f;
            p.FootRTip = 40f;
        });

        /// <summary>The lift: from the ground, an upward swing through the target and on over the left shoulder, a golfer's drive.</summary>
        private static ForgeClip HammerLift(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var clip = Attack(step, -6f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 1.6f);
            return clip.Key(0f, With(HammerDownLeft(), p => p.Hips = V(-0.04f, -0.22f, 0.06f))).Key(hit * 0.6f, HammerAddressed(), Ease.Out)
                .Key(hit * 0.8f, With(HammerAddressed(), p => p.Hips = V(0.05f, -0.38f, -0.05f)), Ease.Linear).Key(hit, HammerRising(), Ease.In)
                .Key(done + 0.04f, HammerOverLeft(), Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), With(HammerOverLeft(), p => p.Hips = V(-0.02f, -0.08f, 0.2f)), Ease.InOut)
                .Key(1f, HammerRest(), Ease.InOut);
        }

        /// <summary>Earthshaker: from hanging behind the back, the body snaps from arched to folded and the head comes over in a half circle; the bearer ends on one knee with the forehead on the hands.</summary>
        private static ForgeClip HammerEarthshaker(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var over = With(HammerRaised(), p =>
            {
                p.HandR = V(0.04f, 2.04f, 0.24f);
                p.Blade = V(0f, 0.8f, 0.6f);
                p.BladeBack = V(0f, 0.6f, -0.8f);
            });
            var kneeling = With(HammerDropped(), p =>
            {
                p.Hips = V(-0.12f, -0.64f, 0.14f);
                p.FootL = V(-0.3f, 0f, -0.3f);
                p.FootR = V(0.22f, 0f, 0.3f);
                p.FootLTip = 60f;
                p.WaistLean = 30f;
                p.HeadLean = 20f;
                p.HandR = V(0.12f, 0.5f, 0.72f);
            });
            var clip = Attack(step, -6f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 2.6f);
            clip.SheetMoments = new[] { 0f, hit * 0.6f, hit, done, Mathf.Lerp(done, 1f, 0.2f), Mathf.Lerp(done, 1f, 0.5f), Mathf.Lerp(done, 1f, 0.8f), 1f };
            // Up from behind first, so the head comes over the top and never round the side.
            var rising = With(HammerHung(), p =>
            {
                p.HandR = V(0.04f, 2.06f, -0.06f);
                p.Blade = V(0f, 0.4f, -0.92f);
                p.BladeBack = V(0f, -0.92f, -0.4f);
                p.Hips = V(0f, 0f, -0.06f);
            });
            return clip.Key(0f, HammerHung()).Key(hit * 0.35f, rising, Ease.In).Key(hit * 0.7f, over, Ease.Linear).Key(hit, HammerDropped(), Ease.Linear).Key(done, kneeling, Ease.Out)
                .Key(Mathf.Lerp(done, 1f, 0.7f), kneeling, Ease.Linear).Key(1f, HammerRest(), Ease.InOut);
        }

        // Waiting for it: feet planted, the right hand up and open.
        private static ForgePose HammerWaiting() => With(Smith(V(0f, -0.14f, -0.04f), 20f, 10f, -4f, V(-0.22f, 0f, 0.2f), V(0.24f, 0f, -0.24f),
            V(0.36f, 1.74f, 0.3f), V(0f, 0.5f, -0.86f), V(0f, 0.86f, 0.5f)), p =>
        {
            p.TwoHanded = false;
            p.HandL = V(-0.3f, 1.02f, 0.24f);
            p.ElbowR = V(0.9f, -0.3f, 0f);
            p.ShoulderRUp = 16f;
        });

        /// <summary>The Spinning Hammer Toss: two turns leaning back against its pull with the arms straight, the release with both hands flung open, two staggering steps, and the hand raised for it.</summary>
        private static ForgeClip HammerToss(AttackStep step)
        {
            var (hit, done) = Moments(step);
            float total = step.Total;
            float release = (step.Windup + step.Shot.ReleaseAfter) / total;
            var turning = With(Smith(V(0f, -0.22f, -0.16f), 0f, 0f, -18f, V(-0.08f, 0f, 0.04f), V(0.34f, 0.06f, -0.2f),
                V(0.12f, 1.14f, 0.6f), V(0.04f, 0.02f, 1f), BackLeftward(V(0.04f, 0f, 1f)), 0f), p =>
            {
                p.ElbowR = V(0.6f, -0.7f, 0f);
                p.ElbowL = V(-0.6f, -0.7f, 0f);
                p.HeadYaw = -24f;
            });
            // The second turn climbs: the head rises from the hip's height to the shoulder's as the lean deepens.
            var faster = With(turning, p =>
            {
                p.Hips = V(0f, -0.3f, -0.26f);
                p.WaistLean = -18f;
                p.HipLean = -10f;
                p.HandR = V(0.12f, 1.3f, 0.6f);
                p.Blade = V(0.04f, 0.26f, 1f);
                p.FootR = V(0.36f, 0.1f, -0.26f);
            });
            var flung = With(turning, p =>
            {
                p.TwoHanded = false;
                p.Hips = V(0f, -0.16f, 0.06f);
                p.WaistLean = 8f;
                p.HandR = V(0.3f, 1.66f, 0.72f);
                p.HandL = V(-0.3f, 1.6f, 0.7f);
                p.Blade = V(0f, 0.4f, 1f);
                p.ShoulderLUp = p.ShoulderRUp = 18f;
                p.FootLTip = 20f;
                p.HeadYaw = 0f;
            });
            var stagger = With(flung, p =>
            {
                p.Hips = V(0.04f, -0.2f, -0.24f);
                p.FootL = V(-0.2f, 0f, -0.1f);
                p.FootR = V(0.26f, 0f, -0.5f);
                p.HandR = V(0.4f, 1.2f, 0.4f);
                p.HandL = V(-0.4f, 1.1f, 0.3f);
                p.WaistLean = -8f;
            });
            var clip = Attack(step, 0f);
            clip.Shape = null;
            clip.SheetMoments = new[] { 0f, hit * 0.6f, hit, Mathf.Lerp(hit, release, 0.5f), release, release + 0.06f, Mathf.Lerp(release, 1f, 0.5f), 1f };
            clip.Key(0f, HammerHung()).Key(hit * 0.5f, turning, Ease.Out).Key(release - 0.03f, faster, Ease.Linear).Key(release + 0.02f, flung, Ease.Snap)
                .Key(Mathf.Lerp(release, 1f, 0.45f), stagger, Ease.Out).Key(1f, HammerWaiting(), Ease.InOut);
            float from = hit * 0.3f;
            clip.MovesRoot = true;
            // Two whole turns, slow into the first and fast out of the second.
            clip.After = TurnAbout(t => { float u = Mathf.InverseLerp(from, release, t); return u * u * (1.6f - 0.6f * u) * -720f; }, Vector3.up, Vector3.zero);
            return clip;
        }

        private static ForgeClip HammerAway() => new ForgeClip("away_hammer", 0.2f).Key(0f, HammerWaiting()).Key(1f, HammerWaiting(), Ease.Linear);

        private static ForgeClip HammerCatch()
        {
            // It arrives into the raised hand and drives the arm back to the shoulder.
            var driven = With(HammerWaiting(), p =>
            {
                p.HandR = V(0.3f, 1.44f, -0.04f);
                p.Blade = V(0.1f, 0.7f, -0.7f);
                p.WaistYaw = 34f;
                p.WaistLean = -10f;
                p.ShoulderRUp = 6f;
            });
            return new ForgeClip("catch_hammer", 0.28f).Key(0f, HammerWaiting()).Key(0.3f, driven, Ease.Snap).Key(1f, HammerRest(), Ease.InOut);
        }

        /// <summary>
        /// The Bell. The lift under the chin sends the enemy up; the bearer turns their back on where it will come
        /// down, lets the head of the hammer to the ground, and winds up the side swing slowly, coiled and still;
        /// then turns into it and meets it in the air.
        /// </summary>
        private static ForgeClip HammerBell(AttackStep step)
        {
            var (hit, done) = Moments(step);
            float total = step.Total;
            float lift = 0.16f / total, turned = 0.3f / total, hold = step.HoldAt / total;
            // Back to the enemy, the head let down on the ground on the far side.
            var away = With(Smith(V(0f, -0.14f, 0f), 120f, 30f, 4f, V(-0.2f, 0f, 0.1f), V(0.22f, 0f, -0.16f),
                V(0.44f, 0.98f, -0.3f), V(0.3f, -0.74f, -0.6f), V(0f, 0.6f, -0.74f), 0f), p =>
            {
                p.HeadYaw = 0f;
                p.HeadLean = 18f;
                p.FootLTurn = -60f;
                p.FootRTurn = 110f;
            });
            // Coiled: the hammer drawn back level on the right, the back still half turned, perfectly still.
            var coiled = With(HammerWoundRight(), p =>
            {
                p.Hips = V(0.08f, -0.34f, -0.08f);
                p.HipYaw = 84f;
                p.WaistYaw = 44f;
                p.HeadYaw = -40f;
                p.HeadLean = 14f;
                p.FootL = V(-0.16f, 0f, 0.2f);
                p.HandR = V(0.46f, 1.16f, -0.36f);
                p.Blade = V(0.62f, 0.08f, -0.78f);
                p.BladeBack = BackLeftward(V(0.62f, 0f, -0.78f));
                p.FootRTip = 20f;
            });
            var still = With(coiled, p => { p.Hips = V(0.09f, -0.39f, -0.1f); p.WaistYaw = 50f; });
            // Met in the air at chest height.
            var meeting = With(HammerAcross(), p =>
            {
                p.HandR = V(0.04f, 1.34f, 0.66f);
                p.Blade = V(-0.12f, 0.14f, 1f);
                p.Hips = V(0f, -0.14f, 0.2f);
                p.FootL = V(-0.22f, 0f, 0.56f);
                p.FootRTip = 34f;
                p.WaistLean = 14f;
            });
            var through = With(meeting, p =>
            {
                p.Blade = V(-0.7f, 0.1f, 0.7f);
                p.HandR = V(-0.2f, 1.3f, 0.5f);
                p.HipYaw = -40f;
                p.WaistYaw = -40f;
            });
            var clip = Attack(step, -4f);
            clip.Shape = ForgeClip.Impact(hit, total, 2.4f);
            clip.SheetMoments = new[] { lift, (lift + turned) * 0.5f, turned, hold + 0.03f, hit * 0.96f, hit, done + 0.04f, 0.9f };
            return clip.Key(0f, HammerRest()).Key(lift * 0.5f, HammerAddressed(), Ease.Out).Key(lift, HammerRising(), Ease.In)
                .Key(Mathf.Lerp(lift, turned, 0.45f), HammerOverLeft(), Ease.Out).Key(turned, away, Ease.InOut).Key(hold + 0.04f, coiled, Ease.InOut)
                .Key(hit * 0.95f, still, Ease.Linear).Key(hit, meeting, Ease.In).Key(done + 0.03f, through, Ease.Out)
                .Key(Mathf.Lerp(done, 1f, 0.45f), HammerDownLeft(), Ease.In).Key(Mathf.Lerp(done, 1f, 0.75f), HammerDownLeft(), Ease.Linear).Key(1f, HammerRest(), Ease.InOut);
        }
    }
}

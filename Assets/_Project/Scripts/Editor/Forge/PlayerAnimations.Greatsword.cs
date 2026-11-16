using System;
using System.Collections.Generic;
using Clover.Player;
using UnityEngine;

namespace Clover.EditorTools.Forge
{
    /// <summary>
    /// The Massive Sword's clips. Nothing here comes from the wrists: every swing starts in the legs, the hips fire,
    /// the chest follows and the arms are dragged round last, almost straight, with the blade behind them all. The
    /// follow-through of each sweep is the wind-up of the next, so the chain reads as one weight changing direction.
    /// Seen from above the blade is laid flat in every sweep (edge leading), which is what makes it a slab on screen.
    /// </summary>
    public static partial class PlayerAnimations
    {
        private static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);

        // Which way the back of a level blade faces when its edge leads a cut travelling right to left (as seen by
        // the swinger), and left to right.
        private static Vector3 BackLeftward(Vector3 blade) => new Vector3(blade.z, 0f, -blade.x);
        private static Vector3 BackRightward(Vector3 blade) => new Vector3(-blade.z, 0f, blade.x);

        // A pose with a long weapon in both hands: where the body is and how it is wound, where the feet stand, where
        // the right hand is, which way the weapon points and which way its back faces.
        private static ForgePose Two(Vector3 hips, float hipYaw, float waistYaw, float lean, Vector3 footL, Vector3 footR,
            Vector3 hand, Vector3 blade, Vector3 back, float gap = 0.26f, float keep = 0.8f)
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
                FootLTurn = 28f,
                FootRTurn = 16f,
                HandR = hand,
                ElbowR = new Vector3(0.9f, -0.5f, -0.3f),
                ElbowL = new Vector3(-0.9f, -0.5f, -0.3f),
                AimBlade = true,
                TwoHanded = true,
                GripGap = gap,
                Blade = blade,
                BladeBack = back,
            }, keep);
        }

        private static ForgePose With(ForgePose pose, Action<ForgePose> change)
        {
            var copy = pose.Copy();
            change(copy);
            return copy;
        }

        private static Func<float, ForgePose, ForgePose> Both(Func<float, ForgePose, ForgePose> first, Func<float, ForgePose, ForgePose> second) =>
            (t, pose) => second(t, first(t, pose));

        // Turns the whole body about a point: a full turn on the spot, a roll along the ground, a walk round an enemy.
        private static Action<ForgeRig, float> TurnAbout(Func<float, float> degreesAt, Vector3 axis, Vector3 pivot) => (rig, t) =>
        {
            var root = rig["root"];
            var spin = Quaternion.AngleAxis(degreesAt(t), axis);
            root.Transform.rotation = spin * root.RestRotation;
            root.Transform.position = pivot - spin * pivot + spin * root.RestPosition;
        };

        private static IEnumerable<ForgeClip> GreatswordClips()
        {
            var combo = Weapons.MassiveSword().Combo;
            yield return GswordIdle();
            yield return GswordRun();
            yield return GswordRoll();
            yield return GswordGuard();
            yield return GswordGuardHit();
            yield return GswordGuardPerfect();
            yield return GswordCharge(combo.ChargeTime);
            yield return GswordSweep(combo.Steps[0]);
            yield return GswordReturn(combo.Steps[1]);
            yield return GswordTurn(combo.Steps[2]);
            yield return GswordOverhead(combo.Heavy);
            yield return GswordRising(combo.ChainHeavy);
            yield return GswordLeap(combo.ChargedHeavy);
            yield return GswordSpike(combo.Skill);
            yield return GswordHeadsman(combo.Finisher);
        }

        // ------------------------------------------------------------------ stances

        /// <summary>The carry: feet wide, left foot ahead, hips sunk, the slab flat on the right shoulder with its point behind and above.</summary>
        private static ForgePose GswordCarry() => With(Two(V(0f, -0.12f, 0f), 18f, 6f, 6f, V(-0.24f, 0f, 0.2f), V(0.26f, 0f, -0.22f),
            V(0.24f, 1.36f, 0.14f), V(0.3f, 0.4f, -0.87f), V(-1f, 0.15f, 0f)), p =>
        {
            p.FootRTurn = 40f;
            p.ElbowR = V(0.8f, -0.6f, 0.2f);
            p.ElbowL = V(-0.6f, -0.8f, 0.2f);
        });

        // Wound right with the blade behind: where the first sweep ends and the second begins.
        private static ForgePose GswordWoundRight() => With(Two(V(-0.06f, -0.2f, 0.12f), 34f, 52f, 4f, V(-0.16f, 0f, 0.46f), V(0.24f, 0f, -0.3f),
            V(0.5f, 0.98f, 0.02f), V(0.72f, -0.06f, -0.69f), BackLeftward(V(0.72f, 0f, -0.69f)), 0.26f, 0.6f), p =>
        {
            p.FootRTip = 22f;
            p.HipRoll = -5f;
        });

        // Wound left with the blade behind the left shoulder: where the second sweep ends and the full turn begins.
        private static ForgePose GswordWoundLeft() => With(Two(V(0.06f, -0.18f, 0.14f), -40f, -58f, 2f, V(-0.22f, 0f, -0.28f), V(0.18f, 0f, 0.46f),
            V(-0.46f, 1.36f, 0.06f), V(-0.7f, 0.24f, -0.67f), BackRightward(V(-0.7f, 0f, -0.67f)), 0.26f, 0.6f), p =>
        {
            p.FootLTip = 24f;
            p.HipRoll = 5f;
        });

        private static ForgeClip GswordIdle() => new ForgeClip("idle_gsword", 3f, true).From(t =>
        {
            float breath = Mathf.Sin(t * Mathf.PI * 2f);
            // Once a loop the weight goes from one leg to the other and back.
            float shift = Mathf.Sin(t * Mathf.PI * 2f + 1.3f);
            var p = GswordCarry();
            p.Hips += V(0.04f * shift, 0.015f * breath, 0f);
            p.HipRoll = 3f * shift;
            p.WaistLean += 2.5f * breath;
            p.ShoulderLUp = p.ShoulderRUp = 2f + 3f * breath;
            p.HandR += V(0f, 0.012f * breath, 0f);
            // The point rises and falls four centimetres with the breath.
            p.Blade = V(0.3f, 0.4f + 0.03f * breath, -0.87f);
            p.HeadYaw += 8f * shift;
            return p;
        });

        private static ForgeClip GswordRun()
        {
            var clip = new ForgeClip("run_gsword", 0.62f, true) { Wind = _ => new Vector3(0f, 1.2f, -9f) };
            return clip.From(t =>
            {
                float phase = t * Mathf.PI * 2f;
                float left = Mathf.Sin(phase), right = -left;
                float leftLift = Mathf.Max(0f, Mathf.Cos(phase)), rightLift = Mathf.Max(0f, -Mathf.Cos(phase));
                float bounce = Mathf.Cos(phase * 2f);
                // The load stops the shoulders turning: the legs work under a bar that hardly moves.
                return new ForgePose
                {
                    Hips = V(0.01f * left, -0.17f + 0.045f * bounce, 0.08f),
                    HipLean = 20f,
                    HipYaw = 10f * left,
                    HipRoll = 5f * left,
                    WaistLean = 12f,
                    WaistYaw = -8f * left,
                    HeadLean = -24f,
                    FootL = V(-0.13f, 0.26f * leftLift, 0.46f * left - 0.06f),
                    FootR = V(0.13f, 0.26f * rightLift, 0.46f * right - 0.06f),
                    FootLTurn = 4f,
                    FootRTurn = 4f,
                    FootLTip = 30f * Mathf.Max(0f, -left) - 12f * leftLift,
                    FootRTip = 30f * Mathf.Max(0f, -right) - 12f * rightLift,
                    HandR = V(0.2f, 1.18f + 0.02f * bounce, 0.3f),
                    ElbowR = V(0.8f, -0.6f, 0f),
                    ElbowL = V(-0.8f, -0.6f, 0f),
                    AimBlade = true,
                    TwoHanded = true,
                    GripGap = 0.26f,
                    // The point bounces behind with each stride.
                    Blade = V(0.24f, 0.5f + 0.07f * bounce, -0.83f),
                    BladeBack = V(-1f, 0.2f, 0f),
                };
            });
        }

        /// <summary>Not a somersault: a low dive onto the left shoulder with the sword hugged along the arm, one turn about the body's own length, up onto a knee.</summary>
        private static ForgeClip GswordRoll()
        {
            var dive = With(Two(V(0f, -0.42f, 0.1f), -20f, -20f, 60f, V(-0.14f, 0f, -0.3f), V(0.14f, 0.05f, -0.1f),
                V(0.22f, 0.62f, 0.3f), V(0.15f, 0.12f, -0.98f), V(0f, 1f, 0.1f)), p => p.HeadLean = 20f);
            var tuck = With(Two(V(0f, -0.6f, 0f), 0f, 0f, 78f, V(-0.13f, 0.2f, -0.2f), V(0.13f, 0.2f, -0.24f),
                V(0.18f, 0.5f, 0.26f), V(0.1f, 0.02f, -1f), V(0f, 1f, 0f)), p => p.HeadLean = 30f);
            var knee = With(Two(V(0f, -0.46f, 0f), 14f, 6f, 16f, V(-0.2f, 0f, 0.26f), V(0.22f, 0f, -0.3f),
                V(0.24f, 1.02f, 0.16f), V(0.3f, 0.4f, -0.87f), V(-1f, 0.15f, 0f)), p => p.FootRTip = 50f);
            var clip = new ForgeClip("roll_gsword", 0.4f) { Wind = t => new Vector3(0f, 0f, t < 0.8f ? -8f : 0f) };
            clip.Key(0f, GswordCarry()).Key(0.18f, dive, Ease.Out).Key(0.4f, tuck, Ease.InOut).Key(0.74f, tuck, Ease.Linear).Key(1f, knee, Ease.Out);
            clip.MovesRoot = true;
            clip.After = TurnAbout(t => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(0.16f, 0.8f, t)) * -360f, Vector3.forward, V(0f, 0.36f, 0f));
            return clip;
        }

        // The wall: the blade swung down point first into the ground in front, flat to the enemy, the shoulder behind
        // it. It is planted on a slant, grip at the right shoulder and point at the left foot's side: stood straight
        // up it hides behind the bearer from the game's camera; slanted it is a bar across the whole front.
        private static ForgePose GswordWall() => With(Two(V(0f, -0.3f, -0.1f), 24f, 14f, 14f, V(-0.16f, 0f, 0.18f), V(0.22f, 0f, -0.5f),
            V(0.34f, 1.1f, 0.44f), V(-0.64f, -0.72f, 0.26f), V(0.75f, -0.66f, 0f)), p =>
        {
            p.ShoulderLForward = 18f;
            p.HeadLean = 12f;
            p.ElbowR = V(1f, -0.2f, -0.2f);
            p.ElbowL = V(-1f, -0.2f, 0.2f);
        });

        private static ForgeClip GswordGuard() =>
            new ForgeClip("guard_gsword", 0.16f).Key(0f, GswordCarry())
                .Key(0.75f, With(GswordWall(), p => { p.Hips = V(0f, -0.36f, -0.1f); p.HandR = V(0.34f, 1.02f, 0.44f); }), Ease.In)
                .Key(1f, GswordWall(), Ease.Out);

        private static ForgeClip GswordGuardHit()
        {
            // The whole wall is driven back and returns; the head ducks behind the guard block.
            var driven = With(GswordWall(), p =>
            {
                p.Hips = V(0f, -0.34f, -0.2f);
                p.HandR = V(0.34f, 1.08f, 0.32f);
                p.Blade = V(-0.64f, -0.74f, 0.14f);
                p.WaistLean -= 8f;
                p.HeadLean = 26f;
                p.ShoulderLUp = p.ShoulderRUp = 12f;
            });
            return new ForgeClip("guard_hit_gsword", 0.22f).Key(0f, GswordWall()).Key(0.25f, driven, Ease.Snap).Key(1f, GswordWall(), Ease.InOut);
        }

        private static ForgeClip GswordGuardPerfect()
        {
            // The flat is punched into the blow with the shoulder, then the point is torn out and the sword comes up to swing.
            var barge = With(GswordWall(), p =>
            {
                p.Hips = V(0f, -0.26f, 0.08f);
                p.HandR = V(0.32f, 1.16f, 0.62f);
                p.Blade = V(-0.62f, -0.7f, 0.36f);
                p.WaistLean += 10f;
                p.ShoulderLForward = 30f;
            });
            var torn = Two(V(0f, -0.14f, 0f), 20f, 20f, 2f, V(-0.2f, 0f, 0.2f), V(0.24f, 0f, -0.3f),
                V(0.3f, 1.5f, 0.2f), V(0.3f, 0.75f, -0.58f), V(-1f, 0.15f, 0f));
            return new ForgeClip("guard_perfect_gsword", 0.3f).Key(0f, GswordWall()).Key(0.22f, barge, Ease.Snap).Key(0.45f, barge, Ease.Linear)
                .Key(0.8f, torn, Ease.InOut).Key(1f, GswordCarry(), Ease.Out);
        }

        // The blade stood up over the head, both arms straight, point at the sky; then tipped back behind the shoulders, chest open.
        private static ForgePose GswordRaised() => With(Two(V(0f, -0.2f, -0.04f), 8f, 4f, -4f, V(-0.2f, 0f, 0.22f), V(0.22f, 0f, -0.26f),
            V(0.06f, 1.92f, 0.06f), V(0f, 1f, 0.02f), V(1f, 0f, 0f)), p =>
        {
            p.ShoulderLUp = p.ShoulderRUp = 24f;
            p.ElbowR = V(0.8f, 0.3f, 0.5f);
            p.ElbowL = V(-0.8f, 0.3f, 0.5f);
        });

        private static ForgePose GswordTipped() => With(GswordRaised(), p =>
        {
            p.Hips = V(0f, -0.28f, -0.08f);
            p.WaistLean = -16f;
            p.HipLean = -4f;
            p.HandR = V(0.06f, 1.86f, -0.14f);
            p.Blade = V(0.04f, -0.12f, -1f);
            p.BladeBack = V(1f, 0f, 0f);
            p.HeadLean = 8f;
        });

        private static ForgeClip GswordCharge(float seconds)
        {
            var clip = new ForgeClip("charge_gsword", seconds);
            // Held at the end of the clip, so the tremble of the arms is written into its last stretch.
            clip.Shape = (t, p) =>
            {
                if (t < 0.8f) return p;
                float tremble = Mathf.Sin(t * 260f) * 0.006f;
                p.HandR += V(tremble, 0f, tremble);
                return p;
            };
            clip.SheetMoments = new[] { 0f, 0.2f, 0.4f, 0.6f, 0.75f, 0.85f, 0.95f, 1f };
            return clip.Key(0f, GswordCarry()).Key(0.7f, GswordRaised(), Ease.InOut).Key(1f, GswordTipped(), Ease.InOut);
        }

        // ------------------------------------------------------------------ the three sweeps

        /// <summary>The first sweep, left to right: dropped off the shoulder behind the left hip, then the hips fire and the arms are dragged round.</summary>
        private static ForgeClip GswordSweep(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var wound = With(Two(V(0.04f, -0.26f, -0.1f), -40f, -52f, 10f, V(-0.22f, 0f, -0.24f), V(0.22f, 0f, 0.12f),
                V(-0.4f, 0.86f, -0.08f), V(-0.58f, -0.22f, -0.78f), BackRightward(V(-0.58f, 0f, -0.78f)), 0.26f, 0.9f), p => p.FootLTurn = 44f);
            var sunk = With(wound, p =>
            {
                p.Hips = V(0.05f, -0.32f, -0.13f);
                p.WaistYaw = -60f;
                p.HandR = V(-0.44f, 0.82f, -0.12f);
                Looking(p, 0.9f);
            });
            // The hips have already turned; the blade is only now coming past the left side.
            var dragged = With(Two(V(0f, -0.28f, 0.04f), -4f, -30f, 12f, V(-0.2f, 0.06f, 0.1f), V(0.22f, 0f, -0.1f),
                V(-0.5f, 0.92f, 0.3f), V(-0.95f, -0.04f, 0.3f), BackRightward(V(-0.95f, 0f, 0.3f))), p => { });
            var contact = With(Two(V(-0.02f, -0.26f, 0.16f), 22f, 6f, 10f, V(-0.16f, 0f, 0.46f), V(0.24f, 0f, -0.3f),
                V(0.04f, 0.98f, 0.6f), V(0.08f, -0.02f, 1f), BackRightward(V(0.08f, 0f, 1f))), p =>
            {
                p.ElbowR = V(0.6f, -0.7f, 0f);
                p.ElbowL = V(-0.6f, -0.7f, 0f);
            });
            // The blade carries on round and takes the chest with it; the body leans away to balance it.
            var through = With(GswordWoundRight(), p =>
            {
                p.Hips = V(-0.1f, -0.22f, 0.14f);
                p.WaistYaw = 62f;
                p.HandR = V(0.56f, 1.02f, 0.08f);
                p.Blade = V(0.8f, 0.04f, -0.6f);
                p.BladeBack = BackRightward(V(0.8f, 0f, -0.6f));
                p.HipRoll = -9f;
                Looking(p, 0.6f);
            });
            var clip = Attack(step, -6f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 1.3f);
            return clip.Key(0f, GswordCarry()).Key(hit * 0.55f, wound, Ease.Out).Key(hit * 0.86f, sunk, Ease.InOut)
                .Key(hit * 0.95f, dragged, Ease.In).Key(hit, contact, Ease.Linear)
                .Key(done, through, Ease.Out).Key(1f, With(GswordWoundRight(), p => p.BladeBack = BackRightward(V(0.72f, 0f, -0.69f))), Ease.InOut);
        }

        /// <summary>The return, right to left at chest height: already wound, only a sink and a roll of the wrists, then the same swing mirrored.</summary>
        private static ForgeClip GswordReturn(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var start = With(GswordWoundRight(), p => p.BladeBack = BackRightward(V(0.72f, 0f, -0.69f)));
            var sunk = With(GswordWoundRight(), p =>
            {
                p.Hips = V(-0.07f, -0.3f, 0.1f);
                p.WaistYaw = 60f;
                p.HandR = V(0.54f, 1.08f, -0.04f);
                Looking(p, 0.6f);
            });
            var dragged = Two(V(-0.02f, -0.28f, 0.14f), 6f, 30f, 10f, V(-0.16f, 0f, 0.3f), V(0.22f, 0.06f, 0.06f),
                V(0.52f, 1.22f, 0.32f), V(0.95f, 0f, 0.32f), BackLeftward(V(0.95f, 0f, 0.32f)));
            var contact = With(Two(V(0.02f, -0.24f, 0.2f), -20f, -8f, 10f, V(-0.22f, 0f, -0.28f), V(0.18f, 0f, 0.46f),
                V(-0.02f, 1.3f, 0.6f), V(-0.08f, 0.02f, 1f), BackLeftward(V(-0.08f, 0f, 1f))), p =>
            {
                p.ElbowR = V(0.6f, -0.7f, 0f);
                p.ElbowL = V(-0.6f, -0.7f, 0f);
            });
            var through = With(GswordWoundLeft(), p =>
            {
                p.Hips = V(0.1f, -0.2f, 0.16f);
                p.WaistYaw = -66f;
                p.HandR = V(-0.5f, 1.42f, 0.1f);
                p.Blade = V(-0.78f, 0.2f, -0.6f);
                p.BladeBack = BackLeftward(V(-0.78f, 0f, -0.6f));
                p.HipRoll = 9f;
                Looking(p, 0.6f);
            });
            var clip = Attack(step, -6f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 1.3f);
            return clip.Key(0f, start).Key(hit * 0.8f, sunk, Ease.InOut).Key(hit * 0.93f, dragged, Ease.In).Key(hit, contact, Ease.Linear)
                .Key(done, through, Ease.Out).Key(1f, With(GswordWoundLeft(), p => p.BladeBack = BackLeftward(V(-0.7f, 0f, -0.67f))), Ease.InOut);
        }

        /// <summary>The full turn: the body goes round with the blade at arm's length, the right leg out as a counterweight, and is pulled on down until the point bites.</summary>
        private static ForgeClip GswordTurn(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var start = With(GswordWoundLeft(), p => p.BladeBack = BackLeftward(V(-0.7f, 0f, -0.67f)));
            // One quick drop with the back half turned and the head snapped round over the left shoulder.
            var drop = With(start, p =>
            {
                p.Hips = V(0.04f, -0.36f, 0.1f);
                p.HandR = V(-0.4f, 1.1f, 0.12f);
                p.Blade = V(-0.82f, 0f, -0.57f);
                p.BladeBack = BackRightward(V(-0.82f, 0f, -0.57f));
                p.HeadYaw = 84f;
            });
            // In the turn: on the left foot, the blade level at the waist and far out, the right leg flung wide behind.
            var whirl = With(Two(V(-0.04f, -0.24f, 0.02f), 10f, 0f, -6f, V(-0.04f, 0f, 0.02f), V(0.5f, 0.2f, -0.38f),
                V(0.14f, 1.06f, 0.58f), V(0.3f, -0.02f, 0.95f), BackRightward(V(0.3f, 0f, 0.95f)), 0.26f, 0f), p =>
            {
                p.ElbowR = V(0.6f, -0.7f, 0f);
                p.ElbowL = V(-0.6f, -0.7f, 0f);
                p.FootRTip = 30f;
                p.KneeR = V(0.6f, 0f, 0.6f);
                p.HeadYaw = 30f;
                p.WaistRoll = 8f;
            });
            // Pulled round another quarter and down: both knees bent, the point in the ground at the right side.
            var bitten = With(Two(V(0.04f, -0.46f, 0.06f), 46f, 34f, 22f, V(-0.2f, 0f, 0.3f), V(0.26f, 0f, -0.24f),
                V(0.46f, 0.76f, 0.2f), V(0.6f, -0.52f, 0.6f), V(-0.6f, 0.2f, 0.75f), 0.26f, 0.5f), p =>
            {
                p.HeadLean = 12f;
                p.FootRTip = 30f;
            });
            var breathing = With(bitten, p =>
            {
                p.Hips = V(0.04f, -0.4f, 0.04f);
                p.HandR = V(0.46f, 0.84f, 0.18f);
                p.WaistLean += 6f;
            });
            var clip = Attack(step, -4f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 1.5f);
            clip.SheetMoments = new[] { 0f, hit * 0.7f, hit, (hit + done) * 0.5f, done, done + 0.06f, Mathf.Lerp(done, 1f, 0.5f), 1f };
            clip.Key(0f, start).Key(hit * 0.55f, drop, Ease.Out).Key(hit, whirl, Ease.In).Key(done, whirl, Ease.Linear)
                .Key(done + 0.1f, bitten, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.6f), breathing, Ease.InOut).Key(1f, GswordCarry(), Ease.InOut);
            // The turn itself: the whole body once round to the right, fastest as the blow counts.
            float from = hit * 0.5f, to = done + 0.06f;
            clip.MovesRoot = true;
            clip.After = TurnAbout(t => Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(from, to, t)) * 360f, Vector3.up, V(-0.04f, 0f, 0.02f));
            return clip;
        }

        // ------------------------------------------------------------------ the heavies

        // The blade hung down the spine: elbows by the ears, chest arched back, the weight on the back foot.
        private static ForgePose GswordHung() => With(Two(V(0f, -0.08f, -0.16f), 6f, 0f, -18f, V(-0.2f, 0f, -0.3f), V(0.2f, 0f, 0.14f),
            V(0.04f, 1.9f, -0.14f), V(0.02f, -0.82f, -0.57f), V(1f, 0f, 0f)), p =>
        {
            p.ElbowR = V(0.5f, 0.7f, 0.5f);
            p.ElbowL = V(-0.5f, 0.7f, 0.5f);
            p.ShoulderLUp = p.ShoulderRUp = 26f;
            p.FootRTip = 14f;
        });

        // The blade flat along the ground straight ahead and the whole back folded over it.
        // The body is sunk behind the blade and a little to its left, not folded over it: from the game's camera a
        // bearer bent over the blade hides the whole of it under his own hat.
        private static ForgePose GswordLanded() => With(Two(V(-0.12f, -0.5f, 0.1f), -12f, -8f, 24f, V(-0.26f, 0f, -0.42f), V(0.16f, 0f, 0.56f),
            V(0.26f, 0.64f, 0.68f), V(0.06f, -0.27f, 0.96f), V(1f, 0f, 0f)), p =>
        {
            p.FootLTip = 30f;
            p.ElbowR = V(0.8f, -0.4f, 0f);
            p.ElbowL = V(-0.8f, -0.4f, 0f);
        });

        private static ForgePose GswordBuried() => With(GswordLanded(), p =>
        {
            p.Hips = V(-0.13f, -0.58f, 0.14f);
            p.WaistLean = 24f;
            p.HipLean = 12f;
            p.HandR = V(0.26f, 0.44f, 0.72f);
            p.Blade = V(0.06f, -0.17f, 0.98f);
            p.HeadLean = 22f;
        });

        // The wrench that heaves it out again.
        private static ForgePose GswordHeave() => Two(V(0f, -0.22f, 0.12f), 14f, 16f, -4f, V(-0.2f, 0f, -0.2f), V(0.2f, 0f, 0.4f),
            V(0.2f, 1.1f, 0.36f), V(0.26f, 0.5f, 0.83f), V(-0.9f, 0.3f, 0.2f));

        /// <summary>The overhead: up off the shoulder until it hangs down the spine, a long held beat, then the whole back folds and it buries itself.</summary>
        private static ForgeClip GswordOverhead(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var held = With(GswordHung(), p =>
            {
                p.Hips = V(0f, -0.14f, -0.2f);
                p.WaistLean = -15f;
                p.HandR = V(0.04f, 1.86f, -0.2f);
            });
            var over = With(Two(V(0f, -0.1f, 0.06f), 0f, 0f, 6f, V(-0.2f, 0f, -0.34f), V(0.2f, 0.1f, 0.4f),
                V(0.02f, 1.96f, 0.22f), V(0f, 0.8f, 0.6f), V(1f, 0f, 0f)), p =>
            {
                p.ShoulderLUp = p.ShoulderRUp = 26f;
                p.ElbowR = V(0.7f, 0.3f, 0.5f);
                p.ElbowL = V(-0.7f, 0.3f, 0.5f);
            });
            var clip = Attack(step, -7f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 1.8f);
            return clip.Key(0f, GswordCarry()).Key(hit * 0.45f, GswordHung(), Ease.Out).Key(hit * 0.86f, held, Ease.InOut)
                .Key(hit * 0.95f, over, Ease.In).Key(hit, GswordLanded(), Ease.Linear).Key(done, GswordBuried(), Ease.Out)
                .Key(Mathf.Lerp(done, 1f, 0.4f), GswordBuried(), Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.75f), GswordHeave(), Ease.InOut)
                .Key(1f, GswordCarry(), Ease.InOut);
        }

        /// <summary>The rising cut that ends a chain: the point let fall behind the heel, dragged forward along the ground, and ripped up from hip to shoulder as the legs straighten.</summary>
        private static ForgeClip GswordRising(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var fallen = With(Two(V(-0.04f, -0.34f, -0.04f), 40f, 44f, 14f, V(-0.2f, 0f, 0.2f), V(0.24f, 0f, -0.26f),
                V(0.44f, 0.8f, -0.08f), V(0.36f, -0.56f, -0.75f), V(0.9f, 0f, 0.43f), 0.26f, 0.7f), p => { });
            var dragging = Two(V(-0.02f, -0.4f, 0.1f), 30f, 30f, 20f, V(-0.2f, 0f, 0.32f), V(0.24f, 0f, -0.3f),
                V(0.42f, 0.7f, 0.3f), V(0.34f, -0.74f, -0.2f), V(0.9f, 0f, 0.43f), 0.26f, 0.7f);
            var ripped = With(Two(V(0f, -0.1f, 0.24f), 0f, -10f, -4f, V(-0.2f, 0f, -0.3f), V(0.2f, 0f, 0.5f),
                V(0.08f, 1.24f, 0.62f), V(0.12f, 0.16f, 0.98f), V(0.6f, -0.8f, 0f)), p =>
            {
                p.ElbowR = V(0.6f, -0.7f, 0f);
                p.ElbowL = V(-0.6f, -0.7f, 0f);
            });
            // Finished high over the left shoulder, stretched tall, both feet a hand's breadth off the ground.
            var high = With(Two(V(0.02f, 0.1f, 0.3f), -24f, -40f, -12f, V(-0.2f, 0.12f, -0.2f), V(0.2f, 0.1f, 0.5f),
                V(-0.3f, 1.98f, 0.18f), V(-0.62f, 0.74f, -0.26f), V(0.75f, 0.6f, 0.2f), 0.26f, 0.6f), p =>
            {
                p.ShoulderLUp = p.ShoulderRUp = 26f;
                p.FootLTip = 40f;
                p.FootRTip = 30f;
            });
            var landed = With(high, p =>
            {
                p.Hips = V(0.02f, -0.16f, 0.3f);
                p.FootL = V(-0.2f, 0f, -0.2f);
                p.FootR = V(0.2f, 0f, 0.5f);
                p.FootLTip = 10f;
                p.FootRTip = 0f;
                p.HandR = V(-0.26f, 1.8f, 0.14f);
            });
            var clip = Attack(step, -8f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 1.6f);
            return clip.Key(0f, With(GswordWoundLeft(), p => p.BladeBack = BackLeftward(V(-0.7f, 0f, -0.67f)))).Key(hit * 0.45f, fallen, Ease.Out)
                .Key(hit * 0.85f, dragging, Ease.InOut).Key(hit, ripped, Ease.In).Key(done, high, Ease.Out)
                .Key(Mathf.Lerp(done, 1f, 0.35f), landed, Ease.Out).Key(1f, GswordCarry(), Ease.InOut);
        }

        /// <summary>The charged cut: released from the raised blade with a jump, both feet off the ground and the knees tucked, landing as the blade does.</summary>
        private static ForgeClip GswordLeap(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var jump = With(Two(V(0f, 0.38f, 0.1f), 0f, 0f, -14f, V(-0.16f, 0.5f, -0.12f), V(0.16f, 0.56f, 0.16f),
                V(0.04f, 2.3f, 0.02f), V(0f, 0.9f, -0.43f), V(1f, 0f, 0f)), p =>
            {
                p.ShoulderLUp = p.ShoulderRUp = 28f;
                p.ElbowR = V(0.7f, 0.3f, 0.5f);
                p.ElbowL = V(-0.7f, 0.3f, 0.5f);
                p.FootLTip = 30f;
                p.FootRTip = 30f;
            });
            var landed = With(GswordLanded(), p =>
            {
                p.Hips = V(-0.12f, -0.56f, 0.12f);
                p.FootL = V(-0.3f, 0f, -0.3f);
                p.FootR = V(0.2f, 0f, 0.56f);
            });
            var clip = Attack(step, -10f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 2.2f);
            clip.SheetMoments = new[] { 0f, hit * 0.5f, hit * 0.8f, hit, done, Mathf.Lerp(done, 1f, 0.3f), Mathf.Lerp(done, 1f, 0.7f), 1f };
            return clip.Key(0f, GswordTipped()).Key(hit * 0.7f, jump, Ease.Out).Key(hit, landed, Ease.In).Key(done, GswordBuried(), Ease.Out)
                .Key(Mathf.Lerp(done, 1f, 0.45f), GswordBuried(), Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.78f), GswordHeave(), Ease.InOut)
                .Key(1f, GswordCarry(), Ease.InOut);
        }

        /// <summary>The Elemental Spike Line: the sword turned point down, raised in both hands, and driven into the ground between the feet; the bearer kneels behind it while the spikes run.</summary>
        private static ForgeClip GswordSpike(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var raised = With(Two(V(0f, -0.06f, -0.04f), 0f, 0f, -8f, V(-0.22f, 0f, -0.1f), V(0.22f, 0f, 0.1f),
                V(0.03f, 2.0f, 0.3f), V(0f, -1f, 0.06f), V(1f, 0f, 0f)), p =>
            {
                p.ShoulderLUp = p.ShoulderRUp = 26f;
                p.ElbowR = V(0.9f, 0.2f, 0.2f);
                p.ElbowL = V(-0.9f, 0.2f, 0.2f);
                p.FootLTip = 20f;
                p.FootRTip = 20f;
            });
            var driven = With(Two(V(0f, -0.56f, 0.06f), 0f, 0f, 30f, V(-0.2f, 0f, -0.46f), V(0.22f, 0f, 0.32f),
                V(0.03f, 0.98f, 0.42f), V(0f, -1f, 0.03f), V(1f, 0f, 0f)), p =>
            {
                p.FootLTip = 62f;
                p.HeadLean = 30f;
                p.ElbowR = V(0.9f, -0.2f, -0.2f);
                p.ElbowL = V(-0.9f, -0.2f, -0.2f);
            });
            var kneeling = With(driven, p =>
            {
                p.Hips = V(0f, -0.6f, 0.08f);
                p.HandR = V(0.03f, 0.92f, 0.42f);
                p.HeadLean = 36f;
            });
            var clip = Attack(step, -3f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 1.6f);
            return clip.Key(0f, GswordTipped()).Key(hit * 0.6f, raised, Ease.Out).Key(hit * 0.85f, raised, Ease.Linear).Key(hit, driven, Ease.In)
                .Key(done, kneeling, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.66f), kneeling, Ease.Linear).Key(1f, GswordCarry(), Ease.InOut);
        }

        /// <summary>
        /// The Headsman. The pommel into the chest; a slow walk round to the enemy's side with the point scoring the
        /// ground; the blade stood up over the head and held while nothing moves; the cut, straight down; and the
        /// bearer already turning away as the ground splits. The walk is a turn of the whole body about the enemy.
        /// </summary>
        private static ForgeClip GswordHeadsman(AttackStep step)
        {
            var (hit, done) = Moments(step);
            float total = step.Total;
            float pommel = 0.14f / total, walked = 0.4f / total, hold = step.HoldAt / total;
            var shove = Two(V(0f, -0.2f, 0.2f), -10f, -10f, 12f, V(-0.2f, 0f, -0.3f), V(0.2f, 0f, 0.4f),
                V(0.14f, 1.22f, 0.56f), V(0.1f, 0.42f, -0.9f), V(-1f, 0f, 0f));
            var trailing = Two(V(0f, -0.16f, 0f), 30f, 24f, 6f, V(-0.2f, 0f, 0.26f), V(0.22f, 0f, -0.24f),
                V(0.44f, 0.84f, -0.06f), V(0.3f, -0.6f, -0.74f), V(0.2f, 0.8f, -0.56f), 0.26f, 0.9f);
            var trailingOn = With(trailing, p =>
            {
                p.FootL = V(-0.2f, 0f, -0.2f);
                p.FootR = V(0.22f, 0f, 0.26f);
                p.Hips = V(0f, -0.14f, 0.02f);
            });
            var raised = With(GswordRaised(), p =>
            {
                p.Hips = V(0f, -0.06f, 0f);
                p.FootL = V(-0.26f, 0f, 0f);
                p.FootR = V(0.26f, 0f, 0.04f);
                p.HandR = V(0.04f, 2.02f, 0.04f);
                p.HeadLean = 14f;
            });
            // Nothing moves in the hold but the breath going out.
            var heldStill = With(raised, p => p.Hips = V(0f, -0.09f, 0f));
            var cut = With(GswordLanded(), p =>
            {
                p.Hips = V(-0.1f, -0.4f, 0.06f);
                p.FootL = V(-0.3f, 0f, -0.1f);
                p.FootR = V(0.26f, 0f, 0.3f);
                p.FootLTip = 0f;
                p.HandR = V(0.24f, 0.62f, 0.56f);
            });
            var sunk = With(cut, p => p.Hips = V(-0.1f, -0.48f, 0.08f));
            // Already standing with the back to it, lifting the sword to the shoulder.
            var away = With(GswordCarry(), p =>
            {
                p.HipYaw = 120f;
                p.WaistYaw = 40f;
                p.HeadYaw = -20f;
                p.Hips = V(0f, -0.06f, 0f);
            });
            var clip = Attack(step, -5f);
            clip.Shape = ForgeClip.Impact(hit, total, 2.2f);
            clip.SheetMoments = new[] { pommel, (pommel + walked) * 0.5f, walked, hold + 0.02f, hit * 0.97f, hit, done + 0.05f, 0.86f };
            clip.Key(0f, GswordCarry()).Key(pommel, shove, Ease.In).Key(Mathf.Lerp(pommel, walked, 0.4f), trailing, Ease.Out)
                .Key(walked, trailingOn, Ease.InOut).Key(hold, raised, Ease.InOut).Key(hit * 0.97f, heldStill, Ease.Linear)
                .Key(hit, cut, Ease.In).Key(done, sunk, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.25f), sunk, Ease.Linear)
                .Key(Mathf.Lerp(done, 1f, 0.7f), away, Ease.InOut).Key(1f, GswordCarry(), Ease.InOut);
            // Round to the enemy's side between the shove and the hold, there through the cut, and back as the sword goes to the shoulder.
            float side = step.EffectTurn, back = Mathf.Lerp(done, 1f, 0.3f);
            clip.MovesRoot = true;
            clip.After = TurnAbout(t => t < back
                ? side * Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(pommel, walked, t))
                : side * (1f - Mathf.SmoothStep(0f, 1f, Mathf.InverseLerp(back, 0.97f, t))), Vector3.up, V(0f, 0f, 1.25f));
            return clip;
        }
    }
}

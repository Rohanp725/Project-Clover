using System;
using System.Collections.Generic;
using Clover.Player;
using UnityEngine;

namespace Clover.EditorTools.Forge
{
    /// <summary>
    /// The Full Body Armament's clips. The four lights, the heavy and the rising ender are the fists' own, baked again
    /// under the Armament's names and set heavier (lower, wider, shoulders squared) so the coat does not look like the
    /// starting weapon with paint on. New here: the braced stance, the ram, the slam, the burst, Meteor Fist, Energy
    /// Reflect, One Inch, and the guard's two answers (which the fists get too).
    /// </summary>
    public static partial class PlayerAnimations
    {
        private static IEnumerable<ForgeClip> ArmamentClips()
        {
            var combo = Weapons.Armament(Elements.Fire).Combo;
            yield return ArmIdle();
            yield return Bruiser(Renamed(RunFist(), "run_arm"));
            yield return Bruiser(Renamed(FistGuard(), "guard_arm"));
            yield return GuardHitFist("arm");
            yield return GuardPerfectFist("arm");
            yield return ArmBrace(combo.ChargeTime);
            yield return Bruiser(Jab(combo.Steps[0]));
            yield return Bruiser(Cross(combo.Steps[1]));
            yield return Bruiser(Hook(combo.Steps[2]));
            yield return Bruiser(FrontKick(combo.Steps[3]));
            yield return Bruiser(Overhand(combo.Heavy));
            yield return ArmRam(combo.ChainHeavies[1]);
            yield return Bruiser(Uppercut(combo.ChainHeavies[2]));
            yield return ArmSlam(combo.ChainHeavies[3]);
            yield return ArmBurst(combo.ChainHeavies[4]);
            yield return ArmMeteor(combo.ChargedHeavy);
            yield return ArmReflect(combo.Skill);
            yield return ArmOneInch(combo.Finisher);
        }

        private static ForgeClip Renamed(ForgeClip clip, string name)
        {
            clip.Name = name;
            return clip;
        }

        // The fists' clip made a bruiser's: weight dropped into the legs, feet wider, shoulders squared and raised.
        private static ForgeClip Bruiser(ForgeClip clip)
        {
            Func<float, ForgePose, ForgePose> heavier = (t, p) =>
            {
                p.Hips += V(0f, -0.06f, 0f);
                p.FootL += V(-0.05f, 0f, 0f);
                p.FootR += V(0.05f, 0f, 0f);
                p.ShoulderLUp += 5f;
                p.ShoulderRUp += 5f;
                p.WaistLean += 3f;
                return p;
            };
            clip.Shape = clip.Shape != null ? Both(clip.Shape, heavier) : heavier;
            return clip;
        }

        // A bare-handed pose: body, feet, and both hands.
        private static ForgePose Bare(Vector3 hips, float hipYaw, float waistYaw, float lean, Vector3 footL, Vector3 footR, Vector3 handL, Vector3 handR, float keep = 0.8f)
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
                FootLTurn = 14f,
                FootRTurn = 14f,
                HandL = handL,
                HandR = handR,
                ElbowL = new Vector3(-0.6f, -0.9f, -0.2f),
                ElbowR = new Vector3(0.6f, -0.9f, -0.2f),
                ShoulderLUp = 10f,
                ShoulderRUp = 10f,
            }, keep);
        }

        // Squared up and low: the Armament's own ready stance. Hands open and held wide, not bouncing.
        private static ForgePose ArmReady() => Bare(V(0f, -0.18f, 0f), 16f, 4f, 10f, V(-0.2f, 0f, 0.2f), V(0.26f, 0f, -0.18f),
            V(-0.3f, 1.24f, 0.36f), V(0.3f, 1.2f, 0.22f));

        private static ForgeClip ArmIdle() => new ForgeClip("idle_arm", 1.3f, true).From(t =>
        {
            float breath = Mathf.Sin(t * Mathf.PI * 2f);
            var p = ArmReady();
            p.Hips += V(0f, 0.012f * breath, 0f);
            p.WaistLean += 2f * breath;
            p.ShoulderLUp = p.ShoulderRUp = 10f + 4f * breath;
            p.HandL += V(0f, 0.012f * breath, 0f);
            p.HandR += V(0f, 0.012f * breath, 0f);
            return p;
        });

        // Braced: feet wide and square, knees bent, both fists at the hips with the elbows back, chest lifted, chin down. An open front, daring the blow.
        private static ForgePose ArmBraced() => With(Bare(V(0f, -0.24f, 0f), 0f, 0f, -10f, V(-0.32f, 0f, 0f), V(0.32f, 0f, 0f),
            V(-0.4f, 1.0f, 0f), V(0.4f, 1.0f, 0f)), p =>
        {
            // The fists stand off the hips and the elbows flare, so from above the stance is a wide square and not a post.
            p.ElbowL = V(-0.8f, 0.1f, -0.7f);
            p.ElbowR = V(0.8f, 0.1f, -0.7f);
            p.HeadLean = 14f;
            p.FootLTurn = 30f;
            p.FootRTurn = 30f;
            p.ShoulderLUp = p.ShoulderRUp = 6f;
        });

        private static ForgeClip ArmBrace(float seconds)
        {
            var clip = new ForgeClip("charge_arm", seconds);
            // A stamp as the feet go wide, then the stance; the end of the clip is what is held.
            var stamp = With(ArmBraced(), p =>
            {
                p.Hips = V(0f, -0.32f, 0f);
                p.HandL = V(-0.36f, 1.1f, 0.1f);
                p.HandR = V(0.36f, 1.1f, 0.1f);
            });
            return clip.Key(0f, ArmReady()).Key(0.6f, stamp, Ease.In).Key(1f, ArmBraced(), Ease.Out);
        }

        // The crossed forearms driven back to the brow, and the body sliding half a foot.
        private static ForgeClip GuardHitFist(string prefix)
        {
            var cross = Bare(V(0f, -0.2f, -0.04f), 10f, 0f, 10f, V(-0.18f, 0f, 0.16f), V(0.22f, 0f, -0.18f),
                V(0.1f, 1.46f, 0.36f), V(-0.1f, 1.4f, 0.4f));
            var driven = With(cross, p =>
            {
                p.Hips = V(0f, -0.24f, -0.16f);
                p.HandL = V(0.1f, 1.56f, 0.2f);
                p.HandR = V(-0.1f, 1.5f, 0.24f);
                p.WaistLean = -4f;
                p.HeadLean = 16f;
            });
            return new ForgeClip("guard_hit_" + prefix, 0.22f).Key(0f, cross).Key(0.25f, driven, Ease.Snap).Key(1f, cross, Ease.InOut);
        }

        // The cross thrown open outward: both arms flung wide and down, chest out, a stamp of the lead foot. The blow is burst apart, not turned.
        private static ForgeClip GuardPerfectFist(string prefix)
        {
            var cross = Bare(V(0f, -0.2f, -0.04f), 10f, 0f, 10f, V(-0.18f, 0f, 0.16f), V(0.22f, 0f, -0.18f),
                V(0.1f, 1.46f, 0.36f), V(-0.1f, 1.4f, 0.4f));
            var open = With(Bare(V(0f, -0.26f, 0.08f), 0f, 0f, -8f, V(-0.2f, 0f, 0.3f), V(0.24f, 0f, -0.2f),
                V(-0.74f, 1.0f, 0.1f), V(0.74f, 1.0f, 0.1f)), p =>
            {
                p.ElbowL = V(-1f, 0.2f, -0.4f);
                p.ElbowR = V(1f, 0.2f, -0.4f);
                p.HeadLean = -10f;
            });
            return new ForgeClip("guard_perfect_" + prefix, 0.3f).Key(0f, cross).Key(0.25f, open, Ease.Snap).Key(0.6f, open, Ease.Linear)
                .Key(1f, prefix == "arm" ? ArmReady() : FistReady(), Ease.InOut);
        }

        /// <summary>The ram: the lead shoulder dips and turns in with the fist under the chin, then a flat drive, shoulder and forearm first; the folded arm is thrown open at the end, which is the shove.</summary>
        private static ForgeClip ArmRam(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var tucked = With(Bare(V(0f, -0.3f, -0.12f), 48f, 14f, 18f, V(-0.16f, 0f, 0.22f), V(0.24f, 0f, -0.3f),
                V(0.06f, 1.34f, 0.24f), V(0.34f, 1.0f, -0.16f)), p =>
            {
                p.ElbowL = V(-0.6f, -0.2f, 0.8f);
                p.ShoulderLForward = 24f;
                p.FootRTip = 30f;
            });
            var drive = With(Bare(V(0f, -0.34f, 0.3f), 60f, 16f, 40f, V(-0.14f, 0f, 0.56f), V(0.22f, 0f, -0.6f),
                V(0.08f, 1.26f, 0.5f), V(0.4f, 0.9f, -0.3f)), p =>
            {
                p.ElbowL = V(-0.5f, 0f, 1f);
                p.ShoulderLForward = 34f;
                p.ShoulderLUp = 20f;
                p.FootRTip = 50f;
                p.HeadLean = -20f;
            });
            var thrown = With(Bare(V(0f, -0.24f, 0.2f), 6f, -10f, 12f, V(-0.2f, 0f, 0.3f), V(0.24f, 0f, -0.2f),
                V(-0.78f, 1.3f, 0.34f), V(0.4f, 0.94f, 0f)), p =>
            {
                p.ElbowL = V(-1f, 0f, -0.3f);
                p.ShoulderLUp = 16f;
            });
            var clip = Attack(step, -12f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 1.3f);
            return clip.Key(0f, ArmReady()).Key(hit * 0.7f, tucked, Ease.Out).Key(hit, drive, Ease.In).Key(done - 0.04f, drive, Ease.Linear)
                .Key(done + 0.06f, thrown, Ease.Snap).Key(Mathf.Lerp(done, 1f, 0.6f), thrown, Ease.Linear).Key(1f, ArmReady(), Ease.InOut);
        }

        /// <summary>The slam: both fists up over the head with the back arched, on the toes; then down side by side onto the ground just ahead as the knees fold.</summary>
        private static ForgeClip ArmSlam(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var up = With(Bare(V(0f, 0.06f, -0.06f), 0f, 0f, -20f, V(-0.2f, 0f, 0f), V(0.2f, 0f, 0f),
                V(-0.16f, 2.06f, 0f), V(0.16f, 2.06f, 0f)), p =>
            {
                p.ElbowL = V(-1f, 0.3f, 0.2f);
                p.ElbowR = V(1f, 0.3f, 0.2f);
                p.ShoulderLUp = p.ShoulderRUp = 28f;
                p.FootLTip = 30f;
                p.FootRTip = 30f;
                p.HeadLean = -10f;
            });
            var down = With(Bare(V(0f, -0.62f, 0.12f), 0f, 0f, 62f, V(-0.28f, 0f, -0.06f), V(0.28f, 0f, -0.06f),
                V(-0.16f, 0.14f, 0.6f), V(0.16f, 0.14f, 0.6f)), p =>
            {
                p.ElbowL = V(-1f, 0.2f, 0f);
                p.ElbowR = V(1f, 0.2f, 0f);
                p.KneeL = V(-0.7f, 0f, 1f);
                p.KneeR = V(0.7f, 0f, 1f);
                p.HeadLean = 20f;
            });
            var crouched = With(down, p =>
            {
                p.Hips = V(0f, -0.66f, 0.14f);
                p.HeadLean = 34f;
            });
            var clip = Attack(step, 0f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 1.8f);
            return clip.Key(0f, ArmReady()).Key(hit * 0.6f, up, Ease.Out).Key(hit * 0.86f, With(up, p => p.Hips = V(0f, 0.09f, -0.08f)), Ease.Linear)
                .Key(hit, down, Ease.In).Key(done, crouched, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.55f), crouched, Ease.Linear).Key(1f, ArmReady(), Ease.InOut);
        }

        /// <summary>The burst: both hands drawn back to the right hip, wrists together, the body side-on and sunk; then a stride and both palms thrust out, and the arms left hanging there while the cone goes.</summary>
        private static ForgeClip ArmBurst(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var gathered = With(Bare(V(0.04f, -0.3f, -0.1f), 58f, 24f, 8f, V(-0.16f, 0f, 0.22f), V(0.26f, 0f, -0.3f),
                V(0.26f, 1.04f, -0.08f), V(0.34f, 0.96f, -0.14f), 0.9f), p =>
            {
                p.ElbowL = V(-0.4f, -0.4f, 1f);
                p.ElbowR = V(0.6f, 0f, -1f);
                p.FootRTurn = 50f;
            });
            var sunk = With(gathered, p => p.Hips = V(0.05f, -0.36f, -0.14f));
            var thrust = With(Bare(V(0f, -0.26f, 0.26f), 0f, 0f, 16f, V(-0.2f, 0f, 0.56f), V(0.24f, 0f, -0.44f),
                V(-0.07f, 1.32f, 0.9f), V(0.07f, 1.26f, 0.9f)), p =>
            {
                p.ElbowL = V(-0.8f, -0.6f, 0f);
                p.ElbowR = V(0.8f, -0.6f, 0f);
                p.HandLBend = -70f;
                p.HandRBend = -70f;
                p.ShoulderLForward = p.ShoulderRForward = 20f;
                p.FootRTip = 34f;
            });
            var clip = Attack(step, -6f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 1.5f);
            return clip.Key(0f, ArmReady()).Key(hit * 0.55f, gathered, Ease.Out).Key(hit * 0.86f, sunk, Ease.InOut).Key(hit, thrust, Ease.In)
                .Key(Mathf.Lerp(done, 1f, 0.6f), With(thrust, p => { p.HandL = V(-0.08f, 1.26f, 0.86f); p.HandR = V(0.08f, 1.2f, 0.86f); p.HandLBend = -20f; p.HandRBend = -20f; }), Ease.Out)
                .Key(1f, ArmReady(), Ease.InOut);
        }

        /// <summary>Meteor Fist: a leap from the braced stance with the right fist drawn back past the ear and the left hand pointing, landing with the punch driven down through the target.</summary>
        private static ForgeClip ArmMeteor(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var air = With(Bare(V(0f, 0.42f, 0.1f), 36f, 20f, -6f, V(-0.14f, 0.5f, 0.2f), V(0.18f, 0.6f, -0.2f),
                V(-0.1f, 1.86f, 0.62f), V(0.44f, 2.1f, -0.24f)), p =>
            {
                p.ElbowR = V(0.8f, 0.4f, -0.8f);
                p.ElbowL = V(-0.8f, -0.4f, 0f);
                p.ShoulderRUp = 24f;
                p.FootLTip = 30f;
                p.FootRTip = 40f;
            });
            var landed = With(Bare(V(0f, -0.46f, 0.36f), -30f, -30f, 40f, V(-0.2f, 0f, 0.6f), V(0.24f, 0f, -0.5f),
                V(-0.6f, 1.0f, -0.1f), V(0.1f, 0.62f, 0.8f)), p =>
            {
                p.ElbowR = V(0.8f, 0.3f, -0.3f);
                p.ElbowL = V(-1f, 0f, -0.4f);
                p.ShoulderRForward = 26f;
                p.FootRTip = 50f;
            });
            var knee = With(landed, p =>
            {
                p.Hips = V(0f, -0.6f, 0.38f);
                p.HandR = V(0.1f, 0.2f, 0.72f);
                p.FootRTip = 64f;
                p.HeadLean = 16f;
            });
            var clip = Attack(step, -12f);
            clip.Shape = ForgeClip.Impact(hit, step.Total, 2f);
            clip.SheetMoments = new[] { 0f, hit * 0.5f, hit * 0.8f, hit, done, Mathf.Lerp(done, 1f, 0.3f), Mathf.Lerp(done, 1f, 0.7f), 1f };
            return clip.Key(0f, ArmBraced()).Key(hit * 0.7f, air, Ease.Out).Key(hit, landed, Ease.In).Key(done, knee, Ease.Out)
                .Key(Mathf.Lerp(done, 1f, 0.55f), knee, Ease.Linear).Key(1f, ArmReady(), Ease.InOut);
        }

        /// <summary>Energy Reflect: there is no wind-up, the braced stance is it. The chest takes the blow and bows back round it; in the same motion both fists come up and are punched out and open, arms wide, head back: a roar with the whole body.</summary>
        private static ForgeClip ArmReflect(AttackStep step)
        {
            var (hit, done) = Moments(step);
            var taken = With(ArmBraced(), p =>
            {
                p.Hips = V(0f, -0.26f, -0.12f);
                p.WaistLean = -22f;
                p.HipLean = -6f;
                p.HeadLean = 22f;
                p.ShoulderLForward = p.ShoulderRForward = 16f;
            });
            var roar = With(Bare(V(0f, -0.2f, 0.12f), 0f, 0f, 8f, V(-0.34f, 0f, 0.04f), V(0.34f, 0f, -0.04f),
                V(-0.8f, 1.52f, 0.3f), V(0.8f, 1.52f, 0.3f)), p =>
            {
                p.ElbowL = V(-1f, -0.3f, -0.4f);
                p.ElbowR = V(1f, -0.3f, -0.4f);
                p.HandLBend = -50f;
                p.HandRBend = -50f;
                p.HeadLean = -28f;
                p.ShoulderLUp = p.ShoulderRUp = 14f;
                p.FootLTurn = 30f;
                p.FootRTurn = 30f;
            });
            var clip = Attack(step, 0f);
            clip.Shape = ForgeClip.Shudder(0f, step.Total, 0.8f);
            clip.SheetMoments = new[] { 0f, hit, (hit + done) * 0.5f, done, Mathf.Lerp(done, 1f, 0.2f), Mathf.Lerp(done, 1f, 0.4f), Mathf.Lerp(done, 1f, 0.7f), 1f };
            return clip.Key(0f, ArmBraced()).Key(hit, taken, Ease.Snap).Key(done, roar, Ease.In)
                .Key(Mathf.Lerp(done, 1f, 0.35f), With(roar, p => p.Hips = V(0f, -0.22f, 0.14f)), Ease.Linear).Key(1f, ArmReady(), Ease.InOut);
        }

        /// <summary>
        /// One Inch. Six fast blows, even as a drum roll: left and right to the ribs, two hooks, a knee, a rising elbow.
        /// Then no blow at all: the right hand laid open and still against the chest, the left fist at the hip. Then the
        /// hips turn and the arm straightens by the width of a palm.
        /// </summary>
        private static ForgeClip ArmOneInch(AttackStep step)
        {
            var (hit, done) = Moments(step);
            float total = step.Total;
            Vector3 footL = V(-0.2f, 0f, 0.3f), footR = V(0.24f, 0f, -0.22f);
            Vector3 guardL = V(-0.24f, 1.34f, 0.26f), guardR = V(0.24f, 1.3f, 0.2f);
            var blows = new[]
            {
                // Left to the ribs, right to the ribs.
                Bare(V(0f, -0.26f, 0.12f), 40f, 30f, 16f, footL, footR, V(0.04f, 1.08f, 0.8f), guardR),
                Bare(V(0f, -0.26f, 0.14f), -34f, -30f, 16f, footL, footR, guardL, V(-0.04f, 1.08f, 0.82f)),
                // Left hook to the head, right hook.
                With(Bare(V(0f, -0.2f, 0.14f), 44f, 36f, 8f, footL, footR, V(0.2f, 1.62f, 0.62f), guardR), p => { p.ElbowL = V(-1f, 0.4f, 0.2f); p.ShoulderLUp = 22f; }),
                With(Bare(V(0f, -0.2f, 0.16f), -40f, -36f, 8f, footL, footR, guardL, V(-0.2f, 1.62f, 0.64f)), p => { p.ElbowR = V(1f, 0.4f, 0.2f); p.ShoulderRUp = 22f; }),
                // A knee, the hands dragging down.
                With(Bare(V(0f, -0.1f, 0.2f), 10f, 0f, -8f, footL, V(0.06f, 0.78f, 0.5f), V(-0.16f, 1.2f, 0.52f), V(0.16f, 1.2f, 0.52f)), p => { p.KneeR = V(0.1f, 0.5f, 1f); p.FootRTip = 50f; p.FootLTip = 20f; }),
                // A rising elbow.
                With(Bare(V(0f, -0.04f, 0.22f), -30f, -34f, -10f, footL, V(0.22f, 0f, 0.1f), guardL, V(0.02f, 1.72f, 0.36f)), p => { p.ElbowR = V(0.2f, 1f, 1f); p.ShoulderRUp = 30f; p.FootLTip = 20f; }),
            };
            // The hold: the hand flat on the chest, the arm bent, the other fist drawn back to the hip. Stillness.
            var laid = With(Bare(V(0f, -0.24f, 0.08f), 26f, 12f, 6f, footL, V(0.26f, 0f, -0.28f), V(-0.32f, 0.98f, -0.1f), V(0.06f, 1.26f, 0.6f)), p =>
            {
                p.ElbowL = V(-0.4f, 0f, -1f);
                p.ElbowR = V(0.9f, -0.5f, -0.2f);
                p.HandRBend = -70f;
                p.FootRTurn = 44f;
            });
            var still = With(laid, p => p.Hips = V(0f, -0.26f, 0.08f));
            // The blow: the hips turn and the arm straightens by the width of the palm. Nothing draws back.
            var inch = With(laid, p =>
            {
                p.Hips = V(0f, -0.3f, 0.18f);
                p.HipYaw = -22f;
                p.WaistYaw = -16f;
                p.HandR = V(0.03f, 1.26f, 0.76f);
                p.ShoulderRForward = 26f;
                p.FootRTip = 34f;
                Looking(p);
            });
            var clip = Attack(step, -3f);
            clip.Shape = ForgeClip.Impact(hit, total, 2.4f);
            float hold = step.HoldAt / total;
            clip.SheetMoments = new[] { 0.08f / total, 0.26f / total, 0.44f / total, 0.53f / total, hold + 0.04f, hit * 0.98f, hit, Mathf.Lerp(done, 1f, 0.3f) };
            clip.Key(0f, ArmReady());
            for (int i = 0; i < blows.Length; i++)
                clip.Key((0.08f + 0.09f * i) / total, blows[i], Ease.In);
            return clip.Key(hold + 0.03f, laid, Ease.Out).Key(hit * 0.98f, still, Ease.Linear).Key(hit, inch, Ease.Snap)
                .Key(Mathf.Lerp(done, 1f, 0.6f), inch, Ease.Linear).Key(1f, ArmReady(), Ease.InOut);
        }
    }
}

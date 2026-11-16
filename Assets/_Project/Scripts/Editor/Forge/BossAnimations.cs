using System;
using System.Collections.Generic;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.EditorTools.Forge
{
    /// <summary>
    /// The animations of Narukami, the god of Lightning. He moves like a master who has done this ten thousand times:
    /// upright, unhurried between blows, the sword carried low in one hand, and then all at once. Each attack in his
    /// list gets its own clip, timed from that attack's numbers; attacks that share a motion name the motion to reuse.
    /// </summary>
    public static class BossAnimations
    {
        public static IEnumerable<ForgeClip> For(EnemyKind kind)
        {
            foreach (var clip in EnemyAnimations.CommonClips("boss", Ready, 0.26f, 0.85f, Kneel())) yield return clip;
            foreach (var attack in kind.Brain.Attacks)
            {
                switch (attack.AnimationName)
                {
                    case "boss_slash_1": yield return Cut(attack, -1f); break;
                    case "boss_slash_2": yield return Cut(attack, 1f); break;
                    case "boss_slash_3": yield return Down(attack); break;
                    case "boss_thrust": yield return Thrust(attack); break;
                    case "boss_iai": yield return Draw(attack); break;
                    case "boss_call": yield return Call(attack, true); break;
                    case "boss_call_again": yield return Call(attack, false); break;
                    case "boss_nova": yield return Burst(attack); break;
                    case "boss_roar": yield return Roar(attack); break;
                    default: Debug.LogWarning($"[Clover.Forge] No motion written for the boss attack '{attack.AnimationName}'."); break;
                }
            }
        }

        private static ForgePose Looking(ForgePose pose, float keep = 0.8f)
        {
            pose.HeadYaw = -(pose.HipYaw + pose.WaistYaw) * keep;
            return pose;
        }

        // Standing at his ease: the long sword low in the right hand, point toward the ground ahead, left hand on the sheath.
        private static ForgePose Ready() => Looking(new ForgePose
        {
            Hips = new Vector3(0f, -0.05f, 0f),
            HipYaw = 22f,
            WaistLean = -2f,
            FootL = new Vector3(-0.15f, 0f, 0.18f),
            FootR = new Vector3(0.18f, 0f, -0.14f),
            FootLTurn = 0f,
            FootRTurn = 42f,
            HandL = new Vector3(-0.27f, 1.0f, 0.1f),
            ElbowL = new Vector3(-0.8f, -0.1f, -0.6f),
            HandR = new Vector3(0.42f, 0.9f, 0.1f),
            AimBlade = true,
            Blade = new Vector3(0.22f, -0.62f, 0.75f),
        });

        private static ForgePose Kneel() => new ForgePose
        {
            // Broken open: down on one knee with the sword driven into the ground to hold him up.
            Hips = new Vector3(0f, -0.5f, 0f),
            HipLean = 8f,
            WaistLean = 24f,
            HeadLean = 30f,
            FootL = new Vector3(-0.17f, 0f, 0.26f),
            FootR = new Vector3(0.18f, 0f, -0.3f),
            FootRTip = 70f,
            KneeR = new Vector3(0.3f, -0.4f, 1f),
            HandL = new Vector3(-0.3f, 0.5f, 0.3f),
            HandR = new Vector3(0.26f, 0.98f, 0.36f),
            AimBlade = true,
            Blade = new Vector3(0.05f, -1f, 0.1f),
        };

        private static ForgeClip Attack(EnemyAttack attack, out float hit, out float done)
        {
            hit = attack.Telegraph / attack.Total;
            done = (attack.Telegraph + attack.Strike) / attack.Total;
            float h = hit, d = done;
            return new ForgeClip(attack.Name, attack.Total)
            {
                Shape = ForgeClip.Impact(attack.Telegraph / attack.Total, attack.Total, 1.2f),
                Wind = t => new Vector3(0f, 0f, t > h * 0.8f && t < d ? -9f : 0f),
                SheetMoments = new[] { 0f, h * 0.45f, h * 0.9f, h, (h + d) * 0.5f, d, Mathf.Lerp(d, 1f, 0.4f), 1f },
            };
        }

        // A level cut across. Side is -1 for a cut from his left to his right, +1 for the return.
        private static ForgeClip Cut(EnemyAttack attack, float side)
        {
            var clip = Attack(attack, out float hit, out float done);
            // side -1: wound up on his left, ends on his right. Mirrored for +1.
            float s = -side;
            var wound = Looking(new ForgePose
            {
                Hips = new Vector3(-0.03f * s, -0.16f, -0.04f),
                HipYaw = -22f * s,
                WaistYaw = -46f * s,
                WaistLean = 8f,
                FootL = new Vector3(-0.2f, 0f, s > 0 ? -0.26f : 0.3f),
                FootR = new Vector3(0.18f, 0f, s > 0 ? 0.3f : -0.26f),
                FootLTurn = 30f,
                FootRTurn = 20f,
                HandR = new Vector3(-0.42f * s + 0.1f, 1.24f, 0.0f),
                ElbowR = new Vector3(0.8f, -0.3f, 0.3f),
                AimBlade = true,
                TwoHanded = true,
                Blade = new Vector3(-0.8f * s, 0.3f, -0.52f),
            }, 0.7f);
            var across = Looking(new ForgePose
            {
                Hips = new Vector3(0f, -0.2f, 0.14f),
                HipYaw = 0f,
                WaistYaw = 4f * s,
                WaistLean = 10f,
                FootL = new Vector3(-0.2f, 0f, s > 0 ? -0.32f : 0.42f),
                FootR = new Vector3(0.18f, 0f, s > 0 ? 0.42f : -0.32f),
                FootLTurn = 30f,
                FootRTurn = 20f,
                HandR = new Vector3(0.1f, 1.16f, 0.7f),
                AimBlade = true,
                TwoHanded = true,
                Blade = new Vector3(-0.15f * s, 0.04f, 1f),
            });
            var through = Looking(new ForgePose
            {
                Hips = new Vector3(0.04f * s, -0.2f, 0.14f),
                HipYaw = 26f * s,
                WaistYaw = 54f * s,
                WaistLean = 6f,
                FootL = new Vector3(-0.2f, 0f, s > 0 ? -0.32f : 0.42f),
                FootR = new Vector3(0.18f, 0f, s > 0 ? 0.42f : -0.32f),
                FootLTurn = 30f,
                FootRTurn = 20f,
                HandR = new Vector3(0.62f * s, 1.2f, 0.06f),
                AimBlade = true,
                TwoHanded = true,
                Blade = new Vector3(0.94f * s, 0.1f, -0.3f),
            }, 0.6f);
            // A cut that leads straight into another does not go back to standing; one that ends the exchange does.
            bool chained = !string.IsNullOrEmpty(attack.ChainInto);
            var last = chained ? through : Ready();
            return clip.Key(0f, attack.BlinkTo > 0f ? wound : Ready()).Key(hit * 0.55f, wound, Ease.Out).Key(hit * 0.88f, wound, Ease.Linear)
                .Key(hit, across, Ease.In).Key(done, through, Ease.Out).Key(1f, last, chained ? Ease.Linear : Ease.InOut);
        }

        // The third cut: both hands, from over the head straight down the middle with a long step.
        private static ForgeClip Down(EnemyAttack attack)
        {
            var clip = Attack(attack, out float hit, out float done);
            var raise = new ForgePose
            {
                Hips = new Vector3(0f, -0.04f, -0.1f),
                HipLean = -8f,
                WaistLean = -18f,
                HeadLean = 14f,
                FootL = new Vector3(-0.17f, 0f, -0.2f),
                FootR = new Vector3(0.16f, 0f, 0.14f),
                FootLTurn = 30f,
                HandR = new Vector3(0.06f, 1.94f, -0.12f),
                ElbowR = new Vector3(1f, 0.3f, 0.3f),
                ElbowL = new Vector3(-1f, 0.3f, 0.3f),
                ShoulderRUp = 24f,
                ShoulderLUp = 24f,
                AimBlade = true,
                TwoHanded = true,
                Blade = new Vector3(0f, 0.2f, -0.98f),
            };
            var cut = new ForgePose
            {
                Hips = new Vector3(0f, -0.26f, 0.24f),
                HipLean = 20f,
                WaistLean = 22f,
                HeadLean = -20f,
                FootL = new Vector3(-0.19f, 0f, -0.38f),
                FootR = new Vector3(0.16f, 0f, 0.56f),
                FootLTurn = 40f,
                FootLTip = 24f,
                HandR = new Vector3(0.05f, 1.0f, 0.76f),
                AimBlade = true,
                TwoHanded = true,
                Blade = new Vector3(0f, 0.02f, 1f),
            };
            var low = cut.Copy();
            low.Hips = new Vector3(0f, -0.38f, 0.28f);
            low.HipLean = 28f;
            low.WaistLean = 28f;
            low.HandR = new Vector3(0.05f, 0.72f, 0.66f);
            low.Blade = new Vector3(0f, -0.4f, 0.92f);
            return clip.Key(0f, raise).Key(hit * 0.5f, raise, Ease.Hold).Key(hit * 0.85f, raise, Ease.Linear).Key(hit, cut, Ease.In)
                .Key(done, low, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), low, Ease.Linear).Key(1f, Ready(), Ease.InOut);
        }

        private static ForgeClip Thrust(EnemyAttack attack)
        {
            var clip = Attack(attack, out float hit, out float done);
            var chamber = Looking(new ForgePose
            {
                Hips = new Vector3(0.04f, -0.22f, -0.16f),
                HipYaw = 36f,
                WaistYaw = 30f,
                WaistLean = 8f,
                FootL = new Vector3(-0.18f, 0f, 0.22f),
                FootR = new Vector3(0.2f, 0f, -0.3f),
                FootRTurn = 40f,
                HandR = new Vector3(0.36f, 1.12f, -0.2f),
                ElbowR = new Vector3(0.6f, 0f, -1f),
                AimBlade = true,
                TwoHanded = true,
                GripGap = 0.15f,
                Blade = new Vector3(-0.12f, 0.02f, 1f),
            });
            var reach = Looking(new ForgePose
            {
                Hips = new Vector3(0f, -0.32f, 0.36f),
                HipYaw = -14f,
                HipLean = 18f,
                WaistYaw = -22f,
                WaistLean = 14f,
                FootL = new Vector3(-0.18f, 0f, 0.72f),
                FootR = new Vector3(0.2f, 0f, -0.44f),
                FootRTurn = 30f,
                FootRTip = 30f,
                HandR = new Vector3(0.1f, 1.14f, 0.94f),
                ShoulderRForward = 22f,
                AimBlade = true,
                TwoHanded = true,
                GripGap = 0.15f,
                Blade = new Vector3(0f, 0f, 1f),
            });
            return clip.Key(0f, Ready()).Key(hit * 0.55f, chamber, Ease.Out).Key(hit * 0.88f, chamber, Ease.Linear).Key(hit, reach, Ease.In)
                .Key(done, reach, Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.45f), reach, Ease.Linear).Key(1f, Ready(), Ease.InOut);
        }

        // The red draw: the sword goes home to the sheath, he sinks and turns away, a long stillness, and then he is past you.
        private static ForgeClip Draw(EnemyAttack attack)
        {
            var clip = Attack(attack, out float hit, out float done);
            var sunk = Looking(new ForgePose
            {
                Hips = new Vector3(-0.04f, -0.36f, -0.06f),
                HipYaw = -38f,
                HipLean = 16f,
                WaistYaw = -32f,
                WaistLean = 18f,
                FootL = new Vector3(-0.24f, 0f, -0.38f),
                FootR = new Vector3(0.16f, 0f, 0.46f),
                FootLTurn = 50f,
                FootLTip = 14f,
                HandL = new Vector3(-0.3f, 0.76f, 0.0f),
                ElbowL = new Vector3(-0.9f, 0.1f, -0.5f),
                HandR = new Vector3(-0.22f, 0.8f, 0.24f),
                ElbowR = new Vector3(0.9f, -0.1f, 0.4f),
                ShoulderRForward = 16f,
                AimBlade = true,
                Blade = new Vector3(-0.5f, -0.2f, -0.85f),
            }, 0.85f);
            var deeper = sunk.Copy();
            deeper.Hips = new Vector3(-0.04f, -0.42f, -0.1f);
            deeper.WaistLean = 22f;
            var through = Looking(new ForgePose
            {
                Hips = new Vector3(0.06f, -0.36f, 0.24f),
                HipYaw = 34f,
                HipLean = 14f,
                WaistYaw = 62f,
                WaistLean = 8f,
                FootL = new Vector3(-0.24f, 0f, -0.48f),
                FootR = new Vector3(0.16f, 0f, 0.58f),
                FootLTurn = 50f,
                FootLTip = 26f,
                HandL = new Vector3(-0.36f, 0.76f, -0.14f),
                ElbowL = new Vector3(-0.9f, 0.1f, -0.5f),
                HandR = new Vector3(0.78f, 1.08f, -0.16f),
                ShoulderRUp = 10f,
                AimBlade = true,
                Blade = new Vector3(0.82f, 0.08f, -0.56f),
            }, 0.5f);
            var held = through.Copy();
            held.WaistYaw = 58f;
            clip.Wind = t => new Vector3(0f, 0f, t > hit * 0.75f && t < done ? -16f : 0f);
            return clip.Key(0f, Ready()).Key(hit * 0.3f, sunk, Ease.Out).Key(hit * 0.94f, deeper, Ease.InOut).Key(hit, through, Ease.Snap)
                .Key(done, through, Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.6f), held, Ease.Linear).Key(1f, Ready(), Ease.InOut);
        }

        // Calling a bolt: the sword raised to the sky, the drums behind him answering, and the point brought down on the place.
        private static ForgeClip Call(EnemyAttack attack, bool first)
        {
            var clip = Attack(attack, out float hit, out float done);
            var raised = new ForgePose
            {
                Hips = new Vector3(0f, 0f, -0.04f),
                WaistLean = -14f,
                HeadLean = -22f,
                FootL = new Vector3(-0.16f, 0f, 0.1f),
                FootR = new Vector3(0.18f, 0f, -0.1f),
                HandL = new Vector3(-0.3f, 1.0f, 0.1f),
                HandR = new Vector3(0.32f, 2.0f, 0.06f),
                ElbowR = new Vector3(1f, 0.2f, 0.2f),
                ShoulderRUp = 30f,
                AimBlade = true,
                Blade = new Vector3(0.04f, 1f, -0.06f),
            };
            var point = new ForgePose
            {
                Hips = new Vector3(0f, -0.12f, 0.08f),
                HipLean = 6f,
                WaistLean = 14f,
                WaistYaw = -16f,
                FootL = new Vector3(-0.16f, 0f, 0.26f),
                FootR = new Vector3(0.18f, 0f, -0.14f),
                HandL = new Vector3(-0.34f, 0.98f, -0.1f),
                HandR = new Vector3(0.2f, 1.3f, 0.7f),
                AimBlade = true,
                Blade = new Vector3(0f, -0.1f, 1f),
            };
            Looking(point);
            return clip.Key(0f, first ? Ready() : point).Key(hit * (first ? 0.4f : 0.5f), raised, Ease.Out).Key(hit * 0.9f, raised, Ease.Linear)
                .Key(hit, point, Ease.In).Key(1f, point, Ease.Linear);
        }

        // The roar that opens his second phase: he draws everything in, then throws his head back and his arms wide,
        // chest to the sky, and holds it while the storm answers. Nothing like the burst, so it is known at once.
        private static ForgeClip Roar(EnemyAttack attack)
        {
            var clip = Attack(attack, out float hit, out float done);
            var drawnIn = new ForgePose
            {
                Hips = new Vector3(0f, -0.34f, -0.04f),
                HipLean = 22f,
                WaistLean = 34f,
                HeadLean = 30f,
                FootL = new Vector3(-0.22f, 0f, 0.04f),
                FootR = new Vector3(0.22f, 0f, -0.04f),
                HandL = new Vector3(-0.16f, 0.9f, 0.34f),
                HandR = new Vector3(0.2f, 0.86f, 0.36f),
                ElbowL = new Vector3(-0.8f, -0.6f, 0f),
                ElbowR = new Vector3(0.8f, -0.6f, 0f),
                ShoulderLForward = 22f,
                ShoulderRForward = 22f,
                AimBlade = true,
                Blade = new Vector3(0.3f, -0.9f, 0.3f),
            };
            var tighter = drawnIn.Copy();
            tighter.Hips = new Vector3(0f, -0.4f, -0.05f);
            tighter.WaistLean = 40f;
            var thrown = new ForgePose
            {
                Hips = new Vector3(0f, 0.03f, 0.06f),
                HipLean = -14f,
                WaistLean = -30f,
                HeadLean = -42f,
                FootL = new Vector3(-0.34f, 0f, 0.02f),
                FootR = new Vector3(0.34f, 0f, -0.02f),
                FootLTurn = 24f,
                FootRTurn = 24f,
                HandL = new Vector3(-0.92f, 1.0f, -0.24f),
                HandR = new Vector3(0.94f, 1.02f, -0.22f),
                ElbowL = new Vector3(-0.6f, -0.3f, -1f),
                ElbowR = new Vector3(0.6f, -0.3f, -1f),
                ShoulderLUp = 14f,
                ShoulderRUp = 14f,
                ShoulderLForward = -24f,
                ShoulderRForward = -24f,
                AimBlade = true,
                Blade = new Vector3(0.82f, -0.5f, -0.28f),
            };
            var held = thrown.Copy();
            held.Hips = new Vector3(0f, 0.05f, 0.07f);
            held.WaistLean = -34f;
            held.HeadLean = -46f;
            held.HandL = new Vector3(-0.98f, 1.08f, -0.3f);
            held.HandR = new Vector3(1f, 1.1f, -0.28f);
            return clip.Key(0f, Ready()).Key(hit * 0.4f, drawnIn, Ease.Out).Key(hit * 0.88f, tighter, Ease.InOut).Key(hit, thrown, Ease.In)
                .Key(done, held, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), thrown, Ease.InOut).Key(1f, Ready(), Ease.InOut);
        }

        // The burst: he turns the sword point-down in both hands, lifts it, and drives it into the ground.
        private static ForgeClip Burst(EnemyAttack attack)
        {
            var clip = Attack(attack, out float hit, out float done);
            var lift = new ForgePose
            {
                Hips = new Vector3(0f, 0.02f, -0.04f),
                WaistLean = -10f,
                HeadLean = -10f,
                FootL = new Vector3(-0.2f, 0f, 0.06f),
                FootR = new Vector3(0.2f, 0f, -0.06f),
                HandR = new Vector3(0.06f, 1.9f, 0.3f),
                ElbowR = new Vector3(1f, 0.3f, 0.2f),
                ElbowL = new Vector3(-1f, 0.3f, 0.2f),
                ShoulderRUp = 26f,
                ShoulderLUp = 26f,
                AimBlade = true,
                TwoHanded = true,
                GripGap = -0.16f,
                Blade = new Vector3(0f, -1f, 0.06f),
            };
            var higher = lift.Copy();
            higher.HandR = new Vector3(0.06f, 2.0f, 0.3f);
            higher.Hips = new Vector3(0f, 0.04f, -0.05f);
            var driven = new ForgePose
            {
                Hips = new Vector3(0f, -0.42f, 0.06f),
                HipLean = 14f,
                WaistLean = 28f,
                HeadLean = -16f,
                FootL = new Vector3(-0.24f, 0f, 0.12f),
                FootR = new Vector3(0.24f, 0f, -0.1f),
                HandR = new Vector3(0.04f, 1.02f, 0.46f),
                ElbowR = new Vector3(1f, 0.1f, 0f),
                ElbowL = new Vector3(-1f, 0.1f, 0f),
                AimBlade = true,
                TwoHanded = true,
                GripGap = -0.16f,
                Blade = new Vector3(0f, -1f, 0f),
            };
            var held = driven.Copy();
            held.Hips = new Vector3(0f, -0.45f, 0.06f);
            return clip.Key(0f, Ready()).Key(hit * 0.45f, lift, Ease.Out).Key(hit * 0.9f, higher, Ease.InOut).Key(hit, driven, Ease.In)
                .Key(done, held, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.55f), held, Ease.Linear).Key(1f, Ready(), Ease.InOut);
        }
    }
}

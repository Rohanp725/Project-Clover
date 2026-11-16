using System.Collections.Generic;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.EditorTools.Forge
{
    /// <summary>
    /// The animations of the Gate Captain. He wears the soldier's body but does not stand like one: where they level
    /// the spear at whoever comes, he carries it slantwise across his chest and waits. Every attack is told from above
    /// by the shoulders winding one way and unwinding the other, the spear crossing the body, and the body sinking and
    /// rising. Each attack in his list gets its own clip, timed from that attack's numbers; attacks that share a motion
    /// name the motion to reuse.
    /// </summary>
    public static class CaptainAnimations
    {
        public static IEnumerable<ForgeClip> For(EnemyKind kind)
        {
            foreach (var clip in EnemyAnimations.CommonClips("captain", Ready, 0.22f, 1.0f, Kneel())) yield return clip;
            foreach (var attack in kind.Brain.Attacks)
            {
                bool chained = !string.IsNullOrEmpty(attack.ChainInto);
                switch (attack.AnimationName)
                {
                    case "captain_thrust_1": yield return FirstThrust(attack); break;
                    case "captain_thrust_2": yield return SecondThrust(attack, Stuck()); break;
                    case "captain_follow": yield return SecondThrust(attack, SweptThrough()); break;
                    case "captain_sweep": yield return Sweep(attack, chained); break;
                    case "captain_jab": yield return Jab(attack); break;
                    case "captain_slam": yield return Slam(attack); break;
                    // The charge that follows another starts from the skid the first one ended in.
                    case "captain_charge": yield return Charge(attack, attack.Name.EndsWith("_back") ? Skid() : Ready(), chained); break;
                    default: Debug.LogWarning($"[Clover.Forge] No motion written for the captain's attack '{attack.AnimationName}'."); break;
                }
            }
        }

        private static ForgePose Looking(ForgePose pose, float keep = 0.8f)
        {
            pose.HeadYaw = -(pose.HipYaw + pose.WaistYaw) * keep;
            return pose;
        }

        private static ForgeClip Attack(EnemyAttack attack, out float hit, out float done)
        {
            hit = attack.Telegraph / attack.Total;
            done = (attack.Telegraph + attack.Strike) / attack.Total;
            float h = hit, d = done;
            return new ForgeClip(attack.Name, attack.Total)
            {
                Shape = ForgeClip.Impact(h, attack.Total, 1.3f),
                Wind = t => new Vector3(0f, 0f, t > h * 0.8f && t < d ? -8f : 0f),
                SheetMoments = new[] { 0f, h * 0.45f, h * 0.9f, h, (h + d) * 0.5f, d, Mathf.Lerp(d, 1f, 0.45f), 1f },
            };
        }

        // Port arms: upright and square, the spear slantwise across the chest in both hands with its point high over
        // the left shoulder. From above it is a bar laid across him, where a soldier's is a line pointing at you.
        private static ForgePose Ready() => Looking(new ForgePose
        {
            Hips = new Vector3(0f, -0.08f, 0f),
            HipYaw = 22f,
            WaistYaw = 6f,
            WaistLean = 3f,
            FootL = new Vector3(-0.18f, 0f, 0.22f),
            FootR = new Vector3(0.22f, 0f, -0.2f),
            FootLTurn = 0f,
            FootRTurn = 44f,
            HandR = new Vector3(0.34f, 0.94f, 0.1f),
            ElbowR = new Vector3(0.8f, -0.2f, -0.6f),
            ElbowL = new Vector3(-0.8f, -0.5f, 0.2f),
            AimBlade = true,
            TwoHanded = true,
            GripGap = -0.5f,
            Blade = new Vector3(-0.55f, 0.74f, 0.38f),
        });

        // Broken open: down on the right knee, hanging off the spear planted in front of him.
        private static ForgePose Kneel() => new ForgePose
        {
            Hips = new Vector3(0f, -0.5f, 0f),
            HipLean = 10f,
            WaistLean = 28f,
            HeadLean = 32f,
            FootL = new Vector3(-0.18f, 0f, 0.28f),
            FootR = new Vector3(0.2f, 0f, -0.3f),
            FootRTip = 70f,
            KneeR = new Vector3(0.3f, -0.4f, 1f),
            HandR = new Vector3(0.2f, 1.02f, 0.36f),
            ElbowR = new Vector3(1f, -0.4f, 0f),
            ElbowL = new Vector3(-1f, -0.4f, 0f),
            AimBlade = true,
            TwoHanded = true,
            GripGap = 0.3f,
            Blade = new Vector3(-0.1f, 1f, 0.1f),
        };

        // ------------------------------------------------------------------ the two thrusts

        // Where the first thrust ends, and so where the second begins: weight forward, spear out, shoulders turned through.
        private static ForgePose Stuck()
        {
            var stuck = Ready();
            stuck.Hips = new Vector3(-0.04f, -0.26f, 0.3f);
            stuck.HipYaw = 4f;
            stuck.HipLean = 12f;
            stuck.WaistYaw = -40f;
            stuck.WaistLean = 20f;
            stuck.FootL = new Vector3(-0.16f, 0f, 0.66f);
            stuck.FootR = new Vector3(0.22f, 0f, -0.28f);
            stuck.FootRTip = 30f;
            stuck.HandR = new Vector3(-0.04f, 1.1f, 0.84f);
            stuck.ElbowR = new Vector3(1f, -0.3f, 0f);
            stuck.ShoulderRForward = 28f;
            stuck.GripGap = -0.26f;
            stuck.Blade = new Vector3(-0.02f, -0.03f, 1f);
            Looking(stuck);
            stuck.HeadLean = 8f;
            return stuck;
        }

        // The first thrust: the point comes down off his shoulder as the rear hand is drawn to the hip and the
        // shoulders wind to the right, then the spear is shot out level. It does not come back: the second follows.
        private static ForgeClip FirstThrust(EnemyAttack attack)
        {
            var clip = Attack(attack, out float hit, out float done);
            var draw = Ready();
            draw.Hips = new Vector3(0.06f, -0.2f, -0.16f);
            draw.HipYaw = 50f;
            draw.WaistYaw = 36f;
            draw.WaistLean = 2f;
            draw.WaistRoll = -6f;
            draw.HandR = new Vector3(0.42f, 0.94f, -0.44f);
            draw.ElbowR = new Vector3(0.8f, 0.3f, -1f);
            draw.ShoulderRUp = 10f;
            draw.GripGap = -0.6f;
            draw.Blade = new Vector3(-0.3f, 0.08f, 0.95f);
            Looking(draw);
            var held = draw.Copy();
            held.Hips = new Vector3(0.08f, -0.28f, -0.24f);
            held.WaistYaw = 48f;
            held.HandR = new Vector3(0.46f, 0.9f, -0.58f);
            Looking(held);
            var drive = Stuck();
            drive.Hips = new Vector3(-0.03f, -0.2f, 0.22f);
            drive.WaistYaw = -28f;
            drive.WaistLean = 16f;
            drive.HandR = new Vector3(0f, 1.14f, 0.7f);
            Looking(drive);
            drive.HeadLean = 10f;
            return clip.Key(0f, Ready()).Key(hit * 0.5f, draw, Ease.Out).Key(hit * 0.9f, held, Ease.InOut).Key(hit, drive, Ease.In)
                .Key(done, Stuck(), Ease.Out).Key(1f, Stuck(), Ease.Linear);
        }

        // The second thrust, out of whatever came before it: the spear is hauled back high, over the right shoulder,
        // with the whole body rocked onto the back foot and the back half turned to the target. That hold is the
        // beat a frightened roll is spent in. Then it comes down and through, overhand, long and low.
        private static ForgeClip SecondThrust(EnemyAttack attack, ForgePose from)
        {
            var clip = Attack(attack, out float hit, out float done);
            var haul = Ready();
            haul.Hips = new Vector3(0.1f, -0.1f, -0.22f);
            haul.HipYaw = 58f;
            haul.WaistYaw = 52f;
            haul.WaistLean = -8f;
            haul.WaistRoll = -8f;
            haul.FootL = new Vector3(-0.16f, 0f, 0.4f);
            haul.FootR = new Vector3(0.24f, 0f, -0.3f);
            haul.HandR = new Vector3(0.5f, 1.62f, -0.4f);
            haul.ElbowR = new Vector3(1f, 0.3f, -0.6f);
            haul.ElbowL = new Vector3(-0.6f, -0.4f, 0.6f);
            haul.ShoulderRUp = 26f;
            haul.GripGap = -0.5f;
            haul.Blade = new Vector3(-0.42f, -0.22f, 0.88f);
            Looking(haul, 0.75f);
            var top = haul.Copy();
            top.Hips = new Vector3(0.12f, -0.04f, -0.3f);
            top.WaistYaw = 62f;
            top.WaistLean = -12f;
            top.HandR = new Vector3(0.54f, 1.7f, -0.52f);
            top.Blade = new Vector3(-0.46f, -0.3f, 0.84f);
            Looking(top, 0.75f);
            var drive = Ready();
            drive.Hips = new Vector3(-0.06f, -0.4f, 0.46f);
            drive.HipYaw = -4f;
            drive.HipLean = 22f;
            drive.WaistYaw = -50f;
            drive.WaistLean = 30f;
            drive.WaistRoll = 8f;
            drive.FootL = new Vector3(-0.16f, 0f, 0.92f);
            drive.FootR = new Vector3(0.22f, 0f, -0.3f);
            drive.FootRTip = 44f;
            drive.HandR = new Vector3(-0.1f, 0.98f, 0.98f);
            drive.ElbowR = new Vector3(1f, 0f, 0f);
            drive.ShoulderRForward = 34f;
            drive.GripGap = -0.24f;
            drive.Blade = new Vector3(0f, -0.14f, 0.99f);
            Looking(drive);
            drive.HeadLean = -18f;
            var sunk = drive.Copy();
            sunk.Hips = new Vector3(-0.07f, -0.48f, 0.54f);
            sunk.WaistYaw = -60f;
            sunk.WaistLean = 34f;
            sunk.HandR = new Vector3(-0.14f, 0.9f, 1.06f);
            Looking(sunk);
            sunk.HeadLean = -12f;
            // Open: he drags the spear back to him and stands up into his guard a hand at a time.
            var drag = Ready();
            drag.Hips = new Vector3(0.02f, -0.24f, 0.22f);
            drag.HipYaw = 30f;
            drag.WaistYaw = 20f;
            drag.WaistLean = 14f;
            drag.FootL = new Vector3(-0.16f, 0f, 0.6f);
            drag.FootR = new Vector3(0.22f, 0f, -0.1f);
            drag.HandR = new Vector3(0.36f, 0.86f, 0.0f);
            drag.GripGap = -0.56f;
            drag.Blade = new Vector3(-0.3f, 0.16f, 0.94f);
            Looking(drag);
            drag.HeadLean = 14f;
            return clip.Key(0f, from).Key(hit * 0.5f, haul, Ease.Out).Key(hit * 0.9f, top, Ease.InOut).Key(hit, drive, Ease.In)
                .Key(done, sunk, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), drag, Ease.InOut).Key(1f, Ready(), Ease.InOut);
        }

        // ------------------------------------------------------------------ the wide cut

        // Where the cut ends: turned right round to his right, the spear out level on that side.
        private static ForgePose SweptThrough()
        {
            var through = Ready();
            through.Hips = new Vector3(0.1f, -0.34f, 0.12f);
            through.HipYaw = 54f;
            through.WaistYaw = 58f;
            through.WaistLean = 10f;
            through.WaistRoll = -10f;
            through.FootL = new Vector3(-0.2f, 0f, 0.44f);
            through.FootR = new Vector3(0.26f, 0f, -0.22f);
            through.HandR = new Vector3(0.6f, 1.0f, -0.08f);
            through.ElbowR = new Vector3(0.8f, -0.2f, -0.6f);
            through.GripGap = -0.45f;
            through.Blade = new Vector3(0.96f, -0.04f, 0.02f);
            Looking(through, 0.55f);
            return through;
        }

        // The wide cut: he turns his back on the target to carry the spear right round behind his left shoulder,
        // rising as he does, then drops and scythes it level from his left to his right through more than a half
        // circle, the shoulders unwinding through the blow and on past it.
        private static ForgeClip Sweep(EnemyAttack attack, bool chained)
        {
            var clip = Attack(attack, out float hit, out float done);
            var wound = Ready();
            wound.Hips = new Vector3(-0.08f, 0.0f, -0.12f);
            wound.HipYaw = -38f;
            wound.WaistYaw = -64f;
            wound.WaistLean = -4f;
            wound.WaistRoll = 10f;
            wound.FootL = new Vector3(-0.22f, 0f, 0.1f);
            wound.FootR = new Vector3(0.2f, 0f, -0.1f);
            wound.HandR = new Vector3(-0.2f, 1.4f, 0.08f);
            wound.ElbowR = new Vector3(0.8f, -0.3f, 0.4f);
            wound.ShoulderRUp = 16f;
            wound.GripGap = -0.45f;
            wound.Blade = new Vector3(-0.78f, 0.3f, -0.55f);
            Looking(wound, 0.65f);
            var held = wound.Copy();
            held.Hips = new Vector3(-0.1f, -0.1f, -0.16f);
            held.WaistYaw = -76f;
            held.HandR = new Vector3(-0.24f, 1.36f, 0.0f);
            held.Blade = new Vector3(-0.62f, 0.26f, -0.74f);
            Looking(held, 0.65f);
            var across = Ready();
            across.Hips = new Vector3(0f, -0.38f, 0.16f);
            across.HipYaw = 14f;
            across.WaistYaw = 6f;
            across.WaistLean = 18f;
            across.FootL = new Vector3(-0.2f, 0f, 0.44f);
            across.FootR = new Vector3(0.26f, 0f, -0.22f);
            across.HandR = new Vector3(0.16f, 0.98f, 0.4f);
            across.GripGap = -0.45f;
            across.Blade = new Vector3(-0.12f, -0.03f, 1f);
            Looking(across);
            var past = SweptThrough();
            past.Hips = new Vector3(0.12f, -0.22f, 0.04f);
            past.HipYaw = 62f;
            past.WaistYaw = 70f;
            past.WaistRoll = -4f;
            past.HandR = new Vector3(0.62f, 0.94f, -0.32f);
            past.Blade = new Vector3(0.68f, -0.16f, -0.7f);
            Looking(past, 0.5f);
            // A cut that runs on into a thrust stays turned; one that ends the exchange swings past and comes back to guard.
            if (chained)
                return clip.Key(0f, Ready()).Key(hit * 0.5f, wound, Ease.Out).Key(hit * 0.88f, held, Ease.InOut).Key(hit, across, Ease.In)
                    .Key(done, SweptThrough(), Ease.Linear).Key(1f, SweptThrough(), Ease.Linear);
            return clip.Key(0f, Ready()).Key(hit * 0.5f, wound, Ease.Out).Key(hit * 0.88f, held, Ease.InOut).Key(hit, across, Ease.In)
                .Key(done, SweptThrough(), Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.4f), past, Ease.Out).Key(1f, Ready(), Ease.InOut);
        }

        // ------------------------------------------------------------------ the short stab

        // For whoever stands on his toes: no wind-up to speak of. He rocks back half a step with the point lifted,
        // and stabs short and downward with the spear choked up, the right shoulder punching through.
        private static ForgeClip Jab(EnemyAttack attack)
        {
            var clip = Attack(attack, out float hit, out float done);
            var rock = Ready();
            rock.Hips = new Vector3(0.05f, -0.02f, -0.16f);
            rock.HipYaw = 40f;
            rock.WaistYaw = 30f;
            rock.WaistLean = -8f;
            rock.HandR = new Vector3(0.42f, 1.2f, -0.2f);
            rock.ShoulderRUp = 14f;
            rock.GripGap = -0.4f;
            rock.Blade = new Vector3(-0.4f, 0.5f, 0.76f);
            Looking(rock);
            var stab = Ready();
            stab.Hips = new Vector3(-0.03f, -0.26f, 0.24f);
            stab.HipYaw = 0f;
            stab.HipLean = 10f;
            stab.WaistYaw = -38f;
            stab.WaistLean = 22f;
            stab.FootL = new Vector3(-0.18f, 0f, 0.5f);
            stab.HandR = new Vector3(0.04f, 1.14f, 0.62f);
            stab.ElbowR = new Vector3(1f, 0f, -0.2f);
            stab.ShoulderRForward = 26f;
            stab.GripGap = -0.32f;
            stab.Blade = new Vector3(-0.04f, -0.36f, 0.93f);
            Looking(stab);
            stab.HeadLean = -8f;
            var set = stab.Copy();
            set.Hips = new Vector3(-0.04f, -0.32f, 0.28f);
            set.WaistYaw = -46f;
            set.HandR = new Vector3(0f, 1.06f, 0.7f);
            Looking(set);
            var back = Ready();
            back.Hips = new Vector3(0.02f, -0.14f, 0.08f);
            back.HipYaw = 30f;
            back.WaistYaw = 16f;
            back.HandR = new Vector3(0.36f, 1.0f, 0.02f);
            back.Blade = new Vector3(-0.44f, 0.4f, 0.8f);
            Looking(back);
            return clip.Key(0f, Ready()).Key(hit * 0.7f, rock, Ease.Out).Key(hit, stab, Ease.In)
                .Key(done, set, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.5f), back, Ease.InOut).Key(1f, Ready(), Ease.InOut);
        }

        // ------------------------------------------------------------------ the leap

        // The leap: he folds right down over his knees with the spear trailed behind him, goes up at full stretch with
        // it reared over his head in both hands, and comes down on one knee driving the point into the stones. The game
        // carries him across the ground while he is in the air; here he only rises and falls.
        private static ForgeClip Slam(EnemyAttack attack)
        {
            var clip = Attack(attack, out float hit, out float done);
            var fold = Ready();
            fold.Hips = new Vector3(0.02f, -0.5f, -0.14f);
            fold.HipYaw = 36f;
            fold.HipLean = 30f;
            fold.WaistYaw = 24f;
            fold.WaistLean = 26f;
            fold.FootL = new Vector3(-0.22f, 0f, 0.24f);
            fold.FootR = new Vector3(0.24f, 0f, -0.3f);
            fold.HandR = new Vector3(0.46f, 0.56f, -0.36f);
            fold.ElbowR = new Vector3(0.8f, 0.4f, -1f);
            fold.GripGap = -0.5f;
            fold.Blade = new Vector3(-0.3f, 0.3f, 0.9f);
            Looking(fold);
            fold.HeadLean = -34f;
            var coiled = fold.Copy();
            coiled.Hips = new Vector3(0.02f, -0.6f, -0.2f);
            coiled.HipLean = 36f;
            coiled.WaistLean = 32f;
            coiled.HandR = new Vector3(0.48f, 0.5f, -0.46f);
            var air = new ForgePose
            {
                Hips = new Vector3(0f, 0.7f, 0.1f),
                HipLean = -10f,
                HipYaw = 16f,
                WaistLean = -26f,
                WaistYaw = 14f,
                HeadLean = 18f,
                FootL = new Vector3(-0.16f, 0.62f, 0.2f),
                FootR = new Vector3(0.18f, 0.78f, -0.4f),
                FootLTip = 50f,
                FootRTip = 70f,
                HandR = new Vector3(0.22f, 2.5f, -0.2f),
                ElbowR = new Vector3(1f, 0.2f, 0.4f),
                ElbowL = new Vector3(-1f, 0.2f, 0.4f),
                ShoulderRUp = 30f,
                ShoulderLUp = 30f,
                AimBlade = true,
                TwoHanded = true,
                GripGap = -0.4f,
                Blade = new Vector3(-0.1f, 0.5f, -0.86f),
            };
            var land = new ForgePose
            {
                Hips = new Vector3(0f, -0.5f, 0.2f),
                HipLean = 30f,
                HipYaw = -8f,
                WaistLean = 34f,
                WaistYaw = -16f,
                HeadLean = -40f,
                FootL = new Vector3(-0.24f, 0f, 0.56f),
                FootR = new Vector3(0.24f, 0f, -0.3f),
                FootRTip = 60f,
                KneeR = new Vector3(0.3f, -0.4f, 1f),
                HandR = new Vector3(0.1f, 0.84f, 0.6f),
                ElbowR = new Vector3(1f, 0.3f, -0.2f),
                ElbowL = new Vector3(-1f, 0.3f, -0.2f),
                AimBlade = true,
                TwoHanded = true,
                GripGap = -0.4f,
                Blade = new Vector3(0f, -0.82f, 0.58f),
            };
            var sunk = land.Copy();
            sunk.Hips = new Vector3(0f, -0.58f, 0.24f);
            sunk.WaistLean = 40f;
            sunk.HeadLean = -30f;
            sunk.HandR = new Vector3(0.1f, 0.76f, 0.62f);
            // He has to wrench it out of the paving before he can stand: that is the opening.
            var wrench = Ready();
            wrench.Hips = new Vector3(0.04f, -0.36f, 0.08f);
            wrench.HipYaw = 30f;
            wrench.WaistYaw = 26f;
            wrench.WaistLean = 12f;
            wrench.FootL = new Vector3(-0.22f, 0f, 0.46f);
            wrench.FootR = new Vector3(0.24f, 0f, -0.24f);
            wrench.HandR = new Vector3(0.4f, 1.0f, 0.1f);
            wrench.GripGap = -0.45f;
            wrench.Blade = new Vector3(-0.2f, -0.4f, 0.9f);
            Looking(wrench);
            float h = hit, d = done;
            // The scarf and the surcoat are left behind going up and thrown up coming down.
            clip.Wind = t => new Vector3(0f, t > h * 0.62f && t < h * 0.88f ? -8f : t >= h * 0.88f && t < h ? 8f : 0f, t > h * 0.62f && t < d ? -7f : 0f);
            clip.SheetMoments = new[] { 0f, h * 0.4f, h * 0.62f, h * 0.86f, h, d, Mathf.Lerp(d, 1f, 0.5f), 1f };
            return clip.Key(0f, Ready()).Key(hit * 0.36f, fold, Ease.Out).Key(hit * 0.62f, coiled, Ease.InOut).Key(hit * 0.86f, air, Ease.Out)
                .Key(hit, land, Ease.In).Key(done, sunk, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.55f), wrench, Ease.InOut).Key(1f, Ready(), Ease.InOut);
        }

        // ------------------------------------------------------------------ the red charge

        // Where the charge leaves him: the front foot dug in, the body swung half round it, the spear still out.
        private static ForgePose Skid()
        {
            var skid = Ready();
            skid.Hips = new Vector3(0.08f, -0.5f, 0.5f);
            skid.HipYaw = 44f;
            skid.HipLean = 24f;
            skid.WaistYaw = 16f;
            skid.WaistLean = 20f;
            skid.FootL = new Vector3(-0.16f, 0f, 0.96f);
            skid.FootR = new Vector3(0.38f, 0f, -0.1f);
            skid.FootRTip = 30f;
            skid.FootRTurn = 60f;
            skid.HandR = new Vector3(0.38f, 0.74f, 0.6f);
            skid.ElbowR = new Vector3(0.5f, 0.2f, -1f);
            skid.GripGap = -0.4f;
            skid.Blade = new Vector3(-0.24f, 0.08f, 0.97f);
            Looking(skid);
            skid.HeadLean = -28f;
            return skid;
        }

        // The red charge. First the warning nobody else in the castle gives: he stands to his full height and lifts
        // the spear upright over his head. Then he drops behind it, couches it under his arm, and goes the length of
        // the lane laid out long and low. The game drives him forward from a little before the blow.
        private static ForgeClip Charge(EnemyAttack attack, ForgePose from, bool chained)
        {
            var clip = Attack(attack, out float hit, out float done);
            var salute = Ready();
            salute.Hips = new Vector3(0f, 0.03f, -0.06f);
            salute.HipYaw = 10f;
            salute.WaistYaw = 4f;
            salute.WaistLean = -10f;
            salute.FootL = new Vector3(-0.16f, 0f, 0.12f);
            salute.FootR = new Vector3(0.18f, 0f, -0.1f);
            salute.HandR = new Vector3(0.24f, 1.7f, 0.12f);
            salute.ElbowR = new Vector3(1f, 0f, 0.2f);
            salute.ElbowL = new Vector3(-1f, 0f, 0.2f);
            salute.ShoulderRUp = 24f;
            salute.ShoulderLUp = 18f;
            salute.GripGap = 0.34f;
            salute.Blade = new Vector3(0f, 1f, -0.06f);
            Looking(salute);
            salute.HeadLean = -10f;
            var couch = Ready();
            couch.Hips = new Vector3(0.05f, -0.46f, -0.24f);
            couch.HipYaw = 32f;
            couch.HipLean = 18f;
            couch.WaistYaw = 6f;
            couch.WaistLean = 32f;
            couch.FootL = new Vector3(-0.2f, 0f, 0.36f);
            couch.FootR = new Vector3(0.28f, 0f, -0.46f);
            couch.HandR = new Vector3(0.32f, 0.72f, -0.22f);
            couch.ElbowR = new Vector3(0.5f, 0.2f, -1f);
            couch.GripGap = -0.42f;
            couch.Blade = new Vector3(-0.1f, -0.05f, 0.99f);
            Looking(couch);
            couch.HeadLean = -32f;
            var shake = couch.Copy();
            shake.Hips = new Vector3(0.05f, -0.52f, -0.3f);
            shake.WaistLean = 36f;
            shake.HandR = new Vector3(0.33f, 0.68f, -0.27f);
            var charge = Ready();
            charge.Hips = new Vector3(0f, -0.46f, 0.54f);
            charge.HipYaw = 16f;
            charge.HipLean = 38f;
            charge.WaistYaw = -6f;
            charge.WaistLean = 30f;
            charge.FootL = new Vector3(-0.16f, 0f, 1.02f);
            charge.FootR = new Vector3(0.22f, 0f, -0.3f);
            charge.FootRTip = 60f;
            charge.FootRTurn = 20f;
            charge.HandR = new Vector3(0.26f, 0.74f, 0.76f);
            charge.ElbowR = new Vector3(0.5f, 0.2f, -1f);
            charge.GripGap = -0.4f;
            charge.Blade = new Vector3(-0.14f, 0.02f, 1f);
            Looking(charge);
            charge.HeadLean = -46f;
            // Still running: the other foot through, so the long drive is strides and not a slide.
            var stride = charge.Copy();
            stride.Hips = new Vector3(0f, -0.4f, 0.5f);
            stride.HipYaw = 4f;
            stride.FootL = new Vector3(-0.16f, 0f, -0.2f);
            stride.FootLTip = 50f;
            stride.FootR = new Vector3(0.2f, 0f, 0.9f);
            stride.FootRTip = 0f;
            stride.FootRTurn = 8f;
            stride.HandR = new Vector3(0.24f, 0.78f, 0.8f);
            var spent = Skid();
            spent.Hips = new Vector3(0.1f, -0.44f, 0.46f);
            spent.HipYaw = 56f;
            spent.WaistYaw = 24f;
            spent.WaistLean = 36f;
            spent.HandR = new Vector3(0.42f, 0.6f, 0.46f);
            spent.Blade = new Vector3(-0.42f, -0.16f, 0.9f);
            Looking(spent);
            spent.HeadLean = 12f;
            var rise = Ready();
            rise.Hips = new Vector3(0.02f, -0.06f, 0.22f);
            rise.WaistLean = -4f;
            rise.FootL = new Vector3(-0.16f, 0f, 0.54f);
            rise.FootR = new Vector3(0.24f, 0f, -0.06f);
            rise.HandR = new Vector3(0.32f, 1.1f, 0.1f);
            rise.Blade = new Vector3(-0.4f, 0.6f, 0.7f);
            Looking(rise);
            float h = hit, d = done;
            clip.SheetMoments = new[] { 0f, h * 0.28f, h * 0.6f, h * 0.84f, h, d, Mathf.Lerp(d, 1f, 0.4f), 1f };
            clip.Wind = t => new Vector3(0f, 0f, t > h * 0.76f && t < d ? -12f : 0f);
            clip.Key(0f, from).Key(hit * 0.28f, salute, Ease.Out).Key(hit * 0.42f, salute, Ease.Linear).Key(hit * 0.62f, couch, Ease.InOut)
                .Key(hit * 0.77f, shake, Ease.InOut).Key(hit * 0.86f, stride, Ease.In).Key(hit, charge, Ease.InOut).Key(done, Skid(), Ease.Linear);
            // The first of two ends where the second begins; a charge that ends the exchange leaves him bent over it.
            if (chained) return clip.Key(1f, Skid(), Ease.Linear);
            return clip.Key(Mathf.Lerp(done, 1f, 0.3f), spent, Ease.Out).Key(Mathf.Lerp(done, 1f, 0.68f), rise, Ease.InOut).Key(1f, Ready(), Ease.InOut);
        }
    }
}

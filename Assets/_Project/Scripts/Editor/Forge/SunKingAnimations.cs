using System.Collections.Generic;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.EditorTools.Forge
{
    /// <summary>
    /// How Khepharu the man moves (fire_land.md, section 6.2), drawn for him and not borrowed: an old king who stands
    /// very straight with the sun-spear grounded beside him like a staff of office, and does not hurry. He fights at
    /// the spear's far end: long level thrusts from behind the left hand, a reach that lets the shaft slide to its
    /// butt, the haft swept low, a short jab of the butt, the spear thrown like a javelin and called back to an open
    /// hand, driven point-down into the floor, and lifted to the sky to bring the noon down.
    /// </summary>
    public static class SunKingAnimations
    {
        private static Vector3 V(float x, float y, float z) => new Vector3(x, y, z);
        private static EnemyAttack Named(EnemyKind kind, string name) => kind.Brain.Attacks.Find(a => a.Name == name);
        private static ForgeClip Attack(EnemyKind kind, string name, out float hit, out float done) => EnemyAnimations.AttackClip(Named(kind, name), out hit, out done);

        // Upright, feet near together, the spear grounded at his right like a staff, the left hand at the small of the back.
        private static ForgePose Ready() => new ForgePose
        {
            Hips = V(0f, -0.01f, 0f),
            HipYaw = 8f,
            WaistLean = -5f,
            HeadLean = -6f,
            HeadYaw = -8f,
            FootL = V(-0.12f, 0f, 0.1f),
            FootR = V(0.14f, 0f, -0.06f),
            FootLTurn = 4f,
            FootRTurn = 22f,
            HandR = V(0.46f, 1.3f, 0.2f),
            ElbowR = V(1f, -0.4f, -0.3f),
            AimBlade = true,
            Blade = V(0.02f, 1f, 0.02f),
            HandL = V(-0.12f, 1.0f, -0.2f),
            ElbowL = V(-1f, -0.2f, -0.5f),
        };

        // The spear levelled in both hands at the hip, side-on, the point toward whoever comes.
        private static ForgePose Level()
        {
            var p = Ready();
            p.Hips = V(0.02f, -0.12f, -0.04f);
            p.HipYaw = 38f;
            p.WaistYaw = 10f;
            p.WaistLean = 2f;
            p.HeadYaw = -44f;
            p.HeadLean = 0f;
            p.FootL = V(-0.14f, 0f, 0.34f);
            p.FootR = V(0.2f, 0f, -0.26f);
            p.FootRTurn = 50f;
            p.HandR = V(0.36f, 1.08f, -0.2f);
            p.ElbowR = V(0.7f, 0.1f, -1f);
            p.Blade = V(-0.16f, 0.03f, 1f);
            p.TwoHanded = true;
            p.GripGap = 0.55f;
            return p;
        }

        // Driven out: the back leg straight, the whole length of him behind the point.
        private static ForgePose Driven(float reach, float low)
        {
            var p = Level();
            p.Hips = V(-0.02f, -0.3f - low, 0.4f);
            p.HipYaw = 2f;
            p.HipLean = 16f;
            p.WaistYaw = -34f;
            p.WaistLean = 22f;
            p.HeadYaw = 28f;
            p.HeadLean = 6f;
            p.FootL = V(-0.15f, 0f, 0.86f);
            p.FootR = V(0.22f, 0f, -0.36f);
            p.FootRTip = 34f;
            p.HandR = V(0.06f, 1.16f - low, 0.9f + reach);
            p.ElbowR = V(1f, -0.3f, 0f);
            p.ShoulderRForward = 34f;
            p.Blade = V(-0.02f, -0.03f, 1f);
            p.GripGap = 0.45f;
            return p;
        }

        private static ForgePose Kneel()
        {
            var p = Ready();
            // Down on one knee, held up by the spear, the crown bowed.
            p.Hips = V(0.04f, -0.52f, 0f);
            p.HipYaw = 0f;
            p.HipLean = 8f;
            p.WaistLean = 28f;
            p.HeadLean = 32f;
            p.HeadYaw = 0f;
            p.FootL = V(-0.18f, 0f, 0.28f);
            p.FootR = V(0.18f, 0f, -0.3f);
            p.FootRTip = 70f;
            p.KneeR = V(0.3f, -0.4f, 1f);
            p.HandR = V(0.34f, 1.05f, 0.36f);
            p.Blade = V(-0.04f, 1f, 0.08f);
            p.HandL = V(-0.3f, 0.55f, 0.3f);
            return p;
        }

        public static IEnumerable<ForgeClip> For(EnemyKind kind)
        {
            foreach (var clip in EnemyAnimations.CommonClips("sunking", Ready, 0.22f, 1.05f, Kneel()))
            {
                if (clip.Name != "sunking_idle_loop")
                {
                    yield return clip;
                    continue;
                }
                // Still as his own statue. The weight settles on the spear and comes off it; the head turns slowly, listening.
                yield return new ForgeClip("sunking_idle_loop", 4.2f, true).From(t =>
                {
                    float a = t * Mathf.PI * 2f;
                    float settle = 0.5f - 0.5f * Mathf.Cos(a);
                    var p = Ready();
                    p.Hips += V(0.025f * settle, -0.014f * settle + 0.006f * Mathf.Sin(a * 2f), 0f);
                    p.HipRoll = -2.5f * settle;
                    p.WaistLean += 1.5f * Mathf.Sin(a * 2f);
                    p.ShoulderRUp = -4f * settle;
                    p.HeadYaw += 10f * Mathf.Sin(a);
                    p.HeadLean += 3f * Mathf.Sin(a * 2f + 1f);
                    p.Blade = V(0.02f + 0.03f * settle, 1f, 0.02f);
                    return p;
                });
            }

            // The thrusts: drawn back level past the hip, held a breath, and driven.
            ForgeClip Thrust(string name, float reach, float low, bool follows)
            {
                var clip = Attack(kind, name, out float hit, out float done);
                var draw = Level();
                draw.Hips = V(0.05f, -0.2f, -0.2f);
                draw.HipYaw = 54f;
                draw.WaistYaw = 28f;
                draw.HandR = V(0.5f, 1.12f - low, -0.56f);
                draw.Blade = V(-0.28f, 0.04f, 0.96f);
                var held = draw.Copy();
                held.Hips = V(0.07f, -0.27f, -0.28f);
                held.WaistYaw = 40f;
                held.HandR = V(0.54f, 1.08f - low, -0.7f);
                var drive = Driven(reach, low);
                var stuck = drive.Copy();
                stuck.Hips += V(0f, -0.05f, 0.08f);
                stuck.WaistYaw = -44f;
                stuck.HandR += V(-0.02f, -0.04f, 0.1f);
                clip.Key(0f, follows ? Driven(0f, 0f) : Ready()).Key(hit * (follows ? 0.4f : 0.5f), draw, Ease.Out).Key(hit * 0.86f, held, Ease.InOut).Key(hit, drive, Ease.In).Key(done, stuck, Ease.Out);
                if (clip.Seconds - done * clip.Seconds < 0.2f) return clip.Key(1f, stuck, Ease.Linear);
                return clip.Key(Mathf.Lerp(done, 1f, 0.45f), stuck, Ease.Linear).Key(Mathf.Lerp(done, 1f, 0.8f), Level(), Ease.InOut).Key(1f, Ready(), Ease.InOut);
            }
            yield return Thrust("sunking_thrust_1", 0f, 0f, false);
            yield return Thrust("sunking_thrust_2", 0.1f, 0.1f, true);
            yield return Thrust("sunking_thrust_3", 0.22f, 0.16f, true);
            // Below half the long thrust is made from standing, and the sweep follows it.
            yield return Thrust("sunking_drive", 0.22f, 0.16f, false);

            // The reach: one hand, the shaft let slide to its butt, the body thrown flat along the line and the left arm flung back.
            {
                var clip = Attack(kind, "sunking_reach", out float hit, out float done);
                var coil = Level();
                coil.Hips = V(0.06f, -0.3f, -0.26f);
                coil.HipYaw = 60f;
                coil.WaistYaw = 34f;
                coil.WaistLean = 10f;
                coil.TwoHanded = false;
                coil.HandR = V(0.5f, 1.3f, -0.6f);
                coil.Blade = V(-0.2f, -0.08f, 0.97f);
                coil.HandL = V(-0.3f, 1.2f, 0.6f);
                var flung = Driven(0.3f, 0.2f);
                flung.TwoHanded = false;
                flung.Hips = V(-0.04f, -0.46f, 0.6f);
                flung.WaistLean = 38f;
                flung.WaistYaw = -58f;
                flung.FootL = V(-0.15f, 0f, 1.1f);
                flung.FootR = V(0.24f, 0f, -0.5f);
                flung.HandR = V(0f, 1.0f, 1.5f);
                flung.ShoulderRForward = 44f;
                flung.Blade = V(0f, -0.06f, 1f);
                flung.HandL = V(-0.5f, 1.1f, -0.7f);
                flung.ElbowL = V(-1f, 0.2f, -0.6f);
                var hang = flung.Copy();
                hang.Hips += V(0f, -0.04f, 0.06f);
                clip.Key(0f, Ready()).Key(hit * 0.5f, coil, Ease.Out).Key(hit * 0.88f, coil, Ease.Linear).Key(hit, flung, Ease.In).Key(done, hang, Ease.Out)
                    .Key(Mathf.Lerp(done, 1f, 0.5f), hang, Ease.Linear).Key(1f, Ready(), Ease.InOut);
                yield return clip;
            }

            // The sweep: the haft carried out to his left and swung flat and low through half a circle by the turn of the trunk.
            ForgeClip Sweep(string name, bool follows)
            {
                var clip = Attack(kind, name, out float hit, out float done);
                var wind = Level();
                wind.Hips = V(-0.04f, -0.22f, -0.06f);
                wind.HipYaw = -24f;
                wind.WaistYaw = -60f;
                wind.WaistLean = 10f;
                wind.HeadYaw = 54f;
                wind.HandR = V(-0.34f, 0.9f, 0.3f);
                wind.ElbowR = V(0.4f, -0.2f, -1f);
                wind.ShoulderRForward = 30f;
                wind.Blade = V(-0.96f, -0.14f, 0.24f);
                var front = Level();
                front.Hips = V(0f, -0.3f, 0.1f);
                front.HipYaw = 10f;
                front.WaistYaw = 0f;
                front.WaistLean = 16f;
                front.HeadYaw = 0f;
                front.HandR = V(0.1f, 0.8f, 0.5f);
                front.Blade = V(0f, -0.2f, 1f);
                var through = Level();
                through.Hips = V(0.06f, -0.26f, 0.06f);
                through.HipYaw = 40f;
                through.WaistYaw = 62f;
                through.WaistLean = 12f;
                through.HeadYaw = -40f;
                through.HandR = V(0.6f, 0.86f, 0.24f);
                through.ElbowR = V(1f, -0.2f, -0.2f);
                through.Blade = V(0.97f, -0.16f, 0.16f);
                return clip.Key(0f, follows ? Driven(0.22f, 0.16f) : Ready()).Key(hit * 0.55f, wind, Ease.Out).Key(hit * 0.9f, wind, Ease.Linear).Key(Mathf.Lerp(hit, done, 0.45f), front, Ease.In).Key(done, through, Ease.Out)
                    .Key(Mathf.Lerp(done, 1f, 0.5f), through, Ease.Linear).Key(1f, Ready(), Ease.InOut);
            }
            yield return Sweep("sunking_sweep", false);
            yield return Sweep("sunking_drive_sweep", true);

            // The butt: the point goes up and back over his shoulder and the foot of the shaft is punched forward, short.
            {
                var clip = Attack(kind, "sunking_butt", out float hit, out float done);
                var cock = Ready();
                cock.Hips = V(0f, -0.08f, -0.08f);
                cock.HipYaw = 24f;
                cock.HandR = V(0.4f, 1.45f, -0.1f);
                cock.Blade = V(0.05f, 0.8f, -0.6f);
                cock.TwoHanded = true;
                cock.GripGap = 0.5f;
                var punch = cock.Copy();
                punch.Hips = V(0f, -0.16f, 0.22f);
                punch.HipYaw = -6f;
                punch.WaistLean = 14f;
                punch.WaistYaw = -20f;
                punch.FootL = V(-0.14f, 0f, 0.5f);
                punch.HandR = V(0.2f, 1.5f, 0.3f);
                punch.Blade = V(0.04f, 0.5f, -0.86f);
                clip.Key(0f, Ready()).Key(hit * 0.8f, cock, Ease.Out).Key(hit, punch, Ease.In).Key(done, punch, Ease.Linear).Key(1f, Ready(), Ease.InOut);
                yield return clip;
            }

            // The throw: a javelin. Side-on, the spear back over the shoulder, the left hand pointing down the lane; then the whole arm over.
            {
                var clip = Attack(kind, "sunking_throw", out float hit, out float done);
                var back = Ready();
                back.Hips = V(0.04f, -0.14f, -0.24f);
                back.HipYaw = 70f;
                back.WaistYaw = 30f;
                back.WaistLean = -14f;
                back.HeadYaw = -80f;
                back.FootL = V(-0.14f, 0f, 0.5f);
                back.FootR = V(0.2f, 0f, -0.4f);
                back.FootRTurn = 70f;
                back.HandR = V(0.5f, 1.95f, -0.6f);
                back.ElbowR = V(1f, 0.3f, -0.6f);
                back.ShoulderRUp = 30f;
                back.Blade = V(-0.14f, 0.12f, 0.98f);
                back.HandL = V(-0.2f, 1.5f, 0.8f);
                back.ElbowL = V(-1f, -0.2f, 0f);
                var further = back.Copy();
                further.Hips = V(0.05f, -0.2f, -0.32f);
                further.WaistLean = -20f;
                further.HandR = V(0.54f, 1.9f, -0.76f);
                var cast = Driven(0.1f, 0f);
                cast.TwoHanded = false;
                cast.Hips = V(-0.03f, -0.3f, 0.44f);
                cast.WaistLean = 34f;
                cast.WaistYaw = -52f;
                cast.HandR = V(0.1f, 1.3f, 1.2f);
                cast.ShoulderRForward = 40f;
                cast.Blade = V(0f, -0.1f, 1f);
                cast.HandL = V(-0.5f, 0.9f, -0.5f);
                var spent = cast.Copy();
                spent.Hips = V(-0.03f, -0.36f, 0.5f);
                spent.HandR = V(0f, 0.8f, 0.9f);
                spent.Blade = V(0f, -0.5f, 0.86f);
                clip.Key(0f, Ready()).Key(hit * 0.5f, back, Ease.Out).Key(hit * 0.9f, further, Ease.InOut).Key(hit, cast, Ease.In).Key(1f, spent, Ease.Out);
                yield return clip;
            }

            // The ray: empty-handed, he straightens, steps round side-on and lifts an open hand to the sky; the spear comes back to it.
            {
                var clip = Attack(kind, "sunking_ray", out float hit, out float done);
                var bare = Ready();
                bare.Hips = V(0f, -0.04f, 0f);
                bare.HipYaw = 30f;
                bare.WaistLean = -8f;
                bare.HandR = V(0.5f, 1.0f, 0.3f);
                bare.Blade = V(0f, 0.3f, 0.95f);
                bare.HandL = V(-0.4f, 1.0f, 0.3f);
                var calls = bare.Copy();
                calls.Hips = V(0f, 0f, -0.04f);
                calls.WaistLean = -16f;
                calls.HeadLean = -20f;
                calls.HandR = V(0.4f, 2.15f, 0.24f);
                calls.ElbowR = V(1f, 0.2f, 0.1f);
                calls.ShoulderRUp = 34f;
                calls.Blade = V(0f, 0.2f, 0.98f);
                calls.HandL = V(-0.6f, 1.2f, 0.1f);
                var caught = calls.Copy();
                caught.Hips = V(0f, -0.14f, -0.2f);
                caught.WaistLean = -22f;
                caught.WaistYaw = 20f;
                caught.HandR = V(0.46f, 1.9f, -0.2f);
                caught.FootR = V(0.2f, 0f, -0.34f);
                clip.Key(0f, bare).Key(hit * 0.45f, bare, Ease.Linear).Key(hit * 0.62f, calls, Ease.Out).Key(hit, calls, Ease.Linear).Key(done, caught, Ease.Snap).Key(1f, Ready(), Ease.InOut);
                yield return clip;
            }

            // The plant: the spear lifted point-down in both hands over his head, and driven into the floor.
            {
                var clip = Attack(kind, "sunking_plant", out float hit, out float done);
                var high = Ready();
                high.Hips = V(0f, 0.02f, -0.06f);
                high.HipYaw = 0f;
                high.WaistLean = -16f;
                high.HeadLean = -24f;
                high.HeadYaw = 0f;
                high.FootL = V(-0.2f, 0f, 0.1f);
                high.FootR = V(0.2f, 0f, -0.1f);
                high.HandR = V(0.1f, 2.1f, 0.3f);
                high.ElbowR = V(1f, 0.2f, 0f);
                high.ShoulderRUp = 34f;
                high.Blade = V(0f, -1f, 0.1f);
                high.TwoHanded = true;
                high.GripGap = -0.3f;
                var down = high.Copy();
                down.Hips = V(0f, -0.36f, 0.12f);
                down.WaistLean = 30f;
                down.HeadLean = 20f;
                down.FootL = V(-0.26f, 0f, 0.2f);
                down.FootR = V(0.26f, 0f, -0.2f);
                down.HandR = V(0.06f, 1.0f, 0.5f);
                down.ShoulderRUp = 0f;
                down.Blade = V(0f, -1f, 0.04f);
                var pull = down.Copy();
                pull.Hips = V(0f, -0.2f, 0.04f);
                pull.WaistLean = 12f;
                pull.HandR = V(0.1f, 1.3f, 0.44f);
                clip.Key(0f, Ready()).Key(hit * 0.5f, high, Ease.Out).Key(hit * 0.9f, high, Ease.Linear).Key(hit, down, Ease.In).Key(done, down, Ease.Linear)
                    .Key(Mathf.Lerp(done, 1f, 0.6f), pull, Ease.InOut).Key(1f, Ready(), Ease.InOut);
                yield return clip;
            }

            // The noon: the spear lifted straight to the sky in one hand, the face turned up to it; then tipped at where the shaft will fall.
            ForgeClip Noon(string attackName, bool first)
            {
                var clip = Attack(kind, attackName, out float hit, out float done);
                var lifted = Ready();
                lifted.Hips = V(0f, 0.02f, -0.04f);
                lifted.WaistLean = -14f;
                lifted.HeadLean = -30f;
                lifted.HeadYaw = 0f;
                lifted.HandR = V(0.4f, 2.1f, 0.06f);
                lifted.ElbowR = V(1f, 0.2f, 0.2f);
                lifted.ShoulderRUp = 34f;
                lifted.Blade = V(0.06f, 1f, -0.04f);
                lifted.HandL = V(-0.5f, 1.3f, 0.3f);
                lifted.ElbowL = V(-1f, -0.3f, 0f);
                var tipped = lifted.Copy();
                tipped.Hips = V(0f, -0.08f, 0.08f);
                tipped.WaistLean = 8f;
                tipped.HeadLean = 4f;
                tipped.HandR = V(0.34f, 1.9f, 0.4f);
                tipped.Blade = V(0f, 0.7f, 0.7f);
                tipped.HandL = V(-0.4f, 1.1f, 0.5f);
                return clip.Key(0f, first ? Ready() : tipped).Key(hit * (first ? 0.45f : 0.5f), lifted, Ease.Out).Key(hit * 0.9f, lifted, Ease.Linear)
                    .Key(hit, tipped, Ease.In).Key(1f, tipped, Ease.Out);
            }
            yield return Noon("sunking_noon_1", true);
            yield return Noon("sunking_noon_2", false);
            yield return Noon("sunking_noon_3", false);
        }
    }
}

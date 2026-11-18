using Clover.Core;
using Clover.World;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The Sun-Bearer's body and its sun as things in the court (fire_land.md 6.4, reworked on 2026-11-18): nothing of
    /// either shape or of the sun passes into the wall or the obelisk, or under the floor where it lies; the loose sun
    /// rolls when struck and sets the shell alight when rolled into it, with a line on the floor to aim by; the shell
    /// heats under someone who camps on its flank, and answers them; and no lane of fire is laid that would leave the
    /// player no way to the shape.
    /// </summary>
    public partial class SunBearer
    {
        private readonly RollingSun rolling = new RollingSun();
        private GroundMark aimMark;
        private float shellFireLeft, untilShellFlame;
        private ScorchedGround shellFire;
        private bool saidShellFire;
        // Flank-camping: seconds the player has stood on the flank, how hot the shell is there, and when it spat last.
        private float campFor, campHeat, untilCampEmber;
        // Without the sun in its hold its shell is cool and takes blows in full.
        private bool shellCool;
        private readonly Vector3[] corner = new Vector3[8];
        private static readonly float[] Asides = { 3f, -3f, 6f, -6f };
        private static readonly float[] Turns = { 25f, -25f, 50f, -50f };

        // ------------------------------------------------------------------ inside the court

        // The court's wall is three metres high on the north, east and west and 1.2 on the south (the forecourt's mouth
        // has none, and is counted as low): above it a flying thing is over the wall, not in it.
        private static float WallTop(Vector3 point)
        {
            float fromSouth = Mathf.Abs(Mathf.DeltaAngle(Mathf.Atan2(point.x, point.z) * Mathf.Rad2Deg, 180f));
            return fromSouth < 75f ? 1.2f : 3f;
        }

        // How far a point is inside the obelisk (2.4 m square at the foot, 1.4 at 13 m, 14.4 tall), with a margin.
        private static float InStone(Vector3 point, float margin)
        {
            if (point.y > 14.6f || point.y < -0.5f) return 0f;
            float half = Mathf.Lerp(1.2f, 0.7f, Mathf.Clamp01(point.y / 13f)) + margin;
            float inX = half - Mathf.Abs(point.x), inZ = half - Mathf.Abs(point.z);
            return inX > 0f && inZ > 0f ? Mathf.Min(inX, inZ) : 0f;
        }

        // Measures every piece of the pose as it stands (the corners of each piece's own box) and moves the whole body back
        // by the worst reach past the wall or into the stone; a body lying on the floor is lifted out of it; a flying
        // falcon climbs over the wall rather than being pulled in. The sun is kept inside the same way, and a sun held by
        // the scarab takes the scarab with it. True when anything moved.
        private bool KeepInside()
        {
            if (mode == Mode.Hidden || mode == Mode.Entering || mode == Mode.Dying || mode == Mode.Dead) return false;
            bool moved = false;
            if (!underground && body.gameObject.activeInHierarchy)
            {
                bool flying = falcon && at.y > 2.5f && mode != Mode.Exposed && mode != Mode.Staggered;
                // Nothing of it goes through the paving: the falcon's wings, the scarab on its back, rearing on its tail or
                // down on its face. Only digging in, coming up, and the ram's horn in the floor go into it on purpose.
                bool sinking = mode == Mode.Burrow || at.y < -0.05f || (mode == Mode.Ram && struck);
                bool lying = !sinking;
                float outMost = 0f, stoneMost = 0f, lowest = float.MaxValue, climb = 0f;
                Vector3 outWay = Vector3.zero, stoneWay = Vector3.zero;
                // A margin, since a skinned wing's box is the one it had at the last frame drawn.
                float rim = FloorRadius - 0.5f;
                foreach (var shown in renderers)
                {
                    if (shown == null || !shown.enabled || !shown.gameObject.activeInHierarchy) continue;
                    Corners(shown);
                    for (int i = 0; i < 8; i++)
                    {
                        Vector3 point = corner[i];
                        float top = WallTop(point) + 0.3f;
                        if (point.y < top)
                        {
                            float out_ = new Vector2(point.x, point.z).magnitude - rim;
                            if (out_ > outMost)
                            {
                                outMost = out_;
                                outWay = OnFloor(point).normalized;
                            }
                            if (out_ > 0f && top - point.y > climb) climb = top - point.y;
                        }
                        float stone = InStone(point, 0.3f);
                        if (stone > stoneMost)
                        {
                            stoneMost = stone;
                            stoneWay = OnFloor(point).sqrMagnitude > 0.0001f ? OnFloor(point).normalized : Facing(yaw);
                        }
                        if (point.y < lowest) lowest = point.y;
                    }
                }
                if (outMost > 0.001f)
                {
                    if (flying) at.y += climb;
                    else at -= outWay * outMost;
                    moved = true;
                }
                // Into the obelisk, whether walking or flying low over its point: out the way it came, sideways.
                if (stoneMost > 0.001f)
                {
                    at += stoneWay * stoneMost;
                    moved = true;
                }
                // Lying on its back, on its face or with its wings spread, it lies on the paving and not in it.
                if (lying && lowest < 0.05f)
                {
                    at.y += 0.05f - lowest;
                    moved = true;
                }
            }
            // The sun: a ball, kept inside the wall while it is low, clear of the stone at any height the stone stands,
            // and on the floor, not in it (unless it is going under with the burrowing scarab).
            if (sunState != SunState.Under && sunState != SunState.High && parryLeft < 0f && sun.gameObject.activeInHierarchy && !(mode == Mode.Burrow && underground == false && withSun && at.y < -0.05f))
            {
                float radius = 1.5f * sunSize;
                Vector3 flat = OnFloor(sunAt);
                float out_ = flat.magnitude;
                Vector3 shift = Vector3.zero;
                if (sunAt.y < 6f && out_ + radius > FloorRadius - 0.2f && out_ > 0.01f) shift = -flat / out_ * (out_ + radius - (FloorRadius - 0.2f));
                if (sunAt.y - radius < 14.6f)
                {
                    // The stone is square: the nearest point of its square at the sun's height, and out from it.
                    float half = Mathf.Lerp(1.2f, 0.7f, Mathf.Clamp01(sunAt.y / 13f)) + 0.1f;
                    var nearest = new Vector3(Mathf.Clamp(flat.x, -half, half), 0f, Mathf.Clamp(flat.z, -half, half));
                    Vector3 off = flat - nearest;
                    float gap = off.magnitude;
                    if (gap < radius) shift += (gap > 0.001f ? off / gap : (out_ > 0.01f ? flat / out_ : Vector3.forward)) * (radius - gap);
                }
                if (sunAt.y < radius - 0.02f && mode != Mode.Burrow) shift.y = radius - sunAt.y;
                if (shift.sqrMagnitude > 0.000001f)
                {
                    sunAt += shift;
                    rolling.Position = OnFloor(sunAt);
                    // What holds it is moved with it, on the floor's plane.
                    if (sunState == SunState.Held)
                    {
                        at += OnFloor(shift);
                        moved = true;
                    }
                    sun.position = sunAt;
                }
            }
            return moved;
        }

        // The eight corners of a piece's own box, where it stands now.
        private void Corners(Renderer shown)
        {
            var bounds = shown.localBounds;
            Matrix4x4 toWorld = shown is SkinnedMeshRenderer skinned && skinned.rootBone != null ? skinned.rootBone.localToWorldMatrix : shown.transform.localToWorldMatrix;
            Vector3 c = bounds.center, e = bounds.extents;
            for (int i = 0; i < 8; i++)
                corner[i] = toWorld.MultiplyPoint3x4(c + new Vector3((i & 1) == 0 ? -e.x : e.x, (i & 2) == 0 ? -e.y : e.y, (i & 4) == 0 ? -e.z : e.z));
        }

        // ------------------------------------------------------------------ the loose sun as a ball

        // Where its body lies as a thing the rolled sun can strike: the scarab's length from tail to horn, or the falcon on
        // the floor. False when it is not where a ball on the floor can reach it.
        private bool BodyOnFloor(out Vector3 tail, out Vector3 head, out float girth)
        {
            Vector3 flat = OnFloor(at);
            Vector3 forward = Facing(yaw);
            tail = head = flat;
            girth = 0f;
            if (underground || IsDead) return false;
            if (!falcon)
            {
                tail = flat - forward * 3f;
                head = flat + forward * 2.2f;
                girth = 1.9f;
                return at.y > -1.5f;
            }
            if (at.y > 4.5f) return false;
            girth = 2.6f;
            return true;
        }

        private Vector3 RollLooseSun(Vector3 flat, float deltaTime)
        {
            rolling.Position = flat;
            int bounces = rolling.Bounces;
            float went = rolling.Step(deltaTime);
            sunSpin += went / 1.5f * Mathf.Rad2Deg;
            if (rolling.Bounces != bounces)
            {
                // Off the wall or the stone: sand thrown up where it struck.
                Vfx.Play(VfxKind.Dust, rolling.Position + rolling.Position.normalized * 1.4f, -rolling.Position.normalized, 1.4f, Sand);
                if (shaker != null) shaker.Shake(0.2f, 0.15f);
            }
            if (went > 0.01f && Every(ref untilPuff, 0.9f, went)) Vfx.Play(VfxKind.Dust, rolling.Position - rolling.Velocity.normalized * 1.2f, Vector3.up, 0.9f, Sand);
            if (rolling.Hard && BodyOnFloor(out Vector3 tail, out Vector3 head, out float girth) && RollingSun.Meets(rolling.Position, 1.5f, tail, head, girth))
            {
                ShellCatches(rolling.Velocity);
                rolling.Rebound(0.3f);
            }
            return rolling.Position;
        }

        // Struck by its own rolled sun the shell catches fire: a heavy blow to its balance and some to its health.
        private void ShellCatches(Vector3 going)
        {
            SunStrikes++;
            shellFireLeft = ShellFireSeconds;
            flashLeft = 0.12f;
            Vector3 struckAt = OnFloor(sunAt) + Vector3.up * 1.4f;
            Vfx.Play(VfxKind.Impact, struckAt, going, 2.2f);
            Vfx.Play(VfxKind.Burst, at + Vector3.up * 1.8f, Vector3.up, 2.2f);
            for (int i = 0; i < 8; i++) Vfx.Play(VfxKind.Fire, at + Facing(yaw + i * 45f) * 1.6f + Vector3.up * 1.6f, Vector3.up, 1.8f);
            Vfx.Play(VfxKind.Smoke, at + Vector3.up * 2.8f, Vector3.up, 2f, Smoke);
            ScreenFlash.Flash(new Color(1f, 0.45f, 0.12f, 0.3f), 0.08f);
            if (shaker != null) shaker.Shake(0.55f, 0.35f);
            if (hitStop != null) hitStop.Freeze(0.08f);
            if (!saidShellFire && victimDefence != null)
            {
                saidShellFire = true;
                victimDefence.Callout("The shell catches fire");
            }
            Health.Damage(ShellFireDamage * Afflictions.DamageTakenMultiplier(false));
            if (Health.IsDead)
            {
                BeginDying();
                return;
            }
            if (!IsStaggered && Poise.Add(Stagger, ShellFireBalance)) BeginStagger();
        }

        private void AdvanceShellFire(float deltaTime)
        {
            if (shellFire != null)
            {
                // Flames standing all over the shell's back while it burns, dying down as it goes out.
                if (shellFireLeft > 0f && body.gameObject.activeInHierarchy)
                {
                    float heat = Mathf.Clamp01(shellFireLeft / ShellFireSeconds * 1.4f);
                    Vector3 back = at + Facing(yaw) * (falcon ? 0f : -0.6f) + Vector3.up * (falcon ? 1.4f : 2.2f + Mathf.Max(0f, pitch) * 0.02f);
                    shellFire.Disc(0, back, falcon ? 2.4f : 2f, heat, 1f, 0.95f, false);
                }
                else shellFire.HideDisc(0);
            }
            if (shellFireLeft <= 0f) return;
            shellFireLeft -= deltaTime;
            if (!Every(ref untilShellFlame, 0.07f, deltaTime)) return;
            // Flames run over the shell and smoke rolls off it.
            Vector3 forward = Facing(yaw), right = Facing(yaw + 90f);
            Vector3 on = at + forward * Random.Range(-2.6f, 1.4f) + right * Random.Range(-1.6f, 1.6f) + Vector3.up * Random.Range(1.4f, 2.6f);
            Vfx.Play(VfxKind.Fire, on, Vector3.up, 1.3f);
            if (Random.value < 0.35f) Vfx.Play(VfxKind.Smoke, on + Vector3.up * 0.8f, Vector3.up, 1f, Smoke);
        }

        // While the sun lies still and the player stands close to it, facing it, a short line is drawn on the floor from
        // its front the way a heavy blow would send it: ember red when it would roll into the shape.
        private void AdvanceAim()
        {
            if (aimMark == null) return;
            bool show = sunState == SunState.Loose && parryLeft < 0f && !rolling.Moving && sunRollLeft <= 0f && target != null && !IsDead;
            Vector3 face = Vector3.zero;
            if (show)
            {
                Vector3 toSun = OnFloor(sunAt) - PlayerOnFloor;
                face = OnFloor(target.forward);
                show = face.sqrMagnitude > 0.01f && toSun.magnitude <= AimReach && Vector3.Angle(face, toSun) <= AimDegrees;
            }
            if (!show)
            {
                if (Aiming) aimMark.Hide();
                Aiming = false;
                return;
            }
            Aiming = true;
            face.Normalize();
            Vector3 from = OnFloor(sunAt) + face * 1.6f;
            float length = Mathf.Max(0.5f, Mathf.Min(AimLength, StormDragon.Chord(from, face, FloorRadius - 1.9f)));
            bool meets = false;
            if (BodyOnFloor(out Vector3 tail, out Vector3 head, out float girth))
                for (float along = 0f; along <= length && !meets; along += 0.8f)
                    meets = RollingSun.Meets(from + face * along, 1.5f, tail, head, girth);
            aimMark.Show(from, Quaternion.LookRotation(face), 1f, meets ? HotMarker : PlainMarker, true, AimWidth, length / (LaneUnit * AimWidth), 0.75f);
        }

        // ------------------------------------------------------------------ flank-camping

        private bool CampRipe => campFor >= CampAfter + CampHeat && !underground && !falcon;

        // Someone standing close on its flank or behind it, not in front, for long enough heats the shell under them.
        private void AdvanceCamp(float deltaTime)
        {
            bool counts = !falcon && !underground && target != null && victim != null
                && (mode == Mode.Idle || mode == Mode.Sparks || mode == Mode.Flares || mode == Mode.Ricochet || mode == Mode.Roll || mode == Mode.Ram || mode == Mode.ShellBurst || mode == Mode.Kick || mode == Mode.Spent);
            bool on = false;
            if (counts && mode != Mode.ShellBurst && mode != Mode.Kick)
            {
                Vector3 forward = Facing(yaw);
                Vector3 flat = OnFloor(at), player = PlayerOnFloor;
                Vector3 tail = flat - forward * 3f, head = flat + forward * 1.6f;
                Vector3 line = head - tail;
                float t = Mathf.Clamp01(Vector3.Dot(player - tail, line) / line.sqrMagnitude);
                float off = (tail + line * t - player).magnitude - 1.9f;
                on = off <= CampReach && Vector3.Angle(forward, player - flat) > CampFrontDegrees;
            }
            campFor = on ? campFor + deltaTime : Mathf.Max(0f, campFor - deltaTime * 1.5f);
            campHeat = Mathf.Clamp01((campFor - CampAfter) / CampHeat);
            // Embers and a thread of smoke spit off the hot side, faster as it heats.
            if (campHeat > 0.05f && Every(ref untilCampEmber, Mathf.Lerp(0.3f, 0.07f, campHeat), deltaTime))
            {
                Vector3 side = OnFloor(PlayerOnFloor - at);
                side = side.sqrMagnitude > 0.01f ? side.normalized : Facing(yaw + 90f);
                Vector3 on_ = at + side * 1.7f + Vector3.up * Random.Range(1.3f, 2.4f) + Facing(yaw) * Random.Range(-2f, 1f);
                Vfx.Play(VfxKind.Embers, on_, side + Vector3.up, 0.6f + campHeat);
                if (campHeat > 0.5f && Random.value < 0.4f) Vfx.Play(VfxKind.Smoke, on_ + Vector3.up * 0.4f, Vector3.up, 0.6f, Smoke);
            }
        }

        // Behind it, it kicks out with its hind legs; on its flank, its shell flares in a ring.
        private void AnswerCamper()
        {
            Vector3 toPlayer = PlayerOnFloor - OnFloor(at);
            HideMarkers();
            if (Vector3.Angle(Facing(yaw), toPlayer) > CampBehindDegrees) Enter(Mode.Kick);
            else Enter(Mode.ShellBurst);
        }

        private void AdvanceShellBurst(float deltaTime)
        {
            float shake = Mathf.Sin(Time.time * 47f);
            if (!struck)
            {
                // The shell glows to its brightest and the cases lift as a pale ring fills round it: guard, or get out.
                float t = Mathf.Clamp01(elapsed / BurstTell);
                lift = Mathf.MoveTowards(lift, 0.35f, 2f * deltaTime);
                roll = shake * 2f * t;
                circles[1].Show(OnFloor(at) - Facing(yaw) * 0.6f, Quaternion.identity, t, PlainMarker, false, BurstRadius);
                MarkAt = OnFloor(at);
                Locked = true;
                if (elapsed < BurstTell) return;
                struck = true;
                ShellBursts++;
                circles[1].Hide();
                Vector3 middle = OnFloor(at) - Facing(yaw) * 0.6f;
                for (int i = 0; i < 14; i++)
                {
                    Vector3 way = Facing(i * 360f / 14f);
                    Vfx.Play(VfxKind.Fire, middle + way * 2.6f + Vector3.up * 0.4f, way + Vector3.up * 0.3f, 1.6f);
                }
                Vfx.Play(VfxKind.Ring, middle + Vector3.up * 0.4f, Vector3.up, 1.5f);
                Vfx.Play(VfxKind.Burst, at + Vector3.up * 2f, Vector3.up, 2.2f);
                if (shaker != null) shaker.Shake(0.4f, 0.3f);
                campFor = 0f;
                Vector3 offset = PlayerOnFloor - middle;
                if (offset.magnitude <= BurstRadius + 0.35f)
                {
                    blowName = "the shell flaring";
                    Vector3 away = offset.sqrMagnitude > 0.01f ? offset.normalized : Facing(yaw + 90f);
                    if (Deal(new Hit(BurstDamage, 0f, away, false, false, at), PerfectGuardBalance * 0.5f) == HitOutcome.Landed && victimBody != null)
                        victimBody.Shove(away * BurstShove, 0.3f);
                }
                return;
            }
            lift = Mathf.MoveTowards(lift, 0f, 1.5f * deltaTime);
            roll = Mathf.MoveTowards(roll, 0f, 60f * deltaTime);
            if (elapsed < BurstTell + 0.6f) return;
            EndAttack();
        }

        private void AdvanceKick(float deltaTime)
        {
            Vector3 back = OnFloor(at) - Facing(yaw) * 3.6f;
            if (!struck)
            {
                // The hind legs drawn up, the head down, a pale mark behind it.
                float t = Mathf.Clamp01(elapsed / KickTell);
                pitch = Mathf.MoveTowards(pitch, 12f, 60f * deltaTime);
                legPhase += 6f * deltaTime;
                circles[1].Show(back, Quaternion.identity, t, PlainMarker, false, 2.3f);
                MarkAt = back;
                Locked = true;
                if (elapsed < KickTell) return;
                struck = true;
                Kicks++;
                circles[1].Hide();
                pitch = -6f;
                legPhase += Mathf.PI;
                for (int i = 0; i < 5; i++) Vfx.Play(VfxKind.Dust, back + Facing(yaw + 180f + (i - 2) * 25f) * 1.6f, -Facing(yaw), 1.3f, Sand);
                if (shaker != null) shaker.Shake(0.35f, 0.25f);
                campFor = 0f;
                Vector3 offset = PlayerOnFloor - OnFloor(at);
                if ((PlayerOnFloor - back).magnitude <= KickReach && Vector3.Dot(offset, Facing(yaw)) < 0f)
                {
                    blowName = "the hind legs";
                    Vector3 away = (PlayerOnFloor - back).sqrMagnitude > 0.01f ? (PlayerOnFloor - back).normalized : -Facing(yaw);
                    if (Deal(new Hit(KickDamage, 0f, away, false, false, back), PerfectGuardBalance * 0.5f) == HitOutcome.Landed && victimBody != null)
                        victimBody.Shove(away * KickShove, 0.35f);
                }
                return;
            }
            pitch = Mathf.MoveTowards(pitch, 0f, 40f * deltaTime);
            if (elapsed < KickTell + 0.55f) return;
            EndAttack();
        }

        // ------------------------------------------------------------------ fire that leaves a way through

        // The roll's lane, once locked: if it would leave the player no way to where the scarab will be, it is laid a few
        // metres aside; if no such lane leaves a way either, the sun rolls down it leaving char and no fire.
        private void CheckRollLane()
        {
            laneChecked = true;
            laneUnlit = false;
            Vector3 goal = laneFrom + laneAlong * Mathf.Max(0f, laneLength - 6f);
            if (FireLeavesWay(laneFrom, laneAlong, laneLength, RollWidth, goal)) return;
            Vector3 along = laneAlong;
            float length = laneLength;
            foreach (float aside in Asides)
            {
                AimLane(aside);
                if (!FireLeavesWay(laneFrom, laneAlong, laneLength, RollWidth, laneFrom + laneAlong * Mathf.Max(0f, laneLength - 6f))) continue;
                LanesShifted++;
                return;
            }
            laneAlong = along;
            laneLength = length;
            laneUnlit = true;
            LanesUnlit++;
            Debug.Log($"[Clover.Gameplay] The Sun-Bearer's {Doing} lane from {laneFrom:0.0} along {laneAlong:0.00} was laid without fire: every lane it could lay would have shut the player at {PlayerOnFloor:0.0} off from it.");
        }

        private void CheckPassLane(float turned)
        {
            laneUnlit = false;
            Vector3 goal = Polar(flyAngle, 10f);
            if (FireLeavesWay(laneFrom, laneAlong, laneLength, PassWidth, goal)) return;
            Vector3 from = laneFrom, along = laneAlong;
            float length = laneLength;
            foreach (float more in Turns)
            {
                AimPass(turned + more);
                if (!FireLeavesWay(laneFrom, laneAlong, laneLength, PassWidth, goal)) continue;
                LanesShifted++;
                return;
            }
            laneFrom = from;
            laneAlong = along;
            laneLength = length;
            laneUnlit = true;
            LanesUnlit++;
            Debug.Log($"[Clover.Gameplay] The Sun-Bearer's pass lane was laid without fire: every lane it could lay would have shut the player at {PlayerOnFloor:0.0} off.");
        }
    }
}

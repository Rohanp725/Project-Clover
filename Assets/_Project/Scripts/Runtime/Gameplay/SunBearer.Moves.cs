using Clover.Core;
using Clover.Player;
using Clover.World;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The Sun-Bearer's newer attacks (fire_land.md 6.4, reworked on 2026-11-18), each with a mark drawn first, an
    /// answer, and where it makes sense an opening. The scarab: the Ricochet (the sun shoved at the wall, its whole
    /// bounce path drawn as lanes first; the shell is cool while the sun is away), the Flares (the sun planted, arcs of
    /// flame looping up off it and slamming down along lines that radiate from it). The falcon: the Ember Gust (a wall
    /// of burning wind across a pale band that shoves whoever it catches and whose guard does not hold), the Talon Grab
    /// (caught, carried up and dropped; dodged, it lands and is open), and the Sunfall (burning pieces of the sun
    /// hurled down on red circles). The Burrow is in SunBearer.Ground.
    /// </summary>
    public partial class SunBearer
    {
        // The Ricochet's path: where the sun starts, where it strikes the wall twice, where it stops; and how far along it the sun has gone.
        private readonly Vector3[] bounce = new Vector3[4];
        private readonly bool[] legStruck = new bool[3];
        private float ricochetAlong, ricochetTotal;
        private int ricochetLeg;
        private bool ricochetUnlit;
        // The Flares: the lines from the planted sun, their lengths, when each fires, and how many.
        private readonly Vector3[] flareFrom = new Vector3[5], flareAlong = new Vector3[5];
        private readonly float[] flareLength = new float[5], flareAt = new float[5];
        private readonly bool[] flareFired = new bool[5];
        private int flareCount;
        // The Gust: where its band starts and which way it blows, how long; how many gusts are left.
        private Vector3 gustFrom, gustAlong;
        private float gustLength, gustFront;
        private int gusts;
        private bool gustStruck;
        // The Grab: the player held in the talons, where it carries them from and to, how high they were let go, how fast they fall.
        private readonly Grip talons = new Grip { BiteDamage = 0f, BiteEvery = 99f, PressTakes = 0.18f };
        private bool carrying, grounded, falling;
        private Vector3 carryFrom, carryTo;
        private float dropHeight, fallSpeed;
        // The Sunfall: how many pieces, and how many are thrown.
        private int pieces, thrown;
        private static readonly float[] RicochetAngles = { 0f, 8f, -8f, 16f, -16f, 24f, -24f, 32f, -32f, 40f, -40f, 48f, -48f, 56f, -56f, 64f, -64f };
        private static readonly float[] FlareSpread = { -38f, 38f, 0f };
        private static readonly float[] FlareFurySpread = { -64f, -32f, 0f, 32f, 64f };

        // ------------------------------------------------------------------ the Ricochet

        private void BeginRicochet()
        {
            Enter(Mode.Ricochet);
            moveFrom = OnFloor(at);
            Vector3 flat = OnFloor(at);
            Vector3 out_ = flat.sqrMagnitude > 1f ? flat.normalized : Vector3.forward;
            laneFrom = out_ * (FloorRadius - 4.5f);
            prepare = Mathf.Clamp((laneFrom - flat).magnitude / 8f, 0.3f, 1.4f);
            ricochetUnlit = false;
            for (int i = 0; i < 3; i++) legStruck[i] = false;
            PlanRicochet();
        }

        // Where the sun goes from a start one way: to the wall, off it, to the wall again, off it, and on until it stops.
        // False when any stretch of it would pass the obelisk or be too short to read.
        private bool BouncePath(Vector3 start, Vector3 way, float rimAt)
        {
            bounce[0] = start;
            Vector3 point = start, going = way;
            for (int k = 1; k <= 3; k++)
            {
                float lead = point.x * going.x + point.z * going.z;
                float square = lead * lead - (point.x * point.x + point.z * point.z - rimAt * rimAt);
                if (square < 0f) return false;
                float run = -lead + Mathf.Sqrt(square);
                if (k == 3) run = Mathf.Min(run * 0.6f, 12f);
                if (run < 5f) return false;
                Vector3 next = point + going * run;
                if (PassesStone(point, next, 4f)) return false;
                bounce[k] = next;
                if (k == 3) break;
                Vector3 normal = OnFloor(next).normalized;
                going = (going - 2f * Vector3.Dot(going, normal) * normal).normalized;
                point = next;
            }
            return true;
        }

        // Of the ways it could shove the sun, the one whose path comes nearest the player, so the bounce is what finds them.
        private void PlanRicochet()
        {
            Vector3 inward = -OnFloor(laneFrom).normalized;
            Vector3 player = PlayerOnFloor;
            float best = float.MaxValue, bestAngle = 0f;
            float rimAt = FloorRadius - 1.9f;
            foreach (float angle in RicochetAngles)
            {
                Vector3 way = Quaternion.Euler(0f, angle, 0f) * inward;
                if (!BouncePath(laneFrom + way * 3.9f, way, rimAt)) continue;
                float nearest = float.MaxValue;
                for (int k = 0; k < 3; k++) nearest = Mathf.Min(nearest, DistanceToLine(player, bounce[k], bounce[k + 1]));
                // A path whose first stretch misses and whose bounce finds them is the better one.
                float score = nearest + (DistanceToLine(player, bounce[0], bounce[1]) < RicochetWidth ? 1.5f : 0f);
                if (score >= best) continue;
                best = score;
                bestAngle = angle;
            }
            laneAlong = Quaternion.Euler(0f, bestAngle, 0f) * inward;
            BouncePath(laneFrom + laneAlong * 3.9f, laneAlong, rimAt);
            ricochetTotal = 0f;
            for (int k = 0; k < 3; k++) ricochetTotal += (bounce[k + 1] - bounce[k]).magnitude;
        }

        private static float DistanceToLine(Vector3 point, Vector3 from, Vector3 to)
        {
            Vector3 line = OnFloor(to - from);
            float t = line.sqrMagnitude > 0.0001f ? Mathf.Clamp01(Vector3.Dot(OnFloor(point - from), line) / line.sqrMagnitude) : 0f;
            return (OnFloor(from) + line * t - OnFloor(point)).magnitude;
        }

        private Vector3 RicochetPoint(float along, out int leg)
        {
            for (leg = 0; leg < 3; leg++)
            {
                float length = (bounce[leg + 1] - bounce[leg]).magnitude;
                if (along <= length || leg == 2) return bounce[leg] + (bounce[leg + 1] - bounce[leg]) * Mathf.Clamp01(along / Mathf.Max(0.01f, length));
                along -= length;
            }
            leg = 2;
            return bounce[3];
        }

        private void ShowRicochetLanes(float fill, float passed)
        {
            float before = 0f;
            for (int k = 0; k < 3; k++)
            {
                Vector3 from = bounce[k], to = bounce[k + 1];
                float length = (to - from).magnitude;
                float gone = Mathf.Clamp(passed - before, 0f, length);
                before += length;
                if (gone >= length - 0.3f)
                {
                    lanes[k].Hide();
                    continue;
                }
                Vector3 way = (to - from) / Mathf.Max(0.01f, length);
                // Each stretch fills in its turn, so the path reads as one line that bounces.
                float own = Mathf.Clamp01(fill * 3f - k);
                lanes[k].Show(from + way * gone, Quaternion.LookRotation(way), own, RedMarker, true, RicochetWidth, (length - gone) / (LaneUnit * RicochetWidth));
            }
        }

        private void AdvanceRicochet(float deltaTime)
        {
            float tell = prepare + RicochetTell;
            if (elapsed < tell)
            {
                // To the rim, then braced behind the sun; the path follows the player until it locks.
                Vector3 stand = laneFrom - laneAlong * 0.5f;
                if (elapsed < prepare) Walk(stand, Mathf.Max(8f, (laneFrom - moveFrom).magnitude / prepare), deltaTime, false);
                else at = Vector3.MoveTowards(at, stand, 10f * deltaTime);
                Turn(laneAlong, 300f, deltaTime);
                if (elapsed < prepare + RicochetTell * 0.55f) PlanRicochet();
                else if (!Locked)
                {
                    Locked = true;
                    // The three stretches together must leave a way to where the scarab stands.
                    ReachOverHotFloor();
                    for (int k = 0; k < 3; k++)
                    {
                        Vector3 way = bounce[k + 1] - bounce[k];
                        reach.Lane(bounce[k], way.normalized, way.magnitude, RicochetWidth);
                    }
                    if (target != null && !reach.Reaches(PlayerOnFloor, stand))
                    {
                        ricochetUnlit = true;
                        LanesUnlit++;
                        Debug.Log($"[Clover.Gameplay] The Sun-Bearer's ricochet was laid without fire: its path would have shut the player at {PlayerOnFloor:0.0} off from it.");
                    }
                }
                sunAt = Vector3.MoveTowards(sunAt, HeldSunPoint(), 40f * deltaTime);
                pitch = Mathf.MoveTowards(pitch, elapsed > prepare ? 12f : 0f, 40f * deltaTime);
                ShowRicochetLanes(Mathf.Clamp01((elapsed - prepare * 0.5f) / (tell - prepare * 0.5f)), 0f);
                MarkAt = (bounce[1] + bounce[2]) * 0.5f;
                return;
            }
            if (step == 0)
            {
                // The shove: the scarab lunges and the sun leaves it, hard. Its shell is cool while the sun is away.
                step = 1;
                sunState = SunState.Left;
                shellCool = true;
                SetOpen(false, true);
                ricochetAlong = 0f;
                ricochetLeg = -1;
                // Head down into the shove.
                pitch = 12f;
                if (shaker != null) shaker.Shake(0.4f, 0.3f);
                Vfx.Play(VfxKind.Dust, OnFloor(at) + Facing(yaw) * 2f, Facing(yaw), 1.6f, Sand);
            }
            if (step == 1)
            {
                // Faster down the first stretches, slowing over the last until it stops.
                float left = ricochetTotal - ricochetAlong;
                float pace = RicochetSpeed * Mathf.Clamp(left / 10f, 0.25f, 1f) * Pace;
                ricochetAlong = Mathf.Min(ricochetTotal, ricochetAlong + pace * deltaTime);
                Vector3 point = RicochetPoint(ricochetAlong, out int leg);
                if (leg != ricochetLeg)
                {
                    // Off the wall: sand, a lick of fire and a jolt; and the next stretch begins to burn behind it.
                    if (ricochetLeg >= 0)
                    {
                        Grown(laneLength);
                        Vfx.Play(VfxKind.TintRing, point + Vector3.up * 0.3f, Vector3.up, 1f, Sand);
                        Vfx.Play(VfxKind.Fire, point + Vector3.up * 1.2f, Vector3.up, 2.4f);
                        for (int i = 0; i < 5; i++) Vfx.Play(VfxKind.Dust, point + Facing(i * 72f) * 1.6f, Vector3.up, 1.3f, Sand);
                        if (shaker != null) shaker.Shake(0.35f, 0.25f);
                    }
                    ricochetLeg = leg;
                    laneFrom = bounce[leg];
                    Vector3 way = bounce[leg + 1] - bounce[leg];
                    laneLength = way.magnitude;
                    laneAlong = way / Mathf.Max(0.01f, laneLength);
                    growing = LayLane(0.01f, RicochetWidth, ricochetUnlit ? 0.8f : RicochetHotSeconds, !ricochetUnlit);
                }
                Grown((point - laneFrom).magnitude);
                Vector3 was = sunAt;
                sunAt = point + Vector3.up * 1.5f;
                sunSpin += (sunAt - was).magnitude / 1.5f * Mathf.Rad2Deg;
                if (Every(ref untilPuff, 1f, (sunAt - was).magnitude)) BowWave(point, RicochetWidth, 1.3f);
                ShowRicochetLanes(1f, ricochetAlong);
                pitch = Mathf.MoveTowards(pitch, 0f, 40f * deltaTime);
                // The sun strikes whoever is on the stretch it is rolling down, once a stretch.
                if (!legStruck[leg] && target != null && (PlayerOnFloor - point).magnitude <= 1.5f + 0.6f)
                {
                    legStruck[leg] = true;
                    blowName = "the ricochet";
                    if (Deal(new Hit(RicochetDamage, 0f, laneAlong, false, true, sunAt), 0f) == HitOutcome.PerfectGuard)
                    {
                        HideMarkers();
                        KnockSun(laneAlong, true);
                        return;
                    }
                }
                if (ricochetAlong < ricochetTotal) return;
                step = 2;
                growing = -1;
                prepare = elapsed;
                for (int k = 0; k < 3; k++) lanes[k].Hide();
                Vfx.Play(VfxKind.TintRing, OnFloor(sunAt) + Vector3.up * 0.3f, Vector3.up, 1f, Sand);
                return;
            }
            // The sun lies where it stopped, still burning; the scarab is without it a moment longer, and open.
            if (elapsed - prepare < 0.8f) return;
            EndAttack();
            // EndAttack takes the cool shell away; it stays cool until the sun is back in its hold.
            shellCool = true;
        }

        // ------------------------------------------------------------------ the Flares

        private void BeginFlares()
        {
            Enter(Mode.Flares);
            flareCount = Furious ? 5 : 3;
            for (int i = 0; i < flareCount; i++) flareFired[i] = false;
            AimFlares();
        }

        // Lines radiating from where the sun will be planted, spread about the way to the player; in the fury five, swept
        // from one side to the other, so following the sweep is no escape.
        private void AimFlares()
        {
            Vector3 planted = OnFloor(at) + Facing(yaw) * 5.2f;
            Vector3 toPlayer = PlayerOnFloor - planted;
            float toward = toPlayer.sqrMagnitude > 0.25f ? YawOf(toPlayer) : yaw;
            var spread = Furious ? FlareFurySpread : FlareSpread;
            float every = Furious ? FlareFuryEvery : FlareEvery;
            bool reverse = Furious && Random.value < 0.5f;
            for (int i = 0; i < flareCount; i++)
            {
                float offset = spread[reverse ? flareCount - 1 - i : i];
                Vector3 way = Facing(toward + offset);
                flareFrom[i] = planted + way * 1.7f;
                flareAlong[i] = way;
                flareLength[i] = Mathf.Max(3f, Mathf.Min(FlareLength, Run(flareFrom[i], way, FloorRadius - 1f, 2.4f)));
                flareAt[i] = FlareFirstAt + i * every;
            }
        }

        private void AdvanceFlares(float deltaTime)
        {
            const float Plant = 0.6f;
            float last = flareAt[flareCount - 1];
            if (elapsed < Plant)
            {
                // It sets the sun down in front of it and the sun swells: the shell is cool without it.
                sunState = SunState.Left;
                shellCool = true;
                SetOpen(false, true);
                Vector3 planted = OnFloor(at) + Facing(yaw) * 5.2f + Vector3.up * (1.5f * 1.2f);
                sunAt = Vector3.MoveTowards(sunAt, planted, 8f * deltaTime);
                sunSize = Mathf.MoveTowards(sunSize, 1.2f, deltaTime);
                pitch = Mathf.MoveTowards(pitch, -8f, 40f * deltaTime);
                AimFlares();
                return;
            }
            sunSize = 1.2f + 0.05f * Mathf.Sin(FormTime * 19f);
            pitch = Mathf.MoveTowards(pitch, 0f, 30f * deltaTime);
            Locked = true;
            Vector3 top = sunAt + Vector3.up * 1.6f;
            for (int i = 0; i < flareCount; i++)
            {
                float until = flareAt[i] - elapsed;
                if (!flareFired[i])
                {
                    // Marked first, all at once; each fills to its own moment.
                    float fill = Mathf.Clamp01((elapsed - Plant) / (flareAt[i] - Plant));
                    lanes[i].Show(flareFrom[i], Quaternion.LookRotation(flareAlong[i]), fill, RedMarker, true, FlareWidth, flareLength[i] / (LaneUnit * FlareWidth));
                    // The arc of flame climbs off the sun over the last half second before it comes down.
                    if (look != null) look.FlareArc(i, top, flareFrom[i] + flareAlong[i] * flareLength[i], Mathf.Clamp01(1f - until / 0.5f), 0f);
                    if (until > 0f) continue;
                    flareFired[i] = true;
                    lanes[i].Hide();
                    SlamFlare(i);
                    continue;
                }
                // Slammed down: it lies along its line a moment, then is gone.
                float since = -until;
                if (look != null) look.FlareArc(i, top, flareFrom[i] + flareAlong[i] * flareLength[i], 1f, Mathf.Clamp01(since / 0.12f) + (since > 0.3f ? 1f : 0f));
            }
            MarkAt = flareFrom[0] + flareAlong[0] * (flareLength[0] * 0.5f);
            if (elapsed < last + 0.6f) return;
            // It takes the sun up again.
            sunState = SunState.Held;
            sunSize = Mathf.MoveTowards(sunSize, 1f, 2f * deltaTime);
            if (elapsed < last + 1.1f) return;
            sunSize = 1f;
            if (look != null) look.FlaresDone();
            EndAttack();
        }

        private void SlamFlare(int index)
        {
            Vector3 from = flareFrom[index], along = flareAlong[index];
            float length = flareLength[index];
            for (float run = 0.6f; run <= length; run += 1.3f)
            {
                Vector3 point = from + along * run + new Vector3(along.z, 0f, -along.x) * Random.Range(-0.4f, 0.4f);
                Vfx.Play(VfxKind.Fire, point + Vector3.up * 0.3f, Vector3.up, 1.5f);
                if (Random.value < 0.4f) Vfx.Play(VfxKind.Dust, point, Vector3.up, 1f, Sand);
            }
            Vfx.Play(VfxKind.Burst, from + along * length + Vector3.up * 0.5f, Vector3.up, 1.2f);
            if (shaker != null) shaker.Shake(0.3f, 0.2f);
            // A slash of flame along the line, burning a moment and leaving char; it does not bite after the slam.
            laneFrom = from;
            laneAlong = along;
            laneLength = length;
            LayLane(length, FlareWidth, 1.1f, false);
            if (target == null) return;
            Vector3 offset = PlayerOnFloor - from;
            float onLine = Vector3.Dot(offset, along);
            float across = Mathf.Abs(offset.x * along.z - offset.z * along.x);
            if (onLine < -0.3f || onLine > length + 0.3f || across > FlareWidth * 0.5f + 0.35f) return;
            blowName = "a flare";
            Deal(new Hit(FlaresDamage, 0f, new Vector3(along.z, 0f, -along.x), false, true, from + along * onLine), 0f);
        }

        // ------------------------------------------------------------------ the Ember Gust

        private void BeginGust()
        {
            Enter(Mode.Gust);
            gusts = Furious ? 2 : 1;
            AimGust(0f);
        }

        // From the far side of the court through the player: a band from the rim, as wide as a wingbeat.
        private void AimGust(float turned)
        {
            Vector3 player = PlayerOnFloor;
            Vector3 out_ = player.sqrMagnitude > 1f ? player.normalized : Vector3.back;
            gustFrom = Quaternion.Euler(0f, 180f + turned, 0f) * out_ * 15f;
            Vector3 way = OnFloor(player - gustFrom);
            gustAlong = way.sqrMagnitude > 0.01f ? way.normalized : -gustFrom.normalized;
            gustLength = StormDragon.Chord(gustFrom, gustAlong, FloorRadius - 0.5f);
            gustFront = -1f;
            gustStruck = false;
        }

        private void AdvanceGust(float deltaTime)
        {
            Vector3 hover = gustFrom - gustAlong * 2.5f + Vector3.up * 6.5f;
            if (elapsed < GustTell)
            {
                // It comes round to the far side, low, and rears its wings back for the beat. The band follows the player a while.
                if (elapsed < GustTell * 0.6f) AimGust(gusts < (Furious ? 2 : 1) ? 60f : 0f);
                Locked = elapsed >= GustTell * 0.6f;
                Fly(hover, 30f, deltaTime, false);
                Turn(gustAlong, 400f, deltaTime);
                pitch = Mathf.MoveTowards(pitch, -10f, 60f * deltaTime);
                roll = Mathf.MoveTowards(roll, 0f, 90f * deltaTime);
                flap = Mathf.MoveTowards(flap, 62f, 120f * deltaTime);
                lanes[0].Show(gustFrom, Quaternion.LookRotation(gustAlong), elapsed / GustTell, PlainMarker, true, GustWidth, gustLength / (LaneUnit * GustWidth));
                MarkAt = gustFrom + gustAlong * (gustLength * 0.5f);
                return;
            }
            if (gustFront < 0f)
            {
                // The great beat: the wings come down and the wall of burning wind goes out across the band.
                gustFront = 0f;
                lanes[0].Hide();
                if (shaker != null) shaker.Shake(0.45f, 0.35f);
                Vfx.Play(VfxKind.Dust, OnFloor(at), gustAlong, 2.4f, Sand);
            }
            flap = Mathf.MoveTowards(flap, -40f, 600f * deltaTime);
            gustFront += GustSpeed * deltaTime;
            if (gustFront <= gustLength + 2f)
            {
                Vector3 front = gustFrom + gustAlong * Mathf.Min(gustFront, gustLength);
                if (look != null) look.GustWall(true, front, gustAlong, GustWidth, 1f - Mathf.Clamp01((gustFront - gustLength) / 2f));
                Vector3 side = new Vector3(gustAlong.z, 0f, -gustAlong.x);
                if (Every(ref untilPuff, 0.04f, deltaTime))
                {
                    Vector3 spot = front + side * Random.Range(-GustWidth * 0.5f, GustWidth * 0.5f);
                    Vfx.Play(VfxKind.Embers, spot + Vector3.up * Random.Range(0.4f, 1.8f), gustAlong + Vector3.up * 0.2f, 1.2f);
                    Vfx.Play(VfxKind.Dust, spot + Vector3.up * 0.2f, gustAlong, 1.2f, Sand);
                    if (Random.value < 0.5f) Vfx.Play(VfxKind.Fire, spot + Vector3.up * 0.6f, gustAlong + Vector3.up, 0.9f);
                }
                if (!gustStruck && target != null)
                {
                    Vector3 offset = PlayerOnFloor - gustFrom;
                    float along = Vector3.Dot(offset, gustAlong);
                    float across = Mathf.Abs(offset.x * gustAlong.z - offset.z * gustAlong.x);
                    if (across <= GustWidth * 0.5f + 0.35f && along >= 0f && along <= gustFront)
                    {
                        gustStruck = true;
                        blowName = "the gust";
                        // A held guard holds its ground; anyone else is thrown along with the wind.
                        if (Deal(new Hit(GustDamage, 0f, gustAlong, false, false, at), 0f) == HitOutcome.Landed && victimBody != null)
                            victimBody.Shove(gustAlong * GustShove, 0.4f);
                    }
                }
                return;
            }
            if (look != null) look.GustWall(false, Vector3.zero, gustAlong, GustWidth, 0f);
            if (gusts > 1)
            {
                // In its fury the other wing beats back across, from round the court.
                gusts--;
                elapsed = GustTell * 0.45f;
                AimGust(60f);
                return;
            }
            flap = Mathf.Sin(FormTime * 9f) * 30f;
            if (gustFront < gustLength + 2f + GustSpeed * 0.35f) return;
            EndAttack();
        }

        // ------------------------------------------------------------------ the Talon Grab

        private void BeginGrab()
        {
            Enter(Mode.Grab);
            grounded = falling = false;
            markPoint = Clear(PlayerOnFloor, 4.6f, FloorRadius - 3f);
        }

        private void AdvanceGrab(float deltaTime)
        {
            float tell = GrabFollow + GrabHold, dive = 0.26f;
            if (elapsed < tell)
            {
                // Its shadow over the player tightens to a red circle; talons spread.
                if (elapsed < GrabFollow) markPoint = Clear(PlayerOnFloor, 4.6f, FloorRadius - 3f);
                Locked = elapsed >= GrabFollow;
                Fly(markPoint + Vector3.up * (FlightHeight - 1f), 26f, deltaTime, false);
                Turn(-markPoint, 200f, deltaTime);
                pitch = Mathf.MoveTowards(pitch, 24f, 80f * deltaTime);
                roll = Mathf.MoveTowards(roll, 0f, 80f * deltaTime);
                flap = Mathf.Lerp(Mathf.Sin(FormTime * 8f) * 30f, 50f, Mathf.Clamp01((elapsed - GrabFollow) / GrabHold));
                circles[0].Show(markPoint, Quaternion.identity, elapsed / tell, RedMarker, false, GrabRadius);
                MarkAt = markPoint;
                moveFrom = at;
                return;
            }
            if (step == 0)
            {
                float t = Mathf.Clamp01((elapsed - tell) / dive);
                at = Vector3.Lerp(moveFrom, markPoint + Vector3.up * 2.6f, t * t);
                flap = 60f;
                if (look != null) look.Diving(moveFrom + Vector3.up * 1.5f, at);
                if (t < 1f) return;
                step = 1;
                circles[0].Hide();
                if (look != null) look.Dived(moveFrom + Vector3.up * 1.5f, markPoint);
                for (int i = 0; i < 8; i++) Vfx.Play(VfxKind.Dust, markPoint + Facing(i * 45f) * GrabRadius, Facing(i * 45f), 1.3f, Sand);
                if (shaker != null) shaker.Shake(0.5f, 0.3f);
                Vector3 offset = PlayerOnFloor - markPoint;
                bool caught = false;
                if (offset.magnitude <= GrabRadius + 0.3f && victimBody != null && !victimBody.IsHeld)
                {
                    blowName = "the talons";
                    caught = Deal(new Hit(GrabCatchDamage, 0f, Vector3.up, false, true, markPoint), 0f) == HitOutcome.Landed;
                }
                if (caught)
                {
                    // Taken up in the talons: the player can only struggle, which brings the drop sooner and lower.
                    Grabbed++;
                    talons.Seconds = GrabCarry + 1.5f;
                    talons.Begin();
                    victimBody.BeginHeld(talons);
                    carrying = true;
                    carryFrom = at;
                    // Carried up and across the court toward its middle, so the fall is somewhere new.
                    Vector3 across = Clear(OnFloor(markPoint) * 0.35f + Facing(yaw + 90f) * 3f, 9.5f, FloorRadius - 5f);
                    carryTo = across + Vector3.up * GrabHeight;
                    step = 2;
                    prepare = elapsed;
                    if (victimDefence != null) victimDefence.Callout("Taken up");
                    return;
                }
                // Missed: it lands hard in the circle, talons in the paving, wings spread, and is open.
                GrabsMissed++;
                grounded = true;
                at = new Vector3(markPoint.x, 1.2f, markPoint.z);
                yaw = YawOf(-markPoint);
                pitch = 14f;
                headOpen = true;
                SetOpen(true, true);
                if (look != null) look.Crater(markPoint, 1.6f, false);
                step = 4;
                prepare = elapsed;
                return;
            }
            if (step == 2)
            {
                // Up with them.
                float t = Mathf.Clamp01((elapsed - prepare) / GrabCarry);
                float eased = t * t * (3f - 2f * t);
                at = Vector3.Lerp(carryFrom, carryTo, eased);
                flap = Mathf.Sin(FormTime * 11f) * 38f + 8f;
                pitch = Mathf.MoveTowards(pitch, -6f, 60f * deltaTime);
                if (t < 1f && talons.IsHeld) return;
                // Let go: at the top, or lower when they struggled free.
                step = 3;
                falling = true;
                fallSpeed = 0f;
                dropHeight = Mathf.Max(0.5f, at.y - 2.6f);
                if (victimDefence != null) victimDefence.Callout("Dropped");
                return;
            }
            if (step == 3)
            {
                // They fall; it climbs away. EndCarry puts them down and the drop is dealt.
                Fly(new Vector3(at.x, FlightHeight, at.z), 10f, deltaTime, false);
                if (falling) return;
                if (elapsed - prepare < GrabCarry + 0.9f) return;
                EndAttack();
                return;
            }
            // Missed and grounded: struggling to pull its talons out of the paving, open.
            float stuck = elapsed - prepare;
            float shudder = Mathf.Sin(FormTime * 41f);
            roll = shudder * 2f;
            flap = Mathf.MoveTowards(flap, 12f, 120f * deltaTime);
            if (stuck < GrabMissed) return;
            grounded = false;
            headOpen = false;
            SetOpen(false, false);
            roll = 0f;
            Fly(new Vector3(at.x, FlightHeight, at.z), 12f, deltaTime, false);
            flap = Mathf.Sin(FormTime * 9f) * 34f;
            if (at.y < FlightHeight - 3f && stuck < GrabMissed + 1.4f) return;
            EndAttack();
        }

        // The player in the talons is kept under them; let go, they fall to the floor and the drop is dealt.
        private void AdvanceCarry()
        {
            if (!carrying || victimBody == null) return;
            float deltaTime = Time.deltaTime * WorldClock.EnemyScale;
            // Under the talons and a little toward the camera, so the falcon's body does not hide them from it.
            Vector3 toward = shaker != null ? -OnFloor(shaker.transform.forward).normalized : Vector3.zero;
            Vector3 held = at + Vector3.down * 2.6f + toward * 1.6f;
            if (falling)
            {
                fallSpeed += 30f * deltaTime;
                Vector3 now = victimBody.transform.position;
                float y = Mathf.Max(0.1f, now.y - fallSpeed * deltaTime);
                victimBody.Teleport(new Vector3(now.x, y, now.z), victimBody.transform.rotation);
                if (y > 0.11f) return;
                EndCarry(false);
                falling = false;
                prepare = elapsed - GrabCarry * 0.5f;
                blowName = "the drop";
                Vfx.Play(VfxKind.Dust, OnFloor(now), Vector3.up, 2.2f, Sand);
                Vfx.Play(VfxKind.Impact, OnFloor(now) + Vector3.up * 0.5f, Vector3.up, 1.4f);
                if (shaker != null) shaker.Shake(0.6f, 0.35f);
                // The higher the fall, the harder; one who struggled free lower down falls less far.
                float share = Mathf.Clamp(dropHeight / (GrabHeight - 2.6f), 0.4f, 1f);
                Deal(new Hit(GrabDropDamage * share, 0f, Vector3.down, true, true, OnFloor(now)), 0f);
                return;
            }
            victimBody.Teleport(new Vector3(held.x, Mathf.Max(0.1f, held.y), held.z), victimBody.transform.rotation);
        }

        // The talons let go of the player. Put down: they are set on the floor where they are, inside the court.
        private void EndCarry(bool putDown)
        {
            if (!carrying) return;
            carrying = false;
            falling = false;
            talons.Release();
            if (victimBody == null) return;
            Vector3 now = victimBody.transform.position;
            Vector3 floor = Clear(OnFloor(now), 2.6f, FloorRadius - 1f);
            if (putDown || now.y > 0.2f) victimBody.Teleport(new Vector3(floor.x, 0.1f, floor.z), victimBody.transform.rotation);
        }

        // ------------------------------------------------------------------ the Sunfall

        private void BeginSunfall()
        {
            Enter(Mode.Sunfall);
            pieces = Furious ? SunfallFuryCount : SunfallCount;
            thrown = 0;
        }

        private void AdvanceSunfall(float deltaTime)
        {
            // It hangs over the north of the court, beating hard, and tears the sun apart in its talons.
            Fly(Polar(Mathf.Clamp(flyAngle, -80f, 80f), FlightRadius - 2f, FlightHeight - 1f), 14f, deltaTime, false);
            Turn(PlayerOnFloor - OnFloor(at), 200f, deltaTime);
            flap = Mathf.Sin(FormTime * 13f) * 40f;
            Locked = thrown > 0;
            if (thrown < pieces && elapsed >= 0.5f + thrown * SunfallEvery)
            {
                Fall(PieceTarget(thrown), SunfallRadius, SunfallWarning, SunfallDamage, true);
                Vfx.Play(VfxKind.Embers, sunAt, Vector3.up, 2f);
                Vfx.Play(VfxKind.Fire, sunAt, Vector3.up, 1.6f);
                sunSize = Mathf.Max(0.75f, sunSize - 0.05f);
                MarkAt = fallPoints[0];
                thrown++;
            }
            if (elapsed < 0.5f + pieces * SunfallEvery + SunfallWarning + 0.4f) return;
            sunSize = 1f;
            EndAttack();
        }

        // Where each piece is thrown: the first where the player stands, then either side of them across the way it was
        // thrown from; in the fury a closing ring of four round them and the last where they stand.
        private Vector3 PieceTarget(int index)
        {
            Vector3 player = PlayerOnFloor;
            Vector3 from = OnFloor(at);
            Vector3 toward = player - from;
            toward = toward.sqrMagnitude > 0.01f ? toward.normalized : Vector3.back;
            Vector3 side = new Vector3(toward.z, 0f, -toward.x);
            Vector3 point;
            if (Furious)
                point = index < pieces - 1 ? player + Facing(YawOf(toward) + 45f + index * 90f) * 3.4f : player;
            else
                point = index == 0 ? player : player + side * (index % 2 == 1 ? 3.2f : -3.2f) + toward * 1.2f;
            return Clear(point, 3.2f, FloorRadius - 1.2f);
        }
    }
}

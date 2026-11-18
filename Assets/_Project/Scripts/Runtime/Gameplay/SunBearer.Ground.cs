using Clover.Core;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The scarab underground (fire_land.md 6.4, reworked on 2026-11-18). It never vanishes: it digs into the court in a
    /// burst of sand, dust and embers; a ridge of heaving, cracking paving and dust shows where it goes; and it bursts up
    /// where it is going. It does this to fetch its sun after it has lain exposed or shoved it away (the sun lies loose
    /// on the floor and the ridge is only dust and broken stone), and as an attack, taking the sun under with it (the
    /// cracks behind the ridge glow with it): the ridge chases the player, locks, and it bursts up under a red circle,
    /// half out of the ground for a moment with its head open.
    /// </summary>
    public partial class SunBearer
    {
        private enum BurrowFor { Attack, Fetch }

        private BurrowFor burrowFor;
        private bool underground, withSun;
        private Vector3 burrowAt, burrowHeading = Vector3.forward;
        private float digFor, chaseFor, comeFor, untilRidgeDust, comeFrom;
        private int eruptions, eruptionsAll;
        // What was chosen while the sun lay away: begun as soon as the sun is fetched.
        private SunBearerAttack afterFetch;
        private const float Depth = 3.6f, HalfOut = 1.3f;
        private static readonly Color Stone = new Color(0.62f, 0.58f, 0.52f, 1f);

        // Down it goes where it stands: sand thrown up in a ring, embers, a cloud of dust, and broken paving.
        private void BeginDig(bool carryingSun)
        {
            digFor = 0f;
            withSun = carryingSun;
            burrowAt = OnFloor(at);
            burrowHeading = Facing(yaw);
            Burrows++;
            SetOpen(false, false);
            headOpen = false;
            if (bulk != null) bulk.enabled = false;
            Vector3 middle = burrowAt + burrowHeading * 0.8f;
            for (int i = 0; i < 10; i++)
            {
                Vector3 way = Facing(i * 36f);
                Vfx.Play(VfxKind.Dust, middle + way * 2.2f + Vector3.up * 0.2f, way + Vector3.up * 0.6f, 1.6f, Sand);
            }
            Vfx.Play(VfxKind.TintRing, middle + Vector3.up * 0.2f, Vector3.up, 1.1f, Sand);
            Vfx.Play(VfxKind.Embers, middle + Vector3.up, Vector3.up, 2f);
            Vfx.Play(VfxKind.Ash, middle + Vector3.up * 0.6f, Vector3.up, 2f, Stone);
            if (shaker != null) shaker.Shake(0.35f, 0.4f);
            if (look != null) look.Crater(middle, 2.2f, carryingSun);
        }

        // Nose first into the court. True once it is under.
        private bool DigIn(float deltaTime)
        {
            digFor += deltaTime;
            float t = Mathf.Clamp01(digFor / DigSeconds);
            at = new Vector3(burrowAt.x, -Depth * t * t, burrowAt.z);
            pitch = Mathf.MoveTowards(pitch, 30f, 90f * deltaTime);
            roll = Mathf.MoveTowards(roll, 0f, 200f * deltaTime);
            lift = Mathf.MoveTowards(lift, 0f, 3f * deltaTime);
            legPhase += 34f * deltaTime;
            if (withSun) sunAt = Vector3.MoveTowards(sunAt, burrowAt + burrowHeading * 3.2f + Vector3.down * 2f, 9f * deltaTime);
            if (Every(ref untilRidgeDust, 0.06f, deltaTime))
            {
                Vector3 round = burrowAt + Facing(Random.value * 360f) * Random.Range(1.2f, 2.8f);
                Vfx.Play(VfxKind.Dust, round + Vector3.up * 0.2f, Vector3.up, 1.3f, Sand);
                if (Random.value < 0.5f) Vfx.Play(VfxKind.Ash, round + Vector3.up * 0.5f, Vector3.up, 0.8f, Stone);
                if (withSun) Vfx.Play(VfxKind.Fire, round + Vector3.up * 0.3f, Vector3.up, 1f);
            }
            if (t < 1f) return false;
            underground = true;
            body.gameObject.SetActive(false);
            if (withSun)
            {
                sunState = SunState.Under;
                sun.gameObject.SetActive(false);
            }
            return true;
        }

        // True when the straight way between two points passes nearer the obelisk than this.
        private static bool PassesStone(Vector3 from, Vector3 to, float clear)
        {
            Vector3 line = OnFloor(to - from);
            float t = line.sqrMagnitude > 0.0001f ? Mathf.Clamp01(-Vector3.Dot(OnFloor(from), line) / line.sqrMagnitude) : 0f;
            return OnFloor(from + line * t).magnitude < clear;
        }

        // Underground toward a place, round the obelisk and never under it, at a pace. True when there.
        private bool Tunnel(Vector3 goal, float speed, float deltaTime)
        {
            goal = Clear(goal, ObeliskClear, FloorRadius - 3.6f);
            Vector3 aim = goal;
            if (PassesStone(burrowAt, goal, 4.8f))
            {
                // Round the stone by the side nearer the goal.
                Vector3 out_ = burrowAt.sqrMagnitude > 0.01f ? OnFloor(burrowAt).normalized : Vector3.forward;
                Vector3 side = new Vector3(out_.z, 0f, -out_.x);
                if (Vector3.Dot(side, goal) < 0f) side = -side;
                aim = Clear((out_ + side) * 6f, 6f, FloorRadius - 3.6f);
            }
            Vector3 way = OnFloor(aim - burrowAt);
            float distance = way.magnitude;
            if (distance > 0.01f)
            {
                burrowHeading = Vector3.RotateTowards(burrowHeading, way / distance, 5f * deltaTime, 0f);
                burrowAt += way / distance * Mathf.Min(distance, speed * deltaTime);
                burrowAt = Clear(burrowAt, ObeliskClear - 0.4f, FloorRadius - 3f);
            }
            at = new Vector3(burrowAt.x, -Depth, burrowAt.z);
            yaw = YawOf(burrowHeading);
            DrawRidge(deltaTime, Mathf.Min(distance / Mathf.Max(deltaTime, 0.0001f), speed));
            return (OnFloor(goal) - burrowAt).magnitude < 0.35f;
        }

        // The ridge over it as it goes: heaving paving, cracks left behind it (glowing with the sun under it), dust.
        private void DrawRidge(float deltaTime, float speed)
        {
            if (look != null) look.Ridge(true, burrowAt, burrowHeading, withSun, speed);
            float every = speed > 0.5f ? Mathf.Clamp(0.55f / speed, 0.04f, 0.12f) : 0.1f;
            if (!Every(ref untilRidgeDust, every, deltaTime)) return;
            Vector3 side = new Vector3(burrowHeading.z, 0f, -burrowHeading.x) * (Random.value < 0.5f ? -1f : 1f);
            Vector3 spot = burrowAt + side * Random.Range(0.6f, 1.6f) - burrowHeading * Random.Range(0f, 1.2f);
            Vfx.Play(VfxKind.Dust, spot + Vector3.up * 0.2f, side + Vector3.up, 1.2f, Sand);
            if (Random.value < 0.4f) Vfx.Play(VfxKind.Ash, spot + Vector3.up * 0.4f, Vector3.up, 0.7f, Stone);
            if (withSun && Random.value < 0.6f) Vfx.Play(VfxKind.Embers, spot + Vector3.up * 0.3f, Vector3.up, 0.7f);
        }

        // Up out of the floor where the ridge has stopped: a burst of sand and broken paving, and fire with the sun.
        private void BeginComingUp(bool erupting)
        {
            comeFor = 0f;
            comeFrom = Depth;
            underground = false;
            body.gameObject.SetActive(true);
            at = new Vector3(burrowAt.x, -Depth, burrowAt.z);
            pitch = -38f;
            if (look != null)
            {
                look.Ridge(false, burrowAt, burrowHeading, withSun, 0f);
                look.Crater(burrowAt + burrowHeading * 0.8f, erupting ? EruptRadius : 2f, withSun);
            }
            Vector3 middle = burrowAt + burrowHeading * 0.8f;
            for (int i = 0; i < 12; i++)
            {
                Vector3 way = Facing(i * 30f);
                Vfx.Play(VfxKind.Dust, middle + way * (erupting ? EruptRadius : 2f) + Vector3.up * 0.2f, way + Vector3.up, erupting ? 2f : 1.5f, Sand);
            }
            Vfx.Play(VfxKind.Ash, middle + Vector3.up, Vector3.up, erupting ? 3f : 2f, Stone);
            Vfx.Play(VfxKind.TintRing, middle + Vector3.up * 0.2f, Vector3.up, erupting ? 1.5f : 1.1f, Sand);
            if (shaker != null) shaker.Shake(erupting ? 0.8f : 0.4f, 0.4f);
            if (withSun)
            {
                // Fire bursts up out of the ground with it; the sun itself stays under until it hauls itself out, so
                // its open head is not hidden behind it (and nobody is burnt by a sun coming up under their feet).
                for (int i = 0; i < 6; i++) Vfx.Play(VfxKind.Fire, middle + Facing(i * 60f) * 1.4f + Vector3.up * (0.5f + i * 0.3f), Vector3.up, 2.2f);
                Vfx.Play(VfxKind.Burst, middle + Vector3.up * 1.5f, Vector3.up, 2.4f);
                Vfx.Play(VfxKind.Smoke, middle + Vector3.up * 2.5f, Vector3.up, 2f, Smoke);
                if (!erupting) SunComesUp();
            }
        }

        // The sun comes up out of the floor in front of the scarab, in a fountain of fire.
        private void SunComesUp()
        {
            if (sunState != SunState.Under) return;
            sunState = SunState.Held;
            sun.gameObject.SetActive(true);
            sunAt = HeldSunPoint() + Vector3.down * 2.5f;
            Vector3 foot = OnFloor(sunAt);
            for (int i = 0; i < 5; i++) Vfx.Play(VfxKind.Fire, foot + Facing(i * 72f) * 1.2f + Vector3.up * 0.4f, Vector3.up, 1.8f);
            for (int i = 0; i < 6; i++) Vfx.Play(VfxKind.Dust, foot + Facing(i * 60f) * 1.8f, Vector3.up, 1.3f, Sand);
            if (look != null) look.Crater(foot, 1.8f, true);
        }

        // Rises out of the floor to this depth (0 is all the way out), nose up. True when there.
        private bool ComeUp(float deltaTime, float depthLeft, float seconds)
        {
            comeFor += deltaTime;
            float t = Mathf.Clamp01(comeFor / Mathf.Max(0.05f, seconds));
            float eased = 1f - (1f - t) * (1f - t) * (1f - t);
            at.y = Mathf.Lerp(-comeFrom, -depthLeft, eased);
            pitch = Mathf.Lerp(-38f, depthLeft > 0.01f ? -26f : 0f, eased);
            legPhase += 24f * deltaTime;
            if (withSun) sunAt = Vector3.MoveTowards(sunAt, HeldSunPoint(), 30f * deltaTime);
            return t >= 1f;
        }

        // ------------------------------------------------------------------ the Burrow

        private void BeginBurrow()
        {
            Enter(Mode.Burrow);
            burrowFor = BurrowFor.Attack;
            eruptionsAll = eruptions = Furious ? 2 : 1;
            BeginDig(true);
        }

        private void AdvanceBurrow(float deltaTime)
        {
            if (burrowFor == BurrowFor.Fetch)
            {
                AdvanceFetch(deltaTime);
                return;
            }
            switch (step)
            {
                case 0:
                    if (!DigIn(deltaTime)) return;
                    step = 1;
                    chaseFor = 0f;
                    return;
                case 1:
                {
                    // The ridge chases the player; in the fury's second dive the chase is shorter.
                    chaseFor += deltaTime;
                    float chase = eruptions < eruptionsAll ? ChaseFurySeconds : ChaseSeconds;
                    Tunnel(PlayerOnFloor, ChaseSpeed * Pace, deltaTime);
                    MarkAt = burrowAt;
                    if (chaseFor < chase) return;
                    markPoint = Clear(burrowAt, ObeliskClear + 0.4f, FloorRadius - 4f);
                    burrowAt = markPoint;
                    step = 2;
                    prepare = elapsed;
                    Locked = true;
                    return;
                }
                case 2:
                {
                    // Locked: the ground heaves under a red circle that fills.
                    float t = (elapsed - prepare) / EruptTell;
                    circles[0].Show(markPoint, Quaternion.identity, t, RedMarker, false, EruptRadius);
                    MarkAt = markPoint;
                    DrawRidge(deltaTime, 0f);
                    if (shaker != null && Every(ref untilPuff, 0.2f, deltaTime)) shaker.Shake(0.12f, 0.15f);
                    if (t < 1f) return;
                    Erupt();
                    step = 3;
                    return;
                }
                case 3:
                    if (!ComeUp(deltaTime, HalfOut, EmergeSeconds * 0.5f)) return;
                    step = 4;
                    prepare = elapsed;
                    return;
                case 4:
                {
                    // Half out of the ground, its head open: the opening.
                    float hold = eruptions > 1 ? 0.75f : HalfOutSeconds;
                    float shudder = Mathf.Sin(Time.time * 37f);
                    roll = shudder * 1.5f;
                    if (elapsed - prepare < hold) return;
                    roll = 0f;
                    if (eruptions > 1)
                    {
                        // In its fury it goes straight back down and comes again.
                        eruptions--;
                        burrowAt = OnFloor(at);
                        BeginDig(true);
                        digFor = DigSeconds * 0.4f;
                        Burrows--;
                        step = 0;
                        return;
                    }
                    headOpen = false;
                    SetOpen(false, true);
                    if (bulk != null) bulk.enabled = true;
                    SunComesUp();
                    step = 5;
                    prepare = elapsed;
                    return;
                }
                default:
                {
                    // It hauls itself out.
                    float t = Mathf.Clamp01((elapsed - prepare) / 0.5f);
                    at.y = Mathf.Lerp(-HalfOut, 0f, t);
                    pitch = Mathf.Lerp(-26f, 0f, t);
                    legPhase += 24f * deltaTime;
                    if (t < 1f) return;
                    EndAttack();
                    return;
                }
            }
        }

        // It bursts up under the locked circle facing whoever it chased, and the blow lands on whoever is still in it.
        private void Erupt()
        {
            circles[0].Hide();
            Vector3 toPlayer = PlayerOnFloor - markPoint;
            if (toPlayer.sqrMagnitude > 0.25f) burrowHeading = toPlayer.normalized;
            yaw = YawOf(burrowHeading);
            // Its middle comes up a little behind the circle's, so the horn and the head are in it.
            burrowAt = Clear(markPoint - burrowHeading * 0.8f, ObeliskClear, FloorRadius - 3.6f);
            BeginComingUp(true);
            Vfx.Play(VfxKind.Impact, markPoint + Vector3.up, Vector3.up, 2f);
            ScreenFlash.Flash(new Color(1f, 0.45f, 0.12f, 0.24f), 0.07f);
            Vector3 offset = PlayerOnFloor - markPoint;
            if (offset.magnitude <= EruptRadius + 0.35f)
            {
                blowName = "the burrow";
                Vector3 away = offset.sqrMagnitude > 0.01f ? offset.normalized : burrowHeading;
                if (Deal(new Hit(EruptDamage, 0f, away, true, true, markPoint), 0f) == HitOutcome.Landed && victimBody != null)
                    victimBody.Shove(away * EruptShove, 0.3f);
            }
            if (mode != Mode.Burrow) return;
            headOpen = true;
            SetOpen(true, true);
        }

        // ------------------------------------------------------------------ fetching the sun

        // The sun lies away from it: walked to when near, burrowed to when far. Then what was chosen begins.
        private void BeginFetch(SunBearerAttack next)
        {
            afterFetch = next;
            Enter(Mode.Burrow);
            burrowFor = BurrowFor.Fetch;
            withSun = false;
            if ((OnFloor(sunAt) - OnFloor(at)).magnitude <= 7.5f)
            {
                step = 10;
                return;
            }
            BeginDig(false);
        }

        private void AdvanceFetch(float deltaTime)
        {
            switch (step)
            {
                case 10:
                {
                    Vector3 toSun = OnFloor(sunAt) - OnFloor(at);
                    Turn(toSun, 200f, deltaTime);
                    bool there = toSun.magnitude <= 4.7f || Walk(SunSide(), WalkSpeed * 1.2f, deltaTime, false) || elapsed > 4f;
                    if (!there) return;
                    step = 13;
                    prepare = elapsed;
                    return;
                }
                case 0:
                    if (!DigIn(deltaTime)) return;
                    step = 1;
                    return;
                case 1:
                    if (!Tunnel(SunSide(), RidgeSpeed, deltaTime)) return;
                    yaw = YawOf(OnFloor(sunAt) - burrowAt);
                    burrowHeading = Facing(yaw);
                    BeginComingUp(false);
                    step = 2;
                    return;
                case 2:
                    if (!ComeUp(deltaTime, 0f, EmergeSeconds)) return;
                    SetOpen(false, true);
                    if (bulk != null) bulk.enabled = true;
                    step = 13;
                    prepare = elapsed;
                    return;
                default:
                    Turn(OnFloor(sunAt) - OnFloor(at), 300f, deltaTime);
                    if (elapsed - prepare < 0.6f) return;
                    // The fetch is not a fall: it takes the sun up without the poise a fall earns.
                    sunState = SunState.Held;
                    rolling.Stop();
                    sunPart.SetVulnerable(false);
                    Vfx.Play(VfxKind.Fire, sunAt, Vector3.up, 2.4f);
                    var next = afterFetch;
                    afterFetch = SunBearerAttack.None;
                    EndAttack();
                    if (next != SunBearerAttack.None) Begin(next);
                    return;
            }
        }

        // Back up out of the floor at once wherever it is, as when its balance breaks underground or it dies there.
        private void Surface()
        {
            if (!underground && at.y > -0.05f) return;
            if (underground)
            {
                underground = false;
                body.gameObject.SetActive(true);
                if (look != null) look.Ridge(false, burrowAt, burrowHeading, withSun, 0f);
                for (int i = 0; i < 8; i++) Vfx.Play(VfxKind.Dust, burrowAt + Facing(i * 45f) * 2f, Vector3.up, 1.5f, Sand);
            }
            if (sunState == SunState.Under)
            {
                sunState = SunState.Held;
                sun.gameObject.SetActive(true);
                sunAt = OnFloor(at) + Facing(yaw) * 4.4f + Vector3.up * 1.5f;
            }
            // Whatever comes next lifts it the rest of the way (a broken balance at thirty metres a second).
            if (at.y < -2f) at.y = -2f;
        }
    }
}

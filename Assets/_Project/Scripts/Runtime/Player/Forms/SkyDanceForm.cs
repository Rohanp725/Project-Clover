using Clover.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Wind's Sky Dance. The enemy faced is thrown into the air (or kept there) and the player goes up beside it.
    /// Count times a ring closes on it: the light attack pressed as the ring closes is a cut in time (half again as
    /// hard, a star); pressed early, or not pressed, it is a plain cut. Then a lane and a circle on the ground follow
    /// the stick for a moment (the heavy attack throws at once) and the enemy is thrown down into the circle, bursting
    /// harder for each cut in time. Untouchable from the leap to the landing. What cannot be lifted reels, and the
    /// dance is done on the ground beside it.
    /// Reads: Count (cuts, 8 at most), Interval (each ring), Damage (each cut), Stagger (of the landing), Radius (of
    /// the landing), Reach (to the enemy, and how far it is thrown), LingerDamage (added to the landing for each cut in time).
    /// </summary>
    public class SkyDanceForm : RunningForm, ISpellGate
    {
        public override string Id => SpellForms.SkyDance;

        private const float RiseSeconds = 0.22f;
        private const float AimSeconds = 0.45f;
        private const float FlightSeconds = 0.24f;
        private const float AfterSeconds = 0.3f;
        private const float InTime = 0.45f;      // the last share of a ring's closing that counts as in time
        private const float Beside = 1.5f;

        /// <summary>
        /// The dance needs an enemy that can be danced with now: alive, not still reeling from the last dance (it
        /// cannot be thrown again until it has its feet), a body free to leave the ground, and no dance in progress.
        /// Refused here, the cast costs nothing.
        /// </summary>
        public bool CanCast(in SpellCast cast)
        {
            var victim = FormKit.Victim(cast);
            var set = SigKit.Set(victim);
            if (set == null || !FormKit.Alive(victim)) return false;
            if (set.IsAirborne && !set.IsLifted) return false;
            if (cast.Player == null || cast.Player.IsLeaping) return false;
            if (cast.Defence != null && cast.Defence.IsHelpless) return false;
            return SpellWorks.RunningOf(Id) == 0;
        }

        // The two buttons the dance listens to, found once for the player who is dancing, not every frame.
        private static PlayerController actionsOf;
        private static InputAction lightAction, heavyAction;

        private static void FindActions(PlayerController player)
        {
            if (actionsOf == player && lightAction != null) return;
            actionsOf = player;
            lightAction = player.FindAction("LightAttack");
            heavyAction = player.FindAction("HeavyAttack");
        }

        public override int Cast(in SpellCast cast)
        {
            var victim = FormKit.Victim(cast);
            var set = SigKit.Set(victim);
            if (set == null || !FormKit.Alive(victim)) return 0;
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            if (!SpellWorks.TakeBody(ref run, true, false))
            {
                run.Phase = 9;
                return 0;
            }
            run.Victim = victim;
            FindActions(cast.Player);
            run.Count = Mathf.Clamp(cast.Spec.Count, 1, 8);
            run.T0 = Mathf.Max(0.18f, cast.Spec.Interval);
            run.C = cast.Origin;
            run.B = cast.Forward;
            // Flag: it left the ground (false: it only reels, and the dance stays down).
            run.Flag = set.IsLifted || set.Lift(RiseSeconds + run.Count * run.T0 + AimSeconds + 0.3f);
            // The body is carried by hand while up: gravity would take it otherwise. Land() gives it back.
            var body = cast.Player.GetComponent<CharacterController>();
            if (body != null) body.enabled = false;
            SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(cast.Colour, 3.4f));
            SpellWorks.Ghost(RiseSeconds + 0.15f, FormKit.Lit(cast.Colour, 2.6f, 0.85f));
            Color light = FormKit.Lit(cast.Colour, 3f);
            FormFx.Lay(FormMark.Ring, cast.Origin, 0.6f, light, 0.25f, 3.4f);
            FormFx.Lay(FormMark.Disc, SigKit.Foot(victim, cast.Origin.y), 2.4f, FormKit.Stain(cast.Colour, 0.5f), RiseSeconds + run.Count * run.T0 + AimSeconds);
            ElementLook.Puff(cast.Element, cast.Origin + Vector3.up * 0.3f, Vector3.up, 1f);
            return 0;
        }

        public override void Swung(ref SpellRun run, AttackStep step, Vector3 origin, Vector3 forward) => run.Flag2 = true;

        public override void End(ref SpellRun run)
        {
            if (run.Phase == 9 || run.Cast.Player == null) return;
            Vector3 at = run.Cast.Player.transform.position;
            at.y = run.C.y;
            Land(run.Cast.Player, at);
        }

        private static void Land(PlayerController player, Vector3 at)
        {
            var body = player.GetComponent<CharacterController>();
            if (body == null || body.enabled) return;
            player.transform.position = at;
            body.enabled = true;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            if (run.Phase == 9) return false;
            var cast = run.Cast;
            var player = cast.Player;
            var marks = SpellWorks.Marks(run);
            bool alive = FormKit.Alive(run.Victim);
            var set = alive ? SigKit.Set(run.Victim) : null;
            Vector3 foot = alive ? SigKit.Foot(run.Victim, run.C.y) : run.A;
            float height = run.Flag ? AfflictionSet.AirRise : 0f;
            Vector3 chest = foot + Vector3.up * (1.1f + height);
            bool light = run.Flag2;
            run.Flag2 = false;
            FindActions(player);
            if (lightAction != null && lightAction.WasPressedThisFrame()) light = true;
            bool heavy = heavyAction != null && heavyAction.WasPressedThisFrame();

            if (run.Phase <= 2)
            {
                if (!alive && run.Phase < 2)
                {
                    // It died up there: come down where it was.
                    run.Phase = 4;
                    run.T1 = run.Age;
                    Land(player, foot - run.B * Beside);
                    return true;
                }
                // Beside it, a new side for every cut, as high as it hangs.
                Vector3 side = Quaternion.Euler(0f, 180f + run.Done * 144f, 0f) * run.B * Beside;
                Vector3 want = foot + side + Vector3.up * height;
                float rise = Mathf.Clamp01(run.Age / RiseSeconds);
                player.transform.position = run.Phase == 0 ? Vector3.Lerp(run.C, want, rise * rise * (3f - 2f * rise)) : Vector3.Lerp(player.transform.position, want, 1f - Mathf.Exp(-30f * deltaTime));
                player.transform.rotation = FormKit.Yaw(foot - player.transform.position);
                player.FormSwordOut = true;
                if (set != null && run.Flag) set.Lift(0.5f);
            }
            if (run.Phase == 0)
            {
                player.FormPose = FormKit.Draw;
                player.FormPoseProgress = Mathf.Lerp(0.1f, 0.5f, run.Age / RiseSeconds);
                if (run.Age < RiseSeconds) return true;
                run.Phase = 1;
                run.T1 = run.Age;
                return true;
            }
            if (run.Phase == 1)
            {
                float through = (run.Age - run.T1) / run.T0;
                FormFx.PlaceMarkFacing(marks[0], chest, Mathf.Lerp(4.2f, 1f, through), run.Done * 36f);
                player.FormPose = run.Done % 2 == 0 ? FormKit.Sweep : FormKit.Thrust;
                player.FormPoseProgress = Mathf.Lerp(0.15f, 0.9f, through);
                if (!light && through < 1f) return true;
                bool timed = light && through >= 1f - InTime;
                if (timed) run.T3 += 1f;
                cast.Combat.StrikeOne(run.Victim, cast.Blow(cast.Damage * (timed ? 1.5f : 1f), 0f, run.Done == 0 ? -1 : 0), player.transform.position);
                Vector3 along = Quaternion.Euler(0f, run.Done * 72f, 0f) * (Vector3.right + Vector3.up * (run.Done % 2 == 0 ? 0.6f : -0.6f));
                FormFx.Cut(chest, along, timed ? 7f : 4.5f, timed ? 0.9f : 0.45f, FormKit.Lit(cast.Colour, timed ? 4.2f : 2.8f), 0.16f, 0.05f);
                if (timed) FormFx.Face(FormMark.Star, chest, 1.6f, FormKit.Lit(cast.Colour, 4f), 0.12f, 3.6f, run.Done * 30f);
                ElementLook.Sparks(cast.Element, chest, along, timed ? 1f : 0.5f);
                if (cast.Upgrades != null) cast.Upgrades.Jolt(timed ? 0.035f : 0f, timed ? 0.16f : 0.07f, 0.1f);
                run.Done++;
                run.T1 = run.Age;
                if (run.Done < run.Count) return true;
                // The cuts are done: the ring goes, and the lane and the circle that follow the stick come.
                run.Phase = 2;
                FormFx.ReleaseMark(marks[0], 0.1f);
                marks[0] = -1;
                marks[1] = FormFx.HoldMark(FormMark.Lane, FormKit.Lit(cast.Colour, 1.8f));
                marks[2] = FormFx.HoldMark(FormMark.Disc, FormKit.Lit(cast.Colour, 2.2f));
                marks[3] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(cast.Colour, 3.2f));
                return true;
            }
            if (run.Phase == 2)
            {
                // Where the stick points, as the camera sees it (or the cursor, while the mouse is in use); with the stick at rest, on past the enemy.
                Vector3 wish = player.Steer;
                wish.y = 0f;
                if (wish.sqrMagnitude > 0.09f) run.B = wish.normalized;
                float far = Mathf.Max(2f, cast.Reach);
                float radius = cast.Spec.Radius;
                run.A = foot + run.B * far;
                float through = Mathf.Clamp01((run.Age - run.T1) / AimSeconds);
                FormFx.PlaceMark(marks[1], foot, FormKit.Yaw(run.B), new Vector3(1.2f, 1f, far));
                FormFx.PlaceMark(marks[2], run.A, Quaternion.identity, new Vector3(radius * 2f, 1f, radius * 2f));
                float ring = Mathf.Lerp(radius * 4f, radius * 2f, through);
                FormFx.PlaceMark(marks[3], run.A, Quaternion.identity, new Vector3(ring, 1f, ring));
                player.FormPose = FormKit.Chop;
                player.FormPoseProgress = Mathf.Lerp(0.05f, 0.45f, through);
                if (!heavy && through < 1f) return true;
                // Thrown: it goes down the lane, and the player comes down where it hung.
                run.Phase = 3;
                run.T1 = run.Age;
                if (set != null) set.Drop();
                if (alive && run.Victim is IShovable shoved) shoved.Push(run.B * (far / FlightSeconds / 6f));
                Color bright = FormKit.Lit(cast.Colour, 4f);
                FormFx.Run(FormMark.Streak, foot, run.B, far, 1.3f, bright, FlightSeconds + 0.1f, 0.15f);
                FormFx.Cut(chest, run.B + Vector3.down * 0.5f, 6f, 1f, bright, 0.14f, 0.1f);
                ElementLook.Puff(cast.Element, chest, run.B, 1.2f);
                Land(player, foot - run.B * 0.6f);
                player.transform.rotation = FormKit.Yaw(run.B);
                if (cast.Upgrades != null) cast.Upgrades.Jolt(0.05f, 0.3f, 0.15f);
                return true;
            }
            player.FormPose = FormKit.Chop;
            player.FormSwordOut = true;
            if (run.Phase == 3)
            {
                player.FormPoseProgress = Mathf.Lerp(0.5f, 0.9f, (run.Age - run.T1) / FlightSeconds);
                // Carried down the lane for as long as the flight lasts: one shove alone is lost the moment it lands and reels.
                if (alive && run.Victim is IShovable flying) flying.Push(run.B * (Mathf.Max(2f, cast.Reach) / FlightSeconds * 1.5f * deltaTime));
                if (run.Age < run.T1 + FlightSeconds) return true;
                run.Phase = 4;
                run.T1 = run.Age;
                float radius = cast.Spec.Radius;
                var blow = cast.Blow(cast.Damage * 2f + cast.Spec.LingerDamage * run.T3, cast.Stagger);
                blow.Heavy = true;
                // The one thrown takes it wherever the throw left it; everything else in the circle takes it once.
                var once = SpellWorks.Once(run);
                once.Clear();
                if (alive && run.Victim is IDamageable thrown)
                {
                    cast.Combat.StrikeOne(run.Victim, blow, run.A - run.B);
                    once.Add(thrown);
                }
                // It bursts where the thrown one lies: what cannot be thrown (or met a wall) breaks the ground where it is.
                Vector3 at = run.A;
                if (alive && run.Victim != null)
                {
                    at = run.Victim.transform.position;
                    at.y = run.A.y;
                }
                cast.Combat.StrikeArea(at, radius, blow, once);
                cast.Combat.Displace(at, radius, 3f, false);
                FormKit.Impact(cast, at, radius, 1.2f + 0.16f * run.T3);
                SigKit.Column(at, FormKit.Lit(cast.Colour, 3.6f), 1.6f, 0.16f);
                Landed = Mathf.RoundToInt(run.T3);
                if (run.T3 >= run.Count) SigKit.Moment(cast, 0.14f, 0.7f, 0.3f, 0.45f, 0.4f);
                return true;
            }
            player.FormPoseProgress = Mathf.Lerp(0.9f, 0.99f, (run.Age - run.T1) / AfterSeconds);
            return run.Age < run.T1 + AfterSeconds;
        }

        /// <summary>How many cuts of the last dance were in time, for probes.</summary>
        public static int Landed { get; private set; }

        // Domain reload is off: a number from the last play must not be read in this one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Landed = 0;
            actionsOf = null;
            lightAction = null;
            heavyAction = null;
        }
    }
}

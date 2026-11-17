using Clover.Gameplay;
using Clover.World;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Tempest's Eye of the Storm. While the spell button is held (for Seconds at most) the player is the dark centre
    /// of a turning ring: enemies inside are dragged in, bitten every Interval, and those that can leave the ground
    /// are carried round. The player walks slower, takes half and cannot be made to flinch. Let go, and everything
    /// is thrown outward and a bolt meets each where it stops.
    /// Reads: Seconds (the longest hold), Interval (between bites), Radius, Damage (each bite), Stagger and
    /// LingerDamage (of each bolt), Reach (how far they are thrown), Stacks (each bite).
    /// </summary>
    public class EyeForm : RunningForm, ISpellGate
    {
        public override string Id => SpellForms.Eye;

        private const float Least = 0.5f;          // it turns at least this long, however short the press
        private const float ThrowSeconds = 0.32f;
        private const float AfterSeconds = 0.25f;
        private const float DragPerSecond = 2f;
        private const float CarryPerSecond = 7f;
        private const int Most = 8;

        /// <summary>Bolts the last storm called down as it let go, and bites it took, for probes.</summary>
        public static int Bolts { get; private set; }
        public static int Bites { get; private set; }
        /// <summary>A probe has no button to hold: while true the storm turns its whole time.</summary>
        public static bool HoldForProbe { get; set; }

        // Domain reload is off: numbers from the last play must not be read in this one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Bolts = 0;
            Bites = 0;
            HoldForProbe = false;
        }

        public bool CanCast(in SpellCast cast)
        {
            if (cast.Player == null || cast.Player.IsLeaping) return false;
            if (cast.Defence != null && cast.Defence.IsHelpless) return false;
            return SpellWorks.RunningOf(Id) == 0;
        }

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.T0 = Mathf.Max(Least, cast.Spec.Seconds);
            run.T2 = 0f;
            run.T3 = 0f;
            Bolts = 0;
            Bites = 0;
            var marks = SpellWorks.Marks(run);
            marks[0] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(cast.Colour, 3.2f));
            marks[1] = FormFx.HoldMark(FormMark.Sigil, FormKit.Lit(cast.Colour, 1.5f, 0.8f));
            marks[2] = FormFx.HoldMark(FormMark.Disc, FormKit.Stain(cast.Colour, 0.8f));
            marks[3] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(cast.Colour, 2.2f));
            SpellWorks.Ghost(0.3f, FormKit.Lit(cast.Colour, 2.6f, 0.85f));
            FormFx.Lay(FormMark.Ring, cast.Origin, 0.6f, FormKit.Lit(cast.Colour, 3f), 0.25f, cast.Spec.Radius * 2f);
            if (cast.Upgrades != null) cast.Upgrades.Jolt(0.03f, 0.15f, 0.12f);
            return 0;
        }

        private static void Let(int[] marks)
        {
            for (int i = 0; i < 4; i++)
            {
                if (marks[i] < 0) continue;
                FormFx.ReleaseMark(marks[i], 0.15f);
                marks[i] = -1;
            }
        }

        public override void End(ref SpellRun run) => Let(SpellWorks.Marks(run));

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var player = cast.Player;
            if (player == null) return false;
            var marks = SpellWorks.Marks(run);
            var things = SpellWorks.Things(run);
            float radius = Mathf.Max(1.5f, cast.Spec.Radius);
            Color light = FormKit.Lit(cast.Colour, 3.4f);

            if (run.Phase == 0)
            {
                Vector3 centre = player.transform.position;
                centre.y = cast.Origin.y;
                run.A = centre;
                run.T3 += deltaTime * 320f;
                float breathe = 1f + 0.05f * Mathf.Sin(run.Age * 14f);
                FormFx.PlaceMark(marks[0], centre, Quaternion.Euler(0f, run.T3, 0f), new Vector3(radius * 2f * breathe, 1f, radius * 2f * breathe));
                FormFx.PlaceMark(marks[1], centre, Quaternion.Euler(0f, -run.T3 * 0.6f, 0f), new Vector3(radius * 1.7f, 1f, radius * 1.7f));
                FormFx.PlaceMark(marks[2], centre, Quaternion.identity, new Vector3(2.6f, 1f, 2.6f));
                FormFx.PlaceMark(marks[3], centre, Quaternion.Euler(0f, run.T3 * 1.7f, 0f), new Vector3(radius * 1.1f, 1f, radius * 1.1f));
                player.FormPose = FormKit.Brace;
                player.FormPoseProgress = 0.5f;

                // Dragged in, and the light ones carried round.
                cast.Combat.Displace(centre, radius, DragPerSecond * deltaTime, true);
                int near = SigKit.Gather(run, centre, radius, Most);
                bool bite = false;
                run.T2 -= deltaTime;
                if (run.T2 <= 0f)
                {
                    run.T2 = Mathf.Max(0.15f, cast.Spec.Interval);
                    bite = true;
                    // Slower on the feet, half as hurt and past flinching for as long as it turns: given again with each bite.
                    if (cast.Upgrades != null)
                    {
                        cast.Upgrades.Stats.AddBoon(PlayerStat.MoveSpeed, -0.3f, run.T2 + 0.05f);
                        cast.Upgrades.Stats.AddBoon(PlayerStat.DamageTaken, -0.5f, run.T2 + 0.05f);
                        cast.Upgrades.StandFirm(run.T2 + 0.05f);
                    }
                    if (near > 0)
                    {
                        Bites += cast.Combat.StrikeArea(centre, radius, cast.Blow(cast.Damage, 0f));
                        if (cast.Upgrades != null) cast.Upgrades.Jolt(0f, 0.1f, 0.1f);
                    }
                    // The turning wind, seen: cuts thrown round the ring's edge.
                    for (int i = 0; i < 5; i++)
                    {
                        Vector3 out5 = Quaternion.Euler(0f, run.T3 + i * 72f, 0f) * Vector3.forward;
                        Vector3 across = Vector3.Cross(Vector3.up, out5);
                        FormFx.Cut(centre + out5 * radius * 0.85f + Vector3.up * (0.8f + 0.5f * (i % 3)), across + Vector3.up * 0.15f, 3.2f, 0.28f, light, 0.22f, 0.04f);
                    }
                    ElementLook.Puff(cast.Element, centre + Vector3.up * 0.4f, Vector3.up, 1f);
                }
                for (int i = 0; i < near; i++)
                {
                    var thing = things[i];
                    if (!FormKit.Alive(thing)) continue;
                    var set = SigKit.Set(thing);
                    Vector3 from = thing.transform.position - centre;
                    from.y = 0f;
                    if (bite)
                    {
                        if (set != null) set.Lift(run.T2 + 0.3f);
                        Vector3 chest = thing.transform.position + Vector3.up * 1.2f;
                        Vector3 heart = centre + Vector3.up * 1.2f;
                        // An arc from the eye to each one it holds.
                        FormFx.Cut((chest + heart) * 0.5f, chest - heart, (chest - heart).magnitude, 0.22f, FormKit.Lit(cast.Colour, 4.4f), 0.12f, 0.05f);
                        ElementLook.Sparks(cast.Element, chest, Vector3.up, 0.7f);
                    }
                    if (set != null && set.IsLifted && thing is IShovable carried && from.sqrMagnitude > 0.04f)
                        carried.Push(Vector3.Cross(Vector3.up, from.normalized) * (CarryPerSecond * deltaTime));
                }
                bool held = HoldForProbe || (cast.Caster != null && cast.Caster.SpellHeld);
                if ((held || run.Age < Least) && run.Age < run.T0) return true;

                // Let go: everything is thrown outward.
                run.Phase = 1;
                run.T1 = run.Age;
                run.Count = SigKit.Gather(run, centre, radius + 0.5f, Most);
                Let(marks);
                FormFx.Lay(FormMark.Ring, centre, radius * 2f, FormKit.Lit(cast.Colour, 4f), 0.35f, radius * 2f + cast.Reach * 2f, 120f);
                FormFx.Lay(FormMark.Star, centre, 2f, light, 0.25f, radius * 2.4f);
                for (int i = 0; i < run.Count; i++)
                {
                    Vector3 from = things[i].transform.position - centre;
                    from.y = 0f;
                    Vector3 away = from.sqrMagnitude > 0.01f ? from.normalized : Quaternion.Euler(0f, i * 97f, 0f) * Vector3.forward;
                    FormFx.Run(FormMark.Streak, things[i].transform.position, away, cast.Reach, 0.9f, light, ThrowSeconds + 0.2f, 0.15f);
                    FormFx.Lay(FormMark.Ring, things[i].transform.position + away * cast.Reach, 2.6f, FormKit.Lit(cast.Colour, 2f), ThrowSeconds + 0.1f, 0.8f);
                }
                FormKit.Impact(cast, centre, radius, 1f);
                if (cast.Upgrades != null) cast.Upgrades.Jolt(0.05f, 0.35f, 0.15f);
                return true;
            }
            if (run.Phase == 1)
            {
                // Carried outward for as long as the throw lasts: one shove alone is lost the moment a thing lands.
                float step = cast.Reach / ThrowSeconds * deltaTime;
                for (int i = 0; i < run.Count; i++)
                {
                    var thing = things[i];
                    if (!FormKit.Alive(thing) || !(thing is IShovable thrown)) continue;
                    Vector3 from = thing.transform.position - run.A;
                    from.y = 0f;
                    thrown.Push((from.sqrMagnitude > 0.01f ? from.normalized : Vector3.forward) * step);
                }
                if (run.Age < run.T1 + ThrowSeconds) return true;
                run.Phase = 2;
                run.T1 = run.Age;
                // A bolt meets each where it stops.
                var bolt = cast.Blow(cast.Spec.LingerDamage, cast.Stagger, 0);
                bolt.Heavy = true;
                for (int i = 0; i < run.Count; i++)
                {
                    var thing = things[i];
                    if (!FormKit.Alive(thing)) continue;
                    var set = SigKit.Set(thing);
                    if (set != null) set.Drop();
                    Vector3 foot = SigKit.Foot(thing, run.A.y);
                    LightningBolts.Strike(foot, 0.5f);
                    SigKit.Column(foot, FormKit.Lit(cast.Colour, 4.4f), 0.9f, 0.16f);
                    FormFx.Lay(FormMark.Ring, foot, 0.6f, light, 0.3f, 3.2f);
                    FormFx.Lay(FormMark.Scorch, foot, 2.2f, FormKit.Stain(cast.Colour, 0.7f), 1.4f);
                    if (cast.Combat.StrikeOne(thing, bolt, foot + Vector3.up * 3f)) Bolts++;
                }
                if (Bolts > 0) SigKit.Moment(cast, 0.09f, 0.55f, Bolts >= 2 ? 0.25f : 0f, 0.45f, 0.3f);
                return true;
            }
            return run.Age < run.T1 + AfterSeconds;
        }
    }
}

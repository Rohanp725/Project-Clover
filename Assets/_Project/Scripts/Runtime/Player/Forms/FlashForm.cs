using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// The player is gone, leaving their shape in light; lines of the element cross the enemy faced one after
    /// another; the player is standing beyond it, still, with their back to it; and then every cut lands at once.
    /// Nothing can strike the player from the vanishing to the landing.
    /// </summary>
    public class FlashForm : RunningForm
    {
        public override string Id => SpellForms.Flash;

        private const float Beyond = 2f;
        private const float AfterSeconds = 0.28f;
        private const float LineLength = 8f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            if (!SpellWorks.TakeBody(ref run, true, false))
            {
                run.Phase = 9;
                return 0;
            }
            run.Victim = FormKit.Victim(cast);
            run.A = FormKit.AimPoint(cast);
            run.B = cast.Forward;
            run.Count = Mathf.Clamp(cast.Spec.Count, 1, 12);
            run.T0 = Mathf.Max(0.035f, cast.Spec.Interval);
            run.T1 = Mathf.Max(0.15f, cast.Spec.Delay);
            SpellWorks.Ghost(run.Count * run.T0 + 0.35f, FormKit.Lit(cast.Colour, 2.6f, 0.85f));
            cast.Player.Hidden = true;
            FormFx.Lay(FormMark.Ring, cast.Origin, 0.6f, FormKit.Lit(cast.Colour, 3f), 0.2f, 3f);
            FormKit.Flash(cast, 0.14f, 0.05f);
            return 0;
        }

        // The way the n-th line lies: each well off the one before, so they cross.
        private static Vector3 Line(in SpellRun run, int index) => Quaternion.Euler(0f, 25f + index * 67f, 0f) * run.B;

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            if (run.Phase == 9) return false;
            var cast = run.Cast;
            var player = cast.Player;
            if (FormKit.Alive(run.Victim) && run.Phase < 2)
            {
                Vector3 under = run.Victim.transform.position;
                run.A = new Vector3(under.x, cast.Origin.y, under.z);
            }
            Vector3 chest = run.A + Vector3.up * 1.1f;
            if (run.Phase == 0)
            {
                while (run.Done < run.Count && run.Age >= run.Done * run.T0)
                {
                    // Each line stays, thin and bright, until they all land.
                    float lasts = (run.Count - run.Done) * run.T0 + run.T1 + 0.04f;
                    Vector3 line = Line(run, run.Done);
                    FormFx.Cut(chest, line, LineLength, 0.2f, FormKit.Lit(cast.Colour, 3.2f), lasts, 0.14f);
                    FormFx.Cut(chest, line, LineLength * 1.2f, 0.7f, FormKit.Lit(cast.Colour, 4f), 0.07f, 0.1f);
                    ElementLook.Sparks(cast.Element, chest, line, 0.4f);
                    if (cast.Upgrades != null) cast.Upgrades.Jolt(0f, 0.07f, 0.08f);
                    run.Done++;
                }
                if (run.Age < run.Count * run.T0) return true;
                // There, beyond it, back turned, the blade going home.
                run.Phase = 1;
                run.T2 = run.Age;
                player.Hidden = false;
                player.StepTo(run.A + run.B * Beyond);
                player.transform.rotation = Quaternion.LookRotation(run.B, Vector3.up);
                FormFx.Lay(FormMark.Ring, player.transform.position, 2.6f, FormKit.Lit(cast.Colour, 3f), 0.2f, 0.8f);
                return true;
            }
            player.FormPose = FormKit.Draw;
            player.FormSwordOut = true;
            if (run.Phase == 1)
            {
                player.FormPoseProgress = Mathf.Lerp(0.62f, 0.9f, (run.Age - run.T2) / run.T1);
                if (run.Age < run.T2 + run.T1) return true;
                run.Phase = 2;
                run.T2 = run.Age;
                int landed = 0;
                for (int i = 0; i < run.Count; i++)
                {
                    var blow = cast.Blow(cast.Damage, cast.Stagger);
                    if (FormKit.Alive(run.Victim)) landed += cast.Combat.StrikeOne(run.Victim, blow, cast.Origin) ? 1 : 0;
                    else landed += cast.Combat.StrikeArea(run.A, 1.6f, blow);
                    FormFx.Cut(chest, Line(run, i), LineLength * 1.25f, 0.85f, FormKit.Lit(cast.Colour, 4f), 0.16f, 0.05f);
                }
                FormFx.Face(FormMark.Star, chest, 4.2f, FormKit.Lit(cast.Colour, 4f), 0.11f, 5.4f);
                FormFx.Lay(FormMark.Scorch, run.A, 4f, FormKit.Stain(cast.Colour), 3f);
                FormFx.Lay(FormMark.Cross, run.A, 5f, FormKit.Lit(cast.Colour, 2.6f), 1.2f, -1f, 0f, 30f);
                ElementLook.Sparks(cast.Element, chest, Vector3.up, 2f);
                if (landed > 0)
                {
                    FormKit.Flash(cast, 0.34f, 0.08f);
                    if (cast.Upgrades != null) cast.Upgrades.Jolt(0.17f, 0.55f, 0.4f);
                }
                return true;
            }
            player.FormPoseProgress = Mathf.Lerp(0.9f, 0.99f, (run.Age - run.T2) / AfterSeconds);
            return run.Age < run.T2 + AfterSeconds;
        }
    }
}

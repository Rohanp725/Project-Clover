using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// A guard of the element: a sign held up in front of the player for a while. The first blow that comes, from any
    /// side, is turned for nothing and answered with a cone of the element thrown back at whoever struck. If nothing
    /// comes, the sign goes out and the mana is lost. The dodge lets it go.
    /// </summary>
    public class ParryForm : RunningForm
    {
        public override string Id => SpellForms.Parry;

        private const float AnswerSeconds = 0.32f;

        public override int Cast(in SpellCast cast)
        {
            SpellWorks.EndRunsOf(this);
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            if (!SpellWorks.TakeBody(ref run, false, true))
            {
                run.Phase = 9;
                return 0;
            }
            var player = cast.Player;
            player.FormParried = false;
            player.FormParrying = true;
            player.FormGuardPose = true;
            player.FormSwordOut = true;
            SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Sigil, FormKit.Lit(cast.Colour, 2.6f));
            SpellWorks.Marks(run)[1] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(cast.Colour, 2f));
            ElementLook.Sparks(cast.Element, cast.Origin + Vector3.up * 1.2f + cast.Forward * 0.7f, cast.Forward, 0.6f);
            return 0;
        }

        // Fire's answer (Kiln Stance): the blow caught counts as a perfect guard, and a pillar of fire stands up under whoever struck.
        private static void Kiln(ref SpellRun run, in SpellBlow blow)
        {
            var cast = run.Cast;
            var player = cast.Player;
            Vector3 from = player.FormParriedFrom;
            var attacker = cast.Combat.EnemyNear(from, 2f, null);
            Vector3 at = attacker != null ? attacker.transform.position : from;
            at.y = player.transform.position.y;
            if (cast.Defence != null)
            {
                cast.Combat.Mana.Gain(cast.Defence.PerfectGuardMana);
                cast.Defence.Callout("Perfect guard");
            }
            cast.Combat.PerfectGuarded(from);
            if (cast.Upgrades != null) cast.Upgrades.OnPerfectGuard(from);
            cast.Combat.StrikeArea(at, 1.7f, blow);
            // The pillar: a column of light, tall flames wound round it, a crown of fire at its foot.
            SigKit.Column(at, FormKit.Lit(cast.Colour, 3.6f), 1.6f, 0.3f);
            for (int i = 0; i < 6; i++)
            {
                Vector3 outward = FormRules.OnRing(i, 6, 0.55f + 0.12f * (i % 2), 15f);
                FormFx.Erupt(FormMesh.Flame, at + outward + Vector3.up * (0.5f * (i % 3)), Quaternion.Euler(0f, i * 60f, 0f), new Vector3(1.3f, 4.2f + 0.8f * (i % 2), 1.3f), cast.Colour, 0.07f + 0.02f * i, 0.45f, 0.4f);
            }
            SigKit.Crown(at, 1.3f, 8, 1.4f, cast.Colour, 0.4f);
            FormKit.Impact(cast, at, 1.7f, 1.5f);
            FormFx.Lay(FormMark.Ring, at, 0.6f, FormKit.Lit(cast.Colour, 4f), 0.35f, 5f);
            FormKit.Flash(cast, 0.3f, 0.07f);
            if (cast.Upgrades != null) cast.Upgrades.Jolt(0.13f, 0.5f, 0.3f);
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            if (run.Phase == 9) return false;
            var cast = run.Cast;
            var player = cast.Player;
            var marks = SpellWorks.Marks(run);
            Transform body = player.transform;
            if (run.Phase == 1)
            {
                player.FormPose = FormKit.Draw;
                player.FormPoseProgress = Mathf.Lerp(0.3f, 0.98f, (run.Age - run.T0) / AnswerSeconds);
                return run.Age < run.T0 + AnswerSeconds;
            }
            if (player.FormParried)
            {
                // Turned: face whoever struck and give it back.
                player.FormParried = false;
                player.FormGuardPose = false;
                Vector3 toward = player.FormParriedFrom - body.position;
                toward.y = 0f;
                if (toward.sqrMagnitude > 0.01f) body.rotation = Quaternion.LookRotation(toward, Vector3.up);
                var blow = cast.Blow(cast.Damage, cast.Stagger);
                blow.Heavy = true;
                if (cast.Affliction == Clover.Gameplay.AfflictionKind.Burn)
                {
                    Kiln(ref run, blow);
                    FormFx.ReleaseMark(marks[0]);
                    FormFx.ReleaseMark(marks[1], 0.1f);
                    marks[0] = marks[1] = -1;
                    run.Phase = 1;
                    run.T0 = run.Age;
                    return true;
                }
                cast.Combat.StrikeCone(cast.Reach, cast.Spec.Arc, blow);
                Vector3 chest = body.position + Vector3.up * 1.1f;
                cast.Caster.ShowArc(cast.Reach, cast.Spec.Arc, 1f, cast.Element != null ? cast.Element.Glow : cast.Colour);
                cast.Caster.ShowArc(cast.Reach * 0.6f, cast.Spec.Arc, -1f, cast.Element != null ? cast.Element.Glow : cast.Colour);
                int rays = 7;
                for (int i = 0; i < rays; i++)
                {
                    Vector3 ray = Quaternion.Euler(0f, FormRules.InFan(i, rays, cast.Spec.Arc * 0.9f), 0f) * body.forward;
                    FormFx.Run(FormMark.Streak, body.position + ray * 0.8f, ray, cast.Reach - 0.8f, 0.9f, FormKit.Lit(cast.Colour, 3.4f), 0.2f, 0.1f);
                    FormFx.Fly(FormMesh.Shard, chest + ray * 0.8f, chest + ray * cast.Reach, Quaternion.LookRotation(ray), Vector3.one * 1.7f, cast.Colour, 0.2f);
                }
                FormFx.Face(FormMark.Star, chest + body.forward * 0.9f, 3.6f, FormKit.Lit(cast.Colour, 4f), 0.11f, 4.8f);
                FormKit.Flash(cast, 0.3f, 0.07f);
                if (cast.Upgrades != null) cast.Upgrades.Jolt(0.13f, 0.45f, 0.3f);
                FormFx.ReleaseMark(marks[0]);
                FormFx.ReleaseMark(marks[1], 0.1f);
                marks[0] = marks[1] = -1;
                run.Phase = 1;
                run.T0 = run.Age;
                return true;
            }
            float seconds = Mathf.Max(0.2f, cast.Spec.Seconds);
            if (run.Age >= seconds)
            {
                ElementLook.Puff(cast.Element, body.position + Vector3.up * 1.2f + body.forward * 0.7f, Vector3.up, 0.5f);
                return false;
            }
            // The sign leans in front of the chest, tipped back so the high camera sees its face, turning; it thins as its time runs out.
            float left = 1f - run.Age / seconds;
            float snap = Mathf.Clamp01(run.Age / 0.08f);
            float size = 2.3f * (snap < 1f ? 1.5f - 0.5f * snap : 0.85f + 0.15f * left);
            FormFx.PlaceMark(marks[0], body.position + Vector3.up * 1.15f + body.forward * 0.75f,
                Quaternion.LookRotation(Vector3.Cross(body.forward, Vector3.up), body.forward + Vector3.up * 1.3f) * Quaternion.Euler(0f, run.Age * 160f, 0f), new Vector3(size, 1f, size), false);
            FormFx.PlaceMark(marks[1], body.position, Quaternion.identity, new Vector3(2.2f, 1f, 2.2f));
            return true;
        }
    }
}

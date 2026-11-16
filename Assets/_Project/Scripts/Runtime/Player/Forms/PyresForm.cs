using Clover.Core;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Pyre's crown, Light the Pyres. The player kneels and drives the weapon into the ground; a line of fire runs to
    /// every burning or poisoned enemy near and follows it. After three breaths a pillar of flame rises under each,
    /// and whatever burn and poison it carries goes off with it. The dodge, or a blow taken, breaks it and nothing rises.
    /// Reads: Count (most enemies), Reach, Radius, Damage, Stagger, Stacks, Delay (the three breaths together).
    /// </summary>
    public class PyresForm : RunningForm
    {
        public override string Id => SpellForms.Pyres;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            var things = SpellWorks.Things(run);
            int near = SigKit.Gather(run, cast.Origin, cast.Reach, SpellWorks.NotesEach);
            int most = Mathf.Clamp(cast.Spec.Count, 1, SpellWorks.MarksEach);
            int lit = 0;
            for (int i = 0; i < near; i++)
                if (lit < most && (SigKit.Has(things[i], AfflictionKind.Burn) || SigKit.Has(things[i], AfflictionKind.Poison))) things[lit++] = things[i];
            for (int i = lit; i < near; i++) things[i] = null;
            run.Count = lit;
            if (lit == 0 || !SpellWorks.TakeBody(ref run, false, true))
            {
                // Nothing to light: the mana comes back.
                cast.Combat.Mana.Gain(cast.Spec.Cost);
                if (cast.Defence != null) cast.Defence.Callout(lit == 0 ? "Nothing burns" : "Not now");
                run.Phase = 9;
                return 0;
            }
            run.T0 = Mathf.Max(0.6f, cast.Spec.Delay);
            var marks = SpellWorks.Marks(run);
            for (int i = 0; i < lit; i++) marks[i] = FormFx.HoldMark(FormMark.Beam, FormKit.Lit(Elements.Colour(Elements.Fire), 2.8f));
            FormFx.Lay(FormMark.Crack, cast.Origin, 4f, FormKit.Lit(cast.Colour, 2.6f), run.T0 + 0.5f);
            FormFx.Lay(FormMark.Sigil, cast.Origin, 3.4f, FormKit.Lit(cast.Colour, 2.4f), run.T0, -1f, 60f);
            return 0;
        }

        public override bool BodyLost(ref SpellRun run) => false;

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            if (run.Phase == 9) return false;
            var cast = run.Cast;
            var player = cast.Player;
            var marks = SpellWorks.Marks(run);
            var things = SpellWorks.Things(run);
            var points = SpellWorks.Points(run);
            Vector3 here = player.transform.position;
            player.FormPose = FormKit.Brace;
            player.FormPoseProgress = Mathf.Clamp01(run.Age / 0.3f) * 0.9f;
            player.FormSwordOut = true;
            float t = run.Age / run.T0;
            float reached = Mathf.Clamp01(run.Age / 0.45f);
            for (int i = 0; i < run.Count; i++)
            {
                if (FormKit.Alive(things[i])) points[i] = SigKit.Foot(things[i], here.y);
                Vector3 line = points[i] - here;
                if (line.sqrMagnitude < 0.01f) continue;
                FormFx.PlaceMark(marks[i], here, Quaternion.LookRotation(line.normalized, Vector3.up), new Vector3(0.5f + 0.5f * t + 0.15f * Mathf.Sin(run.Age * 20f + i), 1f, line.magnitude * reached));
            }
            // Three breaths: each one a ring out from the kneeling player and a tighter one on every pyre.
            int breath = Mathf.Min(3, Mathf.FloorToInt(t * 3f) + 1);
            if (breath > run.Done && run.Age < run.T0)
            {
                run.Done = breath;
                float third = run.T0 / 3f;
                FormFx.Lay(FormMark.Ring, here, 1f, FormKit.Lit(cast.Colour, 3f), third * 0.8f, 4f + breath);
                for (int i = 0; i < run.Count; i++)
                {
                    FormFx.Lay(FormMark.Ring, points[i], cast.Spec.Radius * 3f, FormKit.Lit(cast.Colour, 2.4f + 0.5f * breath), third, cast.Spec.Radius * 1.2f);
                    FormFx.Lay(FormMark.Disc, points[i], cast.Spec.Radius * 2f, FormKit.Lit(cast.Colour, 1.2f + 0.4f * breath), third);
                }
                if (cast.Upgrades != null) cast.Upgrades.Jolt(0f, 0.08f * breath, 0.12f);
            }
            if (run.Age < run.T0) return true;
            // The pyres.
            float radius = cast.Spec.Radius;
            for (int i = 0; i < run.Count; i++)
            {
                Vector3 at = points[i];
                float more = 0f;
                var set = SigKit.Set(things[i]);
                if (set != null && FormKit.Alive(things[i]) && set.Has(AfflictionKind.Poison)) more = set.SetOff(AfflictionKind.Poison);
                var blow = cast.Blow(cast.Damage + more, cast.Stagger);
                blow.Detonate = true;
                blow.Heavy = true;
                // Pyres that stand close together do not strike the same enemy twice.
                cast.Combat.StrikeArea(at, radius, blow, SpellWorks.Once(run));
                FormFx.Erupt(FormMesh.Pillar, at, Quaternion.Euler(0f, i * 40f, 0f), new Vector3(radius * 1.1f, 7f, radius * 1.1f), Elements.Colour(Elements.Fire), 0.07f, 0.5f, 0.35f, 0.1f);
                SigKit.Crown(at, radius * 0.8f, 7, 2f, cast.Colour, 0.4f, i * 13f);
                // The pillar is flame whichever of the two elements is held; the crown round it is the held one.
                Vfx.Play(VfxKind.Fire, at + Vector3.up, Vector3.up, 1.6f);
                SigKit.Column(at, FormKit.Lit(Elements.Colour(Elements.Fire), 3.6f), radius * 1.4f, 0.35f);
                FormKit.Impact(cast, at, radius, 1.5f);
            }
            SigKit.Moment(cast, 0.2f, 0.9f, 0.4f, 0.4f, 0.5f);
            return false;
        }
    }
}

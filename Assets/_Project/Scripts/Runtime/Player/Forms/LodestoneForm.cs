using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Lightning's pull, Lodestone. The caster raises a hand and a line of lightning leaps from it to every enemy
    /// within the spell's radius; each is dragged toward the caster along its line, a metre and more for every stack
    /// of Static on it. When the delay is up everything within the burst's reach of the caster is struck, and all
    /// the Static on it is set off at once and added to the blow. Sums in <see cref="LodestoneRules"/>.
    /// Reads: Radius (how far it reaches for enemies), Delay, Damage, Stagger.
    /// </summary>
    public class LodestoneForm : RunningForm
    {
        public override string Id => SpellForms.Lodestone;

        private const int Most = 8;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            run.A = cast.Origin;
            run.T0 = FormRules.Warning(cast.Spec.Delay);
            run.Count = SigKit.Gather(run, run.A, cast.Spec.Radius, Most);
            var solids = SpellWorks.Solids(run);
            var times = SpellWorks.Times(run);
            var things = SpellWorks.Things(run);
            for (int i = 0; i < run.Count; i++)
            {
                Vector3 off = things[i].transform.position - run.A;
                off.y = 0f;
                times[i] = LodestoneRules.Dragged(off.magnitude, SigKit.Depth(things[i], AfflictionKind.Static));
                solids[i] = FormFx.Hold(FormMesh.Bolt, cast.Colour, 0.03f);
                solids[Most + i] = FormFx.Hold(FormMesh.Bolt, Color.white, 0.02f);
            }
            SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Ring, FormKit.Lit(cast.Colour, 2.4f));
            FormFx.Lay(FormMark.Ring, run.A, cast.Spec.Radius * 2f, FormKit.Lit(cast.Colour, 2.6f), run.T0, LodestoneRules.BurstRadius * 2f);
            SpellWorks.Gesture(cast, FormKit.Cast, run.T0 + 0.15f);
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var solids = SpellWorks.Solids(run);
            var times = SpellWorks.Times(run);
            var things = SpellWorks.Things(run);
            Vector3 here = cast.Player.transform.position;
            Vector3 hand = here + Vector3.up * 1.9f;
            if (run.Age < run.T0)
            {
                float share = Mathf.Min(deltaTime / run.T0, 1f - run.T1);
                run.T1 += Mathf.Max(0f, share);
                for (int i = 0; i < run.Count; i++)
                {
                    var thing = things[i];
                    if (!FormKit.Alive(thing))
                    {
                        FormFx.Place(solids[i], hand, Quaternion.identity, Vector3.zero);
                        FormFx.Place(solids[Most + i], hand, Quaternion.identity, Vector3.zero);
                        continue;
                    }
                    Vector3 chest = thing.transform.position + Vector3.up * 1.1f;
                    Vector3 toward = here - thing.transform.position;
                    toward.y = 0f;
                    // A shove moves a body two thirds of its length: half as much again drags it the whole way.
                    if (share > 0f && times[i] > 0f && toward.sqrMagnitude > 0.01f && thing is IShovable body)
                        body.Push(toward.normalized * (times[i] * share * 1.5f), PushKind.Drag);
                    // The line it is dragged by: thick for one that carries much Static, with a white thread in it.
                    float thick = 1.6f + 0.35f * Mathf.Min(8f, times[i]);
                    Lay(solids[i], hand, chest, thick);
                    Lay(solids[Most + i], chest, hand, thick * 0.5f);
                }
                FormFx.PlaceMark(SpellWorks.Marks(run)[0], here, Quaternion.Euler(0f, run.Age * 300f, 0f), Vector3.one * (LodestoneRules.BurstRadius * 2f));
                run.T2 -= deltaTime;
                if (run.T2 <= 0f)
                {
                    run.T2 = 0.07f;
                    ElementLook.Sparks(cast.Element, hand, Vector3.up, 0.5f);
                }
                return true;
            }

            // The discharge: everything close is struck, and every stack of Static on it goes off with the blow.
            int near = SigKit.Gather(run, here, LodestoneRules.BurstRadius, Most);
            float spent = 0f;
            for (int i = 0; i < near; i++)
            {
                var thing = things[i];
                var set = SigKit.Set(thing);
                float charge = set != null ? set.SetOff(AfflictionKind.Static) : 0f;
                spent += charge;
                var blow = cast.Blow(cast.Damage + charge, cast.Stagger, 0);
                blow.Heavy = true;
                Vector3 foot = thing.transform.position;
                // Struck from its far side, so the blow's own knock is toward the caster, not back the way it was dragged.
                cast.Combat.StrikeOne(thing, blow, foot + (foot - here));
                SigKit.Column(foot, FormKit.Lit(cast.Colour, 3.4f), 0.9f + Mathf.Min(1.2f, charge * 0.03f), 0.16f);
                FormFx.Drop(FormMesh.Bolt, foot + Vector3.up * 7f, foot, Quaternion.LookRotation(Vector3.down), new Vector3(3f, 3f, 7f), Color.white, 0.04f, 0.1f, 0.08f, 0.03f);
                FormFx.Lay(FormMark.Crack, foot, 2.4f, FormKit.Lit(cast.Colour, 2.4f), 1.2f, -1f, 0f, i * 50f);
            }
            FormKit.Impact(cast, here + cast.Player.transform.forward * 0.6f, LodestoneRules.BurstRadius * 0.6f, near > 0 ? 1.5f : 0.7f);
            FormFx.Lay(FormMark.Ring, here, 1f, FormKit.Lit(cast.Colour, 4f), 0.3f, LodestoneRules.BurstRadius * 2.4f);
            for (int i = 0; i < 10; i++)
            {
                Vector3 outward = FormRules.OnRing(i, 10, 1f, 17f);
                FormFx.Fly(FormMesh.Bolt, hand - Vector3.up * 0.7f, here + outward * LodestoneRules.BurstRadius + Vector3.up * 0.3f, Quaternion.LookRotation(outward - Vector3.up * 0.3f), new Vector3(2f, 2f, LodestoneRules.BurstRadius), i % 2 == 0 ? Color.white : cast.Colour, 0.16f, 0.03f);
            }
            if (near > 0) SigKit.Moment(cast, 0.07f + Mathf.Min(0.1f, spent * 0.002f), 0.5f, 0f, 1f, 0.3f);
            run.Done = near;
            run.T3 = spent;
            return false;
        }

        // A length of lightning along +Z laid from one point to another.
        private static void Lay(int handle, Vector3 from, Vector3 to, float thick)
        {
            Vector3 along = to - from;
            float length = along.magnitude;
            if (length < 0.01f) return;
            FormFx.Place(handle, from, Quaternion.LookRotation(along / length), new Vector3(thick, thick, length));
        }
    }
}

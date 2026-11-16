using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Poison's gift, Clean Blade. While the surge lasts poison does not bite and does not fade: every beat it would
    /// have bitten is owed. When the surge ends time slows, the blade is flicked clean, and every bite owed by every
    /// poisoned enemy near lands at once.
    /// Reads: Reach, Stagger.
    /// </summary>
    public class CleanBladeForm : RunningForm
    {
        public override string Id => SpellForms.CleanBlade;

        private const float Flick = 0.32f;

        public override int Cast(in SpellCast cast)
        {
            ref var run = ref SpellWorks.Begin(this, cast, out bool begun);
            if (!begun) return 0;
            Afflictions.PoisonWaits = true;
            SpellWorks.Marks(run)[0] = FormFx.HoldMark(FormMark.Sigil, FormKit.Lit(cast.Colour, 1.5f));
            return 0;
        }

        public override bool Tick(ref SpellRun run, float deltaTime)
        {
            var cast = run.Cast;
            var marks = SpellWorks.Marks(run);
            Vector3 here = cast.Player.transform.position;
            if (run.Phase == 0)
            {
                FormFx.PlaceMark(marks[0], here, Quaternion.Euler(0f, -run.Age * 50f, 0f), new Vector3(2.6f, 1f, 2.6f));
                if (SigKit.SurgeOn(cast)) return true;
                run.Phase = 1;
                run.T1 = run.Age;
                FormFx.ReleaseMark(marks[0], 0.2f);
                marks[0] = -1;
                run.Count = SigKit.Gather(run, here, cast.Reach, SpellWorks.NotesEach);
                var owing = SpellWorks.Things(run);
                for (int i = 0; i < run.Count; i++)
                    if (SigKit.Has(owing[i], AfflictionKind.Poison))
                        FormFx.Face(FormMark.Brackets, owing[i].transform.position + Vector3.up * 1.2f, 2.8f, FormKit.Lit(cast.Colour, 3f), Flick + 0.1f, 1.6f);
                SpellWorks.Gesture(cast, FormKit.Sweep, Flick + 0.25f, 0.1f, 1f, true);
                SigKit.Moment(cast, 0f, 0f, 0.7f, 0.3f, 0.15f);
                return true;
            }
            if (run.Age < run.T1 + Flick) return true;
            Afflictions.PoisonWaits = false;
            var things = SpellWorks.Things(run);
            float total = 0f;
            int paid = 0;
            for (int i = 0; i < run.Count; i++)
            {
                var set = SigKit.Set(things[i]);
                if (set == null || !FormKit.Alive(things[i])) continue;
                float owed = set.TakeOwed();
                if (owed <= 0f) continue;
                Vector3 chest = things[i].transform.position + Vector3.up * 1.2f;
                cast.Combat.StrikeOne(things[i], cast.Blow(owed, cast.Stagger, 0), here);
                FormFx.Cut(chest, Quaternion.Euler(0f, 35f + i * 50f, 0f) * Vector3.forward, 6f, 0.8f, FormKit.Lit(cast.Colour, 4f), 0.18f, 0.05f);
                FormFx.Face(FormMark.Star, chest, 1.8f, FormKit.Lit(cast.Colour, 4f), 0.1f, 3f);
                FormFx.Lay(FormMark.Scorch, SigKit.Foot(things[i], here.y), 3.2f, FormKit.Stain(cast.Colour), 3f);
                FormFx.Run(FormMark.Streak, here, things[i].transform.position - here, (things[i].transform.position - here).magnitude, 0.5f, FormKit.Lit(cast.Colour, 3f), 0.2f, 0.1f);
                ElementLook.Sparks(cast.Element, chest, Vector3.up, 1.4f);
                total += owed;
                paid++;
            }
            LastOwed = total;
            FormFx.Lay(FormMark.Ring, here, 1f, FormKit.Lit(cast.Colour, 3.4f), 0.35f, cast.Reach * 1.2f);
            if (paid > 0) SigKit.Moment(cast, 0.16f, 0.6f, 0f, 1f, 0.35f);
            return false;
        }

        /// <summary>What the last flick paid out in all, for probes.</summary>
        public static float LastOwed { get; private set; }

        // Domain reload is off: a number from the last play must not be read in this one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => LastOwed = 0f;

        public override void End(ref SpellRun run) => Afflictions.PoisonWaits = false;
    }
}

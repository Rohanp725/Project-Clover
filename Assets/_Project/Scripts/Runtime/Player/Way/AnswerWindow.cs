using Clover.Player.Trees.Ways;
using UnityEngine;

namespace Clover.Player.Way
{
    /// <summary>
    /// The Answer's moment to answer (way_build_plan.md 2.7, "the answer window", and 3.4): a perfect guard opens it for
    /// 0.6 s (Open Window 0.8 s; Catch the Red 1.0 s after a red attack turned); light attack in it is the Riposte (24,
    /// critical, 20 stagger), heavy is Through the Gap (30, 40 stagger, through the attacker). Patient Blade adds 10
    /// stagger to the next answer for each 0.5 s the guard was held unstruck, to +40. Second Answer keeps the moment open
    /// for a second perfect guard, and a second answer in it +30 stagger. Kiri-otoshi answers by itself, the Riposte at
    /// once on every perfect guard. Kept in the player's <see cref="AspectRun"/>: <see cref="AspectRun.Window"/> the
    /// moment left, <see cref="AspectRun.Stage"/> answers given in it, <see cref="AspectRun.Count"/> a perfect guard not yet answered, <see cref="AspectRun.SinceCount"/> the guard held
    /// unstruck. Plain, so it is tested without a player.
    /// </summary>
    public static class AnswerWindow
    {
        public const float Seconds = 0.6f, OpenWindowSeconds = 0.8f, RedSeconds = 1f;
        public const float PatientEvery = 0.5f, PatientStagger = 10f, PatientMost = 40f;
        public const float SecondAnswerStagger = 30f;
        /// <summary>Read the Red: the perfect window this much wider against red attacks.</summary>
        public const float ReadTheRedSeconds = 0.04f;
        /// <summary>Answers one moment allows with Second Answer.</summary>
        public const int MostAnswers = 2;
        /// <summary>(P) The Riposte: the guard snaps straight into the cut.</summary>
        public const float RiposteWindup = 0.06f, RiposteActive = 0.08f, RiposteRecovery = 0.3f, RiposteLunge = 0.5f, RiposteHitStop = 0.08f;
        /// <summary>(P) Through the Gap: through the attacker, untouchable while passing, standing behind it.</summary>
        public const float GapWindup = 0.08f, GapRecovery = 0.35f;

        /// <summary>How long a perfect guard opens the moment for, under the marks in force.</summary>
        public static float LengthFor(IMarkQuery marks, bool red)
        {
            if (marks == null) return Seconds;
            if (red && marks.Has(KatanaWay.CatchTheRed)) return RedSeconds;
            return marks.Has(KatanaWay.OpenWindow) ? OpenWindowSeconds : Seconds;
        }

        /// <summary>One step of the Answer: a perfect guard opens (or, with Second Answer, keeps open) the moment; the guard held unstruck counts for Patient Blade.</summary>
        public static void Tick(ref AspectRun run, in AspectContext context)
        {
            var marks = context.Marks;
            if (run.Window > 0f)
            {
                run.Window -= context.DeltaTime;
                if (run.Window <= 0f)
                {
                    run.Window = 0f;
                    run.Stage = 0;
                }
            }
            if (context.PerfectGuard)
            {
                // A new moment, or with Second Answer the open one kept for another answer.
                if (run.Window <= 0f || (marks != null && marks.Has(KatanaWay.SecondAnswer))) run.Window = Mathf.Max(run.Window, LengthFor(marks, context.RedTurned));
                if (run.Stage >= MostAnswers) run.Stage = 0;
                // A guard to answer: one answer for each perfect guard.
                run.Count = 1;
            }
            bool patient = marks != null && marks.Has(KatanaWay.PatientBlade);
            if (!patient || context.Struck || context.GuardHit || !context.Guarding) run.SinceCount = 0f;
            else run.SinceCount += context.DeltaTime;
        }

        /// <summary>True while an answer can be given.</summary>
        public static bool Open(in AspectRun run) => run.Window > 0f;

        /// <summary>Patient Blade's stagger for a guard held unstruck this long: 10 for each half second, at most 40.</summary>
        public static float PatientBonus(float heldSeconds) => Mathf.Min(PatientMost, Mathf.Floor(heldSeconds / PatientEvery + 1e-4f) * PatientStagger);

        /// <summary>
        /// The answer given: the Riposte (light) or Through the Gap (heavy), built from the weapon in hand with what the
        /// marks add, and the moment spent (kept open with Second Answer for one more). Null when there is no moment open.
        /// </summary>
        public static AttackStep Answer(ref AspectRun run, Weapon weapon, bool heavy, IMarkQuery marks)
        {
            if (!Open(run) || run.Count <= 0 || weapon == null) return null;
            var step = heavy ? Aspects.AnswerAspect.ThroughTheGap(weapon) : Aspects.AnswerAspect.Riposte(weapon);
            if (step == null) return null;
            float bonus = 0f;
            if (marks != null && marks.Has(KatanaWay.PatientBlade)) bonus += PatientBonus(run.SinceCount);
            if (marks != null && marks.Has(KatanaWay.SecondAnswer) && run.Stage >= 1) bonus += SecondAnswerStagger;
            step.Stagger += bonus;
            run.SinceCount = 0f;
            run.Count = 0;
            run.Stage++;
            bool keep = marks != null && marks.Has(KatanaWay.SecondAnswer) && run.Stage < MostAnswers;
            if (!keep) run.Window = 0f;
            return step;
        }

        /// <summary>Kiri-otoshi: the answer comes by itself on every perfect guard.</summary>
        public static bool Answers(IMarkQuery marks) => marks != null && marks.Has(KatanaWay.KiriOtoshi);
    }
}

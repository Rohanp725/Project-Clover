using UnityEngine;

namespace Clover.Player.Way
{
    /// <summary>
    /// The thunder god's drums on the Storm Katana's bough (way_build_plan.md 3.5; questions 15 and 22, the owner's
    /// answers): each cut that lands beats one of seven drums (Seven Drums); perfect guards and finishers each beat two
    /// (Drumbeat). With seven beaten, the next draw (any draw: the first cut of a combo, the Thunder Draw or the Red Draw)
    /// breaks balance +40 and spends them. Drums fade once the fight is over, unless Rolling Drums keeps them. Kept in
    /// the player's <see cref="AspectRun"/>: <see cref="AspectRun.Count"/> the drums beaten, <see cref="AspectRun.SinceCount"/>
    /// the seconds out of a fight. Plain, so it is tested without a player.
    /// </summary>
    public static class Drums
    {
        public const int Most = 7;
        /// <summary>Drums a perfect guard or a finisher beats under Drumbeat.</summary>
        public const int DrumbeatBeats = 2;
        /// <summary>Stagger the next draw adds with seven beaten.</summary>
        public const float DrawStagger = 40f;
        /// <summary>(P) Seconds out of a fight before the drums go quiet.</summary>
        public const float FadeSeconds = 3f;

        /// <summary>Beats this many drums, up to seven.</summary>
        public static void Beat(ref AspectRun run, int beats)
        {
            if (beats <= 0) return;
            run.Count = Mathf.Min(Most, run.Count + beats);
        }

        /// <summary>True with all seven beaten.</summary>
        public static bool Full(in AspectRun run) => run.Count >= Most;

        /// <summary>One step: the drums beaten by what happened, and their fading out of a fight.</summary>
        public static void Tick(ref AspectRun run, in AspectContext context, bool drumbeat, bool rolling)
        {
            int beats = context.Landed;
            if (drumbeat) beats += (context.PerfectGuard ? DrumbeatBeats : 0) + (context.Finished ? DrumbeatBeats : 0);
            Beat(ref run, beats);
            if (context.InFight || rolling)
            {
                run.SinceCount = 0f;
                return;
            }
            run.SinceCount += context.DeltaTime;
            if (run.SinceCount >= FadeSeconds) run.Count = 0;
        }

        /// <summary>A draw goes out: with seven beaten it takes <see cref="DrawStagger"/> and spends them. Returns the stagger added.</summary>
        public static float SpendOnDraw(ref AspectRun run, bool draw)
        {
            if (!draw || !Full(run)) return 0f;
            run.Count = 0;
            return DrawStagger;
        }
    }
}

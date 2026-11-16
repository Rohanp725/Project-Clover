namespace Clover.Player
{
    /// <summary>
    /// Decides when a tip may be said. A tip is asked for when the player has just met the thing it is about, and is
    /// said once in a play session, on the status line, when that line is free: not straight after a scene opens, not
    /// close behind another tip, and not over a word the fight itself has just put there. A tip that has to wait too
    /// long is let go and asked for again the next time its moment comes, so it never arrives after the moment has gone.
    /// </summary>
    public sealed class TipRules
    {
        public const string GuardText = "Hold guard to turn a blow";
        public const string BreakGuardText = "A heavy blow breaks a guard";
        public const string FinishText = "Heavy blow: finish it";

        /// <summary>Seconds after a scene opens in which no tip is said: the player is still looking around.</summary>
        public float QuietSeconds = 5f;
        /// <summary>Seconds that must pass between one tip and the next.</summary>
        public float SpacingSeconds = 6f;
        /// <summary>Seconds the status line belongs to another word after it is put there.</summary>
        public float BusySeconds = 1f;
        /// <summary>Seconds a tip waits for the line before it is let go.</summary>
        public float HoldSeconds = 4f;
        /// <summary>Unguarded blows the player takes before being told about the guard.</summary>
        public int UnguardedBlowsNeeded = 2;
        /// <summary>Two blows turned on an enemy's guard within this many seconds earn the tip about heavy blows.</summary>
        public float TurnedWindowSeconds = 3f;
        /// <summary>An enemy staggered farther away than this is not the player's to finish.</summary>
        public float StaggerRange = 6f;

        private const int Count = 3;
        // Indexed by Tip.
        private readonly bool[] shown = new bool[Count];
        private readonly bool[] waiting = new bool[Count];
        private readonly float[] waited = new float[Count];
        private bool silent;
        private float sinceSceneBegan;
        private float sinceTip = float.PositiveInfinity;
        private float sinceLineUsed = float.PositiveInfinity;
        private float sinceTurned = float.PositiveInfinity;
        private int unguardedBlows;

        public bool WasShown(Tip tip) => shown[(int)tip];
        public bool IsWaiting(Tip tip) => waiting[(int)tip];

        public static string TextOf(Tip tip)
        {
            switch (tip)
            {
                case Tip.Guard: return GuardText;
                case Tip.BreakGuard: return BreakGuardText;
                default: return FinishText;
            }
        }

        /// <summary>A scene has opened. In a silent scene (the hub) nothing is counted and nothing is said.</summary>
        public void BeginScene(bool silentHere)
        {
            silent = silentHere;
            sinceSceneBegan = 0f;
            sinceTurned = float.PositiveInfinity;
            // What was waiting belonged to the scene that has gone.
            for (int i = 0; i < Count; i++) waiting[i] = false;
        }

        /// <summary>A blow landed on the player with no guard in its way.</summary>
        public void UnguardedBlow()
        {
            if (silent) return;
            unguardedBlows++;
            if (unguardedBlows >= UnguardedBlowsNeeded) Ask(Tip.Guard);
        }

        /// <summary>An enemy turned one of the player's light blows on its guard.</summary>
        public void BlowTurned()
        {
            if (silent) return;
            if (sinceTurned <= TurnedWindowSeconds) Ask(Tip.BreakGuard);
            sinceTurned = 0f;
        }

        /// <summary>An enemy lost its balance this far from the player.</summary>
        public void EnemyStaggered(float distance)
        {
            if (silent || distance > StaggerRange) return;
            Ask(Tip.Finish);
        }

        /// <summary>The player did the thing unprompted, so there is nothing left to tell.</summary>
        public void Learned(Tip tip)
        {
            shown[(int)tip] = true;
            waiting[(int)tip] = false;
        }

        /// <summary>Something other than a tip was put on the status line.</summary>
        public void LineUsed() => sinceLineUsed = 0f;

        /// <summary>Moves time on, and returns the tip to say now, or null. A tip returned counts as said.</summary>
        public string Tick(float deltaTime)
        {
            sinceSceneBegan += deltaTime;
            sinceTip += deltaTime;
            sinceLineUsed += deltaTime;
            sinceTurned += deltaTime;
            for (int i = 0; i < Count; i++)
            {
                if (!waiting[i]) continue;
                waited[i] += deltaTime;
                if (waited[i] > HoldSeconds) waiting[i] = false;
            }
            if (silent || sinceSceneBegan < QuietSeconds || sinceTip < SpacingSeconds || sinceLineUsed < BusySeconds) return null;

            // The freshest one: it is about what the player is looking at now.
            int pick = -1;
            for (int i = 0; i < Count; i++)
                if (waiting[i] && (pick < 0 || waited[i] < waited[pick])) pick = i;
            if (pick < 0) return null;
            waiting[pick] = false;
            shown[pick] = true;
            sinceTip = 0f;
            return TextOf((Tip)pick);
        }

        private void Ask(Tip tip)
        {
            int index = (int)tip;
            if (shown[index]) return;
            waiting[index] = true;
            waited[index] = 0f;
        }
    }
}

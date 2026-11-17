namespace Clover.Gameplay
{
    /// <summary>
    /// Embers after a fall (fire_rework.md, section 3): the embers lost are spread over the enemies killed since the
    /// last rest, each of whom carries its share back and gives it up on its next death; a second fall before they are
    /// all reclaimed loses what is left for good. Enemies are known by their index in the scene's list. Plain numbers,
    /// so the rule can be tested without a scene.
    /// </summary>
    public class EmberDebt
    {
        private readonly int[] owed;
        private readonly bool[] killed;

        public EmberDebt(int enemies)
        {
            owed = new int[enemies < 0 ? 0 : enemies];
            killed = new bool[owed.Length];
        }

        /// <summary>Embers waiting in the enemies that carry them.</summary>
        public int Waiting { get; private set; }
        /// <summary>Embers lost for good, in all.</summary>
        public int Forfeited { get; private set; }

        public int Owed(int index) => index >= 0 && index < owed.Length ? owed[index] : 0;
        public bool Carries(int index) => Owed(index) > 0;

        /// <summary>An enemy that will stand again has been killed: it is one of those a fall's embers go to.</summary>
        public void Killed(int index)
        {
            if (index >= 0 && index < killed.Length) killed[index] = true;
        }

        /// <summary>A carrier has died again: its share comes back. Returns the embers given.</summary>
        public int Reclaim(int index)
        {
            int share = Owed(index);
            if (share <= 0) return 0;
            owed[index] = 0;
            Waiting -= share;
            return share;
        }

        /// <summary>The player rested: the kills so far no longer carry a fall's embers. What is waiting stays waiting.</summary>
        public void Rested()
        {
            for (int i = 0; i < killed.Length; i++) killed[i] = false;
        }

        /// <summary>
        /// The player fell carrying <paramref name="lost"/> embers. What was still waiting from an earlier fall is lost
        /// for good; the new loss is spread over the enemies killed since the last rest, evenly, the first of them taking
        /// any remainder. With nobody killed there is no one to carry it, and it too is gone. Returns the embers lost for good.
        /// </summary>
        public int Fall(int lost)
        {
            int gone = Waiting;
            for (int i = 0; i < owed.Length; i++) owed[i] = 0;
            Waiting = 0;
            int carriers = 0;
            for (int i = 0; i < killed.Length; i++)
                if (killed[i]) carriers++;
            if (lost > 0 && carriers == 0) gone += lost;
            else if (lost > 0)
            {
                int each = lost / carriers, left = lost - each * carriers;
                for (int i = 0; i < killed.Length; i++)
                {
                    if (!killed[i]) continue;
                    owed[i] = each + (left > 0 ? 1 : 0);
                    if (left > 0) left--;
                    Waiting += owed[i];
                }
            }
            for (int i = 0; i < killed.Length; i++) killed[i] = false;
            Forfeited += gone;
            return gone;
        }
    }
}

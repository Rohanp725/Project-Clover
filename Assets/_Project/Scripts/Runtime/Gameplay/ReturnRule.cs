namespace Clover.Gameplay
{
    /// <summary>
    /// What a fall costs in a land where death returns the player to the last rest fire (docs/design/WORLD.md, "B with
    /// C"): the land's ordinary enemies stand again, a god stays fallen, an elite stays fallen where the land says so,
    /// and the embers being carried are lost where the land says so. Plain rules, so they can be tested without a scene.
    /// </summary>
    public static class ReturnRule
    {
        /// <summary>True when an enemy is put back on its feet by a rest or a fall.</summary>
        public static bool ComesBack(bool isBoss, bool isElite, bool isDead, bool elitesStayFallen)
        {
            if (!isDead) return true;
            if (isBoss) return false;
            return !(isElite && elitesStayFallen);
        }

        /// <summary>The embers still carried after a fall.</summary>
        public static int EmbersAfterFall(int carried, bool loseCarriedEmbers) => loseCarriedEmbers ? 0 : carried;
    }
}

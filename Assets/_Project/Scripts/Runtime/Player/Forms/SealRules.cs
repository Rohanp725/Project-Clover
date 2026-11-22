namespace Clover.Player.Forms
{
    /// <summary>
    /// The numbers of a seal's thunder (Paper Seal; the ledger's seal-thunder), apart from the brand that carries it so
    /// they can be checked without a scene.
    /// </summary>
    public static class SealRules
    {
        /// <summary>What the thunder is multiplied by: all of it at the last blow, half when the seal's time ran out first.</summary>
        public static float Share(bool ranOut) => ranOut ? 0.5f : 1f;

        /// <summary>
        /// The thunder's damage: the cast's own, <paramref name="perDose"/> for each dose the enemy carries, and its
        /// discharged Static (<paramref name="perCharge"/> a charge), all at half when its time ran out.
        /// </summary>
        public static float Thunder(float damage, float perDose, int doses, int charge, float perCharge, bool ranOut) =>
            Share(ranOut) * (damage + perDose * System.Math.Max(0, doses) + perCharge * System.Math.Max(0, charge));
    }
}

namespace Clover.Player.Way
{
    /// <summary>
    /// What a Way's trunk marks do to the weapon's moves in every aspect (steel/weapons.md 0.1; way_plan.md 0.2): the
    /// weapon basics that change a step (the cut out of the roll, the quicker hands) and the temper ranks' own lines.
    /// Temper's raw damage and stagger are stats on its marks, not a patch. Stateless; patched before the aspect, so a
    /// bough that takes over a basic's input (the Stream's Stepping Water over the Rising Draw) has the last word.
    /// </summary>
    public abstract class WayTrunk
    {
        /// <summary>The weapon kind whose trunk it is.</summary>
        public abstract string Weapon { get; }

        public abstract void Patch(Weapon weapon, IMarkQuery marks);
    }
}

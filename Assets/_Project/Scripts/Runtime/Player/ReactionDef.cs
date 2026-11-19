namespace Clover.Player
{
    /// <summary>What a reaction mark (<see cref="UpgradeKind.Reaction"/>) says of its pair beyond its effects (IMPLEMENTATION.md 2.4).</summary>
    public class ReactionDef
    {
        /// <summary>Seconds the pair rests on one enemy once it has reacted; zero takes the pair's entry or the shared default (<see cref="ReactionTable.RestFor"/>).</summary>
        public float Rest;
    }
}

namespace Clover.Player
{
    /// <summary>
    /// For a spell form that can find itself with nothing to do (no enemy it can act on, a body it cannot take): it is
    /// asked before the mana is spent, and a refusal costs the player nothing.
    /// </summary>
    public interface ISpellGate
    {
        bool CanCast(in SpellCast cast);
    }
}

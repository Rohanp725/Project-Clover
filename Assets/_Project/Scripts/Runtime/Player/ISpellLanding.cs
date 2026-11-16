namespace Clover.Player
{
    /// <summary>
    /// For a spell form that throws the player through the air (<see cref="PlayerController.Leap"/>): what happens
    /// when they come down. It is handed the same cast it was begun with.
    /// </summary>
    public interface ISpellLanding
    {
        void Landed(in SpellCast cast);
    }
}

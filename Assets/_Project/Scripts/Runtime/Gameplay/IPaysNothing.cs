namespace Clover.Gameplay
{
    /// <summary>
    /// Something a blow can land on that pays nothing back for it: no mana, no flask, no grey health won back, nothing for
    /// the upgrades that feed on blows. The Sun-Bearer's loose sun is one (fire_land.md 6.4, reworked on 2026-11-18): it
    /// is a ball to be struck at the god, not a post to farm.
    /// </summary>
    public interface IPaysNothing
    {
        bool PaysNothing { get; }
    }
}

namespace Clover.Player
{
    /// <summary>
    /// The shape a spell takes: a wave, a bolt, a leap, a rod. A form knows nothing of any one element: it is handed
    /// the caster, the element's colour and affliction, where the spell is aimed and its numbers, and does the rest
    /// through the player's weapon code (<see cref="PlayerCombat.StrikeCone(float,float,in SpellBlow)"/> and its
    /// kin), the ground (<see cref="Clover.World.Hazards"/>) and the element's look (<see cref="ElementLook"/>).
    /// The mana is already paid when it is called. A form keeps no state of its own between casts: one instance
    /// serves every cast.
    /// </summary>
    public interface ISpellForm
    {
        /// <summary>The name upgrades use for it in <see cref="Upgrade.Form"/>. Lower case, one word.</summary>
        string Id { get; }

        /// <summary>Casts it. Returns how many enemies it struck at once; zero when what it does comes later.</summary>
        int Cast(in SpellCast cast);
    }
}

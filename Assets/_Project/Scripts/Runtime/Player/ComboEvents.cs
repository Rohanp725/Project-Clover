using System;

namespace Clover.Player
{
    /// <summary>What happened in one tick of a <see cref="WeaponCombo"/>.</summary>
    [Flags]
    public enum ComboEvents
    {
        None = 0,
        /// <summary>An attack began this tick.</summary>
        Started = 1,
        /// <summary>An attack's hit lands this tick.</summary>
        Hit = 2,
        /// <summary>A held charge became ready this tick: letting go now releases the weapon's skill.</summary>
        Charged = 4,
    }
}

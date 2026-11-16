namespace Clover.Gameplay
{
    /// <summary>What an element lays on the enemies it touches. Each element has one; the numbers of each are in <see cref="Afflictions"/>.</summary>
    public enum AfflictionKind
    {
        None,
        /// <summary>Fire: damage over time, up to five deep.</summary>
        Burn,
        /// <summary>Lightning: up to five; it is set off all at once and arcs.</summary>
        Static,
        /// <summary>Wind: pushed further, turns slower, staggers sooner.</summary>
        Unsteady,
        /// <summary>Ice: slowed, up to five; at five it is frozen, and a heavy blow shatters it.</summary>
        Chill,
        /// <summary>Water: takes more from spells; carries Static and Chill to those near.</summary>
        Soaked,
        /// <summary>Life: blows on it heal the one who strikes; it bursts into thorns when it dies.</summary>
        Seeded,
        /// <summary>Earth: slowed, and its balance does not recover.</summary>
        Tremor,
        /// <summary>Metal: armour broken; takes more from weapons.</summary>
        Sundered,
        /// <summary>Light: blows on it are critical and give more mana.</summary>
        Marked,
        /// <summary>Dark: deals less; killing it heals the one who killed it.</summary>
        Cursed,
        /// <summary>Poison: stacking damage that lands in bursts.</summary>
        Poison,
    }
}

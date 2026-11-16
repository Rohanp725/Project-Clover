namespace Clover.Player
{
    /// <summary>Where an upgrade's effect happens.</summary>
    public enum EffectAt
    {
        /// <summary>On the trigger's target: the enemy struck, the attacker, the one that died. Without one, on the enemy the player faces; without that, a little ahead of the player.</summary>
        Target,
        /// <summary>On the player.</summary>
        Self,
    }
}

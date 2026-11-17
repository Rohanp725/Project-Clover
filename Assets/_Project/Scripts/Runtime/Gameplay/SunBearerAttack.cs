namespace Clover.Gameplay
{
    /// <summary>What the Sun-Bearer can do. The first five are the scarab's, the next five the falcon's.</summary>
    public enum SunBearerAttack
    {
        None,
        /// <summary>The sun rolled down a red lane right across the court, the scarab behind it.</summary>
        Roll,
        /// <summary>Three rolls back and forth, each lane nearer the player's side.</summary>
        Furrows,
        /// <summary>The horn driven into a pale circle: the one blow of its body, and its plain opening.</summary>
        Ram,
        /// <summary>Sparks shaken from under the wing cases onto an arc of circles, then onto the gaps.</summary>
        Sparks,
        /// <summary>The change: the scarab throws the sun up and goes after it as the falcon.</summary>
        Rising,
        /// <summary>The falcon comes straight down on a red circle, sun first, and stands on it.</summary>
        Stoop,
        /// <summary>The falcon flies a red lane from rim to rim with the sun hanging under it.</summary>
        Pass,
        /// <summary>Burning feathers: the middle, a ring, the middle again with the ring turned.</summary>
        Feathers,
        /// <summary>The sun carried to noon: the whole court burns but for the obelisk's shadow.</summary>
        Zenith,
        /// <summary>The change: the falcon lands on the rim and folds into the scarab.</summary>
        Setting,
    }
}

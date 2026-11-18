namespace Clover.Gameplay
{
    /// <summary>
    /// What the Sun-Bearer can do. The scarab's are the Roll, the Furrows, the Ram, the Sparks, the Rising, the
    /// Ricochet, the Burrow and the Flares; the falcon's the Stoop, the Pass, the Feathers, the Zenith, the Setting, the
    /// Ember Gust, the Talon Grab and the Sunfall. The newer ones are added at the end so the numbers of the older stay.
    /// </summary>
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
        /// <summary>The falcon flies a red lane across the court with the sun hanging under it.</summary>
        Pass,
        /// <summary>Burning feathers: the middle, a ring, the middle again with the ring turned.</summary>
        Feathers,
        /// <summary>The sun carried to noon: the whole court burns but for the obelisk's shadow.</summary>
        Zenith,
        /// <summary>The change: the falcon lands on the rim and folds into the scarab.</summary>
        Setting,
        /// <summary>The sun shoved hard at the court's wall: it bounces twice down a path drawn first as lanes, which burn a little while.</summary>
        Ricochet,
        /// <summary>The scarab digs in with the sun; a ridge of cracking paving chases the player, locks, and it bursts up under a red circle, half out of the ground with its head open.</summary>
        Burrow,
        /// <summary>The sun planted: arcs of flame loop up off it and slam down along lines that radiate from it, one after another.</summary>
        Flares,
        /// <summary>A great wingbeat: a wall of burning wind across a pale band that shoves whoever it catches, unless their guard holds.</summary>
        Gust,
        /// <summary>The shadow's red circle: caught, the player is carried up and dropped; missed, the falcon lands and is open.</summary>
        Grab,
        /// <summary>Burning pieces torn from the sun and hurled down on red circles, each leaving a little fire.</summary>
        Sunfall,
    }
}

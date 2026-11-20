namespace Clover.Player
{
    /// <summary>
    /// A duo surge's own price and time (the ledger's duo-surge-slot): worn in the Hand's surge slot in place of an
    /// element's surge while both its elements are carried, lit with the surge button for its own mana, and burning its own
    /// seconds. Its mark's effects that ask for the surge (<see cref="UpgradeEffect.WhileSurge"/>) answer only while it
    /// burns, since the mark is in force only while it is the surge worn (<see cref="MarkRules.InForce(Upgrade, Build.Loadout)"/>).
    /// </summary>
    public class DuoSurge
    {
        /// <summary>The mana it costs to light.</summary>
        public float Mana = 40f;
        /// <summary>How long it burns.</summary>
        public float Seconds = 10f;
        /// <summary>What the weapon's damage is multiplied by while it burns; its own marks' effects do the rest.</summary>
        public float Damage = 1f;

        /// <summary>True for a duo branch's mark that is a duo surge.</summary>
        public static bool Is(Upgrade mark) => mark != null && mark.IsMix && mark.Surge != null;

        /// <summary>
        /// A kit that lights this surge and keeps its clock: its name, price and time, and nothing else of an element's (no
        /// affliction on the blows, no stats). Made when the surge is worn, never while playing.
        /// </summary>
        public static ElementKit KitFor(Upgrade mark)
        {
            var surge = mark != null ? mark.Surge : null;
            if (surge == null) return null;
            return new ElementKit
            {
                Name = mark.Element,
                SurgeName = mark.Name,
                SurgeCost = surge.Mana,
                SurgeSeconds = surge.Seconds,
                SurgeCooldown = 0f,
                SurgeDamage = surge.Damage,
                SurgeBurnSeconds = 0f,
                SurgeSpeed = 1f,
                SurgeStagger = 0f,
                SurgeStacks = 0,
            };
        }
    }
}

using System;

namespace Clover.Player
{
    /// <summary>
    /// Something a blow throws: it leaves from in front of the player at the hit, flies straight, strikes what it
    /// passes, and may stay where it stops as a zone that bites. <see cref="WeaponShots"/> flies and draws it.
    /// </summary>
    [Serializable]
    public class WeaponShot
    {
        public float Speed = 12f;
        public float Range = 6f;
        public float Radius = 1f;
        public float Damage = 10f;
        public float Stagger = 10f;
        public int Count = 1;
        public float SpreadDegrees;
        /// <summary>True: it goes on through what it strikes. False: it stops at the first enemy.</summary>
        public bool Pierce;
        public bool Heavy;
        /// <summary>Seconds it stays at the end of its flight, biting every <see cref="HangEvery"/> for <see cref="HangBite"/> damage and stagger.</summary>
        public float HangSeconds;
        public float HangBite;
        public float HangEvery = 0.4f;
        /// <summary>It stays in what it hits and says so. A seam for the bow and the spear: not yet read.</summary>
        public bool Sticks;
        /// <summary>It flies back to the hand, striking again. A seam for the hammer: not yet read.</summary>
        public bool Returns;
        public float ReturnSpeed = 14f;
        public float ReturnScale = 0.6f;
        /// <summary>Which thing is drawn riding it: a name the shots' pool knows ("crescent"). Empty: the crescent.</summary>
        public string Prop = "";
    }
}

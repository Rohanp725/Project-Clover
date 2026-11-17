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
        /// <summary>
        /// True for a shot thrown as its step begins to carry the thrower away (the knives' fan): it leaves from where
        /// the step began, not from where the spring back has taken the body by the time of the blow.
        /// </summary>
        public bool FromStepStart;
        public bool Pierce;
        public bool Heavy;
        /// <summary>Seconds it stays at the end of its flight, biting every <see cref="HangEvery"/> for <see cref="HangBite"/> damage and stagger.</summary>
        public float HangSeconds;
        public float HangBite;
        public float HangEvery = 0.4f;
        /// <summary>It stays where it stops, in the first enemy or in the ground, for <see cref="StickSeconds"/> or until it is fetched or called back.</summary>
        public bool Sticks;
        public float StickSeconds = 3f;
        /// <summary>It flies back to the hand from the end of its flight, striking again at <see cref="ReturnScale"/> of its blow.</summary>
        public bool Returns;
        public float ReturnSpeed = 14f;
        public float ReturnScale = 0.6f;
        /// <summary>
        /// Called back from where it stands, it comes home as a ray in <see cref="RaySeconds"/>, striking everything
        /// within <see cref="RayRadius"/> of the line once, at <see cref="ReturnScale"/> of its blow (the Sun-Spear).
        /// </summary>
        public bool RaysBack;
        public float RayRadius = 1.1f;
        public float RaySeconds = 0.18f;
        /// <summary>Seconds after the blow before it leaves: the hammer is let go at the end of its turns, not at their first blow.</summary>
        public float ReleaseAfter;
        /// <summary>Which thing is drawn riding it: the carry name of a weapon whose own model flies ("spear", "hammer"). Empty: the crescent of light.</summary>
        public string Prop = "";
        /// <summary>
        /// An arrow: where it stops it is handed to <see cref="PlantedArrows"/>, to stand in each enemy it struck, or in
        /// the ground at the end of a flight that stopped in no one.
        /// </summary>
        public bool Plants;
        /// <summary>Mana the thrower gains for each enemy it strikes: a shot that is its step's whole blow pays the step's gain.</summary>
        public float ManaGain;
        /// <summary>For a model: metres from its grip to the part that leads (the spear's point), and degrees a second it tumbles end over end.</summary>
        public float PropLength;
        public float Tumble;
    }
}

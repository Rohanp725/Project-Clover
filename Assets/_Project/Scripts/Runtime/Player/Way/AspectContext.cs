namespace Clover.Player.Way
{
    /// <summary>What happened to the player in one step, as an aspect's <see cref="WeaponAspect.Tick"/> reads it.</summary>
    public struct AspectContext
    {
        public float DeltaTime;
        /// <summary>Blows of the weapon that landed this step.</summary>
        public int Landed;
        /// <summary>True when a guard was perfect this step.</summary>
        public bool PerfectGuard;
        /// <summary>True when the guard turned a red attack this step.</summary>
        public bool RedTurned;
        /// <summary>True when the player killed this step.</summary>
        public bool Killed;
        /// <summary>True while a fight is on (the fight clock is running).</summary>
        public bool InFight;
        /// <summary>The marks in force, for the rules a mark changes.</summary>
        public IMarkQuery Marks;
    }
}

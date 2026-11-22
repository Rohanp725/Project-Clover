namespace Clover.Player.Way
{
    /// <summary>What an aspect adds to one blow of its weapon as the blow goes out (<see cref="WeaponAspect.Blow"/>).</summary>
    public struct AspectBlow
    {
        /// <summary>Flat stagger added to every enemy this blow reaches (the seventh drum's draw: 40).</summary>
        public float Stagger;
        /// <summary>The blow's cut stays hanging where it was cut, cutting once more whoever walks into it (A Hundred Days).</summary>
        public bool HangingCut;
    }
}

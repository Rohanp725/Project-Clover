namespace Clover.Gameplay
{
    /// <summary>What the storm dragon can do to the floor the player stands on.</summary>
    public enum DragonAttack
    {
        None,
        /// <summary>Mouth. A beam from the open mouth that crosses the whole floor from one side to the other. Red. The head lies spent afterwards.</summary>
        Beam,
        /// <summary>Mouth. Forking lightning breathed along the ground in three lanes. Red.</summary>
        Breath,
        /// <summary>Mouth. The head lunges onto the floor at whoever is within its reach, and stays down a moment. Can be guarded.</summary>
        Bite,
        /// <summary>Magic. Bolts on marked circles, in a pattern of a middle and rings. Red.</summary>
        Bolts,
        /// <summary>Magic. A ring of sigils round the player that closes in three steps. Red.</summary>
        Sigils,
        /// <summary>Magic. Orbs that drift after the player and burst. Red.</summary>
        Orbs,
        /// <summary>Magic. Two opposite quarters of the floor crackle for a while. Red.</summary>
        Storm,
        /// <summary>Tail. The slither: it winds across the whole floor along a marked lane, rim to rim. Red.</summary>
        TailSweep,
        /// <summary>Tail. It comes down along a lane, and lies there a moment. Can be guarded.</summary>
        TailSlam,
        /// <summary>Tail. It lays itself in a ring round the player and draws tight. Red. Cut enough while it lies there, it springs open.</summary>
        TailCoil,
        /// <summary>Tail. It lies in over one rim and sweeps round the whole floor and back, the tip coming late. Red.</summary>
        TailScythe,
        /// <summary>Tail. Three flops one after another marching across the floor, lightning running out from each. Can be guarded; the lightning is red.</summary>
        TailFlops,
        /// <summary>Tail. It stands over the player like a scorpion's and stabs four times; the last sticks in the stone. Can be guarded.</summary>
        TailSting,
    }
}

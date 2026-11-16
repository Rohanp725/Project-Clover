namespace Clover.Player
{
    /// <summary>What sort of mark an upgrade is on the chart. The kind sets its usual cost and the shape of its frame.</summary>
    public enum UpgradeKind
    {
        /// <summary>The foot of a tree. Costs 2. Turns the element's affliction on for blows of the weapon.</summary>
        Root,
        /// <summary>A plain number. Costs 1. Small and round.</summary>
        Minor,
        /// <summary>A new rule: something happens on a trigger. Costs 3. Square.</summary>
        Major,
        /// <summary>A move the player performs with the spell button held. Costs 4. A diamond.</summary>
        Spell,
        /// <summary>The end of a bough. Costs 6. Large.</summary>
        Keystone,
        /// <summary>A mark on the branch two neighbouring elements share. Costs 3 to 5, as written on the mark.</summary>
        Mix,
        /// <summary>What a god leaves. Never sold. The largest, with a crown.</summary>
        Signature,
    }
}

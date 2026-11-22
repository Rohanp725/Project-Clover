namespace Clover.Player
{
    /// <summary>
    /// When a change to a number holds. The first group is a state of the player and is added into the stat's one sum
    /// while it lasts; the blow group is asked of each blow of the weapon as it lands.
    /// </summary>
    public enum StatWhen
    {
        Always,
        /// <summary>While the guard is raised.</summary>
        Guarding,
        /// <summary>While the surge lasts.</summary>
        Surge,
        /// <summary>While health is below a third.</summary>
        LowHealth,
        /// <summary>While health is below a half.</summary>
        HalfHealth,
        /// <summary>While no blow has landed on the player for eight seconds.</summary>
        Unhurt,
        /// <summary>While no dodge has begun for three seconds (sooner with the PlantedSooner stat).</summary>
        Planted,
        /// <summary>While grey health is held.</summary>
        GreyHeld,
        /// <summary>While health is at a half or more.</summary>
        HighHealth,
        /// <summary>While mana is full.</summary>
        ManaFull,
        /// <summary>While something the player has grown stands within four metres.</summary>
        NearGrown,
        /// <summary>While nothing the player has grown stands within seven metres.</summary>
        FarFromGrown,
        /// <summary>On heavy blows, finishers included. Weapon damage, stagger and critical chance only.</summary>
        HeavyBlow,
        /// <summary>On light blows. Weapon damage, stagger and critical chance only.</summary>
        LightBlow,
        /// <summary>On critical blows. Weapon damage and stagger only.</summary>
        CriticalBlow,
        /// <summary>On the blow that ends a chain: the last light blow, or the heavy blow thrown from a chain. Weapon damage and stagger only.</summary>
        ChainEndBlow,
        /// <summary>On a blow that lands on an enemy in the air (or reeling). Weapon damage, stagger and mana from blows.</summary>
        AirborneBlow,
        /// <summary>While the player is Risen: stood up again from a death, and draining.</summary>
        Risen,
        /// <summary>While a heavy blow of the weapon is being wound up or swung.</summary>
        HeavySwing,
        /// <summary>While the player stands in fire they have left on the ground.</summary>
        InOwnFire,
        /// <summary>While the player's tide is high (High Water, Low Water: the tide clock turns every few seconds).</summary>
        HighWater,
        /// <summary>While the player's tide is low.</summary>
        LowWater,
        /// <summary>On a heavy blow (finishers included) that lands on an enemy in the air (or reeling). Weapon damage and stagger only.</summary>
        AirborneHeavyBlow,
        /// <summary>While the player has stood still, not walking, dodging or stepping, for a second (the ledger's stood-still).</summary>
        StoodStill,
        /// <summary>While the player stands in water they have left on the ground: their own ground that Soaks (the ledger's in-own-water).</summary>
        InOwnWater,
        /// <summary>While the player stands on their own sheet of ice (Hoarfrost's Black Ice; World.OwnGrounds.Sheet).</summary>
        OnOwnIce,
        /// <summary>While the player stands on wet sand of their own, or dodges from it (World.OwnGrounds.WetSand).</summary>
        OnWetSand,
        /// <summary>While the player stands in their own Low Mist (World.OwnGrounds.LowMist).</summary>
        InOwnMist,
        /// <summary>While the player stands in their own lamp's glow (Standing Light; World.OwnGrounds.Glow).</summary>
        InOwnGlow,
        /// <summary>While the player stands in their own ring of green blood (Blood in the Furrow; World.OwnGrounds.Blood).</summary>
        InOwnBlood,
        /// <summary>While the player stands in a cloud: Miasma's own, or any poison ground of theirs (Tasted Every Herb).</summary>
        InCloud,
        /// <summary>While the player stands on the line between Need-Fire's two fires (World.OwnGrounds.NeedFireLine).</summary>
        BetweenFires,
        /// <summary>While the player stands on Earthlight's glowing seams (World.OwnGrounds.Seams).</summary>
        OnSeams,
    }
}

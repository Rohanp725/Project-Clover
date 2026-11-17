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
    }
}

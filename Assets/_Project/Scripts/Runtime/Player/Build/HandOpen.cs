namespace Clover.Player.Build
{
    /// <summary>How much of the Hand is open now (<see cref="SlotSchedule"/>): the spell slots by button order, the surge, the second attunement.</summary>
    public readonly struct HandOpen
    {
        /// <summary>How many spell slots are open, counted West, North, East, South.</summary>
        public readonly int Slots;
        public readonly bool Surge;
        public readonly bool SecondAttunement;

        public HandOpen(int slots, bool surge, bool secondAttunement)
        {
            Slots = slots;
            Surge = surge;
            SecondAttunement = secondAttunement;
        }

        /// <summary>Everything open, as the playtest has it.</summary>
        public static HandOpen All => new HandOpen(Loadout.SlotCount, true, true);

        public bool SlotOpen(int slot) => slot >= 0 && slot < Slots;
    }
}

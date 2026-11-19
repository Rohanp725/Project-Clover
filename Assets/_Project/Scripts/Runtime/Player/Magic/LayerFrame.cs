namespace Clover.Player.Magic
{
    /// <summary>
    /// One frame of the buttons as <see cref="SpellLayerRules"/> needs them, already read from the Input System by the
    /// caller: which layer is held on which device, the slot presses by device (bit 0 is West), and the attack presses.
    /// </summary>
    public struct LayerFrame
    {
        public float DeltaTime;
        /// <summary>L2 or LT is down.</summary>
        public bool PadLayerHeld;
        /// <summary>Left Ctrl is down.</summary>
        public bool KeysLayerHeld;
        /// <summary>Slot buttons pressed this frame on the pad (face buttons) and on the keyboard (1 to 4), one bit a slot.</summary>
        public int PadSlots;
        public int KeySlots;
        public bool Light;
        public bool Heavy;
    }
}

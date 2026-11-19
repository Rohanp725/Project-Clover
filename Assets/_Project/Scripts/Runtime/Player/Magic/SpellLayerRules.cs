using System;

namespace Clover.Player.Magic
{
    /// <summary>
    /// The L2 layer (STEEL_AND_ELEMENT.md 3.5; IMPLEMENTATION.md 3.5). On a pad, holding L2 turns the four face buttons
    /// into the four spell slots, and the Player map's own use of those buttons (flask, weapon skill, dodge, use) is
    /// swallowed while it is held; a short tap of L2 with nothing pressed on the layer lights the surge as it is let go.
    /// On the keyboard 1 to 4 cast at once with no layer. Held with R1 or R2 (or Left Ctrl with a mouse button) the
    /// layer gives the infused strikes, once <see cref="StrikesOn"/> is set (task 1.6); until then the attack buttons
    /// pass through untouched. No Input System types: the caller reads the buttons into a <see cref="LayerFrame"/>.
    /// </summary>
    [Serializable]
    public class SpellLayerRules
    {
        /// <summary>Longest hold of L2, in real seconds, that still counts as a tap and lights the surge.</summary>
        public float SurgeTapSeconds = 0.25f;
        /// <summary>The infused strikes are built: with the layer held, light and heavy are strikes and not attacks.</summary>
        public bool StrikesOn;

        /// <summary>The slot cast this frame (0 West, 1 North, 2 East, 3 South), or -1.</summary>
        public int SlotCast { get; private set; } = -1;
        /// <summary>The surge is lit this frame (on the release of a tap).</summary>
        public bool SurgeLit { get; private set; }
        public bool StrikeFirst { get; private set; }
        public bool StrikeSecond { get; private set; }
        /// <summary>Light or heavy belongs to the layer this frame and must not also attack.</summary>
        public bool SwallowLight { get; private set; }
        public bool SwallowHeavy { get; private set; }
        public bool PadLayerHeld { get; private set; }
        public bool LayerHeld { get; private set; }
        /// <summary>Seconds L2 has been held, while it is.</summary>
        public float HeldFor { get; private set; }

        private bool spoiled;

        /// <summary>Settles one frame. Read the answers from the properties until the next call.</summary>
        public void Step(in LayerFrame frame)
        {
            SlotCast = -1;
            SurgeLit = false;
            StrikeFirst = StrikeSecond = false;
            SwallowLight = SwallowHeavy = false;

            bool wasHeld = PadLayerHeld;
            PadLayerHeld = frame.PadLayerHeld;
            LayerHeld = frame.PadLayerHeld || frame.KeysLayerHeld;

            // A pad's face button is a spell only while L2 is held; a number key always is.
            int slots = frame.KeySlots | (frame.PadLayerHeld ? frame.PadSlots : 0);
            for (int i = 0; i < 4 && SlotCast < 0; i++)
                if ((slots & (1 << i)) != 0) SlotCast = i;

            if (StrikesOn && LayerHeld)
            {
                StrikeFirst = SwallowLight = frame.Light;
                StrikeSecond = SwallowHeavy = frame.Heavy;
            }

            if (frame.PadLayerHeld)
            {
                if (!wasHeld)
                {
                    HeldFor = 0f;
                    spoiled = false;
                }
                else HeldFor += Math.Max(0f, frame.DeltaTime);
                if ((frame.PadSlots & 0xF) != 0 || StrikeFirst || StrikeSecond) spoiled = true;
            }
            else if (wasHeld)
            {
                HeldFor += Math.Max(0f, frame.DeltaTime);
                SurgeLit = !spoiled && HeldFor <= SurgeTapSeconds;
                HeldFor = 0f;
            }
        }

        /// <summary>True when a press of a Player-map face button (flask, weapon skill, dodge, use) belongs to the layer and must be ignored.</summary>
        public bool Swallows(bool pressedOnPad) => pressedOnPad && PadLayerHeld;

        public void Reset()
        {
            SlotCast = -1;
            SurgeLit = StrikeFirst = StrikeSecond = SwallowLight = SwallowHeavy = false;
            PadLayerHeld = LayerHeld = false;
            HeldFor = 0f;
            spoiled = false;
        }
    }
}

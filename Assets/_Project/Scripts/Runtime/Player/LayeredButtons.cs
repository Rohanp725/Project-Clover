using Clover.Player.Magic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Clover.Player
{
    /// <summary>
    /// The one place the Player and Spells maps are read together, so the L2 layer can take a face button from the flask,
    /// the weapon skill, the dodge and the use (IMPLEMENTATION.md 3.5). Whoever reads a button asks
    /// <see cref="Pressed"/> instead of the action; the frame's presses are settled once, on the first question of the
    /// frame, whatever order the scripts run in. The rules are <see cref="SpellLayerRules"/>; this only reads the devices.
    /// </summary>
    public class LayeredButtons
    {
        public readonly SpellLayerRules Rules = new SpellLayerRules();

        private readonly InputAction layer, dodge, light, heavy, guard, flask, skill, surge, interact;
        private readonly InputAction[] slots = new InputAction[SpellSlots.Count];
        private int settledFrame = -1;
        private float lastTime = -1f;
        private int pressed;

        public static readonly string[] SlotActions = { "SpellWest", "SpellNorth", "SpellEast", "SpellSouth" };

        public LayeredButtons(InputActionMap player, InputActionMap spells)
        {
            dodge = player?.FindAction("Dodge");
            light = player?.FindAction("LightAttack");
            heavy = player?.FindAction("HeavyAttack");
            guard = player?.FindAction("Guard");
            flask = player?.FindAction("Flask");
            skill = player?.FindAction("WeaponSkill");
            surge = player?.FindAction("Surge");
            interact = player?.FindAction("Interact");
            layer = spells?.FindAction("SpellLayer");
            for (int i = 0; i < slots.Length; i++) slots[i] = spells?.FindAction(SlotActions[i]);
            // The infused strikes are built (task 1.6): with the layer held, R1 and R2 are strikes, not attacks.
            Rules.StrikesOn = true;
        }

        /// <summary>L2 with R1 (Left Ctrl with the left mouse button) this frame: the infused strike of the first element.</summary>
        public bool StrikeFirst
        {
            get
            {
                Settle();
                return Rules.StrikeFirst;
            }
        }

        /// <summary>L2 with R2 (Left Ctrl with the right mouse button) this frame: the second element's strike, or the deep strike all in.</summary>
        public bool StrikeSecond
        {
            get
            {
                Settle();
                return Rules.StrikeSecond;
            }
        }

        /// <summary>This frame's press of a Player-map button, unless the layer took it.</summary>
        public bool Pressed(PlayerButton button)
        {
            Settle();
            return (pressed & (1 << (int)button)) != 0;
        }

        /// <summary>The slot cast this frame (0 West to 3 South), or -1.</summary>
        public int SlotCast
        {
            get
            {
                Settle();
                return Rules.SlotCast;
            }
        }

        /// <summary>True while L2 (or Left Ctrl) is down: the HUD shows the spells large.</summary>
        public bool LayerHeld
        {
            get
            {
                Settle();
                return Rules.LayerHeld;
            }
        }

        /// <summary>True while a slot's button is down: for spells that last as long as they are held.</summary>
        public bool SlotHeld(int slot)
        {
            if (slot < 0 || slot >= slots.Length || slots[slot] == null || !slots[slot].IsPressed()) return false;
            // A face button held without L2 is the flask or the dodge again, not the spell.
            return !FromPad(slots[slot]) || Rules.PadLayerHeld;
        }

        /// <summary>The weapon skill's button is down (a counter's stance is held on it).</summary>
        public bool SkillHeld => skill != null && skill.IsPressed() && !Rules.Swallows(FromPad(skill));

        public bool HeavyHeld => heavy != null && heavy.IsPressed();

        private void Settle()
        {
            int frame = Time.frameCount;
            if (frame == settledFrame) return;
            settledFrame = frame;
            // Real seconds, so a hit-stop or a slowed moment does not stretch what counts as a tap.
            float now = Time.unscaledTime;
            float delta = lastTime >= 0f ? Mathf.Max(0f, now - lastTime) : 0f;
            lastTime = now;

            var input = new LayerFrame
            {
                DeltaTime = delta,
                PadLayerHeld = HeldOn<Gamepad>(layer),
                KeysLayerHeld = HeldOn<Keyboard>(layer),
                Light = Down(light),
                Heavy = Down(heavy),
            };
            for (int i = 0; i < slots.Length; i++)
            {
                if (!Down(slots[i])) continue;
                if (FromPad(slots[i])) input.PadSlots |= 1 << i;
                else input.KeySlots |= 1 << i;
            }
            Rules.Step(input);

            pressed = 0;
            Mark(PlayerButton.Dodge, dodge, true);
            Mark(PlayerButton.Flask, flask, true);
            Mark(PlayerButton.WeaponSkill, skill, true);
            Mark(PlayerButton.Interact, interact, true);
            Mark(PlayerButton.Guard, guard, false);
            if (input.Light && !Rules.SwallowLight) pressed |= 1 << (int)PlayerButton.LightAttack;
            if (input.Heavy && !Rules.SwallowHeavy) pressed |= 1 << (int)PlayerButton.HeavyAttack;
            // The surge: F on the keyboard, or a tap of L2 let go.
            if (Down(surge) || Rules.SurgeLit) pressed |= 1 << (int)PlayerButton.Surge;
        }

        private void Mark(PlayerButton button, InputAction action, bool layered)
        {
            if (!Down(action)) return;
            if (layered && Rules.Swallows(FromPad(action))) return;
            pressed |= 1 << (int)button;
        }

        /// <summary>Forgets a held layer, as when a menu takes the buttons: no tap is lit on the release that follows.</summary>
        public void Drop()
        {
            Rules.Reset();
            settledFrame = Time.frameCount;
            pressed = 0;
        }

        private static bool Down(InputAction action) => action != null && action.enabled && action.WasPressedThisFrame();

        private static bool FromPad(InputAction action)
        {
            var control = action != null ? action.activeControl : null;
            return control != null && control.device is Gamepad;
        }

        // Whether the action is held on a device of this kind, read from its bound controls (no allocation).
        private static bool HeldOn<T>(InputAction action) where T : InputDevice
        {
            if (action == null || !action.enabled) return false;
            var controls = action.controls;
            for (int i = 0; i < controls.Count; i++)
                if (controls[i].device is T && controls[i] is ButtonControl button && button.isPressed) return true;
            return false;
        }
    }
}

using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;

namespace Clover.Player
{
    /// <summary>
    /// Remembers which device the player last used, so prompts name that device's buttons and attacks aim with the mouse
    /// only while the mouse is in use. It is told by what the actions hear (never by raw device traffic, which a resting
    /// pad sends all the time) and by the mouse moving. <see cref="Version"/> goes up whenever anything a prompt shows may
    /// have changed, the bindings included, so a label can redraw only then.
    /// </summary>
    public static class ControlDevices
    {
        /// <summary>Pixels the mouse must move in one frame before it counts as being used.</summary>
        public const float MousePixels = 6f;

        private static ControlDevice current;
        private static ControlDevice pad;
        private static bool known;

        /// <summary>Whose names the prompts use now.</summary>
        public static ControlDevice Current
        {
            get
            {
                Guess();
                return current;
            }
        }

        /// <summary>The kind of pad last used, or the one plugged in: the names the controls page gives the pad's column.</summary>
        public static ControlDevice Pad
        {
            get
            {
                Guess();
                return pad;
            }
        }

        /// <summary>True when the mouse was used more lately than any pad: attacks and spells then turn toward the cursor.</summary>
        public static bool MouseAim { get; private set; }

        /// <summary>Goes up by one each time the device or a binding changes.</summary>
        public static int Version { get; private set; }

        // Domain reload is off: the last play's device must not carry into the next.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            current = ControlDevice.Keys;
            pad = ControlDevice.Xbox;
            known = false;
            MouseAim = false;
            Version = 0;
        }

        /// <summary>The names a device's buttons go by. Every pad that is not a DualShock or DualSense uses Xbox's letters.</summary>
        public static ControlDevice KindOf(InputDevice device)
        {
            if (device is DualShockGamepad) return ControlDevice.PlayStation;
            if (device is Gamepad) return ControlDevice.Xbox;
            return ControlDevice.Keys;
        }

        /// <summary>An action heard this device.</summary>
        public static void Notice(InputDevice device)
        {
            if (device == null) return;
            known = true;
            var kind = KindOf(device);
            if (kind != ControlDevice.Keys)
            {
                MouseAim = false;
                if (pad != kind)
                {
                    pad = kind;
                    Version++;
                }
            }
            else if (device is Mouse) MouseAim = true;
            if (current == kind) return;
            current = kind;
            Version++;
        }

        /// <summary>Says that something a prompt names (a binding) has changed.</summary>
        public static void BindingsChanged() => Version++;

        /// <summary>Before anything is pressed: a pad that is plugged in is the likely one; with none, the keys.</summary>
        private static void Guess()
        {
            if (known) return;
            known = true;
            current = ControlDevice.Keys;
            pad = ControlDevice.Xbox;
            var pads = Gamepad.all;
            for (int i = 0; i < pads.Count; i++)
            {
                pad = KindOf(pads[i]);
                current = pad;
                if (pad == ControlDevice.PlayStation) break;
            }
        }
    }
}

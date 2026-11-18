using System;
using System.Text;
using UnityEngine.InputSystem;

namespace Clover.Player
{
    /// <summary>
    /// The words for a button: what a binding is called on a PlayStation pad, an Xbox pad or the keyboard and mouse, and
    /// what an action is bound to on the device the player last used. Prompts are written as templates with action
    /// names in braces ("{Pause}: pause and controls") and filled in by <see cref="Expand"/>. Called when a prompt is
    /// drawn, never every frame.
    /// </summary>
    public static class ControlNames
    {
        /// <summary>A pad control by its path, in the names of <paramref name="style"/>'s own buttons.</summary>
        public static string Pad(string path, ControlDevice style)
        {
            if (string.IsNullOrEmpty(path)) return "";
            bool ps = style == ControlDevice.PlayStation;
            string control = path.Substring(path.IndexOf('/') + 1);
            switch (control)
            {
                case "buttonSouth": return ps ? "Cross" : "A";
                case "buttonEast": return ps ? "Circle" : "B";
                case "buttonWest": return ps ? "Square" : "X";
                case "buttonNorth": return ps ? "Triangle" : "Y";
                case "leftShoulder": return ps ? "L1" : "LB";
                case "rightShoulder": return ps ? "R1" : "RB";
                case "leftTrigger": return ps ? "L2" : "LT";
                case "rightTrigger": return ps ? "R2" : "RT";
                case "start": return ps ? "Options" : "Menu";
                case "select": return ps ? "Share" : "View";
                case "leftStickPress": return ps ? "L3" : "LS";
                case "rightStickPress": return ps ? "R3" : "RS";
                case "leftStick": return "Left stick";
                case "rightStick": return "Right stick";
                case "dpad": return "D-pad";
                case "dpad/up": return "D-pad up";
                case "dpad/down": return "D-pad down";
                case "dpad/left": return "D-pad left";
                case "dpad/right": return "D-pad right";
                case "touchpadButton": return "Touchpad";
            }
            return InputControlPath.ToHumanReadableString(path, InputControlPath.HumanReadableStringOptions.OmitDevice);
        }

        /// <summary>A keyboard key or mouse button by its path, as printed on this keyboard where one is plugged in.</summary>
        public static string Keys(string path)
        {
            if (string.IsNullOrEmpty(path)) return "";
            if (path.StartsWith("<Mouse>/", StringComparison.OrdinalIgnoreCase))
            {
                switch (path.Substring(8))
                {
                    case "leftButton": return "Left mouse";
                    case "rightButton": return "Right mouse";
                    case "middleButton": return "Middle mouse";
                    case "forwardButton": return "Mouse forward";
                    case "backButton": return "Mouse back";
                }
            }
            // A keyboard calls both shift keys "Shift"; the side matters when only one of them is bound.
            switch (path.Substring(path.IndexOf('/') + 1))
            {
                case "leftShift": return "Left Shift";
                case "rightShift": return "Right Shift";
                case "leftCtrl": return "Left Ctrl";
                case "rightCtrl": return "Right Ctrl";
                case "leftAlt": return "Left Alt";
                case "rightAlt": return "Right Alt";
            }
            string name = null;
            var keyboard = Keyboard.current;
            if (keyboard != null && path.StartsWith("<Keyboard>/", StringComparison.OrdinalIgnoreCase))
            {
                var control = InputControlPath.TryFindControl(keyboard, path);
                if (control != null) name = control.displayName;
            }
            if (string.IsNullOrEmpty(name)) name = InputControlPath.ToHumanReadableString(path, InputControlPath.HumanReadableStringOptions.OmitDevice);
            if (name == "Escape") return "Esc";
            return name.Length == 1 ? name.ToUpperInvariant() : name;
        }

        /// <summary>A binding in the names of the given device.</summary>
        public static string OfPath(string path, ControlDevice style) => style == ControlDevice.Keys ? Keys(path) : Pad(path, style);

        /// <summary>What an action is bound to, first binding only, on the device whose names are in use now.</summary>
        public static string Of(string actionName) => Of(actionName, ControlDevices.Current);

        /// <summary>The same in a given device's names, from the given controls (the player's own when none are given).</summary>
        public static string Of(string actionName, ControlDevice style, InputActionAsset controls = null)
        {
            if (controls == null) controls = ControlBindings.Active;
            var action = controls != null ? controls.FindAction(actionName) : null;
            return Of(action, style);
        }

        public static string Of(InputAction action, ControlDevice style)
        {
            if (action == null) return "";
            bool pad = style != ControlDevice.Keys;
            string group = ControlBindings.GroupOf(pad);
            if (!pad && action.name == "Move")
            {
                // The four keys of the walk, in the order they are usually said.
                return Part(action, group, "up") + " " + Part(action, group, "left") + " " + Part(action, group, "down") + " " + Part(action, group, "right");
            }
            int index = ControlBindings.IndexOf(action, group);
            return index < 0 ? "" : OfPath(action.bindings[index].effectivePath, style);
        }

        private static string Part(InputAction action, string group, string part)
        {
            int index = ControlBindings.IndexOf(action, group, part);
            return index < 0 ? "?" : Keys(action.bindings[index].effectivePath);
        }

        /// <summary>
        /// Fills in a prompt: each {ActionName} becomes what the action is bound to on the device in use. Older built
        /// scenes still carry the PlayStation-and-keyboard words "Start or Esc"; those are read as {Pause}.
        /// </summary>
        public static string Expand(string template) => Expand(template, ControlDevices.Current);

        public static string Expand(string template, ControlDevice style, InputActionAsset controls = null)
        {
            if (string.IsNullOrEmpty(template)) return template;
            if (template.IndexOf("Start or Esc", StringComparison.Ordinal) >= 0) template = template.Replace("Start or Esc", "{Pause}");
            if (template.IndexOf('{') < 0) return template;
            var built = new StringBuilder(template.Length + 16);
            int at = 0;
            while (at < template.Length)
            {
                int open = template.IndexOf('{', at);
                int close = open < 0 ? -1 : template.IndexOf('}', open + 1);
                if (open < 0 || close < 0)
                {
                    built.Append(template, at, template.Length - at);
                    break;
                }
                built.Append(template, at, open - at);
                string name = template.Substring(open + 1, close - open - 1);
                string named = Of(name, style, controls);
                built.Append(string.IsNullOrEmpty(named) ? name : named);
                at = close + 1;
            }
            return built.ToString();
        }
    }
}

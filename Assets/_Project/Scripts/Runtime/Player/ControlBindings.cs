using System;
using Clover.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Clover.Player
{
    /// <summary>
    /// The player's own bindings: which binding of an action each column of the controls page means, what a rebind may
    /// take, how a clash is settled, and how the changes are kept. Changes are kept as the Input System's binding-override
    /// JSON in PlayerPrefs under <see cref="PrefsKey"/>, and laid over the player's copy of the controls in its Awake,
    /// before anything reads a button. A sandboxed play (probes) keeps them under <see cref="SandboxKey"/> instead, so it
    /// never touches what the owner chose.
    /// </summary>
    public static class ControlBindings
    {
        public const string PrefsKey = "clover.controls.bindings";
        public const string SandboxKey = "clover.controls.bindings.sandbox";
        public const string PadGroup = "Gamepad";
        public const string KeysGroup = "Keyboard&Mouse";
        /// <summary>The d-pad moves and chooses in menus; it is a fixed second binding of Move, never offered for rebinding.</summary>
        public const string DpadPath = "<Gamepad>/dpad";
        /// <summary>Escape and Start (Options, Menu) always open and close the pause menu and cancel a rebind; nothing else may take them.</summary>
        public static readonly string[] Reserved = { "<Keyboard>/escape", "<Gamepad>/start" };

        /// <summary>The controls the player in this scene reads: the asset the controls page rebinds and the prompts name.</summary>
        public static InputActionAsset Active { get; private set; }

        public static string Key => Progress.Sandboxed ? SandboxKey : PrefsKey;

        // Domain reload is off: the last play's player and its controls are gone.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Active = null;

        /// <summary>Makes these the controls prompts name, with the saved bindings laid over them.</summary>
        public static void Use(InputActionAsset controls)
        {
            Active = controls;
            Load(controls);
            ControlDevices.BindingsChanged();
        }

        public static void Forget(InputActionAsset controls)
        {
            if (Active == controls) Active = null;
        }

        public static void Load(InputActionAsset controls)
        {
            if (controls == null) return;
            string json = PlayerPrefs.GetString(Key, "");
            if (string.IsNullOrEmpty(json)) return;
            try
            {
                controls.LoadBindingOverridesFromJson(json);
            }
            catch (Exception exception)
            {
                // Bindings from an older layout of the controls are dropped rather than leaving the player without buttons.
                Debug.LogWarning($"[Clover.Player] The saved bindings could not be read and were ignored: {exception.Message}");
                controls.RemoveAllBindingOverrides();
            }
        }

        public static void Save(InputActionAsset controls)
        {
            if (controls == null) return;
            string json = controls.SaveBindingOverridesAsJson();
            if (string.IsNullOrEmpty(json) || !HasOverrides(controls)) PlayerPrefs.DeleteKey(Key);
            else PlayerPrefs.SetString(Key, json);
            PlayerPrefs.Save();
            ControlDevices.BindingsChanged();
        }

        /// <summary>Every binding back to the game's own, and nothing kept.</summary>
        public static void ResetToDefaults(InputActionAsset controls)
        {
            if (controls == null) return;
            controls.RemoveAllBindingOverrides();
            PlayerPrefs.DeleteKey(Key);
            PlayerPrefs.Save();
            ControlDevices.BindingsChanged();
        }

        public static bool HasOverrides(InputActionAsset controls)
        {
            foreach (var map in controls.actionMaps)
                foreach (var binding in map.bindings)
                    if (!string.IsNullOrEmpty(binding.overridePath)) return true;
            return false;
        }

        public static bool IsReserved(string path)
        {
            for (int i = 0; i < Reserved.Length; i++)
                if (string.Equals(path, Reserved[i], StringComparison.OrdinalIgnoreCase)) return true;
            return false;
        }

        public static string GroupOf(bool pad) => pad ? PadGroup : KeysGroup;

        private static bool InGroup(InputBinding binding, string group) =>
            !string.IsNullOrEmpty(binding.groups) && Array.IndexOf(binding.groups.Split(InputBinding.Separator), group) >= 0;

        /// <summary>
        /// The binding a cell of the controls page means: for a part of a composite (Move's "up"), that part of the first
        /// composite in the group; otherwise the <paramref name="nth"/> plain binding in the group. The d-pad is skipped.
        /// -1 when there is none.
        /// </summary>
        public static int IndexOf(InputAction action, string group, string part = null, int nth = 0)
        {
            if (action == null) return -1;
            var bindings = action.bindings;
            int seen = 0;
            for (int i = 0; i < bindings.Count; i++)
            {
                var binding = bindings[i];
                if (binding.isComposite) continue;
                if (!InGroup(binding, group)) continue;
                if (string.Equals(binding.path, DpadPath, StringComparison.OrdinalIgnoreCase)) continue;
                if (!string.IsNullOrEmpty(part))
                {
                    if (binding.isPartOfComposite && string.Equals(binding.name, part, StringComparison.OrdinalIgnoreCase) && seen++ == nth) return i;
                    continue;
                }
                if (binding.isPartOfComposite) continue;
                if (seen++ == nth) return i;
            }
            return -1;
        }

        /// <summary>The other binding in the same group that now answers to the same control, if any.</summary>
        public static bool FindClash(InputActionMap map, InputAction action, int bindingIndex, out InputAction other, out int otherIndex)
        {
            other = null;
            otherIndex = -1;
            var binding = action.bindings[bindingIndex];
            string path = binding.effectivePath;
            string group = InGroup(binding, PadGroup) ? PadGroup : KeysGroup;
            foreach (var candidate in map.actions)
            {
                var bindings = candidate.bindings;
                for (int i = 0; i < bindings.Count; i++)
                {
                    if (candidate == action && i == bindingIndex) continue;
                    var each = bindings[i];
                    if (each.isComposite || !InGroup(each, group)) continue;
                    if (!string.Equals(each.effectivePath, path, StringComparison.OrdinalIgnoreCase)) continue;
                    other = candidate;
                    otherIndex = i;
                    return true;
                }
            }
            return false;
        }

        /// <summary>What became of a new binding once a clash was looked for.</summary>
        public enum Settled
        {
            /// <summary>Nothing else had the control.</summary>
            Clear,
            /// <summary>Another binding had it and was given this one's old control.</summary>
            Swapped,
            /// <summary>Another binding had it and this one's old control may not be handed on (Escape, Start): the change was undone.</summary>
            Refused,
        }

        /// <summary>
        /// Settles a binding just given <see cref="InputBinding.effectivePath"/>, which was <paramref name="oldPath"/>
        /// before: a clash in its group is swapped, unless what would be handed over is reserved, when the change is
        /// undone instead. No binding is ever left empty, so nothing can be unbound.
        /// </summary>
        public static Settled Settle(InputActionMap map, InputAction action, int bindingIndex, string oldPath, out InputAction other, out int otherIndex)
        {
            if (!FindClash(map, action, bindingIndex, out other, out otherIndex)) return Settled.Clear;
            if (IsReserved(oldPath))
            {
                Restore(action, bindingIndex, oldPath);
                return Settled.Refused;
            }
            Restore(other, otherIndex, oldPath);
            return Settled.Swapped;
        }

        // Gives a binding a path: its own default drops the override, anything else becomes one.
        private static void Restore(InputAction action, int bindingIndex, string path)
        {
            var binding = action.bindings[bindingIndex];
            if (string.Equals(binding.path, path, StringComparison.OrdinalIgnoreCase)) action.RemoveBindingOverride(bindingIndex);
            else action.ApplyBindingOverride(bindingIndex, path);
        }
    }
}

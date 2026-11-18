using System.Collections.Generic;
using Clover.EditorTools;
using Clover.Player;
using Clover.UI;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.DualShock;
using UnityEngine.InputSystem.XInput;

namespace Clover.Tests
{
    /// <summary>
    /// The controls as the owner asked for them on 2026-11-18: an Xbox pad reaches every action and every menu (the
    /// d-pad included) and is named in Xbox's words, the keys and mouse sit where the table says, and a rebind is
    /// settled and kept. Each test works on a private copy of the controls and touches no saved bindings it does not put back.
    /// </summary>
    public class ControlBindingsTests
    {
        private InputActionAsset controls;
        private readonly List<InputDevice> added = new List<InputDevice>();

        [SetUp]
        public void SetUp()
        {
            var shared = AssetDatabase.LoadAssetAtPath<InputActionAsset>(PlayerBuilder.ControlsPath);
            Assert.IsNotNull(shared, $"no input actions at {PlayerBuilder.ControlsPath}");
            controls = Object.Instantiate(shared);
        }

        [TearDown]
        public void TearDown()
        {
            foreach (var device in added)
                if (device != null && device.added) InputSystem.RemoveDevice(device);
            added.Clear();
            if (controls != null) Object.DestroyImmediate(controls);
        }

        private T Add<T>() where T : InputDevice
        {
            var device = InputSystem.AddDevice<T>();
            added.Add(device);
            return device;
        }

        private static readonly string[] Actions = { "Move", "Dodge", "LightAttack", "HeavyAttack", "Guard", "Flask", "Spell", "Surge", "Interact", "Pause" };

        // Every action has a pad binding that finds a control on this pad.
        private void EveryActionReaches(InputDevice pad)
        {
            foreach (string name in Actions)
            {
                var action = controls.FindAction(name);
                Assert.IsNotNull(action, $"no {name} action");
                bool reached = false;
                foreach (var binding in action.bindings)
                    if (!binding.isComposite && binding.groups.Contains(ControlBindings.PadGroup) && InputControlPath.TryFindControl(pad, binding.effectivePath) != null)
                        reached = true;
                Assert.IsTrue(reached, $"{name} has no binding that reaches a {pad.layout}");
            }
        }

        [Test]
        public void AnXboxPadReachesEveryActionAndTheDpadMoves()
        {
            var pad = Add<XInputControllerWindows>();
            EveryActionReaches(pad);
            // The menus and the chart choose with Move, so the d-pad must be on it: an Xbox player reaches for it first.
            bool dpad = false;
            foreach (var binding in controls.FindAction("Move").bindings)
                if (InputControlPath.TryFindControl(pad, binding.effectivePath) == pad.dpad) dpad = true;
            Assert.IsTrue(dpad, "Move has no d-pad binding, so the pause menu and the chart cannot be walked with the d-pad");
        }

        [Test]
        public void APlayStationPadStillReachesEveryAction()
        {
            EveryActionReaches(Add<DualShock4GamepadHID>());
        }

        [Test]
        public void TheDevicesAreKnownByTheirButtons()
        {
            Assert.AreEqual(ControlDevice.Xbox, ControlDevices.KindOf(Add<XInputControllerWindows>()));
            Assert.AreEqual(ControlDevice.PlayStation, ControlDevices.KindOf(Add<DualShock4GamepadHID>()));
            Assert.AreEqual(ControlDevice.Keys, ControlDevices.KindOf(Add<Keyboard>()));
            Assert.AreEqual(ControlDevice.Keys, ControlDevices.KindOf(Add<Mouse>()));
        }

        [TestCase("<Gamepad>/buttonSouth", "A", "Cross")]
        [TestCase("<Gamepad>/buttonEast", "B", "Circle")]
        [TestCase("<Gamepad>/buttonWest", "X", "Square")]
        [TestCase("<Gamepad>/buttonNorth", "Y", "Triangle")]
        [TestCase("<Gamepad>/leftShoulder", "LB", "L1")]
        [TestCase("<Gamepad>/rightShoulder", "RB", "R1")]
        [TestCase("<Gamepad>/leftTrigger", "LT", "L2")]
        [TestCase("<Gamepad>/rightTrigger", "RT", "R2")]
        [TestCase("<Gamepad>/start", "Menu", "Options")]
        public void PadButtonsAreNamedForThePad(string path, string xbox, string playStation)
        {
            Assert.AreEqual(xbox, ControlNames.Pad(path, ControlDevice.Xbox));
            Assert.AreEqual(playStation, ControlNames.Pad(path, ControlDevice.PlayStation));
        }

        [Test]
        public void PromptsNameTheButtonsOfTheDeviceInUse()
        {
            Assert.AreEqual("Menu: pause and controls", ControlNames.Expand(PromptLine.PauseTemplate, ControlDevice.Xbox, controls));
            Assert.AreEqual("Options: pause and controls", ControlNames.Expand(PromptLine.PauseTemplate, ControlDevice.PlayStation, controls));
            Assert.AreEqual("Esc: pause and controls", ControlNames.Expand(PromptLine.PauseTemplate, ControlDevice.Keys, controls));
            // Older scenes carry the fixed words; they are read as the pause button too.
            Assert.AreEqual("Menu pauses.", ControlNames.Expand("Start or Esc pauses.", ControlDevice.Xbox, controls));
            string xboxLine = ControlNames.Expand(PromptLine.FightTemplate, ControlDevice.Xbox, controls);
            foreach (string playStationWord in new[] { "Cross", "Circle", "Square", "Triangle", "R1", "R2", "L1", "L2", "Options" })
                StringAssert.DoesNotContain(playStationWord, xboxLine, "an Xbox prompt names a PlayStation button");
            StringAssert.Contains("RB light", xboxLine);
            StringAssert.Contains("Left mouse light", ControlNames.Expand(PromptLine.FightTemplate, ControlDevice.Keys, controls));
            Assert.AreEqual("W A S D", ControlNames.Of(controls.FindAction("Move"), ControlDevice.Keys));
        }

        [Test]
        public void TheKeysAndMouseDefaultsAreTheAgreedLayout()
        {
            void Keys(string action, string part, int nth, string path)
            {
                var found = controls.FindAction(action);
                int index = ControlBindings.IndexOf(found, ControlBindings.KeysGroup, part, nth);
                Assert.GreaterOrEqual(index, 0, $"{action} {part} {nth} has no keys binding");
                Assert.AreEqual(path, found.bindings[index].effectivePath, $"{action} {part} {nth}");
            }
            Keys("Move", "up", 0, "<Keyboard>/w");
            Keys("Move", "left", 0, "<Keyboard>/a");
            Keys("Move", "down", 0, "<Keyboard>/s");
            Keys("Move", "right", 0, "<Keyboard>/d");
            Keys("LightAttack", null, 0, "<Mouse>/leftButton");
            Keys("HeavyAttack", null, 0, "<Mouse>/rightButton");
            Keys("Guard", null, 0, "<Keyboard>/leftShift");
            Keys("Dodge", null, 0, "<Keyboard>/space");
            Keys("Flask", null, 0, "<Keyboard>/r");
            Keys("Spell", null, 0, "<Keyboard>/q");
            Keys("Surge", null, 0, "<Keyboard>/f");
            Keys("Interact", null, 0, "<Keyboard>/e");
            Keys("Pause", null, 0, "<Keyboard>/escape");
            // The second, keys-only set for playing without a mouse.
            Keys("LightAttack", null, 1, "<Keyboard>/j");
            Keys("HeavyAttack", null, 1, "<Keyboard>/k");
            Keys("Guard", null, 1, "<Keyboard>/l");
        }

        [Test]
        public void NoTwoActionsShareAButtonByDefault()
        {
            var map = controls.FindActionMap("Player");
            foreach (var action in map.actions)
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    if (action.bindings[i].isComposite) continue;
                    Assert.IsFalse(ControlBindings.FindClash(map, action, i, out var other, out _),
                        $"{action.name} {action.bindings[i].effectivePath} is also {other?.name}'s");
                }
        }

        [Test]
        public void TheCellsOfTheControlsPageFindTheirBindings()
        {
            var move = controls.FindAction("Move");
            Assert.AreEqual("<Gamepad>/leftStick", move.bindings[ControlBindings.IndexOf(move, ControlBindings.PadGroup)].path, "the d-pad is not offered for rebinding");
            Assert.AreEqual(-1, ControlBindings.IndexOf(controls.FindAction("Spell"), ControlBindings.KeysGroup, null, 1), "Spell has no second keys binding");
        }

        [Test]
        public void AClashIsSwappedAndEscapeIsNeverHandedOn()
        {
            var map = controls.FindActionMap("Player");
            var light = controls.FindAction("LightAttack");
            var spell = controls.FindAction("Spell");
            int lightKeys = ControlBindings.IndexOf(light, ControlBindings.KeysGroup);
            int spellKeys = ControlBindings.IndexOf(spell, ControlBindings.KeysGroup);
            light.ApplyBindingOverride(lightKeys, "<Keyboard>/q");
            Assert.AreEqual(ControlBindings.Settled.Swapped, ControlBindings.Settle(map, light, lightKeys, "<Mouse>/leftButton", out var other, out _));
            Assert.AreEqual(spell, other);
            Assert.AreEqual("<Keyboard>/q", light.bindings[lightKeys].effectivePath);
            Assert.AreEqual("<Mouse>/leftButton", spell.bindings[spellKeys].effectivePath);

            // Pause given Flask's key would hand Escape to the flask: refused, and Pause keeps Escape.
            var pause = controls.FindAction("Pause");
            var flask = controls.FindAction("Flask");
            int pauseKeys = ControlBindings.IndexOf(pause, ControlBindings.KeysGroup);
            pause.ApplyBindingOverride(pauseKeys, "<Keyboard>/r");
            Assert.AreEqual(ControlBindings.Settled.Refused, ControlBindings.Settle(map, pause, pauseKeys, "<Keyboard>/escape", out _, out _));
            Assert.AreEqual("<Keyboard>/escape", pause.bindings[pauseKeys].effectivePath);
            Assert.AreEqual("<Keyboard>/r", flask.bindings[ControlBindings.IndexOf(flask, ControlBindings.KeysGroup)].effectivePath);
            Assert.IsTrue(ControlBindings.IsReserved("<Keyboard>/escape"));
            Assert.IsTrue(ControlBindings.IsReserved("<Gamepad>/start"));

            // A composite part settles the same way: up given S swaps with down.
            var move = controls.FindAction("Move");
            int up = ControlBindings.IndexOf(move, ControlBindings.KeysGroup, "up");
            int down = ControlBindings.IndexOf(move, ControlBindings.KeysGroup, "down");
            move.ApplyBindingOverride(up, "<Keyboard>/s");
            Assert.AreEqual(ControlBindings.Settled.Swapped, ControlBindings.Settle(map, move, up, "<Keyboard>/w", out _, out _));
            Assert.AreEqual("<Keyboard>/w", move.bindings[down].effectivePath);
        }

        [Test]
        public void BindingsAreKeptAndLoadedUnderTheirOwnKey()
        {
            string key = ControlBindings.Key;
            bool had = PlayerPrefs.HasKey(key);
            string before = PlayerPrefs.GetString(key, "");
            var fresh = Object.Instantiate(AssetDatabase.LoadAssetAtPath<InputActionAsset>(PlayerBuilder.ControlsPath));
            try
            {
                var flask = controls.FindAction("Flask");
                int index = ControlBindings.IndexOf(flask, ControlBindings.PadGroup);
                flask.ApplyBindingOverride(index, "<Gamepad>/select");
                ControlBindings.Save(controls);
                StringAssert.Contains("select", PlayerPrefs.GetString(key, ""));
                ControlBindings.Load(fresh);
                var freshFlask = fresh.FindAction("Flask");
                Assert.AreEqual("<Gamepad>/select", freshFlask.bindings[ControlBindings.IndexOf(freshFlask, ControlBindings.PadGroup)].effectivePath);
                ControlBindings.ResetToDefaults(controls);
                Assert.IsFalse(PlayerPrefs.HasKey(key), "reset leaves nothing kept");
                Assert.AreEqual("<Gamepad>/buttonWest", flask.bindings[index].effectivePath);
            }
            finally
            {
                // Whatever was kept before the test is put back as it was.
                if (had) PlayerPrefs.SetString(key, before);
                else PlayerPrefs.DeleteKey(key);
                PlayerPrefs.Save();
                Object.DestroyImmediate(fresh);
            }
        }
    }
}

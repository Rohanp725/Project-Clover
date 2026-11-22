using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The menus' UI map in Controls.inputactions (ui_rest_fire_plan.md 5.0 and 3.5.4), read from the file's text so the
    /// check runs without the editor: every press the Loadout's map names has its pad button and its keys.
    /// </summary>
    public class MenusControlsTests
    {
        private const string ControlsPath = "Assets/_Project/Settings/Controls.inputactions";

        private static HashSet<string> UiBindings()
        {
            string text = File.ReadAllText(ControlsPath);
            int start = text.IndexOf("\"name\": \"UI\"", System.StringComparison.Ordinal);
            Assert.GreaterOrEqual(start, 0, "Controls has no UI map");
            int end = text.IndexOf("\"controlSchemes\"", start, System.StringComparison.Ordinal);
            string block = text.Substring(start, end - start);
            var found = new HashSet<string>();
            foreach (Match match in Regex.Matches(block, "\"path\": \"([^\"]+)\"[^}]*?\"action\": \"([^\"]+)\""))
                found.Add(match.Groups[2].Value + " " + match.Groups[1].Value);
            return found;
        }

        [TestCase("Navigate", "<Gamepad>/leftStick")]
        [TestCase("Navigate", "<Gamepad>/dpad")]
        [TestCase("Navigate", "<Keyboard>/w")]
        [TestCase("Navigate", "<Keyboard>/a")]
        [TestCase("Navigate", "<Keyboard>/s")]
        [TestCase("Navigate", "<Keyboard>/d")]
        [TestCase("Navigate", "<Keyboard>/upArrow")]
        [TestCase("Navigate", "<Keyboard>/downArrow")]
        [TestCase("Navigate", "<Keyboard>/leftArrow")]
        [TestCase("Navigate", "<Keyboard>/rightArrow")]
        [TestCase("MenuMove", "<Gamepad>/leftStick")]
        [TestCase("MenuMove", "<Gamepad>/dpad")]
        [TestCase("MenuMove", "<Keyboard>/w")]
        [TestCase("MenuMove", "<Keyboard>/a")]
        [TestCase("MenuMove", "<Keyboard>/s")]
        [TestCase("MenuMove", "<Keyboard>/d")]
        [TestCase("MenuMove", "<Keyboard>/upArrow")]
        [TestCase("MenuMove", "<Keyboard>/downArrow")]
        [TestCase("MenuMove", "<Keyboard>/leftArrow")]
        [TestCase("MenuMove", "<Keyboard>/rightArrow")]
        [TestCase("Submit", "<Gamepad>/buttonSouth")]
        [TestCase("Submit", "<Keyboard>/e")]
        [TestCase("Submit", "<Keyboard>/enter")]
        [TestCase("Cancel", "<Gamepad>/buttonEast")]
        [TestCase("Cancel", "<Keyboard>/space")]
        [TestCase("Cancel", "<Keyboard>/escape")]
        [TestCase("Cancel", "<Keyboard>/backspace")]
        [TestCase("Keep", "<Gamepad>/buttonWest")]
        [TestCase("Keep", "<Keyboard>/r")]
        [TestCase("Shape", "<Gamepad>/buttonNorth")]
        [TestCase("Shape", "<Keyboard>/q")]
        [TestCase("HalfLeft", "<Gamepad>/leftShoulder")]
        [TestCase("HalfRight", "<Gamepad>/rightShoulder")]
        [TestCase("HalfRight", "<Keyboard>/tab")]
        [TestCase("ScrollPane", "<Gamepad>/rightStick/y")]
        [TestCase("ScrollPane", "<Keyboard>/pageUp")]
        [TestCase("ScrollPane", "<Keyboard>/pageDown")]
        [TestCase("TextSize", "<Gamepad>/leftStickPress")]
        [TestCase("TextSize", "<Keyboard>/t")]
        [TestCase("Point", "<Pointer>/position")]
        [TestCase("Click", "<Mouse>/leftButton")]
        [TestCase("RightClick", "<Mouse>/rightButton")]
        [TestCase("ScrollWheel", "<Mouse>/scroll")]
        public void TheUiMapBindsEveryPressOfTheLoadoutMap(string action, string path)
        {
            Assert.IsTrue(UiBindings().Contains(action + " " + path), $"UI/{action} is not bound to {path}");
        }

        [Test]
        public void NoKeyOrButtonDoesTwoThingsInTheUiMap()
        {
            var seen = new Dictionary<string, string>();
            foreach (string pair in UiBindings())
            {
                int space = pair.IndexOf(' ');
                string action = pair.Substring(0, space), path = pair.Substring(space + 1);
                if (path == "2DVector" || path == "1DAxis") continue;
                // Navigate is MenuMove's twin for the Input System's UI integration (PassThrough); the screen swallows it.
                if (action == "Navigate") continue;
                Assert.IsFalse(seen.TryGetValue(path, out string other) && other != action, $"{path} is both {other} and {action}");
                seen[path] = action;
            }
        }
    }
}

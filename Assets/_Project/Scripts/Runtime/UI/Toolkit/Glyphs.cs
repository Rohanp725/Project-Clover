using Clover.Player;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UIElements;

namespace Clover.UI.Toolkit
{
    /// <summary>
    /// The picture of a press (ui_rest_fire_plan.md 4.3 and 3.5.4): the control an action is bound to on the device last
    /// used, following rebinding, drawn next to what it acts on. A glyph is Kenney's Input Prompts picture of the control
    /// (<see cref="Pictures"/>, keyed by <see cref="Id"/>: "xbox:a", "ps:cross", "key:e", "mouse:left"), drawn white and
    /// tinted by its class (the four Xbox faces in their colours). A control with no picture is a styled label: a coloured
    /// face with its letter for the four face buttons on an Xbox pad, a rounded box with its word for everything else.
    /// </summary>
    public static class Glyphs
    {
        /// <summary>The pack's pictures, handed over by the screen that draws glyphs; null draws every glyph as a label.</summary>
        public static GlyphPictures Pictures { get; set; }

        // Domain reload is off: the last play's pictures must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Pictures = null;

        /// <summary>The control's id for a picture of the pack: device style, then the control.</summary>
        public static string Id(string path, ControlDevice style)
        {
            if (string.IsNullOrEmpty(path)) return "";
            string control = path.Substring(path.IndexOf('/') + 1);
            if (path.StartsWith("<Mouse>", System.StringComparison.OrdinalIgnoreCase)) return "mouse:" + control.Replace("Button", "");
            if (path.StartsWith("<Keyboard>", System.StringComparison.OrdinalIgnoreCase)) return "key:" + control;
            if (style == ControlDevice.PlayStation)
            {
                switch (control)
                {
                    case "buttonSouth": return "ps:cross";
                    case "buttonEast": return "ps:circle";
                    case "buttonWest": return "ps:square";
                    case "buttonNorth": return "ps:triangle";
                }
                return "ps:" + control;
            }
            switch (control)
            {
                case "buttonSouth": return "xbox:a";
                case "buttonEast": return "xbox:b";
                case "buttonWest": return "xbox:x";
                case "buttonNorth": return "xbox:y";
            }
            return "xbox:" + control;
        }

        /// <summary>The path an action is bound to for the device in hand, or "" when it has none there.</summary>
        public static string PathOf(InputActionAsset controls, string actionName, ControlDevice style)
        {
            var action = controls != null ? controls.FindAction(actionName) : null;
            if (action == null) return "";
            bool keys = style == ControlDevice.Keys;
            foreach (var binding in action.bindings)
            {
                if (binding.isComposite) continue;
                string path = binding.effectivePath ?? "";
                bool isKeys = path.StartsWith("<Keyboard>", System.StringComparison.OrdinalIgnoreCase) || path.StartsWith("<Mouse>", System.StringComparison.OrdinalIgnoreCase)
                    || path.StartsWith("<Pointer>", System.StringComparison.OrdinalIgnoreCase);
                if (isKeys == keys) return path;
            }
            return "";
        }

        /// <summary>Makes the glyph for an action as a label (letter or word on its face), or null when the action has no binding on this device.</summary>
        public static Label Make(InputActionAsset controls, string actionName, ControlDevice style, bool small = false)
        {
            string path = PathOf(controls, actionName, style);
            if (path.Length == 0) return null;
            var glyph = new Label();
            Dress(glyph, path, style, small);
            return glyph;
        }

        /// <summary>Dresses a label as the glyph of a control path.</summary>
        public static void Dress(Label glyph, string path, ControlDevice style, bool small = false)
        {
            glyph.ClearClassList();
            glyph.AddToClassList("glyph");
            if (small) glyph.AddToClassList("glyph-small");
            string id = Id(path, style);
            // A glyph the tree named keeps its name, so the screen can find it again.
            if (string.IsNullOrEmpty(glyph.name)) glyph.name = "glyph-" + id.Replace(':', '-');
            glyph.userData = id;
            var picture = Pictures != null && !Worded(id) ? Pictures.Find(id) : null;
            glyph.style.backgroundImage = picture != null ? new StyleBackground(picture) : new StyleBackground(StyleKeyword.Null);
            if (picture != null)
            {
                glyph.text = "";
                glyph.AddToClassList("glyph-picture");
                switch (id)
                {
                    case "xbox:a": glyph.AddToClassList("glyph-a"); break;
                    case "xbox:b": glyph.AddToClassList("glyph-b"); break;
                    case "xbox:x": glyph.AddToClassList("glyph-x"); break;
                    case "xbox:y": glyph.AddToClassList("glyph-y"); break;
                }
                return;
            }
            switch (id)
            {
                case "xbox:a": Face(glyph, "A", "glyph-a"); return;
                case "xbox:b": Face(glyph, "B", "glyph-b"); return;
                case "xbox:x": Face(glyph, "X", "glyph-x"); return;
                case "xbox:y": Face(glyph, "Y", "glyph-y"); return;
            }
            glyph.AddToClassList("glyph-wide");
            glyph.text = style == ControlDevice.Keys ? ControlNames.Keys(path) : ControlNames.Pad(path, style);
        }

        /// <summary>
        /// True for a key whose picture carries a word (Tab, Space, Page Down): at glyph size the pack's word is a few
        /// pixels tall, so these stay a box with the word in reading type. Letters, digits and arrows use the pictures.
        /// </summary>
        public static bool Worded(string id)
        {
            if (string.IsNullOrEmpty(id) || !id.StartsWith("key:", System.StringComparison.Ordinal)) return false;
            string control = id.Substring(4);
            return control.Length > 1 && !control.EndsWith("Arrow", System.StringComparison.Ordinal);
        }

        private static void Face(Label glyph, string letter, string colour)
        {
            glyph.text = letter;
            glyph.AddToClassList(colour);
        }
    }
}

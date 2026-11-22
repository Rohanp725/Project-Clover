using UnityEngine;

namespace Clover.UI.Toolkit
{
    /// <summary>
    /// Kenney's Input Prompts pictures (ui_rest_fire_plan.md 5.2, the owner's Q7), keyed by <see cref="Glyphs.Id"/>
    /// ("xbox:a", "ps:cross", "key:leftCtrl", "mouse:left"). Made by UiToolkitBuilder from
    /// Art/Textures/UI/Glyphs (file glyph_key_left_ctrl.png is id "key:leftCtrl"), so a new picture there is picked up
    /// by building again; the Loadout's screen hands it to <see cref="Glyphs"/>.
    /// </summary>
    [CreateAssetMenu(menuName = "Clover/UI/Glyph Pictures")]
    public class GlyphPictures : ScriptableObject
    {
        [SerializeField] private string[] ids = new string[0];
        [SerializeField] private Texture2D[] pictures = new Texture2D[0];

        public int Count => ids != null ? ids.Length : 0;

        /// <summary>Puts the pictures in (the builder): ids and pictures pair by index.</summary>
        public void Set(string[] newIds, Texture2D[] newPictures)
        {
            ids = newIds ?? new string[0];
            pictures = newPictures ?? new Texture2D[0];
        }

        /// <summary>
        /// The picture of a control id, or null. A part of a control ("xbox:rightStick/y", "xbox:dpad/up") falls back to
        /// the whole control's picture.
        /// </summary>
        public Texture2D Find(string id)
        {
            if (string.IsNullOrEmpty(id) || ids == null) return null;
            var found = Exact(id);
            if (found != null) return found;
            int part = id.IndexOf('/');
            return part > 0 ? Exact(id.Substring(0, part)) : null;
        }

        private Texture2D Exact(string id)
        {
            for (int i = 0; i < ids.Length && i < pictures.Length; i++)
                if (string.Equals(ids[i], id, System.StringComparison.OrdinalIgnoreCase)) return pictures[i];
            return null;
        }

        /// <summary>The id a picture's file name stands for: glyph_key_left_ctrl is "key:leftCtrl".</summary>
        public static string IdOfFile(string fileName)
        {
            if (string.IsNullOrEmpty(fileName) || !fileName.StartsWith("glyph_", System.StringComparison.Ordinal)) return "";
            string[] words = fileName.Substring(6).Split('_');
            if (words.Length < 2) return "";
            var control = new System.Text.StringBuilder(words[1]);
            for (int i = 2; i < words.Length; i++)
                if (words[i].Length > 0) control.Append(char.ToUpperInvariant(words[i][0])).Append(words[i], 1, words[i].Length - 1);
            return words[0] + ":" + control;
        }
    }
}

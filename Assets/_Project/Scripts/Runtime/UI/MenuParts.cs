using UnityEngine;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// The pieces the menus made at play are built from (the rest menu, the Hand): a placed rectangle, a label in the
    /// HUD's face with the game's ink outline, a filled box, and the HUD's own font and inked frame found by name, so
    /// a menu made by code looks like the ones the builders made.
    /// </summary>
    public static class MenuParts
    {
        public static readonly Color Ink = new Color(0.02f, 0.018f, 0.05f, 1f);
        public static readonly Color Plain = new Color(0.95f, 0.93f, 0.9f, 1f);
        public static readonly Color Gold = new Color(1f, 0.8f, 0.45f, 1f);
        public static readonly Color Faint = new Color(1f, 1f, 1f, 0.6f);
        public static readonly Color Rule = new Color(0.8f, 0.72f, 0.55f, 0.7f);

        /// <summary>A rectangle under a parent, anchored and pivoted at one point.</summary>
        public static RectTransform Rect(string name, Transform parent, Vector2 anchor, Vector2 pivot, Vector2 at, Vector2 size)
        {
            var made = new GameObject(name, typeof(RectTransform));
            made.transform.SetParent(parent, false);
            var rect = (RectTransform)made.transform;
            rect.anchorMin = rect.anchorMax = anchor;
            rect.pivot = pivot;
            rect.anchoredPosition = at;
            rect.sizeDelta = size;
            return rect;
        }

        /// <summary>A label placed from its parent's top left, with the ink outline every HUD word has.</summary>
        public static Text Label(string name, Transform parent, Font font, int size, TextAnchor anchor, Color colour, Vector2 at, Vector2 box)
        {
            var rect = Rect(name, parent, new Vector2(0f, 1f), new Vector2(0f, 1f), at, box);
            var label = rect.gameObject.AddComponent<Text>();
            label.font = font;
            // Sizes are given as for a narrow face, as the HUD's builder gives them.
            label.fontSize = Mathf.RoundToInt(size * 0.86f);
            label.alignment = anchor;
            label.color = colour;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            var outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.8f);
            outline.effectDistance = new Vector2(2f, -2f);
            return label;
        }

        /// <summary>A filled box placed from its parent's top left; sliced when a sprite is given.</summary>
        public static Image Box(string name, Transform parent, Vector2 at, Vector2 size, Color colour, Sprite sprite = null)
        {
            var rect = Rect(name, parent, new Vector2(0f, 1f), new Vector2(0f, 1f), at, size);
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            if (sprite != null) image.type = Image.Type.Sliced;
            image.color = colour;
            image.raycastTarget = false;
            return image;
        }

        /// <summary>The HUD's font, its inked frame and the brush stroke under a chosen row, where the builders left them.</summary>
        public static void FindLook(Transform hud, out Font font, out Sprite frame, out Sprite brush)
        {
            var anyText = hud.GetComponentInChildren<Text>(true);
            font = anyText != null ? anyText.font : null;
            var framed = hud.Find("Pause Menu/Panel");
            frame = framed != null && framed.GetComponent<Image>() != null ? framed.GetComponent<Image>().sprite : null;
            var brushed = hud.Find("Pause Menu/Panel/List/Cursor");
            brush = brushed != null && brushed.GetComponent<Image>() != null ? brushed.GetComponent<Image>().sprite : null;
        }

        /// <summary>The HUD to build a menu under: the pause menu's, the chart's, or any player HUD. Null when the scene has none.</summary>
        public static Transform FindHud()
        {
            if (PauseMenu.Instance != null) return PauseMenu.Instance.transform;
            if (UpgradeMenu.Instance != null) return UpgradeMenu.Instance.transform;
            var found = Object.FindAnyObjectByType<PlayerHud>();
            return found != null ? found.transform : null;
        }

        /// <summary>True when a screen point lies on a rectangle of an overlay canvas.</summary>
        public static bool Contains(RectTransform rect, Vector2 at) => rect != null && rect.gameObject.activeInHierarchy && RectTransformUtility.RectangleContainsScreenPoint(rect, at, null);
    }
}

using Clover.Gameplay;
using Clover.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// The marks by the health bar for what is laid on the player (the owner's answer of 2026-11-22): a small square in the
    /// affliction's colour for each, with its name and the whole seconds it has left under it, in a row to the right of the
    /// bar. Built by code beside the health bar (<see cref="Make"/>); it makes nothing after it is built.
    /// </summary>
    public class AfflictionBadges : MonoBehaviour
    {
        /// <summary>The most marks shown at once.</summary>
        public const int Most = 6;
        private const float Size = 22f, Gap = 6f;

        private PlayerDefence defence;
        private readonly AfflictionKind[] kinds = new AfflictionKind[Most];
        private readonly float[] seconds = new float[Most];
        private readonly Image[] squares = new Image[Most];
        private readonly Text[] words = new Text[Most];
        private readonly int[] shownSeconds = new int[Most];
        private readonly AfflictionKind[] shownKinds = new AfflictionKind[Most];

        /// <summary>The words under a mark: the affliction's name and the whole seconds it has left ("Burn 3").</summary>
        public static string Words(AfflictionKind kind, float secondsLeft) => Afflictions.Spec(kind).Name + " " + Mathf.CeilToInt(Mathf.Max(0f, secondsLeft));

        /// <summary>Builds the row just to the right of <paramref name="healthBar"/>, in its font; null when there is no bar or no player.</summary>
        public static AfflictionBadges Make(RectTransform healthBar, Font font, PlayerDefence owner)
        {
            if (healthBar == null || owner == null) return null;
            var made = new GameObject("Affliction Marks", typeof(RectTransform));
            var area = (RectTransform)made.transform;
            area.SetParent(healthBar, false);
            area.anchorMin = new Vector2(1f, 0.5f);
            area.anchorMax = new Vector2(1f, 0.5f);
            area.pivot = new Vector2(0f, 0.5f);
            area.anchoredPosition = new Vector2(10f, 0f);
            area.sizeDelta = new Vector2(Most * (Size + Gap), Size);
            var shown = made.AddComponent<AfflictionBadges>();
            shown.Build(owner, font);
            return shown;
        }

        private void Build(PlayerDefence owner, Font font)
        {
            defence = owner;
            for (int i = 0; i < Most; i++)
            {
                var square = new GameObject("Mark " + i, typeof(RectTransform), typeof(Image));
                var rect = (RectTransform)square.transform;
                rect.SetParent(transform, false);
                rect.anchorMin = rect.anchorMax = new Vector2(0f, 0.5f);
                rect.pivot = new Vector2(0f, 0.5f);
                rect.sizeDelta = new Vector2(Size, Size);
                rect.anchoredPosition = new Vector2(i * (Size + Gap), 0f);
                squares[i] = square.GetComponent<Image>();
                squares[i].raycastTarget = false;
                var label = new GameObject("Words", typeof(RectTransform), typeof(Text));
                var labelRect = (RectTransform)label.transform;
                labelRect.SetParent(rect, false);
                labelRect.anchorMin = new Vector2(0.5f, 0f);
                labelRect.anchorMax = new Vector2(0.5f, 0f);
                labelRect.pivot = new Vector2(0.5f, 1f);
                labelRect.sizeDelta = new Vector2(Size * 3f, 16f);
                labelRect.anchoredPosition = new Vector2(0f, -2f);
                words[i] = label.GetComponent<Text>();
                words[i].font = font;
                words[i].fontSize = 12;
                words[i].alignment = TextAnchor.UpperCenter;
                words[i].raycastTarget = false;
                words[i].color = Color.white;
                square.SetActive(false);
                shownSeconds[i] = -1;
            }
        }

        private void LateUpdate()
        {
            if (defence == null) return;
            int count = defence.Afflictions.Shown(kinds, seconds);
            for (int i = 0; i < Most; i++)
            {
                bool on = i < count;
                if (squares[i].gameObject.activeSelf != on) squares[i].gameObject.SetActive(on);
                if (!on) continue;
                int whole = Mathf.CeilToInt(seconds[i]);
                // Words are written again only when the kind or the whole second changes: no new text each frame.
                if (whole == shownSeconds[i] && kinds[i] == shownKinds[i]) continue;
                shownSeconds[i] = whole;
                shownKinds[i] = kinds[i];
                var colour = Afflictions.Spec(kinds[i]).Colour;
                squares[i].color = new Color(colour.r, colour.g, colour.b, 0.9f);
                words[i].text = Words(kinds[i], seconds[i]);
            }
        }
    }
}

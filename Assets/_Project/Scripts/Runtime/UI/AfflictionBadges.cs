using Clover.Gameplay;
using Clover.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// The marks by the health bar for what is laid on the player (the owner's answer of 2026-11-22): a small square in the
    /// affliction's colour for each, with its name and the whole seconds it has left under it, in a row to the right of the
    /// bar. The words are the menus' smallest size (ui_rest_fire_plan.md 4.1: 24 on the 1920 by 1080 board, 16 px at 720p)
    /// in the reading face, Atkinson Hyperlegible Bold, with the dark outline words drawn over the world keep. Built by code
    /// beside the health bar (<see cref="Make"/>); it makes nothing after it is built.
    /// </summary>
    public class AfflictionBadges : MonoBehaviour
    {
        /// <summary>The most marks shown at once.</summary>
        public const int Most = 6;
        /// <summary>The words' size on the 1920 by 1080 board: the least anything a player must read may be.</summary>
        public const int WordsSize = 24;
        /// <summary>The coloured square's side, and the room left between one mark's words and the next square.</summary>
        public const float Size = 26f, Gap = 18f;
        /// <summary>The words' colour: the menus' near-white Text (never pure white), since they sit over the world.</summary>
        public static readonly Color WordsColour = new Color(0xEE / 255f, 0xE9 / 255f, 0xE1 / 255f);

        private PlayerDefence defence;
        private readonly AfflictionKind[] kinds = new AfflictionKind[Most];
        private readonly float[] seconds = new float[Most];
        private readonly Image[] squares = new Image[Most];
        private readonly Text[] words = new Text[Most];
        private readonly float[] widths = new float[Most];
        private readonly int[] shownSeconds = new int[Most];
        private readonly AfflictionKind[] shownKinds = new AfflictionKind[Most];

        /// <summary>The words under a mark: the affliction's name and the whole seconds it has left ("Burn 3").</summary>
        public static string Words(AfflictionKind kind, float secondsLeft) => Afflictions.Spec(kind).Name + " " + Mathf.CeilToInt(Mathf.Max(0f, secondsLeft));

        /// <summary>Where each mark starts along the row, given how wide each one's words are: a mark takes its square or its words, whichever is wider.</summary>
        public static void Lay(float[] wordWidths, int count, float[] starts)
        {
            float x = 0f;
            for (int i = 0; i < count; i++)
            {
                starts[i] = x;
                x += Mathf.Max(Size, wordWidths[i]) + Gap;
            }
        }

        /// <summary>Builds the row just to the right of <paramref name="healthBar"/>, in the font given; null when there is no bar or no player.</summary>
        public static AfflictionBadges Make(RectTransform healthBar, Font font, PlayerDefence owner)
        {
            if (healthBar == null || owner == null) return null;
            var made = new GameObject("Affliction Marks", typeof(RectTransform));
            var area = (RectTransform)made.transform;
            area.SetParent(healthBar, false);
            area.anchorMin = new Vector2(1f, 0.5f);
            area.anchorMax = new Vector2(1f, 0.5f);
            area.pivot = new Vector2(0f, 0.5f);
            area.anchoredPosition = new Vector2(14f, 0f);
            area.sizeDelta = new Vector2(Most * (Size + Gap), Size);
            var shown = made.AddComponent<AfflictionBadges>();
            shown.Build(owner, font);
            return shown;
        }

        /// <summary>The words each shown mark reads now, for probes and stills.</summary>
        public string Reading()
        {
            var said = new System.Text.StringBuilder();
            for (int i = 0; i < Most; i++)
            {
                if (!squares[i].gameObject.activeSelf) continue;
                if (said.Length > 0) said.Append(" | ");
                said.Append(words[i].text);
            }
            return said.ToString();
        }

        /// <summary>The font the words are drawn in.</summary>
        public Font WordsFont => words[0] != null ? words[0].font : null;

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
                // Left-aligned under the square, so a long name runs on to the right and never back over the bar.
                labelRect.anchorMin = labelRect.anchorMax = new Vector2(0f, 0f);
                labelRect.pivot = new Vector2(0f, 1f);
                labelRect.sizeDelta = new Vector2(200f, WordsSize * 1.3f);
                labelRect.anchoredPosition = new Vector2(0f, -3f);
                words[i] = label.GetComponent<Text>();
                words[i].font = font;
                words[i].fontSize = WordsSize;
                words[i].fontStyle = FontStyle.Normal;
                words[i].alignment = TextAnchor.UpperLeft;
                words[i].raycastTarget = false;
                words[i].horizontalOverflow = HorizontalWrapMode.Overflow;
                words[i].verticalOverflow = VerticalWrapMode.Overflow;
                words[i].color = WordsColour;
                // Drawn over the world, not on a panel: the dark edge keeps them readable on bright floors and effects.
                var edge = label.AddComponent<Outline>();
                edge.effectColor = new Color(0.04f, 0.035f, 0.08f, 0.9f);
                edge.effectDistance = new Vector2(1.5f, -1.5f);
                square.SetActive(false);
                shownSeconds[i] = -1;
            }
        }

        private readonly float[] starts = new float[Most];

        private void LateUpdate()
        {
            if (defence == null) return;
            int count = defence.Afflictions.Shown(kinds, seconds);
            bool moved = false;
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
                float width = words[i].preferredWidth;
                if (!Mathf.Approximately(width, widths[i])) { widths[i] = width; moved = true; }
            }
            if (!moved) return;
            Lay(widths, count, starts);
            for (int i = 0; i < count; i++) ((RectTransform)squares[i].transform).anchoredPosition = new Vector2(starts[i], 0f);
        }
    }
}

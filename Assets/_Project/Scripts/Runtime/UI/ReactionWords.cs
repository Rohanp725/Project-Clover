using Clover.Gameplay;
using Clover.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// A reaction's name, written briefly over the enemy it happened on (IMPLEMENTATION.md 4.3, 8.2): a short status word,
    /// so it is timed, rising a little as it fades. A few words are made once and reused. Built by code at play under the
    /// HUD, as the spell diamond is, so no scene is rebuilt; per frame it only moves and fades what is showing.
    /// </summary>
    public class ReactionWords : MonoBehaviour
    {
        /// <summary>Real seconds a word stays, how high over the enemy it starts and how far it rises, in metres.</summary>
        public float Seconds = 1.5f;
        public float Height = 2.6f;
        public float Rise = 0.5f;
        public int FontSize = 30;
        /// <summary>How near the screen's sides and top and bottom a word may come, in canvas units (the HUD's bars sit along the top).</summary>
        public Vector2 EdgeMargin = new Vector2(220f, 100f);

        private const int Most = 6;

        private class Word
        {
            public RectTransform Rect;
            public Text Text;
            public Transform Over;
            public Vector3 Last;
            public Color Colour;
            public float Left;
            // Its place in the stack over one enemy, so two reactions close together are both read.
            public int Row;
        }

        private readonly Word[] words = new Word[Most];
        private PlayerReactions reactions;
        private RectTransform area;
        private Camera view;

        /// <summary>Counts for probes: words shown, and the last one written.</summary>
        public int Written { get; private set; }
        public string LastWord { get; private set; } = "";
        /// <summary>How many words are up now.</summary>
        public int Showing
        {
            get
            {
                int up = 0;
                foreach (var word in words)
                    if (word != null && word.Left > 0f) up++;
                return up;
            }
        }

        public static ReactionWords Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;

        /// <summary>Builds the words under the HUD, in the font of the given text and drawn just after it (under the menus).</summary>
        public static ReactionWords Make(Transform hud, Text besides, PlayerController owner)
        {
            var found = owner != null ? owner.GetComponent<PlayerReactions>() : null;
            if (hud == null || found == null) return null;
            var made = new GameObject("Reaction Words", typeof(RectTransform));
            made.transform.SetParent(hud, false);
            if (besides != null && besides.transform.parent == hud) made.transform.SetSiblingIndex(besides.transform.GetSiblingIndex() + 1);
            var shown = made.AddComponent<ReactionWords>();
            shown.Build(found, besides != null ? besides.font : null);
            return shown;
        }

        private void OnEnable() => Instance = this;

        private void OnDisable()
        {
            if (Instance == this) Instance = null;
        }

        private void OnDestroy()
        {
            if (reactions != null) reactions.Shown -= OnShown;
        }

        private void Build(PlayerReactions heard, Font font)
        {
            reactions = heard;
            area = (RectTransform)transform;
            // The whole screen, so a word can go wherever its enemy stands.
            area.anchorMin = Vector2.zero;
            area.anchorMax = Vector2.one;
            area.offsetMin = area.offsetMax = Vector2.zero;
            for (int i = 0; i < Most; i++)
            {
                var made = new GameObject("Word", typeof(RectTransform), typeof(Text), typeof(Shadow));
                made.transform.SetParent(transform, false);
                var rect = (RectTransform)made.transform;
                rect.sizeDelta = new Vector2(420f, 48f);
                var text = made.GetComponent<Text>();
                text.font = font;
                text.fontSize = FontSize;
                text.fontStyle = FontStyle.Bold;
                text.alignment = TextAnchor.MiddleCenter;
                text.horizontalOverflow = HorizontalWrapMode.Overflow;
                text.raycastTarget = false;
                made.GetComponent<Shadow>().effectDistance = new Vector2(2f, -2f);
                made.SetActive(false);
                words[i] = new Word { Rect = rect, Text = text };
            }
            reactions.Shown += OnShown;
        }

        private void OnShown(Component who, int pair)
        {
            if (who == null) return;
            // The word nearest its end gives its place.
            var word = words[0];
            for (int i = 1; i < Most; i++)
                if (words[i].Left < word.Left) word = words[i];
            int row = 0;
            for (int i = 0; i < Most; i++)
                if (words[i] != word && words[i].Left > 0f && words[i].Over == who.transform) row = Mathf.Max(row, words[i].Row + 1);
            word.Row = row;
            word.Over = who.transform;
            word.Last = who.transform.position;
            word.Text.text = AfflictionPairs.Name(pair);
            word.Colour = Color.Lerp(ReactionLook.Colour(pair), Color.white, 0.35f);
            word.Left = Seconds;
            word.Rect.gameObject.SetActive(true);
            Written++;
            LastWord = word.Text.text;
        }

        private void LateUpdate()
        {
            if (view == null) view = Camera.main;
            float step = Time.unscaledDeltaTime;
            for (int i = 0; i < Most; i++)
            {
                var word = words[i];
                if (word.Left <= 0f) continue;
                word.Left -= step;
                if (word.Left <= 0f || view == null)
                {
                    word.Left = 0f;
                    word.Rect.gameObject.SetActive(false);
                    continue;
                }
                // It stays where the enemy was if the enemy is gone.
                if (word.Over != null && word.Over.gameObject.activeInHierarchy) word.Last = word.Over.position;
                float through = 1f - word.Left / Seconds;
                Vector3 screen = view.WorldToScreenPoint(word.Last + Vector3.up * (Height + Rise * through));
                bool behind = screen.z < 0f;
                if (!behind && RectTransformUtility.ScreenPointToLocalPointInRectangle(area, screen, null, out Vector2 local))
                {
                    // Kept on the screen and under the HUD's top line, so a word over an enemy far ahead is still read whole.
                    var bounds = area.rect;
                    float top = bounds.yMax - EdgeMargin.y;
                    local.x = Mathf.Clamp(local.x, bounds.xMin + EdgeMargin.x, bounds.xMax - EdgeMargin.x);
                    local.y = Mathf.Clamp(local.y, bounds.yMin + EdgeMargin.y, top);
                    // A later word over the same enemy goes above the earlier, or below it where the top is near.
                    float row = word.Row * FontSize * 1.15f;
                    local.y = local.y + row <= top ? local.y + row : local.y - row;
                    word.Rect.localPosition = local;
                }
                var colour = word.Colour;
                colour.a = behind ? 0f : Mathf.Clamp01(word.Left / (Seconds * 0.35f));
                word.Text.color = colour;
            }
        }
    }
}

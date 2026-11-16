using System.Text;
using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// The words over a thing in the test yard: what it gives, what that does and what sets it going. The card is
    /// hidden until the player stands near, so a row of stones does not become a wall of text. A thing whose gift
    /// depends on the element held (the debug shrines, the element stone) is asked again whenever the element changes.
    /// Only the yard's builder makes these: a run has no cards.
    /// </summary>
    public class StoneCard : MonoBehaviour
    {
        /// <summary>The player sees the card from this near.</summary>
        public float ShowWithin = 2.4f;
        /// <summary>The longest line, in letters, before the words wrap.</summary>
        public int LineLength = 38;
        /// <summary>Words that never change. Empty: the thing itself is asked (<see cref="Interactable.Card"/>).</summary>
        [TextArea] public string Words = "";

        [SerializeField] private Interactable thing;
        [SerializeField] private TextMesh text;
        [SerializeField] private Transform back;
        [SerializeField] private GameObject shown;
        [SerializeField] private float lineHeight = 0.2f, letterWidth = 0.085f;

        private Transform player;
        private PlayerElement element;
        private string askedFor;
        private bool visible;
        private float nextAsk;

        public bool Visible => visible;
        public string Showing => text != null ? text.text : "";

        public void Configure(Interactable of, TextMesh words, Transform backing, GameObject whole, float heightOfLine, float widthOfLetter)
        {
            thing = of;
            text = words;
            back = backing;
            shown = whole;
            lineHeight = heightOfLine;
            letterWidth = widthOfLetter;
        }

        private void Start()
        {
            var who = FindAnyObjectByType<PlayerInteraction>();
            if (who != null)
            {
                player = who.transform;
                element = who.GetComponent<PlayerElement>();
            }
            if (shown != null) shown.SetActive(false);
            if (!string.IsNullOrEmpty(Words)) Set(Words);
        }

        private void Update()
        {
            if (player == null || shown == null) return;
            bool near = (player.position - transform.position).sqrMagnitude < ShowWithin * ShowWithin;
            if (near != visible)
            {
                visible = near;
                shown.SetActive(near);
                nextAsk = 0f;
            }
            // Words that depend on the element are looked at a few times a second while the card is up, and written
            // again only when the element is another one.
            if (!visible || !string.IsNullOrEmpty(Words) || thing == null || element == null || element.Info == null) return;
            if (Time.unscaledTime < nextAsk) return;
            nextAsk = Time.unscaledTime + 0.25f;
            string holds = element.Info.Name + thing.CardStamp;
            if (holds == askedFor) return;
            askedFor = holds;
            Set(thing.Card(element));
        }

        private void Set(string words)
        {
            if (text == null) return;
            int lines, longest;
            text.text = Wrap(words, LineLength, out lines, out longest);
            if (back != null) back.localScale = new Vector3(longest * letterWidth + 0.35f, lines * lineHeight + 0.3f, 1f);
        }

        /// <summary>Breaks words into lines no longer than <paramref name="width"/> letters; a line break already in the words is kept.</summary>
        public static string Wrap(string words, int width, out int lines, out int longest)
        {
            var built = new StringBuilder(words.Length + 16);
            lines = 1;
            longest = 0;
            int onLine = 0;
            foreach (string paragraph in words.Split('\n'))
            {
                if (built.Length > 0)
                {
                    built.Append('\n');
                    lines++;
                    onLine = 0;
                }
                foreach (string word in paragraph.Split(' '))
                {
                    if (word.Length == 0) continue;
                    if (onLine > 0 && onLine + 1 + word.Length > width)
                    {
                        built.Append('\n');
                        lines++;
                        onLine = 0;
                    }
                    if (onLine > 0)
                    {
                        built.Append(' ');
                        onLine++;
                    }
                    built.Append(word);
                    onLine += word.Length;
                    if (onLine > longest) longest = onLine;
                }
            }
            return built.ToString();
        }
    }
}

using Clover.Core;
using Clover.Player;
using Clover.Player.Magic;
using UnityEngine;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// The spell diamond (IMPLEMENTATION.md 4.3): the four slots laid out as the face buttons they are cast with, each
    /// with its spell's picture (or its initials), a ring that empties as its clock runs, and its mana where it is paid
    /// in mana; the surge in the middle; the two infused strikes under it with their own rings; and the charged
    /// blade's three pips beside it. Small in the bottom-right corner at all times, so cooldowns can be read without
    /// holding anything; large while L2 (or Left Ctrl) is held, with each spell's name. Button names follow the device
    /// in use and the player's own bindings. Built by code at play under the HUD, as the guide is, so no scene is
    /// rebuilt. Per frame it only sets fill amounts and a scale; words change only when what they say changes.
    /// </summary>
    public class SpellDiamondHud : MonoBehaviour
    {
        /// <summary>The diamond's size at rest and while the layer is held (1 is the large layout), and how fast it changes.</summary>
        public float SmallScale = 0.7f;
        public float LargeScale = 1f;
        public float GrowSeconds = 0.1f;
        /// <summary>Real seconds a slot glows when its clock comes ready.</summary>
        public float FlashSeconds = 0.35f;
        /// <summary>Pixels (at 1920 by 1080) from the bottom-right corner.</summary>
        public Vector2 Margin = new Vector2(44f, 36f);

        private const float SlotSize = 76f;
        private const float Spread = 84f;
        private const float StrikeSize = 50f;
        private const int Slots = 4;

        private static readonly Color Ink = new Color(0.03f, 0.025f, 0.06f, 0.82f);
        private static readonly Color Shade = new Color(0.02f, 0.015f, 0.04f, 0.72f);
        private static readonly Color Words = new Color(1f, 0.95f, 0.86f, 1f);
        private static readonly Color Faint = new Color(1f, 0.95f, 0.86f, 0.6f);
        private static readonly Color ManaBlue = new Color(0.55f, 0.75f, 1f, 1f);
        private static readonly Color Short = new Color(1f, 0.45f, 0.4f, 1f);
        // Whole seconds as words, made once: the clocks' numbers are written without making new strings.
        private static readonly string[] Seconds = MakeSeconds(60);

        private class Button
        {
            public RectTransform Root;
            public Image Rim, RimBack, Back, Icon, Clock;
            public Text Initials, Wait, Key, Price, Name;
            public int ShownWait = -1;
            public float ShownFill = -1f;
            public float Flash;
            public bool Affordable = true;
        }

        private PlayerController player;
        private PlayerSpells spells;
        private PlayerCombat combat;
        private PlayerElement element;
        private RectTransform root;
        private readonly Button[] slots = new Button[Slots];
        private Button surge, strikeFirst, strikeSecond;
        private Text layerKey;
        private Image[] pips;
        private Sprite disc, ring;
        private Texture2D discTexture, ringTexture;
        private float scale;
        private int shownSlots = -1, shownDevice = -1, shownBlade = -1;
        private int shownElement = -1;
        private bool shownLarge;
        private string layerLine = "", surgeLine = "";
        private int shownHas = -1;
        private Font font;

        /// <summary>Counts for probes: whether it is drawn large now, and how often the words were rewritten.</summary>
        public bool Large => shownLarge;
        public float Scale => scale;
        public int Rewrites { get; private set; }
        public static SpellDiamondHud Instance { get; private set; }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Instance = null;

        /// <summary>Builds the diamond under the HUD, beside the given text (whose font it takes and after which it draws, under the menus).</summary>
        public static SpellDiamondHud Make(Transform hud, Text besides, PlayerController owner)
        {
            if (hud == null || owner == null) return null;
            var made = new GameObject("Spell Diamond", typeof(RectTransform));
            made.transform.SetParent(hud, false);
            if (besides != null && besides.transform.parent == hud) made.transform.SetSiblingIndex(besides.transform.GetSiblingIndex() + 1);
            var diamond = made.AddComponent<SpellDiamondHud>();
            diamond.Build(owner, besides != null ? besides.font : null);
            return diamond;
        }

        private void OnEnable() => Instance = this;

        private void OnDisable()
        {
            if (Instance == this) Instance = null;
        }

        private void OnDestroy()
        {
            if (discTexture != null) Destroy(discTexture);
            if (ringTexture != null) Destroy(ringTexture);
            if (disc != null) Destroy(disc);
            if (ring != null) Destroy(ring);
            if (spells != null) spells.SlotReady -= OnReady;
        }

        private void Build(PlayerController owner, Font withFont)
        {
            player = owner;
            spells = owner.GetComponent<PlayerSpells>();
            combat = owner.GetComponent<PlayerCombat>();
            element = owner.GetComponent<PlayerElement>();
            font = withFont;
            disc = MakeCircle(out discTexture, 0f);
            ring = MakeCircle(out ringTexture, 0.8f);

            root = (RectTransform)transform;
            root.anchorMin = root.anchorMax = root.pivot = new Vector2(1f, 0f);
            root.sizeDelta = new Vector2(600f, 440f);
            root.anchoredPosition = new Vector2(-Margin.x, Margin.y);

            // The diamond's middle; the strikes sit low on its left, the pips in the gap between West and North.
            var middle = new Vector2(-250f, 250f);
            float half = SlotSize * 0.5f;
            Vector2[] places = { new Vector2(-Spread, 0f), new Vector2(0f, Spread), new Vector2(Spread, 0f), new Vector2(0f, -Spread) };
            // Each slot's key sits on its outer side, so the four never crowd the middle; names go outward too.
            Vector2[] keyAt = { new Vector2(-half - 10f, -half + 6f), new Vector2(half + 6f, half - 4f), new Vector2(half + 10f, -half + 6f), new Vector2(half + 6f, -half + 4f) };
            Vector2[] nameAt = { new Vector2(-half - 128f, 0f), new Vector2(0f, half + 18f), new Vector2(half + 128f, 0f), new Vector2(0f, -half - 40f) };
            TextAnchor[] nameAnchor = { TextAnchor.MiddleRight, TextAnchor.MiddleCenter, TextAnchor.MiddleLeft, TextAnchor.MiddleCenter };
            Vector2[] priceAt = { new Vector2(0f, -half - 10f), new Vector2(-half - 34f, 0f), new Vector2(0f, -half - 10f), new Vector2(-half - 34f, 0f) };
            for (int i = 0; i < Slots; i++)
                slots[i] = MakeButton("Slot " + ControlNames.SlotWord(i), middle + places[i], SlotSize, keyAt[i], nameAt[i], nameAnchor[i], priceAt[i]);
            surge = MakeButton("Surge", middle, SlotSize * 0.62f, Vector2.zero, Vector2.zero, TextAnchor.MiddleCenter, Vector2.zero);
            surge.Key.gameObject.SetActive(false);
            surge.Price.gameObject.SetActive(false);
            float strikeHalf = StrikeSize * 0.5f;
            strikeFirst = MakeButton("Strike R1", new Vector2(-500f, 130f), StrikeSize, new Vector2(0f, -strikeHalf - 13f), new Vector2(0f, -strikeHalf - 36f), TextAnchor.MiddleCenter, new Vector2(0f, strikeHalf + 12f));
            strikeSecond = MakeButton("Strike R2", new Vector2(-375f, 130f), StrikeSize, new Vector2(0f, -strikeHalf - 13f), new Vector2(0f, -strikeHalf - 36f), TextAnchor.MiddleCenter, new Vector2(0f, strikeHalf + 12f));
            // Above North's name (which reaches half + 30 over North's middle while large), so a spell there is not written over.
            layerKey = Label("Layer", root, 22, TextAnchor.LowerCenter, Faint, middle + new Vector2(0f, Spread + half + 56f), new Vector2(420f, 28f));

            // The charged blade's pips, in the gap between West and North.
            pips = new Image[3];
            for (int i = 0; i < pips.Length; i++)
            {
                var pip = Rect("Blade " + (i + 1), root, middle + new Vector2(-58f - i * 24f, 64f + i * 2f), new Vector2(17f, 17f));
                pip.localRotation = Quaternion.Euler(0f, 0f, 45f);
                pips[i] = pip.gameObject.AddComponent<Image>();
                pips[i].raycastTarget = false;
                pip.gameObject.SetActive(false);
            }
            if (spells != null) spells.SlotReady += OnReady;
            scale = SmallScale;
            root.localScale = new Vector3(scale, scale, 1f);
        }

        // One round button: a dark disc, a rim in the element's colour, a picture or initials, the clock's shade over it,
        // the seconds left, and its key, price and name placed where the layout says (the name only while large).
        private Button MakeButton(string name, Vector2 at, float size, Vector2 keyAt, Vector2 nameAt, TextAnchor nameAnchor, Vector2 priceAt)
        {
            var button = new Button();
            button.Root = Rect(name, root, at, new Vector2(size, size));
            button.Back = Picture("Back", button.Root, disc, Ink, size);
            button.Icon = Picture("Icon", button.Root, null, Words, size * 0.6f);
            button.Initials = Label("Initials", button.Root, Mathf.RoundToInt(size * 0.36f), TextAnchor.MiddleCenter, Words, Vector2.zero, new Vector2(size, size));
            button.Clock = Picture("Clock", button.Root, disc, Shade, size);
            button.Clock.type = Image.Type.Filled;
            button.Clock.fillMethod = Image.FillMethod.Radial360;
            button.Clock.fillOrigin = (int)Image.Origin360.Top;
            button.Clock.fillClockwise = false;
            button.Clock.fillAmount = 0f;
            // The rim is the clock too: a faint ring all round, and the element's colour filling back round it as the
            // wait runs out, so a waiting slot reads at a glance even where the shade is dark on dark.
            button.RimBack = Picture("Rim Back", button.Root, ring, new Color(1f, 1f, 1f, 0.16f), size * 1.04f);
            button.Rim = Picture("Rim", button.Root, ring, Faint, size * 1.04f);
            button.Rim.type = Image.Type.Filled;
            button.Rim.fillMethod = Image.FillMethod.Radial360;
            button.Rim.fillOrigin = (int)Image.Origin360.Top;
            button.Rim.fillClockwise = true;
            button.Rim.fillAmount = 1f;
            button.Wait = Label("Wait", button.Root, Mathf.RoundToInt(size * 0.42f), TextAnchor.MiddleCenter, Words, Vector2.zero, new Vector2(size, size));
            button.Key = Label("Key", button.Root, 24, TextAnchor.MiddleCenter, Words, keyAt, new Vector2(110f, 28f));
            var keyBack = button.Key.gameObject.AddComponent<Shadow>();
            keyBack.effectColor = new Color(0f, 0f, 0f, 0.9f);
            button.Price = Label("Price", button.Root, 19, TextAnchor.MiddleCenter, ManaBlue, priceAt, new Vector2(80f, 22f));
            button.Name = Label("Name", button.Root, 19, nameAnchor, Words, nameAt, new Vector2(240f, 24f));
            button.Name.gameObject.SetActive(false);
            return button;
        }

        private void OnReady(int clock)
        {
            if (clock >= 0 && clock < Slots) slots[clock].Flash = FlashSeconds;
            else if (clock == SpellCooldowns.StrikeFirst) strikeFirst.Flash = FlashSeconds;
            else if (clock == SpellCooldowns.StrikeSecond) strikeSecond.Flash = FlashSeconds;
        }

        private void LateUpdate()
        {
            if (spells == null || combat == null || element == null) return;
            bool has = element.HasElement && element.Info != null;
            if ((has ? 1 : 0) != shownHas)
            {
                // Nothing to cast without an element: the corner stays clear.
                shownHas = has ? 1 : 0;
                for (int i = 0; i < root.childCount; i++) root.GetChild(i).gameObject.SetActive(has);
                shownBlade = -1;
            }
            if (!has) return;

            bool large = player.Buttons != null && player.Buttons.LayerHeld && !PauseMenuOpen();
            float want = large ? LargeScale : SmallScale;
            if (!Mathf.Approximately(scale, want))
            {
                // Real time, so a hit-stop or the chart's stillness does not hold it half grown.
                scale = Mathf.MoveTowards(scale, want, Mathf.Abs(LargeScale - SmallScale) * Time.unscaledDeltaTime / Mathf.Max(0.01f, GrowSeconds));
                root.localScale = new Vector3(scale, scale, 1f);
            }
            if (large != shownLarge)
            {
                shownLarge = large;
                for (int i = 0; i < Slots; i++) slots[i].Name.gameObject.SetActive(large && slots[i].Name.text.Length > 0);
                surge.Name.gameObject.SetActive(false);
                layerKey.text = large && surgeLine.Length > 0 ? layerLine + "      " + surgeLine : layerLine;
                strikeFirst.Name.gameObject.SetActive(large);
                strikeSecond.Name.gameObject.SetActive(large);
            }

            if (spells.SlotsVersion != shownSlots || ControlDevices.Version != shownDevice || element.Version != shownElement) Rewrite();

            var mana = combat.Mana.Current;
            for (int i = 0; i < Slots; i++)
            {
                var slot = slots[i];
                float price = spells.ManaIn(i);
                Clocked(slot, spells.Cooldowns.Fraction(i), spells.Cooldowns.Remaining(i), price <= 0f || mana >= price);
            }
            Clocked(strikeFirst, spells.Cooldowns.Fraction(SpellCooldowns.StrikeFirst), spells.Cooldowns.Remaining(SpellCooldowns.StrikeFirst), true);
            // R2 asks mana only as the deep strike (all in, or one element); with two elements it is the second's own strike.
            Clocked(strikeSecond, spells.Cooldowns.Fraction(SpellCooldowns.StrikeSecond), spells.Cooldowns.Remaining(SpellCooldowns.StrikeSecond), element.Mixed || mana >= spells.Strikes.DeepMana);

            // The surge the Hand wears: its ring fills while it burns and shades while it waits to be lit again.
            var kit = element.SurgeKit;
            var surgeInfo = element.SurgeInfo;
            float surgeShade = kit == null ? 0f : kit.SurgeWaiting ? kit.SurgeWaitFraction : 0f;
            Clocked(surge, surgeShade, kit != null ? kit.SurgeWaitLeft : 0f, kit == null || kit.SurgeCost <= 0f || kit.SurgeActive || mana >= kit.SurgeCost);
            Color surgeRim = kit != null && kit.SurgeActive ? surgeInfo.Glow : surgeInfo.Colour;
            if (surge.Rim.color != surgeRim) surge.Rim.color = surgeRim;

            ShowBlade();
        }

        private static bool PauseMenuOpen() => PauseMenu.Instance != null && PauseMenu.Instance.IsOpen;

        // One button's clock: the shade over it, the seconds in it, a glow when it came ready, dim when it cannot be paid for.
        private void Clocked(Button button, float fraction, float remaining, bool affordable)
        {
            if (Mathf.Abs(button.ShownFill - fraction) > 0.002f)
            {
                button.ShownFill = fraction;
                button.Clock.fillAmount = fraction;
                button.Rim.fillAmount = 1f - fraction;
            }
            int wait = remaining > 0f ? Mathf.CeilToInt(remaining) : 0;
            if (wait != button.ShownWait)
            {
                button.ShownWait = wait;
                button.Wait.text = wait > 0 ? Seconds[Mathf.Min(wait, Seconds.Length - 1)] : "";
                // The seconds take the face while it waits; the initials come back when it is ready.
                button.Initials.enabled = wait == 0;
            }
            if (affordable != button.Affordable)
            {
                button.Affordable = affordable;
                button.Price.color = affordable ? ManaBlue : Short;
                float alpha = affordable ? 1f : 0.45f;
                button.Icon.color = new Color(button.Icon.color.r, button.Icon.color.g, button.Icon.color.b, alpha);
                button.Initials.color = new Color(Words.r, Words.g, Words.b, alpha);
            }
            if (button.Flash > 0f)
            {
                button.Flash -= Time.unscaledDeltaTime;
                float swell = 1f + 0.18f * Mathf.Max(0f, button.Flash) / FlashSeconds;
                button.Rim.rectTransform.localScale = new Vector3(swell, swell, 1f);
                if (button.Flash <= 0f) button.Rim.rectTransform.localScale = Vector3.one;
            }
        }

        // The pips: one lit for each charged blow still to come, in the element's colour.
        private void ShowBlade()
        {
            var blade = combat.Blade;
            if (blade.Version == shownBlade) return;
            shownBlade = blade.Version;
            var colour = blade.Charged ? Elements.Colour(blade.Element) : Color.clear;
            var glow = blade.Charged && Elements.ByName(blade.Element) != null ? Elements.ByName(blade.Element).Glow : Color.white;
            for (int i = 0; i < pips.Length; i++)
            {
                bool shown = blade.Charged && i < blade.Blows;
                if (pips[i].gameObject.activeSelf != shown) pips[i].gameObject.SetActive(shown);
                if (!shown) continue;
                bool lit = i < blade.Left;
                pips[i].color = lit ? new Color(Mathf.Min(1f, glow.r), Mathf.Min(1f, glow.g), Mathf.Min(1f, glow.b), 1f) : new Color(colour.r * 0.35f, colour.g * 0.35f, colour.b * 0.35f, 0.7f);
            }
        }

        // What every button says: its picture, key, price and name, each in its own element's colour. Only when the
        // slots, the device, the attunements or the surge change.
        private void Rewrite()
        {
            Rewrites++;
            shownSlots = spells.SlotsVersion;
            shownDevice = ControlDevices.Version;
            shownElement = element.Version;
            var info = element.Info;
            var device = ControlDevices.Current;
            bool keys = device == ControlDevice.Keys;
            var menu = UpgradeMenu.Instance;
            for (int i = 0; i < Slots; i++)
            {
                var slot = slots[i];
                string name = spells.NameIn(i);
                var mark = spells.MarkIn(i);
                var own = spells.ElementIn(i) ?? info;
                string icon = mark != null ? mark.Icon : SpellSlots.IsBase(spells.Slots[i]) ? own.Icon : "";
                Dress(slot, name, menu != null ? menu.IconSprite(icon) : null, own.Colour);
                slot.Key.text = ControlNames.Of(LayeredButtons.SlotActions[i], device);
                float price = spells.ManaIn(i);
                slot.Price.text = price > 0f ? Mathf.RoundToInt(price) + " mana" : "";
                slot.Name.text = name;
                slot.Name.gameObject.SetActive(shownLarge && name.Length > 0);
            }
            var kit = element.SurgeKit;
            var surgeInfo = element.SurgeInfo ?? info;
            string surgeName = kit != null ? kit.SurgeName : "";
            Dress(surge, surgeName, null, surgeInfo.Colour);
            string surgePrice = kit != null && kit.SurgeCost > 0f ? ", " + Mathf.RoundToInt(kit.SurgeCost) + " mana" : kit != null && kit.SurgeCooldown > 0f ? ", health" : "";
            surgeLine = surgeName.Length > 0 ? "Surge " + surgeName + ": " + ControlNames.Of("Surge", device) + surgePrice : "";

            // R1 is the first element's strike; R2 the second's, or the first's deep strike all in or with one element.
            bool deepOnSecond = !element.Mixed;
            var secondInfo = deepOnSecond ? info : element.SecondInfo;
            string first = ElementSteel.StrikeName(info.Name, false), second = ElementSteel.StrikeName(secondInfo.Name, deepOnSecond);
            Dress(strikeFirst, first, null, info.Colour);
            Dress(strikeSecond, second, null, secondInfo.Colour);
            string layer = ControlNames.Of("SpellLayer", device);
            strikeFirst.Key.text = keys ? "Ctrl+" + ShortKey(ControlNames.Of("LightAttack", device)) : ControlNames.Of("LightAttack", device);
            strikeSecond.Key.text = keys ? "Ctrl+" + ShortKey(ControlNames.Of("HeavyAttack", device)) : ControlNames.Of("HeavyAttack", device);
            strikeFirst.Price.text = "";
            strikeSecond.Price.text = deepOnSecond ? Mathf.RoundToInt(spells.Strikes.DeepMana) + " mana" : "";
            strikeFirst.Name.fontSize = strikeSecond.Name.fontSize = 17;
            strikeFirst.Name.text = first;
            strikeSecond.Name.text = second;
            // On a pad the slots and strikes are under the held trigger; on the keys only the strikes are.
            layerLine = keys ? "Ctrl: strikes" : "Hold " + layer;
            layerKey.text = shownLarge && surgeLine.Length > 0 ? layerLine + "      " + surgeLine : layerLine;
        }

        // A button's face: its picture where the chart has one, else its initials, ringed in the element's colour.
        private void Dress(Button button, string name, Sprite picture, Color colour)
        {
            bool empty = string.IsNullOrEmpty(name);
            button.Icon.sprite = picture;
            button.Icon.enabled = picture != null;
            button.Icon.color = new Color(colour.r * 0.6f + 0.4f, colour.g * 0.6f + 0.4f, colour.b * 0.6f + 0.4f, button.Affordable ? 1f : 0.45f);
            button.Initials.text = picture == null && !empty ? Initials(name) : "";
            button.Rim.color = empty ? Faint * new Color(1f, 1f, 1f, 0.4f) : colour;
            button.Back.color = empty ? new Color(Ink.r, Ink.g, Ink.b, 0.4f) : Ink;
        }

        private static string ShortKey(string key) => key == "Left mouse" ? "LMB" : key == "Right mouse" ? "RMB" : key;

        /// <summary>The first letters of a name's words, two at most, skipping the small words: "Flame Wave" is "FW", "The Sun on the Blade" "SB".</summary>
        public static string Initials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "";
            var letters = new System.Text.StringBuilder(2);
            foreach (var word in name.Split(' ', '-'))
            {
                if (word.Length == 0 || word == "the" || word == "The" || word == "of" || word == "on" || word == "a" || word == "to" || word == "Into" || word == "into") continue;
                letters.Append(char.ToUpperInvariant(word[0]));
                if (letters.Length == 2) break;
            }
            return letters.Length > 0 ? letters.ToString() : name.Substring(0, 1).ToUpperInvariant();
        }

        private static string[] MakeSeconds(int count)
        {
            var made = new string[count + 1];
            for (int i = 0; i <= count; i++) made[i] = i.ToString();
            return made;
        }

        // A soft-edged white disc, or a ring of it with the given share of the middle left out, drawn once.
        private static Sprite MakeCircle(out Texture2D texture, float hole)
        {
            const int Size = 128;
            texture = new Texture2D(Size, Size, TextureFormat.RGBA32, false) { name = hole > 0f ? "Diamond Ring" : "Diamond Disc", wrapMode = TextureWrapMode.Clamp, filterMode = FilterMode.Bilinear };
            var pixels = new Color32[Size * Size];
            float half = Size * 0.5f;
            for (int y = 0; y < Size; y++)
            {
                for (int x = 0; x < Size; x++)
                {
                    float distance = Mathf.Sqrt((x + 0.5f - half) * (x + 0.5f - half) + (y + 0.5f - half) * (y + 0.5f - half)) / half;
                    float alpha = Mathf.Clamp01((1f - distance) * half * 0.5f);
                    if (hole > 0f) alpha *= Mathf.Clamp01((distance - hole) * half * 0.5f);
                    pixels[y * Size + x] = new Color32(255, 255, 255, (byte)(alpha * 255f));
                }
            }
            texture.SetPixels32(pixels);
            texture.Apply(false, true);
            return Sprite.Create(texture, new Rect(0f, 0f, Size, Size), new Vector2(0.5f, 0.5f), 100f);
        }

        private RectTransform Rect(string name, Transform parent, Vector2 at, Vector2 size)
        {
            var made = new GameObject(name, typeof(RectTransform));
            made.transform.SetParent(parent, false);
            var rect = (RectTransform)made.transform;
            // Children of a button are placed from its middle; the buttons themselves from the root's bottom-right.
            bool onRoot = parent == root;
            rect.anchorMin = rect.anchorMax = onRoot ? new Vector2(1f, 0f) : new Vector2(0.5f, 0.5f);
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.anchoredPosition = at;
            rect.sizeDelta = size;
            return rect;
        }

        private Image Picture(string name, Transform parent, Sprite sprite, Color colour, float size)
        {
            var rect = Rect(name, parent, Vector2.zero, new Vector2(size, size));
            var image = rect.gameObject.AddComponent<Image>();
            image.sprite = sprite;
            image.color = colour;
            image.raycastTarget = false;
            image.preserveAspect = true;
            return image;
        }

        private Text Label(string name, Transform parent, int size, TextAnchor anchor, Color colour, Vector2 at, Vector2 box)
        {
            var rect = Rect(name, parent, at, box);
            var label = rect.gameObject.AddComponent<Text>();
            label.font = font;
            label.fontSize = size;
            label.alignment = anchor;
            label.color = colour;
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            var outline = rect.gameObject.AddComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.85f);
            outline.effectDistance = new Vector2(1.5f, -1.5f);
            return label;
        }
    }
}

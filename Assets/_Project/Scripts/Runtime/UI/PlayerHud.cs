using Clover.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// Shows the player's health (with the grey part that can be won back), stamina, mana and flasks, and puts a word
    /// or two on screen for a moment when something worth knowing happens, such as a perfect guard.
    /// </summary>
    public class PlayerHud : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private PlayerDefence defence;
        [SerializeField] private RectTransform healthFill;
        [SerializeField] private RectTransform greyFill;
        [SerializeField] private RectTransform staminaFill;
        [SerializeField] private RectTransform manaFill;
        [SerializeField] private RectTransform[] flaskFills;
        [SerializeField] private Text callout;
        [SerializeField] private Text status;
        [SerializeField] private PlayerElement element;
        [SerializeField] private Text prompt;
        [SerializeField] private PlayerInteraction interaction;
        [SerializeField] private PlayerUpgrades upgrades;
        [SerializeField] private Text embers;
        [SerializeField] private GameObject heatRow;
        [SerializeField] private RectTransform[] heatPips = new RectTransform[0];
        [SerializeField] private RectTransform healthLag;
        [SerializeField] private Image manaImage;

        /// <summary>Real seconds a lost piece of health stays shown, pale, before it drains away, and how fast it then drains.</summary>
        public float LagHold = 0.45f;
        public float LagDrainPerSecond = 0.9f;
        public Color FireMana = new Color(1f, 0.5f, 0.1f);
        public Color LightningMana = new Color(0.35f, 0.7f, 1f);

        /// <summary>Real seconds a callout stays up, the last part of it fading.</summary>
        public float CalloutSeconds = 0.9f;

        private int calloutsShown;
        private Weapon shownWeapon;
        private int shownSurge = -1;
        private Clover.World.Interactable shownThing;
        private float calloutLeft;
        private int shownEmbers = -1;
        private Image[] flaskImages;
        private float lagFraction = 1f;
        private float lagHeld;
        private string shownElement;

        /// <summary>Adds the pale piece that lingers where health was just lost, and the mana bar's own image so it can take the element's colour.</summary>
        public void ConfigureJuice(RectTransform healthLagBar, Image manaBarImage)
        {
            healthLag = healthLagBar;
            manaImage = manaBarImage;
        }
        private bool shownCharge;

        /// <summary>Adds what the element's tree shows: the embers held, and Heat's levels while Heat is learned.</summary>
        public void ConfigureUpgrades(PlayerUpgrades upgradesToShow, Text embersText, GameObject heatRowRoot, RectTransform[] heatPipFills)
        {
            upgrades = upgradesToShow;
            embers = embersText;
            heatRow = heatRowRoot;
            heatPips = heatPipFills;
        }

        public void Configure(PlayerController playerToShow, PlayerCombat combatToShow, PlayerDefence defenceToShow,
            RectTransform healthBar, RectTransform greyBar, RectTransform staminaBar, RectTransform manaBar,
            RectTransform[] flaskBars, Text calloutText, Text statusText, Text promptText)
        {
            player = playerToShow;
            combat = combatToShow;
            defence = defenceToShow;
            healthFill = healthBar;
            greyFill = greyBar;
            staminaFill = staminaBar;
            manaFill = manaBar;
            flaskFills = flaskBars;
            callout = calloutText;
            status = statusText;
            prompt = promptText;
            interaction = playerToShow != null ? playerToShow.GetComponent<PlayerInteraction>() : null;
            element = playerToShow != null ? playerToShow.GetComponent<PlayerElement>() : null;
        }

        private void Start()
        {
            if (defence != null) calloutsShown = defence.Callouts;
            if (callout != null) callout.enabled = false;
            if (prompt != null) prompt.enabled = false;
            // A flask drawn as a shape is filled by its image; a plain bar is filled by its size.
            flaskImages = new Image[flaskFills != null ? flaskFills.Length : 0];
            for (int i = 0; i < flaskImages.Length; i++)
            {
                var image = flaskFills[i] != null ? flaskFills[i].GetComponent<Image>() : null;
                flaskImages[i] = image != null && image.type == Image.Type.Filled ? image : null;
            }
        }

        private void LateUpdate()
        {
            if (player != null && staminaFill != null) staminaFill.anchorMax = new Vector2(player.Stamina.Fraction, 1f);
            if (combat != null && manaFill != null) manaFill.anchorMax = new Vector2(combat.Mana.Fraction, 1f);
            if (defence == null) return;

            if (healthFill != null) healthFill.anchorMax = new Vector2(defence.Health.Fraction, 1f);
            if (greyFill != null) greyFill.anchorMax = new Vector2(defence.Health.GreyFraction, 1f);
            if (healthLag != null)
            {
                // What was just lost stays a moment as a pale piece, then drains: the size of the blow can be read.
                float now = Mathf.Max(defence.Health.Fraction, defence.Health.GreyFraction);
                if (now >= lagFraction)
                {
                    lagFraction = now;
                    lagHeld = LagHold;
                }
                else if (lagHeld > 0f) lagHeld -= Time.unscaledDeltaTime;
                else lagFraction = Mathf.MoveTowards(lagFraction, now, LagDrainPerSecond * Time.unscaledDeltaTime);
                healthLag.anchorMax = new Vector2(lagFraction, 1f);
            }
            if (manaImage != null && element != null && element.Kit != null && element.Kit.Name != shownElement)
            {
                shownElement = element.Kit.Name;
                manaImage.color = shownElement == ElementKit.LightningName ? LightningMana : FireMana;
            }
            if (flaskFills != null)
            {
                for (int i = 0; i < flaskFills.Length; i++)
                {
                    if (flaskFills[i] == null) continue;
                    // With none left, the first flask fills slowly as the player earns it back.
                    float full = i < defence.Flasks.Count ? 1f : i == 0 ? defence.Flasks.RechargeFraction : 0f;
                    if (flaskImages != null && i < flaskImages.Length && flaskImages[i] != null) flaskImages[i].fillAmount = full;
                    else flaskFills[i].anchorMax = new Vector2(1f, full);
                }
            }

            // The weapon in hand and, while it lasts, the surge with its seconds. Rewritten only when it changes.
            if (status != null && combat != null && combat.Weapon != null)
            {
                int surge = element != null && element.Kit.SurgeActive ? Mathf.CeilToInt(element.Kit.SurgeLeft) : 0;
                bool charge = upgrades != null && upgrades.ChargeStored;
                if (combat.Weapon != shownWeapon || surge != shownSurge || charge != shownCharge)
                {
                    shownWeapon = combat.Weapon;
                    shownSurge = surge;
                    shownCharge = charge;
                    string text = surge > 0 ? $"{shownWeapon.Name}   {element.Kit.SurgeName} {surge}" : shownWeapon.Name;
                    status.text = charge ? text + "   Charge stored" : text;
                }
            }

            if (upgrades != null)
            {
                if (embers != null && upgrades.Points != shownEmbers)
                {
                    shownEmbers = upgrades.Points;
                    embers.text = $"Embers  {shownEmbers}";
                }
                if (heatRow != null)
                {
                    bool show = upgrades.ShowsHeat;
                    if (heatRow.activeSelf != show) heatRow.SetActive(show);
                    if (show)
                    {
                        for (int i = 0; i < heatPips.Length; i++)
                        {
                            if (heatPips[i] == null) continue;
                            float full = i < upgrades.Heat.Level ? 1f : i == upgrades.Heat.Level ? upgrades.Heat.Progress : 0f;
                            heatPips[i].anchorMax = new Vector2(full, 1f);
                        }
                    }
                }
            }

            // What the interact button would do here, while the player stands by something that can be used.
            if (prompt != null && interaction != null && interaction.Current != shownThing)
            {
                shownThing = interaction.Current;
                prompt.enabled = shownThing != null;
                if (shownThing != null) prompt.text = shownThing.Prompt;
            }

            if (callout == null) return;
            if (defence.Callouts != calloutsShown)
            {
                calloutsShown = defence.Callouts;
                callout.text = defence.LastCallout;
                callout.enabled = true;
                calloutLeft = CalloutSeconds;
            }
            if (calloutLeft > 0f)
            {
                // Real time, so the word is not held on screen by the stop that comes with it.
                calloutLeft -= Time.unscaledDeltaTime;
                var colour = callout.color;
                colour.a = Mathf.Clamp01(calloutLeft / (CalloutSeconds * 0.35f));
                callout.color = colour;
                if (calloutLeft <= 0f) callout.enabled = false;
            }
        }
    }
}

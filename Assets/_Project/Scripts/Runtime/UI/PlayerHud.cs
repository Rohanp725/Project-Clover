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

        /// <summary>Real seconds a callout stays up, the last part of it fading.</summary>
        public float CalloutSeconds = 0.9f;

        private int calloutsShown;
        private Weapon shownWeapon;
        private int shownSurge = -1;
        private Clover.World.Interactable shownThing;
        private float calloutLeft;

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
        }

        private void LateUpdate()
        {
            if (player != null && staminaFill != null) staminaFill.anchorMax = new Vector2(player.Stamina.Fraction, 1f);
            if (combat != null && manaFill != null) manaFill.anchorMax = new Vector2(combat.Mana.Fraction, 1f);
            if (defence == null) return;

            if (healthFill != null) healthFill.anchorMax = new Vector2(defence.Health.Fraction, 1f);
            if (greyFill != null) greyFill.anchorMax = new Vector2(defence.Health.GreyFraction, 1f);
            if (flaskFills != null)
            {
                for (int i = 0; i < flaskFills.Length; i++)
                {
                    if (flaskFills[i] == null) continue;
                    // With none left, the first flask fills slowly as the player earns it back.
                    float full = i < defence.Flasks.Count ? 1f : i == 0 ? defence.Flasks.RechargeFraction : 0f;
                    flaskFills[i].anchorMax = new Vector2(1f, full);
                }
            }

            // The weapon in hand and, while it lasts, the surge with its seconds. Rewritten only when it changes.
            if (status != null && combat != null && combat.Weapon != null)
            {
                int surge = element != null && element.Kit.SurgeActive ? Mathf.CeilToInt(element.Kit.SurgeLeft) : 0;
                if (combat.Weapon != shownWeapon || surge != shownSurge)
                {
                    shownWeapon = combat.Weapon;
                    shownSurge = surge;
                    status.text = surge > 0 ? $"{shownWeapon.Name}   {element.Kit.SurgeName} {surge}" : shownWeapon.Name;
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

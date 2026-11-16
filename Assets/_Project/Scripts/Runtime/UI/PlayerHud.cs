using Clover.Player;
using UnityEngine;

namespace Clover.UI
{
    /// <summary>Shows the player's stamina and mana by stretching each bar across its background.</summary>
    public class PlayerHud : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private RectTransform staminaFill;
        [SerializeField] private RectTransform manaFill;

        public void Configure(PlayerController playerToShow, PlayerCombat combatToShow, RectTransform staminaBar, RectTransform manaBar)
        {
            player = playerToShow;
            combat = combatToShow;
            staminaFill = staminaBar;
            manaFill = manaBar;
        }

        private void LateUpdate()
        {
            if (player != null && staminaFill != null) staminaFill.anchorMax = new Vector2(player.Stamina.Fraction, 1f);
            if (combat != null && manaFill != null) manaFill.anchorMax = new Vector2(combat.Mana.Fraction, 1f);
        }
    }
}

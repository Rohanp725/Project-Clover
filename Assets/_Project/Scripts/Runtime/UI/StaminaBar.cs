using Clover.Player;
using UnityEngine;

namespace Clover.UI
{
    /// <summary>Shows the player's stamina by stretching a bar across its background.</summary>
    public class StaminaBar : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private RectTransform fill;

        public void Configure(PlayerController playerToShow, RectTransform fillRect)
        {
            player = playerToShow;
            fill = fillRect;
        }

        private void LateUpdate()
        {
            if (player == null || fill == null) return;
            fill.anchorMax = new Vector2(player.Stamina.Fraction, 1f);
        }
    }
}

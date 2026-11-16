using Clover.Gameplay;
using UnityEngine;

namespace Clover.UI
{
    /// <summary>
    /// A boss's name and bars across the bottom of the screen: its health, and under it the balance that the player is
    /// trying to break. They appear when the fight begins and go when it is over.
    /// </summary>
    public class BossHud : MonoBehaviour
    {
        [SerializeField] private Enemy boss;
        [SerializeField] private GameObject panel;
        [SerializeField] private RectTransform healthFill;
        [SerializeField] private RectTransform staggerFill;

        private bool shown;

        public void Configure(Enemy bossToShow, GameObject panelToShow, RectTransform healthBar, RectTransform staggerBar)
        {
            boss = bossToShow;
            panel = panelToShow;
            healthFill = healthBar;
            staggerFill = staggerBar;
        }

        private void Start()
        {
            if (panel != null) panel.SetActive(false);
        }

        private void LateUpdate()
        {
            if (boss == null || panel == null) return;
            bool show = boss.IsAware && !boss.IsDead;
            if (show != shown)
            {
                shown = show;
                panel.SetActive(show);
            }
            if (!show) return;
            if (healthFill != null) healthFill.anchorMax = new Vector2(boss.Health.Fraction, 1f);
            // The balance bar grows from the middle outward, and stands full while he is broken open.
            if (staggerFill != null)
            {
                float half = (boss.IsStaggered ? 1f : boss.Stagger.Fraction) * 0.5f;
                staggerFill.anchorMin = new Vector2(0.5f - half, 0f);
                staggerFill.anchorMax = new Vector2(0.5f + half, 1f);
            }
        }
    }
}

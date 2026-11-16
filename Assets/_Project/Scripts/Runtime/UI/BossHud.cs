using Clover.Gameplay;
using UnityEngine;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// The boss's name, health and balance across the bottom of the screen. It appears when the fight begins and goes
    /// when the boss falls. A boss with a second form is shown in turn: the bar is handed the new one, with its name.
    /// </summary>
    public class BossHud : MonoBehaviour
    {
        // Unity cannot keep a reference to an interface, so the boss is kept as the component it is.
        [SerializeField] private MonoBehaviour boss;
        [SerializeField] private GameObject panel;
        [SerializeField] private RectTransform healthFill;
        [SerializeField] private RectTransform staggerFill;
        [SerializeField] private Text nameLabel;

        private IBoss shownBoss;
        private bool shown;

        public void Configure(MonoBehaviour bossToShow, GameObject panelToShow, RectTransform healthBar, RectTransform staggerBar, Text label)
        {
            boss = bossToShow;
            panel = panelToShow;
            healthFill = healthBar;
            staggerFill = staggerBar;
            nameLabel = label;
        }

        private void Awake() => shownBoss = boss as IBoss;

        private void Start()
        {
            if (panel != null) panel.SetActive(false);
        }

        /// <summary>Hands the bar another boss to show, under another name.</summary>
        public void Show(IBoss next, string name)
        {
            shownBoss = next;
            if (nameLabel != null) nameLabel.text = name;
        }

        private void LateUpdate()
        {
            if (shownBoss == null || panel == null) return;
            bool show = shownBoss.InFight;
            if (show != shown)
            {
                shown = show;
                panel.SetActive(show);
            }
            if (!show) return;
            if (healthFill != null) healthFill.anchorMax = new Vector2(shownBoss.HealthFraction, 1f);
            // The balance bar grows from the middle outward, and stands full while he is broken open.
            if (staggerFill != null)
            {
                float half = shownBoss.BalanceFraction * 0.5f;
                staggerFill.anchorMin = new Vector2(0.5f - half, 0f);
                staggerFill.anchorMax = new Vector2(0.5f + half, 1f);
            }
        }
    }
}

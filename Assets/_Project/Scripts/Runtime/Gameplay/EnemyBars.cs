using Clover.Core;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// What the bars over a fighter do beyond showing two numbers: a pale piece lingers where health was just lost and
    /// then drains, and the balance bar is put away while it is empty so that the bars of a crowd do not pile up.
    /// The fighter itself still sets the length of the two bars; this reads them and does the rest.
    /// </summary>
    public class EnemyBars : MonoBehaviour
    {
        [SerializeField] private Transform health;
        [SerializeField] private Transform lag;
        [SerializeField] private Transform balance;
        [SerializeField] private GameObject balanceRow;

        public LagMeter Lag { get; } = new LagMeter();

        public void Configure(Transform healthFill, Transform lagFill, Transform balanceFill, GameObject balanceRowRoot)
        {
            health = healthFill;
            lag = lagFill;
            balance = balanceFill;
            balanceRow = balanceRowRoot;
        }

        // Shown again after being put away: the pale piece starts where the bar is, not where it was last seen.
        private void OnEnable()
        {
            if (health != null) Lag.Snap(health.localScale.x);
        }

        private void LateUpdate()
        {
            if (health != null && lag != null)
                lag.localScale = new Vector3(Lag.Tick(health.localScale.x, Time.unscaledDeltaTime), 1f, 1f);
            if (balance != null && balanceRow != null)
            {
                bool some = balance.localScale.x > 0.004f;
                if (balanceRow.activeSelf != some) balanceRow.SetActive(some);
            }
        }
    }
}

using System.Collections.Generic;
using Clover.Core;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// What the bars over a fighter do beyond showing two numbers: a pale piece lingers where health was just lost and
    /// then drains, and the balance bar is put away while it is empty so that the bars of a crowd do not pile up.
    /// The fighter itself still sets the length of the two bars; this reads them and does the rest. It also keeps the
    /// bars readable: clear of a neighbour's bars, and out from under the player's own bars in the screen's corner.
    /// </summary>
    public class EnemyBars : MonoBehaviour
    {
        [SerializeField] private Transform health;
        [SerializeField] private Transform lag;
        [SerializeField] private Transform balance;
        [SerializeField] private GameObject balanceRow;

        /// <summary>How much room, in metres, these bars need across and upward; a name over them makes them taller.</summary>
        public float Wide = 1.55f;
        public float Tall = 0.46f;
        /// <summary>The corner of the screen the player's own bars fill, as shares of its width and height.</summary>
        public float HudRight = 0.33f;
        public float HudBottom = 0.74f;
        /// <summary>Bars are kept below this share of the screen's height, so a fighter at the top edge still shows them.</summary>
        public float ScreenTop = 0.93f;
        /// <summary>Metres a second the bars slide when they make room.</summary>
        public float SlideSpeed = 4f;

        public LagMeter Lag { get; } = new LagMeter();

        // Every set of bars now showing, so each can keep clear of those before it.
        private static readonly List<EnemyBars> showing = new List<EnemyBars>();

        private Camera view;
        private float restHeight;
        private float shift;
        private bool measured;

        // Domain reload is off: the bars of the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => showing.Clear();

        public void Configure(Transform healthFill, Transform lagFill, Transform balanceFill, GameObject balanceRowRoot)
        {
            health = healthFill;
            lag = lagFill;
            balance = balanceFill;
            balanceRow = balanceRowRoot;
        }

        private void OnEnable()
        {
            if (!measured)
            {
                measured = true;
                restHeight = transform.localPosition.y;
            }
            if (view == null) view = Camera.main;
            shift = 0f;
            showing.Add(this);
            // Shown again after being put away: the pale piece starts where the bar is, not where it was last seen.
            if (health != null) Lag.Snap(health.localScale.x);
        }

        private void OnDisable() => showing.Remove(this);

        private void LateUpdate()
        {
            if (health != null && lag != null)
                lag.localScale = new Vector3(Lag.Tick(health.localScale.x, Time.unscaledDeltaTime), 1f, 1f);
            if (balance != null && balanceRow != null)
            {
                bool some = balance.localScale.x > 0.004f;
                if (balanceRow.activeSelf != some) balanceRow.SetActive(some);
            }
            MakeRoom();
        }

        // The bars face a camera that never turns, so "across" and "up" on the screen are the bars' own right and up.
        private void MakeRoom()
        {
            Transform owner = transform.parent;
            if (owner == null) return;
            Vector3 rest = owner.position + Vector3.up * restHeight;
            Vector3 up = transform.up, right = transform.right;
            float wanted = 0f;
            // Twice over: stepping clear of one neighbour may step onto another.
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < showing.Count; i++)
                {
                    var other = showing[i];
                    // Only those before this one are stepped over, so two neighbours do not both move.
                    if (other == this) break;
                    Vector3 between = rest + up * wanted - other.transform.position;
                    float across = Vector3.Dot(between, right), above = Vector3.Dot(between, up);
                    float needed = (Tall + other.Tall) * 0.5f;
                    if (Mathf.Abs(across) < (Wide + other.Wide) * 0.5f && Mathf.Abs(above) < needed) wanted += needed - above;
                }
            }
            if (view != null)
            {
                Vector3 seen = view.WorldToViewportPoint(rest + up * wanted);
                // Under the player's own bars nothing can be read, and off the top of the screen nothing is seen: the bars
                // come down to sit just below that corner, or just inside the edge.
                float ceiling = seen.x < HudRight ? HudBottom : ScreenTop;
                if (seen.z > 0f && seen.y > ceiling)
                    wanted -= (seen.y - ceiling) * 2f * seen.z * Mathf.Tan(view.fieldOfView * 0.5f * Mathf.Deg2Rad);
            }
            shift = Mathf.MoveTowards(shift, wanted, SlideSpeed * Time.unscaledDeltaTime);
            transform.position = rest + up * shift;
        }
    }
}

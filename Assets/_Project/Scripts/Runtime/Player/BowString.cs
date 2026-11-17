using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The Great Bow's string. Undrawn it is the straight bar the model carries. While the bearer draws (the charge,
    /// or the wind-up of a shot) the bar is hidden and a two-part line runs from tip to tip through the right hand.
    /// On a stand, or on the model the Forge poses, there is no bearer and the bar is all there is.
    /// </summary>
    public class BowString : MonoBehaviour
    {
        [SerializeField] private LineRenderer line;
        [SerializeField] private Transform upper, lower;
        [SerializeField] private Renderer bar;

        private PlayerCombat combat;
        private Transform hand;

        public void Configure(LineRenderer drawnLine, Transform upperTip, Transform lowerTip, Renderer restBar)
        {
            line = drawnLine;
            upper = upperTip;
            lower = lowerTip;
            bar = restBar;
        }

        private void Awake()
        {
            combat = GetComponentInParent<PlayerCombat>();
            if (combat != null) hand = WeaponRack.FindSocket(combat.transform, WeaponRack.HandRight);
            if (line != null) line.enabled = false;
        }

        private void LateUpdate()
        {
            if (line == null) return;
            bool drawn = combat != null && hand != null && upper != null && lower != null && combat.StringDrawn;
            if (line.enabled != drawn)
            {
                line.enabled = drawn;
                if (bar != null) bar.enabled = !drawn;
            }
            if (!drawn) return;
            line.SetPosition(0, upper.position);
            line.SetPosition(1, hand.position);
            line.SetPosition(2, lower.position);
        }
    }
}

namespace Clover.Player.Way
{
    /// <summary>
    /// The Body's rules that are more than a number (way_plan.md 0.4; task 5.4), plain so they are tested without a player:
    /// Steady's poise in a swing and outside one, and the Light Body's flow from one dodge into a second. The numbers
    /// themselves are stats on the marks (<see cref="Trees.Ways.BodyWay"/>).
    /// </summary>
    public static class BodyRules
    {
        /// <summary>The Light Body: a dodge pressed in this last part of a dodge flows into a second.</summary>
        public const float FlowWindow = 0.1f;
        /// <summary>The flowing dodge's stamina, as a share of the usual.</summary>
        public const float FlowCostShare = 0.5f;

        /// <summary>True when a swing with this much poise goes on through a blow of this much damage (it still hurts).</summary>
        public static bool Poised(float poise, float damage) => poise > 0f && damage <= poise;

        /// <summary>
        /// Steady outside a swing: true when a landed blow of this much damage does not make the player flinch, with this
        /// much of the Body's poise (6 with Steady, none without). In a swing the swing's poise decides (<see cref="Poised"/>).
        /// </summary>
        public static bool ShrugsOff(bool inSwing, float steadiness, float damage) => !inSwing && Poised(steadiness, damage);

        /// <summary>
        /// The Light Body: true when a dodge pressed <paramref name="pressedAgo"/> seconds ago may flow on from the one under
        /// way: flows are allowed, this dodge is not itself a flow (never a third), and the press came in the dodge's last
        /// <see cref="FlowWindow"/>.
        /// </summary>
        public static bool MayFlow(int flows, bool rolling, bool alreadyFlowed, float duration, float elapsed, float pressedAgo)
        {
            if (flows <= 0 || !rolling || alreadyFlowed) return false;
            float leftAtPress = duration - (elapsed - pressedAgo);
            return leftAtPress <= FlowWindow + 1e-5f && duration - elapsed <= FlowWindow + 1e-5f;
        }
    }
}

namespace Clover.Gameplay
{
    /// <summary>
    /// What a mark asks of an enemy condition (the action <c>UpgradeAction.Condition</c>): its name, how long it lasts,
    /// its beat, the states it puts the enemy in, and its numbers. Plain data, written in the tree files.
    /// </summary>
    public class ConditionSpec
    {
        /// <summary>Its name: the effects that answer its beat, a blow on it and its end name it (<c>UpgradeEffect.Condition</c>).</summary>
        public string Id = "";
        public float Seconds = 4f;
        /// <summary>Seconds between beats; zero for none.</summary>
        public float Beat;
        public ConditionFlags Flags;
        /// <summary>The share of its speed it keeps while this lasts: 1 unchanged, 0.5 half as fast. Elites take half the slowing, gods none.</summary>
        public float Pace = 1f;
        /// <summary>The run's one number, read by whichever flag needs one: the wind-up's pace, the share of damage taken or dealt, or the share a link passes on.</summary>
        public float Scale = 1f;
        /// <summary>For <see cref="ConditionFlags.SightShort"/>: how far it sees, in metres.</summary>
        public float Sight;
        /// <summary>Set on the enemies near the trigger's enemy, not on it; each is joined to it (Rot Roots).</summary>
        public bool ToOthers;
    }
}

namespace Clover.Player
{
    /// <summary>For an effect on <see cref="UpgradeTrigger.ConditionStruck"/>: which blows it answers.</summary>
    public enum StruckBy
    {
        /// <summary>Any blow, of the weapon or a spell.</summary>
        Any,
        /// <summary>A blow of the weapon only.</summary>
        Weapon,
        /// <summary>A heavy blow only (a finisher counts).</summary>
        Heavy,
        /// <summary>A blow that broke the ice of a frozen enemy (the shatter ends its Chill, so Needs cannot ask for it).</summary>
        Shatter,
    }
}

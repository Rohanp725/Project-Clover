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
        /// <summary>A weapon blow that was a critical hit (Blistered's weals).</summary>
        Critical,
        /// <summary>The finisher's own blow, apart from any other heavy blow (the ledger's finisher-only).</summary>
        Finisher,
        /// <summary>For ConditionAttacked: its carrier's blow turned by a perfect guard, not one that landed or was plainly blocked (Snapped Blade).</summary>
        Turned,
        /// <summary>For ConditionEnded: only an end the enemy's freezing brought (<see cref="Clover.Gameplay.ConditionSpec.EndsOnFreeze"/>).</summary>
        Frozen,
        /// <summary>A shove or a throw (a Gust, a ram, Riptide, a lift), or a heavy weapon blow, which shoves (the ledger's shove-moment).</summary>
        Shove,
    }
}

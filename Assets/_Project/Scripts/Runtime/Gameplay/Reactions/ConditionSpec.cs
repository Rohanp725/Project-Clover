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

        /// <summary>A copy with every number (a mark that changes a reaction's own numbers changes its copy, never this).</summary>
        public ConditionSpec Copy() => (ConditionSpec)MemberwiseClone();
        public float Seconds = 4f;
        /// <summary>Seconds between beats; zero for none.</summary>
        public float Beat;
        public ConditionFlags Flags;
        /// <summary>The share of its speed it keeps while this lasts: 1 unchanged, 0.5 half as fast. Elites take half the slowing, gods none.</summary>
        public float Pace = 1f;
        /// <summary>The run's one number, read by whichever flag needs one: the wind-up's pace, the share of damage or stagger taken or dealt, or the share a link passes on.</summary>
        public float Scale = 1f;
        /// <summary>For <see cref="ConditionFlags.SightShort"/>: how far it sees, in metres.</summary>
        public float Sight;
        /// <summary>Set on the enemies near the trigger's enemy, not on it; each is joined to it (Rot Roots).</summary>
        public bool ToOthers;

        // Shared by the reactions of task 3.4 (docs/design/steel/content_open.tsv says which use each).
        /// <summary>While it runs, this affliction on the enemy neither fades nor is spent by blows (doses that stop fading, notches that stay).</summary>
        public AfflictionKind Keeps = AfflictionKind.None;
        /// <summary>While it runs, this affliction can be laid <see cref="DeepensBy"/> deeper than its usual depth (Rust's notches past three, up to five).</summary>
        public AfflictionKind Deepens = AfflictionKind.None;
        /// <summary>
        /// Above zero, its slow is this share for each stack counted when it is set (Cairn's 15% a stone), in place of
        /// <see cref="Pace"/>; set by an effect marked PerStack. Never slower than 5% of its pace.
        /// </summary>
        public float SlowPerStack;
        /// <summary>While it runs, this affliction's bites on the enemy are <see cref="BiteScale"/> as hard (Red Hot's burns twice as hard while it glows).</summary>
        public AfflictionKind BitesHarder = AfflictionKind.None;
        public float BiteScale = 1f;
        /// <summary>The harder bite only while the enemy is frozen (Cold Damage).</summary>
        public bool BitesHarderFrozen;
        /// <summary>
        /// While it runs, this affliction's bites are held back, not dealt; when nothing holds them any more every bite held
        /// lands at once, <see cref="HeldScale"/> as hard (Burning Sap: poison held 3 s, then landed doubled).
        /// </summary>
        public AfflictionKind HoldsBites = AfflictionKind.None;
        public float HeldScale = 1f;
        public int DeepensBy;
        /// <summary>It lasts only while the enemy carries this (and <see cref="While2"/>): once one is gone it ends, and its end is told ("while both last").</summary>
        public AfflictionKind While = AfflictionKind.None;
        public AfflictionKind While2 = AfflictionKind.None;
        /// <summary>Its beat is the poison biting the enemy, not a clock ("each bite jolts it").</summary>
        public bool OnBite;
        /// <summary>The blow that spends it: answered, then it ends without its end being told ("the next heavy blow").</summary>
        public ConditionBlow EndsWith;
        /// <summary>It ends once it has taken this many weapon blows, after answering the last (Logan Stone's third); 0 for no limit.</summary>
        public int Blows;
    }
}

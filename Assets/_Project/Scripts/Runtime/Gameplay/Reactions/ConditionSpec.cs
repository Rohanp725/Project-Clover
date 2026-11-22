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
        /// <summary>While it runs, this affliction beats <see cref="QuickenBy"/> times as fast; its fading is not hastened (Ichor's bleeding: poison every half second).</summary>
        public AfflictionKind Quickens = AfflictionKind.None;
        public float QuickenBy = 1f;
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
        /// <summary>Its Blows are the most: it is set with one blow for each stack its effect counted, Blows at most (Iron Thorns: a thorn a notch, three at most).</summary>
        public bool BlowsByStacks;
        /// <summary>
        /// What it is, when a shape sets it under a name of its own: the plain reaction's name for the same state (Hot
        /// Shards' crack is a <c>thermalshock.cracked</c>). A mark that asks for an enemy in a condition another mark set
        /// (<c>UpgradeEffect.InState</c>, the ledger's condition-needs) finds it by either name.
        /// </summary>
        public string Is = "";
        /// <summary>It lasts only while the enemy is in this condition (by name or by what it is): once that ends, so does this, and its end is told.</summary>
        public string WhileIn = "";
        /// <summary>
        /// Only critical weapon blows count toward it, are answered, and spend it; any other blow passes as if it were not
        /// there (Kindling Point's shapes count only critical blows; Shining Herb's leaf is picked only by one).
        /// </summary>
        public bool CriticalOnly;
        /// <summary>
        /// The side of a blow on it is read against the way the enemy faced as it was set, not as it faces now (Rime's crust
        /// grows on the side that faced you when it reacted).
        /// </summary>
        public bool KeepsFacing;
        /// <summary>While it runs, a freeze on the enemy lasts this many seconds longer (Rime Tree: 1).</summary>
        public float FreezeLonger;
        /// <summary>While it runs, a shatter of the enemy strikes this many times as hard (Rime Tree: 1.25); 0 or 1 as written.</summary>
        public float ShatterHarder;
        /// <summary>
        /// It ends at once when the enemy freezes, and its end is told then as an end the freeze brought (the ledger's
        /// freeze-moment: Sleet Charge's bolt "at once if it freezes"; Stolen Warmth's last warmth torn out).
        /// </summary>
        public bool EndsOnFreeze;
        /// <summary>
        /// Above zero: when its carrier dies, it drifts whole (its time, beats and blows) to the nearest other enemy within
        /// this many metres (the ledger's drift-on-death: The Grudge's black cloud, Ash Plume's storm).
        /// </summary>
        public float DriftsTo;
        /// <summary>
        /// Above zero: a death of another enemy within this many metres of its carrier is told to it, as a blow of its own kind
        /// carrying the dead one's doses (StruckBy.DeathNear; the ledger's jar-deaths: Shut in the Dark's ring, 4 m across).
        /// </summary>
        public float DeathsWithin;
        /// <summary>Metres its body is drawn down into the floor while it runs, for the look (The Ground Gives' sand: to the knees).</summary>
        public float Sink;
        /// <summary>
        /// With <see cref="ConditionFlags.FacesPoint"/>: it is turned to face away from the player (Turned About), not toward
        /// the middle of the spell that set it (Turned to the Sun's herb).
        /// </summary>
        public bool FaceAway;
        /// <summary>Above zero: the seconds it lasts on an elite or boss, in place of the usual share (Turned About: 0.5 of 1.2).</summary>
        public float EliteSeconds;
        /// <summary>
        /// Above zero: metres it may go from what holds it (the far end of its chain, the player, or the spot it was set on)
        /// while this runs; never on a god (Black Chain: 4; Tethered: 5 from the player).
        /// </summary>
        public float Leash;
        /// <summary>With <see cref="Leash"/>: the player holds the leash (Grasp's Tethered).</summary>
        public bool LeashToPlayer;
        /// <summary>
        /// Above zero, for a chain set on others (<see cref="ToOthers"/>): with no one near to chain it to, the enemy is
        /// chained to the floor within this many metres of where it stands instead (Black Chain: 2).
        /// </summary>
        public float AloneLeash;
        /// <summary>With <see cref="ConditionFlags.Chained"/>: spell blows pass along the chain too, not only weapon blows (Shared Wound).</summary>
        public bool SharesSpells;
        /// <summary>
        /// Above zero: its count of blows counts its carrier's blows on you or your guard instead of yours on it, this many
        /// points each, and <see cref="TurnedPoints"/> more for each one turned by a perfect guard (Witnessed: 2 and 1, paid 15 a
        /// point). Its end carries the count, as any end carries the weapon blows it took.
        /// </summary>
        public int TheirBlowPoints;
        public int TurnedPoints;
        /// <summary>
        /// Each one a cast sets it on is numbered in turn, 1, 2, 3 (its run's Scale), so a line keeps its order (Eaten in Turn:
        /// the doses pass to the next on the line, not the nearest).
        /// </summary>
        public bool Numbered;
        /// <summary>
        /// Above zero (<see cref="FlankMost"/> too): what it keeps (<see cref="Keeps"/>) is kept, and the afflictions' own critical
        /// blows come, only for weapon blows from this side, in degrees from its face (see EnemyConditions.SideDegrees): a blow
        /// from elsewhere is not made critical by them and spends as usual (Mock Suns: 45 to 135, the flanks).
        /// </summary>
        public float FlankLeast;
        public float FlankMost;
        /// <summary>
        /// Above zero: it slows turning by this share for each of the stacks counted as it is set (ConditionFlags.TurnSlowed with
        /// its run's Scale; Seized Joints: 10% a notch), never below a tenth.
        /// </summary>
        public float TurnPerStack;
    }
}

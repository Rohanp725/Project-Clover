namespace Clover.Player
{
    /// <summary>
    /// The numbers of a spell, as its form reads them. Each form says which of these it uses; the rest are ignored.
    /// Distances are in metres, times in seconds.
    /// </summary>
    public class SpellSpec
    {
        /// <summary>Mana it costs: only an element's larger spells (and the built duo spells) cost any; basic spells run on <see cref="Cooldown"/> instead (Magic/SpellTimings).</summary>
        public float Cost = 30f;
        /// <summary>Seconds its slot waits after a cast before it can be cast again; zero for a spell paid in mana.</summary>
        public float Cooldown;
        public float Damage = 20f;
        public float Stagger = 20f;
        /// <summary>How far it reaches from the player.</summary>
        public float Reach = 6f;
        /// <summary>The radius of whatever it strikes with: a burst, a crater, a patch.</summary>
        public float Radius = 2.5f;
        /// <summary>Full width of a cone, in degrees.</summary>
        public float Arc = 70f;
        /// <summary>How long it or what it leaves lasts.</summary>
        public float Seconds = 4f;
        /// <summary>
        /// Above zero, the share of its wait kept when it ends with nothing to answer: a counter nothing struck waits only
        /// half as long at 0.5 (Circle of Steel, Gone to Ash). Zero changes nothing.
        /// </summary>
        public float MissedWait;
        /// <summary>Seconds between bites of what it leaves.</summary>
        public float Interval = 0.5f;
        /// <summary>How many: leaps of a bolt, blades, bullets.</summary>
        public int Count = 2;
        /// <summary>Stacks of the element's affliction it lays on what it strikes.</summary>
        public int Stacks = 1;
        /// <summary>Seconds that affliction lasts; zero for its usual time.</summary>
        public float AfflictionSeconds;
        /// <summary>Seconds before it lands or fires.</summary>
        public float Delay;
        public float Speed = 10f;
        /// <summary>How high a leap rises.</summary>
        public float Height = 3.4f;
        /// <summary>Damage each bite of what it leaves behind deals.</summary>
        public float LingerDamage = 5f;
        /// <summary>Health the cast takes and leaves grey, to be won back by blows. A spell with this may cost no mana. It cannot be cast by one who has no more health than this.</summary>
        public float HealthCost;
        /// <summary>
        /// Not None: the cast is aimed at the nearest enemy within its reach carrying this (or <see cref="AimAt2"/>), before
        /// the one faced (the ledger's aim-at-afflicted: Strangling Vine rises under the nearest Soaked enemy). With
        /// <see cref="AimFarthest"/>, the farthest such. With none such, the one faced, its damage by <see cref="AimMissShare"/> (0: whole).
        /// </summary>
        public Clover.Gameplay.AfflictionKind AimAt = Clover.Gameplay.AfflictionKind.None;
        public Clover.Gameplay.AfflictionKind AimAt2 = Clover.Gameplay.AfflictionKind.None;
        public bool AimFarthest;
        public float AimMissShare;
        /// <summary>
        /// Not None: the spell takes only the enemies carrying this (the ledger's afflicted-only). The pillars rise under
        /// them alone, in a line from the nearest to the nearest to it (One by One's line from poisoned to poisoned).
        /// </summary>
        public Clover.Gameplay.AfflictionKind OnlyCarrying = Clover.Gameplay.AfflictionKind.None;
        /// <summary>
        /// Not None, with <see cref="OnlyCarrying"/>: they must carry this as well (the ledger's reach-only-afflicted: Green
        /// Under the Ice reaches only those carrying both Chill and Seeded). The burst round you then reaches
        /// <see cref="Count"/> of them at most, the nearest first, and with none reached its slot gives back its
        /// <see cref="MissedWait"/> share of the wait.
        /// </summary>
        public Clover.Gameplay.AfflictionKind OnlyCarrying2 = Clover.Gameplay.AfflictionKind.None;
        /// <summary>
        /// For the brand form: a seal (Paper Seal; the ledger's seal-thunder). The blows only burn it down; at the last,
        /// thunder falls on the sealed enemy: Damage and <see cref="PerDose"/> a dose of the cast's affliction it carries, all
        /// its Static discharged, Stagger in Radius, and each dose it carries passing one to every enemy within
        /// <see cref="PassReach"/>. If its Seconds run out first, the thunder falls at half.
        /// </summary>
        public bool Seal;
        public float PerDose;
        public float PassReach;
        /// <summary>Above zero: the caster steps up to the enemy faced, this far at most, before the spell lands on it (Paper Seal's 3 m step).</summary>
        public float StepReach;
        /// <summary>Above zero: the blows a charge of the blade from this cast lasts, in place of the blade's usual three (Return Stroke: five; the ledger's blade-five).</summary>
        public int ChargeBlows;
        /// <summary>
        /// Above one: its damage rises smoothly as the caster's health falls, to this many times its own at one point of
        /// health (Debts Called: 2, 18 at full and 36 at one point; the ledger's health-scaled).
        /// </summary>
        public float HealthScaled;
        /// <summary>Above zero: the cast also takes this share of the caster's present health, left grey (Feed the Ground: a quarter; the ledger's health-share).</summary>
        public float HealthShare;
        /// <summary>
        /// Above zero: seconds a form's lasting thing stands (Meteor Leap's crater burns 4, 6 with its Empower; Lifting Wind's
        /// column hangs its enemy 1, 1.3 with its Empower); zero keeps the form's own time (the ledger's form-times).
        /// </summary>
        public float Lasts;
        /// <summary>Above zero: seconds between a form's pieces landing in turn (In Turn: a quarter second between spears); zero keeps the form's own quick fall.</summary>
        public float Between;
        /// <summary>Above zero: seconds a form's whole mark glows on the ground before it breaks open (Fissure's crack: 0.9); zero breaks it just behind its running mark.</summary>
        public float Glow;
        /// <summary>Above zero: the share of the run's pace the player may walk at while the form's stance holds (Firewalker: half); zero roots them.</summary>
        public float Walk;
        /// <summary>
        /// The form's own thing is set aside by the shape worn (the ledger's shape-sets-aside): no blister or seal is raised
        /// (the brand form), and a stance's answer strikes for nothing, unseen, only so the turned blow is told (Stone Skin).
        /// The cast is still made and its moments still come, for the shape's own effects.
        /// </summary>
        public bool Quiet;
        /// <summary>
        /// The moving form carries what it strikes before it and throws it down at its end (Fire Front's drive, The Black
        /// Buffalo's charge; <see cref="Clover.Player.Forms.FormCarry"/>), only those carrying <see cref="OnlyCarrying"/> when it names one.
        /// </summary>
        public bool Carries;
        /// <summary>Above zero: the cast is paid in this many flasks and no mana (Poured on the Edge: one; the ledger's price-in-flasks).</summary>
        public int FlaskCost;
        /// <summary>
        /// Above zero: the spell is held on its button up to this many seconds and loosed on letting go (or when the time is
        /// up), facing where the player faces then (the ledger's held-press: Glare's Gathered, 1 s; Diamond Thunder, 1.5 s).
        /// A tap shorter than the hold (the Hold interaction in Controls) is loosed at once with its own numbers.
        /// </summary>
        public float HoldUpTo;
        /// <summary>Above zero: the damage and the stagger at a full hold, risen from the spell's own with the hold (Gathered: stagger 22 to 60).</summary>
        public float HeldDamage, HeldStagger;
        /// <summary>
        /// Above zero: the spell is begun, not loosed, and every weapon blow landed after it adds a ring, this many at most; a
        /// second press looses it (or its press window ending), its damage and stagger scaled by the rings over
        /// <see cref="RingsFor"/>, the rings its numbers are written for (the ledger's gathered-rings: Daybreak, eight at most,
        /// written for six). See <see cref="RingGather"/>.
        /// </summary>
        public int Rings;
        public int RingsFor = 6;
        /// <summary>
        /// Above zero: the first press draws the player this many metres straight back from the enemy faced, untouchable for
        /// the moment, and the spell's form is loosed only by its second press (UpgradeAction.Flood), from wherever they stand
        /// back toward where they began and on past it to the rest of the spell's Reach (The Tide Turns: the ebb 5 m, the flood
        /// 3 m past). Pressed once, the ebb alone.
        /// </summary>
        public float Ebb;
        /// <summary>
        /// For the parry form: the stance is kept while its button is held, up to Seconds (a tap still stands it a quarter of a
        /// second), and every blow that reaches it is turned and banked, <see cref="Bank"/> at most (0: no limit), not only the
        /// first. Fire's stance counts each as a perfect guard; any other answers each with its small cone (Damage, Stagger,
        /// Stacks). On letting go: a door of the element's cone, <see cref="BankDamage"/> for each blow banked over Damage
        /// (Kiln Stance's Shut Door), and the next heavy blow within a second carries <see cref="BankStagger"/> more stagger for
        /// each (Mountain Stance). Nothing banked: the spell waits only its MissedWait share (the ledger's held-press stances).
        /// </summary>
        public bool HeldStance;
        public int Bank;
        public float BankDamage;
        public float BankStagger;

        /// <summary>The share of a full hold, 0 to 1, held for <paramref name="held"/> seconds of at most <paramref name="most"/>.</summary>
        public static float HeldShare(float held, float most) => most <= 0f ? 0f : UnityEngine.Mathf.Clamp01(held / most);

        /// <summary>Sets the numbers a hold of <paramref name="share"/> gathered: damage and stagger risen toward their held numbers.</summary>
        public static void ApplyHold(SpellSpec spec, float share)
        {
            if (spec == null) return;
            share = UnityEngine.Mathf.Clamp01(share);
            if (spec.HeldDamage > 0f) spec.Damage += (spec.HeldDamage - spec.Damage) * share;
            if (spec.HeldStagger > 0f) spec.Stagger += (spec.HeldStagger - spec.Stagger) * share;
        }

        /// <summary>The health a cast takes: its fixed price and its share of the present health.</summary>
        public static float HealthPrice(float fixedCost, float share, float present) => System.Math.Max(0f, fixedCost) + System.Math.Max(0f, share) * System.Math.Max(0f, present);

        /// <summary>What a spell rising to <paramref name="most"/> times at one point multiplies its damage by at <paramref name="fraction"/> of full health.</summary>
        public static float HealthScale(float most, float fraction) =>
            most <= 1f ? 1f : 1f + (most - 1f) * (1f - UnityEngine.Mathf.Clamp01(fraction));
    }
}

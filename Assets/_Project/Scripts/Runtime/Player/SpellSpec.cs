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
        /// <summary>Above zero: the cast is paid in this many flasks and no mana (Poured on the Edge: one; the ledger's price-in-flasks).</summary>
        public int FlaskCost;

        /// <summary>The health a cast takes: its fixed price and its share of the present health.</summary>
        public static float HealthPrice(float fixedCost, float share, float present) => System.Math.Max(0f, fixedCost) + System.Math.Max(0f, share) * System.Math.Max(0f, present);

        /// <summary>What a spell rising to <paramref name="most"/> times at one point multiplies its damage by at <paramref name="fraction"/> of full health.</summary>
        public static float HealthScale(float most, float fraction) =>
            most <= 1f ? 1f : 1f + (most - 1f) * (1f - UnityEngine.Mathf.Clamp01(fraction));
    }
}

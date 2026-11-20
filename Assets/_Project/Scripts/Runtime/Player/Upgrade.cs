using System;
using System.Collections.Generic;
using Clover.Player.Magic;
using Clover.Player.Trees;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// One mark on the chart: what it is called, what it costs, where it sits, and what it does, as data. The trees are
    /// written one file to an element under <c>Trees/</c>; <see cref="UpgradeTrees"/> gathers them into
    /// <see cref="All"/>. What a mark does is the sum of its <see cref="Stats"/>, its <see cref="Effects"/>, the spell
    /// it teaches (<see cref="Form"/>) and, for the few written by hand, whatever <see cref="PlayerUpgrades"/> does
    /// for its id.
    /// </summary>
    public class Upgrade
    {
        public string Id = "";
        public string Name = "";
        /// <summary>The element whose tree it is on. For a mix: the first of the two round the ring.</summary>
        public string Element = "";
        /// <summary>For a mark on a mixed branch: the second element. Empty otherwise.</summary>
        public string Element2 = "";
        public UpgradeKind Kind = UpgradeKind.Minor;
        /// <summary>What it costs to learn, in its <see cref="Currency"/>. Left at zero it takes its kind's usual cost (<see cref="UsualCost(Upgrade)"/>).</summary>
        public int Cost;
        public string Description = "";
        /// <summary>The weapon it needs, or empty if it works with any.</summary>
        public string Weapon = "";
        /// <summary>Ids of the marks it grows from: any one of them learned opens it. Empty: it grows straight from the foot of its tree.</summary>
        public string[] Requires = Array.Empty<string>();
        /// <summary>Where it sits in its own tree, in steps: Y runs outward from the element's emblem, X across (positive is clockwise).</summary>
        public float X, Y;
        /// <summary>The name of its picture, from <see cref="UpgradeIcons"/>.</summary>
        public string Icon = UpgradeIcons.Star;
        /// <summary>Changes to the player's numbers for as long as it is learned.</summary>
        public StatMod[] Stats = Array.Empty<StatMod>();
        /// <summary>What it does when things happen.</summary>
        public UpgradeEffect[] Effects = Array.Empty<UpgradeEffect>();
        /// <summary>Stacks of the element's affliction every blow of the weapon lays while this is learned. This is what a root does.</summary>
        public int WeaponStacks;
        /// <summary>The id of the spell form it teaches, from <see cref="SpellForms"/>. A mark with a form can be worn in the hold-spell slot.</summary>
        public string Form = "";
        /// <summary>The numbers of the spell it teaches.</summary>
        public SpellSpec Spell;
        /// <summary>A count the mark keeps for its effects to bank and spend, or null.</summary>
        public TallySpec Tally;
        /// <summary>Which chart it is drawn on (IMPLEMENTATION.md 2.1). <see cref="MarkTree.Hidden"/> marks are never drawn and never sold.</summary>
        public MarkTree Tree = MarkTree.Element;
        /// <summary>For a reaction mark: its rest and what else the pair says; null for every other mark.</summary>
        public ReactionDef Reaction;
        /// <summary>What it is bought with (IMPLEMENTATION.md 2.1). <see cref="Currency.None"/>: given, never sold, and it costs nothing.</summary>
        public Currency Currency = Currency.Shards;
        /// <summary>For a spell branch's marks: the id of its Learn mark, the branch's own id. Empty for a passive.</summary>
        public string Branch = "";
        /// <summary>Its place in its spell branch (Learn, Empower, Shape, Master); None for a passive.</summary>
        public BranchPart Part = BranchPart.None;
        /// <summary>On a Shape mark: the two versions it opens, first and second. Empty on every other mark.</summary>
        public ShapeDef[] Shapes = Array.Empty<ShapeDef>();
        /// <summary>On an Empower mark: the numbers it changes on its branch's spell. Null on every other mark.</summary>
        public Empower Empower;
        /// <summary>On a Learn mark (and a duo spell): what the spell is for, the role icon in the Hand.</summary>
        public SpellRole Role = SpellRole.None;
        /// <summary>On a reaction lift: the element whose reactions it lifts (its effects ride <see cref="UpgradeTrigger.Reacted"/>). Empty otherwise.</summary>
        public string Lifts = "";
        /// <summary>On a Way mark (phase 5): the aspect it belongs to (<c>katana.sheath</c>); empty for the trunk.</summary>
        public string Aspect = "";
        /// <summary>
        /// A progress flag that must be set before it can be bought (an elite beaten, a land reached), or <c>gods.N</c> for
        /// N gods felled; empty for none. Read by <see cref="MarkRules.GateOpen(Upgrade, Func{string, bool}, int, bool)"/>.
        /// </summary>
        public string Gate = "";
        /// <summary>One line for the body sign or look it brings (Heresies, phase 6); empty for none.</summary>
        public string Seen = "";
        /// <summary>
        /// The standard rules it switches while in force (a Devotion's now, a Heresy's from phase 6), written into the
        /// player's <see cref="Growth.RuleBook"/> each time what is in force is worked out; null for none. Written as a
        /// lambda that captures nothing, so calling it allocates nothing.
        /// </summary>
        public Action<Growth.RuleBook> Rules;
        /// <summary>Its place in <see cref="All"/>.</summary>
        public int Index { get; internal set; }

        /// <summary>
        /// True for an element's base spell's Learn mark: its id is the base id (<c>fire.base</c>), it comes with its element
        /// (never bought), and it is cast by the element's kit, so it has no form of its own.
        /// </summary>
        public bool IsBaseSpell => Part == BranchPart.Learn && Magic.SpellSlots.IsBase(Id);
        /// <summary>True for a mark that is never sold: given, a god's gift, a hidden mark.</summary>
        public bool Given => Currency == Currency.None || Signature || IsHidden;

        /// <summary>True for what only a god's fall gives: it is never for sale.</summary>
        public bool Signature => Kind == UpgradeKind.Signature;
        /// <summary>True for a mark on the branch two elements share.</summary>
        public bool IsMix => !string.IsNullOrEmpty(Element2);
        /// <summary>True when it teaches a spell that can be worn.</summary>
        public bool IsSpell => !string.IsNullOrEmpty(Form);

        /// <summary>True for a mark on no chart (a reaction): never drawn, opened or sold.</summary>
        public bool IsHidden => Tree == MarkTree.Hidden;

        /// <summary>True for a mark of a weapon's Way or of the Body (phase 5): bought with Memories, on no element's tree even when it names an element.</summary>
        public bool IsWay => Tree == MarkTree.Way || Tree == MarkTree.Body;

        /// <summary>
        /// True when it is on the tree of this element, or on a mixed branch this element shares. A hidden mark is on no tree,
        /// and nor is a Way mark that names an element (the Katana's Burning Crescent works while Fire is carried, on the Way).
        /// </summary>
        public bool BelongsTo(string element) => !IsHidden && !IsWay && (Element == element || (IsMix && Element2 == element));

        // Fire: pressure and momentum.
        public const string Heat = "fire.heat";
        public const string BurnAndDetonate = "fire.detonate";
        public const string Backdraft = "fire.backdraft";
        public const string FlameTrail = "fire.trail";
        public const string BurningCrescent = "fire.crescent";
        public const string FirebombFlask = "fire.firebomb";
        public const string MeteorLeap = "fire.meteor";
        // Lightning: speed and precision.
        public const string Static = "lightning.static";
        public const string StoredCharge = "lightning.charge";
        public const string Blink = "lightning.blink";
        public const string Afterimage = "lightning.afterimage";
        public const string DelayedCuts = "lightning.cuts";
        public const string LightningRod = "lightning.rod";
        public const string Overclock = "lightning.overclock";
        // What a god leaves, by the choice made over him.
        public const string StormStep = "lightning.stormstep";
        public const string Plasma = "fire.plasma";

        /// <summary>What a mark of each kind costs unless it says otherwise.</summary>
        public static int UsualCost(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Minor: return 1;
                case UpgradeKind.Root: return 2;
                case UpgradeKind.Major: return 3;
                case UpgradeKind.Spell: return 4;
                case UpgradeKind.Keystone: return 6;
                case UpgradeKind.Mix: return 3;
                // The shared prices of steel/README.md: Devotion 6 Shards, an Evolution 5 once found, a deep Heresy 8, an
                // aspect 2 Memories, a technique 2 Memories, a temper rank 1 (the dearer ranks write their own).
                case UpgradeKind.Devotion: return 6;
                case UpgradeKind.Evolution: return 5;
                case UpgradeKind.Heresy: return 8;
                case UpgradeKind.Aspect: return 2;
                case UpgradeKind.Technique: return 2;
                case UpgradeKind.Temper: return 1;
                default: return 0;
            }
        }

        /// <summary>
        /// What a mark of each kind costs on the Way and the Body, in Memories (steel/weapons.md 0.2, way_plan.md 0.3): a
        /// Number 1, a Move or a Rule 2, a keystone (or the Body's bearing) 4, an aspect 2, a temper rank 1 (the dearer
        /// ranks write their own), a technique 2.
        /// </summary>
        public static int UsualWayCost(UpgradeKind kind)
        {
            switch (kind)
            {
                case UpgradeKind.Minor: return 1;
                case UpgradeKind.Major: return 2;
                case UpgradeKind.Keystone: return 4;
                case UpgradeKind.Aspect: return 2;
                case UpgradeKind.Temper: return 1;
                case UpgradeKind.Technique: return 2;
                default: return 0;
            }
        }

        /// <summary>What the mark costs unless it says otherwise: nothing when it is given, else its kind's usual cost in its currency.</summary>
        public static int UsualCost(Upgrade mark)
        {
            if (mark == null || mark.Currency == Currency.None) return 0;
            return mark.Currency == Currency.Memories ? UsualWayCost(mark.Kind) : UsualCost(mark.Kind);
        }

        private static List<Upgrade> all;
        private static Dictionary<string, Upgrade> byId;
        private static bool extended;

        /// <summary>Every mark there is, tree by tree in the order the elements stand round the ring, the mixes after their first element.</summary>
        public static IReadOnlyList<Upgrade> All
        {
            get
            {
                if (all == null) Gather(null);
                return all;
            }
        }

        /// <summary>How many effects there are in the whole catalogue; each has its own <see cref="UpgradeEffect.Slot"/> below this.</summary>
        public static int EffectCount { get; private set; }

        // Domain reload is off: marks a probe added to try the chart at scale must not stay for the next play.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad()
        {
            if (extended) Gather(null);
        }

        /// <summary>
        /// Gathers the catalogue again, with more marks added by the caller. For probes and tests that need a chart
        /// fuller than the game's own; whoever holds the old list must be told (the menu and the player's upgrades
        /// each have a way to look again).
        /// </summary>
        public static void Gather(Action<List<Upgrade>> more)
        {
            var list = new List<Upgrade>();
            UpgradeTrees.AddAll(list);
            extended = more != null;
            more?.Invoke(list);
            var index = new Dictionary<string, Upgrade>();
            int slots = 0;
            int place = 0;
            foreach (var upgrade in list)
            {
                upgrade.Index = place++;
                // A given mark (a base spell's Learn) costs nothing, whatever its kind.
                if (upgrade.Currency == Currency.None) upgrade.Cost = 0;
                else if (upgrade.Cost == 0) upgrade.Cost = UsualCost(upgrade);
                // Basic spells run on cooldowns and the larger ones cost mana, by the one table of the shared rules.
                SpellTimings.Apply(upgrade);
                index[upgrade.Id] = upgrade;
                foreach (var effect in upgrade.Effects)
                {
                    effect.Slot = slots++;
                    effect.Owner = upgrade;
                }
                // A shape's effects belong to its Shape mark: they run only while that shape is worn (SpellBranches.ShapeOf).
                if (upgrade.Shapes != null)
                    foreach (var shape in upgrade.Shapes)
                    {
                        if (shape?.Effects == null) continue;
                        foreach (var effect in shape.Effects)
                        {
                            effect.Slot = slots++;
                            effect.Owner = upgrade;
                        }
                    }
            }
            all = list;
            byId = index;
            EffectCount = slots;
        }

        public static Upgrade ById(string id)
        {
            if (all == null) Gather(null);
            return id != null && byId.TryGetValue(id, out var found) ? found : null;
        }
    }
}

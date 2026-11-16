using System;
using System.Collections.Generic;
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
        /// <summary>Embers to learn it. Left at zero it takes its kind's usual cost.</summary>
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
        /// <summary>Its place in <see cref="All"/>.</summary>
        public int Index { get; internal set; }

        /// <summary>True for what only a god's fall gives: it is never for sale.</summary>
        public bool Signature => Kind == UpgradeKind.Signature;
        /// <summary>True for a mark on the branch two elements share.</summary>
        public bool IsMix => !string.IsNullOrEmpty(Element2);
        /// <summary>True when it teaches a spell that can be worn.</summary>
        public bool IsSpell => !string.IsNullOrEmpty(Form);

        /// <summary>True when it is on the tree of this element, or on a mixed branch this element shares.</summary>
        public bool BelongsTo(string element) => Element == element || (IsMix && Element2 == element);

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
                default: return 0;
            }
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
                if (upgrade.Cost == 0) upgrade.Cost = UsualCost(upgrade.Kind);
                index[upgrade.Id] = upgrade;
                foreach (var effect in upgrade.Effects)
                {
                    effect.Slot = slots++;
                    effect.Owner = upgrade;
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

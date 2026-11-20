using Clover.Core;
using Clover.Player.Build;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The player's build in play (IMPLEMENTATION.md 3.6): the current <see cref="Loadout"/>, read from the parts that
    /// carry it (the attunements and the surge from <see cref="PlayerElement"/>, the slots from <see cref="PlayerSpells"/>,
    /// the weapon from <see cref="PlayerCombat"/>), and the two ways it is changed: the attunements (picked in the hub;
    /// in the playtest at the altars) and the Hand (the slots and the surge, only at a rest fire). The rules are
    /// <see cref="HandRules"/>; this adapts them. The controller adds it to a player built before it existed, so no
    /// scene had to be rebuilt.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerBuild : MonoBehaviour
    {
        /// <summary>
        /// Whether the slot schedule (<see cref="SlotSchedule"/>) shuts slots outside the playtest: West and North at
        /// the start, East once the Champion of the Gate falls, South at the Fire god (task 2.4). In the playtest every
        /// slot is open whatever this says. Off, every slot is open everywhere, as in Phase 1.
        /// </summary>
        public bool UseSchedule = true;

        /// <summary>The loadout as it stands, refreshed in place by <see cref="Read"/>.</summary>
        public Loadout Current { get; } = new Loadout();
        /// <summary>Goes up whenever the Hand or the attunements are changed here.</summary>
        public int Version { get; private set; }
        /// <summary>The Hand or the attunements changed (the Hand screen and the HUD redraw).</summary>
        public event System.Action Changed;
        // Counts for probes.
        public int HandsApplied { get; private set; }
        public int HandsRefused { get; private set; }
        public int AttunementsSet { get; private set; }

        private PlayerElement element;
        private PlayerCombat combat;
        private PlayerUpgrades upgrades;
        private PlayerSpells spellsFound;
        private PlayerSpells spells => spellsFound != null ? spellsFound : (spellsFound = GetComponent<PlayerSpells>());
        private HandRules rules;
        private System.Func<string, bool> keepLent;
        private int surgeReadAt = -1;
        // The schedule read once and kept until a flag, the gods felled, the playtest or the switch changes, so asking
        // for it every frame costs nothing.
        private static readonly System.Func<string, bool> HasFlag = Progress.Has;
        private HandOpen openKept;
        private int openProgress = -1, openGods = -1, openMode = -1;

        /// <summary>The Hand's rules over the game's catalogue, made when first wanted.</summary>
        public HandRules Rules => rules ??= new HandRules();

        private void Awake()
        {
            element = GetComponent<PlayerElement>();
            combat = GetComponent<PlayerCombat>();
            upgrades = GetComponent<PlayerUpgrades>();
            keepLent = KeepLent;
            // The shapes chosen at a fire are kept for the run, as the slots are.
            SpellBranches.CopyChoices(RunState.Shapes, Current.Shapes);
        }

        /// <summary>
        /// Chooses which of its two shapes a spell wears (0 or 1; IMPLEMENTATION.md 2.2), by its branch's Learn id, at a
        /// rest fire only; kept for the run. What is in force is worked out again at once. False away from a fire.
        /// </summary>
        public bool ChooseShape(string branch, int choice, bool atRestFire)
        {
            if (string.IsNullOrEmpty(branch) || !HandRules.CanChangeHere(atRestFire))
            {
                HandsRefused++;
                return false;
            }
            SpellBranches.Choose(Current.Shapes, branch, choice);
            SpellBranches.CopyChoices(Current.Shapes, RunState.Shapes);
            Push();
            return true;
        }

        /// <summary>
        /// The loadout as it stands now, written into <see cref="Current"/> and returned. Read a few times a second by
        /// the marks; it allocates nothing once the surge's id has been made for the elements carried.
        /// </summary>
        public Loadout Read()
        {
            Fill(Current, element, spells, combat, element != null && element.Version == surgeReadAt);
            if (element != null) surgeReadAt = element.Version;
            return Current;
        }

        /// <summary>Writes what the player's parts carry into a loadout (any part may be missing).</summary>
        public static Loadout Fill(Loadout into, PlayerElement element, PlayerSpells spells, PlayerCombat combat) => Fill(into, element, spells, combat, false);

        private static Loadout Fill(Loadout into, PlayerElement element, PlayerSpells spells, PlayerCombat combat, bool surgeKnown)
        {
            // Info is null until PlayerElement.Awake has run, so an early caller reads an empty Hand rather than failing.
            bool holding = element != null && element.HasElement && element.Info != null;
            into.Attunements[0] = holding ? element.Info.Name : "";
            into.Attunements[1] = holding && element.SecondInfo != null ? element.SecondInfo.Name : "";
            if (!surgeKnown) into.Surge = holding && element.SurgeInfo != null ? element.SurgeId : "";
            for (int i = 0; i < Loadout.SlotCount; i++) into.Slots[i] = spells != null ? spells.Slots[i] : "";
            into.Weapon = combat != null && combat.Weapon != null ? combat.Weapon.Name : "";
            return into;
        }

        /// <summary>How much of the Hand is open now: by the schedule's flags; all of it in the playtest, or with the schedule off.</summary>
        public HandOpen Open
        {
            get
            {
                int mode = (UseSchedule ? 1 : 0) + (PlaytestMode.On ? 2 : 0);
                if (openProgress == Progress.Version && openGods == RunState.GodsFelled && openMode == mode) return openKept;
                openProgress = Progress.Version;
                openGods = RunState.GodsFelled;
                openMode = mode;
                openKept = !UseSchedule ? HandOpen.All : SlotSchedule.Open(HasFlag, RunState.GodsFelled, PlaytestMode.On);
                return openKept;
            }
        }

        /// <summary>
        /// Sets the Hand's own choices (the four spells and the surge) from a loadout, at a rest fire only. What the rules
        /// do not allow under the attunements carried is dropped (<see cref="HandRules.Validate"/>); the attunements and
        /// the weapon are never taken from it. From now on the slots no longer fill themselves. False away from a fire.
        /// </summary>
        public bool Apply(Loadout hand, bool atRestFire)
        {
            if (hand == null || !HandRules.CanChangeHere(atRestFire))
            {
                HandsRefused++;
                return false;
            }
            Read();
            Current.CopyHandFrom(hand);
            // A Hand that says nothing of shapes (one kept before shapes were bought) keeps the shapes chosen; one that does sets them.
            if (hand.Shapes == null || hand.Shapes.Count == 0) SpellBranches.CopyChoices(RunState.Shapes, Current.Shapes);
            else SpellBranches.CopyChoices(Current.Shapes, RunState.Shapes);
            Rules.Validate(Current, RunState.Unlocked, keepLent);
            // A slot the schedule keeps shut holds nothing.
            var open = Open;
            for (int i = 0; i < Loadout.SlotCount; i++)
                if (!open.SlotOpen(i)) Current.Slots[i] = "";
            RunState.HandSet = true;
            Push();
            HandsApplied++;
            return true;
        }

        /// <summary>
        /// Carries these elements (the hub's choice, the playtest's altars, a probe): "" in the second for one element,
        /// the first twice for all in. The Hand then drops what no longer fits and its surge falls back to the first's
        /// when its element went.
        /// </summary>
        public void Attune(string first, string second)
        {
            if (element == null) return;
            element.Attune(first, second);
            if (RunState.Element != (first ?? "")) RunState.ElementConsumed = false;
            RunState.Element = first ?? "";
            RunState.Element2 = second ?? "";
            Read();
            Rules.Validate(Current, RunState.Unlocked, keepLent);
            Push();
            AttunementsSet++;
        }

        /// <summary>Keeps the current Hand as saved Hand <paramref name="index"/> (0 to 2).</summary>
        public void SaveHand(int index)
        {
            if (index < 0 || index >= RunState.SavedHands.Length) return;
            RunState.SavedHands[index] = Read().Clone();
            Version++;
            Changed?.Invoke();
        }

        /// <summary>The saved Hand at <paramref name="index"/>, or null when none was kept there.</summary>
        public static Loadout SavedHand(int index) => index >= 0 && index < RunState.SavedHands.Length ? RunState.SavedHands[index] : null;

        /// <summary>Wears saved Hand <paramref name="index"/>, at a rest fire only. False when there is none or away from a fire.</summary>
        public bool LoadHand(int index, bool atRestFire)
        {
            var saved = SavedHand(index);
            return saved != null && Apply(saved, atRestFire);
        }

        /// <summary>
        /// A suggested Hand for the elements carried: each element's own spell, then its learned spells in the chart's
        /// order, the first element's surge. Stands in for the designed starter Hands per element, which are not written yet.
        /// </summary>
        public Loadout Suggested()
        {
            var suggested = Read().Clone();
            for (int i = 0; i < Loadout.SlotCount; i++) suggested.Slots[i] = "";
            var choices = Rules.ChoicesFor(0, suggested, RunState.Unlocked);
            int slot = 0;
            // The base spells first, then the rest in order, as the slots once filled themselves.
            for (int pass = 0; pass < 2; pass++)
                foreach (var choice in choices)
                {
                    if (slot >= Loadout.SlotCount) break;
                    if (choice.IsBase != (pass == 0)) continue;
                    suggested.Slots[slot++] = choice.Id;
                }
            suggested.Surge = HandRules.SurgeId(suggested.First);
            return suggested;
        }

        // The Hand in Current goes out to the parts that carry it.
        private void Push()
        {
            if (spells != null) spells.SetAll(Current.Slots);
            if (element != null) element.WearSurgeById(Current.Surge);
            RunState.Surge = Current.Surge;
            if (upgrades != null) upgrades.RefreshNow();
            Read();
            Version++;
            Changed?.Invoke();
        }

        // A spell lent by the yard's rack is no mark of a tree, but it may stay in its slot.
        private bool KeepLent(string id) => upgrades != null && upgrades.Lent != null && upgrades.Lent.Id == id;
    }
}

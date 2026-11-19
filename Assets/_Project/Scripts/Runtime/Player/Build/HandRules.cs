using System;
using System.Collections.Generic;
using Clover.Player.Magic;

namespace Clover.Player.Build
{
    /// <summary>
    /// The Hand's rules (STEEL_AND_ELEMENT.md 5, IMPLEMENTATION.md 3.6), as plain C# over a catalogue of marks so they are
    /// tested without a scene: what may go in a spell slot (a learned spell of a carried element, the element's own base
    /// spell, a duo spell while both its elements are carried), what may be the surge (a carried element's), what is
    /// dropped when the attunements change, which element each infused strike carries, and where the Hand may be changed.
    /// </summary>
    public class HandRules
    {
        /// <summary>The design's most choices for one slot: six spells from each of two elements and one duo spell.</summary>
        public const int MostChoices = 13;
        public const string SurgeSuffix = ".surge";

        private readonly IReadOnlyList<Upgrade> catalogue;
        private readonly Dictionary<string, Upgrade> byId = new Dictionary<string, Upgrade>();

        /// <summary>The rules over the game's whole catalogue.</summary>
        public HandRules() : this(Upgrade.All) { }

        /// <summary>The rules over the marks given (a test's own).</summary>
        public HandRules(IReadOnlyList<Upgrade> marks)
        {
            catalogue = marks ?? Array.Empty<Upgrade>();
            foreach (var mark in catalogue)
                if (mark != null && !string.IsNullOrEmpty(mark.Id)) byId[mark.Id] = mark;
        }

        public Upgrade Find(string id) => id != null && byId.TryGetValue(id, out var mark) ? mark : null;

        // ---- Ids that are not marks: an element's base spell and its surge.

        public static string BaseId(string element) => SpellSlots.BaseId(element);

        /// <summary>The element whose base spell the id names (<c>fire.base</c> is Fire's), or "".</summary>
        public static string ElementOfBase(string id) => ElementOf(id, SpellSlots.BaseSuffix);

        public static string SurgeId(string element) => string.IsNullOrEmpty(element) ? "" : element.ToLowerInvariant() + SurgeSuffix;

        /// <summary>The element whose surge the id names (<c>fire.surge</c> is Fire's), or "".</summary>
        public static string ElementOfSurge(string id) => ElementOf(id, SurgeSuffix);

        private static string ElementOf(string id, string suffix)
        {
            if (string.IsNullOrEmpty(id) || !id.EndsWith(suffix, StringComparison.Ordinal)) return "";
            int length = id.Length - suffix.Length;
            foreach (var element in Elements.All)
                if (element.Name.Length == length && string.Compare(id, 0, element.Name, 0, length, StringComparison.OrdinalIgnoreCase) == 0)
                    return element.Name;
            return "";
        }

        // ---- What may go where.

        /// <summary>
        /// True when the spell may sit in a slot under this loadout: a carried element's base spell, or a learned spell
        /// mark whose element is carried (a duo spell: both of its elements).
        /// </summary>
        public bool CanSlot(string id, Loadout loadout, ICollection<string> learned)
        {
            if (string.IsNullOrEmpty(id) || loadout == null) return false;
            string baseOf = ElementOfBase(id);
            if (baseOf.Length > 0) return loadout.Carries(baseOf);
            var mark = Find(id);
            if (mark == null || !mark.IsSpell || learned == null || !learned.Contains(id)) return false;
            return MarkRules.Slottable(mark, loadout);
        }

        /// <summary>True when the id is a carried element's surge. Duo surges are not built yet, so none is offered.</summary>
        public static bool CanWearSurge(string id, Loadout loadout)
        {
            if (loadout == null) return false;
            string element = ElementOfSurge(id);
            return element.Length > 0 && loadout.Carries(element);
        }

        /// <summary>
        /// What can go in a slot, grouped by element (the first attunement's base spell and learned spells, then the
        /// second's, then the duo spells of the pair), each with its role and one line. Into the list given (cleared), or
        /// a new one.
        /// </summary>
        public List<HandChoice> ChoicesFor(int slot, Loadout loadout, ICollection<string> learned, List<HandChoice> into = null)
        {
            into ??= new List<HandChoice>();
            into.Clear();
            if (loadout == null || loadout.Attunements == null) return into;
            for (int a = 0; a < loadout.Attunements.Length; a++)
            {
                string element = loadout.Attunements[a];
                if (string.IsNullOrEmpty(element) || Elements.ByName(element) == null || SeenBefore(loadout.Attunements, a)) continue;
                into.Add(BaseChoice(element, loadout));
                foreach (var mark in catalogue)
                {
                    if (mark == null || !mark.IsSpell || mark.IsMix || mark.Element != element) continue;
                    if (learned == null || !learned.Contains(mark.Id) || !MarkRules.WeaponCarried(mark, loadout)) continue;
                    into.Add(MarkChoice(mark, loadout));
                }
            }
            if (loadout.Mixed)
                foreach (var mark in catalogue)
                {
                    if (mark == null || !mark.IsSpell || !mark.IsMix || learned == null || !learned.Contains(mark.Id)) continue;
                    if (!MarkRules.Slottable(mark, loadout)) continue;
                    into.Add(MarkChoice(mark, loadout));
                }
            return into;
        }

        /// <summary>The surges the Hand can wear: each carried element's, the first's first.</summary>
        public List<HandChoice> SurgesFor(Loadout loadout, List<HandChoice> into = null)
        {
            into ??= new List<HandChoice>();
            into.Clear();
            if (loadout == null || loadout.Attunements == null) return into;
            for (int a = 0; a < loadout.Attunements.Length; a++)
            {
                string element = loadout.Attunements[a];
                if (string.IsNullOrEmpty(element) || Elements.ByName(element) == null || SeenBefore(loadout.Attunements, a)) continue;
                var kit = ElementKit.ByName(element, false);
                into.Add(new HandChoice
                {
                    Id = SurgeId(element),
                    Name = kit.SurgeName,
                    Element = element,
                    Element2 = "",
                    Role = SpellRole.None,
                    Line = kit.SurgeCost > 0f ? $"{element}'s surge, {Math.Round(kit.SurgeCost)} mana." : $"{element}'s surge, paid in health.",
                    InSlot = -1,
                    Mana = kit.SurgeCost,
                });
            }
            return into;
        }

        /// <summary>
        /// Makes the loadout fit its attunements and what is learned: a spell no longer allowed leaves its slot (a duo
        /// spell whose pair broke, a spell of an element no longer carried, one not learned); a spell in two slots keeps
        /// the first; a surge not of a carried element becomes the first element's. <paramref name="keep"/> may allow an
        /// id the rules do not know (a spell lent by the yard's rack). True when anything changed.
        /// </summary>
        public bool Validate(Loadout loadout, ICollection<string> learned, Func<string, bool> keep = null)
        {
            if (loadout == null) return false;
            bool changed = false;
            if (loadout.Slots == null || loadout.Slots.Length != Loadout.SlotCount)
            {
                var slots = new string[Loadout.SlotCount];
                for (int i = 0; i < slots.Length; i++) slots[i] = loadout.Slots != null && i < loadout.Slots.Length && loadout.Slots[i] != null ? loadout.Slots[i] : "";
                loadout.Slots = slots;
                changed = true;
            }
            for (int i = 0; i < loadout.Slots.Length; i++)
            {
                string id = loadout.Slots[i] ?? "";
                if (id.Length == 0) continue;
                bool twice = false;
                for (int j = 0; j < i; j++)
                    if (loadout.Slots[j] == id) twice = true;
                if (!twice && (CanSlot(id, loadout, learned) || (keep != null && keep(id)))) continue;
                loadout.Slots[i] = "";
                changed = true;
            }
            string surge = loadout.Surge ?? "";
            if (!CanWearSurge(surge, loadout))
            {
                string fallback = SurgeId(loadout.First);
                if (fallback != surge)
                {
                    loadout.Surge = fallback;
                    changed = true;
                }
            }
            return changed;
        }

        /// <summary>The Hand is changed only at a rest fire (STEEL_AND_ELEMENT.md 4.2).</summary>
        public static bool CanChangeHere(bool atRestFire) => atRestFire;

        /// <summary>
        /// The element an infused strike carries: L2 with R1 the first; L2 with R2 the second. All in, or with one
        /// element (the plan's answer for a Hand of one, IMPLEMENTATION.md 8.5 question 3), L2 with R2 is the first
        /// element's deep strike. "" with no element.
        /// </summary>
        public static string StrikeElement(Loadout loadout, bool second, out bool deep)
        {
            deep = false;
            if (loadout == null || loadout.First == "") return "";
            if (!second) return loadout.First;
            if (loadout.Mixed) return loadout.Second;
            deep = true;
            return loadout.First;
        }

        private static bool SeenBefore(string[] attunements, int index)
        {
            for (int i = 0; i < index; i++)
                if (attunements[i] == attunements[index]) return true;
            return false;
        }

        private static HandChoice BaseChoice(string element, Loadout loadout)
        {
            var kit = ElementKit.ByName(element, false);
            var timing = SpellTimings.Base(element);
            string id = BaseId(element);
            return new HandChoice
            {
                Id = id,
                Name = kit.SpellName,
                Element = element,
                Element2 = "",
                Role = timing.Role,
                Line = $"{element}'s own spell.",
                IsBase = true,
                InSlot = SlotOf(loadout, id),
                Mana = timing.Mana,
                Cooldown = timing.Cooldown,
            };
        }

        private static HandChoice MarkChoice(Upgrade mark, Loadout loadout)
        {
            var timing = SpellTimings.For(mark.Id);
            return new HandChoice
            {
                Id = mark.Id,
                Name = mark.Name,
                Element = mark.Element,
                Element2 = mark.Element2 ?? "",
                Role = timing.Role,
                Line = FirstSentence(mark.Description),
                IsDuo = mark.IsMix,
                InSlot = SlotOf(loadout, mark.Id),
                Mana = mark.Spell != null ? mark.Spell.Cost : timing.Mana,
                Cooldown = mark.Spell != null && mark.Spell.Cooldown > 0f ? mark.Spell.Cooldown : timing.Cooldown,
            };
        }

        private static int SlotOf(Loadout loadout, string id)
        {
            for (int i = 0; i < Loadout.SlotCount; i++)
                if (loadout.SlotAt(i) == id) return i;
            return -1;
        }

        /// <summary>The first sentence of a description: the one plain line the Hand shows.</summary>
        public static string FirstSentence(string text)
        {
            if (string.IsNullOrEmpty(text)) return "";
            int end = text.IndexOf(". ", StringComparison.Ordinal);
            return end > 0 ? text.Substring(0, end + 1) : text;
        }
    }
}

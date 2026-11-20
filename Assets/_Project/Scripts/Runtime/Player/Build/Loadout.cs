using System;
using System.Collections.Generic;

namespace Clover.Player.Build
{
    /// <summary>
    /// Everything the player carries into a fight (IMPLEMENTATION.md 3.6): the Hand (the attunements, the four spell
    /// slots, the surge, the shapes, the Heresies) and the weapon with its active aspect. It is the only thing
    /// <see cref="MarkRules.InForce"/> reads, so "what is in force" is decided in one place. Plain and serializable, so a
    /// save can hold it and tests can build one.
    ///
    /// The attunements are picked in the hub (the owner's decision of 2026-11-18); the Hand at a rest fire sets only the
    /// slots and the surge from them. A third element (Three Bloods) would make <see cref="Attunements"/> longer: every
    /// loop runs over the array, nothing assumes two.
    /// </summary>
    [Serializable]
    public class Loadout
    {
        public const int SlotCount = 4;

        /// <summary>The elements carried, first and second; "" for an empty attunement. The same one twice is all in.</summary>
        public string[] Attunements = { "", "" };
        /// <summary>The spells in the four slots by button (West, North, East, South): a mark id or a base id (<c>fire.base</c>); "" for none.</summary>
        public string[] Slots = { "", "", "", "" };
        /// <summary>The surge a tap of L2 lights: <c>&lt;element&gt;.surge</c> (lower case) or a duo surge's mark id.</summary>
        public string Surge = "";
        /// <summary>The Heresies worn (phase 6); "" for an empty slot.</summary>
        public string[] Heresies = { "", "" };
        /// <summary>Which shape (0 or 1) each spell or reaction with a Shape mark wears (phase 4).</summary>
        public List<IdChoice> Shapes = new List<IdChoice>();
        /// <summary>Learn ids whose evolved spell is chosen (phase 6).</summary>
        public List<IdChoice> UseEvolved = new List<IdChoice>();
        /// <summary>The weapon kind carried (<c>Weapons.KatanaName</c>); a god's weapon counts as its kind.</summary>
        public string Weapon = "";
        /// <summary>The active aspect of each weapon kind (phase 5): the kind's name, then the aspect's id (<c>katana.sheath</c>).</summary>
        public List<IdText> Aspects = new List<IdText>();
        /// <summary>
        /// The keystone set on each bough (way_plan.md 0.1: both can be bought, one is active): the bough's aspect id (or
        /// <see cref="Way.WayRules.BodyBough"/> for the Body's two bearings), then the keystone's mark id.
        /// </summary>
        public List<IdText> Keystones = new List<IdText>();
        /// <summary>The Fists' choice of technique (phase 5).</summary>
        public string Technique = "";

        /// <summary>The first attunement: the element the weapon's blows lay (the owner's rule), or "".</summary>
        public string First => Attunements != null && Attunements.Length > 0 ? Attunements[0] ?? "" : "";
        /// <summary>The second attunement, or "".</summary>
        public string Second => Attunements != null && Attunements.Length > 1 ? Attunements[1] ?? "" : "";
        /// <summary>The same element twice: its Devotion is on and L2 with R2 is the deep strike.</summary>
        public bool AllIn => First != "" && First == Second;
        /// <summary>Two different elements: a mix, with its duo branch and reaction.</summary>
        public bool Mixed => First != "" && Second != "" && First != Second;

        /// <summary>True when the element is in any attunement.</summary>
        public bool Carries(string element)
        {
            if (string.IsNullOrEmpty(element) || Attunements == null) return false;
            for (int i = 0; i < Attunements.Length; i++)
                if (Attunements[i] == element) return true;
            return false;
        }

        /// <summary>True when the spell sits in one of the four slots.</summary>
        public bool Slotted(string id)
        {
            if (string.IsNullOrEmpty(id) || Slots == null) return false;
            for (int i = 0; i < Slots.Length; i++)
                if (Slots[i] == id) return true;
            return false;
        }

        /// <summary>The slot's spell id, or "" for an empty or missing slot.</summary>
        public string SlotAt(int slot) => Slots != null && slot >= 0 && slot < Slots.Length ? Slots[slot] ?? "" : "";

        /// <summary>The aspect chosen for a weapon kind, or "" for none (the plain weapon).</summary>
        public string AspectOf(string kind) => TextOf(Aspects, kind);

        /// <summary>Sets the aspect chosen for a weapon kind; "" takes the choice away.</summary>
        public void SetAspect(string kind, string aspect) => SetText(Aspects, kind, aspect);

        /// <summary>The keystone set on a bough, or "" for none.</summary>
        public string KeystoneOf(string bough) => TextOf(Keystones, bough);

        /// <summary>Sets the keystone of a bough; "" takes it away.</summary>
        public void SetKeystone(string bough, string keystone) => SetText(Keystones, bough, keystone);

        /// <summary>The shape a spell or reaction wears: 0 (the first) unless 1 was chosen.</summary>
        public int ShapeOf(string id)
        {
            for (int i = 0; i < Shapes.Count; i++)
                if (Shapes[i].Id == id) return Shapes[i].Choice;
            return 0;
        }

        /// <summary>Sets the attunements; null and missing ones are taken as "".</summary>
        public void Attune(string first, string second)
        {
            if (Attunements == null || Attunements.Length < 2) Attunements = new string[2];
            Attunements[0] = first ?? "";
            Attunements[1] = second ?? "";
        }

        /// <summary>
        /// Copies only the Hand's own choices (slots, surge, Heresies, shapes, evolved spells) from another loadout: what a
        /// saved Hand swaps in. Never the attunements (picked in the hub) and never the weapon.
        /// </summary>
        public void CopyHandFrom(Loadout other)
        {
            if (other == null) return;
            CopyArray(other.Slots, ref Slots, SlotCount);
            Surge = other.Surge ?? "";
            CopyArray(other.Heresies, ref Heresies, 2);
            CopyChoices(other.Shapes, Shapes);
            CopyChoices(other.UseEvolved, UseEvolved);
        }

        /// <summary>Copies everything: the Hand, the attunements, the weapon and its aspects.</summary>
        public void CopyFrom(Loadout other)
        {
            if (other == null) return;
            CopyHandFrom(other);
            CopyArray(other.Attunements, ref Attunements, Math.Max(2, other.Attunements != null ? other.Attunements.Length : 2));
            Weapon = other.Weapon ?? "";
            Technique = other.Technique ?? "";
            CopyTexts(other.Aspects, Aspects);
            Keystones ??= new List<IdText>();
            CopyTexts(other.Keystones, Keystones);
        }

        /// <summary>A copy that shares nothing with this one: for a saved Hand.</summary>
        public Loadout Clone()
        {
            var copy = new Loadout();
            copy.CopyFrom(this);
            return copy;
        }

        private static void CopyArray(string[] from, ref string[] into, int length)
        {
            if (into == null || into.Length != length) into = new string[length];
            for (int i = 0; i < length; i++) into[i] = from != null && i < from.Length && from[i] != null ? from[i] : "";
        }

        private static string TextOf(List<IdText> list, string id)
        {
            if (list == null || string.IsNullOrEmpty(id)) return "";
            for (int i = 0; i < list.Count; i++)
                if (list[i] != null && list[i].Id == id) return list[i].Text ?? "";
            return "";
        }

        private static void SetText(List<IdText> list, string id, string text)
        {
            if (list == null || string.IsNullOrEmpty(id)) return;
            for (int i = list.Count - 1; i >= 0; i--)
                if (list[i] == null || list[i].Id == id) list.RemoveAt(i);
            if (!string.IsNullOrEmpty(text)) list.Add(new IdText(id, text));
        }

        private static void CopyTexts(List<IdText> from, List<IdText> into)
        {
            into.Clear();
            if (from == null) return;
            foreach (var pair in from)
                if (pair != null) into.Add(new IdText(pair.Id, pair.Text));
        }

        private static void CopyChoices(List<IdChoice> from, List<IdChoice> into)
        {
            into.Clear();
            if (from == null) return;
            foreach (var choice in from)
                if (choice != null) into.Add(new IdChoice(choice.Id, choice.Choice));
        }
    }
}

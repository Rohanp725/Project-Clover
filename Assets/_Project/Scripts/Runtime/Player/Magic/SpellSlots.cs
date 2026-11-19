using System;
using System.Collections.Generic;

namespace Clover.Player.Magic
{
    /// <summary>
    /// What sits in the four spell slots (West, North, East, South: Square, Triangle, Circle, Cross, or 1 to 4), by mark
    /// id. The element's own base spell is the id <c>&lt;element&gt;.base</c> (lower case), which no mark has: it is cast
    /// by the element's kit. Until the Hand exists (Phase 2) the slots fill themselves: the base spell first, then the
    /// learned spells of the element held in the chart's order; the chart's stopgap puts a learned spell in the next
    /// slot round. Plain C#, so the rules are tested without a scene.
    /// </summary>
    [Serializable]
    public class SpellSlots
    {
        public const int Count = 4;
        public const string BaseSuffix = ".base";

        private readonly string[] ids = { "", "", "", "" };
        private int next;

        /// <summary>
        /// How many slots are open, counted West, North, East, South (the Hand's schedule, <c>SlotSchedule</c>): a shut
        /// slot holds nothing, is never filled and is skipped by the chart's "next slot". All four until told otherwise.
        /// </summary>
        public int OpenCount { get; private set; } = Count;

        public static string BaseId(string element) => string.IsNullOrEmpty(element) ? "" : element.ToLowerInvariant() + BaseSuffix;
        public static bool IsBase(string id) => !string.IsNullOrEmpty(id) && id.EndsWith(BaseSuffix, StringComparison.Ordinal);

        public string this[int slot] => slot >= 0 && slot < Count ? ids[slot] : "";
        /// <summary>The slot the chart's "put in the next slot" fills next.</summary>
        public int Next => next;
        public bool IsEmpty(int slot) => string.IsNullOrEmpty(this[slot]);

        public int SlotOf(string id)
        {
            if (string.IsNullOrEmpty(id)) return -1;
            for (int i = 0; i < Count; i++)
                if (ids[i] == id) return i;
            return -1;
        }

        /// <summary>Puts a spell in one slot; if it sat in another it leaves that one, so no spell is in two slots.</summary>
        public void Set(int slot, string id)
        {
            if (slot < 0 || slot >= OpenCount) return;
            id ??= "";
            if (id.Length > 0)
                for (int i = 0; i < Count; i++)
                    if (i != slot && ids[i] == id) ids[i] = "";
            ids[slot] = id;
        }

        /// <summary>The chart's stopgap: the spell goes in the next slot round (West, North, East, South, West again). Returns the slot.</summary>
        public int PutInNext(string id)
        {
            if (next >= OpenCount) next = 0;
            int slot = next;
            Set(slot, id);
            next = (next + 1) % OpenCount;
            return slot;
        }

        /// <summary>Opens the first <paramref name="open"/> slots (1 to 4) and shuts the rest, emptying them. True when anything left a slot.</summary>
        public bool Shut(int open)
        {
            OpenCount = open < 1 ? 1 : open > Count ? Count : open;
            if (next >= OpenCount) next = 0;
            bool changed = false;
            for (int i = OpenCount; i < Count; i++)
            {
                if (ids[i].Length == 0) continue;
                ids[i] = "";
                changed = true;
            }
            return changed;
        }

        /// <summary>
        /// Makes the slots fit what the player has now: a spell that is no longer theirs (<paramref name="isValid"/> false)
        /// leaves its slot; another element's base spell becomes this one's in the same slot; the base spell is put in the
        /// first empty slot when it is in none; then empty slots take <paramref name="offered"/> spells, in order, that sit
        /// in no slot yet. True when anything moved.
        /// </summary>
        public bool Settle(string baseId, Func<string, bool> isValid, IReadOnlyList<string> offered) => Settle(baseId, "", isValid, offered, true);

        /// <summary>
        /// The same with two elements carried (the Hand, phase 2): both base spells are kept, and with
        /// <paramref name="fill"/> each is put in an empty slot when it is in none, then the offered spells. Without it
        /// (once a Hand has been set at a rest fire) nothing is added: only what is no longer theirs leaves.
        /// </summary>
        public bool Settle(string baseId, string secondBaseId, Func<string, bool> isValid, IReadOnlyList<string> offered, bool fill)
        {
            baseId ??= "";
            secondBaseId ??= "";
            bool changed = false;
            for (int i = 0; i < Count; i++)
            {
                string id = ids[i];
                if (id.Length == 0 || id == baseId || (secondBaseId.Length > 0 && id == secondBaseId)) continue;
                if (IsBase(id))
                {
                    // Another element's base spell becomes this one's in the same slot, while the slots still fill themselves.
                    ids[i] = fill && baseId.Length > 0 && SlotOf(baseId) < 0 ? baseId : fill && secondBaseId.Length > 0 && SlotOf(secondBaseId) < 0 ? secondBaseId : "";
                    changed = true;
                }
                else if (isValid == null || !isValid(id))
                {
                    ids[i] = "";
                    changed = true;
                }
            }
            if (!fill) return changed;
            changed |= PlaceIfMissing(baseId);
            changed |= PlaceIfMissing(secondBaseId);
            if (offered != null)
                for (int k = 0; k < offered.Count; k++)
                {
                    string id = offered[k];
                    if (string.IsNullOrEmpty(id) || SlotOf(id) >= 0) continue;
                    int empty = FirstEmpty();
                    if (empty < 0) break;
                    ids[empty] = id;
                    changed = true;
                }
            return changed;
        }

        private bool PlaceIfMissing(string id)
        {
            if (id.Length == 0 || SlotOf(id) >= 0) return false;
            int empty = FirstEmpty();
            if (empty < 0) return false;
            ids[empty] = id;
            return true;
        }

        public int FirstEmpty()
        {
            for (int i = 0; i < OpenCount; i++)
                if (ids[i].Length == 0) return i;
            return -1;
        }

        public void Clear()
        {
            for (int i = 0; i < Count; i++) ids[i] = "";
            next = 0;
        }

        /// <summary>Copies the slots out (to be carried to the next scene) or back in.</summary>
        public void CopyTo(string[] into)
        {
            if (into == null) return;
            for (int i = 0; i < Count && i < into.Length; i++) into[i] = ids[i];
        }

        public void CopyFrom(string[] from)
        {
            for (int i = 0; i < Count; i++) ids[i] = from != null && i < from.Length && from[i] != null ? from[i] : "";
        }
    }
}

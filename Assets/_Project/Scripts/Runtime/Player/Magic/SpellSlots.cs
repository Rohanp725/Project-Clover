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
            if (slot < 0 || slot >= Count) return;
            id ??= "";
            if (id.Length > 0)
                for (int i = 0; i < Count; i++)
                    if (i != slot && ids[i] == id) ids[i] = "";
            ids[slot] = id;
        }

        /// <summary>The chart's stopgap: the spell goes in the next slot round (West, North, East, South, West again). Returns the slot.</summary>
        public int PutInNext(string id)
        {
            int slot = next;
            Set(slot, id);
            next = (next + 1) % Count;
            return slot;
        }

        /// <summary>
        /// Makes the slots fit what the player has now: a spell that is no longer theirs (<paramref name="isValid"/> false)
        /// leaves its slot; another element's base spell becomes this one's in the same slot; the base spell is put in the
        /// first empty slot when it is in none; then empty slots take <paramref name="offered"/> spells, in order, that sit
        /// in no slot yet. True when anything moved.
        /// </summary>
        public bool Settle(string baseId, Func<string, bool> isValid, IReadOnlyList<string> offered)
        {
            baseId ??= "";
            bool changed = false;
            for (int i = 0; i < Count; i++)
            {
                string id = ids[i];
                if (id.Length == 0 || id == baseId) continue;
                if (IsBase(id))
                {
                    ids[i] = baseId.Length > 0 && SlotOf(baseId) < 0 ? baseId : "";
                    changed = true;
                }
                else if (isValid == null || !isValid(id))
                {
                    ids[i] = "";
                    changed = true;
                }
            }
            if (baseId.Length > 0 && SlotOf(baseId) < 0)
            {
                int empty = FirstEmpty();
                if (empty >= 0)
                {
                    ids[empty] = baseId;
                    changed = true;
                }
            }
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

        public int FirstEmpty()
        {
            for (int i = 0; i < Count; i++)
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

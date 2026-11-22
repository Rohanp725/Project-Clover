using System.Collections.Generic;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// What a moving spell carries before it (<see cref="SpellSpec.Carries"/>; the ledger's carried-along): each enemy it
    /// has struck is kept at the front of the spell as it goes, and thrown down where the spell ends. Only those carrying
    /// the spec's <see cref="SpellSpec.OnlyCarrying"/>, when it names one (The Black Buffalo: the Cursed).
    /// </summary>
    public static class FormCarry
    {
        /// <summary>Seconds a carried enemy lies where it is thrown down.</summary>
        public const float DownSeconds = 0.8f;

        /// <summary>Keeps every carried one at <paramref name="front"/>, spread a little across it so they do not stand in one place.</summary>
        public static int Carry(HashSet<IDamageable> struck, Vector3 front, Vector3 forward, AfflictionKind only)
        {
            if (struck == null) return 0;
            Vector3 across = Vector3.Cross(Vector3.up, forward);
            int carried = 0;
            foreach (var one in struck)
            {
                if (!Takes(one, only) || !(one is ICarriable carriable)) continue;
                Vector3 at = front + across * Spread(carried);
                if (carriable.CarryTo(at)) carried++;
            }
            return carried;
        }

        /// <summary>Throws down every carried one where it stands. Returns how many went down.</summary>
        public static int ThrowDown(HashSet<IDamageable> struck, AfflictionKind only)
        {
            if (struck == null) return 0;
            int down = 0;
            foreach (var one in struck)
                if (Takes(one, only) && one is ICarriable carriable && carriable.ThrowDown(DownSeconds)) down++;
            return down;
        }

        /// <summary>Metres to the side of the front the carried one numbered <paramref name="index"/> is kept: the middle, then right and left by turns.</summary>
        public static float Spread(int index) => index == 0 ? 0f : (index % 2 == 1 ? 1f : -1f) * 0.7f * ((index + 1) / 2);

        private static bool Takes(IDamageable one, AfflictionKind only) =>
            one != null && (only == AfflictionKind.None || (one is IAfflictable afflictable && afflictable.Afflictions.Has(only)));
    }
}

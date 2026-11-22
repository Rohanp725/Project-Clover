using Clover.Core;
using Clover.Gameplay;
using Clover.Player.Growth;
using Clover.UI;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// Memories paid in play (task 5.6; way_build_plan.md 2.6): the world's hooks (an area's deaths, a god's fall, a cache
    /// found) ask <see cref="Income"/> what is owed, put it in the save's wallet under its key so it is never paid twice,
    /// write the save, and send the amber light that shows it (<see cref="MemoryGain"/>). Stateless; each hook keeps its
    /// own <see cref="Income"/>.
    /// </summary>
    public static class MemoryPayout
    {
        /// <summary>Seconds a god's light waits, so it never shows over the fall's own moment.</summary>
        public const float GodDelay = 2.5f;

        /// <summary>One of an area's enemies has died (a pen's never come here). Returns the Memories paid.</summary>
        public static int Kill(Income income, EnemyKind kind, bool areasOwn, Vector3 body, Transform to)
        {
            var data = Progress.Data;
            return Shown(income.PayKill(kind, areasOwn, data.KindsDefeated, data.Wallet), body + Vector3.up, to, 0f);
        }

        /// <summary>A god has fallen. Returns the Memories paid.</summary>
        public static int GodFall(Income income, string element, Vector3 body, Transform to)
        {
            var data = Progress.Data;
            return Shown(income.PayGodFall(element, data.KindsDefeated, data.Wallet), body + Vector3.up, to, GodDelay);
        }

        /// <summary>Memories found lying in the world. Returns the Memories paid.</summary>
        public static int Found(Income income, string key, bool duoLand, Vector3 at, Transform to)
        {
            var data = Progress.Data;
            return Shown(income.PayFound(key, duoLand, data.Found, data.Wallet), at, to, 0f);
        }

        private static int Shown(int paid, Vector3 from, Transform to, float delay)
        {
            if (paid <= 0) return 0;
            // Memories are never lost: what is paid is written at once.
            Progress.Save();
            MemoryGain.Fly(from, to, paid, delay);
            return paid;
        }
    }
}

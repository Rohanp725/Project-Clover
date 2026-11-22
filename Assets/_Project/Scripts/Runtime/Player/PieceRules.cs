using UnityEngine;

namespace Clover.Player
{
    /// <summary>How the pieces of an UpgradeAction.Pieces leave the enemy the moment names (the ledger's thrown pieces, pass 11).</summary>
    public enum PieceAim
    {
        /// <summary>A fan away from you, Arc degrees across: feathers riding the wind beyond it.</summary>
        Fan,
        /// <summary>Evenly all round it: blades or shards bursting out of it.</summary>
        Ring,
        /// <summary>Each at one of the nearest other enemies within Reach, turning after it.</summary>
        AtNearest,
        /// <summary>A cone out of its far side, away from you, Reach long and Arc wide: struck at once, not thrown.</summary>
        FarCone,
    }

    /// <summary>The plain rules of thrown pieces: the ways they leave.</summary>
    public static class PieceRules
    {
        /// <summary>A shove this recent still sets the way things ride off an enemy ("downwind").</summary>
        public const float ShoveKept = 3f;

        /// <summary>The way pieces ride off an enemy: the way it was last shoved, if lately, else away from you.</summary>
        public static Vector3 Downwind(Vector3 awayFromYou, Vector3 lastShove, float shovedAt, float now) =>
            lastShove.sqrMagnitude > 0.0001f && now - shovedAt <= ShoveKept ? lastShove : awayFromYou;

        /// <summary>
        /// Writes into <paramref name="ways"/> the way each of <paramref name="count"/> pieces leaves: a fan
        /// <paramref name="arc"/> degrees across centred on <paramref name="away"/>, or a ring all round starting from it.
        /// Returns how many were written.
        /// </summary>
        public static int Ways(PieceAim aim, Vector3 away, int count, float arc, Vector3[] ways)
        {
            away.y = 0f;
            away = away.sqrMagnitude > 0.0001f ? away.normalized : Vector3.forward;
            count = Mathf.Clamp(count, 1, ways.Length);
            for (int i = 0; i < count; i++)
            {
                float turn = aim == PieceAim.Ring ? i * 360f / count : count == 1 ? 0f : -arc * 0.5f + arc * i / (count - 1);
                ways[i] = Yaw(away, turn);
            }
            return count;
        }

        /// <summary>A flat way turned <paramref name="degrees"/> about the up axis, as a turn of that many degrees about Y turns it.</summary>
        public static Vector3 Yaw(Vector3 way, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad, cos = Mathf.Cos(rad), sin = Mathf.Sin(rad);
            return new Vector3(way.x * cos + way.z * sin, way.y, -way.x * sin + way.z * cos);
        }
    }
}

using System.Collections.Generic;

namespace Clover.Player.Way
{
    /// <summary>The three kinds of Body piece found in the world.</summary>
    public enum BodyPiece
    {
        None,
        /// <summary>+5 health.</summary>
        Vessel,
        /// <summary>+5 stamina.</summary>
        BreathStone,
        /// <summary>Three make a flask.</summary>
        FlaskShard,
    }

    /// <summary>
    /// The Body's found pieces (way_plan.md 0.4; way_build_plan.md 2.4 and question 8; task 5.4): about one in each land
    /// and one from each god, as Sekiro's prayer beads are. A vessel is +5 health, a breath stone +5 stamina, and three
    /// flask shards make a flask. They add on top of the Body's marks, are never needed, are always in force, and are kept
    /// for good under their own key in the save (<c>SaveData.Found</c>): <c>body.vessel.fire</c>. Where the Fire land's go
    /// is decided once its levels are signed off; the pickup is <c>World/BodyPieceCache</c>. Plain and stateless.
    /// </summary>
    public static class BodyPieces
    {
        public const string VesselPrefix = "body.vessel.";
        public const string BreathPrefix = "body.breath.";
        public const string ShardPrefix = "body.shard.";
        public const float VesselHealth = 5f;
        public const float BreathStamina = 5f;
        public const int ShardsToAFlask = 3;

        /// <summary>The key a piece is kept under: its kind's prefix and where it was found (<c>fire</c>, <c>fire.god</c>).</summary>
        public static string KeyOf(BodyPiece piece, string where)
        {
            string prefix = PrefixOf(piece);
            return prefix == null || string.IsNullOrEmpty(where) ? "" : prefix + where;
        }

        /// <summary>The kind of piece a key names, or None.</summary>
        public static BodyPiece KindOf(string key)
        {
            if (string.IsNullOrEmpty(key)) return BodyPiece.None;
            if (key.StartsWith(VesselPrefix, System.StringComparison.Ordinal) && key.Length > VesselPrefix.Length) return BodyPiece.Vessel;
            if (key.StartsWith(BreathPrefix, System.StringComparison.Ordinal) && key.Length > BreathPrefix.Length) return BodyPiece.BreathStone;
            if (key.StartsWith(ShardPrefix, System.StringComparison.Ordinal) && key.Length > ShardPrefix.Length) return BodyPiece.FlaskShard;
            return BodyPiece.None;
        }

        /// <summary>Takes a piece: kept under its key, once. False for a key that is no piece or one already found.</summary>
        public static bool Take(string key, ICollection<string> found)
        {
            if (found == null || KindOf(key) == BodyPiece.None || found.Contains(key)) return false;
            found.Add(key);
            return true;
        }

        /// <summary>How many pieces of one kind have been found.</summary>
        public static int Count(BodyPiece piece, IEnumerable<string> found)
        {
            if (found == null || piece == BodyPiece.None) return 0;
            int count = 0;
            foreach (string key in found)
                if (KindOf(key) == piece) count++;
            return count;
        }

        /// <summary>What every piece found adds, as stats on top of the Body's marks: the health, the stamina and the whole flasks.</summary>
        public static StatMod[] StatsOf(IEnumerable<string> found)
        {
            int vessels = Count(BodyPiece.Vessel, found);
            int stones = Count(BodyPiece.BreathStone, found);
            int flasks = Count(BodyPiece.FlaskShard, found) / ShardsToAFlask;
            var stats = new List<StatMod>(3);
            if (vessels > 0) stats.Add(new StatMod(PlayerStat.Health, VesselHealth * vessels));
            if (stones > 0) stats.Add(new StatMod(PlayerStat.StaminaMost, BreathStamina * stones));
            if (flasks > 0) stats.Add(new StatMod(PlayerStat.FlaskCount, flasks));
            return stats.ToArray();
        }

        /// <summary>The words shown when a piece is taken, short and plain.</summary>
        public static string WordsOf(BodyPiece piece)
        {
            switch (piece)
            {
                case BodyPiece.Vessel: return "A vessel. More health.";
                case BodyPiece.BreathStone: return "A breath stone. More stamina.";
                case BodyPiece.FlaskShard: return "A flask shard. Three make a flask.";
                default: return "";
            }
        }

        private static string PrefixOf(BodyPiece piece)
        {
            switch (piece)
            {
                case BodyPiece.Vessel: return VesselPrefix;
                case BodyPiece.BreathStone: return BreathPrefix;
                case BodyPiece.FlaskShard: return ShardPrefix;
                default: return null;
            }
        }
    }
}

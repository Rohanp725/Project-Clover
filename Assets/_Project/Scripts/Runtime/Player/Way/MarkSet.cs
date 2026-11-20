using System.Collections.Generic;

namespace Clover.Player.Way
{
    /// <summary>A plain set of mark ids taken as the marks in force: for tests, previews and the playtest's armoury.</summary>
    public sealed class MarkSet : IMarkQuery
    {
        /// <summary>No marks at all: the bare weapon and the bare aspect.</summary>
        public static readonly MarkSet None = new MarkSet();

        private readonly HashSet<string> ids = new HashSet<string>();

        public MarkSet(params string[] inForce)
        {
            if (inForce == null) return;
            foreach (string id in inForce)
                if (!string.IsNullOrEmpty(id)) ids.Add(id);
        }

        public MarkSet(IEnumerable<string> inForce)
        {
            if (inForce == null) return;
            foreach (string id in inForce)
                if (!string.IsNullOrEmpty(id)) ids.Add(id);
        }

        public bool Has(string id) => id != null && ids.Contains(id);
    }
}

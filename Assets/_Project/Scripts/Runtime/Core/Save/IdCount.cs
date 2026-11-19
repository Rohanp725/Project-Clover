using System;

namespace Clover.Core.Save
{
    /// <summary>
    /// A count kept by id: what a mark tallies for good (the folds of a blade; <c>RunState.Tallies</c> today). A pair, not
    /// a dictionary, so <c>JsonUtility</c> can write it into a save.
    /// </summary>
    [Serializable]
    public class IdCount
    {
        public string Id = "";
        public int Count;

        public IdCount() { }

        public IdCount(string id, int count)
        {
            Id = id ?? "";
            Count = count;
        }
    }
}

using System;

namespace Clover.Core.Save
{
    /// <summary>
    /// Points kept by id: Bond with a weapon, an element or a pair (phase 6). A pair, not a dictionary, so
    /// <c>JsonUtility</c> can write it into a save.
    /// </summary>
    [Serializable]
    public class IdPoints
    {
        public string Id = "";
        public int Points;

        public IdPoints() { }

        public IdPoints(string id, int points)
        {
            Id = id ?? "";
            Points = points;
        }
    }
}

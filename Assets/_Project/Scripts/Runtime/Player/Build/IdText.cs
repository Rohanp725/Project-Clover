using System;

namespace Clover.Player.Build
{
    /// <summary>
    /// One word kept by id: which aspect of a weapon kind is active (the Way, phase 5). A pair, not a dictionary, so
    /// <c>JsonUtility</c> can write it into a save.
    /// </summary>
    [Serializable]
    public class IdText
    {
        public string Id = "";
        public string Text = "";

        public IdText() { }

        public IdText(string id, string text)
        {
            Id = id ?? "";
            Text = text ?? "";
        }
    }
}

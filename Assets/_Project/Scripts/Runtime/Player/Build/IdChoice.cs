using System;

namespace Clover.Player.Build
{
    /// <summary>
    /// One choice kept by id: which of a spell's or a reaction's two shapes is worn (0 or 1), or whether a spell's evolved
    /// form is used (1). A pair, not a dictionary, so <c>JsonUtility</c> can write it into a save.
    /// </summary>
    [Serializable]
    public class IdChoice
    {
        public string Id = "";
        public int Choice;

        public IdChoice() { }

        public IdChoice(string id, int choice)
        {
            Id = id ?? "";
            Choice = choice;
        }
    }
}

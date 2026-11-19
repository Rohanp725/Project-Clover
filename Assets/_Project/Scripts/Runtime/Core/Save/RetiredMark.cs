namespace Clover.Core.Save
{
    /// <summary>
    /// A mark cut from the game after a save may hold it (IMPLEMENTATION.md 8.6): its id, never reused, and the price it
    /// cost, which a save that learned it gets back. The price is kept here because the mark is gone from the trees.
    /// </summary>
    public readonly struct RetiredMark
    {
        public readonly string Id;
        public readonly int Price;

        public RetiredMark(string id, int price)
        {
            Id = id ?? "";
            Price = price;
        }
    }
}

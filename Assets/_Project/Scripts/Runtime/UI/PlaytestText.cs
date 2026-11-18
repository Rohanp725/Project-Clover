namespace Clover.UI
{
    /// <summary>One entry of the playtest's written texts: an effect in the guide, a mark's details, or what is new.</summary>
    [System.Serializable]
    public class PlaytestText
    {
        /// <summary>The id after the "## " of its section: an effect's short name, or a mark's id.</summary>
        public string Id = "";
        public string Title = "";
        /// <summary>Which part of the guide it belongs in (Afflictions, States, Resources, Reactions); empty for a mark's details.</summary>
        public string Group = "";
        /// <summary>The words, in the reading box's rich text (bold kept, bullets as dots).</summary>
        public string Body = "";
    }
}

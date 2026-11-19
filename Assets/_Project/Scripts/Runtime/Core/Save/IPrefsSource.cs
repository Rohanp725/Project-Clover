namespace Clover.Core.Save
{
    /// <summary>
    /// The old settings store, as the save reads it: <see cref="PlayerPrefsSource"/> in the game, a dictionary in tests.
    /// It has no way to write, so the save can never change what an older build still reads (IMPLEMENTATION.md 5.2).
    /// </summary>
    public interface IPrefsSource
    {
        bool HasKey(string key);
        int GetInt(string key, int fallback);
    }
}

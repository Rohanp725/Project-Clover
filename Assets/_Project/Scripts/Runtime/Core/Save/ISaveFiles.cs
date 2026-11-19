namespace Clover.Core.Save
{
    /// <summary>
    /// The few things <see cref="SaveStore"/> does to files, by name within one folder: <see cref="DiskSaveFiles"/> in the
    /// game, a dictionary in tests, so no test ever touches the real disk.
    /// </summary>
    public interface ISaveFiles
    {
        bool Exists(string name);
        string Read(string name);
        /// <summary>Writes the whole text, creating or replacing the file, and returns only once it is on the disk.</summary>
        void Write(string name, string text);
        /// <summary>Puts file <paramref name="from"/> in the place of <paramref name="into"/> (replacing it) in one step; <paramref name="from"/> is gone after.</summary>
        void Swap(string from, string into);
    }
}

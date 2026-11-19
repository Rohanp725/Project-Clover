using System.Collections.Generic;
using System.IO;
using Clover.Core.Save;

namespace Clover.Tests
{
    /// <summary>Save files for the save's tests: a dictionary of names to text, with every touch written in <see cref="Log"/>.</summary>
    public class FakeSaveFiles : ISaveFiles
    {
        public readonly Dictionary<string, string> Files = new Dictionary<string, string>();
        /// <summary>Each call in order: "exists name", "read name", "write name", "swap from>into".</summary>
        public readonly List<string> Log = new List<string>();
        /// <summary>When set, the next swap throws, as a disk might.</summary>
        public bool FailNextSwap;

        public bool Exists(string name)
        {
            Log.Add("exists " + name);
            return Files.ContainsKey(name);
        }

        public string Read(string name)
        {
            Log.Add("read " + name);
            if (!Files.TryGetValue(name, out string text)) throw new FileNotFoundException(name);
            return text;
        }

        public void Write(string name, string text)
        {
            Log.Add("write " + name);
            Files[name] = text;
        }

        public void Swap(string from, string into)
        {
            Log.Add("swap " + from + ">" + into);
            if (FailNextSwap)
            {
                FailNextSwap = false;
                throw new IOException("the disk refused");
            }
            if (!Files.TryGetValue(from, out string text)) throw new FileNotFoundException(from);
            Files.Remove(from);
            Files[into] = text;
        }
    }
}

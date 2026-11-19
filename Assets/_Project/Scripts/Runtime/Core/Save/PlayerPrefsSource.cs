using UnityEngine;

namespace Clover.Core.Save
{
    /// <summary>The machine's PlayerPrefs, read only.</summary>
    public sealed class PlayerPrefsSource : IPrefsSource
    {
        public bool HasKey(string key) => PlayerPrefs.HasKey(key);
        public int GetInt(string key, int fallback) => PlayerPrefs.GetInt(key, fallback);
    }
}

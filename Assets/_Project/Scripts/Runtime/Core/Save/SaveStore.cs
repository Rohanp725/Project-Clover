using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Clover.Core.Save
{
    /// <summary>
    /// Where the save lives (IMPLEMENTATION.md 5.1). The game proper keeps <see cref="GameFile"/> and the playtest its own
    /// <see cref="PlaytestFile"/>, so a tester's game is never mixed with the free chart. A sandboxed play (the probes,
    /// <see cref="Progress.Sandboxed"/>) gets a save in memory that starts empty and never touches a file or PlayerPrefs.
    /// Each file is read when first wanted: made from PlayerPrefs if it does not exist yet, brought up to date if it does.
    /// Writes go to a temporary file that is then swapped in, so a crash mid-write leaves the last good save in place.
    /// </summary>
    public sealed class SaveStore
    {
        public const string GameFile = "clover_save.json";
        public const string PlaytestFile = "clover_playtest.json";
        /// <summary>Added to a file's name for the copy being written before it is swapped in.</summary>
        public const string TempSuffix = ".tmp";
        /// <summary>Added to the name of a file that could not be read, kept beside the new one rather than written over.</summary>
        public const string BrokenSuffix = ".broken";

        private readonly ISaveFiles files;
        private readonly IPrefsSource prefs;
        private readonly string[] knownFlags;
        private SaveData game, playtest, sandbox;

        /// <summary>Files written, for tests and probes.</summary>
        public int Writes { get; private set; }
        /// <summary>Saves made from PlayerPrefs because no file existed, for tests and probes.</summary>
        public int Migrations { get; private set; }

        public SaveStore(ISaveFiles files, IPrefsSource prefs, IEnumerable<string> knownFlags)
        {
            this.files = files;
            this.prefs = prefs;
            this.knownFlags = knownFlags != null ? knownFlags.ToArray() : Array.Empty<string>();
        }

        /// <summary>The store the game uses: files in <c>Application.persistentDataPath</c>, the machine's PlayerPrefs.</summary>
        public static SaveStore ForThisMachine() =>
            new SaveStore(new DiskSaveFiles(Application.persistentDataPath), new PlayerPrefsSource(), SaveMigration.KnownFlags());

        /// <summary>The file the game proper or the playtest keeps.</summary>
        public static string FileFor(bool playtest) => playtest ? PlaytestFile : GameFile;

        /// <summary>The save in force: the sandbox's while sandboxed, else the playtest's or the game's, read when first wanted.</summary>
        public SaveData Get(bool sandboxed, bool playtestOn)
        {
            if (sandboxed) return sandbox ??= new SaveData();
            if (playtestOn) return playtest ??= Load(PlaytestFile);
            return game ??= Load(GameFile);
        }

        /// <summary>True when the save is the sandbox's, which never reaches a file or PlayerPrefs.</summary>
        public bool IsSandbox(SaveData data) => data != null && ReferenceEquals(data, sandbox);

        /// <summary>
        /// Writes the save in force to its file. Nothing is written while sandboxed, or for a file never read in this
        /// play. False when nothing was written or the write failed (the old file is then still there).
        /// </summary>
        public bool Save(bool sandboxed, bool playtestOn)
        {
            if (sandboxed) return false;
            var data = playtestOn ? playtest : game;
            return data != null && Write(FileFor(playtestOn), data);
        }

        /// <summary>Whether a flag is set in this save, asking PlayerPrefs once if the save has never asked (<see cref="SaveMigration.AskPrefs"/>); the sandbox never asks.</summary>
        public bool AskPrefs(SaveData data, string key) =>
            IsSandbox(data) ? data.HasFlag(key) : SaveMigration.AskPrefs(data, prefs, key);

        private SaveData Load(string file)
        {
            bool unreadableInTheWay = false;
            if (files.Exists(file))
            {
                var read = Parse(file);
                if (read != null) return SaveMigration.Upgrade(read);
                // Unreadable: kept beside the new save for someone to look at rather than written over at once.
                Debug.LogWarning($"[Clover.Core] The save {file} could not be read; it is kept as {file + BrokenSuffix} and a new one is made.");
                unreadableInTheWay = !TrySwap(file, file + BrokenSuffix);
            }
            else if (files.Exists(file + TempSuffix))
            {
                // The temporary copy is swapped in only once it is whole, so a whole one with no file beside it is a
                // write cut short between the two steps: it is the newest save.
                var whole = Parse(file + TempSuffix);
                if (whole != null && TrySwap(file + TempSuffix, file)) return SaveMigration.Upgrade(whole);
            }
            var fresh = SaveMigration.FromPrefs(prefs, knownFlags);
            Migrations++;
            // An unreadable file that could not be moved aside waits for the next save point instead.
            if (!unreadableInTheWay) Write(file, fresh);
            return fresh;
        }

        private SaveData Parse(string file)
        {
            try
            {
                string text = files.Read(file);
                return string.IsNullOrWhiteSpace(text) ? null : JsonUtility.FromJson<SaveData>(text);
            }
            catch (Exception)
            {
                return null;
            }
        }

        private bool Write(string file, SaveData data)
        {
            string temp = file + TempSuffix;
            try
            {
                files.Write(temp, JsonUtility.ToJson(data, true));
                files.Swap(temp, file);
                Writes++;
                return true;
            }
            catch (Exception exception)
            {
                Debug.LogError($"[Clover.Core] The save {file} could not be written ({exception.Message}); the last one written is kept.");
                return false;
            }
        }

        private bool TrySwap(string from, string into)
        {
            try
            {
                files.Swap(from, into);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}

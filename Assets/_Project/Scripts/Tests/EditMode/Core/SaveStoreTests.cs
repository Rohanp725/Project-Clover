using System.Linq;
using System.Text.RegularExpressions;
using Clover.Core;
using Clover.Core.Save;
using Clover.Player;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace Clover.Tests
{
    /// <summary>
    /// Where the save is kept (IMPLEMENTATION.md 5.1), over fake files and a fake PlayerPrefs: a sandboxed play never
    /// touches either, every write goes to a temporary file that is then swapped in, and the playtest keeps its own file.
    /// </summary>
    public class SaveStoreTests
    {
        private const string Game = SaveStore.GameFile;
        private const string GameTemp = SaveStore.GameFile + SaveStore.TempSuffix;
        private const string Playtest = SaveStore.PlaytestFile;

        private FakeSaveFiles files;
        private FakePrefs prefs;

        [SetUp]
        public void MakeFakes()
        {
            files = new FakeSaveFiles();
            prefs = new FakePrefs().Flag(Progress.WeaponKey(Weapons.KatanaName)).Flag("door.north");
        }

        private SaveStore NewStore() => new SaveStore(files, prefs, SaveMigration.KnownFlags());

        [Test]
        public void ASandboxedPlayNeverTouchesAFileOrPrefs()
        {
            var store = NewStore();
            var data = store.Get(sandboxed: true, playtestOn: false);
            Assert.IsTrue(store.IsSandbox(data));
            Assert.AreEqual(0, data.Flags.Count, "a sandbox starts as a player who has done nothing");
            Assert.IsFalse(store.AskPrefs(data, "door.north"), "the sandbox never asks PlayerPrefs");
            data.SetFlag("door.south");
            Assert.IsFalse(store.Save(sandboxed: true, playtestOn: false));
            Assert.IsFalse(store.Save(sandboxed: true, playtestOn: true));

            CollectionAssert.IsEmpty(files.Log);
            Assert.AreEqual(0, prefs.Reads);
            Assert.AreEqual(0, store.Writes);
            Assert.AreSame(data, store.Get(sandboxed: true, playtestOn: true), "one sandbox, playtest or not");
        }

        [Test]
        public void TheFirstReadMakesTheSaveFromPrefsAndWritesItThroughATemporaryFile()
        {
            var store = NewStore();
            var data = store.Get(sandboxed: false, playtestOn: false);

            Assert.AreEqual(1, store.Migrations);
            Assert.IsTrue(data.HasFlag(Progress.WeaponKey(Weapons.KatanaName)));
            CollectionAssert.AreEqual(new[] { "exists " + Game, "exists " + GameTemp, "write " + GameTemp, "swap " + GameTemp + ">" + Game }, files.Log);
            Assert.IsTrue(files.Files.ContainsKey(Game));
            Assert.IsFalse(files.Files.ContainsKey(GameTemp), "the temporary file is gone once swapped in");
        }

        [Test]
        public void ASaveIsWrittenToTheTemporaryFileAndThenSwappedIn()
        {
            var store = NewStore();
            store.Get(false, false).SetFlag("door.south");
            files.Log.Clear();

            Assert.IsTrue(store.Save(false, false));
            CollectionAssert.AreEqual(new[] { "write " + GameTemp, "swap " + GameTemp + ">" + Game }, files.Log);
            var written = JsonUtility.FromJson<SaveData>(files.Files[Game]);
            Assert.IsTrue(written.HasFlag("door.south"));
            Assert.IsFalse(files.Files.ContainsKey(GameTemp));
        }

        [Test]
        public void AFailedSwapLeavesTheLastSaveInPlace()
        {
            var store = NewStore();
            store.Get(false, false);
            string before = files.Files[Game];

            store.Get(false, false).SetFlag("door.south");
            files.FailNextSwap = true;
            LogAssert.Expect(LogType.Error, new Regex("could not be written"));
            Assert.IsFalse(store.Save(false, false));
            Assert.AreEqual(before, files.Files[Game]);
        }

        [Test]
        public void ThePrefsAreReadOnlyOnceWhenNoFileExists()
        {
            NewStore().Get(false, false);
            // An older build sets another known flag afterwards; the save already asked about it and keeps its answer.
            prefs.Flag(Progress.WeaponKey(Weapons.ScytheName));
            var before = prefs.Snapshot();

            var again = NewStore();
            var data = again.Get(false, false);
            Assert.AreEqual(0, again.Migrations);
            Assert.IsTrue(data.HasFlag(Progress.WeaponKey(Weapons.KatanaName)), "read back from the file");
            Assert.IsFalse(again.AskPrefs(data, Progress.WeaponKey(Weapons.ScytheName)));
            CollectionAssert.AreEquivalent(before, prefs.Ints, "PlayerPrefs is never written");
        }

        [Test]
        public void ThePlaytestKeepsItsOwnFile()
        {
            var store = NewStore();
            var playtest = store.Get(sandboxed: false, playtestOn: true);
            playtest.SetFlag("door.south");
            Assert.IsTrue(store.Save(sandboxed: false, playtestOn: true));

            Assert.IsTrue(files.Files.ContainsKey(Playtest));
            Assert.IsFalse(files.Files.ContainsKey(Game), "the game's file is not written by the playtest");
            Assert.IsFalse(files.Log.Any(line => line.Contains(Game)), "nor even looked at");

            var game = store.Get(sandboxed: false, playtestOn: false);
            Assert.AreNotSame(playtest, game);
            Assert.IsFalse(game.HasFlag("door.south"));
            Assert.IsTrue(game.HasFlag(Progress.WeaponKey(Weapons.KatanaName)), "each file starts from the same PlayerPrefs");
            Assert.AreEqual(SaveStore.PlaytestFile, SaveStore.FileFor(true));
            Assert.AreEqual(SaveStore.GameFile, SaveStore.FileFor(false));
        }

        [Test]
        public void NothingIsWrittenForAFileNeverRead()
        {
            var store = NewStore();
            Assert.IsFalse(store.Save(false, false));
            Assert.IsFalse(store.Save(false, true));
            CollectionAssert.IsEmpty(files.Log);
        }

        [Test]
        public void AnUnreadableFileIsKeptAsideAndANewSaveMade()
        {
            files.Files[Game] = "{ not a save";
            LogAssert.Expect(LogType.Warning, new Regex("could not be read"));
            var data = NewStore().Get(false, false);

            Assert.AreEqual("{ not a save", files.Files[Game + SaveStore.BrokenSuffix]);
            Assert.IsTrue(data.HasFlag(Progress.WeaponKey(Weapons.KatanaName)));
            Assert.IsTrue(JsonUtility.FromJson<SaveData>(files.Files[Game]).HasFlag(Progress.WeaponKey(Weapons.KatanaName)));
        }

        [Test]
        public void AWholeTemporaryFileLeftByACutWriteIsTheSave()
        {
            var left = new SaveData();
            left.SetFlag("door.south");
            files.Files[GameTemp] = JsonUtility.ToJson(left);

            var store = NewStore();
            var data = store.Get(false, false);
            Assert.AreEqual(0, store.Migrations);
            Assert.IsTrue(data.HasFlag("door.south"));
            Assert.IsTrue(files.Files.ContainsKey(Game));
            Assert.IsFalse(files.Files.ContainsKey(GameTemp));
        }
    }
}

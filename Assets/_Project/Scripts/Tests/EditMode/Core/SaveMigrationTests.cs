using System.Linq;
using Clover.Core;
using Clover.Core.Save;
using Clover.Player;
using Clover.Player.Build;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// A tester's PlayerPrefs becoming a save (IMPLEMENTATION.md 5.2), from a fake PlayerPrefs: the flags copied, the owned
    /// lists built, the same result however often it runs, and PlayerPrefs left as it was.
    /// </summary>
    public class SaveMigrationTests
    {
        private static FakePrefs ATestersPrefs() => new FakePrefs()
            .Flag(Progress.MetSponsor)
            .Flag(Progress.WeaponKey(Weapons.KatanaName))
            .Flag(Progress.WeaponKey(Weapons.StormKatanaName))
            .Flag(Progress.ElementKey(Elements.Lightning))
            .Flag(SlotSchedule.ChampionFlag);

        [Test]
        public void EveryKnownFlagSetInPrefsIsCopied()
        {
            var data = SaveMigration.FromPrefs(ATestersPrefs(), SaveMigration.KnownFlags());
            CollectionAssert.AreEquivalent(new[]
            {
                Progress.MetSponsor, Progress.WeaponKey(Weapons.KatanaName), Progress.WeaponKey(Weapons.StormKatanaName),
                Progress.ElementKey(Elements.Lightning), SlotSchedule.ChampionFlag,
            }, data.Flags);
            Assert.AreEqual(SaveData.CurrentVersion, data.Version);
        }

        [Test]
        public void WeaponAndElementFlagsBecomeTheOwnedLists()
        {
            var data = SaveMigration.FromPrefs(ATestersPrefs(), SaveMigration.KnownFlags());
            CollectionAssert.AreEquivalent(new[] { Weapons.KatanaName, Weapons.StormKatanaName }, data.OwnedWeapons);
            CollectionAssert.AreEquivalent(new[] { Elements.Lightning }, data.OwnedElements);
        }

        [Test]
        public void AFlagThatIsNotOneIsNotCopied()
        {
            var prefs = new FakePrefs();
            prefs.Ints[SaveMigration.PrefsPrefix + Progress.WeaponKey(Weapons.ScytheName)] = 0;
            prefs.Ints[SaveMigration.PrefsPrefix + Progress.ElementKey(Elements.Fire)] = 2;
            var data = SaveMigration.FromPrefs(prefs, SaveMigration.KnownFlags());
            Assert.AreEqual(0, data.Flags.Count);
            Assert.AreEqual(0, data.OwnedWeapons.Count);
            Assert.AreEqual(0, data.OwnedElements.Count);
        }

        [Test]
        public void KnownFlagsCoverEveryWeaponEveryElementAndTheSchedule()
        {
            var known = SaveMigration.KnownFlags().ToList();
            foreach (var weapon in Weapons.Offered) CollectionAssert.Contains(known, Progress.WeaponKey(weapon));
            CollectionAssert.Contains(known, Progress.WeaponKey(Weapons.StormKatanaName));
            CollectionAssert.Contains(known, Progress.WeaponKey(Weapons.SunSpearName));
            foreach (var element in Elements.All) CollectionAssert.Contains(known, Progress.ElementKey(element.Name));
            CollectionAssert.Contains(known, Progress.MetSponsor);
            CollectionAssert.Contains(known, SlotSchedule.ChampionFlag);
            CollectionAssert.Contains(known, SlotSchedule.FireGodFlag);
            CollectionAssert.AllItemsAreUnique(known);
        }

        [Test]
        public void RunningItAgainGivesTheSameSave()
        {
            var prefs = ATestersPrefs();
            var first = SaveMigration.FromPrefs(prefs, SaveMigration.KnownFlags());
            var second = SaveMigration.FromPrefs(prefs, SaveMigration.KnownFlags());
            Assert.AreEqual(JsonUtility.ToJson(first), JsonUtility.ToJson(second));

            // Asking again of a save already made adds nothing and reads nothing.
            int flags = first.Flags.Count, asked = first.PrefsAsked.Count, reads = prefs.Reads;
            foreach (var key in SaveMigration.KnownFlags()) SaveMigration.AskPrefs(first, prefs, key);
            Assert.AreEqual(flags, first.Flags.Count);
            Assert.AreEqual(asked, first.PrefsAsked.Count);
            Assert.AreEqual(reads, prefs.Reads);
        }

        [Test]
        public void PrefsAreLeftAsTheyWere()
        {
            var prefs = ATestersPrefs();
            prefs.Ints["clover.controls.bindings.version"] = 3;
            var before = prefs.Snapshot();
            var data = SaveMigration.FromPrefs(prefs, SaveMigration.KnownFlags());
            SaveMigration.AskPrefs(data, prefs, "door.north");
            data.ClearFlag(Progress.MetSponsor);
            CollectionAssert.AreEquivalent(before, prefs.Ints);
        }

        [Test]
        public void AWorldKeyIsAskedOfPrefsOnceAndAForgottenOneStaysForgotten()
        {
            var prefs = new FakePrefs().Flag("suncity.ladder.3");
            var data = SaveMigration.FromPrefs(prefs, SaveMigration.KnownFlags());
            Assert.IsFalse(data.HasFlag("suncity.ladder.3"), "not a known flag, so not copied up front");

            Assert.IsTrue(SaveMigration.AskPrefs(data, prefs, "suncity.ladder.3"));
            Assert.IsTrue(data.HasFlag("suncity.ladder.3"));
            CollectionAssert.Contains(data.PrefsAsked, "suncity.ladder.3");

            data.ClearFlag("suncity.ladder.3");
            int reads = prefs.Reads;
            Assert.IsFalse(SaveMigration.AskPrefs(data, prefs, "suncity.ladder.3"), "forgotten here, not brought back from PlayerPrefs");
            Assert.AreEqual(reads, prefs.Reads);
        }

        [Test]
        public void AnOlderVersionIsBroughtUpAndRepaired()
        {
            var data = new SaveData { Version = 0, Flags = null };
            SaveMigration.Upgrade(data);
            Assert.AreEqual(SaveData.CurrentVersion, data.Version);
            Assert.IsNotNull(data.Flags);
        }

        [Test]
        public void ARetiredMarkIsTakenOutAndItsPricePaidBackOnce()
        {
            var data = new SaveData();
            data.Learned.Add("fire.old");
            data.Learned.Add("fire.heat");
            data.Wallet.Shards = 1;
            var retired = new[] { new RetiredMark("fire.old", 4) };

            SaveMigration.Upgrade(data, retired);
            CollectionAssert.AreEqual(new[] { "fire.heat" }, data.Learned);
            Assert.AreEqual(5, data.Wallet.Shards);

            SaveMigration.Upgrade(data, retired);
            Assert.AreEqual(5, data.Wallet.Shards, "a second load pays nothing more");
        }

        [Test]
        public void NoMarkIsRetiredYet()
        {
            Assert.AreEqual(0, SaveMigration.RetiredMarks.Count);
        }
    }
}

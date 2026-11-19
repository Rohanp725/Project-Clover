using Clover.Core;
using Clover.Core.Save;
using Clover.Player.Build;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>The save's schema (IMPLEMENTATION.md 5.1): what JsonUtility writes comes back whole, at this version, with no list left null.</summary>
    public class SaveDataTests
    {
        private static SaveData RoundTrip(SaveData data) => JsonUtility.FromJson<SaveData>(JsonUtility.ToJson(data));

        [Test]
        public void EveryFieldComesBackFromJson()
        {
            var data = new SaveData();
            data.SetFlag("met.sponsor");
            data.SetFlag(Progress.WeaponKey("Katana"));
            data.SetFlag(Progress.ElementKey("Fire"));
            data.Learned.Add("fire.heat");
            data.Found.Add("evolution.winged_sun");
            data.SeenReactions.Add("Flashover");
            data.KindsDefeated.Add("sun_acolyte");
            data.Wallet.Shards = 12;
            data.Wallet.Memories = 3;
            data.Wallet.ShardsBanked = 40;
            data.Lost.Amount = 7;
            data.Lost.Scene = "SunCity";
            data.Lost.KillerKey = "SunCity#4";
            data.Lost.Position = new Vector3(1f, 2f, 3f);
            data.Current.Attune("Fire", "Lightning");
            data.Current.Slots[1] = "fire.base";
            data.Current.Shapes.Add(new IdChoice("fire.wave", 1));
            data.Current.Weapon = "Katana";
            data.SavedHands[2].Surge = "fire.surge";
            data.Bond.Add(new IdPoints("weapon:katana", 30));
            data.Tallies.Add(new IdCount("steel.fold", 5));
            data.LastRest.Scene = "SunCity";
            data.LastRest.RestId = "gate";
            data.GodsFelled = 2;
            data.PrefsAsked.Add("door.north");

            var back = RoundTrip(data);

            Assert.AreEqual(SaveData.CurrentVersion, back.Version);
            CollectionAssert.AreEqual(data.Flags, back.Flags);
            CollectionAssert.AreEqual(new[] { "Katana" }, back.OwnedWeapons);
            CollectionAssert.AreEqual(new[] { "Fire" }, back.OwnedElements);
            CollectionAssert.AreEqual(data.Learned, back.Learned);
            CollectionAssert.AreEqual(data.Found, back.Found);
            CollectionAssert.AreEqual(data.SeenReactions, back.SeenReactions);
            CollectionAssert.AreEqual(data.KindsDefeated, back.KindsDefeated);
            Assert.AreEqual(12, back.Wallet.Shards);
            Assert.AreEqual(3, back.Wallet.Memories);
            Assert.AreEqual(40, back.Wallet.ShardsBanked);
            Assert.IsTrue(back.Lost.Any);
            Assert.AreEqual("SunCity#4", back.Lost.KillerKey);
            Assert.AreEqual(new Vector3(1f, 2f, 3f), back.Lost.Position);
            Assert.AreEqual("Fire", back.Current.First);
            Assert.AreEqual("Lightning", back.Current.Second);
            Assert.AreEqual("fire.base", back.Current.SlotAt(1));
            Assert.AreEqual(1, back.Current.ShapeOf("fire.wave"));
            Assert.AreEqual("Katana", back.Current.Weapon);
            Assert.AreEqual(SaveData.SavedHandCount, back.SavedHands.Length);
            Assert.AreEqual("fire.surge", back.SavedHands[2].Surge);
            Assert.AreEqual("weapon:katana", back.Bond[0].Id);
            Assert.AreEqual(30, back.Bond[0].Points);
            Assert.AreEqual(5, back.Tallies[0].Count);
            Assert.IsTrue(back.LastRest.IsSet);
            Assert.AreEqual("gate", back.LastRest.RestId);
            Assert.AreEqual(2, back.GodsFelled);
            CollectionAssert.AreEqual(data.PrefsAsked, back.PrefsAsked);
        }

        [Test]
        public void ANewSaveIsVersionOneAndSaysSoInItsFile()
        {
            Assert.AreEqual(1, SaveData.CurrentVersion);
            var data = new SaveData();
            Assert.AreEqual(SaveData.CurrentVersion, data.Version);
            StringAssert.Contains("\"Version\":1", JsonUtility.ToJson(data));
            Assert.AreEqual(SaveData.CurrentVersion, RoundTrip(data).Version);
        }

        [Test]
        public void AnEmptySaveComesBackWithEmptyListsAndThreeEmptyHands()
        {
            var back = RoundTrip(new SaveData());
            Assert.IsNotNull(back.Flags);
            Assert.AreEqual(0, back.Flags.Count);
            Assert.AreEqual(0, back.OwnedElements.Count);
            Assert.AreEqual(0, back.OwnedWeapons.Count);
            Assert.AreEqual(0, back.Learned.Count);
            Assert.AreEqual(0, back.Found.Count);
            Assert.AreEqual(0, back.SeenReactions.Count);
            Assert.AreEqual(0, back.KindsDefeated.Count);
            Assert.AreEqual(0, back.Bond.Count);
            Assert.AreEqual(0, back.Tallies.Count);
            Assert.AreEqual(0, back.PrefsAsked.Count);
            Assert.IsFalse(back.Lost.Any);
            Assert.IsFalse(back.LastRest.IsSet);
            Assert.AreEqual(SaveData.SavedHandCount, back.SavedHands.Length);
            foreach (var hand in back.SavedHands) Assert.IsNotNull(hand);
        }

        [Test]
        public void RepairPutsBackWhatADamagedFileLacks()
        {
            var data = new SaveData { Flags = null, Learned = null, Wallet = null, Lost = null, Current = null, SavedHands = new Loadout[1], PrefsAsked = null };
            data.Repair();
            Assert.IsNotNull(data.Flags);
            Assert.IsNotNull(data.Learned);
            Assert.IsNotNull(data.Wallet);
            Assert.IsNotNull(data.Lost);
            Assert.IsNotNull(data.Current);
            Assert.IsNotNull(data.PrefsAsked);
            Assert.AreEqual(SaveData.SavedHandCount, data.SavedHands.Length);
            foreach (var hand in data.SavedHands) Assert.IsNotNull(hand);
        }

        [Test]
        public void AWeaponOrElementFlagOwnsItAndForgettingItGivesItUp()
        {
            var data = new SaveData();
            Assert.IsTrue(data.SetFlag(Progress.WeaponKey("Storm Katana")));
            Assert.IsFalse(data.SetFlag(Progress.WeaponKey("Storm Katana")), "set twice is set once");
            Assert.IsTrue(data.SetFlag(Progress.ElementKey("Lightning")));
            Assert.IsTrue(data.SetFlag("door.north"));
            CollectionAssert.AreEqual(new[] { "Storm Katana" }, data.OwnedWeapons);
            CollectionAssert.AreEqual(new[] { "Lightning" }, data.OwnedElements);
            Assert.AreEqual(3, data.Flags.Count);

            Assert.IsTrue(data.ClearFlag(Progress.WeaponKey("Storm Katana")));
            Assert.IsFalse(data.ClearFlag(Progress.WeaponKey("Storm Katana")));
            Assert.AreEqual(0, data.OwnedWeapons.Count);
            Assert.IsFalse(data.HasFlag(Progress.WeaponKey("Storm Katana")));
            Assert.IsTrue(data.HasFlag("door.north"));
        }
    }
}

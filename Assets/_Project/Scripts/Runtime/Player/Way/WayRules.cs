using System;
using System.Collections.Generic;
using Clover.Player.Build;

namespace Clover.Player.Way
{
    /// <summary>
    /// The rules of the Way and the Body (IMPLEMENTATION.md 2.5 and 3.6; steel/weapons.md 0; way_plan.md 0), as plain
    /// rules over plain state so they can be tested without a player:
    ///
    /// - A Way mark is in force while its weapon kind is carried (a god's weapon counts as its kind). A trunk mark works
    ///   in every aspect; a bough's marks only while that bough is the active aspect, and they sleep, kept, while another
    ///   is. Of a bough's two keystones only the one set at a rest fire is in force; the two paths' other marks work
    ///   whichever is set (question G1, decided). An element infusion mark also needs its element carried.
    /// - A Body mark is always in force, but of the two bearings only the one set (question G5, decided).
    /// - Carrying the god's weapon itself (the Storm Katana from the rack) makes its bough the active aspect.
    /// - The first aspect bought for a weapon becomes the active one, and the first keystone bought on a bough the set
    ///   one, so a mark bought is never a mark asleep until the next fire (docs/design/steel/phase5_questions.md).
    /// - Memories spent on one Way are counted for the weapon's first Heresy (30, IMPLEMENTATION.md 3.10).
    ///
    /// Buying is <see cref="MarkRules.CanBuy(Upgrade, ICollection{string}, Growth.Wallet, bool, bool, Func{string, bool}, int)"/>
    /// with <see cref="Open"/> as its open: currency and gate are asked there, for every chart alike.
    /// </summary>
    public static class WayRules
    {
        /// <summary>The bough key under which the Body's set bearing is kept in <see cref="Loadout.Keystones"/>.</summary>
        public const string BodyBough = "body";
        /// <summary>The aspect id of Narukami's bough on the Katana's Way: the Storm Katana itself.</summary>
        public const string StormKatanaBough = "katana.storm";
        /// <summary>The aspect id of Khepharu's bough on the Great Spear's Way: the Sun-Spear itself (its Way is task 5.7 on).</summary>
        public const string SunSpearBough = "spear.sunspear";
        /// <summary>Memories spent on one Way that open the weapon's first Heresy (depth_weapons.md 6).</summary>
        public const int WeaponHeresyAt = 30;

        /// <summary>
        /// The weapon kind a carried weapon belongs to, the name its Way is kept under: a god's weapon counts as its kind
        /// (the Storm Katana is a Katana, the Sun-Spear a Great Spear) and the Armament is the Fists' (steel/weapons.md 9).
        /// "" for none.
        /// </summary>
        public static string KindOf(string weaponName)
        {
            if (string.IsNullOrEmpty(weaponName)) return "";
            if (weaponName == Weapons.StormKatanaName) return Weapons.KatanaName;
            if (weaponName == Weapons.SunSpearName) return Weapons.GreatSpearName;
            if (weaponName == Weapons.ArmamentName) return Weapons.FistsName;
            return weaponName;
        }

        /// <summary>The bough a god's weapon is, when the name is one; null for any other weapon.</summary>
        public static string GodBoughOf(string weaponName)
        {
            if (weaponName == Weapons.StormKatanaName) return StormKatanaBough;
            if (weaponName == Weapons.SunSpearName) return SunSpearBough;
            return null;
        }

        /// <summary>
        /// The active aspect of a weapon kind under the loadout: the god's bough while the god's weapon itself is carried,
        /// else the aspect chosen at a rest fire, else "" (the plain weapon).
        /// </summary>
        public static string ActiveAspect(Loadout loadout, string kind)
        {
            if (loadout == null || string.IsNullOrEmpty(kind)) return "";
            if (KindOf(loadout.Weapon) == kind)
            {
                string god = GodBoughOf(loadout.Weapon);
                if (god != null) return god;
            }
            return loadout.AspectOf(kind);
        }

        /// <summary>True for a god's bough's opening: an aspect given by Take, never bought (a playtest unlock, as a god's gift is).</summary>
        public static bool IsGodBough(Upgrade mark) => mark != null && mark.Kind == UpgradeKind.Aspect && mark.Currency == Currency.None;

        /// <summary>True for a keystone of a bough or a bearing of the Body: the marks of which only the set one is in force.</summary>
        public static bool IsKeystone(Upgrade mark) =>
            mark != null && mark.IsWay && mark.Kind == UpgradeKind.Keystone && (mark.Tree == MarkTree.Body || !string.IsNullOrEmpty(mark.Aspect));

        /// <summary>The key a keystone's choice is kept under: its bough's aspect, or the Body's.</summary>
        public static string BoughOf(Upgrade mark) => mark == null ? "" : mark.Tree == MarkTree.Body ? BodyBough : mark.Aspect ?? "";

        /// <summary>True when a Way or Body mark is in force under the loadout (learning is asked by the caller).</summary>
        public static bool InForce(Upgrade mark, Loadout loadout)
        {
            if (mark == null || loadout == null || !mark.IsWay) return false;
            if (mark.Tree == MarkTree.Body) return !IsKeystone(mark) || loadout.KeystoneOf(BodyBough) == mark.Id;
            string kind = KindOf(loadout.Weapon);
            if (string.IsNullOrEmpty(kind) || kind != mark.Weapon) return false;
            // An element infusion mark (Burning Crescent) works while its element is carried.
            if (!string.IsNullOrEmpty(mark.Element) && !loadout.Carries(mark.Element)) return false;
            // A weapon taught twice (the Fists) puts one technique on the chord; with none chosen, both wait on it.
            if (mark.Kind == UpgradeKind.Technique && !string.IsNullOrEmpty(loadout.Technique) && loadout.Technique != mark.Id) return false;
            if (string.IsNullOrEmpty(mark.Aspect)) return true;
            if (ActiveAspect(loadout, kind) != mark.Aspect) return false;
            return !IsKeystone(mark) || loadout.KeystoneOf(mark.Aspect) == mark.Id;
        }

        /// <summary>True when the weapon kind is owned: any owned weapon of that kind, a god's weapon among them.</summary>
        public static bool KindOwned(string kind, ICollection<string> ownedWeapons)
        {
            if (string.IsNullOrEmpty(kind) || ownedWeapons == null) return false;
            foreach (string owned in ownedWeapons)
                if (KindOf(owned) == kind) return true;
            return false;
        }

        /// <summary>
        /// True when a Way or Body mark is on a chart open now: its weapon kind owned (the Body always), or anything in
        /// the playtest. What is asked before <see cref="MarkRules.CanBuy(Upgrade, ICollection{string}, Growth.Wallet, bool, bool, Func{string, bool}, int)"/>.
        /// </summary>
        public static bool Open(Upgrade mark, ICollection<string> ownedWeapons, bool playtest)
        {
            if (mark == null || !mark.IsWay) return false;
            if (playtest || mark.Tree == MarkTree.Body) return true;
            return KindOwned(mark.Weapon, ownedWeapons);
        }

        /// <summary>
        /// After a mark is bought: an aspect becomes active when its weapon has none, a keystone becomes the set one when
        /// its bough has none. True when the loadout changed.
        /// </summary>
        public static bool SettleBought(Upgrade bought, Loadout loadout)
        {
            if (bought == null || loadout == null || !bought.IsWay) return false;
            if (bought.Kind == UpgradeKind.Aspect && !string.IsNullOrEmpty(bought.Weapon) && loadout.AspectOf(bought.Weapon) == "")
            {
                loadout.SetAspect(bought.Weapon, bought.Id);
                return true;
            }
            if (IsKeystone(bought) && loadout.KeystoneOf(BoughOf(bought)) == "")
            {
                loadout.SetKeystone(BoughOf(bought), bought.Id);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Makes an aspect the active one of its weapon (at a rest fire: the caller's to say). Refused unless it is an
        /// aspect mark that is learned, or a god's bough in the playtest, where the god's weapons are on the rack.
        /// </summary>
        public static bool ChooseAspect(Loadout loadout, Upgrade aspect, ICollection<string> learned, bool playtest)
        {
            if (loadout == null || aspect == null || aspect.Kind != UpgradeKind.Aspect || string.IsNullOrEmpty(aspect.Weapon)) return false;
            bool had = learned != null && learned.Contains(aspect.Id);
            if (!had && !(playtest && IsGodBough(aspect))) return false;
            loadout.SetAspect(aspect.Weapon, aspect.Id);
            return true;
        }

        /// <summary>Sets a learned keystone (or bearing) as its bough's one in force. Refused for anything else.</summary>
        public static bool ChooseKeystone(Loadout loadout, Upgrade keystone, ICollection<string> learned)
        {
            if (loadout == null || !IsKeystone(keystone) || learned == null || !learned.Contains(keystone.Id)) return false;
            loadout.SetKeystone(BoughOf(keystone), keystone.Id);
            return true;
        }

        /// <summary>The Memories spent on one weapon kind's Way: every learned mark of it, sleeping boughs included.</summary>
        public static int Spent(string kind, ICollection<string> learned, IReadOnlyList<Upgrade> catalogue)
        {
            if (string.IsNullOrEmpty(kind) || learned == null || catalogue == null) return 0;
            int spent = 0;
            for (int i = 0; i < catalogue.Count; i++)
            {
                var mark = catalogue[i];
                if (mark.Tree == MarkTree.Way && mark.Weapon == kind && mark.Currency == Currency.Memories && learned.Contains(mark.Id)) spent += mark.Cost;
            }
            return spent;
        }

        /// <summary>True once enough has been spent on the weapon's Way for its first Heresy to open.</summary>
        public static bool WeaponHeresyOpen(string kind, ICollection<string> learned, IReadOnlyList<Upgrade> catalogue) =>
            Spent(kind, learned, catalogue) >= WeaponHeresyAt;

        /// <summary>How many of the weapon kind's temper ranks are learned (0 to 5).</summary>
        public static int TemperRank(string kind, ICollection<string> learned, IReadOnlyList<Upgrade> catalogue)
        {
            if (string.IsNullOrEmpty(kind) || learned == null || catalogue == null) return 0;
            int rank = 0;
            for (int i = 0; i < catalogue.Count; i++)
            {
                var mark = catalogue[i];
                if (mark.Kind == UpgradeKind.Temper && mark.Weapon == kind && learned.Contains(mark.Id)) rank++;
            }
            return rank;
        }
    }
}

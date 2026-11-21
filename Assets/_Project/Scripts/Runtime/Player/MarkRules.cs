using System;
using System.Collections.Generic;
using Clover.Gameplay;
using Clover.Player.Growth;

namespace Clover.Player
{
    /// <summary>
    /// Whether a mark on the chart is open and can be bought, as plain rules over plain state so they can be tested
    /// without a player: the game's own rules, and the playtest's (every tree and branch open, the gods' gifts sold).
    /// A mark is paid in its own currency (Shards for the element trees and duo branches, Memories for the Way and the
    /// Body) and waits behind its gate (an elite beaten, a god felled, a count of gods), both asked here for every chart.
    /// </summary>
    public static class MarkRules
    {
        /// <summary>A gate that opens on a count of gods felled: <c>gods.3</c> opens at the third (the Way's stages, depth_weapons.md 6).</summary>
        public const string GodsGate = "gods.";

        /// <summary>True when the mark is on a tree or branch open now: the element held, a mix of it with an element held or consumed, or, in the playtest, any.</summary>
        public static bool Open(Upgrade upgrade, string held, ICollection<string> everHeld, ICollection<string> consumed, bool playtest)
        {
            // A hidden mark (a reaction) is on no tree, so no tree opens it.
            if (upgrade == null || upgrade.IsHidden) return false;
            if (playtest) return true;
            if (upgrade.IsMix) return UpgradeTree.MixOpen(upgrade, held, everHeld, consumed);
            return upgrade.Element == held;
        }

        /// <summary>
        /// True when the mark is on a tree or branch open under the loadout (IMPLEMENTATION.md 2.3, task 4.5): an element's
        /// mark while its element is carried, a duo branch's mark of any pair, neighbours or not, while both its elements are
        /// carried; in the playtest, any. What the heart's duo branches are opened by; the mixes still on the ring keep the
        /// run's rule above until they are reshaped.
        /// </summary>
        public static bool Open(Upgrade upgrade, Build.Loadout loadout, bool playtest)
        {
            if (upgrade == null || upgrade.IsHidden) return false;
            if (playtest) return true;
            return loadout != null && Carried(upgrade, loadout);
        }

        /// <summary>
        /// True when the mark can be bought: open, not learned, reachable from what is learned, and paid for. A god's
        /// gift is never sold, except in the playtest, where it is a playtest unlock that needs no road to it. The
        /// playtest's "Not built yet" slots are never sold.
        /// </summary>
        public static bool CanBuy(Upgrade upgrade, ICollection<string> unlocked, int points, bool open, bool playtest)
        {
            // The element chart's purse holds Shards only: a mark of the Way or the Body is bought from the wallet's Memories.
            if (upgrade != null && upgrade.Currency == Currency.Memories) return false;
            return Buyable(upgrade, unlocked, points, open, playtest, null, 0);
        }

        /// <summary>
        /// True when the mark can be bought from the wallet: open, not learned, reachable, paid in its own currency, and
        /// its gate open (<see cref="GateOpen(Upgrade, Func{string, bool}, int, bool)"/>, with <paramref name="has"/> asking
        /// the progress flags). A god's gift and a god's bough are never sold, except as the playtest's unlocks.
        /// </summary>
        public static bool CanBuy(Upgrade upgrade, ICollection<string> unlocked, Wallet wallet, bool open, bool playtest, Func<string, bool> has, int godsFelled)
        {
            if (upgrade == null || wallet == null) return false;
            return Buyable(upgrade, unlocked, wallet.Of(upgrade.Currency), open, playtest, has, godsFelled);
        }

        /// <summary>
        /// The chart's one buy, asked in the mark's own currency: a mark priced in Memories (the Way, the Body) from the
        /// wallet, any other from the run's <paramref name="shards"/> (RunState.Points until task L.3 moves them into the
        /// wallet). <paramref name="wallet"/> may be null when the mark is not priced in Memories.
        /// </summary>
        public static bool CanBuyInItsCurrency(Upgrade upgrade, ICollection<string> unlocked, int shards, Wallet wallet, bool open, bool playtest,
            Func<string, bool> has, int godsFelled) =>
            upgrade != null && upgrade.Currency == Currency.Memories
                ? CanBuy(upgrade, unlocked, wallet, open, playtest, has, godsFelled)
                : CanBuy(upgrade, unlocked, shards, open, playtest);

        // The one rule both purses share; funds are what is held of the mark's own currency.
        private static bool Buyable(Upgrade upgrade, ICollection<string> unlocked, int funds, bool open, bool playtest, Func<string, bool> has, int godsFelled)
        {
            if (upgrade == null || unlocked == null || upgrade.IsHidden || NotBuiltYet.IsSlot(upgrade) || !open) return false;
            // A god's gift, and a god's bough on the Way (given by Take), are unlocks the playtest sells for nothing.
            bool unlock = upgrade.Signature || Way.WayRules.IsGodBough(upgrade);
            // A given mark (a base spell's Learn, which comes with its element) is never sold, in the playtest either.
            if (upgrade.Currency == Currency.None && !unlock) return false;
            if (unlock && !playtest) return false;
            if (unlocked.Contains(upgrade.Id) || funds < upgrade.Cost) return false;
            if (!GateOpen(upgrade, has, godsFelled, playtest)) return false;
            return unlock || UpgradeTree.Reachable(upgrade, unlocked);
        }

        /// <summary>True when the mark's gate is open: it has none, its flag is set, enough gods have fallen, or the playtest is on.</summary>
        public static bool GateOpen(Upgrade upgrade, Func<string, bool> has, int godsFelled, bool playtest) =>
            upgrade != null && GateOpen(upgrade.Gate, has, godsFelled, playtest);

        /// <summary>True when a gate is open: none, a progress flag that <paramref name="has"/> says is set, or <c>gods.N</c> with N felled. The playtest opens every gate.</summary>
        public static bool GateOpen(string gate, Func<string, bool> has, int godsFelled, bool playtest)
        {
            if (string.IsNullOrEmpty(gate) || playtest) return true;
            if (gate.StartsWith(GodsGate, StringComparison.Ordinal) && int.TryParse(gate.Substring(GodsGate.Length), out int needed))
                return godsFelled >= needed;
            return has != null && has(gate);
        }

        /// <summary>
        /// True when a learned mark is in force under a loadout (IMPLEMENTATION.md 3.6): the one place that decides it,
        /// read by <see cref="PlayerUpgrades"/>. An element's mark (its gift too) while its element is attuned; a duo
        /// branch's mark while both its elements are; a duo spell only while it also sits in a slot; a mark that names a
        /// weapon only while that weapon kind is carried (a god's weapon counts as its kind: the Storm Katana is a
        /// Katana). A spell branch's Empower, Shape and Master only while its spell sits in a slot (the shape's own effects
        /// only for the shape worn: <see cref="SpellBranches.Worn"/>); a Devotion only all in. A Way or Body mark by the
        /// Way's rules (<see cref="Way.WayRules.InForce"/>): its weapon kind carried, its bough the active aspect, its
        /// keystone the one set. The rules for Heresies, Bond and Evolutions join here as their fields are added (phase 6).
        /// </summary>
        public static bool InForce(Upgrade mark, Build.Loadout loadout)
        {
            // A hidden mark (a spare spell kept for the Evolutions, a plain reaction) is never in force as a mark: a
            // reaction runs through the reaction table, not through what is learned.
            if (mark == null || loadout == null || mark.IsHidden) return false;
            if (mark.IsWay) return Way.WayRules.InForce(mark, loadout);
            if (!WeaponCarried(mark, loadout) || !Carried(mark, loadout)) return false;
            if (mark.IsMix && mark.IsSpell && !loadout.Slotted(mark.Id)) return false;
            // A duo surge only while it is the surge worn (the ledger's duo-surge-slot).
            if (mark.Surge != null && mark.IsMix && loadout.Surge != mark.Id) return false;
            if (BranchNeedsSlot(mark) && !loadout.Slotted(mark.Branch)) return false;
            // The all-in passive (STEEL_AND_ELEMENT.md 5.6): the same element in both attunements.
            if (mark.Kind == UpgradeKind.Devotion && !(loadout.AllIn && loadout.First == mark.Element)) return false;
            return true;
        }

        /// <summary>
        /// The same, for a mark that must also be learned: what <see cref="PlayerUpgrades"/> asks of every mark. A Devotion is
        /// in force only bought and all in on its element (task 4.4).
        /// </summary>
        public static bool InForce(Upgrade mark, Build.Loadout loadout, ICollection<string> learned) =>
            mark != null && learned != null && learned.Contains(mark.Id) && InForce(mark, loadout);

        /// <summary>True for a spell branch's Empower, Shape or Master: in force only while the branch's spell is slotted.</summary>
        public static bool BranchNeedsSlot(Upgrade mark) =>
            mark != null && !string.IsNullOrEmpty(mark.Branch) && (mark.Part == BranchPart.Empower || mark.Part == BranchPart.Shape || mark.Part == BranchPart.Master);

        /// <summary>True when a spell mark may sit in a slot under the loadout: its elements and its weapon (if it names one) carried. Learning is asked elsewhere.</summary>
        public static bool Slottable(Upgrade mark, Build.Loadout loadout) =>
            mark != null && !mark.IsHidden && mark.IsSpell && WeaponCarried(mark, loadout) && Carried(mark, loadout);

        /// <summary>True when the mark names no weapon, or the loadout's weapon is that kind (a god's weapon counts as its kind).</summary>
        public static bool WeaponCarried(Upgrade mark, Build.Loadout loadout)
        {
            if (mark == null || loadout == null) return false;
            return string.IsNullOrEmpty(mark.Weapon) || string.IsNullOrEmpty(loadout.Weapon) || loadout.Weapon.Contains(mark.Weapon);
        }

        /// <summary>True when the trigger is a blow of the weapon (or its skill), the triggers the first-element rule asks about.</summary>
        public static bool IsBlow(UpgradeTrigger trigger) =>
            trigger == UpgradeTrigger.AnyHit || trigger == UpgradeTrigger.LightHit || trigger == UpgradeTrigger.HeavyHit || trigger == UpgradeTrigger.FinisherHit
            || trigger == UpgradeTrigger.SkillHit || trigger == UpgradeTrigger.AfflictedHit || trigger == UpgradeTrigger.CritHit || trigger == UpgradeTrigger.ChainEnd;

        /// <summary>
        /// True when an effect of <paramref name="owner"/> on <paramref name="trigger"/> may lay <paramref name="laid"/>,
        /// with <paramref name="weapon"/> the affliction the weapon lays (the first element's). Weapon blows lay only the
        /// first element's affliction (the owner's rule of 2026-11-18), so the second element comes from spells, the
        /// charged blade and infused strikes, and reactions come from weaving weapon and spell. A mark on a mixed branch
        /// is the exception (the owner's of 2026-11-19): it says its blows lay both elements' afflictions, and they do.
        /// </summary>
        public static bool BlowMayLay(Upgrade owner, UpgradeTrigger trigger, AfflictionKind laid, AfflictionKind weapon)
        {
            if (laid == AfflictionKind.None || !IsBlow(trigger)) return true;
            if (owner != null && owner.Kind == UpgradeKind.Mix) return true;
            return laid == weapon;
        }

        /// <summary>True when the mark's element is attuned; for a duo branch's mark, both of its elements.</summary>
        public static bool Carried(Upgrade mark, Build.Loadout loadout)
        {
            if (mark == null || loadout == null) return false;
            if (!loadout.Carries(mark.Element)) return false;
            return !mark.IsMix || loadout.Carries(mark.Element2);
        }
    }
}

using System.Collections.Generic;
using Clover.Player.Build;
using Clover.Player.Way.Aspects;

namespace Clover.Player.Way
{
    /// <summary>
    /// Every weapon aspect there is and the one place a weapon is built from its kind, its active aspect and the marks in
    /// force (IMPLEMENTATION.md 2.5, task 5.2): a god's bough builds the god's weapon, any other bough patches the plain
    /// weapon, and the trunk patches either first. Run on choosing the aspect at a rest fire, on entering a scene and on
    /// buying a mark of the Way, after which <c>PlayerCombat.Equip</c> takes the result (the wiring is the editor half of
    /// task 5.2). A new bough is one line here and its own class under <c>Way/Aspects/</c>.
    ///
    /// The tables hold stateless instances only, set once, so nothing here needs resetting between plays.
    /// </summary>
    public static class Armoury
    {
        private static readonly WeaponAspect[] aspects =
        {
            new SheathAspect(),
            new StreamAspect(),
            new AnswerAspect(),
            new StormKatanaBough(),
        };

        private static readonly WayTrunk[] trunks =
        {
            new KatanaTrunk(),
        };

        /// <summary>Every aspect built, Way by Way.</summary>
        public static IReadOnlyList<WeaponAspect> All => aspects;

        /// <summary>The aspect with this id, or null.</summary>
        public static WeaponAspect ById(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            for (int i = 0; i < aspects.Length; i++)
                if (aspects[i].Id == id) return aspects[i];
            return null;
        }

        /// <summary>The trunk of a weapon kind's Way, or null for a Way not written yet.</summary>
        public static WayTrunk TrunkOf(string kind)
        {
            if (string.IsNullOrEmpty(kind)) return null;
            for (int i = 0; i < trunks.Length; i++)
                if (trunks[i].Weapon == kind) return trunks[i];
            return null;
        }

        /// <summary>Fills <paramref name="into"/> with the aspects of one weapon kind, in their order on its Way. Returns how many.</summary>
        public static int AspectsOf(string kind, List<WeaponAspect> into)
        {
            if (into == null) return 0;
            into.Clear();
            if (string.IsNullOrEmpty(kind)) return 0;
            for (int i = 0; i < aspects.Length; i++)
                if (aspects[i].Weapon == kind) into.Add(aspects[i]);
            return into.Count;
        }

        /// <summary>The weapon the loadout carries, in its active aspect (<see cref="WayRules.ActiveAspect"/>), with the marks in force.</summary>
        public static Weapon Resolve(Loadout loadout, IMarkQuery marks)
        {
            string weapon = loadout != null ? loadout.Weapon : "";
            return Resolve(weapon, WayRules.ActiveAspect(loadout, WayRules.KindOf(weapon)), marks);
        }

        /// <summary>
        /// Builds a weapon by name in an aspect. Carrying a god's weapon itself makes its bough the aspect, as
        /// <see cref="WayRules.ActiveAspect"/> says. Any other aspect is taken only on its own weapon and only while its
        /// mark is in force (learned and active, as <paramref name="marks"/> answers); otherwise the weapon is plain, with
        /// its trunk.
        /// </summary>
        public static Weapon Resolve(string weapon, string aspectId, IMarkQuery marks)
        {
            marks ??= MarkSet.None;
            string kind = WayRules.KindOf(weapon);
            string god = WayRules.GodBoughOf(weapon);
            var aspect = ById(god ?? aspectId);
            if (aspect != null && (aspect.Weapon != kind || (god == null && !marks.Has(aspect.Id)))) aspect = null;
            Weapon built = aspect != null && aspect.GodBough ? aspect.Build() : null;
            // The god's weapon whose bough is not written yet (the Sun-Spear) is still itself.
            built ??= Weapons.ByName(god != null ? weapon : kind);
            TrunkOf(kind)?.Patch(built, marks);
            aspect?.Patch(built, marks);
            return built;
        }
    }
}

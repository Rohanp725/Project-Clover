using System;
using System.Collections.Generic;

namespace Clover.Player
{
    /// <summary>
    /// Writes one spell branch (IMPLEMENTATION.md 2.2): its four marks, Learn, Empower, Shape and Master, with the branch,
    /// the parts, what each grows from, the kinds (and so the prices: 4, 1, 3, 3) and the places on the chart set in one
    /// call, so a tree's writer cannot get the wiring wrong. The Learn keeps the spell's own id (<c>fire.rush</c>); the
    /// other three are that id with <c>.empower</c>, <c>.shape</c> and <c>.master</c>.
    ///
    /// An element's base spell is a branch too, whose Learn is the base id (<c>fire.base</c>): given with the element,
    /// never bought, and cast by the element's kit, so it carries no form or numbers (<see cref="Upgrade.IsBaseSpell"/>).
    /// In namespace Clover.Player, not Trees, because it is no tree file.
    /// </summary>
    public static class SpellBranch
    {
        public const string EmpowerSuffix = ".empower";
        public const string ShapeSuffix = ".shape";
        public const string MasterSuffix = ".master";

        public static string EmpowerId(string branch) => branch + EmpowerSuffix;
        public static string ShapeId(string branch) => branch + ShapeSuffix;
        public static string MasterId(string branch) => branch + MasterSuffix;

        /// <summary>
        /// Adds the four marks of a branch to <paramref name="to"/> and returns its Learn mark.
        /// <paramref name="stalk"/> is the pattern's stalk (<see cref="BranchPattern.LeftA"/> and the rest) whose four places
        /// the marks take, inmost first. <paramref name="from"/> is what the Learn grows from (a passive on its bough); an
        /// element's base spell grows from its root and is given. <paramref name="form"/> and <paramref name="learn"/> are the
        /// spell's form and numbers; both are left empty for a base spell, which its kit casts.
        /// </summary>
        public static Upgrade Add(List<Upgrade> to, string element, string id, string name, SpellRole role, string stalk, string from,
            string form, SpellSpec learn, string learnWords, string icon,
            Empower empower, string empowerWords,
            ShapeDef shapeA, ShapeDef shapeB, string shapeWords,
            UpgradeEffect[] master, string masterWords,
            string weapon = "", TallySpec tally = null, UpgradeEffect[] learnEffects = null, StatMod[] masterStats = null)
        {
            if (to == null) throw new ArgumentNullException(nameof(to));
            bool given = Magic.SpellSlots.IsBase(id);
            var places = BranchPattern.Stalk(stalk);
            var learnMark = new Upgrade
            {
                Id = id, Name = name, Element = element, Kind = UpgradeKind.Spell, Part = BranchPart.Learn, Branch = id, Role = role,
                Currency = given ? Currency.None : Currency.Shards, Requires = Parents(from), Icon = icon, Weapon = weapon ?? "",
                Form = given ? "" : form ?? "", Spell = given ? null : learn, Description = learnWords ?? "",
                Tally = tally, Effects = learnEffects ?? Array.Empty<UpgradeEffect>(),
            };
            Place(learnMark, places[0]);
            to.Add(learnMark);

            var empowerMark = new Upgrade
            {
                Id = EmpowerId(id), Name = name + ": Empowered", Element = element, Kind = UpgradeKind.Minor, Part = BranchPart.Empower, Branch = id,
                Requires = new[] { id }, Icon = UpgradeIcons.Plus, Weapon = weapon ?? "", Empower = empower ?? new Empower(), Description = empowerWords ?? "",
            };
            Place(empowerMark, places[1]);
            to.Add(empowerMark);

            var shapeMark = new Upgrade
            {
                Id = ShapeId(id), Name = name + ": " + (shapeA?.Name ?? "") + " or " + (shapeB?.Name ?? ""), Element = element, Kind = UpgradeKind.Major,
                Part = BranchPart.Shape, Branch = id, Requires = new[] { EmpowerId(id) }, Icon = UpgradeIcons.Spiral, Weapon = weapon ?? "",
                Shapes = new[] { shapeA ?? new ShapeDef(), shapeB ?? new ShapeDef() }, Description = shapeWords ?? "",
            };
            Place(shapeMark, places[2]);
            to.Add(shapeMark);

            var masterMark = new Upgrade
            {
                Id = MasterId(id), Name = name + ": Mastered", Element = element, Kind = UpgradeKind.Major, Part = BranchPart.Master, Branch = id,
                Requires = new[] { ShapeId(id) }, Icon = UpgradeIcons.Star, Weapon = weapon ?? "",
                Effects = master ?? Array.Empty<UpgradeEffect>(), Stats = masterStats ?? Array.Empty<StatMod>(), Description = masterWords ?? "",
            };
            Place(masterMark, places[3]);
            to.Add(masterMark);
            return learnMark;
        }

        /// <summary>Sets a mark's place on its tree from a pattern place.</summary>
        public static Upgrade Place(Upgrade mark, BranchPattern.Place place)
        {
            mark.X = place.X;
            mark.Y = place.Y;
            return mark;
        }

        private static string[] Parents(string from) => string.IsNullOrEmpty(from) ? Array.Empty<string>() : new[] { from };
    }
}

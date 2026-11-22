using System.Collections.Generic;
using Clover.Player.Build;
using Clover.Player.Magic;

namespace Clover.Player
{
    /// <summary>
    /// What a spell branch makes of its spell (IMPLEMENTATION.md 2.2, 3.6): the shape worn, the form cast, the numbers
    /// after the shape and the Empower, the wait and the price. Plain rules over the catalogue, what is learned and the
    /// loadout, so they are tested without a scene; <see cref="PlayerUpgrades"/> asks them as it casts and as it works
    /// out what is in force. One instance keeps one spec per spell to write the numbers into, so casting allocates
    /// nothing once each spell has been cast.
    ///
    /// Order: the spell's own numbers (the Learn's spec, or for a base spell the shape's), then the shape's whole spec or
    /// change, then the Empower's change. A shape's whole spec keeps the Learn's wait and price unless its change sets
    /// them, so a shape is never dearer or slower by accident.
    /// </summary>
    public sealed class SpellBranches
    {
        private readonly Dictionary<string, SpellSpec> specs = new Dictionary<string, SpellSpec>();

        // ---- The marks of a branch.

        /// <summary>The Learn mark of the branch a mark is on, or null for a passive.</summary>
        public static Upgrade LearnOf(Upgrade mark) =>
            mark == null || string.IsNullOrEmpty(mark.Branch) ? null : mark.Id == mark.Branch ? mark : Upgrade.ById(mark.Branch);

        /// <summary>True when the branch's Empower is learned.</summary>
        public static bool Empowered(string branch, ICollection<string> learned) =>
            !string.IsNullOrEmpty(branch) && learned != null && learned.Contains(SpellBranch.EmpowerId(branch));

        /// <summary>
        /// The shape a Shape mark's spell wears under the loadout: the first unless the second was chosen. Null when the
        /// mark is no spell branch's Shape mark: a duo branch's Shape mark shapes its pair's reaction, and its shapes run
        /// only when the pair reacts (<see cref="ReactionTable.WornShape"/>), never as standing effects.
        /// </summary>
        public static ShapeDef Worn(Upgrade shapeMark, Loadout loadout)
        {
            if (shapeMark == null || shapeMark.Part != BranchPart.Shape || shapeMark.Shapes == null || shapeMark.Shapes.Length == 0) return null;
            if (string.IsNullOrEmpty(shapeMark.Branch)) return null;
            int choice = loadout != null ? loadout.ShapeOf(shapeMark.Branch) : 0;
            if (choice < 0 || choice >= shapeMark.Shapes.Length) choice = 0;
            return shapeMark.Shapes[choice];
        }

        /// <summary>Writes a spell's choice of shape (0 or 1, by its branch's Learn id) into a loadout's list of choices: changed in place, or added.</summary>
        public static void Choose(List<IdChoice> shapes, string branch, int choice)
        {
            if (shapes == null || string.IsNullOrEmpty(branch)) return;
            choice = choice == 1 ? 1 : 0;
            for (int i = 0; i < shapes.Count; i++)
            {
                if (shapes[i] == null || shapes[i].Id != branch) continue;
                shapes[i] = new IdChoice(branch, choice);
                return;
            }
            shapes.Add(new IdChoice(branch, choice));
        }

        /// <summary>Copies a list of choices into another (cleared first), sharing nothing.</summary>
        public static void CopyChoices(List<IdChoice> from, List<IdChoice> into)
        {
            if (into == null || ReferenceEquals(from, into)) return;
            into.Clear();
            if (from == null) return;
            foreach (var choice in from)
                if (choice != null) into.Add(new IdChoice(choice.Id, choice.Choice));
        }

        /// <summary>The shape the branch's spell wears now: none until its Shape mark is learned.</summary>
        public static ShapeDef WornShape(string branch, ICollection<string> learned, Loadout loadout)
        {
            if (string.IsNullOrEmpty(branch) || learned == null) return null;
            string id = SpellBranch.ShapeId(branch);
            return learned.Contains(id) ? Worn(Upgrade.ById(id), loadout) : null;
        }

        /// <summary>
        /// True when an effect asking <see cref="UpgradeEffect.OwnBranch"/> answers this cast: the spell cast is its own
        /// branch's. <paramref name="castSource"/> is the mark whose spell it is (null: an element's base spell cast by its
        /// kit, which answers the base spells' branches).
        /// </summary>
        public static bool OwnBranch(Upgrade owner, Upgrade castSource)
        {
            if (owner == null || string.IsNullOrEmpty(owner.Branch)) return false;
            // NEEDS (base-spell-element): the element of a base spell cast by its kit. Until then, with two elements carried, a base spell's
            // Master answers either element's base spell.
            if (castSource == null) return SpellSlots.IsBase(owner.Branch);
            return castSource.Id == owner.Branch || castSource.Branch == owner.Branch;
        }

        // ---- What is cast.

        /// <summary>
        /// True when a base spell is cast through its Learn mark and not by its kit: its worn shape casts a form or numbers
        /// of its own (Cinder Fan, Rolling Thunder).
        /// </summary>
        public static bool CastsThroughMark(Upgrade learn, ICollection<string> learned, Loadout loadout)
        {
            if (learn == null || !learn.IsBaseSpell) return false;
            var shape = WornShape(learn.Id, learned, loadout);
            return shape != null && shape.CastsItsOwn;
        }

        /// <summary>The form the branch's spell casts now: the worn shape's, else its own. Empty for a base spell its kit casts.</summary>
        public static string FormOf(Upgrade learn, ICollection<string> learned, Loadout loadout)
        {
            if (learn == null) return "";
            var shape = WornShape(learn.Branch.Length > 0 ? learn.Branch : learn.Id, learned, loadout);
            return shape != null && !string.IsNullOrEmpty(shape.Form) ? shape.Form : learn.Form ?? "";
        }

        /// <summary>
        /// The numbers the spell is cast with now, written into this instance's own spec for it (do not keep it past the
        /// cast). Null when the spell has none: a base spell its kit casts, or a mark with no spec.
        /// </summary>
        // What a spec held before an Empower, for a shape that keeps its own numbers under it.
        private readonly SpellSpec kept = new SpellSpec();

        public SpellSpec SpecOf(Upgrade learn, ICollection<string> learned, Loadout loadout)
        {
            if (learn == null) return null;
            string branch = learn.Branch.Length > 0 ? learn.Branch : learn.Id;
            var shape = WornShape(branch, learned, loadout);
            SpellSpec own = learn.Spell;
            if (shape != null && shape.Spell != null) own = shape.Spell;
            if (own == null) return null;
            if (!specs.TryGetValue(learn.Id, out var spec))
            {
                spec = new SpellSpec();
                specs[learn.Id] = spec;
            }
            SpellChange.Copy(own, spec);
            if (shape != null && shape.Spell != null)
            {
                // A shape's whole spec keeps the Learn's wait and price (the timing table's).
                spec.Cooldown = learn.Spell != null ? learn.Spell.Cooldown : SpellTimings.For(learn.Id).Cooldown;
                spec.Cost = learn.Spell != null ? learn.Spell.Cost : SpellTimings.For(learn.Id).Mana;
            }
            shape?.Change?.ApplyTo(spec);
            if (Empowered(branch, learned))
            {
                var empower = EmpowerOf(branch);
                // An Empower for the Learn's own spell leaves a shape's whole spell as the shape gives it, its wait apart.
                if (empower != null && empower.LearnOnly && shape != null && shape.Spell != null)
                {
                    if (empower.Cooldown > 0f) spec.Cooldown = empower.Cooldown;
                    if (empower.Mana > 0f) spec.Cost = empower.Mana;
                }
                else if (empower != null && shape != null && shape.Empowered == EmpowerOverShape.Adds) empower.AddOver(spec, learn.Spell);
                else if (empower != null && shape != null && shape.Empowered == EmpowerOverShape.KeepsOwn && shape.Change != null)
                {
                    // The shape's own numbers stand: the Empower changes the rest (its wait among them).
                    SpellChange.Copy(spec, kept);
                    empower.ApplyTo(spec);
                    shape.Change.KeepOwn(spec, kept);
                }
                else empower?.ApplyTo(spec);
            }
            return spec;
        }

        /// <summary>Seconds the slot waits after the spell: the table's, the shape's, then the Empower's. For a base spell its kit casts, the base wait with the Empower's.</summary>
        public float CooldownOf(Upgrade learn, ICollection<string> learned, Loadout loadout)
        {
            if (learn == null) return SpellTimings.DefaultCooldown;
            var spec = SpecOf(learn, learned, loadout);
            if (spec != null) return spec.Cooldown;
            return BaseCooldown(learn.Id, SpellTimings.For(learn.Id).Cooldown, learned);
        }

        /// <summary>
        /// The wait of a base spell (by its base id, <c>fire.base</c>) with its Empower's, from <paramref name="usual"/> (the
        /// table's, already shortened by its element's marks).
        /// </summary>
        public static float BaseCooldown(string baseId, float usual, ICollection<string> learned)
        {
            if (!Empowered(baseId, learned)) return usual;
            var empower = EmpowerOf(baseId);
            if (empower == null || empower.Cooldown <= 0f) return usual;
            // The Empower's wait stands in for the table's; the marks' share is kept as a ratio.
            float table = SpellTimings.For(baseId).Cooldown;
            return table > 0f ? empower.Cooldown * usual / table : empower.Cooldown;
        }

        /// <summary>The Empower record of a branch, or null.</summary>
        public static Empower EmpowerOf(string branch) => Upgrade.ById(SpellBranch.EmpowerId(branch))?.Empower;

        /// <summary>Forgets the specs (a catalogue gathered again).</summary>
        public void Clear() => specs.Clear();
    }
}

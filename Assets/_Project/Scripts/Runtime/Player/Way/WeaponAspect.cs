namespace Clover.Player.Way
{
    /// <summary>
    /// One bough of a weapon's Way as code (IMPLEMENTATION.md 2.5; steel/weapons.md 0.1): what choosing it at a rest fire
    /// does to the weapon's moves. A bough's marks are data in its Way's tree file (<c>Trees/Ways/KatanaWay.cs</c>); what
    /// they change in the chain, the steps and the rules is here, because aspects change chains, add moves and change
    /// rules. <see cref="Armoury.Resolve(string, string, IMarkQuery)"/> builds the weapon: a god's bough from its own
    /// <see cref="Build"/>, any other from the plain weapon, then the trunk's patch and the aspect's.
    ///
    /// Stateless, like the spell forms: one instance serves every player, and what an aspect keeps while carried (a
    /// window, a count) lives in the player's <see cref="AspectRun"/>. A patch works on a freshly built weapon, so it
    /// never leaks into the next one. The first pass retimes and reorders the weapon's existing clips; a move that needs a
    /// clip of its own carries a <c>NEEDS</c> line and a line in docs/design/steel/content_open.tsv.
    /// </summary>
    public abstract class WeaponAspect
    {
        /// <summary>The aspect's id, which is also the id of the mark that opens it (<c>katana.sheath</c>).</summary>
        public abstract string Id { get; }

        /// <summary>The weapon kind whose Way it is on (<c>Weapons.KatanaName</c>).</summary>
        public abstract string Weapon { get; }

        /// <summary>True for a god's bough: the god's weapon itself, built by <see cref="Build"/>.</summary>
        public virtual bool GodBough => false;

        /// <summary>A god's bough builds its own weapon (the Storm Katana); null for a plain bough, which patches the plain weapon.</summary>
        public virtual Weapon Build() => null;

        /// <summary>What the aspect and its marks in force do to a freshly built weapon: steps retimed, added, replaced or taken out.</summary>
        public abstract void Patch(Weapon weapon, IMarkQuery marks);

        /// <summary>Each step while the aspect is carried, for the rules that count or keep a window. Most aspects keep nothing.</summary>
        public virtual void Tick(ref AspectRun run, in AspectContext context) { }

        /// <summary>A step's wind-up, swing and recovery times a share (below one is quicker).</summary>
        protected static void Retime(AttackStep step, float share)
        {
            if (step == null) return;
            step.Windup *= share;
            step.Active *= share;
            step.Recovery *= share;
        }

        /// <summary>A copy of one step of the chain, or null when the chain is shorter.</summary>
        protected static AttackStep CopyOf(Weapon weapon, int index) =>
            weapon?.Combo?.Steps != null && index >= 0 && index < weapon.Combo.Steps.Count ? weapon.Combo.Steps[index].Clone() : null;

        /// <summary>The chain's step at a place, or null when the chain is shorter.</summary>
        protected static AttackStep StepAt(Weapon weapon, int index) =>
            weapon?.Combo?.Steps != null && index >= 0 && index < weapon.Combo.Steps.Count ? weapon.Combo.Steps[index] : null;
    }
}

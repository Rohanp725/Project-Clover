using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// A spell form that lasts: blades that hang before they fall, a beam that is held, a wall that stands. The form
    /// itself still keeps nothing between casts; <see cref="SpellWorks"/> calls it every frame with the cast's own
    /// <see cref="SpellRun"/>, and tells it when the weapon is swung or lands, for forms that wait on the weapon.
    /// </summary>
    public abstract class RunningForm : ISpellForm
    {
        public abstract string Id { get; }

        public abstract int Cast(in SpellCast cast);

        /// <summary>One frame of a cast in progress. False when it is over.</summary>
        public abstract bool Tick(ref SpellRun run, float deltaTime);

        /// <summary>The run is over, however it ended (its time, the player's fall, the scene's end). What it held is put away after this.</summary>
        public virtual void End(ref SpellRun run) { }

        /// <summary>The body this run was holding has been taken from it (a blow landed, the dodge was pressed). False ends the run.</summary>
        public virtual bool BodyLost(ref SpellRun run) => false;

        /// <summary>The weapon was swung. Nothing may be struck from here: note it and act in <see cref="Tick"/>.</summary>
        public virtual void Swung(ref SpellRun run, AttackStep step, Vector3 origin, Vector3 forward) { }

        /// <summary>A blow of the weapon landed on something for this much. Nothing may be struck from here: note it and act in <see cref="Tick"/>.</summary>
        public virtual void Struck(ref SpellRun run, AttackStep step, Component target, float damage) { }

        /// <summary>The run is asked to last <paramref name="seconds"/> longer (a Master's reward); a form that cannot, ignores it.</summary>
        public virtual void Lengthen(ref SpellRun run, float seconds) { }
    }
}

namespace Clover.Player
{
    /// <summary>
    /// Numbers that replace a spell's own (<see cref="SpellSpec"/>) where they are set: an Empower mark's "stronger, shorter
    /// cooldown", or a shape that is the same spell with other numbers. A field left at zero leaves the spell's number as it
    /// is, so no number can be changed to zero this way (a shape that needs that gives a whole <see cref="ShapeDef.Spell"/>).
    /// </summary>
    public class SpellChange
    {
        /// <summary>Seconds the slot waits after a cast.</summary>
        public float Cooldown;
        /// <summary>Mana it costs (a larger spell).</summary>
        public float Mana;
        public float Damage;
        public float Stagger;
        public float Reach;
        public float Radius;
        public float Arc;
        public float Seconds;
        public float Interval;
        public int Count;
        public int Stacks;
        public float Delay;
        public float Speed;
        public float LingerDamage;
        public float AfflictionSeconds;

        /// <summary>True when it changes nothing.</summary>
        public bool IsEmpty =>
            Cooldown == 0f && Mana == 0f && Damage == 0f && Stagger == 0f && Reach == 0f && Radius == 0f && Arc == 0f && Seconds == 0f
            && Interval == 0f && Count == 0 && Stacks == 0 && Delay == 0f && Speed == 0f && LingerDamage == 0f && AfflictionSeconds == 0f;

        /// <summary>Writes every number it sets over <paramref name="spec"/>'s.</summary>
        public void ApplyTo(SpellSpec spec)
        {
            if (spec == null) return;
            if (Cooldown > 0f) spec.Cooldown = Cooldown;
            if (Mana > 0f) spec.Cost = Mana;
            if (Damage > 0f) spec.Damage = Damage;
            if (Stagger > 0f) spec.Stagger = Stagger;
            if (Reach > 0f) spec.Reach = Reach;
            if (Radius > 0f) spec.Radius = Radius;
            if (Arc > 0f) spec.Arc = Arc;
            if (Seconds > 0f) spec.Seconds = Seconds;
            if (Interval > 0f) spec.Interval = Interval;
            if (Count > 0) spec.Count = Count;
            if (Stacks > 0) spec.Stacks = Stacks;
            if (Delay > 0f) spec.Delay = Delay;
            if (Speed > 0f) spec.Speed = Speed;
            if (LingerDamage > 0f) spec.LingerDamage = LingerDamage;
            if (AfflictionSeconds > 0f) spec.AfflictionSeconds = AfflictionSeconds;
        }

        /// <summary>Copies every number of one spec into another (no allocation): how a spell's own numbers are taken before changes are laid over them.</summary>
        public static void Copy(SpellSpec from, SpellSpec into)
        {
            if (from == null || into == null) return;
            into.Cost = from.Cost;
            into.Cooldown = from.Cooldown;
            into.Damage = from.Damage;
            into.Stagger = from.Stagger;
            into.Reach = from.Reach;
            into.Radius = from.Radius;
            into.Arc = from.Arc;
            into.Seconds = from.Seconds;
            into.Interval = from.Interval;
            into.Count = from.Count;
            into.Stacks = from.Stacks;
            into.AfflictionSeconds = from.AfflictionSeconds;
            into.Delay = from.Delay;
            into.Speed = from.Speed;
            into.Height = from.Height;
            into.LingerDamage = from.LingerDamage;
            into.HealthCost = from.HealthCost;
        }
    }

    /// <summary>An Empower mark's change: "stronger, shorter cooldown" (elements_a.md, rules), as the numbers it sets.</summary>
    public class Empower : SpellChange
    {
    }
}

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
        /// <summary>The blows a charge of the blade from the spell lasts (<see cref="SpellSpec.ChargeBlows"/>).</summary>
        public int ChargeBlows;

        /// <summary>True when it changes nothing.</summary>
        public bool IsEmpty =>
            Cooldown == 0f && Mana == 0f && Damage == 0f && Stagger == 0f && Reach == 0f && Radius == 0f && Arc == 0f && Seconds == 0f
            && Interval == 0f && Count == 0 && Stacks == 0 && Delay == 0f && Speed == 0f && LingerDamage == 0f && AfflictionSeconds == 0f && ChargeBlows == 0;

        /// <summary>
        /// Writes what it adds to <paramref name="learn"/>'s numbers onto <paramref name="spec"/>'s (an Empower over a shape that
        /// takes its rise: <see cref="EmpowerOverShape.Adds"/>). Its wait and price are set as written.
        /// </summary>
        public void AddOver(SpellSpec spec, SpellSpec learn)
        {
            if (spec == null) return;
            if (learn == null)
            {
                ApplyTo(spec);
                return;
            }
            if (Cooldown > 0f) spec.Cooldown = Cooldown;
            if (Mana > 0f) spec.Cost = Mana;
            if (Damage > 0f) spec.Damage = System.Math.Max(0f, spec.Damage + Damage - learn.Damage);
            if (Stagger > 0f) spec.Stagger = System.Math.Max(0f, spec.Stagger + Stagger - learn.Stagger);
            if (Reach > 0f) spec.Reach = System.Math.Max(0.1f, spec.Reach + Reach - learn.Reach);
            if (Radius > 0f) spec.Radius = System.Math.Max(0.1f, spec.Radius + Radius - learn.Radius);
            if (Arc > 0f) spec.Arc = System.Math.Max(1f, spec.Arc + Arc - learn.Arc);
            if (Seconds > 0f) spec.Seconds = System.Math.Max(0.05f, spec.Seconds + Seconds - learn.Seconds);
            if (Interval > 0f) spec.Interval = System.Math.Max(0.05f, spec.Interval + Interval - learn.Interval);
            if (Count > 0) spec.Count = System.Math.Max(1, spec.Count + Count - learn.Count);
            if (Stacks > 0) spec.Stacks = System.Math.Max(0, spec.Stacks + Stacks - learn.Stacks);
            if (Delay > 0f) spec.Delay = System.Math.Max(0f, spec.Delay + Delay - learn.Delay);
            if (Speed > 0f) spec.Speed = System.Math.Max(0.1f, spec.Speed + Speed - learn.Speed);
            if (LingerDamage > 0f) spec.LingerDamage = System.Math.Max(0f, spec.LingerDamage + LingerDamage - learn.LingerDamage);
            if (AfflictionSeconds > 0f) spec.AfflictionSeconds = System.Math.Max(0f, spec.AfflictionSeconds + AfflictionSeconds - learn.AfflictionSeconds);
        }

        /// <summary>Puts back from <paramref name="before"/> every number this change sets (a shape keeping its own under an Empower).</summary>
        public void KeepOwn(SpellSpec spec, SpellSpec before)
        {
            if (spec == null || before == null) return;
            if (Damage > 0f) spec.Damage = before.Damage;
            if (Stagger > 0f) spec.Stagger = before.Stagger;
            if (Reach > 0f) spec.Reach = before.Reach;
            if (Radius > 0f) spec.Radius = before.Radius;
            if (Arc > 0f) spec.Arc = before.Arc;
            if (Seconds > 0f) spec.Seconds = before.Seconds;
            if (Interval > 0f) spec.Interval = before.Interval;
            if (Count > 0) spec.Count = before.Count;
            if (Stacks > 0) spec.Stacks = before.Stacks;
            if (Delay > 0f) spec.Delay = before.Delay;
            if (Speed > 0f) spec.Speed = before.Speed;
            if (LingerDamage > 0f) spec.LingerDamage = before.LingerDamage;
            if (AfflictionSeconds > 0f) spec.AfflictionSeconds = before.AfflictionSeconds;
        }

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
            if (ChargeBlows > 0) spec.ChargeBlows = ChargeBlows;
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
            into.MissedWait = from.MissedWait;
            // What it seeks and how it ends go with the numbers: a branch's spell cast through its Learn keeps its aim.
            into.AimAt = from.AimAt;
            into.AimAt2 = from.AimAt2;
            into.AimFarthest = from.AimFarthest;
            into.AimMissShare = from.AimMissShare;
            into.OnlyCarrying = from.OnlyCarrying;
            into.OnlyCarrying2 = from.OnlyCarrying2;
            into.Seal = from.Seal;
            into.PerDose = from.PerDose;
            into.PassReach = from.PassReach;
            into.StepReach = from.StepReach;
            into.ChargeBlows = from.ChargeBlows;
        }
    }

    /// <summary>
    /// An Empower mark's change, as the numbers it sets: a stronger spell or a shorter wait, or both (the owner's answer of
    /// 2026-11-20: an Empower may do either). Besides the spell's own numbers it may change the numbers of its Learn's own
    /// effects (<see cref="Effects"/>: a burst's mending, a pour's damage a notch), which stand in for the Learn's while the
    /// Empower is in force.
    /// </summary>
    public class Empower : SpellChange
    {
        /// <summary>Changes to the Learn mark's own effects, each by its place in the Learn's list; null for none.</summary>
        public EffectChange[] Effects;
        /// <summary>
        /// True when its spell numbers are laid only over the Learn's own spell, not over a worn shape that brings a whole
        /// spell of its own (Daybreak's 10 a ring is the beam's, not Red Before the Sun's wall or High Noon's column).
        /// </summary>
        public bool LearnOnly;
        /// <summary>For a base spell's Empower: its numbers are not the kit's cast's own (Flung Blade's 16 is the drive's, not the throw's), so the kit takes only its wait.</summary>
        public bool NotForKit;

        /// <summary>True when it changes nothing at all: no spell number, no wait or price, and no effect of its Learn.</summary>
        public bool DoesNothing => IsEmpty && (Effects == null || Effects.Length == 0);

        /// <summary>True when it makes the spell stronger (any number but its wait and price, or one of its Learn's effects).</summary>
        public bool Strengthens
        {
            get
            {
                var numbers = new SpellChange
                {
                    Damage = Damage, Stagger = Stagger, Reach = Reach, Radius = Radius, Arc = Arc, Seconds = Seconds, Interval = Interval,
                    Count = Count, Stacks = Stacks, Delay = Delay, Speed = Speed, LingerDamage = LingerDamage, AfflictionSeconds = AfflictionSeconds,
                };
                return !numbers.IsEmpty || (Effects != null && Effects.Length > 0);
            }
        }
    }

    /// <summary>
    /// New numbers for one of a Learn mark's effects, set by its Empower (the ledger's empower-effects). A number left at
    /// zero leaves the effect's own.
    /// </summary>
    public class EffectChange
    {
        /// <summary>The effect's place in its Learn mark's <see cref="Upgrade.Effects"/>.</summary>
        public int Index;
        public float Damage;
        public float Stagger;
        public float Amount;
        public float Radius;
        public float Seconds;
        public int Stacks;
        public int Count;
        public float Reach;
        public float Interval;
        public float Delay;
        public float Arc;
        /// <summary>For an effect multiplied by a count: the most it is multiplied by (Reckoning's 180 at most).</summary>
        public int PerMost;
        /// <summary>For an effect multiplied by a count: added to the count first (Dry Pods' one more seed).</summary>
        public int PerMore;
        /// <summary>For an effect that sets a condition: the condition's own seconds (a copy of it is changed, never the original).</summary>
        public float ConditionSeconds;

        /// <summary>Writes every number it sets over the effect's.</summary>
        public void ApplyTo(UpgradeEffect effect)
        {
            if (effect == null) return;
            if (Damage > 0f) effect.Damage = Damage;
            if (Stagger > 0f) effect.Stagger = Stagger;
            if (Amount > 0f) effect.Amount = Amount;
            if (Radius > 0f) effect.Radius = Radius;
            if (Seconds > 0f) effect.Seconds = Seconds;
            if (Stacks > 0) effect.Stacks = Stacks;
            if (Count > 0) effect.Count = Count;
            if (Reach > 0f) effect.Reach = Reach;
            if (Interval > 0f) effect.Interval = Interval;
            if (Delay > 0f) effect.Delay = Delay;
            if (Arc > 0f) effect.Arc = Arc;
            if (PerMost > 0) effect.PerMost = PerMost;
            if (PerMore > 0) effect.PerMore = PerMore;
            if (ConditionSeconds > 0f && effect.State != null)
            {
                effect.State = effect.State.Copy();
                effect.State.Seconds = ConditionSeconds;
            }
        }
    }

    /// <summary>
    /// New numbers for one effect of a reaction, set by a mark of its duo branch while that mark is in force (the ledger's
    /// reaction-numbers: Dry Pods' fourth seed, Deep Swell's wider burst). <see cref="Shape"/> says which list: the plain
    /// reaction's own effects, or one of its two shapes'.
    /// </summary>
    public class ReactionChange : EffectChange
    {
        /// <summary>-1: the plain reaction's effects; 0 or 1: the first or second shape's.</summary>
        public int Shape = -1;
    }
}

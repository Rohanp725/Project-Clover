using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The numbers of one affliction. Everything "per stack" is multiplied by how deep the affliction lies. A field
    /// left at zero means the affliction does not do that thing.
    /// </summary>
    public class AfflictionSpec
    {
        public AfflictionKind Kind;
        public string Name = "";
        /// <summary>The colour of its pip over an enemy's bars.</summary>
        public Color Colour = Color.white;
        public int MaximumStacks = 5;
        /// <summary>Seconds it lasts; laying it again starts the time over.</summary>
        public float Seconds = 6f;
        /// <summary>Damage each stack deals each time it bites, and the seconds between bites.</summary>
        public float DamagePerTick;
        public float TickSeconds;
        /// <summary>Share of its speed each stack takes from the afflicted.</summary>
        public float SlowPerStack;
        /// <summary>Share of extra damage each stack lets through, from spells and from weapons.</summary>
        public float SpellDamageTakenPerStack;
        public float WeaponDamageTakenPerStack;
        /// <summary>Share of extra stagger each stack lets through.</summary>
        public float StaggerTakenPerStack;
        /// <summary>How much further each stack lets a blow shove it, and how much of its turning each stack takes.</summary>
        public float ShovePerStack;
        public float TurnSlowPerStack;
        /// <summary>Share of its own damage each stack takes away.</summary>
        public float DealsLessPerStack;
        /// <summary>True when its balance does not drain back while this lasts.</summary>
        public bool StopsBalanceRecovery;
        /// <summary>True when reaching full depth freezes it: held still until the time runs out or a heavy blow shatters it.</summary>
        public bool FreezesAtFull;
        public float FrozenSeconds;
        public float ShatterDamage;
        /// <summary>True when a weapon blow on it is always critical; mana each stack adds to such a blow.</summary>
        public bool MakesBlowsCritical;
        public float ManaPerStack;
        /// <summary>Health each stack gives the striker for a weapon blow on it.</summary>
        public float HealOnBlowPerStack;
        /// <summary>When it dies: damage each stack deals to everyone within the radius.</summary>
        public float DeathBurstPerStack;
        public float DeathBurstRadius;
        /// <summary>When it dies: health each stack gives the player.</summary>
        public float HealOnKillPerStack;
        /// <summary>Laid on something carrying this one, these two are also laid on everyone within the radius.</summary>
        public AfflictionKind Carries = AfflictionKind.None;
        public AfflictionKind AlsoCarries = AfflictionKind.None;
        public float CarryRadius;
        /// <summary>Damage each stack deals when the affliction is set off: spent all at once.</summary>
        public float SetOffPerStack = 8f;
    }
}

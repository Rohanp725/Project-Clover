namespace Clover.Player.Build
{
    /// <summary>One thing the Hand offers for a slot or for the surge: what it is, whose it is, its role and one plain line.</summary>
    public struct HandChoice
    {
        /// <summary>The mark id, the base id (<c>fire.base</c>) or the surge id (<c>fire.surge</c>).</summary>
        public string Id;
        public string Name;
        /// <summary>Its element; for a duo spell the first of its pair, with the other in <see cref="Element2"/>.</summary>
        public string Element;
        public string Element2;
        public SpellRole Role;
        /// <summary>One plain line on what it does.</summary>
        public string Line;
        public bool IsBase;
        public bool IsDuo;
        /// <summary>The slot it sits in now, or -1.</summary>
        public int InSlot;
        /// <summary>Paid in mana (its price), not by a cooldown; 0 for a cooldown spell.</summary>
        public float Mana;
        public float Cooldown;
    }
}

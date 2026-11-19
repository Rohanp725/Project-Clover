namespace Clover.Player
{
    /// <summary>How a spell is paid for: a cooldown in seconds, or mana for an element's larger spell, never both.</summary>
    public readonly struct SpellTiming
    {
        public readonly float Cooldown;
        public readonly float Mana;
        public readonly SpellRole Role;

        public SpellTiming(float cooldown, float mana, SpellRole role)
        {
            Cooldown = cooldown;
            Mana = mana;
            Role = role;
        }

        /// <summary>Paid in mana: an element's larger spell or a built ring-mix duo spell.</summary>
        public bool IsLarger => Mana > 0f;
    }
}

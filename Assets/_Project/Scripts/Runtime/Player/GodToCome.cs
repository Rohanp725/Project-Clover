namespace Clover.Player
{
    /// <summary>One god of the full game as docs/design/WORLD.md and docs/design/steel/README.md have them, and what of them is built.</summary>
    public readonly struct GodToCome
    {
        /// <summary>The element whose tree the god's things sit beside on the chart.</summary>
        public readonly string Element;
        public readonly string God;
        /// <summary>The weapon kind the god's own weapon is a finer version of; its Way will hold the god's bough.</summary>
        public readonly string WeaponKind;
        public readonly string GodWeapon;
        /// <summary>True when the god is fought in a built scene.</summary>
        public readonly bool GodBuilt;
        /// <summary>The god's weapon's name in <see cref="Weapons"/> when it can be taken up now; empty while it is not built.</summary>
        public readonly string BuiltWeapon;

        public GodToCome(string element, string god, string weaponKind, string godWeapon, bool godBuilt, string builtWeapon)
        {
            Element = element;
            God = god;
            WeaponKind = weaponKind;
            GodWeapon = godWeapon;
            GodBuilt = godBuilt;
            BuiltWeapon = builtWeapon;
        }

        public bool WeaponBuilt => !string.IsNullOrEmpty(BuiltWeapon);
    }
}

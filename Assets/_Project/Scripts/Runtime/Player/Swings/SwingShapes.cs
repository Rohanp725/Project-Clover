namespace Clover.Player.Swings
{
    /// <summary>
    /// Each weapon's swing shape (the owner's note of 2026-11-19: a close look at each weapon and its animation). The
    /// trail is always built from the real path of the weapon's blade line; these say how much of that line it covers
    /// and how heavy it reads. The speeds come from the animations' own pace: the knives' flicks are the fastest tips in
    /// the game, the hammer's head the slowest and the heaviest.
    /// </summary>
    public static class SwingShapes
    {
        public static readonly SwingShape Katana = new SwingShape
        {
            Weapon = Weapons.KatanaName, SweepFrom = 0.4f, SweepTo = 1.03f, Width = 0.8f, Life = 0.85f, LeastSpeed = 2.5f, FullSpeed = 9f,
            Note = "a thin fast arc off the outer half of the edge, gone almost as soon as it is drawn",
        };

        public static readonly SwingShape MassiveSword = new SwingShape
        {
            Weapon = Weapons.MassiveSwordName, SweepFrom = 0.15f, SweepTo = 1.05f, Width = 1.4f, Life = 1.3f, LeastSpeed = 2f, FullSpeed = 7f, GroundImpact = true,
            Note = "a broad sheet the whole length of the slab, slow to fade; the overhead that buries itself throws the element on the ground",
        };

        public static readonly SwingShape Scythe = new SwingShape
        {
            Weapon = Weapons.ScytheName, SweepFrom = 0f, SweepTo = 0.72f, Width = 1.3f, Life = 1.25f, LeastSpeed = 2f, FullSpeed = 8f,
            Note = "a great crescent from the top of the haft out past the bend of the blade, where the reach is",
        };

        public static readonly SwingShape Spear = new SwingShape
        {
            Weapon = Weapons.GreatSpearName, SweepFrom = 0.2f, SweepTo = 1.05f, Width = 1f, Life = 1.3f, LeastSpeed = 2.5f, FullSpeed = 9f, StrandWidth = 0.6f, GroundImpact = true,
            // Filmed 2026-11-20 a thrust straight up the screen left only a stub about the point: a wider band, kept longer.
            Note = "a line along each thrust drawn about the point, and a narrow arc for the sweep",
        };

        public static readonly SwingShape Hammer = new SwingShape
        {
            Weapon = Weapons.GreatHammerName, SweepFrom = -0.6f, SweepTo = 1.1f, Width = 1.6f, Life = 1.35f, LeastSpeed = 1.5f, FullSpeed = 5.5f, GroundImpact = true,
            Note = "a heavy wide smear from down the haft out past the head, and the element thrown on the ground where a slam lands",
        };

        public static readonly SwingShape Knives = new SwingShape
        {
            Weapon = Weapons.TwinKnivesName, SweepFrom = 0.15f, SweepTo = 1.08f, Width = 0.7f, Life = 0.6f, LeastSpeed = 3.5f, FullSpeed = 10f,
            Note = "quick paired slashes, one off each hand, short and sharp",
        };

        public static readonly SwingShape Bow = new SwingShape
        {
            Weapon = Weapons.GreatBowName, SweepFrom = 0.35f, SweepTo = 1.02f, Width = 0.9f, Life = 0.9f, LeastSpeed = 2.5f, FullSpeed = 8f, StrandWidth = 0.18f, GroundImpact = true,
            Note = "the limbs' arcs for the swat, a line behind the arrow in the fist for the stab and behind the arrow in flight for the shots",
        };

        public static readonly SwingShape Sickle = new SwingShape
        {
            Weapon = Weapons.ChainSickleName, SweepFrom = 0f, SweepTo = 1.02f, Width = 0.8f, Life = 1.1f, LeastSpeed = 2.5f, FullSpeed = 9f, StrandWidth = 0.3f,
            Note = "a hooked arc off the sickle's concave edge, and a line behind the weight: down the lane, round and round in a spiral for the whirl",
        };

        public static readonly SwingShape Fists = new SwingShape
        {
            Weapon = Weapons.FistsName, SweepFrom = 0.6f, SweepTo = 1f, Width = 0.8f, Life = 0.6f, LeastSpeed = 3f, FullSpeed = 9f, StrandWidth = 0.24f,
            Note = "short punches: a stub of a line behind each fist or foot that strikes",
        };

        public static readonly SwingShape Armament = new SwingShape
        {
            Weapon = Weapons.ArmamentName, SweepFrom = 0.6f, SweepTo = 1f, Width = 1f, Life = 0.7f, LeastSpeed = 3f, FullSpeed = 9f, StrandWidth = 0.3f, GroundImpact = true,
            Note = "the fists' punches, a little fuller: the coat is the element",
        };

        public static readonly SwingShape WarFan = new SwingShape
        {
            Weapon = "War Fan", SweepFrom = 0.1f, SweepTo = 1.05f, Width = 1.2f, Life = 1.1f, LeastSpeed = 2f, FullSpeed = 8f,
            Note = "planned: the whole opened fan sweeps, pivot to rim, a broad fan-shaped trail",
        };

        /// <summary>Every shape, for the tests and the guide.</summary>
        public static readonly SwingShape[] All = { Katana, MassiveSword, Scythe, Spear, Hammer, Knives, Bow, Sickle, Fists, Armament, WarFan };

        /// <summary>The shape a weapon swings with: by its carry, so the Storm Katana swings as the Katana and the Sun-Spear as the spear.</summary>
        public static SwingShape For(Weapon weapon)
        {
            if (weapon == null) return Katana;
            if (weapon.UsesSword) return Katana;
            switch (weapon.Carry)
            {
                case "gsword": return MassiveSword;
                case "scythe": return Scythe;
                case "spear":
                case "sunspear": return Spear;
                case "hammer": return Hammer;
                case "knife": return Knives;
                case "bow": return Bow;
                case "sickle": return Sickle;
                case "arm": return Armament;
                case BladeLines.FanCarry: return WarFan;
                default: return string.IsNullOrEmpty(weapon.Carry) ? Fists : Katana;
            }
        }
    }
}

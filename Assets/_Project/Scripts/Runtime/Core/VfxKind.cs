namespace Clover.Core
{
    /// <summary>The one-shot effects the game can throw at a point. The scene's <see cref="Vfx"/> holds one emitter for each.</summary>
    public enum VfxKind
    {
        /// <summary>Steel on a body: a short spray of sparks along the blow.</summary>
        HitSpark,
        /// <summary>A heavy blow or a finisher: a larger, longer spray.</summary>
        HeavySpark,
        /// <summary>A blocked blow: a tight white flash on the guard.</summary>
        GuardSpark,
        /// <summary>A perfect guard: a bright ring and a burst.</summary>
        PerfectGuard,
        /// <summary>Feet on the ground: a low puff of dust.</summary>
        Dust,
        /// <summary>The player is struck.</summary>
        Wound,
        /// <summary>A flask taking effect: slow rising motes.</summary>
        Heal,
        /// <summary>A burst of flame.</summary>
        Fire,
        /// <summary>Embers left drifting where fire has been.</summary>
        Embers,
        /// <summary>A crackle of lightning at a point.</summary>
        Shock,
        /// <summary>The flash where something dies or is finished.</summary>
        Burst,
        /// <summary>A single ring that widens along the ground or in the air.</summary>
        Ring,
        /// <summary>A hard-edged star for one instant where a heavy blow lands: the comic-book mark of an impact.</summary>
        Impact,
        /// <summary>A small white glint over an enemy the instant it commits to an attack that can be guarded.</summary>
        Glint,
        /// <summary>A red flare over an enemy the instant it commits to an attack that cannot be guarded.</summary>
        Danger,
        /// <summary>Steel on steel: the cold burst where a blow is turned on a guard.</summary>
        Clang,
    }
}

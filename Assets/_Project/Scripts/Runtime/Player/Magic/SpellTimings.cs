using System;
using System.Collections.Generic;

namespace Clover.Player.Magic
{
    /// <summary>
    /// How every built spell is paid for, by the shared rules of docs/design/steel/README.md: basic spells run on
    /// cooldowns of 6 to 12 s and never cost mana; each element has one larger spell paid in mana (35 to 50); the eleven
    /// built ring-mix duo spells stay larger spells paid in mana (40 to 60); the twelve spare ring-mix spells, which
    /// become Evolutions later, take a duo spell's cooldown. Numbers come from the rosters in elements_a.md and
    /// elements_b.md where the spell is listed there, and from its role where it is not (marked "not in the roster").
    /// Applied to the catalogue as it is gathered (<see cref="Apply"/>), so a spell's <see cref="SpellSpec.Cooldown"/>
    /// and <see cref="SpellSpec.Cost"/> are the table's. Earth has no larger spell yet: the design gives it the new Cairn.
    /// </summary>
    public static class SpellTimings
    {
        /// <summary>A spell the table does not know (one lent by the old yard's rack, a probe's own): a middling cooldown.</summary>
        public const float DefaultCooldown = 10f;
        public const float DuoCooldown = 12f;

        private static SpellTiming Cd(float seconds, SpellRole role) => new SpellTiming(seconds, 0f, role);
        private static SpellTiming Mana(float mana, SpellRole role) => new SpellTiming(0f, mana, role);

        // The base (tap) spells, by base id (SpellSlots.BaseId): elements_a.md and elements_b.md, "(built, tap)".
        private static readonly Dictionary<string, SpellTiming> bases = new Dictionary<string, SpellTiming>
        {
            ["fire.base"] = Cd(7f, SpellRole.Opener),        // Flame Wave
            ["lightning.base"] = Cd(9f, SpellRole.Breaker),  // Thunderclap
            ["wind.base"] = Cd(6f, SpellRole.Opener),        // Gust
            ["ice.base"] = Cd(7f, SpellRole.Opener),         // Hoarfrost
            ["water.base"] = Cd(6f, SpellRole.Opener),       // Riptide
            ["life.base"] = Cd(7f, SpellRole.Opener),        // Briar Patch
            ["earth.base"] = Cd(7f, SpellRole.Opener),       // Stone Rise
            ["metal.base"] = Cd(6f, SpellRole.Opener),       // Flung Blade
            ["light.base"] = Cd(6f, SpellRole.Opener),       // Glare
            ["dark.base"] = Cd(7f, SpellRole.Opener),        // Grasp
            ["poison.base"] = Cd(7f, SpellRole.Opener),      // Miasma
        };

        private static readonly Dictionary<string, SpellTiming> spells = new Dictionary<string, SpellTiming>
        {
            // Fire, reshaped into spell branches (Phase 4): the roster of elements_a.md, Flame Wave being the base spell.
            // Sunspot and Cinder Fan are retired (Black Pitch and a shape of Flame Wave took their places).
            ["fire.uraeus"] = Cd(10f, SpellRole.Opener),
            ["fire.rush"] = Cd(8f, SpellRole.Movement),
            ["fire.meteor"] = Mana(45f, SpellRole.Breaker),          // Fire's larger spell
            ["fire.kiln"] = Cd(9f, SpellRole.Counter),
            ["fire.pitch"] = Cd(11f, SpellRole.Detonator),
            // Lightning, reshaped (Phase 4): Thunderclap is the base spell; Lodestone is retired into Lightning Rod's shape.
            ["lightning.lance"] = Cd(7f, SpellRole.Opener),
            ["lightning.waiting"] = Cd(10f, SpellRole.Breaker),
            ["lightning.flashes"] = Mana(45f, SpellRole.Detonator), // Lightning's larger spell
            ["lightning.rod"] = Cd(10f, SpellRole.Movement),
            ["lightning.drums"] = Cd(10f, SpellRole.Counter),
            // Wind, reshaped (Phase 4): Gust is the base spell; the Bag of Winds is new.
            ["wind.lifting"] = Cd(9f, SpellRole.Breaker),
            ["wind.indrawn"] = Cd(11f, SpellRole.Detonator),
            ["wind.circle"] = Cd(9f, SpellRole.Movement),
            ["wind.bag"] = Cd(10f, SpellRole.Counter),
            ["wind.skydance"] = Mana(45f, SpellRole.Breaker),       // Wind's larger spell
            // Ice, reshaped (Phase 4): Hoarfrost is the base spell; Hollow Statue is retired; Rime Guard and Ski Run are new.
            ["ice.eight"] = Cd(11f, SpellRole.Detonator),
            ["ice.hanging"] = Cd(10f, SpellRole.Breaker),
            ["ice.wall"] = Mana(35f, SpellRole.Breaker),            // Ice's larger spell
            ["ice.rime"] = Cd(9f, SpellRole.Counter),
            ["ice.ski"] = Cd(8f, SpellRole.Movement),
            // Water
            ["water.serpent"] = Cd(9f, SpellRole.Opener),           // not in the roster
            ["water.whirlpool"] = Cd(10f, SpellRole.Breaker),
            ["water.rain"] = Cd(10f, SpellRole.Detonator),          // not in the roster
            ["water.moon"] = Cd(10f, SpellRole.Counter),            // Blowhole's place in the roster
            ["water.poured"] = Mana(30f, SpellRole.Breaker),        // Water's larger spell
            // Life
            ["life.harvest"] = Cd(12f, SpellRole.Breaker),          // Drawn From the Wood; also paid in health
            ["life.reap"] = Cd(12f, SpellRole.Detonator),
            ["life.tree"] = Mana(40f, SpellRole.Breaker),           // Life's larger spell
            ["life.ring"] = Cd(10f, SpellRole.Counter),             // not in the roster
            // Earth (its larger spell, Cairn, is not built)
            ["earth.stance"] = Cd(10f, SpellRole.Counter),
            ["earth.quakestep"] = Cd(9f, SpellRole.Breaker),
            ["earth.cleaver"] = Cd(10f, SpellRole.Breaker),         // not in the roster
            ["earth.fissure"] = Cd(10f, SpellRole.Breaker),
            ["earth.rampart"] = Cd(10f, SpellRole.Counter),         // not in the roster
            // Metal
            ["metal.circle"] = Cd(10f, SpellRole.Counter),
            ["metal.hone"] = Cd(9f, SpellRole.Breaker),             // not in the roster
            ["metal.returningedge"] = Cd(9f, SpellRole.Breaker),
            ["metal.graveofswords"] = Mana(50f, SpellRole.Breaker), // Metal's larger spell
            // Light
            ["light.step"] = Cd(8f, SpellRole.Movement),
            ["light.spears"] = Cd(10f, SpellRole.Breaker),
            ["light.reckoning"] = Cd(12f, SpellRole.Detonator),
            ["light.lantern"] = Cd(12f, SpellRole.Counter),
            ["light.daybreak"] = Mana(45f, SpellRole.Detonator),    // Light's larger spell
            // Dark (its larger spell, Pawned Heart, is paid in health and keeps a long cooldown)
            ["dark.debtscalled"] = Cd(10f, SpellRole.Breaker),
            ["dark.pawnedheart"] = Cd(40f, SpellRole.Breaker),
            ["dark.maw"] = Cd(11f, SpellRole.Detonator),
            ["dark.secondshadow"] = Cd(12f, SpellRole.Movement),
            ["dark.deadstand"] = Cd(12f, SpellRole.Breaker),        // not in the roster
            // Poison
            ["poison.coiled"] = Cd(10f, SpellRole.Counter),
            ["poison.blister"] = Cd(10f, SpellRole.Detonator),
            ["poison.onebyone"] = Mana(50f, SpellRole.Detonator),   // Poison's larger spell
            ["poison.censer"] = Cd(8f, SpellRole.Opener),           // folded into Miasma's Censer shape
            ["poison.sharealike"] = Cd(9f, SpellRole.Movement),

            // The eleven built ring-mix duo spells: their pair's larger spell, paid in mana as built.
            ["plasma.ball"] = Mana(40f, SpellRole.Detonator),
            ["tempest.eye"] = Mana(50f, SpellRole.Breaker),
            ["blizzard.avalanche"] = Mana(50f, SpellRole.Breaker),
            ["glacier.wave"] = Mana(50f, SpellRole.Breaker),
            ["bloom.vine"] = Mana(45f, SpellRole.Breaker),
            ["overgrowth.stoneoak"] = Mana(45f, SpellRole.Opener),
            ["forge.drawnfromstone"] = Mana(50f, SpellRole.Breaker),
            ["halo.sixmirrors"] = Mana(60f, SpellRole.Detonator),
            ["eclipse.duskdawn"] = Mana(45f, SpellRole.Movement),
            ["blight.wastinggrip"] = Mana(40f, SpellRole.Opener),
            ["pyre.lightthepyres"] = Mana(60f, SpellRole.Detonator),

            // The twelve spare ring-mix spells (Evolutions to come): a duo spell's cooldown.
            ["plasma.lance"] = Cd(DuoCooldown, SpellRole.Opener),
            ["tempest.cloud"] = Cd(DuoCooldown, SpellRole.Breaker),
            ["blizzard.whiteout"] = Cd(DuoCooldown, SpellRole.Breaker),
            ["glacier.creeping"] = Cd(DuoCooldown, SpellRole.Opener),
            ["bloom.lotus"] = Cd(DuoCooldown, SpellRole.Movement),
            ["overgrowth.thornwall"] = Cd(DuoCooldown, SpellRole.Counter),
            ["forge.triphammer"] = Cd(DuoCooldown, SpellRole.Breaker),
            ["halo.sunwheels"] = Cd(DuoCooldown, SpellRole.Opener),
            ["eclipse.blacksun"] = Cd(DuoCooldown, SpellRole.Detonator),
            ["blight.gravesoil"] = Cd(DuoCooldown, SpellRole.Breaker),
            ["pyre.fireeater"] = Cd(DuoCooldown, SpellRole.Breaker),
            ["pyre.slowfuse"] = Cd(DuoCooldown, SpellRole.Detonator),
        };

        /// <summary>The ids the table names, for tests.</summary>
        public static IEnumerable<string> Ids => spells.Keys;

        public static bool Knows(string id) => id != null && spells.ContainsKey(id);

        /// <summary>A spell's timing, by mark id or by base id (<c>fire.base</c>); a cooldown of <see cref="DefaultCooldown"/> when unknown.</summary>
        public static SpellTiming For(string id)
        {
            if (id != null && (spells.TryGetValue(id, out var found) || bases.TryGetValue(id, out found))) return found;
            return Cd(DefaultCooldown, SpellRole.None);
        }

        /// <summary>The element's base spell's timing.</summary>
        public static SpellTiming Base(string element) =>
            element != null && bases.TryGetValue(SpellSlots.BaseId(element), out var found) ? found : Cd(DefaultCooldown, SpellRole.Opener);

        /// <summary>
        /// What one mark takes off the wait of <paramref name="element"/>'s base spell (stat
        /// <see cref="PlayerStat.BaseSpellCooldown"/>): the element's own marks only, so carrying Earth second, Near to Hand
        /// shortens Stone Rise and not the first element's spell.
        /// </summary>
        public static float BaseCooldownShare(Upgrade mark, string element)
        {
            if (mark == null || mark.Stats == null || string.IsNullOrEmpty(element) || mark.IsMix || mark.Element != element) return 0f;
            float share = 0f;
            foreach (var mod in mark.Stats)
                if (mod.Stat == PlayerStat.BaseSpellCooldown && mod.When == StatWhen.Always && mod.Against == Clover.Gameplay.AfflictionKind.None) share += mod.Amount;
            return share;
        }

        /// <summary>The element's base spell's wait with its marks' share taken off; never less than a tenth of the table's.</summary>
        public static float BaseCooldown(string element, float share) =>
            Base(element).Cooldown * Math.Max(PlayerStats.LeastMultiplier, 1f + share);

        /// <summary>Gives a gathered spell mark the table's cooldown and price. Health prices are left as written.</summary>
        public static void Apply(Upgrade mark)
        {
            if (mark == null || mark.Spell == null || !mark.IsSpell) return;
            var timing = For(mark.Id);
            mark.Spell.Cooldown = timing.Cooldown;
            mark.Spell.Cost = timing.Mana;
        }
    }
}

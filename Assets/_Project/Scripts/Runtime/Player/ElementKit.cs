using System;
using Clover.Gameplay;

namespace Clover.Player
{
    /// <summary>What an element's spell does when it leaves the hand.</summary>
    public enum SpellShape
    {
        /// <summary>A cone thrown ahead that strikes everything in it.</summary>
        Wave,
        /// <summary>A bolt on the enemy aimed at, that leaps on to others near it.</summary>
        Bolt,
    }

    /// <summary>
    /// The base kit every element gives: one spell and one surge, both paid for with mana that only fighting earns. The
    /// spell is a short committed cast, so it serves the melee and does not replace it. The surge is the element's self
    /// buff: for a while every blow of the weapon carries the element. Plain C#: the player feeds it time and acts when
    /// it says the spell is released. Fire and Lightning are tuned; the other nine are plain working kits, each in
    /// its element's colour with its affliction, standing in until their own spells are made.
    /// </summary>
    [Serializable]
    public class ElementKit
    {
        public const string FireName = Elements.Fire;
        public const string LightningName = Elements.Lightning;

        public string Name = FireName;
        public string SpellName = "Flame Wave";
        public string SurgeName = "Kindle";
        public SpellShape Shape = SpellShape.Wave;
        public float SpellCost = 25f;
        /// <summary>Seconds from the press to the spell leaving the hand, and from there until the player can act again.</summary>
        public float SpellWindup = 0.16f;
        public float SpellRecovery = 0.3f;
        public float SpellDamage = 20f;
        public float SpellStagger = 24f;
        public float SpellReach = 5.5f;
        public float SpellArcDegrees = 70f;
        /// <summary>Seconds the spell leaves its targets burning.</summary>
        public float SpellBurnSeconds = 5f;
        /// <summary>Stacks of the element's affliction the spell lays on what it strikes.</summary>
        public int SpellStacks;
        /// <summary>For a bolt: how many more enemies it leaps to, and how far each leap can be.</summary>
        public int SpellLeaps;
        public float SpellLeapRange = 5f;
        /// <summary>The id of the spell form the base spell wears, from <see cref="SpellForms"/>. Empty: a wave or a bolt, by <see cref="Shape"/>.</summary>
        public string SpellForm = "";
        /// <summary>For a base spell that wears a form: its radius, how long it or what it leaves lasts, seconds between bites, each bite, the wait before it lands, and how many.</summary>
        public float SpellRadius = 2.5f;
        public float SpellSeconds = 4f;
        public float SpellInterval = 0.5f;
        public float SpellLinger = 5f;
        public float SpellDelay;
        public int SpellCount = 1;
        /// <summary>Metres the base spell drags what stands within its reach toward the player (negative: throws them away).</summary>
        public float SpellPull;
        public float SurgeCost = 40f;
        public float SurgeSeconds = 10f;
        /// <summary>Weapon damage is multiplied by this during the surge.</summary>
        public float SurgeDamage = 1.3f;
        /// <summary>Seconds each weapon blow leaves its target burning during the surge.</summary>
        public float SurgeBurnSeconds = 3f;
        /// <summary>How much faster the weapon is swung during the surge.</summary>
        public float SurgeSpeed = 1f;
        /// <summary>Stagger each weapon blow adds on top of its own during the surge.</summary>
        public float SurgeStagger;
        /// <summary>Stacks of the element's affliction each weapon blow lays during the surge.</summary>
        public int SurgeStacks;
        /// <summary>What the surge does to the player's numbers while it lasts.</summary>
        [NonSerialized] public StatMod[] SurgeMods = Array.Empty<StatMod>();
        /// <summary>Wards the surge gives when it is lit: each turns aside one blow that would have landed.</summary>
        public int SurgeWards;
        /// <summary>Health the surge gives each second while it lasts; negative drains.</summary>
        public float SurgeHealPerSecond;
        /// <summary>Share of each weapon blow's damage that comes back as health during the surge.</summary>
        public float SurgeLeech;
        /// <summary>Lit, the surge lays this many stacks of the element's affliction on every enemy within the radius.</summary>
        public int SurgeBurstStacks;
        public float SurgeBurstRadius = 7f;
        /// <summary>Lit, the surge turns all grey health back into health.</summary>
        public bool SurgeMendsGrey;

        /// <summary>The element's affliction.</summary>
        public AfflictionKind Affliction => Elements.ByNameOrFire(Name).Affliction;

        /// <summary>The kit of the element of that name; Fire's for a name that is no element's.</summary>
        public static ElementKit ByName(string name, bool consumed)
        {
            switch (name)
            {
                case Elements.Lightning: return Lightning();
                case Elements.Wind: return Wind();
                case Elements.Ice: return Ice();
                case Elements.Water: return Water();
                case Elements.Life: return Life();
                case Elements.Earth: return Earth();
                case Elements.Metal: return Metal();
                case Elements.Light: return Light();
                case Elements.Dark: return Dark();
                case Elements.Poison: return Poison();
                default: return Fire(consumed);
            }
        }

        /// <summary>Fire: pressure and damage over time. Consuming a god makes it burn hotter and longer.</summary>
        public static ElementKit Fire(bool consumed = false) => new ElementKit
        {
            Name = FireName,
            SpellName = "Flame Wave",
            SurgeName = "Kindle",
            Shape = SpellShape.Wave,
            SpellDamage = consumed ? 28f : 20f,
            SpellBurnSeconds = consumed ? 7f : 5f,
            SpellReach = consumed ? 6.5f : 5.5f,
            SurgeDamage = consumed ? 1.45f : 1.3f,
            SurgeSeconds = consumed ? 13f : 10f,
        };

        /// <summary>
        /// Lightning: speed and broken guards. The bolt strikes one enemy hard on its balance and leaps to two more; the
        /// surge quickens every swing and adds stagger to it, where Fire's adds damage.
        /// </summary>
        public static ElementKit Lightning() => new ElementKit
        {
            Name = LightningName,
            SpellName = "Thunderclap",
            SurgeName = "Overdrive",
            Shape = SpellShape.Bolt,
            SpellCost = 25f,
            SpellWindup = 0.14f,
            SpellRecovery = 0.26f,
            SpellDamage = 16f,
            SpellStagger = 45f,
            SpellReach = 10f,
            SpellBurnSeconds = 0f,
            SpellLeaps = 2,
            SurgeDamage = 1.1f,
            SurgeBurnSeconds = 0f,
            SurgeSpeed = 1.3f,
            SurgeStagger = 8f,
            SurgeSeconds = 10f,
        };

        // The nine below carry the names and numbers of docs/design/trees_a.md and trees_b.md as far as the forms that
        // exist allow: a wave, a bolt or one of the spell forms, with a pull or a push where the design gives one.

        /// <summary>Wind: a long narrow gust that leaves enemies Unsteady. The surge is the better dodge.</summary>
        public static ElementKit Wind() => Plain(Elements.Wind, "Gust", "Tailwind", SpellShape.Wave, 20f, 8f, 20f, 5f, 70f, 2, new ElementKit
        {
            // A short cone that does little harm and throws what it reaches three metres off.
            SpellPull = -3f, SurgeCost = 35f,
            SurgeDamage = 1f, SurgeStacks = 1,
            SurgeMods = new[] { new StatMod(PlayerStat.DodgeDistance, 0.3f), new StatMod(PlayerStat.DodgeSafeTime, 0.25f), new StatMod(PlayerStat.DodgeStamina, -0.5f), new StatMod(PlayerStat.SurgeSpeed, 0.15f) },
        });

        /// <summary>Ice: a short wide fan of cold. The surge is a barrier that turns aside the next blow.</summary>
        public static ElementKit Ice() => Plain(Elements.Ice, "Hoarfrost", "Glaze", SpellShape.Wave, 25f, 10f, 15f, 4.5f, 110f, 2, new ElementKit
        {
            SurgeSeconds = 12f,
            SurgeDamage = 1f, SurgeStacks = 1, SurgeWards = 1,
        });

        /// <summary>Water: a broad wave that soaks. The surge makes flasks heal more.</summary>
        public static ElementKit Water() => Plain(Elements.Water, "Riptide", "Wellspring", SpellShape.Wave, 20f, 8f, 18f, 5f, 90f, 1, new ElementKit
        {
            // A low wave that comes back: what it touches is drawn two metres toward the player.
            SpellPull = 2f, SurgeCost = 30f, SurgeSeconds = 12f,
            SurgeDamage = 1f, SurgeStacks = 1,
            SurgeMods = new[] { new StatMod(PlayerStat.FlaskHeal, 0.5f) },
        });

        /// <summary>Life: a lash of thorn that seeds one enemy and the one beside it. The surge heals.</summary>
        public static ElementKit Life() => Plain(Elements.Life, "Briar Patch", "Regrowth", SpellShape.Wave, 20f, 4f, 0f, 3f, 0f, 1, new ElementKit
        {
            // A bed of thorns three metres ahead that stands six seconds and bites twice a second.
            SpellForm = SpellForms.Field, SpellRadius = 1.25f, SpellSeconds = 6f, SpellInterval = 0.5f, SpellLinger = 4f,
            SurgeSeconds = 6f, SurgeMendsGrey = true,
            SurgeDamage = 1f, SurgeStacks = 1, SurgeHealPerSecond = 4f,
        });

        /// <summary>Earth: a short heavy shove of stone that shakes the ground under what it hits. The surge is standing firm.</summary>
        public static ElementKit Earth() => Plain(Elements.Earth, "Stone Rise", "Bedrock", SpellShape.Wave, 25f, 14f, 30f, 5f, 0f, 1, new ElementKit
        {
            // A pillar of rock up under the enemy faced.
            SpellForm = SpellForms.Pillars, SpellCount = 1, SpellRadius = 1.3f, SpellDelay = 0.25f,
            SurgeDamage = 1f, SurgeStacks = 1, SurgeStagger = 6f,
            SurgeMods = new[] { new StatMod(PlayerStat.DamageTaken, -0.2f) },
        });

        /// <summary>Metal: a narrow spray of shards that breaks armour. The surge is defence.</summary>
        public static ElementKit Metal() => Plain(Elements.Metal, "Flung Blade", "Temper", SpellShape.Wave, 25f, 12f, 15f, 9f, 0f, 1, new ElementKit
        {
            // A blade left standing in the enemy faced for six seconds; the next blow of the weapon drives it in.
            SpellForm = SpellForms.Brand, SpellCount = 1, SpellRadius = 1.5f, SpellSeconds = 6f,
            SurgeDamage = 1f, SurgeStacks = 1,
            SurgeMods = new[] { new StatMod(PlayerStat.DamageTaken, -0.3f) },
        });

        /// <summary>Light: a lance on one enemy far off, which marks it. The surge mends what the guard lost and marks everyone near.</summary>
        public static ElementKit Light() => Plain(Elements.Light, "Glare", "Clarity", SpellShape.Wave, 25f, 8f, 22f, 4f, 90f, 1, new ElementKit
        {
            // Grey health made whole at once, and for eight seconds the perfect moments are half again as long.
            SurgeSeconds = 8f,
            SurgeDamage = 1f, SurgeStacks = 1, SurgeMendsGrey = true,
            SurgeMods = new[] { new StatMod(PlayerStat.PerfectGuardWindow, 0.5f), new StatMod(PlayerStat.DodgeSafeTime, 0.5f) },
        });

        /// <summary>Dark: a hex that leaps through a crowd and curses it. The surge is a hunger: health drains, and blows give it back.</summary>
        public static ElementKit Dark() => Plain(Elements.Dark, "Grasp", "Hunger", SpellShape.Bolt, 25f, 10f, 12f, 6f, 0f, 1, new ElementKit
        {
            // A hand of shadow to the enemy faced that drags it to the feet. Hunger asks no mana: its price is health.
            SpellLeaps = 0, SpellPull = 5f, SurgeCost = 0f,
            SurgeDamage = 1.3f, SurgeStacks = 1, SurgeHealPerSecond = -3f, SurgeLeech = 0.3f,
        });

        /// <summary>Poison: a spit of venom in a cone, two deep. The surge heals a little with every blow on a poisoned enemy.</summary>
        public static ElementKit Poison() => Plain(Elements.Poison, "Miasma", "Fever", SpellShape.Wave, 25f, 1f, 0f, 4f, 0f, 1, new ElementKit
        {
            // A cloud four metres ahead that hangs four seconds and doses what stands in it once a second.
            SpellForm = SpellForms.Field, SpellRadius = 1.25f, SpellSeconds = 4f, SpellInterval = 1f, SpellLinger = 1f,
            SurgeDamage = 1f, SurgeStacks = 1, SurgeLeech = 0.06f, SurgeHealPerSecond = 1f,
        });

        private static ElementKit Plain(string name, string spell, string surge, SpellShape shape, float cost, float damage, float stagger, float reach, float arc, int stacks, ElementKit kit)
        {
            kit.Name = name;
            kit.SpellName = spell;
            kit.SurgeName = surge;
            kit.Shape = shape;
            kit.SpellCost = cost;
            kit.SpellDamage = damage;
            kit.SpellStagger = stagger;
            kit.SpellReach = reach;
            kit.SpellArcDegrees = arc;
            kit.SpellStacks = stacks;
            kit.SpellBurnSeconds = 0f;
            kit.SurgeBurnSeconds = 0f;
            return kit;
        }

        public bool IsCasting { get; private set; }
        public float CastProgress => IsCasting ? Math.Min(1f, elapsed / (SpellWindup + SpellRecovery)) : 0f;
        public float SurgeLeft { get; private set; }
        public bool SurgeActive => SurgeLeft > 0f;
        public float SurgeFraction => surgeLength > 0f ? SurgeLeft / surgeLength : 0f;

        private float elapsed;
        private bool released;
        private float surgeLength;

        public void Reset()
        {
            IsCasting = false;
            elapsed = 0f;
            released = false;
            SurgeLeft = 0f;
        }

        /// <summary>Begins the cast if the mana is there. <paramref name="costScale"/> is what the player's upgrades make of the cost.</summary>
        public bool TryCast(ManaPool mana, float costScale = 1f)
        {
            if (IsCasting || !mana.TrySpend(SpellCost * costScale)) return false;
            IsCasting = true;
            elapsed = 0f;
            released = false;
            return true;
        }

        /// <summary>Starts the surge, or tops it back up to full if it is already running.</summary>
        public bool TrySurge(ManaPool mana, float costScale = 1f, float secondsScale = 1f)
        {
            if (!mana.TrySpend(SurgeCost * costScale)) return false;
            surgeLength = SurgeSeconds * secondsScale;
            SurgeLeft = surgeLength;
            return true;
        }

        /// <summary>A blow or a dodge breaks the cast. The mana is gone either way: casting next to an enemy is a risk.</summary>
        public void Interrupt() => IsCasting = false;

        /// <summary>Advances the cast. Returns true on the tick the spell leaves the hand.</summary>
        public bool TickCast(float deltaTime)
        {
            if (!IsCasting) return false;
            elapsed += deltaTime;
            bool releaseNow = false;
            if (!released && elapsed >= SpellWindup)
            {
                released = true;
                releaseNow = true;
            }
            if (elapsed >= SpellWindup + SpellRecovery) IsCasting = false;
            return releaseNow;
        }

        /// <summary>Advances the surge. Returns true on the tick it runs out.</summary>
        public bool TickSurge(float deltaTime)
        {
            if (SurgeLeft <= 0f) return false;
            SurgeLeft = Math.Max(0f, SurgeLeft - deltaTime);
            return SurgeLeft <= 0f;
        }
    }
}

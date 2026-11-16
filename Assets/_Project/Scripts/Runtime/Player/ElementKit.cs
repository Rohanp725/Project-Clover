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

        // The nine below are stand-ins: a wave or a bolt with its own reach, arc and cost, laying the element's
        // affliction, and a surge built from the user's first table of self buffs as far as the game's numbers allow.

        /// <summary>Wind: a long narrow gust that leaves enemies Unsteady. The surge is the better dodge.</summary>
        public static ElementKit Wind() => Plain(Elements.Wind, "Gale", "Tailwind", SpellShape.Wave, 20f, 12f, 30f, 7.5f, 45f, 2, new ElementKit
        {
            SurgeDamage = 1f, SurgeStacks = 1,
            SurgeMods = new[] { new StatMod(PlayerStat.DodgeDistance, 0.3f), new StatMod(PlayerStat.DodgeSafeTime, 0.4f), new StatMod(PlayerStat.StaminaCost, -0.4f), new StatMod(PlayerStat.SurgeSpeed, 0.15f) },
        });

        /// <summary>Ice: a short wide fan of cold. The surge is a barrier that turns aside the next blow.</summary>
        public static ElementKit Ice() => Plain(Elements.Ice, "Frost Fan", "Rime Ward", SpellShape.Wave, 25f, 14f, 14f, 4.5f, 110f, 2, new ElementKit
        {
            SurgeDamage = 1f, SurgeStacks = 1, SurgeWards = 1,
        });

        /// <summary>Water: a broad wave that soaks. The surge makes flasks heal more.</summary>
        public static ElementKit Water() => Plain(Elements.Water, "Tide", "Wellspring", SpellShape.Wave, 25f, 14f, 18f, 6f, 90f, 1, new ElementKit
        {
            SurgeDamage = 1f, SurgeStacks = 1,
            SurgeMods = new[] { new StatMod(PlayerStat.FlaskHeal, 0.5f) },
        });

        /// <summary>Life: a lash of thorn that seeds one enemy and the one beside it. The surge heals.</summary>
        public static ElementKit Life() => Plain(Elements.Life, "Thorn Lash", "Renewal", SpellShape.Bolt, 25f, 14f, 20f, 8f, 0f, 1, new ElementKit
        {
            SpellLeaps = 1, SurgeDamage = 1f, SurgeStacks = 1, SurgeHealPerSecond = 3f,
        });

        /// <summary>Earth: a short heavy shove of stone that shakes the ground under what it hits. The surge is standing firm.</summary>
        public static ElementKit Earth() => Plain(Elements.Earth, "Stone Shove", "Bedrock", SpellShape.Wave, 30f, 22f, 40f, 4f, 80f, 1, new ElementKit
        {
            SurgeDamage = 1f, SurgeStacks = 1, SurgeStagger = 6f,
            SurgeMods = new[] { new StatMod(PlayerStat.DamageTaken, -0.2f) },
        });

        /// <summary>Metal: a narrow spray of shards that breaks armour. The surge is defence.</summary>
        public static ElementKit Metal() => Plain(Elements.Metal, "Shrapnel", "Tempered", SpellShape.Wave, 25f, 24f, 20f, 5f, 40f, 1, new ElementKit
        {
            SurgeDamage = 1f, SurgeStacks = 1,
            SurgeMods = new[] { new StatMod(PlayerStat.DamageTaken, -0.35f) },
        });

        /// <summary>Light: a lance on one enemy far off, which marks it. The surge mends what the guard lost and marks everyone near.</summary>
        public static ElementKit Light() => Plain(Elements.Light, "Lance of Day", "Purify", SpellShape.Bolt, 25f, 20f, 22f, 12f, 0f, 1, new ElementKit
        {
            SpellLeaps = 0, SurgeDamage = 1f, SurgeStacks = 1, SurgeBurstStacks = 1, SurgeMendsGrey = true,
            SurgeMods = new[] { new StatMod(PlayerStat.CritDamage, 0.3f) },
        });

        /// <summary>Dark: a hex that leaps through a crowd and curses it. The surge is a hunger: health drains, and blows give it back.</summary>
        public static ElementKit Dark() => Plain(Elements.Dark, "Hex", "Hunger", SpellShape.Bolt, 25f, 10f, 12f, 9f, 0f, 1, new ElementKit
        {
            SpellLeaps = 3, SpellLeapRange = 4f, SurgeDamage = 1.15f, SurgeStacks = 1, SurgeHealPerSecond = -2f, SurgeLeech = 0.12f,
        });

        /// <summary>Poison: a spit of venom in a cone, two deep. The surge heals a little with every blow on a poisoned enemy.</summary>
        public static ElementKit Poison() => Plain(Elements.Poison, "Spit", "Antivenom", SpellShape.Wave, 20f, 8f, 10f, 5.5f, 60f, 2, new ElementKit
        {
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

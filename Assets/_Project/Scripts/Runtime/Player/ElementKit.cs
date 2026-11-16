using System;

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
    /// it says the spell is released.
    /// </summary>
    [Serializable]
    public class ElementKit
    {
        public const string FireName = "Fire";
        public const string LightningName = "Lightning";

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

        public static ElementKit ByName(string name, bool consumed) => name == LightningName ? Lightning() : Fire(consumed);

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

        public bool IsCasting { get; private set; }
        public float CastProgress => IsCasting ? Math.Min(1f, elapsed / (SpellWindup + SpellRecovery)) : 0f;
        public float SurgeLeft { get; private set; }
        public bool SurgeActive => SurgeLeft > 0f;
        public float SurgeFraction => SurgeSeconds > 0f ? SurgeLeft / SurgeSeconds : 0f;

        private float elapsed;
        private bool released;

        public void Reset()
        {
            IsCasting = false;
            elapsed = 0f;
            released = false;
            SurgeLeft = 0f;
        }

        public bool TryCast(ManaPool mana)
        {
            if (IsCasting || !mana.TrySpend(SpellCost)) return false;
            IsCasting = true;
            elapsed = 0f;
            released = false;
            return true;
        }

        /// <summary>Starts the surge, or tops it back up to full if it is already running.</summary>
        public bool TrySurge(ManaPool mana)
        {
            if (!mana.TrySpend(SurgeCost)) return false;
            SurgeLeft = SurgeSeconds;
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

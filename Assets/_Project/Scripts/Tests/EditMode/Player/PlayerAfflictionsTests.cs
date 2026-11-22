using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Trees;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// Afflictions laid on the player (standin_clearance_plan.md question 1, decided 2026-11-21: elites and gods lay their
    /// land's affliction when they hit), and the three marks that answer them: Tremor's half time, Bedrock's shedding and
    /// High Noon's clearing.
    /// </summary>
    public class PlayerAfflictionsTests
    {
        [Test]
        public void OnlyElitesAndGodsLayAndTheLandSaysWhat()
        {
            Assert.IsFalse(PlayerAfflictions.Lays(EnemyRank.Plain));
            Assert.IsTrue(PlayerAfflictions.Lays(EnemyRank.Elite));
            Assert.IsTrue(PlayerAfflictions.Lays(EnemyRank.God));
            Assert.AreEqual(AfflictionKind.Burn, PlayerAfflictions.OfLand(Outfits.LandOf("SunCity")), "Sun City lays Burn");
            Assert.AreEqual(Elements.ByName(Elements.Lightning).Affliction, PlayerAfflictions.OfLand(Outfits.LandOf("Castle")));
            Assert.AreEqual(AfflictionKind.None, PlayerAfflictions.OfLand(Outfits.LandOf("Hub")), "outside every land, nothing");
        }

        [Test]
        public void ALaidAfflictionBitesAndRunsOut()
        {
            var on = new PlayerAfflictions();
            Assert.IsTrue(on.Lay(AfflictionKind.Burn, 1));
            Assert.IsTrue(on.Has(AfflictionKind.Burn));
            float bitten = 0f;
            for (int i = 0; i < 20; i++) bitten += on.Tick(0.1f);
            Assert.Greater(bitten, 0f, "the burn bites the player");
            for (int i = 0; i < 200; i++) on.Tick(0.1f);
            Assert.IsFalse(on.Any, "and runs out");
            Assert.IsTrue(on.Lay(AfflictionKind.Static, 9));
            Assert.AreEqual(Afflictions.Spec(AfflictionKind.Static).MaximumStacks, on.Stacks(AfflictionKind.Static), "no deeper than its spec");
            Assert.IsFalse(on.Lay(AfflictionKind.None, 1));
        }

        [Test]
        public void EachAfflictionDoesToThePlayerWhatItDoesToAnEnemy()
        {
            var on = new PlayerAfflictions();
            Assert.AreEqual(1f, on.MoveScale, 1e-5f);
            Assert.AreEqual(1f, on.TakenScale, 1e-5f);
            Assert.AreEqual(1f, on.DealtScale, 1e-5f);
            Assert.AreEqual(1f, on.KnockbackScale, 1e-5f);
            Assert.AreEqual(0f, on.DodgeRecoveryAdd, 1e-5f);
            int version = on.Version;
            on.Lay(AfflictionKind.Chill, 2);
            Assert.AreNotEqual(version, on.Version, "what is on the player changed");
            Assert.AreEqual(1f - 2f * Afflictions.Spec(AfflictionKind.Chill).SlowPerStack, on.MoveScale, 1e-4f, "Chill slows the step");
            on.Lay(AfflictionKind.Soaked, 2);
            Assert.Greater(on.DodgeRecoveryAdd, 0f, "Soaked: the dodge comes back slower");
            Assert.AreEqual(on.Stacks(AfflictionKind.Soaked) * PlayerAfflictions.SoakedDodgeSeconds, on.DodgeRecoveryAdd, 1e-5f);
            on.Lay(AfflictionKind.Sundered, 1);
            on.Lay(AfflictionKind.Marked, 1);
            Assert.AreEqual(1f + Afflictions.Spec(AfflictionKind.Sundered).WeaponDamageTakenPerStack + PlayerAfflictions.MarkedTakenPerStack, on.TakenScale, 1e-4f, "blows cut deeper");
            on.Lay(AfflictionKind.Cursed, 1);
            Assert.AreEqual(1f - Afflictions.Spec(AfflictionKind.Cursed).DealsLessPerStack, on.DealtScale, 1e-4f, "Cursed: your blows are weaker");
            on.Lay(AfflictionKind.Unsteady, 1);
            Assert.AreEqual(1f + Afflictions.Spec(AfflictionKind.Unsteady).ShovePerStack, on.KnockbackScale, 1e-4f, "Unsteady: thrown farther");
            on.Lay(AfflictionKind.Burn, 1);
            var kinds = new AfflictionKind[Clover.UI.AfflictionBadges.Most];
            var seconds = new float[Clover.UI.AfflictionBadges.Most];
            int shown = on.Shown(kinds, seconds);
            Assert.AreEqual(Clover.UI.AfflictionBadges.Most, shown, "six on, six shown");
            Assert.IsTrue(kinds.Take(shown).Contains(AfflictionKind.Burn));
            Assert.IsTrue(seconds.Take(shown).All(left => left > 0f), "each with its time");
            version = on.Version;
            on.Clear();
            Assert.AreNotEqual(version, on.Version);
            Assert.AreEqual(1f, on.MoveScale, 1e-5f, "cleared, as before");
        }

        [Test]
        public void TheMarkByTheHealthBarSaysTheNameAndTheSecondsLeft()
        {
            Assert.AreEqual("Burn 3", Clover.UI.AfflictionBadges.Words(AfflictionKind.Burn, 2.2f));
            Assert.AreEqual("Burn 0", Clover.UI.AfflictionBadges.Words(AfflictionKind.Burn, -1f));
        }

        [Test]
        public void TremorHalvesTheTimeBedrockShedsAndHighNoonClears()
        {
            var plain = new PlayerAfflictions();
            var shaken = new PlayerAfflictions { SecondsScale = 0.5f };
            plain.Lay(AfflictionKind.Chill, 1);
            shaken.Lay(AfflictionKind.Chill, 1);
            Assert.AreEqual(plain.SecondsLeft(AfflictionKind.Chill) * 0.5f, shaken.SecondsLeft(AfflictionKind.Chill), 1e-4f, "half as long");
            var root = Upgrade.ById(EarthTree.Root);
            Assert.IsTrue(root.Stats.Any(stat => stat.Stat == PlayerStat.AfflictionsOnYou && stat.Amount == -0.5f), "Tremor, Earth's root");

            plain.Shedding = true;
            Assert.IsFalse(plain.Any, "Bedrock sheds what was on");
            Assert.IsFalse(plain.Lay(AfflictionKind.Burn, 1), "and nothing stays while it lasts");
            plain.Shedding = false;
            Assert.IsTrue(plain.Lay(AfflictionKind.Burn, 1));
            plain.Clear();
            Assert.IsFalse(plain.Any);

            var noon = Upgrade.All.Where(mark => mark.Shapes != null).SelectMany(mark => mark.Shapes).First(shape => shape != null && shape.Name == "High Noon");
            Assert.IsTrue(noon.Effects.Any(effect => effect.Do == UpgradeAction.Cleanse && effect.On == UpgradeTrigger.SpellCast), "High Noon clears what is on you");
        }
    }
}

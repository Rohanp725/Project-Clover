using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Trees;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// Who a reaction reaches (standin_clearance_plan.md pass 3): the enemies a reaction struck, each answered once each
    /// time it goes off, and the reactions picked by an affliction in their pair (the ledger's reaction-struck).
    /// </summary>
    public class ReactionReachTests
    {
        [Test]
        public void EachEnemyAReactionStrikesIsToldOnceEachTimeItGoesOff()
        {
            var once = new PlayerUpgrades.StruckOnce();
            object first = new object(), second = new object();
            Assert.IsTrue(once.First(1, first));
            Assert.IsFalse(once.First(1, first), "its second blow on the same enemy is not told again");
            Assert.IsTrue(once.First(1, second), "another enemy it reached is");
            Assert.IsTrue(once.First(2, first), "the next time it goes off, the same enemy is told again");
        }

        [Test]
        public void AReactionIsPickedByAnAfflictionInItsPair()
        {
            int steam = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Soaked);
            Assert.IsTrue(PlayerUpgrades.ReactionHas(steam, AfflictionKind.Soaked));
            Assert.IsTrue(PlayerUpgrades.ReactionHas(steam, AfflictionKind.Burn));
            Assert.IsFalse(PlayerUpgrades.ReactionHas(steam, AfflictionKind.Chill));
            Assert.IsTrue(PlayerUpgrades.ReactionHas(steam, AfflictionKind.None), "nothing asked: any");
            Assert.IsFalse(PlayerUpgrades.ReactionHas(-1, AfflictionKind.Chill));
        }

        private sealed class Afflicted : IAfflictable
        {
            public AfflictionSet Afflictions { get; } = new AfflictionSet { Reactions = new ReactionEngine((who, pair, laid) => { }, () => 0) };
        }

        [Test]
        public void AnAimSeeksEitherAfflictionAndTheNearestOrFarthest()
        {
            var wet = new Afflicted();
            wet.Afflictions.Lay(AfflictionKind.Soaked, 1, 0f, 1f, LaySource.Spell);
            var marked = new Afflicted();
            marked.Afflictions.Lay(AfflictionKind.Marked, 1, 0f, 1f, LaySource.Spell);
            var clean = new Afflicted();
            Assert.IsTrue(PlayerCombat.CarriesEither(wet, AfflictionKind.Soaked, AfflictionKind.Marked));
            Assert.IsTrue(PlayerCombat.CarriesEither(marked, AfflictionKind.Soaked, AfflictionKind.Marked), "either will do");
            Assert.IsFalse(PlayerCombat.CarriesEither(clean, AfflictionKind.Soaked, AfflictionKind.Marked));
            Assert.IsFalse(PlayerCombat.CarriesEither(wet, AfflictionKind.None, AfflictionKind.None), "asking nothing finds nothing to aim at");
            Assert.IsTrue(PlayerCombat.Better(4f, 9f, false), "nearer is better");
            Assert.IsTrue(PlayerCombat.Better(9f, 4f, true), "farther, when the farthest is wanted");
        }

        [Test]
        public void TheAimedMarksSeekWhatTheySay()
        {
            var flat = Upgrade.ById(Clover.Player.Trees.Duo.TheBoltRunsTrueDuo.Shape).Shapes.First(shape => shape.Name == "Laid Flat").Effects.Single(effect => effect.Do == UpgradeAction.Lane);
            Assert.AreEqual(AfflictionKind.Marked, flat.AimAt);
            Assert.AreEqual(AfflictionKind.Static, flat.AimAt2);
            Assert.AreEqual(10f, flat.Reach, 1e-5f);
            Assert.AreEqual(AfflictionKind.Soaked, Upgrade.ById(BloomTree.StranglingVine).Spell.AimAt, "Strangling Vine");
            var seeks = Upgrade.ById(Clover.Player.Trees.Duo.LiveIronDuo.LightningSeeksIron).Spell;
            Assert.AreEqual(AfflictionKind.Sundered, seeks.AimAt);
            Assert.IsTrue(seeks.AimFarthest);
            Assert.AreEqual(0.5f, seeks.AimMissShare, 1e-5f, "the one faced at half with none");
            var road = Upgrade.ById(Clover.Player.Trees.Duo.CaughtLightDuo.TheGodsRoad).Spell;
            Assert.AreEqual(AfflictionKind.Soaked, road.AimAt);
            Assert.AreEqual(AfflictionKind.Marked, road.AimAt2);
            Assert.AreEqual(AfflictionKind.Soaked, Upgrade.ById(Clover.Player.Trees.Duo.SeizedDuo.TheFlashing).Spell.AimAt);
        }

        [Test]
        public void TheEffectsDoneOnEachEnemyNearAskWhatEachCarries()
        {
            var snow = Upgrade.ById(Clover.Player.Trees.Duo.SleetChargeDuo.Shape).Shapes.First(shape => shape.Name == "Thundersnow").Effects;
            Assert.IsFalse(snow.Any(effect => effect.Do == UpgradeAction.Arc), "no bolt leaping chilled to chilled");
            Assert.IsTrue(snow.Any(effect => effect.Do == UpgradeAction.Burst && effect.EachWithin == 6f && effect.OnlyCarrying == AfflictionKind.Chill), "a bolt on each chilled");
            Assert.IsTrue(snow.Any(effect => effect.Do == UpgradeAction.SetOff && effect.EachWithin == 6f && effect.OnlyCarrying == AfflictionKind.Chill), "each discharging its own");
            var crack = Upgrade.ById(WaterTree.DeepCrack).Effects;
            Assert.IsFalse(crack.Any(effect => effect.Do == UpgradeAction.Arc));
            Assert.IsTrue(crack.Any(effect => effect.Do == UpgradeAction.Burst && effect.EachWithin == 5f && effect.EachOther && effect.OnlyCarrying == AfflictionKind.Soaked), "each other Soaked cracks where it stands");
            Assert.IsTrue(crack.Any(effect => effect.Do == UpgradeAction.Spend && effect.EachWithin == 5f && effect.EachOther && effect.Affliction == AfflictionKind.Soaked), "and its Soaked is spent");
            var sea = Upgrade.ById(Clover.Player.Trees.Duo.SeizedDuo.TheSeaFindsLand).Effects.Single();
            Assert.AreEqual(AfflictionKind.Soaked, sea.OnlyCarrying, "the discharge runs to the Soaked alone");
            var swamp = Upgrade.ById(Clover.Player.Trees.Duo.CapsizeDuo.Swamped).Effects.Single().State;
            Assert.AreEqual(ConditionFlags.StaggerTakenShare, swamp.Flags);
            Assert.AreEqual(2f, swamp.Scale, 1e-5f, "twice as fast");
            Assert.AreEqual(Reactions.Capsized, swamp.WhileIn, "only while it is capsized");
        }

        [Test]
        public void ALayingIsToldAndGraftedEdgeAnswersANotchOnTheSeeded()
        {
            int told = 0;
            AfflictionKind heard = AfflictionKind.None;
            System.Action<UnityEngine.Component, AfflictionKind, int, LaySource> listen = (who, kind, stacks, source) => { told++; heard = kind; };
            CombatEvents.Laid += listen;
            try
            {
                CombatEvents.Report(null, AfflictionKind.Sundered, 1, new AfflictionResult(true, false, false), LaySource.Weapon);
                CombatEvents.Report(null, AfflictionKind.Sundered, 1, new AfflictionResult(false, false, false), LaySource.Weapon);
            }
            finally { CombatEvents.Laid -= listen; }
            Assert.AreEqual(1, told, "only what took is told");
            Assert.AreEqual(AfflictionKind.Sundered, heard);
            var graft = Upgrade.ById(Clover.Player.Trees.Duo.IronThornsDuo.GraftedEdge).Effects.Single();
            Assert.AreEqual(UpgradeTrigger.AfflictionLaid, graft.On);
            Assert.AreEqual(AfflictionKind.Sundered, graft.Spent, "a notch laid");
            Assert.AreEqual(AfflictionKind.Seeded, graft.Needs, "on a Seeded enemy");
            Assert.AreEqual(1, graft.Stacks, "lays one more");
        }

        [Test]
        public void TheMarksAnswerWhatTheReactionStruck()
        {
            UpgradeEffect Struck(string id) => Upgrade.ById(id).Effects.Single(effect => effect.On == UpgradeTrigger.ReactionStruck);
            var settles = Struck(IceTree.ColdSettles);
            Assert.AreEqual(AfflictionKind.Chill, settles.ReactionWith);
            Assert.AreEqual(UpgradeAction.Condition, settles.Do);
            var roots = Struck(LifeTree.RootAndBranch);
            Assert.AreEqual(AfflictionKind.Seeded, roots.ReactionWith);
            Assert.AreEqual(0f, roots.Radius, 1e-5f, "on each it struck, not round where it happened");
            Assert.AreEqual(AfflictionKind.Cursed, Struck(DarkTree.CollectedTwice).ReactionWith);
            var seen = Struck(LightTree.SeenPlainly);
            Assert.AreEqual(AfflictionKind.Marked, seen.ReactionWith);
            Assert.AreEqual(0f, seen.Radius, 1e-5f);
            var rings = Struck(MetalTree.EveryCrackRings);
            Assert.AreEqual(AfflictionKind.Sundered, rings.ReactionWith);
            Assert.AreEqual(0f, rings.Radius, 1e-5f);
            Assert.AreEqual("a reaction strikes an enemy", TriggerWords.Of(UpgradeTrigger.ReactionStruck));
        }
    }
}

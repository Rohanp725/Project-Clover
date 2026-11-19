using System.Collections.Generic;
using System.Linq;
using Clover.Gameplay;
using Clover.Player;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The reactions as marks (IMPLEMENTATION.md 2.4 and 6.1), for what is built so far: each built pair has exactly one
    /// hidden reaction mark named and keyed as the pair is; reaction marks are never drawn, opened or sold; every effect
    /// rides a trigger a reaction can give; every condition and forced reaction named exists; and the table finds each
    /// mark, its effects and its rest.
    /// </summary>
    public class ReactionCatalogueTests
    {
        /// <summary>Phase 3, task 3.2: the design's first six (3.4 adds the other 49).</summary>
        private static readonly string[] FirstSix = { "Flashover", "Steam", "Carried Embers", "The Ground Gives", "Live Iron", "Rot Roots" };

        private static IEnumerable<Upgrade> ReactionMarks => Upgrade.All.Where(mark => mark.Kind == UpgradeKind.Reaction);

        [Test]
        public void EachBuiltPairHasExactlyOneHiddenMarkKeyedAndNamedAsThePairIs()
        {
            var seen = new HashSet<int>();
            foreach (var mark in ReactionMarks)
            {
                int pair = ReactionTable.PairOf(mark.Id);
                Assert.GreaterOrEqual(pair, 0, mark.Id + " is not the id of any pair's reaction");
                Assert.IsTrue(seen.Add(pair), "two reaction marks for " + AfflictionPairs.Name(pair));
                Assert.AreEqual(AfflictionPairs.Id(pair), mark.Id);
                Assert.AreEqual(AfflictionPairs.Name(pair), mark.Name);
                Assert.AreEqual(MarkTree.Hidden, mark.Tree, mark.Id + " is a reaction and must be hidden");
                Assert.IsTrue(mark.IsHidden, mark.Id);
                Assert.IsNotNull(mark.Reaction, mark.Id + " carries no reaction numbers");
                var kinds = new[] { Elements.ByName(mark.Element).Affliction, Elements.ByName(mark.Element2).Affliction };
                CollectionAssert.AreEquivalent(new[] { AfflictionPairs.First(pair), AfflictionPairs.Second(pair) }, kinds, mark.Id + " names the pair's two elements");
                Assert.IsNotEmpty(mark.Effects, mark.Id + " does nothing");
                Assert.AreSame(mark, ReactionTable.MarkFor(pair));
            }
            foreach (string name in FirstSix)
                Assert.IsTrue(ReactionMarks.Any(mark => mark.Name == name), name + " is built");
            Assert.AreEqual(seen.Count, ReactionTable.Built);
        }

        [Test]
        public void ReactionMarksAreNeverDrawnOpenedSoldOrOnATree()
        {
            var nothing = new HashSet<string>();
            foreach (var mark in ReactionMarks)
            {
                Assert.IsFalse(mark.BelongsTo(mark.Element), mark.Id + " belongs to no tree");
                Assert.IsFalse(MarkRules.Open(mark, mark.Element, nothing, nothing, true), mark.Id + " is never open, even in the playtest");
                Assert.IsFalse(MarkRules.CanBuy(mark, nothing, 999, true, true), mark.Id + " is never sold, even in the playtest");
                Assert.AreEqual(0, mark.Cost, mark.Id);
                Assert.IsFalse(mark.IsSpell, mark.Id);
            }
        }

        [Test]
        public void EveryEffectRidesATriggerAReactionGivesAndNamesWhatExists()
        {
            var reactionTriggers = new[] { UpgradeTrigger.Reacted, UpgradeTrigger.ConditionBeat, UpgradeTrigger.ConditionStruck, UpgradeTrigger.ConditionEnded };
            foreach (var mark in ReactionMarks)
            {
                var set = new HashSet<string>(mark.Effects.Where(effect => effect.Do == UpgradeAction.Condition).Select(effect => effect.State != null ? effect.State.Id : ""));
                foreach (var effect in mark.Effects)
                {
                    CollectionAssert.Contains(reactionTriggers, effect.On, mark.Id + " has an effect on " + effect.On);
                    if (effect.Do == UpgradeAction.Condition)
                    {
                        Assert.IsNotNull(effect.State, mark.Id + " sets a condition with no numbers");
                        Assert.IsNotEmpty(effect.State.Id, mark.Id + " sets a condition with no name");
                        Assert.Greater(effect.State.Seconds, 0f, mark.Id);
                    }
                    if (effect.On != UpgradeTrigger.Reacted)
                        Assert.IsTrue(set.Contains(effect.Condition), $"{mark.Id} answers the condition '{effect.Condition}', which it never sets");
                }
            }
            // Anywhere in the catalogue: a reaction named by a condition or a force is a real one.
            foreach (var mark in Upgrade.All)
                foreach (var effect in mark.Effects)
                    if (!string.IsNullOrEmpty(effect.Reaction))
                        Assert.GreaterOrEqual(ReactionTable.PairOf(effect.Reaction), 0, $"{mark.Id} names the reaction '{effect.Reaction}', which is no pair's");
        }

        [Test]
        public void TheTableFindsEachMarkItsEffectsAndItsRest()
        {
            int steam = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Soaked);
            int flashover = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Static);
            int jar = AfflictionPairs.Index(AfflictionKind.Cursed, AfflictionKind.Poison);
            int thermal = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Chill);
            int thunderGust = AfflictionPairs.Index(AfflictionKind.Static, AfflictionKind.Unsteady);
            Assert.AreEqual("Steam", ReactionTable.MarkFor(steam).Name);
            Assert.IsNotEmpty(ReactionTable.EffectsFor(steam, new Clover.Player.Build.Loadout()));
            Assert.IsNull(ReactionTable.MarkFor(thunderGust), "not built until task 3.4");
            Assert.IsEmpty(ReactionTable.EffectsFor(thunderGust, null));
            Assert.AreEqual(3f, ReactionTable.RestFor(steam), "Steam's entry: once every 3 s");
            Assert.AreEqual(4f, ReactionTable.RestFor(flashover), "the shared default");
            Assert.AreEqual(8f, ReactionTable.RestFor(jar), "Shut in the Dark's entry: rest 8 s");
            Assert.AreEqual(6f, ReactionTable.RestFor(thermal), "every distance-3 pair rests 6 s");

            var engine = new ReactionEngine((who, pair, laid) => { }, () => 0);
            ReactionTable.ApplyRests(engine);
            for (int pair = 0; pair < AfflictionPairs.Count; pair++)
                Assert.AreEqual(ReactionTable.RestFor(pair), engine.RestFor(pair), AfflictionPairs.Name(pair));
            Assert.AreEqual(-1, ReactionTable.PairOf("react.nothing"));
        }
    }
}

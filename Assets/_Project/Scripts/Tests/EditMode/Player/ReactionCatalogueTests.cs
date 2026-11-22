using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Clover.Gameplay;
using Clover.Player;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The reactions as marks (IMPLEMENTATION.md 2.4 and 6.1), all 55 since task 3.4: each pair has exactly one hidden
    /// reaction mark named and keyed as the pair is and named in the pair files; reaction marks are never drawn, opened
    /// or sold; every effect rides a trigger a reaction can give; every condition and forced reaction named exists; each
    /// pair rests as its entry says and only Seized and The Grudge wait for two Static; the pieces the reactions share
    /// are used as they are meant; and the table finds each mark, its effects and its rest.
    /// </summary>
    public class ReactionCatalogueTests
    {
        private static readonly string[] PairFiles = { "pairs_d1_d2.md", "pairs_d3.md", "pairs_d4.md", "pairs_d5.md" };
        private const string ReactionLead = "**Reaction: ";
        // How an entry gives its own rest: "; rest 8 s", "Once every 5 s on one enemy", "Once per enemy every 8 s".
        private static readonly Regex OwnRest = new Regex(@"\brest (\d+) s\b|[Oo]nce every (\d+) s on one enemy|[Oo]nce per enemy every (\d+) s");
        private const string WaitsForDepth = "two Static or more";

        private static IEnumerable<Upgrade> ReactionMarks => Upgrade.All.Where(mark => mark.Kind == UpgradeKind.Reaction);

        [Test]
        public void AllFiftyFivePairsHaveExactlyOneHiddenMarkKeyedAndNamedAsThePairFilesNameIt()
        {
            var written = ReadEntries();
            var seen = new HashSet<int>();
            foreach (var mark in ReactionMarks)
            {
                int pair = ReactionTable.PairOf(mark.Id);
                Assert.GreaterOrEqual(pair, 0, mark.Id + " is not the id of any pair's reaction");
                Assert.IsTrue(seen.Add(pair), "two reaction marks for " + AfflictionPairs.Name(pair));
                Assert.AreEqual(AfflictionPairs.Id(pair), mark.Id);
                Assert.AreEqual(AfflictionPairs.Name(pair), mark.Name);
                Assert.IsTrue(written.ContainsKey(mark.Name), mark.Name + " is not a reaction the pair files name");
                Assert.AreEqual(MarkTree.Hidden, mark.Tree, mark.Id + " is a reaction and must be hidden");
                Assert.IsTrue(mark.IsHidden, mark.Id);
                Assert.IsNotNull(mark.Reaction, mark.Id + " carries no reaction numbers");
                var kinds = new[] { Elements.ByName(mark.Element).Affliction, Elements.ByName(mark.Element2).Affliction };
                CollectionAssert.AreEquivalent(new[] { AfflictionPairs.First(pair), AfflictionPairs.Second(pair) }, kinds, mark.Id + " names the pair's two elements");
                Assert.IsNotEmpty(mark.Effects, mark.Id + " does nothing");
                Assert.IsTrue(mark.Effects.Any(effect => effect.On == UpgradeTrigger.Reacted), mark.Id + " does nothing as it reacts");
                Assert.IsNotEmpty(mark.Description, mark.Id + " has no words");
                Assert.AreSame(mark, ReactionTable.MarkFor(pair));
            }
            Assert.AreEqual(AfflictionPairs.Count, written.Count, "the pair files name 55 reactions");
            Assert.AreEqual(AfflictionPairs.Count, seen.Count, "every pair is built");
            Assert.AreEqual(AfflictionPairs.Count, ReactionTable.Built);
            for (int pair = 0; pair < AfflictionPairs.Count; pair++)
                Assert.IsNotNull(ReactionTable.MarkFor(pair), AfflictionPairs.Name(pair) + " is not built");
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
            var reactionTriggers = new[] { UpgradeTrigger.Reacted, UpgradeTrigger.ConditionBeat, UpgradeTrigger.ConditionStruck, UpgradeTrigger.ConditionEnded, UpgradeTrigger.ConditionDied, UpgradeTrigger.ConditionAttacked };
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
        public void EachPairRestsAsItsEntrySaysAndOnlySeizedAndTheGrudgeWaitForTwoStatic()
        {
            var written = ReadEntries();
            var engine = new ReactionEngine((who, pair, laid) => { }, () => 0);
            ReactionTable.ApplyRests(engine);
            for (int pair = 0; pair < AfflictionPairs.Count; pair++)
            {
                string name = AfflictionPairs.Name(pair);
                string entry = written[name];
                var own = OwnRest.Match(entry);
                float expected = own.Success ? float.Parse(own.Groups.Cast<Group>().Skip(1).First(group => group.Success).Value)
                    : AfflictionPairs.Distance(pair) == 3 ? AfflictionPairs.DistanceThreeRest : AfflictionPairs.UsualRest;
                Assert.AreEqual(expected, ReactionTable.RestFor(pair), 0.0001f, name + ": the rest its entry gives");
                Assert.AreEqual(expected, engine.RestFor(pair), 0.0001f, name + ": the engine is given it");
                var numbers = ReactionTable.MarkFor(pair).Reaction;
                Assert.IsTrue(numbers.Rest == 0f || Math.Abs(numbers.Rest - expected) < 0.0001f, name + ": a mark's own rest is its entry's");

                bool waits = entry.Contains(WaitsForDepth);
                Assert.AreEqual(waits ? AfflictionKind.Static : AfflictionKind.None, numbers.DepthOf, name);
                Assert.AreEqual(waits ? 2 : 0, numbers.Depth, name);
                Assert.AreEqual(waits ? AfflictionKind.Static : AfflictionKind.None, engine.DepthKind(pair), name + ": the engine waits as the mark says");
                Assert.AreEqual(waits ? 2 : 0, engine.DepthFor(pair), name);
            }
            Assert.IsTrue(written["Seized"].Contains(WaitsForDepth) && written["The Grudge"].Contains(WaitsForDepth), "the two entries that wait are found");
        }

        [Test]
        public void ThePiecesTheReactionsShareAreUsedAsTheyAreMeant()
        {
            foreach (var mark in ReactionMarks)
            {
                var effects = mark.Effects;
                for (int i = 0; i < effects.Length; i++)
                {
                    var effect = effects[i];
                    string where = $"{mark.Id}, effect {i} ({effect.On} {effect.Do})";
                    if (effect.AtCount > 0)
                        Assert.IsTrue(effect.On == UpgradeTrigger.ConditionStruck || effect.On == UpgradeTrigger.ConditionBeat, where + ": only blows and beats are counted");
                    if (effect.CountOnly) Assert.AreEqual(UpgradeAction.Spend, effect.Do, where + ": only a Spend counts");
                    if (effect.FromTarget) Assert.AreEqual(UpgradeAction.Arc, effect.Do, where + ": only an arc starts from the target");
                    if (effect.NeedsAtLeast > 0) Assert.AreNotEqual(AfflictionKind.None, effect.Needs, where + ": a depth of nothing");

                    // What counts a depth must have something to count: a Spend before it on the same moment, what the dead
                    // enemy carried (its Needs), or the blows a condition took (an end asked for at least one).
                    bool counts = effect.DamagePerSpent > 0f || (effect.PerStack && effect.Do != UpgradeAction.Spend);
                    if (!counts) continue;
                    bool spentBefore = effects.Take(i).Any(earlier => earlier.Do == UpgradeAction.Spend && earlier.On == effect.On && earlier.Condition == effect.Condition
                        && (earlier.AtCount == 0 || earlier.AtCount == effect.AtCount));
                    bool deadDepth = effect.On == UpgradeTrigger.ConditionDied && effect.Needs != AfflictionKind.None;
                    bool blowsTaken = effect.On == UpgradeTrigger.ConditionEnded && effect.SpentAtLeast > 0;
                    // A death near the condition carries the dead one's doses (Shut in the Dark's jar, StruckBy.DeathNear).
                    bool deathNear = effect.On == UpgradeTrigger.ConditionStruck && effect.By == StruckBy.DeathNear;
                    Assert.IsTrue(spentBefore || (effect.PerStack && (deadDepth || blowsTaken || deathNear)), where + ": counts a depth nothing gave it");
                }

                // A condition spent by a blow answers that same kind of blow, so the blow that spends it is the one answered.
                foreach (var setter in effects.Where(effect => effect.Do == UpgradeAction.Condition && effect.State.EndsWith != ConditionBlow.None))
                {
                    var by = setter.State.EndsWith == ConditionBlow.Heavy ? StruckBy.Heavy : setter.State.EndsWith == ConditionBlow.Weapon ? StruckBy.Weapon
                        : setter.State.EndsWith == ConditionBlow.Finisher ? StruckBy.Finisher : setter.State.EndsWith == ConditionBlow.Shove ? StruckBy.Shove : StruckBy.Any;
                    foreach (var answer in effects.Where(effect => effect.On == UpgradeTrigger.ConditionStruck && effect.Condition == setter.State.Id))
                        Assert.AreEqual(by, answer.By, $"{mark.Id}: {setter.State.Id} is spent by one blow and answers another");
                }
            }
        }

        [Test]
        public void TheTableFindsEachMarkItsEffectsAndItsRest()
        {
            int steam = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Soaked);
            int flashover = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Static);
            int jar = AfflictionPairs.Index(AfflictionKind.Cursed, AfflictionKind.Poison);
            int thermal = AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Chill);
            int thunderGust = AfflictionPairs.Index(AfflictionKind.Static, AfflictionKind.Unsteady);
            int chokedamp = AfflictionPairs.Index(AfflictionKind.Tremor, AfflictionKind.Poison);
            Assert.AreEqual("Steam", ReactionTable.MarkFor(steam).Name);
            Assert.IsNotEmpty(ReactionTable.EffectsFor(steam, new Clover.Player.Build.Loadout()));
            Assert.AreEqual("Thunder Gust", ReactionTable.MarkFor(thunderGust).Name, "built in task 3.4");
            Assert.IsNotEmpty(ReactionTable.EffectsFor(thunderGust, null));
            Assert.AreEqual(3f, ReactionTable.RestFor(steam), "Steam's entry: once every 3 s");
            Assert.AreEqual(4f, ReactionTable.RestFor(flashover), "the shared default");
            Assert.AreEqual(8f, ReactionTable.RestFor(jar), "Shut in the Dark's entry: rest 8 s");
            Assert.AreEqual(6f, ReactionTable.RestFor(thermal), "every distance-3 pair rests 6 s");
            Assert.AreEqual(6f, ReactionTable.RestFor(chokedamp), "Chokedamp's entry: once every 6 s");

            var engine = new ReactionEngine((who, pair, laid) => { }, () => 0);
            ReactionTable.ApplyRests(engine);
            for (int pair = 0; pair < AfflictionPairs.Count; pair++)
                Assert.AreEqual(ReactionTable.RestFor(pair), engine.RestFor(pair), AfflictionPairs.Name(pair));
            Assert.AreEqual(-1, ReactionTable.PairOf("react.nothing"));
        }

        // Each reaction the pair files name, with the whole paragraph that gives it ("**Reaction: Flashover** ... ").
        private static Dictionary<string, string> ReadEntries()
        {
            var entries = new Dictionary<string, string>();
            foreach (string file in PairFiles)
            {
                string path = Path.Combine("docs", "design", "steel", file);
                Assert.IsTrue(File.Exists(path), $"{path} is missing: the reactions are checked against it");
                string name = null;
                var paragraph = new StringBuilder();
                foreach (string line in File.ReadLines(path).Append(""))
                {
                    if (name != null)
                    {
                        if (line.Trim().Length == 0)
                        {
                            entries[name] = paragraph.ToString();
                            name = null;
                            paragraph.Clear();
                        }
                        else paragraph.Append(' ').Append(line.Trim());
                        continue;
                    }
                    if (!line.StartsWith(ReactionLead, StringComparison.Ordinal)) continue;
                    string rest = line.Substring(ReactionLead.Length);
                    int end = rest.IndexOf("**", StringComparison.Ordinal);
                    name = (end >= 0 ? rest.Substring(0, end) : rest).Trim().TrimEnd('.');
                    paragraph.Append(line);
                }
            }
            return entries;
        }
    }
}

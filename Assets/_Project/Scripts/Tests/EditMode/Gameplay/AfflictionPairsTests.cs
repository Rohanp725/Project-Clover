using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Clover.Gameplay;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The 55 pairs of the eleven afflictions (IMPLEMENTATION.md 3.7 and 6.1): each pair once, found in either order,
    /// at the distance and with the reaction the pair files in docs/design/steel/ give it.
    /// </summary>
    public class AfflictionPairsTests
    {
        private static readonly string[] PairFiles = { "pairs_d1_d2.md", "pairs_d3.md", "pairs_d4.md", "pairs_d5.md" };
        private static readonly Regex PairHeading = new Regex(@"^### (\w+) and (\w+) \((\w+) \+ (\w+)\), distance (\d)\s*$");
        private const string ReactionLead = "**Reaction: ";

        private struct WrittenPair
        {
            public AfflictionKind First;
            public AfflictionKind Second;
            public int Distance;
            public string Reaction;
        }

        [Test]
        public void ElevenAfflictionsMakeFiftyFiveDistinctPairs()
        {
            var seen = new HashSet<int>();
            for (int one = 1; one < Afflictions.Count; one++)
            {
                for (int other = one + 1; other < Afflictions.Count; other++)
                {
                    int pair = AfflictionPairs.Index((AfflictionKind)one, (AfflictionKind)other);
                    Assert.That(pair, Is.InRange(0, AfflictionPairs.Count - 1), $"{(AfflictionKind)one} and {(AfflictionKind)other}");
                    Assert.IsTrue(seen.Add(pair), $"{(AfflictionKind)one} and {(AfflictionKind)other} share a pair with another two");
                    var kinds = new[] { AfflictionPairs.First(pair), AfflictionPairs.Second(pair) };
                    CollectionAssert.AreEquivalent(new[] { (AfflictionKind)one, (AfflictionKind)other }, kinds, "the pair holds the two it was found by");
                    Assert.AreEqual((AfflictionKind)other, AfflictionPairs.Partner(pair, (AfflictionKind)one));
                }
            }
            Assert.AreEqual(55, AfflictionPairs.Count);
            Assert.AreEqual(AfflictionPairs.Count, seen.Count);
            Assert.AreEqual(11, AfflictionPairs.Ring.Length);
            CollectionAssert.AllItemsAreUnique(AfflictionPairs.Ring);
        }

        [Test]
        public void ThePairIsTheSameInEitherOrderAndAKindMakesNoneWithItself()
        {
            for (int one = 0; one < Afflictions.Count; one++)
            {
                for (int other = 0; other < Afflictions.Count; other++)
                {
                    var a = (AfflictionKind)one;
                    var b = (AfflictionKind)other;
                    Assert.AreEqual(AfflictionPairs.Index(a, b), AfflictionPairs.Index(b, a), $"{a} and {b}");
                    if (one == other || one == 0 || other == 0) Assert.AreEqual(-1, AfflictionPairs.Index(a, b), $"{a} and {b}");
                }
            }
        }

        [Test]
        public void EachPairIsAtTheDistanceThePairFilesGiveIt()
        {
            var written = ReadPairFiles();
            Assert.AreEqual(AfflictionPairs.Count, written.Count, "the four pair files hold one entry for each pair");
            for (int i = 0; i < written.Count; i++)
            {
                var entry = written[i];
                int pair = AfflictionPairs.Index(entry.First, entry.Second);
                Assert.AreEqual(i, pair, $"{entry.Reaction}: numbered in the order of the files and of README.md's table");
                Assert.AreEqual(entry.Distance, AfflictionPairs.Distance(pair), entry.Reaction);
                Assert.AreEqual(entry.First, AfflictionPairs.First(pair), entry.Reaction);
                Assert.AreEqual(entry.Distance == 3 ? 6f : 4f, AfflictionPairs.Rest(pair), entry.Reaction);
            }
        }

        [Test]
        public void EachPairsIdNamesItsReactionInThePairFiles()
        {
            var written = ReadPairFiles();
            Assert.AreEqual(AfflictionPairs.Count, written.Count);
            var ids = new HashSet<string>();
            foreach (var entry in written)
            {
                int pair = AfflictionPairs.Index(entry.First, entry.Second);
                Assert.AreEqual(entry.Reaction, AfflictionPairs.Name(pair));
                Assert.AreEqual("react." + IdWord(entry.Reaction), AfflictionPairs.Id(pair), entry.Reaction);
                Assert.IsTrue(ids.Add(AfflictionPairs.Id(pair)), $"{AfflictionPairs.Id(pair)} is used twice");
            }
            Assert.AreEqual("react.flashover", AfflictionPairs.Id(AfflictionPairs.Index(AfflictionKind.Burn, AfflictionKind.Static)), "the plan's own example");
        }

        // The pair files' headings ("### Fire and Lightning (Burn + Static), distance 1") and the reaction under each
        // ("**Reaction: Flashover** ..."), in the order the files give them.
        private static List<WrittenPair> ReadPairFiles()
        {
            var pairs = new List<WrittenPair>();
            foreach (string file in PairFiles)
            {
                string path = Path.Combine("docs", "design", "steel", file);
                Assert.IsTrue(File.Exists(path), $"{path} is missing: the pairs are checked against it");
                WrittenPair? open = null;
                foreach (string line in File.ReadLines(path))
                {
                    var heading = PairHeading.Match(line);
                    if (heading.Success)
                    {
                        open = new WrittenPair
                        {
                            First = KindNamed(heading.Groups[3].Value),
                            Second = KindNamed(heading.Groups[4].Value),
                            Distance = int.Parse(heading.Groups[5].Value),
                        };
                        continue;
                    }
                    if (open.HasValue && line.StartsWith(ReactionLead, StringComparison.Ordinal))
                    {
                        string rest = line.Substring(ReactionLead.Length);
                        int end = rest.IndexOf("**", StringComparison.Ordinal);
                        var entry = open.Value;
                        entry.Reaction = (end >= 0 ? rest.Substring(0, end) : rest).Trim().TrimEnd('.');
                        pairs.Add(entry);
                        open = null;
                    }
                }
            }
            return pairs;
        }

        // The files write the poison's affliction as "Poisoned"; every other is its kind's own name.
        private static AfflictionKind KindNamed(string word) =>
            word == "Poisoned" ? AfflictionKind.Poison : (AfflictionKind)Enum.Parse(typeof(AfflictionKind), word);

        private static string IdWord(string name)
        {
            var letters = new StringBuilder();
            foreach (char letter in name)
                if (char.IsLetterOrDigit(letter)) letters.Append(char.ToLowerInvariant(letter));
            return letters.ToString();
        }
    }
}

using System;
using System.Collections.Generic;
using Clover.UI.Toolkit;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The Loadout's stick map (ui_rest_fire_plan.md 3.5.4 and 5.3): every direction from every place lands where the
    /// table says, a push and its opposite come back, nothing wraps, nothing is a dead end, and the held push repeats at
    /// 0.32 s and then 0.2 s.
    /// </summary>
    public class LoadoutNavigationTests
    {
        // From, direction, where it lands the first time ("-" stays), as 3.5.4's table writes it.
        private static readonly string[] Table =
        {
            "Aspect Up -", "Aspect Down Keystone1", "Aspect Left -", "Aspect Right Temper",
            "Keystone1 Up Aspect", "Keystone1 Down Keystone2", "Keystone1 Left -", "Keystone1 Right Bearing1",
            "Keystone2 Up Keystone1", "Keystone2 Down Infused", "Keystone2 Left -", "Keystone2 Right Bearing2",
            "Temper Up -", "Temper Down Technique", "Temper Left Aspect", "Temper Right -",
            "Technique Up Temper", "Technique Down Bearing1", "Technique Left Aspect", "Technique Right -",
            "Bearing1 Up Technique", "Bearing1 Down Bearing2", "Bearing1 Left Keystone1", "Bearing1 Right -",
            "Bearing2 Up Bearing1", "Bearing2 Down Infused", "Bearing2 Left Keystone2", "Bearing2 Right -",
            "Infused Up Keystone2", "Infused Down North", "Infused Left -", "Infused Right -",
            "ElementHeader Up Infused", "ElementHeader Down West", "ElementHeader Left -", "ElementHeader Right North",
            "North Up Infused", "North Down Surge", "North Left ElementHeader", "North Right RoleStrip",
            "RoleStrip Up Infused", "RoleStrip Down East", "RoleStrip Left North", "RoleStrip Right -",
            "West Up ElementHeader", "West Down Starter", "West Left -", "West Right Surge",
            "Surge Up North", "Surge Down South", "Surge Left West", "Surge Right East",
            "East Up RoleStrip", "East Down Hand3", "East Left Surge", "East Right -",
            "South Up Surge", "South Down Hand1", "South Left West", "South Right East",
            "Starter Up West", "Starter Down -", "Starter Left -", "Starter Right Hand1",
            "Hand1 Up South", "Hand1 Down -", "Hand1 Left Starter", "Hand1 Right Hand2",
            "Hand2 Up South", "Hand2 Down -", "Hand2 Left Hand1", "Hand2 Right Hand3",
            "Hand3 Up East", "Hand3 Down -", "Hand3 Left Hand2", "Hand3 Right -",
        };

        private static IEnumerable<object[]> TableCases()
        {
            foreach (string row in Table) yield return row.Split(' ');
        }

        private static LoadoutPlace P(string name) => (LoadoutPlace)Enum.Parse(typeof(LoadoutPlace), name);
        private static NavDirection D(string name) => (NavDirection)Enum.Parse(typeof(NavDirection), name);

        [Test]
        public void TheTableNamesEveryPlaceInEveryDirection()
        {
            Assert.AreEqual(LoadoutPlaces.Count * 4, Table.Length);
            Assert.AreEqual(LoadoutPlaces.Count, Enum.GetValues(typeof(LoadoutPlace)).Length);
            Assert.AreEqual(LoadoutPlaces.Count, LoadoutPlaces.Names.Length);
        }

        [TestCaseSource(nameof(TableCases))]
        public void EveryDirectionFromEveryPlaceLandsWhereTheTableSays(string from, string direction, string to)
        {
            var grid = LoadoutPlaces.NewGrid();
            var start = P(from);
            var landed = (LoadoutPlace)grid.Move((int)start, D(direction));
            Assert.AreEqual(to == "-" ? start : P(to), landed, $"{from} {direction}");
        }

        [Test]
        public void APushAndItsOppositeAlwaysComeBack()
        {
            foreach (LoadoutPlace place in Enum.GetValues(typeof(LoadoutPlace)))
                foreach (NavDirection direction in Enum.GetValues(typeof(NavDirection)))
                {
                    var grid = LoadoutPlaces.NewGrid();
                    int there = grid.Move((int)place, direction);
                    if (there == (int)place) continue;
                    int back = grid.Move(there, FocusGrid.Opposite(direction));
                    Assert.AreEqual(place, (LoadoutPlace)back, $"{place} {direction} reached {(LoadoutPlace)there}, and back came to {(LoadoutPlace)back}");
                }
        }

        [Test]
        public void ASharedNeighbourRemembersWhichPlaceFocusCameFrom()
        {
            var grid = LoadoutPlaces.NewGrid();
            int at = grid.Move((int)LoadoutPlace.South, NavDirection.Left);
            Assert.AreEqual(LoadoutPlace.West, (LoadoutPlace)at);
            Assert.AreEqual(LoadoutPlace.South, (LoadoutPlace)grid.Move(at, NavDirection.Right), "right from West goes back to South");

            at = grid.Move((int)LoadoutPlace.Bearing2, NavDirection.Down);
            Assert.AreEqual(LoadoutPlace.Bearing2, (LoadoutPlace)grid.Move(at, NavDirection.Up), "up from the infused strikes goes back to Bearing 2");

            at = grid.Move((int)LoadoutPlace.Hand2, NavDirection.Up);
            Assert.AreEqual(LoadoutPlace.Hand2, (LoadoutPlace)grid.Move(at, NavDirection.Down), "down from South goes back to Hand 2");

            at = grid.Move((int)LoadoutPlace.Technique, NavDirection.Left);
            Assert.AreEqual(LoadoutPlace.Technique, (LoadoutPlace)grid.Move(at, NavDirection.Right), "right from the aspect goes back to the technique");

            grid.Forget();
            Assert.AreEqual(LoadoutPlace.Temper, (LoadoutPlace)grid.Move((int)LoadoutPlace.Aspect, NavDirection.Right), "forgotten: Temper again");
        }

        [Test]
        public void EveryPlaceIsReachableFromTheOpeningPlaceAndNoneIsADeadEnd()
        {
            var grid = LoadoutPlaces.NewGrid();
            var seen = new HashSet<int> { (int)LoadoutPlaces.ElementFirst };
            var queue = new Queue<int>(seen);
            while (queue.Count > 0)
            {
                int at = queue.Dequeue();
                bool leaves = false;
                foreach (NavDirection direction in Enum.GetValues(typeof(NavDirection)))
                {
                    int next = grid.Neighbour(at, direction);
                    if (next == at) continue;
                    leaves = true;
                    if (seen.Add(next)) queue.Enqueue(next);
                }
                Assert.IsTrue(leaves, $"{(LoadoutPlace)at} is a dead end");
            }
            Assert.AreEqual(LoadoutPlaces.Count, seen.Count, "some places cannot be reached from West");
        }

        [Test]
        public void TheHalvesSplitAtTheInfusedStrikes()
        {
            Assert.IsTrue(LoadoutPlaces.InSteel(LoadoutPlace.Aspect));
            Assert.IsTrue(LoadoutPlaces.InSteel(LoadoutPlace.Infused));
            Assert.IsFalse(LoadoutPlaces.InSteel(LoadoutPlace.ElementHeader));
            Assert.IsFalse(LoadoutPlaces.InSteel(LoadoutPlace.Hand3));
            for (int slot = 0; slot <= 4; slot++) Assert.AreEqual(slot, LoadoutPlaces.SlotOf(LoadoutPlaces.PlaceOfSlot(slot)));
        }

        [Test]
        public void AHeldPushRepeatsAfterAThirdOfASecondThenEveryFifth()
        {
            var repeat = new NavRepeat();
            Assert.IsTrue(repeat.Step(0f, 1f, 0.016f, out var first));
            Assert.AreEqual(NavDirection.Up, first);
            Assert.IsFalse(repeat.Step(0f, 1f, 0.3f, out _), "not before 0.32 s");
            Assert.IsTrue(repeat.Step(0f, 1f, 0.03f, out _), "at 0.32 s");
            Assert.IsFalse(repeat.Step(0f, 1f, 0.15f, out _));
            Assert.IsTrue(repeat.Step(0f, 1f, 0.06f, out _), "then every 0.2 s");
            Assert.IsFalse(repeat.Step(0.1f, 0.2f, 0.016f, out _), "let go");
            Assert.IsTrue(repeat.Step(0.9f, 0.3f, 0.016f, out var right));
            Assert.AreEqual(NavDirection.Right, right);
            Assert.IsTrue(repeat.Step(0f, -0.9f, 0.016f, out var down), "a new direction moves at once");
            Assert.AreEqual(NavDirection.Down, down);
        }
    }
}

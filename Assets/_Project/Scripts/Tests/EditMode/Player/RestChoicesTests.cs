using Clover.UI;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>What a rest fire offers (IMPLEMENTATION.md 4.1): the Hand first, the shapes, the chart, the Way and the hub only once built, leave last.</summary>
    public class RestChoicesTests
    {
        [Test]
        public void TheFireOffersTheHandFirstThenTheShapesTheChartAndLeaveLast()
        {
            var choices = new RestChoices();
            Assert.AreEqual(4, choices.Count);
            Assert.AreEqual(RestEntry.Hand, choices.At(0));
            Assert.AreEqual(RestEntry.Shapes, choices.At(1));
            Assert.AreEqual(RestEntry.Chart, choices.At(2));
            Assert.AreEqual(RestEntry.Leave, choices.At(3));
            Assert.AreEqual(-1, choices.IndexOf(RestEntry.Way), "the Way is not built yet");
            Assert.AreEqual(-1, choices.IndexOf(RestEntry.Hub), "nor the road to the hub");
        }

        [Test]
        public void TheWayAndTheHubComeBeforeLeaveOnceBuilt()
        {
            var choices = new RestChoices(true, true);
            CollectionAssert.AreEqual(new[] { RestEntry.Hand, RestEntry.Shapes, RestEntry.Chart, RestEntry.Way, RestEntry.Hub, RestEntry.Leave },
                new[] { choices.At(0), choices.At(1), choices.At(2), choices.At(3), choices.At(4), choices.At(5) });
        }

        [Test]
        public void TheChoiceMovesRoundTheEndsAndAPressActsAtOnce()
        {
            var choices = new RestChoices();
            Assert.AreEqual(RestEntry.Hand, choices.Press(), "nothing at a fire asks twice");
            choices.Move(-1);
            Assert.AreEqual(RestEntry.Leave, choices.Current);
            choices.Move(1);
            Assert.AreEqual(RestEntry.Hand, choices.Current);
            choices.Move(6);
            Assert.AreEqual(RestEntry.Chart, choices.Current);
            Assert.IsTrue(choices.Choose(3));
            Assert.AreEqual(RestEntry.Leave, choices.Press());
            Assert.IsFalse(choices.Choose(4));
            choices.Reset();
            Assert.AreEqual(RestEntry.Hand, choices.Current);
        }

        [Test]
        public void EveryEntryHasALabelAndANote()
        {
            foreach (RestEntry entry in System.Enum.GetValues(typeof(RestEntry)))
            {
                if (entry == RestEntry.None) continue;
                Assert.IsNotEmpty(RestChoices.Label(entry), entry.ToString());
                Assert.IsNotEmpty(RestChoices.Note(entry), entry.ToString());
            }
        }
    }
}

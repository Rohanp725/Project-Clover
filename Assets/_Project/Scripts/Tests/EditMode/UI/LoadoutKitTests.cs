using System;
using Clover.Player;
using Clover.UI.Toolkit;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>The menus' kit (ui_rest_fire_plan.md 4.1, 4.3, 3.5.8): the painted icons, the text sizes, and the words' length.</summary>
    public class LoadoutKitTests
    {
        [Test]
        public void EveryPaintedIconParsesAndStaysInsideItsBox()
        {
            foreach (string key in IconShapes.Keys)
            {
                var shape = IconShapes.Get(key);
                Assert.Greater(shape.Layers.Length, 0, key);
                foreach (var layer in shape.Layers)
                {
                    var steps = IconShapes.Steps(layer.Path);
                    Assert.Greater(steps.Length, 1, key);
                    Assert.AreEqual(IconShapes.StepKind.Move, steps[0].Kind, key + " begins with a move");
                    foreach (var step in steps)
                    {
                        if (step.Kind == IconShapes.StepKind.Close) continue;
                        var point = step.Kind == IconShapes.StepKind.Cubic ? step.C : step.Kind == IconShapes.StepKind.Quad || step.Kind == IconShapes.StepKind.Arc ? step.B : step.A;
                        Assert.That(point.x, Is.InRange(-0.5f, shape.Box + 0.5f), key + " x");
                        Assert.That(point.y, Is.InRange(-0.5f, shape.Box + 0.5f), key + " y");
                    }
                }
            }
        }

        [Test]
        public void EveryElementRoleAndWayMarkWrittenHereHasAnIcon()
        {
            foreach (var element in Elements.All) Assert.IsTrue(IconShapes.Has(element.Icon), element.Name + "'s icon " + element.Icon);
            foreach (var role in LoadoutWords.Roles) Assert.IsTrue(IconShapes.Has(IconShapes.ForRole(role)), role.ToString());
            foreach (string key in new[] { IconShapes.Lock, IconShapes.Mote, IconShapes.Shard, IconShapes.Hand, IconShapes.Anvil })
                Assert.IsTrue(IconShapes.Has(key), key);
            Assert.AreEqual(IconShapes.Flame, IconShapes.ForElement("Fire"));
            Assert.AreEqual(IconShapes.Star, IconShapes.ForElement("Nothing"));
        }

        [Test]
        public void ArcsMeetTheirEndPoints()
        {
            // A circle drawn as two half arcs: each ends where the path says.
            var steps = IconShapes.Steps("M20 32a12 12 0 1 0 24 0a12 12 0 1 0-24 0z");
            Assert.AreEqual(IconShapes.StepKind.Arc, steps[1].Kind);
            Assert.AreEqual(32f, steps[1].A.x, 0.01f, "centre x");
            Assert.AreEqual(32f, steps[1].A.y, 0.01f, "centre y");
            Assert.AreEqual(12f, steps[1].Radius, 0.01f);
            Assert.AreEqual(44f, steps[1].B.x, 0.01f);
            Assert.AreEqual(20f, steps[2].B.x, 0.01f);
        }

        [Test]
        public void TheTextSizeStepsRoundAndNamesItsClass()
        {
            Assert.AreEqual(125, TextSizes.Next(100));
            Assert.AreEqual(150, TextSizes.Next(125));
            Assert.AreEqual(100, TextSizes.Next(150));
            Assert.AreEqual(100, TextSizes.Normalise(0));
            Assert.AreEqual(150, TextSizes.Normalise(200));
            Assert.AreEqual("", TextSizes.ClassOf(100));
            Assert.AreEqual("size-125", TextSizes.ClassOf(125));
            Assert.AreEqual("size-150", TextSizes.ClassOf(150));
            Assert.AreEqual("Text size 150%", TextSizes.Words(150));
        }

        [Test]
        public void EveryWrittenSentenceIsFifteenWordsOrFewer()
        {
            foreach (var role in LoadoutWords.Roles) Short(LoadoutWords.RoleMeaning(role));
            foreach (string id in new[]
                     {
                         Clover.Player.Trees.Ways.KatanaWay.Sheath, Clover.Player.Trees.Ways.KatanaWay.Stream, Clover.Player.Trees.Ways.KatanaWay.Answer,
                         Clover.Player.Trees.Ways.KatanaWay.Storm, Clover.Player.Trees.Ways.KatanaWay.TheCutLandsAtTheClick,
                         Clover.Player.Trees.Ways.KatanaWay.AHundredDays, Clover.Player.Trees.Ways.BodyWay.EnduringBody, Clover.Player.Trees.Ways.BodyWay.LightBody,
                     })
            {
                Assert.IsTrue(LoadoutWords.Has(id), id);
                var entry = LoadoutWords.For(id);
                Short(entry.Short);
                Short(entry.Gives);
                Short(entry.Change);
                Assert.IsTrue(IconShapes.Has(entry.Icon), id + "'s icon " + entry.Icon);
            }
            Assert.AreEqual("No movement spell", LoadoutWords.Missing(new[] { 0, 1, 1, 1, 1, 0 }));
            Assert.AreEqual("No opener or counter", LoadoutWords.Missing(new[] { 0, 0, 1, 1, 0, 1 }));
            Assert.AreEqual("", LoadoutWords.Missing(new[] { 0, 1, 1, 1, 1, 1 }));
        }

        private static void Short(string words)
        {
            foreach (string sentence in words.Split(new[] { ". " }, StringSplitOptions.RemoveEmptyEntries))
                Assert.LessOrEqual(sentence.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Length, 15, sentence);
        }
    }
}

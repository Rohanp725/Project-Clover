using System;
using System.Linq;
using Clover.Player;
using Clover.Player.Forms;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>Each element's own moves: the rules of Riptide's wave, what every tap spell costs and wears, and the stuff each element's spells are made of.</summary>
    public class ElementMovesTests
    {
        [TearDown]
        public void LetGo() => ElementSubstance.Hold(null);

        [Test]
        public void TheWaveStandsWhereItRoseThenComesInAndStopsWhereItBreaks()
        {
            Assert.AreEqual(5f, TideRules.Front(5f, 0f), 1e-4f);
            Assert.AreEqual(5f, TideRules.Front(5f, TideRules.RiseSeconds), 1e-4f);
            float early = TideRules.Front(5f, TideRules.RiseSeconds + 0.1f), later = TideRules.Front(5f, TideRules.RiseSeconds + 0.2f);
            Assert.Less(early, 5f);
            Assert.Less(later, early, "The wave must come toward the caster, not go away.");
            Assert.AreEqual(TideRules.BreaksAt, TideRules.Front(5f, 10f), 1e-4f);
            Assert.IsFalse(TideRules.Broken(5f, 0.1f));
            Assert.IsTrue(TideRules.Broken(5f, 10f));
        }

        [Test]
        public void TheWaveCarriesOnlyWhatItsFaceHasReached()
        {
            // Standing three metres out on the line: nothing while the wave is still beyond it, carried once the face arrives.
            Assert.AreEqual(0f, TideRules.Carry(3f, 0f, 4.5f, 0.2f));
            Assert.AreEqual(0.2f, TideRules.Carry(3f, 0f, 3.2f, 0.2f), 1e-4f);
            // Off to the side of the wave, beyond where it rose, or already at the caster's feet: left alone.
            Assert.AreEqual(0f, TideRules.Carry(3f, TideRules.HalfWidth + 0.5f, 3f, 0.2f));
            Assert.AreEqual(0f, TideRules.Carry(7f, 0f, 5f, 0.2f));
            Assert.AreEqual(0f, TideRules.Carry(1f, 0f, 1.3f, 0.2f));
            // A wave that has not moved carries nothing.
            Assert.AreEqual(0f, TideRules.Carry(3f, 0f, 3f, 0f));
            Assert.GreaterOrEqual(TideRules.Carry(3f, -1f, 3f, 0.2f), 0f, "A carry is a distance toward the caster: never negative.");
        }

        [Test]
        public void TheWaveOwesABodyTheWayToWhereItBreaksAndNoMore()
        {
            // Paid in full, with the knock of the blow, a body is set down where the wave breaks: sword reach, not at the feet.
            foreach (float start in new[] { 2.2f, 3f, 4f, 5f })
                Assert.AreEqual(TideRules.BreaksAt, start - TideRules.Owed(start) - TideRules.Knock, 1e-4f);
            // One already nearer than that is owed nothing: the wave never pushes anything away or past the caster.
            Assert.AreEqual(0f, TideRules.Owed(1.4f));
            Assert.AreEqual(0f, TideRules.Owed(0.5f));
        }

        [Test]
        public void TheGustsShoveIsSizedSoABodyTravelsThreeMetres()
        {
            Assert.AreEqual(3f, GustRules.Impulse * GustRules.ShoveTravels, 1e-4f);
            Assert.AreEqual(4.5f, GustRules.Impulse, 1e-4f);
        }

        [Test]
        public void ABodyTheWaveTakesIsSetDownInSwordReachAndNeverBehindTheCaster()
        {
            foreach (float start in new[] { 1.8f, 3f, 4.4f, 5.2f })
            {
                float along = start, last = TideRules.Front(5f, 0f);
                for (float age = 0f; age < 2f; age += 1f / 60f)
                {
                    float front = TideRules.Front(5f, age);
                    along -= TideRules.Carry(along, 0.5f, front, last - front);
                    last = front;
                }
                Assert.AreEqual(TideRules.BreaksAt, along, 0.25f, $"A body that stood {start} m out should be set down where the wave breaks.");
                Assert.GreaterOrEqual(along, TideRules.BreaksAt - 1e-3f, "The wave never carries a body past where it breaks.");
            }
            // One standing beyond where the wave rose is not taken at all.
            Assert.AreEqual(0f, TideRules.Carry(6.5f, 0f, TideRules.Front(5f, 0.3f), 0.2f));
        }

        [Test]
        public void EveryTapSpellIsItsElementsOwnAndKeepsItsPrice()
        {
            // Mana and damage as they were before each spell was given its own behaviour.
            var kept = new (string element, float cost, float damage)[]
            {
                (Elements.Fire, 25f, 20f), (Elements.Lightning, 25f, 16f), (Elements.Wind, 20f, 8f), (Elements.Ice, 25f, 10f),
                (Elements.Water, 20f, 8f), (Elements.Life, 20f, 4f), (Elements.Earth, 25f, 14f), (Elements.Metal, 25f, 12f),
                (Elements.Light, 25f, 8f), (Elements.Dark, 25f, 10f), (Elements.Poison, 25f, 1f),
            };
            Assert.AreEqual(Elements.Count, kept.Length);
            Assert.IsNotNull(SpellForms.Get(SpellForms.Tap));
            foreach (var (element, cost, damage) in kept)
            {
                var kit = ElementKit.ByName(element, false);
                Assert.AreEqual(SpellForms.Tap, kit.SpellForm, element + "'s tap spell should be its own.");
                Assert.AreEqual(cost, kit.SpellCost, element);
                Assert.AreEqual(damage, kit.SpellDamage, element);
                // The form moves what it moves itself: the old all-round drag is not laid on top.
                Assert.AreEqual(0f, kit.SpellPull, element);
            }
        }

        [Test]
        public void EveryElementIsMadeOfItsOwnStuff()
        {
            var splinters = Enumerable.Range(0, Elements.Count).Select(element => ElementSubstance.Shape(FormMesh.Shard, element)).ToList();
            Assert.AreEqual(Elements.Count, splinters.Distinct().Count(), "Two elements throw the same splinter: " + string.Join(", ", splinters));
            // Shapes that are already something are left alone, and so is a colour that is no element's.
            Assert.AreEqual(FormMesh.Sword, ElementSubstance.Shape(FormMesh.Sword, ElementSubstance.Fire));
            Assert.AreEqual(FormMesh.Shard, ElementSubstance.Shape(FormMesh.Shard, -1));
            Assert.AreEqual(FormMesh.Rock, ElementSubstance.Shape(FormMesh.Spike, ElementSubstance.Earth));
            Assert.AreEqual(FormMesh.Flame, ElementSubstance.Shape(FormMesh.Spike, ElementSubstance.Fire));
            foreach (var info in Elements.All)
                Assert.AreEqual(info.Index, Array.IndexOf(new[] { ElementSubstance.Fire, ElementSubstance.Lightning, ElementSubstance.Wind, ElementSubstance.Ice, ElementSubstance.Water,
                    ElementSubstance.Life, ElementSubstance.Earth, ElementSubstance.Metal, ElementSubstance.Light, ElementSubstance.Dark, ElementSubstance.Poison }, info.Index), info.Name);
            Assert.AreEqual(Elements.Fire, Elements.All[ElementSubstance.Fire].Name);
            Assert.AreEqual(Elements.Water, Elements.All[ElementSubstance.Water].Name);
            Assert.AreEqual(Elements.Poison, Elements.All[ElementSubstance.Poison].Name);
        }

        [Test]
        public void OnlyTheHeldElementsColourIsTakenForItsStuff()
        {
            var water = Elements.ByName(Elements.Water);
            Assert.AreEqual(-1, ElementSubstance.Of(water.Colour), "With no element held nothing is swapped.");
            ElementSubstance.Hold(water);
            Assert.AreEqual(ElementSubstance.Water, ElementSubstance.Of(water.Colour));
            // The same hue brighter or darker is still water; another element's colour, white and black are not.
            Assert.AreEqual(ElementSubstance.Water, ElementSubstance.Of(water.Colour * 2.6f));
            Assert.AreEqual(-1, ElementSubstance.Of(Elements.Colour(Elements.Fire)));
            Assert.AreEqual(-1, ElementSubstance.Of(Color.white));
            Assert.AreEqual(-1, ElementSubstance.Of(Color.black));
        }

        [Test]
        public void EveryShapeCanBeBuilt()
        {
            foreach (FormMesh shape in Enum.GetValues(typeof(FormMesh)))
            {
                var mesh = FormMeshes.Build(shape);
                Assert.Greater(mesh.vertexCount, 0, shape + " has no faces.");
                Assert.Less(mesh.vertexCount, 1500, shape + " is heavier than a spell piece should be.");
                Assert.Greater(mesh.bounds.size.magnitude, 0.2f, shape.ToString());
                UnityEngine.Object.DestroyImmediate(mesh);
            }
        }
    }
}

using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Clover.Player;
using Clover.Player.Forms;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>The library of spell forms: every name the trees may use is there, forms keep nothing between casts, and the plain geometry they share is right.</summary>
    public class SpellFormTests
    {
        // The ids fixed in docs/design/FORMS.md, and the five that are not on the rack: the four that were there before them and the elements' own tap spells.
        private static readonly string[] Promised =
        {
            "wave", "bolt", "leap", "rod", "tap",
            "blades", "sword", "beam", "phalanx", "dragon", "nova", "orb", "bomb", "spikes", "field", "drill", "minions",
            "bullets", "reverse", "parry", "wall", "discs", "brand", "rain", "clone", "flash", "ring", "pillars", "step",
        };

        // The bespoke moves in the same file: the gods' gifts and the crowns. They belong to their marks and have no sample on the rack.
        private static readonly string[] Bespoke =
        {
            "sun", "remember", "stillworld", "shatterchain", "sea", "spring", "hand", "hangingswords", "scales", "rise", "cleanblade",
            "thunder", "dust", "plates", "lotus", "groundwakes", "anvil", "mirrors", "totality", "reaping", "pyres",
            "flashes", "frozenwave", "ball", "heart", "skydance", "circle", "eye", "sunspot", "lodestone", "waiting", "rush",
            "turningblade",
        };

        [Test]
        public void EveryPromisedFormIsThereUnderItsOwnName()
        {
            foreach (string id in Promised.Concat(Bespoke))
            {
                var form = SpellForms.Get(id);
                Assert.IsNotNull(form, $"No form is registered as '{id}'.");
                Assert.AreEqual(id, form.Id);
            }
            Assert.AreEqual(Promised.Length + Bespoke.Length, SpellForms.All.Count, "A form is registered that docs/design/FORMS.md does not name, or one is registered twice.");
            Assert.AreEqual(SpellForms.All.Count, SpellForms.All.Select(form => form.Id).Distinct().Count());
        }

        [Test]
        public void AFormKeepsNothingOfItsOwnBetweenCasts()
        {
            foreach (var form in SpellForms.All)
            {
                var fields = form.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                Assert.IsEmpty(fields, $"{form.GetType().Name} has a field of its own ({string.Join(", ", fields.Select(field => field.Name))}); what a cast must remember belongs in its SpellRun.");
            }
        }

        [Test]
        public void EveryNewFormHasOneSampleSpellThatNamesIt()
        {
            foreach (string id in Promised.Skip(5))
            {
                var sample = FormSamples.Of(id);
                Assert.IsNotNull(sample, $"The form '{id}' has no sample spell for the yard's rack.");
                Assert.IsNotNull(sample.Spell);
                Assert.IsNotNull(Elements.ByName(sample.Element), $"The sample of '{id}' names no element.");
                Assert.IsFalse(sample.Name.Contains("!"));
            }
            Assert.AreEqual(Promised.Length - 5, FormSamples.All.Count);
            // Between them the samples show every element's colour.
            Assert.AreEqual(Elements.Count, FormSamples.All.Select(sample => sample.Element).Distinct().Count());
        }

        [Test]
        public void ThingsOnARingStandEvenlyRoundItStartingAhead()
        {
            Assert.AreEqual(0f, Vector3.Distance(new Vector3(0f, 0f, 2f), FormRules.OnRing(0, 4, 2f)), 0.0001f);
            Assert.AreEqual(0f, Vector3.Distance(new Vector3(2f, 0f, 0f), FormRules.OnRing(1, 4, 2f)), 0.0001f);
            Assert.AreEqual(0f, Vector3.Distance(new Vector3(0f, 0f, -2f), FormRules.OnRing(2, 4, 2f)), 0.0001f);
            Assert.AreEqual(0f, Vector3.Distance(new Vector3(2f, 0f, 0f), FormRules.OnRing(0, 4, 2f, 90f)), 0.0001f);
        }

        [Test]
        public void ScatteredThingsStartAtTheMiddleAndNeverLeaveTheDisc()
        {
            Assert.AreEqual(Vector3.zero, FormRules.Scattered(0, 7, 3f));
            Assert.AreEqual(Vector3.zero, FormRules.Scattered(0, 1, 3f));
            var seen = new HashSet<Vector3>();
            for (int i = 0; i < 7; i++)
            {
                Vector3 spot = FormRules.Scattered(i, 7, 3f);
                Assert.LessOrEqual(spot.magnitude, 3.0001f);
                Assert.IsTrue(seen.Add(spot), "Two of them lie on the same spot.");
            }
        }

        [Test]
        public void ARowIsCentredAndAFanIsSymmetrical()
        {
            Assert.AreEqual(-1f, FormRules.InRow(0, 3, 1f), 0.0001f);
            Assert.AreEqual(0f, FormRules.InRow(1, 3, 1f), 0.0001f);
            Assert.AreEqual(1f, FormRules.InRow(2, 3, 1f), 0.0001f);
            Assert.AreEqual(-0.75f, FormRules.InRow(0, 2, 1.5f), 0.0001f);
            Assert.AreEqual(0f, FormRules.InFan(0, 1, 120f), 0.0001f);
            Assert.AreEqual(-60f, FormRules.InFan(0, 5, 120f), 0.0001f);
            Assert.AreEqual(0f, FormRules.InFan(2, 5, 120f), 0.0001f);
            Assert.AreEqual(60f, FormRules.InFan(4, 5, 120f), 0.0001f);
        }

        [Test]
        public void AWallStandsBetweenTwoPointsOnlyWhenTheLineBetweenThemCrossesIt()
        {
            Vector3 left = new Vector3(-3f, 0f, 4f), right = new Vector3(3f, 0f, 4f);
            Assert.IsTrue(FormRules.Crosses(new Vector3(0f, 0f, 8f), Vector3.zero, left, right), "A blow from straight beyond the wall.");
            Assert.IsFalse(FormRules.Crosses(new Vector3(0f, 0f, 3f), Vector3.zero, left, right), "A blow from the player's own side of it.");
            Assert.IsFalse(FormRules.Crosses(new Vector3(9f, 0f, 8f), new Vector3(9f, 0f, 0f), left, right), "A blow that passes the wall's end.");
            Assert.IsFalse(FormRules.Crosses(new Vector3(0f, 0f, -6f), Vector3.zero, left, right), "A blow from behind the player.");
            // Height does not come into it.
            Assert.IsTrue(FormRules.Crosses(new Vector3(1f, 5f, 9f), new Vector3(0f, 1f, 0f), left, right));
        }

        [Test]
        public void AFormAlwaysShowsItselfComingAndAnEchoIsAShareOfTheBlow()
        {
            Assert.AreEqual(FormRules.LeastWarning, FormRules.Warning(0f), 0.0001f);
            Assert.AreEqual(1.5f, FormRules.Warning(1.5f), 0.0001f);
            Assert.AreEqual(12f, FormRules.Echo(30f, 40f), 0.0001f);
            Assert.AreEqual(0f, FormRules.Echo(30f, -5f), 0.0001f);
            Assert.AreEqual(0f, FormRules.Echo(-30f, 40f), 0.0001f);
            Assert.AreEqual(6, FormRules.Pieces(6f, 1f));
            Assert.AreEqual(7, FormRules.Pieces(6.2f, 1f));
            Assert.AreEqual(1, FormRules.Pieces(0f, 1f));
        }

        [Test]
        public void EverySolidShapeIsDrawnWithFlatFacesAndALineToInk()
        {
            foreach (FormMesh which in System.Enum.GetValues(typeof(FormMesh)))
            {
                var mesh = FormMeshes.Build(which);
                try
                {
                    Assert.Greater(mesh.vertexCount, 0, $"{which} has no corners.");
                    if (which == FormMesh.Crest)
                    {
                        // Water's wave is the one smooth shape: its faces share their corners, and it carries its paint.
                        Assert.Greater(mesh.triangles.Length, mesh.vertexCount, "The wave should be one skin with shared corners.");
                        Assert.AreEqual(mesh.vertexCount, mesh.colors.Length, "The wave carries no paint.");
                        Assert.IsTrue(mesh.colors.Any(paint => paint.a < 0.5f) && mesh.colors.Any(paint => paint.a > 0.5f), "The wave should have both foam and water on it.");
                    }
                    else
                    {
                        Assert.AreEqual(0, mesh.vertexCount % 3, $"{which}: every face has its own three corners.");
                        Assert.AreEqual(mesh.vertexCount, mesh.triangles.Length);
                    }
                    var smooth = new List<Vector3>();
                    mesh.GetUVs(1, smooth);
                    Assert.AreEqual(mesh.vertexCount, smooth.Count, $"{which} carries no smoothed directions for its ink line.");
                    Assert.IsTrue(smooth.All(direction => Mathf.Abs(direction.magnitude - 1f) < 0.01f), $"{which}: a smoothed direction is not of length one.");
                    Assert.Greater(mesh.bounds.size.magnitude, 0.5f, $"{which} is a speck.");
                }
                finally
                {
                    Object.DestroyImmediate(mesh);
                }
            }
        }
    }
}

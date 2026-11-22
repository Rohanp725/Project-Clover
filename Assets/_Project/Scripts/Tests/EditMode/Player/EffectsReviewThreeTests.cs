using System.Linq;
using Clover.EditorTools;
using Clover.Gameplay;
using Clover.Player.Forms;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// The owner's third review of the effects (docs/design/feedback/2026-11-20_effects_review_3.md), built from the Blender
    /// worker's elements 4 and reactions 5: the picked options only, the looks in the keys the game plays, Marked, Glaze,
    /// Bedrock, Gust and Stone Rise drawn by their new looks, Quickening without its green-brown circle, Windthrow's pit and
    /// seeds kept, the anvil and the heave raising the body, and nothing new painted white.
    /// </summary>
    public class EffectsReviewThreeTests
    {
        private static ElementLookSet Built()
        {
            var set = ElementLookBuilder.Set();
            if (set == null || !ElementLookBuilder.Reactions5In()) Assert.Ignore("The third review's recipes are not in the project, or the looks are not built.");
            return set;
        }

        [Test]
        public void OnlyThePickedOptionsAreBuiltUnderTheReactionsOwnNames()
        {
            var set = Built();
            Assert.Less(set.Find("thermal_shock_a"), 0);
            Assert.Less(set.Find("thermal_shock_b"), 0);
            Assert.Less(set.Find("chokedamp_a"), 0);
            Assert.Less(set.Find("chokedamp_b"), 0);
            Assert.GreaterOrEqual(set.Find("thermal_shock"), 0);
            Assert.GreaterOrEqual(set.Find("chokedamp"), 0);
            Assert.AreEqual("thermal_shock", ElementLookBuilder.R5GameSlugs["thermal_shock_b"], "the owner picked Thermal Shock B");
            Assert.AreEqual("chokedamp", ElementLookBuilder.R5GameSlugs["chokedamp_a"], "the owner picked Chokedamp A");
            // Thermal Shock B's frozen flames are ice flames, not the two elements' pieces stacked.
            Assert.IsTrue(PieceNames(set, "thermal_shock").Any(name => name.Contains("iceflame") || name.Contains("frozen")), "Thermal Shock B's frozen flames");
        }

        [Test]
        public void TheNewElementLooksAreWhatTheGamePlays()
        {
            var set = Built();
            Assert.AreEqual("light_marked", AfflictionArt.LookOf(AfflictionKind.Marked));
            Assert.IsNull(SurgeArt.LookOf(ElementSubstance.Ice), "Glaze is GlazeIce's ice on the bones alone (round 4)");
            Assert.Less(set.Find("ice_glaze"), 0, "no crystals at the feet nor frost pool");
            Assert.AreEqual("earth_bedrock", SurgeArt.LookOf(ElementSubstance.Earth));
            foreach (string key in ElementLookBuilder.E4Looks.Keys) Assert.GreaterOrEqual(set.Find(key), 0, key);
            Assert.GreaterOrEqual(set.Find(ElementMoves.SpellKey(ElementSubstance.Wind, false)), 0, "Gust's fronts of curls");
            Assert.GreaterOrEqual(set.Find(ElementMoves.SpellKey(ElementSubstance.Earth, true)), 0, "Stone Rise's stones under the one struck");
            Assert.IsTrue(set.ArmourPlate != null && set.ArmourMaterial != null, "Bedrock's plate for the bones");
            // Marked is a brand on the chest since round 4 (the owner: "I hate this ray of orange light"): no beam, no shaft.
            var marked = PieceNames(set, "light_marked").ToList();
            Assert.IsTrue(marked.Any(name => name.Contains("vfxrd_sigil")), "Marked's sigil");
            Assert.IsFalse(marked.Any(name => name.Contains("vfxe4_beam") || name.Contains("shaft_surge")), "no beam, no slatted shaft");
            // Glaze's shoulder crystals and Bedrock's plates are on the bones (GlazeIce, StoneArmour), never in the look.
            Assert.IsFalse(PieceNames(set, "earth_bedrock").Any(name => name.Contains("vfxe4_rock") || name.Contains("vfxrd_plate")));
            // Seeded is the circling seeds alone, each with its spring-green sprout tip (round 4).
            Assert.IsTrue(PieceNames(set, "life_seeded").All(name => name.Contains("vfxe2_spore") || name.Contains("vfxe2_sprout")), "Seeded: only the circling seeds and their tips");
            Assert.IsTrue(PieceNames(set, "life_seeded").Any(name => name.Contains("vfxe2_sprout")), "Seeded: the sprout tips");
        }

        [Test]
        public void QuickeningHasNoCircleUnderItAndWindthrowKeepsItsPitAndSeeds()
        {
            var set = Built();
            Assert.IsEmpty(ElementLookBuilder.HealPulses, "the owner: \"not sure at all what the green brown circle underneath is trying to mean\"");
            Assert.IsFalse(PieceNames(set, "quickening").Any(name => name.Contains("heal_pulse")));
            var windthrow = PieceNames(set, "seed_storm").ToList();
            Assert.IsTrue(windthrow.Any(name => name.Contains("_pit")), "the torn pit");
            Assert.IsTrue(windthrow.Any(name => name.Contains("vfxe2_seed")), "the seeds in it");
        }

        [Test]
        public void TheAnvilAndTheHeaveRaiseTheBodyAndLetItDown()
        {
            var set = Built();
            foreach (string slug in new[] { "anvil_stone", "frost_heave" })
            {
                var look = set.Looks[set.Find(slug)];
                Assert.Greater(look.RaiseCount, 1, slug);
                float most = 0f;
                for (float t = 0f; t < look.Seconds; t += 0.05f) most = Mathf.Max(most, set.Sample(look.RaiseFirst, look.RaiseCount, t, Vector3.zero).y);
                Assert.Greater(most, 0.3f, $"{slug} lifts the body");
            }
            Assert.AreEqual(0, set.Looks[set.Find("the_grudge")].RaiseCount, "a look that lifts nothing");
        }

        [Test]
        public void NothingTheThirdReviewPaintsIsWhite()
        {
            foreach (var pair in ElementLookBuilder.Paints.Where(p => p.Key.StartsWith("e4_")))
            {
                var c = pair.Value.Colour;
                Assert.IsFalse(c.r > 0.9f && c.g > 0.9f && c.b > 0.9f, pair.Key);
            }
        }

        private static System.Collections.Generic.IEnumerable<string> PieceNames(ElementLookSet set, string key)
        {
            int at = set.Find(key);
            Assert.GreaterOrEqual(at, 0, key);
            var look = set.Looks[at];
            for (int k = 0; k < look.PartCount; k++)
            {
                var mesh = set.Parts[look.FirstPart + k].Mesh;
                if (mesh != null) yield return mesh.name;
            }
        }
    }
}

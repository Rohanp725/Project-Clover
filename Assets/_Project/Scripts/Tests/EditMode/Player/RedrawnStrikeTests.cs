using System.Linq;
using Clover.EditorTools;
using Clover.Player.Forms;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// The infused strikes redrawn from the owner's swing comments (docs/design/feedback/2026-11-21_swing_comments.md),
    /// built from the Blender worker's strikes recipe: all five approved looks, the pieces made once per
    /// enemy struck on the struck body's look and nowhere else, Earth's stones lifting the body, Metal only a glint and
    /// shards, Light's glare warm gold, Dark's hands rimmed violet with the enemy kept in place, Poison dark green, and
    /// nothing painted white.
    /// </summary>
    public class RedrawnStrikeTests
    {
        private const int Earth = 6, Metal = 7, Light = 8, Dark = 9, Poison = 10;

        private static ElementLookSet Built()
        {
            var set = ElementLookBuilder.Set();
            if (set == null || UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(ElementLookBuilder.StrikesRecipePath) == null)
                Assert.Ignore("The strikes recipe is not in the project, or the looks are not built; run Clover > Build > Element Looks.");
            return set;
        }

        private static ElementLookSet.Part[] PartsOf(ElementLookSet set, string key)
        {
            int look = set.Find(key);
            Assert.GreaterOrEqual(look, 0, key);
            var made = set.Looks[look];
            return set.Parts.Skip(made.FirstPart).Take(made.PartCount).ToArray();
        }

        [Test]
        public void EveryRedrawnStrikeIsBuiltAndDarkAndPoisonDrawNothingOnAMiss()
        {
            var set = Built();
            foreach (int element in new[] { Earth, Metal, Light })
                Assert.GreaterOrEqual(set.Find(ElementMoves.RedrawnKey(element, false)), 0, ElementMoves.RedrawnKey(element, false));
            foreach (int element in new[] { Dark, Poison })
            {
                // Every piece of theirs is made per enemy struck: nothing about the player, so a miss shows nothing.
                Assert.Less(set.Find(ElementMoves.RedrawnKey(element, false)), 0, ElementMoves.RedrawnKey(element, false) + " draws on a miss");
                Assert.GreaterOrEqual(set.Find(ElementMoves.RedrawnKey(element, true)), 0, ElementMoves.RedrawnKey(element, true));
            }
            Assert.IsNull(ElementMoves.RedrawnKey(0, false), "Fire keeps its own strike");
            Assert.IsEmpty(ElementLookBuilder.StrikesHeld, "all five are approved");
        }

        [Test]
        public void DarksHandsGripInPlaceWithAVioletRimAndPoisonIsDarkGreen()
        {
            var set = Built();
            var dark = set.Looks[set.Find("e3_strike_dark@b")];
            Assert.AreEqual(0, dark.RaiseCount, "the enemy only strains in place, never lifted or sunk");
            var hands = PartsOf(set, "e3_strike_dark@b").Where(part => part.Mesh.name.Contains("vfxst_hand_big")).ToArray();
            Assert.AreEqual(6, hands.Length, "three hands, each open then gripping");
            foreach (var hand in hands)
            {
                var rim = hand.Material.GetColor("_OutlineColor");
                Assert.Greater(rim.b, 0.6f, $"{hand.Mesh.name}: a pale violet rim");
                Assert.Greater(rim.r, rim.g, $"{hand.Mesh.name}: violet, not blue");
            }
            foreach (var part in PartsOf(set, "e3_strike_poison@b"))
                foreach (var colour in part.Mesh.colors)
                    Assert.IsTrue(colour.g >= colour.r && colour.g >= colour.b && colour.g < 0.3f, $"{part.Mesh.name}: dark green, nothing lime or bright");
        }

        [Test]
        public void WhatIsMadePerEnemyStruckIsOnlyOnTheStruckBody()
        {
            var set = Built();
            // Metal: the shards and sparks ring off each one struck; about the player only the edge's glint and its head.
            var about = PartsOf(set, "e3_strike_metal");
            var struck = PartsOf(set, "e3_strike_metal@b");
            Assert.IsFalse(about.Any(part => part.Mesh.name.Contains("vfxst_shard")), "no shard is drawn on a miss");
            Assert.AreEqual(9, struck.Count(part => part.Mesh.name.Contains("vfxst_shard")), "nine shards off each one struck");
            Assert.AreEqual(1, about.Count(part => part.Mesh.name.Contains("_glinthead")), "the glint at the edge's head");
            Assert.AreEqual(2, about.Count(part => part.Kind == ElementLookSet.Ribbon), "the steel glint along the cut");
            // Light: the flare on each one struck, the glare line about the player.
            Assert.AreEqual(1, PartsOf(set, "e3_strike_light@b").Count(part => part.Mesh.name.Contains("vfxst_flare")));
            Assert.IsFalse(PartsOf(set, "e3_strike_light").Any(part => part.Mesh.name.Contains("vfxst_flare")));
            Assert.AreEqual(2, PartsOf(set, "e3_strike_light").Count(part => part.Kind == ElementLookSet.Ribbon), "the glare and its amber edge");
        }

        [Test]
        public void AStrikeIsMirroredToRunWithABladeSweptTheOtherWay()
        {
            // The recipe cut right to left (anticlockwise from above): a cut left to right is drawn mirrored; a thrust,
            // a shot or a drop that barely turns is drawn as made.
            Assert.IsTrue(ElementMoves.Mirrors(170f));
            Assert.IsFalse(ElementMoves.Mirrors(-170f));
            Assert.IsFalse(ElementMoves.Mirrors(ElementMoves.SweepTrusted * 0.5f));
            foreach (float facing in new[] { -140f, 0f, 90f })
            {
                var side = ElementLooks.MirrorOf(facing);
                var ahead = Quaternion.Euler(0f, facing, 0f) * Vector3.forward;
                Assert.AreEqual(0f, Vector3.Dot(side, ahead), 1e-5f, "the mirror plane runs down the look's own line");
                Assert.AreEqual(1f, side.magnitude, 1e-5f);
                Assert.AreEqual(0f, side.y, 1e-5f);
            }
        }

        [Test]
        public void EarthsStonesRiseOnTheArcAndLiftWhatTheyReach()
        {
            var set = Built();
            var stones = PartsOf(set, "e3_strike_earth").Where(part => part.Mesh.name.Contains("~st_strike_earth") && part.Mesh.name.Contains("stone")).ToArray();
            Assert.AreEqual(7, stones.Length, "seven stones heave up along the cut");
            foreach (var stone in stones)
            {
                float out_ = new Vector2(stone.Position.x, stone.Position.z).magnitude;
                Assert.That(out_, Is.InRange(1.3f, 1.9f), $"{stone.Mesh.name} stands on the arc just past the tip");
            }
            var lift = set.Looks[set.Find("e3_strike_earth@b")];
            Assert.Greater(lift.RaiseCount, 1);
            float most = 0f;
            for (float t = 0f; t < lift.Seconds; t += 0.02f) most = Mathf.Max(most, set.Sample(lift.RaiseFirst, lift.RaiseCount, t, Vector3.zero).y);
            Assert.AreEqual(0.55f, most, 0.05f, "thrown about half a metre up");
            Assert.AreEqual(0f, set.Sample(lift.RaiseFirst, lift.RaiseCount, lift.Seconds, Vector3.zero).y, 0.01f, "and back on the floor");
        }

        [Test]
        public void NothingIsWhiteAndLightsGlareIsWarm()
        {
            var set = Built();
            foreach (string key in new[] { "e3_strike_earth", "e3_strike_metal", "e3_strike_metal@b", "e3_strike_light", "e3_strike_light@b", "e3_strike_dark@b", "e3_strike_poison@b" })
                foreach (var part in PartsOf(set, key))
                    foreach (var colour in part.Mesh.colors)
                        Assert.Less(Mathf.Min(colour.r, Mathf.Min(colour.g, colour.b)), 0.97f, $"{part.Mesh.name} is painted white");
            foreach (var glare in PartsOf(set, "e3_strike_light").Where(part => part.Kind == ElementLookSet.Ribbon))
            {
                var colour = glare.Mesh.colors[0];
                Assert.Greater(colour.r, colour.b + 0.5f, $"{glare.Mesh.name} is gold or amber, not white");
            }
        }
    }
}

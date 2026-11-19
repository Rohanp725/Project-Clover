using System.Linq;
using Clover.EditorTools;
using Clover.Gameplay;
using Clover.Player.Forms;
using NUnit.Framework;
using UnityEngine;
using Cue = Clover.Player.Forms.ReactionMotion.Cue;
using M = Clover.Player.Forms.ReactionMotion;

namespace Clover.Tests
{
    /// <summary>
    /// The reactions' motion: the verbs' numbers (a fall ends where it lands and speeds up on the way, a sweep reaches
    /// its far end, a spread grows, a strobe goes on and off, a rise overshoots, drift rises and thins, a flash is only
    /// where it lands), the roles read from the recipe's names, the pivots the builder moves parts about, and that every
    /// one of the 55 reactions has a timeline with the cause arriving and an impact.
    /// </summary>
    public class ReactionMotionTests
    {
        private static M.Pose At(in Cue cue, float age, out int state)
        {
            state = M.Evaluate(cue, age, 3, out var pose);
            return pose;
        }

        [Test]
        public void AFallEndsWhereItLandsAndSpeedsUpOnTheWay()
        {
            var cue = Cue.Of(M.Fall, M.Arrive, 0.1f, 0.2f, 0.6f, 5f, 0, true);
            At(cue, 0.05f, out int state);
            Assert.AreEqual(M.Waiting, state, "not before its delay");
            Assert.AreEqual(1f, At(cue, 0.1f, out state).Travel, 1e-4f, "it starts at the top of its path");
            Assert.AreEqual(M.Showing, state);
            float quarter = At(cue, 0.15f, out _).Travel, half = At(cue, 0.2f, out _).Travel, threeQuarters = At(cue, 0.25f, out _).Travel;
            Assert.Greater(half - threeQuarters, quarter - half, "it covers more ground each moment: it accelerates");
            Assert.AreEqual(0f, At(cue, 0.3f, out _).Travel, 1e-4f, "it ends on its target");
            Assert.AreEqual(0f, At(cue, 0.5f, out _).Travel, 1e-4f, "and stays there");
            Assert.Greater(At(cue, 0.2f, out _).Along, 1.2f, "drawn long while it falls");
            At(cue, 0.71f, out state);
            Assert.AreEqual(M.Over, state, "gone after its seconds");
        }

        [Test]
        public void AFlashIsOnlyWhereItLands()
        {
            var fall = Cue.Of(M.Fall, M.Arrive, 0f, 0.2f, 0.6f, 5f, 0, true);
            Assert.AreEqual(0f, At(fall, 0.1f, out _).Hot, "not while falling");
            Assert.AreEqual(1f, At(fall, 0.21f, out _).Hot, "white the moment it lands");
            Assert.AreEqual(0f, At(fall, 0.21f + M.FlashSeconds + 0.01f, out _).Hot, "and only a moment");
            fall.Flash = false;
            Assert.AreEqual(0f, At(fall, 0.21f, out _).Hot, "never for one that does not flash");
            var pulse = Cue.Of(M.Pulse, M.Impact, 0.3f, 0.08f, 0.8f, 3f, 0, true);
            Assert.AreEqual(1f, At(pulse, 0.31f, out _).Hot, "a clang flashes as it rings out");
        }

        [Test]
        public void ASweepReachesTheFarEnd()
        {
            var cue = Cue.Of(M.Sweep, M.Arrive, 0f, 0.3f, 0.75f, 2.5f);
            float last = 2f;
            for (float age = 0f; age <= 0.3f; age += 0.03f)
            {
                float travel = At(cue, age, out _).Travel;
                Assert.LessOrEqual(travel, last, $"always onward ({age:0.00} s)");
                last = travel;
            }
            Assert.AreEqual(1f, At(cue, 0f, out _).Travel, 1e-4f, "it starts at the player's end of its path");
            Assert.AreEqual(0f, At(cue, 0.3f, out _).Travel, 1e-4f, "and reaches the far end");
            Assert.Greater(At(cue, 0.05f, out _).Along, 1.5f, "stretched along its way while fast");
            Assert.AreEqual(1f, At(cue, 0.4f, out _).Along, 1e-4f, "whole once there");
        }

        [Test]
        public void ASpreadGrowsFromItsMiddle()
        {
            var cue = Cue.Of(M.Spread, M.Aftermath, 0.2f, 0.4f, 1.2f, 0.3f);
            var first = At(cue, 0.2f, out _);
            Assert.AreEqual(0.3f, first.Across, 1e-3f, "it starts at its first share");
            Assert.AreEqual(0f, first.Reveal, 1e-4f, "with nothing drawn yet");
            float last = 0f;
            for (float age = 0.2f; age <= 0.6f; age += 0.04f)
            {
                float across = At(cue, age, out _).Across;
                Assert.GreaterOrEqual(across, last, $"never shrinking while it spreads ({age:0.00} s)");
                last = across;
            }
            var spread = At(cue, 0.6f, out _);
            Assert.AreEqual(1f, spread.Across, 1e-3f, "whole when its motion ends");
            Assert.AreEqual(1f, spread.Reveal, 1e-4f, "and drawn in full");
            Assert.Greater(At(cue, 1f, out _).Across, 1f, "then still creeping");
        }

        [Test]
        public void AStrobeGoesOnAndOffThenHolds()
        {
            var cue = Cue.Of(M.Flicker, M.Arrive, 0f, 0.3f, 0.8f, 0f, 3);
            int lit = 0, dark = 0, mostRedraw = 0;
            for (float age = 0f; age < 0.3f; age += 0.005f)
            {
                var pose = At(cue, age, out _);
                if (pose.Shown) lit++; else dark++;
                mostRedraw = Mathf.Max(mostRedraw, pose.Redraw);
            }
            Assert.Greater(lit, 0);
            Assert.Greater(dark, 0, "dark frames between the redraws");
            Assert.AreEqual(3, mostRedraw, "redrawn as many times as asked");
            Assert.IsTrue(M.Strobe(0f, 0.3f, 3, out int redraw) && redraw == 0, "lit on its first frame");
            // A strobe on a bolt that falls: dark frames on the way down, steady once it lands.
            var bolt = Cue.Of(M.Fall, M.Arrive, 0f, 0.2f, 0.6f, 5f, 2);
            Assert.IsTrue(Enumerable.Range(0, 40).Any(i => !At(bolt, i * 0.005f, out _).Shown), "a falling bolt strobes");
            Assert.IsTrue(At(bolt, 0.25f, out _).Shown, "and holds once landed");
        }

        [Test]
        public void ARiseOvershootsAndSettles()
        {
            var cue = Cue.Of(M.Rise, M.Impact, 0f, 0.25f, 0.9f);
            Assert.Less(At(cue, 0f, out _).Along, 0.1f, "out of the ground");
            float most = 0f;
            for (float age = 0f; age <= 0.25f; age += 0.01f) most = Mathf.Max(most, At(cue, age, out _).Along);
            Assert.Greater(most, 1.05f, "past its height");
            Assert.AreEqual(1f, At(cue, 0.25f, out _).Along, 1e-3f, "and back");
            var sink = Cue.Of(M.Sink, M.Arrive, 0f, 0.4f, 1f, 0.45f);
            Assert.AreEqual(0.45f, At(sink, 0f, out _).Lift, 1e-4f, "a sink starts held up");
            Assert.AreEqual(0f, At(sink, 0.4f, out _).Lift, 1e-4f, "and drops into place");
            Assert.Greater(At(sink, 0f, out _).Across, 1.1f, "spread wide before it tips in");
        }

        [Test]
        public void DriftRisesAndThinsAndFlameNeverHoldsStill()
        {
            var steam = Cue.Of(M.Drift, M.Aftermath, 0f, 0.15f, 1f, 1.2f);
            Assert.AreEqual(0f, At(steam, 0f, out _).Lift, 1e-4f);
            Assert.Greater(At(steam, 0.9f, out _).Lift, 1f, "it has risen near its height");
            Assert.Less(At(steam, 0.9f, out _).Left, 0.5f, "and thinned");
            var flame = Cue.Of(M.Lick, M.Impact, 0f, 0.18f, 0.9f);
            float low = 9f, high = 0f;
            for (float age = 0.2f; age < 0.6f; age += 0.01f)
            {
                float along = At(flame, age, out _).Along;
                low = Mathf.Min(low, along);
                high = Mathf.Max(high, along);
            }
            Assert.Greater(high - low, 0.15f, "a tongue grows and falls back while it burns");
            var spin = Cue.Of(M.Spin, M.Arrive, 0f, 0.3f, 0.9f, -200f);
            Assert.Less(At(spin, 0.6f, out _).Spin, -100f, "a swirl turns the way it is given");
            Assert.Greater(At(spin, 0f, out _).Size, 1.3f, "drawn in from wider");
            var burst = Cue.Of(M.Burst, M.Impact, 0f, 0.2f, 0.7f);
            Assert.Less(At(burst, 0f, out _).Size, 0.1f, "a burst starts gathered at its middle");
            Assert.GreaterOrEqual(At(burst, 0.2f, out _).Size, 1f, "and flies out to its place");
        }

        [Test]
        public void PartsGoAtTheEndOfTheirTime()
        {
            Assert.AreEqual(1f, M.Left(0.5f, 1f));
            Assert.AreEqual(0f, M.Left(1f, 1f), 1e-4f);
            Assert.Greater(M.Left(0.8f, 1f), 0f);
            Assert.Less(M.Left(0.8f, 1f), 1f);
        }

        [Test]
        public void RolesAreReadFromTheRecipesNames()
        {
            Assert.AreEqual(ReactionTimelines.Strike, ReactionTimelines.RoleOf("vfx2c_light_lightning_truebolt"));
            Assert.AreEqual(ReactionTimelines.Strike, ReactionTimelines.RoleOf("vfx2_light_pencil_affliction"));
            Assert.AreEqual(ReactionTimelines.Flame, ReactionTimelines.RoleOf("vfx3c_vfx_fire_tongue_a"));
            Assert.AreEqual(ReactionTimelines.Vapour, ReactionTimelines.RoleOf("vfx3c_vfx_combo_steam"));
            Assert.AreEqual(ReactionTimelines.Swirl, ReactionTimelines.RoleOf("vfx3c_vfx_combo_mite"), "the combo prefix is not Wind's comb");
            Assert.AreEqual(ReactionTimelines.Growth, ReactionTimelines.RoleOf("vfx3c_vfx_combo_hotstone"));
            Assert.AreEqual(ReactionTimelines.Debris, ReactionTimelines.RoleOf("vfx3c_vfx_ice_chip"));
            Assert.AreEqual(ReactionTimelines.Wave, ReactionTimelines.RoleOf("vfx2_water_aftersplash_hit"));
            Assert.AreEqual(ReactionTimelines.Wind, ReactionTimelines.RoleOf("vfx3c_vfx_wind_streak"));
            Assert.AreEqual(ReactionTimelines.Swirl, ReactionTimelines.RoleOf("vfx2_dark_streaksair_hit"), "Dark's streaks are drawn in, not blown");
            Assert.AreEqual(ReactionTimelines.Ring, ReactionTimelines.RoleOf("vfx2_metal_clang_hit"));
            Assert.AreEqual(ReactionTimelines.Static, ReactionTimelines.RoleOf("vfx2c_poison_lightning_nerveruyi"), "not a fume");
            Assert.AreEqual(ReactionTimelines.Ray, ReactionTimelines.RoleOf("vfx2c_dark_ice_breath"));
            Assert.AreEqual(ReactionTimelines.Still, ReactionTimelines.RoleOf("something_else"));
            Assert.AreEqual("mite", ReactionTimelines.Plain("vfx3c_vfx_combo_mite"));
        }

        [Test]
        public void APieceFollowsItsWordThenItsRoleThenTheDefault()
        {
            int step = ReactionTimelines.StepFor("earth_fire", "vfx3c_vfx_combo_hotstone", ReactionTimelines.Growth, out var stone);
            Assert.GreaterOrEqual(step, 0);
            Assert.AreEqual(M.Fall, stone.Cue.Verb, "Burnt Mound's stones fall back hot");
            ReactionTimelines.StepFor("earth_fire", "vfx2_earth_heave_affliction", ReactionTimelines.Growth, out var heave);
            Assert.AreEqual(M.Rise, heave.Cue.Verb, "its heave ring rises");
            ReactionTimelines.StepFor("fire_lightning", "vfx3c_vfx_fire_tongue_b", ReactionTimelines.Flame, out var tongue);
            Assert.AreEqual(M.Lick, tongue.Cue.Verb);
            Assert.Greater(tongue.Stagger, 0f, "Flashover's tongues climb one after another");
            Assert.AreEqual(-1, ReactionTimelines.StepFor("not_a_look", "vfx3c_vfx_fire_tongue_b", ReactionTimelines.Flame, out var fallback));
            Assert.AreEqual(ReactionTimelines.Default(ReactionTimelines.Flame).Verb, fallback.Cue.Verb);
        }

        [Test]
        public void EveryReactionHasATimelineWithAnArrivalAndAnImpact()
        {
            CollectionAssert.AreEquivalent(ReactionDesigns.Designed, ReactionTimelines.Designed.Select(timeline => timeline.Key), "one timeline for each designed look");
            foreach (var timeline in ReactionTimelines.Designed)
            {
                Assert.IsTrue(ReactionTimelines.HasArrivalAndImpact(timeline.Steps), timeline.Key);
                Assert.IsFalse(string.IsNullOrEmpty(timeline.Story), timeline.Key);
                float end = timeline.Steps.Max(step => step.Cue.End);
                Assert.That(end, Is.InRange(0.6f, 1.6f), $"{timeline.Key} plays in about a second");
                foreach (var step in timeline.Steps)
                {
                    Assert.Greater(step.Cue.Seconds, step.Cue.Move, $"{timeline.Key} {step.Match}: it holds a while after it moves");
                    Assert.Less(step.Cue.Verb, M.VerbNames.Length, $"{timeline.Key} {step.Match}");
                }
            }
            int composed = 0;
            for (int pair = 0; pair < AfflictionPairs.Count; pair++)
            {
                if (ReactionDesigns.IsDesigned(pair)) continue;
                composed++;
                ReactionTimelines.Composed(pair, out var first, out var second, out var mark);
                string name = AfflictionPairs.Name(pair);
                Assert.AreEqual(M.Arrive, first.Beat, name);
                Assert.AreEqual(0f, first.Delay, name);
                Assert.AreEqual(M.Impact, second.Beat, name);
                Assert.Greater(second.Delay, 0.1f, $"{name}: the second parent answers once the first has arrived");
                Assert.AreEqual(M.Spread, mark.Verb, name);
                Assert.Greater(mark.Delay, second.Delay, $"{name}: the ground mark spreads after the impact");
                Assert.That(Mathf.Max(first.End, second.End, mark.End), Is.InRange(0.6f, 1.6f), name);
            }
            Assert.AreEqual(31, composed);
            // Each element arrives by its nature.
            Assert.AreEqual(M.Fall, ReactionTimelines.Nature(ReactionDesigns.RingIndex("lightning")).Verb);
            Assert.AreEqual(M.Sweep, ReactionTimelines.Nature(ReactionDesigns.RingIndex("wind")).Verb);
            Assert.AreEqual(M.Lick, ReactionTimelines.Nature(ReactionDesigns.RingIndex("fire")).Verb);
            Assert.AreEqual(M.Rise, ReactionTimelines.Nature(ReactionDesigns.RingIndex("earth")).Verb);
            Assert.AreEqual(M.Pulse, ReactionTimelines.Nature(ReactionDesigns.RingIndex("metal")).Verb);
        }

        [Test]
        public void PartsMoveAboutTheRightPoint()
        {
            // A bolt standing over the body from 0.5 to 3 m, drawn down: about its top, along -Y.
            var bolt = new[] { new Vector3(-0.1f, 0.5f, 0f), new Vector3(0.1f, 3f, 0f), new Vector3(0f, 1.7f, 0.05f) };
            var top = ReactionArtBuilder.PivotOf(M.Extend, ReactionTimelines.Strike, bolt, out var down);
            Assert.AreEqual(3f, top.y, 1e-4f);
            Assert.AreEqual(Vector3.down, down);
            // What rises does so from its base.
            var bottom = ReactionArtBuilder.PivotOf(M.Rise, ReactionTimelines.Growth, bolt, out var none);
            Assert.AreEqual(0.5f, bottom.y, 1e-4f);
            Assert.AreEqual(Vector3.zero, none);
            // Chips round the body burst from the body's axis.
            var chips = new[] { new Vector3(-1f, 1f, 0f), new Vector3(1f, 1.4f, 0.5f), new Vector3(0.2f, 2f, -1f) };
            var middle = ReactionArtBuilder.PivotOf(M.Burst, ReactionTimelines.Debris, chips, out _);
            Assert.AreEqual(0f, middle.x, 1e-4f);
            Assert.AreEqual(0f, middle.z, 1e-4f);
            // A beam thrown from the body to the next is drawn out from the end nearest the body.
            var beam = new[] { new Vector3(0.3f, 1f, -0.3f), new Vector3(2.5f, 1f, -3.5f), new Vector3(1.4f, 1.2f, -2f) };
            var root = ReactionArtBuilder.PivotOf(M.Extend, ReactionTimelines.Ray, beam, out var way);
            Assert.AreEqual(beam[0], root);
            Assert.Greater(way.x, 0f);
            Assert.Less(way.z, 0f);
        }
    }
}

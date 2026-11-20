using System.IO;
using Clover.EditorTools;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;
using Cue = Clover.Player.Forms.ReactionMotion.Cue;
using M = Clover.Player.Forms.ReactionMotion;

namespace Clover.Tests
{
    /// <summary>
    /// The owner's second review of the effects (docs/design/feedback/2026-11-20_effects_review_2.md), the rules that can
    /// be checked without a scene: the new motion verbs' numbers and their names as the recipes write them, a crackle
    /// that never flashes, Seized without its rings, Rust in rust, Hunger with no purple light, Regrowth pink and green,
    /// Temper's steel on every outfit, Overdrive, Tremor and Cursed without the pieces the owner turned down, Flashover's
    /// floor without orange, the body's real flame, and wind with nothing white in it.
    /// </summary>
    public class EffectsReviewTwoTests
    {
        private static M.Pose At(in Cue cue, float age, out int state)
        {
            state = M.Evaluate(cue, age, 3, out var pose);
            return pose;
        }

        // ---- The new verbs (for the Blender worker's recipes).

        [Test]
        public void TheRecipesNameTheNewVerbsExactly()
        {
            Assert.AreEqual(M.Orbit, M.VerbOf("orbit"));
            Assert.AreEqual(M.Spiral, M.VerbOf("spiral"));
            Assert.AreEqual(M.RibbonDraw, M.VerbOf("ribbon_draw"));
            Assert.AreEqual(M.Crackle, M.VerbOf("crackle"));
            Assert.AreEqual(M.Pop, M.VerbOf("no such verb"));
            Assert.AreEqual(M.VerbNames.Length, M.Crackle + 1, "every verb has its name");
        }

        [Test]
        public void AnOrbitGoesRoundItsCentreAtItsRadiusAndRate()
        {
            var cue = Cue.OfOrbit(M.Impact, 0.2f, 2f, 0.65f, 0.5f, 90f);
            Assert.AreEqual(0.65f, At(cue, 0.2f, out _).Offset.magnitude, 1e-4f, "it starts on its circle");
            Vector3 start = At(cue, 0.2f, out _).Offset;
            Assert.AreEqual(0.65f, start.x, 1e-4f, "at its phase: 90 degrees is +x");
            Vector3 half = At(cue, 0.2f + 1f, out _).Offset;
            Assert.AreEqual(-0.65f, half.x, 1e-3f, "half a turn on after a second at half a turn a second");
            for (float t = 0.2f; t < 2.2f; t += 0.1f) Assert.AreEqual(0.65f, At(cue, t, out _).Offset.magnitude, 1e-3f, "always on its circle");
            Assert.AreEqual(0f, At(cue, 1f, out _).Lift, "an orbit does not climb");
            Assert.AreEqual(M.AngleAt(cue, 0.5f) + 90f, At(cue, 0.7f, out _).Spin, 1e-3f, "it faces along its way round");
            var back = Cue.OfOrbit(M.Impact, 0f, 2f, 1f, -1f);
            Assert.Less(At(back, 0.25f, out _).Offset.x, 0f, "a negative rate goes round the other way");
        }

        [Test]
        public void ASpiralClimbsAndWidensWhileItOrbits()
        {
            var cue = Cue.OfSpiral(M.Impact, 0f, 2f, 0.4f, 1f, 1.5f, 0.3f);
            var at = At(cue, 1f, out _);
            Assert.AreEqual(1.5f, at.Lift, 1e-4f, "climbs its rise a second");
            Assert.AreEqual(0.7f, at.Offset.magnitude, 1e-4f, "radius 0.4 plus 0.3 a second");
            var closing = Cue.OfSpiral(M.Impact, 0f, 3f, 0.5f, 1f, 0f, -0.4f);
            Assert.AreEqual(0f, At(closing, 2f, out _).Offset.magnitude, 1e-4f, "a closing spiral stops at its centre, never past it");
        }

        [Test]
        public void ARibbonDrawsOnHeadFirstAndErasesTailFirst()
        {
            var cue = Cue.OfRibbon(M.Impact, 0f, 1.2f, 0.3f, 0.45f);
            Assert.AreEqual(0f, At(cue, 0f, out _).Head, 1e-4f, "nothing drawn at its start");
            Assert.AreEqual(0.5f, At(cue, 0.15f, out _).Head, 1e-4f, "half drawn half way through its draw");
            Assert.AreEqual(0f, At(cue, 0.15f, out _).Tail, 1e-4f, "nothing erased while it draws on");
            Assert.AreEqual(1f, At(cue, 0.5f, out _).Head, 1e-4f);
            Assert.AreEqual(0f, At(cue, 0.75f, out _).Tail, 1e-4f, "its erase begins at its last 0.45 s");
            Assert.AreEqual(0.5f, At(cue, 0.975f, out _).Tail, 1e-3f);
            Assert.AreEqual(1f, At(cue, 0.5f, out _).Left, "it never fades: the erase is how it goes");
            var round = Cue.OfRibbon(M.Impact, 0f, 1f, 0.3f, 0.3f, 0.6f, 0.8f);
            Assert.AreEqual(0.6f, At(round, 0.4f, out _).Offset.magnitude, 1e-4f, "a ribbon given a way round orbits while it draws");
            Assert.AreEqual(Vector3.zero, At(cue, 0.4f, out _).Offset, "one without stays put");
        }

        [Test]
        public void TheNewVerbsAllocateNothing()
        {
            var cues = new[] { Cue.OfOrbit(M.Impact, 0f, 2f, 1f, 1f), Cue.OfSpiral(M.Impact, 0f, 2f, 1f, 1f, 1f, 0.2f), Cue.OfRibbon(M.Impact, 0f, 2f, 0.3f, 0.4f, 0.5f, 1f), Cue.OfCrackle(M.Impact, 0f, 0.1f, 2f, 70f) };
            float sum = 0f;
            for (int warm = 0; warm < 4; warm++) M.Evaluate(cues[warm], 0.5f, warm, out _);
            long before = System.GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 4000; i++)
            {
                M.Evaluate(cues[i & 3], (i % 200) * 0.01f, i, out var pose);
                sum += pose.Offset.x + pose.Head + pose.Lift;
            }
            long after = System.GC.GetAllocatedBytesForCurrentThread();
            Assert.AreEqual(0, after - before, "bytes allocated by 4000 evaluations");
            Assert.IsFalse(float.IsNaN(sum));
        }

        // ---- Seized: no rings, a crackle that does not flash.

        [Test]
        public void ACrackleIsNeverDarkAndRedrawsAtMostThreeTimesASecond()
        {
            var cue = Cue.OfCrackle(M.Arrive, 0f, 0.14f, 1.35f, 72f);
            int redraws = 0, last = -1;
            for (float t = 0f; t < cue.Seconds; t += 1f / 120f)
            {
                var pose = At(cue, t, out int state);
                if (state != M.Showing) continue;
                Assert.IsTrue(pose.Shown, $"never dark (at {t:0.000} s)");
                Assert.AreEqual(0f, pose.Hot, $"never a white frame (at {t:0.000} s)");
                if (pose.Redraw != last) redraws++;
                last = pose.Redraw;
            }
            Assert.LessOrEqual(redraws, Mathf.CeilToInt(cue.Seconds * M.CrackleRate) + 1, "redrawn at no more than the cap");
            Assert.LessOrEqual(M.CrackleRate, 3f, "the cap is three a second, well under the 3 Hz flash threshold's danger band of fast strobes");
        }

        [Test]
        public void SeizedIsTheLightningAloneAndItCracklesCalmly()
        {
            int pair = ReactionDesigns.Seized;
            Assert.AreEqual("Seized", AfflictionPairs.Name(pair));
            ReactionDesigns.Composed(pair, out string first, out string second, out string mark, out _);
            string[] shown = { first, second, mark };
            foreach (string piece in shown) Assert.AreNotEqual("water_aftersplash_hit", piece, "the water's rings are gone");
            Assert.Contains("lightning_crawl_affliction", shown, "the lightning stays");
            ReactionTimelines.Composed(pair, out var arrives, out var answers, out _);
            var lightning = first == "lightning_crawl_affliction" ? arrives : answers;
            Assert.AreEqual(M.Crackle, lightning.Verb, "a crackle, not a strobing fall");
            Assert.AreEqual(0, lightning.Strobes);
            Assert.IsFalse(lightning.Flash, "no white frame");
        }

        // ---- Rust, Hunger, Regrowth: colours in data.

        [Test]
        public void RustsMetalIsRustColoured()
        {
            int pair = ReactionDesigns.Rust;
            Assert.AreEqual("Rust", AfflictionPairs.Name(pair));
            Color rust = ReactionDesigns.RustColour;
            Color.RGBToHSV(rust, out float hue, out float saturation, out float value);
            Assert.That(hue * 360f, Is.InRange(12f, 32f), "an orange-brown hue");
            Assert.Greater(saturation, 0.7f, "strongly coloured, not grey");
            Assert.That(value, Is.InRange(0.45f, 0.75f), "darker than bright orange");
            ReactionDesigns.Composed(pair, out string first, out string second, out string mark, out _);
            string[] pieces = { first, second, mark };
            for (int part = 0; part < 3; part++)
                if (pieces[part] != null) Assert.AreEqual(rust, ReactionDesigns.ComposedTint(pair, part), $"{pieces[part]} is drawn in rust");
            Assert.AreEqual(Color.white, ReactionDesigns.ComposedTint(ReactionDesigns.Seized, 0), "other looks keep their own colours");
        }

        [Test]
        public void HungerLightsNoPurpleLight()
        {
            Assert.AreEqual(0f, SurgeArt.Glow(Elements.ByName(Elements.Dark)), "darkness gives off no light: no bright purple disc in the middle");
            foreach (var info in Elements.All)
                if (info.Name != Elements.Dark) Assert.Greater(SurgeArt.Glow(info), 0f, info.Name);
            Assert.Less(SurgeArt.Glow(Elements.ByName(Elements.Lightning)), 0.5f, "Overdrive's light is kept low: no ring of it on the floor");
            var eclipse = AssetDatabase.LoadAssetAtPath<Material>(ElementArtBuilder.Folder + "/m_decal_dark_eclipse_surge.mat");
            if (eclipse == null) Assert.Ignore("The element art has not been built.");
            // The decal's colours go to the screen as they are, in linear: what is seen is their sRGB.
            Color middle = eclipse.GetColor("_ColorR") * SurgeArt.HungerHeart;
            Color seen = middle.gamma;
            Assert.Less(Mathf.Max(seen.r, Mathf.Max(seen.g, seen.b)), 0.16f, $"Hunger's middle is near black on screen ({seen})");
            Assert.Greater(middle.b, middle.g, "and purple, not grey");
        }

        [Test]
        public void RegrowthIsPinkAndGreenNotWhiteAndGrey()
        {
            Color.RGBToHSV(SurgeLook.Petal, out float petalHue, out float petalSaturation, out _);
            Assert.That(petalHue * 360f, Is.InRange(320f, 350f), "pink");
            Assert.Greater(petalSaturation, 0.5f, "a true pink, not white");
            Color.RGBToHSV(SurgeLook.Leaf, out float leafHue, out float leafSaturation, out _);
            Assert.That(leafHue * 360f, Is.InRange(90f, 140f), "green");
            Assert.Greater(leafSaturation, 0.5f);
            Assert.LessOrEqual(ElementSubstance.Sheen(FormMesh.Flower, SurgeLook.Petal), 0.15f, "petals keep their colour in the light");
        }

        // ---- Temper: steel on every outfit.

        [Test]
        public void TempersSteelComesOnAndGoesAndEveryOutfitCanTakeIt()
        {
            var toon = Shader.Find("Clover/Toon");
            Assert.IsNotNull(toon);
            Assert.GreaterOrEqual(toon.FindPropertyIndex("_Metal"), 0, "the body's shader has the steel");
            var outfits = AssetDatabase.LoadAssetAtPath<OutfitSet>("Assets/_Project/Generated/Player/Outfits.asset");
            if (outfits == null) Assert.Ignore("The outfits have not been built.");
            foreach (var outfit in outfits.All)
                foreach (var material in outfit.Materials)
                    Assert.AreEqual(toon, material.shader, $"{outfit.Id}: {material.name} must be Clover/Toon to take Temper's steel");

            var sheen = new MetalSheen();
            for (int frame = 0; frame < 60; frame++) sheen.Tick(true, 1f / 60f);
            Assert.AreEqual(1f, sheen.Level, 1e-4f, "all steel within a second: five bands from the feet up, the last at 0.69 s");
            var early = new MetalSheen();
            for (int frame = 0; frame < 24; frame++) early.Tick(true, 1f / 60f);
            Assert.That(early.Level, Is.InRange(0.01f, 0.5f), "at 0.4 s only the lower bands are steel: it climbs, it does not pop on");
            long before = System.GC.GetAllocatedBytesForCurrentThread();
            for (int frame = 0; frame < 600; frame++) sheen.Tick(frame < 300, 1f / 60f);
            Assert.AreEqual(0, System.GC.GetAllocatedBytesForCurrentThread() - before, "nothing allocated a frame");
            Assert.AreEqual(0f, sheen.Level, "all gone once the surge ends");
        }

        // ---- The pieces the owner turned down are not drawn.

        [Test]
        public void TheTurnedDownPiecesAreGone()
        {
            string surge = File.ReadAllText("Assets/_Project/Scripts/Runtime/Player/Forms/SurgeArt.cs");
            string affliction = File.ReadAllText("Assets/_Project/Scripts/Runtime/Player/Forms/AfflictionArt.cs");
            StringAssert.DoesNotContain("\"lightning_drums_surge\"", surge, "Overdrive: no ring of drums");
            StringAssert.DoesNotContain("\"metal_crownblade_surge\"", surge, "Temper: no blades round the player");
            StringAssert.DoesNotContain("\"metal_meander_surge\"", surge, "Temper: no seam on the floor");
            StringAssert.DoesNotContain("\"earth_heave_affliction\"", affliction, "Tremor: no flashing ring of stones");
            StringAssert.DoesNotContain("\"dark_inkring_affliction", affliction, "Cursed: no purple marks dropping");
            StringAssert.Contains("\"lightning_jump_charged\"", affliction, "Static: bolts jump off the body");
            Assert.IsTrue(SurgeArt.ReplacesBuilt(Elements.ByName(Elements.Lightning)), "Overdrive's old wobbling bolts are not drawn under the arcs");
            Assert.IsTrue(SurgeArt.DrawnWithoutHolding(ElementSubstance.Metal) && SurgeArt.DrawnWithoutHolding(ElementSubstance.Lightning));
        }

        [Test]
        public void SeizedsHoldAndFilmAreDrawnByItsOwnLookAlone()
        {
            // No ring at the hold, no bolt from the sky with a white ring and a sky flash at each of the film's bites.
            string three = File.ReadAllText("Assets/_Project/Scripts/Runtime/Player/Trees/Reactions/Reactions.Three.cs");
            StringAssert.Contains("Do = UpgradeAction.Hold, Seconds = 1f, Unseen = true", three);
            StringAssert.Contains("Radius = 2.5f, Seconds = 3f, Interval = 0.5f, Affliction = AfflictionKind.Static, Stacks = 1, Unseen = true", three);
            string upgrades = File.ReadAllText("Assets/_Project/Scripts/Runtime/Player/PlayerUpgrades.cs");
            StringAssert.Contains("Quiet = effect.Unseen", upgrades, "unseen ground is quiet");
            string hazards = File.ReadAllText("Assets/_Project/Scripts/Runtime/World/Hazards.cs");
            StringAssert.Contains("if (zone.Spec.Quiet) { }", hazards, "quiet ground drops no bolt at its bites");
        }

        [Test]
        public void StaticNeverBlinksDarkBetweenItsBeats()
        {
            for (int depth = 1; depth <= 5; depth++)
                Assert.Greater(AfflictionArt.StaticHold(depth), AfflictionLook.Interval(depth), $"depth {depth}: each crawl is held past the next");
        }

        [Test]
        public void CursedIsDrawnInNearBlack()
        {
            foreach (var ink in new[] { AfflictionArt.CurseInk, AfflictionArt.CurseDeep })
                Assert.Less(Mathf.Max(ink.r, Mathf.Max(ink.g, ink.b)), 0.2f);
        }

        // ---- Flashover: no orange left on its floor.

        [Test]
        public void FlashoversScorchHasNoOrange()
        {
            var floor = AssetDatabase.LoadAssetAtPath<Material>(ReactionArtBuilder.Folder + "/m_reaction_decal_fire_lightning_0.mat");
            if (floor == null) Assert.Ignore("The reaction art has not been built.");
            foreach (string slot in new[] { "_ColorR", "_ColorG", "_ColorB", "_ColorA" })
            {
                Color colour = floor.GetColor(slot);
                if (colour.a < 0.05f || Mathf.Max(colour.r, Mathf.Max(colour.g, colour.b)) < 0.2f) continue;
                Color.RGBToHSV(colour, out float hue, out _, out _);
                Assert.IsFalse(hue * 360f < 50f || hue * 360f > 345f, $"{slot} {colour} is fire-coloured; Flashover's fire is all violet");
            }
        }

        // ---- The body's real flame.

        [Test]
        public void ABurningBodyBurnsInRealFlameThatGrowsWithTheBurn()
        {
            for (int depth = 1; depth < 5; depth++) Assert.LessOrEqual(BodyFlames.HeatOf(depth), BodyFlames.HeatOf(depth + 1));
            Assert.Greater(BodyFlames.HeatOf(1), 0.5f, "one stack still burns");
            var mesh = FlameField.Body(BodyFlames.Tongues, 31);
            Assert.AreEqual(BodyFlames.Tongues * 4, mesh.vertexCount, "four corners a tongue");
            foreach (var foot in mesh.vertices)
            {
                Assert.That(foot.y, Is.InRange(0f, 1f), "rooted on the body, shins to shoulders");
                Assert.LessOrEqual(new Vector2(foot.x, foot.z).magnitude, 1.06f, "round the body, not out on the floor");
            }
            Object.DestroyImmediate(mesh);
            var set = ElementArtBuilder.Set();
            if (set == null) Assert.Ignore("The element art has not been built.");
            Assert.IsNotNull(set.BodyFlame, "the art carries the body's flame");
            Assert.AreEqual("Clover/FlameField", set.BodyFlame.shader.name, "the burning ground's real fire, in fire colours");
        }

        // ---- Wind: nothing white.

        [Test]
        public void WindHasNothingWhiteInIt()
        {
            Assert.Less(ElementSubstance.WindDust.gamma.maxColorComponent, 0.85f, "pale dust on screen, not white");
            Assert.IsFalse(ReactionTimelines.Flashes("wind"), "wind never lands with a white frame");
            Assert.IsFalse(ReactionTimelines.Nature(ReactionDesigns.RingIndex("wind")).Flash);
            var wind = AssetDatabase.LoadAssetAtPath<Material>(ElementArtBuilder.Folder + "/m_element_solid_wind.mat");
            if (wind == null) Assert.Ignore("The element art has not been built.");
            Assert.AreEqual(0f, wind.GetFloat("_Sheen"), "no white sheen on wind's faces");
            // Clover/Spell's lit tone is the colour a quarter brighter plus the sheen: pale dust's must stay under white.
            Color body = wind.GetColor("_Color");
            // Linear 0.69 is 85% on screen.
            Assert.Less(0.84f * body.g * 1.25f + wind.GetFloat("_Sheen"), 0.72f, "wind's brightest tone stays under white");
            Assert.AreEqual(0f, ElementSubstance.Sheen(FormMesh.Gust, ElementSubstance.WindDust), "moving air never shines white");
            Assert.Less((ElementSubstance.WindDust * 1.25f).gamma.maxColorComponent, 0.9f, "pale dust lit is still under 90% on screen");
            string tap = File.ReadAllText("Assets/_Project/Scripts/Runtime/Player/Forms/TapForm.cs");
            StringAssert.DoesNotContain("% 3 == 0 ? Color.white : ElementSubstance.Pale", tap, "no white gust every third cast");
        }
    }
}

using System.Linq;
using Clover.EditorTools;
using Clover.Gameplay;
using Clover.Player;
using Clover.Player.Forms;
using NUnit.Framework;
using UnityEngine;
using L = Clover.Player.Forms.ElementLookSet;

namespace Clover.Tests
{
    /// <summary>
    /// The elements' second-pass looks (the Blender worker's batches 1 and 2): the keys ease as the recipe says, the
    /// shells climb band by band in the look-dev's times (Chill snaps on and holds, Rime builds slowly, Temper climbs
    /// behind its front), a repeating part whose window runs past the end is taken whole into the next loop, held parts
    /// that keep moving go on moving, ribbons are tubes drawn tail to head, and once built every look the game asks for
    /// is there, within the pool's size, with no rings round the player in Temper (the shader draws its front), wind
    /// never white and the choices made between the two makers' Burn, Static and Overdrive.
    /// </summary>
    public class ElementLooksTests
    {
        [Test]
        public void KeysEaseAsTheRecipeNamesThem()
        {
            Assert.AreEqual(0.25f, L.Ease(L.In, 0.5f), 1e-5f);
            Assert.AreEqual(0.75f, L.Ease(L.Out, 0.5f), 1e-5f);
            Assert.AreEqual(0.5f, L.Ease(L.InOut, 0.5f), 1e-5f);
            Assert.AreEqual(0f, L.Ease(L.Step, 0.99f));
            Assert.AreEqual(1f, L.Ease(L.Step, 1f));
            Assert.Greater(L.Ease(L.OutBack, 0.7f), 1f, "an overshoot goes past and comes back");
            var set = ScriptableObject.CreateInstance<L>();
            set.Keys = new[]
            {
                new L.Key { Time = 0.1f, Value = Vector3.zero, Ease = L.Linear },
                new L.Key { Time = 0.5f, Value = new Vector3(0f, -2f, 0f), Ease = L.In },
            };
            Assert.AreEqual(Vector3.zero, set.Sample(0, 2, 0f, Vector3.one), "before its first key a channel holds its first value");
            Assert.AreEqual(-0.5f, set.Sample(0, 2, 0.3f, Vector3.one).y, 1e-4f, "half way with an ease in: a quarter of the way");
            Assert.AreEqual(-2f, set.Sample(0, 2, 3f, Vector3.one).y, 1e-5f, "after its last it holds its last");
            Assert.AreEqual(Vector3.one, set.Sample(0, 0, 0.3f, Vector3.one), "no keys: the given rest value");
            Object.DestroyImmediate(set);
        }

        [Test]
        public void ShellsClimbBandByBandAndHold()
        {
            var chill = new L.Look { Shell = L.Ice, ShellBands = 5, ShellStart = 0.1f, ShellStep = 0.1f, ShellGrow = 0.09f };
            Assert.AreEqual(0f, L.ShellReveal(chill, 0.05f), "nothing before the first band");
            Assert.AreEqual(0.36f, L.ShellReveal(chill, 0.25f), 0.03f, "the first band on, the second snapping on");
            Assert.AreEqual(1f, L.ShellReveal(chill, 0.6f), 1e-4f, "feet to shoulders by 0.6 s");
            Assert.AreEqual(1f, L.ShellReveal(chill, 5f), 1e-4f, "then it holds: no rapid change (the owner)");
            var rime = new L.Look { Shell = L.Rime, ShellBands = 5, ShellStart = 0.05f, ShellStep = 0.3f, ShellGrow = 0.45f };
            Assert.Less(L.ShellReveal(rime, 0.6f), 0.5f, "Rime is still building at 0.6 s (\"instead of slowly building\")");
            Assert.AreEqual(1f, L.ShellReveal(rime, 1.75f), 1e-4f);
            float last = 0f;
            for (float t = 0f; t < 2f; t += 0.02f)
            {
                float now = L.ShellReveal(rime, t);
                Assert.GreaterOrEqual(now, last - 1e-5f, "it only ever climbs");
                last = now;
            }
        }

        /// <summary>
        /// The elements 3 moments (staging/blender/2026-11-20_elements3): each charged blade but Lightning's lands in its
        /// own three blows and wears nothing on the blade (the owner, 2026-11-20: "take out all the elements on top of the
        /// blade effects"), each reworked spell and infused strike has its look, Tailwind is the wind's surge, and Red Hot
        /// blasts on.
        /// </summary>
        [Test]
        public void EveryElements3MomentIsBuilt()
        {
            var set = ElementLookBuilder.Set();
            if (set == null || UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(ElementLookBuilder.RevisionRecipePath) == null)
                Assert.Ignore("The elements 3 recipe is not in the project, or the looks are not built.");
            for (int element = 0; element < 11; element++)
            {
                string blade = ElementMoves.BladeSlug(element);
                if (blade == null) continue;
                // Round 4: Life's blows left nothing on the body (its thorns, seed lumps and sprouts are left out), so no look is built.
                if (element == ElementSubstance.Life)
                {
                    for (int blow = 1; blow <= 3; blow++) Assert.Less(set.Find(ElementMoves.HitKey(element, blow)), 0, $"{blade}, blow {blow}");
                    continue;
                }
                for (int blow = 1; blow <= 3; blow++) Assert.GreaterOrEqual(set.Find(ElementMoves.HitKey(element, blow)), 0, $"{blade}, blow {blow}");
                Assert.Less(set.FindSheath(element), 0, $"{blade} wears a sheath on the blade");
                string spell = ElementMoves.SpellKey(element, false), strike = ElementMoves.StrikeKey(element, false);
                if (spell != null) Assert.GreaterOrEqual(set.Find(spell), 0, spell);
                if (strike != null) Assert.IsTrue(set.Find(strike) >= 0 || set.Find(ElementMoves.StrikeKey(element, true)) >= 0, strike);
            }
            Assert.GreaterOrEqual(set.Find(ElementMoves.Tailwind), 0, "Tailwind");
            Assert.GreaterOrEqual(set.FindVariant("red_hot", '+'), 0, "Red Hot's blast");
            Assert.AreEqual(0, set.Sheaths.Length, "blade sheaths built");
        }

        [Test]
        public void EveryLookTheGameAsksForIsBuilt()
        {
            var set = ElementLookBuilder.Set();
            if (set == null) Assert.Ignore("The element looks have not been built; run Clover > Build > Element Looks.");
            Assert.AreEqual(26, set.Looks.Count(look => !look.Key.Contains("@") && !look.Key.StartsWith(ElementMoves.Prefix) && System.Array.IndexOf(ElementLookBuilder.ReactionsBuilt, look.Key) < 0),
                "21 looks of batches 1 and 2, Unsteady built for one, three and five stacks, elements 4's Marked and Bedrock (Glaze's is not built since round 4), and round 4's Kindle");
            foreach (AfflictionKind kind in System.Enum.GetValues(typeof(AfflictionKind)))
                for (int depth = 1; depth <= 6; depth++)
                {
                    string key = AfflictionArt.LookOf(kind, depth);
                    if (key != null) Assert.GreaterOrEqual(set.Find(key), 0, $"{kind} at {depth} wears {key}");
                }
            Assert.GreaterOrEqual(set.Find(StrikeArt.WindStrikeLook), 0, "wind's strike");
            Assert.GreaterOrEqual(set.Find(AfflictionArt.BurnKitLook), 0, "the Blender worker's Burn, kept to be chosen");
            for (int element = 0; element < 11; element++)
            {
                string key = SurgeArt.LookOf(element);
                if (key != null) Assert.GreaterOrEqual(set.Find(key), 0, key);
            }
            Assert.GreaterOrEqual(set.Find(ReactionDesigns.SwordLook), 0);
            Assert.GreaterOrEqual(set.Find(ReactionDesigns.RustLook), 0);
            foreach (var look in set.Looks)
            {
                Assert.Less(look.PartCount, ElementLooks.MostParts, $"{look.Key}: a look built at the cap has lost pieces");
                // A strike's look that only lifts the body struck (Earth's stones throwing it up) has no pieces of its own.
                if (look.RaiseCount == 0) Assert.Greater(look.PartCount, 0, look.Key);
                for (int k = 0; k < look.PartCount; k++)
                {
                    var part = set.Parts[look.FirstPart + k];
                    Assert.IsNotNull(part.Mesh, $"{look.Key} part {k}");
                    Assert.IsNotNull(part.Material, $"{look.Key} part {k}");
                    Assert.LessOrEqual(part.PathFirst + part.PathCount, set.Keys.Length);
                }
            }
            var temper = set.Looks[set.Find("metal_temper")];
            Assert.AreEqual(L.Steel, temper.Shell, "Temper is the player turned to steel");
            for (int k = 0; k < temper.PartCount; k++)
                StringAssert.DoesNotContain("ring", set.Parts[temper.FirstPart + k].Mesh.name, "no rings round the player: the shader draws the front");
            Assert.AreEqual(L.Ice, set.Looks[set.Find("ice_chill")].Shell);
            Assert.AreEqual(L.Rime, set.Looks[set.Find("ice_rime")].Shell);
            Assert.IsTrue(set.Looks.Where(look => look.Key.StartsWith("water_wellspring") || look.Key.StartsWith("poison_fever") || look.Key.StartsWith("life_regrowth") || look.Key.StartsWith("dark_hunger")).All(look => look.OnPlayer), "the surges are on the player");
            Assert.IsTrue(set.Looks.Where(look => look.Key == "lightning_overdrive" || look.Key == "light_clarity" || look.Key == "wind_strike").All(look => look.OnPlayer), "Overdrive, Clarity and the wind strike are the player's");
        }

        [Test]
        public void ARepeatingPartPastTheEndIsTakenWholeIntoTheNextLoop()
        {
            var look = new L.Look { Seconds = 2.6f, LoopFrom = 0.22f };
            // A curl drawn on at the end of the loop (2.02 s to 2.82 s), and one in the middle.
            var late = new L.Part { Repeats = true, Show = 2.02f, Hide = 2.82f };
            var middle = new L.Part { Repeats = true, Show = 0.82f, Hide = 1.62f };
            float span = 2.6f - 0.22f;
            Assert.AreEqual(2.4f, L.PartTime(look, late, 2.4f, false), 1e-4f, "the first time through, the look's own time");
            float justAfter = L.PartTime(look, late, 2.7f, false);
            Assert.AreEqual(2.7f, justAfter, 1e-3f, "past the end it is not cut: it goes on to its own end");
            Assert.AreEqual(1.0f, L.PartTime(look, middle, 2.6f + (1.0f - 0.22f), false), 1e-3f, "the middle one plays again in the loop");
            for (float age = 2.61f; age < 2.6f + 3f * span; age += 0.01f)
            {
                float t = L.PartTime(look, late, age, false);
                bool seen = t >= late.Show && t < late.Hide;
                float into = Mathf.Repeat(age - 2.6f, span) + 0.22f;
                bool shouldBe = (into >= 2.02f && into < 2.82f) || (into + span >= 2.02f && into + span < 2.82f);
                Assert.AreEqual(shouldBe, seen, $"at {age:0.00} s it is seen exactly in its window, every loop");
            }
            var held = new L.Part { Show = 0.1f, Hide = 9f };
            Assert.AreEqual(2.601f, L.PartTime(look, held, 30f, false), 1e-3f, "a held part holds its last pose");
            var flame = new L.Part { Show = 0.1f, Hide = 9f, Cycles = true };
            float a = L.PartTime(look, flame, 10f, false), b = L.PartTime(look, flame, 10.3f, false);
            Assert.That(a, Is.InRange(2.6f - L.CycleSeconds, 2.6f), "a flickering flame goes round its last stretch");
            Assert.AreNotEqual(a, b, "and keeps moving");
        }

        [Test]
        public void TurningPartsTurnOnAtTheirLastRate()
        {
            var set = ScriptableObject.CreateInstance<L>();
            set.Keys = new[]
            {
                new L.Key { Time = 0.2f, Value = Vector3.zero, Ease = L.Linear },
                new L.Key { Time = 2.2f, Value = new Vector3(-600f, 0f, 0f), Ease = L.Linear },
            };
            Assert.AreEqual(-300f, set.SampleOnward(0, 2, 1.2f), 1e-3f, "inside its keys, the keys");
            Assert.AreEqual(-900f, set.SampleOnward(0, 2, 3.2f), 1e-3f, "past them, on at 300 degrees a second");
            Assert.AreEqual(0f, set.SampleOnward(0, 0, 3f), "no keys, no turn");
            Object.DestroyImmediate(set);
        }

        [Test]
        public void ARibbonIsATubeDrawnFromTailToHead()
        {
            var points = new System.Collections.Generic.List<Vector3> { new Vector3(0.4f, 0.6f, 0.5f), new Vector3(0f, 0.5f, 0.65f), new Vector3(-0.6f, 0.45f, 0.1f), new Vector3(-0.2f, 0.57f, -0.4f) };
            foreach (string profile in new[] { "taper", "needle" })
            {
                var mesh = ElementLookBuilder.RibbonMesh("test", points, 0.26f, profile, new Vector2(0.2f, 1f), false, 0.45f, Color.grey);
                var uvs = new System.Collections.Generic.List<Vector2>();
                mesh.GetUVs(0, uvs);
                Assert.AreEqual(0f, uvs.Min(uv => uv.x), 1e-5f, $"{profile}: u starts at the tail");
                Assert.AreEqual(1f, uvs.Max(uv => uv.x), 1e-5f, $"{profile}: and ends at the head");
                Assert.IsTrue(mesh.vertices.All(v => !float.IsNaN(v.x) && !float.IsNaN(v.y) && !float.IsNaN(v.z)), $"{profile}: no broken vertices (the needle's head once went NaN)");
                Assert.Less(mesh.bounds.size.y, 0.6f, $"{profile}: a band, not a wall");
                var normals = mesh.normals;
                var triangles = mesh.triangles;
                var vertices = mesh.vertices;
                int outward = 0, inward = 0;
                for (int i = 0; i < triangles.Length; i += 3)
                {
                    var face = Vector3.Cross(vertices[triangles[i + 1]] - vertices[triangles[i]], vertices[triangles[i + 2]] - vertices[triangles[i]]);
                    if (face.sqrMagnitude < 1e-12f) continue;
                    if (Vector3.Dot(face, normals[triangles[i]]) > 0f) outward++;
                    else inward++;
                }
                Assert.Greater(outward, inward * 10, $"{profile}: its faces turn out, so the ink stays behind it");
                Object.DestroyImmediate(mesh);
            }
        }

        [Test]
        public void RibbonsAreDrawnAndWindIsNeverWhite()
        {
            var set = ElementLookBuilder.Set();
            if (set == null) Assert.Ignore("The element looks have not been built; run Clover > Build > Element Looks.");
            foreach (string key in new[] { "lightning_static", "lightning_overdrive", "wind_unsteady", "wind_unsteady_1", "wind_unsteady_5", "wind_strike" })
            {
                var look = set.Looks[set.Find(key)];
                int ribbons = 0;
                for (int k = 0; k < look.PartCount; k++)
                {
                    var part = set.Parts[look.FirstPart + k];
                    if (part.Kind != L.Ribbon) continue;
                    ribbons++;
                    Assert.Greater(part.DrawCount, 0, $"{key}: a ribbon is drawn on");
                    Assert.LessOrEqual(part.DrawFirst + part.DrawCount, set.Keys.Length);
                    Assert.AreEqual("Clover/Spell", part.Material.shader.name, $"{key}: the spell shader clips it along its length");
                    if (!key.StartsWith("wind")) continue;
                    // What the screen shows at its brightest: the paint through the wind solid's grey and the shader's
                    // bright facet (a quarter more), never white.
                    var paint = part.Mesh.colors[0];
                    var solid = part.Material.GetColor("_Color");
                    float brightest = Mathf.Max(paint.r * solid.r, paint.g * solid.g, paint.b * solid.b) * 1.25f + part.Material.GetFloat("_Sheen");
                    Assert.Less(brightest, 0.9f, $"{key}: pale dust, not white");
                    Assert.Greater(part.InkColour.g, part.InkColour.r, $"{key}: a mint rim");
                }
                Assert.Greater(ribbons, 0, key);
            }
            var unsteady = set.Looks[set.Find("wind_unsteady")];
            var five = set.Looks[set.Find("wind_unsteady_5")];
            var one = set.Looks[set.Find("wind_unsteady_1")];
            Assert.Less(one.PartCount, unsteady.PartCount, "one stack: fewer curls and leaves than three");
            Assert.Less(unsteady.PartCount, five.PartCount, "five stacks: more than three");
        }

        [Test]
        public void AWindLeafIsALeafWithAMidrib()
        {
            var mesh = ElementLookBuilder.LeafMesh("leaf", new Vector3(-0.05f, 0f, -0.1f), new Vector3(0.05f, 0.01f, 0.1f), 2, 0, 1, new Color(0.56f, 0.6f, 0.2f));
            var bounds = mesh.bounds;
            Assert.AreEqual(0.2f, bounds.size.z, 0.01f, "as long as the kit's leaf");
            Assert.Less(bounds.size.x, 0.11f, "and no wider");
            var vertices = mesh.vertices;
            float widestNearTip = vertices.Where(v => v.z > 0.09f).Max(v => Mathf.Abs(v.x));
            float widest = vertices.Max(v => Mathf.Abs(v.x));
            Assert.Less(widestNearTip, widest * 0.2f, "it comes to a point at the tip, not a diamond's corner");
            var colours = mesh.colors;
            Assert.IsTrue(colours.Any(c => c.g < 0.35f), "a darker midrib runs along it");
            Assert.IsTrue(colours.All(c => Mathf.Max(c.r, c.g, c.b) < 0.75f), "muted, never white or gold-bright");
            Object.DestroyImmediate(mesh);
        }

        [Test]
        public void GlazeGrowsOutOfTheBodyAndNothingFloats()
        {
            string surge = System.IO.File.ReadAllText("Assets/_Project/Scripts/Runtime/Player/Forms/SurgeArt.cs");
            StringAssert.DoesNotContain("Floor(0, \"ice_knotring_surge\")", surge, "no white frost ring round Glaze");
            Assert.IsTrue(SurgeArt.ReplacesBuilt(Elements.ByName(Elements.Ice)), "Glaze's built floating crystals are not drawn");
            Assert.IsTrue(SurgeArt.DrawnWithoutHolding(ElementSubstance.Ice), "Glaze holds no pieces: the ice is on the body");
            var set = ElementLookBuilder.Set();
            if (set == null) Assert.Ignore("The element looks have not been built; run Clover > Build > Element Looks.");
            Assert.IsNotNull(set.Crystal, "the crystal Glaze grows");
            Assert.IsNotNull(set.CrystalMaterial);
            Assert.AreEqual(0f, set.Crystal.bounds.min.y, 0.01f, "stood on its base, so it grows out from where it is rooted");
            Assert.Greater(set.Crystal.bounds.size.y, set.Crystal.bounds.size.x, "its length up");
            var paint = set.Crystal.colors[0];
            Assert.Greater(paint.b, paint.r * 2f, "a clear blue, not white");
        }

        [Test]
        public void EveryElementBrazierHoldsItsElement()
        {
            var set = UnityEditor.AssetDatabase.LoadAssetAtPath<L>(ElementLookBuilder.BrazierSetPath);
            if (set == null) Assert.Ignore("The brazier looks have not been built; run Clover > Build > Brazier Looks.");
            foreach (var info in Elements.All)
            {
                int look = set.Find("brazier_" + info.Name.ToLowerInvariant());
                Assert.GreaterOrEqual(look, 0, $"{info.Name}'s brazier has its contents");
                Assert.Greater(set.Looks[look].PartCount, 3, info.Name);
                Assert.AreEqual(2.4f, set.Looks[look].Seconds, 0.01f, $"{info.Name}: one 2.4 s loop");
            }
            var light = set.Looks[set.Find("brazier_light")];
            for (int k = 0; k < light.PartCount; k++)
            {
                var part = set.Parts[light.FirstPart + k];
                if (part.Mesh.name.Contains("softray")) Assert.AreEqual(L.Flat, part.Kind, "Light's rays are soft see-through beams, not solid blades");
            }
        }

        [Test]
        public void EachBuiltReactionHasItsOwnBespokeLook()
        {
            var set = ElementLookBuilder.Set();
            if (set == null || UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(ElementLookBuilder.ReactionRecipePath) == null)
                Assert.Ignore("The combos 4 recipe is not in the project yet, or the looks are not built.");
            for (int pair = 0; pair < AfflictionPairs.Count; pair++)
            {
                string slug = ReactionLook.SlugOf(pair);
                bool built = System.Array.IndexOf(ElementLookBuilder.ReactionsBuilt, slug) >= 0;
                int look = set.Find(slug);
                Assert.AreEqual(built, look >= 0, $"{AfflictionPairs.Name(pair)} ({slug}) has a bespoke look exactly when it is built");
                if (!built) continue;
                Assert.Greater(set.Looks[look].PartCount, 3, slug);
                // Its own pieces, not element looks: no part of it is another look's.
                for (int k = 0; k < set.Looks[look].PartCount; k++)
                    StringAssert.StartsWith("look_" + slug, set.Parts[set.Looks[look].FirstPart + k].Mesh.name, slug);
                for (int n = 1; n < 6; n++)
                {
                    int other = set.FindVariant(slug, (char)('0' + n));
                    if (other >= 0) Assert.AreEqual(slug + "@" + n, set.Looks[other].Key);
                }
            }
            Assert.AreEqual("drowned_weight", ReactionLook.SlugOf(AfflictionPairs.Index(AfflictionKind.Soaked, AfflictionKind.Cursed)));
            Assert.AreEqual("seed_storm", ReactionLook.SlugOf(AfflictionPairs.Index(AfflictionKind.Unsteady, AfflictionKind.Seeded)), "Windthrow's look is under its new name");
            // All 55 are built (the owner's review of 2026-11-20: every reaction its own bespoke effect), none left on the
            // earlier designed or composed looks, and every slug the builder names is a reaction of the game.
            Assert.AreEqual(55, AfflictionPairs.Count);
            Assert.AreEqual(AfflictionPairs.Count, ElementLookBuilder.ReactionsBuilt.Distinct().Count(), "every reaction is built, once");
            for (int pair = 0; pair < AfflictionPairs.Count; pair++)
                Assert.GreaterOrEqual(set.Find(ReactionLook.SlugOf(pair)), 0, $"{AfflictionPairs.Name(pair)} has its bespoke look");
            // Turned to Stone climbs as stone and Venom Rime grows as poisoned ice, not as plain ice.
            Assert.AreEqual(ElementLookSet.Stone, set.Looks[set.Find("turned_to_stone")].Shell, "Turned to Stone's shell is stone");
            Assert.AreEqual(ElementLookSet.Venom, set.Looks[set.Find("venom_rime")].Shell, "Venom Rime's shell is poisoned ice");
            // The stone stands as long as the enemy is stone (3.5 s), not only the look's 1.6 s.
            Assert.AreEqual(3.5f, ElementLookSet.ShownFor(set.Looks[set.Find("turned_to_stone")]), 0.001f, "Turned to Stone holds while it stands as stone");
            // Every other reaction keeps its aftermath standing (the crack, the grown plants, the waiting cloud), the
            // afflictions it spent hidden under it, for the aftermath's hold; its player's part ends at its length.
            Assert.AreEqual(ElementLookBuilder.AftermathHold, ElementLookSet.ShownFor(set.Looks[set.Find("glint")]), 0.001f, "a reaction's aftermath is held");
            int onPlayer = set.FindVariant("glint", 'p');
            if (onPlayer >= 0) Assert.AreEqual(set.Looks[onPlayer].Seconds, ElementLookSet.ShownFor(set.Looks[onPlayer]), 0.001f, "the player's part ends at its length");
        }

        /// <summary>
        /// Across both recipes, every piece the recipe gives a body is built into that body's look and no other, and none
        /// is dropped (the builder once read the numbers in piece names as bodies: "spark15", "heavechip4"). The body is
        /// the recipe's own word: combos 4's motion.body, follow_body, an orbit's centre_is; an elements piece with none
        /// of these is the body it stands nearest. A reaction's piece belongs to its look on that body ("slug", "slug@1");
        /// an element look is built for its main body (Unsteady once per body), the others showing it again.
        /// </summary>
        [Test]
        public void EveryPieceIsBuiltOnTheBodyTheRecipeGivesIt()
        {
            var set = ElementLookBuilder.Set();
            var combos = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(ElementLookBuilder.ReactionRecipePath);
            var elements = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(ElementLookBuilder.RecipePath);
            if (set == null || combos == null || elements == null) Assert.Ignore("The recipes are not in the project, or the looks are not built.");
            // Which looks each recipe piece was built into, by its name.
            var builtInto = new System.Collections.Generic.Dictionary<string, System.Collections.Generic.List<string>>();
            foreach (var look in set.Looks)
                for (int k = 0; k < look.PartCount; k++)
                {
                    string piece = ElementLookBuilder.PieceOf(set.Parts[look.FirstPart + k].Mesh);
                    if (piece == null) continue;
                    if (!builtInto.TryGetValue(piece, out var into)) builtInto[piece] = into = new System.Collections.Generic.List<string>();
                    into.Add(look.Key);
                }
            var wrong = new System.Collections.Generic.List<string>();
            int checkedPieces = 0;
            // The third review's recipes too: reactions 5's looks under the game's slugs (the options not picked left out),
            // elements 4's under the game's keys; the earlier recipes' looks they replace are not built.
            var combos5 = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(ElementLookBuilder.R5RecipePath);
            var elements4 = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(ElementLookBuilder.E4RecipePath);
            var e4Keys = ElementLookBuilder.E4Looks.ToDictionary(pair => pair.Value, pair => pair.Key);
            var passes = new System.Collections.Generic.List<(TextAsset recipe, bool reactions, bool redrawn)> { (combos, true, false), (elements, false, false) };
            if (combos5 != null) passes.Add((combos5, true, true));
            if (elements4 != null) passes.Add((elements4, false, true));
            // Round 4's redesigns, built in place of the looks they revise under their tweak names ("rd_" and the game's key).
            bool fourth = ElementLookBuilder.RedesignsIn();
            var rdKeys = ElementLookBuilder.RdLooks.ToDictionary(pair => pair.Value, pair => pair.Key);
            var rdRecipes = new System.Collections.Generic.HashSet<TextAsset>();
            if (fourth)
                foreach (string path in ElementLookBuilder.RdRecipePaths)
                {
                    var rd = UnityEditor.AssetDatabase.LoadAssetAtPath<TextAsset>(path);
                    if (rd == null) continue;
                    rdRecipes.Add(rd);
                    passes.Add((rd, path.Contains("batch1") || path.Contains("batch2"), true));
                }
            foreach (var (recipe, reactions, redrawn) in passes)
            {
                var root = (System.Collections.Generic.Dictionary<string, object>)ReactionRecipe.ReadJson(recipe.text);
                bool round4 = rdRecipes.Contains(recipe);
                foreach (System.Collections.Generic.Dictionary<string, object> look in (System.Collections.Generic.List<object>)root["looks"])
                {
                    string slug = (string)look["slug"];
                    // The name the builder looked its tweaks up by: a round-4 look's "rd_" and the game's key.
                    string tweak = null;
                    if (round4)
                    {
                        if (reactions) slug = ElementLookBuilder.GameSlug(slug);
                        else if (!rdKeys.TryGetValue(slug, out var rdKey)) continue;
                        else slug = rdKey;
                        tweak = ElementLookBuilder.RdPrefix + slug;
                    }
                    else if (redrawn && reactions)
                    {
                        if (ElementLookBuilder.R5NotPicked.Contains(slug)) continue;
                        if (ElementLookBuilder.R5GameSlugs.TryGetValue(slug, out var game)) slug = game;
                        if (fourth && ElementLookBuilder.RdReactions.Contains(slug)) continue;
                    }
                    else if (redrawn)
                    {
                        if (!e4Keys.TryGetValue(slug, out var key)) continue;
                        slug = key;
                        if (fourth && ElementLookBuilder.RdLooks.ContainsKey(slug)) continue;
                    }
                    else if (!reactions && elements4 != null && ElementLookBuilder.E4Looks.ContainsKey(slug)) continue;
                    else if (!reactions && fourth && ElementLookBuilder.RdLooks.ContainsKey(slug)) continue;
                    tweak = tweak ?? slug;
                    var bodies = new System.Collections.Generic.List<Vector3>();
                    foreach (System.Collections.Generic.Dictionary<string, object> body in (System.Collections.Generic.List<object>)look["bodies"]) bodies.Add(Vec(body["position"]));
                    foreach (System.Collections.Generic.Dictionary<string, object> group in (System.Collections.Generic.List<object>)look["groups"])
                    {
                        var instances = (System.Collections.Generic.List<object>)group["instances"];
                        var motions = (System.Collections.Generic.List<object>)group["motion"];
                        for (int n = 0; n < instances.Count; n++)
                        {
                            var instance = (System.Collections.Generic.List<object>)instances[n];
                            var motion = (System.Collections.Generic.Dictionary<string, object>)motions[n];
                            string piece = (string)instance[0];
                            bool onPlayer = motion.TryGetValue("on_player", out var flag) && flag is bool yes && yes || instance.Count > 4 && (string)instance[4] == "p";
                            int body = onPlayer ? -1 : RecipeBody(motion, Vec(instance[1]), bodies, reactions, piece, wrong);
                            // A burning body's own tongues are its real flame (BodyFlames), by where the piece stands on its body.
                            var fromItsBody = Vec(instance[1]) - (body >= 0 && body < bodies.Count ? bodies[body] : Vector3.zero);
                            string notAMesh = ElementLookBuilder.NotAMesh(tweak, (string)group["kit"], (string)group["kit_object"],
                                reactions ? (string)group["material"] : null, fromItsBody, piece, redrawn);
                            string expected = reactions ? ElementLookBuilder.ReactionKey(slug, body)
                                : ElementLookBuilder.PerBody.TryGetValue(slug, out var variants) ? (body < 0 ? variants[0] : body < variants.Length ? variants[body] : null)
                                : body <= 0 ? slug : null;
                            builtInto.TryGetValue(piece, out var into);
                            checkedPieces++;
                            if (notAMesh != null)
                            {
                                if (into != null) wrong.Add($"{piece}: built, though {notAMesh}");
                                continue;
                            }
                            if (expected == null)
                            {
                                // An element look's other bodies show the main body's timeline again: their pieces are not built.
                                if (into != null) wrong.Add($"{piece} (body {body}) is built into {string.Join(", ", into)}, where only the main body's are");
                                continue;
                            }
                            if (into == null) wrong.Add($"{piece} (body {body}) is dropped: in no look, wanted in {expected}");
                            else if (into.Any(key => key != expected) || !into.Contains(expected)) wrong.Add($"{piece} (body {body}) is built into {string.Join(", ", into)}, wanted in {expected}");
                        }
                    }
                }
            }
            Assert.Greater(checkedPieces, 2500, "both recipes were read");
            Assert.IsEmpty(wrong, $"{wrong.Count} pieces off their body:\n" + string.Join("\n", wrong.Take(40)));
        }

        // The body the recipe gives a piece, in its own words; combos 4 names every piece's body, and its body agrees with
        // the piece's follow_body and orbit centre wherever it has them.
        private static int RecipeBody(System.Collections.Generic.Dictionary<string, object> motion, Vector3 position, System.Collections.Generic.List<Vector3> bodies,
            bool named, string piece, System.Collections.Generic.List<string> wrong)
        {
            int follow = motion.TryGetValue("follow_body", out var followValue) && followValue is double followed ? (int)followed : -1;
            int centre = -1;
            if (motion.TryGetValue("orbit", out var orbitValue) && orbitValue is System.Collections.Generic.Dictionary<string, object> orbit && orbit.TryGetValue("centre_is", out var centreIs)
                && centreIs is string text && text.StartsWith("body ")) centre = int.Parse(text.Substring(5));
            if (named)
            {
                if (!(motion.TryGetValue("body", out var bodyValue) && bodyValue is double given))
                {
                    wrong.Add($"{piece}: the recipe names no body");
                    return 0;
                }
                if (follow >= 0 && follow != (int)given) wrong.Add($"{piece}: body {(int)given} but follows body {follow}");
                if (centre >= 0 && centre != (int)given) wrong.Add($"{piece}: body {(int)given} but orbits body {centre}");
                return (int)given;
            }
            if (follow >= 0) return follow;
            if (centre >= 0) return centre;
            int best = 0;
            for (int i = 1; i < bodies.Count; i++)
                if (new Vector2(bodies[i].x - position.x, bodies[i].z - position.z).sqrMagnitude < new Vector2(bodies[best].x - position.x, bodies[best].z - position.z).sqrMagnitude) best = i;
            return best;
        }

        private static Vector3 Vec(object value)
        {
            var list = (System.Collections.Generic.List<object>)value;
            return new Vector3((float)(double)list[0], (float)(double)list[1], (float)(double)list[2]);
        }

        /// <summary>
        /// Black Chain's chain is drawn live between the two bodies the game bound (BoundChains), not baked toward the spot
        /// the recipe stood the second: no ribbon of its look aims there, and its span carries the built chain's link,
        /// solid and violet ink, timed to whip out and bound by the game's own chain condition.
        /// </summary>
        [Test]
        public void BlackChainsChainIsDrawnLiveBetweenTheBoundBodies()
        {
            var set = ElementLookBuilder.Set();
            if (set == null || set.Find("black_chain") < 0) Assert.Ignore("The looks are not built with combos 4.");
            int at = set.FindSpan("black_chain");
            Assert.GreaterOrEqual(at, 0, "Black Chain has its live span");
            var span = set.Spans[at];
            Assert.AreEqual(Clover.Player.Trees.Reactions.Chain, span.Condition, "bound by the game's chain condition");
            Assert.AreEqual(1, span.Body, "the recipe's second body is the bound one");
            Assert.IsNotNull(span.Link);
            Assert.AreEqual(0.34f, span.Link.bounds.size.z, 0.01f, "one link as long as the built chain's");
            Assert.AreEqual(0.2f, span.Link.bounds.size.x, 0.01f, "and as wide");
            Assert.AreEqual("Clover/Spell", span.Material.shader.name);
            Assert.Greater(span.InkWidth, 0f, "inked");
            Assert.Greater(span.InkColour.b, span.InkColour.g, "a violet rim");
            Assert.Greater(span.InkColour.r, span.InkColour.g, "a violet rim");
            Assert.Less(span.Link.colors[0].maxColorComponent, 0.2f, "pure black iron");
            Assert.Less(span.From, span.Reached, "it whips out, then reaches");
            Assert.That(span.Pitch, Is.InRange(0.2f, 0.34f), "links overlap by their bar");
            var look = set.Looks[set.Find("black_chain")];
            for (int k = 0; k < look.PartCount; k++)
                StringAssert.DoesNotContain("_span", set.Parts[look.FirstPart + k].Mesh.name, "no chain baked toward the recipe's spot");
            Assert.AreEqual(5f, ElementLookSet.ShownFor(look), 0.001f, "its wraps stand the 5 s bind");
        }

        [Test]
        public void TheChosenMakersLooksArePlayed()
        {
            Assert.AreEqual(AfflictionArt.CodeLook, AfflictionArt.DefaultBurnStyle, "Burn: the flame field, filmed beside the Blender tongues");
            Assert.AreEqual(AfflictionArt.BothLooks, AfflictionArt.DefaultStaticStyle, "Static: the code's crawl with the Blender worker's arcs");
            Assert.AreEqual(AfflictionArt.KitLook, SurgeArt.DefaultOverdriveStyle, "Overdrive: the Blender worker's arcs, no white forks");
            Assert.AreEqual("lightning_overdrive", SurgeArt.LookOf(ElementSubstance.Lightning));
            Assert.AreEqual("light_clarity", SurgeArt.LookOf(ElementSubstance.Light), "Clarity's gold lotus replaces the kolam");
            Assert.AreEqual("earth_tremor", AfflictionArt.LookOf(AfflictionKind.Tremor), "Tremor: cracks in steps, dust and pebbles, no ring");
            Assert.AreEqual("wind_unsteady_1", AfflictionArt.LookOf(AfflictionKind.Unsteady, 1));
            Assert.AreEqual("wind_unsteady", AfflictionArt.LookOf(AfflictionKind.Unsteady, 3));
            Assert.AreEqual("wind_unsteady_5", AfflictionArt.LookOf(AfflictionKind.Unsteady, 5));
            string hit = System.IO.File.ReadAllText("Assets/_Project/Scripts/Runtime/Player/Forms/HitArt.cs");
            StringAssert.DoesNotContain("\"wind_debris_hit\"", hit, "a wind blow throws no flat white cards");
        }
    }
}

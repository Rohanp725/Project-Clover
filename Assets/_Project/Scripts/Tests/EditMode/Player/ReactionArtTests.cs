using System.Collections.Generic;
using System.Linq;
using Clover.EditorTools;
using Clover.Gameplay;
using Clover.Player.Forms;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// The reactions' looks: the look-dev's recipe reads into the shape the builder needs, every one of the 55 pairs has
    /// a designed or a composed look, the lamps are turned from Blender's watts by the documented rule, and (once built)
    /// the set holds every designed look with its materials.
    /// </summary>
    public class ReactionArtTests
    {
        // Two combos in the recipe's own shape: suffix names, a 0 for no rotation, scales as a number and as a list, a null range.
        private const string Sample = @"{
  ""conventions"": { ""axes"": ""Unity axes"" },
  ""kits"": [""vfx_combo_kit_v3.fbx"", ""vfx_fire_kit_v2.fbx""],
  ""combos"": [
    {
      ""name"": ""Thermal Shock"", ""pair"": ""fire_ice"", ""timing"": ""static"",
      ""bodies"": [ { ""object"": ""enemy~fire_ice"", ""mesh"": ""enemy_husk"", ""position"": [0, 0, 0], ""rotation_euler_deg"": [0, 40, 0], ""scale"": 1 } ],
      ""lights"": [
        { ""object"": ""L~combofire_ice"", ""type"": ""POINT"", ""colour_linear"": [1, 0.45, 0.1], ""intensity_blender_W"": 700, ""range_m"": null, ""radius_m"": 0.3, ""position"": [0.51, 1.2, 0.61], ""first_pass"": true },
        { ""object"": ""c2_fire_ice_cold"", ""type"": ""POINT"", ""colour_linear"": [0.6, 0.85, 1], ""intensity_blender_W"": 200, ""range_m"": 2.5, ""radius_m"": 0.2, ""position"": [-0.5, 2.2, -0.4], ""first_pass"": false }
      ],
      ""groups"": [
        { ""mesh"": ""vfx_combo_seams"", ""kit"": ""vfx_combo_kit_v3.fbx"", ""kit_object"": ""vfx3c_vfx_combo_seams"", ""material"": ""m_vfx_combo_coal"", ""pass"": ""combo"",
          ""instances"": [ ["""", [0, 0.2, 0], [0, -17, 0], [1.15, 1.05, 1.1], ""b""], ["".001"", [0, 0.19, 0], 0, 1.08, ""g""] ] },
        { ""mesh"": ""v2_fire_charrings~affliction"", ""kit"": ""vfx_fire_kit_v2.fbx"", ""kit_object"": ""vfx2_fire_charrings_affliction"", ""material"": ""m2_fire_char"", ""pass"": ""combo2"",
          ""instances"": [ [""c2_fire_ice_fire_charrings"", [-0.01, 0.02, 0], [0, -23, 0], 1.5, ""g""] ] }
      ]
    },
    { ""name"": ""The Sealed Jar"", ""pair"": ""dark_poison"", ""bodies"": [], ""lights"": [], ""groups"": [] }
  ]
}";

        [Test]
        public void TheRecipeReadsIntoItsShape()
        {
            var recipe = ReactionRecipe.Parse(Sample);
            CollectionAssert.AreEqual(new[] { "vfx_combo_kit_v3.fbx", "vfx_fire_kit_v2.fbx" }, recipe.Kits);
            Assert.AreEqual(2, recipe.Combos.Count);
            var shock = recipe.Combos[0];
            Assert.AreEqual("Thermal Shock", shock.Name);
            Assert.AreEqual("fire_ice", shock.Pair);
            Assert.AreEqual(1, shock.Bodies);
            Assert.AreEqual(3, shock.PieceCount);

            var seams = shock.Groups[0];
            Assert.AreEqual("vfx_combo_kit_v3.fbx", seams.Kit);
            Assert.AreEqual("vfx3c_vfx_combo_seams", seams.KitObject);
            Assert.AreEqual("m_vfx_combo_coal", seams.Material);
            Assert.AreEqual("combo", seams.Pass);
            var first = seams.Instances[0];
            Assert.AreEqual(new Vector3(0f, 0.2f, 0f), first.Position);
            Assert.AreEqual(new Vector3(0f, -17f, 0f), first.Rotation);
            Assert.AreEqual(new Vector3(1.15f, 1.05f, 1.1f), first.Scale);
            Assert.IsFalse(first.OnGround, "'b' is on the body");
            var second = seams.Instances[1];
            Assert.AreEqual(Vector3.zero, second.Rotation, "a 0 is no rotation");
            Assert.AreEqual(Vector3.one * 1.08f, second.Scale, "a number is a uniform scale");
            Assert.IsTrue(second.OnGround, "'g' is on the ground");
            Assert.AreEqual("vfx_combo_seams~fire_ice", ReactionRecipe.FullName(shock, seams, first));
            Assert.AreEqual("vfx_combo_seams~fire_ice.001", ReactionRecipe.FullName(shock, seams, second));
            Assert.AreEqual("c2_fire_ice_fire_charrings", ReactionRecipe.FullName(shock, shock.Groups[1], shock.Groups[1].Instances[0]));

            // The instance's matrix is the recipe's position, Euler rotation and scale, in that order.
            var expected = Matrix4x4.TRS(first.Position, Quaternion.Euler(first.Rotation), first.Scale);
            Assert.That(Vector3.Distance(expected.MultiplyPoint3x4(Vector3.one), first.Matrix.MultiplyPoint3x4(Vector3.one)), Is.LessThan(1e-5f));

            Assert.AreEqual(2, shock.Lamps.Count);
            var lamp = shock.Lamps[0];
            Assert.AreEqual(700f, lamp.Watts);
            Assert.AreEqual(-1f, lamp.Range, "a null range is none");
            Assert.AreEqual(new Color(1f, 0.45f, 0.1f, 1f), lamp.Colour);
            Assert.AreEqual(new Vector3(0.51f, 1.2f, 0.61f), lamp.Position);
            Assert.IsTrue(lamp.FirstPass);
            Assert.AreEqual(2.5f, shock.Lamps[1].Range);
            Assert.AreEqual(0, recipe.Combos[1].PieceCount);
        }

        [Test]
        public void TextThatIsNotTheRecipeIsRefused()
        {
            Assert.Throws<System.FormatException>(() => ReactionRecipe.Parse("[1, 2]"));
            Assert.Throws<System.FormatException>(() => ReactionRecipe.Parse("{\"kits\": []}"));
            Assert.Throws<System.FormatException>(() => ReactionRecipe.Parse("{\"combos\": [ {\"groups\": [ {\"instances\": [ [\"\", [0,0,0]] ] } ] } ]}"));
        }

        [Test]
        public void EveryPairHasADesignedOrAComposedLook()
        {
            var designedPairs = ReactionDesigns.Designed.Select(ReactionDesigns.PairOf).ToList();
            Assert.That(designedPairs, Has.All.InRange(0, AfflictionPairs.Count - 1), "every designed key names a pair");
            Assert.AreEqual(24, designedPairs.Distinct().Count(), "24 designed looks, one to a pair");
            int composed = 0;
            for (int pair = 0; pair < AfflictionPairs.Count; pair++)
            {
                if (ReactionDesigns.IsDesigned(pair)) continue;
                composed++;
                ReactionDesigns.Composed(pair, out string first, out string second, out string mark, out int markElement);
                Assert.IsNotNull(first, AfflictionPairs.Name(pair));
                Assert.IsNotNull(second, AfflictionPairs.Name(pair));
                Assert.AreNotEqual(first, second, AfflictionPairs.Name(pair));
                if (mark != null) Assert.AreEqual(mark, ReactionDesigns.Mark(markElement));
            }
            Assert.AreEqual(31, composed);
            // The recipe's key "dark_poison" is the game's Shut in the Dark (the look-dev called it The Sealed Jar).
            Assert.AreEqual("Shut in the Dark", AfflictionPairs.Name(ReactionDesigns.PairOf("dark_poison")));
            Assert.AreEqual(ReactionDesigns.PairOf("ice_fire"), ReactionDesigns.PairOf("fire_ice"), "either order");
            Assert.AreEqual(-1, ReactionDesigns.PairOf("fire_fire"));
            Assert.AreEqual(-1, ReactionDesigns.PairOf("fire"));
        }

        [Test]
        public void TheRingIsTheElementArtsAndTheAfflictions()
        {
            CollectionAssert.AreEqual(ElementArtBuilder.Elements, ReactionDesigns.Ring);
            for (int i = 0; i < AfflictionPairs.Ring.Length; i++) Assert.AreEqual(i, ReactionDesigns.RingIndex(AfflictionPairs.Ring[i]));
        }

        [Test]
        public void TheLampsFollowTheRule()
        {
            // I = W / (4 pi^2): Blender's watt lights a surface as URP's intensity does once divided by 4 pi^2.
            Assert.AreEqual(10f, ReactionDesigns.LampIntensity(395f), 0.01f);
            Assert.AreEqual(0f, ReactionDesigns.LampIntensity(-5f));
            // The reach: the recipe's own when it has one, else sqrt(2 I), held to 1.5 to 5 m.
            Assert.AreEqual(Mathf.Sqrt(20f), ReactionDesigns.LampRange(395f, -1f), 0.01f);
            Assert.AreEqual(ReactionDesigns.MostLampRange, ReactionDesigns.LampRange(10000f, -1f));
            Assert.AreEqual(ReactionDesigns.LeastLampRange, ReactionDesigns.LampRange(1f, -1f));
            Assert.AreEqual(2.2f, ReactionDesigns.LampRange(100f, 2.2f), 0.001f);
            // At most two, the brightest, in the recipe's order; ties go to the first.
            var kept = new int[ReactionDesigns.MostLampsPerLook];
            Assert.AreEqual(2, ReactionDesigns.KeepBrightest(new[] { 200f, 700f, 450f }, ReactionDesigns.MostLampsPerLook, kept));
            CollectionAssert.AreEqual(new[] { 1, 2 }, kept);
            Assert.AreEqual(2, ReactionDesigns.KeepBrightest(new[] { 300f, 300f, 180f, 180f, 180f }, 2, kept));
            CollectionAssert.AreEqual(new[] { 0, 1 }, kept);
            Assert.AreEqual(1, ReactionDesigns.KeepBrightest(new[] { 500f }, 2, kept));
            Assert.AreEqual(0, kept[0]);
            Assert.AreEqual(0, ReactionDesigns.KeepBrightest(new float[0], 2, kept));
        }

        [Test]
        public void PiecesAreDrawnInTheirElementsInk()
        {
            Assert.AreEqual("lightning", ReactionArtBuilder.ElementOf("m2_bolt_cyan", "light_lightning"));
            Assert.AreEqual("fire", ReactionArtBuilder.ElementOf("m_vfx_combo_coal", "fire_ice"));
            Assert.AreEqual("ice", ReactionArtBuilder.ElementOf("m_vfx_combo_frost", "fire_ice"));
            Assert.AreEqual("ice", ReactionArtBuilder.ElementOf("mc2_breath", "dark_ice"));
            Assert.AreEqual("dark", ReactionArtBuilder.ElementOf("m2_dark_hole", "light_lightning"));
            Assert.AreEqual("water", ReactionArtBuilder.ElementOf("m_vfx_water_foam", "wind_water"));
            Assert.AreEqual("poison", ReactionArtBuilder.ElementOf("m_vfx_combo_unheard", "poison_fire"), "an unknown combo material takes the pair's first");
            Assert.AreEqual("Assets/_Project/Art/VFX/Combos/vfx_combo_kit_v2.fbx", ReactionArtBuilder.KitPath("vfx_combo_kit_v2.fbx"));
            Assert.AreEqual("Assets/_Project/Art/VFX/Lightning/vfx_lightning_kit_v2.fbx", ReactionArtBuilder.KitPath("vfx_lightning_kit_v2.fbx"));
        }

        [Test]
        public void TheRealRecipeIsWholeAndEveryMaterialHasItsPaint()
        {
            var asset = ReactionArtBuilder.RecipeAsset();
            if (asset == null) Assert.Ignore($"The recipe is not in the project yet ({ReactionArtBuilder.RecipePath}); bring in the combo kits first.");
            var recipe = ReactionRecipe.Parse(asset.text);
            Assert.AreEqual(24, recipe.Combos.Count);
            Assert.AreEqual(1024, recipe.Combos.Sum(combo => combo.PieceCount), "the notes count 1024 pieces");
            Assert.AreEqual(35, recipe.Combos.Sum(combo => combo.Lamps.Count), "and 35 lamps");
            CollectionAssert.AreEquivalent(ReactionDesigns.Designed, recipe.Combos.Select(combo => combo.Pair));
            var unpainted = new HashSet<string>();
            var noKit = new HashSet<string>();
            foreach (var combo in recipe.Combos)
                foreach (var group in combo.Groups)
                {
                    if (!ReactionArtBuilder.TryPaint(group.Material, out _)) unpainted.Add(group.Material);
                    if (AssetDatabase.LoadAssetAtPath<GameObject>(ReactionArtBuilder.KitPath(group.Kit)) == null) noKit.Add(group.Kit);
                }
            Assert.That(unpainted, Is.Empty, "materials with no paint (they would be drawn grey)");
            Assert.That(noKit, Is.Empty, "kits the recipe names that are not in the project");
        }

        [Test]
        public void TheComposedLooksNameBuiltPieces()
        {
            var art = ElementArtBuilder.Set();
            if (art == null) Assert.Ignore("The element art has not been built yet; run Clover > Build > Element Art.");
            var known = new HashSet<string>(art.Pieces.Select(piece => piece.Name));
            var missing = new List<string>();
            for (int element = 0; element < ReactionDesigns.Ring.Length; element++)
            {
                if (!known.Contains(ReactionDesigns.BodyPiece(element))) missing.Add(ReactionDesigns.BodyPiece(element));
                string mark = ReactionDesigns.Mark(element);
                if (mark != null && !known.Contains(mark)) missing.Add(mark);
            }
            Assert.That(missing, Is.Empty, "Not in Generated/Vfx/Element/element_art.asset");
        }

        // Every combo-kit piece, placed as the builder places it, against its object in the look-dev's .blend (as Unity
        // imports it) about the same body's feet: within 2 cm on average. Without the kits' half turn about up, every
        // asymmetric one (a bolt, a lip, a glint) faced the wrong way.
        [Test]
        public void TheComboKitPiecesLieAsTheSourceHasThem()
        {
            var asset = ReactionArtBuilder.RecipeAsset();
            var blend = AssetDatabase.LoadAssetAtPath<GameObject>(ReactionArtBuilder.SourcePath);
            if (asset == null || blend == null) Assert.Ignore($"The recipe or {ReactionArtBuilder.SourcePath} is not in the project (the .blend needs Blender to import).");
            var recipe = ReactionRecipe.Parse(asset.text);
            var objects = new Dictionary<string, Transform>();
            foreach (var place in blend.GetComponentsInChildren<Transform>(true))
                if (!objects.ContainsKey(place.name)) objects[place.name] = place;
            var far = new List<string>();
            int checkedPieces = 0;
            foreach (var combo in recipe.Combos)
            {
                Assert.IsTrue(objects.TryGetValue("enemy~" + combo.Pair, out var body), combo.Pair);
                var origin = blend.transform.InverseTransformPoint(body.position);
                origin.y = 0f;
                foreach (var group in combo.Groups.Where(group => group.Kit.StartsWith("vfx_combo_")))
                {
                    var kit = AssetDatabase.LoadAssetAtPath<GameObject>(ReactionArtBuilder.KitPath(group.Kit));
                    var filter = kit.GetComponentsInChildren<MeshFilter>(true).First(piece => piece.name == group.KitObject);
                    var instance = group.Instances[0];
                    if (!objects.TryGetValue(ReactionRecipe.FullName(combo, group, instance), out var source)) continue;
                    var place = ReactionArtBuilder.Placement(group, instance, filter, kit.transform);
                    var fromSource = blend.transform.worldToLocalMatrix * source.localToWorldMatrix;
                    var theirs = source.GetComponent<MeshFilter>().sharedMesh.vertices.Select(v => fromSource.MultiplyPoint3x4(v) - origin).ToArray();
                    var ours = filter.sharedMesh.vertices;
                    float sum = 0f;
                    int count = 0;
                    for (int i = 0; i < ours.Length; i += Mathf.Max(1, ours.Length / 40))
                    {
                        var at = place.MultiplyPoint3x4(ours[i]);
                        sum += Mathf.Sqrt(theirs.Min(v => (v - at).sqrMagnitude));
                        count++;
                    }
                    checkedPieces++;
                    if (sum / count > 0.02f) far.Add($"{combo.Pair} {group.KitObject}: {sum / count:F3} m");
                }
            }
            Assert.Greater(checkedPieces, 50);
            Assert.That(far, Is.Empty, "combo-kit pieces not lying where the source has them");
        }

        // The v3 kit keeps one material an object; the build takes each triangle's band back from the source, so Sleet
        // Charge's bolts are a white core in a cyan band in their violet edge, not violet all through.
        [Test]
        public void TheBoltsKeepTheirBands()
        {
            var set = ReactionArtBuilder.Set();
            if (set == null || AssetDatabase.LoadAssetAtPath<GameObject>(ReactionArtBuilder.SourcePath) == null)
                Assert.Ignore("The reaction art has not been built from the source .blend yet; run Clover > Build > Reaction Art.");
            var look = set.Looks.First(each => each.Key == "lightning_ice");
            var colours = new List<Color>();
            for (int i = look.FirstPart; i < look.FirstPart + look.PartCount; i++)
                if (set.Parts[i].Mesh != null) colours.AddRange(set.Parts[i].Mesh.colors);
            foreach (string band in new[] { "m_vfx_lightning_edge", "m_vfx_lightning_cyan", "m_vfx_lightning_core" })
            {
                Assert.IsTrue(ReactionArtBuilder.TryPaint(band, out var paint), band);
                Assert.IsTrue(colours.Any(colour => Mathf.Abs(colour.r - paint.Colour.r) < 0.01f && Mathf.Abs(colour.g - paint.Colour.g) < 0.01f && Mathf.Abs(colour.b - paint.Colour.b) < 0.01f),
                    $"Sleet Charge has no triangle in {band}");
            }
        }

        // Clover/Decal's colours are [HDR], which Unity hands to the shader unconverted: a mark's colours are stored
        // linear, as the paints are (stored as the eye sees them, Caught Light's navy ink pool showed sky blue).
        [Test]
        public void TheFloorMarksHoldTheirPaintsInLinear()
        {
            var material = AssetDatabase.LoadAssetAtPath<Material>(ReactionArtBuilder.Folder + "/m_reaction_decal_water_light_0.mat");
            if (material == null) Assert.Ignore("The reaction art has not been built yet; run Clover > Build > Reaction Art.");
            Assert.IsTrue(ReactionArtBuilder.TryPaint("m_vfx_water_ink", out var ink));
            var held = new[] { "_ColorR", "_ColorG", "_ColorB", "_ColorA" }.Select(material.GetColor).ToArray();
            Assert.IsTrue(held.Any(colour => Mathf.Abs(colour.r - ink.Colour.r) < 0.002f && Mathf.Abs(colour.g - ink.Colour.g) < 0.002f && Mathf.Abs(colour.b - ink.Colour.b) < 0.002f),
                "the ink pool's colour is not held linear: " + string.Join(", ", held));
        }

        [Test]
        public void TheBuiltSetHoldsEveryDesignedLook()
        {
            var set = ReactionArtBuilder.Set();
            if (set == null) Assert.Ignore("The reaction art has not been built yet; run Clover > Build > Reaction Art.");
            Assert.AreEqual(ReactionDesigns.Designed.Length, set.Looks.Length);
            CollectionAssert.AreEquivalent(ReactionDesigns.Designed.Select(ReactionDesigns.PairOf), set.Looks.Select(look => look.Pair));
            foreach (var look in set.Looks)
            {
                Assert.Greater(look.PartCount, 0, look.Name);
                Assert.LessOrEqual(look.PartCount, ReactionArtBuilder.PartBudget, $"{look.Name}: its parts are renderers the crowd's pool must find");
                Assert.LessOrEqual(look.LampCount, ReactionDesigns.MostLampsPerLook, look.Name);
                Assert.LessOrEqual(look.Triangles, ReactionArtBuilder.TriangleBudget, look.Name);
                Assert.That(look.Seconds, Is.InRange(0.6f, 1.7f), $"{look.Name} plays in about a second");
                Assert.That(look.ImpactAt, Is.InRange(0f, look.Seconds), look.Name);
                Assert.IsFalse(string.IsNullOrEmpty(look.Story), $"{look.Name} has its timeline");
                bool arrives = false, lands = false;
                for (int i = look.FirstPart; i < look.FirstPart + look.PartCount; i++)
                {
                    var part = set.Parts[i];
                    Assert.IsNotNull(part.Material, part.Name);
                    if (part.Kind != ReactionArtSet.Decal) Assert.IsNotNull(part.Mesh, part.Name);
                    else Assert.Greater(part.Size.x * part.Size.z, 0f, part.Name);
                    Assert.IsTrue(part.Motion.Set, $"{part.Name} has its cue");
                    arrives |= part.Motion.Beat == ReactionMotion.Arrive;
                    lands |= part.Motion.Beat == ReactionMotion.Impact;
                }
                Assert.IsTrue(arrives && lands, $"{look.Name}: the cause arrives and lands (arrival {arrives}, impact {lands})");
                for (int i = look.FirstLamp; i < look.FirstLamp + look.LampCount; i++)
                {
                    Assert.Greater(set.Lamps[i].Intensity, 0f, look.Name);
                    Assert.That(set.Lamps[i].Range, Is.InRange(ReactionDesigns.LeastLampRange, ReactionDesigns.MostLampRange), look.Name);
                }
            }
            var art = ElementArtBuilder.Set();
            Assert.IsNotNull(art);
            Assert.AreSame(set, art.Reactions, "the element art carries the reactions into every scene that has it");
        }
    }
}

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Clover.EditorTools;
using Clover.Player.Forms;
using NUnit.Framework;
using UnityEditor;

namespace Clover.Tests
{
    /// <summary>
    /// The element art the moments ask for by name is in the built set: every quoted piece or sheet name in the code
    /// that draws the moments resolves, so a renamed kit piece cannot silently draw nothing.
    /// </summary>
    public class ElementArtTests
    {
        private static readonly string[] Drawers =
        {
            "Assets/_Project/Scripts/Runtime/Player/Forms/AfflictionArt.cs",
            "Assets/_Project/Scripts/Runtime/Player/Forms/HitArt.cs",
            "Assets/_Project/Scripts/Runtime/Player/Forms/BladeArt.cs",
            "Assets/_Project/Scripts/Runtime/Player/Forms/StrikeArt.cs",
            "Assets/_Project/Scripts/Runtime/Player/Forms/SurgeArt.cs",
            "Assets/_Project/Scripts/Runtime/Player/Forms/TapForm.cs",
        };

        // A quoted name that starts with an element: "fire_hoods_spell", "dark_hole_close_v2".
        private static readonly Regex Name = new Regex("\"((fire|lightning|wind|ice|water|life|earth|metal|light|dark|poison)_[a-z0-9_]+)\"");

        [Test]
        public void EveryPieceAndSheetTheMomentsNameIsBuilt()
        {
            var set = ElementArtBuilder.Set();
            if (set == null) Assert.Ignore("The element art has not been built yet; run Clover > Build > Element Art.");
            var known = new HashSet<string>(set.Pieces.Select(piece => piece.Name).Concat(set.Sheets.Select(sheet => sheet.Name)));
            // The second-pass looks are asked for by their slugs ("water_soaked"): they are built into element_looks.asset.
            var looks = ElementLookBuilder.Set();
            if (looks != null) known.UnionWith(looks.Looks.Select(look => look.Key));
            var missing = new List<string>();
            int names = 0;
            foreach (string file in Drawers)
                foreach (Match match in Name.Matches(File.ReadAllText(file)))
                {
                    names++;
                    if (!known.Contains(match.Groups[1].Value)) missing.Add($"{Path.GetFileName(file)}: {match.Groups[1].Value}");
                }
            Assert.Greater(names, 100, "the scan found the names");
            Assert.That(missing, Is.Empty, "Not in Generated/Vfx/Element/element_art.asset:\n" + string.Join("\n", missing));
        }

        [Test]
        public void EveryElementHasItsSolidMaterialAndEveryPieceItsMaterial()
        {
            var set = ElementArtBuilder.Set();
            if (set == null) Assert.Ignore("The element art has not been built yet; run Clover > Build > Element Art.");
            Assert.AreEqual(ElementArtBuilder.Elements.Length, set.Solids.Length);
            foreach (var material in set.Solids) Assert.IsNotNull(material);
            foreach (var piece in set.Pieces)
            {
                Assert.IsNotNull(piece.Material, piece.Name);
                if (piece.Kind != ElementArtSet.Decal) Assert.IsNotNull(piece.Mesh, piece.Name);
            }
            foreach (var sheet in set.Sheets) Assert.IsNotNull(sheet.Material, sheet.Name);
            Assert.IsNotNull(set.ShadowFigure);
        }

        [Test]
        public void TheTurnFromAKitPieceMeetsTheWayItIsAsked()
        {
            // A piece drawn toward 225 degrees, asked to face +Z, is turned by -225; one asked at a fixed 180 is as its panel was.
            Assert.AreEqual(-225f, ElementArt.Turn(UnityEngine.Vector3.forward), 0.001f);
            Assert.AreEqual(90f, ElementArt.Turn(UnityEngine.Vector3.right, 0f), 0.001f);
            Assert.AreEqual(180f, ElementArt.Turn(UnityEngine.Vector3.zero), 0.001f);
        }
    }
}

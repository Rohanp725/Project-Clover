using System.IO;
using System.Linq;
using Clover.EditorTools;
using NUnit.Framework;

namespace Clover.Tests
{
    /// <summary>
    /// The project's rules as tests. They call the same scans as Clover > Checks > Run All, so a convention broken anywhere
    /// in Assets/_Project fails the EditMode run that every session ends with.
    /// </summary>
    public class ConventionsTests
    {
        [Test]
        public void RuntimeCodeUsesNoEditorLegacyInputOrBuiltInRpApis()
        {
            var problems = ProjectChecks.RuntimeCodeProblems();
            Assert.That(problems, Is.Empty, string.Join("\n", problems));
        }

        [Test]
        public void EveryArtAndAudioFileHasAManifestRow()
        {
            var problems = ProjectChecks.UnlistedAssets();
            Assert.That(problems, Is.Empty, "Add a row to Assets/_Project/ASSETS.csv for:\n" + string.Join("\n", problems));
        }

        [Test]
        public void NoManifestRowPointsAtAMissingFile()
        {
            var problems = ProjectChecks.StaleManifestRows();
            Assert.That(problems, Is.Empty, string.Join("\n", problems));
        }

        [Test]
        public void NoMaterialHasABrokenShader()
        {
            var problems = ProjectChecks.BrokenMaterials();
            Assert.That(problems, Is.Empty, string.Join("\n", problems));
        }

        [Test]
        public void NoPrefabHasAMissingScript()
        {
            var problems = ProjectChecks.MissingScriptsInPrefabs();
            Assert.That(problems, Is.Empty, string.Join("\n", problems));
        }

        [Test]
        public void AssemblyDefinitionsAreInPlace()
        {
            string[] expected =
            {
                "Assets/_Project/Scripts/Runtime/Clover.Runtime.asmdef",
                "Assets/_Project/Scripts/Editor/Clover.Editor.asmdef",
                "Assets/_Project/Scripts/Tests/EditMode/Clover.Tests.EditMode.asmdef",
                "Assets/_Project/Scripts/Tests/PlayMode/Clover.Tests.PlayMode.asmdef",
            };
            var missing = expected.Where(p => !File.Exists(p)).ToList();
            Assert.That(missing, Is.Empty, string.Join("\n", missing));
        }

        [Test]
        public void ManifestHasTheExpectedHeader()
        {
            Assert.That(File.Exists(ProjectChecks.ManifestPath), Is.True, ProjectChecks.ManifestPath + " is missing; run Clover > Setup > Ensure Project Layout.");
            string header = File.ReadLines(ProjectChecks.ManifestPath).FirstOrDefault() ?? "";
            Assert.That(header.Trim(), Is.EqualTo(ProjectSetup.ManifestHeader));
        }
    }
}

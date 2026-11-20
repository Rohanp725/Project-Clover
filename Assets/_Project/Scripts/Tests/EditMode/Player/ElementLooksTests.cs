using System.Linq;
using Clover.EditorTools;
using Clover.Gameplay;
using Clover.Player.Forms;
using NUnit.Framework;
using UnityEngine;
using L = Clover.Player.Forms.ElementLookSet;

namespace Clover.Tests
{
    /// <summary>
    /// The elements' second-pass looks (the Blender worker's batch 1): the keys ease as the recipe says, the shells climb
    /// band by band in the look-dev's times (Chill snaps on and holds, Rime builds slowly, Temper climbs behind its front),
    /// and once built every look the game asks for is there, within the pool's size, with no rings round the player in
    /// Temper (the shader draws its front).
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

        [Test]
        public void EveryLookTheGameAsksForIsBuilt()
        {
            var set = ElementLookBuilder.Set();
            if (set == null) Assert.Ignore("The element looks have not been built; run Clover > Build > Element Looks.");
            Assert.AreEqual(14, set.Looks.Length, "the fourteen looks of batch 1");
            foreach (AfflictionKind kind in System.Enum.GetValues(typeof(AfflictionKind)))
            {
                string key = AfflictionArt.LookOf(kind);
                if (key != null) Assert.GreaterOrEqual(set.Find(key), 0, $"{kind} wears {key}");
            }
            for (int element = 0; element < 11; element++)
            {
                string key = SurgeArt.LookOf(element);
                if (key != null) Assert.GreaterOrEqual(set.Find(key), 0, key);
            }
            Assert.GreaterOrEqual(set.Find(ReactionDesigns.SwordLook), 0);
            Assert.GreaterOrEqual(set.Find(ReactionDesigns.RustLook), 0);
            foreach (var look in set.Looks)
            {
                Assert.LessOrEqual(look.PartCount, ElementLooks.MostParts, look.Key);
                Assert.Greater(look.PartCount, 0, look.Key);
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
        }
    }
}

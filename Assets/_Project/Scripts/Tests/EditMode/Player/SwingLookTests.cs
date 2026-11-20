using System.Collections.Generic;
using System.Linq;
using Clover.EditorTools;
using Clover.Player;
using Clover.Player.Forms;
using Clover.Player.Swings;
using NUnit.Framework;
using UnityEditor;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>
    /// The swing looks (the owner's note of 2026-11-19): the ribbon's sums keep their promises (a trail bends round the
    /// swing, fades to nothing smoothly, fades in with the tip's speed), every weapon has a blade line and the builder
    /// measures one off each model, and every element has a look that moves with the swing and adds nothing to the
    /// blade but Life's thorns and Metal's glint.
    /// </summary>
    public class SwingLookTests
    {
        private readonly List<GameObject> made = new List<GameObject>();

        [TearDown]
        public void TearDown()
        {
            foreach (var thing in made)
                if (thing != null) Object.DestroyImmediate(thing);
            made.Clear();
        }

        // ---- The ribbon's sums.

        [Test]
        public void TheFadeReachesNothingSmoothlyAndNeverRisesOnTheWay()
        {
            Assert.AreEqual(1f, RibbonMath.Fade(0f), 1e-5f);
            Assert.AreEqual(0f, RibbonMath.Fade(1f), 1e-5f);
            float last = 1f;
            for (int i = 1; i <= 100; i++)
            {
                float now = RibbonMath.Fade(i / 100f);
                Assert.LessOrEqual(now, last + 1e-6f, $"the fade rose at {i / 100f}");
                last = now;
            }
            // No hard cut: just before its end a trail is almost gone, so nothing visible is cut off when it is let go.
            Assert.Less(RibbonMath.Fade(0.97f), 0.01f);
            Assert.Greater(RibbonMath.Fade(0.5f), 0.4f);
        }

        [Test]
        public void AFrozenTrailHangsWholeThenFades()
        {
            Assert.AreEqual(1f, RibbonMath.Fade(0.3f, 0.55f), 1e-5f);
            Assert.AreEqual(1f, RibbonMath.Fade(0.55f, 0.55f), 1e-5f);
            Assert.Less(RibbonMath.Fade(0.9f, 0.55f), 0.5f);
            Assert.AreEqual(0f, RibbonMath.Fade(1f, 0.55f), 1e-5f);
        }

        [Test]
        public void TheTipsSpeedFadesATrailIn()
        {
            Assert.AreEqual(0f, RibbonMath.Gate(1f, 2f, 7f), 1e-5f);
            Assert.AreEqual(1f, RibbonMath.Gate(9f, 2f, 7f), 1e-5f);
            float last = 0f;
            for (float speed = 2f; speed <= 7f; speed += 0.25f)
            {
                float gate = RibbonMath.Gate(speed, 2f, 7f);
                Assert.GreaterOrEqual(gate, last - 1e-6f);
                last = gate;
            }
        }

        [Test]
        public void InBetweensLieOnTheArcNotTheChord()
        {
            Vector3 pivot = Vector3.up;
            Vector3 middle = RibbonMath.Between(pivot, Vector3.right * 2f, pivot, Vector3.forward * 2f, 0.5f);
            Assert.AreEqual(2f, (middle - pivot).magnitude, 1e-3f, "half-way round a quarter turn is still at the blade's reach");
            Assert.AreEqual(15, RibbonMath.Steps(Vector3.right, Vector3.forward, 6f, 20));
            Assert.AreEqual(8, RibbonMath.Steps(Vector3.right, Vector3.forward, 6f, 8));
            Assert.AreEqual(1, RibbonMath.Steps(Vector3.right, Vector3.right, 6f, 8));
        }

        [Test]
        public void AFastSwingIsSampledRoundItsArc()
        {
            var ribbon = new Ribbon();
            ribbon.Begin(0.4f, 0f, 0f);
            Vector3 pivot = Vector3.up;
            ribbon.Push(pivot, pivot + Vector3.right * 0.5f, pivot + Vector3.right * 1.5f, Vector3.forward, 0f, 1f);
            ribbon.Push(pivot, pivot + Vector3.forward * 0.5f, pivot + Vector3.forward * 1.5f, Vector3.left, 0.05f, 1f);
            Assert.Greater(ribbon.Count, 10, "a quarter turn in one frame is filled in");
            for (int i = 0; i < ribbon.Count; i++)
            {
                ribbon.Get(i, out var inner, out var outer, out _, out _, out _);
                Assert.AreEqual(0.5f, (inner - pivot).magnitude, 1e-3f);
                Assert.AreEqual(1.5f, (outer - pivot).magnitude, 1e-3f);
            }
        }

        [Test]
        public void ARibbonKeepsItsTimeAndNoMore()
        {
            var ribbon = new Ribbon();
            ribbon.Begin(0.3f, 0f, 0f);
            for (int i = 0; i < 400; i++) ribbon.Push(Vector3.zero, Vector3.right * 0.1f, Vector3.right, Vector3.forward, i * 0.001f, 1f);
            Assert.AreEqual(Ribbon.Capacity, ribbon.Count, "the ring gives way at its capacity");
            ribbon.Stop(0.4f);
            ribbon.Trim(0.5f);
            Assert.Greater(ribbon.Count, 0);
            Assert.IsTrue(ribbon.Alive(0.5f));
            ribbon.Trim(0.75f);
            Assert.AreEqual(0, ribbon.Count);
            Assert.IsFalse(ribbon.Alive(0.75f));
        }

        [Test]
        public void ABuiltTrailIsWholeAtTheBladeAndFadesToItsTail()
        {
            var ribbon = new Ribbon();
            var layer = SwingLooks.Fire.Layers[0];
            ribbon.Begin(layer.Seconds, layer.Lag, layer.Hold);
            Vector3 pivot = Vector3.up;
            for (int i = 0; i <= 10; i++)
            {
                Vector3 way = Quaternion.Euler(0f, i * 9f, 0f) * Vector3.right;
                ribbon.Push(pivot, pivot + way * 0.3f, pivot + way * 1.2f, Vector3.Cross(Vector3.up, way), i * 0.03f, 1f);
            }
            var vertices = new Vector3[Ribbon.Capacity * 2];
            var uvs = new Vector2[vertices.Length];
            var colours = new Color[vertices.Length];
            int count = ribbon.Build(layer, 0.3f, vertices, uvs, colours, out var bounds);
            Assert.AreEqual(ribbon.Count * 2, count);
            Assert.Greater(colours[count - 1].a, 0.9f, "the newest part, at the blade, is whole");
            Assert.Less(colours[0].a, colours[count - 1].a, "the oldest part has faded");
            Assert.AreEqual(layer.Style.U1, uvs[count - 1].x, 1e-3f, "U runs to its head at the blade");
            Assert.AreEqual(0.9f, (vertices[count - 1] - vertices[count - 2]).magnitude, 0.05f, "the newest pair spans the swept part of the line");
            Assert.IsTrue(bounds.Contains(vertices[count - 1]));
            // Fire licks upward once left: the oldest part has risen.
            Assert.Greater(vertices[0].y, vertices[count - 1].y);
        }

        [Test]
        public void AFrozenTrailIsWholeUntilItShatters()
        {
            var layer = SwingLooks.Ice.Layers[0];
            var ribbon = new Ribbon();
            ribbon.Begin(layer.Seconds, layer.Lag, layer.Hold);
            for (int i = 0; i < 6; i++) ribbon.Push(Vector3.zero, new Vector3(i * 0.1f, 0f, 0.5f), new Vector3(i * 0.1f, 0f, 1.5f), Vector3.right, i * 0.02f, 1f);
            ribbon.Stop(0.12f);
            var vertices = new Vector3[64];
            var uvs = new Vector2[64];
            var colours = new Color[64];
            ribbon.Build(layer, 0.12f + layer.Seconds * layer.Hold * 0.9f, vertices, uvs, colours, out _);
            Assert.Greater(colours[0].a, 0.8f, "the oldest part still hangs whole");
            Assert.AreEqual(colours[0].a, colours[10].a, 1e-4f, "frozen all at once, not fading part by part");
            Assert.IsFalse(ribbon.Alive(0.12f + layer.Seconds + 0.01f));
        }

        [Test]
        public void AShadeTrailLagsBehindTheBlade()
        {
            var layer = SwingLooks.Dark.Layers[0];
            var ribbon = new Ribbon();
            ribbon.Begin(layer.Seconds, layer.Lag, layer.Hold);
            for (int i = 0; i < 10; i++) ribbon.Push(Vector3.zero, new Vector3(i * 0.1f, 0f, 0.5f), new Vector3(i * 0.1f, 0f, 1.5f), Vector3.right, i * 0.02f, 1f);
            var vertices = new Vector3[64];
            var uvs = new Vector2[64];
            var colours = new Color[64];
            int count = ribbon.Build(layer, 0.18f, vertices, uvs, colours, out _);
            Assert.Less(count, ribbon.Count * 2, "the newest parts, younger than the lag, are not drawn yet");
            Assert.Less(vertices[count - 2].x, 0.85f);
        }

        // ---- Blade lines.

        [Test]
        public void TheLineMeasuredOffAStraightEdgeRunsFromTheGripToThePoint()
        {
            var cloud = new List<Vector3>();
            for (int i = 0; i <= 20; i++)
            {
                cloud.Add(new Vector3(0.005f, -0.02f, 0.12f + i * 0.04f));
                cloud.Add(new Vector3(-0.005f, -0.03f, 0.12f + i * 0.04f));
            }
            var points = new Vector3[3];
            int count = BladeFit.Fit(cloud, Vector3.zero, points);
            Assert.AreEqual(2, count, "a straight edge has no bend");
            Assert.Less(points[0].z, 0.13f);
            Assert.Greater(points[1].z, 0.91f);
            Assert.AreEqual(0.8f, BladeFit.Length(points, count), 0.01f);
            Assert.AreEqual(points[1].z + 0.08f, BladeFit.Along(points, count, 1.1f).z, 0.01f, "past the point it carries on straight");
        }

        [Test]
        public void TheLineMeasuredOffACrescentKeepsItsBend()
        {
            var cloud = new List<Vector3>();
            for (int i = 0; i <= 30; i++)
            {
                float t = i / 30f;
                // The scythe's curve: off the top of the haft, out and round to its point.
                Vector2 a = new Vector2(0.02f, 0.8f), b = new Vector2(-0.7f, 1.02f), c = new Vector2(-0.92f, -0.02f);
                Vector2 at = Vector2.Lerp(Vector2.Lerp(a, b, t), Vector2.Lerp(b, c, t), t);
                cloud.Add(new Vector3(0f, at.x, at.y));
            }
            var points = new Vector3[3];
            Assert.AreEqual(3, BladeFit.Fit(cloud, Vector3.zero, points));
            Assert.Greater(points[0].z, 0.7f, "the base is where the blade leaves the haft");
            Assert.Less(points[2].z, 0.1f, "the tip is the crescent's point");
        }

        [Test]
        public void EveryWeaponHasABladeLine()
        {
            foreach (var weapon in Weapons.All)
            {
                Assert.Greater(BladeLines.CountFor(weapon), 0, $"{weapon.Name} has no blade line");
                Assert.IsNotNull(SwingShapes.For(weapon), $"{weapon.Name} has no swing shape");
            }
            // The War Fan is no Weapon yet (it has no moves), but its prefab is built and its line is ready.
            Assert.IsTrue(BladeLines.TryGet(BladeLines.FanCarry, 0, out var fan) && fan.Valid);
            Assert.AreEqual(4, BladeLines.CountFor(Weapons.Fists()), "bare hands sweep with both fists and both feet");
            Assert.AreEqual(3, BladeLines.CountFor(Weapons.GreatBow()), "the bow's two limbs and its arrow");
            Assert.AreEqual(2, BladeLines.CountFor(Weapons.ChainSickle()), "the sickle and the weight on its chain");
            foreach (var line in BladeLines.Defaults)
            {
                Assert.IsTrue(line.Valid, $"{line.Carry} {line.Part}");
                Vector3 start = line.Points[0], end = line.Points[line.Points.Length - 1];
                Assert.Less(start.magnitude, end.magnitude + 1e-4f, $"{line.Carry} {line.Part}: its base is nearer the grip than its tip");
                Assert.Greater(line.Length, 0.15f, $"{line.Carry} {line.Part}");
            }
        }

        [Test]
        public void EveryWeaponModelHasItsBladeSlotsNamed()
        {
            foreach (var spec in WeaponBuilder.Specs)
            {
                if (spec.Worn != null) continue;
                Assert.IsNotNull(spec.BladeSlots, $"{spec.Carry} names no slot to measure its blade line from");
                Assert.AreEqual(BladeLines.CountOf(spec.Carry), spec.BladeKind == BladeKind.Stave ? 2 : 1, $"{spec.Carry}: the notes' lines and the lines measured are as many");
            }
        }

        [Test]
        public void TheBuilderMeasuresALineOffEachStandIn()
        {
            foreach (var spec in WeaponBuilder.Specs)
            {
                if (spec.Worn != null || spec.StandIn == null) continue;
                var root = new GameObject(spec.Carry);
                made.Add(root);
                spec.StandIn(root.transform);
                var notes = new List<string>();
                var lines = BladeLineFit.Measure(root, spec, notes);
                Assert.AreEqual(BladeLines.CountOf(spec.Carry), lines.Count, $"{spec.Carry}: {string.Join("; ", notes)}");
                for (int n = 0; n < lines.Count; n++)
                {
                    Assert.IsTrue(lines[n].Valid, spec.Carry);
                    BladeLines.TryGet(spec.Carry, n, out var noted);
                    Vector3 tip = lines[n].Points[lines[n].Points.Length - 1], notedTip = noted.Points[noted.Points.Length - 1];
                    Assert.Less((tip - notedTip).magnitude, 0.35f, $"{spec.Carry} {noted.Part}: measured tip {tip} against the notes' {notedTip}");
                }
            }
        }

        [Test]
        public void TheBuiltPrefabsCarryTheirBladeLines()
        {
            int checkedPrefabs = 0;
            foreach (var spec in WeaponBuilder.Specs)
            {
                if (spec.Worn != null) continue;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{WeaponBuilder.Folder}/{spec.Carry}/{spec.Carry}.prefab");
                if (prefab == null) continue;
                checkedPrefabs++;
                Assert.AreEqual(BladeLines.CountOf(spec.Carry), prefab.GetComponents<BladeLine>().Length,
                    $"{spec.Carry}'s prefab has no blade line: run Clover > Weapons > Build Weapon Prefabs");
            }
            if (checkedPrefabs == 0) Assert.Ignore("No weapon prefab has been built yet; run Clover > Weapons > Build Weapon Prefabs.");
        }

        [Test]
        public void ABuiltPointOrHeadRunsAlongItsHaft()
        {
            // A thrust follows the shaft: a line across a spear's flat leaf or the sunspear's disc zigzags the trail.
            int checkedLines = 0;
            foreach (var spec in WeaponBuilder.Specs)
            {
                if (spec.Worn != null || (spec.BladeKind != BladeKind.Point && spec.BladeKind != BladeKind.Head)) continue;
                var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{WeaponBuilder.Folder}/{spec.Carry}/{spec.Carry}.prefab");
                if (prefab == null) continue;
                foreach (var line in prefab.GetComponents<BladeLine>())
                {
                    checkedLines++;
                    var points = line.Spec.Points;
                    Vector3 along = points[points.Length - 1] - points[0];
                    Assert.Greater(along.z, along.magnitude * 0.98f, $"{spec.Carry}: its line {points[0]} to {points[points.Length - 1]} runs along +Z");
                    foreach (var point in points)
                        Assert.Less(new Vector2(point.x - points[0].x, point.y - points[0].y).magnitude, 0.02f, $"{spec.Carry}: {point} is off the line's axis");
                }
            }
            if (checkedLines == 0) Assert.Ignore("No weapon prefab has been built yet; run Clover > Weapons > Build Weapon Prefabs.");
        }

        [Test]
        public void TheWarFansLineRunsThroughItsLeafToTheRim()
        {
            // Measured off the opened fan: out along the middle of the leaf, not rim end to rim end across it.
            var spec = WeaponBuilder.SpecFor(BladeLines.FanCarry);
            Assert.IsNotNull(spec, "the War Fan has a Spec in the weapon builder");
            Assert.AreEqual(BladeKind.Rim, spec.BladeKind);
            var prefab = AssetDatabase.LoadAssetAtPath<GameObject>($"{WeaponBuilder.Folder}/{spec.Carry}/{spec.Carry}.prefab");
            if (prefab == null) Assert.Ignore("The War Fan's prefab has not been built yet; run Clover > Weapons > Build Weapon Prefabs.");
            Assert.IsNotNull(prefab.GetComponentInChildren<Renderer>(), "the fan's model is in its prefab");
            var lines = prefab.GetComponents<BladeLine>();
            Assert.AreEqual(1, lines.Length);
            var points = lines[0].Spec.Points;
            Vector3 root = points[0], crown = points[points.Length - 1];
            Assert.AreEqual(0.41f, crown.z, 0.04f, $"the crown of the rim, 0.41 m ahead of the hand: {crown}");
            Assert.Less(root.z, 0.12f, $"the leaf's root by the rivet: {root}");
            Assert.Less(new Vector2(crown.x, crown.y).magnitude, 0.03f, $"the crown lies on the fan's middle: {crown}");
            Assert.Less(new Vector2(crown.x - root.x, crown.y - root.y).magnitude, 0.01f, "the line runs straight out along +Z");
        }

        // ---- The looks.

        [Test]
        public void EveryElementHasASwingLook()
        {
            foreach (var element in Elements.All)
            {
                var look = SwingLooks.For(element);
                Assert.IsNotNull(look, $"{element.Name} has no swing look");
                Assert.AreEqual(element.Index, look.Element, element.Name);
                Assert.AreEqual(element.Name, look.Name);
                Assert.Greater(look.Layers.Length, 0, $"{element.Name} leaves no trail");
                Assert.LessOrEqual(look.Layers.Length, SwingLooks.MostLayers);
                Assert.IsTrue(look.Sheath.Shown, $"{element.Name} shows nothing along a charged edge");
                Assert.IsFalse(string.IsNullOrEmpty(look.Line), element.Name);
                foreach (var layer in look.Layers)
                {
                    Assert.Greater(layer.Seconds, 0f, element.Name);
                    Assert.Greater(layer.Head.a, 0f, $"{element.Name}: its trail is seen where it leaves the blade");
                    if (layer.Form == TrailForm.Sweep) Assert.Less(layer.From, layer.To, element.Name);
                    else Assert.Greater(layer.Width, 0f, element.Name);
                }
            }
        }

        [Test]
        public void OnlyLifeGrowsOnTheWeaponAndMetalOnlyGlints()
        {
            foreach (var look in SwingLooks.All) Assert.AreEqual(look.Element == ElementSubstance.Life, look.Thorns, look.Name);
            var metal = SwingLooks.Metal;
            Assert.IsTrue(metal.Sheath.Glint, "Metal's charged edge is a glint");
            Assert.AreEqual(SwingShed.None, metal.Shed);
            foreach (var layer in metal.Layers)
                Assert.IsTrue(layer.Form == TrailForm.Strand && layer.Width <= 0.05f && layer.Seconds <= 0.1f, "Metal's swing leaves no more than the glint's streak");
            foreach (var look in SwingLooks.All)
                if (look.Element != ElementSubstance.Metal) Assert.IsFalse(look.Sheath.Glint, look.Name);
        }

        [Test]
        public void EachElementMovesAsItsElement()
        {
            Assert.Greater(SwingLooks.Fire.Layers[0].Rise, 0f, "fire licks upward");
            Assert.Greater(SwingLooks.Fire.Layers[0].Lick, 0f, "in tongues");
            Assert.IsTrue(SwingLooks.Fire.Layers[0].Style.Fire, "fire is drawn as flame, not light");
            Assert.AreEqual(SwingShed.Embers, SwingLooks.Fire.Shed);
            Assert.IsTrue(SwingLooks.Lightning.Layers.Any(layer => layer.Style.Shape == 7f && layer.Jitter > 0f), "lightning's arcs jump");
            var wind = SwingLooks.Wind.Layers[0];
            Assert.Greater(wind.Drift, 0f, "wind blows on");
            Assert.Greater(wind.Scatter, 0f, "wind comes apart");
            Assert.AreEqual(0f, wind.Tail.a, "wind fades to nothing, never cut");
            Assert.IsTrue(SwingLooks.Ice.Sheath.Teeth, "the chainsaw of ice");
            Assert.Greater(SwingLooks.Ice.Layers[0].Hold, 0f, "ice freezes in the air");
            Assert.AreEqual(SwingEnd.Shatter, SwingLooks.Ice.End);
            Assert.AreEqual(SwingEnd.Spray, SwingLooks.Water.End);
            Assert.Less(SwingLooks.Water.Layers[0].Rise, 0f, "water falls");
            Assert.IsTrue(SwingLooks.Earth.ReplacesHitArt && SwingLooks.Ice.ReplacesHitArt, "Earth's floating cubes and Ice's crack-web give way");
            Assert.Less(SwingLooks.Earth.Layers[0].Rise, 0f, "dust settles");
            Assert.Greater(SwingLooks.Dark.Layers[0].Lag, 0f, "shade lags");
            Assert.Greater(SwingLooks.Dark.Layers[0].Drain, 0f, "shade drains");
            Assert.Less(SwingLooks.Poison.Layers[1].Rise, 0f, "the mist sinks");
            Assert.AreEqual(SwingShed.Drips, SwingLooks.Poison.Shed);
            Assert.IsTrue(SwingLooks.Light.Layers[0].Style.Additive && SwingLooks.Light.Layers[0].Style.Noise == 0f, "a clean arc of light");
        }

        [Test]
        public void FlameTonguesRiseUnevenlyFromRootToPeak()
        {
            float lowest = 1f, highest = 0f;
            for (int tongue = 0; tongue < 12; tongue++)
            {
                Assert.AreEqual(0f, RibbonMath.Tongue(tongue * 6, 6f), 1e-4f, "a tongue starts at its root");
                float peak = RibbonMath.Tongue(tongue * 6 + 3, 6f);
                lowest = Mathf.Min(lowest, peak);
                highest = Mathf.Max(highest, peak);
                Assert.Greater(peak, RibbonMath.Tongue(tongue * 6 + 1, 6f), "and stands tallest in its middle");
            }
            Assert.Greater(highest - lowest, 0.2f, "each tongue its own height");
        }

        [Test]
        public void ThrownRocksFallBackToTheGround()
        {
            Vector3 from = new Vector3(0f, 0.15f, 0f), velocity = new Vector3(1.5f, 4.5f, 0f);
            float landing = FormFx.TossLanding(from, velocity, 0f);
            Assert.Greater(landing, 0.5f);
            Assert.AreEqual(0f, FormFx.TossAt(from, velocity, landing).y, 1e-3f, "it comes down where the ground is");
            Assert.Greater(FormFx.TossAt(from, velocity, landing * 0.5f).y, 0.8f, "and goes up first");
        }

        [Test]
        public void EveryWeaponShapeIsSound()
        {
            foreach (var shape in SwingShapes.All)
            {
                Assert.Less(shape.SweepFrom, shape.SweepTo, shape.Weapon);
                Assert.Less(shape.LeastSpeed, shape.FullSpeed, shape.Weapon);
                Assert.Greater(shape.Width, 0f, shape.Weapon);
                Assert.Greater(shape.Life, 0f, shape.Weapon);
                Assert.IsFalse(string.IsNullOrEmpty(shape.Note), shape.Weapon);
            }
            Assert.AreSame(SwingShapes.Katana, SwingShapes.For(Weapons.StormKatana()));
            Assert.AreSame(SwingShapes.Spear, SwingShapes.For(Weapons.SunSpear()));
            Assert.AreSame(SwingShapes.Fists, SwingShapes.For(Weapons.Fists()));
            Assert.Greater(SwingShapes.Hammer.Width, SwingShapes.Katana.Width, "the hammer's smear is heavier than the katana's arc");
            Assert.IsTrue(SwingShapes.Hammer.GroundImpact);
        }
    }
}

using Clover.EditorTools;
using Clover.World;
using NUnit.Framework;
using UnityEngine;

namespace Clover.Tests
{
    /// <summary>The second pass's ground meshes and the shortcuts' rules (docs/design/fire_levels.md, sections 4 and 6).</summary>
    public class SunCityGroundTests
    {
        private static float TopArea(Mesh mesh)
        {
            var vertices = mesh.vertices;
            var triangles = mesh.GetTriangles(0);
            float area = 0f;
            for (int i = 0; i < triangles.Length; i += 3)
            {
                Vector3 a = vertices[triangles[i]], b = vertices[triangles[i + 1]], c = vertices[triangles[i + 2]];
                var cross = Vector3.Cross(b - a, c - a);
                Assert.Greater(cross.y, -0.0001f, "every top triangle faces up");
                area += cross.magnitude * 0.5f;
            }
            return area;
        }

        [Test]
        public void APlateauOfAnLShapeIsFlatOnTopCoversItsOutlineAndCanBeStoodOn()
        {
            var outline = new[] { new Vector2(0f, 0f), new Vector2(0f, 10f), new Vector2(4f, 10f), new Vector2(4f, 4f), new Vector2(10f, 4f), new Vector2(10f, 0f) };
            var plateau = SunCityGround.Plateau("Test Plateau", null, outline, 3f, -1f, null, null, null, 0f);
            try
            {
                var mesh = plateau.GetComponent<MeshFilter>().sharedMesh;
                Assert.AreEqual(4f * 10f + 6f * 4f, TopArea(mesh), 0.01f, "the top covers the L exactly, the notch left open");
                Assert.AreEqual(3f, mesh.bounds.max.y, 0.001f);
                Assert.AreEqual(-1f, mesh.bounds.min.y, 0.001f);
                Assert.AreSame(mesh, plateau.GetComponent<MeshCollider>().sharedMesh, "what is drawn is what is stood on");
            }
            finally { Object.DestroyImmediate(plateau); }
        }

        [Test]
        public void ABrokenEdgeWandersButTheTopStillCoversRoughlyTheOutline()
        {
            var plateau = SunCityGround.Plateau("Test Bank", null, SunCityGround.Rect(0f, 30f, 0f, 20f), 3f, -1f, null, null, null, 0.5f, 12f, 0.75f);
            try
            {
                var mesh = plateau.GetComponent<MeshFilter>().sharedMesh;
                Assert.AreEqual(600f, TopArea(mesh), 600f * 0.06f);
                // A face leaning back 12 degrees over 4 m stands its foot 0.85 m further out than its lip.
                Assert.Greater(mesh.bounds.max.x, 30.6f);
                Assert.Less(mesh.bounds.max.x, 31.5f);
            }
            finally { Object.DestroyImmediate(plateau); }
        }

        [Test]
        public void ARampRisesFromItsFootToItsHeadAtTheSlopeAsked()
        {
            var ramp = SunCityGround.Ramp("Test Ramp", null, new Vector3(0f, 10f, 318f), new Vector3(0f, 13f, 346f), 7f, null, null);
            try
            {
                var collider = ramp.GetComponent<MeshCollider>();
                Assert.IsTrue(collider.Raycast(new Ray(new Vector3(0f, 20f, 332f), Vector3.down), out var hit, 40f));
                Assert.AreEqual(11.5f, hit.point.y, 0.05f, "half way up a 3 m rise");
                float degrees = Mathf.Atan2(3f, 28f) * Mathf.Rad2Deg;
                Assert.Less(degrees, 8f, "a ramp fought on is no steeper than 8 degrees");
            }
            finally { Object.DestroyImmediate(ramp); }
        }

        [Test]
        public void ABarredDoorKnowsWhichSideItsBarIsOn()
        {
            var holder = new GameObject("Door");
            try
            {
                holder.transform.position = new Vector3(-24f, 13f, 350f);
                var door = holder.AddComponent<BarredDoor>();
                door.BarSide = Vector3.right;
                Assert.IsTrue(door.OnBarSide(new Vector3(-22.4f, 13f, 350f)), "the terrace side lifts the bar");
                Assert.IsFalse(door.OnBarSide(new Vector3(-25.6f, 13f, 350f)), "the haul-ramp side is barred");
            }
            finally { Object.DestroyImmediate(holder); }
        }
    }
}

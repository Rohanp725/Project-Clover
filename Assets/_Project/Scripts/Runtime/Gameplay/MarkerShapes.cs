using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>The flat shapes drawn on the ground to show where an attack will land.</summary>
    public static class MarkerShapes
    {
        /// <summary>A flat fan lying on the ground, pointing along +Z and facing up. 360 degrees makes a disc.</summary>
        public static Mesh Sector(float radius, float arcDegrees)
        {
            const int Segments = 32;
            var vertices = new Vector3[Segments + 2];
            var uvs = new Vector2[Segments + 2];
            var triangles = new int[Segments * 3];
            vertices[0] = Vector3.zero;
            uvs[0] = new Vector2(0.5f, 0.5f);
            for (int i = 0; i <= Segments; i++)
            {
                float angle = Mathf.Deg2Rad * Mathf.Lerp(-arcDegrees * 0.5f, arcDegrees * 0.5f, i / (float)Segments);
                vertices[i + 1] = new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
                uvs[i + 1] = new Vector2(0.5f + Mathf.Sin(angle) * 0.5f, 0.5f + Mathf.Cos(angle) * 0.5f);
            }
            for (int i = 0; i < Segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
            var mesh = new Mesh { name = "Attack Marker" };
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}

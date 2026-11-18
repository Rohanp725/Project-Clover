using System.Collections.Generic;
using System.Linq;
using Clover.World;
using UnityEditor;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// Two passes a builder runs over its finished place, before <see cref="SceneryMerge.Prepare"/>:
    /// <see cref="Settle"/> keeps what lies on a surface (blown sand, stains, loose brick and sherds, rubble, what people
    /// kept on their roofs, a fire on a roof) wholly on the top it lies on and out of the walls beside it, moving it in,
    /// making it smaller, or taking it away when there is no room; and <see cref="JoinFades"/> makes whatever lies on a
    /// box that thins out (a house's roof and everything on it) thin out with that box, so a house in front of the player
    /// goes see-through all of a piece and not as walls round a solid roof.
    /// </summary>
    public static class SceneryFit
    {
        /// <summary>What lies on a surface, by the name of the object that holds it.</summary>
        private static readonly HashSet<string> LyingNames = new HashSet<string>
        {
            "ground_dust_drift", "ground_dust_patch", "ground_bricks", "ground_sherds", "rubble",
            "Silt Pan", "Ground Patch", "Trodden Earth", "Dry Stain", "Stone", "Loose Brick", "Roof Fire",
            // Standing, not lying, but grown at the foot of the banks they pushed into the banks' faces.
            "reeds", "papyrus",
        };

        private static readonly RaycastHit[] hits = new RaycastHit[32];
        private static readonly Collider[] touching = new Collider[16];

        /// <summary>
        /// Moves, shrinks or removes each piece that lies on a surface until every point of its underside rests on that
        /// surface (or is hidden inside something solid) and, if it stands up from it, none of it is inside a wall.
        /// Returns how many were moved, made smaller and taken away.
        /// </summary>
        public static (int moved, int shrunk, int removed) Settle(Transform root)
        {
            Physics.SyncTransforms();
            var holders = new List<Transform>();
            foreach (var t in root.GetComponentsInChildren<Transform>(false))
                if (IsLying(t, root)) holders.Add(t);
            int moved = 0, shrunk = 0, removed = 0;
            var gone = new Dictionary<string, int>();
            foreach (var holder in holders)
            {
                if (holder == null) continue;
                var result = SettleOne(holder);
                if (result == 1) moved++;
                else if (result == 2) shrunk++;
                else if (result == 3)
                {
                    gone[holder.name] = gone.TryGetValue(holder.name, out int n) ? n + 1 : 1;
                    Object.DestroyImmediate(holder.gameObject);
                    removed++;
                }
                Physics.SyncTransforms();
            }
            if (gone.Count > 0) Debug.Log("[Clover.Build] Taken away for want of room: " + string.Join(", ", gone.Select(pair => pair.Key + " " + pair.Value)));
            return (moved, shrunk, removed);
        }

        private static bool IsLying(Transform t, Transform root)
        {
            if (t == root || t.GetComponentInChildren<MeshRenderer>() == null) return false;
            bool named = LyingNames.Contains(t.name)
                || (t.name.StartsWith("prop_") && t.name != "prop_ladder" && Mathf.Abs(t.lossyScale.x) < 1.6f);
            if (!named) return false;
            // A holder inside another lying holder moves with it.
            for (var up = t.parent; up != null && up != root; up = up.parent)
            {
                if (LyingNames.Contains(up.name)) return false;
                // Under a behaviour (a wall that falls, a gift, a jar that breaks): it is that behaviour's to place.
                foreach (var behaviour in up.GetComponents<MonoBehaviour>())
                    if (behaviour != null && !(behaviour is Fadeable)) return false;
            }
            return true;
        }

        // 0 left as it was, 1 moved, 2 made smaller (and perhaps moved), 3 no room anywhere near.
        private static int SettleOne(Transform holder)
        {
            var renderers = holder.GetComponentsInChildren<MeshRenderer>(false).Where(r => r.enabled && r.GetComponent<MeshFilter>() != null && r.GetComponent<MeshFilter>().sharedMesh != null).ToArray();
            if (renderers.Length == 0) return 0;
            var points = new List<Vector3>();
            foreach (var renderer in renderers)
            {
                var mesh = renderer.GetComponent<MeshFilter>().sharedMesh;
                var vertices = mesh.isReadable ? mesh.vertices : Corners(mesh.bounds);
                int step = Mathf.Max(1, vertices.Length / 160);
                for (int i = 0; i < vertices.Length; i += step) points.Add(renderer.transform.TransformPoint(vertices[i]));
            }
            float low = points.Min(p => p.y), high = points.Max(p => p.y);
            var under = points.Where(p => p.y <= low + 0.12f).ToArray();
            // What stands up out of its surface may not go into a wall; what lies flat may slip under one (hidden there).
            bool upright = high - low > 0.25f;
            var body = upright ? points.Where(p => p.y > low + 0.12f).ToArray() : new Vector3[0];
            Vector3 pivot = holder.position;
            float top = TopBelow(pivot.x, pivot.z, high + 0.4f, out var support);
            if (support == null) return 3;

            bool flat = !upright;
            var scales = flat ? new[] { 1f, 0.85f, 0.7f, 0.55f } : new[] { 1f };
            var rings = new[] { 0f, 0.1f, 0.2f, 0.35f, 0.5f, 0.75f, 1f, 1.4f, 2f, 2.8f, 3.6f };
            var tries = new List<(float cost, float scale, Vector3 offset)>();
            foreach (float scale in scales)
                foreach (float ring in rings)
                {
                    int around = ring == 0f ? 1 : 16;
                    for (int k = 0; k < around; k++)
                    {
                        float angle = k * Mathf.PI * 2f / around;
                        tries.Add((ring + (1f - scale) * 2.5f, scale, new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * ring));
                    }
                }
            foreach (var (cost, scale, offset) in tries.OrderBy(t => t.cost))
            {
                if (!Fits(under, body, pivot, scale, offset, support)) continue;
                if (cost == 0f) return 0;
                holder.position += offset;
                if (scale < 1f) holder.localScale = new Vector3(holder.localScale.x * scale, holder.localScale.y, holder.localScale.z * scale);
                return scale < 1f ? 2 : 1;
            }
            return 3;
        }

        private static Vector3 Moved(Vector3 p, Vector3 pivot, float scale, Vector3 offset) =>
            new Vector3(pivot.x + (p.x - pivot.x) * scale + offset.x, p.y, pivot.z + (p.z - pivot.z) * scale + offset.z);

        private static bool Fits(Vector3[] under, Vector3[] body, Vector3 pivot, float scale, Vector3 offset, Collider support)
        {
            foreach (var p in under)
                if (!Held(Moved(p, pivot, scale, offset))) return false;
            float foot = under.Length > 0 ? under.Min(p => p.y) : 0f;
            foreach (var p in body)
            {
                var at = Moved(p, pivot, scale, offset);
                if (InWall(at, support) || InRise(at, foot)) return false;
            }
            return true;
        }

        // Something solid within a hand's breadth under the point, or the point hidden inside something solid.
        private static bool Held(Vector3 p)
        {
            // A clutter piece's underside stands up to a few centimetres off its roof, and a heap of rubble sits a little
            // into the ground: anything within reach above or under the point holds it.
            if (TopBelow(p.x, p.z, p.y + 0.8f, out _) >= p.y - 0.3f) return true;
            return Solid(p + Vector3.up * 0.03f);
        }

        // More than a few centimetres inside a wall, a building or a solid piece (not the surface it stands on, and not the
        // undrawn body a drawn prop is bumped into by).
        private static bool InWall(Vector3 p, Collider support)
        {
            int count = Physics.OverlapSphereNonAlloc(p, 0.02f, touching, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var c = touching[i];
                if (c == support || Ignored(c)) continue;
                if (c is BoxCollider box) { if (Depth(box, p) > 0.08f) return true; }
                else if (!(c is MeshCollider)) return true;
            }
            return false;
        }

        // Inside the face of a rise beside the piece (a bank, a terrace): the made ground's top stands over the point there,
        // well above the piece's own foot.
        private static bool InRise(Vector3 p, float foot)
        {
            float top = TopBelow(p.x, p.z, p.y + 40f, out var which);
            return which is MeshCollider && (which.bounds.size.x > 6f || which.bounds.size.z > 6f) && top > p.y + 0.1f && top > foot + 0.8f;
        }

        private static bool Solid(Vector3 p)
        {
            int count = Physics.OverlapSphereNonAlloc(p, 0.02f, touching, ~0, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
                if (!(touching[i] is CharacterController)) return true;
            return false;
        }

        private static bool Ignored(Collider c)
        {
            if (c is CharacterController) return true;
            var drawn = c.GetComponent<Renderer>();
            return drawn != null && !drawn.enabled;
        }

        private static float Depth(BoxCollider box, Vector3 point)
        {
            var t = box.transform;
            Vector3 local = t.InverseTransformPoint(point) - box.center;
            Vector3 half = box.size * 0.5f, scale = t.lossyScale;
            float dx = (half.x - Mathf.Abs(local.x)) * Mathf.Abs(scale.x);
            float dy = (half.y - Mathf.Abs(local.y)) * Mathf.Abs(scale.y);
            float dz = (half.z - Mathf.Abs(local.z)) * Mathf.Abs(scale.z);
            if (dx <= 0f || dy <= 0f || dz <= 0f) return 0f;
            return Mathf.Min(dx, Mathf.Min(dy, dz));
        }

        private static float TopBelow(float x, float z, float from, out Collider which)
        {
            which = null;
            int count = Physics.RaycastNonAlloc(new Vector3(x, from, z), Vector3.down, hits, from + 60f, ~0, QueryTriggerInteraction.Ignore);
            float best = -999f;
            for (int i = 0; i < count; i++)
            {
                if (hits[i].collider is CharacterController) continue;
                if (hits[i].point.y > best) { best = hits[i].point.y; which = hits[i].collider; }
            }
            return best;
        }

        private static Vector3[] Corners(Bounds b) => new[]
        {
            b.min, b.max, new Vector3(b.min.x, b.min.y, b.max.z), new Vector3(b.max.x, b.min.y, b.min.z),
            new Vector3(b.min.x, b.max.y, b.max.z), new Vector3(b.max.x, b.max.y, b.min.z),
        };

        /// <summary>
        /// Every drawn piece whose foot lies on the top of a box that thins out (a house's roof tiles, what stands on
        /// its roof, a parapet, sand blown up there) is added to the pieces that box thins out with. Returns how many.
        /// </summary>
        public static int JoinFades(Transform root)
        {
            Physics.SyncTransforms();
            var already = new HashSet<Renderer>();
            foreach (var fadeable in root.GetComponentsInChildren<Fadeable>(true))
                foreach (var shown in fadeable.AlsoShown)
                    if (shown != null) already.Add(shown);
            var joined = new Dictionary<Fadeable, List<Renderer>>();
            int count = 0;
            foreach (var renderer in root.GetComponentsInChildren<MeshRenderer>(false))
            {
                if (!renderer.enabled || already.Contains(renderer) || renderer.GetComponent<Fadeable>() != null) continue;
                // Made when the scene opens and never saved (laid paving, merged scenery): nothing saved may point at it.
                if ((renderer.gameObject.hideFlags & HideFlags.DontSave) != 0) continue;
                if (renderer.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly) continue;
                if (renderer.sharedMaterials.Any(material => material == null || !material.HasProperty("_Fade"))) continue;
                var b = renderer.bounds;
                if (b.size.y > 4f || (b.size.x > 14f && b.size.z > 14f)) continue;
                TopBelow(b.center.x, b.center.z, b.max.y + 0.3f, out var support);
                if (support == null) continue;
                var fades = support.GetComponent<Fadeable>();
                if (fades == null) continue;
                float top = support.bounds.max.y;
                if (top < b.min.y - 0.35f || top > b.min.y + 0.35f) continue;
                if (!joined.TryGetValue(fades, out var list)) joined[fades] = list = new List<Renderer>();
                list.Add(renderer);
                count++;
            }
            foreach (var pair in joined)
            {
                pair.Key.AlsoFade(pair.Key.AlsoShown.Where(shown => shown != null).Concat(pair.Value).ToArray());
                EditorUtility.SetDirty(pair.Key);
            }
            return count;
        }
    }
}

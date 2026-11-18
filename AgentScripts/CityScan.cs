using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using Clover.World;
using UnityEngine;

// Edit mode, Sun City open: `script run --file AgentScripts/CityScan.cs --entry CityScan.Run --args '["before"]'`.
// Finds, in what the scene draws and what it stands on:
//  1. dressing that hangs off the surface it lies on (drifts, patches, stains, litter, roof clutter);
//  2/9. scenery lying on top of a box that thins out without thinning out with it (roof tiles, clutter, dressing);
//  3. scenery floating in the body's space over ground the player can reach (0.35 to 1.9 m up), with nothing solid there;
//  4. scenery driven more than 0.2 m into a wall's or building's box, or into the face of a rise;
//  7/8. every place the player can reach and walk off into nothing (no floor under them, ever), and drops of more than
//       2 m off the edge of a raised floor.
// Writes review/checks/cityfix/scan_<label>.md and returns the counts.
public static class CityScan
{
    private struct Piece
    {
        public string Name;
        public Mesh Mesh;
        public Matrix4x4 Matrix;
        public int Group; // fade group of a merged piece; -2 for a renderer drawn by itself
        public Renderer Own;
        public Bounds Bounds;
    }

    private const float Cell = 0.5f;
    private const float MinX = -110f, MaxX = 90f, MinZ = -40f, MaxZ = 640f;
    private static int nx, nz;
    private static List<float>[] surfaces;
    private static Dictionary<long, byte> reached;
    private static HashSet<Collider> ignored;

    public static string Run(string label)
    {
        Physics.SyncTransforms();
        var sb = new StringBuilder();
        var world = GameObject.Find("World");
        if (world == null) return "no World";
        ignored = new HashSet<Collider>();
        foreach (var c in Object.FindObjectsByType<Collider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
            if (c is CharacterController || c.isTrigger || c.GetComponentInParent<BarredDoor>() != null) ignored.Add(c);

        var pieces = Pieces(world.transform, out var groups);
        var fadeIndex = new Dictionary<Fadeable, int>();
        for (int i = 0; i < groups.Length; i++) if (groups[i] != null) fadeIndex[groups[i]] = i;

        // ---------------------------------------------------------------- reach (7, 8)
        Reach(out var voids, out var drops, out int nodes);
        var lines = new List<string>();

        // ---------------------------------------------------------------- 1. overhanging dressing
        var hanging = new List<string>();
        var unfaded = new List<string>();
        var floating = new List<string>();
        var walled = new List<string>();
        var rises = new List<string>();
        int houseOverlaps = 0;
        // Walls and buildings: the boxes that thin out, and the kit's solid pieces.
        var wallColliders = Object.FindObjectsByType<BoxCollider>(FindObjectsInactive.Exclude, FindObjectsSortMode.None)
            .Where(c => !c.isTrigger && !ignored.Contains(c) && c.bounds.size.y >= 1.2f && c.GetComponentInParent<Ladder>() == null
                && (c.GetComponent<Fadeable>() != null || c.name == "Model")).ToArray();
        foreach (var piece in pieces)
        {
            var b = piece.Bounds;
            if (b.size.x > 40f || b.size.z > 40f) continue;
            if (piece.Own != null && piece.Own.GetComponentInParent<Ladder>() != null) continue;
            var points = Points(piece, 160);
            // Roof tiles are laid to their box's edges, their spouts and eaves (over the alley's recesses) standing out by design.
            if (IsDressing(piece) && !piece.Name.Contains("roof_flat"))
            {
                // Each point of its underside: how far it reaches past what holds it up.
                int hung = 0;
                float worst = 0f;
                var middle = new Vector3(b.center.x, 0f, b.center.z);
                foreach (var p in points)
                {
                    if (p.y > b.min.y + 0.12f || Held(p, b.max.y + 0.4f)) continue;
                    var flat = new Vector3(p.x, 0f, p.z);
                    float reach = 0f, length = (flat - middle).magnitude;
                    var inward = length > 0.001f ? (middle - flat) / length : Vector3.zero;
                    while (reach < length && !Held(p + inward * reach, b.max.y + 0.4f)) reach += 0.05f;
                    if (reach > 0.1f) { hung++; worst = Mathf.Max(worst, reach); }
                }
                if (hung > 0) hanging.Add($"{piece.Name} at {V(b.center)}: {hung} points reach up to {worst:0.00} m past what holds them up");
            }
            // 2/9: on top of a box that thins out?
            if (b.size.y < 4f)
            {
                var under = SupportUnder(new Vector3(b.center.x, b.min.y, b.center.z), b.max.y + 0.3f);
                if (under != null && under.bounds.max.y <= b.min.y + 0.25f && under.bounds.max.y >= b.min.y - 0.6f)
                {
                    var fades = under.GetComponent<Fadeable>();
                    if (fades != null)
                    {
                        // In a group of its own (an upper room's fronts on the roof under it) it thins out with its own building.
                        bool ok = piece.Group >= 0 || (piece.Own != null && KeptWith(fades, piece.Own));
                        if (!ok) unfaded.Add($"{piece.Name} at {V(b.center)} on {Path(under.transform)} (group {piece.Group})");
                    }
                }
            }
            // 3: in the body's space over reachable ground where a body can stand, and not inside anything solid.
            {
                int found = 0;
                Vector3 at = Vector3.zero;
                float floor = 0f;
                foreach (var p in points)
                {
                    int ci = CellOf(p.x, p.z);
                    if (ci < 0) continue;
                    foreach (float h in Surf(ci))
                    {
                        if (!IsReached(ci, h)) continue;
                        if (p.y < h + 0.3f || p.y > h + 1.8f) continue;
                        // Resting on this floor (its foot on it) is something stood on the ground, not floating.
                        if (b.min.y < h + 0.3f) continue;
                        if (Physics.CheckSphere(p, 0.04f, ~0, QueryTriggerInteraction.Ignore)) continue;
                        // A body standing here, its side at the point: is there room for it?
                        if (Blocked(new Vector3(p.x, h + 0.45f, p.z), new Vector3(p.x, h + 1.4f, p.z), 0.3f)) continue;
                        found++;
                        at = p;
                        floor = h;
                    }
                }
                if (found > 2) floating.Add($"{piece.Name} at {V(b.center)} (bottom {b.min.y:0.00}): {found} points in the body's space over the floor at {floor:0.00}, e.g. {V(at)}");
            }
            // 4: into a wall's box.
            bool house = piece.Name.Contains("house_") || piece.Name.Contains("roof_flat") || piece.Name.Contains("stone_wall") || piece.Name.Contains("prop_ladder");
            // What lies flat may slip under a wall (hidden there), as a floor does; only what stands up out of its surface counts.
            bool flatPiece = b.size.y < 0.25f;
            foreach (var wall in wallColliders)
            {
                if (flatPiece || !wall.bounds.Intersects(b)) continue;
                if (piece.Own != null && (piece.Own.transform == wall.transform || piece.Own.transform.IsChildOf(wall.transform) || wall.transform.IsChildOf(piece.Own.transform.parent ?? piece.Own.transform))) continue;
                var fades = wall.GetComponent<Fadeable>();
                if (fades != null && piece.Group >= 0 && groups[piece.Group] == fades) continue;
                // A kit piece's own box (the barque, a column's shaft): the piece fills it.
                var wb = wall.bounds;
                if (b.Contains(wb.center) && b.size.x >= wb.size.x * 0.6f && b.size.z >= wb.size.z * 0.6f) continue;
                if (wb.Contains(b.center) && wb.size.x <= b.size.x * 1.6f && wb.size.z <= b.size.z * 1.6f && wb.size.y <= b.size.y * 1.6f) continue;
                if (piece.Name.Contains("statue_head") && wall.transform.parent != null && wall.transform.parent.name.StartsWith("statue")) continue;
                float deepest = 0f;
                foreach (var p in points) deepest = Mathf.Max(deepest, Depth(wall, p));
                if (deepest <= 0.2f) continue;
                // A house front or roof inside the house it stands against: hidden in it, by design.
                if (house && fades != null) { houseOverlaps++; continue; }
                walled.Add($"{piece.Name} at {V(b.center)} {deepest:0.00} m into {Path(wall.transform)}");
            }
            // 4b: into the face of a rise (a bank, a terrace): under the ground at its own spot, where that ground stands
            // well above the piece's foot.
            if (!house && !flatPiece && !IsDressing(piece) && b.size.y < 12f)
            {
                float footGround = TopBelow(b.center.x, b.center.z, b.min.y + 0.6f, out _);
                float deepest = 0f;
                foreach (var p in points)
                {
                    float g = TopBelow(p.x, p.z, p.y + 30f, out var what);
                    if (what == null || !(what is MeshCollider) || (what.bounds.size.x < 6f && what.bounds.size.z < 6f)) continue;
                    if (g > b.min.y + 0.8f && g > p.y + deepest) deepest = g - p.y;
                }
                if (deepest > 0.2f) rises.Add($"{piece.Name} at {V(b.center)} {deepest:0.00} m under the top of a rise beside it (its foot at {b.min.y:0.0}, ground there {footGround:0.0})");
            }
        }
        walled.AddRange(rises);
        walled.Add($"(house fronts and roofs inside the next house in their row, hidden there: {houseOverlaps}, not counted)");

        sb.AppendLine($"# Sun City scan: {label}");
        sb.AppendLine();
        sb.AppendLine($"pieces {pieces.Count}, fade groups {groups.Length}, reachable floor cells {nodes}");
        sb.AppendLine();
        Section(sb, "1. dressing hanging off its surface", hanging);
        Section(sb, "2/9. on a box that thins out, not thinning with it", unfaded);
        Section(sb, "3. floating in the body's space over reachable floor", floating);
        Section(sb, "4. more than 0.2 m into a wall or building", walled);
        Section(sb, "7. reachable floor next to nothing (a fall forever)", voids);
        Section(sb, "8. drops of more than 2 m off a reachable floor", drops);
        System.IO.Directory.CreateDirectory("review/checks/cityfix");
        System.IO.File.WriteAllText($"review/checks/cityfix/scan_{label}.md", sb.ToString());
        string summary = $"hanging {hanging.Count} unfaded {unfaded.Count} floating {floating.Count} walled {walled.Count - 1} voids {voids.Count} drops {drops.Count} (reachable {nodes})";
        Debug.Log("[CityScan] " + summary);
        return summary;
    }

    private static void Section(StringBuilder sb, string title, List<string> items)
    {
        sb.AppendLine($"## {title}: {items.Count}");
        sb.AppendLine();
        foreach (var item in items.Take(400)) sb.AppendLine("- " + item);
        if (items.Count > 400) sb.AppendLine($"- ... {items.Count - 400} more");
        sb.AppendLine();
    }

    private static string V(Vector3 v) => $"({v.x:0.0}, {v.y:0.0}, {v.z:0.0})";

    private static string Path(Transform t)
    {
        var names = new List<string>();
        for (var at = t; at != null && names.Count < 4; at = at.parent) names.Insert(0, at.name);
        return string.Join("/", names);
    }

    // Held up: something solid under it at its own height, or the point is hidden inside something solid.
    private static bool Held(Vector3 p, float from)
    {
        if (TopBelow(p.x, p.z, from, out _) >= p.y - 0.25f) return true;
        return Physics.CheckSphere(p + Vector3.up * 0.03f, 0.02f, ~0, QueryTriggerInteraction.Ignore);
    }

    private static readonly FieldInfo shownField = typeof(Fadeable).GetField("alsoShown", BindingFlags.NonPublic | BindingFlags.Instance);

    private static bool KeptWith(Fadeable fades, Renderer renderer)
    {
        var kept = shownField != null ? shownField.GetValue(fades) as Renderer[] : null;
        return kept != null && System.Array.IndexOf(kept, renderer) >= 0;
    }

    private static bool IsDressing(Piece piece)
    {
        string n = piece.Name;
        if (n.Contains("ground_dust") || n.Contains("ground_bricks") || n.Contains("ground_sherds") || n.Contains("silt_pan") || n.Contains("scorch_disc")) return true;
        if (n.Contains("prop_") || n.Contains("rubble") || n.Contains("roof_flat")) return true;
        if (piece.Bounds.size.y >= 0.12f || !(n.StartsWith("Cube") || n.Contains("Cube"))) return false;
        // A flat primitive lying on the ground (a doorstep, a mat, a strip of char), not a slat in the air.
        var b = piece.Bounds;
        return TopBelow(b.center.x, b.center.z, b.max.y + 0.3f, out _) >= b.min.y - 0.15f;
    }

    // Every piece the scene draws: the merged scenery's list, and every renderer drawn by itself.
    private static List<Piece> Pieces(Transform world, out Fadeable[] groups)
    {
        var list = new List<Piece>();
        groups = new Fadeable[0];
        var merged = world.GetComponentInChildren<MergedScenery>(true);
        if (merged != null)
        {
            var field = typeof(MergedScenery).GetField("pieces", BindingFlags.NonPublic | BindingFlags.Instance);
            var groupField = typeof(MergedScenery).GetField("groups", BindingFlags.NonPublic | BindingFlags.Instance);
            groups = (Fadeable[])groupField.GetValue(merged);
            foreach (var p in (MergedScenery.Piece[])field.GetValue(merged))
            {
                if (p.Mesh == null || p.Shadows == 2) continue;
                list.Add(new Piece { Name = p.Mesh.name, Mesh = p.Mesh, Matrix = p.Matrix, Group = p.Group, Bounds = Transform(p.Mesh.bounds, p.Matrix) });
            }
        }
        foreach (var r in world.GetComponentsInChildren<MeshRenderer>(false))
        {
            if (!r.enabled || (r.gameObject.hideFlags & HideFlags.DontSave) != 0) continue;
            if (r.shadowCastingMode == UnityEngine.Rendering.ShadowCastingMode.ShadowsOnly) continue;
            var f = r.GetComponent<MeshFilter>();
            if (f == null || f.sharedMesh == null) continue;
            if (r.GetComponentInParent<MergedScenery>() != null) continue;
            if (r.GetComponentInParent<PavedFloor>() != null) continue;
            list.Add(new Piece { Name = Path(r.transform), Mesh = f.sharedMesh, Matrix = r.transform.localToWorldMatrix, Group = -2, Own = r, Bounds = r.bounds });
        }
        return list;
    }

    private static Bounds Transform(Bounds local, Matrix4x4 m)
    {
        var c = m.MultiplyPoint3x4(local.center);
        var e = local.extents;
        var x = m.MultiplyVector(new Vector3(e.x, 0, 0));
        var y = m.MultiplyVector(new Vector3(0, e.y, 0));
        var z = m.MultiplyVector(new Vector3(0, 0, e.z));
        var size = new Vector3(Mathf.Abs(x.x) + Mathf.Abs(y.x) + Mathf.Abs(z.x), Mathf.Abs(x.y) + Mathf.Abs(y.y) + Mathf.Abs(z.y), Mathf.Abs(x.z) + Mathf.Abs(y.z) + Mathf.Abs(z.z));
        return new Bounds(c, size * 2f);
    }

    private static readonly Dictionary<Mesh, Vector3[]> vertexCache = new Dictionary<Mesh, Vector3[]>();

    private static List<Vector3> Points(Piece piece, int most)
    {
        if (!vertexCache.TryGetValue(piece.Mesh, out var verts))
        {
            verts = piece.Mesh.isReadable ? piece.Mesh.vertices : new Vector3[0];
            if (verts.Length == 0)
            {
                var lb = piece.Mesh.bounds;
                verts = new[] { lb.min, lb.max, new Vector3(lb.min.x, lb.min.y, lb.max.z), new Vector3(lb.max.x, lb.min.y, lb.min.z), new Vector3(lb.min.x, lb.max.y, lb.max.z), new Vector3(lb.max.x, lb.max.y, lb.min.z) };
            }
            vertexCache[piece.Mesh] = verts;
        }
        int step = Mathf.Max(1, verts.Length / most);
        var points = new List<Vector3>(most + 1);
        for (int i = 0; i < verts.Length; i += step) points.Add(piece.Matrix.MultiplyPoint3x4(verts[i]));
        return points;
    }

    // How deep a point is inside a box collider, in metres (0 outside).
    private static float Depth(BoxCollider box, Vector3 point)
    {
        var t = box.transform;
        Vector3 local = t.InverseTransformPoint(point) - box.center;
        Vector3 half = box.size * 0.5f;
        Vector3 scale = t.lossyScale;
        float dx = (half.x - Mathf.Abs(local.x)) * Mathf.Abs(scale.x);
        float dy = (half.y - Mathf.Abs(local.y)) * Mathf.Abs(scale.y);
        float dz = (half.z - Mathf.Abs(local.z)) * Mathf.Abs(scale.z);
        if (dx <= 0f || dy <= 0f || dz <= 0f) return 0f;
        return Mathf.Min(dx, Mathf.Min(dy, dz));
    }

    private static readonly RaycastHit[] hits = new RaycastHit[64];

    // The highest top of anything solid under a point, from a height down; -999 for nothing.
    private static float TopBelow(float x, float z, float from, out Collider which)
    {
        which = null;
        int count = Physics.RaycastNonAlloc(new Vector3(x, from, z), Vector3.down, hits, from + 50f, ~0, QueryTriggerInteraction.Ignore);
        float best = -999f;
        for (int i = 0; i < count; i++)
        {
            if (ignored.Contains(hits[i].collider)) continue;
            if (hits[i].point.y > best) { best = hits[i].point.y; which = hits[i].collider; }
        }
        return best;
    }

    private static Collider SupportUnder(Vector3 foot, float from)
    {
        TopBelow(foot.x, foot.z, from, out var which);
        return which;
    }

    // ---------------------------------------------------------------- the floor the player can reach

    private static int CellOf(float x, float z)
    {
        int i = Mathf.FloorToInt((x - MinX) / Cell), j = Mathf.FloorToInt((z - MinZ) / Cell);
        if (i < 0 || j < 0 || i >= nx || j >= nz) return -1;
        return j * nx + i;
    }

    private static Vector3 CentreOf(int c) => new Vector3(MinX + (c % nx + 0.5f) * Cell, 0f, MinZ + (c / nx + 0.5f) * Cell);

    // Every top under a cell's middle that faces up and has room for a body over it, highest first.
    private static List<float> Surf(int c)
    {
        if (surfaces[c] != null) return surfaces[c];
        var list = new List<float>();
        var at = CentreOf(c);
        int count = Physics.RaycastNonAlloc(new Vector3(at.x, 80f, at.z), Vector3.down, hits, 140f, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
        {
            var hit = hits[i];
            if (ignored.Contains(hit.collider) || hit.normal.y < 0.6f) continue;
            var p = hit.point;
            if (Physics.CheckCapsule(p + Vector3.up * 0.5f, p + Vector3.up * 1.5f, 0.25f, ~0, QueryTriggerInteraction.Ignore) && Blocked(p + Vector3.up * 0.5f, p + Vector3.up * 1.5f, 0.25f)) continue;
            // Inside a made ground's mesh (a floor under a plateau): looking up, the first thing met is a top seen from below.
            if (Inside(p)) continue;
            list.Add(p.y);
        }
        list.Sort((a, b) => b.CompareTo(a));
        surfaces[c] = list;
        return list;
    }

    private static readonly Collider[] overlap = new Collider[32];

    private static bool Blocked(Vector3 a, Vector3 b, float radius)
    {
        int count = Physics.OverlapCapsuleNonAlloc(a, b, radius, overlap, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++) if (!ignored.Contains(overlap[i])) return true;
        return false;
    }

    private static readonly RaycastHit[] upward = new RaycastHit[32];

    private static bool Inside(Vector3 p)
    {
        bool was = Physics.queriesHitBackfaces;
        Physics.queriesHitBackfaces = true;
        int count = Physics.RaycastNonAlloc(p + Vector3.up * 0.05f, Vector3.up, upward, 60f, ~0, QueryTriggerInteraction.Ignore);
        Physics.queriesHitBackfaces = was;
        float nearest = float.MaxValue;
        bool under = false;
        for (int i = 0; i < count; i++)
        {
            if (ignored.Contains(upward[i].collider) || upward[i].distance >= nearest) continue;
            nearest = upward[i].distance;
            under = upward[i].normal.y > 0.5f && upward[i].collider is MeshCollider;
        }
        return under;
    }

    private static readonly RaycastHit[] fallen = new RaycastHit[32];

    private static bool SphereDown(Vector3 at, float from, out RaycastHit found)
    {
        found = default;
        int count = Physics.SphereCastNonAlloc(new Vector3(at.x, from, at.z), 0.3f, Vector3.down, fallen, 90f, ~0, QueryTriggerInteraction.Ignore);
        float nearest = float.MaxValue;
        for (int i = 0; i < count; i++)
        {
            if (ignored.Contains(fallen[i].collider) || fallen[i].distance <= 0f || fallen[i].distance >= nearest) continue;
            nearest = fallen[i].distance;
            found = fallen[i];
        }
        return nearest < float.MaxValue && found.point.y > -3f;
    }

    private static readonly RaycastHit[] swept = new RaycastHit[32];

    private static bool Crosses(Vector3 a, Vector3 b, Vector3 direction)
    {
        int count = Physics.CapsuleCastNonAlloc(a, b, 0.2f, direction, swept, Cell, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++) if (!ignored.Contains(swept[i].collider) && swept[i].distance > 0f) return true;
        return false;
    }

    private static long Key(int c, float h) => (long)c * 4096 + Mathf.RoundToInt((h + 20f) * 20f);

    private static bool IsReached(int c, float h) => reached.ContainsKey(Key(c, h));

    private static void Reach(out List<string> voids, out List<string> drops, out int nodes)
    {
        nx = Mathf.CeilToInt((MaxX - MinX) / Cell);
        nz = Mathf.CeilToInt((MaxZ - MinZ) / Cell);
        surfaces = new List<float>[nx * nz];
        reached = new Dictionary<long, byte>();
        var queue = new Queue<(int, float)>();
        var player = Object.FindAnyObjectByType<Clover.Player.PlayerController>();
        var start = player != null ? player.transform.position : new Vector3(0f, 0f, 4f);
        void Seed(Vector3 at)
        {
            int c = CellOf(at.x, at.z);
            if (c < 0) return;
            float best = float.NaN;
            foreach (float h in Surf(c)) if (h <= at.y + 0.6f) { best = h; break; }
            if (float.IsNaN(best) || IsReached(c, best)) return;
            reached[Key(c, best)] = 1;
            queue.Enqueue((c, best));
        }
        Seed(start + Vector3.up * 0.3f);
        var ladders = Object.FindObjectsByType<Ladder>(FindObjectsInactive.Exclude, FindObjectsSortMode.None);
        var voidCells = new Dictionary<int, Vector3>();
        var dropList = new Dictionary<int, string>();
        int[] dx = { 1, -1, 0, 0 }, dz = { 0, 0, 1, -1 };
        while (true)
        {
            while (queue.Count > 0)
            {
                var (c, h) = queue.Dequeue();
                int ci = c % nx, cj = c / nx;
                for (int k = 0; k < 4; k++)
                {
                    int ni = ci + dx[k], nj = cj + dz[k];
                    if (ni < 0 || nj < 0 || ni >= nx || nj >= nz) continue;
                    int n = nj * nx + ni;
                    var centre = CentreOf(n);
                    // Something in the way at body height over this floor.
                    if (Blocked(new Vector3(centre.x, h + 0.65f, centre.z), new Vector3(centre.x, h + 1.6f, centre.z), 0.2f)) continue;
                    // A face between here and there (a mesh's inside is not solid to an overlap, only its faces are).
                    var here = CentreOf(c);
                    if (Crosses(new Vector3(here.x, h + 0.65f, here.z), new Vector3(here.x, h + 1.6f, here.z), new Vector3(dx[k], 0f, dz[k]))) continue;
                    float landing = float.NaN;
                    foreach (float t in Surf(n)) if (t <= h + 0.55f) { landing = t; break; }
                    if (float.IsNaN(landing))
                    {
                        // Nothing under: is there room for a body to go down there?
                        // A gap narrower than a body is stepped over: there must be nothing under this cell's middle
                        // and nothing half a metre further on either.
                        var further = centre + new Vector3(dx[k], 0f, dz[k]) * 0.5f;
                        if (TopBelow(centre.x, centre.z, h + 1f, out _) < -3f && TopBelow(further.x, further.z, h + 1f, out _) < -3f)
                            if (!voidCells.ContainsKey(n)) voidCells[n] = new Vector3(centre.x, h, centre.z);
                        continue;
                    }
                    int target = n;
                    if (h - landing > 0.55f)
                    {
                        // A fall: where the body comes to rest, sliding off any steep face on the way down.
                        var way = new Vector3(dx[k], 0f, dz[k]);
                        // The body goes over the edge once its middle is past it.
                        var pos = centre + way * 0.35f;
                        float from = h + 0.6f;
                        bool lost = false, rested = false;
                        for (int step = 0; step < 12; step++)
                        {
                            if (!SphereDown(pos, from, out var hit)) { lost = true; break; }
                            if (hit.normal.y >= 0.6f) { landing = hit.point.y; rested = true; break; }
                            pos += way * 0.5f;
                            from = hit.point.y + 0.6f;
                        }
                        if (lost)
                        {
                            int lostCell = CellOf(pos.x, pos.z);
                            if (!voidCells.ContainsKey(lostCell)) voidCells[lostCell] = new Vector3(pos.x, h, pos.z);
                            continue;
                        }
                        if (!rested) continue;
                        target = CellOf(pos.x, pos.z);
                        if (target < 0) continue;
                        float best = float.NaN;
                        foreach (float t in Surf(target)) if (Mathf.Abs(t - landing) < 0.6f) { best = t; break; }
                        if (float.IsNaN(best)) continue;
                        landing = best;
                    }
                    if (h - landing > 2f && !dropList.ContainsKey(target)) dropList[target] = $"({centre.x:0.0}, {h:0.0} -> {landing:0.0}, {centre.z:0.0})";
                    if (IsReached(target, landing)) continue;
                    reached[Key(target, landing)] = 1;
                    queue.Enqueue((target, landing));
                }
            }
            // Ladders join their foot and head both ways once either is reached.
            bool more = false;
            foreach (var ladder in ladders)
            {
                int f = CellOf(ladder.Foot.x, ladder.Foot.z), hd = CellOf(ladder.Head.x, ladder.Head.z);
                bool footIn = f >= 0 && Surf(f).Any(s => Mathf.Abs(s - ladder.Foot.y) < 0.7f && IsReached(f, s));
                bool headIn = hd >= 0 && Surf(hd).Any(s => Mathf.Abs(s - ladder.Head.y) < 0.7f && IsReached(hd, s));
                if (footIn && !headIn) { int before = queue.Count; Seed(ladder.Head + Vector3.up * 0.1f); more |= queue.Count > before; }
                if (headIn && !footIn) { int before = queue.Count; Seed(ladder.Foot + Vector3.up * 0.1f); more |= queue.Count > before; }
            }
            if (!more) break;
        }
        nodes = reached.Count;
        int south = 0;
        foreach (var key in reached.Keys) { var c = CentreOf((int)(key / 4096)); if (c.z < -6.6f) south++; }
        Debug.Log("[CityScan] reached south of the bank: " + south);
        voids = Cluster(voidCells.Values.ToList(), "nothing under");
        drops = dropList.Values.Take(0).ToList();
        // Drops listed by place, grouped in 4 m squares.
        var byPlace = new Dictionary<(int, int), List<string>>();
        foreach (var kv in dropList)
        {
            var c = CentreOf(kv.Key);
            var key = (Mathf.FloorToInt(c.x / 4f), Mathf.FloorToInt(c.z / 4f));
            if (!byPlace.TryGetValue(key, out var l)) byPlace[key] = l = new List<string>();
            l.Add(kv.Value);
        }
        foreach (var kv in byPlace.OrderBy(k => k.Key.Item2).ThenBy(k => k.Key.Item1)) drops.Add($"square x {kv.Key.Item1 * 4}, z {kv.Key.Item2 * 4}: {kv.Value.Count} cells, e.g. {kv.Value[0]}");
    }

    private static List<string> Cluster(List<Vector3> points, string what)
    {
        var result = new List<string>();
        var left = new List<Vector3>(points);
        while (left.Count > 0)
        {
            var seed = left[0];
            var group = new List<Vector3> { seed };
            left.RemoveAt(0);
            for (int i = 0; i < group.Count; i++)
                for (int j = left.Count - 1; j >= 0; j--)
                    if ((left[j] - group[i]).sqrMagnitude < 2.5f * 2.5f) { group.Add(left[j]); left.RemoveAt(j); }
            var min = group.Aggregate((a, b) => Vector3.Min(a, b));
            var max = group.Aggregate((a, b) => Vector3.Max(a, b));
            result.Add($"{what}: {group.Count} cells, x {min.x:0.0} to {max.x:0.0}, z {min.z:0.0} to {max.z:0.0}, walked from floor at {min.y:0.0} to {max.y:0.0}");
        }
        return result;
    }
}

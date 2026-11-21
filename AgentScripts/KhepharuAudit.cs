using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using Clover.UI;
using Clover.World;
using Unity.Collections;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

// The audit of the Khepharu fight as it stands (docs/design/khepharu_relook.md, section 6), before anything changes.
// It films every move of the man and the Sun-Bearer (a JPG every 0.1 s and the four beats as PNG) and measures, at the
// end of every frame that time moved:
//   - how far any drawn part of the sun (ball, corona quad, trail, dull ball; the hidden submesh listed apart) reaches
//     below the paving's top (0.036 m), per move and sun state;
//   - how deep the sun's ball (centre, radius 1.5 x size) sinks into the body: into each piece's triangles (exact) and
//     into each piece's own eight-corner box (the brief's measure), per case;
//   - whether the boss is in the picture, and whether the obelisk stands between the camera and the player or the boss;
//   - frame time and main-thread time per part, leaving out the frames a capture stalled.
// Output under review/khepharu/audit/<camera>/: one folder per move or case (seq/*.jpg, b1..b4 PNG), worst/*.png
// (taken the frame a new worst was measured), frames.csv (one row a frame) and summary.txt.
// AgentScripts/khepharu_only.txt holds the parts to run (Intro Man Change Form Cases Edges Beats; empty for all) and
// "Close" to film from the close camera (0.5 x distance, 35 degrees down, side-on to the sun and the body) instead of
// the game camera. Run with
//   bash AgentScripts/runprobe.sh SunCourt KhepharuAudit 400
public static class KhepharuAudit
{
    public static string Run()
    {
        if (!Application.isPlaying) return "not in play mode";
        new GameObject("Khepharu Audit").AddComponent<KhepharuAuditRunner>();
        return "probe started";
    }
}

public class KhepharuAuditRunner : MonoBehaviour
{
    private const float FloorTop = 0.036f;

    private PlayerController player;
    private PlayerCombat combat;
    private PlayerDefence defence;
    private SunCourtDirector court;
    private SunBearer form;
    private Enemy boss;
    private TopDownCamera topDown;
    private Camera lens;
    private string only = "", cam = "game", root;
    private bool keepAlive = true;
    private Func<Vector3> follow;
    private int guideClosed;

    // What is being filmed and measured.
    private string label = "start";
    private bool measuring;
    private string seqFolder;
    private float seqEvery = 0.1f, nextSeq;
    private int seqIndex, shots;
    private string lastFrame = "";
    private readonly Queue<string> wantedPng = new Queue<string>();
    private float restoreScale = -1f;
    private StreamWriter csv;

    // The close camera, or a fixed view, put in place of the game camera.
    private bool overrideCam, fixedView;
    private Vector3 camAim, fixedAim;
    private float camYaw, fixedYaw, fixedPitch = 35f, fixedDistance = 12f;

    // The pieces measured.
    private Transform sunRoot;
    private Renderer[] pieces = new Renderer[0];
    private Renderer[] sunRenderers = new Renderer[0];
    private FieldInfo sunStateField, rollingField, sunAtField;
    private readonly HashSet<Collider> obelisk = new HashSet<Collider>();
    private Mesh baked;

    private class MeshData
    {
        public Vector3[] Verts;
        public int[] Tris;
        public int[][] SubVerts;
        public BoneWeight[] Weights;
        public Matrix4x4[] Poses;
    }
    private readonly Dictionary<Mesh, MeshData> meshes = new Dictionary<Mesh, MeshData>();

    private class Worst
    {
        public float Depth = -99f;
        public string Who = "", Frame = "";
        public float At;
        public int Frames, Over;
    }
    private readonly Dictionary<string, Worst> floorWorst = new Dictionary<string, Worst>();
    private readonly Dictionary<string, Worst> bodyWorst = new Dictionary<string, Worst>();
    private readonly Dictionary<string, Worst> boxWorst = new Dictionary<string, Worst>();
    private readonly Dictionary<string, float> shotFloor = new Dictionary<string, float>();
    private readonly Dictionary<string, float> shotBody = new Dictionary<string, float>();

    private class Tally
    {
        public int Frames, BossOff, Falcon, FalconOff, CoverPlayer, CoverBoss, PlayerOff;
    }
    private readonly Dictionary<string, Tally> tallies = new Dictionary<string, Tally>();

    private class Times
    {
        public readonly List<float> Frame = new List<float>(), Main = new List<float>();
        public long Garbage;
    }
    private readonly Dictionary<string, Times> times = new Dictionary<string, Times>();
    private string timesPart = "";
    private bool skipTime;
    private ProfilerRecorder mainThread, garbage;
    private readonly List<string> summary = new List<string>();

    private static void Keys(params Key[] held) => InputSystem.QueueStateEvent(Keyboard.current, new KeyboardState(held));
    private void Say(string text)
    {
        Debug.Log("[Probe] " + text);
        summary.Add(text);
    }
    private bool Wants(string word) => only.Length == 0 || only.Split(' ').Contains(word);
    private static Vector3 Flat(Vector3 v) => new Vector3(v.x, 0f, v.z);
    private static Vector3 Polar(float degrees, float radius) => Quaternion.Euler(0f, degrees, 0f) * Vector3.forward * radius;

    private IEnumerator Until(Func<bool> condition, float timeout)
    {
        float end = Time.time + timeout;
        float realEnd = Time.realtimeSinceStartup + timeout * 3f + 5f;
        while (!condition() && Time.time < end && Time.realtimeSinceStartup < realEnd) yield return null;
    }

    private IEnumerator Wait(float seconds) => Until(() => false, seconds);

    private void Go(Vector3 to, Vector3 facing)
    {
        facing.y = 0f;
        player.Teleport(new Vector3(to.x, 0.1f, to.z), facing.sqrMagnitude > 0.01f ? Quaternion.LookRotation(facing) : Quaternion.identity);
    }

    private Vector3 BodyForward => Flat(form.transform.Find("Body").forward).normalized;

    // ------------------------------------------------------------------ every frame

    private void Update()
    {
        if (keepAlive && defence != null && !defence.IsDead) defence.Health.Heal(100f);
        if (follow != null && player != null) player.Teleport(follow(), player.transform.rotation);
        // The guide opened as a list stops time and E only pages it (fixed in a45d15c and d9237de): never leave it open.
        if (GuidePage.Showing)
        {
            GuidePage.Instance.Close();
            guideClosed++;
        }
        if (overrideCam && lens != null) PlaceCamera();
    }

    // The close camera: half the form fight's distance, 35 degrees down, side-on to the line from the body to the sun.
    private void PlaceCamera()
    {
        if (topDown != null && topDown.enabled) topDown.enabled = false;
        Vector3 aim;
        float yaw;
        if (fixedView)
        {
            aim = fixedAim;
            yaw = fixedYaw;
        }
        else
        {
            bool sunLow = sunRoot != null && sunRoot.gameObject.activeInHierarchy && sunRoot.position.y < 5f;
            Vector3 body = form != null && form.HasRisen ? form.BodyAt : boss.transform.position;
            Vector3 want = form != null && form.HasRisen ? (sunLow ? Vector3.Lerp(sunRoot.position, body, 0.35f) : body) : boss.transform.position + Vector3.up;
            camAim = Vector3.Distance(camAim, want) > 12f ? want : Vector3.Lerp(camAim, want, 1f - Mathf.Exp(-8f * Time.unscaledDeltaTime));
            aim = camAim;
            Vector3 between = form != null && form.HasRisen ? Flat(form.SunAt - form.BodyAt) : Flat(player.transform.position - boss.transform.position);
            if (between.sqrMagnitude > 1f)
            {
                float side = Mathf.Atan2(between.x, between.z) * Mathf.Rad2Deg + 90f;
                if (Mathf.Abs(Mathf.DeltaAngle(camYaw, side)) > 90f) side += 180f;
                camYaw = Mathf.MoveTowardsAngle(camYaw, side, 120f * Time.unscaledDeltaTime);
            }
            yaw = camYaw;
        }
        var rotation = Quaternion.Euler(fixedView ? fixedPitch : 35f, yaw, 0f);
        float distance = fixedView ? fixedDistance : 12f;
        lens.transform.SetPositionAndRotation(aim - rotation * Vector3.forward * distance, rotation);
    }

    private void ReleaseCamera()
    {
        overrideCam = false;
        fixedView = false;
        if (topDown != null) topDown.enabled = true;
    }

    // At the end of every frame: measure, then take whatever pictures were asked for, stopping time for the next frame
    // so the stall of writing them does not move the fight on.
    private IEnumerator EndOfFrames()
    {
        while (true)
        {
            yield return new WaitForEndOfFrame();
            if (this == null) yield break;
            if (restoreScale >= 0f)
            {
                if (Time.timeScale == 0f) Time.timeScale = restoreScale;
                restoreScale = -1f;
                skipTime = true;
                continue;
            }
            bool advanced = Time.deltaTime > 0f;
            if (advanced && form != null && measuring)
            {
                try { MeasureFrame(); }
                catch (Exception error) { Debug.Log("[Probe] measure failed: " + error.Message); }
            }
            if (advanced && !skipTime && timesPart.Length > 0) RecordTime();
            skipTime = false;
            if (seqFolder != null && advanced && Time.time >= nextSeq)
            {
                nextSeq = Time.time + seqEvery;
                Capture(seqFolder + "/" + seqIndex.ToString("000") + ".jpg", false);
                seqIndex++;
            }
            if (wantedPng.Count > 0)
            {
                var texture = ScreenCapture.CaptureScreenshotAsTexture();
                byte[] png = texture.EncodeToPNG();
                Destroy(texture);
                while (wantedPng.Count > 0)
                {
                    string path = wantedPng.Dequeue();
                    File.WriteAllBytes(path, png);
                    lastFrame = path;
                    shots++;
                }
                Freeze();
            }
        }
    }

    private void Capture(string path, bool png)
    {
        if (png)
        {
            wantedPng.Enqueue(path);
            return;
        }
        var texture = ScreenCapture.CaptureScreenshotAsTexture();
        File.WriteAllBytes(path, texture.EncodeToJPG(80));
        Destroy(texture);
        lastFrame = path;
        shots++;
        Freeze();
    }

    private void Freeze()
    {
        if (Time.timeScale > 0f && restoreScale < 0f)
        {
            restoreScale = Time.timeScale;
            Time.timeScale = 0f;
        }
    }

    private void Beat(string folder, string name) => wantedPng.Enqueue(folder + "/" + name + ".png");

    private void RecordTime()
    {
        if (!times.TryGetValue(timesPart, out var record)) times[timesPart] = record = new Times();
        record.Frame.Add(Time.unscaledDeltaTime * 1000f);
        if (mainThread.Valid) record.Main.Add(mainThread.LastValue * 1e-6f);
        if (garbage.Valid) record.Garbage += garbage.LastValue;
    }

    // ------------------------------------------------------------------ the measures

    // In Play Mode a mesh imported without Read/Write cannot be read, so the model's meshes come from the file
    // KhepharuMeshDump wrote in edit mode; Unity's own quad is made here.
    private readonly Dictionary<string, MeshData> dumped = new Dictionary<string, MeshData>();

    private void LoadDump()
    {
        string path = "review/khepharu/audit/meshes.bin";
        if (!File.Exists(path))
        {
            Say("WRONG: no mesh dump; run KhepharuMeshDump in edit mode first");
            return;
        }
        using (var reader = new BinaryReader(File.OpenRead(path)))
        {
            int count = reader.ReadInt32();
            for (int m = 0; m < count; m++)
            {
                var data = new MeshData();
                string name = reader.ReadString();
                data.Verts = new Vector3[reader.ReadInt32()];
                for (int i = 0; i < data.Verts.Length; i++) data.Verts[i] = new Vector3(reader.ReadSingle(), reader.ReadSingle(), reader.ReadSingle());
                int subs = reader.ReadInt32();
                data.SubVerts = new int[subs][];
                var tris = new List<int>();
                for (int s = 0; s < subs; s++)
                {
                    bool triangles = reader.ReadBoolean();
                    var indices = new int[reader.ReadInt32()];
                    for (int i = 0; i < indices.Length; i++) indices[i] = reader.ReadInt32();
                    if (triangles) tris.AddRange(indices);
                    data.SubVerts[s] = indices.Distinct().ToArray();
                }
                data.Tris = tris.ToArray();
                data.Weights = new BoneWeight[reader.ReadInt32()];
                for (int i = 0; i < data.Weights.Length; i++)
                    data.Weights[i] = new BoneWeight { boneIndex0 = reader.ReadInt32(), weight0 = reader.ReadSingle(), boneIndex1 = reader.ReadInt32(), weight1 = reader.ReadSingle(),
                        boneIndex2 = reader.ReadInt32(), weight2 = reader.ReadSingle(), boneIndex3 = reader.ReadInt32(), weight3 = reader.ReadSingle() };
                data.Poses = new Matrix4x4[reader.ReadInt32()];
                for (int i = 0; i < data.Poses.Length; i++)
                {
                    var pose = new Matrix4x4();
                    for (int k = 0; k < 16; k++) pose[k] = reader.ReadSingle();
                    data.Poses[i] = pose;
                }
                dumped[name] = data;
            }
        }
        var quad = new MeshData
        {
            Verts = new[] { new Vector3(-0.5f, -0.5f, 0f), new Vector3(0.5f, -0.5f, 0f), new Vector3(-0.5f, 0.5f, 0f), new Vector3(0.5f, 0.5f, 0f) },
            Tris = new[] { 0, 3, 1, 3, 0, 2 },
            SubVerts = new[] { new[] { 0, 1, 2, 3 } },
            Weights = new BoneWeight[0],
            Poses = new Matrix4x4[0],
        };
        dumped["Quad"] = quad;
        Say($"mesh dump: {dumped.Count} meshes ({string.Join(", ", dumped.Keys)})");
    }

    private MeshData Data(Mesh mesh)
    {
        if (meshes.TryGetValue(mesh, out var data)) return data;
        string name = mesh.name.Replace(" Instance", "");
        if (!dumped.TryGetValue(name, out data)) data = null;
        meshes[mesh] = data;
        return data;
    }

    private string SunState() => sunStateField != null ? sunStateField.GetValue(form).ToString() : "?";

    private Worst Note(Dictionary<string, Worst> table, string key, float depth, string who)
    {
        if (!table.TryGetValue(key, out var worst)) table[key] = worst = new Worst();
        worst.Frames++;
        if (depth > 0.05f) worst.Over++;
        if (depth > worst.Depth)
        {
            worst.Depth = depth;
            worst.Who = who;
            worst.Frame = lastFrame;
            worst.At = form.DoingFor;
        }
        return worst;
    }

    private static string Clean(string text)
    {
        var made = new StringBuilder();
        foreach (char c in text) made.Append(char.IsLetterOrDigit(c) ? c : '_');
        return made.ToString();
    }

    private void MeasureFrame()
    {
        if (!form.HasRisen) return;
        string state = SunState();
        string doing = form.Doing;
        string key = label + " | " + doing + " | sun " + state;

        // The sun against the floor: the lowest point of every drawn part.
        float drawnWorst = -99f;
        string drawnWho = "";
        if (sunRoot != null && sunRoot.gameObject.activeInHierarchy)
        {
            foreach (var shown in sunRenderers)
            {
                if (shown == null || !shown.enabled || !shown.gameObject.activeInHierarchy) continue;
                if (shown is TrailRenderer trail)
                {
                    if (trail.positionCount < 2) continue;
                    float low = 99f;
                    for (int i = 0; i < trail.positionCount; i++) low = Mathf.Min(low, trail.GetPosition(i).y);
                    low -= trail.widthMultiplier * 0.5f * Mathf.Abs(lens.transform.up.y);
                    float depth = FloorTop - low;
                    Note(floorWorst, key + " | trail", depth, "trail");
                    if (depth > drawnWorst) { drawnWorst = depth; drawnWho = "trail"; }
                    continue;
                }
                var filter = shown.GetComponent<MeshFilter>();
                if (filter == null || filter.sharedMesh == null) continue;
                var data = Data(filter.sharedMesh);
                if (data == null) continue;
                var matrix = shown.localToWorldMatrix;
                var materials = shown.sharedMaterials;
                string meshName = filter.sharedMesh.name;
                for (int s = 0; s < data.SubVerts.Length; s++)
                {
                    float low = 99f;
                    foreach (int index in data.SubVerts[s]) low = Mathf.Min(low, matrix.MultiplyPoint3x4(data.Verts[index]).y);
                    string material = s < materials.Length && materials[s] != null ? materials[s].name : "none";
                    bool hidden = material.Contains("hidden");
                    string part = meshName == "Quad" ? "corona corner" : meshName == "sun" ? (s == 0 ? "ball" : "sun submesh " + s) : meshName == "sun_loose" ? (s == 0 ? "dull ball" : "dull submesh " + s) : meshName + " " + s;
                    if (hidden) part += " (hidden)";
                    float depth = FloorTop - low;
                    Note(floorWorst, key + " | " + part, depth, part);
                    if (!hidden && depth > drawnWorst) { drawnWorst = depth; drawnWho = part; }
                }
                if (meshName == "Quad")
                {
                    // The ring of flame is drawn on the quad's middle; its bottom edge's middle is where a cut would show.
                    float middle = FloorTop - shown.transform.TransformPoint(new Vector3(0f, -0.5f, 0f)).y;
                    Note(floorWorst, key + " | corona bottom edge", middle, "corona bottom edge");
                }
            }
        }
        if (drawnWorst > -98f)
        {
            Note(floorWorst, key + " | ANY DRAWN", drawnWorst, drawnWho);
            if (drawnWorst > 0.15f && (!shotFloor.TryGetValue(key, out float was) || drawnWorst > was + 0.1f))
            {
                shotFloor[key] = drawnWorst;
                Beat(root + "/worst", "floor_" + Clean(label + "_" + doing + "_" + state) + "_" + drawnWorst.ToString("0.00").Replace('.', 'p'));
            }
        }

        // The sun's ball against the body.
        float meshWorst = -99f, boxWorstNow = -99f;
        string meshWho = "", boxWho = "";
        if (sunRoot != null && sunRoot.gameObject.activeInHierarchy)
        {
            Vector3 centre = sunRoot.position;
            float radius = 1.5f * sunRoot.lossyScale.x;
            foreach (var piece in pieces)
            {
                if (piece == null || !piece.enabled || !piece.gameObject.activeInHierarchy) continue;
                if (piece.bounds.SqrDistance(centre) > radius * radius) continue;
                float box = SphereInBox(piece, centre, radius);
                if (box > boxWorstNow) { boxWorstNow = box; boxWho = piece.name; }
                float mesh = SphereInMesh(piece, centre, radius);
                if (mesh > meshWorst) { meshWorst = mesh; meshWho = piece.name; }
            }
        }
        if (meshWorst > -98f || boxWorstNow > -98f)
        {
            Note(bodyWorst, key, meshWorst, meshWho);
            Note(boxWorst, key, boxWorstNow, boxWho);
            if (meshWorst > 0.2f && (!shotBody.TryGetValue(key, out float was) || meshWorst > was + 0.15f))
            {
                shotBody[key] = meshWorst;
                Beat(root + "/worst", "body_" + Clean(label + "_" + doing + "_" + state) + "_" + meshWorst.ToString("0.00").Replace('.', 'p'));
            }
        }

        // The picture: is the boss in it, and does the obelisk stand in the way.
        if (!tallies.TryGetValue(label, out var tally)) tallies[label] = tally = new Tally();
        tally.Frames++;
        Vector3 bossPoint = form.BodyAt + Vector3.up * (form.IsFalcon ? 0.5f : 1.2f);
        bool bossOn = OnScreen(bossPoint);
        if (!bossOn && !form.Underground) tally.BossOff++;
        if (form.IsFalcon)
        {
            tally.Falcon++;
            if (!bossOn) tally.FalconOff++;
        }
        Vector3 chest = player.transform.position + Vector3.up;
        if (!OnScreen(chest)) tally.PlayerOff++;
        bool coverPlayer = Covered(chest), coverBoss = !form.Underground && Covered(bossPoint);
        if (coverPlayer) tally.CoverPlayer++;
        if (coverBoss) tally.CoverBoss++;

        csv.WriteLine(string.Join(",", Time.frameCount, Time.time.ToString("0.000"), cam, Clean(label), doing, form.DoingFor.ToString("0.00"), state,
            sunRoot.lossyScale.x.ToString("0.00"), sunRoot.position.y.ToString("0.00"),
            drawnWorst.ToString("0.000"), Clean(drawnWho), meshWorst.ToString("0.000"), Clean(meshWho), boxWorstNow.ToString("0.000"), Clean(boxWho),
            bossOn ? 1 : 0, form.IsFalcon ? 1 : 0, coverPlayer ? 1 : 0, coverBoss ? 1 : 0, Path.GetFileName(lastFrame)));
    }

    private bool OnScreen(Vector3 point)
    {
        Vector3 seen = lens.WorldToViewportPoint(point);
        return seen.z > 0f && seen.x > 0.02f && seen.x < 0.98f && seen.y > 0.02f && seen.y < 0.98f;
    }

    private readonly RaycastHit[] hits = new RaycastHit[16];

    private bool Covered(Vector3 point)
    {
        Vector3 from = lens.transform.position;
        Vector3 to = point - from;
        int count = Physics.RaycastNonAlloc(from, to.normalized, hits, to.magnitude - 0.3f, ~0, QueryTriggerInteraction.Ignore);
        for (int i = 0; i < count; i++)
            if (obelisk.Contains(hits[i].collider)) return true;
        return false;
    }

    // How deep a ball goes into a piece's own box (positive is in), as SunFightProbe and KeepInside read the pieces.
    private static float SphereInBox(Renderer piece, Vector3 centre, float radius)
    {
        var bounds = piece.localBounds;
        Matrix4x4 toWorld = piece is SkinnedMeshRenderer skinned && skinned.rootBone != null ? skinned.rootBone.localToWorldMatrix : piece.transform.localToWorldMatrix;
        Vector3 local = toWorld.inverse.MultiplyPoint3x4(centre);
        Vector3 min = bounds.min, max = bounds.max;
        Vector3 nearest = new Vector3(Mathf.Clamp(local.x, min.x, max.x), Mathf.Clamp(local.y, min.y, max.y), Mathf.Clamp(local.z, min.z, max.z));
        if (nearest != local) return radius - Vector3.Distance(toWorld.MultiplyPoint3x4(nearest), centre);
        Vector3 scale = toWorld.lossyScale;
        float x = Mathf.Min(local.x - min.x, max.x - local.x) * Mathf.Abs(scale.x);
        float y = Mathf.Min(local.y - min.y, max.y - local.y) * Mathf.Abs(scale.y);
        float z = Mathf.Min(local.z - min.z, max.z - local.z) * Mathf.Abs(scale.z);
        return radius + Mathf.Min(x, Mathf.Min(y, z));
    }

    private Vector3[] world = new Vector3[0];
    private Matrix4x4[] skinMatrices = new Matrix4x4[0];

    // How deep a ball goes into a piece's triangles: the radius less the distance to the nearest point of the surface,
    // or the radius plus it when the middle is behind that surface.
    private float SphereInMesh(Renderer piece, Vector3 centre, float radius)
    {
        Vector3[] verts;
        int[] tris;
        if (piece is SkinnedMeshRenderer skinned)
        {
            // Skinned on the CPU from the dumped weights and bind poses and the bones as they stand.
            var data = skinned.sharedMesh != null ? Data(skinned.sharedMesh) : null;
            var bones = skinned.bones;
            if (data == null || data.Weights.Length != data.Verts.Length || bones == null || bones.Length == 0) return -99f;
            verts = data.Verts;
            tris = data.Tris;
            if (skinMatrices.Length < bones.Length) skinMatrices = new Matrix4x4[bones.Length];
            for (int b = 0; b < bones.Length && b < data.Poses.Length; b++) skinMatrices[b] = bones[b] != null ? bones[b].localToWorldMatrix * data.Poses[b] : Matrix4x4.identity;
            if (world.Length < verts.Length) world = new Vector3[verts.Length];
            for (int i = 0; i < verts.Length; i++)
            {
                var w = data.Weights[i];
                Vector3 v = verts[i];
                world[i] = skinMatrices[w.boneIndex0].MultiplyPoint3x4(v) * w.weight0 + skinMatrices[w.boneIndex1].MultiplyPoint3x4(v) * w.weight1
                    + skinMatrices[w.boneIndex2].MultiplyPoint3x4(v) * w.weight2 + skinMatrices[w.boneIndex3].MultiplyPoint3x4(v) * w.weight3;
            }
        }
        else
        {
            var filter = piece.GetComponent<MeshFilter>();
            if (filter == null || filter.sharedMesh == null) return -99f;
            var data = Data(filter.sharedMesh);
            if (data == null) return -99f;
            verts = data.Verts;
            tris = data.Tris;
            var matrix = piece.localToWorldMatrix;
            if (world.Length < verts.Length) world = new Vector3[verts.Length];
            for (int i = 0; i < verts.Length; i++) world[i] = matrix.MultiplyPoint3x4(verts[i]);
        }
        float best = float.MaxValue;
        int upCrossings = 0, slantCrossings = 0;
        for (int t = 0; t + 2 < tris.Length; t += 3)
        {
            Vector3 a = world[tris[t]], b = world[tris[t + 1]], c = world[tris[t + 2]];
            if (Crosses(centre, Vector3.up, a, b, c)) upCrossings++;
            if (Crosses(centre, Slant, a, b, c)) slantCrossings++;
            Vector3 point = Closest(centre, a, b, c);
            float distance = (centre - point).sqrMagnitude;
            if (distance < best) best = distance;
        }
        if (best == float.MaxValue) return -99f;
        float d = Mathf.Sqrt(best);
        // Inside only when two rays out of the middle both cross the surface an odd number of times.
        bool inside = (upCrossings & 1) == 1 && (slantCrossings & 1) == 1;
        return inside ? radius + d : radius - d;
    }

    private static readonly Vector3 Slant = new Vector3(0.31f, 0.27f, 0.91f).normalized;

    // Whether a ray from a point crosses a triangle (Moller and Trumbore).
    private static bool Crosses(Vector3 from, Vector3 way, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 ab = b - a, ac = c - a;
        Vector3 p = Vector3.Cross(way, ac);
        float det = Vector3.Dot(ab, p);
        if (Mathf.Abs(det) < 1e-9f) return false;
        float inverse = 1f / det;
        Vector3 s = from - a;
        float u = Vector3.Dot(s, p) * inverse;
        if (u < 0f || u > 1f) return false;
        Vector3 q = Vector3.Cross(s, ab);
        float v = Vector3.Dot(way, q) * inverse;
        if (v < 0f || u + v > 1f) return false;
        return Vector3.Dot(ac, q) * inverse > 0f;
    }

    // The nearest point of a triangle to a point (Ericson, Real-Time Collision Detection, 5.1.5).
    private static Vector3 Closest(Vector3 p, Vector3 a, Vector3 b, Vector3 c)
    {
        Vector3 ab = b - a, ac = c - a, ap = p - a;
        float d1 = Vector3.Dot(ab, ap), d2 = Vector3.Dot(ac, ap);
        if (d1 <= 0f && d2 <= 0f) return a;
        Vector3 bp = p - b;
        float d3 = Vector3.Dot(ab, bp), d4 = Vector3.Dot(ac, bp);
        if (d3 >= 0f && d4 <= d3) return b;
        float vc = d1 * d4 - d3 * d2;
        if (vc <= 0f && d1 >= 0f && d3 <= 0f) return a + ab * (d1 / (d1 - d3));
        Vector3 cp = p - c;
        float d5 = Vector3.Dot(ab, cp), d6 = Vector3.Dot(ac, cp);
        if (d6 >= 0f && d5 <= d6) return c;
        float vb = d5 * d2 - d1 * d6;
        if (vb <= 0f && d2 >= 0f && d6 <= 0f) return a + ac * (d2 / (d2 - d6));
        float va = d3 * d6 - d5 * d4;
        if (va <= 0f && (d4 - d3) >= 0f && (d5 - d6) >= 0f) return b + (c - b) * ((d4 - d3) / ((d4 - d3) + (d5 - d6)));
        float denom = 1f / (va + vb + vc);
        return a + ab * (vb * denom) + ac * (vc * denom);
    }

    // ------------------------------------------------------------------ filming

    // Films while going() holds: a JPG every seqEvery seconds and the four beats as PNG (the start of the tell, its end,
    // the impact, half a second after).
    private IEnumerator Film(string name, Func<bool> going, float most, Func<bool> tellEnded, Func<bool> impact, float every = 0.1f)
    {
        string previous = label;
        label = name;
        string folder = root + "/" + name;
        Directory.CreateDirectory(folder + "/seq");
        foreach (var old in Directory.GetFiles(folder + "/seq")) File.Delete(old);
        seqEvery = every;
        seqIndex = 0;
        nextSeq = 0f;
        seqFolder = folder + "/seq";
        Beat(folder, "b1_tell_start");
        bool b2 = false, b3 = false;
        float b4At = -1f, began = Time.time, realBegan = Time.realtimeSinceStartup;
        while (going() && Time.time - began < most && Time.realtimeSinceStartup - realBegan < most * 4f + 10f)
        {
            if (!b2 && tellEnded()) { b2 = true; Beat(folder, "b2_tell_end"); }
            if (!b3 && impact()) { b3 = true; Beat(folder, "b3_impact"); b4At = Time.time + 0.5f; }
            if (b4At > 0f && Time.time >= b4At) { Beat(folder, "b4_after"); b4At = -2f; }
            yield return null;
        }
        float tail = Time.time + 0.6f;
        while (Time.time < tail && Time.realtimeSinceStartup - realBegan < most * 4f + 12f)
        {
            if (b4At > 0f && Time.time >= b4At) { Beat(folder, "b4_after"); b4At = -2f; }
            yield return null;
        }
        if (b4At > 0f) Beat(folder, "b4_after");
        yield return null;
        seqFolder = null;
        Vector3 subject = form != null && form.HasRisen ? form.BodyAt : boss.transform.position;
        Say($"   filmed {name}: {seqIndex} frames over {Time.time - began:0.0} s; tell ended {b2} impact {b3}; camera {lens.transform.eulerAngles.x:0} degrees down, {Vector3.Distance(lens.transform.position, player.transform.position):0.0} m from the player, {Vector3.Distance(lens.transform.position, subject):0.0} m from the boss");
        label = previous;
    }

    // Reads out any box of words, closes the guide and the rest menu, and says whether time moves after.
    private IEnumerator ClearScreens()
    {
        for (int i = 0; i < 60; i++)
        {
            if (ReadingBox.Instance != null && ReadingBox.Instance.IsOpen)
            {
                Keys(Key.E);
                yield return null;
                yield return null;
                Keys();
                yield return null;
                yield return new WaitForSecondsRealtime(0.05f);
                continue;
            }
            if (GuidePage.Showing)
            {
                Keys(Key.Space);
                yield return null;
                yield return null;
                Keys();
                yield return null;
                if (GuidePage.Showing) GuidePage.Instance.Close();
                guideClosed++;
                continue;
            }
            if (RestMenu.Showing)
            {
                RestMenu.Instance.Close();
                continue;
            }
            break;
        }
        float was = Time.time;
        int frame = Time.frameCount;
        yield return new WaitForSecondsRealtime(0.25f);
        if (Time.time <= was) Say($"WRONG: time does not move (time scale {Time.timeScale}, frames {Time.frameCount - frame})");
    }

    // ------------------------------------------------------------------ start

    private IEnumerator Start()
    {
        player = FindAnyObjectByType<PlayerController>();
        combat = player.GetComponent<PlayerCombat>();
        defence = player.GetComponent<PlayerDefence>();
        court = FindAnyObjectByType<SunCourtDirector>();
        form = FindAnyObjectByType<SunBearer>();
        boss = FindAnyObjectByType<AreaDirector>().Enemies[0];
        lens = Camera.main;
        topDown = lens.GetComponent<TopDownCamera>();
        only = File.Exists("AgentScripts/khepharu_only.txt") ? File.ReadAllText("AgentScripts/khepharu_only.txt").Trim() : "";
        if (only.Split(' ').Contains("Close"))
        {
            cam = "close";
            only = string.Join(" ", only.Split(' ').Where(w => w != "Close" && w.Length > 0));
        }
        root = "review/khepharu/audit/" + cam;
        Directory.CreateDirectory(root + "/worst");
        csv = new StreamWriter(root + "/frames.csv", false);
        csv.WriteLine("frame,time,camera,label,doing,doing_for,sun_state,sun_size,sun_y,floor_depth,floor_part,body_depth,body_piece,box_depth,box_piece,boss_on_screen,falcon,obelisk_covers_player,obelisk_covers_boss,last_frame");
        mainThread = ProfilerRecorder.StartNew(ProfilerCategory.Internal, "Main Thread", 1);
        garbage = ProfilerRecorder.StartNew(ProfilerCategory.Memory, "GC Allocated In Frame");

        var formRoot = form.transform;
        var listed = new List<Renderer>();
        foreach (var shown in formRoot.GetComponentsInChildren<Renderer>(true))
        {
            string path = shown.transform.name;
            for (var up = shown.transform.parent; up != null && up != formRoot; up = up.parent) path = up.name + "/" + path;
            if (!path.StartsWith("Body/") || path.Contains("Reach") || path.Contains("Bulk")) continue;
            listed.Add(shown);
        }
        pieces = listed.ToArray();
        sunRoot = formRoot.Find("Sun");
        sunRenderers = sunRoot.GetComponentsInChildren<Renderer>(true);
        var flags = BindingFlags.Instance | BindingFlags.NonPublic;
        sunStateField = typeof(SunBearer).GetField("sunState", flags);
        rollingField = typeof(SunBearer).GetField("rolling", flags);
        sunAtField = typeof(SunBearer).GetField("sunAt", flags);
        foreach (var collider in FindObjectsByType<Collider>(FindObjectsSortMode.None))
        {
            string names = collider.name + " " + (collider.transform.parent != null ? collider.transform.parent.name : "");
            if (names.ToLower().Contains("obelisk")) obelisk.Add(collider);
        }
        LoadDump();
        wasBackground = InputSystem.settings.backgroundBehavior;
        wasEditorInput = InputSystem.settings.editorInputBehaviorInPlayMode;
        InputSystem.settings.backgroundBehavior = InputSettings.BackgroundBehavior.IgnoreFocus;
        InputSystem.settings.editorInputBehaviorInPlayMode = InputSettings.EditorInputBehaviorInPlayMode.AllDeviceInputAlwaysGoesToGameView;
        StartCoroutine(EndOfFrames());
        StartCoroutine(Watchdog());
        Say($"start: camera {cam}, parts '{only}', {pieces.Length} body pieces, {sunRenderers.Length} sun renderers, {obelisk.Count} obelisk colliders, screen {Screen.width}x{Screen.height}");

        yield return new WaitForSecondsRealtime(0.5f);
        yield return ClearScreens();
        float before = Time.time;
        yield return new WaitForSecondsRealtime(0.6f);
        Say($"time moves: {before:0.00} -> {Time.time:0.00} s, time scale {Time.timeScale}, guide closed {guideClosed} time(s) {(Time.time > before + 0.3f ? "OK" : "WRONG")}");
        SunSlots();
        FloorTops();
        combat.Equip(Weapons.ByName(Weapons.GreatSpearName));
        yield return Wait(0.5f);
        if (cam == "close")
        {
            camYaw = 90f;
            overrideCam = true;
        }

        if (Wants("Intro")) yield return Intro();
        if (!court.Begun)
        {
            Go(new Vector3(0f, 0f, -18.6f), Vector3.forward);
            yield return Wait(0.4f);
            yield return ClearScreens();
        }
        if (Wants("Man")) yield return Man();
        yield return Change(Wants("Change"));
        form.OnlyWhenAsked = true;
        yield return Until(() => form.Doing == "Idle", 14f);
        measuring = true;
        timesPart = "form";
        if (Wants("Form")) yield return FormMoves();
        if (Wants("Cases")) yield return Cases();
        if (Wants("Edges")) yield return Edges();
        if (Wants("Beats")) yield return Beats();
        measuring = false;
        timesPart = "";
        ReleaseCamera();
        Report();
        Say("done");
        Debug.Log("[Probe] done");
        Destroy(gameObject);
    }

    private InputSettings.BackgroundBehavior wasBackground;
    private InputSettings.EditorInputBehaviorInPlayMode wasEditorInput;
    private bool reading;
    private int boxesClosed;

    // A box of words left open holds the player (no blow lands on them) and covers the picture: read it out.
    private IEnumerator Watchdog()
    {
        while (true)
        {
            yield return new WaitForSecondsRealtime(0.4f);
            if (this == null) yield break;
            if (reading || ReadingBox.Instance == null || !ReadingBox.Instance.IsOpen) continue;
            reading = true;
            boxesClosed++;
            for (int i = 0; i < 30 && ReadingBox.Instance.IsOpen; i++)
            {
                Keys(Key.E);
                yield return null;
                yield return null;
                Keys();
                yield return null;
                yield return new WaitForSecondsRealtime(0.08f);
            }
            if (ReadingBox.Instance.IsOpen) Say("WRONG: a box of words would not close");
            reading = false;
        }
    }

    private void OnDestroy()
    {
        InputSystem.settings.backgroundBehavior = wasBackground;
        InputSystem.settings.editorInputBehaviorInPlayMode = wasEditorInput;
        if (csv != null) csv.Dispose();
        if (mainThread.Valid) mainThread.Dispose();
        if (garbage.Valid) garbage.Dispose();
        if (Time.timeScale == 0f) Time.timeScale = 1f;
        if (topDown != null) topDown.enabled = true;
    }

    // Brief section 6 item 11: the sun's submeshes and materials as built, and which are drawn.
    private void SunSlots()
    {
        foreach (var shown in sunRenderers)
        {
            var filter = shown.GetComponent<MeshFilter>();
            string mesh = filter != null && filter.sharedMesh != null ? filter.sharedMesh.name : "(" + shown.GetType().Name + ")";
            var made = new StringBuilder();
            var data = filter != null && filter.sharedMesh != null ? Data(filter.sharedMesh) : null;
            if (data != null)
            {
                for (int s = 0; s < filter.sharedMesh.subMeshCount; s++)
                {
                    float far = 0f;
                    foreach (int index in data.SubVerts[s]) far = Mathf.Max(far, data.Verts[index].magnitude);
                    string material = s < shown.sharedMaterials.Length && shown.sharedMaterials[s] != null ? shown.sharedMaterials[s].name : "none";
                    made.Append($" [{s}: {material}, {data.SubVerts[s].Length} vertices, reaching {far:0.00} m from the middle]");
                }
            }
            Say($"sun slot: {shown.name} mesh {mesh} active {shown.gameObject.activeInHierarchy} enabled {shown.enabled} scale {shown.transform.lossyScale.x:0.00}{made}");
        }
    }

    // The court's drawn floor: the top of every wide flat thing near the middle, and what a ray down meets.
    private void FloorTops()
    {
        var tops = new List<string>();
        foreach (var shown in FindObjectsByType<Renderer>(FindObjectsSortMode.None))
        {
            var bounds = shown.bounds;
            if (bounds.size.x > 8f && bounds.size.z > 8f && bounds.size.y < 0.2f && Mathf.Abs(bounds.center.x) < 3f && Mathf.Abs(bounds.center.z) < 3f)
                tops.Add($"{shown.name} {bounds.max.y:0.000}");
        }
        string ray = Physics.Raycast(new Vector3(6f, 5f, 6f), Vector3.down, out RaycastHit hit, 10f, ~0, QueryTriggerInteraction.Ignore) ? $"{hit.collider.name} at {hit.point.y:0.000}" : "nothing";
        Say($"floor: drawn tops {string.Join(", ", tops.Take(14))}; a ray down at (6, 6) meets {ray}");
    }

    // ------------------------------------------------------------------ B and C: the intro, the man, the change

    private IEnumerator Intro()
    {
        Say("Intro: from the forecourt to the end of the king's words");
        Vector3 from = Flat(player.transform.position), to = new Vector3(0f, 0f, -18.2f);
        if (from.z > -20f) from = new Vector3(0f, 0f, -26f);
        float walked = 0f;
        follow = () => Vector3.Lerp(from, to, Mathf.Clamp01(walked / 4f)) + Vector3.up * 0.1f;
        timesPart = "intro";
        string folder = root + "/intro";
        Directory.CreateDirectory(folder);
        yield return Film("intro", () => { walked += Time.deltaTime; return walked < 4.2f && !(ReadingBox.Instance != null && ReadingBox.Instance.IsOpen); }, 6f, () => court.Begun, () => false, 0.5f);
        follow = null;
        reading = true;
        for (int i = 0; i < 20 && ReadingBox.Instance != null && ReadingBox.Instance.IsOpen; i++)
        {
            yield return new WaitForSecondsRealtime(0.3f);
            Beat(folder, "line_" + i);
            yield return null;
            yield return null;
            Keys(Key.E);
            yield return null;
            yield return null;
            Keys();
            yield return new WaitForSecondsRealtime(0.1f);
        }
        reading = false;
        yield return ClearScreens();
        yield return Wait(0.6f);
        Beat(folder, "after_words");
        yield return Wait(0.2f);
        timesPart = "";
        Say($"   intro: begun {court.Begun}, man at {boss.transform.position:0.0}, the man {ManPixels():0} px tall at the duel camera");
    }

    private float ManPixels()
    {
        var all = boss.GetComponentsInChildren<SkinnedMeshRenderer>().Where(r => r.enabled).ToArray();
        if (all.Length == 0) return 0f;
        var bounds = all[0].bounds;
        foreach (var shown in all) bounds.Encapsulate(shown.bounds);
        Vector3 top = lens.WorldToScreenPoint(new Vector3(boss.transform.position.x, bounds.max.y, boss.transform.position.z));
        Vector3 foot = lens.WorldToScreenPoint(new Vector3(boss.transform.position.x, boss.transform.position.y, boss.transform.position.z));
        return Mathf.Abs(top.y - foot.y);
    }

    private IEnumerator ManMove(string name, float distance, int chain, bool fresh = true)
    {
        if (fresh)
        {
            boss.Restore();
            boss.transform.SetPositionAndRotation(new Vector3(6f, 0.05f, 6f), Quaternion.LookRotation(Vector3.back));
        }
        Vector3 stand = boss.transform.position + Vector3.back * distance;
        Go(stand, Vector3.forward);
        if (fresh) boss.TakeHit(new Hit(0.01f, 0f, Vector3.forward));
        yield return Wait(0.2f);
        stand = boss.transform.position + boss.transform.forward * distance;
        Go(stand, boss.transform.position - stand);
        int taken = defence.BlowsTaken;
        bool forced = boss.Brain.Force(name);
        var attack = boss.Brain.Current;
        float total = 0f;
        var step = attack;
        for (int i = 0; i < chain && step != null; i++)
        {
            total += step.Total;
            step = string.IsNullOrEmpty(step.ChainInto) ? null : boss.Brain.Attacks.Find(a => a.Name == step.ChainInto);
        }
        float began = Time.time;
        float px = ManPixels();
        yield return Film(name.Replace("sunking_", "man_"), () => Time.time - began < total + 0.6f, total + 1f,
            () => boss.Brain.TelegraphProgress >= 0.95f, () => (Time.time - began > 0.15f && boss.Brain.State != EnemyState.Telegraph) || defence.BlowsTaken > taken);
        Say($"   {name}: forced {forced}, {total:0.00} s in all, struck +{defence.BlowsTaken - taken}, the man {px:0} px tall from {Vector3.Distance(lens.transform.position, boss.transform.position):0.0} m");
    }

    private IEnumerator Man()
    {
        Say("Man: each move on a player standing still, four beats each");
        timesPart = "man";
        yield return ManMove("sunking_thrust_1", 4f, 3);
        yield return ManMove("sunking_reach", 5.5f, 1);
        yield return ManMove("sunking_sweep", 2.4f, 1);
        yield return ManMove("sunking_butt", 1.7f, 1);
        yield return ManMove("sunking_throw", 9f, 2);
        yield return ManMove("sunking_noon_1", 10f, 3);
        // Below half: the planted spear at once, then the drive and its sweep.
        boss.Restore();
        boss.transform.SetPositionAndRotation(new Vector3(6f, 0.05f, 6f), Quaternion.LookRotation(Vector3.back));
        Go(new Vector3(6f, 0f, 3f), Vector3.forward);
        boss.TakeHit(new Hit(boss.Health.Maximum * 0.55f, 0f, Vector3.forward, true, true));
        yield return Wait(0.1f);
        Go(new Vector3(6f, 0f, 0f), Vector3.forward);
        float began = Time.time;
        yield return Film("man_plant", () => Time.time - began < 4.5f, 5f, () => boss.Brain.TelegraphProgress >= 0.95f, () => Time.time - began > 1.2f);
        yield return Wait(4f);
        yield return ManMove("sunking_drive", 4f, 2, false);
        timesPart = "";
    }

    private IEnumerator Change(bool film)
    {
        Say("Change: the man felled and the Sun-Bearer rising");
        boss.Restore();
        boss.transform.SetPositionAndRotation(new Vector3(0f, 0.05f, 8f), Quaternion.LookRotation(Vector3.back));
        Go(new Vector3(0f, 0f, -6f), Vector3.forward);
        boss.TakeHit(new Hit(0.01f, 0f, Vector3.forward));
        yield return Wait(0.8f);
        for (int blow = 0; blow < 200 && !boss.IsDead; blow++) boss.TakeHit(new Hit(60f, 0f, Vector3.forward, true, true));
        timesPart = "change";
        measuring = true;
        if (film)
            yield return Film("change", () => form.Doing == "Hidden" || form.Doing == "Entering", 10f,
                () => form.Doing == "Entering" && form.DoingFor >= form.SunLandsAt, () => form.Doing == "Entering" && form.DoingFor >= form.RoarAt, 0.33f);
        else
            yield return Until(() => form.HasRisen && form.Doing != "Entering", 12f);
        timesPart = "";
        Say($"   the form stands: doing {form.Doing}, the scarab covers {ScarabPixels()} of the screen");
    }

    // How much of the screen the drawn body covers: the box round every drawn piece's corners, in pixels.
    private string BodyPixels()
    {
        float left = 1e9f, right = -1e9f, low = 1e9f, high = -1e9f;
        foreach (var piece in pieces)
        {
            if (piece == null || !piece.enabled || !piece.gameObject.activeInHierarchy) continue;
            var b = piece.bounds;
            for (int i = 0; i < 8; i++)
            {
                Vector3 corner = new Vector3((i & 1) == 0 ? b.min.x : b.max.x, (i & 2) == 0 ? b.min.y : b.max.y, (i & 4) == 0 ? b.min.z : b.max.z);
                Vector3 seen = lens.WorldToScreenPoint(corner);
                if (seen.z <= 0f) continue;
                left = Mathf.Min(left, seen.x); right = Mathf.Max(right, seen.x);
                low = Mathf.Min(low, seen.y); high = Mathf.Max(high, seen.y);
            }
        }
        return right > left ? $"{right - left:0}x{high - low:0} px" : "not drawn";
    }

    private string ScarabPixels() => BodyPixels();

    private string FalconPixels() => BodyPixels();

    // ------------------------------------------------------------------ D: every move of the form

    private IEnumerator Idle(float timeout = 16f)
    {
        yield return Until(() => form.Doing == "Idle", timeout);
    }

    private IEnumerator Shape(bool falcon)
    {
        yield return Idle();
        if (form.IsFalcon == falcon) yield break;
        form.Queued = falcon ? SunBearerAttack.Rising : SunBearerAttack.Setting;
        yield return Until(() => form.Doing == (falcon ? "Rising" : "Setting"), 12f);
        yield return Until(() => form.Doing == "Idle", 12f);
    }

    private Vector3 RoundFrom(float degrees, float radius)
    {
        Vector3 flat = Flat(form.BodyAt);
        if (flat.sqrMagnitude < 1f) flat = Vector3.forward;
        return Quaternion.Euler(0f, degrees, 0f) * flat.normalized * radius;
    }

    private static readonly Vector3 South = new Vector3(0f, 0f, -9f);

    private Vector3 TowardSouth(float distance) => Flat(form.BodyAt) + Flat(South - form.BodyAt).normalized * distance;

    private IEnumerator Move(SunBearerAttack attack, Func<Vector3> standOf, string name = null, float most = 18f)
    {
        string doing = attack.ToString();
        name = name ?? doing;
        bool falconMove = attack == SunBearerAttack.Stoop || attack == SunBearerAttack.Pass || attack == SunBearerAttack.Feathers || attack == SunBearerAttack.Zenith
            || attack == SunBearerAttack.Gust || attack == SunBearerAttack.Grab || attack == SunBearerAttack.Sunfall || attack == SunBearerAttack.Setting;
        if (attack != SunBearerAttack.Rising && attack != SunBearerAttack.Setting) yield return Shape(falconMove);
        else yield return Shape(attack == SunBearerAttack.Setting);
        yield return Idle();
        Vector3 stand = standOf();
        Go(stand, form.BodyAt - stand);
        int taken = defence.BlowsTaken;
        form.Queued = attack;
        yield return Until(() => form.Doing == doing, 12f);
        if (form.Doing != doing)
        {
            Say($"   {name}: never began (doing {form.Doing})");
            yield break;
        }
        Go(stand, form.BodyAt - stand);
        string size = BodyPixels();
        yield return Film(name, () => form.Doing == doing || (doing == "Zenith" && form.Doing == "Spent"), most, () => form.Locked, () => defence.BlowsTaken > taken);
        Say($"   {name}: struck +{defence.BlowsTaken - taken} ({form.LastBlow}); {(form.IsFalcon ? "the falcon" : "the scarab")} covers {size} at the start, {BodyPixels()} at the end");
        if (form.OnHotFloor(player.transform.position)) Go(new Vector3(0f, 0f, -15f), Vector3.forward);
    }

    // The shell's flare and the kick answer a player who stays on the flank or behind.
    private IEnumerator Camp(bool behind)
    {
        string doing = behind ? "Kick" : "ShellBurst";
        yield return Shape(false);
        yield return Idle();
        follow = () =>
        {
            Vector3 forward = BodyForward;
            Vector3 spot = behind ? Flat(form.BodyAt) - forward * 4f : Flat(form.BodyAt) + Vector3.Cross(Vector3.up, forward) * 3.2f;
            if (spot.magnitude > 18f) spot = spot.normalized * 18f;
            return spot + Vector3.up * 0.1f;
        };
        int taken = defence.BlowsTaken;
        yield return Until(() => form.Doing == doing || form.Doing == "Kick" || form.Doing == "ShellBurst", 9f);
        follow = null;
        if (form.Doing != "Kick" && form.Doing != "ShellBurst")
        {
            Say($"   {doing}: never began with the form held to asked moves (doing {form.Doing})");
            yield break;
        }
        doing = form.Doing;
        yield return Film(doing, () => form.Doing == doing, 6f, () => form.DoingFor >= (doing == "Kick" ? form.KickTell : form.BurstTell), () => defence.BlowsTaken > taken);
        Say($"   {doing}: struck +{defence.BlowsTaken - taken} ({form.LastBlow})");
    }

    private IEnumerator FormMoves()
    {
        Say("Form: every move of the Sun-Bearer on a player standing in its mark");
        yield return Move(SunBearerAttack.Roll, () => RoundFrom(75f, 12.5f));
        yield return Until(() => form.HotLanes == 0, 10f);
        yield return Move(SunBearerAttack.Furrows, () => RoundFrom(75f, 12.5f));
        yield return Until(() => form.HotLanes == 0, 12f);
        yield return Move(SunBearerAttack.Ram, () => TowardSouth(6f));
        yield return Move(SunBearerAttack.Sparks, () => TowardSouth(7f));
        yield return Move(SunBearerAttack.Ricochet, () => RoundFrom(120f, 10f));
        yield return Until(() => form.HotLanes == 0, 8f);
        yield return Move(SunBearerAttack.Burrow, () => TowardSouth(8f));
        yield return Move(SunBearerAttack.Flares, () => TowardSouth(9f));
        yield return Camp(false);
        yield return Camp(true);
        yield return Move(SunBearerAttack.Rising, () => TowardSouth(7f));
        yield return Move(SunBearerAttack.Stoop, () => new Vector3(6f, 0f, -8f));
        yield return Move(SunBearerAttack.Pass, () => new Vector3(8f, 0f, -7f));
        yield return Until(() => form.HotLanes == 0, 8f);
        yield return Move(SunBearerAttack.Feathers, () => new Vector3(-7f, 0f, -8f));
        yield return Move(SunBearerAttack.Gust, () => new Vector3(4f, 0f, -9f));
        yield return Move(SunBearerAttack.Grab, () => new Vector3(-6f, 0f, -8f));
        yield return Move(SunBearerAttack.Sunfall, () => new Vector3(5f, 0f, -10f));
        yield return Until(() => form.HotPatches == 0, 6f);
        yield return Move(SunBearerAttack.Zenith, () => new Vector3(-8f, 0f, -9f), "Zenith", 20f);
        yield return Move(SunBearerAttack.Setting, () => new Vector3(0f, 0f, -10f));
    }

    // ------------------------------------------------------------------ E: the clipping cases

    private void PlaceSun(Vector3 where)
    {
        var rolling = (RollingSun)rollingField.GetValue(form);
        Vector3 now = (Vector3)sunAtField.GetValue(form);
        where.y = now.y;
        rolling.Stop();
        rolling.Position = where;
        sunAtField.SetValue(form, where);
    }

    private Vector3 Inside(Vector3 point, float most = 16.5f, float least = 5f)
    {
        point = Flat(point);
        if (point.magnitude > most) point = point.normalized * most;
        if (point.magnitude < least) point = (point.sqrMagnitude > 0.01f ? point.normalized : Vector3.back) * least;
        return point;
    }

    private IEnumerator KnockLoose()
    {
        yield return Shape(false);
        yield return Idle();
        yield return Until(() => !form.Poise.Holds, 14f);
        for (int i = 0; i < 6 && !form.SunLoose; i++) form.BodyPart.TakeHit(new Hit(10f, 0f, Vector3.right));
        yield return Wait(0.3f);
    }

    private IEnumerator Case(string name, Func<IEnumerator> body, float most)
    {
        Say($"case {name}");
        bool done = false;
        var running = StartCoroutine(Mark(body(), () => done = true));
        yield return Film("case_" + name, () => !done, most, () => false, () => false);
        if (!done)
        {
            StopCoroutine(running);
            Say($"   case {name}: cut off after {most} s");
        }
        follow = null;
        Keys();
        yield return Until(() => form.Doing == "Idle" && !form.SunLoose, 20f);
    }

    private IEnumerator Mark(IEnumerator body, Action finished)
    {
        yield return body;
        finished();
    }

    private IEnumerator Cases()
    {
        Say("Cases: the sun against the body, each case filmed and measured every frame");
        yield return KnockLooseFirst();
        yield return Case("parry", Parry, 30f);
        yield return Case("ram_front", () => RamFront(0f), 14f);
        yield return Case("ram_front_2", () => RamFront(40f), 14f);
        yield return Case("roll_behind", () => Behind(SunBearerAttack.Roll), 14f);
        yield return Case("ricochet_behind", () => Behind(SunBearerAttack.Ricochet), 14f);
        yield return Case("furrows_behind", () => Behind(SunBearerAttack.Furrows), 22f);
        yield return Case("light_into_on_back", () => Into(false, false), 20f);
        yield return Case("light_into_righted", () => Into(false, true), 20f);
        yield return Case("heavy_into_righted", () => Into(true, true), 20f);
        yield return Case("heavy_into_on_back", () => Into(true, false), 20f);
        yield return Case("takeup_sun_behind", () => Loose(5.5f, 180f), 20f);
        yield return Case("walk_sun_far_side", () => Loose(7.2f, 150f), 20f);
        yield return Case("dig_back", () => Loose(12f, 90f), 24f);
        for (int i = 0; i < 3; i++)
        {
            int k = i;
            yield return Case("ricochet_pass_" + (k + 1), () => RicochetPass(k), 14f);
        }
        yield return Case("falcon_hop", FalconHop, 30f);
    }

    private IEnumerator KnockLooseFirst()
    {
        yield return Shape(false);
        yield return Idle();
    }

    private IEnumerator Parry()
    {
        yield return Shape(false);
        yield return Idle();
        Vector3 stand = RoundFrom(75f, 12.5f);
        form.Queued = SunBearerAttack.Roll;
        Go(stand, form.BodyAt - stand);
        yield return Until(() => form.Doing == "Roll", 12f);
        yield return Until(() => form.Locked || form.Doing != "Roll", 6f);
        Go(stand, form.SunAt - stand);
        int knocks = form.SunKnocks, perfect = defence.PerfectGuards;
        float until = Time.time + 8f;
        bool pressed = false;
        while (Time.time < until && form.Doing == "Roll" && form.SunKnocks == knocks)
        {
            if (!pressed && Flat(form.SunAt - player.transform.position).magnitude < 3.1f)
            {
                Keys(Key.L);
                pressed = true;
            }
            yield return null;
        }
        yield return Wait(0.4f);
        Keys();
        Say($"   parry: perfect +{defence.PerfectGuards - perfect} knocks +{form.SunKnocks - knocks} parries {form.Parries} flying back {form.SunFlyingBack}");
        yield return Until(() => !form.SunFlyingBack, 3f);
        yield return Wait(1.5f);
    }

    // The Ram with the player beyond its sun, so the lunge crosses where the ball lies.
    private IEnumerator RamFront(float turn)
    {
        yield return Shape(false);
        yield return Idle();
        Vector3 body = Flat(form.BodyAt), sun = Flat(form.SunAt);
        Vector3 way = Quaternion.Euler(0f, turn, 0f) * (sun - body).normalized;
        Vector3 stand = Inside(sun + way * 4.5f);
        Go(stand, body - stand);
        form.Queued = SunBearerAttack.Ram;
        yield return Until(() => form.Doing == "Ram", 12f);
        Go(stand, body - stand);
        yield return Until(() => form.Doing != "Ram", 10f);
        Say($"   ram: the sun lay {Flat(sun - body).magnitude:0.0} m ahead; the player {Flat(stand - body).magnitude:0.0} m off; struck {form.LastBlow}");
    }

    private IEnumerator Behind(SunBearerAttack attack)
    {
        yield return Shape(false);
        yield return Idle();
        Vector3 stand = Inside(Flat(form.BodyAt) - BodyForward * 8f);
        Go(stand, form.BodyAt - stand);
        form.Queued = attack;
        string doing = attack.ToString();
        yield return Until(() => form.Doing == doing, 12f);
        yield return Until(() => form.Doing != doing, 18f);
    }

    // The loose sun set rolling into the scarab: light or heavy, while it lies on its back or once it has righted itself.
    private IEnumerator Into(bool heavy, bool righted)
    {
        yield return KnockLoose();
        if (righted) yield return Until(() => !form.Exposed || form.DoingFor > 4.4f, 6f);
        Vector3 body = Flat(form.BodyAt);
        Vector3 side = Vector3.Cross(Vector3.up, BodyForward);
        if ((body + side * 6f).magnitude > 17f) side = -side;
        PlaceSun(body + side * (heavy ? 7f : 3.6f));
        yield return null;
        Vector3 sun = Flat(form.SunAt);
        Vector3 stand = sun + (sun - body).normalized * 2.6f;
        Go(stand, sun - stand);
        int strikes = form.SunStrikes;
        form.SunPart.TakeHit(new Hit(1f, 0f, (body - sun).normalized, heavy));
        yield return Wait(2.5f);
        Say($"   {(heavy ? "heavy" : "light")} into the {(righted ? "righted" : "upturned")} scarab: shell strikes +{form.SunStrikes - strikes}, doing {form.Doing}, the sun {Flat(form.SunAt - form.BodyAt).magnitude:0.0} m from it");
        yield return Until(() => !form.SunLoose, 16f);
    }

    // The sun left lying this far off, at this angle from the way the scarab faces: it walks, or digs, and takes it up.
    private IEnumerator Loose(float distance, float degrees)
    {
        yield return KnockLoose();
        Vector3 body = Flat(form.BodyAt);
        Vector3 where = body + Quaternion.Euler(0f, degrees, 0f) * BodyForward * distance;
        if (where.magnitude > 16.5f || where.magnitude < 5f) where = body + Quaternion.Euler(0f, -degrees, 0f) * BodyForward * distance;
        PlaceSun(Inside(where, 16.5f, 5f));
        Vector3 stand = Inside(body + (body - Flat(form.SunAt)).normalized * 6f);
        Go(stand, form.BodyAt - stand);
        float began = Time.time;
        yield return Until(() => !form.SunLoose, 18f);
        Say($"   the sun left {distance:0.0} m off at {degrees:0} degrees: taken up after {Time.time - began:0.0} s ({(form.SunLoose ? "still loose" : "held")})");
    }

    private IEnumerator RicochetPass(int which)
    {
        yield return Shape(false);
        yield return Idle();
        float[] turns = { 35f, -35f, 160f };
        Vector3 stand = Inside(Quaternion.Euler(0f, turns[which], 0f) * Flat(form.BodyAt).normalized * 16f);
        Go(stand, form.BodyAt - stand);
        form.Queued = SunBearerAttack.Ricochet;
        yield return Until(() => form.Doing == "Ricochet", 12f);
        yield return Until(() => form.Doing != "Ricochet", 10f);
    }

    private IEnumerator FalconHop()
    {
        yield return Shape(true);
        yield return Idle();
        Go(new Vector3(6f, 0f, -8f), Vector3.forward);
        form.Queued = SunBearerAttack.Stoop;
        yield return Until(() => form.Doing == "Stoop", 12f);
        yield return Until(() => form.BodyPart.Vulnerable || form.Doing != "Stoop", 5f);
        for (int blow = 0; blow < 4 && !form.SunLoose; blow++) form.BodyPart.TakeHit(new Hit(10f, 0f, Vector3.right));
        yield return Wait(0.6f);
        if (form.SunLoose)
        {
            Vector3 body = Flat(form.BodyAt);
            PlaceSun(Inside(body + Flat(body).normalized * -6f));
        }
        Go(new Vector3(6f, 0f, -14f), Vector3.forward);
        yield return Until(() => !form.SunLoose, 20f);
        Say($"   the falcon's hop: loose {form.SunLoose} exposed {form.Exposed} doing {form.Doing}");
    }

    // ------------------------------------------------------------------ F: the arena's edges

    private static Key[] Toward(Vector3 way)
    {
        var held = new List<Key>();
        if (way.z > 0.38f) held.Add(Key.W);
        if (way.z < -0.38f) held.Add(Key.S);
        if (way.x > 0.38f) held.Add(Key.D);
        if (way.x < -0.38f) held.Add(Key.A);
        return held.ToArray();
    }

    private IEnumerator Push(Vector3 from, Vector3 way, float seconds, Action<Vector3, float, float> result)
    {
        Go(from, way);
        yield return Wait(0.15f);
        Keys(Toward(way));
        float most = 0f, low = 99f, end = Time.time + seconds;
        while (Time.time < end)
        {
            Vector3 at = player.transform.position;
            most = Mathf.Max(most, Vector3.Dot(Flat(at - from), way));
            low = Mathf.Min(low, at.y);
            yield return null;
        }
        Keys();
        yield return Wait(0.1f);
        result(player.transform.position, most, low);
    }

    private IEnumerator Edges()
    {
        Say("Edges: the rim walked and pushed every 15 degrees, the forecourt's mouth, the obelisk, and what lies beyond");
        bool wasMeasuring = measuring;
        measuring = false;
        yield return Shape(false);
        yield return Idle();
        string folder = root + "/edges";
        Directory.CreateDirectory(folder);
        float outermost = 0f, lowest = 99f;
        var rows = new List<string>();
        for (int degrees = 0; degrees < 360; degrees += 15)
        {
            Vector3 way = Polar(degrees, 1f);
            Vector3 end = Vector3.zero;
            float most = 0f, low = 0f;
            yield return Push(way * 19.3f, way, 1.0f, (at, m, l) => { end = at; most = m; low = l; });
            float radius = Flat(end).magnitude;
            outermost = Mathf.Max(outermost, radius);
            lowest = Mathf.Min(lowest, low);
            rows.Add($"{degrees}: ended r {radius:0.00} y {end.y:0.00} (lowest {low:0.00})");
            if (degrees % 30 == 0)
            {
                Beat(folder, "rim_" + degrees.ToString("000"));
                yield return null;
                yield return null;
            }
        }
        Say($"   the rim pushed outward from r 19.3: furthest r {outermost:0.00}, lowest y {lowest:0.00} {(outermost < 20.4f && lowest > -0.5f ? "OK" : "WRONG")}; {string.Join("; ", rows)}");
        rows.Clear();
        foreach (float x in new[] { -4f, -2f, 0f, 2f, 4f })
        {
            Vector3 end = Vector3.zero;
            float low = 0f;
            yield return Push(new Vector3(x, 0f, -19.3f), Vector3.back, 1.0f, (at, m, l) => { end = at; low = l; });
            rows.Add($"x {x:0}: ended z {end.z:0.00} y {end.y:0.00}");
        }
        Beat(folder, "south_mouth");
        yield return null;
        Say($"   the forecourt's mouth pushed south (the wall of heat {(court.Begun ? "closed" : "open")}): {string.Join("; ", rows)}");
        rows.Clear();
        float nearest = 99f;
        for (int degrees = 0; degrees < 360; degrees += 45)
        {
            Vector3 way = Polar(degrees, 1f);
            Vector3 end = Vector3.zero;
            yield return Push(way * 3.4f, -way, 0.9f, (at, m, l) => end = at);
            float square = Mathf.Max(Mathf.Abs(end.x), Mathf.Abs(end.z));
            nearest = Mathf.Min(nearest, square);
            rows.Add($"{degrees}: stopped at ({end.x:0.00}, {end.z:0.00})");
        }
        Beat(folder, "obelisk_close");
        yield return null;
        var capsule = player.GetComponent<CharacterController>();
        Say($"   the player's collider radius {(capsule != null ? capsule.radius : -1f):0.00} m");
        Say($"   the obelisk pushed into from eight sides: nearest {nearest:0.00} m from its middle on either axis (its foot is 1.2 m half-across) {(nearest >= 1.15f ? "OK" : "WRONG")}; {string.Join("; ", rows)}");

        // What lies beyond, from inside the court looking out over each quarter of the wall.
        foreach (var (degrees, name) in new[] { (0f, "north"), (90f, "east"), (180f, "south"), (270f, "west") })
        {
            Vector3 spot = Polar(degrees, 18.5f);
            Go(spot, Polar(degrees, 1f));
            yield return Wait(0.6f);
            Beat(folder, "beyond_" + name + "_game");
            yield return null;
            yield return null;
            fixedView = true;
            overrideCam = true;
            fixedAim = Polar(degrees, 24f) + Vector3.up * 2f;
            fixedYaw = degrees;
            fixedPitch = 28f;
            fixedDistance = 30f;
            yield return Wait(0.2f);
            Beat(folder, "beyond_" + name + "_out");
            yield return null;
            yield return null;
            fixedView = false;
            if (cam != "close") ReleaseCamera();
        }
        Go(new Vector3(0f, 0f, -12f), Vector3.forward);
        measuring = wasMeasuring;
    }

    // ------------------------------------------------------------------ C again: the fury, the stagger, the death

    private IEnumerator Beats()
    {
        Say("Beats: the fury's start and its blaze, the Flares in fury, the stagger and the finisher, the death");
        yield return Shape(false);
        yield return Idle();
        timesPart = "fury zenith";
        form.Health.Damage(Mathf.Max(0f, form.Health.Current - form.MaximumHealth * 0.45f));
        follow = () => court.Hand.Centre + court.Hand.Direction * 9f + Vector3.up * 0.1f;
        yield return Until(() => form.Doing == "Rising" || form.Doing == "Zenith", 10f);
        yield return Film("fury_zenith", () => form.Doing == "Rising" || form.Doing == "Zenith" || form.Doing == "Spent", 30f, () => form.Blazing, () => form.Doing == "Spent");
        follow = null;
        timesPart = "form";
        yield return Move(SunBearerAttack.Flares, () => TowardSouth(9f), "Flares_fury");
        yield return Move(SunBearerAttack.Furrows, () => RoundFrom(75f, 12.5f), "Furrows_fury");
        yield return Until(() => form.HotLanes == 0, 12f);

        yield return Shape(false);
        yield return Idle();
        yield return Until(() => !form.Poise.Holds, 14f);
        Vector3 stand = TowardSouth(5f);
        Go(stand, form.BodyAt - stand);
        form.ForceBreak();
        float broke = Time.time;
        bool struck = false;
        yield return Film("stagger_finisher", () => Time.time - broke < 3.2f, 4f, () => Time.time - broke > 1f, () =>
        {
            if (!struck && Time.time - broke > 1.2f)
            {
                struck = true;
                form.HeadPart.TakeHit(new Hit(30f, 0f, Vector3.forward, true));
            }
            return struck;
        });
        Say($"   the finisher: staggered {form.TimesStaggered} time(s), doing {form.Doing}");
        yield return Until(() => !form.Exposed && !form.IsStaggered, 14f);

        yield return Idle();
        stand = TowardSouth(7f);
        Go(stand, form.BodyAt - stand);
        timesPart = "death";
        for (int attempt = 0; attempt < 12 && !form.IsDead; attempt++)
        {
            form.ForceBreak();
            yield return Wait(0.6f);
            for (int blow = 0; blow < 40 && !form.IsDead; blow++) form.HeadPart.TakeHit(new Hit(60f, 0f, Vector3.forward));
            yield return Wait(0.3f);
        }
        float died = Time.time;
        yield return Film("death", () => Time.time - died < 7.5f, 8f, () => form.DoingFor >= form.SunOutAt, () => form.DoingFor >= form.CrackAt, 0.385f);
        timesPart = "";
        Say($"   death: felled {court.Felled} man laid {court.ManLaid}");
    }

    // ------------------------------------------------------------------ the report

    private static string Stats(List<float> values)
    {
        if (values.Count == 0) return "none";
        var sorted = values.OrderBy(v => v).ToList();
        float median = sorted[sorted.Count / 2], p95 = sorted[Mathf.Min(sorted.Count - 1, (int)(sorted.Count * 0.95f))], worst = sorted[sorted.Count - 1];
        int over = values.Count(v => v > 16.6f);
        return $"median {median:0.0} p95 {p95:0.0} worst {worst:0.0} over 16.6 ms {over} of {values.Count}";
    }

    private void Report()
    {
        var lines = new List<string>();
        lines.Add($"Khepharu audit, camera {cam}, {DateTime.Now:yyyy-MM-dd HH:mm}, {shots} pictures, guide closed {guideClosed} time(s), boxes of words read out by the watchdog {boxesClosed}");
        lines.Add("");
        lines.Add("== The sun under the paving (depth below 0.036 m; positive is under). Per case | doing | sun state | part: worst, frames over 0.05 m of frames measured");
        foreach (var pair in floorWorst.Where(p => p.Value.Depth > 0f).OrderByDescending(p => p.Value.Depth))
            lines.Add($"{pair.Value.Depth,6:0.00} m  {pair.Key}  ({pair.Value.Over}/{pair.Value.Frames} frames over 0.05; at {pair.Value.At:0.00} s into it; frame {Path.GetFileName(pair.Value.Frame)})");
        lines.Add("");
        lines.Add("== The sun's ball in the body (triangles; positive is in). Per case | doing | sun state: worst, piece, frames over 0.05 m");
        foreach (var pair in bodyWorst.Where(p => p.Value.Depth > 0f).OrderByDescending(p => p.Value.Depth))
        {
            boxWorst.TryGetValue(pair.Key, out var box);
            lines.Add($"{pair.Value.Depth,6:0.00} m  {pair.Key}  by {pair.Value.Who} ({pair.Value.Over}/{pair.Value.Frames}; at {pair.Value.At:0.00} s; frame {Path.GetFileName(pair.Value.Frame)}); box measure {(box != null ? box.Depth.ToString("0.00") + " by " + box.Who : "none")}");
        }
        lines.Add("");
        lines.Add("== The picture. Per move or case: frames, boss off screen, falcon frames, falcon off screen, obelisk over the player, obelisk over the boss, player off screen");
        int frames = 0, off = 0, falcon = 0, falconOff = 0;
        foreach (var pair in tallies)
        {
            var t = pair.Value;
            frames += t.Frames; off += t.BossOff; falcon += t.Falcon; falconOff += t.FalconOff;
            lines.Add($"{pair.Key}: {t.Frames} frames, boss off {t.BossOff}, falcon {t.Falcon} off {t.FalconOff}, obelisk over player {t.CoverPlayer} over boss {t.CoverBoss}, player off {t.PlayerOff}");
        }
        lines.Add($"ALL: {frames} frames, boss off screen {off} ({100f * off / Mathf.Max(1, frames):0.0}%), falcon off screen {falconOff} of {falcon} ({100f * falconOff / Mathf.Max(1, falcon):0.0}%)");
        lines.Add("");
        lines.Add("== Frame time (editor, Game view at the screen above; frames stalled by a capture left out)");
        foreach (var pair in times)
            lines.Add($"{pair.Key}: frame ms {Stats(pair.Value.Frame)}; main thread ms {Stats(pair.Value.Main)}; garbage {pair.Value.Garbage / Mathf.Max(1, pair.Value.Frame.Count)} bytes a frame");
        lines.Add("");
        lines.Add("== The probe's lines");
        lines.AddRange(summary);
        File.WriteAllLines(root + "/summary.txt", lines);
        Say($"report: {root}/summary.txt; floor worst any drawn {floorWorst.Where(p => p.Key.EndsWith("ANY DRAWN")).Select(p => p.Value.Depth).DefaultIfEmpty(0f).Max():0.00} m; body worst {bodyWorst.Values.Select(w => w.Depth).DefaultIfEmpty(0f).Max():0.00} m; boss off screen {off} of {frames}");
    }
}

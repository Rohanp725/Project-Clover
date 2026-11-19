using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// What spell forms are drawn with: solid shapes of the element (blades, spikes, a dragon's head) and flat marks of
    /// light on the ground and in the air (signs, rings, cracks, beams). A fixed number of each is made when the scene
    /// starts and used again and again; nothing is created during play. A form either throws one that looks after
    /// itself (it erupts, falls, flies or fades and is gone) or holds one and moves it every frame until it lets go.
    /// The scene has one of these, made by the builder; with none the calls do nothing.
    /// </summary>
    public class FormFx : MonoBehaviour
    {
        [SerializeField] private Material solid;
        [SerializeField] private Material[] marks = new Material[0];

        public int SolidCount = 120;
        public int MarkCount = 140;

        /// <summary>How many solids and marks are in use now, so probes can see them without the screen.</summary>
        public static int SolidsShown => instance != null ? instance.solidsShown : 0;
        public static int MarksShown => instance != null ? instance.marksShown : 0;

        private const byte Free = 0, Held = 1, Auto = 2;
        private const int Erupting = 0, Dropping = 1, Flying = 2, Shrinking = 3, Standing = 4;

        private struct Solid
        {
            public byte State;
            public int Motion;
            public Vector3 From, To, Scale;
            public Quaternion Rotation;
            public float Age, A, B, C;
        }

        private struct Mark
        {
            public byte State;
            public Vector3 Scale, EndScale;
            public float Age, Seconds, Spin;
            public Color Colour;
            public bool Facing;
            public float Roll;
            // The element art's marks: a drawn sheet played through, a mark drawn on by degrees, one that never fades.
            public byte Extra;
            public float Rate;
        }

        private const byte Sheet = 1, Loops = 2, Reveals = 4, Holds = 8;

        private static readonly int ColourId = Shader.PropertyToID("_Color");
        private static readonly int HotId = Shader.PropertyToID("_Hot");
        private static readonly int InkId = Shader.PropertyToID("_OutlineWidth");
        private static readonly int SheenId = Shader.PropertyToID("_Sheen");
        private static readonly int ThinId = Shader.PropertyToID("_Thin");
        private static readonly int FrameId = Shader.PropertyToID("_Frame");
        private static readonly int RevealId = Shader.PropertyToID("_Reveal");

        private static FormFx instance;

        private Mesh[] meshes;
        private Mesh square, strip;
        private Solid[] solids;
        private Transform[] solidAt;
        private MeshFilter[] solidMesh;
        private MeshRenderer[] solidShown;
        private Color[] solidColour;
        private float[] solidInk;
        private FormMesh[] solidShape;
        // A piece of the element art in place of a built shape: its own mesh and material, its look left to the material.
        private bool[] solidPiece;
        private Mesh pendingMesh;
        private Material pendingMaterial;
        private Mark[] flats;
        private Transform[] flatAt;
        private MeshFilter[] flatMesh;
        private MeshRenderer[] flatShown;
        private MaterialPropertyBlock block;
        private Transform viewer;
        private int nextSolid, nextFlat;
        private int solidsShown, marksShown;

        // Domain reload is off: the scene's effects from the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        /// <summary>The builder hands over the material solids are drawn in and one material for each <see cref="FormMark"/>, in order.</summary>
        public void Configure(Material solidMaterial, Material[] markMaterials)
        {
            solid = solidMaterial;
            marks = markMaterials;
        }

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            int kinds = System.Enum.GetValues(typeof(FormMesh)).Length;
            meshes = new Mesh[kinds];
            for (int i = 0; i < kinds; i++) meshes[i] = FormMeshes.Build((FormMesh)i);
            square = FormMeshes.Square();
            strip = FormMeshes.Strip();

            solids = new Solid[SolidCount];
            solidAt = new Transform[SolidCount];
            solidMesh = new MeshFilter[SolidCount];
            solidShown = new MeshRenderer[SolidCount];
            solidColour = new Color[SolidCount];
            solidInk = new float[SolidCount];
            solidShape = new FormMesh[SolidCount];
            solidPiece = new bool[SolidCount];
            for (int i = 0; i < SolidCount; i++)
                Make("Form Solid", solid, out solidAt[i], out solidMesh[i], out solidShown[i]);

            flats = new Mark[MarkCount];
            flatAt = new Transform[MarkCount];
            flatMesh = new MeshFilter[MarkCount];
            flatShown = new MeshRenderer[MarkCount];
            for (int i = 0; i < MarkCount; i++)
                Make("Form Mark", marks.Length > 0 ? marks[0] : null, out flatAt[i], out flatMesh[i], out flatShown[i]);
        }

        private void Make(string partName, Material material, out Transform at, out MeshFilter filter, out MeshRenderer shown)
        {
            var part = new GameObject(partName);
            at = part.transform;
            at.SetParent(transform, false);
            filter = part.AddComponent<MeshFilter>();
            shown = part.AddComponent<MeshRenderer>();
            shown.sharedMaterial = material;
            shown.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            shown.receiveShadows = false;
            shown.enabled = false;
        }

        private void Start()
        {
            var main = Camera.main;
            viewer = main != null ? main.transform : null;
        }

        private void OnEnable() => instance = this;

        private void OnDisable()
        {
            if (instance == this) instance = null;
            if (solids == null) return;
            for (int i = 0; i < solids.Length; i++)
            {
                solids[i].State = Free;
                if (solidShown[i] != null) solidShown[i].enabled = false;
            }
            for (int i = 0; i < flats.Length; i++)
            {
                flats[i].State = Free;
                if (flatShown[i] != null) flatShown[i].enabled = false;
            }
            solidsShown = marksShown = 0;
        }

        private void OnDestroy()
        {
            if (meshes != null)
                foreach (var mesh in meshes)
                    if (mesh != null) Destroy(mesh);
            if (square != null) Destroy(square);
            if (strip != null) Destroy(strip);
        }

        // ---- Solids.

        private int TakeSolid(FormMesh mesh, Color colour, float ink)
        {
            int found = -1;
            for (int tried = 0; tried < solids.Length && found < 0; tried++)
            {
                int i = (nextSolid + tried) % solids.Length;
                if (solids[i].State == Free) found = i;
            }
            if (found < 0)
            {
                // Every one in use: the next that looks after itself gives way. One a form is holding is never taken.
                for (int tried = 0; tried < solids.Length && found < 0; tried++)
                {
                    int i = (nextSolid + tried) % solids.Length;
                    if (solids[i].State == Auto) found = i;
                }
                if (found < 0) return -1;
                solidsShown--;
            }
            nextSolid = (found + 1) % solids.Length;
            if (pendingMesh != null)
            {
                solidPiece[found] = true;
                solidShape[found] = FormMesh.Slab;
                solidMesh[found].sharedMesh = pendingMesh;
                solidShown[found].sharedMaterial = pendingMaterial != null ? pendingMaterial : solid;
                pendingMesh = null;
                pendingMaterial = null;
            }
            else
            {
                // A plain piece in the held element's colour is made of that element's stuff.
                mesh = ElementSubstance.Shape(mesh, ElementSubstance.Of(colour));
                solidPiece[found] = false;
                solidShape[found] = mesh;
                solidMesh[found].sharedMesh = meshes[(int)mesh];
                solidShown[found].sharedMaterial = solid;
            }
            solidColour[found] = colour;
            solidInk[found] = ink;
            solids[found] = new Solid { State = Held, Rotation = Quaternion.identity, Scale = Vector3.one };
            Paint(found, 0f);
            solidShown[found].enabled = true;
            solidsShown++;
            return found;
        }

        private void Paint(int i, float hot)
        {
            block.Clear();
            block.SetColor(ColourId, solidColour[i]);
            block.SetFloat(HotId, hot);
            block.SetFloat(InkId, solidInk[i]);
            // A piece of the element art keeps its material's sheen and thinness: its element decided them.
            if (!solidPiece[i])
            {
                block.SetFloat(SheenId, ElementSubstance.Sheen(solidShape[i], solidColour[i]));
                block.SetFloat(ThinId, ElementSubstance.Thin(solidShape[i]));
            }
            solidShown[i].SetPropertyBlock(block);
        }

        // The white frame a thing lands with. The element art keeps it only for the cold hard stuff (a bolt, ice, steel,
        // a ray): flame, stone, leaf and smoke never flash white, and the art's own pieces carry their bands throughout.
        private bool Flashes(int i)
        {
            if (solidPiece[i]) return false;
            if (!ElementArt.Ready) return true;
            var shape = solidShape[i];
            return shape == FormMesh.Bolt || shape == FormMesh.Crystal || shape == FormMesh.Shard || shape == FormMesh.Blade || shape == FormMesh.Sword || shape == FormMesh.Ray;
        }

        private void FreeSolid(int i)
        {
            if (solids[i].State == Free) return;
            solids[i].State = Free;
            solidShown[i].enabled = false;
            solidsShown--;
        }

        /// <summary>Takes a solid to be moved by the caller every frame. Returns its handle, or -1 when none can be had; every other call accepts -1 and does nothing.</summary>
        public static int Hold(FormMesh mesh, Color colour, float ink = 0.035f) => instance != null ? instance.TakeSolid(mesh, colour, ink) : -1;

        // ---- Pieces of the element art: a mesh and a material of their own, held, erupting, falling, flying or standing.

        private static bool Pending(Mesh mesh, Material material)
        {
            if (instance == null || mesh == null) return false;
            instance.pendingMesh = mesh;
            instance.pendingMaterial = material;
            return true;
        }

        public static int HoldMesh(Mesh mesh, Material material, Color colour, float ink = 0.03f)
        {
            if (!Pending(mesh, material)) return -1;
            int handle = instance.TakeSolid(FormMesh.Slab, colour, ink);
            instance.pendingMesh = null;
            return handle;
        }

        public static void EruptMesh(Mesh mesh, Material material, Vector3 at, Quaternion rotation, Vector3 scale, Color colour, float rise, float hold, float sink, float ink = 0.03f)
        {
            if (Pending(mesh, material)) Thrown(FormMesh.Slab, colour, ink, Erupting, at, at, rotation, scale, rise, hold, sink);
            if (instance != null) instance.pendingMesh = null;
        }

        public static void DropMesh(Mesh mesh, Material material, Vector3 from, Vector3 to, Quaternion rotation, Vector3 scale, Color colour, float seconds, float stay, float sink, float ink = 0.03f)
        {
            if (Pending(mesh, material)) Thrown(FormMesh.Slab, colour, ink, Dropping, from, to, rotation, scale, seconds, stay, sink);
            if (instance != null) instance.pendingMesh = null;
        }

        public static void FlyMesh(Mesh mesh, Material material, Vector3 from, Vector3 to, Quaternion rotation, Vector3 scale, Color colour, float seconds, float ink = 0.03f)
        {
            if (Pending(mesh, material)) Thrown(FormMesh.Slab, colour, ink, Flying, from, to, rotation, scale, seconds, 0f, 0.01f);
            if (instance != null) instance.pendingMesh = null;
        }

        /// <summary>A piece standing where it is put, whole at once and gone at its end: a held frame of a drawn moment.</summary>
        public static void StandMesh(Mesh mesh, Material material, Vector3 at, Quaternion rotation, Vector3 scale, Color colour, float seconds, float ink = 0.03f)
        {
            if (Pending(mesh, material)) Thrown(FormMesh.Slab, colour, ink, Standing, at, at, rotation, scale, seconds, 0f, 0.01f);
            if (instance != null) instance.pendingMesh = null;
        }

        public static void Place(int handle, Vector3 position, Quaternion rotation, Vector3 scale)
        {
            if (instance == null || handle < 0 || instance.solids[handle].State != Held) return;
            if (ElementSubstance.Moves(instance.solidShape[handle])) ElementSubstance.Sway(instance.solidShape[handle], handle, Time.time, ref scale, ref rotation);
            instance.solidAt[handle].SetPositionAndRotation(position, rotation);
            instance.solidAt[handle].localScale = scale;
        }

        /// <summary>How near white a held solid is drawn: 1 is a blank white frame of it.</summary>
        public static void Heat(int handle, float hot)
        {
            if (instance == null || handle < 0 || instance.solids[handle].State != Held) return;
            instance.Paint(handle, Mathf.Clamp01(hot));
        }

        /// <summary>Lets a held solid go: at once, or shrinking to nothing where it is over the seconds given.</summary>
        public static void Release(int handle, float shrinkSeconds = 0f)
        {
            if (instance == null || handle < 0 || instance.solids[handle].State != Held) return;
            if (shrinkSeconds <= 0f)
            {
                instance.FreeSolid(handle);
                return;
            }
            ref var one = ref instance.solids[handle];
            one.State = Auto;
            one.Motion = Shrinking;
            one.Age = 0f;
            one.A = shrinkSeconds;
            one.Scale = instance.solidAt[handle].localScale;
        }

        private static int Thrown(FormMesh mesh, Color colour, float ink, int motion, Vector3 from, Vector3 to, Quaternion rotation, Vector3 scale, float a, float b, float c)
        {
            if (instance == null) return -1;
            int i = instance.TakeSolid(mesh, colour, ink);
            if (i < 0) return -1;
            instance.solids[i] = new Solid { State = Auto, Motion = motion, From = from, To = to, Rotation = rotation, Scale = scale, A = Mathf.Max(0.01f, a), B = b, C = Mathf.Max(0.01f, c) };
            instance.Advance(i, 0f);
            return i;
        }

        /// <summary>Something bursts out of the ground at a point, stands for a while and sinks back.</summary>
        public static void Erupt(FormMesh mesh, Vector3 at, Quaternion rotation, Vector3 scale, Color colour, float rise, float hold, float sink, float ink = 0.035f) =>
            Thrown(mesh, colour, ink, Erupting, at, at, rotation, scale, rise, hold, sink);

        /// <summary>Something falls from one point to another, stays stuck there for a while and shrinks away.</summary>
        public static void Drop(FormMesh mesh, Vector3 from, Vector3 to, Quaternion rotation, Vector3 scale, Color colour, float seconds, float stay, float sink, float ink = 0.035f) =>
            Thrown(mesh, colour, ink, Dropping, from, to, rotation, scale, seconds, stay, sink);

        /// <summary>Something flies from one point to another, thinning as it goes, and is gone.</summary>
        public static void Fly(FormMesh mesh, Vector3 from, Vector3 to, Quaternion rotation, Vector3 scale, Color colour, float seconds, float ink = 0.035f) =>
            Thrown(mesh, colour, ink, Flying, from, to, rotation, scale, seconds, 0f, 0.01f);

        private void Advance(int i, float deltaTime)
        {
            ref var one = ref solids[i];
            one.Age += deltaTime;
            float age = one.Age;
            var at = solidAt[i];
            switch (one.Motion)
            {
                case Erupting:
                {
                    if (age >= one.A + one.B + one.C) { FreeSolid(i); return; }
                    float up;
                    if (age < one.A)
                    {
                        // Out fast and a little too far, then back: it lands as a shape, not as a growth.
                        float t = age / one.A;
                        up = Mathf.Max(0.02f, 1f - (1f - t) * (1f - t) * (1f - t) + 0.25f * Mathf.Sin(t * Mathf.PI));
                    }
                    else if (age < one.A + one.B) up = 1f;
                    else
                    {
                        float t = (age - one.A - one.B) / one.C;
                        up = Mathf.Max(0.02f, 1f - t * t);
                    }
                    at.SetPositionAndRotation(one.From, one.Rotation);
                    float wide = age < one.A + one.B ? 1f : Mathf.Lerp(1f, 0.5f, (age - one.A - one.B) / one.C);
                    at.localScale = new Vector3(one.Scale.x * wide, one.Scale.y * up, one.Scale.z * wide);
                    Paint(i, age < one.A + 0.05f && Flashes(i) ? 1f : 0f);
                    break;
                }
                case Dropping:
                {
                    if (age >= one.A + one.B + one.C) { FreeSolid(i); return; }
                    if (age < one.A)
                    {
                        float t = age / one.A;
                        at.SetPositionAndRotation(Vector3.Lerp(one.From, one.To, t * t), one.Rotation);
                        // Stretched along its fall: speed drawn as shape.
                        at.localScale = new Vector3(one.Scale.x * 0.8f, one.Scale.y * 0.8f, one.Scale.z * 1.35f);
                        Paint(i, 0f);
                    }
                    else
                    {
                        float since = age - one.A;
                        at.SetPositionAndRotation(one.To, one.Rotation);
                        float left = since < one.B ? 1f : Mathf.Max(0.02f, 1f - (since - one.B) / one.C);
                        at.localScale = one.Scale * left;
                        Paint(i, since < 0.06f && Flashes(i) ? 1f : 0f);
                    }
                    break;
                }
                case Standing:
                {
                    if (age >= one.A) { FreeSolid(i); return; }
                    at.SetPositionAndRotation(one.From, one.Rotation);
                    at.localScale = one.Scale;
                    break;
                }
                case Flying:
                {
                    if (age >= one.A) { FreeSolid(i); return; }
                    float t = age / one.A;
                    at.SetPositionAndRotation(Vector3.Lerp(one.From, one.To, 1f - (1f - t) * (1f - t)), one.Rotation);
                    at.localScale = one.Scale * Mathf.Max(0.02f, 1f - t * t);
                    break;
                }
                default:
                {
                    if (age >= one.A) { FreeSolid(i); return; }
                    at.localScale = one.Scale * Mathf.Max(0.02f, 1f - age / one.A);
                    break;
                }
            }
            // Flame, lightning, smoke and water never hold still, whatever their form is doing with them.
            if (ElementSubstance.Moves(solidShape[i]) && one.Motion != Shrinking)
            {
                Vector3 scale = at.localScale;
                Quaternion rotation = one.Rotation;
                ElementSubstance.Sway(solidShape[i], i, Time.time, ref scale, ref rotation);
                at.localScale = scale;
                at.rotation = rotation;
            }
        }

        // ---- Marks.

        private int TakeMark(FormMark kind, Color colour, bool onStrip, Material own = null)
        {
            if (own == null && (int)kind >= marks.Length) return -1;
            int found = -1;
            for (int tried = 0; tried < flats.Length && found < 0; tried++)
            {
                int i = (nextFlat + tried) % flats.Length;
                if (flats[i].State == Free) found = i;
            }
            if (found < 0)
            {
                for (int tried = 0; tried < flats.Length && found < 0; tried++)
                {
                    int i = (nextFlat + tried) % flats.Length;
                    if (flats[i].State == Auto) found = i;
                }
                if (found < 0) return -1;
                marksShown--;
            }
            nextFlat = (found + 1) % flats.Length;
            flatMesh[found].sharedMesh = onStrip ? strip : square;
            flatShown[found].sharedMaterial = own != null ? own : marks[(int)kind];
            flats[found] = new Mark { State = Held, Colour = colour };
            block.Clear();
            block.SetColor(ColourId, colour);
            flatShown[found].SetPropertyBlock(block);
            flatShown[found].enabled = true;
            marksShown++;
            return found;
        }

        private static bool OnStrip(FormMark kind) => kind == FormMark.Beam || kind == FormMark.Lane || kind == FormMark.Streak;

        // Marks lie a little above the ground, each at its own height so two never fight over the same pixels.
        private float Lift(int i) => 0.05f + i * 0.0005f;

        private void FreeMark(int i)
        {
            if (flats[i].State == Free) return;
            flats[i].State = Free;
            flatShown[i].enabled = false;
            marksShown--;
        }

        private static int Thrown(FormMark kind, Vector3 at, Quaternion rotation, Vector3 scale, Vector3 endScale, Color colour, float seconds, float spin, bool facing, float roll, bool lift, Material own = null)
        {
            if (instance == null || seconds <= 0f) return -1;
            int i = instance.TakeMark(kind, colour, own == null && OnStrip(kind), own);
            if (i < 0) return -1;
            ref var one = ref instance.flats[i];
            one.State = Auto;
            one.Scale = scale;
            one.EndScale = endScale;
            one.Seconds = seconds;
            one.Spin = spin;
            one.Facing = facing;
            one.Roll = roll;
            if (lift) at.y += instance.Lift(i);
            instance.flatAt[i].SetPositionAndRotation(at, rotation);
            instance.flatAt[i].localScale = scale;
            return i;
        }

        /// <summary>
        /// Lays a mark flat on the ground at a point, this many metres across, for some seconds: there at once, fading
        /// only at its end. It may change size as it lasts (a ring widening, or closing in) and turn.
        /// </summary>
        public static void Lay(FormMark kind, Vector3 at, float size, Color colour, float seconds, float endSize = -1f, float spinDegrees = 0f, float yaw = 0f)
        {
            float end = endSize < 0f ? size : endSize;
            Thrown(kind, at, Quaternion.Euler(0f, yaw, 0f), new Vector3(size, 1f, size), new Vector3(end, 1f, end), colour, seconds, spinDegrees, false, 0f, true);
        }

        /// <summary>Lays a strip along the ground from a point: a lane, a beam's scar, a line of light.</summary>
        public static void Run(FormMark kind, Vector3 from, Vector3 direction, float length, float width, Color colour, float seconds, float endWidth = -1f)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;
            float end = endWidth < 0f ? width : endWidth;
            Thrown(kind, from, Quaternion.LookRotation(direction, Vector3.up), new Vector3(width, 1f, length), new Vector3(end, 1f, length), colour, seconds, 0f, false, 0f, true);
        }

        /// <summary>Hangs a mark in the air, turned flat to the camera: an impact's star, a cut across a body.</summary>
        public static void Face(FormMark kind, Vector3 at, float size, Color colour, float seconds, float endSize = -1f, float roll = 0f)
        {
            float end = endSize < 0f ? size : endSize;
            int i = Thrown(kind, at, Quaternion.identity, new Vector3(size, 1f, size), new Vector3(end, 1f, end), colour, seconds, 0f, true, roll, false);
            if (i >= 0) instance.Turn(i);
        }

        /// <summary>A cut of light through a point, turned to the camera and lying along a direction as the camera sees it.</summary>
        public static void Cut(Vector3 through, Vector3 along, float length, float width, Color colour, float seconds, float endWidth = 0f)
        {
            if (instance == null || instance.viewer == null || seconds <= 0f) return;
            Vector3 toward = instance.viewer.forward;
            Vector3 across = Vector3.ProjectOnPlane(along, toward);
            if (across.sqrMagnitude < 0.0001f) across = instance.viewer.right;
            across.Normalize();
            // The strip's own length runs along its Z and its face looks along its Y.
            var rotation = Quaternion.LookRotation(across, -toward);
            Thrown(FormMark.Streak, through - across * (length * 0.5f), rotation, new Vector3(width, 1f, length), new Vector3(endWidth, 1f, length * 1.15f), colour, seconds, 0f, false, 0f, false);
        }

        // ---- Marks of the element art: masked decals on the ground and drawn sheets turned to the camera.

        /// <summary>
        /// Lays a mark of the element art's own material on the ground: <paramref name="scale"/> is its size along X and
        /// Z, growing to <paramref name="endScale"/>. It may be drawn on over <paramref name="revealSeconds"/> (the
        /// material says how) and may hold to its end instead of fading.
        /// </summary>
        public static void LayDecal(Material material, Vector3 at, Quaternion rotation, Vector3 scale, Vector3 endScale, Color colour, float seconds, float revealSeconds = 0f, bool fades = true, float spinDegrees = 0f)
        {
            if (material == null) return;
            int i = Thrown(FormMark.Disc, at, rotation, scale, endScale, colour, seconds, spinDegrees, false, 0f, true, material);
            if (i < 0) return;
            ref var one = ref instance.flats[i];
            one.Extra = (byte)((revealSeconds > 0f ? Reveals : 0) | (fades ? 0 : Holds));
            one.Rate = revealSeconds;
            instance.Paint(i, one.Colour, 0f, revealSeconds > 0f ? 0f : 1f);
        }

        /// <summary>
        /// Hangs a drawn sheet (4x4 frames) at a point, turned to the camera, played through once over the seconds
        /// given, or round and round at <paramref name="framesPerSecond"/> when it loops.
        /// </summary>
        public static void FaceSheet(Material material, Vector3 at, float size, Color colour, float seconds, bool loops = false, float framesPerSecond = 16f, float roll = 0f)
        {
            if (material == null) return;
            int i = Thrown(FormMark.Disc, at, Quaternion.identity, new Vector3(size, 1f, size), new Vector3(size, 1f, size), colour, seconds, 0f, true, roll, false, material);
            if (i < 0) return;
            ref var one = ref instance.flats[i];
            one.Extra = (byte)(Sheet | Holds | (loops ? Loops : 0));
            one.Rate = framesPerSecond;
            instance.Turn(i);
            instance.Paint(i, one.Colour, 0f, 1f);
        }

        /// <summary>Takes a mark of the element art's own material, to be placed by the caller every frame until it is let go.</summary>
        public static int HoldDecal(Material material, Color colour) => instance != null && material != null ? instance.TakeMark(FormMark.Disc, colour, false, material) : -1;

        // The property block of a mark of the element art: its tint, which frame of its sheet, how much of it is drawn.
        private void Paint(int i, Color colour, float frame, float reveal)
        {
            block.Clear();
            block.SetColor(ColourId, colour);
            if ((flats[i].Extra & Sheet) != 0) block.SetFloat(FrameId, frame);
            if ((flats[i].Extra & Reveals) != 0) block.SetFloat(RevealId, reveal);
            flatShown[i].SetPropertyBlock(block);
        }

        /// <summary>Takes a mark to be placed by the caller every frame until it is let go.</summary>
        public static int HoldMark(FormMark kind, Color colour) => instance != null ? instance.TakeMark(kind, colour, OnStrip(kind)) : -1;

        /// <summary>Places a held mark. The scale's X and Z are its size on its own plane.</summary>
        public static void PlaceMark(int handle, Vector3 position, Quaternion rotation, Vector3 scale, bool onGround = true)
        {
            if (instance == null || handle < 0 || instance.flats[handle].State != Held) return;
            if (onGround) position.y += instance.Lift(handle);
            instance.flatAt[handle].SetPositionAndRotation(position, rotation);
            instance.flatAt[handle].localScale = scale;
        }

        /// <summary>A held mark turned flat to the camera at a point.</summary>
        public static void PlaceMarkFacing(int handle, Vector3 position, float size, float roll = 0f)
        {
            if (instance == null || handle < 0 || instance.flats[handle].State != Held || instance.viewer == null) return;
            instance.flatAt[handle].SetPositionAndRotation(position, Quaternion.LookRotation(instance.viewer.up, -instance.viewer.forward) * Quaternion.Euler(0f, roll, 0f));
            instance.flatAt[handle].localScale = new Vector3(size, 1f, size);
        }

        public static void TintMark(int handle, Color colour)
        {
            if (instance == null || handle < 0 || instance.flats[handle].State != Held) return;
            instance.flats[handle].Colour = colour;
            instance.block.Clear();
            instance.block.SetColor(ColourId, colour);
            instance.flatShown[handle].SetPropertyBlock(instance.block);
        }

        /// <summary>Lets a held mark go: at once, or fading where it lies over the seconds given.</summary>
        public static void ReleaseMark(int handle, float fadeSeconds = 0f)
        {
            if (instance == null || handle < 0 || instance.flats[handle].State != Held) return;
            if (fadeSeconds <= 0f)
            {
                instance.FreeMark(handle);
                return;
            }
            ref var one = ref instance.flats[handle];
            one.State = Auto;
            // Its fade is the last two fifths of a mark's time: it starts there.
            one.Age = fadeSeconds * 1.5f;
            one.Seconds = fadeSeconds * 2.5f;
            one.Scale = one.EndScale = instance.flatAt[handle].localScale;
            one.Spin = 0f;
            one.Facing = false;
        }

        private void Turn(int i)
        {
            if (viewer == null) return;
            flatAt[i].rotation = Quaternion.LookRotation(viewer.up, -viewer.forward) * Quaternion.Euler(0f, flats[i].Roll, 0f);
        }

        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f) return;
            for (int i = 0; i < solids.Length; i++)
                if (solids[i].State == Auto) Advance(i, deltaTime);
            for (int i = 0; i < flats.Length; i++)
            {
                if (flats[i].State != Auto) continue;
                ref var one = ref flats[i];
                one.Age += deltaTime;
                if (one.Age >= one.Seconds)
                {
                    FreeMark(i);
                    continue;
                }
                float t = one.Age / one.Seconds;
                // Its size is reached early and held: it snaps to shape. It is whole for most of its time and gone quickly.
                float grown = 1f - (1f - t) * (1f - t);
                flatAt[i].localScale = Vector3.Lerp(one.Scale, one.EndScale, grown);
                if (one.Spin != 0f) flatAt[i].Rotate(0f, one.Spin * deltaTime, 0f, Space.Self);
                var colour = one.Colour;
                if ((one.Extra & Holds) == 0) colour.a *= t < 0.6f ? 1f : 1f - (t - 0.6f) / 0.4f;
                if (one.Extra != 0)
                {
                    // A drawn sheet steps through its sixteen frames; a mark drawn on by degrees is revealed.
                    float frame = (one.Extra & Loops) != 0 ? Mathf.Floor(one.Age * one.Rate) % 16f : Mathf.Min(15f, Mathf.Floor(t * 16f));
                    float reveal = (one.Extra & Reveals) != 0 ? Mathf.Clamp01(one.Age / Mathf.Max(0.01f, one.Rate)) : 1f;
                    Paint(i, colour, frame, reveal);
                    continue;
                }
                block.Clear();
                block.SetColor(ColourId, colour);
                flatShown[i].SetPropertyBlock(block);
            }
        }
    }
}

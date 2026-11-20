using UnityEngine;
using UnityEngine.Rendering;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Plays the elements' second-pass looks (<see cref="ElementLookSet"/>) in the game: each piece posed every frame by
    /// its keys (path, scale, spin, orbit) about the body it is on, seen only in its window. An affliction or a surge is
    /// kept frame by frame (<see cref="Keep"/>): its look plays from its start, and while it is still kept past its end
    /// the pieces that repeat (drips, puffs, wisps, glints) play again round and round while the rest hold. Once it is
    /// no longer kept everything shrinks away in a quarter of a second. A reaction's look plays once (<see cref="Play"/>).
    /// A fixed pool made when the scene starts; nothing is made or allocated while they play.
    /// </summary>
    public class ElementLooks : MonoBehaviour
    {
        /// <summary>
        /// Pieces drawn at once over every look; looks past that go without their later pieces. Twelve afflicted enemies
        /// with two looks each and a surge drew 293 at most (the crowd profile of 2026-11-20).
        /// </summary>
        public const int PoolSize = 384;
        /// <summary>Looks playing at once.</summary>
        public const int MostShowings = 48;
        /// <summary>The most pieces one look has (Overdrive's arcs are seventy ribbons, each seen a tenth of a second).</summary>
        public const int MostParts = 96;
        /// <summary>Seconds a kept look waits unkept before it goes, and how long it takes to go.</summary>
        public const float Linger = 0.25f, Going = 0.25f;
        private const float FlatLift = 0.035f;

        private struct Showing
        {
            public bool Active;
            public int Look;
            public Transform Body;
            /// <summary>Where the body stood when the look began: the ground's pieces stay there.</summary>
            public Vector3 Ground;
            public float Age, KeptAt, Gone;
            public bool Once, Ending;
            /// <summary>Played where it began and not on a body (a strike's look): it never goes for want of a body.</summary>
            public bool Fixed;
            /// <summary>How the look is turned about its anchor, and its size (a strike's look turned to the strike's way and reach).</summary>
            public Quaternion Turn;
            public float Size;
        }

        private static readonly int HeadId = Shader.PropertyToID("_DrawHead");
        private static readonly int TailId = Shader.PropertyToID("_DrawTail");
        private static readonly int InkId = Shader.PropertyToID("_OutlineWidth");
        private static readonly int InkColourId = Shader.PropertyToID("_OutlineColor");
        private static readonly int ThinId = Shader.PropertyToID("_Thin");

        private static ElementLooks instance;

        /// <summary>Pieces drawn now and looks playing now, for probes.</summary>
        public static int Shown => instance != null ? instance.used : 0;
        public static int Playing => instance != null ? instance.playing : 0;
        public static bool Ready => instance != null && instance.set != null && ElementArt.Ready;

        private ElementLookSet set;
        private Showing[] showings;
        private int[] slots;
        private Transform[] at;
        private MeshFilter[] meshes;
        private MeshRenderer[] shown;
        private bool[] taken;
        // A pooled renderer carrying a ribbon's property block, which a piece taken after it must not keep.
        private bool[] blocked;
        private MaterialPropertyBlock block;
        private int used, playing;
        // Where the search for a free pooled renderer starts: just past the last one taken, so a burst of new pieces does
        // not walk the taken ones from the start each time.
        private int searchFrom;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        /// <summary>The element art hands over the looks and the pool is made (once).</summary>
        public void Configure(ElementLookSet looks)
        {
            if (set != null || looks == null) return;
            set = looks;
            showings = new Showing[MostShowings];
            slots = new int[MostShowings * MostParts];
            for (int i = 0; i < slots.Length; i++) slots[i] = -1;
            at = new Transform[PoolSize];
            meshes = new MeshFilter[PoolSize];
            shown = new MeshRenderer[PoolSize];
            taken = new bool[PoolSize];
            blocked = new bool[PoolSize];
            block = new MaterialPropertyBlock();
            for (int i = 0; i < PoolSize; i++)
            {
                var part = new GameObject("Element Look Part");
                part.transform.SetParent(transform, false);
                at[i] = part.transform;
                meshes[i] = part.AddComponent<MeshFilter>();
                shown[i] = part.AddComponent<MeshRenderer>();
                shown[i].shadowCastingMode = ShadowCastingMode.Off;
                shown[i].receiveShadows = false;
                shown[i].lightProbeUsage = LightProbeUsage.Off;
                shown[i].reflectionProbeUsage = ReflectionProbeUsage.Off;
                shown[i].enabled = false;
            }
        }

        private void OnEnable() => instance = this;

        private void OnDisable()
        {
            if (instance == this) instance = null;
            if (showings == null) return;
            for (int i = 0; i < showings.Length; i++) End(i);
        }

        /// <summary>A look by its slug ("water_soaked"); -1 when the scene has none.</summary>
        public static int Find(string key) => Ready ? instance.set.Find(key) : -1;

        /// <summary>Glaze's crystal and the ice it is drawn in, from the built looks; false when the scene has none.</summary>
        public static bool TryCrystal(out Mesh crystal, out Material ice)
        {
            crystal = Ready ? instance.set.Crystal : null;
            ice = Ready ? instance.set.CrystalMaterial : null;
            return crystal != null && ice != null;
        }

        /// <summary>A look's data, for the shells (Temper's steel) that the body draws itself.</summary>
        public static bool TryLook(int look, out ElementLookSet.Look data)
        {
            data = default;
            if (instance == null || instance.set == null || look < 0 || look >= instance.set.Looks.Length) return false;
            data = instance.set.Looks[look];
            return true;
        }

        /// <summary>Keeps a look playing on a body this frame (an affliction it carries, a surge it wears). False when it cannot be shown.</summary>
        public static bool Keep(int look, Transform body)
        {
            if (!Ready || body == null || look < 0 || look >= instance.set.Looks.Length) return false;
            return instance.KeepOn(look, body);
        }

        /// <summary>Plays a look once on a body (a reaction's).</summary>
        public static bool Play(int look, Transform body)
        {
            if (!Ready || body == null || look < 0 || look >= instance.set.Looks.Length) return false;
            int showing = instance.Begin(look, body);
            if (showing < 0) return false;
            instance.showings[showing].Once = true;
            return true;
        }

        /// <summary>
        /// Plays a look once where it stands, not on a body, turned to <paramref name="yaw"/> degrees (the recipe's player
        /// faced the main body; the look is turned so that way becomes this one) and drawn <paramref name="size"/> times
        /// as large (a strike's look fitted to its reach). False when it cannot be shown.
        /// </summary>
        public static bool PlayAt(int look, Vector3 at, float yaw, float size = 1f)
        {
            if (!Ready || look < 0 || look >= instance.set.Looks.Length) return false;
            int showing = instance.Begin(look, null);
            if (showing < 0) return false;
            ref var one = ref instance.showings[showing];
            one.Once = true;
            one.Fixed = true;
            one.Ground = at;
            one.Turn = Quaternion.Euler(0f, yaw - instance.set.Looks[look].FacingYaw, 0f);
            one.Size = Mathf.Max(0.1f, size);
            return true;
        }

        /// <summary>
        /// Seconds the look has played on this body and how far up it a shell covers (a share), when a look with a shell
        /// is on it now; false when none is. The body's own material draws the shell (<see cref="ElementLookSet.Shell"/>).
        /// </summary>
        public static bool ShellOn(Transform body, out byte shell, out float reveal, out float amount) =>
            ShellOn(body, ElementLookSet.NoShell, out shell, out reveal, out amount);

        /// <summary>The same, for one kind of shell only (<paramref name="wanted"/>; NoShell for any): a body may be wet and frozen at once.</summary>
        public static bool ShellOn(Transform body, byte wanted, out byte shell, out float reveal, out float amount)
        {
            shell = ElementLookSet.NoShell;
            reveal = amount = 0f;
            if (instance == null || instance.set == null || body == null) return false;
            for (int i = 0; i < instance.showings.Length; i++)
            {
                ref var one = ref instance.showings[i];
                if (!one.Active || !ReferenceEquals(one.Body, body)) continue;
                ref var look = ref instance.set.Looks[one.Look];
                if (look.Shell == ElementLookSet.NoShell) continue;
                if (wanted != ElementLookSet.NoShell && look.Shell != wanted && !(wanted == ElementLookSet.Ice && look.Shell == ElementLookSet.Rime)) continue;
                shell = look.Shell;
                reveal = ElementLookSet.ShellReveal(look, one.Age);
                amount = one.Ending ? Mathf.Clamp01(1f - one.Gone / Going) : 1f;
                return true;
            }
            return false;
        }

        private bool KeepOn(int look, Transform body)
        {
            for (int i = 0; i < showings.Length; i++)
            {
                ref var one = ref showings[i];
                if (!one.Active || one.Look != look || !ReferenceEquals(one.Body, body)) continue;
                one.KeptAt = Time.time;
                // Kept again while it was going: it comes back as it was.
                one.Ending = false;
                one.Gone = 0f;
                return true;
            }
            return Begin(look, body) >= 0;
        }

        private int Begin(int look, Transform body)
        {
            int free = -1, oldest = -1;
            for (int i = 0; i < showings.Length; i++)
            {
                if (!showings[i].Active) { free = i; break; }
                if (oldest < 0 || showings[i].Age > showings[oldest].Age) oldest = i;
            }
            if (free < 0)
            {
                End(oldest);
                free = oldest;
            }
            showings[free] = new Showing
            {
                Active = true, Look = look, Body = body, Ground = body != null ? body.position : Vector3.zero, Age = 0f, KeptAt = Time.time,
                Turn = Quaternion.identity, Size = 1f,
            };
            playing++;
            return free;
        }

        private void End(int showing)
        {
            if (!showings[showing].Active) return;
            for (int k = 0; k < MostParts; k++) Free(showing * MostParts + k);
            showings[showing].Active = false;
            showings[showing].Body = null;
            playing--;
        }

        private void Free(int slot)
        {
            int pooled = slots[slot];
            if (pooled < 0) return;
            slots[slot] = -1;
            taken[pooled] = false;
            shown[pooled].enabled = false;
            used--;
        }

        private int Take(int slot, in ElementLookSet.Part part)
        {
            if (slots[slot] >= 0) return slots[slot];
            for (int n = 0; n < PoolSize; n++)
            {
                int i = (searchFrom + n) % PoolSize;
                if (taken[i]) continue;
                searchFrom = (i + 1) % PoolSize;
                taken[i] = true;
                used++;
                slots[slot] = i;
                meshes[i].sharedMesh = part.Mesh;
                shown[i].sharedMaterial = part.Material;
                if (blocked[i] && part.Kind != ElementLookSet.Ribbon)
                {
                    shown[i].SetPropertyBlock(null);
                    blocked[i] = false;
                }
                return i;
            }
            return -1;
        }

        private void LateUpdate()
        {
            if (set == null) return;
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f) return;
            float now = Time.time;
            for (int i = 0; i < showings.Length; i++)
            {
                ref var one = ref showings[i];
                if (!one.Active) continue;
                ref var look = ref set.Looks[one.Look];
                bool bodyGone = !one.Fixed && (one.Body == null || !one.Body.gameObject.activeInHierarchy);
                one.Age += deltaTime;
                if (!one.Ending)
                {
                    bool over = one.Once ? one.Age >= look.Seconds : now - one.KeptAt > Linger;
                    if (over || bodyGone) one.Ending = true;
                }
                if (one.Ending)
                {
                    one.Gone += deltaTime;
                    if (one.Gone >= Going)
                    {
                        End(i);
                        continue;
                    }
                }
                Vector3 feet = one.Fixed || bodyGone ? one.Ground : one.Body.position;
                float shrink = one.Ending ? Mathf.Max(0.001f, 1f - one.Gone / Going) : 1f;
                for (int k = 0; k < look.PartCount && k < MostParts; k++)
                {
                    int slot = i * MostParts + k;
                    ref var part = ref set.Parts[look.FirstPart + k];
                    float t = ElementLookSet.PartTime(look, part, one.Age, one.Once);
                    bool visible = t >= part.Show && (part.Hide >= 9f || t < part.Hide);
                    if (!visible)
                    {
                        Free(slot);
                        continue;
                    }
                    int pooled = Take(slot, part);
                    if (pooled < 0) continue;
                    // Spins and orbits of a part that keeps moving run on with the look's own age.
                    float angleTime = part.RunsOn && !one.Once ? one.Age : t;
                    Pose(part, t, angleTime, part.Follows ? feet : one.Ground, one.Turn, one.Size, shrink, slot, at[pooled]);
                    if (part.Kind == ElementLookSet.Ribbon) Draw(part, t, pooled);
                    if (!shown[pooled].enabled) shown[pooled].enabled = true;
                }
            }
        }

        // A ribbon's drawn length: on from its tail to the head's share, wiped off from the tail to the tail's share.
        private void Draw(in ElementLookSet.Part part, float t, int pooled)
        {
            float head = part.DrawCount > 0 ? Mathf.Clamp01(set.Sample(part.DrawFirst, part.DrawCount, t, Vector3.one).x) : 1f;
            float tail = part.EraseCount > 0 ? Mathf.Clamp01(set.Sample(part.EraseFirst, part.EraseCount, t, Vector3.zero).x) : 0f;
            block.SetFloat(HeadId, head);
            block.SetFloat(TailId, tail);
            block.SetFloat(InkId, part.InkWidth);
            block.SetColor(InkColourId, part.InkColour);
            block.SetFloat(ThinId, part.Thin);
            shown[pooled].SetPropertyBlock(block);
            blocked[pooled] = true;
        }

        private void Pose(in ElementLookSet.Part part, float t, float angleTime, Vector3 anchor, Quaternion turn, float size, float shrink, int slot, Transform frame)
        {
            set.LocalPose(part, t, angleTime, slot, out Vector3 position, out Quaternion rotation, out Vector3 scale);
            if (part.Kind == ElementLookSet.Flat) position.y += FlatLift;
            frame.SetPositionAndRotation(anchor + turn * (position * size), turn * rotation);
            frame.localScale = scale * (shrink * size);
        }
    }
}

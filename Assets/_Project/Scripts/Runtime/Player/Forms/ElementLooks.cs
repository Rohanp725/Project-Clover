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
        /// <summary>Pieces drawn at once over every look; looks past that go without their later pieces.</summary>
        public const int PoolSize = 240;
        /// <summary>Looks playing at once.</summary>
        public const int MostShowings = 48;
        /// <summary>The most pieces one look has.</summary>
        public const int MostParts = 32;
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
        }

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
        private int used, playing;

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
        /// Seconds the look has played on this body and how far up it a shell covers (a share), when a look with a shell
        /// is on it now; false when none is. The body's own material draws the shell (<see cref="ElementLookSet.Shell"/>).
        /// </summary>
        public static bool ShellOn(Transform body, out byte shell, out float reveal, out float amount)
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
                Active = true, Look = look, Body = body, Ground = body.position, Age = 0f, KeptAt = Time.time,
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
            for (int i = 0; i < PoolSize; i++)
            {
                if (taken[i]) continue;
                taken[i] = true;
                used++;
                slots[slot] = i;
                meshes[i].sharedMesh = part.Mesh;
                shown[i].sharedMaterial = part.Material;
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
                bool bodyGone = one.Body == null || !one.Body.gameObject.activeInHierarchy;
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
                Vector3 feet = bodyGone ? one.Ground : one.Body.position;
                float shrink = one.Ending ? Mathf.Max(0.001f, 1f - one.Gone / Going) : 1f;
                for (int k = 0; k < look.PartCount && k < MostParts; k++)
                {
                    int slot = i * MostParts + k;
                    ref var part = ref set.Parts[look.FirstPart + k];
                    float t = PartTime(look, part, one.Age, one.Once);
                    bool visible = t >= part.Show && (part.Hide >= 9f || t < part.Hide);
                    if (!visible)
                    {
                        Free(slot);
                        continue;
                    }
                    int pooled = Take(slot, part);
                    if (pooled < 0) continue;
                    Pose(part, t, part.Follows ? feet : one.Ground, shrink, slot, at[pooled]);
                    if (!shown[pooled].enabled) shown[pooled].enabled = true;
                }
            }
        }

        // The part's own time: the look's, held at its end, or for a repeating part round and round its loop.
        private static float PartTime(in ElementLookSet.Look look, in ElementLookSet.Part part, float age, bool once)
        {
            if (age <= look.Seconds || once) return Mathf.Min(age, look.Seconds + 0.001f);
            if (!part.Repeats) return look.Seconds + 0.001f;
            float span = Mathf.Max(0.1f, look.Seconds - look.LoopFrom);
            return look.LoopFrom + Mathf.Repeat(age - look.Seconds, span);
        }

        private void Pose(in ElementLookSet.Part part, float t, Vector3 anchor, float shrink, int slot, Transform frame)
        {
            Vector3 position = part.Position + set.Sample(part.PathFirst, part.PathCount, t, Vector3.zero);
            Vector3 scaleKey = set.Sample(part.ScaleFirst, part.ScaleCount, t, Vector3.one);
            Quaternion rotation = part.Rotation;
            if (part.SpinCount > 0) rotation = Quaternion.AngleAxis(set.Sample(part.SpinFirst, part.SpinCount, t, Vector3.zero).x, part.SpinAxis) * rotation;
            if (part.Redraw)
            {
                // Sparks are turned anew three times a second at most, not every frame (the owner: nothing flickery).
                int redraw = Mathf.FloorToInt(t * 3f) + slot * 7;
                rotation = Quaternion.AngleAxis(ElementSubstance.Hash(redraw) * 360f, Vector3.up) * rotation;
            }
            if (part.OrbitCount > 0)
            {
                var round = Quaternion.AngleAxis(set.Sample(part.OrbitFirst, part.OrbitCount, t, Vector3.zero).x, Vector3.up);
                position = part.OrbitCentre + round * (position - part.OrbitCentre);
                rotation = round * rotation;
            }
            if (part.Kind == ElementLookSet.Flat) position.y += FlatLift;
            frame.SetPositionAndRotation(anchor + position, rotation);
            frame.localScale = Vector3.Scale(part.Scale, scaleKey) * shrink;
        }
    }
}

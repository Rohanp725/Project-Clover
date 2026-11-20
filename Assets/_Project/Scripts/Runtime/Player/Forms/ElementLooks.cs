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
        public const int PoolSize = 448;
        /// <summary>Looks playing at once.</summary>
        public const int MostShowings = 48;
        /// <summary>
        /// The most pieces one look has (Carried Embers' fire whirl and Thunder Gust's funnel are a hundred and more;
        /// Carried Embers is 183 once every piece the recipe gives its struck body is on it). A look built at the cap has
        /// lost pieces, so none may reach it (ElementLooksTests).
        /// </summary>
        public const int MostParts = 200;
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
            /// <summary>A reaction's look (played by <see cref="PlayReaction"/>): Marked's sun-wheel waits while it plays near.</summary>
            public bool Reaction;
            /// <summary>How much taller the body is than the recipe's stand-in (<see cref="StandIn"/>): the look is fitted to it.</summary>
            public float Tall;
        }

        /// <summary>
        /// The height of the clay stand-in the Blender worker drew the reactions on (its head at about 1.75 m), and the
        /// most a look is grown for a taller body. The castle's soldiers stand 2 to 2.3 m and the Gate Captain near 3, so
        /// the looks drawn round the stand-in came out small and low on them (the audit of 2026-11-20).
        /// </summary>
        public const float StandIn = 1.75f, MostTall = 1.45f;

        /// <summary>
        /// Metres out from the body within which a piece is moved out with the body's size; past it a piece moves only that
        /// much further, so what reaches another enemy (a feather, a lane, a thorn) still lands where it was drawn to.
        /// </summary>
        public const float NearBody = 1.2f;

        /// <summary>A ribbon reaching further than this from its body (an arc to another enemy, a lane) grows only upward.</summary>
        public const float FarRibbon = 1.6f;

        /// <summary>The Burn depth a reaction's body burns at in the real flame field while its look plays.</summary>
        public const int BodyFlameDepth = 3;

        /// <summary>How far from a reaction's body Marked's sun-wheel waits for it (<see cref="QuietAt"/>).</summary>
        public const float QuietReach = 1.3f;

        /// <summary>
        /// Reactions whose burning bodies keep their own fire, drawn by the real flame field (<see cref="BodyFlames"/>),
        /// instead of the recipe's rings of tongues stacked up the body, which read as the staircase the owner turned down
        /// for Burn (2026-11-20; still in Steam and Kindling Point in the audit's films). Smoulder and Red Hot draw their own
        /// (the black flame of the elements 3 redesign; the blast of tongues out through the enemy).
        /// </summary>
        private static readonly string[] BodyFlameReactions = { "steam", "kindling_point" };

        /// <summary>True when a reaction leaves its burning bodies' fire to the real flame field (<see cref="BodyFlameReactions"/>).</summary>
        public static bool KeepsBodyFlame(string slug)
        {
            for (int i = 0; i < BodyFlameReactions.Length; i++)
                if (string.Equals(BodyFlameReactions[i], slug, System.StringComparison.Ordinal)) return true;
            return false;
        }

        /// <summary>How much a reaction's look is grown on this body: its height over the stand-in's, never smaller.</summary>
        public static float TallOf(Transform body)
        {
            if (body == null) return 1f;
            var enemy = body.GetComponent<Clover.Gameplay.Enemy>();
            return enemy != null ? Mathf.Clamp(enemy.BodyHeight / StandIn, 1f, MostTall) : 1f;
        }

        /// <summary>
        /// True while a reaction's own look plays on a body standing within <see cref="QuietReach"/> of a point: what is
        /// laid there plainly (Marked's sun-wheel) waits for it.
        /// </summary>
        public static bool QuietAt(Vector3 feet)
        {
            if (instance == null || instance.showings == null) return false;
            for (int i = 0; i < instance.showings.Length; i++)
            {
                ref var one = ref instance.showings[i];
                if (!one.Active || !one.Reaction || one.Ending || one.Body == null) continue;
                Vector3 off = one.Body.position - feet;
                off.y = 0f;
                if (off.sqrMagnitude <= QuietReach * QuietReach) return true;
            }
            return false;
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

        /// <summary>Looks played once (a reaction's, a strike's) that are still playing, for probes: an affliction's kept look is not counted.</summary>
        public static int PlayingOnce
        {
            get
            {
                if (instance == null || instance.showings == null) return 0;
                int count = 0;
                for (int i = 0; i < instance.showings.Length; i++)
                    if (instance.showings[i].Active && instance.showings[i].Once) count++;
                return count;
            }
        }
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
        private static void ResetStatics()
        {
            instance = null;
            for (int i = 0; i < MostHidden; i++)
            {
                hiddenOn[i] = null;
                hiddenKinds[i] = 0;
                hiddenUntil[i] = 0f;
            }
        }

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

        /// <summary>Plays a look once on a body, turned to <paramref name="yaw"/> degrees as <see cref="PlayAt"/> turns it.</summary>
        public static bool Play(int look, Transform body, float yaw)
        {
            if (!Ready || body == null || look < 0 || look >= instance.set.Looks.Length) return false;
            int showing = instance.Begin(look, body);
            if (showing < 0) return false;
            instance.showings[showing].Once = true;
            instance.showings[showing].Turn = Quaternion.Euler(0f, yaw - instance.set.Looks[look].FacingYaw, 0f);
            return true;
        }

        // Bodies whose afflictions' looks are hidden while a reaction's own look plays on them: the body, the kinds, until when.
        private const int MostHidden = 16;
        private static readonly Transform[] hiddenOn = new Transform[MostHidden];
        private static readonly int[] hiddenKinds = new int[MostHidden];
        private static readonly float[] hiddenUntil = new float[MostHidden];

        private static void Hide(Transform body, int kinds, float seconds)
        {
            int slot = -1;
            for (int i = 0; i < MostHidden; i++)
            {
                if (ReferenceEquals(hiddenOn[i], body)) { slot = i; break; }
                if (slot < 0 && (hiddenOn[i] == null || Time.time >= hiddenUntil[i])) slot = i;
            }
            if (slot < 0) slot = 0;
            hiddenOn[slot] = body;
            hiddenKinds[slot] = kinds;
            hiddenUntil[slot] = Time.time + seconds;
        }

        /// <summary>True while a reaction's own look hides this affliction's look on this body.</summary>
        public static bool Hides(Transform body, int kind)
        {
            if (body == null) return false;
            for (int i = 0; i < MostHidden; i++)
                if (ReferenceEquals(hiddenOn[i], body)) return Time.time < hiddenUntil[i] && (hiddenKinds[i] & (1 << kind)) != 0;
            return false;
        }

        // What a reaction's look on another body may land on: enemies (and training dummies) found near where that body stood.
        private static readonly Collider[] Near = new Collider[24];
        private static readonly Transform[] Taken = new Transform[8];

        /// <summary>
        /// Plays a reaction's bespoke look (the Blender worker's combos 4, built under its slug): on <paramref name="body"/>,
        /// the one it landed on, turned so the recipe's way from the player becomes <paramref name="away"/>; its player's
        /// part on <paramref name="player"/>; and each part the recipe has on another body on the real enemy standing
        /// nearest where that body stood, when one stands within reach of it (a chain or an area reaching others). False
        /// when the reaction has no such look. Allocates nothing.
        /// </summary>
        public static bool PlayReaction(string slug, Transform body, Vector3 away, Transform player) => PlayReaction(slug, body, away, player, 0);

        /// <summary>
        /// The same, hiding on the body struck (and on each other body its look reaches) the looks of the afflictions the reaction spends (<paramref name="spends"/>,
        /// one bit a kind) for as long as its own look plays: the reaction's look shows what became of them (the
        /// coordinator, 2026-11-20: Seeded's pods sat over Quickening's bloom).
        /// </summary>
        public static bool PlayReaction(string slug, Transform body, Vector3 away, Transform player, int spends)
        {
            if (!Ready || body == null) return false;
            int main = instance.set.Find(slug);
            if (main < 0) return false;
            if (spends != 0) Hide(body, spends, ElementLookSet.ShownFor(instance.set.Looks[main]));
            away.y = 0f;
            float yaw = away.sqrMagnitude > 0.0001f ? Mathf.Atan2(away.x, away.z) * Mathf.Rad2Deg : instance.set.Looks[main].FacingYaw;
            PlayOnBody(main, body, yaw);
            var turn = Quaternion.Euler(0f, yaw - instance.set.Looks[main].FacingYaw, 0f);
            if (player != null)
            {
                int own = instance.set.FindVariant(slug, 'p');
                if (own >= 0) Play(own, player, yaw);
            }
            int taken = 0;
            Taken[taken++] = body;
            for (int n = 1; n < 8; n++)
            {
                int other = instance.set.FindVariant(slug, (char)('0' + n));
                if (other < 0) break;
                // The body a chain binds is not looked for where the recipe stood it: the game binds whoever is nearest
                // (Black Chain), and the live chain plays this part on that one when the bind begins (BoundChains).
                if (BoundChains.Binds(slug, n)) continue;
                Vector3 wanted = body.position + turn * instance.set.Looks[other].FromMain;
                var found = Nearest(wanted, 1.6f, taken);
                if (found == null) continue;
                PlayOnBody(other, found, yaw);
                // The others it reaches show it in its own look too, not in the plain looks of what it lays on them
                // (Caught Light's and Earthlight's films of 2026-11-20: every body it Marked rang a cream sun-wheel).
                if (spends != 0) Hide(found, spends, ElementLookSet.ShownFor(instance.set.Looks[main]));
                if (taken < Taken.Length) Taken[taken++] = found;
            }
            for (int i = 0; i < Taken.Length; i++) Taken[i] = null;
            return true;
        }

        // A reaction's look on one body: fitted to its height, and Marked's sun-wheel under it hushed while it plays.
        private static void PlayOnBody(int look, Transform body, float yaw)
        {
            if (!Play(look, body, yaw)) return;
            for (int i = 0; i < instance.showings.Length; i++)
            {
                ref var one = ref instance.showings[i];
                if (!one.Active || !ReferenceEquals(one.Body, body) || one.Look != look || one.Age > 0f) continue;
                one.Reaction = true;
                one.Tall = TallOf(body);
                break;
            }
            AfflictionArt.QuietMarks(body.position);
        }

        // The enemy or dummy standing nearest a point, within reach of it, that no part of this reaction is on yet.
        private static Transform Nearest(Vector3 point, float reach, int taken)
        {
            int count = Physics.OverlapSphereNonAlloc(point + Vector3.up, reach + 0.6f, Near, ~0, QueryTriggerInteraction.Ignore);
            Transform best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var enemy = Near[i].GetComponentInParent<Clover.Gameplay.Enemy>();
                Transform at = enemy != null ? enemy.transform : null;
                if (at == null)
                {
                    var dummy = Near[i].GetComponentInParent<Clover.Gameplay.TrainingDummy>();
                    at = dummy != null ? dummy.transform : null;
                }
                if (at == null) continue;
                bool used = false;
                for (int k = 0; k < taken; k++) used |= ReferenceEquals(Taken[k], at);
                if (used) continue;
                Vector3 flat = at.position - point;
                flat.y = 0f;
                float distance = flat.magnitude;
                if (distance > reach || distance >= bestDistance) continue;
                best = at;
                bestDistance = distance;
            }
            return best;
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
                if (wanted != ElementLookSet.NoShell && look.Shell != wanted && !(wanted == ElementLookSet.Ice && ElementLookSet.IsIceShell(look.Shell))) continue;
                shell = look.Shell;
                reveal = ElementLookSet.ShellReveal(look, one.Age);
                amount = one.Ending ? Mathf.Clamp01(1f - one.Gone / Going) : 1f;
                // A shell broken off before the look ends (Rime's crust at the heavy) is gone in a fifth of a second.
                if (look.ShellUntil > 0f && one.Age > look.ShellUntil) amount *= Mathf.Clamp01(1f - (one.Age - look.ShellUntil) / 0.2f);
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
                Turn = Quaternion.identity, Size = 1f, Tall = 1f,
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
                    bool over = one.Once ? one.Age >= ElementLookSet.ShownFor(look) : now - one.KeptAt > Linger;
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
                // Its body burns in the real flame field while it plays (Smoulder's black fire, Steam's lower flame).
                if (look.BodyFlame != ElementLookSet.NoFlame && !one.Ending && !bodyGone && one.Age >= look.BodyFlameFrom)
                    BodyFlames.Keep(one.Body, BodyFlameDepth, look.BodyFlame == ElementLookSet.BlackFlame);
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
                    // Spins and orbits of a part that keeps moving run on with the look's own age (a reaction's held
                    // aftermath too).
                    float angleTime = part.RunsOn ? one.Age : t;
                    Pose(part, t, angleTime, part.Follows ? feet : one.Ground, one.Turn, one.Size, one.Tall, shrink, slot, at[pooled]);
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

        private void Pose(in ElementLookSet.Part part, float t, float angleTime, Vector3 anchor, Quaternion turn, float size, float tall, float shrink, int slot, Transform frame)
        {
            set.LocalPose(part, t, angleTime, slot, out Vector3 position, out Quaternion rotation, out Vector3 scale);
            Vector3 stretch = Vector3.one;
            if (tall > 1.001f) Fit(part, tall, ref position, out stretch);
            if (part.Kind == ElementLookSet.Flat) position.y += FlatLift;
            frame.SetPositionAndRotation(anchor + turn * (position * size), turn * rotation);
            frame.localScale = Vector3.Scale(scale, stretch) * (shrink * size);
        }

        // A reaction's part grown to a body taller than the stand-in: up the body by its height, out from it by as much
        // near it but only a step more far out; a long ribbon reaching another body only upward, so it still lands there.
        private static void Fit(in ElementLookSet.Part part, float tall, ref Vector3 position, out Vector3 stretch)
        {
            position.y *= tall;
            float outward = Mathf.Sqrt(position.x * position.x + position.z * position.z);
            if (outward > 0.0001f)
            {
                float grown = (outward + (tall - 1f) * Mathf.Min(outward, NearBody)) / outward;
                position.x *= grown;
                position.z *= grown;
            }
            stretch = new Vector3(tall, tall, tall);
            if (part.Kind != ElementLookSet.Ribbon || part.Mesh == null) return;
            var bounds = part.Mesh.bounds;
            float reach = Mathf.Max(Mathf.Max(Mathf.Abs(bounds.min.x), Mathf.Abs(bounds.max.x)), Mathf.Max(Mathf.Abs(bounds.min.z), Mathf.Abs(bounds.max.z)));
            if (reach > FarRibbon) stretch = new Vector3(1f, tall, 1f);
        }
    }
}

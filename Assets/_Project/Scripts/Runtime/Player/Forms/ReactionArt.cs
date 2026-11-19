using Clover.Gameplay;
using UnityEngine;
using UnityEngine.Rendering;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Draws the reactions' looks (docs/GAME.md, "Reaction effects in the game") as something happening, not a still
    /// that appears and goes: every look plays in three beats over about 0.6 to 1.4 s (the cause arrives, the impact,
    /// the aftermath), each part moving by its cue (<see cref="ReactionMotion"/>): a bolt falls from above on a slant, a
    /// splash or thorns rise, wind sweeps in from the player's side, a ray is drawn out, a mark spreads from the feet,
    /// flame licks, steam drifts up and thins, a clang pulses, lightning strobes, cold hard things flash white where
    /// they land, and the lamps light at the impact.
    ///
    /// The 24 designed looks come from <see cref="ReactionArtSet"/> (their timelines from
    /// <see cref="ReactionTimelines.Designed"/>), their body parts riding on the struck enemy and their ground parts left
    /// where it stood. The other 31 are composed here from the parents' element art by
    /// <see cref="ReactionTimelines.Composed"/>: the first parent's piece arrives by its element's nature, the second's
    /// answers at the impact, and the ground mark spreads after.
    ///
    /// A fixed pool of renderers and lamps is made once; each renderer hangs under a frame that can be stretched along
    /// any way (a fall's, a sweep's, a ray's), and only transforms and property blocks change in play. Under load a new
    /// look leaves its aftermath out, and a look short of renderers takes the oldest look's aftermath before ending it.
    /// Nothing is made or allocated during play. <see cref="ElementArt"/> adds this beside itself when its set carries the
    /// reactions, and its F8 switch turns these off with the rest (the reaction then shows the old puff).
    /// </summary>
    public class ReactionArt : MonoBehaviour
    {
        [SerializeField] private ReactionArtSet set;

        /// <summary>Renderers in the pool, and lamps. A designed look has up to ten parts and a composed one three; the crowd shows a couple of dozen looks at once.</summary>
        public int PartCount = 160;
        public int LampCount = 6;
        public int MostLooks = 32;
        /// <summary>Past this share of the pool in use, a new look leaves out its aftermath (smoke, steam, the last chips and marks).</summary>
        public float AftermathShare = 0.7f;

        [Header("A set built before the motion (seconds)")]
        public float PopSeconds = 0.1f;
        public float BodySeconds = 0.85f;
        public float GroundSeconds = 1.1f;
        public float MarkSeconds = 1.8f;
        /// <summary>A mark is drawn outward from its middle over this long.</summary>
        public float RevealSeconds = 0.18f;

        [Header("Lamps and paths")]
        /// <summary>A look's lamps light at its impact and fade over this long.</summary>
        public float LampSeconds = 0.45f;
        /// <summary>How far a fall leans sideways (toward the camera's right) for each metre it drops, so it is seen coming down, not only growing.</summary>
        public float FallSlant = 0.3f;
        /// <summary>How far each redraw of a strobe lands off the last, in metres.</summary>
        public float RedrawJitter = 0.06f;

        [Header("Turning the look-dev's frames")]
        /// <summary>The look-dev camera looked along -Z, the game's (at yaw 0) along +Z: a look is turned this far past the camera's yaw to be seen as drawn.</summary>
        public float LookDevTurn = 180f;
        /// <summary>The way from the look-dev's player through the enemy, as a yaw in the recipe's space (the player stood toward +X, +Z).</summary>
        public float AwayInRecipe = 220f;

        private const float Ink = 0.025f;
        private const float MarkLift = 0.045f;
        /// <summary>A composed look's parts: the two parents' pieces and a ground mark.</summary>
        private const int ComposedParts = 3;
        private static readonly int LightElement = ReactionDesigns.RingIndex("light");

        /// <summary>Pool renderers and lamps in use now, for probes: both are zero once every look has ended.</summary>
        public static int PartsShown => instance != null ? instance.partsShown : 0;
        public static int LampsShown => instance != null ? instance.lampsShown : 0;
        /// <summary>Designed looks started this play, composed ones drawn, looks ended early to make room, and aftermath parts left out under load.</summary>
        public static int Played { get; private set; }
        public static int ComposedCount { get; private set; }
        public static int Ended { get; private set; }
        public static int Dropped { get; private set; }
        /// <summary>True when the scene has the designed looks to draw.</summary>
        public static bool Ready => instance != null && instance.built;

        private static ReactionArt instance;

        private struct Showing
        {
            public bool Active;
            /// <summary>The designed look, or -1 for a composed one.</summary>
            public int Look;
            public int Parts;
            public int Lamps;
            public float Age;
            public float ImpactAt;
            public Transform Body;
            public Vector3 Origin;
            public Vector3 BodyAt;
            public Quaternion Turn;
            /// <summary>Flat, from the player through the enemy (a sweep comes in along it).</summary>
            public Vector3 Away;
            /// <summary>Up and a little aside: a fall comes down along it.</summary>
            public Vector3 FallFrom;
        }

        private static readonly int ColourId = Shader.PropertyToID("_Color");
        private static readonly int HotId = Shader.PropertyToID("_Hot");
        private static readonly int InkId = Shader.PropertyToID("_OutlineWidth");
        private static readonly int RevealId = Shader.PropertyToID("_Reveal");

        private bool built;
        private int[] lookOfPair;
        private int mostParts;
        private Showing[] showings;
        private int[] slots;       // showing * mostParts + part: the pool renderer drawing it, or -1
        private ReactionArtSet.Part[] live;   // the same: what it draws (a designed part, or a composed one made here)
        private float[] liveYaw;   // the same: its turn past the look's
        private float[] liveSize;  // the same: its size (a composed look's second parent is drawn smaller)
        private int[] lampSlots;   // showing * MostLampsPerLook + lamp: the pool lamp, or -1
        private Transform[] partAt;      // the frame, stretched along a way
        private Transform[] partShape;   // under it, the renderer, turned back to the look's turn
        private MeshFilter[] partMesh;
        private MeshRenderer[] partShown;
        private bool[] partUsed;
        private float[] paintedAlpha, paintedHot, paintedReveal;
        private Light[] lamps;
        private bool[] lampUsed;
        private Mesh quad;
        private MaterialPropertyBlock block;
        private Transform viewer;
        private int partsShown, lampsShown;

        // Domain reload is off: the scene's looks and the counts of the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            Played = 0;
            ComposedCount = 0;
            Ended = 0;
            Dropped = 0;
        }

        /// <summary>Hands over the looks and makes the pool (once).</summary>
        public void Configure(ReactionArtSet art)
        {
            set = art;
            Build();
        }

        private void Awake() => Build();

        private void OnEnable() => instance = this;

        private void OnDisable()
        {
            if (instance == this) instance = null;
            if (!built) return;
            for (int i = 0; i < showings.Length; i++)
                if (showings[i].Active) End(i);
        }

        private void OnDestroy()
        {
            if (quad != null) Destroy(quad);
        }

        private void Build()
        {
            if (built || set == null || set.Looks == null || set.Looks.Length == 0) return;
            built = true;
            block = new MaterialPropertyBlock();
            quad = FormMeshes.Square();
            lookOfPair = new int[AfflictionPairs.Count];
            for (int pair = 0; pair < lookOfPair.Length; pair++) lookOfPair[pair] = -1;
            for (int look = 0; look < set.Looks.Length; look++)
            {
                int pair = set.Looks[look].Pair;
                if (pair >= 0 && pair < lookOfPair.Length) lookOfPair[pair] = look;
            }
            mostParts = Mathf.Max(ComposedParts, set.MostParts);
            showings = new Showing[Mathf.Max(1, MostLooks)];
            slots = new int[showings.Length * mostParts];
            live = new ReactionArtSet.Part[slots.Length];
            liveYaw = new float[slots.Length];
            liveSize = new float[slots.Length];
            lampSlots = new int[showings.Length * ReactionDesigns.MostLampsPerLook];
            for (int i = 0; i < slots.Length; i++) slots[i] = -1;
            for (int i = 0; i < lampSlots.Length; i++) lampSlots[i] = -1;

            int parts = Mathf.Max(mostParts, PartCount);
            partAt = new Transform[parts];
            partShape = new Transform[parts];
            partMesh = new MeshFilter[parts];
            partShown = new MeshRenderer[parts];
            partUsed = new bool[parts];
            paintedAlpha = new float[parts];
            paintedHot = new float[parts];
            paintedReveal = new float[parts];
            for (int i = 0; i < parts; i++)
            {
                var frame = new GameObject("Reaction Part");
                partAt[i] = frame.transform;
                partAt[i].SetParent(transform, false);
                var shape = new GameObject("Shape");
                partShape[i] = shape.transform;
                partShape[i].SetParent(partAt[i], false);
                partMesh[i] = shape.AddComponent<MeshFilter>();
                partShown[i] = shape.AddComponent<MeshRenderer>();
                partShown[i].shadowCastingMode = ShadowCastingMode.Off;
                partShown[i].receiveShadows = false;
                partShown[i].lightProbeUsage = LightProbeUsage.Off;
                partShown[i].reflectionProbeUsage = ReflectionProbeUsage.Off;
                partShown[i].enabled = false;
            }
            lamps = new Light[Mathf.Max(1, LampCount)];
            lampUsed = new bool[lamps.Length];
            for (int i = 0; i < lamps.Length; i++)
            {
                var lamp = new GameObject("Reaction Lamp");
                lamp.transform.SetParent(transform, false);
                lamps[i] = lamp.AddComponent<Light>();
                lamps[i].type = LightType.Point;
                lamps[i].shadows = LightShadows.None;
                lamps[i].renderMode = LightRenderMode.Auto;
                lamps[i].enabled = false;
            }
            FindViewer();
        }

        private void FindViewer()
        {
            var main = Camera.main;
            viewer = main != null ? main.transform : null;
        }

        // ---- Asking.

        /// <summary>True when the pair has a designed look in this scene.</summary>
        public static bool IsDesigned(int pair) => Ready && pair >= 0 && pair < AfflictionPairs.Count && instance.lookOfPair[pair] >= 0;

        /// <summary>
        /// Shows the pair's designed look on a body whose feet are at <paramref name="at"/>. Body parts follow
        /// <paramref name="body"/> while it lives; <paramref name="away"/> is the way the blow carried (from the player
        /// through the enemy), which turns the looks that have a way to them and is the way wind sweeps in. False when
        /// the pair has no designed look here (the caller composes one).
        /// </summary>
        public static bool Play(int pair, Vector3 at, Transform body, Vector3 away)
        {
            if (!IsDesigned(pair)) return false;
            instance.Show(instance.lookOfPair[pair], at, body, away);
            Played++;
            return true;
        }

        /// <summary>
        /// For probes: the world matrix of every pool renderer drawn now, slot by slot in a fixed order (a look's part
        /// keeps its slot for its life), zero where a slot draws nothing this frame. Returns how many were written.
        /// </summary>
        public static int ShownMatrices(Matrix4x4[] into)
        {
            if (!Ready || into == null) return 0;
            int count = Mathf.Min(into.Length, instance.slots.Length);
            for (int slot = 0; slot < count; slot++)
            {
                int pooled = instance.slots[slot];
                into[slot] = pooled >= 0 && instance.partShown[pooled].enabled ? instance.partShape[pooled].localToWorldMatrix : default;
            }
            return count;
        }

        private void Show(int look, Vector3 at, Transform body, Vector3 away)
        {
            ref var info = ref set.Looks[look];
            int showing = Begin(at, body, away, Quaternion.Euler(0f, Yaw(info.Directed, away), 0f));
            ref var one = ref showings[showing];
            one.Look = look;
            one.Parts = Mathf.Min(info.PartCount, mostParts);
            one.Lamps = Mathf.Min(info.LampCount, ReactionDesigns.MostLampsPerLook);
            one.ImpactAt = info.ImpactAt;
            // Under load the aftermath is what goes: the cause and the impact are the reaction.
            bool lean = partsShown + one.Parts > partUsed.Length * AftermathShare;
            for (int k = 0; k < one.Parts; k++)
            {
                int slot = showing * mostParts + k;
                live[slot] = set.Parts[info.FirstPart + k];
                if (!live[slot].Motion.Set) live[slot].Motion = Unmoving(live[slot]);
                liveYaw[slot] = 0f;
                liveSize[slot] = 1f;
                if (lean && live[slot].Motion.Beat == ReactionMotion.Aftermath)
                {
                    Dropped++;
                    continue;
                }
                Take(showing, k);
            }
            for (int k = 0; k < one.Lamps; k++)
            {
                int pooled = TakeLamp(showing);
                lampSlots[showing * ReactionDesigns.MostLampsPerLook + k] = pooled;
                if (pooled < 0) continue;
                ref var lamp = ref set.Lamps[info.FirstLamp + k];
                lamps[pooled].color = lamp.Colour;
                lamps[pooled].range = lamp.Range;
            }
            Place(showing);
        }

        // A set built before the motion: its parts pop in and hold for the times this one was given.
        private ReactionMotion.Cue Unmoving(in ReactionArtSet.Part part)
        {
            if (part.Kind == ReactionArtSet.Decal) return ReactionMotion.Cue.Of(ReactionMotion.Spread, ReactionMotion.Impact, 0f, RevealSeconds, MarkSeconds, 0.6f);
            return ReactionMotion.Cue.Of(ReactionMotion.Pop, ReactionMotion.Impact, 0f, PopSeconds, part.Place == ReactionArtSet.OnBody ? BodySeconds : GroundSeconds);
        }

        // A free showing, set going at a body's feet; the ways its falls and sweeps come from are fixed now.
        private int Begin(Vector3 at, Transform body, Vector3 away, Quaternion turn)
        {
            int showing = FreeShowing();
            ref var one = ref showings[showing];
            one.Active = true;
            one.Look = -1;
            one.Parts = 0;
            one.Lamps = 0;
            one.Age = 0f;
            one.ImpactAt = 0f;
            one.Body = body;
            one.Origin = at;
            one.BodyAt = body != null ? body.position : at;
            one.Turn = turn;

            Vector3 right = Vector3.right, forward = Vector3.forward;
            if (viewer != null)
            {
                right = Flat(viewer.right, Vector3.right);
                forward = Flat(viewer.forward, Vector3.forward);
            }
            // A sweep comes from the player's side; with no way given, from the left of the screen.
            one.Away = Flat(away, right);
            // A little aside and a little farther from the camera than straight up: higher on the screen, so the fall is seen.
            one.FallFrom = (Vector3.up + right * FallSlant + forward * 0.15f).normalized;
            return showing;
        }

        private static Vector3 Flat(Vector3 way, Vector3 otherwise)
        {
            way.y = 0f;
            return way.sqrMagnitude > 0.0001f ? way.normalized : otherwise;
        }

        // The camera's yaw plus the turn that shows the look as its frame was drawn; or, for a look with a way to it, the
        // turn that lays its way along the blow's.
        private float Yaw(bool directed, Vector3 away)
        {
            away.y = 0f;
            if (directed && away.sqrMagnitude > 0.0001f) return Mathf.Atan2(away.x, away.z) * Mathf.Rad2Deg - AwayInRecipe;
            float cameraYaw = 0f;
            if (viewer != null)
            {
                Vector3 forward = viewer.forward;
                forward.y = 0f;
                if (forward.sqrMagnitude > 0.0001f) cameraYaw = Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg;
            }
            return cameraYaw + LookDevTurn;
        }

        private int FreeShowing()
        {
            int oldest = 0;
            for (int i = 0; i < showings.Length; i++)
            {
                if (!showings[i].Active) return i;
                if (showings[i].Age > showings[oldest].Age) oldest = i;
            }
            End(oldest);
            return oldest;
        }

        // A renderer for a showing's part, dressed in its mesh and material and hidden until its time comes.
        private void Take(int showing, int k)
        {
            int slot = showing * mostParts + k;
            int pooled = TakePart(showing);
            slots[slot] = pooled;
            if (pooled < 0) return;
            ref var part = ref live[slot];
            bool decal = part.Kind == ReactionArtSet.Decal;
            partMesh[pooled].sharedMesh = decal ? quad : part.Mesh;
            partShown[pooled].sharedMaterial = part.Material;
            partShape[pooled].localScale = decal ? new Vector3(part.Size.x, 1f, part.Size.z) : Vector3.one;
            partShown[pooled].enabled = false;
            // Its block is written the first time it is placed.
            paintedAlpha[pooled] = -1f;
        }

        // A free renderer; when none is free, the oldest other look's aftermath gives one up, and failing that the oldest
        // other look gives up all of its own.
        private int TakePart(int forShowing)
        {
            for (int pass = 0; pass < 3; pass++)
            {
                for (int i = 0; i < partUsed.Length; i++)
                    if (!partUsed[i])
                    {
                        partUsed[i] = true;
                        partsShown++;
                        return i;
                    }
                if (pass == 0 && StealAftermath(forShowing)) continue;
                int oldest = Oldest(forShowing);
                if (oldest < 0) return -1;
                End(oldest);
            }
            return -1;
        }

        private bool StealAftermath(int except)
        {
            int oldest = -1, oldestSlot = -1;
            for (int i = 0; i < showings.Length; i++)
            {
                if (i == except || !showings[i].Active) continue;
                if (oldest >= 0 && showings[i].Age <= showings[oldest].Age) continue;
                for (int k = 0; k < showings[i].Parts; k++)
                {
                    int slot = i * mostParts + k;
                    if (slots[slot] < 0 || live[slot].Motion.Beat != ReactionMotion.Aftermath) continue;
                    oldest = i;
                    oldestSlot = slot;
                    break;
                }
            }
            if (oldestSlot < 0) return false;
            FreePart(oldestSlot);
            Dropped++;
            return true;
        }

        private int TakeLamp(int forShowing)
        {
            for (int i = 0; i < lampUsed.Length; i++)
                if (!lampUsed[i])
                {
                    lampUsed[i] = true;
                    lampsShown++;
                    return i;
                }
            // Every lamp lit: the oldest look's lamps go out first (its parts stay).
            int oldest = -1;
            for (int i = 0; i < showings.Length; i++)
            {
                if (i == forShowing || !showings[i].Active || !HasLamp(i)) continue;
                if (oldest < 0 || showings[i].Age > showings[oldest].Age) oldest = i;
            }
            if (oldest < 0) return -1;
            for (int k = 0; k < ReactionDesigns.MostLampsPerLook; k++)
            {
                int slot = oldest * ReactionDesigns.MostLampsPerLook + k;
                int pooled = lampSlots[slot];
                if (pooled < 0) continue;
                lampSlots[slot] = -1;
                lamps[pooled].enabled = false;
                return pooled;
            }
            return -1;
        }

        private bool HasLamp(int showing)
        {
            for (int k = 0; k < ReactionDesigns.MostLampsPerLook; k++)
                if (lampSlots[showing * ReactionDesigns.MostLampsPerLook + k] >= 0) return true;
            return false;
        }

        private int Oldest(int except)
        {
            int oldest = -1;
            for (int i = 0; i < showings.Length; i++)
            {
                if (i == except || !showings[i].Active) continue;
                if (oldest < 0 || showings[i].Age > showings[oldest].Age) oldest = i;
            }
            return oldest;
        }

        private void End(int showing)
        {
            if (!showings[showing].Active) return;
            for (int k = 0; k < mostParts; k++) FreePart(showing * mostParts + k);
            for (int k = 0; k < ReactionDesigns.MostLampsPerLook; k++) FreeLamp(showing * ReactionDesigns.MostLampsPerLook + k);
            showings[showing].Active = false;
            showings[showing].Body = null;
            Ended++;
        }

        private void FreePart(int slot)
        {
            int pooled = slots[slot];
            if (pooled < 0) return;
            slots[slot] = -1;
            partShown[pooled].enabled = false;
            partMesh[pooled].sharedMesh = null;
            partUsed[pooled] = false;
            partsShown--;
        }

        private void FreeLamp(int slot)
        {
            int pooled = lampSlots[slot];
            if (pooled < 0) return;
            lampSlots[slot] = -1;
            lamps[pooled].enabled = false;
            lampUsed[pooled] = false;
            lampsShown--;
        }

        // ---- Each frame.

        private void LateUpdate()
        {
            if (!built) return;
            if (viewer == null && Time.frameCount % 60 == 0) FindViewer();
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f) return;
            for (int i = 0; i < showings.Length; i++)
            {
                if (!showings[i].Active) continue;
                showings[i].Age += deltaTime;
                if (!Place(i))
                {
                    // Every part and lamp has run its time: the look is over, not cut short.
                    for (int k = 0; k < mostParts; k++) FreePart(i * mostParts + k);
                    showings[i].Active = false;
                    showings[i].Body = null;
                }
            }
        }

        // Poses a look's parts and lamps for its age, and lets go of those whose time is up. False when nothing is left.
        private bool Place(int showing)
        {
            ref var one = ref showings[showing];
            // The body is followed while it stands; once it is gone its parts stay where it was last seen.
            if (one.Body != null && one.Body.gameObject.activeInHierarchy) one.BodyAt = one.Body.position;
            float age = one.Age;
            bool any = false;
            for (int k = 0; k < one.Parts; k++)
            {
                int slot = showing * mostParts + k;
                int pooled = slots[slot];
                if (pooled < 0) continue;
                int state = ReactionMotion.Evaluate(live[slot].Motion, age, slot, out var pose);
                if (state == ReactionMotion.Over)
                {
                    FreePart(slot);
                    continue;
                }
                any = true;
                if (state == ReactionMotion.Waiting) continue;
                PosePart(showing, k, slot, pooled, pose);
            }
            for (int k = 0; k < one.Lamps; k++)
            {
                int slot = showing * ReactionDesigns.MostLampsPerLook + k;
                int pooled = lampSlots[slot];
                if (pooled < 0) continue;
                // A look's lamps light at its impact, not before.
                float since = age - one.ImpactAt;
                if (since >= LampSeconds)
                {
                    FreeLamp(slot);
                    continue;
                }
                any = true;
                if (since < 0f) continue;
                ref var lamp = ref set.Lamps[set.Looks[one.Look].FirstLamp + k];
                float fade = 1f - since / LampSeconds;
                lamps[pooled].transform.position = one.BodyAt + one.Turn * lamp.Position;
                lamps[pooled].intensity = lamp.Intensity * fade * fade;
                if (!lamps[pooled].enabled) lamps[pooled].enabled = true;
            }
            return any;
        }

        private void PosePart(int showing, int k, int slot, int pooled, in ReactionMotion.Pose pose)
        {
            ref var one = ref showings[showing];
            ref var part = ref live[slot];
            Quaternion turn = liveYaw[slot] != 0f ? one.Turn * Quaternion.Euler(0f, liveYaw[slot], 0f) : one.Turn;
            Vector3 anchor = part.Place == ReactionArtSet.OnBody ? one.BodyAt : one.Origin;
            var frame = partAt[pooled];
            float alpha = 1f;
            if (part.Kind == ReactionArtSet.Decal)
            {
                // A mark spreads from the feet to its place, and lies a hair above the one before it.
                float spread = pose.Across * pose.Size;
                Vector3 middle = anchor + turn * new Vector3(part.Centre.x * spread, 0f, part.Centre.z * spread);
                middle.y += MarkLift + k * 0.002f + showing * 0.0003f;
                frame.SetPositionAndRotation(middle, Quaternion.identity);
                frame.localScale = new Vector3(spread, 1f, spread);
                partShape[pooled].localRotation = turn;
                alpha = pose.Left;
            }
            else
            {
                Vector3 axis = Vector3.up, path = Vector3.zero;
                byte verb = part.Motion.Verb;
                if (verb == ReactionMotion.Fall)
                {
                    axis = one.FallFrom;
                    path = one.FallFrom * part.Motion.Amount;
                }
                else if (verb == ReactionMotion.Sweep)
                {
                    axis = one.Away;
                    path = -one.Away * part.Motion.Amount;
                }
                else if (verb == ReactionMotion.Extend && part.Axis.sqrMagnitude > 0.0001f)
                {
                    axis = turn * part.Axis;
                }
                Vector3 position = anchor + turn * part.Pivot + path * pose.Travel;
                position.y += pose.Lift;
                if (pose.Redraw > 0) position += Jitter(slot, pose.Redraw);
                float size = pose.Size * liveSize[slot];
                if (part.Kind == ReactionArtSet.Solid)
                {
                    // Solids never fade: they close to nothing about their pivot, as the element art's moments sink.
                    size *= Mathf.Max(0.001f, pose.Left);
                }
                else
                {
                    size *= Mathf.Lerp(0.7f, 1f, pose.Left);
                    alpha = pose.Left;
                }
                // The frame's Y runs along the way it is stretched; the shape under it is turned back, so the stretch
                // lies along that way whatever the look's own turn.
                Quaternion along = axis.y > 0.9999f ? Quaternion.identity : Quaternion.FromToRotation(Vector3.up, axis);
                frame.SetPositionAndRotation(position, along);
                frame.localScale = new Vector3(pose.Across * size, pose.Along * size, pose.Across * size);
                Quaternion shapeTurn = pose.Spin != 0f ? Quaternion.AngleAxis(pose.Spin, Vector3.up) * turn : turn;
                partShape[pooled].localRotation = Quaternion.Inverse(along) * shapeTurn;
            }
            Paint(pooled, part.Kind, alpha, pose.Hot, pose.Reveal);
            if (partShown[pooled].enabled != pose.Shown) partShown[pooled].enabled = pose.Shown;
        }

        // Each redraw of a strobe a little off the last, the same way every time for the same slot.
        private Vector3 Jitter(int slot, int redraw)
        {
            float angle = (slot * 7 + redraw * 13) * 2.39996f;
            return new Vector3(Mathf.Cos(angle), 0f, Mathf.Sin(angle)) * RedrawJitter;
        }

        // A renderer's block, written only when what it carries has changed.
        private void Paint(int pooled, byte kind, float alpha, float hot, float reveal)
        {
            bool solid = kind == ReactionArtSet.Solid, decal = kind == ReactionArtSet.Decal;
            if (paintedAlpha[pooled] >= 0f && Mathf.Abs(paintedAlpha[pooled] - alpha) < 0.004f && (!solid || paintedHot[pooled] == hot) &&
                (!decal || Mathf.Abs(paintedReveal[pooled] - reveal) < 0.004f)) return;
            paintedAlpha[pooled] = alpha;
            paintedHot[pooled] = hot;
            paintedReveal[pooled] = reveal;
            block.Clear();
            block.SetColor(ColourId, new Color(1f, 1f, 1f, alpha));
            if (solid)
            {
                block.SetFloat(HotId, hot);
                block.SetFloat(InkId, Ink);
            }
            if (decal) block.SetFloat(RevealId, reveal);
            partShown[pooled].SetPropertyBlock(block);
        }

        // ---- The 31 without a design: composed from the parents' element art.

        /// <summary>The pair's composed look at a body's feet, not followed (<see cref="Compose(int, Vector3, Transform, Vector3)"/>).</summary>
        public static bool Compose(int pair, Vector3 at, Vector3 away) => Compose(pair, at, null, away);

        /// <summary>
        /// Draws a reaction with no designed look from its two parents' element art, in three beats
        /// (<see cref="ReactionTimelines.Composed"/>): the first parent's own piece arrives by its element's nature at the
        /// body's feet, the second's answers it at the impact (a little smaller and turned away from the first), and one
        /// ground mark spreads after. The pieces follow <paramref name="body"/> while it stands. False when the element
        /// art is off or not in the scene.
        /// </summary>
        public static bool Compose(int pair, Vector3 at, Transform body, Vector3 away)
        {
            if (!ElementArt.Ready || pair < 0 || pair >= AfflictionPairs.Count) return false;
            ReactionDesigns.Composed(pair, out string first, out string second, out string mark, out int markElement);
            int a = ReactionDesigns.RingIndex(AfflictionPairs.First(pair)), b = ReactionDesigns.RingIndex(AfflictionPairs.Second(pair));
            if (Ready) instance.ShowComposed(pair, at, body, away, first, a, second, b, mark, markElement);
            else
            {
                // No pool of our own in this scene: the parents' pieces burst up through the element art's.
                float yaw = instance != null ? instance.Yaw(false, away) : 180f;
                Rise(first, a, at, yaw, 1f);
                Rise(second, b, at, yaw + 150f, 0.85f);
                if (mark != null)
                    ElementArt.Floor(ElementArt.Piece(mark), at, yaw, ReactionDesigns.MarkScale(markElement) * 0.7f, ReactionDesigns.MarkScale(markElement), 1.6f, 0.15f, true, Color.white);
            }
            ComposedCount++;
            return true;
        }

        private void ShowComposed(int pair, Vector3 at, Transform body, Vector3 away, string first, int a, string second, int b, string mark, int markElement)
        {
            float yaw = Yaw(false, away);
            int showing = Begin(at, body, away, Quaternion.Euler(0f, yaw, 0f));
            ReactionTimelines.Composed(pair, out var arrives, out var answers, out var spreads);
            showings[showing].ImpactAt = answers.Delay;
            AddPiece(showing, first, a, arrives, 1f, 0f, yaw);
            AddPiece(showing, second, b, answers, 0.85f, 150f, yaw);
            if (mark != null) AddPiece(showing, mark, markElement, spreads, ReactionDesigns.MarkScale(markElement), 0f, yaw, ReactionArtSet.OnGround);
            Place(showing);
        }

        private void AddPiece(int showing, string name, int element, in ReactionMotion.Cue cue, float scale, float turnBy, float yaw, byte place = ReactionArtSet.OnBody)
        {
            ref var one = ref showings[showing];
            if (name == null || one.Parts >= mostParts || !ElementArt.TryPiece(ElementArt.Piece(name), out var piece)) return;
            int k = one.Parts++;
            int slot = showing * mostParts + k;
            bool decal = piece.Kind == ElementArtSet.Decal;
            // Light's rayed ring stands behind the body from the camera, as its hit does; the rest are at the feet.
            bool behind = element == LightElement && !decal;
            live[slot] = new ReactionArtSet.Part
            {
                Kind = piece.Kind, Place = place, Mesh = piece.Mesh, Material = piece.Material, Motion = cue,
                Pivot = behind ? new Vector3(0f, 0f, -0.35f) : Vector3.zero,
                Centre = decal ? piece.Centre * scale : Vector3.zero, Size = decal ? piece.Size * scale : piece.Size,
            };
            liveYaw[slot] = behind ? 180f - yaw : turnBy;
            liveSize[slot] = decal ? 1f : behind ? 0.85f * scale : scale;
            Take(showing, k);
        }

        private static void Rise(string piece, int element, Vector3 feet, float yaw, float scale)
        {
            if (piece == null) return;
            // Light's rayed ring stands behind the body from the camera, as its hit does; the rest rise at the feet.
            bool behind = element == LightElement;
            Vector3 at = behind ? feet + Vector3.forward * 0.35f : feet;
            float turn = behind ? 180f : yaw;
            ElementArt.Erupt(ElementArt.Piece(piece), at, Quaternion.Euler(0f, turn, 0f), Vector3.one * (behind ? 0.85f * scale : scale), 0.08f, 0.45f, 0.25f);
        }
    }
}

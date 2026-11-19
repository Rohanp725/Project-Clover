using Clover.Gameplay;
using UnityEngine;
using UnityEngine.Rendering;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Draws the reactions' looks (docs/GAME.md, "Reaction effects in the game"). The 24 designed ones come from
    /// <see cref="ReactionArtSet"/>: each look's parts pop in over a tenth of a second, hold, and shrink away (solids) or
    /// fade (marks, glows), its body parts riding on the struck enemy and its ground parts left where it stood, with at
    /// most two short lamps. The other 31 are composed from the parents' element art through <see cref="FormFx"/>.
    /// A fixed pool of renderers and lamps is made once; a new look when all are in use ends the oldest. Nothing is made
    /// or allocated during play. <see cref="ElementArt"/> adds this beside itself when its set carries the reactions,
    /// and its F8 switch turns these off with the rest (the reaction then shows the old puff).
    /// </summary>
    public class ReactionArt : MonoBehaviour
    {
        [SerializeField] private ReactionArtSet set;

        /// <summary>Renderers in the pool, and lamps. A look has up to about ten parts; the crowd shows a dozen looks at once.</summary>
        public int PartCount = 120;
        public int LampCount = 6;
        public int MostLooks = 16;

        [Header("Timing (seconds)")]
        public float PopSeconds = 0.1f;
        public float BodySeconds = 0.85f;
        public float GroundSeconds = 1.1f;
        public float MarkSeconds = 1.8f;
        /// <summary>The share of a part's time at its end in which it shrinks or fades away.</summary>
        public float OutShare = 0.35f;
        /// <summary>A mark is drawn outward from its middle over this long.</summary>
        public float RevealSeconds = 0.18f;
        public float LampSeconds = 0.45f;

        [Header("Turning the look-dev's frames")]
        /// <summary>The look-dev camera looked along -Z, the game's (at yaw 0) along +Z: a look is turned this far past the camera's yaw to be seen as drawn.</summary>
        public float LookDevTurn = 180f;
        /// <summary>The way from the look-dev's player through the enemy, as a yaw in the recipe's space (the player stood toward +X, +Z).</summary>
        public float AwayInRecipe = 220f;

        private const float Ink = 0.025f;
        private const float MarkLift = 0.045f;
        private static readonly int LightElement = ReactionDesigns.RingIndex("light");

        /// <summary>Pool renderers and lamps in use now, for probes: both are zero once every look has ended.</summary>
        public static int PartsShown => instance != null ? instance.partsShown : 0;
        public static int LampsShown => instance != null ? instance.lampsShown : 0;
        /// <summary>Designed looks started this play, composed ones drawn, and looks ended early to make room.</summary>
        public static int Played { get; private set; }
        public static int ComposedCount { get; private set; }
        public static int Ended { get; private set; }
        /// <summary>True when the scene has the designed looks to draw.</summary>
        public static bool Ready => instance != null && instance.built;

        private static ReactionArt instance;

        private struct Showing
        {
            public bool Active;
            public int Look;
            public float Age;
            public Transform Body;
            public Vector3 Origin;
            public Vector3 BodyAt;
            public Quaternion Turn;
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
        private int[] lampSlots;   // showing * MostLampsPerLook + lamp: the pool lamp, or -1
        private Transform[] partAt;
        private MeshFilter[] partMesh;
        private MeshRenderer[] partShown;
        private bool[] partUsed;
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
            mostParts = Mathf.Max(1, set.MostParts);
            showings = new Showing[Mathf.Max(1, MostLooks)];
            slots = new int[showings.Length * mostParts];
            lampSlots = new int[showings.Length * ReactionDesigns.MostLampsPerLook];
            for (int i = 0; i < slots.Length; i++) slots[i] = -1;
            for (int i = 0; i < lampSlots.Length; i++) lampSlots[i] = -1;

            int parts = Mathf.Max(mostParts, PartCount);
            partAt = new Transform[parts];
            partMesh = new MeshFilter[parts];
            partShown = new MeshRenderer[parts];
            partUsed = new bool[parts];
            for (int i = 0; i < parts; i++)
            {
                var part = new GameObject("Reaction Part");
                partAt[i] = part.transform;
                partAt[i].SetParent(transform, false);
                partMesh[i] = part.AddComponent<MeshFilter>();
                partShown[i] = part.AddComponent<MeshRenderer>();
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
        /// through the enemy), which turns the looks that have a way to them. False when the pair has no designed look
        /// here (the caller composes one).
        /// </summary>
        public static bool Play(int pair, Vector3 at, Transform body, Vector3 away)
        {
            if (!IsDesigned(pair)) return false;
            instance.Show(instance.lookOfPair[pair], at, body, away);
            Played++;
            return true;
        }

        private void Show(int look, Vector3 at, Transform body, Vector3 away)
        {
            int showing = FreeShowing();
            ref var one = ref showings[showing];
            ref var info = ref set.Looks[look];
            one.Active = true;
            one.Look = look;
            one.Age = 0f;
            one.Body = body;
            one.Origin = at;
            one.BodyAt = body != null ? body.position : at;
            one.Turn = Quaternion.Euler(0f, Yaw(info.Directed, away), 0f);

            for (int k = 0; k < info.PartCount && k < mostParts; k++)
            {
                int pooled = TakePart(showing);
                slots[showing * mostParts + k] = pooled;
                if (pooled < 0) continue;
                ref var part = ref set.Parts[info.FirstPart + k];
                partMesh[pooled].sharedMesh = part.Kind == ReactionArtSet.Decal ? quad : part.Mesh;
                partShown[pooled].sharedMaterial = part.Material;
                block.Clear();
                block.SetColor(ColourId, Color.white);
                if (part.Kind == ReactionArtSet.Solid)
                {
                    block.SetFloat(HotId, 0f);
                    block.SetFloat(InkId, Ink);
                }
                if (part.Kind == ReactionArtSet.Decal) block.SetFloat(RevealId, 0f);
                partShown[pooled].SetPropertyBlock(block);
            }
            for (int k = 0; k < info.LampCount && k < ReactionDesigns.MostLampsPerLook; k++)
            {
                int pooled = TakeLamp(showing);
                lampSlots[showing * ReactionDesigns.MostLampsPerLook + k] = pooled;
                if (pooled < 0) continue;
                ref var lamp = ref set.Lamps[info.FirstLamp + k];
                lamps[pooled].color = lamp.Colour;
                lamps[pooled].range = lamp.Range;
            }
            Place(showing);
            // Whole parts from the first frame: the renderers are turned on only once they stand in their places.
            for (int k = 0; k < info.PartCount && k < mostParts; k++)
            {
                int pooled = slots[showing * mostParts + k];
                if (pooled >= 0) partShown[pooled].enabled = true;
            }
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

        // A free renderer; when none is free, the oldest other look gives up all of its own.
        private int TakePart(int forShowing)
        {
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < partUsed.Length; i++)
                    if (!partUsed[i])
                    {
                        partUsed[i] = true;
                        partsShown++;
                        return i;
                    }
                int oldest = Oldest(forShowing);
                if (oldest < 0) return -1;
                End(oldest);
            }
            return -1;
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
                    showings[i].Active = false;
                    showings[i].Body = null;
                }
            }
        }

        // Puts a look's parts and lamps where they are at its age, and lets go of those whose time is up. False when nothing is left.
        private bool Place(int showing)
        {
            ref var one = ref showings[showing];
            ref var info = ref set.Looks[one.Look];
            // The body is followed while it stands; once it is gone its parts stay where it was last seen.
            if (one.Body != null && one.Body.gameObject.activeInHierarchy) one.BodyAt = one.Body.position;
            float age = one.Age;
            float grow = Grow(age);
            bool any = false;
            for (int k = 0; k < info.PartCount && k < mostParts; k++)
            {
                int slot = showing * mostParts + k;
                int pooled = slots[slot];
                if (pooled < 0) continue;
                ref var part = ref set.Parts[info.FirstPart + k];
                float life = part.Kind == ReactionArtSet.Decal ? MarkSeconds : part.Place == ReactionArtSet.OnBody ? BodySeconds : GroundSeconds;
                if (age >= life)
                {
                    FreePart(slot);
                    continue;
                }
                any = true;
                float left = Left(age, life);
                var at = partAt[pooled];
                if (part.Kind == ReactionArtSet.Decal)
                {
                    // A mark spreads from the feet to its place as it pops, and lies a hair above the one before it.
                    Vector3 middle = one.Origin + one.Turn * new Vector3(part.Centre.x * grow, 0f, part.Centre.z * grow);
                    middle.y += MarkLift + k * 0.002f + showing * 0.0003f;
                    at.SetPositionAndRotation(middle, one.Turn);
                    at.localScale = new Vector3(part.Size.x * grow, 1f, part.Size.z * grow);
                    // Its block changes only while it is drawn on and while it fades (a frame's margin past the reveal).
                    if (age <= RevealSeconds + 0.1f || left < 1f)
                    {
                        block.Clear();
                        block.SetColor(ColourId, new Color(1f, 1f, 1f, left));
                        block.SetFloat(RevealId, Mathf.Clamp01(age / Mathf.Max(0.01f, RevealSeconds)));
                        partShown[pooled].SetPropertyBlock(block);
                    }
                    continue;
                }
                Vector3 anchor = part.Place == ReactionArtSet.OnBody ? one.BodyAt : one.Origin;
                at.SetPositionAndRotation(anchor, one.Turn);
                if (part.Kind == ReactionArtSet.Glow)
                {
                    at.localScale = Vector3.one * (grow * Mathf.Lerp(0.7f, 1f, left));
                    if (left < 1f)
                    {
                        block.Clear();
                        block.SetColor(ColourId, new Color(1f, 1f, 1f, left));
                        partShown[pooled].SetPropertyBlock(block);
                    }
                }
                else
                {
                    // Solids never fade: they close to nothing about the feet, as the element art's moments sink.
                    at.localScale = Vector3.one * Mathf.Max(0.001f, grow * left);
                }
            }
            for (int k = 0; k < info.LampCount && k < ReactionDesigns.MostLampsPerLook; k++)
            {
                int slot = showing * ReactionDesigns.MostLampsPerLook + k;
                int pooled = lampSlots[slot];
                if (pooled < 0) continue;
                if (age >= LampSeconds)
                {
                    FreeLamp(slot);
                    continue;
                }
                any = true;
                ref var lamp = ref set.Lamps[info.FirstLamp + k];
                float fade = 1f - age / LampSeconds;
                lamps[pooled].transform.position = one.BodyAt + one.Turn * lamp.Position;
                lamps[pooled].intensity = lamp.Intensity * fade * fade;
                if (!lamps[pooled].enabled) lamps[pooled].enabled = true;
            }
            return any;
        }

        // Out fast and a little past its size, then back: it lands as a shape, not as a growth.
        private float Grow(float age)
        {
            if (age >= PopSeconds) return 1f;
            float t = Mathf.Clamp01(age / Mathf.Max(0.01f, PopSeconds));
            return Mathf.Max(0.02f, 1f - (1f - t) * (1f - t) * (1f - t) + 0.12f * Mathf.Sin(t * Mathf.PI));
        }

        // Whole until the last share of its time, then down to nothing.
        private float Left(float age, float life)
        {
            float outFrom = life * (1f - Mathf.Clamp01(OutShare));
            if (age <= outFrom) return 1f;
            float t = Mathf.Clamp01((age - outFrom) / Mathf.Max(0.01f, life - outFrom));
            return 1f - t * t;
        }

        // ---- The 31 without a design: composed from the parents' element art.

        /// <summary>
        /// Draws a reaction with no designed look from its two parents' element art: each parent's own piece bursting up
        /// at the body's feet (the second a little smaller and turned away from the first), and one ground mark. False
        /// when the element art is off or not in the scene.
        /// </summary>
        public static bool Compose(int pair, Vector3 at, Vector3 away)
        {
            if (!ElementArt.Ready || pair < 0 || pair >= AfflictionPairs.Count) return false;
            ReactionDesigns.Composed(pair, out string first, out string second, out string mark, out int markElement);
            int a = ReactionDesigns.RingIndex(AfflictionPairs.First(pair)), b = ReactionDesigns.RingIndex(AfflictionPairs.Second(pair));
            float yaw = instance != null ? instance.Yaw(false, away) : 180f;
            Rise(first, a, at, yaw, 1f);
            Rise(second, b, at, yaw + 150f, 0.85f);
            if (mark != null)
                ElementArt.Floor(ElementArt.Piece(mark), at, yaw, ReactionDesigns.MarkScale(markElement) * 0.7f, ReactionDesigns.MarkScale(markElement), 1.6f, 0.15f, true, Color.white);
            ComposedCount++;
            return true;
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

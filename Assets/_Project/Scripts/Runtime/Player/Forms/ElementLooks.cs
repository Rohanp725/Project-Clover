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
        /// Carried Embers is 183 once every piece the recipe gives its struck body is on it; round 4's Wellspring, its 90
        /// small rain drops each with its own splash and its 24 streams, is 250). A look built at the cap has lost pieces, so none may reach
        /// it (ElementLooksTests).
        /// </summary>
        public const int MostParts = 256;
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
            /// <summary>
            /// The side of the look's own line, in its own frame, it is mirrored across (a strike drawn for a cut the other
            /// way round); zero when it is drawn as made.
            /// </summary>
            public Vector3 Mirror;
            /// <summary>A reaction's look (played by <see cref="PlayReaction"/>): Marked's sun-wheel waits while it plays near.</summary>
            public bool Reaction;
            /// <summary>How much taller the body is than the recipe's stand-in (<see cref="StandIn"/>): the look is fitted to it.</summary>
            public float Tall;
            /// <summary>The least seconds a look played once stays (a spell's look kept while what it laid lasts), and its stamp for a handle.</summary>
            public float Keep;
            public int Stamp;
            /// <summary>
            /// When it was last kept unseen (<see cref="KeepVeiled"/>: an affliction a reaction's look covers), whether it
            /// was unseen the frame before, and the seconds since it was seen again: it comes back in over
            /// <see cref="ComeIn"/> as it stood, never playing its arrival again.
            /// </summary>
            public float VeiledAt, Coming;
            public bool WasVeiled;
            /// <summary>The body's measured slot (<see cref="BodyAnchors"/>): its pieces on the body are drawn in to it; -1 for none.</summary>
            public int Girth;
            /// <summary>
            /// An enemy measured to its anchors: its pieces on the body are carried from the stand-in to them, and grown
            /// with it (<see cref="FitOnEnemy"/>). False for the player (its surge looks were drawn on its own model) and
            /// for a body that could not be measured.
            /// </summary>
            public bool Anchored;
        }

        /// <summary>Seconds a look kept unseen under a reaction's look takes to come back in once that look is over.</summary>
        public const float ComeIn = 0.35f;

        /// <summary>Seconds at the end of a flame's own window in which it dies: shrinking and thinning as it lifts off its base.</summary>
        public const float FlameDying = 0.25f;

        /// <summary>
        /// Reactions whose look is a whirl that blows itself out at its end (Thunder Gust's funnel, Carried Embers' column):
        /// over its last <see cref="BlowOut"/> seconds its bands widen, spin faster and lift as they are wiped away
        /// ("instead of blowing into nothing", 2026-11-20).
        /// </summary>
        private static readonly string[] Whirls = { "thunder_gust", "carried_embers" };
        public const float BlowOut = 0.7f, BlowWiden = 1f;

        /// <summary>
        /// Looks whose flame tongues all die together, within <see cref="TogetherAfter"/> of the first to go: Flashover's
        /// recipe staggers its eight violet cones' deaths over 0.4 s, and the last, on the shoulder, stood on alone ("one
        /// fire cone fades super late, so it looks strange", 2026-11-20).
        /// </summary>
        private static readonly string[] FlamesDieTogether = { "flashover" };
        public const float TogetherAfter = 0.1f;

        /// <summary>The reaction whose column lights only those it reaches, as it reaches them, by the game's rule (Reactions.Near).</summary>
        public const string LitByItsRun = "carried_embers";

        /// <summary>
        /// The height of the clay stand-in the Blender worker drew the reactions on (its head at about 1.75 m), and the
        /// most a look is grown for a taller body. The castle's soldiers stand 2 to 2.3 m and the Gate Captain near 3, so
        /// the looks drawn round the stand-in came out small and low on them (the audit of 2026-11-20).
        /// </summary>
        public const float StandIn = 1.75f, MostTall = 1.45f;

        /// <summary>The least share of its height a piece on a short body is brought down to (a body measured very short is likely half hidden).</summary>
        public const float LeastUp = 0.55f;

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

        /// <summary>
        /// How much a reaction's pieces that reach off the body (a column, an arc to another enemy) are grown on this body:
        /// its size by its anchors (<see cref="BodyAnchors.SizeOf"/>), never smaller and at most <see cref="MostTall"/>;
        /// by its bars' height for an enemy that could not be measured. Pieces on the body are fitted by its anchors.
        /// </summary>
        public static float TallOf(Transform body)
        {
            if (body == null) return 1f;
            var enemy = body.GetComponent<Clover.Gameplay.Enemy>();
            if (enemy == null) return 1f;
            if (BodyAnchors.TryAnchors(BodyAnchors.Slot(body), out var anchors)) return Mathf.Clamp(BodyAnchors.SizeOf(anchors), 1f, MostTall);
            return Mathf.Clamp(enemy.BodyHeight / StandIn, 1f, MostTall);
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

        /// <summary>
        /// Seconds a raised body is let down over at the end of the look that raised it, and the height above its feet a
        /// piece on it must stand at to ride up with it (lower, it is what rises from the floor and lifts it).
        /// </summary>
        public const float LetDown = 0.35f, RaisedFrom = 0.3f;

        /// <summary>
        /// Metres a reaction's look raises this body's model now (Anvil Stone's anvil, Frost Heave's ice under its feet),
        /// grown with the body as the look is; 0 when none does. The model only: the body that moves and is struck stays.
        /// </summary>
        public static float RaiseOf(Transform body)
        {
            if (instance == null || instance.showings == null || body == null) return 0f;
            float most = 0f;
            for (int i = 0; i < instance.showings.Length; i++)
            {
                ref var one = ref instance.showings[i];
                if (!one.Active || one.Ending || !ReferenceEquals(one.Body, body)) continue;
                ref var look = ref instance.set.Looks[one.Look];
                if (look.RaiseCount <= 0) continue;
                float raise = instance.set.Sample(look.RaiseFirst, look.RaiseCount, one.Age, Vector3.zero).y * Mathf.Max(1f, one.Tall);
                // Let down over the look's last moments, never dropped at once as it ends.
                raise *= Mathf.Clamp01((ElementLookSet.ShownFor(look) - one.Age) / LetDown);
                if (raise > most) most = raise;
            }
            return most;
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
        // Each look begun takes the next stamp, so a handle to one that has ended does not move the one in its place.
        private int stamps;
        private Vector3 towardCamera = Vector3.back, towardEye = Vector3.back;
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
            // Which parts are flame tongues (they die lifting off their base) and how tall each is; which looks are whirls
            // and when each blows out. Read once here: nothing is looked up by name while they play.
            flameParts = new bool[set.Parts.Length];
            flameHeight = new float[set.Parts.Length];
            for (int p = 0; p < set.Parts.Length; p++)
            {
                var mesh = set.Parts[p].Mesh;
                if (mesh == null) continue;
                flameParts[p] = IsFlame(mesh.name);
                flameHeight[p] = mesh.bounds.max.y;
            }
            onBody = new bool[set.Parts.Length];
            for (int p = 0; p < set.Parts.Length; p++) onBody[p] = OnBody(set, set.Parts[p]);
            blowFrom = new float[set.Looks.Length];
            blowTo = new float[set.Looks.Length];
            flamesGoneBy = new float[set.Looks.Length];
            for (int l = 0; l < set.Looks.Length; l++)
            {
                if (System.Array.IndexOf(FlamesDieTogether, set.Looks[l].Key) < 0) continue;
                float first = 9f;
                for (int k = 0; k < set.Looks[l].PartCount; k++)
                {
                    int p = set.Looks[l].FirstPart + k;
                    if (flameParts[p] && set.Parts[p].Hide < 9f) first = Mathf.Min(first, set.Parts[p].Hide);
                }
                if (first < 9f) flamesGoneBy[l] = first + TogetherAfter;
            }
            // Carried Embers' column runs as far as its rule carries the fire (Reactions.CarriedEmbersRun): the recipe's
            // column stopped at 2.5 m while its bodies stood 4 to 6 m down the charred lane, so the fire lit what it never
            // reached. Every part travelling with the column (its path ending level, well out from the body) runs on to it.
            runStretch = new float[set.Parts.Length];
            int carried = set.Find(LitByItsRun);
            if (carried >= 0)
            {
                ref var column = ref set.Looks[carried];
                for (int k = 0; k < column.PartCount; k++)
                {
                    int p = column.FirstPart + k;
                    ref var part = ref set.Parts[p];
                    if (part.PathCount <= 0) continue;
                    Vector3 end = set.Keys[part.PathFirst + part.PathCount - 1].Value;
                    float travel = new Vector2(end.x, end.z).magnitude;
                    if (Mathf.Abs(end.y) < 0.1f && travel > 1.5f) runStretch[p] = Clover.Player.Trees.Reactions.CarriedEmbersRun / travel;
                }
            }
            for (int l = 0; l < set.Looks.Length; l++)
            {
                if (System.Array.IndexOf(Whirls, set.Looks[l].Key) < 0) continue;
                float end = 0f;
                for (int k = 0; k < set.Looks[l].PartCount; k++)
                {
                    ref var part = ref set.Parts[set.Looks[l].FirstPart + k];
                    if (part.Kind == ElementLookSet.Ribbon && part.Hide < 9f) end = Mathf.Max(end, part.Hide);
                }
                if (end <= BlowOut) continue;
                blowFrom[l] = end - BlowOut;
                blowTo[l] = end;
            }
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

        // The kit's flame tongue and its core ("vfxe2_flame", "vfxe2_flame_core"), not a lick (which rises off on its own keys).
        private static bool IsFlame(string meshName)
        {
            int at = meshName.IndexOf("vfxe2_flame", System.StringComparison.Ordinal);
            if (at < 0) return false;
            int after = at + "vfxe2_flame".Length;
            if (after >= meshName.Length || meshName[after] == '~') return true;
            return string.CompareOrdinal(meshName, after, "_core", 0, 5) == 0;
        }

        // A part that rides on the body and stays within reach of it (no path carrying it away, no ribbon reaching another
        // body, nothing lying flat on the floor).
        private static bool OnBody(ElementLookSet set, in ElementLookSet.Part part)
        {
            if (!part.Follows || part.Kind == ElementLookSet.Flat) return false;
            if (part.PathCount > 0)
            {
                Vector3 moved = set.Keys[part.PathFirst + part.PathCount - 1].Value - set.Keys[part.PathFirst].Value;
                if (new Vector2(moved.x, moved.z).magnitude > OnBodyDrift) return false;
            }
            if (part.Kind == ElementLookSet.Ribbon)
            {
                if (part.Mesh == null) return false;
                var bounds = part.Mesh.bounds;
                float reach = Mathf.Max(Mathf.Max(Mathf.Abs(bounds.min.x), Mathf.Abs(bounds.max.x)), Mathf.Max(Mathf.Abs(bounds.min.z), Mathf.Abs(bounds.max.z)));
                return reach <= NearBody;
            }
            return new Vector2(part.Position.x, part.Position.z).magnitude <= NearBody;
        }

        /// <summary>Metres sideways a part's path may carry it and still be a part on the body.</summary>
        public const float OnBodyDrift = 0.4f;

        private bool[] flameParts;
        // Parts laid on the body itself (on or round it, staying there), drawn in to the real body (BodyAnchors).
        private bool[] onBody;
        private float[] flameHeight, blowFrom, blowTo, flamesGoneBy, runStretch;

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

        /// <summary>Bedrock's stone plate and the stone it is drawn in, from the built looks; false when the scene has none.</summary>
        public static bool TryArmourPlate(out Mesh plate, out Material stone)
        {
            plate = Ready ? instance.set.ArmourPlate : null;
            stone = Ready ? instance.set.ArmourMaterial : null;
            return plate != null && stone != null;
        }

        /// <summary>An element's charged-blade sheath from the built looks; false when the scene has none for it.</summary>
        public static bool TrySheath(int element, out ElementLookSet.BladeSheath sheath)
        {
            sheath = default;
            if (!Ready) return false;
            int found = instance.set.FindSheath(element);
            if (found < 0) return false;
            sheath = instance.set.Sheaths[found];
            return sheath.Mesh != null && sheath.Material != null;
        }

        /// <summary>True while a reaction's look that keeps its body from glowing frozen plays on it (<see cref="ElementLookSet.Look.Thawed"/>).</summary>
        /// <summary>Seconds a thawed reaction keeps Chill's frost off the body it froze.</summary>
        public const float ThawSeconds = 6f;

        public static bool Thawed(Transform body)
        {
            if (Hides(body, (int)Clover.Gameplay.AfflictionKind.Chill)) return true;
            if (instance == null || instance.showings == null || instance.set == null || body == null) return false;
            for (int i = 0; i < instance.showings.Length; i++)
            {
                ref var one = ref instance.showings[i];
                if (one.Active && ReferenceEquals(one.Body, body) && instance.set.Looks[one.Look].Thawed) return true;
            }
            return false;
        }

        /// <summary>True while a look is playing (not yet going) on a body: a spell's look on the one it struck is not begun again on each bite.</summary>
        public static bool IsPlaying(int look, Transform body)
        {
            if (instance == null || instance.showings == null || body == null) return false;
            for (int i = 0; i < instance.showings.Length; i++)
            {
                ref var one = ref instance.showings[i];
                if (one.Active && !one.Ending && one.Look == look && ReferenceEquals(one.Body, body)) return true;
            }
            return false;
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
            return instance.KeepOn(look, body, false);
        }

        /// <summary>
        /// Keeps a look on a body this frame without drawing it: an affliction still carried under a reaction's look that
        /// covers it. It goes on ageing, so when the reaction's look is over it is simply there, coming back in softly as
        /// it stood, not cast again from its start (the owner, 2026-11-20: "when it ends it insta casts another set of
        /// both afflictions").
        /// </summary>
        public static bool KeepVeiled(int look, Transform body)
        {
            if (!Ready || body == null || look < 0 || look >= instance.set.Looks.Length) return false;
            return instance.KeepOn(look, body, true);
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
        public static bool Play(int look, Transform body, float yaw) => Play(look, body, yaw, false);

        /// <summary>The same, mirrored across the look's own line when <paramref name="mirror"/> (<see cref="MirrorOf"/>).</summary>
        public static bool Play(int look, Transform body, float yaw, bool mirror)
        {
            if (!Ready || body == null || look < 0 || look >= instance.set.Looks.Length) return false;
            int showing = instance.Begin(look, body);
            if (showing < 0) return false;
            instance.showings[showing].Once = true;
            instance.showings[showing].Turn = Quaternion.Euler(0f, yaw - instance.set.Looks[look].FacingYaw, 0f);
            instance.showings[showing].Mirror = mirror ? MirrorOf(instance.set.Looks[look].FacingYaw) : Vector3.zero;
            return true;
        }

        /// <summary>
        /// The side a look is mirrored across, in its own frame: square to the way its recipe's player faced, so the look
        /// is reflected about the vertical plane down that line (a cut from the right drawn as one from the left).
        /// </summary>
        public static Vector3 MirrorOf(float facingYaw)
        {
            float radians = facingYaw * Mathf.Deg2Rad;
            return new Vector3(Mathf.Cos(radians), 0f, -Mathf.Sin(radians));
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
            // A thawed reaction (Winter Seed's ice flower) keeps Chill's frost and frozen glow off the body for as long as
            // its freeze lasts, past its own look.
            if (instance.set.Looks[main].Thawed) spends |= 1 << (int)Clover.Gameplay.AfflictionKind.Chill;
            if (spends != 0) Hide(body, spends, Mathf.Max(ElementLookSet.ShownFor(instance.set.Looks[main]), instance.set.Looks[main].Thawed ? ThawSeconds : 0f));
            away.y = 0f;
            float yaw = away.sqrMagnitude > 0.0001f ? Mathf.Atan2(away.x, away.z) * Mathf.Rad2Deg : instance.set.Looks[main].FacingYaw;
            PlayOnBody(main, body, yaw);
            // A blast the reaction throws on out of the body (Red Hot's fire through the enemy, "slug@+").
            int blast = instance.set.FindVariant(slug, '+');
            if (blast >= 0) PlayOnBody(blast, body, yaw);
            var turn = Quaternion.Euler(0f, yaw - instance.set.Looks[main].FacingYaw, 0f);
            if (player != null)
            {
                int own = instance.set.FindVariant(slug, 'p');
                if (own >= 0) Play(own, player, yaw);
            }
            int taken = 0;
            Taken[taken++] = body;
            // A column that lights whoever it reaches (Carried Embers) burns them in their own fire when it reaches them,
            // through the game's rule: its recipe's flames on the bodies it stood are not put on whoever stands near
            // those spots at their set times ("it doesnt reach a enemy but they get ignited anyways", 2026-11-20).
            bool lightsWhomItReaches = string.Equals(slug, LitByItsRun, System.StringComparison.Ordinal);
            for (int n = 1; n < 8 && !lightsWhomItReaches; n++)
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
        public static bool PlayAt(int look, Vector3 at, float yaw, float size = 1f) => Held(look, at, yaw, size, 0f) >= 0;

        /// <summary>The same, mirrored across the look's own line when <paramref name="mirror"/> (<see cref="MirrorOf"/>).</summary>
        public static bool PlayAt(int look, Vector3 at, float yaw, float size, bool mirror)
        {
            if (!PlayAtShowing(look, at, yaw, size, out int showing)) return false;
            if (mirror) instance.showings[showing].Mirror = MirrorOf(instance.set.Looks[look].FacingYaw);
            return true;
        }

        /// <summary>
        /// Plays a look once where it stands, as <see cref="PlayAt"/>, kept at least <paramref name="keep"/> seconds (its
        /// held pieces standing, its flames and clouds moving on), and hands back a handle to move it by
        /// (<see cref="MoveHeld"/>: Miasma's cloud creeps after the nearest enemy); -1 when it cannot be shown.
        /// </summary>
        public static int Held(int look, Vector3 at, float yaw, float size, float keep)
        {
            if (!PlayAtShowing(look, at, yaw, size, out int showing)) return -1;
            instance.showings[showing].Keep = keep;
            return showing + MostShowings * instance.showings[showing].Stamp;
        }

        /// <summary>Moves a look played by <see cref="Held"/> to stand at <paramref name="at"/>; nothing once it has ended.</summary>
        public static void MoveHeld(int handle, Vector3 at)
        {
            if (instance == null || instance.showings == null || handle < 0) return;
            int showing = handle % MostShowings;
            ref var one = ref instance.showings[showing];
            if (one.Active && one.Fixed && one.Stamp == handle / MostShowings) one.Ground = at;
        }

        /// <summary>
        /// Ends a look played by <see cref="Held"/> now: it goes as any look goes, over a quarter of a second (the shot's
        /// trace when its arrow stops in someone short of where the trace was drawn to). Nothing once it has ended.
        /// </summary>
        public static void EndHeld(int handle)
        {
            if (instance == null || instance.showings == null || handle < 0) return;
            int showing = handle % MostShowings;
            ref var one = ref instance.showings[showing];
            if (one.Active && one.Fixed && one.Stamp == handle / MostShowings) one.Ending = true;
        }

        private static bool PlayAtShowing(int look, Vector3 at, float yaw, float size, out int showing)
        {
            showing = -1;
            if (!Ready || look < 0 || look >= instance.set.Looks.Length) return false;
            showing = instance.Begin(look, null);
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
            bool found = false;
            for (int i = 0; i < instance.showings.Length; i++)
            {
                ref var one = ref instance.showings[i];
                if (!one.Active || !ReferenceEquals(one.Body, body) || Veiled(one)) continue;
                ref var look = ref instance.set.Looks[one.Look];
                if (look.Shell == ElementLookSet.NoShell) continue;
                if (wanted != ElementLookSet.NoShell && look.Shell != wanted && !(wanted == ElementLookSet.Ice && ElementLookSet.IsIceShell(look.Shell))) continue;
                float has = (one.Ending ? Mathf.Clamp01(1f - one.Gone / Going) : 1f) * Mathf.Clamp01(one.Coming / ComeIn);
                // A shell broken off before the look ends (Rime's crust at the heavy) is gone in a fifth of a second.
                if (look.ShellUntil > 0f && one.Age > look.ShellUntil) has *= Mathf.Clamp01(1f - (one.Age - look.ShellUntil) / 0.2f);
                // The strongest shell on it wins: a reaction's ice going as Chill's own comes back in hands over without
                // the body thawing between (Supercooled's film of 2026-11-20 thawed for a tenth of a second and froze again).
                if (found && has <= amount) continue;
                found = true;
                shell = look.Shell;
                reveal = ElementLookSet.ShellReveal(look, one.Age);
                amount = has;
            }
            return found;
        }

        private bool KeepOn(int look, Transform body, bool veiled)
        {
            for (int i = 0; i < showings.Length; i++)
            {
                ref var one = ref showings[i];
                if (!one.Active || one.Look != look || !ReferenceEquals(one.Body, body)) continue;
                one.KeptAt = Time.time;
                if (veiled) one.VeiledAt = Time.time;
                // Kept again while it was going: it comes back as it was.
                one.Ending = false;
                one.Gone = 0f;
                return true;
            }
            int begun = Begin(look, body);
            if (begun < 0) return false;
            if (veiled)
            {
                showings[begun].VeiledAt = Time.time;
                showings[begun].WasVeiled = true;
            }
            return true;
        }

        // Kept unseen at its last keeping (a reaction's look covers it).
        private static bool Veiled(in Showing one) => one.VeiledAt >= 0f && one.VeiledAt >= one.KeptAt;

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
                Turn = Quaternion.identity, Size = 1f, Tall = TallOf(body), Stamp = ++stamps, VeiledAt = -1f, Coming = ComeIn,
                Girth = BodyAnchors.Slot(body),
            };
            // An enemy's pieces go by its anchors; the player's surge looks were drawn on its own model and keep their fit.
            showings[free].Anchored = !set.Looks[look].OnPlayer && body != null && body.GetComponent<PlayerController>() == null
                && BodyAnchors.TryAnchors(showings[free].Girth, out _);
            if (set.Looks[look].FacesCamera)
            {
                // The recipe's camera stood on its player's side: that way is turned onto the game camera's.
                Vector3 seen = -towardCamera;
                float cameraYaw = new Vector2(seen.x, seen.z).sqrMagnitude > 0.0001f ? Mathf.Atan2(seen.x, seen.z) * Mathf.Rad2Deg : 0f;
                showings[free].Turn = Quaternion.Euler(0f, cameraYaw - set.Looks[look].FacingYaw, 0f);
            }
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
            // The way to the camera along the floor, for the parts drawn in front of what stands between (Part.Facing).
            var view = Camera.main;
            if (view != null)
            {
                Vector3 back = -view.transform.forward;
                towardEye = back.normalized;
                back.y = 0f;
                towardCamera = back.sqrMagnitude > 0.0001f ? back.normalized : Vector3.back;
            }
            for (int i = 0; i < showings.Length; i++)
            {
                ref var one = ref showings[i];
                if (!one.Active) continue;
                ref var look = ref set.Looks[one.Look];
                bool bodyGone = !one.Fixed && (one.Body == null || !one.Body.gameObject.activeInHierarchy);
                one.Age += deltaTime;
                if (!one.Ending)
                {
                    bool over = one.Once ? one.Age >= Mathf.Max(ElementLookSet.ShownFor(look), one.Keep) : now - one.KeptAt > Linger;
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
                if (Veiled(one))
                {
                    // Kept unseen under a reaction's look: it ages on and draws nothing.
                    one.WasVeiled = true;
                    for (int k = 0; k < look.PartCount && k < MostParts; k++) Free(i * MostParts + k);
                    continue;
                }
                if (one.WasVeiled)
                {
                    one.WasVeiled = false;
                    one.Coming = 0f;
                }
                if (one.Coming < ComeIn) one.Coming += deltaTime;
                Vector3 feet = one.Fixed || bodyGone ? one.Ground : one.Body.position;
                // Its body burns in the real flame field while it plays (Smoulder's black fire, Steam's lower flame).
                if (look.BodyFlame != ElementLookSet.NoFlame && !one.Ending && !bodyGone && one.Age >= look.BodyFlameFrom)
                    BodyFlames.Keep(one.Body, BodyFlameDepth, look.BodyFlame == ElementLookSet.BlackFlame);
                float shrink = one.Ending ? Mathf.Max(0.001f, 1f - one.Gone / Going) : 1f;
                shrink *= Mathf.Max(0.001f, Mathf.Clamp01(one.Coming / ComeIn));
                // A whirl blowing itself out: how far through its last stretch it is.
                float blow = blowTo[one.Look] > 0f ? Mathf.Clamp01((one.Age - blowFrom[one.Look]) / (blowTo[one.Look] - blowFrom[one.Look])) : 0f;
                // What is on a body that a reaction raises (Anvil Stone's dagger in it) rides up with its model.
                float raised = one.Fixed || bodyGone ? 0f : RaiseOf(one.Body);
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
                    // (Not what rises under it from the floor: the anvil and the ice are what lift it.)
                    // (An enemy placed by its joints already rides up with them.)
                    Vector3 anchor = !part.Follows ? one.Ground : raised > 0f && !one.Anchored && onBody[look.FirstPart + k] && part.Position.y > RaisedFrom ? feet + Vector3.up * raised : feet;
                    posingBody = one.Body;
                    posingFacing = look.FacingYaw;
                    Pose(part, look.FirstPart + k, flamesGoneBy[one.Look], t, angleTime, anchor, one.Turn, one.Size, one.Mirror, one.Tall, one.Girth, one.Anchored, shrink, blow, slot, at[pooled]);
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

        private void Pose(in ElementLookSet.Part part, int index, float goneBy, float t, float angleTime, Vector3 anchor, Quaternion turn, float size, Vector3 mirror, float tall, int girth, bool anchored, float shrink, float blow, int slot, Transform frame)
        {
            set.LocalPose(part, t, angleTime, slot, out Vector3 position, out Quaternion rotation, out Vector3 scale);
            if (runStretch[index] > 0f)
            {
                // Carried on along its run to the rule's distance, as far as its path has taken it so far.
                Vector3 along = set.Sample(part.PathFirst, part.PathCount, t, Vector3.zero) * (runStretch[index] - 1f);
                position.x += along.x;
                position.z += along.z;
            }
            if (flameParts[index]) Die(part, index, goneBy, t, shrink, ref position, ref scale);
            if (part.Bone > 0 && OnBone(part, position, rotation, scale, shrink * size, frame)) return;
            if (blow > 0f && part.Kind != ElementLookSet.Flat)
            {
                // Blowing out: the band widens about its own axis, spins on faster and lifts as it is wiped away.
                float eased = blow * blow;
                rotation = Quaternion.AngleAxis(-420f * eased, Vector3.up) * rotation;
                scale.x *= 1f + BlowWiden * blow;
                scale.z *= 1f + BlowWiden * blow;
                scale.y *= 1f + 0.35f * blow;
                position.y += 0.7f * eased;
            }
            Vector3 stretch = Vector3.one, lean = Vector3.zero;
            if (onBody[index] && girth >= 0)
            {
                if (anchored) FitOnEnemy(part, girth, turn, mirror, ref position, out stretch, out lean);
                else FitOnBody(part, tall, girth, ref position, out stretch);
            }
            // What lies flat on the floor keeps the size it was drawn at: its size is the reach of what the game does
            // there (the owner, 2026-11-21: "scale on-body, not areas").
            else if (tall > 1.001f && part.Kind != ElementLookSet.Flat) Fit(part, tall, ref position, out stretch);
            if (part.Kind == ElementLookSet.Flat) position.y += FlatLift;
            if (mirror.sqrMagnitude > 0.5f)
            {
                // Reflected across the plane down the look's line: its place, and its turn as a turn and a flip of its own
                // x (the renderer draws a flipped mesh's faces the right way out).
                position -= 2f * Vector3.Dot(position, mirror) * mirror;
                Vector3 ahead = rotation * Vector3.forward, up = rotation * Vector3.up;
                ahead -= 2f * Vector3.Dot(ahead, mirror) * mirror;
                up -= 2f * Vector3.Dot(up, mirror) * mirror;
                rotation = Quaternion.LookRotation(ahead, up);
                stretch.x = -stretch.x;
            }
            Vector3 placed = anchor + lean + turn * (position * size);
            if (part.Facing != 0f) placed += (part.FacingAlongView ? towardEye : towardCamera) * part.Facing;
            frame.SetPositionAndRotation(placed, turn * rotation);
            frame.localScale = Vector3.Scale(scale, stretch) * (shrink * size);
        }

        // The body being posed and its look's way (set before each Pose), and the player whose bones carry bound parts.
        private Transform posingBody, boneHolder;
        private float posingFacing;
        private Clover.Player.PlayerOutfit boneOutfit;

        // A part carried by one of the player's bones (round 4: Kindle, Overdrive, Fever): set on the real bone at the share
        // and offset it had on the stand-in's, its offset turned as the bone is turned from the stand-in's way, its own keyed
        // motion added in the player's frame; a ribbon (an arc along a limb) turns with the bone too. False when the body
        // has no such bone, and the part is placed as before.
        private bool OnBone(in ElementLookSet.Part part, Vector3 position, Quaternion rotation, Vector3 scale, float sized, Transform frame)
        {
            if (posingBody == null) return false;
            if (!ReferenceEquals(boneHolder, posingBody))
            {
                boneHolder = posingBody;
                boneOutfit = posingBody.GetComponent<Clover.Player.PlayerOutfit>();
            }
            if (boneOutfit == null || !PlayerBones.Resolve(posingBody, boneOutfit.Body)) return false;
            int index = part.Bone - 1;
            if (!PlayerBones.Segment(index, out var head, out var tail)) return false;
            var playerTurn = Quaternion.Euler(0f, posingBody.eulerAngles.y, 0f);
            var lookToWorld = playerTurn * Quaternion.Euler(0f, -posingFacing, 0f);
            var stand = PlayerBones.StandIn[index];
            var bend = Quaternion.FromToRotation(playerTurn * (stand.tail - stand.head), tail - head);
            Vector3 target = Vector3.Lerp(head, tail, part.BoneShare) + bend * (playerTurn * part.BoneOffset) + lookToWorld * (position - part.Position);
            bool ribbon = part.Kind == ElementLookSet.Ribbon && part.Mesh != null;
            Quaternion turned = ribbon ? bend * lookToWorld * rotation : lookToWorld * rotation;
            // A ribbon's points lie about the player's feet: it is set so its middle lands where it is carried.
            if (ribbon) target -= turned * Vector3.Scale(part.Mesh.bounds.center, scale * sized);
            frame.SetPositionAndRotation(target, turned);
            frame.localScale = scale * sized;
            return true;
        }

        // A flame tongue dying: over the last moments of its own window, and as its look goes, it shrinks and thins as it
        // lifts off its base, never squashed flat into the floor or tipped over (the recipe's own death keys squash it to
        // a twentieth of its height, which read as the flame turning down, 2026-11-20).
        private void Die(in ElementLookSet.Part part, int index, float goneBy, float t, float shrink, ref Vector3 position, ref Vector3 scale)
        {
            // When it is gone: its own window's end, or sooner where its look's flames all die together.
            float hide = goneBy > 0f && part.Hide < 9f ? Mathf.Min(part.Hide, goneBy) : part.Hide;
            // How much of it is left by its own keys (1 alive, 0 gone), and by its look going or coming in (shrink, which
            // Pose lays on every part alike after this).
            float own = 1f;
            if (hide < 9f && t > hide - FlameDying)
            {
                own = Mathf.Clamp01((hide - t) / FlameDying);
                // Its size just before it began to die, not the squash the recipe keys into its end.
                scale = Vector3.Scale(part.Scale, set.Sample(part.ScaleFirst, part.ScaleCount, hide - FlameDying, Vector3.one));
            }
            float life = own * shrink;
            if (life >= 0.999f) return;
            float height = flameHeight[index] * scale.y;
            scale.x *= own;
            scale.z *= own;
            scale.y *= Mathf.Sqrt(own);
            position.y += (1f - life) * 0.5f * height;
        }

        /// <summary>
        /// A part laid on an enemy's body, carried from the clay stand-in to the real body by their anchors
        /// (<see cref="BodyAnchors"/>): what sat at the stand-in's waist, chest or head sits at the body's; it stands out
        /// from the upright as far past the body's own surface that way (ahead, behind or to the side, as the world will
        /// see it once the look is turned and mirrored) as it stood past the stand-in's; and it is grown with the body,
        /// 0.6x to 1.8x. A ribbon drawn round the upright (a coil, the veins) is drawn round the body's own girth.
        /// </summary>
        public static void FitOnEnemy(in ElementLookSet.Part part, int slot, Quaternion turn, Vector3 mirror, ref Vector3 position, out Vector3 stretch, out Vector3 lean)
        {
            BodyAnchors.TryAnchors(slot, out var bound);
            // Sized by the body as built; placed by its joints as they stand now, so what sits on a crouching Knife
            // Acolyte's chest sits on it and not where its chest would be standing up.
            BodyAnchors.TryPosed(slot, out var posed);
            var body = posed.Anchors.Crown > 0f ? posed.Anchors : bound;
            float size = BodyAnchors.SizeOf(bound) * BodyAnchors.FoldOf(bound, body);
            bool ribbon = part.Kind == ElementLookSet.Ribbon && part.Mesh != null;
            // A ribbon's own points lie round the upright: it is measured where its middle is.
            float height = ribbon ? part.Mesh.bounds.center.y : position.y;
            float placed = BodyAnchors.HeightOn(body, height);
            float up = height > 0.05f ? placed / height : size;
            Vector3 seen = new Vector3(position.x, 0f, position.z);
            if (mirror.sqrMagnitude > 0.5f) seen -= 2f * Vector3.Dot(seen, mirror) * mirror;
            seen = turn * seen;
            // The stand-in's own bands, by its own height (as the player's fit reads them).
            float standIn = BodyAnchors.StandInReach(Mathf.Clamp01(height / StandIn));
            float reach = BodyAnchors.ReachAt(slot, placed, body.Crown, ribbon ? Vector3.zero : seen);
            lean = BodyAnchors.ShiftAt(posed, height);
            float ratio = Mathf.Clamp(reach / standIn, BodyAnchors.LeastReach, BodyAnchors.MostReach);
            position.x *= ratio;
            position.z *= ratio;
            if (ribbon)
            {
                position.y *= up;
                stretch = new Vector3(ratio, up, ratio);
                return;
            }
            position.y = placed;
            stretch = new Vector3(size, size, size);
        }

        // A part laid on the player's body, carried from the clay stand-in to the real body: up it by its height, and in toward its
        // upright by how much slimmer (or wider) the real body is than the stand-in there, so what was laid on the
        // stand-in's surface lies on the real body's (Chill's crystals stood 0.15 m off a castle soldier in the air).
        private static void FitOnBody(in ElementLookSet.Part part, float tall, int girth, ref Vector3 position, out Vector3 stretch)
        {
            // A ribbon's own points lie round the upright: it is measured where its middle is.
            float height = part.Kind == ElementLookSet.Ribbon && part.Mesh != null ? part.Mesh.bounds.center.y : position.y;
            float ratio = BodyAnchors.Ratio(girth, height, StandIn);
            // Up a body shorter than the stand-in by its own height (the look is never drawn smaller, but what sits on the
            // body sits at its chest): Anvil Stone's dagger stood over a Knife Acolyte's head (its film of 2026-11-20).
            float measured = BodyAnchors.Height(girth);
            position.y *= measured > 0f && measured < StandIn ? Mathf.Max(LeastUp, measured / StandIn) : tall;
            position.x *= ratio;
            position.z *= ratio;
            stretch = new Vector3(tall, tall, tall);
            // A ribbon is drawn round the upright itself (a coil, a crust): it is drawn in with the body.
            if (part.Kind == ElementLookSet.Ribbon) stretch = new Vector3(ratio * tall, tall, ratio * tall);
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

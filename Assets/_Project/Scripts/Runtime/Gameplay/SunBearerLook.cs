using Clover.Core;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The Sun-Bearer's effects that are not particles (fire_boss_visuals.md, section 2.3): the light the sun throws on
    /// the court, breathing on two waves; the streak the falcon's stoop tears down through the air; and the shock ring
    /// that runs out through the air where it lands. <see cref="SunBearer"/> says what happens; this draws it. Every piece
    /// is made by the builder and only moved, scaled and tinted here, through one property block. Since the owner's
    /// review (2026-11-17) all of it is fire, not light: the sun is a burning ball with flames licking off its rim,
    /// smoke rolling up from it and embers falling, and the colours here are multipliers on the fire material's own.
    /// It runs after the fight has placed the sun for the frame, so the rim is never a frame behind the ball.
    /// </summary>
    [DefaultExecutionOrder(100)]
    public class SunBearerLook : MonoBehaviour
    {
        [SerializeField] private GameObject burningSun;
        [SerializeField] private Transform corona;
        [SerializeField] private GameObject dullSun;
        /// <summary>How far behind the ball's middle, away from the eye, the burning rim hangs, in the ball's own metres.</summary>
        public float CoronaBehind = 0.7f;
        /// <summary>Seconds between puffs of smoke off the burning ball, and between embers falling from it.</summary>
        public float SmokeEvery = 0.12f, EmberEvery = 0.22f;
        public Color SmokeColour = new Color(0.23f, 0.2f, 0.19f, 0.8f);
        private float untilSmoke, untilEmber;
        [SerializeField] private Light sunLight;
        [SerializeField] private Transform sun;
        [SerializeField] private Renderer[] diveStreak = new Renderer[0];
        [SerializeField] private MeshFilter shockRing;
        [SerializeField] private TrailRenderer sunTrail;
        [SerializeField] private Renderer[] stars = new Renderer[0];
        [SerializeField] private UnityEngine.Rendering.Volume blazeLook;
        [SerializeField] private Renderer[] fallers = new Renderer[0];
        [ColorUsage(true, true)] public Color SparkColour = new Color(1f, 1f, 1f, 1f);
        [ColorUsage(true, true)] public Color FeatherColour = new Color(1.1f, 0.95f, 0.8f, 1f);
        [ColorUsage(true, true)] public Color MeteorColour = new Color(1.2f, 1f, 0.9f, 1f);
        // The ridge over the burrowing scarab: a heave of paving, slabs of it lifted round it, and cracks left behind.
        [SerializeField] private Renderer mound;
        [SerializeField] private Renderer[] slabs = new Renderer[0];
        [SerializeField] private Renderer[] cracks = new Renderer[0];
        /// <summary>The cracks: dark in plain paving, glowing ember red where the sun went under; how long they last, how far apart they are laid.</summary>
        public Color CrackColour = new Color(0.07f, 0.05f, 0.04f, 1f);
        [ColorUsage(true, true)] public Color HotCrackColour = new Color(2.2f, 0.55f, 0.08f, 0.95f);
        public float CrackSeconds = 3.5f, CrackEvery = 0.8f, CrackSize = 3.2f;
        // The Flares' arcs of flame and the Ember Gust's wall of burning wind.
        [SerializeField] private LineRenderer[] arcs = new LineRenderer[0];
        /// <summary>The Ember Gust's wall as fire: a row of tall flames across the band, leaning on, and a lower one behind.</summary>
        [SerializeField] private ScorchedGround gustFire;
        public float ArcHeight = 7f, GustHeight = 2.6f;
        private readonly Vector3[] arcPoints = new Vector3[24];
        private float[] crackAge = new float[0];
        private bool[] crackHot = new bool[0];
        private float[] crackSize = new float[0];
        private int nextCrack;
        private Vector3 lastCrack;
        private bool ridgeOn;

        [ColorUsage(true, true)] public Color DiveColour = new Color(1.1f, 1f, 1f, 1f);
        [ColorUsage(true, true)] public Color ShockColour = new Color(1f, 1f, 1f, 1f);
        public float DiveWidth = 3.4f, DiveFade = 0.3f;
        public float ShockHeight = 1f, ShockFrom = 1f, ShockTo = 7f, ShockSeconds = 0.22f;
        /// <summary>How much the sun's light breathes either way, on waves of these speeds.</summary>
        public float Flicker = 0.08f, FlickerSlow = 7f, FlickerFast = 13f;
        /// <summary>How fast the light follows what the fight asks of it, in intensity a second.</summary>
        public float LightPace = 12f;
        /// <summary>The fight asks for the plan's intensities (3 held, 4.5 carried, 6 at the Zenith); the toon shader's
        /// banded lamps need this many times that before the warm pool on the floor is wider than the sun.</summary>
        public float Strength = 4f;
        /// <summary>How far above the sun's middle the light hangs, so its pool spreads round it rather than under it.</summary>
        public float LightAbove = 1.2f;
        /// <summary>How far out the stun stars circle.</summary>
        public float StarRadius = 1.4f;

        private static readonly int ColourId = Shader.PropertyToID("_Color");
        private const int RingSides = 48;
        // The ring's breadth as a share of its radius: thin as it leaves the floor, broader as it spreads and fades.
        private const float RingBreadth = 0.09f;

        private MaterialPropertyBlock block;
        private Renderer shockRenderer;
        private Mesh ringMesh;
        private float lightWanted, lightNow;
        private Vector3 diveTop, diveFoot;
        private float diveDrawn = -1f, diveFading = -1f;
        private Vector3 shockAt;
        private float shockAge = -1f;
        private bool stunned;
        private Vector3 starsOver;
        private float starTurn;
        private float blazeWanted, blazeNow;
        /// <summary>How fast the court whitens and comes back, in weight a second.</summary>
        public float BlazePace = 1f;
        private Transform eye;

        /// <summary>The things that fall: the scarab's burning sparks and the falcon's burning feathers, one per falling mark.</summary>
        public void ConfigureFallers(Renderer[] pool) => fallers = pool;

        /// <summary>The ridge's pieces, the cracks it leaves, the Flares' arcs and the Gust's wall.</summary>
        public void ConfigureGround(Renderer heave, Renderer[] lifted, Renderer[] crackPool, LineRenderer[] flareArcs, ScorchedGround windFire)
        {
            gustFire = windFire;
            mound = heave;
            slabs = lifted;
            cracks = crackPool;
            arcs = flareArcs;
        }

        /// <summary>
        /// One falling thing, this far through its fall (0 to 1), from where it left the body to the middle of its mark:
        /// <paramref name="kind"/> 0 a spark from under the scarab's cases, 1 a feather, 2 a burning piece of the sun.
        /// </summary>
        public void Falling(int index, Vector3 from, Vector3 to, float t, int kind)
        {
            if (index < 0 || index >= fallers.Length || fallers[index] == null) return;
            var shown = fallers[index];
            if (!shown.enabled) shown.enabled = true;
            Vector3 at;
            float spin;
            Vector3 size;
            bool feather = kind == 1;
            if (kind == 2)
            {
                // Torn off the sun and hurled high, it comes down steep onto its mark, a lump of fire.
                at = Vector3.Lerp(from, to, t) + Vector3.up * (4f * 6f * t * (1f - t));
                spin = t * 540f;
                size = new Vector3(1.5f, 2.1f, 1f);
                Quaternion turned = eye != null ? eye.rotation : Quaternion.identity;
                shown.transform.SetPositionAndRotation(at, turned * Quaternion.Euler(0f, 0f, spin));
                shown.transform.localScale = size;
                block.SetColor(ColourId, MeteorColour);
                shown.SetPropertyBlock(block);
                return;
            }
            if (feather)
            {
                // A feather comes down swaying, and lands exactly as its warning ends.
                at = Vector3.Lerp(from, to, t);
                float sway = Mathf.Sin(t * Mathf.PI * 4f) * (1f - t);
                at += new Vector3(sway * 0.6f, 0f, sway * 0.3f);
                spin = sway * 50f;
                size = new Vector3(0.75f, 1.9f, 1f);
            }
            else
            {
                // A spark is thrown up from under the wing cases and arcs down onto its mark.
                at = Vector3.Lerp(from, to, t) + Vector3.up * (4f * 5f * t * (1f - t));
                spin = 0f;
                size = new Vector3(0.65f, 1.1f + 0.6f * t, 1f);
            }
            Quaternion facing = eye != null ? eye.rotation : Quaternion.identity;
            shown.transform.SetPositionAndRotation(at, facing * Quaternion.Euler(0f, 0f, spin));
            shown.transform.localScale = size;
            block.SetColor(ColourId, feather ? FeatherColour : SparkColour);
            shown.SetPropertyBlock(block);
        }

        /// <summary>Where a falling thing is now (for the smoke it trails).</summary>
        public Vector3 FallerAt(int index) => index >= 0 && index < fallers.Length && fallers[index] != null ? fallers[index].transform.position : Vector3.zero;

        public void Fallen(int index)
        {
            if (index >= 0 && index < fallers.Length && fallers[index] != null && fallers[index].enabled) fallers[index].enabled = false;
        }

        /// <summary>The burning ball (shown while the sun burns) and the quad of flames that licks off its rim.</summary>
        public void ConfigureBurning(GameObject burning, Transform rim, GameObject dull)
        {
            burningSun = burning;
            corona = rim;
            dullSun = dull;
        }

        /// <summary>The Zenith's blaze over the whole court: a volume whose weight follows the fill and the blaze.</summary>
        public void ConfigureBlaze(UnityEngine.Rendering.Volume look) => blazeLook = look;

        public void Configure(Light light, Transform sunHolder, Renderer[] streak, MeshFilter ring, TrailRenderer trail, Renderer[] stunStars)
        {
            sunTrail = trail;
            stars = stunStars;
            sunLight = light;
            sun = sunHolder;
            diveStreak = streak;
            shockRing = ring;
        }

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            var main = Camera.main;
            eye = main != null ? main.transform : null;
            if (sunLight != null) sunLight.intensity = 0f;
            if (sunTrail != null) sunTrail.emitting = false;
            foreach (var star in stars)
                if (star != null) star.enabled = false;
            foreach (var faller in fallers)
                if (faller != null) faller.enabled = false;
            crackAge = new float[cracks.Length];
            crackHot = new bool[cracks.Length];
            crackSize = new float[cracks.Length];
            for (int i = 0; i < cracks.Length; i++)
            {
                crackAge[i] = 999f;
                if (cracks[i] != null) cracks[i].enabled = false;
            }
            if (mound != null) mound.enabled = false;
            foreach (var slab in slabs)
                if (slab != null) slab.enabled = false;
            foreach (var arc in arcs)
                if (arc != null) arc.enabled = false;
            foreach (var shown in diveStreak)
                if (shown != null) shown.enabled = false;
            if (shockRing != null)
            {
                ringMesh = Ring();
                shockRing.sharedMesh = ringMesh;
                shockRenderer = shockRing.GetComponent<Renderer>();
                shockRenderer.enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (ringMesh != null) Destroy(ringMesh);
        }

        // A flat band one unit out, its breadth a share of that; v runs across the band so the glow's streak shape is soft at both edges.
        private static Mesh Ring()
        {
            var vertices = new Vector3[(RingSides + 1) * 2];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[RingSides * 6];
            for (int i = 0; i <= RingSides; i++)
            {
                float turn = i * Mathf.PI * 2f / RingSides;
                var way = new Vector3(Mathf.Sin(turn), 0f, Mathf.Cos(turn));
                vertices[i * 2] = way * (1f - RingBreadth);
                vertices[i * 2 + 1] = way;
                uvs[i * 2] = new Vector2(0.5f, 0f);
                uvs[i * 2 + 1] = new Vector2(0.5f, 1f);
                if (i == RingSides) continue;
                int t = i * 6, v = i * 2;
                triangles[t] = v; triangles[t + 1] = v + 1; triangles[t + 2] = v + 2;
                triangles[t + 3] = v + 1; triangles[t + 4] = v + 3; triangles[t + 5] = v + 2;
            }
            var mesh = new Mesh { name = "Shock Ring" };
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }

        /// <summary>How brightly the sun lights the court now: 0 when it lies loose and dull.</summary>
        public void SunShines(float intensity) => lightWanted = intensity;

        /// <summary>The stoop, falling: the streak runs from where it left the sky down to where its body is now.</summary>
        public void Diving(Vector3 top, Vector3 now)
        {
            diveTop = top;
            diveFoot = now;
            diveDrawn = 1f;
            diveFading = -1f;
        }

        /// <summary>The stoop has struck the floor: the streak fades and the shock ring runs out from here.</summary>
        public void Dived(Vector3 top, Vector3 floor)
        {
            diveTop = top;
            diveFoot = floor;
            diveFading = 0f;
            shockAt = new Vector3(floor.x, floor.y + ShockHeight, floor.z);
            shockAge = 0f;
        }

        /// <summary>How white the court is now, 0 to 1: the Zenith's fill and blaze.</summary>
        public void Blaze(float weight) => blazeWanted = Mathf.Clamp01(weight);

        /// <summary>The sun struck back by the parry draws a trail of fire behind it while it flies.</summary>
        public void SunTrail(bool on)
        {
            if (sunTrail == null || sunTrail.emitting == on) return;
            if (on) sunTrail.Clear();
            sunTrail.emitting = on;
        }

        /// <summary>Stars circling over a stunned form's head, a turn a second, for as long as it lies.</summary>
        public void Stunned(bool on, Vector3 over)
        {
            starsOver = over;
            if (stunned == on) return;
            stunned = on;
            foreach (var star in stars)
                if (star != null) star.enabled = on;
        }

        /// <summary>Everything off at once: the form is gone or back to the start.</summary>
        public void Clear()
        {
            Stunned(false, Vector3.zero);
            if (sunTrail != null) sunTrail.emitting = false;
            blazeWanted = blazeNow = 0f;
            if (blazeLook != null) blazeLook.weight = 0f;
            for (int i = 0; i < fallers.Length; i++) Fallen(i);
            lightWanted = lightNow = 0f;
            if (sunLight != null) sunLight.intensity = 0f;
            diveDrawn = diveFading = shockAge = -1f;
            foreach (var shown in diveStreak)
                if (shown != null) shown.enabled = false;
            if (shockRenderer != null) shockRenderer.enabled = false;
            Ridge(false, Vector3.zero, Vector3.forward, false, 0f);
            for (int i = 0; i < cracks.Length && i < crackAge.Length; i++)
            {
                crackAge[i] = 999f;
                if (cracks[i] != null) cracks[i].enabled = false;
            }
            FlaresDone();
            GustWall(false, Vector3.zero, Vector3.forward, 0f, 0f);
        }

        // ------------------------------------------------------------------ the ground broken by the burrowing scarab

        /// <summary>
        /// The ridge over the scarab going underground, its head at <paramref name="head"/>: a heave of paving with slabs
        /// lifted and tilted round it, rocking as it goes, and cracks left behind it on the floor every so often, glowing
        /// ember red when the sun is under it. Off, it is gone.
        /// </summary>
        public void Ridge(bool on, Vector3 head, Vector3 heading, bool fiery, float speed)
        {
            ridgeOn = on;
            if (mound != null && mound.enabled != on) mound.enabled = on;
            foreach (var slab in slabs)
                if (slab != null && slab.enabled != on) slab.enabled = on;
            if (!on) return;
            heading.y = 0f;
            if (heading.sqrMagnitude < 0.0001f) heading = Vector3.forward;
            heading.Normalize();
            var facing = Quaternion.LookRotation(heading);
            float time = Time.time;
            // It heaves: the mound rises and falls a little as it goes, faster when it goes fast.
            float heave = 0.55f + 0.12f * Mathf.Sin(time * (6f + speed));
            if (mound != null)
            {
                // A hump of broken earth and paving half a metre high, over the head of it.
                mound.transform.SetPositionAndRotation(head + Vector3.up * (heave * 0.5f - 0.12f), facing);
                mound.transform.localScale = new Vector3(2.4f, heave, 3.2f);
            }
            for (int i = 0; i < slabs.Length; i++)
            {
                var slab = slabs[i];
                if (slab == null) continue;
                float turn = i * 360f / slabs.Length + 20f;
                Vector3 round = Quaternion.Euler(0f, turn, 0f) * heading;
                float rock = Mathf.Sin(time * (9f + i) + i * 1.9f) * 9f;
                Vector3 spot = head + round * (1.25f + 0.25f * (i % 2)) + Vector3.up * (0.12f + 0.1f * Mathf.Sin(time * 7f + i));
                // Each slab tipped up away from the heave, as paving breaks over something pushing through.
                slab.transform.SetPositionAndRotation(spot, Quaternion.LookRotation(round) * Quaternion.Euler(-28f - rock, (i * 37f) % 40f - 20f, rock));
            }
            if ((head - lastCrack).sqrMagnitude >= CrackEvery * CrackEvery) LayCrack(head - heading * 0.8f, fiery, CrackSize * Random.Range(0.8f, 1.15f));
        }

        /// <summary>A star of cracks broken into the floor where the scarab went down or came up.</summary>
        public void Crater(Vector3 at, float radius, bool fiery)
        {
            LayCrack(at, fiery, radius * 2.2f);
            LayCrack(at + new Vector3(0.6f, 0f, -0.4f), false, radius * 1.6f);
        }

        private void LayCrack(Vector3 at, bool fiery, float size)
        {
            lastCrack = at;
            if (cracks.Length == 0) return;
            int slot = nextCrack;
            nextCrack = (nextCrack + 1) % cracks.Length;
            var shown = cracks[slot];
            if (shown == null) return;
            crackAge[slot] = 0f;
            crackHot[slot] = fiery;
            crackSize[slot] = size;
            shown.enabled = true;
            shown.transform.SetPositionAndRotation(new Vector3(at.x, 0.075f, at.z), Quaternion.Euler(90f, Random.Range(0f, 360f), 0f));
            shown.transform.localScale = new Vector3(size, size, 1f);
        }

        private void DrawCracks(float deltaTime)
        {
            for (int i = 0; i < cracks.Length; i++)
            {
                if (crackAge[i] > CrackSeconds) continue;
                crackAge[i] += deltaTime;
                var shown = cracks[i];
                if (shown == null) continue;
                if (crackAge[i] > CrackSeconds)
                {
                    shown.enabled = false;
                    continue;
                }
                float left = 1f - crackAge[i] / CrackSeconds;
                // Hot cracks cool to plain dark ones as they fade.
                Color colour = crackHot[i] ? Color.Lerp(CrackColour, HotCrackColour, Mathf.Clamp01(left * 1.6f - 0.3f)) : CrackColour;
                colour.a *= Mathf.Clamp01(left * 2.5f);
                block.SetColor(ColourId, colour);
                shown.SetPropertyBlock(block);
            }
        }

        // ------------------------------------------------------------------ the Flares and the Gust

        /// <summary>
        /// One of the Flares' arcs of flame, from the sun's top to the far end of its line: <paramref name="grow"/> from 0
        /// to 1 draws it up and over; <paramref name="slam"/> from 0 to 1 brings it down flat along its line, and past 1 it is gone.
        /// </summary>
        public void FlareArc(int index, Vector3 from, Vector3 to, float grow, float slam)
        {
            if (index < 0 || index >= arcs.Length || arcs[index] == null) return;
            var arc = arcs[index];
            bool on = grow > 0.01f && slam < 1f;
            if (arc.enabled != on) arc.enabled = on;
            if (!on) return;
            float height = ArcHeight * Mathf.Clamp01((to - from).magnitude / 12f + 0.3f) * (1f - slam);
            int count = arcPoints.Length;
            for (int i = 0; i < count; i++)
            {
                float u = grow * i / (count - 1f);
                Vector3 point = Vector3.Lerp(from, to + Vector3.up * 0.3f, u);
                // A loop, not a parabola: it leans back over the sun before it comes down along the line.
                point += Vector3.up * (Mathf.Sin(u * Mathf.PI) * height * (1.1f - 0.2f * u));
                arcPoints[i] = point;
            }
            arc.positionCount = count;
            arc.SetPositions(arcPoints);
            arc.widthMultiplier = Mathf.Lerp(0.9f, 1.4f, slam);
        }

        public void FlaresDone()
        {
            foreach (var arc in arcs)
                if (arc != null && arc.enabled) arc.enabled = false;
        }

        /// <summary>
        /// The Ember Gust's wall of burning wind across its band at <paramref name="front"/>, going along it: a row of tall
        /// flames standing across the band at its front and a lower row a little behind, a front of fire driven over the
        /// floor. The char under them is not drawn: only the wind passes.
        /// </summary>
        public void GustWall(bool on, Vector3 front, Vector3 along, float width, float strength)
        {
            if (gustFire == null) return;
            if (!on || strength <= 0.01f)
            {
                gustFire.HideAll();
                return;
            }
            along.y = 0f;
            along.Normalize();
            var side = new Vector3(along.z, 0f, -along.x);
            gustFire.Lane(0, front - side * (width * 0.5f), side, 2.4f, width, 1f, strength, strength, 1.5f, false);
            gustFire.Lane(1, front - along * 2.2f - side * (width * 0.45f), side, 2f, width * 0.9f, 1f, strength * 0.6f, strength, 0.8f, false);
        }

        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;
            if (sunLight != null)
            {
                lightNow = Mathf.MoveTowards(lightNow, lightWanted, LightPace * deltaTime);
                if (sun != null) sunLight.transform.position = sun.position + Vector3.up * LightAbove;
                float time = Time.time;
                float breath = 1f + Flicker * 0.5f * (Mathf.Sin(time * FlickerSlow * 2f * Mathf.PI) + Mathf.Sin(time * FlickerFast * 2f * Mathf.PI + 1.3f));
                sunLight.intensity = lightNow * breath * Strength;
                bool on = lightNow > 0.01f;
                if (sunLight.enabled != on) sunLight.enabled = on;
            }
            DrawBurning(deltaTime);
            DrawCracks(deltaTime);
            DrawDive(deltaTime);
            DrawShock(deltaTime);
            DrawStars(deltaTime);
            if (blazeLook != null && blazeNow != blazeWanted)
            {
                // It whitens as fast as the fill asks, and comes back over a second when the blaze ends.
                blazeNow = blazeWanted > blazeNow ? blazeWanted : Mathf.MoveTowards(blazeNow, blazeWanted, BlazePace * deltaTime);
                blazeLook.weight = blazeNow;
            }
        }

        // The burning ball: its rim of flames turned to the eye and hung behind it so the ball hides the rim's middle,
        // smoke rolling up off its top, and embers dropping from its underside.
        private void DrawBurning(float deltaTime)
        {
            if (sun == null) return;
            // Burnt out, it is a charred ball that still smokes from its glowing cracks.
            if (dullSun != null && dullSun.activeInHierarchy)
            {
                untilSmoke -= deltaTime;
                if (untilSmoke > 0f) return;
                untilSmoke = SmokeEvery * 2.5f;
                Vector3 crack = new Vector3(Random.Range(-0.8f, 0.8f), 0f, Random.Range(-0.8f, 0.8f)) * sun.lossyScale.x;
                Vfx.Play(VfxKind.Smoke, sun.position + crack + Vector3.up * (1.2f * sun.lossyScale.x), Vector3.up, 0.7f * sun.lossyScale.x, SmokeColour);
                return;
            }
            if (burningSun == null || !burningSun.activeInHierarchy) return;
            float across = sun.lossyScale.x;
            if (corona != null && eye != null)
                corona.SetPositionAndRotation(sun.position + eye.forward * (CoronaBehind * across), eye.rotation);
            untilSmoke -= deltaTime;
            if (untilSmoke <= 0f)
            {
                untilSmoke = SmokeEvery;
                Vector3 drift = new Vector3(Random.Range(-0.6f, 0.6f), 0f, Random.Range(-0.6f, 0.6f)) * across;
                Vfx.Play(VfxKind.Smoke, sun.position + Vector3.up * (1.6f * across) + drift, Vector3.up, across, SmokeColour);
            }
            untilEmber -= deltaTime;
            if (untilEmber <= 0f)
            {
                untilEmber = EmberEvery;
                Vector3 side = new Vector3(Random.Range(-1f, 1f), 0f, Random.Range(-1f, 1f)) * (1.2f * across);
                Vfx.Play(VfxKind.Embers, sun.position + side + Vector3.down * (0.6f * across), Vector3.down, 0.5f);
            }
        }

        private void DrawStars(float deltaTime)
        {
            if (!stunned || stars.Length == 0) return;
            starTurn += deltaTime * Mathf.PI * 2f;
            Quaternion facing = eye != null ? eye.rotation : Quaternion.identity;
            for (int i = 0; i < stars.Length; i++)
            {
                var star = stars[i];
                if (star == null) continue;
                float turn = starTurn + i * Mathf.PI * 2f / stars.Length;
                star.transform.SetPositionAndRotation(starsOver + new Vector3(Mathf.Sin(turn) * StarRadius, Mathf.Sin(turn * 2f) * 0.15f, Mathf.Cos(turn) * StarRadius), facing);
            }
        }

        private void DrawDive(float deltaTime)
        {
            if (diveDrawn < 0f) return;
            float strength = 1f;
            if (diveFading >= 0f)
            {
                diveFading += deltaTime;
                strength = 1f - diveFading / DiveFade;
                if (strength <= 0f)
                {
                    diveDrawn = diveFading = -1f;
                    foreach (var shown in diveStreak)
                        if (shown != null) shown.enabled = false;
                    return;
                }
            }
            // Fading, it draws in toward the floor, as the air closes behind the blow.
            Vector3 top = Vector3.Lerp(diveTop, diveFoot, diveFading >= 0f ? (1f - strength) * 0.7f : 0f);
            Vector3 line = top - diveFoot;
            float length = line.magnitude;
            if (length < 0.1f) return;
            Vector3 middle = (top + diveFoot) * 0.5f;
            block.SetColor(ColourId, DiveColour * strength);
            for (int i = 0; i < diveStreak.Length; i++)
            {
                var shown = diveStreak[i];
                if (shown == null) continue;
                if (!shown.enabled) shown.enabled = true;
                // Two quads crossed about the fall's line, each with its length along the fall.
                Vector3 facing = i == 0 ? Vector3.forward : Vector3.right;
                Vector3 side = Vector3.Cross(line / length, facing);
                if (side.sqrMagnitude < 0.0001f) side = Vector3.right;
                shown.transform.SetPositionAndRotation(middle, Quaternion.LookRotation(Vector3.Cross(line / length, side.normalized), side.normalized));
                shown.transform.localScale = new Vector3(length, DiveWidth * (0.6f + 0.4f * strength), 1f);
                shown.SetPropertyBlock(block);
            }
        }

        private void DrawShock(float deltaTime)
        {
            if (shockAge < 0f || shockRenderer == null) return;
            shockAge += deltaTime;
            float t = shockAge / ShockSeconds;
            if (t >= 1f)
            {
                shockAge = -1f;
                shockRenderer.enabled = false;
                return;
            }
            if (!shockRenderer.enabled) shockRenderer.enabled = true;
            float eased = 1f - (1f - t) * (1f - t);
            float radius = Mathf.Lerp(ShockFrom, ShockTo, eased);
            shockRing.transform.SetPositionAndRotation(shockAt, Quaternion.identity);
            shockRing.transform.localScale = new Vector3(radius, 1f, radius);
            block.SetColor(ColourId, ShockColour * (1f - t * t));
            shockRenderer.SetPropertyBlock(block);
        }
    }
}

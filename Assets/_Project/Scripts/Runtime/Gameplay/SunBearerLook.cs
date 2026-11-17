using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The Sun-Bearer's effects that are not particles (fire_boss_visuals.md, section 2.3): the light the sun throws on
    /// the court, breathing on two waves; the streak the falcon's stoop tears down through the air; and the shock ring
    /// that runs out through the air where it lands. <see cref="SunBearer"/> says what happens; this draws it. Every piece
    /// is made by the builder and only moved, scaled and tinted here, through one property block.
    /// </summary>
    public class SunBearerLook : MonoBehaviour
    {
        [SerializeField] private Light sunLight;
        [SerializeField] private Transform sun;
        [SerializeField] private Renderer[] diveStreak = new Renderer[0];
        [SerializeField] private MeshFilter shockRing;
        [SerializeField] private TrailRenderer sunTrail;
        [SerializeField] private Renderer[] stars = new Renderer[0];
        [SerializeField] private UnityEngine.Rendering.Volume blazeLook;
        [SerializeField] private Renderer[] fallers = new Renderer[0];
        [ColorUsage(true, true)] public Color SparkColour = new Color(4.5f, 1.6f, 0.3f, 1f);
        [ColorUsage(true, true)] public Color FeatherColour = new Color(3.4f, 2.4f, 0.7f, 1f);

        [ColorUsage(true, true)] public Color DiveColour = new Color(4.4f, 3.1f, 0.9f, 1f);
        [ColorUsage(true, true)] public Color ShockColour = new Color(3.4f, 3.2f, 2.6f, 1f);
        public float DiveWidth = 2.2f, DiveFade = 0.2f;
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

        /// <summary>One falling thing, this far through its fall (0 to 1), from where it left the body to the middle of its mark.</summary>
        public void Falling(int index, Vector3 from, Vector3 to, float t, bool feather)
        {
            if (index < 0 || index >= fallers.Length || fallers[index] == null) return;
            var shown = fallers[index];
            if (!shown.enabled) shown.enabled = true;
            Vector3 at;
            float spin;
            Vector3 size;
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

        public void Fallen(int index)
        {
            if (index >= 0 && index < fallers.Length && fallers[index] != null && fallers[index].enabled) fallers[index].enabled = false;
        }

        /// <summary>The Zenith's whitening of the whole court: a volume whose weight follows the fill and the blaze.</summary>
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

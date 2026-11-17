using Clover.Core;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The Sun-Spear's ray drawn as a beam of real light (fire_boss_visuals.md 2.4). While he calls the spear back a
    /// flickering thread joins its blade to his raised hand and motes drift into it; at the strike the dragon's beam, in
    /// gold: a white core with the glow's beam shape and two gold strands kinking round it, redrawn every 0.04 s, with the
    /// spear itself flying down it as a white streak carrying a light; after it, a line of heat on the floor fades. Every
    /// line is made by the builder; nothing here is created during play.
    /// </summary>
    public class SpearRay : MonoBehaviour
    {
        [SerializeField] private LineRenderer thread;
        [SerializeField] private LineRenderer core;
        [SerializeField] private LineRenderer[] strands = new LineRenderer[0];
        [SerializeField] private LineRenderer spearStreak;
        [SerializeField] private Light rayLight;
        [SerializeField] private Renderer heatLine;

        public float ThreadWidth = 0.2f, ThreadWideWidth = 0.4f, ThreadWidensAt = 0.7f;
        // The core a little under the plan's metre and the strands wider, so the gold reads beside the white under bloom.
        public float CoreWidth = 0.8f, StrandWidth = 0.5f, StrandStray = 0.55f;
        public float StrikeSeconds = 0.15f, FadeSeconds = 0.14f, HeatSeconds = 1.5f, HeatWidth = 0.6f;
        public float LightIntensity = 14f;
        [ColorUsage(true, true)] public Color HeatColour = new Color(2.6f, 0.9f, 0.2f, 1f);

        private const int Points = 12;
        private static readonly int ColourId = Shader.PropertyToID("_Color");
        private static readonly Color SunWhite = new Color(1f, 0.92f, 0.62f, 1f);
        private static readonly Color Gold = new Color(1f, 0.72f, 0.2f, 1f);

        private readonly Vector3[] points = new Vector3[Points];
        private readonly Vector3[] pair = new Vector3[2];
        private MaterialPropertyBlock block;
        private Vector3 from, to;
        private float strikeAge = -1f, heatAge = -1f, untilRedraw, untilMote, untilSpark;

        /// <summary>True from the strike until the beam has faded: for probes.</summary>
        public bool Striking => strikeAge >= 0f;

        public void Configure(LineRenderer threadLine, LineRenderer coreLine, LineRenderer[] strandLines, LineRenderer streak, Light light, Renderer heat)
        {
            thread = threadLine;
            core = coreLine;
            strands = strandLines;
            spearStreak = streak;
            rayLight = light;
            heatLine = heat;
        }

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            Clear();
        }

        private void OnDisable() => Clear();

        /// <summary>All of it off: he is dead, or the fight began again.</summary>
        public void Clear()
        {
            strikeAge = heatAge = -1f;
            Show(thread, false);
            Show(core, false);
            foreach (var strand in strands) Show(strand, false);
            Show(spearStreak, false);
            if (rayLight != null) rayLight.enabled = false;
            if (heatLine != null) heatLine.enabled = false;
        }

        private static void Show(Renderer shown, bool on)
        {
            if (shown != null && shown.enabled != on) shown.enabled = on;
        }

        /// <summary>The call: the thread from the standing spear's blade to his hand, so far through the tell (0 to 1).</summary>
        public void Thread(Vector3 blade, Vector3 hand, float progress)
        {
            from = blade;
            to = hand;
            if (thread == null) return;
            Show(thread, true);
            pair[0] = blade;
            pair[1] = hand;
            thread.positionCount = 2;
            thread.SetPositions(pair);
            float flicker = 1f + 0.2f * Mathf.Sin(Time.time * 15f * 2f * Mathf.PI);
            bool wide = progress >= ThreadWidensAt;
            thread.widthMultiplier = (wide ? ThreadWideWidth : ThreadWidth) * flicker;
            var tint = new Color(1f, 0.95f, 0.8f, Mathf.Lerp(0.4f, 0.9f, progress));
            thread.startColor = tint;
            thread.endColor = tint;
            // Motes drift into the thread from either side; near the end the blade throws sparks.
            float deltaTime = Time.deltaTime;
            untilMote -= deltaTime;
            if (untilMote <= 0f)
            {
                untilMote = 0.1f;
                Vector3 point = Vector3.Lerp(blade, hand, Random.value);
                Vector3 across = Vector3.Cross(Vector3.up, (hand - blade).normalized) * (Random.value < 0.5f ? -1.4f : 1.4f);
                Vfx.Play(VfxKind.Embers, point + across, -across, 0.4f, SunWhite);
            }
            if (!wide) return;
            untilSpark -= deltaTime;
            if (untilSpark > 0f) return;
            untilSpark = 0.15f;
            Vfx.Play(VfxKind.TintSpark, blade, Vector3.up, 0.6f, Gold);
        }

        public void HideThread() => Show(thread, false);

        /// <summary>The strike: the beam from the blade to his hand, the spear flying down it.</summary>
        public void Strike(Vector3 blade, Vector3 hand)
        {
            from = blade;
            to = hand;
            Show(thread, false);
            strikeAge = 0f;
            untilRedraw = 0f;
            heatAge = 0f;
            if (heatLine != null)
            {
                Vector3 a = new Vector3(blade.x, 0.02f, blade.z), b = new Vector3(hand.x, 0.02f, hand.z);
                Vector3 line = b - a;
                float length = line.magnitude;
                if (length > 0.1f)
                {
                    heatLine.transform.SetPositionAndRotation((a + b) * 0.5f, Quaternion.LookRotation(Vector3.up, Vector3.Cross(Vector3.up, line / length)));
                    heatLine.transform.localScale = new Vector3(length, HeatWidth, 1f);
                    heatLine.enabled = true;
                }
            }
            Vfx.Play(VfxKind.Impact, blade, Vector3.up, 1f, SunWhite);
        }

        private void LateUpdate()
        {
            float deltaTime = Time.deltaTime;
            if (heatAge >= 0f && heatLine != null)
            {
                heatAge += deltaTime;
                float left = 1f - heatAge / HeatSeconds;
                if (left <= 0f)
                {
                    heatAge = -1f;
                    heatLine.enabled = false;
                }
                else
                {
                    block.SetColor(ColourId, HeatColour * (left * left));
                    heatLine.SetPropertyBlock(block);
                }
            }
            if (strikeAge < 0f) return;
            strikeAge += deltaTime;
            float strength = strikeAge <= StrikeSeconds ? 1f : 1f - (strikeAge - StrikeSeconds) / FadeSeconds;
            if (strength <= 0f)
            {
                strikeAge = -1f;
                Show(core, false);
                foreach (var strand in strands) Show(strand, false);
                Show(spearStreak, false);
                if (rayLight != null) rayLight.enabled = false;
                return;
            }
            // The spear comes down the beam to his hand in the strike's time, a white streak carrying a light.
            float flown = Mathf.Clamp01(strikeAge / StrikeSeconds);
            Vector3 head = Vector3.Lerp(from, to, flown);
            if (spearStreak != null)
            {
                Show(spearStreak, flown < 1f);
                pair[0] = Vector3.Lerp(from, to, Mathf.Max(0f, flown - 0.3f));
                pair[1] = head;
                spearStreak.positionCount = 2;
                spearStreak.SetPositions(pair);
            }
            if (rayLight != null)
            {
                rayLight.enabled = true;
                rayLight.transform.position = head;
                rayLight.intensity = LightIntensity * strength;
            }
            var fade = new Color(1f, 1f, 1f, strength);
            untilRedraw -= deltaTime;
            bool redraw = untilRedraw <= 0f;
            if (redraw) untilRedraw = 0.04f;
            Vector3 along = (to - from).normalized;
            Vector3 across = Vector3.Cross(Vector3.up, along).normalized;
            if (core != null)
            {
                Show(core, true);
                if (redraw)
                {
                    for (int i = 0; i < Points; i++)
                    {
                        float t = i / (float)(Points - 1);
                        points[i] = Vector3.Lerp(from, to, t) + (i == 0 || i == Points - 1 ? Vector3.zero : across * Random.Range(-0.06f, 0.06f));
                    }
                    core.positionCount = Points;
                    core.SetPositions(points);
                    core.widthMultiplier = CoreWidth * Random.Range(0.9f, 1.1f);
                }
                core.startColor = fade;
                core.endColor = fade;
            }
            for (int strand = 0; strand < strands.Length; strand++)
            {
                var line = strands[strand];
                if (line == null) continue;
                Show(line, true);
                if (redraw)
                {
                    // Kinked round the core, never further out than the lane's edge.
                    for (int i = 0; i < Points; i++)
                    {
                        float t = i / (float)(Points - 1);
                        bool end = i == 0 || i == Points - 1;
                        points[i] = Vector3.Lerp(from, to, t) + (end ? Vector3.zero : across * Random.Range(-StrandStray, StrandStray) + Vector3.up * Random.Range(-0.2f, 0.3f));
                    }
                    line.positionCount = Points;
                    line.SetPositions(points);
                    line.widthMultiplier = StrandWidth;
                }
                line.startColor = fade;
                line.endColor = fade;
            }
        }
    }
}

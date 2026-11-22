using Clover.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// What the player sees when Memories are paid (way_build_plan.md question 10, the owner's answer): a small warm amber
    /// light rises from the fallen and flies into the player's chest over 0.8 s, leaving a short trail of amber motes, and
    /// as it lands "+4 Memories" shows for two seconds under the Shards count. The Memories themselves are already in the
    /// wallet when the light leaves; this is for the eye only, and with no HUD in the scene nothing flies.
    ///
    /// Made at play beside the Shards count (<see cref="Make"/>, from <see cref="PlayerHud"/>), so no scene needs
    /// rebuilding. A few lights are made once and reused; nothing is made or allocated while they fly.
    /// </summary>
    public class MemoryGain : MonoBehaviour
    {
        /// <summary>The light's colour: warm amber, never white.</summary>
        public static readonly Color Amber = new Color(1f, 0.6f, 0.18f, 1f);
        /// <summary>The line's colour: a paler amber that reads on the HUD.</summary>
        public static readonly Color LineColour = new Color(1f, 0.78f, 0.42f, 1f);
        /// <summary>Lights that can be in the air at once; a payment with none free still shows its number.</summary>
        public const int MostInFlight = 6;

        /// <summary>Seconds between the motes the light leaves behind it.</summary>
        public float TrailEvery = 0.035f;
        public float LightRange = 2.6f;
        public float LightIntensity = 2.4f;

        /// <summary>The line's tally, for probes.</summary>
        public MemoryLine Line { get; } = new MemoryLine();
        /// <summary>Lights that have landed, for probes.</summary>
        public int Landed { get; private set; }

        private static MemoryGain instance;

        private Text text;
        private Light[] lights;
        private Vector3[] froms;
        private Transform[] takers;
        private float[] ages;
        private float[] delays;
        private int[] amounts;
        private float[] sinceMote;
        private bool[] busy;
        private int shownVersion = -1;

        // Domain reload is off: the last play's HUD must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        /// <summary>The one in the scene, or null.</summary>
        public static MemoryGain Current => instance;

        /// <summary>Builds the line just under the Shards count, in its font and size, and the lights it sends.</summary>
        public static MemoryGain Make(Text shards)
        {
            if (shards == null) return null;
            var made = new GameObject("Memories Gained", typeof(RectTransform));
            var rect = (RectTransform)made.transform;
            var under = shards.rectTransform;
            rect.SetParent(under.parent, false);
            rect.anchorMin = under.anchorMin;
            rect.anchorMax = under.anchorMax;
            rect.pivot = under.pivot;
            rect.sizeDelta = under.sizeDelta;
            float height = under.rect.height > 0f ? under.rect.height : shards.fontSize * 1.3f;
            rect.anchoredPosition = under.anchoredPosition - new Vector2(0f, height);
            var line = made.AddComponent<Text>();
            line.font = shards.font;
            line.fontSize = shards.fontSize;
            line.fontStyle = shards.fontStyle;
            line.alignment = shards.alignment;
            line.horizontalOverflow = HorizontalWrapMode.Overflow;
            line.raycastTarget = false;
            line.color = LineColour;
            line.text = "";
            line.enabled = false;
            // The same edge the Shards count has, so it reads over the world the same way.
            var edge = shards.GetComponent<Shadow>();
            if (edge != null && made.AddComponent(edge.GetType()) is Shadow copy)
            {
                copy.effectColor = edge.effectColor;
                copy.effectDistance = edge.effectDistance;
            }
            var gain = made.AddComponent<MemoryGain>();
            gain.text = line;
            return gain;
        }

        /// <summary>
        /// Sends the light of a payment from a point to someone, after <paramref name="delay"/> seconds (a god's waits for
        /// its fall's moment to pass). With no HUD nothing shows; with no light free the number shows at once.
        /// </summary>
        public static void Fly(Vector3 from, Transform to, int amount, float delay = 0f)
        {
            if (amount <= 0 || instance == null) return;
            if (to == null || !instance.Launch(from, to, amount, delay)) instance.Line.Add(amount);
        }

        private void Awake()
        {
            lights = new Light[MostInFlight];
            froms = new Vector3[MostInFlight];
            takers = new Transform[MostInFlight];
            ages = new float[MostInFlight];
            delays = new float[MostInFlight];
            amounts = new int[MostInFlight];
            sinceMote = new float[MostInFlight];
            busy = new bool[MostInFlight];
        }

        private void OnEnable() => instance = this;

        private void OnDisable()
        {
            if (instance == this) instance = null;
        }

        private void OnDestroy()
        {
            if (lights == null) return;
            foreach (var light in lights)
                if (light != null) Destroy(light.gameObject);
        }

        private bool Launch(Vector3 from, Transform to, int amount, float delay)
        {
            for (int i = 0; i < MostInFlight; i++)
            {
                if (busy[i]) continue;
                busy[i] = true;
                froms[i] = from;
                takers[i] = to;
                ages[i] = 0f;
                delays[i] = Mathf.Max(0f, delay);
                amounts[i] = amount;
                sinceMote[i] = TrailEvery;
                return true;
            }
            return false;
        }

        // The world's lights are made the first time one flies: a scene with no payment makes none.
        private Light LightAt(int i)
        {
            if (lights[i] != null) return lights[i];
            var holder = new GameObject("Memory Light " + (i + 1));
            var light = holder.AddComponent<Light>();
            light.type = LightType.Point;
            light.color = Amber;
            light.range = LightRange;
            light.intensity = 0f;
            light.shadows = LightShadows.None;
            holder.SetActive(false);
            lights[i] = light;
            return light;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < MostInFlight; i++)
            {
                if (!busy[i]) continue;
                ages[i] += deltaTime;
                float share = MemoryFlight.Share(ages[i], delays[i]);
                if (share < 0f) continue;
                var light = LightAt(i);
                Vector3 chest = takers[i] != null ? takers[i].position + Vector3.up * MemoryFlight.ChestHeight : froms[i];
                if (!light.gameObject.activeSelf)
                {
                    // The light leaves the body with a small breath of amber where it rose.
                    light.gameObject.SetActive(true);
                    Vfx.Play(VfxKind.TintPuff, froms[i], Vector3.up, 0.6f, Amber);
                }
                if (share >= 1f)
                {
                    busy[i] = false;
                    light.intensity = 0f;
                    light.gameObject.SetActive(false);
                    Vfx.Play(VfxKind.TintPuff, chest, Vector3.up, 1f, Amber);
                    Line.Add(amounts[i]);
                    Landed++;
                    continue;
                }
                Vector3 at = MemoryFlight.At(froms[i], chest, share);
                light.transform.position = at;
                // It swells as it rises and dims a little as it is taken in.
                light.intensity = LightIntensity * (share < 0.15f ? share / 0.15f : 1f - 0.4f * share);
                sinceMote[i] += deltaTime;
                if (sinceMote[i] >= TrailEvery)
                {
                    sinceMote[i] = 0f;
                    Vfx.Play(VfxKind.TintPuff, at, Vector3.up, 0.25f, Amber);
                }
            }

            // The line is drawn in unscaled time, as the HUD's own pulses are, so a hit stop does not hold it up.
            Line.Tick(Time.unscaledDeltaTime);
            if (text == null) return;
            if (Line.Version != shownVersion)
            {
                shownVersion = Line.Version;
                text.enabled = Line.Visible;
                if (Line.Visible) text.text = MemoryLine.Words(Line.Amount);
            }
            if (Line.Visible)
            {
                var colour = LineColour;
                colour.a = Line.Alpha;
                text.color = colour;
            }
        }
    }
}

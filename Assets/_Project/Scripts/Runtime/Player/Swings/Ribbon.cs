using UnityEngine;

namespace Clover.Player.Swings
{
    /// <summary>
    /// One swing trail's memory: the inner and outer points of the blade line, sampled each frame of the swing, kept in a
    /// fixed ring (the oldest given way when it is full), with the in-betweens bent round the swinger when the blade
    /// moved far between two frames. <see cref="Build"/> lays it out as a strip of two vertices a sample into arrays the
    /// caller owns, ageing, moving and fading each part as its layer says. Nothing is allocated after it is made.
    /// </summary>
    public sealed class Ribbon
    {
        public const int Capacity = 192;

        private readonly Vector3[] inner = new Vector3[Capacity], outer = new Vector3[Capacity], way = new Vector3[Capacity];
        private readonly float[] time = new float[Capacity], gate = new float[Capacity];
        private readonly int[] id = new int[Capacity];
        private int first, count, nextId;
        private Vector3 lastPivot, lastInnerOffset, lastOuterOffset, lastWay;
        private float lastTime, lastGate;
        private bool hasLast;

        /// <summary>True while the swing that draws it is still being made.</summary>
        public bool Emitting { get; private set; }
        /// <summary>Seconds each part lasts, the lag it is drawn behind the blade, and the share of its life a frozen trail hangs whole.</summary>
        public float Seconds { get; private set; } = 0.3f;
        public float Lag { get; private set; }
        public float Hold { get; private set; }
        /// <summary>When the swing that drew it ended.</summary>
        public float Stopped { get; private set; } = float.MaxValue;
        public int Count => count;
        /// <summary>For the caller: whether its end (a spray, a shatter) has been drawn.</summary>
        public bool Ended;

        public float NewestTime => count > 0 ? time[(first + count - 1) % Capacity] : 0f;

        /// <summary>Starts a fresh trail.</summary>
        public void Begin(float seconds, float lag, float hold)
        {
            first = count = 0;
            hasLast = false;
            Emitting = true;
            Seconds = Mathf.Max(0.02f, seconds);
            Lag = Mathf.Max(0f, lag);
            Hold = Mathf.Clamp01(hold);
            Stopped = float.MaxValue;
            Ended = false;
        }

        /// <summary>The swing is over: nothing more is added, and what is there lives out its time.</summary>
        public void Stop(float now)
        {
            if (!Emitting) return;
            Emitting = false;
            Stopped = now;
        }

        public void Clear()
        {
            count = 0;
            Emitting = false;
            hasLast = false;
            Stopped = float.MaxValue;
        }

        /// <summary>True while it is being drawn or anything of it is still to be seen.</summary>
        public bool Alive(float now)
        {
            if (Emitting) return true;
            if (count == 0) return false;
            if (Hold > 0f) return now < Stopped + Seconds;
            return now - NewestTime < Seconds + Lag;
        }

        /// <summary>
        /// Adds where the blade line is now: its inner and outer points, the way it is moving, and how whole the trail is
        /// drawn here (from the tip's speed). <paramref name="pivot"/> is the swinger's chest: when the blade has turned
        /// far round it since the last sample, points are put in between along the arc.
        /// </summary>
        public void Push(Vector3 pivot, Vector3 innerPoint, Vector3 outerPoint, Vector3 moving, float now, float strength, float degreesPerStep = 6f, int most = 12)
        {
            Vector3 innerOffset = innerPoint - pivot, outerOffset = outerPoint - pivot;
            if (hasLast)
            {
                int steps = RibbonMath.Steps(lastOuterOffset, outerOffset, degreesPerStep, most);
                for (int k = 1; k < steps; k++)
                {
                    float s = k / (float)steps;
                    Add(RibbonMath.Between(lastPivot, lastInnerOffset, pivot, innerOffset, s), RibbonMath.Between(lastPivot, lastOuterOffset, pivot, outerOffset, s),
                        Vector3.Slerp(lastWay, moving, s), Mathf.Lerp(lastTime, now, s), Mathf.Lerp(lastGate, strength, s));
                }
            }
            Add(innerPoint, outerPoint, moving, now, strength);
            lastPivot = pivot;
            lastInnerOffset = innerOffset;
            lastOuterOffset = outerOffset;
            lastWay = moving;
            lastTime = now;
            lastGate = strength;
            hasLast = true;
        }

        private void Add(Vector3 innerPoint, Vector3 outerPoint, Vector3 moving, float at, float strength)
        {
            if (count == Capacity)
            {
                first = (first + 1) % Capacity;
                count--;
            }
            int i = (first + count) % Capacity;
            inner[i] = innerPoint;
            outer[i] = outerPoint;
            way[i] = moving;
            time[i] = at;
            gate[i] = strength;
            id[i] = nextId++;
            count++;
        }

        /// <summary>Lets go of the parts whose time is over. A frozen trail keeps every part until it shatters.</summary>
        public void Trim(float now)
        {
            if (Hold > 0f) return;
            float keep = Seconds + Lag;
            while (count > 0 && now - time[first] >= keep)
            {
                first = (first + 1) % Capacity;
                count--;
            }
        }

        /// <summary>A sample, the oldest being 0.</summary>
        public void Get(int index, out Vector3 innerPoint, out Vector3 outerPoint, out Vector3 moving, out float at, out float strength)
        {
            int i = (first + Mathf.Clamp(index, 0, Mathf.Max(0, count - 1))) % Capacity;
            innerPoint = inner[i];
            outerPoint = outer[i];
            moving = way[i];
            at = time[i];
            strength = gate[i];
        }

        /// <summary>
        /// Lays the trail out as a strip, oldest first: two vertices a sample, each part aged, moved, thinned and faded as
        /// the layer says, U along the trail (head at the window's end) and V across it. Returns the vertices written
        /// (zero when fewer than two parts are to be seen).
        /// </summary>
        /// <summary>How long a burning trail's tongues climb (<see cref="TrailLayer.Lick"/>) before they only fade.</summary>
        public const float LickSeconds = 0.3f;

        public int Build(in TrailLayer layer, float now, Vector3[] vertices, Vector2[] uvs, Color[] colours, out Bounds bounds)
        {
            bounds = default;
            if (count < 2) return 0;
            var style = layer.Style;
            bool bolt = style.Shape > 6.5f && style.Shape < 7.5f;
            int beat = Mathf.FloorToInt(now * 20f);
            // A frozen trail fades all at once after hanging whole; the rest fade part by part with their age.
            float frozenFade = 1f;
            if (Hold > 0f)
            {
                float since = Emitting ? 0f : now - Stopped;
                float holdSeconds = Seconds * Hold, fadeSeconds = Mathf.Max(0.02f, Seconds - holdSeconds);
                frozenFade = RibbonMath.Fade(Mathf.Clamp01((since - holdSeconds) / fadeSeconds));
            }
            int written = 0;
            Vector3 low = Vector3.zero, high = Vector3.zero;
            for (int k = 0; k < count && written + 2 <= vertices.Length; k++)
            {
                int i = (first + k) % Capacity;
                float age = now - time[i];
                if (age < Lag) continue;
                float t = Hold > 0f ? 1f - k / (float)Mathf.Max(1, count - 1) : Mathf.Clamp01((age - Lag) / Seconds);
                float fade = Hold > 0f ? frozenFade : RibbonMath.Fade(t);
                float moved = Hold > 0f ? 0f : age;

                Vector3 a = inner[i], b = outer[i];
                Vector3 across = b - a;
                Vector3 normal = Vector3.Cross(way[i], across);
                normal = normal.sqrMagnitude > 1e-8f ? normal.normalized : Vector3.up;
                // The trail thins (or spreads) about its middle, and shade drains back to its inner edge.
                float width = 1f - layer.Taper * t;
                Vector3 middle = (a + b) * 0.5f;
                a = middle + (a - middle) * width;
                b = middle + (b - middle) * width;
                if (layer.Drain > 0f) b = Vector3.Lerp(b, a, layer.Drain * t);
                Vector3 shift = Vector3.up * (layer.Rise * moved) + way[i] * (layer.Drift * moved);
                if (layer.Scatter != 0f)
                {
                    float side = RibbonMath.Hash(id[i] * 31) * 2f - 1f;
                    float along = RibbonMath.Hash(id[i] * 17 + 5) - 0.5f;
                    shift += normal * (side * layer.Scatter * moved) + across.normalized * (along * layer.Scatter * moved);
                }
                if (layer.Jitter > 0f) shift += normal * ((RibbonMath.Hash(id[i] * 13 + beat * 7) * 2f - 1f) * layer.Jitter);
                a += shift;
                b += shift;
                // Fire: the outer edge climbs in tongues as it is left, so the burning arc licks upward unevenly. It climbs
                // in widths of its own trail, and only for a moment: a narrow trail seen edge on (the hammer's overhead)
                // stretched by whole metres became a tall thin zigzag that read as lightning (filmed 2026-11-20).
                if (layer.Lick > 0f) b += Vector3.up * (layer.Lick * Mathf.Min(moved, LickSeconds) * (0.15f + RibbonMath.Tongue(id[i], layer.LickEvery)) * across.magnitude);

                Color colour = Color.Lerp(layer.Head, layer.Tail, t);
                colour.a *= fade * gate[i];
                // A bolt takes its kinks from the red of its colour: a new shape on every beat.
                if (bolt) colour.r = 0.55f + 0.45f * RibbonMath.Hash(beat * 3 + id[i] / 4);
                float u = RibbonMath.Along(t, style.U0, style.U1);
                vertices[written] = a;
                vertices[written + 1] = b;
                uvs[written] = style.Swap ? new Vector2(style.V0, u) : new Vector2(u, style.V0);
                uvs[written + 1] = style.Swap ? new Vector2(style.V1, u) : new Vector2(u, style.V1);
                colours[written] = colour;
                colours[written + 1] = colour;
                if (written == 0)
                {
                    low = Vector3.Min(a, b);
                    high = Vector3.Max(a, b);
                }
                else
                {
                    low = Vector3.Min(low, Vector3.Min(a, b));
                    high = Vector3.Max(high, Vector3.Max(a, b));
                }
                written += 2;
            }
            if (written < 4) return 0;
            bounds = new Bounds((low + high) * 0.5f, high - low + Vector3.one * 0.05f);
            return written;
        }
    }
}

using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// What each element is made of, for everything that draws spells. A form asks <see cref="FormFx"/> for a plain
    /// splinter, fang or column in the element's colour; here that becomes a tongue of flame, a jagged length of
    /// lightning, a crystal, a drop, a rock, a leaf, a blade, a ray, a wisp of smoke or a bubble, and is given the
    /// motion of that stuff (flame flickers, lightning jumps, bubbles wobble). The same table says what an impact
    /// throws out and leaves on the ground. One form, eleven substances.
    /// </summary>
    public static class ElementSubstance
    {
        public const int Fire = 0, Lightning = 1, Wind = 2, Ice = 3, Water = 4, Life = 5, Earth = 6, Metal = 7, Light = 8, Dark = 9, Poison = 10;

        // What a splinter flying along +Z, a fang standing on the ground and a column become, by the element's place on the ring.
        private static readonly FormMesh[] Splinters =
        {
            FormMesh.Fireball, FormMesh.Bolt, FormMesh.Gust, FormMesh.Shard, FormMesh.Droplet, FormMesh.Leaf,
            FormMesh.Pebble, FormMesh.Blade, FormMesh.Ray, FormMesh.Cloud, FormMesh.Bubble,
        };
        private static readonly FormMesh[] Fangs =
        {
            FormMesh.Flame, FormMesh.Spike, FormMesh.Spike, FormMesh.Crystal, FormMesh.Spout, FormMesh.Vine,
            FormMesh.Rock, FormMesh.Spike, FormMesh.Spike, FormMesh.Spike, FormMesh.Spike,
        };

        private static int held = -1;
        private static Color heldColour;

        // Domain reload is off: the element of the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            held = -1;
            heldColour = default;
        }

        /// <summary>The player has taken an element: spells drawn in its colour are made of its stuff from now on.</summary>
        public static void Hold(ElementInfo element)
        {
            held = element != null ? element.Index : -1;
            heldColour = element != null ? element.Colour : default;
        }

        /// <summary>Which element a colour is the held element's, at any brightness; -1 when it is some other colour.</summary>
        public static int Of(Color colour) => held >= 0 && SameHue(colour, heldColour) ? held : -1;

        /// <summary>True when two colours are one hue at different brightness.</summary>
        public static bool SameHue(Color a, Color b)
        {
            float topA = Mathf.Max(a.r, Mathf.Max(a.g, a.b)), topB = Mathf.Max(b.r, Mathf.Max(b.g, b.b));
            if (topA < 0.001f || topB < 0.001f) return false;
            return Mathf.Abs(a.r / topA - b.r / topB) + Mathf.Abs(a.g / topA - b.g / topB) + Mathf.Abs(a.b / topA - b.b / topB) < 0.1f;
        }

        /// <summary>The shape a plain piece takes in an element; shapes that are already something stay as they are.</summary>
        public static FormMesh Shape(FormMesh asked, int element)
        {
            if (element < 0 || element >= Splinters.Length) return asked;
            if (asked == FormMesh.Shard) return Splinters[element];
            if (asked == FormMesh.Spike) return Fangs[element];
            if (asked == FormMesh.Pillar) return element == Earth ? FormMesh.Rock : element == Ice ? FormMesh.Crystal : asked;
            return asked;
        }

        /// <summary>How much white a shape takes on the faces turned to the light: steel and ice shine, water and leaves a little, smoke and anything near black not at all.</summary>
        public static float Sheen(FormMesh shape, Color colour)
        {
            if (Mathf.Max(colour.r, Mathf.Max(colour.g, colour.b)) < 0.3f) return 0f;
            switch (shape)
            {
                case FormMesh.Cloud: return 0.06f;
                case FormMesh.Crest:
                case FormMesh.Droplet:
                case FormMesh.Spout: return 0.1f;
                case FormMesh.Vine:
                case FormMesh.Leaf:
                case FormMesh.Rock:
                case FormMesh.Pebble: return 0.14f;
                default: return 0.42f;
            }
        }

        /// <summary>How thin a shape is drawn, as the share of it left out: smoke and moving air are seen through.</summary>
        public static float Thin(FormMesh shape) => shape == FormMesh.Cloud ? 0.42f : shape == FormMesh.Gust ? 0.2f : 0f;

        /// <summary>True for stuff that never holds still.</summary>
        public static bool Moves(FormMesh shape) =>
            shape == FormMesh.Flame || shape == FormMesh.Fireball || shape == FormMesh.Bolt || shape == FormMesh.Cloud ||
            shape == FormMesh.Bubble || shape == FormMesh.Droplet || shape == FormMesh.Spout;

        /// <summary>The motion of the stuff itself, laid over wherever its form has put it: flame flickers upward, lightning jumps to a new shape, smoke swells, water and bubbles wobble.</summary>
        public static void Sway(FormMesh shape, int seed, float time, ref Vector3 scale, ref Quaternion rotation)
        {
            switch (shape)
            {
                case FormMesh.Flame:
                    scale.y *= 0.78f + 0.5f * Noise(seed, time * 11f);
                    scale.x *= 0.9f + 0.2f * Noise(seed + 7, time * 9f);
                    break;
                case FormMesh.Fireball:
                    scale.x *= 0.85f + 0.3f * Noise(seed, time * 12f);
                    scale.y *= 0.85f + 0.3f * Noise(seed + 5, time * 12f);
                    break;
                case FormMesh.Bolt:
                {
                    // A new bolt eighteen times a second: turned about its own length and thrown wider or thinner.
                    int beat = Mathf.FloorToInt(time * 18f) + seed * 31;
                    rotation *= Quaternion.Euler(0f, 0f, Hash(beat) * 360f);
                    float wide = 0.6f + 1.1f * Hash(beat + 13);
                    scale.x *= wide;
                    scale.y *= wide;
                    break;
                }
                case FormMesh.Cloud:
                {
                    float swell = 1f + 0.14f * Mathf.Sin(time * 2.6f + seed);
                    scale.x *= swell;
                    scale.z *= 2f - swell;
                    break;
                }
                default:
                {
                    float wobble = 0.14f * Mathf.Sin(time * 10f + seed * 1.7f);
                    scale.x *= 1f + wobble;
                    scale.y *= 1f - wobble;
                    break;
                }
            }
        }

        /// <summary>
        /// What an impact of the element throws out and leaves: flames standing where it landed, crystals, a spout
        /// and ripples, rocks, blades stuck in the ground, smoke that sinks, bubbles that rise. Lightning leaves nothing.
        /// </summary>
        public static void Burst(in SpellCast cast, Vector3 at, float radius, float weight)
        {
            int element = cast.Element != null ? cast.Element.Index : -1;
            if (element < 0) return;
            Color colour = cast.Colour;
            int thrown = Mathf.Clamp(Mathf.RoundToInt(3f + 3f * weight), 3, 8);
            for (int i = 0; i < thrown; i++)
            {
                Vector3 way = FormRules.OnRing(i, thrown, 1f, Random.value * 40f);
                Vector3 from = at + Vector3.up * 0.5f + way * 0.2f;
                Vector3 to = at + way * (radius * (0.9f + 0.6f * Random.value)) + Vector3.up * (element == Dark ? 0.05f : 0.5f + Random.value);
                FormFx.Fly(FormMesh.Shard, from, to, Quaternion.LookRotation(to - from), Vector3.one * (0.5f + 0.2f * weight), colour, 0.28f + 0.1f * Random.value);
            }
            int stood = Mathf.Clamp(Mathf.RoundToInt(2f + 2f * weight), 2, 5);
            float hold = 0.5f + 0.6f * weight;
            switch (element)
            {
                case Fire:
                case Ice:
                case Life:
                case Earth:
                    for (int i = 0; i < stood; i++)
                    {
                        Vector3 spot = at + FormRules.OnRing(i, stood, radius * 0.55f, Random.value * 90f);
                        float tall = element == Fire ? 0.9f + Random.value * 0.7f : 0.6f + Random.value * 0.6f;
                        FormFx.Erupt(FormMesh.Spike, spot, Quaternion.Euler(0f, Random.value * 360f, 0f), new Vector3(0.7f, tall, 0.7f), colour, 0.06f, hold * (element == Fire ? 0.7f : 1.5f), 0.3f);
                    }
                    if (element == Life) FormFx.Erupt(FormMesh.Flower, at + Vector3.up * 0.05f, Quaternion.Euler(0f, Random.value * 360f, 0f), Vector3.one * 1.2f, new Color(1f, 0.72f, 0.85f), 0.2f, hold * 1.5f, 0.3f);
                    break;
                case Water:
                    FormFx.Erupt(FormMesh.Spout, at, Quaternion.Euler(0f, Random.value * 360f, 0f), new Vector3(0.8f, 1.5f + weight * 0.5f, 0.8f), colour, 0.08f, 0.1f, 0.3f);
                    FormFx.Lay(FormMark.Ring, at, radius * 0.6f, FormKit.Lit(colour, 1.6f), 0.7f, radius * 3.2f);
                    FormFx.Lay(FormMark.Ring, at, radius * 0.2f, FormKit.Lit(colour, 1.3f), 1f, radius * 2.4f);
                    break;
                case Metal:
                    for (int i = 0; i < stood; i++)
                    {
                        Vector3 spot = at + FormRules.OnRing(i, stood, radius * 0.6f, Random.value * 90f);
                        FormFx.Drop(FormMesh.Blade, spot + Vector3.up * 3f, spot + Vector3.up * 0.75f, SigKit.Down * Quaternion.Euler(0f, 0f, Random.value * 360f), Vector3.one * 0.8f, colour, 0.12f, hold * 1.5f, 0.2f);
                    }
                    break;
                case Light:
                    FormFx.Cut(at + Vector3.up * 0.9f, Vector3.right, radius * 4f, 0.3f, FormKit.Lit(colour, 3.4f), 0.22f);
                    FormFx.Cut(at + Vector3.up * 0.9f, Vector3.forward, radius * 2.4f, 0.2f, FormKit.Lit(colour, 3.4f), 0.22f);
                    break;
                case Dark:
                    for (int i = 0; i < stood; i++)
                    {
                        Vector3 spot = at + FormRules.OnRing(i, stood, radius * 0.5f, Random.value * 90f);
                        FormFx.Fly(FormMesh.Cloud, spot + Vector3.up * 1.4f, spot + Vector3.up * 0.1f, Quaternion.identity, new Vector3(0.9f, 0.6f, 0.9f), Shade(colour), 0.7f);
                    }
                    FormFx.Lay(FormMark.Ring, at, radius * 2.8f, FormKit.Lit(colour, 2.2f), 0.3f, 0.3f);
                    break;
                case Poison:
                    for (int i = 0; i < stood; i++)
                    {
                        Vector3 spot = at + FormRules.OnRing(i, stood, radius * 0.5f, Random.value * 90f);
                        FormFx.Fly(FormMesh.Bubble, spot + Vector3.up * 0.1f, spot + Vector3.up * (1.2f + Random.value), Quaternion.LookRotation(Vector3.up), Vector3.one * 0.5f, colour, 0.8f);
                    }
                    FormFx.Lay(FormMark.Disc, at, radius * 2f, FormKit.Lit(colour, 1.1f, 0.6f), 1.4f, radius * 2.6f);
                    break;
                case Wind:
                    for (int i = 0; i < stood; i++)
                    {
                        Vector3 way = FormRules.OnRing(i, stood, 1f);
                        Vector3 around = Vector3.Cross(Vector3.up, way);
                        FormFx.Fly(FormMesh.Gust, at + way * radius * 0.5f + Vector3.up * 0.6f, at + way * radius * 0.9f + around * radius + Vector3.up * 1.2f, Quaternion.LookRotation(around), new Vector3(1.6f, 1.6f, 2f), Pale(colour), 0.3f);
                    }
                    break;
            }
        }

        /// <summary>The element's colour gone almost to black: what Dark's solid things are made of.</summary>
        public static Color Shade(Color colour) => new Color(colour.r * 0.16f, colour.g * 0.1f, colour.b * 0.2f, 1f);

        /// <summary>The element's colour half-way to white: air, foam, the heart of a bolt.</summary>
        public static Color Pale(Color colour) => Color.Lerp(colour, Color.white, 0.55f);

        /// <summary>A number from 0 to 1 that is always the same for the same whole number.</summary>
        public static float Hash(int n)
        {
            float s = Mathf.Sin(n * 12.9898f + 4.1414f) * 43758.547f;
            return s - Mathf.Floor(s);
        }

        // A value that wanders smoothly between hashes: flicker without jumps.
        private static float Noise(int seed, float t)
        {
            int whole = Mathf.FloorToInt(t);
            float part = t - whole;
            part = part * part * (3f - 2f * part);
            return Mathf.Lerp(Hash(whole + seed * 57), Hash(whole + 1 + seed * 57), part);
        }
    }
}

using System;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// The elements' second-pass looks (the Blender worker's batch 1, staging/blender/2026-11-20_elements): each look a
    /// timeline of kit pieces keyed as the look-dev keyed them (a path, a scale, a spin and an orbit, each a few keys with
    /// their easings, and a window when the piece is seen), for one body or for the player. Made by Clover > Build >
    /// Element Looks from elements2_recipe.json and played by <see cref="ElementLooks"/>.
    /// </summary>
    public class ElementLookSet : ScriptableObject
    {
        /// <summary>How a part is drawn: an inked solid in its element's material, or a flat see-through mark lying on the floor.</summary>
        public const byte Solid = 0, Flat = 1;

        /// <summary>A body's shell the look asks the body's own material for: none, steel (Temper), ice (Chill), rime (Rime).</summary>
        public const byte NoShell = 0, Steel = 1, Ice = 2, Rime = 3, Wet = 4;

        /// <summary>Easings as the recipe names them: linear, out, in, inout, outback, step.</summary>
        public const byte Linear = 0, Out = 1, In = 2, InOut = 3, OutBack = 4, Step = 5;

        [Serializable]
        public struct Look
        {
            /// <summary>The recipe's slug: "water_soaked", "metal_temper"...</summary>
            public string Key;
            public float Seconds;
            /// <summary>
            /// While it is kept past its end (an affliction that lasts), the parts that repeat play again from here to the
            /// end, round and round; the rest hold where they ended.
            /// </summary>
            public float LoopFrom;
            /// <summary>Drawn on the player (a surge), not on an enemy.</summary>
            public bool OnPlayer;
            public int FirstPart, PartCount;
            /// <summary>The body's shell and its bands' times: the first band begins, each next one this much later, each taking this long.</summary>
            public byte Shell;
            public float ShellStart, ShellStep, ShellGrow;
            public int ShellBands;
        }

        [Serializable]
        public struct Part
        {
            public Mesh Mesh;
            public Material Material;
            public byte Kind;
            /// <summary>Rides on the body as it moves (on or round it); a part on the ground stays where the look began.</summary>
            public bool Follows;
            /// <summary>Played again while the look is kept past its end (drips, puffs, wisps, glints, mist).</summary>
            public bool Repeats;
            /// <summary>Turned anew every few frames (sparks).</summary>
            public bool Redraw;
            /// <summary>The key pose about the body's feet (or the player's): world-aligned, Unity axes.</summary>
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
            /// <summary>Seen from, and gone at (9 or more: it stays).</summary>
            public float Show, Hide;
            public int PathFirst, PathCount, ScaleFirst, ScaleCount, SpinFirst, SpinCount, OrbitFirst, OrbitCount;
            public Vector3 SpinAxis;
            public Vector3 OrbitCentre;
        }

        [Serializable]
        public struct Key
        {
            public float Time;
            /// <summary>An offset or a scale (xyz), or degrees (x).</summary>
            public Vector3 Value;
            /// <summary>The easing into this key from the one before.</summary>
            public byte Ease;
        }

        public Look[] Looks = new Look[0];
        public Part[] Parts = new Part[0];
        public Key[] Keys = new Key[0];

        /// <summary>A look by its slug; -1 for none.</summary>
        public int Find(string key)
        {
            for (int i = 0; i < Looks.Length; i++)
                if (Looks[i].Key == key) return i;
            return -1;
        }

        /// <summary>An easing of the share <paramref name="x"/> (0 to 1) of the way between two keys.</summary>
        public static float Ease(byte ease, float x)
        {
            x = Mathf.Clamp01(x);
            switch (ease)
            {
                case Out: return 1f - (1f - x) * (1f - x);
                case In: return x * x;
                case InOut: return x * x * (3f - 2f * x);
                case OutBack:
                {
                    const float c = 1.70158f;
                    float y = x - 1f;
                    return 1f + (c + 1f) * y * y * y + c * y * y;
                }
                case Step: return x >= 1f ? 1f : 0f;
                default: return x;
            }
        }

        /// <summary>A channel's value at a time: its first value before its first key, its last after its last.</summary>
        public Vector3 Sample(int first, int count, float time, Vector3 none)
        {
            if (count <= 0) return none;
            if (time <= Keys[first].Time) return Keys[first].Value;
            int last = first + count - 1;
            if (time >= Keys[last].Time) return Keys[last].Value;
            for (int k = first; k < last; k++)
            {
                if (time > Keys[k + 1].Time) continue;
                float span = Mathf.Max(0.0001f, Keys[k + 1].Time - Keys[k].Time);
                return Vector3.LerpUnclamped(Keys[k].Value, Keys[k + 1].Value, Ease(Keys[k + 1].Ease, (time - Keys[k].Time) / span));
            }
            return Keys[last].Value;
        }

        /// <summary>
        /// How far up the body (a share, 0 to 1) a shell covers this many seconds into its look: band after band, each
        /// eased on over its own time.
        /// </summary>
        public static float ShellReveal(in Look look, float age)
        {
            if (look.Shell == NoShell || look.ShellBands <= 0) return 0f;
            float covered = 0f;
            for (int band = 0; band < look.ShellBands; band++)
            {
                float from = look.ShellStart + band * look.ShellStep;
                float share = Ease(Out, (age - from) / Mathf.Max(0.01f, look.ShellGrow));
                if (share <= 0f) break;
                covered = (band + share) / look.ShellBands;
            }
            return covered;
        }
    }
}

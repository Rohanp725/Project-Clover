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
        /// <summary>
        /// How a part is drawn: an inked solid in its element's material, a flat see-through mark lying on the floor, or a
        /// ribbon (a tube along the recipe's points, its u running tail 0 to head 1) drawn on head first and wiped off tail
        /// first by Clover/Spell's _DrawHead and _DrawTail.
        /// </summary>
        public const byte Solid = 0, Flat = 1, Ribbon = 2;

        /// <summary>
        /// A body's shell the look asks the body's own material for: none, steel (Temper), ice (Chill), rime (Rime), wet
        /// (Soaked), stone bound by black seams (Turned to Stone), poisoned green-yellow ice (Venom Rime).
        /// </summary>
        public const byte NoShell = 0, Steel = 1, Ice = 2, Rime = 3, Wet = 4, Stone = 5, Venom = 6;

        /// <summary>Shells the body's ice draws (Clover/Toon's _Ice, in its frost, stone or venom colouring).</summary>
        public static bool IsIceShell(byte shell) => shell == Ice || shell == Rime || shell == Stone || shell == Venom;

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
            /// <summary>
            /// Degrees of yaw the recipe's player faced (toward the main body): a look played turned to a strike's way
            /// (<see cref="ElementLooks.PlayAt"/>) is turned by the strike's yaw less this.
            /// </summary>
            public float FacingYaw;
            /// <summary>
            /// A reaction's look on another body (or the player): where that one stood from the main body in the recipe, so
            /// the game can find the real one there (<see cref="ElementLooks.PlayReaction"/>).
            /// </summary>
            public Vector3 FromMain;
            /// <summary>
            /// Seconds a reaction's look played once stays on past its own length, its held pieces and its shell standing,
            /// for as long as what the reaction does lasts (Turned to Stone stands as stone 3.5 s); 0 ends it at its length.
            /// </summary>
            public float Hold;
        }

        /// <summary>How long a look played once is shown: its own length, or its hold when that is longer.</summary>
        public static float ShownFor(in Look look) => Mathf.Max(look.Seconds, look.Hold);

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
            /// <summary>
            /// Stays while the look is kept but keeps moving (a flame flickering, a cloud churning, a leaf orbiting): its
            /// scale and path go round its last <see cref="CycleSeconds"/>.
            /// </summary>
            public bool Cycles;
            /// <summary>Stays while the look is kept and its spin and orbit run on at their last rate (a lotus turning, a leaf orbiting).</summary>
            public bool RunsOn;
            /// <summary>The key pose about the body's feet (or the player's): world-aligned, Unity axes.</summary>
            public Vector3 Position;
            public Quaternion Rotation;
            public Vector3 Scale;
            /// <summary>Seen from, and gone at (9 or more: it stays).</summary>
            public float Show, Hide;
            public int PathFirst, PathCount, ScaleFirst, ScaleCount, SpinFirst, SpinCount, OrbitFirst, OrbitCount;
            public Vector3 SpinAxis;
            public Vector3 OrbitCentre;
            /// <summary>A ribbon's keys: the share of it drawn on from its tail (x), and the share wiped off from its tail (x).</summary>
            public int DrawFirst, DrawCount, EraseFirst, EraseCount;
            /// <summary>A ribbon's ink: its colour and width in metres (none: no ink), and how much of it is stippled away (dust).</summary>
            public Color InkColour;
            public float InkWidth, Thin;
        }

        /// <summary>The most seconds a cycling part's scale and path go round at the end of its look.</summary>
        public const float CycleSeconds = 1.2f;

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
        /// <summary>Glaze's crystal, stood on its base with its length up, and the ice it is drawn in (none in a set without it).</summary>
        public Mesh Crystal;
        public Material CrystalMaterial;
        public Part[] Parts = new Part[0];
        public Key[] Keys = new Key[0];

        /// <summary>A look by its slug; -1 for none.</summary>
        public int Find(string key)
        {
            for (int i = 0; i < Looks.Length; i++)
                if (Looks[i].Key == key) return i;
            return -1;
        }

        /// <summary>A reaction's look on another body or the player: the key "slug@n" (or "slug@p"), found without making the string.</summary>
        public int FindVariant(string slug, char which)
        {
            if (string.IsNullOrEmpty(slug)) return -1;
            for (int i = 0; i < Looks.Length; i++)
            {
                string key = Looks[i].Key;
                if (key != null && key.Length == slug.Length + 2 && key[slug.Length] == '@' && key[slug.Length + 1] == which
                    && string.CompareOrdinal(key, 0, slug, 0, slug.Length) == 0) return i;
            }
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
        /// An angle channel's degrees (x) at a time, running on past its last key at the rate of its last two keys (a
        /// lotus still turning, a leaf still orbiting while the look is kept).
        /// </summary>
        public float SampleOnward(int first, int count, float time)
        {
            if (count <= 0) return 0f;
            int last = first + count - 1;
            if (count < 2 || time <= Keys[last].Time) return Sample(first, count, time, Vector3.zero).x;
            float span = Keys[last].Time - Keys[last - 1].Time;
            float rate = span > 0.0001f ? (Keys[last].Value.x - Keys[last - 1].Value.x) / span : 0f;
            return Keys[last].Value.x + rate * (time - Keys[last].Time);
        }

        /// <summary>
        /// A part's pose about its look's anchor at its own time <paramref name=t/> (spins and orbits at
        /// <paramref name=angleTime/>): its keyed path, scale, spin, the turn a spark is redrawn in (<paramref name=seed/>
        /// keeps two sparks from turning alike) and its orbit, in the look's own axes, before it is turned or sized.
        /// </summary>
        public void LocalPose(in Part part, float t, float angleTime, int seed, out Vector3 position, out Quaternion rotation, out Vector3 scale)
        {
            position = part.Position + Sample(part.PathFirst, part.PathCount, t, Vector3.zero);
            scale = Vector3.Scale(part.Scale, Sample(part.ScaleFirst, part.ScaleCount, t, Vector3.one));
            rotation = part.Rotation;
            if (part.SpinCount > 0) rotation = Quaternion.AngleAxis(SampleOnward(part.SpinFirst, part.SpinCount, angleTime), part.SpinAxis) * rotation;
            if (part.Redraw)
            {
                // Sparks are turned anew three times a second at most, not every frame (the owner: nothing flickery).
                int redraw = Mathf.FloorToInt(t * 3f) + seed * 7;
                rotation = Quaternion.AngleAxis(ElementSubstance.Hash(redraw) * 360f, Vector3.up) * rotation;
            }
            if (part.OrbitCount > 0)
            {
                var round = Quaternion.AngleAxis(SampleOnward(part.OrbitFirst, part.OrbitCount, angleTime), Vector3.up);
                position = part.OrbitCentre + round * (position - part.OrbitCentre);
                rotation = round * rotation;
            }
        }

        /// <summary>
        /// A part's own time <paramref name="age"/> seconds into its look: the look's, held at its end; for a repeating
        /// part round and round the loop from <see cref="Look.LoopFrom"/> to the end, taken one loop on when its window
        /// reaches past the end (a curl drawn on at the end of the loop is wiped off at the start of the next, never cut);
        /// for a cycling part round its last stretch.
        /// </summary>
        public static float PartTime(in Look look, in Part part, float age, bool once)
        {
            if (age <= look.Seconds || once) return Mathf.Min(age, look.Seconds + 0.001f);
            if (part.Repeats)
            {
                float span = Mathf.Max(0.1f, look.Seconds - look.LoopFrom);
                float t = look.LoopFrom + Mathf.Repeat(age - look.Seconds, span);
                if (t < part.Show && t + span < part.Hide) t += span;
                return t;
            }
            if (part.Cycles)
            {
                float cycle = Mathf.Clamp(look.Seconds - part.Show - 0.4f, 0.2f, CycleSeconds);
                return look.Seconds - cycle + Mathf.Repeat(age - look.Seconds, cycle);
            }
            return look.Seconds + 0.001f;
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

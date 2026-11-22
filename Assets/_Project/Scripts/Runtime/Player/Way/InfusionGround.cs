using Clover.Gameplay;
using Clover.World;
using UnityEngine;

namespace Clover.Player.Way
{
    /// <summary>
    /// The ground a family mark leaves behind its infused strike (way_build_plan.md 2.2; task 5.3):
    ///
    /// - Lying Draw: the drawing cut's arc lies on the floor in the element. Fire keeps the whole cut alight, not a few
    ///   patches, for 6 s (question 6, the owner's answer), with the hazard's own real fire on char; Poison and Life lie
    ///   3 s, laying their affliction on whoever stands in them (as written). Doubled all in.
    /// - Running Draw with Lightning, the "charged path" (the owner's answer, 2026-11-21): the dash's path stays charged
    ///   for 2 s, and each enemy who crosses it takes Static.
    ///
    /// The patches are the world's own <see cref="Hazards"/>, laid along the cut; where they go is plain geometry
    /// (<see cref="ArcPatches"/>, <see cref="LanePatches"/>) so it is tested without a scene.
    /// </summary>
    public static class InfusionGround
    {
        /// <summary>Seconds the charged path stays charged.</summary>
        public const float ChargedSeconds = 2f;
        /// <summary>(P) Static an enemy takes crossing the charged path, once each.</summary>
        public const int ChargedStacks = 1;
        /// <summary>(P) Poison's and Life's lying cut lays one stack each second on whoever stands in it.</summary>
        public const float LieBiteEvery = 1f;
        public const int LieStacks = 1;
        /// <summary>Fire's lying cut bites as the strike's own ground fire does: 5 a bite, 4 s of Burn.</summary>
        public const float FireBiteDamage = 5f, FireBurnSeconds = 4f, FireBiteEvery = 0.35f;
        /// <summary>Metres between the patches' centres along the cut, and each patch's radius: they overlap, so the whole cut is ground.</summary>
        public const float Spacing = 0.9f, PatchRadius = 0.6f;
        /// <summary>Most patches one cut lays.</summary>
        public const int MostPatches = 16;

        /// <summary>
        /// Where the patches go along an arc's cut, at the blade's sweep (85% of the reach), spread evenly from one end to
        /// the other so neighbours overlap. Writes into <paramref name="into"/> and returns how many.
        /// </summary>
        public static int ArcPatches(in Footprint print, Vector3[] into)
        {
            if (into == null || into.Length == 0) return 0;
            float arc = print.Shape == FootprintShape.Ring ? 360f : Mathf.Max(20f, print.ArcDegrees);
            float radius = Mathf.Max(0.5f, print.Reach * 0.85f);
            float length = radius * arc * Mathf.Deg2Rad;
            int count = Mathf.Clamp(Mathf.CeilToInt(length / Spacing) + 1, 2, Mathf.Min(into.Length, MostPatches));
            for (int i = 0; i < count; i++)
            {
                float share = print.Shape == FootprintShape.Ring ? i / (float)count : i / (float)(count - 1);
                Vector3 outward = Turned(print.Forward, (share - 0.5f) * arc);
                into[i] = print.Origin + outward * radius;
            }
            return count;
        }

        /// <summary>Where the patches go along a straight path from one end to the other. Writes into <paramref name="into"/> and returns how many.</summary>
        public static int LanePatches(Vector3 from, Vector3 to, Vector3[] into)
        {
            if (into == null || into.Length == 0) return 0;
            Vector3 line = to - from;
            line.y = 0f;
            int count = Mathf.Clamp(Mathf.CeilToInt(line.magnitude / Spacing) + 1, 1, Mathf.Min(into.Length, MostPatches));
            for (int i = 0; i < count; i++)
                into[i] = from + line * (count == 1 ? 0f : i / (float)(count - 1));
            return count;
        }

        // A direction turned about the vertical, clockwise seen from above as Quaternion.Euler(0, degrees, 0) turns it.
        private static Vector3 Turned(Vector3 forward, float degrees)
        {
            float r = degrees * Mathf.Deg2Rad, c = Mathf.Cos(r), s = Mathf.Sin(r);
            return new Vector3(forward.x * c + forward.z * s, forward.y, forward.z * c - forward.x * s);
        }

        // Filled and read at once, on the main thread, never kept: nothing to reset between plays.
        private static readonly Vector3[] patches = new Vector3[MostPatches];

        /// <summary>Lays what the bend leaves on the ground, on the carrier's footprint. Nothing for a bend that leaves nothing.</summary>
        public static void Lay(in InfusionBend bend, ElementInfo element, in Footprint print)
        {
            if (element == null) return;
            if (bend.LieSeconds > 0f)
            {
                int count = print.Shape == FootprintShape.Path || print.Shape == FootprintShape.Lane
                    ? LanePatches(print.Origin, print.Far, patches)
                    : ArcPatches(print, patches);
                for (int i = 0; i < count; i++) LieAt(patches[i], bend, element);
            }
            if (bend.ChargedPath > 0f)
            {
                int count = LanePatches(print.Origin, print.Far, patches);
                for (int i = 0; i < count; i++)
                    Hazards.Lay(new HazardSpec
                    {
                        Kind = HazardKind.Lightning,
                        Position = patches[i],
                        Radius = PatchRadius,
                        Seconds = bend.ChargedPath,
                        Once = true,
                        Quiet = true,
                        Blow = new SpellBlow { Affliction = AfflictionKind.Static, Stacks = ChargedStacks, Effect = Clover.Core.VfxKind.Shock, Tint = element.Colour },
                    });
            }
        }

        private static void LieAt(Vector3 at, in InfusionBend bend, ElementInfo element)
        {
            if (bend.WholeCut)
            {
                // The strike's own ground fire, the whole cut of it, for as long as the mark says.
                Hazards.Lay(HazardKind.Fire, at, PatchRadius, bend.LieSeconds, FireBiteDamage, 0f, FireBurnSeconds, FireBiteEvery);
                Clover.Player.Forms.StrikeScorch.Lay(at, PatchRadius * 1.35f, bend.LieSeconds * Hazards.FireSecondsScale, 0.5f);
                return;
            }
            // NEEDS (way-infusion-looks): Poison's lying miasma and Life's growth along the cut; the hazard's tinted motes show it.
            Hazards.Lay(new HazardSpec
            {
                Kind = HazardKind.Element,
                Tint = element.Colour,
                Position = at,
                Radius = PatchRadius,
                Seconds = bend.LieSeconds,
                BiteEvery = LieBiteEvery,
                Quiet = true,
                Blow = new SpellBlow { Affliction = element.Affliction, Stacks = LieStacks, Tint = element.Colour },
            });
        }
    }
}

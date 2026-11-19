using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// What an element's blow looks like where it lands on a body, in the approved look (the look-dev's "hit" panels):
    /// Fire's globs bursting into tongues round a ring of soot under a smoke puff; Lightning's jagged impact fork
    /// redrawn and its strike sheet; Wind's paper slips and leaves; Ice's hexagonal crack-web held before the shatter;
    /// Water's after-splash crown; Life's fiddlehead uncoiling into a flower; Earth's slab jut and cubes in dust;
    /// Metal's broken clang arcs, chrome shards and forking sparks; Light's rayed ring standing behind it over lotus
    /// petals; Dark's bite closing on the floor with streaks pulled in; Poison's wound bubbling, its beaded string and
    /// barb, and pits. A blow that lands within 0.6 m of where the same element landed a fifth of a second ago (a patch
    /// biting, two blows of one spell on one body) is drawn once, so the same spot is never drawn over itself.
    /// </summary>
    public static class HitArt
    {
        private const int Remembered = 8;
        private static readonly Vector3[] lastAt = new Vector3[Remembered];
        private static readonly float[] lastTime = new float[Remembered];
        private static readonly int[] lastElement = new int[Remembered];
        private static int next;

        // Domain reload is off: the last play's hits must not hold back this play's.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            for (int i = 0; i < Remembered; i++)
            {
                lastAt[i] = Vector3.zero;
                lastTime[i] = -10f;
                lastElement[i] = -1;
            }
            next = 0;
        }

        /// <summary>
        /// The element landing on a body whose middle is <paramref name="at"/> (about 1.1 m above its feet), struck from
        /// <paramref name="direction"/>. False when the new effects are off, the scene has no art or there is no element.
        /// </summary>
        public static bool Play(ElementInfo element, Vector3 at, Vector3 direction)
        {
            if (!ElementArt.Ready || element == null) return false;
            int index = element.Index;
            float now = Time.time;
            for (int i = 0; i < Remembered; i++)
                if (lastElement[i] == index && now - lastTime[i] < 0.2f && (lastAt[i] - at).sqrMagnitude < 0.36f) return true;
            lastAt[next] = at;
            lastTime[next] = now;
            lastElement[next] = index;
            next = (next + 1) % Remembered;

            Vector3 feet = new Vector3(at.x, at.y - 1.1f, at.z);
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) direction = Vector3.forward;
            float along = ElementArt.Turn(direction);
            float turn = Random.value * 360f;
            switch (index)
            {
                case ElementSubstance.Fire:
                    ElementArt.Erupt(ElementArt.Piece("fire_globs_hit"), feet, Quaternion.Euler(0f, turn, 0f), Vector3.one, 0.05f, 0.18f, 0.16f);
                    ElementArt.Floor(ElementArt.Piece("fire_sootring_hit"), feet, turn, 0.7f, 1f, 1.6f);
                    ElementArt.Sheet(ElementArt.SheetOf("fire_smoke_puff"), at + Vector3.up * 1.3f, 2.2f, 1.1f);
                    break;
                case ElementSubstance.Lightning:
                    // The fork, drawn twice in two shapes: there and gone.
                    ElementArt.Stand(ElementArt.Piece("lightning_fork_hit"), feet, turn, 1f, 0.06f);
                    ElementArt.Stand(ElementArt.Piece("lightning_fork_hit"), feet, turn + 137f, 0.85f, 0.12f);
                    ElementArt.Sheet(ElementArt.SheetOf("lightning_bolt_strike"), at + Vector3.up * 0.4f, 2.8f, 0.45f);
                    break;
                case ElementSubstance.Wind:
                    ElementArt.Fly(ElementArt.Piece("wind_debris_hit"), feet, feet + direction * 1.2f + Vector3.up * 0.4f, Quaternion.Euler(0f, along, 0f), 1f, 0.55f);
                    ElementArt.Sheet(ElementArt.SheetOf("wind_gust"), at + direction * 0.6f, 2.6f, 0.55f);
                    break;
                case ElementSubstance.Ice:
                    // The hold before the shatter: the crack-web on the body and the floor, still.
                    ElementArt.Stand(ElementArt.Piece("ice_crackweb_hit"), feet, 180f, 1f, 0.45f);
                    ElementArt.Sheet(ElementArt.SheetOf("ice_frost_creep"), at, 2.4f, 0.8f);
                    break;
                case ElementSubstance.Water:
                    ElementArt.Erupt(ElementArt.Piece("water_aftersplash_hit"), feet, Quaternion.Euler(0f, turn, 0f), Vector3.one * 1.2f, 0.07f, 0.15f, 0.2f);
                    ElementArt.Sheet(ElementArt.SheetOf("water_splash"), at, 2.8f, 0.7f);
                    break;
                case ElementSubstance.Life:
                    ElementArt.Erupt(ElementArt.Piece("life_fiddle_hit"), at - direction * 0.3f + Vector3.down * 0.4f, Quaternion.Euler(0f, 180f, 0f), Vector3.one * 1.1f, 0.25f, 0.5f, 0.25f);
                    ElementArt.Sheet(ElementArt.SheetOf("life_bloom"), at, 2.2f, 0.9f);
                    break;
                case ElementSubstance.Earth:
                    // A beat of nothing, then all at once.
                    ElementArt.Erupt(ElementArt.Piece("earth_cubes_hit"), feet, Quaternion.Euler(0f, along, 0f), Vector3.one * 0.8f, 0.05f, 0.5f, 0.3f);
                    ElementArt.Sheet(ElementArt.SheetOf("earth_dust_burst"), feet + Vector3.up * 0.8f, 3f, 0.8f);
                    break;
                case ElementSubstance.Metal:
                {
                    ElementArt.Stand(ElementArt.Piece("metal_clang_hit"), feet, 180f, 1f, 0.3f);
                    ElementArt.Stand(ElementArt.Piece("metal_sparks_hit"), feet, along, 1f, 0.14f);
                    for (int i = 0; i < 6; i++)
                    {
                        Vector3 way = Quaternion.Euler(0f, i * 60f + turn, 0f) * Vector3.forward;
                        ElementArt.Fly(ElementArt.Piece(Shard(i)), at, at + way * 2f + Vector3.up * 0.4f, Quaternion.Euler(i * 50f, i * 70f, 0f), 1f, 0.35f);
                    }
                    ElementArt.Sheet(ElementArt.SheetOf("metal_clang_v2"), at, 3f, 0.55f);
                    break;
                }
                case ElementSubstance.Light:
                    // The rayed ring stands behind the body (from the camera), the petals open under it.
                    ElementArt.Stand(ElementArt.Piece("light_prabhavali_hit"), feet + Vector3.forward * 0.35f, 180f, 0.85f, 0.45f);
                    ElementArt.Floor(ElementArt.Piece("light_lotus_hit"), feet, 0f, 0.6f, 0.85f, 0.9f);
                    ElementArt.Sheet(ElementArt.SheetOf("light_flash_v2"), at, 2.6f, 0.5f);
                    break;
                case ElementSubstance.Dark:
                    // Streaks pulled in, the bite closing on the floor; never a burst outward.
                    ElementArt.Floor(ElementArt.Piece("dark_drain_hit"), feet, 0f, 0.8f, 0.8f, 0.7f, 0.12f, true);
                    ElementArt.Floor(ElementArt.Piece("dark_streaks_hit"), feet, turn, 1f, 0.5f, 0.35f);
                    ElementArt.Floor(ElementArt.Piece("dark_bite_hit"), feet, turn, 0.7f, 0.7f, 0.9f, 0.25f, true);
                    ElementArt.Stand(ElementArt.Piece("dark_streaksair_hit"), feet, turn, 0.8f, 0.2f);
                    ElementArt.Sheet(ElementArt.SheetOf("dark_hole_close_v2"), at, 2.4f, 0.6f);
                    break;
                case ElementSubstance.Poison:
                    // The wound bubbles a moment after the cut: the bubbles rise rather than appear.
                    ElementArt.Erupt(ElementArt.Piece("poison_wound_hit"), feet, Quaternion.Euler(0f, 180f, 0f), Vector3.one, 0.12f, 0.5f, 0.15f);
                    ElementArt.Stand(ElementArt.Piece("poison_venomstring_hit"), feet, along, 1f, 0.5f);
                    ElementArt.Floor(ElementArt.Piece("poison_pits_hit"), feet, turn, 1.2f, 1.2f, 1.6f);
                    ElementArt.Floor(ElementArt.Piece("poison_stain_hit"), feet, turn, 0.7f, 0.8f, 1.6f, 0.3f, true);
                    break;
            }
            return true;
        }

        private static string Shard(int i)
        {
            switch (i)
            {
                case 0: return "metal_shard_hit";
                case 1: return "metal_shard_hit_1";
                case 2: return "metal_shard_hit_2";
                case 3: return "metal_shard_hit_3";
                case 4: return "metal_shard_hit_4";
                default: return "metal_shard_hit_5";
            }
        }

        /// <summary>The element whose affliction this is; null for none.</summary>
        public static ElementInfo ElementOf(AfflictionKind kind)
        {
            var all = Elements.All;
            for (int i = 0; i < all.Count; i++)
                if (all[i].Affliction == kind) return all[i];
            return null;
        }
    }
}

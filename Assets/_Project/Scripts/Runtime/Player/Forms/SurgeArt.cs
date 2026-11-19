using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Each element's surge in the approved look (the look-dev's "surge" panels), held round the player for as long as
    /// it lasts, for <see cref="SurgeLook"/>. Kindle: a char ring under the built flames. Overdrive: Raijin's ring of
    /// drums. Tailwind: Fujin's bag-mouth spiral with a column of leaves. Glaze: a knotwork frost ring. Wellspring:
    /// three rings rising, hanging and falling in turn. Regrowth and Bedrock: none (their built leaves and stones).
    /// Temper: eight blades standing edge-out round them over a Greek-key seam. Clarity: a kolam drawn round them, a
    /// halo at their back and a slatted shaft from above that go after a moment. Hunger: an eclipse under them, one
    /// burning crescent and its bead, black streaks and motes pouring in. Fever: two terraced bile bands, ruyi
    /// fumes rising off them, bubbles drifting up. One player surges at a time, so the held pieces are kept here.
    /// </summary>
    public static class SurgeArt
    {
        private const int MostSolids = 10, MostFloors = 6;
        private static readonly int[] solids = Empty(MostSolids);
        private static readonly int[] floors = Empty(MostFloors);
        private static readonly int[] floorPieces = Empty(MostFloors);

        private static int[] Empty(int length)
        {
            var list = new int[length];
            for (int i = 0; i < length; i++) list[i] = -1;
            return list;
        }
        private static int element = -1;
        private static float began;

        // Domain reload is off: nothing held from the last play may be thought still held.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            for (int i = 0; i < MostSolids; i++) solids[i] = -1;
            for (int i = 0; i < MostFloors; i++)
            {
                floors[i] = -1;
                floorPieces[i] = -1;
            }
            element = -1;
            began = 0f;
        }

        /// <summary>
        /// True for the elements whose built surge pieces (hanging blades, a crown of rays, smoke, bubbles, Wind's
        /// spinning streaks and leaves) the art replaces outright. Wind's did not read as wind (the owner's review,
        /// 2026-11-19): its bag-mouth spiral stands alone.
        /// </summary>
        public static bool ReplacesBuilt(ElementInfo info) =>
            info != null && (info.Index == ElementSubstance.Metal || info.Index == ElementSubstance.Light || info.Index == ElementSubstance.Dark || info.Index == ElementSubstance.Poison
                || info.Index == ElementSubstance.Wind);

        /// <summary>Seconds since the surge's pieces were taken; long ago when none are held (the built pieces then show grown).</summary>
        public static float Age(float time) => element >= 0 ? time - began : 1000f;

        /// <summary>Seconds Clarity's shaft and halo stand at its start before they go; its drawn ground stays.</summary>
        public const float RaysSeconds = 1.2f, RaysGo = 0.5f;

        /// <summary>The ink Hunger's turning streaks and motes are drawn in: black, not white (the owner's review).</summary>
        private static readonly Color HungerInk = new Color(0.04f, 0.015f, 0.06f, 1f);

        /// <summary>Takes the surge's pieces. False when the new effects are off or the scene has no art.</summary>
        public static bool Begin(ElementInfo info)
        {
            End();
            if (!ElementArt.Ready || info == null) return false;
            element = info.Index;
            began = Time.time;
            switch (element)
            {
                // The owner's review (2026-11-19): Kindle's cobra and its heat bands took the screen, and only the
                // flames at the base were liked; Bedrock's outer ring of standing stones and Regrowth's great turning
                // triskele go, leaving their built stones (turning on the ground) and leaves.
                case ElementSubstance.Fire: Floor(0, "fire_charrings_affliction"); break;
                case ElementSubstance.Lightning: Solid(0, "lightning_drums_surge"); break;
                case ElementSubstance.Wind: Solid(0, "wind_bagspiral_surge"); break;
                case ElementSubstance.Ice: Floor(0, "ice_knotring_surge"); break;
                case ElementSubstance.Water: Solid(0, "water_rings_surge"); break;
                case ElementSubstance.Life:
                case ElementSubstance.Earth: break;
                case ElementSubstance.Metal:
                    for (int i = 0; i < 8; i++) Solid(i, "metal_crownblade_surge");
                    Floor(0, "metal_meander_surge");
                    break;
                case ElementSubstance.Light:
                    Solid(0, "light_backhalo_surge");
                    Solid(1, "light_shaft_surge");
                    Floor(0, "light_kolam_surge");
                    Floor(1, "light_kolamdots_surge");
                    break;
                case ElementSubstance.Dark:
                    Floor(0, "dark_eclipse_surge");
                    Floor(1, "dark_corona_surge");
                    Floor(2, "dark_bead_surge");
                    Floor(3, "dark_streaks_surge");
                    Floor(4, "dark_motes_surge");
                    FormFx.TintMark(floors[3], HungerInk);
                    FormFx.TintMark(floors[4], HungerInk);
                    break;
                case ElementSubstance.Poison:
                    Floor(0, "poison_band_surge");
                    Floor(1, "poison_band_surge_1");
                    Solid(0, "poison_bandlip_surge");
                    Solid(1, "poison_bandlip_surge_1");
                    Solid(2, "poison_bubbles_surge");
                    for (int i = 0; i < 4; i++) Solid(3 + i, i == 0 ? "poison_ruyi_surge" : i == 1 ? "poison_ruyi_surge_1" : i == 2 ? "poison_ruyi_surge_2" : "poison_ruyi_surge_3");
                    break;
            }
            // Nothing could be had (the pools full, a piece missing): the built surge is shown instead.
            bool any = false;
            for (int i = 0; i < MostSolids; i++) any |= solids[i] >= 0;
            for (int i = 0; i < MostFloors; i++) any |= floors[i] >= 0;
            if (!any) End();
            return any;
        }

        private static void Solid(int slot, string piece) => solids[slot] = ElementArt.Hold(ElementArt.Piece(piece));

        private static void Floor(int slot, string piece)
        {
            floorPieces[slot] = ElementArt.Piece(piece);
            floors[slot] = ElementArt.HoldFloor(floorPieces[slot]);
        }

        /// <summary>Hands every piece back.</summary>
        public static void End()
        {
            for (int i = 0; i < MostSolids; i++)
            {
                FormFx.Release(solids[i], 0.15f);
                solids[i] = -1;
            }
            for (int i = 0; i < MostFloors; i++)
            {
                FormFx.ReleaseMark(floors[i], 0.2f);
                floors[i] = -1;
                floorPieces[i] = -1;
            }
            element = -1;
        }

        /// <summary>Places the pieces round the player's feet.</summary>
        public static void Place(Vector3 feet, float time)
        {
            if (element < 0) return;
            float age = time - began;
            // Everything opens out over its first half second rather than appearing.
            float open = Mathf.Clamp01(age / 0.5f);
            open = 1f - (1f - open) * (1f - open);
            float grown = Mathf.Max(0.02f, open);
            var still = Quaternion.Euler(0f, 180f, 0f);
            switch (element)
            {
                case ElementSubstance.Fire:
                    ElementArt.PlaceFloor(floors[0], floorPieces[0], feet, time * 6f, 0.75f * grown);
                    break;
                case ElementSubstance.Lightning:
                    FormFx.Place(solids[0], feet, Quaternion.Euler(0f, time * 25f, 0f), Vector3.one * (0.85f * grown));
                    break;
                case ElementSubstance.Wind:
                    FormFx.Place(solids[0], feet, Quaternion.Euler(0f, time * -240f, 0f), new Vector3(0.8f, 0.8f * grown, 0.8f));
                    break;
                case ElementSubstance.Ice:
                    ElementArt.PlaceFloor(floors[0], floorPieces[0], feet, 0f, 0.75f * grown);
                    break;
                case ElementSubstance.Water:
                {
                    // The rings rise, hang and fall in turn: the whole set breathes up and down.
                    float rise = 0.25f * Mathf.Sin(time * 2.4f);
                    FormFx.Place(solids[0], feet + Vector3.up * rise, Quaternion.Euler(0f, time * 30f, 0f), new Vector3(0.75f, 0.75f * grown, 0.75f));
                    break;
                }
                case ElementSubstance.Metal:
                    for (int i = 0; i < 8; i++)
                    {
                        float angle = i * 45f;
                        Vector3 round = Quaternion.Euler(0f, angle, 0f) * Vector3.forward;
                        // Edge-out: each blade's flat faces round the circle; one is turned to catch the light.
                        FormFx.Place(solids[i], feet + round * 1.25f, Quaternion.Euler(0f, angle + (i == 2 ? 50f : 90f), 0f), new Vector3(0.85f, 0.85f * grown, 0.85f));
                    }
                    ElementArt.PlaceFloor(floors[0], floorPieces[0], feet, 0f, 0.9f * grown);
                    break;
                case ElementSubstance.Light:
                {
                    // The halo and the shaft come with the surge and then go, narrowing away, so the rays are not there
                    // the whole time; the drawn ground stays (the owner's review, 2026-11-19).
                    float rays = grown * Mathf.Clamp01(1f - (age - RaysSeconds) / RaysGo);
                    FormFx.Place(solids[0], feet + new Vector3(0f, 0.3f, 0.45f), still, Vector3.one * (0.8f * Mathf.Max(0.001f, rays)));
                    FormFx.Place(solids[1], feet, still, new Vector3(Mathf.Max(0.001f, rays), grown, Mathf.Max(0.001f, rays)));
                    ElementArt.PlaceFloor(floors[0], floorPieces[0], feet, 0f, 0.8f * grown);
                    ElementArt.PlaceFloor(floors[1], floorPieces[1], feet, 0f, 0.8f * grown);
                    break;
                }
                case ElementSubstance.Dark:
                    ElementArt.PlaceFloor(floors[0], floorPieces[0], feet, 0f, 0.6f * grown);
                    ElementArt.PlaceFloor(floors[1], floorPieces[1], feet, 0f, 0.6f * grown);
                    ElementArt.PlaceFloor(floors[2], floorPieces[2], feet + new Vector3(1.35f, 0f, -1.1f) * grown, 0f, 0.7f * (1f + 0.15f * Mathf.Sin(time * 11f)));
                    // Streaks and motes pour in from far out: each turn they are drawn a little closer, then begin again.
                    float pour = Mathf.Repeat(time * 0.6f, 1f);
                    ElementArt.PlaceFloor(floors[3], floorPieces[3], feet, time * -30f, 0.65f * (1f - 0.35f * pour));
                    ElementArt.PlaceFloor(floors[4], floorPieces[4], feet, time * -20f, 0.65f * (1f - 0.35f * Mathf.Repeat(pour + 0.5f, 1f)));
                    break;
                case ElementSubstance.Poison:
                    ElementArt.PlaceFloor(floors[0], floorPieces[0], feet, time * 4f, 0.65f * grown);
                    ElementArt.PlaceFloor(floors[1], floorPieces[1], feet, time * -5f, 0.65f * grown);
                    FormFx.Place(solids[0], feet, Quaternion.Euler(0f, time * 4f, 0f), new Vector3(0.65f * grown, 0.65f * grown * (1f + 0.06f * Mathf.Sin(time * 2f)), 0.65f * grown));
                    FormFx.Place(solids[1], feet, Quaternion.Euler(0f, time * -5f, 0f), new Vector3(0.65f * grown, 0.65f * grown * (1f + 0.06f * Mathf.Sin(time * 2.3f + 1f)), 0.65f * grown));
                    FormFx.Place(solids[2], feet + Vector3.up * (0.15f * Mathf.Repeat(time * 0.5f, 1f)), Quaternion.Euler(0f, time * 10f, 0f), Vector3.one * (0.65f * grown));
                    for (int i = 0; i < 4; i++)
                    {
                        float angle = i * 90f + 20f + time * 4f;
                        float lift = Mathf.Repeat(time * 0.4f + i * 0.25f, 1f);
                        FormFx.Place(solids[3 + i], feet + Quaternion.Euler(0f, angle, 0f) * Vector3.forward * 1.7f + Vector3.up * (0.2f + 0.9f * lift), Quaternion.Euler(0f, angle + 180f, 0f), Vector3.one * (0.9f * grown * (1f - 0.4f * lift)));
                    }
                    break;
            }
        }
    }
}

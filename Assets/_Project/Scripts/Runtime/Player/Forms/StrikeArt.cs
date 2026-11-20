using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Each element's infused strike in the approved look (the look-dev's "infused" panels), drawn over the footprint
    /// the carrier covered; the strike's own blows are <see cref="ElementSteel"/>'s and unchanged. Fire's Burning Edge:
    /// a carpet of tongues along the arc with flakes lifting and curling away. Lightning's Flash Cut: the channel
    /// re-struck with new twigs. Wind's Cutting Wind: the crescent combs the floor clean. Ice's Frost Bite: a spike rising
    /// through each one struck in three hard steps. Water's Lash: a fishhook barb on the whip. Life's Grown Into It: a
    /// shoot with cracking roots in each. Earth's Broken Ground: a line of standing stones out to the enemy. Metal's
    /// Struck Plate: a bronze aspis slammed flat, four clang rings, filings snapping into lines. Light's Ray Along the
    /// Edge: a sundial of broad and thin rays with the ground between in hard shadow, every shadow thrown away. Dark's
    /// Taken: a slit in the air zipped shut at both ends, ink legs hanging from it, the floor drained. Poison's Venom
    /// Flick: a scorpion's tail barbing the enemy and gas blooming from it in two lobed terraces.
    /// </summary>
    public static class StrikeArt
    {
        /// <summary>How many strikes the element art has drawn this play, for probes.</summary>
        public static int Played { get; private set; }

        // Domain reload is off: the count starts again each play.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Played = 0;

        /// <summary>Wind's strike, the Blender worker's keyed look (elements2_recipe.json).</summary>
        public const string WindStrikeLook = "wind_strike";

        private static readonly Color FireOrange = new Color(1f, 0.34f, 0.04f);
        private static readonly Color FireRed = new Color(0.85f, 0.16f, 0.04f);

        /// <summary>Draws an element's strike. <paramref name="struck"/> holds the bodies it struck (the first <paramref name="count"/>). False when the new effects are off.</summary>
        public static bool Play(ElementInfo element, in Footprint print, Component first, Component[] struck, int count)
        {
            if (!ElementArt.Ready || element == null) return false;
            Played++;
            Vector3 forward = print.Forward;
            forward.y = 0f;
            forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward;
            Vector3 origin = Ground(print.Origin);
            Vector3 target = first != null ? Ground(first.transform.position) : Ground(print.Far);
            float yaw = ElementArt.Turn(forward);
            float reach = Mathf.Max(1.5f, print.Reach);
            // The elements 3 strikes (Frost Bite's spike, Cutting Wind's crescent, Taken's hooks, Grown Into It's shoots,
            // Broken Ground's slabs), about the player and on each one struck, in place of the pieces below.
            if (ElementMoves.PlayStrike(element.Index, Ground(print.Origin) - Vector3.up * 0.05f, forward, struck, count)) return true;
            switch (element.Index)
            {
                case ElementSubstance.Fire:
                {
                    // The carpet keeps burning along the arc; flakes lift off it and curl away.
                    float arc = Mathf.Max(60f, print.ArcDegrees);
                    for (int i = 0; i < 9; i++)
                    {
                        float t = i / 8f;
                        Vector3 way = Quaternion.Euler(0f, (t - 0.5f) * arc, 0f) * forward;
                        for (int ring = 1; ring <= 2; ring++)
                        {
                            Vector3 at = origin + way * (reach * (0.45f + 0.5f * ring) * (0.9f + 0.2f * Random.value));
                            FormFx.Erupt(FormMesh.Flame, at, Quaternion.Euler(0f, Random.value * 360f, 0f), new Vector3(0.6f, 0.9f + 0.8f * Random.value, 0.6f),
                                (i + ring) % 3 == 0 ? FireRed : FireOrange, 0.05f, 0.3f + 0.3f * Random.value, 0.25f, 0.03f);
                        }
                    }
                    ElementArt.Stand(ElementArt.Piece("fire_flakes_infused"), origin + forward * (reach + 0.6f) + Vector3.up * 0.4f, yaw - 45f, 1f, 0.6f);
                    ElementArt.Sheet(ElementArt.SheetOf("fire_smoke_puff"), origin + forward * reach + Vector3.up * 2.4f, 2.6f, 1.4f);
                    break;
                }
                case ElementSubstance.Lightning:
                {
                    // The channel re-struck along the lunge, and the strike where it ends.
                    float length = Mathf.Max(2f, print.Reach);
                    ElementArt.Stand(ElementArt.Piece("lightning_restrike_infused"), Ground(print.Origin + forward * (length * 0.5f)), ElementArt.Turn(forward, 90f), length / 2.87f, 0.12f);
                    ElementArt.Sheet(ElementArt.SheetOf("lightning_bolt_strike"), Ground(print.Far) + Vector3.up * 1.4f, 3f, 0.5f);
                    ElementArt.Floor(ElementArt.Piece("lightning_fern_spell"), Ground(print.Far), Random.value * 360f, 0.7f, 0.9f, 1.6f, 0.08f, true);
                    break;
                }
                case ElementSubstance.Wind:
                {
                    // The Blender worker's strike (design B of the wind research): a needle-tipped crescent of air drawn on
                    // with the blow and eroding from its tail, parting round the one struck and blowing on past the arc
                    // in curls, the floor combed along it; turned to the strike's way and fitted to how far the one struck
                    // stood (the recipe's stood 1.7 m off). Nothing white, nothing cut off.
                    float off = first != null ? Vector3.Distance(origin, target) : reach;
                    if (ElementLooks.PlayAt(ElementLooks.Find(WindStrikeLook), origin, Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg, Mathf.Clamp(off / 1.7f, 0.85f, 1.6f)))
                        break;
                    ElementArt.Floor(ElementArt.Piece("wind_comb_infused"), origin + forward * (reach * 0.8f), ElementArt.Turn(forward, 180f), 0.8f, 0.9f, 1.4f, 0.15f, true);
                    // The crescent's gust is carried on past the reach and blows apart there, never cut off at a line;
                    // the streaks fan out and are gone over a longer way (the owner's review, 2026-11-19).
                    ElementArt.BlowSheet(ElementArt.SheetOf("wind_gust"), origin + forward * reach + Vector3.up * 0.8f, 4f, 1.05f, forward * 3f, 1.4f, ElementSubstance.WindDust);
                    for (int i = 0; i < 5; i++)
                    {
                        Vector3 way = Quaternion.Euler(0f, (i - 2) * 24f, 0f) * forward;
                        Vector3 from = origin + way * 1f + Vector3.up * (0.6f + 0.2f * i);
                        FormFx.Fly(FormMesh.Gust, from, from + way * 7.5f + Vector3.up * (0.3f * (i - 2)), Quaternion.LookRotation(way), new Vector3(4f, 4f, 3f), new Color(0.82f, 0.84f, 0.78f), 0.5f, 0.03f);
                    }
                    break;
                }
                case ElementSubstance.Ice:
                    // A spike up through each one struck, in three hard steps.
                    for (int i = 0; i < count; i++)
                    {
                        Vector3 at = Ground(struck[i].transform.position);
                        ElementArt.Erupt(ElementArt.Piece("ice_stepspike_infused"), at, Quaternion.Euler(0f, Random.value * 360f, 0f), Vector3.one * 1.15f, 0.12f, 0.75f, 0.2f);
                    }
                    ElementArt.Sheet(ElementArt.SheetOf("ice_frost_creep"), target + Vector3.up * 0.7f, 3f, 0.8f);
                    break;
                case ElementSubstance.Water:
                    ElementArt.Stand(ElementArt.Piece("water_hook_infused"), target + Vector3.up * 0.6f, yaw, 1.4f, 0.45f);
                    ElementArt.Sheet(ElementArt.SheetOf("water_splash"), target + Vector3.up * 0.9f, 3f, 0.7f);
                    break;
                case ElementSubstance.Life:
                    for (int i = 0; i < count; i++)
                        ElementArt.Erupt(ElementArt.Piece("life_shoot_infused"), Ground(struck[i].transform.position), Quaternion.Euler(0f, 180f, 0f), Vector3.one * 0.8f, 0.3f, 1f, 0.35f);
                    ElementArt.Sheet(ElementArt.SheetOf("life_bloom"), target + Vector3.up * 0.9f, 2.6f, 1f);
                    break;
                case ElementSubstance.Earth:
                    // Standing stones in a line out to the enemy.
                    ElementArt.Erupt(ElementArt.Piece("earth_line_infused"), origin + forward * 2.2f, Quaternion.Euler(0f, ElementArt.Turn(forward, 180f), 0f), Vector3.one, 0.07f, 1.6f, 0.4f);
                    ElementArt.Sheet(ElementArt.SheetOf("earth_dust_burst"), origin + forward * 2.5f + Vector3.up * 0.8f, 4f, 0.9f);
                    break;
                case ElementSubstance.Metal:
                    // The aspis slams down, rings step out from it, filings snap into lines.
                    ElementArt.Drop(ElementArt.Piece("metal_aspis_infused"), target + Vector3.up * 2.4f, target + Vector3.up * 0.05f, Quaternion.Euler(0f, 180f, 0f), 1f, 0.1f, 0.9f, 0.2f);
                    ElementArt.Floor(ElementArt.Piece("metal_clangrings_infused"), target, 0f, 0.45f, 1f, 1.1f);
                    ElementArt.Floor(ElementArt.Piece("metal_filings_infused"), target, 0f, 1f, 1f, 1.6f);
                    ElementArt.Sheet(ElementArt.SheetOf("metal_clang_v2"), target + Vector3.up * 1f, 3.4f, 0.6f);
                    break;
                case ElementSubstance.Light:
                {
                    // The dial's point is at the player; it fans out ahead, the ground between its rays in shadow.
                    float dial = ElementArt.Turn(forward, 180f);
                    ElementArt.Floor(ElementArt.Piece("light_dialshade_infused"), ElementArt.Anchor(origin, dial, new Vector3(0f, 0f, 3f)), dial, 0.8f, 0.8f, 1f, 0.2f, true);
                    ElementArt.Floor(ElementArt.Piece("light_dial_infused"), ElementArt.Anchor(origin, dial, new Vector3(-0.35f, 0f, 2.9f)), dial, 0.8f, 0.8f, 1f, 0.2f, true);
                    ElementArt.Sheet(ElementArt.SheetOf("light_flash_v2"), origin + Vector3.up * 1.1f + forward * 0.5f, 2f, 0.4f);
                    for (int i = 0; i < count; i++)
                    {
                        Vector3 feet = Ground(struck[i].transform.position);
                        ElementArt.Shadow(feet, feet - origin, 3.5f, 1f, 1f, 0.12f);
                    }
                    break;
                }
                case ElementSubstance.Dark:
                {
                    // A slit in the air across the cut, ink legs hanging from its lip, the floor drained under it.
                    Vector3 middle = origin + forward * Mathf.Min(2.2f, reach);
                    float across = ElementArt.Turn(forward, 180f);
                    ElementArt.Stand(ElementArt.Piece("dark_slit_infused"), middle + Vector3.up * 0.7f, across, 0.9f, 0.5f);
                    ElementArt.Stand(ElementArt.Piece("dark_inklegs_infused"), middle, across, 0.9f, 0.5f);
                    ElementArt.Floor(ElementArt.Piece("dark_drain_infused"), middle, across, 0.8f, 0.8f, 0.8f, 0.15f, true);
                    ElementArt.Sheet(ElementArt.SheetOf("dark_hole_close_v2"), middle + Vector3.up * 1f, 2.6f, 0.55f);
                    break;
                }
                case ElementSubstance.Poison:
                {
                    // The tail barbs the enemy; gas blooms from the barb in two lobed terraces and the floor pits.
                    Vector3 far = origin + forward * 5f;
                    ElementArt.Stand(ElementArt.Piece("poison_scorpion_infused"), origin + forward * 2.6f, yaw, 1.2f, 0.6f);
                    ElementArt.Erupt(ElementArt.Piece("poison_bloomlip_infused"), far, Quaternion.Euler(0f, yaw, 0f), Vector3.one * 0.7f, 0.4f, 2.2f, 0.4f);
                    ElementArt.Erupt(ElementArt.Piece("poison_bloomlip_infused_1"), far, Quaternion.Euler(0f, yaw, 0f), Vector3.one * 0.7f, 0.5f, 2.1f, 0.4f);
                    ElementArt.Floor(ElementArt.Piece("poison_bloomgas_infused"), far, yaw, 0.5f, 0.75f, 3f, 0.3f, true);
                    ElementArt.Floor(ElementArt.Piece("poison_bloomgas_infused_1"), far, yaw, 0.5f, 0.75f, 3f, 0.3f, true);
                    ElementArt.Floor(ElementArt.Piece("poison_pits_infused"), origin + forward * 3f, yaw, 1.2f, 1.2f, 2.5f);
                    break;
                }
            }
            return true;
        }

        private static Vector3 Ground(Vector3 at) => new Vector3(at.x, at.y + 0.05f, at.z);
    }
}

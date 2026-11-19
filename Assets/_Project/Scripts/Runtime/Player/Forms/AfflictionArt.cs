using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// How each affliction is seen on a body in the approved look (the look-dev's "affliction" panels), called where
    /// <see cref="AfflictionLook"/> is, a few times a second while it lasts; each call puts down pieces that look after
    /// themselves. Burn: tongues climbing the body in threes over a char of stepped broken rings, smoke off the top.
    /// Static: arcs crawling only along the edges and limbs, redrawn every few frames. Unsteady: a pinned leaf and
    /// streaming strips. Chill: frost crust climbing the legs in stepped tiers. Soaked: a wet sheen, necking drips,
    /// draining rings. Seeded: a sprout bursting from the shoulder in two pulses. Tremor: a heaved ring of ground and
    /// stones thrown up out of it that fall (the body itself is shaken by <see cref="Clover.Gameplay.Enemy"/>).
    /// Sundered: bronze plates on the body, a white-hot notch, the molten vein to an ankle stud, shake marks and filings.
    /// Marked: a turning sun-wheel under it and a pencil shaft on its head. Cursed: the floor drained, a small dark mark
    /// centred under it, ink rings sinking off it. Poison: pocked skin, bubbles bursting on
    /// it, a pitted stain and a hooked fume.
    /// </summary>
    public static class AfflictionArt
    {
        /// <summary>How many beats of an affliction the element art has drawn this play, by affliction, for probes.</summary>
        private static readonly int[] shown = new int[16];

        public static int Shown(AfflictionKind kind) => (int)kind >= 0 && (int)kind < shown.Length ? shown[(int)kind] : 0;

        // The kit's climbing tongues lean (top over bottom, in the kit's own axes): this stands them upright.
        private static readonly Quaternion ClimbUpright = Quaternion.FromToRotation(new Vector3(-0.42f, 2.72f, 0.16f), Vector3.up);

        /// <summary>Nerve Fire's jolts drawn this play, for probes.</summary>
        public static int Jolts { get; private set; }

        /// <summary>
        /// Nerve Fire's jolt at a bite of poison (the owner's review, 2026-11-19: small jolts chaining over the body at
        /// each tick, not one big burst): Lightning's static drawn three times over the body in quick turns, each a
        /// little smaller and turned on from the last, there and gone within a fifth of a second. False when the new
        /// effects are off.
        /// </summary>
        public static bool Jolt(Vector3 feet, int beat)
        {
            if (!ElementArt.Ready) return false;
            Jolts++;
            int crawl = ElementArt.Piece("lightning_crawl_affliction");
            float turn = ElementSubstance.Hash(beat) * 360f;
            for (int link = 0; link < 3; link++)
                ElementArt.Stand(crawl, feet + Vector3.up * (0.12f * link), turn + link * 115f, 0.95f - 0.15f * link, 0.07f + 0.06f * link);
            return true;
        }

        // Domain reload is off: the counts start again each play.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            System.Array.Clear(shown, 0, shown.Length);
            Jolts = 0;
        }

        /// <summary>Puts down one beat of an affliction's look. False when the new effects are off or the scene has no art.</summary>
        public static bool Show(AfflictionKind kind, int depth, bool frozen, Vector3 feet, int beat)
        {
            if (!ElementArt.Ready) return false;
            if ((int)kind >= 0 && (int)kind < shown.Length) shown[(int)kind]++;
            float jitter = (ElementSubstance.Hash(beat) - 0.5f) * 40f;
            float more = 1f + 0.08f * depth;
            switch (kind)
            {
                case AfflictionKind.Burn:
                    // Each tongue lives a fraction of a second; the next is drawn turned and stretched a little differently,
                    // stood upright first: the kit's tongues lean, and turned to the camera they always leaned up and to
                    // the right (the owner's review, 2026-11-19).
                    ElementArt.Erupt(ElementArt.Piece("fire_climb_affliction"), feet, Quaternion.Euler(0f, 180f + jitter * 2f, 0f) * ClimbUpright,
                        new Vector3(more, (0.85f + 0.3f * ElementSubstance.Hash(beat + 3)) * more, more), 0.06f, 0.22f, 0.12f);
                    if (beat % 4 == 0) ElementArt.Floor(ElementArt.Piece("fire_charrings_affliction"), feet, beat * 37f, 0.85f, 1f, 1.8f, 0.35f, true);
                    if (beat % 5 == 1) ElementArt.Sheet(ElementArt.SheetOf("fire_smoke_puff"), feet + Vector3.up * 3.1f, 1.8f, 1.3f);
                    return true;
                case AfflictionKind.Static:
                    // There for a frame or two, redrawn in a new place: no tween.
                    ElementArt.Stand(ElementArt.Piece("lightning_crawl_affliction"), feet, ElementSubstance.Hash(beat + 1) * 360f, 1f, 0.12f);
                    ElementArt.Stand(ElementArt.Piece("lightning_crawl_affliction"), feet, ElementSubstance.Hash(beat + 7) * 360f, 0.9f, 0.08f);
                    return true;
                case AfflictionKind.Unsteady:
                    ElementArt.Stand(ElementArt.Piece("wind_pinned_affliction"), feet, 180f + jitter * 0.5f, 1f, 0.36f);
                    if (beat % 3 == 0)
                    {
                        Vector3 from = feet + Vector3.up * (0.4f + ElementSubstance.Hash(beat + 2));
                        Vector3 along = Quaternion.Euler(0f, beat * 53f, 0f) * Vector3.forward;
                        FormFx.Fly(FormMesh.Leaf, from, from + along * 1.6f + Vector3.up * 0.6f, Quaternion.LookRotation(along), Vector3.one * 0.4f, new Color(0.5f, 0.42f, 0.16f), 0.45f, 0.02f);
                    }
                    return true;
                case AfflictionKind.Chill:
                    if (frozen)
                    {
                        ElementArt.Erupt(ElementArt.Piece("ice_tiers_affliction"), feet, Quaternion.Euler(0f, beat * 60f, 0f), new Vector3(1.9f, 2.6f, 1.9f), 0.05f, 0.5f, 0.12f);
                        ElementArt.Erupt(ElementArt.Piece("ice_stepspike_infused"), feet + FormRules.OnRing(0, 1, 0.55f, beat * 71f), Quaternion.Euler(0f, beat * 33f, 0f), Vector3.one * 0.9f, 0.05f, 0.5f, 0.12f);
                    }
                    // Grows in steps: one tier more each beat it lies deeper, held dead still.
                    else ElementArt.Erupt(ElementArt.Piece("ice_tiers_affliction"), feet, Quaternion.Euler(0f, beat * 60f, 0f), new Vector3(1.5f, 1.2f * more, 1.5f), 0.04f, 0.45f, 0.08f);
                    return true;
                case AfflictionKind.Soaked:
                    ElementArt.Stand(ElementArt.Piece("water_wet_affliction"), feet, 180f, 0.9f, 0.4f);
                    if (beat % 2 == 0)
                    {
                        Vector3 from = feet + FormRules.OnRing(0, 1, 0.35f, beat * 71f) + Vector3.up * 1.4f;
                        FormFx.Fly(FormMesh.Droplet, from, from + Vector3.down * 1.3f, Quaternion.LookRotation(Vector3.down), Vector3.one * 0.5f, new Color(0.08f, 0.6f, 0.62f), 0.45f, 0.02f);
                    }
                    return true;
                case AfflictionKind.Seeded:
                    // From the shoulder nearest the camera, in two pulses.
                    if (beat % 2 == 0)
                        ElementArt.Erupt(ElementArt.Piece("life_sprout_affliction"), feet + new Vector3(0.22f, 1.35f, -0.12f), Quaternion.Euler(-10f, 180f + jitter, 0f),
                            Vector3.one * (beat % 4 == 0 ? 1f : 1.25f) * more, 0.18f, 0.5f, 0.2f);
                    else FormFx.Erupt(FormMesh.Vine, feet + FormRules.OnRing(0, 1, 0.6f, beat * 77f), Quaternion.Euler(0f, beat * 40f, 0f), new Vector3(0.6f, 0.7f * more, 0.6f), new Color(0.12f, 0.5f, 0.08f), 0.15f, 0.4f, 0.2f, 0.025f);
                    return true;
                case AfflictionKind.Tremor:
                {
                    // The body is shaken (the enemy's own shake); the ground jolts and stones are thrown up and out from
                    // its feet and fall, never drawn in toward it (the owner's review, 2026-11-19). The heaved ring
                    // drops back at once, so it is not seen closing in on the body.
                    if (beat % 2 == 0) ElementArt.Erupt(ElementArt.Piece("earth_heave_affliction"), feet, Quaternion.Euler(0f, beat * 45f, 0f), new Vector3(1f, more, 1f), 0.05f, 0.18f, 0.04f);
                    for (int stone = 0; stone < 2; stone++)
                    {
                        Vector3 out_ = FormRules.OnRing(0, 1, 1f, beat * 67f + stone * 160f);
                        float up = 3f + 1.2f * ElementSubstance.Hash(beat * 3 + stone);
                        FormFx.Toss(FormMesh.Pebble, feet + out_ * 0.45f + Vector3.up * 0.05f, out_ * (1.6f + 0.3f * depth) + Vector3.up * up,
                            Quaternion.Euler(beat * 30f, beat * 50f + stone * 90f, 0f), Vector3.one * (0.4f + 0.06f * stone), new Color(0.3f, 0.27f, 0.23f), 0.2f, 0.03f);
                    }
                    return true;
                }
                case AfflictionKind.Sundered:
                {
                    // Plates fitted to the body on the side the camera sees, the notch on the largest, the vein down
                    // the leg to a stud at the ankle; shake marks either side of the head show it ringing.
                    float hold = 0.45f;
                    ElementArt.Stand(ElementArt.Piece("metal_plate_affliction"), feet + new Vector3(0.05f, 1.05f, -0.32f), 180f, 1f, hold);
                    ElementArt.Stand(ElementArt.Piece("metal_plate_affliction_1"), feet + new Vector3(-0.28f, 1.35f, -0.25f), 160f, 1f, hold);
                    ElementArt.Stand(ElementArt.Piece("metal_plate_affliction_2"), feet + new Vector3(0.18f, 0.55f, -0.28f), 200f, 1f, hold);
                    ElementArt.Stand(ElementArt.Piece("metal_notch_affliction"), feet + new Vector3(0.05f, 1.25f, -0.42f), 180f, 1.3f, hold);
                    ElementArt.Stand(ElementArt.Piece("metal_ichor_affliction"), feet + new Vector3(0.12f, 0.02f, -0.3f), 180f, 1f, hold);
                    ElementArt.Stand(ElementArt.Piece("metal_stud_affliction"), feet + new Vector3(0.12f, 0.02f, -0.3f), 180f, 1.4f, hold);
                    if (beat % 2 == 0) ElementArt.Stand(ElementArt.Piece("metal_ring_affliction"), feet + Vector3.up * 1.75f, 180f, 0.9f + 0.15f * ElementSubstance.Hash(beat), 0.12f);
                    if (beat % 4 == 0) ElementArt.Floor(ElementArt.Piece("metal_filings_affliction"), feet, 0f, 0.9f, 1f, 1.8f);
                    return true;
                }
                case AfflictionKind.Marked:
                    // The wheel turns under it; the shaft lands on its head.
                    if (beat % 3 == 0) ElementArt.Floor(ElementArt.Piece("light_wheel_affliction"), feet, beat * 15f, 1f, 1f, 1.5f, 0f, true, Color.white, 40f);
                    ElementArt.Stand(ElementArt.Piece("light_pencil_affliction"), feet + Vector3.up * 1.9f, 180f, 1f, 0.4f);
                    ElementArt.Stand(ElementArt.Piece("light_glint_affliction"), feet + Vector3.up * 2.05f, 180f, 0.8f, 0.2f);
                    return true;
                case AfflictionKind.Cursed:
                {
                    // The floor drained round it and a small dark mark centred under it, ink rings sinking off it. The
                    // owner wanted the shadow reaching for the player (and its hand) gone, and a small centred mark under
                    // the cursed one instead (2026-11-19).
                    if (beat % 3 == 0)
                    {
                        ElementArt.Floor(ElementArt.Piece("dark_drain_affliction"), feet, 0f, 1f, 1f, 1.4f, 0.2f, true);
                        ElementArt.Floor(ElementArt.Piece("dark_hole_spell"), feet, beat * 20f, 0.42f, 0.45f, 1.4f, 0.15f, true);
                    }
                    int ring = beat % 3;
                    Vector3 from = feet + FormRules.OnRing(0, 1, 0.3f, beat * 83f) + Vector3.up * 1.3f;
                    ElementArt.Drop(ElementArt.Piece(ring == 0 ? "dark_inkring_affliction" : ring == 1 ? "dark_inkring_affliction_1" : "dark_inkring_affliction_2"), from, from + Vector3.down * 1.1f, Quaternion.identity, 0.8f, 0.8f, 0.1f, 0.2f);
                    return true;
                }
                case AfflictionKind.Poison:
                {
                    ElementArt.Stand(ElementArt.Piece("poison_pocks_affliction"), feet + new Vector3(0f, 0.55f, -0.3f), 180f, 1f, 0.45f);
                    int burst = beat % 4;
                    Vector3 on = feet + FormRules.OnRing(0, 1, 0.32f, 150f + beat * 47f) + Vector3.up * (0.5f + 0.9f * ElementSubstance.Hash(beat + 5));
                    ElementArt.Erupt(ElementArt.Piece(burst == 0 ? "poison_burst_affliction" : burst == 1 ? "poison_burst_affliction_1" : burst == 2 ? "poison_burst_affliction_2" : "poison_burst_affliction_3"),
                        on, Quaternion.Euler(-60f, 180f + jitter, 0f), Vector3.one * 1.4f, 0.25f, 0.15f, 0.05f);
                    if (beat % 4 == 0)
                    {
                        ElementArt.Floor(ElementArt.Piece("poison_stain_affliction"), feet, beat * 29f, 0.8f, 1f, 1.8f, 0.4f, true);
                        ElementArt.Floor(ElementArt.Piece("poison_pits_affliction"), feet + new Vector3(0.2f, 0f, -0.2f), beat * 29f, 1f, 1f, 1.8f);
                    }
                    if (beat % 5 == 2)
                    {
                        // One hooked fume rises off the head, slow, and bends back.
                        Vector3 head = feet + Vector3.up * 1.9f;
                        ElementArt.Fly(ElementArt.Piece("poison_ruyi_affliction"), head, head + Vector3.up * 0.9f, Quaternion.Euler(0f, 180f + jitter, 0f), 0.9f, 1.4f);
                    }
                    return true;
                }
            }
            return false;
        }
    }
}

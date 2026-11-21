using Clover.Core;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// How each affliction is seen on a body in the approved look (the look-dev's "affliction" panels), called where
    /// <see cref="AfflictionLook"/> is, a few times a second while it lasts; each call puts down pieces that look after
    /// themselves. Burn: the body burns in real flame (<see cref="BodyFlames"/>) over a char of stepped broken rings,
    /// smoke off the top. Static: arcs crawling along the edges and limbs, held on until the next is drawn, and bolts
    /// jumping off the body outward. Unsteady: a pinned leaf and streaming strips. Chill: frost crust climbing the legs
    /// in stepped tiers. Soaked: a wet sheen, necking drips, draining rings. Seeded: a sprout bursting from the shoulder
    /// in two pulses. Tremor: dust kicked up at the feet and grit hopping off the ground (the body itself is shaken by
    /// <see cref="Clover.Gameplay.Enemy"/>). Sundered: bronze plates on the body, a white-hot notch, the molten vein to an
    /// ankle stud, shake marks and filings. Marked: a turning sun-wheel under it and a pencil shaft on its head. Cursed:
    /// a black hole at its feet, turning, and dark energy pulled off the body down into it. Poison: pocked skin, bubbles
    /// bursting on it, a pitted stain and a hooked fume.
    /// </summary>
    public static class AfflictionArt
    {
        /// <summary>How many beats of an affliction the element art has drawn this play, by affliction, for probes.</summary>
        private static readonly int[] shown = new int[16];

        public static int Shown(AfflictionKind kind) => (int)kind >= 0 && (int)kind < shown.Length ? shown[(int)kind] : 0;

        // The kit's climbing tongues lean (top over bottom, in the kit's own axes): this stands them upright.
        private static readonly Quaternion ClimbUpright = Quaternion.FromToRotation(new Vector3(-0.42f, 2.72f, 0.16f), Vector3.up);

        /// <summary>
        /// Seconds a Static crawl is held: a little past the next beat of its look (each affliction carried takes its
        /// turn once in <see cref="AfflictionLook.Interval"/>), so the body never blinks dark between redraws.
        /// </summary>
        public static float StaticHold(int depth) => AfflictionLook.Interval(depth) * 1.12f;
        /// <summary>A bolt jumping off a charged body: metres long, seconds before it is gone, and how far out from the body's middle it starts.</summary>
        public const float ArcLength = 1.15f, ArcSeconds = 0.16f, BodyRadius = 0.28f;
        // The kit's jump arc is about this long (metres) at scale one.
        private const float JumpPieceLength = 3.2f;
        /// <summary>The curse's dark: near-black purple, never bright (the owner, 2026-11-20).</summary>
        public static readonly Color CurseInk = new Color(0.012f, 0.004f, 0.02f), CurseDeep = new Color(0.03f, 0.008f, 0.05f);

        /// <summary>
        /// Where Unity and the Blender worker each made a look, which is drawn: the code's (Burn's real flame field,
        /// Static's crawl and jumping arcs), the Blender worker's keyed look, or both at once.
        /// </summary>
        public const byte CodeLook = 0, KitLook = 1, BothLooks = 2;

        /// <summary>
        /// Filmed side by side on the same group of five (2026-11-20). Burn: the flame field (<see cref="BodyFlames"/>),
        /// thin licking tongues banded red to yellow, read as real flickering flame where the Blender worker's tongues read
        /// as large pale cones, so the flame field is kept. Static: both, the code's crawl round and over the body (which
        /// the owner called decent) with the Blender worker's cyan arcs jumping off it to the floor and the air (the
        /// addition the owner asked for) in place of the code's near-white jump arcs. Tuning points a probe may set; put
        /// back each play.
        /// </summary>
        public static byte BurnStyle = DefaultBurnStyle, StaticStyle = DefaultStaticStyle;
        public const byte DefaultBurnStyle = CodeLook, DefaultStaticStyle = BothLooks;

        /// <summary>The Blender worker's Burn (flame tongues ringing the body), drawn when <see cref="BurnStyle"/> asks for it.</summary>
        public const string BurnKitLook = "fire_burn";

        /// <summary>
        /// The second-pass look an affliction wears (the Blender worker's batches 1 and 2, <see cref="ElementLooks"/>);
        /// null for one that has none (Burn's is BodyFlames'). Marked's is elements 4's one beam; its sun-wheel below is
        /// drawn only in a scene without the built looks.
        /// </summary>
        public static string LookOf(AfflictionKind kind) => LookOf(kind, 3);

        /// <summary>The same, for a depth: Unsteady's look has its curls and leaves by stacks (one, three, five).</summary>
        public static string LookOf(AfflictionKind kind, int depth)
        {
            switch (kind)
            {
                case AfflictionKind.Soaked: return "water_soaked";
                case AfflictionKind.Sundered: return "metal_sundered";
                case AfflictionKind.Poison: return "poison_poisoned";
                case AfflictionKind.Seeded: return "life_seeded";
                case AfflictionKind.Cursed: return "dark_cursed";
                case AfflictionKind.Chill: return "ice_chill";
                case AfflictionKind.Tremor: return "earth_tremor";
                case AfflictionKind.Static: return StaticStyle != CodeLook ? "lightning_static" : null;
                case AfflictionKind.Unsteady: return depth <= 2 ? "wind_unsteady_1" : depth >= 5 ? "wind_unsteady_5" : "wind_unsteady";
                // Elements 4's one beam of light coming down on it (the owner, 2026-11-20: "right now it looks like 3 beams").
                case AfflictionKind.Marked: return "light_marked";
                default: return null;
            }
        }

        private static readonly AfflictionKind[] Looked =
        {
            AfflictionKind.Soaked, AfflictionKind.Sundered, AfflictionKind.Poison, AfflictionKind.Seeded, AfflictionKind.Cursed, AfflictionKind.Chill,
            AfflictionKind.Tremor, AfflictionKind.Static, AfflictionKind.Unsteady, AfflictionKind.Marked,
        };

        /// <summary>
        /// Keeps the second-pass looks of what a body carries playing on it this frame. Soaked is not drawn on a body that
        /// is Seized (two Static or more: its water has run off into the film) or rusting (Sundered too: the water has
        /// gone into the rust), as the owner had the rings over those taken away (2026-11-20).
        /// </summary>
        public static void KeepLooks(AfflictionSet afflictions, Transform body)
        {
            if (afflictions == null || body == null || !ElementLooks.Ready) return;
            for (int i = 0; i < Looked.Length; i++)
            {
                var kind = Looked[i];
                int depth = afflictions.Stacks(kind);
                if (depth <= 0) continue;
                if (kind == AfflictionKind.Soaked && (afflictions.Stacks(AfflictionKind.Static) >= SeizedStatic || afflictions.Stacks(AfflictionKind.Sundered) > 0)) continue;
                // A reaction's own look is playing on it and shows what became of this one: still carried, it is kept
                // unseen and ageing, so it is simply there when that look is over, not cast again from its start.
                if (ElementLooks.Hides(body, (int)kind))
                {
                    ElementLooks.KeepVeiled(ElementLooks.Find(LookOf(kind, depth)), body);
                    continue;
                }
                // Under ice that bursts through the body (Frost Bite's spike, the charged ice blade's crystals) Chill's
                // shell does not cover it (the owner: "that white spiderweb thing is really covering everything up").
                if (kind == AfflictionKind.Chill && ElementLooks.Thawed(body)) continue;
                // Marked after a reaction shows as its small gold flare for a while (FlareSeconds), not the beam from the
                // sky: "The Marked mark must not be a reaction's biggest shape" (the beam stood over every reaction's
                // aftermath in the films of the third pass).
                if (kind == AfflictionKind.Marked && FlareNear(body.position) >= 0) continue;
                ElementLooks.Keep(ElementLooks.Find(LookOf(kind, depth)), body);
            }
        }

        /// <summary>Seconds Marked's sun-wheel takes to come in, so it fades back after a reaction's look instead of popping in.</summary>
        public const float WheelFadeIn = 0.45f;

        /// <summary>Marked's sun-wheels lying under a body fade out: a reaction's own look is starting there.</summary>
        public static void QuietMarks(Vector3 feet)
        {
            ElementArt.Hush(ElementArt.Piece("light_wheel_affliction"), feet, ElementLooks.QuietReach, 0.2f);
            // After the reaction's look, Marked shows there as a small gold flare on the body, not the floor's sun-wheel
            // (the Blender worker's scale notes, 2026-11-20: "The Marked mandala must not be a reaction's biggest shape").
            int slot = FlareNear(feet);
            if (slot < 0)
            {
                slot = 0;
                for (int i = 0; i < MostFlares; i++)
                    if (flareUntil[i] < flareUntil[slot]) slot = i;
            }
            flareAt[slot] = feet;
            flareUntil[slot] = Time.time + FlareSeconds;
            flareSeen[slot] = Time.time;
            flareDepth[slot] = -1;
        }

        /// <summary>
        /// Seconds after a reaction that Marked on a body it reached shows only as a gold flare on the body; the full
        /// sun-wheel comes back in sooner when the mark is laid again by something else (deeper, or after it was gone).
        /// </summary>
        public const float FlareSeconds = 6f;
        public static readonly Color FlareGold = new Color(1f, 0.7f, 0.2f, 1f);

        private const int MostFlares = 16;
        private static readonly Vector3[] flareAt = new Vector3[MostFlares];
        private static readonly float[] flareUntil = new float[MostFlares];
        private static readonly float[] flareSeen = new float[MostFlares];
        private static readonly int[] flareDepth = new int[MostFlares];

        private static int FlareNear(Vector3 feet)
        {
            float now = Time.time;
            for (int i = 0; i < MostFlares; i++)
            {
                if (flareUntil[i] <= now) continue;
                Vector3 off = flareAt[i] - feet;
                off.y = 0f;
                if (off.sqrMagnitude <= ElementLooks.QuietReach * ElementLooks.QuietReach) return i;
            }
            return -1;
        }

        // Marked on a body a reaction reached a moment ago: its gold flare instead of the wheel, until it is laid anew or its time is out.
        private static bool Flare(int depth, Vector3 feet)
        {
            int slot = FlareNear(feet);
            if (slot < 0) return false;
            float now = Time.time;
            bool laidAgain = flareDepth[slot] >= 0 && (depth > flareDepth[slot] || now - flareSeen[slot] > 0.6f);
            if (laidAgain)
            {
                flareUntil[slot] = 0f;
                return false;
            }
            flareAt[slot] = feet;
            flareSeen[slot] = now;
            if (flareDepth[slot] < 0) flareDepth[slot] = depth;
            ElementArt.Stand(ElementArt.Piece("light_glint_affliction"), feet + Vector3.up * 1.25f, 180f, 0.75f, 0.3f, FlareGold);
            return true;
        }

        /// <summary>
        /// Metres from a player swinging a Light blade within which Marked's sun-wheel is not laid on the floor, and how
        /// long after the swing's last frame. The wheels under the soldiers it marked lay cream rings over the swing's own
        /// gold (the swing films of 2026-11-20): the shaft and the glint on the body stay.
        /// </summary>
        public const float SwingQuietReach = 6f, SwingQuietSeconds = 0.6f;

        private static float swingQuietUntil;
        private static Vector3 swingQuietAt;

        /// <summary>A Light swing is being drawn by a player standing at <paramref name="at"/>: Marked's floor wheels near wait.</summary>
        public static void SwingQuiet(Vector3 at)
        {
            swingQuietUntil = Time.time + SwingQuietSeconds;
            swingQuietAt = at;
        }

        private static bool SwingQuietAt(Vector3 feet)
        {
            if (Time.time >= swingQuietUntil) return false;
            Vector3 off = feet - swingQuietAt;
            off.y = 0f;
            return off.sqrMagnitude <= SwingQuietReach * SwingQuietReach;
        }

        /// <summary>The depth of Static at which a Soaked body is Seized (Reactions.Three: "two Static or more").</summary>
        public const int SeizedStatic = 2;

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
            System.Array.Clear(flareAt, 0, MostFlares);
            System.Array.Clear(flareUntil, 0, MostFlares);
            System.Array.Clear(flareSeen, 0, MostFlares);
            System.Array.Clear(flareDepth, 0, MostFlares);
            Jolts = 0;
            swingQuietUntil = 0f;
            swingQuietAt = Vector3.zero;
            BurnStyle = DefaultBurnStyle;
            StaticStyle = DefaultStaticStyle;
        }

        /// <summary>Puts down one beat of an affliction's look. False when the new effects are off or the scene has no art.</summary>
        public static bool Show(AfflictionKind kind, int depth, bool frozen, Vector3 feet, int beat)
        {
            if (!ElementArt.Ready) return false;
            if ((int)kind >= 0 && (int)kind < shown.Length) shown[(int)kind]++;
            // An affliction with a second-pass look is drawn by it (KeepLooks), not by pieces thrown a few times a second;
            // Static drawn both ways keeps its crawl and jumping arcs under the look's.
            if (ElementLooks.Find(LookOf(kind, depth)) >= 0 && !(kind == AfflictionKind.Static && StaticStyle == BothLooks))
            {
                // Marked on a body a reaction reached a moment ago is its small gold flare, not the beam (KeepLooks waits).
                if (kind == AfflictionKind.Marked) Flare(depth, feet);
                return true;
            }
            float jitter = (ElementSubstance.Hash(beat) - 0.5f) * 40f;
            float more = 1f + 0.08f * depth;
            switch (kind)
            {
                case AfflictionKind.Burn:
                    // The flame itself is BodyFlames' real fire, kept by the burning body every frame (the owner's review,
                    // 2026-11-20: no more stacked staircase of tongues). Only a scene without its material falls back on
                    // the kit's tongues, stood upright (they leaned up and to the right, 2026-11-19).
                    if (!BodyFlames.Ready)
                        ElementArt.Erupt(ElementArt.Piece("fire_climb_affliction"), feet, Quaternion.Euler(0f, 180f + jitter * 2f, 0f) * ClimbUpright,
                            new Vector3(more, (0.85f + 0.3f * ElementSubstance.Hash(beat + 3)) * more, more), 0.06f, 0.22f, 0.12f);
                    // The stepped char rings fought the real flame (the coordinator's check, 2026-11-20): only the old tongues keep them.
                    if (!BodyFlames.Ready && beat % 4 == 0) ElementArt.Floor(ElementArt.Piece("fire_charrings_affliction"), feet, beat * 37f, 0.85f, 1f, 1.8f, 0.35f, true);
                    if (beat % 5 == 1) ElementArt.Sheet(ElementArt.SheetOf("fire_smoke_puff"), feet + Vector3.up * 3.1f, 1.8f, 1.3f);
                    return true;
                case AfflictionKind.Static:
                {
                    // Redrawn in a new place each beat and held on until the next is drawn, so it crackles without
                    // blinking dark between (easy on the eyes, the owner's word on Seized, 2026-11-20).
                    float hold = StaticHold(depth);
                    ElementArt.Stand(ElementArt.Piece("lightning_crawl_affliction"), feet, ElementSubstance.Hash(beat + 1) * 360f, 1f, hold);
                    ElementArt.Stand(ElementArt.Piece("lightning_crawl_affliction"), feet, ElementSubstance.Hash(beat + 7) * 360f, 0.9f, hold * 0.9f);
                    // A bolt jumps off the body outward and is gone (the owner, 2026-11-20: "the bolts static off the
                    // enemy not just around them"): rooted at the body's side, reaching away, shrinking as it goes. With
                    // the Blender worker's arcs drawn too, theirs jump off instead: these read near white on film.
                    if (StaticStyle == CodeLook)
                    {
                        float yaw = ElementSubstance.Hash(beat + 11) * 360f;
                        Vector3 outward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
                        float length = ArcLength * (0.8f + 0.4f * ElementSubstance.Hash(beat + 13)) * (0.9f + 0.1f * depth);
                        Vector3 root = feet + outward * BodyRadius + Vector3.up * (0.7f + 0.8f * ElementSubstance.Hash(beat + 17));
                        // The jump arc lies along its own length about its middle: its near end is put at the body's side.
                        Vector3 middle = root + outward * (length * 0.5f) + Vector3.down * 0.15f;
                        var lie = Quaternion.LookRotation(outward + Vector3.down * 0.25f, Vector3.up);
                        ElementArt.Fly(ElementArt.Piece("lightning_jump_charged"), middle, middle + outward * 0.35f, lie, length / JumpPieceLength, ArcSeconds);
                    }
                    return true;
                }
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
                    // The body is shaken (the enemy's own shake), and that is the look (the owner, 2026-11-20: "the
                    // shaking is good, but the random flashing ring of stones is not great"): no heaved ring. The ground
                    // under it answers: dust kicked up round the feet, and small grit hopping off the floor a hand high
                    // and falling back, never drawn in toward the body.
                    if (beat % 3 == 0)
                    {
                        Vector3 side = FormRules.OnRing(0, 1, 0.35f, beat * 97f);
                        Vfx.Play(VfxKind.Dust, feet + side + Vector3.up * 0.05f, Vector3.up, 0.35f + 0.05f * depth);
                    }
                    Vector3 out_ = FormRules.OnRing(0, 1, 1f, beat * 67f);
                    float hop = 1.6f + 0.6f * ElementSubstance.Hash(beat * 3);
                    FormFx.Toss(FormMesh.Pebble, feet + out_ * (0.3f + 0.25f * ElementSubstance.Hash(beat + 5)) + Vector3.up * 0.03f, out_ * 0.5f + Vector3.up * hop,
                        Quaternion.Euler(beat * 30f, beat * 50f, 0f), Vector3.one * 0.22f, new Color(0.3f, 0.27f, 0.23f), 0.15f, 0.03f);
                    return true;
                }
                case AfflictionKind.Sundered:
                {
                    // Plates fitted to the body on the side the camera sees, the notch on the largest, the vein down
                    // the leg to a stud at the ankle; shake marks either side of the head show it ringing.
                    // Sundered and Soaked at once is Rust (the caller says so through "frozen"): all of it is rust.
                    float hold = 0.45f;
                    Color steel = frozen ? ReactionDesigns.RustColour : Color.white;
                    ElementArt.Stand(ElementArt.Piece("metal_plate_affliction"), feet + new Vector3(0.05f, 1.05f, -0.32f), 180f, 1f, hold, steel);
                    ElementArt.Stand(ElementArt.Piece("metal_plate_affliction_1"), feet + new Vector3(-0.28f, 1.35f, -0.25f), 160f, 1f, hold, steel);
                    ElementArt.Stand(ElementArt.Piece("metal_plate_affliction_2"), feet + new Vector3(0.18f, 0.55f, -0.28f), 200f, 1f, hold, steel);
                    ElementArt.Stand(ElementArt.Piece("metal_notch_affliction"), feet + new Vector3(0.05f, 1.25f, -0.42f), 180f, 1.3f, hold, steel);
                    ElementArt.Stand(ElementArt.Piece("metal_ichor_affliction"), feet + new Vector3(0.12f, 0.02f, -0.3f), 180f, 1f, hold, steel);
                    ElementArt.Stand(ElementArt.Piece("metal_stud_affliction"), feet + new Vector3(0.12f, 0.02f, -0.3f), 180f, 1.4f, hold, steel);
                    if (beat % 2 == 0) ElementArt.Stand(ElementArt.Piece("metal_ring_affliction"), feet + Vector3.up * 1.75f, 180f, 0.9f + 0.15f * ElementSubstance.Hash(beat), 0.12f, steel);
                    if (beat % 4 == 0) ElementArt.Floor(ElementArt.Piece("metal_filings_affliction"), feet, 0f, 0.9f, 1f, 1.8f, 0f, true, steel);
                    return true;
                }
                case AfflictionKind.Marked:
                    // A reaction's own look playing on or beside it is what is seen there (the audit of 2026-11-20: the
                    // cream sun-wheel was the largest shape on screen in nine reactions): the wheel waits, then comes
                    // back in softly.
                    if (ElementLooks.QuietAt(feet)) return true;
                    if (Flare(depth, feet)) return true;
                    // While a Light swing plays beside it the wheel is not laid, and one already down goes.
                    if (SwingQuietAt(feet)) ElementArt.Hush(ElementArt.Piece("light_wheel_affliction"), feet, ElementLooks.QuietReach, 0.15f);
                    // The wheel turns under it; the shaft lands on its head.
                    else if (beat % 3 == 0) ElementArt.Floor(ElementArt.Piece("light_wheel_affliction"), feet, beat * 15f, 1f, 1f, 1.5f, 0f, true, Color.white, 40f, WheelFadeIn);
                    ElementArt.Stand(ElementArt.Piece("light_pencil_affliction"), feet + Vector3.up * 1.9f, 180f, 1f, 0.4f);
                    ElementArt.Stand(ElementArt.Piece("light_glint_affliction"), feet + Vector3.up * 2.05f, 180f, 0.8f, 0.2f);
                    return true;
                case AfflictionKind.Cursed:
                {
                    // A black hole at its feet, turning, and the dark energy pulled off the body down into it (the owner,
                    // 2026-11-19 and 2026-11-20: "dark energy being sucked down into the black hole at the bottom", no
                    // purple marks dropping). The drained floor and the hole are laid every third beat and overlap, so
                    // they never go; a whirl of black streaks closes in on the hole as it turns.
                    if (beat % 3 == 0)
                    {
                        ElementArt.Floor(ElementArt.Piece("dark_drain_affliction"), feet, 0f, 1f, 1f, 1.4f, 0.2f, true);
                        ElementArt.Floor(ElementArt.Piece("dark_hole_spell"), feet, beat * 20f, 0.72f, 0.8f, 1.4f, 0.15f, true, Color.white, 90f);
                        ElementArt.Floor(ElementArt.Piece("dark_streaks_hit"), feet, beat * 37f, 0.5f, 0.08f, 0.9f, 0f, true, CurseInk, 260f);
                    }
                    // A wisp of near-black is torn off the body and drawn down into the hole, faster as it nears it,
                    // stretched along its fall and gone into it.
                    Vector3 into = feet + Vector3.up * 0.04f;
                    for (int wisp = 0; wisp < 3; wisp++)
                    {
                        // Streaks of black drawn down along their own way into the hole, faster as they near it (a
                        // streak, not a drop: no marks falling, the owner's word), each from its own side of the body.
                        float round = beat * 83f + wisp * 120f;
                        Vector3 from = feet + FormRules.OnRing(0, 1, 0.6f + 0.4f * ElementSubstance.Hash(beat * 3 + wisp), round) + Vector3.up * (0.6f + 1.1f * ElementSubstance.Hash(beat * 3 + wisp + 4));
                        FormFx.Drop(FormMesh.Gust, from, into, Quaternion.LookRotation(into - from), new Vector3(1.1f, 1.1f, 0.9f), wisp == 1 ? CurseDeep : CurseInk, 0.4f, 0f, 0.06f, 0.02f);
                    }
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

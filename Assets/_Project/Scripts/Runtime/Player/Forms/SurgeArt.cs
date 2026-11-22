using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Each element's surge in the approved look (the look-dev's "surge" panels), held round the player for as long as
    /// it lasts, for <see cref="SurgeLook"/>. Kindle: a char ring under the built flames. Overdrive: no rings (the owner,
    /// 2026-11-19 and 2026-11-20): jagged, branching arcs of the lightning kit jumping on and off the body and gone in a
    /// blink, a few at a time. Tailwind: Fujin's bag-mouth spiral with a column of leaves. Glaze: no pieces: ice grows out of the player (GlazeIce).
    /// Wellspring: three rings rising, hanging and falling in turn. Regrowth and Bedrock: none (their built leaves and
    /// stones). Temper: no pieces at all: the player turns to steel (<see cref="MetalSheen"/>). Clarity: a kolam drawn round them, a
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
            nextArc = 0f;
            nextCrawl = 0f;
            arcBeat = 0;
            look = -1;
            wearer = null;
            OverdriveStyle = DefaultOverdriveStyle;
            for (int i = 0; i < MostJaws; i++)
            {
                jaws[i] = -1;
                jawOn[i] = null;
            }
            nextJawLook = 0f;
        }

        /// <summary>
        /// Hunger's jaws (round 4): the player's shadow stretches to each near enemy and two rows of black fangs rise round
        /// its feet and gnash, for as long as Hunger lasts. Up to this many enemies, this near, looked for this often.
        /// </summary>
        public const int MostJaws = 2;
        public const float JawReach = 3.5f, JawLookEvery = 0.25f;
        private static readonly int[] jaws = { -1, -1 };
        private static readonly Transform[] jawOn = new Transform[MostJaws];
        private static readonly Transform[] jawFound = new Transform[MostJaws];
        private static readonly Collider[] nearJaws = new Collider[32];
        private static float nextJawLook;

        /// <summary>How many enemies Hunger's jaws are round now, for probes.</summary>
        public static int JawsShown
        {
            get
            {
                int count = 0;
                for (int i = 0; i < MostJaws; i++)
                    if (jaws[i] >= 0 && jawOn[i] != null) count++;
                return count;
            }
        }

        // The near enemies (and dummies) standing nearest the player's feet, nearest first, the dead left out.
        private static int FindJawBodies(Vector3 feet)
        {
            for (int i = 0; i < MostJaws; i++) jawFound[i] = null;
            int count = Physics.OverlapSphereNonAlloc(feet + Vector3.up, JawReach + 0.6f, nearJaws, ~0, QueryTriggerInteraction.Ignore);
            float first = float.MaxValue, second = float.MaxValue;
            for (int i = 0; i < count; i++)
            {
                var enemy = nearJaws[i].GetComponentInParent<Clover.Gameplay.Enemy>();
                Transform at = enemy != null && !enemy.IsDead ? enemy.transform : null;
                if (at == null && enemy == null)
                {
                    var dummy = nearJaws[i].GetComponentInParent<Clover.Gameplay.TrainingDummy>();
                    at = dummy != null ? dummy.transform : null;
                }
                if (at == null || ReferenceEquals(at, jawFound[0]) || ReferenceEquals(at, jawFound[1])) continue;
                Vector3 flat = at.position - feet;
                flat.y = 0f;
                float distance = flat.magnitude;
                if (distance > JawReach) continue;
                if (distance < first)
                {
                    jawFound[1] = jawFound[0];
                    second = first;
                    jawFound[0] = at;
                    first = distance;
                }
                else if (distance < second)
                {
                    jawFound[1] = at;
                    second = distance;
                }
            }
            return (jawFound[0] != null ? 1 : 0) + (jawFound[1] != null ? 1 : 0);
        }

        private static void PlaceJaws(Vector3 feet, float time)
        {
            int key = ElementLooks.Find(HungerJawsKey);
            if (key < 0) return;
            if (time >= nextJawLook)
            {
                nextJawLook = time + JawLookEvery;
                FindJawBodies(feet);
                // Jaws round an enemy no longer near close; a newly near one has jaws opened round it.
                for (int i = 0; i < MostJaws; i++)
                    if (jaws[i] >= 0 && !ReferenceEquals(jawOn[i], jawFound[0]) && !ReferenceEquals(jawOn[i], jawFound[1])) EndJaw(i);
                for (int f = 0; f < MostJaws; f++)
                {
                    var body = jawFound[f];
                    if (body == null || ReferenceEquals(jawOn[0], body) || ReferenceEquals(jawOn[1], body)) continue;
                    int slot = jaws[0] < 0 ? 0 : jaws[1] < 0 ? 1 : -1;
                    if (slot < 0) break;
                    Vector3 toward = body.position - feet;
                    toward.y = 0f;
                    float yaw = toward.sqrMagnitude > 0.0001f ? Mathf.Atan2(toward.x, toward.z) * Mathf.Rad2Deg : 0f;
                    jaws[slot] = ElementLooks.Held(key, body.position, yaw, 1f, JawsHeld);
                    jawOn[slot] = jaws[slot] >= 0 ? body : null;
                }
            }
            for (int i = 0; i < MostJaws; i++)
                if (jaws[i] >= 0 && jawOn[i] != null) ElementLooks.MoveHeld(jaws[i], jawOn[i].position);
        }

        private static void EndJaw(int i)
        {
            if (jaws[i] >= 0) ElementLooks.EndHeld(jaws[i]);
            jaws[i] = -1;
            jawOn[i] = null;
        }

        /// <summary>The key Hunger's jaws round one enemy are built under (ElementLookBuilder.Redesigns).</summary>
        public const string HungerJawsKey = "dark_hunger@b";
        // A jaw is held at most this long (Hunger lasts less); it is taken off when Hunger ends or the enemy leaves.
        private const float JawsHeld = 60f;

        /// <summary>
        /// True for the elements whose built surge pieces (hanging blades, a crown of rays, smoke, bubbles, Wind's
        /// spinning streaks and leaves) the art replaces outright. Wind's did not read as wind (the owner's review,
        /// 2026-11-19): its bag-mouth spiral stands alone.
        /// </summary>
        public static bool ReplacesBuilt(ElementInfo info) =>
            info != null && (info.Index == ElementSubstance.Metal || info.Index == ElementSubstance.Ice || info.Index == ElementSubstance.Light || info.Index == ElementSubstance.Dark || info.Index == ElementSubstance.Poison
                || info.Index == ElementSubstance.Wind || info.Index == ElementSubstance.Lightning || (LookOf(info.Index) != null && ElementLooks.Find(LookOf(info.Index)) >= 0));

        /// <summary>
        /// The second-pass look a surge wears (the Blender worker's batch 1, <see cref="ElementLooks"/>): Wellspring's
        /// pool, Temper's glints over the steel, Fever's clouds, Regrowth's roots and flowers, Hunger's eclipse.
        /// </summary>
        public static string LookOf(int element)
        {
            switch (element)
            {
                case ElementSubstance.Lightning: return OverdriveStyle != AfflictionArt.CodeLook ? "lightning_overdrive" : null;
                // Kindle: you catch fire, flame running up the arms and streaming off the shoulders (round 4, approved).
                case ElementSubstance.Fire: return "fire_kindle";
                // Clarity: the Blender worker's gold lotus of light replaces the kolam (the owner, 2026-11-20).
                case ElementSubstance.Light: return "light_clarity";
                case ElementSubstance.Water: return "water_wellspring";
                case ElementSubstance.Metal: return "metal_temper";
                case ElementSubstance.Poison: return "poison_fever";
                case ElementSubstance.Life: return "life_regrowth";
                case ElementSubstance.Dark: return "dark_hunger";
                // Tailwind: the spiral turned upside down, its small end at the feet (elements 4).
                case ElementSubstance.Wind: return ElementMoves.Tailwind;
                // Glaze: the ice grown on the player's own bones (GlazeIce) alone, as before elements 4 (round 4: no extra
                // crystals and no pool under the player).
                case ElementSubstance.Ice: return null;
                // Bedrock: the earthen armour (the owner's pick, 2026-11-20); the stone skin and its plates are StoneArmour's,
                // the grit and dust falling as it closes the look's.
                case ElementSubstance.Earth: return "earth_bedrock";
                default: return null;
            }
        }

        /// <summary>
        /// Overdrive drawn by the code's arcs, the Blender worker's keyed arcs (<see cref="AfflictionArt.KitLook"/>), or
        /// both. Filmed side by side (2026-11-20), the Blender worker's arcs won: cyan zigzags with a violet rim snapping
        /// across the body and down to the floor, no white, where the code's forks burst as white stars. A tuning point a
        /// probe may set; put back each play.
        /// </summary>
        public static byte OverdriveStyle = DefaultOverdriveStyle;
        public const byte DefaultOverdriveStyle = AfflictionArt.KitLook;

        // The surge's look while it plays, and the player it is on.
        private static int look = -1;
        private static Transform wearer;

        /// <summary>The player the surge is drawn on (its second-pass look follows them).</summary>
        public static void Wear(Transform player) => wearer = player;

        /// <summary>
        /// True for the surges drawn by thrown pieces or by the body itself rather than held ones: Overdrive's arcs and
        /// Temper's steel. They hold nothing and still replace the built look.
        /// </summary>
        public static bool DrawnWithoutHolding(int element) => element == ElementSubstance.Lightning || element == ElementSubstance.Metal || element == ElementSubstance.Ice;

        /// <summary>
        /// How much of the player's surge light an element's surge lights (1 all of it). Dark lights none: its light
        /// lay a bright purple disc on the floor in the middle of Hunger (the owner, 2026-11-20: "The middle is still too
        /// bright purple instead of a much darker almost black purple"); darkness gives off no light.
        /// </summary>
        public static float Glow(ElementInfo info) =>
            info == null ? 1f : info.Index == ElementSubstance.Dark ? 0f
            // Overdrive's light kept low, so the floor under the player shows no ring of it (the owner: no rings).
            : info.Index == ElementSubstance.Lightning ? 0.45f : 1f;

        /// <summary>Overdrive: arcs thrown a second, how long each is seen, and how far off the body's middle they reach.</summary>
        public const float ArcsPerSecond = 14f, ArcSeconds = 0.2f, ArcReach = 0.55f;
        /// <summary>Overdrive: a crawl held close on the body, redrawn this often (never dark between).</summary>
        public const float CrawlEvery = 0.16f;
        private static float nextCrawl;
        private static float nextArc;
        private static int arcBeat;

        /// <summary>Seconds since the surge's pieces were taken; long ago when none are held (the built pieces then show grown).</summary>
        public static float Age(float time) => element >= 0 ? time - began : 1000f;

        /// <summary>Seconds Clarity's shaft and halo stand at its start before they go; its drawn ground stays.</summary>
        public const float RaysSeconds = 1.2f, RaysGo = 0.5f;

        /// <summary>The ink Hunger's turning streaks and motes are drawn in: black, not white (the owner's review).</summary>
        /// <summary>Tailwind's spiral tint: wind's pale dust, a little lighter than its pieces so the spiral still reads.</summary>
        public static readonly Color TailwindDust = new Color(0.72f, 0.76f, 0.66f, 1f);

        private static readonly Color HungerInk = new Color(0.04f, 0.015f, 0.06f, 1f);

        /// <summary>
        /// The tint over Hunger's eclipse, its middle. The eclipse's own colour is dark only in linear terms (0.09), which
        /// the screen shows as a 33% purple, lighter than the floor: "too bright purple" (the owner, 2026-11-20). This
        /// takes it to about a tenth of that, a near-black purple on screen.
        /// </summary>
        public static readonly Color HungerHeart = new Color(0.12f, 0.08f, 0.14f, 1f);

        /// <summary>Takes the surge's pieces. False when the new effects are off or the scene has no art.</summary>
        public static bool Begin(ElementInfo info)
        {
            End();
            if (!ElementArt.Ready || info == null) return false;
            element = info.Index;
            began = Time.time;
            look = ElementLooks.Find(LookOf(element));
            if (look >= 0) return true;
            switch (element)
            {
                // The owner's review (2026-11-19): Kindle's cobra and its heat bands took the screen, and only the
                // flames at the base were liked; Bedrock's outer ring of standing stones and Regrowth's great turning
                // triskele go, leaving their built stones (turning on the ground) and leaves.
                case ElementSubstance.Fire: Floor(0, "fire_charrings_affliction"); break;
                // Overdrive's arcs are thrown in Place; nothing is held (no drums, no rings).
                case ElementSubstance.Lightning: break;
                // Tailwind's spiral (liked, 2026-11-19) in wind's pale dust: it read white on film (2026-11-20).
                case ElementSubstance.Wind: solids[0] = ElementArt.Hold(ElementArt.Piece("wind_bagspiral_surge"), TailwindDust); break;
                // Glaze is ice grown out of the player (GlazeIce, driven by PlayerElement): no knotwork frost ring on the
                // floor and nothing floating round them (the owner, 2026-11-19: "should grow out of you instead of floating
                // around you cause its ice").
                case ElementSubstance.Ice: break;
                case ElementSubstance.Water: Solid(0, "water_rings_surge"); break;
                case ElementSubstance.Life:
                case ElementSubstance.Earth: break;
                // Temper is the player turned to steel (MetalSheen, driven by PlayerElement): no blades round them and no
                // seam on the floor (the owner, 2026-11-19: "the swords around you look super bad"; 2026-11-20: "a
                // visible metallic sheen essentially turning metal yourself").
                case ElementSubstance.Metal: break;
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
                    FormFx.TintMark(floors[0], HungerHeart);
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
            any |= DrawnWithoutHolding(element);
            nextArc = 0f;
            if (!any) End();
            return any;
        }

        private static void Solid(int slot, string piece) => solids[slot] = ElementArt.Hold(ElementArt.Piece(piece));

        // Overdrive's buzzing as real electricity (the owner, 2026-11-19: "the buzzing electricity, doesnt look like
        // electricity"): the kit's branching fork bursts on the body and its long jagged arcs jump off it, each seen for a
        // tenth of a second and shrinking away as it goes, a new one every ninety milliseconds somewhere else, turned
        // anew. Nothing wobbles; nothing stays.
        private static void ThrowArcs(Vector3 feet, float time)
        {
            // A steady crackle while it lasts: a crawl always on the body, redrawn turned on and held past the next, so
            // with the arcs two or three are alive at any moment and no frame is empty or white.
            if (time >= nextCrawl)
            {
                nextCrawl = time + CrawlEvery;
                int crawlBeat = Mathf.FloorToInt(time / CrawlEvery);
                ElementArt.Stand(ElementArt.Piece("lightning_crawl_affliction"), feet, ElementSubstance.Hash(crawlBeat * 5) * 360f, 0.95f, CrawlEvery * 1.3f);
            }
            if (time < nextArc) return;
            nextArc = time + 1f / ArcsPerSecond;
            int beat = ++arcBeat;
            float yaw = ElementSubstance.Hash(beat * 7) * 360f;
            Vector3 outward = Quaternion.Euler(0f, yaw, 0f) * Vector3.forward;
            float height = 0.45f + 1.15f * ElementSubstance.Hash(beat * 7 + 1);
            // Half the arcs stay close round the body (forks on it), half jump off it.
            if (beat % 2 == 0)
            {
                // A branching fork bursting on the body: its five jagged branches reach out from one point.
                Vector3 at = feet + outward * 0.12f + Vector3.up * height;
                var tilt = Quaternion.Euler(-60f + 120f * ElementSubstance.Hash(beat * 7 + 2), yaw, 90f * ElementSubstance.Hash(beat * 7 + 3));
                ElementArt.Fly(ElementArt.Piece("lightning_fork_hit"), at, at + outward * 0.15f, tilt, 0.85f + 0.35f * ElementSubstance.Hash(beat * 7 + 4), ArcSeconds);
            }
            else
            {
                // A long jagged arc jumping off the body to the air beside it.
                float length = 1.7f + 0.9f * ElementSubstance.Hash(beat * 7 + 5);
                Vector3 middle = feet + outward * (ArcReach * 0.4f + length * 0.5f) + Vector3.up * height;
                var lie = Quaternion.LookRotation(outward + Vector3.up * (ElementSubstance.Hash(beat * 7 + 6) - 0.5f), Vector3.up);
                ElementArt.Fly(ElementArt.Piece("lightning_jump_charged"), middle, middle + outward * 0.3f, lie, length / 3.2f, ArcSeconds * 1.3f);
            }
        }

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
            for (int i = 0; i < MostJaws; i++) EndJaw(i);
            element = -1;
            look = -1;
        }

        /// <summary>Places the pieces round the player's feet.</summary>
        public static void Place(Vector3 feet, float time)
        {
            if (element < 0) return;
            if (look >= 0)
            {
                if (wearer != null) ElementLooks.Keep(look, wearer);
                if (element == ElementSubstance.Dark) PlaceJaws(feet, time);
                if (element == ElementSubstance.Lightning && OverdriveStyle == AfflictionArt.BothLooks) ThrowArcs(feet, time);
                return;
            }
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
                    ThrowArcs(feet, time);
                    break;
                case ElementSubstance.Wind:
                    FormFx.Place(solids[0], feet, Quaternion.Euler(0f, time * -240f, 0f), new Vector3(0.8f, 0.8f * grown, 0.8f));
                    break;
                case ElementSubstance.Ice:
                    break;
                case ElementSubstance.Water:
                {
                    // The rings rise, hang and fall in turn: the whole set breathes up and down.
                    float rise = 0.25f * Mathf.Sin(time * 2.4f);
                    FormFx.Place(solids[0], feet + Vector3.up * rise, Quaternion.Euler(0f, time * 30f, 0f), new Vector3(0.75f, 0.75f * grown, 0.75f));
                    break;
                }
                case ElementSubstance.Metal:
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

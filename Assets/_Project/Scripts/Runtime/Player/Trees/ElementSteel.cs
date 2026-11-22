using Clover.Core;
using Clover.Gameplay;
using Clover.Player.Forms;
using Clover.Player.Magic;
using Clover.World;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// Where each element meets the weapon (STEEL_AND_ELEMENT.md 3.3, 3.4; steel/elements_a.md and elements_b.md): what a
    /// charged blade's blows add for each element, its third blow's extra, and each element's infused strike and deep
    /// strike acting on the carrier's <see cref="Footprint"/>. Each is the simplest version that keeps the design's
    /// intent, drawn with the built effect pieces (flame, crackle, frost, motes of the element's colour); where a piece
    /// of the design needs engine work not built yet (an enemy state, a standing sapling, a wall to be thrown into) the
    /// line says so with <c>NEEDS</c>. No state of its own: everything is done through <see cref="PlayerCombat"/>'s
    /// strike helpers, so the blows it deals are effect blows (they never charge the blade and lay with the player's
    /// numbers).
    /// </summary>
    public static class ElementSteel
    {
        /// <summary>The name of an element's infused strike (L2 with R1) or deep strike (L2 with R2), for the HUD and the guide.</summary>
        public static string StrikeName(string element, bool deep)
        {
            switch (element)
            {
                case Elements.Fire: return deep ? "Burnt Offering" : "Burning Edge";
                case Elements.Lightning: return deep ? "Down the Channel" : "Flash Cut";
                case Elements.Wind: return deep ? "Whirl Cut" : "Cutting Wind";
                case Elements.Ice: return deep ? "Ice Through" : "Frost Bite";
                case Elements.Water: return deep ? "Thrown on the Reef" : "Lash";
                case Elements.Life: return deep ? "Briar Root" : "Grown Into It";
                case Elements.Earth: return deep ? "Mountain Cleaver" : "Broken Ground";
                case Elements.Metal: return deep ? "Red Iron" : "Struck Plate";
                case Elements.Light: return deep ? "The Sun on the Blade" : "Ray Along the Edge";
                case Elements.Dark: return deep ? "Yama's Noose" : "Taken";
                case Elements.Poison: return deep ? "Nine Times Refined" : "Venom Flick";
                default: return deep ? "Deep strike" : "Infused strike";
            }
        }

        // ---- The charged blade.

        /// <summary>True when a charged blow of this element passes any guard (Metal's: "a shield or a raised guard takes them in full").</summary>
        public static bool PassesGuard(string element) => element == Elements.Metal;

        /// <summary>How many stacks a charged blow lays: one, or two for Poison ("each charged blow doses twice").</summary>
        public static int Stacks(string element) => element == Elements.Poison ? 2 : 1;

        /// <summary>
        /// Before a charged blow strikes: Ice's charged blow shatters what is Frozen, light blow or heavy. Returns the
        /// damage the shatter adds (zero when nothing broke). Read before the blow lays anything of its own.
        /// </summary>
        public static float BeforeChargedBlow(string element, IAfflictable target, bool heavy)
        {
            if (element != Elements.Ice || heavy || target == null || !target.Afflictions.IsFrozen) return 0f;
            float broke = target.Afflictions.Shatter();
            if (target is Component body)
            {
                CombatEvents.RaiseShattered(body);
                var colour = Afflictions.Spec(AfflictionKind.Chill).Colour;
                Vfx.Play(VfxKind.Burst, body.transform.position + Vector3.up * 1.2f);
                Vfx.Play(VfxKind.TintSpark, body.transform.position + Vector3.up * 1.2f, Vector3.up, 2.5f, colour);
            }
            return broke;
        }

        /// <summary>A charged blow has landed on <paramref name="target"/> (the first the move struck): the element's own addition, and the third's extra.</summary>
        public static void AfterChargedBlow(PlayerCombat combat, ElementInfo element, Component target, in ChargedBlow blow, float damage)
        {
            if (combat == null || element == null || target == null) return;
            Vector3 at = target.transform.position;
            Vector3 forward = combat.transform.forward;
            // The element shows where the charge went in, whatever it adds. Ice's crack-web and Earth's floating cubes gave
            // way (the owner's notes of 2026-11-19) to the swing look's own landing: frost that bursts and splinters fall,
            // rocks thrown up that fall back.
            var swingLook = Swings.SwingLooks.For(element);
            // The elements 3 landing, on the body struck, its third with the element's extra (the Blender worker's blade
            // timelines): in place of the old hit pieces, never beside them.
            bool own = ElementArt.Ready && ElementMoves.PlayBladeHit(element.Index, target.transform, forward, blow.Third ? 3 : Mathf.Clamp(blow.Number, 1, 2));
            bool drawn = own || (Swings.SwingTrails.On && swingLook != null && swingLook.ReplacesHitArt) || HitArt.Play(element, at + Vector3.up * 1.1f, forward);
            if (!drawn) ElementLook.Sparks(element, at + Vector3.up * 1.1f, forward, blow.Third ? 1.2f : 0.7f);
            switch (element.Name)
            {
                case Elements.Fire:
                    // The third leaves the ground under its target burning: a 1.5 m patch for 3 s, a Burn a second.
                    if (blow.Third) Hazards.Lay(HazardKind.Fire, Ground(at), 1.5f, 3f, 3f, 0f, 4f, 1f, struck: Swings.SwingLooks.Fire.Tint);
                    break;
                case Elements.Lightning:
                {
                    // Each charged blow leaps on to one more enemy within 4 m for half its damage, laying Static there too.
                    var next = combat.EnemyNear(at, 4f, combat.Struck);
                    if (next != null)
                    {
                        LightningBolts.Strike(next.transform.position, 0.3f);
                        combat.StrikeOne(next, Blow(element, damage * 0.5f, 6f, 1), at);
                    }
                    break;
                }
                case Elements.Wind:
                    // Each shoves its target along the swing, 1.5 m; the third throws it 4 m.
                    // NEEDS: the 20 damage and 30 stagger when what is thrown meets a wall or another enemy.
                    if (target is IShovable shoved) shoved.Push(forward * (blow.Third ? 4f : 1.5f) * combat.StatPush);
                    if (!own) Vfx.Play(VfxKind.Dust, Ground(at), forward, blow.Third ? 1.6f : 0.8f);
                    break;
                case Elements.Water:
                    // Each splashes everything within 1.5 m of its target for a fifth of the blow, and soaks it.
                    combat.StrikeArea(at, 1.5f, Blow(element, damage * 0.2f, 4f, 1));
                    if (!ElementArt.Ready) FormFx.Lay(FormMark.Ring, Ground(at), 1.5f, FormKit.Lit(element.Colour, 1.6f, 0.8f), 0.35f, 3f);
                    break;
                case Elements.Life:
                    // NEEDS: the seed that stays and the sapling three seeds make. For now the third throws thorns round its target.
                    if (blow.Third)
                    {
                        combat.StrikeArea(at, 2f, Blow(element, 8f, 6f, 1));
                        for (int i = 0; i < 3 && !own; i++)
                            FormFx.Erupt(FormMesh.Vine, Ground(at) + Quaternion.Euler(0f, i * 120f, 0f) * Vector3.forward * 0.9f, Quaternion.Euler(0f, i * 120f, 0f), new Vector3(0.5f, 1.2f, 0.5f), element.Colour, 0.12f, 0.5f, 0.4f);
                    }
                    break;
                case Elements.Earth:
                    // Each throws a short spray of gravel: a 2 m cone, 4 damage. NEEDS: the wound-up blow that cannot be stopped (unshaken).
                    combat.StrikeCone(2.6f, 60f, Blow(element, 4f, 4f, 0));
                    if (!own) Vfx.Play(VfxKind.Dust, Ground(at), forward, 1.2f);
                    break;
                case Elements.Light:
                    // The third throws a ray 5 m along the swing that Marks everything on it.
                    if (blow.Third)
                    {
                        Vector3 from = combat.transform.position;
                        combat.StrikeLane(from, forward, 5f, 0.6f, Blow(element, 6f, 6f, 1));
                        if (!ElementArt.Ready) FormFx.Run(FormMark.Beam, Ground(from) + Vector3.up * 0.9f, forward, 5f, 0.5f, FormKit.Lit(element.Colour, 2.2f), 0.35f, 0.1f);
                    }
                    break;
                case Elements.Dark:
                    // NEEDS: the shadow cut loose (an enemy state, shadow-cut-loose). For now the third sinks smoke at its feet and Curses twice more.
                    if (blow.Third && target is IAfflictable cursed)
                    {
                        cursed.Afflictions.Lay(AfflictionKind.Cursed, 2, 0f, 1f, LaySource.Weapon, target);
                        if (!ElementArt.Ready) Vfx.Play(VfxKind.TintPuff, Ground(at) + Vector3.up * 0.3f, Vector3.down, 1.4f, ElementSubstance.Shade(element.Colour));
                    }
                    break;
                case Elements.Poison:
                    // The third makes the poison bite at once, out of its beat.
                    if (blow.Third && target is IAfflictable poisoned)
                    {
                        float bite = poisoned.Afflictions.SetOff(AfflictionKind.Poison) * 0.5f;
                        if (bite > 0f) combat.StrikeOne(target, Blow(element, bite, 4f, 1), combat.transform.position);
                    }
                    break;
                // Ice shatters before the blow (BeforeChargedBlow); Metal passes the guard (PassesGuard).
            }
        }

        // ---- The infused strike.

        /// <summary>
        /// Bends the carrier before it is started (IMPLEMENTATION.md 3.4): Lightning's Flash Cut is a lunge of 5 m as a
        /// line of lightning that cuts at the end for one and a half times the blow. The others keep the carrier's shape.
        /// </summary>
        public static void Bend(AttackStep step, string element, bool deep) => Bend(step, element, deep, false);

        /// <summary>
        /// As above; <paramref name="inPlace"/> keeps the carrier's own shape (the Great Hammer's slam lands where the head
        /// comes down): Lightning's blow is still one and a half times, without the lunge.
        /// </summary>
        public static void Bend(AttackStep step, string element, bool deep, bool inPlace)
        {
            if (step == null) return;
            if (element == Elements.Lightning && inPlace) step.Damage *= 1.5f;
            else if (element == Elements.Lightning)
            {
                step.Travel = 5f;
                step.Active = Mathf.Max(step.Active, 0.16f);
                step.PassThrough = true;
                step.Damage *= 1.5f;
            }
        }

        /// <summary>
        /// How long the player cannot be struck from the moment the bent carrier starts: Lightning's Flash Cut is
        /// untouchable through the lunge, its wind-up and the 5 m it covers. The others are open as any blow is.
        /// </summary>
        public static float UntouchableSeconds(AttackStep step, string element)
        {
            if (step == null || element != Elements.Lightning) return 0f;
            return step.Windup + step.Active;
        }

        /// <summary>
        /// The element's strike, once the carrier's blow is out: on the footprint it covered, with <paramref name="first"/>
        /// the first enemy it struck (null when it struck none), <paramref name="blowDamage"/> the carrier's damage.
        /// </summary>
        public static void Strike(PlayerCombat combat, ElementInfo element, in Footprint print, bool deep, float blowDamage, Component first)
        {
            if (combat == null || element == null) return;
            Color colour = element.Colour;
            // The element art draws the strike once its blows are dealt (StrikeArt, below); the old pieces stay for the switch.
            bool art = ElementArt.Ready;
            switch (element.Name)
            {
                case Elements.Fire:
                    // Checked against elements_a.md when Fire was reshaped (task 4.2): the third blow's patch above, Burning
                    // Edge and Burnt Offering here are the design's numbers.
                    // Burning Edge: a sheet of flame along the arc and 3 m beyond, 2 Burn on all it reaches, and the ground along
                    // the cut burning for about a second (the owner's review, 2026-11-21: three seconds read as flame out of nothing).
                    Cover(combat, print, Blow(element, blowDamage * 0.25f, 8f, 2), 3f);
                    // The Burn bites at once, but the bodies' own flames wait for the strike's hero flame to play out
                    // (the owner's answer, 2026-11-22), then fade in.
                    foreach (var one in combat.LastStruck)
                        if (one is Component body && body != null) BodyFlames.HoldBack(body.transform, BodyFlames.AfterStrike);
                    if (!art) Show(combat, element, print, 3f);
                    Burn(print);
                    if (deep)
                    {
                        // Burnt Offering: a pillar of flame on the enemy struck, 30 damage, and its burn goes off.
                        Vector3 at = first != null ? first.transform.position : print.Far;
                        combat.StrikeArea(at, 1.6f, new SpellBlow { Damage = 30f, Stagger = 30f, Affliction = AfflictionKind.Burn, Stacks = 1, Detonate = true, Heavy = true, Effect = VfxKind.Fire, Struck = Swings.SwingLooks.Fire.Tint });
                        FormFx.Erupt(FormMesh.Flame, Ground(at), Quaternion.identity, new Vector3(1.4f, 4.2f, 1.4f), colour, 0.1f, 0.35f, 0.4f);
                        if (!art) Vfx.Play(VfxKind.Fire, at + Vector3.up * 1.4f, Vector3.up, 2.4f);
                        Vfx.Play(VfxKind.Embers, at + Vector3.up * 2f, Vector3.up, 2.2f);
                        Vfx.Play(VfxKind.Smoke, at + Vector3.up * 3f, Vector3.up, 1.6f);
                    }
                    break;
                case Elements.Lightning:
                    // Flash Cut: the lunge was the carrier, bent; its end lays 2 Static on what it cut.
                    combat.StrikeLane(print.Origin, print.Forward, print.Reach, 0.9f, Blow(element, 0f, 6f, 2));
                    if (!art) FormFx.Run(FormMark.Streak, Ground(print.Origin) + Vector3.up * 1f, print.Forward, print.Reach, 0.35f, FormKit.Lit(colour, 3f), 0.3f, 0.05f);
                    // The slam's own bolt has already struck the hammer head.
                    if (!combat.SlamDrawn) LightningBolts.Strike(print.Far, 0.35f);
                    if (deep)
                    {
                        // Down the Channel: 0.4 s later a bolt comes down along the whole line and discharges every enemy on it.
                        // NEEDS: the discharge itself (checked in task 4.3): the bolt strikes and lays Static; a blow's Detonate sets
                        // off Burn only, so what lies on the line waits for a heavy blow to go off.
                        for (int i = 0; i < 4; i++)
                        {
                            Vector3 at = print.Origin + print.Forward * (print.Reach * (i + 0.5f) / 4f);
                            Hazards.Lay(new HazardSpec
                            {
                                Kind = HazardKind.Lightning, Position = Ground(at), Radius = 1.2f, Seconds = 0.15f, Delay = 0.4f, Once = true, Burst = true, BiteEvery = 0.1f,
                                Blow = Blow(element, 16f, 20f, 2),
                            });
                        }
                    }
                    break;
                case Elements.Wind:
                    // Cutting Wind: a crescent 7 m along the arc, 60% of the blow, 2 Unsteady and a 1 m push.
                    Cover(combat, print, Blow(element, blowDamage * 0.6f, 10f, 2), 7f - print.Reach);
                    if (!art) Show(combat, element, print, 7f - print.Reach);
                    PushStruck(combat, print.Forward, 1f);
                    if (deep && first != null)
                    {
                        // Whirl Cut: a small whirlwind that holds the first enemy it reaches in the air for 1 s.
                        combat.LiftArea(first.transform.position, 1f, 1f);
                        if (!art) Vfx.Play(VfxKind.TintPuff, first.transform.position + Vector3.up * 0.6f, Vector3.up, 2f, colour);
                    }
                    break;
                case Elements.Ice:
                {
                    // Frost Bite: a spike of ice stands up through each enemy struck: 2 Chill, held to the spot 0.8 s.
                    Cover(combat, print, Blow(element, 0f, 8f, 2), 0f);
                    int count = combat.CopyLastStruck(out var struck);
                    for (int i = 0; i < count; i++)
                    {
                        var one = struck[i];
                        Vector3 at = one.transform.position;
                        combat.HoldArea(at, 0.6f, 0.8f);
                        if (!art) FormFx.Erupt(FormMesh.Crystal, Ground(at), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), new Vector3(0.7f, 2.4f, 0.7f), colour, 0.08f, 0.7f, 0.3f);
                        // Ice Through: frozen at once when it already carries three Chill or more.
                        if (deep && one is IAfflictable cold && cold.Afflictions.Stacks(AfflictionKind.Chill) >= 3)
                            cold.Afflictions.Lay(AfflictionKind.Chill, Afflictions.Depth(AfflictionKind.Chill), 0f, 1f, LaySource.Weapon, one);
                    }
                    if (!art) Vfx.Play(VfxKind.TintPuff, Ground(print.Centre) + Vector3.up * 0.4f, Vector3.up, 1.4f, ElementSubstance.Pale(colour));
                    // NEEDS: the next blow breaking the spike for 15 damage in 2 m.
                    break;
                }
                case Elements.Water:
                    // Lash: a whip of water twice the weapon's reach, 80% of the blow, Soaked, pulled 1.5 m in.
                    combat.StrikeLane(print.Origin, print.Forward, Mathf.Max(4f, print.Reach * 2f), 0.8f, Blow(element, blowDamage * 0.8f, 10f, 1));
                    if (!art) FormFx.Run(FormMark.Lane, Ground(print.Origin) + Vector3.up * 0.05f, print.Forward, Mathf.Max(4f, print.Reach * 2f), 0.7f, FormKit.Lit(colour, 1.8f, 0.8f), 0.4f, 0.2f);
                    // Thrown on the Reef: the wave throws it 5 m away instead. NEEDS: the 30 damage on meeting a wall.
                    PushStruck(combat, print.Forward, deep ? 5f : -1.5f);
                    break;
                case Elements.Life:
                {
                    // Grown Into It: a cutting set in each enemy struck: 2 Seeded. Briar Root: rooted 1 s.
                    Cover(combat, print, Blow(element, 0f, 6f, 2), 0f);
                    int count = combat.CopyLastStruck(out var struck);
                    for (int i = 0; i < count; i++)
                    {
                        Vector3 at = struck[i].transform.position;
                        if (!art) FormFx.Erupt(FormMesh.Vine, Ground(at), Quaternion.Euler(0f, Random.Range(0f, 360f), 0f), new Vector3(0.6f, 1.6f, 0.6f), colour, 0.15f, 1.2f, 0.4f);
                        if (deep) combat.HoldArea(at, 0.6f, 1f);
                    }
                    // NEEDS: the cutting counting as something grown, and the thorn thrown to a neighbour on each blow.
                    break;
                }
                case Elements.Earth:
                {
                    // Broken Ground: four slabs in a 4 m line, 14 damage, 25 stagger, Tremor 1. Mountain Cleaver: 6.5 m, 55 and 70, Tremor 2.
                    float length = deep ? 6.5f : 4f;
                    combat.StrikeLane(print.Origin, print.Forward, length, deep ? 1.4f : 1f, Blow(element, deep ? 55f : 14f, deep ? 70f : 25f, deep ? 2 : 1, deep));
                    // The elements 3 look heaves its own slabs up the crack (StrikeArt): these are the old look's.
                    int slabs = art && !deep && ElementMoves.HasStrike(element.Index) ? 0 : deep ? 5 : 4;
                    for (int i = 0; i < slabs; i++)
                    {
                        Vector3 at = print.Origin + print.Forward * (1f + i * (length - 1f) / Mathf.Max(1, slabs - 1));
                        FormFx.Erupt(FormMesh.Slab, Ground(at), Quaternion.LookRotation(print.Forward) * Quaternion.Euler(0f, Random.Range(-15f, 15f), 0f),
                            deep ? new Vector3(1.4f, 1.1f, 0.8f) : new Vector3(1.1f, 0.7f, 0.6f), colour, 0.08f, 2f, 0.4f);
                        Vfx.Play(VfxKind.Dust, Ground(at), Vector3.up, 0.8f);
                    }
                    break;
                }
                case Elements.Metal:
                {
                    // Struck Plate: a sharp ripple 3 m from the one struck, 10 damage and a notch. Red Iron: a cut half again as hard, two notches.
                    Vector3 at = first != null ? first.transform.position : print.Far;
                    combat.StrikeArea(at, 3f, Blow(element, 10f, 12f, 1));
                    if (!art)
                    {
                        FormFx.Lay(FormMark.Ring, Ground(at), 0.5f, FormKit.Lit(colour, 2.4f), 0.3f, 6f);
                        Vfx.Play(VfxKind.GuardSpark, at + Vector3.up * 1.1f, print.Forward, 1.4f);
                    }
                    if (deep && first != null) combat.StrikeOne(first, Blow(element, blowDamage * 0.5f, 20f, 2, true), print.Origin);
                    break;
                }
                case Elements.Light:
                {
                    // Ray Along the Edge: a ray follows the swing 6 m out, 10 damage, 15 stagger, Marks.
                    combat.StrikeLane(print.Origin, print.Forward, 6f, 0.7f, Blow(element, 10f, 15f, 1));
                    if (!art) FormFx.Run(FormMark.Beam, Ground(print.Origin) + Vector3.up * 0.9f, print.Forward, 6f, 0.6f, FormKit.Lit(colour, 2.4f), 0.35f, 0.15f);
                    if (deep)
                    {
                        // The Sun on the Blade: the same cut opens at half strength on every Marked enemy within 8 m.
                        int count = combat.Gather(print.Origin, 8f, out var near);
                        for (int i = 0; i < count; i++)
                        {
                            var one = near[i];
                            if (!(one is IAfflictable marked) || !marked.Afflictions.Has(AfflictionKind.Marked)) continue;
                            combat.StrikeOne(one, Blow(element, blowDamage * 0.5f, 10f, 0), print.Origin);
                            FormFx.Erupt(FormMesh.Ray, Ground(one.transform.position), Quaternion.identity, new Vector3(0.5f, 3.5f, 0.5f), colour, 0.06f, 0.2f, 0.3f);
                        }
                    }
                    break;
                }
                case Elements.Dark:
                    // Taken: the cut drags each enemy struck 2 m toward you, 12 damage, 20 stagger, Cursed.
                    Cover(combat, print, Blow(element, 12f, 20f, 1), 0f);
                    PushStruck(combat, print.Forward, -2f);
                    if (!art) Vfx.Play(VfxKind.TintPuff, Ground(print.Centre) + Vector3.up * 0.2f, Vector3.down, 1.2f, ElementSubstance.Shade(colour));
                    if (deep)
                    {
                        // Yama's Noose: a black rope to the enemy faced within 8 m: 40 damage and 50 stagger through any guard.
                        var faced = combat.EnemyAhead(8f, 60f);
                        if (faced != null)
                        {
                            var body = faced.GetComponentInParent<IDamageable>() as Component;
                            if (body != null) combat.StrikeOne(body, Blow(element, 40f, 50f, 1, true), print.Origin);
                            FormFx.Cut(Ground((print.Origin + faced.position) * 0.5f) + Vector3.up, faced.position - print.Origin, Vector3.Distance(print.Origin, faced.position), 0.1f, FormKit.Stain(colour, 0.9f), 0.5f);
                        }
                    }
                    break;
                case Elements.Poison:
                {
                    // Venom Flick: a string of venom 5 m ahead; a bubbling puddle 2 m across lies 3 s, dosing once a second.
                    Vector3 at = print.Origin + print.Forward * 5f;
                    combat.StrikeLane(print.Origin, print.Forward, 5f, 0.5f, Blow(element, 4f, 4f, 1));
                    Hazards.Lay(new HazardSpec
                    {
                        Kind = HazardKind.Element, Tint = colour, Position = Ground(at), Radius = 1f, Seconds = 3f, BiteEvery = 1f,
                        Blow = Blow(element, 2f, 0f, 1),
                    });
                    if (!art) FormFx.Lay(FormMark.Disc, Ground(at) + Vector3.up * 0.03f, 2f, FormKit.Lit(colour, 1.4f, 0.7f), 3f);
                    // Nine Times Refined (simplest version): the cut itself lays six doses on what it struck.
                    if (deep) Cover(combat, print, Blow(element, 0f, 10f, 6), 0f);
                    break;
                }
            }
            // The Great Hammer's slam has already drawn this strike where the head landed (PlayerCombat.SlamLands).
            if (art && !combat.SlamDrawn)
            {
                int count = combat.CopyLastStruck(out var lastStruck);
                // Which way the blade went round: the strike's look runs with it (a strike lands once in eight seconds).
                var trails = combat.GetComponent<Clover.Player.Swings.SwingTrails>();
                StrikeArt.Play(element, print, first, lastStruck, count, trails != null ? trails.Swept : 0f);
            }
        }

        // ---- Pieces.

        private static Vector3 Ground(Vector3 at) => new Vector3(at.x, at.y + 0.05f, at.z);

        private static SpellBlow Blow(ElementInfo element, float damage, float stagger, int stacks, bool heavy = false) => new SpellBlow
        {
            Damage = damage,
            Stagger = stagger,
            Affliction = stacks > 0 ? element.Affliction : AfflictionKind.None,
            Stacks = stacks,
            Heavy = heavy,
            Effect = ElementLook.HitEffect(element),
            Tinted = ElementLook.Tints(element),
            Tint = element.Colour,
            Look = element,
            // The weapon carries it: what it strikes flashes in the trail's colour, softened (the owner's choice, 2026-11-19).
            Struck = Swings.SwingLooks.For(element)?.Tint ?? default,
        };

        // Strikes the footprint, reaching further by the metres given: an arc as a cone, a ring and a disc as circles, a lane and a path as lanes.
        private static void Cover(PlayerCombat combat, in Footprint print, in SpellBlow blow, float further)
        {
            further = Mathf.Max(0f, further);
            switch (print.Shape)
            {
                case FootprintShape.Arc: combat.StrikeCone(print.Reach + further, Mathf.Max(60f, print.ArcDegrees), blow); break;
                case FootprintShape.Ring: combat.StrikeArea(print.Origin, print.Reach + further, blow); break;
                case FootprintShape.Disc: combat.StrikeArea(print.Origin + print.Forward * print.Reach, print.Radius + further * 0.5f, blow); break;
                default: combat.StrikeLane(print.Origin, print.Forward, print.Reach + further, Mathf.Max(0.6f, print.Width * 0.5f), blow); break;
            }
        }

        // The element thrown along the footprint's outer edge: flame for fire, crackle for lightning, motes of the colour for the rest.
        private static void Show(PlayerCombat combat, ElementInfo element, in Footprint print, float further)
        {
            float reach = print.Reach + Mathf.Max(0f, further);
            if (print.Shape == FootprintShape.Arc || print.Shape == FootprintShape.Ring)
            {
                float arc = print.Shape == FootprintShape.Ring ? 360f : Mathf.Max(60f, print.ArcDegrees);
                combat.ShowArc(reach, arc, new Color(element.Colour.r * 1.9f, element.Colour.g * 1.9f, element.Colour.b * 1.9f, 0.9f));
                int points = print.Shape == FootprintShape.Ring ? 10 : 6;
                for (int i = 0; i < points; i++)
                {
                    float t = points == 1 ? 0.5f : i / (float)(points - 1);
                    Vector3 outward = Quaternion.Euler(0f, (t - 0.5f) * arc, 0f) * print.Forward;
                    for (int ring = 1; ring <= 2; ring++)
                        ElementLook.Puff(element, Ground(print.Origin + outward * (reach * ring / 2f)) + Vector3.up * 0.4f, Vector3.up, 0.9f);
                }
                return;
            }
            Vector3 centre = print.Shape == FootprintShape.Disc ? print.Origin + print.Forward * print.Reach : print.Origin;
            if (print.Shape == FootprintShape.Disc)
            {
                FormFx.Lay(FormMark.Ring, Ground(centre), print.Radius, FormKit.Lit(element.Colour, 2f), 0.4f, print.Radius + further * 0.5f);
                for (int i = 0; i < 6; i++)
                    ElementLook.Puff(element, Ground(centre + Quaternion.Euler(0f, i * 60f, 0f) * Vector3.forward * (print.Radius + further * 0.5f)) + Vector3.up * 0.4f, Vector3.up, 0.9f);
                return;
            }
            for (int i = 0; i < 6; i++)
                ElementLook.Puff(element, Ground(centre + print.Forward * (reach * (i + 0.5f) / 6f)) + Vector3.up * 0.4f, Vector3.up, 0.9f);
        }

        /// <summary>How long the infused strike's ground fire burns before the player's numbers: about a second (the owner's review, 2026-11-21).</summary>
        public const float StrikeBurnSeconds = 1f;

        // How long the last flames a patch of ground fire threw stay up after it is out (the Fire puff's life, about).
        private const float FlameLinger = 0.3f;

        // Fire left on the ground along the cut itself, not out past it, so it reads as the blade's own burn. Only the
        // infused strike lays this, so the trail, the crater, the flask and the spells keep their own burning times.
        private static void Burn(in Footprint print)
        {
            if (print.Shape == FootprintShape.Arc || print.Shape == FootprintShape.Ring)
            {
                float arc = print.Shape == FootprintShape.Ring ? 300f : Mathf.Max(60f, print.ArcDegrees);
                for (int i = 0; i < 4; i++)
                {
                    Vector3 outward = Quaternion.Euler(0f, (i / 3f - 0.5f) * arc * 0.8f, 0f) * print.Forward;
                    BurnPatch(Ground(print.Origin + outward * (print.Reach * 0.85f)), 0.85f);
                }
                return;
            }
            if (print.Shape == FootprintShape.Disc)
            {
                for (int i = 0; i < 3; i++)
                    BurnPatch(Ground(print.Centre + Quaternion.Euler(0f, i * 120f, 0f) * print.Forward * (print.Radius * 0.5f)), 0.85f);
                return;
            }
            for (int i = 0; i < 3; i++)
                BurnPatch(Ground(print.Origin + print.Forward * (print.Reach * (i + 0.5f) / 3f)), 0.8f);
        }

        // One patch of the strike's ground fire: the flames (the hazard's own real fire) stand on char with ember cracks
        // glowing in it (StrikeScorch, the burning floors' own look), a little wider than the flames, so they burn on
        // something and not on bare stone. Bites three times in its second.
        private static void BurnPatch(Vector3 at, float radius)
        {
            Hazards.Lay(HazardKind.Fire, at, radius, StrikeBurnSeconds, 5f, 0f, 4f, 0.35f, struck: Swings.SwingLooks.Fire.Tint);
            // The embers glow as long as the player's numbers make fire on the ground last; the char outlives them a little.
            StrikeScorch.Lay(at, radius * 1.35f, StrikeBurnSeconds * Hazards.FireSecondsScale + FlameLinger, 0.5f);
            Vfx.Play(VfxKind.Embers, at + Vector3.up * 0.2f, Vector3.up, 0.6f);
        }

        // Pushes every enemy the last strike reached along the strike (negative: toward the player).
        private static void PushStruck(PlayerCombat combat, Vector3 forward, float metres)
        {
            foreach (var one in combat.LastStruck)
                if (one is IShovable shovable) shovable.Push(forward * metres);
        }
    }
}

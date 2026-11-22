using Clover.Core;
using Clover.Player.Forms;
using Clover.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Clover.Player
{
    /// <summary>
    /// The player's elements: the two attunements of the Hand (IMPLEMENTATION.md 3.6), each with its kit (its base
    /// spell, cast from a spell slot by <see cref="PlayerSpells"/> through <see cref="CastBase(string)"/>, and its
    /// surge), the surge the Hand wears, and how they show. The rules and numbers are in <see cref="ElementKit"/>; a
    /// spell is thrown by a <see cref="ISpellForm"/>, the same way the spells the tree teaches are. Like the weapon and
    /// the guard it has no Update of its own for the rules: <see cref="PlayerController"/> calls <see cref="Advance"/>
    /// so everything is settled in one order.
    ///
    /// <see cref="Kit"/>, <see cref="Info"/> and <see cref="Colour"/> are the FIRST attunement's: the element the
    /// weapon carries (only its affliction lays on weapon blows, the owner's rule of 2026-11-18), the chart's tree, the
    /// HUD's mana colour. A spell is cast in its own element (<see cref="ElementOf"/>), the surge in the surge's
    /// (<see cref="SurgeKit"/>).
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerElement : MonoBehaviour
    {
        [SerializeField] private Light glow;
        [SerializeField] private SlashArcs arcs;
        [SerializeField] private TopDownCamera shaker;

        /// <summary>The element the player starts the scene with, unless the run already carries one.</summary>
        public string StartingElement = ElementKit.FireName;
        /// <summary>
        /// Set by the scene's builder for a land the player enters with no element (Fire's): unless the run already
        /// carries one, there is no spell, no surge, no colour and no affliction until an element is taken.
        /// </summary>
        public bool StartsWithNone;
        /// <summary>False while the player holds no element at all.</summary>
        public bool HasElement { get; private set; } = true;
        // The surge slot's third choice: a duo surge of the carried pair, with a kit of its own for its price and clock.
        private const int DuoIndex = 2;
        private Upgrade duoMark;
        private ElementKit duoKit;
        private ElementInfo duoInfo;
        /// <summary>Seconds a surge press is remembered while the player is busy.</summary>
        public float PressBufferTime = 0.25f;
        public float GlowWhenCasting = 9f;
        public float GlowWhenSurging = 2.6f;
        /// <summary>Seconds between the sparks or embers that fall from the player while the surge lasts.</summary>
        public float EmberInterval = 0.12f;

        /// <summary>The first attunement's kit.</summary>
        public ElementKit Kit => kits[0];
        /// <summary>What is fixed about the first attunement: its place on the ring, its colour, its affliction.</summary>
        public ElementInfo Info => infos[0];
        /// <summary>The second attunement's kit and element, or null when only one element is carried. All in, they are the first's.</summary>
        public ElementKit SecondKit => kits[1];
        public ElementInfo SecondInfo => infos[1];
        /// <summary>True when the second attunement is a different element from the first: a mix.</summary>
        public bool Mixed => HasElement && infos[1] != null && infos[1] != infos[0];
        /// <summary>True when the same element is carried twice.</summary>
        public bool AllIn => HasElement && infos[1] != null && infos[1] == infos[0];
        /// <summary>The kit whose surge a tap of L2 lights (the Hand's surge slot) and its element: an element's, or a duo surge's.</summary>
        public ElementKit SurgeKit => surgeIndex == DuoIndex ? duoKit ?? kits[0] : kits[surgeIndex] ?? kits[0];
        public ElementInfo SurgeInfo => surgeIndex == DuoIndex ? duoInfo ?? infos[0] : infos[surgeIndex] ?? infos[0];
        /// <summary>The duo surge worn in the surge slot, or null while an element's surge is worn.</summary>
        public Upgrade DuoSurgeWorn => surgeIndex == DuoIndex ? duoMark : null;
        /// <summary>The id of the surge worn, as the Hand keeps it: an element's (<c>fire.surge</c>) or a duo surge's mark id.</summary>
        public string SurgeId => DuoSurgeWorn != null ? DuoSurgeWorn.Id : SurgeInfo != null ? Build.HandRules.SurgeId(SurgeInfo.Name) : "";
        /// <summary>True while the worn surge burns.</summary>
        public bool SurgeActive => SurgeKit != null && SurgeKit.SurgeActive;
        /// <summary>The first element's colour as light; brighter than white so it blooms.</summary>
        public Color Colour { get; private set; }
        /// <summary>True while a base spell is being cast, from either kit.</summary>
        public bool IsCasting => (kits[0] != null && kits[0].IsCasting) || (kits[1] != null && kits[1].IsCasting);
        /// <summary>How far through its cast the base spell being cast is, for the animator.</summary>
        public float CastProgress => castingKit != null ? castingKit.CastProgress : 0f;
        /// <summary>Goes up whenever an attunement or the surge changes: the HUD and the slots look again then.</summary>
        public int Version { get; private set; }
        public int SpellsCast { get; private set; }
        public int Surges { get; private set; }
        public int SpellHits { get; private set; }
        /// <summary>
        /// The number of the slot cast being made now, set by <see cref="PlayerSpells"/> around a cast and carried by
        /// every blow of it (<see cref="SpellCast.Cast"/>), so the blade is charged by that cast's own landing. 0 outside one.
        /// </summary>
        public int CastTag { get; set; }

        private PlayerController player;
        private PlayerCombat combat;
        private PlayerDefence defence;
        private PlayerUpgrades upgrades;
        // Looked up when first wanted: the controller adds the slots to a player built before they existed, perhaps after this Awake.
        private PlayerSpells spellsFound;
        private PlayerSpells spells => spellsFound != null ? spellsFound : (spellsFound = GetComponent<PlayerSpells>());
        private readonly ElementKit[] kits = new ElementKit[2];
        private readonly ElementInfo[] infos = new ElementInfo[2];
        private readonly SpellSpec[] baseSpells = { new SpellSpec(), new SpellSpec() };
        private int surgeIndex;
        private ElementKit castingKit;
        private int castingIndex;
        private int releaseTag;
        private float surgeBuffered;
        private float surgeHeldShown;
        private float glowLevel;
        private float untilEmber;
        private float surgeHealOwed;

        public void Configure(Light glowLight, SlashArcs arcsToShow, TopDownCamera cameraToShake)
        {
            glow = glowLight;
            arcs = arcsToShow;
            shaker = cameraToShake;
        }

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            combat = GetComponent<PlayerCombat>();
            defence = GetComponent<PlayerDefence>();
            upgrades = GetComponent<PlayerUpgrades>();
            outfit = GetComponent<PlayerOutfit>();
            sheen = new MetalSheen();
            glaze = new GlazeIce();
            armour = new StoneArmour();
            fever = new SurgeSkin(SurgeSkin.Kind.Fever);
            charge = new SurgeSkin(SurgeSkin.Kind.Charge);
            if (Elements.StartsEmpty(StartsWithNone, RunState.Element)) TakeNone();
            else
            {
                string first = string.IsNullOrEmpty(RunState.Element) ? StartingElement : RunState.Element;
                Take(ElementKit.ByName(first, RunState.ElementConsumed));
                // The run carries the second attunement and the surge worn from scene to scene.
                if (!string.IsNullOrEmpty(RunState.Element2)) Attune(first, RunState.Element2);
                WearSurgeById(RunState.Surge);
            }
            if (glow != null) glow.intensity = 0f;
        }

        // Holding nothing. A kit is kept so that nothing that reads one finds a hole, but it is never cast from:
        // the buttons do nothing, the colour is the plain grey of a blow with no element in it, and nothing is laid.
        private void TakeNone()
        {
            HasElement = false;
            kits[0] = ElementKit.ByName(ElementKit.FireName, false);
            kits[0].Reset();
            infos[0] = Elements.None;
            kits[1] = null;
            infos[1] = null;
            surgeIndex = 0;
            castingKit = null;
            Colour = infos[0].Glow;
            ElementSubstance.Hold(null, null);
            if (glow != null) glow.color = infos[0].Colour;
            Version++;
        }

        /// <summary>
        /// Puts an element's kit in the first attunement, in place of the one there (an altar, a god, the yard's stone);
        /// the second attunement is left as it is. Any surge running on the old first ends.
        /// </summary>
        public void Take(ElementKit kit)
        {
            if (kit == null) return;
            var keptSecond = HasElement && infos[1] != null ? infos[1].Name : "";
            SetAttunements(kit, keptSecond);
        }

        /// <summary>
        /// Carries these two elements (IMPLEMENTATION.md 3.6; "" for none in the second). An element already carried
        /// keeps its kit, so its surge and the surge's wait go on; a kit that leaves takes its running surge with it. A
        /// surge not of a carried element falls back to the first's.
        /// </summary>
        public void Attune(string first, string second)
        {
            if (string.IsNullOrEmpty(first))
            {
                TakeNone();
                return;
            }
            SetAttunements(KitFor(first), second ?? "");
        }

        /// <summary>The surge a tap of L2 lights, by the Hand's id: an element's (<c>fire.surge</c>) or a duo surge's mark id.</summary>
        public void WearSurgeById(string id)
        {
            var mark = Upgrade.ById(id);
            if (DuoSurge.Is(mark)) WearDuoSurge(mark);
            else WearSurge(Build.HandRules.ElementOfSurge(id));
        }

        /// <summary>
        /// Wears a duo surge in the surge slot while both its elements are carried; otherwise the first element's surge. It
        /// has its own kit, so its own price and time.
        /// </summary>
        public void WearDuoSurge(Upgrade mark)
        {
            if (!DuoSurge.Is(mark) || !Mixed || !Carries(mark.Element) || !Carries(mark.Element2))
            {
                WearSurge(Info != null ? Info.Name : "");
                return;
            }
            if (surgeIndex == DuoIndex && duoMark == mark) return;
            EndSurgeShown();
            if (duoMark != mark)
            {
                duoMark = mark;
                duoKit = DuoSurge.KitFor(mark);
                duoInfo = Elements.ByNameOrFire(mark.Element);
            }
            surgeIndex = DuoIndex;
            Version++;
        }

        /// <summary>The surge a tap of L2 lights: a carried element's, by name. Any other name wears the first's.</summary>
        public void WearSurge(string elementName)
        {
            int index = Mixed && infos[1] != null && infos[1].Name == elementName ? 1 : 0;
            if (index == surgeIndex) return;
            EndSurgeShown();
            surgeIndex = index;
            Version++;
        }

        /// <summary>True when the element is in either attunement.</summary>
        public bool Carries(string elementName)
        {
            if (!HasElement || string.IsNullOrEmpty(elementName)) return false;
            return (infos[0] != null && infos[0].Name == elementName) || (infos[1] != null && infos[1].Name == elementName);
        }

        /// <summary>The kit of a carried element, or null.</summary>
        public ElementKit KitOf(string elementName)
        {
            if (!HasElement || string.IsNullOrEmpty(elementName)) return null;
            if (infos[0] != null && infos[0].Name == elementName) return kits[0];
            if (infos[1] != null && infos[1].Name == elementName) return kits[1];
            return null;
        }

        /// <summary>The element a mark's spell or effect is made of: its own (a duo mark's first), or the first attunement's for one with none.</summary>
        public ElementInfo ElementOf(Upgrade mark)
        {
            var own = mark != null ? Elements.ByName(mark.Element) : null;
            return own ?? Info;
        }

        // A kit for the first attunement: the one already carried when it is the same element, else a fresh one.
        private ElementKit KitFor(string elementName)
        {
            var kept = KitOf(elementName);
            if (kept != null) return kept;
            bool consumed = RunState.ElementConsumed && RunState.Element == elementName;
            return ElementKit.ByName(elementName, consumed);
        }

        private void SetAttunements(ElementKit firstKit, string second)
        {
            bool wasHolding = HasElement;
            var oldFirst = wasHolding ? kits[0] : null;
            var oldSecond = wasHolding ? kits[1] : null;
            var oldSurge = wasHolding ? SurgeKit : null;
            bool surging = oldSurge != null && oldSurge.SurgeActive;
            string oldSurgeName = wasHolding && SurgeInfo != null ? SurgeInfo.Name : "";
            var oldFirstInfo = wasHolding ? infos[0] : null;

            // The same element taken again keeps the wait on its surge, so taking it anew is no way round the wait.
            if (firstKit != oldFirst && firstKit != oldSecond)
            {
                var same = oldFirst != null && oldFirst.Name == firstKit.Name ? oldFirst : oldSecond != null && oldSecond.Name == firstKit.Name ? oldSecond : null;
                float wait = same != null ? same.SurgeWaitLeft : 0f;
                firstKit.Reset();
                firstKit.KeepWait(wait);
            }
            HasElement = true;
            kits[0] = firstKit;
            infos[0] = Elements.ByNameOrFire(firstKit.Name);
            if (string.IsNullOrEmpty(second) || Elements.ByName(second) == null)
            {
                kits[1] = null;
                infos[1] = null;
            }
            else if (second == firstKit.Name)
            {
                // All in: one kit, so there is one surge and one wait.
                kits[1] = firstKit;
                infos[1] = infos[0];
            }
            else
            {
                var kept = oldFirst != null && oldFirst.Name == second ? oldFirst : oldSecond != null && oldSecond.Name == second ? oldSecond : null;
                if (kept == null)
                {
                    kept = ElementKit.ByName(second, RunState.ElementConsumed && RunState.Element == second);
                    kept.Reset();
                }
                kits[1] = kept;
                infos[1] = Elements.ByNameOrFire(second);
            }
            // The surge stays on the element it was worn in if that is still carried; else the first's. A duo surge stays
            // while its pair is still carried.
            bool keepDuo = surgeIndex == DuoIndex && duoMark != null && Mixed && Carries(duoMark.Element) && Carries(duoMark.Element2);
            surgeIndex = keepDuo ? DuoIndex : Mixed && infos[1].Name == oldSurgeName ? 1 : 0;
            var newSurge = SurgeKit;
            if (newSurge != oldSurge && surgeShown)
            {
                SurgeLook.End(surgePieces);
                surgeShown = false;
            }
            if (castingKit != null && castingKit != kits[0] && castingKit != kits[1])
            {
                castingKit.Interrupt();
                castingKit = null;
            }
            // Spells are made of the carried elements' stuff.
            ElementSubstance.Hold(infos[0], Mixed ? infos[1] : null);
            Colour = infos[0].Glow;
            // The run remembers every element that has been in the player's hands: a mixed branch opens on it.
            RunState.ElementsHeld.Add(infos[0].Name);
            if (infos[1] != null) RunState.ElementsHeld.Add(infos[1].Name);
            if (glow != null) glow.color = infos[0].Name == Elements.Lightning ? new Color(0.5f, 0.75f, 1f) : infos[0].Name == Elements.Fire ? new Color(1f, 0.5f, 0.16f) : infos[0].Colour;
            bool firstChanged = oldFirstInfo != infos[0];
            if (combat != null && (newSurge != oldSurge || firstChanged)) combat.ClearEmpower();
            // A weapon that is the element's own (the Armament) is made again in the new first element.
            if (firstChanged && combat != null && combat.Weapon != null && combat.Weapon.ElementColoured && !combat.IsAttacking) combat.Arm(Weapons.ArmamentName, infos[0].Name);
            if (defence != null && (firstChanged || newSurge != oldSurge)) defence.ClearWards();
            if (surging && newSurge != oldSurge && upgrades != null) upgrades.OnSurgeEnd();
            if (surging && newSurge != oldSurge) oldSurge.Reset();
            FillBase(baseSpells[0], kits[0]);
            if (kits[1] != null) FillBase(baseSpells[1], kits[1]);
            if (firstChanged || newSurge != oldSurge)
            {
                // Nothing asked of the old element is carried into the new one.
                surgeBuffered = 0f;
                glowLevel = 0f;
                untilEmber = 0f;
                surgeHealOwed = 0f;
            }
            Version++;
        }

        // The base spell's numbers, as its form reads them.
        private static void FillBase(SpellSpec spec, ElementKit kit)
        {
            // The base spell runs on its slot's cooldown and costs no mana (STEEL_AND_ELEMENT.md 3.1).
            spec.Cost = 0f;
            spec.Cooldown = Magic.SpellTimings.Base(kit.Name).Cooldown;
            spec.Damage = kit.SpellDamage;
            spec.Stagger = kit.SpellStagger;
            spec.Reach = kit.SpellReach;
            spec.Arc = kit.SpellArcDegrees;
            spec.Count = kit.SpellLeaps;
            spec.Radius = kit.SpellLeapRange;
            spec.Stacks = kit.SpellStacks;
            spec.AfflictionSeconds = kit.SpellBurnSeconds;
            spec.Seconds = kit.SpellSeconds;
            spec.Interval = kit.SpellInterval;
            spec.LingerDamage = kit.SpellLinger;
            spec.Delay = kit.SpellDelay;
            if (!string.IsNullOrEmpty(kit.SpellForm))
            {
                // A base spell that wears a form reads its numbers as that form does.
                spec.Radius = kit.SpellRadius;
                spec.Count = kit.SpellCount;
            }
        }

        private void EndSurgeShown()
        {
            if (SurgeActive)
            {
                SurgeKit.Reset();
                if (combat != null) combat.ClearEmpower();
                if (upgrades != null) upgrades.OnSurgeEnd();
            }
            if (surgeShown)
            {
                SurgeLook.End(surgePieces);
                surgeShown = false;
            }
        }

        /// <summary>True while the button of the slot the last spell was cast from is down: for a spell that lasts as long as it is held.</summary>
        public bool SpellHeld => spells != null && spells.CastButtonHeld;

        /// <summary>Forgets a button being held, as when a menu or a leap takes the buttons away part-way through a hold.</summary>
        public void DropHolds()
        {
            surgeBuffered = 0f;
            if (spells != null) spells.DropHolds();
        }

        /// <summary>Begins the first element's base spell. See <see cref="CastBase(string)"/>.</summary>
        public bool CastBase() => CastBase(HasElement ? Info.Name : "");

        /// <summary>
        /// Begins a carried element's base spell, as its slot asks: it costs no mana (its slot's cooldown is its price),
        /// roots the player for its short cast and leaves the hand part-way through. False with that element not
        /// carried, or mid-cast.
        /// </summary>
        public bool CastBase(string elementName) => CastBase(elementName, false);

        /// <summary>As above; <paramref name="instant"/> looses it now, with no wind-up (a slot a Master made instant).</summary>
        public bool CastBase(string elementName, bool instant)
        {
            if (!HasElement || combat == null || IsCasting) return false;
            var kit = KitOf(elementName);
            if (kit == null) return false;
            if (!kit.TryCast(combat.Mana, 0f, instant)) return false;
            castingKit = kit;
            castingIndex = kit == kits[0] ? 0 : 1;
            // The cast's number is kept for the moment it leaves the hand.
            releaseTag = CastTag;
            BeginCast();
            if (instant && kit.TickCast(0f)) Release();
            return true;
        }

        /// <summary>A blow or a dodge breaks a cast in progress.</summary>
        public void Interrupt()
        {
            if (kits[0] != null) kits[0].Interrupt();
            if (kits[1] != null) kits[1].Interrupt();
            castingKit = null;
        }

        /// <summary>Draws an arc of the first element's light on the ground in front of the player, as a spell's front.</summary>
        public void ShowArc(float reach, float arcDegrees, float sweep) => ShowArc(reach, arcDegrees, sweep, Colour);

        /// <summary>Draws an arc of a given light: a spell's front in its own element.</summary>
        public void ShowArc(float reach, float arcDegrees, float sweep, Color colour)
        {
            if (arcs != null) arcs.Show(transform.position, transform.forward, reach, arcDegrees, sweep, StruckLook.Arc(colour));
        }

        /// <summary>A cast of a spell in its own element (the mark's; the first attunement's for none). See <see cref="NewCast(SpellSpec, string, Upgrade, ElementInfo)"/>.</summary>
        public SpellCast NewCast(SpellSpec spec, string name, Upgrade source) => NewCast(spec, name, source, ElementOf(source));

        /// <summary>
        /// A cast of a spell in the element given, aimed where the player faces: what a form is handed. The damage,
        /// stagger and reach already carry what the upgrades add to spells. It carries <see cref="CastTag"/>.
        /// </summary>
        public SpellCast NewCast(SpellSpec spec, string name, Upgrade source, ElementInfo element)
        {
            var made = element ?? Info;
            float damage = upgrades != null ? upgrades.Stats.Multiplier(PlayerStat.SpellDamage) : 1f;
            float reach = spec.Reach * (upgrades != null ? upgrades.Stats.Multiplier(PlayerStat.SpellReach) : 1f);
            Transform target = combat.EnemyAhead(reach);
            Vector3 forward = transform.forward;
            // Aimed at the nearest (or farthest) enemy carrying what the spell seeks, before the one faced.
            if (spec.AimAt != AfflictionKind.None)
            {
                var sought = combat.EnemyCarrying(transform.position, reach, spec.AimAt, spec.AimAt2, spec.AimFarthest);
                if (sought != null)
                {
                    target = sought.transform;
                    Vector3 toward = target.position - transform.position;
                    toward.y = 0f;
                    if (toward.sqrMagnitude > 0.01f) forward = toward.normalized;
                }
                else if (spec.AimMissShare > 0f) damage *= spec.AimMissShare;
            }
            // Stronger the lower the caster's health (Debts Called).
            if (spec.HealthScaled > 1f && defence != null) damage *= SpellSpec.HealthScale(spec.HealthScaled, defence.Health.Fraction);
            return new SpellCast
            {
                Name = name,
                Source = source,
                Spec = spec,
                Damage = spec.Damage * damage,
                Stagger = spec.Stagger,
                Reach = reach,
                Player = player,
                Combat = combat,
                Defence = defence,
                Caster = this,
                Upgrades = upgrades,
                Element = made,
                Colour = made.Colour,
                Affliction = made.Affliction,
                Cast = CastTag,
                Target = target,
                Origin = transform.position,
                Forward = forward,
                Aim = target != null ? target.position : transform.position + forward * reach,
            };
        }

        /// <summary>Runs one frame. <paramref name="free"/> is false while the player is rolling, attacking, flinching or drinking.</summary>
        public void Advance(float deltaTime, bool free)
        {
            var buttons = player.Buttons;
            if (buttons == null) return;
            // With no element the surge button does nothing at all.
            if (!HasElement) return;
            // F on the keys, or a tap of L2 on the pad.
            if (buttons.Pressed(PlayerButton.Surge)) surgeBuffered = PressBufferTime;

            var stats = upgrades != null ? upgrades.Stats : null;
            var surgeKit = SurgeKit;
            // The surge is instant and never roots the player, so it can be lit at any free moment.
            if (surgeBuffered > 0f && free)
            {
                surgeBuffered = 0f;
                // A gift that waits on a second press takes it; the surge is not lit over itself.
                if (surgeKit.SurgeActive && upgrades != null && upgrades.ListensForSurge) upgrades.HeardSurge();
                else if (surgeKit.TrySurge(combat.Mana, stats != null ? stats.Multiplier(PlayerStat.SurgeCost) : 1f, stats != null ? stats.Multiplier(PlayerStat.SurgeSeconds) : 1f)) BeginSurge();
                else if (defence != null) defence.Callout(surgeKit.SurgeWaiting ? "Not ready" : "No mana");
            }
            if (surgeBuffered > 0f) surgeBuffered -= deltaTime;

            if (castingKit != null && castingKit.TickCast(deltaTime)) Release();
            if (castingKit != null && !castingKit.IsCasting) castingKit = null;
            if (surgeKit.SurgeActive && surgeKit.SurgeHealPerSecond != 0f && defence != null && !defence.IsDead)
            {
                // Given or taken a point at a time, so the bar moves in steps that can be seen.
                surgeHealOwed += surgeKit.SurgeHealPerSecond * deltaTime * (surgeKit.SurgeHealPerSecond < 0f && stats != null ? stats.Multiplier(PlayerStat.HealthPrices) : 1f);
                if (surgeHealOwed >= 1f)
                {
                    defence.Heal(1f, false);
                    surgeHealOwed -= 1f;
                }
                else if (surgeHealOwed <= -1f)
                {
                    // The hunger never takes the last of it.
                    if (defence.Health.Current > 2f) defence.Health.Chip(1f);
                    surgeHealOwed += 1f;
                }
            }
            bool held = surgeKit.SurgeActive && upgrades != null && upgrades.SurgeHeld;
            if (held)
            {
                // The fire underfoot feeds the surge: its clock stands still, and the flames reach up round the feet to say so.
                surgeHeldShown -= deltaTime;
                if (surgeHeldShown <= 0f)
                {
                    surgeHeldShown = 0.3f;
                    Vector3 feet = transform.position;
                    var surgeColour = SurgeInfo.Glow;
                    Forms.FormFx.Lay(Forms.FormMark.Ring, feet, 2.6f, Forms.FormKit.Lit(surgeColour, 2.6f), 0.3f, 1f);
                    Vector3 outward = Forms.FormRules.OnRing(0, 1, 0.6f, Time.time * 420f);
                    Forms.FormFx.Erupt(Forms.FormMesh.Flame, feet + outward, Quaternion.Euler(0f, Time.time * 200f, 0f), new Vector3(0.6f, 1.5f, 0.6f), surgeColour, 0.08f, 0.2f, 0.3f);
                }
            }
            if (!held && surgeKit.TickSurge(deltaTime))
            {
                combat.ClearEmpower();
                if (upgrades != null) upgrades.OnSurgeEnd();
            }
            // The other kits' surges cannot burn, but a wait they carry still runs down.
            var other = surgeKit == kits[0] ? kits[1] : kits[0];
            if (other != null && other != surgeKit && other.SurgeWaiting) other.TickSurge(deltaTime);
            if (surgeIndex == DuoIndex && kits[1] != null && kits[1] != kits[0] && kits[1].SurgeWaiting) kits[1].TickSurge(deltaTime);

            if (surgeKit.SurgeActive)
            {
                untilEmber -= deltaTime;
                if (untilEmber <= 0f)
                {
                    untilEmber = EmberInterval;
                    // Overdrive's buzzing is the art's jagged arcs now, not the old shock sparks (the owner, 2026-11-19).
                    bool arcs = SurgeInfo != null && SurgeInfo.Name == Elements.Lightning && ArtDrawsSurge(SurgeInfo);
                    // Glaze's ice is grown on the body; the pale sparks thrown round it read as white streaks floating
                    // round the player (the owner, 2026-11-19: "instead of floating around you").
                    bool grown = SurgeInfo != null && SurgeInfo.Name == Elements.Ice && ArtDrawsSurge(SurgeInfo);
                    if (!arcs && !grown) ElementLook.Sparks(SurgeInfo, transform.position + Vector3.up * 1.1f, Vector3.up, 0.25f);
                }
            }
        }

        private readonly int[] surgePieces = { -1, -1, -1, -1, -1, -1 };
        private bool surgeShown;
        // Temper turns the body to steel; whatever outfit is worn, its body renderer is the one asked.
        // Made in Awake: a property block may not be made while Unity constructs the component.
        private MetalSheen sheen;
        // Glaze grows ice out of the body (crystals on its bones, its material frozen over).
        private GlazeIce glaze;
        // Bedrock grows earthen armour over the body (plates on its bones, its material closed over in stone).
        private StoneArmour armour;
        // Fever flushes the skin and Overdrive charges the body (a cyan rim, the hair lifting), each drawn by its material.
        private SurgeSkin fever, charge;
        private PlayerOutfit outfit;

        /// <summary>How strongly Fever's flush and Overdrive's charge are drawn on the body now, 0 to 1, for probes.</summary>
        public float FeverLevel => fever != null ? fever.Level : 0f;
        public float ChargeLevel => charge != null ? charge.Level : 0f;

        /// <summary>How far the body is turned to steel now (Temper), 0 to 1, for probes.</summary>
        public float SteelLevel => sheen != null ? sheen.Level : 0f;

        /// <summary>How far the body is iced over now (Glaze), 0 to 1, and how many crystals stand out of it, for probes.</summary>
        public float IceLevel => glaze != null ? glaze.Frost : 0f;
        public int GlazeCrystals => glaze != null ? glaze.Standing : 0;

        /// <summary>How far the body is armoured in stone now (Bedrock), 0 to 1, and how many plates stand on it, for probes.</summary>
        public float StoneLevel => armour != null ? armour.Level : 0f;
        public int BedrockPlates => armour != null ? armour.Standing : 0;

        // True while the surge's own look is the element art's, which draws Overdrive's arcs in place of the old sparks.
        private bool ArtDrawsSurge(ElementInfo info) => info != null && Forms.ElementArt.Ready && Forms.SurgeArt.ReplacesBuilt(info);

        private void OnDisable()
        {
            SurgeLook.End(surgePieces);
            surgeShown = false;
            // No steel left on the body when the element goes (Temper's sheen is written only as it changes).
            sheen?.Clear();
            glaze?.Clear();
            armour?.Clear();
            fever?.Clear();
            charge?.Clear();
        }

        private void LateUpdate()
        {
            // The surge is worn: taken up when it is lit, placed round the body every frame, put away when it ends.
            bool surging = SurgeActive;
            if (surging != surgeShown)
            {
                surgeShown = surging;
                if (surging) SurgeLook.Begin(SurgeInfo, surgePieces);
                else SurgeLook.End(surgePieces);
            }
            if (surging)
            {
                Forms.SurgeArt.Wear(transform);
                SurgeLook.Place(SurgeInfo, surgePieces, transform.position, Time.time);
            }
            if (outfit != null && sheen != null) sheen.Watch(outfit.Body);
            sheen?.Tick(surging && SurgeInfo != null && SurgeInfo.Name == Elements.Metal && Forms.ElementArt.Ready, Time.deltaTime, transform.position.y);
            if (outfit != null && glaze != null) glaze.Watch(outfit.Body, transform);
            glaze?.Tick(surging && SurgeInfo != null && SurgeInfo.Name == Elements.Ice && Forms.ElementArt.Ready, Time.deltaTime, transform.position.y);
            if (outfit != null && armour != null) armour.Watch(outfit.Body, transform);
            armour?.Tick(surging && SurgeInfo != null && SurgeInfo.Name == Elements.Earth && Forms.ElementArt.Ready, Time.deltaTime, transform.position.y);
            if (outfit != null && fever != null) fever.Watch(outfit.Body);
            fever?.Tick(surging && SurgeInfo != null && SurgeInfo.Name == Elements.Poison && Forms.ElementArt.Ready, Time.deltaTime, transform.position.y);
            if (outfit != null && charge != null) charge.Watch(outfit.Body);
            charge?.Tick(surging && SurgeInfo != null && SurgeInfo.Name == Elements.Lightning && Forms.ElementArt.Ready, Time.deltaTime, transform.position.y);
            if (glow == null) return;
            float resting = surging ? GlowWhenSurging * Forms.SurgeArt.Glow(SurgeInfo) * (0.85f + 0.15f * Mathf.Sin(Time.time * 17f)) : 0f;
            glowLevel = Mathf.MoveTowards(glowLevel, resting, Time.deltaTime * 30f);
            glow.intensity = glowLevel;
            glow.enabled = glowLevel > 0.01f;
        }

        private void BeginCast()
        {
            var kit = castingKit;
            // The spell goes at whoever is nearest, as an attack would.
            var enemy = combat.NearestEnemy(kit.Shape == SpellShape.Bolt || !string.IsNullOrEmpty(kit.SpellForm) ? kit.SpellReach : 0f);
            if (enemy != null)
            {
                Vector3 toward = enemy.position - transform.position;
                toward.y = 0f;
                if (toward.sqrMagnitude > 0.0025f) transform.rotation = Quaternion.LookRotation(toward, Vector3.up);
            }
        }

        /// <summary>How many times a base spell has been thrown at no cost by a mark (Storm Front).</summary>
        public int FreeCasts { get; private set; }

        /// <summary>
        /// Throws a carried element's base spell now (the first's when that one is not carried), as the marks have made
        /// it, the way the player faces, for no mana and without the cast's wind-up. It is not counted as a spell cast:
        /// nothing that waits on one hears it, and it charges nothing.
        /// </summary>
        public void CastBaseFree(string elementName = null)
        {
            if (!HasElement || combat == null) return;
            var kit = KitOf(elementName) ?? kits[0];
            FreeCasts++;
            int index = kit == kits[0] ? 0 : 1;
            int tag = CastTag;
            CastTag = 0;
            Release(kit, index, true);
            CastTag = tag;
        }

        private void Release()
        {
            var kit = castingKit;
            if (kit == null) return;
            // The blows of the spell carry the number of the slot cast that began it.
            int tag = CastTag;
            CastTag = releaseTag;
            Release(kit, castingIndex, false);
            CastTag = tag;
        }

        private void Release(ElementKit kit, int index, bool free)
        {
            if (!free) SpellsCast++;
            var spec = baseSpells[index];
            var info = infos[index] ?? infos[0];
            var form = (string.IsNullOrEmpty(kit.SpellForm) ? null : SpellForms.Get(kit.SpellForm)) ?? SpellForms.Get(kit.Shape == SpellShape.Bolt ? SpellForms.Bolt : SpellForms.Wave);
            if (upgrades != null)
            {
                // One more leap or blade, a wider cone, where a mark gives the base spell one.
                spec.Count = (string.IsNullOrEmpty(kit.SpellForm) ? kit.SpellLeaps : kit.SpellCount) + Mathf.Max(0, upgrades.Stats.Whole(PlayerStat.BaseSpellCount));
                spec.Arc = kit.SpellArcDegrees * upgrades.Stats.Multiplier(PlayerStat.BaseSpellArc);
            }
            // The base spell's Empower, once bought, gives the kit's cast all its numbers, not only its wait (the ledger's
            // base-spell-marks: Glare strikes for 12 and reaches seven metres).
            var empower = upgrades != null ? upgrades.BaseEmpower(kit.Name) : null;
            if (empower != null)
            {
                SpellChange.Copy(spec, empoweredBase[index]);
                empower.ApplyTo(empoweredBase[index]);
                spec = empoweredBase[index];
            }
            var cast = NewCast(spec, kit.SpellName, null, info);
            if (upgrades != null)
            {
                // What the marks change of the base spell alone, on top of what they change of every spell.
                float further = upgrades.Stats.Multiplier(PlayerStat.BaseSpellReach);
                cast.Damage *= upgrades.Stats.Multiplier(PlayerStat.BaseSpellDamage);
                if (further != 1f)
                {
                    cast.Reach *= further;
                    if (cast.Target == null) cast.Aim = cast.Origin + cast.Forward * cast.Reach;
                }
            }
            // Life's bed of thorns is something grown.
            Clover.World.Hazards.Growing = kit.Name == Elements.Life && !string.IsNullOrEmpty(kit.SpellForm);
            if (upgrades != null && !free) upgrades.EchoLater(form, cast);
            SpellHits += form.Cast(cast);
            Clover.World.Hazards.Growing = false;
            // The pull of a tide or a grasping hand, or the push of a gust, on what stands within its reach.
            if (kit.SpellPull != 0f && string.IsNullOrEmpty(kit.SpellForm)) combat.Displace(transform.position, cast.Reach, Mathf.Abs(kit.SpellPull), kit.SpellPull > 0f);
            glowLevel = GlowWhenCasting;
            if (shaker != null) shaker.Shake(0.14f, 0.2f);
            if (upgrades != null && !free) upgrades.OnSpellCast(kit.Name);
        }

        // The base spells' numbers with their Empowers laid over, made at each cast (the kit's own are never changed).
        private readonly SpellSpec[] empoweredBase = { new SpellSpec(), new SpellSpec() };

        /// <summary>
        /// Lights the worn surge for <paramref name="seconds"/> at no cost when it is <paramref name="elementName"/>'s (Sky
        /// Dance's Master: Tailwind). False when another surge is worn.
        /// </summary>
        public bool LightSurgeFree(string elementName, float seconds)
        {
            var info = SurgeInfo;
            var kit = SurgeKit;
            if (info == null || kit == null || DuoSurgeWorn != null || info.Name != elementName) return false;
            if (kit.LightFree(seconds)) BeginSurge();
            return true;
        }

        private void BeginSurge()
        {
            Surges++;
            var kit = SurgeKit;
            var info = SurgeInfo;
            float speed = kit.SurgeSpeed;
            // The surge coats the weapon in its own element.
            combat.Empower(kit.SurgeDamage, kit.SurgeBurnSeconds, info.Glow, speed, kit.SurgeStagger, info.Affliction, kit.SurgeStacks);
            if (!ArtDrawsSurge(info)) ElementLook.Puff(info, transform.position + Vector3.up, Vector3.up, 1.2f);
            // No ring or puff of white for a surge the art draws: Overdrive's arcs (the owner, 2026-11-19: "I dont like the
            // ring visuals at all"), Temper's steel climbing the body (no white half-dome, 2026-11-20), and the rest.
            if (!ArtDrawsSurge(info)) ElementLook.Ring(info, transform.position + Vector3.up * 0.2f);
            if (info.Name == Elements.Lightning) Clover.World.LightningBolts.Strike(transform.position, 0.4f);
            glowLevel = GlowWhenCasting * Forms.SurgeArt.Glow(info);
            if (defence != null)
            {
                defence.Callout(kit.SurgeName);
                if (kit.SurgeWards > 0) defence.GiveWards(kit.SurgeWards);
                if (kit.SurgeMendsGrey) defence.Heal(defence.Health.Grey, true);
            }
            if (kit.SurgeBurstStacks > 0) combat.AfflictArea(transform.position, kit.SurgeBurstRadius, info.Affliction, kit.SurgeBurstStacks);
            if (upgrades != null) upgrades.OnSurge();
        }
    }
}

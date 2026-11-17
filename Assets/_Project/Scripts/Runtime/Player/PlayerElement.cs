using Clover.Core;
using Clover.Player.Forms;
using Clover.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Clover.Player
{
    /// <summary>
    /// The player's element: casts its spell and its surge, and shows them. The rules and numbers are in
    /// <see cref="ElementKit"/>; the spell itself is thrown by a <see cref="ISpellForm"/>, the same way the spells
    /// the tree teaches are. Like the weapon and the guard it has no Update of its own for the rules:
    /// <see cref="PlayerController"/> calls <see cref="Advance"/> so everything is settled in one order.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerElement : MonoBehaviour
    {
        [SerializeField] private Light glow;
        [SerializeField] private SlashArcs arcs;
        [SerializeField] private TopDownCamera shaker;

        /// <summary>The element the player starts the scene with, unless the run already carries one.</summary>
        public string StartingElement = ElementKit.FireName;
        /// <summary>Seconds a spell or surge press is remembered while the player is busy.</summary>
        public float PressBufferTime = 0.25f;
        public float GlowWhenCasting = 9f;
        public float GlowWhenSurging = 2.6f;
        /// <summary>Seconds between the sparks or embers that fall from the player while the surge lasts.</summary>
        public float EmberInterval = 0.12f;
        /// <summary>With a spell worn: holding the spell button this long casts the worn spell; a shorter press casts the element's own.</summary>
        public float SpecialHoldSeconds = 0.3f;

        public ElementKit Kit { get; private set; }
        /// <summary>What is fixed about the element held: its place on the ring, its colour, its affliction.</summary>
        public ElementInfo Info { get; private set; }
        /// <summary>The element's colour as light; brighter than white so it blooms.</summary>
        public Color Colour { get; private set; }
        public bool IsCasting => Kit.IsCasting;
        public int SpellsCast { get; private set; }
        public int Surges { get; private set; }
        public int SpellHits { get; private set; }

        private PlayerController player;
        private PlayerCombat combat;
        private PlayerDefence defence;
        private PlayerUpgrades upgrades;
        private readonly SpellSpec baseSpell = new SpellSpec();
        private bool spellHolding;
        private float spellHeldFor;
        private float specialBuffered;
        private InputAction spellAction;
        private InputAction surgeAction;
        private float spellBuffered;
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
            Take(ElementKit.ByName(string.IsNullOrEmpty(RunState.Element) ? StartingElement : RunState.Element, RunState.ElementConsumed));
            if (glow != null) glow.intensity = 0f;
        }

        private void Start()
        {
            spellAction = player.FindAction("Spell");
            surgeAction = player.FindAction("Surge");
        }

        /// <summary>Gives the player an element's kit in place of the one they had; any surge running ends.</summary>
        public void Take(ElementKit kit)
        {
            bool surging = Kit != null && Kit.SurgeActive;
            Kit = kit;
            Kit.Reset();
            Info = Elements.ByNameOrFire(kit.Name);
            // The old element's surge is not left on the body, and spells are made of the new one's stuff.
            if (surgeShown)
            {
                SurgeLook.End(surgePieces);
                surgeShown = false;
            }
            ElementSubstance.Hold(Info);
            Colour = Info.Glow;
            // The run remembers every element that has been in the player's hands: a mixed branch opens on it.
            RunState.ElementsHeld.Add(Info.Name);
            if (glow != null) glow.color = Info.Name == Elements.Lightning ? new Color(0.5f, 0.75f, 1f) : Info.Name == Elements.Fire ? new Color(1f, 0.5f, 0.16f) : Info.Colour;
            if (combat != null) combat.ClearEmpower();
            // A weapon that is the element's own (the Armament) is made again in the new element.
            if (combat != null && combat.Weapon != null && combat.Weapon.ElementColoured && !combat.IsAttacking) combat.Equip(Weapons.Armament(Info.Name));
            if (defence != null) defence.ClearWards();
            if (surging && upgrades != null) upgrades.OnSurgeEnd();
            // The base spell's numbers, as its form reads them.
            baseSpell.Cost = kit.SpellCost;
            baseSpell.Damage = kit.SpellDamage;
            baseSpell.Stagger = kit.SpellStagger;
            baseSpell.Reach = kit.SpellReach;
            baseSpell.Arc = kit.SpellArcDegrees;
            baseSpell.Count = kit.SpellLeaps;
            baseSpell.Radius = kit.SpellLeapRange;
            baseSpell.Stacks = kit.SpellStacks;
            baseSpell.AfflictionSeconds = kit.SpellBurnSeconds;
            baseSpell.Seconds = kit.SpellSeconds;
            baseSpell.Interval = kit.SpellInterval;
            baseSpell.LingerDamage = kit.SpellLinger;
            baseSpell.Delay = kit.SpellDelay;
            if (!string.IsNullOrEmpty(kit.SpellForm))
            {
                // A base spell that wears a form reads its numbers as that form does.
                baseSpell.Radius = kit.SpellRadius;
                baseSpell.Count = kit.SpellCount;
            }
            // Nothing asked of the old element is carried into the new one.
            spellBuffered = surgeBuffered = specialBuffered = 0f;
            spellHolding = false;
            glowLevel = 0f;
            untilEmber = 0f;
            surgeHealOwed = 0f;
        }

        /// <summary>Forgets a button being held, as when a menu or a leap takes the buttons away part-way through a hold.</summary>
        /// <summary>True while the spell button is down: for a spell that lasts as long as it is held.</summary>
        public bool SpellHeld => spellAction != null && spellAction.IsPressed();

        public void DropHolds()
        {
            spellHolding = false;
            spellBuffered = surgeBuffered = specialBuffered = 0f;
        }

        /// <summary>A blow or a dodge breaks a cast in progress.</summary>
        public void Interrupt() => Kit.Interrupt();

        /// <summary>Draws an arc of the element's light on the ground in front of the player, as a spell's front.</summary>
        public void ShowArc(float reach, float arcDegrees, float sweep)
        {
            if (arcs != null) arcs.Show(transform.position, transform.forward, reach, arcDegrees, sweep, Colour);
        }

        /// <summary>
        /// A cast of a spell in the element held, aimed where the player faces: what a form is handed. The damage,
        /// stagger and reach already carry what the upgrades add to spells.
        /// </summary>
        public SpellCast NewCast(SpellSpec spec, string name, Upgrade source)
        {
            float damage = upgrades != null ? upgrades.Stats.Multiplier(PlayerStat.SpellDamage) : 1f;
            float reach = spec.Reach * (upgrades != null ? upgrades.Stats.Multiplier(PlayerStat.SpellReach) : 1f);
            Transform target = combat.EnemyAhead(reach);
            Vector3 forward = transform.forward;
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
                Element = Info,
                Colour = Info.Colour,
                Affliction = Info.Affliction,
                Target = target,
                Origin = transform.position,
                Forward = forward,
                Aim = target != null ? target.position : transform.position + forward * reach,
            };
        }

        /// <summary>Runs one frame. <paramref name="free"/> is false while the player is rolling, attacking, flinching or drinking.</summary>
        public void Advance(float deltaTime, bool free)
        {
            if (spellAction == null || surgeAction == null) return;
            if (spellAction.WasPressedThisFrame())
            {
                if (upgrades != null && upgrades.HasSpecial)
                {
                    spellHolding = true;
                    spellHeldFor = 0f;
                }
                else spellBuffered = PressBufferTime;
            }
            if (spellHolding)
            {
                spellHeldFor += deltaTime;
                if (!spellAction.IsPressed())
                {
                    spellHolding = false;
                    spellBuffered = PressBufferTime;
                }
                else if (spellHeldFor >= SpecialHoldSeconds)
                {
                    spellHolding = false;
                    specialBuffered = PressBufferTime;
                }
            }
            if (specialBuffered > 0f && free && !Kit.IsCasting)
            {
                specialBuffered = 0f;
                if (!upgrades.TrySpecial() && defence != null) defence.Callout("No mana");
            }
            if (specialBuffered > 0f) specialBuffered -= deltaTime;
            if (surgeAction.WasPressedThisFrame()) surgeBuffered = PressBufferTime;

            var stats = upgrades != null ? upgrades.Stats : null;
            // The surge is instant and never roots the player, so it can be lit at any free moment.
            if (surgeBuffered > 0f && free)
            {
                surgeBuffered = 0f;
                // A gift that waits on a second press takes it; the surge is not lit over itself.
                if (Kit.SurgeActive && upgrades != null && upgrades.ListensForSurge) upgrades.HeardSurge();
                else if (Kit.TrySurge(combat.Mana, stats != null ? stats.Multiplier(PlayerStat.SurgeCost) : 1f, stats != null ? stats.Multiplier(PlayerStat.SurgeSeconds) : 1f)) BeginSurge();
                else if (defence != null) defence.Callout("No mana");
            }
            if (spellBuffered > 0f && free && !Kit.IsCasting)
            {
                spellBuffered = 0f;
                // With the mark for it, mana that is missing is paid in health.
                if (upgrades != null) upgrades.CoverWithHealth(Kit.SpellCost * stats.Multiplier(PlayerStat.SpellCost) * stats.Multiplier(PlayerStat.BaseSpellCost));
                // Spring Tide: the spell grows with all the mana held, and takes it all.
                bool tide = upgrades != null && upgrades.TideRises(Kit.SpellCost * stats.Multiplier(PlayerStat.SpellCost) * stats.Multiplier(PlayerStat.BaseSpellCost));
                if (Kit.TryCast(combat.Mana, stats != null ? stats.Multiplier(PlayerStat.SpellCost) * stats.Multiplier(PlayerStat.BaseSpellCost) : 1f))
                {
                    if (tide) upgrades.TideSpent();
                    BeginCast();
                }
                else if (defence != null) defence.Callout("No mana");
            }
            if (spellBuffered > 0f) spellBuffered -= deltaTime;
            if (surgeBuffered > 0f) surgeBuffered -= deltaTime;

            if (Kit.TickCast(deltaTime)) Release();
            if (Kit.SurgeActive && Kit.SurgeHealPerSecond != 0f && defence != null && !defence.IsDead)
            {
                // Given or taken a point at a time, so the bar moves in steps that can be seen.
                surgeHealOwed += Kit.SurgeHealPerSecond * deltaTime * (Kit.SurgeHealPerSecond < 0f && stats != null ? stats.Multiplier(PlayerStat.HealthPrices) : 1f);
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
            bool held = Kit.SurgeActive && upgrades != null && upgrades.SurgeHeld;
            if (held)
            {
                // The fire underfoot feeds the surge: its clock stands still, and the flames reach up round the feet to say so.
                surgeHeldShown -= deltaTime;
                if (surgeHeldShown <= 0f)
                {
                    surgeHeldShown = 0.3f;
                    Vector3 feet = transform.position;
                    Forms.FormFx.Lay(Forms.FormMark.Ring, feet, 2.6f, Forms.FormKit.Lit(Colour, 2.6f), 0.3f, 1f);
                    Vector3 outward = Forms.FormRules.OnRing(0, 1, 0.6f, Time.time * 420f);
                    Forms.FormFx.Erupt(Forms.FormMesh.Flame, feet + outward, Quaternion.Euler(0f, Time.time * 200f, 0f), new Vector3(0.6f, 1.5f, 0.6f), Colour, 0.08f, 0.2f, 0.3f);
                }
            }
            if (!held && Kit.TickSurge(deltaTime))
            {
                combat.ClearEmpower();
                if (upgrades != null) upgrades.OnSurgeEnd();
            }

            if (Kit.SurgeActive)
            {
                untilEmber -= deltaTime;
                if (untilEmber <= 0f)
                {
                    untilEmber = EmberInterval;
                    ElementLook.Sparks(Info, transform.position + Vector3.up * 1.1f, Vector3.up, 0.25f);
                }
            }
        }

        private readonly int[] surgePieces = { -1, -1, -1, -1, -1, -1 };
        private bool surgeShown;

        private void OnDisable()
        {
            SurgeLook.End(surgePieces);
            surgeShown = false;
        }

        private void LateUpdate()
        {
            // The surge is worn: taken up when it is lit, placed round the body every frame, put away when it ends.
            bool surging = Kit != null && Kit.SurgeActive;
            if (surging != surgeShown)
            {
                surgeShown = surging;
                if (surging) SurgeLook.Begin(Info, surgePieces);
                else SurgeLook.End(surgePieces);
            }
            if (surging) SurgeLook.Place(Info, surgePieces, transform.position, Time.time);
            if (glow == null) return;
            float resting = Kit.SurgeActive ? GlowWhenSurging * (0.85f + 0.15f * Mathf.Sin(Time.time * 17f)) : 0f;
            glowLevel = Mathf.MoveTowards(glowLevel, resting, Time.deltaTime * 30f);
            glow.intensity = glowLevel;
            glow.enabled = glowLevel > 0.01f;
        }

        private void BeginCast()
        {
            // The spell goes at whoever is nearest, as an attack would.
            var enemy = combat.NearestEnemy(Kit.Shape == SpellShape.Bolt || !string.IsNullOrEmpty(Kit.SpellForm) ? Kit.SpellReach : 0f);
            if (enemy != null)
            {
                Vector3 toward = enemy.position - transform.position;
                toward.y = 0f;
                if (toward.sqrMagnitude > 0.0025f) transform.rotation = Quaternion.LookRotation(toward, Vector3.up);
            }
        }

        /// <summary>How many times the base spell has been thrown at no cost by a mark (Storm Front).</summary>
        public int FreeCasts { get; private set; }

        /// <summary>Throws the element's base spell now, as the marks have made it, the way the player faces, for no mana and without the cast's wind-up. It is not counted as a spell cast: nothing that waits on one hears it.</summary>
        public void CastBaseFree()
        {
            if (Kit == null || combat == null) return;
            FreeCasts++;
            Release(true);
        }

        private void Release(bool free = false)
        {
            if (!free) SpellsCast++;
            var form = (string.IsNullOrEmpty(Kit.SpellForm) ? null : SpellForms.Get(Kit.SpellForm)) ?? SpellForms.Get(Kit.Shape == SpellShape.Bolt ? SpellForms.Bolt : SpellForms.Wave);
            if (upgrades != null)
            {
                // One more leap or blade, a wider cone, where a mark gives the base spell one.
                baseSpell.Count = (string.IsNullOrEmpty(Kit.SpellForm) ? Kit.SpellLeaps : Kit.SpellCount) + Mathf.Max(0, upgrades.Stats.Whole(PlayerStat.BaseSpellCount));
                baseSpell.Arc = Kit.SpellArcDegrees * upgrades.Stats.Multiplier(PlayerStat.BaseSpellArc);
            }
            var cast = NewCast(baseSpell, Kit.SpellName, null);
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
            Clover.World.Hazards.Growing = Kit.Name == Elements.Life && !string.IsNullOrEmpty(Kit.SpellForm);
            SpellHits += form.Cast(cast);
            Clover.World.Hazards.Growing = false;
            // The pull of a tide or a grasping hand, or the push of a gust, on what stands within its reach.
            if (Kit.SpellPull != 0f && string.IsNullOrEmpty(Kit.SpellForm)) combat.Displace(transform.position, cast.Reach, Mathf.Abs(Kit.SpellPull), Kit.SpellPull > 0f);
            glowLevel = GlowWhenCasting;
            if (shaker != null) shaker.Shake(0.14f, 0.2f);
            if (upgrades != null && !free) upgrades.OnSpellCast();
        }

        private void BeginSurge()
        {
            Surges++;
            float speed = Kit.SurgeSpeed;
            combat.Empower(Kit.SurgeDamage, Kit.SurgeBurnSeconds, Colour, speed, Kit.SurgeStagger, Info.Affliction, Kit.SurgeStacks);
            ElementLook.Puff(Info, transform.position + Vector3.up, Vector3.up, 1.2f);
            ElementLook.Ring(Info, transform.position + Vector3.up * 0.2f);
            if (Info.Name == Elements.Lightning) Clover.World.LightningBolts.Strike(transform.position, 0.4f);
            glowLevel = GlowWhenCasting;
            if (defence != null)
            {
                defence.Callout(Kit.SurgeName);
                if (Kit.SurgeWards > 0) defence.GiveWards(Kit.SurgeWards);
                if (Kit.SurgeMendsGrey) defence.Heal(defence.Health.Grey, true);
            }
            if (Kit.SurgeBurstStacks > 0) combat.AfflictArea(transform.position, Kit.SurgeBurstRadius, Info.Affliction, Kit.SurgeBurstStacks);
            if (upgrades != null) upgrades.OnSurge();
        }
    }
}

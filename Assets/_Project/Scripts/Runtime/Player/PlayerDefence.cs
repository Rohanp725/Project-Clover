using Clover.Core;
using Clover.Gameplay;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Clover.Player
{
    /// <summary>
    /// Everything about the player being hit: health, the guard, the flasks, and what a blow does when it arrives (passes
    /// through a dodge, is guarded, or lands and makes the player flinch). The rules are in <see cref="RegainHealth"/>,
    /// <see cref="Guard"/> and <see cref="FlaskBelt"/>. It has no Update of its own: <see cref="PlayerController"/>
    /// calls it each frame so that dodge, guard and weapon are always settled in the same order. Its LateUpdate only
    /// gives <see cref="Tips"/> the end of the frame.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerDefence : MonoBehaviour, IDamageable
    {
        [SerializeField] private HitStop hitStop;
        [SerializeField] private TopDownCamera shaker;
        [SerializeField] private GuardArc guardArc;

        public RegainHealth Health = new RegainHealth();
        public Guard Guard = new Guard();
        public FlaskBelt Flasks = new FlaskBelt();
        /// <summary>Seconds the player cannot act after a blow lands.</summary>
        public float HurtSeconds = 0.35f;
        /// <summary>Seconds the player cannot act after their guard is broken.</summary>
        public float GuardBreakSeconds = 0.8f;
        /// <summary>Metres a landed blow pushes the player back.</summary>
        public float Knockback = 1.1f;
        /// <summary>A blow that passes through a roll this soon after it began is a perfect dodge.</summary>
        public float PerfectDodgeWindow = 0.13f;
        public float PerfectGuardMana = 15f;
        public float PerfectDodgeMana = 12f;
        /// <summary>Grey health won back for each point of damage dealt.</summary>
        public float RegainPerDamage = 0.6f;
        public float PerfectGuardHitStop = 0.12f;
        public float PerfectDodgeSlowSeconds = 0.35f;
        public float PerfectDodgeSlowScale = 0.3f;
        /// <summary>Seconds a guard or flask press is remembered while the player is busy.</summary>
        public float PressBufferTime = 0.2f;
        /// <summary>
        /// Seconds after falling before the player stands up again with everything restored. This is for the practice
        /// arena only: in the game a death ends the run. Zero or less means the player stays down.
        /// </summary>
        public float ReviveAfterSeconds = 2.5f;
        /// <summary>With the Firebomb Flask learned: holding the flask button this long throws it; a shorter press drinks.</summary>
        public bool HoldToThrow;
        public float ThrowHoldSeconds = 0.3f;
        /// <summary>What the damage of a blow that lands is multiplied by; the upgrades and the surge set it.</summary>
        public float DamageTaken = 1f;
        /// <summary>
        /// The share of each wound that others may carry for the player (Shared Roots), and who carries it: given the share,
        /// it returns how much was carried (nothing when no one is near to carry it). Set by the player's upgrades.
        /// </summary>
        [System.NonSerialized] public float WoundShare;
        /// <summary>No blow lands until this time (the Untouchable action).</summary>
        [System.NonSerialized] public float UntouchableUntil;
        [System.NonSerialized] public System.Func<float, float> ShareWound;
        /// <summary>The most wards that can be held at once.</summary>
        public int MaximumWards = 3;
        /// <summary>Wards held: each turns aside one blow that would otherwise have landed.</summary>
        public int Wards { get; private set; }
        public int BlowsWarded { get; private set; }

        public bool IsHurt => hurtLeft > 0f;
        public bool IsDead => Health.IsDead;
        public bool IsGuarding => Guard.IsRaised;
        public bool IsDrinking => Flasks.IsDrinking;
        /// <summary>How many drinks have been started: the thirsting watch it, and come for the flask when it moves.</summary>
        public int DrinksBegun { get; private set; }
        /// <summary>The enemy the raised guard is turned toward; null when none is near or the guard is down.</summary>
        public Transform GuardTarget { get; private set; }
        /// <summary>Seconds between looks for the nearest enemy while the guard is up.</summary>
        public float GuardRetargetSeconds = 0.25f;
        /// <summary>True when the player can do nothing: flinching or fallen.</summary>
        public bool IsHelpless => IsHurt || IsDead;
        /// <summary>Seconds since the player fell.</summary>
        public float DeadSeconds => deadFor;
        /// <summary>How far through the flinch the player is, from 0 to 1; 1 when fallen.</summary>
        public float HurtProgress => IsDead ? 1f : hurtSeconds > 0f ? Mathf.Clamp01(1f - hurtLeft / hurtSeconds) : 0f;
        /// <summary>A guard pressed during an attack's recovery: the controller cuts the recovery short for it.</summary>
        public bool WantsGuardCancel => guardHeld && guardPressBuffered > 0f;
        /// <summary>How far the flinch moves the player this frame.</summary>
        public Vector3 Step { get; private set; }

        /// <summary>The latest thing worth telling the player in a word or two, and how many there have been.</summary>
        public string LastCallout { get; private set; } = "";
        public int Callouts { get; private set; }
        // Counters so tests and probes can see what happened without watching the screen.
        public int BlowsTaken { get; private set; }
        public int Blocks { get; private set; }
        /// <summary>Red (unguardable) blows turned by a perfect guard, counted for the Way's answers (Catch the Red).</summary>
        public int RedTurns { get; private set; }
        public int PerfectGuards { get; private set; }
        public int Dodges { get; private set; }
        public int PerfectDodges { get; private set; }
        public int Deaths { get; private set; }

        /// <summary>A perfect guard (a spell's own parry counts) and a perfect dodge: the spells' clocks are cut by each.</summary>
        public event System.Action PerfectGuarded;
        public event System.Action PerfectDodged;

        private PlayerController player;
        private PlayerCombat combat;
        private PlayerElement element;
        private PlayerUpgrades upgrades;
        private bool flaskHolding;
        private float flaskHeldFor;
        private float throwBuffered;
        private InputAction guardAction;
        private InputAction flaskAction;
        private bool guardHeld;
        private float guardPressBuffered;
        private float flaskBuffered;
        private float hurtLeft;
        private float hurtSeconds;
        private Vector3 knockDirection;
        private float deadFor;
        private float guardRetargetIn;

        public void Configure(HitStop hitStopToUse, TopDownCamera cameraToShake, GuardArc arcToShow)
        {
            hitStop = hitStopToUse;
            shaker = cameraToShake;
            guardArc = arcToShow;
        }

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            combat = GetComponent<PlayerCombat>();
            element = GetComponent<PlayerElement>();
            upgrades = GetComponent<PlayerUpgrades>();
            Health.Fill();
            Guard.Reset();
            Flasks.Refill();
            Tips.BeginScene(this, gameObject.scene.name);
        }

        private void Start()
        {
            guardAction = player.FindAction("Guard");
            flaskAction = player.FindAction("Flask");
        }

        // Tips are said last in the frame, when it is known whether the fight has already used the status line.
        private void LateUpdate()
        {
            Tips.EndFrame(this, Time.deltaTime);
            // What the elites and gods laid bites on its own clock.
            float bite = Afflictions.Tick(Time.deltaTime);
            if (bite > 0f && !IsDead) Health.Damage(bite);
        }

        /// <summary>The afflictions laid on the player (question 1 of the stand-in plan: elites and gods lay their land's).</summary>
        public readonly PlayerAfflictions Afflictions = new PlayerAfflictions();

        /// <summary>
        /// A blow of an enemy of <paramref name="rank"/> landed on the player in the land a scene belongs to: an elite or a
        /// god lays that land's affliction, one stack. True when it took.
        /// </summary>
        public bool StruckInLand(Clover.Gameplay.EnemyRank rank, string sceneName)
        {
            if (!PlayerAfflictions.Lays(rank)) return false;
            var kind = PlayerAfflictions.OfLand(Outfits.LandOf(sceneName));
            return kind != Clover.Gameplay.AfflictionKind.None && Afflictions.Lay(kind, 1);
        }

        /// <summary>Forgets a button being held, as when a menu or a leap takes the buttons away part-way through a hold.</summary>
        public void DropHolds()
        {
            flaskHolding = false;
            flaskBuffered = 0f;
            throwBuffered = 0f;
            guardPressBuffered = 0f;
        }

        /// <summary>Puts a word or two on the screen for a moment.</summary>
        public void Callout(string text)
        {
            LastCallout = text;
            Callouts++;
        }

        /// <summary>
        /// Heat on the skin (the noon's swell, molten gold underfoot): health taken a little at a time without a flinch.
        /// What would kill is dealt as a blow, so a fall from it is a fall like any other. Returns the health taken.
        /// </summary>
        public float Scorch(float amount)
        {
            if (amount <= 0f || IsDead) return 0f;
            if (Health.Current - amount <= 0.5f)
            {
                TakeHit(new Hit(amount, 0f, -transform.forward, false, true));
                return amount;
            }
            Health.Damage(amount);
            Scorched += amount;
            return amount;
        }

        /// <summary>Health taken by heat, in all.</summary>
        public float Scorched { get; private set; }

        /// <summary>Gives health, as an upgrade or an affliction on an enemy does. Shown as healing unless asked not to be.</summary>
        public void Heal(float amount, bool show = true)
        {
            if (amount <= 0f || IsDead) return;
            Health.Heal(amount);
            if (show) Vfx.Play(VfxKind.Heal, transform.position + Vector3.up * 0.6f, Vector3.up, 0.5f);
        }

        /// <summary>Gives wards, up to the most that can be held.</summary>
        /// <summary>True when the attacker is in a condition that turns a plain guard against it into a perfect one (ConditionFlags.GuardMadePerfect).</summary>
        public static bool GuardMadePerfect(Component attacker) =>
            attacker is Clover.Gameplay.IAfflictable carrier && carrier.Afflictions.Conditions.Has(Clover.Gameplay.ConditionFlags.GuardMadePerfect);

        /// <summary>
        /// A spell started as a blow would land counts as a perfect dodge of it (the shared movement rule): what a perfect
        /// dodge gives, the marks that answer one told, and no blow lands for the moment it had left.
        /// </summary>
        public void CountAsPerfectDodge(Vector3 attacker, float strikeIn)
        {
            PerfectDodges++;
            LastPerfectDodgeAt = Time.time;
            if (combat != null) combat.Mana.Gain(PerfectDodgeMana);
            if (hitStop != null) hitStop.Slow(PerfectDodgeSlowSeconds, PerfectDodgeSlowScale);
            Callout("Perfect dodge");
            UntouchableUntil = Mathf.Max(UntouchableUntil, Time.time + Mathf.Clamp(strikeIn, 0f, 1f) + 0.15f);
            if (upgrades != null) upgrades.OnPerfectDodge(attacker);
            PerfectDodged?.Invoke();
        }

        /// <summary>When the last perfect dodge was made (Time.time); far in the past when none was.</summary>
        public float LastPerfectDodgeAt { get; private set; } = -100f;

        public void GiveWards(int count)
        {
            if (count > 0) Wards = Mathf.Min(MaximumWards, Wards + count);
        }

        /// <summary>The share of a blow a plate of stone takes when its mark names none.</summary>
        public const float PlainPlateShare = 0.6f;
        /// <summary>Plates of stone held at most (Stone Skin grows two).</summary>
        public int MaximumPlates = 2;
        /// <summary>
        /// Plates of stone held (Stone Skin): each takes its share of one blow that lands and stops its flinch, then breaks.
        /// Spent after the wards, which turn the whole blow.
        /// </summary>
        public int Plates { get; private set; }
        /// <summary>The share of a blow each plate held takes.</summary>
        public float PlateShare { get; private set; } = PlainPlateShare;
        /// <summary>Blows a plate took its share of: for probes.</summary>
        public int BlowsPlated { get; private set; }

        public void GivePlates(int count, float share)
        {
            if (count <= 0) return;
            Plates = Mathf.Min(MaximumPlates, Plates + count);
            PlateShare = Mathf.Clamp01(share);
        }

        /// <summary>What of a wound gets past a plate that takes <paramref name="share"/> of it.</summary>
        public static float PlatedWound(float wound, float share) => Mathf.Max(0f, wound) * (1f - Mathf.Clamp01(share));

        // A plate breaks on a blow that was about to land: the blow lands at what the plate let through, and does not stagger.
        private bool SpendPlate(in Hit hit)
        {
            if (Plates <= 0) return false;
            Plates--;
            BlowsPlated++;
            Vfx.Play(VfxKind.Burst, GuardPoint());
            Callout("Plate broken");
            Land(hit, 0f, false, PlateShare);
            if (upgrades != null) upgrades.OnPlateBroken();
            return true;
        }

        /// <summary>Takes every ward away, as when the element that gave them is put down.</summary>
        public void ClearWards() => Wards = 0;

        // A ward is spent on a blow that was about to land: a pale ring, and the blow is gone.
        private bool SpendWard(in Hit hit)
        {
            if (Wards <= 0) return false;
            Wards--;
            BlowsWarded++;
            if (upgrades != null)
            {
                upgrades.OnWardSpent();
                // A blow turned by a ward is an enemy's blow turned, as a guard turns one.
                upgrades.OnWarded(hit.HasOrigin ? hit.Origin : transform.position - hit.Direction);
            }
            Vfx.Play(VfxKind.TintRing, GuardPoint(), transform.forward, 1f, new Color(0.7f, 0.92f, 1f));
            Vfx.Play(VfxKind.TintSpark, GuardPoint(), transform.forward, 2f, new Color(0.7f, 0.92f, 1f));
            Vfx.Play(VfxKind.Clang, GuardPoint());
            Callout("Warded");
            if (hitStop != null) hitStop.Freeze(0.05f);
            return true;
        }

        /// <summary>Tells the defence how much damage the player's blow dealt: it wins back grey health and earns flasks.</summary>
        public void DealtDamage(float damage)
        {
            Health.Regain(damage * RegainPerDamage);
            if (Flasks.EarnFromDamage(damage)) Callout("Flask earned");
        }

        /// <summary>Reads the buttons. Called first each frame, before anything is decided.</summary>
        public void ReadInput()
        {
            if (guardAction == null || flaskAction == null) return;
            guardHeld = guardAction.IsPressed();
            if (guardAction.WasPressedThisFrame())
            {
                Guard.Edge = upgrades != null && upgrades.Rules.Guard == Growth.GuardMode.Edge;
                Guard.Press(!IsHelpless && !IsDrinking);
                guardPressBuffered = PressBufferTime;
            }
            // Under the L2 layer the flask's button is a spell, and nothing is drunk.
            if (player.Buttons != null ? player.Buttons.Pressed(PlayerButton.Flask) : flaskAction.WasPressedThisFrame())
            {
                if (HoldToThrow)
                {
                    flaskHolding = true;
                    flaskHeldFor = 0f;
                }
                else flaskBuffered = PressBufferTime;
            }
            if (!flaskHolding) return;
            // Let go early it is a drink; held, it is thrown.
            flaskHeldFor += Time.deltaTime;
            if (!flaskAction.IsPressed())
            {
                flaskHolding = false;
                flaskBuffered = PressBufferTime;
            }
            else if (flaskHeldFor >= ThrowHoldSeconds)
            {
                flaskHolding = false;
                throwBuffered = PressBufferTime;
            }
        }

        /// <summary>Runs one frame. The guard and the flask wait while the player is rolling or attacking.</summary>
        public void Advance(float deltaTime, bool rolling, bool attacking)
        {
            Step = Vector3.zero;
            Guard.Tick(deltaTime);
            Health.Tick(deltaTime);
            if (guardPressBuffered > 0f) guardPressBuffered -= deltaTime;
            if (flaskBuffered > 0f) flaskBuffered -= deltaTime;
            if (throwBuffered > 0f) throwBuffered -= deltaTime;

            if (IsDead)
            {
                Guard.SetRaised(false);
                if (guardArc != null) guardArc.Show(false);
                GuardTarget = null;
                deadFor += deltaTime;
                if (ReviveAfterSeconds > 0f && deadFor >= ReviveAfterSeconds) Revive();
                return;
            }
            if (hurtLeft > 0f)
            {
                Guard.SetRaised(false);
                if (guardArc != null) guardArc.Show(false);
                GuardTarget = null;
                // The push is spent early in the flinch and has died away by its end.
                float before = Eased(1f - hurtLeft / hurtSeconds);
                hurtLeft = Mathf.Max(0f, hurtLeft - deltaTime);
                float after = Eased(1f - hurtLeft / hurtSeconds);
                Step = knockDirection * (Knockback * (after - before));
                return;
            }

            // A roll abandons a drink; nothing is lost if the flask had not yet been used.
            if (rolling && Flasks.IsDrinking) Flasks.Interrupt();
            bool free = !rolling && !attacking;
            // At full health the press is ignored, so a flask is never wasted.
            if (flaskBuffered > 0f && free && Health.Current < Health.Maximum && Flasks.TryStartDrink())
            {
                flaskBuffered = 0f;
                DrinksBegun++;
            }
            if (throwBuffered > 0f && free && !Flasks.IsDrinking)
            {
                throwBuffered = 0f;
                if (Flasks.TryThrow())
                {
                    if (upgrades != null) upgrades.OnFlaskThrown();
                }
                else Callout("No flask");
            }
            if (Flasks.Tick(deltaTime))
            {
                Health.Heal(Flasks.HealAmount);
                Vfx.Play(VfxKind.Heal, transform.position + Vector3.up * 0.6f);
                if (upgrades != null) upgrades.OnFlaskDrunk();
            }

            bool raise = guardHeld && free && !Flasks.IsDrinking;
            if (raise && !Guard.IsRaised)
            {
                guardPressBuffered = 0f;
                guardRetargetIn = 0f;
            }
            if (raise && combat != null)
            {
                // The guard only covers the front, so it keeps turned to whoever is nearest. Who that is, is looked up
                // a few times a second rather than every frame.
                guardRetargetIn -= deltaTime;
                if (guardRetargetIn <= 0f)
                {
                    guardRetargetIn = GuardRetargetSeconds;
                    bool justRaised = !Guard.IsRaised;
                    GuardTarget = combat.NearestEnemy();
                    // Raising the guard squares up at once, so a guard pressed at the last moment faces the blow.
                    if (justRaised && GuardTarget != null)
                    {
                        Vector3 toward = GuardTarget.position - transform.position;
                        toward.y = 0f;
                        if (toward.sqrMagnitude > 0.0025f) transform.rotation = Quaternion.LookRotation(toward, Vector3.up);
                    }
                }
            }
            else
            {
                GuardTarget = null;
            }
            Guard.SetRaised(raise);
            if (guardArc != null) guardArc.Show(raise);
        }

        /// <summary>While true a blow that lands hurts but does not make the player flinch. The upgrades keep it.</summary>
        public bool Unflinching { get; set; }
        /// <summary>Blows taken without flinching, counted for probes.</summary>
        public int UnflinchedBlows { get; private set; }
        /// <summary>The Body's Steady (task 5.4): outside a swing, a blow of this much damage or less hurts but does not make the player flinch. The upgrades keep it.</summary>
        public float Steadiness { get; set; }
        /// <summary>Blows shrugged off that way, counted for probes.</summary>
        public int SteadiedBlows { get; private set; }

        public HitOutcome TakeHit(in Hit hit)
        {
            // A blow that arrives during the flinch from the last one is ignored, so two enemies cannot keep the
            // player flinching for ever.
            if (IsDead || IsHurt) return HitOutcome.Missed;
            // The guard is down the moment an attack begins, even if both buttons went down on the same frame.
            // (The guard's own thrust is the one attack made from behind it.)
            if (combat != null && combat.IsAttacking && !combat.IsGuardAttacking) Guard.SetRaised(false);

            // Nothing interrupts the finisher.
            if (combat != null && combat.IsFinishing) return HitOutcome.Dodged;
            // Nor reaches the player in the air.
            if (player.IsLeaping) return HitOutcome.Dodged;
            // Nor while a spell has made them something that cannot be struck.
            if (player.FormHold != 0 && player.FormUntouchable) return HitOutcome.Dodged;
            // A spell's moment of being untouchable (Three Weasels' pass through its enemy).
            if (Time.time < UntouchableUntil) return HitOutcome.Dodged;
            if (player.FormParrying)
            {
                // The spell's own guard: the blow is turned for nothing and the form answers it.
                player.FormParrying = false;
                player.FormParried = true;
                // A red attack turned: the counters' Masters answer only that (the ledger's red-attack).
                if (hit.Unblockable && upgrades != null) upgrades.RedTurned();
                player.FormParriedFrom = hit.HasOrigin ? hit.Origin : transform.position - hit.Direction;
                // What it would have dealt, for a counter that mends by it (Death Turned Back).
                LastTurnedWound = Mathf.Max(0f, hit.Damage * DamageTaken);
                // Whose blow it was, for an answer meant only for the attacker (The Golden Chair).
                if (upgrades != null && combat != null) upgrades.NoteTurned(combat.EnemyAt(player.FormParriedFrom, 2.5f));
                PerfectGuards++;
                Vfx.Play(VfxKind.PerfectGuard, GuardPoint(), transform.forward);
                Vfx.Play(VfxKind.Impact, GuardPoint());
                if (hitStop != null) hitStop.Freeze(PerfectGuardHitStop);
                PerfectGuarded?.Invoke();
                return HitOutcome.PerfectGuard;
            }
            // A wall of the element between the blow and the player takes it.
            if (hit.HasOrigin && Clover.Player.Forms.SpellWorks.Screens(hit.Origin, transform.position)) return HitOutcome.Dodged;

            if (player.Dodge.IsInvulnerable)
            {
                // A dodge away from the enemy under Into the Fire is never perfect.
                if (player.Dodge.Elapsed <= PerfectDodgeWindow && !player.Dodge.NoPerfect)
                {
                    PerfectDodges++;
                    LastPerfectDodgeAt = Time.time;
                    if (combat != null) combat.Mana.Gain(PerfectDodgeMana);
                    if (hitStop != null) hitStop.Slow(PerfectDodgeSlowSeconds, PerfectDodgeSlowScale);
                    Callout("Perfect dodge");
                    if (upgrades != null) upgrades.OnPerfectDodge(hit.HasOrigin ? hit.Origin : transform.position - hit.Direction);
                    PerfectDodged?.Invoke();
                    return HitOutcome.PerfectDodge;
                }
                Dodges++;
                return HitOutcome.Dodged;
            }

            // A counter's stance, held and paid for, takes the blow from any side and throws it back.
            if (combat != null && combat.TryCounter(hit))
            {
                Vfx.Play(VfxKind.PerfectGuard, transform.position + Vector3.up * 1.2f, transform.forward);
                Vfx.Play(VfxKind.TintRing, transform.position + Vector3.up * 1.1f, transform.forward, 1.2f, Color.white);
                ScreenFlash.Flash(new Color(1f, 1f, 1f, 0.35f), 0.06f);
                if (hitStop != null) hitStop.Freeze(PerfectGuardHitStop);
                if (shaker != null) shaker.Shake(0.14f, 0.15f);
                Callout("Reflect");
                return HitOutcome.PerfectGuard;
            }

            // The guard covers the front only: a blow from behind lands whatever is held.
            // Where the attacker stands decides it; the blow's own direction is only a fallback, since a wide swing
            // need not travel straight from the attacker to the player.
            Vector3 fromAttacker = hit.HasOrigin ? hit.Origin - transform.position : -hit.Direction;
            fromAttacker.y = 0f;
            bool covered = fromAttacker.sqrMagnitude < 0.0001f || Guard.Covers(Vector3.Angle(transform.forward, fromAttacker));
            float chip = 0f;
            // The RuleBook may hold the guard whatever the stamina (The Guard Holds).
            bool unbreakable = upgrades != null && upgrades.Rules.Guard == Growth.GuardMode.Unbreakable;
            // A plain guard against an attacker whose condition makes it perfect (Snapped Blade).
            bool madePerfect = covered && combat != null && GuardMadePerfect(combat.EnemyAt(hit.HasOrigin ? hit.Origin : transform.position - hit.Direction, 2.5f));
            bool edge = upgrades != null && upgrades.Rules.Guard == Growth.GuardMode.Edge;
            switch (covered ? Guard.Resolve(hit.Damage, hit.Unblockable, player.Stamina, out chip, unbreakable, madePerfect, edge) : GuardResult.NotGuarded)
            {
                case GuardResult.Perfect:
                    PerfectGuards++;
                    if (hit.Unblockable)
                    {
                        RedTurns++;
                        if (upgrades != null) upgrades.RedTurned();
                    }
                    Vfx.Play(VfxKind.PerfectGuard, GuardPoint(), transform.forward);
                    if (guardArc != null) guardArc.Pulse(new Color(4.5f, 3.2f, 0.7f, 1f), 0.35f, 0.5f);
                    Vfx.Play(VfxKind.Ring, GuardPoint(), transform.forward);
                    Vfx.Play(VfxKind.Impact, GuardPoint());
                    ScreenFlash.Flash(new Color(0.75f, 0.9f, 1f, 0.4f), 0.07f);
                    if (combat != null) combat.Mana.Gain(PerfectGuardMana);
                    if (hitStop != null) hitStop.Freeze(PerfectGuardHitStop);
                    if (shaker != null) shaker.Shake(0.12f, 0.15f);
                    Callout("Perfect guard");
                    if (combat != null) combat.PerfectGuarded(hit.HasOrigin ? hit.Origin : transform.position - hit.Direction);
                    if (upgrades != null) upgrades.OnPerfectGuard(hit.HasOrigin ? hit.Origin : transform.position - hit.Direction);
                    PerfectGuarded?.Invoke();
                    return HitOutcome.PerfectGuard;
                case GuardResult.Blocked:
                    Blocks++;
                    Vfx.Play(VfxKind.GuardSpark, GuardPoint(), transform.forward);
                    Vfx.Play(VfxKind.Clang, GuardPoint());
                    if (guardArc != null) guardArc.Pulse(new Color(3f, 3.4f, 4f, 1f), 0.18f, 0.22f);
                    if (shaker != null) shaker.Shake(0.07f, 0.12f);
                    Health.Chip(chip);
                    if (hitStop != null) hitStop.Freeze(0.04f);
                    if (upgrades != null) upgrades.OnGuard(hit.HasOrigin ? hit.Origin : transform.position - hit.Direction);
                    return HitOutcome.Blocked;
                case GuardResult.Broken:
                    if (SpendWard(hit)) return HitOutcome.Blocked;
                    if (SpendPlate(hit)) return HitOutcome.Landed;
                    Callout("Guard broken");
                    if (guardArc != null) guardArc.Pulse(new Color(5f, 0.4f, 0.3f, 1f), 0.4f, 0.9f);
                    Vfx.Play(VfxKind.Danger, GuardPoint());
                    Land(hit, GuardBreakSeconds, true);
                    return HitOutcome.Landed;
                default:
                    if (SpendWard(hit)) return HitOutcome.Blocked;
                    if (SpendPlate(hit)) return HitOutcome.Landed;
                    Land(hit, HurtSeconds);
                    // The blow that kills teaches nothing.
                    if (!IsDead) Tips.UnguardedBlow();
                    return HitOutcome.Landed;
            }
        }

        /// <summary>What the last blow a counter's stance turned would have dealt you (Death Turned Back mends by it).</summary>
        public float LastTurnedWound { get; private set; }

        /// <summary>The mending a counter gives for a blow it turned: what the blow would have dealt, <paramref name="most"/> at most.</summary>
        public static float TurnedMend(float wound, float most) => Mathf.Clamp(wound, 0f, Mathf.Max(0f, most));

        /// <summary>What the last blow that landed took, and when (Second Shadow's Master gives it back).</summary>
        public float LastWound { get; private set; }
        public float LastWoundAt { get; private set; } = -100f;

        private void Land(in Hit hit, float seconds, bool guardBroken = false, float plated = 0f)
        {
            BlowsTaken++;
            float wound = hit.Damage * DamageTaken;
            // A plate of stone took its share first (Stone Skin).
            if (plated > 0f) wound = PlatedWound(wound, plated);
            // Shared Roots: a share of the wound is carried by the Seeded enemies near, as much as they can take of it.
            if (WoundShare > 0f && ShareWound != null && wound > 0f) wound -= Mathf.Clamp(ShareWound(wound * WoundShare), 0f, wound);
            float before = Health.Current;
            Health.Damage(wound);
            LastWound = before - Health.Current;
            LastWoundAt = Time.time;
            // With Dark's gift the blow that kills is not the end, once.
            if (IsDead && upgrades != null && upgrades.Rise(hit.HasOrigin ? hit.Origin : transform.position - hit.Direction)) Callout("Risen");
            if (upgrades != null) upgrades.OnStruck(hit.HasOrigin ? hit.Origin : transform.position - hit.Direction);
            Vfx.Play(VfxKind.Wound, transform.position + Vector3.up * 1.1f, hit.Direction);
            ScreenFlash.Flash(new Color(1f, 0.08f, 0.1f, 0.16f), 0.09f);
            if (!IsDead && plated > 0f)
            {
                // The plate stopped its flinch: it hurts, and you stand through it.
                if (shaker != null) shaker.Shake(0.1f, 0.12f);
                return;
            }
            if (!IsDead && combat != null && (!hit.Unblockable || (combat.HeavyUnbroken && combat.InHeavySwing)) && combat.Poised(hit.Damage))
            {
                // Inside a heavy swing a small blow is taken and walked through: it hurts, the swing goes on.
                PoisedBlows++;
                if (hitStop != null) hitStop.Freeze(0.04f);
                if (shaker != null) shaker.Shake(0.1f, 0.12f);
                return;
            }
            if (!IsDead && Unflinching)
            {
                // Past caring: the blow is taken whole and nothing is broken off for it.
                UnflinchedBlows++;
                if (shaker != null) shaker.Shake(0.1f, 0.12f);
                return;
            }
            if (!IsDead && !guardBroken && Way.BodyRules.ShrugsOff(combat != null && combat.InSwingMoment, Steadiness, hit.Damage))
            {
                // Steady: a small blow outside a swing hurts, and the player stands through it.
                SteadiedBlows++;
                if (shaker != null) shaker.Shake(0.1f, 0.12f);
                return;
            }
            hurtSeconds = seconds;
            hurtLeft = seconds;
            knockDirection = hit.Direction;
            knockDirection.y = 0f;
            Guard.SetRaised(false);
            Flasks.Interrupt();
            player.Dodge.Stop();
            player.BreakFormHold();
            if (combat != null) combat.Interrupt();
            if (element != null) element.Interrupt();
            if (hitStop != null) hitStop.Freeze(0.07f);
            if (shaker != null) shaker.Shake(0.22f, 0.22f);
            if (IsDead)
            {
                Deaths++;
                deadFor = 0f;
                hurtLeft = 0f;
                Callout("Fallen");
            }
        }

        /// <summary>Blows taken inside a swing that had the poise to go on through them.</summary>
        public int PoisedBlows { get; private set; }
        /// <summary>The last guard reaction, for the animator: counts that only go up.</summary>
        public int GuardReactions => Blocks + PerfectGuards;

        /// <summary>Shows the guard as wide as the weapon in hand makes it.</summary>
        public void GuardWidth(float degrees)
        {
            if (guardArc != null) guardArc.SetArc(degrees);
        }

        // Where a blow meets the guard: in front of the chest.
        private Vector3 GuardPoint() => transform.position + Vector3.up * 1.2f + transform.forward * 0.6f;

        /// <summary>Raised when the player stands up again after falling, so the scene can put things back.</summary>
        public event System.Action Revived;

        /// <summary>Full health and flasks, as a rest gives.</summary>
        public void Restore()
        {
            Health.Fill();
            Flasks.Refill();
            player.Stamina.Fill();
            Wards = 0;
            Plates = 0;
        }

        private void Revive()
        {
            Restore();
            deadFor = 0f;
            Revived?.Invoke();
        }

        private static float Eased(float t)
        {
            t = Mathf.Clamp01(t);
            return 1f - (1f - t) * (1f - t);
        }
    }
}

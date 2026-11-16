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
    /// calls it each frame so that dodge, guard and weapon are always settled in the same order.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerDefence : MonoBehaviour, IDamageable
    {
        [SerializeField] private HitStop hitStop;
        [SerializeField] private TopDownCamera shaker;

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

        public bool IsHurt => hurtLeft > 0f;
        public bool IsDead => Health.IsDead;
        public bool IsGuarding => Guard.IsRaised;
        public bool IsDrinking => Flasks.IsDrinking;
        /// <summary>The enemy the raised guard is turned toward; null when none is near or the guard is down.</summary>
        public Transform GuardTarget { get; private set; }
        /// <summary>Seconds between looks for the nearest enemy while the guard is up.</summary>
        public float GuardRetargetSeconds = 0.25f;
        /// <summary>True when the player can do nothing: flinching or fallen.</summary>
        public bool IsHelpless => IsHurt || IsDead;
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
        public int PerfectGuards { get; private set; }
        public int Dodges { get; private set; }
        public int PerfectDodges { get; private set; }
        public int Deaths { get; private set; }

        private PlayerController player;
        private PlayerCombat combat;
        private PlayerElement element;
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

        public void Configure(HitStop hitStopToUse, TopDownCamera cameraToShake)
        {
            hitStop = hitStopToUse;
            shaker = cameraToShake;
        }

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            combat = GetComponent<PlayerCombat>();
            element = GetComponent<PlayerElement>();
            Health.Fill();
            Guard.Reset();
            Flasks.Refill();
        }

        private void Start()
        {
            guardAction = player.FindAction("Guard");
            flaskAction = player.FindAction("Flask");
        }

        /// <summary>Puts a word or two on the screen for a moment.</summary>
        public void Callout(string text)
        {
            LastCallout = text;
            Callouts++;
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
                Guard.Press(!IsHelpless && !IsDrinking);
                guardPressBuffered = PressBufferTime;
            }
            if (flaskAction.WasPressedThisFrame()) flaskBuffered = PressBufferTime;
        }

        /// <summary>Runs one frame. The guard and the flask wait while the player is rolling or attacking.</summary>
        public void Advance(float deltaTime, bool rolling, bool attacking)
        {
            Step = Vector3.zero;
            Guard.Tick(deltaTime);
            Health.Tick(deltaTime);
            if (guardPressBuffered > 0f) guardPressBuffered -= deltaTime;
            if (flaskBuffered > 0f) flaskBuffered -= deltaTime;

            if (IsDead)
            {
                Guard.SetRaised(false);
                GuardTarget = null;
                deadFor += deltaTime;
                if (ReviveAfterSeconds > 0f && deadFor >= ReviveAfterSeconds) Revive();
                return;
            }
            if (hurtLeft > 0f)
            {
                Guard.SetRaised(false);
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
            if (flaskBuffered > 0f && free && Health.Current < Health.Maximum && Flasks.TryStartDrink()) flaskBuffered = 0f;
            if (Flasks.Tick(deltaTime))
            {
                Health.Heal(Flasks.HealAmount);
                Vfx.Play(VfxKind.Heal, transform.position + Vector3.up * 0.6f);
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
        }

        public HitOutcome TakeHit(in Hit hit)
        {
            // A blow that arrives during the flinch from the last one is ignored, so two enemies cannot keep the
            // player flinching for ever.
            if (IsDead || IsHurt) return HitOutcome.Missed;
            // The guard is down the moment an attack begins, even if both buttons went down on the same frame.
            if (combat != null && combat.IsAttacking) Guard.SetRaised(false);

            // Nothing interrupts the finisher.
            if (combat != null && combat.IsFinishing) return HitOutcome.Dodged;

            if (player.Dodge.IsInvulnerable)
            {
                if (player.Dodge.Elapsed <= PerfectDodgeWindow)
                {
                    PerfectDodges++;
                    if (combat != null) combat.Mana.Gain(PerfectDodgeMana);
                    if (hitStop != null) hitStop.Slow(PerfectDodgeSlowSeconds, PerfectDodgeSlowScale);
                    Callout("Perfect dodge");
                    return HitOutcome.PerfectDodge;
                }
                Dodges++;
                return HitOutcome.Dodged;
            }

            // The guard covers the front only: a blow from behind lands whatever is held.
            // Where the attacker stands decides it; the blow's own direction is only a fallback, since a wide swing
            // need not travel straight from the attacker to the player.
            Vector3 fromAttacker = hit.HasOrigin ? hit.Origin - transform.position : -hit.Direction;
            fromAttacker.y = 0f;
            bool covered = fromAttacker.sqrMagnitude < 0.0001f || Guard.Covers(Vector3.Angle(transform.forward, fromAttacker));
            float chip = 0f;
            switch (covered ? Guard.Resolve(hit.Damage, hit.Unblockable, player.Stamina, out chip) : GuardResult.NotGuarded)
            {
                case GuardResult.Perfect:
                    PerfectGuards++;
                    Vfx.Play(VfxKind.PerfectGuard, GuardPoint(), transform.forward);
                    Vfx.Play(VfxKind.Ring, GuardPoint(), transform.forward);
                    Vfx.Play(VfxKind.Impact, GuardPoint());
                    ScreenFlash.Flash(new Color(0.75f, 0.9f, 1f, 0.4f), 0.07f);
                    if (combat != null) combat.Mana.Gain(PerfectGuardMana);
                    if (hitStop != null) hitStop.Freeze(PerfectGuardHitStop);
                    if (shaker != null) shaker.Shake(0.12f, 0.15f);
                    Callout("Perfect guard");
                    return HitOutcome.PerfectGuard;
                case GuardResult.Blocked:
                    Blocks++;
                    Vfx.Play(VfxKind.GuardSpark, GuardPoint(), transform.forward);
                    Health.Chip(chip);
                    if (hitStop != null) hitStop.Freeze(0.04f);
                    return HitOutcome.Blocked;
                case GuardResult.Broken:
                    Callout("Guard broken");
                    Land(hit, GuardBreakSeconds);
                    return HitOutcome.Landed;
                default:
                    Land(hit, HurtSeconds);
                    return HitOutcome.Landed;
            }
        }

        private void Land(in Hit hit, float seconds)
        {
            BlowsTaken++;
            Health.Damage(hit.Damage);
            Vfx.Play(VfxKind.Wound, transform.position + Vector3.up * 1.1f, hit.Direction);
            ScreenFlash.Flash(new Color(1f, 0.08f, 0.1f, 0.16f), 0.09f);
            hurtSeconds = seconds;
            hurtLeft = seconds;
            knockDirection = hit.Direction;
            knockDirection.y = 0f;
            Guard.SetRaised(false);
            Flasks.Interrupt();
            player.Dodge.Stop();
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

using Clover.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Clover.Player
{
    /// <summary>
    /// Adapts input to movement: walks the player relative to the camera's heading, turns them to face where they move, and
    /// runs the dodge roll. The rules live in <see cref="StaminaPool"/> and <see cref="DodgeRoll"/>. It is also the one place
    /// that decides, each frame and always in the same order, what the player is doing: flinching, rolling, attacking
    /// (<see cref="PlayerCombat"/>), guarding or drinking (<see cref="PlayerDefence"/>), or walking.
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private InputActionAsset controls;
        [SerializeField] private Transform cameraTransform;

        public float MoveSpeed = 6f;
        public float TurnDegreesPerSecond = 900f;
        public float Gravity = 25f;
        public StaminaPool Stamina = new StaminaPool();
        public DodgeRoll Dodge = new DodgeRoll();
        /// <summary>Seconds a dodge press is remembered, so one made a moment before it is allowed still rolls.</summary>
        public float DodgeBufferTime = 0.2f;
        /// <summary>Share of the walking speed kept while the guard is up, and while drinking.</summary>
        public float GuardMoveFactor = 0.45f;
        public float DrinkMoveFactor = 0.4f;
        /// <summary>How fast the raised guard turns to follow the nearest enemy.</summary>
        public float GuardTurnDegreesPerSecond = 540f;

        public bool IsInvulnerable => Dodge.IsInvulnerable;
        /// <summary>True while a menu has the buttons: the player does nothing and time is expected to be stopped.</summary>
        public bool Suspended;
        /// <summary>True while the player is in the air on a leap; nothing reaches them and nothing else can be done.</summary>
        public bool IsLeaping { get; private set; }
        public float LeapProgress => leapSeconds > 0f ? Mathf.Clamp01(leapElapsed / leapSeconds) : 0f;
        public bool IsGuarding => defence != null && defence.IsGuarding;
        /// <summary>Speed along the ground in metres per second, as asked for this frame.</summary>
        public float PlanarSpeed { get; private set; }

        private CharacterController body;
        private PlayerCombat combat;
        private PlayerDefence defence;
        private PlayerElement element;
        private PlayerUpgrades upgrades;
        private Vector3 leapFrom, leapTo;
        private float leapSeconds, leapElapsed, leapHeight;
        private InputActionAsset ownControls;
        private InputActionMap playerMap;
        private InputAction moveAction;
        private InputAction dodgeAction;
        private float fallSpeed;
        private float dodgeBuffered;

        public void Configure(InputActionAsset controlsAsset, Transform cameraToFollow)
        {
            controls = controlsAsset;
            cameraTransform = cameraToFollow;
        }

        /// <summary>One of the player's input actions by name; null when the controls are missing. Valid from Start on.</summary>
        public InputAction FindAction(string actionName) => playerMap?.FindAction(actionName, true);

        private void Awake()
        {
            body = GetComponent<CharacterController>();
            combat = GetComponent<PlayerCombat>();
            defence = GetComponent<PlayerDefence>();
            element = GetComponent<PlayerElement>();
            upgrades = GetComponent<PlayerUpgrades>();
            if (controls == null)
            {
                Debug.LogError("[Clover.Player] The controls asset is missing; the player cannot be driven.");
                enabled = false;
                return;
            }
            // A private copy: with domain reload off, enabling the shared asset would leak its state into the next play.
            ownControls = Instantiate(controls);
            playerMap = ownControls.FindActionMap("Player", true);
            moveAction = playerMap.FindAction("Move", true);
            dodgeAction = playerMap.FindAction("Dodge", true);
            Stamina.Fill();
            Dodge.Reset();
        }

        private void OnEnable() => playerMap?.Enable();

        private void OnDisable() => playerMap?.Disable();

        private void OnDestroy()
        {
            if (ownControls != null) Destroy(ownControls);
        }

        private void Update()
        {
            if (Suspended)
            {
                // Standing and listening, not running on the spot.
                PlanarSpeed = 0f;
                dodgeBuffered = 0f;
                if (defence != null) defence.DropHolds();
                if (element != null) element.DropHolds();
                return;
            }
            float deltaTime = Time.deltaTime;
            if (IsLeaping)
            {
                AdvanceLeap(deltaTime);
                return;
            }
            Vector3 wish = CameraRelative(moveAction.ReadValue<Vector2>());
            if (defence != null) defence.ReadInput();
            bool helpless = defence != null && defence.IsHelpless;
            bool attacking = combat != null && combat.IsAttacking;
            bool casting = element != null && element.IsCasting;

            // The dodge is settled before the weapon runs this frame, so it wins over an attack pressed with it. It may
            // interrupt an attack's recovery or a charge, never a wind-up or the swing itself; pressed early, it waits.
            if (dodgeAction.WasPressedThisFrame()) dodgeBuffered = DodgeBufferTime;
            // A cast can be rolled out of at any point; the mana is not given back.
            if (dodgeBuffered > 0f && !helpless && (!attacking || combat.CanBeCancelled))
            {
                // With no stick input the roll goes the way the player is facing.
                Vector3 rollDirection = wish.sqrMagnitude > 0.01f ? wish : transform.forward;
                if (Dodge.TryStart(rollDirection, Stamina))
                {
                    dodgeBuffered = 0f;
                    if (attacking) combat.Interrupt();
                    if (casting) element.Interrupt();
                    Vfx.Play(VfxKind.Dust, transform.position, -rollDirection);
                    if (upgrades != null) upgrades.OnDodge(transform.position, Dodge.Direction, Dodge.Distance, Dodge.Duration);
                }
            }
            if (dodgeBuffered > 0f) dodgeBuffered -= deltaTime;

            // A guard pressed during a recovery cuts it short the same way.
            if (defence != null && combat != null && !helpless && !Dodge.IsRolling && combat.IsAttacking
                && combat.CanBeCancelled && defence.WantsGuardCancel)
                combat.Interrupt();

            if (defence != null)
            {
                defence.Advance(deltaTime, Dodge.IsRolling, (combat != null && combat.IsAttacking) || (element != null && element.IsCasting));
                helpless = defence.IsHelpless;
            }
            bool busy = Dodge.IsRolling || helpless || (defence != null && defence.IsDrinking);
            if (element != null)
            {
                element.Advance(deltaTime, !busy && (combat == null || !combat.IsAttacking));
                casting = element.IsCasting;
            }
            if (combat != null)
            {
                combat.Advance(deltaTime, busy || casting);
                attacking = combat.IsAttacking;
            }

            Vector3 step;
            if (helpless)
            {
                Dodge.Tick(deltaTime);
                step = defence.Step;
            }
            else if (Dodge.IsRolling)
            {
                step = Dodge.Tick(deltaTime);
                // The roll commits to its direction at once; there is no turning mid-roll.
                transform.rotation = Quaternion.LookRotation(Dodge.Direction, Vector3.up);
            }
            else if (casting)
            {
                // A cast roots the player for its short length.
                Dodge.Tick(deltaTime);
                step = Vector3.zero;
            }
            else if (attacking)
            {
                Dodge.Tick(deltaTime);
                step = combat.Step;
                // While the charge is held the player stands and picks where the cut will go.
                if (combat.IsCharging && wish.sqrMagnitude > 0.01f) Face(wish, TurnDegreesPerSecond * deltaTime);
            }
            else
            {
                Dodge.Tick(deltaTime);
                bool guarding = defence != null && defence.IsGuarding;
                bool drinking = defence != null && defence.IsDrinking;
                float speed = MoveSpeed * (guarding ? GuardMoveFactor : drinking ? DrinkMoveFactor : 1f);
                step = wish * (speed * deltaTime);
                // The guard stays turned to the nearest enemy and the player sidesteps behind it; with no enemy near,
                // the stick turns the player as usual.
                Transform guardTarget = guarding ? defence.GuardTarget : null;
                if (guardTarget != null)
                {
                    Vector3 toward = guardTarget.position - transform.position;
                    toward.y = 0f;
                    if (toward.sqrMagnitude > 0.0025f) Face(toward, GuardTurnDegreesPerSecond * deltaTime);
                }
                else if (wish.sqrMagnitude > 0.01f)
                {
                    Face(wish, TurnDegreesPerSecond * deltaTime);
                }
            }

            PlanarSpeed = deltaTime > 0f ? new Vector2(step.x, step.z).magnitude / deltaTime : 0f;
            fallSpeed = body.isGrounded ? 1f : fallSpeed + Gravity * deltaTime;
            step.y = -fallSpeed * deltaTime;
            body.Move(step);
            Stamina.Tick(deltaTime);
        }

        /// <summary>
        /// Throws the player through the air to a spot on the ground, over the given seconds and up to the given height.
        /// Whatever they were doing is dropped. The upgrades are told when they land.
        /// </summary>
        public void Leap(Vector3 to, float seconds, float height)
        {
            if (combat != null) combat.Interrupt();
            if (element != null)
            {
                element.Interrupt();
                element.DropHolds();
            }
            if (defence != null) defence.DropHolds();
            Dodge.Stop();
            Vector3 toward = to - transform.position;
            toward.y = 0f;
            if (toward.sqrMagnitude > 0.01f) transform.rotation = Quaternion.LookRotation(toward, Vector3.up);
            leapFrom = transform.position;
            leapTo = to;
            leapSeconds = Mathf.Max(0.05f, seconds);
            leapHeight = height;
            leapElapsed = 0f;
            IsLeaping = true;
        }

        // The leap is a set path: rising fast, hanging, and coming down faster than it went up.
        private void AdvanceLeap(float deltaTime)
        {
            leapElapsed += deltaTime;
            float t = Mathf.Clamp01(leapElapsed / leapSeconds);
            // Most of the ground is covered in the fall, so it reads as coming down on someone.
            float along = t * t * (3f - 2f * t);
            Vector3 want = Vector3.Lerp(leapFrom, leapTo, along);
            want.y = Mathf.Lerp(leapFrom.y, leapTo.y, t) + leapHeight * 4f * t * (1f - t);
            body.Move(want - transform.position);
            PlanarSpeed = 0f;
            Stamina.Tick(deltaTime);
            if (defence != null) defence.Advance(deltaTime, true, false);
            if (combat != null) combat.Advance(deltaTime, true);
            if (t < 1f) return;
            IsLeaping = false;
            fallSpeed = 1f;
            if (upgrades != null) upgrades.OnLeapLanded();
        }

        /// <summary>Puts the player somewhere else at once, as a respawn does. A CharacterController ignores a plain move of its transform.</summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            body.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            body.enabled = true;
            fallSpeed = 0f;
            IsLeaping = false;
            Dodge.Reset();
        }

        /// <summary>Turns stick input into a direction on the ground as the camera sees it.</summary>
        public Vector3 CameraRelative(Vector2 input)
        {
            if (input.sqrMagnitude > 1f) input.Normalize();
            float yaw = cameraTransform != null ? cameraTransform.eulerAngles.y : 0f;
            return Quaternion.Euler(0f, yaw, 0f) * new Vector3(input.x, 0f, input.y);
        }

        private void Face(Vector3 direction, float maxDegrees)
        {
            var target = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, target, maxDegrees);
        }
    }
}

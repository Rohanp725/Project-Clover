using UnityEngine;
using UnityEngine.InputSystem;

namespace Clover.Player
{
    /// <summary>
    /// Adapts input to movement: walks the player relative to the camera's heading, turns them to face where they move, and
    /// runs the dodge roll. The rules live in <see cref="StaminaPool"/> and <see cref="DodgeRoll"/>. While an attack is in
    /// progress <see cref="PlayerCombat"/> decides the movement, and the dodge may cut the attack's recovery short.
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

        public bool IsInvulnerable => Dodge.IsInvulnerable;
        /// <summary>Speed along the ground in metres per second, as asked for this frame.</summary>
        public float PlanarSpeed { get; private set; }

        private CharacterController body;
        private PlayerCombat combat;
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
            float deltaTime = Time.deltaTime;
            Vector3 wish = CameraRelative(moveAction.ReadValue<Vector2>());
            bool attacking = combat != null && combat.IsAttacking;

            // The dodge is settled before the weapon runs this frame, so it wins over an attack pressed with it. It may
            // interrupt an attack's recovery, never its wind-up or the swing itself; pressed early, it waits its turn.
            if (dodgeAction.WasPressedThisFrame()) dodgeBuffered = DodgeBufferTime;
            if (dodgeBuffered > 0f && (!attacking || combat.CanBeCancelled))
            {
                // With no stick input the roll goes the way the player is facing.
                Vector3 rollDirection = wish.sqrMagnitude > 0.01f ? wish : transform.forward;
                if (Dodge.TryStart(rollDirection, Stamina))
                {
                    dodgeBuffered = 0f;
                    if (attacking) combat.Interrupt();
                }
            }
            if (dodgeBuffered > 0f) dodgeBuffered -= deltaTime;

            if (combat != null)
            {
                combat.Advance(deltaTime, Dodge.IsRolling);
                attacking = combat.IsAttacking;
            }

            Vector3 step;
            if (Dodge.IsRolling)
            {
                step = Dodge.Tick(deltaTime);
                // The roll commits to its direction at once; there is no turning mid-roll.
                transform.rotation = Quaternion.LookRotation(Dodge.Direction, Vector3.up);
            }
            else if (attacking)
            {
                Dodge.Tick(deltaTime);
                step = combat.Step;
            }
            else
            {
                Dodge.Tick(deltaTime);
                step = wish * (MoveSpeed * deltaTime);
                if (wish.sqrMagnitude > 0.01f) Face(wish, TurnDegreesPerSecond * deltaTime);
            }

            PlanarSpeed = deltaTime > 0f ? new Vector2(step.x, step.z).magnitude / deltaTime : 0f;
            fallSpeed = body.isGrounded ? 1f : fallSpeed + Gravity * deltaTime;
            step.y = -fallSpeed * deltaTime;
            body.Move(step);
            Stamina.Tick(deltaTime);
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

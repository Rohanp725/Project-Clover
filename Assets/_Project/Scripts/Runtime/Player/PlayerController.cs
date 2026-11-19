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
        /// <summary>What the ground underfoot does to the pace (cooling gold slows it). One everywhere else; set each frame by whatever lies there.</summary>
        [System.NonSerialized] public float GroundFactor = 1f;
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

        /// <summary>
        /// Not zero while a spell form has the body (a beam braced, a drill bored, a parry held): the number is the
        /// form's own token. The buttons do nothing but the dodge, which breaks a hold that may be broken.
        /// </summary>
        public int FormHold { get; private set; }
        /// <summary>True while the form holding the body lets nothing reach it.</summary>
        public bool FormUntouchable { get; private set; }
        public bool FormCancellable { get; private set; }
        /// <summary>Metres the form moves the body this frame; it is spent as it is used.</summary>
        public Vector3 FormStep;
        /// <summary>The animation state the form shows on the body (a hash; zero for none) and how far through it.</summary>
        public int FormPose;
        public float FormPoseProgress;
        /// <summary>The form holds the guard up over whatever the legs do, or has the sword out.</summary>
        public bool FormGuardPose;
        public bool FormSwordOut;
        /// <summary>True while the body is not drawn at all: it has become something else for a moment.</summary>
        public bool Hidden;
        /// <summary>A parry form is waiting for a blow; set when one came, with where it came from.</summary>
        public bool FormParrying;
        public bool FormParried;
        public Vector3 FormParriedFrom;
        /// <summary>Where the stick points on the ground, as last read; zero with no stick.</summary>
        public Vector3 Wish { get; private set; }

        private readonly RaycastHit[] sweep = new RaycastHit[16];
        private CharacterController body;
        private PlayerCombat combat;
        private PlayerDefence defence;
        private PlayerElement element;
        private PlayerSpells spells;
        private PlayerUpgrades upgrades;
        private Vector3 leapFrom, leapTo;
        private float leapSeconds, leapElapsed, leapHeight;
        private InputActionAsset ownControls;
        private InputActionMap playerMap;
        private InputActionMap spellsMap;
        private InputAction moveAction;
        private InputAction dodgeAction;
        private float fallSpeed;
        private float dodgeBuffered;
        private Grip heldBy;
        private InputAction struggleAction;
        private Camera aimCamera;
        private bool inputHeld;
        private Vector3 shoveVelocity;
        private float shoveLeft, shoveSeconds = 1f;

        /// <summary>True while something has hold of the player: they can do nothing but struggle.</summary>
        public bool IsHeld => heldBy != null && heldBy.IsHeld;
        /// <summary>How many times the player has been taken hold of.</summary>
        public int TimesHeld { get; private set; }

        /// <summary>
        /// Something takes hold of the player. Whatever they were doing is dropped, and until the grip lets go every
        /// press of light attack or dodge is a struggle against it. The holder ticks the grip and deals its bites.
        /// </summary>
        public void BeginHeld(Grip grip)
        {
            if (grip == null) return;
            BreakFormHold();
            if (combat != null) combat.Interrupt();
            if (element != null)
            {
                element.Interrupt();
                element.DropHolds();
            }
            if (defence != null)
            {
                defence.DropHolds();
                defence.Flasks.Interrupt();
            }
            Dodge.Stop();
            dodgeBuffered = 0f;
            heldBy = grip;
            TimesHeld++;
        }

        /// <summary>
        /// Pushes the body along the ground by about <paramref name="metres"/> over <paramref name="seconds"/>, on top of
        /// whatever it is doing, as a gust of wind does; walls stop it as they stop a step.
        /// </summary>
        public void Shove(Vector3 metres, float seconds)
        {
            metres.y = 0f;
            shoveSeconds = Mathf.Max(0.05f, seconds);
            shoveLeft = shoveSeconds;
            shoveVelocity = metres / shoveSeconds;
            Shoves++;
        }

        /// <summary>How many times the player has been shoved: for probes.</summary>
        public int Shoves { get; private set; }

        /// <summary>Counts one struggle against the hold, as a press of light attack or dodge does.</summary>
        public void Struggle()
        {
            if (heldBy != null) heldBy.Press();
        }

        public void Configure(InputActionAsset controlsAsset, Transform cameraToFollow)
        {
            controls = controlsAsset;
            cameraTransform = cameraToFollow;
        }

        /// <summary>One of the player's input actions by name, from the Player map or the Spells map; null when there is none. Valid from Start on.</summary>
        public InputAction FindAction(string actionName) => playerMap?.FindAction(actionName) ?? spellsMap?.FindAction(actionName);

        /// <summary>
        /// The buttons with the L2 layer settled: what the player's parts ask instead of reading an action, so a face
        /// button held under L2 is a spell and not also a flask or a dodge. Valid from Awake on.
        /// </summary>
        public LayeredButtons Buttons { get; private set; }

        private void Awake()
        {
            body = GetComponent<CharacterController>();
            combat = GetComponent<PlayerCombat>();
            defence = GetComponent<PlayerDefence>();
            element = GetComponent<PlayerElement>();
            spells = GetComponent<PlayerSpells>();
            // A player built before the four slots existed gets them here, so no scene needs rebuilding for them.
            if (spells == null) spells = gameObject.AddComponent<PlayerSpells>();
            // The charged blade's look on the weapon, added the same way so no scene is rebuilt.
            if (GetComponent<BladeLook>() == null) gameObject.AddComponent<BladeLook>();
            // The element on a swung weapon (the swing trails, the sheath, Life's thorns), added the same way.
            if (GetComponent<Swings.SwingTrails>() == null) gameObject.AddComponent<Swings.SwingTrails>();
            // The build (the Hand and the attunements), added the same way.
            if (GetComponent<PlayerBuild>() == null) gameObject.AddComponent<PlayerBuild>();
            upgrades = GetComponent<PlayerUpgrades>();
            // The reactions' adapter (it needs the upgrades' runner), added the same way.
            if (upgrades != null && GetComponent<PlayerReactions>() == null) gameObject.AddComponent<PlayerReactions>();
            if (controls == null)
            {
                Debug.LogError("[Clover.Player] The controls asset is missing; the player cannot be driven.");
                enabled = false;
                return;
            }
            // A private copy: with domain reload off, enabling the shared asset would leak its state into the next play.
            ownControls = Instantiate(controls);
            // The player's own bindings go on before anything can read a button.
            ControlBindings.Use(ownControls);
            playerMap = ownControls.FindActionMap("Player", true);
            // Controls saved before the spell layer existed have no Spells map: the player still walks and fights.
            spellsMap = ownControls.FindActionMap("Spells");
            moveAction = playerMap.FindAction("Move", true);
            dodgeAction = playerMap.FindAction("Dodge", true);
            playerMap.actionTriggered += NoticeDevice;
            if (spellsMap != null) spellsMap.actionTriggered += NoticeDevice;
            Buttons = new LayeredButtons(playerMap, spellsMap);
            if (cameraTransform != null) aimCamera = cameraTransform.GetComponent<Camera>();
            Stamina.Fill();
            Dodge.Reset();
        }

        private void OnEnable()
        {
            if (inputHeld) return;
            playerMap?.Enable();
            spellsMap?.Enable();
        }

        private void OnDisable()
        {
            playerMap?.Disable();
            spellsMap?.Disable();
        }

        private void OnDestroy()
        {
            if (playerMap != null) playerMap.actionTriggered -= NoticeDevice;
            if (spellsMap != null) spellsMap.actionTriggered -= NoticeDevice;
            ControlBindings.Forget(ownControls);
            if (ownControls != null) Destroy(ownControls);
        }

        // Prompts follow whichever device's press an action last heard; a resting pad's steady reports never get this far.
        private static void NoticeDevice(InputAction.CallbackContext context)
        {
            if (context.phase == InputActionPhase.Started || context.phase == InputActionPhase.Performed)
                ControlDevices.Notice(context.control?.device);
        }

        /// <summary>
        /// Turns every button of the player off (true) or back on (false): a rebind needs its action quiet while it
        /// listens. Menus read the same actions, so they hear nothing either until it is given back.
        /// </summary>
        public void HoldInput(bool held)
        {
            inputHeld = held;
            if (playerMap == null) return;
            if (held)
            {
                playerMap.Disable();
                spellsMap?.Disable();
                Buttons?.Drop();
            }
            else if (isActiveAndEnabled)
            {
                playerMap.Enable();
                spellsMap?.Enable();
            }
        }

        /// <summary>The player's copy of the controls, with their own bindings on it: what the controls page rebinds.</summary>
        public InputActionAsset Controls => ownControls;

        /// <summary>
        /// Where the mouse cursor lies on the ground, as a direction from the player, while the mouse is the device in
        /// use. False with a pad, with no mouse, or with the cursor on the player.
        /// </summary>
        public bool PointerAim(out Vector3 direction)
        {
            direction = Vector3.zero;
            var mouse = Mouse.current;
            if (!ControlDevices.MouseAim || mouse == null) return false;
            var view = aimCamera != null ? aimCamera : Camera.main;
            if (view == null) return false;
            var ray = view.ScreenPointToRay(mouse.position.ReadValue());
            var ground = new Plane(Vector3.up, transform.position);
            if (!ground.Raycast(ray, out float distance)) return false;
            Vector3 offset = ray.GetPoint(distance) - transform.position;
            offset.y = 0f;
            if (offset.sqrMagnitude < 0.04f) return false;
            direction = offset.normalized;
            return true;
        }

        /// <summary>Where a held spell or charge turns: the cursor while the mouse is in use, else the stick.</summary>
        public Vector3 Steer => PointerAim(out Vector3 toward) ? toward : Wish;

        /// <summary>Turns the player at once to the cursor, when the mouse is in use: what a spell does as it is pressed.</summary>
        public void FacePointer()
        {
            if (PointerAim(out Vector3 toward)) transform.rotation = Quaternion.LookRotation(toward, Vector3.up);
        }

        private void Update()
        {
            // Moving the mouse makes it the device in use, so the cursor aims and the prompts name keys.
            var mouse = Mouse.current;
            if (mouse != null && !inputHeld && mouse.delta.ReadValue().sqrMagnitude > ControlDevices.MousePixels * ControlDevices.MousePixels)
                ControlDevices.Notice(mouse);
            if (Suspended)
            {
                // Standing and listening, not running on the spot.
                PlanarSpeed = 0f;
                dodgeBuffered = 0f;
                if (defence != null) defence.DropHolds();
                if (element != null) element.DropHolds();
                Buttons?.Drop();
                return;
            }
            float deltaTime = Time.deltaTime;
            if (heldBy != null)
            {
                if (!heldBy.IsHeld) heldBy = null;
                else
                {
                    // Held fast: no step, no blow, no guard, no drink. Only the struggle is heard.
                    PlanarSpeed = 0f;
                    dodgeBuffered = 0f;
                    if (struggleAction == null) struggleAction = playerMap.FindAction("LightAttack", true);
                    if (Buttons.Pressed(PlayerButton.Dodge) || struggleAction.WasPressedThisFrame()) heldBy.Press();
                    if (defence != null)
                    {
                        defence.DropHolds();
                        defence.Advance(deltaTime, false, true);
                    }
                    if (element != null) element.DropHolds();
                    Stamina.Tick(deltaTime);
                    return;
                }
            }
            if (IsLeaping)
            {
                AdvanceLeap(deltaTime);
                return;
            }
            Vector3 wish = CameraRelative(moveAction.ReadValue<Vector2>());
            Wish = wish;
            if (FormHold != 0)
            {
                AdvanceFormHold(deltaTime);
                return;
            }
            if (defence != null) defence.ReadInput();
            bool helpless = defence != null && defence.IsHelpless;
            bool attacking = combat != null && combat.IsAttacking;
            bool casting = element != null && element.IsCasting;

            // The dodge is settled before the weapon runs this frame, so it wins over an attack pressed with it. It may
            // interrupt an attack's recovery or a charge, never a wind-up or the swing itself; pressed early, it waits.
            if (Buttons.Pressed(PlayerButton.Dodge)) dodgeBuffered = DodgeBufferTime;
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
                // The guard's own thrust does not bring the guard down.
                defence.Advance(deltaTime, Dodge.IsRolling, (combat != null && combat.IsAttacking && !combat.IsGuardAttacking) || (element != null && element.IsCasting));
                helpless = defence.IsHelpless;
            }
            bool busy = Dodge.IsRolling || helpless || (defence != null && defence.IsDrinking);
            if (element != null)
            {
                element.Advance(deltaTime, !busy && (combat == null || !combat.IsAttacking));
                // The four slots, their cooldowns and the fight's clock, right after the element in the one order.
                if (spells != null) spells.Advance(deltaTime, !busy && (combat == null || !combat.IsAttacking));
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
                // The roll has just come to its end: some upgrades answer that.
                if (!Dodge.IsRolling && upgrades != null) upgrades.OnDodgeEnd();
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
                // While the charge is held the player stands and picks where the cut will go (with the stick, or the
                // cursor while the mouse is in use); some weapons let them creep.
                Vector3 pick = combat.IsCharging ? Steer : Vector3.zero;
                if (pick.sqrMagnitude > 0.01f) Face(pick, TurnDegreesPerSecond * deltaTime);
                if (combat.IsCharging && combat.Combo.ChargeMoveFactor > 0f) step = wish * (MoveSpeed * combat.Combo.ChargeMoveFactor * deltaTime);
            }
            else
            {
                Dodge.Tick(deltaTime);
                bool guarding = defence != null && defence.IsGuarding;
                bool drinking = defence != null && defence.IsDrinking;
                var carried = combat != null ? combat.Weapon : null;
                float speed = MoveSpeed * GroundFactor * (carried != null ? carried.MoveFactor : 1f)
                    * (guarding ? (carried != null ? carried.GuardMoveFactor : GuardMoveFactor) : drinking ? DrinkMoveFactor : 1f);
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
            if (shoveLeft > 0f)
            {
                // A shove carries the body on over whatever it is doing, easing off as it ends.
                float share = Mathf.Min(deltaTime, shoveLeft);
                step += shoveVelocity * (share * Mathf.Clamp01(shoveLeft / shoveSeconds) * 2f);
                shoveLeft -= deltaTime;
            }
            fallSpeed = body.isGrounded ? 1f : fallSpeed + Gravity * deltaTime;
            step.y = -fallSpeed * deltaTime;
            if (combat != null && combat.PassingThrough)
            {
                // A passing cut goes through bodies and stops at walls: the body is put there. The controller is not
                // asked to move at all on these frames: standing inside an enemy, any move of its own pushes it back out
                // the way it came, and a pass made in small steps never gets through.
                Vector3 along = new Vector3(step.x, 0f, step.z);
                float far = along.magnitude;
                if (far > 0.0001f)
                {
                    float clear = ClearRun(along, far);
                    body.enabled = false;
                    transform.position += along * (clear / far);
                    body.enabled = true;
                }
            }
            else body.Move(step);
            Stamina.Tick(deltaTime);
        }

        /// <summary>
        /// Throws the player through the air to a spot on the ground, over the given seconds and up to the given height.
        /// Whatever they were doing is dropped. The upgrades are told when they land.
        /// </summary>
        public void Leap(Vector3 to, float seconds, float height)
        {
            BreakFormHold();
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

        /// <summary>
        /// Gives the body to a spell form until it gives it back: whatever the player was doing is dropped. A hold that
        /// is untouchable lets no blow land; one that is cancellable ends when the dodge is pressed, and the roll follows.
        /// False when the body cannot be taken now.
        /// </summary>
        public bool BeginFormHold(int token, bool untouchable, bool cancellable)
        {
            if (token == 0 || IsLeaping || (defence != null && defence.IsHelpless)) return false;
            EndFormHold(FormHold);
            if (combat != null) combat.Interrupt();
            if (element != null)
            {
                element.Interrupt();
                element.DropHolds();
            }
            if (defence != null) defence.DropHolds();
            Dodge.Stop();
            FormHold = token;
            FormUntouchable = untouchable;
            FormCancellable = cancellable;
            FormStep = Vector3.zero;
            return true;
        }

        /// <summary>The form gives the body back. Nothing happens unless it is the one holding it.</summary>
        public void EndFormHold(int token)
        {
            if (token == 0 || FormHold != token) return;
            FormHold = 0;
            FormUntouchable = false;
            FormCancellable = false;
            FormStep = Vector3.zero;
            FormPose = 0;
            FormGuardPose = false;
            FormSwordOut = false;
            FormParrying = false;
            Hidden = false;
            fallSpeed = 1f;
        }

        /// <summary>Whatever form holds the body lets go: a blow landed, the player fell, or the scene took them.</summary>
        public void BreakFormHold() => EndFormHold(FormHold);

        private void AdvanceFormHold(float deltaTime)
        {
            if (FormCancellable && Buttons.Pressed(PlayerButton.Dodge))
            {
                // The roll itself is settled next frame, in the usual order.
                dodgeBuffered = DodgeBufferTime;
                BreakFormHold();
                return;
            }
            Vector3 step = FormStep;
            FormStep = Vector3.zero;
            PlanarSpeed = 0f;
            if (body.enabled)
            {
                fallSpeed = body.isGrounded ? 1f : fallSpeed + Gravity * deltaTime;
                step.y = -fallSpeed * deltaTime;
                body.Move(step);
            }
            Stamina.Tick(deltaTime);
            if (defence != null) defence.Advance(deltaTime, true, false);
            if (combat != null) combat.Advance(deltaTime, true);
        }

        /// <summary>
        /// How far the body can go in a straight line before something solid stops it. Things that can be shoved aside
        /// (enemies) do not count: a form that passes through them asks this first.
        /// </summary>
        public float ClearRun(Vector3 direction, float distance)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f || distance <= 0f) return 0f;
            direction.Normalize();
            Vector3 foot = transform.position + Vector3.up * (body.radius + 0.35f);
            Vector3 head = transform.position + Vector3.up * (body.height - body.radius);
            int count = Physics.CapsuleCastNonAlloc(foot, head, body.radius * 0.8f, direction, sweep, distance, ~0, QueryTriggerInteraction.Ignore);
            float clear = distance;
            for (int i = 0; i < count; i++)
            {
                var other = sweep[i].collider;
                if (other.transform.root == transform.root) continue;
                if (other.GetComponentInParent<Clover.Gameplay.IShovable>() != null) continue;
                clear = Mathf.Min(clear, Mathf.Max(0f, sweep[i].distance - 0.2f));
            }
            return clear;
        }

        /// <summary>Puts the body at a spot at once, through whatever stands between, keeping its feet on the ground there.</summary>
        public void PassTo(Vector3 to)
        {
            if (Physics.Raycast(to + Vector3.up * 1.5f, Vector3.down, out RaycastHit under, 4f, ~0, QueryTriggerInteraction.Ignore)
                && Mathf.Abs(under.point.y - transform.position.y) < 1.2f)
                to.y = under.point.y + 0.02f;
            else to.y = transform.position.y;
            body.enabled = false;
            transform.position = to;
            body.enabled = true;
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

        /// <summary>
        /// A short step through the air to a spot on the ground, as some upgrades give: there at once if the spot has
        /// ground under it and room to stand, and otherwise as far toward it as the world allows.
        /// </summary>
        public void StepTo(Vector3 to)
        {
            to.y = transform.position.y;
            bool ground = Physics.Raycast(to + Vector3.up * 1.5f, Vector3.down, out RaycastHit under, 3f, ~0, QueryTriggerInteraction.Ignore)
                && Mathf.Abs(under.point.y - transform.position.y) < 0.6f;
            bool room = ground && !Physics.CheckCapsule(under.point + Vector3.up * (body.radius + 0.15f), under.point + Vector3.up * (body.height - body.radius), body.radius * 0.9f, ~0, QueryTriggerInteraction.Ignore);
            if (room)
            {
                body.enabled = false;
                transform.position = new Vector3(to.x, under.point.y + 0.02f, to.z);
                body.enabled = true;
            }
            else body.Move(to - transform.position);
        }

        /// <summary>Puts the player somewhere else at once, as a respawn does. A CharacterController ignores a plain move of its transform.</summary>
        public void Teleport(Vector3 position, Quaternion rotation)
        {
            shoveLeft = 0f;
            body.enabled = false;
            transform.SetPositionAndRotation(position, rotation);
            body.enabled = true;
            fallSpeed = 0f;
            IsLeaping = false;
            BreakFormHold();
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

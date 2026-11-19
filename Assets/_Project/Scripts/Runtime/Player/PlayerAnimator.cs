using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// Shows what the player is doing on the character model. Idle and run blend by speed. The roll, the flinch, the
    /// charge, the cast and every attack are scrubbed by the game's own progress through them, so the animation always
    /// starts, lands and ends exactly when the rules say, whatever the timings are tuned to. The guard and the drink
    /// play on the upper body only, over the legs' own walking. Also shows or hides the sword to suit the weapon in
    /// hand, and moves it between sheath and hand.
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private PlayerDefence defence;
        [SerializeField] private PlayerElement element;
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject drawnSword;
        [SerializeField] private GameObject sheathedHilt;
        [SerializeField] private GameObject sheath;
        [SerializeField] private WeaponRack rack;

        /// <summary>Seconds the blend between idle and run takes to follow a change of speed.</summary>
        public float SpeedDampTime = 0.07f;
        /// <summary>Seconds to blend from the end of a roll or an attack back to moving.</summary>
        public float ExitBlend = 0.1f;
        /// <summary>Seconds after the last attack before the sword goes back in its sheath.</summary>
        public float SheatheDelay = 0.6f;
        /// <summary>Seconds the guard and the drink take to come up over the body and to go down again.</summary>
        public float UpperBodyBlend = 0.07f;
        /// <summary>Seconds the fall takes, from the blow to lying still.</summary>
        public float DeathSeconds = 1.1f;
        /// <summary>Seconds the guard's answer to a blocked blow, and to a blow turned perfectly, plays for.</summary>
        public float GuardHitSeconds = 0.22f;
        public float PerfectGuardSeconds = 0.30f;
        /// <summary>How brightly a weapon's inlay burns with the held element: at rest, in a surge, and for a charge or the skill.</summary>
        public float InlayRest = 0.7f;
        public float InlaySurge = 2.2f;
        public float InlaySkill = 3f;

        public const string SpeedParameter = "Speed";
        public const string LocomotionState = "locomotion";
        public const string FistLocomotionState = "locomotion_fist";
        public const string DeathState = "death";
        public const string RollState = "roll";
        public const string ChargeState = "charge";
        public const string FistChargeState = "charge_fist";
        public const string HurtState = "hurt";
        public const string CastState = "cast";
        public const string LeapState = "leap";
        public const string UpperBodyLayer = "Upper Body";
        public const string UpperIdleState = "none";
        public const string GuardState = "guard";
        public const string FistGuardState = "guard_fist";
        public const string DrinkState = "drink";

        private static readonly int SpeedHash = Animator.StringToHash(SpeedParameter);
        private static readonly int LocomotionHash = Animator.StringToHash(LocomotionState);
        private static readonly int RollHash = Animator.StringToHash(RollState);
        private static readonly int FistLocomotionHash = Animator.StringToHash(FistLocomotionState);
        private static readonly int DeathHash = Animator.StringToHash(DeathState);
        private static readonly int HurtHash = Animator.StringToHash(HurtState);
        private static readonly int CastHash = Animator.StringToHash(CastState);
        private static readonly int LeapHash = Animator.StringToHash(LeapState);
        private static readonly int GuardHash = Animator.StringToHash(GuardState);
        private static readonly int FistGuardHash = Animator.StringToHash(FistGuardState);
        private static readonly int DrinkHash = Animator.StringToHash(DrinkState);

        // A clip scrubbed to exactly its end would wrap to its start.
        private const float ClipEnd = 0.99f;

        private AttackStep shownStep;
        private int shownStepHash;
        private Weapon shownWeapon;
        private int chargeHash;
        private bool wasBusy;
        private float sinceAttack = float.MaxValue;
        private float guardFor;
        private float upperWeight;
        private int upperLayer = -1;
        private int shownLocomotion;
        private Renderer[] drawn;
        private bool hidden;
        private int locomotionHash, guardHash, rollHash, guardHitHash, perfectGuardHash;
        private int seenBlocks, seenPerfects;
        private int reactionHash;
        private int awayHash, catchHash, dashHash, seenCatches;
        /// <summary>Seconds the catch of a returning weapon plays for.</summary>
        public float CatchSeconds = 0.28f;
        private float reactionFor, reactionSeconds;

        /// <summary>Adds the weapons hung on the character's sockets, shown to suit the weapon in hand.</summary>
        public void ConfigureRack(WeaponRack rackToShow) => rack = rackToShow;

        /// <summary>The weapon as it is seen now: the drawn sword, the sheath it waits in, or the rack's part. Null for bare hands.</summary>
        public GameObject ShownWeapon
        {
            get
            {
                if (drawnSword != null && drawnSword.activeInHierarchy) return drawnSword;
                if (sheath != null && sheath.activeInHierarchy) return sheath;
                return rack != null ? rack.ShownPart : null;
            }
        }

        public void Configure(PlayerController playerToShow, PlayerCombat combatToShow, PlayerDefence defenceToShow,
            PlayerElement elementToShow, Animator animatorToDrive, GameObject drawnSwordObject, GameObject sheathedHiltObject,
            GameObject sheathObject)
        {
            player = playerToShow;
            combat = combatToShow;
            defence = defenceToShow;
            element = elementToShow;
            animator = animatorToDrive;
            drawnSword = drawnSwordObject;
            sheathedHilt = sheathedHiltObject;
            sheath = sheathObject;
        }

        private void Start()
        {
            if (animator != null) upperLayer = animator.GetLayerIndex(UpperBodyLayer);
            if (animator != null) drawn = animator.GetComponentsInChildren<Renderer>(true);
        }

        // A state of the upper body by name, or zero when the controller was built without it.
        private int Upper(string state)
        {
            if (string.IsNullOrEmpty(state) || upperLayer < 0) return 0;
            int hash = Animator.StringToHash(state);
            return animator.HasState(upperLayer, hash) ? hash : 0;
        }

        private void LateUpdate()
        {
            if (player == null || animator == null || combat == null || combat.Weapon == null) return;
            float deltaTime = Time.deltaTime;
            var weapon = combat.Weapon;
            if (weapon != shownWeapon)
            {
                shownWeapon = weapon;
                chargeHash = Animator.StringToHash(weapon.ChargeAnimation);
                locomotionHash = Animator.StringToHash(weapon.LocomotionState);
                guardHash = Animator.StringToHash(weapon.GuardState);
                rollHash = Animator.StringToHash(weapon.RollState);
                guardHitHash = string.IsNullOrEmpty(weapon.GuardHitState) ? 0 : Animator.StringToHash(weapon.GuardHitState);
                perfectGuardHash = string.IsNullOrEmpty(weapon.PerfectGuardState) ? 0 : Animator.StringToHash(weapon.PerfectGuardState);
                // A state the controller was built without is not asked for.
                if (guardHitHash != 0 && upperLayer >= 0 && !animator.HasState(upperLayer, guardHitHash)) guardHitHash = 0;
                if (perfectGuardHash != 0 && upperLayer >= 0 && !animator.HasState(upperLayer, perfectGuardHash)) perfectGuardHash = 0;
                if (!animator.HasState(0, rollHash)) rollHash = RollHash;
                awayHash = Upper(weapon.AwayState);
                catchHash = Upper(weapon.CatchState);
                dashHash = string.IsNullOrEmpty(weapon.DashState) ? 0 : Animator.StringToHash(weapon.DashState);
                if (dashHash != 0 && !animator.HasState(0, dashHash)) dashHash = 0;
                seenCatches = combat.Catches;
                shownStep = null;
                reactionHash = 0;
                if (defence != null)
                {
                    seenBlocks = defence.Blocks;
                    seenPerfects = defence.PerfectGuards;
                }
            }

            bool helpless = defence != null && defence.IsHelpless;
            bool leaping = player.IsLeaping;
            // A spell form may show a pose of its own on the body, or take the body out of sight altogether.
            bool posed = !helpless && !leaping && player.FormHold != 0 && player.FormPose != 0;
            if (player.Hidden != hidden && drawn != null)
            {
                hidden = player.Hidden;
                foreach (var part in drawn)
                    if (part != null) part.enabled = !hidden;
            }
            bool rolling = !helpless && !leaping && player.Dodge.IsRolling;
            bool charging = !helpless && !rolling && !leaping && combat.IsCharging;
            bool attacking = !helpless && !rolling && !leaping && !charging && combat.Combo.Current != null;
            bool casting = !helpless && !rolling && !charging && !attacking && element != null && element.IsCasting;
            bool drinking = !helpless && defence != null && defence.IsDrinking;
            // The guard's own thrust is a whole-body clip that holds the guard itself: the upper body's guard would hide its arms.
            bool guarding = !helpless && !combat.IsGuardAttacking && ((defence != null && defence.IsGuarding) || (player.FormHold != 0 && player.FormGuardPose));

            int locomotion = locomotionHash;
            if (helpless && defence.IsDead)
            {
                animator.Play(DeathHash, 0, Mathf.Min(ClipEnd, defence.DeadSeconds / DeathSeconds));
            }
            else if (helpless)
            {
                animator.Play(HurtHash, 0, Mathf.Min(ClipEnd, defence.HurtProgress));
            }
            else if (leaping)
            {
                // The run to a thrown weapon is a leap laid flat, with its own look.
                animator.Play(combat.IsDashing && dashHash != 0 ? dashHash : LeapHash, 0, Mathf.Min(ClipEnd, player.LeapProgress));
            }
            else if (posed)
            {
                animator.Play(player.FormPose, 0, Mathf.Clamp(player.FormPoseProgress, 0f, ClipEnd));
            }
            else if (rolling)
            {
                animator.Play(rollHash, 0, player.Dodge.Progress);
            }
            else if (charging)
            {
                animator.Play(chargeHash, 0, Mathf.Min(ClipEnd, combat.Combo.ChargeProgress));
            }
            else if (attacking)
            {
                var step = combat.Combo.Current;
                if (step != shownStep)
                {
                    shownStep = step;
                    shownStepHash = Animator.StringToHash(step.Name);
                }
                animator.Play(shownStepHash, 0, Mathf.Min(ClipEnd, combat.Combo.StepProgress));
            }
            else if (casting)
            {
                animator.Play(CastHash, 0, Mathf.Min(ClipEnd, element.CastProgress));
            }
            else
            {
                if (wasBusy || locomotion != shownLocomotion) animator.CrossFadeInFixedTime(locomotion, ExitBlend);
                shownLocomotion = locomotion;
                float fraction = player.MoveSpeed > 0f ? Mathf.Clamp01(player.PlanarSpeed / player.MoveSpeed) : 0f;
                animator.SetFloat(SpeedHash, fraction, SpeedDampTime, deltaTime);
            }
            wasBusy = helpless || leaping || posed || rolling || charging || attacking || casting;

            if (upperLayer >= 0)
            {
                guardFor = guarding ? guardFor + deltaTime : 0f;
                // A blow on the guard is answered: the guard is driven back, or the blow is thrown off.
                if (defence != null && (defence.Blocks != seenBlocks || defence.PerfectGuards != seenPerfects))
                {
                    bool perfect = defence.PerfectGuards != seenPerfects;
                    seenBlocks = defence.Blocks;
                    seenPerfects = defence.PerfectGuards;
                    reactionHash = perfect ? perfectGuardHash : guardHitHash;
                    reactionSeconds = perfect ? PerfectGuardSeconds : GuardHitSeconds;
                    reactionFor = 0f;
                }
                if (combat.Catches != seenCatches)
                {
                    // A thrown weapon has come home: the hand that was waiting is driven back by it.
                    seenCatches = combat.Catches;
                    reactionHash = catchHash;
                    reactionSeconds = CatchSeconds;
                    reactionFor = 0f;
                }
                bool waiting = awayHash != 0 && combat.WeaponAway && !helpless && !attacking && !rolling && !leaping && !drinking;
                bool reacting = reactionHash != 0 && !helpless && !attacking && !rolling;
                if (reacting)
                {
                    reactionFor += Time.unscaledDeltaTime;
                    if (reactionFor >= reactionSeconds)
                    {
                        reactionHash = 0;
                        reacting = false;
                    }
                }
                else reactionHash = 0;
                if (drinking) animator.Play(DrinkHash, upperLayer, Mathf.Min(ClipEnd, defence.Flasks.Progress));
                else if (reacting) animator.Play(reactionHash, upperLayer, Mathf.Min(ClipEnd, reactionFor / reactionSeconds));
                else if (guarding) animator.Play(guardHash, upperLayer, Mathf.Min(ClipEnd, guardFor / Mathf.Max(0.01f, UpperBodyBlend)));
                else if (waiting) animator.Play(awayHash, upperLayer, ClipEnd);
                float target = drinking || guarding || reacting || waiting ? 1f : 0f;
                upperWeight = Mathf.MoveTowards(upperWeight, target, deltaTime / Mathf.Max(0.01f, UpperBodyBlend));
                animator.SetLayerWeight(upperLayer, upperWeight);
            }

            sinceAttack = attacking ? 0f : sinceAttack + deltaTime;
            // With the sword carried: out for an attack and a little after, and for the guard; a charge is made from
            // the sheath. With bare hands there is no sword on the character at all.
            bool inHand = weapon.UsesSword && leaping || weapon.UsesSword && posed && player.FormSwordOut
                || weapon.UsesSword && !posed && !rolling && !charging && !drinking && !casting && (guarding || sinceAttack < SheatheDelay);
            Show(drawnSword, inHand);
            Show(sheathedHilt, weapon.UsesSword && !inHand);
            Show(sheath, weapon.UsesSword);

            if (rack != null)
            {
                // A great weapon never leaves the hand except to drink or to cast, when it hangs at rest on the body.
                rack.Show(weapon.UsesSword || combat.WeaponAway ? null : weapon.Carry,weapon.ElementColoured || !(drinking || casting || (helpless && defence.IsDead)));
                bool lit = charging || (attacking && (combat.Combo.Current == combat.Combo.Skill || combat.IsFinishing));
                bool surging = element != null && element.SurgeActive;
                float heat = lit ? InlaySkill : surging ? InlaySurge : InlayRest;
                if (element != null && element.Info != null) rack.Tint(true, surging && element.SurgeInfo != null ? element.SurgeInfo.Colour : element.Info.Colour, heat);
                else rack.Tint(false, Color.black, 0f);
            }
        }

        private static void Show(GameObject part, bool shown)
        {
            if (part != null && part.activeSelf != shown) part.SetActive(shown);
        }
    }
}

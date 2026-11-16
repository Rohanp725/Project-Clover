using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// Shows what the player is doing on the character model. Idle and run blend by speed. The roll, the flinch, the
    /// charge and every attack are scrubbed by the game's own progress through them, so the animation always starts,
    /// lands and ends exactly when the rules say, whatever the timings are tuned to. The guard and the drink play on
    /// the upper body only, over the legs' own walking. Also moves the sword between sheath and hand.
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private PlayerDefence defence;
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject drawnSword;
        [SerializeField] private GameObject sheathedHilt;

        /// <summary>Seconds the blend between idle and run takes to follow a change of speed.</summary>
        public float SpeedDampTime = 0.07f;
        /// <summary>Seconds to blend from the end of a roll or an attack back to moving.</summary>
        public float ExitBlend = 0.1f;
        /// <summary>Seconds after the last attack before the sword goes back in its sheath.</summary>
        public float SheatheDelay = 0.6f;
        /// <summary>Seconds the guard and the drink take to come up over the body and to go down again.</summary>
        public float UpperBodyBlend = 0.07f;

        public const string SpeedParameter = "Speed";
        public const string LocomotionState = "locomotion";
        public const string RollState = "roll";
        public const string ChargeState = "charge";
        public const string HurtState = "hurt";
        public const string UpperBodyLayer = "Upper Body";
        public const string UpperIdleState = "none";
        public const string GuardState = "guard";
        public const string DrinkState = "drink";

        private static readonly int SpeedHash = Animator.StringToHash(SpeedParameter);
        private static readonly int LocomotionHash = Animator.StringToHash(LocomotionState);
        private static readonly int RollHash = Animator.StringToHash(RollState);
        private static readonly int ChargeHash = Animator.StringToHash(ChargeState);
        private static readonly int HurtHash = Animator.StringToHash(HurtState);
        private static readonly int GuardHash = Animator.StringToHash(GuardState);
        private static readonly int DrinkHash = Animator.StringToHash(DrinkState);

        // A clip scrubbed to exactly its end would wrap to its start.
        private const float ClipEnd = 0.99f;

        private AttackStep shownStep;
        private int shownStepHash;
        private bool wasBusy;
        private float sinceAttack = float.MaxValue;
        private float guardFor;
        private float upperWeight;
        private int upperLayer = -1;

        public void Configure(PlayerController playerToShow, PlayerCombat combatToShow, PlayerDefence defenceToShow,
            Animator animatorToDrive, GameObject drawnSwordObject, GameObject sheathedHiltObject)
        {
            player = playerToShow;
            combat = combatToShow;
            defence = defenceToShow;
            animator = animatorToDrive;
            drawnSword = drawnSwordObject;
            sheathedHilt = sheathedHiltObject;
        }

        private void Start()
        {
            if (animator != null) upperLayer = animator.GetLayerIndex(UpperBodyLayer);
            ShowSword(false);
        }

        private void LateUpdate()
        {
            if (player == null || animator == null) return;
            float deltaTime = Time.deltaTime;
            bool helpless = defence != null && defence.IsHelpless;
            bool rolling = !helpless && player.Dodge.IsRolling;
            bool charging = !helpless && !rolling && combat != null && combat.IsCharging;
            bool attacking = !helpless && !rolling && !charging && combat != null && combat.Combo.Current != null;
            bool drinking = !helpless && defence != null && defence.IsDrinking;
            bool guarding = !helpless && defence != null && defence.IsGuarding;

            if (helpless)
            {
                animator.Play(HurtHash, 0, Mathf.Min(ClipEnd, defence.HurtProgress));
            }
            else if (rolling)
            {
                animator.Play(RollHash, 0, player.Dodge.Progress);
            }
            else if (charging)
            {
                animator.Play(ChargeHash, 0, Mathf.Min(ClipEnd, combat.Combo.ChargeProgress));
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
            else
            {
                if (wasBusy) animator.CrossFadeInFixedTime(LocomotionHash, ExitBlend);
                float fraction = player.MoveSpeed > 0f ? Mathf.Clamp01(player.PlanarSpeed / player.MoveSpeed) : 0f;
                animator.SetFloat(SpeedHash, fraction, SpeedDampTime, deltaTime);
            }
            wasBusy = helpless || rolling || charging || attacking;

            if (upperLayer >= 0)
            {
                guardFor = guarding ? guardFor + deltaTime : 0f;
                if (drinking) animator.Play(DrinkHash, upperLayer, Mathf.Min(ClipEnd, defence.Flasks.Progress));
                else if (guarding) animator.Play(GuardHash, upperLayer, Mathf.Min(ClipEnd, guardFor / Mathf.Max(0.01f, UpperBodyBlend)));
                float target = drinking || guarding ? 1f : 0f;
                upperWeight = Mathf.MoveTowards(upperWeight, target, deltaTime / Mathf.Max(0.01f, UpperBodyBlend));
                animator.SetLayerWeight(upperLayer, upperWeight);
            }

            sinceAttack = attacking ? 0f : sinceAttack + deltaTime;
            // The sword is out for an attack and a little after, and for the guard. A charge is made from the sheath.
            ShowSword(!rolling && !charging && !drinking && (guarding || sinceAttack < SheatheDelay));
        }

        private void ShowSword(bool drawn)
        {
            if (drawnSword != null && drawnSword.activeSelf != drawn) drawnSword.SetActive(drawn);
            if (sheathedHilt != null && sheathedHilt.activeSelf == drawn) sheathedHilt.SetActive(!drawn);
        }
    }
}

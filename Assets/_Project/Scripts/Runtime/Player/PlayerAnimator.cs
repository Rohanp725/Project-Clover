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

        public const string SpeedParameter = "Speed";
        public const string LocomotionState = "locomotion";
        public const string FistLocomotionState = "locomotion_fist";
        public const string DeathState = "death";
        public const string RollState = "roll";
        public const string ChargeState = "charge";
        public const string FistChargeState = "charge_fist";
        public const string HurtState = "hurt";
        public const string CastState = "cast";
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
                shownStep = null;
            }

            bool helpless = defence != null && defence.IsHelpless;
            bool rolling = !helpless && player.Dodge.IsRolling;
            bool charging = !helpless && !rolling && combat.IsCharging;
            bool attacking = !helpless && !rolling && !charging && combat.Combo.Current != null;
            bool casting = !helpless && !rolling && !charging && !attacking && element != null && element.IsCasting;
            bool drinking = !helpless && defence != null && defence.IsDrinking;
            bool guarding = !helpless && defence != null && defence.IsGuarding;

            int locomotion = weapon.UsesSword ? LocomotionHash : FistLocomotionHash;
            if (helpless && defence.IsDead)
            {
                animator.Play(DeathHash, 0, Mathf.Min(ClipEnd, defence.DeadSeconds / DeathSeconds));
            }
            else if (helpless)
            {
                animator.Play(HurtHash, 0, Mathf.Min(ClipEnd, defence.HurtProgress));
            }
            else if (rolling)
            {
                animator.Play(RollHash, 0, player.Dodge.Progress);
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
                animator.Play(CastHash, 0, Mathf.Min(ClipEnd, element.Kit.CastProgress));
            }
            else
            {
                if (wasBusy || locomotion != shownLocomotion) animator.CrossFadeInFixedTime(locomotion, ExitBlend);
                shownLocomotion = locomotion;
                float fraction = player.MoveSpeed > 0f ? Mathf.Clamp01(player.PlanarSpeed / player.MoveSpeed) : 0f;
                animator.SetFloat(SpeedHash, fraction, SpeedDampTime, deltaTime);
            }
            wasBusy = helpless || rolling || charging || attacking || casting;

            if (upperLayer >= 0)
            {
                guardFor = guarding ? guardFor + deltaTime : 0f;
                if (drinking) animator.Play(DrinkHash, upperLayer, Mathf.Min(ClipEnd, defence.Flasks.Progress));
                else if (guarding) animator.Play(weapon.UsesSword ? GuardHash : FistGuardHash, upperLayer, Mathf.Min(ClipEnd, guardFor / Mathf.Max(0.01f, UpperBodyBlend)));
                float target = drinking || guarding ? 1f : 0f;
                upperWeight = Mathf.MoveTowards(upperWeight, target, deltaTime / Mathf.Max(0.01f, UpperBodyBlend));
                animator.SetLayerWeight(upperLayer, upperWeight);
            }

            sinceAttack = attacking ? 0f : sinceAttack + deltaTime;
            // With the sword carried: out for an attack and a little after, and for the guard; a charge is made from
            // the sheath. With bare hands there is no sword on the character at all.
            bool drawn = weapon.UsesSword && !rolling && !charging && !drinking && !casting && (guarding || sinceAttack < SheatheDelay);
            Show(drawnSword, drawn);
            Show(sheathedHilt, weapon.UsesSword && !drawn);
            Show(sheath, weapon.UsesSword);
        }

        private static void Show(GameObject part, bool shown)
        {
            if (part != null && part.activeSelf != shown) part.SetActive(shown);
        }
    }
}

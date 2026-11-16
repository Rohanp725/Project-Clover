using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// Shows what the player is doing on the character model. Idle and run blend by speed. The roll and every attack are
    /// scrubbed by the game's own progress through them, so the animation always starts, lands and ends exactly when the
    /// rules say, whatever the timings are tuned to. Also moves the sword between sheath and hand.
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private Animator animator;
        [SerializeField] private GameObject drawnSword;
        [SerializeField] private GameObject sheathedHilt;

        /// <summary>Seconds the blend between idle and run takes to follow a change of speed.</summary>
        public float SpeedDampTime = 0.07f;
        /// <summary>Seconds to blend from the end of a roll or an attack back to moving.</summary>
        public float ExitBlend = 0.1f;
        /// <summary>Seconds after the last attack before the sword goes back in its sheath.</summary>
        public float SheatheDelay = 0.6f;

        public const string SpeedParameter = "Speed";
        public const string LocomotionState = "locomotion";
        public const string RollState = "roll";

        private static readonly int SpeedHash = Animator.StringToHash(SpeedParameter);
        private static readonly int LocomotionHash = Animator.StringToHash(LocomotionState);
        private static readonly int RollHash = Animator.StringToHash(RollState);

        private int[] attackHashes = new int[0];
        private bool wasBusy;
        private float sinceAttack = float.MaxValue;

        public void Configure(PlayerController playerToShow, PlayerCombat combatToShow, Animator animatorToDrive,
            GameObject drawnSwordObject, GameObject sheathedHiltObject)
        {
            player = playerToShow;
            combat = combatToShow;
            animator = animatorToDrive;
            drawnSword = drawnSwordObject;
            sheathedHilt = sheathedHiltObject;
        }

        private void Start()
        {
            if (combat != null)
            {
                attackHashes = new int[combat.Combo.Steps.Count];
                for (int i = 0; i < attackHashes.Length; i++) attackHashes[i] = Animator.StringToHash(combat.Combo.Steps[i].Name);
            }
            ShowSword(false);
        }

        private void LateUpdate()
        {
            if (player == null || animator == null) return;
            bool rolling = player.Dodge.IsRolling;
            bool attacking = !rolling && combat != null && combat.IsAttacking && combat.Combo.StepIndex < attackHashes.Length;

            if (rolling)
            {
                animator.Play(RollHash, 0, player.Dodge.Progress);
            }
            else if (attacking)
            {
                animator.Play(attackHashes[combat.Combo.StepIndex], 0, combat.Combo.StepProgress);
            }
            else
            {
                if (wasBusy) animator.CrossFadeInFixedTime(LocomotionHash, ExitBlend);
                float fraction = player.MoveSpeed > 0f ? Mathf.Clamp01(player.PlanarSpeed / player.MoveSpeed) : 0f;
                animator.SetFloat(SpeedHash, fraction, SpeedDampTime, Time.deltaTime);
            }
            wasBusy = rolling || attacking;

            sinceAttack = attacking ? 0f : sinceAttack + Time.deltaTime;
            ShowSword(sinceAttack < SheatheDelay && !rolling);
        }

        private void ShowSword(bool drawn)
        {
            if (drawnSword != null && drawnSword.activeSelf != drawn) drawnSword.SetActive(drawn);
            if (sheathedHilt != null && sheathedHilt.activeSelf == drawn) sheathedHilt.SetActive(!drawn);
        }
    }
}

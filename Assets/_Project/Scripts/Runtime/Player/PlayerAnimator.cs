using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// Shows what the player is doing on the character model: idle and run blended by speed, and the roll scrubbed by the
    /// roll's own progress so the tumble always starts and ends exactly with the dodge, whatever its duration is tuned to.
    /// </summary>
    public class PlayerAnimator : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private Animator animator;

        /// <summary>Seconds the blend between idle and run takes to follow a change of speed.</summary>
        public float SpeedDampTime = 0.07f;
        /// <summary>Seconds to blend from the end of the roll back to moving.</summary>
        public float RollExitBlend = 0.08f;

        public const string SpeedParameter = "Speed";
        public const string LocomotionState = "locomotion";
        public const string RollState = "roll";

        private static readonly int SpeedHash = Animator.StringToHash(SpeedParameter);
        private static readonly int LocomotionHash = Animator.StringToHash(LocomotionState);
        private static readonly int RollHash = Animator.StringToHash(RollState);

        private bool wasRolling;

        public void Configure(PlayerController playerToShow, Animator animatorToDrive)
        {
            player = playerToShow;
            animator = animatorToDrive;
        }

        private void LateUpdate()
        {
            if (player == null || animator == null) return;
            bool rolling = player.Dodge.IsRolling;
            if (rolling)
            {
                animator.Play(RollHash, 0, player.Dodge.Progress);
            }
            else
            {
                if (wasRolling) animator.CrossFadeInFixedTime(LocomotionHash, RollExitBlend);
                float fraction = player.MoveSpeed > 0f ? Mathf.Clamp01(player.PlanarSpeed / player.MoveSpeed) : 0f;
                animator.SetFloat(SpeedHash, fraction, SpeedDampTime, Time.deltaTime);
            }
            wasRolling = rolling;
        }
    }
}

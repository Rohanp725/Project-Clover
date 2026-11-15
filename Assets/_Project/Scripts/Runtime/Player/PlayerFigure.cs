using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// Animates the stand-in blocky figure from what the player is doing: a run cycle whose stride matches the ground covered,
    /// a forward lean, a tucked spin for the roll and a small squash on landing. It exists so movement can be judged before
    /// there is a real character with real animation; it changes nothing about how the player moves.
    /// </summary>
    public class PlayerFigure : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private Transform pivot;
        [SerializeField] private Transform leftArm;
        [SerializeField] private Transform rightArm;
        [SerializeField] private Transform leftLeg;
        [SerializeField] private Transform rightLeg;

        /// <summary>Metres covered by one full cycle of two steps.</summary>
        public float StrideLength = 2.2f;
        public float LegSwingDegrees = 42f;
        public float ArmSwingDegrees = 36f;
        public float RunLeanDegrees = 14f;
        public float RunBobHeight = 0.07f;
        /// <summary>Seconds the figure takes to settle into or out of the run pose.</summary>
        public float BlendTime = 0.08f;
        public float RollCrouchDepth = 0.35f;
        public float LandingSquash = 0.18f;
        public float LandingTime = 0.14f;

        private float pivotHeight;
        private float stridePhase;
        private float runAmount;
        private float runAmountVelocity;
        private float sinceLanding = float.MaxValue;
        private bool wasRolling;

        public void Configure(PlayerController playerToShow, Transform bodyPivot, Transform leftArmPivot, Transform rightArmPivot,
            Transform leftLegPivot, Transform rightLegPivot)
        {
            player = playerToShow;
            pivot = bodyPivot;
            leftArm = leftArmPivot;
            rightArm = rightArmPivot;
            leftLeg = leftLegPivot;
            rightLeg = rightLegPivot;
        }

        private void Awake()
        {
            if (pivot != null) pivotHeight = pivot.localPosition.y;
        }

        private void LateUpdate()
        {
            if (player == null || pivot == null) return;
            float deltaTime = Time.deltaTime;
            bool rolling = player.Dodge.IsRolling;
            if (wasRolling && !rolling) sinceLanding = 0f;
            wasRolling = rolling;
            sinceLanding += deltaTime;

            if (rolling) PoseRoll(player.Dodge.Progress);
            else PoseOnFoot(deltaTime);
        }

        private void PoseRoll(float progress)
        {
            // One full forward turn, low to the ground, limbs pulled in.
            float crouch = Mathf.Sin(progress * Mathf.PI) * RollCrouchDepth;
            pivot.localPosition = new Vector3(0f, pivotHeight - crouch, 0f);
            pivot.localRotation = Quaternion.Euler(360f * progress, 0f, 0f);
            pivot.localScale = Vector3.one * (1f - 0.12f * Mathf.Sin(progress * Mathf.PI));
            Limbs(-95f, -95f, -110f, -110f);
            runAmount = 1f;
        }

        private void PoseOnFoot(float deltaTime)
        {
            float speedFraction = player.MoveSpeed > 0f ? Mathf.Clamp01(player.PlanarSpeed / player.MoveSpeed) : 0f;
            runAmount = Mathf.SmoothDamp(runAmount, speedFraction, ref runAmountVelocity, BlendTime);
            stridePhase += player.PlanarSpeed * deltaTime / Mathf.Max(0.1f, StrideLength) * Mathf.PI * 2f;
            if (stridePhase > Mathf.PI * 2f) stridePhase -= Mathf.PI * 2f;

            float swing = Mathf.Sin(stridePhase) * runAmount;
            float bob = Mathf.Abs(Mathf.Cos(stridePhase)) * RunBobHeight * runAmount;
            float breath = Mathf.Sin(Time.time * 2.2f) * 0.012f * (1f - runAmount);
            float squash = sinceLanding < LandingTime ? LandingSquash * (1f - sinceLanding / LandingTime) : 0f;

            pivot.localPosition = new Vector3(0f, pivotHeight + bob + breath - squash * 0.5f, 0f);
            pivot.localRotation = Quaternion.Euler(RunLeanDegrees * runAmount, 0f, 0f);
            pivot.localScale = new Vector3(1f + squash * 0.5f, 1f - squash, 1f + squash * 0.5f);
            Limbs(-swing * ArmSwingDegrees, swing * ArmSwingDegrees, swing * LegSwingDegrees, -swing * LegSwingDegrees);
        }

        private void Limbs(float leftArmDegrees, float rightArmDegrees, float leftLegDegrees, float rightLegDegrees)
        {
            leftArm.localRotation = Quaternion.Euler(leftArmDegrees, 0f, 0f);
            rightArm.localRotation = Quaternion.Euler(rightArmDegrees, 0f, 0f);
            leftLeg.localRotation = Quaternion.Euler(leftLegDegrees, 0f, 0f);
            rightLeg.localRotation = Quaternion.Euler(rightLegDegrees, 0f, 0f);
        }
    }
}

using UnityEngine;

namespace Clover.Core
{
    /// <summary>
    /// Plays one-shot effects anywhere in the scene. The scene holds one particle emitter per <see cref="VfxKind"/>,
    /// simulated in world space; playing an effect moves that emitter to the spot and has it throw a burst, so nothing
    /// is created or destroyed during play. With no Vfx in the scene the calls do nothing.
    /// </summary>
    public class Vfx : MonoBehaviour
    {
        [SerializeField] private ParticleSystem[] emitters = new ParticleSystem[0];
        [SerializeField] private int[] counts = new int[0];

        private static Vfx instance;

        // Domain reload is off: the scene's Vfx from the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        /// <summary>The builder hands over one emitter and its burst size for each kind, indexed by the kind's number.</summary>
        public void Configure(ParticleSystem[] emittersByKind, int[] burstByKind)
        {
            emitters = emittersByKind;
            counts = burstByKind;
        }

        private void OnEnable() => instance = this;

        private void OnDisable()
        {
            if (instance == this) instance = null;
        }

        /// <summary>Throws the effect at a point. The direction turns effects that have one, such as a spray of sparks.</summary>
        public static void Play(VfxKind kind, Vector3 position, Vector3 direction, float scale = 1f)
        {
            if (instance == null) return;
            int index = (int)kind;
            if (index >= instance.emitters.Length) return;
            var emitter = instance.emitters[index];
            if (emitter == null) return;
            var emitterTransform = emitter.transform;
            emitterTransform.position = position;
            if (direction.sqrMagnitude > 0.0001f) emitterTransform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            emitter.Emit(Mathf.Max(1, Mathf.RoundToInt(instance.counts[index] * scale)));
        }

        public static void Play(VfxKind kind, Vector3 position) => Play(kind, position, Vector3.up);

        /// <summary>Throws the effect in a colour of the caller's choosing, as when a body breaks up into flakes of itself.</summary>
        public static void Play(VfxKind kind, Vector3 position, Vector3 direction, float scale, Color tint)
        {
            if (instance == null) return;
            int index = (int)kind;
            if (index >= instance.emitters.Length) return;
            var emitter = instance.emitters[index];
            if (emitter == null) return;
            var emitterTransform = emitter.transform;
            emitterTransform.position = position;
            if (direction.sqrMagnitude > 0.0001f) emitterTransform.rotation = Quaternion.LookRotation(direction, Vector3.up);
            var with = new ParticleSystem.EmitParams { startColor = tint };
            emitter.Emit(with, Mathf.Max(1, Mathf.RoundToInt(instance.counts[index] * scale)));
        }
    }
}

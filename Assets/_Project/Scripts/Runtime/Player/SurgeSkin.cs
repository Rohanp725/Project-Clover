using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// A surge the body's own toon material draws on itself (round 4, staging/blender/2026-11-22_round4): Fever's flush,
    /// the skin turning a strong sickly green-grey from the feet up in 0.4 s (the Blender worker's see-through shell
    /// rd_fever_skin, "a skin tint on the real model"), and Overdrive's charge, a faint cyan rim with the hair lifting over
    /// the crown. Clover/Toon is asked through the body's property block, read back first so the other surges' values
    /// stay, and written only when something changed, so it allocates nothing a frame.
    /// </summary>
    public sealed class SurgeSkin
    {
        /// <summary>Which of the two this one draws.</summary>
        public enum Kind { Fever, Charge }

        /// <summary>Seconds the flush takes to climb the body (the shell's five bands, 0.05 to 0.41 s), and to go when the surge ends.</summary>
        public const float Climb = 0.4f, TurnOff = 0.3f;

        public static readonly int FeverId = Shader.PropertyToID("_Fever");
        public static readonly int FeverRevealId = Shader.PropertyToID("_FeverReveal");
        public static readonly int ChargeId = Shader.PropertyToID("_Charge");
        private static readonly int BaseId = Shader.PropertyToID("_RevealBase");
        private static readonly int HeightId = Shader.PropertyToID("_RevealHeight");

        private readonly Kind kind;
        private readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        private Renderer body;
        private float level, age, writtenLevel = -1f, writtenReveal = -1f;

        public SurgeSkin(Kind kind) => this.kind = kind;

        /// <summary>How strongly it is drawn now (0 to 1), for probes.</summary>
        public float Level => level;

        public void Watch(Renderer bodyRenderer)
        {
            if (body == bodyRenderer) return;
            if (body != null) Write(0f, 1f, 0f, 0f);
            body = bodyRenderer;
            writtenLevel = -1f;
        }

        /// <summary>Moves it on or off over the frame's time, for a body standing at <paramref name="feet"/>.</summary>
        public void Tick(bool on, float deltaTime, float feet = 0f)
        {
            float reveal = 1f;
            if (on)
            {
                if (level <= 0f) age = 0f;
                age += deltaTime;
                level = 1f;
                if (kind == Kind.Fever) reveal = Mathf.Clamp01(age / Climb);
                // The charge comes on in a blink and crackles: never quite steady while it lasts.
                else level = 0.8f + 0.2f * Mathf.Sin(age * 23f);
            }
            else level = Mathf.MoveTowards(level, 0f, deltaTime / TurnOff);
            if (Mathf.Abs(level - writtenLevel) < 0.004f && Mathf.Abs(reveal - writtenReveal) < 0.004f && (level != 0f || writtenLevel == 0f)) return;
            float height = 1.85f;
            if (body != null) height = Mathf.Max(1f, body.bounds.max.y - feet);
            Write(level, reveal, feet, height);
            writtenLevel = level;
            writtenReveal = reveal;
        }

        /// <summary>Takes it off at once.</summary>
        public void Clear()
        {
            level = 0f;
            Write(0f, 1f, 0f, 0f);
            writtenLevel = 0f;
        }

        private void Write(float amount, float reveal, float feet, float height)
        {
            if (body == null) return;
            body.GetPropertyBlock(block);
            if (kind == Kind.Fever)
            {
                block.SetFloat(FeverId, amount);
                block.SetFloat(FeverRevealId, reveal);
            }
            else block.SetFloat(ChargeId, amount);
            // The height the flush and the lift count from; left as another surge set it when this one is off.
            if (amount > 0f && height > 0f)
            {
                block.SetFloat(BaseId, feet);
                block.SetFloat(HeightId, height);
            }
            body.SetPropertyBlock(block);
        }
    }
}

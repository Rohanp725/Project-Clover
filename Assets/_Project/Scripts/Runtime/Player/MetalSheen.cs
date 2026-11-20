using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// Temper, Metal's surge, seen as the player turned to steel (the owner's review of 2026-11-20: "a visible metallic
    /// sheen essentially turning metal yourself"): the body's toon material (Clover/Toon, which every outfit wears) is
    /// asked through a property block for its steel, a cool grey with a matcap-like banded shine, a hard specular spot and
    /// a cool rim. It comes on over a third of a second and goes over a little less when the surge ends. The block is
    /// read back and written only when the amount changes, so it costs nothing while it holds and allocates nothing.
    /// </summary>
    public sealed class MetalSheen
    {
        /// <summary>Seconds the steel takes to come over the body, and to go.</summary>
        public const float TurnOn = 0.35f, TurnOff = 0.3f;

        public static readonly int MetalId = Shader.PropertyToID("_Metal");

        private readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        private Renderer body;
        private float level, written;

        /// <summary>How far the body is turned to steel now, 0 to 1.</summary>
        public float Level => level;

        public void Watch(Renderer bodyRenderer)
        {
            if (body == bodyRenderer) return;
            if (body != null) Write(0f);
            body = bodyRenderer;
            written = -1f;
        }

        /// <summary>Moves the steel toward on or off over the frame's time.</summary>
        public void Tick(bool on, float deltaTime)
        {
            level = Mathf.MoveTowards(level, on ? 1f : 0f, deltaTime / (on ? TurnOn : TurnOff));
            float shown = Ease(level);
            // Written only when it has moved, and always once it is all gone, so no trace of steel is left.
            if (Mathf.Abs(shown - written) < 0.004f && (shown != 0f || written == 0f)) return;
            Write(shown);
            written = shown;
        }

        /// <summary>Takes the steel off at once.</summary>
        public void Clear()
        {
            level = 0f;
            Write(0f);
            written = 0f;
        }

        /// <summary>The steel's amount for a share of the way on: quick to take, settling as it holds.</summary>
        public static float Ease(float share) => 1f - (1f - share) * (1f - share);

        private void Write(float amount)
        {
            if (body == null) return;
            // Read back first, so whatever else the body's block carries is kept.
            body.GetPropertyBlock(block);
            block.SetFloat(MetalId, amount);
            body.SetPropertyBlock(block);
        }
    }
}

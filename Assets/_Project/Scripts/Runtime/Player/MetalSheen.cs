using Clover.Player.Forms;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// Temper, Metal's surge, seen as the player turned to steel (the owner's review of 2026-11-20: "a visible metallic
    /// sheen essentially turning metal yourself"): the body's toon material (Clover/Toon, which every outfit wears) is
    /// asked through a property block for its steel, a cool grey with a matcap-like banded shine, a hard specular spot and
    /// a cool rim. It climbs the body from the feet in five bands behind a bright front, in the Blender worker's timings
    /// (staging/blender/2026-11-20_elements, e2_metal_temper: bands at 0.25 s and every 0.11 s after, each 0.09 s, the
    /// front climbing from 0.22 s to 0.82 s), holds while the surge lasts, and goes over 0.3 s when it ends. The block is
    /// read back first and written only when something changed, so it allocates nothing.
    /// </summary>
    public sealed class MetalSheen
    {
        /// <summary>Seconds the steel takes to go when the surge ends.</summary>
        public const float TurnOff = 0.3f;
        /// <summary>The bright front: from when to when it climbs from the feet to the top of the head.</summary>
        public const float LineFrom = 0.22f, LineTo = 0.82f;

        public static readonly int MetalId = Shader.PropertyToID("_Metal");
        public static readonly int RevealId = Shader.PropertyToID("_MetalReveal");
        public static readonly int LineId = Shader.PropertyToID("_MetalLine");
        public static readonly int BaseId = Shader.PropertyToID("_RevealBase");
        public static readonly int HeightId = Shader.PropertyToID("_RevealHeight");

        /// <summary>The bands as the look-dev timed them, when the scene has no built look to read them from.</summary>
        public static readonly ElementLookSet.Look Timing = new ElementLookSet.Look
        {
            Shell = ElementLookSet.Steel, ShellBands = 5, ShellStart = 0.25f, ShellStep = 0.11f, ShellGrow = 0.09f,
        };

        private readonly MaterialPropertyBlock block = new MaterialPropertyBlock();
        private Renderer body;
        private float level, age, writtenLevel = -1f, writtenReveal = -1f, writtenLine = -2f;

        /// <summary>How far the body is turned to steel now (0 to 1): the share of it covered while it climbs, then fading as it goes.</summary>
        public float Level => level * reveal;
        private float reveal;

        public void Watch(Renderer bodyRenderer)
        {
            if (body == bodyRenderer) return;
            if (body != null) Write(0f, 1f, -1f, 0f, 0f);
            body = bodyRenderer;
            writtenLevel = -1f;
        }

        /// <summary>Moves the steel on or off over the frame's time, for a body standing at <paramref name="feet"/>.</summary>
        public void Tick(bool on, float deltaTime, float feet = 0f)
        {
            if (on)
            {
                if (level <= 0f) age = 0f;
                age += deltaTime;
                level = 1f;
                ElementLooks.TryLook(ElementLooks.Find("metal_temper"), out var look);
                if (look.Shell != ElementLookSet.Steel) look = Timing;
                reveal = ElementLookSet.ShellReveal(look, age);
            }
            else
            {
                // It goes whole, all of it at once, as the surge ends.
                level = Mathf.MoveTowards(level, 0f, deltaTime / TurnOff);
                reveal = level > 0f ? 1f : 0f;
            }
            float line = on && age >= LineFrom && age <= LineTo ? (age - LineFrom) / (LineTo - LineFrom) : -1f;
            if (Mathf.Abs(level - writtenLevel) < 0.004f && Mathf.Abs(reveal - writtenReveal) < 0.004f && Mathf.Abs(line - writtenLine) < 0.004f
                && (level != 0f || writtenLevel == 0f)) return;
            float height = 1.85f;
            if (body != null) height = Mathf.Max(1f, body.bounds.max.y - feet);
            Write(level, reveal, line, feet, height);
            writtenLevel = level;
            writtenReveal = reveal;
            writtenLine = line;
        }

        /// <summary>Takes the steel off at once.</summary>
        public void Clear()
        {
            level = 0f;
            reveal = 0f;
            Write(0f, 1f, -1f, 0f, 0f);
            writtenLevel = 0f;
        }

        private void Write(float amount, float covered, float line, float feet, float height)
        {
            if (body == null) return;
            // Read back first, so whatever else the body's block carries is kept.
            body.GetPropertyBlock(block);
            block.SetFloat(MetalId, amount);
            block.SetFloat(RevealId, covered);
            block.SetFloat(LineId, line);
            block.SetFloat(BaseId, feet);
            block.SetFloat(HeightId, height);
            body.SetPropertyBlock(block);
        }
    }
}

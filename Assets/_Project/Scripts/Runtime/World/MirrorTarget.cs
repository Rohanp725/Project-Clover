using Clover.Core;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// Something a beam from a <see cref="MirrorStand"/> can light: a sun-disc set over a door, a glyph that shows a
    /// hidden way. Held in the beam for <see cref="Needs"/> seconds it is lit for good: <see cref="Lit"/> is raised,
    /// whatever it was given to open (a door's blocking object) is put away, and whatever it was given to reveal is
    /// shown. Lit stays lit through rests and falls.
    /// </summary>
    public class MirrorTarget : MonoBehaviour
    {
        [SerializeField] private GameObject opens;
        [SerializeField] private GameObject reveals;
        [SerializeField] private Renderer glow;

        public float Needs = 1f;
        [ColorUsage(true, true)] public Color Dark = new Color(0.3f, 0.22f, 0.1f, 1f);
        [ColorUsage(true, true)] public Color Bright = new Color(4f, 3f, 1.4f, 1f);

        public bool IsLit { get; private set; }
        public float Charge { get; private set; }

        /// <summary>Raised once, when it is lit.</summary>
        public event System.Action<MirrorTarget> Lit;

        private static readonly int ColourId = Shader.PropertyToID("_Color");
        private MaterialPropertyBlock block;
        private float heldThisFrame;

        public void Configure(GameObject toOpen, GameObject toReveal, Renderer glowToShow)
        {
            opens = toOpen;
            reveals = toReveal;
            glow = glowToShow;
        }

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            if (reveals != null) reveals.SetActive(false);
            Paint(0f);
        }

        /// <summary>Called each frame a beam falls on it.</summary>
        public void Light(float deltaTime)
        {
            heldThisFrame = Time.frameCount;
            if (IsLit) return;
            Charge += deltaTime;
            Paint(Mathf.Clamp01(Charge / Needs));
            if (Charge < Needs) return;
            IsLit = true;
            if (opens != null) opens.SetActive(false);
            if (reveals != null) reveals.SetActive(true);
            Vfx.Play(VfxKind.Burst, transform.position, Vector3.up, 1.5f);
            Vfx.Play(VfxKind.TintRing, transform.position, Vector3.up, 1.4f, new Color(1f, 0.9f, 0.55f, 1f));
            Lit?.Invoke(this);
        }

        private void Update()
        {
            // Out of the beam it dims again until it is lit.
            if (IsLit || Charge <= 0f || heldThisFrame >= Time.frameCount - 1) return;
            Charge = Mathf.Max(0f, Charge - Time.deltaTime * 0.5f);
            Paint(Mathf.Clamp01(Charge / Needs));
        }

        private void Paint(float lit)
        {
            if (glow == null) return;
            block.Clear();
            block.SetColor(ColourId, Color.Lerp(Dark, Bright, IsLit ? 1f : lit));
            glow.SetPropertyBlock(block);
        }
    }
}

using Clover.Core;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The mark over a staggered fighter that says: finish me. A broken ring turning round a crossed cut, in white-gold,
    /// which lands with a burst the moment the stagger begins and is gone the instant it ends. Whoever owns it says each
    /// frame whether it should show; it does the rest, and does nothing at all while hidden.
    /// </summary>
    public class StaggerMark : MonoBehaviour
    {
        [SerializeField] private GameObject shapes;
        [SerializeField] private Transform ring;
        [SerializeField] private Transform cut;

        public float SpinDegreesPerSecond = 80f;
        /// <summary>Real seconds over which the mark drops from large to its own size when it appears.</summary>
        public float LandSeconds = 0.16f;
        public float LandFrom = 1.7f;
        public float BeatsPerSecond = 2.5f;
        public float BeatSwell = 0.16f;

        public bool IsShown { get; private set; }

        private Quaternion facing;
        private float age;
        private float spun;

        public void Configure(GameObject shapesRoot, Transform ringShape, Transform cutShape)
        {
            shapes = shapesRoot;
            ring = ringShape;
            cut = cutShape;
        }

        private void Awake()
        {
            // Built facing the camera, which never turns; the fighter under it does.
            facing = transform.rotation;
            if (shapes != null) shapes.SetActive(false);
            enabled = false;
        }

        /// <summary>Shows or hides the mark. Asking for what is already so costs nothing.</summary>
        public void Show(bool show)
        {
            if (show == IsShown) return;
            IsShown = show;
            enabled = show;
            if (shapes != null) shapes.SetActive(show);
            if (!show) return;
            age = 0f;
            spun = 0f;
            Vfx.Play(VfxKind.Burst, transform.position, Vector3.up, 0.45f);
            Place();
        }

        private void LateUpdate()
        {
            // Real time: the mark lands and turns through the stops and slows that blows bring.
            age += Time.unscaledDeltaTime;
            spun += SpinDegreesPerSecond * Time.unscaledDeltaTime;
            Place();
        }

        private void Place()
        {
            transform.rotation = facing;
            float landed = Mathf.Clamp01(age / LandSeconds);
            float size = Mathf.Lerp(LandFrom, 1f, landed * (2f - landed));
            // The beat is a quick swell and a slow settle, like a pulse, not a sine.
            float beat = 1f - Mathf.Repeat(age * BeatsPerSecond, 1f);
            float swell = 1f + BeatSwell * beat * beat;
            if (ring != null)
            {
                ring.localRotation = Quaternion.Euler(0f, 0f, spun);
                ring.localScale = new Vector3(size, size, 1f);
            }
            if (cut != null) cut.localScale = new Vector3(size * swell, size * swell, 1f);
        }
    }
}

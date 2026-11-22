using Clover.Core;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The mark over a staggered fighter that says: finish me. A gold crossed cut pulsing over its head (round 4, the
    /// owner: "drop the ring"; it was a dashed ring round the cross over the body, drawn over the player in close fights),
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
        public float BeatSwell = 0.3f;
        /// <summary>Over an enemy: this far above its bars, at this share of the size it was built at.</summary>
        public float OverBars = 0.55f, OverHeadSize = 0.65f;

        public bool IsShown { get; private set; }

        private static readonly Color LandingGold = new Color(1f, 0.72f, 0.2f);

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
            // No ring: the cross alone.
            if (ring != null) ring.gameObject.SetActive(false);
            placedOverHead = false;
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
            PlaceOverHead();
            // The landing burst in the mark's gold, held below white: untinted it was a pure-white disc behind the mark on
            // every staggered enemy (the swing films of 2026-11-20). 0.22 is the brightness a heavy blow's star is lit to.
            Vfx.Play(VfxKind.Burst, transform.position, Vector3.up, 0.45f, StruckLook.Glow(LandingGold, 0.22f));
            Place();
        }

        private bool placedOverHead;

        // Over an enemy's head, clear of its bars (a god's mark keeps the place its builder gave it).
        private void PlaceOverHead()
        {
            if (placedOverHead) return;
            placedOverHead = true;
            var enemy = transform.parent != null ? transform.parent.GetComponent<Enemy>() : null;
            if (enemy == null || enemy.Kind == null || enemy.Kind.IsBoss) return;
            transform.localPosition = new Vector3(0f, enemy.BodyHeight + 0.3f + OverBars, 0f);
            transform.localScale *= OverHeadSize;
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

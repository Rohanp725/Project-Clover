using Clover.Core;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// A tall clay water jar standing about the river and the city. Any blow breaks it (the player's, a burst, a stray
    /// sweep): the clay flies, the water runs out dark on the mud, and for a while any thirsting near enough comes to
    /// drink it (<see cref="ThirstDrinker"/>). It stands whole again when the player rests or falls.
    /// </summary>
    public class WaterJar : MonoBehaviour, IDamageable
    {
        [SerializeField] private GameObject whole;
        [SerializeField] private Transform spill;
        [SerializeField] private AreaDirector director;

        public float WetSeconds = 30f;
        public Color ClayColour = new Color(0.55f, 0.33f, 0.24f, 1f);
        public Color WaterColour = new Color(0.42f, 0.6f, 0.78f, 1f);

        public bool Broken { get; private set; }
        public int Breaks { get; private set; }

        private Collider solid;
        private float wetLeft;
        private Vector3 spillScale;

        public void Configure(GameObject jar, Transform spillShape, AreaDirector areaDirector)
        {
            whole = jar;
            spill = spillShape;
            director = areaDirector;
        }

        private void Awake()
        {
            solid = GetComponent<Collider>();
            if (spill != null)
            {
                spillScale = spill.localScale;
                spill.gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (director != null) director.Rested += Mend;
        }

        private void OnDisable()
        {
            if (director != null) director.Rested -= Mend;
        }

        public HitOutcome TakeHit(in Hit hit)
        {
            if (Broken) return HitOutcome.Missed;
            Break();
            return HitOutcome.Landed;
        }

        /// <summary>Breaks it where it stands: clay and water thrown out, and a spill left to drink.</summary>
        public void Break()
        {
            if (Broken) return;
            Broken = true;
            Breaks++;
            Vector3 at = transform.position;
            if (whole != null) whole.SetActive(false);
            if (solid != null) solid.enabled = false;
            Vfx.Play(VfxKind.TintSpark, at + Vector3.up * 0.7f, Vector3.up, 1.4f, WaterColour);
            Vfx.Play(VfxKind.Ash, at + Vector3.up * 0.6f, Vector3.up, 2f, ClayColour);
            Vfx.Play(VfxKind.TintRing, at + Vector3.up * 0.1f, Vector3.up, 0.6f, WaterColour);
            Vfx.Play(VfxKind.Dust, at, Vector3.up, 1f);
            if (spill != null)
            {
                spill.gameObject.SetActive(true);
                spill.localScale = spillScale * 0.3f;
            }
            wetLeft = WetSeconds;
            Waters.Add(at, WetSeconds, this);
        }

        /// <summary>Whole again, as after a rest.</summary>
        public void Mend()
        {
            Broken = false;
            wetLeft = 0f;
            if (whole != null) whole.SetActive(true);
            if (solid != null) solid.enabled = true;
            if (spill != null) spill.gameObject.SetActive(false);
        }

        private void Update()
        {
            if (!Broken || spill == null || !spill.gameObject.activeSelf) return;
            wetLeft -= Time.deltaTime;
            // The water runs out over a second, then soaks away.
            float spread = Mathf.Clamp01((WetSeconds - wetLeft) / 1f);
            float dry = Mathf.Clamp01(wetLeft / 4f);
            spill.localScale = spillScale * Mathf.Lerp(0.3f, 1f, spread) * Mathf.Lerp(0.4f, 1f, dry);
            if (wetLeft <= 0f) spill.gameObject.SetActive(false);
        }
    }
}

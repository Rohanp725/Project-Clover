using System.Collections.Generic;
using Clover.Core;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The wick's spreading flame (fire_rework.md, section 1). A wick set alight (by a burst, a brazier, another wick)
    /// wakes if it lay dormant, burns from head and hands, moves faster, and leaves a short trail of burning footprints
    /// that bite whoever steps in them. A burning wick that comes within touching distance of a dormant or unlit one
    /// lights it too. A wick placed already burning starts lit.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class WickFlame : MonoBehaviour
    {
        [SerializeField] private bool startsLit;
        [SerializeField] private Light flameLight;

        public float LitPace = 1.4f;
        /// <summary>Metres between a burning wick and another it sets alight.</summary>
        public float SpreadDistance = 1.6f;
        public float FootprintEvery = 0.35f;
        public float FootprintSeconds = 2.2f;

        public bool IsLit { get; private set; }
        public int Spread { get; private set; }

        private static readonly List<WickFlame> all = new List<WickFlame>();

        // Domain reload is off: the last play's wicks are not this one's.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => all.Clear();

        private Enemy enemy;
        private float untilFlame, untilPrint, untilSpread;
        private bool leftFoot;
        private Vector3 lastPrint;

        public void Configure(bool burningFromTheStart, Light lamp)
        {
            startsLit = burningFromTheStart;
            flameLight = lamp;
        }

        private void Awake() => enemy = GetComponent<Enemy>();

        private void OnEnable()
        {
            all.Add(this);
            enemy.Restored += OnRestored;
        }

        private void OnDisable()
        {
            all.Remove(this);
            enemy.Restored -= OnRestored;
        }

        private void Start()
        {
            if (startsLit) SetLit(true);
            else if (flameLight != null) flameLight.enabled = false;
        }

        private void OnRestored(Enemy who) => SetLit(startsLit);

        /// <summary>Sets it alight and wakes it. False if it was already burning or is dead.</summary>
        public bool Light()
        {
            if (IsLit || enemy.IsDead) return false;
            SetLit(true);
            enemy.WakeNow();
            Vfx.Play(VfxKind.Fire, transform.position + Vector3.up * 1.4f, Vector3.up, 1.4f);
            Vfx.Play(VfxKind.Embers, transform.position + Vector3.up * 1.4f, Vector3.up, 1.2f);
            return true;
        }

        /// <summary>Lights every unlit wick standing within the radius of a point. Returns how many caught.</summary>
        public static int LightWithin(Vector3 point, float radius)
        {
            int lit = 0;
            for (int i = all.Count - 1; i >= 0; i--)
            {
                var wick = all[i];
                Vector3 offset = wick.transform.position - point;
                offset.y = 0f;
                if (offset.sqrMagnitude <= radius * radius && wick.Light()) lit++;
            }
            return lit;
        }

        private void SetLit(bool lit)
        {
            IsLit = lit;
            enemy.FlamePace = lit ? LitPace : 1f;
            if (flameLight != null) flameLight.enabled = lit;
            lastPrint = transform.position;
        }

        private void Update()
        {
            if (!IsLit) return;
            if (enemy.IsDead)
            {
                if (flameLight != null) flameLight.enabled = false;
                return;
            }
            float deltaTime = Time.deltaTime;
            Vector3 at = transform.position;
            untilFlame -= deltaTime;
            if (untilFlame <= 0f)
            {
                // Burning from the head and both hands, as a wick does.
                untilFlame = 0.09f;
                Vfx.Play(VfxKind.Fire, at + Vector3.up * 1.75f + transform.right * Random.Range(-0.1f, 0.1f), Vector3.up, 0.22f);
                Vfx.Play(VfxKind.Fire, at + Vector3.up * 1.05f + transform.right * (Random.value < 0.5f ? -0.55f : 0.55f), Vector3.up, 0.14f);
                if (Random.value < 0.4f) Vfx.Play(VfxKind.Embers, at + Vector3.up * 1.8f, Vector3.up, 0.25f);
            }
            if (flameLight != null) flameLight.intensity = 2.6f + 0.8f * Mathf.PerlinNoise(Time.time * 7f, 0.3f);
            // Burning footprints where it has walked.
            untilPrint -= deltaTime;
            Vector3 walked = at - lastPrint;
            walked.y = 0f;
            if (untilPrint <= 0f && walked.sqrMagnitude > 0.25f)
            {
                untilPrint = FootprintEvery;
                lastPrint = at;
                leftFoot = !leftFoot;
                Vector3 foot = at + transform.right * (leftFoot ? -0.14f : 0.14f);
                MoltenFloor.Footprint(foot);
            }
            // The flame spreads to any wick it touches.
            untilSpread -= deltaTime;
            if (untilSpread > 0f) return;
            untilSpread = 0.2f;
            for (int i = all.Count - 1; i >= 0; i--)
            {
                var other = all[i];
                if (other == this || other.IsLit) continue;
                Vector3 offset = other.transform.position - at;
                offset.y = 0f;
                if (offset.sqrMagnitude <= SpreadDistance * SpreadDistance && other.Light()) Spread++;
            }
        }
    }
}

using Clover.Core;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The thirsting drink the dead (fire_rework.md, section 1): one near a fallen body or a broken water jar's spill
    /// leaves the fight, goes to it and drinks, and rises healed and swollen with it. In the noon's swell they go
    /// wild for water and smell it from twice as far. Breaking a jar near one is bait.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class ThirstDrinker : MonoBehaviour
    {
        [SerializeField] private Transform model;
        [SerializeField] private Enemy[] others = new Enemy[0];

        public float SeekRange = 8f;
        public float DrinkSeconds = 1.4f;
        /// <summary>The share of its health a drink gives back, and the share its most grows by.</summary>
        public float Heals = 0.6f;
        public float Swells = 0.25f;
        /// <summary>How long a body can be drunk from after it falls.</summary>
        public float BodySeconds = 14f;
        public float RunSpeed = 4.6f;

        public int Drinks { get; private set; }
        public bool Seeking => seeking >= 0;

        private Enemy enemy;
        private int seeking = -1;
        private bool sipping;
        private float untilLook;
        private Vector3 baseScale;
        private int swollen;

        public void Configure(Transform bodyModel, Enemy[] fallen)
        {
            model = bodyModel;
            others = fallen ?? new Enemy[0];
        }

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            if (model != null) baseScale = model.localScale;
        }

        private void OnEnable()
        {
            foreach (var other in others)
                if (other != null && other != enemy) other.Died += OnFallen;
            enemy.Restored += OnRestored;
        }

        private void OnDisable()
        {
            foreach (var other in others)
                if (other != null && other != enemy) other.Died -= OnFallen;
            enemy.Restored -= OnRestored;
        }

        private void OnFallen(Enemy who)
        {
            // A thirsting leaves nothing to drink: it crumbles to dust.
            if (who.Kind != null && who.Kind.Prefix == "thirst") return;
            Waters.Add(who.transform.position, BodySeconds, who);
        }

        private void OnRestored(Enemy who)
        {
            seeking = -1;
            sipping = false;
            swollen = 0;
            if (model != null) model.localScale = baseScale;
        }

        private void Update()
        {
            if (enemy.IsDead || enemy.IsDormant)
            {
                if (seeking >= 0) enemy.ClearSteer();
                seeking = -1;
                return;
            }
            if (sipping)
            {
                if (enemy.IsSipping)
                {
                    if (Random.value < 0.25f) Vfx.Play(VfxKind.TintPuff, transform.position + transform.forward * 0.6f + Vector3.up * 0.4f, Vector3.up, 0.3f, new Color(0.45f, 0.6f, 0.75f, 1f));
                    return;
                }
                // The drink is down: healed, and swollen with it.
                sipping = false;
                Drinks++;
                swollen++;
                enemy.Heal(enemy.Health.Maximum * Heals, Swells);
                if (model != null) model.localScale = baseScale * (1f + 0.08f * Mathf.Min(3, swollen));
                Vfx.Play(VfxKind.Heal, transform.position + Vector3.up * 0.8f, Vector3.up, 0.8f);
                Vfx.Play(VfxKind.TintRing, transform.position + Vector3.up * 0.3f, Vector3.up, 0.6f, new Color(0.5f, 0.7f, 0.9f, 1f));
            }
            untilLook -= Time.deltaTime;
            if (untilLook > 0f) return;
            untilLook = 0.2f;
            if (enemy.IsStriking || enemy.IsHolding || enemy.IsDrinking) return;
            float reach = SeekRange * (Clover.World.NoonHeat.Swelling ? 2f : 1f);
            if (!Waters.Nearest(transform.position, reach, out Vector3 water, out int index))
            {
                if (seeking >= 0) enemy.ClearSteer();
                seeking = -1;
                return;
            }
            seeking = index;
            Vector3 offset = water - transform.position;
            offset.y = 0f;
            if (offset.magnitude > 0.8f)
            {
                enemy.Steer(water, RunSpeed);
                return;
            }
            enemy.ClearSteer();
            seeking = -1;
            if (Waters.Drink(index) && enemy.Sip(DrinkSeconds))
            {
                sipping = true;
                Vfx.Play(VfxKind.Dust, water, Vector3.up);
            }
        }
    }
}

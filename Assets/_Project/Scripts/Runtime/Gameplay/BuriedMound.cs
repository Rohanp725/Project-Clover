using Clover.Core;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The thirsting under the mud (fire_rework.md, section 1): placed dormant, it lies buried as a low mound whose
    /// cracks breathe dust, the tell for a player who reads the ground. Someone near, it bursts up in a spray of dust
    /// and lunges for the belt (the snatch while two flasks are carried, the pounce otherwise).
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class BuriedMound : MonoBehaviour
    {
        [SerializeField] private Transform mound;
        [SerializeField] private Transform model;

        public Color DustColour = new Color(0.68f, 0.6f, 0.47f, 1f);
        public float BreathEvery = 1.7f;

        public int Bursts { get; private set; }
        public bool Buried => buried;

        private Enemy enemy;
        private Renderer[] body;
        private bool buried;
        private bool lungeOwed;
        private float breath;
        private Vector3 moundScale;

        public void Configure(Transform moundShape, Transform bodyModel)
        {
            mound = moundShape;
            model = bodyModel;
        }

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            body = model != null ? model.GetComponentsInChildren<Renderer>(true) : new Renderer[0];
            if (mound != null) moundScale = mound.localScale;
            breath = Random.value * BreathEvery;
        }

        private void OnEnable()
        {
            enemy.Woke += OnWoke;
            enemy.Restored += OnRestored;
        }

        private void OnDisable()
        {
            enemy.Woke -= OnWoke;
            enemy.Restored -= OnRestored;
        }

        private void Start() => Bury(enemy.IsDormant);

        private void OnRestored(Enemy who) => Bury(enemy.IsDormant);

        private void Bury(bool under)
        {
            buried = under;
            foreach (var bodyRenderer in body)
                if (bodyRenderer != null) bodyRenderer.enabled = !under;
            if (mound != null) mound.gameObject.SetActive(under);
        }

        private void OnWoke(Enemy who)
        {
            if (!buried) return;
            Bury(false);
            Bursts++;
            lungeOwed = true;
            // Up out of the mud: a spray of dust and clods thrown outward, and the cracked crust flung off.
            Vector3 at = transform.position;
            Vfx.Play(VfxKind.Dust, at, Vector3.up, 2.5f);
            Vfx.Play(VfxKind.TintSpark, at + Vector3.up * 0.3f, Vector3.up, 1.6f, DustColour);
            Vfx.Play(VfxKind.TintRing, at + Vector3.up * 0.15f, Vector3.up, 0.9f, DustColour);
            for (int i = 0; i < 8; i++)
            {
                Vector2 spot = Random.insideUnitCircle * 0.9f;
                Vfx.Play(VfxKind.Ash, at + new Vector3(spot.x, 0.25f, spot.y), Vector3.up, 1.2f, DustColour * 0.8f);
                Vfx.Play(VfxKind.TintPuff, at + new Vector3(spot.x, 0.4f, spot.y), Vector3.up, 0.9f, DustColour);
            }
        }

        private void Update()
        {
            if (buried)
            {
                // The cracks breathe: the mound rises a little and a thin puff of dust comes out of it.
                breath += Time.deltaTime;
                float phase = Mathf.Repeat(breath / BreathEvery, 1f);
                if (mound != null) mound.localScale = new Vector3(moundScale.x, moundScale.y * (1f + 0.12f * Mathf.Sin(phase * Mathf.PI * 2f)), moundScale.z);
                if (breath >= BreathEvery)
                {
                    breath -= BreathEvery;
                    Vector2 crack = Random.insideUnitCircle * 0.35f;
                    Vfx.Play(VfxKind.TintPuff, transform.position + new Vector3(crack.x, 0.35f, crack.y), Vector3.up, 0.7f, DustColour);
                    Vfx.Play(VfxKind.Dust, transform.position + new Vector3(crack.x, 0.1f, crack.y), Vector3.up, 0.4f);
                }
                return;
            }
            if (!lungeOwed || enemy.IsDead) return;
            if (enemy.IsRising || enemy.Brain.State != EnemyState.Stalk) return;
            lungeOwed = false;
            var flasks = enemy.Brain.TargetFlasks;
            if (!(flasks >= 2 && enemy.Brain.Force(enemy.Kind.SnatchAttack))) enemy.Brain.Force("thirst_pounce");
        }
    }
}

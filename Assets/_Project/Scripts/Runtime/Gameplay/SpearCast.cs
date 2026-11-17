using Clover.Core;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The gilded spearman's throw (fire_rework.md, section 1): on level ground he casts his spear down a red lane, and
    /// it stands glowing in the floor where the lane ends (or where it struck someone), smoking. Until he has walked to
    /// it and pulled it out he has no spear: no guard, and he goes for it before anything else.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class SpearCast : MonoBehaviour
    {
        [SerializeField] private Transform standing;
        [SerializeField] private Light glow;

        public float PickUpWithin = 1.1f;

        public bool Thrown { get; private set; }
        public int Throws { get; private set; }
        public int Fetched { get; private set; }
        public Vector3 StandsAt => standing != null ? standing.position : transform.position;

        private Enemy enemy;
        private float untilSmoke;

        public void Configure(Transform spearInFloor, Light spearGlow)
        {
            standing = spearInFloor;
            glow = spearGlow;
        }

        private void Awake()
        {
            enemy = GetComponent<Enemy>();
            Show(false);
        }

        private void OnEnable()
        {
            enemy.AttackStruck += OnAttackStruck;
            enemy.Restored += OnRestored;
        }

        private void OnDisable()
        {
            enemy.AttackStruck -= OnAttackStruck;
            enemy.Restored -= OnRestored;
        }

        private void OnRestored(Enemy who)
        {
            Thrown = false;
            Show(false);
        }

        private void Show(bool shown)
        {
            if (standing != null)
            {
                standing.gameObject.SetActive(shown);
                // The spear is its own thing in the world once thrown, not a part of him.
                if (shown) standing.SetParent(null, true);
            }
        }

        private void OnAttackStruck(Enemy who, EnemyAttack attack)
        {
            if (!attack.Casts || Thrown || standing == null) return;
            Thrown = true;
            Throws++;
            Vector3 along = transform.forward;
            Vector3 end = transform.position + along * attack.Reach;
            // It stops in whoever it hit, or in the first wall.
            Vector3 toTarget = enemy.Target != null ? enemy.Target.position - transform.position : Vector3.zero;
            toTarget.y = 0f;
            float ahead = Vector3.Dot(toTarget, along);
            if (ahead > 0f && ahead < attack.Reach && (toTarget - along * ahead).magnitude < attack.LineWidth * 0.5f + 0.4f) end = transform.position + along * ahead;
            if (Physics.Raycast(transform.position + Vector3.up * 1.2f, along, out RaycastHit wall, attack.Reach, ~0, QueryTriggerInteraction.Ignore)
                && wall.collider.GetComponentInParent<Enemy>() == null && wall.collider.GetComponentInParent<Clover.Player.PlayerDefence>() == null)
                end = transform.position + along * Mathf.Max(0.5f, wall.distance - 0.4f);
            end.y = transform.position.y;
            Show(true);
            // Standing in the floor, leaning back toward the one who threw it.
            standing.SetPositionAndRotation(end, Quaternion.LookRotation(along, Vector3.up) * Quaternion.Euler(-24f, 0f, 0f));
            Vfx.Play(VfxKind.Dust, end, Vector3.up, 1.2f);
            Vfx.Play(VfxKind.TintSpark, end + Vector3.up * 0.3f, -along, 0.8f, new Color(1f, 0.75f, 0.3f, 1f));
            Vfx.Play(VfxKind.Fire, end + Vector3.up * 0.4f, Vector3.up, 0.6f);
            for (int i = 1; i <= 5; i++) Vfx.Play(VfxKind.Embers, Vector3.Lerp(transform.position, end, i / 6f) + Vector3.up * 1.2f, along, 0.3f);
        }

        private void Update()
        {
            if (!Thrown) return;
            if (enemy.IsDead)
            {
                // Dead with his spear in the floor: it stays there until the area is restored.
                return;
            }
            untilSmoke -= Time.deltaTime;
            if (untilSmoke <= 0f)
            {
                untilSmoke = 0.18f;
                Vfx.Play(VfxKind.TintPuff, standing.position + Vector3.up * 0.5f, Vector3.up, 0.35f, new Color(0.5f, 0.45f, 0.42f, 1f));
                if (Random.value < 0.4f) Vfx.Play(VfxKind.Embers, standing.position + Vector3.up * 0.9f, Vector3.up, 0.2f);
            }
            if (glow != null) glow.intensity = 2.2f + 0.6f * Mathf.Sin(Time.time * 9f);
            // Bare-handed he has no guard, and he goes for the spear.
            enemy.GuardOpenFor = 0.2f;
            Vector3 offset = standing.position - transform.position;
            offset.y = 0f;
            if (offset.magnitude > PickUpWithin)
            {
                enemy.Steer(standing.position, enemy.Kind.WalkSpeed * 1.3f);
                return;
            }
            enemy.ClearSteer();
            Thrown = false;
            Fetched++;
            Show(false);
            standing.SetParent(transform, true);
            Vfx.Play(VfxKind.Dust, standing.position, Vector3.up, 0.8f);
        }
    }
}

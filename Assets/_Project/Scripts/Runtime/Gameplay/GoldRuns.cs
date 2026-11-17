using Clover.Core;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The gilded's running gold (fire_rework.md, section 1): every blow that lands on one splashes molten gold onto
    /// the floor beside him, which cools into a puddle (see <see cref="MoltenFloor"/>); in the noon's swell the gold
    /// runs from him on its own. The warden's pour is laid here when his pouring attack falls.
    /// </summary>
    [RequireComponent(typeof(Enemy))]
    public class GoldRuns : MonoBehaviour
    {
        public float SplashSize = 0.6f;
        /// <summary>Seconds between drips while the sun swells.</summary>
        public float SwellDripEvery = 1.1f;

        public int Splashed { get; private set; }
        public int Pours { get; private set; }

        private Enemy enemy;
        private float untilDrip;

        private void Awake() => enemy = GetComponent<Enemy>();

        private void OnEnable()
        {
            enemy.Struck += OnStruck;
            enemy.AttackStruck += OnAttackStruck;
        }

        private void OnDisable()
        {
            enemy.Struck -= OnStruck;
            enemy.AttackStruck -= OnAttackStruck;
        }

        private void OnStruck(Enemy who, Hit hit, HitOutcome outcome)
        {
            if (outcome == HitOutcome.Missed || outcome == HitOutcome.Dodged || hit.Damage <= 0f) return;
            // The gold is thrown off the side the blow drove it from, a little behind the line of the blow.
            Vector3 along = hit.Direction;
            along.y = 0f;
            along = along.sqrMagnitude > 0.01f ? along.normalized : -transform.forward;
            Vector3 side = Vector3.Cross(Vector3.up, along) * Random.Range(-0.7f, 0.7f);
            Vector3 at = transform.position + along * Random.Range(0.6f, 1.1f) + side;
            MoltenFloor.Splash(at, SplashSize * (hit.Heavy ? 1.3f : 1f));
            Vfx.Play(VfxKind.TintPuff, transform.position + Vector3.up * 1.2f, along, 0.5f, new Color(1f, 0.8f, 0.3f, 1f));
            Splashed++;
        }

        private void OnAttackStruck(Enemy who, EnemyAttack attack)
        {
            if (!attack.Pours) return;
            Pours++;
            MoltenFloor.PourArc(transform.position, transform.forward, attack.Reach, attack.ArcDegrees);
        }

        private void Update()
        {
            if (enemy.IsDead || !enemy.IsAware || !Clover.World.NoonHeat.Swelling) return;
            untilDrip -= Time.deltaTime;
            if (untilDrip > 0f) return;
            untilDrip = SwellDripEvery * Random.Range(0.7f, 1.3f);
            Vector2 spot = Random.insideUnitCircle * 0.5f;
            MoltenFloor.Splash(transform.position + new Vector3(spot.x, 0f, spot.y), SplashSize * 0.8f);
            Splashed++;
        }
    }
}

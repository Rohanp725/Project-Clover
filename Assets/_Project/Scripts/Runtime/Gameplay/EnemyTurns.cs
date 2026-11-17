using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// Makes a group of enemies fight like duellists and not like a mob: only so many may be attacking at once, and
    /// after one begins, the others wait a moment before the next may. The rest hold a ring around the fight and circle.
    /// A fight against three is then three duels with a threat at the player's back, not three blows landing together.
    /// </summary>
    public class EnemyTurns : MonoBehaviour
    {
        [SerializeField] private Enemy[] enemies = new Enemy[0];

        /// <summary>How many enemies may be showing or striking a hand-to-hand attack at once, and how many a thrown one.</summary>
        public int MeleeAtOnce = 1;
        public int RangedAtOnce = 1;
        /// <summary>Seconds after any hand-to-hand attack begins before another enemy may begin one.</summary>
        public float GapBetweenAttacks = 1.1f;

        private float sinceMeleeBegan = float.MaxValue;

        public void Configure(Enemy[] enemiesHere) => enemies = enemiesHere;

        private void Update()
        {
            if (sinceMeleeBegan < float.MaxValue) sinceMeleeBegan += Time.deltaTime;
        }

        /// <summary>Whether this enemy may begin an attack now.</summary>
        public bool MayAttack(Enemy asking)
        {
            bool ranged = asking.IsRanged;
            if (!ranged && sinceMeleeBegan < GapBetweenAttacks) return false;
            int busy = 0;
            for (int i = 0; i < enemies.Length; i++)
            {
                var other = enemies[i];
                if (other == null || other == asking || other.IsDead || other.IsRanged != ranged) continue;
                if (other.IsStriking) busy++;
            }
            return busy < (ranged ? RangedAtOnce : MeleeAtOnce);
        }

        private int drinkAnswered = -1;
        private Enemy drinkAnswerer;

        /// <summary>
        /// The player has started a drink (their count of drinks is <paramref name="drink"/>): only the nearest of the
        /// thirsting that can answer it does. True for that one, once.
        /// </summary>
        public bool AnswersDrink(Enemy asking, int drink, Vector3 drinker)
        {
            if (drink != drinkAnswered)
            {
                drinkAnswered = drink;
                drinkAnswerer = null;
                float nearest = float.MaxValue;
                for (int i = 0; i < enemies.Length; i++)
                {
                    var other = enemies[i];
                    if (other == null || !other.CanAnswerDrink) continue;
                    float apart = (other.transform.position - drinker).sqrMagnitude;
                    if (apart >= nearest) continue;
                    nearest = apart;
                    drinkAnswerer = other;
                }
            }
            return drinkAnswerer == asking;
        }

        /// <summary>True while any enemy here has a flask of the player's in its hands: only one may at a time.</summary>
        public bool AnyHoldsFlask()
        {
            for (int i = 0; i < enemies.Length; i++)
                if (enemies[i] != null && enemies[i].HoldsFlask) return true;
            return false;
        }

        /// <summary>An enemy has begun an attack.</summary>
        public void Began(Enemy enemy)
        {
            if (!enemy.IsRanged) sinceMeleeBegan = 0f;
        }
    }
}

using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// The storm striking the castle itself: every so often a bolt comes down on one of the high points the builder
    /// marked (tower tops, spires). It is scenery, but it is what tells the player where they are.
    /// </summary>
    public class StormRods : MonoBehaviour
    {
        [SerializeField] private Transform[] rods = new Transform[0];

        public float MinSecondsBetween = 2.5f;
        public float MaxSecondsBetween = 6.5f;

        private float untilNext;

        public void Configure(Transform[] highPoints) => rods = highPoints;

        private void OnEnable() => untilNext = Random.Range(1f, MinSecondsBetween);

        private void Update()
        {
            untilNext -= Time.deltaTime;
            if (untilNext > 0f || rods.Length == 0) return;
            untilNext = Random.Range(MinSecondsBetween, MaxSecondsBetween);
            var rod = rods[Random.Range(0, rods.Length)];
            if (rod != null) LightningBolts.Strike(rod.position, Random.Range(0.5f, 1f));
        }
    }
}

using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// A training dummy that always carries one element's affliction, laid again to full depth every few seconds, so
    /// what each affliction does, and what a mark does to an enemy carrying it, can be tried on the Proving Ground
    /// without first laying it. A frozen dummy is let thaw before it is chilled again.
    /// </summary>
    [RequireComponent(typeof(TrainingDummy))]
    public class AfflictedDummy : MonoBehaviour
    {
        public AfflictionKind Kind = AfflictionKind.Burn;
        /// <summary>Seconds between layings.</summary>
        public float Every = 3f;

        public int Laid { get; private set; }

        private TrainingDummy dummy;
        private float untilNext;

        private void Awake() => dummy = GetComponent<TrainingDummy>();

        private void Update()
        {
            untilNext -= Time.deltaTime;
            if (untilNext > 0f || dummy == null || Kind == AfflictionKind.None) return;
            untilNext = Every;
            if (dummy.Afflictions.IsFrozen) return;
            Laid++;
            // Chill is laid one short of freezing, so a blow of Ice is what freezes it.
            int depth = Kind == AfflictionKind.Chill ? Afflictions.Depth(Kind) - 1 : Afflictions.Depth(Kind);
            int want = depth - dummy.Afflictions.Stacks(Kind);
            // Laid to its depth, or once more to start its time over; Chill only ever up to one short. Laid as the world's,
            // so what the player lays on it reacts with it, as on an enemy wet from the river.
            int count = want > 0 ? want : Kind != AfflictionKind.Chill ? 1 : 0;
            if (count > 0) dummy.Afflictions.Lay(Kind, count, Kind == AfflictionKind.Burn ? 6f : 0f, 1f, LaySource.World, dummy);
        }
    }
}

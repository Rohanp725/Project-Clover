using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// The scene's list of standing-thing prefabs, one for each <see cref="StandingKind"/> (the editor step wires them after
    /// importing the models in staging/blender/2026-11-22_standin_art/), and the clock that ends standing things when their
    /// time is up and bursts the ones struck long enough ago (Cracked Wall). Put one in each fighting scene.
    /// </summary>
    public class StandingLibrary : MonoBehaviour
    {
        [System.Serializable]
        public struct Entry
        {
            public StandingKind Kind;
            public StandingThingBody Prefab;
        }

        public Entry[] Prefabs = new Entry[0];

        /// <summary>Told of a standing thing that burst by itself (Cracked Wall), with where it stood; the marks that raised it answer.</summary>
        public static event System.Action<Standing> Burst;

        // Domain reload is off: a listener from the last play must not be called in this one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => Burst = null;

        private void OnEnable()
        {
            foreach (var entry in Prefabs)
                if (entry.Prefab != null) StandingThings.SetPrefab(entry.Kind, entry.Prefab);
        }

        private void Update() => Run(Time.time, Time.frameCount);

        /// <summary>
        /// The standing things' clock for this frame, run once whoever calls first: the library in the scene, or the player's
        /// marks (which raise things in scenes that have no library yet). A thing that burst by itself is told to <see cref="Burst"/>.
        /// </summary>
        public static void Run(float now, int frame)
        {
            StandingThings.TickOnce(now, frame, out int burst);
            if (burst >= 0) Burst?.Invoke(StandingThings.Get(burst));
        }
    }
}

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

        private void Update()
        {
            StandingThings.Tick(Time.time, out int burst);
            if (burst >= 0) Burst?.Invoke(StandingThings.Get(burst));
        }
    }
}

using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// The scene's spell creature prefabs, one for each <see cref="CreatureKind"/>, wired by the editor step (Clover > Standing
    /// Things > Wire Standing Library In Open Scene wires these too). The forms take a body from here when one is wired.
    /// </summary>
    public class CreatureLibrary : MonoBehaviour
    {
        [System.Serializable]
        public struct Entry
        {
            public CreatureKind Kind;
            public CreatureRig Prefab;
        }

        public Entry[] Prefabs = new Entry[0];

        private static readonly CreatureRig[] prefabs = new CreatureRig[System.Enum.GetValues(typeof(CreatureKind)).Length];

        // Domain reload is off: a prefab from the last scene must not be used in this one.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            for (int i = 0; i < prefabs.Length; i++) prefabs[i] = null;
        }

        private void OnEnable()
        {
            foreach (var entry in Prefabs)
                if (entry.Prefab != null) prefabs[(int)entry.Kind] = entry.Prefab;
        }

        /// <summary>A body of the kind, made now at a place and turn; null when no prefab is wired (the form draws its own shapes).</summary>
        public static CreatureRig Make(CreatureKind kind, Vector3 at, Quaternion turn)
        {
            var prefab = prefabs[(int)kind];
            return prefab != null ? Instantiate(prefab, at, turn) : null;
        }

        /// <summary>Lets a body go.</summary>
        public static void Unmake(CreatureRig body)
        {
            if (body != null) Destroy(body.gameObject);
        }
    }
}

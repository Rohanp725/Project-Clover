using UnityEngine;

namespace Clover.World
{
    /// <summary>The spell creatures with a Blender model and a simple rig (owner question 3 of the stand-in plan: the cobra, the serpent, the totem).</summary>
    public enum CreatureKind
    {
        /// <summary>cobra.fbx: Uraeus' cobra of flame; takes cobra_rise, cobra_idle, cobra_spit; spit point at the snout.</summary>
        Cobra,
        /// <summary>river_serpent.fbx: Riptide's River Serpent; takes serpent_swim, serpent_bite; grab point between the jaws.</summary>
        RiverSerpent,
        /// <summary>stone_oak.fbx: Stone Oak's totem; three ages on one rig; takes oak_idle, oak_strike, oak_grow.</summary>
        StoneOak,
    }

    /// <summary>
    /// A spell creature's body, made by the editor step from its model (Clover > Standing Things > Build Creature Prefabs): its
    /// animator with one state for each take, the point it spits from or holds in its jaws, and for the oak its three ages.
    /// The forms move and turn it and call its takes by name; with no prefab wired they draw their own stand-in shapes.
    /// </summary>
    public class CreatureRig : MonoBehaviour
    {
        public CreatureKind Kind;
        public Animator Animator;
        /// <summary>The cobra's snout, where its venom leaves; the serpent's jaws, where it holds what it took.</summary>
        public Transform Mouth;
        /// <summary>The oak's ages, sapling to grown: one shown at a time.</summary>
        public GameObject[] Ages = new GameObject[0];

        /// <summary>Plays a take by its name ("cobra_spit"), from its start; nothing when it has no such state.</summary>
        public void Play(string take)
        {
            if (Animator == null || string.IsNullOrEmpty(take)) return;
            int state = Animator.StringToHash(take);
            if (Animator.HasState(0, state)) Animator.Play(state, 0, 0f);
        }

        /// <summary>Shows the age given (1 to the number of ages) and hides the others.</summary>
        public void ShowAge(int age)
        {
            for (int i = 0; i < Ages.Length; i++)
                if (Ages[i] != null) Ages[i].SetActive(i == Mathf.Clamp(age, 1, Ages.Length) - 1);
        }

        /// <summary>Where the mouth is now, or a point on the body when it has none.</summary>
        public Vector3 MouthAt => Mouth != null ? Mouth.position : transform.position + Vector3.up;
    }
}

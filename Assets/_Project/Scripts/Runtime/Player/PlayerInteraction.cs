using Clover.World;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Clover.Player
{
    /// <summary>
    /// Lets the player use things in the world. The scene's builder hands over everything that can be used; a few times
    /// a second this picks the nearest one in range, and the interact button uses it.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerInteraction : MonoBehaviour
    {
        [SerializeField] private Interactable[] things = new Interactable[0];

        public float LookEvery = 0.15f;

        /// <summary>The thing in range right now, or null.</summary>
        public Interactable Current { get; private set; }
        public PlayerCombat Combat { get; private set; }
        public PlayerDefence Defence { get; private set; }
        public PlayerController Controller { get; private set; }
        public int Uses { get; private set; }

        private InputAction interactAction;
        private float untilLook;

        public void Configure(Interactable[] thingsToUse) => things = thingsToUse;

        private void Awake()
        {
            Controller = GetComponent<PlayerController>();
            Combat = GetComponent<PlayerCombat>();
            Defence = GetComponent<PlayerDefence>();
        }

        private void Start() => interactAction = Controller.FindAction("Interact");

        private void Update()
        {
            if (Controller.Suspended) return;
            untilLook -= Time.deltaTime;
            if (untilLook <= 0f)
            {
                untilLook = LookEvery;
                Current = Nearest();
            }
            if (Current == null || interactAction == null) return;
            // Under the L2 layer the use button is a spell.
            if (!(Controller.Buttons != null ? Controller.Buttons.Pressed(PlayerButton.Interact) : interactAction.WasPressedThisFrame())) return;
            // Not in the middle of a fight's motions, and not when fallen.
            if (Defence != null && Defence.IsHelpless) return;
            if (Combat != null && Combat.IsAttacking) return;
            if (Controller.IsLeaping) return;
            Uses++;
            Current.Use(this);
            untilLook = 0f;
        }

        private Interactable Nearest()
        {
            Interactable best = null;
            float bestDistance = float.MaxValue;
            for (int i = 0; i < things.Length; i++)
            {
                var thing = things[i];
                if (thing == null || !thing.isActiveAndEnabled || !thing.Available) continue;
                Vector3 offset = thing.transform.position - transform.position;
                offset.y = 0f;
                float distance = offset.magnitude;
                if (distance > thing.Range || distance >= bestDistance) continue;
                best = thing;
                bestDistance = distance;
            }
            return best;
        }
    }
}

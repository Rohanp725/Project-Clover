using Clover.Core;
using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// A door barred on one side (a shortcut): from the bar's side "Lift the bar" opens it for good, and its progress
    /// key keeps it open across falls and sittings; from the other side it says so and stays shut. The bar's side is
    /// the side its <see cref="BarSide"/> direction points to from the door.
    /// </summary>
    public class BarredDoor : Interactable
    {
        /// <summary>A flat direction from the door toward the side the bar is on.</summary>
        public Vector3 BarSide = Vector3.forward;
        public string OpenKey = "";
        public string BarredWords = "Barred from the other side.";
        public string LiftWords = "Lift the bar";

        [SerializeField] private GameObject door;
        [SerializeField] private GameObject bar;

        public bool IsOpen { get; private set; }
        public int Refusals { get; private set; }

        public override bool Available => !IsOpen;

        public void Configure(GameObject doorBlock, GameObject barShown)
        {
            door = doorBlock;
            bar = barShown;
        }

        private void Start()
        {
            Prompt = LiftWords;
            if (!string.IsNullOrEmpty(OpenKey) && Progress.Has(OpenKey)) Open(false);
        }

        /// <summary>True for a point on the bar's side of the door.</summary>
        public bool OnBarSide(Vector3 point)
        {
            Vector3 offset = point - transform.position;
            return offset.x * BarSide.x + offset.z * BarSide.z > 0f;
        }

        public override void Use(PlayerInteraction who)
        {
            if (IsOpen) return;
            if (!OnBarSide(who.transform.position))
            {
                Refusals++;
                who.Defence.Callout(BarredWords);
                return;
            }
            Open(true);
        }

        private void Open(bool seen)
        {
            IsOpen = true;
            if (door != null) door.SetActive(false);
            if (bar != null) bar.SetActive(false);
            if (!string.IsNullOrEmpty(OpenKey)) Progress.Unlock(OpenKey);
            if (seen) Vfx.Play(VfxKind.Dust, transform.position, Vector3.up, 1.4f);
        }
    }
}

using Clover.Core;
using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// A ladder up a face: used from below it climbs to the head, from above it climbs down to the foot, in the
    /// player's leap. One that starts lying on the top (a shortcut) can only be kicked down from there, and stays down
    /// across falls and sittings once its progress key is written.
    /// </summary>
    public class Ladder : Interactable
    {
        public Vector3 Foot;
        public Vector3 Head;
        public float ClimbSeconds = 1.2f;
        public float ClimbArc = 1.4f;
        /// <summary>Empty for a ladder that is always down; otherwise it lies on the top until kicked down, and this key keeps it down.</summary>
        public string DownKey = "";
        public string UpWords = "Climb the ladder";
        public string DownWords = "Climb down the ladder";
        public string KickWords = "Kick down the ladder";
        public string LyingWords = "A ladder lies on the top, out of reach.";

        [SerializeField] private Transform shown;
        [SerializeField] private Vector3 lyingEuler;
        [SerializeField] private Vector3 standingEuler;
        [SerializeField] private Vector3 lyingPosition;
        [SerializeField] private Vector3 standingPosition;

        public bool IsDown { get; private set; }
        public int Climbs { get; private set; }
        [SerializeField] private Transform player;

        public void Configure(Transform climber, Transform model, Vector3 modelLying, Vector3 lyingAngles, Vector3 modelStanding, Vector3 standingAngles)
        {
            player = climber;
            shown = model;
            lyingPosition = modelLying;
            lyingEuler = lyingAngles;
            standingPosition = modelStanding;
            standingEuler = standingAngles;
        }

        private void Start()
        {
            IsDown = string.IsNullOrEmpty(DownKey) || Progress.Has(DownKey);
            Pose();
        }

        private void Pose()
        {
            if (shown == null) return;
            if (IsDown) shown.SetPositionAndRotation(standingPosition, Quaternion.Euler(standingEuler));
            else shown.SetPositionAndRotation(lyingPosition, Quaternion.Euler(lyingEuler));
        }

        private bool Above(Vector3 at) => at.y > (Foot.y + Head.y) * 0.5f;

        private void Update()
        {
            if (player == null) return;
            bool above = Above(player.position);
            Prompt = !IsDown ? (above ? KickWords : "Look up") : above ? DownWords : UpWords;
        }

        public override void Use(PlayerInteraction who)
        {
            bool above = Above(who.transform.position);
            if (!IsDown)
            {
                if (!above)
                {
                    who.Defence.Callout(LyingWords);
                    return;
                }
                IsDown = true;
                if (!string.IsNullOrEmpty(DownKey)) Progress.Unlock(DownKey);
                Pose();
                Vfx.Play(VfxKind.Dust, Foot, Vector3.up, 1.2f);
                return;
            }
            var controller = who.GetComponent<PlayerController>();
            if (controller == null || controller.IsLeaping) return;
            Climbs++;
            // Arched, so the body clears the lip of the face on the way over rather than sliding up against it.
            controller.Leap(above ? Foot : Head, ClimbSeconds, ClimbArc);
        }
    }
}

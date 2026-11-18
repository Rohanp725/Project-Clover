using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// The floor under a whole scene: a player who drops below it (through a gap nobody meant, off an edge no wall holds)
    /// is put back on the last ground they stood on firmly, or at the last fire lit when there is none, instead of
    /// falling for ever. It costs nothing and is not a death: the world is not turned back.
    /// </summary>
    public class FallCatch : MonoBehaviour
    {
        [SerializeField] private PlayerController player;
        [SerializeField] private AreaDirector director;

        /// <summary>Below this height the player is caught. Set under the lowest floor of the scene.</summary>
        public float Floor = -6f;

        public int Catches { get; private set; }
        public Vector3 LastFirm => lastFirm;
        public bool HasFirm => hasFirm;

        private CharacterController body;
        private PlayerDefence defence;
        private Vector3 lastFirm;
        private bool hasFirm;
        private float untilLook;
        private readonly RaycastHit[] hits = new RaycastHit[8];

        // Firm ground: the middle and four points a body's width round it all stand on something at the same height.
        private static readonly Vector3[] Round =
        {
            Vector3.zero, new Vector3(0.6f, 0f, 0f), new Vector3(-0.6f, 0f, 0f), new Vector3(0f, 0f, 0.6f), new Vector3(0f, 0f, -0.6f),
        };

        public void Configure(PlayerController playerToCatch, AreaDirector areaDirector)
        {
            player = playerToCatch;
            director = areaDirector;
            Find();
        }

        // Also from Configure: a builder adds this and configures it after, so Awake has run with no player yet.
        private void Awake() => Find();

        private void Find()
        {
            if (player == null) return;
            body = player.GetComponent<CharacterController>();
            defence = player.GetComponent<PlayerDefence>();
        }

        private void Update()
        {
            if (player == null) return;
            Vector3 at = player.transform.position;
            if (at.y < Floor)
            {
                Catch();
                return;
            }
            untilLook -= Time.deltaTime;
            if (untilLook > 0f) return;
            untilLook = 0.2f;
            if (body != null && body.isGrounded && !player.IsLeaping && !player.IsHeld && Firm(at))
            {
                lastFirm = at;
                hasFirm = true;
            }
        }

        /// <summary>Puts the player back on firm ground now (what a fall below the floor does).</summary>
        public void Catch()
        {
            Vector3 to = hasFirm ? lastFirm : director != null && director.ReturnPoint != null ? director.ReturnPoint.position : player.transform.position + Vector3.up * 20f;
            player.Teleport(to + Vector3.up * 0.1f, player.transform.rotation);
            Catches++;
            if (defence != null) defence.Callout("Back on firm ground");
        }

        private bool Firm(Vector3 at)
        {
            for (int i = 0; i < Round.Length; i++)
            {
                Vector3 from = at + Round[i] + Vector3.up * 0.5f;
                int count = Physics.RaycastNonAlloc(from, Vector3.down, hits, 1.2f, ~0, QueryTriggerInteraction.Ignore);
                float nearest = float.MaxValue;
                for (int k = 0; k < count; k++)
                    if (!(hits[k].collider is CharacterController) && hits[k].distance < nearest) nearest = hits[k].distance;
                if (nearest == float.MaxValue || Mathf.Abs(from.y - nearest - at.y) > 0.4f) return false;
            }
            return true;
        }
    }
}

using Clover.Core;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// A cracked mud-brick wall that one heavy blow, or a burst beside it, brings down: the wall is put away, the
    /// rubble shown, and the progress key keeps it down. Light blows only knock dust off it.
    /// </summary>
    public class BreakableWall : MonoBehaviour, IDamageable
    {
        public string BrokenKey = "";
        /// <summary>A blow at least this strong breaks it though it is not heavy (an ashen's burst is 30 to the player and 45 to the rest).</summary>
        public float BreaksAt = 40f;

        [SerializeField] private GameObject whole;
        [SerializeField] private GameObject rubble;

        public bool Broken { get; private set; }
        public int Knocks { get; private set; }

        public void Configure(GameObject wallShown, GameObject rubbleShown)
        {
            whole = wallShown;
            rubble = rubbleShown;
        }

        private void Start()
        {
            if (!string.IsNullOrEmpty(BrokenKey) && Progress.Has(BrokenKey)) Break(false);
            else if (rubble != null) rubble.SetActive(false);
        }

        public HitOutcome TakeHit(in Hit hit)
        {
            if (Broken) return HitOutcome.Missed;
            if (!hit.Heavy && hit.Damage < BreaksAt)
            {
                Knocks++;
                Vfx.Play(VfxKind.Dust, hit.HasOrigin ? hit.Origin : transform.position, Vector3.up, 0.6f);
                return HitOutcome.Landed;
            }
            Break(true);
            return HitOutcome.Landed;
        }

        public void Break(bool seen)
        {
            if (Broken) return;
            Broken = true;
            if (whole != null) whole.SetActive(false);
            if (rubble != null) rubble.SetActive(true);
            var solid = GetComponent<Collider>();
            if (solid != null) solid.enabled = false;
            var drawn = GetComponent<Renderer>();
            if (drawn != null) drawn.enabled = false;
            if (!string.IsNullOrEmpty(BrokenKey)) Progress.Unlock(BrokenKey);
            if (seen)
            {
                Vfx.Play(VfxKind.Dust, transform.position + Vector3.up * 1.2f, Vector3.up, 2.4f);
                Vfx.Play(VfxKind.Ash, transform.position + Vector3.up * 1.6f, Vector3.up, 2f, new Color(0.6f, 0.5f, 0.4f, 1f));
            }
        }
    }
}

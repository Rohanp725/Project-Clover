using Clover.Core;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The levy's shield wall (fire_rework.md, section 1). Levies placed together lock shields into a line across the
    /// player's way and advance on them in step, spears thrusting over the rim. From the front a light blow is turned
    /// on the shields as a levy's always is; flank the line, or break it with a heavy blow to one shield: the line opens,
    /// every guard in it is down for a while, and each fights alone until it re-forms.
    /// </summary>
    public class ShieldWall : MonoBehaviour
    {
        [SerializeField] private Enemy[] members = new Enemy[0];

        public float Spacing = 1.25f;
        /// <summary>Metres a second the line advances, and how near the player it stops.</summary>
        public float Pace = 1.5f;
        public float HoldAt = 2.4f;
        public float BrokenSeconds = 4f;
        public float GuardDownSeconds = 2.5f;

        public bool Formed { get; private set; }
        public bool IsBroken => brokenLeft > 0f;
        public int Breaks { get; private set; }

        private Vector3 centre;
        private float brokenLeft;
        private float untilStep;

        public void Configure(Enemy[] levies) => members = levies ?? new Enemy[0];

        private void OnEnable()
        {
            foreach (var member in members)
                if (member != null) member.Struck += OnStruck;
        }

        private void OnDisable()
        {
            foreach (var member in members)
                if (member != null) member.Struck -= OnStruck;
        }

        private void OnStruck(Enemy who, Hit hit, HitOutcome outcome)
        {
            if (!Formed || !hit.Heavy || outcome == HitOutcome.Missed || outcome == HitOutcome.Dodged) return;
            // Only a blow on the shield, from in front of the line, opens it.
            if (Vector3.Angle(who.transform.forward, -hit.Direction) > 80f) return;
            Break(who);
        }

        /// <summary>Opens the line, as a heavy blow on one shield does.</summary>
        public void Break(Enemy where)
        {
            Breaks++;
            brokenLeft = BrokenSeconds;
            Formed = false;
            foreach (var member in members)
            {
                if (member == null) continue;
                member.ClearSteer();
                member.GuardOpenFor = GuardDownSeconds;
            }
            Vector3 at = where != null ? where.transform.position + Vector3.up * 1.2f + where.transform.forward * 0.5f : centre;
            Vfx.Play(VfxKind.Clang, at, Vector3.up, 1.6f);
            Vfx.Play(VfxKind.TintSpark, at, Vector3.up, 1.4f, new Color(1f, 0.78f, 0.3f, 1f));
            Vfx.Play(VfxKind.Impact, at);
        }

        private int Standing(out Vector3 middle, out Transform target)
        {
            int count = 0;
            middle = Vector3.zero;
            target = null;
            foreach (var member in members)
            {
                if (member == null || member.IsDead || !member.InFight || member.IsDormant) continue;
                count++;
                middle += member.transform.position;
                if (target == null) target = member.Target;
            }
            if (count > 0) middle /= count;
            return count;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            if (brokenLeft > 0f)
            {
                brokenLeft -= deltaTime;
                return;
            }
            untilStep -= deltaTime;
            if (untilStep > 0f) return;
            untilStep = 0.1f;
            int standing = Standing(out Vector3 middle, out Transform target);
            if (standing < 2 || target == null)
            {
                if (Formed) foreach (var member in members) if (member != null) member.ClearSteer();
                Formed = false;
                return;
            }
            if (!Formed) centre = middle;
            Formed = true;
            Vector3 toTarget = target.position - centre;
            toTarget.y = 0f;
            float distance = toTarget.magnitude;
            Vector3 facing = distance > 0.01f ? toTarget / distance : Vector3.forward;
            // The line steps forward together until it is a spear's length off.
            if (distance > HoldAt) centre += facing * Mathf.Min(distance - HoldAt, Pace * 0.1f);
            Vector3 along = Vector3.Cross(Vector3.up, facing);
            int slot = 0;
            foreach (var member in members)
            {
                if (member == null || member.IsDead || !member.InFight || member.IsDormant) continue;
                float offset = (slot - (standing - 1) * 0.5f) * Spacing;
                slot++;
                member.Steer(centre + along * offset, member.Kind.WalkSpeed * 1.2f, true);
            }
        }
    }
}

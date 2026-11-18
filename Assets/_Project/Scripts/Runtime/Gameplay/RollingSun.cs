using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The loose sun as a ball on the court's floor (fire_land.md 6.4, reworked on 2026-11-18): struck, it rolls the way
    /// the blow went, hard for a heavy blow, slowing as it goes; it bounces off the court's wall and the obelisk and never
    /// passes into either. While it rolls hard it is a weapon: rolled into the scarab or the grounded falcon it sets the
    /// shell alight. Plain C#, on the floor (y is ignored).
    /// </summary>
    public class RollingSun
    {
        public float Radius = 1.5f;
        /// <summary>How far out its middle may come before it meets the wall, and how near the middle before it meets the stone.</summary>
        public float RimAt = 18.3f, StoneAt = 3.4f;
        /// <summary>Metres a second lost each second, and the share of its speed kept off a bounce.</summary>
        public float Friction = 9f, Bounce = 0.55f;
        /// <summary>Below this speed it is no longer rolling hard, and cannot set anything alight.</summary>
        public float HardSpeed = 4f;

        public Vector3 Position;
        public Vector3 Velocity;
        public int Bounces { get; private set; }
        /// <summary>True from a heavy blow until it slows or strikes something.</summary>
        public bool Hard { get; private set; }
        public bool Moving => Velocity.sqrMagnitude > 0.01f;
        public float Speed => Velocity.magnitude;

        /// <summary>Struck: it goes the way of the blow at the speed given (a heavy blow's or a light one's).</summary>
        public void Strike(Vector3 direction, float speed, bool heavy)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;
            Velocity = direction.normalized * speed;
            Hard = heavy;
        }

        public void Stop()
        {
            Velocity = Vector3.zero;
            Hard = false;
        }

        /// <summary>It has struck something and comes back off it a little.</summary>
        public void Rebound(float share)
        {
            Velocity = -Velocity * share;
            Hard = false;
        }

        /// <summary>Rolls on for one step. Returns the metres it went.</summary>
        public float Step(float deltaTime)
        {
            if (!Moving)
            {
                Velocity = Vector3.zero;
                Hard = false;
                return 0f;
            }
            Vector3 was = Position;
            Position += Velocity * deltaTime;
            Position.y = 0f;
            float out_ = new Vector2(Position.x, Position.z).magnitude;
            if (out_ > RimAt)
            {
                // Off the wall: the part of its speed toward the wall is turned back, and some of it lost.
                var normal = new Vector3(Position.x, 0f, Position.z) / out_;
                Velocity = (Velocity - 2f * Vector3.Dot(Velocity, normal) * normal) * Bounce;
                Position = normal * RimAt;
                Bounces++;
            }
            else if (out_ < StoneAt)
            {
                var normal = out_ > 0.001f ? new Vector3(Position.x, 0f, Position.z) / out_ : Vector3.forward;
                Velocity = (Velocity - 2f * Vector3.Dot(Velocity, normal) * normal) * Bounce;
                Position = normal * StoneAt;
                Bounces++;
            }
            float speed = Velocity.magnitude;
            float slower = Mathf.Max(0f, speed - Friction * deltaTime);
            Velocity = speed > 0.0001f ? Velocity * (slower / speed) : Vector3.zero;
            if (slower < HardSpeed) Hard = false;
            return (Position - was).magnitude;
        }

        /// <summary>How far a heavy blow sends it on open floor at this speed before it stops.</summary>
        public float Reach(float speed) => Friction > 0f ? speed * speed / (2f * Friction) : 999f;

        /// <summary>True when a ball of <paramref name="radius"/> at <paramref name="ball"/> touches a body lying from <paramref name="tail"/> to <paramref name="head"/> with the given girth.</summary>
        public static bool Meets(Vector3 ball, float radius, Vector3 tail, Vector3 head, float girth)
        {
            var p = new Vector2(ball.x, ball.z);
            var a = new Vector2(tail.x, tail.z);
            var b = new Vector2(head.x, head.z);
            Vector2 line = b - a;
            float t = line.sqrMagnitude > 0.0001f ? Mathf.Clamp01(Vector2.Dot(p - a, line) / line.sqrMagnitude) : 0f;
            return (a + line * t - p).magnitude <= radius + girth;
        }
    }
}

using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// The obelisk's shadow in the sun-court (fire_land.md, section 6.3): a straight strip on the floor that starts at
    /// the obelisk, points away from the sun and turns like the hand of a clock. Whoever stands in it is out of the
    /// blaze. It is plain numbers, so the rule can be tested without a scene; the court's director turns the light to
    /// match it and tells <see cref="Clover.World.Sunlight"/> where it lies, so shade here is the same shade the
    /// sun-blind priests hunt by.
    /// </summary>
    public class ShadowHand
    {
        /// <summary>Where the strip begins and ends, in metres from the court's centre, and half its width.</summary>
        public float From = 1.2f, To = 18f, HalfWidth = 1.8f;
        public Vector3 Centre;
        /// <summary>Degrees a second, clockwise seen from above. Negative turns it back the other way.</summary>
        public float DegreesPerSecond = 4f;
        /// <summary>Degrees round from north (+Z), toward the east for more.</summary>
        public float Angle { get; private set; }
        /// <summary>Seconds it has still to stand before it turns again: the fury's warning.</summary>
        public float Stopped { get; private set; }

        public const float AgainstTheMan = 4f, AgainstTheForm = 6f, InFury = 9f;

        /// <summary>The way the hand points along the floor.</summary>
        public Vector3 Direction
        {
            get
            {
                float turn = Angle * Mathf.Deg2Rad;
                return new Vector3(Mathf.Sin(turn), 0f, Mathf.Cos(turn));
            }
        }

        /// <summary>Puts the hand back to north, turning at the rate given.</summary>
        public void Reset(float degreesPerSecond = AgainstTheMan)
        {
            Angle = 0f;
            Stopped = 0f;
            DegreesPerSecond = degreesPerSecond;
        }

        /// <summary>Points the hand this many degrees round from north at once, as the setting sun moves it after the fight.</summary>
        public void PointTo(float degrees)
        {
            Angle = Mathf.Repeat(degrees, 360f);
            Stopped = 0f;
        }

        public void Tick(float deltaTime)
        {
            if (Stopped > 0f)
            {
                Stopped -= deltaTime;
                return;
            }
            Angle = Mathf.Repeat(Angle + DegreesPerSecond * deltaTime, 360f);
        }

        /// <summary>Stands still for a moment and then turns the other way: once, in the fury's blaze.</summary>
        public void StopAndTurnBack(float seconds)
        {
            Stopped = seconds;
            DegreesPerSecond = -DegreesPerSecond;
        }

        /// <summary>How far a point lies along the hand, and how far to the side of its middle line.</summary>
        public void Measure(Vector3 position, out float along, out float across)
        {
            Vector3 direction = Direction;
            float x = position.x - Centre.x, z = position.z - Centre.z;
            along = x * direction.x + z * direction.z;
            across = Mathf.Abs(x * direction.z - z * direction.x);
        }

        /// <summary>True for a point on the floor that the shadow covers.</summary>
        public bool Covers(Vector3 position)
        {
            Measure(position, out float along, out float across);
            return along >= From && along <= To && across <= HalfWidth;
        }

        /// <summary>
        /// The nearest point outside the shadow for a point inside it (a point outside is given back as it is), this
        /// far clear of its edge: where a shaft of sun is laid when it is aimed at someone the shadow hides. With the
        /// shaft's own radius as the clearance, nothing of it falls in the shadow.
        /// </summary>
        public Vector3 NearestOutside(Vector3 position, float clear = 0.2f)
        {
            if (!Covers(position)) return position;
            Vector3 direction = Direction;
            float x = position.x - Centre.x, z = position.z - Centre.z;
            // Which side of the middle line it stands on; the edge on that side is the nearer.
            float side = x * direction.z - z * direction.x;
            float push = HalfWidth + clear - Mathf.Abs(side);
            var square = new Vector3(direction.z, 0f, -direction.x) * (side >= 0f ? 1f : -1f);
            return position + square * push;
        }
    }
}

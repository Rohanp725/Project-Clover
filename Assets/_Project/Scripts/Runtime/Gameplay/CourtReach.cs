using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// Whether the player can still walk to the Sun-Bearer without stepping on fire (fire_land.md 6.4, the hot-floor rule
    /// as reworked on 2026-11-18): the court as a coarse grid of one-metre cells, the obelisk and every burning patch
    /// blocked, a flood from where the player stands. Burning floor within a few metres of the player is let through,
    /// since they may be standing on it and need only step off. Before a lane is laid the Sun-Bearer asks this with the
    /// new lane added; when the answer is no it shifts the lane or lays no fire. Plain C#: nothing is allocated after
    /// it is made.
    /// </summary>
    public class CourtReach
    {
        /// <summary>The floor one can walk on, the stone in the middle, and the size of a cell, in metres.</summary>
        public readonly float FloorRadius, ObeliskRadius, Cell;
        /// <summary>How far round the player burning floor is let through, and how near the goal counts as there.</summary>
        public float StepOff = 2.5f, Near = 3.5f;
        /// <summary>The player's own width: a cell is blocked when fire reaches within this of its middle.</summary>
        public float Body = 0.4f;

        private readonly int side;
        private readonly bool[] walkable, burning, seen;
        private readonly int[] queue;

        public CourtReach(float floorRadius = 19.3f, float obeliskRadius = 2.2f, float cell = 1f)
        {
            FloorRadius = floorRadius;
            ObeliskRadius = obeliskRadius;
            Cell = cell;
            side = Mathf.CeilToInt(floorRadius * 2f / cell) + 1;
            walkable = new bool[side * side];
            burning = new bool[side * side];
            seen = new bool[side * side];
            queue = new int[side * side];
            for (int i = 0; i < walkable.Length; i++)
            {
                Vector2 at = Middle(i);
                float out_ = at.magnitude;
                walkable[i] = out_ <= floorRadius && out_ >= obeliskRadius;
            }
        }

        private Vector2 Middle(int index) => new Vector2((index % side) * Cell - FloorRadius, (index / side) * Cell - FloorRadius);

        private int IndexOf(Vector2 point)
        {
            int x = Mathf.Clamp(Mathf.RoundToInt((point.x + FloorRadius) / Cell), 0, side - 1);
            int z = Mathf.Clamp(Mathf.RoundToInt((point.y + FloorRadius) / Cell), 0, side - 1);
            return z * side + x;
        }

        /// <summary>No fire anywhere: start again.</summary>
        public void Clear()
        {
            for (int i = 0; i < burning.Length; i++) burning[i] = false;
        }

        /// <summary>A burning lane on the floor, from a point along a level direction (x and z of both).</summary>
        public void Lane(Vector3 from, Vector3 along, float length, float width)
        {
            var start = new Vector2(from.x, from.z);
            var way = new Vector2(along.x, along.z);
            if (way.sqrMagnitude < 0.0001f) return;
            way.Normalize();
            float half = width * 0.5f + Body;
            for (int i = 0; i < burning.Length; i++)
            {
                if (!walkable[i] || burning[i]) continue;
                Vector2 offset = Middle(i) - start;
                float run = Vector2.Dot(offset, way);
                if (run < -Body || run > length + Body) continue;
                float across = Mathf.Abs(offset.x * way.y - offset.y * way.x);
                if (across <= half) burning[i] = true;
            }
        }

        /// <summary>A burning disc on the floor.</summary>
        public void Disc(Vector3 centre, float radius)
        {
            var middle = new Vector2(centre.x, centre.z);
            float reach = (radius + Body) * (radius + Body);
            for (int i = 0; i < burning.Length; i++)
                if (walkable[i] && !burning[i] && (Middle(i) - middle).sqrMagnitude <= reach) burning[i] = true;
        }

        /// <summary>True when a path of cells free of fire leads from <paramref name="from"/> to within <see cref="Near"/> of <paramref name="to"/>.</summary>
        public bool Reaches(Vector3 from, Vector3 to)
        {
            var start = new Vector2(from.x, from.z);
            var goal = new Vector2(to.x, to.z);
            for (int i = 0; i < seen.Length; i++) seen[i] = false;
            int head = 0, tail = 0;
            int first = IndexOf(start);
            queue[tail++] = first;
            seen[first] = true;
            float stepOff = StepOff * StepOff, near = Near * Near;
            while (head < tail)
            {
                int at = queue[head++];
                Vector2 middle = Middle(at);
                if ((middle - goal).sqrMagnitude <= near && (!burning[at] || at == first)) return true;
                int x = at % side, z = at / side;
                for (int k = 0; k < 4; k++)
                {
                    int nx = x + (k == 0 ? 1 : k == 1 ? -1 : 0), nz = z + (k == 2 ? 1 : k == 3 ? -1 : 0);
                    if (nx < 0 || nz < 0 || nx >= side || nz >= side) continue;
                    int next = nz * side + nx;
                    if (seen[next] || !walkable[next]) continue;
                    // Fire is crossed only to step off it near where the player stands.
                    if (burning[next] && (Middle(next) - start).sqrMagnitude > stepOff) continue;
                    seen[next] = true;
                    queue[tail++] = next;
                }
            }
            return false;
        }
    }
}

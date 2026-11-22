namespace Clover.UI.Toolkit
{
    /// <summary>A push of the stick, the d-pad or an arrow key.</summary>
    public enum NavDirection
    {
        Up,
        Down,
        Left,
        Right,
    }

    /// <summary>
    /// Where the stick goes on a menu screen (ui_rest_fire_plan.md 3.5.4 and 4.5): each place names its neighbour in each
    /// direction, or none, so focus moves to what the stick points at and nothing wraps round the ends. A link may be
    /// "back where it came from": when two places share one neighbour, that neighbour remembers which one focus came from
    /// and returns there, so pushing a direction and then its opposite always comes back. Plain C#, so the map is tested
    /// without a screen. Places are small numbers (a screen's own enum).
    /// </summary>
    public sealed class FocusGrid
    {
        private const int Directions = 4;
        private const int None = -1;

        private readonly int count;
        private readonly int[] links;
        private readonly bool[] remembers;
        // Who entered each place moving the opposite way of each direction: the place a "back" link returns to.
        private readonly int[] enteredFrom;

        public FocusGrid(int count)
        {
            this.count = count;
            links = new int[count * Directions];
            remembers = new bool[count * Directions];
            enteredFrom = new int[count * Directions];
            for (int i = 0; i < links.Length; i++)
            {
                links[i] = None;
                enteredFrom[i] = None;
            }
        }

        public int Count => count;

        public static NavDirection Opposite(NavDirection direction) => direction switch
        {
            NavDirection.Up => NavDirection.Down,
            NavDirection.Down => NavDirection.Up,
            NavDirection.Left => NavDirection.Right,
            _ => NavDirection.Left,
        };

        /// <summary>Pushing <paramref name="direction"/> on <paramref name="from"/> goes to <paramref name="to"/>.</summary>
        public FocusGrid Link(int from, NavDirection direction, int to)
        {
            links[from * Directions + (int)direction] = to;
            remembers[from * Directions + (int)direction] = false;
            return this;
        }

        /// <summary>Pushing <paramref name="direction"/> goes back where focus came from, or to <paramref name="firstTime"/> before it has come from anywhere.</summary>
        public FocusGrid LinkBack(int from, NavDirection direction, int firstTime)
        {
            links[from * Directions + (int)direction] = firstTime;
            remembers[from * Directions + (int)direction] = true;
            return this;
        }

        /// <summary>Where a push would go, without moving: the place itself when the push goes nowhere.</summary>
        public int Neighbour(int from, NavDirection direction)
        {
            if (from < 0 || from >= count) return from;
            int at = from * Directions + (int)direction;
            int to = links[at];
            if (to == None) return from;
            if (remembers[at] && enteredFrom[at] != None) return enteredFrom[at];
            return to;
        }

        /// <summary>True when the push leads somewhere.</summary>
        public bool CanMove(int from, NavDirection direction) => Neighbour(from, direction) != from;

        /// <summary>Moves focus: the place reached (the same one when the push goes nowhere), remembered for the way back.</summary>
        public int Move(int from, NavDirection direction)
        {
            int to = Neighbour(from, direction);
            if (to == from) return from;
            enteredFrom[to * Directions + (int)Opposite(direction)] = from;
            return to;
        }

        /// <summary>Forgets where focus came from (a screen opened afresh).</summary>
        public void Forget()
        {
            for (int i = 0; i < enteredFrom.Length; i++) enteredFrom[i] = None;
        }
    }
}

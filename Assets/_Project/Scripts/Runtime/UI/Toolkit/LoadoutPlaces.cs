namespace Clover.UI.Toolkit
{
    /// <summary>The places on the Loadout that take focus (ui_rest_fire_plan.md 3.5.4). The weapon picture and the pane never do.</summary>
    public enum LoadoutPlace
    {
        Aspect,
        Keystone1,
        Keystone2,
        Temper,
        Technique,
        Bearing1,
        Bearing2,
        Infused,
        ElementHeader,
        RoleStrip,
        North,
        West,
        Surge,
        East,
        South,
        Starter,
        Hand1,
        Hand2,
        Hand3,
    }

    /// <summary>The Loadout's places: their names in Loadout.uxml, their half, and the stick's map of 3.5.4.</summary>
    public static class LoadoutPlaces
    {
        public const int Count = 19;

        /// <summary>Each place's element name in the tree, in the enum's order (probes find places by these).</summary>
        public static readonly string[] Names =
        {
            "aspect", "keystone-1", "keystone-2", "temper", "technique", "bearing-1", "bearing-2", "infused",
            "element-header", "role-strip", "slot-north", "slot-west", "surge", "slot-east", "slot-south",
            "hand-starter", "hand-1", "hand-2", "hand-3",
        };

        /// <summary>Where focus first lands in each half: the aspect in Steel, the West slot in Element (3.5.3).</summary>
        public const LoadoutPlace SteelFirst = LoadoutPlace.Aspect;
        public const LoadoutPlace ElementFirst = LoadoutPlace.West;

        public static string NameOf(LoadoutPlace place) => Names[(int)place];

        /// <summary>True for the Steel half (the Way's choices and the infused strikes); false for the Element half.</summary>
        public static bool InSteel(LoadoutPlace place) => place <= LoadoutPlace.Infused;

        /// <summary>The Hand slot a place is (0 West, 1 North, 2 East, 3 South, by button, as <c>Loadout.Slots</c>), 4 for the surge, -1 for none.</summary>
        public static int SlotOf(LoadoutPlace place) => place switch
        {
            LoadoutPlace.West => 0,
            LoadoutPlace.North => 1,
            LoadoutPlace.East => 2,
            LoadoutPlace.South => 3,
            LoadoutPlace.Surge => 4,
            _ => -1,
        };

        public static LoadoutPlace PlaceOfSlot(int slot) => slot switch
        {
            0 => LoadoutPlace.West,
            1 => LoadoutPlace.North,
            2 => LoadoutPlace.East,
            3 => LoadoutPlace.South,
            _ => LoadoutPlace.Surge,
        };

        /// <summary>The Hands row: 0 the Starter, 1 to 3 the saved Hands; -1 for any other place.</summary>
        public static int HandOf(LoadoutPlace place) => place >= LoadoutPlace.Starter ? place - LoadoutPlace.Starter : -1;

        /// <summary>The pair cell a place is (0 the top, 1 the bottom), or -1.</summary>
        public static int PairCellOf(LoadoutPlace place) => place switch
        {
            LoadoutPlace.Keystone1 or LoadoutPlace.Bearing1 => 0,
            LoadoutPlace.Keystone2 or LoadoutPlace.Bearing2 => 1,
            _ => -1,
        };

        /// <summary>
        /// The stick's map, 3.5.4's table. Two links the table writes as plain neighbours are "back where it came from" here,
        /// as the rule under the table asks, so a push and its opposite always come back: right from the aspect (Temper
        /// the first time) and down from the infused strikes (North the first time).
        /// </summary>
        public static FocusGrid NewGrid()
        {
            const NavDirection U = NavDirection.Up, D = NavDirection.Down, L = NavDirection.Left, R = NavDirection.Right;
            var grid = new FocusGrid(Count);
            void Link(LoadoutPlace from, NavDirection way, LoadoutPlace to) => grid.Link((int)from, way, (int)to);
            void Back(LoadoutPlace from, NavDirection way, LoadoutPlace firstTime) => grid.LinkBack((int)from, way, (int)firstTime);

            Link(LoadoutPlace.Aspect, D, LoadoutPlace.Keystone1);
            Back(LoadoutPlace.Aspect, R, LoadoutPlace.Temper);
            Link(LoadoutPlace.Keystone1, U, LoadoutPlace.Aspect);
            Link(LoadoutPlace.Keystone1, D, LoadoutPlace.Keystone2);
            Link(LoadoutPlace.Keystone1, R, LoadoutPlace.Bearing1);
            Link(LoadoutPlace.Keystone2, U, LoadoutPlace.Keystone1);
            Link(LoadoutPlace.Keystone2, D, LoadoutPlace.Infused);
            Link(LoadoutPlace.Keystone2, R, LoadoutPlace.Bearing2);
            Link(LoadoutPlace.Temper, D, LoadoutPlace.Technique);
            Link(LoadoutPlace.Temper, L, LoadoutPlace.Aspect);
            Link(LoadoutPlace.Technique, U, LoadoutPlace.Temper);
            Link(LoadoutPlace.Technique, D, LoadoutPlace.Bearing1);
            Link(LoadoutPlace.Technique, L, LoadoutPlace.Aspect);
            Link(LoadoutPlace.Bearing1, U, LoadoutPlace.Technique);
            Link(LoadoutPlace.Bearing1, D, LoadoutPlace.Bearing2);
            Link(LoadoutPlace.Bearing1, L, LoadoutPlace.Keystone1);
            Link(LoadoutPlace.Bearing2, U, LoadoutPlace.Bearing1);
            Link(LoadoutPlace.Bearing2, D, LoadoutPlace.Infused);
            Link(LoadoutPlace.Bearing2, L, LoadoutPlace.Keystone2);
            Back(LoadoutPlace.Infused, U, LoadoutPlace.Keystone2);
            Back(LoadoutPlace.Infused, D, LoadoutPlace.North);
            Link(LoadoutPlace.ElementHeader, U, LoadoutPlace.Infused);
            Link(LoadoutPlace.ElementHeader, D, LoadoutPlace.West);
            Link(LoadoutPlace.ElementHeader, R, LoadoutPlace.North);
            Link(LoadoutPlace.North, U, LoadoutPlace.Infused);
            Link(LoadoutPlace.North, D, LoadoutPlace.Surge);
            Link(LoadoutPlace.North, L, LoadoutPlace.ElementHeader);
            Link(LoadoutPlace.North, R, LoadoutPlace.RoleStrip);
            Link(LoadoutPlace.RoleStrip, U, LoadoutPlace.Infused);
            Link(LoadoutPlace.RoleStrip, D, LoadoutPlace.East);
            Link(LoadoutPlace.RoleStrip, L, LoadoutPlace.North);
            Link(LoadoutPlace.West, U, LoadoutPlace.ElementHeader);
            Link(LoadoutPlace.West, D, LoadoutPlace.Starter);
            Back(LoadoutPlace.West, R, LoadoutPlace.Surge);
            Link(LoadoutPlace.Surge, U, LoadoutPlace.North);
            Link(LoadoutPlace.Surge, D, LoadoutPlace.South);
            Link(LoadoutPlace.Surge, L, LoadoutPlace.West);
            Link(LoadoutPlace.Surge, R, LoadoutPlace.East);
            Link(LoadoutPlace.East, U, LoadoutPlace.RoleStrip);
            Link(LoadoutPlace.East, D, LoadoutPlace.Hand3);
            Back(LoadoutPlace.East, L, LoadoutPlace.Surge);
            Link(LoadoutPlace.South, U, LoadoutPlace.Surge);
            Back(LoadoutPlace.South, D, LoadoutPlace.Hand1);
            Link(LoadoutPlace.South, L, LoadoutPlace.West);
            Link(LoadoutPlace.South, R, LoadoutPlace.East);
            Link(LoadoutPlace.Starter, U, LoadoutPlace.West);
            Link(LoadoutPlace.Starter, R, LoadoutPlace.Hand1);
            Link(LoadoutPlace.Hand1, U, LoadoutPlace.South);
            Link(LoadoutPlace.Hand1, L, LoadoutPlace.Starter);
            Link(LoadoutPlace.Hand1, R, LoadoutPlace.Hand2);
            Link(LoadoutPlace.Hand2, U, LoadoutPlace.South);
            Link(LoadoutPlace.Hand2, L, LoadoutPlace.Hand1);
            Link(LoadoutPlace.Hand2, R, LoadoutPlace.Hand3);
            Link(LoadoutPlace.Hand3, U, LoadoutPlace.East);
            Link(LoadoutPlace.Hand3, L, LoadoutPlace.Hand2);
            return grid;
        }
    }
}

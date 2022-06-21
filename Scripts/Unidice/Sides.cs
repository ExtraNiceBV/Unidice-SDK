namespace Unidice.SDK.Unidice
{
    public interface ISide
    {
        public bool IsLocal { get; }
        bool Opposes(ISide side);
    }

    /// <summary>
    /// The side relative to the die's current rotation.
    /// </summary>
    public class SideLocal : ISide
    {
        public static readonly ISide Top = new SideLocal("Top (local)");
        public static readonly ISide Bottom = new SideLocal("Bottom (local)", Top);
        public static readonly ISide Left = new SideLocal("Left (local)");
        public static readonly ISide Right = new SideLocal("Right (local)", Left);
        public static readonly ISide Front = new SideLocal("Front (local)");
        public static readonly ISide Back = new SideLocal("Back (local)", Front);
        public static readonly ISide All = new SideLocal("All");

        public bool IsLocal => true;
        public bool Opposes(ISide side) => side == _opposing;

        private readonly string _name;
        private ISide _opposing;

        public SideLocal(string name, ISide opposing = null)
        {
            _name = name;

            // Initialize opposite; only way for them to refer to each other
            if (opposing is SideLocal o)
            {
                _opposing = o;
                o._opposing = this;
            }
        }
        
        public override string ToString() => _name;

        public static ISide[] Each { get; } = { Top, Bottom, Left, Right, Front, Back };
    }

    /// <summary>
    /// The side relative to the table. Only top and bottom can be identified with precision.
    /// </summary>
    public class SideWorld : ISide
    {
        public static readonly ISide Top = new SideWorld("Top (world)");
        public static readonly ISide Bottom = new SideWorld("Bottom (world)", Top);
        public static readonly ISide All = SideLocal.All;

        public bool IsLocal => false;
        public bool Opposes(ISide side) => side == _opposing;

        private readonly string _name;
        private ISide _opposing;

        public SideWorld(string name, ISide opposing = null)
        {
            _name = name;
            // Initialize opposite; only way for them to refer to each other
            if (opposing is SideWorld o)
            {
                _opposing = o;
                o._opposing = this;
            }
        }

        public override string ToString() => _name;
    }
}
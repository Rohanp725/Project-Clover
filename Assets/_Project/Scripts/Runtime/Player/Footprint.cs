using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// The ground a carrier move covered, recorded when it strikes (steel/weapons.md 15.2; IMPLEMENTATION.md 3.4): an arc
    /// (the Katana's draw), a lane (the Spear's thrust), a disc (the Hammer's drop), a ring (the Scythe's full turn) or a
    /// path (a shot's line, the Knives' crossing). Each element's infused strike acts on this shape, so one strike reads
    /// right on every weapon. A plain value: read from the step and where the player stood.
    /// </summary>
    public struct Footprint
    {
        public FootprintShape Shape;
        /// <summary>Where the player stood and faced when the move struck.</summary>
        public Vector3 Origin;
        public Vector3 Forward;
        /// <summary>How far it reaches (a lane's or path's length; an arc's or ring's radius; a disc's distance ahead).</summary>
        public float Reach;
        public float ArcDegrees;
        /// <summary>A lane's or path's width.</summary>
        public float Width;
        /// <summary>A disc's radius.</summary>
        public float Radius;

        /// <summary>The middle of what it covered: a disc's centre, half way along a lane or path, the player for an arc or a ring.</summary>
        public Vector3 Centre
        {
            get
            {
                switch (Shape)
                {
                    case FootprintShape.Disc: return Origin + Forward * Reach;
                    case FootprintShape.Lane:
                    case FootprintShape.Path: return Origin + Forward * (Reach * 0.5f);
                    default: return Origin;
                }
            }
        }

        /// <summary>The furthest point straight ahead it reached.</summary>
        public Vector3 Far => Origin + Forward * (Shape == FootprintShape.Disc ? Reach + Radius : Reach);

        /// <summary>The shape a move covers, read from its numbers in the order the weapon code strikes them.</summary>
        public static Footprint Of(AttackStep step, Vector3 origin, Vector3 forward)
        {
            var print = new Footprint { Origin = origin, Forward = forward.sqrMagnitude > 0.0001f ? forward.normalized : Vector3.forward };
            if (step == null) return print;
            if (step.Shot != null)
            {
                print.Shape = FootprintShape.Path;
                print.Reach = step.Shot.Range > 0f ? step.Shot.Range : 9f;
                print.Width = Mathf.Max(0.6f, step.Shot.Radius * 2f);
            }
            else if (step.Travel > 0f)
            {
                print.Shape = FootprintShape.Path;
                print.Reach = step.Travel + step.Reach;
                print.Width = Mathf.Max(1f, step.LineWidth);
            }
            else if (step.Radius > 0f)
            {
                print.Shape = FootprintShape.Disc;
                print.Reach = step.Reach;
                print.Radius = step.Radius;
            }
            else if (step.LineWidth > 0f)
            {
                print.Shape = FootprintShape.Lane;
                print.Reach = step.Reach;
                print.Width = step.LineWidth;
            }
            else
            {
                print.Shape = step.ArcDegrees >= 300f ? FootprintShape.Ring : FootprintShape.Arc;
                print.Reach = step.Reach;
                print.ArcDegrees = step.ArcDegrees;
            }
            return print;
        }
    }

    public enum FootprintShape
    {
        Arc,
        Lane,
        Disc,
        Ring,
        Path,
    }
}

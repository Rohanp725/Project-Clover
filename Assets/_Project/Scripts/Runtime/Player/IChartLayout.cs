using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// Where a chart draws its marks (IMPLEMENTATION.md 4.2): the menu takes every place from a layout instead of asking
    /// the ring's geometry itself, so the ring, the duo branches' heart (task 4.5) and the Way (task 5.5) can each lay out
    /// their own marks. Places are in steps; the menu decides how many pixels a step is.
    /// </summary>
    public interface IChartLayout
    {
        /// <summary>True when the mark is drawn on this chart.</summary>
        bool Draws(Upgrade mark);

        /// <summary>Where the mark lies on the chart.</summary>
        Vector2 Place(Upgrade mark);

        /// <summary>Where a line to a mark that grows from nothing on the chart starts: the foot of its tree or branch.</summary>
        Vector2 Foot(Upgrade mark);
    }

    /// <summary>
    /// The ring as it is: eleven trees round one middle, each mixed branch not reshaped yet between its two elements
    /// (<see cref="UpgradeTree.OnChart(Upgrade)"/>). The trees written as spell branches are drawn on it too, in the
    /// pattern of <see cref="BranchPattern"/>. The duo branches written into the heart are not (<see cref="HeartLayout"/>):
    /// their gap on the ring goes empty. Stateless, so one instance serves every menu.
    /// </summary>
    public sealed class RingLayout : IChartLayout
    {
        public static readonly RingLayout Instance = new RingLayout();

        /// <summary>Hidden marks (the reactions), the heart's duo branches and the Way's and Body's marks are not on the ring.</summary>
        public bool Draws(Upgrade mark) => mark != null && !mark.IsHidden && mark.Tree == MarkTree.Element;

        public Vector2 Place(Upgrade mark) => UpgradeTree.OnChart(mark);

        public Vector2 Foot(Upgrade mark) =>
            mark.IsMix ? UpgradeTree.MixFoot(mark.Element, mark.Element2) : UpgradeTree.Emblem(mark.Element);
    }
}

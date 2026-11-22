using Clover.Player.Trees.Ways;

namespace Clover.Player.Way
{
    /// <summary>The four families the eleven elements fall into for the infused strike (steel/weapons.md 0.4).</summary>
    public enum InfusionFamily
    {
        None,
        /// <summary>Fire, Poison, Life: the cut lies on the floor.</summary>
        Ground,
        /// <summary>Lightning, Wind, Light: the strike runs down a line.</summary>
        Line,
        /// <summary>Earth, Metal, Ice: the strike settles in, slow and unbroken.</summary>
        Weight,
        /// <summary>Water, Dark: a second cut follows and drags.</summary>
        Draw,
    }

    /// <summary>
    /// What a family mark did to one infused strike: the numbers the engine reads, the parts already in play applied to
    /// the carrier's copy by <see cref="Infusion.Bend"/>, the rest kept here for the engine pieces still to come.
    /// </summary>
    public struct InfusionBend
    {
        /// <summary>The family whose mark bent the strike, or None.</summary>
        public InfusionFamily Family;
        /// <summary>The mark that bent it, or null.</summary>
        public string Mark;
        /// <summary>True when the deep strike all in doubled the family's length, time or pull.</summary>
        public bool Doubled;
        /// <summary>Lying Draw: seconds the cut lies on the floor in the element. Zero: it does not.</summary>
        public float LieSeconds;
        /// <summary>Lying Draw with Fire: the whole cut alight, not only patches along it (way_build_plan.md question 6).</summary>
        public bool WholeCut;
        /// <summary>Running Draw: metres of the dash down the lane. Zero: no dash.</summary>
        public float Dash;
        /// <summary>Running Draw with Lightning: seconds the dash's path stays charged, giving Static to whoever crosses it. Zero: none.</summary>
        public float ChargedPath;
        /// <summary>Settling Draw: the cut stops in the first enemy and sets the element in it.</summary>
        public bool StopsInFirst;
        /// <summary>Dragging Draw: the cut that follows by itself, after <see cref="FollowDelay"/>, dragging what the draw struck <see cref="Drag"/> metres onto the point.</summary>
        public AttackStep Follow;
        public float FollowDelay;
        public float Drag;
    }

    /// <summary>
    /// Infusion (steel/weapons.md 0.4; way_build_plan.md 2.2; task 5.3): the infused strike (L2 with R1 or R2) plays the
    /// weapon's carrier with an element on it. Four family marks on each Way's trunk, 2 Memories each and opened at the
    /// first god, bend the carrier for every element of one family, while the weapon is carried; all in on one element,
    /// the deep strike doubles the family's length, time or pull. The bend works on the carrier's copy, after the
    /// element's own (<c>ElementSteel.Bend</c>), so it never reaches the chain.
    ///
    /// The Katana's four, on the drawing cut: Lying Draw lays the arc on the floor 3 s (6 s doubled), with Fire the whole
    /// cut alight for twice as long (question 6, the owner's answer); Running Draw makes it a 5 m dash (10 m) through
    /// every enemy on the lane; Settling Draw slows its wind-up to 0.32 s, unbroken, +60% stagger, stopping in the first
    /// enemy; Dragging Draw has the return cut follow by itself 0.15 s later, dragging what the draw struck 1.5 m (3 m).
    /// Plain and stateless.
    /// </summary>
    public static class Infusion
    {
        public const float LieSeconds = 3f;
        /// <summary>Lying Draw with Fire lies twice as long (question 6).</summary>
        public const float FireLieShare = 2f;
        public const float Dash = 5f;
        public const float SettleWindup = 0.32f;
        public const float SettleStagger = 1.6f;
        /// <summary>Poise past any blow: the settling wind-up cannot be broken.</summary>
        public const float Unbroken = 9999f;
        /// <summary>(P) Seconds after the draw's blow that the return cut follows.</summary>
        public const float FollowDelay = 0.15f;
        public const float Drag = 1.5f;
        /// <summary>(P) Stacks of the element Settling Draw sets in the enemy it stops in.</summary>
        public const int SettleStacks = 2;
        /// <summary>All in on one element, the deep strike doubles the family's length, time or pull.</summary>
        public const float AllInShare = 2f;

        /// <summary>The family an element belongs to; None for no element.</summary>
        public static InfusionFamily FamilyOf(string element)
        {
            switch (element)
            {
                case Elements.Fire:
                case Elements.Poison:
                case Elements.Life:
                    return InfusionFamily.Ground;
                case Elements.Lightning:
                case Elements.Wind:
                case Elements.Light:
                    return InfusionFamily.Line;
                case Elements.Earth:
                case Elements.Metal:
                case Elements.Ice:
                    return InfusionFamily.Weight;
                case Elements.Water:
                case Elements.Dark:
                    return InfusionFamily.Draw;
                default:
                    return InfusionFamily.None;
            }
        }

        /// <summary>The family mark of a weapon kind's Way, or null for a Way not written yet.</summary>
        public static string MarkOf(string kind, InfusionFamily family)
        {
            if (kind != Weapons.KatanaName) return null;
            switch (family)
            {
                case InfusionFamily.Ground: return KatanaWay.InfuseGround;
                case InfusionFamily.Line: return KatanaWay.InfuseLine;
                case InfusionFamily.Weight: return KatanaWay.InfuseWeight;
                case InfusionFamily.Draw: return KatanaWay.InfuseDraw;
                default: return null;
            }
        }

        /// <summary>
        /// Bends the carrier's copy by the family mark in force for the element: the parts the engine plays now are set on
        /// <paramref name="carrier"/> (Running Draw's dash through the lane; Settling Draw's slower, unbroken draw and its
        /// stagger), the rest returned. <paramref name="weapon"/> gives the return cut that Dragging Draw follows with.
        /// Nothing at all when no family mark of the weapon's Way is in force for the element.
        /// </summary>
        public static InfusionBend Bend(AttackStep carrier, Weapon weapon, string element, bool deep, bool allIn, IMarkQuery marks)
        {
            var bend = new InfusionBend();
            if (carrier == null || weapon == null || marks == null) return bend;
            var family = FamilyOf(element);
            string mark = MarkOf(WayRules.KindOf(weapon.Name), family);
            if (mark == null || !marks.Has(mark)) return bend;
            bend.Family = family;
            bend.Mark = mark;
            bend.Doubled = deep && allIn;
            float share = bend.Doubled ? AllInShare : 1f;
            switch (family)
            {
                case InfusionFamily.Ground:
                    bool fire = element == Elements.Fire;
                    bend.WholeCut = fire;
                    bend.LieSeconds = LieSeconds * (fire ? FireLieShare : 1f) * share;
                    break;
                case InfusionFamily.Line:
                    // The element's own strike may already run (Lightning's Flash Cut, 5 m): the longer of the two.
                    bend.Dash = Dash * share;
                    carrier.Travel = UnityEngine.Mathf.Max(carrier.Travel, bend.Dash);
                    carrier.PassThrough = true;
                    // With Lightning the path stays charged (the owner's answer, 2026-11-21): Flash Cut alone is the same dash.
                    if (element == Elements.Lightning) bend.ChargedPath = InfusionGround.ChargedSeconds;
                    break;
                case InfusionFamily.Weight:
                    // The whole step slowed by one share, so the blow still lands on its clip's frame (way_build_plan.md
                    // 1.2 item 8); NEEDS: Settling Draw's own clip with the carrier's recovery.
                    if (carrier.Windup > 0f && carrier.Windup < SettleWindup)
                    {
                        float slower = SettleWindup / carrier.Windup;
                        carrier.Windup = SettleWindup;
                        carrier.Active *= slower;
                        carrier.Recovery *= slower;
                    }
                    carrier.Poise = Unbroken;
                    carrier.Stagger *= SettleStagger;
                    // The cut stops in the first enemy it meets and sets the element in it.
                    carrier.FirstOnly = true;
                    carrier.ElementStacks += SettleStacks;
                    bend.StopsInFirst = true;
                    break;
                case InfusionFamily.Draw:
                    var steps = weapon.Combo != null ? weapon.Combo.Steps : null;
                    if (steps != null && steps.Count > 1)
                    {
                        bend.Follow = steps[1].Clone();
                        bend.Drag = Drag * share;
                        bend.Follow.Pull = bend.Drag;
                        // It comes by itself, part of the strike already paid for.
                        bend.Follow.StaminaCost = 0f;
                        bend.FollowDelay = FollowDelay;
                    }
                    break;
            }
            return bend;
        }
    }
}

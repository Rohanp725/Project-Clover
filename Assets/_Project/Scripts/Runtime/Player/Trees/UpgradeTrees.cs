using System.Collections.Generic;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Gathers every tree file into the one catalogue. A new tree is a new file beside this one with a static
    /// <c>Add(List&lt;Upgrade&gt;)</c>, and one line here; a test fails if a tree file is left out.
    /// </summary>
    public static class UpgradeTrees
    {
        public static void AddAll(List<Upgrade> to)
        {
            // Round the ring from straight up; each mix after the first of its two elements.
            FireTree.Add(to);
            PlasmaTree.Add(to);
            LightningTree.Add(to);
        }
    }
}

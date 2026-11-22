using Clover.Gameplay;

namespace Clover.Player.Forms
{
    /// <summary>Who a spell that seeks the afflicted takes (the ledger's afflicted-only and reach-only-afflicted), apart from the forms so it can be checked without a scene.</summary>
    public static class SpellReach
    {
        /// <summary>True when nothing is sought, or the target carries the first kind and the second as well when one is named.</summary>
        public static bool Takes(object target, AfflictionKind kind, AfflictionKind kind2) =>
            PlayerCombat.Carries(target, kind) && PlayerCombat.Carries(target, kind2);
    }
}

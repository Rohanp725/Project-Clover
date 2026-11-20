namespace Clover.Player
{
    /// <summary>What sort of mark an upgrade is on the chart. The kind sets its usual cost and the shape of its frame.</summary>
    public enum UpgradeKind
    {
        /// <summary>The foot of a tree. Costs 2. Turns the element's affliction on for blows of the weapon.</summary>
        Root,
        /// <summary>A plain number. Costs 1. Small and round. A spell branch's Empower is one.</summary>
        Minor,
        /// <summary>A new rule: something happens on a trigger. Costs 3. Square. A spell branch's Shape and Master are ones, and so is a reaction lift.</summary>
        Major,
        /// <summary>A spell cast from a slot: the Learn of a spell branch. Costs 4. A diamond.</summary>
        Spell,
        /// <summary>The end of a bough. Costs 6. Large.</summary>
        Keystone,
        /// <summary>A mark on the branch two neighbouring elements share. Costs 3 to 5, as written on the mark.</summary>
        Mix,
        /// <summary>What a god leaves. Never sold. The largest, with a crown.</summary>
        Signature,
        /// <summary>A pair's plain reaction (IMPLEMENTATION.md 2.4): one hidden mark per pair, never drawn or sold, always in force; its effects ride <see cref="UpgradeTrigger.Reacted"/>.</summary>
        Reaction,
        /// <summary>An element's all-in passive (STEEL_AND_ELEMENT.md 5.6): in force only while the same element is carried twice. Costs 6. Drawn as a keystone.</summary>
        Devotion,
        /// <summary>A mark that breaks a standard rule (phase 6): given, or 8 Shards for an element's deep Heresy.</summary>
        Heresy,
        /// <summary>A spell changed by its partner (phase 6): hidden until found, then 5 Shards.</summary>
        Evolution,
        /// <summary>A bough of a weapon's Way (phase 5): 2 Memories.</summary>
        Aspect,
        /// <summary>A rank of the Way's temper (phase 5): 1, 1, 2, 2, 3 Memories, written on each mark.</summary>
        Temper,
        /// <summary>A gated technique on the R1 and R2 chord (phase 5): 2 Memories once its elite has fallen.</summary>
        Technique,
        /// <summary>What Bond V gives (phase 6). Never sold.</summary>
        Bond,
    }
}

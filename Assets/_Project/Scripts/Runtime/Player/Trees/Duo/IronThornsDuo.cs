using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Life and Metal (pairs_d1_d2.md, "Life and Metal", distance 2; task 4.15), named by its reaction,
    /// Iron Thorns. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark,
    /// Graft, opens Pruning Hook and Iron Harvest; Sap of Iron and Ore in the Root (majors), Grafted Edge (a minor), and the
    /// duo spell The Sown Men, its pair's larger spell paid in mana. Life is written first: the duo spell is cast
    /// in Life. The pair file gives this branch no keystone.
    /// </summary>
    public static class IronThornsDuo
    {
        public const string Shape = "ironthorns.shape";
        public const string SapOfIron = "ironthorns.sap";
        public const string OreInTheRoot = "ironthorns.ore";
        public const string GraftedEdge = "ironthorns.grafted";
        public const string TheSownMen = "ironthorns.sownmen";
        /// <summary>The conditions the two shapes set, each the thorns as the plain reaction's.</summary>
        public const string Pruned = "ironthorns.pruned";
        public const string Harvest = "ironthorns.harvest";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Life, B = Elements.Metal;

            // NEEDS: thorns as separate blades, five flying out at once (Iron Harvest; now one ring of 8 to all within 3 m).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Iron Thorns: Pruning Hook or Iron Harvest", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Iron Thorns' two shapes, chosen at a rest fire. Pruning Hook: your blows prune the thorns and each heals you. Iron Harvest: the thorns all burst out together at the end. Either runs instead of the plain Iron Thorns.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Pruning Hook",
                        Description = "Iron thorns grow through its armour for 6 s; your weapon blows prune them rather than throw them, three in all, and each one cut heals you 4.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Pruned, Seconds = 6f, Blows = 3, Is = Reactions.Thorns } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Pruned, By = StruckBy.Weapon, Do = UpgradeAction.Heal, Amount = 4f },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Iron Harvest",
                        Description = "Five thorns grow and none is thrown: they all burst out together when it dies or the 6 s end, a ring of blades to 3 m, 8 and a notch on each enemy.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Harvest, Seconds = 6f, Is = Reactions.Thorns } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Harvest, Do = UpgradeAction.Burst, Radius = 3f, Damage = 8f, Stagger = 8f, Affliction = AfflictionKind.Sundered, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionDied, Condition = Harvest, Do = UpgradeAction.Burst, Radius = 3f, Damage = 8f, Stagger = 8f, Affliction = AfflictionKind.Sundered, Stacks = 1 },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // A kill of an enemy with Iron Thorns' thorns in it, in whichever shape, read from what it carried as it died
            // (UpgradeEffect.InState).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = SapOfIron, Name = "Sap of Iron", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Flask,
                Description = "An enemy that dies with thorns in it gives its iron to the ground and the ground to you: a quarter of a flask fills at once.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, InState = Reactions.Thorns, Do = UpgradeAction.FlaskFill, Amount = 0.25f } },
            }, HeartPattern.LeftLow));

            // Every grown thing lays a notch when it bites (Hazards.GrownBitesMask, set by PlayerUpgrades while this is owned).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = OreInTheRoot, Name = "Ore in the Root", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Leaf,
                Description = "Things you grow draw iron from the ground: each grown thing lays a notch of Sundered when it bites.",
            }, HeartPattern.RightLow));

            // A notch laid on a Seeded enemy lays one more (UpgradeTrigger.AfflictionLaid; the second is not answered again).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = GraftedEdge, Name = "Grafted Edge", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { SapOfIron }, Icon = UpgradeIcons.Blade,
                Description = "A notch laid on a Seeded enemy lays two.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictionLaid, Spent = AfflictionKind.Sundered, Needs = AfflictionKind.Seeded, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Sundered, Stacks = 1 } },
            }, HeartPattern.LeftHigh));

            // The duo spell, bought with Shards once both are carried (no god gives it): its pair's larger spell, 45 mana (SpellTimings). The minions form sends helpers that
            // run at enemies and strike them; each strike seeds (the branch's first element), and one in three notches.
            // Each heavy blow you land is answered at once by every sown man on that same enemy (SpellSpec.AnswersHeavy).
            // NEEDS: the teeth thrown in a fan and the warriors rising 0.8 s later (form-pieces).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheSownMen, Name = "The Sown Men", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Opener, Requires = new[] { SapOfIron, OreInTheRoot }, Icon = UpgradeIcons.Fist,
                Description = "You throw five teeth ahead; they sink, and warriors of bronze and root rise from the soil. Each goes to the nearest enemy and strikes it again and again, seeding it, and every third strike notches. After 6 s they crumble to rust and leaves. 45 mana.",
                Form = SpellForms.Minions,
                Spell = new SpellSpec { Cost = 45f, Count = 5, Seconds = 6f, Damage = 8f, Stagger = 4f, Stacks = 1, Interval = 0.7f, AnswersHeavy = true },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Sundered, Stacks = 1, Chance = 0.34f } },
            }, HeartPattern.Spell));
        }
    }
}

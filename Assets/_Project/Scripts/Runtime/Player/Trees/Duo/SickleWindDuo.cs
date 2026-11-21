using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Wind and Metal (pairs_d5.md, "Wind and Metal", distance 5; task 4.17), named by its reaction,
    /// Sickle Wind. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark,
    /// Loose Plates, opens Sent Downwind and Cut Unfelt; Bellows (a major), Grit (a minor), the keystone Wind Sickle, and the
    /// duo spell Three Weasels. Wind is written first, as the ring names the pair.
    /// </summary>
    public static class SickleWindDuo
    {
        public const string Shape = "sicklewind.shape";
        public const string Bellows = "sicklewind.bellows";
        public const string Grit = "sicklewind.grit";
        public const string WindSickle = "sicklewind.windsickle";
        public const string ThreeWeasels = "sicklewind.weasels";
        /// <summary>The condition Cut Unfelt sets: the debt of cuts, paid when the whirl ends.</summary>
        public const string Unfelt = "sicklewind.unfelt";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Wind, B = Elements.Metal;

            // NEEDS: thrown-from-enemy (Sent Downwind's whirl wandering 6 m down the way the enemy was last shoved, at 2 m a
            // second; now three patches laid at once on your line beyond it, and each enemy there notched once);
            // damage-tally (Cut Unfelt's cuts counted and paid back at half again; now a set debt and 6 more for each of your
            // weapon blows on it meanwhile).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Sickle Wind: Sent Downwind or Cut Unfelt", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Sickle Wind's two shapes, chosen at a rest fire. Sent Downwind: the whirl leaves its enemy and wanders down the last wind, cutting and notching all it passes. Cut Unfelt: the cuts draw nothing until the whirl ends, then open all at once. Either runs instead of the plain Sickle Wind.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Sent Downwind",
                        Description = "The whirl of torn plates leaves its enemy and wanders 6 m down the last wind (the way it was last shoved, else away from you) at 2 m a second, cutting everything it passes for 3 and 4 stagger every 0.4 s and breaking a notch of armour on each.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Shape = HazardShape.Downwind, Count = 3, Reach = 3f, Amount = 2f, Radius = 1f, Seconds = 3f, Interval = 0.4f,
                                Damage = 3f, Stagger = 4f, Affliction = AfflictionKind.Sundered },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Shape = HazardShape.Downwind, Count = 3, Reach = 3f, Amount = 2f, Radius = 1f, Delay = 0.5f,
                                Affliction = AfflictionKind.Sundered, Stacks = 1 },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Cut Unfelt",
                        Description = "The shards draw nothing at first: each cut leaves a thin pale line on the body. When the whirl ends after 3 s they all open at once for half again what they owed, and every weapon blow you land on it in those 3 s adds to the debt.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Unfelt, Seconds = 3f } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Unfelt, Do = UpgradeAction.Burst, Radius = 1.5f, Damage = 32f, Stagger = 28f, Affliction = AfflictionKind.Sundered },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Unfelt, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 6f, PerStack = true, Affliction = AfflictionKind.Sundered },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // NEEDS: a dodge ending near a whirl feeding it a shard and its next cut laying Unsteady (whirl-fed); now a
            // dodge's end lays an Unsteady on everything within 3 m of you, once in 2 s.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Bellows, Name = "Bellows", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Boot,
                Description = "A dodge that ends within 3 m of a whirl feeds it: one more shard, and its next cut lays Unsteady.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, At = EffectAt.Self, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Unsteady, Stacks = 1, Cooldown = 2f } },
            }, HeartPattern.LeftLow));

            // Changes its reaction's own numbers while it is in force (reaction-numbers, ReactionNumbers):
            // one more shard's cut each bite and the whirl a second longer (Sent Downwind: one more patch, a second longer; Cut Unfelt: its debt a second longer).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Grit, Name = "Grit", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { Shape }, Icon = UpgradeIcons.Hourglass,
                Description = "Sickle Wind's whirl has one more shard and lasts 1 s longer.",
                ReactionChanges = new[]
                {
                    new ReactionChange { Shape = -1, Index = 1, Damage = 6f, Count = 10 },
                    new ReactionChange { Shape = 0, Index = 0, Seconds = 4f, Count = 4 },
                    new ReactionChange { Shape = 0, Index = 1, Count = 4 },
                    new ReactionChange { Shape = 1, Index = 0, ConditionSeconds = 4f },
                },
            }, HeartPattern.RightLow));

            // The sickle of wind: a narrow cone 5 m along the swing from the last blow of a chain.
            // NEEDS: a notch torn from a Sundered enemy into a shard that joins the nearest whirl (whirl-fed); now only
            // the sickle's cut.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = WindSickle, Name = "Wind Sickle", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { Bellows, Grit }, Icon = UpgradeIcons.Claw,
                Description = "The last blow of a light chain throws a sickle of wind 5 m along the swing: 6 damage and an Unsteady, and a notch torn from a Sundered enemy becomes a shard that joins the nearest whirl.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.ChainEnd, Do = UpgradeAction.Cone, Reach = 5f, Arc = 30f, Damage = 6f, Stagger = 6f, Affliction = AfflictionKind.Unsteady, Stacks = 1 } },
            }, HeartPattern.Keystone));

            // The duo spell, a movement on a 10 s cooldown (SpellTimings), bought with 5 Shards once both are carried. Three
            // presses within 4 s (Upgrade.Presses, second-press). The rush form is the first weasel: a rush that stops just in
            // front of the enemy faced (the owner's answer of 2026-11-20; the step form went 1 m through it), unsteadies it
            // (the branch's first element) two deep, throws it 2 m, and starts the count of blows again. The second press is
            // the pass straight through it and out 3 m beyond, untouchable, for 12 and a notch; the third the step back that
            // binds 2 health for each weapon blow landed since the first press, eight at most.
            // NEEDS: the first weasel's own look, a gust that runs ahead of the player and a sickle cut in front of the enemy
            // (looks); now the step's streak and cut, stopped short (RushForm).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = ThreeWeasels, Name = "Three Weasels", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Movement, Requires = new[] { Bellows, Grit }, Icon = UpgradeIcons.Wing,
                Description = "Three presses within 4 s, as the three sickle weasels work together: a rush of wind to the enemy you face that throws it, a pass straight through it that notches its armour, and a step back out of reach that binds your wounds.",
                Form = SpellForms.Rush,
                Spell = new SpellSpec { Reach = 5f, Damage = 6f, Stagger = 20f, Stacks = 2, Seconds = 2f },
                Tally = new TallySpec { Name = "Wounds", Cap = 8 },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnSpell = true, Do = UpgradeAction.ClearCount },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Push, Shape = HazardShape.Downwind, Amount = 2f },
                    // Each weapon blow landed counts toward the third weasel's binding, eight at most.
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Bank, Count = 1 },
                },
                PressWindow = 4f,
                Presses = new[]
                {
                    // The second weasel: straight through the enemy faced to 3 m beyond, untouchable, 12 damage and a notch.
                    new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, Do = UpgradeAction.Untouchable, Seconds = 0.4f },
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, Do = UpgradeAction.Burst, Radius = 1f, Damage = 12f, Stagger = 10f, Affliction = AfflictionKind.Sundered, Stacks = 1 },
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, Do = UpgradeAction.Step, Reach = 6f, Amount = 3f },
                    },
                    // The third: a step back out of reach that binds the wounds, 2 health for each weapon blow since the first.
                    new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, Do = UpgradeAction.StepBack, Reach = 4f },
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, SpendsAll = true, PerStack = true, Do = UpgradeAction.Heal, Amount = 2f },
                    },
                },
            }, HeartPattern.Spell));
        }
    }
}

using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Poison and Wind (pairs_d3.md, "Poison and Wind", distance 3; task 4.16), named by its reaction,
    /// Ill Wind. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark opens
    /// Dust on the Blade and Dust Bank; Spring Dust and Gust at the Heel (majors), Fine Dust (a minor), the keystone No Still
    /// Air, and the duo spell The Yellow Gale. Poison is written first, as the ring names the pair.
    /// </summary>
    public static class IllWindDuo
    {
        public const string Shape = "illwind.shape";
        public const string SpringDust = "illwind.springdust";
        public const string GustAtTheHeel = "illwind.gustheel";
        public const string FineDust = "illwind.finedust";
        public const string NoStillAir = "illwind.nostillair";
        public const string TheYellowGale = "illwind.yellowgale";
        /// <summary>The conditions the two shapes set (each the loose dust as the plain reaction's), the coat Dust on the
        /// Blade leaves, and No Still Air's restless wind.</summary>
        public const string BladeDust = "illwind.bladedust";
        public const string Coat = "illwind.coat";
        public const string BankDust = "illwind.bankdust";
        public const string Restless = "illwind.restless";
        private const string Reaction = "react.illwind";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Poison, B = Elements.Wind;

            // NEEDS: every shove and throw shedding (shove-moment; now a heavy blow, as the plain reaction); Dust on the Blade's
            // coat on your own edge for your next three light blows on any enemy (depth-moved; now the next three weapon blows on the one
            // shed); Dust Bank's trail drifting on 2 m a second (plume-along-path; now a longer line of Unsteady ground laid
            // at once downwind).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Ill Wind: Dust on the Blade or Dust Bank", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Ill Wind's two shapes, chosen at a rest fire. Dust on the Blade: no trail; the grit coats your edge instead, and your next blows dose and unsteady. Dust Bank: the trail drifts on as a bank of dust that makes Unsteady what it passes. Either runs instead of the plain Ill Wind.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Dust on the Blade",
                        Description = "Its poison goes loose as before, but your heavy blow sheds it with no trail: the grit coats your edge, and your next three weapon blows on it each lay a dose and an Unsteady.",
                        Effects = new[]
                        {
                            Loose(BladeDust),
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = BladeDust, By = StruckBy.Heavy, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Poison },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = BladeDust, By = StruckBy.Heavy, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Coat, Seconds = 6f, Blows = 3 } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Coat, By = StruckBy.Weapon, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Poison, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Coat, By = StruckBy.Weapon, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Unsteady, Stacks = 1 },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Dust Bank",
                        Description = "Its poison goes loose and your heavy blow sheds it in a plume behind it, as before; and the dust does not settle but runs on 8 m the way it was thrown, a low bank for 4 s that makes Unsteady what it passes.",
                        Effects = new[]
                        {
                            Loose(BankDust),
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = BankDust, By = StruckBy.Heavy, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Poison },
                            Plume(UpgradeTrigger.ConditionStruck, BankDust, 3f),
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = BankDust, By = StruckBy.Heavy, Do = UpgradeAction.Hazard, Shape = HazardShape.Downwind, Count = 5,
                                Reach = 8f, Amount = 1.2f, Radius = 0.6f, Seconds = 4f, Interval = 1f, Affliction = AfflictionKind.Unsteady, Stacks = 1, SparesTarget = true },
                        },
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = SpringDust, Name = "Spring Dust", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Hammer,
                Description = "Heavy blows dose what they shove, and make it Unsteady.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Poison, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Unsteady, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            // NEEDS: the attacker of the dodged blow alone (attacker-of-turned-blow); now everything within 2.5 m of you is thrown.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = GustAtTheHeel, Name = "Gust at the Heel", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Boot,
                Description = "A perfect dodge leaves a gust behind you that throws the attacker 3 m, so a poisoned attacker trails at once.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, At = EffectAt.Self, Do = UpgradeAction.Push, Radius = 2.5f, Amount = 3f, Cooldown = 0.5f } },
            }, HeartPattern.RightLow));

            // Changes its reaction's own numbers while it is in force (reaction-numbers, ReactionNumbers):
            // the trails lasting 2 s longer.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = FineDust, Name = "Fine Dust", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { SpringDust }, Icon = UpgradeIcons.Hourglass,
                Description = "Ill Wind's trails last 2 s longer.",
                ReactionChanges = new[]
                {
                    new ReactionChange { Shape = -1, Index = 2, Seconds = 5f },
                    new ReactionChange { Shape = 1, Index = 2, Seconds = 5f },
                    new ReactionChange { Shape = 1, Index = 3, Seconds = 6f },
                },
            }, HeartPattern.LeftHigh));

            // The restless wind: while an enemy carries both after Ill Wind fires, every 2 s it is shoved 1 m away from you and
            // sheds its poison in a short plume. NEEDS: the shove itself setting off the shed (shove-moment).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = NoStillAir, Name = "No Still Air", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { SpringDust, GustAtTheHeel }, Icon = UpgradeIcons.Gust,
                Description = "A poisoned, Unsteady enemy cannot stand still: every 2 s the wind shoves it 1 m away from you, and it sheds a trail each time.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, Do = UpgradeAction.Condition,
                        State = new ConditionSpec { Id = Restless, Seconds = 30f, Beat = 2f, While = AfflictionKind.Poison, While2 = AfflictionKind.Unsteady } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Restless, Do = UpgradeAction.Push, Shape = HazardShape.Downwind, Amount = 1f },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Restless, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Poison },
                    Plume(UpgradeTrigger.ConditionBeat, Restless, 2f),
                },
            }, HeartPattern.Keystone));

            // The duo spell, bought with Shards once both are carried (no god gives it): a detonator on a 12 s cooldown (SpellTimings). The wave form throws the gale, a cone 9 m
            // long and 60 degrees wide; it doses (the branch's first element), throws 4 m, unsteadies three deep and bites.
            // NEEDS: the held press drinking for 1.5 s (held-press), every cloud, trail and half of each poisoned enemy's doses
            // within 6 m drunk into a store and shared among those the gale strikes (depth-moved), and two more doses for each
            // weapon blow in the 4 s before the cast.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheYellowGale, Name = "The Yellow Gale", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Detonator, Requires = new[] { SpringDust, GustAtTheHeel }, Icon = UpgradeIcons.Wave,
                Description = "The wind turns round you and goes out as a gale of yellow grit, a cone 9 m long. Everything in it is thrown 4 m, made Unsteady three deep, dosed, and its poison bites at once.",
                Form = SpellForms.Wave,
                Spell = new SpellSpec { Reach = 9f, Arc = 60f, Damage = 8f, Stagger = 20f, Stacks = 2 },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Unsteady, Stacks = 3 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Push, Shape = HazardShape.Downwind, Amount = 4f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Bite, Delay = 0.2f },
                },
            }, HeartPattern.Spell));
        }

        // The loose dust as the plain Ill Wind's: it holds while the enemy carries both.
        private static UpgradeEffect Loose(string id) => new UpgradeEffect
        {
            On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition,
            State = new ConditionSpec { Id = id, Seconds = 30f, While = AfflictionKind.Poison, While2 = AfflictionKind.Unsteady },
        };

        // The plume as the plain Ill Wind's (doses once a second to all in it but the one that shed it), on a heavy blow when
        // answering a blow, or on the condition's beat; so many seconds long.
        private static UpgradeEffect Plume(UpgradeTrigger on, string id, float seconds) => new UpgradeEffect
        {
            On = on, Condition = id, By = on == UpgradeTrigger.ConditionStruck ? StruckBy.Heavy : StruckBy.Any, Do = UpgradeAction.Hazard, Shape = HazardShape.Downwind, Count = 3,
            Reach = 2.2f, Amount = 1.2f, Radius = 0.6f, Seconds = seconds, Interval = 1f, Affliction = AfflictionKind.Poison, Stacks = 1, SparesTarget = true,
        };
    }
}

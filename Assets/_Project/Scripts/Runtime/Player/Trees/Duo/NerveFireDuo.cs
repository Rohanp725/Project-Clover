using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Poison and Lightning (pairs_d1_d2.md, "Poison and Lightning", distance 2; task 4.15), named by its
    /// reaction, Nerve Fire. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its
    /// Shape mark, Struck Nerve, opens Ozone and Lockjaw; Strike on the Beat and Five Thunders, the keystone Hidden Crimes,
    /// and the duo spell Paper Seal, the crown. Poison is written first, as the ring names the pair: the duo spell is cast in
    /// Poison.
    /// </summary>
    public static class NerveFireDuo
    {
        public const string Shape = "nervefire.shape";
        public const string StrikeOnTheBeat = "nervefire.beat";
        public const string FiveThunders = "nervefire.fivethunders";
        public const string HiddenCrimes = "nervefire.hiddencrimes";
        public const string PaperSeal = "nervefire.seal";
        /// <summary>The conditions the two shapes set, each the nerves as the plain reaction's.</summary>
        public const string Ozone = "nervefire.ozone";
        public const string Lockjaw = "nervefire.lockjaw";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Poison, B = Elements.Lightning;

            // NEEDS: the jolt's own look on a shape's nerves (the plain reaction's small jolts over the body are drawn by its
            // condition's name; the shapes' jolts show as the burst alone), and per-enemy cooldowns as the plain reaction.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Nerve Fire: Ozone or Lockjaw", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Nerve Fire's two shapes, chosen at a rest fire. Ozone: each jolt lets off a puff of sharp blue mist that poisons and charges those near. Lockjaw: the third jolt locks it in place. Either runs instead of the plain Nerve Fire.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Ozone",
                        Description = "While both last, each bite spends a Static and jolts it as before, 7 and a flinch, and each jolt lets off a puff of sharp blue mist 2 m across that lays a dose and a Static on the enemies near.",
                        Effects = Nerves(Ozone, new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Ozone, Do = UpgradeAction.Afflict, Radius = 1f, Affliction = AfflictionKind.Poison, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Ozone, Do = UpgradeAction.Afflict, Radius = 1f, Affliction = AfflictionKind.Static, Stacks = 1 },
                        }),
                    },
                    new ShapeDef
                    {
                        Name = "Lockjaw",
                        Description = "While both last, each bite spends a Static and jolts it as before, 7 and a flinch; the third jolt locks it: held 1.2 s.",
                        Effects = Nerves(Lockjaw, new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Lockjaw, AtCount = 3, Do = UpgradeAction.Hold, Seconds = 1.2f },
                        }),
                    },
                },
            }, HeartPattern.Foot));

            // NEEDS: the moment of a bite (within 0.3 s of its bite; now any heavy blow on a poisoned, charged enemy).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = StrikeOnTheBeat, Name = "Strike on the Beat", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Hourglass,
                Description = "A heavy blow that lands on the beat of its poison discharges all its Static and lays as many doses.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Static, Needs2 = AfflictionKind.Poison, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Static, CountOnly = true, Cooldown = 0.5f },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Static, Needs2 = AfflictionKind.Poison, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Poison, Stacks = 1, PerStack = true, Cooldown = 0.5f },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Static, Needs2 = AfflictionKind.Poison, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 4f, Stagger = 5f, Cooldown = 0.5f },
                },
            }, HeartPattern.LeftLow));

            // NEEDS: a count of bites kept on each enemy (every fifth; now one bite in five by chance).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = FiveThunders, Name = "Five Thunders", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Bolt,
                Description = "Every fifth bite of poison on an enemy carrying Static calls a bolt down on it: 14 and a Static, leaping to two more.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PoisonBite, Needs = AfflictionKind.Static, Chance = 0.2f, Do = UpgradeAction.Arc, Damage = 14f, Count = 3, Reach = 5f, Affliction = AfflictionKind.Static, Stacks = 1 } },
            }, HeartPattern.RightLow));

            // Poison counts as charge: a discharge on a poisoned enemy spends its doses, 3.5 for each (7 for every two).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = HiddenCrimes, Name = "Hidden Crimes", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { StrikeOnTheBeat, FiveThunders }, Icon = UpgradeIcons.Skull,
                Description = "Poison counts as charge: a discharge on a poisoned enemy deals 7 more for every two doses it carries, and spends them.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SetOff, Spent = AfflictionKind.Static, Needs = AfflictionKind.Poison, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Poison, Cooldown = 0.3f },
                    new UpgradeEffect { On = UpgradeTrigger.SetOff, Spent = AfflictionKind.Static, Needs = AfflictionKind.Poison, Do = UpgradeAction.Burst, Radius = 4f, DamagePerSpent = 3.5f, Affliction = AfflictionKind.Static, Cooldown = 0.3f },
                },
            }, HeartPattern.Keystone));

            // The duo spell, the crown: a detonator on a 13 s cooldown (SpellTimings). The brand form sets a sign on the enemy
            // faced that the next weapon blows burn down and set off; it is held as the seal is slapped on.
            // NEEDS: the step to the enemy; its Static discharged and each dose passed to all within 3 m at the thunder; the
            // thunder at half if the 6 s run out first; elites held 0.5 s and gods not at all.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = PaperSeal, Name = "Paper Seal", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Detonator, Requires = new[] { StrikeOnTheBeat, FiveThunders }, Icon = UpgradeIcons.Target,
                Description = "You slap a seal of yellow paper on the brow of the enemy you face and it is held. For 6 s each weapon blow on it burns the seal down a fifth; at the fifth, thunder falls on it.",
                Form = SpellForms.Brand,
                Spell = new SpellSpec { Reach = 3f, Seconds = 6f, Count = 5, Radius = 3.5f, Damage = 30f, Stagger = 50f, Stacks = 1 },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Ranks = EnemyRank.Plain, Do = UpgradeAction.Hold, Seconds = 1.2f, Cooldown = 6f } },
            }, HeartPattern.Spell));
        }

        // The nerves as the plain Nerve Fire's: while both last, each bite spends a Static and jolts it (7, 5 stagger, a 0.3 s
        // flinch; a god 11 stagger and no flinch; an elite's flinch once in 4 s); then the shape's own answers.
        private static UpgradeEffect[] Nerves(string id, UpgradeEffect[] more)
        {
            var effects = new List<UpgradeEffect>
            {
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = id, Seconds = 30f, OnBite = true, While = AfflictionKind.Poison, While2 = AfflictionKind.Static } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = id, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Static, Count = 1 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = id, Ranks = EnemyRank.Plain | EnemyRank.Elite, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 7f, Stagger = 5f, Affliction = AfflictionKind.Static, Unseen = true },
                new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = id, Ranks = EnemyRank.God, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 7f, Stagger = 11f, Affliction = AfflictionKind.Static, Unseen = true },
                new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = id, Ranks = EnemyRank.Plain, Do = UpgradeAction.Flinch, Seconds = 0.3f },
                new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = id, Ranks = EnemyRank.Elite, Cooldown = 4f, Do = UpgradeAction.Flinch, Seconds = 0.3f },
            };
            effects.AddRange(more);
            return effects.ToArray();
        }
    }
}

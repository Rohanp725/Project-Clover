using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Wind and Water (pairs_d1_d2.md, "Wind and Water", distance 2; task 4.15), named by its reaction,
    /// Capsize. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark opens
    /// Waterspout and Breakers; Wake and Rolled In, the keystone Swamped, the duo spell Ride the Squall (the crown) and the duo
    /// surge Open the Gourd. Wind is written first: the duo spell is cast in Wind.
    /// </summary>
    public static class CapsizeDuo
    {
        public const string Shape = "capsize.shape";
        public const string Wake = "capsize.wake";
        public const string RolledIn = "capsize.rolledin";
        public const string Swamped = "capsize.swamped";
        public const string RideTheSquall = "capsize.squall";
        public const string OpenTheGourd = "capsize.gourd";
        /// <summary>The condition Breakers sets: down as the plain Capsize, with the spray thrown behind it.</summary>
        public const string Breakers = "capsize.breakers";
        private const string Reaction = "react.capsize";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Wind, B = Elements.Water;

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Capsize: Waterspout or Breakers", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Capsize's two shapes, chosen at a rest fire. Waterspout: it is lifted in a column of spray instead of going down. Breakers: as it falls it throws a sheet of spray behind it, and what stands there goes over in turn. Either runs instead of the plain Capsize.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Waterspout",
                        Description = "It is lifted in a column of spray for 1.2 s instead, in the air where your blows hold it up. What is too heavy to lift reels instead.",
                        Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Lift, Seconds = 1.2f } },
                    },
                    new ShapeDef
                    {
                        Name = "Breakers",
                        Description = "It goes down as before, and as it falls it throws a sheet of spray 4 m behind it: whatever stands there is Soaked and made 2 deeper Unsteady, and one already Unsteady capsizes in turn, so a line goes over like dominoes.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Ranks = EnemyRank.Plain, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Breakers, Seconds = 1.2f, Flags = ConditionFlags.Down } },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Ranks = EnemyRank.Elite, Do = UpgradeAction.Burst, Radius = 0.5f, Stagger = 25f },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Ranks = EnemyRank.God, Do = UpgradeAction.Burst, Radius = 0.5f, Stagger = 15f },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Shape = HazardShape.Downwind, Count = 2, Reach = 2.5f, Amount = 1.6f, Radius = 1.2f,
                                Affliction = AfflictionKind.Soaked, Stacks = 1, Delay = 0.2f, Cascades = true },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Shape = HazardShape.Downwind, Count = 2, Reach = 2.5f, Amount = 1.6f, Radius = 1.2f,
                                Affliction = AfflictionKind.Unsteady, Stacks = 2, Delay = 0.25f, Cascades = true },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // NEEDS: the dodge's own path (now a short line ahead of where it ends).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Wake, Name = "Wake", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Drop,
                Description = "A perfect dodge leaves a line of spray for 3 s that Soaks and unsteadies what crosses it.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, At = EffectAt.Self, Do = UpgradeAction.Hazard, Shape = HazardShape.Line, Count = 4, Reach = 3f, Radius = 0.8f, Seconds = 3f, Interval = 0.5f, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, At = EffectAt.Self, Do = UpgradeAction.Hazard, Shape = HazardShape.Line, Count = 4, Reach = 3f, Radius = 0.8f, Seconds = 3f, Interval = 1f, Affliction = AfflictionKind.Unsteady, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = RolledIn, Name = "Rolled In", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Wave,
                Description = "An enemy that capsizes is dragged 2 m toward you as it falls, into reach.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, Do = UpgradeAction.Pull, Shape = HazardShape.Downwind, Amount = 2f } },
            }, HeartPattern.RightLow));

            // NEEDS: a stagger share on blows against an enemy in a condition (twice as fast while it is down); now each blow
            // on a soaked, unsteady enemy adds 10 stagger.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Swamped, Name = "Swamped", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { Wake, RolledIn }, Icon = UpgradeIcons.Rock,
                Description = "Blows on a capsized enemy fill its stagger meter twice as fast.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AnyHit, Needs = AfflictionKind.Soaked, Needs2 = AfflictionKind.Unsteady, Do = UpgradeAction.Burst, Radius = 0.5f, Stagger = 10f, Cooldown = 0.2f } },
            }, HeartPattern.Keystone));

            // The duo spell, the crown: a movement on a 9 s cooldown (SpellTimings). The drill form carries the player along a
            // line through everything on it.
            // NEEDS: untouchable for the ride; cast out of a perfect dodge it starts no cooldown and the first blow after it is
            // critical (spell-windows).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = RideTheSquall, Name = "Ride the Squall", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Movement, Requires = new[] { Wake, RolledIn }, Icon = UpgradeIcons.Boot,
                Description = "You are carried 7 m along the stick on a low racing sheet of spray. Every enemy near the line takes 6, is Soaked and made 2 deeper Unsteady, and so capsizes if it was already unsteady.",
                Form = SpellForms.Drill,
                Spell = new SpellSpec { Reach = 7f, Radius = 1.5f, Count = 1, Damage = 6f, Stagger = 6f, Stacks = 2, Speed = 16f },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Soaked, Stacks = 1 } },
            }, HeartPattern.Spell));

            // The duo surge (pairs_d1_d2.md): a gourd of winds let out by each dodge.
            // NEEDS: a duo surge worn in the Hand's surge slot, its own 40 mana and 10 s, and 1 s more for each capsize to 15 s
            // (duo-surge-slot); now its answers ride any surge while both are carried.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = OpenTheGourd, Name = "Open the Gourd", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { RideTheSquall }, Icon = UpgradeIcons.Gust,
                Description = "A duo surge. While your surge lasts a gourd of winds hangs at your hip: each dodge lets one out, a gust down the dodge's line that Soaks, unsteadies twice and shoves, and each capsize gives you 6 mana back.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, WhileSurge = true, Do = UpgradeAction.Lane, Reach = 6f, Radius = 1f, Affliction = AfflictionKind.Unsteady, Stacks = 2 },
                    new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, WhileSurge = true, Do = UpgradeAction.Lane, Reach = 6f, Radius = 1f, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, WhileSurge = true, At = EffectAt.Self, Do = UpgradeAction.Push, Radius = 3f, Amount = 2f },
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, WhileSurge = true, Do = UpgradeAction.Mana, Amount = 6f },
                },
            }, HeartPattern.RightTop));
        }
    }
}

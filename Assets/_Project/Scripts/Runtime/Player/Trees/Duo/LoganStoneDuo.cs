using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Wind and Earth (pairs_d4.md, "Wind and Earth", distance 4; task 4.17), named by its reaction,
    /// Logan Stone. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark,
    /// Grit or Drift, opens Sandblast and Drifted Over; Grit in the Wind and New Face (majors), the keystone The Prophecy
    /// Broken, and the duo spell Walking Dune, bought with Shards once both are carried. Wind is written first, as the ring
    /// names the pair.
    /// </summary>
    public static class LoganStoneDuo
    {
        public const string Shape = "loganstone.shape";
        public const string GritInTheWind = "loganstone.grit";
        public const string NewFace = "loganstone.newface";
        public const string TheProphecyBroken = "loganstone.prophecy";
        public const string WalkingDune = "loganstone.dune";
        /// <summary>The prefixes of the conditions the two shapes set, each the rocking as the plain reaction's.</summary>
        public const string Sandblast = "loganstone.sandblast";
        public const string DriftedOver = "loganstone.drift";
        /// <summary>New Face's own count of the rocking's blows and the felled enemy's open face; The Prophecy Broken's fall;
        /// the dune Walking Dune leaves on what it rolls over.</summary>
        public const string Counted = "loganstone.counted";
        public const string Face = "loganstone.face";
        public const string Thrown = "loganstone.thrown";
        public const string Dune = "loganstone.wade";
        private const string Reaction = "react.loganstone";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Wind, B = Elements.Earth;

            // NEEDS: a cone that leaves an enemy's far side (cone-from-enemy): Sandblast's grit, 5 m long and 60 degrees wide;
            // now three bursts on the line beyond it. Ground that acts on whoever walks in later (ground-states): Drifted Over's
            // sand slowing those who wade in after the fall; now those within 2 m as it falls.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Logan Stone: Sandblast or Drifted Over", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Logan Stone's two shapes, chosen at a rest fire. Sandblast: the dust of the fall is thrown on as stinging grit that sets the next rocking. Drifted Over: sand heaps over it where it falls and those near wade. Either runs instead of the plain Logan Stone.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Sandblast",
                        Description = "It rocks as before and your third blow throws it down. The dust of the fall is thrown on away from you as stinging grit: 6, 15 stagger and an Unsteady to each enemy behind it, so one already shaken may begin to rock in turn.",
                        Effects = Rock(Sandblast, 1.8f, new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Sandblast + ".rocking", By = StruckBy.Weapon, AtCount = 3, Ranks = EnemyRank.Plain, Do = UpgradeAction.Burst, Shape = HazardShape.Downwind, Count = 3,
                                Reach = 2.5f, Amount = 1.4f, Radius = 1f, Damage = 6f, Stagger = 15f, Affliction = AfflictionKind.Unsteady, Stacks = 1, Cascades = true },
                        }),
                    },
                    new ShapeDef
                    {
                        Name = "Drifted Over",
                        Description = "It rocks as before and your third blow throws it down, and the wind heaps sand over it: it lies buried to the shoulders 1 s longer, and enemies within 2 m of it wade at 60% speed while it lies.",
                        Effects = Rock(DriftedOver, 2.8f, new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = DriftedOver + ".rocking", By = StruckBy.Weapon, AtCount = 3, Ranks = EnemyRank.Plain, Do = UpgradeAction.Condition, Radius = 2f,
                                State = new ConditionSpec { Id = DriftedOver + ".wade", Seconds = 2.8f, Pace = 0.6f } },
                        }),
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = GritInTheWind, Name = "Grit in the Wind", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Gust,
                Description = "Your light blows on a shaken enemy leave it Unsteady; your heavy blows on an Unsteady enemy shake it.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.LightHit, Needs = AfflictionKind.Tremor, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Unsteady, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Unsteady, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Tremor, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            // NEEDS: an answer to an enemy in a condition another mark set (condition-needs): the felled enemy; now New Face
            // counts the rocking's weapon blows itself from each Logan Stone, and after the third (the fall) the next weapon blow
            // on a plain enemy within 1.8 s is critical and gives you 6 mana back.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = NewFace, Name = "New Face", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Star,
                Description = "Your first blow on a felled enemy is critical and gives you 6 mana back.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Counted, Seconds = 3f, Blows = 3 } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Counted, By = StruckBy.Weapon, AtCount = 3, Ranks = EnemyRank.Plain, Do = UpgradeAction.Condition,
                        State = new ConditionSpec { Id = Face, Seconds = 1.8f, Flags = ConditionFlags.TakesCritical, EndsWith = ConditionBlow.Weapon } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Face, By = StruckBy.Weapon, Do = UpgradeAction.Mana, Amount = 6f },
                },
            }, HeartPattern.RightLow));

            // NEEDS: a count of one enemy's reactions over a fight (count-rules): the fourth Logan Stone on an elite that only
            // reeled throws it flat; now one Logan Stone in three on an elite throws it flat for 1 s by chance, at most once in 6
            // s; and a god knocked flat (gods never go down: ConditionCaster).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheProphecyBroken, Name = "The Prophecy Broken", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { GritInTheWind, NewFace }, Icon = UpgradeIcons.Rock,
                Description = "Elites and gods can be felled: after three Logan Stones have only made one reel in a fight, the fourth throws it flat for 1 s.",
                Effects = new[]
                {
                    // An elite lies half the time of a fall: 2 s asked is the entry's 1 s.
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, Ranks = EnemyRank.Elite, Chance = 0.34f, Cooldown = 6f, Do = UpgradeAction.Condition,
                        State = new ConditionSpec { Id = Thrown, Seconds = 2f, Flags = ConditionFlags.Down } },
                },
            }, HeartPattern.Keystone));

            // The duo spell, bought with 5 Shards once both are carried (no god gives it): a breaker on a 12 s cooldown
            // (SpellTimings). The wave form drives the ridge 7 m, cast in Wind (two Unsteady); each enemy it rolls over is
            // shaken, so it rocks as the dune passes, and wades at 60% speed for 4 s.
            // NEEDS: a ridge that rolls forward over its enemies in 1.2 s and then lies 4 s as slowing ground, driven 5 m on by
            // a heavy blow struck into it (form-pieces, ground-states); now a cone 7 m long, and those it struck slowed.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = WalkingDune, Name = "Walking Dune", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Breaker, Requires = new[] { GritInTheWind, NewFace }, Icon = UpgradeIcons.Wave,
                Description = "A ridge of sand heaps up before you and the wind drives it 7 m on. Each enemy it rolls over takes 16 and 35 stagger, two Unsteady and a Tremor, so it rocks as the dune passes, and wades for 4 s after.",
                Form = SpellForms.Wave,
                Spell = new SpellSpec { Reach = 7f, Arc = 45f, Damage = 16f, Stagger = 35f, Stacks = 2 },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Tremor, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Dune, Seconds = 4f, Pace = 0.6f } },
                },
            }, HeartPattern.Spell));
        }

        // The rocking as the plain Logan Stone's, under the shape's own names (prefix.rocking, .lean, .felled): 3 s of
        // rocking, the balance broken at 1.25 times and then 1.5; the third weapon blow throws a plain enemy flat for so many
        // seconds and its fall strikes 20, 30 stagger and a Tremor in 3 m; an elite only reels (40 stagger). Then the shape's own.
        private static UpgradeEffect[] Rock(string prefix, float felled, UpgradeEffect[] more)
        {
            string rocking = prefix + ".rocking", lean = prefix + ".lean";
            var effects = new List<UpgradeEffect>
            {
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = rocking, Seconds = 3f, Blows = 3 } },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = lean, Seconds = 3f, Flags = ConditionFlags.StaggerTakenShare, Scale = 1.25f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = rocking, By = StruckBy.Weapon, AtCount = 1, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = lean, Seconds = 2f, Flags = ConditionFlags.StaggerTakenShare, Scale = 1.5f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = rocking, By = StruckBy.Weapon, AtCount = 3, Ranks = EnemyRank.Plain, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = prefix + ".felled", Seconds = felled, Flags = ConditionFlags.Down } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = rocking, By = StruckBy.Weapon, AtCount = 3, Ranks = EnemyRank.Plain, Do = UpgradeAction.Burst, Radius = 3f,
                    Damage = 20f, Stagger = 30f, Affliction = AfflictionKind.Tremor, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = rocking, By = StruckBy.Weapon, AtCount = 3, Ranks = EnemyRank.Elite, Do = UpgradeAction.Burst, Radius = 0.5f,
                    Stagger = 40f, Affliction = AfflictionKind.Tremor },
            };
            effects.AddRange(more);
            return effects.ToArray();
        }
    }
}

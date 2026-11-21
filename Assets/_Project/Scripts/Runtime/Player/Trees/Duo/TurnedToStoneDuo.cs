using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Earth and Dark (pairs_d3.md, "Earth and Dark", distance 3; task 4.16), named by its reaction,
    /// Turned to Stone. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape
    /// mark opens The King's Men and Grave Marker; Grave Dirt and Broken Idol (majors), Heavy Curse (a minor), the keystone
    /// Stone Does Not Forgive, and the duo spell Stone Underfoot. Earth is written first.
    /// </summary>
    public static class TurnedToStoneDuo
    {
        public const string Shape = "turnedtostone.shape";
        public const string GraveDirt = "turnedtostone.gravedirt";
        public const string BrokenIdol = "turnedtostone.brokenidol";
        public const string HeavyCurse = "turnedtostone.heavycurse";
        public const string StoneDoesNotForgive = "turnedtostone.forgive";
        public const string StoneUnderfoot = "turnedtostone.underfoot";
        /// <summary>The prefixes of the conditions the two shapes set, each the climb and the stone as the plain reaction's.</summary>
        public const string KingsMen = "turnedtostone.kingsmen";
        public const string GraveMarker = "turnedtostone.gravemarker";

        // Spell forms by the ids fixed in docs/design/FORMS.md.
        private const string FormStep = "step";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Earth, B = Elements.Dark;

            // NEEDS: standing things that block (Grave Marker's stone; now a patch that curses what stands by it, on purpose
            // able to react).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Turned to Stone: The King's Men or Grave Marker", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Turned to Stone's two shapes, chosen at a rest fire. The King's Men: when the stone breaks the curse moves on to the next. Grave Marker: a turned enemy that dies leaves its stone standing, cursing those near. Either runs instead of the plain Turned to Stone.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "The King's Men",
                        Description = "Stone climbs it and it stands as stone as before. When your heavy blow breaks it out, the curse moves on: the nearest enemy within 5 m is Cursed and shaken, so it begins to turn in its turn.",
                        Effects = Stone(KingsMen, new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = KingsMen + ".stone", By = StruckBy.Heavy, Do = UpgradeAction.Arc, FromTarget = true, Count = 1, Reach = 5f, Affliction = AfflictionKind.Cursed, Stacks = 1, Cascades = true },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = KingsMen + ".stone", By = StruckBy.Heavy, Do = UpgradeAction.Arc, FromTarget = true, Count = 1, Reach = 5f, Affliction = AfflictionKind.Tremor, Stacks = 1, Cascades = true },
                        }),
                    },
                    new ShapeDef
                    {
                        Name = "Grave Marker",
                        Description = "Stone climbs it and it stands as stone as before. A turned enemy that dies leaves its stone standing for 10 s, and every enemy within 1.5 m of it is Cursed every 2 s.",
                        Effects = Stone(GraveMarker, new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.ConditionDied, Condition = GraveMarker + ".stone", Do = UpgradeAction.Hazard, Radius = 1.5f, Seconds = 10f, Interval = 2f, Affliction = AfflictionKind.Cursed, Stacks = 1, Cascades = true },
                        }),
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = GraveDirt, Name = "Grave Dirt", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Skull,
                Description = "Heavy blows on a Cursed enemy shake it; the blow that ends a chain on a shaken enemy Curses it.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Cursed, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Tremor, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.ChainEnd, Needs = AfflictionKind.Tremor, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Cursed, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            // NEEDS: an answer to an enemy just broken out of stone (death-in-condition); now a finisher on one shaken and cursed.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = BrokenIdol, Name = "Broken Idol", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Hammer,
                Description = "A finisher on an enemy just broken out of stone heals you 10 and Curses every enemy within 5 m.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Needs = AfflictionKind.Tremor, Needs2 = AfflictionKind.Cursed, Do = UpgradeAction.Heal, Amount = 10f },
                    new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Needs = AfflictionKind.Tremor, Needs2 = AfflictionKind.Cursed, Do = UpgradeAction.Afflict, Radius = 5f, Affliction = AfflictionKind.Cursed, Stacks = 1 },
                },
            }, HeartPattern.RightLow));

            // Changes its reaction's own numbers while it is in force (reaction-numbers, ReactionNumbers):
            // the stone climbing in half a second, not one.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = HeavyCurse, Name = "Heavy Curse", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { GraveDirt }, Icon = UpgradeIcons.Hourglass,
                Description = "Stone climbs half a second faster.",
                ReactionChanges = new[]
                {
                    new ReactionChange { Shape = -1, Index = 0, ConditionSeconds = 0.5f },
                    new ReactionChange { Shape = 0, Index = 0, ConditionSeconds = 0.5f },
                    new ReactionChange { Shape = 1, Index = 0, ConditionSeconds = 0.5f },
                },
            }, HeartPattern.LeftHigh));

            // NEEDS: afflictions kept for the rest of the fight on one enemy (fades-never); until then it does nothing.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = StoneDoesNotForgive, Name = "Stone Does Not Forgive", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { GraveDirt, BrokenIdol }, Icon = UpgradeIcons.Rock,
                Description = "An enemy turned to stone once keeps its Tremor and its Curse until it dies.",
            }, HeartPattern.Keystone));

            // The duo spell, bought with Shards once both are carried (no god gives it): a movement on a 10 s cooldown (SpellTimings). The step form carries you to the enemy
            // faced; your foot on its shadow shakes it (the branch's first element) and Curses it, so it begins to turn.
            // NEEDS: turned to stone at once, past its climb; those passed slowed; the heavy blow that breaks it out within
            // 1.5 s at double stagger.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = StoneUnderfoot, Name = "Stone Underfoot", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Movement, Requires = new[] { GraveDirt, BrokenIdol }, Icon = UpgradeIcons.Boot,
                Description = "You stride to the enemy you face, leaving grey dust footprints, and set your foot on its shadow: it is Cursed and shaken two deep, and stone climbs it.",
                Form = FormStep,
                Spell = new SpellSpec { Reach = 8f, Damage = 6f, Stagger = 10f, Stacks = 2, Seconds = 1.5f },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Cursed, Stacks = 2 } },
            }, HeartPattern.Spell));
        }

        // The climb and the stone as the plain Turned to Stone's, under the shape's own names (prefix.climbs, .climbed,
        // .stone, .spellproof): slowed over 2 s, then stone for 1.5 s (rooted, beginning no attack, double stagger, half from
        // spells); a heavy blow on the stone breaks it out for 50 stagger. Then the shape's own answers.
        private static UpgradeEffect[] Stone(string prefix, UpgradeEffect[] more)
        {
            string climbs = prefix + ".climbs", climbed = prefix + ".climbed", stone = prefix + ".stone", spellproof = prefix + ".spellproof";
            var effects = new List<UpgradeEffect>
            {
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = climbs, Seconds = 1f, Pace = 0.7f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = climbs, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = climbed, Seconds = 1f, Pace = 0.4f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = climbed, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = stone, Seconds = 1.5f, Flags = ConditionFlags.Rooted | ConditionFlags.BeginsNoAttack | ConditionFlags.StaggerTakenShare, Scale = 2f, EndsWith = ConditionBlow.Heavy } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = climbed, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = spellproof, Seconds = 1.5f, Flags = ConditionFlags.SpellTakenShare, Scale = 0.5f, EndsWith = ConditionBlow.Heavy } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = stone, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 0.5f, Stagger = 50f, Affliction = AfflictionKind.Tremor },
            };
            effects.AddRange(more);
            return effects.ToArray();
        }
    }
}

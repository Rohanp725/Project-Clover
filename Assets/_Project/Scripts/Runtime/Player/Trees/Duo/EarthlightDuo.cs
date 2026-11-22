using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Earth and Light (pairs_d1_d2.md, "Earth and Light", distance 2; task 4.15), named by its reaction,
    /// Earthlight. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark,
    /// Lit Stone, opens Down the Passage and Lit Seam; Keeping the Hour and Dawn Through the Door (majors), Quartz Vein (a
    /// minor), and the duo spell The Stone Keeps the Hour. Earth is written first: the duo spell is cast in Earth.
    /// The pair file gives this branch no keystone.
    /// </summary>
    public static class EarthlightDuo
    {
        public const string Shape = "earthlight.shape";
        public const string KeepingTheHour = "earthlight.hour";
        public const string DawnThroughTheDoor = "earthlight.dawn";
        public const string QuartzVein = "earthlight.quartz";
        /// <summary>The Stone Keeps the Hour: the moment its line crosses an enemy, when a blow on it is critical and staggers double.</summary>
        public const string OnTheLine = "earthlight.stone.line";
        public const string TheStoneKeepsTheHour = "earthlight.stone";
        /// <summary>The condition Lit Seam sets: the seams staying lit for heavy blows to flash again.</summary>
        public const string LitSeam = "earthlight.seam";
        private const string Reaction = "react.earthlight";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Earth, B = Elements.Light;

            // The seams themselves: only enemies standing on the star of six cracks are struck (UpgradeEffect.Seams), and Lit Seam's
            // flash runs along the star of seams where they were first laid (UpgradeEffect.AtOwnGround: the seams are your own ground
            // while they glow, World.OwnGrounds.Seams), even if the enemy has moved off them.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Earthlight: Down the Passage or Lit Seam", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Earthlight's two shapes, chosen at a rest fire. Down the Passage: the light runs on through the ground in a line away from you. Lit Seam: the seams stay lit, and your heavy blows flash them again. Either runs instead of the plain Earthlight.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Down the Passage",
                        Description = "Its Mark is renewed, and the light runs on through the ground in a straight line from it away from you, 8 m long and 1 m wide: 12 and a Mark on all on it.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Marked, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Lane, Reach = 8f, Radius = 0.5f, Delay = 0.5f, Damage = 12f, Affliction = AfflictionKind.Marked, Stacks = 1 },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Lit Seam",
                        Description = "Light breaks up through the seams as before, 14, 25 stagger and a Mark in 3 m, and its Mark is renewed. The seams stay lit for 5 s: each heavy blow you land on it flashes them again, 10 and 15 stagger to all within 3 m.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Marked, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Radius = 3f, Delay = 0.5f, Damage = 14f, Stagger = 25f, Affliction = AfflictionKind.Marked, Stacks = 1, Seams = 6,
                                Named = Clover.World.OwnGrounds.Seams, GroundSeconds = 5f },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = LitSeam, Seconds = 5f } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = LitSeam, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 3f, Damage = 10f, Stagger = 15f, Cooldown = 0.3f, Seams = 6,
                                AtOwnGround = Clover.World.OwnGrounds.Seams },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // The hour (UpgradeEffect.Hour): once in each 6 s of fighting the next heavy blow is on it, critical (the hour's promise)
            // with two Tremor, and a dial at your feet fills toward it (PlayerUpgrades.KeepHour).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = KeepingTheHour, Name = "Keeping the Hour", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Hourglass,
                Description = "Every 6 s in a fight, the next heavy blow is on the hour: critical, with two Tremor. A small dial at your feet fills to show it.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Hour = 6f, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Tremor, Stacks = 2 },
                },
            }, HeartPattern.LeftLow));

            // Only a perfect guard made standing on glowing seams (StatWhen.OnSeams: Earthlight's star while it glows, 5 s for Lit Seam).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = DawnThroughTheDoor, Name = "Dawn Through the Door", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Sun,
                Description = "A perfect guard made on glowing seams blinds the attacker: Marked, and Tremor to full depth.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, While = StatWhen.OnSeams, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Marked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, While = StatWhen.OnSeams, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Tremor, Stacks = 99 },
                },
            }, HeartPattern.RightLow));

            // Changes its reaction's own numbers while it is in force (reaction-numbers, ReactionNumbers):
            // the star 0.5 m wider (Down the Passage's lane half a metre wider).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = QuartzVein, Name = "Quartz Vein", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { KeepingTheHour }, Icon = UpgradeIcons.Gem,
                Description = "Earthlight's star is 0.5 m wider.",
                ReactionChanges = new[]
                {
                    new ReactionChange { Shape = -1, Index = 1, Radius = 3.5f },
                    new ReactionChange { Shape = 0, Index = 1, Radius = 0.75f },
                    new ReactionChange { Shape = 1, Index = 1, Radius = 3.5f },
                    new ReactionChange { Shape = 1, Index = 3, Radius = 3.5f },
                },
            }, HeartPattern.LeftHigh));

            // The duo spell, bought with Shards once both are carried (no god gives it): an opener on a 16 s cooldown (SpellTimings). The rod form plants a stone that strikes
            // round it on a beat.
            // The stone is a standing stone (it blocks the way and counts as stone at an enemy's back), and a line of sunlight 6 m long
            // turns round it once in 3 s (SpellSpec.SweepTurn): each enemy it crosses is struck once a turn, Marked, and for the
            // moment after (0.35 s) a blow on it is critical and staggers double (ConditionFlags.TakesCritical, StaggerTakenShare).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheStoneKeepsTheHour, Name = "The Stone Keeps the Hour", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Opener, Requires = new[] { KeepingTheHour, DawnThroughTheDoor }, Icon = UpgradeIcons.Tower,
                Description = "A standing stone rises 3 m ahead for 9 s, and sunlight wheels round it like a dial's shadow: every enemy it crosses is struck, shaken and Marked. It never harms you.",
                Form = SpellForms.Rod,
                Spell = new SpellSpec { Reach = 3f, Radius = 6f, Seconds = 9f, Interval = 1f, Damage = 10f, Stagger = 15f, Delay = 0.3f, Stacks = 1, SweepTurn = 3f },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Marked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Condition,
                        State = new ConditionSpec { Id = OnTheLine, Seconds = 0.35f, Flags = ConditionFlags.TakesCritical | ConditionFlags.StaggerTakenShare, Scale = 2f } },
                },
            }, HeartPattern.Spell));
        }
    }
}

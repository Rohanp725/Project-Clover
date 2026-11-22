using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Earth and Poison (pairs_d4.md, "Earth and Poison", distance 4; task 4.17), named by its reaction,
    /// Chokedamp. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark,
    /// Damp or Ore, opens Blackdamp and Red Ore; Foul Ground and The Lamp Goes Out (majors), the keystone Seam of Ore, and
    /// the duo spell The Clay Rank, bought with Shards once both are carried. Earth is written first, as the ring names the
    /// pair.
    /// </summary>
    public static class ChokedampDuo
    {
        public const string Shape = "chokedamp.shape";
        public const string FoulGround = "chokedamp.foulground";
        public const string TheLampGoesOut = "chokedamp.lamp";
        public const string SeamOfOre = "chokedamp.seam";
        public const string TheClayRank = "chokedamp.clayrank";
        /// <summary>The conditions the two shapes set (Blackdamp's still pool, Red Ore's veins), and The Lamp Goes Out's
        /// own damp.</summary>
        public const string Black = "chokedamp.black";
        public const string Ore = "chokedamp.ore";
        public const string LampOut = "chokedamp.lampout";
        private const string Reaction = "react.chokedamp";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Earth, B = Elements.Poison;

            // Blackdamp's pool slows those who wade in after it forms while they stand in it (UpgradeEffect.Inside), and is told
            // as the damp (The Lamp Goes Out); those in it as it pools are slowed for its 7 s.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Chokedamp: Blackdamp or Red Ore", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Chokedamp's two shapes, chosen at a rest fire. Blackdamp: the damp does not run but lies still and deep under the one. Red Ore: no mist; the poison sets as veins of red ore that your heavy blow bursts. Either runs instead of the plain Chokedamp.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Blackdamp",
                        Description = "The damp does not run: it lies still and deep under the one, a pool 3.5 m wide for 7 s. What stands in it takes a dose each second, and those in it as it pools move a fifth slower. A place to hold, not a spread.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Radius = 1.75f, Seconds = 7f, Interval = 1f, Affliction = AfflictionKind.Poison, Stacks = 1,
                                Inside = new ConditionSpec { Id = Black, Seconds = 1f, Pace = 0.8f, Is = Reactions.Damp }, Ground = Reactions.Damp },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, Radius = 1.75f, State = new ConditionSpec { Id = Black, Seconds = 7f, Pace = 0.8f, Is = Reactions.Damp } },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Red Ore",
                        Description = "No mist. The poison sets in the cracks as veins of orange-red ore under it. Your next heavy blow on it within 4 s breaks the ore: a burst of red dust in 3 m, 14 and 3 more for each dose it carries, three doses and 30 stagger to each enemy reached.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Ore, Seconds = 4f, EndsWith = ConditionBlow.Heavy } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Ore, By = StruckBy.Heavy, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Poison, CountOnly = true },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Ore, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 3f, Damage = 14f, DamagePerSpent = 3f, Stagger = 30f,
                                Affliction = AfflictionKind.Poison, Stacks = 3 },
                        },
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = FoulGround, Name = "Foul Ground", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Rock,
                Description = "Your heavy blows lay a Tremor and a dose together.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Tremor, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Poison, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            // Those in the damp read from each enemy, in whichever shape: the share is set on all within 7 m of the reaction and
            // lasts only while each is in the damp (ConditionSpec.WhileIn), so it leaves at once those the damp did not reach;
            // it asks weapon blows alone (ConditionFlags.WeaponStaggerShare).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheLampGoesOut, Name = "The Lamp Goes Out", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Skull,
                Description = "Your weapon breaks the balance of enemies standing in the damp a quarter harder.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.InGround, Condition = Reactions.Damp, Do = UpgradeAction.Condition,
                        State = new ConditionSpec { Id = LampOut, Seconds = 1f, Flags = ConditionFlags.WeaponStaggerShare, Scale = 1.25f, WhileIn = Reactions.Damp } },
                },
            }, HeartPattern.RightLow));

            // The ground shakes under a shaken enemy each time its poison bites: a Tremor to all within 2 m. A reaction's
            // rest bounds what it sets off, and the shaking at most every quarter second.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = SeamOfOre, Name = "Seam of Ore", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { FoulGround, TheLampGoesOut }, Icon = UpgradeIcons.Gem,
                Description = "Each time poison bites a shaken enemy, the ground shakes under it: a Tremor to every enemy within 2 m.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PoisonBite, Needs = AfflictionKind.Tremor, Cooldown = 0.25f, Do = UpgradeAction.Afflict, Radius = 2f, Affliction = AfflictionKind.Tremor, Stacks = 1 },
                },
            }, HeartPattern.Keystone));

            // The duo spell, bought with 5 Shards once both are carried (no god gives it): an opener on a 14 s cooldown
            // (SpellTimings). The minions form raises the three clay soldiers for 6 s, cast in Earth (a Tremor each strike);
            // each strike also doses.
            // NEEDS: three soldiers marching 7 m in a rank, pushing, taking enemy blows from the front and breaking after two,
            // each bursting in damp and clay, 12, a Tremor and three doses in 2.5 m, twice as hard broken by your heavy blow
            // (form-pieces, helpers-answer); now three helpers that run at the enemies and strike them.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheClayRank, Name = "The Clay Rank", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Opener, Requires = new[] { FoulGround, TheLampGoesOut }, Icon = UpgradeIcons.Tower,
                Description = "Three clay soldiers rise in a rank before you, grey men with cracked faces leaking damp, and go at the enemy for 6 s. Each strike shakes and doses what it hits.",
                Form = SpellForms.Minions,
                Spell = new SpellSpec { Count = 3, Seconds = 6f, Interval = 1.2f, Damage = 6f, Stagger = 10f, Stacks = 1 },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Poison, Stacks = 1 },
                },
            }, HeartPattern.Spell));
        }
    }
}

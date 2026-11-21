using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Light and Poison (pairs_d1_d2.md, "Light and Poison", distance 2; task 4.15), named by its
    /// reaction, Burning Sap. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its
    /// Shape mark, Lit Veins, opens The Light Finds Them and Blistered; Nectar After Poison and Seen Through (majors), Bright
    /// Venom (a minor), and the duo spell Bared to the Sun. Light is written first: the duo spell is cast in
    /// Light. The pair file gives this branch no keystone.
    /// </summary>
    public static class BurningSapDuo
    {
        public const string Shape = "burningsap.shape";
        public const string NectarAfterPoison = "burningsap.nectar";
        public const string SeenThrough = "burningsap.seenthrough";
        public const string BrightVenom = "burningsap.brightvenom";
        public const string BaredToTheSun = "burningsap.bared";
        /// <summary>The conditions the two shapes set, each the sap waiting as the plain reaction's.</summary>
        public const string Found = "burningsap.found";
        public const string Blistered = "burningsap.blistered";
        private const string Reaction = "react.burningsap";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Light, B = Elements.Poison;

            // The poison held back and landed doubled, as the plain reaction (ConditionSpec.HoldsBites).
            // The Light Finds Them Marks only the poisoned within 8 m (OnlyCarrying); Blistered's weals are raised by critical
            // blows alone (StruckBy.Critical).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Burning Sap: The Light Finds Them or Blistered", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Burning Sap's two shapes, chosen at a rest fire. The Light Finds Them: when it reacts, the poisoned across the room are Marked as well. Blistered: the weals burst at the blow that raises them. Either runs instead of the plain Burning Sap.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "The Light Finds Them",
                        Description = "The sap wakes as before for 3 s, and the enemies within 8 m are Marked as well, their veins lit across the room. At the end every bite the poison held back lands at once, doubled, and each weal bursts, two doses on all within 2.5 m.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Found, Seconds = 3f, HoldsBites = AfflictionKind.Poison, HeldScale = 2f } },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Afflict, Radius = 8f, Affliction = AfflictionKind.Marked, Stacks = 1, OnlyCarrying = AfflictionKind.Poison },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Found, SpentAtLeast = 1, Do = UpgradeAction.Afflict, Radius = 2.5f, Affliction = AfflictionKind.Poison, Stacks = 2, PerStack = true },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Blistered",
                        Description = "The weals do not wait for the end: each of your critical blows in the 3 s raises one that bursts at once, two doses on every enemy within 2.5 m. At the end every bite the poison held back lands at once, doubled.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Blistered, Seconds = 3f, HoldsBites = AfflictionKind.Poison, HeldScale = 2f } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Blistered, By = StruckBy.Critical, Do = UpgradeAction.Afflict, Radius = 2.5f, Affliction = AfflictionKind.Poison, Stacks = 2, Cooldown = 0.2f },
                        },
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = NectarAfterPoison, Name = "Nectar After Poison", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Flask,
                Description = "An enemy that dies carrying both a Mark and poison gives up 8 mana and 6 health.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Marked, Needs2 = AfflictionKind.Poison, Do = UpgradeAction.Heal, Amount = 6f },
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Marked, Needs2 = AfflictionKind.Poison, Do = UpgradeAction.Mana, Amount = 8f },
                },
            }, HeartPattern.LeftLow));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = SeenThrough, Name = "Seen Through", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Eye,
                Description = "A perfect guard against a poisoned enemy Marks it and makes all its poison bite at once.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Poison, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Marked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Poison, Do = UpgradeAction.Bite },
                },
            }, HeartPattern.RightLow));

            // The held bites land 15% harder (AfflictionRules.HeldBitesStronger).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = BrightVenom, Name = "Bright Venom", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { SeenThrough }, Icon = UpgradeIcons.Drop,
                Description = "The held bites strike 15% harder when they land.",
                Rules = book => book.Afflictions.HeldBitesStronger(0.15f),
            }, HeartPattern.RightHigh));

            // The duo spell, bought with Shards once both are carried (no god gives it): an opener on a 13 s cooldown (SpellTimings). The field form lays a patch at the aim
            // point; each poisoned enemy it bites is Marked (the branch's first element) and starts Burning Sap past its rest.
            // Once on each enemy in a cast (its clock runs the shaft's 5 s on each enemy apart).
            // NEEDS: each weapon blow inside raising one more weal on the one struck.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = BaredToTheSun, Name = "Bared to the Sun", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Opener, Requires = new[] { NectarAfterPoison, SeenThrough }, Icon = UpgradeIcons.Sun,
                Description = "You tear the shade off a place: a hard-edged shaft of sun falls on the aim point for 5 s. Every enemy in it is Marked, and the poisoned start Burning Sap at once, whatever its rest.",
                Form = SpellForms.Field,
                Spell = new SpellSpec { Reach = 8f, Radius = 2.5f, Seconds = 5f, Interval = 1f, LingerDamage = 2f, Stacks = 1 },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Needs = AfflictionKind.Poison, Do = UpgradeAction.ForceReaction, Reaction = Reaction, Cooldown = 5f, CooldownPerEnemy = true } },
            }, HeartPattern.Spell));
        }
    }
}

using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Light and Fire (pairs_d3.md, "Light and Fire", distance 3; task 4.16), named by its reaction,
    /// Kindling Point. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark
    /// opens Burning Glass and The Fire Is Hungry; The Sun Points and Noon Guard (majors), Clear Sky (a minor), the keystone
    /// The Feast, and the duo spell Hands of the Sun. Light is written first, as the ring names the pair.
    /// </summary>
    public static class KindlingPointDuo
    {
        public const string Shape = "kindlingpoint.shape";
        public const string TheSunPoints = "kindlingpoint.sunpoints";
        public const string NoonGuard = "kindlingpoint.noonguard";
        public const string ClearSky = "kindlingpoint.clearsky";
        public const string TheFeast = "kindlingpoint.feast";
        public const string HandsOfTheSun = "kindlingpoint.hands";
        /// <summary>The conditions the two shapes set: each burning point, and Burning Glass's lens held after it.</summary>
        public const string Glass = "kindlingpoint.glass";
        public const string Lens = "kindlingpoint.lens";
        public const string Hungry = "kindlingpoint.hungry";
        /// <summary>Hands of the Sun's grip, let go at the first weapon blow.</summary>
        public const string Hand = "kindlingpoint.hand";
        /// <summary>The reaction's id, for what answers a death its own burst dealt (The Feast).</summary>
        public static readonly string KindlingReaction = AfflictionPairs.Id(AfflictionPairs.Index(AfflictionKind.Marked, AfflictionKind.Burn));

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Light, B = Elements.Fire;

            // The shapes count only critical blows (ConditionSpec.CriticalOnly): any other blow passes the point by.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Kindling Point: Burning Glass or The Fire Is Hungry", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Kindling Point's two shapes, chosen at a rest fire. Burning Glass: after the point bursts, it stays as a lens and keeps deepening the Burn. The Fire Is Hungry: the tongue of flame leaps on to the next enemy. Either runs instead of the plain Kindling Point.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Burning Glass",
                        Description = "Each of your blows deepens its Burn and the third sets it alight as before, 10 for each Burn; then the point stays on it for 4 s, a lens held still, deepening its Burn by one each second.",
                        Effects = Point(Glass, new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Glass, By = StruckBy.Weapon, AtCount = 3, Do = UpgradeAction.Burst, Radius = 0.6f, Damage = 10f, PerStack = true, Affliction = AfflictionKind.Burn },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Glass, By = StruckBy.Weapon, AtCount = 3, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 2 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Glass, By = StruckBy.Weapon, AtCount = 3, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Lens, Seconds = 4f, Beat = 1f } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Lens, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1 },
                        }),
                    },
                    new ShapeDef
                    {
                        Name = "The Fire Is Hungry",
                        Description = "Each of your blows deepens its Burn, and at the third the tongue of flame does not stay: it leaps to the nearest other enemy within 6 m, 10 for each Burn, and Marks it and burns it two deep, so the point moves on through a crowd.",
                        Effects = Point(Hungry, new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Hungry, By = StruckBy.Weapon, AtCount = 3, Do = UpgradeAction.Arc, FromTarget = true, Count = 1, Reach = 6f, Damage = 10f, PerStack = true, Affliction = AfflictionKind.Marked, Stacks = 1, Cascades = true },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Hungry, By = StruckBy.Weapon, AtCount = 3, Do = UpgradeAction.Arc, FromTarget = true, Count = 1, Reach = 6f, Affliction = AfflictionKind.Burn, Stacks = 2, Cascades = true },
                        }),
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheSunPoints, Name = "The Sun Points", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Sun,
                Description = "The blow that ends a chain on a burning enemy Marks it.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.ChainEnd, Needs = AfflictionKind.Burn, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Marked, Stacks = 1 } },
            }, HeartPattern.LeftLow));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = NoonGuard, Name = "Noon Guard", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard against a Marked, burning enemy sets it alight at the point at once: all its Burn bursts, 10 for each, and it burns on two deep.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Marked, Needs2 = AfflictionKind.Burn, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Burn },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Marked, Do = UpgradeAction.Burst, Radius = 0.6f, DamagePerSpent = 10f, Affliction = AfflictionKind.Burn },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Marked, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 2 },
                },
            }, HeartPattern.RightLow));

            // A Mark lasts a blow longer on an enemy that also burns, whichever was laid first (Afflictions.MoreBlowsBurning, set
            // while this mark is in force).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = ClearSky, Name = "Clear Sky", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { TheSunPoints }, Icon = UpgradeIcons.Eye,
                Description = "A Mark lasts one blow longer on a burning enemy.",
            }, HeartPattern.LeftHigh));

            // Only a death dealt by Kindling Point's own blow (UpgradeEffect.KilledBy).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheFeast, Name = "The Feast", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { TheSunPoints, NoonGuard }, Icon = UpgradeIcons.Flame,
                Description = "When a Kindling Point burst kills, every burn within 8 m deepens by one and you gain 10 mana back.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Kill, KilledBy = KindlingReaction, Do = UpgradeAction.Afflict, Radius = 8f, Affliction = AfflictionKind.Burn, Stacks = 1, Cooldown = 0.5f },
                    new UpgradeEffect { On = UpgradeTrigger.Kill, KilledBy = KindlingReaction, Do = UpgradeAction.Mana, Amount = 10f, Cooldown = 0.5f },
                },
            }, HeartPattern.Keystone));

            // The duo spell, bought with Shards once both are carried (no god gives it): an opener on a 12 s cooldown (SpellTimings). The bolt form reaches four enemies; each
            // is Marked (the branch's first element), burned two deep and held, so each carries a Kindling Point.
            // The hand lets go at your first weapon blow on what it grips: that blow is critical (ConditionFlags.TakesCritical)
            // and deepens the Burn by two. NEEDS: the four rays from one sun (form-pieces; now the bolt form's own reach).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = HandsOfTheSun, Name = "Hands of the Sun", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Opener, Requires = new[] { TheSunPoints, NoonGuard }, Icon = UpgradeIcons.Fist,
                Description = "A burning sun rises over you for a breath, and long rays reach down to the four nearest enemies. Each ray ends in a hand of flame that grips: held, burned two deep and Marked. The hand lets go at your first blow, which is critical and burns two deeper.",
                Form = SpellForms.Bolt,
                Spell = new SpellSpec { Reach = 9f, Count = 3, Radius = 9f, Damage = 6f, Stagger = 10f, Stacks = 1 },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 2 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Hold, Seconds = 1f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Condition,
                        State = new ConditionSpec { Id = Hand, Seconds = 4f, Flags = ConditionFlags.TakesCritical, EndsWith = ConditionBlow.Weapon } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Hand, By = StruckBy.Weapon, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 2 },
                },
            }, HeartPattern.Spell));
        }

        // The burning point as the plain Kindling Point's: for the Mark's three critical blows (8 s at most) each deepens
        // its Burn, and the third spends it, counting it for the shape's own burst. Then the shape's own answers.
        private static UpgradeEffect[] Point(string id, UpgradeEffect[] more)
        {
            var effects = new List<UpgradeEffect>
            {
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = id, Seconds = 8f, Blows = 3, CriticalOnly = true } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = id, By = StruckBy.Weapon, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = id, By = StruckBy.Weapon, AtCount = 3, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Burn },
            };
            effects.AddRange(more);
            return effects.ToArray();
        }
    }
}

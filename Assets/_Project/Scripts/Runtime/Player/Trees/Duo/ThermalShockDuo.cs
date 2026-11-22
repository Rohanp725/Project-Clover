using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Fire and Ice (pairs_d3.md, "Fire and Ice", distance 3; task 4.16), named by its reaction, Thermal
    /// Shock. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark opens
    /// Hot Shards and Crazing; Spark and Rime and Hot Stone, Cold Water (majors), Neither Wins (a minor), the keystone Fire
    /// Under the Ice, the duo spell Fire-Setting and the duo surge The Yawning Gap. Fire is written first.
    /// </summary>
    public static class ThermalShockDuo
    {
        public const string Shape = "thermalshock.shape";
        public const string SparkAndRime = "thermalshock.sparkrime";
        public const string HotStoneColdWater = "thermalshock.hotstone";
        public const string NeitherWins = "thermalshock.neitherwins";
        public const string FireUnderTheIce = "thermalshock.underice";
        public const string FireSetting = "thermalshock.firesetting";
        /// <summary>Fire-Setting's fire banked against the enemy, quenched at its end.</summary>
        public const string Banked = "thermalshock.firesetting.banked";
        public const string TheYawningGap = "thermalshock.gap";
        /// <summary>The conditions the two shapes set, each a crack as the plain reaction's.</summary>
        public const string Shards = "thermalshock.shards";
        public const string Crazing = "thermalshock.crazing";
        private const string Reaction = "react.thermalshock";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Fire, B = Elements.Ice;

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Thermal Shock: Hot Shards or Crazing", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Thermal Shock's two shapes, chosen at a rest fire. Hot Shards: the split throws shards that set a crowd cracking too. Crazing: the crack splits again by itself while both afflictions last. Either runs instead of the plain Thermal Shock.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Hot Shards",
                        Description = "The body cracks as before, 14. The next heavy blow within 5 s splits it, 30 and 40 stagger, and throws shards of hot stone and ice 3 m round: 8, a Burn and a Chill on everything they hit, which on purpose may crack them too.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 14f, Affliction = AfflictionKind.Chill },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Shards, Seconds = 5f, EndsWith = ConditionBlow.Heavy, Is = Reactions.Cracked } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Shards, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 30f, Stagger = 40f, Affliction = AfflictionKind.Burn },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Shards, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 3f, Damage = 8f, Affliction = AfflictionKind.Burn, Stacks = 1, Delay = 0.1f, Cascades = true },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Shards, By = StruckBy.Heavy, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Chill, Stacks = 1, Cascades = true },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Crazing",
                        Description = "The body cracks, 14, and the crack does not wait for a heavy blow: while it carries both Burn and Chill it splits again by itself every 1.5 s, 10 and 12 stagger.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 14f, Affliction = AfflictionKind.Chill },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Crazing, Seconds = 30f, Beat = 1.5f, While = AfflictionKind.Burn, While2 = AfflictionKind.Chill, Is = Reactions.Cracked } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Crazing, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 10f, Stagger = 12f, Affliction = AfflictionKind.Chill },
                        },
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = SparkAndRime, Name = "Spark and Rime", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Flame,
                Description = "Heavy blows lay one Burn and one Chill.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            // A perfect guard against a Cracked enemy (the plain crack, Hot Shards' or Crazing's) splits it at once: the crack
            // takes the heavy blow it waits for, so it splits as its own shape splits and is spent. Crazing's crack waits for
            // no blow; it splits once now, as at its beat.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = HotStoneColdWater, Name = "Hot Stone, Cold Water", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard against a Cracked enemy splits it at once.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, InState = Reactions.Cracked, Do = UpgradeAction.StrikeCondition, Condition = Reactions.Cracked },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, InState = Crazing, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 10f, Stagger = 12f, Affliction = AfflictionKind.Chill },
                },
            }, HeartPattern.RightLow));

            // Each of the two kept from fading while the other is carried (AfflictionRules.KeepWhile).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = NeitherWins, Name = "Neither Wins", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { SparkAndRime }, Icon = UpgradeIcons.Snowflake,
                Description = "On an enemy that carries both, neither the Burn nor the Chill fades.",
                Rules = book =>
                {
                    book.Afflictions.KeepWhile(AfflictionKind.Burn, AfflictionKind.Chill);
                    book.Afflictions.KeepWhile(AfflictionKind.Chill, AfflictionKind.Burn);
                },
            }, HeartPattern.LeftHigh));

            // A burning Frozen enemy shatters on any blow, weapon or spell (UpgradeEffect.NeedsFrozen, UpgradeAction.Shatter), and its
            // shatter sets all within 3 m burning two deep.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = FireUnderTheIce, Name = "Fire Under the Ice", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { SparkAndRime, HotStoneColdWater }, Icon = UpgradeIcons.Burst,
                Description = "A Frozen enemy that is burning shatters on any blow, not only a heavy, and its shatter sets everything within 3 m burning two deep.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Needs = AfflictionKind.Burn, NeedsFrozen = true, Do = UpgradeAction.Shatter },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, Needs = AfflictionKind.Burn, NeedsFrozen = true, Do = UpgradeAction.Shatter },
                    new UpgradeEffect { On = UpgradeTrigger.Shatter, Needs = AfflictionKind.Burn, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Burn, Stacks = 2, Cooldown = 0.2f },
                },
            }, HeartPattern.Keystone));

            // The duo spell, bought with Shards once both are carried (no god gives it): a detonator on a 12 s cooldown (SpellTimings).
            // The first press banks a fire against the enemy faced (a quiet brand: only the gesture; the condition Banked, 4 s): 4
            // damage and a Burn more each second. The second press (or the 4 s ending) quenches it: the condition ends now, and its
            // end is the quench, 20 and 8 a Burn, 60 stagger, Chill three deep, the Burn spent, neighbours within 2.5 m half, each
            // weapon blow it took before 10% harder (UpgradeEffect.HarderPerBlow, five at most), and Thermal Shock forced on it.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = FireSetting, Name = "Fire-Setting", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Detonator, Requires = new[] { SparkAndRime, HotStoneColdWater }, Icon = UpgradeIcons.Flame,
                Description = "A fire is laid against the enemy you face. When it is quenched with a dash of rime the heated body splits: a hard blow, Chill three deep, and its neighbours take half.",
                Form = SpellForms.Brand,
                Spell = new SpellSpec { Reach = 6f, Seconds = 4f, Damage = 0f, Stagger = 0f, Stacks = 0, Quiet = true },
                PressWindow = 4f,
                Presses = new[] { new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, Do = UpgradeAction.EndCondition, Condition = Banked, EachWithin = 30f } } },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnSpell = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Banked, Seconds = 4f, Beat = 1f } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Banked, Do = UpgradeAction.Burst, Radius = 0.3f, Damage = 4f, Stagger = 0f, Affliction = AfflictionKind.Burn, Stacks = 1 },
                    // The quench, at the fire's end however it came.
                    new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Banked, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Burn, CountOnly = true },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Banked, Do = UpgradeAction.Burst, Radius = 0.4f, Damage = 20f, DamagePerSpent = 8f, Stagger = 60f,
                        Affliction = AfflictionKind.Chill, Stacks = 3, HarderPerBlow = 0.1f },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Banked, Do = UpgradeAction.Burst, Radius = 2.5f, SparesTarget = true, Damage = 10f, DamagePerSpent = 4f, Stagger = 30f,
                        Affliction = AfflictionKind.Chill, Stacks = 1, HarderPerBlow = 0.1f },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Banked, Do = UpgradeAction.ForceReaction, Reaction = Reaction, Alone = true },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Banked, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Burn },
                },
            }, HeartPattern.Spell));

            // The duo surge, worn in the Hand's surge slot with its own 40 mana and 10 s (DuoSurge). While it burns the weapon's
            // swings take turns (UpgradeEffect.TakesTurns), a Burn then a Chill; Thermal Shock rests only 2 s on each enemy
            // (UpgradeAction.ShortenRest), and each one heals you 2.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheYawningGap, Name = "The Yawning Gap", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { FireSetting }, Icon = UpgradeIcons.Sun,
                Description = "A duo surge for your surge slot: 40 mana, 10 seconds. While it lasts your weapon blows take turns, burning and chilling, so Thermal Shock keeps setting off, and each split lets fall drops of melt that heal you.",
                Surge = new DuoSurge { Mana = 40f, Seconds = 10f },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, WhileSurge = true, TakesTurns = 2, Turn = 0, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, WhileSurge = true, TakesTurns = 2, Turn = 1, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, WhileSurge = true, Do = UpgradeAction.ShortenRest, Seconds = 2f },
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, WhileSurge = true, Do = UpgradeAction.Heal, Amount = 2f },
                },
            }, HeartPattern.RightTop));
        }
    }
}

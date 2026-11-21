using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Metal and Poison (pairs_d3.md, "Metal and Poison", distance 3; task 4.16), named by its reaction,
    /// Quicksilver. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark
    /// opens Heavy Silver and Running Silver; Envenomed Edge and The Nail Pulled (majors), Cinnabar (a minor), the keystone
    /// The Undying Drink, and the duo spell Ichor. Metal is written first.
    /// </summary>
    public static class QuicksilverDuo
    {
        public const string Shape = "quicksilver.shape";
        public const string EnvenomedEdge = "quicksilver.edge";
        public const string TheNailPulled = "quicksilver.nail";
        public const string Cinnabar = "quicksilver.cinnabar";
        public const string TheUndyingDrink = "quicksilver.drink";
        public const string Ichor = "quicksilver.ichor";
        /// <summary>The conditions the two shapes set, each the beads as the plain reaction's.</summary>
        public const string HeavySilver = "quicksilver.heavy";
        public const string RunningSilver = "quicksilver.running";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Metal, B = Elements.Poison;

            // NEEDS: a puddle that follows the enemy and slows only inside it (Heavy Silver; now a puddle where it reacts, and
            // the enemy slowed while it carries both); every dose and notch passed on at death (Running Silver; now four doses
            // and two notches).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Quicksilver: Heavy Silver or Running Silver", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Quicksilver's two shapes, chosen at a rest fire. Heavy Silver: the mercury pools under it, slowing it and dosing what steps in. Running Silver: when it dies, its mercury runs to the nearest enemy. Either runs instead of the plain Quicksilver.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Heavy Silver",
                        Description = "While it carries both, each of your weapon blows makes its poison bite at once, as before; it moves 30% slower, and the mercury pools under it 1.5 m across for 6 s, dosing and notching what steps in.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = HeavySilver, Seconds = 30f, Pace = 0.7f, While = AfflictionKind.Sundered, While2 = AfflictionKind.Poison } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = HeavySilver, By = StruckBy.Weapon, Cooldown = 0.5f, CooldownPerEnemy = true, Do = UpgradeAction.Bite },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Radius = 0.75f, Seconds = 6f, Interval = 1f, Affliction = AfflictionKind.Poison, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Radius = 0.75f, Seconds = 6f, Interval = 2f, Affliction = AfflictionKind.Sundered, Stacks = 1 },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Running Silver",
                        Description = "While it carries both, each of your weapon blows makes its poison bite at once, as before. When it dies, its mercury runs along the floor to the nearest enemy within 6 m and gives it doses and notches.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = RunningSilver, Seconds = 30f, While = AfflictionKind.Sundered, While2 = AfflictionKind.Poison } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = RunningSilver, By = StruckBy.Weapon, Cooldown = 0.5f, CooldownPerEnemy = true, Do = UpgradeAction.Bite },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionDied, Condition = RunningSilver, Do = UpgradeAction.Pass, Reach = 6f, Affliction = AfflictionKind.Poison, Stacks = 4 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionDied, Condition = RunningSilver, Do = UpgradeAction.Pass, Reach = 6f, Affliction = AfflictionKind.Sundered, Stacks = 2 },
                        },
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = EnvenomedEdge, Name = "Envenomed Edge", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Blade,
                Description = "Heavy blows lay one notch and one dose.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Sundered, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Poison, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheNailPulled, Name = "The Nail Pulled", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Spike,
                Description = "A finisher on a Quicksilver enemy pulls the nail: all its poison bites at once, twice.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Needs = AfflictionKind.Sundered, Needs2 = AfflictionKind.Poison, Do = UpgradeAction.Bite },
                    new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Needs = AfflictionKind.Sundered, Needs2 = AfflictionKind.Poison, Do = UpgradeAction.Bite, Delay = 0.3f },
                },
            }, HeartPattern.RightLow));

            // NEEDS: an affliction lasting as long as another on the same enemy (fades-never); until then it does nothing.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Cinnabar, Name = "Cinnabar", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { EnvenomedEdge }, Icon = UpgradeIcons.Gem,
                Description = "Notches last as long as the poison does.",
            }, HeartPattern.LeftHigh));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheUndyingDrink, Name = "The Undying Drink", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { EnvenomedEdge, TheNailPulled }, Icon = UpgradeIcons.Flask,
                Description = "Your flasks are cut with quicksilver: drinking one throws a ring of silver drops 4 m round you that poison three deep and notch everything, but each flask heals a fifth less.",
                Stats = new[] { new StatMod(PlayerStat.FlaskHeal, -0.2f) },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.FlaskDrunk, At = EffectAt.Self, Do = UpgradeAction.Afflict, Radius = 4f, Affliction = AfflictionKind.Poison, Stacks = 3 },
                    new UpgradeEffect { On = UpgradeTrigger.FlaskDrunk, At = EffectAt.Self, Do = UpgradeAction.Afflict, Radius = 4f, Affliction = AfflictionKind.Sundered, Stacks = 1 },
                },
            }, HeartPattern.Keystone));

            // The duo spell, bought with Shards once both are carried (no god gives it): a counter on an 11 s cooldown (SpellTimings). The parry form turns the first blow and
            // answers it; the answer notches the attacker to full (the branch's first element), poisons it three deep and
            // makes its poison bite.
            // NEEDS: the bleeding 4 s (its poison biting every half second, then every dose at once), each weapon blow in it
            // adding a dose, and half the cooldown back when nothing strikes.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Ichor, Name = "Ichor", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Counter, Requires = new[] { EnvenomedEdge, TheNailPulled }, Icon = UpgradeIcons.Shield,
                Description = "You raise the guard. The first blow that lands on it is turned, and your answer pulls the nail: the attacker is notched to full, poisoned three deep, and its poison bites.",
                Form = SpellForms.Parry,
                Spell = new SpellSpec { Seconds = 1.2f, Reach = 3f, Arc = 90f, Damage = 10f, Stagger = 20f, Stacks = 99 },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Poison, Stacks = 3 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Bite, Delay = 0.2f },
                },
            }, HeartPattern.Spell));
        }
    }
}

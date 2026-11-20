using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Water and Earth (pairs_d1_d2.md, "Water and Earth", distance 2; task 4.15), named by its reaction,
    /// The Ground Gives. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape
    /// mark, Wet Ground, opens Quicksand and Sand Boil; Given to the Bog and Driven Down, the keystone The Land Was Sea, and
    /// the duo spell The Island Breathes, the crown. Water is written first: the duo spell is cast in Water.
    /// </summary>
    public static class TheGroundGivesDuo
    {
        public const string Shape = "thegroundgives.shape";
        public const string GivenToTheBog = "thegroundgives.bog";
        public const string DrivenDown = "thegroundgives.drivendown";
        public const string TheLandWasSea = "thegroundgives.landwassea";
        public const string TheIslandBreathes = "thegroundgives.island";
        /// <summary>The conditions the two shapes set, each a sinking as the plain reaction's.</summary>
        public const string Quicksand = "thegroundgives.quicksand";
        public const string SandBoil = "thegroundgives.boil";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Water, B = Elements.Earth;

            // NEEDS: the pool itself (ground-states): a 3 m pool that draws what stands in it to its middle (Quicksand; now
            // the sunk enemy draws in what is within 1.5 m once a second).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "The Ground Gives: Quicksand or Sand Boil", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens The Ground Gives' two shapes, chosen at a rest fire. Quicksand: the sand drags what stands near into its middle. Sand Boil: the pool lasts a moment, then erupts in a fountain of mud. Either runs instead of the plain The Ground Gives.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Quicksand",
                        Description = "It sinks for 4 s, slowed by half, and the sand drags everything within 1.5 m of it 1 m toward it each second, so enemies end up together. A heavy blow holds it fast for 0.5 s.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Quicksand, Seconds = 4f, Beat = 1f, Pace = 0.5f } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Quicksand, Do = UpgradeAction.Pull, Radius = 1.5f, Amount = 1f },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Quicksand, By = StruckBy.Heavy, Do = UpgradeAction.Hold, Seconds = 0.5f },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Sand Boil",
                        Description = "The pool lasts only 1.2 s, then erupts in a fountain of mud and water: it is lifted for 1 s, and everything within 3 m takes 12, Soaked and a Tremor.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = SandBoil, Seconds = 1.2f, Pace = 0.5f } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = SandBoil, Do = UpgradeAction.Lift, Seconds = 1f },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = SandBoil, Do = UpgradeAction.Burst, Radius = 3f, Damage = 12f, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = SandBoil, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Tremor, Stacks = 1 },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // NEEDS: an answer to an enemy that dies in a condition (sunk); now one that dies soaked and shaken.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = GivenToTheBog, Name = "Given to the Bog", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Flask,
                Description = "An enemy that dies sunk is swallowed and the bog gives back: a quarter of a flask.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Soaked, Needs2 = AfflictionKind.Tremor, Do = UpgradeAction.FlaskFill, Amount = 0.25f, Cooldown = 0.5f } },
            }, HeartPattern.LeftLow));

            // NEEDS: a heavy blow that passes the guard; an answer to blows on an enemy in a condition (sunk); now heavy blows
            // on a soaked, shaken enemy hold it 0.5 s.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = DrivenDown, Name = "Driven Down", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Hammer,
                Description = "Heavy blows on a sunk enemy pass its guard and hold it 0.5 s longer.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Soaked, Needs2 = AfflictionKind.Tremor, Do = UpgradeAction.Hold, Seconds = 0.5f } },
            }, HeartPattern.RightLow));

            // NEEDS: a dodge through ground of one's own as a slide (ground-states); now every dodge leaves wet sand behind it
            // and pays back its stamina.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheLandWasSea, Name = "The Land Was Sea", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { GivenToTheBog, DrivenDown }, Icon = UpgradeIcons.Wave,
                Description = "Your dodge through wet sand is a slide: it costs no stamina and leaves 3 m of wet sand behind it for 3 s.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, At = EffectAt.Self, Do = UpgradeAction.Hazard, Shape = HazardShape.Line, Count = 3, Reach = 3f, Radius = 0.8f, Seconds = 3f, Interval = 0.5f, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, Do = UpgradeAction.Stamina, Amount = 15f },
                },
            }, HeartPattern.Keystone));

            // The duo spell, the crown: a breaker on a 15 s cooldown (SpellTimings). The pillars form heaves the ground up under
            // the enemies at the aim point; each one struck is shaken twice as well as soaked, so The Ground Gives sinks it.
            // NEEDS: the in-breath and out-breath as one place (the heave, then 4 s of wet sand); blows on the lifted holding
            // them up and a heavy blow on the sunk at double stagger.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheIslandBreathes, Name = "The Island Breathes", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Breaker, Requires = new[] { GivenToTheBog, DrivenDown }, Icon = UpgradeIcons.Rock,
                Description = "The ground at the aim point breathes in: it heaves up like a back, and every enemy on it is lifted, shaken twice and soaked, so it sinks as The Ground Gives when it comes down.",
                Form = "pillars",
                Spell = new SpellSpec { Count = 5, Reach = 8f, Radius = 2.5f, Damage = 10f, Stagger = 30f, Stacks = 1, Delay = 0.9f },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Tremor, Stacks = 2 } },
            }, HeartPattern.Spell));
        }
    }
}

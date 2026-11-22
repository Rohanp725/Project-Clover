using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Dark and Wind (pairs_d4.md, "Dark and Wind", distance 4; task 4.17), named by its reaction, Breath
    /// Taken. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark, Storm
    /// or Breath, opens Black Storm and Second Wind; Ill Omen and Short of Breath (majors), the keystone The Last Breath, the
    /// duo spell Blown Through and the duo surge Wind From the Grave, each bought with Shards once both are carried. Dark is
    /// written first, as the ring names the pair.
    /// </summary>
    public static class BreathTakenDuo
    {
        public const string Shape = "breathtaken.shape";
        public const string IllOmen = "breathtaken.illomen";
        public const string ShortOfBreath = "breathtaken.short";
        public const string TheLastBreath = "breathtaken.lastbreath";
        public const string BlownThrough = "breathtaken.blownthrough";
        public const string WindFromTheGrave = "breathtaken.grave";
        /// <summary>The conditions the two shapes set (Black Storm's winding on it and on those the streak reaches, Second
        /// Wind's), Short of Breath's and The Last Breath's own winding, and the back Blown Through leaves open.</summary>
        public const string Storm = "breathtaken.storm";
        public const string StormOn = "breathtaken.stormon";
        public const string StormDust = "breathtaken.stormdust";
        public const string Second = "breathtaken.second";
        public const string Gasping = "breathtaken.gasping";
        public const string Last = "breathtaken.last";
        public const string Back = "breathtaken.back";
        private const string Reaction = "react.breathtaken";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Dark, B = Elements.Wind;

            // NEEDS: things thrown from an enemy riding downwind body to body (thrown-from-enemy): Black Storm's streak running
            // on from one to the next; now the two nearest within 6 m are winded, Cursed and Unsteady at once. A boon that ends
            // with the next dodge (until-dodge): Second Wind's next dodge free; now every dodge in the 3 s.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Breath Taken: Black Storm or Second Wind", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Breath Taken's two shapes, chosen at a rest fire. Black Storm: the taken breath runs on as black dust and winds the next ones too. Second Wind: the breath is blown to you, and you draw it in. Either runs instead of the plain Breath Taken.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Black Storm",
                        Description = "It is winded as before, and the taken breath does not scatter: it runs on as a streak of black dust to the next enemies within 6 m, two more at most, and winds each of them too, leaving them Cursed and Unsteady.",
                        Effects = Winded(Storm, new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, Radius = 6f, Count = 2, Affliction = AfflictionKind.Cursed, Stacks = 1,
                                State = new ConditionSpec { Id = StormOn, Seconds = 1.5f, Flags = ConditionFlags.BeginsNoAttack | ConditionFlags.StaggerTakenShare, Scale = 1.3f, ToOthers = true } },
                            // The same two, nearest first, take the streak's Unsteady.
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, Radius = 6f, Count = 2, Affliction = AfflictionKind.Unsteady, Stacks = 1,
                                State = new ConditionSpec { Id = StormDust, Seconds = 1.5f, ToOthers = true } },
                        }),
                    },
                    new ShapeDef
                    {
                        Name = "Second Wind",
                        Description = "It is winded as before, and the breath is blown to you instead: you draw it in, 25 stamina and 8 mana, and your dodges cost nothing for 3 s.",
                        Effects = Winded(Second, new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Stamina, Amount = 25f },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Mana, Amount = 8f },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Boon, Stat = PlayerStat.DodgeStamina, Amount = -1f, Seconds = 3f },
                        }),
                    },
                },
            }, HeartPattern.Foot));

            // NEEDS: the one dodged apart from everything near (attacker-of-turned-blow); now everything within 2.5 m of you is
            // Cursed and made two deeper Unsteady, at most once a second.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = IllOmen, Name = "Ill Omen", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Moon,
                Description = "A perfect dodge Curses the one dodged and leaves it two deeper in Unsteady.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, At = EffectAt.Self, Cooldown = 1f, Do = UpgradeAction.Afflict, Radius = 2.5f, Affliction = AfflictionKind.Cursed, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, At = EffectAt.Self, Cooldown = 1f, Do = UpgradeAction.Afflict, Radius = 2.5f, Affliction = AfflictionKind.Unsteady, Stacks = 2 },
                },
            }, HeartPattern.LeftLow));

            // The winded enemy read from the enemy, in whichever shape: the half again rides on a share that lasts only while it
            // is winded (ConditionSpec.WhileIn) and asks only heavy blows (ConditionFlags.HeavyTakenShare); the clocks are cut by
            // each heavy blow on a winded enemy (UpgradeEffect.InState).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = ShortOfBreath, Name = "Short of Breath", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Hourglass,
                Description = "Your heavy blows on a winded enemy strike half again as hard, and each takes 1 s off every running spell cooldown.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, Do = UpgradeAction.Condition,
                        State = new ConditionSpec { Id = Gasping, Seconds = 2f, Flags = ConditionFlags.HeavyTakenShare, Scale = 1.5f, WhileIn = Reactions.Winded } },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, InState = Reactions.Winded, Cooldown = 0.3f, Do = UpgradeAction.CutCooldowns, Amount = 1f },
                },
            }, HeartPattern.RightLow));

            // A winded enemy that dies lets its last breath out: everything within 4 m pushed 3 m and Cursed. The winding is read
            // from what it carried as it died, in whichever shape (UpgradeEffect.InState).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheLastBreath, Name = "The Last Breath", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { IllOmen, ShortOfBreath }, Icon = UpgradeIcons.Skull,
                Description = "A winded enemy that dies lets its last breath out as a black gust: everything within 4 m is pushed 3 m and Cursed.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Kill, InState = Reactions.Winded, Do = UpgradeAction.Push, Radius = 4f, Amount = 3f },
                    new UpgradeEffect { On = UpgradeTrigger.Kill, InState = Reactions.Winded, Do = UpgradeAction.Afflict, Radius = 4f, Affliction = AfflictionKind.Cursed, Stacks = 1 },
                },
            }, HeartPattern.Keystone));

            // The duo spell, bought with 5 Shards once both are carried (no god gives it): a counter on a 9 s cooldown
            // (SpellTimings). The parry form stands you in the gust for 1 s, cast in Dark (a Curse on what it answers); the
            // answer makes the attacker Unsteady, so its breath is taken, and the wind sets you down 2 m behind it. Your first
            // weapon blow on it within a second is critical and cuts every running clock 4.5 s.
            // NEEDS: the wind carrying you through the attacker untouchable, facing it (form-pieces; now a step to 2 m beyond
            // it); half its own cooldown back rather than seconds off every clock (own-clock-share); the small push round you when
            // nothing comes (missed-counter).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = BlownThrough, Name = "Blown Through", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Counter, Requires = new[] { IllOmen, ShortOfBreath }, Icon = UpgradeIcons.Wing,
                Description = "You stand in a black gust for 1 s. The first blow that comes finds no one: the wind carries you through the attacker and sets you down 2 m behind it, its breath taken. Your first blow from its back within a second is critical and gives back half the wait.",
                Form = SpellForms.Parry,
                Spell = new SpellSpec { Seconds = 1f, Reach = 3f, Arc = 120f, Damage = 6f, Stagger = 15f, Stacks = 1 },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Unsteady, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Cooldown = 0.5f, Do = UpgradeAction.Step, Reach = 4f, Amount = 2f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Condition,
                        State = new ConditionSpec { Id = Back, Seconds = 1f, Flags = ConditionFlags.TakesCritical, EndsWith = ConditionBlow.Weapon } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Back, By = StruckBy.Weapon, Do = UpgradeAction.CutCooldowns, Amount = 4.5f },
                },
            }, HeartPattern.Spell));

            // The duo surge, worn in the Hand's surge slot with its own 35 mana and 10 s (DuoSurge). NEEDS: a streaming field 9 m
            // long and 100 degrees wide turning with you, that carries a Curse laid in it to every enemy downwind (form-pieces);
            // now while it burns, once a second, a cone of black wind ahead lays an Unsteady and everything within 6 m is
            // pushed half a metre away.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = WindFromTheGrave, Name = "Wind From the Grave", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { BlownThrough }, Icon = UpgradeIcons.Gust,
                Description = "A duo surge for your surge slot: 35 mana, 10 seconds. While it lasts a black wind blows from you the way you face, 9 m long and 100 degrees wide: enemies in it are pushed away half a metre a second and made Unsteady each second, and a Curse laid on any in the wind is carried downwind.",
                Surge = new DuoSurge { Mana = 35f, Seconds = 10f },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.EverySeconds, Every = 1f, WhileSurge = true, Do = UpgradeAction.Cone, Reach = 9f, Arc = 100f, Affliction = AfflictionKind.Unsteady, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.EverySeconds, Every = 1f, WhileSurge = true, At = EffectAt.Self, Do = UpgradeAction.Push, Radius = 6f, Amount = 0.5f },
                },
            }, HeartPattern.RightTop));
        }

        // The winding as the plain Breath Taken's, under the shape's own name: 1.5 s (an elite 0.6 s) of beginning no attack
        // and 30% more stagger. Then the shape's own.
        private static UpgradeEffect[] Winded(string id, UpgradeEffect[] more)
        {
            var effects = new List<UpgradeEffect>
            {
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Ranks = EnemyRank.Plain | EnemyRank.God, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = id, Seconds = 1.5f, Flags = ConditionFlags.BeginsNoAttack | ConditionFlags.StaggerTakenShare, Scale = 1.3f, Is = Reactions.Winded } },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Ranks = EnemyRank.Elite, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = id, Seconds = 0.6f, Flags = ConditionFlags.BeginsNoAttack | ConditionFlags.StaggerTakenShare, Scale = 1.3f, Is = Reactions.Winded } },
            };
            effects.AddRange(more);
            return effects.ToArray();
        }
    }
}

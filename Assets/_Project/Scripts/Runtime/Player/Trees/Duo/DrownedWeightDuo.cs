using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Water and Dark (pairs_d5.md, "Water and Dark", distance 5; task 4.17), named by its reaction,
    /// Drowned Weight. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape
    /// mark, Bound in Debt, opens Drawn Under and The Pull of Night; Held Under (a major), Deep Swell (a minor), the keystone
    /// The Debt Passes, and the duo spell Under the Black Water. Water is written first, as the ring names the pair.
    /// </summary>
    public static class DrownedWeightDuo
    {
        public const string Shape = "drownedweight.shape";
        public const string HeldUnder = "drownedweight.heldunder";
        public const string DeepSwell = "drownedweight.deepswell";
        public const string TheDebtPasses = "drownedweight.debtpasses";
        public const string UnderTheBlackWater = "drownedweight.blackwater";
        /// <summary>The prefixes of the conditions the two shapes set, each the swelling as the plain reaction's (prefix
        /// and prefix.heavy).</summary>
        public const string DrawnUnder = "drownedweight.under";
        public const string PullOfNight = "drownedweight.night";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Water, B = Elements.Dark;

            // NEEDS: pull-to-point (Drawn Under's pool drawing the others in at 1.5 m a second; now a pull of 2 m at once);
            // afflicted-only (The Pull of Night drawing only the Soaked; now every enemy within 5 m).
            var under = new List<UpgradeEffect>(Swell(DrawnUnder, true))
            {
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hold, Seconds = 1.5f },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Pull, Radius = 3f, Amount = 2f },
            };
            var night = new List<UpgradeEffect>(Swell(PullOfNight, false))
            {
                new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = PullOfNight, By = StruckBy.Weapon, Do = UpgradeAction.Pull, Radius = 5f, Amount = 1f, Cooldown = 0.3f },
            };
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Drowned Weight: Drawn Under or The Pull of Night", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Drowned Weight's two shapes, chosen at a rest fire. Drawn Under: as it swells it sinks into a pool of black water that draws the others to it. The Pull of Night: no burst; each of your blows on it draws the wet toward it. Either runs instead of the plain Drowned Weight.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Drawn Under",
                        Description = "It swells as before, and the black water pulls down: it sinks knee-deep into a pool 3 m across, held 1.5 s, and every other enemy in the pool is drawn toward it. It bursts as before when it dies or a heavy blow lands.",
                        Effects = under.ToArray(),
                    },
                    new ShapeDef
                    {
                        Name = "The Pull of Night",
                        Description = "It swells as before but does not burst. While it is swollen, each of your weapon blows on it draws every other Soaked enemy within 5 m 1 m toward it, as the night sea draws all things back into itself.",
                        Effects = night.ToArray(),
                    },
                },
            }, HeartPattern.Foot));

            // NEEDS: a mark's answer to an enemy in a condition (condition-needs: swollen); now a perfect guard against one
            // both Soaked and Cursed, once a second.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = HeldUnder, Name = "Held Under", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard against a swollen enemy pushes it down knee-deep: held 1 s, and the attack it was making is lost.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Soaked, Needs2 = AfflictionKind.Cursed, Do = UpgradeAction.Flinch, Seconds = 0.4f, Cooldown = 1f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Soaked, Needs2 = AfflictionKind.Cursed, Do = UpgradeAction.Hold, Seconds = 1f, Cooldown = 1f },
                },
            }, HeartPattern.LeftLow));

            // NEEDS: a mark that changes the reaction's own numbers (reaction-numbers); until then it does nothing.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = DeepSwell, Name = "Deep Swell", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { Shape }, Icon = UpgradeIcons.Hourglass,
                Description = "Drowned Weight stays swollen 2 s longer, and its burst reaches 5 m.",
            }, HeartPattern.RightLow));

            // The debt passes on: a Curse laid by the mark (not the reaction, so on purpose it reacts) on every enemy near a
            // soaked, cursed enemy that dies, and 6 more health for the kill.
            // NEEDS: an answer to an enemy that dies swollen (death-in-condition), the nearest one the burst reaches swelling
            // alone; now every enemy within 4 m of a soaked, cursed enemy that dies is Cursed, once a second.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheDebtPasses, Name = "The Debt Passes", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { HeldUnder, DeepSwell }, Icon = UpgradeIcons.Chain,
                Description = "When a swollen enemy bursts, the nearest enemy the burst reaches swells at once, and killing a swollen enemy heals you 6 more.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Soaked, Needs2 = AfflictionKind.Cursed, Do = UpgradeAction.Afflict, Radius = 4f, Affliction = AfflictionKind.Cursed, Stacks = 1, Cooldown = 1f },
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Soaked, Needs2 = AfflictionKind.Cursed, Do = UpgradeAction.Heal, Amount = 6f },
                },
            }, HeartPattern.Keystone));

            // The duo spell, a movement on a 10 s cooldown (SpellTimings), bought with 5 Shards once both are carried. The step
            // form takes you to the enemy faced through the black water, soaking it (the branch's first element); it is Cursed
            // and held knee-deep 1 s, and the water you rose from stays as a pool that soaks.
            // NEEDS: the player and the enemy changing places (form-pieces: you rise where it stood and it where you stood,
            // gods and elites unmoved with you behind them; now you step to it and it stays), the pool cursing as well as soaking,
            // and the next weapon blow within 2 s dealing half again (now the next heavy blow is critical).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = UnderTheBlackWater, Name = "Under the Black Water", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Movement, Requires = new[] { HeldUnder, DeepSwell }, Icon = UpgradeIcons.Drop,
                Description = "Black water opens under you and under the enemy you face. Both sink and each rises out of the other's water: you where it stood, it where you stood, Soaked, Cursed and held knee-deep 1 s. The water you rose from stays 3 s as a pool that soaks.",
                Form = SpellForms.Step,
                Spell = new SpellSpec { Reach = 9f, Damage = 8f, Stagger = 15f, Stacks = 1, Seconds = 2f },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Cursed, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Hold, Seconds = 1f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, At = EffectAt.Self, Do = UpgradeAction.Hazard, Radius = 1.25f, Seconds = 3f, Interval = 1f,
                        Affliction = AfflictionKind.Soaked, Stacks = 1 },
                },
            }, HeartPattern.Spell));
        }

        // The swelling as the plain Drowned Weight's, under the shape's own names (prefix, prefix.heavy): 5 s, 15% more from
        // weapon blows and 20% slower; with the burst, it bursts in black water to 4 m (14, 20 stagger, Soaked and Cursed)
        // when it dies or a heavy blow lands on it.
        private static UpgradeEffect[] Swell(string prefix, bool bursts)
        {
            var effects = new List<UpgradeEffect>
            {
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = prefix, Seconds = 5f, Flags = ConditionFlags.WeaponTakenShare, Scale = 1.15f, EndsWith = bursts ? ConditionBlow.Heavy : ConditionBlow.None } },
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = prefix + ".heavy", Seconds = 5f, Pace = 0.8f } },
            };
            if (bursts)
                foreach (var on in new[] { UpgradeTrigger.ConditionStruck, UpgradeTrigger.ConditionDied })
                {
                    effects.Add(new UpgradeEffect { On = on, Condition = prefix, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 4f, Damage = 14f, Stagger = 20f, Affliction = AfflictionKind.Soaked, Stacks = 1 });
                    effects.Add(new UpgradeEffect { On = on, Condition = prefix, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 4f, Affliction = AfflictionKind.Cursed, Stacks = 1 });
                }
            return effects.ToArray();
        }
    }
}

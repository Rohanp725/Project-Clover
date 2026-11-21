using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Water and Metal (pairs_d3.md, "Water and Metal", distance 3; task 4.16), named by its reaction,
    /// Rust. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark opens
    /// Seized Joints and Flaking; Salt Edge and Salt Guard (majors), Brine (a minor), the keystone The Wreck, and the duo
    /// spell The Sea Takes It. Water is written first.
    /// </summary>
    public static class RustDuo
    {
        public const string Shape = "rust.shape";
        public const string SaltEdge = "rust.saltedge";
        public const string SaltGuard = "rust.saltguard";
        public const string Brine = "rust.brine";
        public const string TheWreck = "rust.wreck";
        public const string TheSeaTakesIt = "rust.seatakesit";
        /// <summary>The conditions the two shapes set, each a rusting and its fallen plate as the plain reaction's.</summary>
        public const string Joints = "rust.joints";
        public const string JointsPlate = "rust.jointsplate";
        public const string Flaking = "rust.flaking";
        public const string FlakingPlate = "rust.flakingplate";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Water, B = Elements.Metal;

            // NEEDS: Sundered past three (sundered-past-three), as the plain reaction; a slowing and a turning slowed by the
            // notch (Seized Joints; now the rusting enemy walks a fifth slower).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Rust: Seized Joints or Flaking", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Rust's two shapes, chosen at a rest fire. Seized Joints: the rust slows it so it cannot turn to face you in time. Flaking: a heavy blow knocks the rust off in a shower of flakes. Either runs instead of the plain Rust.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Seized Joints",
                        Description = "It rusts as before, its notches kept and spreading while it is wet, a plate falling when it is rusted through; and while it rusts its joints seize, so it walks and turns slower. Go round it.",
                        Effects = Rusting(Joints, JointsPlate, 0.8f, new UpgradeEffect[0]),
                    },
                    new ShapeDef
                    {
                        Name = "Flaking",
                        Description = "It rusts as before; each heavy blow knocks the rust off in a shower: its notches are spent, and each throws flakes at the enemies within 3 m, 6, a notch and Soaked.",
                        Effects = Rusting(Flaking, FlakingPlate, 1f, new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Flaking, By = StruckBy.Heavy, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Sundered },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Flaking, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 3f, DamagePerSpent = 6f, Affliction = AfflictionKind.Sundered, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Flaking, By = StruckBy.Heavy, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                        }),
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = SaltEdge, Name = "Salt Edge", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Blade,
                Description = "Heavy blows lay one notch and Soak.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Sundered, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = SaltGuard, Name = "Salt Guard", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard lays one notch and Soaks the attacker.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Sundered, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                },
            }, HeartPattern.RightLow));

            // NEEDS: an affliction lasting longer while another lies on the same enemy (fades-never); until then nothing.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Brine, Name = "Brine", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { SaltEdge }, Icon = UpgradeIcons.Drop,
                Description = "Soaked lasts longer on a notched enemy.",
            }, HeartPattern.LeftHigh));

            // NEEDS: heavy blows struck from inside ground of one's own not broken by blows taken (ground-states); the wreck is a
            // patch that soaks and notches.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheWreck, Name = "The Wreck", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { SaltEdge, SaltGuard }, Icon = UpgradeIcons.Anvil,
                Description = "A rusted enemy that dies leaves its armour on the floor as a wreck 1.5 m across for 8 s. It Soaks and notches whatever stands in it, and your heavy blows struck from inside it are not broken by blows taken.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Soaked, Needs2 = AfflictionKind.Sundered, Do = UpgradeAction.Hazard, Radius = 0.75f, Seconds = 8f, Interval = 1f, Affliction = AfflictionKind.Soaked, Stacks = 1, Cooldown = 0.5f },
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Soaked, Needs2 = AfflictionKind.Sundered, Do = UpgradeAction.Hazard, Radius = 0.75f, Seconds = 8f, Interval = 2f, Affliction = AfflictionKind.Sundered, Stacks = 1, Cooldown = 0.5f },
                },
            }, HeartPattern.Keystone));

            // The duo spell, bought with Shards once both are carried (no god gives it): a breaker on a 13 s cooldown (SpellTimings). The wave form throws a cone of brine ahead
            // that Soaks (the branch's first element); every notched enemy it crosses rusts through to full.
            // NEEDS: its joints seizing (40% slower turning for 4 s) and a heavy blow on a seized enemy knocking a plate off.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheSeaTakesIt, Name = "The Sea Takes It", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Breaker, Requires = new[] { SaltEdge, SaltGuard }, Icon = UpgradeIcons.Wave,
                Description = "A wave of brine rolls ahead. Everything it crosses is Soaked, and every notched enemy it crosses rusts through to full at once.",
                Form = SpellForms.Wave,
                Spell = new SpellSpec { Reach = 7f, Arc = 50f, Damage = 8f, Stagger = 10f, Stacks = 1 },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Needs = AfflictionKind.Sundered, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Sundered, Stacks = 99 } },
            }, HeartPattern.Spell));
        }

        // Rusting as the plain Rust's: while Soaked its notches stay and spread one every 2 s, and once rusted through a plate
        // falls (weapon blows 16% harder for 6 s); a pace below 1 slows it while it rusts. Then the shape's own answers.
        private static UpgradeEffect[] Rusting(string id, string plate, float pace, UpgradeEffect[] more)
        {
            var effects = new List<UpgradeEffect>
            {
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = id, Seconds = 30f, Beat = 2f, Pace = pace, While = AfflictionKind.Soaked, Keeps = AfflictionKind.Sundered } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = id, Needs = AfflictionKind.Sundered, NeedsFull = true, Do = UpgradeAction.Condition,
                    State = new ConditionSpec { Id = plate, Seconds = 6f, Flags = ConditionFlags.WeaponTakenShare, Scale = 1.16f } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = id, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Sundered, Stacks = 1 },
            };
            effects.AddRange(more);
            return effects.ToArray();
        }
    }
}

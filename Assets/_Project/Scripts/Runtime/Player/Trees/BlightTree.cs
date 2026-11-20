using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Blight: the duo branch Dark and Poison share, reshaped by pairs_d1_d2.md ("Dark and Poison", task 4.15) and drawn in
    /// the chart's heart (<see cref="HeartPattern"/>). Its reaction is Shut in the Dark; the Shape mark opens its two shapes,
    /// Swarm and Rot to the Bone. Carrion Flies is now Swarm and Grave Soil, offered to Dark's or Poison's tree, is cut
    /// because both rosters are full; both are in <see cref="RetiredMarks"/>. Wither is re-aimed to a minor (the curse's half
    /// of it overlapped the jar). Kept: Feeding Rot, One Death Feeds (the keystone), Wasting Grip (the duo spell, re-aimed at
    /// the jar) and The Reaping, begun by a finisher, the branch's top mark. Open while both are carried.
    /// </summary>
    public static class BlightTree
    {
        public const string Shape = "blight.shape";
        public const string Wither = "blight.wither";
        public const string FeedingRot = "blight.feedingrot";
        public const string WastingGrip = "blight.wastinggrip";
        public const string OneDeathFeeds = "blight.onedeathfeeds";
        public const string TheReaping = "blight.reaping";
        /// <summary>The jars the two shapes seal, each in place of the plain reaction's.</summary>
        public const string SwarmJar = "shutinthedark.swarm";
        public const string RotJar = "shutinthedark.rot";

        // Spell forms by the ids fixed in docs/design/FORMS.md.
        private const string FormBeam = "beam";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Dark, B = Elements.Poison;
            const AfflictionKind Cursed = AfflictionKind.Cursed, Poisoned = AfflictionKind.Poison;

            // NEEDS: a swarm that crawls to the nearest enemy and seals a new jar there (Swarm; now the breaking jar throws its
            // doses and a Curse at the nearest enemy within 8 m, on purpose able to react); bites that fill the stagger meter by
            // the dose and a meter that does not recover while sealed (Rot to the Bone; now 9 stagger each second).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Shut in the Dark: Swarm or Rot to the Bone", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Shut in the Dark's two shapes, chosen at a rest fire. Swarm: when the jar breaks, its swarm crawls on and seals another. Rot to the Bone: the sealed one's balance is eaten as it bites. Either runs instead of the plain Shut in the Dark.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Swarm",
                        Description = "The jar is sealed as before for 8 s, its doses kept, and breaks as before. When it breaks, its swarm crawls to the nearest enemy within 8 m with 2 doses and a Curse, to seal a new jar there.",
                        Effects = Jar(SwarmJar, new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = SwarmJar, Do = UpgradeAction.Arc, FromTarget = true, Count = 1, Reach = 8f, Affliction = Poisoned, Stacks = 2, Cascades = true },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = SwarmJar, Do = UpgradeAction.Arc, FromTarget = true, Count = 1, Reach = 8f, Affliction = Cursed, Stacks = 1, Cascades = true },
                        }),
                    },
                    new ShapeDef
                    {
                        Name = "Rot to the Bone",
                        Description = "The jar is sealed as before for 8 s, its doses kept, and breaks as before; while it is sealed the rot eats its balance, 9 stagger every second.",
                        Effects = Jar(RotJar, new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = RotJar, Do = UpgradeAction.Burst, Radius = 0.5f, Stagger = 9f },
                        }, beat: 1f),
                    },
                },
            }, HeartPattern.Foot));

            // Re-aimed to a minor (pairs_d1_d2.md): the curse's half of it overlapped the jar. Built in the beat
            // (Afflictions.PoisonOnCursed, kept by PlayerUpgrades): a quarter harder on the Cursed, and it does not fade while
            // the curse is on them.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Wither, Name = "Wither", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { Shape }, Icon = UpgradeIcons.Leaf,
                Description = "Poison bites a quarter harder on the Cursed, and does not fade from them while the curse lasts.",
            }, HeartPattern.LeftLow));

            // On the bite itself (UpgradeTrigger.PoisonBite): half a point of health for each dose that bit.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = FeedingRot, Name = "Feeding Rot", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Plus,
                Description = "Each time poison bites a Cursed enemy, a little of it comes back to you as health.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PoisonBite, Do = UpgradeAction.Heal, Needs = Cursed, PerStack = true, Amount = 0.5f } },
            }, HeartPattern.RightLow));

            // Built on the beat (UpgradeAction.Bite): every poisoned enemy within twelve metres of the dead one bites on the next step, and each one bitten feeds four points.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = OneDeathFeeds, Name = "One Death Feeds", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { Wither, FeedingRot }, Icon = UpgradeIcons.Skull,
                Description = "When a Cursed enemy dies, the poison in every other enemy near it bites at once, and you are fed for each one bitten.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Cursed, Do = UpgradeAction.Bite, Radius = 12f, Amount = 4f, Cooldown = 0.5f } },
            }, HeartPattern.Keystone));

            // The duo spell, re-aimed at the jar (pairs_d1_d2.md): the stream seals a jar on what it bites, past the rest.
            // BESPOKE: the grip that holds an enemy, drinks its poison and throws it. Until then a short held stream bites what
            // stands before the player, and spells that strike the poisoned feed.
            // NEEDS: a jar sealed on an enemy carrying only one of the two (forced-alone; now only on one Cursed and poisoned);
            // each second of the stream adding a dose to whatever is inside the ring.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = WastingGrip, Name = "Wasting Grip", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Opener, Requires = new[] { Wither, FeedingRot }, Icon = UpgradeIcons.Claw,
                Description = "You brace and pour a short stream of rot at what stands before you. It bites again and again, and striking a poisoned enemy with it feeds you. On one Cursed and poisoned it seals the jar at once, whatever its rest. A blow that lands on you breaks it. 40 mana.",
                Form = FormBeam,
                Spell = new SpellSpec { Cost = 40f, Reach = 2.5f, Radius = 0.6f, Seconds = 2.5f, Interval = 0.5f, Damage = 8f, Stagger = 6f, Stacks = 1 },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, Do = UpgradeAction.Heal, Needs = Poisoned, Amount = 5f, Cooldown = 1f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Needs = Poisoned, Needs2 = Cursed, Do = UpgradeAction.ForceReaction, Reaction = "react.shutinthedark", Cooldown = 2.5f },
                },
            }, HeartPattern.Spell));

            // BESPOKE (built): ReapingForm. The branch's top mark, begun by a finisher.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheReaping, Name = "The Reaping", Element = A, Element2 = B, Kind = UpgradeKind.Signature, Icon = UpgradeIcons.Crown,
                Description = "Your finisher on a Cursed, poisoned enemy reaps the field. All the poison in it and in every enemy near lands at once, and half of it is yours.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Form, Form = SpellForms.Reaping, Needs = AfflictionKind.Cursed, Needs2 = AfflictionKind.Poison, Affliction = AfflictionKind.Poison, Spell = new SpellSpec { Cost = 0f, Radius = 9f, Damage = 3f, Stagger = 4f } } },
            }, HeartPattern.Crown));
        }

        // The plain reaction's jar under a shape's own name: sealed 8 s with its doses kept; breaking at the end, 5 a dose in
        // 3 m and half its doses on all it reaches, or as it dies; and the shape's own answers after.
        private static UpgradeEffect[] Jar(string id, UpgradeEffect[] more, float beat = 0f)
        {
            var effects = new List<UpgradeEffect>
            {
                new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = id, Seconds = 8f, Beat = beat, Keeps = AfflictionKind.Poison } },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = id, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Poison, CountOnly = true },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = id, Do = UpgradeAction.Burst, Radius = 3f, DamagePerSpent = 5f, Affliction = AfflictionKind.Poison },
                new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = id, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Poison, Stacks = 1, PerStack = true, Share = 0.5f },
                new UpgradeEffect { On = UpgradeTrigger.ConditionDied, Condition = id, Needs = AfflictionKind.Poison, Do = UpgradeAction.Burst, Radius = 3f, Damage = 5f, PerStack = true, Affliction = AfflictionKind.Poison },
                new UpgradeEffect { On = UpgradeTrigger.ConditionDied, Condition = id, Needs = AfflictionKind.Poison, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Poison, Stacks = 1, PerStack = true, Share = 0.5f },
            };
            effects.AddRange(more);
            return effects.ToArray();
        }
    }
}

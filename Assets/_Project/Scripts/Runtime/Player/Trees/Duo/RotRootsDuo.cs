using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Life and Poison (pairs_d5.md, "Life and Poison", distance 5; task 4.17), named by its reaction,
    /// Rot Roots. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark,
    /// Bad Ground, opens Gathered In and Summer Grass; Tasting (a major), Deep Threads (a minor), the keystone The Host, and
    /// the duo spell The Stalks Rise. Life is written first, as the ring names the pair.
    /// </summary>
    public static class RotRootsDuo
    {
        public const string Shape = "rotroots.shape";
        public const string Tasting = "rotroots.tasting";
        public const string DeepThreads = "rotroots.deepthreads";
        public const string TheHost = "rotroots.host";
        public const string TheStalksRise = "rotroots.stalks";
        /// <summary>The conditions the two shapes set: Gathered In's drinking roots on each joined enemy, and Summer
        /// Grass's fungus in the host, waiting for it to die.</summary>
        public const string Gathered = "rotroots.gathered";
        public const string SummerGrass = "rotroots.summergrass";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Life, B = Elements.Poison;

            // Gathered In draws each joined enemy's bitten dose along the roots into the host itself, to eight (UpgradeEffect.ToLink).
            // Summer Grass's 6 health comes only if you stand inside the spores as they burst (UpgradeEffect.YouInside on a brief
            // cloud laid with the burst).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Rot Roots: Gathered In or Summer Grass", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Rot Roots' two shapes, chosen at a rest fire. Gathered In: the roots drink, drawing the crowd's poison into the host. Summer Grass: no roots; when the host dies a stalk grows from it and bursts in spores. Either runs instead of the plain Rot Roots.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Gathered In",
                        Description = "The roots run out to three enemies within 5 m and Seed them as before, but they drink instead of giving: each bite of poison on a joined enemy draws one dose out of it along the roots into the host, to eight, so the crowd's poison gathers in one body for your detonation.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, Radius = 5f, Count = 3, Affliction = AfflictionKind.Seeded, Stacks = 1,
                                State = new ConditionSpec { Id = Gathered, Seconds = 6f, OnBite = true, ToOthers = true } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Gathered, Do = UpgradeAction.Pass, Affliction = AfflictionKind.Poison, Stacks = 1, ToLink = true },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Summer Grass",
                        Description = "No roots. The fungus waits in the host: when it dies a dark stalk grows from its head and after 1.5 s bursts in a puff of spores 4 m across, three doses and a Seed on everything in it, and 6 health to you if you stand inside.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = SummerGrass, Seconds = 20f } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionDied, Condition = SummerGrass, Do = UpgradeAction.Burst, Radius = 4f, Delay = 1.5f, Affliction = AfflictionKind.Poison, Stacks = 3 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionDied, Condition = SummerGrass, Do = UpgradeAction.Burst, Radius = 4f, Delay = 1.5f, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionDied, Condition = SummerGrass, Do = UpgradeAction.Hazard, Radius = 4f, Delay = 1.5f, Seconds = 0.3f, Interval = 0.5f, Damage = 0f, Stacks = 0,
                                Affliction = AfflictionKind.Poison, Unseen = true, YouInside = new[] { new UpgradeEffect { On = UpgradeTrigger.InGround, Do = UpgradeAction.Heal, Amount = 6f, Cooldown = 2f } } },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // NEEDS: a state of standing on the roots (in-own-ground); now each poison bite on a Seeded enemy heals you 1,
            // wherever you stand, at most twice a second.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Tasting, Name = "Tasting", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Heart,
                Description = "While you stand on a root, each poison bite on a rooted enemy heals you 1.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PoisonBite, Needs = AfflictionKind.Seeded, Do = UpgradeAction.Heal, Amount = 1f, Cooldown = 0.5f } },
            }, HeartPattern.LeftLow));

            // Changes its reaction's own numbers while it is in force (reaction-numbers, ReactionNumbers):
            // roots reaching 7 m and joining four enemies, not three.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = DeepThreads, Name = "Deep Threads", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { Shape }, Icon = UpgradeIcons.Hourglass,
                Description = "Rot Roots' roots reach 7 m and join one more enemy.",
                ReactionChanges = new[]
                {
                    new ReactionChange { Shape = -1, Index = 0, Radius = 7f, Count = 4 },
                    // Gathered In's doses go along the roots to the host, however far: only the roots' own reach is longer.
                    new ReactionChange { Shape = 0, Index = 0, Radius = 7f, Count = 4 },
                },
            }, HeartPattern.RightLow));

            // The grown thing nearest where a poisoned enemy fell, within 3 m, stands 3 s longer and doses with every bite (Tend, BitesLay).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheHost, Name = "The Host", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { Tasting, DeepThreads }, Icon = UpgradeIcons.Leaf,
                Description = "Whatever you grow feeds on the poisoned: a poisoned enemy that dies within 3 m of something grown makes it stand 3 s longer and lay a dose of poison with every bite.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Poison, Do = UpgradeAction.Tend, Radius = 3f, Seconds = 3f, BitesLay = AfflictionKind.Poison, Cooldown = 0.2f },
                },
            }, HeartPattern.Keystone));

            // The duo spell, a detonator on a 14 s cooldown (SpellTimings), bought with 5 Shards once both are carried. The
            // nova reaches every enemy within 9 m; on each that carries both, a stalk grows and bursts 1.2 s later, spending
            // its doses (5 a dose) and its seeds (8 a stack) in 2.5 m, the spores laying two doses on what is near.
            // NEEDS: a weapon blow on an enemy while its stalk stands doubling that stalk's spores (while-spell-stands); its
            // doses spent with the seeds (now counted for the burst and left on it).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheStalksRise, Name = "The Stalks Rise", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Detonator, Requires = new[] { Tasting, DeepThreads }, Icon = UpgradeIcons.Spike,
                Description = "Every enemy within 9 m that is both Seeded and Poisoned grows a dark stalk from its head; after 1.2 s each stalk bursts, spending both: 5 a dose and 8 a seed in 2.5 m, the spores laying two doses on what is near.",
                Form = SpellForms.Nova,
                Spell = new SpellSpec { Radius = 9f, Delay = 0.2f, Damage = 2f, Stagger = 2f, Stacks = 0 },
                Effects = new[]
                {
                    // Every effect asks for both as it is tried, so a Seeded or a poisoned enemy alone is left be. The doses are
                    // counted, not taken, as the seeds' spending leaves nothing that tells the two apart afterwards.
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Needs = AfflictionKind.Poison, Needs2 = AfflictionKind.Seeded, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Poison, CountOnly = true },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Needs = AfflictionKind.Poison, Needs2 = AfflictionKind.Seeded, Do = UpgradeAction.Burst, Radius = 2.5f, Delay = 1.2f, DamagePerSpent = 5f,
                        Stagger = 15f, Affliction = AfflictionKind.Poison, Stacks = 2 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Needs = AfflictionKind.Poison, Needs2 = AfflictionKind.Seeded, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Seeded },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Needs = AfflictionKind.Poison, Do = UpgradeAction.Burst, Radius = 2.5f, Delay = 1.2f, DamagePerSpent = 8f, Unseen = true,
                        Affliction = AfflictionKind.Seeded },
                },
            }, HeartPattern.Spell));
        }
    }
}

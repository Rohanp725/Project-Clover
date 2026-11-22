using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Life: the garden, written as spell branches (docs/design/steel/elements_a.md, Life; Phase 4). Six spells of four
    /// marks each and ten passives on the pattern of <see cref="BranchPattern"/>: in the middle Garden (Fallen Seeds, Reap
    /// What Is Sown and Elder Tree, the lift Root and Branch and Devotion above it), on the left Harvest (Ripening, Drawn
    /// From the Wood, The Garden Holds You, Root Walk, Wildwood at the crown), on the right Bramble (Bitter Rind, Briar
    /// Patch, Briar Coat and Sapling). Briar Patch is the base spell: it comes with Life, its Learn is never bought, and it
    /// grows from the root.
    ///
    /// Marks the design keeps keep their ids (Sapling, once a major, is a spell now; Red Harvest is rebuilt as Drawn From
    /// the Wood under its id); the cut ones are in <see cref="RetiredMarks"/>. All of it is data. What the data cannot say
    /// yet is the nearest working version with a <c>NEEDS</c> line, gathered in docs/design/steel/content_open.tsv.
    /// </summary>
    public static class LifeTree
    {
        public const string Root = "life.seed";
        public const string FallenSeeds = "life.fallen";
        public const string Ripening = "life.ripen";
        public const string TheGardenHoldsYou = "life.garden";
        public const string Wildwood = "life.wildwood";
        public const string BitterRind = "life.rind";
        public const string BriarCoat = "life.coat";
        public const string RootAndBranch = "life.lift";
        public const string WoodGrowsBack = "life.devotion";
        // The two keystones the owner picked on 2026-11-20 (keystones_fire_water_life.md). Shared Roots comes back under a
        // new id, since life.roots was retired with its refund and a retired id is never used again.
        public const string SharedRoots = "life.sharedroots";
        public const string InItsSeason = "life.inseason";
        /// <summary>Kept as a mark by the owner's answer of 2026-11-20, under its old id.</summary>
        public const string Shade = "life.shade";
        /// <summary>In Its Season: seconds a seed lies alone before it is ripe, and what a ripe and an unripe seed's burst is multiplied by.</summary>
        public const float RipensAfter = 6f;
        public const float RipeShare = 3f;
        public const float UnripeShare = 0.5f;
        // The spells: each is its branch's Learn mark and the branch's id.
        public const string BriarPatch = "life.base";
        public const string Sapling = "life.sapling";
        public const string RootWalk = "life.walk";
        public const string ReapWhatIsSown = "life.reap";
        public const string ElderTree = "life.tree";
        public const string DrawnFromTheWood = "life.harvest";
        // The god's gift.
        public const string Gift = "life.spring";
        // Conditions the branches set on enemies.
        public const string Burred = "life.base.burrs";
        public const string Thicket = "life.base.thicket";
        public const string Strangled = "life.sapling.strangler";
        public const string Dodder = "life.sapling.dodder";
        public const string Changeling = "life.walk.changeling";

        // Retired in Phase 4 (RetiredMarks): kept as names because code still asks for them.
        public const string Heartwood = "life.heartwood";
        public const string ReadySoil = "life.soil";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Life;

            to.Add(BranchPattern.Put(new Upgrade { Id = Root, Name = "Seeded", Element = E, Kind = UpgradeKind.Root, Icon = UpgradeIcons.Leaf,
                Description = "Blows plant a seed in an enemy. Each blow on a Seeded enemy mends you a little. When it dies, it bursts into thorns.",
                WeaponStacks = 1 }, BranchPattern.Root));

            // ---- The middle bough: Garden. Things that grow where you fight, and the reaping of them.
            to.Add(BranchPattern.Put(new Upgrade { Id = FallenSeeds, Name = "Fallen Seeds", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, Icon = UpgradeIcons.Skull,
                Description = "Where a Seeded enemy dies, a thornbush grows and stands for a while.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Seeded, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Grown = true, Limit = 4, Radius = 2f, Seconds = 8f, Interval = 0.5f, Damage = 4f, Affliction = AfflictionKind.Seeded, Stacks = 1 } } },
                BranchPattern.MiddleFoot));

            // The detonator: the nova is the call, every grown thing bursts and mends, and the seeds it reaches burst now.
            // NEEDS: every Seeded enemy in the fight bursting, not only those within the call's six metres (grown-bursts).
            SpellBranch.Add(to, E, ReapWhatIsSown, "Reap What Is Sown", SpellRole.Detonator, BranchPattern.MiddleA, FallenSeeds, SpellForms.Nova,
                new SpellSpec { Radius = 6f, Delay = 0.2f, Damage = 16f, Stagger = 25f, Stacks = 0 },
                "You call in the harvest. Everything you have grown bursts where it stands, 16 damage and 25 stagger three metres round, and every Seeded enemy's thorns burst now and its seed is spent. You mend 5 for each burst.",
                UpgradeIcons.Burst,
                // Mending 7 a burst is the Learn's own mending, set by the Empower (empower-effects).
                new Empower { Cooldown = 10f, Effects = new[] { new EffectChange { Index = 0, Amount = 7f }, new EffectChange { Index = 2, Amount = 7f } } },
                "Reap What Is Sown waits 10 seconds, not 12, and you mend 7 a burst.",
                // NEEDS: the garden kept standing (a shape that sets aside its Learn's burst, shape-sets-aside). Until then the
                // gold sweep reaches eight metres all round, and the garden still bursts.
                new ShapeDef { Name = "Golden Sickle", Form = SpellForms.Wave,
                    Description = "Only the Seeded within eight metres are reaped, each cut once by one sweep of gold, and the garden is kept standing.",
                    Spell = new SpellSpec { Reach = 8f, Arc = 360f, Damage = 16f, Stagger = 25f, Stacks = 0 } },
                // NEEDS: each grown thing's burst sowing too (grown-bursts). Until then each seed set off sows a
                // seed in every enemy within three metres of it.
                new ShapeDef { Name = "Second Sowing",
                    Description = "Everything bursts as before, and each burst sows a new seed in all it reaches. The loop keeps going.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SetOff, Spent = AfflictionKind.Seeded, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Seeded, Stacks = 1 } } },
                "Opens two shapes of Reap What Is Sown, chosen at any rest fire: Golden Sickle, the Seeded reaped and the garden kept, or Second Sowing, every burst sowing again.",
                // NEEDS: cast within 0.5 s of a Seeded enemy dying (spell-windows), and the garden's bursts and the mending counted
                // twice too. Until then for half a second after a Seeded enemy dies every seed set off bursts twice as hard.
                new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Seeded, Do = UpgradeAction.Boon, Stat = PlayerStat.BurstDamage, Amount = 1f, Seconds = 0.5f } },
                "Cast it within half a second of a Seeded enemy dying, and every burst counts twice.",
                learnEffects: new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.BurstGrown, Damage = 16f, Stagger = 25f, Radius = 3f, Amount = 5f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = AfflictionKind.Seeded, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Seeded, Damage = 11f, Stagger = 5f, Radius = 3f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = AfflictionKind.Seeded, Do = UpgradeAction.Heal, Amount = 5f },
                });

            // The larger spell: the rod is the tree, planted at your feet. Its ring under the crown keeps what is inside Seeded,
            // and a count of fifteen mends you 3 a second while you fight. NEEDS: the tree's 40 health while it grows and the
            // enemies turning on it (standing-things), each root under one enemy, a staggered one first (it strikes all in the
            // ring), and the mending only inside the ring while it stands (while-spell-stands).
            SpellBranch.Add(to, E, ElderTree, "Elder Tree", SpellRole.Breaker, BranchPattern.MiddleB, FallenSeeds, SpellForms.Rod,
                new SpellSpec { Reach = 0.5f, Radius = 3f, Seconds = 15f, Interval = 3f, Damage = 16f, Stagger = 45f, Delay = 3f, Stacks = 1 },
                "A tree grows where you stand, shoot, sapling and oak, in three seconds; while it grows enemies turn on it. Grown, for fifteen seconds a ring six metres across lies under its crown. Enemies inside stay Seeded, you mend 3 a second inside, and every three seconds a root comes up under an enemy inside, a staggered one first: 16 damage and 45 stagger. 40 mana.",
                UpgradeIcons.Tower,
                new Empower { Mana = 32f, Delay = 2f },
                "Elder Tree costs 32 mana, not 40, and grows in two seconds.",
                // NEEDS: the tree pulling up its roots and following you at walking pace (form-pieces).
                new ShapeDef { Name = "Walking Tree",
                    Description = "The tree pulls up its roots and follows you at walking pace, with a ring four metres across. The stag grown through with forest.",
                    Change = new SpellChange { Radius = 2f } },
                new ShapeDef { Name = "Grown at Once", Form = SpellForms.Rod,
                    Description = "No growing. It comes up full in an instant and stands only six seconds. Setup becomes burst.",
                    Spell = new SpellSpec { Reach = 0.5f, Radius = 3f, Seconds = 6f, Interval = 3f, Damage = 16f, Stagger = 45f, Delay = 0f, Stacks = 1 } },
                "Opens two shapes of Elder Tree, chosen at any rest fire: Walking Tree, a tree that follows you, or Grown at Once, no growing and a short stand.",
                // NEEDS: planted on something you grew (at-grown), the whole tree grown at once. Until then, cast within four
                // metres of something you grew, its first root comes up at once.
                new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, While = StatWhen.NearGrown, Do = UpgradeAction.Burst, At = EffectAt.Self, Damage = 16f, Stagger = 45f, Radius = 3f, Affliction = AfflictionKind.Seeded, Stacks = 1 } },
                "Plant it on something you grew, and it grows at once.",
                tally: new TallySpec { Name = "Shade", Cap = 15 },
                learnEffects: new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Hazard, At = EffectAt.Self, Shape = HazardShape.Patch, Radius = 3f, Seconds = 18f, Interval = 1f, Damage = 1f, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Bank, Count = 15 },
                    new UpgradeEffect { On = UpgradeTrigger.EverySeconds, Every = 1f, Spends = 1, Do = UpgradeAction.Heal, Amount = 3f },
                });

            // A seed sown in everything the reaction struck: the enemy it went off on and each its blows reached
            // (UpgradeTrigger.ReactionStruck).
            to.Add(BranchPattern.Put(new Upgrade { Id = RootAndBranch, Name = "Root and Branch", Element = E, Kind = UpgradeKind.Major, Lifts = E, Requires = new[] { FallenSeeds }, Icon = UpgradeIcons.Leaf,
                Description = "Every reaction with Seeded in it mends you 4 and sows a seed in everything it reaches.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Needs = AfflictionKind.Seeded, Do = UpgradeAction.Heal, Amount = 4f },
                    new UpgradeEffect { On = UpgradeTrigger.ReactionStruck, ReactionWith = AfflictionKind.Seeded, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                } },
                BranchPattern.MiddleCrown));
            // The Garden bough's keystone (the owner's pick of 2026-11-20). The RuleBook's affliction rules ripen a seed left
            // alone six seconds (AfflictionSet: it stops fading); a ripe seed's burst is three times as strong and an unripe
            // one's half (set off, its bearer's death, a reaction), and a ripe burst is told as RipeBurst, where the thornbush
            // grows.
            to.Add(BranchPattern.Put(new Upgrade { Id = InItsSeason, Name = "In Its Season", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { RootAndBranch }, Icon = UpgradeIcons.Leaf,
                Description = "A seed left alone for six seconds, with no new seed planted on it, ripens: it flowers and stops fading. When a ripe seed bursts, by your spells, its death or a reaction, it hits three times as hard and leaves a thornbush. A seed that bursts before it is ripe hits only half as hard.",
                Rules = book => book.Afflictions.Ripen(AfflictionKind.Seeded, RipensAfter, RipeShare, UnripeShare),
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.RipeBurst, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Grown = true, Limit = 4, Radius = 2f, Seconds = 8f, Interval = 0.5f, Damage = 4f, Affliction = AfflictionKind.Seeded, Stacks = 1 } } },
                BranchPattern.TrunkLeft));
            // Kept as a mark (the owner's answer of 2026-11-20), at the top of the trunk, the one place left on this tree.
            to.Add(BranchPattern.Put(new Upgrade { Id = Shade, Name = "Shade", Element = E, Kind = UpgradeKind.Major, Requires = new[] { RootAndBranch }, Icon = UpgradeIcons.Leaf,
                Description = "While you stand near anything you have grown, you heal slowly and your stamina comes back faster.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.EverySeconds, Every = 1f, While = StatWhen.NearGrown, Do = UpgradeAction.Heal, Amount = 2f },
                    new UpgradeEffect { On = UpgradeTrigger.EverySeconds, Every = 1f, While = StatWhen.NearGrown, Do = UpgradeAction.Stamina, Amount = 5f },
                } },
                BranchPattern.TrunkTop));
            // NEEDS: whatever you grow growing back where it stood 6 s after it is spent, burst or cut down (grows-back).
            // Until then every spell that strikes sows a sapling where it lands, once in two seconds.
            to.Add(BranchPattern.Put(new Upgrade { Id = WoodGrowsBack, Name = "Wood Grows Back", Element = E, Kind = UpgradeKind.Devotion, Requires = new[] { RootAndBranch }, Icon = UpgradeIcons.Moon,
                Description = "All in on Life only. Whatever you grow grows back where it stood six seconds after it is spent, burst or cut down. Every spell you cast sows a sapling where it lands. The garden never runs out: spend it freely, and every fight becomes a forest.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Grown = true, Limit = 4, Radius = 3.5f, Seconds = 12f, Interval = 1.5f, Damage = 6f, Stagger = 8f, Affliction = AfflictionKind.Seeded, Stacks = 1, Cooldown = 2f } } },
                BranchPattern.TrunkRight));

            // ---- The left bough: Harvest. What the garden gives back.
            // NEAREST: Seeded goes three deep, not five, and a heavy blow bursts a seed of any depth.
            to.Add(BranchPattern.Put(new Upgrade { Id = Ripening, Name = "Ripening", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, Icon = UpgradeIcons.Burst,
                Description = "A heavy blow on a Seeded enemy bursts its seed at once, without waiting for death, and mends you.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.SetOff, Needs = AfflictionKind.Seeded, Affliction = AfflictionKind.Seeded, Damage = 11f, Stagger = 5f, Radius = 3f, Cooldown = 1f },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Heal, Needs = AfflictionKind.Seeded, Amount = 10f, Cooldown = 1f },
                } },
                BranchPattern.LeftFoot));

            // Red Harvest rebuilt (its id kept): the garden is the price, not health. The kneel is a still nova; every grown
            // thing bursts in thorns as it withers, and the green blood is two timed boons. NEEDS: the window as 1.5 s for each
            // grown thing withered, 10 s at most (count-from-depth), only those within 12 m (grown-bursts), and the 0.5 s kneel. Until then,
            // cast within four metres of something you grew, the window is six seconds.
            SpellBranch.Add(to, E, DrawnFromTheWood, "Drawn From the Wood", SpellRole.Breaker, BranchPattern.LeftA, Ripening, SpellForms.Nova,
                new SpellSpec { Radius = 1f, Damage = 0f, Stagger = 0f, Stacks = 0 },
                "You kneel, and every grown thing of yours within twelve metres withers into you. As each withers it bursts in thorns, 12 damage and Seeded, and gives a second and a half of green blood, ten seconds at most: every weapon blow is critical and mends a fifth of its damage. The garden you grew is the price.",
                UpgradeIcons.Heart,
                // The thorns doing 16 are the Learn's own, set by the Empower (empower-effects).
                new Empower { Cooldown = 10f, Effects = new[] { new EffectChange { Index = 2, Damage = 16f } } },
                "Drawn From the Wood waits 10 seconds, not 12, and its thorns do 16.",
                // NEEDS: the window held only while you stand in the ring (in-own-ground). Until then the ring is laid where you
                // knelt and the window goes with you.
                new ShapeDef { Name = "Blood in the Furrow",
                    Description = "The green blood soaks into the ground where you knelt, a ring eight metres across for as long as it lasts, and everything in it is Seeded each second. Fight in a place, not round yourself.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Hazard, At = EffectAt.Self, Shape = HazardShape.Patch, Radius = 4f, Seconds = 6f, Interval = 1f, Damage = 1f, Affliction = AfflictionKind.Seeded, Stacks = 1 } } },
                // NEEDS: nothing withering (shape-sets-aside), and a quarter of your present health, not a fixed 25 (health-share). Until then
                // the garden still withers, and what you grow in the next eight seconds strikes twice as hard.
                new ShapeDef { Name = "Feed the Ground", Form = SpellForms.Nova,
                    Description = "The one place Life pays in blood. Nothing withers: you give a quarter of your present health to your garden, and for eight seconds everything you have grown within twelve metres strikes twice as hard and twice as often.",
                    Spell = new SpellSpec { Radius = 1f, Damage = 0f, Stagger = 0f, Stacks = 0, HealthCost = 25f },
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Boon, Stat = PlayerStat.GrownDamage, Amount = 1f, Seconds = 8f } } },
                "Opens two shapes of Drawn From the Wood, chosen at any rest fire: Blood in the Furrow, a ring of green blood to fight in, or Feed the Ground, health given so the garden strikes harder.",
                // NEEDS: the price taken from the grey the blow left and nothing withering (spell-windows, shape-sets-aside).
                // Until then for a second after a blow lands on you Feed the Ground takes no real health.
                new[] { new UpgradeEffect { On = UpgradeTrigger.Struck, Do = UpgradeAction.Boon, Stat = PlayerStat.HealthPrices, Amount = -1f, Seconds = 1f } },
                "Cast it within a second of being struck, and the price comes from the grey health that blow left: nothing withers, each still gives its time, and Feed the Ground takes no real health.",
                learnEffects: new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, While = StatWhen.NearGrown, Do = UpgradeAction.Boon, Stat = PlayerStat.CritChance, Amount = 1f, Seconds = 6f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, While = StatWhen.NearGrown, Do = UpgradeAction.Boon, Stat = PlayerStat.Leech, Amount = 0.2f, Seconds = 6f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.BurstGrown, Damage = 12f, Stagger = 0f, Radius = 2f, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                });

            // NEEDS: never thrown, pulled or knocked down, and plain blows not making you flinch, within 4 m of something you
            // grew (unshaken). Until then wounds take a fifth less there.
            to.Add(BranchPattern.Put(new Upgrade { Id = TheGardenHoldsYou, Name = "The Garden Holds You", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Ripening }, Icon = UpgradeIcons.Shield,
                Description = "Within four metres of something you grew you cannot be thrown, pulled or knocked down, and plain blows do not make you flinch. Roots run from the nearest plant to your heels. Fight in the garden.",
                Stats = new[] { new StatMod(PlayerStat.DamageTaken, -0.2f, StatWhen.NearGrown) } },
                BranchPattern.LeftMiddle));

            // The step form carries the walk: untouchable in it, and its roots strike along the line. NEEDS: going under the
            // ground to a grown thing of yours within 12 m (at-grown), coming up 0.5 s later, and the burst of roots round you
            // as you come up, not along the way (form-pieces). Until then a step toward the enemy faced, six metres with none.
            SpellBranch.Add(to, E, RootWalk, "Root Walk", SpellRole.Movement, BranchPattern.LeftB, TheGardenHoldsYou, SpellForms.Step,
                new SpellSpec { Reach = 8f, Damage = 10f, Stagger = 15f, Stacks = 1 },
                "You sink into the ground in a swirl of leaves and come up half a second later at a grown thing of yours within twelve metres, the one nearest where you aim, or six metres ahead if there is none. Nothing can touch you under the ground. Where you went down a sapling sprouts for six seconds. As you come up roots burst round you: 10 damage, 15 stagger and Seeded within two and a half metres.",
                UpgradeIcons.Boot,
                new Empower { Cooldown = 6f, Stacks = 2 },
                "Root Walk waits 6 seconds, not 8, and its roots lay two Seeded.",
                // NEEDS: going to the nearest Seeded enemy, not the one faced (form-pieces).
                new ShapeDef { Name = "Up Through Them",
                    Description = "You go to a Seeded enemy instead, and come up under it, lifting it on roots for a moment.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = AfflictionKind.Seeded, Do = UpgradeAction.Lift, Seconds = 0.6f } } },
                // NEEDS: changing places with the grown thing (form-pieces). Until then a thornbush rises where you stood and
                // the enemies beside it lose track of you a moment, striking at it.
                new ShapeDef { Name = "Changeling",
                    Description = "You change places with the grown thing, so it stands where you stood, in the path of the blow aimed at you.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Hazard, At = EffectAt.Self, Shape = HazardShape.Patch, Grown = true, Limit = 2, Radius = 2f, Seconds = 8f, Interval = 0.5f, Damage = 4f, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Condition, At = EffectAt.Self, Radius = 2.5f, State = new ConditionSpec { Id = Changeling, Seconds = 1.5f, Flags = ConditionFlags.SightShort, Sight = 0.5f } },
                    } },
                "Opens two shapes of Root Walk, chosen at any rest fire: Up Through Them, up under a Seeded enemy, or Changeling, a grown thing left in the blow's path.",
                // NEEDS: the shared movement rule's timing (started as a blow would land, blow-about-to-land), and the full
                // thornbush in place of the sapling. Until then the cast gives what a perfect dodge gives, and a thornbush
                // grows beside the sapling.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Mana, Amount = 12f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.CutCooldowns, Amount = 1.5f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Hazard, At = EffectAt.Self, Shape = HazardShape.Patch, Grown = true, Limit = 2, Radius = 2f, Seconds = 8f, Interval = 0.5f, Damage = 4f, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                },
                "Started in the instant a blow would land on you, it counts as a perfect dodge, and the sapling left behind is a full thornbush.",
                learnEffects: new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Hazard, At = EffectAt.Self, Shape = HazardShape.Patch, Grown = true, Limit = 2, Radius = 3.5f, Seconds = 6f, Interval = 1.5f, Damage = 6f, Stagger = 8f, Affliction = AfflictionKind.Seeded, Stacks = 1 } });

            // Built: what you grow stands three times as long (GrownSeconds), three more of each may stand (GrownLimit), and
            // far from your garden blows are weaker. NEAREST: three times as long, not for ever.
            to.Add(BranchPattern.Put(new Upgrade { Id = Wildwood, Name = "Wildwood", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { TheGardenHoldsYou }, Icon = UpgradeIcons.Leaf,
                Description = "What you grow stands three times as long, and you may keep three more of each kind. Far from your garden, your blows are a fifth weaker.",
                Stats = new[] { new StatMod(PlayerStat.GrownSeconds, 2f), new StatMod(PlayerStat.GrownLimit, 3f), new StatMod(PlayerStat.WeaponDamage, -0.2f, StatWhen.FarFromGrown) } },
                BranchPattern.LeftCrown));

            // ---- The right bough: Bramble. The seed, the thorn and the guard.
            to.Add(BranchPattern.Put(new Upgrade { Id = BitterRind, Name = "Bitter Rind", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, Icon = UpgradeIcons.Shield,
                Description = "A plain guard seeds the attacker. A perfect guard roots it where it stands for a moment.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Guard, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Seeded, Stacks = 1 }, new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Hold, Seconds = 1f } } },
                BranchPattern.RightFoot));

            // Briar Patch, the base spell: cast by Life's kit, so its Learn carries no form; its shapes cast their own.
            SpellBranch.Add(to, E, BriarPatch, "Briar Patch", SpellRole.Opener, BranchPattern.RightA, Root, "", null,
                "Life's own spell, given with the element. A bed of thorned vines two and a half metres across grows three metres ahead and stands six seconds. It lashes twice a second for 4, Seeds what stands in it and slows it by a fifth. It counts as grown.",
                UpgradeIcons.Leaf,
                // NEEDS: the bed 3.3 m across (base-spell-marks). The base spell is cast by its kit, which takes only the Empower's wait.
                new Empower { Cooldown = 5.5f, Radius = 1.65f },
                "Briar Patch waits 5.5 seconds, not 7, and its bed is 3.3 metres across.",
                // Thorn Ring, once a spell, is cut; Burrs takes its shape slot. NEEDS: a seed dropped every 1.5 m the enemy moves,
                // not every 1.5 s, and the burrs themselves shaken onto its neighbours (plume-along-path). Until then a burred enemy drops a tuft
                // every second and a half for six seconds, and your blows on it seed and burr everything within two metres.
                new ShapeDef { Name = "Burrs", Form = SpellForms.Wave,
                    Description = "No bed. A spray of hooked burrs is thrown over a fan four metres ahead. Every enemy hit carries them: it is Seeded, and as it moves it drops a seed every metre and a half that sprouts a tuft of briar for four seconds, lashing for 4, Seeding and counting as grown. Your blows on a burred enemy shake burrs onto everything within two metres. The enemy plants your garden for you.",
                    Spell = new SpellSpec { Reach = 4f, Arc = 90f, Damage = 4f, Stagger = 5f, Stacks = 1 },
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Burred, Seconds = 6f, Beat = 1.5f } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Burred, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Grown = true, Limit = 6, Radius = 0.8f, Seconds = 4f, Interval = 0.5f, Damage = 4f, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Burred, By = StruckBy.Weapon, Do = UpgradeAction.Afflict, Radius = 2f, Affliction = AfflictionKind.Seeded, Stacks = 1, Cooldown = 0.5f },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Burred, By = StruckBy.Weapon, Do = UpgradeAction.Condition, Radius = 2f, State = new ConditionSpec { Id = Burred, Seconds = 6f, Beat = 1.5f }, Cooldown = 0.5f },
                    } },
                // NEEDS: the pods thrown in a fan and a bush left where each bursts, struck or not (form-pieces). Until then three
                // pods burst under the nearest enemies and leave a bush under each they strike.
                new ShapeDef { Name = "Seed Pods", Form = SpellForms.Pillars,
                    Description = "No bed. Three pods are thrown in a fan to six metres. They swell, and a second later each bursts in a spray of seeds three metres across, 12 damage and two Seeded, and leaves a small bush.",
                    Spell = new SpellSpec { Count = 3, Reach = 6f, Radius = 1.5f, Delay = 1f, Damage = 12f, Stagger = 10f, Stacks = 2 },
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Grown = true, Limit = 3, Radius = 1f, Seconds = 6f, Interval = 0.5f, Damage = 4f, Affliction = AfflictionKind.Seeded, Stacks = 1 } } },
                "Opens two shapes of Briar Patch, chosen at any rest fire: Burrs, seeds the enemy carries for you, or Seed Pods, three pods that burst and leave bushes.",
                // NEEDS: cast within 1 m of something you grew, the thicket grown round that thing (at-grown). Until then, cast
                // within four metres of something you grew, a thicket grows where the bed lands and roots what stands in it.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, While = StatWhen.NearGrown, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Grown = true, Limit = 2, Radius = 2f, Seconds = 10f, Interval = 0.5f, Damage = 4f, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, While = StatWhen.NearGrown, Do = UpgradeAction.Condition, Radius = 2f, State = new ConditionSpec { Id = Thicket, Seconds = 1f, Flags = ConditionFlags.Rooted } },
                },
                "Cast it on something you grew, within a metre of it, and the bed grows into a thicket round it: four metres across, standing ten seconds, and whatever stands in it as it grows is rooted for a second.");

            // NEAREST: nothing tells a blow by hand from a thrown one, so any attacker is cut.
            to.Add(BranchPattern.Put(new Upgrade { Id = BriarCoat, Name = "Briar Coat", Element = E, Kind = UpgradeKind.Major, Requires = new[] { BitterRind }, Icon = UpgradeIcons.Claw,
                Description = "An enemy that strikes you, or whose blow you guard, is cut by thorns and Seeded.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Struck, Do = UpgradeAction.Burst, Damage = 8f, Radius = 1.2f, Affliction = AfflictionKind.Seeded, Stacks = 1, Cooldown = 0.5f },
                    new UpgradeEffect { On = UpgradeTrigger.Guard, Do = UpgradeAction.Burst, Damage = 8f, Radius = 1.2f, Affliction = AfflictionKind.Seeded, Stacks = 1, Cooldown = 0.5f },
                } },
                BranchPattern.RightMiddle));

            // The Bramble bough's keystone (the owner's pick of 2026-11-20), as written in trees_a.md. Built by hand:
            // PlayerDefence passes a third of each wound to the Seeded enemies within 8 m, split among them, and PlayerCombat's
            // seed mending is switched off (wounds-shared, seed-mending-off; PlayerUpgrades.ShareWound).
            to.Add(BranchPattern.Put(new Upgrade { Id = SharedRoots, Name = "Shared Roots", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { BriarCoat }, Icon = UpgradeIcons.Heart,
                Description = "A third of every wound you take is taken instead by the Seeded enemies within 8 metres, split between them. In return, your seeds no longer heal you when you strike."
            }, BranchPattern.RightCrown));

            // Sapling, once a major, is the counter: a still nova opens the window as a count of three, and each perfect guard
            // or perfect dodge spends one on a sapling at the attacker (on a dodge, the enemy faced). NEEDS: the window as 8 s
            // (a count that empties with its time), three at most between the guard's and the dodge's, the Empower's fourth
            // (count-rules), and each lashing only the nearest enemy (it lashes all within its reach).
            SpellBranch.Add(to, E, Sapling, "Sapling", SpellRole.Counter, BranchPattern.RightB, BriarCoat, SpellForms.Nova,
                new SpellSpec { Radius = 1f, Damage = 0f, Stagger = 0f, Stacks = 0 },
                "For eight seconds, each perfect guard or perfect dodge plants a sapling where the attacker stands, three at most. Each stands twelve seconds and every second and a half lashes the nearest enemy within three and a half metres for 6 damage and 8 stagger, Seeding it. They count as grown.",
                UpgradeIcons.Spike,
                new Empower { Cooldown = 7f, Count = 4 },
                "Sapling waits 7 seconds, not 9, and plants four saplings.",
                // NEEDS: in place of the sapling, and only within the cast's window (shape-sets-aside). Until then, while this
                // shape is worn, every perfect guard or dodge also roots its attacker.
                new ShapeDef { Name = "Strangler",
                    Description = "Each one climbs the attacker instead, as a strangler fig climbs its host. It is rooted for a second and Seeded two deep, and nothing stands after.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Strangled, Seconds = 1f, Flags = ConditionFlags.Rooted }, Affliction = AfflictionKind.Seeded, Stacks = 2 },
                        new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Strangled, Seconds = 1f, Flags = ConditionFlags.Rooted }, Affliction = AfflictionKind.Seeded, Stacks = 2 },
                    } },
                // NEEDS: only within the cast's window, the seed in place of the sapling (shape-sets-aside).
                new ShapeDef { Name = "Dodder",
                    Description = "The seed grows on the attacker itself, pale threads of dodder that drink from it for six seconds. Each of your blows on it mends you 3 more, and its balance breaks a fifth sooner.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Dodder, Seconds = 6f, Flags = ConditionFlags.StaggerTakenShare, Scale = 1.25f } },
                        new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Dodder, Seconds = 6f, Flags = ConditionFlags.StaggerTakenShare, Scale = 1.25f } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Dodder, By = StruckBy.Weapon, Do = UpgradeAction.Heal, Amount = 3f },
                    } },
                "Opens two shapes of Sapling, chosen at any rest fire: Strangler, roots that climb the attacker, or Dodder, threads that drink from it.",
                // NEEDS: all three planted within 3 s, the third raised as the tree (count-rules). Until then a perfect guard
                // raises a small Elder Tree on the attacker for six seconds, once in fifteen.
                new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Form, Form = SpellForms.Rod, Cooldown = 15f, Spell = new SpellSpec { Cost = 0f, Reach = 0.5f, Radius = 3f, Seconds = 6f, Interval = 3f, Damage = 16f, Stagger = 45f, Delay = 0.3f, Stacks = 1 } } },
                "Plant all three within three seconds, and the third comes up as a full Elder Tree for six seconds.",
                tally: new TallySpec { Name = "Saplings", Cap = 3 },
                learnEffects: new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Bank, Count = 3 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Spends = 1, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Grown = true, Limit = 3, Radius = 3.5f, Seconds = 12f, Interval = 1.5f, Damage = 6f, Stagger = 8f, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Spends = 1, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Grown = true, Limit = 3, Radius = 3.5f, Seconds = 12f, Interval = 1.5f, Damage = 6f, Stagger = 8f, Affliction = AfflictionKind.Seeded, Stacks = 1 },
                });

            // BESPOKE (built): SpringForm. The surge button is pressed again, not held.
            to.Add(BranchPattern.Put(new Upgrade { Id = Gift, Name = "First Spring", Element = E, Kind = UpgradeKind.Signature, Icon = UpgradeIcons.Crown,
                Description = "While Regrowth lasts, press the surge button again and give half your health to the ground. A forest comes up through everything Seeded and holds it. Strike what it holds and the health returns.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.Spring, Spell = new SpellSpec { Cost = 0f, Reach = 12f, Seconds = 4f, Damage = 30f, Stagger = 60f, Count = 50, Stacks = 0 } } } },
                BranchPattern.Gift));
        }
    }
}

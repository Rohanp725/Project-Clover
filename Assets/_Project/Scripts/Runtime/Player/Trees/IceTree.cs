using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Ice: patience, written as spell branches (docs/design/steel/elements_a.md, Ice; Phase 4). Six spells of four marks
    /// each and ten passives on the pattern of <see cref="BranchPattern"/>: in the middle Shatter (Flying Shards, Eight
    /// Points and Hanging Ice, the lift Cold Settles, All at Once and Devotion above it), on the left Stillness (Cold Read,
    /// Rime Guard and Splinter Ward, Winter Wall, Sent Sliding at the crown), on the right Winter (Brittle, Hoarfrost and Ski
    /// Run). Hoarfrost is the base spell: it comes with Ice, its Learn is never bought, and it grows from the root.
    ///
    /// Marks the design keeps keep their ids; the cut ones are in <see cref="RetiredMarks"/>, and Winter Draw waits for the
    /// Katana's Way there. Brittle and All at Once are done by hand in <see cref="PlayerUpgrades"/> under their ids; the rest
    /// are data. What the data cannot say yet is the nearest working version with a <c>NEEDS</c> line, gathered in
    /// docs/design/steel/content_open.tsv.
    /// </summary>
    public static class IceTree
    {
        public const string Root = "ice.chill";
        public const string ColdRead = "ice.coldread";
        public const string FlyingShards = "ice.shards";
        public const string Brittle = "ice.brittle";
        public const string SplinterWard = "ice.splinter";
        public const string SentSliding = "ice.sliding";
        public const string AllAtOnce = "ice.allatonce";
        public const string ColdSettles = "ice.lift";
        public const string DeepWinter = "ice.devotion";
        // The spells: each is its branch's Learn mark and the branch's id.
        public const string Hoarfrost = "ice.base";
        public const string EightPoints = "ice.eight";
        public const string HangingIce = "ice.hanging";
        public const string WinterWall = "ice.wall";
        public const string RimeGuard = "ice.rime";
        public const string SkiRun = "ice.ski";
        // The god's gift.
        public const string Gift = "ice.stillworld";
        // Conditions the branches and the lift set on enemies.
        public const string FreshBreak = "ice.base.break";
        public const string ShardsClosing = "ice.eight.closing";
        public const string RimeFog = "ice.rime.fog";
        public const string ThroughTheIce = "ice.ski.through";
        public const string Settled = "ice.lift.slow";

        // Retired in Phase 4 (RetiredMarks): kept as names because code still asks for them.
        /// <summary>Kept as a mark by the owner's answer of 2026-11-20, under its old id; built by hand (PlayerUpgrades.FinishBroken).</summary>
        public const string CleanBreak = "ice.cleanbreak";
        public const string Unmoved = "ice.unmoved";
        public const string BlackIce = "ice.blackice";
        public const string ThinIce = "ice.thinice";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Ice;

            to.Add(BranchPattern.Put(new Upgrade { Id = Root, Name = "Chill", Element = E, Kind = UpgradeKind.Root, Icon = UpgradeIcons.Snowflake,
                Description = "Blows chill an enemy, up to five. Each slows it. At five it is Frozen, and a heavy blow shatters it.",
                WeaponStacks = 1 }, BranchPattern.Root));

            // ---- The middle bough: Shatter. Freeze and break.
            to.Add(BranchPattern.Put(new Upgrade { Id = FlyingShards, Name = "Flying Shards", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, Icon = UpgradeIcons.Burst,
                Description = "A shattered enemy throws its ice wide. Everyone it reaches is chilled two deep.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Shatter, Do = UpgradeAction.Burst, Damage = 14f, Stagger = 10f, Radius = 4.5f, Affliction = AfflictionKind.Chill, Stacks = 2 } } },
                BranchPattern.MiddleFoot));

            // Re-aimed as the detonator: each shard that lands spends the enemy's Chill.
            // NEEDS: a Frozen enemy shattered by the shards (a spell's blow does not shatter); until then its Chill is spent as any other's.
            SpellBranch.Add(to, E, EightPoints, "Eight Points", SpellRole.Detonator, BranchPattern.MiddleA, FlyingShards, SpellForms.Bullets,
                new SpellSpec { Reach = 6f, Count = 8, Radius = 3.5f, Delay = 1.2f, Damage = 5f, Stagger = 4f, Stacks = 1 },
                "Eight shards stand on a ring round the enemy faced for a breath, then fly in, 5 damage and a Chill each. As they close they spend its Chill, 5 more for every stack, and a Frozen enemy they shatter.",
                UpgradeIcons.Target,
                new Empower { Cooldown = 9f, Delay = 0.9f },
                "Eight Points waits 9 seconds, not 11, and its shards close sooner.",
                // NEEDS: two rows of four, one closing from in front and one from behind. Until then the ring is tighter and faster.
                new ShapeDef { Name = "Wolf's Jaws",
                    Description = "Two rows of four close on the enemy from in front and behind in half a second, too fast to walk out of. One target only.",
                    Change = new SpellChange { Delay = 0.5f, Radius = 2f } },
                // NEEDS: the shards falling from above, not rising as pillars.
                new ShapeDef { Name = "Eight on Eight", Form = SpellForms.Pillars,
                    Description = "One shard falls from above on each of up to eight enemies within eight metres, 8 damage each, each spending that enemy's Chill. For crowds.",
                    Spell = new SpellSpec { Count = 8, Reach = 8f, Radius = 1f, Damage = 8f, Stagger = 10f, Stacks = 1, Delay = 0.5f } },
                "Opens two shapes of Eight Points, chosen at any rest fire: Wolf's Jaws, too fast to walk out of, or Eight on Eight, a shard for each of a crowd.",
                // NEEDS: every shard counting as a shatter (eight shatters, Flying Shards from each). Until then a heavy blow on
                // it within 0.3 s of a shard landing strikes once more as a shatter does.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = ShardsClosing, Seconds = 0.3f, EndsWith = ConditionBlow.Heavy } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = ShardsClosing, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Damage = 45f, Stagger = 30f, Radius = 1f, Affliction = AfflictionKind.Chill, Stacks = 1 },
                },
                "Land a heavy blow on the enemy as the shards close, and every shard counts as a shatter.",
                learnEffects: new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = AfflictionKind.Chill, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Chill, Damage = 5f, Stagger = 2f, Radius = 0.8f } });

            // The sword form carries the icicle. NEEDS: the shatter of a Frozen target for 90 (it shatters as a heavy blow does, 45).
            SpellBranch.Add(to, E, HangingIce, "Hanging Ice", SpellRole.Breaker, BranchPattern.MiddleB, FlyingShards, SpellForms.Sword,
                new SpellSpec { Reach = 7f, Delay = 3f, Radius = 2f, Damage = 30f, Stagger = 60f, Stacks = 3, Seconds = 2f, LingerDamage = 2f },
                "A great icicle hangs over the enemy faced, up to seven metres away, and follows it while its shadow grows for three seconds. Then it falls: 30 damage, 60 stagger and three Chill on everything within two metres. On a Frozen enemy it shatters it for 90.",
                UpgradeIcons.Comet,
                new Empower { Cooldown = 8f, Delay = 2.4f },
                "Hanging Ice waits 8 seconds, not 10, and falls sooner.",
                new ShapeDef { Name = "Icicles", Form = SpellForms.Blades,
                    Description = "Three smaller icicles fall a second apart along where its enemy moves, each 12 damage and 25 stagger. It punishes running.",
                    Spell = new SpellSpec { Count = 3, Delay = 1f, Radius = 1.5f, Damage = 12f, Stagger = 25f, Stacks = 1, Reach = 7f } },
                // NEEDS: only a heavy blow bringing it down, and the icicle seen hanging. Until then it is a sign on the enemy that
                // the next weapon blow on it brings down.
                new ShapeDef { Name = "Held by a Thread", Form = SpellForms.Brand,
                    Description = "It does not fall by itself. It hangs up to eight seconds and falls with your next heavy blow on its enemy. Weave it into the heavy.",
                    Spell = new SpellSpec { Reach = 7f, Seconds = 8f, Count = 1, Radius = 2f, Damage = 30f, Stagger = 60f, Stacks = 3 } },
                "Opens two shapes of Hanging Ice, chosen at any rest fire: Icicles, that punish running, or Held by a Thread, that falls with your next heavy blow.",
                // NEEDS: landing within 0.3 s of the enemy freezing (freeze-moment), and the shatter four times over. Until then
                // a fall that freezes what it strikes brings the cooldown back.
                new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = AfflictionKind.Chill, NeedsFull = true, Do = UpgradeAction.ResetCooldown } },
                "Let it land in the moment its enemy freezes, and it shatters it four times over and its cooldown comes back.");

            // NEEDS: every reaction with Chill in it slowing all it struck (a reaction's struck set); until then the enemy it happened on.
            to.Add(BranchPattern.Put(new Upgrade { Id = ColdSettles, Name = "Cold Settles", Element = E, Kind = UpgradeKind.Major, Lifts = E, Requires = new[] { FlyingShards }, Icon = UpgradeIcons.Snowflake,
                Description = "Every reaction with Chill in it leaves frozen ground where it happened for four seconds, and what it struck is slower for two.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Needs = AfflictionKind.Chill, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 1.5f, Seconds = 4f, Interval = 1f, Damage = 1f, Affliction = AfflictionKind.Chill, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Needs = AfflictionKind.Chill, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Settled, Seconds = 2f, Pace = 0.6f } },
                } },
                BranchPattern.MiddleCrown));
            // Built: the shatter itself is tripled (ShatterDamage) and Chill slows no one (Afflictions.ChillSlowOff); it still freezes at five.
            to.Add(BranchPattern.Put(new Upgrade { Id = AllAtOnce, Name = "All at Once", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { ColdSettles }, Icon = UpgradeIcons.Star,
                Description = "Shattering strikes three times as hard. Chill no longer slows anyone.",
                Stats = new[] { new StatMod(PlayerStat.ShatterDamage, 2f) } },
                BranchPattern.TrunkLeft));
            // The RuleBook's affliction rules (task 4.4), read by every enemy's set: Chill never fades by itself (a freeze still
            // spends it as it thaws, and a shatter). Lighting Glaze lays full Chill within 4 m, which freezes.
            to.Add(BranchPattern.Put(new Upgrade { Id = DeepWinter, Name = "Deep Winter", Element = E, Kind = UpgradeKind.Devotion, Requires = new[] { ColdSettles }, Icon = UpgradeIcons.Moon,
                Description = "All in on Ice only. Chill never fades, and lighting Glaze freezes every enemy within four metres at once. No Chill is ever wasted: spread it over the whole room and come back for each in turn.",
                Rules = book => book.Afflictions.Keep(AfflictionKind.Chill),
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Afflict, At = EffectAt.Self, Radius = 4f, Affliction = AfflictionKind.Chill, Stacks = 5 } } },
                BranchPattern.TrunkRight));

            // ---- The left bough: Stillness. The guard and the wait.
            to.Add(BranchPattern.Put(new Upgrade { Id = ColdRead, Name = "Cold Read", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard chills the one who struck, two deep.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 2 } } },
                BranchPattern.LeftFoot));

            // NEEDS: the fog as the window (6 s) and only blows struck at you inside it, the rime on the attacker's weapon arm,
            // and only the attacker's next wind-up slowed. Until then a cast holds three answers, spent by the next blows that
            // come at you, and every enemy standing in the fog winds up half again as slowly.
            SpellBranch.Add(to, E, RimeGuard, "Rime Guard", SpellRole.Counter, BranchPattern.LeftA, ColdRead, SpellForms.Field,
                new SpellSpec { Reach = 0.5f, Radius = 3f, Seconds = 6f, Interval = 1f, Damage = 0f, Stagger = 0f, LingerDamage = 0f, Stacks = 0 },
                "You breathe out a low freezing fog round you, three metres, for six seconds. An enemy that strikes at you in it is chilled two deep and winds up its next attack half again as slowly, and a perfect guard in it freezes the attacker at once.",
                UpgradeIcons.Shield,
                new Empower { Cooldown = 7f, Radius = 4f },
                "Rime Guard waits 7 seconds, not 9, and its fog reaches four metres.",
                // NEEDS: the Empower's wider fog not reaching past this shape's own (an Empower sets its numbers after the shape's).
                new ShapeDef { Name = "Freezing Fog",
                    Description = "The fog stays where it was breathed, five metres wide, and everything in it gains a Chill a second. Hold a door.",
                    Change = new SpellChange { Radius = 2.5f, Stacks = 1 } },
                // NEEDS: the rime on the guard for 6 s after the cast. Until then, while this shape is worn, every guard chills and
                // every perfect guard freezes, cast or not.
                new ShapeDef { Name = "Breath on the Steel", Form = SpellForms.Nova,
                    Description = "No fog. The rime goes on your guard for six seconds: every blow guarded lays two Chill on the attacker, and every perfect guard freezes it.",
                    Spell = new SpellSpec { Radius = 1f, Damage = 0f, Stagger = 0f, Stacks = 0 },
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.Guard, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 2 },
                        new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 5 },
                    } },
                "Opens two shapes of Rime Guard, chosen at any rest fire: Freezing Fog, a fog left to hold a door, or Breath on the Steel, rime on your guard.",
                // NEEDS: only in the fog's first second (spell-windows) and only what stands in the fog. Until then a perfect guard
                // freezes everything within three metres of you, once in nine seconds.
                new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, At = EffectAt.Self, Radius = 3f, Affliction = AfflictionKind.Chill, Stacks = 5, Cooldown = 9f } },
                "A perfect guard in the fog's first second freezes everything in the fog, not only the attacker.",
                tally: new TallySpec { Name = "Rime", Cap = 3 },
                learnEffects: new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Bank, Count = 3 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = RimeFog, Seconds = 1.2f, Flags = ConditionFlags.WindupSlowed, Scale = 0.67f } },
                    new UpgradeEffect { On = UpgradeTrigger.Struck, Spends = 1, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 2 },
                    new UpgradeEffect { On = UpgradeTrigger.Guard, Spends = 1, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 2 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Spends = 1, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 5 },
                });

            // A perfect guard's mana and its cut to every cooldown; the turned blow does not yet count as a perfect guard for other marks.
            to.Add(BranchPattern.Put(new Upgrade { Id = SplinterWard, Name = "Splinter Ward", Element = E, Kind = UpgradeKind.Major, Requires = new[] { ColdRead }, Icon = UpgradeIcons.Shield,
                Description = "When Glaze breaks, it bursts round you and gives what a perfect guard gives: its mana, and two seconds off every cooldown.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.WardSpent, Do = UpgradeAction.Burst, At = EffectAt.Self, Damage = 12f, Stagger = 30f, Radius = 3.5f, Affliction = AfflictionKind.Chill, Stacks = 3 },
                    new UpgradeEffect { On = UpgradeTrigger.WardSpent, Do = UpgradeAction.Mana, Amount = 15f },
                    new UpgradeEffect { On = UpgradeTrigger.WardSpent, Do = UpgradeAction.CutCooldowns, Amount = 2f },
                } },
                BranchPattern.LeftMiddle));

            // NEEDS: the wall's 60 health and its touch chilling, and the burst thrown forward as a cone of 6 m. As built it
            // chills only when it bursts, round itself.
            SpellBranch.Add(to, E, WinterWall, "Winter Wall", SpellRole.Breaker, BranchPattern.LeftB, SplinterWard, SpellForms.Wall,
                new SpellSpec { Reach = 2f, Arc = 5f, Delay = 0.2f, Seconds = 6f, Radius = 4f, Damage = 30f, Stagger = 50f, Stacks = 3 },
                "A wall of ice five metres wide rises two metres ahead for six seconds. It stops blows, and what touches it is chilled. Your heavy blow bursts it forward as a cone: 30 damage, 50 stagger and three Chill. 35 mana.",
                UpgradeIcons.Wall,
                new Empower { Mana = 30f, Damage = 36f },
                "Winter Wall costs 30 mana, not 35, and its burst strikes harder.",
                // NEEDS: the wall walking ahead of you at walking pace.
                new ShapeDef { Name = "Shield Wall",
                    Description = "A shorter wall, three metres, that moves two metres ahead of you at walking pace for four seconds. Advance behind it.",
                    Change = new SpellChange { Arc = 3f, Seconds = 4f } },
                // NEEDS: the wall bursting 1.5 s after an enemy first strikes it (a wall struck, standing-things). Until then it
                // bursts by itself three seconds after it rises.
                new ShapeDef { Name = "Cracked Wall",
                    Description = "It bursts by itself a second and a half after an enemy first strikes it. A trap that answers the enemy, not you.",
                    Change = new SpellChange { Seconds = 3f } },
                "Opens two shapes of Winter Wall, chosen at any rest fire: Shield Wall, a wall you advance behind, or Cracked Wall, a trap that answers the enemy.",
                // NEEDS: only a heavy blow's burst within 0.3 s of an enemy striking the wall (their-blow-moment). Until then
                // every burst freezes what it reaches.
                new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 5 } },
                "Burst it with a heavy blow in the instant after an enemy strikes it, and the cone freezes everything it reaches.");

            // NEEDS: Sent Sliding as designed: the heavy blow no longer shattering a Frozen enemy where it stands, the body
            // sliding 10 m along the blow as a block, throwing aside what it meets and shattering at its end or on a wall
            // (thrown-into). Until then every shatter runs a lane of ice ten metres on along the blow.
            to.Add(BranchPattern.Put(new Upgrade { Id = SentSliding, Name = "Sent Sliding", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { SplinterWard }, Icon = UpgradeIcons.Hammer,
                Description = "Your heavy blow no longer shatters a Frozen enemy where it stands. It sends it sliding ten metres along the blow as a block of ice, striking all it meets for 30 and two Chill, and it shatters at the end or on a wall. Freeze one, then bowl it into the rest.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Shatter, Do = UpgradeAction.Lane, Reach = 10f, Radius = 1f, Damage = 30f, Stagger = 30f, Affliction = AfflictionKind.Chill, Stacks = 2, Cooldown = 0.5f } } },
                BranchPattern.LeftCrown));

            // ---- The right bough: Winter. Ice on the ground.
            // Built (AfflictionSet.BrittleLeft): for five seconds after a freeze ends the enemy takes a fifth more from the weapon and to its balance.
            to.Add(BranchPattern.Put(new Upgrade { Id = Brittle, Name = "Brittle", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, Icon = UpgradeIcons.Spike,
                Description = "An enemy that thaws or is shattered stays brittle for a while. Blows on it cut deeper and break its balance sooner.",
                Stats = new StatMod[0] },
                BranchPattern.RightFoot));

            // Hoarfrost, the base spell: cast by Ice's kit, so its Learn carries no form; its shapes cast their own.
            SpellBranch.Add(to, E, Hoarfrost, "Hoarfrost", SpellRole.Opener, BranchPattern.RightA, Root, "", null,
                "Ice's own spell, given with the element. Thirteen crystals grow out over a fan four and a half metres ahead: 10 damage, 15 stagger and Chill two deep. They stand four seconds, and a weapon blow among them shatters them all for 5 damage and one more Chill.",
                UpgradeIcons.Snowflake,
                // NEEDS: Chill three deep. The base spell is cast by its kit, which takes only the Empower's wait.
                new Empower { Cooldown = 5.5f, Stacks = 3 },
                "Hoarfrost waits 5.5 seconds, not 7, and chills three deep.",
                // Black Ice, once a major, folded in. NEEDS: enemies on the sheet thrown half again as far, and a dodge on it
                // sliding twice as far.
                new ShapeDef { Name = "Black Ice", Form = SpellForms.Field,
                    Description = "No crystals. A sheet of clear ice covers the fan's ground for five seconds. Enemies on it gain a Chill a second and are thrown half again as far, and a dodge on it slides twice as far.",
                    Spell = new SpellSpec { Reach = 2.5f, Radius = 2.3f, Seconds = 5f, Interval = 1f, Damage = 0f, Stagger = 0f, LingerDamage = 1f, Stacks = 1 } },
                new ShapeDef { Name = "Frost Line", Form = SpellForms.Spikes,
                    Description = "One branching line of frost creeps ten metres along the floor in a moment, laying two Chill on all it crosses. It reaches the archer behind the shield.",
                    Spell = new SpellSpec { Reach = 10f, Speed = 16f, Count = 1, Radius = 0.7f, Damage = 8f, Stagger = 10f, Stacks = 2 } },
                "Opens two shapes of Hoarfrost, chosen at any rest fire: Black Ice, a sheet of ice on the ground, or Frost Line, one long creeping line.",
                // NEEDS: only Hoarfrost cast within 0.5 s after a shatter, its crystals grown from the shattered spot
                // (spell-windows). Until then, for a moment after a shatter, the next blow or spell on each enemy near it lays
                // two more Chill.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Shatter, Do = UpgradeAction.Condition, Radius = 6f, State = new ConditionSpec { Id = FreshBreak, Seconds = 0.8f, EndsWith = ConditionBlow.Any } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = FreshBreak, By = StruckBy.Any, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 2 },
                },
                "Cast it within half a second after a shatter, and the crystals grow out from the shattered spot, Chill four deep.");

            // The drill carries the slide and leaves the lane; nothing can touch you for the whole slide, where the design asks
            // for its first 0.2 s. NEEDS: a dodge along the lane carrying twice as far.
            // Kept as a mark (the owner's answer of 2026-11-20), in the right bough's empty middle place, from Brittle. Built by
            // hand (PlayerUpgrades.OnShattered and FinishBroken): a shattered enemy under a fifth of its health dies at once;
            // the gods are spared, elites are not.
            to.Add(BranchPattern.Put(new Upgrade { Id = CleanBreak, Name = "Clean Break", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Brittle }, Icon = UpgradeIcons.Skull,
                Description = "Shatter an enemy that has less than a fifth of its health left, and nothing is left of it. Gods are spared; elites are not." },
                BranchPattern.RightMiddle));

            // Ski Run grows from Brittle, or from Clean Break beside it, so keeping Clean Break changes no price.
            var grown = SpellBranch.Add(to, E, SkiRun, "Ski Run", SpellRole.Movement, BranchPattern.RightB, Brittle, SpellForms.Drill,
                new SpellSpec { Reach = 9f, Radius = 1f, Count = 1, Damage = 6f, Stagger = 10f, Stacks = 2, Speed = 18f, Seconds = 5f, LingerDamage = 1f },
                "A lane of clear ice runs nine metres ahead and you slide down it at once, throwing snow, and nothing can touch you as you set off. Enemies on the lane are chilled two deep. The lane stays five seconds, and a dodge along it carries twice as far.",
                UpgradeIcons.Boot,
                new Empower { Cooldown = 6f, Reach = 12f },
                "Ski Run waits 6 seconds, not 8, and its lane is twelve metres.",
                // NEEDS: an enemy stepping onto the lane later cracking through (ground-states), and a god slowed by 30% instead
                // of held. Until then each enemy the slide passes is held, three deep, and a heavy blow on it then freezes it.
                new ShapeDef { Name = "Thin Ice",
                    Description = "The lane is lake ice over black water. An enemy that steps onto it within five seconds cracks through to the knees: held for a moment and three deep in Chill. A heavy blow on it then freezes the water round it, and it is Frozen.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Hold, Seconds = 1.2f },
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 1 },
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = ThroughTheIce, Seconds = 1.2f, EndsWith = ConditionBlow.Heavy } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = ThroughTheIce, By = StruckBy.Heavy, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Chill, Stacks = 5 },
                    } },
                // NEEDS: the shards loosed through the slide, not all as it begins.
                new ShapeDef { Name = "Shooting Run",
                    Description = "While you slide you loose three shards of ice at the nearest enemy, 8 damage and a Chill each, as she shoots from her skis.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Projectile, Flight = Flight.Homing, Count = 3, Damage = 8f, Stagger = 6f, Radius = 0.5f, Speed = 18f, Seconds = 0.8f, Affliction = AfflictionKind.Chill, Stacks = 1 } } },
                "Opens two shapes of Ski Run, chosen at any rest fire: Thin Ice, a lane that gives under those who follow, or Shooting Run, shards loosed as you slide.",
                // NEEDS: the shared movement rule's timing (started as a blow would land, their-blow-moment), and the moment of a
                // shatter. Until then the cast gives what a perfect dodge gives, and sliding through a Frozen enemy freezes
                // everything on the lane on from it.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Mana, Amount = 12f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.CutCooldowns, Amount = 1.5f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = AfflictionKind.Chill, NeedsFull = true, Do = UpgradeAction.Lane, Reach = 9f, Radius = 1f, Damage = 0f, Stagger = 0f, Affliction = AfflictionKind.Chill, Stacks = 5, Cooldown = 1f },
                },
                "Started in the instant a blow would land on you, it counts as a perfect dodge. Slide through an enemy in the moment it shatters, and the lane freezes everything standing on it.");

            grown.Requires = new[] { Brittle, CleanBreak };

            // BESPOKE (built): StillWorldForm and ShatterChainForm.
            to.Add(BranchPattern.Put(new Upgrade { Id = Gift, Name = "The World Holds Still", Element = E, Kind = UpgradeKind.Signature, Icon = UpgradeIcons.Crown,
                Description = "With Glaze standing, press the surge button again to break it outward. Everything it crosses is Frozen where it stands. Shatter one and its ice shatters the next.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.StillWorld, Spell = new SpellSpec { Cost = 0f, Damage = 10f, Stagger = 30f, Radius = 9f } }, new UpgradeEffect { On = UpgradeTrigger.Shatter, Do = UpgradeAction.Form, Form = SpellForms.ShatterChain, Cooldown = 0.1f, Spell = new SpellSpec { Cost = 0f, Damage = 20f, Stagger = 30f, Reach = 7f } } } },
                BranchPattern.Gift));
        }
    }
}

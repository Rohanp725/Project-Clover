using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Light: mark the enemy, then make the few blows that matter count, written as spell branches
    /// (docs/design/steel/elements_b.md, Light; Phase 4). Six spells of four marks each and nine passives on the pattern of
    /// <see cref="BranchPattern"/>: in the middle Sentence (Weighed, Reckoning and Ring of Spears, Condemned at the crown),
    /// on the left Sight (Just Cut, Step Into Light and Standing Light, Unblemished at the crown, grown from the lamp), on
    /// the right Radiance (Brimming, Glare, the lift Seen Plainly, Daybreak, and the Devotion Full Daylight at the crown).
    /// Glare is the base spell: it comes with Light, its Learn is never bought, and it grows from the root.
    ///
    /// Marks the design keeps keep their ids; the cut ones are in <see cref="RetiredMarks"/> (Sentence and What It Owes
    /// live on in Reckoning's Spoken Sentence, Dazzle in Glare's Master; Two Suns is a keystone not carried over). The
    /// marks are data. What the data cannot say yet is the nearest working version with a <c>NEEDS</c> line, gathered in
    /// docs/design/steel/content_open.tsv.
    /// </summary>
    public static class LightTree
    {
        public const string Root = "light.mark";
        public const string JustCut = "light.justcut";
        public const string Weighed = "light.weighed";
        public const string Brimming = "light.brimming";
        public const string Unblemished = "light.unblemished";
        public const string Condemned = "light.condemned";
        public const string SeenPlainly = "light.lift";
        public const string FullDaylight = "light.devotion";
        /// <summary>Full Daylight: seconds after a mark fades before another can be laid on that enemy.</summary>
        public const float DaylightRest = 4f;
        // The spells: each is its branch's Learn mark and the branch's id.
        public const string Glare = "light.base";
        public const string StepIntoLight = "light.step";
        public const string RingOfSpears = "light.spears";
        public const string TwoSuns = "light.twosuns";
        public const string StandingLight = "light.lantern";
        public const string Reckoning = "light.reckoning";
        public const string Daybreak = "light.daybreak";
        // The god's gift.
        public const string TheScales = "light.scales";
        // Conditions the branches set on enemies.
        public const string Hanging = "light.spears.hanging";
        public const string InTurn = "light.spears.inturn";
        public const string Sealed = "light.reckoning.seal";
        public const string Witnessed = "light.reckoning.witnessed";
        public const string Spoken = "light.reckoning.spoken";
        public const string Sentenced = "light.reckoning.sentenced";
        public const string Owed = "light.reckoning.owed";

        // Retired in Phase 4 (RetiredMarks): kept as a name because code still asks for it.
        public const string ReadyLight = "light.readylight";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Light;
            const AfflictionKind M = AfflictionKind.Marked;

            // The design marks with the last blow of a chain only; a root lays its affliction with every blow, as the schema asks.
            to.Add(BranchPattern.Put(new Upgrade { Id = Root, Name = "Marked", Element = E, Kind = UpgradeKind.Root, Icon = UpgradeIcons.Sun,
                Description = "The last blow of a chain and every perfect guard leave a mark of light. The next three blows on a Marked enemy are critical and give more mana.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.ChainEnd, Do = UpgradeAction.Afflict, Affliction = M, Stacks = 1 }, new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, Affliction = M, Stacks = 1 } } },
                BranchPattern.Root));

            // ---- The middle bough: Sentence. Marks laid deep, and judgement passed on them.
            // NEAREST: marks go three deep, as designed. Critical damage by the depth of the mark is a boon when a mark fills.
            to.Add(BranchPattern.Put(new Upgrade { Id = Weighed, Name = "Weighed", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, Icon = UpgradeIcons.Target,
                Description = "Marks deepen, up to three. When a mark reaches three, your critical blows strike 30% harder for six seconds.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictionFull, Do = UpgradeAction.Boon, Stat = PlayerStat.CritDamage, Amount = 0.3f, Seconds = 6f } }, Stats = new[] { new StatMod(PlayerStat.AfflictionDepth, 2f) } },
                BranchPattern.MiddleFoot));

            // The detonator: the brand form sets the seal on the enemy faced and flashes, striking nothing, at each blow it
            // counts; the clock is a condition whose end carries the weapon blows it took: 8 for each weapon blow landed on it,
            // as the owner decided (docs/design/steel/phase4_questions.md, the owner's answers: "Reckoning: 8 per blow").
            SpellBranch.Add(to, E, Reckoning, "Reckoning", SpellRole.Detonator, BranchPattern.MiddleA, Weighed, SpellForms.Brand,
                new SpellSpec { Reach = 10f, Seconds = 6f, Count = 12, Radius = 0.6f, Damage = 0f, Stagger = 0f, Stacks = 0 },
                "You set a seal on the enemy you face: a ring like a clock face on the ground under it, one hand sweeping it in six seconds. On the last tick it breaks upward in a column of light: 8 damage for each weapon blow you landed on that enemy in the six seconds, with 40 stagger.",
                UpgradeIcons.Hourglass,
                // The roster's numbers alone (the owner's answer of 2026-11-20). The count is the approved 8 for each weapon blow
                // (the owner's "Reckoning: 8 per blow"); 60% for 50% is 10 a blow, and 180 at most is eighteen blows' worth, set
                // on the Learn's own burst (empower-effects).
                new Empower { Effects = new[] { new EffectChange { Index = 1, Damage = 10f, PerMost = 18 } } },
                "Reckoning pays back 10 for each blow, not 8, up to 180.",
                // NEEDS: the bearer's blows on you and your guard counted, 15 more for each turned with a perfect guard
                // (seal-counts-bearer), and the Learn's own count set aside (shape-sets-aside). Until then a column falls on
                // the sealed enemy for one blow's worth as the seal ends, beside the Learn's count.
                new ShapeDef { Name = "Witnessed",
                    Description = "The seal counts the bearer's deeds, not yours. For six seconds every blow it lands on you or your guard is scratched in gold on the clock's rim. On the last tick a column falls on it: 30 for each blow written and 15 more for each you turned with a perfect guard, 150 at most, and 40 stagger. Guard well and the sun judges for you.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Witnessed, Seconds = 6f } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Witnessed, Do = UpgradeAction.Falling, Radius = 1.2f, Delay = 0.3f, Damage = 30f, Stagger = 40f, Affliction = M, Stacks = 1 },
                    } },
                // Sentence and What It Owes, once majors, folded in here. NEEDS: no clock (shape-sets-aside), and 12 more for each
                // depth of mark (count-from-depth). Until then the Learn's clock still pays, and the heavy blow sets a short
                // sentence on every Marked enemy within eight metres whose end drops a blade for 32 and spends the marks.
                new ShapeDef { Name = "Spoken Sentence",
                    Description = "No clock. The seal waits up to eight seconds for your next heavy blow on its bearer, then a blade of beaten gold falls on every Marked enemy within eight metres: 20 damage and 12 more for each depth of mark, 40 stagger, and the marks are spent. Many enemies, at the moment you choose.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Spoken, Seconds = 8f, EndsWith = ConditionBlow.Heavy } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Spoken, By = StruckBy.Heavy, Do = UpgradeAction.Condition, Radius = 8f, LeapsTo = M, State = new ConditionSpec { Id = Sentenced, Seconds = 0.4f } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Sentenced, Do = UpgradeAction.Falling, Radius = 1.2f, Delay = 0.2f, Damage = 32f, Stagger = 40f, Stacks = 0 },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Sentenced, Do = UpgradeAction.Spend, Affliction = M },
                    } },
                "Opens two shapes of Reckoning, chosen at any rest fire: Witnessed, a seal that counts the blows struck at you, or Spoken Sentence, a seal your heavy blow breaks over every Marked enemy.",
                // A finisher on the sealed enemy (StruckBy.Finisher), not any heavy blow. NEEDS: the count paid a second time
                // (damage-tally); until then the finisher strikes it for 40 and 40 stagger, once a seal.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Owed, Seconds = 6f, EndsWith = ConditionBlow.Finisher } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Owed, By = StruckBy.Finisher, Do = UpgradeAction.Burst, Radius = 1.2f, Damage = 40f, Stagger = 40f, Stacks = 0 },
                },
                "A finisher on the sealed enemy pays the count twice.",
                learnEffects: new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Sealed, Seconds = 6f } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Sealed, Do = UpgradeAction.Burst, PerStack = true, Unseen = true, Radius = 0.6f, Damage = 8f, Stagger = 0f, Stacks = 0 },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Sealed, Do = UpgradeAction.Burst, Radius = 1.2f, Damage = 0f, Stagger = 40f, Stacks = 0 },
                });

            // The breaker: the bullets form closes six spears on the enemy faced, and what they strike is held.
            SpellBranch.Add(to, E, RingOfSpears, "Ring of Spears", SpellRole.Breaker, BranchPattern.MiddleB, Weighed, SpellForms.Bullets,
                new SpellSpec { Count = 6, Radius = 3f, Delay = 0.6f, Damage = 6f, Stagger = 5f, Stacks = 1, Reach = 10f },
                "Six spears of beaten gold stand in the air in a ring three metres round the enemy you face, wait a breath, and drive in together: 6 damage each, 30 stagger in all, and Marked. It is held still a second and a half, less for a god or an elite.",
                UpgradeIcons.Ring,
                // The roster's numbers alone (the owner's answer of 2026-11-20: an Empower may strengthen without a shorter wait).
                new Empower { Stagger = 6.7f },
                "Its spears break 40 stagger in all.",
                // NEEDS: a line six metres across your front that holds what crosses it and makes your blows across it critical,
                // the spears not driving into the enemy (ground-states, form-pieces, shape-sets-aside). Until then the spears
                // still drive in, and stand five seconds in the floor in their ring round it, Marking and jarring what is inside.
                new ShapeDef { Name = "The Drawn Line",
                    Description = "The six spears drive into the floor in a straight line six metres across your front and stand five seconds, dust lit along them. An enemy that crosses it is held a second and Marked, and your blows across the line are critical. Fight across it.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Hazard, Shape = HazardShape.Ring, Reach = 3f, Count = 6, Radius = 0.7f, Seconds = 5f, Interval = 1f, Damage = 0f, Stagger = 12f, Affliction = M, Stacks = 1, Cooldown = 1f } } },
                // NEEDS: a quarter second between spears (form-times), and a spear landing within 0.1 s of a weapon blow as the
                // moment (spell-windows). Until then the blades form drops them one after another, and each weapon blow on the
                // enemy for a second and a half after strikes a spear's worth more.
                new ShapeDef { Name = "In Turn", Form = SpellForms.Blades,
                    Description = "The spears drive in one after another, a quarter second apart. A spear that lands within a tenth of a second of one of your weapon blows strikes double and is critical. Six beats to meet.",
                    Spell = new SpellSpec { Count = 6, Delay = 0.6f, Radius = 1f, Damage = 6f, Stagger = 5f, Stacks = 1, Reach = 10f },
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = InTurn, Seconds = 1.5f } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = InTurn, By = StruckBy.Weapon, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 6f, Stagger = 5f, Stacks = 0, Cooldown = 0.25f },
                    } },
                "Opens two shapes of Ring of Spears, chosen at any rest fire: The Drawn Line, a row of spears to fight across, or In Turn, spears that fall one by one to the beat of your blows.",
                // NEEDS: a critical blow as the one answered, and the spears driven at once (form-pieces). Until then the first
                // weapon blow on the enemy faced while they hang strikes it for half again their worth and ends the wait.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Hanging, Seconds = 0.6f, EndsWith = ConditionBlow.Weapon } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Hanging, By = StruckBy.Weapon, Do = UpgradeAction.Burst, Radius = 1.5f, Damage = 18f, Stagger = 15f, Affliction = M, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Hanging, By = StruckBy.Weapon, Do = UpgradeAction.ResetCooldown },
                },
                "Land a critical blow on the enemy while the spears hang, and they all drive in at once, critical, and Ring of Spears is ready again.",
                learnEffects: new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Hold, Seconds = 1.5f } });

            // BESPOKE: an enemy marked three deep and under a quarter of its health (a tenth for a god or elite) can be finished as if staggered.
            // Until then: a mark reaching full depth breaks much of the enemy's balance, and the weapon deals less to enemies that bear nothing.
            to.Add(BranchPattern.Put(new Upgrade { Id = Condemned, Name = "Condemned", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { Weighed }, Icon = UpgradeIcons.Skull,
                Description = "When an affliction on an enemy reaches its full depth, its balance takes a great blow. Your weapon deals less to enemies that bear nothing.",
                Stats = new[]
                {
                    new StatMod(PlayerStat.WeaponDamage, -0.15f),
                    new StatMod(PlayerStat.DamageToAfflicted, 0.15f),
                },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictionFull, Do = UpgradeAction.Burst, Radius = 1.2f, Damage = 0f, Stagger = 60f, Cooldown = 4f } } },
                BranchPattern.MiddleCrown));

            // The third keystone, back under its id (the owner's word of 2026-11-20: trees may keep three). It stands at the
            // top of the trunk, grown from Ring of Spears' Master, the spell that comes at the enemy from every side. Its old
            // price, spells costing a quarter more, means nothing now that spells wait on clocks: they wait a fifth longer
            // instead (phase4_questions.md, Light 1).
            // NEEDS: a figure of light that casts the spell again from the far side of the enemy faced, at a share of its
            // damage (two-suns). Until then: each enemy a spell strikes is struck again by light a moment later, and Marked.
            to.Add(BranchPattern.Put(new Upgrade { Id = TwoSuns, Name = "Two Suns", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { SpellBranch.MasterId(RingOfSpears) }, Icon = UpgradeIcons.Sun,
                Description = "Each enemy your spell strikes is struck again by a burst of light a moment later, and Marked. Your spells wait a fifth longer.",
                Stats = new[] { new StatMod(PlayerStat.SpellCooldown, -1f / 6f) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, Do = UpgradeAction.Burst, Radius = 2.5f, Delay = 0.4f, Damage = 12f, Stagger = 10f, Affliction = M, Stacks = 1, Cooldown = 0.2f } } },
                BranchPattern.TrunkRight));

            // ---- The left bough: Sight. The exact moment, and the fight without a wound.
            // BESPOKE: the next attack of a chain pressed within 0.08 s of the moment the current blow hits is critical and free of stamina; a white glint runs the blade.
            // Until then: one blow in four makes the next one flash, and gives a little stamina back.
            to.Add(BranchPattern.Put(new Upgrade { Id = JustCut, Name = "Just Cut", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, Icon = UpgradeIcons.Blade,
                Description = "Each blow that lands may flash, one time in four: for a moment after, your blows are critical. A blow may also give a little stamina back.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Chance = 0.25f, Do = UpgradeAction.Boon, Stat = PlayerStat.CritChance, Amount = 1f, Seconds = 0.8f, Cooldown = 1.5f },
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Chance = 0.25f, Do = UpgradeAction.Stamina, Amount = 6f, Cooldown = 1.5f },
                } },
                BranchPattern.LeftFoot));

            // The movement spell: the step form carries you through the enemy faced, cuts and marks what is on the line, and
            // makes the next heavy blow critical.
            SpellBranch.Add(to, E, StepIntoLight, "Step Into Light", SpellRole.Movement, BranchPattern.LeftA, JustCut, SpellForms.Step,
                new SpellSpec { Reach = 7f, Damage = 10f, Stagger = 10f, Stacks = 1, Seconds = 1.5f },
                "You step through the enemy you face, up to seven metres, on a line of gold dust, cutting and Marking it. If the cut lands, your next heavy blow within a second and a half is critical.",
                UpgradeIcons.Wing,
                new Empower { Cooldown = 7f, Reach = 9f },
                "Step Into Light waits 7 seconds, not 8, and carries you nine metres.",
                // NEEDS: the step back along the road as a second press within 3 s (second-press). The road that marks is data.
                new ShapeDef { Name = "Aruna's Road",
                    Description = "The line you stepped stays three seconds as a road of gold dust. It Marks whatever crosses it, and pressing again within the three seconds steps you back along it to where you began. Out and back.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Hazard, At = EffectAt.Self, Shape = HazardShape.Line, Reach = 7f, Count = 5, Radius = 0.7f, Seconds = 3f, Interval = 1f, Damage = 0f, Stagger = 0f, Affliction = M, Stacks = 1 } } },
                // The step already comes out at the enemy's back, turned toward it; the next blow is three times as hard.
                new ShapeDef { Name = "Behind the Eye",
                    Description = "You step to behind the enemy rather than through, landing at its back. Your first blow within a second deals a finisher's damage, three times as much, without ending any stagger.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Boon, Stat = PlayerStat.WeaponDamage, Amount = 2f, Seconds = 1f, UntilBlow = true } } },
                "Opens two shapes of Step Into Light, chosen at any rest fire: Aruna's Road, a road of dust that Marks and leads you home, or Behind the Eye, a step to the enemy's back for one great blow.",
                // NEEDS: two enemies struck by one cast as the moment (struck-count). Until then a step that cuts anything ends its
                // own wait, once in sixteen seconds.
                new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.ResetCooldown, Cooldown = 16f } },
                "Cast it through two enemies standing in one line: both are cut and Marked, and Step Into Light is ready again. Light is straight.");

            // The counter: the rod form is the lamp; enemies in its glow are struck lightly and Marked. Its perfect-guard burst is
            // banked by each lamp (three, the most a lamp is likely to meet). NEEDS: the burst only for a perfect guard made
            // inside the glow (in-own-ground), and no enemy cloud, flame or frost inside the glow (purify).
            SpellBranch.Add(to, E, StandingLight, "Standing Light", SpellRole.Counter, BranchPattern.LeftB, JustCut, SpellForms.Rod,
                new SpellSpec { Reach = 2.5f, Radius = 5f, Seconds = 10f, Interval = 2.5f, Damage = 2f, Stagger = 0f, Delay = 0.2f, Stacks = 1 },
                "You plant a lamp of brass. A shaft of light stands over it, dust turning in it, and a glow five metres round lies about it for ten seconds. Enemies in the glow are Marked every two and a half seconds, and a perfect guard made inside it bursts round you: 10 damage and 25 stagger within three and a half metres. No enemy cloud, flame or frost can lie inside the glow.",
                UpgradeIcons.Tower,
                // The roster's numbers alone (the owner's answer of 2026-11-20): no shorter wait. NEEDS: the longer windows inside
                // the glow (in-own-ground: standing inside one's own lamp's glow as a state the stats read). Until then it does
                // nothing.
                new Empower(),
                "Inside its glow, the moments for a perfect guard and a perfect dodge are half again as long.",
                // NEEDS: the enemies between two lamps, and the guard made in line with them (in-own-ground), the lamps drawn as
                // three (form-pieces), and nothing laid between them (purify). Until then the lamp still stands, a row of light
                // six metres ahead Marks what stands on it each second, and a perfect guard throws a ray along the attacker's line.
                new ShapeDef { Name = "Row of Lamps",
                    Description = "Three small lamps are set in a line six metres long. Every enemy between two lamps is Marked each second and can carry no cloud, flame or frost, and a perfect guard made in line with them throws a ray down the row: 12 damage and 25 stagger. The guard fires the row.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Hazard, At = EffectAt.Self, Shape = HazardShape.Line, Reach = 6f, Count = 3, Radius = 1f, Seconds = 10f, Interval = 1f, Damage = 0f, Stagger = 0f, Affliction = M, Stacks = 1 },
                        new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Lane, Reach = 6f, Radius = 0.6f, Damage = 12f, Stagger = 25f, Affliction = M, Stacks = 1, Cooldown = 1f },
                    } },
                // NEEDS: the lamp drawn circling you, and hazards you walk into burned away (form-pieces, purify). The circling is
                // the discs form, one disc at two metres for eight seconds.
                new ShapeDef { Name = "Circled Lamp", Form = SpellForms.Discs,
                    Description = "A smaller lamp circles you two metres out, once a second, for eight seconds. It Marks what it passes and burns away any hazard you walk into. It goes where you go.",
                    Spell = new SpellSpec { Count = 1, Radius = 2f, Seconds = 8f, Interval = 1f, Damage = 2f, Stagger = 0f, Stacks = 1 } },
                "Opens two shapes of Standing Light, chosen at any rest fire: Row of Lamps, a row your perfect guard fires, or Circled Lamp, a lamp that walks round you.",
                // NEEDS: inside the glow only (in-own-ground). The resets are data, once in twelve seconds; Daybreak waits on no
                // clock, so it has none to end.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.ResetCooldown, Cooldown = 12f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.ResetCooldown, Branch = Glare, Cooldown = 12f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.ResetCooldown, Branch = StepIntoLight, Cooldown = 12f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.ResetCooldown, Branch = RingOfSpears, Cooldown = 12f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.ResetCooldown, Branch = Reckoning, Cooldown = 12f },
                },
                "The first perfect guard inside the glow makes every Light spell ready again, once a lamp.",
                tally: new TallySpec { Name = "Lamp", Cap = 3 },
                learnEffects: new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Bank, Count = 3 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Spends = 1, Do = UpgradeAction.Burst, At = EffectAt.Self, Radius = 3.5f, Damage = 10f, Stagger = 25f, Stacks = 0 },
                });

            // Built as data (StatWhen.Unhurt). It grows from Standing Light's Learn, whose line passes clear.
            to.Add(BranchPattern.Put(new Upgrade { Id = Unblemished, Name = "Unblemished", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { StandingLight }, Icon = UpgradeIcons.Star,
                Description = "Fight a while without a wound and every blow you land is critical. A wound ends it, and until it returns you take more from every blow.",
                Stats = new[] { new StatMod(PlayerStat.CritChance, 1f, StatWhen.Unhurt), new StatMod(PlayerStat.ManaFromBlows, 0.25f, StatWhen.Unhurt), new StatMod(PlayerStat.DamageTaken, 0.15f), new StatMod(PlayerStat.DamageTaken, -0.15f, StatWhen.Unhurt) } },
                BranchPattern.LeftCrown));

            // ---- The right bough: Radiance. The light itself, kept and loosed.
            to.Add(BranchPattern.Put(new Upgrade { Id = Brimming, Name = "Brimming", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, Icon = UpgradeIcons.Wave,
                Description = "While your mana is full, every heavy blow throws a wave of light ahead of it.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, While = StatWhen.ManaFull, Do = UpgradeAction.Cone, Reach = 5f, Arc = 70f, Damage = 12f, Stagger = 12f, Affliction = M, Stacks = 1 } } },
                BranchPattern.RightFoot));

            // Glare, the base spell: cast by Light's kit, so its Learn carries no form; its shapes add to the kit's ray or cast
            // their own.
            SpellBranch.Add(to, E, Glare, "Glare", SpellRole.Opener, BranchPattern.RightA, Root, "", null,
                "Light's own spell, given with the element. A narrow ray five and a half metres along your facing, the dust lit along it: 8 damage, 22 stagger and Marked to what it crosses. What it marks wears brackets of gold.",
                UpgradeIcons.Sun,
                // NEEDS: 12 damage and seven metres (base-spell-marks). The base spell is cast by its kit, which takes only the
                // Empower's wait; Seven Horses, cast through the mark, does take them (empower-after-shape).
                new Empower { Cooldown = 5f, Damage = 12f, Reach = 7f },
                "Glare waits 5 seconds, not 6, strikes for 12 and reaches seven metres.",
                // NEEDS: held for up to a second, the ray narrowing and the stagger rising with the hold (held-press). Until then
                // every ray strikes as at a full hold: 38 more stagger on what it crosses.
                new ShapeDef { Name = "Gathered",
                    Description = "Hold for up to a second and the ray narrows, the dust in it drawn to a point on the enemy. Let go, and the stagger rises from 22 to 60 with the hold. A lens, for breaking one enemy.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 0f, Stagger = 38f, Stacks = 0 } } },
                // NEEDS: seven thin rays rather than one cone (form-pieces). Until then the wave form throws a cone of the fan's
                // width that strikes and Marks all in it.
                new ShapeDef { Name = "Seven Horses", Form = SpellForms.Wave,
                    Description = "The ray splits into seven thin rays in a fan of 70 degrees, six metres long, as Surya's seven horses run: 5 damage each, and every enemy touched is Marked. Marking a crowd.",
                    Spell = new SpellSpec { Reach = 6f, Arc = 70f, Damage = 5f, Stagger = 6f, Stacks = 1 } },
                "Opens two shapes of Glare, chosen at any rest fire: Gathered, a ray drawn to a point that breaks one enemy, or Seven Horses, a fan of rays that Marks a crowd.",
                // Dazzle, once a major, folded in here. NEEDS: the cast in the perfect dodge's window as the moment
                // (spell-windows), and a blinding that stops only turning. Until then a perfect dodge holds the enemy faced and
                // all within two metres of it for two seconds and Marks them, once in six seconds.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Hold, Radius = 2f, Seconds = 2f, Cooldown = 6f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Afflict, Affliction = M, Stacks = 1, Radius = 2f, Cooldown = 6f },
                },
                "Cast it in the perfect dodge's moment, and the attacker is blinded and cannot turn for two seconds, one for a god or an elite.");

            // The mark is laid on what the reaction struck (UpgradeTrigger.ReactionStruck). NEEDS: every reaction with Marked
            // in it critical (reaction-stats).
            to.Add(BranchPattern.Put(new Upgrade { Id = SeenPlainly, Name = "Seen Plainly", Element = E, Kind = UpgradeKind.Major, Lifts = E, Requires = new[] { Brimming }, Icon = UpgradeIcons.Eye,
                Description = "Every reaction Marked takes part in is critical, and Marks everything it reaches.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.ReactionStruck, ReactionWith = M, Do = UpgradeAction.Afflict, Affliction = M, Stacks = 1 } } },
                BranchPattern.RightMiddle));

            // The larger spell, as built: the beam is loosed at once at the strength of six rings over its four bites. NEEDS:
            // begun, then gathered ring by ring by the blows you land, eight at most, and loosed by a second press
            // (gathered-rings, second-press).
            SpellBranch.Add(to, E, Daybreak, "Daybreak", SpellRole.Detonator, BranchPattern.RightB, SeenPlainly, SpellForms.Beam,
                new SpellSpec { Reach = 12f, Radius = 0.8f, Seconds = 0.4f, Interval = 0.1f, Damage = 12f, Stagger = 9f, Stacks = 1 },
                "Begin, and every blow you land adds a ring of light up the blade, eight at most. Press again to loose it all as a beam twelve metres long and a metre and a half wide: 8 damage and 6 stagger a ring, and it Marks. 45 mana.",
                UpgradeIcons.Beam,
                // The roster's numbers alone (the owner's answer of 2026-11-20): 10 a ring, not 8, on the beam (four bites of 15
                // for six rings), and the shapes' own wall and column left as they are (LearnOnly).
                new Empower { Damage = 15f, LearnOnly = true },
                "Each ring of the beam strikes for 10, not 8.",
                // NEEDS: each blow it stops adding a ring, and the beam loosed by a second press (gathered-rings, second-press);
                // stopping thrown attacks as well (form-pieces). Until then the wall form stands before you four seconds and
                // bursts along its width when its time is up or your heavy blow breaks it.
                new ShapeDef { Name = "Red Before the Sun", Form = SpellForms.Wall,
                    Description = "The rings are not loosed as a beam. They stand before you as a curved shield of red dawn three metres wide for four seconds, which stops thrown attacks and blows from the front. Each blow it stops adds a ring; press again to loose what is left as the beam. Light's one shield, and still a beam at the end.",
                    Spell = new SpellSpec { Reach = 1.5f, Arc = 3f, Delay = 0.15f, Seconds = 4f, Radius = 2.5f, Damage = 48f, Stagger = 36f, Stacks = 1 } },
                // NEEDS: enemy clouds, flame, frost and hazards burned away inside (purify), and the afflictions on you cleared
                // (afflictions-on-player). The column is the nova form, six rings' worth.
                new ShapeDef { Name = "High Noon", Form = SpellForms.Nova,
                    Description = "The light comes straight down on you as a column five metres across. It burns away every enemy cloud, flame, frost and hazard inside, clears every affliction on you, and strikes everything inside for 6 damage a ring, Marking it. The purifying version.",
                    Spell = new SpellSpec { Radius = 2.5f, Delay = 0.3f, Damage = 36f, Stagger = 36f, Stacks = 1 } },
                "Opens two shapes of Daybreak, chosen at any rest fire: Red Before the Sun, a shield of dawn before the beam, or High Noon, a column that burns the ground clean.",
                // NEEDS: the loosing within 0.2 s of a blow landing on you as the moment (blow-about-to-land). Until then every cast
                // turns the next blow that would land, and one loosed in the half second after a perfect guard strikes half again
                // as hard.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Ward, Count = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Boon, Stat = PlayerStat.SpellDamage, Amount = 0.5f, Seconds = 0.5f },
                },
                "Loose it within a fifth of a second of a blow landing on you, and the blow is turned and the beam is critical.");

            // The RuleBook's affliction rules (task 4.4), read by every enemy's set: a mark is not spent by blows, and once it
            // has faded no mark can be laid on that enemy for 4 s. It fades in six seconds, not eight (the stat: every
            // affliction you lay lasts a quarter less, and all in on Light that is the mark).
            to.Add(BranchPattern.Put(new Upgrade { Id = FullDaylight, Name = "Full Daylight", Element = E, Kind = UpgradeKind.Devotion, Requires = new[] { SeenPlainly }, Icon = UpgradeIcons.Sun,
                Description = "All in on Light only. A mark is no longer spent by blows: every weapon blow on a Marked enemy is critical until the mark fades, in six seconds, but no mark can be laid on that enemy again for four seconds after. Mark, spend the window in full, wait out the light.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionSeconds, -0.25f) },
                Rules = book =>
                {
                    book.Afflictions.NotSpentByBlows(AfflictionKind.Marked);
                    book.Afflictions.RestAfterFading(AfflictionKind.Marked, DaylightRest);
                } },
                BranchPattern.RightCrown));

            // BESPOKE (built): ScalesForm. Found wanting, the mana is emptied.
            to.Add(BranchPattern.Put(new Upgrade { Id = TheScales, Name = "The Scales", Element = E, Kind = UpgradeKind.Signature, Icon = UpgradeIcons.Crown,
                Description = "While Clarity lasts, the wounds you deal and the wounds you take are weighed. If yours are the heavier when it ends, a sword of light falls on every Marked enemy. If not, the light leaves you, and your mana with it.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.Scales, Spell = new SpellSpec { Cost = 0f, Damage = 60f, Stagger = 80f, Radius = 2.5f, Reach = 12f, Count = 120, Stacks = 1 } } } },
                BranchPattern.Gift));
        }
    }
}

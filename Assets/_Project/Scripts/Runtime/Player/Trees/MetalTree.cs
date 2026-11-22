using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Metal: meet the blow exactly, and drive steel in, written as spell branches (docs/design/steel/elements_b.md, Metal;
    /// Phase 4). Six spells of four marks each and nine passives on the pattern of <see cref="BranchPattern"/>: in the
    /// middle Edge (Counter Cut, Poured Bronze and Grave of Swords, Master's Answer at the crown), on the left Guard (Open
    /// Line, Returning Edge and Circle of Steel, Edge Guard at the crown, grown from the circle), on the right Arsenal
    /// (Forged Answer, Flung Blade, the lift Every Crack Rings, To the Blade, and the Devotion The Guard Holds at the
    /// crown). Flung Blade is the base spell: it comes with Metal, its Learn is never bought, and it grows from the root.
    ///
    /// Marks the design keeps keep their ids; the cut ones are in <see cref="RetiredMarks"/> (Buried Blades lives on in
    /// Flung Blade's Empower, Hone in the charged blade, and Returning Edge is rebuilt under its id as the turning blade of
    /// trees_b.md). Counter Cut and Master's Answer are done by hand in <see cref="PlayerUpgrades"/> under their ids; the
    /// rest are data. What the data cannot say yet is the nearest working version with a <c>NEEDS</c> line, gathered in
    /// docs/design/steel/content_open.tsv.
    /// </summary>
    public static class MetalTree
    {
        public const string Root = "metal.sunder";
        public const string ForgedAnswer = "metal.forgedanswer";
        public const string OpenLine = "metal.openline";
        public const string EdgeGuard = "metal.edgeguard";
        public const string CounterCut = "metal.countercut";
        public const string MastersAnswer = "metal.mastersanswer";
        public const string EveryCrackRings = "metal.lift";
        public const string TheGuardHolds = "metal.devotion";
        // The spells: each is its branch's Learn mark and the branch's id.
        public const string FlungBlade = "metal.base";
        public const string CircleOfSteel = "metal.circle";
        public const string ReturningEdge = "metal.returningedge";
        public const string PouredBronze = "metal.bronze";
        public const string ToTheBlade = "metal.toblade";
        /// <summary>To the Blade's Chain: an enemy tripped flat crossing it.</summary>
        public const string Tripped = "metal.toblade.tripped";
        public const string GraveOfSwords = "metal.graveofswords";
        public const string AllBladesHome = "metal.allbladeshome";
        // The god's gift.
        public const string HangingSwords = "metal.hangingswords";
        // Conditions the branches set on enemies.
        public const string CherryRed = "metal.base.red";
        /// <summary>Flung Blade's two shapes by name: PlayerUpgrades.OrdersOf reads which is worn to plant the kit's blade.</summary>
        public const string CherryRedName = "Cherry Red", NailName = "The Nail";
        public const string Nailed = "metal.base.nail";
        public const string Kneeling = "metal.base.kneel";
        public const string GoldenChair = "metal.circle.chair";
        public const string OnTheLine = "metal.returningedge.line";
        public const string Crusted = "metal.bronze.crust";
        public const string Molten = "metal.bronze.molten";
        public const string Ringing = "metal.graveofswords.ringing";
        public const string InTheGrave = "metal.graveofswords.grave";

        // Retired in Phase 4 (RetiredMarks): kept as a name because code still asks for it.
        public const string ReadySteel = "metal.readysteel";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Metal;
            const AfflictionKind S = AfflictionKind.Sundered;

            // The design notches with heavy blows only; a root lays its affliction with every blow, as the schema asks.
            to.Add(BranchPattern.Put(new Upgrade { Id = Root, Name = "Sunder", Element = E, Kind = UpgradeKind.Root, Icon = UpgradeIcons.Anvil,
                Description = "Heavy blows and perfect guards notch an enemy's armour, up to three, and a finisher notches it three deep at once. Each notch lets your weapon bite deeper.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = S, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, Affliction = S, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Afflict, Affliction = S, Stacks = 3 },
                } },
                BranchPattern.Root));

            // ---- The middle bough: Edge. The weapon itself, and what it drives into the notches.
            // Built (PlayerCombat.CounterCut): a blow that lands on an enemy winding up or striking is critical. The design's 25% more stagger is not in it.
            to.Add(BranchPattern.Put(new Upgrade { Id = CounterCut, Name = "Counter Cut", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, Icon = UpgradeIcons.Eye,
                Description = "A blow that lands while an enemy is showing its own attack is critical.",
                Stats = new StatMod[0] },
                BranchPattern.MiddleFoot));

            // The detonator: a nova that strikes for nothing; each notched enemy it reaches has bronze poured into its cracks
            // for each notch, is held, and its notches are spent. The count comes first and the spend last, so the pour is
            // counted before the notches go; the hold is 0.4 s a notch (PerStack). NEEDS: a quarter of the hold on a god or an
            // elite (count-from-depth; now the enemy's own freeze share), a lip of bronze tipping over each enemy and a crust
            // cooling on it (form-pieces).
            SpellBranch.Add(to, E, PouredBronze, "Poured Bronze", SpellRole.Detonator, BranchPattern.MiddleA, CounterCut, SpellForms.Nova,
                new SpellSpec { Radius = 8f, Delay = 0.4f, Damage = 0f, Stagger = 0f, Stacks = 0 },
                "You raise a hand. Over every notched enemy within eight metres a lip of molten bronze tips in the air, then pours into its cracks: 10 damage, and 12 damage and 15 stagger for each notch. The notches are spent. The bronze sets, and the enemy is held under a crust cooling from cherry red to black, longer for each notch it carried.",
                UpgradeIcons.Anvil,
                // The roster's numbers alone (the owner's answer of 2026-11-20): 15 a notch, set on the Learn's own pour.
                new Empower { Effects = new[] { new EffectChange { Index = 1, Damage = 15f } } },
                "It pours 15 damage for each notch, not 12.",
                // Only the enemies poured into are crusted (Needs: every effect of the moment is asked before the Learn spends
                // the notches it tests).
                new ShapeDef { Name = "Set",
                    Description = "The crust stays four seconds. A heavy blow on a crusted enemy shatters it for 20 more and throws bronze splinters three metres out behind it, notching what they hit. Pour, then break.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = S, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Crusted, Seconds = 4f, EndsWith = ConditionBlow.Heavy } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Crusted, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 1f, Damage = 20f, Stagger = 15f, Stacks = 0 },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Crusted, By = StruckBy.Heavy, Do = UpgradeAction.Lane, Reach = 3f, Radius = 0.8f, Damage = 8f, Stagger = 8f, Affliction = S, Stacks = 1 },
                    } },
                // No hold while it is worn (ShapeDef.SetsAside: the Learn's hold). Only the enemies poured into are molten, and
                // each keeps the notches the pour spent (CountsNeeds): every weapon blow spits 6 for each (ByPower).
                new ShapeDef { Name = "Red-Hot", SetsAside = new[] { 3 },
                    Description = "The bronze stays molten in the cracks for four seconds. There is no hold, but every weapon blow on that enemy spits sparks and deals 6 more for each notch that was spent. Pour, then press.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = S, CountsNeeds = true, PerStack = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Molten, Seconds = 4f } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Molten, By = StruckBy.Weapon, Do = UpgradeAction.Burst, ByPower = true, Share = 6f, Radius = 0.5f, Stagger = 0f, Stacks = 0 },
                    } },
                "Opens two shapes of Poured Bronze, chosen at any rest fire: Set, a crust your heavy blow shatters, or Red-Hot, bronze that burns under every blow.",
                // Cast within a second of a perfect guard: the pour starts no wait, and the attacker's pour is doubled, counted by its
                // notches before the pour spends them (UpgradeEffect.CastAfter, NeedsAttacker, CountsNeeds).
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.InstantNext, Seconds = 1f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, CastAfter = CastMoment.PerfectGuard, CastWindow = 1f, NeedsAttacker = true, Needs = S, CountsNeeds = true, PerStack = true,
                        Do = UpgradeAction.Burst, Radius = 0.6f, Damage = 12f, Stagger = 15f, Stacks = 0 },
                },
                "Cast it within a second after a perfect guard, and the attacker's pour is doubled and Poured Bronze is ready again.",
                learnEffects: new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = S, Do = UpgradeAction.Spend, Affliction = S, CountOnly = true },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = S, Do = UpgradeAction.Burst, PerStack = true, Radius = 0.6f, Damage = 12f, Stagger = 15f, Stacks = 0 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = S, Do = UpgradeAction.Burst, Unseen = true, Radius = 0.6f, Damage = 10f, Stagger = 0f, Stacks = 0 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = S, Do = UpgradeAction.Hold, PerStack = true, Seconds = 0.4f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = S, Do = UpgradeAction.Spend, Affliction = S },
                });

            // The larger spell, as built: the phalanx hangs the blades at the shoulders, and they fly by themselves at the
            // nearest enemy; a heavy blow sends all that remain. NEEDS: the twelve falling in a ring 4.5 m round you (8 damage
            // under each) and standing, a light blow calling the nearest through its enemy, a notch every three (form-pieces,
            // count-rules). Until then every blade notches.
            SpellBranch.Add(to, E, GraveOfSwords, "Grave of Swords", SpellRole.Breaker, BranchPattern.MiddleB, CounterCut, SpellForms.Phalanx,
                new SpellSpec { Count = 12, Seconds = 12f, Reach = 6f, Damage = 9f, Stagger = 6f, Stacks = 1, Speed = 16f },
                "Twelve blades fall in a ring four and a half metres round you, 8 damage under each, and stand twelve seconds. Each light blow you land calls the nearest one through that enemy, 9 damage and 6 stagger, and a heavy blow calls all that remain. Every three add a notch. 50 mana.",
                UpgradeIcons.Blade,
                new Empower { Mana = 45f, Count = 16 },
                "Grave of Swords costs 45 mana, not 50, and sixteen blades fall.",
                // NEEDS: the blades standing in their ring and ringing in place of flying (form-pieces, shape-sets-aside).
                // Until then the enemies within eight metres as it is cast ring for its twelve seconds under your blows.
                new ShapeDef { Name = "Clashing Bronze",
                    Description = "The blades do not fly; they ring. Each light blow you land makes the nearest standing blade ring, a sharp ripple three metres from it: 8 damage, 15 stagger and a notch to all it reaches. A heavy blow rings them all at once, and they fall. Every blow rings.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Condition, Radius = 8f, State = new ConditionSpec { Id = Ringing, Seconds = 12f } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Ringing, By = StruckBy.Weapon, Do = UpgradeAction.Burst, Radius = 3f, Damage = 8f, Stagger = 15f, Affliction = S, Stacks = 1, Cooldown = 0.2f },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Ringing, By = StruckBy.Heavy, Do = UpgradeAction.Burst, At = EffectAt.Self, Radius = 4.5f, Damage = 16f, Stagger = 30f, Affliction = S, Stacks = 1, Cooldown = 1f },
                    } },
                // NEEDS: going to notched enemies first, a notch every third cut, and a heavy blow calling each home through
                // what it passes (form-pieces, count-rules). Until then three helpers cut the nearest, and every cut notches.
                new ShapeDef { Name = "The Tripods", Form = SpellForms.Minions,
                    Description = "Three bronze tripods roll out, four blades on each, and go to notched enemies first, cutting twice a second: 9 damage, and a notch every third cut. A heavy blow you land calls each one back to you, and it throws its remaining blades through whatever it passes on the way.",
                    Spell = new SpellSpec { Count = 3, Seconds = 6f, Interval = 0.5f, Damage = 9f, Stagger = 6f, Stacks = 1 } },
                "Opens two shapes of Grave of Swords, chosen at any rest fire: Clashing Bronze, blades that ring under every blow, or The Tripods, bronze helpers that cut for you.",
                // A finisher (StruckBy.Finisher) on an enemy that was within eight metres as it was cast gives the mana, once a
                // cast, only while the blades stand (WhileOwnStands). NEEDS: every blade left loosed by it (form-pieces).
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Condition, Radius = 8f, State = new ConditionSpec { Id = InTheGrave, Seconds = 12f } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = InTheGrave, By = StruckBy.Finisher, WhileOwnStands = true, Do = UpgradeAction.Mana, Amount = 20f, Cooldown = 12f },
                },
                "A finisher looses every blade left, and 20 mana comes back to you.");

            // Built by hand (WeaponCombo.SkillCostScale and SkillAtOnce, kept by PlayerUpgrades): for two seconds after a perfect guard the
            // skill costs no mana and a press of heavy releases it at once; at any other time it costs half again as much.
            to.Add(BranchPattern.Put(new Upgrade { Id = MastersAnswer, Name = "Master's Answer", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { CounterCut }, Icon = UpgradeIcons.Star,
                Description = "After a perfect guard, your weapon's skill is free and comes at once, with no time to charge. At any other time it costs more.",
                Stats = new StatMod[0] },
                BranchPattern.MiddleCrown));

            // The third keystone, back under its id (the owner's word of 2026-11-20: trees may keep three). It stands at the
            // top of the trunk, grown from Grave of Swords' Master, the blades left standing.
            // Your thrown blades never fade (PlayerUpgrades.OrdersOf: sixteen at most stand, the oldest going home), and lighting
            // the surge calls every one home in a straight line through whatever stands between, 12 damage, 8 stagger and a
            // notch on each enemy on its line, each blade adding 0.4 s to the surge (UpgradeAction.CallBladesHome). NEEDS:
            // Grave of Swords' blades standing in their ring as blades of yours (form-pieces).
            to.Add(BranchPattern.Put(new Upgrade { Id = AllBladesHome, Name = "All Blades Home", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { SpellBranch.MasterId(GraveOfSwords) }, Icon = UpgradeIcons.Spiral,
                Description = "Blades you throw never fade: they stand in enemies or in the ground, sixteen at most. Beginning Temper calls every one back to you in a straight line, cutting and notching each enemy in the way, and each blade that comes home makes Temper last a little longer. Temper costs more.",
                Stats = new[]
                {
                    new StatMod(PlayerStat.SurgeCost, 0.375f),
                    new StatMod(PlayerStat.SurgeSeconds, 0.2f),
                },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.CallBladesHome, Damage = 12f, Stagger = 8f, Radius = 0.7f, Seconds = 0.4f, Affliction = S, Stacks = 1 } } },
                BranchPattern.TrunkRight));

            // ---- The left bough: Guard. Meet the blow exactly.
            // NEAREST: the attacker is held for a moment (it cannot turn, nor move or strike) and blows on it are a little harder. Blows do not count as from behind.
            to.Add(BranchPattern.Put(new Upgrade { Id = OpenLine, Name = "Open Line", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, Icon = UpgradeIcons.Arrow,
                Description = "A perfect guard throws the attacker's weapon wide. It is held still for a moment, and for that moment your weapon strikes 15% harder.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Hold, Seconds = 1.2f }, new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Boon, Stat = PlayerStat.WeaponDamage, Amount = 0.15f, Seconds = 1.5f } } },
                BranchPattern.LeftFoot));

            // Rebuilt to its trees_b design (the built one, a lunging head of steel, was Devouring Maw again): a turning blade
            // flies out along your facing and back to the hand, cutting what it passes each way (TurningBladeForm).
            SpellBranch.Add(to, E, ReturningEdge, "Returning Edge", SpellRole.Breaker, BranchPattern.LeftA, OpenLine, SpellForms.TurningBlade,
                new SpellSpec { Reach = 8f, Speed = 16f, Count = 1, Radius = 0.75f, Damage = 12f, Stagger = 10f, Stacks = 1 },
                "A turning blade a metre and a half across flies eight metres along your facing and back to your hand, drawing its lane on the ground: 12 damage and 10 stagger to each enemy it passes each way, and a notch on the first pass.",
                UpgradeIcons.Spiral,
                // The roster's numbers alone (the owner's answer of 2026-11-20): 16 and 14 each way.
                new Empower { Damage = 16f, Stagger = 14f },
                "It strikes for 16 damage and 14 stagger each way.",
                // The blade's arrival at the hand met by a perfect guard sends it out again, three times at most (SpellSpec.BatsBack).
                new ShapeDef { Name = "Bat It Home",
                    Description = "Guard inside the perfect window as it arrives, and it goes out again faster and harder, at 1.4 times the speed and one and a half times the damage. Three times at most.",
                    Spell = new SpellSpec { Reach = 8f, Speed = 16f, Count = 1, Radius = 0.75f, Damage = 12f, Stagger = 10f, Stacks = 1, BatsBack = 3 } },
                // It stops eight metres out and spins there, and only your next dodge calls it home (SpellSpec.WaitsOut).
                new ShapeDef { Name = "Through the Line",
                    Description = "It does not come back by itself. It stops eight metres out and spins in place for three seconds, 8 damage to what touches it every 0.3 seconds. Your next dodge calls it home along the line, cutting everything between: 20 damage, 30 stagger and a notch. Put them between you and the blade.",
                    Spell = new SpellSpec { Reach = 8f, Speed = 16f, Count = 1, Radius = 0.9f, Damage = 12f, Stagger = 10f, Stacks = 1, WaitsOut = true, Seconds = 3f, Interval = 0.3f, LingerDamage = 8f, HomeDamage = 20f, HomeStagger = 30f } },
                "Opens two shapes of Returning Edge, chosen at any rest fire: Bat It Home, a blade your perfect guard sends out again, or Through the Line, a blade that waits out there for your dodge to call it home.",
                // The catch itself (UpgradeTrigger.BladeCaught: a perfect guard as it arrives at the hand).
                new[] { new UpgradeEffect { On = UpgradeTrigger.BladeCaught, Do = UpgradeAction.ResetCooldown } },
                "Catch it on a perfect guard as it comes home, and Returning Edge is ready again at once.");

            // The counter: the parry form turns the first blow and answers with a cut all round; nothing struck, it waits 5 s
            // (SpellSpec.MissedWait). NEEDS: two blows turned with the Empower (parry-count).
            SpellBranch.Add(to, E, CircleOfSteel, "Circle of Steel", SpellRole.Counter, BranchPattern.LeftB, OpenLine, SpellForms.Parry,
                new SpellSpec { Seconds = 1.2f, Reach = 3f, Arc = 360f, Damage = 14f, Stagger = 20f, Stacks = 1, MissedWait = 0.5f },
                "You raise the blade for a breath, unable to move. The first blow from any side is turned for nothing and answered with a cut all round you, three metres: 14 damage, 20 stagger and a notch. If nothing strikes, it waits only half as long.",
                UpgradeIcons.Ring,
                // The roster's numbers alone (the owner's answer of 2026-11-20: an Empower may strengthen without a shorter wait).
                new Empower { Damage = 16f },
                "It turns two blows, and its answer strikes for 16.",
                // The cut all round kept, and only the attacker bound (UpgradeEffect.NeedsAttacker: the one whose blow was turned);
                // "passes any guard" is that it cannot raise one.
                new ShapeDef { Name = "The Golden Chair", Form = SpellForms.Parry,
                    Description = "A blow turned closes bands of gold round the attacker, a faint shimmer that turns bright as they lock. It is held two seconds, less for a god or an elite, notched twice, and your blows on it pass any guard.",
                    Spell = new SpellSpec { Seconds = 1.2f, Reach = 3f, Arc = 360f, Damage = 14f, Stagger = 20f, Stacks = 1 },
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, NeedsAttacker = true, Do = UpgradeAction.Hold, Seconds = 2f },
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, NeedsAttacker = true, Do = UpgradeAction.Afflict, Affliction = S, Stacks = 1 },
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, NeedsAttacker = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = GoldenChair, Seconds = 2f, Flags = ConditionFlags.CannotGuard } },
                    } },
                // NEEDS: every blow turned in 1.5 s ringing, four at most, each ring wider than the last (parry-count). Until then
                // the first blow turned rings once.
                new ShapeDef { Name = "Ringing Plate", Form = SpellForms.Parry,
                    Description = "You stand as a struck plate. Every blow turned in a second and a half, four at most, rings: a sharp ripple to four metres, 8 damage, 20 stagger and a notch on all, each ring wider than the last. The more of them strike you, the better.",
                    Spell = new SpellSpec { Seconds = 1.5f, Reach = 4f, Arc = 360f, Damage = 8f, Stagger = 20f, Stacks = 1 } },
                "Opens two shapes of Circle of Steel, chosen at any rest fire: The Golden Chair, bands that bind the one who struck, or Ringing Plate, a ring for every blow turned.",
                // Only a red attack turned (UpgradeEffect.RedOnly): the mana, and its wait ended.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, RedOnly = true, Do = UpgradeAction.ResetCooldown, Cooldown = 0.5f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, RedOnly = true, Do = UpgradeAction.Mana, Amount = 15f, Cooldown = 0.5f },
                },
                "Turn a red attack, and 15 mana comes back to you and Circle of Steel is ready again at once.");

            // The RuleBook's edge guard (GuardMode.Edge, read by PlayerDefence and Guard): a held guard blocks nothing; the perfect
            // window is 0.28 s (0.18 half again) and may be pressed again after 0.2 s; each perfect guard cuts the attacker. NEEDS: the
            // cut counted as a blow landed (its mana and its cut to the waits). It grows from Circle of Steel's Learn, whose line passes
            // clear, as the old tree had it.
            to.Add(BranchPattern.Put(new Upgrade { Id = EdgeGuard, Name = "Edge Guard", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { CircleOfSteel }, Icon = UpgradeIcons.Shield,
                Description = "Your guard is all edge. Held, it blocks nothing: only the perfect guard turns a blow. Its moment is half again as long and may be pressed again after a fifth of a second, and each one cuts the attacker and notches its armour.",
                Stats = new[] { new StatMod(PlayerStat.PerfectGuardWindow, 0.55f) },
                Rules = book => book.Guard = Growth.GuardMode.Edge,
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Burst, Radius = 1.2f, Damage = 12f, Stagger = 10f, Affliction = S, Stacks = 1 } } },
                BranchPattern.LeftCrown));

            // ---- The right bough: Arsenal. Steel thrown, planted and called back.
            to.Add(BranchPattern.Put(new Upgrade { Id = ForgedAnswer, Name = "Forged Answer", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, Icon = UpgradeIcons.Blade,
                Description = "A perfect guard forges a blade from the blow it turned. Up to three hang at your shoulder. Your next heavy blow sends them all.",
                Tally = new TallySpec { Name = "Blades", Cap = 3 },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Bank, Count = 1 }, new UpgradeEffect { On = UpgradeTrigger.HeavyHit, SpendsAll = true, PerStack = true, Do = UpgradeAction.Projectile, Flight = Flight.Homing, Count = 1, Damage = 10f, Stagger = 12f, Speed = 16f, Affliction = S, Stacks = 0 } } },
                BranchPattern.RightFoot));

            // Flung Blade, the base spell: cast by Metal's kit, so its Learn carries no form; its shapes add to the kit's blade.
            SpellBranch.Add(to, E, FlungBlade, "Flung Blade", SpellRole.Opener, BranchPattern.RightA, Root, "", null,
                "Metal's own spell, given with the element. A blade flies into the enemy faced within nine metres, 4 damage, and stands in it six seconds. Your next weapon blow on that enemy drives it in: 12 damage, 15 stagger, a notch and a burst of sparks, and it flies back to your hand, cutting the lane for a third as much.",
                UpgradeIcons.Blade,
                // Buried Blades, once a major, folded in here: 16 when driven, up to three blades standing in one enemy and a
                // heavy blow driving them all (PlayerUpgrades.OrdersOf, PlantedBlades). The Empower's 16 is the drive's, not the
                // throw's, so the kit's own cast takes none of it (Empower.NotForKit).
                new Empower { Damage = 16f, NotForKit = true },
                "It drives in for 16. Up to three blades may stand in one enemy, and a heavy blow drives them all.",
                // The kit's blade and two more thrown into two other enemies within its nine metres (UpgradeAction.PlantBlades),
                // each a Cherry Red blade: a ring of heat cooling over six seconds, and a drive in its first two striking double
                // (BladeRules.DriveDamage).
                new ShapeDef { Name = CherryRedName,
                    Description = "Three red-hot blades fly in a fan, each standing in a different enemy. They glow cherry red and cool to black over six seconds, and one driven while still red, in its first two seconds, strikes double. You throw wide and then hurry.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.PlantBlades, Count = 2, Reach = 9f, Affliction = S,
                            Spell = new SpellSpec { Damage = 12f, Stagger = 15f, Reach = 9f, Radius = 1.5f, Seconds = 6f, Count = 1, Stacks = 1 } },
                    } },
                // The kit's blade is the nail (PlantedKind.Nail): it stays ten seconds, only heavy blows drive it, 12, 16 and 24
                // (BladeRules.NailDrives), and it never comes home. Its third drive is the moment (UpgradeTrigger.BladeDriven
                // carrying the nail's drives), so only heavy blows count toward it.
                new ShapeDef { Name = NailName,
                    Description = "One long bronze nail is driven low into the enemy, and it does not come back. Each heavy blow on that enemy drives it deeper: 12, then 16, then 24. At the third it kneels for a second and a half, less for a god or an elite, and golden ichor runs out round it, a pool three metres across for four seconds that bites for 3 and notches. Three heavies on one enemy.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.BladeDriven, SpentAtLeast = BladeRules.NailKneelsAt, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Kneeling, Seconds = 1.5f, Flags = ConditionFlags.Down } },
                        new UpgradeEffect { On = UpgradeTrigger.BladeDriven, SpentAtLeast = BladeRules.NailKneelsAt, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 1.5f, Seconds = 4f, Interval = 1f, Damage = 3f, Stagger = 0f, Affliction = S, Stacks = 1 },
                    } },
                "Opens two shapes of Flung Blade, chosen at any rest fire: Cherry Red, a fan of blades to drive while they glow, or The Nail, one nail three heavy blows drive home.",
                // Every blade standing in the attacker driven at once (UpgradeAction.DriveBlades: each strikes as its own drive).
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.DriveBlades },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.ResetCooldown },
                },
                "A perfect guard drives every blade standing in the attacker at once, and Flung Blade is ready again.");

            // Every reaction's own blow on a notched enemy strikes 25% harder (PlayerStat.ReactionDamageOn), and every reaction with
            // Sundered in it notches what it struck (UpgradeTrigger.ReactionStruck).
            to.Add(BranchPattern.Put(new Upgrade { Id = EveryCrackRings, Name = "Every Crack Rings", Element = E, Kind = UpgradeKind.Major, Lifts = E, Requires = new[] { ForgedAnswer }, Icon = UpgradeIcons.Burst,
                Description = "Every reaction Sundered takes part in rings out like struck plate. It notches everything it reaches, and strikes 25% harder on the notched.",
                Stats = new[] { new StatMod(PlayerStat.ReactionDamageOn, 0.25f, S) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.ReactionStruck, ReactionWith = S, Do = UpgradeAction.Afflict, Affliction = S, Stacks = 1 } } },
                BranchPattern.RightMiddle));

            // The movement spell: the drill form carries you through what lies on the line and strikes it, untouchable, to the
            // nearest blade of yours standing within 12 m (SpellSpec.BladeReach, PlantedBlades: a Flung Blade in an enemy or the
            // ground), and drives that blade as you arrive; with none, six metres ahead. NEEDS: Grave of Swords' blades standing
            // as blades of yours (form-pieces).
            SpellBranch.Add(to, E, ToTheBlade, "To the Blade", SpellRole.Movement, BranchPattern.RightB, EveryCrackRings, SpellForms.Drill,
                new SpellSpec { Reach = 6f, Radius = 1f, Count = 1, Damage = 10f, Stagger = 15f, Stacks = 1, Speed = 24f, BladeReach = BladeRules.ToBladeReach },
                "You fly along a line of sparks to a blade of yours within twelve metres, a Flung Blade standing in an enemy or the ground or a blade of Grave of Swords, untouchable for the quarter second of the flight. What lies between is cut, 10 damage, 15 stagger and a notch, and you arrive driving that blade. With no blade out, you throw one six metres ahead into the ground and go to it.",
                UpgradeIcons.Arrow,
                new Empower { Cooldown = 7f, BladeReach = BladeRules.ToBladeEmpoweredReach },
                "To the Blade waits 7 seconds, not 8, and reaches a blade fourteen metres off.",
                // The chain is ground along your path that notches and staggers what stands on it, and trips each one that crosses
                // it flat for 0.6 s, once in 1.5 s each (UpgradeEffect.InsideDo).
                new ShapeDef { Name = "Chain",
                    Description = "A bronze chain stays stretched along the line you flew for three seconds. An enemy that crosses it is notched and tripped, 20 stagger. Your path becomes a tripwire.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Hazard, At = EffectAt.Self, Shape = HazardShape.Line, Reach = 6f, Count = 5, Radius = 0.6f, Seconds = 3f, Interval = 0.5f, Damage = 0f, Stagger = 10f, Affliction = S, Stacks = 1,
                        InsideDo = new[] { new UpgradeEffect { On = UpgradeTrigger.InGround, Do = UpgradeAction.Condition, CooldownPerEnemy = true, Cooldown = 1.5f,
                            State = new ConditionSpec { Id = Tripped, Seconds = 0.6f, Flags = ConditionFlags.Down } } } } } },
                // The flight back as a second press within 2 s of arriving (ShapeDef.Presses, UpgradeAction.ReturnToCast), cutting the
                // line a second time as the flight did: 10 damage, 15 stagger and a notch.
                new ShapeDef { Name = "There and Back", PressWindow = 2f,
                    Description = "Within two seconds of arriving, press again to fly back along the same line, cutting it a second time. In and out.",
                    Presses = new[] { new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, Do = UpgradeAction.ReturnToCast, Radius = 1f, Damage = 10f, Stagger = 15f, Affliction = S, Stacks = 1 } } } },
                "Opens two shapes of To the Blade, chosen at any rest fire: Chain, a tripwire left along your flight, or There and Back, a flight out and home again.",
                // Cast as a blow would land (UpgradeEffect.BlowLanding): a perfect dodge (UpgradeAction.PerfectDodgeNow), untouchable from the
                // press to the blade, and its wait ended.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, BlowLanding = 0.3f, Do = UpgradeAction.PerfectDodgeNow },
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, BlowLanding = 0.3f, Do = UpgradeAction.Untouchable, Seconds = 0.6f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, BlowLanding = 0.3f, Do = UpgradeAction.ResetCooldown },
                },
                "Cast it in the perfect dodge's moment, and you are untouchable from the press and To the Blade is ready again.");

            // The RuleBook's guard (task 4.4), read by PlayerDefence: the guard never breaks, and what the stamina cannot pay
            // comes through as grey health (Guard.Resolve). You take 15% less, and every blow held on the guard notches and sparks.
            to.Add(BranchPattern.Put(new Upgrade { Id = TheGuardHolds, Name = "The Guard Holds", Element = E, Kind = UpgradeKind.Devotion, Requires = new[] { EveryCrackRings }, Icon = UpgradeIcons.Shield,
                Description = "All in on Metal only. Your guard never breaks: a blow your stamina cannot pay for is held anyway, and what it cannot pay comes through as grey health. Every blow held on your guard notches the attacker and throws sparks into it. You take 15% less from everything. Red attacks still need a dodge or a perfect guard.",
                Stats = new[] { new StatMod(PlayerStat.DamageTaken, -0.15f) },
                Rules = book => book.Guard = Growth.GuardMode.Unbreakable,
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Guard, Do = UpgradeAction.Burst, Radius = 0.6f, Damage = 4f, Stagger = 0f, Affliction = S, Stacks = 1, Cooldown = 0.2f } } },
                BranchPattern.RightCrown));

            // BESPOKE (built): HangingSwordsForm; the held guard is perfect through the window made as long as the surge.
            to.Add(BranchPattern.Put(new Upgrade { Id = HangingSwords, Name = "Hanging Swords", Element = E, Kind = UpgradeKind.Signature, Icon = UpgradeIcons.Crown,
                Description = "While Temper lasts, every blow you hold on your guard is a perfect guard, and a sword is forged above the one who struck, up to eight. When Temper ends, they fall.",
                Stats = new[] { new StatMod(PlayerStat.PerfectGuardWindow, 200f, StatWhen.Surge) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Guard, While = StatWhen.Surge, Do = UpgradeAction.Mana, Amount = 5f, Cooldown = 0.3f }, new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, While = StatWhen.Surge, Do = UpgradeAction.Mana, Amount = 5f, Cooldown = 0.3f }, new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.HangingSwords, Spell = new SpellSpec { Cost = 0f, Count = 8, Damage = 16f, Stagger = 20f, Radius = 1.5f, Stacks = 1 } } } },
                BranchPattern.Gift));
        }
    }
}

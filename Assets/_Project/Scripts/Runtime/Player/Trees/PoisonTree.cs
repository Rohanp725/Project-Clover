using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Poison: lay doses, wait, burst, and the poison feeds you, written as spell branches (docs/design/steel/elements_b.md,
    /// Poison; Phase 4). Six spells of four marks each and nine passives on the pattern of <see cref="BranchPattern"/>: in
    /// the middle Rupture (Rupture, Blister and One by One, Volatile at the crown), on the left Patience (In Time, Five
    /// Needles and Coiled Strike, Slow Death at the crown, grown from the strike), on the right Contagion (Catching, Miasma,
    /// the lift The Delayed Touch, Share Alike, and the Devotion Tasted Every Herb at the crown). Miasma is the base spell:
    /// it comes with Poison, its Learn is never bought, and it grows from the root. The root carries Poison's self-heal.
    ///
    /// Marks the design keeps keep their ids; the cut ones are in <see cref="RetiredMarks"/> (Swinging Censer lives on in
    /// Miasma's Censer; Left to Ripen, One Sickness, Lance the Wound and Rotten Fruit are gone; Plague Bearer is a keystone
    /// not carried over). The marks are data. What the data cannot say yet is the nearest working version with a
    /// <c>NEEDS</c> line, gathered in docs/design/steel/content_open.tsv.
    /// </summary>
    public static class PoisonTree
    {
        public const string Venom = "poison.venom";
        public const string InTime = "poison.intime";
        public const string Catching = "poison.catching";
        public const string Rupture = "poison.rupture";
        public const string SlowDeath = "poison.slowdeath";
        public const string Volatile = "poison.volatile";
        public const string TheDelayedTouch = "poison.lift";
        public const string TastedEveryHerb = "poison.devotion";
        // The spells: each is its branch's Learn mark and the branch's id.
        public const string Miasma = "poison.base";
        public const string FiveNeedles = "poison.needles";
        public const string CoiledStrike = "poison.coiled";
        public const string Blister = "poison.blister";
        public const string ShareAlike = "poison.sharealike";
        public const string OneByOne = "poison.onebyone";
        public const string PlagueBearer = "poison.plaguebearer";
        // The god's gift.
        public const string CleanBlade = "poison.cleanblade";
        // Conditions the branches and passives set on enemies.
        public const string ThickCloud = "poison.base.thick";
        public const string RedPaper = "poison.needles.redpaper";
        public const string ShedSkin = "poison.coiled.skin";
        public const string WeepingSore = "poison.blister.weeping";
        public const string TheVessel = "poison.blister.vessel";
        public const string BrokenOnTheBeat = "poison.blister.beat";
        public const string EatenInTurn = "poison.onebyone.eaten";
        public const string HeldInTheBody = "poison.lift.held";

        // Retired in Phase 4 (RetiredMarks): kept as names because code still asks for them.
        public const string LeftToRipen = "poison.ripen";
        public const string OneSickness = "poison.onesickness";
        public const string LanceTheWound = "poison.lance";
        public const string ReadyPoison = "poison.readypoison";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Poison;
            const AfflictionKind P = AfflictionKind.Poison;

            // A light blow lays one dose, a heavy blow two, a finisher four (a finisher is also a heavy blow). The self-heal,
            // once Fever's alone, is the root's now (steel/README.md, "Moved"): a bite deals 2 a dose and a burst 6, so a tenth
            // came back as 0.2 and 0.6 a dose; now it is a tenth of what the poison really dealt, its strength and ripening
            // counted, given back over time (UpgradeAction.Mend).
            to.Add(BranchPattern.Put(new Upgrade { Id = Venom, Name = "Venom", Element = E, Kind = UpgradeKind.Root, Icon = UpgradeIcons.Fang,
                Description = "Blows of the weapon poison, up to eight deep. On a slow, steady beat the poison bites for all its depth at once, and a tenth of all the damage your poison deals comes back to you as health.",
                WeaponStacks = 1,
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = P, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Afflict, Affliction = P, Stacks = 2 },
                    new UpgradeEffect { On = UpgradeTrigger.PoisonBite, Do = UpgradeAction.Mend, Share = 0.1f },
                    new UpgradeEffect { On = UpgradeTrigger.SetOff, Spent = P, Do = UpgradeAction.Mend, Share = 0.1f },
                } },
                BranchPattern.Root));

            // ---- The middle bough: Rupture. Poison spent all at once.
            // It bursts poison of any depth, not only poison at its full depth.
            to.Add(BranchPattern.Put(new Upgrade { Id = Rupture, Name = "Rupture", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Venom }, Icon = UpgradeIcons.Burst,
                Description = "A heavy blow on an enemy poisoned to its full depth bursts the poison. All of it lands at once, and the depth is spent.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.SetOff, Needs = P, NeedsFull = true, Affliction = P, Stagger = 4f, Radius = 1f, Cooldown = 0.3f } } },
                BranchPattern.MiddleFoot));

            // The detonator: the brand form raises the blister on the enemy faced, and the next weapon blow on it breaks it.
            // NEEDS: the blister swelling at each bite through three sizes, broken by a heavy blow alone (blister-swells). Until
            // then it breaks at its middle size, 40 in three metres, on any weapon blow.
            SpellBranch.Add(to, E, Blister, "Blister", SpellRole.Detonator, BranchPattern.MiddleA, Rupture, SpellForms.Brand,
                new SpellSpec { Reach = 8f, Seconds = 12f, Count = 1, Radius = 3f, Damage = 40f, Stagger = 30f, Stacks = 4 },
                "You raise a blister on the enemy you face. It swells each time its poison bites, through three sizes. A heavy blow breaks it: 20, 40 or 70 damage in two, three or four metres, and 2, 4 or 6 doses on everyone else it reaches. Left too long, it withers.",
                UpgradeIcons.Orb,
                // The roster's numbers alone (the owner's answer of 2026-11-20: an Empower may strengthen without a shorter wait).
                new Empower { Damage = 50f },
                "It breaks for 25, 50 or 85.",
                // NEEDS: the blister set aside, never broken by a blow (shape-sets-aside), and a heavy blow bursting every
                // puddle where it lies (puddles-burst). Until then the sore weeps a puddle at each bite, and a heavy blow on the
                // sore bursts round it for 12 and two doses.
                new ShapeDef { Name = "Weeping Sore",
                    Description = "It never bursts by itself or by a heavy blow. Each time its poison bites it weeps a bubbling puddle a metre and a half across where the enemy stands, for three seconds, a dose a second, so a walking enemy lays your poison. Your heavy blow bursts every puddle at once: 12 damage and 2 doses to all in each.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = WeepingSore, Seconds = 12f, OnBite = true } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = WeepingSore, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 0.75f, Seconds = 3f, Interval = 1f, Damage = 0f, Stagger = 0f, Affliction = P, Stacks = 1 },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = WeepingSore, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 2f, Damage = 12f, Stagger = 0f, Affliction = P, Stacks = 2, Cooldown = 0.5f },
                    } },
                // NEEDS: the blister set aside (shape-sets-aside). The cloud at a full bite is data, once a cast.
                new ShapeDef { Name = "The Vessel",
                    Description = "The whole enemy swells. When its poison bites at full depth it bursts by itself into a cloud four metres across that doses everything in it three deep and hangs three seconds. You set it and leave it.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = TheVessel, Seconds = 12f, OnBite = true } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = TheVessel, Needs = P, NeedsFull = true, Do = UpgradeAction.Afflict, Affliction = P, Stacks = 3, Radius = 2f, Cooldown = 12f },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = TheVessel, Needs = P, NeedsFull = true, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 2f, Seconds = 3f, Interval = 1f, Damage = 0f, Stagger = 0f, Affliction = P, Stacks = 1, Cooldown = 12f },
                    } },
                "Opens two shapes of Blister, chosen at any rest fire: Weeping Sore, a sore that leaves a trail of puddles, or The Vessel, an enemy that bursts by itself when full.",
                // NEEDS: the blister's own burst doubled (blister-swells). Until then a heavy blow on the blistered enemy within a
                // fifth of a second of its bite strikes round it with a second burst as large, once a blister.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = BrokenOnTheBeat, Seconds = 12f, EndsWith = ConditionBlow.Heavy } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = BrokenOnTheBeat, By = StruckBy.Heavy, OnBeat = 0.2f, Do = UpgradeAction.Burst, Radius = 3f, Damage = 40f, Stagger = 30f, Affliction = P, Stacks = 4 },
                },
                "Break it with a heavy blow as its poison bites, within a fifth of a second, and the burst doubles.");

            // The larger spell: the pillars form comes up under the poisoned alone, six at most, in a line from the nearest to the
            // nearest to it (SpellSpec.OnlyCarrying), and each one's poison bursts in turn, 6 a dose, carrying the doses of the one
            // before it on (UpgradeEffect.PassesOn): the last the largest.
            SpellBranch.Add(to, E, OneByOne, "One by One", SpellRole.Detonator, BranchPattern.MiddleB, Rupture, SpellForms.Pillars,
                new SpellSpec { Count = 6, Reach = 10f, Radius = 1f, Delay = 0.5f, Damage = 0f, Stagger = 0f, Stacks = 0, OnlyCarrying = P },
                "Raise a hand. A green line runs along the ground from poisoned enemy to poisoned enemy, nearest first, then you snap your fingers and each one's poison bursts in turn, 6 damage and 4 stagger a dose, each passing its doses to the next before it goes. The last burst is the largest. 50 mana.",
                UpgradeIcons.Chain,
                // The roster's numbers alone (the owner's answer of 2026-11-20): 7 a dose, the Learn's set-off striking 1 more
                // for each dose over poison's own 6.
                new Empower { Effects = new[] { new EffectChange { Index = 0, Damage = 1f } } },
                "Its bursts strike for 7 for each dose, not 6.",
                // For eight seconds each enemy on the line that dies poisoned passes twice its doses to the nearest other on the
                // line (UpgradeEffect.ToState), else the nearest within ten metres; one pushed past eight bursts at once, 6 a dose,
                // and passes the rest on (UpgradeEffect.Overflow). NEEDS: nothing burst by the snap (shape-sets-aside), and the
                // next by the line's own order rather than the nearest on it. Until then the snap still bursts.
                new ShapeDef { Name = "Eaten in Turn",
                    Description = "The line is drawn, but nothing bursts. For eight seconds, when an enemy on it dies, all its doses pass to the next on the line, doubled; one pushed past eight bursts at once, 6 a dose, and passes on. The blade chooses the order.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = EatenInTurn, Seconds = 8f } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionDied, Condition = EatenInTurn, Needs = P, Do = UpgradeAction.Pass, Affliction = P, PerStack = true, Stacks = 2, Reach = 10f,
                            ToState = EatenInTurn, Overflow = true },
                    } },
                // The doses of every poisoned enemy within ten metres are gathered as it is cast (UpgradeAction.Gather) and the one
                // pillar, under the one faced when it is poisoned, bursts with them all, with no limit. NEEDS: the beads of mercury
                // rolling across the floor (form-pieces).
                new ShapeDef { Name = "All Into One",
                    Description = "No line. The poison runs out of every poisoned enemy within ten metres as beads of mercury that roll across the floor and join in the enemy you face, its doses added to its own with no limit. Then it alone bursts. For the one that matters.",
                    Change = new SpellChange { Count = 1 },
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Gather, Affliction = P, Radius = 10f } } },
                "Opens two shapes of One by One, chosen at any rest fire: Eaten in Turn, a line fed by the order you kill, or All Into One, every dose run into the one you face.",
                // The snap timed to the beat (UpgradeEffect.OnCastBeat): every burst of the cast half again as hard (PlayerStat.BurstDamage
                // while its columns rise).
                new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, OnCastBeat = true, Do = UpgradeAction.Boon, Stat = PlayerStat.BurstDamage, Amount = 0.5f, Seconds = 1.5f } },
                "Snap your fingers within a fifth of a second of the poison's beat, and every burst is one and a half times as strong.",
                learnEffects: new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.SetOff, Affliction = P, Stagger = 4f, Radius = 1f, PassesOn = true } });

            // The beat is stilled (Afflictions.PoisonBeatOff); what is full is set off. Nothing closes Slow Death to one who holds this.
            to.Add(BranchPattern.Put(new Upgrade { Id = Volatile, Name = "Volatile", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { Rupture }, Icon = UpgradeIcons.Burst,
                Description = "Your poison no longer bites on its beat. It builds until it is full, and then bursts by itself and begins again.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictionFull, Do = UpgradeAction.SetOff, Needs = P, Affliction = P, Damage = 1f, Stagger = 4f, Radius = 2.5f, Cooldown = 0.5f } } },
                BranchPattern.MiddleCrown));

            // The third keystone, back under its id (the owner's word of 2026-11-20: trees may keep three). It stands at the
            // top of the trunk, grown from One by One's Master. Built as designed: an aura of doses, paid for in flasks.
            to.Add(BranchPattern.Put(new Upgrade { Id = PlagueBearer, Name = "Plague Bearer", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { SpellBranch.MasterId(OneByOne) }, Icon = UpgradeIcons.Fang,
                Description = "You carry the sickness. Enemies near you are poisoned without a blow, deeper the longer they stay. Your flasks heal less.",
                Stats = new[] { new StatMod(PlayerStat.FlaskHeal, -0.3f) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.EverySeconds, Every = 1.5f, Do = UpgradeAction.Afflict, At = EffectAt.Self, Affliction = P, Stacks = 1, Radius = 4f } } },
                BranchPattern.TrunkRight));

            // ---- The left bough: Patience. The beat, and the moment to strike.
            // Built on the beat (UpgradeEffect.OnBeat): a blow within a quarter of a second of the target's bite doses again and makes the blows of the next moment critical. The timed blow itself is not made critical.
            to.Add(BranchPattern.Put(new Upgrade { Id = InTime, Name = "In Time", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Venom }, Icon = UpgradeIcons.Hourglass,
                Description = "A blow that lands as the poison bites doses twice, and for a moment after it your blows are critical.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Afflict, Needs = P, OnBeat = 0.25f, Affliction = P, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Boon, Needs = P, OnBeat = 0.25f, Stat = PlayerStat.CritChance, Amount = 1f, Seconds = 1.2f },
                } },
                BranchPattern.LeftFoot));

            // The breaker: the wave form throws a narrow fan of the five needles' worth at the enemy faced, and the first
            // struck is pinned, once a cast. NEEDS: five needles thrown, each its own strike, the pin only on one struck by
            // all five (form-pieces).
            SpellBranch.Add(to, E, FiveNeedles, "Five Needles", SpellRole.Breaker, BranchPattern.LeftA, InTime, SpellForms.Wave,
                new SpellSpec { Reach = 8f, Arc = 12f, Damage = 15f, Stagger = 50f, Stacks = 5 },
                "Five long needles, venom beading on them, are thrown in a narrow fan eight metres at the enemy you face: 3 damage, 10 stagger and a dose each. An enemy struck by all five is pinned at its points: held 0.8 seconds, less for a god or an elite, and 40 stagger more.",
                UpgradeIcons.Spike,
                // The roster's numbers alone, with no shorter wait (the owner's answer of 2026-11-20). The pin's 50 is the Learn's
                // own burst (empower-effects); the needles' 14 is laid on the fan alone, not on Spread's own needles (LearnOnly).
                new Empower { Stagger = 70f, LearnOnly = true, Effects = new[] { new EffectChange { Index = 1, Stagger = 50f } } },
                "Its needles break 14 stagger each, and an enemy struck by all five takes 50 more.",
                // NEEDS: one needle for each enemy (form-pieces), and the Learn's pin set aside (shape-sets-aside). Until then the
                // wide fan strikes all in it, each for one needle and two doses, and its poison bites at once.
                new ShapeDef { Name = "Spread",
                    Description = "A wide fan of 60 degrees, so each needle finds a different enemy. Each enemy pierced takes two doses, and its poison bites at once.",
                    Change = new SpellChange { Arc = 60f, Damage = 3f, Stagger = 10f, Stacks = 2 },
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Bite } } },
                // NEEDS: a blow it cannot turn, and blows that pass any shield (shield-pass). Its guard is taken away, as data.
                new ShapeDef { Name = "Red Paper",
                    Description = "All five needles pin one sheet of red paper to the enemy you face. For six seconds it cannot guard or turn a blow, and your blows on it pass any shield. The breaker for what hides behind a guard.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = RedPaper, Seconds = 6f, Flags = ConditionFlags.CannotGuard }, Cooldown = 1f } } },
                "Opens two shapes of Five Needles, chosen at any rest fire: Spread, a wide fan that finds a needle for every enemy, or Red Paper, a sheet pinned on one that takes away its guard.",
                // NEEDS: a bite for every needle (form-pieces). Until then the fan, struck on a poisoned enemy's beat, makes its
                // poison bite once more.
                new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = P, OnBeat = 0.2f, Do = UpgradeAction.Bite } },
                "Throw them on a poisoned enemy's beat, within a fifth of a second of its bite, and every needle that strikes it makes its poison bite once more.",
                learnEffects: new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Hold, Seconds = 0.8f, Cooldown = 1f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 0f, Stagger = 40f, Stacks = 0, Cooldown = 1f },
                });

            // The counter: the step form strikes through the enemy faced at two fangs' strength, and makes the next heavy blow
            // critical. NEEDS: the coil held up to 2.4 s through three fangs (held-press), and the let-go as a blow is about to
            // land passing it through you at three fangs (blow-about-to-land).
            SpellBranch.Add(to, E, CoiledStrike, "Coiled Strike", SpellRole.Counter, BranchPattern.LeftB, InTime, SpellForms.Step,
                new SpellSpec { Reach = 6f, Damage = 24f, Stagger = 25f, Stacks = 4, Seconds = 3f },
                "Coil for as long as you dare, up to 2.4 seconds, three fangs lighting over your head. Let go to strike through the enemy you face and stand behind it: 14, 24 or 38 damage and 2, 4 or 8 doses by the fangs lit. Let go as a blow is about to land and it passes through you, and the strike counts as three fangs.",
                UpgradeIcons.Arrow,
                // The roster's numbers alone (the owner's answer of 2026-11-20: an Empower may strengthen without a shorter wait).
                new Empower { Damage = 30f },
                "It strikes for 18, 30 or 46.",
                // NEEDS: a zigzag of short rushes through three at most, the fangs' doses shared between them (form-pieces). Until
                // then the drill form runs twelve metres through all on its line, each struck for one fang's strike and two doses.
                new ShapeDef { Name = "Centipede's Run", Form = SpellForms.Drill,
                    Description = "Let go and you strike through up to three enemies in a zigzag of short rushes, four metres each, a bite on each, the fangs' doses shared between them. You end behind the last. The snake strikes one, the centipede many.",
                    Spell = new SpellSpec { Reach = 12f, Radius = 1f, Count = 1, Damage = 14f, Stagger = 15f, Stacks = 2, Speed = 22f } },
                // NEEDS: the skin left only when let go inside the perfect window (blow-about-to-land). Until then every cast
                // leaves the enemies within three metres of where you stood unable to see you for a second and a half.
                new ShapeDef { Name = "Shed Skin",
                    Description = "Let go inside the perfect window and you leave your skin standing where you were, pale and papery, for a second and a half. The enemy goes on striking it while your strike lands from behind.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Condition, At = EffectAt.Self, Radius = 3f, State = new ConditionSpec { Id = ShedSkin, Seconds = 1.5f, Flags = ConditionFlags.SightShort, Sight = 0.5f } } } },
                "Opens two shapes of Coiled Strike, chosen at any rest fire: Centipede's Run, a zigzag through three, or Shed Skin, a skin left for the blow.",
                // NEEDS: the let-go inside the perfect window as the moment (held-press, blow-about-to-land). Until then a perfect
                // dodge ends Coiled Strike's wait, once in ten seconds.
                new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.ResetCooldown, Cooldown = 10f } },
                "Let go inside the perfect window, and Coiled Strike is ready again.");

            // Poison goes twelve deeper (not without limit) and does not fade (Afflictions.PoisonKeeps). Nothing closes Volatile
            // to one who holds this. It grows from Coiled Strike's Learn, whose line passes clear.
            to.Add(BranchPattern.Put(new Upgrade { Id = SlowDeath, Name = "Slow Death", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { CoiledStrike }, Icon = UpgradeIcons.Skull,
                Description = "Your weapon wounds far less. Your poison goes twelve doses deeper, and it does not fade while its bearer lives.",
                Stats = new[] { new StatMod(PlayerStat.WeaponDamage, -0.3f), new StatMod(PlayerStat.AfflictionDepth, 12f) } },
                BranchPattern.LeftCrown));

            // ---- The right bough: Contagion. The sickness spread and carried.
            // On the bite itself (UpgradeTrigger.PoisonBite): one dose leaps to an enemy within four metres.
            to.Add(BranchPattern.Put(new Upgrade { Id = Catching, Name = "Catching", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Venom }, Icon = UpgradeIcons.Chain,
                Description = "Each time poison bites, one dose leaps to the nearest enemy within reach.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PoisonBite, Do = UpgradeAction.Pass, Affliction = P, Reach = 4f, Stacks = 1 } } },
                BranchPattern.RightFoot));

            // Miasma, the base spell: cast by Poison's kit, so its Learn carries no form; its shapes add to the kit's cloud or
            // cast their own.
            SpellBranch.Add(to, E, Miasma, "Miasma", SpellRole.Opener, BranchPattern.RightA, Venom, "", null,
                "Poison's own spell, given with the element. Eight clouds over a disc four metres ahead hang for four seconds, creep after the nearest enemy and give what stands in them a dose a second. A stain is left where they lay. You stand in it unharmed.",
                UpgradeIcons.Drop,
                // NEEDS: a cloud three metres across (base-spell-marks). The base spell is cast by its kit, which takes only the
                // Empower's wait; a radius set here would also shrink the Censer's swing (empower-after-shape).
                new Empower { Cooldown = 6f },
                "Miasma waits 6 seconds, not 7, and its cloud is three metres across.",
                // NEEDS: the cloud set aside (shape-sets-aside), the sheet stopping at walls and a dodge in it costing an enemy a
                // dose (form-pieces, ground-states). Until then a row of mist eight metres long is laid from your feet beside the
                // cloud, dosing what stands in it each second.
                new ShapeDef { Name = "Low Mist",
                    Description = "The cloud is heavier than air. It pours out from your feet along the floor the way you face, a low sheet two and a half metres wide and eight long with a hard top edge, and lies five seconds, stopping at walls. Enemies in it are dosed once a second, and a dodge or roll in it costs them a dose. A lane where the plain cloud is a disc.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Hazard, At = EffectAt.Self, Shape = HazardShape.Line, Reach = 8f, Count = 6, Radius = 1.25f, Seconds = 5f, Interval = 1f, Damage = 0f, Stagger = 0f, Affliction = P, Stacks = 1 } } },
                // Swinging Censer, once a spell, folded in here. NEEDS: the smoke left hanging two seconds after each pass
                // (form-pieces). The censer is the discs form, one disc at three metres.
                new ShapeDef { Name = "Censer", Form = SpellForms.Discs,
                    Description = "The cloud is carried. A censer swings round you on its chain three metres out, once every 1.2 seconds, for eight seconds. Each pass deals 4 damage and a dose and leaves smoke hanging for two seconds. The fight comes with you.",
                    Spell = new SpellSpec { Count = 1, Radius = 3f, Seconds = 8f, Interval = 1.2f, Damage = 4f, Stagger = 4f, Stacks = 1 } },
                "Opens two shapes of Miasma, chosen at any rest fire: Low Mist, a sheet poured along the floor, or Censer, a cloud carried round you.",
                // The cast timed to a bite on any enemy within eight metres (UpgradeEffect.OnCastBeat): every enemy the cloud doses is
                // dosed once more each second for the cloud's four seconds. NEEDS: the cloud itself dosing twice a second (its own
                // interval halved, empower-after-shape), not a second dose carried on each enemy.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, OnCastBeat = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = ThickCloud, Seconds = 4f, Beat = 1f } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = ThickCloud, Do = UpgradeAction.Afflict, Affliction = P, Stacks = 1 },
                },
                "Cast it on the poison's beat, within a fifth of a second of a bite on any enemy near, and the cloud doses twice a second.");

            // NEEDS: the reaction itself held in the body and landed later at double strength (reaction-held, reaction-stats),
            // and the closing ring of mist drawn (looks). Until then each reaction poison is in holds a burst of its own on the
            // enemy for two and a half seconds, or until a heavy blow on it brings it on: 20 damage, 20 stagger, two doses.
            to.Add(BranchPattern.Put(new Upgrade { Id = TheDelayedTouch, Name = "The Delayed Touch", Element = E, Kind = UpgradeKind.Major, Lifts = E, Requires = new[] { Catching }, Icon = UpgradeIcons.Hourglass,
                Description = "Every reaction poison takes part in is held in the body: a thin ring of mist closes on it, and the reaction lands two and a half seconds later at double strength, dosing all it reaches two deep. A heavy blow on that enemy brings it on at once.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Needs = P, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = HeldInTheBody, Seconds = 2.5f, EndsWith = ConditionBlow.Heavy } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = HeldInTheBody, Do = UpgradeAction.Burst, Radius = 2.5f, Damage = 20f, Stagger = 20f, Affliction = P, Stacks = 2 },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = HeldInTheBody, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 2.5f, Damage = 20f, Stagger = 20f, Affliction = P, Stacks = 2 },
                } },
                BranchPattern.RightMiddle));

            // The movement spell: the drill form rushes through all before you, untouchable, dosing each two deep, and every enemy
            // passed is raised to the depth of the sickest of them (UpgradeAction.EvenDepth).
            SpellBranch.Add(to, E, ShareAlike, "Share Alike", SpellRole.Movement, BranchPattern.RightB, TheDelayedTouch, SpellForms.Drill,
                new SpellSpec { Reach = 7f, Radius = 1f, Count = 1, Damage = 10f, Stagger = 10f, Stacks = 2, Speed = 20f },
                "You rush seven metres through everything before you, untouchable: 10 damage and 10 stagger to each enemy passed, and then every one of them is raised to the depth of the sickest, two at least.",
                UpgradeIcons.Comet,
                // The roster's numbers alone (the owner's answer of 2026-11-20: an Empower may strengthen without a shorter wait).
                new Empower { Reach = 9f },
                "It rushes nine metres.",
                // The sickest one's depth goes onto the blade (UpgradeAction.CoatBlade): the next five weapon blows each raise what
                // they strike to it.
                new ShapeDef { Name = "Carried on the Blade",
                    Description = "You rush through as learned, but take the sickest one's depth onto your blade: your next five weapon blows each set what they strike to that depth. The rush loads the weapon.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.CoatBlade, Affliction = P, SetsDepth = true, Count = 5 } } },
                // NEEDS: the rush backward, away from the enemies (form-pieces). Until then you rush forward as learned, and the
                // tail lashes where you stood.
                new ShapeDef { Name = "Shed Tail",
                    Description = "You rush six metres backward, away. A thrashing tail is left where you stood for two seconds, lashing everything within two metres every half second for three doses. Away from them, and what follows you pays.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Hazard, At = EffectAt.Self, Shape = HazardShape.Patch, Radius = 2f, Seconds = 2f, Interval = 0.5f, Damage = 0f, Stagger = 0f, Affliction = P, Stacks = 3 } } },
                "Opens two shapes of Share Alike, chosen at any rest fire: Carried on the Blade, the sickness loaded on your weapon, or Shed Tail, a rush away that leaves a lashing tail.",
                // The rush timed to a bite on any enemy near (UpgradeEffect.OnCastBeat): each enemy passed takes its two doses again.
                new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, OnCastBeat = true, Do = UpgradeAction.Afflict, Affliction = P, Stacks = 2 } },
                "Rush on the poison's beat, within a fifth of a second of a bite on any enemy you pass, and the doses double.",
                learnEffects: new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.EvenDepth, Affliction = P, Stacks = 2 } });

            // NEEDS: no poison, cloud or mist harming you, the enemy's included (harmless-clouds), and the healing and the
            // second dose only while you stand in a cloud (in-own-ground). Until then you mend 2 a second whenever an enemy is
            // near, and every blow doses once more.
            to.Add(BranchPattern.Put(new Upgrade { Id = TastedEveryHerb, Name = "Tasted Every Herb", Element = E, Kind = UpgradeKind.Devotion, Requires = new[] { TheDelayedTouch }, Icon = UpgradeIcons.Leaf,
                Description = "All in on Poison only. No poison, cloud or mist harms you, yours or the enemy's. Standing in any cloud heals you 2 a second, and your blows dose twice while you stand in one. Where everyone else leaves the mist, you fight in it.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionStacks, 1f) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.EverySeconds, Every = 1f, Do = UpgradeAction.Heal, Amount = 2f } } },
                BranchPattern.RightCrown));

            // BESPOKE (built): CleanBladeForm; the beat is held through Afflictions.PoisonWaits.
            to.Add(BranchPattern.Put(new Upgrade { Id = CleanBlade, Name = "Clean Blade", Element = E, Kind = UpgradeKind.Signature, Icon = UpgradeIcons.Crown,
                Description = "While Fever lasts the poison waits, and every blow doses twice. When it ends you flick the blade clean, and every bite that was owed lands at once.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Afflict, WhileSurge = true, Affliction = AfflictionKind.Poison, Stacks = 1 }, new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.CleanBlade, Spell = new SpellSpec { Cost = 0f, Stagger = 20f, Reach = 14f } } } },
                BranchPattern.Gift));
        }
    }
}

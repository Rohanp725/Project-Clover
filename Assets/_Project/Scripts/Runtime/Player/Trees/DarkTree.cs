using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Dark: pay in health, take it back out of the enemy, rise once, written as spell branches
    /// (docs/design/steel/elements_b.md, Dark; Phase 4). Six spells of four marks each and ten passives on the pattern of
    /// <see cref="BranchPattern"/>: in the middle Hunger (Dread, Devouring Maw and Gone to Ash, the lift Collected Twice,
    /// then Never Sated and the Devotion Nothing Stays Buried at the top of the trunk), on the left Debt (Blood Price,
    /// Debts Called, Near the Grave, Pawned Heart, Dead Already at the crown), on the right Grave (Hollow Strength, Grasp and
    /// Second Shadow). Grasp is the base spell: it comes with Dark, its Learn is never bought, and it grows from the root.
    ///
    /// Marks the design keeps keep their ids; the cut ones are in <see cref="RetiredMarks"/> (The Dead Stand lives on in
    /// the Devotion; Unpaid, Feast, Glut and Borrowed Strength are gone). The Rise stays with the god's gift for now: it is
    /// written by hand in PlayerUpgrades and RiseForm, which ask for the gift (a NEEDS on the root). The marks are data.
    /// What the data cannot say yet is the nearest working version with a <c>NEEDS</c> line, gathered in
    /// docs/design/steel/content_open.tsv.
    /// </summary>
    public static class DarkTree
    {
        public const string Curse = "dark.curse";
        public const string BloodPrice = "dark.bloodprice";
        public const string NearTheGrave = "dark.nearthegrave";
        public const string HollowStrength = "dark.hollowstrength";
        public const string Dread = "dark.dread";
        public const string NeverSated = "dark.neversated";
        public const string DeadAlready = "dark.deadalready";
        public const string CollectedTwice = "dark.lift";
        public const string NothingStaysBuried = "dark.devotion";
        // The spells: each is its branch's Learn mark and the branch's id.
        public const string Grasp = "dark.base";
        public const string DebtsCalled = "dark.debtscalled";
        public const string DevouringMaw = "dark.maw";
        public const string GoneToAsh = "dark.ash";
        public const string SecondShadow = "dark.secondshadow";
        public const string PawnedHeart = "dark.pawnedheart";
        // The god's gift.
        public const string LifeForALife = "dark.lifeforalife";
        // Conditions the branches and passives set on enemies.
        public const string Answered = "dark.base.answered";
        public const string Tethered = "dark.base.tethered";
        public const string Swallowed = "dark.maw.swallowed";
        public const string Ashen = "dark.ash.ashen";
        public const string AshOnTheBrow = "dark.ash.brow";
        public const string HungHeart = "dark.pawnedheart.hung";
        public const string LostHeart = "dark.dread.lostheart";
        public const string Thumbprint = "dark.lift.print";

        // Retired in Phase 4 (RetiredMarks): kept as a name because code still asks for it.
        public const string SlowRot = "dark.slowrot";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Dark;
            const AfflictionKind C = AfflictionKind.Cursed;

            // BESPOKE (built): the Rise, in PlayerUpgrades.Rise and RiseForm, given by the root (the owner's decision of
            // 2026-11-20, rise-on-root); the gift, A Life for a Life, is the finisher that buys it back.
            to.Add(BranchPattern.Put(new Upgrade { Id = Curse, Name = "Curse", Element = E, Kind = UpgradeKind.Root, Icon = UpgradeIcons.Moon,
                Description = "Blows of the weapon curse an enemy. A Cursed enemy deals less, and killing it heals you. Once, the blow that would kill you does not: you rise, Risen, your health draining away.",
                WeaponStacks = 1 },
                BranchPattern.Root));

            // ---- The middle bough: Hunger. Feeding on the Cursed.
            // NEAREST: every enemy within eight metres buckles, not only the Cursed; the guard falls on the Cursed alone.
            to.Add(BranchPattern.Put(new Upgrade { Id = Dread, Name = "Dread", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Curse }, Icon = UpgradeIcons.Eye,
                Description = "When a Cursed enemy dies, the other Cursed near it lose heart: their balance buckles and their guard falls.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = C, Do = UpgradeAction.Burst, Damage = 0f, Stagger = 30f, Radius = 8f, Cooldown = 1f },
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = C, Do = UpgradeAction.Condition, Radius = 8f, LeapsTo = C, State = new ConditionSpec { Id = LostHeart, Seconds = 3f, Flags = ConditionFlags.CannotGuard }, Cooldown = 1f },
                } },
                BranchPattern.MiddleFoot));

            // The detonator: the dragon form lunges down a lane; on a Cursed enemy the bite spends the Curse for more and feeds
            // you. The jaw lays no Curse of its own, so that what it bites was Cursed before.
            SpellBranch.Add(to, E, DevouringMaw, "Devouring Maw", SpellRole.Detonator, BranchPattern.MiddleA, Dread, SpellForms.Dragon,
                new SpellSpec { Reach = 5f, Radius = 1.6f, Damage = 24f, Stagger = 30f, Stacks = 0, Speed = 14f },
                "A jaw twice your height rises out of your shadow and lunges five metres along a lane: 24 damage and 30 stagger. On a Cursed enemy it spends the Curse: 20 damage more, and you heal 12.",
                UpgradeIcons.Fang,
                // The heal of 18 is the Learn's own heal, set by the Empower (empower-effects).
                new Empower { Cooldown = 10f, Effects = new[] { new EffectChange { Index = 1, Amount = 18f } } },
                "Devouring Maw waits 10 seconds, not 11, and a Cursed enemy bitten heals you 18.",
                // Only an ordinary Cursed enemy is swallowed (Needs, asked before the Learn spends the Curse). NEEDS: the enemy
                // taken out of the fight unseen and rolled out of the throat (swallowed). Until then it is held still a second
                // and a half, then dragged to your feet and staggered.
                new ShapeDef { Name = "Rahu",
                    Description = "An ordinary Cursed enemy is swallowed whole, and a second and a half later rolls out of the open throat at your feet, staggered, because there is nothing to hold it. A god or an elite is only bitten.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Ranks = EnemyRank.Plain, Needs = C, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Swallowed, Seconds = 1.5f, Flags = ConditionFlags.Rooted | ConditionFlags.BeginsNoAttack | ConditionFlags.CannotGuard } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Swallowed, Do = UpgradeAction.Pull, Shape = HazardShape.Downwind, Amount = 5f },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = Swallowed, Do = UpgradeAction.Burst, Radius = 1f, Damage = 0f, Stagger = 60f, Stacks = 0 },
                    } },
                // NEEDS: the jaw drawn rising out of the enemy's own shadow, on the enemy faced rather than the nearest
                // (form-pieces). The pillars form, one pillar, closes from below on the nearest within nine metres.
                new ShapeDef { Name = "From Its Own Shadow", Form = SpellForms.Pillars,
                    Description = "The jaw rises out of the enemy's own shadow instead, up to nine metres away, and closes on it from below. No lane: it reaches the one far off.",
                    Spell = new SpellSpec { Count = 1, Reach = 9f, Radius = 1.6f, Delay = 0.4f, Damage = 24f, Stagger = 30f, Stacks = 0 } },
                "Opens two shapes of Devouring Maw, chosen at any rest fire: Rahu, a jaw that swallows an enemy and lets it fall out of the throat, or From Its Own Shadow, a jaw that rises under one far off.",
                // Only a bite on an enemy in its wind-up (UpgradeEffect.NeedsWindup): its attack swallowed and broken, and the wait ended.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, NeedsWindup = 99f, Do = UpgradeAction.Flinch, Seconds = 0.4f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, NeedsWindup = 99f, Do = UpgradeAction.ResetCooldown, Cooldown = 0.5f },
                },
                "Bite an enemy in its wind-up, and its attack is swallowed and broken, and Devouring Maw is ready again.",
                learnEffects: new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = C, Do = UpgradeAction.Burst, Radius = 0.6f, Damage = 20f, Stagger = 0f, Stacks = 0 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = C, Do = UpgradeAction.Heal, Amount = 12f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = C, Do = UpgradeAction.Spend, Affliction = C },
                });

            // The counter: the parry form turns the first blow and answers with a Curse; you step behind the attacker, and your
            // first blow on it within a second is struck as a heavy (UpgradeAction.NextHeavy) and heals; nothing struck, it waits
            // 5 s (SpellSpec.MissedWait). NEEDS: the scattering as ash and standing again 0.4 s later (form-pieces).
            SpellBranch.Add(to, E, GoneToAsh, "Gone to Ash", SpellRole.Counter, BranchPattern.MiddleB, Dread, SpellForms.Parry,
                new SpellSpec { Seconds = 1.2f, Reach = 2.5f, Arc = 90f, Damage = 0f, Stagger = 0f, Stacks = 1, MissedWait = 0.5f },
                "You stand with arms crossed as grey ash runs over you, for up to 1.2 seconds. The first blow to land scatters you as a cloud of ash, and you take nothing. A moment later you stand again behind the attacker. It is Cursed, and your first blow within a second is a heavy blow that heals you 10. If nothing strikes, it waits only half as long.",
                UpgradeIcons.Shield,
                // The heal of 15 is the Learn's own heal, set by the Empower (empower-effects).
                new Empower { Cooldown = 9f, Seconds = 1.5f, Effects = new[] { new EffectChange { Index = 3, Amount = 15f } } },
                "Gone to Ash waits 9 seconds, not 10, holds for a second and a half, and its blow heals you 15.",
                // Standing again where you were: the Learn's step behind set aside (ShapeDef.SetsAside). NEEDS: the ward kept for
                // that attacker's blow alone, and the grey stripe across its head (looks). The Curse held six seconds and one blow
                // turned are data.
                new ShapeDef { Name = "Ash on the Brow", SetsAside = new[] { 0 },
                    Description = "You stand again where you were. The ash settles on the attacker's brow as a grey smear: for six seconds it is Cursed, the next blow it lands on you scatters you for nothing once more, and your first blow on it is a heavy. The ash marks the one who struck.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = AshOnTheBrow, Seconds = 6f, Keeps = C } },
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Ward, Count = 1, Cooldown = 1f },
                    } },
                // Standing again where you were: the Learn's step behind set aside (ShapeDef.SetsAside). NEEDS: an attack begun
                // inside going where it already faced (cannot-turn). The cloud that Curses is data, laid on the attacker.
                new ShapeDef { Name = "Ash Hangs", SetsAside = new[] { 0 },
                    Description = "You stand again where you were. The ash stays as a sinking cloud three metres across for three seconds. Enemies inside it are Cursed, and an attack they begin inside goes where they already faced.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 1.5f, Seconds = 3f, Interval = 1f, Damage = 0f, Stagger = 0f, Affliction = C, Stacks = 1, Cooldown = 1f } } },
                "Opens two shapes of Gone to Ash, chosen at any rest fire: Ash on the Brow, a smear that marks the one who struck, or Ash Hangs, a cloud that Curses all inside.",
                // Only a red attack scattered (UpgradeEffect.RedOnly): the attacker staggered and the wait ended.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, RedOnly = true, Do = UpgradeAction.Burst, Radius = 1f, Damage = 0f, Stagger = 60f, Stacks = 0, Cooldown = 0.5f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, RedOnly = true, Do = UpgradeAction.ResetCooldown, Cooldown = 0.5f },
                },
                "Scatter a red attack, and the attacker is staggered and Gone to Ash is ready again.",
                learnEffects: new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Step, Reach = 4f, Amount = 1.5f, Cooldown = 0.5f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.NextHeavy, Seconds = 1f, Cooldown = 0.5f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Ashen, Seconds = 1f, EndsWith = ConditionBlow.Weapon } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Ashen, By = StruckBy.Weapon, Do = UpgradeAction.Heal, Amount = 10f },
                });

            // The print is laid on everything the reaction struck (UpgradeTrigger.ReactionStruck), keeping the damage its blow dealt
            // there (UpgradeEffect.ByPower); the next weapon blow on it repeats that damage.
            to.Add(BranchPattern.Put(new Upgrade { Id = CollectedTwice, Name = "Collected Twice", Element = E, Kind = UpgradeKind.Major, Lifts = E, Requires = new[] { Dread }, Icon = UpgradeIcons.Target,
                Description = "Every reaction the Curse takes part in leaves a black thumbprint on the brow of what it struck for four seconds. The next weapon blow on it repeats the reaction's damage. The debt is collected by the blade.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.ReactionStruck, ReactionWith = C, Do = UpgradeAction.Condition, ByPower = true, State = new ConditionSpec { Id = Thumbprint, Seconds = 4f, EndsWith = ConditionBlow.Weapon } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Thumbprint, By = StruckBy.Weapon, Do = UpgradeAction.Burst, ByPower = true, Radius = 0.6f, Stagger = 0f, Stacks = 0 },
                } },
                BranchPattern.MiddleCrown));

            // The game never lets the flasks fall below one, so one is left.
            to.Add(BranchPattern.Put(new Upgrade { Id = NeverSated, Name = "Never Sated", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { CollectedTwice }, Icon = UpgradeIcons.Flask,
                Description = "You carry no flasks. Every blow you land feeds you a little, and every kill and finisher a great deal.",
                Stats = new[] { new StatMod(PlayerStat.FlaskCount, -3f), new StatMod(PlayerStat.Leech, 0.06f) },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = C, Do = UpgradeAction.Heal, Amount = 12f },
                    new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Heal, Amount = 20f },
                } },
                BranchPattern.TrunkLeft));

            // The Dead Stand, once a spell, folded in here. NEEDS: three shades at most (helpers-most), each walking to its
            // own enemy (form-pieces), and the Risen's dead raised only when they were not Cursed (a Cursed
            // death while Risen raises two for now). The shade is a helper that stands where the dead fell and strikes what
            // is near it, a blow every 0.8 s for six seconds.
            to.Add(BranchPattern.Put(new Upgrade { Id = NothingStaysBuried, Name = "Nothing Stays Buried", Element = E, Kind = UpgradeKind.Devotion, Requires = new[] { CollectedTwice }, Icon = UpgradeIcons.Tower,
                Description = "All in on Dark only. Every enemy that dies Cursed stands up again as your shade, ash-grey with violet eyes, and fights for you for six seconds, three at most. While you are Risen, every enemy that dies stands up, Cursed or not.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = C, Do = UpgradeAction.Helper, Count = 7, Interval = 0.8f, Radius = 2.5f, Damage = 12f, Stagger = 12f, Affliction = C, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.Kill, While = StatWhen.Risen, Do = UpgradeAction.Helper, Count = 7, Interval = 0.8f, Radius = 2.5f, Damage = 12f, Stagger = 12f, Affliction = C, Stacks = 1 },
                } },
                BranchPattern.TrunkRight));

            // ---- The left bough: Debt. Power bought with health.
            // Re-aimed at cooldowns (elements_b.md): a spell still waiting is cast anyway for 1 health a second left, grey and
            // owed, never the last point (PlayerStat.CooldownPrice, read by the slots).
            to.Add(BranchPattern.Put(new Upgrade { Id = BloodPrice, Name = "Blood Price", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Curse }, Icon = UpgradeIcons.Drop,
                Description = "A spell still waiting can be cast anyway: each second left on its wait costs 1 health. Health paid this way is only owed: blows win it back. It never takes your last point.",
                Stats = new[] { new StatMod(PlayerStat.CooldownPrice, 1f) } },
                BranchPattern.LeftFoot));

            // The breaker: the reverse form drags all within six metres to a point before you and bursts, its damage rising as your
            // health falls, 18 at full and 36 at one point (SpellSpec.HealthScaled). NEEDS: the burst throwing back what it struck
            // (form-pieces).
            SpellBranch.Add(to, E, DebtsCalled, "Debts Called", SpellRole.Breaker, BranchPattern.LeftA, BloodPrice, SpellForms.Reverse,
                new SpellSpec { Reach = 1f, Radius = 6f, Delay = 0.5f, Damage = 18f, Stagger = 35f, Stacks = 1, HealthScaled = 2f },
                "Everything within six metres is dragged in to a point before you over half a second, then thrown back by a burst: 18 damage, rising to 36 at one point of your health, 35 stagger, and Cursed.",
                UpgradeIcons.Spiral,
                // Eight metres on the Learn's own pull alone (LearnOnly), so the buffalo's lane keeps its width.
                new Empower { Cooldown = 9f, Radius = 8f, LearnOnly = true },
                "Debts Called waits 9 seconds, not 10, and calls from eight metres.",
                // NEEDS: only the Cursed carried on the horns and thrown down at the far end (afflicted-only, carried-along), and
                // a buffalo of smoke rather than the dragon's head (form-pieces).
                new ShapeDef { Name = "The Black Buffalo", Form = SpellForms.Dragon,
                    Description = "No pull. A buffalo of sinking black smoke charges eight metres through the point before you, carrying everything Cursed on its horns to the far end and throwing it down: 18 damage, rising to 36 at one point of your health, and 35 stagger. Follow the lane and cut what it threw. Dark's one push.",
                    Spell = new SpellSpec { Reach = 8f, Radius = 1.2f, Damage = 18f, Stagger = 35f, Stacks = 1, Speed = 16f, HealthScaled = 2f } },
                // Only the Cursed called (SpellSpec.OnlyCarrying), from ten metres, with no burst; each Cursed one it reaches heals 4.
                new ShapeDef { Name = "Collected",
                    Description = "Only the Cursed are called, from up to ten metres, and each one that reaches you gives back 4 health. There is no burst; the pull does 20 stagger.",
                    Spell = new SpellSpec { Reach = 1f, Radius = 10f, Delay = 0.5f, Damage = 0f, Stagger = 20f, Stacks = 0, OnlyCarrying = C },
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = C, Do = UpgradeAction.Heal, Amount = 4f } } },
                "Opens two shapes of Debts Called, chosen at any rest fire: The Black Buffalo, a charge of smoke down a lane, or Collected, a call that brings only the Cursed and feeds you.",
                // Spells strike twice as hard for the second the burst or the charge takes, cast below a third of your health.
                new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, HealthBelow = 0.34f, Do = UpgradeAction.Boon, Stat = PlayerStat.SpellDamage, Amount = 1f, Seconds = 1f } },
                "Cast it below a third of your health, and the burst, or the buffalo, strikes twice as hard.");

            // The strength is a boon given when health falls low, not held for as long as it stays low; the shade comes then too, not with each kill.
            to.Add(BranchPattern.Put(new Upgrade { Id = NearTheGrave, Name = "Near the Grave", Element = E, Kind = UpgradeKind.Major, Requires = new[] { BloodPrice }, Icon = UpgradeIcons.Skull,
                Description = "Below a third of your health you strike harder, and each kill wraps you in a shade that takes the next blow.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, While = StatWhen.LowHealth, Do = UpgradeAction.Ward, Count = 1, Cooldown = 1f } }, Stats = new[] { new StatMod(PlayerStat.WeaponDamage, 0.2f, StatWhen.LowHealth) } },
                BranchPattern.LeftMiddle));

            // The larger spell, paid in health, not mana (steel/README.md): its table entry keeps a long wait. BESPOKE (built):
            // HeartForm takes the health itself; the price of one point only says that one who has nothing to pawn cannot cast it.
            SpellBranch.Add(to, E, PawnedHeart, "Pawned Heart", SpellRole.Breaker, BranchPattern.LeftB, NearTheGrave, SpellForms.Heart,
                new SpellSpec { Cost = 0f, HealthCost = 1f, Seconds = 10f, Damage = 60f, Count = 30, Radius = 3f, Stacks = 0 },
                "Your health drops to one and the rest is set down behind you as a beating heart in a ring that closes over ten seconds. While it beats your weapon deals 60% more and swings cost the least stamina they can. Walk at least 2.6 metres away, then back onto it, for all that was pawned and 30% of the damage dealt meanwhile. If the ring closes first, your health stays at one and half of what was pawned comes back grey.",
                UpgradeIcons.Heart,
                // The roster's numbers alone (the owner's answer of 2026-11-20: an Empower may strengthen without a shorter wait).
                new Empower { Damage = 70f, Seconds = 12f },
                "While the heart beats, your weapon deals 70% more, and the ring closes over twelve seconds.",
                // NEEDS: the heart sunk under where you stand, its strength only within three metres, rising whole when the ring
                // closes (heart-places). Until then the heart is set behind you as learned, and a ring of ash three metres
                // round where you cast Curses what stands in it.
                new ShapeDef { Name = "Buried Heart",
                    Description = "The heart sinks into the floor where you stand and beats under it, a dark pulse in a ring of ash. For ten seconds your weapon deals 60% more only while you stand within three metres of it; when the ring closes it rises whole, with 30% of the damage dealt. Stand your ground and cut.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Hazard, At = EffectAt.Self, Shape = HazardShape.Patch, Radius = 3f, Seconds = 10f, Interval = 1f, Damage = 0f, Stagger = 0f, Affliction = C, Stacks = 1 } } },
                // NEEDS: the heart hung over the enemy faced, following it, to be touched under it (heart-places). Until then the
                // heart is set behind you as learned, and the enemy faced keeps its Curse while the heart beats.
                new ShapeDef { Name = "Hung From the Tree",
                    Description = "The heart hangs in the air over the enemy you face instead, and follows it, as the vetala hangs from its tree. You must fight your way in under it to touch it. You go in, not out.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = HungHeart, Seconds = 10f, Keeps = C } } } },
                "Opens two shapes of Pawned Heart, chosen at any rest fire: Buried Heart, a heart under your feet to stand over, or Hung From the Tree, a heart over the enemy to fight your way to.",
                // The heart touched within a moment of a perfect dodge returns 30 more of each hundred dealt (PlayerStat.HeartTouchMore, read by HeartForm).
                System.Array.Empty<UpgradeEffect>(),
                "Touch the heart during a perfect dodge, and it returns 60% of the damage dealt, not 30%.",
                masterStats: new[] { new StatMod(PlayerStat.HeartTouchMore, 30f) });

            // Built in PlayerUpgrades: Risen for good (StatWhen.Risen always holds), two points of health a second while an enemy is within twelve metres, never the last point, and Rise() refuses.
            to.Add(BranchPattern.Put(new Upgrade { Id = DeadAlready, Name = "Dead Already", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { NearTheGrave }, Icon = UpgradeIcons.Skull,
                Description = "You do not wait to die. You are Risen from now on: in every fight your health drains to its last point, and dealing damage feeds you. You strike far harder. There is no second rising.",
                Stats = new[] { new StatMod(PlayerStat.WeaponDamage, 0.25f), new StatMod(PlayerStat.Leech, 0.3f) } },
                BranchPattern.LeftCrown));

            // ---- The right bough: Grave. The edge of death and past it.
            // The Rise is built (PlayerUpgrades.Rise, RiseForm), and "Risen" means it: StatWhen.Risen. Not flinching and the
            // drain stilled by a kill are done by hand in PlayerUpgrades, by this id.
            to.Add(BranchPattern.Put(new Upgrade { Id = HollowStrength, Name = "Hollow Strength", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Curse }, Icon = UpgradeIcons.Fist,
                Description = "Risen, you strike harder and no blow makes you flinch. Each kill stills the draining for a few breaths.",
                Stats = new[] { new StatMod(PlayerStat.WeaponDamage, 0.25f, StatWhen.Risen) } },
                BranchPattern.RightFoot));

            // Grasp, the base spell: cast by Dark's kit, so its Learn carries no form; its shapes add to the kit's ball or cast
            // their own.
            SpellBranch.Add(to, E, Grasp, "Grasp", SpellRole.Opener, BranchPattern.RightA, Curse, "", null,
                "Dark's own spell, given with the element. A black ball crawls along the ground to the enemy you face within six metres, drawing in what it passes. Where it arrives: 10 damage, 12 stagger and Cursed, and up to five bodies are dragged to your feet. What cannot be dragged drags you to it.",
                UpgradeIcons.Claw,
                // 14 damage and eight metres. The Empower's numbers reach the kit's own cast too (PlayerUpgrades.BaseEmpower, read by PlayerElement), and The Pit, cast through the mark,
                // takes them as its Learn's.
                new Empower { Cooldown = 6f, Damage = 14f, Reach = 8f },
                "Grasp waits 6 seconds, not 7, reaches eight metres and strikes for 14.",
                // Kept within five metres of you for its four seconds (ConditionSpec.Leash, LeashToPlayer).
                // NEEDS: a rope thrown in place of the ball (form-pieces).
                // The yank of the first heavy blow is data.
                new ShapeDef { Name = "Tethered",
                    Description = "A rope of black is thrown instead and binds the enemy you face to you for four seconds. It cannot go more than five metres from you, and your first heavy blow on it yanks it to your feet with 20 stagger. You keep it close.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Tethered, Seconds = 4f, EndsWith = ConditionBlow.Heavy, Leash = 5f, LeashToPlayer = true } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Tethered, By = StruckBy.Heavy, Do = UpgradeAction.Pull, Shape = HazardShape.Downwind, Amount = 4f },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Tethered, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 0.5f, Damage = 0f, Stagger = 20f, Stacks = 0 },
                    } },
                // NEEDS: everything in the pool drawn slowly to its middle (pull-to-point). The pool that Curses is the field form.
                new ShapeDef { Name = "The Pit", Form = SpellForms.Field,
                    Description = "The ball stops at the point you aim and spreads into a pool of ink three metres across for four seconds, dragging everything in it slowly to the middle and Cursing it. You gather them at a spot.",
                    Spell = new SpellSpec { Reach = 6f, Radius = 1.5f, Seconds = 4f, Interval = 1f, LingerDamage = 2f, Stacks = 1 } },
                "Opens two shapes of Grasp, chosen at any rest fire: Tethered, a rope that keeps one enemy close, or The Pit, a pool of ink that gathers them at a spot.",
                // NEEDS: Grasp's own cast as the answer (spell-windows). Until then any blow of yours on the one that struck you,
                // within a second, drags it to your feet and Curses it twice.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Struck, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Answered, Seconds = 1f, EndsWith = ConditionBlow.Any } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Answered, By = StruckBy.Any, Do = UpgradeAction.Pull, Shape = HazardShape.Downwind, Amount = 4f },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Answered, By = StruckBy.Any, Do = UpgradeAction.Afflict, Affliction = C, Stacks = 2 },
                },
                "Cast it within a second of being struck, and the one that struck you is dragged to your feet and Cursed twice. Dark answers when it is hurt.");

            // The movement spell: the clone form leaves your shadow to repeat your blows. NEEDS: the second press that changes
            // places with it, once (press-on-standing-thing).
            SpellBranch.Add(to, E, SecondShadow, "Second Shadow", SpellRole.Movement, BranchPattern.RightB, HollowStrength, SpellForms.Clone,
                new SpellSpec { Seconds = 8f, Damage = 50f, Stacks = 1 },
                "Leave your shadow where you stand: your shape in flat black with violet eyes, joined to you by a thin line on the ground. For eight seconds it repeats every blow you land a tenth of a second late on the nearest enemy within its reach, at half the damage, Cursing. Press again to change places with it, once.",
                UpgradeIcons.Moon,
                // 60% on the Learn's own shadow alone (LearnOnly), so Long Shadow's road keeps its own damage.
                new Empower { Cooldown = 10f, Damage = 60f, LearnOnly = true },
                "Second Shadow waits 10 seconds, not 12, and repeats your blows at 60%.",
                // NEEDS: the second press that sinks you into the shadow, and every wound taken since you left it given back as
                // grey health (press-on-standing-thing, wounds-returned). Until then you take a quarter less from every blow while it stands.
                new ShapeDef { Name = "Return to the Shadow",
                    Description = "The shadow fights as learned, but the second press does not swap. Within its eight seconds, press again to sink into it and rise where it lies, and every wound taken since you left it comes back as grey health to win back. A rewind paid in position.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Boon, Stat = PlayerStat.DamageTaken, Amount = -0.25f, Seconds = 8f } } },
                // NEEDS: sunk into the floor and unseen along the road (form-pieces). The drill form carries you eight metres and
                // Curses all it passes.
                new ShapeDef { Name = "Long Shadow", Form = SpellForms.Drill,
                    Description = "Your shadow is thrown out along the floor eight metres ahead, long as an evening shadow. You sink into its foot and rise at its head a moment later, and everything it lay across is Cursed. No copy: just the road.",
                    Spell = new SpellSpec { Reach = 8f, Radius = 1f, Count = 1, Damage = 2f, Stagger = 0f, Stacks = 1, Speed = 26f } },
                "Opens two shapes of Second Shadow, chosen at any rest fire: Return to the Shadow, a shadow to sink back into and mend, or Long Shadow, a road along the floor.",
                // NEEDS: changing places within a second of being struck as the moment, the blow passed to the shadow and its
                // health given back (press-on-standing-thing, spell-windows). Until then each cast turns the next blow that would land.
                new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Ward, Count = 1 } },
                "Change places within a second of being struck, and the blow passes to the shadow, and the health it took comes back to you.");

            // BESPOKE (built): the finisher that buys the Rise back, in RiseForm (the Rise itself is the root's, rise-on-root).
            to.Add(BranchPattern.Put(new Upgrade { Id = LifeForALife, Name = "A Life for a Life", Element = E, Kind = UpgradeKind.Signature, Icon = UpgradeIcons.Crown,
                Description = "When you are Risen a sigil marks the one that killed you: land a finisher on it and you live again, with half your health back, and may rise once more.",
                Effects = System.Array.Empty<UpgradeEffect>() },
                BranchPattern.Gift));
        }
    }
}

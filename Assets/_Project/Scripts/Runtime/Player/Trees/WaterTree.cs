using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Water: the return, written as spell branches (docs/design/steel/elements_a.md, Water; Phase 4). Six spells of four
    /// marks each and ten passives on the pattern of <see cref="BranchPattern"/>: in the middle Current (Ebb and Flood,
    /// Deep Crack and Blowhole, the lift Spreading Water and Devotion above it), on the left Tide (Undertow and The Tide
    /// Turns, Heavy Water and Whirlpool), on the right Vessel (Broken Gourd, Riptide, Drawn From the Drowned and Poured
    /// Blade, Deep Well at the crown). Riptide is the base spell: it comes with Water, its Learn is never bought, and it
    /// grows from the root.
    ///
    /// Marks the design keeps keep their ids; the cut ones are in <see cref="RetiredMarks"/> (River Serpent lives on as a
    /// shape of Riptide). Deep Well is done by hand in <see cref="PlayerUpgrades"/> under its id; the rest are data. What
    /// the data cannot say yet is the nearest working version with a <c>NEEDS</c> line, gathered in
    /// docs/design/steel/content_open.tsv.
    /// </summary>
    public static class WaterTree
    {
        public const string Root = "water.soak";
        public const string EbbAndFlood = "water.ebbflood";
        public const string Undertow = "water.undertow";
        public const string HeavyWater = "water.heavy";
        public const string BrokenGourd = "water.gourd";
        public const string DrawnFromTheDrowned = "water.drowned";
        public const string DeepWell = "water.well";
        public const string SpreadingWater = "water.lift";
        public const string TheSeaComesIn = "water.devotion";
        // The two keystones the owner picked on 2026-11-20 (keystones_fire_water_life.md).
        public const string HighWaterLowWater = "water.highlow";
        /// <summary>Kept as a mark by the owner's answer of 2026-11-20, under its old id.</summary>
        public const string SpilledWater = "water.spilled";
        public const string DrawnBackToBreak = "water.drawnback";
        // The spells: each is its branch's Learn mark and the branch's id.
        public const string Riptide = "water.base";
        public const string TheTideTurns = "water.tide";
        public const string Blowhole = "water.blowhole";
        public const string Whirlpool = "water.whirlpool";
        public const string DeepCrack = "water.crack";
        public const string PouredBlade = "water.poured";
        // The god's gift.
        public const string Gift = "water.sea";
        // Conditions the branches set on enemies.
        public const string GuardDown = "water.base.open";
        public const string ThrownDown = "water.base.down";
        public const string BacksTurned = "water.base.return";
        public const string KingTideDown = "water.tide.down";
        public const string ThrownOpen = "water.blowhole.open";
        public const string Churned = "water.whirlpool.churn";
        public const string SpoutRising = "water.whirlpool.spout";
        public const string InInk = "water.crack.ink";
        public const string InTheCurrent = "water.whirlpool.current";
        public const string Snapped = "water.crack.snap";

        // Retired in Phase 4 (RetiredMarks): kept as names because code still asks for them.
        public const string SpringTide = "water.spring";
        public const string LowWater = "water.lowwater";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Water;

            to.Add(BranchPattern.Put(new Upgrade { Id = Root, Name = "Soaked", Element = E, Kind = UpgradeKind.Root, Icon = UpgradeIcons.Drop,
                Description = "Blows leave an enemy Soaked. Spells strike it harder, and Static or Chill laid on it spreads to others who are Soaked.",
                WeaponStacks = 1 }, BranchPattern.Root));

            // ---- The middle bough: Current. The rhythm of guard and blow.
            // NEAREST: the Flood is more damage and one splash on the attacker (no longer reach, no splash on each blow); the Ebb is as designed.
            to.Add(BranchPattern.Put(new Upgrade { Id = EbbAndFlood, Name = "Ebb and Flood", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, Icon = UpgradeIcons.Wave,
                Description = "A perfect guard brings the Flood: a splash soaks the attacker and those beside it, and for five seconds your blows strike 15% harder. A heavy blow that lands brings the Ebb: for four seconds your guard and dodge cost half the stamina and the moment of your perfect guard is longer.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Boon, Stat = PlayerStat.WeaponDamage, Amount = 0.15f, Seconds = 5f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Burst, Damage = 6f, Stagger = 8f, Radius = 2.5f, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Boon, Stat = PlayerStat.StaminaCost, Amount = -0.5f, Seconds = 4f },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Boon, Stat = PlayerStat.PerfectGuardWindow, Amount = 0.22f, Seconds = 4f },
                } },
                BranchPattern.MiddleFoot));

            // The detonator: the bubble's crack spends the Soaked of the enemy it closes on, and every other Soaked enemy within
            // five metres cracks where it stands and its own Soaked is spent (UpgradeEffect.EachWithin).
            SpellBranch.Add(to, E, DeepCrack, "Deep Crack", SpellRole.Detonator, BranchPattern.MiddleA, EbbAndFlood, SpellForms.Pillars,
                new SpellSpec { Count = 1, Reach = 7f, Radius = 1f, Delay = 0.4f, Damage = 18f, Stagger = 40f, Stacks = 0 },
                "A fist of water snaps shut on the enemy faced, up to seven metres away. A bubble as big as a body forms on it and collapses with a crack: 18 damage and 40 stagger. On a Soaked enemy the water boils out of it, and every other Soaked enemy within five metres cracks with it, 18 damage and 30 stagger each.",
                UpgradeIcons.Burst,
                new Empower { Cooldown = 9f, Damage = 22f },
                "Deep Crack waits 9 seconds, not 11, and cracks for 22.",
                // Only what stands in the ink loses track of you, while it stands there (UpgradeEffect.Inside).
                new ShapeDef { Name = "Ink",
                    Description = "The bubble is black water from the deep. It bursts into a pool of ink three metres across that stays five seconds. Enemies in it lose track of you beyond three metres and stay Soaked. You stay in plain sight.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 1.5f, Seconds = 5f, Interval = 1f, Damage = 1f, Affliction = AfflictionKind.Soaked, Stacks = 1, Cooldown = 0.5f,
                            Inside = new ConditionSpec { Id = InInk, Seconds = 0.5f, Flags = ConditionFlags.SightShort, Sight = 3f } },
                    } },
                // NEEDS: three bubbles 2 m apart each cracking 0.15 s after the last. Until then one line of water runs seven
                // metres along the way you face at that pace and cracks on all it crosses.
                new ShapeDef { Name = "String of Bubbles", Form = SpellForms.Spikes,
                    Description = "Three bubbles in a row, two metres apart along the line you face, each cracking a moment after the last. Aim it down a corridor.",
                    Spell = new SpellSpec { Reach = 7f, Speed = 13f, Count = 1, Radius = 1f, Damage = 18f, Stagger = 40f, Stacks = 0 } },
                "Opens two shapes of Deep Crack, chosen at any rest fire: Ink, a pool that hides you, or String of Bubbles, three cracks down a corridor.",
                // Every Soaked enemy within twenty metres cracks where it stands (UpgradeEffect.EachWithin). NEEDS: the snap
                // timed to the heavy blow's landing, not the crack's (spell-windows); until then a heavy blow on the cracked enemy
                // within 0.3 s.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Snapped, Seconds = 0.3f, EndsWith = ConditionBlow.Heavy } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Snapped, By = StruckBy.Heavy, Do = UpgradeAction.Burst, EachWithin = 20f, OnlyCarrying = AfflictionKind.Soaked, Radius = 0.5f, Damage = 18f, Stagger = 30f, Stacks = 0 },
                },
                "Snap it as your heavy blow lands on the same enemy, and every Soaked enemy in the fight cracks.",
                learnEffects: new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = AfflictionKind.Soaked, Do = UpgradeAction.Burst, EachWithin = 5f, EachOther = true, OnlyCarrying = AfflictionKind.Soaked,
                        Radius = 0.5f, Damage = 18f, Stagger = 30f, Stacks = 0 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = AfflictionKind.Soaked, Do = UpgradeAction.Spend, EachWithin = 5f, EachOther = true, OnlyCarrying = AfflictionKind.Soaked,
                        Affliction = AfflictionKind.Soaked },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = AfflictionKind.Soaked, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Soaked },
                });

            // The parry form turns the first blow and answers at once, as a spout of three steps under the attacker that leaves
            // it open. NEEDS: the vent standing 3 s and turning every blow, red ones too (15 stamina each), each raising the
            // spray a step to five, the spout loosed on letting go under the nearest attacker for 14 a step, thrown up 0.8 s at
            // three steps or more and 1.2 s at five (held-press, count-rules).
            SpellBranch.Add(to, E, Blowhole, "Blowhole", SpellRole.Counter, BranchPattern.MiddleB, EbbAndFlood, SpellForms.Parry,
                new SpellSpec { Seconds = 3f, Reach = 3f, Arc = 140f, Damage = 42f, Stagger = 66f, Stacks = 1 },
                "You stand on a vent in black rock for up to three seconds. Each blow that comes, red ones too, is turned and drives the sea down the vent, and its spray rises a step, five at most. Let go, and the sea comes up as a spout under the nearest attacker: 14 damage and 22 stagger a step, and Soaked. At three steps or more it is thrown up, open to your blade.",
                UpgradeIcons.Tower,
                new Empower { Cooldown = 8f, Damage = 54f },
                "Blowhole waits 8 seconds, not 10, and its spout strikes 18 a step, not 14.",
                new ShapeDef { Name = "Still Water",
                    Description = "The first blow only. It is turned and answered at once with a thrust of water six metres straight through the attacker. A quick parry, not a store.",
                    Change = new SpellChange { Reach = 6f, Arc = 25f } },
                // NEEDS: the vent's store before the spout (as for the Learn). Until then the first blow turned brings the spout
                // up under you at once, all round, and throws everything near out.
                new ShapeDef { Name = "Blown Out",
                    Description = "The spout comes up under you instead and throws everything within four metres out three metres. Room, not a target.",
                    Change = new SpellChange { Reach = 4f, Arc = 360f },
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Push, At = EffectAt.Self, Radius = 4f, Amount = 3f, Cooldown = 0.5f } } },
                "Opens two shapes of Blowhole, chosen at any rest fire: Still Water, a quick parry, or Blown Out, a spout under you that makes room.",
                // NEEDS: letting go within 0.2 s of a blow landing on the vent (held-press, blow-about-to-land). As built every
                // answer comes in that instant, so every answer strikes again as hard and the vent comes back once in twenty seconds.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Burst, Damage = 42f, Stagger = 66f, Radius = 1.5f, Affliction = AfflictionKind.Soaked, Stacks = 1, Cooldown = 0.5f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.ResetCooldown, Cooldown = 20f },
                },
                "Let go in the same instant as a blow lands on the vent, and the spout is doubled and the vent stays for a second round.",
                learnEffects: new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = ThrownOpen, Seconds = 0.8f, Flags = ConditionFlags.BeginsNoAttack | ConditionFlags.CannotGuard } } });

            // Every reaction with Soaked in it runs on itself, at half strength, to each other Soaked enemy within four metres
            // (UpgradeAction.EchoReaction, done on each of them: UpgradeEffect.EachWithin).
            to.Add(BranchPattern.Put(new Upgrade { Id = SpreadingWater, Name = "Spreading Water", Element = E, Kind = UpgradeKind.Major, Lifts = E, Requires = new[] { EbbAndFlood }, Icon = UpgradeIcons.Drop,
                Description = "Every reaction with Soaked in it runs on at half strength to every other Soaked enemy within four metres.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Reacted, ReactionWith = AfflictionKind.Soaked, Do = UpgradeAction.EchoReaction, Amount = 0.5f,
                    EachWithin = 4f, EachOther = true, OnlyCarrying = AfflictionKind.Soaked } } },
                BranchPattern.MiddleCrown));
            // The Current bough's keystone (the owner's pick of 2026-11-20). The tide clock is kept by hand (PlayerUpgrades:
            // States turns it every six seconds and sets StatWhen.HighWater or LowWater, tide-clock); what each half does and
            // the perfect guard's and dodge's turn are data.
            to.Add(BranchPattern.Put(new Upgrade { Id = HighWaterLowWater, Name = "High Water, Low Water", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { SpreadingWater }, Icon = UpgradeIcons.Wave,
                Description = "Your tide turns every six seconds. At high water your spells hit a third harder and your weapon a fifth softer. At low water it is the other way round. A perfect guard or a perfect dodge turns the tide at once.",
                Stats = new[]
                {
                    new StatMod(PlayerStat.SpellDamage, 0.33f, StatWhen.HighWater), new StatMod(PlayerStat.WeaponDamage, -0.2f, StatWhen.HighWater),
                    new StatMod(PlayerStat.WeaponDamage, 0.33f, StatWhen.LowWater), new StatMod(PlayerStat.SpellDamage, -0.2f, StatWhen.LowWater),
                },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.TurnTide, Cooldown = 0.3f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.TurnTide, Cooldown = 0.3f },
                } },
                BranchPattern.TrunkLeft));
            // Kept as a mark (the owner's answer of 2026-11-20), at the top of the trunk, the one place left on this tree.
            to.Add(BranchPattern.Put(new Upgrade { Id = SpilledWater, Name = "Spilled Water", Element = E, Kind = UpgradeKind.Major, Requires = new[] { SpreadingWater }, Icon = UpgradeIcons.Ring,
                Description = "When you drink a flask, a ring of water bursts out from you. It soaks everyone near and pushes them back.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.FlaskDrunk, Do = UpgradeAction.Burst, At = EffectAt.Self, Damage = 1f, Stagger = 15f, Radius = 3.5f, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.FlaskDrunk, Do = UpgradeAction.Push, At = EffectAt.Self, Radius = 3.5f, Amount = 2.5f },
                } },
                BranchPattern.TrunkTop));
            // NEEDS: the pools four at most, enemies in them 15% slower, and the dodge and the clocks quicker only while you
            // stand in your own water (in-own-water, ground-states). Until then every spell that strikes leaves
            // a soaking pool where it lands, and the dodge and the clocks are quicker everywhere.
            to.Add(BranchPattern.Put(new Upgrade { Id = TheSeaComesIn, Name = "The Sea Comes In", Element = E, Kind = UpgradeKind.Devotion, Requires = new[] { SpreadingWater }, Icon = UpgradeIcons.Moon,
                Description = "All in on Water only. Every spell leaves standing water where it lands, a pool four metres across, for ten seconds, four at most. Enemies in water are always Soaked and slower. In water your dodge slides half again as far and your cooldowns run 30% faster. Draw the fight onto your own water and stay there.",
                Stats = new[] { new StatMod(PlayerStat.DodgeDistance, 0.5f), new StatMod(PlayerStat.SpellCooldown, 0.3f) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 2f, Seconds = 10f, Interval = 1f, Damage = 1f, Affliction = AfflictionKind.Soaked, Stacks = 1, Cooldown = 2.5f } } },
                BranchPattern.TrunkRight));

            // ---- The left bough: Tide. Pulling and placing.
            // NEAREST: the pull takes everyone near, Soaked or not.
            to.Add(BranchPattern.Put(new Upgrade { Id = Undertow, Name = "Undertow", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, Icon = UpgradeIcons.Arrow,
                Description = "A heavy blow drags every enemy near a step toward you.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Pull, At = EffectAt.Self, Radius = 6f, Amount = 1.5f, Cooldown = 0.5f } } },
                BranchPattern.LeftFoot));

            // The ebb first (SpellSpec.Ebb): drawn 5 m straight back in a moment, untouchable. The flood is the second press within
            // 3 s (UpgradeAction.Flood): the drill carries you back along the line and 3 m past where you began, and nothing can
            // touch you while it runs. Pressed once, the ebb alone. (The ebb's wet sand as ground: ground-states, in Cast.)
            var tideTurns = SpellBranch.Add(to, E, TheTideTurns, "The Tide Turns", SpellRole.Movement, BranchPattern.LeftA, Undertow, SpellForms.Drill,
                new SpellSpec { Reach = 8f, Radius = 1f, Count = 1, Damage = 10f, Stagger = 10f, Stacks = 1, Speed = 20f, Ebb = 5f },
                "Press, and the ebb draws you five metres straight back in a moment, out of reach, and nothing can touch you. It leaves a line of wet sand where you went. Press again within three seconds, and the flood brings you back along the line and three metres past where you began as a breaking wave. Everything on the line takes 10 damage, is Soaked and is carried two metres ahead of you.",
                UpgradeIcons.Boot,
                new Empower { Cooldown = 6f, Reach = 10f },
                "The Tide Turns waits 6 seconds, not 8, and its flood runs five metres past.",
                // The Empower's longer flood does not reach past this shape's own (EmpowerOverShape.KeepsOwn); its shorter wait does.
                new ShapeDef { Name = "King Tide", Empowered = EmpowerOverShape.KeepsOwn,
                    Description = "The flood comes in four metres wide and throws what it carries down for a moment, but it stops where you began. For a crowd.",
                    Change = new SpellChange { Radius = 2f, Reach = 5f },
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = KingTideDown, Seconds = 0.6f, Flags = ConditionFlags.Down } } } },
                // The ebb draws you the 5 m back (its own Ebb), and the enemy faced is drawn after you to your feet, Soaked. There is
                // no flood: its one press does nothing (a press with no effects is no press, and the slot casts again).
                new ShapeDef { Name = "Out to Sea", Form = SpellForms.Nova, Presses = new[] { new UpgradeEffect[0] },
                    Description = "There is no flood. The ebb takes the enemy faced with you: it is drawn the five metres out with you, Soaked, and dropped at your feet, away from its friends. A pull, not a return.",
                    Spell = new SpellSpec { Radius = 1f, Damage = 0f, Stagger = 0f, Stacks = 0, Ebb = 5f },
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Pull, Shape = HazardShape.Downwind, Amount = 5f },
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                    } },
                "Opens two shapes of The Tide Turns, chosen at any rest fire: King Tide, a wide flood that throws a crowd down, or Out to Sea, an enemy drawn away with you.",
                // Started as a blow would land (UpgradeEffect.BlowLanding), it is a perfect dodge (UpgradeAction.PerfectDodgeNow); the flood
                // loosed as a blow would land (the SpellLoosed moment) ends the wait its first press started.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, BlowLanding = 0.3f, Do = UpgradeAction.PerfectDodgeNow },
                    new UpgradeEffect { On = UpgradeTrigger.SpellLoosed, OwnBranch = true, BlowLanding = 0.3f, Do = UpgradeAction.ResetCooldown },
                },
                "Started in the instant a blow would land on you, it counts as a perfect dodge, and a flood pressed in the instant a blow would land starts no cooldown.",
                learnEffects: new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Push, Shape = HazardShape.Downwind, Amount = 2f } });
            // The flood is the second press, within three seconds of the ebb.
            tideTurns.PressWindow = 3f;
            tideTurns.Presses = new[] { new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, Do = UpgradeAction.Flood } } };

            // NEAREST: an enemy's recovery cannot be changed, so blows on a Soaked enemy break its balance sooner for a while.
            to.Add(BranchPattern.Put(new Upgrade { Id = HeavyWater, Name = "Heavy Water", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Undertow }, Icon = UpgradeIcons.Hammer,
                Description = "Striking a Soaked enemy makes your weapon break balance a fifth sooner for three seconds.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictedHit, Do = UpgradeAction.Boon, Needs = AfflictionKind.Soaked, Stat = PlayerStat.StaggerDealt, Amount = 0.2f, Seconds = 3f, Cooldown = 0.5f } } },
                BranchPattern.LeftMiddle));

            // The Tide bough's keystone (the owner's pick of 2026-11-20): each dodge away from the enemy faced draws a wave
            // (dodge-direction, a count of three), the next weapon blow that lands breaks them all on it as one cone that
            // throws, and with none drawn your blows are a fifth softer (the count's empty state, while-count).
            to.Add(BranchPattern.Put(new Upgrade { Id = DrawnBackToBreak, Name = "Drawn Back to Break", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { HeavyWater }, Icon = UpgradeIcons.Drop,
                Description = "Each time you dodge away from the enemy you face, the sea comes back with you as a wave, three at most. Your next weapon blow that lands breaks all of them on it: for each wave, 12 damage and 20 stagger in front of you, out to 4 metres. Everything hit is Soaked and thrown 2 metres back. With no wave held, your blows hit a fifth softer.",
                Tally = new TallySpec { Name = "Waves", Cap = 3, EmptyStat = PlayerStat.WeaponDamage, EmptyAmount = -0.2f },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.DodgeEnd, Way = DodgeWay.Away, Do = UpgradeAction.Bank, Count = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, SpendsAll = true, PerStack = true, Do = UpgradeAction.Cone, Reach = 4f, Arc = 120f, Damage = 12f, Stagger = 20f,
                        Affliction = AfflictionKind.Soaked, Stacks = 1, Amount = 2f },
                } },
                BranchPattern.LeftCrown));

            // The field form bites and soaks; each bite breaks balance harder for a moment and holds it there (ConditionFlags.
            // BalanceHeld, kept up by each bite, so it holds while they are inside); everything in it is drawn to the middle at
            // 1.2 m a second (SpellSpec.Pull).
            SpellBranch.Add(to, E, Whirlpool, "Whirlpool", SpellRole.Breaker, BranchPattern.LeftB, HeavyWater, SpellForms.Field,
                new SpellSpec { Reach = 6f, Radius = 4f, Seconds = 5f, Interval = 0.5f, LingerDamage = 3f, Stacks = 1, Pull = 1.2f },
                "A whirlpool four metres round opens under the enemy faced, up to six metres away, for five seconds. Enemies in it are drawn to the middle, kept Soaked and bitten for 3 twice a second. Their balance does not recover inside it, and your blows there break balance 30% harder.",
                UpgradeIcons.Spiral,
                new Empower { Cooldown = 8f, Seconds = 7f },
                "Whirlpool waits 8 seconds, not 10, and lasts seven seconds.",
                // The hard pull, 3 m a second (SpellChange.Pull). The throw at its end is the end of each enemy's churn: when the
                // bites stop.
                new ShapeDef { Name = "Maelstrom",
                    Description = "Smaller, two and a half metres round, and pulling hard. At its end it throws everything inside up in a spout and down again for 50 stagger.",
                    Change = new SpellChange { Radius = 2.5f, Pull = 3f },
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = SpoutRising, Seconds = 0.7f } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = SpoutRising, Do = UpgradeAction.Lift, Seconds = 0.8f },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = SpoutRising, Do = UpgradeAction.Burst, Damage = 0f, Stagger = 50f, Radius = 0.6f },
                    } },
                // The lane of foam, 2 m wide and 9 m long from the point away from you for 6 s, carries all in it down it at 2.5 m a
                // second (UpgradeEffect.Carry), and blows on one in it break balance 30% harder (UpgradeEffect.Inside); the
                // whirlpool draws nothing in (SpellChange.NoPull). NEEDS: no dodging against it (no-dodge-or-leap).
                new ShapeDef { Name = "Rip Current",
                    Description = "No pull to the middle. A lane of churning foam two metres wide and nine long runs from the whirlpool's point away from you for six seconds. Enemies in it are carried down it and cannot dodge against it, and your blows on a carried enemy break balance 30% harder. Cut them as the current drags them past.",
                    Change = new SpellChange { NoPull = true },
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Hazard, Shape = HazardShape.Downwind, Count = 5, Reach = 4.5f, Amount = 2f,
                        Radius = 1f, Seconds = 6f, Interval = 0.25f, Damage = 0f, Stacks = 0, Affliction = AfflictionKind.Soaked, Carry = 2.5f,
                        Inside = new ConditionSpec { Id = InTheCurrent, Seconds = 0.25f, Flags = ConditionFlags.StaggerTakenShare, Scale = 1.3f } } } },
                "Opens two shapes of Whirlpool, chosen at any rest fire: Maelstrom, a hard pull that ends in a spout, or Rip Current, a lane that carries them past you.",
                // In the instant its enemy's attack mark shows (UpgradeEffect.NeedsWindup, 0.3 s), the attack is lost and its own
                // cooldown halved (UpgradeAction.KeepWaitShare).
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, NeedsWindup = 0.3f, Do = UpgradeAction.Flinch, Seconds = 0.4f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, NeedsWindup = 0.3f, Do = UpgradeAction.KeepWaitShare, Amount = 0.5f, Cooldown = 0.5f },
                },
                "Open it under an enemy in the instant its attack mark shows, and the attack is lost as it goes in to the knees. Its cooldown is halved.",
                learnEffects: new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Churned, Seconds = 0.7f, Flags = ConditionFlags.StaggerTakenShare | ConditionFlags.BalanceHeld, Scale = 1.3f } } });

            // ---- The right bough: Vessel. The flask and the copy.
            // NEAREST: the pool soaks; it cannot mend the player who stands in it, so the throw mends a part at once.
            to.Add(BranchPattern.Put(new Upgrade { Id = BrokenGourd, Name = "Broken Gourd", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, Icon = UpgradeIcons.Flask,
                Description = "Hold the flask button to throw it. Where it breaks it strikes and soaks everyone near and leaves a pool that keeps what stands in it Soaked for six seconds. The throw mends you 20 at once.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.FlaskThrown, Do = UpgradeAction.Burst, Damage = 18f, Stagger = 20f, Radius = 2.6f, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.FlaskThrown, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 1.5f, Seconds = 6f, Interval = 1f, Damage = 1f, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.FlaskThrown, Do = UpgradeAction.Heal, Amount = 20f },
                } },
                BranchPattern.RightFoot));

            // Riptide, the base spell: cast by Water's kit, so its Learn carries no form; its shapes cast their own.
            SpellBranch.Add(to, E, Riptide, "Riptide", SpellRole.Opener, BranchPattern.RightA, Root, "", null,
                "Water's own spell, given with the element. A wave rises five metres ahead and rolls back toward you. Everything in its path takes 8 damage and 18 stagger, is Soaked, and is carried to your feet, within reach.",
                UpgradeIcons.Wave,
                // Rising 6.5 m away. The Empower's numbers reach the kit's own cast too (PlayerUpgrades.BaseEmpower, read by PlayerElement).
                new Empower { Cooldown = 4.5f, Reach = 6.5f },
                "Riptide waits 4.5 seconds, not 6, and rises six and a half metres away.",
                // River Serpent, once a spell, folded in: the serpent form goes out along its lane and back, and carries the first
                // enemy it bites to 1.5 m in front of you (SerpentForm). Its guard is down 1.5 s from the bite, a second after it is
                // set down.
                new ShapeDef { Name = "River Serpent", Form = SpellForms.Serpent,
                    Description = "A serpent of water goes eight metres out along a lane and back, and brings only the first enemy it takes, dropping it in front of you with its guard down for a second. One target, from further.",
                    Spell = new SpellSpec { Reach = 8f, Radius = 1.2f, Damage = 12f, Stagger = 30f, Stacks = 1, Speed = 16f },
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = GuardDown, Seconds = 1.5f, Flags = ConditionFlags.CannotGuard }, Cooldown = 0.5f },
                    } },
                new ShapeDef { Name = "Breaking Swell", Form = SpellForms.Wave,
                    Description = "The wave does not come back. It rolls six metres away from you and breaks, throwing everything in it down three metres off. The shape that makes room.",
                    Spell = new SpellSpec { Reach = 6f, Arc = 90f, Damage = 8f, Stagger = 18f, Stacks = 1 },
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Push, Shape = HazardShape.Downwind, Amount = 3f },
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = ThrownDown, Seconds = 0.6f, Flags = ConditionFlags.Down } },
                    } },
                "Opens two shapes of Riptide, chosen at any rest fire: River Serpent, one enemy brought from further, or Breaking Swell, a wave that rolls away and makes room.",
                // A press the Master adds (its Presses, found by PlayerUpgrades.PressesOf when the Master is in force): pressed again
                // within a second of the wave breaking at your feet (After WaveIn, noted by the kit's wave), what it brought is carried
                // on past you four metres (UpgradeAction.CarryOn), its back to you a moment: it begins nothing, and your first blow on
                // it is critical.
                null,
                "Press again within a second of the wave reaching you, and it goes out again the way it came, carrying what it brought past you and on four metres, backs turned.");
            var riptideMaster = to[to.Count - 1];
            riptideMaster.PressWindow = 2f;
            riptideMaster.Presses = new[]
            {
                new[] { new UpgradeEffect { On = UpgradeTrigger.SpellCast, After = CastMoment.WaveIn, AfterWindow = 1f, Do = UpgradeAction.CarryOn, Reach = 4f,
                    State = new ConditionSpec { Id = BacksTurned, Seconds = 1.2f, Flags = ConditionFlags.BeginsNoAttack | ConditionFlags.TakesCritical, EndsWith = ConditionBlow.Weapon } } },
            };

            // NEEDS: the line of foam seen running over the floor to you.
            to.Add(BranchPattern.Put(new Upgrade { Id = DrawnFromTheDrowned, Name = "Drawn From the Drowned", Element = E, Kind = UpgradeKind.Major, Requires = new[] { BrokenGourd }, Icon = UpgradeIcons.Flask,
                Description = "When a Soaked enemy dies its water runs over the floor to you as a line of foam and fills a fifth of an empty flask. Soak, then kill.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Soaked, Do = UpgradeAction.FlaskFill, Amount = 0.2f } } },
                BranchPattern.RightMiddle));

            // The clone form repeats each blow a beat late. NEEDS: the copy's blows carrying all of the blow's stagger (it
            // carries none; form-pieces).
            SpellBranch.Add(to, E, PouredBlade, "Poured Blade", SpellRole.Breaker, BranchPattern.RightB, DrawnFromTheDrowned, SpellForms.Clone,
                new SpellSpec { Cost = 30f, Seconds = 12f, Damage = 30f, Stacks = 1 },
                "A copy of you in water stands at your side for twelve seconds and repeats each blow a beat late, for 30% of its damage and all of its stagger, and Soaked. 30 mana.",
                UpgradeIcons.Blade,
                new Empower { Mana = 25f, Damage = 35f, Seconds = 15f },
                "Poured Blade costs 25 mana, not 30, and its copy strikes for 35% for fifteen seconds.",
                // Its first design: a flask is its price, not mana (SpellSpec.FlaskCost), and for its 12 s (UpgradeEffect.WithinOwnCast)
                // the weapon reaches 40% further (PlayerStat.WeaponReach), every blow mends 2 and soaks, and a chain's last blow throws a wave.
                new ShapeDef { Name = "Poured on the Edge", Form = SpellForms.Nova,
                    Description = "No copy. A flask is poured along the weapon, and for twelve seconds its reach is 40% longer, every blow mends 2 and soaks, and the last blow of a chain throws a wave five metres. It costs a flask, not mana.",
                    Spell = new SpellSpec { Radius = 1f, Damage = 0f, Stagger = 0f, Stacks = 0, FlaskCost = 1 },
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Boon, Stat = PlayerStat.WeaponReach, Amount = 0.4f, Seconds = 12f },
                        new UpgradeEffect { On = UpgradeTrigger.AnyHit, WithinOwnCast = 12f, Do = UpgradeAction.Heal, Amount = 2f },
                        new UpgradeEffect { On = UpgradeTrigger.AnyHit, WithinOwnCast = 12f, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                        new UpgradeEffect { On = UpgradeTrigger.ChainEnd, WithinOwnCast = 12f, Do = UpgradeAction.Cone, Reach = 5f, Arc = 60f, Damage = 10f, Stagger = 15f, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                    } },
                // NEEDS: the copy standing behind you, facing the other way, and striking what is behind you.
                new ShapeDef { Name = "Brother at Your Back",
                    Description = "The copy stands behind you facing the other way and repeats your blows on whatever is behind you. For being surrounded." },
                "Opens two shapes of Poured Blade, chosen at any rest fire: Poured on the Edge, a flask poured along the weapon, or Brother at Your Back, a copy that guards your back.",
                // NEEDS: only while the copy stands, the answer aimed at the attacker (while-spell-stands). Until then a perfect
                // dodge leaves a copy in the blow you left that strikes again at full strength, once in two seconds.
                new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Echo, Amount = 1f, Delay = 0.3f, Radius = 2.3f, Stagger = 30f, Cooldown = 2f } },
                "Make a perfect dodge while it stands, and the copy steps into the blow you left and answers the attacker at full strength.");

            // Built by hand (PlayerUpgrades.OnFlaskDrunk and Update): a flask adds 70 to a well of 140 at most, kept as the mark's count;
            // the well pays 6 health a second while a blow has landed in the last three seconds and health is not full.
            to.Add(BranchPattern.Put(new Upgrade { Id = DeepWell, Name = "Deep Well", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { DrawnFromTheDrowned }, Icon = UpgradeIcons.Heart,
                Description = "You carry one more flask. A flask no longer mends you at once: it fills a well that mends you only while you are landing blows.",
                Tally = new TallySpec { Name = "Well", Cap = 140 },
                Stats = new[] { new StatMod(PlayerStat.FlaskCount, 1f) } },
                BranchPattern.RightCrown));

            // BESPOKE (built): SeaForm. The surge button is pressed again, not held.
            to.Add(BranchPattern.Put(new Upgrade { Id = Gift, Name = "The Sea Rises", Element = E, Kind = UpgradeKind.Signature, Icon = UpgradeIcons.Crown,
                Description = "While Wellspring lasts, press the surge button again to flood the ground. One flask. For six seconds everything in the flood is Soaked and your blows run out across the water as waves. Then the sea draws back to you and breaks where you face.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.Sea, Spell = new SpellSpec { Cost = 0f, Radius = 9f, Seconds = 6f, Damage = 55f, Stagger = 90f, Reach = 9f, Arc = 120f, Count = 40, Stacks = 1 } } } },
                BranchPattern.Gift));
        }
    }
}

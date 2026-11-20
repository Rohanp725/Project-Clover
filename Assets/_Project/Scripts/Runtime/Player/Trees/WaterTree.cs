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

            // The detonator: the bubble's crack spends the Soaked of the enemy it closes on and runs to every other Soaked enemy
            // near. NEEDS: each of those cracking where it stands and its own Soaked spent (spend-soaked); until then an arc
            // runs from the first to each Soaked enemy within five metres and leaves their Soaked on them.
            SpellBranch.Add(to, E, DeepCrack, "Deep Crack", SpellRole.Detonator, BranchPattern.MiddleA, EbbAndFlood, SpellForms.Pillars,
                new SpellSpec { Count = 1, Reach = 7f, Radius = 1f, Delay = 0.4f, Damage = 18f, Stagger = 40f, Stacks = 0 },
                "A fist of water snaps shut on the enemy faced, up to seven metres away. A bubble as big as a body forms on it and collapses with a crack: 18 damage and 40 stagger. On a Soaked enemy the water boils out of it, and every other Soaked enemy within five metres cracks with it, 18 damage and 30 stagger each.",
                UpgradeIcons.Burst,
                new Empower { Cooldown = 9f, Damage = 22f },
                "Deep Crack waits 9 seconds, not 11, and cracks for 22.",
                // NEEDS: only what stands in the ink losing track of you while it stands there (ground-states). Until then what
                // the ink first reaches loses track of you for its five seconds.
                new ShapeDef { Name = "Ink",
                    Description = "The bubble is black water from the deep. It bursts into a pool of ink three metres across that stays five seconds. Enemies in it lose track of you beyond three metres and stay Soaked. You stay in plain sight.",
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 1.5f, Seconds = 5f, Interval = 1f, Damage = 1f, Affliction = AfflictionKind.Soaked, Stacks = 1, Cooldown = 0.5f },
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, Radius = 1.5f, State = new ConditionSpec { Id = InInk, Seconds = 5f, Flags = ConditionFlags.SightShort, Sight = 3f } },
                    } },
                // NEEDS: three bubbles 2 m apart each cracking 0.15 s after the last. Until then one line of water runs seven
                // metres along the way you face at that pace and cracks on all it crosses.
                new ShapeDef { Name = "String of Bubbles", Form = SpellForms.Spikes,
                    Description = "Three bubbles in a row, two metres apart along the line you face, each cracking a moment after the last. Aim it down a corridor.",
                    Spell = new SpellSpec { Reach = 7f, Speed = 13f, Count = 1, Radius = 1f, Damage = 18f, Stagger = 40f, Stacks = 0 } },
                "Opens two shapes of Deep Crack, chosen at any rest fire: Ink, a pool that hides you, or String of Bubbles, three cracks down a corridor.",
                // NEEDS: the snap timed to the heavy blow's landing, not the crack's (spell-windows), and every Soaked enemy in
                // the fight cracking where it stands. Until then a heavy blow on the cracked enemy within 0.3 s runs the crack on
                // to every Soaked enemy within twenty metres.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Snapped, Seconds = 0.3f, EndsWith = ConditionBlow.Heavy } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Snapped, By = StruckBy.Heavy, Do = UpgradeAction.Arc, LeapsTo = AfflictionKind.Soaked, Count = 12, Reach = 20f, Damage = 18f, Stagger = 30f, Stacks = 0 },
                },
                "Snap it as your heavy blow lands on the same enemy, and every Soaked enemy in the fight cracks.",
                learnEffects: new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Needs = AfflictionKind.Soaked, Do = UpgradeAction.Arc, LeapsTo = AfflictionKind.Soaked, Count = 6, Reach = 5f, Damage = 18f, Stagger = 30f, Stacks = 0 },
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
                // NEEDS: letting go within 0.2 s of a blow landing on the vent (held-press, their-blow-moment). As built every
                // answer comes in that instant, so every answer strikes again as hard and the vent comes back once in twenty seconds.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Burst, Damage = 42f, Stagger = 66f, Radius = 1.5f, Affliction = AfflictionKind.Soaked, Stacks = 1, Cooldown = 0.5f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.ResetCooldown, Cooldown = 20f },
                },
                "Let go in the same instant as a blow lands on the vent, and the spout is doubled and the vent stays for a second round.",
                learnEffects: new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = ThrownOpen, Seconds = 0.8f, Flags = ConditionFlags.BeginsNoAttack | ConditionFlags.CannotGuard } } });

            // NEEDS: every reaction with Soaked in it running on itself, at half strength, to the other Soaked enemies (a
            // reaction struck again elsewhere, reaction-stats). Until then an arc of its water runs from where it happened to
            // each Soaked enemy within four metres.
            to.Add(BranchPattern.Put(new Upgrade { Id = SpreadingWater, Name = "Spreading Water", Element = E, Kind = UpgradeKind.Major, Lifts = E, Requires = new[] { EbbAndFlood }, Icon = UpgradeIcons.Drop,
                Description = "Every reaction with Soaked in it runs on at half strength to every other Soaked enemy within four metres.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Reacted, Needs = AfflictionKind.Soaked, Do = UpgradeAction.Arc, LeapsTo = AfflictionKind.Soaked, Count = 4, Reach = 4f, Damage = 10f, Stagger = 15f, Stacks = 0 } } },
                BranchPattern.MiddleCrown));
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

            // The drill carries the flood, and nothing can touch you while it runs. NEEDS: the ebb first, drawing you 5 m
            // straight back in 0.3 s and leaving a line of wet sand, and the flood as the second press within 3 s back along
            // that line (second-press); pressed once, the ebb alone. Until then one press is the flood, run eight metres ahead.
            SpellBranch.Add(to, E, TheTideTurns, "The Tide Turns", SpellRole.Movement, BranchPattern.LeftA, Undertow, SpellForms.Drill,
                new SpellSpec { Reach = 8f, Radius = 1f, Count = 1, Damage = 10f, Stagger = 10f, Stacks = 1, Speed = 20f },
                "Press, and the ebb draws you five metres straight back in a moment, out of reach, and nothing can touch you. It leaves a line of wet sand where you went. Press again within three seconds, and the flood brings you back along the line and three metres past where you began as a breaking wave. Everything on the line takes 10 damage, is Soaked and is carried two metres ahead of you.",
                UpgradeIcons.Boot,
                new Empower { Cooldown = 6f, Reach = 10f },
                "The Tide Turns waits 6 seconds, not 8, and its flood runs five metres past.",
                // NEEDS: the Empower's longer flood not reaching past this shape's own (an Empower sets its numbers after the shape's).
                new ShapeDef { Name = "King Tide",
                    Description = "The flood comes in four metres wide and throws what it carries down for a moment, but it stops where you began. For a crowd.",
                    Change = new SpellChange { Radius = 2f, Reach = 5f },
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = KingTideDown, Seconds = 0.6f, Flags = ConditionFlags.Down } } } },
                // NEEDS: you drawn the 5 m back with it (the ebb). Until then the enemy faced is drawn five metres to you, Soaked.
                new ShapeDef { Name = "Out to Sea", Form = SpellForms.Nova,
                    Description = "There is no flood. The ebb takes the enemy faced with you: it is drawn the five metres out with you, Soaked, and dropped at your feet, away from its friends. A pull, not a return.",
                    Spell = new SpellSpec { Radius = 1f, Damage = 0f, Stagger = 0f, Stacks = 0 },
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Pull, Shape = HazardShape.Downwind, Amount = 5f },
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Soaked, Stacks = 1 },
                    } },
                "Opens two shapes of The Tide Turns, chosen at any rest fire: King Tide, a wide flood that throws a crowd down, or Out to Sea, an enemy drawn away with you.",
                // NEEDS: the shared movement rule's timing (started as a blow would land, their-blow-moment), and the flood's
                // own press. Until then the cast gives what a perfect dodge gives, and the next cast within a second of a perfect
                // dodge starts no clock.
                new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Mana, Amount = 12f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.CutCooldowns, Amount = 1.5f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.InstantNext, Seconds = 1f },
                },
                "Started in the instant a blow would land on you, it counts as a perfect dodge, and a flood pressed in the instant a blow would land starts no cooldown.",
                learnEffects: new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Push, Shape = HazardShape.Downwind, Amount = 2f } });

            // NEAREST: an enemy's recovery cannot be changed, so blows on a Soaked enemy break its balance sooner for a while.
            to.Add(BranchPattern.Put(new Upgrade { Id = HeavyWater, Name = "Heavy Water", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Undertow }, Icon = UpgradeIcons.Hammer,
                Description = "Striking a Soaked enemy makes your weapon break balance a fifth sooner for three seconds.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictedHit, Do = UpgradeAction.Boon, Needs = AfflictionKind.Soaked, Stat = PlayerStat.StaggerDealt, Amount = 0.2f, Seconds = 3f, Cooldown = 0.5f } } },
                BranchPattern.LeftMiddle));

            // The field form bites and soaks; each bite breaks balance harder for a moment. NEEDS: enemies drawn to the middle
            // at 1.2 m a second (pull-to-point), and their balance not recovering inside it (enemy-balance-held).
            SpellBranch.Add(to, E, Whirlpool, "Whirlpool", SpellRole.Breaker, BranchPattern.LeftB, HeavyWater, SpellForms.Field,
                new SpellSpec { Reach = 6f, Radius = 4f, Seconds = 5f, Interval = 0.5f, LingerDamage = 3f, Stacks = 1 },
                "A whirlpool four metres round opens under the enemy faced, up to six metres away, for five seconds. Enemies in it are drawn to the middle, kept Soaked and bitten for 3 twice a second. Their balance does not recover inside it, and your blows there break balance 30% harder.",
                UpgradeIcons.Spiral,
                new Empower { Cooldown = 8f, Seconds = 7f },
                "Whirlpool waits 8 seconds, not 10, and lasts seven seconds.",
                // NEEDS: the hard pull (3 m a second, pull-to-point). The throw at its end is the end of each enemy's churn: when
                // the bites stop.
                new ShapeDef { Name = "Maelstrom",
                    Description = "Smaller, two and a half metres round, and pulling hard. At its end it throws everything inside up in a spout and down again for 50 stagger.",
                    Change = new SpellChange { Radius = 2.5f },
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = SpoutRising, Seconds = 0.7f } },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = SpoutRising, Do = UpgradeAction.Lift, Seconds = 0.8f },
                        new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = SpoutRising, Do = UpgradeAction.Burst, Damage = 0f, Stagger = 50f, Radius = 0.6f },
                    } },
                // NEEDS: the lane of foam 2 m wide and 9 m long running from the point away from you for 6 s, and no dodging
                // against it (ground-states, no-dodge-or-leap). Until then the whirlpool carries what it bites away from you at
                // two and a half metres a second instead of drawing it in.
                new ShapeDef { Name = "Rip Current",
                    Description = "No pull to the middle. A lane of churning foam two metres wide and nine long runs from the whirlpool's point away from you for six seconds. Enemies in it are carried down it and cannot dodge against it, and your blows on a carried enemy break balance 30% harder. Cut them as the current drags them past.",
                    Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Push, Shape = HazardShape.Downwind, Amount = 1.25f } } },
                "Opens two shapes of Whirlpool, chosen at any rest fire: Maelstrom, a hard pull that ends in a spout, or Rip Current, a lane that carries them past you.",
                // NEEDS: in the instant its enemy's attack mark shows (enemy-begins-attack), and its own cooldown halved
                // (own-clock-share). Until then the first enemy it bites on a cast flinches out of what it had begun.
                new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Flinch, Seconds = 0.4f, Cooldown = 5f } },
                "Open it under an enemy in the instant its attack mark shows, and the attack is lost as it goes in to the knees. Its cooldown is halved.",
                learnEffects: new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Churned, Seconds = 0.7f, Flags = ConditionFlags.StaggerTakenShare, Scale = 1.3f } } });

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
                // NEEDS: rising 6.5 m away. The base spell is cast by its kit, which takes only the Empower's wait.
                new Empower { Cooldown = 4.5f, Reach = 6.5f },
                "Riptide waits 4.5 seconds, not 6, and rises six and a half metres away.",
                // River Serpent, once a spell, folded in. NEEDS: the serpent going out and coming back, and the first enemy it
                // takes dropped 1.5 m in front of you. Until then the dragon runs its lane and the first enemy it strikes is
                // drawn four metres back toward you with its guard down.
                new ShapeDef { Name = "River Serpent", Form = SpellForms.Dragon,
                    Description = "A serpent of water goes eight metres out along a lane and back, and brings only the first enemy it takes, dropping it in front of you with its guard down for a second. One target, from further.",
                    Spell = new SpellSpec { Reach = 8f, Radius = 1.2f, Damage = 12f, Stagger = 30f, Stacks = 1, Speed = 16f },
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Pull, Shape = HazardShape.Downwind, Amount = 4f, Cooldown = 0.5f },
                        new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = GuardDown, Seconds = 1f, Flags = ConditionFlags.CannotGuard }, Cooldown = 0.5f },
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
                // NEEDS: the second press within 1 s of the wave reaching you, sending it out again and carrying what it brought
                // 4 m past you (second-press). Until then what the wave brings stands with its back to you a moment: it begins
                // nothing, and your first blow on it is critical.
                new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnBranch = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = BacksTurned, Seconds = 1.2f, Flags = ConditionFlags.BeginsNoAttack | ConditionFlags.TakesCritical, EndsWith = ConditionBlow.Weapon } } },
                "Press again within a second of the wave reaching you, and it goes out again the way it came, carrying what it brought past you and on four metres, backs turned.");

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
                // Its first design. NEEDS: the flask as its price, not mana, and the wave only within the 12 s (price-in-flasks),
                // and the weapon's reach 40% longer. Until then the cast makes every blow mend for twelve seconds, and while this
                // shape is worn the last blow of every chain throws a wave.
                new ShapeDef { Name = "Poured on the Edge", Form = SpellForms.Nova,
                    Description = "No copy. A flask is poured along the weapon, and for twelve seconds its reach is 40% longer, every blow mends 2 and soaks, and the last blow of a chain throws a wave five metres. It costs a flask, not mana.",
                    Spell = new SpellSpec { Radius = 1f, Damage = 0f, Stagger = 0f, Stacks = 0 },
                    Effects = new[]
                    {
                        new UpgradeEffect { On = UpgradeTrigger.SpellCast, OwnBranch = true, Do = UpgradeAction.Boon, Stat = PlayerStat.Leech, Amount = 0.1f, Seconds = 12f },
                        new UpgradeEffect { On = UpgradeTrigger.ChainEnd, Do = UpgradeAction.Cone, Reach = 5f, Arc = 60f, Damage = 10f, Stagger = 15f, Affliction = AfflictionKind.Soaked, Stacks = 1 },
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

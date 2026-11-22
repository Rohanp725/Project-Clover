using System;
using System.Collections.Generic;
using Clover.Gameplay;
using Clover.Player.Way;

namespace Clover.Player.Trees.Ways
{
    /// <summary>
    /// The Katana's Way (docs/design/steel/way_plan.md 1, every fork option 1 as the owner decided on 2026-11-19; task
    /// 5.2): the trunk (Temper I to V, the four infusion families, the five weapon basics, the gated technique Before the
    /// Thunder, and the five element infusion marks that left the element trees under their old ids), the three plain
    /// boughs (the Sheath, the Stream, the Answer: an aspect, a stem of three and two paths of two marks and a keystone,
    /// 23 Memories each) and Narukami's Storm Katana (a stem of five, 25 Memories, given by Take). 132 Memories in all.
    ///
    /// Every mark is bought with Memories and drawn on the Way's chart (task 5.5 places it; X and Y wait for
    /// <c>WayLayout</c>). Marks are bought in order along a bough; trunk marks in any order, the temper ranks one after
    /// another. What a mark does to the moves is the bough's code (<c>Way/Aspects/</c>), read by id; what it can say as
    /// data (a stat, a boon, a lane) is written here. What neither can say yet is the nearest working version with a
    /// <c>NEEDS</c> line, gathered in docs/design/steel/content_open.tsv; moves that need a clip of their own name it, and
    /// none is made here (the Forge waits for the editor).
    ///
    /// Burning Crescent and Delayed Cuts are done by hand in <see cref="PlayerUpgrades"/> under their ids, on the Iaido
    /// Slash, as they were on the Fire and Lightning trees. Their ids, and those of Drawing Wind, Winter Draw and Answering
    /// Draw, are the ones a save kept for them (<see cref="RetiredMarks.Moving"/>).
    /// </summary>
    public static class KatanaWay
    {
        private const string W = Weapons.KatanaName;

        // The trunk.
        public const string Temper1 = "katana.temper1";
        public const string Temper2 = "katana.temper2";
        public const string Temper3 = "katana.temper3";
        public const string Temper4 = "katana.temper4";
        public const string Temper5 = "katana.temper5";
        public const string InfuseGround = "katana.infuse.ground";
        public const string InfuseLine = "katana.infuse.line";
        public const string InfuseWeight = "katana.infuse.weight";
        public const string InfuseDraw = "katana.infuse.draw";
        public const string RisingDraw = "katana.risingdraw";
        public const string TsukaAte = "katana.tsukaate";
        public const string SettledHands = "katana.settledhands";
        public const string Kozuka = "katana.kozuka";
        public const string Zanshin = "katana.zanshin";
        public const string BeforeTheThunder = "katana.beforethethunder";
        /// <summary>The progress flag the Last Retainer sets when beaten (WORLD.md; the Lightning and Wind land's elite).</summary>
        public const string LastRetainerFlag = "elite.last_retainer";
        // The element infusion marks, moved off the element trees with their ids (steel/weapons.md 1, "Element infusion marks").
        public const string BurningCrescent = Upgrade.BurningCrescent;
        public const string DelayedCuts = Upgrade.DelayedCuts;
        public const string DrawingWind = "wind.drawing";
        public const string WinterDraw = "ice.winterdraw";
        public const string AnsweringDraw = "halo.answeringdraw";

        // The Sheath (iai).
        public const string Sheath = "katana.sheath";
        public const string SharpDraw = "katana.sheath.sharpdraw";
        public const string QuickSheath = "katana.sheath.quicksheath";
        public const string Chiburi = "katana.sheath.chiburi";
        public const string HeldDraw = "katana.sheath.helddraw";
        public const string SayaBiki = "katana.sheath.sayabiki";
        public const string TheCutLandsAtTheClick = "katana.sheath.click";
        public const string NukitsukeAside = "katana.sheath.aside";
        public const string LightScabbard = "katana.sheath.lightscabbard";
        public const string AHundredDays = "katana.sheath.hundreddays";

        // The Stream (flow).
        public const string Stream = "katana.stream";
        public const string LightWrist = "katana.stream.lightwrist";
        public const string RiverBend = "katana.stream.riverbend";
        public const string SteppingWater = "katana.stream.steppingwater";
        public const string SecondBreath = "katana.stream.secondbreath";
        public const string Unbroken = "katana.stream.unbroken";
        public const string NoLastCut = "katana.stream.nolastcut";
        public const string EbbStep = "katana.stream.ebbstep";
        public const string Eddy = "katana.stream.eddy";
        public const string TheWaterHasNoShape = "katana.stream.noshape";

        // The Answer (counter).
        public const string Answer = "katana.answer";
        public const string TurnedEdge = "katana.answer.turnededge";
        public const string OpenWindow = "katana.answer.openwindow";
        public const string PatientBlade = "katana.answer.patientblade";
        public const string GuardCut = "katana.answer.guardcut";
        public const string SecondAnswer = "katana.answer.secondanswer";
        public const string KiriOtoshi = "katana.answer.kiriotoshi";
        public const string CatchTheRed = "katana.answer.catchthered";
        public const string ReadTheRed = "katana.answer.readthered";
        public const string TheFirstSword = "katana.answer.firstsword";

        // Narukami's Storm Katana (Lightning; the god's bough).
        public const string Storm = WayRules.StormKatanaBough;
        public const string LongBlade = "katana.storm.longblade";
        public const string FlankStep = "katana.storm.flankstep";
        public const string SevenDrums = "katana.storm.sevendrums";
        public const string TheRedDraw = "katana.storm.reddraw";
        public const string TheFallingCut = "katana.storm.fallingcut";
        public const string ThunderFollows = "katana.storm.thunderfollows";
        public const string LongThunder = "katana.storm.longthunder";
        public const string Raikiri = "katana.storm.raikiri";
        public const string Drumbeat = "katana.storm.drumbeat";
        public const string RollingDrums = "katana.storm.rollingdrums";
        public const string TheDrumRing = "katana.storm.drumring";

        // Numbers the boughs' code reads, kept with the words that state them.
        /// <summary>The Sheath's draw: the first blow of every chain, times this, with this much more stagger; Sharp Draw's, and a Hundred Days' (way_plan.md 1).</summary>
        public const float DrawShare = 1.5f, SharpDrawShare = 1.7f, HundredDaysShare = 1.3f, DrawStagger = 10f;

        public static void Add(List<Upgrade> to)
        {
            AddTrunk(to);
            AddSheath(to);
            AddStream(to);
            AddAnswer(to);
            AddStorm(to);
        }

        // A mark of the Katana's Way: bought with Memories, on its bough (or the trunk, for an empty aspect), grown from one mark.
        private static Upgrade Mark(List<Upgrade> to, string id, string name, UpgradeKind kind, string aspect, string grows, string icon, string words)
        {
            var mark = new Upgrade
            {
                Id = id, Name = name, Kind = kind, Tree = MarkTree.Way, Currency = Currency.Memories, Weapon = W, Aspect = aspect ?? "",
                Requires = string.IsNullOrEmpty(grows) ? Array.Empty<string>() : new[] { grows }, Icon = icon, Description = words,
            };
            to.Add(mark);
            return mark;
        }

        private static void AddTrunk(List<Upgrade> to)
        {
            // Temper (steel/weapons.md 0.3): the only raw damage, +6% weapon damage and +5% stagger a rank, opening with the
            // climb (depth_weapons.md 6: I and II in the Fire land, III at the third god, IV the fifth, V the seventh).
            string[] tempers = { Temper1, Temper2, Temper3, Temper4, Temper5 };
            string[] numerals = { "I", "II", "III", "IV", "V" };
            // The player's words (way_build_plan.md 2.1): one shared line, and what ranks III and V add.
            string[] own =
            {
                "", "",
                " The drawing cut reaches a little further.",
                "",
                " The finishing blow hits harder, and you hold your last pose a moment longer.",
            };
            for (int i = 0; i < tempers.Length; i++)
            {
                var temper = Mark(to, tempers[i], "Katana Temper " + numerals[i], UpgradeKind.Temper, "", i == 0 ? "" : tempers[i - 1], UpgradeIcons.Anvil,
                    "Every Katana hit does a little more damage and breaks balance a little sooner." + own[i]);
                temper.Cost = Temper.PriceOf(i + 1);
                temper.Gate = Temper.GateOf(i + 1);
                temper.Stats = new[] { new StatMod(PlayerStat.WeaponDamage, Temper.DamagePerRank), new StatMod(PlayerStat.StaggerDealt, Temper.StaggerPerRank) };
            }
            // Temper V's longer hold and harder blow are the trunk's patch (KatanaTrunk). NEEDS: the held pose in play (time
            // slowed while the pose holds, way-finisher-hold) and the finisher's clip with the pose in it.

            // The infusion families (steel/weapons.md 1, "Infusion", on the drawing cut): opened once an element is owned.
            // The infused strike reads them through Way/Infusion.cs and Way/InfusionGround.cs (task 5.3), all four in play.
            // NEEDS: Settling Draw's own clip (way-infusion-families) and the looks the approved pieces do not give (way-infusion-looks).
            // The player's words are way_build_plan.md 2.2's; the rules are Way/Infusion.cs.
            var ground = Mark(to, InfuseGround, "Lying Draw", UpgradeKind.Major, "", "", UpgradeIcons.Flame,
                "Your infused draw with Fire, Poison or Life leaves its cut burning on the floor for a few seconds.");
            var line = Mark(to, InfuseLine, "Running Draw", UpgradeKind.Major, "", "", UpgradeIcons.Bolt,
                "Your infused draw with Lightning, Wind or Light becomes a dash. You cut everyone in a line.");
            var weight = Mark(to, InfuseWeight, "Settling Draw", UpgradeKind.Major, "", "", UpgradeIcons.Rock,
                "Your infused draw with Earth, Metal or Ice is slower, but nothing can stop it. It sticks in the first enemy and staggers it hard.");
            var draw = Mark(to, InfuseDraw, "Dragging Draw", UpgradeKind.Major, "", "", UpgradeIcons.Drop,
                "Your infused draw with Water or Dark is followed by a second cut on its own. It pulls back everything the draw hit.");
            foreach (var family in new[] { ground, line, weight, draw }) family.Gate = MarkRules.GodsGate + "1";

            // The five weapon basics (way_plan.md 0.2 and 1, "Trunk basics"), in any order once the Katana is owned.
            // NEEDS: a clip of its own, katana_roll_cut (a low rising cut out of the roll); and the cut counting as the chain's
            // first step. The trunk's patch makes it the dodge's cut on the drawing cut's clip.
            Mark(to, RisingDraw, "Rising Draw", UpgradeKind.Major, "", "", UpgradeIcons.Boot,
                "Press light attack as your dodge ends. You rise out of the roll with a cut. Your combo goes on from there.");
            // The heavy out of the roll (WeaponCombo.DashHeavy, patched by KatanaTrunk): 6 and 45 stagger, through a raised guard,
            // which stays open 1 s (AttackStep.OpensGuard). NEEDS: its own clip, attack_katana_tsuka_ate (way-clips); the heavy's plays.
            Mark(to, TsukaAte, "Tsuka-ate", UpgradeKind.Major, "", "", UpgradeIcons.Fist,
                "Press heavy attack as your dodge ends. You slam the hilt into their face. It does little damage but breaks their balance. It also breaks a raised guard or shield for a moment.");
            Mark(to, SettledHands, "Settled Hands", UpgradeKind.Minor, "", "", UpgradeIcons.Hourglass,
                "Your three light cuts recover a little faster.");
            // NEEDS: the guard's light attack throwing a knife (a shot from the hip, E8) once every 6 s, with its clip and prop.
            Mark(to, Kozuka, "Kozuka", UpgradeKind.Major, "", "", UpgradeIcons.Arrow,
                "While guarding, press light attack. You flick a small knife at the enemy you face. It barely hurts, but a light enemy flinches. You can throw it every few seconds.");
            // The turn to the nearest other enemy is PlayerCombat.Zanshin; no effect is drawn (question 19). The wider window is a boon.
            // NEEDS (turn-to-face): the head leading the turn.
            var zanshin = Mark(to, Zanshin, "Zanshin", UpgradeKind.Major, "", "", UpgradeIcons.Eye,
                "After a kill, you turn to face the next enemy. For a moment, a perfect guard is easier.");
            zanshin.Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Do = UpgradeAction.Boon, At = EffectAt.Self, Stat = PlayerStat.PerfectGuardWindow, Amount = 0.28f, Seconds = 1f } };

            // The gated technique (steel/weapons.md 0.5 and 1): sealed until the Last Retainer falls, on light and heavy together.
            // The chord plays it once bought (Way/Techniques.cs). NEEDS (way-technique): the broken wind-up (50 stagger, the
            // shorter recovery) and its own clip, attack_katana_before_thunder; the drawing cut's clip plays at its timings.
            var technique = Mark(to, BeforeTheThunder, "Before the Thunder", UpgradeKind.Technique, "", "", UpgradeIcons.Bolt,
                "Press light and heavy attack together. You draw faster than anything. Hit an enemy as it winds up an attack, even a red one, and the attack is broken. Miss the timing and you are left open.");
            technique.Gate = LastRetainerFlag;

            // The element infusion marks: the built tree marks that need the Katana, moved here with their ids and their rules
            // on the Iaido Slash, each working while its element is carried. Opened once an element is owned, as the families.
            var crescent = Mark(to, BurningCrescent, "Burning Crescent", UpgradeKind.Major, "", "", UpgradeIcons.Flame,
                "With Fire, your Iaido Slash throws a slow wave of flame ahead of it.");
            crescent.Element = Elements.Fire;
            var cuts = Mark(to, DelayedCuts, "Delayed Cuts", UpgradeKind.Major, "", "", UpgradeIcons.Bolt,
                "With Lightning, your Iaido Slash passes through. The cuts land a moment later, all at once, when your sword is home.");
            cuts.Element = Elements.Lightning;
            var wind = Mark(to, DrawingWind, "Drawing Wind", UpgradeKind.Major, "", "", UpgradeIcons.Gust,
                "With Wind, your Iaido Slash pulls everyone nearby toward you and throws them off balance.");
            wind.Element = Elements.Wind;
            wind.Effects = new[]
            {
                new UpgradeEffect { On = UpgradeTrigger.SkillHit, Do = UpgradeAction.Pull, At = EffectAt.Self, Radius = 6f, Amount = 2.5f, Cooldown = 1f },
                new UpgradeEffect { On = UpgradeTrigger.SkillHit, Do = UpgradeAction.Afflict, At = EffectAt.Self, Radius = 6f, Affliction = AfflictionKind.Unsteady, Stacks = 2, Cooldown = 1f },
            };
            var winter = Mark(to, WinterDraw, "Winter Draw", UpgradeKind.Major, "", "", UpgradeIcons.Snowflake,
                "With Ice, your Iaido Slash on a chilled enemy sends a blade of ice flying on.");
            winter.Element = Elements.Ice;
            winter.Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SkillHit, Do = UpgradeAction.Projectile, Needs = AfflictionKind.Chill, Flight = Flight.Straight, Damage = 18f, Stagger = 20f, Radius = 1f, Speed = 16f, Seconds = 0.5f, Affliction = AfflictionKind.Chill, Stacks = 2, Cooldown = 1f } };
            // Halo's (Metal and Light) once; on the Way it is Light's, so its notch lays only while Metal is the first element
            // (the weapon's blows lay the first element's affliction, MarkRules.BlowMayLay).
            var answering = Mark(to, AnsweringDraw, "Answering Draw", UpgradeKind.Major, "", "", UpgradeIcons.Sun,
                "With Light, your Iaido Slash marks enemies and cracks their armour. After a perfect guard, your hits are critical for a moment.");
            answering.Element = Elements.Light;
            answering.Effects = new[]
            {
                new UpgradeEffect { On = UpgradeTrigger.SkillHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Marked, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.SkillHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Sundered, Stacks = 1 },
                new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Boon, Stat = PlayerStat.CritChance, Amount = 1f, Seconds = 1.5f },
            };
            foreach (var mark in new[] { crescent, cuts, wind, winter, answering }) mark.Gate = MarkRules.GodsGate + "1";
        }

        private static void AddSheath(List<Upgrade> to)
        {
            // NEEDS: the sheathe as a beat of its own (the sword goes home after every chain or 0.6 s idle, a fast noto clip).
            // The patch gives the draw its bonus and ends the chain at the return cut.
            Mark(to, Sheath, "The Sheath", UpgradeKind.Aspect, Sheath, "", UpgradeIcons.Blade,
                "Iai. Your sword goes back into its sheath after every combo. The first cut of each combo is a draw from the sheath, and it hits much harder. Each combo is the draw, a return cut, and then the sword goes home.");
            Mark(to, SharpDraw, "Sharp Draw", UpgradeKind.Minor, Sheath, Sheath, UpgradeIcons.Blade,
                "Your draw from the sheath hits even harder.");
            // NEEDS: the guard pressed in a cut's recovery sheathing in 0.15 s, the next light a draw, with a clip of its own (a fast noto).
            Mark(to, QuickSheath, "Quick Sheath", UpgradeKind.Major, Sheath, SharpDraw, UpgradeIcons.Shield,
                "Press guard during a cut's recovery to sheathe at once. Your next light attack is a draw.");
            // NEEDS: the flick that clears the recovery and sheathes, with its clip. Its next draw's stagger is a boon spent by the next blow.
            var chiburi = Mark(to, Chiburi, "Chiburi", UpgradeKind.Major, Sheath, QuickSheath, UpgradeIcons.Drop,
                "Right after a kill, press light attack. You flick your sword clean and sheathe it. Your next draw breaks balance harder.");
            chiburi.Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Do = UpgradeAction.Boon, At = EffectAt.Self, Stat = PlayerStat.StaggerDealt, Amount = 1f, Seconds = 4f, UntilBlow = true } };

            // Path A: the one perfect draw.
            // NEEDS: a held light press (hold 0.4 s to sink and creep; let go to draw down a lane 5 m by 1 m, carrying 3 m, 28 and
            // 36 stagger; a thin white line on the floor while held), with its clip katana_held_draw.
            Mark(to, HeldDraw, "Held Draw", UpgradeKind.Major, Sheath, Chiburi, UpgradeIcons.Hourglass,
                "Hold light attack to crouch and creep forward. Let go to dash and cut along the line in front of you.");
            // NEEDS: the held draw (as above): held a second or more, its lane longer and cutting all on it.
            Mark(to, SayaBiki, "Saya-biki", UpgradeKind.Major, Sheath, HeldDraw, UpgradeIcons.Arrow,
                "Hold the draw for a second or more. It goes further and cuts everyone in the line.");
            // NEEDS: the held draw held to 2 s, released full at three times through any guard, everyone on the lane held 0.4 s and
            // the wounds opening as the guard clicks home, with its clip (the slow noto with a held pose).
            Mark(to, TheCutLandsAtTheClick, "The Cut Lands at the Click", UpgradeKind.Keystone, Sheath, SayaBiki, UpgradeIcons.Star,
                "Hold the draw even longer, up to two seconds. Let go when it is full. You cut through any guard, and everyone in the line freezes. Their wounds open when your sword clicks back into its sheath. If you are hit while holding, you lose the draw.");

            // Path B: a flurry of draws.
            // NEEDS: the draw stepping 1.5 m to the side the stick holds, with its clip (one clip mirrored).
            Mark(to, NukitsukeAside, "Nukitsuke Aside", UpgradeKind.Major, Sheath, Chiburi, UpgradeIcons.Boot,
                "Hold the stick to one side and press light attack. You step that way as you draw, out of the path of their attack.");
            // NEEDS: a draw that lands sheathing at once when its recovery ends, and the stamina taken off only draws within 1 s
            // of each other. The patch takes it off every draw.
            Mark(to, LightScabbard, "Light Scabbard", UpgradeKind.Major, Sheath, NukitsukeAside, UpgradeIcons.Wing,
                "When a draw hits, your sword goes straight home. Draws close together cost less stamina.");
            // The count and the hanging cut are SheathAspect's (Tick, Blow). NEEDS: its clip, attack_katana_draw_loop (way-clips),
            // and the slash trail held where it was cut (way-hanging-cut). The patch makes every light a draw.
            Mark(to, AHundredDays, "A Hundred Days at the Shrine", UpgradeKind.Keystone, Sheath, LightScabbard, UpgradeIcons.Sun,
                "Every light attack is a draw. You draw and sheathe in one quick stroke, again and again. If you are not hit, every fourth draw leaves its cut hanging in the air. Anyone who walks into it is cut again.");
        }

        private static void AddStream(List<Upgrade> to)
        {
            // The patch keeps the chain through a dodge (WeaponCombo.ChainKeptThroughDodge). NEEDS: clips of their own for the
            // turning cut and the rising cut (way-clips; the return cut's and the drawing cut's play).
            Mark(to, Stream, "The Stream", UpgradeKind.Aspect, Stream, "", UpgradeIcons.Wave,
                "Flow. Your combo is five cuts instead of three. After the third come a turning cut and a rising cut. You can dodge in the middle of the combo and keep going.");
            Mark(to, LightWrist, "Light Wrist", UpgradeKind.Minor, Stream, Stream, UpgradeIcons.Hourglass,
                "From the third cut on, your cuts are faster.");
            // NEEDS: a heavy in the middle of the chain that does not end it (a quick overhead, 26), with its clip.
            Mark(to, RiverBend, "River Bend", UpgradeKind.Major, Stream, LightWrist, UpgradeIcons.Wave,
                "Press heavy attack in the middle of your combo. You make a quick overhead cut, and the combo keeps going.");
            // The patch makes it the dodge's cut from any part of the roll, the chain's next step (WeaponCombo.DashCutFrom,
            // DashCutInChain). NEEDS: its clip, the Rising Draw's attack_katana_roll_cut (way-clips).
            Mark(to, SteppingWater, "Stepping Water", UpgradeKind.Major, Stream, RiverBend, UpgradeIcons.Boot,
                "Press light attack at any point in a dodge. You cut out of the roll, and it counts as the next cut of your combo.");

            // Path A: stand and never stop.
            // NEEDS: only from the fourth cut on (a step of the chain as a condition); now every light that lands.
            var second = Mark(to, SecondBreath, "Second Breath", UpgradeKind.Major, Stream, SteppingWater, UpgradeIcons.Heart,
                "From the fourth cut on, each cut that hits gives you back a little stamina.");
            second.Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.LightHit, Do = UpgradeAction.Stamina, At = EffectAt.Self, Amount = 4f } };
            // NEEDS: the recovery too, and the 0.15 s flinch in place; the patch gives the chain's wind-ups and swings poise 12.
            Mark(to, Unbroken, "Unbroken", UpgradeKind.Major, Stream, SecondBreath, UpgradeIcons.Shield,
                "A small hit during your combo does not stop it. You flinch for a moment and keep cutting.");
            // NEEDS: a chain that loops (the fifth cut into the first, +8% a loop, three at most, ended only by a blow over 12, the
            // guard or no press), light blows giving half the mana, and the ring of five arcs at the third loop with its clip.
            Mark(to, NoLastCut, "No Last Cut", UpgradeKind.Keystone, Stream, Unbroken, UpgradeIcons.Ring,
                "After the fifth cut, your combo starts again with no pause, and each time round it hits harder. It can go round three times. Only a big hit, your guard, or stopping ends it. At the end of the third round, you cut all around you at once.");

            // Path B: never stand still.
            // NEEDS: a dodge inside the chain as a short slide (2.4 m for 15 stamina), with the slide's clip.
            Mark(to, EbbStep, "Ebb Step", UpgradeKind.Major, Stream, SteppingWater, UpgradeIcons.Wing,
                "A dodge in the middle of your combo becomes a short, cheap slide.");
            // NEEDS: only the cut out of a perfect dodge; now the next blow after any perfect dodge is critical.
            var eddy = Mark(to, Eddy, "Eddy", UpgradeKind.Major, Stream, EbbStep, UpgradeIcons.Spiral,
                "A cut out of a perfect dodge is a critical hit.");
            eddy.Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Boon, At = EffectAt.Self, Stat = PlayerStat.CritChance, Amount = 1f, Seconds = 0.8f, UntilBlow = true } };
            // NEEDS: every dodge in the chain cutting along its own line beside the bodies it passes and counting as the chain's
            // next step, the pale wake on the floor, and a clip of its own (a sliding cut).
            Mark(to, TheWaterHasNoShape, "The Water Has No Shape", UpgradeKind.Keystone, Stream, Eddy, UpgradeIcons.Drop,
                "Every dodge during your combo cuts everyone you slide past, and counts as the next cut. Your whole combo can be slides.");
        }

        private static void AddAnswer(List<Upgrade> to)
        {
            // The moment to answer is Way/AnswerWindow.cs, kept in the player's AspectRun. NEEDS: the Riposte's and Through the
            // Gap's own clips (way-clips) and the moment's look (way-answer-look). The patch takes a tenth off the light blows.
            Mark(to, Answer, "The Answer", UpgradeKind.Aspect, Answer, "", UpgradeIcons.Shield,
                "Counter. A perfect guard gives you a moment to answer. Press light attack to riposte with a critical cut. Press heavy attack to slip past and cut them from behind. Your normal light cuts do a little less damage.");
            // NEEDS: a share on the perfect guard's stagger; the patch adds a little to what it throws.
            Mark(to, TurnedEdge, "Turned Edge", UpgradeKind.Minor, Answer, Answer, UpgradeIcons.Shield,
                "Perfect guards break the attacker's balance faster.");
            // Way/AnswerWindow.cs: the moment lasts 0.8 s.
            Mark(to, OpenWindow, "Open Window", UpgradeKind.Major, Answer, TurnedEdge, UpgradeIcons.Hourglass,
                "You have longer to answer.");
            // Way/AnswerWindow.cs: the guard held unstruck counted in AspectRun.SinceCount.
            Mark(to, PatientBlade, "Patient Blade", UpgradeKind.Major, Answer, OpenWindow, UpgradeIcons.Eye,
                "The longer you hold your guard without being hit, the harder your next answer breaks balance.");

            // Path A: every blow answered as it comes.
            // NEEDS: a clip of its own (a short cut with the guard still raised); the patch makes it the guard's light attack on the
            // drawing cut's clip. It takes Kozuka's input while the Answer is active.
            Mark(to, GuardCut, "Guard Cut", UpgradeKind.Major, Answer, PatientBlade, UpgradeIcons.Blade,
                "While guarding, press light attack. You make a short cut and keep your guard up.");
            // Way/AnswerWindow.cs: the moment kept open for a second perfect guard.
            Mark(to, SecondAnswer, "Second Answer", UpgradeKind.Major, Answer, GuardCut, UpgradeIcons.Ring,
                "Your moment to answer stays open for a second perfect guard. A second answer breaks balance harder.");
            // The answer comes by itself (AnswerWindow.Answers). NEEDS (way-answer-window): Through the Gap if heavy follows within
            // 0.3 s, and the Riposte running on 4 m past the attacker. The wider window is a stat; the patch makes the chain two cuts.
            var kiri = Mark(to, KiriOtoshi, "Kiri-otoshi", UpgradeKind.Keystone, Answer, SecondAnswer, UpgradeIcons.Star,
                "Your answer comes on its own. Every perfect guard is followed by a riposte right away. Press heavy attack quickly to slip past instead. Perfect guards are a little easier, and your light combo is two cuts. The riposte cuts on past them and hits whoever is behind.");
            kiri.Stats = new[] { new StatMod(PlayerStat.PerfectGuardWindow, 0.17f) };

            // Path B: kept for the red blow.
            // Way/AnswerWindow.cs: a red blow turned (PlayerDefence.RedTurns) opens the moment for 1 s.
            Mark(to, CatchTheRed, "Catch the Red", UpgradeKind.Major, Answer, PatientBlade, UpgradeIcons.Target,
                "After a perfect guard on a red attack, you have much longer to answer.");
            // Guard.RedWindowBonus, set as the weapon is armed.
            Mark(to, ReadTheRed, "Read the Red", UpgradeKind.Major, Answer, CatchTheRed, UpgradeIcons.Eye,
                "Red attacks are easier to guard perfectly.");
            // NEEDS: a red attack turned as a trigger, time stopped 0.3 s with the camera's lean, and one passing cut through the
            // attacker (three times the Riposte, 120 stagger, gods included) with its clip.
            Mark(to, TheFirstSword, "The First Sword", UpgradeKind.Keystone, Answer, ReadTheRed, UpgradeIcons.Crown,
                "A perfect guard on a red attack stops time for a moment. Your riposte becomes one cut straight through them, even a god. Perfect guards on other attacks answer as normal.");
        }

        private static void AddStorm(List<Upgrade> to)
        {
            // Given when Narukami is taken (never sold; a playtest unlock). The patch makes his third cut a downward cut in a lane.
            // NEEDS: a clip of its own for the downward third cut (the stepping cut's is played).
            var storm = Mark(to, Storm, "Narukami's Storm Katana", UpgradeKind.Aspect, Storm, "", UpgradeIcons.Bolt,
                "Narukami's own sword. It is longer, and every hit is stronger. Its skill is the Thunder Draw. Its combo is his: across, back, then straight down along a line.");
            storm.Currency = Currency.None;
            storm.Element = "";
            Mark(to, LongBlade, "Long Blade", UpgradeKind.Minor, Storm, Storm, UpgradeIcons.Blade,
                "Every cut reaches a little further.");
            // NEEDS: a dodge then light within 0.2 s stepping through a line of lightning to the faced enemy's side, with its clip.
            Mark(to, FlankStep, "Flank Step", UpgradeKind.Major, Storm, LongBlade, UpgradeIcons.Bolt,
                "Right after a dodge, press light attack. You flash along a bolt of lightning to the enemy's side and cut as you land.");
            // The drums are Way/Drums.cs, kept in the player's AspectRun. NEEDS: the seven marks on the scabbard (way-drum-looks).
            Mark(to, SevenDrums, "Seven Drums", UpgradeKind.Major, Storm, FlankStep, UpgradeIcons.Ring,
                "Each cut that hits beats one of seven drums. When all seven are beaten, your next draw breaks balance much harder.");
            // NEEDS: its mana price (40; a charged heavy is free), being untouchable on the lane, and a clip of its own. The patch makes
            // it the held heavy on the Thunder Draw's clip.
            Mark(to, TheRedDraw, "The Red Draw", UpgradeKind.Major, Storm, SevenDrums, UpgradeIcons.Comet,
                "Hold heavy attack. You sheathe and crouch, then flash through everyone in a long line and stop behind them. Nothing can hurt you as you pass. It costs mana.");
            // NEEDS: a light enemy driven to one knee for 0.6 s (knocked down, task 3.7). The patch lengthens the lane.
            Mark(to, TheFallingCut, "The Falling Cut", UpgradeKind.Major, Storm, TheRedDraw, UpgradeIcons.Hammer,
                "His downward cut reaches further, and it knocks light enemies to one knee.");

            // Path A: the bolt called down a lane.
            // NEEDS: the lane left by every draw itself (the drawing cut too), thunder running down it from the player, and its flash
            // then sound. Now a lane struck from what the Thunder Draw hit, half a second later.
            var follows = Mark(to, ThunderFollows, "Thunder Follows", UpgradeKind.Major, Storm, TheFallingCut, UpgradeIcons.Bolt,
                "Each draw leaves a mark on the floor. Half a second later, thunder runs along it and hits everyone there again.");
            follows.Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SkillHit, Do = UpgradeAction.Lane, Damage = 27f, Stagger = 30f, Reach = 6f, Radius = 0.6f, Delay = 0.5f, Affliction = AfflictionKind.Static, Stacks = 1, Cooldown = 0.5f } };
            // NEEDS: Raikiri itself.
            Mark(to, LongThunder, "Long Thunder", UpgradeKind.Major, Storm, ThunderFollows, UpgradeIcons.Arrow,
                "Raikiri reaches further and bends toward the enemy you face.");
            // NEEDS: the skill held past the Thunder Draw (1.2 s, 50 mana), the bolt down onto the raised blade (a column of cyan
            // light as the tell) and the cut down a 12 m lane (60, 80 stagger, scorched 2 s), with its clip katana_raikiri.
            Mark(to, Raikiri, "Raikiri, the Cut Through Lightning", UpgradeKind.Keystone, Storm, LongThunder, UpgradeIcons.Bolt,
                "Hold the skill button past the Thunder Draw. You raise your sword, and lightning strikes it. Then you cut down, and the lightning tears along a long line in front of you. It costs a lot of mana.");

            // Path B: the thunder god's drums.
            // Way/Drums.cs: two drums for each perfect guard and each finisher.
            Mark(to, Drumbeat, "Drumbeat", UpgradeKind.Major, Storm, TheFallingCut, UpgradeIcons.Ring,
                "Perfect guards and finishing blows each beat two drums.");
            // Way/Drums.cs: the drums never fade out of a fight.
            Mark(to, RollingDrums, "Rolling Drums", UpgradeKind.Major, Storm, Drumbeat, UpgradeIcons.Hourglass,
                "Your drums do not fade after a fight. Once all seven are beaten, they wait until you use them.");
            // NEEDS: the drums, the ring of seven struck in turn round you over 0.7 s and the last bolt down the draw's lane, with
            // its clip (a drummer's stance between the beats).
            Mark(to, TheDrumRing, "The Drum Ring", UpgradeKind.Keystone, Storm, RollingDrums, UpgradeIcons.Crown,
                "When all seven drums are beaten, your Thunder Draw calls lightning all around you. Bolts strike one after another around you. The last one hits you and flies out along your draw.");
        }
    }
}

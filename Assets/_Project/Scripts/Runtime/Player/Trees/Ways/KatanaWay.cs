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
            // The infused strike reads them through Way/Infusion.cs (task 5.3): Running Draw's dash and Settling Draw's slow,
            // unbroken, harder draw are in play. NEEDS (way-infusion-families): the arc lying on the floor, the cut stopping
            // in the first enemy with the element set in it, the return cut that follows and drags, and Settling Draw's clip.
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
                "A light attack late in a dodge, or just after it, is a low rising cut out of the roll.");
            // NEEDS: a heavy out of the roll (the roll's second press) and a guard broken for a second, with a clip of its own.
            Mark(to, TsukaAte, "Tsuka-ate", UpgradeKind.Major, "", "", UpgradeIcons.Fist,
                "A heavy attack out of a dodge drives the hilt into the face. Little damage, a great deal of stagger, and a raised guard is broken for a moment.");
            Mark(to, SettledHands, "Settled Hands", UpgradeKind.Minor, "", "", UpgradeIcons.Hourglass,
                "The three light cuts recover a little sooner.");
            // NEEDS: the guard's light attack throwing a knife (a shot from the hip, E8) once every 6 s, with its clip and prop.
            Mark(to, Kozuka, "Kozuka", UpgradeKind.Major, "", "", UpgradeIcons.Arrow,
                "A light attack while guarding flicks the small knife from the scabbard at the enemy you face. It barely cuts, but a light enemy flinches.");
            // NEEDS: the turn to the nearest other enemy after a kill, and the thin white glance line. The wider window is a boon.
            var zanshin = Mark(to, Zanshin, "Zanshin", UpgradeKind.Major, "", "", UpgradeIcons.Eye,
                "For a moment after a kill you turn to the next enemy, and a guard raised then is perfect a little more easily.");
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
                "The Iaido Slash throws a slow arc of flame that travels on after the cut. Works while Fire is carried.");
            crescent.Element = Elements.Fire;
            var cuts = Mark(to, DelayedCuts, "Delayed Cuts", UpgradeKind.Major, "", "", UpgradeIcons.Bolt,
                "The Iaido Slash passes through. Its cuts land a moment later, all at once, when the blade is home. Works while Lightning is carried.");
            cuts.Element = Elements.Lightning;
            var wind = Mark(to, DrawingWind, "Drawing Wind", UpgradeKind.Major, "", "", UpgradeIcons.Gust,
                "When the Iaido Slash lands, everything within six metres is pulled toward you and left Unsteady. Works while Wind is carried.");
            wind.Element = Elements.Wind;
            wind.Effects = new[]
            {
                new UpgradeEffect { On = UpgradeTrigger.SkillHit, Do = UpgradeAction.Pull, At = EffectAt.Self, Radius = 6f, Amount = 2.5f, Cooldown = 1f },
                new UpgradeEffect { On = UpgradeTrigger.SkillHit, Do = UpgradeAction.Afflict, At = EffectAt.Self, Radius = 6f, Affliction = AfflictionKind.Unsteady, Stacks = 2, Cooldown = 1f },
            };
            var winter = Mark(to, WinterDraw, "Winter Draw", UpgradeKind.Major, "", "", UpgradeIcons.Snowflake,
                "When the Iaido Slash lands on a chilled enemy, a blade of ice flies on ahead and strikes and chills everything in its path. Works while Ice is carried.");
            winter.Element = Elements.Ice;
            winter.Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SkillHit, Do = UpgradeAction.Projectile, Needs = AfflictionKind.Chill, Flight = Flight.Straight, Damage = 18f, Stagger = 20f, Radius = 1f, Speed = 16f, Seconds = 0.5f, Affliction = AfflictionKind.Chill, Stacks = 2, Cooldown = 1f } };
            // Halo's (Metal and Light) once; on the Way it is Light's, so its notch lays only while Metal is the first element
            // (the weapon's blows lay the first element's affliction, MarkRules.BlowMayLay).
            var answering = Mark(to, AnsweringDraw, "Answering Draw", UpgradeKind.Major, "", "", UpgradeIcons.Sun,
                "The Iaido Slash Marks what it cuts and notches its armour. After a perfect guard, every blow you land for a moment is critical. Works while Light is carried.");
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
                "Iai: the sword goes home after every chain. The first cut of every chain is a draw from the sheath, far harder and heavier than a cut. The chain is the draw, the return cut, and the sheathe.");
            Mark(to, SharpDraw, "Sharp Draw", UpgradeKind.Minor, Sheath, Sheath, UpgradeIcons.Blade,
                "The draw from the sheath cuts deeper still.");
            // NEEDS: the guard pressed in a cut's recovery sheathing in 0.15 s, the next light a draw, with a clip of its own (a fast noto).
            Mark(to, QuickSheath, "Quick Sheath", UpgradeKind.Major, Sheath, SharpDraw, UpgradeIcons.Shield,
                "Guard in a cut's recovery to sheathe at once. The next light attack is a draw.");
            // NEEDS: the flick that clears the recovery and sheathes, with its clip. Its next draw's stagger is a boon spent by the next blow.
            var chiburi = Mark(to, Chiburi, "Chiburi", UpgradeKind.Major, Sheath, QuickSheath, UpgradeIcons.Drop,
                "A light attack just after a kill shakes the blood from the blade and sheathes. The next draw breaks balance far sooner.");
            chiburi.Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Do = UpgradeAction.Boon, At = EffectAt.Self, Stat = PlayerStat.StaggerDealt, Amount = 1f, Seconds = 4f, UntilBlow = true } };

            // Path A: the one perfect draw.
            // NEEDS: a held light press (hold 0.4 s to sink and creep; let go to draw down a lane 5 m by 1 m, carrying 3 m, 28 and
            // 36 stagger; a thin white line on the floor while held), with its clip katana_held_draw.
            Mark(to, HeldDraw, "Held Draw", UpgradeKind.Major, Sheath, Chiburi, UpgradeIcons.Hourglass,
                "Hold the light attack to sink and creep. Let go to draw down a lane in front of you, carried with the cut. A thin white line on the floor shows the lane while you hold.");
            // NEEDS: the held draw (as above): held a second or more, its lane longer and cutting all on it.
            Mark(to, SayaBiki, "Saya-biki", UpgradeKind.Major, Sheath, HeldDraw, UpgradeIcons.Arrow,
                "Held a second or more, the held draw's lane is longer and it cuts every enemy on it, not only the first.");
            // NEEDS: the held draw held to 2 s, released full at three times through any guard, everyone on the lane held 0.4 s and
            // the wounds opening as the guard clicks home, with its clip (the slow noto with a held pose).
            Mark(to, TheCutLandsAtTheClick, "The Cut Lands at the Click", UpgradeKind.Keystone, Sheath, SayaBiki, UpgradeIcons.Star,
                "The held draw can be held longer, the white line brightening as it fills. Released full, it cuts through any guard, and every enemy on the lane stands still until the sword clicks home. Then the wounds open, all at once.");

            // Path B: a flurry of draws.
            // NEEDS: the draw stepping 1.5 m to the side the stick holds, with its clip (one clip mirrored).
            Mark(to, NukitsukeAside, "Nukitsuke Aside", UpgradeKind.Major, Sheath, Chiburi, UpgradeIcons.Boot,
                "A light attack with the stick held to one side draws as you step that way, off the line of their blow.");
            // NEEDS: a draw that lands sheathing at once when its recovery ends, and the stamina taken off only draws within 1 s
            // of each other. The patch takes it off every draw.
            Mark(to, LightScabbard, "Light Scabbard", UpgradeKind.Major, Sheath, NukitsukeAside, UpgradeIcons.Wing,
                "A draw that lands sheathes at once, and draws close together cost less stamina.");
            // NEEDS: every fourth draw in a row with no blow taken leaving its white crescent hanging 0.6 s, cutting again for 40%
            // (a count in AspectRun and its look), and a clip of its own (a looping draw and sheathe). The patch makes every light a draw.
            Mark(to, AHundredDays, "A Hundred Days at the Shrine", UpgradeKind.Keystone, Sheath, LightScabbard, UpgradeIcons.Sun,
                "Every light attack is a draw: drawn and sheathed in one stroke, without end. Every fourth draw in a row with no blow taken leaves its crescent hanging in the air to cut again.");
        }

        private static void AddStream(List<Upgrade> to)
        {
            // NEEDS: clips of their own for the turning cut and the rising cut (the patch plays the return cut's and the drawing
            // cut's), and a dodge in a cut's recovery that keeps the chain.
            Mark(to, Stream, "The Stream", UpgradeKind.Aspect, Stream, "", UpgradeIcons.Wave,
                "Flow: five cuts in place of three, a turning cut and a rising cut after the three, and a dodge in a cut's recovery no longer ends the chain.");
            Mark(to, LightWrist, "Light Wrist", UpgradeKind.Minor, Stream, Stream, UpgradeIcons.Hourglass,
                "From the third cut on, cuts are quicker.");
            // NEEDS: a heavy in the middle of the chain that does not end it (a quick overhead, 26), with its clip.
            Mark(to, RiverBend, "River Bend", UpgradeKind.Major, Stream, LightWrist, UpgradeIcons.Wave,
                "A heavy attack in the middle of the chain is a quick overhead that does not end it.");
            // NEEDS: the cut from any part of a dodge, not only its second half, counting as the chain's next step; its clip is
            // the Rising Draw's katana_roll_cut. The patch makes it the dodge's cut, taking the Rising Draw's place.
            Mark(to, SteppingWater, "Stepping Water", UpgradeKind.Major, Stream, RiverBend, UpgradeIcons.Boot,
                "A light attack in a dodge cuts out of the roll, and the chain goes on from it.");

            // Path A: stand and never stop.
            // NEEDS: only from the fourth cut on (a step of the chain as a condition); now every light that lands.
            var second = Mark(to, SecondBreath, "Second Breath", UpgradeKind.Major, Stream, SteppingWater, UpgradeIcons.Heart,
                "Each cut from the fourth on that lands gives back a little stamina.");
            second.Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.LightHit, Do = UpgradeAction.Stamina, At = EffectAt.Self, Amount = 4f } };
            // NEEDS: the recovery too, and the 0.15 s flinch in place; the patch gives the chain's wind-ups and swings poise 12.
            Mark(to, Unbroken, "Unbroken", UpgradeKind.Major, Stream, SecondBreath, UpgradeIcons.Shield,
                "A small blow taken in the chain does not end it. You flinch in place and the next light attack carries on.");
            // NEEDS: a chain that loops (the fifth cut into the first, +8% a loop, three at most, ended only by a blow over 12, the
            // guard or no press), light blows giving half the mana, and the ring of five arcs at the third loop with its clip.
            Mark(to, NoLastCut, "No Last Cut", UpgradeKind.Keystone, Stream, Unbroken, UpgradeIcons.Ring,
                "After the fifth cut the first follows with no pause, harder each time round. Only a heavy blow, your guard or a still hand ends it. At the end of the third loop all five arcs are cut at once as one ring round you.");

            // Path B: never stand still.
            // NEEDS: a dodge inside the chain as a short slide (2.4 m for 15 stamina), with the slide's clip.
            Mark(to, EbbStep, "Ebb Step", UpgradeKind.Major, Stream, SteppingWater, UpgradeIcons.Wing,
                "A dodge inside the chain is a short, cheap slide.");
            // NEEDS: only the cut out of a perfect dodge; now the next blow after any perfect dodge is critical.
            var eddy = Mark(to, Eddy, "Eddy", UpgradeKind.Major, Stream, EbbStep, UpgradeIcons.Spiral,
                "A cut out of a perfect dodge is critical.");
            eddy.Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Boon, At = EffectAt.Self, Stat = PlayerStat.CritChance, Amount = 1f, Seconds = 0.8f, UntilBlow = true } };
            // NEEDS: every dodge in the chain cutting along its own line beside the bodies it passes and counting as the chain's
            // next step, the pale wake on the floor, and a clip of its own (a sliding cut).
            Mark(to, TheWaterHasNoShape, "The Water Has No Shape", UpgradeKind.Keystone, Stream, Eddy, UpgradeIcons.Drop,
                "Every dodge in the chain cuts all you slide past and counts as the chain's next cut: a chain can be all slides. Each slide leaves a pale wake on the floor.");
        }

        private static void AddAnswer(List<Upgrade> to)
        {
            // NEEDS: the answer window (a perfect guard opening 0.6 s, a white flick down the blade; light attack in it the
            // Riposte, 24 and critical; heavy, Through the Gap, a step to the attacker's back, 30 and 40 stagger). The patch takes
            // a tenth off the light blows.
            Mark(to, Answer, "The Answer", UpgradeKind.Aspect, Answer, "", UpgradeIcons.Shield,
                "Counter: a perfect guard opens a moment to answer, shown as a white flick down the blade. A light attack in it is the Riposte, a critical cut. A heavy attack is Through the Gap, a step to the attacker's back. Light blows hit a little less.");
            // NEEDS: a share on the perfect guard's stagger; the patch adds a little to what it throws.
            Mark(to, TurnedEdge, "Turned Edge", UpgradeKind.Minor, Answer, Answer, UpgradeIcons.Shield,
                "Perfect guards break the attacker's balance sooner.");
            // NEEDS: the answer window (as the Answer's).
            Mark(to, OpenWindow, "Open Window", UpgradeKind.Major, Answer, TurnedEdge, UpgradeIcons.Hourglass,
                "The moment to answer lasts longer.");
            // NEEDS: the answer window, and a count of the guard held unstruck (AspectRun).
            Mark(to, PatientBlade, "Patient Blade", UpgradeKind.Major, Answer, OpenWindow, UpgradeIcons.Eye,
                "The longer the guard is held without being struck, the harder the next answer breaks balance.");

            // Path A: every blow answered as it comes.
            // NEEDS: a clip of its own (a short cut with the guard still raised); the patch makes it the guard's light attack on the
            // drawing cut's clip. It takes Kozuka's input while the Answer is active.
            Mark(to, GuardCut, "Guard Cut", UpgradeKind.Major, Answer, PatientBlade, UpgradeIcons.Blade,
                "A light attack while guarding is a short cut with the guard still raised. It does not chain.");
            // NEEDS: the answer window (as the Answer's), kept open for a second perfect guard.
            Mark(to, SecondAnswer, "Second Answer", UpgradeKind.Major, Answer, GuardCut, UpgradeIcons.Ring,
                "The moment to answer stays open for a second perfect guard, and a second answer in it breaks balance harder.");
            // NEEDS: the answer coming by itself after every perfect guard (Through the Gap if heavy follows within 0.3 s) and the
            // Riposte's seam 4 m past the attacker. The wider window is a stat; the patch makes the light chain two cuts.
            var kiri = Mark(to, KiriOtoshi, "Kiri-otoshi", UpgradeKind.Keystone, Answer, SecondAnswer, UpgradeIcons.Star,
                "The answer comes by itself: every perfect guard is followed at once by the Riposte, or by Through the Gap if you press heavy. Perfect guards come a little easier, and the light chain is two cuts. The Riposte runs on past the attacker as a white seam.");
            kiri.Stats = new[] { new StatMod(PlayerStat.PerfectGuardWindow, 0.17f) };

            // Path B: kept for the red blow.
            // NEEDS: a red attack turned as a trigger (content_open.tsv, red-attack) and the answer window.
            Mark(to, CatchTheRed, "Catch the Red", UpgradeKind.Major, Answer, PatientBlade, UpgradeIcons.Target,
                "After a perfect guard on a red attack, the moment to answer lasts far longer.");
            // NEEDS: a perfect window that is wider against red attacks only.
            Mark(to, ReadTheRed, "Read the Red", UpgradeKind.Major, Answer, CatchTheRed, UpgradeIcons.Eye,
                "Red attacks are easier to guard perfectly.");
            // NEEDS: a red attack turned as a trigger, time stopped 0.3 s with the camera's lean, and one passing cut through the
            // attacker (three times the Riposte, 120 stagger, gods included) with its clip.
            Mark(to, TheFirstSword, "The First Sword", UpgradeKind.Keystone, Answer, ReadTheRed, UpgradeIcons.Crown,
                "A perfect guard on a red attack stops time for a breath, and the answer becomes one cut through the attacker and out behind it, gods included. Perfect guards on other blows answer only as the Answer does.");
        }

        private static void AddStorm(List<Upgrade> to)
        {
            // Given when Narukami is taken (never sold; a playtest unlock). The patch makes his third cut a downward cut in a lane.
            // NEEDS: a clip of its own for the downward third cut (the stepping cut's is played).
            var storm = Mark(to, Storm, "Narukami's Storm Katana", UpgradeKind.Aspect, Storm, "", UpgradeIcons.Bolt,
                "His own sword, longer, storm-grey, with lightning crawling down the fuller: every blow harder, and the Thunder Draw. The chain is his: across, back, and down a lane.");
            storm.Currency = Currency.None;
            storm.Element = "";
            Mark(to, LongBlade, "Long Blade", UpgradeKind.Minor, Storm, Storm, UpgradeIcons.Blade,
                "Every cut reaches a little further.");
            // NEEDS: a dodge then light within 0.2 s stepping through a line of lightning to the faced enemy's side, with its clip.
            Mark(to, FlankStep, "Flank Step", UpgradeKind.Major, Storm, LongBlade, UpgradeIcons.Bolt,
                "A light attack just after a dodge steps through a line of lightning to the enemy's side and cuts as you arrive.");
            // NEEDS: seven drums drawn round the guard, one beaten by each cut that lands (AspectRun), the seventh's draw +40 stagger.
            Mark(to, SevenDrums, "Seven Drums", UpgradeKind.Major, Storm, FlankStep, UpgradeIcons.Ring,
                "Each cut that lands beats one of seven drums round you. At the seventh, the next draw breaks balance far harder.");
            // NEEDS: its mana price (40; a charged heavy is free), being untouchable on the lane, and a clip of its own. The patch makes
            // it the held heavy on the Thunder Draw's clip.
            Mark(to, TheRedDraw, "The Red Draw", UpgradeKind.Major, Storm, SevenDrums, UpgradeIcons.Comet,
                "Hold the heavy attack: you sheathe, sink, and go through along a long lane to stand behind it. Nothing touches you on the lane. His own red draw, given.");
            // NEEDS: a light enemy driven to one knee for 0.6 s (knocked down, task 3.7). The patch lengthens the lane.
            Mark(to, TheFallingCut, "The Falling Cut", UpgradeKind.Major, Storm, TheRedDraw, UpgradeIcons.Hammer,
                "His third cut's lane is longer, and a light enemy it strikes is driven to one knee.");

            // Path A: the bolt called down a lane.
            // NEEDS: the lane left by every draw itself (the drawing cut too), thunder running down it from the player, and its flash
            // then sound. Now a lane struck from what the Thunder Draw hit, half a second later.
            var follows = Mark(to, ThunderFollows, "Thunder Follows", UpgradeKind.Major, Storm, TheFallingCut, UpgradeIcons.Bolt,
                "Every draw leaves its lane on the floor. Half a second later thunder runs down it and strikes all on it again: the flash, then the sound.");
            follows.Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SkillHit, Do = UpgradeAction.Lane, Damage = 27f, Stagger = 30f, Reach = 6f, Radius = 0.6f, Delay = 0.5f, Affliction = AfflictionKind.Static, Stacks = 1, Cooldown = 0.5f } };
            // NEEDS: Raikiri itself.
            Mark(to, LongThunder, "Long Thunder", UpgradeKind.Major, Storm, ThunderFollows, UpgradeIcons.Arrow,
                "Raikiri's lane is longer, and it bends toward the enemy you face.");
            // NEEDS: the skill held past the Thunder Draw (1.2 s, 50 mana), the bolt down onto the raised blade (a column of cyan
            // light as the tell) and the cut down a 12 m lane (60, 80 stagger, scorched 2 s), with its clip katana_raikiri.
            Mark(to, Raikiri, "Raikiri, the Cut Through Lightning", UpgradeKind.Keystone, Storm, LongThunder, UpgradeIcons.Bolt,
                "Hold the skill past the Thunder Draw: you raise the sword and a bolt comes down onto the blade. Then you cut down a long lane with the bolt carried on the edge, and the lane is left scorched.");

            // Path B: the thunder god's drums.
            // NEEDS: the drums (as Seven Drums').
            Mark(to, Drumbeat, "Drumbeat", UpgradeKind.Major, Storm, TheFallingCut, UpgradeIcons.Ring,
                "Perfect guards and finishing blows each beat two drums.");
            // NEEDS: the drums (as Seven Drums').
            Mark(to, RollingDrums, "Rolling Drums", UpgradeKind.Major, Storm, Drumbeat, UpgradeIcons.Hourglass,
                "Beaten drums no longer fade between fights. Seven beaten wait until spent.");
            // NEEDS: the drums, the ring of seven struck in turn round you over 0.7 s and the last bolt down the draw's lane, with
            // its clip (a drummer's stance between the beats).
            Mark(to, TheDrumRing, "The Drum Ring", UpgradeKind.Keystone, Storm, RollingDrums, UpgradeIcons.Crown,
                "With seven drums beaten, the Thunder Draw becomes the ring: seven drums stand round you and a bolt strikes each in turn, each drum flashing before its bolt, the last coming down on you and out down the lane.");
        }
    }
}

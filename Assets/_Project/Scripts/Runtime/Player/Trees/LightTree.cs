using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Light: the exact moment and the clean fight. Three boughs from Marked: on the left Sight (precision, the
    /// fight without a wound), in the middle Sentence (marks and judgement), on the right Radiance (the light
    /// itself). The marks named BESPOKE do the nearest thing data can do until their own code is written.
    /// </summary>
    public static class LightTree
    {
        public const string Root = "light.mark";
        // Sight.
        public const string JustCut = "light.justcut";
        public const string SharpLight = "light.sharplight";
        public const string MeasuredGuard = "light.measuredguard";
        public const string Dazzle = "light.dazzle";
        public const string NarrowEscape = "light.narrowescape";
        public const string BrightEvasion = "light.brightevasion";
        public const string StepIntoLight = "light.step";
        public const string LongClarity = "light.longclarity";
        public const string Unblemished = "light.unblemished";
        // Sentence.
        public const string Weighed = "light.weighed";
        public const string PlainGuilt = "light.plainguilt";
        public const string LastingMark = "light.lastingmark";
        public const string Sentence = "light.sentence";
        public const string RichMark = "light.richmark";
        public const string BowedHead = "light.bowedhead";
        public const string RingOfSpears = "light.spears";
        public const string WhatItOwes = "light.whatitowes";
        public const string Reckoning = "light.reckoning";
        public const string HeavyVerdict = "light.heavyverdict";
        public const string Condemned = "light.condemned";
        // Radiance.
        public const string Brimming = "light.brimming";
        public const string Gleaning = "light.gleaning";
        public const string HardLight = "light.hardlight";
        public const string ReturningLight = "light.returninglight";
        public const string LongRays = "light.longrays";
        public const string StandingLight = "light.lantern";
        public const string ReadyLight = "light.readylight";
        public const string Daybreak = "light.daybreak";
        public const string TwoSuns = "light.twosuns";
        // The god's gift.
        public const string TheScales = "light.scales";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Light;
            const AfflictionKind M = AfflictionKind.Marked;

            // The design marks with the last blow of a chain only; a root lays its affliction with every blow, as the schema asks.
            to.Add(new Upgrade { Id = Root, Name = "Marked", Element = E, Kind = UpgradeKind.Root, X = 0f, Y = 1.2f, Icon = UpgradeIcons.Sun,
                Description = "The last blow of a chain and every perfect guard leave a mark of light. The next three blows on a Marked enemy are critical and give more mana.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.ChainEnd, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Marked, Stacks = 1 }, new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, Affliction = M, Stacks = 1 } } });

            // Left bough: Sight.
            // BESPOKE: the next attack of a chain pressed within 0.08 s of the moment the current blow hits is critical and free of stamina; a white glint runs the blade.
            // Until then: one blow in four makes the next one flash, and gives a little stamina back.
            to.Add(new Upgrade { Id = JustCut, Name = "Just Cut", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = -1.02f, Y = 2.73f, Icon = UpgradeIcons.Blade,
                Description = "Press the next blow of a chain at the instant the last one lands and it flashes. A flashing blow is critical and costs no stamina.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Chance = 0.25f, Do = UpgradeAction.Boon, Stat = PlayerStat.CritChance, Amount = 1f, Seconds = 0.8f, Cooldown = 1.5f },
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Chance = 0.25f, Do = UpgradeAction.Stamina, Amount = 6f, Cooldown = 1.5f },
                } });
            to.Add(new Upgrade { Id = SharpLight, Name = "Sharp Light", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { JustCut }, X = -2.09f, Y = 3.99f, Icon = UpgradeIcons.Eye,
                Description = "Critical blows strike harder.",
                Stats = new[] { new StatMod(PlayerStat.CritDamage, 0.15f) } });
            to.Add(new Upgrade { Id = MeasuredGuard, Name = "Measured Guard", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { JustCut }, X = -1.88f, Y = 3.07f, Icon = UpgradeIcons.Hourglass,
                Description = "The moment for a perfect guard is a little longer.",
                Stats = new[] { new StatMod(PlayerStat.PerfectGuardWindow, 0.17f) } });
            // NEAREST: a perfect dodge names no attacker, so the enemy faced is Marked and held for a second (it cannot turn, nor move or strike).
            to.Add(new Upgrade { Id = Dazzle, Name = "Dazzle", Element = E, Kind = UpgradeKind.Major, Requires = new[] { JustCut }, X = -1.5f, Y = 4.92f, Icon = UpgradeIcons.Eye,
                Description = "A perfect dodge flashes in the attacker's eyes. Blinded, it cannot turn to follow you, and it is Marked.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Marked, Stacks = 1, Radius = 2f }, new UpgradeEffect { On = UpgradeTrigger.PerfectDodge, Do = UpgradeAction.Hold, Radius = 2f, Seconds = 1f } } });
            to.Add(new Upgrade { Id = NarrowEscape, Name = "Narrow Escape", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Dazzle }, X = -1.41f, Y = 6.11f, Icon = UpgradeIcons.Boot,
                Description = "The moment for a perfect dodge is a little longer.",
                Stats = new[] { new StatMod(PlayerStat.DodgeSafeTime, 0.23f) } });
            to.Add(new Upgrade { Id = BrightEvasion, Name = "Bright Evasion", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Dazzle }, X = -2.52f, Y = 5.35f, Icon = UpgradeIcons.Gem,
                Description = "Perfect dodges give more mana.",
                Stats = new[] { new StatMod(PlayerStat.ManaFromDodges, 0.42f) } });
            // The step form gives a critical heavy blow after it lands; the design's finisher-strength blow for a cast timed as a perfect dodge is not in it yet.
            to.Add(new Upgrade { Id = StepIntoLight, Name = "Step Into Light", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Dazzle }, X = -2.51f, Y = 6.57f, Icon = UpgradeIcons.Wing,
                Description = "Hold the spell button to vanish and stand behind the enemy you face. Do it as its blow is about to land, and your next blow strikes as a finisher does. 35 mana.",
                Form = "step",
                Spell = new SpellSpec { Cost = 35f, Reach = 7f, Damage = 10f, Stagger = 10f, Stacks = 1, Seconds = 1.5f } });
            to.Add(new Upgrade { Id = LongClarity, Name = "Long Clarity", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { StepIntoLight }, X = -1.71f, Y = 7.52f, Icon = UpgradeIcons.Hourglass,
                Description = "Clarity lasts longer.",
                Stats = new[] { new StatMod(PlayerStat.SurgeSeconds, 0.38f) } });
            to.Add(new Upgrade { Id = Unblemished, Name = "Unblemished", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { StepIntoLight }, X = -2.95f, Y = 7.93f, Icon = UpgradeIcons.Star,
                Description = "Fight a while without a wound and every blow you land is critical. A wound ends it, and until it returns you take more from every blow.", Stats = new[] { new StatMod(PlayerStat.CritChance, 1f, StatWhen.Unhurt), new StatMod(PlayerStat.ManaFromBlows, 0.25f, StatWhen.Unhurt), new StatMod(PlayerStat.DamageTaken, 0.15f), new StatMod(PlayerStat.DamageTaken, -0.15f, StatWhen.Unhurt) } });

            // Middle bough: Sentence.
            // NEAREST: marks go three deep, as designed. Critical damage by the depth of the mark is a boon when a mark fills.
            to.Add(new Upgrade { Id = Weighed, Name = "Weighed", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = 0.17f, Y = 3.26f, Icon = UpgradeIcons.Target,
                Description = "Marks deepen, up to three. Each one makes your critical blows strike harder.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictionFull, Do = UpgradeAction.Boon, Stat = PlayerStat.CritDamage, Amount = 0.3f, Seconds = 6f } }, Stats = new[] { new StatMod(PlayerStat.AfflictionDepth, 2f) } });
            to.Add(new Upgrade { Id = PlainGuilt, Name = "Plain Guilt", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Weighed }, X = -0.02f, Y = 4.41f, Icon = UpgradeIcons.Fist,
                Description = "Marked enemies take more from your weapon.",
                Stats = new[] { new StatMod(PlayerStat.WeaponDamage, 0.06f, AfflictionKind.Marked) } });
            to.Add(new Upgrade { Id = LastingMark, Name = "Lasting Mark", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Weighed }, X = -0.82f, Y = 3.87f, Icon = UpgradeIcons.Hourglass,
                Description = "A mark gives one more critical blow before it is spent.",
                Stats = new[] { new StatMod(PlayerStat.MarkBlows, 1f) } });
            to.Add(new Upgrade { Id = Sentence, Name = "Sentence", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Weighed }, X = 0.65f, Y = 5.21f, Icon = UpgradeIcons.Blade,
                Description = "A heavy blow on an enemy marked three deep passes sentence. The marks are spent, and a blade of light falls on it a moment later.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Marked, NeedsFull = true, Do = UpgradeAction.Falling, Radius = 1.8f, Delay = 0.8f, Damage = 36f, Stagger = 45f, Cooldown = 1f }, new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Marked, NeedsFull = true, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Marked, Radius = 1f, Cooldown = 1f } } });
            to.Add(new Upgrade { Id = RichMark, Name = "Rich Mark", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Sentence }, X = 0.21f, Y = 6.4f, Icon = UpgradeIcons.Gem,
                Description = "Blows on Marked enemies give still more mana.",
                Stats = new[] { new StatMod(PlayerStat.ManaFromBlows, 0.25f, AfflictionKind.Marked) } });
            to.Add(new Upgrade { Id = BowedHead, Name = "Bowed Head", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Sentence }, X = -0.48f, Y = 5.53f, Icon = UpgradeIcons.Hammer,
                Description = "Critical blows break balance sooner.",
                Stats = new[] { new StatMod(PlayerStat.StaggerDealt, 0.2f, StatWhen.CriticalBlow) } });
            // The bullets form does not yet hold its target still.
            to.Add(new Upgrade { Id = RingOfSpears, Name = "Ring of Spears", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Sentence }, X = 1.33f, Y = 6.78f, Icon = UpgradeIcons.Ring,
                Description = "Hold the spell button. Six spears of light stand in the air round the enemy you face, wait a breath, and drive in together. It is held where it stands. 40 mana.",
                Form = "bullets",
                Spell = new SpellSpec { Cost = 40f, Count = 6, Radius = 3f, Delay = 0.6f, Damage = 6f, Stagger = 5f, Stacks = 1, Reach = 10f } });
            // NEEDS: a tally kept on an enemy that another effect spends. Here each wound marks the one who gave it, which deepens Sentence.
            to.Add(new Upgrade { Id = WhatItOwes, Name = "What It Owes", Element = E, Kind = UpgradeKind.Major, Requires = new[] { RingOfSpears }, X = 0.4f, Y = 8f, Icon = UpgradeIcons.Skull,
                Description = "Light keeps count of every wound an enemy gives you. Sentence pays them back.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Struck, Do = UpgradeAction.Afflict, Affliction = M, Stacks = 1 } } });
            // BESPOKE: a seal on the enemy faced counts all weapon damage dealt to it for 6 s, then deals half of the count again in one burst (150 at most) with 40 stagger;
            // if the enemy dies first it breaks on the nearest at half strength. Until then: the brand form, where each of the next six blows sets off its own small burst.
            to.Add(new Upgrade { Id = Reckoning, Name = "Reckoning", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { WhatItOwes }, X = -0.28f, Y = 9.4f, Icon = UpgradeIcons.Hourglass,
                Description = "Hold the spell button to set a seal on the enemy you face. A hand turns once round it. When it comes round, half of all your weapon did to it in that time is done again at once. 45 mana.",
                Form = "brand",
                Spell = new SpellSpec { Cost = 45f, Reach = 10f, Seconds = 6f, Count = 6, Radius = 2f, Damage = 12f, Stagger = 7f, Stacks = 1 } });
            // Spell damage does not reach Sentence or The Scales, which are effects. NEEDS: effect damage that a stat can raise.
            to.Add(new Upgrade { Id = HeavyVerdict, Name = "Heavy Verdict", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Reckoning }, X = -0.81f, Y = 10.46f, Icon = UpgradeIcons.Hammer,
                Description = "Blades and seals of judgement strike harder.",
                Stats = new[] { new StatMod(PlayerStat.SpellDamage, 0.1f) } });
            // BESPOKE: an enemy at full depth of Mark and under a quarter of its health (a tenth for a god or elite) can be finished as if staggered.
            // Until then: a mark reaching full depth breaks much of the enemy's balance, and the weapon deals less to enemies that bear nothing.
            to.Add(new Upgrade { Id = Condemned, Name = "Condemned", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { Reckoning }, X = 0.76f, Y = 10.55f, Icon = UpgradeIcons.Skull,
                Description = "An enemy marked three deep and near death is Condemned: it may be finished whatever its balance. Your weapon deals less to enemies that bear no mark.",
                Stats = new[]
                {
                    new StatMod(PlayerStat.WeaponDamage, -0.15f),
                    new StatMod(PlayerStat.DamageToAfflicted, 0.15f),
                },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictionFull, Do = UpgradeAction.Burst, Radius = 1.2f, Damage = 0f, Stagger = 60f, Cooldown = 4f } } });

            // Right bough: Radiance.
            to.Add(new Upgrade { Id = Brimming, Name = "Brimming", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Root }, X = 1.1f, Y = 2.67f, Icon = UpgradeIcons.Wave,
                Description = "While your mana is full, every heavy blow throws a wave of light ahead of it.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, While = StatWhen.ManaFull, Do = UpgradeAction.Cone, Reach = 5f, Arc = 70f, Damage = 12f, Stagger = 12f, Affliction = AfflictionKind.Marked, Stacks = 1 } } });
            to.Add(new Upgrade { Id = Gleaning, Name = "Gleaning", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Brimming }, X = 1.02f, Y = 3.96f, Icon = UpgradeIcons.Gem,
                Description = "Every blow of the weapon gives a little more mana.",
                Stats = new[] { new StatMod(PlayerStat.ManaFromBlows, 0.1f) } });
            to.Add(new Upgrade { Id = HardLight, Name = "Hard Light", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Brimming }, X = 1.97f, Y = 3.37f, Icon = UpgradeIcons.Sun,
                Description = "Spells of light strike harder.",
                Stats = new[] { new StatMod(PlayerStat.SpellDamage, 0.12f) } });
            // NEEDS: mana given as a share of the spell's cost, once a cast. A plain amount with a cooldown stands in.
            to.Add(new Upgrade { Id = ReturningLight, Name = "Returning Light", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Brimming }, X = 2.05f, Y = 4.53f, Icon = UpgradeIcons.Gem,
                Description = "A spell that strikes a Marked enemy gives back part of what it cost.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, Needs = M, Do = UpgradeAction.Mana, Amount = 9f, Cooldown = 1f } } });
            to.Add(new Upgrade { Id = LongRays, Name = "Long Rays", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { ReturningLight }, X = 1.74f, Y = 5.68f, Icon = UpgradeIcons.Arrow,
                Description = "Waves, rays and lanterns reach further.",
                Stats = new[] { new StatMod(PlayerStat.SpellReach, 0.25f) } });
            // The rod form is the planted totem. The burst from a perfect guard inside its glow is not in it yet.
            to.Add(new Upgrade { Id = StandingLight, Name = "Standing Light", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { ReturningLight }, X = 2.74f, Y = 6.32f, Icon = UpgradeIcons.Tower,
                Description = "Hold the spell button to plant a lantern of light. Enemies in its glow are Marked again and again, and a perfect guard inside it flashes and staggers all near you. 35 mana.",
                Form = SpellForms.Rod,
                Spell = new SpellSpec { Cost = 35f, Reach = 2.5f, Radius = 5f, Seconds = 10f, Interval = 2.5f, Damage = 2f, Stagger = 0f, Delay = 0.2f, Stacks = 1 } });
            // The stat makes every spell cheaper, not the base spell alone.
            to.Add(new Upgrade { Id = ReadyLight, Name = "Ready Light", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { StandingLight }, X = 3.09f, Y = 7.44f, Icon = UpgradeIcons.Gem,
                Description = "Glare costs less mana.",
                Stats = new[] { new StatMod(PlayerStat.BaseSpellCost, -0.2f) } });
            // BESPOKE: the first hold begins gathering; each blow landed adds a ring to the blade, up to eight; a second hold (or 7 s) looses a beam 12 m long and 1.6 m wide
            // for 8 damage and 6 stagger a ring. Until then: the beam at once, at the strength of six rings over its four bites.
            to.Add(new Upgrade { Id = Daybreak, Name = "Daybreak", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { StandingLight }, X = 2.03f, Y = 7.88f, Icon = UpgradeIcons.Beam,
                Description = "Hold the spell button and the blade begins to gather light. Every blow you land adds to it, up to eight. Hold again to loose it all down one line. 45 mana.",
                Form = "beam",
                Spell = new SpellSpec { Cost = 45f, Reach = 12f, Radius = 0.8f, Seconds = 0.4f, Interval = 0.1f, Damage = 12f, Stagger = 9f, Stacks = 1 } });
            // NEEDS: an action that casts the last spell again from the far side of the enemy at a share of its damage. Here each enemy a spell strikes is struck again by light a moment later.
            to.Add(new Upgrade { Id = TwoSuns, Name = "Two Suns", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { Daybreak }, X = 1.72f, Y = 9.32f, Icon = UpgradeIcons.Sun,
                Description = "When you cast a spell, a figure of light casts it again from the far side of your enemy. Spells cost more.",
                Stats = new[] { new StatMod(PlayerStat.SpellCost, 0.25f) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, Do = UpgradeAction.Burst, Radius = 2.5f, Delay = 0.4f, Damage = 12f, Stagger = 10f, Affliction = M, Stacks = 1, Cooldown = 0.2f } } });

            // BESPOKE (built): ScalesForm. Found wanting, the mana is emptied.
            to.Add(new Upgrade { Id = TheScales, Name = "The Scales", Element = E, Kind = UpgradeKind.Signature, X = 0f, Y = 12f, Icon = UpgradeIcons.Crown,
                Description = "While Clarity lasts, the wounds you deal and the wounds you take are weighed. If yours are the heavier when it ends, a sword of light falls on every Marked enemy. If not, the light leaves you, and your mana with it.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.Scales, Spell = new SpellSpec { Cost = 0f, Damage = 60f, Stagger = 80f, Radius = 2.5f, Reach = 12f, Count = 120, Stacks = 1 } } } });
        }
    }
}

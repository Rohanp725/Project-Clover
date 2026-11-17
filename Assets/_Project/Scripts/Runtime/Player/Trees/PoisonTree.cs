using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Poison: the long game at arm's length. Doses are laid with every blow, then the player guards and waits, and
    /// chooses to burst the poison or let it spread. Three boughs from Venom: on the left Patience (depth and the
    /// beat), in the middle Rupture (bursts), on the right Contagion (spreading).
    /// </summary>
    public static class PoisonTree
    {
        public const string Venom = "poison.venom";
        // Patience.
        public const string InTime = "poison.intime";
        public const string DeeperDose = "poison.deeperdose";
        public const string BitterDose = "poison.bitterdose";
        public const string LeftToRipen = "poison.ripen";
        public const string Lingering = "poison.lingering";
        public const string LanceTheWound = "poison.lance";
        public const string WaitingGame = "poison.waitinggame";
        public const string CalmPulse = "poison.calmpulse";
        public const string CoiledStrike = "poison.coiled";
        public const string SlowDeath = "poison.slowdeath";
        // Rupture.
        public const string Rupture = "poison.rupture";
        public const string Ripe = "poison.ripe";
        public const string WiderRupture = "poison.wideburst";
        public const string RottenFruit = "poison.rottenfruit";
        public const string Buckling = "poison.buckling";
        public const string Blister = "poison.blister";
        public const string SpentVenom = "poison.spentvenom";
        public const string OneByOne = "poison.onebyone";
        public const string Volatile = "poison.volatile";
        // Contagion.
        public const string Catching = "poison.catching";
        public const string StillAir = "poison.stillair";
        public const string WideCloud = "poison.widecloud";
        public const string OneSickness = "poison.onesickness";
        public const string LongLeap = "poison.longleap";
        public const string SwingingCenser = "poison.censer";
        public const string ReadyPoison = "poison.readypoison";
        public const string ShareAlike = "poison.sharealike";
        public const string SweetFever = "poison.sweetfever";
        public const string PlagueBearer = "poison.plaguebearer";
        // The god's gift.
        public const string CleanBlade = "poison.cleanblade";

        // Spell forms by the ids fixed in docs/design/FORMS.md; they are being built while this is written.
        private const string FormStep = "step";
        private const string FormBrand = "brand";
        private const string FormPillars = "pillars";
        private const string FormDiscs = "discs";
        private const string FormDrill = "drill";

        public static void Add(List<Upgrade> to)
        {
            const string E = Elements.Poison;
            const AfflictionKind P = AfflictionKind.Poison;

            // A light blow lays one dose, a heavy blow two, a finisher four (a finisher is also a heavy blow).
            to.Add(new Upgrade { Id = Venom, Name = "Venom", Element = E, Kind = UpgradeKind.Root, X = 0f, Y = 1.2f, Icon = UpgradeIcons.Fang,
                Description = "Blows of the weapon poison, up to eight deep. On a slow, steady beat the poison bites for all its depth at once.",
                WeaponStacks = 1,
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = P, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.FinisherHit, Do = UpgradeAction.Afflict, Affliction = P, Stacks = 2 },
                } });

            // Left bough: Patience. Depth and the beat.

            // Built on the beat (UpgradeEffect.OnBeat): a blow within a quarter of a second of the target's bite doses again and makes the blows of the next moment critical. The timed blow itself is not made critical.
            to.Add(new Upgrade { Id = InTime, Name = "In Time", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Venom }, X = -1.21f, Y = 2.4f, Icon = UpgradeIcons.Hourglass,
                Description = "A blow that lands as the poison bites doses twice, and for a moment after it your blows are critical.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Afflict, Needs = P, OnBeat = 0.25f, Affliction = P, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Boon, Needs = P, OnBeat = 0.25f, Stat = PlayerStat.CritChance, Amount = 1f, Seconds = 1.2f },
                } });
            // Stands in: nothing can raise an affliction's greatest depth, so each dose works harder.
            to.Add(new Upgrade { Id = DeeperDose, Name = "Deeper Dose", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { InTime }, X = -0.78f, Y = 3.57f, Icon = UpgradeIcons.Drop,
                Description = "Poison goes two doses deeper.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionDepth, 2f) } });
            to.Add(new Upgrade { Id = BitterDose, Name = "Bitter Dose", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { InTime }, X = -1.89f, Y = 3.06f, Icon = UpgradeIcons.Fang,
                Description = "Each dose bites harder.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionPower, 0.2f) } });
            // Built in the beat (Afflictions.PoisonRipens, kept by PlayerUpgrades): each bite with no fresh dose since the last is a quarter stronger than the one before. Any fresh dose resets it, not only a blow of the weapon.
            to.Add(new Upgrade { Id = LeftToRipen, Name = "Left to Ripen", Element = E, Kind = UpgradeKind.Major, Requires = new[] { InTime }, X = -1.61f, Y = 4.2f, Icon = UpgradeIcons.Leaf,
                Description = "Poison you leave alone ripens. Each bite that comes without a fresh dose from you is stronger than the last.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionSeconds, 0.34f) } });
            to.Add(new Upgrade { Id = Lingering, Name = "Lingering", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { LeftToRipen }, X = -2.4f, Y = 4.91f, Icon = UpgradeIcons.Hourglass,
                Description = "Poison stays longer before it fades.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionSeconds, 0.5f) } });
            // Built on the beat (UpgradeAction.Bite): every poisoned enemy within five metres of the attacker bites on the next step, each at its own depth.
            to.Add(new Upgrade { Id = LanceTheWound, Name = "Lance the Wound", Element = E, Kind = UpgradeKind.Major, Requires = new[] { LeftToRipen }, X = -2.0f, Y = 6.0f, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard makes the attacker's poison bite at once, and with it the poison in everyone near.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Bite, Radius = 5f } } });
            to.Add(new Upgrade { Id = WaitingGame, Name = "Waiting Game", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { LanceTheWound }, X = -1.45f, Y = 7.03f, Icon = UpgradeIcons.Gem,
                Description = "Perfect guards give more mana.",
                Stats = new[] { new StatMod(PlayerStat.ManaFromGuards, 0.33f) } });
            to.Add(new Upgrade { Id = CalmPulse, Name = "Calm Pulse", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { LanceTheWound }, X = -2.91f, Y = 6.76f, Icon = UpgradeIcons.Heart,
                Description = "Holding your guard costs less stamina.",
                Stats = new[] { new StatMod(PlayerStat.GuardStamina, -0.167f) } });
            // BESPOKE: the hold that coils through three fangs, and the release that passes through a blow. Until then: the step through the enemy at the strength of two fangs.
            to.Add(new Upgrade { Id = CoiledStrike, Name = "Coiled Strike", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { LanceTheWound }, X = -2.4f, Y = 7.8f, Icon = UpgradeIcons.Arrow,
                Description = "Hold the spell button to strike through the enemy you face, cutting it and poisoning it deeply. If the cut lands, your next heavy blow in the moment after is critical. 35 mana.",
                Form = FormStep,
                Spell = new SpellSpec { Cost = 35f, Reach = 6f, Damage = 24f, Stagger = 25f, Stacks = 4, Seconds = 3f } });
            // Poison goes twelve deeper (not without limit) and does not fade (Afflictions.PoisonKeeps). Nothing closes Volatile to one who holds this.
            to.Add(new Upgrade { Id = SlowDeath, Name = "Slow Death", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { CoiledStrike }, X = -1.93f, Y = 9.18f, Icon = UpgradeIcons.Skull,
                Description = "Your weapon wounds far less. Your poison goes twelve doses deeper, and it does not fade while its bearer lives.",
                Stats = new[] { new StatMod(PlayerStat.WeaponDamage, -0.3f), new StatMod(PlayerStat.AfflictionDepth, 12f) } });

            // Middle bough: Rupture. Bursts.

            // It bursts poison of any depth, not only poison at its full depth.
            to.Add(new Upgrade { Id = Rupture, Name = "Rupture", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Venom }, X = 0f, Y = 2.9f, Icon = UpgradeIcons.Burst,
                Description = "A heavy blow on an enemy poisoned to its full depth bursts the poison. All of it lands at once, and the depth is spent.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.SetOff, Needs = AfflictionKind.Poison, NeedsFull = true, Affliction = AfflictionKind.Poison, Stagger = 4f, Radius = 1f, Cooldown = 0.3f } } });
            // Stands in: no stat reaches the damage of a burst alone, so everything that strikes an afflicted enemy deals more.
            to.Add(new Upgrade { Id = Ripe, Name = "Ripe", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Rupture }, X = 0.92f, Y = 4.83f, Icon = UpgradeIcons.Star,
                Description = "Bursting poison deals more.",
                Stats = new[] { new StatMod(PlayerStat.BurstDamage, 0.15f) } });
            // Stands in: no stat reaches the size of a burst, so spells reach further.
            to.Add(new Upgrade { Id = WiderRupture, Name = "Wider Rupture", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Rupture }, X = -0.92f, Y = 4.88f, Icon = UpgradeIcons.Ring,
                Description = "Bursts of poison reach further.",
                Stats = new[] { new StatMod(PlayerStat.BurstReach, 0.25f) } });
            // Every death bursts, poisoned or not, and passes on two doses, not the depth the dead one carried.
            to.Add(new Upgrade { Id = RottenFruit, Name = "Rotten Fruit", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Rupture }, X = 0f, Y = 4.7f, Icon = UpgradeIcons.Skull,
                Description = "A poisoned enemy that dies bursts. Those the burst reaches take its depth.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Poison, Do = UpgradeAction.Burst, Affliction = AfflictionKind.Poison, Damage = 10f, Stagger = 6f, Radius = 3.5f, Stacks = 1, CarriesDepth = true, Cooldown = 0.3f } } });
            to.Add(new Upgrade { Id = Buckling, Name = "Buckling", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { RottenFruit }, X = 0.84f, Y = 5.78f, Icon = UpgradeIcons.Hammer,
                Description = "Bursting poison breaks balance sooner.",
                Stats = new[] { new StatMod(PlayerStat.BurstStagger, 0.4f) } });
            // The blister is the brand form: the next blow of the weapon breaks it, light or heavy, at one size.
            to.Add(new Upgrade { Id = Blister, Name = "Blister", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { RottenFruit }, X = 0f, Y = 6.5f, Icon = UpgradeIcons.Orb,
                Description = "Hold the spell button to raise a blister on the enemy you face. Your next blow on it breaks it in a wide burst that poisons deeply. Wait too long and it withers. 40 mana.",
                Form = FormBrand,
                Spell = new SpellSpec { Cost = 40f, Reach = 8f, Seconds = 12f, Count = 1, Radius = 3f, Damage = 40f, Stagger = 30f, Stacks = 4 } });
            to.Add(new Upgrade { Id = SpentVenom, Name = "Spent Venom", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Blister }, X = 1.04f, Y = 7.1f, Icon = UpgradeIcons.Gem,
                Description = "Blows on poisoned enemies give more mana.",
                Stats = new[] { new StatMod(PlayerStat.ManaFromBlows, 0.2f, AfflictionKind.Poison) } });
            // BESPOKE: the raised hand, the line from enemy to enemy, each burst passing its depth to the next. Until then a burst rises under every enemy near.
            to.Add(new Upgrade { Id = OneByOne, Name = "One by One", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { Blister }, X = 0f, Y = 8.3f, Icon = UpgradeIcons.Chain,
                Description = "Hold the spell button and a burst rises under each of the six nearest enemies in turn, throwing them into the air. 50 mana.",
                Form = FormPillars,
                Spell = new SpellSpec { Cost = 50f, Count = 6, Reach = 10f, Radius = 2.5f, Damage = 30f, Stagger = 20f, Stacks = 0, Delay = 0.5f } });
            // The beat is stilled (Afflictions.PoisonBeatOff); what is full is set off. Nothing closes Slow Death to one who holds this.
            to.Add(new Upgrade { Id = Volatile, Name = "Volatile", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { OneByOne }, X = 0.26f, Y = 9.84f, Icon = UpgradeIcons.Burst,
                Description = "Your poison no longer bites on its beat. It builds until it is full, and then bursts by itself and begins again.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictionFull, Do = UpgradeAction.SetOff, Needs = P, Affliction = P, Damage = 1f, Stagger = 4f, Radius = 2.5f, Cooldown = 0.5f } } });

            // Right bough: Contagion. Spreading.

            // On the bite itself (UpgradeTrigger.PoisonBite): one dose leaps to an enemy within four metres.
            to.Add(new Upgrade { Id = Catching, Name = "Catching", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Venom }, X = 1.21f, Y = 2.4f, Icon = UpgradeIcons.Chain,
                Description = "Each time poison bites, one dose leaps to the nearest enemy within reach.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PoisonBite, Do = UpgradeAction.Pass, Affliction = P, Reach = 4f, Stacks = 1 } } });
            // There are no clouds: the words say what the number does.
            to.Add(new Upgrade { Id = StillAir, Name = "Still Air", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Catching }, X = 0.77f, Y = 3.52f, Icon = UpgradeIcons.Gust,
                Description = "Poison you lay lasts longer before it fades.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionSeconds, 0.25f) } });
            to.Add(new Upgrade { Id = WideCloud, Name = "Wide Cloud", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { Catching }, X = 1.88f, Y = 3.05f, Icon = UpgradeIcons.Ring,
                Description = "Your spells reach further.",
                Stats = new[] { new StatMod(PlayerStat.SpellReach, 0.25f) } });
            // Built in the beat (Afflictions.SharedBeat, kept by PlayerUpgrades): one beat for everything, each bite 15% stronger for every poisoned enemy beyond the first that bit on the beat before.
            to.Add(new Upgrade { Id = OneSickness, Name = "One Sickness", Element = E, Kind = UpgradeKind.Major, Requires = new[] { Catching }, X = 1.61f, Y = 4.2f, Icon = UpgradeIcons.Spiral,
                Description = "The poison in every enemy bites on the same beat. Each poisoned enemy beyond the first makes every bite stronger.",
                Stats = new[] { new StatMod(PlayerStat.ManaFromBlows, 0.1f, AfflictionKind.Poison) } });
            // LeapReach: how far a dose leaps (the Pass action of Catching, and every Arc effect).
            to.Add(new Upgrade { Id = LongLeap, Name = "Long Leap", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { OneSickness }, X = 2.39f, Y = 4.9f, Icon = UpgradeIcons.Arrow,
                Description = "Poison leaps further between enemies.",
                Stats = new[] { new StatMod(PlayerStat.LeapReach, 0.5f) } });
            // The censer leaves no smoke behind it yet.
            to.Add(new Upgrade { Id = SwingingCenser, Name = "Swinging Censer", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { OneSickness }, X = 2.0f, Y = 6.0f, Icon = UpgradeIcons.Ring,
                Description = "Hold the spell button to set a smoking censer swinging round you on its chain. It doses whatever it passes. 35 mana.",
                Form = FormDiscs,
                Spell = new SpellSpec { Cost = 35f, Count = 1, Radius = 3f, Seconds = 8f, Interval = 1.2f, Damage = 4f, Stagger = 4f, Stacks = 1 } });
            // It makes every spell cheaper by half as much, not the base spell alone.
            to.Add(new Upgrade { Id = ReadyPoison, Name = "Ready Poison", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { SwingingCenser }, X = 2.91f, Y = 6.74f, Icon = UpgradeIcons.Gem,
                Description = "Miasma costs less mana.",
                Stats = new[] { new StatMod(PlayerStat.BaseSpellCost, -0.2f) } });
            // It doses everyone passed two deep; it does not yet raise them all to the depth of the sickest.
            to.Add(new Upgrade { Id = ShareAlike, Name = "Share Alike", Element = E, Kind = UpgradeKind.Spell, Requires = new[] { SwingingCenser }, X = 2.4f, Y = 7.8f, Icon = UpgradeIcons.Comet,
                Description = "Hold the spell button to rush through everything before you, untouchable. Every enemy you pass is struck and poisoned two deep. 40 mana.",
                Form = FormDrill,
                Spell = new SpellSpec { Cost = 40f, Reach = 7f, Radius = 1f, Count = 1, Damage = 10f, Stagger = 10f, Stacks = 2, Speed = 20f } });
            // The words say what the number does.
            to.Add(new Upgrade { Id = SweetFever, Name = "Sweet Fever", Element = E, Kind = UpgradeKind.Minor, Requires = new[] { ShareAlike }, X = 1.53f, Y = 8.6f, Icon = UpgradeIcons.Plus,
                Description = "Fever lasts longer.",
                Stats = new[] { new StatMod(PlayerStat.SurgeSeconds, 0.2f) } });
            to.Add(new Upgrade { Id = PlagueBearer, Name = "Plague Bearer", Element = E, Kind = UpgradeKind.Keystone, Requires = new[] { ShareAlike }, X = 2.59f, Y = 9.3f, Icon = UpgradeIcons.Fang,
                Description = "You carry the sickness. Enemies near you are poisoned without a blow, deeper the longer they stay. Your flasks heal less.",
                Stats = new[] { new StatMod(PlayerStat.FlaskHeal, -0.3f) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.EverySeconds, Every = 1.5f, Do = UpgradeAction.Afflict, At = EffectAt.Self, Affliction = P, Stacks = 1, Radius = 4f } } });

            // BESPOKE (built): CleanBladeForm; the beat is held through Afflictions.PoisonWaits.
            to.Add(new Upgrade { Id = CleanBlade, Name = "Clean Blade", Element = E, Kind = UpgradeKind.Signature, X = 0f, Y = 12f, Icon = UpgradeIcons.Crown,
                Description = "While Fever lasts the poison waits, and every blow doses twice. When it ends you flick the blade clean, and every bite that was owed lands at once.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AnyHit, Do = UpgradeAction.Afflict, WhileSurge = true, Affliction = AfflictionKind.Poison, Stacks = 1 }, new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.CleanBlade, Spell = new SpellSpec { Cost = 0f, Stagger = 20f, Reach = 14f } } } });
        }
    }
}

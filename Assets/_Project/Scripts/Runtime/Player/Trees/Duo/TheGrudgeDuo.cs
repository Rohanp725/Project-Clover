using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Dark and Lightning (pairs_d3.md, "Dark and Lightning", distance 3; task 4.16), named by its
    /// reaction, The Grudge. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its
    /// Shape mark opens First Strike and Ash Plume; Grievance and The Exile Returns (majors), Long Memory (a minor), the
    /// keystone Given Bones, the duo spell The Exile's Grudge and the duo surge Ash and Thunder. Dark is written
    /// first, as the ring names the pair.
    /// </summary>
    public static class TheGrudgeDuo
    {
        public const string Shape = "thegrudge.shape";
        public const string Grievance = "thegrudge.grievance";
        public const string TheExileReturns = "thegrudge.exilereturns";
        public const string LongMemory = "thegrudge.longmemory";
        public const string GivenBones = "thegrudge.givenbones";
        public const string TheExilesGrudge = "thegrudge.exilesgrudge";
        public const string AshAndThunder = "thegrudge.ashthunder";
        /// <summary>The conditions the two shapes set, each the cloud as the plain reaction's.</summary>
        public const string FirstStrike = "thegrudge.firststrike";
        public const string AshPlume = "thegrudge.plume";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Dark, B = Elements.Lightning;

            // Ash Plume's storm is called down by the enemy's own blow at you (the their-blow moment, ConditionAttacked), or
            // falls when its 6 s run out, one bolt for each weapon blow it took, five at most (the owner's answer of
            // 2026-11-20). NEEDS: the cloud drifting to the nearest enemy if its host dies (drift-on-death).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "The Grudge: First Strike or Ash Plume", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens The Grudge's two shapes, chosen at a rest fire. First Strike: your first heavy blow calls the cloud down, so you need not be struck. Ash Plume: the cloud grows with your blows and falls as a storm of bolts. Either runs instead of the plain Grudge.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "First Strike",
                        Description = "The black cloud gathers over it for 8 s, and your first heavy blow on it calls it down: a black bolt that discharges all its Static at double, 14 a stack in 4 m, and stops it where it stands for 0.6 s.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = FirstStrike, Seconds = 8f, EndsWith = ConditionBlow.Heavy } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = FirstStrike, By = StruckBy.Heavy, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 4f, Damage = 7f, Stagger = 5f },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = FirstStrike, By = StruckBy.Heavy, Do = UpgradeAction.Hold, Seconds = 0.6f },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Ash Plume",
                        Description = "The black cloud gathers over it for 6 s, and each of your weapon blows on it adds a flash inside, five at most. When it next strikes at you, or its 6 s run out, it falls as a storm: one bolt for each flash round it, 10 each and a Static, and its Static discharged in 4 m.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = AshPlume, Seconds = 6f, EndsWith = ConditionBlow.TheirBlow } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionAttacked, Condition = AshPlume, Do = UpgradeAction.Falling, PerStack = true, PerMost = AshPlumeMost, Count = 1, Radius = 1.5f, Delay = 0.3f, Damage = 10f, Stagger = 8f, Affliction = AfflictionKind.Static, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionAttacked, Condition = AshPlume, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 4f, Stagger = 5f },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = AshPlume, Do = UpgradeAction.Falling, PerStack = true, PerMost = AshPlumeMost, Count = 1, Radius = 1.5f, Delay = 0.3f, Damage = 10f, Stagger = 8f, Affliction = AfflictionKind.Static, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = AshPlume, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 4f, Stagger = 5f },
                        },
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Grievance, Name = "Grievance", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Shield,
                Description = "A blow held on your guard against a Cursed enemy lays two Static on it.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Guard, Needs = AfflictionKind.Cursed, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 2, Cooldown = 0.3f } },
            }, HeartPattern.LeftLow));

            // Answers the Grudge's own bolt alone (the owner's answer of 2026-11-20), in whichever shape calls it down: the
            // plain cloud on the enemy's blow, First Strike's on your heavy blow, Ash Plume's storm. Done before the bolt, it
            // counts the Static the bolt is about to discharge.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheExileReturns, Name = "The Exile Returns", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Plus,
                Description = "The Grudge's bolt heals you 1 for each Static it discharged and Curses everything within 3 m.",
                Effects = AtTheBolt(
                    new UpgradeEffect { Do = UpgradeAction.Spend, Affliction = AfflictionKind.Static, CountOnly = true },
                    new UpgradeEffect { Do = UpgradeAction.Heal, Amount = 1f, PerStack = true },
                    new UpgradeEffect { Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Cursed, Stacks = 1 }),
            }, HeartPattern.RightLow));

            // NEEDS: an affliction lasting as long as another on the same enemy (fades-never); until then it does nothing.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = LongMemory, Name = "Long Memory", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { Grievance }, Icon = UpgradeIcons.Hourglass,
                Description = "A Curse on a charged enemy lasts as long as its Static does.",
            }, HeartPattern.LeftHigh));

            // The Grudge's bolt alone (the owner's answer of 2026-11-20) lands twice as hard: a second burst as large as the
            // bolt's discharge (14 a Static, counted before the bolt spends it), paid in grey health.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = GivenBones, Name = "Given Bones", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { Grievance, TheExileReturns }, Icon = UpgradeIcons.Skull,
                Description = "The Grudge's bolt costs you 2 health, left grey, and lands twice as hard.",
                Effects = AtTheBolt(
                    new UpgradeEffect { Do = UpgradeAction.SpendHealth, Amount = 2f },
                    new UpgradeEffect { Do = UpgradeAction.Spend, Affliction = AfflictionKind.Static, CountOnly = true },
                    new UpgradeEffect { Do = UpgradeAction.Burst, Radius = 4f, Damage = 14f, Stagger = 5f, PerStack = true, Affliction = AfflictionKind.Static }),
            }, HeartPattern.Keystone));

            // The duo spell, bought with Shards once both are carried (no god gives it): a counter on a 12 s cooldown (SpellTimings). The parry form stands for 3 s and answers
            // what strikes it; the answer Curses (the branch's first element) and charges three deep.
            // NEEDS: every attacker tied by a thread and paid back twice what it dealt (60 at most) when the cloud breaks, a
            // blow on the guard counting double (damage-tally).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheExilesGrudge, Name = "The Exile's Grudge", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Counter, Requires = new[] { Grievance, TheExileReturns }, Icon = UpgradeIcons.Bolt,
                Description = "A black cloud stands over you for 3 s. Whoever strikes you or your guard in that time is answered when the cloud breaks: a bolt that Curses and charges it three deep.",
                Form = SpellForms.Parry,
                Spell = new SpellSpec { Seconds = 3f, Reach = 4f, Arc = 120f, Damage = 12f, Stagger = 15f, Stacks = 1 },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 3 } },
            }, HeartPattern.Spell));

            // The duo surge, worn in the Hand's surge slot with its own 40 mana and 10 s (DuoSurge); each kill while it burns
            // adds a second (LengthenSurge). NEEDS: the bolt on every third blow you land, at the nearest Cursed enemy within
            // 8 m (count-rules, aim-at-afflicted); now every third blow on each Cursed enemy struck, counted on it.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = AshAndThunder, Name = "Ash and Thunder", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { TheExilesGrudge }, Icon = UpgradeIcons.Comet,
                Description = "A duo surge for your surge slot: 40 mana, 10 seconds. While it lasts a plume of ash streams up off your shoulders: every blow you land lays a Static, every third calls a bolt down on a Cursed enemy, and each Cursed enemy killed heals you 6 more. Each kill makes the surge last 1 second longer.",
                Surge = new DuoSurge { Mana = 40f, Seconds = 10f },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Kill, WhileSurge = true, Do = UpgradeAction.LengthenSurge, Amount = 1f },
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, WhileSurge = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, WhileSurge = true, Needs = AfflictionKind.Cursed, EveryOnEnemy = 3, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 2f, Damage = 2f, Stagger = 5f },
                    new UpgradeEffect { On = UpgradeTrigger.Kill, WhileSurge = true, Needs = AfflictionKind.Cursed, Do = UpgradeAction.Heal, Amount = 6f },
                },
            }, HeartPattern.RightTop));
        }

        /// <summary>The most flashes Ash Plume's storm throws (the owner's answer of 2026-11-20).</summary>
        public const int AshPlumeMost = 5;

        /// <summary>
        /// The moments the Grudge's bolt falls in: the plain cloud on the enemy's blow at you, First Strike's on your heavy
        /// blow, Ash Plume's storm on the enemy's blow or at its end. Each effect given is copied onto each moment, all of
        /// them answering the condition whoever set it (<see cref="UpgradeEffect.AnyMark"/>), at most once in 0.3 s.
        /// </summary>
        public static UpgradeEffect[] AtTheBolt(params UpgradeEffect[] effects)
        {
            var moments = new (UpgradeTrigger on, string condition, StruckBy by)[]
            {
                (UpgradeTrigger.ConditionAttacked, Trees.Reactions.Cloud, StruckBy.Any),
                (UpgradeTrigger.ConditionStruck, FirstStrike, StruckBy.Heavy),
                (UpgradeTrigger.ConditionAttacked, AshPlume, StruckBy.Any),
                (UpgradeTrigger.ConditionEnded, AshPlume, StruckBy.Any),
            };
            var all = new UpgradeEffect[moments.Length * effects.Length];
            int at = 0;
            foreach (var moment in moments)
                foreach (var effect in effects)
                    all[at++] = new UpgradeEffect
                    {
                        On = moment.on, Condition = moment.condition, By = moment.by, AnyMark = true, Cooldown = 0.3f,
                        Do = effect.Do, Affliction = effect.Affliction, CountOnly = effect.CountOnly, Amount = effect.Amount, PerStack = effect.PerStack,
                        Radius = effect.Radius, Stacks = effect.Stacks, Damage = effect.Damage, Stagger = effect.Stagger,
                    };
            return all;
        }
    }
}

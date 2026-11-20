using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Dark and Lightning (pairs_d3.md, "Dark and Lightning", distance 3; task 4.16), named by its
    /// reaction, The Grudge. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its
    /// Shape mark opens First Strike and Ash Plume; Grievance and The Exile Returns (majors), Long Memory (a minor), the
    /// keystone Given Bones, the duo spell The Exile's Grudge (the crown) and the duo surge Ash and Thunder. Dark is written
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

            // NEEDS: Ash Plume's storm called down by the enemy's own blow and its flashes capped at five (now it falls when
            // its 6 s run out, one bolt for each weapon blow it took, as ConditionEnded carries them); the cloud drifting to
            // the nearest enemy if its host dies (their-blow-moment covers the plain cloud's own call).
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
                        Description = "The black cloud gathers over it for 6 s, and each of your weapon blows on it adds a flash inside. Then it falls as a storm: one bolt for each flash round it, 10 each and a Static, and its Static discharged in 4 m.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = AshPlume, Seconds = 6f } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionEnded, Condition = AshPlume, Do = UpgradeAction.Falling, PerStack = true, Count = 1, Radius = 1.5f, Delay = 0.3f, Damage = 10f, Stagger = 8f, Affliction = AfflictionKind.Static, Stacks = 1 },
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

            // NEEDS: an answer to the Grudge's own bolt (now any discharge on a cursed enemy).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheExileReturns, Name = "The Exile Returns", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Plus,
                Description = "The Grudge's bolt heals you 1 for each Static it discharged and Curses everything within 3 m.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SetOff, Spent = AfflictionKind.Static, Needs = AfflictionKind.Cursed, Do = UpgradeAction.Heal, Amount = 1f, PerStack = true, Cooldown = 0.3f },
                    new UpgradeEffect { On = UpgradeTrigger.SetOff, Spent = AfflictionKind.Static, Needs = AfflictionKind.Cursed, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Cursed, Stacks = 1, Cooldown = 0.3f },
                },
            }, HeartPattern.RightLow));

            // NEEDS: an affliction lasting as long as another on the same enemy (fades-never); until then it does nothing.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = LongMemory, Name = "Long Memory", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { Grievance }, Icon = UpgradeIcons.Hourglass,
                Description = "A Curse on a charged enemy lasts as long as its Static does.",
            }, HeartPattern.LeftHigh));

            // A discharge on a Cursed enemy lands twice as hard (a second burst of the same size) and is paid in grey health.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = GivenBones, Name = "Given Bones", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { Grievance, TheExileReturns }, Icon = UpgradeIcons.Skull,
                Description = "A discharge on a Cursed enemy costs you 2 health, left grey, and lands twice as hard.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SetOff, Spent = AfflictionKind.Static, Needs = AfflictionKind.Cursed, Do = UpgradeAction.SpendHealth, Amount = 2f, Cooldown = 0.3f },
                    new UpgradeEffect { On = UpgradeTrigger.SetOff, Spent = AfflictionKind.Static, Needs = AfflictionKind.Cursed, Do = UpgradeAction.Burst, Radius = 4f, Damage = 7f, Stagger = 5f, PerStack = true, Affliction = AfflictionKind.Static, Cooldown = 0.3f },
                },
            }, HeartPattern.Keystone));

            // The duo spell, the crown: a counter on a 12 s cooldown (SpellTimings). The parry form stands for 3 s and answers
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

            // The duo surge. NEEDS: its own 40 mana and 10 s in the Hand's surge slot (duo-surge-slot), the bolt on every third
            // blow (now one blow in three by chance, on a cursed enemy struck), a second more for each kill.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = AshAndThunder, Name = "Ash and Thunder", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { TheExilesGrudge }, Icon = UpgradeIcons.Comet,
                Description = "A duo surge. While your surge lasts a plume of ash streams up off your shoulders: every blow you land lays a Static, every third calls a bolt down on a Cursed enemy, and each Cursed enemy killed heals you 6 more.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, WhileSurge = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.AnyHit, WhileSurge = true, Needs = AfflictionKind.Cursed, Chance = 0.34f, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 2f, Damage = 2f, Stagger = 5f },
                    new UpgradeEffect { On = UpgradeTrigger.Kill, WhileSurge = true, Needs = AfflictionKind.Cursed, Do = UpgradeAction.Heal, Amount = 6f },
                },
            }, HeartPattern.RightTop));
        }
    }
}

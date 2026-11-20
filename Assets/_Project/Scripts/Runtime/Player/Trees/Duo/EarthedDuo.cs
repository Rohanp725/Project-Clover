using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Lightning and Earth (pairs_d5.md, "Lightning and Earth", distance 5; task 4.17), named by its
    /// reaction, Earthed. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape
    /// mark, Ground Strike, opens Thunderstone and Step Current; The Watch Kept (a major), Deep Earth (a minor), the keystone
    /// The Ground Answers, the duo spell The Capstone and the duo surge When the Watch Sleeps. Lightning is written first, as
    /// the ring names the pair.
    /// </summary>
    public static class EarthedDuo
    {
        public const string Shape = "earthed.shape";
        public const string TheWatchKept = "earthed.watchkept";
        public const string DeepEarth = "earthed.deepearth";
        public const string TheGroundAnswers = "earthed.groundanswers";
        public const string TheCapstone = "earthed.capstone";
        public const string WhenTheWatchSleeps = "earthed.watchsleeps";
        /// <summary>The conditions the two shapes set (Thunderstone's held charge and the root each gives, as the plain
        /// reaction's), and the stone The Capstone leaves on the enemy it pins.</summary>
        public const string Thunderstone = "earthed.thunderstone";
        public const string StoneRooted = "earthed.thunderstone.rooted";
        public const string CurrentRooted = "earthed.stepcurrent.rooted";
        public const string Pinned = "earthed.capstone.stone";
        private const string Reaction = "react.earthed";

        // Laid as stacks, more than any affliction holds: "full Tremor", whatever the depth is.
        private const int ToFull = 99;

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Lightning, B = Elements.Earth;

            // NEEDS: standing-things (Thunderstone's knee-high stone of glass standing where the enemy stood, set off by a
            // heavy blow on it or any discharge within 4 m; now the enemy itself keeps its charge 6 s and your heavy blow on it
            // throws it); ground-states (Step Current's figure biting only what moves on it; now everything on it).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Earthed: Thunderstone or Step Current", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Earthed's two shapes, chosen at a rest fire. Thunderstone: the charge does not spread but is held in a stone until a heavy blow throws it. Step Current: the figure spreads wider and stays live, biting what moves on it. Either runs instead of the plain Earthed.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Thunderstone",
                        Description = "The charge does not spread: it fuses into a knee-high stone of glassy rock where the enemy stood, holding its Static for 6 s. A heavy blow on it, or any discharge within 4 m, throws the stored charge as bolts at every enemy within 6 m, 7 a stack and a Static each.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Thunderstone, Seconds = 6f, Keeps = AfflictionKind.Static, EndsWith = ConditionBlow.Heavy } },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = StoneRooted, Seconds = 1f, Flags = ConditionFlags.Rooted } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Thunderstone, By = StruckBy.Heavy, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 6f, Damage = 7f, Stagger = 5f },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Thunderstone, By = StruckBy.Heavy, Do = UpgradeAction.Afflict, Radius = 6f, Affliction = AfflictionKind.Static, Stacks = 1 },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Step Current",
                        Description = "All its Static runs into the floor as before, but the figure spreads to 6 m and stays live for 3 s: any enemy that moves on it takes 4 and 6 stagger every half second, and one that stands still is spared.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Static },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Radius = 6f, Damage = 5f, Stagger = 3f, PerStack = true, Affliction = AfflictionKind.Static },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Burst, Radius = 6f, Affliction = AfflictionKind.Tremor, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Radius = 6f, Seconds = 3f, Interval = 0.5f, Damage = 4f, Stagger = 6f, Affliction = AfflictionKind.Static, SparesTarget = true },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = CurrentRooted, Seconds = 1f, Flags = ConditionFlags.Rooted } },
                        },
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheWatchKept, Name = "The Watch Kept", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard against an enemy that carries Tremor lays two Static on it.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Tremor, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 2, Cooldown = 0.3f } },
            }, HeartPattern.LeftLow));

            // NEEDS: a mark that changes the reaction's own numbers (reaction-numbers); until then it does nothing.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = DeepEarth, Name = "Deep Earth", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { Shape }, Icon = UpgradeIcons.Hourglass,
                Description = "Earthed's figure is 1 m wider, and the grounded enemy is rooted 0.5 s longer.",
            }, HeartPattern.RightLow));

            // NEEDS: the player's raised rocks as things that hold charge (standing-things): Static discharged within 4 m of
            // one runs into it and out of every other rock standing, 7 a stack in 2.5 m round each; now a discharge on a
            // shaken enemy bursts again round it, 7 a stack in 2.5 m, once in half a second.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheGroundAnswers, Name = "The Ground Answers", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { TheWatchKept, DeepEarth }, Icon = UpgradeIcons.Rock,
                Description = "Every rock you raise holds charge. Static discharged within 4 m of one runs into it and out of every other rock you have standing: 7 a stack in 2.5 m round each.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SetOff, Spent = AfflictionKind.Static, Needs = AfflictionKind.Tremor, Do = UpgradeAction.Burst, Radius = 2.5f, Damage = 7f, Stagger = 5f, PerStack = true,
                        Affliction = AfflictionKind.Static, Cooldown = 0.5f },
                },
            }, HeartPattern.Keystone));

            // The duo spell, a breaker on a 12 s cooldown (SpellTimings), bought with 5 Shards once both are carried. The sword
            // form drops the stone on the enemy faced after 0.4 s and charges it (the branch's first element); it is shaken to
            // full and pinned, and for the 5 s the stone stands each weapon blow on it calls a bolt down into the ground.
            // NEEDS: a round-headed stone with a straw rope that stands 5 s (form-pieces; now the sword's fall and crack), and
            // blows on any enemy within 2 m of it calling the bolt (now blows on the enemy it pinned).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheCapstone, Name = "The Capstone", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Breaker, Requires = new[] { TheWatchKept, DeepEarth }, Icon = UpgradeIcons.Rock,
                Description = "A round-headed stone falls on the enemy you face: 18 and 40 stagger, Tremor to full, and it is pinned 2 s. While the stone stands, each weapon blow near it calls a bolt down it into the ground: 6, 12 stagger and a Static in 2 m.",
                Form = SpellForms.Sword,
                Spell = new SpellSpec { Reach = 7f, Delay = 0.4f, Radius = 1.5f, Damage = 18f, Stagger = 40f, Stacks = 1, Seconds = 5f, LingerDamage = 2f },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Tremor, Stacks = ToFull },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Hold, Seconds = 2f },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Pinned, Seconds = 5f } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Pinned, By = StruckBy.Weapon, Do = UpgradeAction.Burst, Radius = 2f, Damage = 6f, Stagger = 12f,
                        Affliction = AfflictionKind.Static, Stacks = 1, Cooldown = 0.5f },
                },
            }, HeartPattern.Spell));

            // The duo surge. NEEDS: its own 40 mana and 10 s in the Hand's surge slot (duo-surge-slot); now it rides any surge.
            // While it lasts each heavy blow shakes the floor round the enemy struck, and one carrying Static is Earthed at once.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = WhenTheWatchSleeps, Name = "When the Watch Sleeps", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { TheCapstone }, Icon = UpgradeIcons.Hammer,
                Description = "A duo surge. While your surge lasts the god looks away and the ground is loose: every heavy blow shakes the floor in 3 m round the enemy struck, any enemy shaken while it carries Static is Earthed at once, and heavy blows deal 8 more stagger.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, WhileSurge = true, Do = UpgradeAction.Afflict, Radius = 3f, Affliction = AfflictionKind.Tremor, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, WhileSurge = true, Do = UpgradeAction.Burst, Radius = 0.3f, Stagger = 8f, Affliction = AfflictionKind.Tremor },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, WhileSurge = true, Needs = AfflictionKind.Static, Do = UpgradeAction.ForceReaction, Reaction = Reaction, Cooldown = 0.5f },
                },
            }, HeartPattern.RightTop));
        }
    }
}

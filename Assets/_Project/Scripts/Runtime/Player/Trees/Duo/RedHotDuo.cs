using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Metal and Fire (pairs_d4.md, "Metal and Fire", distance 4; task 4.17), named by its reaction, Red
    /// Hot. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape mark, Slag or
    /// Shirt, opens Slag and The Burning Shirt; Heated Edge and Strike While Hot (majors), the keystone Red-Short, the duo
    /// spell Tipped Crucible (paid in mana, as the pair file gives it) and the duo surge Heated Bronze, each bought with
    /// Shards once both are carried. Metal is written first, as the ring names the pair.
    /// </summary>
    public static class RedHotDuo
    {
        public const string Shape = "redhot.shape";
        public const string HeatedEdge = "redhot.heatededge";
        public const string StrikeWhileHot = "redhot.strikewhilehot";
        public const string RedShort = "redhot.redshort";
        public const string TippedCrucible = "redhot.crucible";
        public const string HeatedBronze = "redhot.heatedbronze";
        /// <summary>The conditions the two shapes set (Slag's running armour, The Burning Shirt's clinging plates), Strike While
        /// Hot's and Red-Short's own red heat, and the armour Red-Short crumbles.</summary>
        public const string Slag = "redhot.slag";
        public const string Shirt = "redhot.shirt";
        public const string Hot = "redhot.hot";
        public const string Short = "redhot.short";
        public const string Crumbled = "redhot.crumbled";
        private const string Reaction = "react.redhot";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Metal, B = Elements.Fire;

            // NEEDS: ground laid along the path an enemy walks (plume-along-path): Slag's drip for each metre it moves; now a
            // drip each second where it stands. An enemy beginning an attack as a moment (enemy-begins-attack): The Burning
            // Shirt deepening its Burn by two and making it flinch with each attack it begins, and unable to guard only while it
            // burns five deep; now a Burn every 1.5 s and no guard for the 5 s. As the plain one, burns biting twice as hard
            // while it glows (burn-bite-share).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Red Hot: Slag or The Burning Shirt", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Red Hot's two shapes, chosen at a rest fire. Slag: the armour runs and drips fire where it walks. The Burning Shirt: no sparks; the hot plates cling and sear it, and it cannot guard. Either runs instead of the plain Red Hot.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Slag",
                        Description = "The notched armour glows red for 5 s and runs: every second it drips molten slag where it stands, a pool 0.8 m round that burns for 3 s, 3 and a Burn each second to whatever stands in it but its own wearer. Walk it through its friends.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Slag, Seconds = 5f, Beat = 1f } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Slag, Do = UpgradeAction.Hazard, Radius = 0.8f, Seconds = 3f, Interval = 1f, Damage = 3f,
                                Affliction = AfflictionKind.Burn, Stacks = 1, SparesTarget = true },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "The Burning Shirt",
                        Description = "No sparks: the hot plates cling. For 5 s it cannot guard, and every 1.5 s the metal sears it, a Burn deeper.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Shirt, Seconds = 5f, Beat = 1.5f, Flags = ConditionFlags.CannotGuard } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Shirt, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1 },
                        },
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = HeatedEdge, Name = "Heated Edge", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Anvil,
                Description = "Your heavy blows lay a Burn and a notch together.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Sundered, Stacks = 1 },
                },
            }, HeartPattern.LeftLow));

            // NEEDS: an answer to an enemy in a condition another mark set (condition-needs): the red-hot enemy; now Strike While
            // Hot keeps its own 5 s from each Red Hot.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = StrikeWhileHot, Name = "Strike While Hot", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Hammer,
                Description = "Each heavy blow on a red-hot enemy gives 4 mana and takes 0.5 s off every running spell cooldown.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Hot, Seconds = 5f } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Hot, By = StruckBy.Heavy, Do = UpgradeAction.Mana, Amount = 4f },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Hot, By = StruckBy.Heavy, Do = UpgradeAction.CutCooldowns, Amount = 0.5f },
                },
            }, HeartPattern.RightLow));

            // NEEDS: a finisher as the blow a condition answers (finisher-only): now the first heavy blow on the red-hot enemy
            // crumbles it, once for each Red Hot. And the red heat read from the enemy (condition-needs); now its own 5 s.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = RedShort, Name = "Red-Short", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { HeatedEdge, StrikeWhileHot }, Icon = UpgradeIcons.Burst,
                Description = "A finisher on a red-hot enemy crumbles its armour: every notch is spent for 15 damage each, and for 10 s it takes 30% more from your weapon.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Short, Seconds = 5f, EndsWith = ConditionBlow.Heavy } },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Short, By = StruckBy.Heavy, Do = UpgradeAction.Spend, Affliction = AfflictionKind.Sundered },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Short, By = StruckBy.Heavy, Do = UpgradeAction.Burst, Radius = 0.5f, DamagePerSpent = 15f, Affliction = AfflictionKind.Sundered },
                    new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = Short, By = StruckBy.Heavy, Do = UpgradeAction.Condition,
                        State = new ConditionSpec { Id = Crumbled, Seconds = 10f, Flags = ConditionFlags.WeaponTakenShare, Scale = 1.3f } },
                },
            }, HeartPattern.Keystone));

            // The duo spell, bought with 5 Shards once both are carried (no god gives it), and paid in mana as the pair file
            // gives it (SpellTimings, 35): a breaker. The phalanx form hangs the pours at your shoulder for 8 s, cast in Metal
            // (a notch each); each that lands lays a Burn and leaves the ground burning under its enemy.
            // NEEDS: a crucible at your shoulder tipped along each heavy swing as a tongue of pour 4 m long and 1.2 m wide,
            // cooling from white-gold to black over 3 s, four at most (form-pieces), only while the spell stands
            // (while-spell-stands); now four pours that fly by themselves, or all at once on a heavy blow.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TippedCrucible, Name = "Tipped Crucible", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Breaker, Requires = new[] { HeatedEdge, StrikeWhileHot }, Icon = UpgradeIcons.Flask,
                Description = "A crucible of molten bronze hangs at your shoulder for 8 s, four pours in it. Each pour that lands strikes 14 and 25 stagger, a Burn and a notch, and the ground burns under it while the bronze is red. A heavy blow tips all that are left. 35 mana.",
                Form = SpellForms.Phalanx,
                Spell = new SpellSpec { Count = 4, Seconds = 8f, Reach = 6f, Damage = 14f, Stagger = 25f, Stacks = 1, Speed = 12f },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Cooldown = 0.3f, Do = UpgradeAction.Hazard, Radius = 0.8f, Seconds = 3f, Interval = 0.5f, Damage = 3f,
                        Affliction = AfflictionKind.Burn, Stacks = 1 },
                },
            }, HeartPattern.Spell));

            // The duo surge. NEEDS: its own 40 mana and 10 s in the Hand's surge slot (duo-surge-slot); now its answers ride any
            // surge while both are carried. The perfect guard's grip, held 1 s before the throw, waits for a push that comes
            // after a hold (form-pieces); now it is burned and thrown at once.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = HeatedBronze, Name = "Heated Bronze", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { TippedCrucible }, Icon = UpgradeIcons.Shield,
                Description = "A duo surge. While your surge lasts you glow red to the bone like the bronze guardian: every blow that lands on you or your guard burns and notches the one who struck, your guard costs 30% less stamina, and a perfect guard grips the attacker, burns it three deep and throws it 2 m.",
                Stats = new[] { new StatMod(PlayerStat.GuardStamina, -0.3f, StatWhen.Surge) },
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Struck, WhileSurge = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.Struck, WhileSurge = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Sundered, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.Guard, WhileSurge = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.Guard, WhileSurge = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Sundered, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, WhileSurge = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 3 },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, WhileSurge = true, Do = UpgradeAction.Push, Shape = HazardShape.Downwind, Amount = 2f },
                },
            }, HeartPattern.RightTop));
        }
    }
}

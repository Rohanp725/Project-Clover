using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Plasma: the duo branch Fire and Lightning share, reshaped by pairs_d1_d2.md ("Fire and Lightning", task 4.5) and
    /// drawn in the chart's heart (<see cref="HeartPattern"/>). Its reaction is Flashover; the branch's Shape mark opens
    /// Flashover's two shapes, Corona and Fulgurite, chosen at a rest fire, and before it is bought the plain Flashover
    /// works. Charged Embers is cut (both afflictions are laid by carrying both; the Shape mark takes its place), Arc Fire
    /// folds into Corona, Fused Core is cut as a plain number, all three in <see cref="RetiredMarks"/>;
    /// Plasma Lance (Lightning's roster is full at six) is kept hidden for the Evolutions (<see cref="SpareSpells"/>). Kept under their ids: Twin Break, Chain Reaction, Meltdown (the
    /// keystone), Ionised Air (re-aimed at Flashover), Ball Lightning (the duo spell) and Plasma, bought from it, the branch's
    /// top mark. Open while both are carried (<see cref="MarkRules.Open(Upgrade, Build.Loadout, bool)"/>).
    /// </summary>
    public static class PlasmaTree
    {
        public const string Shape = "plasma.shape";
        public const string TwinBreak = "plasma.twin";
        public const string IonisedAir = "plasma.ionised";
        public const string ChainReaction = "plasma.chain";
        public const string Meltdown = "plasma.meltdown";
        public const string BallLightning = "plasma.ball";
        /// <summary>The condition Corona sets: violet flame crawling over the enemy, an arc leaping from it on each beat.</summary>
        public const string Corona = "flashover.corona";

        public static void Add(List<Upgrade> to)
        {
            // The first element is the earlier of the two round the ring, as the reaction names them.
            const string A = Elements.Fire, B = Elements.Lightning;

            // The Shape mark: Flashover's two shapes in place of Charged Embers. Corona keeps the live flame and throws arcs
            // from it; Fulgurite sends the crack down into the floor instead. Each runs instead of the plain Flashover.
            // NEEDS: discharges that bend toward one enemy (Corona's arcs leap from the flame as the plain reaction's would
            // bend into it); the scorched glassy patch's own look (Fulgurite is drawn as a burning and crackling patch).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Flashover: Corona or Fulgurite", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Flashover's two shapes, chosen at a rest fire. Corona: violet flame crawls over it and throws arcs at its neighbours. Fulgurite: the ground under it fuses into a glassy patch that burns and shocks. Either runs instead of the plain Flashover.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Corona",
                        Description = "Nothing is spent. Violet flame crawls over it for 4 s, and every 0.7 s an arc leaps from it to the nearest enemy within 5 m: 4 damage, 1 Burn and 1 Static.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = Corona, Seconds = 4f, Beat = 0.7f, Flags = ConditionFlags.FlameLive } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Corona, Do = UpgradeAction.Arc, FromTarget = true, Count = 1, Reach = 5f, Damage = 4f, Affliction = AfflictionKind.Burn, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionBeat, Condition = Corona, Do = UpgradeAction.Arc, FromTarget = true, Count = 1, Reach = 5f, Affliction = AfflictionKind.Static, Stacks = 1 },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Fulgurite",
                        Description = "The crack goes down instead of out: the ground under it fuses to a scorched glassy patch 2.5 m across for 4 s that bites 4 a second with 1 Burn and 1 Static. A place to fight on rather than a burst.",
                        Effects = new[]
                        {
                            // One patch laid as two: half the bite lays Burn, half Static. What it lays starts no reaction of its own.
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 1.25f, Seconds = 4f, Interval = 1f, Damage = 2f, Affliction = AfflictionKind.Burn, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 1.25f, Seconds = 4f, Interval = 1f, Damage = 2f, Affliction = AfflictionKind.Static, Stacks = 1 },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // An effect can ask for one affliction, not two: each is set off if it is there. The flinch that breaks an attack is
            // done by hand in PlayerUpgrades (Twin Break's burst). Breath of the Lioness, when learned, spends the burn first.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TwinBreak, Name = "Twin Break", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Shield,
                Description = "A perfect guard sets off Burn and Static together on the one who struck. Set off together, they strike half again as hard and stop it where it stands.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Burn, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Burn, Damage = 5f, Stagger = 5f, Radius = 3f },
                    new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Needs = AfflictionKind.Static, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Damage = 3.5f, Stagger = 5f, Radius = 3f },
                },
            }, HeartPattern.LeftLow));

            // Re-aimed by the pair file: no longer any full affliction, but Flashover itself (plain or shaped) leaping on.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = IonisedAir, Name = "Ionised Air", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Chain,
                Description = "When Flashover fires, an arc leaps on from the burning one through three more, 10 each and a Static.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = "react.flashover", Do = UpgradeAction.Arc, FromTarget = true, Count = 3, Reach = 5f, Damage = 10f, Stagger = 8f, Affliction = AfflictionKind.Static, Stacks = 1 },
                },
            }, HeartPattern.RightLow));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = ChainReaction, Name = "Chain Reaction", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { IonisedAir }, Icon = UpgradeIcons.Skull,
                Description = "An enemy that dies carrying both Burn and Static bursts, and passes both on at full depth.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Burn, Needs2 = AfflictionKind.Static, Do = UpgradeAction.Burst, Damage = 18f, Radius = 4f, Affliction = AfflictionKind.Burn, Stacks = 1, CarriesDepth = true, Cooldown = 0.3f },
                    new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Static, Needs2 = AfflictionKind.Burn, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 1, CarriesDepth = true, Radius = 4f, Cooldown = 0.3f },
                },
            }, HeartPattern.RightHigh));

            // The branch's keystone. Done by hand in PlayerUpgrades (the shaking light, then the burst); no data effects.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Meltdown, Name = "Meltdown", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { TwinBreak, ChainReaction }, Icon = UpgradeIcons.Burst,
                Description = "An enemy at five Burn and five Static begins to shake with light. A few moments later it bursts, far harder than either alone. Nothing stops it.",
            }, HeartPattern.Keystone));

            // The duo spell (BESPOKE, built: BallForm). Drawn as violet-cored plasma with real flame on its skin, never white
            // fire (pairs_d1_d2.md). Its pair's larger spell, paid in mana (SpellTimings).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = BallLightning, Name = "Ball Lightning", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Detonator, Requires = new[] { TwinBreak, IonisedAir }, Icon = UpgradeIcons.Orb,
                Description = "You loose a slow ball of plasma, violet at its heart with real flame on its skin. Strike it to send it where the blow points, larger each time, up to three. It bursts on the first enemy it meets. 40 mana.",
                Form = SpellForms.Ball,
                Spell = new SpellSpec { Cost = 40f, Speed = 2f, Seconds = 8f, Radius = 2.5f, Damage = 16f, Stagger = 20f, Stacks = 2, LingerDamage = 12f },
            }, HeartPattern.Spell));

            // The branch's top mark (pairs_d1_d2.md), bought with 5 Shards once both are carried, from Ball Lightning: gods give
            // no duo crowns (the owner's decision of 2026-11-20), so a Fire holder consuming the storm is not given it. the burst on a burning enemy is done by hand in
            // PlayerUpgrades under Upgrade.Plasma and spends nothing. Flashover goes off with it, whatever its rest: one Static
            // is laid just before, so its discharge always fires (the owner's answer of 2026-11-22, standin_clearance_plan.md
            // question 6). A heavy blow counts the finisher, so the finisher needs no effects of its own.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Upgrade.Plasma, Name = "Plasma", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { BallLightning }, Icon = UpgradeIcons.Crown,
                Description = "A heavy blow or a finisher on a burning enemy bursts, and sets alight everyone near. One Static is laid on it and Flashover goes off at once, whatever its rest. Nothing else is spent.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Burn, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Burn, Do = UpgradeAction.ForceReaction, Reaction = "react.flashover", Alone = true },
                },
            }, HeartPattern.Crown));
        }
    }
}

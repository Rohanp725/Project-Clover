using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Pyre: the duo branch Poison and Fire share, reshaped by pairs_d1_d2.md ("Poison and Fire", task 4.15) and drawn in the
    /// chart's heart (<see cref="HeartPattern"/>). Its reaction is Fire Medicine; the Shape mark opens its two shapes, Fire
    /// Lance and Flying Fire. Fuel is cut (the Shape mark takes its place), and Fire Eater and Lit Fuse, offered to Poison's
    /// and Fire's trees, are cut because those rosters are full; all three are in <see cref="RetiredMarks"/>. Goes Up is
    /// re-aimed at the reaction. Kept: Funeral Fire, Slow Burn (the keystone) and Light the Pyres, the god's gift at the top
    /// that is also the branch's spell. Open while both are carried.
    /// </summary>
    public static class PyreTree
    {
        public const string Shape = "pyre.shape";
        public const string GoesUp = "pyre.flashpoint";
        public const string FuneralFire = "pyre.funeralfire";
        public const string SlowBurn = "pyre.slowburn";
        public const string LightThePyres = "pyre.lightthepyres";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Poison, B = Elements.Fire;
            const AfflictionKind Poisoned = AfflictionKind.Poison, Burn = AfflictionKind.Burn;

            // NEEDS: a cone that leaves the enemy's far side (Fire Lance; now a lane 5 m long and 2 m wide beyond it).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Fire Medicine: Fire Lance or Flying Fire", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Fire Medicine's two shapes, chosen at a rest fire. Fire Lance: the bang is a jet of fire out of its far side. Flying Fire: the bang throws burning pellets that land round it as fire. Either runs instead of the plain Fire Medicine.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Fire Lance",
                        Description = "The bang is a jet out of its far side: after the 0.6 s fuse, a lane 5 m long beyond it takes 4 a dose and 6 a Burn stack, 30 stagger, a dose and a Burn. Half its doses are spent.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = Poisoned, CountOnly = true },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Lane, Reach = 5f, Radius = 1f, Delay = 0.6f, DamagePerSpent = 4f, Stagger = 30f, Affliction = Burn, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = Burn, CountOnly = true },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Lane, Reach = 5f, Radius = 1f, Delay = 0.6f, DamagePerSpent = 6f, Affliction = Poisoned, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = Poisoned, Share = 0.5f },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Flying Fire",
                        Description = "The bang throws five burning pellets that land in a ring 3 m out as fire patches 1 m across for 3 s, burning and poisoning what walks in. Half its doses are spent.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Shape = HazardShape.Ring, Count = 5, Reach = 3f, Radius = 0.5f, Seconds = 3f, Interval = 0.5f, Damage = 3f, Delay = 0.6f, Affliction = Burn, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hazard, Shape = HazardShape.Ring, Count = 5, Reach = 3f, Radius = 0.5f, Seconds = 3f, Interval = 1f, Damage = 0f, Delay = 0.6f, Affliction = Poisoned, Stacks = 1 },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Spend, Affliction = Poisoned, Share = 0.5f },
                        },
                    },
                },
            }, HeartPattern.Foot));

            // Re-aimed by the pair file: at full depth of poison, Fire Medicine spends nothing and strikes twice as hard.
            // NEEDS: a mark that changes the reaction's own numbers; now a second bang of the same size and the spent doses laid
            // back, on an enemy whose poison is full as it reacts.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = GoesUp, Name = "Goes Up", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Burst,
                Description = "When Fire Medicine goes off on an enemy at full depth of poison, it strikes twice as hard and spends nothing.",
                Effects = new[]
                {
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = "react.firemedicine", Needs = Poisoned, NeedsFull = true, Do = UpgradeAction.Burst, Radius = 2.5f, Delay = 0.65f, Damage = 32f, Stagger = 30f, Affliction = Burn, Stacks = 1 },
                    new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = "react.firemedicine", Needs = Poisoned, NeedsFull = true, Do = UpgradeAction.Afflict, Affliction = Poisoned, Stacks = 4 },
                },
            }, HeartPattern.LeftLow));

            // Every death does it, whatever the dead one carried.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = FuneralFire, Name = "Funeral Fire", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Skull,
                Description = "An enemy that dies poisoned and burning explodes, and leaves the ground burning where it fell.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Poison, Needs2 = AfflictionKind.Burn, Do = UpgradeAction.Burst, Affliction = AfflictionKind.Poison, Damage = 20f, Stagger = 20f, Radius = 3.5f, Stacks = 2, Cooldown = 0.5f }, new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Poison, Needs2 = AfflictionKind.Burn, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Burn, Stacks = 1, Radius = 3.5f, Cooldown = 0.5f }, new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Poison, Needs2 = AfflictionKind.Burn, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Affliction = AfflictionKind.Burn, Damage = 5f, Radius = 1.5f, Seconds = 4f, Interval = 0.5f, Stacks = 1, Cooldown = 0.5f } },
            }, HeartPattern.RightLow));

            // NOT BUILT: burn biting on the poison's beat, each stronger for the depth of the other. The words say what the number does.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = SlowBurn, Name = "Slow Burn", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { GoesUp, FuneralFire }, Icon = UpgradeIcons.Hourglass,
                Description = "Burn and poison both work harder.",
                Stats = new[] { new StatMod(PlayerStat.AfflictionPower, 0.3f) },
            }, HeartPattern.Keystone));

            // BESPOKE (built): PyresForm. The god's gift at the top, which the pair file names the branch's duo spell; kept a gift
            // so the crown a consumed god gives is still there (phase4_questions.md, task 4.15).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = LightThePyres, Name = "Light the Pyres", Element = A, Element2 = B, Kind = UpgradeKind.Signature, Icon = UpgradeIcons.Crown,
                Description = "You drive your weapon into the ground. A line of fire runs to every poisoned or burning enemy near. Stand through three breaths, and a pillar of flame rises under each. 60 mana.",
                Form = SpellForms.Pyres,
                Spell = new SpellSpec { Cost = 60f, Count = 6, Reach = 9f, Radius = 2f, Damage = 40f, Stagger = 40f, Stacks = 2, Delay = 2.2f },
            }, HeartPattern.Crown));
        }
    }
}

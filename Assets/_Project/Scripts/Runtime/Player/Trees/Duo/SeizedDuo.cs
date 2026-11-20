using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees.Duo
{
    /// <summary>
    /// The duo branch of Lightning and Water (pairs_d3.md, "Lightning and Water", distance 3; task 4.16), named by its
    /// reaction, Seized. Drawn in the chart's heart (<see cref="HeartPattern"/>) and open while both are carried. Its Shape
    /// mark opens Can't Let Go and Running Water; Wet Thread and Grounded (majors), Salt Water (a minor), the keystone The
    /// Sea Finds Land, and the duo spell The Flashing. Lightning is written first.
    /// </summary>
    public static class SeizedDuo
    {
        public const string Shape = "seized.shape";
        public const string WetThread = "seized.wetthread";
        public const string Grounded = "seized.grounded";
        public const string SaltWater = "seized.saltwater";
        public const string TheSeaFindsLand = "seized.seafindsland";
        public const string TheFlashing = "seized.flashing";
        /// <summary>The condition Can't Let Go sets: the lock that weapon blows hold longer.</summary>
        public const string CantLetGo = "seized.cantletgo";
        private const string Reaction = "react.seized";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Lightning, B = Elements.Water;

            // NEEDS: a film that runs outward as a ring, seizing what its edge crosses (Running Water; now all within 7 m at
            // once), and a hold capped at 3 s in all (Can't Let Go; now each blow in its 3 s adds 0.3 s).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Seized: Can't Let Go or Running Water", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Seized's two shapes, chosen at a rest fire. Can't Let Go: your blade keeps it locked. Running Water: no hold, and the live water runs outward and seizes what it crosses. Either runs instead of the plain Seized.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Can't Let Go",
                        Description = "Held 1 s as before, with no film; for 3 s each of your weapon blows on it holds it 0.3 s longer.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hold, Seconds = 1f },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Condition, State = new ConditionSpec { Id = CantLetGo, Seconds = 3f } },
                            new UpgradeEffect { On = UpgradeTrigger.ConditionStruck, Condition = CantLetGo, By = StruckBy.Weapon, Do = UpgradeAction.Hold, Seconds = 0.3f },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Running Water",
                        Description = "No hold: the water runs outward to 7 m, and every enemy it reaches is seized for 0.5 s and given a Static.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hold, Radius = 7f, Seconds = 0.5f },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Afflict, Radius = 7f, Affliction = AfflictionKind.Static, Stacks = 1 },
                        },
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = WetThread, Name = "Wet Thread", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Bolt,
                Description = "Light blows on a Soaked enemy lay one Static.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.LightHit, Needs = AfflictionKind.Soaked, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Static, Stacks = 1 } },
            }, HeartPattern.LeftLow));

            // NEEDS: live water as a place (ground-states): the discharge reaching all standing in it; now all within 4 m.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Grounded, Name = "Grounded", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Hammer,
                Description = "A heavy blow on a seized enemy discharges its Static into the floor: everything standing in live water takes the discharge in full.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Soaked, Needs2 = AfflictionKind.Static, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 4f, Stagger = 5f, Cooldown = 0.3f } },
            }, HeartPattern.RightLow));

            // NEEDS: a mark that changes the reaction's own numbers (reaction-numbers); now a second film for 2 s after the first.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = SaltWater, Name = "Salt Water", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 1, Requires = new[] { WetThread }, Icon = UpgradeIcons.Drop,
                Description = "The film of live water stays 2 s longer.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Reacted, Reaction = Reaction, Do = UpgradeAction.Hazard, Radius = 2.5f, Seconds = 2f, Delay = 3f, Interval = 0.5f, Affliction = AfflictionKind.Static, Stacks = 1 } },
            }, HeartPattern.LeftHigh));

            // NEEDS: a discharge by itself at full depth, through the water to the Soaked only (now to all within 8 m).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheSeaFindsLand, Name = "The Sea Finds Land", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { WetThread, Grounded }, Icon = UpgradeIcons.Wave,
                Description = "Static on a Soaked enemy needs no heavy blow: at full depth it discharges by itself, through the water, to every Soaked enemy within 8 m.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.AfflictionFull, Needs = AfflictionKind.Static, NeedsFull = true, Needs2 = AfflictionKind.Soaked, Do = UpgradeAction.SetOff, Affliction = AfflictionKind.Static, Radius = 8f, Stagger = 5f, Cooldown = 1f } },
            }, HeartPattern.Keystone));

            // The duo spell, bought with Shards once both are carried (no god gives it): an opener on a 10 s cooldown (SpellTimings). The bolt form leaps from enemy to enemy;
            // each it reaches is charged twice (the branch's first element) and Soaked, so it is seized at once.
            // NEEDS: a flash along the floor from the nearest Soaked enemy (aim-at-afflicted), the path staying wet 2 s, and
            // the next blow on the last reached discharging all the flash touched (the cast remembering its targets).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheFlashing, Name = "The Flashing", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Opener, Requires = new[] { WetThread, Grounded }, Icon = UpgradeIcons.Beam,
                Description = "You slap the floor and a flash runs across the ground to an enemy, then darts on to the next and the next, up to five. Each it reaches is struck, Soaked and charged twice, so it is seized at once.",
                Form = SpellForms.Bolt,
                Spell = new SpellSpec { Reach = 10f, Count = 4, Radius = 6f, Damage = 12f, Stagger = 20f, Stacks = 2 },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SpellHit, OwnSpell = true, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Soaked, Stacks = 1 } },
            }, HeartPattern.Spell));
        }
    }
}

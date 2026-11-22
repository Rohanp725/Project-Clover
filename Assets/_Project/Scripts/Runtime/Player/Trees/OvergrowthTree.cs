using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Overgrowth: the duo branch Life and Earth share, reshaped by pairs_d1_d2.md ("Life and Earth", task 4.15) and drawn
    /// in the chart's heart (<see cref="HeartPattern"/>). Its reaction is Split Rock; the Shape mark opens its two shapes,
    /// Taking Root and Root Web. Root Grip is now the reaction (the Shape mark takes its place) (<see cref="RetiredMarks"/>) and
    /// Thorn Wall, offered to Earth's tree, whose roster is full, is kept hidden for the Evolutions (<see cref="SpareSpells"/>). Kept: Moss on Stone, Thorns
    /// Take Root (once Briar Patch), Living Rampart (the keystone), Stone Oak (the duo spell) and The Ground Wakes, begun by
    /// the surge, the branch's duo surge and top mark. Open while both are carried.
    /// </summary>
    public static class OvergrowthTree
    {
        public const string Shape = "overgrowth.shape";
        public const string MossOnStone = "overgrowth.moss";
        public const string BriarPatch = "overgrowth.briarpatch";
        public const string StoneOak = "overgrowth.stoneoak";
        public const string LivingRampart = "overgrowth.livingrampart";
        public const string TheGroundWakes = "overgrowth.groundwakes";

        public static void Add(List<Upgrade> to)
        {
            const string A = Elements.Life, B = Elements.Earth;

            // Taking Root's crack spares the reacting enemy (SparesTarget); Root Web reaches only the Seeded (OnlyCarrying).
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = Shape, Name = "Split Rock: Taking Root or Root Web", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Part = BranchPart.Shape, Cost = 3, Icon = UpgradeIcons.Spiral,
                Description = "Opens Split Rock's two shapes, chosen at a rest fire. Taking Root: a short hold, and the floor cracks round it at once. Root Web: the roots run on underground and catch the enemies near it. Either runs instead of the plain Split Rock.",
                Shapes = new[]
                {
                    new ShapeDef
                    {
                        Name = "Taking Root",
                        Description = "Held only 1 s, and no split: the floor cracks round it at once, Tremor 1 on every other enemy within 2.5 m.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hold, Seconds = 1f },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Afflict, Radius = 2.5f, Affliction = AfflictionKind.Tremor, Stacks = 1, SparesTarget = true },
                        },
                    },
                    new ShapeDef
                    {
                        Name = "Root Web",
                        Description = "The roots run on underground, a line of lifting soil to each Seeded enemy within 6 m: each is held 0.5 s and given Tremor 1.",
                        Effects = new[]
                        {
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Hold, Radius = 6f, Seconds = 0.5f, OnlyCarrying = AfflictionKind.Seeded },
                            new UpgradeEffect { On = UpgradeTrigger.Reacted, Do = UpgradeAction.Afflict, Radius = 6f, Affliction = AfflictionKind.Tremor, Stacks = 1, OnlyCarrying = AfflictionKind.Seeded },
                        },
                    },
                },
            }, HeartPattern.Foot));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = MossOnStone, Name = "Moss on Stone", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Leaf,
                Description = "While you fight without rolling you heal slowly, and thorns grow on you that cut whoever strikes you.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.EverySeconds, Every = 3f, While = StatWhen.Planted, Do = UpgradeAction.Heal, Amount = 2f }, new UpgradeEffect { On = UpgradeTrigger.Struck, While = StatWhen.Planted, Do = UpgradeAction.Burst, Radius = 1f, Damage = 8f, Affliction = AfflictionKind.Seeded, Stacks = 1, Cooldown = 1f } },
            }, HeartPattern.LeftLow));

            // Once called Briar Patch: Life's base spell is a mark of that name now (Phase 4), and no two marks share one.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = BriarPatch, Name = "Thorns Take Root", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { Shape }, Icon = UpgradeIcons.Spike,
                Description = "The thorns of a Seeded enemy's death take root where they fall. The patch slows what walks in it and seeds it.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Seeded, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 3f, Seconds = 6f, Interval = 1f, Damage = 3f, Affliction = AfflictionKind.Seeded, Stacks = 1, Cooldown = 1f }, new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Seeded, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 3f, Seconds = 6f, Interval = 2f, Damage = 0f, Affliction = AfflictionKind.Tremor, Stacks = 1, Cooldown = 1f } },
            }, HeartPattern.RightLow));

            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = LivingRampart, Name = "Living Rampart", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { MossOnStone, BriarPatch }, Icon = UpgradeIcons.Shield,
                Description = "Your guard is alive. Every blow held on it heals you a little and seeds the one who struck. A perfect guard roots the attacker where it stands. Guarding costs more stamina.",
                Stats = new[] { new StatMod(PlayerStat.GuardStamina, 0.15f) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Guard, Do = UpgradeAction.Heal, Amount = 2f }, new UpgradeEffect { On = UpgradeTrigger.Guard, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Seeded, Stacks = 1 }, new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Hold, Seconds = 1f }, new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Tremor, Stacks = 2 } },
            }, HeartPattern.Keystone));

            // The duo spell. The rod form is the planted totem. NEEDS: a totem that grows with blows landed near it, heals in
            // its shade and blocks the way. Here it strikes as the young tree from the start.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = StoneOak, Name = "Stone Oak", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Role = SpellRole.Opener, Requires = new[] { MossOnStone, BriarPatch }, Icon = UpgradeIcons.Tower,
                Description = "You plant a stone oak a little ahead. For twenty seconds its roots strike everything in a circle round it, every two seconds. 45 mana.",
                Form = SpellForms.Rod,
                Spell = new SpellSpec { Cost = 45f, Reach = 2f, Radius = 5f, Seconds = 20f, Interval = 2f, Damage = 10f, Stagger = 15f, Delay = 0.5f, Stacks = 1 },
            }, HeartPattern.Spell));

            // BESPOKE (built): GroundWakesForm. The branch's duo surge, worn in the Hand's surge slot with its own 40 mana and 10 s
            // (DuoSurge), and its top mark: in force only while it is the surge worn.
            to.Add(HeartPattern.Put(new Upgrade
            {
                Id = TheGroundWakes, Name = "The Ground Wakes", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { StoneOak }, Icon = UpgradeIcons.Crown,
                Description = "A duo surge for your surge slot: 40 mana, 10 seconds. When it begins, the ground rises round you in a ring of root and stone and holds every enemy inside it. Then it flowers, and you are healed for each one held.",
                Surge = new DuoSurge { Mana = 40f, Seconds = 10f },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.GroundWakes, Affliction = AfflictionKind.Tremor, Spell = new SpellSpec { Cost = 0f, Radius = 7f, Seconds = 2f, Damage = 15f, Stagger = 20f, Stacks = 3, LingerDamage = 5f } } },
            }, HeartPattern.Crown));
        }
    }
}

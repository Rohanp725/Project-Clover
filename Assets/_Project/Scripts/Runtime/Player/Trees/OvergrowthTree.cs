using System.Collections.Generic;
using Clover.Gameplay;

namespace Clover.Player.Trees
{
    /// <summary>
    /// Overgrowth: the branch Life and Earth share. The ground takes the player's side: things are planted and grow
    /// while the fight goes on, and the one who stands in one place is fed by it. Its crown, The Ground Wakes, is
    /// given by a god and never sold.
    /// </summary>
    public static class OvergrowthTree
    {
        public const string RootGrip = "overgrowth.rootgrip";
        public const string MossOnStone = "overgrowth.moss";
        public const string BriarPatch = "overgrowth.briarpatch";
        public const string ThornWall = "overgrowth.thornwall";
        public const string StoneOak = "overgrowth.stoneoak";
        public const string LivingRampart = "overgrowth.livingrampart";
        public const string TheGroundWakes = "overgrowth.groundwakes";

        public static void Add(List<Upgrade> to)
        {
            // The first element is the one whose mix this is: the earlier of the two round the ring.
            const string A = Elements.Life, B = Elements.Earth;

            to.Add(new Upgrade { Id = RootGrip, Name = "Root Grip", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, X = 0f, Y = 1f, Icon = UpgradeIcons.Claw,
                Description = "Heavy blows lay both Tremor and Seed. A heavy blow on an enemy that bears both sends roots up its legs and holds it where it stands.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Tremor, Stacks = 1 }, new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Seeded, Stacks = 1 }, new UpgradeEffect { On = UpgradeTrigger.HeavyHit, Needs = AfflictionKind.Seeded, Needs2 = AfflictionKind.Tremor, Do = UpgradeAction.Hold, Seconds = 1.2f, Cooldown = 3f } } });
            to.Add(new Upgrade { Id = MossOnStone, Name = "Moss on Stone", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { RootGrip }, X = -0.5f, Y = 2.36f, Icon = UpgradeIcons.Leaf,
                Description = "While you fight without rolling you heal slowly, and thorns grow on you that cut whoever strikes you.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.EverySeconds, Every = 3f, While = StatWhen.Planted, Do = UpgradeAction.Heal, Amount = 2f }, new UpgradeEffect { On = UpgradeTrigger.Struck, While = StatWhen.Planted, Do = UpgradeAction.Burst, Radius = 1f, Damage = 8f, Affliction = AfflictionKind.Seeded, Stacks = 1, Cooldown = 1f } } });
            to.Add(new Upgrade { Id = BriarPatch, Name = "Briar Patch", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 3, Requires = new[] { MossOnStone }, X = 0f, Y = 3.48f, Icon = UpgradeIcons.Spike,
                Description = "The thorns of a Seeded enemy's death take root where they fall. The patch slows what walks in it and seeds it.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Seeded, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 3f, Seconds = 6f, Interval = 1f, Damage = 3f, Affliction = AfflictionKind.Seeded, Stacks = 1, Cooldown = 1f }, new UpgradeEffect { On = UpgradeTrigger.Kill, Needs = AfflictionKind.Seeded, Do = UpgradeAction.Hazard, Shape = HazardShape.Patch, Radius = 3f, Seconds = 6f, Interval = 2f, Damage = 0f, Affliction = AfflictionKind.Tremor, Stacks = 1, Cooldown = 1f } } });
            // The wall form does not cut what touches it, and a spell lays only the affliction of the element held.
            to.Add(new Upgrade { Id = ThornWall, Name = "Thorn Wall", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 4, Requires = new[] { MossOnStone }, X = -0.5f, Y = 4.6f, Icon = UpgradeIcons.Wall,
                Description = "You raise a hedge of stone and briar before you. It blocks the way for eight seconds, then bursts into thorns, or sooner if you strike it with a heavy blow.",
                Form = "wall",
                Spell = new SpellSpec { Cost = 35f, Reach = 2.5f, Arc = 5f, Delay = 0.2f, Seconds = 8f, Radius = 5f, Damage = 20f, Stagger = 25f, Stacks = 1 } });
            // The rod form is the planted totem. NEEDS: a totem that grows with blows landed near it, heals in its shade and blocks the way. Here it strikes as the young tree from the start.
            to.Add(new Upgrade { Id = StoneOak, Name = "Stone Oak", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { ThornWall }, X = 0f, Y = 5.72f, Icon = UpgradeIcons.Tower,
                Description = "You plant a stone oak a little ahead. For twenty seconds its roots strike everything in a circle round it, every two seconds. 45 mana.",
                Form = SpellForms.Rod,
                Spell = new SpellSpec { Cost = 45f, Reach = 2f, Radius = 5f, Seconds = 20f, Interval = 2f, Damage = 10f, Stagger = 15f, Delay = 0.5f, Stacks = 1 } });
            to.Add(new Upgrade { Id = LivingRampart, Name = "Living Rampart", Element = A, Element2 = B, Kind = UpgradeKind.Mix, Cost = 5, Requires = new[] { BriarPatch }, X = 0.5f, Y = 4.6f, Icon = UpgradeIcons.Shield,
                Description = "Your guard is alive. Every blow held on it heals you a little and seeds the one who struck. A perfect guard roots the attacker where it stands. Guarding costs more stamina.",
                Stats = new[] { new StatMod(PlayerStat.GuardStamina, 0.15f) },
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.Guard, Do = UpgradeAction.Heal, Amount = 2f }, new UpgradeEffect { On = UpgradeTrigger.Guard, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Seeded, Stacks = 1 }, new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Hold, Seconds = 1f }, new UpgradeEffect { On = UpgradeTrigger.PerfectGuard, Do = UpgradeAction.Afflict, Affliction = AfflictionKind.Tremor, Stacks = 2 } } });

            // BESPOKE (built): GroundWakesForm.
            to.Add(new Upgrade { Id = TheGroundWakes, Name = "The Ground Wakes", Element = A, Element2 = B, Kind = UpgradeKind.Signature, X = 0f, Y = 7.96f, Icon = UpgradeIcons.Crown,
                Description = "When your surge begins, the ground rises round you in a ring of root and stone and holds every enemy inside it. Then it flowers, and you are healed for each one held.",
                Effects = new[] { new UpgradeEffect { On = UpgradeTrigger.SurgeStart, Do = UpgradeAction.Form, Form = SpellForms.GroundWakes, Affliction = AfflictionKind.Tremor, Spell = new SpellSpec { Cost = 0f, Radius = 7f, Seconds = 2f, Damage = 15f, Stagger = 20f, Stacks = 3, LingerDamage = 5f } } } });
        }
    }
}

using System.Collections.Generic;

namespace Clover.Player.Trees
{
    /// <summary>
    /// The spare spells of the reshaped ring mixes and Plasma Lance, kept for Phase 6's Evolutions (the owner's decision of
    /// 2026-11-20, GAME.md's decisions log): hidden marks (<see cref="MarkTree.Hidden"/>), never drawn, never sold and
    /// never in force (<see cref="MarkRules.InForce"/>), so a save that learned one keeps it and is refunded nothing.
    /// Each is as it stood on its branch before the reshaping (task 4.5 and 4.15), with what it grew from left out, since
    /// it is on no chart. The d2 to d5 branches cut no spare spell (phase4_questions.md, tasks 4.15 to 4.17).
    /// </summary>
    public static class SpareSpells
    {
        public const string StormOverhead = "tempest.cloud";
        public const string Whiteout = "blizzard.whiteout";
        public const string CreepingIce = "glacier.creeping";
        public const string LotusStep = "bloom.lotus";
        public const string ThornWall = "overgrowth.thornwall";
        public const string TripHammer = "forge.triphammer";
        public const string SunWheels = "halo.sunwheels";
        public const string BlackSun = "eclipse.blacksun";
        public const string GraveSoil = "blight.gravesoil";
        public const string FireEater = "pyre.fireeater";
        public const string LitFuse = "pyre.slowfuse";
        public const string PlasmaLance = "plasma.lance";

        /// <summary>Every spare spell's id, in the ring's order.</summary>
        public static readonly string[] All =
        {
            PlasmaLance, StormOverhead, Whiteout, CreepingIce, LotusStep, ThornWall, TripHammer, SunWheels, BlackSun, GraveSoil, FireEater, LitFuse,
        };

        public static void Add(List<Upgrade> to)
        {
            // Plasma Lance (Fire and Lightning), cut with Plasma's reshaping (task 4.5).
            to.Add(Spare(PlasmaLance, "Plasma Lance", Elements.Fire, Elements.Lightning, 5, UpgradeIcons.Beam, SpellForms.Bolt,
                "You throw a bolt of plasma that leaps through three more.",
                new SpellSpec { Cost = 35f, Reach = 11f, Damage = 30f, Stagger = 26f, Count = 3, Radius = 5f, Stacks = 2 }));
            // Tempest (Lightning and Wind). BESPOKE when it is an Evolution: a cloud that follows the player and answers each
            // dodge beneath it with a bolt where they stood; the rain form until then.
            to.Add(Spare(StormOverhead, "Storm Overhead", Elements.Lightning, Elements.Wind, 4, UpgradeIcons.Comet, SpellForms.Rain,
                "You call a storm over the ground before you. For ten seconds bolts fall inside its circle, one every 0.8 seconds.",
                new SpellSpec { Cost = 40f, Reach = 1f, Radius = 3f, Seconds = 10f, Interval = 0.8f, Damage = 14f, Stagger = 20f, Stacks = 1 }));
            // Blizzard (Wind and Ice).
            to.Add(Spare(Whiteout, "Whiteout", Elements.Wind, Elements.Ice, 4, UpgradeIcons.Spiral, SpellForms.Rain,
                "You call down driving snow in a wide circle for eight seconds. Shards fall inside it again and again, each a small bite.",
                new SpellSpec { Cost = 40f, Reach = 0.5f, Radius = 7f, Seconds = 8f, Interval = 0.6f, Damage = 3f, Stagger = 4f, Stacks = 1 }));
            // Glacier (Ice and Water).
            to.Add(Spare(CreepingIce, "Creeping Ice", Elements.Ice, Elements.Water, 4, UpgradeIcons.Spike, SpellForms.Spikes,
                "You send a line of ice spikes eight metres along the ground. Each enemy on the line is struck once.",
                new SpellSpec { Cost = 35f, Reach = 8f, Speed = 20f, Count = 1, Radius = 0.6f, Damage = 8f, Stagger = 10f, Stacks = 2 }));
            // Bloom (Water and Life).
            to.Add(Spare(LotusStep, "Lotus Step", Elements.Water, Elements.Life, 4, UpgradeIcons.Boot, SpellForms.Field,
                "You open a lotus on the ground just ahead. For twelve seconds it cuts every enemy standing on it, once a second.",
                new SpellSpec { Cost = 30f, Reach = 2f, Radius = 1.5f, Seconds = 12f, Interval = 1f, LingerDamage = 10f, Stacks = 1 }));
            // Overgrowth (Life and Earth).
            to.Add(Spare(ThornWall, "Thorn Wall", Elements.Life, Elements.Earth, 4, UpgradeIcons.Wall, SpellForms.Wall,
                "You raise a hedge of stone and briar before you. It blocks the way for eight seconds, then bursts into thorns, or sooner if you strike it with a heavy blow.",
                new SpellSpec { Cost = 35f, Reach = 2.5f, Arc = 5f, Delay = 0.2f, Seconds = 8f, Radius = 5f, Damage = 20f, Stagger = 25f, Stacks = 1 }));
            // Forge (Earth and Metal).
            to.Add(Spare(TripHammer, "Trip Hammer", Elements.Earth, Elements.Metal, 4, UpgradeIcons.Hammer, SpellForms.Sword,
                "A great hammer forms above the enemy you face, and falls after a moment. It leaves a crack in the ground that bites for a second.",
                new SpellSpec { Cost = 40f, Reach = 10f, Delay = 1f, Radius = 2.5f, Damage = 30f, Stagger = 50f, Stacks = 1, Seconds = 1f, LingerDamage = 2f }));
            // Halo (Metal and Light).
            to.Add(Spare(SunWheels, "Sun Wheels", Elements.Metal, Elements.Light, 4, UpgradeIcons.Spiral, SpellForms.Discs,
                "Two wheels of bright steel turn round you for a while, cutting what they pass.",
                new SpellSpec { Cost = 40f, Count = 2, Radius = 2.5f, Seconds = 9f, Interval = 0.5f, Damage = 5f, Stagger = 4f, Stacks = 1 }));
            // Eclipse (Light and Dark).
            to.Add(Spare(BlackSun, "Black Sun", Elements.Light, Elements.Dark, 4, UpgradeIcons.Orb, SpellForms.Reverse,
                "You hang a black sun before you. For three seconds it draws everything near it inward, then it bursts.",
                new SpellSpec { Cost = 50f, Reach = 5f, Radius = 5f, Delay = 3f, Damage = 40f, Stagger = 40f, Stacks = 1 }));
            // Blight (Dark and Poison).
            to.Add(Spare(GraveSoil, "Grave Soil", Elements.Dark, Elements.Poison, 4, UpgradeIcons.Spike, SpellForms.Field,
                "You foul the ground round you. What stands on it is bitten every two seconds and takes the affliction of the element you hold.",
                new SpellSpec { Cost = 40f, Reach = 0.5f, Radius = 5f, Seconds = 8f, Interval = 2f, LingerDamage = 3f, Stacks = 1 }));
            // Pyre (Poison and Fire), its two.
            to.Add(Spare(FireEater, "Fire Eater", Elements.Poison, Elements.Fire, 4, UpgradeIcons.Wave, SpellForms.Beam,
                "You brace and breathe out a stream of green fire for two seconds, turning slowly as it burns. It lays the affliction of the element you hold on all it sweeps across.",
                new SpellSpec { Cost = 40f, Reach = 5f, Radius = 1.2f, Seconds = 2f, Interval = 0.25f, Damage = 4f, Stagger = 3f, Stacks = 1 }));
            to.Add(Spare(LitFuse, "Lit Fuse", Elements.Poison, Elements.Fire, 5, UpgradeIcons.Target, SpellForms.Brand,
                "You set a fuse on the enemy you face. Your next blow on it within five seconds sets off a great blast.",
                new SpellSpec { Cost = 45f, Reach = 8f, Seconds = 5f, Count = 1, Radius = 3.5f, Damage = 40f, Stagger = 40f, Stacks = 2 }));
        }

        private static Upgrade Spare(string id, string name, string first, string second, int cost, string icon, string form, string words, SpellSpec spell) =>
            new Upgrade
            {
                Id = id, Name = name, Element = first, Element2 = second, Kind = UpgradeKind.Mix, Tree = MarkTree.Hidden, Cost = cost, Icon = icon,
                Description = words, Form = form, Spell = spell,
            };
    }
}

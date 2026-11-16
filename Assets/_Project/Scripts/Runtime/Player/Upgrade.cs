using System.Collections.Generic;

namespace Clover.Player
{
    /// <summary>One thing an element's tree can teach: what it is called, what it costs, and what it does, in the game's own words.</summary>
    public class Upgrade
    {
        public string Id;
        public string Name;
        public string Element;
        public int Cost;
        public string Description;
        /// <summary>The weapon it needs, or empty if it works with any.</summary>
        public string Weapon = "";
        /// <summary>True for what only a god's fall gives: it is never for sale, and the tree does not list it.</summary>
        public bool Signature;

        // Fire: pressure and momentum.
        public const string Heat = "fire.heat";
        public const string BurnAndDetonate = "fire.detonate";
        public const string Backdraft = "fire.backdraft";
        public const string FlameTrail = "fire.trail";
        public const string BurningCrescent = "fire.crescent";
        public const string FirebombFlask = "fire.firebomb";
        public const string MeteorLeap = "fire.meteor";
        // Lightning: speed and precision.
        public const string Static = "lightning.static";
        public const string StoredCharge = "lightning.charge";
        public const string Blink = "lightning.blink";
        public const string Afterimage = "lightning.afterimage";
        public const string DelayedCuts = "lightning.cuts";
        public const string LightningRod = "lightning.rod";
        public const string Overclock = "lightning.overclock";
        // What a god leaves, by the choice made over him.
        public const string StormStep = "lightning.stormstep";
        public const string Plasma = "fire.plasma";

        /// <summary>Every upgrade in the slice, in the order its tree shows them: cheapest first.</summary>
        public static readonly IReadOnlyList<Upgrade> All = new List<Upgrade>
        {
            new Upgrade { Id = Heat, Name = "Heat", Element = ElementKit.FireName, Cost = 1,
                Description = "Blows landed without being struck build Heat, up to five. Each level adds damage. A blow taken puts it out. Kindle fills it at once." },
            new Upgrade { Id = BurnAndDetonate, Name = "Burn and Detonate", Element = ElementKit.FireName, Cost = 2,
                Description = "Every blow of the weapon sets a burn, up to five deep. A perfect guard against a burning enemy sets all of it off at once." },
            new Upgrade { Id = Backdraft, Name = "Backdraft", Element = ElementKit.FireName, Cost = 2,
                Description = "A perfect guard throws a cone of fire back at the one who struck." },
            new Upgrade { Id = FlameTrail, Name = "Flame Trail", Element = ElementKit.FireName, Cost = 2,
                Description = "The dodge leaves a line of fire behind it that burns what follows." },
            new Upgrade { Id = BurningCrescent, Name = "Burning Crescent", Element = ElementKit.FireName, Cost = 3, Weapon = Weapons.KatanaName,
                Description = "The Iaido Slash throws a slow arc of flame that travels on after the cut." },
            new Upgrade { Id = FirebombFlask, Name = "Firebomb Flask", Element = ElementKit.FireName, Cost = 3,
                Description = "Hold the flask button to throw it. No healing: a great burst of fire where it lands." },
            new Upgrade { Id = MeteorLeap, Name = "Meteor Leap", Element = ElementKit.FireName, Cost = 4,
                Description = "Hold the spell button to leap out of the fight and come down on the enemy faced, leaving a burning crater. 50 mana." },

            new Upgrade { Id = Static, Name = "Static", Element = ElementKit.LightningName, Cost = 1,
                Description = "Blows build Static on an enemy, up to five. A heavy blow or a finisher discharges it, and it arcs to everyone near." },
            new Upgrade { Id = StoredCharge, Name = "Stored Charge", Element = ElementKit.LightningName, Cost = 2,
                Description = "A perfect guard banks a charge. The next blow spends it for double damage and stagger." },
            new Upgrade { Id = Blink, Name = "Blink", Element = ElementKit.LightningName, Cost = 2,
                Description = "The dodge becomes a short step through lightning that shocks whatever it passes through." },
            new Upgrade { Id = Afterimage, Name = "Afterimage", Element = ElementKit.LightningName, Cost = 3,
                Description = "A dodge at the last moment leaves a copy of you that strikes where you stood." },
            new Upgrade { Id = DelayedCuts, Name = "Delayed Cuts", Element = ElementKit.LightningName, Cost = 3, Weapon = Weapons.KatanaName,
                Description = "The Iaido Slash passes through. Its cuts land a moment later, all at once, when the blade is home." },
            new Upgrade { Id = LightningRod, Name = "Lightning Rod", Element = ElementKit.LightningName, Cost = 3,
                Description = "Hold the spell button to plant a rod. Bolts fall around it for a while. 40 mana." },
            new Upgrade { Id = Overclock, Name = "Overclock", Element = ElementKit.LightningName, Cost = 4,
                Description = "Overdrive opens with a breath in which everything but you is slowed." },

            new Upgrade { Id = StormStep, Name = "Storm Step", Element = ElementKit.LightningName, Cost = 0, Signature = true,
                Description = "While Overdrive lasts, every dodge costs nothing and shocks what it passes through." },
            new Upgrade { Id = Plasma, Name = "Plasma", Element = ElementKit.FireName, Cost = 0, Signature = true,
                Description = "A heavy blow or a finisher on a burning enemy bursts, and sets alight everyone near." },
        };

        public static Upgrade ById(string id)
        {
            foreach (var upgrade in All)
                if (upgrade.Id == id) return upgrade;
            return null;
        }
    }
}

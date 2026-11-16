using System.Collections.Generic;
using Clover.Player.Forms;

namespace Clover.Player
{
    /// <summary>
    /// The library of spell forms, by name. A new form is a class under <c>Forms/</c> that implements
    /// <see cref="ISpellForm"/>, a name here, and one line in the list below; upgrades then ask for it by that name.
    /// The fields each reads are listed in <c>docs/UPGRADE_SCHEMA.md</c>, section 10.
    /// </summary>
    public static class SpellForms
    {
        /// <summary>A cone thrown ahead that strikes everything in it. Reads: Damage, Stagger, Reach, Arc, Stacks.</summary>
        public const string Wave = "wave";
        /// <summary>A bolt on the enemy faced that leaps on to others. Reads: Damage, Stagger, Reach, Count (leaps), Radius (how far each leap can be), Stacks.</summary>
        public const string Bolt = "bolt";
        /// <summary>A leap out of the fight, down beside the enemy faced, and a crater. Reads: Reach, Seconds (in the air), Height, Damage, Stagger, Radius, LingerDamage, Stacks.</summary>
        public const string Leap = "leap";
        /// <summary>A rod planted a little ahead; the element strikes round it for a while. Reads: Reach (how far ahead), Radius, Seconds, Interval, Damage, Stagger, Delay, Stacks.</summary>
        public const string Rod = "rod";

        /// <summary>Blades that hang over the enemy faced and fall one after another. Reads: Count, Delay, Radius, Damage, Stagger, Stacks, Reach.</summary>
        public const string Blades = "blades";
        /// <summary>One great sword from the sky and a crack that bites. Reads: Delay, Radius, Damage, Stagger, Stacks, Seconds, LingerDamage, Reach.</summary>
        public const string Sword = "sword";
        /// <summary>A held beam the player can turn slowly. Reads: Reach, Radius (half width), Seconds, Interval, Damage (each bite), Stagger, Stacks.</summary>
        public const string Beam = "beam";
        /// <summary>Blades at the shoulders that fire by themselves, or all at once on a heavy blow. Reads: Count, Seconds, Reach, Damage, Stagger, Stacks, Speed.</summary>
        public const string Phalanx = "phalanx";
        /// <summary>A dragon's head that lunges down a lane and bites at its end. Reads: Reach, Radius, Damage, Stagger, Stacks, Speed.</summary>
        public const string Dragon = "dragon";
        /// <summary>A burst round the caster. Reads: Radius, Delay, Damage, Stagger, Stacks.</summary>
        public const string Nova = "nova";
        /// <summary>A slow orb that follows the nearest enemy and bursts; a weapon blow bursts it at once, larger. Reads: Speed, Seconds, Radius, Damage, Stagger, Stacks.</summary>
        public const string Orb = "orb";
        /// <summary>A great sphere raised overhead and thrown. Reads: Delay, Reach, Radius, Damage, Stagger, Stacks, Seconds, LingerDamage.</summary>
        public const string Bomb = "bomb";
        /// <summary>Spikes running out along the ground in rows. Reads: Reach, Speed, Count (rows), Radius (of one spike), Damage, Stagger, Stacks.</summary>
        public const string Spikes = "spikes";
        /// <summary>A patch of ground that bites. Reads: Reach, Radius, Seconds, Interval, LingerDamage, Stacks.</summary>
        public const string Field = "field";
        /// <summary>The player bores through everything on a line. Reads: Reach, Radius, Count (strikes on each), Damage (each), Stagger, Stacks, Speed.</summary>
        public const string Drill = "drill";
        /// <summary>Small helpers that run at enemies and strike them. Reads: Count, Seconds, Damage (each strike), Stagger, Stacks, Interval.</summary>
        public const string Minions = "minions";
        /// <summary>A ring of bullets round the enemy faced that fly inward together. Reads: Count, Radius, Delay, Damage (each), Stagger, Stacks, Reach.</summary>
        public const string Bullets = "bullets";
        /// <summary>A point that pulls enemies in and then bursts. Reads: Reach, Radius, Delay, Damage, Stagger, Stacks.</summary>
        public const string Reverse = "reverse";
        /// <summary>A guard of the element that turns the first blow and answers with a cone. Reads: Seconds, Reach, Arc, Damage, Stagger, Stacks.</summary>
        public const string Parry = "parry";
        /// <summary>A wall that blocks and then bursts. Reads: Reach, Arc (width in metres), Delay, Seconds, Radius, Damage, Stagger, Stacks.</summary>
        public const string Wall = "wall";
        /// <summary>Toothed discs that orbit the player. Reads: Count, Radius, Seconds, Interval, Damage (each strike), Stagger, Stacks.</summary>
        public const string Discs = "discs";
        /// <summary>A sign on the enemy faced that the next weapon blows set off. Reads: Reach, Seconds, Count, Radius, Damage, Stagger, Stacks.</summary>
        public const string Brand = "brand";
        /// <summary>Strikes falling at random in a circle for a while. Reads: Reach, Radius, Seconds, Interval, Damage (each), Stagger, Stacks.</summary>
        public const string Rain = "rain";
        /// <summary>A copy of the player that repeats every weapon blow a beat late. Reads: Seconds, Damage (percent of the blow), Stacks.</summary>
        public const string Clone = "clone";
        /// <summary>The player vanishes, lines cross the enemy faced, and all the cuts land at once. Reads: Count, Interval, Delay, Damage (each cut), Stagger, Stacks, Reach.</summary>
        public const string Flash = "flash";
        /// <summary>A ring nothing crosses, closed round the player and the enemy faced. Reads: Radius, Seconds, LingerDamage, Stacks, Reach.</summary>
        public const string Ring = "ring";
        /// <summary>Pillars under the nearest enemies. Reads: Count, Reach, Radius, Damage, Stagger, Stacks, Delay.</summary>
        public const string Pillars = "pillars";
        /// <summary>A step through the enemy faced; if it lands, the next heavy blow is critical. Reads: Reach, Damage, Stagger, Stacks, Seconds.</summary>
        public const string Step = "step";

        private static readonly ISpellForm[] all =
        {
            new WaveForm(),
            new BoltForm(),
            new LeapForm(),
            new RodForm(),
            new BladesForm(),
            new SwordForm(),
            new BeamForm(),
            new PhalanxForm(),
            new DragonForm(),
            new NovaForm(),
            new OrbForm(),
            new BombForm(),
            new SpikesForm(),
            new FieldForm(),
            new DrillForm(),
            new MinionsForm(),
            new BulletsForm(),
            new ReverseForm(),
            new ParryForm(),
            new WallForm(),
            new DiscsForm(),
            new BrandForm(),
            new RainForm(),
            new CloneForm(),
            new FlashForm(),
            new RingForm(),
            new PillarsForm(),
            new StepForm(),
        };

        private static readonly Dictionary<string, ISpellForm> byId = Index();

        public static IReadOnlyList<ISpellForm> All => all;

        private static Dictionary<string, ISpellForm> Index()
        {
            var made = new Dictionary<string, ISpellForm>();
            foreach (var form in all) made[form.Id] = form;
            return made;
        }

        /// <summary>The form of that name, or null when there is none.</summary>
        public static ISpellForm Get(string id) => id != null && byId.TryGetValue(id, out var found) ? found : null;
    }
}

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
        /// <summary>The eleven tap spells: each element's own, by the element it is cast in.</summary>
        public const string Tap = "tap";
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

        // The bespoke moves: the gods' gifts and the crowns of the mixed branches. Each has its own class under Forms/ that says what it reads.
        /// <summary>Fire's gift: a sun that hangs and falls.</summary>
        public const string Sun = "sun";
        /// <summary>Wind's gift: cuts that hang and fall again.</summary>
        public const string Remember = "remember";
        /// <summary>Ice's gift: the shell broken outward.</summary>
        public const string StillWorld = "stillworld";
        /// <summary>Ice's gift: a shatter that runs on.</summary>
        public const string ShatterChain = "shatterchain";
        /// <summary>Water's gift: the flood.</summary>
        public const string Sea = "sea";
        /// <summary>Life's gift: the forest.</summary>
        public const string Spring = "spring";
        /// <summary>Earth's gift: the hand of stone.</summary>
        public const string Hand = "hand";
        /// <summary>Metal's gift: swords over those who struck the guard.</summary>
        public const string HangingSwords = "hangingswords";
        /// <summary>Light's gift: the weighing.</summary>
        public const string Scales = "scales";
        /// <summary>Dark's gift: the Rise.</summary>
        public const string Rise = "rise";
        /// <summary>Poison's gift: the held beat and the flick.</summary>
        public const string CleanBlade = "cleanblade";
        /// <summary>Tempest's crown.</summary>
        public const string Thunder = "thunder";
        /// <summary>Blizzard's crown.</summary>
        public const string Dust = "dust";
        /// <summary>Glacier's crown.</summary>
        public const string Plates = "plates";
        /// <summary>Bloom's crown.</summary>
        public const string Lotus = "lotus";
        /// <summary>Overgrowth's crown.</summary>
        public const string GroundWakes = "groundwakes";
        /// <summary>Forge's crown.</summary>
        public const string Anvil = "anvil";
        /// <summary>Halo's crown.</summary>
        public const string Mirrors = "mirrors";
        /// <summary>Eclipse's crown.</summary>
        public const string Totality = "totality";
        /// <summary>Blight's crown.</summary>
        public const string Reaping = "reaping";
        /// <summary>Pyre's crown.</summary>
        public const string Pyres = "pyres";
        /// <summary>Lightning's Seven Flashes: lines from enemy to enemy, the return, the cuts together.</summary>
        public const string Flashes = "flashes";
        /// <summary>Glacier's The Wave Stands Still: a wave that rolls while the button is held and freezes when it is let go.</summary>
        public const string FrozenWave = "frozenwave";
        /// <summary>Plasma's Ball Lightning: a slow ball the weapon bats at the enemy, larger each time.</summary>
        public const string Ball = "ball";
        /// <summary>Dark's Pawned Heart: health set down as a beating heart, to be come back for.</summary>
        public const string Heart = "heart";
        /// <summary>Wind's Sky Dance: up beside an enemy in the air, cuts pressed as a ring closes, and a throw the stick aims.</summary>
        public const string SkyDance = "skydance";
        /// <summary>Wind's Closing Circle: a ring run round an enemy that snaps shut and lifts all inside.</summary>
        public const string Circle = "circle";
        /// <summary>Tempest's Eye of the Storm: a held, walking column that drags in, carries round, throws out and calls a bolt on each.</summary>
        public const string Eye = "eye";
        /// <summary>Fire's brand: light blows feed it, a heavy blow sets it off.</summary>
        public const string Sunspot = "sunspot";
        /// <summary>Lightning's pull: enemies are dragged in along lines of lightning, further for their Static, and discharged.</summary>
        public const string Lodestone = "lodestone";
        /// <summary>Lightning's hung bolts, loosed only by the weapon's blows.</summary>
        public const string Waiting = "waiting";
        /// <summary>Three Weasels' first weasel: a rush that stops just in front of the enemy faced and cuts it. Reads what the step reads.</summary>
        public const string Rush = "rush";

        private static readonly ISpellForm[] all =
        {
            new WaveForm(),
            new TapForm(),
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
            new SunForm(),
            new RememberForm(),
            new StillWorldForm(),
            new ShatterChainForm(),
            new SeaForm(),
            new SpringForm(),
            new HandForm(),
            new HangingSwordsForm(),
            new ScalesForm(),
            new RiseForm(),
            new CleanBladeForm(),
            new ThunderForm(),
            new DustForm(),
            new PlatesForm(),
            new LotusForm(),
            new GroundWakesForm(),
            new AnvilForm(),
            new MirrorsForm(),
            new TotalityForm(),
            new ReapingForm(),
            new PyresForm(),
            new FlashesForm(),
            new FrozenWaveForm(),
            new BallForm(),
            new HeartForm(),
            new SkyDanceForm(),
            new CircleForm(),
            new EyeForm(),
            new SunspotForm(),
            new LodestoneForm(),
            new WaitingThunderForm(),
            new RushForm(),
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

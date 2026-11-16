using System.Collections.Generic;
using Clover.Player.Forms;

namespace Clover.Player
{
    /// <summary>
    /// The library of spell forms, by name. A new form is a class under <c>Forms/</c> that implements
    /// <see cref="ISpellForm"/>, a name here, and one line in the list below; upgrades then ask for it by that name.
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

        private static readonly ISpellForm[] all =
        {
            new WaveForm(),
            new BoltForm(),
            new LeapForm(),
            new RodForm(),
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

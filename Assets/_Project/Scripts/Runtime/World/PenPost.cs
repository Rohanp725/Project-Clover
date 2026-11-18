using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>A post at the edge of an <see cref="EnemyPen"/>: using it lets out the enemies it names, or with none named puts them all away.</summary>
    public class PenPost : Interactable
    {
        [SerializeField] private EnemyPen pen;
        [SerializeField] private int[] which = new int[0];

        /// <summary>What it lets out, in words, for the callout.</summary>
        public string What = "";
        public int Used { get; private set; }

        public void Configure(EnemyPen enemyPen, int[] places, string what)
        {
            pen = enemyPen;
            which = places;
            What = what;
            Prompt = places.Length == 0 ? "Clear the pen" : "Let out: " + what;
        }

        public override void Use(PlayerInteraction who)
        {
            if (pen == null) return;
            Used++;
            if (which.Length == 0)
            {
                pen.Clear();
                if (who.Defence != null) who.Defence.Callout("The pen is empty");
                return;
            }
            pen.Release(which);
            if (who.Defence != null) who.Defence.Callout(What);
        }
    }
}

using Clover.Player;

namespace Clover.World
{
    /// <summary>
    /// The door at the end of the area, to the summit where the god of Lightning waits. It opens only when every enemy
    /// in the area is dead. Until the summit itself is built it says so and stays shut.
    /// </summary>
    public class SealedWay : Interactable
    {
        [UnityEngine.SerializeField] private AreaDirector director;

        public int Tries { get; private set; }

        public void Configure(AreaDirector areaDirector) => director = areaDirector;

        public override void Use(PlayerInteraction who)
        {
            Tries++;
            if (who.Defence == null) return;
            int left = director != null ? director.EnemiesAlive : 0;
            who.Defence.Callout(left > 0 ? $"Sealed. {left} still stand." : "The storm waits above.");
        }
    }
}

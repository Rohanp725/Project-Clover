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

        /// <summary>The scene the way leads to once it opens; empty if nothing lies beyond yet.</summary>
        public string LeadsTo = "";

        public int Tries { get; private set; }

        public void Configure(AreaDirector areaDirector) => director = areaDirector;

        public override void Use(PlayerInteraction who)
        {
            Tries++;
            if (who.Defence == null) return;
            int left = director != null ? director.EnemiesAlive : 0;
            if (left > 0)
            {
                who.Defence.Callout($"Sealed. {left} still stand.");
                return;
            }
            who.Defence.Callout("The storm waits above.");
            if (!string.IsNullOrEmpty(LeadsTo)) UnityEngine.SceneManagement.SceneManager.LoadScene(LeadsTo);
        }
    }
}

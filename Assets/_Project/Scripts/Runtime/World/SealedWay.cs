using Clover.Gameplay;
using Clover.Player;

namespace Clover.World
{
    /// <summary>
    /// The door at the end of the area, to the summit where the god of Lightning waits. One enemy keeps it: when that
    /// one has fallen the seal is broken, and it stays broken even if a rest brings the keeper back. The rest of the
    /// area may be fought or run past. With no keeper named, it opens when every enemy in the area is dead.
    /// </summary>
    public class SealedWay : Interactable
    {
        [UnityEngine.SerializeField] private AreaDirector director;

        /// <summary>The scene the way leads to once it opens; empty if nothing lies beyond yet.</summary>
        public string LeadsTo = "";
        /// <summary>The kind of enemy that keeps the seal; empty if every enemy must fall.</summary>
        public string KeeperName = "";

        public int Tries { get; private set; }
        /// <summary>True once the keeper has fallen in this visit to the area.</summary>
        public bool SealBroken { get; private set; }

        private Enemy keeper;

        public void Configure(AreaDirector areaDirector, string keeperName = "")
        {
            director = areaDirector;
            KeeperName = keeperName;
        }

        private void Start()
        {
            if (director == null || string.IsNullOrEmpty(KeeperName)) return;
            foreach (var enemy in director.Enemies)
                if (enemy != null && enemy.Kind != null && enemy.Kind.Name == KeeperName) keeper = enemy;
            if (keeper != null) keeper.Died += OnKeeperDied;
        }

        private void OnDestroy()
        {
            if (keeper != null) keeper.Died -= OnKeeperDied;
        }

        private void OnKeeperDied(Enemy who) => SealBroken = true;

        public override void Use(PlayerInteraction who)
        {
            Tries++;
            if (who.Defence == null) return;
            if (keeper != null)
            {
                if (!SealBroken)
                {
                    who.Defence.Callout($"Sealed. The {KeeperName} holds the way.");
                    return;
                }
            }
            else
            {
                int left = director != null ? director.EnemiesAlive : 0;
                if (left > 0)
                {
                    who.Defence.Callout($"Sealed. {left} still stand.");
                    return;
                }
            }
            who.Defence.Callout("The storm waits above.");
            if (!string.IsNullOrEmpty(LeadsTo)) UnityEngine.SceneManagement.SceneManager.LoadScene(LeadsTo);
        }
    }
}

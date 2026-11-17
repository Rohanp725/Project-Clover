using Clover.Gameplay;
using Clover.Player;

namespace Clover.World
{
    /// <summary>
    /// The door at the end of a land, kept by its elite. While the keeper stands it is sealed. Once the keeper has
    /// fallen it opens: it says so, and leads on to <see cref="LeadsTo"/> if that scene is among the scenes the game
    /// can load; if it is not built yet, the words are all that happens.
    /// </summary>
    public class GateOfNoon : Interactable
    {
        [UnityEngine.SerializeField] private AreaDirector director;

        /// <summary>The scene beyond; it is loaded only if the game has it.</summary>
        public string LeadsTo = "SunCourt";
        public string KeeperName = "";
        public string SealedWords = "Sealed. The Champion of the Gate holds the way.";
        public string OpenWords = "It is noon within.";

        public int Tries { get; private set; }
        public int Opened { get; private set; }
        /// <summary>Times the editor's loader took the player on to a scene the build does not hold.</summary>
        public int Loaded { get; private set; }
        public bool SealBroken => keeper == null || keeper.IsDead;

        private Enemy keeper;

        /// <summary>
        /// In the editor a scene need not be in the build list to be loaded in play: the editor's own code puts a
        /// loader here as play begins (the runtime may not name the editor). It is given the scene's name and says
        /// whether it loaded it. In a built game it is never set, and only scenes the game was built with are loaded.
        /// </summary>
        public static System.Func<string, bool> LoadOutsideBuild;

        // Domain reload is off: the last play's loader must not be this one's.
        [UnityEngine.RuntimeInitializeOnLoadMethod(UnityEngine.RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => LoadOutsideBuild = null;

        public void Configure(AreaDirector areaDirector, string keeperName)
        {
            director = areaDirector;
            KeeperName = keeperName;
        }

        private void Start()
        {
            if (director == null) return;
            foreach (var enemy in director.Enemies)
                if (enemy != null && enemy.Kind != null && enemy.Kind.Name == KeeperName) keeper = enemy;
        }

        public override void Use(PlayerInteraction who)
        {
            Tries++;
            if (who.Defence == null) return;
            if (!SealBroken)
            {
                who.Defence.Callout(SealedWords);
                return;
            }
            Opened++;
            who.Defence.Callout(OpenWords);
            if (string.IsNullOrEmpty(LeadsTo)) return;
            if (UnityEngine.Application.CanStreamedLevelBeLoaded(LeadsTo)) UnityEngine.SceneManagement.SceneManager.LoadScene(LeadsTo);
            else if (LoadOutsideBuild != null && LoadOutsideBuild(LeadsTo)) Loaded++;
        }
    }
}

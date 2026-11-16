using Clover.Player;

namespace Clover.World
{
    /// <summary>A way from one scene to another. It can be shut until something opens it.</summary>
    public class SceneDoor : Interactable
    {
        /// <summary>The name of the scene it leads to.</summary>
        public string LeadsTo = "";
        public bool Open = true;

        public override bool Available => Open && !string.IsNullOrEmpty(LeadsTo);

        public override void Use(PlayerInteraction who)
        {
            if (!Available) return;
            UnityEngine.SceneManagement.SceneManager.LoadScene(LeadsTo);
        }
    }
}

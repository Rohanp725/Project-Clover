using Clover.Player;

namespace Clover.World
{
    /// <summary>A way from one scene to another. It can be shut until something opens it.</summary>
    public class SceneDoor : Interactable
    {
        /// <summary>The name of the scene it leads to.</summary>
        public string LeadsTo = "";
        public bool Open = true;
        /// <summary>True for the road home: going through it ends the run.</summary>
        public bool EndsRun;

        public override bool Available => Open && !string.IsNullOrEmpty(LeadsTo);

        public override void Use(PlayerInteraction who)
        {
            if (!Available) return;
            if (EndsRun) Clover.Core.RunState.End(false);
            UnityEngine.SceneManagement.SceneManager.LoadScene(LeadsTo);
        }
    }
}

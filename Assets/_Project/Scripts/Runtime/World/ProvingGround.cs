using Clover.Core;
using Clover.UI;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// The combat playtest's starting place (built by Clover > Build > Proving Ground). Being here switches the
    /// playtest on, since the place exists for nothing else, and the first arrival in a sitting opens the title: the
    /// playtest's version and what is new, read at the player's own pace.
    /// </summary>
    public class ProvingGround : MonoBehaviour
    {
        /// <summary>False to arrive without the title, as a probe that has seen it already may want.</summary>
        public bool ShowTitle = true;

        /// <summary>True once the title has been shown in this sitting; it is not shown again on coming back from a level.</summary>
        public static bool TitleShown { get; private set; }

        // Domain reload is off: each play is a new sitting and sees the title again.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => TitleShown = false;

        private void Awake() => PlaytestMode.Set(true);

        private void Start()
        {
            if (!ShowTitle || TitleShown) return;
            var page = GuidePage.Ensure();
            if (page == null) return;
            TitleShown = true;
            page.OpenTitle();
        }
    }
}

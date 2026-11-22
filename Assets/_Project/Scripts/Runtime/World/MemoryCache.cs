using Clover.Core;
using Clover.Player;
using Clover.Player.Growth;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// Memories lying hidden in the world (way_plan.md 0.3; task 5.6): 3 in a land of one element, 5 in a land of two,
    /// each paid once ever under its own key in the save (<c>SaveData.Found</c>). Taking it sends the amber light into the
    /// player as a defeat's does. Where each land's caches go is decided once its levels are signed off
    /// (way_build_plan.md question 8); until then they are tested in the Proving Ground.
    /// </summary>
    public class MemoryCache : Interactable
    {
        [SerializeField] private GameObject shown;

        /// <summary>The key it is paid under, unique in the game: <c>memory.fire.1</c>.</summary>
        public string Key = "";
        /// <summary>True in a land of two elements, where a cache holds more.</summary>
        public bool DuoLand;

        public bool Taken { get; private set; }
        public override bool Available => !Taken;
        /// <summary>The Memories it paid, for probes.</summary>
        public int Paid { get; private set; }

        private readonly Income income = new Income();

        public void Configure(string key, bool duoLand, GameObject whatIsShown)
        {
            Key = key;
            DuoLand = duoLand;
            shown = whatIsShown;
            Prompt = "Take";
        }

        // Found in an earlier sitting: it is not here again.
        private void Start()
        {
            if (string.IsNullOrEmpty(Key) || !Progress.Data.Found.Contains(Key)) return;
            Taken = true;
            if (shown != null) shown.SetActive(false);
        }

        public override void Use(PlayerInteraction who)
        {
            // One with no key could never be told apart from another: it gives nothing and is left as it is.
            if (Taken || who == null || string.IsNullOrEmpty(Key)) return;
            Taken = true;
            Paid = MemoryPayout.Found(income, Key, DuoLand, transform.position + Vector3.up, who.transform);
            if (shown != null) shown.SetActive(false);
        }
    }
}

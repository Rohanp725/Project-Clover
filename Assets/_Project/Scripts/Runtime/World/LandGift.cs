using Clover.Core;
using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>Something left in a land's side path that is taken once: a waterskin (one more flask) or embers left for the dead.</summary>
    public class LandGift : Interactable
    {
        [SerializeField] private GameObject shown;

        /// <summary>Above nothing: the belt is made to carry this many flasks.</summary>
        public int FlasksAfter;
        public int Embers;
        /// <summary>The status line shown when it is taken.</summary>
        public string Words = "";

        public bool Taken { get; private set; }
        public override bool Available => !Taken;

        public void Configure(GameObject whatIsShown) => shown = whatIsShown;

        public override void Use(PlayerInteraction who)
        {
            if (Taken) return;
            Taken = true;
            if (FlasksAfter > 0 && who.Defence != null && who.Defence.Flasks.Maximum < FlasksAfter)
            {
                who.Defence.Flasks.SetMaximum(FlasksAfter);
                who.Defence.Flasks.Refill();
            }
            var upgrades = who.GetComponent<PlayerUpgrades>();
            if (Embers > 0 && upgrades != null) upgrades.Earn(Embers);
            if (shown != null) shown.SetActive(false);
            Vfx.Play(VfxKind.Ring, transform.position + Vector3.up * 0.3f, Vector3.up);
            if (who.Defence != null && !string.IsNullOrEmpty(Words)) who.Defence.Callout(Words);
        }
    }
}

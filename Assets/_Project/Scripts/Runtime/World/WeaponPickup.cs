using Clover.Core;
using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>A weapon waiting in the world. Taking it puts it in the player's hands in place of what they carried.</summary>
    public class WeaponPickup : Interactable
    {
        [SerializeField] private GameObject shown;

        /// <summary>A name from <see cref="Weapons"/>.</summary>
        public string WeaponName = Weapons.KatanaName;

        public bool Taken { get; private set; }
        public override bool Available => !Taken;

        public void Configure(string weaponName, GameObject whatIsShown)
        {
            WeaponName = weaponName;
            shown = whatIsShown;
        }

        public override void Use(PlayerInteraction who)
        {
            if (Taken || who.Combat == null) return;
            Taken = true;
            who.Combat.Equip(Weapons.ByName(WeaponName));
            // Once drawn, a later run may begin with it.
            Progress.Unlock(Progress.WeaponKey(WeaponName));
            if (shown != null) shown.SetActive(false);
            Vfx.Play(VfxKind.Burst, transform.position + Vector3.up * 1.2f);
            Vfx.Play(VfxKind.Ring, transform.position + Vector3.up * 0.3f, Vector3.up);
            if (who.Defence != null) who.Defence.Callout(WeaponName);
        }
    }
}

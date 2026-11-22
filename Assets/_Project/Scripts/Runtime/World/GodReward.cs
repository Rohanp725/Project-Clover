using Clover.Core;
using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// One of the two things a fallen god leaves: its element to be taken, or its power to be consumed. Taking gives the
    /// god's element and its sword, and the element the player had is gone. Consuming keeps the player's element and
    /// makes it stronger, with a plain sword. Choosing one ends the other.
    /// </summary>
    public class GodReward : Interactable
    {
        [SerializeField] private SummitDirector director;
        [SerializeField] private SunCourtDirector court;

        /// <summary>True to take the god's element; false to consume it.</summary>
        public bool Takes;
        /// <summary>The element of the god this stands over. The swordsman of the summit is the storm.</summary>
        public string Element = Elements.Lightning;
        /// <summary>Embers a consumed god gives toward the tree the player already has.</summary>
        public int ConsumeEmbers = 4;

        public bool Chosen { get; private set; }
        public override bool Available => !Chosen && isActiveAndEnabled;

        public void Configure(SummitDirector summit, bool takes)
        {
            director = summit;
            Takes = takes;
        }

        /// <summary>For the sun-court, where the first god is simply taken from: there is no second thing to choose.</summary>
        public void Configure(SunCourtDirector sunCourt)
        {
            court = sunCourt;
            Takes = true;
        }

        public override void Use(PlayerInteraction who)
        {
            if (Chosen) return;
            Chosen = true;
            var element = who.GetComponent<PlayerElement>();
            var upgrades = who.GetComponent<PlayerUpgrades>();
            if (Takes)
            {
                // The old element goes, and what was learned of it with it; the embers spent on it come back.
                // Someone who already carries this god's element keeps what they learned of it: there is no old tree to lose.
                bool sameElement = element != null && element.Kit != null && element.Kit.Name == Element;
                // Someone who holds no element (the Fire land is entered with none) has no tree to lose either.
                bool heldNone = element != null && !element.HasElement;
                // In the playtest nothing learned is ever lost: the old tree's marks stay, for when it is held again.
                if (upgrades != null && !heldNone && !PlaytestMode.On)
                {
                    if (!sameElement)
                    {
                        int spent = 0;
                        // The Way and the Body are paid in Memories and kept for good: nothing of theirs is refunded or forgotten.
                        foreach (var upgrade in Upgrade.All)
                            if (!upgrade.IsWay && upgrades.Owns(upgrade.Id)) spent += upgrade.Cost;
                        upgrades.Forget();
                        upgrades.Refund(spent);
                    }
                }
                RunState.Element = Element;
                // Once taken from its god, a later run may begin with it.
                Progress.Unlock(Progress.ElementKey(Element));
                RunState.ElementConsumed = false;
                if (element != null) element.Take(ElementKit.ByName(Element, false));
                // The god's own gift comes with his element: the Signature at the crown of its tree, worn if it is a spell.
                if (upgrades != null) upgrades.Give(Gifts.SignatureOf(Element));
                // And the god's own weapon: the storm's katana, the sun's spear.
                string godWeapon = Weapons.OfGod(Element);
                if (godWeapon != null && who.Combat != null) who.Combat.Arm(godWeapon);
                if (Element == Elements.Lightning)
                {
                    LightningBolts.Strike(who.transform.position, 1f);
                    if (who.Defence != null) who.Defence.Callout("The storm is yours");
                }
                else
                {
                    Vfx.Play(VfxKind.Fire, who.transform.position + Vector3.up, Vector3.up, 3f);
                    if (who.Defence != null) who.Defence.Callout(Element + " is yours.");
                }
            }
            else
            {
                if (string.IsNullOrEmpty(RunState.Element)) RunState.Element = element != null ? element.Kit.Name : ElementKit.FireName;
                RunState.ElementConsumed = true;
                // The god's element, swallowed, is noted; it gives no duo crown (the owner's decision of 2026-11-20).
                RunState.ElementsConsumed.Add(Element);
                if (element != null) element.Take(ElementKit.ByName(RunState.Element, true));
                // What is consumed only strengthens what the player already has: its Shards feed the element owned.
                if (upgrades != null) upgrades.Earn(ConsumeEmbers);
                if (who.Combat != null) who.Combat.Arm(Weapons.KatanaName);
                Vfx.Play(VfxKind.Fire, who.transform.position + Vector3.up, Vector3.up, 3f);
                if (who.Defence != null) who.Defence.Callout("The storm feeds the fire");
            }
            Vfx.Play(VfxKind.Ring, who.transform.position + Vector3.up * 0.2f, Vector3.up);
            ScreenFlash.Flash(new Color(1f, 1f, 1f, 0.4f), 0.09f);
            if (who.Defence != null) who.Defence.Restore();
            if (director != null) director.Chose(this);
            if (court != null) court.Chose(this);
            // A god taken or consumed is a save point (IMPLEMENTATION.md 5.1).
            Clover.Core.Save.SavePoints.GodRewarded();
        }
    }
}

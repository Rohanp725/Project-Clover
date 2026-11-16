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

        /// <summary>True to take the god's element; false to consume it.</summary>
        public bool Takes;
        /// <summary>Embers a consumed god gives toward the tree the player already has.</summary>
        public int ConsumeEmbers = 4;

        public bool Chosen { get; private set; }
        public override bool Available => !Chosen && isActiveAndEnabled;

        public void Configure(SummitDirector summit, bool takes)
        {
            director = summit;
            Takes = takes;
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
                bool sameElement = element != null && element.Kit != null && element.Kit.Name == ElementKit.LightningName;
                if (upgrades != null)
                {
                    if (!sameElement)
                    {
                        int spent = 0;
                        foreach (var upgrade in Upgrade.All)
                            if (upgrades.Owns(upgrade.Id)) spent += upgrade.Cost;
                        upgrades.Forget();
                        upgrades.Earn(spent);
                    }
                    // His own way of moving comes with his element.
                    RunState.Unlocked.Add(Upgrade.StormStep);
                }
                RunState.Element = ElementKit.LightningName;
                // Once taken from its god, a later run may begin with it.
                Progress.Unlock(Progress.ElementKey(ElementKit.LightningName));
                RunState.ElementConsumed = false;
                if (element != null) element.Take(ElementKit.Lightning());
                if (who.Combat != null) who.Combat.Equip(Weapons.ByName(Weapons.StormKatanaName));
                LightningBolts.Strike(who.transform.position, 1f);
                if (who.Defence != null) who.Defence.Callout("The storm is yours");
            }
            else
            {
                if (string.IsNullOrEmpty(RunState.Element)) RunState.Element = element != null ? element.Kit.Name : ElementKit.FireName;
                RunState.ElementConsumed = true;
                if (element != null) element.Take(ElementKit.ByName(RunState.Element, true));
                // What is consumed feeds the tree the player already has.
                if (upgrades != null) upgrades.Earn(ConsumeEmbers);
                // The storm, swallowed, mixes with the fire.
                if (RunState.Element == ElementKit.FireName) RunState.Unlocked.Add(Upgrade.Plasma);
                if (who.Combat != null) who.Combat.Equip(Weapons.ByName(Weapons.KatanaName));
                Vfx.Play(VfxKind.Fire, who.transform.position + Vector3.up, Vector3.up, 3f);
                if (who.Defence != null) who.Defence.Callout("The storm feeds the fire");
            }
            Vfx.Play(VfxKind.Ring, who.transform.position + Vector3.up * 0.2f, Vector3.up);
            ScreenFlash.Flash(new Color(1f, 1f, 1f, 0.4f), 0.09f);
            if (who.Defence != null) who.Defence.Restore();
            if (director != null) director.Chose(this);
        }
    }
}

using Clover.Core;
using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// One of the things a run can begin with, set out in the hub: a weapon on its stand or an element in its brazier.
    /// What has not yet been won in a run stands dark and cannot be chosen. Choosing puts it in the player's hands at
    /// once, and the run carries it through the gate.
    /// </summary>
    public class LoadoutChoice : Interactable
    {
        [SerializeField] private GameObject whenUnlocked;
        [SerializeField] private GameObject whenChosen;

        /// <summary>True for an element, false for a weapon.</summary>
        public bool IsElement;
        /// <summary>The weapon's or the element's name.</summary>
        public string Choice = Weapons.FistsName;
        /// <summary>True for what every run may begin with, whatever has been won.</summary>
        public bool AlwaysOpen;
        public string OpenPrompt = "Take it";
        public string SealedPrompt = "Empty";
        /// <summary>What is said when the player tries to take what they have not won.</summary>
        public string SealedWords = "Not yet yours";

        public bool IsUnlocked => AlwaysOpen || Progress.Has(IsElement ? Progress.ElementKey(Choice) : Progress.WeaponKey(Choice));
        public int Taken { get; private set; }

        private PlayerCombat combat;
        private PlayerElement element;
        private float untilLook;

        public void Configure(bool isElement, string choice, bool alwaysOpen, GameObject shownWhenUnlocked, GameObject shownWhenChosen)
        {
            IsElement = isElement;
            Choice = choice;
            AlwaysOpen = alwaysOpen;
            whenUnlocked = shownWhenUnlocked;
            whenChosen = shownWhenChosen;
        }

        /// <summary>Whose hands the choice is shown against.</summary>
        public void Watch(PlayerCombat combatToWatch, PlayerElement elementToWatch)
        {
            combat = combatToWatch;
            element = elementToWatch;
        }

        private void Start() => Refresh();

        public override void Use(PlayerInteraction who)
        {
            if (!IsUnlocked)
            {
                if (who.Defence != null) who.Defence.Callout(SealedWords);
                return;
            }
            Taken++;
            if (IsElement)
            {
                var playerElement = who.GetComponent<PlayerElement>();
                RunState.Element = Choice;
                RunState.ElementConsumed = false;
                if (playerElement != null) playerElement.Take(ElementKit.ByName(Choice, false));
                ElementLook.Puff(Elements.ByNameOrFire(Choice), who.transform.position + Vector3.up, Vector3.up, 1.4f);
                ElementLook.Sparks(Elements.ByNameOrFire(Choice), who.transform.position + Vector3.up * 1.2f, Vector3.up, 1.2f);
            }
            else if (who.Combat != null)
            {
                who.Combat.Equip(Weapons.ByName(Choice));
                Vfx.Play(VfxKind.Glint, who.transform.position + Vector3.up * 1.8f);
            }
            Vfx.Play(VfxKind.Ring, who.transform.position + Vector3.up * 0.2f, Vector3.up);
            if (who.Defence != null) who.Defence.Callout(Choice);
            Refresh();
        }

        private void Update()
        {
            // What the player holds can change at another stand; this one is looked at a few times a second.
            untilLook -= Time.deltaTime;
            if (untilLook > 0f) return;
            untilLook = 0.2f;
            Refresh();
        }

        private void Refresh()
        {
            bool unlocked = IsUnlocked;
            Prompt = unlocked ? OpenPrompt : SealedPrompt;
            if (whenUnlocked != null && whenUnlocked.activeSelf != unlocked) whenUnlocked.SetActive(unlocked);
            bool chosen = unlocked && (IsElement
                ? element != null && element.Kit != null && element.Kit.Name == Choice
                : combat != null && combat.Weapon != null && combat.Weapon.Name == Choice);
            if (whenChosen != null && whenChosen.activeSelf != chosen) whenChosen.SetActive(chosen);
        }
    }
}

using Clover.Core;
using Clover.Player;
using Clover.Player.Build;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// One of the things a run can begin with, set out in the hub: a weapon on its stand or an element in its brazier.
    /// What has not yet been won in a run stands dark and cannot be chosen. Choosing puts it in the player's hands at
    /// once, and the run carries it through the gate. An altar of the Proving Ground (<see cref="SetsEitherAttunement"/>)
    /// sets either attunement by <see cref="AltarChoice"/>'s rule, and its prompt says which before it is used.
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
        /// <summary>
        /// An element altar of the Proving Ground: it carries its element first, second or all in, or alone again, by
        /// <see cref="AltarChoice"/>; the hub's braziers leave it off and only ever set the first.
        /// </summary>
        public bool SetsEitherAttunement;
        /// <summary>Said after the altar's offer in its prompt, as "lays Burn".</summary>
        public string AltarNote = "";

        public bool IsUnlocked => AlwaysOpen || Progress.Has(IsElement ? Progress.ElementKey(Choice) : Progress.WeaponKey(Choice));
        public int Taken { get; private set; }

        private PlayerCombat combat;
        private PlayerElement element;
        private float untilLook;
        // The carried elements the altar's prompt was last written for, so it is written again only when they change.
        private int promptFor = -1;

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

        private void Start()
        {
            // The builders' Watch is not kept in the scene (the fields are not serialized), so the player is found here
            // once; without it a stand never shows what is in hand and an altar cannot say what it will do.
            if (combat == null) combat = FindAnyObjectByType<PlayerCombat>();
            if (element == null) element = FindAnyObjectByType<PlayerElement>();
            Refresh();
        }

        public override void Use(PlayerInteraction who)
        {
            if (!IsUnlocked)
            {
                if (who.Defence != null) who.Defence.Callout(SealedWords);
                return;
            }
            Taken++;
            string said = Choice;
            if (IsElement && SetsEitherAttunement && who.GetComponent<PlayerBuild>() is PlayerBuild build && who.GetComponent<PlayerElement>() is PlayerElement carried)
            {
                Carried(carried, out string first, out string second);
                AltarChoice.Next(first, second, Choice, out string newFirst, out string newSecond);
                build.Attune(newFirst, newSecond);
                said = AltarChoice.Said(newFirst, newSecond);
                ElementLook.Puff(Elements.ByNameOrFire(Choice), who.transform.position + Vector3.up, Vector3.up, 1.4f);
                ElementLook.Sparks(Elements.ByNameOrFire(Choice), who.transform.position + Vector3.up * 1.2f, Vector3.up, 1.2f);
            }
            else if (IsElement)
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
                who.Combat.Arm(Choice);
                Vfx.Play(VfxKind.Glint, who.transform.position + Vector3.up * 1.8f);
            }
            Vfx.Play(VfxKind.Ring, who.transform.position + Vector3.up * 0.2f, Vector3.up);
            if (who.Defence != null) who.Defence.Callout(said);
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
            if (unlocked && IsElement && SetsEitherAttunement && element != null)
            {
                if (promptFor != element.Version)
                {
                    promptFor = element.Version;
                    Carried(element, out string first, out string second);
                    string offer = AltarChoice.Prompt(first, second, Choice);
                    Prompt = AltarNote.Length > 0 ? offer + " (" + AltarNote + ")" : offer;
                }
            }
            else
            {
                Prompt = unlocked ? OpenPrompt : SealedPrompt;
                promptFor = -1;
            }
            if (whenUnlocked != null && whenUnlocked.activeSelf != unlocked) whenUnlocked.SetActive(unlocked);
            bool chosen = unlocked && (IsElement
                ? element != null && element.Carries(Choice)
                : combat != null && combat.Weapon != null && combat.Weapon.Name == Choice);
            if (whenChosen != null && whenChosen.activeSelf != chosen) whenChosen.SetActive(chosen);
        }

        // The elements carried now, first and second ("" for none; the first again when all in).
        private static void Carried(PlayerElement carried, out string first, out string second)
        {
            bool holding = carried != null && carried.HasElement && carried.Info != null;
            first = holding ? carried.Info.Name : "";
            second = holding && carried.SecondInfo != null ? carried.SecondInfo.Name : "";
        }
    }
}

using Clover.Core;
using Clover.Player;
using Clover.Player.Forms;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// A stone in the practice yard that lends the player one spell form to try: using it puts the form's sample spell
    /// in the hold-spell slot, in the element that suits it, and fills the mana. A stone with no form named gives the
    /// next element round the ring instead and leaves the spell alone. While any spell is lent the stones keep the
    /// mana coming back, so a form can be cast again and again. The yard's builder makes these; no other scene has them.
    /// </summary>
    public class FormStone : Interactable
    {
        /// <summary>The form this stone lends (a name from <see cref="SpellForms"/>); empty for the stone that changes the element.</summary>
        public string Form = "";
        /// <summary>Mana given back each second while a spell is lent.</summary>
        public float ManaPerSecond = 45f;

        public int Used { get; private set; }

        [SerializeField] private PlayerUpgrades upgrades;
        [SerializeField] private PlayerCombat combat;
        [SerializeField] private bool feeds;

        public void Configure(string formToLend, string prompt, bool keepsManaComing)
        {
            Form = formToLend;
            Prompt = prompt;
            feeds = keepsManaComing;
        }

        /// <summary>Whose mana the stone keeps coming.</summary>
        public void Watch(PlayerUpgrades upgradesToWatch, PlayerCombat combatToFeed)
        {
            upgrades = upgradesToWatch;
            combat = combatToFeed;
        }

        public override void Use(PlayerInteraction who)
        {
            Used++;
            var element = who.GetComponent<PlayerElement>();
            var worn = who.GetComponent<PlayerUpgrades>();
            if (element == null || worn == null) return;
            if (string.IsNullOrEmpty(Form))
            {
                var next = Elements.All[(element.Info.Index + 1) % Elements.Count];
                Take(element, next.Name, who);
                if (who.Defence != null) who.Defence.Callout(next.Name);
                return;
            }
            var sample = FormSamples.Of(Form);
            if (sample == null || SpellForms.Get(Form) == null)
            {
                if (who.Defence != null) who.Defence.Callout("Not made yet");
                return;
            }
            if (element.Info.Name != sample.Element) Take(element, sample.Element, who);
            worn.Lend(sample);
            if (who.Combat != null) who.Combat.Mana.Gain(who.Combat.Mana.Maximum);
            Vfx.Play(VfxKind.Glint, who.transform.position + Vector3.up * 1.8f);
            if (who.Defence != null) who.Defence.Callout(sample.Name);
        }

        private static void Take(PlayerElement element, string name, PlayerInteraction who)
        {
            RunState.Element = name;
            RunState.ElementConsumed = false;
            element.Take(ElementKit.ByName(name, false));
            ElementLook.Puff(Elements.ByNameOrFire(name), who.transform.position + Vector3.up, Vector3.up, 1.4f);
            ElementLook.Ring(Elements.ByNameOrFire(name), who.transform.position + Vector3.up * 0.2f);
        }

        private void Update()
        {
            if (!feeds || upgrades == null || combat == null || upgrades.Lent == null) return;
            combat.Mana.Gain(ManaPerSecond * Time.deltaTime);
        }
    }
}

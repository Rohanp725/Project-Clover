using Clover.Player;
using UnityEngine;
using UnityEngine.UI;

namespace Clover.UI
{
    /// <summary>
    /// A line of text that names buttons: its template's {Action} names are filled in for the device in use, and filled
    /// in again only when the device or a binding changes.
    /// </summary>
    public class PromptLine : MonoBehaviour
    {
        /// <summary>The quiet line most scenes keep at the foot of the screen.</summary>
        public const string PauseTemplate = "{Pause}: pause and controls";

        /// <summary>The practice yard's fuller line.</summary>
        public const string FightTemplate =
            "{LightAttack} light   {HeavyAttack} heavy   {Guard} guard   {Dodge} dodge   {Flask} flask   {WeaponSkill} skill   {Surge} surge   {Interact} use   {Pause} pause\n"
            + "Spells: {Spells}.   The weapon's skill costs mana.   Heavy held: the charged heavy.   Heavy on a staggered enemy: finisher.";

        [SerializeField] private Text text;
        [SerializeField] private string template;

        private int shownVersion = -1;

        public string Template => template;

        public void Configure(Text label, string words)
        {
            text = label;
            template = words;
            shownVersion = -1;
        }

        /// <summary>
        /// Gives each line named "Controls" directly under <paramref name="hud"/> a prompt of its own, by what it says:
        /// the scenes were built with fixed PlayStation-and-keyboard words, and this keeps them right without a rebuild.
        /// </summary>
        public static void Attach(Transform hud)
        {
            if (hud == null) return;
            for (int i = 0; i < hud.childCount; i++)
            {
                var child = hud.GetChild(i);
                if (child.name != "Controls") continue;
                var label = child.GetComponent<Text>();
                if (label == null || child.GetComponent<PromptLine>() != null) continue;
                string words = label.text ?? "";
                string chosen = words.Contains("pause and controls") ? PauseTemplate : words.Length > 0 ? FightTemplate : null;
                if (chosen == null) continue;
                child.gameObject.AddComponent<PromptLine>().Configure(label, chosen);
            }
        }

        private void Update()
        {
            if (text == null || shownVersion == ControlDevices.Version) return;
            shownVersion = ControlDevices.Version;
            text.text = ControlNames.Expand(template);
        }
    }
}

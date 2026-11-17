using Clover.Player;
using Clover.UI;
using UnityEngine;

namespace Clover.World
{
    /// <summary>A sign the player may read: its words come up in the reading box and are clicked through at the player's own pace.</summary>
    public class Notice : Interactable
    {
        public string Speaker = "A notice";
        [TextArea] public string[] Lines = new string[0];

        public int TimesRead { get; private set; }

        public override bool Available => Lines != null && Lines.Length > 0;

        public override void Use(PlayerInteraction who)
        {
            if (!Available) return;
            TimesRead++;
            ReadingBox.Show(Speaker, Lines);
        }
    }
}

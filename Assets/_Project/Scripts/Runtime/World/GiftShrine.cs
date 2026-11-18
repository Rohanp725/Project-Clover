using Clover.Core;
using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// A stone for trying the gods' gifts while only one god stands in the game: the yard has two. One gives the
    /// Signature of the element held, as taking it from its god would. The other marks the next element round the
    /// ring as consumed, which opens the branch the two share and gives its crown, as consuming that god would.
    /// Nothing here is part of a run: it is for the test yard only.
    /// </summary>
    public class GiftShrine : Interactable
    {
        /// <summary>True for the stone that consumes the next element; false for the one that gives the held element's own gift.</summary>
        public bool Consumes;

        public int Used { get; private set; }
        /// <summary>The id of what it last gave, or empty.</summary>
        public string LastGift { get; private set; } = "";

        public void Configure(bool consumes, string prompt)
        {
            Consumes = consumes;
            Prompt = prompt;
        }

        public override string CardStamp => Used.ToString();

        public override string Card(PlayerElement element)
        {
            string held = element.Info.Name;
            if (!Consumes)
            {
                var own = Gifts.SignatureOf(held);
                if (own == null) return $"{held} has no god's gift yet.";
                return $"GOD'S GIFT OF {held.ToUpperInvariant()}: {own.Name}\n{own.Description}\n{TriggerWords.HowTo(own)}\nPress use ({ControlNames.Of("Interact")}) to be given it, as if you took {held} from its god.";
            }
            var next = Elements.All[(element.Info.Index + 1) % Elements.Count];
            var crown = Gifts.CrownOf(held, next.Name);
            if (crown == null) return $"{held} and {next.Name} share no branch.";
            return $"CONSUME {next.Name.ToUpperInvariant()} WHILE HOLDING {held.ToUpperInvariant()}: opens {element.Info.MixName} and gives its crown, {crown.Name}\n{crown.Description}\n{TriggerWords.HowTo(crown)}\nPress use ({ControlNames.Of("Interact")}) to be given it.";
        }

        public override void Use(PlayerInteraction who)
        {
            Used++;
            var element = who.GetComponent<PlayerElement>();
            var upgrades = who.GetComponent<PlayerUpgrades>();
            if (element == null || upgrades == null) return;
            string held = element.Info.Name;
            Upgrade gift;
            if (Consumes)
            {
                string next = Elements.All[(element.Info.Index + 1) % Elements.Count].Name;
                RunState.ElementsConsumed.Add(next);
                gift = Gifts.CrownOf(held, next);
            }
            else gift = Gifts.SignatureOf(held);
            LastGift = gift != null ? gift.Id : "";
            bool given = upgrades.Give(gift);
            ElementLook.Ring(element.Info, who.transform.position + Vector3.up * 0.2f);
            Vfx.Play(VfxKind.Glint, who.transform.position + Vector3.up * 1.8f);
            if (who.Defence != null) who.Defence.Callout(gift == null ? "Nothing to give" : given ? gift.Name : gift.Name + " is yours already");
        }
    }
}

using Clover.Core;
using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// A stone for trying the gods' gifts while only one god stands in the game: the yard has two. One gives the
    /// Signature of the element held, as taking it from its god would. The other marks the next element round the
    /// ring as consumed and gives the Shards consuming a god gives, which only strengthen the element held: no god gives
    /// a duo crown (the owner's decision of 2026-11-20). Nothing here is part of a run: it is for the test yard only.
    /// </summary>
    public class GiftShrine : Interactable
    {
        /// <summary>True for the stone that consumes the next element; false for the one that gives the held element's own gift.</summary>
        public bool Consumes;

        /// <summary>The Shards consuming gives, as <see cref="GodReward.ConsumeEmbers"/> does.</summary>
        public int ConsumeShards = 4;

        /// <summary>The consuming stone's prompt. The scene built before 2026-11-20 holds the old words, so it is set again on waking.</summary>
        public const string ConsumePrompt = "Debug shrine: consume the next element round the ring (4 Shards; no god gives a duo crown)";

        private void Awake()
        {
            if (Consumes) Prompt = ConsumePrompt;
        }

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
            return $"CONSUME {next.Name.ToUpperInvariant()} WHILE HOLDING {held.ToUpperInvariant()}: {ConsumeShards} Shards for {held}. A god gives no duo crown: the duo branch's spells are bought with Shards once both elements are carried.\nPress use ({ControlNames.Of("Interact")}) to consume it.";
        }

        public override void Use(PlayerInteraction who)
        {
            Used++;
            var element = who.GetComponent<PlayerElement>();
            var upgrades = who.GetComponent<PlayerUpgrades>();
            if (element == null || upgrades == null) return;
            string held = element.Info.Name;
            if (Consumes)
            {
                // Consuming strengthens the element held, and gives no duo crown.
                string next = Elements.All[(element.Info.Index + 1) % Elements.Count].Name;
                RunState.ElementsConsumed.Add(next);
                upgrades.Earn(ConsumeShards);
                LastGift = "";
                ElementLook.Ring(element.Info, who.transform.position + Vector3.up * 0.2f);
                if (who.Defence != null) who.Defence.Callout($"{next} consumed: {ConsumeShards} Shards");
                return;
            }
            var gift = Gifts.SignatureOf(held);
            LastGift = gift != null ? gift.Id : "";
            bool given = upgrades.Give(gift);
            ElementLook.Ring(element.Info, who.transform.position + Vector3.up * 0.2f);
            Vfx.Play(VfxKind.Glint, who.transform.position + Vector3.up * 1.8f);
            if (who.Defence != null) who.Defence.Callout(gift == null ? "Nothing to give" : given ? gift.Name : gift.Name + " is yours already");
        }
    }
}

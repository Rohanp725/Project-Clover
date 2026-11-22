using System.Collections.Generic;
using Clover.Player;
using Clover.UI.Toolkit;

namespace Clover.Tests
{
    /// <summary>
    /// A stand-in build for the Loadout's tests, in the mockup's state (docs/design/mockups/loadout_c.html): the Katana in
    /// the Sheath at Temper II, The Cut Lands at the Click set, the Light Body set, Fire and Water carried, Flame Wave,
    /// Deep Crack, Whirlpool and Kiln Stance worn with Kindle, Hand 1 worn, Hand 3 empty. It changes itself as the real
    /// build would and counts every call.
    /// </summary>
    public sealed class FakeLoadoutSource : ILoadoutSource
    {
        public readonly LoadoutBoard Board = new LoadoutBoard();
        public readonly List<string> Calls = new List<string>();
        public readonly List<SpellCard> Learned = new List<SpellCard>();
        public readonly List<WayCard> Aspects = new List<WayCard>();
        public readonly Dictionary<string, WayCard[]> KeystonesOf = new Dictionary<string, WayCard[]>();

        public static SpellCard Spell(string id, string name, string element, SpellRole role, float cooldown, params string[] shapes) => new SpellCard
        {
            Id = id, Name = name, Element = element, Role = role, Cooldown = cooldown, Line = name + " does a thing.", ShapeKey = id,
            Shapes = shapes, ShapeHint = shapes.Length == 0 ? "Buy " + name + "'s Shape on the element chart. 3 Shards." : "",
        };

        private static WayCard Keystone(string id, string name, string gives, bool bought, bool set) => new WayCard
        {
            Id = id, Name = name, Kind = "Keystone of the Sheath", Words = name + " does a thing with a draw.", Gives = gives, Bought = bought, Set = set,
            Price = 4, Lock = bought ? "" : "Buy it on the Way chart. 4 Memories.", Icon = IconShapes.Star,
        };

        public FakeLoadoutSource()
        {
            Board.Memories = 12;
            Board.Shards = 340;
            Board.Weapon = "Katana";
            Board.Temper = 2;
            Board.TemperNextPrice = 2;
            Board.Elements = new[] { "Fire", "Water" };
            Board.StrikeFirst = "Fire";
            Board.StrikeSecond = "Water";
            Board.Technique = new WayCard { Id = "katana.beforethethunder", Name = "Before the Thunder", Lock = "Sealed until the Last Retainer falls." };

            var sheathKeys = new[]
            {
                Keystone("katana.sheath.click", "The Cut Lands at the Click", "the long held draw that cuts through guards", true, true),
                Keystone("katana.sheath.hundreddays", "A Hundred Days at the Shrine", "the quick draws that leave cuts in the air", true, false),
            };
            var streamKeys = new[]
            {
                Keystone("katana.stream.nolastcut", "No Last Cut", "the combo that loops", false, false),
                Keystone("katana.stream.noshape", "The Water Has No Shape", "the slides that cut", false, false),
            };
            KeystonesOf["katana.sheath"] = sheathKeys;
            KeystonesOf["katana.stream"] = streamKeys;
            KeystonesOf["katana.answer"] = new[] { new WayCard(), new WayCard() };
            Aspects.Add(new WayCard { Id = "katana.sheath", Name = "The Sheath", Kind = "Aspect of the Katana", Words = "Iai.", Bought = true, Set = true, MarksBought = 6, MarksTotal = 10, Keystones = sheathKeys });
            Aspects.Add(new WayCard { Id = "katana.stream", Name = "The Stream", Kind = "Aspect of the Katana", Words = "Flow.", Bought = true, MarksBought = 2, MarksTotal = 10, Keystones = streamKeys });
            Aspects.Add(new WayCard { Id = "katana.answer", Name = "The Answer", Kind = "Aspect of the Katana", Words = "Counter.", Lock = "Buy it on the Way chart. 2 Memories.", MarksTotal = 10, Keystones = KeystonesOf["katana.answer"] });
            Board.Bearings = new[]
            {
                new WayCard { Id = "body.enduring", Name = "The Enduring Body", Kind = "Bearing of the Body", Short = "Hits push you less", Gives = "the toughness", Bought = true },
                new WayCard { Id = "body.light", Name = "The Light Body", Kind = "Bearing of the Body", Short = "Quicker dodges", Gives = "the second dodge", Bought = true, Set = true },
            };

            Learned.Add(Spell("fire.base", "Flame Wave", "Fire", SpellRole.Opener, 7f, "Cinder Fan", "Long Wave"));
            Learned.Add(Spell("fire.kiln", "Kiln Stance", "Fire", SpellRole.Counter, 9f, "The Kiln", "Open Kiln"));
            Learned.Add(Spell("fire.dash", "Ember Dash", "Fire", SpellRole.Movement, 6f));
            Learned.Add(Spell("water.base", "Whirlpool", "Water", SpellRole.Breaker, 10f, "Maelstrom", "Eddy"));
            Learned.Add(Spell("water.crack", "Deep Crack", "Water", SpellRole.Detonator, 11f, "Ink", "Spout"));
            var duo = Spell("steam.cloud", "Steam Cloud", "Fire", SpellRole.Breaker, 12f);
            duo.Element2 = "Water";
            Learned.Add(duo);
            Wear(0, "fire.base");
            Wear(1, "water.crack");
            Wear(2, "water.base");
            Wear(3, "fire.kiln");
            Board.Surge = new SpellCard { Id = "fire.surge", Name = "Kindle", Element = "Fire", IsSurge = true, Mana = 40f, Line = "Your fire burns hotter for a while." };
            SetAspectCards("katana.sheath");

            Board.Hands[0].Empty = false;
            Board.Hands[1].Empty = false;
            Board.Hands[1].Worn = true;
            Board.Hands[2].Empty = false;
            for (int i = 0; i < 4; i++)
            {
                Board.Hands[0].Slots[i] = Board.Slots[i].Clone();
                Board.Hands[1].Slots[i] = Board.Slots[i].Clone();
            }
            // Hand 2: all Water, no opener and no counter.
            Board.Hands[2].Slots[0] = Find("water.base");
            Board.Hands[2].Slots[1] = Find("water.crack");
            Board.Hands[2].Slots[2] = new SpellCard();
            Board.Hands[2].Slots[3] = new SpellCard();
        }

        public SpellCard Find(string id)
        {
            foreach (var spell in Learned)
                if (spell.Id == id) return spell.Clone();
            return new SpellCard();
        }

        private void Wear(int slot, string id)
        {
            var spell = Find(id);
            spell.Shape = Board.Slots[slot] != null && Board.Slots[slot].Id == id ? Board.Slots[slot].Shape : 0;
            Board.Slots[slot] = spell;
        }

        private void SetAspectCards(string aspect)
        {
            foreach (var card in Aspects)
            {
                card.Set = card.Id == aspect;
                if (card.Set) Board.Aspect = card;
            }
            Board.Keystones = KeystonesOf[aspect];
        }

        public LoadoutBoard Read()
        {
            Calls.Add("Read");
            return Board;
        }

        public void SpellChoices(int slot, List<SpellCard> into)
        {
            into.Clear();
            if (slot == 4)
            {
                into.Add(Board.Surge.Clone());
                into.Add(new SpellCard { Id = "water.surge", Name = "High Tide", Element = "Water", IsSurge = true, Mana = 40f });
                return;
            }
            foreach (var spell in Learned)
            {
                var row = spell.Clone();
                for (int i = 0; i < 4; i++)
                    if (Board.Slots[i].Id == row.Id) row.InSlot = i;
                into.Add(row);
            }
        }

        public void AspectChoices(List<WayCard> into)
        {
            into.Clear();
            into.AddRange(Aspects);
        }

        public bool WearSpell(int slot, string id, int shape)
        {
            Calls.Add($"WearSpell {slot} {id} {shape}");
            if (slot == 4)
            {
                foreach (var surge in new[] { "fire.surge", "water.surge" })
                    if (surge == id) Board.Surge = new SpellCard { Id = id, Name = id == "fire.surge" ? "Kindle" : "High Tide", IsSurge = true };
                return true;
            }
            var was = Board.Slots[slot];
            for (int i = 0; i < 4; i++)
                if (i != slot && Board.Slots[i].Id == id && id.Length > 0) Board.Slots[i] = was;
            Board.Slots[slot] = id.Length == 0 ? new SpellCard() : Find(id);
            Board.Slots[slot].Shape = shape;
            return true;
        }

        public bool ChooseShape(string shapeKey, int shape)
        {
            Calls.Add($"ChooseShape {shapeKey} {shape}");
            foreach (var spell in Board.Slots)
                if (spell.ShapeKey == shapeKey) spell.Shape = shape;
            return true;
        }

        public bool SetAspect(string aspectId)
        {
            Calls.Add("SetAspect " + aspectId);
            SetAspectCards(aspectId);
            return true;
        }

        public bool SetPair(bool bearing, string markId)
        {
            Calls.Add($"SetPair {bearing} {markId}");
            foreach (var mark in bearing ? Board.Bearings : Board.Keystones) mark.Set = mark.Id == markId;
            return true;
        }

        public bool WearHand(int hand)
        {
            Calls.Add("WearHand " + hand);
            for (int i = 0; i < 4; i++) Board.Slots[i] = Board.Hands[hand].Slots[i].Clone();
            for (int i = 0; i < 4; i++) Board.Hands[i].Worn = i == hand;
            return true;
        }

        public bool KeepHand(int hand)
        {
            Calls.Add("KeepHand " + hand);
            Board.Hands[hand].Empty = false;
            for (int i = 0; i < 4; i++) Board.Hands[hand].Slots[i] = Board.Slots[i].Clone();
            return true;
        }
    }
}

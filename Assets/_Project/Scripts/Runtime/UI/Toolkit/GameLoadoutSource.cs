using System.Collections.Generic;
using Clover.Core;
using Clover.Player;
using Clover.Player.Build;
using Clover.Player.Magic;
using Clover.Player.Trees.Ways;
using Clover.Player.Way;

namespace Clover.UI.Toolkit
{
    /// <summary>
    /// The Loadout's source in the game: the player's build (the Hand, the surge, the shapes, the saved Hands), the run's
    /// Way choices for the carried weapon (Q5: only that weapon), the Body's bearings, Temper, the technique, and the
    /// wallet. Changes go through the same doors the old Hand and shapes screens used (<see cref="PlayerBuild.Apply"/>,
    /// <see cref="PlayerBuild.ChooseShape"/>, <see cref="WayRules.ChooseAspect"/>, <see cref="WayRules.ChooseKeystone"/>),
    /// then the marks in force are worked out again and the weapon is re-armed (way_build_plan.md 2.0). Made only at a
    /// rest fire, so every change is allowed.
    /// </summary>
    public sealed class GameLoadoutSource : ILoadoutSource
    {
        private const bool AtRestFire = true;

        private readonly PlayerBuild build;
        private readonly PlayerCombat combat;
        private readonly PlayerUpgrades upgrades;
        private readonly LoadoutBoard board = new LoadoutBoard();
        private readonly List<HandChoice> handChoices = new List<HandChoice>();
        private readonly Dictionary<string, string> lines = new Dictionary<string, string>();

        public GameLoadoutSource(PlayerBuild build, PlayerCombat combat, PlayerUpgrades upgrades)
        {
            this.build = build;
            this.combat = combat;
            this.upgrades = upgrades;
        }

        public static GameLoadoutSource For(PlayerController player) =>
            player == null || player.GetComponent<PlayerBuild>() == null ? null
                : new GameLoadoutSource(player.GetComponent<PlayerBuild>(), player.GetComponent<PlayerCombat>(), player.GetComponent<PlayerUpgrades>());

        private static IReadOnlyList<Upgrade> Catalogue => Upgrade.All;
        private static ICollection<string> Learned => RunState.Unlocked;

        // ---- Reading.

        public LoadoutBoard Read()
        {
            var loadout = build.Read();
            var open = build.Open;
            board.Memories = Progress.Data != null && Progress.Data.Wallet != null ? Progress.Data.Wallet.Memories : 0;
            board.Shards = RunState.Points;

            string kind = WayRules.KindOf(loadout.Weapon);
            board.Weapon = kind;
            string active = WayRules.ActiveAspect(loadout, kind);
            var aspect = Upgrade.ById(active);
            board.Aspect = aspect != null ? AspectCard(aspect, loadout, kind) : new WayCard();
            board.Keystones = KeystonesOf(active, loadout);
            board.Bearings = BearingsOf(loadout);
            board.Temper = WayRules.TemperRank(kind, Learned, Catalogue);
            board.TemperNextPrice = board.Temper < Temper.Ranks ? Temper.PriceOf(board.Temper + 1) : 0;
            board.Technique = TechniqueOf(kind);

            board.Elements[0] = loadout.First;
            board.Elements[1] = loadout.Second;
            board.Devotion = loadout.AllIn ? DevotionOf(loadout.First) : "";
            board.StrikeFirst = HandRules.StrikeElement(loadout, false, loadout.AllIn, out _);
            board.StrikeSecond = HandRules.StrikeElement(loadout, true, loadout.AllIn, out bool deep);
            board.StrikeSecondDeep = deep;

            LearnLines(loadout);
            for (int i = 0; i < Loadout.SlotCount; i++)
            {
                var card = SpellOf(loadout.SlotAt(i), loadout);
                card.Open = open.SlotOpen(i);
                if (!card.Open)
                {
                    card.ShutReason = i == 2 ? "Opens at the Champion" : "Opens at the Fire god";
                    card.ShutDetail = i == 2 ? "This slot opens when the Champion of the Gate falls. It stays open from then on."
                        : "This slot opens when the Fire god falls. It stays open from then on.";
                }
                board.Slots[i] = card;
            }
            board.Surge = SurgeOf(loadout.Surge);
            board.Surge.Open = open.Surge;
            if (!open.Surge)
            {
                board.Surge.ShutReason = "Not open yet";
                board.Surge.ShutDetail = "The surge opens later in the run.";
            }

            HandOf(board.Hands[0], build.Suggested(), loadout);
            for (int i = 0; i < RunState.SavedHands.Length && i + 1 < board.Hands.Length; i++)
                HandOf(board.Hands[i + 1], PlayerBuild.SavedHand(i), loadout);
            return board;
        }

        public void SpellChoices(int slot, List<SpellCard> into)
        {
            into.Clear();
            var loadout = build.Read();
            if (slot == 4)
            {
                build.Rules.SurgesFor(loadout, Learned, handChoices);
                foreach (var choice in handChoices)
                {
                    var surge = SurgeOf(choice.Id);
                    if (!string.IsNullOrEmpty(choice.Line)) surge.Line = choice.Line;
                    into.Add(surge);
                }
                return;
            }
            build.Rules.ChoicesFor(slot, loadout, Learned, handChoices);
            foreach (var choice in handChoices)
            {
                var card = SpellOf(choice.Id, loadout);
                if (!string.IsNullOrEmpty(choice.Line)) card.Line = choice.Line;
                card.InSlot = choice.InSlot;
                into.Add(card);
            }
        }

        public void AspectChoices(List<WayCard> into)
        {
            into.Clear();
            var loadout = build.Read();
            string kind = WayRules.KindOf(loadout.Weapon);
            if (kind.Length == 0) return;
            foreach (var mark in Catalogue)
            {
                if (mark.Kind != UpgradeKind.Aspect || mark.Weapon != kind || !mark.IsWay) continue;
                // A god's bough shows once taken (or on the playtest's rack).
                if (WayRules.IsGodBough(mark) && !Learned.Contains(mark.Id) && !PlaytestMode.On) continue;
                into.Add(AspectCard(mark, loadout, kind));
            }
        }

        // ---- Changing.

        public bool WearSpell(int slot, string id, int shape)
        {
            var hand = build.Read().Clone();
            id ??= "";
            if (slot == 4) hand.Surge = id;
            else
            {
                // A spell worn in another slot changes places with what was here (3.5.5).
                string was = hand.SlotAt(slot);
                for (int i = 0; i < Loadout.SlotCount; i++)
                    if (i != slot && id.Length > 0 && hand.Slots[i] == id) hand.Slots[i] = was;
                hand.Slots[slot] = id;
            }
            if (!build.Apply(hand, AtRestFire)) return false;
            if (slot < 4 && id.Length > 0 && ShapeMarkOf(id) != null && build.Read().ShapeOf(id) != shape) build.ChooseShape(id, shape, AtRestFire);
            return true;
        }

        public bool ChooseShape(string shapeKey, int shape) => build.ChooseShape(shapeKey, shape, AtRestFire);

        public bool SetAspect(string aspectId)
        {
            if (!WayRules.ChooseAspect(RunState.WayChoices, Upgrade.ById(aspectId), Learned, PlaytestMode.On)) return false;
            WayChanged();
            return true;
        }

        public bool SetPair(bool bearing, string markId)
        {
            if (!WayRules.ChooseKeystone(RunState.WayChoices, Upgrade.ById(markId), Learned)) return false;
            WayChanged();
            return true;
        }

        public bool WearHand(int hand) => hand == 0 ? build.Apply(build.Suggested(), AtRestFire) : build.LoadHand(hand - 1, AtRestFire);

        public bool KeepHand(int hand)
        {
            if (hand < 1 || hand > RunState.SavedHands.Length) return false;
            build.SaveHand(hand - 1);
            return true;
        }

        // The Way's choices are the run's; the build reads them on its next read. Marks in force and the weapon follow.
        private void WayChanged()
        {
            build.Read();
            if (upgrades != null) upgrades.RefreshNow();
            if (combat != null) combat.Rearm();
        }

        // ---- Spells.

        // The Hand's own plain line for each spell it could wear now, by id.
        private void LearnLines(Loadout loadout)
        {
            lines.Clear();
            build.Rules.ChoicesFor(0, loadout, Learned, handChoices);
            foreach (var choice in handChoices)
                if (!string.IsNullOrEmpty(choice.Id) && !string.IsNullOrEmpty(choice.Line)) lines[choice.Id] = choice.Line;
        }

        private SpellCard SpellOf(string id, Loadout loadout)
        {
            if (string.IsNullOrEmpty(id)) return new SpellCard();
            string baseOf = HandRules.ElementOfBase(id);
            var mark = baseOf.Length == 0 ? Upgrade.ById(id) : null;
            var kit = baseOf.Length > 0 ? ElementKit.ByName(baseOf, false) : null;
            var timing = SpellTimings.For(id);
            var card = new SpellCard
            {
                Id = id,
                Name = kit != null ? kit.SpellName : mark != null ? mark.Name : id,
                Element = baseOf.Length > 0 ? baseOf : mark != null ? mark.Element ?? "" : "",
                Element2 = mark != null ? mark.Element2 ?? "" : "",
                Role = timing.Role != SpellRole.None ? timing.Role : mark != null ? mark.Role : SpellRole.None,
                Mana = mark != null && mark.Spell != null ? mark.Spell.Cost : timing.Mana,
                Cooldown = mark != null && mark.Spell != null && mark.Spell.Cooldown > 0f ? mark.Spell.Cooldown : timing.Cooldown,
                Details = mark != null ? mark.Description ?? "" : "",
                ShapeKey = id,
            };
            card.Line = lines.TryGetValue(id, out string line) ? line
                : mark != null ? LoadoutWords.FirstSentence(mark.Description) + "." : card.Element + "'s own spell.";
            if (card.Mana > 0f) card.Cooldown = 0f;
            var shapeMark = ShapeMarkOf(id);
            if (shapeMark != null)
            {
                if (Learned.Contains(shapeMark.Id))
                {
                    card.Shapes = new string[shapeMark.Shapes.Length];
                    for (int i = 0; i < shapeMark.Shapes.Length; i++) card.Shapes[i] = shapeMark.Shapes[i].Name;
                    card.Shape = System.Math.Clamp(loadout.ShapeOf(id), 0, card.Shapes.Length - 1);
                }
                else card.ShapeHint = "Buy " + shapeMark.Name + " on the element chart. " + shapeMark.Cost + " Shards.";
            }
            return card;
        }

        // A spell's Shape mark: the one on its branch (its Learn id, or an element's base id).
        private static Upgrade ShapeMarkOf(string id)
        {
            foreach (var mark in Catalogue)
                if (mark.Part == BranchPart.Shape && mark.Branch == id && !mark.IsHidden && mark.Shapes != null && mark.Shapes.Length > 0) return mark;
            return null;
        }

        private static SpellCard SurgeOf(string id)
        {
            var card = new SpellCard { Id = id ?? "", IsSurge = true };
            if (string.IsNullOrEmpty(id)) return card;
            var duo = Upgrade.ById(id);
            if (DuoSurge.Is(duo))
            {
                card.Name = duo.Name;
                card.Element = duo.Element;
                card.Element2 = duo.Element2;
                card.Mana = duo.Surge.Mana;
                card.Line = LoadoutWords.FirstSentence(duo.Description) + ".";
                card.Details = duo.Description;
                return card;
            }
            string element = HandRules.ElementOfSurge(id);
            var kit = element.Length > 0 ? ElementKit.ByName(element, false) : null;
            card.Element = element;
            card.Name = kit != null ? kit.SurgeName : id;
            card.Mana = kit != null ? kit.SurgeCost : 0f;
            card.Cooldown = kit != null && kit.SurgeCost <= 0f ? kit.SurgeCooldown : 0f;
            card.Line = element + "'s surge. Your element runs stronger for " + (kit != null ? SpellCard.Round(kit.SurgeSeconds) : 10) + " seconds.";
            return card;
        }

        private void HandOf(HandCard into, Loadout hand, Loadout worn)
        {
            into.Empty = hand == null;
            into.Worn = false;
            if (hand == null)
            {
                for (int i = 0; i < into.Slots.Length; i++) into.Slots[i] = new SpellCard();
                into.Surge = new SpellCard { IsSurge = true };
                return;
            }
            bool same = (hand.Surge ?? "") == (worn.Surge ?? "");
            for (int i = 0; i < Loadout.SlotCount; i++)
            {
                into.Slots[i] = SpellOf(hand.SlotAt(i), worn);
                same &= hand.SlotAt(i) == worn.SlotAt(i);
            }
            into.Surge = SurgeOf(hand.Surge);
            into.Worn = same;
        }

        // ---- The Way.

        private static WayCard WayCardOf(Upgrade mark, string kind)
        {
            var words = LoadoutWords.For(mark.Id);
            bool bought = Learned.Contains(mark.Id) || (PlaytestMode.On && WayRules.IsGodBough(mark));
            return new WayCard
            {
                Id = mark.Id,
                Name = mark.Name,
                Icon = words.Icon.Length > 0 ? words.Icon : mark.Icon,
                Kind = kind,
                Short = words.Short.Length > 0 ? words.Short : LoadoutWords.FirstSentence(mark.Description),
                Words = mark.Description ?? "",
                Gives = words.Gives,
                Cost = words.Cost,
                Press = words.Press,
                Bought = bought,
                Price = mark.Cost,
                Lock = bought ? "" : mark.Currency == Currency.None ? "Given when its god is taken." : "Buy it on the Way chart. " + LoadoutWords.Memories(mark.Cost) + ".",
            };
        }

        private WayCard AspectCard(Upgrade mark, Loadout loadout, string kind)
        {
            var card = WayCardOf(mark, "Aspect of the " + kind);
            card.Set = WayRules.ActiveAspect(loadout, kind) == mark.Id;
            foreach (var each in Catalogue)
            {
                if (each.Tree != MarkTree.Way || each.Aspect != mark.Id) continue;
                card.MarksTotal++;
                if (Learned.Contains(each.Id)) card.MarksBought++;
            }
            card.Keystones = KeystonesOf(mark.Id, loadout);
            return card;
        }

        private static WayCard[] KeystonesOf(string bough, Loadout loadout)
        {
            var pair = new[] { new WayCard(), new WayCard() };
            if (string.IsNullOrEmpty(bough)) return pair;
            var aspect = Upgrade.ById(bough);
            string kind = "Keystone of " + (aspect != null ? aspect.Name : "this aspect");
            int at = 0;
            foreach (var mark in Catalogue)
            {
                if (at >= 2) break;
                if (!WayRules.IsKeystone(mark) || mark.Tree == MarkTree.Body || mark.Aspect != bough) continue;
                var card = WayCardOf(mark, kind);
                card.Set = loadout.KeystoneOf(bough) == mark.Id;
                pair[at++] = card;
            }
            return pair;
        }

        private static WayCard[] BearingsOf(Loadout loadout)
        {
            var pair = new[] { new WayCard(), new WayCard() };
            int at = 0;
            foreach (var mark in Catalogue)
            {
                if (at >= 2) break;
                if (mark.Tree != MarkTree.Body || !WayRules.IsKeystone(mark)) continue;
                var card = WayCardOf(mark, "Bearing of the Body");
                card.Set = loadout.KeystoneOf(WayRules.BodyBough) == mark.Id;
                pair[at++] = card;
            }
            return pair;
        }

        private static WayCard TechniqueOf(string kind)
        {
            foreach (var mark in Catalogue)
            {
                if (mark.Kind != UpgradeKind.Technique || mark.Weapon != kind) continue;
                var card = WayCardOf(mark, "Technique");
                if (!card.Bought && !string.IsNullOrEmpty(mark.Gate) && !PlaytestMode.On && !Progress.Has(mark.Gate))
                    card.Lock = mark.Gate == KatanaWay.LastRetainerFlag ? "Sealed until the Last Retainer falls." : "Sealed until its elite falls.";
                return card;
            }
            return new WayCard();
        }

        private static string DevotionOf(string element)
        {
            foreach (var mark in Catalogue)
                if (mark.Kind == UpgradeKind.Devotion && mark.Element == element && Learned.Contains(mark.Id)) return mark.Name;
            return "";
        }
    }
}

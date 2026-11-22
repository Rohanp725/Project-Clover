using Clover.Player;

namespace Clover.UI.Toolkit
{
    /// <summary>
    /// The pane's words for whatever has focus (ui_rest_fire_plan.md 4.4 and 3.5.6): the name, the kind line, what it does,
    /// its cost and press, then under the rule the compare block ("Worn here now", "Set now", "You lose") and the one press
    /// that acts here, then the long words and the first keyword's meaning.
    /// </summary>
    public sealed partial class LoadoutController
    {
        private void Describe()
        {
            Pane.Clear();
            if (Mode == LoadoutMode.SpellList) DescribeRow();
            else if (Mode == LoadoutMode.AspectCards) DescribeAspectCard(FocusedCard);
            else
            {
                switch (Focus)
                {
                    case LoadoutPlace.Aspect: DescribeAspect(); break;
                    case LoadoutPlace.Keystone1:
                    case LoadoutPlace.Keystone2:
                    case LoadoutPlace.Bearing1:
                    case LoadoutPlace.Bearing2: DescribePair(); break;
                    case LoadoutPlace.Temper: DescribeTemper(); break;
                    case LoadoutPlace.Technique: DescribeTechnique(); break;
                    case LoadoutPlace.Infused: DescribeInfused(); break;
                    case LoadoutPlace.ElementHeader: DescribeElements(); break;
                    case LoadoutPlace.RoleStrip: DescribeRoles(); break;
                    case LoadoutPlace.Starter:
                    case LoadoutPlace.Hand1:
                    case LoadoutPlace.Hand2:
                    case LoadoutPlace.Hand3: DescribeHand(LoadoutPlaces.HandOf(Focus)); break;
                    default: DescribeSlot(LoadoutPlaces.SlotOf(Focus)); break;
                }
            }
            if (Notice.Length > 0) Pane.Add(PaneSection.Foot, Notice);
            AddKeyword();
        }

        // ---- Spells.

        private void DescribeSpell(SpellCard spell, int shape)
        {
            Pane.Icon = IconShapes.ForElement(spell.Element);
            Pane.IconElement = spell.Element;
            Pane.Title = spell.Name;
            string kind = spell.IsSurge ? spell.Element + " surge" : spell.IsDuo ? spell.Element + " and " + spell.Element2 + " spell" : spell.Element + " spell";
            if (spell.Role != SpellRole.None) kind += " · " + LoadoutWords.RoleName(spell.Role);
            Pane.Kind = kind;
            Pane.Add(PaneSection.Body, spell.Line);
            Pane.Add(PaneSection.Facts, spell.CostLine);
            if (spell.Shapes != null && spell.Shapes.Length > 0)
            {
                string name = spell.Shapes[System.Math.Clamp(shape, 0, spell.Shapes.Length - 1)];
                Pane.Add(PaneSection.Facts, spell.HasShapes ? "Shape: " + name + "   {Shape} next shape" : "Shape: " + name);
            }
            if (!string.IsNullOrEmpty(spell.Details) && spell.Details != spell.Line) Pane.Add(PaneSection.Foot, spell.Details, PaneTone.Meta);
        }

        private void DescribeSlot(int slot)
        {
            var spell = slot == 4 ? Board.Surge : Board.Slots[slot];
            string where = slot == 4 ? "The surge" : LoadoutWords.SlotWords[slot] + " slot";
            if (!spell.Open)
            {
                Pane.Icon = IconShapes.Lock;
                Pane.Title = "Shut";
                Pane.Kind = where;
                Pane.Add(PaneSection.Body, spell.ShutDetail.Length > 0 ? spell.ShutDetail : spell.ShutReason);
                return;
            }
            if (spell.IsEmpty)
            {
                Pane.Title = "Empty";
                Pane.Kind = where;
                Pane.Add(PaneSection.Body, slot == 4 ? "No surge is worn." : "Nothing is here yet.");
            }
            else DescribeSpell(spell, spell.Shape);
            Pane.Action = "Submit";
            Pane.ActionText = slot == 4 ? "Choose the surge" : "Choose a spell here";
        }

        private void DescribeRow()
        {
            var row = FocusedRow;
            if (row == null) return;
            var worn = ListSlot == 4 ? Board.Surge : Board.Slots[ListSlot];
            if (row.IsEmpty)
            {
                Pane.Title = row.Name;
                Pane.Kind = LoadoutWords.SlotWords[ListSlot] + " slot";
                Pane.Add(PaneSection.Body, row.Line);
            }
            else DescribeSpell(row, ShapeOf(row));

            if (row.Id == worn.Id) Pane.Add(PaneSection.Compare, worn.IsEmpty ? "This slot is empty now." : "Worn here now.");
            else
            {
                string wornWords = worn.IsEmpty ? "nothing" : worn.Name;
                if (!worn.IsEmpty && worn.Role != SpellRole.None) wornWords += ", " + LoadoutWords.RoleName(worn.Role);
                if (!worn.IsEmpty && worn.CostShort.Length > 0) wornWords += ", " + worn.CostShort;
                Pane.Add(PaneSection.Compare, "Worn here now: " + wornWords);
                if (ListSlot < 4 && row.InSlot >= 0 && row.InSlot != ListSlot)
                {
                    string from = LoadoutWords.SlotWords[row.InSlot];
                    Pane.Add(PaneSection.Compare, "It moves here from " + from + ". " + from + " gets " + (worn.IsEmpty ? "nothing" : worn.Name) + ".");
                }
                foreach (var role in LoadoutWords.Roles)
                    if (Roles[(int)role] > 0 && RolesPreview[(int)role] == 0)
                        Pane.Add(PaneSection.Compare, "You will have no " + LoadoutWords.RoleNoun(role) + ".", PaneTone.Loss);
            }
            Pane.Action = "Submit";
            Pane.ActionText = row.Id == worn.Id ? "Keep it here" : row.IsEmpty ? "Leave it empty" : "Wear it";
        }

        // ---- The Way.

        private void DescribeAspect()
        {
            var aspect = Board.Aspect;
            string weapon = Board.Weapon.Length > 0 ? Board.Weapon : "weapon";
            Pane.Kind = "Aspect of the " + weapon;
            Pane.Action = "Submit";
            Pane.ActionText = "See the aspects";
            if (string.IsNullOrEmpty(aspect.Id))
            {
                Pane.Icon = IconShapes.Blade;
                Pane.Title = "No aspect";
                Pane.Add(PaneSection.Body, "Buy an aspect on the Way chart. It changes how the " + weapon + " fights.");
                return;
            }
            Pane.Icon = aspect.Icon;
            Pane.Title = aspect.Name;
            if (aspect.MarksTotal > 0) Pane.Price = aspect.MarksBought + " of " + aspect.MarksTotal + " marks bought";
            Pane.Add(PaneSection.Body, aspect.Words);
            Pane.Add(PaneSection.Compare, "Set now.");
        }

        private void DescribeAspectCard(WayCard card)
        {
            if (card == null) return;
            var active = Board.Aspect;
            Pane.Icon = card.Icon;
            Pane.Title = card.Name;
            Pane.Kind = card.Kind;
            if (card.Lock.Length > 0) Pane.Add(PaneSection.Body, card.Lock);
            Pane.Add(PaneSection.Body, card.Words);
            if (card.MarksTotal > 0) Pane.Add(PaneSection.Facts, card.MarksBought + " of " + card.MarksTotal + " marks bought");
            if (card.Set)
            {
                Pane.Add(PaneSection.Compare, "Set now.");
                return;
            }
            if (!card.Bought) return;
            Pane.Add(PaneSection.Compare, string.IsNullOrEmpty(active.Id) ? "No aspect is set now." : "Set now: " + active.Name + ".");
            string change = LoadoutWords.For(card.Id).Change;
            Pane.Add(PaneSection.Compare, change);
            if (!string.IsNullOrEmpty(active.Id) && active.MarksBought > 0)
                Pane.Add(PaneSection.Compare, active.Name + "'s " + active.MarksBought + (active.MarksBought == 1 ? " mark goes" : " marks go") + " to sleep.", PaneTone.Loss, "You lose:");
            string keystone = "";
            foreach (var each in card.Keystones)
                if (each != null && each.Set) keystone = each.Name;
            Pane.Add(PaneSection.Compare, "Keystone: " + (keystone.Length > 0 ? keystone : "none set yet") + ".");
            Pane.Action = "Submit";
            Pane.ActionText = "Make this the aspect";
        }

        private void DescribePair()
        {
            bool bearing = Focus == LoadoutPlace.Bearing1 || Focus == LoadoutPlace.Bearing2;
            int cell = LoadoutPlaces.PairCellOf(Focus);
            var pair = bearing ? Board.Bearings : Board.Keystones;
            var mark = pair[cell];
            var other = pair[1 - cell];
            string noun = bearing ? "bearing" : "keystone";
            if (string.IsNullOrEmpty(mark.Id))
            {
                Pane.Icon = IconShapes.Lock;
                Pane.Title = bearing ? "No bearing" : "No keystone";
                Pane.Kind = bearing ? "Bearing of the Body" : "Keystone";
                Pane.Add(PaneSection.Body, bearing ? "Buy the Body's bearings on the Way chart."
                    : string.IsNullOrEmpty(Board.Aspect.Id) ? "Choose an aspect first. Each aspect has two keystones." : "This aspect has no keystone here.");
                return;
            }
            Pane.Icon = mark.Icon;
            Pane.Title = mark.Name;
            Pane.Kind = mark.Kind;
            if (mark.Bought && mark.Price > 0) Pane.Price = "Bought for " + LoadoutWords.Memories(mark.Price);
            if (mark.Lock.Length > 0) Pane.Add(PaneSection.Body, mark.Lock);
            Pane.Add(PaneSection.Body, mark.Words);
            Pane.Add(PaneSection.Facts, mark.Cost);
            Pane.Add(PaneSection.Facts, mark.Press);
            if (!mark.Bought) return;
            if (mark.Set)
            {
                Pane.Add(PaneSection.Compare, "Set now.");
                return;
            }
            if (other.Set)
            {
                Pane.Add(PaneSection.Compare, "Set now: " + other.Name + ".");
                Pane.Add(PaneSection.Compare, "Setting this one turns that one off.");
                if (other.Gives.Length > 0) Pane.Add(PaneSection.Compare, other.Gives + ".", PaneTone.Loss, "You lose:");
            }
            else Pane.Add(PaneSection.Compare, "Neither " + noun + " is set yet.");
            Pane.Action = "Submit";
            Pane.ActionText = "Set this " + noun;
        }

        private void DescribeTemper()
        {
            Pane.Icon = IconShapes.Anvil;
            Pane.Title = LoadoutWords.TemperName(Board.Temper);
            Pane.Kind = "Trunk of the " + (Board.Weapon.Length > 0 ? Board.Weapon : "weapon") + "'s Way";
            Pane.Add(PaneSection.Body, "Each rank makes your weapon hit 6% harder. It also fills the stagger bar 5% faster.");
            Pane.Add(PaneSection.Facts, "Rank " + Board.Temper + " of " + LoadoutBoard.TemperMost);
            if (Board.Temper < LoadoutBoard.TemperMost && Board.TemperNextPrice > 0)
                Pane.Add(PaneSection.Compare, "Buy the next rank on the Way chart. " + LoadoutWords.Memories(Board.TemperNextPrice) + ".");
        }

        private void DescribeTechnique()
        {
            var technique = Board.Technique;
            if (string.IsNullOrEmpty(technique.Id))
            {
                Pane.Icon = IconShapes.Lock;
                Pane.Title = "No technique";
                Pane.Kind = "Technique";
                Pane.Add(PaneSection.Body, "This weapon has no technique yet.");
                return;
            }
            Pane.Icon = technique.Icon;
            Pane.Title = technique.Name;
            Pane.Kind = "Technique";
            if (technique.Lock.Length > 0) Pane.Add(PaneSection.Body, technique.Lock);
            Pane.Add(PaneSection.Body, technique.Words);
            Pane.Add(PaneSection.Facts, technique.Press);
        }

        private void DescribeInfused()
        {
            Pane.Icon = IconShapes.ForElement(Board.StrikeFirst);
            Pane.IconElement = Board.StrikeFirst;
            Pane.Title = "Infused strikes";
            Pane.Kind = "Your steel and your element";
            if (Board.StrikeFirst.Length == 0)
            {
                Pane.Add(PaneSection.Body, "Carry an element to lay it with your blade.");
                return;
            }
            Pane.Add(PaneSection.Body, "Hold {SpellLayer} and attack. The blow lays your element.");
            Pane.Add(PaneSection.Facts, "{SpellLayer} + {LightAttack}: " + Board.StrikeFirst + " strike");
            if (Board.StrikeSecond.Length > 0)
                Pane.Add(PaneSection.Facts, "{SpellLayer} + {HeavyAttack}: " + (Board.StrikeSecondDeep ? "Deep " : "") + Board.StrikeSecond + " strike");
        }

        private void DescribeElements()
        {
            Pane.Icon = IconShapes.ForElement(Board.First);
            Pane.IconElement = Board.First;
            Pane.Title = Board.ElementWords;
            Pane.Kind = "Carried from the hub";
            Pane.Add(PaneSection.Body, "You chose these in the hub. They decide which spells you can wear.");
            if (Board.Devotion.Length > 0) Pane.Add(PaneSection.Facts, "Devotion is on: " + Board.Devotion + ".");
            Pane.Action = "Submit";
            Pane.ActionText = "See the reactions";
        }

        private void DescribeRoles()
        {
            Pane.Icon = IconShapes.RoleOpener;
            Pane.Title = "Roles";
            Pane.Kind = "Your Hand at a glance";
            if (MissingRoles.Length > 0) Pane.Add(PaneSection.Body, MissingRoles + ".", PaneTone.Loss);
            foreach (var role in LoadoutWords.Roles)
                Pane.Add(PaneSection.Facts, LoadoutWords.RoleMeaning(role), PaneTone.Plain, LoadoutWords.RoleName(role) + " " + RolesPreview[(int)role] + ":");
        }

        private void DescribeHand(int hand)
        {
            var card = Board.Hands[hand];
            Pane.Icon = IconShapes.Hand;
            Pane.Title = card.Name;
            Pane.Kind = hand == 0 ? "A Hand made from what you know" : card.Empty ? "Nothing kept here yet" : "A Hand you kept";
            if (!card.Empty)
            {
                for (int i = 0; i < card.Slots.Length; i++)
                {
                    var spell = card.Slots[i];
                    var worn = Board.Slots[i];
                    string line = LoadoutWords.SlotWords[i] + ": " + (spell.IsEmpty ? "Empty" : spell.Name);
                    bool differs = spell.Id != worn.Id;
                    if (differs) line += " (now " + (worn.IsEmpty ? "empty" : worn.Name) + ")";
                    Pane.Add(PaneSection.Facts, line, differs ? PaneTone.Plain : PaneTone.Meta);
                }
                bool surgeDiffers = card.Surge.Id != Board.Surge.Id;
                Pane.Add(PaneSection.Facts, "Surge: " + (card.Surge.IsEmpty ? "none" : card.Surge.Name) + (surgeDiffers ? " (now " + (Board.Surge.IsEmpty ? "none" : Board.Surge.Name) + ")" : ""),
                    surgeDiffers ? PaneTone.Plain : PaneTone.Meta);
                if (card.Worn) Pane.Add(PaneSection.Compare, "Worn now.");
                Pane.Action = "Submit";
                Pane.ActionText = "Wear it";
                if (hand > 0) Pane.Add(PaneSection.Foot, "Hold {Keep} to keep the Hand you wear here. It replaces this one.");
                return;
            }
            Pane.Add(PaneSection.Body, "Keep the Hand you wear here, to wear it again later.");
            Pane.Action = "Keep";
            Pane.ActionText = "Keep the Hand you wear here";
        }

        // The first keyword the pane's words use gets its meaning at the foot (4.4).
        private void AddKeyword()
        {
            foreach (var line in Pane.Lines)
            {
                if (line.Section != PaneSection.Body) continue;
                if (!LoadoutWords.FindKeyword(line.Text, out string keyword, out string meaning)) continue;
                Pane.Add(PaneSection.Foot, meaning, PaneTone.Keyword, keyword + ":");
                return;
            }
        }
    }
}

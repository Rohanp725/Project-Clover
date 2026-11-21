using System.Text;
using Clover.Gameplay;
using Clover.Player;

namespace Clover.UI
{
    /// <summary>
    /// The details view of a mark on the chart: what it does, when, the numbers, what it works with and an example.
    /// Marks with written details (docs/design/playtest/marks/*.md) show those; every other high-level mark gets an
    /// account read from its own data (kind, price, trigger, the numbers of its stats, effects and spell), until its
    /// words are written.
    /// </summary>
    public static class MarkDetails
    {
        /// <summary>True for the marks that have a details view: everything but the plain minors.</summary>
        public static bool HasDetails(Upgrade mark) => mark != null && (mark.Kind != UpgradeKind.Minor || NotBuiltYet.IsSlot(mark));

        public static PlaytestText For(Upgrade mark, PlaytestTexts texts)
        {
            if (mark == null) return null;
            var entry = new PlaytestText { Id = mark.Id, Title = mark.Name };
            var words = new StringBuilder(1024);
            words.Append("<color=#E8C77A>").Append(Heading(mark)).Append("</color>\n\n");
            if (NotBuiltYet.IsSlot(mark))
            {
                words.Append(mark.Description);
                entry.Body = words.ToString();
                return entry;
            }
            var written = texts != null ? texts.MarkDetails(mark.Id) : null;
            if (written != null)
            {
                words.Append(written.Body);
                entry.Body = words.ToString();
                return entry;
            }
            Auto(mark, words);
            entry.Body = words.ToString();
            return entry;
        }

        /// <summary>The line over the details: its kind, its tree and its price.</summary>
        public static string Heading(Upgrade mark)
        {
            if (NotBuiltYet.IsSlot(mark)) return $"{NotBuiltYet.Label}  ·  beside the {mark.Element} tree";
            if (mark.Kind == UpgradeKind.Reaction) return $"Reaction  ·  {mark.Element} and {mark.Element2}  ·  always in force, never sold";
            if (mark.IsWay) return $"{KindName(mark)}  ·  {WayPlace(mark)}  ·  {WayPrice(mark)}";
            string tree = mark.IsMix ? $"{HeartLayout.BranchName(mark.Element, mark.Element2)} ({mark.Element} and {mark.Element2})" : mark.Element + " tree";
            string price = mark.Signature ? "a god's gift, never sold (a playtest unlock here)"
                : mark.Currency == Currency.None ? "comes with " + mark.Element + ", never sold"
                : mark.Cost == 1 ? "1 Shard" : mark.Cost + " Shards";
            return $"{KindName(mark)}  ·  {tree}  ·  {price}";
        }

        // Where on the Way a mark sits: the Body, a weapon's trunk, or one of its boughs.
        private static string WayPlace(Upgrade mark)
        {
            if (mark.Tree == MarkTree.Body) return "the Body";
            string way = $"the {mark.Weapon}'s Way";
            if (string.IsNullOrEmpty(mark.Aspect) || mark.Aspect == mark.Id) return mark.Kind == UpgradeKind.Aspect ? way : way + ", the trunk";
            var bough = Upgrade.ById(mark.Aspect);
            return bough != null ? $"{way}, {bough.Name}" : way;
        }

        // The Way's prices are in Memories; a god's bough comes with taking the god.
        private static string WayPrice(Upgrade mark)
        {
            if (mark.Currency == Currency.None) return "given when the god is taken, never sold (a playtest unlock here)";
            string price = mark.Cost == 1 ? "1 Memory" : mark.Cost + " Memories";
            if (string.IsNullOrEmpty(mark.Gate)) return price;
            if (mark.Gate.StartsWith(MarkRules.GodsGate, System.StringComparison.Ordinal))
                return $"{price}, opens once {mark.Gate.Substring(MarkRules.GodsGate.Length)} gods have fallen";
            return price + ", sealed until its teacher falls";
        }

        public static string KindName(Upgrade mark)
        {
            if (mark.Signature) return mark.IsMix ? "Mix crown" : "God's gift";
            if (mark.IsWay)
                switch (mark.Kind)
                {
                    case UpgradeKind.Aspect: return mark.Currency == Currency.None ? "God's bough" : "Aspect";
                    case UpgradeKind.Temper: return "Temper";
                    case UpgradeKind.Technique: return "Technique";
                    case UpgradeKind.Keystone: return mark.Tree == MarkTree.Body ? "Bearing" : "Keystone";
                    case UpgradeKind.Minor: return "Number";
                    case UpgradeKind.Major: return "Move or rule";
                }
            // A duo branch's Shape mark shapes its pair's reaction (task 4.5).
            if (ReactionTable.IsReactionShape(mark)) return "Reaction shape";
            if (mark.IsMix) return mark.IsSpell ? "Duo spell" : "Mix";
            // A spell branch's marks say their place in it (Phase 4).
            switch (mark.Part)
            {
                case BranchPart.Learn: return "Spell";
                case BranchPart.Empower: return "Empower";
                case BranchPart.Shape: return "Shape";
                case BranchPart.Master: return "Master";
            }
            if (!string.IsNullOrEmpty(mark.Lifts)) return "Reaction lift";
            if (mark.Kind == UpgradeKind.Devotion) return "Devotion (all in)";
            switch (mark.Kind)
            {
                case UpgradeKind.Root: return "Root";
                case UpgradeKind.Minor: return "Minor";
                case UpgradeKind.Major: return "Major";
                case UpgradeKind.Spell: return "Spell";
                case UpgradeKind.Keystone: return "Keystone";
                default: return "Mark";
            }
        }

        // The account read from the mark's own data, in the same five parts as the written ones.
        private static void Auto(Upgrade mark, StringBuilder words)
        {
            words.Append("<b>What it does.</b> ").Append(mark.Description);
            if (!string.IsNullOrEmpty(mark.Weapon)) words.Append(" Needs the ").Append(mark.Weapon).Append('.');
            if (mark.Signature) words.Append(' ').Append(NotBuiltYet.GiftSource(mark.Element));
            words.Append("\n\n<b>When it happens.</b> ").Append(TriggerWords.HowTo(mark));
            words.Append("\n\n<b>The numbers.</b>");
            int before = words.Length;
            if (mark.WeaponStacks > 0) words.Append("\n• Every weapon blow lays ").Append(mark.WeaponStacks).Append(" stack of ").Append(AfflictionName(Elements.ByNameOrFire(mark.Element).Affliction)).Append('.');
            foreach (var stat in mark.Stats) words.Append("\n• ").Append(StatWords(stat));
            foreach (var effect in mark.Effects) words.Append("\n• ").Append(EffectWords(effect));
            if (mark.Spell != null) words.Append("\n• ").Append(SpellWords(mark.Spell));
            if (mark.Tally != null) words.Append("\n• It keeps a count, shown on the HUD.");
            if (words.Length == before) words.Append(" None beyond its words: it is written by hand in the code.");
            words.Append("\n\n<b>Grows from.</b> ").Append(Parents(mark));
            words.Append("\n\n<i>Read from the mark's own data. Written details for this tree, with what it works with and an example, come before release.</i>");
        }

        private static string Parents(Upgrade mark)
        {
            if (mark.Signature) return "the crown of its tree; it is given, not grown.";
            if (mark.Requires == null || mark.Requires.Length == 0) return "the foot of its tree.";
            var names = new StringBuilder();
            foreach (string id in mark.Requires)
            {
                var parent = Upgrade.ById(id);
                if (parent == null) continue;
                if (names.Length > 0) names.Append(" or ");
                names.Append(parent.Name);
            }
            return names.Append('.').ToString();
        }

        private static string AfflictionName(AfflictionKind kind) => kind == AfflictionKind.None ? "nothing" : Afflictions.Spec(kind).Name;

        /// <summary>A stat change in words: "+10% weapon damage", "+1 affliction stacks (flat)", "while the surge lasts".</summary>
        public static string StatWords(StatMod stat)
        {
            string name = Spaced(stat.Stat.ToString());
            string amount = PlayerStats.IsFlat(stat.Stat) ? $"{Signed(stat.Amount)} {name}" : $"{Signed(stat.Amount * 100f)}% {name}";
            if (stat.When != StatWhen.Always) amount += " (" + Spaced(stat.When.ToString()) + ")";
            if (stat.Against != AfflictionKind.None) amount += " against what carries " + AfflictionName(stat.Against);
            return amount + ".";
        }

        private static string EffectWords(UpgradeEffect effect)
        {
            var words = new StringBuilder(96);
            words.Append("When ").Append(TriggerWords.Of(effect.On));
            if (effect.WhileSurge) words.Append(", during the surge");
            if (effect.Needs != AfflictionKind.None) words.Append(", on what carries ").Append(AfflictionName(effect.Needs)).Append(effect.NeedsFull ? " at full depth" : "");
            words.Append(": ").Append(Spaced(effect.Do.ToString()).ToLowerInvariant());
            if (effect.Damage > 0f) words.Append(", ").Append(effect.Damage.ToString("0.##")).Append(" damage");
            if (effect.Stagger > 0f) words.Append(", ").Append(effect.Stagger.ToString("0.##")).Append(" stagger");
            if (effect.Affliction != AfflictionKind.None && effect.Stacks > 0) words.Append(", ").Append(effect.Stacks).Append(' ').Append(AfflictionName(effect.Affliction));
            if (effect.Radius > 0f) words.Append(", ").Append(effect.Radius.ToString("0.##")).Append(" m round");
            if (effect.Reach > 0f) words.Append(", ").Append(effect.Reach.ToString("0.##")).Append(" m reach");
            if (effect.Seconds > 0f) words.Append(", for ").Append(effect.Seconds.ToString("0.##")).Append(" s");
            if (effect.Amount != 0f) words.Append(", amount ").Append(effect.Amount.ToString("0.##"));
            if (effect.Chance < 1f) words.Append(", ").Append((effect.Chance * 100f).ToString("0")).Append("% of the time");
            if (effect.Cooldown > 0f) words.Append(", at most once in ").Append(effect.Cooldown.ToString("0.##")).Append(" s");
            return words.Append('.').ToString();
        }

        private static string SpellWords(SpellSpec spell)
        {
            var words = new StringBuilder(96);
            words.Append("The spell: ").Append(spell.Cost.ToString("0")).Append(" mana");
            if (spell.Damage > 0f) words.Append(", ").Append(spell.Damage.ToString("0.##")).Append(" damage");
            if (spell.Stagger > 0f) words.Append(", ").Append(spell.Stagger.ToString("0.##")).Append(" stagger");
            if (spell.Reach > 0f) words.Append(", ").Append(spell.Reach.ToString("0.##")).Append(" m reach");
            if (spell.Radius > 0f) words.Append(", ").Append(spell.Radius.ToString("0.##")).Append(" m round");
            if (spell.Stacks > 0) words.Append(", ").Append(spell.Stacks).Append(spell.Stacks == 1 ? " stack" : " stacks").Append(" of the element's affliction");
            return words.Append('.').ToString();
        }

        private static string Signed(float value) => (value >= 0f ? "+" : "") + value.ToString("0.##");

        // "WeaponDamage" reads as "weapon damage".
        private static string Spaced(string name)
        {
            var spaced = new StringBuilder(name.Length + 8);
            for (int i = 0; i < name.Length; i++)
            {
                char letter = name[i];
                if (i > 0 && char.IsUpper(letter)) spaced.Append(' ');
                spaced.Append(i > 0 ? char.ToLowerInvariant(letter) : letter);
            }
            return spaced.ToString().ToLowerInvariant();
        }
    }
}

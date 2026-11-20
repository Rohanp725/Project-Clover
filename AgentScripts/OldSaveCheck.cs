using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Clover.Core.Save;
using Clover.Player;
using UnityEngine;

// The Phase 4 merge's "load a save from before the merge": every mark id main's trees had before the merge (read from
// git into a comma list, the first argument's file) is learned in one save written as the old build wrote it, with Shards
// in the wallet and retired spells in the Hand; the save is read back through the real SaveStore (files in memory, no
// PlayerPrefs) and what it holds after is written: Shards refunded against the retired prices, marks kept, marks gone,
// and any old id that is neither a mark now nor retired (an orphan a save would carry for nothing).
// unity command script run -- --file AgentScripts/OldSaveCheck.cs --entry OldSaveCheck.Run --args '["<path to ids>"]'
public static class OldSaveCheck
{
    private sealed class MemoryFiles : ISaveFiles
    {
        public readonly Dictionary<string, string> Files = new Dictionary<string, string>();
        public bool Exists(string name) => Files.ContainsKey(name);
        public string Read(string name) => Files[name];
        public void Write(string name, string text) => Files[name] = text;
        public void Swap(string from, string into)
        {
            Files[into] = Files[from];
            Files.Remove(from);
        }
    }

    private sealed class NoPrefs : IPrefsSource
    {
        public bool HasKey(string key) => false;
        public int GetInt(string key, int fallback) => fallback;
    }

    public static string Run(string idsPath)
    {
        var old = File.ReadAllText(idsPath).Split(',').Select(id => id.Trim()).Where(id => id.Length > 0).ToList();
        var now = new HashSet<string>(Upgrade.All.Select(mark => mark.Id));
        var retired = RetiredMarks.All.ToDictionary(mark => mark.Id, mark => mark.Price);
        var moving = new HashSet<string>(RetiredMarks.Moving.Select(mark => mark.Id));
        var marks = old.Where(id => now.Contains(id) || retired.ContainsKey(id) || moving.Contains(id)).ToList();
        var neither = old.Where(id => !now.Contains(id) && !retired.ContainsKey(id) && !moving.Contains(id)).ToList();

        var data = new SaveData();
        data.Learned.AddRange(marks);
        data.Wallet.Shards = 7;
        // Retired spells in the Hand carried and a kept Hand, beside one still in the trees.
        string keptSpell = marks.FirstOrDefault(id => now.Contains(id) && Upgrade.ById(id).IsSpell) ?? "";
        data.Current.Slots[0] = keptSpell;
        data.Current.Slots[1] = "plasma.lance";
        data.Current.Slots[2] = "tempest.storm";
        data.SavedHands[0].Slots = new[] { "pyre.fireeater", keptSpell, "", "" };

        var files = new MemoryFiles();
        files.Files[SaveStore.GameFile] = JsonUtility.ToJson(data, true);
        var store = new SaveStore(files, new NoPrefs(), SaveMigration.KnownFlags());
        var loaded = store.Get(false, false);

        int expectedRefund = marks.Where(retired.ContainsKey).Sum(id => retired[id]);
        var kept = loaded.Learned.ToList();
        var left = kept.Where(retired.ContainsKey).ToList();
        var lost = marks.Where(id => !retired.ContainsKey(id) && !kept.Contains(id)).ToList();
        var said = new StringBuilder();
        said.Append($"old ids read {old.Count}: marks {marks.Count} (in the trees now {marks.Count(now.Contains)}, retired {marks.Count(retired.ContainsKey)}, moving to the Way {marks.Count(moving.Contains)}), neither {neither.Count}\n");
        said.Append($"Shards 7 -> {loaded.Wallet.Shards} (expected {7 + expectedRefund}: refund {expectedRefund}) {(loaded.Wallet.Shards == 7 + expectedRefund ? "OK" : "WRONG")}\n");
        said.Append($"learned {marks.Count} -> {kept.Count}; retired left {left.Count} {(left.Count == 0 ? "OK" : "WRONG: " + string.Join(",", left))}; live marks lost {lost.Count} {(lost.Count == 0 ? "OK" : "WRONG: " + string.Join(",", lost))}\n");
        said.Append($"Hand carried [{string.Join("|", loaded.Current.Slots)}] (plasma.lance and tempest.storm retired: empty expected); kept Hand 1 [{string.Join("|", loaded.SavedHands[0].Slots)}] (pyre.fireeater retired)\n");
        said.Append($"retired ids that were marks before the merge: {string.Join(",", marks.Where(retired.ContainsKey).Where(id => id.StartsWith("plasma") || id.StartsWith("tempest") || id.StartsWith("blizzard") || id.StartsWith("glacier") || id.StartsWith("bloom") || id.StartsWith("overgrowth") || id.StartsWith("forge") || id.StartsWith("halo") || id.StartsWith("eclipse") || id.StartsWith("blight") || id.StartsWith("pyre")))}\n");
        said.Append($"neither (not marks now, not retired; check none was a mark): {string.Join(",", neither)}");
        return said.ToString();
    }
}

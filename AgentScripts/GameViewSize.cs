using System;
using System.Reflection;
using UnityEditor;
using UnityEngine;

// Sets the Game View to the fixed size written in AgentScripts/gameview_size.txt ("1024x768"), adding it to the
// editor's list of sizes under the name "probe 1024x768" when it is not there, so a capture is of the game really
// drawn at that shape and not of a stretched picture. "free" puts it back to Free Aspect (the first entry).
public static class GameViewSize
{
    public static string Run()
    {
        string asked = System.IO.File.ReadAllText("AgentScripts/gameview_size.txt").Trim();
        var editor = typeof(Editor).Assembly;
        var sizesType = editor.GetType("UnityEditor.GameViewSizes");
        var instance = typeof(ScriptableSingleton<>).MakeGenericType(sizesType).GetProperty("instance").GetValue(null);
        var group = sizesType.GetProperty("currentGroup").GetValue(instance);
        var groupType = group.GetType();
        var viewType = editor.GetType("UnityEditor.GameView");
        var view = EditorWindow.GetWindow(viewType, false, null, false);
        var selected = viewType.GetProperty("selectedSizeIndex", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
        if (asked == "free")
        {
            selected.SetValue(view, 0);
            view.Repaint();
            return "free";
        }
        string[] parts = asked.Split('x');
        int width = int.Parse(parts[0]), height = int.Parse(parts[1]);
        string name = "probe " + asked;
        var texts = (string[])groupType.GetMethod("GetDisplayTexts").Invoke(group, null);
        int index = Array.FindIndex(texts, text => text.StartsWith(name));
        if (index < 0)
        {
            var sizeType = editor.GetType("UnityEditor.GameViewSize");
            var kind = editor.GetType("UnityEditor.GameViewSizeType");
            var size = sizeType.GetConstructor(new[] { kind, typeof(int), typeof(int), typeof(string) })
                .Invoke(new object[] { Enum.ToObject(kind, 1), width, height, name });
            groupType.GetMethod("AddCustomSize").Invoke(group, new[] { size });
            texts = (string[])groupType.GetMethod("GetDisplayTexts").Invoke(group, null);
            index = Array.FindIndex(texts, text => text.StartsWith(name));
        }
        selected.SetValue(view, index);
        view.Repaint();
        return name + " at " + index;
    }
}

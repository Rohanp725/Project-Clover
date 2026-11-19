using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// The look-dev's recipe for the 24 designed reactions (Art/VFX/Combos/combos_recipe.json, written in Blender on
    /// 2026-11-19; its "conventions" block explains each field). For each combo: its pieces, grouped by kit object and
    /// material, each placed in Unity's axes about the main body's feet and marked as lying on the ground or held on
    /// the body; its point lamps in Blender watts; how many bodies stood in it. Every combo is one held frame. Read by
    /// <see cref="ReactionArtBuilder"/>; JsonUtility cannot read it (an instance is a list of mixed values).
    /// </summary>
    public class ReactionRecipe
    {
        public sealed class Combo
        {
            public string Name = "";
            /// <summary>"fire_ice": the two elements as the kits name them.</summary>
            public string Pair = "";
            public readonly List<Group> Groups = new List<Group>();
            public readonly List<Lamp> Lamps = new List<Lamp>();
            public int Bodies;

            public int PieceCount
            {
                get
                {
                    int count = 0;
                    foreach (var group in Groups) count += group.Instances.Count;
                    return count;
                }
            }
        }

        public sealed class Group
        {
            /// <summary>The Blender mesh, its kit file and the object in it, the material it wears here, and its pass.</summary>
            public string Mesh = "", Kit = "", KitObject = "", Material = "", Pass = "";
            public readonly List<Instance> Instances = new List<Instance>();
        }

        public sealed class Instance
        {
            /// <summary>The Blender object's name, or only its suffix (see <see cref="FullName"/>).</summary>
            public string Object = "";
            public Vector3 Position;
            /// <summary>Unity Euler angles in degrees, as Quaternion.Euler takes them.</summary>
            public Vector3 Rotation;
            public Vector3 Scale = Vector3.one;
            /// <summary>True for a piece lying on the ground (no higher than 0.15 m), false for one on or round the body.</summary>
            public bool OnGround;

            /// <summary>Where the piece's own space lies in the combo's: the recipe's position, rotation and scale.</summary>
            public Matrix4x4 Matrix => Matrix4x4.TRS(Position, Quaternion.Euler(Rotation), Scale);
        }

        public sealed class Lamp
        {
            public string Object = "";
            public Color Colour = Color.white;
            public float Watts;
            /// <summary>The lamp's own cut-off in metres, or -1 when Blender used none.</summary>
            public float Range = -1f;
            public Vector3 Position;
            public bool FirstPass;
        }

        public readonly List<string> Kits = new List<string>();
        public readonly List<Combo> Combos = new List<Combo>();

        /// <summary>
        /// An instance's whole Blender name: as given, unless it is empty or starts with '.', when it is the group's mesh
        /// (less any ".001") + '~' + the combo's pair + the suffix: "vfx_fire_ember~fire_ice.013".
        /// </summary>
        public static string FullName(Combo combo, Group group, Instance instance)
        {
            if (!string.IsNullOrEmpty(instance.Object) && instance.Object[0] != '.') return instance.Object;
            string mesh = group.Mesh;
            int dot = mesh.LastIndexOf('.');
            if (dot > 0 && int.TryParse(mesh.Substring(dot + 1), out _)) mesh = mesh.Substring(0, dot);
            return mesh + "~" + combo.Pair + instance.Object;
        }

        /// <summary>Reads the recipe. Throws <see cref="FormatException"/> on text that is not its shape.</summary>
        public static ReactionRecipe Parse(string json)
        {
            var root = Json.Parse(json) as Dictionary<string, object>;
            if (root == null) throw new FormatException("The recipe is not a JSON object.");
            var recipe = new ReactionRecipe();
            if (root.TryGetValue("kits", out var kits) && kits is List<object> kitList)
                foreach (var kit in kitList) recipe.Kits.Add(kit as string ?? "");
            if (!(root.TryGetValue("combos", out var combos) && combos is List<object> comboList))
                throw new FormatException("The recipe has no \"combos\" list.");
            foreach (var entry in comboList)
            {
                if (!(entry is Dictionary<string, object> source)) continue;
                var combo = new Combo { Name = Text(source, "name"), Pair = Text(source, "pair") };
                if (source.TryGetValue("bodies", out var bodies) && bodies is List<object> bodyList) combo.Bodies = bodyList.Count;
                if (source.TryGetValue("lights", out var lights) && lights is List<object> lightList)
                    foreach (var light in lightList)
                    {
                        if (!(light is Dictionary<string, object> lamp)) continue;
                        combo.Lamps.Add(new Lamp
                        {
                            Object = Text(lamp, "object"),
                            Colour = ColourOf(lamp.TryGetValue("colour_linear", out var colour) ? colour : null),
                            Watts = Number(lamp, "intensity_blender_W", 0f),
                            Range = Number(lamp, "range_m", -1f),
                            Position = Vector(lamp.TryGetValue("position", out var at) ? at : null, Vector3.zero),
                            FirstPass = lamp.TryGetValue("first_pass", out var first) && first is bool yes && yes,
                        });
                    }
                if (source.TryGetValue("groups", out var groups) && groups is List<object> groupList)
                    foreach (var item in groupList)
                    {
                        if (!(item is Dictionary<string, object> from)) continue;
                        var group = new Group
                        {
                            Mesh = Text(from, "mesh"), Kit = Text(from, "kit"), KitObject = Text(from, "kit_object"),
                            Material = Text(from, "material"), Pass = Text(from, "pass"),
                        };
                        if (from.TryGetValue("instances", out var instances) && instances is List<object> instanceList)
                            foreach (var one in instanceList)
                                if (one is List<object> fields) group.Instances.Add(InstanceOf(fields));
                        combo.Groups.Add(group);
                    }
                recipe.Combos.Add(combo);
            }
            return recipe;
        }

        // [object, position, rotation_euler_deg (0 or [x, y, z]), scale (a number or [x, y, z]), "g" or "b"].
        private static Instance InstanceOf(List<object> fields)
        {
            if (fields.Count < 5) throw new FormatException($"An instance has {fields.Count} fields, not 5.");
            return new Instance
            {
                Object = fields[0] as string ?? "",
                Position = Vector(fields[1], Vector3.zero),
                Rotation = Vector(fields[2], Vector3.zero),
                Scale = Vector(fields[3], Vector3.one),
                OnGround = (fields[4] as string) == "g",
            };
        }

        private static string Text(Dictionary<string, object> from, string key) =>
            from.TryGetValue(key, out var value) && value is string text ? text : "";

        private static float Number(Dictionary<string, object> from, string key, float fallback) =>
            from.TryGetValue(key, out var value) && value is double number ? (float)number : fallback;

        // A number is the same on every axis (a uniform scale, or the 0 that means no rotation); a list is x, y, z.
        private static Vector3 Vector(object value, Vector3 fallback)
        {
            if (value is double number) return new Vector3((float)number, (float)number, (float)number);
            if (value is List<object> list && list.Count >= 3 && list[0] is double x && list[1] is double y && list[2] is double z)
                return new Vector3((float)x, (float)y, (float)z);
            return fallback;
        }

        private static Color ColourOf(object value)
        {
            var rgb = Vector(value, Vector3.one);
            return new Color(rgb.x, rgb.y, rgb.z, 1f);
        }

        /// <summary>A small reader for plain JSON: objects, lists, strings, numbers, true, false and null.</summary>
        private sealed class Json
        {
            private readonly string text;
            private int at;

            // A byte-order mark is not white space to char.IsWhiteSpace: it is dropped first.
            private Json(string text) => this.text = (text ?? "").TrimStart('﻿');

            public static object Parse(string text)
            {
                var reader = new Json(text);
                var value = reader.Value();
                reader.Space();
                if (reader.at < reader.text.Length) throw new FormatException($"Unexpected text at {reader.at}.");
                return value;
            }

            private void Space()
            {
                while (at < text.Length && char.IsWhiteSpace(text[at])) at++;
            }

            private object Value()
            {
                Space();
                if (at >= text.Length) throw new FormatException("The text ends early.");
                char next = text[at];
                if (next == '{') return Object();
                if (next == '[') return List();
                if (next == '"') return String();
                if (Word("true")) return true;
                if (Word("false")) return false;
                if (Word("null")) return null;
                return Number();
            }

            private bool Word(string word)
            {
                if (string.CompareOrdinal(text, at, word, 0, word.Length) != 0) return false;
                at += word.Length;
                return true;
            }

            private Dictionary<string, object> Object()
            {
                var result = new Dictionary<string, object>();
                at++;
                Space();
                if (at < text.Length && text[at] == '}')
                {
                    at++;
                    return result;
                }
                while (true)
                {
                    Space();
                    string key = String();
                    Space();
                    Expect(':');
                    result[key] = Value();
                    Space();
                    if (at < text.Length && text[at] == ',')
                    {
                        at++;
                        continue;
                    }
                    Expect('}');
                    return result;
                }
            }

            private List<object> List()
            {
                var result = new List<object>();
                at++;
                Space();
                if (at < text.Length && text[at] == ']')
                {
                    at++;
                    return result;
                }
                while (true)
                {
                    result.Add(Value());
                    Space();
                    if (at < text.Length && text[at] == ',')
                    {
                        at++;
                        continue;
                    }
                    Expect(']');
                    return result;
                }
            }

            private string String()
            {
                Expect('"');
                var built = new StringBuilder();
                while (at < text.Length && text[at] != '"')
                {
                    char c = text[at++];
                    if (c != '\\')
                    {
                        built.Append(c);
                        continue;
                    }
                    if (at >= text.Length) break;
                    char escaped = text[at++];
                    switch (escaped)
                    {
                        case 'n': built.Append('\n'); break;
                        case 't': built.Append('\t'); break;
                        case 'r': built.Append('\r'); break;
                        case 'b': built.Append('\b'); break;
                        case 'f': built.Append('\f'); break;
                        case 'u':
                            if (at + 4 > text.Length) throw new FormatException("A \\u escape ends early.");
                            built.Append((char)Convert.ToInt32(text.Substring(at, 4), 16));
                            at += 4;
                            break;
                        default: built.Append(escaped); break;
                    }
                }
                Expect('"');
                return built.ToString();
            }

            private double Number()
            {
                int start = at;
                while (at < text.Length && "+-0123456789.eE".IndexOf(text[at]) >= 0) at++;
                if (start == at) throw new FormatException($"Unexpected '{text[at]}' at {at}.");
                return double.Parse(text.Substring(start, at - start), NumberStyles.Float, CultureInfo.InvariantCulture);
            }

            private void Expect(char wanted)
            {
                if (at >= text.Length || text[at] != wanted) throw new FormatException($"Expected '{wanted}' at {at}.");
                at++;
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;

namespace Clover.UI.Toolkit
{
    /// <summary>
    /// The menus' icons, painted by code (ui_rest_fire_plan.md 4.3, the owner's Q6): flat, bold, symmetrical shapes, one
    /// colour on the ink, written as SVG path data in a small box and drawn by <see cref="PaintedIcon"/> with Painter2D,
    /// so they stay sharp at every size. A STAND-IN: every spell shows its element's icon and every Way mark a generic
    /// shape until each gets its own art (content_open.tsv, ui-icons). Kept as data so tests can check each one parses.
    /// </summary>
    public static class IconShapes
    {
        // The keys. Elements use the chart's own icon names (UpgradeIcons), so an element's icon is its Elements entry's.
        public const string Flame = "flame", Bolt = "bolt", Gust = "gust", Snowflake = "snowflake", Drop = "drop", Leaf = "leaf",
            Rock = "rock", Anvil = "anvil", Sun = "sun", Moon = "moon", Fang = "fang";
        public const string RoleOpener = "role-opener", RoleBreaker = "role-breaker", RoleDetonator = "role-detonator",
            RoleCounter = "role-counter", RoleMovement = "role-movement";
        public const string Lock = "lock", Mote = "mote", Shard = "shard", Hand = "hand";
        public const string Sheath = "sheath", Click = "click", Shrine = "shrine", Stone = "stone", Feather = "feather",
            Wave = "wave", Shield = "shield", Crown = "crown", Star = "star", Ring = "ring", Blade = "blade", Wing = "wing";

        /// <summary>One layer of an icon: a path filled or stroked, in the icon's colour, or in the ink as a cut.</summary>
        public readonly struct Layer
        {
            public readonly string Path;
            public readonly float Stroke;
            public readonly bool Cut;
            public readonly bool Round;

            public Layer(string path, float stroke = 0f, bool cut = false, bool round = true)
            {
                Path = path;
                Stroke = stroke;
                Cut = cut;
                Round = round;
            }
        }

        /// <summary>An icon: its box (the SVG viewBox, square) and its layers.</summary>
        public sealed class Shape
        {
            public readonly float Box;
            public readonly Layer[] Layers;

            public Shape(float box, params Layer[] layers)
            {
                Box = box;
                Layers = layers;
            }
        }

        private static Layer F(string path) => new Layer(path);
        private static Layer S(string path, float width, bool round = true) => new Layer(path, width, false, round);
        private static Layer CutF(string path) => new Layer(path, 0f, true);
        private static Layer CutS(string path, float width) => new Layer(path, width, true);

        private const string Shield32 = "M16 3l12 5v8c0 7-5 12-12 14C9 28 4 23 4 16V8z";

        private static readonly Dictionary<string, Shape> shapes = new Dictionary<string, Shape>
        {
            [Flame] = new Shape(64, F("M32 4c4 12 18 18 18 34a18 18 0 0 1-36 0c0-9 5-14 9-18 0 7 3 10 6 11-1-11 1-20 3-27z"),
                CutF("M32 34c3 5 8 7 8 13a8 8 0 0 1-16 0c0-5 4-8 8-13z")),
            [Drop] = new Shape(64, F("M32 4C25 18 12 30 12 42a20 20 0 0 0 40 0C52 30 39 18 32 4z"), CutS("M22 42a10 10 0 0 0 10 10", 5f)),
            [Bolt] = new Shape(64, F("M38 4L14 36h14l-6 24 28-34H35l9-22z")),
            [Gust] = new Shape(64, S("M8 22h30a8 8 0 1 0-8-8", 6f), S("M8 34h42a8 8 0 1 1-8 8", 6f), S("M8 46h22", 6f)),
            [Snowflake] = new Shape(64, S("M32 6v52", 6f), S("M9.5 19l45 26", 6f), S("M9.5 45l45-26", 6f), S("M24 10l8 8 8-8", 5f), S("M24 54l8-8 8 8", 5f)),
            [Leaf] = new Shape(64, F("M52 8C24 8 10 22 10 42c0 6 2 10 4 14 2-16 12-28 26-34-12 8-20 20-22 34 24 2 36-18 34-48z")),
            [Rock] = new Shape(64, F("M8 52l10-26 12 8 8-20 18 38z")),
            [Anvil] = new Shape(64, F("M6 18h40c0 8 6 12 14 12v4H46c-2 6-6 8-10 10v6h8v6H18v-6h8v-6c-6-2-10-6-12-12C8 32 6 26 6 18z")),
            [Sun] = new Shape(64, F("M20 32a12 12 0 1 0 24 0a12 12 0 1 0-24 0z"), S("M32 6v8M32 50v8M6 32h8M50 32h8", 5f),
                S("M13.6 13.6l5.7 5.7M44.7 44.7l5.7 5.7M13.6 50.4l5.7-5.7M44.7 19.3l5.7-5.7", 5f)),
            [Moon] = new Shape(64, F("M40 6a26 26 0 1 0 18 40A22 22 0 0 1 40 6z")),
            [Fang] = new Shape(64, F("M14 10h36c0 10-4 16-10 18l-8 30-8-30c-6-2-10-8-10-18z")),

            [RoleOpener] = new Shape(32, F("M4 13h14V6l11 10-11 10v-7H4z")),
            [RoleBreaker] = new Shape(32, F(Shield32), CutS("M17 6l-4 9h5l-4 11", 3f)),
            [RoleDetonator] = new Shape(32, F("M16 2l3.5 8.5L28 7l-4 8.5L31 20l-9 .5L20 30l-4-7-4 7-2-9.5L1 20l7-4.5L4 7l8.5 3.5z")),
            [RoleCounter] = new Shape(32, F(Shield32), CutS("M20 11l-7 5 7 5", 3.5f)),
            [RoleMovement] = new Shape(32, F("M3 7h7l8 9-8 9H3l8-9zM15 7h7l8 9-8 9h-7l8-9z")),

            [Lock] = new Shape(32, F("M6 17a3 3 0 0 1 3-3h14a3 3 0 0 1 3 3v9a3 3 0 0 1-3 3H9a3 3 0 0 1-3-3z"), S("M10 14V10a6 6 0 0 1 12 0v4", 4f, false)),
            [Mote] = new Shape(32, F("M16 2l5 9 9 5-9 5-5 9-5-9-9-5 9-5z")),
            [Shard] = new Shape(32, F("M16 2l9 10-9 18-9-18z"), CutS("M7 12h18", 2.5f)),
            // The Hand as the HUD draws it: four slots on a diamond round the surge.
            [Hand] = new Shape(32, F("M16 2l5 5-5 5-5-5zM7 11l5 5-5 5-5-5zM25 11l5 5-5 5-5-5zM16 20l5 5-5 5-5-5zM13 16a3 3 0 1 0 6 0a3 3 0 1 0-6 0z")),

            [Sheath] = new Shape(64, S("M14 54L46 12", 9f), S("M40 22l10 7", 5f), S("M48 14l6-8", 6f), F("M9 54a5 5 0 1 0 10 0a5 5 0 1 0-10 0z")),
            [Click] = new Shape(48, S("M6 30h36", 5f), S("M10 22l6 8-6 8M24 18v24M38 22l-6 8 6 8", 4f), F("M20 10a4 4 0 1 0 8 0a4 4 0 1 0-8 0z")),
            [Shrine] = new Shape(48, F("M4 10h40l-3 6H7z"), S("M9 20h30", 4f, false), S("M13 16v28M35 16v28", 5f)),
            [Stone] = new Shape(48, F("M6 40l9-22 8 8 7-16 12 30z")),
            [Feather] = new Shape(48, F("M40 6C22 8 10 20 10 36l-4 6 3 1 4-6c16 0 26-12 27-31z"), CutS("M13 37L32 16", 3f)),
            [Wave] = new Shape(64, F("M4 40c8-14 20-20 32-16-8 2-12 8-10 14 4 8 16 6 22-2 4-6 8-8 12-6v20H4z")),
            [Shield] = new Shape(64, F("M32 4l24 10v16c0 14-10 24-24 30C18 54 8 44 8 30V14z")),
            [Crown] = new Shape(64, F("M6 20l14 12 12-20 12 20 14-12-6 32H12z")),
            [Star] = new Shape(64, F("M32 4l7 19h20l-16 12 6 20-17-12-17 12 6-20L5 23h20z")),
            [Ring] = new Shape(64, S("M14 32a18 18 0 1 0 36 0a18 18 0 1 0-36 0z", 8f)),
            [Blade] = new Shape(64, F("M50 6h8v8L28 44l-8-8z"), S("M14 38l12 12", 6f), S("M18 46L8 56", 6f)),
            [Wing] = new Shape(64, F("M6 50C10 28 28 12 58 8c-6 8-10 14-18 18 6 0 10-2 14-4-6 10-14 16-24 18 4 2 8 2 12 0-8 8-18 10-36 10z")),
        };

        // Chart icon names with no shape of their own here, drawn as the nearest one until they get theirs.
        private static readonly Dictionary<string, string> nearest = new Dictionary<string, string>
        {
            ["blade"] = Blade, ["shield"] = Shield, ["wave"] = Wave, ["ring"] = Ring, ["crown"] = Crown, ["star"] = Star,
            ["wing"] = Wing, ["sun"] = Sun, ["drop"] = Drop, ["bolt"] = Bolt, ["rock"] = Rock, ["anvil"] = Anvil,
        };

        /// <summary>Every key that has a shape.</summary>
        public static IEnumerable<string> Keys => shapes.Keys;

        public static bool Has(string key) => key != null && shapes.ContainsKey(key);

        /// <summary>The icon's shape; an unknown key is drawn as the star, so nothing is a blank box.</summary>
        public static Shape Get(string key)
        {
            if (key != null && shapes.TryGetValue(key, out var shape)) return shape;
            if (key != null && nearest.TryGetValue(key, out string other)) return shapes[other];
            return shapes[Star];
        }

        /// <summary>An element's icon key: the chart's icon for it (flame for Fire), or the star when there is none.</summary>
        public static string ForElement(string element)
        {
            if (string.IsNullOrEmpty(element)) return Star;
            var info = Clover.Player.Elements.ByName(element);
            return info != null && Has(info.Icon) ? info.Icon : Star;
        }

        /// <summary>A role's icon key.</summary>
        public static string ForRole(Clover.Player.SpellRole role) => role switch
        {
            Clover.Player.SpellRole.Opener => RoleOpener,
            Clover.Player.SpellRole.Breaker => RoleBreaker,
            Clover.Player.SpellRole.Detonator => RoleDetonator,
            Clover.Player.SpellRole.Counter => RoleCounter,
            Clover.Player.SpellRole.Movement => RoleMovement,
            _ => "",
        };

        // ---- Path data, read once into drawing steps.

        public enum StepKind
        {
            Move,
            Line,
            Cubic,
            Quad,
            Arc,
            Close,
        }

        /// <summary>One drawing step in the icon's box: a point, control points, or an arc's centre, radius and angles (degrees).</summary>
        public struct Step
        {
            public StepKind Kind;
            public Vector2 A, B, C;
            public float Radius, From, To;
            public bool Clockwise;
        }

        private static readonly Dictionary<string, Step[]> parsed = new Dictionary<string, Step[]>();

        /// <summary>The steps of an SVG path (M, L, H, V, C, S, Q, A, Z, absolute and relative), read once and kept.</summary>
        public static Step[] Steps(string path)
        {
            if (path == null) return Array.Empty<Step>();
            if (parsed.TryGetValue(path, out var known)) return known;
            var made = Parse(path);
            parsed[path] = made;
            return made;
        }

        private static Step[] Parse(string path)
        {
            var steps = new List<Step>();
            int at = 0;
            char command = 'M';
            Vector2 now = Vector2.zero, start = Vector2.zero, lastControl = Vector2.zero;
            char lastCommand = ' ';
            while (true)
            {
                SkipSpace(path, ref at);
                if (at >= path.Length) break;
                char c = path[at];
                if (char.IsLetter(c))
                {
                    command = c;
                    at++;
                }
                else if (lastCommand == ' ') throw new FormatException("path must begin with a command: " + path);
                bool relative = char.IsLower(command);
                Vector2 origin = relative ? now : Vector2.zero;
                switch (char.ToUpperInvariant(command))
                {
                    case 'M':
                        now = origin + new Vector2(Number(path, ref at), Number(path, ref at));
                        start = now;
                        steps.Add(new Step { Kind = StepKind.Move, A = now });
                        // Pairs after a move are lines.
                        command = relative ? 'l' : 'L';
                        break;
                    case 'L':
                        now = origin + new Vector2(Number(path, ref at), Number(path, ref at));
                        steps.Add(new Step { Kind = StepKind.Line, A = now });
                        break;
                    case 'H':
                        now = new Vector2((relative ? now.x : 0f) + Number(path, ref at), now.y);
                        steps.Add(new Step { Kind = StepKind.Line, A = now });
                        break;
                    case 'V':
                        now = new Vector2(now.x, (relative ? now.y : 0f) + Number(path, ref at));
                        steps.Add(new Step { Kind = StepKind.Line, A = now });
                        break;
                    case 'C':
                    {
                        var one = origin + new Vector2(Number(path, ref at), Number(path, ref at));
                        var two = origin + new Vector2(Number(path, ref at), Number(path, ref at));
                        now = origin + new Vector2(Number(path, ref at), Number(path, ref at));
                        steps.Add(new Step { Kind = StepKind.Cubic, A = one, B = two, C = now });
                        lastControl = two;
                        break;
                    }
                    case 'S':
                    {
                        char last = char.ToUpperInvariant(lastCommand);
                        var one = last == 'C' || last == 'S' ? now * 2f - lastControl : now;
                        var two = origin + new Vector2(Number(path, ref at), Number(path, ref at));
                        now = origin + new Vector2(Number(path, ref at), Number(path, ref at));
                        steps.Add(new Step { Kind = StepKind.Cubic, A = one, B = two, C = now });
                        lastControl = two;
                        break;
                    }
                    case 'Q':
                    {
                        var control = origin + new Vector2(Number(path, ref at), Number(path, ref at));
                        now = origin + new Vector2(Number(path, ref at), Number(path, ref at));
                        steps.Add(new Step { Kind = StepKind.Quad, A = control, B = now });
                        break;
                    }
                    case 'A':
                    {
                        float radius = Number(path, ref at);
                        Number(path, ref at); // the second radius: only circles are drawn here
                        Number(path, ref at); // the rotation, which a circle does not need
                        bool large = Flag(path, ref at);
                        bool sweep = Flag(path, ref at);
                        var end = origin + new Vector2(Number(path, ref at), Number(path, ref at));
                        steps.Add(ArcStep(now, end, radius, large, sweep));
                        now = end;
                        break;
                    }
                    case 'Z':
                        steps.Add(new Step { Kind = StepKind.Close });
                        now = start;
                        break;
                    default:
                        throw new FormatException("unknown path command " + command + " in " + path);
                }
                lastCommand = command;
            }
            return steps.ToArray();
        }

        // An SVG arc between two points, as a centre, a radius and two angles (the endpoint-to-centre form, for circles).
        private static Step ArcStep(Vector2 from, Vector2 to, float radius, bool large, bool sweep)
        {
            var half = (from - to) * 0.5f;
            float squared = half.sqrMagnitude;
            if (squared <= 0f) return new Step { Kind = StepKind.Line, A = to };
            float r = Mathf.Max(radius, Mathf.Sqrt(squared));
            float factor = Mathf.Sqrt(Mathf.Max(0f, (r * r - squared) / squared)) * (large == sweep ? -1f : 1f);
            var centre = new Vector2(factor * half.y, -factor * half.x) + (from + to) * 0.5f;
            float a0 = Mathf.Atan2(from.y - centre.y, from.x - centre.x) * Mathf.Rad2Deg;
            float a1 = Mathf.Atan2(to.y - centre.y, to.x - centre.x) * Mathf.Rad2Deg;
            return new Step { Kind = StepKind.Arc, A = centre, B = to, Radius = r, From = a0, To = a1, Clockwise = sweep };
        }

        private static void SkipSpace(string path, ref int at)
        {
            while (at < path.Length && (path[at] == ' ' || path[at] == ',' || path[at] == '\n' || path[at] == '\t')) at++;
        }

        private static bool Flag(string path, ref int at)
        {
            SkipSpace(path, ref at);
            if (at >= path.Length) throw new FormatException("path ended inside an arc: " + path);
            return path[at++] == '1';
        }

        private static float Number(string path, ref int at)
        {
            SkipSpace(path, ref at);
            int begin = at;
            if (at < path.Length && (path[at] == '-' || path[at] == '+')) at++;
            bool dot = false;
            while (at < path.Length && (char.IsDigit(path[at]) || (path[at] == '.' && !dot)))
            {
                if (path[at] == '.') dot = true;
                at++;
            }
            if (at == begin) throw new FormatException("a number was expected at " + begin + " in " + path);
            return float.Parse(path.Substring(begin, at - begin), NumberStyles.Float, CultureInfo.InvariantCulture);
        }
    }
}

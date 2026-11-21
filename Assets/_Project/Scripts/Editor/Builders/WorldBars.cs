using Clover.Gameplay;
using UnityEngine;

namespace Clover.EditorTools
{
    /// <summary>
    /// The small things that float over or lie under a fighter in the world: its health and balance bars in their inked
    /// frames, the mark that stands over it while it is staggered, and the mark on the ground for its attack.
    /// </summary>
    public static class WorldBars
    {
        private const string Folder = BuildKit.GeneratedRoot + "/Enemies";
        private const float Length = 1.2f;

        /// <summary>
        /// Health and balance bars above a fighter, turned to face the camera, which never rotates. Each sits in a frame
        /// like the HUD's: ink, with a thin pale line set in from its edge. Behind the health a pale piece lingers where
        /// a blow has just taken some, and the balance bar is put away while it is empty.
        /// </summary>
        public static (Transform health, Transform stagger, Transform root) Build(Transform parent, float height, Quaternion towardCamera,
            Material back, Material healthMaterial, Material staggerMaterial)
        {
            // Drawn over whatever is in front of them: a tall fighter's own body must not cover its bars.
            var ink = BuildKit.Glow(Folder, "m_bar_ink", UiArt.Ink, 3, false, 2f, 0f, true);
            var line = BuildKit.Glow(Folder, "m_bar_line", new Color(0.8f, 0.72f, 0.55f, 0.8f), 3, false, 2f, 0f, true);
            var pale = BuildKit.Glow(Folder, "m_bar_lag", new Color(1f, 0.9f, 0.75f, 1f), 3, false, 2f, 0f, true);

            var bars = BuildKit.Group("Bars", parent);
            bars.localPosition = new Vector3(0f, height, 0f);
            bars.rotation = towardCamera;

            Frame("Health Frame", bars, 0.11f, 0.14f, ink, line, back);
            var lag = Fill("Health Lag", bars, 0.11f, 0.1f, pale, 3);
            var health = Fill("Health", bars, 0.11f, 0.1f, healthMaterial, 4);

            // The balance bar hangs under the health bar and shares its lower edge; it is only there while there is something to show.
            var balanceRow = BuildKit.Group("Balance Row", bars);
            Frame("Balance Frame", balanceRow, -0.035f, 0.07f, ink, line, back);
            var stagger = Fill("Stagger", balanceRow, -0.035f, 0.05f, staggerMaterial, 4);

            // Under the bars, a row of small diamonds: one lights for each affliction the fighter carries, in that affliction's colour.
            var pipInk = BuildKit.Glow(Folder, "m_bar_pip_ink", UiArt.Ink, 3, false, 2f, 0f, true);
            var pipLight = BuildKit.Glow(Folder, "m_bar_pip", Color.white, 3, false, 2f, 0f, true);
            var pipRow = BuildKit.Group("Afflictions", bars);
            var pips = new Renderer[6];
            for (int i = 0; i < pips.Length; i++)
            {
                var holder = BuildKit.Group("Pip " + (i + 1), pipRow);
                holder.localPosition = new Vector3(-Length * 0.5f + 0.06f + i * 0.2f, -0.215f, 0f);
                holder.localRotation = Quaternion.Euler(0f, 0f, 45f);
                Layer(BuildKit.Shape(PrimitiveType.Quad, "Pip Ink", holder, new Vector3(0f, 0f, 0.01f), new Vector3(0.17f, 0.17f, 1f), pipInk), 5);
                pips[i] = Layer(BuildKit.Shape(PrimitiveType.Quad, "Pip Light", holder, Vector3.zero, new Vector3(0.115f, 0.115f, 1f), pipLight), 6);
            }

            var shown = bars.gameObject.AddComponent<EnemyBars>();
            shown.Configure(health, lag, stagger, balanceRow.gameObject);
            shown.ConfigurePips(pips);
            return (health, stagger, bars);
        }

        /// <summary>
        /// A name over a fighter's bars, for the few who have one: small pale letters edged in ink, standing just above
        /// the health bar and facing the camera with it. It is part of the bars, so it comes and goes with them.
        /// </summary>
        public static void NamePlate(Transform bars, string name)
        {
            var plate = new GameObject("Name Plate", typeof(RectTransform));
            plate.transform.SetParent(bars, false);
            var canvas = plate.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.WorldSpace;
            canvas.sortingOrder = 6;
            var area = (RectTransform)plate.transform;
            area.sizeDelta = new Vector2(600f, 80f);
            area.localPosition = new Vector3(0f, 0.36f, 0f);
            area.localRotation = Quaternion.identity;
            // Laid out large and shrunk, so the letters are sharp at the size they are seen.
            area.localScale = Vector3.one * 0.0042f;

            // A name makes the bars taller, so a neighbour's bars keep further off.
            var room = bars.GetComponent<EnemyBars>();
            if (room != null) room.Tall = 0.9f;

            var label = plate.AddComponent<UnityEngine.UI.Text>();
            label.font = UiArt.Font();
            // The world would hide an ordinary canvas that stands in it; these letters are drawn over it.
            label.material = BuildKit.MaterialAt(Folder, "m_name_plate", "Clover/WorldText");
            label.fontSize = 46;
            label.alignment = TextAnchor.MiddleCenter;
            label.color = new Color(1f, 0.93f, 0.8f);
            label.raycastTarget = false;
            label.horizontalOverflow = HorizontalWrapMode.Overflow;
            label.verticalOverflow = VerticalWrapMode.Overflow;
            label.text = name;
            var edge = plate.AddComponent<UnityEngine.UI.Outline>();
            edge.effectColor = new Color(0.02f, 0.015f, 0.05f, 0.95f);
            edge.effectDistance = new Vector2(3f, -3f);
        }

        // The slab a bar sits in: ink outermost, the pale line inside it, and the dark the bar empties to.
        private static void Frame(string name, Transform parent, float height, float inside, Material ink, Material line, Material track)
        {
            Layer(BuildKit.Shape(PrimitiveType.Quad, name, parent, new Vector3(0f, height, 0.03f), new Vector3(Length + 0.2f, inside + 0.14f, 1f), ink), 0);
            Layer(BuildKit.Shape(PrimitiveType.Quad, name + " Line", parent, new Vector3(0f, height, 0.02f), new Vector3(Length + 0.095f, inside + 0.035f, 1f), line), 1);
            Layer(BuildKit.Shape(PrimitiveType.Quad, name + " Track", parent, new Vector3(0f, height, 0.01f), new Vector3(Length + 0.06f, inside, 1f), track), 2);
        }

        // A bar that shrinks toward its left end: the pivot sits at the left and the block hangs to its right.
        private static Transform Fill(string name, Transform parent, float height, float thickness, Material material, int order)
        {
            var pivot = BuildKit.Group(name, parent);
            pivot.localPosition = new Vector3(-Length * 0.5f, height, 0f);
            Layer(BuildKit.Shape(PrimitiveType.Quad, name + " Block", pivot, new Vector3(Length * 0.5f, 0f, 0f), new Vector3(Length, thickness, 1f), material), order);
            return pivot;
        }

        // These lie almost on top of one another, so the order they are drawn in is said outright.
        private static Renderer Layer(Renderer renderer, int order)
        {
            Plain(renderer);
            renderer.sortingOrder = order;
            return renderer;
        }

        private static void Plain(Renderer renderer)
        {
            renderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            renderer.receiveShadows = false;
        }

        /// <summary>
        /// The finish mark's crossed cut: a pale gold held below white, so it still stands out from the deeper gold ring
        /// without blooming to pure white (the swing films of 2026-11-20: about 2,000 near-white pixels on every staggered
        /// enemy, read as a white starburst). Public so the material can be remade without rebuilding a scene.
        /// </summary>
        public static Material FinishCutMaterial() =>
            BuildKit.Glow(Folder, "m_mark_cut", new Color(1.75f, 1.3f, 0.48f, 1f), 9, false, 1f, 0f, true).Graphic(0.54f);

        /// <summary>
        /// The mark that stands over a staggered fighter: a broken ring that turns, round a crossed cut that beats, in
        /// gold on ink. It is drawn through whatever is in front of it, since it is the one thing that must be seen.
        /// </summary>
        public static StaggerMark FinishMark(Transform parent, Vector3 localPosition, float size, Quaternion towardCamera)
        {
            var ringInk = BuildKit.Glow(Folder, "m_mark_ring_ink", UiArt.Ink, 8, false, 1f, 0f, true).Graphic(0.36f);
            var ringGold = BuildKit.Glow(Folder, "m_mark_ring", new Color(1.9f, 1.25f, 0.22f, 1f), 8, false, 1f, 0f, true).Graphic(0.62f);
            var cutInk = BuildKit.Glow(Folder, "m_mark_cut_ink", UiArt.Ink, 9, false, 1f, 0f, true).Graphic(0.36f);
            var cutGold = FinishCutMaterial();

            var mark = BuildKit.Group("Finish Mark", parent);
            mark.localPosition = localPosition;
            mark.rotation = towardCamera;
            mark.localScale = new Vector3(size, size, size);
            var shapes = BuildKit.Group("Shapes", mark);
            var ring = BuildKit.Group("Ring", shapes);
            Layer(BuildKit.Shape(PrimitiveType.Quad, "Ring Ink", ring, new Vector3(0f, 0f, 0.02f), Vector3.one, ringInk), 30);
            Layer(BuildKit.Shape(PrimitiveType.Quad, "Ring Gold", ring, Vector3.zero, Vector3.one, ringGold), 31);
            var cut = BuildKit.Group("Cut", shapes);
            Layer(BuildKit.Shape(PrimitiveType.Quad, "Cut Ink", cut, new Vector3(0f, 0f, 0.01f), Vector3.one, cutInk), 32);
            Layer(BuildKit.Shape(PrimitiveType.Quad, "Cut Gold", cut, new Vector3(0f, 0f, -0.01f), Vector3.one, cutGold), 33);

            var staggerMark = mark.gameObject.AddComponent<StaggerMark>();
            staggerMark.Configure(shapes.gameObject, ring, cut);
            return staggerMark;
        }

        /// <summary>An empty mesh holder lying on the ground; the fighter gives it the shape of its attack when the game starts.</summary>
        public static MeshFilter Marker(string name, Transform parent, float height, Material material)
        {
            var marker = new GameObject(name);
            marker.transform.SetParent(parent, false);
            marker.transform.localPosition = new Vector3(0f, height, 0f);
            var filter = marker.AddComponent<MeshFilter>();
            var renderer = marker.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = material;
            Plain(renderer);
            renderer.enabled = false;
            return filter;
        }
    }
}

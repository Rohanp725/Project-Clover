namespace Clover.Player.Swings
{
    /// <summary>
    /// One element's look on a swung weapon (the owner's note of 2026-11-19: the element moving with the swing, not
    /// pieces on the blade): the layers of its trail, what it sheds and how it ends, and whether it draws its own landing
    /// in place of the hit art. Read from <see cref="SwingLooks"/>. Nothing of the element is drawn on the blade itself
    /// (the owner, 2026-11-20: "take out all the elements on top of the blade effects"): no look sets a sheath or thorns,
    /// and the weapon shows the element only in its dyed accent and inlay (<see cref="WeaponRack"/>).
    /// </summary>
    public class SwingLook
    {
        /// <summary>The element's place on the ring (<see cref="Forms.ElementSubstance"/>), and its name.</summary>
        public int Element;
        public string Name = "";
        /// <summary>The look in a line, for the guide and the tests.</summary>
        public string Line = "";
        public TrailLayer[] Layers = new TrailLayer[0];
        /// <summary>A strip of the element drawn along the edge while charged: none since 2026-11-20 (see above).</summary>
        public SheathStyle Sheath;
        /// <summary>Thorns grown along the real edge of each weapon while charged: none since 2026-11-20 (see above).</summary>
        public bool Thorns;
        public SwingShed Shed;
        /// <summary>Seconds between two sheds at the trail's head.</summary>
        public float ShedEvery = 0.06f;
        public SwingEnd End;
        /// <summary>True when the charged blow's landing is drawn by the swing look and not by the hit art (Ice's crack-web, Earth's floating cubes).</summary>
        public bool ReplacesHitArt;
        /// <summary>
        /// True when a landed blow draws no cut streak through the body: Wind's, which goes round what it strikes (the
        /// owner, 2026-11-20: "Wind is a line through the enemy"; nothing in a wind look passes through the body).
        /// </summary>
        public bool CutsRound;
        /// <summary>
        /// The colour the trail is drawn in, which a landed blow's flash and cut streak take, softened, under this element
        /// (the owner's "Tint and soften", 2026-11-19; <see cref="Core.StruckLook"/>), so the two match.
        /// Clear until set, so a look that forgets it keeps the plain white rather than a half-strength one.
        /// </summary>
        public UnityEngine.Color Tint;
        /// <summary>
        /// The file a later art pass is to draw the trail's texture as (Art/VFX/&lt;Element&gt;/), empty until it exists. The
        /// trail is built to take one: U runs along the swing from tail to head, V across from the inner edge to the outer.
        /// </summary>
        public string RibbonTexture = "";
    }
}

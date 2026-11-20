using System.Collections.Generic;
using Clover.Core;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Clover.Player.Forms
{
    /// <summary>
    /// The elements' approved effect art in the scene (docs/GAME.md, "Elemental effects in the game"): the kits' pieces,
    /// the drawn sheets and the floor marks of <see cref="ElementArtSet"/>, put down through <see cref="FormFx"/>'s
    /// pools so nothing is made during play. The moments (<see cref="AfflictionArt"/>, <see cref="SpellArt"/>,
    /// <see cref="HitArt"/>, <see cref="BladeArt"/>, <see cref="StrikeArt"/>, <see cref="SurgeArt"/>) ask here.
    ///
    /// The owner's switch, for the playtest only and temporary: F8 flips between these effects and the old ones, so the
    /// two can be compared in play. <see cref="On"/> is that switch; with it off, or with no art in the scene, every
    /// moment draws what it drew before.
    /// </summary>
    public class ElementArt : MonoBehaviour
    {
        [SerializeField] private ElementArtSet set;

        /// <summary>Seconds the line naming the effects shown stays up after the switch is pressed.</summary>
        public float LabelSeconds = 2.5f;

        /// <summary>The owner's switch: the new effects (true) or the old ones. Every play starts with the new.</summary>
        public static bool On { get; set; } = true;

        /// <summary>True when the new effects are to be drawn: the switch is on and the scene has the art.</summary>
        public static bool Ready => On && instance != null && instance.set != null;

        /// <summary>True when the scene has the art, whichever way the switch is: for the charged blade's old look, which was drawn with it.</summary>
        public static bool HasArt => instance != null && instance.set != null;

        /// <summary>How many times the switch has been pressed this play, for probes.</summary>
        public static int Switched { get; private set; }

        private static ElementArt instance;

        private readonly Dictionary<string, int> pieces = new Dictionary<string, int>();
        private readonly Dictionary<string, int> sheets = new Dictionary<string, int>();
        private Transform player;
        private Text label;
        private float labelUntil;

        // Domain reload is off: the switch and the scene's art from the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            instance = null;
            On = true;
            Switched = 0;
        }

        /// <summary>The builder hands over the art.</summary>
        public void Configure(ElementArtSet art) => set = art;

        private void Awake()
        {
            if (set == null) return;
            for (int i = 0; i < set.Pieces.Length; i++) pieces[set.Pieces[i].Name] = i;
            for (int i = 0; i < set.Sheets.Length; i++) sheets[set.Sheets[i].Name] = i;
            MakeLabel();
            // The reactions' looks come with the art, so every scene that has it draws them without being rebuilt.
            if (set.Reactions != null)
            {
                var reactions = GetComponent<ReactionArt>();
                if (reactions == null) reactions = gameObject.AddComponent<ReactionArt>();
                reactions.Configure(set.Reactions);
            }
            // A burning body is drawn in real flame when the art has the flame's material.
            if (set.BodyFlame != null)
            {
                var flames = GetComponent<BodyFlames>();
                if (flames == null) flames = gameObject.AddComponent<BodyFlames>();
                flames.Configure(set.BodyFlame);
            }
        }

        private void OnEnable() => instance = this;

        private void OnDisable()
        {
            if (instance == this) instance = null;
        }

        private void Start()
        {
            var controller = FindAnyObjectByType<PlayerController>();
            player = controller != null ? controller.transform : null;
        }

        private void Update()
        {
            // A player that arrives after the scene starts is looked for now and then, never every frame.
            if (player == null && Time.frameCount % 60 == 0)
            {
                var controller = FindAnyObjectByType<PlayerController>();
                player = controller != null ? controller.transform : null;
            }
            // The switch answers only where comparing is the point: the playtest, and the editor.
            var keyboard = Keyboard.current;
            if (keyboard != null && keyboard.f8Key.wasPressedThisFrame && (PlaytestMode.On || Application.isEditor)) Flip();
            if (label != null && label.enabled && Time.unscaledTime >= labelUntil) label.enabled = false;
        }

        /// <summary>Flips the switch and says which effects are shown now.</summary>
        public static void Flip()
        {
            On = !On;
            Switched++;
            if (instance == null || instance.label == null) return;
            instance.label.text = On ? "Effects: new (F8 for the old)" : "Effects: old (F8 for the new)";
            instance.label.enabled = true;
            instance.labelUntil = Time.unscaledTime + instance.LabelSeconds;
        }

        // A plain line at the top of the screen: a status line, gone by itself.
        private void MakeLabel()
        {
            if (set.Label == null) return;
            var canvasObject = new GameObject("Effects Switch Line");
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 400;
            var scaler = canvasObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920f, 1080f);
            var textObject = new GameObject("Line");
            textObject.transform.SetParent(canvasObject.transform, false);
            label = textObject.AddComponent<Text>();
            label.font = set.Label;
            label.fontSize = 34;
            label.alignment = TextAnchor.UpperCenter;
            label.color = new Color(1f, 0.97f, 0.9f, 1f);
            label.raycastTarget = false;
            var shadow = textObject.AddComponent<Outline>();
            shadow.effectColor = new Color(0f, 0f, 0f, 0.85f);
            shadow.effectDistance = new Vector2(2f, -2f);
            var rect = label.rectTransform;
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.anchoredPosition = new Vector2(0f, -90f);
            rect.sizeDelta = new Vector2(0f, 60f);
            label.enabled = false;
        }

        // ---- Looking things up.

        /// <summary>A piece by its kit name; -1 when the scene has no art or no such piece.</summary>
        public static int Piece(string name) => instance != null && instance.pieces.TryGetValue(name, out int i) ? i : -1;

        /// <summary>A drawn sheet by its name; -1 when there is none.</summary>
        public static int SheetOf(string name) => instance != null && instance.sheets.TryGetValue(name, out int i) ? i : -1;

        /// <summary>Where the player stands, for marks that reach toward them (Dark's shadow).</summary>
        public static Vector3 PlayerAt => instance != null && instance.player != null ? instance.player.position : Vector3.zero;

        public static bool HasPlayer => instance != null && instance.player != null;

        private static bool Valid(int piece) => instance != null && instance.set != null && piece >= 0 && piece < instance.set.Pieces.Length;

        /// <summary>A piece's mesh, material, kind and footprint, for <see cref="ReactionArt"/> to draw in its own pool; false for none.</summary>
        public static bool TryPiece(int piece, out ElementArtSet.Piece data)
        {
            data = Valid(piece) ? instance.set.Pieces[piece] : default;
            return Valid(piece);
        }

        /// <summary>The size of a piece's footprint, in metres.</summary>
        public static Vector3 SizeOf(int piece) => Valid(piece) ? instance.set.Pieces[piece].Size : Vector3.one;

        // ---- Putting pieces down. Yaw turns a piece about the up axis from the way it was drawn; scale is uniform.

        private const float Ink = 0.025f;

        /// <summary>A piece standing whole for some seconds: a solid or see-through shape, or a mark on the floor.</summary>
        public static void Stand(int piece, Vector3 at, float yaw, float scale, float seconds) => Stand(piece, at, yaw, scale, seconds, Color.white);

        public static void Stand(int piece, Vector3 at, float yaw, float scale, float seconds, Color tint)
        {
            if (!Valid(piece)) return;
            ref var one = ref instance.set.Pieces[piece];
            if (one.Kind == ElementArtSet.Decal)
            {
                Floor(piece, at, yaw, scale, scale, seconds, 0f, true, tint);
                return;
            }
            FormFx.StandMesh(one.Mesh, one.Material, at, Quaternion.Euler(0f, yaw, 0f), Vector3.one * scale, tint, seconds, Ink);
        }

        /// <summary>A piece bursting up out of the ground (stretched up from flat), standing, and sinking back.</summary>
        public static void Erupt(int piece, Vector3 at, Quaternion rotation, Vector3 scale, float rise, float hold, float sink) =>
            Erupt(piece, at, rotation, scale, rise, hold, sink, Color.white);

        public static void Erupt(int piece, Vector3 at, Quaternion rotation, Vector3 scale, float rise, float hold, float sink, Color tint)
        {
            if (!Valid(piece)) return;
            ref var one = ref instance.set.Pieces[piece];
            if (one.Kind == ElementArtSet.Decal)
            {
                Floor(piece, at, rotation.eulerAngles.y, scale.x * 0.6f, scale.x, rise + hold + sink, 0f, true, tint);
                return;
            }
            FormFx.EruptMesh(one.Mesh, one.Material, at, rotation, scale, tint, rise, hold, sink, Ink);
        }

        /// <summary>A piece flying from one point to another, shrinking as it goes.</summary>
        public static void Fly(int piece, Vector3 from, Vector3 to, Quaternion rotation, float scale, float seconds)
        {
            if (!Valid(piece)) return;
            ref var one = ref instance.set.Pieces[piece];
            if (one.Kind == ElementArtSet.Decal) return;
            FormFx.FlyMesh(one.Mesh, one.Material, from, to, rotation, Vector3.one * scale, Color.white, seconds, Ink);
        }

        /// <summary>A piece falling to a point, stuck there a while and shrinking away.</summary>
        public static void Drop(int piece, Vector3 from, Vector3 to, Quaternion rotation, float scale, float seconds, float stay, float sink)
        {
            if (!Valid(piece)) return;
            ref var one = ref instance.set.Pieces[piece];
            if (one.Kind == ElementArtSet.Decal) return;
            FormFx.DropMesh(one.Mesh, one.Material, from, to, rotation, Vector3.one * scale, Color.white, seconds, stay, sink, Ink);
        }

        /// <summary>A piece thrown up from a point at a velocity, tumbling, that falls back to the ground it left and settles away.</summary>
        public static void Toss(int piece, Vector3 from, Vector3 velocity, Quaternion rotation, float scale, float settle)
        {
            if (!Valid(piece)) return;
            ref var one = ref instance.set.Pieces[piece];
            if (one.Kind == ElementArtSet.Decal) return;
            FormFx.TossMesh(one.Mesh, one.Material, from, velocity, rotation, Vector3.one * scale, Color.white, settle, Ink);
        }

        /// <summary>A drawn sheet played through, carried along by <paramref name="drift"/> and breaking up at its end (<see cref="FormFx.BlowSheet"/>).</summary>
        public static void BlowSheet(int sheet, Vector3 at, float size, float seconds, Vector3 drift, float spread = 1.3f) =>
            BlowSheet(sheet, at, size, seconds, drift, spread, Color.white);

        public static void BlowSheet(int sheet, Vector3 at, float size, float seconds, Vector3 drift, float spread, Color tint)
        {
            if (instance == null || instance.set == null || sheet < 0 || sheet >= instance.set.Sheets.Length) return;
            FormFx.BlowSheet(instance.set.Sheets[sheet].Material, at, size, tint, seconds, drift, spread);
        }

        /// <summary>Takes a piece to be placed by the caller every frame (<see cref="FormFx.Place"/>); -1 when none can be had or it is a floor mark.</summary>
        public static int Hold(int piece)
        {
            if (!Valid(piece)) return -1;
            ref var one = ref instance.set.Pieces[piece];
            if (one.Kind == ElementArtSet.Decal) return -1;
            return FormFx.HoldMesh(one.Mesh, one.Material, Color.white, Ink);
        }

        /// <summary>
        /// A piece's mark on the floor at a point, turned by <paramref name="yaw"/>, growing from one scale to another
        /// over its life, drawn on over <paramref name="revealSeconds"/> when that is above zero, fading at its end or not.
        /// </summary>
        public static void Floor(int piece, Vector3 at, float yaw, float scale, float endScale, float seconds, float revealSeconds = 0f, bool fades = true) =>
            Floor(piece, at, yaw, scale, endScale, seconds, revealSeconds, fades, Color.white);

        public static void Floor(int piece, Vector3 at, float yaw, float scale, float endScale, float seconds, float revealSeconds, bool fades, Color tint, float spinDegrees = 0f)
        {
            if (!Valid(piece)) return;
            ref var one = ref instance.set.Pieces[piece];
            if (one.Kind != ElementArtSet.Decal)
            {
                FormFx.StandMesh(one.Mesh, one.Material, at, Quaternion.Euler(0f, yaw, 0f), Vector3.one * scale, tint, seconds, Ink);
                return;
            }
            var turn = Quaternion.Euler(0f, yaw, 0f);
            // The mark's quad is centred on its footprint, which need not be the piece's origin.
            Vector3 middle = at + turn * new Vector3(one.Centre.x * scale, 0f, one.Centre.z * scale);
            var size = new Vector3(one.Size.x * scale, 1f, one.Size.z * scale);
            var endSize = new Vector3(one.Size.x * endScale, 1f, one.Size.z * endScale);
            FormFx.LayDecal(one.Material, middle, turn, size, endSize, tint, seconds, revealSeconds, fades, spinDegrees);
        }

        /// <summary>The yaw that turns a kit piece drawn toward <paramref name="kitForward"/> to face <paramref name="forward"/>.</summary>
        /// <remarks>
        /// The look-dev's camera looked the opposite way to the game's, so a piece drawn reaching up and to the right of
        /// its panel (toward the enemy) points down and to the left in the kit: about 225 degrees. A piece placed with
        /// a fixed yaw of 180 is seen from the game camera exactly as in its panel.
        /// </remarks>
        public static float Turn(Vector3 forward, float kitForward = 225f)
        {
            forward.y = 0f;
            if (forward.sqrMagnitude < 0.0001f) return 180f;
            return Mathf.Atan2(forward.x, forward.z) * Mathf.Rad2Deg - kitForward;
        }

        /// <summary>Where to put a piece's origin so its point <paramref name="kitAnchor"/> (in the kit's own space) lands on <paramref name="point"/>.</summary>
        public static Vector3 Anchor(Vector3 point, float yaw, Vector3 kitAnchor) => point - Quaternion.Euler(0f, yaw, 0f) * kitAnchor;

        /// <summary>Takes a floor mark to be placed by the caller every frame with <see cref="PlaceFloor"/>; -1 when none.</summary>
        public static int HoldFloor(int piece)
        {
            if (!Valid(piece)) return -1;
            ref var one = ref instance.set.Pieces[piece];
            return one.Kind == ElementArtSet.Decal ? FormFx.HoldDecal(one.Material, Color.white) : -1;
        }

        public static void PlaceFloor(int handle, int piece, Vector3 at, float yaw, float scale)
        {
            if (handle < 0 || !Valid(piece)) return;
            ref var one = ref instance.set.Pieces[piece];
            var turn = Quaternion.Euler(0f, yaw, 0f);
            Vector3 middle = at + turn * new Vector3(one.Centre.x * scale, 0f, one.Centre.z * scale);
            FormFx.PlaceMark(handle, middle, turn, new Vector3(one.Size.x * scale, 1f, one.Size.z * scale));
        }

        /// <summary>A drawn sheet hung at a point, turned to the camera, played through over some seconds (or looping).</summary>
        public static void Sheet(int sheet, Vector3 at, float size, float seconds, float roll = 0f) => Sheet(sheet, at, size, seconds, Color.white, roll);

        public static void Sheet(int sheet, Vector3 at, float size, float seconds, Color tint, float roll = 0f)
        {
            if (instance == null || instance.set == null || sheet < 0 || sheet >= instance.set.Sheets.Length) return;
            ref var one = ref instance.set.Sheets[sheet];
            FormFx.FaceSheet(one.Material, at, size, tint, seconds, one.Loops, 12f, roll);
        }

        /// <summary>
        /// A body's flat shadow lying along the ground from its feet, <paramref name="length"/> metres the way given:
        /// Light throws it away from the light, Dark stretches it toward what it wants. Drawn out from the feet over
        /// <paramref name="reachSeconds"/>.
        /// </summary>
        public static void Shadow(Vector3 feet, Vector3 direction, float length, float width, float seconds, float reachSeconds = 0f)
        {
            if (instance == null || instance.set == null || instance.set.ShadowFigure == null) return;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;
            direction.Normalize();
            var turn = Quaternion.LookRotation(direction, Vector3.up);
            var size = new Vector3(width, 1f, length);
            FormFx.LayDecal(instance.set.ShadowFigure, feet + direction * (length * 0.5f), turn, size, size, Color.white, seconds, reachSeconds, true);
        }
    }
}

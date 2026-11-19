using Clover.Core;
using Clover.Player.Forms;
using UnityEngine;

namespace Clover.Player.Swings
{
    /// <summary>
    /// The element on a swung weapon (the owner's note of 2026-11-19): while a charged or infused swing is out, every
    /// blade line of the weapon in hand (<see cref="BladeLine"/>, measured off the model by the weapon builder; the body's
    /// bones for bare hands) is sampled each frame from a little before its blow to a little after, and each element
    /// layer of <see cref="SwingLooks"/> leaves a trail along the real path that weapon's animation drew, shaped by
    /// <see cref="SwingShapes"/>. Between swings a charged blade wears a thin sheath of its element along the edge (Life
    /// grows thorns there, Metal only glints). Where a blow lands, or a heavy weapon meets the ground, the element is
    /// thrown out (<see cref="SwingImpacts"/>).
    ///
    /// The owner's F8 switch decides: with the new effects on this draws; with the old ones <see cref="BladeLook"/>
    /// draws the charged blade as it was. Everything is made when the player starts: the trails are pooled meshes, the
    /// materials copies of the slash arcs' `Clover/Glow`, and nothing is allocated per frame.
    /// </summary>
    [RequireComponent(typeof(PlayerCombat))]
    [DefaultExecutionOrder(200)]
    public class SwingTrails : MonoBehaviour
    {
        /// <summary>True when the swing looks are drawn: the owner's switch on the new effects.</summary>
        public static bool On => ElementArt.On;

        /// <summary>Trails that can be alive at once.</summary>
        public int RibbonPool = 36;
        /// <summary>The share of the wind-up before the blow that is already drawn, and the seconds after the blow's window.</summary>
        public float LeadShare = 0.6f;
        public float TailSeconds = 0.14f;
        /// <summary>How much fuller an infused strike's swing is than a charged blade's.</summary>
        public float InfusedWeight = 1.3f;
        /// <summary>How strongly the sheath shows between swings (a swing shows it whole).</summary>
        public float IdleSheath = 0.55f;
        /// <summary>The most degrees a trail turns between two of its points: a fast swing gets points put in between.</summary>
        public float DegreesPerStep = 6f;
        /// <summary>Most landings drawn for one swing, so a crowd struck at once is not drawn over itself.</summary>
        public int LandingsPerSwing = 3;

        /// <summary>For probes: counts of what was drawn this play, and what is shown now.</summary>
        public int SwingsDrawn { get; private set; }
        public int Landings { get; private set; }
        public int GroundBlows { get; private set; }
        public int RibbonsShown { get; private set; }
        public int SheathsShown { get; private set; }
        public int ThornsHeld { get; private set; }
        public int SourcesActive { get; private set; }
        public int SourcesFitted { get; private set; }
        public int SourcesTotal => sourceCount;
        public bool Ready => baseMaterial != null;

        private struct Source
        {
            public Transform At, TipBone;
            public BladeLineSpec Line;
            public LimbLine Limb;
            public bool IsLimb, Thrown, Fitted;
            public string Carry;
            public Vector3[] World;
            public int WorldCount;
            public Vector3 EdgeSide, LastTip, Way;
            public bool HasLast;
            public float Speed, Length;
        }

        private const int MostSources = 48, MostActive = 8, SheathPool = 6, MostThorns = 40;

        private static readonly int ColourId = Shader.PropertyToID("_Color");
        private static readonly int ShapeId = Shader.PropertyToID("_Shape");
        private static readonly int FireId = Shader.PropertyToID("_Fire");
        private static readonly int SharpnessId = Shader.PropertyToID("_Sharpness");
        private static readonly int HardId = Shader.PropertyToID("_Hard");
        private static readonly int CoreId = Shader.PropertyToID("_Core");
        private static readonly int NoiseId = Shader.PropertyToID("_Noise");
        private static readonly int NoiseScaleId = Shader.PropertyToID("_NoiseScale");
        private static readonly int NoiseSpeedId = Shader.PropertyToID("_NoiseSpeed");
        private static readonly int SrcBlendId = Shader.PropertyToID("_SrcBlend");
        private static readonly int DstBlendId = Shader.PropertyToID("_DstBlend");
        private static readonly int ZTestId = Shader.PropertyToID("_ZTest");

        private PlayerCombat combat;
        private PlayerAnimator animator;
        private Transform viewer, handRight;
        // The last slot is kept for a part with no line of its own, measured when it is shown: the Katana's sheath.
        private const int RestSlot = MostSources - 1;
        private GameObject measuredPart;
        private Material baseMaterial;
        private Material[] layerMaterials, sheathMaterials;
        private bool[] ownMaterial;

        private readonly Source[] sources = new Source[MostSources];
        private int sourceCount;
        private readonly int[] active = new int[MostActive];
        private int activeCount;

        private Ribbon[] ribbons;
        private MeshFilter[] ribbonFilters;
        private MeshRenderer[] ribbonShown;
        private Mesh[] ribbonMeshes;
        private int[] ribbonElement, ribbonLayer, ribbonSource;
        private float[] ribbonWeight;
        private readonly int[] ribbonFor = new int[MostSources * SwingLooks.MostLayers];

        private MeshFilter[] sheathFilters;
        private MeshRenderer[] sheathShown;
        private Mesh[] sheathMeshes;

        private Vector3[] vertices;
        private Vector2[] uvs;
        private Color[] colours;
        private int[] ribbonTriangles, sheathTriangles;

        private readonly int[] thornHandle = new int[MostThorns];
        private readonly int[] thornSource = new int[MostThorns];
        private readonly float[] thornShare = new float[MostThorns];
        private int thornCount;
        private int thornedSourcesKey = -1;

        private Weapon shownWeapon;
        private SwingShape shape = SwingShapes.Katana;
        private AttackStep lastStep;
        private float lastElapsed;
        private ElementInfo swingElement;
        private bool swingInfused, groundDone;
        private int landedThisSwing;
        private float shedLeft;
        private ElementInfo thrownElement;
        private float thrownUntil;
        private float glintFrom = -10f;
        private float chargedSince;
        private int chargedVersion = -1;
        private float dripLeft;

        private void Awake()
        {
            combat = GetComponent<PlayerCombat>();
            animator = GetComponent<PlayerAnimator>();
            handRight = WeaponRack.FindSocket(transform, WeaponRack.HandRight);
            for (int i = 0; i < ribbonFor.Length; i++) ribbonFor[i] = -1;
            for (int i = 0; i < MostThorns; i++) thornHandle[i] = -1;
        }

        private void OnEnable() => combat.BladeMet += Met;

        private void OnDisable()
        {
            combat.BladeMet -= Met;
            LetThorns();
            if (ribbons != null)
                for (int r = 0; r < ribbons.Length; r++)
                {
                    ribbons[r].Clear();
                    if (ribbonShown[r] != null) ribbonShown[r].enabled = false;
                }
            if (sheathShown != null)
                foreach (var shown in sheathShown)
                    if (shown != null) shown.enabled = false;
        }

        private void Start()
        {
            var main = Camera.main;
            viewer = main != null ? main.transform : null;
            var arcs = GetComponent<SlashArcs>();
            baseMaterial = arcs != null ? arcs.Material : null;
            if (baseMaterial == null)
            {
                Debug.LogWarning("[Clover.Player] The player has no slash arcs' material to draw swing trails with; charged and infused swings will show none.");
                enabled = false;
                return;
            }
            MakeMaterials();
            MakePools();
            FindSources();
        }

        private void OnDestroy()
        {
            if (ribbonShown != null)
                foreach (var shown in ribbonShown)
                    if (shown != null) Destroy(shown.gameObject);
            if (sheathShown != null)
                foreach (var shown in sheathShown)
                    if (shown != null) Destroy(shown.gameObject);
            DestroyAll(ribbonMeshes);
            DestroyAll(sheathMeshes);
            if (layerMaterials != null)
                for (int i = 0; i < layerMaterials.Length; i++)
                    if (layerMaterials[i] != null && ownMaterial[i]) Destroy(layerMaterials[i]);
            DestroyAll(sheathMaterials);
        }

        private static void DestroyAll(Object[] things)
        {
            if (things == null) return;
            foreach (var thing in things)
                if (thing != null) Destroy(thing);
        }

        /// <summary>
        /// Puts a material of a later art pass in place of the copied glow for one element's trail layer (or its sheath,
        /// with <paramref name="layer"/> below 0): the hook for per-element ribbon textures. U runs along the trail from
        /// tail to head and V across it; vertex colours carry the fade.
        /// </summary>
        public void UseMaterial(int element, int layer, Material material)
        {
            if (material == null || layerMaterials == null || element < 0 || element >= SwingLooks.All.Length) return;
            if (layer < 0)
            {
                if (sheathMaterials[element] != null) Destroy(sheathMaterials[element]);
                sheathMaterials[element] = material;
                return;
            }
            if (layer >= SwingLooks.MostLayers) return;
            int index = element * SwingLooks.MostLayers + layer;
            if (layerMaterials[index] != null && ownMaterial[index]) Destroy(layerMaterials[index]);
            layerMaterials[index] = material;
            ownMaterial[index] = false;
        }

        // ---- Making.

        private void MakeMaterials()
        {
            int elements = SwingLooks.All.Length;
            layerMaterials = new Material[elements * SwingLooks.MostLayers];
            ownMaterial = new bool[layerMaterials.Length];
            sheathMaterials = new Material[elements];
            for (int e = 0; e < elements; e++)
            {
                var look = SwingLooks.All[e];
                for (int l = 0; l < look.Layers.Length && l < SwingLooks.MostLayers; l++)
                {
                    layerMaterials[e * SwingLooks.MostLayers + l] = Made(look.Layers[l].Style, $"Swing {look.Name} {l}");
                    ownMaterial[e * SwingLooks.MostLayers + l] = true;
                }
                if (look.Sheath.Shown) sheathMaterials[e] = Made(look.Sheath.Style, $"Sheath {look.Name}");
            }
        }

        private Material Made(in GlowStyle style, string label)
        {
            var made = new Material(baseMaterial) { name = label };
            made.SetFloat(ShapeId, style.Shape);
            made.SetFloat(FireId, style.Fire ? 1f : 0f);
            made.SetFloat(SharpnessId, style.Sharpness > 0f ? style.Sharpness : 1f);
            made.SetFloat(HardId, style.Hard);
            made.SetFloat(CoreId, style.Core);
            made.SetFloat(NoiseId, style.Noise);
            made.SetFloat(NoiseScaleId, style.NoiseScale > 0f ? style.NoiseScale : 6f);
            made.SetVector(NoiseSpeedId, new Vector4(style.NoiseSpeed.x, style.NoiseSpeed.y, 0f, 0f));
            made.SetColor(ColourId, style.Colour);
            made.SetFloat(SrcBlendId, (float)UnityEngine.Rendering.BlendMode.SrcAlpha);
            made.SetFloat(DstBlendId, (float)(style.Additive ? UnityEngine.Rendering.BlendMode.One : UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha));
            made.SetFloat(ZTestId, (float)UnityEngine.Rendering.CompareFunction.LessEqual);
            // Laid-over colour first, light added after, so a flame's smoke never hides the light of the same swing.
            made.renderQueue = (int)UnityEngine.Rendering.RenderQueue.Transparent + (style.Additive ? 10 : 0);
            return made;
        }

        private void MakePools()
        {
            int count = Mathf.Max(4, RibbonPool);
            ribbons = new Ribbon[count];
            ribbonFilters = new MeshFilter[count];
            ribbonShown = new MeshRenderer[count];
            ribbonMeshes = new Mesh[count];
            ribbonElement = new int[count];
            ribbonLayer = new int[count];
            ribbonSource = new int[count];
            ribbonWeight = new float[count];
            for (int r = 0; r < count; r++)
            {
                ribbons[r] = new Ribbon();
                // In the world, not under the player, so a trail stays where the swing was made.
                Make("Swing Trail", out ribbonFilters[r], out ribbonShown[r], out ribbonMeshes[r]);
            }
            sheathFilters = new MeshFilter[SheathPool];
            sheathShown = new MeshRenderer[SheathPool];
            sheathMeshes = new Mesh[SheathPool];
            for (int s = 0; s < SheathPool; s++) Make("Blade Sheath", out sheathFilters[s], out sheathShown[s], out sheathMeshes[s]);

            int most = Mathf.Max(Ribbon.Capacity * 2, Sheath.MostVertices);
            vertices = new Vector3[most];
            uvs = new Vector2[most];
            colours = new Color[most];
            ribbonTriangles = Strip(Ribbon.Capacity);
            sheathTriangles = Strip(Sheath.MostVertices / 2);
        }

        private static void Make(string label, out MeshFilter filter, out MeshRenderer shown, out Mesh mesh)
        {
            var part = new GameObject(label);
            filter = part.AddComponent<MeshFilter>();
            shown = part.AddComponent<MeshRenderer>();
            shown.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            shown.receiveShadows = false;
            shown.enabled = false;
            mesh = new Mesh { name = label };
            mesh.MarkDynamic();
            filter.sharedMesh = mesh;
        }

        // The triangles of a strip of pairs, two to each piece between pairs.
        private static int[] Strip(int pairs)
        {
            var triangles = new int[Mathf.Max(0, pairs - 1) * 6];
            for (int k = 0; k + 1 < pairs; k++)
            {
                int a = k * 2;
                triangles[k * 6] = a;
                triangles[k * 6 + 1] = a + 1;
                triangles[k * 6 + 2] = a + 2;
                triangles[k * 6 + 3] = a + 1;
                triangles[k * 6 + 4] = a + 3;
                triangles[k * 6 + 5] = a + 2;
            }
            return triangles;
        }

        // Every line the player can swing: what the builder measured onto each weapon part (in the hands, at rest, the
        // chain's weight, the props that are thrown), the notes' lines for a part built before that, and the body's.
        private void FindSources()
        {
            sourceCount = 0;
            SourcesFitted = 0;
            foreach (var line in GetComponentsInChildren<BladeLine>(true)) AddFitted(line, false);
            var chain = GetComponent<ChainLine>();
            if (chain != null)
            {
                AddPart(chain.WeightProp, "weight", false);
                AddPart(chain.SickleProp, "sickle", true);
            }
            var shots = GetComponent<WeaponShots>();
            if (shots != null)
                for (int p = 0; p < shots.PropCount; p++)
                {
                    var prop = shots.PropAt(p);
                    AddPart(prop, shots.PropName(p), true);
                }
            // Parts hung before the builder measured them: found by the names the rack gives them.
            for (int i = 0; i < BladeLines.Defaults.Length; i++)
            {
                string carry = BladeLines.Defaults[i].Carry;
                if (i > 0 && BladeLines.Defaults[i - 1].Carry == carry) continue;
                AddPart(WeaponRack.FindSocket(transform, carry + "_hand"), carry, false);
                AddPart(WeaponRack.FindSocket(transform, carry + "_hand_L"), carry, false);
                AddPart(WeaponRack.FindSocket(transform, carry + "_rest"), carry, false);
                AddPart(WeaponRack.FindSocket(transform, carry + "_rest_L"), carry, false);
            }
            AddPart(WeaponRack.FindSocket(transform, "katana_drawn"), Weapon.KatanaCarry, false);
            foreach (var limb in BladeLines.Limbs)
            {
                var bone = WeaponRack.FindSocket(transform, limb.Bone);
                var tip = WeaponRack.FindSocket(transform, limb.TipBone);
                if (bone == null || tip == null || sourceCount >= RestSlot) continue;
                sources[sourceCount++] = new Source
                {
                    At = bone, TipBone = tip, Limb = limb, IsLimb = true, Carry = BladeLines.BodyCarry, World = new Vector3[3], Fitted = true,
                };
            }
        }

        private bool Known(Transform part)
        {
            for (int i = 0; i < sourceCount; i++)
                if (sources[i].At == part) return true;
            return false;
        }

        private void AddFitted(BladeLine line, bool thrown)
        {
            if (line == null || sourceCount >= RestSlot || !line.Spec.Valid) return;
            sources[sourceCount++] = new Source { At = line.transform, Line = line.Spec, Carry = line.Carry, Thrown = thrown, Fitted = true, World = new Vector3[Mathf.Max(3, line.Count)] };
            SourcesFitted++;
        }

        // A part by itself: its own measured lines if the prefab carries them, else the notes' lines for its carry.
        private void AddPart(Transform part, string carry, bool thrown)
        {
            if (part == null || Known(part)) return;
            var fitted = part.GetComponents<BladeLine>();
            if (fitted.Length > 0)
            {
                foreach (var line in fitted) AddFitted(line, thrown);
                return;
            }
            for (int n = 0; BladeLines.TryGet(carry, n, out var line) && sourceCount < RestSlot; n++)
                sources[sourceCount++] = new Source { At = part, Line = line, Carry = carry, Thrown = thrown, World = new Vector3[Mathf.Max(3, line.Points.Length)] };
        }

        // ---- Each frame.

        private void LateUpdate()
        {
            float now = Time.time, deltaTime = Time.deltaTime;
            if (viewer == null && Time.frameCount % 60 == 0)
            {
                var main = Camera.main;
                viewer = main != null ? main.transform : null;
            }
            Vector3 view = viewer != null ? viewer.forward : Vector3.down;
            var weapon = combat.Weapon;
            if (weapon != shownWeapon)
            {
                shownWeapon = weapon;
                shape = SwingShapes.For(weapon);
                StopAll(now);
                LetThorns();
            }

            var step = combat.Combo.Current;
            float elapsed = combat.Combo.Elapsed;
            if (step != lastStep || (step != null && elapsed + 1e-4f < lastElapsed))
            {
                // A new move: the last one's trails live out their time, and this one starts its own.
                StopAll(now);
                lastStep = step;
                groundDone = false;
                landedThisSwing = 0;
                swingElement = step != null ? combat.SwingElement : null;
                swingInfused = step != null && combat.SwingInfused;
                if (swingElement != null && On)
                {
                    SwingsDrawn++;
                    glintFrom = now;
                    // An infused strike's element grows onto the weapon through its own wind-up.
                    if (swingInfused) chargedSince = now;
                    if (step.Shot != null)
                    {
                        thrownElement = swingElement;
                        thrownUntil = now + 1.6f;
                    }
                }
            }
            lastElapsed = elapsed;

            Gather(weapon, now, deltaTime);
            bool drawing = On && swingElement != null && step != null && combat.Combo.Phase != AttackPhase.Charge && InWindow(step, elapsed);
            if (drawing && deltaTime > 0f) Emit(swingElement, swingInfused ? InfusedWeight : 1f, false, view, now, deltaTime);
            else if (!drawing) StopEmitting(now, false);
            bool throwing = On && thrownElement != null && now < thrownUntil;
            if (throwing && deltaTime > 0f) Emit(thrownElement, 1f, true, view, now, deltaTime);
            else if (!throwing) StopEmitting(now, true);

            if (drawing && !groundDone && shape.GroundImpact && (step.Effect == StepEffect.GroundSlam || step.Radius > 0f) && elapsed >= step.Windup)
            {
                groundDone = true;
                Slam(step);
            }

            DrawRibbons(now);
            DrawSheaths(weapon, step, elapsed, view, now, deltaTime);
        }

        // From a little before the blow (the swing is already moving) to a little after its window; a finisher's beats throughout.
        private bool InWindow(AttackStep step, float elapsed)
        {
            float start = step.Beats != null && step.Beats.Length > 0 ? 0f : step.Windup * (1f - LeadShare);
            float end = step.Windup + step.Active + Mathf.Min(TailSeconds, step.Recovery * 0.5f);
            if (step.Repeats != null)
                for (int i = 0; i < step.Repeats.Length; i++) end = Mathf.Max(end, step.Windup + step.Repeats[i] + step.Active);
            return elapsed >= start && elapsed <= end;
        }

        // Which lines sweep with the weapon now, where they are, and how fast each tip is going.
        private void Gather(Weapon weapon, float now, float deltaTime)
        {
            activeCount = 0;
            for (int i = 0; i < sourceCount; i++)
            {
                ref var source = ref sources[i];
                bool shown = source.At != null && BladeLines.Belongs(source.Carry, weapon) && (source.IsLimb || source.At.gameObject.activeInHierarchy);
                if (!shown)
                {
                    source.HasLast = false;
                    continue;
                }
                Locate(ref source);
                Vector3 tip = source.World[source.WorldCount - 1];
                if (source.HasLast && deltaTime > 0f)
                {
                    Vector3 moved = tip - source.LastTip;
                    source.Speed = moved.magnitude / deltaTime;
                    if (moved.sqrMagnitude > 1e-8f) source.Way = moved.normalized;
                }
                else if (!source.HasLast)
                {
                    source.Speed = 0f;
                    source.Way = transform.forward;
                }
                if (deltaTime > 0f || !source.HasLast)
                {
                    source.LastTip = tip;
                    source.HasLast = true;
                }
                if (activeCount < MostActive) active[activeCount++] = i;
            }
            // The Katana waits in its sheath between attacks: a charge shows along the sheath.
            if (activeCount == 0 && weapon != null && weapon.UsesSword && animator != null && MeasureRest(animator.ShownWeapon))
            {
                Locate(ref sources[RestSlot]);
                active[activeCount++] = RestSlot;
            }
            SourcesActive = activeCount;
        }

        // A part with no blade line: the longest axis of its biggest piece, the end nearer the hand its base.
        private bool MeasureRest(GameObject part)
        {
            if (part == null || !part.activeInHierarchy) return false;
            if (part == measuredPart) return sources[RestSlot].At != null;
            measuredPart = part;
            sources[RestSlot] = default;
            Renderer biggest = null;
            float most = 0f;
            foreach (var piece in part.GetComponentsInChildren<Renderer>())
            {
                float size = piece.localBounds.size.sqrMagnitude * piece.transform.lossyScale.sqrMagnitude;
                if (size > most)
                {
                    most = size;
                    biggest = piece;
                }
            }
            if (biggest == null) return false;
            var bounds = biggest.localBounds;
            Vector3 extent = bounds.extents;
            Vector3 axis = extent.x >= extent.y && extent.x >= extent.z ? new Vector3(extent.x, 0f, 0f) : extent.y >= extent.z ? new Vector3(0f, extent.y, 0f) : new Vector3(0f, 0f, extent.z);
            Vector3 one = bounds.center - axis, other = bounds.center + axis;
            Vector3 hand = handRight != null ? handRight.position : transform.position;
            var at = biggest.transform;
            bool oneNearer = (at.TransformPoint(one) - hand).sqrMagnitude <= (at.TransformPoint(other) - hand).sqrMagnitude;
            sources[RestSlot] = new Source
            {
                At = at, Carry = Weapon.KatanaCarry, World = new Vector3[3],
                Line = new BladeLineSpec(Weapon.KatanaCarry, "sheath", BladeKind.Edge, Vector3.up, 0.03f, Vector3.Lerp(oneNearer ? one : other, oneNearer ? other : one, 0.25f), oneNearer ? other : one),
            };
            return true;
        }

        private static void Locate(ref Source source)
        {
            if (source.IsLimb)
            {
                Vector3 from = source.At.position, to = source.TipBone.position;
                Vector3 along = to - from;
                source.World[0] = from;
                source.World[1] = to + (along.sqrMagnitude > 1e-6f ? along.normalized * source.Limb.Beyond : Vector3.zero);
                source.WorldCount = 2;
                source.EdgeSide = Vector3.up;
                source.Length = BladeFit.Length(source.World, 2);
                return;
            }
            var points = source.Line.Points;
            int count = Mathf.Min(points.Length, source.World.Length);
            for (int k = 0; k < count; k++) source.World[k] = source.At.TransformPoint(points[k]);
            source.WorldCount = count;
            source.EdgeSide = source.At.TransformDirection(source.Line.EdgeSide);
            source.Length = BladeFit.Length(source.World, count);
        }

        private static bool Strand(in Source source) =>
            source.IsLimb || source.Thrown || source.Line.Kind == BladeKind.Point || source.Carry == "weight";

        private void Emit(ElementInfo element, float weight, bool thrown, Vector3 view, float now, float deltaTime)
        {
            var look = SwingLooks.For(element);
            if (look == null) return;
            Vector3 pivot = transform.position + Vector3.up * 1.1f;
            int fastest = -1;
            float fastestSpeed = 0f;
            for (int a = 0; a < activeCount; a++)
            {
                ref var source = ref sources[active[a]];
                if (source.Thrown != thrown || source.WorldCount < 2) continue;
                float gate = RibbonMath.Gate(source.Speed, shape.LeastSpeed, shape.FullSpeed);
                if (source.Speed > fastestSpeed)
                {
                    fastestSpeed = source.Speed;
                    fastest = active[a];
                }
                bool strand = Strand(source);
                Vector3 from = thrown ? source.World[source.WorldCount - 1] : pivot;
                for (int l = 0; l < look.Layers.Length && l < SwingLooks.MostLayers; l++)
                {
                    ref readonly var layer = ref look.Layers[l];
                    // A line that is not moving (the other fist, the knife in the hand that waits) takes no trail from the pool.
                    if (gate <= 0f && !Drawing(active[a], l)) continue;
                    int r = RibbonFor(l, look, element.Index, weight, active[a], now);
                    if (r < 0) continue;
                    Vector3 inner, outer;
                    if (layer.Form == TrailForm.Sweep && !strand)
                    {
                        inner = BladeFit.Along(source.World, source.WorldCount, Mathf.Lerp(shape.SweepFrom, shape.SweepTo, layer.From));
                        outer = BladeFit.Along(source.World, source.WorldCount, Mathf.Lerp(shape.SweepFrom, shape.SweepTo, layer.To));
                    }
                    else
                    {
                        // About one point, turned to the camera and across the way it moves.
                        float at = layer.Form == TrailForm.Strand ? layer.At : 1f;
                        float width = (layer.Form == TrailForm.Strand ? layer.Width * shape.Width : shape.StrandWidth * shape.Width * Mathf.Max(0.2f, layer.To - layer.From)) * weight;
                        Vector3 centre = BladeFit.Along(source.World, source.WorldCount, at);
                        Vector3 side = Vector3.Cross(source.Way, view);
                        if (side.sqrMagnitude < 1e-6f) side = Vector3.Cross(BladeFit.Tangent(source.World, source.WorldCount, 1f), view);
                        side = side.sqrMagnitude > 1e-8f ? side.normalized : Vector3.right;
                        inner = centre - side * (width * 0.5f);
                        outer = centre + side * (width * 0.5f);
                    }
                    ribbons[r].Push(thrown ? from : pivot, inner, outer, source.Way, now, gate, DegreesPerStep);
                }
            }
            if (thrown || look.Shed == SwingShed.None || fastest < 0) return;
            ref var lead = ref sources[fastest];
            if (RibbonMath.Gate(lead.Speed, shape.LeastSpeed, shape.FullSpeed) < 0.5f) return;
            shedLeft -= deltaTime;
            if (shedLeft > 0f) return;
            shedLeft = look.ShedEvery;
            SwingImpacts.Shed(look.Shed, element.Index, lead.World[lead.WorldCount - 1], lead.Way, transform.position.y, weight);
        }

        private bool Drawing(int source, int layer)
        {
            int r = ribbonFor[source * SwingLooks.MostLayers + layer];
            return r >= 0 && ribbons[r].Emitting && ribbonSource[r] == source;
        }

        // The trail one layer of one line is drawing in this swing, taken from the pool when it starts.
        private int RibbonFor(int layer, SwingLook look, int element, float weight, int source, float now)
        {
            int key = source * SwingLooks.MostLayers + layer;
            int r = ribbonFor[key];
            if (r >= 0 && ribbons[r].Emitting && ribbonSource[r] == source && ribbonElement[r] == element) return r;
            r = -1;
            for (int i = 0; i < ribbons.Length && r < 0; i++)
                if (!ribbons[i].Alive(now)) r = i;
            if (r < 0) return -1;
            ref readonly var trail = ref look.Layers[layer];
            float life = trail.Seconds * shape.Life * (weight > 1f ? 1.2f : 1f);
            ribbons[r].Begin(life, trail.Lag, trail.Hold);
            ribbonElement[r] = element;
            ribbonLayer[r] = layer;
            ribbonSource[r] = source;
            ribbonWeight[r] = weight;
            ribbonShown[r].sharedMaterial = layerMaterials[element * SwingLooks.MostLayers + layer];
            ribbonFor[key] = r;
            return r;
        }

        // The swing is over: its trails stop growing, and Water's throws its water off the end.
        private void StopEmitting(float now, bool thrown)
        {
            if (ribbons == null) return;
            for (int r = 0; r < ribbons.Length; r++)
            {
                var ribbon = ribbons[r];
                if (!ribbon.Emitting) continue;
                int source = ribbonSource[r];
                if (source < 0 || source >= sourceCount || sources[source].Thrown != thrown) continue;
                ribbon.Stop(now);
                var look = SwingLooks.For(ribbonElement[r]);
                if (look != null && look.End == SwingEnd.Spray && ribbonLayer[r] == 0 && ribbon.Count > 0 && !ribbon.Ended)
                {
                    ribbon.Ended = true;
                    ribbon.Get(ribbon.Count - 1, out _, out var outer, out var way, out _, out var strength);
                    if (strength > 0.3f) SwingImpacts.Spray(outer, way, transform.position.y, ribbonWeight[r], Elements.All[ribbonElement[r]].Colour);
                }
            }
        }

        // A new move: what the hands drew stops; what is in flight goes on.
        private void StopAll(float now)
        {
            StopEmitting(now, false);
            for (int i = 0; i < ribbonFor.Length; i++)
            {
                int source = i / SwingLooks.MostLayers;
                if (source >= sourceCount || !sources[source].Thrown) ribbonFor[i] = -1;
            }
        }

        // A heavy weapon's slam: the element thrown out where the lowest of its lines meets the ground.
        private void Slam(AttackStep step)
        {
            var element = swingElement;
            if (element == null || activeCount == 0) return;
            Vector3 lowest = transform.position + transform.forward * step.Reach;
            float lowestY = float.MaxValue;
            for (int a = 0; a < activeCount; a++)
            {
                ref var source = ref sources[active[a]];
                if (source.Thrown || source.WorldCount < 2) continue;
                Vector3 tip = source.World[source.WorldCount - 1];
                if (tip.y < lowestY)
                {
                    lowestY = tip.y;
                    lowest = tip;
                }
            }
            GroundBlows++;
            float radius = step.Radius > 0f ? step.Radius : 1.2f;
            SwingImpacts.Ground(element.Index, lowest, transform.forward, transform.position.y, radius, swingInfused ? InfusedWeight : 1f);
        }

        // Where a blow of the weapon landed (PlayerCombat's own hit test): the element's landing, a few to a swing.
        private void Met(Vector3 point, Vector3 way)
        {
            if (!On || !enabled) return;
            var element = combat.SwingElement;
            if (element == null || landedThisSwing >= LandingsPerSwing) return;
            landedThisSwing++;
            Landings++;
            SwingImpacts.Landing(element.Index, point, way, transform.position.y, combat.SwingInfused ? InfusedWeight : 1f);
        }

        private void DrawRibbons(float now)
        {
            int shown = 0;
            for (int r = 0; r < ribbons.Length; r++)
            {
                var ribbon = ribbons[r];
                if (!ribbon.Alive(now))
                {
                    if (ribbonShown[r].enabled) ribbonShown[r].enabled = false;
                    continue;
                }
                ribbon.Trim(now);
                var look = SwingLooks.For(ribbonElement[r]);
                if (look == null || ribbonLayer[r] >= look.Layers.Length)
                {
                    ribbon.Clear();
                    ribbonShown[r].enabled = false;
                    continue;
                }
                ref readonly var layer = ref look.Layers[ribbonLayer[r]];
                // Ice's frozen arc shatters once it has hung its time.
                if (look.End == SwingEnd.Shatter && ribbonLayer[r] == 0 && !ribbon.Emitting && !ribbon.Ended && now >= ribbon.Stopped + ribbon.Seconds * ribbon.Hold)
                {
                    ribbon.Ended = true;
                    Shatter(ribbon, Elements.All[ribbonElement[r]].Colour);
                }
                int count = ribbon.Build(layer, now, vertices, uvs, colours, out var bounds);
                if (count < 4)
                {
                    if (ribbonShown[r].enabled) ribbonShown[r].enabled = false;
                    continue;
                }
                Upload(ribbonMeshes[r], count, bounds, ribbonTriangles);
                if (!ribbonShown[r].enabled) ribbonShown[r].enabled = true;
                shown++;
            }
            RibbonsShown = shown;
        }

        private void Shatter(Ribbon ribbon, Color colour)
        {
            Color pale = ElementSubstance.Pale(colour);
            int shards = Mathf.Min(7, ribbon.Count);
            for (int k = 0; k < shards; k++)
            {
                ribbon.Get(Mathf.RoundToInt((k + 0.5f) * ribbon.Count / shards), out var inner, out var outer, out var way, out _, out var strength);
                if (strength < 0.2f) continue;
                Vector3 outward = (outer - inner).normalized * 0.6f + way * 0.4f;
                SwingImpacts.Shard(outer, outward.sqrMagnitude > 1e-6f ? outward.normalized : Vector3.up, transform.position.y, pale);
            }
        }

        private void Upload(Mesh mesh, int count, Bounds bounds, int[] triangles)
        {
            mesh.Clear(true);
            mesh.SetVertices(vertices, 0, count);
            mesh.SetUVs(0, uvs, 0, count);
            mesh.SetColors(colours, 0, count);
            mesh.SetTriangles(triangles, 0, (count / 2 - 1) * 6, 0, false);
            mesh.bounds = bounds;
        }

        // ---- The sheath between swings, Life's thorns, Metal's glint.

        private void DrawSheaths(Weapon weapon, AttackStep step, float elapsed, Vector3 view, float now, float deltaTime)
        {
            ElementInfo element = null;
            float strength = 0f;
            var blade = combat.Blade;
            if (blade.Charged)
            {
                element = Elements.ByName(blade.Element);
                strength = step != null && swingElement == element ? 1f : IdleSheath;
                if (blade.Version != chargedVersion)
                {
                    // A fresh charge grows in; a blow spent keeps what is there.
                    if (chargedVersion < 0 || blade.Length == blade.Left) chargedSince = now;
                    chargedVersion = blade.Version;
                    glintFrom = now;
                }
            }
            else chargedVersion = -1;
            if (step != null && swingInfused && swingElement != null)
            {
                // The infused strike's element gathers on the weapon through its wind-up.
                element = swingElement;
                strength = step.Windup > 0f ? Mathf.Clamp01(elapsed / step.Windup) : 1f;
            }
            var look = On && element != null ? SwingLooks.For(element) : null;
            int shown = 0;
            if (look != null && look.Sheath.Shown && sheathMaterials[look.Element] != null)
            {
                float glintAt = -1f;
                if (look.Sheath.Glint)
                {
                    float since = (now - glintFrom) % Mathf.Max(0.1f, look.Sheath.GlintEvery);
                    glintAt = since <= look.Sheath.GlintSeconds ? since / Mathf.Max(0.01f, look.Sheath.GlintSeconds) : -1f;
                }
                for (int a = 0; a < activeCount && shown < SheathPool; a++)
                {
                    ref var source = ref sources[active[a]];
                    if (source.Thrown || source.WorldCount < 2) continue;
                    var sheath = look.Sheath;
                    if (step != null) sheath.ToothRun *= 3f;
                    int count = Sheath.Build(source.World, source.WorldCount, source.EdgeSide, view, sheath, sheath.Width * shape.Width, strength, now, glintAt, vertices, uvs, colours, out var bounds);
                    if (count < 4) continue;
                    Upload(sheathMeshes[shown], count, bounds, sheathTriangles);
                    sheathShown[shown].sharedMaterial = sheathMaterials[look.Element];
                    if (!sheathShown[shown].enabled) sheathShown[shown].enabled = true;
                    shown++;
                }
                // Venom drips off the edge now and then.
                if (look.Element == ElementSubstance.Poison && activeCount > 0)
                {
                    dripLeft -= deltaTime;
                    if (dripLeft <= 0f)
                    {
                        dripLeft = 0.45f + 0.4f * Random.value;
                        ref var source = ref sources[active[Random.Range(0, activeCount)]];
                        if (!source.Thrown && source.WorldCount >= 2)
                            SwingImpacts.Drip(BladeFit.Along(source.World, source.WorldCount, Random.Range(0.3f, 1f)), Vector3.zero, transform.position.y, new Color(0.3f, 0.78f, 0.26f), 0.8f);
                    }
                }
            }
            for (int s = shown; s < SheathPool; s++)
                if (sheathShown[s].enabled) sheathShown[s].enabled = false;
            SheathsShown = shown;

            if (look != null && look.Thorns) Thorns(look, now);
            else LetThorns();
        }

        // Thorns along the real edge of each line in hand: grown in from the base when the charge lands, leaning back
        // along the blade as a briar's do, fewer and smaller as the blows are spent.
        private void Thorns(SwingLook look, float now)
        {
            int key = 0;
            for (int a = 0; a < activeCount; a++)
                if (!sources[active[a]].Thrown) key = key * 31 + active[a] + 1;
            if (key != thornedSourcesKey)
            {
                LetThorns();
                thornedSourcesKey = key;
                for (int a = 0; a < activeCount; a++)
                {
                    ref var source = ref sources[active[a]];
                    if (source.Thrown || source.WorldCount < 2) continue;
                    int count = Mathf.Clamp(Mathf.RoundToInt(source.Length / 0.11f), 3, 9);
                    for (int k = 0; k < count && thornCount < MostThorns; k++)
                    {
                        int handle = FormFx.Hold(FormMesh.Spike, SwingImpacts.ThornColour, 0.02f);
                        if (handle < 0) break;
                        thornHandle[thornCount] = handle;
                        thornSource[thornCount] = active[a];
                        thornShare[thornCount] = Mathf.Lerp(0.14f, 0.94f, count == 1 ? 0.5f : k / (float)(count - 1));
                        thornCount++;
                    }
                }
            }
            var blade = combat.Blade;
            float left = blade.Length > 0 ? blade.Left / (float)blade.Length : 1f;
            for (int t = 0; t < thornCount; t++)
            {
                ref var source = ref sources[thornSource[t]];
                if (source.WorldCount < 2) continue;
                float share = thornShare[t];
                Vector3 at = BladeFit.Along(source.World, source.WorldCount, share);
                Vector3 along = BladeFit.Tangent(source.World, source.WorldCount, share);
                Vector3 side = Vector3.ProjectOnPlane(source.EdgeSide, along);
                if (side.sqrMagnitude < 1e-6f) side = Vector3.ProjectOnPlane(Vector3.up, along);
                side.Normalize();
                // Leaning back toward the grip, and turned a little to one side and the other in turn.
                Vector3 point = Quaternion.AngleAxis(t % 2 == 0 ? 25f : -25f, along) * (side * 0.82f - along * 0.57f);
                float grown = Mathf.Clamp01((now - chargedSince - share * 0.3f) / 0.25f);
                grown = grown * grown * (3f - 2f * grown) * (0.65f + 0.35f * left);
                float length = Mathf.Clamp(source.IsLimb ? 0.07f : source.Line.Thickness * 0.5f + 0.05f, 0.05f, 0.15f) * shape.Width * grown;
                FormFx.Place(thornHandle[t], at, Quaternion.LookRotation(along, point), new Vector3(length * 0.42f, Mathf.Max(0.001f, length), length * 0.42f));
            }
            ThornsHeld = thornCount;
        }

        private void LetThorns()
        {
            for (int t = 0; t < thornCount; t++)
            {
                if (thornHandle[t] >= 0) FormFx.Release(thornHandle[t], 0.15f);
                thornHandle[t] = -1;
            }
            thornCount = 0;
            thornedSourcesKey = -1;
            ThornsHeld = 0;
        }
    }
}

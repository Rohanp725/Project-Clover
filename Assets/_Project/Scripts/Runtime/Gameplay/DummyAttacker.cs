using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// Makes a training dummy hit back, so guarding and dodging can be practised: it turns to the player, shows on the
    /// ground where its blow will land while it raises its arm, then strikes. Every few blows one is red and cannot be
    /// blocked. A perfect guard throws the blow back onto the dummy's own stagger meter.
    /// </summary>
    [RequireComponent(typeof(TrainingDummy))]
    public class DummyAttacker : MonoBehaviour
    {
        [SerializeField] private Transform pivot;
        [SerializeField] private Transform arm;
        [SerializeField] private Transform target;
        [SerializeField] private MeshFilter markerOutline;
        [SerializeField] private MeshFilter markerFill;

        public EnemyAttackCycle Cycle = new EnemyAttackCycle();
        public float Damage = 22f;
        /// <summary>Metres from the dummy's centre the blow reaches.</summary>
        public float Reach = 3f;
        public float ArcDegrees = 100f;
        /// <summary>The dummy starts an attack only when the player is within this many metres.</summary>
        public float ActivationRange = 5.5f;
        public float TurnDegreesPerSecond = 260f;
        /// <summary>The dummy stops following the player this far through the telegraph, so a late sidestep gets clear.</summary>
        public float AimLockAt = 0.6f;
        /// <summary>Stagger the dummy takes when its blow is perfectly guarded.</summary>
        public float PerfectGuardStagger = 40f;
        public float ArmRestDegrees = 75f;
        public float ArmRaisedDegrees = -75f;
        public float ArmStruckDegrees = 60f;
        public Color PlainColour = new Color(1f, 0.93f, 0.75f, 1f);
        public Color RedColour = new Color(1f, 0.08f, 0.05f, 1f);
        public float OutlineAlpha = 0.22f;
        public float FillAlpha = 0.6f;
        public LayerMask HitLayers = ~0;

        public int BlowsStruck { get; private set; }

        private static readonly int BaseColour = Shader.PropertyToID("_Color");

        private TrainingDummy dummy;
        private Transform targetRoot;
        private Renderer outlineRenderer;
        private Renderer fillRenderer;
        private MaterialPropertyBlock block;
        private Mesh sector;
        private readonly Collider[] overlaps = new Collider[32];
        private bool markerShown;
        private bool markerRed;
        private float armDegrees;

        public void Configure(Transform pivotToTurn, Transform armToSwing, Transform targetToStrike, MeshFilter outline, MeshFilter fill)
        {
            pivot = pivotToTurn;
            arm = armToSwing;
            target = targetToStrike;
            markerOutline = outline;
            markerFill = fill;
        }

        private void Awake()
        {
            dummy = GetComponent<TrainingDummy>();
            block = new MaterialPropertyBlock();
            if (pivot == null) pivot = transform;
            targetRoot = target != null ? target.root : null;
            sector = BuildSector(Reach, ArcDegrees);
            if (markerOutline != null)
            {
                markerOutline.sharedMesh = sector;
                outlineRenderer = markerOutline.GetComponent<Renderer>();
            }
            if (markerFill != null)
            {
                markerFill.sharedMesh = sector;
                fillRenderer = markerFill.GetComponent<Renderer>();
            }
            Cycle.Reset();
            ShowMarker(false, false);
            SetArm(ArmRestDegrees);
        }

        private void OnDestroy()
        {
            if (sector != null) Destroy(sector);
        }

        private void Update()
        {
            if (target == null) return;
            float deltaTime = Time.deltaTime;

            // A staggered dummy drops whatever it was doing.
            if (dummy.Stagger.IsStaggered)
            {
                Cycle.Reset();
                ShowMarker(false, false);
                SetArm(ArmRestDegrees);
                return;
            }

            Vector3 toTarget = target.position - pivot.position;
            toTarget.y = 0f;
            bool inRange = toTarget.sqrMagnitude <= ActivationRange * ActivationRange;
            bool following = Cycle.Phase == EnemyAttackPhase.Rest
                || (Cycle.Phase == EnemyAttackPhase.Telegraph && Cycle.TelegraphProgress < AimLockAt);
            if (following && inRange && toTarget.sqrMagnitude > 0.01f)
            {
                var facing = Quaternion.LookRotation(toTarget, Vector3.up);
                pivot.rotation = Quaternion.RotateTowards(pivot.rotation, facing, TurnDegreesPerSecond * deltaTime);
            }

            if (Cycle.Tick(deltaTime, inRange)) Strike();

            switch (Cycle.Phase)
            {
                case EnemyAttackPhase.Telegraph:
                    float shown = Cycle.TelegraphProgress;
                    ShowMarker(true, Cycle.IsRed);
                    if (markerFill != null) markerFill.transform.localScale = new Vector3(shown, 1f, shown);
                    SetArm(Mathf.Lerp(ArmRestDegrees, ArmRaisedDegrees, Mathf.SmoothStep(0f, 1f, shown * 1.6f)));
                    break;
                case EnemyAttackPhase.Strike:
                    ShowMarker(false, false);
                    SetArm(Mathf.Lerp(ArmRaisedDegrees, ArmStruckDegrees, Mathf.Clamp01(Cycle.StrikeProgress * 4f)));
                    break;
                default:
                    ShowMarker(false, false);
                    SetArm(Mathf.MoveTowards(armDegrees, ArmRestDegrees, 240f * deltaTime));
                    break;
            }
        }

        private void Strike()
        {
            BlowsStruck++;
            Vector3 origin = pivot.position + Vector3.up;
            Vector3 forward = pivot.forward;
            int count = Physics.OverlapSphereNonAlloc(origin, Reach, overlaps, HitLayers, QueryTriggerInteraction.Ignore);
            for (int i = 0; i < count; i++)
            {
                var other = overlaps[i];
                // The dummy fights the player only; it does not knock its neighbours about.
                if (other.transform.root != targetRoot) continue;
                var struck = other.GetComponentInParent<IDamageable>();
                if (struck == null) continue;
                Vector3 offset = other.ClosestPoint(origin) - origin;
                offset.y = 0f;
                if (offset.magnitude > 0.6f && Vector3.Angle(forward, offset) > ArcDegrees * 0.5f) continue;
                var outcome = struck.TakeHit(new Hit(Damage, 0f, forward, false, Cycle.IsRed, pivot.position));
                if (outcome == HitOutcome.PerfectGuard) dummy.Rebuff(PerfectGuardStagger, -forward);
                break;
            }
        }

        private void SetArm(float degrees)
        {
            armDegrees = degrees;
            if (arm != null) arm.localRotation = Quaternion.Euler(degrees, 0f, 0f);
        }

        private void ShowMarker(bool shown, bool red)
        {
            if (shown == markerShown && red == markerRed) return;
            markerShown = shown;
            markerRed = red;
            Color colour = red ? RedColour : PlainColour;
            Paint(outlineRenderer, shown, colour, OutlineAlpha);
            Paint(fillRenderer, shown, colour, FillAlpha);
        }

        private void Paint(Renderer markerRenderer, bool shown, Color colour, float alpha)
        {
            if (markerRenderer == null) return;
            markerRenderer.enabled = shown;
            if (!shown) return;
            colour.a = alpha;
            block.SetColor(BaseColour, colour);
            markerRenderer.SetPropertyBlock(block);
        }

        // A flat fan on the ground, pointing along +Z, facing up.
        private static Mesh BuildSector(float radius, float arcDegrees)
        {
            const int Segments = 24;
            var vertices = new Vector3[Segments + 2];
            var triangles = new int[Segments * 3];
            vertices[0] = Vector3.zero;
            for (int i = 0; i <= Segments; i++)
            {
                float angle = Mathf.Deg2Rad * Mathf.Lerp(-arcDegrees * 0.5f, arcDegrees * 0.5f, i / (float)Segments);
                vertices[i + 1] = new Vector3(Mathf.Sin(angle) * radius, 0f, Mathf.Cos(angle) * radius);
            }
            for (int i = 0; i < Segments; i++)
            {
                triangles[i * 3] = 0;
                triangles[i * 3 + 1] = i + 1;
                triangles[i * 3 + 2] = i + 2;
            }
            var mesh = new Mesh { name = "Attack Marker" };
            mesh.vertices = vertices;
            mesh.triangles = triangles;
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}

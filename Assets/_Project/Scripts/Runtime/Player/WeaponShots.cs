using System.Collections.Generic;
using Clover.Core;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// What weapons throw (<see cref="WeaponShot"/>): each flies straight from the blow, strikes what it passes once,
    /// stops at the first enemy unless it pierces, and may then hang where it stopped, turning, biting what stands in
    /// it; or stand stuck where it stopped until it is fetched or called back; or fly home to the hand, striking again.
    /// Drawn as a crescent of light in the held element's colour, or, for a weapon that is itself thrown, as that
    /// weapon's own model.
    /// </summary>
    public class WeaponShots : MonoBehaviour
    {
        [SerializeField] private Material material;
        [SerializeField] private string[] propNames = new string[0];
        [SerializeField] private GameObject[] propPrefabs = new GameObject[0];

        public int Capacity = 6;
        public float Height = 1.1f;
        /// <summary>Degrees a second a hanging shot turns.</summary>
        public float HangTurn = 620f;

        private static readonly int ColourId = Shader.PropertyToID("_Color");

        private struct Flying
        {
            public bool Live;
            public PlayerCombat Combat;
            public WeaponShot Shot;
            public Vector3 At, Direction;
            public float Flown, HangLeft, BiteIn, Age, Turn;
            public bool Hanging, Stuck, InEnemy, Returning, Harmless, Ray;
            // The thrower is told where this one stopped (the Great Bow's infused shot: its impact is drawn there).
            public bool Watched;
            public float StuckLeft, PauseLeft, ReturnSpeed;
            public int Prop;
            public Transform Owner;
            public SpellBlow Back;
            public Color Colour;
            public SpellBlow Blow, Bite;
        }

        private Flying[] flying;
        private HashSet<IDamageable>[] bitten;
        private Transform[] shown;
        private MeshRenderer[] renderers;
        private MaterialPropertyBlock block;
        private Mesh mesh;
        private Transform[] props;

        /// <summary>For probes: shots thrown, enemies struck in flight, and bites taken while hanging.</summary>
        public int Thrown { get; private set; }
        public int Struck { get; private set; }
        public int Bites { get; private set; }
        public int Hanging { get; private set; }

        /// <summary>Thrown weapons that came home to the hand, and ones that stuck.</summary>
        public int Returned { get; private set; }
        public int Sticking { get; private set; }
        /// <summary>Thrown weapons called home as a ray, and the enemies those rays struck.</summary>
        public int Rays { get; private set; }
        public int RayStruck { get; private set; }

        [SerializeField] private LineRenderer rayLine;
        private Vector3 rayFrom;
        private int rayOf = -1;
        private float rayFade = -1f;
        private readonly Vector3[] rayPoints = new Vector3[2];

        /// <summary>The line drawn while a thrown weapon comes home as a ray.</summary>
        public void ConfigureRay(LineRenderer line) => rayLine = line;

        public void Configure(Material shotMaterial) => material = shotMaterial;

        /// <summary>The weapons that can themselves be thrown: each name with the model that flies for it.</summary>
        public void ConfigureProps(string[] names, GameObject[] prefabs)
        {
            propNames = names;
            propPrefabs = prefabs;
        }

        /// <summary>True while the shot with this handle is anywhere in the world: flying, stuck, or coming back.</summary>
        /// <summary>The weapon models that fly when thrown, one per carry name, made when the player starts (null where none was).</summary>
        public int PropCount => props != null ? props.Length : 0;
        public Transform PropAt(int index) => props != null && index >= 0 && index < props.Length ? props[index] : null;
        public string PropName(int index) => propNames != null && index >= 0 && index < propNames.Length ? propNames[index] : "";

        /// <summary>The thrower is told where this shot stops (<see cref="PlayerCombat.ShotStopped"/>), once.</summary>
        public void Watch(int handle)
        {
            if (IsOut(handle)) flying[handle].Watched = true;
        }

        public bool IsOut(int handle) => flying != null && handle >= 0 && handle < flying.Length && flying[handle].Live;
        public bool IsStuck(int handle) => IsOut(handle) && flying[handle].Stuck;
        public bool StuckInEnemy(int handle) => IsStuck(handle) && flying[handle].InEnemy;
        public bool IsReturning(int handle) => IsOut(handle) && flying[handle].Returning;
        /// <summary>Where on the ground the shot is.</summary>
        public Vector3 Where(int handle) => IsOut(handle) ? flying[handle].At : transform.position;

        /// <summary>Calls a shot back to whoever threw it, to arrive in about this many seconds, harming nothing on the way.</summary>
        public void Recall(int handle, float seconds)
        {
            if (!IsOut(handle) || flying[handle].Returning) return;
            ref Flying f = ref flying[handle];
            Vector3 gap = (f.Owner != null ? f.Owner.position : transform.position) - f.At;
            gap.y = 0f;
            f.Stuck = false;
            f.Hanging = false;
            f.Returning = true;
            f.PauseLeft = 0f;
            // The Sun-Spear comes home as a ray, which strikes what it crosses; anything else is only fetched.
            f.Ray = f.Shot != null && f.Shot.RaysBack;
            f.Harmless = !f.Ray;
            if (f.Ray)
            {
                seconds = f.Shot.RaySeconds;
                bitten[handle].Clear();
                Rays++;
                rayOf = handle;
                rayFrom = f.At;
                rayFade = -1f;
                Vfx.Play(VfxKind.Impact, f.At + Vector3.up * Height, Vector3.up, 1f, new Color(1f, 0.92f, 0.62f, 1f));
            }
            f.ReturnSpeed = gap.magnitude / Mathf.Max(0.05f, seconds);
        }

        /// <summary>Takes a shot out of the world at once: the thrown weapon is in the hand again.</summary>
        public void Release(int handle)
        {
            if (IsOut(handle)) End(handle);
        }

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            mesh = CrescentMesh();
            flying = new Flying[Capacity];
            bitten = new HashSet<IDamageable>[Capacity];
            shown = new Transform[Capacity];
            renderers = new MeshRenderer[Capacity];
            for (int i = 0; i < Capacity; i++)
            {
                bitten[i] = new HashSet<IDamageable>();
                var shot = new GameObject("Weapon Shot");
                shown[i] = shot.transform;
                shot.AddComponent<MeshFilter>().sharedMesh = mesh;
                renderers[i] = shot.AddComponent<MeshRenderer>();
                renderers[i].sharedMaterial = material;
                renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderers[i].receiveShadows = false;
                renderers[i].enabled = false;
            }
            props = new Transform[propPrefabs != null ? propPrefabs.Length : 0];
            for (int p = 0; p < props.Length; p++)
            {
                if (propPrefabs[p] == null) continue;
                var prop = Instantiate(propPrefabs[p]);
                prop.name = "Thrown " + propNames[p];
                foreach (var collider in prop.GetComponentsInChildren<Collider>()) Destroy(collider);
                prop.SetActive(false);
                props[p] = prop.transform;
            }
        }

        private int PropFor(string name)
        {
            if (string.IsNullOrEmpty(name) || props == null) return -1;
            for (int p = 0; p < props.Length; p++)
                if (propNames[p] == name && props[p] != null) return p;
            return -1;
        }

        private void OnDestroy()
        {
            if (mesh != null) Destroy(mesh);
            if (shown == null) return;
            foreach (var shot in shown)
                if (shot != null) Destroy(shot.gameObject);
            if (props == null) return;
            foreach (var prop in props)
                if (prop != null) Destroy(prop.gameObject);
        }

        /// <summary>
        /// Throws the shot (or its whole fan) from a point along a direction, in a colour, carrying the held element's
        /// affliction if there is one. Returns a handle to the last one thrown, or -1 when nothing left.
        /// </summary>
        public int Fire(PlayerCombat combat, WeaponShot shot, Vector3 from, Vector3 direction, Color colour, ElementInfo held, Color struck = default)
        {
            if (flying == null || shot == null) return -1;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return -1;
            direction.Normalize();
            int count = Mathf.Max(1, shot.Count);
            int handle = -1;
            for (int n = 0; n < count; n++)
            {
                int i = Free();
                if (i < 0) return handle;
                handle = i;
                float angle = count > 1 ? Mathf.Lerp(-shot.SpreadDegrees * 0.5f, shot.SpreadDegrees * 0.5f, n / (float)(count - 1)) : 0f;
                bitten[i].Clear();
                // A shot loosed from a weapon carrying an element marks what it strikes in that element's softened tint (Hit.Tint).
                // Its sparks are thrown in the colour's hue made deep and held low: thrown in the colour itself, their
                // cores lit a pure-white star at every arrow (the bow's films of 2026-11-22).
                Color spark = StruckLook.Spark(colour, PlayerCombat.ImpactGlow);
                var blow = new SpellBlow { Damage = shot.Damage, Stagger = shot.Stagger, Heavy = shot.Heavy, Effect = VfxKind.TintSpark, Tinted = true, Tint = spark, Struck = struck };
                var bite = new SpellBlow
                {
                    Damage = shot.HangBite, Stagger = shot.HangBite, Effect = VfxKind.TintSpark, Tinted = true, Tint = spark, Struck = struck,
                    // What hangs lays the held element on what it bites.
                    Affliction = held != null ? held.Affliction : AfflictionKind.None, Stacks = held != null && held.Affliction != AfflictionKind.None ? 1 : 0,
                };
                flying[i] = new Flying
                {
                    Live = true, Combat = combat, Shot = shot, At = from, Direction = Quaternion.AngleAxis(angle, Vector3.up) * direction,
                    Colour = new Color(colour.r * 2.2f, colour.g * 2.2f, colour.b * 2.2f, 0.95f), Blow = blow, Bite = bite,
                    Prop = PropFor(shot.Prop), Owner = combat != null ? combat.transform : transform, ReturnSpeed = shot.ReturnSpeed,
                    Back = new SpellBlow { Damage = shot.Damage * shot.ReturnScale, Stagger = shot.Stagger * shot.ReturnScale, Heavy = shot.Heavy, Effect = VfxKind.TintSpark, Tinted = true, Tint = spark, Struck = struck },
                };
                // A weapon that is itself thrown is drawn as itself, one at a time.
                if (flying[i].Prop >= 0) props[flying[i].Prop].gameObject.SetActive(true);
                renderers[i].enabled = flying[i].Prop < 0;
                Thrown++;
            }
            return handle;
        }

        private int Free()
        {
            for (int i = 0; i < flying.Length; i++)
                if (!flying[i].Live) return i;
            return -1;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            int hanging = 0;
            for (int i = 0; i < flying.Length; i++)
            {
                if (!flying[i].Live) continue;
                ref Flying f = ref flying[i];
                var shot = f.Shot;
                f.Age += deltaTime;
                float alpha = 1f;
                if (f.Stuck)
                {
                    // Standing where it stopped. Left too long, it tears itself loose and comes home.
                    f.StuckLeft -= deltaTime;
                    if (f.StuckLeft <= 0f) Recall(i, 0.5f);
                }
                else if (f.Returning)
                {
                    if (f.PauseLeft > 0f) f.PauseLeft -= deltaTime;
                    else
                    {
                        Vector3 home = f.Owner != null ? f.Owner.position : transform.position;
                        Vector3 gap = home - f.At;
                        gap.y = 0f;
                        float distance = gap.magnitude;
                        float back = f.ReturnSpeed * deltaTime;
                        bool arrived = distance <= Mathf.Max(0.7f, back);
                        f.Direction = distance > 0.001f ? gap / distance : f.Direction;
                        Vector3 start = f.At;
                        float went = arrived ? distance : back;
                        f.At += f.Direction * went;
                        if (!f.Harmless && f.Combat != null)
                        {
                            if (f.Ray)
                            {
                                // A ray is as fast as light: walked in strides no longer than its own width, so nothing on the line is passed over.
                                float stride = Mathf.Max(0.3f, shot.RayRadius);
                                for (float walked = 0f; walked < went + stride; walked += stride)
                                {
                                    int now = f.Combat.StrikeArea(start + f.Direction * Mathf.Min(walked, went), shot.RayRadius, f.Back, bitten[i]);
                                    Struck += now;
                                    RayStruck += now;
                                }
                            }
                            else if (!arrived) Struck += f.Combat.StrikeArea(f.At, shot.Radius, f.Back, bitten[i]);
                        }
                        if (arrived)
                        {
                            Returned++;
                            if (f.Ray)
                            {
                                rayFade = 0.12f;
                                Vfx.Play(VfxKind.Clang, HandPoint(f), Vector3.up, 1f);
                            }
                            End(i);
                            continue;
                        }
                    }
                }
                else if (!f.Hanging)
                {
                    // A fast shot on a slow frame covers more ground than it is wide: it is walked there in strides
                    // no longer than its own radius, so nothing standing in its way is stepped over.
                    float left = Mathf.Min(shot.Speed * deltaTime, shot.Range - f.Flown);
                    float stride = Mathf.Max(0.2f, shot.Radius);
                    var planted = shot.Plants && f.Combat != null ? f.Combat.Arrows : null;
                    int hit = 0;
                    do
                    {
                        float step = Mathf.Min(left, stride);
                        left -= step;
                        f.At += f.Direction * step;
                        f.Flown += step;
                        int now = f.Combat != null ? f.Combat.StrikeArea(f.At, shot.Radius, f.Blow, bitten[i]) : 0;
                        hit += now;
                        if (now == 0) continue;
                        if (shot.ManaGain > 0f) f.Combat.Mana.Gain(shot.ManaGain * now);
                        if (planted == null) continue;
                        // An arrow stays in each body it struck.
                        foreach (var body in f.Combat.LastStruck)
                            if (body is Component host) planted.Plant(host, f.Direction);
                    }
                    while (left > 0.0001f && (hit == 0 || shot.Pierce));
                    Struck += hit;
                    if (f.Flown >= shot.Range - 0.001f || (hit > 0 && !shot.Pierce))
                    {
                        if (f.Watched && f.Combat != null)
                        {
                            f.Watched = false;
                            f.Combat.ShotStopped(f.At, f.Direction, hit > 0 && !shot.Pierce, f.Flown);
                        }
                        if (shot.Plants)
                        {
                            // One that stopped in no one stands in the ground where its flight ended.
                            if (planted != null && (hit == 0 || shot.Pierce)) planted.PlantGround(f.At, f.Direction);
                            End(i);
                        }
                        else if (shot.Sticks)
                        {
                            f.Stuck = true;
                            f.InEnemy = hit > 0;
                            // It struck from its own radius short of the body: the point goes on into it.
                            if (f.InEnemy) f.At += f.Direction * shot.Radius;
                            f.StuckLeft = shot.StickSeconds;
                            Sticking++;
                            Vfx.Play(f.InEnemy ? VfxKind.HeavySpark : VfxKind.Dust, f.At + Vector3.up * (f.InEnemy ? 1.1f : 0.1f), -f.Direction, 1.4f);
                        }
                        else if (shot.Returns)
                        {
                            // It hangs for an instant at the far end, then comes back through whatever is in the way.
                            f.Returning = true;
                            f.PauseLeft = 0.08f;
                            bitten[i].Clear();
                        }
                        else if (shot.HangSeconds > 0f)
                        {
                            f.Hanging = true;
                            f.HangLeft = shot.HangSeconds;
                            f.BiteIn = shot.HangEvery;
                            Vfx.Play(VfxKind.TintRing, f.At + Vector3.up * 0.2f, Vector3.up, shot.Radius * 0.6f, f.Colour);
                        }
                        else End(i);
                    }
                }
                else
                {
                    hanging++;
                    f.HangLeft -= deltaTime;
                    f.BiteIn -= deltaTime;
                    f.Turn += HangTurn * deltaTime;
                    if (f.BiteIn <= 0f)
                    {
                        f.BiteIn += Mathf.Max(0.05f, shot.HangEvery);
                        bitten[i].Clear();
                        if (f.Combat != null && shot.HangBite > 0f) Bites += f.Combat.StrikeArea(f.At, shot.Radius, f.Bite, bitten[i]);
                    }
                    // It thins away over its last third of a second.
                    alpha = Mathf.Clamp01(f.HangLeft / 0.3f);
                    if (f.HangLeft <= 0f)
                    {
                        End(i);
                        continue;
                    }
                }
                if (!f.Live) continue;
                if (f.Prop >= 0)
                {
                    // The weapon itself: point first in flight, slanted where it stands, end over end if it tumbles.
                    Vector3 across = Vector3.Cross(Vector3.up, f.Direction);
                    Quaternion turn;
                    Vector3 lead;
                    if (f.Stuck)
                    {
                        turn = Quaternion.LookRotation(Quaternion.AngleAxis(f.InEnemy ? 10f : 40f, across) * f.Direction, Vector3.up);
                        lead = f.At + Vector3.up * (f.InEnemy ? 1.15f : -0.12f);
                    }
                    else
                    {
                        Vector3 nose = f.Returning && f.Harmless ? -f.Direction : f.Direction;
                        turn = shot.Tumble != 0f ? Quaternion.LookRotation(f.Direction, Vector3.up) * Quaternion.Euler(f.Age * shot.Tumble, 0f, 0f) : Quaternion.LookRotation(nose, Vector3.up);
                        lead = f.At + Vector3.up * Height;
                    }
                    props[f.Prop].SetPositionAndRotation(lead - turn * Vector3.forward * shot.PropLength, turn);
                    continue;
                }
                shown[i].SetPositionAndRotation(f.At + Vector3.up * Height, Quaternion.LookRotation(f.Direction, Vector3.up) * Quaternion.Euler(0f, f.Turn, 0f));
                float size = shot.Radius * (f.Hanging ? 1f + 0.06f * Mathf.Sin(f.Age * 20f) : Mathf.Min(1f, 0.4f + f.Age * 6f));
                shown[i].localScale = new Vector3(size, 1f, size);
                var colour = f.Colour;
                colour.a *= alpha;
                block.SetColor(ColourId, colour);
                renderers[i].SetPropertyBlock(block);
            }
            Hanging = hanging;
            DrawRay(deltaTime);
        }

        private Vector3 HandPoint(in Flying f) => (f.Owner != null ? f.Owner.position : transform.position) + Vector3.up * Height;

        // The ray from where the spear stood to the hand, the spear riding it home; it lingers a moment after the catch.
        private void DrawRay(float deltaTime)
        {
            if (rayLine == null) return;
            bool riding = rayOf >= 0 && flying[rayOf].Live && flying[rayOf].Ray;
            if (!riding && rayFade < 0f)
            {
                if (rayLine.enabled) rayLine.enabled = false;
                rayOf = -1;
                return;
            }
            float strength = 1f;
            if (!riding)
            {
                rayFade -= deltaTime;
                strength = Mathf.Clamp01(rayFade / 0.12f);
                if (rayFade <= 0f)
                {
                    rayFade = -1f;
                    rayLine.enabled = false;
                    rayOf = -1;
                    return;
                }
            }
            Transform owner = rayOf >= 0 && flying[rayOf].Owner != null ? flying[rayOf].Owner : transform;
            rayPoints[0] = rayFrom + Vector3.up * Height;
            rayPoints[1] = owner.position + Vector3.up * Height;
            rayLine.positionCount = 2;
            rayLine.SetPositions(rayPoints);
            rayLine.widthMultiplier = 0.9f * (0.85f + 0.15f * Mathf.Sin(Time.time * 90f));
            var tint = new Color(1f, 1f, 1f, strength);
            rayLine.startColor = tint;
            rayLine.endColor = tint;
            if (!rayLine.enabled) rayLine.enabled = true;
        }

        private void End(int i)
        {
            if (i == rayOf && rayFade < 0f) rayFade = 0.12f;
            flying[i].Live = false;
            renderers[i].enabled = false;
            if (flying[i].Prop >= 0 && props != null && props[flying[i].Prop] != null) props[flying[i].Prop].gameObject.SetActive(false);
        }

        // A crescent lying flat, one metre in radius, bulging toward +Z: thick in the middle, pointed at both horns.
        // u runs along it and v from the inner edge to the outer, as the slash material expects.
        private static Mesh CrescentMesh()
        {
            const int Segments = 24;
            const float Degrees = 190f;
            var vertices = new Vector3[(Segments + 1) * 2];
            var uvs = new Vector2[vertices.Length];
            var triangles = new int[Segments * 6];
            for (int s = 0; s <= Segments; s++)
            {
                float t = s / (float)Segments;
                float angle = Mathf.Deg2Rad * Mathf.Lerp(Degrees * 0.5f, -Degrees * 0.5f, t);
                var direction = new Vector3(Mathf.Sin(angle), 0f, Mathf.Cos(angle));
                float thick = 0.42f * Mathf.Sin(t * Mathf.PI);
                vertices[s * 2] = direction * (1f - thick) - Vector3.forward * 0.25f;
                vertices[s * 2 + 1] = direction - Vector3.forward * 0.25f;
                uvs[s * 2] = new Vector2(t, 0f);
                uvs[s * 2 + 1] = new Vector2(t, 1f);
            }
            for (int s = 0; s < Segments; s++)
            {
                int v = s * 2;
                triangles[s * 6] = v;
                triangles[s * 6 + 1] = v + 1;
                triangles[s * 6 + 2] = v + 2;
                triangles[s * 6 + 3] = v + 2;
                triangles[s * 6 + 4] = v + 1;
                triangles[s * 6 + 5] = v + 3;
            }
            var mesh = new Mesh { name = "Weapon Shot Crescent" };
            mesh.vertices = vertices;
            mesh.uv = uvs;
            mesh.triangles = triangles;
            mesh.RecalculateNormals();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}

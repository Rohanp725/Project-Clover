using System.Collections.Generic;
using Clover.Core;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// What weapons throw (<see cref="WeaponShot"/>): each flies straight from the blow, strikes what it passes once,
    /// stops at the first enemy unless it pierces, and may then hang where it stopped, turning, biting what stands in
    /// it. Drawn as a crescent of light in the held element's colour. The hammer's return and the arrows that stick
    /// are fields on the shot that this does not read yet.
    /// </summary>
    public class WeaponShots : MonoBehaviour
    {
        [SerializeField] private Material material;

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
            public bool Hanging;
            public Color Colour;
            public SpellBlow Blow, Bite;
        }

        private Flying[] flying;
        private HashSet<IDamageable>[] bitten;
        private Transform[] shown;
        private MeshRenderer[] renderers;
        private MaterialPropertyBlock block;
        private Mesh mesh;

        /// <summary>For probes: shots thrown, enemies struck in flight, and bites taken while hanging.</summary>
        public int Thrown { get; private set; }
        public int Struck { get; private set; }
        public int Bites { get; private set; }
        public int Hanging { get; private set; }

        public void Configure(Material shotMaterial) => material = shotMaterial;

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
        }

        private void OnDestroy()
        {
            if (mesh != null) Destroy(mesh);
            if (shown == null) return;
            foreach (var shot in shown)
                if (shot != null) Destroy(shot.gameObject);
        }

        /// <summary>Throws the shot (or its whole fan) from a point along a direction, in a colour, carrying the held element's affliction if there is one.</summary>
        public void Fire(PlayerCombat combat, WeaponShot shot, Vector3 from, Vector3 direction, Color colour, ElementInfo held)
        {
            if (flying == null || shot == null) return;
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;
            direction.Normalize();
            int count = Mathf.Max(1, shot.Count);
            for (int n = 0; n < count; n++)
            {
                int i = Free();
                if (i < 0) return;
                float angle = count > 1 ? Mathf.Lerp(-shot.SpreadDegrees * 0.5f, shot.SpreadDegrees * 0.5f, n / (float)(count - 1)) : 0f;
                bitten[i].Clear();
                var blow = new SpellBlow { Damage = shot.Damage, Stagger = shot.Stagger, Heavy = shot.Heavy, Effect = VfxKind.TintSpark, Tinted = true, Tint = colour };
                var bite = new SpellBlow
                {
                    Damage = shot.HangBite, Stagger = shot.HangBite, Effect = VfxKind.TintSpark, Tinted = true, Tint = colour,
                    // What hangs lays the held element on what it bites.
                    Affliction = held != null ? held.Affliction : AfflictionKind.None, Stacks = held != null && held.Affliction != AfflictionKind.None ? 1 : 0,
                };
                flying[i] = new Flying
                {
                    Live = true, Combat = combat, Shot = shot, At = from, Direction = Quaternion.AngleAxis(angle, Vector3.up) * direction,
                    Colour = new Color(colour.r * 2.2f, colour.g * 2.2f, colour.b * 2.2f, 0.95f), Blow = blow, Bite = bite,
                };
                renderers[i].enabled = true;
                Thrown++;
            }
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
                if (!f.Hanging)
                {
                    float step = Mathf.Min(shot.Speed * deltaTime, shot.Range - f.Flown);
                    f.At += f.Direction * step;
                    f.Flown += step;
                    int hit = f.Combat != null ? f.Combat.StrikeArea(f.At, shot.Radius, f.Blow, bitten[i]) : 0;
                    Struck += hit;
                    if (f.Flown >= shot.Range - 0.001f || (hit > 0 && !shot.Pierce))
                    {
                        if (shot.HangSeconds > 0f)
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
                shown[i].SetPositionAndRotation(f.At + Vector3.up * Height, Quaternion.LookRotation(f.Direction, Vector3.up) * Quaternion.Euler(0f, f.Turn, 0f));
                float size = shot.Radius * (f.Hanging ? 1f + 0.06f * Mathf.Sin(f.Age * 20f) : Mathf.Min(1f, 0.4f + f.Age * 6f));
                shown[i].localScale = new Vector3(size, 1f, size);
                var colour = f.Colour;
                colour.a *= alpha;
                block.SetColor(ColourId, colour);
                renderers[i].SetPropertyBlock(block);
            }
            Hanging = hanging;
        }

        private void End(int i)
        {
            flying[i].Live = false;
            renderers[i].enabled = false;
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

using System.Collections.Generic;
using Clover.Core;
using Clover.Gameplay;
using UnityEngine;

namespace Clover.Player
{
    /// <summary>
    /// Spikes that break out of the ground in a line running away from the player: the Massive Sword's Elemental Spike
    /// Line, and the split that follows its finisher. Each spike strikes once as it comes up (when the run strikes at
    /// all), one list of who has been bitten is shared along the whole line, and the spikes stand a second and sink.
    /// </summary>
    public class WeaponSpikes : MonoBehaviour
    {
        [SerializeField] private Material material;

        public int Capacity = 20;
        public float RiseSeconds = 0.07f;
        public float StandSeconds = 1f;
        public float SinkSeconds = 0.25f;
        public float Height = 1.7f;
        public float Width = 0.55f;
        /// <summary>Metres to either side of the line a spike strikes.</summary>
        public float HalfWidth = 0.8f;

        private static readonly int BaseColour = Shader.PropertyToID("_BaseColor");
        private static readonly int Emission = Shader.PropertyToID("_EmissionColor");

        private struct Line
        {
            public bool Live;
            public PlayerCombat Combat;
            public Vector3 From, Direction;
            public int Count, Next;
            public float Spacing, Every, Wait;
            public Color Colour;
            public bool Strikes;
            public SpellBlow Blow;
        }

        private readonly Line[] lines = new Line[3];
        private readonly HashSet<IDamageable>[] bitten = { new HashSet<IDamageable>(), new HashSet<IDamageable>(), new HashSet<IDamageable>() };
        private Transform[] spikes;
        private MeshRenderer[] renderers;
        private float[] ages;
        private float[] sizes;
        private MaterialPropertyBlock block;
        private Mesh mesh;
        private int nextSpike;

        /// <summary>How many spikes have come up, and how many enemies they struck, since the scene began: for probes.</summary>
        public int Raised { get; private set; }
        public int Struck { get; private set; }

        public void Configure(Material spikeMaterial) => material = spikeMaterial;

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            mesh = SpikeMesh();
            spikes = new Transform[Capacity];
            renderers = new MeshRenderer[Capacity];
            ages = new float[Capacity];
            sizes = new float[Capacity];
            for (int i = 0; i < Capacity; i++)
            {
                // They live in the world, not under the player, so they stay where they broke out.
                var spike = new GameObject("Weapon Spike");
                spikes[i] = spike.transform;
                spike.AddComponent<MeshFilter>().sharedMesh = mesh;
                renderers[i] = spike.AddComponent<MeshRenderer>();
                renderers[i].sharedMaterial = material;
                renderers[i].enabled = false;
                ages[i] = float.MaxValue;
            }
        }

        private void OnDestroy()
        {
            if (mesh != null) Destroy(mesh);
            if (spikes == null) return;
            foreach (var spike in spikes)
                if (spike != null) Destroy(spike.gameObject);
        }

        /// <summary>
        /// Starts a line: <paramref name="count"/> spikes from a point along a direction, <paramref name="spacing"/>
        /// metres apart, one every <paramref name="every"/> seconds after <paramref name="wait"/>.
        /// </summary>
        public void Run(PlayerCombat combat, Vector3 from, Vector3 direction, int count, float spacing, float every, float wait,
            Color colour, bool strikes, SpellBlow blow)
        {
            direction.y = 0f;
            if (direction.sqrMagnitude < 0.0001f) return;
            for (int i = 0; i < lines.Length; i++)
            {
                if (lines[i].Live) continue;
                bitten[i].Clear();
                lines[i] = new Line
                {
                    Live = true, Combat = combat, From = from, Direction = direction.normalized, Count = count, Next = 0,
                    Spacing = spacing, Every = every, Wait = wait, Colour = colour, Strikes = strikes, Blow = blow,
                };
                return;
            }
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            for (int i = 0; i < lines.Length; i++)
            {
                if (!lines[i].Live) continue;
                lines[i].Wait -= deltaTime;
                while (lines[i].Live && lines[i].Wait <= 0f)
                {
                    Raise(i);
                    lines[i].Wait += Mathf.Max(0.01f, lines[i].Every);
                    if (++lines[i].Next >= lines[i].Count) lines[i].Live = false;
                }
            }

            float whole = RiseSeconds + StandSeconds + SinkSeconds;
            for (int i = 0; i < spikes.Length; i++)
            {
                if (ages[i] > whole) continue;
                ages[i] += deltaTime;
                if (ages[i] > whole)
                {
                    renderers[i].enabled = false;
                    continue;
                }
                // Up fast and a little past, standing, then down.
                float up = ages[i] < RiseSeconds ? ages[i] / RiseSeconds
                    : ages[i] < RiseSeconds + StandSeconds ? 1f + 0.12f * Mathf.Exp(-(ages[i] - RiseSeconds) * 14f)
                    : 1f - (ages[i] - RiseSeconds - StandSeconds) / SinkSeconds;
                spikes[i].localScale = new Vector3(Width * sizes[i], Height * sizes[i] * Mathf.Max(0.01f, up), Width * sizes[i]);
            }
        }

        private void Raise(int lineIndex)
        {
            ref Line line = ref lines[lineIndex];
            Vector3 at = line.From + line.Direction * (line.Spacing * line.Next);
            Vector3 side = new Vector3(-line.Direction.z, 0f, line.Direction.x);
            // Each leans a little its own way and the line grows as it goes, so it reads as breaking rock and not a fence.
            float lean = (line.Next % 2 == 0 ? 1f : -1f) * (8f + 5f * (line.Next % 3));
            int i = nextSpike;
            nextSpike = (nextSpike + 1) % spikes.Length;
            sizes[i] = 0.8f + 0.4f * (line.Next / (float)Mathf.Max(1, line.Count - 1)) + 0.15f * (line.Next % 2);
            spikes[i].SetPositionAndRotation(at + side * (0.18f * (line.Next % 2 == 0 ? 1f : -1f)),
                Quaternion.AngleAxis(lean, line.Direction) * Quaternion.AngleAxis(-10f, side) * Quaternion.LookRotation(line.Direction, Vector3.up) * Quaternion.Euler(0f, 37f * line.Next, 0f));
            spikes[i].localScale = new Vector3(Width, 0.01f, Width);
            ages[i] = 0f;
            block.SetColor(BaseColour, Color.Lerp(line.Colour, Color.white, 0.25f));
            block.SetColor(Emission, line.Colour * 0.9f);
            renderers[i].SetPropertyBlock(block);
            renderers[i].enabled = true;
            Raised++;
            Vfx.Play(VfxKind.Dust, at, Vector3.up, 1.2f);
            Vfx.Play(VfxKind.TintSpark, at + Vector3.up * 0.4f, Vector3.up, 0.7f, line.Colour);
            if (line.Strikes && line.Combat != null) Struck += line.Combat.StrikeArea(at, HalfWidth, line.Blow, bitten[lineIndex]);
        }

        // A four-sided spike one metre tall on a base one metre across, its foot at the origin.
        private static Mesh SpikeMesh()
        {
            var tip = new Vector3(0f, 1f, 0f);
            var corners = new[] { new Vector3(-0.5f, 0f, -0.5f), new Vector3(0.5f, 0f, -0.5f), new Vector3(0.5f, 0f, 0.5f), new Vector3(-0.5f, 0f, 0.5f) };
            var vertices = new Vector3[12];
            var normals = new Vector3[12];
            var triangles = new int[12];
            for (int side = 0; side < 4; side++)
            {
                Vector3 a = corners[side], b = corners[(side + 1) % 4];
                Vector3 normal = Vector3.Cross(tip - a, b - a).normalized;
                vertices[side * 3] = a;
                vertices[side * 3 + 1] = tip;
                vertices[side * 3 + 2] = b;
                for (int k = 0; k < 3; k++)
                {
                    normals[side * 3 + k] = normal;
                    triangles[side * 3 + k] = side * 3 + k;
                }
            }
            var mesh = new Mesh { name = "Weapon Spike" };
            mesh.vertices = vertices;
            mesh.normals = normals;
            mesh.triangles = triangles;
            mesh.RecalculateTangents();
            mesh.RecalculateBounds();
            return mesh;
        }
    }
}

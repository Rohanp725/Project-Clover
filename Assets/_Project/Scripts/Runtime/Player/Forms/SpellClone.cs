using UnityEngine;

namespace Clover.Player.Forms
{
    /// <summary>
    /// A copy of the player made of the element's light, standing beside them and doing what they did a beat ago. The
    /// body is copied every frame into the next of a ring of shapes, and the one shown is the oldest of the ring, so
    /// the copy trails the player by the ring's length in frames. The shapes are made once, when the scene starts.
    /// </summary>
    public class SpellClone : MonoBehaviour
    {
        /// <summary>How many frames the copy trails the player by.</summary>
        public int Frames = 12;
        /// <summary>Where it stands, in the player's own space: beside the right shoulder and a little behind.</summary>
        public Vector3 Offset = new Vector3(1.25f, 0f, -0.55f);

        public bool Showing { get; private set; }
        /// <summary>Where the copy stands now.</summary>
        public Vector3 Position { get; private set; }
        public Vector3 Forward { get; private set; }

        private static readonly int ColourId = Shader.PropertyToID("_Color");

        private SkinnedMeshRenderer[] bodies;
        private Material material;
        private Mesh[][] shapes;
        private Vector3[][] places;
        private Quaternion[][] turns;
        private Vector3[] stood;
        private Vector3[] faced;
        private MeshFilter[] filters;
        private MeshRenderer[] renderers;
        private MaterialPropertyBlock block;
        private Transform owner;
        private int next, filled;
        private Color colour;

        public void Configure(Afterimage image)
        {
            bodies = image != null ? image.Bodies : null;
            material = image != null ? image.Material : null;
            owner = image != null ? image.transform : null;
        }

        private void Start()
        {
            if (bodies == null || bodies.Length == 0 || material == null) return;
            block = new MaterialPropertyBlock();
            shapes = new Mesh[Frames][];
            places = new Vector3[Frames][];
            turns = new Quaternion[Frames][];
            stood = new Vector3[Frames];
            faced = new Vector3[Frames];
            for (int f = 0; f < Frames; f++)
            {
                shapes[f] = new Mesh[bodies.Length];
                places[f] = new Vector3[bodies.Length];
                turns[f] = new Quaternion[bodies.Length];
                for (int i = 0; i < bodies.Length; i++)
                {
                    shapes[f][i] = new Mesh { name = "Spell Clone" };
                    shapes[f][i].MarkDynamic();
                }
            }
            filters = new MeshFilter[bodies.Length];
            renderers = new MeshRenderer[bodies.Length];
            for (int i = 0; i < bodies.Length; i++)
            {
                var part = new GameObject("Spell Clone");
                part.transform.SetParent(transform, false);
                filters[i] = part.AddComponent<MeshFilter>();
                renderers[i] = part.AddComponent<MeshRenderer>();
                renderers[i].sharedMaterial = material;
                renderers[i].shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
                renderers[i].receiveShadows = false;
                renderers[i].enabled = false;
            }
        }

        private void OnDestroy()
        {
            if (shapes == null) return;
            foreach (var frame in shapes)
                foreach (var shape in frame)
                    if (shape != null) Destroy(shape);
        }

        /// <summary>Shows the copy in a light, from now until <see cref="Hide"/>.</summary>
        public void Show(Color light)
        {
            if (shapes == null || owner == null) return;
            colour = light;
            Showing = true;
            filled = 0;
            Position = owner.TransformPoint(Offset);
            Forward = owner.forward;
        }

        public void Hide()
        {
            Showing = false;
            if (renderers == null) return;
            foreach (var shown in renderers)
                if (shown != null) shown.enabled = false;
        }

        private void LateUpdate()
        {
            if (!Showing || owner == null || Time.deltaTime <= 0f) return;
            // This frame's body goes into the ring; the oldest in the ring is what is seen.
            for (int i = 0; i < bodies.Length; i++)
            {
                if (bodies[i] == null) continue;
                bodies[i].BakeMesh(shapes[next][i], true);
                places[next][i] = bodies[i].transform.position;
                turns[next][i] = bodies[i].transform.rotation;
            }
            stood[next] = owner.position;
            faced[next] = owner.forward;
            next = (next + 1) % Frames;
            if (filled < Frames) filled++;
            int oldest = filled < Frames ? 0 : next;
            Vector3 aside = Quaternion.LookRotation(faced[oldest], Vector3.up) * Offset;
            Position = stood[oldest] + aside;
            Forward = faced[oldest];
            block.SetColor(ColourId, new Color(colour.r, colour.g, colour.b, colour.a * (0.82f + 0.18f * Mathf.Sin(Time.time * 9f))));
            for (int i = 0; i < bodies.Length; i++)
            {
                filters[i].sharedMesh = shapes[oldest][i];
                renderers[i].transform.SetPositionAndRotation(places[oldest][i] + aside, turns[oldest][i]);
                renderers[i].SetPropertyBlock(block);
                renderers[i].enabled = true;
            }
        }
    }
}

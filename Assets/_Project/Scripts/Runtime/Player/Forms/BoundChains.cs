using Clover.Gameplay;
using UnityEngine;
using UnityEngine.Rendering;

namespace Clover.Player.Forms
{
    /// <summary>
    /// Draws a reaction's chain (<see cref="ElementLookSet.Span"/>, Black Chain's) between the two bodies the game really
    /// bound, for as long as the bind holds. The recipe drew its chain toward the spot it stood the second body; in a fight
    /// the game binds the nearest enemy within 6 m (Reactions.Near), wherever that one stands, so a chain baked toward the
    /// spot reached an enemy only when one stood within 1.6 m of it. Told when the bind begins on the bound one (its
    /// condition, joined to the struck one), it whips the chain out of the struck one, lays its links along an arc from
    /// chest to chest that follows both as they move, plays the recipe's look for the bound end (its wrap and clang rings)
    /// on the real bound one, and lets the chain go when the bind ends or either is gone. The same link, paint, solid and
    /// violet ink as the built look; a fixed pool made once, nothing allocated while it plays.
    /// </summary>
    public class BoundChains : MonoBehaviour
    {
        /// <summary>Chains drawn at once, and links in one (a 0.25 m pitch: ten metres of chain).</summary>
        public const int MostChains = 4, MostLinks = 40;
        /// <summary>Seconds a chain takes to go once its bind ends.</summary>
        public const float Going = 0.25f;
        /// <summary>How far the chain's links swing aside as it whips out, metres, and how long after it reaches that dies away.</summary>
        public const float Sway = 0.22f, Settles = 0.4f;
        /// <summary>The longest a chain is drawn, whatever its bind says: a little past the 5 s bind.</summary>
        public const float Longest = 6f;

        private struct Chain
        {
            public bool Active, Ending;
            public int Span;
            public Transform Host, Bound;
            public AfflictionSet Binds;
            public float Age, Gone;
        }

        private static readonly int HeadId = Shader.PropertyToID("_DrawHead");
        private static readonly int TailId = Shader.PropertyToID("_DrawTail");
        private static readonly int InkId = Shader.PropertyToID("_OutlineWidth");
        private static readonly int InkColourId = Shader.PropertyToID("_OutlineColor");
        private static readonly int ThinId = Shader.PropertyToID("_Thin");
        // Every other link stands across the one before it, turned a quarter about the chain's line.
        private static readonly Quaternion Across = Quaternion.Euler(0f, 0f, 90f);

        private static BoundChains instance;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => instance = null;

        private ElementLookSet set;
        private Chain[] chains;
        private Transform[] links;
        private MeshFilter[] meshes;
        private MeshRenderer[] shown;
        // The span each chain's links were last dressed for, so a link is dressed when its chain begins and never per frame.
        private int[] dressedFor;
        private MaterialPropertyBlock block;
        private int linksShown;

        /// <summary>True when the scene draws live chains (the looks are built with a span).</summary>
        public static bool Ready => instance != null && instance.set != null && instance.set.Spans != null && instance.set.Spans.Length > 0;

        /// <summary>Chains drawn now and their links shown, for probes.</summary>
        public static int Drawn
        {
            get
            {
                if (instance == null || instance.chains == null) return 0;
                int count = 0;
                for (int i = 0; i < MostChains; i++)
                    if (instance.chains[i].Active) count++;
                return count;
            }
        }

        public static int LinksShown => instance != null ? instance.linksShown : 0;

        /// <summary>
        /// A chain drawn now, for probes: the two bodies it binds and where its first and last links lie this frame (the
        /// last is its head while it whips out). False when no chain is in that place.
        /// </summary>
        public static bool TryChain(int index, out Transform host, out Transform bound, out Vector3 firstLink, out Vector3 lastLink, out int shownLinks)
        {
            host = bound = null;
            firstLink = lastLink = Vector3.zero;
            shownLinks = 0;
            if (instance == null || instance.chains == null || index < 0 || index >= MostChains || !instance.chains[index].Active) return false;
            host = instance.chains[index].Host;
            bound = instance.chains[index].Bound;
            int first = index * MostLinks;
            bool any = false;
            for (int k = 0; k < MostLinks; k++)
            {
                if (!instance.shown[first + k].enabled) continue;
                if (!any) firstLink = instance.links[first + k].position;
                lastLink = instance.links[first + k].position;
                any = true;
                shownLinks++;
            }
            return true;
        }

        /// <summary>True when a reaction's look leaves the body its span binds to this (the recipe's other body <paramref name="body"/>).</summary>
        public static bool Binds(string slug, int body)
        {
            if (!Ready || !ElementLooks.Ready) return false;
            int span = instance.set.FindSpan(slug);
            return span >= 0 && instance.set.Spans[span].Body == body;
        }

        /// <summary>The element art hands over the looks and the pool is made (once).</summary>
        public void Configure(ElementLookSet looks)
        {
            if (set != null || looks == null) return;
            set = looks;
            chains = new Chain[MostChains];
            links = new Transform[MostChains * MostLinks];
            meshes = new MeshFilter[links.Length];
            shown = new MeshRenderer[links.Length];
            dressedFor = new int[MostChains];
            block = new MaterialPropertyBlock();
            for (int i = 0; i < MostChains; i++) dressedFor[i] = -1;
            for (int i = 0; i < links.Length; i++)
            {
                var link = new GameObject("Bound Chain Link");
                link.transform.SetParent(transform, false);
                links[i] = link.transform;
                meshes[i] = link.AddComponent<MeshFilter>();
                shown[i] = link.AddComponent<MeshRenderer>();
                shown[i].shadowCastingMode = ShadowCastingMode.Off;
                shown[i].receiveShadows = false;
                shown[i].lightProbeUsage = LightProbeUsage.Off;
                shown[i].reflectionProbeUsage = ReflectionProbeUsage.Off;
                shown[i].enabled = false;
            }
        }

        private void OnEnable()
        {
            instance = this;
            CombatEvents.Condition += OnCondition;
        }

        private void OnDisable()
        {
            CombatEvents.Condition -= OnCondition;
            if (instance == this) instance = null;
            if (chains == null) return;
            for (int i = 0; i < MostChains; i++) End(i);
        }

        // A bind begins on the bound one, joined to the one struck: its chain is drawn between the two.
        private void OnCondition(Component who, ConditionNews news)
        {
            if (news.Moment != ConditionMoment.Began || who == null || news.Link == null || set == null || !ElementLooks.Ready) return;
            int span = -1;
            for (int i = 0; i < set.Spans.Length; i++)
                if (!string.IsNullOrEmpty(set.Spans[i].Condition) && set.Spans[i].Condition == news.Id) { span = i; break; }
            if (span < 0) return;
            Begin(span, news.Link.transform, who.transform, who is IAfflictable afflictable ? afflictable.Afflictions : null);
        }

        private void Begin(int span, Transform host, Transform bound, AfflictionSet binds)
        {
            int slot = -1, oldest = -1;
            for (int i = 0; i < MostChains; i++)
            {
                // The same two bound again: the chain goes on from where it is.
                if (chains[i].Active && ReferenceEquals(chains[i].Host, host) && ReferenceEquals(chains[i].Bound, bound))
                {
                    chains[i].Ending = false;
                    chains[i].Gone = 0f;
                    return;
                }
                if (slot < 0 && !chains[i].Active) slot = i;
                if (chains[i].Active && (oldest < 0 || chains[i].Age > chains[oldest].Age)) oldest = i;
            }
            if (slot < 0)
            {
                End(oldest);
                slot = oldest;
            }
            chains[slot] = new Chain { Active = true, Span = span, Host = host, Bound = bound, Binds = binds };
            if (dressedFor[slot] != span) Dress(slot, span);
            PlayBoundEnd(span, host, bound);
        }

        // The recipe's look for the bound end (its wrap and clang rings) on the real bound one, turned so the recipe's way
        // from the struck body to that one becomes the real way.
        private void PlayBoundEnd(int span, Transform host, Transform bound)
        {
            ref var spanData = ref set.Spans[span];
            int look = set.FindVariant(spanData.Key, (char)('0' + spanData.Body));
            if (look < 0 || !ElementLooks.TryLook(look, out var data)) return;
            Vector3 real = bound.position - host.position;
            float yaw = data.FacingYaw;
            if (real.x * real.x + real.z * real.z > 0.0001f && data.FromMain.x * data.FromMain.x + data.FromMain.z * data.FromMain.z > 0.0001f)
                yaw += Mathf.Atan2(real.x, real.z) * Mathf.Rad2Deg - Mathf.Atan2(data.FromMain.x, data.FromMain.z) * Mathf.Rad2Deg;
            ElementLooks.Play(look, bound, yaw);
        }

        // A chain's links take the span's link, solid and ink once, when the chain begins.
        private void Dress(int slot, int span)
        {
            ref var spanData = ref set.Spans[span];
            block.Clear();
            block.SetFloat(HeadId, 1f);
            block.SetFloat(TailId, 0f);
            block.SetFloat(InkId, spanData.InkWidth);
            block.SetColor(InkColourId, spanData.InkColour);
            block.SetFloat(ThinId, spanData.Thin);
            for (int k = 0; k < MostLinks; k++)
            {
                int i = slot * MostLinks + k;
                meshes[i].sharedMesh = spanData.Link;
                shown[i].sharedMaterial = spanData.Material;
                shown[i].SetPropertyBlock(block);
            }
            dressedFor[slot] = span;
        }

        private void End(int slot)
        {
            if (!chains[slot].Active) return;
            chains[slot] = default;
            for (int k = 0; k < MostLinks; k++) shown[slot * MostLinks + k].enabled = false;
        }

        private void LateUpdate()
        {
            if (set == null) return;
            float deltaTime = Time.deltaTime;
            if (deltaTime <= 0f) return;
            linksShown = 0;
            for (int i = 0; i < MostChains; i++)
            {
                ref var chain = ref chains[i];
                if (!chain.Active) continue;
                chain.Age += deltaTime;
                ref var span = ref set.Spans[chain.Span];
                bool gone = chain.Host == null || chain.Bound == null || !chain.Host.gameObject.activeInHierarchy || !chain.Bound.gameObject.activeInHierarchy;
                // Held while the bound one carries the bind (it is told before the bind is counted, so the first frames hold too).
                bool held = chain.Age < span.Reached || chain.Binds != null && chain.Binds.Conditions.Holds(span.Condition);
                if (!chain.Ending && (gone || !held || chain.Age > Longest)) chain.Ending = true;
                if (chain.Ending)
                {
                    chain.Gone += deltaTime;
                    if (gone || chain.Gone >= Going)
                    {
                        End(i);
                        continue;
                    }
                }
                Lay(i, ref chain, ref span);
            }
        }

        // The links along the arc from the struck one's chest to the bound one's, as far as the chain has whipped out.
        private void Lay(int slot, ref Chain chain, ref ElementLookSet.Span span)
        {
            int first = slot * MostLinks;
            float reach = chain.Age < span.From ? 0f : ElementLookSet.Ease(ElementLookSet.Out, (chain.Age - span.From) / Mathf.Max(0.01f, span.Reached - span.From));
            Vector3 from = chain.Host.position, to = chain.Bound.position;
            Vector3 flat = to - from;
            flat.y = 0f;
            float apart = flat.magnitude;
            Vector3 way = apart > 0.001f ? flat / apart : chain.Host.forward;
            Vector3 side = new Vector3(way.z, 0f, -way.x);
            float inset = Mathf.Min(span.Inset, apart * 0.25f);
            Vector3 start = from + way * inset + Vector3.up * span.Height;
            Vector3 end = to - way * inset + Vector3.up * span.Height;
            // It swings aside as it whips out and settles once it has reached; it is thrown a little higher meanwhile.
            float unsettled = 1f - Mathf.Clamp01((chain.Age - span.Reached) / Settles);
            float arc = span.Arc * (1f + 0.4f * unsettled);
            float sway = Sway * unsettled;
            Vector3 line = end - start;
            float length = line.magnitude + 8f * arc * arc / (3f * Mathf.Max(0.5f, line.magnitude));
            int count = Mathf.Clamp(Mathf.RoundToInt(length / Mathf.Max(0.05f, span.Pitch)), 2, MostLinks);
            // Past ten metres of chain the links are drawn longer rather than leave a gap.
            float stretch = Mathf.Max(1f, length / (count * span.Pitch));
            float shrink = chain.Ending ? Mathf.Max(0.001f, 1f - chain.Gone / Going) : 1f;
            for (int k = 0; k < MostLinks; k++)
            {
                int i = first + k;
                float t = (k + 0.5f) / count;
                if (k >= count || t > reach)
                {
                    if (shown[i].enabled) shown[i].enabled = false;
                    continue;
                }
                float wave = sway * Mathf.Sin(Mathf.PI * 2f * (t - chain.Age * 2f)) * Mathf.Sin(Mathf.PI * t);
                Vector3 point = start + line * t + Vector3.up * (arc * 4f * t * (1f - t)) + side * wave;
                Vector3 tangent = line + Vector3.up * (arc * 4f * (1f - 2f * t));
                if (tangent.sqrMagnitude < 0.0001f) tangent = way;
                var turn = Quaternion.LookRotation(tangent, Vector3.up);
                if ((k & 1) == 1) turn *= Across;
                links[i].SetPositionAndRotation(point, turn);
                links[i].localScale = new Vector3(shrink, shrink, shrink * stretch);
                if (!shown[i].enabled) shown[i].enabled = true;
                linksShown++;
            }
        }
    }
}

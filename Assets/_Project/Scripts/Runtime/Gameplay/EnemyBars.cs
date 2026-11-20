using System.Collections.Generic;
using Clover.Core;
using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>
    /// What the bars over a fighter do beyond showing two numbers: a pale piece lingers where health was just lost and
    /// then drains, and the balance bar is put away while it is empty so that the bars of a crowd do not pile up.
    /// The fighter itself still sets the length of the two bars; this reads them and does the rest. It also keeps the
    /// bars readable: clear of a neighbour's bars, and out from under the player's own bars in the screen's corner.
    /// Under the bars it shows a pip for each affliction the fighter carries, in that affliction's colour, larger the
    /// deeper it lies.
    /// </summary>
    public class EnemyBars : MonoBehaviour
    {
        [SerializeField] private Transform health;
        [SerializeField] private Transform lag;
        [SerializeField] private Transform balance;
        [SerializeField] private GameObject balanceRow;
        [SerializeField] private Renderer[] pips = new Renderer[0];

        /// <summary>How much room, in metres, these bars need across and upward; a name over them makes them taller.</summary>
        public float Wide = 1.55f;
        public float Tall = 0.46f;
        /// <summary>The corner of the screen the player's own bars fill, as shares of its width and height.</summary>
        public float HudRight = 0.33f;
        public float HudBottom = 0.74f;
        /// <summary>Bars are kept below this share of the screen's height, so a fighter at the top edge still shows them.</summary>
        public float ScreenTop = 0.93f;
        /// <summary>Metres a second the bars slide when they make room.</summary>
        public float SlideSpeed = 4f;
        /// <summary>How small a pip is at one stack, as a share of its size at full depth.</summary>
        public float PipSmallest = 0.55f;

        public LagMeter Lag { get; } = new LagMeter();
        /// <summary>How many pips are showing now, so probes can see them without the screen.</summary>
        public int PipsShown { get; private set; }

        // Every set of bars now showing, so each can keep clear of those before it.
        private static readonly List<EnemyBars> showing = new List<EnemyBars>();
        private static readonly int Colour = Shader.PropertyToID("_Color");

        private Camera view;
        private float restHeight;
        private float shift;
        private bool measured;
        private AfflictionSet watched;
        private MaterialPropertyBlock block;
        private AfflictionKind[] pipKind;
        private int[] pipDepth;
        private bool[] pipFrozen;
        private Vector3[] pipSize;
        // Each pip is a light on a piece of ink; the two are shown, hidden and sized together by what holds them.
        private Transform[] pipHolder;

        // Domain reload is off: the bars of the last play must not be kept.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics() => showing.Clear();

        public void Configure(Transform healthFill, Transform lagFill, Transform balanceFill, GameObject balanceRowRoot)
        {
            health = healthFill;
            lag = lagFill;
            balance = balanceFill;
            balanceRow = balanceRowRoot;
        }

        /// <summary>The builder hands over the row of pips that lie under the bars.</summary>
        public void ConfigurePips(Renderer[] pipRenderers) => pips = pipRenderers;

        /// <summary>Whose afflictions the pips show.</summary>
        public void Watch(AfflictionSet afflictions) => watched = afflictions;

        private void Awake()
        {
            block = new MaterialPropertyBlock();
            pipKind = new AfflictionKind[pips.Length];
            pipDepth = new int[pips.Length];
            pipFrozen = new bool[pips.Length];
            pipSize = new Vector3[pips.Length];
            pipHolder = new Transform[pips.Length];
            for (int i = 0; i < pips.Length; i++)
            {
                if (pips[i] == null) continue;
                pipHolder[i] = pips[i].transform.parent;
                pipSize[i] = pipHolder[i].localScale;
                pipHolder[i].gameObject.SetActive(false);
            }
        }

        private void OnEnable()
        {
            if (!measured)
            {
                measured = true;
                restHeight = transform.localPosition.y;
            }
            if (view == null) view = Camera.main;
            shift = 0f;
            showing.Add(this);
            // Shown again after being put away: the pale piece starts where the bar is, not where it was last seen.
            if (health != null) Lag.Snap(health.localScale.x);
        }

        private void OnDisable() => showing.Remove(this);

        private void LateUpdate()
        {
            if (health != null && lag != null)
                lag.localScale = new Vector3(Lag.Tick(health.localScale.x, Time.unscaledDeltaTime), 1f, 1f);
            if (balance != null && balanceRow != null)
            {
                bool some = balance.localScale.x > 0.004f;
                if (balanceRow.activeSelf != some) balanceRow.SetActive(some);
            }
            ShowPips();
            ShowOnBody();
            MakeRoom();
        }

        // An affliction's second state, as its look is told it: Chill frozen, Sundered rusting (it is Soaked too: Rust).
        private bool Second(AfflictionKind affliction) =>
            affliction == AfflictionKind.Chill ? watched.IsFrozen : affliction == AfflictionKind.Sundered && watched.Stacks(AfflictionKind.Soaked) > 0;

        /// <summary>The depth of Static at which a Soaked body is Seized (Reactions.Three: "two Static or more").</summary>
        public const int SeizedStatic = 2;

        private float untilLook;
        private int lookKind, lookBeat;

        // Each affliction carried is also seen on the body itself, as the stuff of its element: one small piece at a
        // time, the afflictions taking turns, on the game's own clock.
        private void ShowOnBody()
        {
            if (watched == null || PipsShown == 0 || transform.parent == null) return;
            // A burning body burns in real flame every frame, not in pieces thrown a few times a second; a flame gone
            // live (Flashover) is its reaction's alone.
            int burn = watched.Stacks(AfflictionKind.Burn);
            if (burn > 0 && !watched.Conditions.Has(ConditionFlags.FlameLive)) Clover.Player.Forms.BodyFlames.Keep(transform.parent, burn, watched.Conditions.KeepsKind(AfflictionKind.Burn));
            untilLook -= Time.deltaTime;
            if (untilLook > 0f) return;
            for (int tried = 1; tried < Afflictions.Count; tried++)
            {
                lookKind = lookKind % (Afflictions.Count - 1) + 1;
                var affliction = (AfflictionKind)lookKind;
                int depth = watched.Stacks(affliction);
                if (depth <= 0) continue;
                // A flame gone live is drawn by its reaction alone, never as the plain Burn beside it.
                if (affliction == AfflictionKind.Burn && watched.Conditions.Has(ConditionFlags.FlameLive)) continue;
                // A reaction's own look is playing on it and shows what became of this one (Golden Feathers over Marked).
                if (Clover.Player.Forms.ElementLooks.Hides(transform.parent, (int)affliction)) continue;
                // Soaked under two Static or more is Seized: its water has run off into the live film, so its wet look
                // and the rings that drain off it are not drawn (the owner, 2026-11-20: "remove the circles").
                if (affliction == AfflictionKind.Soaked && watched.Stacks(AfflictionKind.Static) >= SeizedStatic) continue;
                // Soaked steel is Rust: the water has gone into the rust, so no wet rings over it (the owner: "rust coloured").
                if (affliction == AfflictionKind.Soaked && watched.Stacks(AfflictionKind.Sundered) > 0) continue;
                // Two pieces at a time, so there is always something of it on the body.
                for (int piece = 0; piece < 2; piece++)
                    Clover.Player.Forms.AfflictionLook.Show(affliction, depth, Second(affliction), transform.parent.position, lookBeat++);
                untilLook = Mathf.Max(0.08f, Clover.Player.Forms.AfflictionLook.Interval(depth) / PipsShown);
                return;
            }
        }

        // One pip for each affliction carried, in the order the elements stand. A pip is repainted only when what it shows changes.
        private void ShowPips()
        {
            if (pips.Length == 0) return;
            int used = 0;
            if (watched != null)
            {
                for (int kind = 1; kind < Afflictions.Count && used < pips.Length; kind++)
                {
                    var affliction = (AfflictionKind)kind;
                    int depth = watched.Stacks(affliction);
                    if (depth <= 0) continue;
                    var pip = pips[used];
                    bool frozen = affliction == AfflictionKind.Chill && watched.IsFrozen;
                    if (pip != null && (!pipHolder[used].gameObject.activeSelf || pipKind[used] != affliction || pipDepth[used] != depth || pipFrozen[used] != frozen))
                    {
                        pipKind[used] = affliction;
                        pipDepth[used] = depth;
                        pipFrozen[used] = frozen;
                        var spec = Afflictions.Spec(affliction);
                        float size = Mathf.Lerp(PipSmallest, 1f, spec.MaximumStacks > 1 ? (depth - 1f) / (spec.MaximumStacks - 1f) : 1f);
                        pipHolder[used].localScale = pipSize[used] * (frozen ? 1.25f : size);
                        block.SetColor(Colour, frozen ? Color.white : spec.Colour);
                        pip.SetPropertyBlock(block);
                        pipHolder[used].gameObject.SetActive(true);
                    }
                    used++;
                }
            }
            for (int i = used; i < pips.Length; i++)
                if (pips[i] != null && pipHolder[i].gameObject.activeSelf) pipHolder[i].gameObject.SetActive(false);
            PipsShown = used;
        }

        // The bars face a camera that never turns, so "across" and "up" on the screen are the bars' own right and up.
        private void MakeRoom()
        {
            Transform owner = transform.parent;
            if (owner == null) return;
            Vector3 rest = owner.position + Vector3.up * restHeight;
            Vector3 up = transform.up, right = transform.right;
            float wanted = 0f;
            // Twice over: stepping clear of one neighbour may step onto another.
            for (int pass = 0; pass < 2; pass++)
            {
                for (int i = 0; i < showing.Count; i++)
                {
                    var other = showing[i];
                    // Only those before this one are stepped over, so two neighbours do not both move.
                    if (other == this) break;
                    Vector3 between = rest + up * wanted - other.transform.position;
                    float across = Vector3.Dot(between, right), above = Vector3.Dot(between, up);
                    float needed = (Tall + other.Tall) * 0.5f;
                    if (Mathf.Abs(across) < (Wide + other.Wide) * 0.5f && Mathf.Abs(above) < needed) wanted += needed - above;
                }
            }
            if (view != null)
            {
                Vector3 seen = view.WorldToViewportPoint(rest + up * wanted);
                // Under the player's own bars nothing can be read, and off the top of the screen nothing is seen: the bars
                // come down to sit just below that corner, or just inside the edge.
                float ceiling = seen.x < HudRight ? HudBottom : ScreenTop;
                if (seen.z > 0f && seen.y > ceiling)
                    wanted -= (seen.y - ceiling) * 2f * seen.z * Mathf.Tan(view.fieldOfView * 0.5f * Mathf.Deg2Rad);
            }
            shift = Mathf.MoveTowards(shift, wanted, SlideSpeed * Time.unscaledDeltaTime);
            transform.position = rest + up * shift;
        }
    }
}

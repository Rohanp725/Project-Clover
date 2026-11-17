using Clover.Core;
using Clover.Gameplay;
using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// The rule of a land where a fall returns the player to the last rest fire lit (the scene has no RunEnder, so
    /// <see cref="AreaDirector"/> already puts the player back and the ordinary enemies on their feet). This adds the
    /// land's own terms: its elite stays dead once beaten, and the unspent embers carried are lost. A scene without this
    /// component behaves as it always did. It also lets enemies far from the player stop animating, since a land is
    /// several hundred metres long and its people are all in one scene.
    /// </summary>
    public class LandReturn : MonoBehaviour
    {
        [SerializeField] private AreaDirector director;
        [SerializeField] private PlayerDefence defence;

        /// <summary>The cost of a fall proposed in WORLD.md: the embers carried and not yet spent are lost. Switch off to keep them.</summary>
        public bool LoseCarriedEmbers = true;
        /// <summary>The land's elite, once beaten, is not brought back by a rest or a fall.</summary>
        public bool ElitesStayFallen = true;
        /// <summary>Enemies farther than this that have not noticed anyone stop animating until the player comes near. Nothing or less: none sleep.</summary>
        public float SleepBeyond = 55f;

        public int Falls { get; private set; }
        public int EmbersLost { get; private set; }
        public int Asleep { get; private set; }

        private Animator[] animators;
        private float untilLook;

        public void Configure(AreaDirector areaDirector, PlayerDefence playerDefence)
        {
            director = areaDirector;
            defence = playerDefence;
        }

        private void Awake()
        {
            if (director != null) director.ElitesStayFallen = ElitesStayFallen;
        }

        private void Start()
        {
            if (defence != null) defence.Revived += OnRevived;
            if (director == null) return;
            animators = new Animator[director.Enemies.Length];
            for (int i = 0; i < animators.Length; i++)
                if (director.Enemies[i] != null) animators[i] = director.Enemies[i].GetComponentInChildren<Animator>();
        }

        private void OnDestroy()
        {
            if (defence != null) defence.Revived -= OnRevived;
        }

        private void OnRevived()
        {
            Falls++;
            int kept = ReturnRule.EmbersAfterFall(RunState.Points, LoseCarriedEmbers);
            int lost = RunState.Points - kept;
            if (lost <= 0) return;
            EmbersLost += lost;
            RunState.Points = kept;
            if (defence != null) defence.Callout(lost == 1 ? "1 ember lost" : lost + " embers lost");
        }

        private void Update()
        {
            if (animators == null || defence == null) return;
            untilLook -= Time.deltaTime;
            if (untilLook > 0f) return;
            untilLook = 0.5f;
            Vector3 centre = defence.transform.position;
            float limit = SleepBeyond * SleepBeyond;
            int asleep = 0;
            for (int i = 0; i < animators.Length; i++)
            {
                var enemy = director.Enemies[i];
                if (enemy == null || animators[i] == null) continue;
                bool far = SleepBeyond > 0f && !enemy.IsAware && (enemy.transform.position - centre).sqrMagnitude > limit;
                if (far) asleep++;
                // Far off it holds the pose it had; near or in a fight it animates whether it is seen or not, as elsewhere.
                animators[i].cullingMode = far ? AnimatorCullingMode.CullCompletely : AnimatorCullingMode.AlwaysAnimate;
            }
            Asleep = asleep;
        }
    }
}

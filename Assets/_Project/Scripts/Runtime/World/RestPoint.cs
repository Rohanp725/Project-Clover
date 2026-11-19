using Clover.Core;
using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// A place to rest: a cold brazier that the player lights. Resting refills health and flasks, brings the area's
    /// enemies back, and makes this the place the player returns to after falling.
    /// </summary>
    public class RestPoint : Interactable
    {
        [SerializeField] private AreaDirector director;
        [SerializeField] private Transform returnPoint;
        [SerializeField] private Light fire;
        [SerializeField] private ParticleSystem flames;

        public float FireIntensity = 14f;
        /// <summary>What a rest fire says to someone who holds no element yet.</summary>
        public const string NothingToFeed = "Rested. Nothing to feed yet.";

        public bool IsLit { get; private set; }
        public int Rests { get; private set; }

        private float flicker;

        public void Configure(AreaDirector areaDirector, Transform whereToReturn, Light fireLight, ParticleSystem fireFlames)
        {
            director = areaDirector;
            returnPoint = whereToReturn;
            fire = fireLight;
            flames = fireFlames;
        }

        private void Awake()
        {
            if (fire != null) fire.enabled = false;
        }

        public override void Use(PlayerInteraction who)
        {
            Rests++;
            if (!IsLit)
            {
                IsLit = true;
                if (fire != null) fire.enabled = true;
                if (flames != null) flames.Play();
                Vfx.Play(VfxKind.Fire, transform.position + Vector3.up * 1.2f, Vector3.up, 1.5f);
            }
            Vfx.Play(VfxKind.Heal, who.transform.position + Vector3.up * 0.6f);
            if (director != null) director.Rest(returnPoint != null ? returnPoint : transform);
            if (who.Defence != null) who.Defence.Callout("Rested");
            // A rest ends every spell's clock and any fight.
            var spells = who.GetComponent<Clover.Player.PlayerSpells>();
            if (spells != null) spells.Rested();
            // The playtest's rule: a rest gives as many Shards as the whole chart could ask for.
            RunState.Points = PlaytestMode.ShardsAfterRest(RunState.Points);
            // By the fire is where the embers are spent.
            var held = who.GetComponent<PlayerElement>();
            if (held != null && !held.HasElement && !PlaytestMode.On)
            {
                // No element, no tree: the embers are carried until there is something to feed them to.
                if (who.Defence != null) who.Defence.Callout(NothingToFeed);
                return;
            }
            // The fire's menu: the Hand, the chart, leave (IMPLEMENTATION.md 4.1). A scene with no HUD to build it under opens the chart as before.
            var menu = Clover.UI.RestMenu.Ensure();
            if (menu != null) menu.Open(this);
            else if (Clover.UI.UpgradeMenu.Instance != null) Clover.UI.UpgradeMenu.Instance.Open();
        }

        private void Update()
        {
            if (!IsLit || fire == null) return;
            // A fire is never steady.
            flicker += Time.deltaTime * 11f;
            fire.intensity = FireIntensity * (0.86f + 0.14f * Mathf.PerlinNoise(flicker, 0.37f));
        }
    }
}

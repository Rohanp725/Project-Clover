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
            // By the fire is where the embers are spent.
            if (Clover.UI.UpgradeMenu.Instance != null) Clover.UI.UpgradeMenu.Instance.Open();
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

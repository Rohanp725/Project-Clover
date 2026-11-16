using Clover.Core;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Clover.Player
{
    /// <summary>
    /// The player's element: casts its spell and its surge, and shows them. The rules and numbers are in
    /// <see cref="ElementKit"/>. Like the weapon and the guard it has no Update of its own for the rules:
    /// <see cref="PlayerController"/> calls <see cref="Advance"/> so everything is settled in one order.
    /// </summary>
    [RequireComponent(typeof(PlayerController))]
    public class PlayerElement : MonoBehaviour
    {
        [SerializeField] private Light glow;
        [SerializeField] private SlashArcs arcs;
        [SerializeField] private TopDownCamera shaker;

        public ElementKit Kit = new ElementKit();
        /// <summary>The element's colour as light; brighter than white so it blooms.</summary>
        [ColorUsage(true, true)] public Color Colour = new Color(2.6f, 0.9f, 0.15f, 0.95f);
        /// <summary>Seconds a spell or surge press is remembered while the player is busy.</summary>
        public float PressBufferTime = 0.25f;
        public float GlowWhenCasting = 9f;
        public float GlowWhenSurging = 2.6f;
        /// <summary>Seconds between the embers that fall from the player while the surge lasts.</summary>
        public float EmberInterval = 0.12f;

        public bool IsCasting => Kit.IsCasting;
        public int SpellsCast { get; private set; }
        public int Surges { get; private set; }
        public int SpellHits { get; private set; }

        private PlayerController player;
        private PlayerCombat combat;
        private PlayerDefence defence;
        private InputAction spellAction;
        private InputAction surgeAction;
        private float spellBuffered;
        private float surgeBuffered;
        private float glowLevel;
        private float untilEmber;

        public void Configure(Light glowLight, SlashArcs arcsToShow, TopDownCamera cameraToShake)
        {
            glow = glowLight;
            arcs = arcsToShow;
            shaker = cameraToShake;
        }

        private void Awake()
        {
            player = GetComponent<PlayerController>();
            combat = GetComponent<PlayerCombat>();
            defence = GetComponent<PlayerDefence>();
            Kit.Reset();
            if (glow != null) glow.intensity = 0f;
        }

        private void Start()
        {
            spellAction = player.FindAction("Spell");
            surgeAction = player.FindAction("Surge");
        }

        /// <summary>A blow or a dodge breaks a cast in progress.</summary>
        public void Interrupt() => Kit.Interrupt();

        /// <summary>Runs one frame. <paramref name="free"/> is false while the player is rolling, attacking, flinching or drinking.</summary>
        public void Advance(float deltaTime, bool free)
        {
            if (spellAction == null || surgeAction == null) return;
            if (spellAction.WasPressedThisFrame()) spellBuffered = PressBufferTime;
            if (surgeAction.WasPressedThisFrame()) surgeBuffered = PressBufferTime;

            // The surge is instant and never roots the player, so it can be lit at any free moment.
            if (surgeBuffered > 0f && free)
            {
                surgeBuffered = 0f;
                if (Kit.TrySurge(combat.Mana)) BeginSurge();
                else if (defence != null) defence.Callout("No mana");
            }
            if (spellBuffered > 0f && free && !Kit.IsCasting)
            {
                spellBuffered = 0f;
                if (Kit.TryCast(combat.Mana)) BeginCast();
                else if (defence != null) defence.Callout("No mana");
            }
            if (spellBuffered > 0f) spellBuffered -= deltaTime;
            if (surgeBuffered > 0f) surgeBuffered -= deltaTime;

            if (Kit.TickCast(deltaTime)) Release();
            if (Kit.TickSurge(deltaTime)) combat.ClearEmpower();

            if (Kit.SurgeActive)
            {
                untilEmber -= deltaTime;
                if (untilEmber <= 0f)
                {
                    untilEmber = EmberInterval;
                    Vfx.Play(VfxKind.Embers, transform.position + Vector3.up * 1.1f, Vector3.up, 0.25f);
                }
            }
        }

        private void LateUpdate()
        {
            if (glow == null) return;
            float resting = Kit.SurgeActive ? GlowWhenSurging * (0.85f + 0.15f * Mathf.Sin(Time.time * 17f)) : 0f;
            glowLevel = Mathf.MoveTowards(glowLevel, resting, Time.deltaTime * 30f);
            glow.intensity = glowLevel;
            glow.enabled = glowLevel > 0.01f;
        }

        private void BeginCast()
        {
            // The spell goes at whoever is nearest, as an attack would.
            var enemy = combat.NearestEnemy();
            if (enemy != null)
            {
                Vector3 toward = enemy.position - transform.position;
                toward.y = 0f;
                if (toward.sqrMagnitude > 0.0025f) transform.rotation = Quaternion.LookRotation(toward, Vector3.up);
            }
        }

        private void Release()
        {
            SpellsCast++;
            Vector3 forward = transform.forward;
            Vector3 hand = transform.position + Vector3.up * 1.1f;
            SpellHits += combat.StrikeCone(Kit.SpellReach, Kit.SpellArcDegrees, Kit.SpellDamage, Kit.SpellStagger, Kit.SpellBurnSeconds, VfxKind.Fire);
            // The wave itself: fire thrown along the cone, an arc of light at its front, and a flash on everything near.
            for (int i = 1; i <= 4; i++)
                Vfx.Play(VfxKind.Fire, hand + forward * (Kit.SpellReach * i / 4.5f), forward, 0.6f);
            Vfx.Play(VfxKind.Embers, hand + forward * (Kit.SpellReach * 0.6f), forward, 1.5f);
            if (arcs != null)
            {
                arcs.Show(transform.position, forward, Kit.SpellReach, Kit.SpellArcDegrees, 1f, Colour);
                arcs.Show(transform.position, forward, Kit.SpellReach * 0.7f, Kit.SpellArcDegrees, -1f, Colour);
            }
            glowLevel = GlowWhenCasting;
            if (shaker != null) shaker.Shake(0.14f, 0.2f);
        }

        private void BeginSurge()
        {
            Surges++;
            combat.Empower(Kit.SurgeDamage, Kit.SurgeBurnSeconds, Colour);
            Vfx.Play(VfxKind.Fire, transform.position + Vector3.up, Vector3.up, 1.2f);
            Vfx.Play(VfxKind.Ring, transform.position + Vector3.up * 0.2f, Vector3.up);
            glowLevel = GlowWhenCasting;
            if (defence != null) defence.Callout(Kit.SurgeName);
        }
    }
}

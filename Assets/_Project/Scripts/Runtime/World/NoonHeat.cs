using Clover.Core;
using Clover.Gameplay;
using UnityEngine;
using UnityEngine.Rendering;

namespace Clover.World
{
    /// <summary>
    /// The heat of noon (fire_rework.md, section 2): out under the sky the sun swells on a rhythm of the scene's own.
    /// For the warning the look whitens, the HUD's sun mark fills and the shadows harden; then for the swell anyone
    /// standing in the sun burns a little at a time, and the corrupted feed on it (wicks and the ashen quicken, the
    /// thirsting go wild, the gilded's gold runs on its own). Calm rectangles (an elite's arena) are spared. It lives
    /// on the scene's director object, so a scene without it has no swells at all.
    /// </summary>
    public class NoonHeat : MonoBehaviour
    {
        [SerializeField] private HeatSchedule schedule = new HeatSchedule(20f, 50f);
        [SerializeField] private Vector4[] calm = new Vector4[0];
        [SerializeField] private Clover.Player.PlayerDefence defence;
        [SerializeField] private Light sun;
        [SerializeField] private Volume swellLook;
        [SerializeField] private Clover.UI.LandHud hud;
        [SerializeField] private Enemy[] enemies = new Enemy[0];

        public float BurnDamage = 3f;
        public float BurnEvery = 0.8f;
        public float WickPace = 1.45f;
        public float ThirstPace = 1.3f;
        /// <summary>How much brighter the sun is at the height of a swell.</summary>
        public float SunGain = 0.35f;

        public HeatSchedule Schedule => schedule;
        public HeatSchedule.Phase Current { get; private set; }
        public float Progress { get; private set; }
        public float Clock { get; private set; }
        public int Swells { get; private set; }
        public int Burns { get; private set; }
        public float Burnt { get; private set; }

        /// <summary>True while the sun is swelling in the scene now.</summary>
        public static bool Swelling { get; private set; }
        public static NoonHeat Instance { get; private set; }

        // Domain reload is off: the last play's swell is not this one's.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStatics()
        {
            Swelling = false;
            Instance = null;
        }

        private float baseIntensity, baseShadow;
        private float untilBurn, untilPace, untilShow, untilShimmer;

        public void Configure(HeatSchedule rhythm, Vector4[] calmRectangles, Clover.Player.PlayerDefence playerDefence, Light key, Volume look, Clover.UI.LandHud landHud, Enemy[] corrupted)
        {
            schedule = rhythm;
            calm = calmRectangles ?? new Vector4[0];
            defence = playerDefence;
            sun = key;
            swellLook = look;
            hud = landHud;
            enemies = corrupted ?? new Enemy[0];
        }

        /// <summary>Changes the rhythm from now on (a district's own), keeping the clock.</summary>
        public void SetSchedule(HeatSchedule rhythm) => schedule = rhythm;

        /// <summary>Sets the clock, as a probe does to bring a swell on at once.</summary>
        public void SetClock(float seconds) => Clock = seconds;

        /// <summary>True for a point inside a calm rectangle, where the swell never burns.</summary>
        public bool IsCalm(Vector3 point)
        {
            foreach (var r in calm)
                if (point.x >= r.x && point.x <= r.y && point.z >= r.z && point.z <= r.w) return true;
            return false;
        }

        private void Awake()
        {
            Instance = this;
            Swelling = false;
            if (sun != null)
            {
                baseIntensity = sun.intensity;
                baseShadow = sun.shadowStrength;
            }
            if (swellLook != null) swellLook.weight = 0f;
        }

        private void OnDestroy()
        {
            if (Instance != this) return;
            Instance = null;
            Swelling = false;
        }

        private void Update()
        {
            float deltaTime = Time.deltaTime;
            Clock += deltaTime;
            var was = Current;
            Current = schedule.At(Clock, out float progress);
            Progress = progress;
            bool playerCalm = defence != null && IsCalm(defence.transform.position);
            if (Current == HeatSchedule.Phase.Swell && was != HeatSchedule.Phase.Swell) Swells++;
            if (Current == HeatSchedule.Phase.Warning && was == HeatSchedule.Phase.Calm && defence != null && !playerCalm) defence.Callout("The sun swells. Find shade.");
            Swelling = Current == HeatSchedule.Phase.Swell;

            // The look: whiter and harder-shadowed through the warning, held through the swell, eased off at its end.
            float white = Current == HeatSchedule.Phase.Warning ? progress * progress
                : Current == HeatSchedule.Phase.Swell ? Mathf.Clamp01((1f - progress) * schedule.Swell / 1.5f) : 0f;
            if (playerCalm) white = 0f;
            if (swellLook != null) swellLook.weight = white;
            if (sun != null)
            {
                sun.intensity = baseIntensity * (1f + SunGain * white);
                sun.shadowStrength = Mathf.Lerp(baseShadow, 1f, white);
            }

            // Heat shimmers off the lit ground round the player while the sun swells, none of it in shade.
            if (white > 0.3f && defence != null)
            {
                untilShimmer -= deltaTime;
                if (untilShimmer <= 0f)
                {
                    untilShimmer = 0.04f / white;
                    Vector3 near = defence.transform.position + new Vector3(Random.Range(-9f, 9f), 0.1f, Random.Range(-6f, 7f));
                    if (Sunlight.IsLit(near) && !IsCalm(near)) HeatShimmer.Play(near, 1.1f);
                }
            }

            bool lit = defence != null && Sunlight.IsLit(defence.transform.position);
            bool burning = defence != null && !defence.IsDead && HeatSchedule.Burns(Current, lit, playerCalm);
            if (hud != null) hud.ShowSun(playerCalm ? HeatSchedule.Phase.Calm : Current, progress, burning);
            if (burning)
            {
                untilShow -= deltaTime;
                if (untilShow <= 0f)
                {
                    // The skin scorching: embers lifting off the shoulders, a thread of smoke, the odd tongue of flame.
                    untilShow = 0.15f;
                    Vector3 at = defence.transform.position;
                    Vfx.Play(VfxKind.Embers, at + Vector3.up * (1.1f + Random.value * 0.6f) + Random.insideUnitSphere * 0.25f, Vector3.up, 0.3f);
                    if (Random.value < 0.5f) Vfx.Play(VfxKind.Smoke, at + Vector3.up * 1.7f + Random.insideUnitSphere * 0.2f, Vector3.up, 0.35f, new Color(0.26f, 0.24f, 0.23f, 0.8f));
                    if (Random.value < 0.35f) Vfx.Play(VfxKind.Fire, at + Vector3.up * 1.4f, Vector3.up, 0.12f);
                }
                untilBurn -= deltaTime;
                if (untilBurn <= 0f)
                {
                    untilBurn = BurnEvery;
                    float taken = defence.Scorch(BurnDamage);
                    if (taken > 0f)
                    {
                        Burns++;
                        Burnt += taken;
                        Vfx.Play(VfxKind.Embers, defence.transform.position + Vector3.up * 1f, Vector3.up, 0.8f);
                    }
                }
            }
            else untilBurn = Mathf.Min(untilBurn, BurnEvery * 0.5f);

            untilPace -= deltaTime;
            if (untilPace > 0f) return;
            untilPace = 0.25f;
            foreach (var enemy in enemies)
            {
                if (enemy == null || enemy.Kind == null) continue;
                float pace = 1f;
                if (Swelling && !IsCalm(enemy.transform.position))
                {
                    string kind = enemy.Kind.Prefix;
                    pace = kind == "wick" || kind == "ashen" ? WickPace : kind == "thirst" ? ThirstPace : 1f;
                }
                enemy.SwellPace = pace;
            }
        }
    }
}

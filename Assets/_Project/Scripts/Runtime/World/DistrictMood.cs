using UnityEngine;
using UnityEngine.Rendering;

namespace Clover.World
{
    /// <summary>
    /// What changes as the player crosses from one district of a land into the next (the visual overview, steps 6 and 7):
    /// the air that drifts round them (dust over the river town, ash and smoke over the city of wicks, glare motes on the
    /// king's ground) and the light (a look of the district's own, a fog of its own), eased over a couple of seconds.
    /// The district is the one <see cref="LandZones"/> says the player stands in. Each array is indexed by district;
    /// an empty or missing entry leaves that part alone.
    /// </summary>
    public class DistrictMood : MonoBehaviour
    {
        [SerializeField] private LandZones zones;
        [SerializeField] private Transform player;
        [SerializeField] private GameObject[] air = new GameObject[0];
        [SerializeField] private Volume[] looks = new Volume[0];
        [SerializeField] private float[] fog = new float[0];

        /// <summary>Seconds to ease from one district's light into the next.</summary>
        public float Ease = 2.5f;

        private int shown = -2;
        private float baseFog;

        /// <summary>The district whose air is drifting now (-1 before any).</summary>
        public int Showing => shown;

        public void Configure(LandZones landZones, Transform who, GameObject[] districtAir, Volume[] districtLooks, float[] districtFog)
        {
            zones = landZones;
            player = who;
            air = districtAir;
            looks = districtLooks;
            fog = districtFog;
        }

        private void OnEnable() => baseFog = RenderSettings.fogDensity;

        // The scene's own fog comes back when this goes (a scene left mid-district keeps nothing of it).
        private void OnDisable() => RenderSettings.fogDensity = baseFog;

        private void Start()
        {
            foreach (var look in looks)
                if (look != null) look.weight = 0f;
            foreach (var drift in air)
                if (drift != null) drift.SetActive(false);
        }

        private void LateUpdate()
        {
            if (player != null) transform.position = player.position;
            int district = zones != null ? zones.District : -1;
            if (district != shown)
            {
                shown = district;
                for (int i = 0; i < air.Length; i++)
                    if (air[i] != null) air[i].SetActive(i == district);
            }
            float step = Ease > 0f ? Time.deltaTime / Ease : 1f;
            for (int i = 0; i < looks.Length; i++)
                if (looks[i] != null) looks[i].weight = Mathf.MoveTowards(looks[i].weight, i == district ? 1f : 0f, step);
            if (district >= 0 && district < fog.Length && fog[district] > 0f)
                RenderSettings.fogDensity = Mathf.MoveTowards(RenderSettings.fogDensity, fog[district], step * 0.01f);
            else if (district >= 0)
                RenderSettings.fogDensity = Mathf.MoveTowards(RenderSettings.fogDensity, baseFog, step * 0.01f);
        }
    }
}

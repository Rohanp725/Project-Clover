using Clover.Gameplay;
using Clover.Player;
using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// The places of a land that change something while the player stands in them, each a rectangle of floor (least x,
    /// greatest x, least z, greatest z) within a band of heights: vistas, where the camera eases back to show what is
    /// ahead (not while anyone has noticed the player); districts, each with its own rhythm of noon swells, set as the
    /// player crosses into it; and the places where a district's first swell is brought on at once, once a sitting.
    /// Checked by position each frame: no physics triggers.
    /// </summary>
    public class LandZones : MonoBehaviour
    {
        public float VistaDistance = 1.5f;
        public Vector3 VistaLookAhead = new Vector3(0f, 0f, 6f);
        public float VistaPitch = -10f;

        [SerializeField] private Transform player;
        [SerializeField] private TopDownCamera view;
        [SerializeField] private AreaDirector director;
        [SerializeField] private Vector4[] vistas = new Vector4[0];
        [SerializeField] private Vector2[] vistaHeights = new Vector2[0];
        [SerializeField] private Vector4[] districts = new Vector4[0];
        [SerializeField] private float[] districtEvery = new float[0];
        [SerializeField] private Vector4[] forced = new Vector4[0];
        [SerializeField] private Vector2[] forcedHeights = new Vector2[0];

        /// <summary>Which vista the player stands in now, or -1.</summary>
        public int InVista { get; private set; } = -1;
        /// <summary>Which district's rhythm is in force, or -1 before any.</summary>
        public int District { get; private set; } = -1;
        public int Forced { get; private set; }
        private bool[] spent = new bool[0];
        private bool framing;

        public void Configure(Transform who, TopDownCamera camera, AreaDirector areaDirector)
        {
            player = who;
            view = camera;
            director = areaDirector;
        }

        public void SetVistas(Vector4[] rectangles, Vector2[] heights)
        {
            vistas = rectangles;
            vistaHeights = heights;
        }

        public void SetDistricts(Vector4[] rectangles, float[] every)
        {
            districts = rectangles;
            districtEvery = every;
        }

        public void SetForced(Vector4[] rectangles, Vector2[] heights)
        {
            forced = rectangles;
            forcedHeights = heights;
        }

        private void Awake() => spent = new bool[forced.Length];

        private static bool Inside(Vector4 r, Vector3 at) => at.x >= r.x && at.x <= r.y && at.z >= r.z && at.z <= r.w;

        private bool Noticed()
        {
            if (director == null) return false;
            var enemies = director.Enemies;
            for (int i = 0; i < enemies.Length; i++)
                if (enemies[i] != null && !enemies[i].IsDead && enemies[i].IsAware) return true;
            return false;
        }

        private void Update()
        {
            if (player == null) return;
            Vector3 at = player.position;

            int vista = -1;
            for (int i = 0; i < vistas.Length; i++)
                if (Inside(vistas[i], at) && at.y >= vistaHeights[i].x && at.y <= vistaHeights[i].y) { vista = i; break; }
            if (vista >= 0 && Noticed()) vista = -1;
            InVista = vista;
            if (view != null)
            {
                if (vista >= 0)
                {
                    view.SetFraming(VistaDistance, VistaLookAhead, VistaPitch);
                    framing = true;
                }
                else if (framing)
                {
                    view.ClearFraming();
                    framing = false;
                }
            }

            var heat = NoonHeat.Instance;
            if (heat == null) return;
            for (int i = 0; i < districts.Length; i++)
            {
                if (!Inside(districts[i], at)) continue;
                if (i != District)
                {
                    District = i;
                    float every = districtEvery[i];
                    // A district's rhythm starts from the moment it is entered; one with none is calm.
                    heat.SetSchedule(every > 0f ? new HeatSchedule(heat.Clock + every, every) : HeatSchedule.Never);
                }
                break;
            }
            for (int i = 0; i < forced.Length; i++)
            {
                if (spent[i] || !Inside(forced[i], at) || at.y < forcedHeights[i].x || at.y > forcedHeights[i].y) continue;
                spent[i] = true;
                Forced++;
                float every = heat.Schedule.Every > 0f ? heat.Schedule.Every : 60f;
                // The warning begins now and the swell follows it; the district's rhythm runs on from there.
                heat.SetSchedule(new HeatSchedule(heat.Clock + heat.Schedule.Warning, every));
            }
        }
    }
}

using UnityEngine;

namespace Clover.World
{
    /// <summary>
    /// A scene's light and shade, kept by the builder that made the roofs, walls and columns so the two cannot
    /// disagree, and handed to <see cref="Sunlight"/> when the scene wakes. Each rectangle is least x, greatest x,
    /// least z, greatest z.
    /// </summary>
    public class SunlightZones : MonoBehaviour
    {
        [SerializeField] private bool openSky = true;
        [SerializeField] private Vector4[] shade = new Vector4[0];
        [SerializeField] private Vector4[] lit = new Vector4[0];

        public void Configure(bool skyIsOpen, Vector4[] shadeRectangles, Vector4[] lightRectangles)
        {
            openSky = skyIsOpen;
            shade = shadeRectangles ?? new Vector4[0];
            lit = lightRectangles ?? new Vector4[0];
        }

        private void Awake()
        {
            Sunlight.Clear();
            Sunlight.SetOpenSky(openSky);
            foreach (var r in lit) Sunlight.AddLight(r.x, r.y, r.z, r.w);
            foreach (var r in shade) Sunlight.AddShade(r.x, r.y, r.z, r.w);
        }

        private void Update() => Sunlight.Tick(Time.deltaTime);

        private void OnDestroy() => Sunlight.Clear();
    }
}

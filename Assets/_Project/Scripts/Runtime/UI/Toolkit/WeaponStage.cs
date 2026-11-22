using UnityEngine;

namespace Clover.UI.Toolkit
{
    /// <summary>
    /// The weapon picture in the Loadout's centre (ui_rest_fire_plan.md 3.5.9, the owner's N1 (a)): the carried weapon's
    /// own model, copied from the player's rack, on a small stage far under the world, drawn by its own camera into a
    /// texture the menu shows, turning slowly in real time (the world is stopped while the menu is open). It wears
    /// whatever the weapon wears, so each aspect's look shows as soon as its props are in. Made by code when the Loadout
    /// opens and put away when it closes; nothing of it is saved in a scene.
    /// </summary>
    public class WeaponStage : MonoBehaviour
    {
        public const int TextureSize = 512;
        /// <summary>Degrees a second the weapon turns.</summary>
        public const float TurnSpeed = 14f;
        // Far below the world, and the camera's reach short, so nothing of the world is in the picture.
        private static readonly Vector3 StageAt = new Vector3(0f, -4000f, 0f);

        private Camera stageCamera;
        private RenderTexture texture;
        private Transform turntable, tilt;
        private GameObject shown;

        public RenderTexture Texture => texture;
        /// <summary>The aspect whose look is shown (the focused card's while choosing); kept for when aspects change the model.</summary>
        public string Look { get; private set; } = "";

        public static WeaponStage Make()
        {
            var made = new GameObject("Loadout Weapon Stage");
            made.transform.position = StageAt;
            return made.AddComponent<WeaponStage>();
        }

        private void Awake()
        {
            texture = new RenderTexture(TextureSize, TextureSize, 24, RenderTextureFormat.ARGB32) { name = "Loadout Weapon", antiAliasing = 4 };
            turntable = new GameObject("Turntable").transform;
            turntable.SetParent(transform, false);
            // The weapon lies on the diagonal, as in the mockup, and the whole stage turns about the up axis.
            tilt = new GameObject("Tilt").transform;
            tilt.SetParent(turntable, false);
            tilt.localRotation = Quaternion.Euler(0f, 0f, -42f);
            var eye = new GameObject("Stage Camera");
            eye.transform.SetParent(transform, false);
            eye.transform.localPosition = new Vector3(0f, 0f, -3f);
            stageCamera = eye.AddComponent<Camera>();
            stageCamera.clearFlags = CameraClearFlags.SolidColor;
            stageCamera.backgroundColor = new Color(0f, 0f, 0f, 0f);
            stageCamera.fieldOfView = 22f;
            stageCamera.nearClipPlane = 0.5f;
            stageCamera.farClipPlane = 6f;
            stageCamera.targetTexture = texture;
            stageCamera.depth = -10f;
            var fill = new GameObject("Stage Light").AddComponent<Light>();
            fill.transform.SetParent(transform, false);
            fill.transform.localPosition = new Vector3(-1.2f, 1.4f, -2f);
            fill.type = LightType.Point;
            fill.range = 5f;
            fill.intensity = 1.2f;
        }

        /// <summary>Shows a copy of the weapon's model (the rack's part shown now), laid on the diagonal as the mockup has it.</summary>
        public void Show(GameObject weaponPart, string look)
        {
            Look = look ?? "";
            if (shown != null) Destroy(shown);
            shown = null;
            if (weaponPart == null) return;
            shown = Instantiate(weaponPart, tilt, false);
            shown.name = "Shown Weapon";
            shown.SetActive(true);
            // A picture, nothing more: no scripts, no colliders, no bodies.
            foreach (var behaviour in shown.GetComponentsInChildren<MonoBehaviour>(true)) Destroy(behaviour);
            foreach (var collider in shown.GetComponentsInChildren<Collider>(true)) Destroy(collider);
            foreach (var body in shown.GetComponentsInChildren<Rigidbody>(true)) Destroy(body);
            shown.transform.localPosition = Vector3.zero;
            shown.transform.localRotation = Quaternion.identity;
            Frame();
        }

        /// <summary>Changes the look shown while an aspect is previewed. The weapon has one model for now, so only the name is kept.</summary>
        public void SetLook(string look) => Look = look ?? "";

        // Centres the model on the stage and scales its longest side to 1.1 m, which fills the picture at 3 m.
        private void Frame()
        {
            var renderers = shown.GetComponentsInChildren<Renderer>(true);
            if (renderers.Length == 0) return;
            var spin = turntable.localRotation;
            turntable.localRotation = Quaternion.identity;
            var bounds = renderers[0].bounds;
            foreach (var each in renderers) bounds.Encapsulate(each.bounds);
            float size = Mathf.Max(bounds.size.x, Mathf.Max(bounds.size.y, bounds.size.z));
            if (size > 0.0001f)
            {
                float scale = 1.1f / size;
                var offset = tilt.InverseTransformVector(bounds.center - shown.transform.position);
                shown.transform.localScale *= scale;
                shown.transform.localPosition = -offset * scale;
            }
            turntable.localRotation = spin;
        }

        private void Update()
        {
            if (shown != null) turntable.Rotate(Vector3.up, TurnSpeed * Time.unscaledDeltaTime, Space.World);
        }

        private void OnDestroy()
        {
            if (stageCamera != null) stageCamera.targetTexture = null;
            if (texture != null)
            {
                texture.Release();
                Destroy(texture);
            }
        }
    }
}

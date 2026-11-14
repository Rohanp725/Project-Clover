using UnityEngine;

namespace Clover.Core
{
    /// <summary>
    /// A named camera position for review screenshots. Clover > Review > Capture Stations renders every station in the open
    /// scenes and compares each frame with review/shots/golden/[name].png, so a change that moves pixels gets noticed before
    /// anyone plays. Put one wherever a look matters: a vista, a room, a UI state.
    /// </summary>
    public class ShotStation : MonoBehaviour
    {
        [Tooltip("File name of the shot: letters, digits and underscores. Empty means the GameObject's name.")]
        public string shotName;
        [Range(10f, 120f)] public float fieldOfView = 60f;
        public int width = 1280;
        public int height = 720;

        public string Name => string.IsNullOrEmpty(shotName) ? gameObject.name : shotName;

        private void OnDrawGizmos()
        {
            Gizmos.color = new Color(0.2f, 0.9f, 0.4f, 0.8f);
            Gizmos.matrix = transform.localToWorldMatrix;
            Gizmos.DrawFrustum(Vector3.zero, fieldOfView, 2f, 0.1f, (float)width / Mathf.Max(1, height));
        }
    }
}

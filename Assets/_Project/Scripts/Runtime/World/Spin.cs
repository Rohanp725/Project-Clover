using UnityEngine;

namespace Clover.World
{
    /// <summary>Turns an object steadily about an axis of its own.</summary>
    public class Spin : MonoBehaviour
    {
        public Vector3 Axis = Vector3.up;
        public float DegreesPerSecond = 30f;

        private void Update() => transform.Rotate(Axis, DegreesPerSecond * Time.deltaTime, Space.Self);
    }
}

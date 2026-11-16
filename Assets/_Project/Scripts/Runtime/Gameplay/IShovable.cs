using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>Something a pull or a push can move along the ground.</summary>
    public interface IShovable
    {
        /// <summary>Adds a shove: metres, roughly, in the direction given.</summary>
        void Push(Vector3 impulse);
    }
}

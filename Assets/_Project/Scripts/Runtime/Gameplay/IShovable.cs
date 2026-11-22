using UnityEngine;

namespace Clover.Gameplay
{
    /// <summary>Something a pull or a push can move along the ground.</summary>
    public interface IShovable
    {
        /// <summary>
        /// Adds a push: metres, roughly, in the direction given. <paramref name="kind"/> says whether it is a shove the
        /// body's conditions answer: a blow's push is one when it is strong, a wind's first touch always is, a drag never.
        /// </summary>
        void Push(Vector3 impulse, PushKind kind = PushKind.Blow);
    }
}

using System;

namespace Clover.Core.Save
{
    /// <summary>
    /// A rest fire named so a later sitting can find it: its scene and the stable id its builder gives it
    /// (IMPLEMENTATION.md 3.14). Empty until the player first leaves a fire for the hub.
    /// </summary>
    [Serializable]
    public class RestRef
    {
        public string Scene = "";
        public string RestId = "";

        /// <summary>True once a fire has been kept.</summary>
        public bool IsSet => !string.IsNullOrEmpty(Scene);
    }
}

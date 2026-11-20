using UnityEngine;

namespace Clover.Core
{
    /// <summary>
    /// The combat playtest's one switch (docs/GAME.md, "The evolving combat playtest"). While it is on: resting gives
    /// unlimited Shards, so every mark can be bought, the gods' gifts included; the chart can be walked into every tree
    /// and branch; a fall returns the player to the last rest fire with nothing lost; and the pause menu and the chart
    /// offer the guide to effects, the marks' details and the way back to the Proving Ground. Off, the game is as it
    /// always was.
    ///
    /// It is on in a build made with the CLOVER_PLAYTEST define (Clover > Build > Combat Playtest), in the editor when
    /// Clover > Play > Playtest Mode is ticked, and whenever the Proving Ground is entered, since that place exists only
    /// for the playtest.
    /// </summary>
    public static class PlaytestMode
    {
        /// <summary>The playtest's starting scene and the home every way out of a level leads to while the mode is on.</summary>
        public const string ProvingGroundScene = "ProvingGround";

        /// <summary>What a rest tops the carried Shards up to: more than every mark on the chart costs together.</summary>
        public const int RestShards = 9999;

        /// <summary>
        /// What a rest tops the Memories up to (the Way's currency, phase 5 of the playtest), so the weapon trees and the
        /// Body are as free as the element trees. Read through <see cref="MemoriesAfterRest"/> by the wallet's rest.
        /// </summary>
        public const int RestMemories = 9999;

        /// <summary>True in a build made for the combat playtest.</summary>
        public static bool Built =>
#if CLOVER_PLAYTEST
            true;
#else
            false;
#endif

        /// <summary>
        /// Set by the editor's menu (Clover > Play > Playtest Mode) and read as each play begins. It is not reset with the
        /// play: it is the editor's setting, kept across plays.
        /// </summary>
        public static bool EditorSwitch;

        public static bool On { get; private set; }

        // Domain reload is off: each play decides afresh from the build and the editor's setting.
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetOnLoad() => ResetToSetting();

        /// <summary>
        /// Back to what the build and the editor's setting say. The editor calls it when a play ends, so a Proving Ground
        /// visit does not leave the mode on for the edit-mode tests that follow (it made one of them loop forever).
        /// </summary>
        public static void ResetToSetting() => On = Built || EditorSwitch;

        /// <summary>Turns the mode on or off for the rest of this play: the Proving Ground turns it on, tests and probes either way.</summary>
        public static void Set(bool on) => On = on;

        /// <summary>Shards after a rest: unlimited in the playtest, unchanged otherwise.</summary>
        public static int ShardsAfterRest(int carried) => On ? Mathf.Max(carried, RestShards) : carried;

        /// <summary>Memories after a rest: unlimited in the playtest, unchanged otherwise.</summary>
        public static int MemoriesAfterRest(int carried) => On ? Mathf.Max(carried, RestMemories) : carried;
    }
}

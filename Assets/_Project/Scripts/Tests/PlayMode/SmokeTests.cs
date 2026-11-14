using System.Collections;
using NUnit.Framework;
using UnityEngine.SceneManagement;
using UnityEngine.TestTools;

namespace Clover.Tests
{
    /// <summary>The cheapest proof the game boots: the first scene in Build Settings runs sixty frames without an error.</summary>
    public class SmokeTests
    {
        [UnityTest]
        public IEnumerator FirstBuildSceneRunsSixtyFramesWithoutErrors()
        {
            if (SceneManager.sceneCountInBuildSettings == 0)
                Assert.Ignore("No scene in Build Settings yet; add one with 'unity command scene add_to_build'.");

            var load = SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
            while (load != null && !load.isDone) yield return null;
            for (int i = 0; i < 60; i++) yield return null;
            LogAssert.NoUnexpectedReceived();
        }
    }
}

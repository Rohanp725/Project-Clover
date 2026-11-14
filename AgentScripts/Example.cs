// Run with: unity command script run --file AgentScripts/Example.cs --entry Example.Run
// A template for bulk work in the editor without a domain reload. Replace the body, keep the shape.
using UnityEngine;
using UnityEngine.SceneManagement;

public static class Example
{
    public static string Run()
    {
        int cameras = Object.FindObjectsByType<Camera>().Length;
        int lights = Object.FindObjectsByType<Light>().Length;
        return $"{SceneManager.GetActiveScene().name}: {cameras} camera(s), {lights} light(s)";
    }
}

using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BreathIsland.Tests
{
    public sealed class SceneBootstrapTests
    {
        [UnityTest]
        public IEnumerator CompositionRootBuildsWorldCameraAndExploreHud()
        {
            var root = new GameObject("Bootstrap smoke test");
            try
            {
                var game = root.AddComponent<BreathIslandGame>();
                yield return null;
                yield return null;
                Assert.That(game.Phase, Is.EqualTo(GamePhase.Explore));
                Assert.That(game.SaveData, Is.Not.Null);
                Assert.That(root.GetComponentInChildren<ThirdPersonMotor>(), Is.Not.Null);
                Assert.That(root.GetComponentInChildren<Camera>(), Is.Not.Null);
                Assert.That(root.GetComponent<ProceduralSeaAudio>(), Is.Not.Null);
                LogAssert.NoUnexpectedReceived();
            }
            finally
            {
                // Destroy before Editor shutdown so this read-only smoke test never auto-saves user data.
                Object.Destroy(root);
            }
            yield return null;
        }
    }
}

using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
namespace BreathIsland.Tests
{
    public sealed class LifeInputTests
    {
        [UnityTest] public IEnumerator EquipmentKeyboardAndPauseRouteThroughCompositionRoot()
        {
            string previous = Environment.GetEnvironmentVariable("BREATH_ISLAND_SAVE_ROOT");
            string path = Path.Combine(Application.temporaryCachePath, "life-input-" + Guid.NewGuid().ToString("N"));
            Environment.SetEnvironmentVariable("BREATH_ISLAND_SAVE_ROOT", path);
            var root = new GameObject("Life input test"); var keyboard = InputSystem.AddDevice<Keyboard>();
            try
            {
                var game = root.AddComponent<BreathIslandGame>(); yield return null;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.I)); yield return null; yield return null;
                Assert.That(game.Phase, Is.EqualTo(GamePhase.Equipment));
                InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape)); yield return null; yield return null;
                Assert.That(game.Phase, Is.EqualTo(GamePhase.Explore));
                InputSystem.QueueStateEvent(keyboard, new KeyboardState()); yield return null;
                InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.Escape)); yield return null; yield return null;
                Assert.That(game.Phase, Is.EqualTo(GamePhase.Pause));
            }
            finally
            {
                UnityEngine.Object.Destroy(root); InputSystem.RemoveDevice(keyboard);
                Environment.SetEnvironmentVariable("BREATH_ISLAND_SAVE_ROOT", previous);
            }
            yield return null;
            if (Directory.Exists(path)) Directory.Delete(path, true);
        }
    }
}

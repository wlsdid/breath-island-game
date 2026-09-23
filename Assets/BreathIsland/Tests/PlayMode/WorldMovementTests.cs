using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace BreathIsland.Tests
{
    public sealed class WorldMovementTests
    {
        [UnityTest]
        public IEnumerator GroundMatchesVisibleSurfaceAndPlayerCanWalk()
        {
            var root = new GameObject("Terrain movement regression");
            var keyboard = InputSystem.AddDevice<Keyboard>();
            try
            {
                var world = ProceduralWorld.Build(root.transform);
                yield return null;
                Physics.SyncTransforms();
                foreach (string name in new[] { "Island Rock", "Island Grass", "Beach" })
                {
                    var terrain = root.transform.Find(name);
                    var visible = terrain.GetComponent<Renderer>().bounds;
                    var ray = new Ray(visible.center + Vector3.up * 20, Vector3.down);
                    Assert.That(terrain.GetComponent<Collider>().Raycast(ray, out var hit, 40), Is.True, name);
                    Assert.That(hit.point.y, Is.EqualTo(visible.max.y).Within(.03f), name + " collision surface");
                }
                yield return new WaitForSeconds(.7f);
                var before = world.player.transform.position;
                Assert.That(before.y, Is.LessThan(1.2f), "Player must settle onto the visible grass");
                // Reassert the virtual keyboard each frame: Editor focus changes can reset devices.
                for (int frame = 0; frame < 12; frame++)
                {
                    InputSystem.QueueStateEvent(keyboard, new KeyboardState(Key.W));
                    InputSystem.Update();
                    Assert.That(GameInput.Move.y, Is.EqualTo(1), "Virtual movement input");
                    yield return null;
                }
                InputSystem.QueueStateEvent(keyboard, new KeyboardState());
                yield return null;
                var after = world.player.transform.position;
                Debug.Log($"Movement diagnostic: {before} -> {after}, canMove={world.motor.CanMove}, dt={Time.deltaTime}, forward={world.camera.transform.forward}, grounded={world.player.GetComponent<CharacterController>().isGrounded}");
                Assert.That(Vector3.Distance(new Vector3(before.x, 0, before.z), new Vector3(after.x, 0, after.z)), Is.GreaterThan(.7f));
                Assert.That(after.y, Is.InRange(.5f, 1.2f), "Walking must not climb an invisible dome");
            }
            finally
            {
                Object.Destroy(root);
                InputSystem.RemoveDevice(keyboard);
            }
            yield return null;
        }
    }
}

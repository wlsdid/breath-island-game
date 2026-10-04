using System.Collections;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace BreathIsland.Tests
{
    public sealed class CharacterLocomotionTests
    {
        private GameObject root, player;
        private ThirdPersonMotor motor;
        private Keyboard keyboard;
        [UnitySetUp] public IEnumerator Setup()
        {
            root = new GameObject("Locomotion test");
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(root.transform); floor.transform.position = new Vector3(0, -.1f, 0);
            floor.transform.localScale = new Vector3(30, .2f, 30);
            player = new GameObject("Player", typeof(CharacterController)); player.transform.SetParent(root.transform);
            var controller = player.GetComponent<CharacterController>();
            controller.height = 1.65f; controller.radius = .42f; controller.center = Vector3.up * .82f;
            player.transform.position = Vector3.up * .2f;
            motor = player.AddComponent<ThirdPersonMotor>(); motor.SetCamera(root.transform);
            player.AddComponent<CharacterPresentation>().Initialize(IslandLifeCatalog.Load());
            keyboard = InputSystem.AddDevice<Keyboard>(); Physics.SyncTransforms();
            yield return new WaitForSeconds(.6f);
        }
        [UnityTearDown] public IEnumerator Cleanup()
        { InputSystem.RemoveDevice(keyboard); Object.Destroy(root); yield return null; }
        private void Keys(params Key[] keys)
        { InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys)); }
        [UnityTest] public IEnumerator VisibleSolesMeetFloorAndWalkingUsesApprovedSpeed()
        {
            Assert.That(motor.Grounded, Is.True);
            float minimum = float.PositiveInfinity;
            foreach (var renderer in player.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var mesh = new Mesh(); renderer.BakeMesh(mesh);
                foreach (var v in mesh.vertices) minimum = Mathf.Min(minimum, renderer.transform.TransformPoint(v).y);
                Object.Destroy(mesh);
            }
            Assert.That(minimum, Is.InRange(-.04f, .10f), "Visible soles, not animation bounds, must touch the floor.");
            Assert.That(motor.speed, Is.EqualTo(5.2f * .55f).Within(.001f));
            var start = player.transform.position; float duration=0;
            Keys(Key.W);
            while(duration < .5f) { yield return null; duration += Time.deltaTime; }
            Keys();
            Assert.That(player.transform.position.z-start.z, Is.EqualTo(motor.speed * duration).Within(.15f));
        }
        [UnityTest] public IEnumerator JumpLandsAndMenuBlocksRepeatedOrClosingInput()
        {
            float floor = player.transform.position.y;
            Keys(Key.Space); yield return null; Keys();
            Assert.That(motor.VerticalVelocity, Is.GreaterThan(0));
            float peak = floor;
            float end = Time.time + 2;
            for(int i=0;Time.time<end;i++)
            {
                peak = Mathf.Max(peak,player.transform.position.y);
                if(i==8)Keys(Key.Space); if(i==9)Keys();
                yield return null;
            }
            Assert.That(peak-floor, Is.InRange(.55f, .95f), "Repeated press in air must not double jump.");
            Assert.That(motor.Grounded, Is.True);
            Assert.That(player.transform.position.y, Is.EqualTo(floor).Within(.04f));
            motor.CanMove=false; Keys(Key.Space); yield return null; Keys();
            Assert.That(player.transform.position.y, Is.EqualTo(floor).Within(.04f));
            motor.CanMove=true;motor.BlockActions();Keys(Key.Space);yield return null;Keys();
            Assert.That(motor.VerticalVelocity, Is.LessThanOrEqualTo(0));
        }
    }
}

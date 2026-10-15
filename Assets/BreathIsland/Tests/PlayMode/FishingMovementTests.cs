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
    public sealed class FishingMovementTests
    {
        [UnityTest] public IEnumerator MovingLookingAndPauseKeepCastInWorldAndNeverJump()
        {
            var root=new GameObject("Fishing movement test");var world=ProceduralWorld.Build(root.transform);
            var path=Path.Combine(Application.temporaryCachePath,"pier-"+Guid.NewGuid().ToString("N"));
            var context=new GameContext(GameSaveData.CreateDefault(),new SaveService(path),world,DefinitionCatalog.CreateVerticalSlice(),_=>{});
            var phases=new GamePhaseController(world);var fishing=new FishingSystem(context,phases,()=>{});
            root.AddComponent<FishingPresentation>().Initialize(world,fishing,phases);
            var keyboard=InputSystem.AddDevice<Keyboard>();var mouse=InputSystem.AddDevice<Mouse>();
            try
            {
                world.motor.Teleport(new Vector3(0,.93f,23));fishing.Start();yield return new WaitForSeconds(.25f);
                fishing.Cast();yield return new WaitForSeconds(1);
                var bobber=root.transform.Find("FishingRig(Clone)/Bobber");var anchor=bobber.position;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W,Key.Space));yield return new WaitForSeconds(1);
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());yield return null;
                Assert.That(world.player.transform.position.z,Is.InRange(23.5f,24.46f));Assert.That(world.motor.VerticalVelocity,Is.LessThanOrEqualTo(0));
                var rotation=world.camera.transform.rotation;
                InputSystem.QueueStateEvent(mouse,new MouseState{buttons=2,delta=new Vector2(120,20)});yield return null;yield return new WaitForEndOfFrame();
                Assert.That(Quaternion.Angle(rotation,world.camera.transform.rotation),Is.GreaterThan(1));
                Assert.That(Vector2.Distance(new Vector2(anchor.x,anchor.z),new Vector2(bobber.position.x,bobber.position.z)),Is.LessThan(.02f));
                InputSystem.QueueStateEvent(mouse,new MouseState());
                phases.Set(GamePhase.Pause);var position=world.player.transform.position;
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.A));yield return new WaitForSeconds(.2f);
                Assert.That(world.player.transform.position,Is.EqualTo(position));InputSystem.QueueStateEvent(keyboard,new KeyboardState());
                phases.Cancel();Assert.That(world.motor.FishingMovement,Is.True);
                phases.Set(GamePhase.Explore);Assert.That(world.motor.FishingMovement,Is.False);
            }
            finally{InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(mouse);UnityEngine.Object.Destroy(root);if(Directory.Exists(path))Directory.Delete(path,true);Cursor.lockState=CursorLockMode.None;}
        }
    }
}

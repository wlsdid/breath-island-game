using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace BreathIsland.Tests
{
    public sealed class GatheringProgressTests
    {
        [UnityTest] public IEnumerator HoldCancelsOnReleaseMenuDistanceAndFacingThenRewardsOnce()
        {
            var directory=Path.Combine(Application.temporaryCachePath,"gather-work-"+Guid.NewGuid().ToString("N"));
            var player=new GameObject("Gather test",typeof(CharacterController),typeof(ThirdPersonMotor),typeof(IslandCameraRig));
            var catalog=UnityEngine.Object.Instantiate(IslandLifeCatalog.Load());catalog.nodes=catalog.nodes.Select(n=>UnityEngine.Object.Instantiate(n)).ToArray();
            try
            {
                var world=new WorldHandles{player=player,motor=player.GetComponent<ThirdPersonMotor>(),cameraRig=player.GetComponent<IslandCameraRig>()};
                var context=new GameContext(GameSaveData.CreateDefault(),new SaveService(directory),world,DefinitionCatalog.CreateVerticalSlice(),_=>{});
                var phases=new GamePhaseController(world);var achievements=new AchievementSystem(context,catalog);
                var equipment=new EquipmentSystem(context,phases,catalog,achievements);var land=new LandActivitySystem(context,phases,catalog,equipment,achievements);
                var node=catalog.nodes.First(n=>n.resource==LifeResource.Herb);node.gatherSeconds=1; // Isolated fixture, not content balance.
                player.transform.position=node.position+Vector3.back;player.transform.forward=Vector3.forward;
                phases.Set(GamePhase.Explore);yield return null;yield return null;yield return null;
                Assert.That(land.Harvest(node.id),Is.False,"A press cannot bypass the configured hold.");
                Assert.That(land.TickHeldGather(.4f,true),Is.False);Assert.That(land.Progress(node),Is.EqualTo(.4f).Within(.001));
                land.TickHeldGather(.1f,false);Assert.That(land.Progress(node),Is.Zero);
                land.TickHeldGather(.4f,true);phases.OpenInventory();Assert.That(land.TickHeldGather(1,true),Is.False);Assert.That(land.Progress(node),Is.Zero);
                phases.CloseInventory();yield return null;yield return null;yield return null;
                player.transform.forward=Vector3.back;Assert.That(land.TickHeldGather(1,true),Is.False);
                player.transform.forward=Vector3.forward;player.transform.position=node.position+Vector3.back*4;Assert.That(land.TickHeldGather(1,true),Is.False);
                player.transform.position=node.position+Vector3.back;
                Assert.That(land.TickHeldGather(.6f,true),Is.False);Assert.That(context.Save.resources.herbs,Is.Zero);
                Assert.That(land.TickHeldGather(.4f,true),Is.True);Assert.That(context.Save.resources.herbs,Is.EqualTo(node.yield));
                Assert.That(land.TickHeldGather(1,true),Is.False);Assert.That(context.Save.resources.herbHarvests,Is.EqualTo(1));
                phases.Set(GamePhase.Home);Assert.That(land.Rest(),Is.True);Assert.That(land.Depleted(node.id),Is.False);Assert.That(land.Progress(node),Is.Zero);
            }
            finally
            {
                foreach(var node in catalog.nodes)UnityEngine.Object.DestroyImmediate(node);UnityEngine.Object.DestroyImmediate(catalog);
                UnityEngine.Object.DestroyImmediate(player);if(Directory.Exists(directory))Directory.Delete(directory,true);
            }
        }
    }
}

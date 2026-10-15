using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace BreathIsland.Tests
{
    public sealed class FishingBiteTests
    {
        private GameObject player;
        private string path;
        private GameContext context;
        private GamePhaseController phases;
        private FishingSystem fishing;
        [SetUp] public void Setup()
        {
            path=Path.Combine(Path.GetTempPath(),"bite-"+Guid.NewGuid().ToString("N"));
            player=new GameObject("Bite test",typeof(CharacterController),typeof(ThirdPersonMotor),typeof(IslandCameraRig));
            var world=new WorldHandles{player=player,motor=player.GetComponent<ThirdPersonMotor>(),cameraRig=player.GetComponent<IslandCameraRig>()};
            context=new GameContext(GameSaveData.CreateDefault(),new SaveService(path),world,DefinitionCatalog.CreateVerticalSlice(),_=>{});
            phases=new GamePhaseController(world);fishing=new FishingSystem(context,phases,()=>phases.Set(GamePhase.PredatorCombat));
        }
        [TearDown] public void Cleanup()
        {UnityEngine.Object.DestroyImmediate(player);if(Directory.Exists(path))Directory.Delete(path,true);Cursor.lockState=CursorLockMode.None;}
        private void Bite(){fishing.Start();fishing.Cast();fishing.Tick(3);Assert.That(fishing.Stage,Is.EqualTo(FishingStage.Bite));}
        [Test] public void ReactionIsRequiredAndRepeatedHookCannotReelOrConsumeBait()
        {
            int bait=context.Save.inventory.bait;Bite();
            Assert.That(fishing.ReactionRemaining,Is.EqualTo(3));Assert.That(fishing.Progress,Is.Zero);
            fishing.Reel();fishing.Ease();Assert.That(fishing.Stage,Is.EqualTo(FishingStage.Bite));
            fishing.Hook();var fish=fishing.ActiveFish;fishing.Hook();
            Assert.That(fishing.Stage,Is.EqualTo(FishingStage.Hooked));Assert.That(fishing.Progress,Is.EqualTo(8));
            Assert.That(fishing.Tension,Is.EqualTo(34));Assert.That(fishing.ActiveFish,Is.SameAs(fish));
            Assert.That(context.Save.inventory.bait,Is.EqualTo(bait-1));
        }
        [Test] public void DeadlineMissUsesOnlyCastBaitAndRetryStartsFresh()
        {
            int bait=context.Save.inventory.bait, coins=context.Save.inventory.coins;Bite();
            fishing.Tick(3);fishing.Hook();fishing.Reel();
            Assert.That(fishing.Stage,Is.EqualTo(FishingStage.Failed));Assert.That(fishing.MissedBite,Is.True);
            Assert.That(fishing.ActiveFish,Is.Null);Assert.That(context.Save.inventory.bait,Is.EqualTo(bait-1));
            Assert.That(context.Save.inventory.coins,Is.EqualTo(coins));Assert.That(context.Save.inventory.totalCatchCount,Is.Zero);
            fishing.Retry();fishing.Cast();fishing.Tick(3);
            Assert.That(fishing.Stage,Is.EqualTo(FishingStage.Bite));Assert.That(fishing.MissedBite,Is.False);
            Assert.That(fishing.ReactionRemaining,Is.EqualTo(3));Assert.That(context.Save.inventory.bait,Is.EqualTo(bait-2));
        }
        [Test] public void PauseFreezesReactionAndRejectsHookUntilResumed()
        {
            Bite();fishing.Tick(1);phases.Set(GamePhase.Pause);fishing.Tick(20);fishing.Hook();
            Assert.That(fishing.Stage,Is.EqualTo(FishingStage.Bite));Assert.That(fishing.ReactionRemaining,Is.EqualTo(2));
            phases.Cancel();fishing.Tick(1.99f);fishing.Hook();Assert.That(fishing.Stage,Is.EqualTo(FishingStage.Hooked));
        }
        [Test] public void PrematureOrCancelledHookIsIgnoredAndBothLocalesHavePrompts()
        {
            fishing.Start();fishing.Hook();Assert.That(fishing.Stage,Is.EqualTo(FishingStage.Ready));
            fishing.Cast();fishing.Hook();Assert.That(fishing.Stage,Is.EqualTo(FishingStage.Waiting));
            fishing.Tick(3);phases.Set(GamePhase.Explore);fishing.Hook();Assert.That(fishing.Stage,Is.EqualTo(FishingStage.Bite));
            fishing.Start();Assert.That(fishing.ActiveFish,Is.Null);Assert.That(fishing.ReactionRemaining,Is.Zero);
            foreach(bool ko in new[]{true,false})foreach(string key in new[]{"fishing_bite_now","fishing_hook_hint","fishing_bite_time","fishing_hook_action","fishing_bite_missed"})
                Assert.That(GameText.Get(key,ko),Is.Not.Empty.And.Not.EqualTo(key));
        }
    }
}

using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace BreathIsland.Tests
{
    public sealed class ToolUseRulesTests
    {
        private GameObject player;
        private string directory;
        private GameContext context;
        private GamePhaseController phases;
        private EquipmentSystem equipment;
        private LandActivitySystem land;
        private ToolUseSystem tools;
        private ResourceNodeDefinition tree;
        private IslandLifeCatalog catalog;
        [SetUp] public void Setup()
        {
            directory=Path.Combine(Path.GetTempPath(),"tool-use-"+Guid.NewGuid().ToString("N"));
            player=new GameObject("Tool test",typeof(CharacterController),typeof(ThirdPersonMotor),typeof(IslandCameraRig));
            var world=new WorldHandles{player=player,motor=player.GetComponent<ThirdPersonMotor>(),cameraRig=player.GetComponent<IslandCameraRig>()};
            context=new GameContext(GameSaveData.CreateDefault(),new SaveService(directory),world,DefinitionCatalog.CreateVerticalSlice(),_=>{});
            phases=new GamePhaseController(world);catalog=UnityEngine.Object.Instantiate(IslandLifeCatalog.Load());
            catalog.nodes=catalog.nodes.Select(n=>UnityEngine.Object.Instantiate(n)).ToArray();var achievements=new AchievementSystem(context,catalog);
            equipment=new EquipmentSystem(context,phases,catalog,achievements);land=new LandActivitySystem(context,phases,catalog,equipment,achievements);
            tools=new ToolUseSystem(context,phases,equipment,land,catalog);tree=catalog.nodes.First(n=>n.resource==LifeResource.Wood);
            player.transform.position=tree.position+Vector3.back;player.transform.forward=Vector3.forward;
        }
        [TearDown] public void Cleanup(){foreach(var node in catalog.nodes)UnityEngine.Object.DestroyImmediate(node);UnityEngine.Object.DestroyImmediate(catalog);UnityEngine.Object.DestroyImmediate(player);Directory.Delete(directory,true);}
        [Test] public void ConfiguredHitsAwardOnlyOnCompletionAndRestoreClearsPartialWork()
        {
            Assert.That(tree.requiredHits,Is.EqualTo(3));equipment.SelectTool(LifeTool.Axe);
            tools.Use();tools.Tick(1);Assert.That(land.Progress(tree),Is.EqualTo(1f/3).Within(.001f));Assert.That(context.Save.resources.wood,Is.Zero);
            player.transform.forward=Vector3.back;tools.Use();tools.Tick(1);Assert.That(land.Progress(tree),Is.EqualTo(1f/3).Within(.001f));
            land.Restore();Assert.That(land.Progress(tree),Is.Zero);player.transform.forward=Vector3.forward;
            for(int i=0;i<2;i++){tools.Use();tools.Tick(1);Assert.That(context.Save.resources.wood,Is.Zero);}
            tools.Use();tools.Tick(1);Assert.That(context.Save.resources.wood,Is.EqualTo(tree.yield));
            Assert.That(context.Save.resources.woodHarvests,Is.EqualTo(1));Assert.That(land.Progress(tree),Is.Zero);
            tools.Use();tools.Tick(1);Assert.That(context.Save.resources.woodHarvests,Is.EqualTo(1));
        }
        [Test] public void SwingAwardsOnceAtImpactAndNeverOnStart()
        {
            equipment.Equip("sword");equipment.SelectTool(LifeTool.Axe);
            Assert.That(tools.Use(),Is.True);Assert.That(context.Save.resources.wood,Is.Zero);
            Assert.That(tools.Use(),Is.False);tools.Tick(.26f);Assert.That(context.Save.resources.wood,Is.Zero);
            tools.Tick(1);tools.Use();tools.Tick(1);Assert.That(context.Save.resources.wood,Is.Zero);
            tools.Use();tools.Tick(.26f);Assert.That(context.Save.resources.wood,Is.EqualTo(3));
            tools.Tick(1);tools.Use();tools.Tick(1);Assert.That(context.Save.resources.wood,Is.EqualTo(3));
            Assert.That(equipment.Job.id,Is.EqualTo("warrior"));Assert.That(equipment.Tool,Is.EqualTo(LifeTool.Axe));
        }
        [Test] public void AirWrongToolBehindAndDistanceGiveNoReward()
        {
            equipment.SelectTool(LifeTool.Pickaxe);tools.Use();tools.Tick(1);Assert.That(context.Save.resources.wood,Is.Zero);
            equipment.SelectTool(LifeTool.Axe);player.transform.forward=Vector3.back;tools.Use();tools.Tick(1);Assert.That(context.Save.resources.wood,Is.Zero);
            player.transform.forward=Vector3.forward;player.transform.position=tree.position+Vector3.back*3;tools.Use();tools.Tick(1);Assert.That(context.Save.resources.wood,Is.Zero);
            player.transform.position=new Vector3(50,1,50);Assert.That(tools.Use(),Is.True);tools.Tick(1);Assert.That(context.Save.resources.wood,Is.Zero);
        }
        [Test] public void OpeningMenuOrChangingToolCancelsPendingImpact()
        {
            equipment.SelectTool(LifeTool.Axe);tools.Use();phases.OpenInventory();tools.Tick(1);Assert.That(context.Save.resources.wood,Is.Zero);
            Assert.That(tools.Use(),Is.False);phases.CloseInventory();tools.Use();equipment.SelectTool(LifeTool.Hammer);tools.Tick(1);Assert.That(context.Save.resources.wood,Is.Zero);
        }
        [Test] public void EquippedToolCannotAutomaticallySwitchToWeaponWhenAttacking()
        {
            var catalog=IslandLifeCatalog.Load();var combat=new TrainingCombatSystem(context,phases,equipment,player.transform,new JobMasterySystem(context),new AchievementSystem(context,catalog));
            equipment.Equip("sword");equipment.SelectTool(LifeTool.Axe);
            Assert.That(combat.Attack(false),Is.False);Assert.That(combat.Health,Is.EqualTo(100));Assert.That(equipment.Tool,Is.EqualTo(LifeTool.Axe));
        }
    }
}

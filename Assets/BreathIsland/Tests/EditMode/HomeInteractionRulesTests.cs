using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace BreathIsland.Tests
{
    public sealed class HomeInteractionRulesTests
    {
        private GameObject root,target;
        private string path;
        private GameContext context;
        private GamePhaseController phases;
        private IslandLifeCatalog catalog;
        private HousingSystem housing;
        private HomeStorageSystem storage;
        private SaveService saves;
        [SetUp] public void Setup()
        {
            path=Path.Combine(Path.GetTempPath(),"home-v4-"+Guid.NewGuid().ToString("N"));saves=new SaveService(path);
            root=new GameObject("Home rules",typeof(CharacterController),typeof(ThirdPersonMotor),typeof(IslandCameraRig));
            var world=new WorldHandles{player=root,motor=root.GetComponent<ThirdPersonMotor>(),cameraRig=root.GetComponent<IslandCameraRig>()};
            context=new GameContext(GameSaveData.CreateDefault(),saves,world,DefinitionCatalog.CreateVerticalSlice(),_=>{});
            phases=new GamePhaseController(world);catalog=IslandLifeCatalog.Load();housing=new HousingSystem(context,phases,catalog);
            storage=new HomeStorageSystem(context,phases,housing,catalog);context.Save.housing.inside=true;
            root.transform.position=HousingSystem.InteriorOrigin+new Vector3(1.5f,.05f,1.5f);
            target=new GameObject("Chest",typeof(BoxCollider),typeof(HomeFurnitureTarget));target.transform.position=HousingSystem.InteriorOrigin+new Vector3(1.5f,.5f,3.5f);
            target.GetComponent<HomeFurnitureTarget>().InstanceId="a";
            context.Save.housing.placed.Add(new PlacedFurniture{instanceId="a",furnitureId="chest",x=1,z=3});
            context.Save.housing.placed.Add(new PlacedFurniture{instanceId="b",furnitureId="chest",x=4,z=3});
            phases.Set(GamePhase.Home);Physics.SyncTransforms();Assert.That(storage.Open("a"),Is.True);
        }
        [TearDown] public void Cleanup(){storage.Dispose();UnityEngine.Object.DestroyImmediate(target);UnityEngine.Object.DestroyImmediate(root);if(Directory.Exists(path))Directory.Delete(path,true);}
        [Test] public void ChestLimitsStacksAndPreservesHarvestAndMasteryCounters()
        {
            context.Save.resources.wood=1700;context.Save.resources.woodHarvests=7;context.Save.resources.totalWood=1800;
            Assert.That(storage.Deposit("wood",false,int.MaxValue),Is.EqualTo(1584));
            Assert.That(storage.Current.slots.Count,Is.EqualTo(16));Assert.That(storage.Current.slots.All(s=>s.count==99),Is.True);
            Assert.That(storage.Deposit("wood",false,1),Is.Zero);Assert.That(context.Save.resources.wood,Is.EqualTo(116));
            Assert.That(storage.Withdraw(0,27),Is.EqualTo(27));Assert.That(storage.Deposit("wood",false,10),Is.EqualTo(10));
            Assert.That(storage.Current.slots[0].count,Is.EqualTo(82));
            Assert.That(context.Save.resources.woodHarvests,Is.EqualTo(7));Assert.That(context.Save.resources.totalWood,Is.EqualTo(1800));
            Assert.That(context.Save.resources.wood+storage.Current.slots.Sum(s=>s.count),Is.EqualTo(1700));
        }
        [Test] public void FurnitureUsesOneSlotAndNonemptyChestCannotBeReclaimed()
        {
            housing.Stock("chair").count=20;housing.Stock("chest").count=1;
            Assert.That(storage.Deposit("chest",true,1),Is.EqualTo(1));
            Assert.That(storage.Deposit("chair",true,20),Is.EqualTo(15));Assert.That(housing.Available("chair"),Is.EqualTo(5));
            phases.Set(GamePhase.HomeDecorate);Assert.That(housing.Recall("a"),Is.False);
            phases.Set(GamePhase.Home);Assert.That(storage.Open("a"),Is.True);
            for(int i=0;i<16;i++)Assert.That(storage.Withdraw(i,int.MaxValue),Is.EqualTo(1));
            phases.Set(GamePhase.HomeDecorate);Assert.That(housing.Recall("a"),Is.True);Assert.That(housing.Recall("a"),Is.False);
            Assert.That(housing.Available("chair"),Is.EqualTo(20));Assert.That(housing.Available("chest"),Is.EqualTo(2));
        }
        [Test] public void ChestIdentityInvalidCommandsAndClosedMenusCannotTransfer()
        {
            context.Save.resources.stone=9;Assert.That(storage.Deposit("stone",false,4),Is.EqualTo(4));
            Assert.That(storage.Deposit("sword",false,1),Is.Zero);Assert.That(storage.Deposit("stone",false,-1),Is.Zero);
            Assert.That(storage.Withdraw(-1,1),Is.Zero);Assert.That(storage.Withdraw(16,1),Is.Zero);
            phases.Set(GamePhase.Home);Assert.That(storage.Withdraw(0,1),Is.Zero);
            Assert.That(storage.Open("b"),Is.False,"Cannot open another chest through the current target");
            target.GetComponent<HomeFurnitureTarget>().InstanceId="b";
            Assert.That(storage.Open("b"),Is.True);Assert.That(storage.Current.slots.Sum(s=>s.count),Is.Zero);
            Assert.That(storage.Deposit("stone",false,5),Is.EqualTo(5));
            Assert.That(context.Save.housing.chests.Single(c=>c.instanceId=="a").slots[0].count,Is.EqualTo(4));
        }
        [TestCase(-1)] [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void AllSlotsRestoreIndependentChestContents(int slot)
        {
            context.Save.resources.herbs=120;storage.Deposit("herbs",false,120);housing.Stock("bed").count=1;storage.Deposit("bed",true,1);
            context.SaveTo(slot);context.LoadFrom(slot);
            Assert.That(storage.OpenId,Is.Null);Assert.That(context.Save.housing.chests.Single().slots.Sum(s=>s.count),Is.EqualTo(121));
            Assert.That(context.Save.resources.herbs,Is.Zero);Assert.That(context.Save.housing.stock.Single().count,Is.Zero);
        }
        [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void LegacyMigrationAddsEmptyStorageWithoutRewriting(int version)
        {
            string json="{\"version\":"+version+",\"inventory\":{\"coins\":123},\"housing\":{\"wallFinishId\":\"stone\",\"floorFinishId\":\"tiles\",\"placed\":[{\"instanceId\":\"old\",\"furnitureId\":\"chair\",\"x\":1,\"z\":3,\"turns\":2}]}}";
            File.WriteAllText(saves.PathForSlot(0),json);var loaded=saves.Load(0);
            Assert.That(loaded.version,Is.EqualTo(GameSaveData.CurrentVersion));Assert.That(loaded.inventory.coins,Is.EqualTo(123));Assert.That(loaded.housing.chests,Is.Empty);
            Assert.That(loaded.housing.placed.Single().turns,Is.EqualTo(2));Assert.That(loaded.housing.wallFinishId,Is.EqualTo(version==3?"stone":"wood_dark"));
            Assert.That(File.ReadAllText(saves.PathForSlot(0)),Is.EqualTo(json));
        }
        [Test] public void PlacementRejectsPlayerOverlapAndSafeRecoveryKeepsValidPosition()
        {
            phases.Set(GamePhase.Home);housing.Stock("chair").count=1;housing.Select("chair");housing.Move(1,1);
            Assert.That(housing.IsValid,Is.False);housing.Move(6,6);Assert.That(housing.IsValid,Is.True);
            var valid=HomeNavigation.Arrival;Assert.That(HomeNavigation.Safe(valid,housing.State.placed,catalog),Is.EqualTo(valid));
            var blocked=HousingSystem.InteriorOrigin+new Vector3(1.5f,.1f,3.5f);
            Assert.That(HomeNavigation.Clear(blocked,housing.State.placed,catalog),Is.False);
            Assert.That(HomeNavigation.Clear(HomeNavigation.Safe(blocked,housing.State.placed,catalog),housing.State.placed,catalog),Is.True);
        }
    }
}

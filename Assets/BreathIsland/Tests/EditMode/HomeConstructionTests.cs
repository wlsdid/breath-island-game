using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;
namespace BreathIsland.Tests
{
    public sealed class HomeConstructionTests
    {
        private GameObject player;
        private string path; private SaveService saves;
        private GameContext context;
        private GamePhaseController phases;
        private HousingSystem housing;
        private HomeConstructionSystem build;
        private IslandLifeCatalog catalog;
        [SetUp] public void Setup()
        {
            path=Path.Combine(Path.GetTempPath(),"home-build-"+Guid.NewGuid().ToString("N"));
            player=new GameObject("Builder",typeof(CharacterController),typeof(ThirdPersonMotor),typeof(IslandCameraRig));
            var world=new WorldHandles{player=player,motor=player.GetComponent<ThirdPersonMotor>(),cameraRig=player.GetComponent<IslandCameraRig>()};
            saves=new SaveService(path);context=new GameContext(GameSaveData.CreateDefault(),saves,world,DefinitionCatalog.CreateVerticalSlice(),_=>{});
            phases=new GamePhaseController(world);catalog=IslandLifeCatalog.Load();housing=new HousingSystem(context,phases,catalog);
            build=new HomeConstructionSystem(context,phases,housing,catalog);context.Save.housing.inside=true;context.Save.resources.wood=20;
            player.transform.position=HomeNavigation.Arrival;phases.Set(GamePhase.HomeDecorate);Assert.That(build.Begin(),Is.True);
        }
        [TearDown] public void Cleanup(){build.Dispose();UnityEngine.Object.DestroyImmediate(player);if(Directory.Exists(path))Directory.Delete(path,true);}
        [Test] public void PreviewCancelAndUnaffordableApplyDoNotSpendOrSave()
        {
            Assert.That(build.ToggleFloor(20,5),Is.True);Assert.That(build.WoodCost,Is.EqualTo(2));
            Assert.That(context.Save.housing.structure.customized,Is.False);Assert.That(context.Save.resources.wood,Is.EqualTo(20));
            context.Save.resources.wood=1;Assert.That(build.Apply(),Is.False);Assert.That(context.Save.resources.wood,Is.EqualTo(1));
            phases.Set(GamePhase.Home);Assert.That(build.Editing,Is.False);Assert.That(build.Apply(),Is.False);
            Assert.That(File.Exists(saves.PathForSlot(-1)),Is.False);
        }
        [Test] public void PaidFloorRefundDoesNotMintResourcesFromInitialFloorOrCancelledChanges()
        {
            build.ToggleFloor(19,19);Assert.That(build.WoodCost,Is.Zero);Assert.That(build.Apply(),Is.True);
            build.Begin();build.ToggleFloor(20,5);build.ToggleFloor(20,5);Assert.That(build.WoodCost,Is.Zero);
            build.ToggleFloor(20,5);Assert.That(build.Apply(),Is.True);Assert.That(context.Save.resources.wood,Is.EqualTo(18));
            build.Begin();build.ToggleFloor(20,5);Assert.That(build.WoodCost,Is.EqualTo(-1));build.Apply();Assert.That(context.Save.resources.wood,Is.EqualTo(19));
        }
        [Test] public void BoundsDoorFurniturePlayerAndDisconnectedCellsAreRejected()
        {
            Assert.That(build.ToggleFloor(32,0),Is.False);Assert.That(build.ToggleFloor(-1,0),Is.False);
            Assert.That(build.ToggleFloor(31,31),Is.False);Assert.That(build.ToggleFloor(3,0),Is.False);
            player.transform.position=HousingSystem.InteriorOrigin+new Vector3(10.5f,.05f,10.5f);
            Assert.That(build.ToggleFloor(10,10),Is.False);
            context.Save.housing.placed.Add(new PlacedFurniture{instanceId="chair",furnitureId="chair",x=9,z=9});
            Assert.That(build.ToggleFloor(9,9),Is.False);Assert.That(build.ToggleWall(9,9,false),Is.False);
            Assert.That(build.ToggleWall(4,1,true),Is.False);
        }
        [Test] public void DemolitionCannotRemoveTheLastBedSpaceAndBlockResourceRegeneration()
        {
            build.Cancel();var shape=new HomeStructureState{customized=true};
            for(int x=3;x<5;x++)for(int z=0;z<5;z++)shape.cells.Add(new FloorCell{x=x,z=z});
            context.Save.housing.structure=shape;build.Begin();
            Assert.That(build.ToggleFloor(3,4),Is.False);Assert.That(build.Draft.cells.Count,Is.EqualTo(10));
        }
        [Test] public void PartitionRequiresConnectedPassageAndCostsOnlyOnce()
        {
            Assert.That(build.ToggleWall(10,10,true),Is.True);Assert.That(build.WoodCost,Is.EqualTo(1));
            Assert.That(build.ToggleWall(10,10,true),Is.True);Assert.That(build.WoodCost,Is.Zero);
            for(int z=0;z<19;z++)Assert.That(build.ToggleWall(10,z,true),Is.True);
            Assert.That(build.ToggleWall(10,19,true),Is.False,"A complete wall would strand half the floor.");
            Assert.That(build.Apply(),Is.True);Assert.That(context.Save.resources.wood,Is.EqualTo(1));
        }
        [TestCase(-1)][TestCase(0)][TestCase(1)][TestCase(2)]
        public void AppliedShapeAndPartitionsSurviveAllSaveSlots(int slot)
        {
            build.ToggleFloor(20,5);build.ToggleWall(10,10,true);Assert.That(build.Apply(),Is.True);
            context.SaveTo(slot);context.LoadFrom(slot);
            Assert.That(HomeFootprint.Cell(context.Save.housing.structure,20,5),Is.True);
            Assert.That(context.Save.housing.structure.walls.Count,Is.EqualTo(1));Assert.That(context.Save.resources.wood,Is.EqualTo(17));
            Assert.That(FurniturePlacement.Valid(catalog,catalog.Furniture("chair"),20,5,0,housing.State.placed,housing.State.structure),Is.True);
        }
        [TestCase(1)][TestCase(2)][TestCase(3)][TestCase(4)]
        public void LegacyShapeMigrationDoesNotRewriteTheOriginal(int version)
        {
            var save=saves.PathForSlot(0);Directory.CreateDirectory(path);
            string json="{\"version\":"+version+",\"resources\":{\"wood\":7},\"housing\":{\"placed\":[{\"instanceId\":\"chair\",\"furnitureId\":\"chair\",\"x\":7,\"z\":4}]}}";
            File.WriteAllText(save,json);var loaded=saves.Load(0);
            Assert.That(loaded.version,Is.EqualTo(5));Assert.That(loaded.housing.structure.customized,Is.False);
            Assert.That(HomeFootprint.Cell(loaded.housing.structure,19,19),Is.True);
            Assert.That(loaded.resources.wood,Is.EqualTo(7));Assert.That(loaded.housing.placed[0].x,Is.EqualTo(7));
            Assert.That(File.ReadAllText(save),Is.EqualTo(json));
        }
    }
}

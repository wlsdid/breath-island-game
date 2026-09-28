using System;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace BreathIsland.Tests
{
    public sealed class HousingExpansionTests
    {
        private GameObject root;
        private string path;
        private GameContext context;
        private GamePhaseController phases;
        private InteriorFinishSystem finishes;
        private SaveService saves;
        [SetUp] public void Setup()
        {
            path=Path.Combine(Path.GetTempPath(),"home-v3-"+Guid.NewGuid().ToString("N")); saves=new SaveService(path);
            root=new GameObject("Home Test",typeof(CharacterController),typeof(ThirdPersonMotor),typeof(IslandCameraRig));
            var world=new WorldHandles {player=root,motor=root.GetComponent<ThirdPersonMotor>(),cameraRig=root.GetComponent<IslandCameraRig>()};
            context=new GameContext(GameSaveData.CreateDefault(),saves,world,DefinitionCatalog.CreateVerticalSlice(),_=>{});
            phases=new GamePhaseController(world);finishes=new InteriorFinishSystem(context,phases);
        }
        [TearDown] public void Cleanup() {finishes.Dispose(); UnityEngine.Object.DestroyImmediate(root);Directory.Delete(path,true);}
        private void Open() {context.Save.housing.inside=true;phases.Set(GamePhase.Home);phases.OpenInventory();Assert.That(finishes.Begin(),Is.True);}
        [Test] public void FourRecipesPreserveOriginalEconomyAndUseExpectedFootprints()
        {
            var catalog=IslandLifeCatalog.Load();string[] ids={"chair","table","bed","lamp","couch","cabinet","shelf","table_low"};
            int[] costs={3,5,8,2,8,6,4,3};
            for(int i=0;i<ids.Length;i++) {var f=catalog.Furniture(ids[i]);Assert.That(f,Is.Not.Null);Assert.That(f.woodCost,Is.EqualTo(costs[i]));Assert.That(f.stoneCost,Is.EqualTo(ids[i]=="lamp"?2:0));}
            Assert.That(catalog.Furniture("couch").footprint,Is.EqualTo(new Vector2Int(3,1)));
            foreach(string id in ids.Skip(5))Assert.That(catalog.Furniture(id).footprint,Is.EqualTo(new Vector2Int(2,1)));
        }
        [Test] public void PreviewDoesNotSaveAndLeavingInventoryRestoresCommittedAppearance()
        {
            Assert.That(finishes.Begin(),Is.False);Open();
            Assert.That(finishes.Preview("plaster"),Is.True);Assert.That(finishes.Preview("tiles"),Is.True);
            context.SaveTo();Assert.That(saves.Load().housing.wallFinishId,Is.EqualTo("wood_dark"));
            phases.CloseInventory();Assert.That(finishes.Previewing,Is.False);Assert.That(finishes.WallId,Is.EqualTo("wood_dark"));
            Assert.That(finishes.FloorId,Is.EqualTo("planks_light"));Assert.That(finishes.Apply(),Is.False);
            Open();finishes.Preview("stone");context.LoadFrom();Assert.That(finishes.Previewing,Is.False);
        }
        [TestCase(-1)] [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void AppliedFinishesAndFurnitureSurviveSlotReload(int slot)
        {
            Open();finishes.Preview("stone");finishes.Preview("tiles");Assert.That(finishes.Apply(),Is.True);
            context.Save.housing.stock.Add(new FurnitureStock{id="couch",count=2});
            context.Save.housing.placed.Add(new PlacedFurniture{instanceId="old-chair",furnitureId="chair",x=0,z=3,turns=1});
            context.SaveTo(slot);context.LoadFrom(slot);
            Assert.That(context.Save.housing.wallFinishId,Is.EqualTo("stone"));Assert.That(finishes.FloorId,Is.EqualTo("tiles"));
            Assert.That(context.Save.housing.stock.Single().count,Is.EqualTo(2));Assert.That(context.Save.housing.placed.Single().instanceId,Is.EqualTo("old-chair"));
        }
        [Test] public void LegacyV2MigrationPreservesDataAndNeverRewritesTheSource()
        {
            const string legacy="{\"version\":2,\"inventory\":{\"coins\":91},\"housing\":{\"inside\":true,\"outdoorPosition\":{\"x\":4,\"y\":1,\"z\":5},\"placed\":[{\"instanceId\":\"saved-bed\",\"furnitureId\":\"bed\",\"x\":1,\"z\":3,\"turns\":1}],\"stock\":[{\"id\":\"chair\",\"count\":2}]}}";
            File.WriteAllText(saves.PathForSlot(0),legacy);var data=saves.Load(0);
            Assert.That(data.version,Is.EqualTo(3));Assert.That(data.inventory.coins,Is.EqualTo(91));
            Assert.That(data.housing.wallFinishId,Is.EqualTo("wood_dark"));Assert.That(data.housing.floorFinishId,Is.EqualTo("planks_light"));
            Assert.That(data.housing.placed.Single().instanceId,Is.EqualTo("saved-bed"));Assert.That(data.housing.stock.Single().count,Is.EqualTo(2));
            Assert.That(data.housing.outdoorPosition,Is.EqualTo(new Vector3(4,1,5)));Assert.That(File.ReadAllText(saves.PathForSlot(0)),Is.EqualTo(legacy));
        }
        [Test] public void VillageRecoveryOnlyMovesBlockedSavesAndKeepsMainApproachesOpen()
        {
            var instance=UnityEngine.Object.Instantiate(Resources.Load<GameObject>("World/ForestVillage"));
            try
            {
                var village=instance.GetComponent<VillageLayout>();Physics.SyncTransforms();
                var blocked=new Vector3(-9,1.2f,4);Assert.That(village.Blocks(blocked),Is.True);
                Assert.That(village.Blocks(village.RestoreSafePosition(blocked)),Is.False);
                foreach(var p in village.safePoints)Assert.That(village.RestoreSafePosition(p),Is.EqualTo(p));
                foreach(var p in new[]{new Vector3(-7,1.2f,9),new Vector3(8,1.2f,6),new Vector3(0,1.2f,10),new Vector3(4,1.2f,5.5f)})
                    foreach(var offset in new[]{Vector3.left,Vector3.right})Assert.That(village.Blocks(p+offset),Is.False);
                var oldPosition=new Vector3(0,1.5f,4);Assert.That(village.RestoreSafePosition(oldPosition),Is.EqualTo(oldPosition));
            }
            finally {UnityEngine.Object.DestroyImmediate(instance);}
        }
    }
}

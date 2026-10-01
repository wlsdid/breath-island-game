using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace BreathIsland.Tests
{
    public sealed class HomeInteractionLoopTests
    {
        private GameObject root;
        private string path;
        private GameContext context;
        private GamePhaseController phases;
        private IslandLifeSession life;
        private GameHud hud;
        private Keyboard keyboard;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            path=Path.Combine(Application.temporaryCachePath,"home-interaction-"+Guid.NewGuid().ToString("N"));root=new GameObject("Home interaction loop");
            var world=ProceduralWorld.Build(root.transform);
            context=new GameContext(GameSaveData.CreateDefault(),new SaveService(path),world,DefinitionCatalog.CreateVerticalSlice(),_=>{});
            phases=new GamePhaseController(world);life=new IslandLifeSession(context,phases,root.transform);
            hud=new GameHud(phases,new Dictionary<GamePhase,HudScreen>{
                [GamePhase.Home]=new HousingHud(context,phases,life),[GamePhase.HomeDecorate]=new HomeDecorateHud(context,phases,life),
                [GamePhase.HomeStorage]=new HomeStorageHud(context,phases,life.Storage),[GamePhase.HomeRest]=new HomeRestHud(context,phases,life.Home),
                [GamePhase.Equipment]=new EquipmentHud(context,phases,life)},new HudOverlay(context,phases,new GameSession(context,phases)));
            phases.Set(GamePhase.Explore);context.World.motor.Teleport(HousingSystem.Entrance);life.Housing.Enter();
            keyboard=InputSystem.AddDevice<Keyboard>();yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {
            if(keyboard!=null)InputSystem.RemoveDevice(keyboard);hud?.Dispose();life?.Dispose();UnityEngine.Object.Destroy(root);yield return null;
            if(Directory.Exists(path))Directory.Delete(path,true);
        }
        private void Place(string id,int x,int z,int turns=0)
        {
            phases.Set(GamePhase.Home);life.Housing.Stock(id).count++;Assert.That(life.Housing.Select(id),Is.True);
            life.Housing.Move(x,z);for(int i=0;i<turns;i++)life.Housing.Rotate();Assert.That(life.Housing.Place(),Is.True);
            phases.Set(GamePhase.Home);Physics.SyncTransforms();
        }
        private void Face(float x,float z)
        {
            context.World.motor.Teleport(HousingSystem.InteriorOrigin+new Vector3(x,.05f,z));context.World.player.transform.rotation=Quaternion.identity;Physics.SyncTransforms();
        }
        [UnityTest] public IEnumerator WalkCollideMenuReturnAndDecorateRejectPlayerOverlap()
        {
            yield return null;var start=context.World.player.transform.position;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W));InputSystem.Update();
            for(int i=0;i<12;i++)yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();
            Assert.That(context.World.player.transform.position.z,Is.GreaterThan(start.z+.1f));
            Assert.That(context.World.player.transform.position.y,Is.InRange(.9f,1.4f));
            phases.OpenInventory();var before=context.World.player.transform.position;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W));InputSystem.Update();yield return null;yield return null;
            Assert.That(context.World.player.transform.position,Is.EqualTo(before));
            phases.CloseInventory();Assert.That(phases.HomeInputAllowed,Is.False);Assert.That(context.World.motor.CanMove,Is.True);
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();
            Face(4,7);InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.W));InputSystem.Update();
            for(int i=0;i<25;i++)yield return null;
            InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();
            Assert.That(context.World.player.transform.position.z-HousingSystem.InteriorOrigin.z,Is.LessThan(8));
            life.Home.Decorate();Assert.That(context.World.motor.CanMove,Is.False);
            phases.Cancel();Assert.That(phases.Current,Is.EqualTo(GamePhase.Home));
            Face(4,1);context.World.player.transform.rotation=Quaternion.Euler(0,180,0);Assert.That(life.Home.Use(),Is.True);
            Assert.That(phases.Current,Is.EqualTo(GamePhase.Explore));
        }
        [UnityTest] public IEnumerator RotatedSeatsOcclusionBedConfirmationAndEquipmentArePreserved()
        {
            phases.OpenInventory();Assert.That(life.Equipment.Equip("sword"),Is.True);phases.CloseInventory();
            for(int turns=0;turns<4;turns++)
            {
                Face(4,1);Place("chair",0,3,turns);yield return null;Face(.5f,1.9f);
                Assert.That(life.Home.Nearby?.furnitureId,Is.EqualTo("chair"));Assert.That(life.Home.Use(),Is.True);Assert.That(life.Home.Seated,Is.True);
                for(int i=0;i<12;i++)yield return null;
                var visual=context.World.player.transform.Find("Hero Visual");
                Assert.That(Quaternion.Angle(visual.rotation,Quaternion.Euler(0,turns*90,0)),Is.LessThan(1));
                Assert.That(context.World.motor.CanMove,Is.False);Assert.That(life.Equipment.Weapon.id,Is.EqualTo("sword"));
                var hipJoints=visual.GetComponentsInChildren<Transform>().Where(t=>t.name=="upperleg.l"||t.name=="upperleg.r").ToArray();
                Assert.That(hipJoints.Average(t=>t.position.y),Is.EqualTo(HousingSystem.InteriorOrigin.y+.52f).Within(.12f),"Pelvis rests on the authored seat, using the evaluated animation height");
                if(turns==0)
                {
                    yield return Capture("seated",1280,720);
                    context.World.cameraRig.enabled=false;context.World.camera.transform.position=HousingSystem.InteriorOrigin+new Vector3(2.8f,2.6f,5.8f);
                    context.World.camera.transform.LookAt(HousingSystem.InteriorOrigin+new Vector3(.5f,.75f,3.5f));
                    yield return Capture("seat-detail",1280,720);context.World.cameraRig.enabled=true;
                }
                life.Home.Stand();Assert.That(HomeNavigation.Clear(context.World.player.transform.position,life.Housing.State.placed,life.Catalog),Is.True);
                Assert.That(life.Housing.Recall(life.Housing.State.placed.Single().instanceId),Is.True);yield return null;
            }
            Face(4,1);Place("couch",2,5);yield return null;Face(3.5f,3.5f);Assert.That(life.Home.Use(),Is.True);
            yield return new WaitForSeconds(.25f);yield return Capture("sofa",1280,720);life.Home.Stand();
            life.Housing.Recall(life.Housing.State.placed.Single().instanceId);yield return null;
            Face(4,1);Place("bed",4,3);yield return null;Face(5,1.8f);
            var blocker=GameObject.CreatePrimitive(PrimitiveType.Cube);blocker.transform.position=HousingSystem.InteriorOrigin+new Vector3(5,.65f,2.5f);blocker.transform.localScale=new Vector3(1,1,.1f);Physics.SyncTransforms();
            Assert.That(life.Home.Nearby,Is.Null);UnityEngine.Object.Destroy(blocker);yield return null;Physics.SyncTransforms();
            Assert.That(life.Home.Use(),Is.True);int day=context.Save.world.day;
            Assert.That(phases.Current,Is.EqualTo(GamePhase.HomeRest));phases.Cancel();Assert.That(context.Save.world.day,Is.EqualTo(day));
            Assert.That(life.Home.ConfirmRest(),Is.False);Assert.That(life.Home.Use(),Is.True);
            yield return Capture("rest",1280,720);
            Assert.That(life.Home.ConfirmRest(),Is.True);Assert.That(life.Home.ConfirmRest(),Is.False);
            Assert.That(context.Save.world.day,Is.EqualTo(day+1));Assert.That(context.Save.player.hour,Is.EqualTo(9));
            Assert.That(life.Equipment.Weapon.id,Is.EqualTo("sword"));
        }
        [UnityTest] public IEnumerator CraftStoreReloadAndRenderBothLanguagesAtThreeSizes()
        {
            life.Equipment.SelectTool(LifeTool.Hammer);context.Save.resources.wood=205;
            Assert.That(life.Crafting.Craft("chest"),Is.True);Assert.That(context.Save.resources.wood,Is.EqualTo(200));
            life.Housing.Select("chest");life.Housing.Move(6,3);Assert.That(life.Housing.Place(),Is.True);phases.Set(GamePhase.Home);yield return null;
            Face(6.5f,1.8f);
            context.World.cameraRig.enabled=false;context.World.camera.transform.position=HousingSystem.InteriorOrigin+new Vector3(4.5f,2.6f,1.2f);context.World.camera.transform.LookAt(HousingSystem.InteriorOrigin+new Vector3(6.5f,.5f,3.5f));
            yield return Capture("chest-detail",1280,720);context.World.cameraRig.enabled=true;
            Assert.That(life.Home.Use(),Is.True);Assert.That(life.Storage.Deposit("wood",false,120),Is.EqualTo(120));
            life.Housing.Stock("couch").count=2;Assert.That(life.Storage.Deposit("couch",true,2),Is.EqualTo(2));
            foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(2560,1080)})
                foreach(bool ko in new[]{true,false}){context.Korean=ko;yield return Capture("storage-"+(ko?"ko":"en"),size.x,size.y);}
            context.SaveTo(2);new GameSession(context,phases).Load(2);Assert.That(phases.Current,Is.EqualTo(GamePhase.Home));
            Assert.That(life.Storage.OpenId,Is.Null);Assert.That(context.Save.housing.chests.Single().slots.Sum(s=>s.count),Is.EqualTo(122));
            yield return null;Face(6.5f,1.8f);Assert.That(life.Home.Use(),Is.True);Assert.That(life.Storage.Withdraw(0,99),Is.EqualTo(99));
            Assert.That(context.Save.resources.wood,Is.EqualTo(179));Assert.That(context.Save.resources.woodHarvests,Is.Zero);
            LogAssert.NoUnexpectedReceived();
        }
        private IEnumerator Capture(string name,int width,int height)
        {
            string output=Path.GetFullPath("Builds/HomeInteraction");Directory.CreateDirectory(output);
            var camera=context.World.camera;var texture=new RenderTexture(width,height,24);camera.targetTexture=texture;
            for(int frame=0;frame<8;frame++)
            {
                hud.Draw(width,height);
                foreach(var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                {canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=.5f;}
                yield return null;
            }
            Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;RenderTexture.active=texture;
            var image=new Texture2D(width,height,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,width,height),0,0);image.Apply();
            File.WriteAllBytes(Path.Combine(output,name+"-"+width+".png"),image.EncodeToPNG());
            RenderTexture.active=previous;camera.targetTexture=null;UnityEngine.Object.Destroy(image);texture.Release();UnityEngine.Object.Destroy(texture);
        }
    }
}

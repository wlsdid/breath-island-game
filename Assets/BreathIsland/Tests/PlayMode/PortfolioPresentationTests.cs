using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace BreathIsland.Tests
{
    public sealed class PortfolioPresentationTests
    {
        [UnityTest] public IEnumerator DemoFixturesAndContextualHintsPreserveRulesAndRender()
        {
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            string path=Path.Combine(Application.temporaryCachePath,"portfolio-"+Guid.NewGuid().ToString("N"));
            var root=new GameObject("Portfolio review");
            var world=ProceduralWorld.Build(root.transform);
            var save=JsonUtility.FromJson<GameSaveData>(File.ReadAllText("Documentation/Portfolio/Seeds/village.json"));
            GameSession session=null;
            var context=new GameContext(save,new SaveService(path),world,DefinitionCatalog.CreateVerticalSlice(),m=>session?.ShowMessage(m));
            var phases=new GamePhaseController(world);session=new GameSession(context,phases);
            var life=new IslandLifeSession(context,phases,root.transform);
            var fishing=new FishingSystem(context,phases,()=>{});
            var hud=new GameHud(phases,new Dictionary<GamePhase,HudScreen>{
                [GamePhase.Explore]=new ExploreHud(context,phases,new RelationshipSystem(context,phases),life),
                [GamePhase.Fishing]=new FishingHud(context,phases,fishing),
                [GamePhase.Equipment]=new EquipmentHud(context,phases,life),
                [GamePhase.Home]=new HousingHud(context,phases,life),
                [GamePhase.HomeDecorate]=new HomeDecorateHud(context,phases,life)},new HudOverlay(context,phases,session));
            try
            {
                foreach(string scene in new[]{"village","sea","home","gather"})
                {
                    var seed=JsonUtility.FromJson<GameSaveData>(File.ReadAllText("Documentation/Portfolio/Seeds/"+scene+".json"));
                    Assert.That(seed.version,Is.EqualTo(GameSaveData.CurrentVersion));
                    for(int i=0;i<seed.housing.placed.Count;i++)
                    {
                        var p=seed.housing.placed[i];
                        Assert.That(FurniturePlacement.Valid(life.Catalog,life.Catalog.Furniture(p.furnitureId),p.x,p.z,p.turns,seed.housing.placed.GetRange(0,i),seed.housing.structure),Is.True);
                    }
                    var service=new SaveService(Path.Combine(path,scene));
                    foreach(int slot in new[]{-1,0,1,2}){service.Save(seed,slot);var restored=service.Load(slot);Assert.That(restored.housing.placed.Count,Is.EqualTo(9));Assert.That(restored.resources.wood,Is.EqualTo(30));}
                }
                phases.Set(GamePhase.Explore);world.motor.Teleport(new Vector3(0,1.2f,8));
                world.cameraRig.enabled=false;world.camera.transform.position=new Vector3(18,20,26);world.camera.transform.LookAt(new Vector3(0,0,1));
                yield return Capture(world.camera,hud,"01-village");
                phases.OpenInventory();yield return Capture(world.camera,hud,"03-inventory");phases.CloseInventory();
                var node=life.Catalog.nodes.First(n=>n.id=="tree_a");
                world.motor.Teleport(node.position-Vector3.forward*1.4f);world.player.transform.forward=Vector3.back;
                Assert.That(GatheringHint.Text(context,life,node),Is.EqualTo(context.Text("gather_face_target")));
                world.player.transform.forward=Vector3.forward;life.Equipment.SelectTool(LifeTool.Pickaxe);
                Assert.That(GatheringHint.Text(context,life,node),Is.EqualTo(string.Format(context.Text("life_need_tool"),context.Text("life_tool_Axe"))));
                life.Equipment.SelectTool(LifeTool.Axe);int wood=save.resources.wood;
                Assert.That(life.Land.Harvest(node.id),Is.False);
                Assert.That(life.Land.Progress(node),Is.EqualTo(1f/3).Within(.001f));
                Assert.That(save.resources.wood,Is.EqualTo(wood));
                world.camera.transform.position=node.position+new Vector3(-4,5,-6);world.camera.transform.LookAt(node.position+Vector3.up);
                yield return Capture(world.camera,hud,"04-gathering");
                Assert.That(life.Land.Harvest(node.id),Is.False);Assert.That(life.Land.Harvest(node.id),Is.True);
                Assert.That(GatheringHint.Text(context,life,node),Is.EqualTo(context.Text("life_depleted")));
                Assert.That(save.resources.wood,Is.EqualTo(wood+3));
                yield return new WaitForSeconds(3);
                world.motor.Teleport(new Vector3(0,1.2f,23));fishing.Start();
                world.camera.transform.position=new Vector3(0,2.4f,23);world.camera.transform.rotation=Quaternion.Euler(15,0,0);
                yield return Capture(world.camera,hud,"02-fishing");phases.Set(GamePhase.Explore);
                world.motor.Teleport(HousingSystem.Entrance);Assert.That(life.Housing.Enter(),Is.True);
                world.motor.Teleport(HousingSystem.InteriorOrigin+new Vector3(5,.1f,2));world.cameraRig.enabled=true;
                yield return Capture(world.camera,hud,"05-interior");
                phases.Set(GamePhase.HomeDecorate);Assert.That(life.Construction.Begin(),Is.True);
                Assert.That(life.Construction.ToggleFloor(20,5),Is.True);
                yield return Capture(world.camera,hud,"06-construction");
                life.Construction.Cancel();
                LogAssert.NoUnexpectedReceived();
            }
            finally {hud.Dispose();life.Dispose();UnityEngine.Object.Destroy(root);Cursor.lockState=CursorLockMode.None;if(Directory.Exists(path))Directory.Delete(path,true);}
            yield return null;
        }
        private static IEnumerator Capture(Camera camera,GameHud hud,string name)
        {
            var texture=new RenderTexture(1920,1080,24);camera.targetTexture=texture;
            try
            {
                for(int i=0;i<8;i++)
                {
                    hud.Draw(1920,1080);
                    foreach(var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                    {canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=.5f;}
                    yield return null;
                }
                Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;RenderTexture.active=texture;
                var image=new Texture2D(1920,1080,TextureFormat.RGB24,false);
                image.ReadPixels(new Rect(0,0,1920,1080),0,0);image.Apply();
                Directory.CreateDirectory("Builds/Portfolio/Images");File.WriteAllBytes("Builds/Portfolio/Images/"+name+".png",image.EncodeToPNG());
                UnityEngine.Object.Destroy(image);RenderTexture.active=previous;
            }
            finally{camera.targetTexture=null;texture.Release();UnityEngine.Object.Destroy(texture);}
        }
    }
}

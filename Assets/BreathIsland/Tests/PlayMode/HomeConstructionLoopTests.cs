using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
using UnityEngine.UI;
namespace BreathIsland.Tests
{
    public sealed class HomeConstructionLoopTests
    {
        [UnityTest] public IEnumerator GridPreviewApplyReloadCollisionAndLocalizedScreens()
        {
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            var root=new GameObject("Construction loop");var directory=Path.Combine(Application.temporaryCachePath,"construction-"+Guid.NewGuid().ToString("N"));
            var world=ProceduralWorld.Build(root.transform);
            var context=new GameContext(GameSaveData.CreateDefault(),new SaveService(directory),world,DefinitionCatalog.CreateVerticalSlice(),_=>{});
            var phases=new GamePhaseController(world);var life=new IslandLifeSession(context,phases,root.transform);
            var hud=new GameHud(phases,new Dictionary<GamePhase,HudScreen>{[GamePhase.HomeDecorate]=new HomeDecorateHud(context,phases,life)},new HudOverlay(context,phases,new GameSession(context,phases)));
            try
            {
                world.motor.Teleport(HousingSystem.Entrance);Assert.That(life.Housing.Enter(),Is.True);
                context.Save.resources.wood=20;phases.Set(GamePhase.HomeDecorate);hud.Draw(1920,1080);
                var button=UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.gameObject.activeInHierarchy&&b.name=="Action: "+context.Text("build_title"));
                ExecuteEvents.Execute(button.gameObject,new PointerEventData(EventSystem.current),ExecuteEvents.pointerClickHandler);
                Assert.That(life.Construction.Editing,Is.True);
                Assert.That(life.Construction.ToggleFloor(20,5),Is.True);Assert.That(life.Construction.ToggleWall(10,10,true),Is.True);
                Assert.That(context.Save.resources.wood,Is.EqualTo(20));
                foreach(bool korean in new[]{true,false})
                {
                    context.Korean=korean;
                    foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(2560,1080)})
                    {
                        hud.Draw(size.x,size.y);yield return null;
                        yield return Capture(world.cameraRig.GetComponent<Camera>()??Camera.main,hud,size,korean);
                    }
                }
                Assert.That(life.Construction.Apply(),Is.True);Assert.That(context.Save.resources.wood,Is.EqualTo(17));
                context.SaveTo(2);life.Dispose();context.LoadFrom(2);life=new IslandLifeSession(context,phases,root.transform);
                yield return null;Physics.SyncTransforms();
                Assert.That(HomeFootprint.Cell(context.Save.housing.structure,20,5),Is.True);
                var tile=HousingSystem.InteriorOrigin+new Vector3(20.5f,2,5.5f);
                Assert.That(Physics.Raycast(tile,Vector3.down,out var hit,3),Is.True);Assert.That(hit.point.y,Is.EqualTo(HousingSystem.InteriorOrigin.y).Within(.03f));
                Assert.That(Physics.Raycast(HousingSystem.InteriorOrigin+new Vector3(9.5f,1.5f,10.5f),Vector3.right,.6f),Is.True);
                life.Construction.Begin();life.Construction.ToggleFloor(20,6);phases.Set(GamePhase.Home);
                Assert.That(life.Construction.Editing,Is.False);Assert.That(HomeFootprint.Cell(context.Save.housing.structure,20,6),Is.False);
                life.Housing.Exit();Assert.That(world.player.transform.position,Is.EqualTo(HousingSystem.Entrance));
                LogAssert.NoUnexpectedReceived();
            }
            finally{hud.Dispose();life.Dispose();UnityEngine.Object.Destroy(root);if(Directory.Exists(directory))Directory.Delete(directory,true);}
            yield return null;
        }
        private static IEnumerator Capture(Camera camera,GameHud hud,Vector2Int size,bool korean)
        {
            Assert.That(camera,Is.Not.Null);var texture=new RenderTexture(size.x,size.y,24);camera.targetTexture=texture;
            try
            {
                for(int i=0;i<6;i++)
                {
                    hud.Draw(size.x,size.y);
                    foreach(var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)){canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=.5f;}
                    yield return null;
                }
                Canvas.ForceUpdateCanvases();camera.Render();var previous=RenderTexture.active;RenderTexture.active=texture;
                var image=new Texture2D(size.x,size.y,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,size.x,size.y),0,0);image.Apply();
                string output=Path.GetFullPath("Builds/ConstructionReview");Directory.CreateDirectory(output);
                File.WriteAllBytes(Path.Combine(output,$"build-{size.x}-{(korean?"ko":"en")}.png"),image.EncodeToPNG());
                UnityEngine.Object.Destroy(image);RenderTexture.active=previous;
            }
            finally{camera.targetTexture=null;texture.Release();UnityEngine.Object.Destroy(texture);}
        }
    }
}

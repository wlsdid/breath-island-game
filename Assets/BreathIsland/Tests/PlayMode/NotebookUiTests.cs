using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.TestTools;
namespace BreathIsland.Tests
{
    public sealed class NotebookUiTests
    {
        private GameObject root;
        private string path;
        private GameContext context;
        private GamePhaseController phases;
        private IslandLifeSession life;
        private GameHud hud;
        [UnitySetUp] public IEnumerator Setup()
        {
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            root=new GameObject("Notebook UI test");path=Path.Combine(Application.temporaryCachePath,"notebook-"+Guid.NewGuid().ToString("N"));
            var world=ProceduralWorld.Build(root.transform);
            context=new GameContext(GameSaveData.CreateDefault(),new SaveService(path),world,DefinitionCatalog.CreateVerticalSlice(),_=>{});
            phases=new GamePhaseController(world);life=new IslandLifeSession(context,phases,root.transform);
            hud=new GameHud(phases,new Dictionary<GamePhase,HudScreen>{
                [GamePhase.Explore]=new ExploreHud(context,phases,new RelationshipSystem(context,phases),life),
                [GamePhase.Equipment]=new EquipmentHud(context,phases,life),
                [GamePhase.Home]=new HousingHud(context,phases,life)},new HudOverlay(context,phases,new GameSession(context,phases)));
            phases.Set(GamePhase.Explore);yield return null;
        }
        [UnityTearDown] public IEnumerator Cleanup()
        {hud.Dispose();life.Dispose();UnityEngine.Object.Destroy(root);Cursor.lockState=CursorLockMode.None;yield return null;Directory.Delete(path,true);}
        private void Click(string key)
        {
            var button=UnityEngine.Object.FindObjectsByType<Button>(FindObjectsSortMode.None).Single(b=>b.gameObject.activeInHierarchy&&b.name=="Action: "+context.Text(key));
            ExecuteEvents.Execute(button.gameObject,new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);
        }
        [UnityTest] public IEnumerator InventoryButtonsHoldToolAndBlockClickThroughThenReturnToRoom()
        {
            phases.OpenInventory();hud.Draw();yield return null;hud.Draw();
            Assert.That(Cursor.lockState,Is.EqualTo(CursorLockMode.None));Assert.That(context.World.motor.CanMove,Is.False);
            Click("bag_hold");Assert.That(life.Equipment.Tool,Is.EqualTo(LifeTool.Axe));Assert.That(phases.Current,Is.EqualTo(GamePhase.Explore));
            Assert.That(phases.FieldInputAllowed,Is.False);
            yield return null;yield return null;yield return null;Assert.That(phases.FieldInputAllowed,Is.True);
            context.World.motor.Teleport(HousingSystem.Entrance);Assert.That(life.Housing.Enter(),Is.True);
            phases.OpenInventory();hud.Draw();yield return null;hud.Draw();Click("bag_close");
            Assert.That(phases.Current,Is.EqualTo(GamePhase.Home));Assert.That(context.World.cameraRig.homeView,Is.True);
        }
        [UnityTest] public IEnumerator IndoorInventoryCraftsAndPlacesWithoutDuplicatingSavedStock()
        {
            context.World.motor.Teleport(HousingSystem.Entrance);life.Housing.Enter();phases.OpenInventory();
            var inventory=new InventorySystem(context,phases,life);context.Save.resources.wood=3;
            Assert.That(inventory.Craft("chair"),Is.False);life.Equipment.SelectTool(LifeTool.Hammer);
            Assert.That(inventory.Craft("chair"),Is.True);Assert.That(context.Save.resources.wood,Is.Zero);
            Assert.That(inventory.Items(InventoryTab.Furniture).Single(i=>i.Id=="chair").Count,Is.EqualTo(1));
            Assert.That(inventory.Place("chair"),Is.True);life.Housing.Move(1,3);Assert.That(life.Housing.Place(),Is.True);
            context.LoadFrom();Assert.That(context.Save.housing.placed.Count,Is.EqualTo(1));Assert.That(life.Housing.Available("chair"),Is.Zero);
            yield return null;
        }
        [UnityTest] public IEnumerator VectorUiAndLicensedFontsRenderAtThreeAspectRatiosInBothLanguages()
        {
            string output=Path.GetFullPath("Builds/NotebookReview");Directory.CreateDirectory(output);
            var camera=context.World.camera;var texture=new RenderTexture(1280,720,24);camera.targetTexture=texture;
            try
            {
                foreach(var size in new[]{new Vector2Int(1280,720),new Vector2Int(1920,1080),new Vector2Int(2560,1080)})
                {
                    texture.Release();texture.width=size.x;texture.height=size.y;texture.Create();
                    foreach(bool korean in new[]{true,false})
                    {
                        context.Korean=korean;phases.OpenInventory();
                        for(int frame=0;frame<10;frame++)
                        {
                            hud.Draw(size.x,size.y);
                            foreach(var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                            {canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=.5f;}
                            yield return null;
                        }
                        Canvas.ForceUpdateCanvases();camera.Render();
                        foreach(var graphic in UnityEngine.Object.FindObjectsByType<NotebookGraphic>(FindObjectsSortMode.None))
                        {
                            if(!graphic.enabled)continue;
                            var mesh=graphic.canvasRenderer.GetMesh();Assert.That(mesh,Is.Not.Null);Assert.That(mesh.vertexCount,Is.GreaterThan(0),"Visible vector panel must have a mesh");
                        }
                        foreach(var text in UnityEngine.Object.FindObjectsByType<Text>(FindObjectsSortMode.None))
                            Assert.That(text.font.name,Is.EqualTo("Gaegu-Bold").Or.EqualTo("GowunDodum-Regular"));
                        var previous=RenderTexture.active;RenderTexture.active=texture;
                        var image=new Texture2D(size.x,size.y,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,size.x,size.y),0,0);image.Apply();
                        File.WriteAllBytes(Path.Combine(output,$"inventory-{size.x}-{(korean?"ko":"en")}.png"),image.EncodeToPNG());
                        UnityEngine.Object.Destroy(image);RenderTexture.active=previous;
                        phases.CloseInventory();
                    }
                }
            }
            finally {camera.targetTexture=null;texture.Release();UnityEngine.Object.Destroy(texture);}
            LogAssert.NoUnexpectedReceived();
        }
    }
}

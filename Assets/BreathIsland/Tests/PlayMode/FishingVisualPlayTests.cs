using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BreathIsland.Tests
{
    public sealed class FishingVisualPlayTests
    {
        [UnityTest] public IEnumerator CastReelEaseFailureAndExitKeepRulesAndReleaseVisuals()
        {
            var root=new GameObject("Coastal visual review");
            string folder=Path.Combine(Application.temporaryCachePath,"fishing-view-"+Guid.NewGuid().ToString("N"));
            var world=ProceduralWorld.Build(root.transform);
            var context=new GameContext(GameSaveData.CreateDefault(),new SaveService(folder),world,DefinitionCatalog.CreateVerticalSlice(),_=>{});
            var phases=new GamePhaseController(world);
            var fishing=new FishingSystem(context,phases,()=>phases.Set(GamePhase.PredatorCombat));
            var view=root.AddComponent<FishingPresentation>();view.Initialize(world,fishing,phases);
            var life=new IslandLifeSession(context,phases,root.transform);
            try
            {
                world.motor.Teleport(new Vector3(0,.93f,23));world.motor.enabled=false;
                var anchor=world.player.transform.position;
                fishing.Start();yield return new WaitForSeconds(.5f);
                Assert.That(view.Visible,Is.True);
                int bait=context.Save.inventory.bait;
                fishing.Cast();yield return new WaitForSeconds(1);
                Assert.That(context.Save.inventory.bait,Is.EqualTo(bait-1));
                Assert.That(fishing.Stage,Is.EqualTo(FishingStage.Waiting));
                fishing.Tick(3);yield return new WaitForSeconds(.45f);
                var rod=root.GetComponentInChildren<FishingRodView>();
                var line=root.transform.Find("FishingRig(Clone)/Line").GetComponent<LineRenderer>();
                Assert.That(Vector3.Distance(line.GetPosition(0),rod.Tip),Is.LessThan(.001f));
                float tension=fishing.Tension;fishing.Reel();Assert.That(fishing.Tension,Is.GreaterThan(tension));
                yield return Capture(world.camera,"Fishing");
                var hud=new FishingHud(context,phases,fishing);
                try
                {
                    float scale=Mathf.Min(Screen.width/1600f,Screen.height/900f);
                    hud.Render(Screen.width/scale,Screen.height/scale);
                    yield return new WaitForSeconds(.2f);
                    hud.Render(Screen.width/scale,Screen.height/scale);Canvas.ForceUpdateCanvases();
                    yield return new WaitForEndOfFrame();
                    var screen=ScreenCapture.CaptureScreenshotAsTexture();
                    File.WriteAllBytes("Builds/CoastalReview/Fishing-Hud.png",screen.EncodeToPNG());UnityEngine.Object.Destroy(screen);
                }
                finally {hud.Dispose();}
                tension=fishing.Tension;fishing.Ease();Assert.That(fishing.Tension,Is.LessThan(tension));
                phases.Set(GamePhase.Pause);yield return null;Assert.That(view.Visible,Is.False);
                float paused=fishing.Tension;fishing.Ease();Assert.That(fishing.Tension,Is.EqualTo(paused));
                phases.Cancel();yield return new WaitForEndOfFrame();Assert.That(view.Visible,Is.True);
                fishing.Tick(100);yield return new WaitForEndOfFrame();
                Assert.That(fishing.Stage,Is.EqualTo(FishingStage.Failed));Assert.That(line.enabled,Is.False);
                fishing.Retry();fishing.Cast();fishing.Tick(3);
                for(int i=0;i<8;i++){fishing.Ease();fishing.Reel();}
                yield return null;Assert.That(phases.Current,Is.EqualTo(GamePhase.CatchResolution));Assert.That(view.Visible,Is.False);
                Assert.That(world.player.transform.position,Is.EqualTo(anchor));
                Assert.That(root.GetComponentInChildren<CoastalScenery>().GetComponentsInChildren<Collider>(),Is.Empty);
                phases.Set(GamePhase.Explore);world.cameraRig.enabled=false;
                world.camera.transform.position=new Vector3(23,18,31);world.camera.transform.LookAt(new Vector3(0,0,1));
                yield return Capture(world.camera,"Coast");
                LogAssert.NoUnexpectedReceived();
            }
            finally {life.Dispose();UnityEngine.Object.Destroy(root);if(Directory.Exists(folder))Directory.Delete(folder,true);Cursor.lockState=CursorLockMode.None;}
            yield return null;
        }
        private static IEnumerator Capture(Camera camera,string name)
        {
            yield return new WaitForEndOfFrame();Directory.CreateDirectory("Builds/CoastalReview");
            var previous=RenderTexture.active;var target=new RenderTexture(1600,900,24);var image=new Texture2D(1600,900,TextureFormat.RGB24,false);
            try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1600,900),0,0);image.Apply();File.WriteAllBytes("Builds/CoastalReview/"+name+".png",image.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=previous;UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(image);}
        }
    }
}

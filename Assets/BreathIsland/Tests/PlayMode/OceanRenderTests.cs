using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BreathIsland.Tests
{
    public sealed class OceanRenderTests
    {
        [UnityTest] public IEnumerator FishAreVisibleThroughWaterAndBobberTracksAndDipsBelowWaves()
        {
            var root=new GameObject("Ocean review");var world=ProceduralWorld.Build(root.transform);
            var path=Path.Combine(Application.temporaryCachePath,"ocean-"+Guid.NewGuid().ToString("N"));
            var context=new GameContext(GameSaveData.CreateDefault(),new SaveService(path),world,DefinitionCatalog.CreateVerticalSlice(),_=>{});
            var phases=new GamePhaseController(world);var fishing=new FishingSystem(context,phases,()=>{});
            root.AddComponent<FishingPresentation>().Initialize(world,fishing,phases);
            world.motor.enabled=false;world.cameraRig.enabled=false;
            try
            {
                yield return new WaitForSeconds(.3f);
                var ocean=root.GetComponentInChildren<OceanSurface>();
                Assert.That(ocean.GetComponentsInChildren<Collider>(),Is.Empty);
                Assert.That(ocean.GetComponent<MeshFilter>().sharedMesh.vertexCount,Is.GreaterThan(20000));
                Assert.That(ocean.GetComponent<Renderer>().sharedMaterial.shader.isSupported,Is.True);
                var swimmers=root.GetComponentsInChildren<AmbientFishMotion>();
                Assert.That(swimmers.Length,Is.EqualTo(5));
                foreach(var swimmer in swimmers)
                    Assert.That(swimmer.transform.position.y,Is.LessThan(OceanSurface.Height(swimmer.transform.position,Time.time)-.65f));
                world.camera.transform.position=new Vector3(0,5,35);world.camera.transform.LookAt(new Vector3(0,-1,29));
                yield return new WaitForEndOfFrame();
                var withFish=Capture(world.camera,"Underwater-fish");
                foreach(var swimmer in swimmers)foreach(var renderer in swimmer.GetComponentsInChildren<Renderer>())renderer.enabled=false;
                var withoutFish=Capture(world.camera,null);
                int changed=0;
                for(int i=0;i<withFish.Length;i++)
                    if(Math.Abs(withFish[i].r-withoutFish[i].r)+Math.Abs(withFish[i].g-withoutFish[i].g)+Math.Abs(withFish[i].b-withoutFish[i].b)>15)changed++;
                Assert.That(changed,Is.GreaterThan(100),"Opaque fish must remain visible through refractive water, not disappear behind a blue plane.");
                foreach(var swimmer in swimmers)foreach(var renderer in swimmer.GetComponentsInChildren<Renderer>())renderer.enabled=true;
                world.motor.Teleport(new Vector3(0,.93f,23));
                world.camera.transform.position=new Vector3(0,2.18f,23.15f);world.camera.transform.rotation=Quaternion.Euler(12,0,0);
                fishing.Start();fishing.Cast();yield return new WaitForSeconds(1.1f);yield return new WaitForEndOfFrame();
                var bobber=root.transform.Find("FishingRig(Clone)/Bobber");
                Assert.That(bobber.position.y-OceanSurface.Height(bobber.position,Time.time),Is.EqualTo(.035f).Within(.005f));
                Capture(world.camera,"Floating");
                fishing.Tick(3);yield return new WaitForSeconds(.25f);yield return new WaitForEndOfFrame();
                Assert.That(bobber.position.y,Is.LessThan(OceanSurface.Height(bobber.position,Time.time)-.12f));
                Capture(world.camera,"Bite-submerged");
                fishing.Hook();yield return new WaitForSeconds(.35f);yield return new WaitForEndOfFrame();
                Capture(world.camera,"Jump-splash");
                yield return new WaitForSeconds(.95f);yield return new WaitForEndOfFrame();
                var caught=root.transform.Find("FishingRig(Clone)/Fish");
                Assert.That(caught.gameObject.activeSelf,Is.True);Assert.That(caught.position.y,Is.LessThan(OceanSurface.Height(caught.position,Time.time)));
                Capture(world.camera,"Hooked-underwater");
                LogAssert.NoUnexpectedReceived();
            }
            finally{UnityEngine.Object.Destroy(root);if(Directory.Exists(path))Directory.Delete(path,true);Cursor.lockState=CursorLockMode.None;}
            yield return null;
        }
        private static Color32[] Capture(Camera camera,string name)
        {
            Directory.CreateDirectory("Builds/OceanReview");
            var previous=RenderTexture.active;var target=new RenderTexture(1280,720,24);var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=target;camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();
                if(name!=null)File.WriteAllBytes("Builds/OceanReview/"+name+".png",image.EncodeToPNG());return image.GetPixels32();
            }
            finally{camera.targetTexture=null;RenderTexture.active=previous;UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(image);}
        }
    }
}

using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BreathIsland.Tests
{
    public sealed class VillagePolishPlayTests
    {
        [UnityTest] public IEnumerator VillagersAnimateAndLookWithoutMovingTheirInteractionAnchors()
        {
            var root=new GameObject("Village polish review");
            var world=ProceduralWorld.Build(root.transform);
            var catalog=IslandLifeCatalog.Load();
            // The player remains the attention target, but does not obscure NPC review shots.
            foreach(var renderer in world.player.GetComponentsInChildren<Renderer>())renderer.forceRenderingOff=true;
            var lifeWorld=new IslandLifeWorld(root.transform,catalog);
            world.motor.enabled=false;world.cameraRig.enabled=false;
            try
            {
                foreach(var pair in world.npcs)
                {
                    Vector3 anchor=pair.Value.position;
                    var visual=pair.Value.GetComponent<NpcPresentation>().visual;
                    world.player.transform.position=anchor+new Vector3(1.5f,0,1.5f);
                    yield return new WaitForSeconds(2);
                    Assert.That(pair.Value.position,Is.EqualTo(anchor));
                    Assert.That(Vector3.Dot(visual.forward,new Vector3(1,0,1).normalized),Is.GreaterThan(.95f));
                    var animator=visual.GetComponentInChildren<Animator>();
                    Assert.That(animator.GetCurrentAnimatorStateInfo(0).normalizedTime,Is.GreaterThan(.1f));
                    world.camera.transform.position=anchor+new Vector3(3,1.8f,4);
                    world.camera.transform.LookAt(anchor+Vector3.up*.25f);
                    yield return Capture(world.camera,pair.Key.ToString());
                }
                world.player.transform.position=new Vector3(0,1.2f,8);
                world.camera.transform.position=new Vector3(18,18,25);
                world.camera.transform.LookAt(new Vector3(0,0,1));
                yield return Capture(world.camera,"Village");
                LogAssert.NoUnexpectedReceived();
            }
            finally {lifeWorld.Dispose();Object.Destroy(root);Cursor.lockState=CursorLockMode.None;}
            yield return null;
        }

        private static IEnumerator Capture(Camera camera,string name)
        {
            yield return new WaitForEndOfFrame();
            Directory.CreateDirectory("Builds/VillagePolish");
            var previous=RenderTexture.active;
            var target=new RenderTexture(1600,900,24);
            var image=new Texture2D(1600,900,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=target;camera.Render();RenderTexture.active=target;
                image.ReadPixels(new Rect(0,0,1600,900),0,0);image.Apply();
                File.WriteAllBytes("Builds/VillagePolish/"+name+".png",image.EncodeToPNG());
            }
            finally {camera.targetTexture=null;RenderTexture.active=previous;Object.Destroy(target);Object.Destroy(image);}
        }
    }
}

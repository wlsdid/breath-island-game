using System;
using System.Collections;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;
namespace BreathIsland.Tests
{
    public sealed class WeaponPoseTests
    {
        [UnityTest] public IEnumerator EachWeaponKeepsItsJobAndAnimatesTheArmOnValidHit()
        {
            var root=new GameObject("Weapon pose review");var world=ProceduralWorld.Build(root.transform);
            var folder=Path.Combine(Application.temporaryCachePath,"weapon-pose-"+Guid.NewGuid().ToString("N"));
            var context=new GameContext(GameSaveData.CreateDefault(),new SaveService(folder),world,DefinitionCatalog.CreateVerticalSlice(),_=>{});
            var phases=new GamePhaseController(world);var life=new IslandLifeSession(context,phases,root.transform);
            world.motor.enabled=false;world.cameraRig.enabled=false;
            try
            {
                var standing=life.Training.TargetPosition-Vector3.forward*2;standing.y=.62f;world.motor.Teleport(standing);world.player.transform.rotation=Quaternion.identity;
                yield return new WaitForSeconds(.6f);
                var upper=world.player.GetComponentsInChildren<Transform>().First(t=>t.name=="upperarm.r");
                context.Save.equipment.achievements.AddRange(new[]{"craft","training","gather"});
                foreach(var id in new[]{"sword","bow","staff","sword_special","bow_special","staff_special"})
                {
                    Assert.That(life.Equipment.Equip(id),Is.True);life.Training.ResetTarget();life.Training.Tick(10);
                    yield return new WaitForSeconds(.7f);var rest=upper.rotation;
                    Assert.That(life.Training.Attack(false),Is.True);
                    yield return new WaitForSeconds(.15f);yield return new WaitForEndOfFrame();
                    Assert.That(Quaternion.Angle(rest,upper.rotation),Is.GreaterThan(10),id);
                    Assert.That(life.Equipment.Job,Is.SameAs(life.Equipment.Weapon.job));
                    world.camera.transform.position=world.player.transform.position+new Vector3(2,1.4f,3);world.camera.transform.LookAt(world.player.transform.position+Vector3.up*.8f);
                                        Capture(world.camera,id);
                    var socket = world.player.GetComponentsInChildren<Transform>().First(t=>t.name=="Held Item Socket");
                                        var meshes = socket.GetComponentsInChildren<Renderer>();
                    var bounds = meshes[0].bounds; foreach(var mesh in meshes) bounds.Encapsulate(mesh.bounds);
                    Assert.That(Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z),Is.InRange(.8f,1.5f),id+" world size");
                    Assert.That(Vector3.Distance(bounds.center,socket.position),Is.LessThan(.65f),id+" grip");
                    Assert.That(root.GetComponentsInChildren<CombatFeedback>().SelectMany(f=>f.GetComponents<LineRenderer>()),Is.Empty);
                    yield return new WaitForSeconds(.55f);
                    Capture(world.camera,id+"-carry");
                }
            }
            finally{life.Dispose();UnityEngine.Object.Destroy(root);if(Directory.Exists(folder))Directory.Delete(folder,true);Cursor.lockState=CursorLockMode.None;}
        }
        private static void Capture(Camera camera,string name)
        {
            Directory.CreateDirectory("Builds/VisualRepair");var before=RenderTexture.active;var target=new RenderTexture(1280,720,24);var image=new Texture2D(1280,720,TextureFormat.RGB24,false);
            try{camera.targetTexture=target;camera.Render();RenderTexture.active=target;image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes("Builds/VisualRepair/"+name+".png",image.EncodeToPNG());}
            finally{camera.targetTexture=null;RenderTexture.active=before;UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(image);}
        }
    }
}

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.TestTools;
namespace BreathIsland.Tests
{
    public sealed class IslandLifeLoopTests
    {
        private GameObject root;
        private string directory;
        private GameContext context;
        private GamePhaseController phases;
        private IslandLifeSession life;
        private SaveService saves;
        [UnitySetUp] public IEnumerator SetUp()
        {
            yield return LocalizationSettings.InitializationOperation;
            directory = Path.Combine(Application.temporaryCachePath, "breath-life-loop-" + Guid.NewGuid().ToString("N"));
            saves = new SaveService(directory); root = new GameObject("Life Loop Test");
            var world = ProceduralWorld.Build(root.transform);
            context = new GameContext(GameSaveData.CreateDefault(), saves, world, DefinitionCatalog.CreateVerticalSlice(), _ => { });
            phases = new GamePhaseController(world); life = new IslandLifeSession(context, phases, root.transform);
            phases.Set(GamePhase.Explore); yield return null;
        }
        [UnityTearDown] public IEnumerator TearDown()
        {
            life.Dispose(); UnityEngine.Object.Destroy(root); yield return null;
            Directory.Delete(directory, true);
        }
        [UnityTest] public IEnumerator BowOutfitDiffersFromUnarmedAndRestoresWithoutChangingRig()
        {
            life.Equipment.Equip(""); yield return null;
            var player=context.World.player;
            var original=player.GetComponentsInChildren<SkinnedMeshRenderer>().Where(r=>r.enabled && r.GetComponentInParent<ToolSwing>()==null).Select(r=>r.sharedMesh).ToArray();
            var hand=player.GetComponentsInChildren<Transform>().First(t=>t.name=="handslot.r");
            context.Save.equipment.achievements.Add("training");
            foreach(var id in new[]{"bow","bow_special"})
            {
                Assert.That(life.Equipment.Equip(id),Is.True);yield return null;
                var skins=player.GetComponentsInChildren<SkinnedMeshRenderer>().Where(r=>r.enabled && r.GetComponentInParent<ToolSwing>()==null).ToArray();
                Assert.That(skins.Length,Is.GreaterThan(0));
                Assert.That(skins.All(r=>r.transform.parent.name=="ranger outfit"),Is.True,string.Join(",",skins.Select(r=>r.name+" parent="+r.transform.parent.name)));
                Assert.That(skins.Any(r=>!original.Contains(r.sharedMesh)),Is.True);
                Assert.That(player.GetComponentsInChildren<Transform>().First(t=>t.name=="handslot.r"),Is.SameAs(hand));
                life.Equipment.SelectTool(LifeTool.Axe);yield return null;
                Assert.That(skins.All(r=>r.gameObject.activeInHierarchy && r.enabled),Is.True);
                life.Equipment.Restore(); yield return null;
                Assert.That(skins.All(r=>r.gameObject.activeInHierarchy && r.enabled),Is.True);
                life.Equipment.Equip("");yield return null;
                var restored=player.GetComponentsInChildren<SkinnedMeshRenderer>().Where(r=>r.enabled && r.GetComponentInParent<ToolSwing>()==null).Select(r=>r.sharedMesh).ToArray();
                CollectionAssert.AreEquivalent(original,restored);
            }
        }
        [UnityTest] public IEnumerator SavedBowOutfitSurvivesEquipmentReplacement()
        {
            life.Dispose(); yield return null;
            context.Save.equipment.weaponId="bow";context.Save.equipment.tool=LifeTool.None;
            life=new IslandLifeSession(context,phases,root.transform);yield return null;
            Assert.That(life.Equipment.Equip("staff"),Is.True);yield return null;
            Assert.That(life.Equipment.Equip("bow"),Is.True);yield return null;
            Assert.That(context.World.player.GetComponentsInChildren<SkinnedMeshRenderer>().Any(r=>r.enabled && r.transform.parent.name=="ranger outfit"),Is.True);
        }
        [UnityTest] public IEnumerator TrainingRecoversThreeSecondsAfterLastHitAndOutfitsKeepRig()
        {
            context.World.motor.enabled=false;
            context.World.motor.Teleport(life.Training.TargetPosition-Vector3.forward*2);
            var hand=context.World.player.GetComponentsInChildren<Transform>().First(t=>t.name=="handslot.r");
            foreach(var id in new[]{"sword","bow","staff"})
            {
                life.Equipment.Equip(id); yield return null;
                Assert.That(context.World.player.GetComponentsInChildren<Transform>().First(t=>t.name=="handslot.r"),Is.SameAs(hand));
                var visible=context.World.player.GetComponentsInChildren<SkinnedMeshRenderer>().Where(r=>r.enabled && r.GetComponentInParent<ToolSwing>()==null).ToArray();
                Assert.That(visible.Length,Is.GreaterThan(0));
                Assert.That(visible.All(r=>r.bones.All(b=>b!=null)),Is.True);
                if(id=="sword" || id=="staff") Assert.That(visible.Any(r=>r.transform.parent.name==(id=="sword"?"warrior outfit":"mage outfit")),Is.True);
                life.Equipment.SelectTool(LifeTool.Axe);
                Assert.That(life.Equipment.Job.id,Is.EqualTo(id=="sword"?"warrior":id=="bow"?"ranger":"mage"));
            }
            life.Equipment.Equip("sword");
            Assert.That(life.Training.Attack(false),Is.True);
            life.Training.Tick(2.9f); Assert.That(life.Training.Health,Is.LessThan(100));
            Assert.That(life.Training.Attack(false),Is.True);
            life.Training.Tick(.2f); Assert.That(life.Training.Health,Is.LessThan(100));
            life.Training.Tick(2.81f); Assert.That(life.Training.Health,Is.EqualTo(100));
            for(int i=0;i<20 && life.Training.Health>0;i++){life.Training.Tick(.51f);life.Training.Attack(false);}
            Assert.That(life.Training.Health,Is.Zero);life.Training.Tick(3);Assert.That(life.Training.Health,Is.EqualTo(100));
        }
        [UnityTest] public IEnumerator HarvestCraftPlaceReloadAndReturnKeepInventoryJobAndOutdoorPosition()
        {
            life.Equipment.Equip("sword");
            foreach (var node in life.Catalog.nodes.Where(n => n.resource == LifeResource.Wood))
            {
                context.World.motor.Teleport(node.position + Vector3.up * .2f); life.Equipment.SelectTool(LifeTool.Axe);
                Assert.That(life.Land.Harvest(node.id), Is.False); Assert.That(life.Land.Harvest(node.id), Is.False);
                Assert.That(life.Land.Harvest(node.id), Is.True); Assert.That(life.Land.Harvest(node.id), Is.False);
            }
            var outdoor = HousingSystem.Entrance + Vector3.up * .5f; context.World.motor.Teleport(outdoor);
            Assert.That(life.Housing.Enter(), Is.True); life.Equipment.SelectTool(LifeTool.Hammer);
            Assert.That(life.Crafting.Craft("bed"), Is.True); Assert.That(life.Equipment.Job.id, Is.EqualTo("warrior"));
            life.Housing.Select("bed"); life.Housing.Move(0, 3); life.Housing.Rotate(); Assert.That(life.Housing.Place(), Is.True);
            context.SaveTo(2); var session = new GameSession(context, phases);
            life.Housing.Exit(); session.Load(2);
            Assert.That(phases.Current, Is.EqualTo(GamePhase.Home)); Assert.That(life.Housing.State.placed.Single().turns, Is.EqualTo(1));
            Assert.That(context.Save.resources.wood, Is.EqualTo(1)); Assert.That(context.Save.inventory.coins, Is.EqualTo(72));
            Assert.That(life.Equipment.Weapon.id, Is.EqualTo("sword"));
            // Recreate every feature to simulate a restarted scene, using only serialized state.
            life.Dispose(); life = new IslandLifeSession(context, phases, root.transform);
            Assert.That(life.Housing.State.placed.Single().furnitureId, Is.EqualTo("bed"));
            life.Housing.Exit(); Assert.That(context.World.player.transform.position, Is.EqualTo(outdoor));
            Assert.That(phases.Current, Is.EqualTo(GamePhase.Explore)); yield return null;
        }
        [UnityTest] public IEnumerator UnlockTrainingWeaponUseSkillAndRejectStaleWeaponAction()
        {
            context.World.motor.Teleport(life.Training.TargetPosition + Vector3.back * 2);
            life.Equipment.Equip("bow"); Assert.That(life.Training.Attack(true), Is.False);
            for (int i = 0; i < 30; i++)
            {
                if (life.Training.Health == 0) life.Training.ResetTarget();
                life.Training.Tick(1); Assert.That(life.Training.Attack(false), Is.True);
            }
            Assert.That(life.Equipment.Unlocked(life.Catalog.Weapon("bow_special")), Is.True);
            Assert.That(life.Equipment.Weapon.id, Is.EqualTo("bow")); life.Training.ResetTarget();
            life.Equipment.Equip("bow_special"); Assert.That(life.Training.Attack(true), Is.True);
            Assert.That(life.Training.Health, Is.EqualTo(65));
            life.Equipment.Equip("staff"); Assert.That(life.Training.Attack(true, "bow_special"), Is.False);
            context.SaveTo(0); context.LoadFrom(0); Assert.That(life.Mastery.Hits("ranger"), Is.EqualTo(31));
            yield return null;
        }
        [UnityTest] public IEnumerator PreviewCancelPauseAndRecallNeverDuplicateItems()
        {
            context.World.motor.Teleport(HousingSystem.Entrance); life.Housing.Enter();
            context.Save.resources.wood = 3; life.Equipment.SelectTool(LifeTool.Hammer); life.Crafting.Craft("chair");
            life.Housing.Select("chair"); life.Housing.Move(3, 0); Assert.That(life.Housing.IsValid, Is.False);
            life.Housing.Move(0, 4); Assert.That(life.Housing.IsValid, Is.True);
            phases.Cancel(); Assert.That(life.Housing.Place(), Is.False); Assert.That(phases.Current, Is.EqualTo(GamePhase.Home));
            phases.Cancel(); Assert.That(phases.Current, Is.EqualTo(GamePhase.Pause)); phases.Cancel();
            life.Housing.Cancel(); Assert.That(life.Housing.Available("chair"), Is.EqualTo(1));
            life.Housing.Select("chair"); life.Housing.Move(0, 4); Assert.That(life.Housing.Place(), Is.True);
            string instance = life.Housing.State.placed.Single().instanceId;
            Assert.That(life.Housing.Recall(instance), Is.True); Assert.That(life.Housing.Recall(instance), Is.False);
            context.SaveTo(); context.LoadFrom(); Assert.That(life.Housing.Available("chair"), Is.EqualTo(1));
            Assert.That(life.Housing.State.placed, Is.Empty); yield return null;
        }
        [UnityTest] public IEnumerator RestRegrowsSavedNodesAndCannotRunDuringCombat()
        {
            var node = life.Catalog.nodes.First(n => n.resource == LifeResource.Herb);
            context.World.motor.Teleport(node.position);
            yield return null; yield return null;
            Assert.That(life.Land.TickHeldGather(3,true),Is.True); context.LoadFrom(); Assert.That(life.Land.Depleted(node.id), Is.True);
            phases.Set(GamePhase.PredatorCombat); Assert.That(life.Land.Rest(), Is.False); Assert.That(context.Save.world.day, Is.EqualTo(1));
            phases.Set(GamePhase.Explore); context.World.motor.Teleport(HousingSystem.Entrance); life.Housing.Enter(); Assert.That(life.Land.Rest(), Is.True);
            life.Housing.Exit(); context.World.motor.Teleport(node.position);
            yield return null; yield return null;
            Assert.That(life.Land.TickHeldGather(3,true), Is.True);
            Assert.That(context.Save.resources.herbs, Is.EqualTo(4)); yield return null;
        }
        [UnityTest] public IEnumerator ImportedCharacterAndBothLocalizedScreensRender()
        {
            var animator = context.World.player.GetComponentInChildren<Animator>();
            Assert.That(animator, Is.Not.Null); Assert.That(animator.avatar, Is.Not.Null); Assert.That(animator.avatar.isValid, Is.True);
            Assert.That(animator.runtimeAnimatorController, Is.Not.Null);
            life.Equipment.Equip("sword");
            var session = new GameSession(context, phases); var host = root.AddComponent<HudTestHost>();
            host.Hud = new GameHud(phases, new Dictionary<GamePhase, HudScreen>
            {
                [GamePhase.Explore] = new ExploreHud(context, phases, new RelationshipSystem(context, phases), life),
                [GamePhase.Equipment] = new EquipmentHud(context, phases, life),
                [GamePhase.Home] = new HousingHud(context, phases, life)
            }, new HudOverlay(context, phases, session));
            string images = Path.GetFullPath("Builds/LifeReview"); Directory.CreateDirectory(images);
            foreach (bool korean in new[] { true, false })
            {
                context.Korean = korean;
                foreach (var phase in new[] { GamePhase.Explore, GamePhase.Equipment, GamePhase.Home })
                {
                    if (phase == GamePhase.Home)
                    {
                        phases.Set(GamePhase.Explore); context.World.motor.Teleport(HousingSystem.Entrance); life.Housing.Enter();
                        life.Equipment.SelectTool(LifeTool.Hammer); context.Save.resources.wood = 8; life.Crafting.Craft("bed");
                        life.Housing.Select("bed"); life.Housing.Move(1, 3);
                    }
                    phases.Set(phase); int before = host.DrawCount;
                    for (int frame = 0; frame < 6; frame++) yield return null;
                    Assert.That(host.DrawCount, Is.GreaterThan(before));
                    if (phase == GamePhase.Explore)
                    {
                        var socket = context.World.player.GetComponentsInChildren<Transform>().First(t => t.name == "Held Item Socket");
                        var renderers = socket.GetComponentsInChildren<Renderer>();
                        Assert.That(renderers.Length, Is.GreaterThan(0));
                        var bounds = renderers[0].bounds; foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
                        Assert.That(Mathf.Max(bounds.size.x, bounds.size.y, bounds.size.z), Is.InRange(.05f, 1.5f), "Held model must fit the character, regardless of source axis.");
                        float bottom = float.PositiveInfinity;
                        foreach (var skin in animator.GetComponentsInChildren<SkinnedMeshRenderer>())
                        {
                            var baked = new Mesh(); skin.BakeMesh(baked);
                            foreach (var vertex in baked.vertices) bottom = Mathf.Min(bottom, skin.transform.TransformPoint(vertex).y);
                            UnityEngine.Object.Destroy(baked);
                        }
                        Assert.That(bottom - context.World.player.transform.position.y, Is.InRange(-.35f, .35f), "Animated feet should stay at the player root.");
                    }
                    ScreenCapture.CaptureScreenshot(Path.Combine(images, phase + (korean ? "-ko.png" : "-en.png")));
                    for (int frame = 0; frame < 3; frame++) yield return null;
                    if (phase == GamePhase.Home) life.Housing.Exit();
                }
            }
            foreach (var renderer in context.World.player.GetComponentsInChildren<Renderer>())
                foreach (var material in renderer.sharedMaterials) Assert.That(material.shader.name, Does.StartWith("Universal Render Pipeline/"));
            LogAssert.NoUnexpectedReceived();
        }
        [UnityTest] public IEnumerator ImportedNatureKeepsUpAxisAndToolChopMovesTheArmWithItsGrip()
        {
            life.Equipment.SelectTool(LifeTool.Axe);
            yield return null;
            var player = context.World.player.transform;
            var swing = player.GetComponentInChildren<ToolSwing>();
            var hand = player.GetComponentsInChildren<Transform>().Single(t => t.name == "handslot.r");
            var upper = player.GetComponentsInChildren<Transform>().Single(t => t.name == "upperarm.r");
            var tree = life.Catalog.nodes.First(n => n.resource == LifeResource.Wood);
            var treeView = root.GetComponentsInChildren<Transform>().Single(t => t.name == tree.id);
            Assert.That(Quaternion.Angle(treeView.GetChild(0).localRotation, tree.model.transform.localRotation), Is.LessThan(.01f), "Keep the FBX root axis correction; otherwise trees lie on their side.");
            var treeBounds = treeView.GetComponentInChildren<Renderer>().bounds;
            Assert.That(treeBounds.min.y, Is.EqualTo(tree.position.y).Within(.02f));
            Assert.That(treeBounds.size.y, Is.EqualTo(tree.height).Within(.02f));
            var camera = context.World.camera;
            context.World.cameraRig.enabled = false;
            context.World.motor.Teleport(new Vector3(0, .68f, 4));
            // This test samples fixed arm poses. Finish ground settling and its short
            // landing scale animation before freezing locomotion presentation.
            for (int frame = 0; frame < 60 && !context.World.motor.Grounded; frame++) yield return null;
            Assert.That(context.World.motor.Grounded, Is.True);
            yield return new WaitForSeconds(.3f);
            player.GetComponent<CharacterPresentation>().enabled = false;
            camera.transform.position = player.position + new Vector3(3, 2, 3);
            camera.transform.LookAt(player.position + Vector3.up);
            swing.enabled = false;
            player.GetComponentInChildren<Animator>().enabled = false;
            swing.ApplyPose(.5f); Vector3 restingHand = player.InverseTransformPoint(hand.position); Quaternion restingArm = upper.rotation;
            Assert.That(Vector3.Angle(swing.transform.up, player.up), Is.EqualTo(25).Within(.1f), "Carried tool tilts 25 degrees away from the torso.");
            Assert.That(Vector3.Dot(swing.transform.up, player.right), Is.GreaterThan(.4f), "The carry tilt must point outward, not into the torso.");
            yield return null; CaptureTool(camera, "carry");
            swing.ApplyPose(.16f);
            Assert.That(Vector3.Distance(restingHand, player.InverseTransformPoint(hand.position)), Is.GreaterThan(.4f), "The arm must wind up, not just rotate a prop.");
            Assert.That(Quaternion.Angle(restingArm, upper.rotation), Is.GreaterThan(60));
            Assert.That(Vector3.Distance(swing.transform.position, hand.position), Is.LessThan(.001f));
            yield return null; CaptureTool(camera, "windup");
            swing.ApplyPose(.25f); yield return null; CaptureTool(camera, "impact");
            swing.ApplyPose(.5f);
            Assert.That(Vector3.Distance(restingHand, player.InverseTransformPoint(hand.position)), Is.LessThan(.02f), "Compare the pose relative to the player, independently of ground settling.");
            Assert.That(context.Save.resources.wood, Is.Zero, "Presentation must not grant resources.");
            yield return null;
        }
        private static void CaptureTool(Camera camera, string pose)
        {
            string directory = Path.GetFullPath("Builds/ToolReview"); Directory.CreateDirectory(directory);
            var target = new RenderTexture(960, 720, 24); var prior = RenderTexture.active;
            camera.targetTexture = target; camera.Render(); RenderTexture.active = target;
            var image = new Texture2D(960, 720, TextureFormat.RGB24, false);
            image.ReadPixels(new Rect(0, 0, 960, 720), 0, 0); image.Apply();
            File.WriteAllBytes(Path.Combine(directory, pose + ".png"), image.EncodeToPNG());
            camera.targetTexture = null; RenderTexture.active = prior;
            UnityEngine.Object.Destroy(image); UnityEngine.Object.Destroy(target);
        }
    }
}

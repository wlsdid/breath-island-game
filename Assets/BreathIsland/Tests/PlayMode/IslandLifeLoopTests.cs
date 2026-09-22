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
        [UnityTest] public IEnumerator HarvestCraftPlaceReloadAndReturnKeepInventoryJobAndOutdoorPosition()
        {
            life.Equipment.Equip("sword");
            foreach (var node in life.Catalog.nodes.Where(n => n.resource == LifeResource.Wood))
            {
                context.World.motor.Teleport(node.position + Vector3.up * .2f); life.Equipment.SelectTool(LifeTool.Axe);
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
            phases.Cancel(); Assert.That(life.Housing.Place(), Is.False); phases.Cancel();
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
            context.World.motor.Teleport(node.position); life.Land.Harvest(node.id); context.LoadFrom(); Assert.That(life.Land.Depleted(node.id), Is.True);
            phases.Set(GamePhase.PredatorCombat); Assert.That(life.Land.Rest(), Is.False); Assert.That(context.Save.world.day, Is.EqualTo(1));
            phases.Set(GamePhase.Explore); context.World.motor.Teleport(HousingSystem.Entrance); life.Housing.Enter(); Assert.That(life.Land.Rest(), Is.True);
            life.Housing.Exit(); context.World.motor.Teleport(node.position); Assert.That(life.Land.Harvest(node.id), Is.True);
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
    }
}

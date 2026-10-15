using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.TestTools;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;

namespace BreathIsland.Tests
{
    public sealed class CoreLoopTests
    {
        private GameObject root;
        private string directory;
        private GameContext context;
        private GamePhaseController phases;
        private FishingSystem fishing;
        private PredatorCombatSystem combat;
        private RelationshipSystem relationships;
        private FacilitySystem facilities;
        private SaveService saves;
        private Vector3 position;

        [UnitySetUp]
        public IEnumerator SetUp()
        {
            yield return LocalizationSettings.InitializationOperation;
            directory = Path.Combine(Application.temporaryCachePath, "breath-loop-" + Guid.NewGuid().ToString("N"));
            saves = new SaveService(directory);
            root = new GameObject("Loop Test World");
            var player = new GameObject("Player", typeof(CharacterController), typeof(ThirdPersonMotor));
            player.transform.SetParent(root.transform);
            var camera = new GameObject("Camera", typeof(Camera), typeof(IslandCameraRig)); camera.transform.SetParent(root.transform);
            camera.GetComponent<Camera>().clearFlags = CameraClearFlags.SolidColor;
            camera.GetComponent<Camera>().backgroundColor = new Color(.25f, .45f, .46f);
            var world = new WorldHandles { player = player, motor = player.GetComponent<ThirdPersonMotor>(), cameraRig = camera.GetComponent<IslandCameraRig>() };
            position = new Vector3(2.5f, 1.5f, 22f); player.transform.position = position;
            context = new GameContext(GameSaveData.CreateDefault(), saves, world, DefinitionCatalog.CreateVerticalSlice(), _ => { });
            phases = new GamePhaseController(world);
            combat = new PredatorCombatSystem(context, phases);
            fishing = new FishingSystem(context, phases, combat.Begin);
            relationships = new RelationshipSystem(context, phases);
            facilities = new FacilitySystem(context);
            phases.Set(GamePhase.Explore);
        }

        [UnityTearDown]
        public IEnumerator TearDown()
        {
            UnityEngine.Object.Destroy(root);
            if (Directory.Exists(directory)) Directory.Delete(directory, true);
            yield return null;
        }

        private void Catch()
        {
            fishing.Start(); fishing.Cast(); fishing.Tick(3f);
            Assert.That(fishing.Stage, Is.EqualTo(FishingStage.Bite)); fishing.Hook();
            Assert.That(fishing.Stage, Is.EqualTo(FishingStage.Hooked));
            for (int i = 0; i < 8; i++) { fishing.Ease(); fishing.Reel(); }
        }

        [UnityTest]
        public IEnumerator BiteRequiresFreshSpaceOrMousePressRatherThanHeldInput()
        {
            var keyboard=InputSystem.AddDevice<Keyboard>();
            var mouse=InputSystem.AddDevice<Mouse>();
            try
            {
                fishing.Start();fishing.Cast();
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space));InputSystem.Update();
                fishing.Tick(3);Assert.That(fishing.Stage,Is.EqualTo(FishingStage.Bite));
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space));InputSystem.Update();
                fishing.Tick(.1f);Assert.That(fishing.Stage,Is.EqualTo(FishingStage.Bite),"A held cast button must not auto-hook");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();
                InputSystem.QueueStateEvent(keyboard,new KeyboardState(Key.Space));InputSystem.Update();
                fishing.Tick(.01f);Assert.That(fishing.Stage,Is.EqualTo(FishingStage.Hooked));
                Assert.That(fishing.Progress,Is.EqualTo(8),"Hooking must not reel in the same update");
                InputSystem.QueueStateEvent(keyboard,new KeyboardState());InputSystem.Update();
                phases.Set(GamePhase.Explore);fishing.Start();fishing.Cast();fishing.Tick(3);
                InputSystem.QueueStateEvent(mouse,new MouseState().WithButton(MouseButton.Left));InputSystem.Update();
                fishing.Tick(.01f);Assert.That(fishing.Stage,Is.EqualTo(FishingStage.Hooked));
                Assert.That(fishing.Progress,Is.EqualTo(8));
            }
            finally{InputSystem.RemoveDevice(keyboard);InputSystem.RemoveDevice(mouse);}
            yield return null;
        }

        [UnityTest]
        public IEnumerator CatchFacilityNpcLoopPersistsProgressAndPosition()
        {
            var inventory = context.Save.inventory;
            Catch();
            Assert.That(phases.Current, Is.EqualTo(GamePhase.CatchResolution));
            var target = fishing.ActiveFish;
            fishing.Resolve(CatchDisposition.Sell);
            Assert.That(phases.Current, Is.EqualTo(GamePhase.Explore));
            Assert.That(context.Save.inventory.totalCatchCount, Is.EqualTo(1));
            fishing.Resolve(CatchDisposition.Sell);
            Assert.That(context.Save.inventory.totalCatchCount, Is.EqualTo(1), "Repeated GUI events must not duplicate rewards");
            phases.Set(GamePhase.Facility); facilities.Upgrade(FacilityId.Boatyard);
            Assert.That(context.Save.player.boatUnlocked, Is.True);
            Assert.That(context.Save.world.coralRouteUnlocked, Is.True);
            relationships.Open(NpcId.Nari); relationships.Talk(); relationships.GiveGift();
            Assert.That(context.Save.relationships.nari, Is.EqualTo(1.25f));
            phases.Set(GamePhase.Collection); phases.Set(GamePhase.Explore);
            Assert.That(fishing.ActiveFish, Is.SameAs(target));
            Assert.That(relationships.ActiveNpc, Is.EqualTo(NpcId.Nari));
            Assert.That(context.Save.inventory, Is.SameAs(inventory));
            Assert.That(context.World.player.transform.position, Is.EqualTo(position));
            var loaded = saves.Load();
            Assert.That(loaded.inventory.totalCatchCount, Is.EqualTo(1));
            Assert.That(loaded.facilities.boatyard, Is.EqualTo(1));
            Assert.That(loaded.relationships.nari, Is.EqualTo(1.25f));
            Assert.That(loaded.player.x, Is.EqualTo(position.x));
            yield return null;
        }

        [UnityTest]
        public IEnumerator PredatorVictoryKeepsTargetAndDefeatReturnsToShore()
        {
            context.Save.inventory.totalCatchCount = 2;
            Catch();
            Assert.That(phases.Current, Is.EqualTo(GamePhase.PredatorCombat));
            Assert.That(fishing.ActiveFish.id, Is.EqualTo("breaker"));
            combat.Act(0); Assert.That(combat.Charge, Is.Zero);
            combat.Act(1); Assert.That(combat.Health, Is.EqualTo(82));
            combat.Act(2); combat.Act(2); combat.Act(2);
            Assert.That(phases.Current, Is.EqualTo(GamePhase.CatchResolution));
            Assert.That(context.World.player.transform.position, Is.EqualTo(position));
            int material = context.Save.inventory.materials;
            fishing.Resolve(CatchDisposition.Material);
            Assert.That(context.Save.inventory.materials, Is.EqualTo(material + 9));
            context.Save.inventory.totalCatchCount = 6;
            Catch(); int bait = context.Save.inventory.bait;
            combat.Tick(4); combat.Tick(4); combat.Tick(4);
            Assert.That(phases.Current, Is.EqualTo(GamePhase.Explore));
            Assert.That(context.Save.inventory.bait, Is.EqualTo(bait - 1));
            Assert.That(context.World.player.transform.position, Is.EqualTo(new Vector3(0, 1.2f, 8)));
            Assert.That(saves.Load().player.z, Is.EqualTo(8));
            Assert.That(context.Save.inventory.totalCatchCount, Is.EqualTo(6));
            yield return null;
        }

        [UnityTest]
        public IEnumerator PauseAndAllScreensPreserveSessionReferences()
        {
            Catch(); var fish = fishing.ActiveFish; var save = context.Save;
            foreach (GamePhase phase in Enum.GetValues(typeof(GamePhase)))
            {
                phases.Set(phase);
                Assert.That(context.Save, Is.SameAs(save));
                Assert.That(fishing.ActiveFish, Is.SameAs(fish));
                Assert.That(context.World.player.transform.position, Is.EqualTo(position));
                Assert.That(context.World.motor.CanMove, Is.EqualTo(phase == GamePhase.Explore || phase == GamePhase.Home));
                Assert.That(context.World.cameraRig.firstPerson, Is.EqualTo(phase == GamePhase.Fishing || phase == GamePhase.PredatorCombat));
            }
            phases.Set(GamePhase.Explore); phases.Cancel();
            Assert.That(phases.Current, Is.EqualTo(GamePhase.Pause)); phases.Cancel();
            Assert.That(phases.Current, Is.EqualTo(GamePhase.Explore));
            yield return null;
        }

        [UnityTest]
        public IEnumerator LocalizedRuntimeQueriesSwitchBothLanguages()
        {
            Assert.That(context.Text("welcome"), Is.EqualTo("숨결섬에 오신 것을 환영해요."));
            Assert.That(context.Content.Fish[0].LocalizedName(true), Is.EqualTo("윤슬멸치"));
            context.Korean = false;
            Assert.That(context.Text("welcome"), Is.EqualTo("Welcome to Breath Island."));
            Assert.That(context.Content.Npcs[NpcId.Nari].LocalizedLine(false, 0), Is.EqualTo("The sea likes people who know how to wait."));
            yield return null;
        }

        [UnityTest]
        public IEnumerator SessionLoadRestoresSlotAndExitsTransientPhase()
        {
            var session = new GameSession(context, phases);
            for (int slot = 0; slot < 3; slot++)
            {
                context.Save.inventory.coins = 100 + slot;
                session.Save(slot);
                Assert.That(session.EventMessage, Does.Contain((slot + 1).ToString()));
            }
            Catch();
            for (int slot = 0; slot < 3; slot++)
            {
                session.Load(slot);
                Assert.That(context.Save.inventory.coins, Is.EqualTo(100 + slot));
                Assert.That(phases.Current, Is.EqualTo(GamePhase.Explore));
                Assert.That(context.World.player.transform.position, Is.EqualTo(position));
            }
            yield return null;
        }

        [UnityTest]
        public IEnumerator LineFailureConsumesOnlyCastBaitAndAllowsRetry()
        {
            int bait = context.Save.inventory.bait;
            fishing.Start(); fishing.Cast(); fishing.Tick(3); fishing.Hook(); fishing.Tick(100);
            Assert.That(fishing.Stage, Is.EqualTo(FishingStage.Failed));
            Assert.That(context.Save.inventory.bait, Is.EqualTo(bait - 1));
            Assert.That(context.Save.inventory.coins, Is.EqualTo(72));
            fishing.Retry(); Assert.That(fishing.Stage, Is.EqualTo(FishingStage.Ready));
            yield return null;
        }

        [UnityTest]
        public IEnumerator NightCastHandlesEmptyPoolWithoutChangingSpawnHoursOrCost()
        {
            context.Save.player.hour = 23;
            int bait = context.Save.inventory.bait;
            fishing.Start(); fishing.Cast(); fishing.Tick(3);
            Assert.That(fishing.Stage, Is.EqualTo(FishingStage.Ready));
            Assert.That(context.Save.player.hour, Is.Zero);
            Assert.That(context.Save.inventory.bait, Is.EqualTo(bait - 1));
            Assert.That(context.Save.inventory.totalCatchCount, Is.Zero);
            yield return null;
        }

        [UnityTest]
        public IEnumerator FallenPositionRecoveryPreservesProgressAndValidCoordinates()
        {
            var saved = context.Save.player;
            saved.x = position.x; saved.y = position.y; saved.z = 0;
            context.RestorePosition();
            Assert.That(context.World.player.transform.position, Is.EqualTo(new Vector3(position.x, position.y, 0)));
            saved.x = -11.7f; saved.y = -464f; saved.z = 16f;
            context.RestorePosition();
            Assert.That(context.World.player.transform.position, Is.EqualTo(new Vector3(0, 1.2f, 8)));
            Assert.That(saved.y, Is.EqualTo(-464f), "Recovery must not rewrite the loaded save object");
            context.World.motor.Teleport(new Vector3(30, -6, 30));
            context.Positions.Tick();
            Assert.That(context.World.player.transform.position, Is.EqualTo(new Vector3(0, 1.2f, 8)));
            Assert.That(context.Save.inventory.coins, Is.EqualTo(72));
            Assert.That(context.Save.inventory.bait, Is.EqualTo(8));
            Assert.That(context.World.player.GetComponent<CharacterController>().enabled, Is.True);
            yield return null;
        }

        [UnityTest]
        public IEnumerator EveryHudScreenRendersInBothLocales()
        {
            Catch(); combat.Begin(); relationships.Open(NpcId.Nari);
            var session = new GameSession(context, phases);
            var host = root.AddComponent<HudTestHost>();
            using var life = new IslandLifeSession(context, phases, root.transform);
            host.Hud = new GameHud(phases, new Dictionary<GamePhase, HudScreen>
            {
                [GamePhase.Explore] = new ExploreHud(context, phases, relationships),
                [GamePhase.Fishing] = new FishingHud(context, phases, fishing),
                [GamePhase.CatchResolution] = new CatchResolutionHud(context, phases, fishing),
                [GamePhase.PredatorCombat] = new PredatorCombatHud(context, phases, combat),
                [GamePhase.Dialogue] = new DialogueHud(context, phases, relationships),
                [GamePhase.Facility] = new FacilityHud(context, phases, facilities),
                [GamePhase.Collection] = new CollectionHud(context, phases),
                [GamePhase.Pause] = new PauseHud(context, phases, session),
                [GamePhase.Equipment] = new EquipmentHud(context, phases, life),
                [GamePhase.Home] = new HousingHud(context, phases, life),
                [GamePhase.HomeDecorate] = new HomeDecorateHud(context, phases, life),
                [GamePhase.HomeStorage] = new HomeStorageHud(context, phases, life.Storage),
                [GamePhase.HomeRest] = new HomeRestHud(context, phases, life.Home)
            }, new HudOverlay(context, phases, session));
            var missingLabels = new List<string>();
            foreach (bool korean in new[] { true, false })
            {
                context.Korean = korean;
                foreach (GamePhase phase in Enum.GetValues(typeof(GamePhase)))
                {
                    phases.Set(phase); int before = host.DrawCount;
                    yield return null; yield return null;
                    Assert.That(host.DrawCount, Is.GreaterThan(before), phase.ToString());
                    yield return new WaitForSeconds(.18f);
                    Canvas.ForceUpdateCanvases();
                    foreach (var label in UnityEngine.Object.FindObjectsByType<UnityEngine.UI.Text>(FindObjectsSortMode.None))
                        if (!string.IsNullOrWhiteSpace(label.text))
                            if (label.cachedTextGenerator.vertexCount == 0) missingLabels.Add(phase + ": " + label.text);
                    string review = System.IO.Path.GetFullPath("Builds/NotebookReview/AllScreens");
                    System.IO.Directory.CreateDirectory(review);
                    ScreenCapture.CaptureScreenshot(System.IO.Path.Combine(review, phase + (korean ? "-ko.png" : "-en.png")));
                    yield return null; yield return null;
                }
            }
            Assert.That(missingLabels, Is.Empty, "Missing rendered labels");
            LogAssert.NoUnexpectedReceived();
        }
    }
}

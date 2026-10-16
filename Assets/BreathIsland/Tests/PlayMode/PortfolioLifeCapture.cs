using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.TestTools;

namespace BreathIsland.Tests
{
    public sealed class PortfolioLifeCapture
    {
        private const string Output = "Builds/Portfolio/FinalCapture-20261016";
        [UnityTest, Explicit("Offline portfolio life capture")]
        public IEnumerator CaptureMovementAndFurniture()
        {
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            var root = new GameObject("Portfolio life capture");
            var world = ProceduralWorld.Build(root.transform);
            var save = JsonUtility.FromJson<GameSaveData>(File.ReadAllText("Documentation/Portfolio/Seeds/village.json"));
            string saves = Path.Combine(Application.temporaryCachePath, "portfolio-life-" + Guid.NewGuid().ToString("N"));
            var context = new GameContext(save, new SaveService(saves), world, DefinitionCatalog.CreateVerticalSlice(), _ => {});
            var phases = new GamePhaseController(world);
            var life = new IslandLifeSession(context, phases, root.transform);
            var session = new GameSession(context, phases);
            var hud = new GameHud(phases, new Dictionary<GamePhase, HudScreen> {
                [GamePhase.Home] = new HousingHud(context, phases, life),
                [GamePhase.HomeStorage] = new HomeStorageHud(context, phases, life.Storage),
                [GamePhase.HomeRest] = new HomeRestHud(context, phases, life.Home)
            }, new HudOverlay(context, phases, session));
            var keyboard = InputSystem.AddDevice<Keyboard>();
            var axes = new GameObject("Fixed movement axes"); axes.transform.SetParent(root.transform);
            world.motor.SetCamera(axes.transform); world.cameraRig.enabled = false;
            int rate = Time.captureFramerate; Time.captureFramerate = 60;
            try
            {
                phases.Set(GamePhase.Explore);
                Assert.That(life.Equipment.Equip(""), Is.True);
                world.motor.Teleport(new Vector3(0, 1.1f, 14));
                yield return new WaitForSeconds(.7f);
                var start = world.player.transform.position; float peak = start.y;
                using (var movie = new PortfolioMovieWriter(world.camera, Output, "04-walk-jump"))
                for (int i = 0; i < 480; i++) {
                    if (i == 30) Keys(keyboard, Key.W);
                    if (i == 150) Keys(keyboard, Key.W, Key.Space);
                    if (i == 151) Keys(keyboard, Key.W);
                    if (i == 210) Keys(keyboard);
                    var p = world.player.transform.position;
                    world.camera.transform.position = new Vector3(p.x + 3, start.y + 2, p.z + 4);
                    world.camera.transform.LookAt(new Vector3(p.x, start.y + .8f, p.z));
                    yield return new WaitForEndOfFrame(); peak = Mathf.Max(peak, world.player.transform.position.y);
                    movie.Frame(); if (i == 100) movie.Still("05-walking"); if (i == 173) movie.Still("06-jump");
                }
                Assert.That(world.player.transform.position.z - start.z, Is.GreaterThan(5));
                Assert.That(peak - start.y, Is.GreaterThan(.4f)); Assert.That(world.motor.Grounded, Is.True);
                world.motor.Teleport(HousingSystem.Entrance); Assert.That(life.Housing.Enter(), Is.True);
                // Each new shot starts off-camera; no teleport is presented as walking.
                world.motor.Teleport(HousingSystem.InteriorOrigin + new Vector3(6.2f, .05f, 2.2f));
                world.player.transform.forward = Vector3.forward;
                world.camera.transform.position = HousingSystem.InteriorOrigin + new Vector3(10, 7, 11);
                world.camera.transform.LookAt(HousingSystem.InteriorOrigin + new Vector3(4.5f, .7f, 6));
                yield return new WaitForSeconds(.5f);
                using (var movie = new PortfolioMovieWriter(world.camera, Output, "05-chair"))
                for (int i = 0; i < 480; i++) {
                    if (i == 15) Keys(keyboard, Key.W);
                    if (i == 105) Keys(keyboard);
                    if (i >= 110 && i < 150) world.player.transform.rotation = Quaternion.Slerp(world.player.transform.rotation, Quaternion.LookRotation(Vector3.left), .12f);
                    if (i == 165) { Assert.That(life.Home.Nearby?.furnitureId, Is.EqualTo("chair")); Assert.That(life.Home.Use(), Is.True); }
                    if (i == 330) { life.Home.Stand(); Assert.That(life.Home.Seated, Is.False); }
                    RenderHud(hud, world.camera);
                    yield return new WaitForEndOfFrame(); movie.Frame(); if (i == 260) movie.Still("07-seated");
                }
                world.motor.Teleport(HousingSystem.InteriorOrigin + new Vector3(3.2f, .05f, 3.5f));
                world.player.transform.forward = Vector3.left; Physics.SyncTransforms();
                yield return new WaitForSeconds(.5f);
                using (var movie = new PortfolioMovieWriter(world.camera, Output, "06-storage"))
                for (int i = 0; i < 480; i++) {
                    if (i == 120) { Assert.That(life.Home.Nearby?.furnitureId, Is.EqualTo("chest")); Assert.That(life.Home.Use(), Is.True); }
                    if (i == 240) Assert.That(life.Storage.Deposit("wood", false, 5), Is.EqualTo(5));
                    if (i == 360) Assert.That(life.Storage.Withdraw(0, 2), Is.EqualTo(2));
                    if (i == 450) phases.Set(GamePhase.Home);
                    RenderHud(hud, world.camera);
                    yield return new WaitForEndOfFrame(); movie.Frame(); if (i == 300) movie.Still("08-storage");
                }
                world.motor.Teleport(HousingSystem.InteriorOrigin + new Vector3(3.4f, .05f, 6));
                world.player.transform.forward = Vector3.left; Physics.SyncTransforms();
                yield return new WaitForSeconds(.5f);
                int day = save.world.day;
                using (var movie = new PortfolioMovieWriter(world.camera, Output, "07-rest"))
                for (int i = 0; i < 360; i++) {
                    if (i == 120) { Assert.That(life.Home.Nearby?.furnitureId, Is.EqualTo("bed")); Assert.That(life.Home.Use(), Is.True); }
                    if (i == 270) Assert.That(life.Home.ConfirmRest(), Is.True);
                    RenderHud(hud, world.camera);
                    yield return new WaitForEndOfFrame(); movie.Frame(); if (i == 210) movie.Still("09-rest");
                }
                Assert.That(save.world.day, Is.EqualTo(day + 1));
                LogAssert.NoUnexpectedReceived();
            }
            finally {
                Time.captureFramerate = rate; InputSystem.RemoveDevice(keyboard); hud.Dispose(); life.Dispose();
                UnityEngine.Object.Destroy(root); Cursor.lockState = CursorLockMode.None;
                if (Directory.Exists(saves)) Directory.Delete(saves, true);
            }
        }
        private static void Keys(Keyboard keyboard, params Key[] keys) => InputSystem.QueueStateEvent(keyboard, new KeyboardState(keys));
        private static void RenderHud(GameHud hud, Camera camera)
        {
            hud.Draw(1280, 720);
            foreach (var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None)) {
                canvas.renderMode = RenderMode.ScreenSpaceCamera; canvas.worldCamera = camera; canvas.planeDistance = .5f;
            }
            Canvas.ForceUpdateCanvases();
        }
    }
}

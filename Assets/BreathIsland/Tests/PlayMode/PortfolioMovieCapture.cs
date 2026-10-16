using System;
using System.Collections;
using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.TestTools;

namespace BreathIsland.Tests
{
    // Opt-in presentation fixture, not evidence of manual Windows gameplay.
    public sealed class PortfolioMovieCapture
    {
        [UnityTest, Explicit("Offline portfolio capture; requires the local FFmpeg executable")]
        public IEnumerator CaptureLatestVisuals()
        {
            yield return UnityEngine.Localization.Settings.LocalizationSettings.InitializationOperation;
            string output = "Builds/Portfolio/FinalCapture-20261016";
            string saves = Path.Combine(Application.temporaryCachePath, "portfolio-movie-" + Guid.NewGuid().ToString("N"));
            var root = new GameObject("Offline portfolio capture");
            var world = ProceduralWorld.Build(root.transform);
            var save = JsonUtility.FromJson<GameSaveData>(File.ReadAllText("Documentation/Portfolio/Seeds/village.json"));
            var context = new GameContext(save, new SaveService(saves), world, DefinitionCatalog.CreateVerticalSlice(), _ => {});
            var phases = new GamePhaseController(world);
            var life = new IslandLifeSession(context, phases, root.transform);
            var fishing = new FishingSystem(context, phases, () => phases.Set(GamePhase.PredatorCombat));
            root.AddComponent<FishingPresentation>().Initialize(world, fishing, phases);
            int previousRate = Time.captureFramerate; var random = UnityEngine.Random.state;
            Time.captureFramerate = 60; UnityEngine.Random.InitState(42);
            world.motor.enabled = false; world.motor.CanMove = false; world.cameraRig.enabled = false;
            try
            {
                world.motor.Teleport(new Vector3(0, .62f, 8));
                yield return new WaitForSeconds(.5f);
                using (var movie = new PortfolioMovieWriter(world.camera, output, "01-village"))
                for (int frame = 0; frame < 420; frame++) {
                    float i = frame / 2f;
                    float t = i / 209f;
                    world.camera.transform.position = Vector3.Lerp(new Vector3(20, 15, 27), new Vector3(13, 12, 25), t);
                    world.camera.transform.LookAt(new Vector3(0, 1, 2));
                    yield return new WaitForEndOfFrame(); movie.Frame(); if (i == 100) movie.Still("01-village");
                }
                using (var movie = new PortfolioMovieWriter(world.camera, output, "02-coast"))
                for (int frame = 0; frame < 360; frame++) {
                    float i = frame / 2f;
                    world.camera.transform.position = Vector3.Lerp(new Vector3(15, 5, 39), new Vector3(9, 3, 37), i / 179f);
                    world.camera.transform.LookAt(new Vector3(0, 0, 27));
                    yield return new WaitForEndOfFrame(); movie.Frame(); if (i == 90) movie.Still("02-coast");
                }
                world.motor.Teleport(new Vector3(0, .93f, 23));
                fishing.Start(); world.camera.transform.position = new Vector3(0, 2.2f, 23);
                world.camera.transform.rotation = Quaternion.Euler(15, 0, 0);
                yield return new WaitForSeconds(.5f); fishing.Cast();
                using (var movie = new PortfolioMovieWriter(world.camera, output, "03-fishing"))
                for (int frame = 0; frame < 660; frame++) {
                    float i = frame / 2f;
                    fishing.Tick(1f / 60);
                    if (fishing.Stage == FishingStage.Bite && fishing.ReactionRemaining < 2.3f) fishing.Hook();
                    if (fishing.Stage == FishingStage.Hooked && i % 60 == 0) { fishing.Ease(); fishing.Reel(); }
                    yield return new WaitForEndOfFrame(); movie.Frame();
                    if (i == 15) movie.Still("03-cast"); if (i == 140) movie.Still("04-fishing");
                }
                phases.Set(GamePhase.Explore);
                var standing = life.Training.TargetPosition - Vector3.forward * 2; standing.y = .62f;
                world.motor.Teleport(standing); world.player.transform.rotation = Quaternion.identity;
                world.motor.enabled = true;
                yield return new WaitForSeconds(.5f);
                world.motor.enabled = false; world.motor.CanMove = false;
                save.equipment.achievements.AddRange(new[] { "craft", "training", "gather" });
                foreach (string id in new[] { "sword_special", "bow_special", "staff_special" }) {
                    Assert.That(life.Equipment.Equip(id), Is.True);
                    life.Training.ResetTarget(); life.Training.Tick(10);
                    yield return new WaitForSeconds(.7f);
                    using (var movie = new PortfolioMovieWriter(world.camera, output, id))
                    for (int frame = 0; frame < 420; frame++) {
                    float i = frame / 2f;
                        bool wide = true;
                        world.camera.transform.position = wide ? standing + new Vector3(2, 1.7f, 3.5f) : standing + new Vector3(2, 1.4f, 3);
                        world.camera.transform.LookAt(wide ? standing + Vector3.forward * .9f + Vector3.up * .9f : standing + Vector3.up * .8f);
                        life.Training.Tick(1f / 60);
                        if (i == 30) Assert.That(life.Training.Attack(false), Is.True);
                        if (i == 110) { life.Training.ResetTarget(); Assert.That(life.Training.Attack(true), Is.True); }
                        yield return new WaitForEndOfFrame(); movie.Frame();
                        if (i == 15) movie.Still(id + "-outfit");
                        if (i == (id == "bow_special" ? 132 : 116)) movie.Still(id + "-skill");
                    }
                }
                LogAssert.NoUnexpectedReceived();
            }
            finally {
                Time.captureFramerate = previousRate; UnityEngine.Random.state = random;
                life.Dispose(); UnityEngine.Object.Destroy(root); Cursor.lockState = CursorLockMode.None;
                if (Directory.Exists(saves)) Directory.Delete(saves, true);
            }
        }
    }
}

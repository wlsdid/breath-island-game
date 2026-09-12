using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace BreathIsland.Tests
{
    public sealed class GameLogicTests
    {
        [Test]
        public void ReelingAdvancesProgressAndRaisesTension()
        {
            float progress = 8f;
            float tension = 30f;
            FishingMath.Reel(ref progress, ref tension);
            Assert.That(progress, Is.GreaterThan(8f));
            Assert.That(tension, Is.GreaterThan(30f));
        }

        [Test]
        public void EasingNeverDropsBelowSafeMinimum()
        {
            float tension = 5f;
            FishingMath.Ease(ref tension);
            Assert.That(tension, Is.EqualTo(12f));
        }

        [Test]
        public void CatchDispositionMutatesOnlySelectedReward()
        {
            var save = GameSaveData.CreateDefault();
            var fish = ScriptableObject.CreateInstance<FishDefinition>();
            fish.id = "test"; fish.coinValue = 25; fish.materialValue = 4;
            int materials = save.inventory.materials;
            GameEconomy.ApplyCatch(save, fish, CatchDisposition.Sell);
            Assert.That(save.inventory.coins, Is.EqualTo(97));
            Assert.That(save.inventory.materials, Is.EqualTo(materials));
            Assert.That(save.inventory.discoveredFish, Contains.Item("test"));
        }

        [Test]
        public void FacilitiesCanBeUpgradedInEitherOrder()
        {
            var aquariumFirst = GameSaveData.CreateDefault();
            var aquarium = Facility(FacilityId.Aquarium);
            var boatyard = Facility(FacilityId.Boatyard);
            Assert.That(GameEconomy.TryUpgrade(aquariumFirst, aquarium), Is.True);
            aquariumFirst.inventory.coins += 60;
            aquariumFirst.inventory.materials += 3;
            Assert.That(GameEconomy.TryUpgrade(aquariumFirst, boatyard), Is.True);
            Assert.That(aquariumFirst.player.boatUnlocked, Is.True);
        }

        [Test]
        public void SaveRoundTripPreservesProgress()
        {
            string directory = Path.Combine(Path.GetTempPath(), "breath-island-tests", System.Guid.NewGuid().ToString("N"));
            try
            {
                var service = new SaveService(directory);
                var original = GameSaveData.CreateDefault();
                original.inventory.coins = 314;
                original.relationships.Add(NpcId.Nari, 1.5f);
                service.Save(original, 1);
                GameSaveData loaded = service.Load(1);
                Assert.That(loaded.inventory.coins, Is.EqualTo(314));
                Assert.That(loaded.relationships.nari, Is.EqualTo(1.5f));
                Assert.That(loaded.version, Is.EqualTo(GameSaveData.CurrentVersion));
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        [Test]
        public void BrokenSaveReturnsSafeNewGame()
        {
            string directory = Path.Combine(Path.GetTempPath(), "breath-island-tests", System.Guid.NewGuid().ToString("N"));
            try
            {
                Directory.CreateDirectory(directory);
                File.WriteAllText(Path.Combine(directory, "autosave.json"), "not json");
                GameSaveData loaded = new SaveService(directory).Load();
                Assert.That(loaded.inventory.bait, Is.EqualTo(8));
            }
            finally
            {
                if (Directory.Exists(directory)) Directory.Delete(directory, true);
            }
        }

        [Test]
        public void VerticalSliceCatalogKeepsFeatureContentSeparatedFromBootstrap()
        {
            DefinitionCatalog catalog = DefinitionCatalog.CreateVerticalSlice();
            Assert.That(catalog.Fish, Has.Count.EqualTo(6));
            Assert.That(catalog.Facilities.Keys, Is.EquivalentTo(new[] { FacilityId.Boatyard, FacilityId.Aquarium }));
            Assert.That(catalog.Npcs.Keys, Is.EquivalentTo(new[] { NpcId.Nari, NpcId.Maru }));
        }

        private static FacilityDefinition Facility(FacilityId id)
        {
            var value = ScriptableObject.CreateInstance<FacilityDefinition>();
            value.id = id;
            value.coinCosts = new[] { 60, 110 };
            value.materialCosts = new[] { 3, 7 };
            return value;
        }
    }
}

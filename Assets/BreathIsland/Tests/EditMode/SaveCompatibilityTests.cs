using System;
using System.IO;
using NUnit.Framework;
using UnityEngine;

namespace BreathIsland.Tests
{
    public sealed class SaveCompatibilityTests
    {
        private string root;
        private SaveService service;
        [SetUp] public void SetUp() { root = Path.Combine(Path.GetTempPath(), "breath-save-" + Guid.NewGuid().ToString("N")); service = new SaveService(root); }
        [TearDown] public void TearDown() { Directory.Delete(root, true); }

        [TestCase(-1)] [TestCase(0)] [TestCase(1)] [TestCase(2)]
        public void LegacyV1RoundTripPreservesAllFieldsAndSource(int slot)
        {
            const string legacy = "{\"version\":1,\"savedAtUtcTicks\":1234567,\"player\":{\"x\":3.25,\"y\":1.5,\"z\":0,\"hour\":22,\"boatUnlocked\":true,\"endingSeen\":true},\"inventory\":{\"coins\":314,\"materials\":19,\"bait\":6,\"discoveredFish\":[\"glimmer\",\"breaker\"],\"displayedFish\":[\"breaker\"],\"totalCatchCount\":9},\"relationships\":{\"nari\":2.5,\"maru\":1.25},\"facilities\":{\"boatyard\":2,\"aquarium\":1},\"world\":{\"weather\":\"Clear\",\"day\":4,\"coralRouteUnlocked\":true}}";
            File.WriteAllText(service.PathForSlot(slot), legacy);
            var data = service.Load(slot);
            Assert.That(File.ReadAllText(service.PathForSlot(slot)), Is.EqualTo(legacy));
            var expected = JsonUtility.FromJson<GameSaveData>(legacy);
            Assert.That(JsonUtility.ToJson(data), Is.EqualTo(JsonUtility.ToJson(expected)));
            service.Save(data, slot);
            var roundTrip = service.Load(slot);
            expected.savedAtUtcTicks = roundTrip.savedAtUtcTicks;
            Assert.That(JsonUtility.ToJson(roundTrip), Is.EqualTo(JsonUtility.ToJson(expected)));
            Assert.That(File.ReadAllText(service.PathForSlot(slot) + ".bak"), Is.EqualTo(legacy));
        }

        [Test] public void FourSlotsAreIndependent()
        {
            for (int slot = -1; slot < 3; slot++) { var data = GameSaveData.CreateDefault(); data.inventory.coins = 100 + slot; service.Save(data, slot); }
            for (int slot = -1; slot < 3; slot++) Assert.That(service.Load(slot).inventory.coins, Is.EqualTo(100 + slot));
            Assert.That(Directory.GetFiles(root, "*.json"), Has.Length.EqualTo(4));
        }

        [Test] public void VersionZeroRepairsMissingFieldsWithoutChangingKnownValues()
        {
            var migrated = SaveMigration.Apply(JsonUtility.FromJson<GameSaveData>("{\"version\":0,\"inventory\":{\"coins\":99,\"discoveredFish\":null},\"player\":null}"));
            Assert.That(migrated.version, Is.EqualTo(1)); Assert.That(migrated.inventory.coins, Is.EqualTo(99));
            Assert.That(migrated.player, Is.Not.Null); Assert.That(migrated.inventory.discoveredFish, Is.Not.Null);
        }

        [Test] public void UnknownFutureVersionIsRejectedWithoutRewritingFile()
        {
            const string json = "{\"version\":99,\"inventory\":{\"coins\":555}}";
            File.WriteAllText(service.PathForSlot(0), json);
            Assert.Throws<NotSupportedException>(() => service.Load(0));
            Assert.That(File.ReadAllText(service.PathForSlot(0)), Is.EqualTo(json));
        }

        [Test] public void CorruptionRecoversLastBackup()
        {
            var data = GameSaveData.CreateDefault(); data.inventory.coins = 123; service.Save(data);
            data.inventory.coins = 456; service.Save(data);
            File.WriteAllText(service.PathForSlot(-1), "broken");
            Assert.That(service.Load().inventory.coins, Is.EqualTo(123));
        }
    }
}

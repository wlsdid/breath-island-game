using NUnit.Framework;

namespace BreathIsland.Tests
{
    public sealed class FeatureRulesTests
    {
        [TestCase(CatchDisposition.Sell)] [TestCase(CatchDisposition.Record)]
        [TestCase(CatchDisposition.Display)] [TestCase(CatchDisposition.Material)]
        public void CatchRewardsAndDiscoveryKeepOriginalBalance(CatchDisposition disposition)
        {
            var save = GameSaveData.CreateDefault();
            var fish = DefinitionCatalog.CreateVerticalSlice().Fish[0];
            GameEconomy.ApplyCatch(save, fish, disposition);
            Assert.That(save.inventory.coins, Is.EqualTo(disposition == CatchDisposition.Sell ? 90 : 72));
            Assert.That(save.inventory.materials, Is.EqualTo(disposition == CatchDisposition.Material ? 5 : 4));
            Assert.That(save.inventory.discoveredFish, Is.EqualTo(new[] { "glimmer" }));
            Assert.That(save.inventory.displayedFish.Count, Is.EqualTo(disposition == CatchDisposition.Display ? 1 : 0));
            GameEconomy.ApplyCatch(save, fish, disposition);
            Assert.That(save.inventory.discoveredFish.Count, Is.EqualTo(1));
            Assert.That(save.inventory.totalCatchCount, Is.EqualTo(2));
        }

        [TestCase(FacilityId.Aquarium)] [TestCase(FacilityId.Boatyard)]
        public void FacilityMaximumAndInsufficientFundsNeverConsumeResources(FacilityId id)
        {
            var save = GameSaveData.CreateDefault();
            var definition = DefinitionCatalog.CreateVerticalSlice().Facilities[id];
            save.inventory.coins = 59;
            Assert.That(GameEconomy.TryUpgrade(save, definition), Is.False);
            Assert.That(save.inventory.coins, Is.EqualTo(59));
            save.inventory.coins = 170; save.inventory.materials = 10;
            Assert.That(GameEconomy.TryUpgrade(save, definition), Is.True);
            Assert.That(GameEconomy.TryUpgrade(save, definition), Is.True);
            Assert.That(save.facilities.Get(id), Is.EqualTo(2));
            Assert.That(save.inventory.coins, Is.Zero); Assert.That(save.inventory.materials, Is.Zero);
            Assert.That(GameEconomy.TryUpgrade(save, definition), Is.False);
            Assert.That(save.inventory.coins, Is.Zero);
        }

        [TestCase(NpcId.Nari)] [TestCase(NpcId.Maru)]
        public void FriendshipClampsWithoutChangingOtherNpc(NpcId id)
        {
            var state = new RelationshipState(); state.Add(id, 5);
            Assert.That(state.Get(id), Is.EqualTo(3));
            Assert.That(state.Get(id == NpcId.Nari ? NpcId.Maru : NpcId.Nari), Is.Zero);
            state.Add(id, -10); Assert.That(state.Get(id), Is.Zero);
        }

        [Test] public void FishingPassiveRulesClampAtTheirBounds()
        {
            Assert.That(FishingMath.PassiveTension(95, 13, 10), Is.EqualTo(100));
            Assert.That(FishingMath.PassiveProgress(2, 10), Is.Zero);
        }
    }
}

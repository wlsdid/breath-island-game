using System.Linq;
using NUnit.Framework;
using UnityEngine;
namespace BreathIsland.Tests
{
    public sealed class InventoryLayoutTests
    {
        [Test] public void LayoutKeepsEmptyPositionsAndRejectsDuplicateOrRemovedIds()
        {
            var order=InventoryLayoutSystem.Arrange(new[]{"", "Tools:Axe", "Tools:Axe", "unknown"},new[]{"Tools:Axe","Resources:wood"});
            Assert.That(order[0],Is.EqualTo("Resources:wood"));Assert.That(order[1],Is.EqualTo("Tools:Axe"));
            Assert.That(order[2],Is.Empty);Assert.That(order[3],Is.Empty);Assert.That(order.Count,Is.EqualTo(24));
        }
        [Test] public void PagesExpandWithoutDroppingItems()
        {
            var keys=Enumerable.Range(0,65).Select(i=>"item"+i).ToArray();
            var order=InventoryLayoutSystem.Arrange(null,keys);
            Assert.That(order.Count,Is.EqualTo(72));Assert.That(order.Where(k=>k!=""),Is.EquivalentTo(keys));
        }
        [TestCase(1)] [TestCase(2)] [TestCase(3)] [TestCase(4)]
        public void LegacySaveKeepsQuantitiesAndOptionalSlotOrder(int version)
        {
            var data=SaveMigration.Apply(JsonUtility.FromJson<GameSaveData>("{\"version\":"+version+",\"inventory\":{\"coins\":321,\"bait\":17},\"resources\":{\"wood\":987}}"));
            Assert.That(data.inventory.slotOrder,Is.Empty);Assert.That(data.inventory.coins,Is.EqualTo(321));
            Assert.That(data.inventory.bait,Is.EqualTo(17));Assert.That(data.resources.wood,Is.EqualTo(987));
            data.inventory.slotOrder.AddRange(new[]{"","Tools:Axe"});
            var restored=SaveMigration.Apply(JsonUtility.FromJson<GameSaveData>(JsonUtility.ToJson(data)));
            Assert.That(restored.inventory.slotOrder,Is.EqualTo(data.inventory.slotOrder));
            Assert.That(restored.resources.wood,Is.EqualTo(987));
        }
    }
}

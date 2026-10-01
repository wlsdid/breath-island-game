using NUnit.Framework;
using UnityEngine;
namespace BreathIsland.Tests
{
    public sealed class StarterBedRulesTests
    {
        [Test] public void NewHomeReceivesOneBedWithoutCraftingAndCannotRepeat()
        {
            var data=GameSaveData.CreateDefault();
            Assert.That(StarterBedSystem.Ensure(data.housing),Is.True);
            Assert.That(data.housing.stock.Count,Is.EqualTo(1));Assert.That(data.housing.stock[0].id,Is.EqualTo("bed"));
            Assert.That(data.housing.stock[0].count,Is.EqualTo(1));Assert.That(data.resources.craftedFurniture,Is.Zero);
            Assert.That(StarterBedSystem.Ensure(data.housing),Is.False);
            var restored=SaveMigration.Apply(JsonUtility.FromJson<GameSaveData>(JsonUtility.ToJson(data)));
            Assert.That(StarterBedSystem.Ensure(restored.housing),Is.False);Assert.That(restored.housing.stock[0].count,Is.EqualTo(1));
        }
        [TestCase("bag")] [TestCase("placed")] [TestCase("chest")]
        public void ExistingBedInAnyLocationPreventsExtraProvision(string location)
        {
            var housing=new HousingState();
            if(location=="bag")housing.stock.Add(new FurnitureStock{id="bed",count=2});
            if(location=="placed")housing.placed.Add(new PlacedFurniture{instanceId="bed-a",furnitureId="bed"});
            if(location=="chest")housing.chests.Add(new ChestState{instanceId="chest-a",slots=new(){new StorageSlot{id="bed",furniture=true,count=1}}});
            Assert.That(StarterBedSystem.Ensure(housing),Is.False);Assert.That(housing.starterBedGranted,Is.True);
            Assert.That(housing.stock.Count,Is.EqualTo(location=="bag"?1:0));
            if(location=="bag")Assert.That(housing.stock[0].count,Is.EqualTo(2));
        }
        [TestCase(1)] [TestCase(2)] [TestCase(3)]
        public void OldHomesWithNoWoodReceiveABedWithoutChangingOldFields(int version)
        {
            var data=SaveMigration.Apply(JsonUtility.FromJson<GameSaveData>("{\"version\":"+version+",\"inventory\":{\"coins\":123},\"resources\":{\"wood\":0,\"craftedFurniture\":3},\"housing\":{\"stock\":[{\"id\":\"chair\",\"count\":3}]}}"));
            Assert.That(StarterBedSystem.Ensure(data.housing),Is.True);Assert.That(data.inventory.coins,Is.EqualTo(123));
            Assert.That(data.resources.wood,Is.Zero);Assert.That(data.resources.craftedFurniture,Is.EqualTo(3));
            Assert.That(data.housing.stock[0].id,Is.EqualTo("chair"));Assert.That(data.housing.stock[0].count,Is.EqualTo(3));
        }
    }
}

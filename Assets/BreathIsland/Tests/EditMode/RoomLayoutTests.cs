using NUnit.Framework;
using UnityEngine;

namespace BreathIsland.Tests
{
    public sealed class RoomLayoutTests
    {
        [Test] public void DefaultIsTwentySquareAndOldEntranceRemainsReserved()
        {
            var layout=RoomLayoutDefinition.Default;
            Assert.That(layout.width,Is.EqualTo(20));Assert.That(layout.depth,Is.EqualTo(20));
            Assert.That(layout.arrival,Is.EqualTo(new Vector2(4,1)));
            var catalog=IslandLifeCatalog.Load();var chair=catalog.Furniture("chair");
            Assert.That(FurniturePlacement.Valid(catalog,chair,19,19,0,new PlacedFurniture[0]),Is.True);
            Assert.That(FurniturePlacement.Valid(catalog,chair,20,19,0,new PlacedFurniture[0]),Is.False);
            Assert.That(FurniturePlacement.Valid(catalog,chair,3,0,0,new PlacedFurniture[0]),Is.False);
        }
        [Test] public void RectangularDimensionsAreIndependentAndDefinitionIsNotMutated()
        {
            var layout=Object.Instantiate(RoomLayoutDefinition.Default);
            try
            {
                layout.width=24;layout.depth=12;
                Assert.That(layout.Contains(new Rect(22,10,2,2)),Is.True);
                Assert.That(layout.Contains(new Rect(10,12,1,1)),Is.False);
                Assert.That(layout.Contains(new Rect(-1,0,1,1)),Is.False);
                Assert.That(RoomLayoutDefinition.Default.width,Is.EqualTo(20));
            }
            finally{Object.DestroyImmediate(layout);}
        }
        [Test] public void ExpandedAreaAndOriginalPositionsRemainNavigable()
        {
            var catalog=IslandLifeCatalog.Load();var placed=new[]{new PlacedFurniture{furnitureId="bed",x=0,z=3,turns=0}};
            foreach(var position in new[]{new Vector3(4,.05f,1),new Vector3(18,.05f,18)})
            {
                var world=HousingSystem.InteriorOrigin+position;
                Assert.That(HomeNavigation.Clear(world,placed,catalog),Is.True);
                Assert.That(HomeNavigation.Safe(world,placed,catalog),Is.EqualTo(world));
            }
            Assert.That(HomeNavigation.Clear(HousingSystem.InteriorOrigin+new Vector3(20,.05f,19),placed,catalog),Is.False);
        }
    }
}

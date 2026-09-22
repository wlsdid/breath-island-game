using System.Collections.Generic;
using UnityEngine;
namespace BreathIsland
{
    public static class FurniturePlacement
    {
        public const int RoomSize = 8;
        public static Vector2Int Size(FurnitureDefinition definition, int turns) => (turns & 1) == 0
            ? definition.footprint : new Vector2Int(definition.footprint.y, definition.footprint.x);
        public static Rect Area(FurnitureDefinition definition, int x, int z, int turns)
        { var size = Size(definition, turns); return new Rect(x, z, size.x, size.y); }
        public static bool Valid(IslandLifeCatalog catalog, FurnitureDefinition definition, int x, int z, int turns, IEnumerable<PlacedFurniture> placed)
        {
            if (definition == null || turns < 0 || turns > 3) return false;
            Rect area = Area(definition, x, z, turns);
            if (area.xMin < 0 || area.yMin < 0 || area.xMax > RoomSize || area.yMax > RoomSize) return false;
            // Keep the arrival area and door clear, including the player's capsule.
            if (area.Overlaps(new Rect(3, 0, 2, 2))) return false;
            foreach (var item in placed)
            {
                var other = catalog.Furniture(item.furnitureId);
                if (other != null && area.Overlaps(Area(other, item.x, item.z, item.turns))) return false;
            }
            return true;
        }
    }
}

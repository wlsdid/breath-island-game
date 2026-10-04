using System.Collections.Generic;
using UnityEngine;
namespace BreathIsland
{
    public static class FurniturePlacement
    {
        public static int RoomSize => RoomLayoutDefinition.Default.width;
        public static Vector2Int Size(FurnitureDefinition definition, int turns) => (turns & 1) == 0
            ? definition.footprint : new Vector2Int(definition.footprint.y, definition.footprint.x);
        public static Rect Area(FurnitureDefinition definition, int x, int z, int turns)
        { var size = Size(definition, turns); return new Rect(x, z, size.x, size.y); }
        public static bool Valid(IslandLifeCatalog catalog, FurnitureDefinition definition, int x, int z, int turns, IEnumerable<PlacedFurniture> placed, HomeStructureState structure=null)
        {
            if (definition == null || turns < 0 || turns > 3) return false;
            Rect area = Area(definition, x, z, turns);
            var layout = RoomLayoutDefinition.Default;
            if (!HomeFootprint.Contains(structure,area)) return false;
            // Keep the arrival area and door clear, including the player's capsule.
            if (area.Overlaps(layout.entranceClearance)) return false;
            foreach (var item in placed)
            {
                var other = catalog.Furniture(item.furnitureId);
                if (other != null && area.Overlaps(Area(other, item.x, item.z, item.turns))) return false;
            }
            return true;
        }
    }
}

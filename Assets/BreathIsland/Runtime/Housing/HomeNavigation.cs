using System.Collections.Generic;
using UnityEngine;
namespace BreathIsland
{
    public static class HomeNavigation
    {
        public static Vector3 Arrival => HousingSystem.InteriorOrigin + new Vector3(RoomLayoutDefinition.Default.arrival.x, .05f, RoomLayoutDefinition.Default.arrival.y);
        public static bool OverlapsPlayer(FurnitureDefinition definition, int x, int z, int turns, Vector3 position)
        {
            var p = position - HousingSystem.InteriorOrigin;
            var area = FurniturePlacement.Area(definition, x, z, turns);
            return p.y > -1 && p.y < 3 && new Rect(area.x - .38f, area.y - .38f, area.width + .76f, area.height + .76f).Contains(new Vector2(p.x,p.z));
        }
        public static bool Clear(Vector3 position, IEnumerable<PlacedFurniture> placed, IslandLifeCatalog catalog)
        {
            var p = position - HousingSystem.InteriorOrigin;
            if (!RoomLayoutDefinition.Default.Contains(new Rect(p.x-.4f,p.z-.4f,.8f,.8f)) || p.y < -.5f || p.y > 2) return false;
            foreach (var item in placed)
            {
                var definition = catalog.Furniture(item.furnitureId);
                if (definition != null && OverlapsPlayer(definition,item.x,item.z,item.turns,position)) return false;
            }
            return true;
        }
        public static Vector3 Safe(Vector3 desired, IEnumerable<PlacedFurniture> placed, IslandLifeCatalog catalog)
        {
            if (Clear(desired,placed,catalog)) return desired;
            for (float radius=.6f; radius<=2.4f; radius+=.6f)
                for (int n=0;n<8;n++)
                {
                    var p=desired+Quaternion.Euler(0,n*45,0)*Vector3.forward*radius;
                    p.y=HousingSystem.InteriorOrigin.y+.05f;
                    if(Clear(p,placed,catalog)) return p;
                }
            return Arrival;
        }
    }
}

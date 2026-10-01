using System.Linq;
namespace BreathIsland
{
    // A one-time provision, independent of crafting costs and achievement progress.
    public static class StarterBedSystem
    {
        public static bool Ensure(HousingState housing)
        {
            if(housing.starterBedGranted)return false;
            bool owns=housing.stock.Any(s=>s.id=="bed"&&s.count>0)
                || housing.placed.Any(p=>p.furnitureId=="bed")
                || housing.chests.Any(c=>c.slots.Any(s=>s.furniture&&s.id=="bed"&&s.count>0));
            if(!owns)
            {
                var stock=housing.stock.FirstOrDefault(s=>s.id=="bed");
                if(stock==null)housing.stock.Add(new FurnitureStock{id="bed",count=1});else stock.count=1;
            }
            housing.starterBedGranted=true;
            return !owns;
        }
    }
}

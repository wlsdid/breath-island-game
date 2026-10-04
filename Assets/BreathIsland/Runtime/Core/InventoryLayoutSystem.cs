using System.Collections.Generic;
using System.Linq;
namespace BreathIsland
{
    // Only item identities and empty positions are persisted here; quantities stay in their original systems.
    public sealed class InventoryLayoutSystem
    {
        public const int PageSize=24;
        private readonly GameContext context;
        private readonly GamePhaseController phases;
        private readonly InventorySystem inventory;
        public InventoryLayoutSystem(GameContext context,GamePhaseController phases,InventorySystem inventory)
        {this.context=context;this.phases=phases;this.inventory=inventory;}
        public List<InventoryItem> Slots()
        {
            var owned=inventory.Items(InventoryTab.All).Where(i=>i.Available && i.Count>0).ToDictionary(i=>i.Key);
            var order=Arrange(context.Save.inventory.slotOrder,owned.Keys);
            return order.Select(key=>owned.TryGetValue(key,out var item)?item:null).ToList();
        }
        public bool Move(int from,int to)
        {
            if(phases.Current!=GamePhase.Equipment)return false;
            var slots=Slots();
            if(from<0 || to<0 || from>=slots.Count || to>=slots.Count || from==to || slots[from]==null)return false;
            (slots[from],slots[to])=(slots[to],slots[from]);
            context.Save.inventory.slotOrder=slots.Select(i=>i?.Key??"").ToList();
            context.SaveTo();return true;
        }
        public bool Sort()
        {
            if(phases.Current!=GamePhase.Equipment)return false;
            var keys=inventory.Items(InventoryTab.All).Where(i=>i.Available && i.Count>0).OrderBy(i=>i.Tab).ThenBy(i=>i.Id,System.StringComparer.Ordinal).Select(i=>i.Key);
            context.Save.inventory.slotOrder=Arrange(new List<string>(),keys);
            context.SaveTo();return true;
        }
        public static List<string> Arrange(IReadOnlyList<string> saved,IEnumerable<string> itemKeys)
        {
            var available=new HashSet<string>(itemKeys);var seen=new HashSet<string>();var result=new List<string>();
            if(saved!=null)foreach(var key in saved)
                result.Add(!string.IsNullOrEmpty(key) && available.Contains(key) && seen.Add(key)?key:"");
            foreach(var key in itemKeys)
            {
                if(!seen.Add(key))continue;
                int empty=result.IndexOf("");if(empty<0)result.Add(key);else result[empty]=key;
            }
            // Additional pages appear as needed; this is presentation space, not a carrying limit.
            int size=System.Math.Max(PageSize,((result.Count+PageSize-1)/PageSize)*PageSize);
            while(result.Count<size)result.Add("");return result;
        }
    }
}

using System.Collections.Generic;
namespace BreathIsland
{
    public enum InventoryTab { Tools, Weapons, Resources, Furniture, All }
    public sealed class InventoryItem
    {
        public string Id, NameKey, DetailKey;
        public int Count;
        public bool Available=true, Selected;
        public InventoryTab Tab;
        public string Key => Tab + ":" + Id;
    }
    // Inventory is a projection of existing save state, not a second item database.
    public sealed class InventorySystem
    {
        private readonly GameContext context;
        private readonly GamePhaseController phases;
        private readonly IslandLifeSession life;
        public bool Indoors => context.Save.housing.inside;
        public InventorySystem(GameContext context,GamePhaseController phases,IslandLifeSession life)
        {this.context=context;this.phases=phases;this.life=life;}
        public List<InventoryItem> Items(InventoryTab tab)
        {
            var items=new List<InventoryItem>();
            if(tab==InventoryTab.All)
            {
                for(int i=0;i<4;i++)items.AddRange(Items((InventoryTab)i));
                return items;
            }
            if(tab==InventoryTab.Tools)
                foreach(LifeTool tool in System.Enum.GetValues(typeof(LifeTool)))
                    if(tool!=LifeTool.None) items.Add(new InventoryItem{Id=tool.ToString(),NameKey="life_tool_"+tool,DetailKey="bag_tool_detail",Count=1,Selected=life.Equipment.Tool==tool});
            if(tab==InventoryTab.Weapons)
                foreach(var w in life.Equipment.Weapons) items.Add(new InventoryItem{Id=w.id,NameKey=w.nameKey,Count=1,Selected=life.Equipment.Weapon==w&&life.Equipment.Tool==LifeTool.None,Available=life.Equipment.Unlocked(w),DetailKey=life.Equipment.Unlocked(w)?(w.skill==null?"bag_regular_detail":"bag_special_detail"):"life_achievement_"+w.achievementId});
            if(tab==InventoryTab.Resources)
            {
                var r=context.Save.resources;var i=context.Save.inventory;
                string[] ids={"wood","stone","herbs","coins","materials","bait"};int[] counts={r.wood,r.stone,r.herbs,i.coins,i.materials,i.bait};
                for(int n=0;n<ids.Length;n++)items.Add(new InventoryItem{Id=ids[n],NameKey="bag_"+ids[n],DetailKey="bag_resource_detail",Count=counts[n]});
            }
            if(tab==InventoryTab.Furniture)
                foreach(var f in life.Catalog.furniture)items.Add(new InventoryItem{Id=f.id,NameKey=f.nameKey,Count=life.Housing.Available(f.id),Available=life.Housing.Available(f.id)>0,DetailKey="bag_furniture_detail"});
            foreach(var item in items)item.Tab=tab;
            return items;
        }
        public bool Hold(InventoryTab tab,string id)
        {
            if(phases.Current!=GamePhase.Equipment)return false;
            bool success=tab==InventoryTab.Tools ? System.Enum.TryParse<LifeTool>(id,out var tool)&&life.Equipment.SelectTool(tool) : tab==InventoryTab.Weapons&&life.Equipment.Equip(id);
            if(success)phases.CloseInventory();return success;
        }
        public bool PutAway()
        {
            if(phases.Current!=GamePhase.Equipment)return false;
            return life.Equipment.Tool!=LifeTool.None?life.Equipment.SelectTool(LifeTool.None):life.Equipment.Equip("");
        }
        public bool Place(string id)
        {
            if(phases.Current!=GamePhase.Equipment||!Indoors||life.Housing.Available(id)<1)return false;
            phases.CloseInventory();return life.Housing.Select(id);
        }
        public bool Craft(string id)
        {
            if(phases.Current!=GamePhase.Equipment||!Indoors)return false;
            return life.Crafting.Craft(id);
        }
    }
}

using UnityEngine;
namespace BreathIsland
{
    public sealed class EquipmentHud : HudScreen
    {
        private readonly IslandLifeSession life;
        private readonly InventorySystem inventory;
        private InventoryTab tab=InventoryTab.All;
        private readonly InventoryLayoutSystem layout;
        private readonly InventoryDragState drag=new();
        private int selected;
        private bool recipes;
        private int recipePage;
        private int itemPage;
        public EquipmentHud(GameContext context,GamePhaseController phases,IslandLifeSession life):base(context,phases)
        {this.life=life;inventory=new InventorySystem(context,phases,life);layout=new InventoryLayoutSystem(context,phases,inventory);phases.Changed+=CancelDrag;}
        private void CancelDrag(){drag.Cancel();recipes=false;}
        public override void Dispose(){phases.Changed-=CancelDrag;drag.Cancel();base.Dispose();}
        private void MoveSlot(int from,int to,string key)
        {
            var slots=layout.Slots();
            if(from>=0 && from<slots.Count && slots[from]?.Key==key && layout.Move(from,to)){selected=to;recipes=false;}
        }
        public override void Draw(float width,float height)
        {
            if (life.Finishes.Previewing) { InteriorFinishPanel.Draw(ui, context, life.Finishes); return; }
            float x=(width-1260)/2,y=(height-740)/2;
            ui.Panel(new Rect(x-12,y+12,1284,738),"",NotebookStyle.Cloth);
            ui.Panel(new Rect(x,y,1260,730));
            ui.Icon(new Rect(x+34,y+20,65,65),"bag");
            ui.Label(new Rect(x+107,y+12,670,70),context.Text("bag_title"),titleStyle);
            ui.Label(new Rect(x+110,y+76,660,38),context.Text(tab==InventoryTab.All?"bag_drag_help":"bag_subtitle"),smallStyle);
            ui.Button(new Rect(x+1090,y+30,130,44),context.Text("bag_close"),()=>phases.CloseInventory());
            var tabs=new[]{InventoryTab.All,InventoryTab.Tools,InventoryTab.Weapons,InventoryTab.Resources,InventoryTab.Furniture};
            for(int i=0;i<tabs.Length;i++)
            {
                var choice=tabs[i];
                ui.Button(new Rect(x+40+i*145,y+132,137,52),context.Text("bag_tab_"+choice),()=>{drag.Cancel();tab=choice;selected=0;itemPage=0;recipes=false;},tab==choice?NotebookStyle.Selected:buttonStyle,true,true);
            }
            var items=tab==InventoryTab.All?layout.Slots():inventory.Items(tab);selected=Mathf.Clamp(selected,0,Mathf.Max(0,items.Count-1));
            int pageSize=tab==InventoryTab.All?InventoryLayoutSystem.PageSize:8;
            int itemPages=Mathf.Max(1,Mathf.CeilToInt(items.Count/(float)pageSize));itemPage=Mathf.Clamp(itemPage,0,itemPages-1);
            if(tab==InventoryTab.All)
            {
                for(int i=itemPage*pageSize;i<Mathf.Min(items.Count,(itemPage+1)*pageSize);i++)
                {
                    int index=i;var item=items[i];int cell=i%pageSize;
                    float cx=x+40+(cell%6)*120,cy=y+208+(cell/6)*92;
                    ui.InventorySlot(new Rect(cx,cy,110,84),drag,index,item,()=>{selected=index;recipes=false;},MoveSlot,selected==i);
                    if(item!=null)
                    {
                        ui.Icon(new Rect(cx+19,cy+2,72,72),item.Id,selected==i?NotebookSurface.Paper:NotebookSurface.Ink);
                        if(item.Count>1)ui.Quantity(new Rect(cx+48,cy+56,57,25),item.Count);
                        if(item.Selected)ui.Icon(new Rect(cx+5,cy+5,14,14),"reticle",NotebookSurface.Coral);
                    }
                }
            }
            else for(int i=itemPage*8;i<Mathf.Min(items.Count,(itemPage+1)*8);i++)
            {
                int index=i;var item=items[i];float cx=x+40+(i%4)*181,cy=y+211+((i%8)/4)*181;
                ui.Button(new Rect(cx,cy,165,163),"",()=>{selected=index;recipes=false;},selected==i?NotebookStyle.Selected:buttonStyle);
                ui.Icon(new Rect(cx+40,cy+14,83,83),item.Available?item.Id:"lock",selected==i?NotebookSurface.Paper:NotebookSurface.Ink);
                ui.Label(new Rect(cx+5,cy+101,155,35),context.Text(item.NameKey),selected==i?NotebookStyle.Light:NotebookStyle.Center);
                if(tab==InventoryTab.Resources||tab==InventoryTab.Furniture)ui.Label(new Rect(cx+5,cy+133,155,26),item.Count.ToString(),selected==i?NotebookStyle.Light:NotebookStyle.Center);
                if(item.Selected)ui.Label(new Rect(cx+10,cy-17,145,31),context.Text("bag_held"),smallStyle);
            }
            if(items.Count>0 && items[selected]!=null)DrawDetail(new Rect(x+810,y+132,408,531),items[selected]);
            else ui.Panel(new Rect(x+810,y+132,408,531),context.Text("bag_empty_slot"));
            ui.Icon(new Rect(x+40,y+650,30,30),"sun");
            ui.Label(new Rect(x+75,y+642,655,40),string.Format(context.Text("bag_day"),context.Save.world.day,context.Save.player.hour),smallStyle);
            if(tab==InventoryTab.All)
            {
                ui.Button(new Rect(x+40,y+590,160,48),context.Text("bag_previous"),()=>{drag.Cancel();itemPage--;selected=itemPage*pageSize;},buttonStyle,itemPage>0);
                ui.Button(new Rect(x+210,y+590,380,48),context.Text("bag_sort"),()=>{drag.Cancel();layout.Sort();selected=0;itemPage=0;});
                ui.Button(new Rect(x+600,y+590,160,48),context.Text("bag_next"),()=>{drag.Cancel();itemPage++;selected=itemPage*pageSize;},buttonStyle,itemPage+1<itemPages);
            }
            else if (tab == InventoryTab.Furniture)
            {
                ui.Button(new Rect(x+40,y+590,160,48),context.Text("bag_previous"),()=>{itemPage--;selected=itemPage*8;},buttonStyle,itemPage>0);
                ui.Button(new Rect(x+210,y+590,380,48),context.Text("finish_title"),()=>life.Finishes.Begin(),buttonStyle,inventory.Indoors);
                ui.Button(new Rect(x+600,y+590,160,48),context.Text("bag_next"),()=>{itemPage++;selected=itemPage*8;},buttonStyle,itemPage+1<itemPages);
            }
            else ui.Label(new Rect(x+40,y+587,720,55),context.Text("bag_hint"),smallStyle);
        }
        private void DrawDetail(Rect rect,InventoryItem item)
        {
            ui.Panel(rect,"",NotebookStyle.Cloth);
            if(recipes){DrawRecipes(rect);return;}
            ui.Icon(new Rect(rect.x+130,rect.y+30,145,145),item.Available?item.Id:"lock",NotebookSurface.Paper);
            ui.Label(new Rect(rect.x+20,rect.y+193,368,60),context.Text(item.NameKey),NotebookStyle.Light);
            string detail=context.Text(item.DetailKey);
            if(item.Tab==InventoryTab.Weapons && item.Available)
            {
                var weapon=life.Catalog.Weapon(item.Id);
                detail=string.Format(context.Text("life_mastery"),context.Text(weapon.job.nameKey),life.Mastery.Hits(weapon.job.id),life.Mastery.Rank(weapon.job.id))+"\n"+context.Text(weapon.skill==null?weapon.basicAttack.nameKey:weapon.skill.nameKey);
            }
            ui.Label(new Rect(rect.x+28,rect.y+263,352,95),detail,NotebookStyle.Light);
            if(item.Tab==InventoryTab.Tools||item.Tab==InventoryTab.Weapons)
            {
                ui.Button(new Rect(rect.x+30,rect.y+370,348,52),context.Text("bag_hold"),()=>inventory.Hold(item.Tab,item.Id),primaryButtonStyle,item.Available);
                if(item.Id=="Hammer")ui.Button(new Rect(rect.x+30,rect.y+434,348,48),context.Text("bag_craft"),()=>{life.Equipment.SelectTool(LifeTool.Hammer);recipes=true;},buttonStyle,inventory.Indoors);
                else ui.Button(new Rect(rect.x+30,rect.y+434,348,48),context.Text("bag_put_away"),()=>inventory.PutAway());
            }
            if(item.Tab==InventoryTab.Furniture)
                ui.Button(new Rect(rect.x+30,rect.y+404,348,56),context.Text(inventory.Indoors?"bag_place":"bag_indoor_only"),()=>inventory.Place(item.Id),primaryButtonStyle,inventory.Indoors&&item.Available);
        }
        private void DrawRecipes(Rect rect)
        {
            ui.Label(new Rect(rect.x+15,rect.y+18,378,50),context.Text("bag_craft"),NotebookStyle.Light);
            int pages = Mathf.Max(1, Mathf.CeilToInt(life.Catalog.furniture.Length / 4f));
            recipePage = Mathf.Clamp(recipePage, 0, pages - 1);
            for(int i=recipePage*4;i<Mathf.Min(life.Catalog.furniture.Length,(recipePage+1)*4);i++)
            {
                var f=life.Catalog.furniture[i];float y=rect.y+74+(i%4)*82;
                ui.Button(new Rect(rect.x+20,y,368,72),context.Text(f.nameKey)+"\n"+string.Format(context.Text("bag_cost"),f.woodCost,f.stoneCost),()=>inventory.Craft(f.id));
            }
            ui.Button(new Rect(rect.x+20,rect.y+408,176,36),context.Text("bag_previous"),()=>recipePage--,buttonStyle,recipePage>0);
            ui.Button(new Rect(rect.x+212,rect.y+408,176,36),context.Text("bag_next"),()=>recipePage++,buttonStyle,recipePage+1<pages);
            ui.Button(new Rect(rect.x+20,rect.y+463,368,42),context.Text("bag_back"),()=>recipes=false);
        }
    }
}

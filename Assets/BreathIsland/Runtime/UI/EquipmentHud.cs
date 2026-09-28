using UnityEngine;
namespace BreathIsland
{
    public sealed class EquipmentHud : HudScreen
    {
        private readonly IslandLifeSession life;
        private readonly InventorySystem inventory;
        private InventoryTab tab;
        private int selected;
        private bool recipes;
        private int recipePage;
        public EquipmentHud(GameContext context,GamePhaseController phases,IslandLifeSession life):base(context,phases)
        {this.life=life;inventory=new InventorySystem(context,phases,life);}
        public override void Draw(float width,float height)
        {
            if (life.Finishes.Previewing) { InteriorFinishPanel.Draw(ui, context, life.Finishes); return; }
            float x=(width-1260)/2,y=(height-740)/2;
            ui.Panel(new Rect(x-12,y+12,1284,738),"",NotebookStyle.Cloth);
            ui.Panel(new Rect(x,y,1260,730));
            ui.Icon(new Rect(x+34,y+20,65,65),"bag");
            ui.Label(new Rect(x+107,y+12,670,70),context.Text("bag_title"),titleStyle);
            ui.Label(new Rect(x+110,y+76,660,38),context.Text("bag_subtitle"),smallStyle);
            ui.Button(new Rect(x+1090,y+30,130,44),context.Text("bag_close"),()=>phases.CloseInventory());
            for(int i=0;i<4;i++)
            {
                var choice=(InventoryTab)i;
                ui.Button(new Rect(x+40+i*181,y+132,169,52),context.Text("bag_tab_"+choice),()=>{tab=choice;selected=0;recipes=false;},tab==choice?NotebookStyle.Selected:buttonStyle);
            }
            var items=inventory.Items(tab);selected=Mathf.Clamp(selected,0,Mathf.Max(0,items.Count-1));
            for(int i=0;i<items.Count;i++)
            {
                int index=i;var item=items[i];float cx=x+40+(i%4)*181,cy=y+211+(i/4)*181;
                ui.Button(new Rect(cx,cy,165,163),"",()=>{selected=index;recipes=false;},selected==i?NotebookStyle.Selected:buttonStyle);
                ui.Icon(new Rect(cx+40,cy+14,83,83),item.Available?item.Id:"lock",selected==i?NotebookSurface.Paper:NotebookSurface.Ink);
                ui.Label(new Rect(cx+5,cy+101,155,35),context.Text(item.NameKey),selected==i?NotebookStyle.Light:NotebookStyle.Center);
                if(tab==InventoryTab.Resources||tab==InventoryTab.Furniture)ui.Label(new Rect(cx+5,cy+133,155,26),item.Count.ToString(),selected==i?NotebookStyle.Light:NotebookStyle.Center);
                if(item.Selected)ui.Label(new Rect(cx+10,cy-17,145,31),context.Text("bag_held"),smallStyle);
            }
            if(items.Count>0)DrawDetail(new Rect(x+810,y+132,408,531),items[selected]);
            ui.Icon(new Rect(x+40,y+650,30,30),"sun");
            ui.Label(new Rect(x+75,y+642,655,40),string.Format(context.Text("bag_day"),context.Save.world.day,context.Save.player.hour),smallStyle);
            if (tab == InventoryTab.Furniture)
                ui.Button(new Rect(x+40,y+590,720,48),context.Text("finish_title"),()=>life.Finishes.Begin(),buttonStyle,inventory.Indoors);
            else ui.Label(new Rect(x+40,y+587,720,55),context.Text("bag_hint"),smallStyle);
        }
        private void DrawDetail(Rect rect,InventoryItem item)
        {
            ui.Panel(rect,"",NotebookStyle.Cloth);
            if(recipes){DrawRecipes(rect);return;}
            ui.Icon(new Rect(rect.x+130,rect.y+30,145,145),item.Available?item.Id:"lock",NotebookSurface.Paper);
            ui.Label(new Rect(rect.x+20,rect.y+193,368,60),context.Text(item.NameKey),NotebookStyle.Light);
            string detail=context.Text(item.DetailKey);
            if(tab==InventoryTab.Weapons && item.Available)
            {
                var weapon=life.Catalog.Weapon(item.Id);
                detail=string.Format(context.Text("life_mastery"),context.Text(weapon.job.nameKey),life.Mastery.Hits(weapon.job.id),life.Mastery.Rank(weapon.job.id))+"\n"+context.Text(weapon.skill==null?weapon.basicAttack.nameKey:weapon.skill.nameKey);
            }
            ui.Label(new Rect(rect.x+28,rect.y+263,352,95),detail,NotebookStyle.Light);
            if(tab==InventoryTab.Tools||tab==InventoryTab.Weapons)
            {
                ui.Button(new Rect(rect.x+30,rect.y+370,348,52),context.Text("bag_hold"),()=>inventory.Hold(tab,item.Id),primaryButtonStyle,item.Available);
                if(item.Id=="Hammer")ui.Button(new Rect(rect.x+30,rect.y+434,348,48),context.Text("bag_craft"),()=>{life.Equipment.SelectTool(LifeTool.Hammer);recipes=true;},buttonStyle,inventory.Indoors);
                else ui.Button(new Rect(rect.x+30,rect.y+434,348,48),context.Text("bag_put_away"),()=>inventory.PutAway());
            }
            if(tab==InventoryTab.Furniture)
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

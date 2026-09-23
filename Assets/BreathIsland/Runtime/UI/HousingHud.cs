using UnityEngine;
namespace BreathIsland
{
    public sealed class HousingHud : HudScreen
    {
        private readonly IslandLifeSession life;
        private int page;
        public HousingHud(GameContext context,GamePhaseController phases,IslandLifeSession life):base(context,phases){this.life=life;}
        public override void Draw(float width,float height)
        {
            var housing=life.Housing;
            ui.Panel(new Rect(30,140,310,620));
            ui.Label(new Rect(50,155,270,74),context.Text("life_home"),titleStyle);
            ui.Button(new Rect(55,256,260,56),context.Text("bag_open"),()=>phases.OpenInventory(),primaryButtonStyle);
            ui.Label(new Rect(50,330,270,135),context.Text("bag_room_help"),bodyStyle);
            ui.Button(new Rect(55,578,260,56),context.Text("life_rest"),()=>life.Land.Rest());
            ui.Button(new Rect(55,654,260,56),context.Text("life_exit"),()=>housing.Exit());
            float x=width-405;
            ui.Panel(new Rect(x,140,375,620));
            if(housing.Selection!=null)
            {
                ui.Label(new Rect(x+15,160,345,70),context.Text(housing.Selection.nameKey),titleStyle);
                for(int z=0;z<8;z++)for(int column=0;column<8;column++)
                {
                    int cx=column,cz=z;
                    bool marked=FurniturePlacement.Area(housing.Selection,housing.X,housing.Z,housing.Turns).Contains(new Vector2(column+.5f,z+.5f));
                    ui.Button(new Rect(x+27+column*40,255+(7-z)*40,36,36),"",()=>housing.Move(cx,cz),marked?(housing.IsValid?NotebookStyle.Selected:NotebookStyle.Primary):buttonStyle);
                }
                ui.Button(new Rect(x+27,590,151,46),context.Text("life_rotate"),()=>housing.Rotate());
                ui.Button(new Rect(x+189,590,151,46),context.Text("life_cancel"),()=>housing.Cancel());
                ui.Button(new Rect(x+27,650,313,54),context.Text("life_place"),()=>housing.Place(),primaryButtonStyle,housing.IsValid);
            }
            else
            {
                ui.Label(new Rect(x+20,160,335,70),context.Text("bag_placed"),titleStyle);
                ui.Label(new Rect(x+20,234,335,64),context.Text("life_recall_help"),smallStyle);
                int pages=Mathf.Max(1,Mathf.CeilToInt(housing.State.placed.Count/5f));page=Mathf.Clamp(page,0,pages-1);
                for(int i=page*5;i<Mathf.Min(housing.State.placed.Count,page*5+5);i++)
                {
                    var item=housing.State.placed[i];var definition=life.Catalog.Furniture(item.furnitureId);
                    ui.Button(new Rect(x+24,321+(i%5)*63,327,52),context.Text(definition?.nameKey??"life_unknown"),()=>housing.Recall(item.instanceId));
                }
                ui.Button(new Rect(x+24,675,155,42),context.Text("bag_previous"),()=>page--,buttonStyle,page>0);
                ui.Button(new Rect(x+195,675,155,42),context.Text("bag_next"),()=>page++,buttonStyle,page+1<pages);
            }
        }
    }
}

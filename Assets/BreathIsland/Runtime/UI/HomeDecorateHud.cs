using UnityEngine;
namespace BreathIsland
{
    public sealed class HomeDecorateHud : HudScreen
    {
        private readonly IslandLifeSession life;
        private int page;
        private int gridX, gridZ;
        private readonly HomeConstructionPanel construction=new();
        public HomeDecorateHud(GameContext context,GamePhaseController phases,IslandLifeSession life):base(context,phases){this.life=life;}
        public override void Draw(float width,float height)
        {
            var housing=life.Housing;
            ui.Panel(new Rect(30,140,310,620));
            ui.Label(new Rect(50,155,270,74),context.Text("life_home"),titleStyle);
            ui.Button(new Rect(55,256,260,56),context.Text("bag_open"),()=>phases.OpenInventory(),primaryButtonStyle);
            ui.Label(new Rect(50,330,270,135),context.Text("bag_room_help"),bodyStyle);
            ui.Button(new Rect(55,476,260,52),context.Text("build_title"),()=>life.Construction.Begin());
            if(life.Construction.Editing)ui.Label(new Rect(50,536,270,105),context.Text("build_help"),smallStyle);

            ui.Button(new Rect(55,654,260,56),context.Text("bag_back"),()=>phases.Set(GamePhase.Home));
            if(life.Construction.Editing){construction.Draw(ui,context,life.Construction,width);return;}
            float x=width-405;
            ui.Panel(new Rect(x,140,375,620));
            if(housing.Selection!=null)
            {
                ui.Label(new Rect(x+15,160,345,70),context.Text(housing.Selection.nameKey),titleStyle);
                var size=HomeFootprint.Size(housing.State.structure);
                gridX=Mathf.Clamp(gridX,0,Mathf.Max(0,(int)size.x-8));
                gridZ=Mathf.Clamp(gridZ,0,Mathf.Max(0,(int)size.y-8));
                ui.Button(new Rect(x+27,216,72,32),"←",()=>gridX-=6,buttonStyle,gridX>0,true);
                ui.Button(new Rect(x+107,216,72,32),"→",()=>gridX+=6,buttonStyle,gridX+8<size.x,true);
                ui.Button(new Rect(x+188,216,72,32),"↑",()=>gridZ+=6,buttonStyle,gridZ+8<size.y,true);
                ui.Button(new Rect(x+269,216,72,32),"↓",()=>gridZ-=6,buttonStyle,gridZ>0,true);
                for(int z=0;z<8;z++)for(int column=0;column<8;column++)
                {
                    int cx=column+gridX,cz=z+gridZ;
                    bool marked=FurniturePlacement.Area(housing.Selection,housing.X,housing.Z,housing.Turns).Contains(new Vector2(cx+.5f,cz+.5f));
                    ui.Button(new Rect(x+27+column*40,255+(7-z)*40,36,36),"",()=>housing.Move(cx,cz),marked?(housing.IsValid?NotebookStyle.Selected:NotebookStyle.Primary):buttonStyle,HomeFootprint.Contains(housing.State.structure,new Rect(cx,cz,1,1)));
                }
                ui.Button(new Rect(x+27,590,151,46),context.Text("life_rotate"),()=>housing.Rotate());
                ui.Button(new Rect(x+189,590,151,46),context.Text("life_cancel"),()=>housing.Cancel());
                ui.Button(new Rect(x+27,650,313,54),context.Text("life_place"),()=>housing.Place(),primaryButtonStyle,housing.IsValid);
                ui.Label(new Rect(x+20,707,335,43),context.Text(housing.IsValid?"place_ready":"place_blocked"),smallStyle);
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

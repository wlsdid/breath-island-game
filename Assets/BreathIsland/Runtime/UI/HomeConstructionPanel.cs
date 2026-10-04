using System.Linq;
using UnityEngine;
namespace BreathIsland
{
    public sealed class HomeConstructionPanel
    {
        private int x,z,mode;
        public void Draw(NotebookSurface ui,GameContext context,HomeConstructionSystem system,float width)
        {
            float left=width-405;
            ui.Panel(new Rect(left,140,375,620));
            ui.Label(new Rect(left+20,151,335,42),context.Text("build_title"),NotebookStyle.Title);
            string[] modes={"build_floor","build_wall_n","build_wall_e"};
            for(int i=0;i<3;i++){int m=i;ui.Button(new Rect(left+20+i*113,202,108,42),context.Text(modes[i]),()=>mode=m,mode==i?NotebookStyle.Selected:NotebookStyle.Button,true,true);}
            ui.Button(new Rect(left+20,250,76,32),"←",()=>x=Mathf.Max(0,x-6),NotebookStyle.Button,x>0,true);
            ui.Button(new Rect(left+104,250,76,32),"→",()=>x=Mathf.Min(24,x+6),NotebookStyle.Button,x<24,true);
            ui.Button(new Rect(left+188,250,76,32),"↑",()=>z=Mathf.Min(24,z+6),NotebookStyle.Button,z<24,true);
            ui.Button(new Rect(left+272,250,76,32),"↓",()=>z=Mathf.Max(0,z-6),NotebookStyle.Button,z>0,true);
            for(int row=0;row<8;row++)for(int col=0;col<8;col++)
            {
                int cx=x+col,cz=z+row;
                bool occupied=mode==0?HomeFootprint.Cell(system.Draft,cx,cz):system.Draft.walls.Any(w=>w.x==cx+(mode==2?1:0)&&w.z==cz+(mode==1?1:0)&&w.vertical==(mode==2));
                ui.Button(new Rect(left+27+col*40,294+(7-row)*37,36,33),"",()=>{if(mode==0)system.ToggleFloor(cx,cz);else system.ToggleWall(cx+(mode==2?1:0),cz+(mode==1?1:0),mode==2);},occupied?NotebookStyle.Selected:NotebookStyle.Button);
            }
            ui.Label(new Rect(left+20,597,335,52),string.Format(context.Text("build_cost"),system.WoodCost,context.Save.resources.wood),NotebookStyle.Small);
            ui.Button(new Rect(left+24,662,155,50),context.Text("build_apply"),()=>system.Apply(),NotebookStyle.Primary,context.Save.resources.wood>=system.WoodCost);
            ui.Button(new Rect(left+195,662,155,50),context.Text("life_cancel"),()=>system.Cancel());
        }
    }
}

using UnityEngine;
namespace BreathIsland
{
    public sealed class HomeRestHud : HudScreen
    {
        private readonly HomeInteractionSystem home;
        public HomeRestHud(GameContext context,GamePhaseController phases,HomeInteractionSystem home):base(context,phases){this.home=home;}
        public override void Draw(float width,float height)
        {
            float x=width/2-300,y=height/2-170;
            ui.Panel(new Rect(x,y,600,340));
            ui.Label(new Rect(x+35,y+25,530,75),context.Text("home_sleep_title"),titleStyle);
            ui.Label(new Rect(x+35,y+115,530,75),context.Text("home_sleep_body"),bodyStyle);
            ui.Button(new Rect(x+35,y+235,250,60),context.Text("life_rest"),()=>home.ConfirmRest(),primaryButtonStyle);
            ui.Button(new Rect(x+310,y+235,250,60),context.Text("life_cancel"),()=>phases.Set(GamePhase.Home));
        }
    }
}

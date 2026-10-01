using UnityEngine;
namespace BreathIsland
{
    public sealed class HousingHud : HudScreen
    {
        private readonly IslandLifeSession life;
        public HousingHud(GameContext context,GamePhaseController phases,IslandLifeSession life):base(context,phases){this.life=life;}
        public override void Draw(float width,float height)
        {
            ui.Button(new Rect(width-290,35,250,56),context.Text("home_decorate"),()=>life.Home.Decorate());
            ui.Button(new Rect(width-290,104,250,56),context.Text("bag_open"),()=>phases.OpenInventory());
            string key="home_walk_hint";
            var item=life.Home.Nearby;
            if(life.Home.Seated)key="home_stand_hint";
            else if(life.Home.AtDoor)key="home_exit_hint";
            else if(item!=null)
            {
                var type=life.Catalog.Furniture(item.furnitureId).interaction;
                key=type==FurnitureInteraction.Seat?"home_sit_hint":type==FurnitureInteraction.Bed?"home_sleep_hint":type==FurnitureInteraction.Storage?"home_storage_hint":"home_walk_hint";
            }
            ui.Panel(new Rect(width/2-310,height-115,620,76),context.Text(key));
        }
    }
}

using UnityEngine;
namespace BreathIsland
{
    public sealed class ExploreHud : HudScreen
    {
        private readonly RelationshipSystem relationships;
        private readonly IslandLifeSession life;
        public ExploreHud(GameContext context,GamePhaseController phases,RelationshipSystem relationships,IslandLifeSession life=null):base(context,phases)
        {this.relationships=relationships;this.life=life;}
        public override void Draw(float width,float height)
        {
            string prompt="";
            if(life?.Housing.CanEnter==true)prompt=context.Text("life_enter");
            else if(life?.Land.Nearby!=null)
            {
                var node=life.Land.Nearby;
                prompt=life.Land.Depleted(node.id)?context.Text("life_depleted"):string.Format(context.Text(node.tool==LifeTool.None?(node.gatherSeconds>0?"bag_gather_hold":"bag_gather_hint"):"bag_tool_hint"),context.Text(node.nameKey),context.Text("life_tool_"+node.tool));
                if(!life.Land.Depleted(node.id) && life.Land.Progress(node)>0)
                    ui.Meter(new Rect(width/2-180,height-57,360,10),"",life.Land.Progress(node)*100,NotebookSurface.Sea);
            }
            else if(relationships.NearbyNpc().HasValue)prompt=context.Text("bag_talk_hint");
            else if(Vector3.Distance(context.World.player.transform.position,context.World.fishingPoint)<7.5f)prompt=context.Text("bag_fish_hint");
            if(!string.IsNullOrEmpty(prompt))ui.Panel(new Rect(width/2-310,height-134,620,62),prompt);
            ui.Icon(new Rect(width/2-6,height/2-6,12,12),"reticle",NotebookSurface.Paper);
            if(life!=null && Vector3.Distance(context.World.player.transform.position,life.Training.TargetPosition)<5)
            {
                ui.Panel(new Rect(width/2-210,130,420,62),string.Format(context.Text("life_training"),life.Training.Health,life.Training.SkillCooldown));
                ui.Label(new Rect(width/2-225,198,450,38),context.Text("bag_reset_hint"),smallStyle);
            }
        }
    }
}

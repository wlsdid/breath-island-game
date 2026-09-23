using UnityEngine;
namespace BreathIsland
{
    public sealed class HudOverlay : HudScreen
    {
        private readonly GameSession session;
        private readonly IslandLifeCatalog catalog=IslandLifeCatalog.Load();
        public HudOverlay(GameContext context,GamePhaseController phases,GameSession session):base(context,phases){this.session=session;}
        public override void Draw(float width,float height)
        {
            if(phases.Current==GamePhase.Equipment)return;
            ui.Panel(new Rect(32,28,280,83));
            ui.Icon(new Rect(46,41,54,54),"sun");
            ui.Label(new Rect(108,36,180,35),string.Format(context.Text("bag_day_short"),context.Save.world.day),smallStyle);
            ui.Label(new Rect(108,65,180,35),context.Save.player.hour.ToString("00")+":00",bodyStyle);
            if(phases.Current==GamePhase.Explore)
            {
                string held=context.Save.equipment.tool!=LifeTool.None?context.Text("life_tool_"+context.Save.equipment.tool):context.Text("life_unarmed");
                var weapon=catalog.Weapon(context.Save.equipment.weaponId);
                if(context.Save.equipment.tool==LifeTool.None&&weapon!=null)held=context.Text(weapon.nameKey);
                ui.Panel(new Rect(32,height-111,390,78),"",NotebookStyle.Cloth);
                ui.Icon(new Rect(48,height-101,57,57),context.Save.equipment.tool!=LifeTool.None?context.Save.equipment.tool.ToString():weapon?.id,NotebookSurface.Paper);
                ui.Label(new Rect(111,height-98,295,48),held,NotebookStyle.Light);
                ui.Panel(new Rect(width-220,height-101,188,62),context.Text("bag_open"));
            }
            if(Time.unscaledTime<session.MessageUntil)ui.Panel(new Rect(width/2-310,28,620,68),session.EventMessage);
        }
    }
}
